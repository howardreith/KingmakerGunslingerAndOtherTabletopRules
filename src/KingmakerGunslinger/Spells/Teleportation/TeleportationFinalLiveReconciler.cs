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
using KingmakerGunslinger.Spells.ShieldOther;
using UnityModManagerNet;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Optional Call of the Wild Oracle reconciliation for Word of Recall.
    // The base publication adds the canonical ability to the native Cleric 6
    // and Druid 8 lists while the library loads; Call of the Wild keeps the
    // Oracle on its OWN spell list (a distinct BlueprintSpellList derived from
    // the cleric list), so that publication never reaches an Oracle. The
    // native scroll reader predicate (BlueprintAbility.IsInSpellListOfUnit)
    // walks Progression.Classes -> ClassData.Spellbook.SpellList, so a
    // zero-UMD Oracle is refused exactly like the owner reported. This
    // reconciler mirrors the qualified Shield Other final-live pattern: at
    // the first idle update after every optional mod has loaded, the Oracle
    // class is resolved through validated blueprint identity and structure,
    // and the canonical Word of Recall is merged once at level 6 of that
    // class's actual final spell list. Oracle absence, a disabled module, a
    // failed/absent base publication, or a foreign duplicate leaves every
    // other module and the native Cleric 6 / Druid 8 registration untouched.
    internal static class TeleportationFinalLiveReconciler
    {
        internal const string OracleClassId = "32c02466b2364c8a906e6e4761175099";
        internal const int OracleWordOfRecallLevel = 6;
        private static readonly object Gate = new object();
        private static readonly FieldInfo FilteredCache = typeof(SpellLevelList)
            .GetField("m_SpellsFiltered", BindingFlags.Instance | BindingFlags.NonPublic);
        private static ModContext _context;
        private static bool _attached;

        internal static void AttachFirstUpdate(ModContext context)
        {
            if (context == null) return;
            lock (Gate)
            {
                _context = context;
                if (_attached) return;
                _attached = true;
            }
            context.ModEntry.OnUpdate += FirstUpdate;
        }

        private static void FirstUpdate(UnityModManager.ModEntry entry, float delta)
        {
            ModContext context;
            lock (Gate)
            {
                context = _context; _attached = false;
            }
            context.ModEntry.OnUpdate -= FirstUpdate;
            try
            {
                LibraryScriptableObject library = BlueprintBootstrap.Library;
                TeleportationSpellBlueprintSet set = BlueprintBootstrap.Teleportation;
                if (library == null || set == null || set.WordOfRecall == null)
                    throw new InvalidOperationException(
                        "Teleportation bootstrap identities were unavailable at first idle update.");
                BlueprintAbility wordOfRecall = set.WordOfRecall;
                BlueprintAbility[] duplicates = FinalAbilities(library).Where(value =>
                    !ReferenceEquals(value, wordOfRecall) && IsWordOfRecall(value)).ToArray();
                if (duplicates.Length != 0)
                {
                    // Fail closed for the optional Oracle reconciliation only; the
                    // native Cleric 6 / Druid 8 base publication stays intact.
                    context.Logger.Failure("teleportation-spells", "duplicate.final-live",
                        "Final-live duplicate Word of Recall content was found; optional Oracle reconciliation was refused. Candidates=" +
                        string.Join(",", duplicates.Select(value => value.AssetGuid + ":" + value.name).ToArray()), null);
                    return;
                }
                if (!context.FeatureModules.Active.TeleportationSpells ||
                    BlueprintBootstrap.TeleportationPublication == null)
                {
                    context.Logger.Info("teleportation-spells", "reconcile.disabled",
                        "Final-live duplicate scan passed; Oracle reconciliation is disabled while the module is off or the base publication is absent; identities remain registered.");
                    return;
                }
                ReconcileOptional(library, wordOfRecall);
                ReconcileOptional(library, wordOfRecall);
                context.Logger.Info("teleportation-spells", "reconcile.complete",
                    "Final-live optional Oracle level-6 Word of Recall reconciliation completed twice idempotently.");
            }
            catch (Exception exception)
            {
                context.Logger.Failure("teleportation-spells", "reconcile.failed",
                    "Final-live optional Oracle reconciliation failed closed without disabling other modules.",
                    exception);
            }
        }

        private static void ReconcileOptional(LibraryScriptableObject library,
            BlueprintAbility wordOfRecall)
        {
            BlueprintCharacterClass candidate = ResolveOracleClass(library);
            if (candidate == null) return;
            var changes = new List<Change>();
            try
            {
                SpellLevelList level = candidate.Spellbook.SpellList.SpellsByLevel
                    .Single(value => value != null && value.SpellLevel == OracleWordOfRecallLevel);
                List<BlueprintAbility> before = level.Spells;
                List<BlueprintAbility> published = ShieldOtherSpellListMergePolicy
                    .Merge(before, wordOfRecall, value => value.AssetGuid);
                if (!ReferenceEquals(before, published))
                {
                    level.Spells = published; ClearCache(level);
                    changes.Add(new Change(level, before, published));
                }
                Validate(level, wordOfRecall, candidate.name);
            }
            catch
            {
                for (int i = changes.Count - 1; i >= 0; i--)
                {
                    Change change = changes[i];
                    if (!ReferenceEquals(change.Level.Spells, change.Published))
                        throw new InvalidOperationException(
                            "Optional Oracle spell list changed during rollback; restoration refused.");
                    change.Level.Spells = change.Before; ClearCache(change.Level);
                }
                throw;
            }
        }

        // Shared production identity resolution: the optional Oracle class is
        // resolved through its known Call of the Wild GUID plus the exact
        // structural contract (self-owned spontaneous divine Charisma
        // spellbook reaching the required spell level), never a display name
        // alone. Exactly one candidate or none; ambiguity fails closed.
        internal static BlueprintCharacterClass ResolveOracleClass(
            LibraryScriptableObject library)
        {
            BlueprintCharacterClass[] named = FinalBlueprints(library)
                .OfType<BlueprintCharacterClass>().Where(value =>
                {
                    string text = (value.name + " " + value.Name).ToLowerInvariant();
                    return text.Contains("oracle");
                }).ToArray();
            if (named.Length == 0) return null;
            BlueprintCharacterClass[] matches = named.Where(value =>
            {
                BlueprintSpellbook book = value.Spellbook;
                return book != null &&
                    ReferenceEquals(book.CharacterClass, value) && book.SpellList != null &&
                    book.SpellList.MaxLevel >= OracleWordOfRecallLevel &&
                    book.Spontaneous && !book.IsArcane &&
                    book.CastingAttribute == StatType.Charisma &&
                    string.Equals(value.AssetGuid, OracleClassId, StringComparison.Ordinal);
            }).ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException("Oracle final-live candidate count=" +
                    matches.Length + ".");
            return matches[0];
        }

        private static void Validate(SpellLevelList level,
            BlueprintAbility wordOfRecall, string role)
        {
            int references = level.Spells.Count(value => ReferenceEquals(value, wordOfRecall));
            int guids = level.Spells.Count(value => value != null && string.Equals(
                value.AssetGuid, wordOfRecall.AssetGuid, StringComparison.Ordinal));
            if (references != 1 || guids != 1)
                throw new InvalidOperationException(role +
                    " Word of Recall optional publication is not singular.");
        }

        private static bool IsWordOfRecall(BlueprintAbility ability)
        {
            string text = (ability.name + " " + ability.Name + " " +
                ability.Description).ToLowerInvariant();
            return text.Contains("word of recall") || text.Contains("wordofrecall") ||
                text.Contains("word_of_recall");
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
