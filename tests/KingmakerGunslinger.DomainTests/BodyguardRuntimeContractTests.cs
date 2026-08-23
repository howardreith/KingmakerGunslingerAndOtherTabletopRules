using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BodyguardRuntimeContractTests
    {
        internal static void NativeActionEconomyIsAuthoritative()
        {
            string source = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "BodyguardActionEconomyAccess.cs");
            foreach (string token in new[]
            {
                "unit.CombatState.CanAttackOfOpportunity",
                "unit.CombatState.AttackOfOpportunity(attacker, true)",
                "unit.CombatState.AttackOfOpportunityCount",
                "AttackOfOpportunityCount = before - 1",
                "UnitCondition.DisableAttacksOfOpportunity",
                "UnitCondition.CanNotAttack",
                "attacker.Memory.Contains(unit)",
                "UnitEngagementExtension.GetThreatHand(unit)",
                "unit.HasSwiftAction()",
                "unit.CombatState.Cooldown.SwiftAction",
                "SwiftActionCooldownSeconds = 6f",
                "TryRollbackImmediateAction"
            })
                Assertions.True(source.Contains(token),
                    "Native action-economy adapter lacks: " + token);
            Assertions.True(!source.Contains("AttackOfOpportunity(target") &&
                !source.Contains("new UnitAttackOfOpportunity") &&
                !source.Contains("InHarmUsedThisRound") &&
                !source.Contains("Dictionary<UnitEntityData"),
                "Bodyguard created an attack command or transient dictionary counter.");

            string immediate = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "ImmediateActionEconomyRuntime.cs");
            foreach (string token in new[]
            {
                "ImmediatePending",
                "ImmediateChargedTurn",
                "OnCooldownCleared",
                "OnTurnDisposed",
                "TurnStatus.Delayed",
                "SwiftActionCooldownSeconds",
                "RestoreAfterLoad",
                "ClearAll"
            })
                Assertions.True(immediate.Contains(token),
                    "Turn-based immediate debt runtime lacks: " + token);

            string patches = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "ImmediateActionEconomyPatches.cs");
            foreach (string token in new[]
            {
                "UnitCombatState.Cooldowns",
                "typeof(TurnController), \"Dispose\"",
                "typeof(UnitEntityData), \"HasSwiftAction\"",
                "HasChargedTurnDebt(__instance)",
                "typeof(UnitEntityData), \"PostLoad\"",
                "ImmediateActionEconomyRuntime.ClearAll"
            })
                Assertions.True(patches.Contains(token),
                    "Immediate-action lifecycle patch lacks: " + token);
        }

        internal static void ThreatAndAidUseNativeRulePaths()
        {
            string threat = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "BodyguardThreatAccess.cs");
            foreach (string token in new[]
            {
                "protector.Body.PrimaryHand",
                "protector.Body.SecondaryHand",
                "protector.Body.AdditionalLimbs",
                "UnitEngagementExtension.IsReach(protector, attacker, slot)",
                "new RuleCalculateAttackBonus(protector, attacker,",
                "slot.Weapon.Blueprint.IsMelee",
                "slot.Weapon.Blueprint.IsUnarmed",
                "Features.ImprovedUnarmedStrike",
                "BodyguardAttackSelectionPolicy.Select(candidates)"
            })
                Assertions.True(threat.Contains(token),
                    "Native threat/Aid selection adapter lacks: " + token);
            string runtime = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "BodyguardRuntime.cs");
            Assertions.True(runtime.Contains("new RuleRollD20(") &&
                runtime.Contains("TryApplyAidOverride(roll)") &&
                runtime.Contains("BodyguardAttemptCoordinator.Execute(") &&
                runtime.Contains("TrySpendAttackOfOpportunity") &&
                !runtime.Contains("new RuleAttackRoll(candidate") &&
                !runtime.Contains("new RuleAttackWithWeapon(candidate") &&
                !runtime.Contains("UnitAttackOfOpportunity"),
                "Aid Another does not remain a nonattacking native bonus+d20 calculation.");
        }

        internal static void AttackTimingAndAcScopeAreExact()
        {
            string patches = Read("src", "KingmakerGunslinger", "Diagnostics",
                "CombatTracePatches.cs");
            int before = patches.IndexOf(
                "BodyguardRuntime.BeforeAttackRoll(__instance as RuleAttackRoll)",
                StringComparison.Ordinal);
            int firearm = patches.IndexOf(
                "FirearmDischargeRuntime.BeforeAttackRoll(__instance)",
                StringComparison.Ordinal);
            int after = patches.IndexOf(
                "BodyguardRuntime.AfterAttackRoll(__instance as RuleAttackRoll)",
                StringComparison.Ordinal);
            int bleeding = patches.IndexOf("BleedingWoundRuntime.AfterAttack(",
                StringComparison.Ordinal);
            int firearmAc = patches.IndexOf(
                "FirearmArmorClassRuntime.AfterCalculateArmorClass(__instance)",
                StringComparison.Ordinal);
            int bodyguardAc = patches.IndexOf(
                "BodyguardRuntime.AfterCalculateArmorClass(",
                StringComparison.Ordinal);
            Assertions.True(before >= 0 && firearm > before && after >= 0 &&
                bleeding > after && firearmAc >= 0 && bodyguardAc > firearmAc,
                "Shared Harmony ordering no longer preauthorizes Bodyguard, arbitrates interception before riders, and applies AC after firearm touch selection.");

            string runtime = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "BodyguardRuntime.cs");
            Assertions.True(runtime.Contains("BeforeAttackRoll(") &&
                runtime.Contains("CommitAttempt(frame, candidate)") &&
                runtime.Contains("frame.Policy.FinishBodyguard()") &&
                runtime.Contains("bool hit = attack.IsHit;") &&
                runtime.IndexOf("CommitAttempt(frame, candidate)",
                    StringComparison.Ordinal) < runtime.IndexOf(
                    "bool hit = attack.IsHit;", StringComparison.Ordinal) &&
                runtime.Contains("TryApplyArmorClass(armorClass)") &&
                runtime.Contains("ApplyArmorClassAttribution(armorClass, frame, before, bonus)") &&
                runtime.Contains("plan.FinalArmorClass") &&
                runtime.Contains("FramesByRoll") && runtime.Contains(
                    "[ThreadStatic]"),
                "Attack-scoped preauthorization/AC/frame ordering changed.");
        }

        internal static void ArmorClassBreakdownUsesNativeBodyguardSources()
        {
            string runtime = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "BodyguardRuntime.cs");
            foreach (string token in new[]
            {
                "BodyguardArmorClassAttributionPolicy.Create(",
                "attempt.Protector.Descriptor.GetFact(",
                "blueprints.Bodyguard",
                "new BonusSource(contribution.Bonus, source)",
                "armorClass.BonusSources.Add(source)",
                "plan.FinalArmorClass",
                "armorClass.BonusSources.RemoveRange(",
                "bodyguardSourceCount=",
                "value.Source.Name",
                "value.Source.Blueprint.AssetGuid"
            })
                Assertions.True(runtime.Contains(token),
                    "Bodyguard native AC attribution lacks: " + token);
            Assertions.True(!runtime.Contains("armorClass.AddBonus(") &&
                !runtime.Contains("AddTemporaryModifier") &&
                !runtime.Contains("Stats.AC.AddModifier"),
                "Postfix attribution can double-count or leak through a target stat modifier.");

            string blueprint = Read("src", "KingmakerGunslinger",
                "Blueprints", "BodyguardFeatBlueprints.cs");
            Assertions.True(blueprint.Contains(
                    "CreateFeat(\"KMG_Bodyguard_Feature\", \"Bodyguard\"") &&
                blueprint.Contains(
                    "LocalizationService.Create(localizationStem + \".Name\", displayName)") &&
                runtime.Contains("value.Source.Name"),
                "The native BonusSource cannot resolve the player-facing Bodyguard label.");

            string il = Read("artifacts", "inspection", "bodyguard-native",
                "Assembly-CSharp.il");
            foreach (string token in new[]
            {
                "RuleCalculateAC::BonusSources",
                "RuleCalculateAC::set_TargetAC",
                "RuleCalculateAC::AddBonus",
                "AttackLogMessage::AppendArmorClassBreakdown",
                "StatModifiersBreakdown::AddBonusSources",
                "Kingmaker.RuleSystem.Rules.BonusSource::Bonus",
                "Kingmaker.RuleSystem.Rules.BonusSource::Source",
                "Kingmaker.UI.IUIDataProvider::get_Name()"
            })
                Assertions.True(il.Contains(token),
                    "Installed native AC-breakdown IL contract lacks: " + token);

            string investigation = Read("docs", "investigations",
                "bodyguard-in-harms-way.md");
            Assertions.True(investigation.Contains(
                    "AppendArmorClassBreakdown") &&
                investigation.Contains("display-only BonusSource") &&
                investigation.Contains("postfix") &&
                investigation.Contains("does not") &&
                investigation.Contains("change TargetAC"),
                "Durable investigation does not explain the no-double-count attribution seam.");
        }

        internal static void DeliverySeamsRedirectCompleteRecipients()
        {
            string access = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "InHarmsWayDeliveryAccess.cs");
            foreach (string token in new[]
            {
                "typeof(RulebookTargetEvent).GetField(\"Target\"",
                "typeof(AbilityDeliveryTarget).GetField(\"Target\"",
                "typeof(AbilityDeliveryTarget).GetMethod(\"set_AttackRoll\"",
                "typeof(AbilityExecutionProcess).GetMethod(\"ApplyEffect\"",
                "typeof(RulebookEventContext).GetMethod(\"PopEvent\"",
                "RuleTargetField.SetValue(rule, interceptor)",
                "AbilityTargetField.SetValue(delivery, redirected)",
                "IsRuleTarget",
                "TryRestoreRuleTarget",
                "TryRestoreAbilityTarget",
                "typeof(ElementsContextData).GetMethod(\"Dispose\""
            })
                Assertions.True(access.Contains(token),
                    "Exact In Harm's Way delivery contract lacks: " + token);

            string runtime = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "BodyguardRuntime.cs");
            string deliveryPatches = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "BodyguardDeliveryPatches.cs");
            Assertions.True(runtime.Contains("TryCommitRedirection") &&
                runtime.Contains("RuleEventCompleted") &&
                runtime.Contains("AbilityDeliveryTargetAssigned") &&
                runtime.Contains("RestoreTargets(frame") &&
                runtime.Contains(
                    "Partial weapon-target redirection did not restore every original recipient.") &&
                runtime.Contains("TryRollbackImmediateAction(") &&
                runtime.Contains("PendingProjectileResolves") &&
                deliveryPatches.Contains(
                    "BodyguardRuleEventCompletionPatch") &&
                deliveryPatches.Contains("BodyguardAbilityDeliveryTargetPatch") &&
                deliveryPatches.Contains("BodyguardAbilityApplyEffectPatch") &&
                deliveryPatches.Contains(
                    "BodyguardAbilityContextDisposePatch") &&
                deliveryPatches.Contains(
                    "Rulebook.TriggerEventInternal catches failures") &&
                deliveryPatches.Contains(
                    "ContextAttackData from") &&
                !deliveryPatches.Contains("Finalizer(") &&
                !runtime.Contains("RuleDealDamage") &&
                !runtime.Contains("RestoreHitPoints") &&
                !runtime.Contains("new RuleAttackWithWeapon("),
                "In Harm's Way regressed to replay/damage transfer or lost its exception-safe full-delivery seams.");
        }

        internal static void HarmonyTwelveCleanupContractsAreExplicit()
        {
            string project = Read("src", "KingmakerGunslinger",
                "KingmakerGunslinger.csproj");
            string patches = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "BodyguardDeliveryPatches.cs");
            string observer = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "BodyguardNativeContractObserver.cs");
            Assertions.True(project.Contains(
                    "<Reference Include=\"0Harmony12\">") &&
                patches.Contains("private static void Prefix(") &&
                patches.Contains("private static void Postfix(") &&
                !patches.Contains("private static Exception Finalizer") &&
                observer.Contains("RulebookEventContext.PopEvent") &&
                observer.Contains("ElementsContextData.Dispose") &&
                observer.Contains("patches.Prefixes") &&
                observer.Contains("patches.Postfixes") &&
                observer.Contains("patches.Transpilers") &&
                observer.Contains(
                    "ExpectedSupportedGameVersion = \"2.1.7b\"") &&
                observer.Contains(
                    "exact Assembly-CSharp SHA-256 and MVID contract") &&
                observer.Contains(
                    "typeof(RuleRollDice).GetMethod(\"Override\"") &&
                observer.Contains(
                    "AttackOfOpportunityBeforeInitiative"),
                "Bodyguard cleanup is not auditable through Harmony 1.2's actual patch registry.");
        }

        internal static void ImmediateActionAssemblyContractIsExact()
        {
            string il = Read("artifacts", "inspection", "bodyguard-native",
                "Assembly-CSharp.il");
            foreach (string token in new[]
            {
                "HasSwiftAction() cil managed",
                "UnitCombatState/Cooldowns::get_SwiftAction()",
                "end of method UnitEntityData::HasSwiftAction",
                "instance void  Clear() cil managed",
                "Cooldowns::set_SwiftAction(float32)",
                "end of method Cooldowns::Clear",
                "TurnController::Prepare()",
                "TurnController::Dispose()",
                "TurnController::ForceToEnd",
                "TurnController::DelayInitiaive",
                "RuleCheckTargetFlatFooted::OnTrigger",
                "UnitCombatState::get_CanActInCombat()",
                "UnitState::get_IsHelpless()"
            })
                Assertions.True(il.Contains(token),
                    "Installed immediate-action engine contract lacks: " +
                    token);

            string patches = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "ImmediateActionEconomyPatches.cs");
            Assertions.True(patches.Contains(
                    "typeof(TurnController), \"Prepare\"") &&
                patches.Contains(
                    "typeof(UnitCombatState.Cooldowns), \"Clear\"") &&
                patches.Contains(
                    "typeof(TurnController), \"Dispose\"") &&
                patches.Contains(
                    "typeof(UnitEntityData), \"HasSwiftAction\"") &&
                patches.Contains(
                    "HandlePartyCombatStateChanged") &&
                !patches.Contains("RoundNumber"),
                "Immediate debt is not tied to exact actual-turn/native-swift lifecycle seams.");
        }

        internal static void GuardedRuntimeScenariosCoverTheSubsystem()
        {
            string catalog = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string runner = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs");
            string scenario = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "BodyguardCombatScenario.cs");
            string fixture = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "BodyguardCombatFixture.cs");
            string probe = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "BodyguardQualificationProbe.cs");
            string automation = Read("scripts",
                "RuntimeAutomation.Common.ps1");
            string qualification = Read("scripts",
                "Invoke-BodyguardRuntimeQualification.ps1");
            string humanRepro = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "InHarmsWayHumanReproScenario.cs");
            string offTurnEconomy = Read("src", "KingmakerGunslinger",
                "RuntimeTesting", "InHarmsWayOffTurnEconomyScenario.cs");
            string humanReproLauncher = Read("scripts",
                "Invoke-InHarmsWayHumanRepro.ps1");
            foreach (string id in new[]
            {
                "observe-bodyguard-native-contracts",
                "observe-aid-another-compatibility-contracts",
                "disposable-bodyguard-feats",
                "disposable-helpful-bodyguard",
                "disposable-bodyguard-feats-disabled",
                "disposable-in-harms-way-human-repro",
                "disposable-in-harms-way-off-turn-economy"
            })
                Assertions.True(catalog.Contains(id),
                    "Guarded runtime catalog lacks " + id + ".");
            Assertions.True(automation.Contains(
                    "'observe-bodyguard-native-contracts'") &&
                automation.Contains(
                    "'observe-aid-another-compatibility-contracts'") &&
                automation.Contains("'disposable-bodyguard-feats'") &&
                automation.Contains("'disposable-helpful-bodyguard'") &&
                automation.Contains(
                    "'disposable-bodyguard-feats-disabled'") &&
                automation.Contains(
                    "'disposable-in-harms-way-human-repro'") &&
                automation.Contains(
                    "'disposable-in-harms-way-off-turn-economy'") &&
                qualification.Contains("Set-BodyguardFeatureState $true") &&
                qualification.Contains("Set-BodyguardFeatureState $false") &&
                qualification.Contains(
                    "Invoke-BodyguardScenario 'observe-aid-another-compatibility-contracts'") &&
                qualification.Contains(
                    "Invoke-BodyguardScenario 'disposable-helpful-bodyguard'") &&
                qualification.Contains("-ReuseInstalledArtifact") &&
                qualification.Contains(
                    "Feature settings bytes were not restored exactly."),
                "Bodyguard scenarios are not registered in the guarded launcher or do not restore restart-gated settings exactly.");
            foreach (string token in new[]
            {
                "HelpfulDefenderTest", "VictimTest", "Kobold",
                "new RuleAttackWithWeapon(attacker, victim, weapon, 0)",
                "Rulebook.Trigger(attack)",
                "available-normal-hit", "available-confirmed-critical",
                "immediate-unavailable", "VictimHpLoss",
                "ProtectorHpLoss", "stage=rule-deal-damage-prefix",
                "decision=eligible", "decision=swift-cooldown-active"
            })
                Assertions.True(humanRepro.Contains(token),
                    "Human-equivalent In Harm's Way scenario lacks: " + token);
            Assertions.True(humanRepro.Contains("ArmCritical(incoming,") &&
                !humanRepro.Contains("AutoCriticalThreat") &&
                !humanRepro.Contains("TryCommitRedirection"),
                "Human repro fixture bypasses the native attack/critical/redirection path.");
            foreach (string token in new[]
            {
                "CombatController.IsInTurnBasedCombat()",
                "Game.Instance.TurnBasedCombatController.CurrentTurn",
                "ReferenceEquals(turn.Unit, _attacker)",
                "BodyguardActionEconomyAccess.ObserveImmediateAction(",
                "HasSwiftAction = snapshot.HasSwiftAction",
                "PendingNextTurn",
                "ChargedTurn",
                "turn.ForceToEnd(true)",
                "new RuleAttackWithWeapon(_attacker, _victim,",
                "Rulebook.Trigger(attack)",
                "value.BodyguardContribution == 4",
                "value.VictimHpAfter == value.VictimHpBefore",
                "value.ProtectorHpAfter < value.ProtectorHpBefore",
                "off-turn-confirmed-critical-intercepts"
            })
                Assertions.True(offTurnEconomy.Contains(token),
                    "Off-turn native economy scenario lacks: " + token);
            Assertions.True(!offTurnEconomy.Contains(
                    "TryCommitRedirection") &&
                !offTurnEconomy.Contains("TrySpendImmediateAction") &&
                !offTurnEconomy.Contains("Descriptor.Damage =") &&
                !offTurnEconomy.Contains("HasSwiftAction = true"),
                "Off-turn economy scenario manufactures availability, interception, or HP delivery.");
            Assertions.True(humanReproLauncher.Contains(
                    "3414D67CB2E5F8C4F18A952D23247DC6DD9D9F5579066EA64CA7FF29E61B8F01") &&
                humanReproLauncher.Contains(
                    "KMG_IHW_HUMAN_REPRO_COPY.zks") &&
                humanReproLauncher.Contains(
                    "The original human test save changed") &&
                humanReproLauncher.Contains("Remove-Item -LiteralPath $staged") &&
                humanReproLauncher.Contains("$sidecarPrefix") &&
                humanReproLauncher.Contains(
                    "A transaction-owned staged human-repro sidecar was not removed") &&
                humanReproLauncher.Contains(
                    "StartsWith($saveRootFull + '\\'") &&
                humanReproLauncher.Contains(
                    "disposable-in-harms-way-off-turn-economy") &&
                humanReproLauncher.Contains("Scenario = $Scenario") &&
                !humanReproLauncher.Contains("Remove-Item -Recurse"),
                "Human repro save transaction is not exact-hash guarded and self-cleaning.");
            Assertions.True(scenario.Contains(
                    "combat.AidGrantObservations[0].Contains(\";aidD20=20;\")") &&
                !scenario.Contains("aidRolls=20"),
                "Helpful runtime qualification must assert the observed native Aid d20 rather than a nonexistent queue-description field.");
            Assertions.True(fixture.Contains(
                    "its native zero-projectile synchronous resolve branch") &&
                fixture.Contains("new BlueprintProjectile[0]") &&
                fixture.Contains("attacker.Memory.Add(protector)") &&
                fixture.Contains("protector.LastMoveTime =") &&
                fixture.Contains("RemoveMemory(Attacker, ProtectorOne)") &&
                !fixture.Contains(
                    ".SetValue(visual, source.VisualParameters, null)") &&
                fixture.Contains("catch\n            {\n                Dispose();") &&
                fixture.Contains("if (_disposed) return;"),
                "The request-local ranged fixture can inherit projectiles or leak a partially constructed fixture.");
            string diceControl = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "BodyguardQualificationControl.cs");
            string misfirePatches = Read("src", "KingmakerGunslinger",
                "Misfires", "FirearmMisfirePatches.cs");
            Assertions.True(diceControl.Contains(
                    "BeforeEvaluateIncomingRoll") &&
                diceControl.Contains("<Roll>k__BackingField") &&
                diceControl.Contains("naturalRoll = forced") &&
                diceControl.Contains("roll.Override(naturalRoll)") &&
                misfirePatches.Contains(
                    "BeforeEvaluateIncomingRoll(__instance, ref d20)") &&
                misfirePatches.Contains(
                    "replace RuleAttackRoll's ordinary Roll property assignment"),
                "Guarded incoming-roll control lacks the exact direct-field-write compatibility fallback.");
            Assertions.True(diceControl.Contains(
                    "BodyguardCriticalConfirmationQualificationPatch") &&
                diceControl.Contains("set_CriticalConfirmationRoll") == false &&
                diceControl.Contains(
                    "GetNestedType(\"Dice\"") &&
                diceControl.Contains(
                    "TryOverrideCriticalConfirmation(") &&
                diceControl.Contains("attack.IsCriticalRoll") &&
                diceControl.Contains("context.CurrentEvent as RuleAttackRoll") &&
                diceControl.Contains("Patching the tiny") &&
                diceControl.Contains("setter proved vulnerable to JIT inlining") &&
                fixture.Contains("ArmCritical(incomingRoll, 20") &&
                fixture.Contains("ConfirmationRoll =") &&
                fixture.Contains("ConfirmationTotal =") &&
                !fixture.Contains("AutoCriticalThreat =") &&
                !fixture.Contains("AutoCriticalConfirmation ="),
                "The confirmed-critical fixture still bypasses the native confirmation roll/result path.");
            foreach (string caseName in new[]
            {
                "baseline", "bodyguard-hit-to-miss", "bodyguard-failure",
                "preauthorized-already-miss",
                "preauthorized-overwhelming-hit", "outside-threat",
                "inside-threat", "ranged-outside-threat",
                "ranged-inside-threat", "in-harms-way-mode-off",
                "in-harms-way-full-delivery", "no-immediate-action",
                "helpful-bodyguard-halfling-four-intercept",
                "helpful-bodyguard-halfling-four-critical",
                "multiple-protectors", "sequential-first",
                "sequential-second", "zero-damage-rider",
                "shield-other-on-interceptor", "shield-other-on-original",
                "module-disabled"
            })
                Assertions.True(scenario.Contains("\"" + caseName + "\""),
                    "Bodyguard runtime qualification lacks " + caseName + ".");
            Assertions.True(runner.Contains(
                    "BodyguardNativeContractObserver.Run") &&
                runner.Contains("AidAnotherCompatibilityObserver.Run") &&
                runner.Contains("BodyguardCombatScenario.Run") &&
                runner.Contains(
                    "feature-module-bodyguard-publication-gate") &&
                runner.Contains("bodyguardSet.Count == 9") &&
                runner.Contains("basicBodyguardFeatures ==") &&
                runner.Contains("fighterBodyguardAll ==") &&
                fixture.Contains("new RuleAttackWithWeapon(") &&
                fixture.Contains("Rulebook.Trigger(attack)") &&
                fixture.Contains("RuleSavingThrow") == false &&
                probe.Contains("new RuleSavingThrow(") &&
                probe.Contains("Rulebook.Trigger(saving)") &&
                probe.Contains("BodyguardQualificationDamageProbe") &&
                fixture.Contains("BodyguardCombatLog.Published") &&
                scenario.Contains("CombatLogLastMessage") &&
                scenario.Contains("DamageKinds.Any") &&
                scenario.Contains("DamageEvents.Length == 1") &&
                scenario.Contains("decision=swift-cooldown-active") &&
                scenario.Contains("confirmationConsumed=1") &&
                scenario.Contains("BodyguardSources") &&
                scenario.Contains("NativeAcBeforeBodyguard") &&
                scenario.Contains("fixture.ApplyShieldOther") &&
                scenario.Contains("global unit snapshot restored"),
                "Guarded Bodyguard qualification no longer uses live rule events, rider/save evidence, Shield Other ordering, and exact cleanup.");
        }

        internal static void AssociatedRidersUseTheFinalRecipient()
        {
            string bleeding = Read("src", "KingmakerGunslinger", "Deeds",
                "BleedingWoundRuntime.cs");
            Assertions.True(bleeding.Contains(
                    "BodyguardRuntime.ResolveDeliveryTarget(attack)") &&
                bleeding.Contains("deliveryTarget.Descriptor.Buffs.AddBuff(") &&
                !bleeding.Contains("attack.Target.Descriptor.Buffs.AddBuff("),
                "Project-owned Bleeding Wound still targets the original ally after interception.");
            string shield = Read("src", "KingmakerGunslinger", "Spells",
                "ShieldOther", "ShieldOtherDamagePatch.cs");
            string runtime = Read("src", "KingmakerGunslinger",
                "BodyguardFeats", "BodyguardRuntime.cs");
            Assertions.True(shield.Contains("RuleDealDamage") &&
                !runtime.Contains("ShieldOther") &&
                !runtime.Contains("shield-other"),
                "Shield Other was special-cased instead of remaining downstream of redirected delivery.");
        }

        internal static void InvestigationRecordsExactInstalledContracts()
        {
            string investigation = Read("docs", "investigations",
                "bodyguard-in-harms-way.md");
            foreach (string token in new[]
            {
                "2.1.7b",
                "3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB",
                "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7",
                "0f8939ae6f220984e8fb568abbdfba95",
                "AttackOfOpportunityCount",
                "HasSwiftAction",
                "UnitEngagementExtension.IsReach",
                "RuleCalculateAttackBonus",
                "RuleRollD20",
                "AbilityDeliveryTarget.set_AttackRoll",
                "RulebookEventContext.PopEvent",
                "ElementsContextData.Dispose",
                "RuleDealDamage-only",
                "Shield Other"
            })
                Assertions.True(investigation.Contains(token),
                    "Durable Bodyguard engine investigation lacks: " + token);
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
