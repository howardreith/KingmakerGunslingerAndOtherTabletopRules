using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Spells.ShieldOther;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class TeleportationSpellListPublication
    {
        internal const string WizardListId = "ba0401fdeb4062f40a7aa95b6f07fe89";
        internal const string TravelListId = "ab90308db82342f47bf0d636fe941434";
        internal const string ClericListId = "8443ce803d2d31347897a3d85cc32f53";
        internal const string DruidListId = "bad8638d40639d04fa2f80a1cac67d6b";
        private static readonly FieldInfo FilteredCache = typeof(SpellLevelList).GetField("m_SpellsFiltered", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly List<Mutation> _mutations;
        private TeleportationSpellListPublication(List<Mutation> mutations, bool travelDomain)
        { _mutations = mutations; TravelDomainPublished = travelDomain; }
        internal bool TravelDomainPublished { get; private set; }
        internal int ChangedLevelCount { get { return _mutations.Count; } }

        internal static TeleportationSpellListPublication Publish(LibraryScriptableObject library, TeleportationSpellBlueprintSet spells)
        {
            if (library == null || spells == null) throw new ArgumentNullException("library/spells");
            if (FilteredCache == null) throw new MissingFieldException(typeof(SpellLevelList).FullName, "m_SpellsFiltered");
            var targets = new List<Target> {
                Resolve(library, WizardListId, 5, spells.Teleport),
                Resolve(library, WizardListId, 7, spells.GreaterTeleport),
                Resolve(library, ClericListId, 6, spells.WordOfRecall),
                Resolve(library, DruidListId, 8, spells.WordOfRecall)
            };
            // Optional means genuinely absent. A present wrong type or malformed list
            // fails the module transaction rather than silently claiming publication.
            var travelMatches = library.BlueprintsByAssetId.Values.Where(value => value != null && value.AssetGuid == TravelListId).ToArray();
            if (travelMatches.Length > 1) throw new InvalidOperationException("Native Travel list identity is ambiguous.");
            bool travelPresent = travelMatches.Length == 1;
            if (travelPresent) {
                targets.Add(Resolve(library, TravelListId, 5, spells.Teleport));
                targets.Add(Resolve(library, TravelListId, 7, spells.GreaterTeleport));
            }
            var planned = new List<Mutation>();
            foreach (Target target in targets) {
                Mutation sameLevel = planned.SingleOrDefault(value => ReferenceEquals(value.Level, target.Level));
                if (sameLevel != null) {
                    if (!ReferenceEquals(sameLevel.Spell, target.Spell))
                        throw new InvalidOperationException("One physical native spell level aliases incompatible publication targets.");
                    continue;
                }
                if (target.Level.Spells == null) throw new InvalidOperationException("Native spell level has no collection.");
                var before = target.Level.Spells;
                var after = ShieldOtherSpellListMergePolicy.Merge(before, target.Spell, value => value.AssetGuid);
                planned.Add(new Mutation(target.Level, target.Spell, before, after, FilteredCache.GetValue(target.Level)));
            }
            var applied = new List<Mutation>();
            try {
                foreach (Mutation mutation in planned) {
                    if (mutation.Before.Count == mutation.After.Count && mutation.Before.SequenceEqual(mutation.After)) continue;
                    applied.Add(mutation);
                    mutation.Level.Spells = mutation.After;
                    FilteredCache.SetValue(mutation.Level, null);
                }
                foreach (Target target in targets) Validate(target.Level, target.Spell);
                return new TeleportationSpellListPublication(applied, travelPresent);
            }
            catch (Exception publicationException) {
                try { RollbackAll(applied); }
                catch (Exception rollbackException) {
                    throw new AggregateException("Teleportation spell publication and exact rollback failed.", publicationException, rollbackException);
                }
                throw;
            }
        }

        internal void Rollback() { RollbackAll(_mutations); }
        private static void RollbackAll(List<Mutation> mutations)
        {
            // Validate the entire rollback before modifying even one level.
            foreach (Mutation mutation in mutations)
                if (!ReferenceEquals(mutation.Level.Spells, mutation.After))
                    throw new InvalidOperationException("Native list changed after Teleportation publication; exact rollback refused.");
            for (int index = mutations.Count - 1; index >= 0; index--) {
                Mutation mutation = mutations[index];
                mutation.Level.Spells = mutation.Before;
                FilteredCache.SetValue(mutation.Level, mutation.CacheBefore);
                if (!ReferenceEquals(mutation.Level.Spells, mutation.Before) || !ReferenceEquals(FilteredCache.GetValue(mutation.Level), mutation.CacheBefore))
                    throw new InvalidOperationException("Teleportation rollback could not verify exact prior instances.");
            }
            mutations.Clear();
        }
        private static Target Resolve(LibraryScriptableObject library, string id, int level, BlueprintAbility spell)
        {
            var list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(library, id, "native strategic spell list");
            var levels = (list.SpellsByLevel ?? Array.Empty<SpellLevelList>()).Where(value => value != null && value.SpellLevel == level).ToArray();
            if (levels.Length != 1) throw new InvalidOperationException("Native spell list " + id + " lacks one unambiguous level " + level + ".");
            return new Target(levels[0], spell);
        }
        private static void Validate(SpellLevelList level, BlueprintAbility spell)
        {
            if (level.Spells.Count(value => ReferenceEquals(value, spell)) != 1 ||
                level.Spells.Count(value => value != null && value.AssetGuid == spell.AssetGuid) != 1)
                throw new InvalidOperationException("Strategic spell list does not contain exactly one exact spell instance.");
        }
        private sealed class Target
        {
            internal Target(SpellLevelList level, BlueprintAbility spell) { Level = level; Spell = spell; }
            internal SpellLevelList Level { get; private set; }
            internal BlueprintAbility Spell { get; private set; }
        }
        private sealed class Mutation
        {
            internal Mutation(SpellLevelList level, BlueprintAbility spell, List<BlueprintAbility> before, List<BlueprintAbility> after, object cache)
            { Level = level; Spell = spell; Before = before; After = after; CacheBefore = cache; }
            internal SpellLevelList Level { get; private set; }
            internal BlueprintAbility Spell { get; private set; }
            internal List<BlueprintAbility> Before { get; private set; }
            internal List<BlueprintAbility> After { get; private set; }
            internal object CacheBefore { get; private set; }
        }
    }
}
