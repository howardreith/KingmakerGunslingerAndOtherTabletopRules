using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Components;
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
        private IEnumerable<int> ReviewAstral31Support(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            UnitEntityData target = fixture.Hostile;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;

                owner = CastExpandedSummoningOwnTier(fixture, "astral-deva");
                sibling = CastExpandedSummoningOwnTier(fixture, "astral-deva");
                foreach (UnitEntityData unit in new[] { owner, sibling })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                }

                var cure = (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                    .BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "CureLightWounds"];
                var cureResource = (BlueprintAbilityResource)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "CureLightWoundsResource"];
                var see = (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                    .BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "SeeInvisibility"];
                var seeResource = (BlueprintAbilityResource)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "SeeInvisibilityResource"];
                var healResource = (BlueprintAbilityResource)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "HealResource"];
                var bladeResource = (BlueprintAbilityResource)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        "BladeBarrierResource"];
                var nativeCure = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    BlueprintBootstrap.Library, "5590652e1c2225c4ca30c4a699ab3649",
                    "native Cure Light Wounds spell");
                var nativeSee = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    BlueprintBootstrap.Library, "30e5dc243f937fc4b95d2f8f4e1b7ff3",
                    "native See Invisibility spell");
                var cureGranted = owner.Descriptor.Abilities.GetAbility(cure);
                var seeGranted = owner.Descriptor.Abilities.GetAbility(see);
                var cureData = cureGranted == null ? null : new AbilityData(cureGranted);
                var seeData = seeGranted == null ? null : new AbilityData(seeGranted);
                var cureParameters = cureData?.CalculateParams();
                var seeParameters = seeData?.CalculateParams();
                var cureCost = cure.GetComponent<AbilityResourceLogic>();
                var seeCost = see.GetComponent<AbilityResourceLogic>();
                Func<BlueprintAbility, BlueprintAbility, bool> nativeGraph = (ability, native) =>
                    (ability.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                        .Where(value => !(value is ContextCalculateAbilityParams) &&
                            !(value is AbilityResourceLogic)).Select(value =>
                                value.GetType()).SequenceEqual((native.ComponentsArray ??
                                    Array.Empty<BlueprintComponent>()).Where(value =>
                                        !(value is SpellListComponent)).Select(value =>
                                            value.GetType()));
                var supportAi = owner.Blueprint.Brain.Actions.OfType<BlueprintAiCastSpell>()
                    .Where(value => ReferenceEquals(value.Ability, cure) ||
                        ReferenceEquals(value.Ability, see)).ToArray();
                bool nativeGraphs = nativeGraph(cure, nativeCure) &&
                    nativeGraph(see, nativeSee);
                bool abilityShape = cureData != null && seeData != null &&
                    cure.Type == AbilityType.SpellLike &&
                    see.Type == AbilityType.SpellLike &&
                    cure.ActionType == UnitCommand.CommandType.Standard &&
                    see.ActionType == UnitCommand.CommandType.Standard &&
                    cure.ActionType == nativeCure.ActionType &&
                    see.ActionType == nativeSee.ActionType &&
                    cure.Range == nativeCure.Range && see.Range == nativeSee.Range &&
                    cure.CanTargetSelf == nativeCure.CanTargetSelf &&
                    cure.CanTargetFriends == nativeCure.CanTargetFriends &&
                    see.CanTargetSelf == nativeSee.CanTargetSelf &&
                    ReferenceEquals(cure.Icon, nativeCure.Icon) &&
                    ReferenceEquals(see.Icon, nativeSee.Icon);
                bool resourceShape = cureCost != null && ReferenceEquals(cureCost.RequiredResource,
                        cureResource) && cureCost.IsSpendResource && cureCost.Amount == 1 &&
                    seeCost != null && ReferenceEquals(seeCost.RequiredResource,
                        seeResource) && seeCost.IsSpendResource && seeCost.Amount == 1;
                bool parameterShape = cureParameters != null && cureParameters.CasterLevel == 13 &&
                    cureParameters.SpellLevel == 1 && seeParameters != null &&
                    seeParameters.CasterLevel == 13 && seeParameters.SpellLevel == 2;
                bool autonomyShape = supportAi.Count(value =>
                        ReferenceEquals(value.Ability, cure)) == 1 &&
                    supportAi.Count(value => ReferenceEquals(value.Ability, see)) == 0 &&
                    owner.Descriptor.Resources.GetResourceAmount(cureResource) == 7 &&
                    owner.Descriptor.Resources.GetResourceAmount(seeResource) == 7 &&
                    sibling.Descriptor.Resources.GetResourceAmount(cureResource) == 7 &&
                    sibling.Descriptor.Resources.GetResourceAmount(seeResource) == 7;
                bool mapping = abilityShape && nativeGraphs && resourceShape &&
                    parameterShape && autonomyShape;
                Sprint25Check("astral-support-native-mapping", mapping, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                    ["cure"] = cure.AssetGuid, ["nativeCure"] = nativeCure.AssetGuid,
                    ["see"] = see.AssetGuid, ["nativeSee"] = nativeSee.AssetGuid,
                    ["cureCl"] = cureParameters?.CasterLevel,
                    ["cureSpellLevel"] = cureParameters?.SpellLevel,
                    ["seeCl"] = seeParameters?.CasterLevel,
                    ["seeSpellLevel"] = seeParameters?.SpellLevel,
                    ["cureUses"] = owner.Descriptor.Resources
                        .GetResourceAmount(cureResource),
                    ["seeUses"] = owner.Descriptor.Resources
                        .GetResourceAmount(seeResource),
                    ["cureAiActions"] = supportAi.Count(value =>
                        ReferenceEquals(value.Ability, cure)),
                    ["seeAiActions"] = supportAi.Count(value =>
                        ReferenceEquals(value.Ability, see)),
                    ["abilityShape"] = abilityShape,
                    ["nativeGraphs"] = nativeGraphs,
                    ["resourceShape"] = resourceShape,
                    ["parameterShape"] = parameterShape,
                    ["autonomyShape"] = autonomyShape,
                    ["cureComponents"] = new JArray((cure.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Select(value =>
                            value.GetType().Name)),
                    ["nativeCureComponents"] = new JArray((nativeCure.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Select(value =>
                            value.GetType().Name)),
                    ["seeComponents"] = new JArray((see.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Select(value =>
                            value.GetType().Name)),
                    ["nativeSeeComponents"] = new JArray((nativeSee.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Select(value =>
                            value.GetType().Name)),
                    ["manualSeeInvisibility"] = true },
                    "The two private Astral SLAs preserve the installed native effect graphs at CL13, own separate seven-use resources, and expose AI only for bounded healing.");
                if (!mapping) throw new InvalidOperationException(
                    "Astral support native mapping/resource boundary failed.");

                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target,
                    8f, out far, out lane, out direction,
                    new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(sibling, origin + Vector3.right * 2f);
                PlaceExpandedSummoningUnit(target, far);
                foreach (UnitEntityData unit in new[] { owner, sibling, target })
                {
                    unit.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                sibling.Memory.Add(target); target.Memory.Add(sibling);
                owner.SightCache.ClearIfNeeded(); sibling.SightCache.ClearIfNeeded();
                target.SightCache.ClearIfNeeded();
                owner.JoinCombat(); sibling.JoinCombat(); target.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                for (int frame = 0; frame < 4; frame++) yield return 0;

                sibling.Damage = 20;
                int manualDamageBefore = sibling.Damage;
                int cureOwnerBefore = owner.Descriptor.Resources
                    .GetResourceAmount(cureResource);
                int cureSiblingBefore = sibling.Descriptor.Resources
                    .GetResourceAmount(cureResource);
                UnitUseAbility cureCommand = null;
                UnitUseAbility heldCommand = null;
                var heldEvidence = new List<string>();
                var cureCosts = new JArray();
                using (var economy = new FirearmReviewCosts(owner, cureCosts))
                {
                    var queuedBefore = owner.Commands.Queue.ToArray();
                    foreach (int frame in Sprint26Cast(owner, cure,
                        new TargetWrapper(sibling), value => cureCommand = value))
                        yield return frame;
                    var sticky = cure.GetComponent<AbilityEffectStickyTouch>();
                    var touch = owner.Get<UnitPartTouch>();
                    var heldCandidates = owner.Commands.Queue.Except(queuedBefore)
                        .OfType<UnitUseAbility>().Where(value => sticky != null &&
                            ReferenceEquals(value.Spell.Blueprint,
                                sticky.TouchDeliveryAbility)).ToArray();
                    bool heldFromQueue = heldCandidates.Length == 1;
                    var heldData = touch?.Ability?.Data;
                    heldCommand = heldFromQueue ? heldCandidates[0] :
                        (heldData == null ? null : new UnitUseAbility(heldData,
                            new TargetWrapper(sibling)));
                    string heldAbility = touch?.Ability?.Data?.Blueprint?.AssetGuid;
                    string stickySource = touch?.Ability?.Data?.StickyTouch?.Blueprint
                        ?.AssetGuid;
                    owner.Commands.RemoveFinishedAndUpdateQueue();
                    bool heldReady = sticky != null && touch != null &&
                        heldCandidates.Length <= 1 && heldCommand != null &&
                        (heldCommand.IsStarted || heldCommand.CanStart) &&
                        ReferenceEquals(heldCommand.Target?.Unit, sibling) &&
                        ReferenceEquals(heldData?.Blueprint,
                            sticky.TouchDeliveryAbility) &&
                        ReferenceEquals(heldData?.StickyTouch?.Blueprint, cure) &&
                        heldData.CanTarget(new TargetWrapper(sibling));
                    if (heldReady)
                    {
                        if (!heldFromQueue) owner.Commands.Run(heldCommand);
                        DateTime heldDeadline = DateTime.UtcNow.AddSeconds(25);
                        int heldFrames = 0;
                        while (heldFrames++ < 3000 &&
                            DateTime.UtcNow < heldDeadline &&
                            (!heldCommand.IsFinished ||
                             heldCommand.ExecutionProcess == null ||
                             !heldCommand.ExecutionProcess.IsEnded))
                        {
                            if (heldCommand.ExecutionProcess != null &&
                                !heldCommand.ExecutionProcess.IsEnded)
                                heldCommand.ExecutionProcess.Tick();
                            yield return 0;
                        }
                        heldEvidence.Add("frames=" + heldFrames +
                            ";fromQueue=" + heldFromQueue +
                            ";started=" + heldCommand.IsStarted +
                            ";finished=" + heldCommand.IsFinished +
                            ";result=" + heldCommand.Result);
                        owner.Commands.RemoveFinishedAndUpdateQueue();
                    }
                    int healed = manualDamageBefore - sibling.Damage;
                    bool exact = cureCommand != null && cureCommand.IsFinished &&
                        cureCommand.Result == UnitCommand.ResultType.Success &&
                        cureCommand.ExecutionProcess != null &&
                        cureCommand.ExecutionProcess.IsEnded && cureCommand.AiAction == null &&
                        !cureCommand.Cutscene && !cureCommand.IsIgnoreCooldown &&
                        economy.Count(cureCommand) == 1 &&
                        economy.Exact(cureCommand, false) && heldReady &&
                        heldCommand.IsFinished &&
                        heldCommand.Result == UnitCommand.ResultType.Success &&
                        heldCommand.ExecutionProcess != null &&
                        heldCommand.ExecutionProcess.IsEnded &&
                        economy.Count(heldCommand) == 1 &&
                        economy.Unchanged(heldCommand) &&
                        owner.Get<UnitPartTouch>() == null &&
                        healed >= 6 && healed <= 13 &&
                        cureOwnerBefore == 7 && cureSiblingBefore == 7 &&
                        owner.Descriptor.Resources.GetResourceAmount(cureResource) == 6 &&
                        sibling.Descriptor.Resources.GetResourceAmount(cureResource) == 7 &&
                        owner.Descriptor.Resources.GetResourceAmount(seeResource) == 7 &&
                        owner.Descriptor.Resources.GetResourceAmount(healResource) == 1 &&
                        owner.Descriptor.Resources.GetResourceAmount(bladeResource) == 1;
                    Sprint25Check("astral-support-cure-manual", exact,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["target"] = sibling.UniqueId,
                            ["lane"] = lane,
                            ["rootCommand"] = cureCommand == null ? null :
                                Sprint27AiCommand(cureCommand),
                            ["heldCommand"] = heldCommand == null ? null :
                                Sprint27AiCommand(heldCommand),
                            ["heldAbility"] = heldAbility,
                            ["stickySource"] = stickySource,
                            ["heldCandidateCount"] = heldCandidates.Length,
                            ["heldFromQueue"] = heldFromQueue,
                            ["heldReady"] = heldReady,
                            ["heldCostApplications"] = heldCommand == null ? 0 :
                                economy.Count(heldCommand),
                            ["heldCostUnchanged"] = heldCommand != null &&
                                economy.Unchanged(heldCommand),
                            ["heldCutscene"] = heldCommand?.Cutscene,
                            ["heldIgnoreCooldown"] = heldCommand?.IsIgnoreCooldown,
                            ["touchAfterDelivery"] = owner.Get<UnitPartTouch>() != null,
                            ["heldEvidence"] = new JArray(heldEvidence),
                            ["costs"] = cureCosts,
                            ["damage"] = manualDamageBefore + "->" + sibling.Damage,
                            ["healed"] = healed,
                            ["ownerCure"] = cureOwnerBefore + "->" + owner.Descriptor
                                .Resources.GetResourceAmount(cureResource),
                            ["siblingCure"] = cureSiblingBefore + "->" + sibling.Descriptor
                                .Resources.GetResourceAmount(cureResource),
                            ["ownerSee"] = owner.Descriptor.Resources
                                .GetResourceAmount(seeResource),
                            ["ownerHeal"] = owner.Descriptor.Resources
                                .GetResourceAmount(healResource),
                            ["ownerBlade"] = owner.Descriptor.Resources
                                .GetResourceAmount(bladeResource),
                            ["manualNativeRootAndHeldTouch"] = true,
                            ["forcedHealingRolls"] = 0 },
                        "A real manual CL13 Cure Light Wounds root plus its exact native held-touch AbilityData delivery pays one standard action, resolves native 1d8+5 healing, and spends only the caster's Cure resource.");
                    if (!exact) throw new InvalidOperationException(
                        "Astral manual Cure Light Wounds path failed.");
                }

                owner.Commands.RemoveFinishedAndUpdateQueue();
                var buffsBefore = owner.Buffs.Enumerable.ToArray();
                int seeWaitFrames = 0;
                seeData = new AbilityData(owner.Descriptor.Abilities.GetAbility(see));
                DateTime seeReadyDeadline = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < seeReadyDeadline && seeWaitFrames++ < 1500 &&
                    !seeData.IsAvailable) yield return 0;
                UnitUseAbility seeCommand = null;
                var seeCosts = new JArray();
                using (var economy = new FirearmReviewCosts(owner, seeCosts))
                {
                    foreach (int frame in Sprint26Cast(owner, see,
                        new TargetWrapper(owner), value => seeCommand = value))
                        yield return frame;
                    var added = owner.Buffs.Enumerable.Where(value =>
                        !buffsBefore.Contains(value) &&
                        ReferenceEquals(value.Context?.MaybeCaster, owner) &&
                        ReferenceEquals(value.Context?.SourceAbility, see)).ToArray();
                    var buff = added.SingleOrDefault();
                    bool exact = seeCommand != null && seeCommand.IsFinished &&
                        seeCommand.Result == UnitCommand.ResultType.Success &&
                        seeCommand.ExecutionProcess != null &&
                        seeCommand.ExecutionProcess.IsEnded && seeCommand.AiAction == null &&
                        !seeCommand.Cutscene && !seeCommand.IsIgnoreCooldown &&
                        economy.Count(seeCommand) == 1 && economy.Exact(seeCommand, false) &&
                        owner.Descriptor.IsSeeInvisibility && buff != null &&
                        buff.Context.Params.CasterLevel == 13 &&
                        buff.TimeLeft.TotalSeconds > 7700d &&
                        buff.TimeLeft.TotalSeconds <= 7800.1d &&
                        owner.Descriptor.Resources.GetResourceAmount(seeResource) == 6 &&
                        owner.Descriptor.Resources.GetResourceAmount(cureResource) == 6 &&
                        sibling.Descriptor.Resources.GetResourceAmount(seeResource) == 7;
                    Sprint25Check("astral-support-see-manual", exact,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["command"] = seeCommand == null ? null :
                                Sprint27AiCommand(seeCommand),
                            ["costs"] = seeCosts,
                            ["readyWaitFrames"] = seeWaitFrames,
                            ["seeInvisibility"] = owner.Descriptor.IsSeeInvisibility,
                            ["buff"] = buff?.Blueprint?.AssetGuid,
                            ["sourceAbility"] = buff?.Context?.SourceAbility?.AssetGuid,
                            ["cl"] = buff?.Context?.Params?.CasterLevel,
                            ["durationSeconds"] = buff?.TimeLeft.TotalSeconds,
                            ["ownerSee"] = owner.Descriptor.Resources
                                .GetResourceAmount(seeResource),
                            ["ownerCure"] = owner.Descriptor.Resources
                                .GetResourceAmount(cureResource),
                            ["siblingSee"] = sibling.Descriptor.Resources
                                .GetResourceAmount(seeResource),
                            ["manualNativeCommand"] = true },
                        "A real manual CL13 See Invisibility command pays one standard action, applies the attributed native long-duration state, and spends only its own resource.");
                    if (!exact) throw new InvalidOperationException(
                        "Astral manual See Invisibility path failed.");
                }

                sibling.Damage = 0;
                owner.Damage = 20;
                owner.Commands.RemoveFinishedAndUpdateQueue();
                int aiReadyFrames = 0;
                cureData = new AbilityData(owner.Descriptor.Abilities.GetAbility(cure));
                DateTime aiReadyDeadline = DateTime.UtcNow.AddSeconds(12);
                while (DateTime.UtcNow < aiReadyDeadline && aiReadyFrames++ < 1500 &&
                    !cureData.IsAvailable) yield return 0;
                var eligibility = fixture.Blueprints.OfType<AstralHealingEligibility>()
                    .Where(value => ReferenceEquals(value.OwningBlueprint,
                        owner.Blueprint)).ToArray();
                var cureEligibility = eligibility.Single(value =>
                    ReferenceEquals(value.Heal, cure));
                var healEligibility = eligibility.Single(value =>
                    value.Heal != null && value.Heal.name ==
                        "KMG_Summoning_OutsiderCelestial_AstralDeva_Heal");
                var context = new Kingmaker.Controllers.Brain.DecisionContext {
                    Unit = owner,
                    Target = new Kingmaker.Controllers.Brain.TargetInfo { Unit = owner } };
                float cureScore = cureEligibility.Score(context);
                float healScore = healEligibility.Score(context);
                bool bounded = cureData.IsAvailable && cureScore == 1f &&
                    healScore == 0f && cureEligibility.MinimumDamage == 5 &&
                    healEligibility.MinimumDamage == 50;
                Sprint25Check("astral-support-cure-ai-boundary", bounded,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["damage"] = owner.Damage,
                        ["cureScore"] = cureScore,
                        ["healScore"] = healScore,
                        ["cureMinimumDamage"] = cureEligibility.MinimumDamage,
                        ["healMinimumDamage"] = healEligibility.MinimumDamage,
                        ["readyWaitFrames"] = aiReadyFrames,
                        ["cureAvailable"] = cureData.IsAvailable },
                    "At twenty damage the bounded native decision admits Cure and suppresses Heal; healthy allies and the higher-value one-use resource remain protected.");
                if (!bounded) throw new InvalidOperationException(
                    "Astral Cure/Heal AI priority boundary failed.");

                Sprint28SetBrainActivePreservingActions(owner, true);
                owner.Brain.RestoreAvailableActions();
                var commands = new HashSet<UnitCommand>();
                var ambient = new HashSet<string>();
                var aiCosts = new JArray();
                int aiDamageBefore = owner.Damage;
                using (var economy = new FirearmReviewCosts(owner, aiCosts))
                {
                    DateTime deadline = DateTime.UtcNow.AddSeconds(30);
                    int frames = 0, ticks = 0;
                    while (DateTime.UtcNow < deadline && frames++ < 3000)
                    {
                        if (frames == 1 || frames % 60 == 0 &&
                            !owner.Commands.Raw.Any(value => value != null &&
                                !value.IsFinished))
                        {
                            foreach (string candidate in Sprint24TickExactAi(owner))
                                if (candidate != owner.UniqueId) ambient.Add(candidate);
                            ticks++;
                        }
                        foreach (UnitCommand command in owner.Commands.Raw.Where(value =>
                            value != null)) commands.Add(command);
                        if (commands.OfType<UnitUseAbility>().Any(value =>
                            value.AiAction?.name ==
                                "KMG_Summoning_OutsiderCelestial_AstralDeva_CureLightWoundsAi" &&
                            value.IsFinished && value.ExecutionProcess != null &&
                            value.ExecutionProcess.IsEnded)) break;
                        Game.Instance.IsPaused = false;
                        yield return 0;
                    }
                    var cast = commands.OfType<UnitUseAbility>().FirstOrDefault(value =>
                        value.AiAction?.name ==
                            "KMG_Summoning_OutsiderCelestial_AstralDeva_CureLightWoundsAi" &&
                        value.IsFinished && value.Result == UnitCommand.ResultType.Success &&
                        value.ExecutionProcess != null &&
                        value.ExecutionProcess.IsEnded);
                    int healed = aiDamageBefore - owner.Damage;
                    bool exact = cast != null && ReferenceEquals(cast.Target?.Unit, owner) &&
                        !cast.Cutscene && !cast.IsIgnoreCooldown &&
                        economy.Count(cast) == 1 && economy.Exact(cast, false) &&
                        healed >= 6 && healed <= 13 &&
                        owner.Descriptor.Resources.GetResourceAmount(cureResource) == 5 &&
                        owner.Descriptor.Resources.GetResourceAmount(seeResource) == 6 &&
                        owner.Descriptor.Resources.GetResourceAmount(healResource) == 1 &&
                        owner.Descriptor.Resources.GetResourceAmount(bladeResource) == 1 &&
                        sibling.Descriptor.Resources.GetResourceAmount(cureResource) == 7 &&
                        sibling.Descriptor.Resources.GetResourceAmount(seeResource) == 7;
                    Sprint25Check("astral-support-cure-ai", exact,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["sibling"] = sibling.UniqueId,
                            ["hostile"] = target.UniqueId,
                            ["lane"] = lane, ["damage"] = aiDamageBefore + "->" +
                                owner.Damage, ["healed"] = healed,
                            ["aiTicks"] = ticks,
                            ["commands"] = new JArray(commands.Select(
                                Sprint27AiCommand)),
                            ["costs"] = aiCosts,
                            ["ownerCure"] = owner.Descriptor.Resources
                                .GetResourceAmount(cureResource),
                            ["ownerSee"] = owner.Descriptor.Resources
                                .GetResourceAmount(seeResource),
                            ["ownerHeal"] = owner.Descriptor.Resources
                                .GetResourceAmount(healResource),
                            ["ownerBlade"] = owner.Descriptor.Resources
                                .GetResourceAmount(bladeResource),
                            ["siblingCure"] = sibling.Descriptor.Resources
                                .GetResourceAmount(cureResource),
                            ["siblingSee"] = sibling.Descriptor.Resources
                                .GetResourceAmount(seeResource),
                            ["ambientCandidatesNeverTicked"] = new JArray(ambient),
                            ["fixtureCastCommands"] = 0,
                            ["forcedHealingRolls"] = 0 },
                        "Actual native AI selects low-priority Cure on its moderately injured living owner, pays one standard action, heals, and leaves Heal, Blade Barrier, See Invisibility and the sibling's resources independent.");
                    if (!exact) throw new InvalidOperationException(
                        "Astral native Cure Light Wounds AI/resource path failed.");
                }
                Sprint28SetBrainActivePreservingActions(owner, false);

                var traits = owner.Blueprint.AddFacts.Single(fact => fact.name ==
                    "KMG_Summoning_OutsiderCelestial_AstralDeva_Traits");
                owner.Descriptor.GetFact(traits).Recalculate();
                bool retained = owner.Descriptor.Resources
                        .GetResourceAmount(cureResource) == 5 &&
                    owner.Descriptor.Resources.GetResourceAmount(seeResource) == 6 &&
                    sibling.Descriptor.Resources.GetResourceAmount(cureResource) == 7 &&
                    sibling.Descriptor.Resources.GetResourceAmount(seeResource) == 7 &&
                    owner.Descriptor.IsSeeInvisibility;
                Sprint25Check("astral-support-no-refresh", retained,
                    new JObject { ["ownerCure"] = owner.Descriptor.Resources
                            .GetResourceAmount(cureResource),
                        ["ownerSee"] = owner.Descriptor.Resources
                            .GetResourceAmount(seeResource),
                        ["siblingCure"] = sibling.Descriptor.Resources
                            .GetResourceAmount(cureResource),
                        ["siblingSee"] = sibling.Descriptor.Resources
                            .GetResourceAmount(seeResource),
                        ["seeInvisibility"] = owner.Descriptor.IsSeeInvisibility },
                    "Trait recalculation does not refresh either finite resource or erase the active native See Invisibility state.");
                if (!retained) throw new InvalidOperationException(
                    "Astral support resource retention failed.");

                owner.Commands.InterruptAll(true); sibling.Commands.InterruptAll(true);
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling });
                string hostileReset = ResetExpandedSummoningHostile(fixture);
                int cleanupFrames = 0;
                DateTime cleanupDeadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < cleanupDeadline && cleanupFrames++ < 600 &&
                    Game.Instance.State.AreaEffects.All.Any(value =>
                        value?.Context != null && (ReferenceEquals(
                            value.Context.MaybeCaster, owner) || ReferenceEquals(
                            value.Context.MaybeCaster, sibling)))) yield return 0;
                bool noOwnedAreas = !Game.Instance.State.AreaEffects.All.Any(value =>
                    value?.Context != null && (ReferenceEquals(
                        value.Context.MaybeCaster, owner) || ReferenceEquals(
                        value.Context.MaybeCaster, sibling)));
                bool cleanup = owner.Destroyed && sibling.Destroyed &&
                    !owner.IsInState && !sibling.IsInState && noOwnedAreas &&
                    !owner.Commands.Raw.Any(value => value != null && !value.IsFinished) &&
                    !sibling.Commands.Raw.Any(value => value != null && !value.IsFinished) &&
                    target.Damage == fixture.HostileDamage &&
                    target.Descriptor.State.Size == fixture.HostileSize;
                Sprint25Check("astral-support-owned-cleanup", cleanup,
                    new JObject { ["ownerAbsent"] = !owner.IsInState,
                        ["siblingAbsent"] = !sibling.IsInState,
                        ["ownedAreasAbsent"] = noOwnedAreas,
                        ["cleanupFrames"] = cleanupFrames,
                        ["hostileReset"] = hostileReset,
                        ["hostileDamageRestored"] = target.Damage ==
                            fixture.HostileDamage,
                        ["hostileSizeRestored"] = target.Descriptor.State.Size ==
                            fixture.HostileSize },
                    "The fixed support sequence removes only its two Astrals, commands, auras and support state, then restores the disposable hostile boundary.");
                if (!cleanup) throw new InvalidOperationException(
                    "Astral support owned cleanup failed.");
            }
            finally
            {
                foreach (UnitEntityData unit in new[] { owner, sibling }.Where(
                    value => value != null && !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    if (unit.IsInCombat) unit.LeaveCombat();
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling }.Where(value => value != null &&
                        !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
