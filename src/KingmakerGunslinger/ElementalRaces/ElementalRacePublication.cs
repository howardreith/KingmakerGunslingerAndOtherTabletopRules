using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalRacePublication
    {
        private readonly BlueprintRace[] _previous;
        private readonly BlueprintRace[] _published;

        internal ElementalRacePublication(BlueprintRace[] previous,
            BlueprintRace[] published)
        {
            _previous = previous ?? throw new ArgumentNullException("previous");
            _published = published ?? throw new ArgumentNullException("published");
        }

        internal bool Changed
        {
            get { return !ReferenceEquals(_previous, _published); }
        }

        internal int PreviousCount { get { return _previous.Length; } }
        internal int PublishedCount { get { return _published.Length; } }

        internal void Rollback()
        {
            if (!Changed) return;
            BlueprintRoot root = BlueprintRoot.Instance;
            if (root == null || root.Progression == null ||
                !ReferenceEquals(root.Progression.CharacterRaces, _published))
                throw new InvalidOperationException(
                    "CharacterRaces changed after elemental race publication; rollback refused.");
            root.Progression.CharacterRaces = _previous;
            if (!ReferenceEquals(root.Progression.CharacterRaces, _previous))
                throw new InvalidOperationException(
                    "Elemental race publication rollback could not restore the original catalog reference.");
        }

        internal static ElementalRacePublication Apply(
            ElementalRaceBlueprintSet set, bool enabled)
        {
            if (set == null) throw new ArgumentNullException("set");
            BlueprintRoot root = BlueprintRoot.Instance;
            if (root == null || root.Progression == null ||
                root.Progression.CharacterRaces == null)
                throw new InvalidOperationException(
                    "Kingmaker's CharacterRaces catalog is unavailable.");
            BlueprintRace[] previous = root.Progression.CharacterRaces;
            ValidateSharedCatalog(previous);
            BlueprintRace[] ordered = set.OrderedRaces();
            ValidateProjectRaces(ordered);
            if (!enabled)
                return new ElementalRacePublication(previous, previous);

            var missing = new List<BlueprintRace>();
            foreach (BlueprintRace race in ordered)
            {
                BlueprintRace[] matches = previous.Where(value =>
                    ReferenceEquals(value, race) || string.Equals(
                        value.AssetGuid, race.AssetGuid,
                        StringComparison.Ordinal)).ToArray();
                if (matches.Length > 1)
                    throw new InvalidOperationException(
                        "CharacterRaces already contains duplicate project race identity " +
                        race.AssetGuid + ".");
                if (matches.Length == 1)
                {
                    if (!ReferenceEquals(matches[0], race))
                        throw new InvalidOperationException(
                            "CharacterRaces contains a foreign object for project race identity " +
                            race.AssetGuid + ".");
                    continue;
                }
                missing.Add(race);
            }
            if (missing.Count == 0)
                return new ElementalRacePublication(previous, previous);

            BlueprintRace[] published = previous.Concat(missing).ToArray();
            try
            {
                root.Progression.CharacterRaces = published;
                if (!ReferenceEquals(root.Progression.CharacterRaces,
                    published))
                    throw new InvalidOperationException(
                        "CharacterRaces did not retain the published array reference.");
                for (int index = 0; index < previous.Length; index++)
                    if (!ReferenceEquals(published[index], previous[index]))
                        throw new InvalidOperationException(
                            "Elemental race publication changed a native or third-party race entry.");
                foreach (BlueprintRace race in ordered)
                    if (published.Count(value => ReferenceEquals(value, race)) != 1)
                        throw new InvalidOperationException(
                            "Elemental race publication did not produce exactly one " +
                            race.name + " entry.");
                int expectedIndex = previous.Length;
                foreach (BlueprintRace race in ordered.Where(value =>
                    missing.Contains(value)))
                    if (!ReferenceEquals(published[expectedIndex++], race))
                        throw new InvalidOperationException(
                            "Elemental race append order is not deterministic.");
                return new ElementalRacePublication(previous, published);
            }
            catch
            {
                if (ReferenceEquals(root.Progression.CharacterRaces,
                    published))
                    root.Progression.CharacterRaces = previous;
                throw;
            }
        }

        private static void ValidateSharedCatalog(BlueprintRace[] races)
        {
            var identities = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < races.Length; index++)
            {
                BlueprintRace race = races[index];
                if (race == null || string.IsNullOrWhiteSpace(race.AssetGuid))
                    throw new InvalidOperationException(
                        "CharacterRaces contains a null or identity-free entry at index " +
                        index + ".");
                if (!identities.Add(race.AssetGuid))
                    throw new InvalidOperationException(
                        "CharacterRaces contains duplicate identity " +
                        race.AssetGuid + ".");
            }
        }

        private static void ValidateProjectRaces(BlueprintRace[] races)
        {
            if (races == null || races.Length != ElementalRaceCatalog.RaceCount ||
                races.Any(value => value == null ||
                    string.IsNullOrWhiteSpace(value.AssetGuid)) ||
                races.Select(value => value.AssetGuid).Distinct(
                    StringComparer.Ordinal).Count() !=
                        ElementalRaceCatalog.RaceCount)
                throw new InvalidOperationException(
                    "All four distinct elemental race identities are required before publication.");
        }
    }
}
