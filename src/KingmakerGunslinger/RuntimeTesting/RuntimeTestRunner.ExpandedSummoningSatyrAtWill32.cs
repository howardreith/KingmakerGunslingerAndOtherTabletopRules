using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
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
        // Bounded extension of the existing isolated combat fixture. Native
        // level-up supplies actual HD; no product profile, AI score, spell,
        // save roll, action or effect is replaced to select these at-will SLAs.
        private IEnumerable<int> ReviewSatyrAtWill32(ExpandedSummoningCorrectionFixture fixture,
            bool sleep, bool turnBased)
        {
            const string prefix = "KMG_Summoning_FeyCelestial_Satyr_";
            string suffix = sleep ? "Sleep" : "CharmPerson";
            string id = "satyr-atwill-" + (sleep ? "sleep" : "charm") + (turnBased ? "-turn" : "");
            var game = Game.Instance;
            var target = fixture.Hostile;
            var extra = new List<UnitEntityData>();
            var subjects = new List<UnitEntityData> { target };
            var immune = new List<UnitEntityData>();
            var capturedEffects = new List<Buff>();
            UnitEntityData owner = null, sibling = null, sentinel = null, ally = null;
            BlueprintFeature charmImmunity = null;
            BlueprintUnit allyBlueprint = null;
            Dretch30CloudSaves saves = null;
            Lillend32CommandStarts starts = null;
            int oldWill = target.Stats.SaveWill.BaseValue, oldAc = target.Stats.AC.BaseValue;
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = game.IsPaused;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                game.TurnBasedCombatController.Activate(); game.IsPaused = false;
                ResetExpandedSummoningHostile(fixture);
                PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 40f);
                owner = CastExpandedSummoningOwnTier(fixture, "satyr");
                sibling = CastExpandedSummoningOwnTier(fixture, "satyr");
                foreach (var actor in new[] { owner, sibling })
                {
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    actor.Commands.InterruptAll(true); actor.Commands.RemoveFinishedAndUpdateQueue();
                }
                var ability = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name == prefix + suffix);
                var resource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == prefix + "FearResource");
                var policy = fixture.Blueprints.OfType<SatyrControlEligibility>().Single(value =>
                    ReferenceEquals(value.OwningBlueprint, owner.Blueprint) && ReferenceEquals(value.Ability, ability));
                var fighter = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                    "48ac8db94d5de7645906c7d0ad3bcfbd", "native fighter for disposable HD");
                int hd = sleep ? 2 : 5;
                int levels = hd - target.Descriptor.Progression.CharacterLevel;
                if (levels < 0) throw new InvalidOperationException("At-will target already exceeds selected HD.");
                ElementalSpellAffinityScenario.Advance(target.Descriptor, fighter, levels);
                target.Stats.HitPoints.BaseValue = 100000;
                target.Stats.SaveWill.BaseValue = -100; target.Stats.AC.BaseValue = -100;
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction,
                    originAllyExclusionRadius: new Feet(30).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(target, far);
                var forward = (far - origin).normalized;
                var side = Vector3.Cross(Vector3.up, forward).normalized;
                var placementRows = new JArray();
                Func<BlueprintUnit, Vector3, int, UnitEntityData> spawn = (blueprint, requested, hitDice) =>
                {
                    // A navmesh point alone need not be visible down this indoor
                    // lane. Reuse native point LOS before creating an actor;
                    // the final actor LOS and memory checks still must pass.
                    var candidates = hitDice == 2 ? new[] { requested,
                        far - forward * .9f + side * .6f, far - forward * .9f - side * .6f,
                        far - forward * 1.5f, far + forward * .5f + side * .9f,
                        far + forward * .5f - side * .9f, far + side * .9f, far - side * .9f } : new[] { requested };
                    Vector3? selected = null;
                    foreach (var candidate in candidates)
                    {
                        var point = AstarPath.active.GetNearest(candidate);
                        if (point.node == null || !point.node.Walkable || Vector3.Distance(point.clampedPosition, candidate) > .3f ||
                            hitDice == 2 && Vector3.Distance(point.clampedPosition, far) > 1.5f ||
                            subjects.Concat(extra).Any(value => Vector3.Distance(value.Position, point.clampedPosition) < .8f) ||
                            !owner.HasLOS(point.clampedPosition + Kingmaker.Visual.FogOfWar.LineOfSightGeometry.EyeShift, origin)) continue;
                        selected = point.clampedPosition; break;
                    }
                    if (!selected.HasValue) throw new InvalidOperationException("At-will disposable actor lacks a separated walkable native-LOS point.");
                    var actor = game.EntityCreator.SpawnUnit(blueprint, selected.Value, Quaternion.identity, fixture.Scene);
                    extra.Add(actor); fixture.Created.Add(actor);
                    SetExpandedSummoningBrainActive(actor, false);
                    ElementalSpellAffinityScenario.Advance(actor.Descriptor, fighter,
                        hitDice - actor.Descriptor.Progression.CharacterLevel);
                    actor.Stats.HitPoints.BaseValue = 100000;
                    actor.Stats.SaveWill.BaseValue = -100; actor.Stats.AC.BaseValue = -100;
                    PlaceExpandedSummoningUnit(actor, selected.Value);
                    placementRows.Add(new JObject { ["unit"] = actor.UniqueId, ["requested"] = requested.ToString(),
                        ["selected"] = selected.Value.ToString(), ["pointLos"] = true });
                    return actor;
                };
                var descriptor = ScriptableObject.CreateInstance<BuffDescriptorImmunity>();
                descriptor.Descriptor = SpellDescriptor.Charm;
                charmImmunity = ScriptableObject.CreateInstance<BlueprintFeature>();
                charmImmunity.name = "KMG_Runtime_Satyr32_AtWillCharmImmunity";
                charmImmunity.Ranks = 1; charmImmunity.ComponentsArray = new BlueprintComponent[] { descriptor };
                if (sleep)
                {
                    subjects.Add(spawn(fixture.HostileBlueprint, far + side * .9f, 2));
                    subjects.Add(spawn(fixture.HostileBlueprint, far - side * .9f, 2));
                    immune.AddRange(subjects);
                    allyBlueprint = UnityEngine.Object.Instantiate(fixture.HostileBlueprint);
                    allyBlueprint.name = "KMG_Runtime_Satyr32_AtWillAlliedTarget";
                    ExpandedSummoningFields(typeof(BlueprintUnit)).Single(value =>
                        value.Name == "Faction" || value.Name == "m_Faction").SetValue(allyBlueprint, fixture.Caster.Blueprint.Faction);
                    ally = spawn(allyBlueprint, far + forward * .9f, 2);
                }
                // Above-four-HD, Charm-immune enemy preserves actual combat
                // through the primary's condition/faction change.
                sentinel = spawn(fixture.HostileBlueprint, origin + forward * 4.5f, 5);
                immune.Add(sentinel);
                foreach (var actor in immune) actor.Descriptor.AddFact(charmImmunity);
                int fearBefore = owner.Descriptor.Resources.GetResourceAmount(resource);
                owner.Descriptor.Resources.Spend(resource, fearBefore);
                var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(ability));
                var allTargets = subjects.Concat(new[] { sentinel, ally }.Where(value => value != null)).ToArray();
                foreach (var actor in allTargets.Concat(new[] { owner }))
                {
                    actor.Wake(0f);
                    if (!game.State.AwakeUnits.Contains(actor)) game.State.AwakeUnits.Add(actor);
                    if (owner.IsEnemy(actor)) { owner.Memory.Add(actor); actor.Memory.Add(owner); }
                    actor.SightCache.ClearIfNeeded();
                }
                var memory = new Kingmaker.Controllers.Units.UnitMemoryController();
                memory.Tick();
                foreach (var actor in subjects.Concat(new[] { owner, sentinel })) actor.JoinCombat();
                game.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                // UnitGroupMemory.Add initializes Visible=false. The Dialog
                // clock does not run UnitMemoryController; yield for actual
                // awake-group registration, then refresh native visibility.
                // No memory-visible, LOS or faction result is supplied.
                int visibilityFrames = 0;
                var visibilityDeadline = DateTime.UtcNow.AddSeconds(8);
                do
                {
                    game.IsPaused = false; yield return 0; visibilityFrames++;
                    owner.SightCache.ClearIfNeeded();
                    foreach (var actor in allTargets) actor.SightCache.ClearIfNeeded();
                    memory.Tick();
                } while (DateTime.UtcNow < visibilityDeadline && visibilityFrames < 480 &&
                    (visibilityFrames < 8 || subjects.Concat(new[] { sentinel }).Any(actor =>
                        !owner.HasLOS(actor) || !owner.Memory.ContainsVisible(actor))));
                var admission = new JObject();
                if (turnBased)
                {
                    foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                    var deadline = DateTime.UtcNow.AddSeconds(30);
                    while (DateTime.UtcNow < deadline && !Sprint18OwnerIsActing(owner)) yield return 0;
                    if (!(bool)admission["admitted"] || !Sprint18OwnerIsActing(owner))
                        throw new InvalidOperationException("At-will exact-owner native turn admission failed: " + admission);
                }
                Func<UnitEntityData, float> score = actor => policy.Score(new DecisionContext {
                    Unit = owner, Target = new TargetInfo { Unit = actor } });
                bool ready = subjects.All(actor => actor.Descriptor.Progression.CharacterLevel == hd &&
                    owner.IsEnemy(actor) && owner.HasLOS(actor) && owner.Memory.ContainsVisible(actor) && score(actor) == 1f) &&
                    sentinel.Descriptor.Progression.CharacterLevel == 5 && owner.IsEnemy(sentinel) &&
                    owner.HasLOS(sentinel) && owner.Memory.ContainsVisible(sentinel) && score(sentinel) == 0f &&
                    (!sleep || !owner.IsEnemy(ally) && ally.Descriptor.Progression.CharacterLevel == 2 &&
                        subjects.All(actor => Vector3.Distance(actor.Position, ally.Position) < new Feet(10).Meters) &&
                        subjects.All(actor => subjects.All(other => Vector3.Distance(actor.Position, other.Position) < new Feet(10).Meters))) &&
                    fearBefore == 1 && owner.Descriptor.Resources.GetResourceAmount(resource) == 0 &&
                    sibling.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                    data.Spellbook == null && data.Blueprint.Type == AbilityType.SpellLike &&
                    data.RuntimeActionType == UnitCommand.CommandType.Standard && !data.RequireFullRoundAction;
                Sprint25Check(id + "-ready", ready, new JObject {
                    ["lane"] = lane, ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                    ["placements"] = placementRows, ["visibilityFrames"] = visibilityFrames,
                    ["nativeVisionMeters"] = owner.VisionRangeMeters, ["available"] = data.IsAvailable,
                    ["subjects"] = new JArray(subjects.Select(actor => new JObject { ["unit"] = actor.UniqueId,
                        ["hd"] = actor.Descriptor.Progression.CharacterLevel, ["distance"] = owner.DistanceTo(actor),
                        ["eligibility"] = score(actor), ["willBase"] = actor.Stats.SaveWill.BaseValue,
                        ["enemy"] = owner.IsEnemy(actor), ["inState"] = actor.IsInState,
                        ["dead"] = actor.Descriptor.State.IsDead, ["los"] = owner.HasLOS(actor),
                        ["visibleMemory"] = owner.Memory.ContainsVisible(actor),
                        ["canTarget"] = data.CanTarget(new TargetWrapper(actor)) })),
                    ["sentinel"] = sentinel.UniqueId, ["sentinelHd"] = sentinel.Descriptor.Progression.CharacterLevel,
                    ["sentinelEligibility"] = score(sentinel), ["ally"] = ally?.UniqueId,
                    ["turnBased"] = turnBased, ["admission"] = admission,
                    ["fearSetupBefore"] = fearBefore, ["fearSetupAfter"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                    ["siblingFear"] = sibling.Descriptor.Resources.GetResourceAmount(resource),
                    ["action"] = data.RuntimeActionType.ToString(), ["fullRound"] = data.RequireFullRoundAction },
                    "Actual native HD, Charm-only immunity and one disclosed Fear pre-spend isolate unchanged at-will choices; owned awake sentinel preserves combat. Sleep includes an allied two-HD actor inside every subject's burst.");
                if (!ready) throw new InvalidOperationException("Satyr at-will fixture preconditions failed.");
                saves = new Dretch30CloudSaves(allTargets); starts = new Lillend32CommandStarts { Owner = owner };
                EventBus.Subscribe(saves); EventBus.Subscribe(starts);
                var commands = new HashSet<UnitCommand>(); var costs = new JArray();
                UnitUseAbility cast = null; int ticks = 0, nativeTurns = 0;
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                    var deadline = DateTime.UtcNow.AddSeconds(40); int frames = 0;
                    while (DateTime.UtcNow < deadline && frames++ < 4000)
                    {
                        if (!turnBased && (frames == 1 || frames % 60 == 0 &&
                            !owner.Commands.Raw.Any(value => value != null && !value.IsFinished)))
                        { Sprint24TickExactAi(owner); ticks++; }
                        if (turnBased && Sprint18OwnerIsActing(owner)) nativeTurns++;
                        foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                        foreach (var command in starts.Commands) commands.Add(command);
                        cast = commands.OfType<UnitUseAbility>().FirstOrDefault(value =>
                            ReferenceEquals(value.Spell?.Blueprint, ability) && value.IsFinished &&
                            value.ExecutionProcess != null && value.ExecutionProcess.IsEnded);
                        if (cast != null) break;
                        game.IsPaused = false; yield return 0;
                    }
                    // Stop selecting further actions only after observing the
                    // original completed natural cast and native cost boundary.
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    var effects = subjects.SelectMany(actor => actor.Buffs.Enumerable.Where(value =>
                        ReferenceEquals(value.Context?.MaybeCaster, owner))).ToArray();
                    capturedEffects.AddRange(effects);
                    var spellSaves = saves.Rows.Where(value => (string)value["sourceAbility"] == ability.AssetGuid &&
                        (string)value["caster"] == owner.UniqueId).ToArray();
                    var applications = saves.Applications.Where(value => (string)value["sourceAbility"] == ability.AssetGuid &&
                        (string)value["caster"] == owner.UniqueId && (bool)value["applied"]).ToArray();
                    double expectedDuration = sleep ? 8 * 60 : 8 * 60 * 60;
                    bool affected = sleep ? subjects.Count(actor => actor.Descriptor.State.HasCondition(UnitCondition.Sleeping)) > 0 &&
                        subjects.Count(actor => actor.Descriptor.State.HasCondition(UnitCondition.Sleeping)) <= 2 &&
                        spellSaves.Length == 2 && spellSaves.Select(value => (string)value["target"]).Distinct().Count() == 2 &&
                        spellSaves.All(value => subjects.Any(actor => actor.UniqueId == (string)value["target"])) &&
                        !ally.Descriptor.State.HasCondition(UnitCondition.Sleeping) :
                        effects.Length > 0 && spellSaves.Length == 1 && (string)spellSaves[0]["target"] == target.UniqueId;
                    bool exact = cast != null && cast.Result == UnitCommand.ResultType.Success &&
                        cast.AiAction?.name == prefix + suffix + "Ai" && cast.Type == UnitCommand.CommandType.Standard &&
                        economy.Count(cast) == 1 && economy.Exact(cast, turnBased) && !cast.Cutscene && !cast.IsIgnoreCooldown &&
                        !cast.Spell.RequireFullRoundAction && cast.Spell.Spellbook == null &&
                        ReferenceEquals(cast.Spell.Fact, data.Fact) && (turnBased ? ticks == 0 && nativeTurns > 0 : ticks > 0) &&
                        affected && effects.Length > 0 && effects.All(value => value.Context.Params.CasterLevel == 8 &&
                            value.Context.Params.DC == 15) && spellSaves.All(value => (int)value["dc"] == 15) &&
                        applications.Length == effects.Length && applications.All(value =>
                            (double?)value["durationSeconds"] == expectedDuration) &&
                        owner.Descriptor.Resources.GetResourceAmount(resource) == 0 &&
                        sibling.Descriptor.Resources.GetResourceAmount(resource) == 1;
                    Sprint25Check(id + "-natural-cast", exact, new JObject {
                        ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                        ["saves"] = saves.Rows, ["buffApplications"] = saves.Applications,
                        ["expectedDurationSeconds"] = expectedDuration,
                        ["aiTicks"] = ticks, ["nativeTurnFrames"] = nativeTurns,
                        ["subjects"] = new JArray(subjects.Select(actor => new JObject {
                            ["unit"] = actor.UniqueId, ["sleeping"] = actor.Descriptor.State.HasCondition(UnitCondition.Sleeping),
                            ["enemyAfter"] = owner.IsEnemy(actor), ["eligibilityAfter"] = score(actor) })),
                        ["effects"] = new JArray(effects.Select(value => new JObject {
                            ["guid"] = value.Blueprint.AssetGuid, ["name"] = value.Blueprint.name,
                            ["cl"] = value.Context.Params.CasterLevel, ["dc"] = value.Context.Params.DC,
                            ["end"] = value.EndTime.TotalSeconds,
                            ["components"] = new JArray(value.Blueprint.ComponentsArray.Select(component => component.GetType().FullName)) })),
                        ["allySleeping"] = ally == null ? (bool?)null : ally.Descriptor.State.HasCondition(UnitCondition.Sleeping),
                        ["fixtureCastCommands"] = 0, ["forcedRolls"] = 0,
                        ["fearAfter"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                        ["siblingFear"] = sibling.Descriptor.Resources.GetResourceAmount(resource) },
                        "Natural native Standard/non-full-round SLA at CL8/DC15, real saves/effects and unchanged independent budgets. Sleep uses exactly four HD of the six-HD enemy group and excludes the allied subject.");
                    if (!exact) throw new InvalidOperationException("Satyr at-will natural cast failed.");
                    var controlled = subjects.Where(actor => actor.Buffs.Enumerable.Any(value => effects.Contains(value))).ToArray();
                    bool refused = controlled.All(actor => score(actor) == 0f) && score(sentinel) == 0f;
                    Sprint25Check(id + "-controlled-refusal", refused, new JObject {
                        ["controlled"] = new JArray(controlled.Select(actor => new JObject { ["unit"] = actor.UniqueId,
                            ["eligibility"] = score(actor), ["enemy"] = owner.IsEnemy(actor) })),
                        ["sentinelEligibility"] = score(sentinel), ["originalEffectCount"] = effects.Length },
                        "The live product policy refuses already-controlled subjects and the ineligible sentinel. This is a native eligibility observation, not a second-cast claim.");
                    // Preserve a failed refusal row while still observing this
                    // exact cast's independent source-expiry boundary.
                    // Exact captured references survive loss of MaybeCaster.
                    // No victim removal or fixture disposal supplies the verdict.
                    SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                    game.TurnBasedCombatController.Activate();
                    // A separate native effect-only probe establishes sibling
                    // isolation. The natural cast/save/cost rows above are
                    // already captured; this is not another cast or AI proof.
                    var probeRecipient = sleep ? subjects.First(actor =>
                        !actor.Buffs.Enumerable.Any(value => effects.Contains(value))) : sentinel;
                    if (!sleep) probeRecipient.Descriptor.RemoveFact(charmImmunity);
                    PlaceExpandedSummoningUnit(sibling, owner.Position);
                    sibling.Wake(0f);
                    if (!game.State.AwakeUnits.Contains(sibling)) game.State.AwakeUnits.Add(sibling);
                    sibling.Memory.Add(probeRecipient); probeRecipient.Memory.Add(sibling);
                    var probeDeadline = DateTime.UtcNow.AddSeconds(8); int probeFrames = 0;
                    do
                    {
                        game.IsPaused = false; yield return 0; probeFrames++;
                        sibling.SightCache.ClearIfNeeded(); probeRecipient.SightCache.ClearIfNeeded(); memory.Tick();
                    } while (DateTime.UtcNow < probeDeadline && probeFrames < 480 &&
                        (probeFrames < 8 || !sibling.HasLOS(probeRecipient) || !sibling.Memory.ContainsVisible(probeRecipient)));
                    bool probeEligible = policy.Score(new DecisionContext { Unit = sibling,
                        Target = new TargetInfo { Unit = probeRecipient } }) == 1f &&
                        !probeRecipient.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, effects[0].Blueprint));
                    if (!probeEligible) throw new InvalidOperationException("Separate sibling lifecycle-probe recipient is not eligible.");
                    var cooldown = sibling.CombatState.Cooldown;
                    var beforeProbeCosts = new[] { cooldown.StandardAction, cooldown.MoveAction, cooldown.SwiftAction };
                    var probe = ApplySprint12Buff(probeRecipient, effects[0].Blueprint, sibling,
                        TimeSpan.FromSeconds(expectedDuration), 15).AppliedBuff;
                    if (probe != null) capturedEffects.Add(probe);
                    var afterProbeCosts = new[] { cooldown.StandardAction, cooldown.MoveAction, cooldown.SwiftAction };
                    double probeEnd = probe?.EndTime.TotalSeconds ?? 0;
                    bool probeReady = probe != null && probe.Active && !probe.IsDisposed &&
                        probeRecipient.Buffs.Enumerable.Contains(probe) && ReferenceEquals(probe.Context?.MaybeCaster, sibling) &&
                        probeEnd > game.Player.GameTime.TotalSeconds && beforeProbeCosts.SequenceEqual(afterProbeCosts) &&
                        sibling.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                        sibling.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint,
                            BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)).EndTime > game.Player.GameTime;
                    Sprint25Check(id + "-sibling-effect-probe", probeReady, new JObject {
                        ["source"] = sibling.UniqueId, ["recipient"] = probeRecipient.UniqueId,
                        ["eligibleBeforeProbe"] = probeEligible, ["buff"] = probe?.Blueprint.AssetGuid,
                        ["active"] = probe?.Active, ["exactSource"] = ReferenceEquals(probe?.Context?.MaybeCaster, sibling),
                        ["originalEnd"] = probeEnd, ["costsBefore"] = new JArray(beforeProbeCosts),
                        ["costsAfter"] = new JArray(afterProbeCosts), ["fear"] = sibling.Descriptor.Resources.GetResourceAmount(resource),
                        ["fixtureCharmImmunityRemovedAfterCastProof"] = !sleep,
                        ["probeKind"] = "native RuleApplyBuff only; no cast, AI, save or action-cost evidence" },
                        "A separate eligible owned recipient holds a real exact-sibling native buff without replacement stacking or budget changes.");
                    if (!probeReady) throw new InvalidOperationException("Exact sibling lifecycle probe failed.");
                    var nativeCharmFaction = sleep ? null : effects.Single().SelectComponents<NeutralToPlayer>().Single().Faction;
                    Func<bool> originalConditionsClear = () => controlled.All(actor => sleep ?
                        !actor.Descriptor.State.HasCondition(UnitCondition.Sleeping) : sibling.IsEnemy(actor));
                    Func<JArray> charmFactions = () => sleep ? new JArray() : new JArray(controlled.Select(actor => new JObject {
                        ["unit"] = actor.UniqueId, ["nativeCharmFaction"] = nativeCharmFaction?.name,
                        ["directContainsFaction"] = nativeCharmFaction == null ? (bool?)null : actor.AttackFactions.Contains(nativeCharmFaction),
                        ["groupContainsFaction"] = nativeCharmFaction == null ? (bool?)null : actor.Group.AttackFactions.Contains(nativeCharmFaction),
                        ["directAttackFactions"] = new JArray(actor.AttackFactions.Select(value => value.name)),
                        ["groupAttackFactions"] = new JArray(actor.Group.AttackFactions.Select(value => value.name)),
                        ["enemyOfSibling"] = sibling.IsEnemy(actor) }));
                    var originalEnds = effects.Select(value => value.EndTime.TotalSeconds).ToArray();
                    var expiry = ExpireSprint16OwnedBuff(owner, owner.Buffs.Enumerable.Single(value =>
                        ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)));
                    deadline = DateTime.UtcNow.AddSeconds(18);
                    JArray firstRemovedFactions = null; int framesAfterRemoval = 0;
                    while (DateTime.UtcNow < deadline)
                    {
                        bool removed = Sprints3031SourceAbsent(owner) && effects.All(value => value.IsDisposed || !value.Active);
                        if (removed && firstRemovedFactions == null) firstRemovedFactions = charmFactions();
                        if (removed && originalConditionsClear()) break;
                        // Native group caches tick before entity destruction.
                        // Let normal frames propagate Charm's faction event;
                        // never assign factions or drive an extra group tick.
                        game.IsPaused = false; yield return 0;
                        if (removed) framesAfterRemoval++;
                    }
                    bool conditionsCleared = originalConditionsClear();
                    bool probePreserved = probe.Active && !probe.IsDisposed && probeRecipient.Buffs.Enumerable.Contains(probe) &&
                        ReferenceEquals(probe.Context?.MaybeCaster, sibling) && probe.EndTime.TotalSeconds == probeEnd &&
                        probeEnd > game.Player.GameTime.TotalSeconds && probeRecipient.IsInState &&
                        !probeRecipient.Destroyed && !probeRecipient.Descriptor.State.IsDead;
                    bool clean = Sprints3031SourceAbsent(owner) && effects.All(value => value.IsDisposed || !value.Active) &&
                        conditionsCleared && probePreserved &&
                        subjects.All(actor => actor.IsInState && !actor.Destroyed && !actor.Descriptor.State.IsDead) &&
                        originalEnds.All(value => value > game.Player.GameTime.TotalSeconds) &&
                        sibling.IsInState && !sibling.Destroyed && sibling.Descriptor.Resources.GetResourceAmount(resource) == 1;
                    Sprint25Check(id + "-source-expiry", clean, new JObject {
                        ["expiry"] = expiry, ["sourceAbsent"] = Sprints3031SourceAbsent(owner),
                        ["gameSeconds"] = game.Player.GameTime.TotalSeconds, ["originalEffectEnds"] = new JArray(originalEnds),
                        ["capturedEffects"] = new JArray(effects.Select(value => new JObject {
                            ["disposed"] = value.IsDisposed, ["active"] = value.Active, ["guid"] = value.Blueprint.AssetGuid })),
                        ["siblingAlive"] = sibling.IsInState && !sibling.Destroyed,
                        ["originalConditionsCleared"] = conditionsCleared,
                        ["firstRemovedFactions"] = firstRemovedFactions, ["finalFactions"] = charmFactions(),
                        ["normalFramesAfterRemoval"] = framesAfterRemoval,
                        ["siblingEffectPreserved"] = probePreserved, ["siblingEffectEnd"] = probe.EndTime.TotalSeconds,
                        ["subjectsSurvived"] = subjects.All(actor => actor.IsInState && !actor.Destroyed && !actor.Descriptor.State.IsDead),
                        ["fixtureVictimRemovalsBeforeVerdict"] = 0 },
                        "Native summon expiry removes the source's captured spell effects and conditions before their deadlines while preserving the sibling's separately probed exact effect.");
                    // A failed row remains FAIL. Finalizers can safely remove
                    // this isolated case and permit the other at-will case.
                }
            }
            finally
            {
                if (saves != null) EventBus.Unsubscribe(saves);
                if (starts != null) EventBus.Unsubscribe(starts);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; game.TurnBasedCombatController.Activate();
                foreach (var actor in extra.Concat(new[] { owner, sibling, target }).Where(value => value != null && !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(actor, false); actor.Commands.InterruptAll(true);
                    if (actor.IsInCombat) actor.LeaveCombat();
                }
                game.Player.UpdateIsInCombat();
                foreach (var buff in capturedEffects.Where(value => !value.IsDisposed)) buff.Remove();
                foreach (var actor in subjects.Where(value => value != null && !value.Destroyed))
                    foreach (var buff in actor.Buffs.Enumerable.Where(value =>
                        ReferenceEquals(value.Context?.MaybeCaster, owner)).ToArray()) actor.Buffs.RemoveFact(buff);
                if (charmImmunity != null)
                    foreach (var actor in immune.Where(value => value != null && !value.Destroyed)) actor.Descriptor.RemoveFact(charmImmunity);
                DisposeExpandedSummoningUnits(fixture.Created, extra.Concat(new[] { owner, sibling })
                    .Where(value => value != null && !value.Destroyed).ToArray());
                if (allyBlueprint != null) UnityEngine.Object.DestroyImmediate(allyBlueprint);
                if (charmImmunity != null)
                {
                    foreach (var component in charmImmunity.ComponentsArray) UnityEngine.Object.DestroyImmediate(component);
                    UnityEngine.Object.DestroyImmediate(charmImmunity);
                }
                target.Stats.SaveWill.BaseValue = oldWill; target.Stats.AC.BaseValue = oldAc;
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                game.TurnBasedCombatController.Activate(); game.IsPaused = oldPause;
            }
        }
        // Same native expiry/effect-only sibling probe used by the at-will
        // control cases. No supplied spell, save or action proves a cast here.
        private IEnumerable<int> ReviewFeyControlExpiry32(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData sibling, UnitEntityData target, Buff original,
            AbilityData siblingData, UnitCondition condition, Func<JObject> siblingBudget, string id, Func<JObject> sourceAttribution = null,
            Action<UnitEntityData> prepareProbe = null)
        {
            var game = Game.Instance;
            UnitEntityData probeTarget = null; Buff probe = null;
            try
            {
                if (original == null || !original.Active || original.IsDisposed ||
                    !ReferenceEquals(original.Context?.MaybeCaster, owner) ||
                    !target.Descriptor.State.HasCondition(condition) || !fixture.Created.Contains(owner) ||
                    !fixture.Created.Contains(sibling))
                    throw new InvalidOperationException("Exact original control/source is not available for expiry proof.");
                double originalEnd = original.EndTime.TotalSeconds;
                var budget = siblingBudget();
                var cooldown = sibling.CombatState.Cooldown;
                var costsBefore = new[] { cooldown.StandardAction, cooldown.MoveAction, cooldown.SwiftAction };
                Sprint28SetBrainActivePreservingActions(owner, false);
                Sprint28SetBrainActivePreservingActions(sibling, false);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                game.TurnBasedCombatController.Activate(); game.IsPaused = false;
                // This is an effect-only source-isolation probe. Reuse the
                // sibling's surveyed point; no second cast, LOS, acquisition,
                // approach or action claim is made from the supplied effect.
                var vacated = sibling.Position;
                var nearest = AstarPath.active.GetNearest(vacated);
                if (nearest.node == null || !nearest.node.Walkable ||
                    Vector3.Distance(nearest.clampedPosition, vacated) > .3f ||
                    Vector3.Distance(nearest.clampedPosition, target.Position) < .8f)
                    throw new InvalidOperationException("Vacated sibling point is not a separate walkable control-probe position.");
                PlaceExpandedSummoningUnit(sibling, owner.Position);
                probeTarget = game.EntityCreator.SpawnUnit(fixture.HostileBlueprint,
                    nearest.clampedPosition, Quaternion.identity, fixture.Scene);
                fixture.Created.Add(probeTarget); Sprint28SetBrainActivePreservingActions(probeTarget, false);
                prepareProbe?.Invoke(probeTarget);
                probeTarget.Stats.HitPoints.BaseValue = 100000;
                foreach (var actor in new[] { sibling, probeTarget })
                { actor.Wake(0f); if (!game.State.AwakeUnits.Contains(actor)) game.State.AwakeUnits.Add(actor); }
                // SpawnUnit queues native creation; finish that existing seam before checking live state.
                game.EntityCreator.Tick();
                bool eligible = !ReferenceEquals(probeTarget, target) && probeTarget.IsInState &&
                    !probeTarget.Destroyed && !probeTarget.Descriptor.State.IsDead &&
                    sibling.IsEnemy(probeTarget) && siblingData.IsAvailable &&
                    siblingData.CanTarget(new TargetWrapper(probeTarget));
                if (!eligible) throw new InvalidOperationException("Separate native sibling effect probe is not eligible.");
                probe = ApplySprint12Buff(probeTarget, original.Blueprint, sibling,
                    TimeSpan.FromSeconds(48), original.Context.Params.DC).AppliedBuff;
                double probeEnd = probe?.EndTime.TotalSeconds ?? 0;
                bool probeReady = probe != null && probe.Active && !probe.IsDisposed &&
                    ReferenceEquals(probe.Context?.MaybeCaster, sibling) && probeTarget.Buffs.Enumerable.Contains(probe) &&
                    probeTarget.Descriptor.State.HasCondition(condition) && JToken.DeepEquals(budget, siblingBudget()) &&
                    costsBefore.SequenceEqual(new[] { cooldown.StandardAction, cooldown.MoveAction, cooldown.SwiftAction });
                if (!probeReady) throw new InvalidOperationException("Separate exact-sibling native effect probe failed.");
                bool originalStillLive = original.Active && !original.IsDisposed &&
                    target.Buffs.Enumerable.Contains(original) && ReferenceEquals(original.Context?.MaybeCaster, owner) &&
                    original.EndTime.TotalSeconds == originalEnd && originalEnd > game.Player.GameTime.TotalSeconds &&
                    target.Descriptor.State.HasCondition(condition);
                if (!originalStillLive)
                    throw new InvalidOperationException("Original control ended during sibling setup; source expiry is unverified.");
                var expiry = ExpireSprint16OwnedBuff(owner, owner.Buffs.Enumerable.Single(value =>
                    ReferenceEquals(value.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)));
                var limit = DateTime.UtcNow.AddSeconds(18);
                while (DateTime.UtcNow < limit && (!Sprints3031SourceAbsent(owner) || original.Active && !original.IsDisposed ||
                    target.Descriptor.State.HasCondition(condition)))
                { game.IsPaused = false; yield return 0; }
                bool isolated = probe.Active && !probe.IsDisposed && probeTarget.Buffs.Enumerable.Contains(probe) &&
                    ReferenceEquals(probe.Context?.MaybeCaster, sibling) && probe.EndTime.TotalSeconds == probeEnd &&
                    probeEnd > game.Player.GameTime.TotalSeconds && probeTarget.Descriptor.State.HasCondition(condition) &&
                    sibling.IsInState && !sibling.Destroyed && JToken.DeepEquals(budget, siblingBudget());
                var attribution = sourceAttribution?.Invoke();
                bool clean = (attribution == null || (bool)attribution["passed"]) &&
                    Sprints3031SourceAbsent(owner) && (!original.Active || original.IsDisposed) &&
                    !target.Descriptor.State.HasCondition(condition) && originalEnd > game.Player.GameTime.TotalSeconds &&
                    target.IsInState && !target.Destroyed && !target.Descriptor.State.IsDead && isolated;
                Sprint25Check(id + "-source-expiry", clean, new JObject {
                    ["source"] = owner.UniqueId, ["sourceAbsent"] = Sprints3031SourceAbsent(owner), ["expiry"] = expiry,
                    ["originalBuff"] = original.Blueprint.AssetGuid, ["originalEnd"] = originalEnd,
                    ["gameSeconds"] = game.Player.GameTime.TotalSeconds, ["originalActive"] = original.Active,
                    ["originalDisposed"] = original.IsDisposed, ["conditionCleared"] = !target.Descriptor.State.HasCondition(condition),
                    ["sibling"] = sibling.UniqueId, ["probeTarget"] = probeTarget.UniqueId,
                    ["probeHd"] = probeTarget.Descriptor.Progression.CharacterLevel,
                    ["probePointKind"] = "vacated surveyed sibling point; no LOS or casting claim",
                    ["probeEligible"] = eligible, ["siblingEffectPreserved"] = isolated, ["probeEnd"] = probeEnd,
                    ["sourceRemovalAttribution"] = attribution,
                    ["siblingBudgetBefore"] = budget, ["siblingBudgetAfter"] = siblingBudget(),
                    ["probeKind"] = "native RuleApplyBuff effect-only; not a second spell/save/action proof",
                    ["fixtureVictimRemovalsBeforeVerdict"] = 0 },
                    "Native source expiry removes its captured control before the original deadline and preserves a separately applied exact-sibling effect and budgets.");
            }
            finally
            {
                if (original != null && !original.IsDisposed) original.Remove();
                if (probe != null && !probe.IsDisposed) probe.Remove();
                if (probeTarget != null && !probeTarget.Destroyed)
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { probeTarget });
            }
        }
    }
}
