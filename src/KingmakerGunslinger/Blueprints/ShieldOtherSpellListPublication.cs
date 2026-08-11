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
    internal sealed class ShieldOtherSpellListPublication
    {
        private static readonly FieldInfo FilteredCache = typeof(SpellLevelList)
            .GetField("m_SpellsFiltered", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly List<Mutation> _mutations;
        private ShieldOtherSpellListPublication(List<Mutation> mutations)
        { _mutations = mutations; }

        internal static ShieldOtherSpellListPublication PublishRequiredBaseLists(
            LibraryScriptableObject library, BlueprintAbility shieldOther)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (shieldOther == null) throw new ArgumentNullException("shieldOther");
            if (FilteredCache == null)
                throw new MissingFieldException(typeof(SpellLevelList).FullName,
                    "m_SpellsFiltered");
            var mutations = new List<Mutation>();
            try
            {
                foreach (Target target in RequiredTargets)
                {
                    BlueprintSpellList list = BlueprintLibraryLookup
                        .RequireExact<BlueprintSpellList>(library, target.Guid, target.Role);
                    SpellLevelList level = RequireLevel(list, 2, target.Role);
                    Mutation existing = mutations.SingleOrDefault(value =>
                        value.Levels.Any(item => ReferenceEquals(item, level)) ||
                        ReferenceEquals(value.Before, level.Spells));
                    if (existing == null)
                    {
                        List<BlueprintAbility> before = level.Spells;
                        List<BlueprintAbility> published = ShieldOtherSpellListMergePolicy
                            .Merge(before, shieldOther, value => value.AssetGuid);
                        level.Spells = published;
                        ClearCache(level);
                        existing = new Mutation(level, before, published);
                        mutations.Add(existing);
                    }
                    else if (!existing.Levels.Any(item => ReferenceEquals(item, level)))
                    {
                        level.Spells = existing.Published;
                        ClearCache(level);
                        existing.Levels.Add(level);
                    }
                    Validate(level, shieldOther, target.Role);
                }
                return new ShieldOtherSpellListPublication(mutations);
            }
            catch (Exception publicationException)
            {
                try { RollbackAll(mutations); }
                catch (Exception rollbackException)
                {
                    throw new AggregateException(
                        "Shield Other base publication failed and rollback was refused.",
                        publicationException, rollbackException);
                }
                throw;
            }
        }

        internal void Rollback() { RollbackAll(_mutations); }

        private static SpellLevelList RequireLevel(BlueprintSpellList list,
            int spellLevel, string role)
        {
            SpellLevelList[] levels = (list.SpellsByLevel ??
                Array.Empty<SpellLevelList>()).Where(value => value != null &&
                    value.SpellLevel == spellLevel).ToArray();
            if (levels.Length != 1)
                throw new InvalidOperationException(role + " has " + levels.Length +
                    " physical level-" + spellLevel + " entries.");
            return levels[0];
        }

        private static void Validate(SpellLevelList level,
            BlueprintAbility shieldOther, string role)
        {
            if (level.Spells == null)
                throw new InvalidOperationException(role + " has a null spell list.");
            int references = level.Spells.Count(value => ReferenceEquals(value, shieldOther));
            int guids = level.Spells.Count(value => value != null && string.Equals(
                value.AssetGuid, shieldOther.AssetGuid, StringComparison.Ordinal));
            if (references != 1 || guids != 1)
                throw new InvalidOperationException(role +
                    " does not contain exactly one Shield Other at level 2.");
        }

        private static void RollbackAll(IList<Mutation> mutations)
        {
            for (int i = mutations.Count - 1; i >= 0; i--)
            {
                Mutation mutation = mutations[i];
                foreach (SpellLevelList level in mutation.Levels)
                    if (!ShieldOtherSpellListMergePolicy.CanRollback(
                        level.Spells, mutation.Published))
                        throw new InvalidOperationException(
                            "A spell list changed after Shield Other publication; rollback refused.");
                foreach (SpellLevelList level in mutation.Levels)
                {
                    level.Spells = mutation.Before;
                    ClearCache(level);
                }
            }
        }

        private static void ClearCache(SpellLevelList level)
        { FilteredCache.SetValue(level, null); }

        private sealed class Mutation
        {
            internal Mutation(SpellLevelList level, List<BlueprintAbility> before,
                List<BlueprintAbility> published)
            { Levels = new List<SpellLevelList> { level }; Before = before;
                Published = published; }
            internal List<SpellLevelList> Levels { get; private set; }
            internal List<BlueprintAbility> Before { get; private set; }
            internal List<BlueprintAbility> Published { get; private set; }
        }

        private sealed class Target
        {
            internal Target(string role, string guid) { Role = role; Guid = guid; }
            internal string Role { get; private set; }
            internal string Guid { get; private set; }
        }

        private static readonly Target[] RequiredTargets = {
            new Target("native Cleric spell list", "8443ce803d2d31347897a3d85cc32f53"),
            new Target("native Paladin spell list", "9f5be2f7ea64fe04eb40878347b147bc"),
            new Target("native Inquisitor spell list", "57c894665b7895c499b3dce058c284b3"),
            new Target("native Community domain spell list", "75576ed8cab010644a11f9ecd512a7f9"),
            new Target("native Protection domain spell list", "93228f4df23d2d448a0db59141af8aed")
        };
    }
}
