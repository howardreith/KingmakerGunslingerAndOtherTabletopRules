using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbOracleMysterySelectionGuid = "896d48002fc54e8f8cec86755db77428";
        private const string FcbOracleRevelationSelectionGuid = "54e769b78fb844babb5d68ba9fdd5856";
        private const string FcbFlameMysteryGuid = "f70828f0300e4f8da5e779c6545e4aca";
        private const string FcbFlameRevelationSelectionGuid = "661d3db2e8c74b6889136ac67071657c";
        private const string FcbWindMysteryGuid = "8185bce933c141f98fd23b74810642c2";
        private const string FcbWindRevelationSelectionGuid = "50e2cf4e57364ac8964bdd5517d26a4d";
        private const string FcbFireBreathAbilityGuid = "29e09b5e6855479486992ba5d721968e";
        private const string FcbFireBreathResourceGuid = "b2475d2838504f63ab1b8860ea3d67e1";
        private const string FcbHeatAuraAbilityGuid = "388cbfa3cc714463b3ff09f5f2e2913f";
        private const string FcbHeatAuraResourceGuid = "6c728eca4984497383ee1c5b3f94d403";
        private const string FcbAncestralWeaponResourceGuid = "2d9adba619574152a726456627f5679f";
        private const string FcbBleedingWoundsBuffGuid = "743cb1d1ee9a4e769033d53fd02ec77e";
        private const string FcbFirestormAreaGuid = "9c9984872c6b43a4a1d27339d124f84b";
        private const string FcbGiftBiteAbilityGuid = "32ed966b5d5e45c090bd4c042ec0b999";
        private const string FcbSpiritShieldAbilityGuid = "59f5b1a736a1461e9a464f4993b6243a";
        private const string FcbBattlecryEffectBuffGuid = "bbc83bf7650b4eb5891fce8ddf7cfe53";
        private const string FcbEraseFromTimeResourceGuid = "d4b9a6296d964bb787728bb0a40a828d";

        // I06/S04: the scoped revelation read points, native Oracle menus and
        // level-9 effective-level probes (with neighbor, held-back and removal
        // controls) on detached fixture units.
        private RuntimeTestResult RunFavoredClassOracleRevelations()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            BlueprintFeatureSelection bonus = host == null ? null :
                host.BonusSelectionFor(FavoredClassRevelationManifest.OracleClassGuid);
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                host != null && leaves != null && FavoredClassRuntime.MechanicsEnabled && bonus != null &&
                FavoredClassRevelationScopes.ScopeCount == FavoredClassRevelationManifest.All.Count;
            assertions.Add(Assertion("fcb-oracle-ready",
                "the exact host is published with an Oracle bonus selection, mechanics are enabled and every manifest target was scoped",
                status + ";oracleSelection=" + (bonus == null ? "none" : bonus.AssetGuid) + ";scopes=" +
                    FavoredClassRevelationScopes.ScopeCount, ready,
                "FavoredClassIntegrationStatusRegistry, host.BonusSelectionFor and FavoredClassRevelationScopes"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            object player = ReadExactMember(Game.Instance, "Player");
            object state = ReadExactMember(Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);

            var evidence = new JObject();
            var scopeFailures = new List<string>();
            var menuFailures = new List<string>();
            var mechanicsFailures = new List<string>();
            var gateFailures = new List<string>();
            bool cleaned = false;
            try
            {
                evidence["scopes"] = ObserveRevelationScopes(scopeFailures);
                evidence["menus"] = RunOracleMenus(bonus, leaves, menuFailures);
                evidence["mechanics"] = ObserveRevelationMechanics(leaves, mechanicsFailures);
                evidence["gates"] = ObserveRevelationGates(bonus, leaves, gateFailures);
            }
            catch (Exception exception)
            {
                mechanicsFailures.Add("exception=" + exception);
            }
            finally
            {
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }
            string evidencePath = WriteFavoredClassEvidence("favored-class-oracle-revelations.json", evidence);
            assertions.Add(Assertion("fcb-oracle-scopes",
                "every revelation target has read points in the live provider graph and includes every adapter family the audit implements; its breakpoint tables and tiers are scaled, and only the charter-excluded possession BAB read is not",
                Describe(evidence["scopes"] == null ? null : evidence["scopes"]["summary"], scopeFailures),
                scopeFailures.Count == 0, "FavoredClassRevelationScopes (walk of the live blueprint graph)"));
            assertions.Add(Assertion("fcb-oracle-menus",
                "an Ifrit or Sylph Oracle is offered exactly the counters of the manifest revelations it owns (and the chosen one), a Human Oracle none",
                Describe(evidence["menus"], menuFailures), menuFailures.Count == 0,
                "level-1 native Oracle visits with the chosen mystery and revelation; BlueprintFeatureSelection.CanSelect"));
            assertions.Add(Assertion("fcb-oracle-mechanics",
                "two Fire Breath steps give exactly the native values at oracle level 11 (dice, caster level, DC, uses) while Heat Aura, Fireball and other revelations stay at level 9; per-level uses, buff and area ranks, tiers, steps and single-level uses of the owned revelation follow its own counter; the possession BAB read is excluded; a feature context refreshes on gain and removal",
                Describe(evidence["mechanics"], mechanicsFailures), mechanicsFailures.Count == 0,
                "AddClassLevel fixtures; AbilityData.CreateExecutionContext, MechanicsContext ranks and GetMaxAmount"));
            assertions.Add(Assertion("fcb-oracle-level-gates",
                "every level gate of every published revelation (the abilities and forms it grants at later oracle levels) decides at the effective level: one level below the gate the native state holds, one earned step moves it exactly as the native gate at the next level, removing the step restores it; a neighbor revelation's gate, BAB, saves and class level stay native; Spirit of the Warrior (its possession BAB) is excluded and never published",
                Describe(evidence["gates"] == null ? null : evidence["gates"]["summary"], gateFailures),
                gateFailures.Count == 0,
                "AddClassLevel fixtures; the native AddFeatureOnClassLevel gates (HasFact of each gated feature); publication skip evidence"));
            assertions.Add(Assertion("external-isolation", "unchanged party and global-unit snapshots",
                "cleaned=" + cleaned, cleaned, "detached entity disposal and exact reference snapshots"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion,
                _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version,
                "Unity Mod Manager ModEntry.Info.Version"));
            RuntimeTestResult result = CreateResult(assertions.TrueForAll(value => value.Status == "PASS")
                ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions, null);
            result.EvidenceFiles.Add(evidencePath);
            return result;
        }

        private static JObject ObserveRevelationScopes(IList<string> failures)
        {
            var rows = new JArray();
            var extras = new JArray();
            int ranks = 0, resources = 0, parameters = 0;
            foreach (FavoredClassRevelationTarget target in FavoredClassRevelationManifest.All)
            {
                FavoredClassRevelationScope scope = FavoredClassRevelationScopes.ForKey(target.Key);
                if (scope == null)
                {
                    failures.Add(target.Key + ": no scope");
                    continue;
                }
                string found = scope.Families;
                var row = new JObject
                {
                    ["key"] = target.Key,
                    ["features"] = scope.Features.Count,
                    ["auditFamilies"] = target.Families,
                    ["foundFamilies"] = found,
                    ["ranks"] = new JArray(scope.RankSources.Select(source => source.Value.name + "|" +
                        source.Key.Type)),
                    ["resources"] = new JArray(scope.Resources.Keys.Select(resource => resource.name)),
                    ["params"] = new JArray(scope.ParamsAbilities.Select(ability => ability.name)),
                    ["refresh"] = new JArray(scope.RefreshFeatures.Select(feature => feature.name)),
                    ["evidence"] = new JArray(scope.Evidence)
                };
                rows.Add(row);
                ranks += scope.RankSources.Count;
                resources += scope.Resources.Count;
                parameters += scope.ParamsAbilities.Count;
                if (scope.Features.Count != target.FeatureGuids.Length)
                    failures.Add(target.Key + ": " + scope.Features.Count + "/" + target.FeatureGuids.Length +
                        " revelation features resolved");
                if (!scope.HasReadPoints)
                    failures.Add(target.Key + ": no read point");
                foreach (char family in new[] { 'A', 'B', 'C' })
                {
                    if (target.HasFamily(family) && found.IndexOf(family) < 0)
                        failures.Add(target.Key + ": audited family " + family + " not found");
                    if (!target.HasFamily(family) && found.IndexOf(family) >= 0)
                        extras.Add(target.Key + ":" + family);
                }
                Func<string, bool> scaled = read => scope.RankSources.Any(source =>
                    source.Value.AssetGuid + "|" + source.Key.Type == read);
                foreach (string held in target.ExcludedRanks)
                    if (scaled(held))
                        failures.Add(target.Key + ": the charter-excluded read was scaled " + held);
                // Charter 8.10: the owned revelation's own steps, tiers and
                // breakpoint tables are level-dependent values and scale.
                if (target.Key == "TimeSight" && !scope.Evidence.Any(value =>
                        value.StartsWith("breakpoint-table:", StringComparison.Ordinal)))
                    failures.Add(target.Key + ": its breakpoint table was not scaled");
                if (new[] { "SpiritShield", "AirBarrier", "IceArmor", "ArmorOfBones", "GiftOfClawAndHorn",
                        "RaiseTheDead" }.Contains(target.Key) &&
                    !scope.Evidence.Any(value => value.StartsWith("tier:", StringComparison.Ordinal)))
                    failures.Add(target.Key + ": its tier rank was not scaled");
                if (target.Key == "SpiritOfTheWarrior" && !scope.Evidence.Any(value =>
                        value.StartsWith("excluded-read:", StringComparison.Ordinal)))
                    failures.Add(target.Key + ": the possession BAB read was not excluded");
            }
            return new JObject
            {
                ["summary"] = new JObject
                {
                    ["targets"] = rows.Count,
                    ["rankSources"] = ranks,
                    ["resources"] = resources,
                    ["paramsAbilities"] = parameters,
                    ["foundBeyondAudit"] = extras
                },
                ["targets"] = rows
            };
        }

        private JArray RunOracleMenus(BlueprintFeatureSelection bonus, FavoredClassBlueprintSet leaves,
            IList<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            BlueprintCharacterClass oracle = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FavoredClassRevelationManifest.OracleClassGuid, "Oracle");
            FavoredClassLeafPair[] pairs = leaves.Pairs.Where(pair =>
                pair.Effect.Id == FavoredClassCatalog.EffectSelectedRevelation).ToArray();
            // Mysteries are progressions and revelation lists are selections.
            Func<string, BlueprintFeature> feature = guid =>
            {
                BlueprintScriptableObject value;
                library.BlueprintsByAssetId.TryGetValue(guid, out value);
                if (!(value is BlueprintFeature))
                    throw new InvalidOperationException("Missing provider feature " + guid);
                return (BlueprintFeature)value;
            };
            var cases = new[]
            {
                Tuple.Create(FavoredClassAncestry.Ifrit, FcbFlameMysteryGuid, FcbFlameRevelationSelectionGuid,
                    "FireBreath", true),
                Tuple.Create(FavoredClassAncestry.Sylph, FcbWindMysteryGuid, FcbWindRevelationSelectionGuid,
                    "LightningBreath", true),
                Tuple.Create(FavoredClassAncestry.Human, FcbFlameMysteryGuid, FcbFlameRevelationSelectionGuid,
                    "FireBreath", false),
            };
            var rows = new JArray();
            foreach (var entry in cases)
            {
                var row = new JObject { ["ancestry"] = entry.Item1, ["revelation"] = entry.Item4 };
                UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
                LevelUpController controller = null;
                try
                {
                    BlueprintRace race = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                        FavoredClassRaceIdentities.ForAncestry(entry.Item1).RaceGuid, entry.Item1);
                    controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, oracle,
                        "KMG FCB Oracle Menu");
                    FeatureSelectionState mystery = FavoredClassLevelUpHarness.FindOpenState(controller,
                        FcbOracleMysterySelectionGuid);
                    row["mysterySelected"] = mystery != null &&
                        FavoredClassLevelUpHarness.Select(controller, mystery, feature(entry.Item2));
                    FeatureSelectionState root = FavoredClassLevelUpHarness.FindOpenState(controller,
                        FcbOracleRevelationSelectionGuid);
                    row["rootSelection"] = root == null ? "none" : "open";
                    if (root != null)
                        row["rootSelected"] = FavoredClassLevelUpHarness.Select(controller, root,
                            feature(entry.Item3));
                    FeatureSelectionState revelations = FavoredClassLevelUpHarness.FindOpenState(controller,
                        entry.Item3);
                    BlueprintFeature chosen = feature(FavoredClassRevelationManifest.For(entry.Item4).FeatureGuids[0]);
                    row["revelationSelected"] = revelations != null &&
                        FavoredClassLevelUpHarness.Select(controller, revelations, chosen);
                    FavoredClassLevelUpHarness.ChooseFavoredClass(controller, oracle, row);
                    row["filled"] = FavoredClassLevelUpHarness.FillOthers(controller,
                        new HashSet<string>(StringComparer.Ordinal) { bonus.AssetGuid });
                    UnitDescriptor preview = controller.Preview;
                    string[] owned = FavoredClassRevelationManifest.All.Where(target => target.FeatureGuids.Any(guid =>
                        preview.Progression.Features.Enumerable.Any(value => value.Blueprint != null &&
                            value.Blueprint.AssetGuid == guid))).Select(target => target.Key).ToArray();
                    FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller, bonus.AssetGuid);
                    string[] offered = fcb == null ? new string[0] : pairs.Where(pair =>
                        FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full) ||
                        (pair.Partial != null && FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Partial)))
                        .Select(pair => pair.TargetKey).ToArray();
                    row["owned"] = new JArray(owned);
                    row["offered"] = new JArray(offered);
                    string[] expected = entry.Item5 ? owned : new string[0];
                    if (!owned.Contains(entry.Item4))
                        failures.Add(entry.Item1 + ": the chosen revelation " + entry.Item4 + " was not gained");
                    if (!offered.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                            expected.OrderBy(value => value, StringComparer.Ordinal)))
                        failures.Add(entry.Item1 + ": offered " + string.Join(",", offered) + " expected " +
                            string.Join(",", expected));
                }
                catch (Exception exception)
                {
                    failures.Add(entry.Item1 + ": " + exception.GetType().Name + ": " + exception.Message);
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(controller);
                    unit.Dispose();
                }
                rows.Add(row);
            }
            return rows;
        }

        private JObject ObserveRevelationMechanics(FavoredClassBlueprintSet leaves, IList<string> failures)
        {
            var result = new JObject();
            var library = BlueprintBootstrap.Library;
            string effect = FavoredClassCatalog.EffectSelectedRevelation;
            BlueprintCharacterClass oracle = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FavoredClassRevelationManifest.OracleClassGuid, "Oracle");
            Func<string, BlueprintScriptableObject> blueprint = guid =>
            {
                BlueprintScriptableObject value;
                if (!library.BlueprintsByAssetId.TryGetValue(guid, out value) || value == null)
                    throw new InvalidOperationException("Missing provider blueprint " + guid);
                return value;
            };
            Func<string, BlueprintFeature> revelation = key =>
                (BlueprintFeature)blueprint(FavoredClassRevelationManifest.For(key).FeatureGuids[0]);
            Func<string, BlueprintFeature> full = key => leaves.Pair(effect, key).Full;
            var units = new List<UnitEntityData>();
            try
            {
                Func<int, string[], UnitEntityData> oracleAt = (levels, owned) =>
                {
                    UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                        BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                    units.Add(unit);
                    for (int added = 0; added < levels; added++)
                        unit.Descriptor.Progression.AddClassLevel(oracle);
                    foreach (string key in owned)
                        unit.Descriptor.AddFact(revelation(key));
                    return unit;
                };
                UnitEntityData target = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                units.Add(target);
                string[] kit = { "FireBreath", "HeatAura", "AncestralWeapon", "SpiritBoost" };
                UnitEntityData control = oracleAt(9, kit);
                UnitEntityData breath = oracleAt(9, kit);
                UnitEntityData weapon = oracleAt(9, kit);
                UnitEntityData spread = oracleAt(9, kit);
                GrantFavoredClassRanks(breath, full("FireBreath"), 2);
                GrantFavoredClassRanks(weapon, full("AncestralWeapon"), 2);
                foreach (string key in new[] { "BleedingWounds", "Firestorm", "GiftOfClawAndHorn", "Battlecry" })
                    GrantFavoredClassRanks(spread, full(key), 2);
                UnitEntityData twelve = oracleAt(12, new string[0]);
                UnitEntityData twelveInvested = oracleAt(12, new string[0]);
                GrantFavoredClassRanks(twelveInvested, full("SpiritShield"), 1);
                UnitEntityData tenth = oracleAt(10, new string[0]);
                UnitEntityData tenthInvested = oracleAt(10, new string[0]);
                GrantFavoredClassRanks(tenthInvested, full("EraseFromTime"), 1);
                result["oracleLevel"] = control.Descriptor.Progression.GetClassLevel(oracle);

                Func<UnitEntityData, string, JObject> ability = (caster, guid) =>
                {
                    var data = new AbilityData((BlueprintAbility)blueprint(guid), caster.Descriptor);
                    var context = data.CreateExecutionContext(new TargetWrapper(target));
                    context.Recalculate();
                    return new JObject
                    {
                        ["casterLevel"] = context.Params.CasterLevel,
                        ["dc"] = context.Params.DC,
                        ["rankBonus"] = context.Params.RankBonus,
                        ["default"] = context[AbilityRankType.Default],
                        ["damageDice"] = context[AbilityRankType.DamageDice],
                        ["statBonus"] = context[AbilityRankType.StatBonus]
                    };
                };
                Func<UnitEntityData, string, AbilityRankType, int> rank = (caster, guid, type) =>
                {
                    var context = new MechanicsContext(caster, caster.Descriptor, blueprint(guid));
                    context.Recalculate();
                    return context[type];
                };
                Func<UnitEntityData, string, int> uses = (unit, guid) =>
                    ((BlueprintAbilityResource)blueprint(guid)).GetMaxAmount(unit.Descriptor);
                Func<JObject, JObject, string, int> delta = (after, before, key) => (int)after[key] - (int)before[key];

                JObject breathControl = ability(control, FcbFireBreathAbilityGuid);
                JObject breathInvested = ability(breath, FcbFireBreathAbilityGuid);
                JObject auraControl = ability(control, FcbHeatAuraAbilityGuid);
                JObject auraNeighbor = ability(breath, FcbHeatAuraAbilityGuid);
                JObject fireballControl = ability(control, FcbFireballGuid);
                JObject fireballInvested = ability(breath, FcbFireballGuid);
                result["fireBreath"] = new JObject { ["control"] = breathControl, ["invested"] = breathInvested };
                result["heatAura"] = new JObject { ["control"] = auraControl, ["neighbor"] = auraNeighbor };
                result["fireball"] = new JObject { ["control"] = fireballControl, ["invested"] = fireballInvested };
                int level = (int)result["oracleLevel"];
                if (delta(breathInvested, breathControl, "casterLevel") != 2 ||
                    delta(breathInvested, breathControl, "default") != 2 ||
                    delta(breathInvested, breathControl, "rankBonus") != 0)
                    failures.Add("two Fire Breath steps did not add exactly +2 caster level and dice (no rank bonus)");
                if (delta(breathInvested, breathControl, "dc") != FavoredClassMechanicsPolicy.HalfLevelDelta(level, 2))
                    failures.Add("the Fire Breath DC changed by " + delta(breathInvested, breathControl, "dc"));
                foreach (var pair in new[] { Tuple.Create(auraNeighbor, auraControl),
                    Tuple.Create(fireballInvested, fireballControl) })
                    foreach (string key in new[] { "casterLevel", "dc", "rankBonus", "default", "damageDice", "statBonus" })
                        if (delta(pair.Item1, pair.Item2, key) != 0)
                            failures.Add("an unchosen revelation or spell changed " + key);

                var resources = new JObject
                {
                    ["fireBreathControl"] = uses(control, FcbFireBreathResourceGuid),
                    ["fireBreathInvested"] = uses(breath, FcbFireBreathResourceGuid),
                    ["heatAuraControl"] = uses(control, FcbHeatAuraResourceGuid),
                    ["heatAuraNeighbor"] = uses(breath, FcbHeatAuraResourceGuid),
                    ["ancestralWeaponControl"] = uses(control, FcbAncestralWeaponResourceGuid),
                    ["ancestralWeaponInvested"] = uses(weapon, FcbAncestralWeaponResourceGuid),
                    ["ancestralWeaponNeighbor"] = uses(breath, FcbAncestralWeaponResourceGuid)
                };
                result["resources"] = resources;
                if ((int)resources["fireBreathControl"] != 2 || (int)resources["fireBreathInvested"] != 3)
                    failures.Add("Fire Breath uses were not 2 at level 9 and 3 at effective level 11");
                if ((int)resources["heatAuraNeighbor"] != (int)resources["heatAuraControl"] ||
                    (int)resources["ancestralWeaponNeighbor"] != (int)resources["ancestralWeaponControl"])
                    failures.Add("a neighbor revelation's uses changed");
                if ((int)resources["ancestralWeaponInvested"] - (int)resources["ancestralWeaponControl"] != 2)
                    failures.Add("per-level Ancestral Weapon minutes did not rise by exactly 2");

                var contexts = new JObject
                {
                    ["bleedingWoundsBuffControl"] = rank(control, FcbBleedingWoundsBuffGuid, AbilityRankType.Default),
                    ["bleedingWoundsBuffInvested"] = rank(spread, FcbBleedingWoundsBuffGuid, AbilityRankType.Default),
                    ["firestormAreaControl"] = rank(control, FcbFirestormAreaGuid, AbilityRankType.DamageDice),
                    ["firestormAreaInvested"] = rank(spread, FcbFirestormAreaGuid, AbilityRankType.DamageDice),
                    ["giftTierControl"] = rank(control, FcbGiftBiteAbilityGuid, AbilityRankType.StatBonus),
                    ["giftTierInvested"] = rank(spread, FcbGiftBiteAbilityGuid, AbilityRankType.StatBonus),
                    ["battlecryStepControl"] = rank(control, FcbBattlecryEffectBuffGuid, AbilityRankType.StatBonus),
                    ["battlecryStepInvested"] = rank(spread, FcbBattlecryEffectBuffGuid, AbilityRankType.StatBonus),
                    ["spiritShieldTierAt12"] = rank(twelve, FcbSpiritShieldAbilityGuid, AbilityRankType.StatBonus),
                    ["spiritShieldTierAt12Invested"] = rank(twelveInvested, FcbSpiritShieldAbilityGuid,
                        AbilityRankType.StatBonus),
                    ["eraseFromTimeUsesAt10"] = uses(tenth, FcbEraseFromTimeResourceGuid),
                    ["eraseFromTimeUsesAt10Invested"] = uses(tenthInvested, FcbEraseFromTimeResourceGuid)
                };
                result["contexts"] = contexts;
                Func<string, int> value = key => (int)contexts[key];
                // Bleeding Wounds 1 + L/5 (buff context), Firestorm L dice (area
                // context), Gift of Claw and Horn tier 1 + L/5: native at level 11.
                if (value("bleedingWoundsBuffControl") != 1 + level / 5 ||
                    value("bleedingWoundsBuffInvested") != 1 + (level + 2) / 5)
                    failures.Add("the Bleeding Wounds buff rank did not follow its own counter");
                if (value("firestormAreaInvested") - value("firestormAreaControl") != 2)
                    failures.Add("the Firestorm area dice did not rise by exactly 2");
                if (value("giftTierControl") != 1 + level / 5 || value("giftTierInvested") != 1 + (level + 2) / 5)
                    failures.Add("the audited Gift of Claw and Horn tier did not follow its counter");
                // Owned-power thresholds move with the effective level:
                // Battlecry's +2 step (1 + L/10, max 2), Spirit Shield's
                // 13th-level tier (1 + L/13) and Erase From Time's 11th-level use.
                if (value("battlecryStepControl") != 1 + level / 10 ||
                    value("battlecryStepInvested") != Math.Min(2, 1 + (level + 2) / 10))
                    failures.Add("the Battlecry step did not follow the effective level");
                if (value("spiritShieldTierAt12") != 1 || value("spiritShieldTierAt12Invested") != 2)
                    failures.Add("the 13th-level Spirit Shield tier did not follow the effective level");
                if (value("eraseFromTimeUsesAt10") != 1 || value("eraseFromTimeUsesAt10Invested") != 2)
                    failures.Add("Erase From Time's 11th-level use did not follow the effective level");

                // A persistent feature context refreshes when the leaf is gained and removed.
                BlueprintFeature boost = revelation("SpiritBoost");
                Func<UnitEntityData, int> boostRank = unit =>
                {
                    var fact = unit.Descriptor.Progression.Features.GetFact(boost);
                    return fact == null || fact.MaybeContext == null ? -1 : fact.MaybeContext[AbilityRankType.Default];
                };
                int boostBefore = boostRank(breath);
                GrantFavoredClassRanks(breath, full("SpiritBoost"), 1);
                int boostAfter = boostRank(breath);
                breath.Descriptor.Progression.Features.RemoveFact(full("SpiritBoost"));
                int boostRemoved = boostRank(breath);
                // Removing the chosen counter restores the level-9 values.
                while (breath.Descriptor.Progression.Features.GetFact(full("FireBreath")) != null)
                    breath.Descriptor.Progression.Features.RemoveFact(full("FireBreath"));
                JObject breathRemoved = ability(breath, FcbFireBreathAbilityGuid);
                int usesRemoved = uses(breath, FcbFireBreathResourceGuid);
                result["refresh"] = new JObject
                {
                    ["spiritBoostBefore"] = boostBefore,
                    ["spiritBoostAfterGain"] = boostAfter,
                    ["spiritBoostAfterRemoval"] = boostRemoved,
                    ["fireBreathAfterRemoval"] = breathRemoved,
                    ["fireBreathUsesAfterRemoval"] = usesRemoved
                };
                if (boostBefore != level || boostAfter != level + 1 || boostRemoved != level)
                    failures.Add("the Spirit Boost feature context did not refresh on gain and removal");
                foreach (string key in new[] { "casterLevel", "dc", "default" })
                    if (delta(breathRemoved, breathControl, key) != 0)
                        failures.Add("removing the Fire Breath counter left " + key + " changed");
                if (usesRemoved != (int)resources["fireBreathControl"])
                    failures.Add("removing the Fire Breath counter left its uses changed");
            }
            catch (Exception exception)
            {
                failures.Add("probe: " + exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                foreach (UnitEntityData unit in units)
                    try { unit.Dispose(); } catch (Exception) { }
            }
            return result;
        }

        // Charter 8.10 (review finding 2): an owned revelation's own level
        // gates follow its effective level; no other gate, revelation, BAB,
        // save or class level moves, and the excluded target is never offered.
        private JObject ObserveRevelationGates(BlueprintFeatureSelection bonus, FavoredClassBlueprintSet leaves,
            IList<string> failures)
        {
            var library = BlueprintBootstrap.Library;
            string effect = FavoredClassCatalog.EffectSelectedRevelation;
            BlueprintCharacterClass oracle = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FavoredClassRevelationManifest.OracleClassGuid, "Oracle");
            var rows = new JArray();
            var units = new List<UnitEntityData>();
            int tested = 0, moved = 0, skipped = 0;
            var result = new JObject();
            Func<int, UnitEntityData> oracleAt = levels =>
            {
                UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(
                    BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
                units.Add(unit);
                for (int added = 0; added < levels; added++)
                    unit.Descriptor.Progression.AddClassLevel(oracle);
                return unit;
            };
            Action<UnitEntityData, BlueprintFeature> remove = (unit, leaf) =>
            {
                while (unit.Descriptor.Progression.Features.GetFact(leaf) != null)
                    unit.Descriptor.Progression.Features.RemoveFact(leaf);
            };
            try
            {
                foreach (FavoredClassRevelationTarget target in FavoredClassRevelationManifest.All)
                {
                    FavoredClassRevelationScope scope = FavoredClassRevelationScopes.ForKey(target.Key);
                    if (scope == null || !target.Published)
                        continue;
                    BlueprintFeature leaf = leaves.Pair(effect, target.Key).Full;
                    foreach (FavoredClassRevelationGate gate in scope.Gates)
                    {
                        var row = new JObject { ["target"] = target.Key, ["gate"] = gate.Label };
                        rows.Add(row);
                        var owner = gate.Owner as BlueprintFeature;
                        if (owner == null || !target.FeatureGuids.Contains(owner.AssetGuid) || gate.Level < 2 ||
                            gate.Feature == null)
                        {
                            // A gate on a deeper feature is decided the same way
                            // once that feature is present; the fixture adds the
                            // revelation's own selectable features only.
                            row["skipped"] = "the gate's owner is not a selectable revelation feature";
                            skipped++;
                            continue;
                        }
                        int level = gate.Level - 1;
                        UnitEntityData unit = oracleAt(level);
                        unit.Descriptor.AddFact(owner);
                        bool below = unit.Descriptor.HasFact(gate.Feature);
                        GrantFavoredClassRanks(unit, leaf, 1);
                        bool invested = unit.Descriptor.HasFact(gate.Feature);
                        remove(unit, leaf);
                        bool restored = unit.Descriptor.HasFact(gate.Feature);
                        bool nativeBelow = FavoredClassMechanicsPolicy.GateApplies(level, gate.Level, gate.BeforeThisLevel);
                        bool nativeNext = FavoredClassMechanicsPolicy.GateApplies(level + 1, gate.Level,
                            gate.BeforeThisLevel);
                        row["level"] = level;
                        row["below"] = below;
                        row["invested"] = invested;
                        row["restored"] = restored;
                        tested++;
                        if (below != nativeBelow || invested != nativeNext || restored != nativeBelow)
                            failures.Add(target.Key + " " + gate.Label + ": below=" + below + " invested=" + invested +
                                " restored=" + restored);
                        else if (below != invested)
                            moved++;
                    }
                }
                // Neighbor, BAB, saves and class level: an Oracle 10 with two
                // gated revelations invests only in Touch of Flame.
                FavoredClassRevelationScope flame = FavoredClassRevelationScopes.ForKey("TouchOfFlame");
                FavoredClassRevelationScope shock = FavoredClassRevelationScopes.ForKey("TouchOfElectricity");
                FavoredClassRevelationGate flameGate = flame == null ? null :
                    flame.Gates.FirstOrDefault(gate => gate.Level == 11 && !gate.BeforeThisLevel);
                FavoredClassRevelationGate shockGate = shock == null ? null :
                    shock.Gates.FirstOrDefault(gate => gate.Level == 11 && !gate.BeforeThisLevel);
                if (flameGate == null || shockGate == null)
                    failures.Add("the Touch of Flame and Touch of Electricity 11th-level gates were not scoped");
                else
                {
                    Func<UnitEntityData> pair = () =>
                    {
                        UnitEntityData unit = oracleAt(10);
                        unit.Descriptor.AddFact((BlueprintFeature)flameGate.Owner);
                        unit.Descriptor.AddFact((BlueprintFeature)shockGate.Owner);
                        return unit;
                    };
                    UnitEntityData control = pair();
                    UnitEntityData invested = pair();
                    GrantFavoredClassRanks(invested, leaves.Pair(effect, "TouchOfFlame").Full, 1);
                    Func<UnitEntityData, JObject> sheet = unit => new JObject
                    {
                        ["oracleLevel"] = unit.Descriptor.Progression.GetClassLevel(oracle),
                        ["characterLevel"] = unit.Descriptor.Progression.CharacterLevel,
                        ["bab"] = unit.Stats.BaseAttackBonus.ModifiedValue,
                        ["fortitude"] = unit.Stats.SaveFortitude.ModifiedValue,
                        ["reflex"] = unit.Stats.SaveReflex.ModifiedValue,
                        ["will"] = unit.Stats.SaveWill.ModifiedValue,
                        ["flamingWeapon"] = unit.Descriptor.HasFact(flameGate.Feature),
                        ["shockWeapon"] = unit.Descriptor.HasFact(shockGate.Feature)
                    };
                    JObject controlSheet = sheet(control), investedSheet = sheet(invested);
                    result["neighbor"] = new JObject { ["control"] = controlSheet, ["invested"] = investedSheet };
                    if ((bool)controlSheet["flamingWeapon"] || !(bool)investedSheet["flamingWeapon"])
                        failures.Add("one Touch of Flame step did not bring its 11th-level flaming weapon to an Oracle 10");
                    if ((bool)controlSheet["shockWeapon"] || (bool)investedSheet["shockWeapon"])
                        failures.Add("the neighbor Touch of Electricity gate moved");
                    foreach (string key in new[] { "oracleLevel", "characterLevel", "bab", "fortitude", "reflex", "will" })
                        if ((int)controlSheet[key] != (int)investedSheet[key])
                            failures.Add("an invested revelation changed " + key);
                }
                // The excluded target is registered but never published.
                FavoredClassLeafPair warrior = leaves.Pair(effect, "SpiritOfTheWarrior");
                FavoredClassPublication publication = FavoredClassIntegrationCoordinator.Publication;
                bool skippedWarrior = publication != null && publication.Skipped.Any(value =>
                    value.EndsWith("excluded-target:SpiritOfTheWarrior", StringComparison.Ordinal));
                bool offeredWarrior = (bonus.AllFeatures ?? new BlueprintFeature[0]).Any(feature =>
                    ReferenceEquals(feature, warrior.Full) || ReferenceEquals(feature, warrior.Partial));
                result["spiritOfTheWarrior"] = new JObject
                {
                    ["registered"] = warrior.Full != null,
                    ["skipped"] = skippedWarrior,
                    ["offered"] = offeredWarrior
                };
                if (warrior.Full == null || !skippedWarrior || offeredWarrior)
                    failures.Add("Spirit of the Warrior was not registered-but-excluded");
                if (tested == 0 || moved == 0)
                    failures.Add("no revelation gate was exercised");
            }
            catch (Exception exception)
            {
                failures.Add("probe: " + exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                foreach (UnitEntityData unit in units)
                    try { unit.Dispose(); } catch (Exception) { }
            }
            result["summary"] = new JObject { ["tested"] = tested, ["moved"] = moved, ["skipped"] = skipped };
            result["gates"] = rows;
            return result;
        }
    }
}
