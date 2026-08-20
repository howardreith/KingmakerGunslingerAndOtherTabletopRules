using System;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Misfires;
using Kingmaker.UI.Selection;
using KingmakerGunslinger.Actions;

namespace KingmakerGunslinger.Development
{
    /// <summary>
    /// Exception-contained command surface for manual development controls.
    /// No operation runs automatically, and no operation is available before the
    /// blueprint lifecycle has published all required content.
    /// </summary>
    internal static class DevelopmentControls
    {
        internal static DevelopmentActionResult DescribeFirearmAudio()
        { return DevelopmentActionResult.Success(Audio.FirearmSoundRuntime.Describe()); }
        internal static DevelopmentActionResult RetryFirearmAudio()
        { return DevelopmentActionResult.Success(Audio.FirearmSoundRuntime.RetryConfigurationForDevelopment()); }

        internal static DevelopmentActionResult PreviewGlobalPistolAudio()
        {
            Audio.FirearmSoundPostResult result=Audio.FirearmSoundRuntime.TryPostGlobalPistolPreview();
            return result.Accepted ? DevelopmentActionResult.Success("Global non-spatial Wwise preview accepted; playingId="+result.PlayingId+".") : DevelopmentActionResult.Failure(result.Fault+" "+Audio.FirearmSoundRuntime.Describe());
        }

        internal static DevelopmentActionResult PreviewSelectedFirearmAudio()
        {
            SelectionManager selection=SelectionManager.Instance;
            Kingmaker.EntitySystem.Entities.UnitEntityData unit=selection==null?null:selection.GetSingleSelectedUnit();
            if(unit==null) return DevelopmentActionResult.Failure("Select exactly one unit with a supported equipped firearm.");
            ExactEquippedFirearmContext firearm; string reason;
            if(!ExactEquippedFirearmResolver.TryResolve(unit.Descriptor,out firearm,out reason)) return DevelopmentActionResult.Failure(reason);
            Audio.FirearmSoundPostResult result=Audio.FirearmSoundRuntime.TryPostCommittedDischarge(firearm.Definition.Kind,unit,"development-selected-preview");
            return result.Accepted?DevelopmentActionResult.Success("Selected-unit Wwise preview accepted; event="+result.EventName+";playingId="+result.PlayingId+"."):DevelopmentActionResult.Failure(result.Fault);
        }
        internal static DevelopmentActionResult GrantFirearmProficiency()
        {
            return Execute("grant-proficiency", bridge => bridge.GrantFirearmProficiency());
        }

        internal static DevelopmentActionResult DescribeRareFirearmCatalog()
        { return Execute("rare-firearm-catalog", bridge => bridge.DescribeRareFirearmCatalog()); }
        internal static DevelopmentActionResult AddRareFirearmSet()
        { return Execute("rare-firearm-add-set", bridge => bridge.AddRareFirearmSet()); }
        internal static DevelopmentActionResult AddRareFirearm(int index)
        { return Execute("rare-firearm-add-" + index, bridge => bridge.AddRareFirearm(index)); }
        internal static DevelopmentActionResult DescribeRareFirearmAcquisition()
        { return Execute("rare-firearm-acquisition-audit", bridge => bridge.DescribeRareFirearmAcquisition()); }

        internal static DevelopmentActionResult DescribeElvenBranchedSpearCatalog()
        { return Execute("elven-branched-spear-catalog", bridge => bridge.DescribeElvenBranchedSpearCatalog()); }
        internal static DevelopmentActionResult AddElvenBranchedSpearSet()
        { return Execute("elven-branched-spear-add-set", bridge => bridge.AddElvenBranchedSpearSet()); }
        internal static DevelopmentActionResult AddElvenBranchedSpear(int index)
        { return Execute("elven-branched-spear-add-" + index, bridge => bridge.AddElvenBranchedSpear(index)); }

        internal static DevelopmentActionResult DescribeEasternWeaponCatalog()
        { return Execute("eastern-weapons-catalog", bridge => bridge.DescribeEasternWeaponCatalog()); }
        internal static DevelopmentActionResult DescribeBorderSentinelAcquisition()
        { return Execute("border-sentinel-acquisition-audit", bridge => bridge.DescribeBorderSentinelAcquisition()); }
        internal static DevelopmentActionResult AddEasternWeaponSet()
        { return Execute("eastern-weapons-add-all", bridge => bridge.AddEasternWeaponSet()); }
        internal static DevelopmentActionResult AddWakizashiPath()
        { return Execute("eastern-weapons-add-wakizashi-path", bridge => bridge.AddWakizashiPath()); }
        internal static DevelopmentActionResult AddKatanaPath()
        { return Execute("eastern-weapons-add-katana-path", bridge => bridge.AddKatanaPath()); }
        internal static DevelopmentActionResult AddNodachiPath()
        { return Execute("eastern-weapons-add-nodachi-path", bridge => bridge.AddNodachiPath()); }
        internal static DevelopmentActionResult AddEasternWeapon(int index)
        { return Execute("eastern-weapons-add-" + index, bridge => bridge.AddEasternWeapon(index)); }

        internal static DevelopmentActionResult DescribeReloadReadiness()
        {
            return Execute(
                "describe-reload-readiness",
                bridge => bridge.DescribeReloadReadiness());
        }

        internal static DevelopmentActionResult ReloadEquippedTestMusketNowForDebug()
        {
            return Execute(
                "reload-equipped-test-musket-now",
                bridge => bridge.ReloadEquippedTestMusketNowForDebug());
        }

        internal static DevelopmentActionResult DescribeOverhaulReadiness()
        {
            return Execute(
                "describe-overhaul-readiness",
                bridge => bridge.DescribeOverhaulReadiness());
        }

        internal static DevelopmentActionResult OverhaulEquippedTestMusketNowForDebug()
        {
            return Execute(
                "overhaul-equipped-test-musket-now",
                bridge => bridge.OverhaulEquippedTestMusketNowForDebug());
        }

        internal static DevelopmentActionResult DescribeRepairReadiness()
        {
            return Execute(
                "describe-repair-readiness",
                bridge => bridge.DescribeRepairReadiness());
        }

        internal static DevelopmentActionResult RepairEquippedTestMusketNowForDebug()
        {
            return Execute(
                "repair-equipped-test-musket-now",
                bridge => bridge.RepairEquippedTestMusketNowForDebug());
        }

        internal static DevelopmentActionResult PrepareMaintenanceQualificationFixture()
        {
            return Execute(
                "prepare-maintenance-qualification-fixture",
                bridge => bridge.PrepareMaintenanceQualificationFixture());
        }

        internal static DevelopmentActionResult RunMaintenanceQualificationImmediately()
        {
            return Execute(
                "run-maintenance-qualification-immediately",
                bridge => bridge.RunMaintenanceQualificationImmediately());
        }

        internal static DevelopmentActionResult DescribeMaintenanceQualification()
        {
            return Execute(
                "describe-maintenance-qualification",
                bridge => bridge.DescribeMaintenanceQualification());
        }

        internal static DevelopmentActionResult ResetMaintenanceQualification()
        {
            return Execute(
                "reset-maintenance-qualification",
                bridge => bridge.ResetMaintenanceQualification());
        }

        internal static DevelopmentActionResult AddFirearmRepairKits()
        {
            return Execute(
                "add-firearm-repair-kits",
                bridge => bridge.AddFirearmRepairKits(5));
        }

        internal static DevelopmentActionResult AddOneFirearmRepairKit()
        {
            return Execute(
                "add-one-firearm-repair-kit",
                bridge => bridge.AddFirearmRepairKits(1));
        }

        internal static DevelopmentActionResult DescribeFirearmRepairKits()
        {
            return Execute(
                "describe-firearm-repair-kits",
                bridge => bridge.DescribeFirearmRepairKits());
        }

        internal static DevelopmentActionResult RemoveAllFirearmRepairKits()
        {
            return Execute(
                "remove-all-firearm-repair-kits",
                bridge => bridge.RemoveAllFirearmRepairKits());
        }

        internal static DevelopmentActionResult ForceNextFirearmNaturalRollOne()
        {
            return ForceNextFirearmNaturalRoll(1);
        }

        internal static DevelopmentActionResult ForceNextFirearmNaturalRollTwo()
        {
            return ForceNextFirearmNaturalRoll(2);
        }

        internal static DevelopmentActionResult ForceNextFirearmNaturalRollThree()
        {
            return ForceNextFirearmNaturalRoll(3);
        }

        internal static DevelopmentActionResult ForceNextFirearmNaturalRollTwenty()
        {
            return ForceNextFirearmNaturalRoll(20);
        }

        internal static DevelopmentActionResult CancelForcedFirearmNaturalRoll()
        {
            return Execute(
                "cancel-forced-firearm-natural-roll",
                bridge => DevelopmentActionResult.Success(
                    FirearmMisfireRuntime.CancelForcedNaturalRoll()));
        }

        internal static DevelopmentActionResult AddTestMusket()
        {
            return Execute("add-test-musket", bridge => bridge.AddTestMusket());
        }

        internal static DevelopmentActionResult RemoveTestMuskets()
        {
            return Execute("remove-test-muskets", bridge => bridge.RemoveTestMuskets());
        }

        internal static DevelopmentActionResult AddBasicAmmunition()
        {
            return Execute(
                "add-basic-ammunition",
                bridge => bridge.AddBasicAmmunition(20));
        }

        internal static DevelopmentActionResult AddOneBlackPowder()
        {
            return Execute(
                "add-one-black-powder",
                bridge => bridge.AddBlackPowder(1));
        }

        internal static DevelopmentActionResult AddOneLeadBall()
        {
            return Execute(
                "add-one-lead-ball",
                bridge => bridge.AddLeadBalls(1));
        }

        internal static DevelopmentActionResult DescribeBasicAmmunition()
        {
            return Execute(
                "describe-basic-ammunition",
                bridge => bridge.DescribeBasicAmmunition());
        }

        internal static DevelopmentActionResult ConsumeOneBasicAmmunitionLoad()
        {
            return Execute(
                "consume-basic-ammunition-load",
                bridge => bridge.ConsumeOneBasicAmmunitionLoad());
        }

        internal static DevelopmentActionResult RemoveAllBasicAmmunition()
        {
            return Execute(
                "remove-all-basic-ammunition",
                bridge => bridge.RemoveAllBasicAmmunition());
        }

        internal static DevelopmentActionResult DescribeEquippedFirearms()
        {
            return Execute("describe-equipped-firearms", bridge => bridge.DescribeEquippedFirearms());
        }

        internal static DevelopmentActionResult DescribeVisibleFirearmStates()
        {
            return Execute("describe-visible-firearm-states", bridge => bridge.DescribeVisibleFirearmStates());
        }

        internal static DevelopmentActionResult CreatePersistenceFixtureAd()
        {
            return Execute("create-persistence-fixture-ad", bridge => bridge.CreatePersistenceFixtureAd());
        }

        internal static DevelopmentActionResult PrimeIndependentTestMusketStates()
        {
            return Execute("prime-independent-test-musket-states", bridge => bridge.PrimeIndependentTestMusketStates());
        }

        internal static DevelopmentActionResult SeedLegacyStateTokenForDebug()
        {
            return Execute("seed-legacy-state-token", bridge => bridge.SeedLegacyStateTokenForDebug());
        }

        internal static DevelopmentActionResult SeedLegacyReferenceStateForDebug()
        {
            return Execute("seed-legacy-reference-state", bridge => bridge.SeedLegacyReferenceStateForDebug());
        }

        internal static DevelopmentActionResult LoadFirstEquippedFirearmForDebug()
        {
            return Execute("load-first-equipped-firearm", bridge => bridge.LoadFirstEquippedFirearmForDebug());
        }

        internal static DevelopmentActionResult DamageFirstEquippedFirearmForDebug()
        {
            return Execute("damage-first-equipped-firearm", bridge => bridge.DamageFirstEquippedFirearmForDebug());
        }

        internal static DevelopmentActionResult RepairFirstEquippedFirearmForDebug()
        {
            return Execute("repair-first-equipped-firearm", bridge => bridge.RepairFirstEquippedFirearmForDebug());
        }

        internal static DevelopmentActionResult OverhaulFirstEquippedWreckedFirearmForDebug()
        {
            return Execute(
                "overhaul-first-equipped-wrecked-firearm",
                bridge => bridge.OverhaulFirstEquippedWreckedFirearmForDebug());
        }

        internal static DevelopmentActionResult ResetFirstEquippedFirearmState()
        {
            return Execute("reset-first-equipped-firearm", bridge => bridge.ResetFirstEquippedFirearmState());
        }

        internal static PersistenceEvidenceCaptureResult CapturePersistenceEvidenceSnapshot()
        {
            ModContext context;
            if (!ModContext.TryGet(out context))
            {
                return PersistenceEvidenceCaptureResult.Failure(
                    "The Kingmaker Gunslinger mod context is not available.");
            }

            try
            {
                BlueprintFeature proficiency = BlueprintBootstrap.FirearmProficiency;
                BlueprintAbility reloadAbility = BlueprintBootstrap.ReloadTestMusketAbility;
                BlueprintAbility overhaulAbility = BlueprintBootstrap.OverhaulTestMusketAbility;
                BlueprintAbility repairAbility = BlueprintBootstrap.RepairTestMusketAbility;
                BlueprintItemWeapon testMusket = BlueprintBootstrap.TestMusketItem;
                BasicAmmunitionBlueprintSet ammunition = BlueprintBootstrap.BasicAmmunition;
                BlueprintItem repairKit = BlueprintBootstrap.FirearmRepairKit;
                if (!BlueprintBootstrap.IsInitialized ||
                    proficiency == null ||
                    reloadAbility == null ||
                    overhaulAbility == null ||
                    repairAbility == null ||
                    testMusket == null ||
                    ammunition == null ||
                    repairKit == null)
                {
                    return PersistenceEvidenceCaptureResult.Failure(
                        "Blueprint initialization has not completed.");
                }

                var bridge = new KingmakerDevelopmentBridge(
                    proficiency,
                    reloadAbility,
                    overhaulAbility,
                    repairAbility,
                    testMusket,
                    ammunition.BlackPowder,
                    ammunition.LeadBall,
                    repairKit);
                PersistenceEvidenceSnapshotData snapshot = bridge.CapturePersistenceEvidenceSnapshot();
                context.Logger.Info(
                    "persistence-evidence",
                    "snapshot.captured",
                    snapshot.ToCanonicalSummary());
                return PersistenceEvidenceCaptureResult.Success(snapshot);
            }
            catch (Exception exception)
            {
                context.Logger.Failure(
                    "persistence-evidence",
                    "snapshot.failed",
                    "A persistence-evidence snapshot could not be captured.",
                    exception);
                return PersistenceEvidenceCaptureResult.Failure(
                    exception.GetType().Name + ": " + exception.Message);
            }
        }

        private static DevelopmentActionResult ForceNextFirearmNaturalRoll(
            int naturalRoll)
        {
            return Execute(
                "force-next-firearm-natural-roll-" +
                    naturalRoll.ToString(System.Globalization.CultureInfo.InvariantCulture),
                bridge => DevelopmentActionResult.Success(
                    FirearmMisfireRuntime.QueueForcedNaturalRoll(naturalRoll)));
        }

        private static DevelopmentActionResult Execute(
            string operation,
            Func<KingmakerDevelopmentBridge, DevelopmentActionResult> action)
        {
            if (string.IsNullOrWhiteSpace(operation))
            {
                throw new ArgumentException("A development operation name is required.", "operation");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            ModContext context;
            if (!ModContext.TryGet(out context))
            {
                return DevelopmentActionResult.Failure(
                    "The Kingmaker Gunslinger mod context is not available.");
            }

            try
            {
                BlueprintFeature proficiency = BlueprintBootstrap.FirearmProficiency;
                BlueprintAbility reloadAbility = BlueprintBootstrap.ReloadTestMusketAbility;
                BlueprintAbility overhaulAbility = BlueprintBootstrap.OverhaulTestMusketAbility;
                BlueprintAbility repairAbility = BlueprintBootstrap.RepairTestMusketAbility;
                BlueprintItemWeapon testMusket = BlueprintBootstrap.TestMusketItem;
                BasicAmmunitionBlueprintSet ammunition = BlueprintBootstrap.BasicAmmunition;
                BlueprintItem repairKit = BlueprintBootstrap.FirearmRepairKit;
                if (!BlueprintBootstrap.IsInitialized ||
                    proficiency == null ||
                    reloadAbility == null ||
                    overhaulAbility == null ||
                    repairAbility == null ||
                    testMusket == null ||
                    ammunition == null ||
                    repairKit == null)
                {
                    return DevelopmentActionResult.Failure(
                        "Blueprint initialization has not completed. Return to the main menu or inspect the KMG log for bootstrap errors.");
                }

                var bridge = new KingmakerDevelopmentBridge(
                    proficiency,
                    reloadAbility,
                    overhaulAbility,
                    repairAbility,
                    testMusket,
                    ammunition.BlackPowder,
                    ammunition.LeadBall,
                    repairKit);
                DevelopmentActionResult result = action(bridge);
                context.Logger.Info("development", operation + ".complete", result.Message);
                return result;
            }
            catch (Exception exception)
            {
                context.Logger.Failure(
                    "development",
                    operation + ".failed",
                    "A manual development control failed without mutating any later content automatically.",
                    exception);
                return DevelopmentActionResult.Failure(
                    exception.GetType().Name + ": " + exception.Message);
            }
        }

    }
}
