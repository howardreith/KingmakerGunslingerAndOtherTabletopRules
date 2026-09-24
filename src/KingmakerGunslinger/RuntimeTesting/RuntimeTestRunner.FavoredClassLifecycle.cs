using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.LevelUp;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.Visual.FogOfWar;
using Kingmaker.Visual.Particles;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.FavoredClass;
using KingmakerGunslinger.FavoredClass.Mechanics;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string FcbAuraOfCourageFeatureGuid = "e45ab30f49215054e83b4ea12165409f";
        private const string FcbAuraOfCourageAreaGuid = "7ced0efa297bd5142ab749f6e33b112b";
        private const string FcbRemoveFearBuffGuid = "c5c86809a1c834e42a2eb33133e90a28";
        private const string FcbInspireCompetenceBuffGuid = "f58e8500ebc8594499bd804b0277cdd8";
        private const string FcbInspireCompetenceEffectGuid = "1fa5f733fa1d77743bf54f5f3da5a6b1";
        private const string FcbBeastShapeSpellGuid = "5d4028eb28a106d4691ed1b92bbb1915";
        private const string FcbBeastShapeBuffGuid = "8dc6510d31614345a8c718208fbac1f8";
        private const string FcbMonkClassGuid = "e8f21e5b58e0569468e420ebea456124";
        private const string FcbPaladinClassGuid = "bfa11238e7ae3544bbeb4d0b92e897ec";
        private const string FcbRangerClassGuid = "cda0615668a6df14eb36ba19ee881af6";
        private const int FcbSettleUpdates = 4;

        private IEnumerator<object> _fcbLifecycleSteps;
        private readonly List<RuntimeTestAssertion> _fcbLifecycleAssertions = new List<RuntimeTestAssertion>();
        private JObject _fcbLifecycleEvidence;

        // L02, M15 (moving, crossing, interruption, death, area transition)
        // and M17 in the loaded working save: write-free, paused, ticked by
        // the native area and life controllers' own methods.
        private void PollFcbLifecycle()
        {
            if (!_request.ExitAfterCompletion || _workingSaveSmoke == null || !_workingSaveSmoke.Complete ||
                _workingSaveSmoke.WriteObserved)
                throw new InvalidOperationException(
                    "The favored-class lifecycle lane lacks its guarded working save or intact write boundary.");
            if (_fcbLifecycleSteps == null)
            {
                _fcbLifecycleEvidence = new JObject();
                _fcbLifecycleSteps = RunFcbLifecycle().GetEnumerator();
            }
            Exception failure = null;
            try { if (_fcbLifecycleSteps.MoveNext()) return; }
            catch (Exception exception) { failure = exception; }
            try { _fcbLifecycleSteps.Dispose(); }
            catch (Exception exception) { if (failure == null) failure = exception; }
            _fcbLifecycleSteps = null;
            string path = WriteFavoredClassEvidence("favored-class-lifecycle.json", _fcbLifecycleEvidence);
            var assertions = new List<RuntimeTestAssertion>(_fcbLifecycleAssertions);
            assertions.Add(Assertion("fcb-lifecycle-write-boundary", "no save write in the whole lane",
                "writeObserved=" + _workingSaveSmoke.WriteObserved, !_workingSaveSmoke.WriteObserved,
                "working-save write guard"));
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _request.ExpectedModVersion == _context.ModEntry.Info.Version, "Unity Mod Manager ModEntry.Info.Version"));
            bool pass = failure == null && assertions.TrueForAll(value => value.Status == "PASS");
            RuntimeTestResult result = CreateResult(failure != null ? RuntimeTestStatuses.Error :
                pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail, assertions,
                failure == null ? null : failure.ToString());
            result.EvidenceFiles.Add(path);
            Complete(result);
        }

        private sealed class FcbLifecycleFixtures
        {
            internal readonly List<UnitEntityData> Units = new List<UnitEntityData>();
            internal readonly List<BlueprintUnit> Blueprints = new List<BlueprintUnit>();
            internal readonly List<AreaEffectEntityData> Areas = new List<AreaEffectEntityData>();
            internal readonly HashSet<string> PartyMembers = new HashSet<string>(StringComparer.Ordinal);
            internal UnitEntityData Anchor;
            internal Vector3 Origin;
            internal Vector3 Direction;
        }

        private IEnumerable<object> RunFcbLifecycle()
        {
            if (!Game.Instance.IsPaused)
                throw new InvalidOperationException("The lifecycle lane requires the paused loaded working save.");
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            if (FavoredClassIntegrationStatusRegistry.Current.Availability !=
                    FavoredClassIntegrationAvailability.Published || leaves == null)
                throw new InvalidOperationException("The lifecycle lane requires the published integration.");
            var fixtures = new FcbLifecycleFixtures();
            fixtures.Anchor = Game.Instance.Player.Party.FirstOrDefault(value => value != null &&
                value.HoldingState != null && value.IsInGame);
            if (fixtures.Anchor == null)
                throw new InvalidOperationException("The working save has no active-area party anchor.");
            fixtures.Origin = fixtures.Anchor.Position;
            fixtures.Direction = ClearDirection(fixtures.Origin, 16f);
            _fcbLifecycleEvidence["origin"] = new JArray(fixtures.Origin.x, fixtures.Origin.y, fixtures.Origin.z);
            _fcbLifecycleEvidence["direction"] = new JArray(fixtures.Direction.x, fixtures.Direction.z);
            try
            {
                foreach (object step in FcbAuraOverlap(fixtures, leaves)) yield return step;
                foreach (object step in FcbPerformanceMembership(fixtures, leaves)) yield return step;
                foreach (object step in FcbFamilyLifecycle(fixtures, leaves)) yield return step;
                foreach (object step in FcbRespecPet(fixtures, leaves)) yield return step;
            }
            finally
            {
                CleanupFcbLifecycle(fixtures);
            }
        }

        // M17: two paladins' auras through the native area enter/exit logic.
        private IEnumerable<object> FcbAuraOverlap(FcbLifecycleFixtures fixtures, FavoredClassBlueprintSet leaves)
        {
            var failures = new List<string>();
            var evidence = new JObject();
            _fcbLifecycleEvidence["auras"] = evidence;
            var library = BlueprintBootstrap.Library;
            BlueprintFeature courage = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbAuraOfCourageFeatureGuid, "AuraOfCourageFeature");
            var area = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(library,
                FcbAuraOfCourageAreaGuid, "AuraOfCourageArea");
            var effect = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                FavoredClassAuraPublication.CourageEffectBuffGuid, "AuraOfCourageEffectBuff");
            var removeFear = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                FcbRemoveFearBuffGuid, "RemoveFearBuff");
            BlueprintFeature leaf = leaves.Pair(FavoredClassCatalog.EffectPaladinAuras, null).Full;
            Vector3 right = new Vector3(fixtures.Direction.z, 0f, -fixtures.Direction.x);
            Vector3 overlap = fixtures.Origin + fixtures.Direction * 5f;
            UnitEntityData paladinA = SpawnFcbFixture(fixtures, "PaladinA", fixtures.Origin + fixtures.Direction * 2f);
            UnitEntityData paladinB = SpawnFcbFixture(fixtures, "PaladinB", fixtures.Origin + fixtures.Direction * 8f);
            UnitEntityData allyC = SpawnFcbFixture(fixtures, "AllyC", overlap);
            UnitEntityData allyD = SpawnFcbFixture(fixtures, "AllyD", overlap + right * 12f);
            foreach (object step in WaitFcbFixtures(fixtures)) yield return step;
            int plainC = FearSaveBonus(allyC), plainD = FearSaveBonus(allyD);
            GrantFavoredClassRanks(paladinA, leaf, 2);
            paladinA.Descriptor.AddFact(courage);
            paladinB.Descriptor.AddFact(courage);
            yield return null;
            AreaEffectEntityData auraA = SpawnedFcbArea(paladinA, area), auraB = SpawnedFcbArea(paladinB, area);
            evidence["areas"] = new JObject { ["a"] = auraA != null, ["b"] = auraB != null };
            if (auraA == null || auraB == null)
            {
                failures.Add("the native Aura of Courage did not spawn its area for both paladins");
                RecordFcbLifecycle("fcb-lifecycle-aura-overlap", "two paladins' native auras", evidence, failures);
                yield break;
            }
            Func<UnitEntityData, int> instances = unit => unit.Buffs.Enumerable.Count(value =>
                ReferenceEquals(value.Blueprint, effect));
            // C in the overlap: A's aura enters first, then B's. The native
            // Replace stacking keeps the last applicant's single instance.
            auraA.Tick();
            int afterA = FearSaveBonus(allyC) - plainC, countA = instances(allyC);
            auraB.Tick();
            int afterAB = FearSaveBonus(allyC) - plainC, countAB = instances(allyC);
            // D walks into the overlap: B's aura enters first, then A's.
            PlaceFcbUnit(allyD, overlap + right * 0.3f);
            auraB.Tick();
            int dAfterB = FearSaveBonus(allyD) - plainD;
            auraA.Tick();
            int afterBA = FearSaveBonus(allyD) - plainD, countBA = instances(allyD);
            // Another morale bonus against fear (Remove Fear, +4) never adds.
            Buff morale = allyD.Descriptor.AddBuff(removeFear,
                new MechanicsContext(paladinB, paladinB.Descriptor, removeFear));
            int withMorale = FearSaveBonus(allyD) - plainD;
            if (morale != null) morale.Remove();
            // C leaves B's aura but stays inside A's: B's exit removes its
            // single instance; A does not re-enter a unit already inside.
            PlaceFcbUnit(allyC, fixtures.Origin + fixtures.Direction * 1f);
            auraB.Tick();
            auraA.Tick();
            int afterLeave = FearSaveBonus(allyC) - plainC, countLeave = instances(allyC);
            evidence["c"] = new JObject { ["aFirst"] = afterA, ["aFirstInstances"] = countA,
                ["thenB"] = afterAB, ["thenBInstances"] = countAB, ["leftB"] = afterLeave,
                ["leftBInstances"] = countLeave };
            evidence["d"] = new JObject { ["bFirst"] = dAfterB, ["thenA"] = afterBA, ["instances"] = countBA,
                ["withRemoveFearMorale"] = withMorale, ["removeFearApplied"] = morale != null };
            if (afterA != 6 || countA != 1)
                failures.Add("the invested paladin's aura alone did not give exactly +6 in one instance");
            if (afterAB != 4 || countAB != 1)
                failures.Add("after the uninvested aura applied last, the native Replace instance is not the single +4");
            if (dAfterB != 4 || afterBA != 6 || countBA != 1)
                failures.Add("after the invested aura applied last, the single instance is not +6");
            if (morale == null || withMorale != 6)
                failures.Add("another morale bonus against fear stacked with the aura (" + withMorale + ")");
            if (new[] { afterA, afterAB, dAfterB, afterBA, withMorale }.Any(value => value > 6))
                failures.Add("two paladins' aura bonuses stacked");
            evidence["nativeExitQuirk"] = "leaving the later paladin's aura removes the only Replace instance even inside the first; observed " +
                afterLeave + " with " + countLeave + " instance(s), the same native behavior as two uninvested paladins";
            RecordFcbLifecycle("fcb-lifecycle-aura-overlap",
                "two overlapping paladins follow the native Replace and Morale rules: one aura instance at a time carrying its own paladin's bonus (+6 invested, +4 native, whichever applied last), never a sum, and no stacking with Remove Fear's +4 morale bonus against fear",
                evidence, failures);
            fixtures.Areas.Add(auraA);
            fixtures.Areas.Add(auraB);
        }

        // M15 in the live area: owner-local membership, crossing, moving,
        // interruption and the bard's death, compared with an uninvested bard.
        private IEnumerable<object> FcbPerformanceMembership(FcbLifecycleFixtures fixtures, FavoredClassBlueprintSet leaves)
        {
            var failures = new List<string>();
            var evidence = new JObject();
            _fcbLifecycleEvidence["performance"] = evidence;
            var library = BlueprintBootstrap.Library;
            var performer = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                FcbInspireCompetenceBuffGuid, "InspireCompetenceBuff");
            var effect = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                FcbInspireCompetenceEffectGuid, "InspireCompetenceEffectBuff");
            FavoredClassPerformanceTarget target = FavoredClassPerformanceManifest.For("InspireCompetence");
            var area = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(library, target.AreaGuids[0],
                "InspireCompetenceArea");
            BlueprintFeature leaf = leaves.Pair(FavoredClassCatalog.EffectPerformanceRange, target.Key).Full;
            float native = area.Size.Meters;
            float widened = FavoredClassMechanicsPolicy.PerformanceRadiusMeters(native, 2);
            // The bards stand behind the origin, away from the paladins' auras,
            // on their own obstacle-free ray.
            Vector3 bardAt = fixtures.Origin - fixtures.Direction * 2f;
            Vector3 ray = ClearDirection(bardAt, widened + 4f);
            Vector3 side = new Vector3(-ray.z, 0f, ray.x);
            evidence["ray"] = new JArray(ray.x, ray.z);
            UnitEntityData bardA = SpawnFcbFixture(fixtures, "BardA", bardAt);
            UnitEntityData bardB = SpawnFcbFixture(fixtures, "BardB", bardAt + side * 0.5f);
            UnitEntityData ally = SpawnFcbFixture(fixtures, "AllyE", bardAt + ray * (native + 1.2f));
            foreach (object step in WaitFcbFixtures(fixtures)) yield return step;
            GrantFavoredClassRanks(bardA, leaf, 2);
            Buff buffA = bardA.Descriptor.AddBuff(performer, new MechanicsContext(bardA, bardA.Descriptor, performer));
            yield return null;
            AreaEffectEntityData areaA = SpawnedFcbArea(bardA, area);
            if (buffA == null || areaA == null)
            {
                failures.Add("the invested bard's native performance area did not spawn");
                RecordFcbLifecycle("fcb-lifecycle-performance", "two bards' live performance areas", evidence, failures);
                yield break;
            }
            Func<bool> has = () => ally.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, effect));
            Func<AreaEffectEntityData, float> radius = data =>
            {
                var cylinder = data == null || data.View == null ? null : data.View.Shape as ScriptZoneCylinder;
                return cylinder == null ? -1f : cylinder.Radius;
            };
            areaA.Tick();
            bool insideWidened = has();
            PlaceFcbUnit(ally, bardAt + ray * (widened + 0.8f));
            areaA.Tick();
            bool afterLeaving = has();
            PlaceFcbUnit(ally, bardAt + ray * (widened - 0.8f));
            areaA.Tick();
            bool afterReturning = has();
            // The aura follows its bard: moving the bard away takes the ally out.
            PlaceFcbUnit(bardA, bardAt - ray * 4f);
            areaA.Tick();
            bool afterBardMoved = has();
            PlaceFcbUnit(bardA, bardAt);
            areaA.Tick();
            // Control: the uninvested bard's same performance at the same spot.
            Buff buffB = bardB.Descriptor.AddBuff(performer, new MechanicsContext(bardB, bardB.Descriptor, performer));
            yield return null;
            AreaEffectEntityData areaB = SpawnedFcbArea(bardB, area);
            evidence["radius"] = new JObject { ["native"] = native, ["invested"] = radius(areaA),
                ["control"] = radius(areaB) };
            evidence["membership"] = new JObject { ["insideWidened"] = insideWidened, ["afterLeaving"] = afterLeaving,
                ["afterReturning"] = afterReturning, ["afterBardMoved"] = afterBardMoved };
            if (Math.Abs(radius(areaA) - widened) > 0.001f || areaB == null || Math.Abs(radius(areaB) - native) > 0.001f)
                failures.Add("the live areas do not carry their own owners' radii");
            if (!insideWidened || afterLeaving || !afterReturning || afterBardMoved)
                failures.Add("membership did not follow the widened, moving boundary");
            // Interruption: ending the performer buff ends the area and the
            // effect. The native controller's next tick of the ended area runs
            // its exits and destruction; the paused lane runs that tick itself.
            buffA.Remove();
            yield return null;
            areaA.Tick();
            evidence["afterInterruption"] = new JObject { ["areaEnded"] = areaA.IsEnded, ["allyHasEffect"] = has() };
            if (!areaA.IsEnded || has())
                failures.Add("interrupting the performance left its area or an orphaned effect");
            // Death: both bards perform again and die; the invested bard's
            // area must behave exactly as the control's.
            PlaceFcbUnit(ally, bardAt + ray * 2f);
            Buff again = bardA.Descriptor.AddBuff(performer, new MechanicsContext(bardA, bardA.Descriptor, performer));
            yield return null;
            AreaEffectEntityData areaA2 = SpawnedFcbArea(bardA, area);
            if (areaA2 != null) areaA2.Tick();
            if (areaB != null) areaB.Tick();
            foreach (object step in KillAndResurrect(bardA, evidence, "invested-bard")) yield return step;
            foreach (object step in KillAndResurrect(bardB, evidence, "control-bard")) yield return step;
            if (areaA2 != null && !areaA2.IsEnded) areaA2.Tick();
            if (areaB != null && !areaB.IsEnded) areaB.Tick();
            evidence["afterDeath"] = new JObject
            {
                ["investedAreaEnded"] = areaA2 == null || areaA2.IsEnded,
                ["controlAreaEnded"] = areaB == null || areaB.IsEnded,
                ["investedRadius"] = areaA2 == null ? -1f : radius(areaA2),
                ["allyEffects"] = ally.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, effect)),
            };
            if ((areaA2 == null || areaA2.IsEnded) != (areaB == null || areaB.IsEnded))
                failures.Add("the invested bard's area survived or ended differently from the control's on death");
            if ((int)((JObject)evidence["afterDeath"])["allyEffects"] > 1)
                failures.Add("death left duplicated performance effects on the ally");
            if (areaA2 != null && !areaA2.IsEnded && Math.Abs(radius(areaA2) - widened) > 0.001f)
                failures.Add("the surviving invested area lost its owner's radius");
            if (again != null && !again.IsDisposed) fixtures.Areas.Add(areaA2);
            if (areaB != null) fixtures.Areas.Add(areaB);
            RecordFcbLifecycle("fcb-lifecycle-performance",
                "in the live area the invested bard's own area is widened and the control's native; an ally crossing the widened boundary leaves and re-enters exactly at it, the area follows its moving bard, interruption ends it without an orphaned effect, and on death it behaves exactly like the control's",
                evidence, failures);
        }

        // L02: every stateful owned-effect family through death and
        // resurrection, polymorph and return, and an area reload.
        private IEnumerable<object> FcbFamilyLifecycle(FcbLifecycleFixtures fixtures, FavoredClassBlueprintSet leaves)
        {
            var failures = new List<string>();
            var evidence = new JObject();
            _fcbLifecycleEvidence["families"] = evidence;
            var library = BlueprintBootstrap.Library;
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            Func<string, BlueprintFeature> full = effect => leaves.Pair(effect, null).Full;
            // Gunslinger: grit (resource maximum), Nimble (conditional AC),
            // Dodge, Initiative and confirmation, earned through native picks
            // and the remaining counters as ranks.
            UnitEntityData gunslingerUnit = SpawnFcbFixture(fixtures, "Gunslinger", fixtures.Origin + fixtures.Direction * 11f);
            foreach (object step in WaitFcbFixtures(fixtures)) yield return step;
            BlueprintRace halfElf = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(FavoredClassAncestry.HalfElf).RaceGuid, "Half-elf");
            FavoredClassLeafPair grit = leaves.Pair(FavoredClassCatalog.EffectGrit, null);
            FavoredClassLeafPair confirmation = leaves.Pair(FavoredClassCatalog.EffectFirearmConfirmation, null);
            var reserved = new HashSet<string>(StringComparer.Ordinal)
                { FavoredClassIntegrationCoordinator.Host.GunslingerSelection.AssetGuid };
            var levelFailures = new List<string>();
            LevelFcbRespecSubject(gunslingerUnit, halfElf, new[] { grit.Partial, grit.Partial, grit.Partial, grit.Full,
                confirmation.Partial }, reserved, levelFailures, "lifecycle");
            if (levelFailures.Count != 0)
                throw new InvalidOperationException("The lifecycle Gunslinger could not level: " +
                    string.Join("; ", levelFailures.ToArray()));
            GrantFavoredClassRanks(gunslingerUnit, full(FavoredClassCatalog.EffectHalflingNimble), 2);
            GrantFavoredClassRanks(gunslingerUnit, full(FavoredClassCatalog.EffectHalflingDodge), 2);
            GrantFavoredClassRanks(gunslingerUnit, full(FavoredClassCatalog.EffectInitiative), 2);
            gunslingerUnit.Descriptor.Resources.Spend(gunslinger.Grit.Resource, 1);
            // Monk: grapple CMD and the stunning-use maximum (mixed rates).
            UnitEntityData monk = SpawnFcbFixture(fixtures, "Monk", fixtures.Origin + fixtures.Direction * 12f);
            foreach (object step in WaitFcbFixtures(fixtures)) yield return step;
            BlueprintCharacterClass monkClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FcbMonkClassGuid, "Monk");
            BlueprintRace undine = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(FavoredClassAncestry.Undine).RaceGuid, "Undine");
            LevelFcbClass(monk, undine, monkClass, 2);
            GrantFavoredClassRanks(monk, full(FavoredClassCatalog.EffectGrappleStunning), 2);
            // Fighter counter as ranks (CMD against bull rush).
            GrantFavoredClassRanks(monk, full(FavoredClassCatalog.EffectBullRushDragDefense), 1);
            // Mostly Human Ifrit: the human identity and its human grit.
            UnitEntityData ifrit = SpawnFcbFixture(fixtures, "MostlyHuman", fixtures.Origin + fixtures.Direction * 13f);
            foreach (object step in WaitFcbFixtures(fixtures)) yield return step;
            ElementalMostlyHumanRaceBlueprints ifritAncestry = BlueprintBootstrap.MostlyHuman.Races.Single(value =>
                value.Definition.RaceName == "Ifrit");
            LevelFcbRespecSubject(ifrit, ifritAncestry.Race, new[] { grit.Partial }, reserved, levelFailures, "mostly-human",
                ifritAncestry.Trait);
            // Ranger companion: the projected pet armor.
            UnitEntityData ranger = SpawnFcbFixture(fixtures, "Ranger", fixtures.Origin + fixtures.Direction * 14f);
            foreach (object step in WaitFcbFixtures(fixtures)) yield return step;
            GrantFavoredClassRanks(ranger, BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbAnimalCompanionRankGuid, "AnimalCompanionRank"), 4);
            ranger.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbCompanionLeopardGuid, "AnimalCompanionFeatureLeopard"));
            Game.Instance.EntityCreator.Tick();
            GrantFavoredClassRanks(ranger, full(FavoredClassCatalog.EffectCompanionArmor), 2);
            if (ranger.Descriptor.Pet != null) fixtures.Units.Add(ranger.Descriptor.Pet);
            yield return null;
            var subjects = new[] { gunslingerUnit, monk, ifrit, ranger };
            var baseline = subjects.ToDictionary(unit => unit.UniqueId, FcbCensus);
            evidence["baseline"] = new JObject(baseline.Select(pair => new JProperty(pair.Key, pair.Value)));
            // Death and resurrection.
            var afterDeath = new JObject();
            foreach (UnitEntityData unit in subjects)
            {
                foreach (object step in KillAndResurrect(unit, evidence, unit.Blueprint.name)) yield return step;
                JObject census = FcbCensus(unit);
                afterDeath[unit.UniqueId] = census;
                if (!JToken.DeepEquals(Comparable(baseline[unit.UniqueId]), Comparable(census)))
                    failures.Add(unit.Blueprint.name + ": death and resurrection changed its owned effects");
            }
            evidence["afterResurrection"] = afterDeath;
            // Polymorph and return.
            var spell = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, FcbBeastShapeSpellGuid,
                "Beast Shape II");
            var shape = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, FcbBeastShapeBuffGuid,
                "Beast Shape II buff");
            var during = new JObject();
            var afterReturn = new JObject();
            foreach (UnitEntityData unit in subjects)
            {
                var context = new MechanicsContext(unit, unit.Descriptor, spell, null, new TargetWrapper(unit));
                context.Params.CasterLevel = 20;
                Buff polymorph = unit.Buffs.AddBuff(shape, context, TimeSpan.FromMinutes(20));
                for (int wait = 0; wait < FcbSettleUpdates; wait++) yield return null;
                during[unit.UniqueId] = FcbCensus(unit);
                if (polymorph != null) polymorph.Remove();
                for (int wait = 0; wait < FcbSettleUpdates; wait++) yield return null;
                JObject census = FcbCensus(unit);
                afterReturn[unit.UniqueId] = census;
                if (polymorph == null)
                    failures.Add(unit.Blueprint.name + ": native Beast Shape II was rejected");
                if (!JToken.DeepEquals(Comparable(baseline[unit.UniqueId]), Comparable(census)))
                    failures.Add(unit.Blueprint.name + ": polymorph and return changed its owned effects");
                if (!JToken.DeepEquals(((JObject)baseline[unit.UniqueId])["counters"],
                        ((JObject)during[unit.UniqueId])["counters"]))
                    failures.Add(unit.Blueprint.name + ": polymorph changed its counters");
            }
            evidence["duringPolymorph"] = during;
            evidence["afterReturn"] = afterReturn;
            // Area transition owners: an invested paladin's aura and an
            // invested bard's performance.
            UnitEntityData auraOwner = SpawnFcbFixture(fixtures, "TransitionPaladin",
                fixtures.Origin + fixtures.Direction * 15f);
            UnitEntityData performanceOwner = SpawnFcbFixture(fixtures, "TransitionBard",
                fixtures.Origin + fixtures.Direction * 16f);
            foreach (object step in WaitFcbFixtures(fixtures)) yield return step;
            GrantFavoredClassRanks(auraOwner, full(FavoredClassCatalog.EffectPaladinAuras), 2);
            auraOwner.Descriptor.AddFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbAuraOfCourageFeatureGuid, "AuraOfCourageFeature"));
            GrantFavoredClassRanks(performanceOwner, leaves.Pair(FavoredClassCatalog.EffectPerformanceRange,
                FavoredClassPerformanceManifest.For("InspireCompetence").Key).Full, 2);
            var performer = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                FcbInspireCompetenceBuffGuid, "InspireCompetenceBuff");
            performanceOwner.Descriptor.AddBuff(performer,
                new MechanicsContext(performanceOwner, performanceOwner.Descriptor, performer));
            Game.Instance.EntityCreator.Tick();
            // A native area unload destroys every cross-scene unit outside the
            // party roster, so the transition subjects join the party exactly
            // as companions do (in memory only: the lane never saves).
            UnitEntityData[] travelers = subjects.Concat(new[] { auraOwner, performanceOwner }).ToArray();
            foreach (UnitEntityData traveler in travelers)
                JoinFcbParty(fixtures, traveler);
            yield return null;
            string[] ids = travelers.Select(unit => unit.UniqueId).ToArray();
            var owners = new HashSet<string>(new[] { auraOwner.UniqueId, performanceOwner.UniqueId });
            var beforeReload = travelers.ToDictionary(unit => unit.UniqueId, FcbCensus);
            JObject performanceBefore = DescribeOwnedAreas(owners);
            var performanceKey = performanceOwner.UniqueId + "|" + BlueprintLibraryLookup.RequireExact<
                BlueprintAbilityAreaEffect>(library, FavoredClassPerformanceManifest.For("InspireCompetence")
                .AreaGuids[0], "InspireCompetenceArea").name;
            if (performanceBefore.Count != 2 || performanceBefore[performanceKey] == null ||
                (float)performanceBefore[performanceKey]["ringFactor"] <= 1f)
                failures.Add("the transition owners' aura and widened performance were not live before the reload");
            Game.Instance.ReloadArea();
            int guard = 0;
            while ((LoadingProcess.Instance.IsLoadingInProcess || Game.Instance.CurrentMode != Kingmaker.GameModes.GameModeType.Default) &&
                   guard++ < 3000)
                yield return null;
            for (int wait = 0; wait < FcbSettleUpdates; wait++) yield return null;
            Game.Instance.EntityCreator.Tick();
            var afterReload = new JObject();
            foreach (string id in ids)
            {
                UnitEntityData unit = Game.Instance.State.Units.FirstOrDefault(value => value.UniqueId == id);
                if (unit == null)
                {
                    failures.Add(id + ": the subject did not return after the area reload");
                    continue;
                }
                JObject census = FcbCensus(unit);
                afterReload[id] = census;
                if (!JToken.DeepEquals(Comparable(beforeReload[id]), Comparable(census)))
                    failures.Add(unit.Blueprint.name + ": the area reload changed its owned effects");
            }
            // Re-resolve the fixtures for cleanup after the reload.
            fixtures.Units.RemoveAll(value => value == null);
            for (int index = 0; index < fixtures.Units.Count; index++)
            {
                string id = fixtures.Units[index].UniqueId;
                UnitEntityData reloaded = Game.Instance.State.Units.FirstOrDefault(value => value.UniqueId == id);
                if (reloaded != null) fixtures.Units[index] = reloaded;
            }
            JObject performanceAfter = DescribeOwnedAreas(owners);
            evidence["afterReload"] = afterReload;
            evidence["areasBeforeReload"] = performanceBefore;
            evidence["areasAfterReload"] = performanceAfter;
            if (!JToken.DeepEquals(performanceBefore, performanceAfter))
                failures.Add("owner-local areas did not return with their owners' radii and rings after the reload");
            RecordFcbLifecycle("fcb-lifecycle-families",
                "grit (spent, with its raised maximum), Nimble, Dodge, Initiative, confirmation, Monk grapple/stunning, Fighter CMD, Mostly Human identity and human access, and the Ranger companion's projected armor keep exactly their counters, owned modifiers, resources, pet projection and identity through death and resurrection, polymorph and return, and an area reload; owner-local aura and performance areas return with their owners' radii and rings",
                evidence, failures);
        }

        // L03 with a pet: an Oread Ranger whose companion armor was earned
        // through native picks. The committed native respec rebuilds Ranger 1
        // and destroys the old companion with no orphaned projection; the
        // counters earned again project exactly once onto the new companion.
        private IEnumerable<object> FcbRespecPet(FcbLifecycleFixtures fixtures, FavoredClassBlueprintSet leaves)
        {
            var failures = new List<string>();
            var evidence = new JObject();
            _fcbLifecycleEvidence["respecPet"] = evidence;
            var library = BlueprintBootstrap.Library;
            FavoredClassHostHandles host = FavoredClassIntegrationCoordinator.Host;
            BlueprintCharacterClass ranger = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                FcbRangerClassGuid, "Ranger");
            BlueprintFeatureSelection reward = host.BonusSelectionFor(ranger.AssetGuid);
            BlueprintRace oread = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library,
                FavoredClassRaceIdentities.ForAncestry(FavoredClassAncestry.Oread).RaceGuid, "Oread");
            BlueprintFeature hitPoint = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                FcbHostHitPointRewardGuid, "host favored-class hit point");
            FavoredClassLeafPair armor = leaves.Pair(FavoredClassCatalog.EffectCompanionArmor, null);
            BlueprintFeature petFeature = leaves.PetFeature(FavoredClassCatalog.EffectCompanionArmor);
            if (reward == null || armor.Partial == null || petFeature == null)
                throw new InvalidOperationException("The Ranger companion-armor route is incomplete.");
            var reserved = new HashSet<string>(StringComparer.Ordinal) { reward.AssetGuid };
            BlueprintFeature[] picks = { armor.Partial, armor.Partial, armor.Partial, armor.Full };
            UnitEntityData master = SpawnFcbFixture(fixtures, "RespecRanger", fixtures.Origin + fixtures.Direction * 6f);
            foreach (object step in WaitFcbUnit(master)) yield return step;
            LevelFcbRespecSubject(master, oread, picks, reserved, failures, "respec-pet", null, ranger, reward,
                GrantsFcbPet);
            Game.Instance.EntityCreator.Tick();
            UnitEntityData first = master.Descriptor.Pet;
            if (first != null) fixtures.Units.Add(first);
            Func<int> projections = () => Game.Instance.State.Units.Count(unit => unit != null && !unit.Destroyed &&
                unit.Descriptor.HasFact(petFeature) && (ReferenceEquals(unit, first) ||
                    ReferenceEquals(unit.Descriptor.Master.Value, master)));
            JObject before = FcbCensus(master);
            evidence["before"] = before;
            evidence["sourceLevel"] = master.Descriptor.Progression.GetClassLevel(ranger);
            if (failures.Count == 0 && (first == null || !FcbArmor(before, 1) || projections() != 1))
                failures.Add("the source Ranger 4 has no companion carrying exactly one +1 projection");
            if (failures.Count == 0)
            {
                // The loaded game's level-up screen also answers the native
                // respec's level-up start; this backend drive detaches it for
                // the one call and subscribes it again.
                CharacterBuildController presenter = Game.Instance.UI.CharacterBuildController;
                bool detached = false;
                JObject respec;
                try
                {
                    if (presenter != null)
                    {
                        EventBus.Unsubscribe(presenter);
                        detached = true;
                    }
                    // E13: a respec driven to its first choices and cancelled
                    // leaves the counters, the projection and the same
                    // companion untouched.
                    JObject cancelled = RunFcbRespec(master, (controller, row) =>
                    {
                        FavoredClassLevelUpHarness.Configure(controller, controller.Unit, oread, ranger,
                            "KMG FCB Respec", null);
                        FavoredClassLevelUpHarness.ChooseFavoredClass(controller, ranger, row);
                        return false;
                    }, failures, "pet-cancel", ranger);
                    Game.Instance.EntityCreator.Tick();
                    Game.Instance.EntityDestroyer.Tick();
                    JObject afterCancel = FcbCensus(master);
                    evidence["cancel"] = cancelled;
                    evidence["afterCancel"] = afterCancel;
                    if (!(bool)cancelled["invoked"] || (bool)cancelled["committed"] || (bool)cancelled["callback"] ||
                        !JToken.DeepEquals(before, afterCancel) || !ReferenceEquals(master.Descriptor.Pet, first) ||
                        first.Destroyed || projections() != 1)
                        failures.Add("the cancelled respec changed the counters, the projection or the companion");
                    respec = RunFcbRespec(master, (controller, row) =>
                    {
                        FavoredClassLevelUpHarness.Configure(controller, controller.Unit, oread, ranger,
                            "KMG FCB Respec", null);
                        if (FavoredClassLevelUpHarness.ChooseFavoredClass(controller, ranger, row) == null)
                            throw new InvalidOperationException("the favored Ranger progression is unavailable");
                        FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                        FeatureSelectionState state = FavoredClassLevelUpHarness.FindOpenState(controller,
                            reward.AssetGuid);
                        if (state == null || !FavoredClassLevelUpHarness.Select(controller, state, hitPoint))
                            throw new InvalidOperationException("the Ranger respec could not take the hit point");
                        FavoredClassLevelUpHarness.FillOthers(controller, reserved);
                        return true;
                    }, failures, "pet", ranger);
                }
                finally
                {
                    if (detached) EventBus.Subscribe(presenter);
                }
                Game.Instance.EntityDestroyer.Tick();
                JObject afterRespec = FcbCensus(master);
                evidence["respec"] = respec;
                evidence["afterRespec"] = afterRespec;
                evidence["oldCompanion"] = new JObject
                {
                    ["destroyed"] = first == null || first.Destroyed,
                    ["liveProjections"] = projections(),
                    ["masterHasPet"] = master.Descriptor.Pet != null,
                };
                if (!(bool)respec["committed"] || !(bool)respec["callback"])
                    failures.Add("the Ranger respec did not commit through the native callback");
                if (((JObject)afterRespec["counters"]).Count != 0 || master.Descriptor.Pet != null ||
                    projections() != 0 || (first != null && !first.Destroyed))
                    failures.Add("the committed respec left a counter, a companion or an orphaned projection");
                // Earned again through native picks on levels 2 to 5.
                LevelFcbRespecSubject(master, oread, picks, reserved, failures, "respec-pet-again", null, ranger,
                    reward, GrantsFcbPet);
                Game.Instance.EntityCreator.Tick();
                UnitEntityData second = master.Descriptor.Pet;
                if (second != null) fixtures.Units.Add(second);
                JObject after = FcbCensus(master);
                evidence["earnedAgain"] = after;
                evidence["earnedAgainLevel"] = master.Descriptor.Progression.GetClassLevel(ranger);
                evidence["newCompanion"] = new JObject
                {
                    ["present"] = second != null,
                    ["distinct"] = second != null && !ReferenceEquals(first, second),
                    ["liveProjections"] = projections(),
                };
                if (second == null || ReferenceEquals(first, second) || !FcbArmor(after, 1) || projections() != 1)
                    failures.Add("the counters earned again did not project exactly once onto the new companion");
            }
            RecordFcbLifecycle("fcb-lifecycle-respec-pet",
                "a cancelled native respec of an Oread Ranger 4 with +1 projected companion armor leaves its counters, projection and companion unchanged; a committed one rebuilds Ranger 1 without counters, destroys the old companion and leaves no projection; re-earning the counters projects +1 exactly once onto the new companion",
                evidence, failures);
            yield return null;
        }

        private static bool FcbArmor(JObject census, int value)
        {
            var pet = census["pet"] as JObject;
            return pet != null && (int)pet["petFeatureFacts"] == 1 &&
                ((JArray)pet["ownedModifiers"]).Select(item => (string)item)
                    .SequenceEqual(new[] { "AC|NaturalArmor|" + value });
        }

        private IEnumerable<object> KillAndResurrect(UnitEntityData unit, JObject evidence, string label)
        {
            int immortality = unit.Descriptor.State.Immortality.Count;
            unit.Descriptor.State.Immortality.ReleaseAll();
            int lethal = unit.MaxHP + Math.Max(1, unit.Stats.Constitution.ModifiedValue) + 10;
            Rulebook.Trigger(new RuleDealDamage(unit, unit,
                new DamageBundle(new DirectDamage(new DiceFormula(0, DiceType.D6), lethal)))
                { DisablePrecisionDamage = true, IgnoreDamageReduction = true });
            int guard = 0;
            while (!unit.Descriptor.State.IsDead && guard++ < 60)
            {
                TickFcbLife(unit);
                yield return null;
            }
            bool died = unit.Descriptor.State.IsDead;
            unit.Descriptor.ResurrectAndFullRestore();
            guard = 0;
            while (unit.Descriptor.State.IsDead && guard++ < 60)
            {
                TickFcbLife(unit);
                yield return null;
            }
            for (int wait = 0; wait < FcbSettleUpdates; wait++) yield return null;
            while (unit.Descriptor.State.Immortality.Count < immortality)
                unit.Descriptor.State.Immortality.Retain();
            var deaths = evidence["deaths"] as JObject ?? new JObject();
            deaths[label] = new JObject { ["died"] = died, ["alive"] = !unit.Descriptor.State.IsDead };
            evidence["deaths"] = deaths;
        }

        private static void TickFcbLife(UnitEntityData unit)
        {
            MethodInfo tick = typeof(UnitLifeController).GetMethod("TickOnUnit",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                new[] { typeof(UnitEntityData) }, null);
            if (tick == null) throw new MissingMethodException("Native UnitLifeController.TickOnUnit");
            tick.Invoke(new UnitLifeController(), new object[] { unit });
        }

        /// <summary>The owned favored-class state lifecycle events must preserve.</summary>
        private static JObject FcbCensus(UnitEntityData unit)
        {
            FavoredClassBlueprintSet leaves = BlueprintBootstrap.FavoredClassLeaves;
            var owned = new HashSet<BlueprintScriptableObject>(leaves.Pairs.SelectMany(pair => pair.Leaves));
            foreach (string effect in new[] { FavoredClassCatalog.EffectCompanionArmor, FavoredClassCatalog.EffectEidolonArmor })
            {
                BlueprintFeature pet = leaves.PetFeature(effect);
                if (pet != null) owned.Add(pet);
            }
            var counters = new JObject();
            foreach (Feature feature in unit.Descriptor.Progression.Features.Enumerable.Where(value =>
                value.Blueprint != null && owned.Contains(value.Blueprint)).OrderBy(value => value.Blueprint.name))
                counters[feature.Blueprint.name] = feature.GetRank();
            var modifiers = new JArray();
            foreach (StatType stat in Enum.GetValues(typeof(StatType)).Cast<StatType>())
            {
                ModifiableValue value = unit.Stats.GetStat(stat);
                if (value == null) continue;
                foreach (ModifiableValue.Modifier modifier in value.Modifiers.Where(item =>
                    item.Source != null && item.Source.Blueprint != null && owned.Contains(item.Source.Blueprint)))
                    modifiers.Add(stat + "|" + modifier.ModDescriptor + "|" + modifier.ModValue + "|" +
                        modifier.Source.Blueprint.name);
            }
            var resources = new JObject();
            foreach (UnitAbilityResource resource in unit.Descriptor.Resources.PersistantResources
                .Where(value => value != null && value.Blueprint != null).OrderBy(value => value.Blueprint.name))
                resources[resource.Blueprint.name] = new JObject
                {
                    ["max"] = resource.GetMaxAmount(unit.Descriptor),
                    ["current"] = resource.Amount
                };
            UnitEntityData companion = unit.Descriptor.Pet;
            JObject petState = null;
            if (companion != null)
            {
                BlueprintFeature projected = leaves.PetFeature(FavoredClassCatalog.EffectCompanionArmor);
                petState = new JObject
                {
                    ["petFeatureRank"] = projected == null ? 0 : companion.Descriptor.Progression.Features.GetRank(projected),
                    ["petFeatureFacts"] = projected == null ? 0 : companion.Descriptor.Progression.Features.Enumerable.Count(
                        value => ReferenceEquals(value.Blueprint, projected)),
                    ["ownedModifiers"] = new JArray(Enum.GetValues(typeof(StatType)).Cast<StatType>()
                        .Select(stat => companion.Stats.GetStat(stat)).Where(value => value != null)
                        .SelectMany(value => value.Modifiers.Where(item => item.Source != null &&
                            item.Source.Blueprint != null && owned.Contains(item.Source.Blueprint))
                            .Select(item => value.Type + "|" + item.ModDescriptor + "|" + item.ModValue))),
                };
            }
            ElementalMostlyHumanBlueprintSet mostlyHuman = BlueprintBootstrap.MostlyHuman;
            return new JObject
            {
                ["counters"] = counters,
                ["modifiers"] = new JArray(modifiers.OrderBy(value => (string)value, StringComparer.Ordinal)),
                ["resources"] = resources,
                ["pet"] = petState,
                ["mostlyHumanIdentity"] = mostlyHuman != null && unit.Descriptor.HasFact(mostlyHuman.Identity),
                ["hostHumanAccess"] = FavoredClassRuntime.GrantsHostHumanAccess(unit.Descriptor),
            };
        }

        /// <summary>The census without current resource amounts that a native rest legitimately refills.</summary>
        private static JObject Comparable(JObject census)
        {
            var copy = (JObject)census.DeepClone();
            foreach (JProperty resource in ((JObject)copy["resources"]).Properties())
                ((JObject)resource.Value).Remove("current");
            return copy;
        }

        private JObject DescribeOwnedAreas(ICollection<string> ids)
        {
            var result = new JObject();
            foreach (AreaEffectEntityData area in Game.Instance.State.AreaEffects.Where(value => value != null &&
                !value.IsEnded && value.Context != null && value.Context.MaybeCaster != null &&
                ids.Contains(value.Context.MaybeCaster.UniqueId)).OrderBy(value => value.Context.MaybeCaster.UniqueId +
                value.Blueprint.AssetGuid, StringComparer.Ordinal))
            {
                var cylinder = area.View == null ? null : area.View.Shape as ScriptZoneCylinder;
                GameObject ring = area.View == null ? null : typeof(Kingmaker.View.MapObjects.AreaEffectView)
                    .GetField("m_SpawnedFx", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(area.View) as GameObject;
                result[area.Context.MaybeCaster.UniqueId + "|" + area.Blueprint.name] = new JObject
                {
                    ["radius"] = cylinder == null ? -1d : Math.Round((double)cylinder.Radius, 3),
                    ["ringFactor"] = ring == null ? 1d : Math.Round((double)FavoredClassPerformanceRing.FactorOf(ring), 3),
                };
            }
            return result;
        }

        private UnitEntityData SpawnFcbFixture(FcbLifecycleFixtures fixtures, string label, Vector3 position)
        {
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
            blueprint.name = "KMG_Runtime_FcbLifecycle_" + label;
            blueprint.IsCheater = false;
            blueprint.Brain = null;
            fixtures.Blueprints.Add(blueprint);
            UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(blueprint, position, Quaternion.identity,
                fixtures.Anchor.HoldingState);
            Game.Instance.EntityCreator.Tick();
            if (unit == null)
                throw new InvalidOperationException("The lifecycle fixture " + label + " was not created.");
            fixtures.Units.Add(unit);
            unit.Descriptor.State.Immortality.Retain();
            unit.Descriptor.Stats.HitPoints.BaseValue = 200;
            if (!unit.Descriptor.IsTurnedOn) unit.Descriptor.TurnOn();
            PlaceFcbUnit(unit, position);
            return unit;
        }

        /// <summary>Waits (bounded) until one fixture is live in the area.</summary>
        private static IEnumerable<object> WaitFcbUnit(UnitEntityData unit)
        {
            for (int frame = 0; frame < 600 && (!unit.IsInState || unit.View == null); frame++)
            {
                Game.Instance.EntityCreator.Tick();
                yield return null;
            }
            if (!unit.IsInState || unit.View == null)
                throw new InvalidOperationException("The fixture " + unit.Blueprint.name + " did not enter the live area.");
            PlaceFcbUnit(unit, unit.Position);
        }

        /// <summary>Views are created asynchronously: wait (bounded) until every fixture is live.</summary>
        private static IEnumerable<object> WaitFcbFixtures(FcbLifecycleFixtures fixtures)
        {
            for (int frame = 0; frame < 600; frame++)
            {
                if (fixtures.Units.All(unit => unit != null && unit.IsInState && unit.View != null))
                {
                    foreach (UnitEntityData unit in fixtures.Units)
                        PlaceFcbUnit(unit, unit.Position);
                    yield break;
                }
                Game.Instance.EntityCreator.Tick();
                yield return null;
            }
            throw new InvalidOperationException("Lifecycle fixtures did not enter the live area: " + string.Join(",",
                fixtures.Units.Where(unit => unit == null || !unit.IsInState || unit.View == null)
                    .Select(unit => unit == null ? "<null>" : unit.Blueprint.name).ToArray()));
        }

        private void LevelFcbClass(UnitEntityData unit, BlueprintRace race, BlueprintCharacterClass characterClass,
            int levels)
        {
            for (int level = 0; level < levels; level++)
            {
                var row = new JObject();
                LevelUpController controller = null;
                try
                {
                    controller = FavoredClassLevelUpHarness.Open(unit.Descriptor, race, characterClass,
                        "KMG FCB Lifecycle");
                    if (unit.Descriptor.Progression.CharacterLevel == 0)
                        FavoredClassLevelUpHarness.ChooseFavoredClass(controller, characterClass, row);
                    FavoredClassLevelUpHarness.FillOthers(controller, new HashSet<string>(StringComparer.Ordinal));
                    if (!FavoredClassLevelUpHarness.Confirm(controller, unit.Descriptor, row))
                        throw new InvalidOperationException("the lifecycle level is incomplete: " + row["completion"]);
                }
                finally
                {
                    FavoredClassLevelUpHarness.Close(controller);
                }
            }
        }

        private static void PlaceFcbUnit(UnitEntityData unit, Vector3 position)
        {
            unit.Position = position;
            if (unit.View != null) unit.View.transform.position = position;
            Game.Instance.CurrentScene.Area.InteractiveObjectGrid.MoveTo(unit, position.x, position.z);
        }

        /// <summary>A horizontal direction with no line-of-sight obstacle for the given distance.</summary>
        private static Vector3 ClearDirection(Vector3 origin, float distance)
        {
            for (int step = 0; step < 16; step++)
            {
                float angle = step * (float)Math.PI / 8f;
                var direction = new Vector3((float)Math.Cos(angle), 0f, (float)Math.Sin(angle));
                bool clear = true;
                for (float along = 1f; along <= distance && clear; along += 1f)
                    clear = !LineOfSightGeometry.Instance.HasObstacle(origin, origin + direction * along);
                if (clear) return direction;
            }
            throw new InvalidOperationException("No obstacle-free direction exists around the party anchor.");
        }

        /// <summary>
        /// A native spawn queues its area until the entity creator's next
        /// tick; the paused lane ticks it once so the area enters the state.
        /// </summary>
        private static AreaEffectEntityData SpawnedFcbArea(UnitEntityData owner, BlueprintAbilityAreaEffect blueprint)
        {
            Game.Instance.EntityCreator.Tick();
            return OwnedArea(owner, blueprint);
        }

        private static void JoinFcbParty(FcbLifecycleFixtures fixtures, UnitEntityData unit)
        {
            Player player = Game.Instance.Player;
            if (!player.PartyCharacters.Any(value => value.UniqueId == unit.UniqueId))
                player.PartyCharacters.Add(unit);
            fixtures.PartyMembers.Add(unit.UniqueId);
            player.InvalidateCharacterLists();
            player.UpdateCharacterLists();
        }

        private static AreaEffectEntityData OwnedArea(UnitEntityData owner, BlueprintAbilityAreaEffect blueprint)
        {
            return Game.Instance.State.AreaEffects.LastOrDefault(value => value != null && !value.IsEnded &&
                ReferenceEquals(value.Blueprint, blueprint) && value.Context != null &&
                ReferenceEquals(value.Context.MaybeCaster, owner));
        }

        private int FearSaveBonus(UnitEntityData target)
        {
            var neutral = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                FcbMagicMissileGuid, "MagicMissile");
            var context = new MechanicsContext(target, target.Descriptor, neutral);
            context.AddSpellDescriptor(SpellDescriptor.Fear);
            using (context.GetDataScope(new TargetWrapper(target)))
                return context.TriggerRule(new RuleSavingThrow(target, SavingThrowType.Will, 10)).StatValue;
        }

        private void RecordFcbLifecycle(string name, string expected, JObject evidence, List<string> failures)
        {
            evidence["failures"] = new JArray(failures);
            _fcbLifecycleAssertions.Add(Assertion(name, expected, Describe(evidence, failures), failures.Count == 0,
                "loaded working save: native area, buff, life and polymorph controllers"));
        }

        private void CleanupFcbLifecycle(FcbLifecycleFixtures fixtures)
        {
            try
            {
                Player player = Game.Instance.Player;
                player.PartyCharacters.RemoveAll(value => fixtures.PartyMembers.Contains(value.UniqueId));
                player.InvalidateCharacterLists();
                player.UpdateCharacterLists();
            }
            catch (Exception) { }
            foreach (AreaEffectEntityData area in fixtures.Areas.Where(value => value != null))
                try { if (!area.IsEnded) area.ForceEnd(); } catch (Exception) { }
            foreach (UnitEntityData unit in fixtures.Units.Where(value => value != null))
                try
                {
                    if (unit.Descriptor.Master.Value != null) unit.Descriptor.SetMaster(null);
                    unit.Destroy();
                }
                catch (Exception) { }
            try
            {
                Game.Instance.EntityDestroyer.Tick();
                Game.Instance.EntityDestroyer.Tick();
            }
            catch (Exception) { }
            foreach (BlueprintUnit blueprint in fixtures.Blueprints)
                try { UnityEngine.Object.Destroy(blueprint); } catch (Exception) { }
        }
    }
}
