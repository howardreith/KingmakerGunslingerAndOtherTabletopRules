using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Blueprints;
using UnityModManagerNet;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal static class ShieldOtherFinalLiveReconciler
    {
        private static readonly object Gate = new object();
        private static readonly FieldInfo FilteredCache = typeof(SpellLevelList)
            .GetField("m_SpellsFiltered", BindingFlags.Instance | BindingFlags.NonPublic);
        private static ModContext _context;
        private static LibraryScriptableObject _library;
        private static BlueprintAbility _shieldOther;
        private static bool _enabled;
        private static ShieldOtherSpellListPublication _basePublication;
        private static bool _attached;

        internal static void AttachFirstUpdate(ModContext context,
            LibraryScriptableObject library, BlueprintAbility shieldOther, bool enabled,
            ShieldOtherSpellListPublication basePublication)
        {
            if (context == null || library == null || shieldOther == null) return;
            lock (Gate)
            {
                _context = context; _library = library; _shieldOther = shieldOther;
                _enabled = enabled;
                _basePublication = basePublication;
                if (_attached) return;
                _attached = true;
            }
            context.ModEntry.OnUpdate += FirstUpdate;
        }

        private static void FirstUpdate(UnityModManager.ModEntry entry, float delta)
        {
            ModContext context;
            LibraryScriptableObject library;
            BlueprintAbility shieldOther;
            bool enabled;
            ShieldOtherSpellListPublication basePublication;
            lock (Gate)
            {
                context = _context; library = _library; shieldOther = _shieldOther;
                enabled = _enabled; _attached = false;
                basePublication = _basePublication;
            }
            context.ModEntry.OnUpdate -= FirstUpdate;
            try
            {
                BlueprintAbility[] duplicates = FinalAbilities(library).Where(value =>
                    !ReferenceEquals(value, shieldOther) && IsShieldOther(value)).ToArray();
                if (duplicates.Length != 0)
                {
                    if (basePublication != null) basePublication.Rollback();
                    context.Logger.Failure("shield-other", "duplicate.final-live",
                        "Final-live duplicate Shield Other content was found; optional publication was refused. Candidates=" +
                        string.Join(",", duplicates.Select(value => value.AssetGuid + ":" +
                            value.name).ToArray()), null);
                    return;
                }
                if (!enabled)
                {
                    context.Logger.Info("shield-other", "reconcile.disabled",
                        "Final-live duplicate scan passed; Shield Other publication is disabled while identities remain registered.");
                    return;
                }
                ReconcileOptional(library, shieldOther);
                ReconcileOptional(library, shieldOther);
                context.Logger.Info("shield-other", "reconcile.complete",
                    "Final-live optional Oracle, Warpriest, and Psychic reconciliation completed twice idempotently.");
            }
            catch (Exception exception)
            {
                context.Logger.Failure("shield-other", "reconcile.failed",
                    "Final-live optional publication failed closed without disabling other modules.",
                    exception);
            }
        }

        private static void ReconcileOptional(LibraryScriptableObject library,
            BlueprintAbility shieldOther)
        {
            var classes = FinalBlueprints(library).OfType<BlueprintCharacterClass>().ToArray();
            BlueprintCharacterClass[] candidates = new[] {
                Resolve(classes, "Oracle", "32c02466b2364c8a906e6e4761175099",
                    true, false, StatType.Charisma),
                Resolve(classes, "Warpriest", "e119d84528144a7797ad34fd718b1f87",
                    false, false, StatType.Wisdom),
                Resolve(classes, "Psychic", "359bbaacabc445499049b59d295194cb",
                    true, false, StatType.Intelligence)
            }.Where(value => value != null).ToArray();
            var changes = new List<Change>();
            try
            {
                foreach (BlueprintCharacterClass candidate in candidates)
                {
                    SpellLevelList level = candidate.Spellbook.SpellList.SpellsByLevel
                        .Single(value => value != null && value.SpellLevel == 2);
                    List<BlueprintAbility> before = level.Spells;
                    List<BlueprintAbility> published = ShieldOtherSpellListMergePolicy
                        .Merge(before, shieldOther, value => value.AssetGuid);
                    if (!ReferenceEquals(before, published))
                    {
                        level.Spells = published; ClearCache(level);
                        changes.Add(new Change(level, before, published));
                    }
                    Validate(level, shieldOther, candidate.name);
                }
            }
            catch
            {
                for (int i = changes.Count - 1; i >= 0; i--)
                {
                    Change change = changes[i];
                    if (!ReferenceEquals(change.Level.Spells, change.Published))
                        throw new InvalidOperationException(
                            "Optional spell list changed during rollback; restoration refused.");
                    change.Level.Spells = change.Before; ClearCache(change.Level);
                }
                throw;
            }
        }

        private static BlueprintCharacterClass Resolve(
            IEnumerable<BlueprintCharacterClass> classes, string role,
            string knownGuid, bool spontaneous, bool arcane, StatType attribute)
        {
            BlueprintCharacterClass[] named = classes.Where(value =>
            {
                string text = (value.name + " " + value.Name).ToLowerInvariant();
                return text.Contains(role.ToLowerInvariant());
            }).ToArray();
            if (named.Length == 0) return null;
            BlueprintCharacterClass[] matches = named.Where(value =>
            {
                BlueprintSpellbook book = value.Spellbook;
                return book != null &&
                    ReferenceEquals(book.CharacterClass, value) && book.SpellList != null &&
                    book.SpellList.MaxLevel >= 6 && book.Spontaneous == spontaneous &&
                    book.IsArcane == arcane && book.CastingAttribute == attribute &&
                    (string.Equals(value.AssetGuid, knownGuid, StringComparison.Ordinal) ||
                        !string.IsNullOrWhiteSpace(value.Name));
            }).ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException(role + " final-live candidate count=" +
                    matches.Length + ".");
            return matches[0];
        }

        private static void Validate(SpellLevelList level,
            BlueprintAbility shieldOther, string role)
        {
            int references = level.Spells.Count(value => ReferenceEquals(value, shieldOther));
            int guids = level.Spells.Count(value => value != null && string.Equals(
                value.AssetGuid, shieldOther.AssetGuid, StringComparison.Ordinal));
            if (references != 1 || guids != 1)
                throw new InvalidOperationException(role + " optional publication is not singular.");
        }

        private static bool IsShieldOther(BlueprintAbility ability)
        {
            string text = (ability.name + " " + ability.Name + " " +
                ability.Description).ToLowerInvariant();
            return text.Contains("shield other") || text.Contains("shieldother") ||
                text.Contains("shield_other");
        }

        private static BlueprintAbility[] FinalAbilities(LibraryScriptableObject library)
        { return FinalBlueprints(library).OfType<BlueprintAbility>().ToArray(); }

        private static BlueprintScriptableObject[] FinalBlueprints(
            LibraryScriptableObject library)
        { return library.BlueprintsByAssetId.Values.Where(value => value != null).ToArray(); }

        private static void ClearCache(SpellLevelList level)
        {
            if (FilteredCache == null)
                throw new MissingFieldException(typeof(SpellLevelList).FullName,
                    "m_SpellsFiltered");
            FilteredCache.SetValue(level, null);
        }

        private sealed class Change
        {
            internal Change(SpellLevelList level, List<BlueprintAbility> before,
                List<BlueprintAbility> published)
            { Level = level; Before = before; Published = published; }
            internal SpellLevelList Level { get; private set; }
            internal List<BlueprintAbility> Before { get; private set; }
            internal List<BlueprintAbility> Published { get; private set; }
        }
    }
}
