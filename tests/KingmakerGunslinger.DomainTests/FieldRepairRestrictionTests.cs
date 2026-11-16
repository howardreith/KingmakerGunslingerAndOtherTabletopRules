using System;
using System.IO;
using System.Linq;
using System.Text;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json.Linq;

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

            // GS-REP-0001. The Gunsmith's Kit record's note used to say the
            // kit serves "unified Broken-or-Wrecked-to-Normal firearm
            // repair". Only the space-separated spelling was checked here,
            // so the hyphenated one slipped past. The note is corrected
            // under the owner's authorization of 2026-11-16 (INT-07), with
            // the historical prefix guarded by the one-record exception in
            // BetterVendorsProgressionTests.AuthorizedNoteException, and the
            // semantic check now reads the note itself and every active
            // record's note.
            JToken[] entries = ((JArray)JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"])
                .ToArray();
            JToken kit = entries.Single(value => (string)value["symbol"] ==
                BetterVendorsProgressionTests.AuthorizedNoteException.Symbol);
            Assertions.False(PromisesWreckedFieldRepair((string)kit["notes"]),
                "The Gunsmith's Kit manifest note still promises Wrecked field repair: " +
                (string)kit["notes"]);
            foreach (JToken entry in entries.Where(value => (string)value["status"] == "active"))
                Assertions.False(PromisesWreckedFieldRepair((string)entry["notes"]),
                    "An active manifest note promises Wrecked field repair: " +
                    (string)entry["symbol"]);

            // The check tells the two claims apart: field repair of a
            // Wrecked firearm is the promise it refuses, in either
            // spelling; a completed full rest restoring Wrecked firearms is
            // true and stays sayable.
            Assertions.True(PromisesWreckedFieldRepair(
                    BetterVendorsProgressionTests.AuthorizedNoteException.FrozenNote) &&
                PromisesWreckedFieldRepair(
                    "required for unified Broken or Wrecked to Normal firearm repair"),
                "The stale note is no longer recognized as a Wrecked field-repair promise.");
            Assertions.False(PromisesWreckedFieldRepair(
                    BetterVendorsProgressionTests.AuthorizedNoteException.ApprovedNote) ||
                PromisesWreckedFieldRepair(
                    "A completed full rest restores carried Broken and Wrecked firearms to Normal.") ||
                PromisesWreckedFieldRepair(
                    "Repairs an equipped Broken firearm straight to Normal. A Wrecked firearm is restored only by a completed full rest."),
                "An accurate description of completed-rest recovery was read as a field-repair promise.");
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
