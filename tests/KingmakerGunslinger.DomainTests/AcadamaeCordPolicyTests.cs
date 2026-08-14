using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Acadamae;
using KingmakerGunslinger.Cord;

namespace KingmakerGunslinger.DomainTests
{
    internal static class AcadamaeCordPolicyTests
    {
        internal static void AcadamaeEligibilityMatrix()
        {
            AcadamaeCastRequest valid = Valid();
            AssertEligible(valid, AcadamaeCastingTime.Standard, 0, 18);
            Mutate(valid, r => r.HasFeat = false, "no-feat");
            Mutate(valid, r => r.AccelerationModeActive = false, "mode-disabled");
            Mutate(valid, r => r.IsRealSpell = false, "not-spell");
            Mutate(valid, r => r.HasSpellbook = false, "no-spellbook");
            Mutate(valid, r => r.IsPreparedInvocation = false, "not-prepared");
            Mutate(valid, r => r.IsArcane = false, "not-arcane");
            Mutate(valid, r => r.IsConjuration = false, "not-conjuration");
            Mutate(valid, r => r.IsSummoning = false, "not-summoning");
            valid.EffectiveCastingTime = AcadamaeCastingTime.Standard;
            Assertions.Equal("already-standard-or-faster",
                AcadamaeCastingPolicy.Decide(valid).Status,
                "Standard spells must be ineligible.");
            valid.EffectiveCastingTime = AcadamaeCastingTime.Swift;
            Assertions.False(AcadamaeCastingPolicy.Decide(valid).Eligible,
                "Quickened spells must be ineligible.");
        }

        internal static void AcadamaeModeAndFatigueSourceContracts()
        {
            string mode = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "AcadamaeGraduateModeBlueprints.cs"));
            foreach (string token in new[] {
                "KMG.Feats.AcadamaeGraduateModeMarker",
                "KMG.Feats.UseAcadamaeGraduate",
                "IsOnByDefault = false",
                "AbilityActivationType.Immediately",
                "DeactivateIfCombatEnded = false",
                "DeactivateAfterFirstRound = false",
                "DeactivateImmediately = false",
                "DeactivateIfOwnerDisabled = false",
                "DeactivateIfOwnerUnconscious = false",
                "OnlyInCombat = false",
                "ActionBarAutoFillIgnored = false",
                "marker.FxOnStart = new PrefabLink()",
                "marker.FxOnRemove = new PrefabLink()",
                "marker.ResourceAssetIds = Array.Empty<string>()" })
                Assertions.True(mode.Contains(token),
                    "Acadamae mode blueprint contract lacks exact token: " + token);

            string feature = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "AcadamaeGraduateBlueprints.cs"));
            Assertions.True(feature.Contains("grant.DoNotRestoreMissingFacts = false") &&
                feature.Contains("grant.Facts = new BlueprintUnitFact[] { ability }") &&
                feature.Contains("Activate Use Acadamae Graduate"),
                "Acadamae feat must restore and describe its native mode grant.");

            string casting = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Acadamae",
                "AcadamaeCastingPatches.cs"));
            Assertions.True(casting.Contains("AccelerationModeActive =") &&
                casting.Contains("AcadamaeGraduateMode.Marker") &&
                casting.Contains("Buffs.AddBuff(\n                    _fatigued, rule.Initiator, null)") &&
                casting.Contains("fatigue.MakePermanent()") &&
                casting.Contains("AcadamaeSavingThrowTestCompletionPatch") &&
                casting.Contains("__instance.BaseRollResult = naturalRoll") &&
                casting.Contains("if (naturalRoll == 20) __instance.AutoPass = true") &&
                !casting.Contains("AddBuff(_fatigued, rule.Context"),
                "Acadamae must require the exact unit marker and use independent ordinary fatigue context.");
        }

        internal static void AcadamaeModeIdentityContracts()
        {
            string manifest = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json"));
            Assertions.True(manifest.Split(new[] {
                "KMG.Feats.AcadamaeGraduateModeMarker" }, StringSplitOptions.None).Length == 2 &&
                manifest.Split(new[] { "KMG.Feats.UseAcadamaeGraduate" },
                    StringSplitOptions.None).Length == 2,
                "The repair must append exactly two mode identities.");
            foreach (string identity in new[] {
                "7939ff087cb843729448589ba2de19f1",
                "c4b804d9ebf941b4842b0a461a2b6b6d",
                "b5fc52ec666640318f8921d5fa60ec39",
                "a780ab99b76849ed825729808e2bbf29" })
                Assertions.True(manifest.Contains(identity),
                    "Required stable repair identity is missing: " + identity);
            string bootstrap = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Bootstrap", "BlueprintBootstrap.cs"));
            Assertions.True(bootstrap.Contains(
                "internal const int ExpectedRegisteredBlueprintCount = 327 +") &&
                bootstrap.Contains("AcadamaeGraduateBlueprints.AttachMode") &&
                bootstrap.Contains("AcadamaeGraduateModeBlueprints.Register"),
                "Bootstrap must derive the current active identity count and attach the mode grant.");
        }

        internal static void CordProjectIconContract()
        {
            string root = Environment.CurrentDirectory;
            string source = Path.Combine(root, "assets-source", "original-icons",
                "cord-of-stubborn-resolve", "cord-of-stubborn-resolve-chroma-source.png");
            string production = Path.Combine(root, "assets", "game", "icons",
                "cord-of-stubborn-resolve.png");
            Assertions.True(File.Exists(source) && File.Exists(production),
                "Cord source and production icons must exist.");
            byte[] png = File.ReadAllBytes(production);
            Assertions.True(png.Length > 24 && png[12] == 0x49 && png[13] == 0x48 &&
                png[14] == 0x44 && png[15] == 0x52 &&
                ReadBigEndian(png, 16) == 128 && ReadBigEndian(png, 20) == 128 &&
                (png[25] == 4 || png[25] == 6),
                "Cord production art must be a 128x128 PNG with alpha.");
            string hash;
            using (SHA256 sha = SHA256.Create())
                hash = string.Concat(sha.ComputeHash(png).Select(value =>
                    value.ToString("x2")).ToArray());
            Assertions.Equal("cf3f040eb22691b1e526eb32cc31d1151eafef7113cb0ebe55d0c2637d5d9928",
                hash, "Cord production hash must match its provenance record.");
            string[] duplicates = Directory.GetFiles(Path.Combine(root, "assets", "game", "icons"),
                "*.png").Where(path => !string.Equals(path, production,
                    StringComparison.OrdinalIgnoreCase) &&
                    File.ReadAllBytes(path).SequenceEqual(png)).ToArray();
            Assertions.Equal(0, duplicates.Length,
                "Cord production art must not duplicate another project icon.");
            string iconSource = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints", "ProjectAssetIcons.cs"));
            Assertions.True(iconSource.Contains("\"cord-of-stubborn-resolve\"") &&
                iconSource.Contains("items.SetIcon(cordOfStubbornResolve, cordIcon)") &&
                iconSource.Contains("ReferenceEquals(cordDonorIcon, cordIcon)"),
                "Cord must receive and validate its project sprite after donor cloning.");
        }

        internal static void AcadamaeMultiRoundAndDc()
        {
            AcadamaeCastRequest request = Valid();
            request.EffectiveCastingTime = AcadamaeCastingTime.MultipleRounds;
            request.EffectiveRounds = 3;
            request.SpellLevel = 9;
            AssertEligible(request, AcadamaeCastingTime.MultipleRounds, 2, 24);
            request.EffectiveRounds = 2;
            AssertEligible(request, AcadamaeCastingTime.FullRound, 1, 24);
            request.EffectiveRounds = 1;
            Assertions.Throws<ArgumentException>(() =>
                AcadamaeCastingPolicy.Decide(request),
                "Invalid multi-round representation must fail closed.");
        }

        internal static void CordFatigueAndExhaustion()
        {
            CordSubstitutionDecision absent = CordSubstitutionPolicy.Decide(false,
                CordConditionKind.Fatigue, 6, 20, false);
            Assertions.False(absent.Substituted, "Inventory-only Cord must do nothing.");
            CordSubstitutionDecision fatigue = CordSubstitutionPolicy.Decide(true,
                CordConditionKind.Fatigue, 6, 20, false);
            Assertions.True(fatigue.Substituted && fatigue.Damage == 6 &&
                !fatigue.ApplyFatigue, "Fatigue substitution contract failed.");
            CordSubstitutionDecision exhaustion = CordSubstitutionPolicy.Decide(true,
                CordConditionKind.Exhaustion, 1, 20, false);
            Assertions.True(exhaustion.Substituted && exhaustion.Damage == 1 &&
                exhaustion.ApplyFatigue, "Exhaustion downgrade contract failed.");
        }

        internal static void AcadamaePrerequisiteMatrix()
        {
            var request = new AcadamaePrerequisiteRequest {
                CommittedWizardLevel = 1, HasSpecialistSchool = true };
            AssertPrerequisite(request, true, "eligible", 1);
            request.CommittedWizardLevel = 0; request.PendingWizardLevels = 1;
            request.HasSpecialistSchool = false; request.PendingSpecialistSchool = true;
            AssertPrerequisite(request, true, "eligible", 1);
            request.PendingWizardLevels = 0;
            AssertPrerequisite(request, false, "wizard-level-required", 0);
            request.PendingWizardLevels = 1; request.PendingUniversalist = true;
            AssertPrerequisite(request, false, "universalist-ineligible", 1);
            request.PendingUniversalist = false; request.PendingConjurationForbidden = true;
            AssertPrerequisite(request, false, "conjuration-forbidden", 1);
            request.PendingConjurationForbidden = false; request.GivesUpSpecialization = true;
            AssertPrerequisite(request, false, "specialization-replaced", 1);
        }

        internal static void AcadamaeNativeIdentityContracts()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "AcadamaeGraduateBlueprints.cs"));
            Assertions.True(source.Contains(
                "BlueprintProgression universalist = BlueprintLibraryLookup.RequireExact<BlueprintProgression>"),
                "The exact installed Universalist identity is a BlueprintProgression.");
            Assertions.True(source.Contains(
                "BlueprintProgression iconDonor = BlueprintLibraryLookup.RequireExact<BlueprintProgression>"),
                "The exact installed Conjuration specialization donor is a BlueprintProgression.");
            Assertions.True(source.Contains(
                "BlueprintLibraryLookup.RequireExact<BlueprintBuff>"),
                "Acadamae fatigue must use the exact native BlueprintBuff contract.");
            string prerequisite = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Acadamae", "PrerequisiteAcadamaeGraduate.cs"));
            Assertions.True(prerequisite.Contains("SelectedFeatures(state,") &&
                prerequisite.Contains(
                    "OppositionSelection).Any(IsConjurationOpposition)"),
                "Pending prerequisites must inspect every selected opposition school.");
            string casting = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Acadamae",
                "AcadamaeCastingPatches.cs"));
            foreach (string token in new[] {
                "AcadamaeSavingThrowTestControl.Begin(saving)",
                "finally { AcadamaeSavingThrowTestControl.End(); }",
                "[HarmonyPatch(typeof(RuleRollD20), \"PreRollDice\")]",
                "AcadamaeSavingThrowTestControl.TryConsume(out naturalRoll)",
                "__result = naturalRoll" })
                Assertions.True(casting.Contains(token),
                    "Guarded Acadamae saving-throw control lacks exact token: " + token);
            string runtime = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs"));
            int forcedFailure = runtime.IndexOf(
                "GetStat(StatType.SaveFortitude).BaseValue = -100;",
                StringComparison.Ordinal);
            int cordCast = runtime.IndexOf(
                "AbilityData cordCast = PrepareAcadamaeSpell",
                StringComparison.Ordinal);
            Assertions.True(forcedFailure >= 0 && cordCast > forcedFailure,
                "The clean-first Cord integration phase must force a failed Fortitude save before casting.");
            Assertions.True(runtime.Split(new[] { "AcadamaeSavingThrowTestControl.Queue(20)" },
                    StringSplitOptions.None).Length == 3 &&
                runtime.Split(new[] { "AcadamaeSavingThrowTestControl.Queue(1)" },
                    StringSplitOptions.None).Length == 3,
                "The guarded scenario must force native automatic success and failure boundaries.");
            foreach (string token in new[] {
                "canonical Summon Monster I parent for Acadamae fixture",
                "ExpandedSummoningCatalog",
                "ExpandedSummoningIdentityCatalog.AbilitySymbol(dog)",
                "new AbilityData(slot.Spell, concrete)",
                "invocation.ParamSpellSlot = slot",
                "Canonical Summon Monster I has no published KMG Dog logical root" })
                Assertions.True(runtime.Contains(token),
                    "Acadamae runtime fixture no longer resolves the exact player-facing KMG summon variant: " +
                    token);
            foreach (string token in new[] {
                "IsPreparedInvocation(ability, spellbook)",
                "slot == null || !slot.Available || slot.Spell == null",
                "ReferenceEquals(slot.Spell.Spellbook, spellbook)",
                "current = current.ConvertedFrom",
                "ReferenceEquals(current, slot.Spell)" })
                Assertions.True(casting.Contains(token),
                    "Acadamae prepared-variant eligibility contract is missing: " +
                    token);
        }

        internal static void AcadamaeInvocationCorrelation()
        {
            var tracker = new AcadamaeInvocationTracker<object, object>();
            object commandA = new object(), commandB = new object();
            object spellA = new object(), sameLookingSpell = new object();
            Assertions.True(tracker.Arm(commandA, spellA), "First command must arm.");
            Assertions.False(tracker.Arm(commandA, spellA), "Repeated UI/constructor work must not double-arm.");
            Assertions.True(tracker.Arm(commandB, sameLookingSpell), "Second command must remain isolated.");
            Assertions.True(tracker.Begin(commandA), "Armed command must begin.");
            Assertions.False(tracker.ConsumeSuccessful(sameLookingSpell),
                "A distinct prepared invocation must not consume the active marker.");
            Assertions.True(tracker.ConsumeSuccessful(spellA),
                "The exact successful spell invocation must consume once.");
            Assertions.False(tracker.ConsumeSuccessful(spellA),
                "A successful invocation must not consume twice.");
            Assertions.Equal(1, tracker.Count, "The other command marker must remain.");
            Assertions.True(tracker.Begin(commandB), "Second command must begin independently.");
            tracker.EndAction(commandB);
            Assertions.False(tracker.ConsumeSuccessful(sameLookingSpell),
                "A cast observed after action scope must not consume.");
            Assertions.True(tracker.Cancel(commandB), "Cancellation must clear the pending marker.");
            Assertions.Equal(0, tracker.Count, "No marker may leak after cancellation.");
        }

        internal static void CordDamageBoundaries()
        {
            Assertions.Equal(0, CordSubstitutionPolicy.Decide(true,
                CordConditionKind.Fatigue, 6, 1, false).Damage,
                "Fallback damage must preserve 1 HP.");
            Assertions.Equal(2, CordSubstitutionPolicy.Decide(true,
                CordConditionKind.Fatigue, 6, 3, false).Damage,
                "Fallback damage must cap at HP minus one.");
            Assertions.Equal(6, CordSubstitutionPolicy.Decide(true,
                CordConditionKind.Fatigue, 6, 1, true).Damage,
                "Native nonlethal must retain the full roll.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                CordSubstitutionPolicy.Decide(true, CordConditionKind.Fatigue,
                    0, 10, false), "A d6 roll below one must fail.");
        }

        internal static void CordNativeConditionSourceContract()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Cord", "CordConditionPatches.cs"));
            foreach (string token in new[] {
                "[HarmonyPatch(typeof(UnitState), \"AddCondition\"",
                "ReferenceEquals(state.Owner.Unit.Body.Belt.Item.Blueprint",
                "BlueprintBootstrap.CordOfStubbornResolve",
                "new RuleRollDice(state.Owner.Unit",
                "state.Owner.Unit.HPLeft - 1",
                "new DiceFormula(0, DiceType.D6), amount",
                "IgnoreDamageReduction = true",
                "IWarningNotificationUIHandler",
                "[HarmonyPatch(typeof(BuffCollection), \"TriggerRuleApplyBuff\"",
                ".OfType<AddCondition>()",
                "component.Condition",
                "CordConditionRuntime.BeginBuff(__instance, __0,",
                "return !skipOriginal",
                "CordConditionRuntime.EndBuff(__state)",
                "ReferenceEquals(blueprint, _fatiguedBlueprint)",
                "skipOriginal = true",
                "[System.ThreadStatic] private static UnitState _fatigueBypass",
                "[System.ThreadStatic] private static UnitState _buffSubstitutionState",
                "ConditionalWeakTable<Buff, object> ExhaustionSources",
                "state.AddCondition(UnitCondition.Fatigued, source)" })
                Assertions.True(source.Contains(token),
                    "Cord native condition hook lacks exact token: " + token);
        }

        private static AcadamaeCastRequest Valid()
        {
            return new AcadamaeCastRequest { HasFeat = true,
                AccelerationModeActive = true, IsRealSpell = true,
                HasSpellbook = true, IsPreparedInvocation = true, IsArcane = true,
                IsConjuration = true, IsSummoning = true,
                EffectiveCastingTime = AcadamaeCastingTime.FullRound,
                EffectiveRounds = 1, SpellLevel = 3 };
        }

        private static int ReadBigEndian(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) |
                (bytes[offset + 2] << 8) | bytes[offset + 3];
        }

        private static void Mutate(AcadamaeCastRequest original,
            Action<AcadamaeCastRequest> mutation, string status)
        {
            AcadamaeCastRequest request = Valid(); mutation(request);
            AcadamaeCastDecision decision = AcadamaeCastingPolicy.Decide(request);
            Assertions.True(!decision.Eligible && decision.Status == status,
                "Acadamae rejection status mismatch: " + status);
        }

        private static void AssertEligible(AcadamaeCastRequest request,
            AcadamaeCastingTime time, int rounds, int dc)
        {
            AcadamaeCastDecision decision = AcadamaeCastingPolicy.Decide(request);
            Assertions.True(decision.Eligible && decision.ResultingTime == time &&
                decision.ResultingRounds == rounds && decision.FortitudeDc == dc,
                "Acadamae eligible decision mismatch.");
        }

        private static void AssertPrerequisite(AcadamaePrerequisiteRequest request,
            bool eligible, string status, int level)
        {
            AcadamaePrerequisiteDecision result = AcadamaePrerequisitePolicy.Decide(request);
            Assertions.True(result.Eligible == eligible && result.Status == status &&
                result.EffectiveWizardLevel == level,
                "Acadamae prerequisite mismatch: " + status);
        }
    }
}
