using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Inventory-backed strategic sources. The traveling party's scroll items —
    // equipped or carried — form one shared stock per scroll kind; any active,
    // living reader who can legitimately attempt the native activation (the
    // spell appears in one of the reader's books, or Use Magic Device is
    // trained) offers the scroll action. The stash and inactive companions are
    // never party inventories and are therefore never enumerated.
    internal static class TeleportationScrollAdapter
    {
        internal static IReadOnlyList<TeleportCastSourceSnapshot> Enumerate(Player player)
        {
            var result = new List<TeleportCastSourceSnapshot>();
            if (player == null || BlueprintBootstrap.TeleportationScrolls == null ||
                !ModContext.TryGet(out var context) || !context.FeatureModules.Active.TeleportationSpells)
                return result;
            var scrolls = BlueprintBootstrap.TeleportationScrolls;
            var party = player.Party.ToArray();
            foreach (var reader in party)
            {
                if (!IsReader(reader, scrolls)) continue;
                int order = Array.IndexOf(party, reader);
                foreach (var pair in new[]
                {
                    new { item = scrolls.Teleport, spell = TeleportSpellKind.Teleport },
                    new { item = scrolls.GreaterTeleport, spell = TeleportSpellKind.GreaterTeleport },
                    new { item = scrolls.WordOfRecall, spell = TeleportSpellKind.WordOfRecall }
                })
                {
                    int stock = Stock(party, pair.item);
                    if (stock <= 0) continue;
                    result.Add(new TeleportCastSourceSnapshot(reader.UniqueId, order,
                        reader.CharacterName,
                        pair.item.AssetGuid, "Scroll", pair.spell, TeleportCastSourceKind.Scroll,
                        pair.item.SpellLevel, stock,
                        TeleportCastSourceFacts.ActiveParty | TeleportCastSourceFacts.LivingAvailableCaster |
                        TeleportCastSourceFacts.ScrollStock | TeleportCastSourceFacts.ExactSpell |
                        TeleportCastSourceFacts.RealResource));
                }
            }
            return result;
        }

        internal static TeleportationScrollCastSource Resolve(TeleportCastSourceSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Kind != TeleportCastSourceKind.Scroll) return null;
            var player = Game.Instance == null ? null : Game.Instance.Player;
            if (player == null) return null;
            var scroll = ScrollFor(snapshot);
            if (scroll == null) return null;
            // Deterministic binding: the first matching item in traveling-party
            // order, then collection order. Equivalent stacks aggregate; the
            // choice never switches material variants because the blueprint is
            // part of the source key.
            foreach (var unit in player.Party)
            {
                if (unit == null || unit.Inventory == null) continue;
                foreach (var entity in unit.Inventory)
                {
                    if (entity != null && entity.Count > 0 && ReferenceEquals(entity.Blueprint, scroll))
                        return new TeleportationScrollCastSource(snapshot, entity);
                }
            }
            return null;
        }

        internal static BlueprintItemEquipmentUsable ScrollFor(TeleportCastSourceSnapshot snapshot)
        {
            var scrolls = BlueprintBootstrap.TeleportationScrolls;
            if (scrolls == null) return null;
            return snapshot.Spell == TeleportSpellKind.Teleport ? scrolls.Teleport :
                snapshot.Spell == TeleportSpellKind.GreaterTeleport ? scrolls.GreaterTeleport : scrolls.WordOfRecall;
        }

        internal static int Stock(IReadOnlyList<Kingmaker.EntitySystem.Entities.UnitEntityData> party, BlueprintItem blueprint)
        {
            int total = 0;
            foreach (var unit in party)
            {
                if (unit == null || unit.Inventory == null) continue;
                foreach (var entity in unit.Inventory)
                    if (entity != null && entity.Count > 0 && ReferenceEquals(entity.Blueprint, blueprint))
                        total += entity.Count;
            }
            return total;
        }

        private static bool IsReader(Kingmaker.EntitySystem.Entities.UnitEntityData unit, TeleportationScrollBlueprintSet scrolls)
        {
            if (unit == null || unit.Descriptor == null || unit.Descriptor.State.IsDead || unit.Descriptor.State.IsUnconscious)
                return false;
            foreach (var book in unit.Descriptor.Spellbooks)
                foreach (var level in new[] { 5, 6, 7 })
                    foreach (var ability in book.GetKnownSpells(level))
                        if (ability != null && (ReferenceEquals(ability.Blueprint, scrolls.Teleport) ||
                            ReferenceEquals(ability.Blueprint, scrolls.GreaterTeleport) ||
                            ReferenceEquals(ability.Blueprint, scrolls.WordOfRecall)))
                            return true;
            // A trained Use Magic Device reader may attempt the native activation
            // without any spellbook, slot or preparation.
            var umd = unit.Descriptor.Stats.GetStat(StatType.SkillUseMagicDevice);
            return umd != null && umd.BaseValue > 0;
        }
    }

    internal sealed class TeleportationScrollCastSource
    {
        private readonly TeleportCastSourceSnapshot _snapshot;
        private readonly ItemEntity _item;
        internal TeleportCastSourceSnapshot Snapshot { get { return _snapshot; } }
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

    // One component owns consumption: exactly one scroll leaves the bound
    // stack; no spell slot is touched. Only a proven technical debit before any
    // material effect is compensated.
    internal sealed class TeleportationScrollCastResource : ITeleportCastResource
    {
        private readonly TeleportationScrollCastSource _source;
        private readonly ItemEntity _item;
        private readonly int _before;
        private int _spendAttempted;
        private int _restoreAttempted;
        internal TeleportationScrollCastResource(TeleportationScrollCastSource source)
        {
            _source = source ?? throw new ArgumentNullException("source");
            _item = BoundItem(source);
            if (_item == null || _item.Collection == null) throw new InvalidOperationException("The bound scroll item left its inventory.");
            _before = CurrentStock();
        }

        public void Spend()
        {
            if (_spendAttempted > 0) throw new InvalidOperationException("One scroll can be spent once per request.");
            _spendAttempted++;
            _item.Collection.Remove(_item.Blueprint, 1);
        }

        public TeleportExpenditure ObserveExpenditure()
        {
            int delta = _before - CurrentStock();
            return delta == 1 ? TeleportExpenditure.ExactlyOne : delta == 0 ? TeleportExpenditure.None : TeleportExpenditure.Ambiguous;
        }

        public bool RestoreAndVerifyExactResource()
        {
            if (_restoreAttempted > 0) return ObserveExpenditure() == TeleportExpenditure.None;
            // Only a proven single technical debit is ever compensated, and only
            // before any material effect. Rules outcomes are never refunded.
            if (_spendAttempted == 0 || ObserveExpenditure() != TeleportExpenditure.ExactlyOne) return false;
            _restoreAttempted++;
            _item.Collection.Add(_item.Blueprint, 1);
            return ObserveExpenditure() == TeleportExpenditure.None;
        }

        public object Evidence()
        {
            return new { kind = "scroll", scrollId = _item.Blueprint.AssetGuid, readerId = _source.Snapshot.CasterId,
                boundCount = _item.Count, stockBefore = _before, stockNow = CurrentStock(),
                spendAttempted = _spendAttempted, restoreAttempted = _restoreAttempted };
        }

        private int CurrentStock()
        {
            var player = Game.Instance == null ? null : Game.Instance.Player;
            return player == null ? 0 : TeleportationScrollAdapter.Stock(player.Party, _item.Blueprint);
        }

        private static ItemEntity BoundItem(TeleportationScrollCastSource source)
        {
            var player = Game.Instance == null ? null : Game.Instance.Player;
            if (player == null) return null;
            foreach (var unit in player.Party)
            {
                if (unit == null || unit.Inventory == null) continue;
                foreach (var entity in unit.Inventory)
                    if (entity != null && entity.Count > 0 && ReferenceEquals(entity.Blueprint, source.Scroll))
                        return entity;
            }
            return null;
        }
    }

}
