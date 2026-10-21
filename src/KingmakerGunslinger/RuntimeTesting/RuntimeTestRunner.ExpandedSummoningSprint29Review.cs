using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool IsSprint29Review { get { return _request.Scenario ==
            RuntimeTestScenarioCatalog.DisposableExpandedSummoningSprint29NativeReview; } }

        // First complete native path: one bounded fresh-world execution
        // covers the three exact hidden-candidate bodies, their adult chassis,
        // resolved armament and initial resources. Combat and public routes
        // are separate phases and cannot inherit this verdict.
        private IEnumerable<int> ReviewSprint29(
            ExpandedSummoningCorrectionFixture fixture)
        {
            string phase = RuntimeTestRequestParser.Sprint29ReviewPhase(_request);
            Sprint25Check("phase", phase == "profiles" || phase == "modes",
                new JObject { ["phase"] = phase },
                "Exactly one implemented Sprint 29 native review phase.");
            if (phase == "modes")
            {
                foreach (int frame in ReviewSprint29Modes(fixture))
                    yield return frame;
                yield break;
            }
            if (phase != "profiles") yield break;
            bool oldTurnBased = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            try
            {
                var hiddenRoutes = ExpandedSummoningCatalog
                    .GenerateVariants(SummonFamily.NaturesAlly)
                    .Where(value => value.Creature.Key == "ettin" ||
                        value.Creature.Key == "cloud-giant" ||
                        value.Creature.Key == "storm-giant").ToArray();
                var hiddenIds = new HashSet<string>(hiddenRoutes.Select(value =>
                    fixture.Blueprints.OfType<BlueprintAbility>().Single(ability =>
                        ability.name == ExpandedSummoningInternalName(
                            ExpandedSummoningIdentityCatalog.AbilitySymbol(value)))
                        .AssetGuid), StringComparer.Ordinal);
                var parents = ExpandedSummoningInventoryObserver
                    .CanonicalParentGuids.Select(guid =>
                        BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                            BlueprintBootstrap.Library, guid,
                            "Sprint 29 live summon parent")).ToArray();
                var displayed = parents.SelectMany(parent =>
                    parent.ComponentsArray.OfType<AbilityVariants>().Single()
                        .Variants).ToArray();
                var hill = ExpandedSummoningCatalog
                    .GenerateVariants(SummonFamily.NaturesAlly).Single(value =>
                        value.Creature.Key == "hill-giant" &&
                        value.ParentTier == 6);
                string hillGuid = fixture.Blueprints.OfType<BlueprintAbility>()
                    .Single(ability => ability.name ==
                        ExpandedSummoningInternalName(
                            ExpandedSummoningIdentityCatalog.AbilitySymbol(hill)))
                    .AssetGuid;
                Sprint25Check("hidden-publication",
                    hiddenRoutes.Length == 8 && hiddenIds.Count == 8 &&
                    displayed.All(value => !hiddenIds.Contains(value.AssetGuid)) &&
                    parents[14].ComponentsArray.OfType<AbilityVariants>()
                        .Single().Variants.Any(value => value.AssetGuid == hillGuid),
                    new JObject { ["hiddenRoots"] = hiddenRoutes.Length,
                        ["hiddenInParents"] = displayed.Count(value =>
                            hiddenIds.Contains(value.AssetGuid)),
                        ["releasedHillTier6Present"] = parents[14]
                            .ComponentsArray.OfType<AbilityVariants>().Single()
                            .Variants.Any(value => value.AssetGuid == hillGuid) },
                    "All new roots are absent from live parents and a released giant remains present.");
                Game.Instance.IsPaused = true;
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int i = 0; i < 12; i++) yield return 0;
                foreach (var spec in new[] {
                    new { Key="ettin", Token="Ettin", Size=Size.Large,
                        Scores=new[]{23,8,15,6,10,11}, Bab=7, Hp=65,
                        Ac=18, Touch=8, Flat=18, Fort=9, Ref=2, Will=5,
                        Speed=40, Perception=12,
                        Main="KMG_Summoning_Giant_Sprint29_EttinMainFlail2d6",
                        Off="KMG_Summoning_Giant_Sprint29_EttinOffFlail2d6" },
                    new { Key="cloud-giant", Token="CloudGiant", Size=Size.Huge,
                        Scores=new[]{35,13,23,12,16,12}, Bab=12, Hp=168,
                        Ac=25, Touch=9, Flat=24, Fort=16, Ref=6, Will=10,
                        Speed=50, Perception=17,
                        Main="KMG_Summoning_Giant_Sprint29_CloudMorningstar4d6",
                        Off="KMG_Summoning_Giant_CloudSlam2d6" },
                    new { Key="storm-giant", Token="StormGiant", Size=Size.Huge,
                        Scores=new[]{39,14,23,16,20,15}, Bab=14, Hp=199,
                        Ac=28, Touch=10, Flat=26, Fort=17, Ref=8, Will=13,
                        Speed=35, Perception=27,
                        Main="KMG_Summoning_Giant_Sprint29_StormGreatsword4d6",
                        Off="KMG_Summoning_Giant_Sprint29_StormSlam2d6" }
                })
                {
                    UnitEntityData owner = CastExpandedSummoningOwnTier(
                        fixture, spec.Key);
                    SetExpandedSummoningBrainActive(owner, false);
                    int appearanceFrames = 0;
                    DateTime appearanceDeadline = DateTime.UtcNow.AddSeconds(25);
                    while (appearanceFrames < MotionReviewFadeBudget &&
                        DateTime.UtcNow < appearanceDeadline &&
                        (LoadingOrScreenFadeActive() ||
                         !EntityFadedIn(owner) ||
                         DissolveAmount(owner) > MotionReviewIntactDissolve ||
                         owner.Descriptor.Buffs.GetBuff(Kingmaker.Blueprints
                            .Root.BlueprintRoot.Instance.SystemMechanics
                            .SummonedUnitAppearBuff) != null))
                    {
                        appearanceFrames++;
                        yield return 0;
                    }
                    var stats = owner.Stats;
                    int[] scores = { stats.Strength.ModifiedValue,
                        stats.Dexterity.ModifiedValue,
                        stats.Constitution.ModifiedValue,
                        stats.Intelligence.ModifiedValue,
                        stats.Wisdom.ModifiedValue,
                        stats.Charisma.ModifiedValue };
                    string main = owner.Body.PrimaryHand.MaybeWeapon?.Blueprint?.name;
                    string off = owner.Body.SecondaryHand?.MaybeWeapon?.Blueprint?.name;
                    string configuredOff = owner.Blueprint.Body.SecondaryHand?.name;
                    bool? mainTwoHanded = owner.Body.PrimaryHand.MaybeWeapon?
                        .Blueprint?.Type?.IsTwoHanded;
                    bool? mainHeldInTwoHands = owner.Body.PrimaryHand.MaybeWeapon?
                        .HoldInTwoHands;
                    var handSets = new JArray(owner.Body.HandsEquipmentSets.Select(
                        (set, index) => new JObject {
                            ["index"] = index,
                            ["main"] = set.PrimaryHand?.MaybeWeapon?.Blueprint?.name,
                            ["off"] = set.SecondaryHand?.MaybeWeapon?.Blueprint?.name
                        }).ToArray());
                    bool ettinHands = spec.Key != "ettin" ||
                        mainHeldInTwoHands == false &&
                        !ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon,
                            owner.Body.SecondaryHand.MaybeWeapon);
                    UnitPartSummonEttinInitialArms armMarker =
                        spec.Key == "ettin" ?
                        owner.Get<UnitPartSummonEttinInitialArms>() : null;
                    bool? ettinFlailSeeded = armMarker?.FlailSeeded;
                    bool? ettinJavelinSeeded = armMarker?.JavelinSeeded;
                    int perception = stats.SkillPerception.ModifiedValue;
                    bool profile = scores.SequenceEqual(spec.Scores) &&
                        owner.Descriptor.State.Size == spec.Size &&
                        stats.BaseAttackBonus.ModifiedValue == spec.Bab &&
                        stats.HitPoints.ModifiedValue == spec.Hp &&
                        stats.AC.ModifiedValue == spec.Ac &&
                        stats.AC.Touch == spec.Touch &&
                        stats.AC.FlatFooted == spec.Flat &&
                        stats.SaveFortitude.ModifiedValue == spec.Fort &&
                        stats.SaveReflex.ModifiedValue == spec.Ref &&
                        stats.SaveWill.ModifiedValue == spec.Will &&
                        stats.Speed.ModifiedValue == spec.Speed &&
                        perception == spec.Perception &&
                        main == spec.Main && off == spec.Off && ettinHands &&
                        (spec.Key != "ettin" || ettinFlailSeeded == true);
                    Sprint25Check(spec.Key + "-profile", profile,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["scores"] = new JArray(scores),
                            ["size"] = owner.Descriptor.State.Size.ToString(),
                            ["bab"] = stats.BaseAttackBonus.ModifiedValue,
                            ["hp"] = stats.HitPoints.ModifiedValue,
                            ["ac"] = stats.AC.ModifiedValue,
                            ["touch"] = stats.AC.Touch,
                            ["flat"] = stats.AC.FlatFooted,
                            ["fort"] = stats.SaveFortitude.ModifiedValue,
                            ["reflex"] = stats.SaveReflex.ModifiedValue,
                            ["will"] = stats.SaveWill.ModifiedValue,
                            ["speed"] = stats.Speed.ModifiedValue,
                            ["perception"] = perception,
                            ["main"] = main, ["off"] = off,
                            ["configuredOff"] = configuredOff,
                            ["mainTypeTwoHanded"] = mainTwoHanded,
                            ["mainHeldInTwoHands"] = mainHeldInTwoHands,
                            ["ettinHands"] = ettinHands,
                            ["ettinFlailSeeded"] = ettinFlailSeeded,
                            ["ettinJavelinSeeded"] = ettinJavelinSeeded,
                            ["handSets"] = handSets },
                        "Native adult profile and resolved equipment match the printed anchor.");
                    bool visible = owner.View != null &&
                        !LoadingOrScreenFadeActive() &&
                        EntityFadedIn(owner) &&
                        DissolveAmount(owner) <= MotionReviewIntactDissolve &&
                        owner.Descriptor.Buffs.GetBuff(Kingmaker.Blueprints
                            .Root.BlueprintRoot.Instance.SystemMechanics
                            .SummonedUnitAppearBuff) == null;
                    string head = spec.Key == "ettin" ?
                        EttinSecondHeadVisual.Describe(owner.View) : "native-head";
                    Sprint25Check(spec.Key + "-body", visible &&
                        (spec.Key != "ettin" || head.StartsWith(
                            "attached;heads=2;", StringComparison.Ordinal)),
                        new JObject { ["owner"] = owner.UniqueId,
                            ["view"] = owner.View == null ? null :
                                owner.View.name,
                            ["fadedIn"] = visible,
                            ["appearanceFrames"] = appearanceFrames,
                            ["secondHead"] = head },
                        "Native body is rendered; Ettin has its original and owned second visible head.");
                    string[] abilityTokens = spec.Key == "ettin"
                        ? Array.Empty<string>()
                        : spec.Key == "cloud-giant"
                        ? new[] { "AwesomeBlowAction", "CloudObscuringMist",
                            "CloudFogCloud" }
                        : new[] { "AwesomeBlowAction", "StormCallLightning",
                            "StormChainLightning" };
                    var abilityRows = new JArray();
                    bool abilities = true;
                    foreach (string token in abilityTokens)
                    {
                        string name = "KMG_Summoning_Giant_Sprint29_" + token;
                        BlueprintAbility blueprint = fixture.Blueprints
                            .OfType<BlueprintAbility>().Single(value =>
                                value.name == name);
                        bool granted = owner.Descriptor.Abilities
                            .GetAbility(blueprint) != null;
                        abilityRows.Add(new JObject { ["name"] = name,
                            ["guid"] = blueprint.AssetGuid,
                            ["granted"] = granted });
                        abilities &= granted;
                    }
                    if (spec.Key == "ettin")
                    {
                        BlueprintAbility awesome = fixture.Blueprints
                            .OfType<BlueprintAbility>().Single(value =>
                                value.name == "KMG_Summoning_Giant_Sprint29_AwesomeBlowAction");
                        bool wronglyGranted = owner.Descriptor.Abilities
                            .GetAbility(awesome) != null;
                        abilityRows.Add(new JObject {
                            ["name"] = awesome.name,
                            ["guid"] = awesome.AssetGuid,
                            ["granted"] = wronglyGranted,
                            ["expected"] = false });
                        abilities &= !wronglyGranted;
                    }
                    var resourceRows = new JArray();
                    foreach (string token in spec.Key == "cloud-giant"
                        ? new[] { "CloudFogResource" }
                        : spec.Key == "storm-giant"
                        ? new[] { "StormCallResource",
                            "StormChainResource" }
                        : Array.Empty<string>())
                    {
                        string name = "KMG_Summoning_Giant_Sprint29_" + token;
                        BlueprintAbilityResource resource = fixture.Blueprints
                            .OfType<BlueprintAbilityResource>().Single(value =>
                                value.name == name);
                        int amount = owner.Descriptor.Resources
                            .GetResourceAmount(resource);
                        resourceRows.Add(new JObject { ["name"] = name,
                            ["guid"] = resource.AssetGuid,
                            ["amount"] = amount });
                        abilities &= amount == 1;
                    }
                    Sprint25Check(spec.Key + "-initial-actions", abilities,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["abilities"] = abilityRows,
                            ["resources"] = resourceRows },
                        "Direct actions and independent limited resources are granted to the exact new unit.");
                }
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                    oldTurnBased;
                Game.Instance.IsPaused = oldPause;
            }
        }

        // A switch command lazily constructs the alternate hand set. The
        // project must provision that set's second distinct projectile only
        // when its own primary javelin is present, and never refill after a
        // disarm/load. These are plans and item observations, not attacks.
        private IEnumerable<int> ReviewSprint29Modes(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldTurnBased = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            try
            {
                Game.Instance.IsPaused = true;
                CreateExpandedSummoningCorrectionHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int i = 0; i < 12; i++) yield return 0;
                UnitEntityData owner = CastExpandedSummoningOwnTier(fixture,
                    "ettin");
                SetExpandedSummoningBrainActive(owner, false);
                for (int i = 0; i < 8; i++) yield return 0;
                var initialMain = owner.Body.PrimaryHand.MaybeWeapon;
                var initialOff = owner.Body.SecondaryHand.MaybeWeapon;
                var initialMarker = owner.Get<UnitPartSummonEttinInitialArms>();
                var flailAttack = new UnitAttack(fixture.Hostile);
                flailAttack.Init(owner);
                var flailPlan = flailAttack.CreateFullAttack();
                bool flails = initialMain?.Blueprint?.name ==
                        "KMG_Summoning_Giant_Sprint29_EttinMainFlail2d6" &&
                    initialOff?.Blueprint?.name ==
                        "KMG_Summoning_Giant_Sprint29_EttinOffFlail2d6" &&
                    !initialMain.HoldInTwoHands &&
                    initialMarker?.FlailSeeded == true &&
                    initialMarker.JavelinSeeded == false &&
                    flailPlan.Count == 4 &&
                    flailPlan.Count(row => ReferenceEquals(row.Hand,
                        owner.Body.PrimaryHand)) == 2 &&
                    flailPlan.Count(row => ReferenceEquals(row.Hand,
                        owner.Body.SecondaryHand)) == 2;
                Sprint25Check("ettin-flail-plan", flails,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["main"] = initialMain?.Blueprint?.name,
                        ["off"] = initialOff?.Blueprint?.name,
                        ["twoHandGrip"] = initialMain?.HoldInTwoHands,
                        ["flailSeeded"] = initialMarker?.FlailSeeded,
                        ["javelinSeeded"] = initialMarker?.JavelinSeeded,
                        ["planCount"] = flailPlan.Count,
                        ["primaryCount"] = flailPlan.Count(row =>
                            ReferenceEquals(row.Hand, owner.Body.PrimaryHand)),
                        ["secondaryCount"] = flailPlan.Count(row =>
                            ReferenceEquals(row.Hand, owner.Body.SecondaryHand)) },
                    "Native flail plan has two ordinary attacks per distinct arm.");
                var toJavelin = new UnitSwitchHandEquipmentSet(1);
                owner.Commands.Run(toJavelin);
                int frames = 0;
                DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                while (!toJavelin.IsFinished && frames++ < 1000 &&
                    DateTime.UtcNow < deadline) yield return 0;
                bool switched = toJavelin.IsFinished &&
                    owner.Body.CurrentHandEquipmentSetIndex == 1;
                Sprint25Check("ettin-javelin-switch", switched,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["finished"] = toJavelin.IsFinished,
                        ["result"] = toJavelin.Result.ToString(),
                        ["active"] = owner.Body.CurrentHandEquipmentSetIndex,
                        ["frames"] = frames },
                    "A native hand-set command selects Ettin's projectile set.");
                if (!switched) yield break;
                var javelinMain = owner.Body.PrimaryHand.MaybeWeapon;
                var javelinOff = owner.Body.SecondaryHand.MaybeWeapon;
                var javelinAttack = new UnitAttack(fixture.Hostile);
                javelinAttack.Init(owner);
                var javelinPlan = javelinAttack.CreateFullAttack();
                var marker = owner.Get<UnitPartSummonEttinInitialArms>();
                bool javelins = javelinMain?.Blueprint?.name ==
                        "KMG_Summoning_Giant_Sprint29_EttinMainJavelin1d8" &&
                    javelinOff?.Blueprint?.name ==
                        "KMG_Summoning_Giant_Sprint29_EttinOffJavelin1d8" &&
                    !ReferenceEquals(javelinMain, javelinOff) &&
                    marker?.JavelinSeeded == true &&
                    javelinPlan.Count == 2 &&
                    javelinPlan.Count(row => ReferenceEquals(row.Hand,
                        owner.Body.PrimaryHand)) == 1 &&
                    javelinPlan.Count(row => ReferenceEquals(row.Hand,
                        owner.Body.SecondaryHand)) == 1;
                Sprint25Check("ettin-javelin-plan", javelins,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["main"] = javelinMain?.Blueprint?.name,
                        ["off"] = javelinOff?.Blueprint?.name,
                        ["javelinSeeded"] = marker?.JavelinSeeded,
                        ["planCount"] = javelinPlan.Count },
                    "Native javelin plan has one attack per distinct arm.");
                var toFlail = new UnitSwitchHandEquipmentSet(0);
                owner.Commands.Run(toFlail);
                frames = 0;
                deadline = DateTime.UtcNow.AddSeconds(20);
                while (!toFlail.IsFinished && frames++ < 1000 &&
                    DateTime.UtcNow < deadline) yield return 0;
                bool returned = toFlail.IsFinished &&
                    owner.Body.CurrentHandEquipmentSetIndex == 0 &&
                    ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon,
                        initialMain) &&
                    ReferenceEquals(owner.Body.SecondaryHand.MaybeWeapon,
                        initialOff);
                Sprint25Check("ettin-flail-return", returned,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["finished"] = toFlail.IsFinished,
                        ["active"] = owner.Body.CurrentHandEquipmentSetIndex,
                        ["sameMain"] = ReferenceEquals(
                            owner.Body.PrimaryHand.MaybeWeapon, initialMain),
                        ["sameOff"] = ReferenceEquals(
                            owner.Body.SecondaryHand.MaybeWeapon, initialOff) },
                    "Returning to flails preserves both exact initial items.");
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                    oldTurnBased;
                Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
