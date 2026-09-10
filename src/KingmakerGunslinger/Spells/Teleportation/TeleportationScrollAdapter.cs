using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules.Abilities;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Inventory-backed strategic sources. Genuine scroll items — including
    // compatible crafted variants — are discovered through verified item type
    // and canonical spell association, never display names. Native player
    // characters share one party inventory; stock is counted once per distinct
    // collection. Reader eligibility is evaluated per reader and per spell
    // through the native scroll pathway: the spell appearing in one of the
    // reader's class spell lists (no knowledge/preparation/slot requirement),
    // or a trained Use Magic Device skill for a legitimate uncertain attempt.
    internal static class TeleportationScrollAdapter
    {
        internal sealed class ScrollGroup
        {
            internal TeleportSpellKind Spell;
            internal int CasterLevel;
            internal int SpellLevel;
            internal BlueprintItemEquipmentUsable Representative;
            internal readonly List<ItemEntity> Items = new List<ItemEntity>();
        }

        internal static IReadOnlyList<TeleportCastSourceSnapshot> Enumerate(Player player)
        {
            var result = new List<TeleportCastSourceSnapshot>();
            if (player == null || BlueprintBootstrap.Teleportation == null ||
                !ModContext.TryGet(out var context) || !context.FeatureModules.Active.TeleportationSpells)
                return result;
            var party = player.Party.ToArray();
            var groups = CollectGroups(player);
            foreach (var reader in party)
            {
                int order = Array.IndexOf(party, reader);
                foreach (var group in groups)
                {
                    // Per-reader, per-spell native eligibility.
                    if (!IsReader(reader, group)) continue;
                    result.Add(new TeleportCastSourceSnapshot(reader.UniqueId, order,
                        reader.CharacterName,
                        group.Representative.AssetGuid, "Scroll", group.Spell, TeleportCastSourceKind.Scroll,
                        group.SpellLevel, group.Items.Sum(value => value.Count),
                        TeleportCastSourceFacts.ActiveParty | TeleportCastSourceFacts.LivingAvailableCaster |
                        TeleportCastSourceFacts.ScrollStock | TeleportCastSourceFacts.ExactSpell |
                        TeleportCastSourceFacts.RealResource));
                }
            }
            return result;
        }

        // The verified association a genuine scroll carries: its CopyScroll
        // teaching target, or the ability it activates, pointing at one of the
        // canonical strategic spells. Any other charged item type is ignored.
        internal static TeleportSpellKind? AssociatedSpell(BlueprintItem item)
        {
            var usable = item as BlueprintItemEquipmentUsable;
            if (usable == null || usable.Type != UsableItemType.Scroll || usable.Ability == null) return null;
            var spells = BlueprintBootstrap.Teleportation;
            if (spells == null) return null;
            var copy = usable.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>()
                .Select(value => value.CustomSpell).FirstOrDefault(value => value != null);
            BlueprintAbilityClass(usable, copy, spells, out var kind);
            return kind;
        }

        private static void BlueprintAbilityClass(BlueprintItemEquipmentUsable usable, Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility copy,
            TeleportationSpellBlueprintSet spells, out TeleportSpellKind? kind)
        {
            kind = KindFor(copy, spells) ?? KindFor(usable.Ability, spells);
        }

        private static TeleportSpellKind? KindFor(Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility ability, TeleportationSpellBlueprintSet spells)
        {
            if (ability == null) return null;
            if (ReferenceEquals(ability, spells.Teleport)) return TeleportSpellKind.Teleport;
            if (ReferenceEquals(ability, spells.GreaterTeleport)) return TeleportSpellKind.GreaterTeleport;
            if (ReferenceEquals(ability, spells.WordOfRecall)) return TeleportSpellKind.WordOfRecall;
            return null;
        }

        // Equivalent variants (same spell and caster level) aggregate into one
        // row; materially different caster levels stay distinguishable. The
        // representative identity is stable: the project's standard scroll when
        // present, otherwise the lowest blueprint GUID in the group.
        private static List<ScrollGroup> CollectGroups(Player player)
        {
            var groups = new List<ScrollGroup>();
            var seen = new HashSet<ItemsCollection>();
            foreach (var unit in player.Party)
            {
                if (unit == null || unit.Inventory == null || !seen.Add(unit.Inventory)) continue;
                foreach (var entity in unit.Inventory)
                {
                    if (entity == null || entity.Count <= 0) continue;
                    var kind = AssociatedSpell(entity.Blueprint);
                    if (kind == null) continue;
                    var usable = (BlueprintItemEquipmentUsable)entity.Blueprint;
                    var group = groups.FirstOrDefault(value => value.Spell == kind.Value && value.CasterLevel == usable.CasterLevel);
                    if (group == null)
                    {
                        group = new ScrollGroup { Spell = kind.Value, CasterLevel = usable.CasterLevel, SpellLevel = usable.SpellLevel };
                        groups.Add(group);
                    }
                    if (!group.Items.Contains(entity)) group.Items.Add(entity);
                }
            }
            foreach (var group in groups)
            {
                var blueprints = group.Items.Select(value => value.Blueprint).OfType<BlueprintItemEquipmentUsable>().Distinct().ToArray();
                group.Representative = blueprints.FirstOrDefault(value => IsStandardScroll(value, group.Spell)) ??
                    blueprints.OrderBy(value => value.AssetGuid, StringComparer.Ordinal).First();
                group.SpellLevel = group.Representative.SpellLevel;
            }
            return groups;
        }

        private static bool IsStandardScroll(BlueprintItem item, TeleportSpellKind kind)
        {
            var scrolls = BlueprintBootstrap.TeleportationScrolls;
            return scrolls != null && ReferenceEquals(item,
                kind == TeleportSpellKind.Teleport ? scrolls.Teleport :
                kind == TeleportSpellKind.GreaterTeleport ? scrolls.GreaterTeleport : scrolls.WordOfRecall);
        }

        internal static TeleportationScrollCastSource Resolve(TeleportCastSourceSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Kind != TeleportCastSourceKind.Scroll) return null;
            var player = Game.Instance == null ? null : Game.Instance.Player;
            if (player == null) return null;
            var groups = CollectGroups(player);
            var group = groups.FirstOrDefault(value => value.Spell == snapshot.Spell &&
                string.Equals(value.Representative.AssetGuid, snapshot.BookId, StringComparison.Ordinal));
            if (group == null || !group.Items.Any(value => value.Count > 0)) return null;
            // Deterministic binding: first item in traveling-party order, then
            // collection order. Equivalent stacks aggregate; the choice stays
            // within the aggregated variant group.
            var seen = new HashSet<ItemsCollection>();
            foreach (var unit in player.Party)
            {
                if (unit == null || unit.Inventory == null || !seen.Add(unit.Inventory)) continue;
                foreach (var entity in unit.Inventory)
                {
                    if (entity != null && entity.Count > 0 && AssociatedSpell(entity.Blueprint) == snapshot.Spell &&
                        ((BlueprintItemEquipmentUsable)entity.Blueprint).CasterLevel == group.CasterLevel)
                        return new TeleportationScrollCastSource(snapshot, entity);
                }
            }
            return null;
        }

        // Native reader eligibility for one spell group: the spell appears in
        // one of the reader's class spell lists (the same check the native
        // inventory UI performs), or the reader has the native UMD skill for a
        // legitimate uncertain attempt. No knowledge, preparation, slot or
        // spellbook is required beyond what native activation itself needs.
        internal static bool IsReader(Kingmaker.EntitySystem.Entities.UnitEntityData unit, ScrollGroup group)
        {
            if (unit == null || unit.Descriptor == null || unit.Descriptor.State.IsDead ||
                unit.Descriptor.State.IsUnconscious || !unit.Descriptor.State.CanAct) return false;
            var ability = AbilityFor(group);
            if (ability != null && ability.IsInSpellListOfUnit(unit.Descriptor)) return true;
            return unit.Descriptor.HasUMDSkill && group.Representative.RequireUMDIfCasterHasNoSpellInSpellList;
        }

        internal static Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility AbilityFor(ScrollGroup group)
        {
            var spells = BlueprintBootstrap.Teleportation;
            return spells == null ? null :
                group.Spell == TeleportSpellKind.Teleport ? spells.Teleport :
                group.Spell == TeleportSpellKind.GreaterTeleport ? spells.GreaterTeleport : spells.WordOfRecall;
        }

        // Counted once per distinct collection; blueprint-association based so
        // crafted variants aggregate with the standard stock.
        internal static int Stock(IReadOnlyList<Kingmaker.EntitySystem.Entities.UnitEntityData> party, BlueprintItem blueprint)
        {
            int total = 0;
            var counted = new HashSet<ItemsCollection>();
            foreach (var unit in party)
            {
                if (unit == null || unit.Inventory == null || !counted.Add(unit.Inventory)) continue;
                foreach (var entity in unit.Inventory)
                    if (entity != null && entity.Count > 0 && ReferenceEquals(entity.Blueprint, blueprint))
                        total += entity.Count;
            }
            return total;
        }
    }

    internal sealed class TeleportationScrollCastSource
    {
        private readonly TeleportCastSourceSnapshot _snapshot;
        private readonly ItemEntity _item;
        internal TeleportCastSourceSnapshot Snapshot { get { return _snapshot; } }
        internal ItemEntity Item { get { return _item; } }
        internal TeleportationScrollCastSource(TeleportCastSourceSnapshot snapshot, ItemEntity item)
        {
            if (snapshot == null || item == null) throw new ArgumentNullException("snapshot/item");
            _snapshot = snapshot; _item = item;
        }
        internal BlueprintItemEquipmentUsable Scroll { get { return (BlueprintItemEquipmentUsable)_item.Blueprint; } }
        internal Kingmaker.EntitySystem.Entities.UnitEntityData Reader
        {
            get
            {
                var player = Game.Instance == null ? null : Game.Instance.Player;
                return player == null ? null : player.AllCharacters.SingleOrDefault(value =>
                    value != null && value.UniqueId == _snapshot.CasterId);
            }
        }
        internal ITeleportCastResource Capture() { return new TeleportationScrollCastResource(this); }
    }

    // One component owns consumption through the native activation boundary:
    // ItemEntity.TryUseFromInventory binds the actual reader and item, runs the
    // native eligibility checks and the RuleCastSpell rulebook event (including
    // a genuine UMD roll when the reader needs one), and consumes exactly one
    // scroll through the native Spend path on a successful activation. The
    // adapter never removes items itself.
    internal sealed class TeleportationScrollCastResource : ITeleportCastResource
    {
        private readonly TeleportationScrollCastSource _source;
        private readonly ItemEntity _item;
        private readonly ItemsCollection _collection;
        private readonly int _beforeCharges;
        private readonly int _beforeCount;
        private readonly int _beforeStock;
        private int _spendAttempted;
        private int _restoreAttempted;
        private TeleportActivationOutcome _activation = TeleportActivationOutcome.NotAttempted;
        private object _activationEvidence;

        internal TeleportationScrollCastResource(TeleportationScrollCastSource source)
        {
            _source = source ?? throw new ArgumentNullException("source");
            _item = source.Item;
            _collection = _item.Collection;
            if (_collection == null) throw new InvalidOperationException("The bound scroll item left its inventory.");
            _beforeCharges = _item.Charges;
            _beforeCount = _item.Count;
            _beforeStock = CurrentStock();
        }

        internal TeleportActivationOutcome Activation { get { return _activation; } }
        internal object ActivationEvidence { get { return _activationEvidence; } }

        public TeleportActivationOutcome ActivationOutcome { get { return _activation; } }

        public void Spend()
        {
            if (_spendAttempted > 0) throw new InvalidOperationException("One scroll can be activated once per request.");
            _spendAttempted++;
            var reader = _source.Reader;
            if (reader == null || !reader.Descriptor.State.CanAct || _item.Count <= 0 || _item.Collection == null)
            {
                _activation = TeleportActivationOutcome.RefusedUnspent;
                _activationEvidence = new { stage = "pre-activation", readerPresent = reader != null };
                return;
            }
            var observer = new TeleportScrollActivationObserver();
            Kingmaker.PubSubSystem.EventBus.Subscribe(observer);
            bool attempted;
            try
            {
                // The exact native boundary the inventory context action reaches:
                // temporary SourceItem fact, native availability checks, the
                // RuleCastSpell event (with the UMD roll when required), native
                // delivery and native consumption on success.
                attempted = _item.TryUseFromInventory(reader, new Kingmaker.Utility.TargetWrapper(reader));
            }
            finally
            {
                Kingmaker.PubSubSystem.EventBus.Unsubscribe(observer);
            }
            TeleportExpenditure expenditure = ObserveExpenditure();
            if (observer.Event == null)
            {
                // The native path refused the activation before any rulebook event.
                _activation = expenditure == TeleportExpenditure.None ?
                    TeleportActivationOutcome.RefusedUnspent : TeleportActivationOutcome.FailedSpent;
            }
            else if (observer.Event.IsUMDFailed)
            {
                // A failed UMD check is a rules failure: no consumption, no teleport.
                _activation = TeleportActivationOutcome.RefusedUnspent;
            }
            else if (!observer.Event.Success)
            {
                // A failed cast keeps the native consumption; no teleport, no refund.
                _activation = TeleportActivationOutcome.FailedSpent;
            }
            else _activation = TeleportActivationOutcome.Succeeded;
            _activationEvidence = new { attempted, observer.Success, observer.IsUMDFailed,
                observer.UmdRoll, umdDC = observer.UmdDc, expenditure = expenditure.ToString() };
        }

        public TeleportExpenditure ObserveExpenditure()
        {
            int delta = _beforeStock - CurrentStock();
            return delta == 1 ? TeleportExpenditure.ExactlyOne : delta == 0 ? TeleportExpenditure.None : TeleportExpenditure.Ambiguous;
        }

        public bool RestoreAndVerifyExactResource()
        {
            if (_restoreAttempted > 0) return ObserveExpenditure() == TeleportExpenditure.None;
            // A native activation already attributed its consumption; a later
            // technical exception never refunds a legitimate native debit.
            if (_activation == TeleportActivationOutcome.Succeeded || _activation == TeleportActivationOutcome.FailedSpent)
                return false;
            if (_spendAttempted == 0 || ObserveExpenditure() != TeleportExpenditure.ExactlyOne) return false;
            _restoreAttempted++;
            _collection.Add(_item.Blueprint, 1);
            return ObserveExpenditure() == TeleportExpenditure.None;
        }

        public object Evidence()
        {
            return new { kind = "scroll", scrollId = _item.Blueprint.AssetGuid, readerId = _source.Snapshot.CasterId,
                boundChargesBefore = _beforeCharges, boundCountBefore = _beforeCount, boundCountNow = _item.Count,
                stockBefore = _beforeStock, stockNow = CurrentStock(),
                spendAttempted = _spendAttempted, restoreAttempted = _restoreAttempted,
                activation = _activation.ToString(), activationEvidence = _activationEvidence };
        }

        private int CurrentStock()
        {
            var player = Game.Instance == null ? null : Game.Instance.Player;
            return player == null ? 0 : TeleportationScrollAdapter.Stock(player.Party, _item.Blueprint);
        }
    }

    // Request-local rulebook observer around exactly one native activation.
    internal sealed class TeleportScrollActivationObserver :
        Kingmaker.PubSubSystem.IGlobalRulebookHandler<RuleCastSpell>
    {
        internal RuleCastSpell Event { get; private set; }
        internal bool? Success { get { return Event == null ? (bool?)null : Event.Success; } }
        internal bool IsUMDFailed { get { return Event != null && Event.IsUMDFailed; } }
        internal int? UmdRoll
        { get { return Event == null || Event.UseMagicDeviceCheck == null ? (int?)null : Event.UseMagicDeviceCheck.RollResult; } }
        internal int? UmdDc
        { get { return Event == null || Event.UseMagicDeviceCheck == null ? (int?)null : (int)Event.UseMagicDeviceCheck.DC; } }
        public void OnEventAboutToTrigger(RuleCastSpell evt) { }
        public void OnEventDidTrigger(RuleCastSpell evt)
        { if (Event == null) Event = evt; }
    }
}
