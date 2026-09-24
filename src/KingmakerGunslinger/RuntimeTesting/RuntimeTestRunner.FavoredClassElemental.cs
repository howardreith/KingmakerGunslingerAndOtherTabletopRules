using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbBombStandardGuid = "5fa0111ac60ed194db82d3110a9d0352";
        private const string FcbAcidBombBuffGuid = "4e15565b830d4b841b59e701d3395371";
        private const string FcbFireballGuid = "2d81362af43aeac4387a3d4fced489c3";
        private const string FcbPersuasionUseAbilityGuid = "7d2233c3b7a0b984ba058a83b736e6ac";
        private const string FcbMagicMissileGuid = "4ac47ddb9fa1eaf43a1b6809980cfbd2";
        private const string FcbLongswordGuid = "6fd0a849531617844b195f452661b2cd";

        /// <summary>Records every Intimidate skill check's final bonus.</summary>
        private sealed class FavoredClassSkillCheckObserver : IGlobalRulebookHandler<RuleSkillCheck>
        {
            internal readonly List<Tuple<UnitEntityData, int>> Checks = new List<Tuple<UnitEntityData, int>>();

            public void OnEventAboutToTrigger(RuleSkillCheck evt) { }

            public void OnEventDidTrigger(RuleSkillCheck evt)
            {
                if (evt != null && evt.StatType == StatType.CheckIntimidate)
                    Checks.Add(Tuple.Create(evt.Initiator, (int)evt.Bonus));
            }
        }

        // Phase 3 simple rows: I01, I05, I07, O04, O05, U02, U04 through real
        // per-class host menus, native level-ups and native rules, with
        // negative races, archetype replacement and two-owner controls.
        private RuntimeTestResult RunFavoredClassElementalCore()
        {
            var assertions = new List<RuntimeTestAssertion>();
            FavoredClassIntegrationStatus status = FavoredClassIntegrationStatusRegistry.Current;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            bool ready = status.Availability == FavoredClassIntegrationAvailability.Published &&
                host != null && leaves != null && FavoredClassRuntime.MechanicsEnabled;
            assertions.Add(Assertion("fcb-integration-published",
                "the exact qualified host is ready, owned leaves are published and mechanics are enabled",
                status.ToString(), ready, "FavoredClassIntegrationStatusRegistry after the first update"));
            if (!ready)
                return CreateResult(RuntimeTestStatuses.Fail, assertions, null);
            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);

            var evidence = new JObject();
            var menuFailures = new List<string>();
            var progressionFailures = new List<string>();
            var mechanicsFailures = new List<string>();
            var disposables = new List<UnitEntityData>();
            bool cleaned = false;
            try
            {
                evidence["menus"] = RunElementalMenus(host, leaves, menuFailures);
                evidence["progressions"] = RunElementalProgressions(host, leaves, progressionFailures);
                BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
                Func<UnitEntityData> create = () =>
                {
                    UnitEntityData unit = new Kingmaker.UI.LevelUp.ChargenUnit(source).Unit;
                    disposables.Add(unit);
                    return unit;
                };
                evidence["mechanics"] = ObserveElementalMechanics(create, leaves, mechanicsFailures);
            }
            catch (Exception exception)
            {
                mechanicsFailures.Add("exception=" + exception);
            }
            finally
            {
                foreach (UnitEntityData unit in disposables)
                {
                    try
                    {
                        if (unit.Body != null && unit.Body.PrimaryHand.MaybeItem != null)
                            unit.Body.PrimaryHand.RemoveItem(false);
                        unit.Dispose();
                    }
                    catch (Exception) { }
                }
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }
            string evidencePath = WriteFavoredClassEvidence("favored-class-elemental-core.json", evidence);
            assertions.Add(Assertion("fcb-elemental-class-menus",
                "each class's own host menu offers exactly its adopted geniekin counter to that ancestry (Ifrit Alchemist/Inquisitor/Rogue, Oread Fighter/Monk, Undine Monk/Cleric), never to other ancestries, and not to a Vivisectionist",
                Describe(evidence["menus"], menuFailures), menuFailures.Count == 0,
                "level-1 native visit per race and class; BlueprintFeatureSelection.CanSelect"));
            assertions.Add(Assertion("fcb-elemental-native-progressions",
                "native level-ups bank the mixed-rate Undine Monk counter (every investment +1 grapple CMD, every third +1 Stunning Fist use) and the divisor-one Oread Fighter counter (+1 bull rush CMD each) over lockstep controls",
                Describe(evidence["progressions"], progressionFailures), progressionFailures.Count == 0,
                "LevelUpController visits; RuleCalculateCMD; BlueprintAbilityResource.GetMaxAmount"));
            assertions.Add(Assertion("fcb-elemental-native-mechanics",
                "bomb damage only on bomb damage (not buff follow-ups or other abilities), Intimidate only against fire creatures (I05) or when demoralizing (I07), unarmed confirmation only on unarmed strikes with the Critical Focus comparison, spell penetration only against aquatic/water creatures; two owners keep their own values",
                Describe(evidence["mechanics"], mechanicsFailures), mechanicsFailures.Count == 0,
                "native RuleDealDamage, Demoralize action, RuleAttackRoll, RuleSpellResistanceCheck"));
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

        private JArray RunElementalMenus(FavoredClassHostHandles host, FavoredClassBlueprintSet leaves,
            IList<string> failures)
        {
            BlueprintArchetype vivisectionist = BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(
                BlueprintBootstrap.Library, "68cbcd9fbf1fb1d489562f829bb97e38", "Vivisectionist");
            var cases = new[]
            {
                Tuple.Create(FavoredClassAncestry.Ifrit, FavoredClassCatalog.Alchemist, (BlueprintArchetype)null,
                    new[] { FavoredClassCatalog.EffectBombDamage }),
                Tuple.Create(FavoredClassAncestry.Human, FavoredClassCatalog.Alchemist, (BlueprintArchetype)null,
                    new string[0]),
                Tuple.Create(FavoredClassAncestry.Ifrit, FavoredClassCatalog.Alchemist, vivisectionist,
                    new string[0]),
                Tuple.Create(FavoredClassAncestry.Ifrit, FavoredClassCatalog.Inquisitor, (BlueprintArchetype)null,
                    new[] { FavoredClassCatalog.EffectFireIntimidate }),
                Tuple.Create(FavoredClassAncestry.Ifrit, FavoredClassCatalog.Rogue, (BlueprintArchetype)null,
                    new[] { FavoredClassCatalog.EffectDemoralize }),
                Tuple.Create(FavoredClassAncestry.Oread, FavoredClassCatalog.Fighter, (BlueprintArchetype)null,
                    new[] { FavoredClassCatalog.EffectBullRushDragDefense }),
                Tuple.Create(FavoredClassAncestry.Oread, FavoredClassCatalog.Monk, (BlueprintArchetype)null,
                    new[] { FavoredClassCatalog.EffectUnarmedConfirmation }),
                Tuple.Create(FavoredClassAncestry.Undine, FavoredClassCatalog.Monk, (BlueprintArchetype)null,
                    new[] { FavoredClassCatalog.EffectGrappleStunning }),
                Tuple.Create(FavoredClassAncestry.Undine, FavoredClassCatalog.Cleric, (BlueprintArchetype)null,
                    new[] { FavoredClassCatalog.EffectAquaticPenetration }),
                Tuple.Create(FavoredClassAncestry.Oread, FavoredClassCatalog.Cleric, (BlueprintArchetype)null,
                    new string[0]),
            };
            var rows = new JArray();
            foreach (var entry in cases)
            {
                var row = new JObject { ["ancestry"] = entry.Item1, ["class"] = entry.Item2,
                    ["archetype"] = entry.Item3 == null ? null : entry.Item3.name };
                UnitEntityData unit = null;
                LevelUpController controller = null;
                try
                {
                    FavoredClassLeafPair[] classPairs = leaves.Pairs.Where(pair =>
                        pair.Effect.ClassFamily == entry.Item2).ToArray();
                    BlueprintCharacterClass characterClass = BlueprintLibraryLookup
                        .RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                            classPairs[0].HostClassGuid, entry.Item2);
                    BlueprintFeatureSelection selection = host.BonusSelectionFor(characterClass.AssetGuid);
                    BlueprintRace race = BlueprintLibraryLookup.RequireExact<BlueprintRace>(
                        BlueprintBootstrap.Library, FavoredClassRaceIdentities.All.Single(identity =>
                            identity.Ancestry == entry.Item1).RaceGuid, entry.Item1);
                    unit = FavoredClassLevelUpHarness.CreateUnit(14);
                    controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, characterClass,
                        "KMG FCB Elemental Menu", entry.Item3);
                    if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, characterClass, row) == null)
                    {
                        failures.Add(entry.Item1 + "/" + entry.Item2 + ": favored class progression unavailable");
                        rows.Add(row);
                        continue;
                    }
                    FavoredClassLevelUpHarness.FillOthers(controller,
                        new HashSet<string>(StringComparer.Ordinal) { selection.AssetGuid });
                    FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                        selection.AssetGuid);
                    if (fcb == null)
                    {
                        failures.Add(entry.Item1 + "/" + entry.Item2 + ": no favored-class state");
                        rows.Add(row);
                        continue;
                    }
                    var offered = classPairs.Where(pair =>
                        FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Full) ||
                        (pair.Partial != null && FavoredClassLevelUpHarness.CanSelect(controller, fcb, pair.Partial)))
                        .Select(pair => pair.Effect.Id).Distinct().ToArray();
                    row["offered"] = new JArray(offered);
                    if (!offered.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                            entry.Item4.OrderBy(value => value, StringComparer.Ordinal)))
                        failures.Add(entry.Item1 + "/" + entry.Item2 + (entry.Item3 == null ? "" : "/" +
                            entry.Item3.name) + ": offered " + string.Join(",", offered) + " expected " +
                            string.Join(",", entry.Item4));
                }
                catch (Exception exception)
                {
                    failures.Add(entry.Item1 + "/" + entry.Item2 + ": " + exception.GetType().Name + ": " +
                        exception.Message);
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(controller);
                    if (unit != null) unit.Dispose();
                }
                rows.Add(row);
            }
            return rows;
        }

        private JObject RunElementalProgressions(FavoredClassHostHandles host, FavoredClassBlueprintSet leaves,
            IList<string> failures)
        {
            var result = new JObject();
            FavoredClassLeafPair grapple = leaves.Pair(FavoredClassCatalog.EffectGrappleStunning, null);
            FavoredClassLeafPair bullRush = leaves.Pair(FavoredClassCatalog.EffectBullRushDragDefense, null);
            BlueprintAbilityResource stunning = BlueprintLibraryLookup.RequireExact<BlueprintAbilityResource>(
                BlueprintBootstrap.Library, FavoredClassBlueprints.StunningFistResourceGuid, "Stunning Fist resource");
            result["undineMonk"] = RunLockstepProgression("undine monk", FavoredClassAncestry.Undine,
                grapple, 6, host, (test, control, target) =>
                {
                    var row = new JObject();
                    int cmd = CmdAgainst(target, test, CombatManeuver.Grapple) -
                        CmdAgainst(target, control, CombatManeuver.Grapple);
                    int bullRushDelta = CmdAgainst(target, test, CombatManeuver.BullRush) -
                        CmdAgainst(target, control, CombatManeuver.BullRush);
                    int uses = stunning.GetMaxAmount(test.Descriptor) - stunning.GetMaxAmount(control.Descriptor);
                    row["grappleCmdDelta"] = cmd;
                    row["bullRushCmdDelta"] = bullRushDelta;
                    row["stunningUsesDelta"] = uses;
                    row["ranks"] = FavoredClassLevelUpHarness.Rank(test.Descriptor, grapple.Full) + "/" +
                        FavoredClassLevelUpHarness.Rank(test.Descriptor, grapple.Partial);
                    if (cmd != 6) failures.Add("undine monk grapple CMD delta " + cmd + " expected 6");
                    if (bullRushDelta != 0) failures.Add("undine monk bull rush CMD changed");
                    if (uses != 2) failures.Add("undine monk Stunning Fist delta " + uses + " expected 2");
                    return row;
                }, failures);
            result["oreadFighter"] = RunLockstepProgression("oread fighter", FavoredClassAncestry.Oread,
                bullRush, 3, host, (test, control, target) =>
                {
                    var row = new JObject();
                    int cmd = CmdAgainst(target, test, CombatManeuver.BullRush) -
                        CmdAgainst(target, control, CombatManeuver.BullRush);
                    int trip = CmdAgainst(target, test, CombatManeuver.Trip) -
                        CmdAgainst(target, control, CombatManeuver.Trip);
                    row["bullRushCmdDelta"] = cmd;
                    row["tripCmdDelta"] = trip;
                    row["rank"] = FavoredClassLevelUpHarness.Rank(test.Descriptor, bullRush.Full);
                    if (cmd != 3) failures.Add("oread fighter bull rush CMD delta " + cmd + " expected 3");
                    if (trip != 0) failures.Add("oread fighter trip CMD changed");
                    return row;
                }, failures);
            return result;
        }

        private static int CmdAgainst(UnitEntityData attacker, UnitEntityData defender, CombatManeuver maneuver)
        {
            return Rulebook.Trigger(new RuleCalculateCMD(attacker, defender, maneuver)).Result;
        }

        private JObject RunLockstepProgression(string label, string ancestry, FavoredClassLeafPair pair,
            int levels, FavoredClassHostHandles host,
            Func<UnitEntityData, UnitEntityData, UnitEntityData, JObject> observe, IList<string> failures)
        {
            var result = new JObject();
            BlueprintCharacterClass characterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                BlueprintBootstrap.Library, pair.HostClassGuid, label);
            BlueprintFeatureSelection selection = host.BonusSelectionFor(characterClass.AssetGuid);
            BlueprintRace race = BlueprintLibraryLookup.RequireExact<BlueprintRace>(BlueprintBootstrap.Library,
                FavoredClassRaceIdentities.All.Single(identity => identity.Ancestry == ancestry).RaceGuid, ancestry);
            var reserved = new HashSet<string>(StringComparer.Ordinal) { selection.AssetGuid };
            UnitEntityData test = null, control = null, target = null;
            try
            {
                test = FavoredClassLevelUpHarness.CreateUnit(14);
                control = FavoredClassLevelUpHarness.CreateUnit(14);
                target = FavoredClassLevelUpHarness.CreateUnit(10);
                var rows = new JArray();
                for (int level = 1; level <= levels; level++)
                {
                    int before = FavoredClassLevelUpHarness.Rank(test.Descriptor, pair.Full) +
                        FavoredClassLevelUpHarness.Rank(test.Descriptor, pair.Partial);
                    bool full = FavoredClassRankPolicy.NextInvestmentIsFull(before, pair.Effect.Rate.Divisor);
                    var row = new JObject { ["level"] = level };
                    foreach (bool isTest in new[] { true, false })
                    {
                        UnitEntityData unit = isTest ? test : control;
                        BlueprintFeature choice = isTest ? (full ? pair.Full : pair.Partial) : host.GenericHitPoint;
                        var visit = new JObject();
                        LevelUpController controller = null;
                        try
                        {
                            controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, characterClass,
                                "KMG FCB " + label, null);
                            if (level == 1 && FavoredClassLevelUpHarness.ChooseFavoredClass(controller,
                                    characterClass, visit) == null)
                                failures.Add(label + ": favored class unavailable");
                            FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                            FeatureSelectionState fcb = FavoredClassLevelUpHarness.FindOpenState(controller,
                                selection.AssetGuid);
                            if (fcb == null || !FavoredClassLevelUpHarness.Select(controller, fcb, choice))
                                failures.Add(Fmt("{0} level {1}: could not take {2}", label, level, choice.name));
                            FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                            if (!FavoredClassLevelUpHarness.Confirm(controller, unit.Descriptor, visit))
                                failures.Add(Fmt("{0} level {1}: incomplete {2}", label, level, visit["completion"]));
                        }
                        finally
                        {
                            FavoredClassLevelUpHarness.Close(controller);
                        }
                        row[isTest ? "test" : "control"] = visit;
                    }
                    rows.Add(row);
                }
                result["levels"] = rows;
                result["observed"] = observe(test, control, target);
            }
            catch (Exception exception)
            {
                failures.Add(label + ": exception=" + exception);
            }
            finally
            {
                if (test != null) test.Dispose();
                if (control != null) control.Dispose();
                if (target != null) target.Dispose();
            }
            return result;
        }

        private static JObject ObserveElementalMechanics(Func<UnitEntityData> create,
            FavoredClassBlueprintSet leaves, IList<string> failures)
        {
            var row = new JObject();
            var library = BlueprintBootstrap.Library;
            UnitEntityData target = create();
            target.Descriptor.State.Immortality.Retain();
            target.Descriptor.Stats.HitPoints.BaseValue = 1000;

            // I01: flat bomb damage only from the bomb lineage's own ability context.
            UnitEntityData control = create();
            UnitEntityData bomber = create();
            UnitEntityData bomberTwo = create();
            GrantFavoredClassRanks(bomber, leaves.Pair(FavoredClassCatalog.EffectBombDamage, null).Full, 3);
            GrantFavoredClassRanks(bomberTwo, leaves.Pair(FavoredClassCatalog.EffectBombDamage, null).Full, 1);
            var bomb = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, FcbBombStandardGuid, "BombStandart");
            var acidBuff = BlueprintLibraryLookup.RequireExact<BlueprintScriptableObject>(library, FcbAcidBombBuffGuid,
                "AcidBombBuff");
            var fireball = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, FcbFireballGuid, "Fireball");
            Func<UnitEntityData, BlueprintScriptableObject, int> damage = (caster, blueprint) =>
            {
                var context = new MechanicsContext(caster, caster.Descriptor, blueprint);
                var deal = context.TriggerRule(new RuleDealDamage(caster, target, new DamageBundle(
                    new EnergyDamage(DiceFormula.Zero, DamageEnergyType.Fire))));
                return deal.Damage;
            };
            row["bombControl"] = damage(control, bomb);
            row["bombRank3"] = damage(bomber, bomb);
            row["bombRank1OtherOwner"] = damage(bomberTwo, bomb);
            row["acidBuffFollowUpRank3"] = damage(bomber, acidBuff);
            row["fireballRank3"] = damage(bomber, fireball);
            if ((int)row["bombRank3"] - (int)row["bombControl"] != 3)
                failures.Add("three bomb steps did not add +3 damage");
            if ((int)row["bombRank1OtherOwner"] - (int)row["bombControl"] != 1)
                failures.Add("a second owner did not keep its own +1");
            if ((int)row["acidBuffFollowUpRank3"] != (int)row["bombControl"])
                failures.Add("a bomb buff's follow-up damage received the bonus");
            if ((int)row["fireballRank3"] != (int)row["bombControl"])
                failures.Add("a non-bomb ability received the bonus");

            // I05 / I07: the native Demoralize action and a non-demoralize check.
            var persuasion = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                FcbPersuasionUseAbilityGuid, "PersuasionUseAbility");
            Demoralize demoralize = FindAction<Demoralize>(persuasion);
            row["demoralizeElementFound"] = demoralize != null;
            if (demoralize == null)
            {
                failures.Add("the native Demoralize element was not found");
                return row;
            }
            var fireSubtype = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FavoredClassBlueprints.SubtypeFireGuid, "SubtypeFire");
            UnitEntityData fireTarget = create();
            fireTarget.Descriptor.AddFact(fireSubtype);
            UnitEntityData inquisitor = create();
            UnitEntityData rogue = create();
            GrantFavoredClassRanks(inquisitor, leaves.Pair(FavoredClassCatalog.EffectFireIntimidate, null).Full, 2);
            GrantFavoredClassRanks(rogue, leaves.Pair(FavoredClassCatalog.EffectDemoralize, null).Full, 2);
            var observer = new FavoredClassSkillCheckObserver();
            EventBus.Subscribe(observer);
            Func<UnitEntityData, UnitEntityData, bool, int> intimidate = (caster, victim, viaDemoralize) =>
            {
                observer.Checks.Clear();
                var context = new MechanicsContext(caster, caster.Descriptor, persuasion);
                using (context.GetDataScope(new TargetWrapper(victim)))
                {
                    if (viaDemoralize)
                        demoralize.RunAction();
                    else
                        context.TriggerRule(new RuleSkillCheck(caster, StatType.CheckIntimidate, 10));
                }
                Tuple<UnitEntityData, int> check = observer.Checks.LastOrDefault(value =>
                    ReferenceEquals(value.Item1, caster));
                return check == null ? int.MinValue : check.Item2;
            };
            try
            {
                row["controlDemoralizeFire"] = intimidate(control, fireTarget, true);
                row["inquisitorDemoralizeFire"] = intimidate(inquisitor, fireTarget, true);
                row["inquisitorDemoralizePlain"] = intimidate(inquisitor, target, true);
                row["rogueDemoralizePlain"] = intimidate(rogue, target, true);
                row["rogueOtherIntimidatePlain"] = intimidate(rogue, target, false);
                row["controlDemoralizePlain"] = intimidate(control, target, true);
            }
            finally
            {
                EventBus.Unsubscribe(observer);
            }
            int baseFire = (int)row["controlDemoralizeFire"];
            int basePlain = (int)row["controlDemoralizePlain"];
            if (baseFire == int.MinValue || basePlain == int.MinValue)
                failures.Add("a demoralize action raised no Intimidate check");
            if ((int)row["inquisitorDemoralizeFire"] - baseFire != 2)
                failures.Add("the Inquisitor bonus against a fire creature is not +2");
            if ((int)row["inquisitorDemoralizePlain"] != basePlain)
                failures.Add("the Inquisitor bonus reached a creature without the fire subtype");
            if ((int)row["rogueDemoralizePlain"] - basePlain != 2)
                failures.Add("the Rogue bonus did not apply to demoralize");
            if ((int)row["rogueOtherIntimidatePlain"] != basePlain)
                failures.Add("the Rogue bonus reached an Intimidate check that is not a demoralize");

            // O05: unarmed strikes only, with the Critical Focus comparison.
            var criticalFocus = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FavoredClassBlueprints.CriticalFocusGuid, "Critical Focus");
            var longsword = BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(library, FcbLongswordGuid, "Longsword");
            UnitEntityData monk = create();
            UnitEntityData monkControl = create();
            GrantFavoredClassRanks(monk, leaves.Pair(FavoredClassCatalog.EffectUnarmedConfirmation, null).Full, 5);
            Func<UnitEntityData, ItemEntityWeapon, int> confirm = (attacker, weapon) =>
            {
                var roll = new RuleAttackRoll(attacker, target, weapon, 0);
                Rulebook.Trigger(roll);
                return roll.CriticalConfirmationBonus;
            };
            row["unarmedControl"] = confirm(monkControl, monkControl.Body.EmptyHandWeapon);
            row["unarmedRank5"] = confirm(monk, monk.Body.EmptyHandWeapon);
            var sword = new ItemEntityWeapon(longsword);
            row["longswordRank5"] = confirm(monk, sword);
            monk.Descriptor.AddFact(criticalFocus);
            monkControl.Descriptor.AddFact(criticalFocus);
            row["unarmedFocusControl"] = confirm(monkControl, monkControl.Body.EmptyHandWeapon);
            row["unarmedFocusRank5"] = confirm(monk, monk.Body.EmptyHandWeapon);
            if ((int)row["unarmedRank5"] - (int)row["unarmedControl"] != 5)
                failures.Add("five unarmed steps did not add +5 confirmation");
            if ((int)row["longswordRank5"] != (int)row["unarmedControl"])
                failures.Add("a longsword received the unarmed confirmation bonus");
            int focus = (int)row["unarmedFocusControl"] - (int)row["unarmedControl"];
            if (focus <= 0 || (int)row["unarmedFocusRank5"] - (int)row["unarmedControl"] != Math.Max(5, focus))
                failures.Add("with Critical Focus the unarmed total is not the better of the two");

            // U02: spell penetration only against aquatic or water creatures.
            var magicMissile = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, FcbMagicMissileGuid,
                "MagicMissile");
            UnitEntityData cleric = create();
            GrantFavoredClassRanks(cleric, leaves.Pair(FavoredClassCatalog.EffectAquaticPenetration, null).Full, 3);
            UnitEntityData waterTarget = create();
            waterTarget.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FavoredClassBlueprints.SubtypeWaterGuid, "SubtypeWater"));
            UnitEntityData aquaticTarget = create();
            aquaticTarget.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FavoredClassBlueprints.SubtypeAquaticGuid, "SubtypeAquatic"));
            Func<UnitEntityData, UnitEntityData, int> penetration = (caster, victim) =>
            {
                var context = new MechanicsContext(caster, caster.Descriptor, magicMissile);
                return Rulebook.Trigger(new RuleSpellResistanceCheck(context, victim)).AdditionalSpellPenetration;
            };
            row["penetrationWater"] = penetration(cleric, waterTarget);
            row["penetrationAquatic"] = penetration(cleric, aquaticTarget);
            row["penetrationPlain"] = penetration(cleric, target);
            row["penetrationControlWater"] = penetration(control, waterTarget);
            if ((int)row["penetrationWater"] != 3 || (int)row["penetrationAquatic"] != 3)
                failures.Add("three ranks did not add +3 against water and aquatic creatures");
            if ((int)row["penetrationPlain"] != 0 || (int)row["penetrationControlWater"] != 0)
                failures.Add("spell penetration changed for another creature or another caster");
            return row;
        }

        private static T FindAction<T>(BlueprintAbility ability) where T : GameAction
        {
            AbilityEffectRunAction run = ability.GetComponent<AbilityEffectRunAction>();
            return run == null || run.Actions == null ? null : FindAction<T>(run.Actions.Actions, 0);
        }

        private static T FindAction<T>(GameAction[] actions, int depth) where T : GameAction
        {
            if (actions == null || depth > 6)
                return null;
            foreach (GameAction action in actions)
            {
                if (action is T)
                    return (T)action;
                if (action == null)
                    continue;
                foreach (FieldInfo field in action.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    var list = field.GetValue(action) as ActionList;
                    T nested = list == null ? null : FindAction<T>(list.Actions, depth + 1);
                    if (nested != null)
                        return nested;
                }
            }
            return null;
        }
    }
}
