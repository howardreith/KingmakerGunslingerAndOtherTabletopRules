using System;
using System.Linq;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Explosions;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Firing;
using KingmakerGunslinger.Misfires;
using KingmakerGunslinger.Rules;
using KingmakerGunslinger.Reloading;
using KingmakerGunslinger.Recovery;
using UnityModManagerNet;

namespace KingmakerGunslinger.Development
{
    // Historical validator anchor: 0.0.29 Sprint 29 complete maintenance-loop smoke test.
    /// <summary>
    /// Unity Mod Manager panel for the Sprint 29 complete maintenance loop and
    /// accelerated deterministic qualification harness.
    /// </summary>
    internal static class DevelopmentUi
    {
        private static readonly object Gate = new object();
        private static string _status =
            "No development command has run. Load a disposable campaign before using these controls.";
        private static bool _removeTestMusketsConfirmationArmed;

        internal static void Draw(UnityModManager.ModEntry modEntry)
        {
            ImmediateModeGui.Label(
                "Kingmaker Gunslinger - 0.0.99 URBAN-BARBARIAN / CRAFT-MAGIC-ITEMS-AMMUNITION-UI-REPAIR / CRAFT-MAGIC-ITEMS-COMPATIBILITY / COMPATIBILITY-ATTRIBUTION-AUDIT / FIREARM-AUDIO-RESTORATION / IMMEDIATE-ACTION-ECONOMY / IN-HARMS-WAY-RUNTIME-REPAIR / EASTERN-FAVORED-COMPATIBILITY / HELPFUL-AID-ANOTHER / BODYGUARD-AC-BREAKDOWN / BODYGUARD-IN-HARMS-WAY / WEAPON-PRESENTATION-CALIBRATION / OVERNIGHT-GUNSLINGER-BUGFIXES / BROWN-FUR-HUMAN-REVIEW-REPAIR / EASTERN-WEAPONS / ELVEN-BRANCHED-SPEAR / EXPANDED-SUMMONING / SHIELD-OTHER / ACADAMAE-MODE-FATIGUE-ICON-REPAIR / FEATURE-MODULES / PAPER-CARTRIDGES-AUTO-RELOAD / RARE-FIREARMS / PISTOLERO-MUSKET-MASTER / FIREARM-NATIVE-WEAPON-RIGS / DODGE-EXPIRATION-R3");
            ImmediateModeGui.Label(
                "Dodge duration graph: native ContextActionApplyBuff plus a blueprint-scoped expired-fact removal guard.");
            ImmediateModeGui.Label(
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Dodge AC lifecycle: on={0}; off={1}; activeModifiers={2}.",
                    GunslingerDodgeArmorClassBonus.TurnedOn,
                    GunslingerDodgeArmorClassBonus.TurnedOff,
                    GunslingerDodgeArmorClassBonus.ActiveModifiers));
            ImmediateModeGui.Label(
                "Dodge expiration guard: " +
                GunslingerDodgeExpirationPatch.Describe());
            ImmediateModeGui.Label(
                "Use only a disposable campaign. Production inventory icons are project-owned; the temporary crossbow doll model remains pending the separately tracked 3D asset checkpoint.");
            ImmediateModeGui.Label(
                BlueprintBootstrap.IsInitialized
                    ? "Blueprint state: initialized."
                    : "Blueprint state: not initialized; controls will fail closed.");

            FirearmVisualCalibrationUi.Draw();

            lock (Gate)
            {
                ImmediateModeGui.Label("Last result: " + _status);
            }

            ImmediateModeGui.Label(
                "Sprint 28 is runtime-accepted. Version 0.0.29 completes the player-facing maintenance loop: Overhaul changes one exact empty/Wrecked Test Musket to empty/Broken, Repair changes that same exact empty/Broken item to empty/Normal, and Reload then consumes one powder-and-ball pair to load it. Overhaul and Repair are separate full-round actions and each consumes one Firearm Repair Kit only when delivery completes. The accelerated fixture below prints one concise identity, resource, fault, duplicate, and second-item PASS/FAIL matrix after every stage.");

            bool tracingWasEnabled = CombatTraceSettings.Enabled;
            bool tracingIsEnabled = ImmediateModeGui.Toggle(
                tracingWasEnabled,
                "Enable firearm combat tracing (verbose AC and attack log output)");
            if (tracingIsEnabled != tracingWasEnabled)
            {
                SetCombatTracing(tracingIsEnabled);
            }

            ImmediateModeGui.Label(
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Combat traces: enabled={0}; active={1}; completed={2}; faults={3}.",
                    CombatTraceSettings.Enabled,
                    CombatTraceRuntime.ActiveTraceCount,
                    CombatTraceRuntime.CompletedTraceCount,
                    CombatTraceRuntime.FaultCount));
            ImmediateModeGui.Label(
                string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Firearm AC: active attack depth={0}; touch selections={1}; ordinary selections={2}; duplicate events={3}; faults={4}.",
                    FirearmArmorClassRuntime.ActiveAttackDepth,
                    FirearmArmorClassRuntime.AppliedCount,
                    FirearmArmorClassRuntime.OrdinaryCount,
                    FirearmArmorClassRuntime.DuplicateCount,
                    FirearmArmorClassRuntime.FaultCount));

            ImmediateModeGui.Label("Reload runtime: " + ReloadRuntimeDiagnostics.Describe());
            ImmediateModeGui.Label("Overhaul runtime: " + OverhaulRuntimeDiagnostics.Describe());
            ImmediateModeGui.Label("Repair runtime: " + RepairRuntimeDiagnostics.Describe());
            ImmediateModeGui.Label("Firearm attack enforcement: " + FirearmDischargeRuntimeDiagnostics.Describe());
            ImmediateModeGui.Label("Natural-roll misfires: " + FirearmMisfireRuntime.Describe());
            ImmediateModeGui.Label("Firearm Wwise audio: " + Audio.FirearmSoundRuntime.Describe());
            if (ImmediateModeGui.Button("Print firearm Wwise diagnostics")) Run(DevelopmentControls.DescribeFirearmAudio);
            if (ImmediateModeGui.Button("Retry newly installed firearm Wwise bank (development)")) Run(DevelopmentControls.RetryFirearmAudio);
            if (ImmediateModeGui.Button("Play global Pistol Wwise preview (non-spatial)")) Run(DevelopmentControls.PreviewGlobalPistolAudio);
            if (ImmediateModeGui.Button("Play selected equipped firearm Wwise preview")) Run(DevelopmentControls.PreviewSelectedFirearmAudio);
            ImmediateModeGui.Label("Second-misfire explosion: " + FirearmExplosionRuntimeDiagnostics.Describe());
            ImmediateModeGui.Label("State-token native reconciliation: " + FirearmStateTokenReconciliationDiagnostics.Describe());

            if (FirearmRuntimeState.IsConfigured)
            {
                IFirearmStateRepository repository = FirearmRuntimeState.Repository;
                ImmediateModeGui.Label(
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "State carrier: {0}; entriesCreated={1}; mutations={2}; removals={3}; save/restart proof=PASSED; quicksave reconciliation repair=ACTIVE; player-facing Overhaul + ordinary Repair=ACTIVE; accelerated maintenance qualification=ACTIVE.",
                        FirearmRuntimeState.CarrierDescription,
                        repository.CreatedEntryCount,
                        repository.MutationCount,
                        repository.RemovalCount));
            }
            else
            {
                ImmediateModeGui.Label(
                    "Item-token persistence candidate: NOT CONFIGURED; inspect the KMG bootstrap log.");
            }

            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label("Rare Firearm Acceptance (DEVELOPMENT ONLY)");
            ImmediateModeGui.Label("Use a disposable campaign. Spawning never proves campaign placement and grants no proficiency or ammunition. The read-only acquisition locator reports exact blueprint/current-area identity; live coordinates remain unavailable unless safely resolved.");
            if (ImmediateModeGui.Button("Print complete rare-firearm catalog audit"))
                Run(DevelopmentControls.DescribeRareFirearmCatalog);
            if (ImmediateModeGui.Button("Add one copy of all eight test items"))
                Run(DevelopmentControls.AddRareFirearmSet);
            string[] rareNames = { "Pistol +1", "Musket +1", "Blunderbuss +1",
                "Duelist's Rebuttal", "The River King's Measure",
                "Irovetti's Ovation", "The Last Word",
                "Watch at the World's End" };
            for (int rareIndex = 0; rareIndex < rareNames.Length; rareIndex++)
            {
                int selected = rareIndex;
                if (ImmediateModeGui.Button("Add one " + rareNames[rareIndex]))
                    Run(() => DevelopmentControls.AddRareFirearm(selected));
            }
            if (ImmediateModeGui.Button("Print acquisition/current-area location audit"))
                Run(DevelopmentControls.DescribeRareFirearmAcquisition);
            if (ImmediateModeGui.Button(
                    "Print all project magic-item location audits"))
                Run(DevelopmentControls.DescribeProjectMagicItemAcquisition);

            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label(
                "Elven Branched Spear Acceptance (DEVELOPMENT ONLY)");
            ImmediateModeGui.Label(
                "Use only KMG_AUTOMATION_WORKING or another disposable save. These grants prove inventory and save behavior, not campaign placement, and grant no proficiency or feats.");
            if (ImmediateModeGui.Button(
                    "Print complete Elven Branched Spear catalog audit"))
                Run(DevelopmentControls.DescribeElvenBranchedSpearCatalog);
            if (ImmediateModeGui.Button(
                    "Add one copy of all 12 Elven Branched Spear variants"))
                Run(DevelopmentControls.AddElvenBranchedSpearSet);
            string[] spearNames = { "Elven Branched Spear",
                "Masterwork Elven Branched Spear",
                "Cold Iron Elven Branched Spear",
                "Masterwork Cold Iron Elven Branched Spear",
                "+1 Elven Branched Spear",
                "+1 Cold Iron Elven Branched Spear", "Boughkeeper",
                "Thornstep", "Moonlit Fork", "Viper's Reach",
                "Briar-Crowned Spear", "Spear of the First Branch" };
            for (int spearIndex = 0; spearIndex < spearNames.Length;
                spearIndex++)
            {
                int selectedSpear = spearIndex;
                if (ImmediateModeGui.Button("Add one " +
                        spearNames[spearIndex]))
                    Run(() => DevelopmentControls.AddElvenBranchedSpear(
                        selectedSpear));
            }

            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label("Eastern Weapons Acceptance (DEVELOPMENT ONLY)");
            ImmediateModeGui.Label(
                "Disposable saves only. Inventory grants do not grant proficiency, feats, class levels, vendor state, loot state, campaign flags, or invoke save APIs.");
            if (ImmediateModeGui.Button(
                    "Print complete Eastern Weapons catalog audit"))
                Run(DevelopmentControls.DescribeEasternWeaponCatalog);
            if (ImmediateModeGui.Button(
                    "Print Border Sentinel location audit"))
                Run(DevelopmentControls.DescribeBorderSentinelAcquisition);
            if (ImmediateModeGui.Button("Add all 30 Eastern Weapon variants"))
                Run(DevelopmentControls.AddEasternWeaponSet);
            if (ImmediateModeGui.Button("Add complete Wakizashi path (10)"))
                Run(DevelopmentControls.AddWakizashiPath);
            if (ImmediateModeGui.Button("Add complete Katana path (10)"))
                Run(DevelopmentControls.AddKatanaPath);
            if (ImmediateModeGui.Button("Add complete Nodachi path (10)"))
                Run(DevelopmentControls.AddNodachiPath);
            string[] easternNames = BlueprintBootstrap.EasternWeapons == null ||
                BlueprintBootstrap.EasternWeapons.Named == null ?
                new string[0] : BlueprintBootstrap.EasternWeapons.Entries.Select(
                    value => value.Item.Name).Concat(BlueprintBootstrap
                        .EasternWeapons.Named.Entries.Select(value =>
                            value.Item.Name)).ToArray();
            for (int easternIndex = 0; easternIndex < easternNames.Length;
                easternIndex++)
            {
                int selectedEastern = easternIndex;
                if (ImmediateModeGui.Button("Add [" + easternIndex + "] " +
                        easternNames[easternIndex]))
                    Run(() => DevelopmentControls.AddEasternWeapon(
                        selectedEastern));
            }

            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label("Character and Test Musket controls");

            if (ImmediateModeGui.Button("Grant Firearm Proficiency to selected unit"))
            {
                Run(DevelopmentControls.GrantFirearmProficiency);
            }

            if (ImmediateModeGui.Button("Add one Test Musket to shared inventory"))
            {
                Run(DevelopmentControls.AddTestMusket);
            }

            bool removalArmed;
            lock (Gate)
            {
                removalArmed = _removeTestMusketsConfirmationArmed;
            }

            if (!removalArmed)
            {
                if (ImmediateModeGui.Button("Arm removal of ALL unequipped Test Muskets (destructive)"))
                {
                    lock (Gate)
                    {
                        _removeTestMusketsConfirmationArmed = true;
                        _status = "Test Musket removal is armed but has not run. Use the explicit confirmation button or cancel it.";
                    }
                }
            }
            else
            {
                ImmediateModeGui.Label(
                    "WARNING: the next confirmation removes every unequipped Test Musket from shared inventory. It is a cleanup diagnostic, not a condition transition.");
                if (ImmediateModeGui.Button("CONFIRM remove ALL unequipped Test Muskets"))
                {
                    lock (Gate)
                    {
                        _removeTestMusketsConfirmationArmed = false;
                    }

                    Run(DevelopmentControls.RemoveTestMuskets);
                }

                if (ImmediateModeGui.Button("Cancel Test Musket removal"))
                {
                    lock (Gate)
                    {
                        _removeTestMusketsConfirmationArmed = false;
                        _status = "Cancelled Test Musket removal; no inventory or state mutation was requested.";
                    }
                }
            }

            if (ImmediateModeGui.Button("Print selected unit's equipped-firearm state diagnostics"))
            {
                Run(DevelopmentControls.DescribeEquippedFirearms);
            }

            if (ImmediateModeGui.Button("Print visible firearm states (equipment + shared inventory)"))
            {
                Run(DevelopmentControls.DescribeVisibleFirearmStates);
            }

            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label("Reload and loaded-round attack controls");
            ImmediateModeGui.Label(
                "Equip one empty Normal or Broken Test Musket and carry one Black Powder Charge plus one Lead Ball. Reload must preserve condition, consume one component pair, and load one round. Wrecked firearms must remain unavailable.");

            if (ImmediateModeGui.Button("Print Reload Test Musket readiness"))
            {
                Run(DevelopmentControls.DescribeReloadReadiness);
            }

            if (ImmediateModeGui.Button("Reload equipped Test Musket immediately (diagnostic)"))
            {
                Run(DevelopmentControls.ReloadEquippedTestMusketNowForDebug);
            }

            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label("Player-facing Overhaul and Repair controls");
            ImmediateModeGui.Label(
                "Granting Firearm Proficiency grants Reload Test Musket, Overhaul Test Musket, and Repair Test Musket. Overhaul is a full-round Wrecked-to-Broken action; Repair is a separate full-round Broken-to-Normal action. Each requires exactly one equipped empty Test Musket in the matching condition and consumes one Firearm Repair Kit only when delivery completes. Neither action loads ammunition or replaces the item. Immediate controls bypass action economy and are diagnostics only.");

            if (ImmediateModeGui.Button("Print Overhaul Test Musket readiness"))
            {
                Run(DevelopmentControls.DescribeOverhaulReadiness);
            }

            if (ImmediateModeGui.Button("Overhaul equipped Test Musket immediately (diagnostic)"))
            {
                Run(DevelopmentControls.OverhaulEquippedTestMusketNowForDebug);
            }

            if (ImmediateModeGui.Button("Print Repair Test Musket readiness"))
            {
                Run(DevelopmentControls.DescribeRepairReadiness);
            }

            if (ImmediateModeGui.Button("Repair equipped Test Musket immediately (diagnostic)"))
            {
                Run(DevelopmentControls.RepairEquippedTestMusketNowForDebug);
            }

            if (ImmediateModeGui.Button("Add five Firearm Repair Kits"))
            {
                Run(DevelopmentControls.AddFirearmRepairKits);
            }

            if (ImmediateModeGui.Button("Add one Firearm Repair Kit"))
            {
                Run(DevelopmentControls.AddOneFirearmRepairKit);
            }

            if (ImmediateModeGui.Button("Print Firearm Repair Kit count"))
            {
                Run(DevelopmentControls.DescribeFirearmRepairKits);
            }

            if (ImmediateModeGui.Button("Remove all Firearm Repair Kits from shared inventory"))
            {
                Run(DevelopmentControls.RemoveAllFirearmRepairKits);
            }


            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label("Sprint 29 accelerated maintenance qualification");
            ImmediateModeGui.Label(
                "Equip exactly one Test Musket. Prepare creates or normalizes a second independent Test Musket, sets the equipped exact item to empty/Wrecked, sets the second to empty/Normal, ensures two repair kits plus one powder-and-ball pair, and captures a process-local baseline. Then use the action-bar Overhaul ability, print the matrix, use Repair, print the matrix, use Reload, and print the final matrix. Preparation is destructive to the equipped firearm state and is for a disposable campaign only.");

            if (ImmediateModeGui.Button("Prepare Sprint 29 maintenance qualification fixture"))
            {
                Run(DevelopmentControls.PrepareMaintenanceQualificationFixture);
            }

            if (ImmediateModeGui.Button("Run complete maintenance qualification immediately (diagnostic)"))
            {
                Run(DevelopmentControls.RunMaintenanceQualificationImmediately);
            }

            if (ImmediateModeGui.Button("Print Sprint 29 maintenance PASS/FAIL matrix"))
            {
                Run(DevelopmentControls.DescribeMaintenanceQualification);
            }

            if (ImmediateModeGui.Button("Clear Sprint 29 qualification baseline (no item mutation)"))
            {
                Run(DevelopmentControls.ResetMaintenanceQualification);
            }

            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label("Natural-roll, burst, and item-lifecycle diagnostic controls");
            ImmediateModeGui.Label(
                "The Test Musket retains the accepted natural 1-2 misfire path, native 5-foot second-misfire burst, and recoverable Wrecked item state. Forced rolls and direct condition mutations below remain diagnostics for deterministic setup. Use the action-bar abilities—not the immediate diagnostic buttons—to qualify player-facing Overhaul, Repair, and Reload delivery and interruption behavior.");

            if (ImmediateModeGui.Button("Force next eligible firearm natural d20 to 1"))
            {
                Run(DevelopmentControls.ForceNextFirearmNaturalRollOne);
            }

            if (ImmediateModeGui.Button("Force next eligible firearm natural d20 to 2"))
            {
                Run(DevelopmentControls.ForceNextFirearmNaturalRollTwo);
            }

            if (ImmediateModeGui.Button("Force next eligible firearm natural d20 to 3"))
            {
                Run(DevelopmentControls.ForceNextFirearmNaturalRollThree);
            }

            if (ImmediateModeGui.Button("Force next eligible firearm natural d20 to 20"))
            {
                Run(DevelopmentControls.ForceNextFirearmNaturalRollTwenty);
            }

            if (ImmediateModeGui.Button("Cancel pending forced firearm natural d20"))
            {
                Run(DevelopmentControls.CancelForcedFirearmNaturalRoll);
            }

            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label("Basic ammunition inventory controls");
            ImmediateModeGui.Label(
                "The full-round Reload Test Musket ability now consumes one of each component. The standalone consume command remains an inventory-only diagnostic.");

            if (ImmediateModeGui.Button("Add 20 Black Powder Charges and 20 Lead Balls"))
            {
                Run(DevelopmentControls.AddBasicAmmunition);
            }

            if (ImmediateModeGui.Button("Add one Black Powder Charge"))
            {
                Run(DevelopmentControls.AddOneBlackPowder);
            }

            if (ImmediateModeGui.Button("Add one Lead Ball"))
            {
                Run(DevelopmentControls.AddOneLeadBall);
            }

            if (ImmediateModeGui.Button("Print basic-ammunition counts"))
            {
                Run(DevelopmentControls.DescribeBasicAmmunition);
            }

            if (ImmediateModeGui.Button("Consume one powder + ball pair atomically"))
            {
                Run(DevelopmentControls.ConsumeOneBasicAmmunitionLoad);
            }

            if (ImmediateModeGui.Button("Remove all basic ammunition from shared inventory"))
            {
                Run(DevelopmentControls.RemoveAllBasicAmmunition);
            }

            ImmediateModeGui.Space(8f);
            ImmediateModeGui.Label("Item-token persistence controls");
            ImmediateModeGui.Label(
                "Create the A-D fixture, save, exit completely, reload, and print visible states. A-C must retain distinct token states; D must remain empty/normal with no token.");

            if (ImmediateModeGui.Button("Create/normalize A-D item-token persistence fixture"))
            {
                Run(DevelopmentControls.CreatePersistenceFixtureAd);
            }

            if (ImmediateModeGui.Button("Store independent token states on two inventory Test Muskets"))
            {
                Run(DevelopmentControls.PrimeIndependentTestMusketStates);
            }

            ImmediateModeGui.Space(6f);
            ImmediateModeGui.Label(
                "The following controls consume no inventory ammunition and are only for state-carrier testing.");

            if (ImmediateModeGui.Button("Load first equipped firearm with one debug round"))
            {
                Run(DevelopmentControls.LoadFirstEquippedFirearmForDebug);
            }

            if (ImmediateModeGui.Button("Apply misfire damage to first equipped firearm"))
            {
                Run(DevelopmentControls.DamageFirstEquippedFirearmForDebug);
            }

            if (ImmediateModeGui.Button("Repair first equipped Broken firearm to Normal (diagnostic)"))
            {
                Run(DevelopmentControls.RepairFirstEquippedFirearmForDebug);
            }

            if (ImmediateModeGui.Button("Overhaul first equipped Wrecked firearm to Broken (direct contract diagnostic)"))
            {
                Run(DevelopmentControls.OverhaulFirstEquippedWreckedFirearmForDebug);
            }

            if (ImmediateModeGui.Button("Reset first equipped firearm to empty / normal"))
            {
                Run(DevelopmentControls.ResetFirstEquippedFirearmState);
            }
        }

        private static void SetCombatTracing(bool enabled)
        {
            bool changed = CombatTraceSettings.SetEnabled(enabled);
            int clearedTraces = enabled ? 0 : CombatTraceRuntime.ResetCurrentThread();
            int clearedAttackContexts = enabled ? 0 : FirearmArmorClassRuntime.ResetCurrentThread();
            lock (Gate)
            {
                _status = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Combat tracing {0}; changed={1}; clearedActiveTraces={2}; clearedAttackContexts={3}.",
                    enabled ? "ENABLED" : "DISABLED",
                    changed,
                    clearedTraces,
                    clearedAttackContexts);
            }

            ModContext context;
            if (ModContext.TryGet(out context))
            {
                context.Logger.Info(
                    "combat",
                    enabled ? "trace.enabled" : "trace.disabled",
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Firearm combat tracing was {0} from the UMM panel; clearedActiveTraces={1}; clearedAttackContexts={2}. Touch-AC behavior remains active while tracing is disabled.",
                        enabled ? "enabled" : "disabled",
                        clearedTraces,
                        clearedAttackContexts));
            }
        }

        private static void Run(Func<DevelopmentActionResult> command)
        {
            if (command == null)
            {
                return;
            }

            DevelopmentActionResult result = command();
            lock (Gate)
            {
                _status = (result.Succeeded ? "SUCCESS - " : "FAILED - ") + result.Message;
            }
        }
    }
}
