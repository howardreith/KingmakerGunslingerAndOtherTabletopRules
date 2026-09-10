using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class TeleportationNativeCastSource
    {
        internal TeleportationNativeCastSource(TeleportCastSourceSnapshot snapshot, Spellbook book, AbilityData ability)
        { Snapshot = snapshot; Book = book; Ability = ability; }
        // Inventory-backed sources have no spellbook; the reader is explicit.
        internal TeleportationNativeCastSource(TeleportCastSourceSnapshot snapshot, Spellbook unused,
            AbilityData ability, Kingmaker.EntitySystem.Entities.UnitEntityData reader)
        { Snapshot = snapshot; Book = null; Ability = ability; Caster = reader; }
        internal Kingmaker.EntitySystem.Entities.UnitEntityData Caster
        { get { return _caster ?? (Book == null ? null : Book.Owner.Unit); } private set { _caster = value; } }
        private Kingmaker.EntitySystem.Entities.UnitEntityData _caster;
        internal TeleportCastSourceSnapshot Snapshot { get; private set; }
        internal Spellbook Book { get; private set; }
        internal AbilityData Ability { get; private set; }
        internal TeleportationNativeCastResource Capture()
        { return new TeleportationNativeCastResource(this); }
        internal object Evidence() { return new { kind = Snapshot.Kind.ToString(), casterId = Snapshot.CasterId,
            bookId = Snapshot.BookId, spell = Snapshot.Spell.ToString(), uses = Snapshot.Uses }; }
    }

    internal static class TeleportationSpellbookAdapter
    {
        // Diagnostic suppression only; actual availability is always read from the
        // current campaign's native spellbooks, never from a process cache.
        private static readonly HashSet<string> ReportedFailures = new HashSet<string>(StringComparer.Ordinal);
        internal static IReadOnlyList<TeleportationNativeCastSource> Enumerate(Player player)
        {
            var result = new List<TeleportationNativeCastSource>();
            ModContext context;
            if (!ModContext.TryGet(out context) || !context.FeatureModules.Active.TeleportationSpells ||
                BlueprintBootstrap.TeleportationPublication == null || BlueprintBootstrap.Teleportation == null ||
                Game.Instance == null || !ReferenceEquals(Game.Instance.Player, player)) return result.AsReadOnly();
            UnitEntityData[] party = player.Party.ToArray();
            if (party.Any(value => value == null || string.IsNullOrWhiteSpace(value.UniqueId)) ||
                party.Select(value => value.UniqueId).Distinct(StringComparer.Ordinal).Count() != party.Length)
                return result.AsReadOnly();
            for (int order = 0; order < party.Length; order++)
            {
                UnitEntityData caster = party[order];
                if (!CasterAvailable(caster)) continue;
                Spellbook[] books = caster.Descriptor.Spellbooks.Where(value => value != null && value.Blueprint != null).ToArray();
                foreach (Spellbook book in books.OrderBy(value => value.Blueprint.AssetGuid, StringComparer.Ordinal))
                {
                    if (books.Count(value => value.Blueprint.AssetGuid == book.Blueprint.AssetGuid) != 1 ||
                        !OwnedBook(caster, book)) continue;
                    foreach (TeleportSpellKind kind in Enum.GetValues(typeof(TeleportSpellKind)))
                    {
                        try
                        {
                            TeleportationNativeCastSource source = Read(caster, order, book, kind);
                            if (source != null) result.Add(source);
                        }
                        catch (Exception exception)
                        {
                            string key = caster.UniqueId + "/" + book.Blueprint.AssetGuid + "/" + kind;
                            if (ReportedFailures.Add(key)) context.Logger.Failure("teleportation", "spell-source.unproven",
                                "source=" + key + ";actionOmitted=true", exception);
                        }
                    }
                }
            }
            return result.AsReadOnly();
        }

        internal static TeleportationNativeCastSource Resolve(TeleportCastSourceSnapshot selected)
        {
            if (selected == null || Game.Instance == null) return null;
            TeleportationNativeCastSource[] sources = Enumerate(Game.Instance.Player).Where(value =>
                value.Snapshot.Key == selected.Key && value.Snapshot.Kind == selected.Kind &&
                value.Snapshot.SpellLevel == selected.SpellLevel).ToArray();
            return sources.Length == 1 ? sources[0] : null;
        }

        internal static IEnumerable<AbilityData> KnownAbilities(Spellbook book, int level)
        {
            // Native AddSpecial also registers m_KnownSpellLevels. Its real known
            // instances live in a separate list and use the same spontaneous pool.
            return (book.GetKnownSpells(level) ?? Enumerable.Empty<AbilityData>())
                .Concat(book.GetSpecialSpells(level) ?? Enumerable.Empty<AbilityData>());
        }

        internal static bool CasterAvailable(UnitEntityData caster)
        {
            return caster != null && !caster.IsDetached && caster.Descriptor != null &&
                caster.Descriptor.IsPlayerFaction && !caster.Descriptor.IsPet && !caster.IsSummoned() &&
                !caster.Descriptor.State.IsDead && !caster.Descriptor.State.IsUnconscious && caster.Descriptor.State.CanAct;
        }
        internal static bool OwnedBook(UnitEntityData caster, Spellbook book)
        {
            return caster != null && book != null && book.Blueprint != null &&
                TeleportDestinationPolicy.IsStableId(book.Blueprint.AssetGuid) &&
                ReferenceEquals(book.Owner, caster.Descriptor) && ReferenceEquals(caster.Descriptor.GetSpellbook(book.Blueprint), book);
        }
        internal static bool ExactAbility(AbilityData ability, Spellbook book, BlueprintAbility spell, int level)
        {
            return ability != null && ability.GetType() == typeof(AbilityData) && ReferenceEquals(ability.Blueprint, spell) &&
                ReferenceEquals(ability.Caster, book.Owner) && ReferenceEquals(ability.Spellbook, book) &&
                ability.SourceItem == null && ability.Fact == null && ability.ConvertedFrom == null &&
                (ability.MetamagicData == null || (ability.MetamagicData.MetamagicMask == 0 && ability.MetamagicData.SpellLevelCost == 0)) &&
                book.GetSpellLevel(ability) == level;
        }

        internal static TeleportPreparedPoolDecision Prepared(SpellSlot[] slots, Spellbook book, BlueprintAbility spell, int level)
        {
            if (slots == null) return null;
            // Broken physical identities or cross-level links cannot prove a real use.
            if (slots.Any(value => value == null || value.SpellLevel != level) || slots.Distinct().Count() != slots.Length) return null;
            var states = slots.Select(slot => new TeleportPreparedSlotState(ExactAbility(slot.Spell, book, spell, level),
                slot.Available, slot.IsOpposition, slot.LinkedSlots == null ? null :
                    slot.LinkedSlots.Select(link => Array.FindIndex(slots, value => ReferenceEquals(value, link))))).ToArray();
            return TeleportPreparedPoolPolicy.Analyze(states);
        }

        private static TeleportationNativeCastSource Read(UnitEntityData caster, int order, Spellbook book, TeleportSpellKind kind)
        {
            BlueprintAbility spell = BlueprintBootstrap.Teleportation.Get(kind);
            int level = book.GetSpellLevel(spell);
            if (level < 1 || level > 9) return null;
            AbilityData ability;
            int uses;
            TeleportCastSourceFacts facts = TeleportCastSourceFacts.Required;
            if (book.Blueprint.Spontaneous)
            {
                if (!book.IsKnown(spell)) return null;
                ability = KnownAbilities(book, level).FirstOrDefault(value => ExactAbility(value, book, spell, level));
                uses = book.GetSpontaneousSlots(level);
                if (uses > book.GetSpellsPerDay(level)) return null; // Unproven over-cap/cheat pool.
                facts |= TeleportCastSourceFacts.Known;
            }
            else
            {
                SpellSlot[] slots = (book.GetMemorizedSpellSlots(level) ?? Enumerable.Empty<SpellSlot>()).ToArray();
                TeleportPreparedPoolDecision pool = Prepared(slots, book, spell, level);
                if (pool == null || pool.Uses == 0) return null;
                ability = slots[pool.SelectedOrdinal].Spell;
                uses = pool.Uses;
                facts |= TeleportCastSourceFacts.PreparedUse;
            }
            if (uses <= 0 || ability == null || !ability.IsAvailable || !book.CanSpend(ability)) return null;
            string bookName = book.Blueprint.DisplayName;
            if (string.IsNullOrWhiteSpace(bookName) && book.Blueprint.CharacterClass != null) bookName = book.Blueprint.CharacterClass.Name;
            if (string.IsNullOrWhiteSpace(bookName)) return null;
            var snapshot = new TeleportCastSourceSnapshot(caster.UniqueId, order, caster.CharacterName, book.Blueprint.AssetGuid,
                bookName, kind, book.Blueprint.Spontaneous ? TeleportCastSourceKind.Spontaneous : TeleportCastSourceKind.Prepared,
                level, uses, facts);
            return TeleportCastAvailabilityPolicy.Usable(snapshot) ? new TeleportationNativeCastSource(snapshot, book, ability) : null;
        }
    }
}
