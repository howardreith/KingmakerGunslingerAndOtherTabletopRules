using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
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
        private IEnumerable<int> ReviewAstral31Dispel(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData dismissalOwner = null, dismissalTarget = null;
            UnitEntityData firstDispelOwner = null, secondDispelOwner = null;
            UnitEntityData expiryOwner = null;
            UnitEntityData target = null;
            var transient = new List<UnityEngine.Object>();
            var transientBuffs = new List<Buff>();
            var areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
            int targetAc = 0, targetWill = 0, targetHitPoints = 0;
            Alignment targetAlignment = Alignment.TrueNeutral;
            bool targetChanged = false;
            Astral31DispelEvents observed = null;
            Astral31DismissalKillTrace killTrace = null;
            UnitPartSpellResistance fixtureResistance = null;
            int fixtureResistanceId = -1;
            AbilityExecutionContext fixtureResistanceContext = null;
            JObject fixtureResistanceBefore = null;
            int sharedCl = AbilityParams.Empty.CasterLevel;
            int sharedSpellLevel = AbilityParams.Empty.SpellLevel;
            int sharedDc = AbilityParams.Empty.DC;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;

                CreateExpandedSummoningCorrectionHostile(fixture);
                target = fixture.Hostile;
                targetAc = target.Stats.AC.BaseValue;
                targetWill = target.Stats.SaveWill.BaseValue;
                targetHitPoints = target.Stats.HitPoints.BaseValue;
                targetAlignment = target.Descriptor.Alignment.Value;
                targetChanged = true;

                dismissalOwner = CastExpandedSummoningOwnTier(fixture,
                    "astral-deva");
                dismissalTarget = CastExpandedSummoningOwnTier(fixture,
                    "astral-deva", fixture.Hostile);
                firstDispelOwner = CastExpandedSummoningOwnTier(fixture,
                    "astral-deva");
                secondDispelOwner = CastExpandedSummoningOwnTier(fixture,
                    "astral-deva");
                expiryOwner = CastExpandedSummoningOwnTier(fixture,
                    "astral-deva");
                UnitEntityData[] owned = { dismissalOwner, dismissalTarget,
                    firstDispelOwner, secondDispelOwner, expiryOwner };
                foreach (UnitEntityData unit in owned)
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                }

                Vector3 origin = fixture.Caster.Position;
                PlaceExpandedSummoningUnit(dismissalOwner, origin);
                PlaceExpandedSummoningUnit(dismissalTarget,
                    origin + Vector3.forward * 1.5f);
                PlaceExpandedSummoningUnit(firstDispelOwner,
                    origin + Vector3.right * 2f);
                PlaceExpandedSummoningUnit(secondDispelOwner,
                    origin + Vector3.left * 2f);
                PlaceExpandedSummoningUnit(expiryOwner,
                    origin + Vector3.back * 2f);
                PlaceExpandedSummoningUnit(target,
                    origin + Vector3.forward * 1.5f);
                PlaceExpandedSummoningUnit(fixture.Caster,
                    origin + Vector3.back * 14f);
                foreach (UnitEntityData unit in owned.Concat(new[] { target }))
                {
                    unit.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                }
                foreach (UnitEntityData unit in owned)
                {
                    unit.Memory.Add(target); target.Memory.Add(unit);
                    unit.SightCache.ClearIfNeeded();
                    unit.JoinCombat();
                }
                target.SightCache.ClearIfNeeded();
                target.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
                new Kingmaker.Controllers.Combat.UnitCombatPrepareController().Tick();
                for (int frame = 0; frame < 4; frame++) yield return 0;

                string prefix = ExpandedSummoningSpecialBuilder.Astral31Prefix;
                var family = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "DispelEvil"];
                var dismissal = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "DispelEvilDismissal"];
                var dismissalTouch = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "DispelEvilDismissalTouch"];
                var dispel = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "DispelEvilDispel"];
                var dispelTouch = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "DispelEvilDispelTouch"];
                var ward = (BlueprintBuff)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "DispelEvilWard"];
                var native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    BlueprintBootstrap.Library,
                    "95f7cdcec94e293489a85afdf5af1fd7",
                    "native Dismissal effect graph");
                var nativeTouchDonor = BlueprintLibraryLookup.RequireExact<
                    BlueprintAbility>(BlueprintBootstrap.Library,
                        "17451c1327c571641a1345bd31155209",
                        "qualified native held-touch delivery weapon");
                AbilityDeliverTouch nativeTouch = nativeTouchDonor.GetComponents<
                    AbilityDeliverTouch>().SingleOrDefault();
                AbilityEffectRunAction[] nativeRuns = native.GetComponents<
                    AbilityEffectRunAction>().ToArray();
                AbilityEffectRunAction[] dismissalRuns = dismissalTouch.GetComponents<
                    AbilityEffectRunAction>().ToArray();

                Ability root = dismissalOwner.Descriptor.Abilities.GetAbility(family);
                var dismissalData = root == null ? null : new AbilityData(
                    new AbilityData(root), dismissal);
                var dispelData = root == null ? null : new AbilityData(
                    new AbilityData(root), dispel);
                var dismissalParameters = dismissalData?.CalculateParams();
                var dispelParameters = dispelData?.CalculateParams();
                BlueprintAbility[] variants = family.GetComponent<AbilityVariants>()
                    ?.Variants ?? Array.Empty<BlueprintAbility>();
                bool deliveryGraph = dismissalTouch.GetComponents<
                        AbilityDeliverTouch>().Count() == 1 &&
                    nativeTouch?.TouchWeapon != null && ReferenceEquals(
                        dismissalTouch.GetComponent<AbilityDeliverTouch>()
                            .TouchWeapon, nativeTouch.TouchWeapon) &&
                    nativeRuns.Length > 0 && dismissalRuns.Length ==
                        nativeRuns.Length && !dismissalRuns.Any(value =>
                            nativeRuns.Contains(value)) && nativeRuns.GroupBy(value =>
                                value.GetType()).All(group => dismissalRuns.Count(value =>
                                    value.GetType() == group.Key) == group.Count()) &&
                    dismissalTouch.SpellResistance &&
                    dismissalTouch.GetComponent<
                        AstralDispelEvilDismissalTargetChecker>() != null &&
                    dismissalTouch.GetComponent<
                        AstralDispelEvilDeliveryMarker>()?.Ward == ward &&
                    dispelTouch.GetComponents<AbilityDeliverTouch>().Count() == 1 &&
                    dispelTouch.GetComponent<AstralDispelEvilTargetChecker>() != null &&
                    dispelTouch.GetComponent<AstralDispelEvilDeliveryMarker>()
                        ?.Ward == ward;
                var wardAc = ward.GetComponent<
                    ArmorClassBonusAgainstAlignment>();
                var wardLifetime = ward.GetComponent<
                    AstralDispelEvilWardLifetime>();
                var ai = dismissalOwner.Blueprint.Brain.Actions.OfType<
                    BlueprintAiCastSpell>().Where(value => ReferenceEquals(
                        value.Ability, family) || ReferenceEquals(value.Ability,
                            dismissal) || ReferenceEquals(value.Ability, dispel))
                    .ToArray();
                bool mapping = root != null && variants.SequenceEqual(new[] {
                        dismissal, dispel }) && dismissalData != null &&
                    dispelData != null && dismissalData.IsAvailable &&
                    dispelData.IsAvailable && dismissalParameters != null &&
                    dispelParameters != null &&
                    dismissalParameters.CasterLevel == 13 &&
                    dismissalParameters.SpellLevel == 5 &&
                    dismissalParameters.DC == 21 &&
                    dispelParameters.CasterLevel == 13 &&
                    dispelParameters.SpellLevel == 5 &&
                    dispelParameters.DC == 21 && deliveryGraph &&
                    ward.Stacking == StackingType.Replace && wardAc != null &&
                    wardAc.alignment == AlignmentComponent.Evil &&
                    wardAc.Value == 4 && wardAc.Descriptor ==
                        ModifierDescriptor.Deflection && wardLifetime != null &&
                    wardLifetime.Deliveries != null &&
                    wardLifetime.Deliveries.SequenceEqual(new[] {
                        dismissalTouch, dispelTouch }) &&
                    !family.ComponentsArray.OfType<AbilityResourceLogic>().Any() &&
                    !dismissal.ComponentsArray.OfType<AbilityResourceLogic>().Any() &&
                    !dispel.ComponentsArray.OfType<AbilityResourceLogic>().Any() &&
                    ai.Length == 0;
                Sprint25Check("astral-dispel-native-mapping", mapping,
                    new JObject { ["owner"] = dismissalOwner.UniqueId,
                        ["family"] = family.AssetGuid,
                        ["variants"] = new JArray(variants.Select(value =>
                            value.AssetGuid)),
                        ["dismissalTouch"] = dismissalTouch.AssetGuid,
                        ["dispelTouch"] = dispelTouch.AssetGuid,
                        ["nativeDismissal"] = native.AssetGuid,
                        ["nativeDismissalComponents"] = new JArray(
                            native.ComponentsArray.Select(value =>
                                value.GetType().FullName)),
                        ["nativeDismissalRunActions"] = nativeRuns.Length,
                        ["dismissalTouchRunActions"] = dismissalRuns.Length,
                        ["nativeDismissalActions"] = new JArray(
                            AlternateSummoningGraph.Objects<GameAction>(nativeRuns)
                                .Select(value => value.GetType().FullName)),
                        ["dismissalTouchActions"] = new JArray(
                            AlternateSummoningGraph.Objects<GameAction>(dismissalRuns)
                                .Select(value => value.GetType().FullName)),
                        ["nativeTouchWeapon"] = nativeTouch?.TouchWeapon?.AssetGuid,
                        ["dismissalCl"] = dismissalParameters?.CasterLevel,
                        ["dismissalSpellLevel"] = dismissalParameters?.SpellLevel,
                        ["dismissalDc"] = dismissalParameters?.DC,
                        ["dispelCl"] = dispelParameters?.CasterLevel,
                        ["dispelSpellLevel"] = dispelParameters?.SpellLevel,
                        ["dispelDc"] = dispelParameters?.DC,
                        ["wardValue"] = wardAc?.Value,
                        ["wardDescriptor"] = wardAc?.Descriptor.ToString(),
                        ["wardStacking"] = ward.Stacking.ToString(),
                        ["aiActions"] = ai.Length,
                        ["resources"] = 0, ["manualOnly"] = true },
                    "The visible at-will family owns exactly two CL13/level5/live-DC21 variants, deep-clones the installed Dismissal effect graph onto the qualified native melee-touch delivery, and maps its private automatic-dispel touch and finite +4 deflection ward without AI or a resource.");
                if (!mapping) throw new InvalidOperationException(
                    "Astral Dispel Evil native mapping failed.");

                observed = new Astral31DispelEvents();
                EventBus.Subscribe(observed);

                dismissalTarget.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                dismissalTarget.Stats.AC.BaseValue = -100;
                dismissalTarget.Stats.SaveWill.BaseValue = -100;
                observed.Owner = dismissalOwner;
                observed.Targets.Add(dismissalTarget);
                UnitPartSummonedMonster dismissalSummon = dismissalTarget.Get<
                    UnitPartSummonedMonster>();
                bool dismissalNativeKillAllowed = !Kingmaker.Designers.GameHelper
                    .IsAttackingGreenNPC(dismissalOwner, dismissalTarget);
                bool dismissalTargetReady = dismissalSummon != null &&
                    ReferenceEquals(dismissalSummon.Summoner, fixture.Hostile) &&
                    dismissalOwner.IsEnemy(dismissalTarget) &&
                    dismissalTarget.IsEnemy(dismissalOwner) &&
                    dismissalNativeKillAllowed;
                int evilBefore = Astral31DispelAttackAc(dismissalTarget,
                    dismissalOwner);
                int goodBefore = Astral31DispelAttackAc(firstDispelOwner,
                    dismissalOwner);
                UnitUseAbility dismissalParent = null;
                var dismissalCosts = new JArray();
                killTrace = new Astral31DismissalKillTrace(dismissalOwner,
                    dismissalTarget);
                using (var economy = new FirearmReviewCosts(dismissalOwner,
                    dismissalCosts))
                {
                    foreach (int frame in Astral31DispelCast(dismissalOwner,
                        dismissalData, new TargetWrapper(dismissalOwner), value =>
                            dismissalParent = value)) yield return frame;
                    Buff activeWard = dismissalOwner.Buffs.Enumerable.SingleOrDefault(
                        value => ReferenceEquals(value.Blueprint, ward));
                    UnitPartTouch held = dismissalOwner.Get<UnitPartTouch>();
                    int evilAfter = Astral31DispelAttackAc(dismissalTarget,
                        dismissalOwner);
                    int goodAfter = Astral31DispelAttackAc(firstDispelOwner,
                        dismissalOwner);
                    bool armed = dismissalParent != null &&
                        dismissalParent.IsFinished && dismissalParent.Result ==
                            UnitCommand.ResultType.Success &&
                        economy.Count(dismissalParent) == 1 &&
                        economy.Exact(dismissalParent, false) &&
                        activeWard != null && activeWard.IsFromSpell &&
                        ReferenceEquals(activeWard.Context?.MaybeCaster,
                            dismissalOwner) && activeWard.Context.Params.CasterLevel == 13 &&
                        activeWard.TimeLeft.TotalSeconds > 70d &&
                        activeWard.TimeLeft.TotalSeconds <= 78.1d && held != null &&
                        ReferenceEquals(held.Ability?.Data?.Blueprint,
                            dismissalTouch) &&
                        ReferenceEquals(held.Ability?.Data?.StickyTouch?.Blueprint,
                            dismissal) && evilBefore == goodBefore + 4 &&
                        evilAfter == goodAfter + 4 && evilAfter == evilBefore;
                    Sprint25Check("astral-dispel-ward-held-action", armed,
                        new JObject { ["owner"] = dismissalOwner.UniqueId,
                            ["target"] = dismissalTarget.UniqueId,
                            ["parent"] = dismissalParent == null ? null :
                                Sprint27AiCommand(dismissalParent),
                            ["costs"] = dismissalCosts,
                            ["wardSeconds"] = activeWard?.TimeLeft.TotalSeconds,
                            ["wardFromSpell"] = activeWard?.IsFromSpell,
                            ["wardCaster"] = activeWard?.Context?.MaybeCaster?.UniqueId,
                            ["held"] = held?.Ability?.Data?.Blueprint?.AssetGuid,
                            ["stickySource"] = held?.Ability?.Data?.StickyTouch
                                ?.Blueprint?.AssetGuid,
                            ["evilAttackWeapon"] = dismissalTarget.Body.PrimaryHand
                                .MaybeWeapon?.Blueprint.AssetGuid,
                            ["goodAttackWeapon"] = firstDispelOwner.Body.PrimaryHand
                                .MaybeWeapon?.Blueprint.AssetGuid,
                            ["evilAcBefore"] = evilBefore,
                            ["goodAcBefore"] = goodBefore,
                            ["evilAcAfter"] = evilAfter,
                            ["goodAcAfter"] = goodAfter,
                            ["replacementNotStacked"] = evilAfter == evilBefore },
                        "A real standard action applies the attributed 13-round ward and arms exactly the selected native held touch. Native AC rules retain +4 against evil and no bonus against good; Replace prevents the deva's equal aura bonus from becoming +8.");
                    if (!armed) throw new InvalidOperationException(
                        "Astral Dispel Evil ward/held action failed.");

                    float cooldownBeforeTouch = dismissalOwner.CombatState
                        .Cooldown.StandardAction;
                    UnitUseAbility dismissalHeld = null;
                    foreach (int frame in Astral31DispelHeld(dismissalOwner,
                        dismissalTarget, value => dismissalHeld = value))
                        yield return frame;
                    float cooldownAfterTouch = dismissalOwner.CombatState
                        .Cooldown.StandardAction;
                    for (int frame = 0; frame < 60 && dismissalTarget != null &&
                        !dismissalTarget.Destroyed &&
                        !dismissalTarget.ShouldBeDestroyed &&
                        !dismissalTarget.Descriptor.State.IsDead; frame++)
                        yield return 0;
                    killTrace.Dispose();
                    RuleAttackRoll dismissalRoll = observed.Attacks.SingleOrDefault(
                        value => ReferenceEquals(value.Target, dismissalTarget));
                    RuleSpellResistanceCheck dismissalResistance = observed
                        .Resistances.SingleOrDefault(value => ReferenceEquals(
                            value.Target, dismissalTarget));
                    RuleSavingThrow dismissalSave = observed.Saves.SingleOrDefault(
                        value => ReferenceEquals(value.Initiator,
                            dismissalTarget));
                    bool dismissed = dismissalTargetReady &&
                        dismissalHeld != null &&
                        dismissalHeld.IsFinished && dismissalHeld.Result ==
                            UnitCommand.ResultType.Success &&
                        economy.Count(dismissalHeld) == 1 &&
                        economy.Unchanged(dismissalHeld) && dismissalRoll != null &&
                        dismissalRoll.IsHit && !dismissalRoll.AutoHit &&
                        dismissalResistance != null &&
                        dismissalResistance.HasSpellResistance &&
                        dismissalResistance.HasResistanceRoll &&
                        dismissalResistance.SpellResistance == 25 &&
                        !dismissalResistance.IsSpellResisted &&
                        dismissalResistance.Context.Params.CasterLevel == 13 &&
                        dismissalSave != null && dismissalSave.StatType ==
                            StatType.SaveWill && dismissalSave.DifficultyClass == 21 &&
                        !dismissalSave.IsPassed &&
                        (dismissalTarget.Destroyed ||
                            dismissalTarget.ShouldBeDestroyed ||
                            !dismissalTarget.IsInState ||
                            dismissalTarget.Descriptor.State.IsDead) &&
                        killTrace.Restored && killTrace.Calls.Count == 1 &&
                        (bool)killTrace.Calls[0]["nativeGuardBefore"] &&
                        (bool)killTrace.Calls[0]["markedAfter"] &&
                        dismissalOwner.Get<UnitPartTouch>() == null &&
                        !dismissalOwner.Buffs.Enumerable.Any(value =>
                            ReferenceEquals(value.Blueprint, ward));
                    Sprint25Check("astral-dispel-native-dismissal", dismissed,
                        new JObject { ["owner"] = dismissalOwner.UniqueId,
                            ["target"] = dismissalTarget.UniqueId,
                            ["targetSummoner"] = dismissalSummon?.Summoner?.UniqueId,
                            ["expectedSummoner"] = fixture.Hostile.UniqueId,
                            ["ownerConsidersTargetEnemy"] =
                                dismissalOwner.IsEnemy(dismissalTarget),
                            ["targetConsidersOwnerEnemy"] =
                                dismissalTarget.IsEnemy(dismissalOwner),
                            ["ownerCanAttackTarget"] =
                                dismissalOwner.CanAttack(dismissalTarget),
                            ["ownerPlayerFaction"] = dismissalOwner.IsPlayerFaction,
                            ["targetPlayerFaction"] = dismissalTarget.IsPlayerFaction,
                            ["ownerInCombat"] = dismissalOwner.IsInCombat,
                            ["targetInCombat"] = dismissalTarget.IsInCombat,
                            ["nativeKillGuardAllows"] =
                                dismissalNativeKillAllowed,
                            ["nativeKillCalls"] = killTrace.Calls,
                            ["nativeKillTraceRestored"] = killTrace.Restored,
                            ["targetMarkedForDeath"] = dismissalTarget.Descriptor
                                .State.MarkedForDeath,
                            ["targetLifeState"] = dismissalTarget.Descriptor.State
                                .LifeState.ToString(),
                            ["targetDead"] = dismissalTarget.Descriptor.State.IsDead,
                            ["targetDamage"] = dismissalTarget.Damage,
                            ["targetHpLeft"] = dismissalTarget.HPLeft,
                            ["heldCostApplications"] = economy.Count(dismissalHeld),
                            ["heldNativeCostUnchanged"] = economy.Unchanged(
                                dismissalHeld),
                            ["costs"] = new JArray(dismissalCosts.Select(value =>
                                value.DeepClone())),
                            ["command"] = dismissalHeld == null ? null :
                                Sprint27AiCommand(dismissalHeld),
                            ["touchRoll"] = dismissalRoll?.Roll.Value,
                            ["touchHit"] = dismissalRoll?.IsHit,
                            ["autoHit"] = dismissalRoll?.AutoHit,
                            ["sr"] = dismissalResistance?.SpellResistance,
                            ["srRoll"] = dismissalResistance != null &&
                                dismissalResistance.HasResistanceRoll ?
                                (int?)dismissalResistance.Roll.Value : null,
                            ["resisted"] = dismissalResistance?.IsSpellResisted,
                            ["willDc"] = dismissalSave?.DifficultyClass,
                            ["willRoll"] = dismissalSave?.D20.Value,
                            ["willPassed"] = dismissalSave?.IsPassed,
                            ["targetDestroyed"] = dismissalTarget.Destroyed,
                            ["targetShouldBeDestroyed"] =
                                dismissalTarget.ShouldBeDestroyed,
                            ["targetInState"] = dismissalTarget.IsInState,
                            ["standardCooldown"] = cooldownBeforeTouch + "->" +
                                cooldownAfterTouch,
                            ["wardAfterDelivery"] = dismissalOwner.Buffs.Enumerable
                                .Any(value => ReferenceEquals(value.Blueprint, ward)),
                            ["touchAfterDelivery"] = dismissalOwner.Get<
                                UnitPartTouch>() != null },
                        "The held delivery resolves a real melee touch, native SR25 check and DC21 Will save against an evil extraplanar Astral. The exact native kill action sets its death mark and the native life controller resolves death or removal, matching the established Banishment mapping. Its exact ward/touch discharge and one unchanged native cost application prove no second standard-action cost.");
                    if (!dismissed) throw new InvalidOperationException(
                        "Astral native Dismissal discharge failed.");
                }

                PlaceExpandedSummoningUnit(target,
                    origin + Vector3.forward * 1.5f);
                target.Descriptor.Alignment.Set(Alignment.NeutralEvil);
                target.Stats.AC.BaseValue = -100;
                target.Stats.SaveWill.BaseValue = 100;
                target.Stats.HitPoints.BaseValue = 100000;
                observed.Targets.Add(target);
                fixtureResistanceContext = new AbilityExecutionContext(
                    new AbilityData(dispelTouch, firstDispelOwner.Descriptor),
                    new AbilityParams { CasterLevel = 13, SpellLevel = 5,
                        DC = 21 }, new TargetWrapper(target), null);
                fixtureResistanceBefore = Lemure30SrState(target,
                    fixtureResistanceContext);
                fixtureResistance = target.Ensure<UnitPartSpellResistance>();
                fixtureResistanceId = fixtureResistance.AddResistance(1000,
                    null, null);
                Buff evilSpell = Astral31DispelFixtureEffect(target,
                    firstDispelOwner, "evil-spell", "d15e0000000000000000000000000001",
                    "d15e1000000000000000000000000001", 11,
                    SpellSchool.Abjuration, SpellDescriptor.Evil, false,
                    transient, transientBuffs, observed);
                Buff evilEnchantment = Astral31DispelFixtureEffect(target,
                    target, "evil-enchantment",
                    "d15e0000000000000000000000000002",
                    "d15e1000000000000000000000000002", 9,
                    SpellSchool.Enchantment, SpellDescriptor.None, false,
                    transient, transientBuffs, observed);
                Buff goodEnchantment = Astral31DispelFixtureEffect(target,
                    firstDispelOwner, "good-enchantment",
                    "d15e0000000000000000000000000003",
                    "d15e1000000000000000000000000003", 8,
                    SpellSchool.Enchantment, SpellDescriptor.None, false,
                    transient, transientBuffs, observed);
                Buff ordinarySpell = Astral31DispelFixtureEffect(target,
                    firstDispelOwner, "ordinary-spell",
                    "d15e0000000000000000000000000004",
                    "d15e1000000000000000000000000004", 7,
                    SpellSchool.Abjuration, SpellDescriptor.None, false,
                    transient, transientBuffs, observed);
                Buff protectedEvil = Astral31DispelFixtureEffect(target,
                    target, "nondispellable-evil",
                    "d15e0000000000000000000000000005",
                    "d15e1000000000000000000000000005", 15,
                    SpellSchool.Abjuration, SpellDescriptor.Evil, true,
                    transient, transientBuffs, observed);
                Buff[] fixtureEffects = { evilSpell, evilEnchantment,
                    goodEnchantment, ordinarySpell, protectedEvil };
                int[] fixtureLevels = { 11, 9, 8, 7, 15 };
                bool parameterIsolation = fixtureEffects.Select((value, index) =>
                        value.Context.Params.CasterLevel == fixtureLevels[index] &&
                        value.Context.Params.SpellLevel == 4 &&
                        !ReferenceEquals(value.Context.Params, AbilityParams.Empty) &&
                        fixtureEffects.Take(index).All(previous => !ReferenceEquals(
                            previous.Context.Params, value.Context.Params)))
                    .All(value => value) && AbilityParams.Empty.CasterLevel ==
                        sharedCl && AbilityParams.Empty.SpellLevel ==
                        sharedSpellLevel && AbilityParams.Empty.DC == sharedDc;
                bool eligibility = parameterIsolation &&
                    fixtureResistance.GetValue(fixtureResistanceContext) == 1000 &&
                    evilSpell != null && evilEnchantment != null &&
                    goodEnchantment != null && ordinarySpell != null &&
                    protectedEvil != null && evilSpell.IsFromSpell &&
                    evilEnchantment.IsFromSpell && goodEnchantment.IsFromSpell &&
                    ordinarySpell.IsFromSpell && protectedEvil.IsFromSpell &&
                    !evilSpell.IsNotDispelable &&
                    protectedEvil.IsNotDispelable &&
                    AstralDispelEvilEligibility.CanDispel(evilSpell) &&
                    AstralDispelEvilEligibility.CanDispel(evilEnchantment) &&
                    !AstralDispelEvilEligibility.CanDispel(goodEnchantment) &&
                    !AstralDispelEvilEligibility.CanDispel(ordinarySpell) &&
                    !AstralDispelEvilEligibility.CanDispel(protectedEvil);
                Sprint25Check("astral-dispel-eligibility", eligibility,
                    new JObject { ["target"] = target.UniqueId,
                        ["evilSpell"] = Astral31DispelBuffRow(evilSpell),
                        ["evilCasterEnchantment"] =
                            Astral31DispelBuffRow(evilEnchantment),
                        ["goodCasterEnchantment"] =
                            Astral31DispelBuffRow(goodEnchantment),
                        ["ordinarySpell"] = Astral31DispelBuffRow(ordinarySpell),
                        ["nondispellableEvil"] =
                            Astral31DispelBuffRow(protectedEvil),
                        ["nativeFixtureApplications"] =
                            observed.FixtureApplications,
                        ["parameterIsolation"] = parameterIsolation,
                        ["fixtureCasterLevels"] = new JArray(fixtureEffects.Select(
                            value => value.Context.Params.CasterLevel)),
                        ["sharedEmptyUnchanged"] =
                            AbilityParams.Empty.CasterLevel == sharedCl &&
                            AbilityParams.Empty.SpellLevel == sharedSpellLevel &&
                            AbilityParams.Empty.DC == sharedDc,
                        ["fixtureResistanceId"] = fixtureResistanceId,
                        ["fixtureResistance"] = Lemure30SrState(target,
                            fixtureResistanceContext) },
                    "Five request-local real spell buffs prove that only a dispellable evil spell or an enchantment from an evil caster is eligible; good-caster enchantment, unrelated spell and nondispellable evil spell are refused.");
                if (!eligibility) throw new InvalidOperationException(
                    "Astral Dispel Evil eligibility fixture failed.");

                int savesBeforeDispel = observed.Saves.Count;
                int resistanceBeforeDispel = observed.Resistances.Count;
                int dispelsBefore = observed.Dispels.Count;
                foreach (var row in new[] {
                    new { Owner = firstDispelOwner, Expected = evilSpell,
                        Preserved = evilEnchantment, Label = "evil-spell" },
                    new { Owner = secondDispelOwner, Expected = evilEnchantment,
                        Preserved = (Buff)null, Label = "evil-enchantment" } })
                {
                    observed.Owner = row.Owner;
                    observed.Targets.Add(target);
                    Ability ownerRoot = row.Owner.Descriptor.Abilities
                        .GetAbility(family);
                    var ownerDispel = new AbilityData(new AbilityData(ownerRoot),
                        dispel);
                    UnitUseAbility parentCommand = null;
                    foreach (int frame in Astral31DispelCast(row.Owner,
                        ownerDispel, new TargetWrapper(row.Owner), value =>
                            parentCommand = value)) yield return frame;
                    Buff activeWard = row.Owner.Buffs.Enumerable.SingleOrDefault(
                        value => ReferenceEquals(value.Blueprint, ward));
                    UnitUseAbility touchCommand = null;
                    foreach (int frame in Astral31DispelHeld(row.Owner, target,
                        value => touchCommand = value)) yield return frame;
                    RuleDispelMagic rule = observed.Dispels.LastOrDefault(value =>
                        ReferenceEquals(value.Buff, row.Expected));
                    bool removed = parentCommand != null &&
                        parentCommand.IsFinished && parentCommand.Result ==
                            UnitCommand.ResultType.Success && activeWard != null &&
                        touchCommand != null && touchCommand.IsFinished &&
                        touchCommand.Result == UnitCommand.ResultType.Success &&
                        rule != null && rule.Check == RuleDispelMagic.CheckType.None &&
                        rule.Success && rule.CheckRoll == 0 &&
                        row.Expected.IsDisposed &&
                        !target.Buffs.Enumerable.Contains(row.Expected) &&
                        (row.Preserved == null || !row.Preserved.IsDisposed &&
                            target.Buffs.Enumerable.Contains(row.Preserved)) &&
                        row.Owner.Get<UnitPartTouch>() == null &&
                        !row.Owner.Buffs.Enumerable.Any(value =>
                            ReferenceEquals(value.Blueprint, ward));
                    Sprint25Check("astral-dispel-automatic-" + row.Label,
                        removed, new JObject { ["owner"] = row.Owner.UniqueId,
                            ["target"] = target.UniqueId,
                            ["parent"] = parentCommand == null ? null :
                                Sprint27AiCommand(parentCommand),
                            ["touch"] = touchCommand == null ? null :
                                Sprint27AiCommand(touchCommand),
                            ["selected"] = row.Expected.Blueprint.AssetGuid,
                            ["selectedCl"] = row.Expected.Context.Params.CasterLevel,
                            ["checkType"] = rule?.Check.ToString(),
                            ["ruleSuccess"] = rule?.Success,
                            ["checkRoll"] = rule?.CheckRoll,
                            ["removed"] = row.Expected.IsDisposed,
                            ["preserved"] = row.Preserved == null ? null :
                                (bool?)!row.Preserved.IsDisposed,
                            ["wardAfterDelivery"] = row.Owner.Buffs.Enumerable.Any(
                                value => ReferenceEquals(value.Blueprint, ward)),
                            ["touchAfterDelivery"] = row.Owner.Get<
                                UnitPartTouch>() != null },
                        "One real held melee touch invokes native RuleDispelMagic.CheckType.None, removes exactly the highest-level eligible buff, spends no save/SR check, preserves every other effect, and discharges only its own ward/touch.");
                    if (!removed) throw new InvalidOperationException(
                        "Astral automatic Dispel Evil failed: " + row.Label);
                }
                RuleSpellResistanceCheck[] automaticResistance = observed
                    .Resistances.Skip(resistanceBeforeDispel).ToArray();
                bool automaticBoundary = observed.Dispels.Count ==
                        dispelsBefore + 2 && observed.Saves.Count ==
                        savesBeforeDispel && automaticResistance.Length == 2 &&
                    automaticResistance.All(value => ReferenceEquals(value.Ability,
                            dispelTouch) && !value.Ability.SpellResistance &&
                        ReferenceEquals(value.Target, target) &&
                        value.HasSpellResistance && value.SpellResistance == 1000 &&
                        !value.HasResistanceRoll && !value.TargetIsImmune &&
                        !value.IsSpellResisted) &&
                    !AstralDispelEvilEligibility.CanDispel(goodEnchantment) &&
                    !AstralDispelEvilEligibility.CanDispel(ordinarySpell) &&
                    !AstralDispelEvilEligibility.CanDispel(protectedEvil) &&
                    target.Buffs.Enumerable.Contains(goodEnchantment) &&
                    target.Buffs.Enumerable.Contains(ordinarySpell) &&
                    target.Buffs.Enumerable.Contains(protectedEvil);
                Sprint25Check("astral-dispel-automatic-boundary",
                    automaticBoundary, new JObject {
                        ["rules"] = observed.Dispels.Count - dispelsBefore,
                        ["newSaves"] = observed.Saves.Count - savesBeforeDispel,
                        ["nativeResistanceEvents"] = automaticResistance.Length,
                        ["actualResistanceChecks"] = automaticResistance.Count(
                            value => value.HasResistanceRoll),
                        ["resistance"] = new JArray(automaticResistance.Select(
                            value => new JObject {
                                ["caster"] = value.Context?.MaybeCaster?.UniqueId,
                                ["target"] = value.Target.UniqueId,
                                ["ability"] = value.Ability.AssetGuid,
                                ["abilityAllowsSr"] = value.Ability.SpellResistance,
                                ["targetSr"] = value.SpellResistance,
                                ["hasResistanceRoll"] = value.HasResistanceRoll,
                                ["immune"] = value.TargetIsImmune,
                                ["resisted"] = value.IsSpellResisted })),
                        ["goodEnchantmentPresent"] = target.Buffs.Enumerable
                            .Contains(goodEnchantment),
                        ["ordinarySpellPresent"] = target.Buffs.Enumerable
                            .Contains(ordinarySpell),
                        ["nondispellableEvilPresent"] = target.Buffs.Enumerable
                            .Contains(protectedEvil) },
                    "The two automatic removals are independent no-check rules; ineligible and nondispellable states remain, and neither a saving throw nor spell resistance is consulted.");
                if (!automaticBoundary) throw new InvalidOperationException(
                    "Astral automatic Dispel Evil boundary failed.");

                observed.Owner = expiryOwner;
                Ability expiryRoot = expiryOwner.Descriptor.Abilities
                    .GetAbility(family);
                var expiryData = new AbilityData(new AbilityData(expiryRoot),
                    dispel);
                UnitUseAbility expiryCommand = null;
                foreach (int frame in Astral31DispelCast(expiryOwner,
                    expiryData, new TargetWrapper(expiryOwner), value =>
                        expiryCommand = value)) yield return frame;
                Buff expiringWard = expiryOwner.Buffs.Enumerable.SingleOrDefault(
                    value => ReferenceEquals(value.Blueprint, ward));
                UnitPartTouch expiringTouch = expiryOwner.Get<UnitPartTouch>();
                double expiryStart = Game.Instance.TimeController.GameTime.TotalSeconds;
                DateTime expiryDeadline = DateTime.UtcNow.AddSeconds(95);
                int expiryFrames = 0;
                while (DateTime.UtcNow < expiryDeadline && expiryFrames++ < 12000 &&
                    (expiryOwner.Buffs.Enumerable.Contains(expiringWard) ||
                        expiryOwner.Get<UnitPartTouch>() != null)) yield return 0;
                double elapsed = Game.Instance.TimeController.GameTime.TotalSeconds -
                    expiryStart;
                Buff summonLifetime = expiryOwner.Buffs.Enumerable.FirstOrDefault(
                    value => ReferenceEquals(value.Blueprint,
                        Kingmaker.Blueprints.Root.BlueprintRoot.Instance
                            .SystemMechanics.SummonedUnitBuff));
                bool expiryOwnerAlive = !expiryOwner.Destroyed &&
                    expiryOwner.IsInState && !expiryOwner.Descriptor.State.IsDead &&
                    summonLifetime != null && !summonLifetime.IsDisposed &&
                    summonLifetime.TimeLeft.TotalSeconds > 0d;
                bool expiry = expiryOwnerAlive &&
                    expiryCommand != null && expiryCommand.IsFinished &&
                    expiringWard != null && expiringTouch != null &&
                    expiringWard.IsDisposed &&
                    !expiryOwner.Buffs.Enumerable.Contains(expiringWard) &&
                    expiryOwner.Get<UnitPartTouch>() == null && elapsed >= 70d &&
                    elapsed <= 90d;
                Sprint25Check("astral-dispel-native-expiry", expiry,
                    new JObject { ["owner"] = expiryOwner.UniqueId,
                        ["command"] = expiryCommand == null ? null :
                            Sprint27AiCommand(expiryCommand),
                        ["initialWardSeconds"] = 78,
                        ["ownerAlive"] = expiryOwnerAlive,
                        ["nativeSummonSecondsRemaining"] =
                            summonLifetime?.TimeLeft.TotalSeconds,
                        ["elapsedGameSeconds"] = elapsed,
                        ["frames"] = expiryFrames,
                        ["wardDisposed"] = expiringWard?.IsDisposed,
                        ["wardPresent"] = expiryOwner.Buffs.Enumerable.Contains(
                            expiringWard),
                        ["touchPresent"] = expiryOwner.Get<UnitPartTouch>() != null,
                        ["manualRemoval"] = false },
                    "Without delivering the held action, the native game clock expires the 13-round ward and its exact owner-bound future touch together while the same living summon retains its active native duration marker.");
                if (!expiry) throw new InvalidOperationException(
                    "Astral Dispel Evil native expiry failed.");

                EventBus.Unsubscribe(observed);
                observed = null;
                foreach (Buff buff in transientBuffs.Where(value => value != null &&
                    !value.IsDisposed).ToArray()) buff.Remove();
                target.Descriptor.Alignment.Set(targetAlignment);
                target.Stats.AC.BaseValue = targetAc;
                target.Stats.SaveWill.BaseValue = targetWill;
                target.Stats.HitPoints.BaseValue = targetHitPoints;
                fixtureResistance.Remove(fixtureResistanceId);
                fixtureResistanceId = -1;
                JObject resistanceAfter = Lemure30SrState(target,
                    fixtureResistanceContext);
                bool resistanceRestored = JToken.DeepEquals(
                        fixtureResistanceBefore["entries"], resistanceAfter["entries"]) &&
                    (int)fixtureResistanceBefore["nativeValue"] ==
                        (int)resistanceAfter["nativeValue"];
                string hostileReset = ResetExpandedSummoningHostile(fixture);
                UnitEntityData[] survivors = { dismissalOwner, firstDispelOwner,
                    secondDispelOwner, expiryOwner };
                foreach (UnitEntityData unit in survivors.Where(value =>
                    value != null && !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                    if (unit.IsInCombat) unit.LeaveCombat();
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.Where(value => value != null && !value.Destroyed)
                        .ToArray());
                var areasBeforeSettle = Game.Instance.State.AreaEffects.All.ToArray();
                DateTime cleanupDeadline = DateTime.UtcNow.AddSeconds(8);
                int cleanupFrames = 0;
                // Destroyed units can precede native aura-area retirement.
                // Observe that retirement; never delete an area to force PASS.
                while (DateTime.UtcNow < cleanupDeadline && cleanupFrames++ < 1000 &&
                    (owned.Any(value => value != null && !value.Destroyed) ||
                        !Game.Instance.State.AreaEffects.All.SequenceEqual(areasBefore) ||
                        target.Buffs.Enumerable.Any(value => owned.Contains(
                            value.Context?.MaybeCaster))))
                    yield return 0;
                foreach (UnitEntityData unit in owned.Where(value => value != null))
                {
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                bool areasExact = Game.Instance.State.AreaEffects.All
                    .SequenceEqual(areasBefore);
                bool auraGrantsAbsent = !target.Buffs.Enumerable.Any(value =>
                    owned.Contains(value.Context?.MaybeCaster));
                bool cleanup = resistanceRestored && auraGrantsAbsent &&
                    AbilityParams.Empty.CasterLevel == sharedCl &&
                    AbilityParams.Empty.SpellLevel == sharedSpellLevel &&
                    AbilityParams.Empty.DC == sharedDc &&
                    owned.All(value => value.Destroyed &&
                        !value.IsInState && !value.Commands.Raw.Any(command =>
                            command != null && !command.IsFinished)) && areasExact &&
                    transientBuffs.All(value => value.IsDisposed ||
                        !target.Buffs.Enumerable.Contains(value)) &&
                    target.Descriptor.Alignment.Value == targetAlignment &&
                    target.Stats.AC.BaseValue == targetAc &&
                    target.Stats.SaveWill.BaseValue == targetWill &&
                    target.Stats.HitPoints.BaseValue == targetHitPoints &&
                    target.Damage == fixture.HostileDamage &&
                    target.Descriptor.State.Size == fixture.HostileSize;
                Sprint25Check("astral-dispel-owned-cleanup", cleanup,
                    new JObject { ["ownersDestroyed"] = new JArray(owned.Select(
                            value => value.Destroyed)),
                        ["ownersAbsent"] = new JArray(owned.Select(value =>
                            !value.IsInState)),
                        ["activeCommands"] = owned.Sum(value => value.Commands.Raw
                            .Count(command => command != null && !command.IsFinished)),
                        ["areasExact"] = areasExact,
                        ["areasBaseline"] = new JArray(areasBefore.Select(value =>
                            new JObject { ["id"] = value.UniqueId,
                                ["blueprint"] = value.Blueprint?.AssetGuid,
                                ["owner"] = value.Context?.MaybeCaster?.UniqueId })),
                        ["areasBeforeSettle"] = new JArray(areasBeforeSettle.Select(value =>
                            new JObject { ["id"] = value.UniqueId,
                                ["blueprint"] = value.Blueprint?.AssetGuid,
                                ["owner"] = value.Context?.MaybeCaster?.UniqueId })),
                        ["areasAfterSettle"] = new JArray(
                            Game.Instance.State.AreaEffects.All.Select(value =>
                                new JObject { ["id"] = value.UniqueId,
                                    ["blueprint"] = value.Blueprint?.AssetGuid,
                                    ["owner"] = value.Context?.MaybeCaster?.UniqueId })),
                        ["auraGrantsAbsent"] = auraGrantsAbsent,
                        ["remainingOwnedAuraGrants"] = new JArray(
                            target.Buffs.Enumerable.Where(value => owned.Contains(
                                value.Context?.MaybeCaster)).Select(value =>
                                    new JObject { ["blueprint"] = value.Blueprint.AssetGuid,
                                        ["owner"] = value.Context.MaybeCaster?.UniqueId })),
                        ["fixtureResistanceRestored"] = resistanceRestored,
                        ["fixtureResistanceAfter"] = resistanceAfter,
                        ["sharedEmptyUnchanged"] =
                            AbilityParams.Empty.CasterLevel == sharedCl &&
                            AbilityParams.Empty.SpellLevel == sharedSpellLevel &&
                            AbilityParams.Empty.DC == sharedDc,
                        ["transientBuffsAbsent"] = transientBuffs.All(value =>
                            value.IsDisposed || !target.Buffs.Enumerable.Contains(value)),
                        ["hostileReset"] = hostileReset,
                        ["hostileAlignmentRestored"] = target.Descriptor.Alignment
                            .Value == targetAlignment,
                        ["hostileAcRestored"] = target.Stats.AC.BaseValue == targetAc,
                        ["hostileWillRestored"] = target.Stats.SaveWill.BaseValue ==
                            targetWill,
                        ["hostileHpRestored"] = target.Stats.HitPoints.BaseValue ==
                            targetHitPoints,
                        ["cleanupFrames"] = cleanupFrames },
                    "The fresh transaction removes every request-owned Astral, held command, ward and transient spell effect, preserves the exact area set, and restores every changed hostile field.");
                if (!cleanup) throw new InvalidOperationException(
                    "Astral Dispel Evil owned cleanup failed.");
                targetChanged = false;
            }
            finally
            {
                if (killTrace != null) killTrace.Dispose();
                if (fixtureResistance != null && fixtureResistanceId >= 0)
                {
                    fixtureResistance.Remove(fixtureResistanceId);
                    fixtureResistanceId = -1;
                }
                if (observed != null) EventBus.Unsubscribe(observed);
                if (target != null && !target.Destroyed)
                {
                    foreach (Buff buff in transientBuffs.Where(value =>
                        value != null && !value.IsDisposed).ToArray()) buff.Remove();
                    if (targetChanged)
                    {
                        target.Descriptor.Alignment.Set(targetAlignment);
                        target.Stats.AC.BaseValue = targetAc;
                        target.Stats.SaveWill.BaseValue = targetWill;
                        target.Stats.HitPoints.BaseValue = targetHitPoints;
                    }
                }
                UnitEntityData[] owned = { dismissalOwner, dismissalTarget,
                    firstDispelOwner, secondDispelOwner, expiryOwner };
                foreach (UnitEntityData unit in owned.Where(value => value != null &&
                    !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    if (unit.IsInCombat) unit.LeaveCombat();
                }
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value =>
                    value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                foreach (UnityEngine.Object value in transient.Where(value =>
                    value != null).Reverse<UnityEngine.Object>())
                    UnityEngine.Object.DestroyImmediate(value);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> Astral31DispelCast(UnitEntityData owner,
            AbilityData data, TargetWrapper target, Action<UnitUseAbility> result)
        {
            if (owner == null || data == null || !data.IsAvailable ||
                !data.CanTarget(target))
                throw new InvalidOperationException(
                    "Astral Dispel Evil parent is unavailable.");
            var command = new UnitUseAbility(data, target);
            result(command);
            owner.Commands.Run(command);
            DateTime deadline = DateTime.UtcNow.AddSeconds(25);
            int frames = 0;
            while (frames++ < 3000 && DateTime.UtcNow < deadline)
            {
                if (command.ExecutionProcess != null &&
                    !command.ExecutionProcess.IsEnded)
                    command.ExecutionProcess.Tick();
                yield return 0;
                if (command.IsFinished && command.ExecutionProcess != null &&
                    command.ExecutionProcess.IsEnded) break;
            }
            owner.Commands.RemoveFinishedAndUpdateQueue();
            if (!command.IsFinished || command.ExecutionProcess == null ||
                !command.ExecutionProcess.IsEnded)
                throw new InvalidOperationException(
                    "Astral Dispel Evil parent did not finish.");
        }

        private IEnumerable<int> Astral31DispelHeld(UnitEntityData owner,
            UnitEntityData target, Action<UnitUseAbility> result)
        {
            UnitPartTouch touch = owner?.Get<UnitPartTouch>();
            AbilityData data = touch?.Ability?.Data;
            var selected = new TargetWrapper(target);
            if (data == null || !data.CanTarget(selected))
                throw new InvalidOperationException(
                    "Astral Dispel Evil held touch is unavailable.");
            var command = new UnitUseAbility(data, selected);
            result(command);
            owner.Commands.Run(command);
            DateTime deadline = DateTime.UtcNow.AddSeconds(25);
            int frames = 0;
            while (frames++ < 3000 && DateTime.UtcNow < deadline)
            {
                if (command.ExecutionProcess != null &&
                    !command.ExecutionProcess.IsEnded)
                    command.ExecutionProcess.Tick();
                yield return 0;
                if (command.IsFinished && command.ExecutionProcess != null &&
                    command.ExecutionProcess.IsEnded) break;
            }
            owner.Commands.RemoveFinishedAndUpdateQueue();
            if (!command.IsFinished || command.ExecutionProcess == null ||
                !command.ExecutionProcess.IsEnded)
                throw new InvalidOperationException(
                    "Astral Dispel Evil held touch did not finish.");
        }

        private static int Astral31DispelAttackAc(UnitEntityData attacker,
            UnitEntityData target)
        {
            var weapon = attacker?.Body?.PrimaryHand?.MaybeWeapon;
            if (weapon == null) throw new InvalidOperationException(
                "Astral Dispel Evil AC source has no equipped primary weapon.");
            int damage = target.Damage;
            try
            {
                RuleAttackRoll roll = Rulebook.Trigger(new RuleAttackWithWeapon(
                    attacker, target, weapon, 0)).AttackRoll;
                if (roll?.ACRule == null) throw new InvalidOperationException(
                    "Astral Dispel Evil AC probe did not calculate target AC.");
                return roll.TargetAC;
            }
            finally
            {
                target.Damage = damage;
            }
        }

        private static Buff Astral31DispelFixtureEffect(UnitEntityData target,
            UnitEntityData source, string name, string abilityGuid,
            string buffGuid, int casterLevel, SpellSchool school,
            SpellDescriptor descriptor, bool nondispellable,
            ICollection<UnityEngine.Object> transient,
            ICollection<Buff> applied, Astral31DispelEvents observed)
        {
            var ability = ScriptableObject.CreateInstance<BlueprintAbility>();
            Astral31DispelSetGuid(ability, abilityGuid);
            ability.name = "KMG_Runtime_AstralDispel_" + name + "_Ability";
            ability.Type = AbilityType.Spell;
            var schoolComponent = ScriptableObject.CreateInstance<SpellComponent>();
            schoolComponent.School = school;
            var descriptorComponent = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            descriptorComponent.Descriptor = descriptor;
            ability.ComponentsArray = new BlueprintComponent[] {
                schoolComponent, descriptorComponent };
            transient.Add(ability); transient.Add(schoolComponent);
            transient.Add(descriptorComponent);

            var blueprint = ScriptableObject.CreateInstance<BlueprintBuff>();
            Astral31DispelSetGuid(blueprint, buffGuid);
            blueprint.name = "KMG_Runtime_AstralDispel_" + name + "_Buff";
            blueprint.IsClassFeature = false;
            blueprint.Stacking = StackingType.Stack;
            blueprint.ComponentsArray = new BlueprintComponent[] {
                descriptorComponent };
            transient.Add(blueprint);
            var context = new AbilityExecutionContext(
                new AbilityData(ability, source.Descriptor), new AbilityParams {
                    CasterLevel = casterLevel, SpellLevel = 4, DC = 10 },
                new TargetWrapper(target), null);
            Buff result = target.Descriptor.Buffs.AddBuff(blueprint, context,
                TimeSpan.FromMinutes(10));
            bool apiReturnedBuff = result != null;
            RuleApplyBuff native = observed.Applications.LastOrDefault(value =>
                ReferenceEquals(value.Blueprint, blueprint) &&
                ReferenceEquals(value.Initiator, target));
            Buff[] actual = target.Descriptor.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, blueprint) &&
                ReferenceEquals(value.Context?.MaybeCaster, source) &&
                ReferenceEquals(value.Context?.SourceAbility, ability)).ToArray();
            // The engine can insert a request-local fact yet return null.
            // Recover only its one exact attributed live collection instance,
            // the same narrow pattern used by other qualified native fixtures.
            if (result == null && actual.Length == 1) result = actual[0];
            if (result == null || actual.Length != 1 ||
                !ReferenceEquals(result, actual[0]) || native == null ||
                !native.CanApply || native.Immunity || native.StoryModeImmunity ||
                result.Context.Params.CasterLevel != casterLevel ||
                result.Context.Params.SpellLevel != 4)
            {
                var refusal = new JObject {
                    ["fixture"] = name, ["target"] = target.UniqueId,
                    ["targetDead"] = target.Descriptor.State.IsDead,
                    ["targetLifeState"] = target.Descriptor.State.LifeState.ToString(),
                    ["buffsDisabled"] = Convert.ToString(
                        ReadExpandedSummoningOptionalMember(target.Descriptor.Buffs,
                            "m_Disabled")),
                    ["nativeRuleSeen"] = native != null,
                    ["apiReturnedBuff"] = apiReturnedBuff,
                    ["actualAttributedCount"] = actual.Length,
                    ["canApply"] = native?.CanApply,
                    ["immunity"] = native?.Immunity,
                    ["storyModeImmunity"] = native?.StoryModeImmunity,
                    ["source"] = source.UniqueId,
                    ["ability"] = ability.AssetGuid,
                    ["spellLevel"] = context.Params.SpellLevel,
                    ["casterLevel"] = context.Params.CasterLevel,
                    ["contextDescriptors"] = context.SpellDescriptor.ToString(),
                    ["nativeSource"] = native?.Context?.MaybeCaster?.UniqueId,
                    ["nativeSpellLevel"] = native?.Context?.Params?.SpellLevel,
                    ["nativeCasterLevel"] = native?.Context?.Params?.CasterLevel,
                    ["nativeDescriptors"] = native?.Context?.SpellDescriptor.ToString(),
                    ["removeOnResurrect"] = blueprint.RemoveOnResurrect,
                    ["immortality"] = target.Descriptor.State.Immortality.Count,
                    ["targetBuffs"] = new JArray(target.Buffs.Enumerable.Select(
                        value => new JObject { ["guid"] = value.Blueprint.AssetGuid,
                            ["name"] = value.Blueprint.name,
                            ["source"] = value.Context?.MaybeCaster?.UniqueId }))
                };
                throw new InvalidOperationException(
                    "Astral Dispel Evil fixture buff was refused: " +
                    refusal.ToString(Newtonsoft.Json.Formatting.None));
            }
            result.IsFromSpell = true;
            result.IsNotDispelable = nondispellable;
            observed.FixtureApplications.Add(new JObject {
                ["fixture"] = name, ["target"] = target.UniqueId,
                ["apiReturnedBuff"] = apiReturnedBuff,
                ["actualAttributedCount"] = actual.Length,
                ["nativeCanApply"] = native.CanApply,
                ["nativeImmunity"] = native.Immunity,
                ["nativeStoryModeImmunity"] = native.StoryModeImmunity,
                ["effect"] = Astral31DispelBuffRow(result) });
            applied.Add(result);
            return result;
        }

        private static void Astral31DispelSetGuid(
            BlueprintScriptableObject blueprint, string guid)
        {
            if (BlueprintBootstrap.Library.BlueprintsByAssetId.Values.Any(value =>
                value != null && value.AssetGuid == guid))
                throw new InvalidOperationException(
                    "Astral Dispel Evil request-local GUID collision: " + guid);
            FieldInfo field = typeof(BlueprintScriptableObject).GetField(
                "m_AssetGuid", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(string))
                throw new MissingFieldException(
                    typeof(BlueprintScriptableObject).FullName, "m_AssetGuid");
            field.SetValue(blueprint, guid);
        }

        private static JObject Astral31DispelBuffRow(Buff value)
        {
            return new JObject { ["buff"] = value?.Blueprint?.AssetGuid,
                ["ability"] = value?.Context?.SourceAbility?.AssetGuid,
                ["source"] = value?.Context?.MaybeCaster?.UniqueId,
                ["casterLevel"] = value?.Context?.Params?.CasterLevel,
                ["school"] = value?.Context?.SpellSchool.ToString(),
                ["descriptors"] = value?.Context?.SpellDescriptor.ToString(),
                ["fromSpell"] = value?.IsFromSpell,
                ["nondispellable"] = value?.IsNotDispelable,
                ["eligible"] = value != null &&
                    AstralDispelEvilEligibility.CanDispel(value) };
        }

        private sealed class Astral31DismissalKillTrace : IDisposable
        {
            private static Astral31DismissalKillTrace active;
            private readonly UnitEntityData owner;
            private readonly UnitEntityData target;
            private readonly Harmony12.HarmonyInstance harmony;
            private readonly string id = "KMG.S31.AstralDismissalKillTrace." +
                Guid.NewGuid().ToString("N");
            private readonly MethodInfo method = typeof(ContextActionKillTarget)
                .GetMethod("RunAction", BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic);
            private readonly PropertyInfo contextProperty = typeof(ContextAction)
                .GetProperty("Context", BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic);
            private readonly PropertyInfo targetProperty = typeof(ContextAction)
                .GetProperty("Target", BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic);
            private bool disposed;
            internal readonly JArray Calls = new JArray();
            internal bool Restored { get; private set; }

            internal Astral31DismissalKillTrace(UnitEntityData owner,
                UnitEntityData target)
            {
                if (active != null || owner == null || target == null ||
                    method == null || contextProperty == null ||
                    targetProperty == null)
                    throw new InvalidOperationException(
                        "Astral native Dismissal kill trace is unavailable or nested.");
                this.owner = owner;
                this.target = target;
                harmony = Harmony12.HarmonyInstance.Create(id);
                var flags = BindingFlags.Static | BindingFlags.NonPublic;
                try
                {
                    harmony.Patch(method, new Harmony12.HarmonyMethod(typeof(
                        Astral31DismissalKillTrace).GetMethod("Before", flags)),
                        new Harmony12.HarmonyMethod(typeof(
                            Astral31DismissalKillTrace).GetMethod("After", flags)));
                    active = this;
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            private static void Before(ContextActionKillTarget __instance,
                out JObject __state)
            {
                __state = null;
                Astral31DismissalKillTrace trace = active;
                MechanicsContext context = __instance == null ? null :
                    trace?.contextProperty.GetValue(__instance, null) as
                        MechanicsContext;
                TargetWrapper wrapper = __instance == null ? null :
                    trace?.targetProperty.GetValue(__instance, null) as
                        TargetWrapper;
                UnitEntityData actionTarget = wrapper?.Unit;
                if (trace == null || !ReferenceEquals(context?.MaybeCaster,
                        trace.owner) || !ReferenceEquals(actionTarget,
                        trace.target)) return;
                __state = new JObject {
                    ["contextCaster"] = context.MaybeCaster.UniqueId,
                    ["contextMainTarget"] = context.MainTarget.Unit?.UniqueId,
                    ["actionTarget"] = actionTarget.UniqueId,
                    ["sourceAbility"] = context.SourceAbility?.AssetGuid,
                    ["nativeGuardBefore"] = !Kingmaker.Designers.GameHelper
                        .IsAttackingGreenNPC(context.MaybeCaster, actionTarget),
                    ["markedBefore"] = actionTarget.Descriptor.State
                        .MarkedForDeath,
                    ["shouldDestroyBefore"] = actionTarget.ShouldBeDestroyed,
                    ["inStateBefore"] = actionTarget.IsInState,
                    ["callers"] = new JArray((new System.Diagnostics.StackTrace(
                        1, false).GetFrames() ?? Array.Empty<
                            System.Diagnostics.StackFrame>()).Take(12).Select(
                                value => (value.GetMethod().DeclaringType?.FullName ??
                                    "<dynamic>") + "." + value.GetMethod().Name))
                };
            }

            private static void After(JObject __state)
            {
                Astral31DismissalKillTrace trace = active;
                if (trace == null || __state == null) return;
                __state["markedAfter"] = trace.target.Descriptor.State
                    .MarkedForDeath;
                __state["shouldDestroyAfter"] = trace.target.ShouldBeDestroyed;
                __state["inStateAfter"] = trace.target.IsInState;
                trace.Calls.Add(__state);
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;
                if (ReferenceEquals(active, this)) active = null;
                if (harmony == null || method == null)
                {
                    Restored = true;
                    return;
                }
                harmony.Unpatch(method, Harmony12.HarmonyPatchType.All, id);
                var info = harmony.GetPatchInfo(method);
                Restored = info == null || (!info.Prefixes.Any(value =>
                        value.owner == id) && !info.Postfixes.Any(value =>
                        value.owner == id));
                if (!Restored) throw new InvalidOperationException(
                    "Astral native Dismissal kill trace remains installed.");
            }
        }

        private sealed class Astral31DispelEvents :
            IGlobalRulebookHandler<RuleAttackRoll>,
            IGlobalRulebookHandler<RuleSpellResistanceCheck>,
            IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleDispelMagic>,
            IGlobalRulebookHandler<RuleApplyBuff>
        {
            internal UnitEntityData Owner;
            internal readonly HashSet<UnitEntityData> Targets =
                new HashSet<UnitEntityData>();
            internal readonly List<RuleAttackRoll> Attacks =
                new List<RuleAttackRoll>();
            internal readonly List<RuleSpellResistanceCheck> Resistances =
                new List<RuleSpellResistanceCheck>();
            internal readonly List<RuleSavingThrow> Saves =
                new List<RuleSavingThrow>();
            internal readonly List<RuleDispelMagic> Dispels =
                new List<RuleDispelMagic>();
            internal readonly List<RuleApplyBuff> Applications =
                new List<RuleApplyBuff>();
            internal readonly JArray FixtureApplications = new JArray();

            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                if (ReferenceEquals(evt?.Initiator, Owner) &&
                    Targets.Contains(evt.Target))
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
            }
            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                if (ReferenceEquals(evt?.Initiator, Owner) &&
                    Targets.Contains(evt.Target)) Attacks.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
            {
                if (ReferenceEquals(evt?.Context?.MaybeCaster, Owner) &&
                    Targets.Contains(evt.Target))
                    UnityEngine.Random.InitState(FindNativeD20Seed(20));
            }
            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            {
                if (ReferenceEquals(evt?.Context?.MaybeCaster, Owner) &&
                    Targets.Contains(evt.Target)) Resistances.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleSavingThrow evt)
            {
                if (Targets.Contains(evt?.Initiator) && ReferenceEquals(
                    evt?.Reason?.Context?.MaybeCaster, Owner))
                    UnityEngine.Random.InitState(FindNativeD20Seed(1));
            }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (Targets.Contains(evt?.Initiator) && ReferenceEquals(
                    evt?.Reason?.Context?.MaybeCaster, Owner)) Saves.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (Targets.Contains(evt?.Initiator)) Applications.Add(evt);
            }
            public void OnEventAboutToTrigger(RuleDispelMagic evt) { }
            public void OnEventDidTrigger(RuleDispelMagic evt)
            {
                if (ReferenceEquals(evt?.Initiator, Owner) &&
                    Targets.Contains(evt.Target)) Dispels.Add(evt);
            }
        }
    }
}
