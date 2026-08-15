using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityModManagerNet;

namespace KingmakerGunslinger.BrownFur
{
    internal static class CotwArcanistResolver
    {
        private const string CotwAssemblyName = "CallOfTheWild";
        private const string ArcanistTypeName = "CallOfTheWild.Arcanist";
        private const string ClassGuid = "19c3cf3d51cf4cbf9a136a600c26585a";
        private const string ProgressionGuid = "2d28526efc2e4a9cb6a84c85267fb344";
        private const string CastingSpellbookGuid = "0c21cfcab6ce4395bd4df330ab3cf715";
        private const string MemorizationSpellbookGuid = "ab76417567444a6cb87d9d53e9752955";
        private const string ReservoirGuid = "3b775ee982444493b3de8f7bc31bd872";
        private const string MagicalSupremacyGuid = "2d86a417ab1542f98a8444b2b97d4951";

        internal static CotwArcanistResolution Resolve(
            UnityModManager.ModEntry cotwEntry)
        {
            if (cotwEntry == null)
                return new CotwArcanistResolution(
                    CotwArcanistContractPolicy.Evaluate(null), null);
            try
            {
                Assembly assembly = cotwEntry.Assembly;
                Type arcanist = assembly == null ? null : assembly.GetType(
                    ArcanistTypeName, false, false);
                BlueprintCharacterClass cls = Field<BlueprintCharacterClass>(
                    arcanist, "arcanist_class");
                BlueprintProgression progression = Field<BlueprintProgression>(
                    arcanist, "arcanist_progression");
                BlueprintSpellbook casting = Field<BlueprintSpellbook>(
                    arcanist, "arcanist_spellbook");
                BlueprintSpellbook memorization = Field<BlueprintSpellbook>(
                    arcanist, "memorization_spellbook");
                BlueprintAbilityResource reservoir = Field<BlueprintAbilityResource>(
                    arcanist, "arcane_reservoir_resource");
                BlueprintFeatureSelection exploits = Field<BlueprintFeatureSelection>(
                    arcanist, "arcane_exploits");
                BlueprintFeature magical = Field<BlueprintFeature>(
                    arcanist, "magical_supremacy");
                CotwSharedSpellsBridge shared;
                bool sharedResolved = CotwSharedSpellsBridge.TryResolve(
                    assembly, out shared);

                int[] exploitLevels = ResolveExploitLevels(progression, exploits);
                BlueprintAbility[] transmutations = ResolveTransmutations(casting);
                var candidate = new CotwArcanistContractCandidate
                {
                    CotwDetected = true,
                    CotwActive = cotwEntry.Loaded && cotwEntry.Active &&
                        cotwEntry.HasAssembly && !cotwEntry.ErrorOnLoading,
                    AssemblyIdentityResolved = assembly != null &&
                        string.Equals(assembly.GetName().Name, CotwAssemblyName,
                            StringComparison.Ordinal),
                    ArcanistClassResolved = Exact(cls, ClassGuid),
                    ArcanistProgressionResolved = Exact(progression,
                        ProgressionGuid) && cls != null &&
                        ReferenceEquals(cls.Progression, progression),
                    CastingSpellbookResolved = Exact(casting,
                        CastingSpellbookGuid) && cls != null &&
                        ReferenceEquals(casting.CharacterClass, cls) &&
                        casting.SpellList != null,
                    MemorizationSpellbookResolved = Exact(memorization,
                        MemorizationSpellbookGuid) && cls != null &&
                        ReferenceEquals(cls.Spellbook, memorization) &&
                        ReferenceEquals(memorization.CharacterClass, cls) &&
                        casting != null && ReferenceEquals(
                            memorization.SpellList, casting.SpellList),
                    ReservoirResolved = Exact(reservoir, ReservoirGuid),
                    ExploitSelectionResolved = exploits != null &&
                        exploitLevels.Length > 0,
                    MagicalSupremacyResolved = Exact(magical,
                        MagicalSupremacyGuid) && ContainsAtLevel(
                            progression, magical, 20),
                    SharedSpellsContractResolved = sharedResolved,
                    ArchetypeArrayResolved = cls != null &&
                        cls.Archetypes != null,
                    TransmutationInventoryResolved = transmutations.Length > 0,
                    ExploitBearingLevels = exploitLevels
                };
                CotwArcanistContractDecision decision =
                    CotwArcanistContractPolicy.Evaluate(candidate);
                CotwCompatibilityFingerprint fingerprint = Fingerprint(cotwEntry,
                    assembly, cls, progression, casting, memorization, reservoir,
                    exploits, magical, exploitLevels, shared, transmutations,
                    decision);
                if (!decision.IsCompatible)
                {
                    return new CotwArcanistResolution(decision,
                        new CotwArcanistContract { Assembly = assembly,
                            Fingerprint = fingerprint });
                }
                return new CotwArcanistResolution(decision,
                    new CotwArcanistContract
                    {
                        Assembly = assembly,
                        ArcanistClass = cls,
                        ArcanistProgression = progression,
                        CastingSpellbook = casting,
                        MemorizationSpellbook = memorization,
                        Reservoir = reservoir,
                        ExploitSelection = exploits,
                        MagicalSupremacy = magical,
                        SharedSpells = shared,
                        ProgressionDecision = decision.Progression,
                        Fingerprint = fingerprint
                    });
            }
            catch (Exception exception)
            {
                return new CotwArcanistResolution(
                    new CotwArcanistContractDecision(
                        CotwContractAvailability.Incompatible,
                        CotwProgressionDecision.Reject("resolver exception"),
                        "resolver-exception:" + exception.GetType().FullName +
                        ":" + exception.Message), null);
            }
        }

        internal static bool HasConstructedArcanist(Assembly assembly)
        {
            Type arcanist = assembly == null ? null : assembly.GetType(
                ArcanistTypeName, false, false);
            return Field<BlueprintCharacterClass>(arcanist,
                "arcanist_class") != null;
        }

        private static T Field<T>(Type type, string name) where T : class
        {
            FieldInfo field = type == null ? null : type.GetField(name,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null || !typeof(T).IsAssignableFrom(field.FieldType))
                return null;
            return field.GetValue(null) as T;
        }

        private static int[] ResolveExploitLevels(BlueprintProgression progression,
            BlueprintFeatureSelection exploits)
        {
            if (progression == null || progression.LevelEntries == null ||
                exploits == null) return new int[0];
            return progression.LevelEntries.Where(value => value != null &&
                value.Features != null && value.Features.Any(feature =>
                    ReferenceEquals(feature, exploits))).Select(value =>
                        value.Level).ToArray();
        }

        private static BlueprintAbility[] ResolveTransmutations(
            BlueprintSpellbook spellbook)
        {
            if (spellbook == null || spellbook.SpellList == null ||
                spellbook.SpellList.SpellsByLevel == null)
                return new BlueprintAbility[0];
            return spellbook.SpellList.SpellsByLevel.Where(value => value != null &&
                value.Spells != null).SelectMany(value => value.Spells)
                .Where(value => value != null && value.Type == AbilityType.Spell &&
                    value.School == SpellSchool.Transmutation)
                .GroupBy(value => value.AssetGuid, StringComparer.Ordinal)
                .Select(value => value.First()).OrderBy(value => value.AssetGuid,
                    StringComparer.Ordinal).ToArray();
        }

        private static bool ContainsAtLevel(BlueprintProgression progression,
            BlueprintFeature feature, int level)
        {
            return progression != null && progression.LevelEntries != null &&
                progression.LevelEntries.Any(value => value != null &&
                    value.Level == level && value.Features != null &&
                    value.Features.Any(item => ReferenceEquals(item, feature)));
        }

        private static bool Exact(Kingmaker.Blueprints.BlueprintScriptableObject value,
            string guid)
        {
            return value != null && string.Equals(value.AssetGuid, guid,
                StringComparison.Ordinal);
        }

        private static CotwCompatibilityFingerprint Fingerprint(
            UnityModManager.ModEntry entry, Assembly assembly,
            BlueprintCharacterClass cls, BlueprintProgression progression,
            BlueprintSpellbook casting, BlueprintSpellbook memorization,
            BlueprintAbilityResource reservoir, BlueprintFeatureSelection exploits,
            BlueprintFeature magical, int[] levels, CotwSharedSpellsBridge shared,
            BlueprintAbility[] transmutations,
            CotwArcanistContractDecision decision)
        {
            string assemblyPath = assembly == null ? null : assembly.Location;
            string settingsPath = entry == null || string.IsNullOrWhiteSpace(entry.Path)
                ? null : Path.Combine(entry.Path, "settings.json");
            return new CotwCompatibilityFingerprint
            {
                AssemblyFullName = assembly == null ? null : assembly.FullName,
                FileVersion = FileVersion(assemblyPath),
                DllSha256 = Hash(assemblyPath),
                DllMvid = assembly == null ? null : assembly.ManifestModule
                    .ModuleVersionId.ToString("D"),
                ModVersion = entry == null || entry.Info == null ? null :
                    entry.Info.Version,
                SettingsSha256 = Hash(settingsPath),
                BalanceFixesSetting = ReadBalanceFixes(assembly),
                ArcanistClassGuid = Guid(cls),
                ProgressionGuid = Guid(progression),
                CastingSpellbookGuid = Guid(casting),
                MemorizationSpellbookGuid = Guid(memorization),
                ReservoirGuid = Guid(reservoir),
                ExploitSelectionGuid = Guid(exploits),
                MagicalSupremacyGuid = Guid(magical),
                ExploitLevels = levels,
                SharedSpellsSignatures = shared == null ? new string[0] :
                    shared.Signatures,
                TransmutationSpellCount = transmutations.Length,
                PersonalTransmutationSpellCount = transmutations.Count(value =>
                    value.Range == AbilityRange.Personal),
                AbilityBonusTransmutationSpellCount = 0,
                SupportedComponentPatternCount = 0,
                UnsupportedComponentPatternCount = 0,
                PublicationStatus = decision.IsCompatible ?
                    "compatible-not-yet-published" : "blocked:" +
                    decision.FailedCheck
            };
        }

        private static string ReadBalanceFixes(Assembly assembly)
        {
            Type main = assembly == null ? null : assembly.GetType(
                "CallOfTheWild.Main", false, false);
            FieldInfo settingsField = main == null ? null : main.GetField(
                "settings", BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic);
            object settings = settingsField == null ? null : settingsField.GetValue(null);
            if (settings == null) return "<unavailable>";
            FieldInfo field = settings.GetType().GetField("balance_fixes",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return field == null || field.FieldType != typeof(bool) ?
                "<unavailable>" : ((bool)field.GetValue(settings)).ToString();
        }

        private static string Guid(Kingmaker.Blueprints.BlueprintScriptableObject value)
        { return value == null ? null : value.AssetGuid; }

        private static string FileVersion(string path)
        {
            return string.IsNullOrWhiteSpace(path) || !File.Exists(path) ? null :
                FileVersionInfo.GetVersionInfo(path).FileVersion;
        }

        private static string Hash(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }
    }
}
