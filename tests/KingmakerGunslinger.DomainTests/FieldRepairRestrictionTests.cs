using System;
using System.IO;
using System.Text;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Regression coverage for the mission Z-FIREARM-MAINTENANCE §4.1 field
    /// repair restriction: Repair Firearm is an out-of-combat, Broken-only
    /// maintenance action whose concrete target is bound at genuine command
    /// start and rechecked at delivery; Wrecked firearms are rest-only and
    /// every active user-facing text says so.
    /// </summary>
    internal static class FieldRepairRestrictionTests
    {
        private static FirearmDefinition Definition()
        {
            return KingmakerGunslinger.Firearms.FirearmDefinitions.CreateEarlyMusket();
        }

        private static FirearmState State(FirearmCondition condition)
        {
            return new FirearmState(
                FirearmState.CurrentSchemaVersion,
                condition == FirearmCondition.Broken ? 1 : 0,
                condition == FirearmCondition.Broken
                    ? FirearmStateTokenCatalog.DiagnosticLeadBall
                    : null,
                condition);
        }

        internal static void CombatBeatsEveryOtherEligibility()
        {
            foreach (FirearmCondition condition in new[] {
                FirearmCondition.Normal,
                FirearmCondition.Broken,
                FirearmCondition.Wrecked })
            {
                foreach (bool kit in new[] { false, true })
                {
                    FirearmActionDecision decision = FirearmActionPolicy.Evaluate(
                        FirearmActionKind.Repair, Definition(), State(condition),
                        kit, true);
                    Assertions.False(decision.IsAvailable,
                        "Combat must reject repair regardless of state or kit: " +
                        condition + "/" + kit);
                    Assertions.Equal("Cannot repair firearms during combat.", decision.Reason,
                        "Combat has one exact contextual rejection.");
                }
            }
        }

        internal static void WreckedIsRestOnly()
        {
            FirearmActionDecision decision = FirearmActionPolicy.Evaluate(
                FirearmActionKind.Repair, Definition(),
                State(FirearmCondition.Wrecked), true, false);
            Assertions.False(decision.IsAvailable,
                "A Wrecked firearm must never be field-repair eligible.");
            Assertions.Equal("This firearm is Wrecked. A full rest is required.", decision.Reason,
                "Wrecked has one exact contextual rejection.");
        }

        internal static void NormalAndMissingKitRejected()
        {
            FirearmActionDecision normal = FirearmActionPolicy.Evaluate(
                FirearmActionKind.Repair, Definition(),
                State(FirearmCondition.Normal), true, false);
            Assertions.False(normal.IsAvailable,
                "A Normal firearm must not be field-repair eligible.");
            FirearmActionDecision noKit = FirearmActionPolicy.Evaluate(
                FirearmActionKind.Repair, Definition(),
                State(FirearmCondition.Broken), false, false);
            Assertions.False(noKit.IsAvailable,
                "A Broken firearm without a kit must not be repair eligible.");
            Assertions.Equal("Requires a Gunsmith's Kit.", noKit.Reason,
                "The missing-kit rejection must name the reusable kit briefly.");
            Assertions.Equal("This firearm is not broken.", normal.Reason,
                "A Normal gun has its own contextual rejection.");
            FirearmActionDecision broken = FirearmActionPolicy.Evaluate(
                FirearmActionKind.Repair, Definition(),
                State(FirearmCondition.Broken), true, false);
            Assertions.True(broken.IsAvailable,
                "A Broken firearm with a kit outside combat remains repairable.");
        }

        internal static void ReloadPolicyUnchangedByRepairRestriction()
        {
            FirearmDecisionReloadRegression();
        }

        private static void FirearmDecisionReloadRegression()
        {
            FirearmDefinition definition = Definition();
            // An empty firearm of any condition reaches the reload decision;
            // reload must not inherit the repair-only combat restriction.
            FirearmState emptyBroken = new FirearmState(
                FirearmState.CurrentSchemaVersion,
                0,
                null,
                FirearmCondition.Broken);
            FirearmActionDecision reload = FirearmActionPolicy.Evaluate(
                FirearmActionKind.Reload, definition,
                new FirearmState(
                    FirearmState.CurrentSchemaVersion,
                    0,
                    null,
                    FirearmCondition.Wrecked), true, true);
            Assertions.False(reload.IsAvailable,
                "A Wrecked firearm still cannot reload.");
            FirearmActionDecision loaded = FirearmActionPolicy.Evaluate(
                FirearmActionKind.Reload, definition,
                emptyBroken, true, true);
            Assertions.True(loaded.IsAvailable,
                "Reload availability must not inherit the repair combat restriction.");
        }

        internal static void TransactionRejectsWreckedWithoutMutation()
        {
            // The transaction layer is the last line of defense: even a caller
            // that skipped the policy cannot restore a Wrecked item in the field.
            string source = Read("src/KingmakerGunslinger/Recovery",
                "FirearmRepairTransactionService.cs");
            Assertions.True(source.Contains(
                    "WreckedRequiresRest") &&
                source.IndexOf(
                    "state.Condition == FirearmCondition.Wrecked",
                    StringComparison.Ordinal) <
                source.IndexOf(
                    "state.Condition != FirearmCondition.Broken",
                    StringComparison.Ordinal),
                "The field-repair transaction must reject Wrecked before any other eligibility check.");
        }

        internal static void RuntimeUsesPartyCombatAuthority()
        {
            string source = Read("src/KingmakerGunslinger/Recovery",
                "RepairTestMusketRuntime.cs");
            Assertions.True(source.Contains("IsPartyInCombat") &&
                source.Contains("game.Player.IsInCombat"),
                "Field repair must use the native party-level combat authority, not a personal-engagement check.");
            Assertions.True(source.Contains(
                    "FirearmMaintenanceCapability.CanMaintainFirearms(caster)"),
                "Field repair availability must explicitly verify the Gunsmithing entitlement and ability to act (review R5).");
        }

        internal static void CapabilityEnforcedAtStartAndDelivery()
        {
            string capability = Read("src/KingmakerGunslinger/Recovery",
                "FirearmMaintenanceCapability.cs");
            Assertions.True(capability.Contains("IsDead") &&
                capability.Contains("!caster.State.IsConscious") &&
                capability.Contains("!caster.State.CanAct") &&
                capability.Contains("HasCondition(UnitCondition.Sleeping)") &&
                capability.Contains("HasFact(gunslinger.Gunsmithing)"),
                "Field capability must use the crafting IsConscious+CanAct idiom; rest capability must accept only transient camping sleep (CR2-03).");
            string binding = Read("src/KingmakerGunslinger/Recovery",
                "RepairCommandStartBinding.cs");
            Assertions.True(binding.Contains(
                    "FirearmMaintenanceCapability.CanMaintainFirearms(caster)") &&
                binding.Contains("IdentifyRepairCaster") &&
                binding.Contains("TryGetBoundWeaponForDelivery") &&
                binding.Contains("FindOwningCommand(context)") &&
                binding.Contains("ReferenceEquals(binding.Command, owner)") &&
                binding.Contains("EligibleAtStart") &&
                binding.Contains("binding.Command.IsFinished") &&
                binding.Contains("ReferenceEquals(binding.Command, endedCommand)") &&
                binding.Contains("BlueprintBootstrap.OverhaulTestMusketAbility"),
                "Command start must capture the caster before fallible work, enforce capability, and be command-owned; delivery must correlate with the exact owning native command; the legacy alias must be recognized (review R4/CR2-01).");
        }

        internal static void DeliveryRequiresCommandStartBinding()
        {
            string source = Read("src/KingmakerGunslinger/Recovery",
                "RepairTestMusketAbilityLogic.cs");
            Assertions.True(source.Contains(
                    "RepairCommandStartBinding.TryGetBoundWeaponForDelivery") &&
                source.Contains("ReferenceEquals(boundAtCommandStart, start.Weapon)"),
                "Delivery must verify the exact firearm bound at genuine command commencement.");
            string binding = Read("src/KingmakerGunslinger/Recovery",
                "RepairCommandStartBinding.cs");
            Assertions.True(binding.Contains(
                    "[HarmonyPatch(typeof(UnitUseAbility), \"OnStart\")]") &&
                binding.Contains(
                    "[HarmonyPatch(typeof(UnitUseAbility), \"OnEnded\")]") &&
                binding.Contains("BlueprintBootstrap.RepairTestMusketAbility") &&
                binding.Contains("RepairTestMusketRuntime.IsPartyInCombat") &&
                binding.Contains("FirearmCondition.Broken"),
                "The command-start hook must bind only the repair ability's exact Broken firearm outside combat.");
        }

        internal static void LegacyAliasStaysBrokenOnlyDelegate()
        {
            string alias = Read("src/KingmakerGunslinger/Blueprints",
                "OverhaulTestMusketAbilityBlueprints.cs");
            Assertions.True(alias.Contains("RepairTestMusketAbilityLogic.Create(") &&
                alias.Contains("outside combat") &&
                alias.Contains("completed full rest") &&
                !alias.Contains("OverhaulTestMusketAbilityLogic"),
                "The hidden legacy alias must delegate to the same Broken-only field-repair logic and say so.");
        }

        internal static void ActiveTextsNeverPromiseWreckedFieldRepair()
        {
            foreach (string[] parts in new[]
            {
                new[] { "src/KingmakerGunslinger/Blueprints", "RepairTestMusketAbilityBlueprints.cs" },
                new[] { "src/KingmakerGunslinger/Blueprints", "GunsmithingBlueprints.cs" },
                new[] { "src/KingmakerGunslinger/Blueprints", "GunsmithingSupplyBlueprints.cs" },
                new[] { "src/KingmakerGunslinger/Blueprints", "OverhaulTestMusketAbilityBlueprints.cs" },
                new[] { "src/KingmakerGunslinger/Recovery", "RepairTestMusketRuntime.cs" },
                new[] { "src/KingmakerGunslinger/Recovery", "RepairTestMusketAbilityLogic.cs" },
                new[] { "src/KingmakerGunslinger/Actions", "FirearmActionPolicy.cs" }
            })
            {
                string text = Read(parts);
                Assertions.False(
                    PromisesWreckedFieldRepair(text),
                    "Active user-facing text still promises unified Wrecked field repair: " +
                    parts[1]);
            }

            string manifest = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json"));
            // GS-REP-0001, recorded rather than silently fixed. This assertion
            // matches only the space-separated spelling. The manifest's entry
            // for the Gunsmith's Kit carries the HYPHENATED form in its notes
            // ("unified Broken-or-Wrecked-to-Normal firearm repair"), which
            // slips past it. That text is stale: the field-repair action is
            // Broken-only.
            //
            // It is deliberately not corrected here. That notes field sits
            // inside the accepted 1913-entry prefix whose exact bytes -
            // symbol, guid, plannedType, status, milestone AND notes - are
            // pinned by BetterVendorsProgressionTests.ManifestPrefixSha256.
            // Editing it means re-pinning an accepted block, which needs an
            // approved catalog change, not a repair-campaign edit. Moving that
            // pin to make a text correction pass would defeat the guard that
            // protects released identities.
            //
            // The field is developer metadata, not a player-facing string;
            // every shipped player-facing description was checked and is
            // already correct. So this stays a known, documented gap with a
            // named prerequisite. Normalising this particular assertion would
            // turn it red until that approved change happens.
            Assertions.False(manifest.Contains("Broken or Wrecked"),
                "The blueprint manifest still promises unified Wrecked field repair.");
        }

        /// <summary>
        /// True when the text promises that the field-repair action handles a
        /// Wrecked firearm. Hyphens, non-breaking hyphens and runs of
        /// whitespace are all normalised to single spaces first, because the
        /// same claim ships in both "Broken or Wrecked" and
        /// "Broken-or-Wrecked-to-Normal" forms and only the first was caught.
        /// This deliberately does not ban the phrase everywhere: a completed
        /// full rest legitimately restores Broken AND Wrecked, so only the
        /// field-repair promises are matched.
        /// </summary>
        private static bool PromisesWreckedFieldRepair(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            var builder = new StringBuilder(text.Length);
            bool pendingSeparator = false;
            foreach (char character in text)
            {
                if (char.IsWhiteSpace(character) || character == '-' ||
                    character == '‐' || character == '‑')
                {
                    pendingSeparator = builder.Length > 0;
                    continue;
                }
                if (pendingSeparator) { builder.Append(' '); pendingSeparator = false; }
                builder.Append(character);
            }
            string normalized = builder.ToString();
            return normalized.Contains("repair a Broken or Wrecked") ||
                normalized.Contains("Broken or Wrecked firearm to Normal") ||
                normalized.Contains("Broken or Wrecked firearm straight to Normal") ||
                normalized.Contains("Broken or Wrecked to Normal firearm repair") ||
                normalized.Contains("unified Broken or Wrecked to Normal");
        }

        internal static void QuickClearRouteStaysCombatUsable()
        {
            string quickClear = Read("src/KingmakerGunslinger/Deeds",
                "QuickClearRuntime.cs");
            Assertions.True(quickClear.Contains("FirearmStateMachine.Repair(before)") &&
                !quickClear.Contains("IsPartyInCombat"),
                "Quick Clear must keep its direct shared-transition combat route without any field-repair combat gate.");
            string stateMachine = Read("src/KingmakerGunslinger/Firearms",
                "FirearmStateMachine.cs");
            Assertions.True(stateMachine.Contains(
                    "internal static FirearmState Repair(FirearmState state)"),
                "The shared low-level repair transition must remain available for combat recovery.");
        }

        private static string Read(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in parts) path = Path.Combine(path, part);
            return File.ReadAllText(path);
        }
    }
}
