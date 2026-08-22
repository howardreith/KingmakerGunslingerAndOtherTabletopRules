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
                "TryRestoreImmediateAction"
            })
                Assertions.True(source.Contains(token),
                    "Native action-economy adapter lacks: " + token);
            Assertions.True(!source.Contains("AttackOfOpportunity(target") &&
                !source.Contains("new UnitAttackOfOpportunity") &&
                !source.Contains("InHarmUsedThisRound") &&
                !source.Contains("Dictionary<UnitEntityData"),
                "Bodyguard created an attack command or custom action counter.");
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
                runtime.Contains("checked(before + bonus)") &&
                runtime.Contains("FramesByRoll") && runtime.Contains(
                    "[ThreadStatic]"),
                "Attack-scoped preauthorization/AC/frame ordering changed.");
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
                runtime.Contains(
                    "BodyguardActionEconomyAccess.TryRestoreImmediateAction(") &&
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
            foreach (string id in new[]
            {
                "observe-bodyguard-native-contracts",
                "disposable-bodyguard-feats",
                "disposable-bodyguard-feats-disabled"
            })
                Assertions.True(catalog.Contains(id),
                    "Guarded runtime catalog lacks " + id + ".");
            Assertions.True(automation.Contains(
                    "'observe-bodyguard-native-contracts'") &&
                automation.Contains("'disposable-bodyguard-feats'") &&
                automation.Contains(
                    "'disposable-bodyguard-feats-disabled'") &&
                qualification.Contains("Set-BodyguardFeatureState $true") &&
                qualification.Contains("Set-BodyguardFeatureState $false") &&
                qualification.Contains("-ReuseInstalledArtifact") &&
                qualification.Contains(
                    "Feature settings bytes were not restored exactly."),
                "Bodyguard scenarios are not registered in the guarded launcher or do not restore restart-gated settings exactly.");
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
            foreach (string caseName in new[]
            {
                "baseline", "bodyguard-hit-to-miss", "bodyguard-failure",
                "preauthorized-already-miss",
                "preauthorized-overwhelming-hit", "outside-threat",
                "inside-threat", "ranged-outside-threat",
                "ranged-inside-threat", "in-harms-way-mode-off",
                "in-harms-way-full-delivery", "no-immediate-action",
                "multiple-protectors", "sequential-first",
                "sequential-second", "zero-damage-rider",
                "shield-other-on-interceptor", "shield-other-on-original",
                "module-disabled"
            })
                Assertions.True(scenario.Contains("\"" + caseName + "\""),
                    "Bodyguard runtime qualification lacks " + caseName + ".");
            Assertions.True(runner.Contains(
                    "BodyguardNativeContractObserver.Run") &&
                runner.Contains("BodyguardCombatScenario.Run") &&
                fixture.Contains("new RuleAttackWithWeapon(") &&
                fixture.Contains("Rulebook.Trigger(attack)") &&
                fixture.Contains("RuleSavingThrow") == false &&
                probe.Contains("new RuleSavingThrow(") &&
                probe.Contains("Rulebook.Trigger(saving)") &&
                probe.Contains("BodyguardQualificationDamageProbe") &&
                fixture.Contains("BodyguardCombatLog.Published") &&
                scenario.Contains("CombatLogLastMessage") &&
                scenario.Contains("DamageKinds.Any") &&
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
