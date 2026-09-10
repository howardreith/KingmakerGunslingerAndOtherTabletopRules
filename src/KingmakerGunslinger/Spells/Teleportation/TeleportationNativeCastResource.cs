using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kingmaker;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class TeleportationNativeCastResource : ITeleportCastResource
    {
        private readonly Player _player;
        private readonly Spellbook _book;
        private readonly AbilityData _ability;
        private readonly int _level;
        private readonly bool _spontaneous;
        private readonly object[] _collections;
        private readonly SlotEvidence[][] _slots;
        private readonly SlotEvidence[] _flatSlots;
        private readonly int[] _spontaneousBefore;
        private readonly bool[] _preparedBefore;
        private readonly int[] _expectedPrepared;
        private int _spendAttempted;
        private bool _nativeSpendInvoked;
        private int _restoreAttempted;
        internal bool? NativeSpendReturned { get; private set; }
        
        internal TeleportationNativeCastResource(TeleportationNativeCastSource source)
        {
            if (source == null || Game.Instance == null) throw new ArgumentException("Current native spell source required.", "source");
            _player = Game.Instance.Player;
            _book = source.Book;
            _ability = source.Ability;
            _level = source.Snapshot.SpellLevel;
            _spontaneous = source.Snapshot.Kind == TeleportCastSourceKind.Spontaneous;
            if (!CurrentOwner() || !TeleportationSpellbookAdapter.CasterAvailable(_book.Owner.Unit) ||
                _book.Blueprint.Spontaneous != _spontaneous || _level < 1 || _level > 9 ||
                !TeleportationSpellbookAdapter.ExactAbility(_ability, _book, Bootstrap.BlueprintBootstrap.Teleportation.Get(source.Snapshot.Spell), _level) ||
                !_ability.IsAvailable || !_book.CanSpend(_ability))
                throw new InvalidOperationException("Selected spell source became unavailable before resource capture.");
            _collections = new object[10];
            _slots = new SlotEvidence[10][];
            for (int level = 0; level < 10; level++)
            {
                IEnumerable<SpellSlot> collection = _book.GetMemorizedSpellSlots(level);
                if (collection == null) throw new InvalidOperationException("A native prepared resource list is absent.");
                _collections[level] = collection;
                _slots[level] = collection.Select(value => new SlotEvidence(value)).ToArray();
            }
            _flatSlots = _slots.SelectMany(value => value).ToArray();
            if (_flatSlots.Select(value => value.Slot).Distinct().Count() != _flatSlots.Length)
                throw new InvalidOperationException("Physical spell slots are duplicated across native levels.");
            _preparedBefore = _flatSlots.Select(value => value.Slot.Available).ToArray();
            _spontaneousBefore = SpontaneousCounts();
            if (_spontaneousBefore.Any(value => value < 0)) throw new InvalidOperationException("A native resource count is negative.");
            if (_spontaneous)
            {
                if (_spontaneousBefore[_level] <= 0 || _spontaneousBefore[_level] > _book.GetSpellsPerDay(_level) ||
                    !TeleportationSpellbookAdapter.KnownAbilities(_book, _level).Any(value => ReferenceEquals(value, _ability)))
                    throw new InvalidOperationException("The selected known spell or spontaneous pool is no longer proven.");
                _expectedPrepared = new int[0];
            }
            else
            {
                SpellSlot[] physical = _slots[_level].Select(value => value.Slot).ToArray();
                TeleportPreparedPoolDecision pool = TeleportationSpellbookAdapter.Prepared(physical, _book, _ability.Blueprint, _level);
                if (pool == null || pool.Uses <= 0 || !ReferenceEquals(physical[pool.SelectedOrdinal].Spell, _ability))
                    throw new InvalidOperationException("The deterministic prepared use changed before capture.");
                int offset = _slots.Take(_level).Sum(value => value.Length);
                _expectedPrepared = pool.SpentOrdinals.Select(value => offset + value).ToArray();
            }
        }

        public void Spend()
        {
            if (Interlocked.CompareExchange(ref _spendAttempted, 1, 0) != 0) return;
            if (ObserveExpenditure() != TeleportExpenditure.None || !CurrentOwner() ||
                !TeleportationSpellbookAdapter.CasterAvailable(_book.Owner.Unit) || !_ability.IsAvailable || !_book.CanSpend(_ability))
                throw new InvalidOperationException("Captured native spell resource changed before expenditure.");
            _nativeSpendInvoked = true;
            NativeSpendReturned = _book.Spend(_ability, false);
            if (NativeSpendReturned != true) throw new InvalidOperationException("Native Spellbook.Spend rejected the captured cast.");
        }

        public TeleportExpenditure ObserveExpenditure()
        {
            return TeleportResourceDeltaPolicy.Observe(TopologyMatches(), _spontaneousBefore, SpontaneousCounts(),
                _preparedBefore, _flatSlots.Select(value => value.Slot.Available).ToArray(), _spontaneous ? _level : -1, _expectedPrepared, _nativeSpendInvoked);
        }

        public bool RestoreAndVerifyExactResource()
        {
            if (Interlocked.CompareExchange(ref _restoreAttempted, 1, 0) != 0 ||
                ObserveExpenditure() != TeleportExpenditure.ExactlyOne) return false;
            if (_spontaneous)
            {
                // Native restoration clamps to current capacity: prove that clamp
                // can still restore the captured exact count before calling it.
                if (_book.GetSpellsPerDay(_level) < _spontaneousBefore[_level]) return false;
                _book.RestoreSpontaneousSlots(_level, 1);
            }
            else
            {
                // Exact captured fields, matching native AbilityRestoreSpellSlot.Apply.
                // Linked opposition members are one use; restore every captured member.
                foreach (int index in _expectedPrepared) _flatSlots[index].Slot.Available = _preparedBefore[index];
            }
            return ObserveExpenditure() == TeleportExpenditure.None;
        }

        public object Evidence()
        {
            return new { casterId = _book.Owner.Unit.UniqueId, spellbookId = _book.Blueprint.AssetGuid,
                spellId = _ability.Blueprint.AssetGuid, level = _level, spontaneous = _spontaneous,
                nativeSpendInvoked = _nativeSpendInvoked, nativeSpendReturned = NativeSpendReturned, spontaneousBefore = _spontaneousBefore.ToArray(),
                spontaneousCurrent = SpontaneousCounts(), expectedPreparedOrdinals = _expectedPrepared.ToArray(),
                prepared = _flatSlots.Select((value, index) => new { ordinal = index, level = value.Slot.SpellLevel,
                    nativeIndex = value.Slot.Index, type = value.Slot.Type.ToString(),
                    spellId = value.Slot.Spell == null ? null : value.Slot.Spell.Blueprint.AssetGuid,
                    before = _preparedBefore[index], current = value.Slot.Available,
                    opposition = value.Slot.IsOpposition }).ToArray(), exactTopology = TopologyMatches() };
        }
        private int[] SpontaneousCounts()
        { return Enumerable.Range(0, 10).Select(level => _book.GetSpontaneousSlots(level)).ToArray(); }
        private bool CurrentOwner()
        {
            return Game.Instance != null && ReferenceEquals(Game.Instance.Player, _player) && _book != null &&
                _book.Owner != null && TeleportationSpellbookAdapter.OwnedBook(_book.Owner.Unit, _book) &&
                _player.Party.Count(value => ReferenceEquals(value, _book.Owner.Unit)) == 1;
        }
        private bool TopologyMatches()
        {
            if (!CurrentOwner() || _book.Blueprint.Spontaneous != _spontaneous || _book.GetSpellLevel(_ability) != _level) return false;
            if (_spontaneous && !TeleportationSpellbookAdapter.KnownAbilities(_book, _level).Any(value => ReferenceEquals(value, _ability))) return false;
            for (int level = 0; level < 10; level++)
            {
                IEnumerable<SpellSlot> collection = _book.GetMemorizedSpellSlots(level);
                if (!ReferenceEquals(collection, _collections[level])) return false;
                SpellSlot[] current = collection.ToArray();
                if (current.Length != _slots[level].Length) return false;
                for (int index = 0; index < current.Length; index++)
                    if (!_slots[level][index].Matches(current[index])) return false;
            }
            return true;
        }

        private sealed class SlotEvidence
        {
            internal readonly SpellSlot Slot;
            private readonly AbilityData _spell;
            private readonly SpellSlot[] _linksReference;
            private readonly SpellSlot[] _links;
            private readonly bool _opposition;
            internal SlotEvidence(SpellSlot slot)
            {
                Slot = slot ?? throw new InvalidOperationException("A physical native spell slot is null.");
                _spell = slot.Spell;
                _linksReference = slot.LinkedSlots;
                _links = slot.LinkedSlots == null ? null : slot.LinkedSlots.ToArray();
                _opposition = slot.IsOpposition;
            }
            internal bool Matches(SpellSlot current)
            {
                return ReferenceEquals(Slot, current) && ReferenceEquals(current.Spell, _spell) && current.IsOpposition == _opposition &&
                    ReferenceEquals(current.LinkedSlots, _linksReference) &&
                    (_links == null || (_links.Length == current.LinkedSlots.Length &&
                        _links.Select((value, index) => ReferenceEquals(value, current.LinkedSlots[index])).All(value => value)));
            }
        }
    }
}
