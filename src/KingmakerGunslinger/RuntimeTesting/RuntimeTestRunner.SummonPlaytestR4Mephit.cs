using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.Visual.Particles;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Repair R4 (MEPH-01..04), one save-free phase in the established
        // Oleg outdoor native Default fixture (the R3 fixture):
        //  1. Loaded configuration for all ten breaths after mod
        //     initialization: owned grant, AI reference, damage, cone and
        //     icon, plus the untouched native donors and the Pyrotechnics
        //     flash component.
        //  2. Ten fresh summons through their ordinary own-tier casts; the
        //     native ability description window for each breath (R3's
        //     verified construction and unrelated-modal refusal).
        //  3. Executed casts: Water breath (acid damage events correlated to
        //     caster, ability and target; capture at projectile launch),
        //     Earth breath (the changed physical path: bludgeoning), and
        //     Pyrotechnics (a cancelled command first, then one executed cast:
        //     one flash instance, its retirement, captures before, during and
        //     after). No input, no save; brains stay inactive.
        private sealed class R4MephitRecorder : IGlobalRulebookHandler<RuleDealDamage>, IApplyAbilityEffectHandler
        {
            internal readonly List<RuleDealDamage> Damage = new List<RuleDealDamage>();
            internal readonly List<AbilityExecutionContext> Applied = new List<AbilityExecutionContext>();
            internal readonly List<int> AppliedFrames = new List<int>();
            internal readonly List<KeyValuePair<AbilityExecutionContext, UnitEntityData>> Delivered =
                new List<KeyValuePair<AbilityExecutionContext, UnitEntityData>>();
            public void OnEventAboutToTrigger(RuleDealDamage evt) { }
            public void OnEventDidTrigger(RuleDealDamage evt) { if (evt != null) Damage.Add(evt); }
            public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
            { if (context != null && target != null && target.Unit != null) Delivered.Add(new KeyValuePair<AbilityExecutionContext, UnitEntityData>(context, target.Unit)); }
            public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, TargetWrapper target) { }
            public void OnAbilityEffectApplied(AbilityExecutionContext context)
            { if (context != null) { Applied.Add(context); AppliedFrames.Add(Time.frameCount); } }
        }

        // Read-only postfixes on the two native creation seams the casts use:
        // ProjectileController.CreateView (the breath cone) and
        // FxHelper.SpawnFxOnUnit (the Pyrotechnics flash). They only record.
        private sealed class R4SpawnTrace : IDisposable
        {
            private static R4SpawnTrace active;
            private readonly Harmony12.HarmonyInstance harmony;
            private readonly string id = "KMG.R4.Spawn." + Guid.NewGuid().ToString("N");
            private readonly System.Reflection.MethodInfo projectileMethod = typeof(Kingmaker.Controllers.Projectiles.ProjectileController)
                .GetMethod("CreateView", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance, null,
                    new[] { typeof(Kingmaker.Controllers.Projectiles.Projectile), typeof(Vector3?) }, null);
            private readonly System.Reflection.MethodInfo fxMethod = typeof(FxHelper).GetMethod("SpawnFxOnUnit",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null,
                new[] { typeof(GameObject), typeof(UnitEntityView), typeof(string), typeof(Vector3) }, null);
            internal UnitEntityData Owner;
            internal GameObject FlashPrefab;
            internal readonly JArray Projectiles = new JArray();
            internal readonly List<GameObject> Flashes = new List<GameObject>();
            internal readonly JArray FlashRows = new JArray();

            internal R4SpawnTrace()
            {
                if (active != null || projectileMethod == null || fxMethod == null)
                    throw new InvalidOperationException("Native projectile/effect spawn readers unavailable.");
                harmony = Harmony12.HarmonyInstance.Create(id);
                try
                {
                    harmony.Patch(projectileMethod, null, new Harmony12.HarmonyMethod(typeof(R4SpawnTrace).GetMethod("AfterProjectile",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)));
                    harmony.Patch(fxMethod, null, new Harmony12.HarmonyMethod(typeof(R4SpawnTrace).GetMethod("AfterFx",
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)));
                    active = this;
                }
                catch { Dispose(); throw; }
            }

            private static void AfterProjectile(Kingmaker.Controllers.Projectiles.Projectile __0)
            {
                var value = active;
                if (value == null || value.Owner == null || __0 == null || !ReferenceEquals(__0.Launcher, value.Owner)) return;
                value.Projectiles.Add(new JObject { ["frame"] = Time.frameCount, ["realtime"] = Time.realtimeSinceStartup,
                    ["owner"] = value.Owner.UniqueId, ["projectile"] = __0.Blueprint == null ? null : __0.Blueprint.AssetGuid,
                    ["projectileName"] = __0.Blueprint == null ? null : __0.Blueprint.name, ["viewPresent"] = __0.View != null });
            }

            private static void AfterFx(GameObject __result, GameObject __0, UnitEntityView __1)
            {
                var value = active;
                if (value == null || value.Owner == null || value.FlashPrefab == null || __0 == null ||
                    !ReferenceEquals(__0, value.FlashPrefab) && __0.name != value.FlashPrefab.name) return;
                value.Flashes.Add(__result);
                value.FlashRows.Add(new JObject { ["frame"] = Time.frameCount, ["realtime"] = Time.realtimeSinceStartup,
                    ["samePrefabObject"] = ReferenceEquals(__0, value.FlashPrefab),
                    ["onOwner"] = __1 != null && ReferenceEquals(__1, value.Owner.View),
                    ["instance"] = __result == null ? null : __result.name, ["prefab"] = __0.name });
            }

            public void Dispose()
            {
                if (projectileMethod != null) harmony.Unpatch(projectileMethod, Harmony12.HarmonyPatchType.All, id);
                if (fxMethod != null) harmony.Unpatch(fxMethod, Harmony12.HarmonyPatchType.All, id);
                if (ReferenceEquals(active, this)) active = null;
            }
        }

        private static void R4CollectDamage(object node, List<ContextActionDealDamage> found, HashSet<object> seen)
        {
            if (node == null || !seen.Add(node)) return;
            var damage = node as ContextActionDealDamage;
            if (damage != null) { found.Add(damage); return; }
            var list = node as ActionList;
            if (list != null) { foreach (GameAction action in list.Actions ?? new GameAction[0]) R4CollectDamage(action, found, seen); return; }
            var run = node as AbilityEffectRunAction;
            if (run != null) { R4CollectDamage(run.Actions, found, seen); return; }
            var conditional = node as Conditional;
            if (conditional != null) { R4CollectDamage(conditional.IfTrue, found, seen); R4CollectDamage(conditional.IfFalse, found, seen); return; }
            var saved = node as ContextActionConditionalSaved;
            if (saved != null) { R4CollectDamage(saved.Succeed, found, seen); R4CollectDamage(saved.Failed, found, seen); }
        }

        private static List<ContextActionDealDamage> R4Damages(BlueprintAbility ability)
        {
            var found = new List<ContextActionDealDamage>();
            foreach (BlueprintComponent component in ability.ComponentsArray ?? new BlueprintComponent[0])
                R4CollectDamage(component, found, new HashSet<object>());
            return found;
        }

        private static string R4DamageName(DamageTypeDescription type)
        {
            if (type == null) return "<null>";
            if (type.Type == DamageType.Physical) return "Physical:" + (type.Physical == null ? "?" : type.Physical.Form.ToString());
            if (type.Type == DamageType.Energy) return "Energy:" + type.Energy;
            return type.Type.ToString();
        }

        private static string R4Required(MephitBreathRow row)
        {
            return MephitBreathPolicy.IsPhysical(row.Required) ? "Physical:" + row.Required : "Energy:" + row.Required;
        }

        private static string R4PacketName(BaseDamage damage)
        {
            var energy = damage as EnergyDamage;
            if (energy != null) return "Energy:" + energy.EnergyType;
            var physical = damage as PhysicalDamage;
            if (physical != null) return "Physical:" + physical.Form;
            return damage == null ? "<null>" : damage.GetType().Name;
        }

        private static JObject R4DamageEvent(RuleDealDamage evt)
        {
            return new JObject { ["initiator"] = evt.Initiator == null ? null : evt.Initiator.UniqueId,
                ["target"] = evt.Target == null ? null : evt.Target.UniqueId,
                ["sourceAbility"] = evt.SourceAbility == null ? null : evt.SourceAbility.AssetGuid,
                ["sourceAbilityName"] = evt.SourceAbility == null ? null : evt.SourceAbility.name,
                ["total"] = evt.Damage,
                ["packets"] = new JArray(((IEnumerable<DamageValue>)evt.ResultDamage ?? new DamageValue[0]).Select(value => new JObject {
                    ["type"] = R4PacketName(value.Source), ["final"] = value.FinalValue,
                    ["rolled"] = value.RolledValue })) };
        }

        // Mirrors the qualified Sprint26Cast driver, with one owned capture
        // hook: when trigger() first holds, the capture runs while the
        // execution process keeps being ticked.
        private IEnumerable<int> R4RunCast(UnitEntityData owner, BlueprintAbility blueprint, TargetWrapper target,
            JObject row, Func<bool> trigger, Func<IEnumerable<int>> onTrigger)
        {
            var fact = owner.Descriptor.Abilities.GetAbility(blueprint);
            if (fact == null) throw new InvalidOperationException("Ability not granted: " + blueprint.name);
            var data = new AbilityData(fact);
            if (!data.IsAvailable || !data.CanTarget(target)) throw new InvalidOperationException("Native ability unavailable: " + blueprint.name);
            var command = new UnitUseAbility(data, target);
            row["commandId"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(command);
            row["startFrame"] = Time.frameCount;
            owner.Commands.Run(command);
            DateTime deadline = DateTime.UtcNow.AddSeconds(25);
            int frames = 0; bool fired = false;
            while (frames++ < 3000 && DateTime.UtcNow < deadline)
            {
                if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Tick();
                if (!fired && trigger != null && trigger())
                {
                    fired = true; row["triggerFrame"] = Time.frameCount;
                    foreach (int frame in onTrigger())
                    {
                        if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Tick();
                        yield return frame;
                    }
                }
                yield return 0;
                if (!fired && trigger != null && trigger())
                {
                    fired = true; row["triggerFrame"] = Time.frameCount; row["triggerAfterYield"] = true;
                    foreach (int frame in onTrigger())
                    {
                        if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Tick();
                        yield return frame;
                    }
                }
                if (command.IsFinished && command.ExecutionProcess != null && command.ExecutionProcess.IsEnded) break;
            }
            row["finished"] = command.IsFinished;
            row["executionEnded"] = command.ExecutionProcess != null && command.ExecutionProcess.IsEnded;
            row["endFrame"] = Time.frameCount; row["triggerFired"] = fired;
            if (!command.IsFinished || command.ExecutionProcess == null || !command.ExecutionProcess.IsEnded)
                throw new InvalidOperationException("Native ability did not finish: " + blueprint.name);
        }

        private IEnumerable<int> ReviewSummonPlaytestMephitR4(ExpandedSummoningCorrectionFixture fixture, string phase,
            Cleanup2NativeDefaultScope mode)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || fixture.Hostile == null || mode == null || !mode.Current ||
                !_context.FeatureModules.Active.ExpandedSummoning ||
                phase != ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestMephitPhase &&
                    phase != ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestMephitFinishPhase ||
                !ExpandedSummoningNativeProfilePolicy.Sprint32NativeDefaultOutdoor(_request.Scenario, phase) ||
                !Cleanup2NoNarrativeOverlay())
                throw new InvalidOperationException("Exact fresh R4 mephit fixture in owned outdoor native Default, without narrative, required.");
            const string prefix = "r4-mephit-";
            // Finishing selection: configuration checks for all ten, then fresh
            // presentation only for Salt, the ally-safety case and Pyrotechnics.
            bool finish = phase == ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestMephitFinishPhase;
            var game = Game.Instance; var ui = game.UI;
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId,
                ["fixture"] = "established non-DLC Oleg outdoor native Default; save-free",
                ["limits"] = "Matrix rows read the loaded blueprints after mod initialization. Executed damage comes only from RuleDealDamage events correlated to the exact caster, ability and target. Screenshots support presentation; owner visual approval is separate. No input; brains inactive; no save." };
            var owned = new List<UnitEntityData>();
            var units = new Dictionary<string, UnitEntityData>(StringComparer.Ordinal);
            Vector3 centre = fixture.Caster.Position;
            Vector3 hostilePosition = fixture.Hostile.Position;
            NativeIconScreenEvidence screens = null;
            TooltipData openDescription = null;
            R4MephitRecorder recorder = null; bool subscribed = false;
            R4SpawnTrace trace = null;
            Sprint26Saves saves = null;
            var hostile = fixture.Hostile;
            Func<string, BlueprintAbility> breathOf = key => (BlueprintAbility)symbols["KMG.Summoning.Special." +
                ExpandedSummoningSpecialBuilder.MephitToken(key) + ".Breath"];
            Func<bool> worldOwned = () => ExpandedSummoningNativeProfilePolicy.SummonPlaytestR3SurfaceOwned(mode.Current,
                Cleanup2NoNarrativeOverlay(), R3UnrelatedModal(), !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy);
            try
            {
                // 1. Loaded configuration, all ten.
                BlueprintAbility[] nativeBreaths = MephitBreathPolicy.NativeBreathGuids.Select(guid =>
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, guid, "native mephit breath")).ToArray();
                var matrix = new JArray(); bool matrixOk = true;
                foreach (MephitBreathRow row in MephitBreathPolicy.Rows)
                {
                    string token = ExpandedSummoningSpecialBuilder.MephitToken(row.Key);
                    var unit = (BlueprintUnit)symbols["KMG.Summoning.Unit." + token];
                    BlueprintAbility breath = breathOf(row.Key);
                    BlueprintUnitFact[] facts = unit.AddFacts ?? new BlueprintUnitFact[0];
                    BlueprintAbility[] brainAbilities = unit.Brain == null || unit.Brain.Actions == null ? new BlueprintAbility[0] :
                        unit.Brain.Actions.OfType<BlueprintAiCastSpell>().Select(value => value.Ability).ToArray();
                    List<ContextActionDealDamage> damages = R4Damages(breath);
                    AbilityDeliverProjectile deliver = (breath.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityDeliverProjectile>().FirstOrDefault();
                    string projectile = deliver == null || deliver.Projectiles == null || deliver.Projectiles.Length != 1 ||
                        deliver.Projectiles[0] == null ? null : deliver.Projectiles[0].AssetGuid;
                    var iconSource = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, row.IconSourceGuid, "icon source");
                    string actual = damages.Count == 1 ? R4DamageName(damages[0].DamageType) : "count=" + damages.Count;
                    AbilityEffectRunAction runAction = (breath.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilityEffectRunAction>().FirstOrDefault();
                    Conditional gate = runAction == null || runAction.Actions == null || runAction.Actions.Actions == null ||
                        runAction.Actions.Actions.Length != 1 ? null : runAction.Actions.Actions[0] as Conditional;
                    bool enemiesOnly = gate != null && gate.ConditionsChecker != null && gate.ConditionsChecker.Conditions != null &&
                        gate.ConditionsChecker.Conditions.Length == 1 && gate.ConditionsChecker.Conditions[0] is ContextConditionIsEnemy &&
                        !gate.ConditionsChecker.Conditions[0].Not &&
                        (gate.IfFalse == null || gate.IfFalse.Actions == null || gate.IfFalse.Actions.Length == 0);
                    bool ok = facts.Count(value => ReferenceEquals(value, breath)) == 1 &&
                        !facts.Any(value => nativeBreaths.Contains(value)) &&
                        brainAbilities.Count(value => ReferenceEquals(value, breath)) == 1 &&
                        !brainAbilities.Any(value => nativeBreaths.Contains(value)) &&
                        actual == R4Required(row) && projectile == MephitBreathPolicy.ProjectileFor(row.Required) &&
                        breath.Icon != null && ReferenceEquals(breath.Icon, iconSource.Icon) && breath.Icon.name != "BurningHands" &&
                        deliver != null && deliver.Type == AbilityProjectileType.Cone && deliver.Length.Value == 15f && enemiesOnly;
                    matrixOk &= ok;
                    matrix.Add(new JObject { ["key"] = row.Key, ["unit"] = unit.AssetGuid, ["nativeBacked"] = row.NativeBacked,
                        ["breath"] = breath.AssetGuid, ["breathName"] = breath.Name, ["grantSource"] = "BlueprintUnit.AddFacts",
                        ["grants"] = facts.Count(value => ReferenceEquals(value, breath)),
                        ["nativeBreathGrants"] = facts.Count(value => nativeBreaths.Contains(value)),
                        ["brain"] = unit.Brain == null ? null : unit.Brain.AssetGuid,
                        ["aiReferences"] = brainAbilities.Count(value => ReferenceEquals(value, breath)),
                        ["nativeAiReferences"] = brainAbilities.Count(value => nativeBreaths.Contains(value)),
                        ["required"] = R4Required(row), ["configured"] = actual,
                        ["dice"] = damages.Count == 1 && damages[0].Value != null ? damages[0].Value.DiceCountValue + "d" + (int)damages[0].Value.DiceType : null,
                        ["halfIfSaved"] = damages.Count == 1 && damages[0].HalfIfSaved, ["enemiesOnlyGate"] = enemiesOnly,
                        ["projectile"] = projectile, ["coneFeet"] = deliver == null ? -1 : deliver.Length.Value,
                        ["icon"] = breath.Icon == null ? null : breath.Icon.name, ["iconSource"] = row.IconSourceName,
                        ["evidence"] = "loaded configuration", ["passed"] = ok });
                }
                record["matrix"] = matrix;
                Sprint25Check(prefix + "loaded-breath-matrix", matrixOk && matrix.Count == 10, new JObject { ["rows"] = matrix.DeepClone() },
                    "All ten loaded KMG mephit units grant exactly one owned breath, referenced by exactly one brain cast action (a configured reference, not an observed autonomous cast), never the native breath; each owned effect is gated to the caster's enemies (charter ally-safety) and deals its required damage through a 15-foot cone of the matching native visual with its own existing art.");

                var donors = new JArray(); bool donorsOk = true;
                foreach (MephitBreathRow row in MephitBreathPolicy.NativeBackedRows)
                {
                    var donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library, ExpandedSummoningDonorCatalog.For(row.Key).Guid, "donor");
                    var native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, row.NativeBreathGuid, "native breath");
                    List<ContextActionDealDamage> damages = R4Damages(native);
                    bool unchanged = (donor.AddFacts ?? new BlueprintUnitFact[0]).Count(value => ReferenceEquals(value, native)) == 1 &&
                        donor.Brain != null && donor.Brain.Actions.OfType<BlueprintAiCastSpell>().Count(value => ReferenceEquals(value.Ability, native)) == 1 &&
                        native.Icon != null && native.Icon.name == "BurningHands" && damages.Count == 1;
                    donorsOk &= unchanged;
                    donors.Add(new JObject { ["key"] = row.Key, ["donor"] = donor.AssetGuid, ["nativeBreath"] = native.AssetGuid,
                        ["nativeDamage"] = damages.Count == 1 ? R4DamageName(damages[0].DamageType) : null,
                        ["nativeIcon"] = native.Icon == null ? null : native.Icon.name,
                        ["nativeProjectile"] = native.ComponentsArray.OfType<AbilityDeliverProjectile>().Select(value =>
                            value.Projectiles == null || value.Projectiles.Length == 0 || value.Projectiles[0] == null ? null : value.Projectiles[0].AssetGuid).FirstOrDefault(),
                        ["unchanged"] = unchanged });
                }
                record["nativeDonors"] = donors; record["grantLog"] = ExpandedSummoningSpecialBuilder.NativeMephitBreathGrants;
                Sprint25Check(prefix + "native-donors-untouched", donorsOk, new JObject { ["donors"] = donors.DeepClone(),
                    ["grantLog"] = ExpandedSummoningSpecialBuilder.NativeMephitBreathGrants },
                    "The native summoned-mephit donors still grant and cast their own native breaths (actual installed damage recorded), and keep their native icon; only the KMG units were redirected.");

                var pyro = (BlueprintAbility)symbols["KMG.Summoning.Special.MagmaMephit.SpellLikeOne"];
                AbilitySpawnFx[] flashes = (pyro.ComponentsArray ?? new BlueprintComponent[0]).OfType<AbilitySpawnFx>().ToArray();
                bool flashConfigured = flashes.Length == 1 && flashes[0].Time == AbilitySpawnFxTime.OnApplyEffect &&
                    flashes[0].Anchor == AbilitySpawnFxAnchor.Caster && !flashes[0].UsesSelectedTarget && flashes[0].PrefabLink != null &&
                    flashes[0].PrefabLink.AssetId == MephitBreathPolicy.PyrotechnicsFlashPrefabAssetId && !flashes[0].DestroyOnCast;
                Sprint25Check(prefix + "pyrotechnics-flash-configured", flashConfigured, new JObject { ["ability"] = pyro.AssetGuid,
                    ["spawners"] = flashes.Length, ["time"] = flashes.Length == 1 ? flashes[0].Time.ToString() : null,
                    ["anchor"] = flashes.Length == 1 ? flashes[0].Anchor.ToString() : null,
                    ["prefab"] = flashes.Length == 1 && flashes[0].PrefabLink != null ? flashes[0].PrefabLink.AssetId : null },
                    "Pyrotechnics carries one caster-anchored, non-target OnApplyEffect spawn of the native Flare Burst blinding burst.");

                // 2. Ten fresh summons; descriptions before any cast.
                string[] keys = finish ? new[] { "salt-mephit", "water-mephit", "earth-mephit", "magma-mephit" } :
                    MephitBreathPolicy.Rows.Select(row => row.Key).ToArray();
                string[] describe = finish ? new[] { "salt-mephit" } : keys;
                for (int index = 0; index < keys.Length; index++)
                {
                    UnitEntityData summon = CastExpandedSummoningOwnTier(fixture, keys[index]);
                    owned.Add(summon); units[keys[index]] = summon;
                    Sprint28SetBrainActivePreservingActions(summon, false);
                    summon.Commands.InterruptAll(command => true); summon.Commands.RemoveFinishedAndUpdateQueue();
                    Vector3 offset = Quaternion.Euler(0f, index * (360f / keys.Length), 0f) * Vector3.forward * 6f;
                    PlaceExpandedSummoningUnit(summon, ObstacleAnalyzer.GetNearestNode(centre + offset).clampedPosition);
                }
                for (int frame = 0; frame < 10; frame++) { if (!mode.Current) throw new InvalidOperationException("Lost native Default."); yield return 0; }
                var live = new JObject(); bool liveOk = true;
                foreach (string key in keys)
                {
                    UnitEntityData unit = units[key]; BlueprintAbility breath = breathOf(key);
                    BlueprintAbility[] liveBrain = unit.Brain == null ? new BlueprintAbility[0] : unit.Brain.Actions
                        .Select(action => action.Blueprint as BlueprintAiCastSpell).Where(value => value != null)
                        .Select(value => value.Ability).ToArray();
                    int owns = unit.Descriptor.Abilities.Enumerable.Count(value => ReferenceEquals(value.Blueprint, breath));
                    int natives = unit.Descriptor.Abilities.Enumerable.Count(value => nativeBreaths.Contains(value.Blueprint));
                    bool exact = owns == 1 && natives == 0 && liveBrain.Count(value => ReferenceEquals(value, breath)) == 1 &&
                        !liveBrain.Any(value => nativeBreaths.Contains(value)) && ReferenceEquals(unit.Blueprint,
                            symbols["KMG.Summoning.Unit." + ExpandedSummoningSpecialBuilder.MephitToken(key)]);
                    liveOk &= exact;
                    live[key] = new JObject { ["unit"] = unit.UniqueId, ["blueprint"] = unit.Blueprint.name, ["ownedBreathFacts"] = owns,
                        ["nativeBreathFacts"] = natives, ["liveBrainCastsOwned"] = liveBrain.Count(value => ReferenceEquals(value, breath)),
                        ["liveBrainCastsNative"] = liveBrain.Count(value => nativeBreaths.Contains(value)), ["exact"] = exact };
                }
                record["live"] = live;
                Sprint25Check(prefix + "live-breath-facts", liveOk, (JObject)live.DeepClone(),
                    "Each fresh live mephit holds exactly one owned breath fact and no native breath fact, and its live brain casts the owned breath.");

                screens = new NativeIconScreenEvidence(_request);
                var descriptions = new JArray();
                foreach (string key in describe)
                {
                    BlueprintAbility breath = breathOf(key);
                    var target = new R3DescriptionTarget { Label = key + "-breath", Creature = key, Blueprint = breath,
                        Duration = R3Text(breath.LocalizedDuration), SavingThrow = R3Text(breath.LocalizedSavingThrow) };
                    Ability fact = units[key].Descriptor.Abilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, breath));
                    var window = ui.DescriptionController.DescWindow;
                    if (R3UnrelatedModal()) throw new InvalidOperationException("Unrelated native modal shown before opening a description; UI interaction stopped.");
                    if (window.gameObject.activeInHierarchy || ui.TooltipsController.ContextTooltipData != null)
                        throw new InvalidOperationException("A foreign description window or hover is open.");
                    var data = new TooltipData { Type = TooltipType.Ability, Ability = fact };
                    ui.DescriptionController.HandleOpenDescription(data); openDescription = data;
                    var settle = System.Diagnostics.Stopwatch.StartNew();
                    for (int frame = 0; frame < 600 && settle.Elapsed.TotalSeconds < 8 && !R3DescriptionOwned(mode, fact, data); frame++)
                    {
                        if (!mode.Current || !Cleanup2NoNarrativeOverlay()) throw new InvalidOperationException("Native Default lost while opening a description.");
                        if (R3UnrelatedModal()) throw new InvalidOperationException("Unrelated native modal shown while opening a description; UI interaction stopped.");
                        yield return 0;
                    }
                    if (!R3DescriptionOwned(mode, fact, data)) throw new InvalidOperationException("Exact owned description window did not become visible: " + target.Label);
                    Func<bool> owns = () => R3DescriptionOwned(mode, fact, data);
                    for (int frame = 0; frame < 20; frame++)
                    {
                        yield return 0;
                        if (!owns()) throw new InvalidOperationException("Description ownership lost during native layout: " + target.Label);
                    }
                    JObject construction = R3Construction(data, fact);
                    JObject card = R3DescriptionCard(target);
                    foreach (int frame in screens.Capture("r4-description-" + target.Label, card, owns)) yield return frame;
                    card = R3DescriptionCard(target);
                    bool passed = (bool)card["contentCorrect"] && (bool)card["rowsVisible"];
                    var result = new JObject { ["construction"] = construction, ["card"] = card, ["passed"] = passed };
                    descriptions.Add(result);
                    Sprint25Check(prefix + "render-" + target.Label, passed, (JObject)result.DeepClone(),
                        "Captured native description window for the live breath fact shows its name, description, its own visible icon and only its intended rows, unclipped, with no <null>.");
                    ui.DescriptionController.HandleCloseDescriptionWindow(window);
                    for (int frame = 0; frame < 120 && window.gameObject.activeInHierarchy; frame++) yield return 0;
                    if (window.gameObject.activeInHierarchy) throw new InvalidOperationException("Native description window did not close.");
                    openDescription = null;
                    if (R3UnrelatedModal()) throw new InvalidOperationException("Unrelated native modal shown after closing a description; UI interaction stopped.");
                }
                record["descriptions"] = descriptions;

                // 3. Executed casts. Only water, earth and magma stay; the rest
                // leave so no ally stands in a native-backed cone.
                var keep = new HashSet<string>(new[] { "water-mephit", "earth-mephit", "magma-mephit" });
                var leaving = owned.Where(value => !keep.Contains(keys.First(key => ReferenceEquals(units[key], value)))).ToArray();
                DisposeExpandedSummoningUnits(fixture.Created, leaving);
                owned.RemoveAll(value => leaving.Contains(value));
                for (int frame = 0; frame < 10; frame++) yield return 0;
                UnitEntityData water = units["water-mephit"], earth = units["earth-mephit"], magma = units["magma-mephit"];
                Vector3 forward = Vector3.forward;
                Vector3 site = ObstacleAnalyzer.GetNearestNode(centre + forward * 12f).clampedPosition;
                PlaceExpandedSummoningUnit(hostile, site);
                PlaceExpandedSummoningUnit(water, ObstacleAnalyzer.GetNearestNode(centre + forward * 9.5f).clampedPosition);
                PlaceExpandedSummoningUnit(earth, ObstacleAnalyzer.GetNearestNode(centre + Vector3.right * 9f).clampedPosition);
                PlaceExpandedSummoningUnit(magma, ObstacleAnalyzer.GetNearestNode(centre + Vector3.left * 9f).clampedPosition);
                for (int frame = 0; frame < 15; frame++) yield return 0;
                recorder = new R4MephitRecorder();
                EventBus.Subscribe(recorder); subscribed = true;
                trace = new R4SpawnTrace();
                saves = new Sprint26Saves(hostile) { DesiredRoll = 2 };

                if (finish)
                {
                    foreach (int frame in R4AllySafety(record, prefix, hostile, water, earth, centre, breathOf("water-mephit"),
                        recorder, trace, saves, screens, worldOwned)) yield return frame;
                }
                else
                {
                // Water: acid.
                var waterRow = new JObject();
                BlueprintAbility waterBreath = breathOf("water-mephit");
                trace.Owner = water;
                int damageBefore = recorder.Damage.Count;
                waterRow["distance"] = Vector3.Distance(water.Position, hostile.Position);
                foreach (int frame in R4RunCast(water, waterBreath, new TargetWrapper(hostile), waterRow,
                    () => trace.Projectiles.Count > 0,
                    () => screens.Capture("r4-water-breath-during", new JObject { ["caster"] = water.UniqueId,
                        ["projectiles"] = trace.Projectiles.DeepClone() }, worldOwned))) yield return frame;
                for (int frame = 0; frame < 20; frame++) yield return 0;
                RuleDealDamage[] waterEvents = recorder.Damage.Skip(damageBefore).Where(evt => ReferenceEquals(evt.Initiator, water) &&
                    ReferenceEquals(evt.Target, hostile) && ReferenceEquals(evt.SourceAbility, waterBreath)).ToArray();
                RuleDealDamage[] waterStray = recorder.Damage.Skip(damageBefore).Where(evt => ReferenceEquals(evt.SourceAbility, waterBreath) &&
                    !ReferenceEquals(evt.Target, hostile)).ToArray();
                waterRow["events"] = new JArray(waterEvents.Select(R4DamageEvent));
                waterRow["strayTargets"] = new JArray(waterStray.Select(R4DamageEvent));
                waterRow["projectiles"] = trace.Projectiles.DeepClone();
                bool waterAcid = waterEvents.Length >= 1 && waterEvents.All(evt => evt.ResultDamage != null && evt.ResultDamage.Any() &&
                    evt.ResultDamage.All(value => value.Source is EnergyDamage && ((EnergyDamage)value.Source).EnergyType == DamageEnergyType.Acid)) &&
                    trace.Projectiles.OfType<JObject>().Any(value => (string)value["projectile"] == MephitBreathPolicy.AcidConeProjectileGuid) &&
                    (bool)waterRow["finished"] && waterStray.Length == 0;
                waterRow["hostileSaves"] = saves.Rows.DeepClone();
                record["water"] = waterRow;
                Sprint25Check(prefix + "water-acid-executed", waterAcid, (JObject)waterRow.DeepClone(),
                    "One executed KMG Water Mephit breath, correlated by caster, owned ability and target, deals only acid damage packets and launches the native acid cone.");

                // Earth: the changed physical path.
                PlaceExpandedSummoningUnit(water, ObstacleAnalyzer.GetNearestNode(centre + Vector3.back * 9f).clampedPosition);
                PlaceExpandedSummoningUnit(earth, ObstacleAnalyzer.GetNearestNode(centre + forward * 9.5f).clampedPosition);
                for (int frame = 0; frame < 15; frame++) yield return 0;
                var earthRow = new JObject();
                BlueprintAbility earthBreath = breathOf("earth-mephit");
                trace.Owner = earth; int projectilesBefore = trace.Projectiles.Count;
                damageBefore = recorder.Damage.Count;
                earthRow["distance"] = Vector3.Distance(earth.Position, hostile.Position);
                foreach (int frame in R4RunCast(earth, earthBreath, new TargetWrapper(hostile), earthRow,
                    () => trace.Projectiles.Count > projectilesBefore,
                    () => screens.Capture("r4-earth-breath-during", new JObject { ["caster"] = earth.UniqueId,
                        ["projectiles"] = trace.Projectiles.DeepClone() }, worldOwned))) yield return frame;
                for (int frame = 0; frame < 20; frame++) yield return 0;
                RuleDealDamage[] earthEvents = recorder.Damage.Skip(damageBefore).Where(evt => ReferenceEquals(evt.Initiator, earth) &&
                    ReferenceEquals(evt.Target, hostile) && ReferenceEquals(evt.SourceAbility, earthBreath)).ToArray();
                earthRow["events"] = new JArray(earthEvents.Select(R4DamageEvent));
                earthRow["projectiles"] = new JArray(trace.Projectiles.Skip(projectilesBefore));
                bool earthBludgeoning = earthEvents.Length >= 1 && earthEvents.All(evt => evt.ResultDamage != null && evt.ResultDamage.Any() &&
                    evt.ResultDamage.All(value => value.Source is PhysicalDamage &&
                        ((PhysicalDamage)value.Source).Form == PhysicalDamageForm.Bludgeoning)) && (bool)earthRow["finished"];
                earthRow["hostileSaves"] = saves.Rows.DeepClone();
                record["earth"] = earthRow;
                Sprint25Check(prefix + "earth-bludgeoning-executed", earthBludgeoning, (JObject)earthRow.DeepClone(),
                    "One executed KMG Earth Mephit breath, correlated by caster, owned ability and target, deals only physical bludgeoning packets (the changed explicit physical path), never acid.");

                }

                // Pyrotechnics: cancelled command, then one executed cast.
                PlaceExpandedSummoningUnit(water, ObstacleAnalyzer.GetNearestNode(centre + Vector3.back * 9f).clampedPosition);
                PlaceExpandedSummoningUnit(earth, ObstacleAnalyzer.GetNearestNode(centre + Vector3.right * 9f).clampedPosition);
                PlaceExpandedSummoningUnit(magma, ObstacleAnalyzer.GetNearestNode(centre + forward * 9f).clampedPosition);
                for (int frame = 0; frame < 15; frame++) yield return 0;
                var pyroRow = new JObject();
                trace.Owner = magma; trace.FlashPrefab = flashes[0].PrefabLink.Load();
                if (trace.FlashPrefab == null) throw new InvalidOperationException("Native Flare Burst blinding prefab did not load.");
                Ability pyroFact = magma.Descriptor.Abilities.GetAbility(pyro);
                var pyroResource = pyro.ComponentsArray.OfType<AbilityResourceLogic>().Single().RequiredResource;
                int resourceBefore = magma.Descriptor.Resources.GetResourceAmount(pyroResource);
                int appliedBefore = recorder.Applied.Count(context => ReferenceEquals(context.AbilityBlueprint, pyro));
                var cancelled = new UnitUseAbility(new AbilityData(pyroFact), new TargetWrapper(magma));
                magma.Commands.Run(cancelled);
                bool queued = magma.Commands.Contains(cancelled), startedBeforeCancel = cancelled.IsStarted;
                magma.Commands.InterruptAll(true); magma.Commands.RemoveFinishedAndUpdateQueue();
                for (int frame = 0; frame < 45; frame++) yield return 0;
                bool cancelClean = !startedBeforeCancel && trace.Flashes.Count == 0 &&
                    recorder.Applied.Count(context => ReferenceEquals(context.AbilityBlueprint, pyro)) == appliedBefore &&
                    magma.Descriptor.Resources.GetResourceAmount(pyroResource) == resourceBefore;
                Sprint25Check(prefix + "pyrotechnics-cancel-no-flash", cancelClean, new JObject { ["queued"] = queued,
                    ["startedBeforeCancel"] = startedBeforeCancel, ["flashes"] = trace.Flashes.Count,
                    ["resourceBefore"] = resourceBefore, ["resourceAfter"] = magma.Descriptor.Resources.GetResourceAmount(pyroResource) },
                    "A queued Pyrotechnics command cancelled before it starts spawns no flash, applies nothing and spends nothing.");

                foreach (int frame in screens.Capture("r4-pyrotechnics-before", new JObject { ["caster"] = magma.UniqueId,
                    ["flashes"] = trace.Flashes.Count }, worldOwned)) yield return frame;
                float castRealtime = Time.realtimeSinceStartup;
                var during = new JObject();
                foreach (int frame in R4RunCast(magma, pyro, new TargetWrapper(magma), pyroRow,
                    () => trace.Flashes.Count > 0,
                    () => R4FlashDuring(screens, trace, worldOwned, during))) yield return frame;
                GameObject instance = trace.Flashes.FirstOrDefault();
                var lifetime = new JArray();
                int retireFrame = -1; float retireRealtime = -1f;
                var retire = System.Diagnostics.Stopwatch.StartNew();
                for (int frame = 0; frame < 1800 && retire.Elapsed.TotalSeconds < 15; frame++)
                {
                    bool alive = instance != null && instance.activeInHierarchy;
                    if (frame % 15 == 0) lifetime.Add(new JObject { ["frame"] = Time.frameCount, ["realtime"] = Time.realtimeSinceStartup,
                        ["alive"] = alive, ["parentIsCaster"] = instance != null && magma.View != null && instance.transform.IsChildOf(magma.View.transform) });
                    if (!alive) { retireFrame = Time.frameCount; retireRealtime = Time.realtimeSinceStartup; break; }
                    yield return 0;
                }
                for (int frame = 0; frame < 10; frame++) yield return 0;
                foreach (int frame in screens.Capture("r4-pyrotechnics-after", new JObject { ["caster"] = magma.UniqueId,
                    ["flashAlive"] = instance != null && instance.activeInHierarchy }, worldOwned)) yield return frame;
                int applied = recorder.Applied.Count(context => ReferenceEquals(context.AbilityBlueprint, pyro)) - appliedBefore;
                var blinded = (Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff)symbols["KMG.Summoning.Special.MagmaMephit.PyrotechnicsBlindedState"];
                pyroRow["castRealtime"] = castRealtime;
                pyroRow["flashes"] = trace.FlashRows.DeepClone();
                pyroRow["lifetime"] = lifetime;
                pyroRow["retireFrame"] = retireFrame; pyroRow["retireRealtime"] = retireRealtime;
                pyroRow["appliedExecutions"] = applied;
                pyroRow["resourceBefore"] = resourceBefore; pyroRow["resourceAfter"] = magma.Descriptor.Resources.GetResourceAmount(pyroResource);
                pyroRow["hostileBlinded"] = hostile.Buffs.GetBuff(blinded) != null;
                pyroRow["casterBlinded"] = magma.Buffs.GetBuff(blinded) != null;
                bool oneFlash = trace.Flashes.Count == 1 && trace.FlashRows.Count == 1 && (bool)trace.FlashRows[0]["onOwner"] &&
                    applied == 1 && retireFrame >= 0 && resourceBefore - magma.Descriptor.Resources.GetResourceAmount(pyroResource) == 1 &&
                    (bool)pyroRow["finished"] && magma.Buffs.GetBuff(blinded) == null;
                pyroRow["during"] = during;
                record["pyrotechnics"] = pyroRow;
                bool capturedLive = pyroRow["triggerFired"] != null && (bool)pyroRow["triggerFired"] &&
                    during["requestFrame"] != null && (int)during["requestFrame"] >= 0 && (bool)during["aliveAtRequest"] &&
                    (bool)during["exactInstance"] && (string)during["status"] == "captured-native-screen-awaiting-visual-inspection" &&
                    retireFrame > (int)during["requestFrame"];
                Sprint25Check(prefix + "pyrotechnics-flash-captured-live", capturedLive, (JObject)during.DeepClone(),
                    "The during image was requested while the exact flash instance from the executed cast was alive (checked at the request frame), completed, and that instance retired naturally afterwards. Visual acceptance still requires inspecting the image for the flash.");
                Sprint25Check(prefix + "pyrotechnics-one-flash", oneFlash, (JObject)pyroRow.DeepClone(),
                    "One executed Pyrotechnics cast applies once, spends exactly one use, spawns exactly one native blinding flash on the caster, and that instance retires on its own; the caster is not blinded.");
            }
            finally
            {
                var errors = new JArray();
                Action<string, Action> restore = (name, action) => { try { action(); } catch (Exception error) { errors.Add(name + ":" + error.Message); } };
                if (openDescription != null)
                    restore("description", () => {
                        var window = ui.DescriptionController.DescWindow;
                        if (ReferenceEquals(window.Data, openDescription)) ui.DescriptionController.HandleCloseDescriptionWindow(window);
                    });
                restore("trace", () => { if (trace != null) trace.Dispose(); });
                restore("saves", () => { if (saves != null) saves.Dispose(); });
                restore("recorder", () => { if (subscribed) EventBus.Unsubscribe(recorder); });
                restore("screens", () => { if (screens != null) screens.Dispose(); });
                restore("combat", () => {
                    foreach (UnitEntityData unit in owned.Concat(new[] { fixture.Hostile, fixture.Caster }).Where(value => value != null && !value.Destroyed))
                    {
                        unit.Commands.InterruptAll(command => true); unit.Commands.RemoveFinishedAndUpdateQueue();
                        if (unit.IsInCombat) unit.LeaveCombat();
                    }
                });
                restore("hostile", () => { if (!fixture.Hostile.Destroyed) PlaceExpandedSummoningUnit(fixture.Hostile, hostilePosition); });
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                Sprint25Check(prefix + "owned-ui-restored", errors.Count == 0 &&
                    !ui.DescriptionController.DescWindow.gameObject.activeInHierarchy,
                    new JObject { ["errors"] = errors },
                    "Owned description window, spawn readers, event subscription, combat and positions restore exactly.");
                record["restoreErrors"] = errors;
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"),
                    record.ToString(Formatting.Indented));
            }
        }

        // The helper requests the screenshot (ScreenCapture) in the frame of
        // its first yield after the request; liveness is sampled exactly then.
        // The effect is not required to survive while the file is written.
        private IEnumerable<int> R4FlashDuring(NativeIconScreenEvidence screens, R4SpawnTrace trace, Func<bool> worldOwned, JObject during)
        {
            GameObject instance = trace.Flashes.FirstOrDefault();
            during["triggerFrame"] = Time.frameCount; during["requestFrame"] = -1; during["aliveAtRequest"] = false;
            during["exactInstance"] = instance != null && trace.Flashes.Count == 1 && trace.FlashRows.Count == 1 && (bool)trace.FlashRows[0]["onOwner"];
            foreach (int frame in screens.Capture("r4-pyrotechnics-during", new JObject {
                ["flashes"] = trace.FlashRows.DeepClone(), ["realtime"] = Time.realtimeSinceStartup }, worldOwned))
            {
                JObject last = screens.LastRecord;
                if ((int)during["requestFrame"] < 0 && last != null && (string)last["stage"] == "r4-pyrotechnics-during" &&
                    last["requestedFrame"] != null && (int)last["requestedFrame"] == Time.frameCount)
                {
                    during["requestFrame"] = Time.frameCount; during["requestRealtime"] = Time.realtimeSinceStartup;
                    during["aliveAtRequest"] = instance != null && instance.activeInHierarchy;
                    during["file"] = last["file"];
                }
                yield return frame;
            }
            JObject done = screens.LastRecord;
            during["status"] = done == null ? null : done["status"];
            during["completedFrame"] = done == null ? null : done["completedFrame"];
        }

        // Charter ally-safety, one representative native case: a fresh KMG
        // Water Mephit breathes at the disposable hostile with a fresh allied
        // Earth Mephit also inside the cone. The cone must deliver to both;
        // only the hostile may take damage.
        private IEnumerable<int> R4AllySafety(JObject record, string prefix, UnitEntityData hostile, UnitEntityData water,
            UnitEntityData ally, Vector3 centre, BlueprintAbility waterBreath, R4MephitRecorder recorder, R4SpawnTrace trace,
            Sprint26Saves saves, NativeIconScreenEvidence screens, Func<bool> worldOwned)
        {
            var row = new JObject();
            Vector3 site = ObstacleAnalyzer.GetNearestNode(centre + Vector3.forward * 12f).clampedPosition;
            PlaceExpandedSummoningUnit(hostile, site);
            Vector3 toCentre = centre - site; toCentre.y = 0f;
            Vector3 dir = toCentre.sqrMagnitude < 0.01f ? Vector3.back : toCentre.normalized;
            Vector3 side = new Vector3(dir.z, 0f, -dir.x);
            JObject geometry = null; bool layout = false;
            foreach (float back in new[] { 2.6f, 3.0f, 2.2f })
            {
                foreach (float lateral in new[] { 0.9f, -0.9f, 1.3f, -1.3f })
                {
                    PlaceExpandedSummoningUnit(water, ObstacleAnalyzer.GetNearestNode(site + dir * back).clampedPosition);
                    PlaceExpandedSummoningUnit(ally, ObstacleAnalyzer.GetNearestNode(site + side * lateral).clampedPosition);
                    for (int frame = 0; frame < 10; frame++) yield return 0;
                    Vector3 aim = hostile.Position - water.Position; aim.y = 0f;
                    Vector3 toAlly = ally.Position - water.Position; toAlly.y = 0f;
                    float angle = Vector3.Angle(aim, toAlly);
                    geometry = new JObject { ["back"] = back, ["lateral"] = lateral, ["hostileDistance"] = aim.magnitude,
                        ["allyDistance"] = toAlly.magnitude, ["allyAngle"] = angle };
                    layout = aim.magnitude >= 1.0f && aim.magnitude <= 4.0f && toAlly.magnitude >= 0.8f &&
                        toAlly.magnitude <= 4.0f && angle <= 20f;
                    if (layout) break;
                }
                if (layout) break;
            }
            row["geometry"] = geometry; row["layout"] = layout;
            if (!layout)
            {
                record["allySafety"] = row;
                Sprint25Check(prefix + "ally-safety-water-breath", false, (JObject)row.DeepClone(),
                    "A bounded layout places the ally inside the Water breath cone with the hostile.");
                throw new InvalidOperationException("Ally-safety layout not obtained.");
            }
            trace.Owner = water;
            int damageBefore = recorder.Damage.Count, deliveredBefore = recorder.Delivered.Count, projectilesBefore = trace.Projectiles.Count;
            int allyDamageBefore = ally.Descriptor.Damage;
            foreach (int frame in R4RunCast(water, waterBreath, new TargetWrapper(hostile), row,
                () => trace.Projectiles.Count > projectilesBefore,
                () => screens.Capture("r4-ally-safety-water-breath", new JObject { ["caster"] = water.UniqueId,
                    ["ally"] = ally.UniqueId, ["hostile"] = hostile.UniqueId }, worldOwned))) yield return frame;
            for (int frame = 0; frame < 20; frame++) yield return 0;
            var events = recorder.Damage.Skip(damageBefore).Where(evt => ReferenceEquals(evt.SourceAbility, waterBreath)).ToArray();
            var delivered = recorder.Delivered.Skip(deliveredBefore).Where(pair => ReferenceEquals(pair.Key.AbilityBlueprint, waterBreath))
                .Select(pair => pair.Value).ToArray();
            var hostileEvents = events.Where(evt => ReferenceEquals(evt.Target, hostile) && ReferenceEquals(evt.Initiator, water)).ToArray();
            var allyEvents = events.Where(evt => ReferenceEquals(evt.Target, ally)).ToArray();
            row["deliveredTo"] = new JArray(delivered.Select(unit => unit.UniqueId));
            row["allyDelivered"] = delivered.Contains(ally); row["hostileDelivered"] = delivered.Contains(hostile);
            row["hostileEvents"] = new JArray(hostileEvents.Select(R4DamageEvent));
            row["allyEvents"] = new JArray(allyEvents.Select(R4DamageEvent));
            row["allyDamageBefore"] = allyDamageBefore; row["allyDamageAfter"] = ally.Descriptor.Damage;
            row["projectiles"] = new JArray(trace.Projectiles.Skip(projectilesBefore));
            row["hostileSaves"] = saves.Rows.DeepClone();
            bool safe = (bool)row["finished"] && delivered.Contains(ally) && delivered.Contains(hostile) && allyEvents.Length == 0 &&
                ally.Descriptor.Damage == allyDamageBefore && hostileEvents.Length >= 1 &&
                hostileEvents.All(evt => evt.ResultDamage != null && evt.ResultDamage.Any() && evt.ResultDamage.All(value =>
                    value.Source is EnergyDamage && ((EnergyDamage)value.Source).EnergyType == DamageEnergyType.Acid));
            record["allySafety"] = row;
            Sprint25Check(prefix + "ally-safety-water-breath", safe, (JObject)row.DeepClone(),
                "One executed KMG Water Mephit breath delivers to both an allied KMG summon and the hostile inside its cone; the ally takes no damage event and no damage, while the hostile still takes acid damage (controlled failed save).");
        }
    }
}
