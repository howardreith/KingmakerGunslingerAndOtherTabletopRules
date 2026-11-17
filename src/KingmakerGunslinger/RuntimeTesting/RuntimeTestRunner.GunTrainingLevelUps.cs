using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Classes;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// DEF-04 / S4 T1: the Gun Training opportunities as actual level-up
    /// transactions, kept apart from the scenario's direct-fact controls.
    ///
    /// Every character here is a detached chargen unit levelled through real
    /// LevelUpController visits (LevelUpState.IsComplete, then ApplyLevelup),
    /// with every choice other than Gun Training filled deterministically. The
    /// two legacy states are constructed: a hidden Rifle Gun Training fact is
    /// added directly to stand for a character who trained it when it was
    /// still offered. They are compatibility controls, not an old save.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const int GunTrainingDexterity = 18;

        private sealed class GunTrainingVisit
        {
            internal bool Open;
            internal bool Required;
            internal string[] Selectable = new string[0];
            internal string Picked;
            internal bool Completed;
            internal JObject Evidence = new JObject();
        }

        private List<RuntimeTestAssertion> ObserveGunTrainingLevelUps(ICollection<string> evidenceFiles)
        {
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            GunTrainingBlueprintSet training = gunslinger.GunTraining;
            BlueprintRace human = BlueprintLibraryLookup.RequireExact<BlueprintRace>(
                BlueprintBootstrap.Library, FcbHumanRace, "native Human");
            var reserved = new HashSet<string>(StringComparer.Ordinal) { training.Selection.AssetGuid };
            BlueprintFeature pistol = training.ChoiceFor(FirearmKind.Pistol);
            BlueprintFeature musket = training.ChoiceFor(FirearmKind.Musket);
            BlueprintFeature blunderbuss = training.ChoiceFor(FirearmKind.Blunderbuss);
            BlueprintFeature rifle = training.ChoiceFor(FirearmKind.Rifle);
            BlueprintFeature revolver = training.ChoiceFor(FirearmKind.Revolver);
            BlueprintFeature[] official = { pistol, musket, blunderbuss };
            BlueprintFeature[] all = { pistol, musket, blunderbuss, rifle, revolver };

            object player = ReadExactMember(Kingmaker.Game.Instance, "Player");
            object state = ReadExactMember(Kingmaker.Game.Instance, "State");
            object party = ReadExactMember(player, "Party");
            object allUnits = ReadExactMember(state, "AllUnits");
            object[] partyBefore = SnapshotReferences(party);
            object[] unitsBefore = SnapshotReferences(allUnits);

            var evidence = new JObject
            {
                ["method"] = "detached chargen units; real LevelUpController visits through FavoredClassLevelUpHarness; " +
                    "LevelUpState.IsComplete gate then ApplyLevelup; Gun Training reserved and chosen explicitly, " +
                    "every other choice filled deterministically",
                ["dexterity"] = GunTrainingDexterity
            };
            var baseFailures = new List<string>();
            var cancelFailures = new List<string>();
            var strangerFailures = new List<string>();
            var legacyFailures = new List<string>();
            var rankedFailures = new List<string>();
            var presentationFailures = new List<string>();
            bool cleaned = false;
            var created = new List<UnitEntityData>();
            Func<UnitEntityData> create = () =>
            {
                UnitEntityData unit = FavoredClassLevelUpHarness.CreateUnit(14);
                unit.Stats.Dexterity.BaseValue = GunTrainingDexterity;
                created.Add(unit);
                return unit;
            };

            // Each group runs under its own guard and must leave its own
            // evidence: an exception in one group fails that group, never
            // lets a later group pass without having run.
            Action<string, IList<string>, Action> guard = (label, failures, body) =>
            {
                try { body(); }
                catch (Exception exception) { failures.Add(label + ": exception=" + exception); }
            };
            var basePicks = new Dictionary<int, BlueprintFeature> { { 5, blunderbuss }, { 9, pistol }, { 13, musket } };
            try
            {
                // 1. Base Gunslinger: distinct picks at 5, 9 and 13; level 17
                // exhausted. A cancelled level-9 visit comes first.
                guard("base", baseFailures, () =>
                {
                    UnitEntityData baseUnit = create();
                    JArray rows = LevelGunTraining(baseUnit, human, gunslinger.CharacterClass, null, 17, basePicks,
                        reserved, training, all, baseFailures, level =>
                        {
                            if (level != 9) return;
                            guard("cancel", cancelFailures, () => evidence["cancel"] = CancelGunTrainingPick(baseUnit,
                                human, gunslinger.CharacterClass, reserved, training, pistol, cancelFailures));
                        }, before17 => evidence["baseDamageAt16"] = DamageByKind(baseUnit));
                    CheckOfficialTrained(baseUnit, official, new[] { rifle, revolver }, "base", baseFailures);
                    JObject at16 = evidence["baseDamageAt16"] as JObject;
                    JObject at17 = DamageByKind(baseUnit);
                    evidence["baseDamageAt17"] = at17;
                    int dex = baseUnit.Stats.Dexterity.Bonus;
                    foreach (FirearmKind kind in OfficialFirearmSupport.Kinds)
                    {
                        FirearmTrainingEntitlement entitlement = FirearmTrainingRuntime.Resolve(baseUnit, kind);
                        if (!entitlement.Eligible || entitlement.DamageBonus != dex)
                            baseFailures.Add(kind + ": entitlement " + entitlement.Eligible + "/" +
                                entitlement.DamageBonus + " expected Dexterity bonus " + dex);
                        JToken before = at16 == null ? null : at16[kind.ToString()];
                        if (before == null || (int)before != (int)at17[kind.ToString()])
                            baseFailures.Add(kind + ": weapon bonus damage changed at 17 (" + before + " -> " +
                                at17[kind.ToString()] + ")");
                    }
                    evidence["base"] = rows;
                });

                // 2. Mysterious Stranger: no pick at 5; distinct picks at 9, 13, 17.
                guard("stranger", strangerFailures, () =>
                {
                    if (gunslinger.MysteriousStranger == null)
                        throw new InvalidOperationException("the Mysterious Stranger archetype is unavailable");
                    UnitEntityData stranger = create();
                    var picks = new Dictionary<int, BlueprintFeature> { { 9, musket }, { 13, blunderbuss }, { 17, pistol } };
                    JArray rows = LevelGunTraining(stranger, human, gunslinger.CharacterClass,
                        gunslinger.MysteriousStranger.Archetype, 17, picks, reserved, training, all,
                        strangerFailures, null, null);
                    CheckOfficialTrained(stranger, official, new[] { rifle, revolver }, "stranger", strangerFailures);
                    evidence["mysteriousStranger"] = rows;
                });

                // 3a. Constructed legacy: a Rifle fact before level 5. The three
                // official picks are still required and still offered.
                guard("legacyEarly", legacyFailures, () =>
                {
                    UnitEntityData legacyEarly = create();
                    JArray rows = LevelGunTraining(legacyEarly, human, gunslinger.CharacterClass, null, 17,
                        basePicks, reserved, training, all, legacyFailures, level =>
                        {
                            if (level == 5) legacyEarly.Descriptor.AddFact(rifle);
                        }, null);
                    if (FavoredClassLevelUpHarness.Rank(legacyEarly.Descriptor, rifle) != 1)
                        legacyFailures.Add("legacyEarly: the constructed Rifle fact was not kept exactly once");
                    CheckOfficialTrained(legacyEarly, official, new[] { revolver }, "legacyEarly", legacyFailures);
                    evidence["legacyEarly"] = rows;
                });

                // 3b. Constructed legacy: at 16 the history is Rifle, Pistol and
                // Musket. Level 17 must still require and offer Blunderbuss.
                guard("legacyLate", legacyFailures, () =>
                {
                    UnitEntityData legacyLate = create();
                    var latePicks = new Dictionary<int, BlueprintFeature> { { 5, pistol }, { 9, musket }, { 13, blunderbuss }, { 17, blunderbuss } };
                    JArray rows = LevelGunTraining(legacyLate, human, gunslinger.CharacterClass, null, 17,
                        latePicks, reserved, training, all, legacyFailures, level =>
                        {
                            if (level != 17) return;
                            legacyLate.Descriptor.RemoveFact(blunderbuss);
                            legacyLate.Descriptor.AddFact(rifle);
                            evidence["legacyLateConstructed"] = "Blunderbuss fact removed and Rifle fact added at 16";
                        }, null, expectLevel17Pick: true);
                    if (FavoredClassLevelUpHarness.Rank(legacyLate.Descriptor, rifle) != 1)
                        legacyFailures.Add("legacyLate: the constructed Rifle fact was not kept exactly once");
                    CheckOfficialTrained(legacyLate, official, new[] { revolver }, "legacyLate", legacyFailures);
                    evidence["legacyLate"] = rows;
                });

                // 4. Ranked archetype training: never Gun Training; four ranks.
                guard("pistolero", rankedFailures, () => evidence["pistolero"] = LevelRanked(create(), human,
                    gunslinger.CharacterClass,
                    gunslinger.Pistolero == null ? null : gunslinger.Pistolero.Archetype,
                    gunslinger.Pistolero == null ? null : gunslinger.Pistolero.Training,
                    new[] { FirearmKind.Pistol }, new[] { FirearmKind.Musket, FirearmKind.Blunderbuss },
                    reserved, training, all, rankedFailures, "pistolero"));
                guard("musketMaster", rankedFailures, () => evidence["musketMaster"] = LevelRanked(create(), human,
                    gunslinger.CharacterClass,
                    gunslinger.MusketMaster == null ? null : gunslinger.MusketMaster.Archetype,
                    gunslinger.MusketMaster == null ? null : gunslinger.MusketMaster.Training,
                    new[] { FirearmKind.Musket, FirearmKind.Blunderbuss }, new[] { FirearmKind.Pistol },
                    reserved, training, all, rankedFailures, "musketMaster"));

                // 7. What the player reads.
                guard("presentation", presentationFailures, () =>
                    evidence["presentation"] = ReadGunTrainingPresentation(gunslinger, presentationFailures));
            }
            finally
            {
                foreach (UnitEntityData unit in created)
                {
                    try { unit.Dispose(); }
                    catch (Exception exception) { presentationFailures.Add("dispose: " + exception.GetType().Name); }
                }
                cleaned = SameReferences(partyBefore, SnapshotReferences(party)) &&
                    SameReferences(unitsBefore, SnapshotReferences(allUnits));
            }

            evidenceFiles.Add(WriteFavoredClassEvidence("gun-training-levelups.json", evidence));
            const string route = "real LevelUpController visits: LevelUpState.IsComplete gate, then ApplyLevelup";
            Func<string, bool> ran = key => evidence[key] != null && evidence[key].Type != JTokenType.Null;
            return new List<RuntimeTestAssertion>
            {
                Assertion("gun-training-levelup-base",
                    "base Gunslinger: Gun Training required at 5, 9 and 13, offering exactly the untrained official types (3, 2, 1), " +
                    "a distinct type taken each time; at 17 it opens with nothing selectable, the level completes, no fact is added " +
                    "and no weapon gains damage; each official type gives exactly its Dexterity bonus",
                    Describe(evidence["base"], baseFailures), ran("base") && baseFailures.Count == 0, route),
                Assertion("gun-training-levelup-cancel",
                    "a type selected in a cancelled level-9 visit is held only by the preview; the character keeps level 8 and its facts",
                    Describe(evidence["cancel"], cancelFailures), ran("cancel") && cancelFailures.Count == 0,
                    "LevelUpController.Cancel after SelectFeature"),
                Assertion("gun-training-levelup-mysterious-stranger",
                    "Mysterious Stranger: no Gun Training at 5; required at 9, 13 and 17 with a distinct type each time; all three trained",
                    Describe(evidence["mysteriousStranger"], strangerFailures),
                    ran("mysteriousStranger") && strangerFailures.Count == 0, route),
                Assertion("gun-training-levelup-legacy-constructed",
                    "constructed legacy states (a hidden Rifle fact added directly): the official picks stay required and offered while " +
                    "an official type is untrained, including at 17; the Rifle fact is kept once and never offered",
                    Describe(new JObject { ["early"] = evidence["legacyEarly"], ["late"] = evidence["legacyLate"] }, legacyFailures),
                    ran("legacyEarly") && ran("legacyLate") && legacyFailures.Count == 0,
                    route + "; constructed compatibility control, not an old save"),
                Assertion("gun-training-levelup-ranked",
                    "Pistolero and Musket Master: Gun Training never opens; the ranked training reaches rank 1, 2, 3 and 4 at 5, 9, 13 " +
                    "and 17 and gives Dexterity + 3 to its own firearms at 17 and nothing to the others",
                    Describe(new JObject { ["pistolero"] = evidence["pistolero"], ["musketMaster"] = evidence["musketMaster"] },
                        rankedFailures), ran("pistolero") && ran("musketMaster") && rankedFailures.Count == 0,
                    route + "; FirearmTrainingRuntime.Resolve"),
                Assertion("gun-training-levelup-presentation",
                    "the Gun Training, Mysterious Stranger, Pistolero and Musket Master descriptions state what the transactions above do",
                    Describe(evidence["presentation"], presentationFailures),
                    ran("presentation") && presentationFailures.Count == 0,
                    "runtime LocalizedString of the published blueprints"),
                Assertion("gun-training-levelup-isolation", "unchanged party and global-unit snapshots",
                    "cleaned=" + cleaned + ";units=" + created.Count, cleaned, "detached units disposed")
            };
        }

        /// <summary>
        /// Levels a unit from its current level to <paramref name="target"/>,
        /// choosing Gun Training explicitly at the levels in
        /// <paramref name="picks"/> and checking the selection at each visit.
        /// </summary>
        private static JArray LevelGunTraining(UnitEntityData unit, BlueprintRace race,
            BlueprintCharacterClass characterClass, BlueprintArchetype archetype, int target,
            IDictionary<int, BlueprintFeature> picks, ICollection<string> reserved,
            GunTrainingBlueprintSet training, BlueprintFeature[] all, IList<string> failures,
            Action<int> beforeVisit, Action<int> before17, bool expectLevel17Pick = false)
        {
            var rows = new JArray();
            for (int level = unit.Descriptor.Progression.GetClassLevel(characterClass) + 1; level <= target; level++)
            {
                if (beforeVisit != null) beforeVisit(level);
                if (level == 17 && before17 != null) before17(level);
                int factsBefore = all.Sum(feature => FavoredClassLevelUpHarness.Rank(unit.Descriptor, feature));
                string[] untrained = training.Choices
                    .Where(feature => FavoredClassLevelUpHarness.Rank(unit.Descriptor, feature) == 0)
                    .Select(feature => feature.name).OrderBy(name => name, StringComparer.Ordinal).ToArray();
                BlueprintFeature pick;
                picks.TryGetValue(level, out pick);
                GunTrainingVisit visit = VisitGunTraining(unit, race, characterClass, archetype, level, pick,
                    reserved, training, failures);
                int factsAfter = all.Sum(feature => FavoredClassLevelUpHarness.Rank(unit.Descriptor, feature));
                visit.Evidence["trainingFactsBefore"] = factsBefore;
                visit.Evidence["trainingFactsAfter"] = factsAfter;
                visit.Evidence["untrainedOfficialBefore"] = new JArray(untrained);
                bool expectOpen = picks.ContainsKey(level) || (level == 17 && archetype == null);
                if (visit.Open != expectOpen)
                    failures.Add(string.Format("level {0}: Gun Training open={1}, expected {2}", level, visit.Open, expectOpen));
                bool expectPick = picks.ContainsKey(level) && (level < 17 || expectLevel17Pick || archetype != null);
                if (expectPick)
                {
                    if (!visit.Selectable.SequenceEqual(untrained))
                        failures.Add(string.Format("level {0}: offered [{1}], expected exactly the untrained official types [{2}]",
                            level, string.Join(",", visit.Selectable), string.Join(",", untrained)));
                    if (!visit.Required || visit.Picked == null || factsAfter != factsBefore + 1)
                        failures.Add(string.Format("level {0}: required={1} picked={2} facts {3}->{4}", level,
                            visit.Required, visit.Picked ?? "<none>", factsBefore, factsAfter));
                }
                else if (visit.Open)
                {
                    if (visit.Required || visit.Picked != null || visit.Selectable.Length != 0 || factsAfter != factsBefore)
                        failures.Add(string.Format("level {0}: exhausted pick required={1} picked={2} selectable={3} facts {4}->{5}",
                            level, visit.Required, visit.Picked ?? "<none>", string.Join(",", visit.Selectable),
                            factsBefore, factsAfter));
                }
                if (visit.Selectable.Any(name => name.Contains("Rifle") || name.Contains("Revolver")))
                    failures.Add(string.Format("level {0}: a legacy type was offered: {1}", level, string.Join(",", visit.Selectable)));
                if (!visit.Completed)
                    failures.Add(string.Format("level {0}: the level did not complete: {1}", level, visit.Evidence["blockers"]));
                if (visit.Open) rows.Add(visit.Evidence);
            }
            return rows;
        }

        private static GunTrainingVisit VisitGunTraining(UnitEntityData unit, BlueprintRace race,
            BlueprintCharacterClass characterClass, BlueprintArchetype archetype, int level, BlueprintFeature pick,
            ICollection<string> reserved, GunTrainingBlueprintSet training, IList<string> failures)
        {
            var visit = new GunTrainingVisit();
            visit.Evidence["level"] = level;
            LevelUpController controller = null;
            try
            {
                controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, characterClass,
                    "KMG Gun Training", archetype);
                FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                FeatureSelectionState state = controller.State.Selections.FirstOrDefault(value =>
                    ReferenceEquals(value.Selection, training.Selection));
                visit.Open = state != null;
                if (state != null)
                {
                    visit.Required = state.CanSelectAnything(controller.State, controller.Preview);
                    visit.Selectable = FavoredClassLevelUpHarness.Items(controller, state)
                        .Where(item => state.Selection.CanSelect(controller.Preview, controller.State, state, item))
                        .Select(item => item.Feature.name).OrderBy(name => name, StringComparer.Ordinal).ToArray();
                    visit.Evidence["required"] = visit.Required;
                    visit.Evidence["selectable"] = new JArray(visit.Selectable);
                    if (pick != null && visit.Required)
                    {
                        if (FavoredClassLevelUpHarness.Select(controller, state, pick))
                            visit.Picked = pick.name;
                        else
                            failures.Add(string.Format("level {0}: SelectFeature refused {1}", level, pick.name));
                        FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                    }
                    visit.Evidence["picked"] = visit.Picked;
                }
                visit.Completed = FavoredClassLevelUpHarness.Confirm(controller, unit.Descriptor, visit.Evidence) &&
                    unit.Descriptor.Progression.GetClassLevel(characterClass) == level;
                visit.Evidence["completed"] = visit.Completed;
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(controller);
            }
            return visit;
        }

        private static JObject CancelGunTrainingPick(UnitEntityData unit, BlueprintRace race,
            BlueprintCharacterClass characterClass, ICollection<string> reserved,
            GunTrainingBlueprintSet training, BlueprintFeature pick, IList<string> failures)
        {
            var row = new JObject();
            int levelBefore = unit.Descriptor.Progression.GetClassLevel(characterClass);
            int rankBefore = FavoredClassLevelUpHarness.Rank(unit.Descriptor, pick);
            LevelUpController controller = null;
            try
            {
                controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, characterClass,
                    "KMG Gun Training", null);
                FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                FeatureSelectionState state = controller.State.Selections.FirstOrDefault(value =>
                    ReferenceEquals(value.Selection, training.Selection));
                bool selected = state != null && FavoredClassLevelUpHarness.Select(controller, state, pick);
                row["previewSelected"] = selected;
                row["previewRank"] = FavoredClassLevelUpHarness.Rank(controller.Preview, pick);
                if (!selected || (int)row["previewRank"] != rankBefore + 1)
                    failures.Add("the preview did not hold the selected type: " + row);
            }
            finally
            {
                FavoredClassLevelUpHarness.Close(controller);
            }
            row["unitLevelAfterCancel"] = unit.Descriptor.Progression.GetClassLevel(characterClass);
            row["unitRankAfterCancel"] = FavoredClassLevelUpHarness.Rank(unit.Descriptor, pick);
            if ((int)row["unitLevelAfterCancel"] != levelBefore || (int)row["unitRankAfterCancel"] != rankBefore)
                failures.Add("the cancelled visit changed the character: " + row);
            return row;
        }

        private static void CheckOfficialTrained(UnitEntityData unit, BlueprintFeature[] official,
            BlueprintFeature[] absent, string label, IList<string> failures)
        {
            foreach (BlueprintFeature feature in official)
                if (FavoredClassLevelUpHarness.Rank(unit.Descriptor, feature) != 1)
                    failures.Add(label + ": " + feature.name + " rank " +
                        FavoredClassLevelUpHarness.Rank(unit.Descriptor, feature) + ", expected 1");
            foreach (BlueprintFeature feature in absent)
                if (FavoredClassLevelUpHarness.Rank(unit.Descriptor, feature) != 0)
                    failures.Add(label + ": " + feature.name + " present");
        }

        private static JObject LevelRanked(UnitEntityData unit, BlueprintRace race,
            BlueprintCharacterClass characterClass, BlueprintArchetype archetype, BlueprintFeature ranked,
            FirearmKind[] own, FirearmKind[] others, ICollection<string> reserved,
            GunTrainingBlueprintSet training, BlueprintFeature[] all, IList<string> failures, string label)
        {
            var result = new JObject();
            if (archetype == null || ranked == null)
            {
                failures.Add(label + ": archetype or ranked training unavailable");
                return result;
            }
            var ranks = new JObject();
            for (int level = 1; level <= 17; level++)
            {
                GunTrainingVisit visit = VisitGunTraining(unit, race, characterClass, archetype, level, null,
                    reserved, training, failures);
                if (visit.Open) failures.Add(label + ": Gun Training opened at " + level);
                if (!visit.Completed) failures.Add(label + ": level " + level + " did not complete");
                int rank = FavoredClassLevelUpHarness.Rank(unit.Descriptor, ranked);
                int expected = GunTrainingProgression.Levels.Count(value => value <= level);
                if (rank != expected) failures.Add(label + ": rank " + rank + " at " + level + ", expected " + expected);
                if (GunTrainingProgression.GrantsAt(level)) ranks[level.ToString()] = rank;
            }
            result["ranks"] = ranks;
            if (all.Any(feature => FavoredClassLevelUpHarness.Rank(unit.Descriptor, feature) != 0))
                failures.Add(label + ": holds a Gun Training fact");
            int dex = unit.Stats.Dexterity.Bonus;
            var entitlements = new JObject();
            foreach (FirearmKind kind in own.Concat(others))
            {
                FirearmTrainingEntitlement value = FirearmTrainingRuntime.Resolve(unit, kind);
                entitlements[kind.ToString()] = value.Eligible + "/" + value.DamageBonus;
                bool mine = own.Contains(kind);
                if (mine && (!value.Eligible || value.DamageBonus != dex + 3))
                    failures.Add(label + ": " + kind + " " + value.Eligible + "/" + value.DamageBonus + ", expected Dexterity + 3 = " + (dex + 3));
                if (!mine && value.Eligible)
                    failures.Add(label + ": " + kind + " is trained");
            }
            result["dexterityBonus"] = dex;
            result["entitlements"] = entitlements;
            result["weaponBonusDamage"] = DamageByKind(unit);
            return result;
        }

        /// <summary>Bonus damage of each official firearm in the unit's hand,
        /// through the native RuleCalculateWeaponStats pipeline.</summary>
        private static JObject DamageByKind(UnitEntityData unit)
        {
            var result = new JObject();
            ProductionFirearmBlueprintCatalog firearms = BlueprintBootstrap.ProductionFirearms;
            foreach (var entry in new[] { Tuple.Create(FirearmKind.Pistol, firearms.Pistol.Item),
                Tuple.Create(FirearmKind.Musket, firearms.Musket.Item),
                Tuple.Create(FirearmKind.Blunderbuss, firearms.Blunderbuss.Item) })
            {
                var weapon = new Kingmaker.Items.ItemEntityWeapon(entry.Item2);
                try
                {
                    unit.Body.PrimaryHand.InsertItem(weapon);
                    var stats = new Kingmaker.RuleSystem.Rules.RuleCalculateWeaponStats(unit, weapon, null);
                    Kingmaker.RuleSystem.Rulebook.Trigger(stats);
                    result[entry.Item1.ToString()] = stats.BonusDamage;
                }
                finally
                {
                    FirearmRuntimeState.Service.Forget(weapon);
                    if (unit.Body.PrimaryHand.MaybeItem != null) unit.Body.PrimaryHand.RemoveItem(false);
                }
            }
            return result;
        }

        private static JObject ReadGunTrainingPresentation(GunslingerClassBlueprintSet gunslinger, IList<string> failures)
        {
            var result = new JObject();
            Func<BlueprintUnitFact, string> text = fact =>
            {
                try { return fact == null ? null : fact.Description; }
                catch (Exception exception) { return "<unreadable: " + exception.GetType().Name + ">"; }
            };
            string selection = text(gunslinger.GunTraining.Selection);
            result["gunTrainingSelection"] = selection;
            foreach (string required in new[] { "Pistol, Musket or Blunderbuss", "only once", "5, 9, 13 and 17",
                "grants nothing more", "9, 13 and 17", "Pistol Training", "Musket Training" })
                if (selection == null || selection.IndexOf(required, StringComparison.Ordinal) < 0)
                    failures.Add("Gun Training description lacks '" + required + "'");
            Action<string, BlueprintArchetype, string[]> archetypeText = (label, archetype, required) =>
            {
                string value = archetype == null ? null : archetype.Description;
                result[label] = value;
                foreach (string part in required)
                    if (value == null || value.IndexOf(part, StringComparison.Ordinal) < 0)
                        failures.Add(label + " description lacks '" + part + "'");
            };
            archetypeText("mysteriousStranger", gunslinger.MysteriousStranger == null ? null :
                gunslinger.MysteriousStranger.Archetype, new[] { "9, 13 and 17" });
            archetypeText("pistolero", gunslinger.Pistolero == null ? null : gunslinger.Pistolero.Archetype,
                new[] { "Pistol Training", "5, 9, 13 and 17" });
            archetypeText("musketMaster", gunslinger.MusketMaster == null ? null : gunslinger.MusketMaster.Archetype,
                new[] { "Musket Training", "5, 9, 13 and 17" });
            return result;
        }
    }
}
