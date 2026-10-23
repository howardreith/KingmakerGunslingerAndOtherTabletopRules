using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    // Source contracts for the native player-class Animal exclusion. The native
    // SelectClass.Check/MeetsPrerequisites behavior itself is executed against the
    // installed assemblies by scripts/Test-GunslingerAnimalCompanionEligibility.ps1.
    internal static class GunslingerAnimalCompanionEligibilityTests
    {
        private const string NativeAnimalClassGuid = "4cd1757a0eea7694ba5c933729a53920";

        internal static void ClassCarriesFightersNativeAnimalExclusion()
        {
            string factory = Read("src", "KingmakerGunslinger", "Blueprints",
                "GunslingerClassBlueprints.cs");
            Assertions.True(factory.Contains("private const string AnimalClassGuid = \"" +
                    NativeAnimalClassGuid + "\";") &&
                Regex.IsMatch(factory,
                    @"RequireExact<BlueprintCharacterClass>\(\s*library, AnimalClassGuid,"),
                "Gunslinger must resolve the exact native AnimalClass blueprint.");

            string createClass = Between(factory,
                "private static BlueprintCharacterClass CreateClass(",
                "private static BlueprintProgression CreateProgression(");
            foreach (string token in new[]
            {
                "BlueprintCharacterClass animal,",
                "ScriptableObject.CreateInstance<PrerequisiteNoClassLevel>()",
                "noAnimalLevels.CharacterClass = animal;",
                "noAnimalLevels.Group = Prerequisite.GroupType.All;",
                "result.ComponentsArray = new BlueprintComponent[] { noAnimalLevels };"
            })
                Assertions.True(createClass.Contains(token),
                    "Gunslinger class creation lacks the native Animal exclusion: " + token);
            Assertions.Equal(1, Regex.Matches(createClass, @"\.ComponentsArray\s*=").Count,
                "Gunslinger class creation must assign exactly one component set.");
            Assertions.True(createClass.IndexOf("result.ComponentsArray", StringComparison.Ordinal) <
                createClass.IndexOf("return result;", StringComparison.Ordinal),
                "The exclusion must exist before the registry receives the class.");

            string validate = Between(factory, "private static void Validate(",
                "AddFacts grant = (AddFacts)proficiencies.ComponentsArray[0];");
            Assertions.True(validate.Contains(
                    "RequireNativePlayerClassEligibility(characterClass, animal);"),
                "Bootstrap validation must fail closed without the native Animal exclusion.");
            string register = Between(factory, "internal static GunslingerClassBlueprintSet Register(",
                "internal static GunslingerClassCatalogPublication Publish(");
            Assertions.True(register.IndexOf("Validate(characterClass, progression, proficiencies, animal,",
                    StringComparison.Ordinal) >= 0 &&
                register.IndexOf("Validate(characterClass", StringComparison.Ordinal) <
                register.IndexOf("return set;", StringComparison.Ordinal),
                "Gunslinger eligibility must be validated during registration.");

            Assertions.False(Regex.IsMatch(factory, @"\.(HideIfRestricted|PrestigeClass)\s*=(?!=)"),
                "Gunslinger keeps Fighter's visible base-class presentation flags.");
            Assertions.Equal(0, Regex.Matches(factory, @"\banimal\.\w+\s*=(?!=)").Count,
                "The native Animal class and its progression must only be referenced, never changed.");
        }

        internal static void IdentityAndCatalogPublicationAreUnchanged()
        {
            JToken[] entries = JObject.Parse(Read("blueprints", "blueprints.json"))["entries"]
                .Where(value => string.Equals((string)value["symbol"],
                    "KMG.Classes.GunslingerClass", StringComparison.Ordinal)).ToArray();
            Assertions.Equal(1, entries.Length, "The Gunslinger class identity must be unique.");
            Assertions.Equal("abca4797366d4df0831a418eee39069a", (string)entries[0]["guid"],
                "The Gunslinger class GUID changed.");
            Assertions.Equal("BlueprintCharacterClass", (string)entries[0]["plannedType"],
                "The Gunslinger class blueprint type changed.");
            Assertions.Equal("active", (string)entries[0]["status"],
                "The Gunslinger class registration was retired.");

            string factory = Read("src", "KingmakerGunslinger", "Blueprints",
                "GunslingerClassBlueprints.cs");
            Assertions.True(factory.Contains(
                    "internal const string ClassSymbol = \"KMG.Classes.GunslingerClass\";") &&
                factory.Contains("return new GunslingerClassCatalogPublication(previous, previous);"),
                "The class symbol or idempotent catalog publication changed.");
            string bootstrap = Read("src", "KingmakerGunslinger", "Bootstrap",
                "BlueprintBootstrap.cs");
            Assertions.Equal(1, Regex.Matches(bootstrap,
                    @"GunslingerClassBlueprints\.Publish\(").Count,
                "Gunslinger must be published to the class catalog exactly once.");
            Assertions.True(bootstrap.IndexOf("GunslingerClassBlueprints.Register(",
                    StringComparison.Ordinal) <
                bootstrap.IndexOf("GunslingerClassBlueprints.Publish(", StringComparison.Ordinal),
                "Gunslinger must be complete before catalog publication.");

            string source = Path.Combine(Environment.CurrentDirectory, "src",
                "KingmakerGunslinger");
            string[] production = Directory.GetFiles(source, "*.cs", SearchOption.AllDirectories)
                .Where(path => path.IndexOf(Path.DirectorySeparatorChar + "RuntimeTesting" +
                    Path.DirectorySeparatorChar, StringComparison.Ordinal) < 0).ToArray();
            string[] catalogWriters = production.Where(path => Regex.IsMatch(
                    File.ReadAllText(path), @"Progression\.CharacterClasses\s*=(?!=)"))
                .Select(Path.GetFileName).ToArray();
            Assertions.True(catalogWriters.SequenceEqual(new[] { "GunslingerClassBlueprints.cs" }),
                "Only Gunslinger publication/rollback may write the class catalog: " +
                string.Join(", ", catalogWriters));
            // The level-up selector layer also hosts spell selection. Only its two
            // spell-selector view methods may be patched (the elemental learning
            // fold); every class display/application seam stays forbidden.
            const string spellSelectorPatch = @"HarmonyPatch\(typeof\(CharBSelectorLayer\),\s*" +
                @"""(SetupSpellSelectorView|FillSpellLightUpdate)""\)";
            string[] selectorPatches = production.Where(path => Regex.IsMatch(
                    Regex.Replace(File.ReadAllText(path), spellSelectorPatch, ""),
                    @"HarmonyPatch\(typeof\((SelectClass|CharBSelectorLayer|CharBPhaseClass|" +
                    @"CharGenClassPhaseVM)\)|""MeetsPrerequisites""|" +
                    @"nameof\(BlueprintCharacterClass\.MeetsPrerequisites\)"))
                .Select(Path.GetFileName).ToArray();
            Assertions.Equal(0, selectorPatches.Length,
                "Class display and application must stay on Kingmaker's one native predicate: " +
                string.Join(", ", selectorPatches));
        }

        private static string Between(string text, string start, string end)
        {
            int from = text.IndexOf(start, StringComparison.Ordinal);
            int to = from < 0 ? -1 : text.IndexOf(end, from, StringComparison.Ordinal);
            Assertions.True(from >= 0 && to > from, "Source section is missing: " + start);
            return text.Substring(from, to - from);
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            for (int index = 0; index < parts.Length; index++)
                path = Path.Combine(path, parts[index]);
            return File.ReadAllText(path);
        }
    }
}
