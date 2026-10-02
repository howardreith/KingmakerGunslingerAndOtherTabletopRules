using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint10Tests
    {
        internal const int AppendedLedgerIdentities = 29;
        internal const int StirgeAppendedLedgerIdentities = 13;
        internal const int StirgeRemovalIdentityCount = 1;

        internal static void StirgeAttachRulesBoundDrainAndDetachment()
        {
            Assertions.Equal(8, StirgeAttachPolicy.MaintainGrappleRacialBonus,
                "An attached Stirge has the printed grapple bonus.");
            Assertions.Equal(10, StirgeAttachPolicy.DiseaseChancePercent,
                "One Stirge's blood drain has the printed disease chance.");
            Assertions.Equal(12, StirgeAttachPolicy.FilthFeverFortitudeDc,
                "Native filth fever exposure uses the printed Fortitude DC.");
            Assertions.True(StirgeAttachPolicy.DiseaseExposureSelected(0) &&
                StirgeAttachPolicy.DiseaseExposureSelected(9) &&
                !StirgeAttachPolicy.DiseaseExposureSelected(10) &&
                !StirgeAttachPolicy.DiseaseExposureSelected(99),
                "Exactly ten of the hundred percentile outcomes expose prey.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                StirgeAttachPolicy.DiseaseExposureSelected(-1),
                "Negative disease rolls fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                StirgeAttachPolicy.DiseaseExposureSelected(100),
                "Out-of-range disease rolls fail closed.");
            Assertions.False(StirgeAttachPolicy.ShouldRollDiseaseExposure(0),
                "No actual Constitution loss cannot expose the prey.");
            Assertions.True(StirgeAttachPolicy.ShouldRollDiseaseExposure(1),
                "Every successful one-point drain rolls exposure.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                StirgeAttachPolicy.ShouldRollDiseaseExposure(-1),
                "Negative actual damage fails closed.");
            Assertions.True(StirgeAttachPolicy.MayAttach(true, false, true),
                "A touch hit against a live prey establishes attachment.");
            Assertions.False(StirgeAttachPolicy.MayAttach(false, false, true),
                "A missed touch attack cannot attach.");
            Assertions.False(StirgeAttachPolicy.MayAttach(true, true, true),
                "One Stirge cannot establish a second simultaneous link.");
            Assertions.False(StirgeAttachPolicy.MayAttach(true, false, false),
                "A dead target cannot become a new attachment.");

            int cumulative = 0;
            for (int turn = 1; turn <= 4; turn++)
            {
                Assertions.Equal(1, StirgeAttachPolicy.RequestedDamage(true,
                    true, cumulative),
                    "A live attached prey receives one drain attempt.");
                StirgeDrainStep step = StirgeAttachPolicy.EndTurn(true,
                    true, cumulative, 1);
                Assertions.Equal(1, step.Damage,
                    "Each attached end turn drains exactly one Constitution.");
                Assertions.Equal(turn, step.CumulativeDamage,
                    "The same Stirge's drain tracks its four-point meal.");
                Assertions.Equal(turn == 4, step.Detach,
                    "The Stirge detaches only on reaching four points.");
                cumulative = step.CumulativeDamage;
            }
            StirgeDrainStep immune = StirgeAttachPolicy.EndTurn(true,
                true, 2, 0);
            Assertions.True(immune.Damage == 0 &&
                immune.CumulativeDamage == 2 && !immune.Detach,
                "An immune target cannot advance the four-point meal.");
            StirgeDrainStep dead = StirgeAttachPolicy.EndTurn(true, false,
                2, 0);
            Assertions.True(dead.Detach && dead.Damage == 0 &&
                dead.CumulativeDamage == 2,
                "Prey death releases the Stirge without an extra drain.");
            StirgeDrainStep escaped = StirgeAttachPolicy.EndTurn(false,
                true, 2, 0);
            Assertions.True(!escaped.Detach && escaped.Damage == 0,
                "Escape ends future blood drain.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                StirgeAttachPolicy.EndTurn(true, true, 5, 1),
                "Out-of-range cumulative damage must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                StirgeAttachPolicy.EndTurn(false, true, 2, 1),
                "Detached Stirges cannot claim blood drain.");
        }

        internal static void GiantWaspRegisteredUnderSuppressionAtExactTiers()
        {
            SummonCreatureSpec wasp = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "giant-wasp");
            Assertions.Equal(4, wasp.MonsterTier.Value,
                "Wasp belongs at Summon Monster IV.");
            Assertions.Equal(4, wasp.NaturesAllyTier.Value,
                "Wasp belongs at Nature's Ally IV.");
            Assertions.True(wasp.MonsterTemplated,
                "Summon Monster Wasp keeps the native template policy.");
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            {
                SummonVariantSpec[] variants = ExpandedSummoningCatalog
                    .GenerateVariants(family).Where(value =>
                        value.Creature.Key == wasp.Key).OrderBy(value =>
                            value.ParentTier).ToArray();
                Assertions.Equal(6, variants.Length,
                    "Wasp has one identity at each legal tier in each family.");
                Assertions.Equal(SummonMultiplicity.One, variants[0].Multiplicity,
                    "Wasp's own tier is a single creature.");
                Assertions.Equal(SummonMultiplicity.OneD3, variants[1].Multiplicity,
                    "Wasp's next tier is 1d3.");
                Assertions.True(variants.Skip(2).All(value =>
                    value.Multiplicity == SummonMultiplicity.OneD4PlusOne),
                    "Later Wasp tiers use 1d4+1.");
                Assertions.True(variants.All(value =>
                    SummonVisibilityCatalog.IsPublished(value)),
                    "All Wasp placements publish after their live combat and icon gates.");
            }
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For("giant-wasp");
            Assertions.Equal("Vermin", profile.HitDieClass,
                "Vermin racial hit dice carry Wasp immunities.");
            Assertions.Equal(4, profile.HitDice, "Wasp has four racial hit dice.");
            Assertions.Equal("Large", profile.Size, "Wasp is Large.");
            Assertions.Equal("WaspSting1d8", profile.PrimaryWeapon,
                "Wasp requires its exact sting weapon.");
            Assertions.Equal(60, profile.SpeedFeet,
                "Wasp uses its flying speed on Kingmaker maps.");
            Assertions.True(profile.Facts.Contains("Airborne"),
                "Wasp must navigate as an airborne creature.");
            Assertions.Equal("406c1e1af5400ac4881e330502ccbd9e",
                ExpandedSummoningDonorCatalog.For("giant-wasp").Guid,
                "Wasp must bind against the audited Giant Eagle skeleton.");
            Assertions.True(ExpandedSummoningIdentityCatalog.Build().Any(value =>
                value.Symbol == "KMG.Summoning.Natural.WaspSting1d8" &&
                value.PlannedType == "BlueprintItemWeapon"),
                "Wasp sting has its own append-only blueprint identity.");
            Assertions.Equal(0,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "No placement is suppressed once every sprint has qualified.");
        }

        internal static void StirgePublishesAtAllNineNatureTiers()
        {
            SummonCreatureSpec stirge = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "stirge");
            Assertions.True(!stirge.MonsterTier.HasValue &&
                stirge.NaturesAllyTier == 1 && !stirge.MonsterTemplated,
                "Stirge belongs only to the untemplated Nature's Ally I roster.");
            SummonVariantSpec[] variants = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.NaturesAlly).Where(value =>
                    value.Creature.Key == "stirge").OrderBy(value =>
                        value.ParentTier).ToArray();
            Assertions.Equal(9, variants.Length,
                "Stirge owns one legal placement at each Nature's Ally tier.");
            for (int tier = 1; tier <= 9; tier++)
            {
                SummonVariantSpec variant = variants[tier - 1];
                Assertions.Equal(tier, variant.ParentTier,
                    "Stirge's tier order is append-only and complete.");
                Assertions.Equal(tier == 1 ? SummonMultiplicity.One :
                    tier == 2 ? SummonMultiplicity.OneD3 :
                    SummonMultiplicity.OneD4PlusOne, variant.Multiplicity,
                    "Stirge follows the preserved summon quantity ladder.");
                Assertions.True(SummonVisibilityCatalog.IsPublished(variant),
                    "Qualified Stirge publishes at every Nature's Ally tier.");
            }
            NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                .For("stirge");
            Assertions.True(profile.HitDieClass == "MagicalBeast" &&
                profile.HitDice == 1 && profile.Size == "Tiny" &&
                profile.SpeedFeet == 40 && profile.PrimaryWeapon ==
                "StirgeTouch" && profile.Facts.Contains("Airborne") &&
                profile.Facts.Contains("WeaponFinesse"),
                "The published profile retains Stirge's physical and touch-carrier role.");
            Assertions.Equal("406c1e1af5400ac4881e330502ccbd9e",
                ExpandedSummoningDonorCatalog.For("stirge").Guid,
                "Stirge uses the audited flying donor rig.");
            Assertions.True(ExpandedSummoningIdentityCatalog.Build().Any(value =>
                value.Symbol == "KMG.Summoning.Natural.StirgeTouch" &&
                value.PlannedType == "BlueprintItemWeapon"),
                "The zero-damage touch carrier needs its own stable identity.");
            Assertions.Equal("Stirge", SummonIconCatalog.For("stirge")
                .DisplayName, "Published Stirge keeps its original species art.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            Assertions.True(builder.Contains("ConfigureWeapon(nativeTouch, stirgeTouch, StirgeTouchSymbol, 0,") &&
                builder.Contains("stirgeTouch.IsNonRemovable = true"),
                "The proboscis has zero ordinary damage and cannot become dropped equipment on expiry.");
            Assertions.True(builder.Contains("17451c1327c571641a1345bd31155209") &&
                builder.Contains("nativeTouch.AttackType != AttackType.Touch") &&
                builder.Contains("ConfigureWeapon(nativeTouch"),
                "Stirge must clone the game's verified held-touch weapon, not a bite AC type.");
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            string special = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ExpandedSummoningSpecialCombatComponents.cs"));
            Assertions.True(runtime.Contains("expanded-summoning-stirge-native-touch-attack") &&
                runtime.Contains("roll.AttackType == AttackType.Touch") &&
                runtime.Contains("ordinaryAc > touchAc && roll.TargetAC == touchAc") &&
                runtime.Contains("roll.IsHit && damageAfter == damageBefore"),
                "The guarded combat fixture must demand a real zero-HP touch hit against armored AC controls.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-native-attachment") &&
                runtime.Contains("StirgeHoldComponent.AttachedTarget(stirge)") &&
                runtime.Contains("StirgeHoldComponent.Detach(stirge)") &&
                runtime.Contains("bool targetFree = hostile.Get<Kingmaker.UnitLogic.Parts"),
                "The guarded fixture must inspect the Stirge-only link and free prey state.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-first-blood-drain") &&
                runtime.Contains("liveHold.OnNewRound()") &&
                runtime.Contains("liveHold.CumulativeDamage == 1 && stillAttached"),
                "The first live round must measure actual Constitution loss and persistent attachment.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-native-disease") &&
                runtime.Contains("attach.TryDiseaseExposure(hostile, 1, 0)") &&
                runtime.Contains("attach.TryDiseaseExposure(hostile, 0, 0)") &&
                runtime.Contains("attach.DiseaseCheckedVictimCount") &&
                special.Contains("attach.TryDiseaseExposure(target, actual)") &&
                special.Contains("m_DiseaseCheckedVictims.Contains(target.UniqueId)"),
                "Zero loss cannot consume the one primary-source check per victim and Stirge.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-four-point-detach") &&
                runtime.Contains("for (int round = 2; round <= 4") &&
                runtime.Contains("fourPointDetach = mealExact && automaticCleanup"),
                "The guarded fixture must require four actual drains and automatic attachment release.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-escape-and-transition") &&
                runtime.Contains("ExecuteExpandedSummoningRuntimeAbility(hostile,") &&
                runtime.Contains("SummonGrappleAreaSafeguard.SweepStirge(") &&
                runtime.Contains("escapeAndTransition = reattachedForEscape && failedRemoval") &&
                runtime.Contains("failedMobilityRemoval && successfulMobilityRemoval") &&
                runtime.Contains("mobilityUsedNativeSkill") &&
                runtime.Contains("StirgeRemovalRuleObserver") &&
                runtime.Contains("expectedMobilityCmd") &&
                runtime.Contains("successfulRemoval"),
                "The prey's standard action must show failed and successful CMB/Mobility removal with native rule observation, then area cleanup.");
            Assertions.True(runtime.Contains("attachedMaintainCmb - baseMaintainCmb ==") &&
                runtime.Contains("stirge.Descriptor.Stats.BaseAttackBonus.BaseValue = babBefore;") &&
                runtime.Contains("StirgeAttachPolicy.MaintainGrappleRacialBonus") &&
                runtime.Contains("attachmentEstablished = sessionLink && holderBuff && removeIcon &&") &&
                runtime.Contains("targetFree && losesDexterity && maintainBonus"),
                "A live owner-side grapple calculation must retain Stirge's printed +8 maintain bonus while attached.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-quantity-freedom") &&
                runtime.Contains("ExerciseExpandedSummoningStirgeQuantityFreedom") &&
                runtime.Contains("\"stirge\", 3, SummonMultiplicity.OneD4PlusOne") &&
                runtime.Contains("RemoveExpandedSummoningAppearanceBuffs(pony);") &&
                runtime.Contains("Vector3 firstContact = hostile.Position +") &&
                runtime.Contains("first.Position = firstContact;") &&
                runtime.Contains("hostile.Position = movedPreyPosition;") &&
                runtime.Contains("hostile.View.transform.position = movedPreyPosition;") &&
                runtime.Contains("float targetHorizontalDistance = Vector2.Distance(") &&
                runtime.Contains("StirgeHoldComponent.FollowAttached(first)") &&
                runtime.Contains("new RuleAttackWithWeapon(hostile, first, weapon, 0)") &&
                runtime.Contains("otherIntact"),
                "Quantity Stirges must keep distinct links while an appearance-ready prey takes an ordinary movement step and kills its attacker.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-dismissal-release") &&
                runtime.Contains("ExerciseExpandedSummoningStirgeDismissal") &&
                runtime.Contains("CleanupExpandedSummoningUnit(stirge);") &&
                runtime.Contains("Game.Instance.EntityDestroyer.Tick();") &&
                runtime.Contains("return attached && stirge.Destroyed && victimFree"),
                "The guarded fixture must destroy an attached summon and verify its victim is free.");
            Assertions.True(runtime.Contains("expanded-summoning-stirge-prey-death-release") &&
                runtime.Contains("ExerciseExpandedSummoningStirgePreyDeath") &&
                runtime.Contains("hostile.Descriptor.State.IsDead") &&
                runtime.Contains("new Kingmaker.Controllers.Units.UnitLifeController()") &&
                runtime.Contains("expanded-summoning-stirge-timed-expiry-release") &&
                runtime.Contains("ExerciseExpandedSummoningStirgeExpiry") &&
                runtime.Contains("marker.EndTime +") &&
                runtime.Contains("stirge.Descriptor.Buffs.Tick()") &&
                special.Contains(".SystemMechanics.SummonedUnitBuff) == null") &&
                special.Contains("Detach(owner);"),
                "The guarded fixture must drive native death and timed-marker expiry, and the holder must release after that marker expires.");
            string correction = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.ExpandedSummoningCorrection.cs"));
            Assertions.True(runtime.Contains("new[] { \"NaturesAlly\", \"stirge\", \"1\" }") &&
                runtime.Contains("expanded-summoning-stirge-attached-save-load") &&
                runtime.Contains("PrepareExpandedSummoningPersistentStirge(units,") &&
                runtime.Contains("VerifyExpandedSummoningReloadedStirge(units,") &&
                correction.Contains("attach.TryAttach(pony, touch, true)") &&
                correction.Contains("baselineCantMove") &&
                correction.Contains("baselineCantAct") &&
                correction.Contains("!holderPart && !victimPart && !holderBuff &&") &&
                correction.Contains("!orphanRemovalAction"),
                "The exact working-save fixture must attach before saving and clear link, hold and action on reload.");
            string specialBuilder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            Assertions.True(special.Contains("StirgeAttachPolicy.MayAttach") &&
                special.Contains("StirgeAttachPolicy.EndTurn") &&
                special.Contains("new RuleDealStatDamage(owner, target,") &&
                special.Contains("link.Attach(target)") &&
                special.Contains("private UnitEntityData m_Target;") &&
                special.Contains("StirgeHoldComponent.AttachedTarget(owner)") &&
                specialBuilder.Contains("ConfigureStirgeAttachment(library, bySymbol)") &&
                specialBuilder.Contains("9545a5550d89feb47a84edaeb4e63d0b") &&
                specialBuilder.Contains("0f775c7d5d8b6494197e1ce937754482") &&
                specialBuilder.Contains("UnitCondition.LoseDexterityToAC"),
                "The published unit must own a session-only attachment and bounded actual-Constitution drain.");
            string stirgeBlock = special.Substring(
                special.IndexOf("public sealed class StirgeAttachComponent", StringComparison.Ordinal),
                special.IndexOf("internal static class StirgeNativeTouchAttachPatch", StringComparison.Ordinal) -
                special.IndexOf("public sealed class StirgeAttachComponent", StringComparison.Ordinal));
            Assertions.False(stirgeBlock.Contains("UnitPartGrappleTarget") ||
                stirgeBlock.Contains("UnitPartGrappleInitiator") ||
                stirgeBlock.Contains("GrappledBuff"),
                "Stirge Attach must never initialize native grapple state on its prey.");
            Assertions.True(special.Contains("class StirgePreyTranslocationPatch") &&
                special.Contains("method.Name == \"Translocate\"") &&
                special.Contains("DetachFromTranslocatedTarget(__instance)") &&
                special.Contains("ReferenceEquals(AttachedTarget(unit), target)"),
                "Any native prey translocation must release only that prey's session links.");
            string iconBuilder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningIconBuilder.cs"));
            Assertions.True(iconBuilder.Contains(
                    "Set(bySymbol, \"KMG.Summoning.Special.Stirge.Remove\",") &&
                iconBuilder.Contains("ExpandedSummoningProjectIcons.Require(\"remove-stirge\")") &&
                SummonIconCatalog.For("remove-stirge").Key !=
                    SummonIconCatalog.For("stirge").Key,
                "Remove Stirge must have its own original action icon, distinct from the creature portrait.");
            Assertions.Equal(0,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Sprint 10 remains published and no later sprint is withheld.");
        }

        internal static void GiantWaspPoisonTracksConstitutionAndTabletopExposure()
        {
            Assertions.Equal(18, GiantWaspPoisonPolicy.DifficultyClass(4),
                "The baseline 4-HD/Con 18 Wasp poison DC is 18.");
            Assertions.Equal(16, GiantWaspPoisonPolicy.DifficultyClass(2),
                "Constitution loss must lower the poison DC by the same amount.");
            Assertions.Equal(20, GiantWaspPoisonPolicy.DifficultyClass(6),
                "Constitution gains must raise the poison DC by the same amount.");
            Assertions.Equal(6, GiantWaspPoisonPolicy.Exposures,
                "Wasp poison has six total exposures, including the initial hit.");
            Assertions.Equal(1, GiantWaspPoisonPolicy.SavesToCure,
                "One successful later save cures Wasp venom.");
            Assertions.True(ExpandedSummoningNaturalProfiles.For("giant-wasp")
                .Facts.Contains("WaspPoison"),
                "The Wasp unit must carry its dedicated poison feature.");
            SummoningIdentitySpec[] poison = ExpandedSummoningIdentityCatalog
                .Build().Where(value => value.Symbol ==
                    "KMG.Summoning.Natural.GiantWasp.Poison" ||
                    value.Symbol ==
                    "KMG.Summoning.Natural.GiantWasp.Venom").ToArray();
            Assertions.Equal(2, poison.Length,
                "The poison feature and saved venom buff have separate identities.");
            Assertions.True(poison.Any(value => value.PlannedType ==
                    "BlueprintFeature") && poison.Any(value =>
                    value.PlannedType == "BlueprintBuff"),
                "Poison identities must preserve their blueprint types.");
        }

        internal static void PublishedWaspHasOwnIconConsumers()
        {
            Assertions.Equal("Giant Wasp",
                SummonIconCatalog.For("giant-wasp").DisplayName,
                "Published Wasp must have its own creature concept.");
            JObject manifest = JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "assets-source", "original-icons",
                "expanded-summoning", "icon-manifest.json")));
            JToken row = ((JArray)manifest["icons"]).Single(value =>
                (string)value["key"] == "giant-wasp");
            string[] consumers = ((JArray)row["blueprintSymbols"])
                .Select(value => (string)value).ToArray();
            Assertions.Equal(26, consumers.Length,
                "Wasp unit, type and 24 ability/template consumers share one icon.");
            Assertions.True(consumers.Contains(
                    "KMG.Summoning.Natural.GiantWasp.UnitType") &&
                consumers.Contains("KMG.Summoning.Unit.GiantWasp"),
                "The inspectable Wasp type and unit must be recorded.");
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runtime.Contains(
                    ".Where(SummonIconCatalog.IsPublishedSomewhere)") &&
                runtime.Contains("creatureIcons.Count &&"),
                "The live menu icon audit must check only published creatures.");
        }

        internal static void Sprint10CreatureReviewAcceptsPublishedStirge()
        {
            string root = Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "RuntimeTesting");
            string review = File.ReadAllText(Path.Combine(root,
                "RuntimeTestRunner.ExpandedSummoningCreatureReview.cs"));
            string movement = File.ReadAllText(Path.Combine(root,
                "RuntimeTestRunner.PteranodonReview.cs"));
            Assertions.True(!review.Contains("suppressedSprint10Candidate =") &&
                review.Contains("!SummonVisibilityCatalog.IsPublished(variant)") &&
                !review.Contains("suppressedSprint11Candidate") &&
                review.Contains("MotionReviewTravelValid") &&
                review.Contains("MotionReviewDoorwayValid") &&
                movement.Contains("GiantWaspBlueprintName") &&
                movement.Contains("PrepareSprint9FlightMovement(unit)"),
                "Published Sprint 10 flyers use the ordinary guarded review and native flight travel checks.");
            Assertions.True(review.Contains("CaptureWaspWithoutAuxiliaryRenderer") &&
                review.Contains("giant-wasp-review-summoned-attack-no-auxiliary.png") &&
                review.Contains("finally") &&
                review.Contains("renderer.enabled = true"),
                "The isolated Wasp frame must restore every temporarily hidden auxiliary renderer.");
            Assertions.Equal(0,
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "The review scenario must tolerate an empty hidden set.");
        }

        internal static void WaspQuantityCoveragePublishesAllLegalVariants()
        {
            SummonVariantSpec[] crowd = new[] { SummonFamily.Monster,
                    SummonFamily.NaturesAlly }
                .SelectMany(ExpandedSummoningCatalog.GenerateVariants)
                .Where(value => value.Creature.Key == "giant-wasp" &&
                    value.Multiplicity != SummonMultiplicity.One)
                .GroupBy(value => new { value.Family, value.Multiplicity })
                .Select(group => group.OrderBy(value => value.ParentTier).First())
                .ToArray();
            Assertions.Equal(4, crowd.Length,
                "Both Wasp families must offer 1d3 and 1d4+1 for the guarded quantity fixture.");
            Assertions.True(crowd.All(value =>
                    SummonVisibilityCatalog.IsPublished(value)),
                "Qualified Wasp quantity variants must appear in both menus.");
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runtime.Contains("expanded-summoning-giant-wasp-quantity") &&
                runtime.Contains(".Concat(waspCrowd).Concat(ungulateExtra).ToArray()") &&
                runtime.Contains("waspCrowdLegal == 4"),
                "The guarded cast loop must exercise all four private Wasp quantity variants.");
        }

        internal static void WaspVerminProbeUsesTheNativeTypeFeature()
        {
            string runtime = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runtime.Contains(
                    "09478937695300944a179530664e42ec") &&
                runtime.Replace("\r\n", "\n").Contains(
                    "blueprint.name ==\n                    \"KMG_Summoning_Natural_GiantWasp_Poison\"") &&
                runtime.Contains("wasp.Descriptor.HasFact(verminType)") &&
                runtime.Contains("expanded-summoning-giant-wasp-vermin-immunity") &&
                runtime.Contains("SpellImmunityToSpellDescriptor") &&
                runtime.Contains("Rulebook.Trigger(waspRule)") &&
                runtime.Contains("Rulebook.Trigger(humanRule)") &&
                runtime.Contains("waspRule.Immunity && !waspRule.CanApply") &&
                runtime.Contains("!humanRule.Immunity && humanRule.CanApply") &&
                runtime.Contains("RemoveFact(onWasp)") &&
                runtime.Contains("RemoveFact(onHuman)"),
                "The guarded Wasp immunity check must compare native mind-affecting buff outcomes and clean both units.");
        }

        internal static void WaspHasAnOwnedSpeciesMarker()
        {
            SummoningIdentitySpec marker = ExpandedSummoningIdentityCatalog
                .Build().Single(value => value.Symbol ==
                    "KMG.Summoning.Natural.GiantWasp.UnitType");
            Assertions.Equal("BlueprintUnitType", marker.PlannedType,
                "Wasp must have an append-only inspectable species identity.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            Assertions.True(builder.Contains("unit.Type = Require<BlueprintUnitType>") &&
                builder.Contains("type.KnowledgeStat = StatType.SkillLoreNature") &&
                builder.Contains("type.Image = null"),
                "Wasp must not inherit the Eagle donor's type or image.");
        }

        internal static void HiddenWaspUsesBoundedNativeFlightCombatReview()
        {
            string root = Environment.CurrentDirectory;
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "SummonSameTurnActivationScenario.cs"));
            string request = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRequest.cs"));
            string launcher = File.ReadAllText(Path.Combine(root, "scripts",
                "Invoke-KingmakerRuntimeTest.ps1"));
            string automation = File.ReadAllText(Path.Combine(root, "scripts",
                "RuntimeAutomation.Common.ps1"));
            Assertions.True(scenario.Contains("SummonMonsterFourGuid") &&
                scenario.Contains("_flightCreature == \"giant-wasp\" ? 2 : 1") &&
                scenario.Contains("new[] { \"Tail\" }") &&
                scenario.Contains("sprint10-flight-") &&
                scenario.Contains("_waspImpactCaptures < 2") &&
                scenario.Contains("WriteExpandedSummoningPartyCameraCapture(") &&
                scenario.Contains("WriteExpandedSummoningOverheadStrikeCapture(") &&
                scenario.Contains("ProbeWaspTailAim(attack, mesh, targetBounds") &&
                scenario.Contains("tail.rotation = native;") &&
                scenario.Contains("class WaspTailAimFrameProbe : MonoBehaviour") &&
                scenario.Contains("yield return new WaitForEndOfFrame()") &&
                scenario.Contains("private void OnDisable()") &&
                // Closed, named allowlist; Sprint 12's Dire Rat was added to it
                // so a ground creature can prove both combat modes.
                request.Contains("\"eagle\", \"dire-bat\", \"giant-wasp\", \"stirge\",") &&
                request.Contains("\"dire-rat\" }.Contains(") &&
                launcher.Contains("@('eagle', 'dire-bat', 'giant-wasp', 'stirge', 'dire-rat')") &&
                automation.Contains("@('eagle', 'dire-bat', 'giant-wasp', 'stirge', 'dire-rat')"),
                "Only named published summons may enter the guarded combat fixture; Wasp retains its two exact hostile strikes and stinger geometry.");
        }

        internal static void HiddenStirgeHasBoundedNativeAttackVisualReview()
        {
            string root = Environment.CurrentDirectory;
            string scenario = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "SummonSameTurnActivationScenario.cs"));
            string request = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRequest.cs"));
            Assertions.True(scenario.Contains("SummonNaturesAllyOneGuid") &&
                scenario.Contains("GenerateVariants(SummonFamily.NaturesAlly)") &&
                scenario.Contains("value.Creature.Key == \"stirge\"") &&
                scenario.Contains("sprint10-stirge-native-attack-overhead") &&
                scenario.Contains("stirge-native-attack-overhead.png") &&
                scenario.Contains("!_stirgeAttackCaptured && attack.AttackRoll != null") &&
                scenario.Contains("ReferenceEquals(StirgeHoldComponent.AttachedTarget(") &&
                scenario.Contains("stirge-visual-fixture-bab=") &&
                scenario.Contains("BaseAttackBonus.BaseValue = 100") &&
                scenario.Contains(";touch=True;hit=True;attached=True") &&
                scenario.Contains("WriteExpandedSummoningOverheadStrikeCapture(") &&
                request.Contains("\"giant-wasp\", \"stirge\""),
                "The guarded Stirge fixture must cast its own SNA I variant, correlate a native touch hit and session attachment, and capture the live pose.");
            string special = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSpecialCombatComponents.cs"));
            Assertions.True(special.Contains("class StirgeNativeTouchAttachPatch") &&
                special.Contains("attach.AttachAfterNativeRule(__instance)") &&
                special.Contains("ReferenceEquals(attack.Weapon.Blueprint, TouchWeapon)") &&
                special.Contains("ReferenceEquals(StirgeHoldComponent.AttachedTarget(attack.Initiator),") &&
                scenario.Contains("attach.NativeFallbackCalls"),
                "Only an exact, otherwise unattached Stirge touch hit may retry the native attach rule after UnitAttack.");
            string pose = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning", "StirgeVisualTouch.cs"));
            string view = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            string project = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "KingmakerGunslinger.csproj"));
            Assertions.True(project.Contains("Summoning\\StirgeVisualTouch.cs") &&
                view.Contains("attachment.StirgeTouch.Configure(view, donor)") &&
                view.Contains("DestroyImmediate(attachment.StirgeTouch)") &&
                pose.Contains("class StirgeVisualTouch : MonoBehaviour") &&
                pose.Contains("StirgeHoldComponent.AttachedTarget(") &&
                pose.Contains("MaximumApproachMeters = 2.5f") &&
                pose.Contains("SurfaceClearanceMeters = 0.05f") &&
                pose.Contains("Vector3 forward = -_root.forward") &&
                pose.Contains("private void RestoreNative()") &&
                pose.Contains("private void OnDestroy()") &&
                scenario.Contains("sprint10-stirge-visible-contact") &&
                scenario.Contains("value >= 0f && value <= 0.20f") &&
                scenario.Contains("_stirgeTipInside.All(value => !value)") &&
                scenario.Contains("_stirgeForwardDots.All(value => value >= 0.80f)"),
                "Stirge's exact skinned view must use a bounded target-facing touch pose, retain it only while attached, restore it on teardown, and prove baked contact without clipping.");
            string disposable = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs"));
            string viewAudit = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.PteranodonAttachedView.cs"));
            int waspAudit = viewAudit.IndexOf("private static bool IsGiantWaspAttached",
                StringComparison.Ordinal);
            int stirgeAudit = viewAudit.IndexOf("private static bool IsStirgeAttached",
                StringComparison.Ordinal);
            Assertions.True(disposable.Contains("_giantWaspVisualChecked + _stirgeVisualChecked") &&
                disposable.Contains("expanded-summoning-stirge-visual-attached") &&
                disposable.Contains("IsStirgeAttached(renderers)") &&
                waspAudit >= 0 && stirgeAudit > waspAudit &&
                viewAudit.Substring(waspAudit, stirgeAudit - waspAudit)
                    .Contains("(Instance);bones=16;") &&
                viewAudit.Substring(stirgeAudit).Contains("(Instance);bones=15;"),
                "The full disposable roster must account for Stirge's view in the shared lifecycle count and verify its renderer separately.");
            string creatureReview = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningCreatureReview.cs"));
            Assertions.True(creatureReview.Contains(
                    "expanded-summoning-stirge-attached-prey-movement") &&
                creatureReview.Contains("new UnitMoveTo(_stirgePreyDestination, 0.5f)") &&
                creatureReview.Contains("prey.Commands.Run(move)") &&
                creatureReview.Contains("agent.Stop()") &&
                creatureReview.Contains("_stirgePreyMoveAccepted &&") &&
                creatureReview.Contains("_stirgePreyInitialGap - _stirgePreyMinGap >= 1f") &&
                creatureReview.Contains("_stirgePreyTravel >= 1f") &&
                creatureReview.Contains("StirgeHoldComponent.Detach(stirge)") &&
                creatureReview.Contains("PlaceExpandedSummoningUnit(prey,") &&
                creatureReview.Contains("shortTranslocationDetached=") &&
                creatureReview.Contains("translocationDetached && restored") &&
                creatureReview.Contains("stirge-attached-moving-prey.png"),
                "Attached prey must receive native movement, visible follow and short native translocation cleanup with request-local restoration.");
        }

        internal static void GiantWaspOriginalVisualUsesAuditedInstanceBinding()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "flying-animals");
            string path = Path.Combine(directory, "giant-wasp-mesh.json");
            JObject mesh = JObject.Parse(File.ReadAllText(path));
            Assertions.Equal(2, (int)mesh["schemaVersion"],
                "Wasp must use the shared skinned-mesh schema.");
            Assertions.True(((string)mesh["space"]).Contains("donor renderer local"),
                "Wasp geometry must bind in the donor's measured local frame.");
            int vertices = (int)mesh["vertexCount"];
            int triangles = (int)mesh["triangleCount"];
            Assertions.True(vertices >= 500 && triangles >= 500,
                "Wasp must carry a body, stinger, six legs and four wings.");
            byte[] payload = Convert.FromBase64String((string)mesh["data"]);
            Assertions.Equal(vertices * 64 + triangles * 12, payload.Length,
                "Wasp vertex, triangle and bone-weight payload must be complete.");
            string[] bones = ((JArray)mesh["bones"])
                .Select(value => (string)value).ToArray();
            Assertions.Equal(bones.Length,
                bones.Distinct(StringComparer.Ordinal).Count(),
                "Every Wasp weight must have one unambiguous bone name.");
            Assertions.True(bones.Contains("L_Arm_Upper") &&
                bones.Contains("R_Arm_Upper") && bones.Contains("Tail"),
                "Wing and stinger geometry must follow animated donor bones.");
            JObject albedo = (JObject)mesh["albedo"];
            Assertions.Equal("giant-wasp-albedo.png", (string)albedo["file"],
                "Wasp mesh must name its own adjacent painting.");
            using (var sha = SHA256.Create())
            {
                string actual = string.Concat(sha.ComputeHash(File.ReadAllBytes(
                    Path.Combine(directory, (string)albedo["file"])))
                    .Select(value => value.ToString("x2")));
                Assertions.Equal((string)albedo["sha256"], actual,
                    "Wasp painting must match the exact audited mesh manifest.");
            }
            Assertions.True(File.Exists(Path.Combine(root, "assets-source",
                "original-models", "flying-animals", "generate_giant_wasp.py")) &&
                File.Exists(Path.Combine(root, "assets-source",
                    "original-models", "flying-animals",
                    "paint_giant_wasp_albedo.py")),
                "Wasp's original editable geometry and painting must remain available.");
            string view = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            Assertions.True(view.Contains("TryGetGiantWaspVisual") &&
                view.Contains("GiantWaspVisualName") &&
                view.Contains("TryResolveDonorBinding(donor, boneNames") &&
                view.Contains("donor.sharedMesh = mesh") &&
                view.Contains("donor.sharedMaterials = new[] { material }") &&
                view.Contains("Revert(attachment)"),
                "Wasp must share the validated per-instance swap and rollback.");
            string stingPose = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "GiantWaspVisualSting.cs"));
            Assertions.True(view.Contains("AddComponent<GiantWaspVisualSting>()") &&
                view.Contains("attachment.WaspSting.Configure(view, donor)") &&
                view.Contains("Destroy(attachment.WaspSting)") &&
                view.Contains("ReleasePhase2View(UnitEntityView view)") &&
                view.Contains("DestroyImmediate(attachment.Mesh)") &&
                stingPose.Contains("MaximumApproachMeters = 0.25f") &&
                stingPose.Contains("owner.Blueprint.name != ExpandedSummoningPteranodonViewPatch") &&
                stingPose.Contains("RestoreNative();") &&
                stingPose.Contains("typeof(RuleAttackWithWeapon), \"OnTrigger\"") &&
                stingPose.Contains("KMG_Summoning_Natural_WaspSting1d8") &&
                stingPose.Contains("pose.enabled = false") &&
                stingPose.Contains("Destroy(_baked)") &&
                stingPose.Contains("_gapAfter = Vector3.Distance") &&
                stingPose.Contains("_bakedGapAfter = Vector3.Distance"),
                "The Wasp's native strike must drive a bounded instance-only Tail pose and restore it on teardown.");
            string loader = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Assets", "PteranodonAssetRuntime.cs"));
            Assertions.True(loader.Contains(
                "assets/flying-animals/giant-wasp-mesh.json") &&
                loader.Contains("ConfigureGiantWasp(context)") &&
                loader.Contains("BuildMesh(File.ReadAllText(path)") &&
                loader.Contains("LoadAlbedo(Path.GetDirectoryName(path)"),
                "Wasp must load through the package's audited mesh and painting parser.");
            string audit = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            Assertions.True(audit.Contains("giant-wasp-original-asset-loader") &&
                audit.Contains("PteranodonAssetRuntime.GiantWaspStatus == \"visual:published\""),
                "The guarded audit must fail if the packaged Wasp mesh or painting is rejected in game.");
        }

        internal static void StirgeOriginalVisualIsBoundAndPackaged()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "flying-animals");
            JObject mesh = JObject.Parse(File.ReadAllText(Path.Combine(
                directory, "stirge-mesh.json")));
            Assertions.Equal(2, (int)mesh["schemaVersion"],
                "Stirge uses the audited skinned-mesh format.");
            int vertices = (int)mesh["vertexCount"];
            int triangles = (int)mesh["triangleCount"];
            Assertions.True(vertices >= 500 && triangles >= 500,
                "Stirge carries a body, six legs, proboscis and four wings.");
            byte[] payload = Convert.FromBase64String((string)mesh["data"]);
            Assertions.Equal(vertices * 64 + triangles * 12, payload.Length,
                "Stirge mesh payload is complete.");
            string[] bones = ((JArray)mesh["bones"])
                .Select(value => (string)value).ToArray();
            Assertions.True(bones.Contains("Head") &&
                bones.Contains("L_Arm_Upper") &&
                bones.Contains("R_Arm_Upper") &&
                bones.Distinct(StringComparer.Ordinal).Count() == bones.Length,
                "Stirge's proboscis and four wings have unambiguous bindings.");
            JObject albedo = (JObject)mesh["albedo"];
            Assertions.Equal("stirge-albedo.png", (string)albedo["file"],
                "Stirge uses its own adjacent painting.");
            using (var sha = SHA256.Create())
            {
                string actual = string.Concat(sha.ComputeHash(
                    File.ReadAllBytes(Path.Combine(directory,
                        (string)albedo["file"])))
                    .Select(value => value.ToString("x2")));
                Assertions.Equal((string)albedo["sha256"], actual,
                    "Stirge albedo is the painting pinned by its mesh.");
            }
            float scale;
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier(
                "KMG_Summoning_Unit_Stirge", out scale) && scale == 0.25f,
                "The Tiny Stirge uses a view-only scale.");
            string view = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            string loader = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Assets",
                "PteranodonAssetRuntime.cs"));
            Assertions.True(view.Contains("TryGetStirgeVisual") &&
                view.Contains("StirgeVisualName") &&
                view.Contains("ReleasePhase2View(UnitEntityView view)") &&
                loader.Contains("ConfigureStirge(context)") &&
                loader.Contains("stirge-mesh.json"),
                "Stirge uses the validated per-view swap and teardown.");
            string audit = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            Assertions.True(audit.Contains("stirge-original-asset-loader") &&
                audit.Contains("PteranodonAssetRuntime.StirgeStatus == \"visual:published\""),
                "Guarded startup must reject a missing Stirge mesh or painting.");
            Assertions.True(File.Exists(Path.Combine(root, "assets-source",
                "original-models", "flying-animals", "generate_stirge.py")) &&
                File.Exists(Path.Combine(root, "assets-source",
                    "original-models", "flying-animals",
                    "paint_stirge_albedo.py")),
                "Stirge retains its editable original geometry and painting.");
        }

        internal static void NativeFlyingVerminSurveyStaysMetadataOnly()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            Assertions.True(source.Contains("\"wasp\", \"stirge\"") &&
                source.Contains("\"mosquito\"") &&
                source.Contains("\"beetle\"") &&
                source.Contains("\"mantis\"") &&
                source.Contains("56ec8788092b6314e8f3c1c502e8433f") &&
                source.Contains("\"blood\"") &&
                source.Contains("\"attach\"") &&
                source.Contains("\"drain\"") &&
                source.Contains("GetAllBlueprints().Where(value => value != null)") &&
                source.Contains("units.Add(DescribeNativeUnit(unit))") &&
                source.Contains("native-donor-audit.json") &&
                !source.Contains("AssetBundle.LoadFromFile") &&
                !source.Contains("Texture2D.EncodeToPNG"),
                "Sprint 10 intake must inventory installed native metadata for both flying vermin and their signature seams without exporting game art.");
        }
    }
}
