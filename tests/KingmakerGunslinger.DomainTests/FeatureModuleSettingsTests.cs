using System;
using System.IO;
using KingmakerGunslinger.FeatureModules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    missing.Active.BrownFurTransmuter && missing.Active.UrbanBarbarian &&
                    missing.Active.BodyguardFeats && missing.Active
                        .ProtectionFromAlignmentControlImmunity &&
                    missing.Active.ElementalRaces && missing.Active.TeleportationSpells,
                    "Missing settings must default all twelve modules ON.");
                File.WriteAllText(Path.Combine(path, FeatureModuleSettingsStore.FileName), "{}");
                FeatureModuleSettingsState legacy = FeatureModuleSettingsStore.Load(path);
                Assertions.True(legacy.Active.Gunslinger && legacy.Active.AcadamaeGraduate &&
                    legacy.Active.ShieldOther && legacy.Active.ExpandedSummoning &&
                    legacy.Active.ElvenBranchedSpears && legacy.Active.EasternWeapons &&
                    legacy.Active.BrownFurTransmuter && legacy.Active.UrbanBarbarian &&
                    legacy.Active.BodyguardFeats && legacy.Active
                        .ProtectionFromAlignmentControlImmunity &&
                    legacy.Active.ElementalRaces && legacy.Active.TeleportationSpells,
                    "Schema 0 settings must default absent modules ON.");
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
                    migrated.Active.UrbanBarbarian && migrated.Active.BodyguardFeats &&
                    migrated.Active.ProtectionFromAlignmentControlImmunity &&
                    migrated.Active.ElementalRaces &&
                    migratedJson.Contains("\"schemaVersion\": 12") &&
                    migratedJson.Contains("\"shield-other\": true") &&
                    migratedJson.Contains("\"expanded-summoning\": true") &&
                    migratedJson.Contains("\"elven-branched-spears\": true") &&
                    migratedJson.Contains("\"eastern-weapons\": true") &&
                    migratedJson.Contains("\"brown-fur-transmuter\": true") &&
                    migratedJson.Contains("\"urban-barbarian\": true") &&
                    migratedJson.Contains("\"bodyguard-feats\": true") &&
                    migratedJson.Contains(
                        "\"protection-from-alignment-control-immunity\": true") &&
                    migratedJson.Contains("\"elemental-races\": true"),
                    "Schema 1 must migrate atomically to schema 12 with Elemental Races ON.");
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
                    schemaTwo.Active.UrbanBarbarian && schemaTwo.Active.BodyguardFeats &&
                    schemaTwo.Active.ProtectionFromAlignmentControlImmunity,
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
                    schemaFive.Active.UrbanBarbarian && schemaFive.Active.BodyguardFeats &&
                    schemaFive.Active.ProtectionFromAlignmentControlImmunity,
                    "Schema 5 must retain every prior value and add Brown-Fur ON.");
                File.WriteAllText(Path.Combine(path, FeatureModuleSettingsStore.FileName),
                    "{\"schemaVersion\":6,\"gunslinger\":false," +
                    "\"brown-fur-transmuter\":false}");
                FeatureModuleSettingsState schemaSix =
                    FeatureModuleSettingsStore.Load(path);
                Assertions.True(!schemaSix.Active.Gunslinger &&
                    !schemaSix.Active.BrownFurTransmuter &&
                    schemaSix.Active.UrbanBarbarian && schemaSix.Active.BodyguardFeats &&
                    schemaSix.Active.ProtectionFromAlignmentControlImmunity,
                    "Schema 6 must preserve prior values and add Urban Barbarian ON.");
                File.WriteAllText(Path.Combine(path, FeatureModuleSettingsStore.FileName),
                    "{\"schemaVersion\":7,\"gunslinger\":false," +
                    "\"acadamae-graduate\":true,\"shield-other\":false," +
                    "\"expanded-summoning\":true,\"elven-branched-spears\":false," +
                    "\"eastern-weapons\":true,\"brown-fur-transmuter\":false," +
                    "\"urban-barbarian\":true}");
                FeatureModuleSettingsState schemaSeven =
                    FeatureModuleSettingsStore.Load(path);
                Assertions.True(!schemaSeven.Active.Gunslinger &&
                    schemaSeven.Active.AcadamaeGraduate &&
                    !schemaSeven.Active.ShieldOther &&
                    schemaSeven.Active.ExpandedSummoning &&
                    !schemaSeven.Active.ElvenBranchedSpears &&
                    schemaSeven.Active.EasternWeapons &&
                    !schemaSeven.Active.BrownFurTransmuter &&
                    schemaSeven.Active.UrbanBarbarian &&
                    schemaSeven.Active.BodyguardFeats && schemaSeven.Active
                        .ProtectionFromAlignmentControlImmunity,
                    "Schema 7 must preserve every prior explicit value and default Bodyguard ON.");
                File.WriteAllText(Path.Combine(path, FeatureModuleSettingsStore.FileName),
                    "{\"schemaVersion\":7,\"bodyguard-feats\":false}");
                FeatureModuleSettingsState explicitBodyguard =
                    FeatureModuleSettingsStore.Load(path);
                Assertions.True(!explicitBodyguard.Active.BodyguardFeats &&
                    explicitBodyguard.Active.Gunslinger &&
                    explicitBodyguard.Active.UrbanBarbarian && explicitBodyguard
                        .Active.ProtectionFromAlignmentControlImmunity,
                    "An explicit schema-7 Bodyguard value must survive migration.");
            });
        }

        internal static void SchemasZeroThroughTenPreserveValuesAndElementalIntent()
        {
            WithDirectory(path =>
            {
                string settings = Path.Combine(path,
                    FeatureModuleSettingsStore.FileName);
                for (int schema = 0; schema <= 10; schema++)
                {
                    File.WriteAllText(settings, LegacySettings(schema, null)
                        .ToString(Formatting.None));
                    FeatureModuleSettingsState absent =
                        FeatureModuleSettingsStore.Load(path);
                    AssertLegacyFixture(absent.Active, true,
                        "schema " + schema + " absent elemental-races");
                    JObject migrated = JObject.Parse(
                        File.ReadAllText(settings));
                    Assertions.Equal(12, (int)migrated["schemaVersion"],
                        "Legacy settings did not migrate to schema 12.");
                    Assertions.True(migrated["elemental-races"].Type ==
                        JTokenType.Boolean &&
                        (bool)migrated["elemental-races"],
                        "Absent Elemental Races did not migrate ON.");

                    bool explicitElemental = schema % 2 == 0;
                    File.WriteAllText(settings, LegacySettings(schema,
                        explicitElemental).ToString(Formatting.None));
                    FeatureModuleSettingsState explicitState =
                        FeatureModuleSettingsStore.Load(path);
                    AssertLegacyFixture(explicitState.Active,
                        explicitElemental, "schema " + schema +
                        " explicit elemental-races");
                }
            });
        }

        internal static void EightThousandOneHundredNinetyTwoCombinationsRoundTrip()
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
                foreach (bool bodyguardFeats in new[] { false, true })
                foreach (bool protectionFromAlignmentControlImmunity in
                    new[] { false, true })
                foreach (bool elementalRaces in new[] { false, true })
                foreach (bool teleportationSpells in new[] { false, true })
                foreach (bool magicCircleSpells in new[] { false, true })
                {
                    FeatureModuleSettingsState state = FeatureModuleSettingsStore.Load(path);
                    state.SetPending(gunslinger, acadamae, shieldOther, expandedSummoning,
                        elvenBranchedSpears, easternWeapons, brownFurTransmuter,
                        urbanBarbarian, bodyguardFeats,
                        protectionFromAlignmentControlImmunity,
                        elementalRaces, teleportationSpells, magicCircleSpells);
                    FeatureModuleSettingsStore.Save(state);
                    FeatureModuleSettingsState loaded = FeatureModuleSettingsStore.Load(path);
                    Assertions.True(loaded.Active.Gunslinger == gunslinger &&
                        loaded.Active.AcadamaeGraduate == acadamae &&
                        loaded.Active.ShieldOther == shieldOther &&
                        loaded.Active.ExpandedSummoning == expandedSummoning &&
                        loaded.Active.ElvenBranchedSpears == elvenBranchedSpears &&
                        loaded.Active.EasternWeapons == easternWeapons &&
                        loaded.Active.BrownFurTransmuter == brownFurTransmuter &&
                        loaded.Active.UrbanBarbarian == urbanBarbarian &&
                        loaded.Active.BodyguardFeats == bodyguardFeats &&
                        loaded.Active.ProtectionFromAlignmentControlImmunity ==
                            protectionFromAlignmentControlImmunity &&
                        loaded.Active.ElementalRaces == elementalRaces &&
                        loaded.Active.TeleportationSpells == teleportationSpells &&
                        loaded.Active.MagicCircleSpells == magicCircleSpells,
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
                    state.Active.BrownFurTransmuter && state.Active.UrbanBarbarian &&
                    state.Active.BodyguardFeats && state.Active
                        .ProtectionFromAlignmentControlImmunity &&
                    state.Active.ElementalRaces && state.Active.TeleportationSpells,
                    "Malformed settings did not recover all modules ON.");
                Assertions.True(warning != null && Directory.GetFiles(path,
                    "FeatureModules.json.malformed.*").Length == 1 &&
                    warning.Contains("recovered defaults (all modules ON)") &&
                    !warning.Contains("Elemental Races OFF"),
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
                File.WriteAllText(settings, "{\"schemaVersion\":13}");
                bool rejected = false;
                try { FeatureModuleSettingsStore.Load(path); }
                catch (JsonException) { rejected = true; }
                Assertions.True(rejected,
                    "A future feature-module schema must fail closed.");

                File.Delete(settings);
                FeatureModuleSettingsState state = FeatureModuleSettingsStore.Load(path);
                state.SetPending(false, true, false, true, false, true, false, true,
                    false, true, true, true);
                FeatureModuleSettingsStore.Save(state);
                string json = File.ReadAllText(settings);
                string[] keys = { "\"schemaVersion\"", "\"gunslinger\"",
                    "\"acadamae-graduate\"", "\"shield-other\"",
                    "\"expanded-summoning\"", "\"elven-branched-spears\"",
                    "\"eastern-weapons\"", "\"brown-fur-transmuter\"",
                    "\"urban-barbarian\"", "\"bodyguard-feats\"",
                    "\"protection-from-alignment-control-immunity\"",
                    "\"elemental-races\"", "\"teleportation-spells\"", "\"magic-circle-spells\"" };
                int prior = -1;
                foreach (string key in keys)
                {
                    int current = json.IndexOf(key, StringComparison.Ordinal);
                    Assertions.True(current > prior,
                        "Feature-module serialization order changed at " + key + ".");
                    prior = current;
                }
                JObject roundTrip = JObject.Parse(json);
                Assertions.True((int)roundTrip["schemaVersion"] == 12 &&
                    (bool)roundTrip["elemental-races"],
                    "A schema-12 Elemental Races value did not round-trip exactly.");
            });
        }

        internal static void ShowOriginalSummonElementalDefaultsOffAndRoundTrips()
        {
            WithDirectory(path =>
            {
                string settings = Path.Combine(path, FeatureModuleSettingsStore.FileName);
                Assertions.False(FeatureModuleSettingsStore.Load(path).ShowOriginalSummonElemental,
                    "Missing settings hide original Summon Elemental spells.");
                File.WriteAllText(settings, "{\"schemaVersion\":12,\"expanded-summoning\":true}");
                FeatureModuleSettingsState state = FeatureModuleSettingsStore.Load(path);
                Assertions.False(state.ShowOriginalSummonElemental, "An existing schema-12 file without the key stays hidden.");
                Assertions.Equal("settings", state.Source, "The optional key is not a schema migration.");
                Assertions.False(state.RestartRequired, "The presentation option never requires a restart.");
                state.SetShowOriginalSummonElemental(true);
                Assertions.False(state.RestartRequired, "Revealing needs no restart (close and reopen the spell list to apply).");
                FeatureModuleSettingsStore.Save(state);
                JObject saved = JObject.Parse(File.ReadAllText(settings));
                Assertions.True((int)saved["schemaVersion"] == 12 &&
                    (bool)saved[FeatureModuleSettingsStore.ShowOriginalSummonElementalId],
                    "The option is saved under its key without a schema bump.");
                FeatureModuleSettingsState loaded = FeatureModuleSettingsStore.Load(path);
                Assertions.True(loaded.ShowOriginalSummonElemental && loaded.Active.Equals(state.Active),
                    "The option round-trips without changing any module.");
                File.WriteAllText(settings, "{\"schemaVersion\":12,\"show-original-summon-elemental\":\"yes\"}");
                FeatureModuleSettingsState malformed = FeatureModuleSettingsStore.Load(path);
                Assertions.True(malformed.Recovered && !malformed.ShowOriginalSummonElemental,
                    "A non-boolean value recovers to the hidden default.");
            });
        }

        internal static void ActiveSnapshotIsImmutable()
        {
            var state = new FeatureModuleSettingsState(
                FeatureModuleConfiguration.Defaults, "fixture", "fixture", false);
            state.SetPending(false, true, false, false, false, false, false, false,
                false, false, false, true);
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
            Assertions.Equal("bodyguard-feats",
                FeatureModuleConfiguration.BodyguardFeatsId,
                "Bodyguard module ID changed.");
            Assertions.True(state.Active.BodyguardFeats &&
                !state.Pending.BodyguardFeats,
                "Bodyguard pending edits mutated the active snapshot.");
            Assertions.Equal("protection-from-alignment-control-immunity",
                FeatureModuleConfiguration
                    .ProtectionFromAlignmentControlImmunityId,
                "Protection from Alignment module ID changed.");
            Assertions.True(state.Active.ProtectionFromAlignmentControlImmunity &&
                !state.Pending.ProtectionFromAlignmentControlImmunity,
                "Protection from Alignment pending edits mutated the active snapshot.");
            Assertions.Equal("elemental-races",
                FeatureModuleConfiguration.ElementalRacesId,
                "Elemental Races module ID changed.");
            Assertions.True(state.Active.ElementalRaces &&
                !state.Pending.ElementalRaces && state.RestartRequired,
                "Elemental Races pending edits mutated the active snapshot or bypassed restart.");
        }

        internal static void ValueSemanticsIncludeAllModules()
        {
            var enabled = new FeatureModuleConfiguration(true, true, true, true,
                true, true, true, true, true, true, true, true);
            var same = new FeatureModuleConfiguration(true, true, true, true,
                true, true, true, true, true, true, true, true);
            var brownFurOff = new FeatureModuleConfiguration(true, true, true, true,
                true, true, false, true, true, true, true, true);
            var urbanOff = new FeatureModuleConfiguration(true, true, true, true,
                true, true, true, false, true, true, true, true);
            var bodyguardOff = new FeatureModuleConfiguration(true, true, true, true,
                true, true, true, true, false, true, true, true);
            var protectionOff = new FeatureModuleConfiguration(true, true, true,
                true, true, true, true, true, true, false, true, true);
            var elementalOff = new FeatureModuleConfiguration(true, true, true,
                true, true, true, true, true, true, true, false, true);
            Assertions.True(enabled.Equals(same) && enabled.GetHashCode() ==
                same.GetHashCode(), "Equal twelve-module values disagree.");
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
            Assertions.True(!enabled.Equals(bodyguardOff) &&
                enabled.GetHashCode() != bodyguardOff.GetHashCode() &&
                enabled.ToString().Contains("bodyguard-feats=True") &&
                bodyguardOff.ToString().Contains("bodyguard-feats=False"),
                "Bodyguard is absent from value semantics or formatting.");
            Assertions.True(!enabled.Equals(protectionOff) &&
                enabled.GetHashCode() != protectionOff.GetHashCode() &&
                enabled.ToString().Contains(
                    "protection-from-alignment-control-immunity=True") &&
                protectionOff.ToString().Contains(
                    "protection-from-alignment-control-immunity=False"),
                "Protection from Alignment is absent from value semantics or formatting.");
            Assertions.True(!enabled.Equals(elementalOff) &&
                enabled.GetHashCode() != elementalOff.GetHashCode() &&
                enabled.GetHashCode() == 8191 &&
                elementalOff.GetHashCode() == 7167 &&
                enabled.ToString().Contains("elemental-races=True") &&
                elementalOff.ToString().Contains("elemental-races=False"),
                "Elemental Races is absent from value semantics, bit 1024, or formatting.");
        }

        internal static void TeleportationMigrationAndIsolation()
        {
            MagicCircleMigrationAndIsolation();
            Assertions.Equal(13, FeatureModuleConfiguration.ModuleCount, "Module count");
            Assertions.Equal("teleportation-spells", FeatureModuleConfiguration.TeleportationSpellsId, "Stable module ID");
            WithDirectory(path => {
                string settings = Path.Combine(path, FeatureModuleSettingsStore.FileName);
                for (int schema = 0; schema <= 11; schema++)
                foreach (bool? intent in new bool?[] { null, false, true })
                {
                    JObject json = LegacySettings(schema, false);
                    if (intent.HasValue) json["teleportation-spells"] = intent.Value;
                    File.WriteAllText(settings, json.ToString(Formatting.None));
                    FeatureModuleSettingsState loaded = FeatureModuleSettingsStore.Load(path);
                    AssertLegacyFixture(loaded.Active, false, "Teleportation migration must preserve all other modules");
                    Assertions.Equal(intent ?? true, loaded.Active.TeleportationSpells, "Explicit intent/default ON");
                    FeatureModuleSettingsStore.Save(loaded);
                    byte[] prior = File.ReadAllBytes(settings);
                    FeatureModuleSettingsState reloaded = FeatureModuleSettingsStore.Load(path);
                    Assertions.True(loaded.Active.Equals(reloaded.Active) &&
                        Convert.ToBase64String(prior) == Convert.ToBase64String(File.ReadAllBytes(settings)),
                        "Migration must be idempotent");
                    Assertions.Equal(12, (int)JObject.Parse(File.ReadAllText(settings))["schemaVersion"], "Persisted schema");
                }
                File.WriteAllText(settings, "{\"schemaVersion\":11,\"teleportation-spells\":\"false\"}");
                var malformed = FeatureModuleSettingsStore.Load(path);
                Assertions.True(malformed.Recovered && malformed.Active.Equals(FeatureModuleConfiguration.Defaults),
                    "Malformed Teleportation intent must use existing quarantined recovery");
            });
            var on = FeatureModuleConfiguration.Defaults;
            var off = new FeatureModuleConfiguration(true, true, true, true, true, true, true, true, true, true, true, false);
            Assertions.True(!on.Equals(off) && on.GetHashCode() == 8191 && off.GetHashCode() == 6143 &&
                on.ToString().Contains("teleportation-spells=True") && off.ToString().Contains("teleportation-spells=False"),
                "Teleportation must participate in equality/hash/formatting");
            var state = new FeatureModuleSettingsState(off, "fixture", "fixture", false);
            Assertions.True(state.Pending.Equals(off) && !state.Pending.TeleportationSpells && !state.RestartRequired,
                "Copied pending state lost explicit OFF");
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
                ui.Contains("Bodyguard, In Harms Way, and Helpful") &&
                ui.Contains("Aid Another compatibility:") &&
                ui.Contains("Helpful publication:") &&
                ui.Contains("Protection from Alignment: control immunity") &&
                ui.Contains(
                    "Elemental Races: Ifrit, Oread, Sylph, and Undine") &&
                !ui.Contains("Elemental Races: Ifrit, Oread, Sylph, and Undine (preview)") &&
                !ui.Contains("Urban Barbarian  requires Call of the Wild"),
                "Brown-Fur UMM state presentation is incomplete.");
        }

        internal static void ThirteenModuleMatrixCountsAreExact()
        {
            Assertions.Equal(8192, FeatureModuleMatrixPolicy.ExhaustiveCount(13),
                "Thirteen-module exhaustive count changed.");
            Assertions.Equal(28, FeatureModuleMatrixPolicy.BoundaryCount(13),
                "Thirteen-module boundary count changed.");
            int observedBoundary = 0;
            for (int mask = 0; mask < FeatureModuleMatrixPolicy.ExhaustiveCount(13);
                mask++)
            {
                int enabled = 0;
                for (int bit = 0; bit < 13; bit++)
                    if ((mask & (1 << bit)) != 0) enabled++;
                if (FeatureModuleMatrixPolicy.IsBoundaryState(13, enabled))
                    observedBoundary++;
            }
            Assertions.Equal(FeatureModuleMatrixPolicy.BoundaryCount(13),
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
            foreach (bool bodyguardFeats in new[] { false, true })
            foreach (bool protectionFromAlignmentControlImmunity in
                new[] { false, true })
            foreach (bool elementalRaces in new[] { false, true })
                foreach (bool teleportationSpells in new[] { false, true })
                foreach (bool magicCircleSpells in new[] { false, true })
            {
                var plan = new FeatureModulePublicationPlan(
                    new FeatureModuleConfiguration(gunslinger, acadamae, shieldOther,
                        expandedSummoning, elvenBranchedSpears, easternWeapons,
                        brownFurTransmuter, urbanBarbarian, bodyguardFeats,
                        protectionFromAlignmentControlImmunity,
                        elementalRaces, teleportationSpells, magicCircleSpells));
                Assertions.True(plan.GunslingerClass == gunslinger &&
                    plan.GunslingerFeats == gunslinger &&
                    plan.FirearmParameters == gunslinger &&
                    plan.CapitalGunslingerStock == gunslinger &&
                    plan.BeneathStolenLandsStock == gunslinger &&
                    plan.RareFirearmLoot == gunslinger &&
                    plan.SkeletalSalesmanStock == gunslinger,
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
                Assertions.True(plan.BodyguardFeats == bodyguardFeats,
                    "Bodyguard feat publication escaped its independent gate.");
                Assertions.True(plan.ProtectionFromAlignmentControlImmunity ==
                    protectionFromAlignmentControlImmunity,
                    "Protection from Alignment publication escaped its independent gate.");
                Assertions.True(plan.TeleportationSpellLists == teleportationSpells,
                    "Teleportation publication escaped its independent gate.");
                Assertions.True(plan.ElementalRaceSelectors == elementalRaces,
                    "Elemental race selector publication escaped its independent gate.");
            }
        }

        internal static void RuntimeMatrixUsesAuthoritativeTwelveModuleCatalog()
        {
            string root = Environment.CurrentDirectory;
            string matrix = File.ReadAllText(Path.Combine(root, "scripts",
                "Invoke-FeatureModuleRuntimeMatrix.ps1"));
            string catalog = File.ReadAllText(Path.Combine(root, "scripts",
                "FeatureModuleCatalog.ps1"));
            string matrixContract = matrix + Environment.NewLine + catalog;
            foreach (string token in new[] {
                "FeatureModuleCatalog.ps1", "[switch]$Boundary",
                "Get-KmgFeatureModuleConfigurations", "schemaVersion = 12",
                "BrownFurTransmuter", "brown-fur-transmuter",
                "brownFurTransmuter", "UrbanBarbarian", "urban-barbarian",
                "urbanBarbarian", "BodyguardFeats", "bodyguard-feats",
                "bodyguardFeats", "ProtectionFromAlignmentControlImmunity",
                "protection-from-alignment-control-immunity",
                "protectionFromAlignmentControlImmunity",
                "ElementalRaces", "elemental-races", "elementalRaces",
                "TeleportationSpells", "teleportation-spells", "teleportationSpells",
                "28 states for thirteen modules",
                "2 + 2 * $moduleCount",
                "$boundaryRequested = $Combination -ceq 'all'",
                "Get-KmgFeatureModuleConfigurations -Boundary",
                "deliberately has no generic 2^N game-launch mode",
                "[switch]$AllowDirtyGit", "AllowDirtyGit = [bool]$AllowDirtyGit",
                "Settings byte-for-byte restoration failed." })
                Assertions.True(matrixContract.Contains(token),
                    "The authoritative boundary runtime matrix contract is missing: " + token);
            string common = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(common.Contains(
                    "$Parameters.Count -ne $(if($sprint40Recovery){14}else{13})") &&
                common.Contains(
                    "$sprint40Recovery = $Parameters.ContainsKey('sprint40DiagnosticRecovery')") &&
                common.Contains("expandedSummoning = [bool]$Parameters.expandedSummoning") &&
                common.Contains("elvenBranchedSpears = [bool]$Parameters.elvenBranchedSpears") &&
                common.Contains("easternWeapons = [bool]$Parameters.easternWeapons") &&
                common.Contains("brownFurTransmuter = [bool]$Parameters.brownFurTransmuter") &&
                common.Contains("urbanBarbarian = [bool]$Parameters.urbanBarbarian") &&
                common.Contains("bodyguardFeats = [bool]$Parameters.bodyguardFeats") &&
                common.Contains("[bool]$Parameters.protectionFromAlignmentControlImmunity") &&
                common.Contains("elementalRaces = [bool]$Parameters.elementalRaces") &&
                common.Contains("teleportationSpells = [bool]$Parameters.teleportationSpells"),
                "The guarded request writer does not require all thirteen module states.");
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            foreach (string token in new[] {
                "firearmParameterCount == (expectedGunslinger ? 15 : 0)",
                "spearRegisteredItems == 12",
                "spearParameterizedOptions ==",
                "(expectedElvenBranchedSpears ? 7 : 0)",
                "spearStaticOptions ==",
                "(expectedElvenBranchedSpears ? 3 : 0)",
                "spearFamiliarityCategories == 1",
                "boundaryVendorRows.Where(row => row.Group == \"spear\").All(row => row.Passed)",
                // 5 loot rows since Thornstep was retired from acquisition (#129).
                "spearLootRows ==",
                "(expectedElvenBranchedSpears ? 5 : 0)",
                "5 loot rows (Thornstep retired)",
                "always-registered identities and exact selector, familiarity, vendor, and fixed-loot surfaces" })
                Assertions.True(runner.Contains(token),
                    "The live module observer lacks the spear assertion: " +
                    token);
            string request = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRequest.cs")).Replace("\r\n", "\n");
            Assertions.True(request.Contains(
                "request.Parameters.Count != (diagnosticRecovery ? 14 : 13)") &&
                request.Contains("Property(\"sprint40DiagnosticRecovery\")") && request.Contains(
                    "Property(\"elvenBranchedSpears\")") && request.Contains(
                    "request.Parameters[\"elvenBranchedSpears\"]") && request.Contains(
                    "request.Parameters[\"easternWeapons\"]") && request.Contains(
                    "request.Parameters[\"brownFurTransmuter\"]") && request.Contains(
                    "request.Parameters[\"urbanBarbarian\"]") && request.Contains(
                    "request.Parameters[\"bodyguardFeats\"]") && request.Contains(
                    "Property(\n                        \"protectionFromAlignmentControlImmunity\")") &&
                    request.Contains(
                        "\"protectionFromAlignmentControlImmunity\"].Type") &&
                    request.Contains("Property(\"elementalRaces\")") &&
                    request.Contains("[\"elementalRaces\"].Type") &&
                    request.Contains("Property(\"teleportationSpells\")") &&
                    request.Contains("[\"teleportationSpells\"].Type"),
                "The in-mod request validator does not require all thirteen module states.");
        }

        private static void MagicCircleMigrationAndIsolation()
        {
            Assertions.Equal("magic-circle-spells", FeatureModuleConfiguration.MagicCircleSpellsId,
                "Magic Circle has one independent stable content setting");
            WithDirectory(path => {
                string settings = Path.Combine(path, FeatureModuleSettingsStore.FileName);
                for (int schema = 0; schema <= 12; schema++)
                foreach (bool? circle in new bool?[] { null, false, true })
                foreach (bool control in new[] { false, true })
                {
                    JObject json = LegacySettings(schema, false);
                    json[FeatureModuleConfiguration.ProtectionFromAlignmentControlImmunityId] = control;
                    json[FeatureModuleConfiguration.TeleportationSpellsId] = false;
                    if (circle.HasValue) json[FeatureModuleConfiguration.MagicCircleSpellsId] = circle.Value;
                    File.WriteAllText(settings, json.ToString(Formatting.None));
                    var loaded = FeatureModuleSettingsStore.Load(path);
                    Assertions.Equal(circle ?? true, loaded.Active.MagicCircleSpells,
                        "Missing Circle setting defaults ON; explicit choice survives migration");
                    Assertions.Equal(control, loaded.Active.ProtectionFromAlignmentControlImmunity,
                        "Circle never replaces the shared Protection authority");
                    Assertions.True(!loaded.Active.Gunslinger && !loaded.Active.ShieldOther &&
                        !loaded.Active.TeleportationSpells && !loaded.Active.ElementalRaces,
                        "Migration preserves unrelated disabled modules");
                    Assertions.Equal(loaded.Active.MagicCircleSpells, loaded.Pending.MagicCircleSpells,
                        "Pending snapshot preserves explicit OFF");
                    var plan = new FeatureModulePublicationPlan(loaded.Active);
                    Assertions.Equal(loaded.Active.MagicCircleSpells, plan.MagicCircleSpellLists,
                        "Content publication is independent of enhanced control");
                    Assertions.Equal(control, plan.ProtectionFromAlignmentControlImmunity,
                        "Shared control publication follows only its existing setting");
                    FeatureModuleSettingsStore.Save(loaded);
                    Assertions.True(loaded.Active.Equals(FeatureModuleSettingsStore.Load(path).Active),
                        "Every startup combination survives save and reload");
                }
            });
        }

        private static JObject LegacySettings(int schema, bool? elementalRaces)
        {
            var root = new JObject();
            if (schema != 0) root["schemaVersion"] = schema;
            root[FeatureModuleConfiguration.GunslingerId] = false;
            root[FeatureModuleConfiguration.AcadamaeGraduateId] = true;
            root[FeatureModuleConfiguration.ShieldOtherId] = false;
            root[FeatureModuleConfiguration.ExpandedSummoningId] = true;
            root[FeatureModuleConfiguration.ElvenBranchedSpearsId] = false;
            root[FeatureModuleConfiguration.EasternWeaponsId] = true;
            root[FeatureModuleConfiguration.BrownFurTransmuterId] = false;
            root[FeatureModuleConfiguration.UrbanBarbarianId] = true;
            root[FeatureModuleConfiguration.BodyguardFeatsId] = false;
            root[FeatureModuleConfiguration
                .ProtectionFromAlignmentControlImmunityId] = true;
            if (elementalRaces.HasValue)
                root[FeatureModuleConfiguration.ElementalRacesId] =
                    elementalRaces.Value;
            return root;
        }

        private static void AssertLegacyFixture(
            FeatureModuleConfiguration actual, bool elementalRaces,
            string label)
        {
            Assertions.True(!actual.Gunslinger &&
                actual.AcadamaeGraduate && !actual.ShieldOther &&
                actual.ExpandedSummoning && !actual.ElvenBranchedSpears &&
                actual.EasternWeapons && !actual.BrownFurTransmuter &&
                actual.UrbanBarbarian && !actual.BodyguardFeats &&
                actual.ProtectionFromAlignmentControlImmunity &&
                actual.ElementalRaces == elementalRaces,
                "Feature-module values changed during " + label + ".");
        }

        /// <summary>
        /// GS-AUD-0006. A valid older-schema file whose migration write fails
        /// must keep its own parsed values for the session, leave the original
        /// bytes untouched, avoid the malformed quarantine, and report a WRITE
        /// failure. The fault is real, not mocked: the destination is made
        /// read-only, which makes File.Replace throw UnauthorizedAccessException
        /// while the read still succeeds. The read-only attribute is always
        /// restored so the fixture cannot leak.
        /// </summary>
        internal static void FailedMigrationWritePreservesValidSettings()
        {
            WithDirectory(path =>
            {
                string settings = Path.Combine(path,
                    FeatureModuleSettingsStore.FileName);
                // A valid schema-10 file with a deliberate mix, including OFF
                // values, so substituted all-ON defaults are unmistakable.
                string original = new JObject
                {
                    ["schemaVersion"] = 10,
                    ["gunslinger"] = true,
                    ["acadamae-graduate"] = false,
                    ["shield-other"] = true,
                    ["expanded-summoning"] = false,
                    ["elven-branched-spears"] = true,
                    ["eastern-weapons"] = false,
                    ["brown-fur-transmuter"] = true,
                    ["urban-barbarian"] = false,
                    ["bodyguard-feats"] = true,
                    ["protection-from-alignment-control-immunity"] = false,
                    ["elemental-races"] = true
                }.ToString(Formatting.Indented);
                File.WriteAllText(settings, original);

                FileAttributes before = File.GetAttributes(settings);
                string warning = null;
                FeatureModuleSettingsState state;
                try
                {
                    File.SetAttributes(settings, before | FileAttributes.ReadOnly);
                    state = FeatureModuleSettingsStore.Load(path,
                        value => warning = value);
                }
                finally
                {
                    // Restore the attribute even if the assertions below fail.
                    if (File.Exists(settings)) File.SetAttributes(settings, before);
                    // Clear read-only on anything the store may have produced
                    // from a read-only source, so the shared directory cleanup
                    // cannot fail and mask this test's real assertion message.
                    foreach (string produced in Directory.GetFiles(path))
                        File.SetAttributes(produced,
                            File.GetAttributes(produced) & ~FileAttributes.ReadOnly);
                }

                // The parsed configuration, including every OFF value, stands.
                Assertions.True(state.Active.Gunslinger &&
                    !state.Active.AcadamaeGraduate && state.Active.ShieldOther &&
                    !state.Active.ExpandedSummoning &&
                    state.Active.ElvenBranchedSpears &&
                    !state.Active.EasternWeapons &&
                    state.Active.BrownFurTransmuter &&
                    !state.Active.UrbanBarbarian && state.Active.BodyguardFeats &&
                    !state.Active.ProtectionFromAlignmentControlImmunity &&
                    state.Active.ElementalRaces,
                    "A failed migration write discarded the parsed values of the file.");
                // Not a malformed recovery.
                Assertions.False(state.Recovered,
                    "A valid file with an unwritable destination was reported as recovered.");
                Assertions.True(state.MigrationWriteFailed,
                    "The migration write failure was not reported on the state.");
                Assertions.Equal("migrated-schema-10", state.Source,
                    "A failed migration write changed the migrated source identity.");
                // The original file is untouched and never quarantined.
                Assertions.Equal(original, File.ReadAllText(settings),
                    "A failed migration write altered the original settings bytes.");
                Assertions.Equal(0, Directory.GetFiles(path,
                    "FeatureModules.json.malformed.*").Length,
                    "A valid file was quarantined as malformed.");
                Assertions.Equal(0, Directory.GetFiles(path,
                    "FeatureModules.json.tmp").Length,
                    "A failed migration write left its temporary file behind.");
                // The warning names a write failure, not a malformed file.
                Assertions.True(warning != null &&
                    warning.Contains("Could not write the migrated") &&
                    !warning.Contains("Malformed") &&
                    !warning.Contains("recovered defaults (all modules ON)"),
                    "A failed migration write did not report an accurate write failure.");
            });
        }

        /// <summary>
        /// GS-AUD-0006 negative control: the same valid schema-10 input on a
        /// writable destination must migrate, persist schema 12 and report no
        /// failure at all. Without this, the test above could pass against a
        /// repair that simply never writes.
        /// </summary>
        internal static void SuccessfulMigrationWriteStillPersists()
        {
            WithDirectory(path =>
            {
                string settings = Path.Combine(path,
                    FeatureModuleSettingsStore.FileName);
                File.WriteAllText(settings, new JObject
                {
                    ["schemaVersion"] = 10,
                    ["gunslinger"] = true,
                    ["acadamae-graduate"] = false,
                    ["elemental-races"] = true
                }.ToString(Formatting.Indented));
                string warning = null;
                FeatureModuleSettingsState state = FeatureModuleSettingsStore.Load(
                    path, value => warning = value);
                Assertions.False(state.MigrationWriteFailed,
                    "A writable destination reported a migration write failure.");
                Assertions.False(state.Recovered,
                    "A valid migrating file was reported as recovered.");
                Assertions.Equal("migrated-schema-10", state.Source,
                    "The migrated source identity is wrong.");
                Assertions.True(warning == null,
                    "A successful migration emitted a warning.");
                Assertions.Equal(12, (int)JObject.Parse(
                    File.ReadAllText(settings))["schemaVersion"],
                    "A successful migration did not persist schema 12.");
                Assertions.False(state.Active.AcadamaeGraduate,
                    "A successful migration lost an explicit OFF value.");
            });
        }

        /// <summary>
        /// GS-AUD-0006 negative control: a file already at the current schema
        /// never attempts the migration write, so a read-only destination is
        /// not a failure at all.
        /// </summary>
        internal static void CurrentSchemaNeedsNoMigrationWrite()
        {
            WithDirectory(path =>
            {
                string settings = Path.Combine(path,
                    FeatureModuleSettingsStore.FileName);
                File.WriteAllText(settings, new JObject
                {
                    ["schemaVersion"] = 12,
                    ["gunslinger"] = true,
                    ["acadamae-graduate"] = false
                }.ToString(Formatting.Indented));
                FileAttributes before = File.GetAttributes(settings);
                string warning = null;
                FeatureModuleSettingsState state;
                try
                {
                    File.SetAttributes(settings, before | FileAttributes.ReadOnly);
                    state = FeatureModuleSettingsStore.Load(path,
                        value => warning = value);
                }
                finally
                {
                    if (File.Exists(settings)) File.SetAttributes(settings, before);
                }
                Assertions.False(state.MigrationWriteFailed,
                    "A current-schema file attempted a migration write.");
                Assertions.False(state.Recovered,
                    "A current-schema read-only file was reported as recovered.");
                Assertions.Equal("settings", state.Source,
                    "A current-schema file did not report the settings source.");
                Assertions.True(warning == null,
                    "A current-schema read-only file emitted a warning.");
                Assertions.False(state.Active.AcadamaeGraduate,
                    "A current-schema read-only file lost an explicit OFF value.");
            });
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
