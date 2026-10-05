using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewSprint16Lifecycle(ExpandedSummoningCorrectionFixture fixture)
        {
            UnityEngine.Random.State random = UnityEngine.Random.state;
            bool pause = Game.Instance.IsPaused;
            var controls = fixture.Blueprints.OfType<BlueprintBuff>().Where(value =>
                value.name == "KMG_Summoning_Special_PurpleWorm_Swallowed" ||
                value.name == "KMG_Summoning_Special_GiantFlytrap_Engulfed").ToArray();
            string[] before = controls.Select(value => DescribeGraph(value, 0,
                new HashSet<object>(NativeDonorReferenceComparer.Instance), 32).ToString()).ToArray();
            try
            {
                foreach (string key in CrocodilianVisualPolicy.Keys)
                foreach (bool active in new[] { true, false })
                foreach (string boundary in new[] { "source-death", "dismissal", "expiry", "transition" }
                    .Concat(key == "dire-crocodile" && active ? new[] { "spit-out", "target-death" } : new string[0]))
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(fixture, key);
                    SetExpandedSummoningBrainActive(owner, false);
                    SummonGrabComponent grab = SummonGrabComponent.Find(owner);
                    var targets = new List<UnitEntityData>();
                    UnitEntityData target = fixture.Hostile;
                    // A dead target is never revived or reused by this drill.
                    if (boundary == "target-death")
                    {
                        target = CastExpandedSummoningQuietUnit(fixture, "wolf", fixture.Hostile);
                        targets.Add(target);
                        target.Descriptor.Stats.HitPoints.BaseValue = 100000;
                    }
                    var targetBefore = fixture.Hostile;
                    var damaged = fixture.HostileDamage;
                    var sizeBefore = fixture.HostileSize;
                    try
                    {
                        if (targets.Count > 0)
                        { fixture.Hostile = target; fixture.HostileDamage = 0; fixture.HostileSize = target.Descriptor.State.Size; }
                        ExecuteExpandedSummoningRuntimeAbility(owner, Sprint16Sprint(fixture.Blueprints, key), 0,
                            new TargetWrapper(owner), false);
                        Buff sprint = owner.Descriptor.Buffs.GetBuff(Sprint16SprintBuff(fixture.Blueprints, key, false));
                        if (!active) ExpireSprint16OwnedBuff(owner, sprint);
                        Buff held = Sprint16EstablishHold(fixture, owner, grab,
                            key == "crocodile" ? owner.Descriptor.State.Size : Size.Large, key == "dire-crocodile");
                        if (key == "dire-crocodile")
                        {
                            int claimed = -1;
                            UnityEngine.Random.InitState(FindNativeD20Seed(20));
                            SummonHoldComponent.MaintainLink(owner, target, grab, null,
                                owner.Descriptor.Buffs.GetBuff(grab.HoldBuff), held, ref claimed);
                        }
                        bool armed = key == "crocodile" ? ReferenceEquals(SummonHoldComponent.HeldTarget(owner), target) :
                            target.Get<UnitPartSwallowed>() != null &&
                                ReferenceEquals(target.Get<UnitPartSwallowed>().Swallower.Value, owner);
                        bool speedState = owner.Descriptor.Stats.Speed.BaseValue == 20 &&
                            owner.Descriptor.Stats.Speed.ModifiedValue == (active ? 40 : 20) &&
                            owner.Descriptor.HasFact(Sprint16SprintBuff(fixture.Blueprints, key, true));
                        var privateMeshes = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                            .Select(value => value.sharedMesh).Where(value => value != null &&
                                value.name == "KMG_" + key + "_Original").ToArray();
                        var privateMaterials = owner.View.GetComponentsInChildren<Renderer>(true)
                            .SelectMany(value => value.sharedMaterials).Where(value => value != null &&
                                value.name.StartsWith("KMG_" + key + "_Original", StringComparison.Ordinal)).ToArray();
                        int damageBefore = target.Damage;
                        if (boundary == "source-death") GameHelper.KillUnit(owner, fixture.Hostile);
                        else if (boundary == "target-death") GameHelper.KillUnit(target, owner);
                        else if (boundary == "dismissal") CleanupExpandedSummoningUnit(owner);
                        else if (boundary == "expiry")
                        {
                            Buff marker = owner.Descriptor.Buffs.Enumerable.Single(value => ReferenceEquals(
                                value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff));
                            ExpireSprint16OwnedBuff(owner, marker);
                        }
                        else if (boundary == "transition") SummonGrappleAreaSafeguard.Sweep(true, new[] { target });
                        else owner.Get<UnitPartSwallowWhole>().SpitOut(true);
                        Game.Instance.IsPaused = false;
                        DateTime until = DateTime.UtcNow.AddSeconds(15);
                        for (int frame = 0; frame < 1200; frame++)
                        {
                            bool reached = Sprint16RelationshipReleased(owner, target, grab) &&
                                (boundary != "source-death" || owner.Descriptor.State.IsDead) &&
                                (boundary != "target-death" || target.Descriptor.State.IsDead) &&
                                (boundary != "expiry" || owner.Destroyed);
                            if (frame >= 5 && (reached || DateTime.UtcNow >= until)) break;
                            yield return 0;
                        }
                        Game.Instance.IsPaused = pause;
                        bool free = Sprint16RelationshipReleased(owner, target, grab);
                        bool boundaryReached = boundary == "source-death" ? owner.Descriptor.State.IsDead :
                            boundary == "target-death" ? target.Descriptor.State.IsDead :
                            boundary == "dismissal" || boundary == "expiry" ? owner.Destroyed : true;
                        bool noDamage = boundary == "target-death" || target.Damage == damageBefore;
                        Sprint16Check(_crocodilianAssertions, _sprint16FinalRows,
                            key + "-" + (active ? "active" : "cooldown") + "-" + boundary,
                            armed && speedState && free && boundaryReached && noDamage,
                            new JObject { ["armed"] = armed, ["initialSpeedState"] = speedState,
                                ["boundaryReached"] = boundaryReached, ["released"] = free,
                                ["victimDamageBefore"] = damageBefore, ["victimDamageAfter"] = target.Damage,
                                ["destroyed"] = owner.Destroyed,
                                ["transitionScope"] = boundary == "transition" ?
                                    "production area safeguard over an explicit disposable target, not campaign party mutation" : null },
                            "source-owned Sprint and hold/swallow state remains local and releases cleanly at the named native boundary");
                        // Destruction of a corpse / transition survivor is
                        // cleanup only, never evidence for the boundary above.
                        DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                        for (int frame = 0; frame < 5; frame++) yield return 0;
                        bool reclaimed = owner.Destroyed && owner.View == null && owner.HoldingState == null &&
                            privateMeshes.Length == 1 && privateMaterials.Length > 0 &&
                            privateMeshes.All(value => value == null) && privateMaterials.All(value => value == null);
                        Sprint16Check(_crocodilianAssertions, _sprint16FinalRows,
                            key + "-" + (active ? "active" : "cooldown") + "-" + boundary + "-resources",
                            reclaimed, new JObject { ["meshes"] = privateMeshes.Length,
                                ["materials"] = privateMaterials.Length, ["reclaimed"] = reclaimed },
                            "exact captured per-view mesh/material objects destroyed after the source view; no donor/cache destruction");
                    }
                    finally
                    {
                        Game.Instance.IsPaused = pause;
                        if (!owner.Destroyed) DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                        if (targets.Count > 0) DisposeExpandedSummoningUnits(fixture.Created, targets.ToArray());
                        fixture.Hostile = targetBefore; fixture.HostileDamage = damaged; fixture.HostileSize = sizeBefore;
                        ResetExpandedSummoningHostile(fixture);
                    }
                }
                bool controlsIntact = controls.Length == 2 && controls.Select((value, index) =>
                    DescribeGraph(value, 0, new HashSet<object>(NativeDonorReferenceComparer.Instance), 32).ToString() == before[index]).All(value => value);
                Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, "worm-flytrap-graph-negative-controls",
                    controlsIntact, new JObject { ["unchanged"] = controlsIntact,
                        ["controls"] = new JArray(controls.Select(value => value.AssetGuid)) },
                    "both qualified swallowed/engulfed component graphs remain byte-for-byte unchanged through the Dire cases");
            }
            finally { UnityEngine.Random.state = random; Game.Instance.IsPaused = pause; }
        }

        private static bool Sprint16RelationshipReleased(UnitEntityData owner, UnitEntityData target, SummonGrabComponent grab)
        {
            var swallower = owner.Get<UnitPartSwallowWhole>();
            return target.Get<UnitPartGrappleTarget>() == null && target.Get<UnitPartSwallowed>() == null &&
                !target.Descriptor.HasFact(grab.GrappledBuff) &&
                (grab.SwallowedBuff == null || !target.Descriptor.HasFact(grab.SwallowedBuff)) &&
                (swallower == null || !swallower.SwallowedUnits.Contains(target)) &&
                SummonGrappleLinks.EstablishingWeapon(owner, target) == null &&
                !target.Descriptor.State.HasCondition(UnitCondition.CantAct) &&
                !target.Descriptor.State.HasCondition(UnitCondition.CantMove);
        }

        private void ExerciseSprint16Routes(ExpandedSummoningCorrectionFixture fixture)
        {
            object levelController = null;
            MethodInfo castRule = typeof(RuleCastSpell).GetMethod("OnTrigger");
            MethodInfo spawnAction = typeof(ContextActionSpawnMonster).GetMethod("RunAction");
            const BindingFlags statics = BindingFlags.NonPublic | BindingFlags.Static;
            try
            {
                fixture.Caster.Descriptor.Stats.Intelligence.BaseValue = 30;
                var wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "ba34257984f4c41408ce1dc2004e342e", "native Wizard targeted player path");
                AdvanceDisposableSpellcaster(fixture.Caster.Descriptor, wizard, 20, ref levelController);
                Spellbook book = fixture.Caster.Descriptor.GetSpellbook(wizard);
                while (book.CasterLevel < 20) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false); book.Rest();
                var parents = ExpandedSummoningInventoryObserver.CanonicalParentGuids.Select(guid =>
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, guid, "canonical summon parent")).ToArray();
                _context.Harmony.Patch(castRule, null, new HarmonyMethod(typeof(RuntimeTestRunner)
                    .GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix", statics)), null);
                _context.Harmony.Patch(spawnAction, new HarmonyMethod(typeof(RuntimeTestRunner)
                    .GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", statics)), null, null);
                _expandedSummoningPlayerPathCaptureActive = true;
                SummonVariantSpec[] variants = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                    .Where(value => CrocodilianVisualPolicy.Keys.Contains(value.Creature.Key)).ToArray();
                int published = 0, hidden = 0;
                foreach (SummonVariantSpec variant in variants)
                {
                    if (SummonVisibilityCatalog.IsPublished(variant))
                    {
                        var cases = new List<ExpandedSummoningPlayerPathCase>();
                        AddExpandedSummoningPlayerPathRootCase(cases, fixture.Blueprints, fixture.Caster, book, parents,
                            variant, ExpandedSummoningPlayerPathAlignmentFor(variant),
                            variant.Family == SummonFamily.Monster && variant.Creature.MonsterTemplated ?
                                (SummonAlignmentMode?)SummonAlignmentMode.Celestial : null,
                            fixture.SceneEntities, fixture.AllUnits, variant.StableKey);
                        ExpandedSummoningPlayerPathCase item = cases.Single();
                        bool exact = item.LiveContract && item.SlotContract && item.CommandStarted &&
                            item.CommandResult == "Success" && item.RuleCastCount == 1 && item.SpawnActionCount == 1;
                        Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, "player-root-" + variant.StableKey,
                            exact, new JObject { ["detail"] = item.Describe() },
                            "exact published parent/variant spellbook path, one slot, quantity/template/duration/view and cleanup");
                        published++;
                    }
                    else
                    {
                        UnitEntityData[] units = CastExpandedSummoningVariant(fixture.Blueprints,
                            fixture.Caster, variant, null, fixture.Evidence);
                        fixture.Created.AddRange(units);
                        int min = variant.Multiplicity == SummonMultiplicity.OneD4PlusOne ? 2 : 1;
                        int max = variant.Multiplicity == SummonMultiplicity.One ? 1 :
                            variant.Multiplicity == SummonMultiplicity.OneD3 ? 3 : 5;
                        bool exact = units.Length >= min && units.Length <= max && units.All(value =>
                            ReferenceEquals(value.Blueprint, ExpandedSummoningUnit(fixture.Blueprints, variant)) &&
                            ExpandedSummoningPlayerPathUnitExact(value, fixture.Caster) &&
                            ExpandedSummoningPteranodonViewPatch.DescribeView(value.View).StartsWith("visual:attached;", StringComparison.Ordinal));
                        Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, "private-root-" + variant.StableKey,
                            exact, new JObject { ["units"] = units.Length,
                                ["ids"] = new JArray(units.Select(value => value.UniqueId)), ["published"] = false },
                            "exact withheld private execution, quantity, duration/source and original view; no public route created");
                        DisposeExpandedSummoningUnits(fixture.Created, units);
                        hidden++;
                    }
                }
                Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, "targeted-root-census",
                    variants.Count(value => value.Creature.Key == "crocodile") == 14 &&
                        variants.Count(value => value.Creature.Key == "dire-crocodile") == 6 && published + hidden == 20,
                    new JObject { ["published"] = published, ["private"] = hidden, ["total"] = variants.Length },
                    "fourteen preserved Crocodile roots and six Dire routes only; no historical 970-root replay");
            }
            finally
            {
                _expandedSummoningPlayerPathCaptureActive = false;
                foreach (MethodInfo method in new[] { castRule, spawnAction })
                    _context.Harmony.Unpatch(method, HarmonyPatchType.All, _context.ModId);
                ExpandedSummoningPlayerPathEvents.Clear();
                if (levelController != null) levelController.GetType().GetMethod("Cancel").Invoke(levelController, null);
            }
        }

        private IEnumerable<int> ReviewSprint16Fallback(ExpandedSummoningCorrectionFixture fixture)
        {
            // Exact untouched native Monitor Lizard is the control, not another
            // KMG creature with an independent proxy configuration.
            var native = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(BlueprintBootstrap.Library,
                "4109b40f6bbb49640840644cc84ada67", "native Monitor Lizard negative control");
            UnitEntityData control = Game.Instance.EntityCreator.SpawnUnit(native, fixture.Caster.Position,
                Quaternion.identity, fixture.Scene);
            fixture.Created.Add(control);
            SetExpandedSummoningBrainActive(control, false);
            Mesh[] donor = control.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(value => value.sharedMesh).ToArray();
            try
            {
                foreach (string key in CrocodilianVisualPolicy.Keys)
                foreach (bool fault in new[] { true, false })
                {
                    UnitEntityData unit;
                    int reached = 0;
                    try
                    {
                        if (fault) ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest = () =>
                        { reached++; throw new InvalidOperationException("Sprint16 request-local post-swap fault"); };
                        unit = CastExpandedSummoningOwnTier(fixture, key);
                    }
                    finally { ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest = null; }
                    SetExpandedSummoningBrainActive(unit, false);
                    var renderers = unit.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(value => value.enabled).ToArray();
                    string outcome = ExpandedSummoningPteranodonViewPatch.DescribeView(unit.View);
                    bool original = renderers.Any(value => value.sharedMesh != null && value.sharedMesh.name == "KMG_" + key + "_Original");
                    bool exact = fault ? reached == 1 && !original && renderers.Any(value => donor.Contains(value.sharedMesh)) :
                        reached == 0 && original && outcome.StartsWith("visual:attached;", StringComparison.Ordinal);
                    Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, key + (fault ? "-fault-fallback" : "-recovery"), exact,
                        new JObject { ["faultReached"] = reached, ["outcome"] = outcome, ["original"] = original,
                            ["meshes"] = new JArray(renderers.Select(value => value.sharedMesh == null ? null : value.sharedMesh.name)) },
                        "post-swap fault restores the enabled native donor exactly; subsequent cast recovers the original");
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { unit });
                    for (int frame = 0; frame < 5; frame++) yield return 0;
                    int meshes = Resources.FindObjectsOfTypeAll<Mesh>().Count(value => value != null && value.name == "KMG_" + key + "_Original");
                    int materials = Resources.FindObjectsOfTypeAll<Material>().Count(value => value != null &&
                        value.name.StartsWith("KMG_" + key + "_Original", StringComparison.Ordinal));
                    Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, key + (fault ? "-fault" : "-recovery") + "-reclaimed",
                        meshes == 0 && materials == 0, new JObject { ["meshes"] = meshes, ["materials"] = materials },
                        "zero private crocodilian view resources after fault/recovery disposal");
                }
                bool unchanged = control.View != null && ExpandedSummoningPteranodonViewPatch.DescribeView(control.View) == "not-attempted" &&
                    control.View.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(value => value.sharedMesh).SequenceEqual(donor) &&
                    donor.All(value => value != null);
                Sprint16Check(_crocodilianAssertions, _sprint16FinalRows, "native-monitor-lizard-negative-control",
                    unchanged, new JObject { ["unchanged"] = unchanged, ["blueprint"] = native.AssetGuid },
                    "native Monitor Lizard renderers and mesh references remain untouched across both original-view faults and recoveries");
            }
            finally
            {
                ExpandedSummoningPteranodonViewPatch.PostSuppressionFaultForTest = null;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { control });
            }
        }
    }
}
