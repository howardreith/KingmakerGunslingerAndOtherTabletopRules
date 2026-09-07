using System;
using System.IO;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ElementalCharacterCreationRoutingTests
    {
        private sealed class EqualObjects
        {
            public override bool Equals(object value) { return value is EqualObjects; }
            public override int GetHashCode() { return 7; }
        }
        internal static void ObservationIdentityUsesReferences()
        {
            var ids = new CharacterCreationObservationIdentity();
            var first = new EqualObjects();
            var second = new EqualObjects();
            Assertions.True(first.Equals(second), "Equality collision fixture is invalid.");
            Assertions.True(ids.Get(first) != ids.Get(second),
                "Equal objects must retain distinct observation references.");
            Assertions.Equal(ids.Get(first), ids.Get(first),
                "Repeated observation must retain its reference ID.");
            Assertions.Equal<string>(null, ids.Get(null), "Null must stay absent.");
        }
        internal static void ArraysDistinguishIdentityOrderAndNull()
        {
            var first = new EqualObjects();
            var second = new EqualObjects();
            var array = new[] { first, second };
            Assertions.True(CharacterCreationObservationIdentity.SameOrderedReferences(
                array, new[] { first, second }), "Cloned arrays must preserve ordered entry references.");
            Assertions.False(CharacterCreationObservationIdentity.SameOrderedReferences(
                array, new[] { second, first }), "A semantic-equality collision hid changed order.");
            Assertions.False(CharacterCreationObservationIdentity.SameOrderedReferences(
                array, new[] { new EqualObjects(), second }), "A new object replaced a foreign reference.");
            Assertions.False(CharacterCreationObservationIdentity.SameOrderedReferences<EqualObjects>(
                null, new EqualObjects[0]), "Null and empty are different contracts.");
            var ids = new CharacterCreationObservationIdentity();
            Assertions.True(ids.Get(array) != ids.Get(array.Clone()),
                "Array reference identity must be separate from ordered entry identity.");
        }
        internal static void ObserverCannotOperateCharacterCreator()
        {
            string path = Path.Combine(FindRoot(), "src", "KingmakerGunslinger",
                "RuntimeTesting", "ElementalCharacterCreationRoutingObserver.cs");
            string source = File.ReadAllText(path);
            foreach (string forbidden in new[] { ".SelectRace(", ".SelectClass(", ".SelectFeature(",
                ".SetupViewState(", ".StartWithoutAssigningStaticInstance(", ".SetPhase(",
                ".ToNextPhase(", ".Commit(", ".LoadGame(", ".SaveGame(", ".SetValue(" })
                Assertions.False(source.Contains(forbidden),
                    "Read-only observer must not operate a build: " + forbidden);
            Assertions.True(source.Contains("request.Scenario !=") &&
                source.Contains("RuntimeTestScenarioCatalog.ObserveElementalCharacterCreationRouting") &&
                source.Contains("if (_request == null"), "Observer requires an exact guarded scenario.");
            Assertions.True(source.Contains("NOT-RUN") && source.Contains("observationOnly") &&
                source.Contains("consumedByActualPhases") && source.Contains("existingViewEmpty") &&
                source.Contains("Progression.CharacterRaces") && source.Contains("published-race-selection-coverage") &&
                source.Contains("selectorLayers") && source.Contains("visibleChoiceCount") &&
                source.Contains("onChargenApply") && source.Contains("callbackInvoked"),
                "Actual routing and unavailable-state evidence must remain explicit.");
        }
        private static string FindRoot()
        {
            var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
                directory = directory.Parent;
            if (directory == null) throw new InvalidOperationException("Repository root unavailable.");
            return directory.FullName;
        }
    }
}
