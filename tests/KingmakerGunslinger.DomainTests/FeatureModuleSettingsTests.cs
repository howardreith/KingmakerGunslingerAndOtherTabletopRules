using System;
using System.IO;
using KingmakerGunslinger.FeatureModules;
using Newtonsoft.Json;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FeatureModuleSettingsTests
    {
        internal static void DefaultsAndLegacyAreOn()
        {
            WithDirectory(path =>
            {
                FeatureModuleSettingsState missing = FeatureModuleSettingsStore.Load(path);
                Assertions.True(missing.Active.Gunslinger && missing.Active.AcadamaeGraduate &&
                    missing.Active.ShieldOther && missing.Active.ExpandedSummoning &&
                    missing.Active.ElvenBranchedSpears && missing.Active.EasternWeapons &&
                    missing.Active.BrownFurTransmuter && missing.Active.UrbanBarbarian,
                    "Missing settings must default all eight modules ON.");
                File.WriteAllText(Path.Combine(path, FeatureModuleSettingsStore.FileName), "{}");
                FeatureModuleSettingsState legacy = FeatureModuleSettingsStore.Load(path);
                Assertions.True(legacy.Active.Gunslinger && legacy.Active.AcadamaeGraduate &&
                    legacy.Active.ShieldOther && legacy.Active.ExpandedSummoning &&
                    legacy.Active.ElvenBranchedSpears && legacy.Active.EasternWeapons &&
                    legacy.Active.BrownFurTransmuter && legacy.Active.UrbanBarbarian,
                    "Legacy settings must default all eight modules ON.");
                File.WriteAllText(Path.Combine(path, FeatureModuleSettingsStore.FileName),
                    "{\"schemaVersion\":1,\"gunslinger\":false,\"acadamae-graduate\":true}");
                FeatureModuleSettingsState migrated = FeatureModuleSettingsStore.Load(path);
                string migratedJson = File.ReadAllText(Path.Combine(path,
                    FeatureModuleSettingsStore.FileName));
                Assertions.True(!migrated.Active.Gunslinger &&
                    migrated.Active.AcadamaeGraduate && migrated.Active.ShieldOther &&
                    migrated.Active.ExpandedSummoning &&
                    migrated.Active.ElvenBranchedSpears &&
                    migrated.Active.EasternWeapons && migrated.Active.BrownFurTransmuter &&
                    migrated.Active.UrbanBarbarian &&
                    migratedJson.Contains("\"schemaVersion\": 7") &&
                    migratedJson.Contains("\"shield-other\": true") &&
                    migratedJson.Contains("\"expanded-summoning\": true") &&
                    migratedJson.Contains("\"elven-branched-spears\": true") &&
                    migratedJson.Contains("\"eastern-weapons\": true") &&
                    migratedJson.Contains("\"brown-fur-transmuter\": true") &&
                    migratedJson.Contains("\"urban-barbarian\": true"),
                    "Schema 1 must migrate atomically to schema 7 with newer modules ON.");
                File.WriteAllText(Path.Combine(path, FeatureModuleSettingsStore.FileName),
                    "{\"schemaVersion\":2,\"gunslinger\":true," +
                    "\"acadamae-graduate\":false,\"shield-other\":false}");
                FeatureModuleSettingsState schemaTwo =
                    FeatureModuleSettingsStore.Load(path);
                Assertions.True(schemaTwo.Active.Gunslinger &&
                    !schemaTwo.Active.AcadamaeGraduate &&
                    !schemaTwo.Active.ShieldOther &&
                    schemaTwo.Active.ExpandedSummoning &&
                    schemaTwo.Active.ElvenBranchedSpears &&
                    schemaTwo.Active.EasternWeapons && schemaTwo.Active.BrownFurTransmuter &&
                    schemaTwo.Active.UrbanBarbarian,
                    "Schema 2 must preserve explicit values and add newer modules ON.");
                File.WriteAllText(Path.Combine(path, FeatureModuleSettingsStore.FileName),
                    "{\"schemaVersion\":5,\"gunslinger\":false," +
                    "\"acadamae-graduate\":true,\"shield-other\":false," +
                    "\"expanded-summoning\":true,\"elven-branched-spears\":false," +
                    "\"eastern-weapons\":true}");
                FeatureModuleSettingsState schemaFive =
                    FeatureModuleSettingsStore.Load(path);
                Assertions.True(!schemaFive.Active.Gunslinger &&
                    schemaFive.Active.AcadamaeGraduate && !schemaFive.Active.ShieldOther &&
                    schemaFive.Active.ExpandedSummoning &&
                    !schemaFive.Active.ElvenBranchedSpears &&
                    schemaFive.Active.EasternWeapons && schemaFive.Active.BrownFurTransmuter &&
                    schemaFive.Active.UrbanBarbarian,
                    "Schema 5 must retain every prior value and add Brown-Fur ON.");
                File.WriteAllText(Path.Combine(path, FeatureModuleSettingsStore.FileName),
                    "{\"schemaVersion\":6,\"gunslinger\":false," +
                    "\"brown-fur-transmuter\":false}");
                FeatureModuleSettingsState schemaSix =
                    FeatureModuleSettingsStore.Load(path);
                Assertions.True(!schemaSix.Active.Gunslinger &&
                    !schemaSix.Active.BrownFurTransmuter &&
                    schemaSix.Active.UrbanBarbarian,
                    "Schema 6 must preserve prior values and add Urban Barbarian ON.");
            });
        }

        internal static void TwoHundredFiftySixCombinationsRoundTrip()
        {
            WithDirectory(path =>
            {
                foreach (bool gunslinger in new[] { false, true })
                foreach (bool acadamae in new[] { false, true })
                foreach (bool shieldOther in new[] { false, true })
                foreach (bool expandedSummoning in new[] { false, true })
                foreach (bool elvenBranchedSpears in new[] { false, true })
                foreach (bool easternWeapons in new[] { false, true })
                foreach (bool brownFurTransmuter in new[] { false, true })
                foreach (bool urbanBarbarian in new[] { false, true })
                {
                    FeatureModuleSettingsState state = FeatureModuleSettingsStore.Load(path);
                    state.SetPending(gunslinger, acadamae, shieldOther, expandedSummoning,
                        elvenBranchedSpears, easternWeapons, brownFurTransmuter,
                        urbanBarbarian);
                    FeatureModuleSettingsStore.Save(state);
                    FeatureModuleSettingsState loaded = FeatureModuleSettingsStore.Load(path);
                    Assertions.True(loaded.Active.Gunslinger == gunslinger &&
                        loaded.Active.AcadamaeGraduate == acadamae &&
                        loaded.Active.ShieldOther == shieldOther &&
                        loaded.Active.ExpandedSummoning == expandedSummoning &&
                        loaded.Active.ElvenBranchedSpears == elvenBranchedSpears &&
                        loaded.Active.EasternWeapons == easternWeapons &&
                        loaded.Active.BrownFurTransmuter == brownFurTransmuter &&
                        loaded.Active.UrbanBarbarian == urbanBarbarian,
                        "Module combination did not round-trip.");
                }
            });
        }

        internal static void MalformedRecoversAndQuarantines()
        {
            WithDirectory(path =>
            {
                string settings = Path.Combine(path, FeatureModuleSettingsStore.FileName);
                File.WriteAllText(settings, "{broken");
                string warning = null;
                DateTime instant = new DateTime(2026, 8, 9, 12, 0, 0, DateTimeKind.Utc);
                FeatureModuleSettingsState state = FeatureModuleSettingsStore.Load(
                    path, value => warning = value, () => instant);
                Assertions.True(state.Recovered && state.Active.Gunslinger &&
                    state.Active.AcadamaeGraduate && state.Active.ShieldOther &&
                    state.Active.ExpandedSummoning &&
                    state.Active.ElvenBranchedSpears && state.Active.EasternWeapons &&
                    state.Active.BrownFurTransmuter && state.Active.UrbanBarbarian,
                    "Malformed settings did not recover all eight modules ON.");
                Assertions.True(warning != null && Directory.GetFiles(path,
                    "FeatureModules.json.malformed.*").Length == 1,
                    "Malformed bytes were not quarantined with a diagnostic.");
                Assertions.Equal("{broken", File.ReadAllText(settings),
                    "Malformed source bytes changed during load.");
            });
        }

        internal static void FutureSchemaIsRejectedAndSerializationIsOrdered()
        {
            WithDirectory(path =>
            {
                string settings = Path.Combine(path, FeatureModuleSettingsStore.FileName);
                File.WriteAllText(settings, "{\"schemaVersion\":8}");
                bool rejected = false;
                try { FeatureModuleSettingsStore.Load(path); }
                catch (JsonException) { rejected = true; }
                Assertions.True(rejected,
                    "A future feature-module schema must fail closed.");

                File.Delete(settings);
                FeatureModuleSettingsState state = FeatureModuleSettingsStore.Load(path);
                state.SetPending(false, true, false, true, false, true, false, true);
                FeatureModuleSettingsStore.Save(state);
                string json = File.ReadAllText(settings);
                string[] keys = { "\"schemaVersion\"", "\"gunslinger\"",
                    "\"acadamae-graduate\"", "\"shield-other\"",
                    "\"expanded-summoning\"", "\"elven-branched-spears\"",
                    "\"eastern-weapons\"", "\"brown-fur-transmuter\"",
                    "\"urban-barbarian\"" };
                int prior = -1;
                foreach (string key in keys)
                {
                    int current = json.IndexOf(key, StringComparison.Ordinal);
                    Assertions.True(current > prior,
                        "Feature-module serialization order changed at " + key + ".");
                    prior = current;
                }
            });
        }

        internal static void ActiveSnapshotIsImmutable()
        {
            var state = new FeatureModuleSettingsState(
                FeatureModuleConfiguration.Defaults, "fixture", "fixture", false);
            state.SetPending(false, true, false, false, false, false, false, false);
            Assertions.True(state.Active.Gunslinger && !state.Pending.Gunslinger &&
                state.RestartRequired, "UI edits must not mutate the active snapshot.");
            Assertions.Equal("gunslinger", FeatureModuleConfiguration.GunslingerId,
                "Gunslinger module ID changed.");
            Assertions.Equal("acadamae-graduate",
                FeatureModuleConfiguration.AcadamaeGraduateId,
                "Acadamae module ID changed.");
            Assertions.Equal("shield-other", FeatureModuleConfiguration.ShieldOtherId,
                "Shield Other module ID changed.");
            Assertions.True(state.Active.ShieldOther && !state.Pending.ShieldOther,
                "Shield Other pending edits mutated the active snapshot.");
            Assertions.Equal("expanded-summoning",
                FeatureModuleConfiguration.ExpandedSummoningId,
                "Expanded Summoning module ID changed.");
            Assertions.True(state.Active.ExpandedSummoning &&
                !state.Pending.ExpandedSummoning,
                "Expanded Summoning pending edits mutated the active snapshot.");
            Assertions.Equal("elven-branched-spears",
                FeatureModuleConfiguration.ElvenBranchedSpearsId,
                "Elven Branched Spears module ID changed.");
            Assertions.True(state.Active.ElvenBranchedSpears &&
                !state.Pending.ElvenBranchedSpears,
                "Elven Branched Spears pending edits mutated the active snapshot.");
            Assertions.Equal("eastern-weapons",
                FeatureModuleConfiguration.EasternWeaponsId,
                "Eastern Weapons module ID changed.");
            Assertions.True(state.Active.EasternWeapons &&
                !state.Pending.EasternWeapons,
                "Eastern Weapons pending edits mutated the active snapshot.");
            Assertions.Equal("brown-fur-transmuter",
                FeatureModuleConfiguration.BrownFurTransmuterId,
                "Brown-Fur module ID changed.");
            Assertions.True(state.Active.BrownFurTransmuter &&
                !state.Pending.BrownFurTransmuter,
                "Brown-Fur pending edits mutated the active snapshot.");
            Assertions.Equal("urban-barbarian",
                FeatureModuleConfiguration.UrbanBarbarianId,
                "Urban Barbarian module ID changed.");
            Assertions.True(state.Active.UrbanBarbarian &&
                !state.Pending.UrbanBarbarian,
                "Urban Barbarian pending edits mutated the active snapshot.");
        }

        internal static void ValueSemanticsIncludeAllModules()
        {
            var enabled = new FeatureModuleConfiguration(true, true, true, true,
                true, true, true, true);
            var same = new FeatureModuleConfiguration(true, true, true, true,
                true, true, true, true);
            var brownFurOff = new FeatureModuleConfiguration(true, true, true, true,
                true, true, false, true);
            var urbanOff = new FeatureModuleConfiguration(true, true, true, true,
                true, true, true, false);
            Assertions.True(enabled.Equals(same) && enabled.GetHashCode() ==
                same.GetHashCode(), "Equal eight-module values disagree.");
            Assertions.True(!enabled.Equals(brownFurOff) && enabled.GetHashCode() !=
                brownFurOff.GetHashCode(), "Brown-Fur is absent from value semantics.");
            Assertions.True(enabled.ToString().Contains("brown-fur-transmuter=True") &&
                brownFurOff.ToString().Contains("brown-fur-transmuter=False"),
                "Brown-Fur is absent from configuration formatting.");
            Assertions.True(!enabled.Equals(urbanOff) && enabled.GetHashCode() !=
                urbanOff.GetHashCode() &&
                enabled.ToString().Contains("urban-barbarian=True") &&
                urbanOff.ToString().Contains("urban-barbarian=False"),
                "Urban Barbarian is absent from value semantics or formatting.");
        }

        internal static void BrownFurStatusDistinguishesIntentAndDependency()
        {
            var unavailable = new BrownFurFeatureStatus(
                BrownFurDependencyAvailability.Unavailable, false, "missing");
            var available = new BrownFurFeatureStatus(
                BrownFurDependencyAvailability.Available, true, "compatible");
            var blocked = new BrownFurFeatureStatus(
                BrownFurDependencyAvailability.Blocked, false, "contract");
            Assertions.Equal("Unavailable  Call of the Wild not detected",
                unavailable.DependencyStatus, "Unavailable status changed.");
            Assertions.Equal("Available  compatible Call of the Wild detected",
                available.DependencyStatus, "Available status changed.");
            Assertions.Equal("Blocked  installed Call of the Wild is incompatible",
                blocked.DependencyStatus, "Blocked status changed.");
            Assertions.True(!unavailable.Published && available.Published &&
                !blocked.Published,
                "Dependency availability is not distinct from publication state.");
            string ui = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "FeatureModules", "FeatureModuleUi.cs"));
            Assertions.True(ui.Contains(
                "Brown-Fur Transmuter  requires Call of the Wild") &&
                ui.Contains("Active this process:") &&
                ui.Contains("Saved for next restart:") &&
                ui.Contains("RESTART REQUIRED") &&
                ui.Contains("effective current-process state") &&
                ui.Contains("Urban Barbarian core:") &&
                ui.Contains("Urban Barbarian optional CotW interoperability:") &&
                ui.Contains("Urban Barbarian CotW detail:") &&
                !ui.Contains("Urban Barbarian  requires Call of the Wild"),
                "Brown-Fur UMM state presentation is incomplete.");
        }

        internal static void EightModuleMatrixCountsAreExact()
        {
            Assertions.Equal(256, FeatureModuleMatrixPolicy.ExhaustiveCount(8),
                "Eight-module exhaustive count changed.");
            Assertions.Equal(18, FeatureModuleMatrixPolicy.BoundaryCount(8),
                "Eight-module boundary count changed.");
            int observedBoundary = 0;
            for (int mask = 0; mask < FeatureModuleMatrixPolicy.ExhaustiveCount(8);
                mask++)
            {
                int enabled = 0;
                for (int bit = 0; bit < 8; bit++)
                    if ((mask & (1 << bit)) != 0) enabled++;
                if (FeatureModuleMatrixPolicy.IsBoundaryState(8, enabled))
                    observedBoundary++;
            }
            Assertions.Equal(FeatureModuleMatrixPolicy.BoundaryCount(8),
                observedBoundary, "Generated boundary states are not 2 + 2N.");
        }

        internal static void PublicationPlansAreIndependent()
        {
            foreach (bool gunslinger in new[] { false, true })
            foreach (bool acadamae in new[] { false, true })
            foreach (bool shieldOther in new[] { false, true })
            foreach (bool expandedSummoning in new[] { false, true })
            foreach (bool elvenBranchedSpears in new[] { false, true })
            foreach (bool easternWeapons in new[] { false, true })
            foreach (bool brownFurTransmuter in new[] { false, true })
            foreach (bool urbanBarbarian in new[] { false, true })
            {
                var plan = new FeatureModulePublicationPlan(
                    new FeatureModuleConfiguration(gunslinger, acadamae, shieldOther,
                        expandedSummoning, elvenBranchedSpears, easternWeapons,
                        brownFurTransmuter, urbanBarbarian));
                Assertions.True(plan.GunslingerClass == gunslinger &&
                    plan.GunslingerFeats == gunslinger &&
                    plan.FirearmParameters == gunslinger &&
                    plan.CapitalGunslingerStock == gunslinger &&
                    plan.BeneathStolenLandsStock == gunslinger &&
                    plan.RareFirearmLoot == gunslinger,
                    "A Gunslinger publication surface escaped its module gate.");
                Assertions.True(plan.AcadamaeFeat == acadamae &&
                    plan.CordCampaignLoot == acadamae,
                    "Acadamae and Cord publication gates are not independent.");
                Assertions.True(plan.ShieldOtherSpellLists == shieldOther,
                    "Shield Other spell-list publication escaped its independent gate.");
                Assertions.True(plan.ExpandedSummoningParents == expandedSummoning,
                    "Expanded Summoning parent publication escaped its independent gate.");
                Assertions.True(plan.ElvenBranchedSpearSelectors == elvenBranchedSpears &&
                    plan.ElvenBranchedSpearCommerce == elvenBranchedSpears &&
                    plan.ElvenBranchedSpearPresentation == elvenBranchedSpears,
                    "Elven Branched Spears publication surfaces escaped their independent gate.");
                Assertions.True(plan.EasternWeaponSelectors == easternWeapons &&
                    plan.EasternWeaponCommerce == easternWeapons &&
                    plan.EasternWeaponPresentation == easternWeapons,
                    "Eastern Weapons publication surfaces escaped their independent gate.");
                Assertions.True(plan.BrownFurPublicationRequested == brownFurTransmuter,
                    "Brown-Fur publication intent escaped its independent gate.");
                Assertions.True(plan.UrbanBarbarianArchetype == urbanBarbarian,
                    "Urban Barbarian publication escaped its independent gate.");
            }
        }

        internal static void RuntimeMatrixUsesAuthoritativeEightModuleCatalog()
        {
            string root = Environment.CurrentDirectory;
            string matrix = File.ReadAllText(Path.Combine(root, "scripts",
                "Invoke-FeatureModuleRuntimeMatrix.ps1"));
            string catalog = File.ReadAllText(Path.Combine(root, "scripts",
                "FeatureModuleCatalog.ps1"));
            string matrixContract = matrix + Environment.NewLine + catalog;
            foreach (string token in new[] {
                "FeatureModuleCatalog.ps1", "[switch]$Boundary",
                "Get-KmgFeatureModuleConfigurations", "schemaVersion = 7",
                "BrownFurTransmuter", "brown-fur-transmuter",
                "brownFurTransmuter", "UrbanBarbarian", "urban-barbarian",
                "urbanBarbarian", "2 + 2 * $moduleCount",
                "$boundaryRequested = $Combination -ceq 'all'",
                "Get-KmgFeatureModuleConfigurations -Boundary",
                "deliberately has no generic 2^N game-launch mode",
                "[switch]$AllowDirtyGit", "AllowDirtyGit = [bool]$AllowDirtyGit",
                "Settings byte-for-byte restoration failed." })
                Assertions.True(matrixContract.Contains(token),
                    "The authoritative boundary runtime matrix contract is missing: " + token);
            string common = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(common.Contains("$Parameters.Count -ne 8") &&
                common.Contains("expandedSummoning = [bool]$Parameters.expandedSummoning") &&
                common.Contains("elvenBranchedSpears = [bool]$Parameters.elvenBranchedSpears") &&
                common.Contains("easternWeapons = [bool]$Parameters.easternWeapons") &&
                common.Contains("brownFurTransmuter = [bool]$Parameters.brownFurTransmuter") &&
                common.Contains("urbanBarbarian = [bool]$Parameters.urbanBarbarian"),
                "The guarded request writer does not require all eight module states.");
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            foreach (string token in new[] {
                "spearRegisteredItems == 12",
                "spearParameterizedOptions ==",
                "(expectedElvenBranchedSpears ? 7 : 0)",
                "spearStaticOptions ==",
                "(expectedElvenBranchedSpears ? 3 : 0)",
                "spearFamiliarityCategories == 1",
                "22 + installedSpearHonestGuyTables * 6 : 0",
                "(expectedElvenBranchedSpears ? 6 : 0)",
                "always-registered identities and exact selector, familiarity, vendor, and fixed-loot surfaces" })
                Assertions.True(runner.Contains(token),
                    "The live six-module observer lacks the spear assertion: " +
                    token);
            string request = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRequest.cs"));
            Assertions.True(request.Contains(
                "request.Parameters.Count != 8") && request.Contains(
                    "Property(\"elvenBranchedSpears\")") && request.Contains(
                    "request.Parameters[\"elvenBranchedSpears\"]") && request.Contains(
                    "request.Parameters[\"easternWeapons\"]") && request.Contains(
                    "request.Parameters[\"brownFurTransmuter\"]") && request.Contains(
                    "request.Parameters[\"urbanBarbarian\"]"),
                "The in-mod request validator does not require all eight module states.");
        }

        private static void WithDirectory(Action<string> action)
        {
            string path = Path.Combine(Path.GetTempPath(),
                "kmg-feature-settings-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            try { action(path); }
            finally { Directory.Delete(path, true); }
        }
    }
}
