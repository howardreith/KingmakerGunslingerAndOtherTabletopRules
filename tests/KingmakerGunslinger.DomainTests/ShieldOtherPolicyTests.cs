using System;
using System.IO;
using System.Collections.Generic;
using KingmakerGunslinger.Spells.ShieldOther;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ShieldOtherPolicyTests
    {
        internal static void DamageSplitBoundariesAndConservation()
        {
            int[] values = { 0, 1, 2, 3, 1000001 };
            foreach (int value in values)
            {
                ShieldOtherDamageSplit split = ShieldOtherDamageSplitPolicy.Split(
                    value, true, false);
                Assertions.Equal(value / 2, split.SubjectShare,
                    "Subject share must be floor(D/2).");
                Assertions.Equal(value - (value / 2), split.CasterShare,
                    "Odd remainder must belong to the caster.");
                Assertions.Equal(value, split.SubjectShare + split.CasterShare,
                    "Shield Other damage was not conserved.");
            }
        }

        internal static void DamageSplitGuards()
        {
            ShieldOtherDamageSplit invalid = ShieldOtherDamageSplitPolicy.Split(7,
                false, false);
            ShieldOtherDamageSplit transfer = ShieldOtherDamageSplitPolicy.Split(7,
                true, true);
            Assertions.True(invalid.SubjectShare == 7 && invalid.CasterShare == 0 &&
                invalid.Status == "invalid-link", "Invalid links must not split.");
            Assertions.True(transfer.SubjectShare == 7 && transfer.CasterShare == 0 &&
                transfer.Status == "transferred-event",
                "Transferred events must not recursively split.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ShieldOtherDamageSplitPolicy.Split(-1, true, false),
                "Negative finalized damage must fail closed.");
        }

        internal static void LinkValidityMatrix()
        {
            var valid = Request();
            Assertions.True(ShieldOtherLinkValidityPolicy.Evaluate(valid).Valid,
                "Complete established link must be valid.");
            AssertInvalid("subject-missing", value => value.SubjectPresent = false);
            AssertInvalid("caster-missing", value => value.CasterPresent = false);
            AssertInvalid("caster-level-missing", value => value.CasterLevel = 0);
            AssertInvalid("caster-dead", value => value.CasterAlive = false);
            AssertInvalid("different-area", value => value.SameArea = false);

            ShieldOtherLinkValidityRequest distant = Request();
            distant.DistanceFeet = 10000f;
            Assertions.True(ShieldOtherLinkValidityPolicy.Evaluate(distant).Valid,
                "Close range limits initial targeting, not an established bond.");

            ShieldOtherLinkValidityRequest unavailableDistance = Request();
            unavailableDistance.DistanceFeet = float.NaN;
            Assertions.True(
                ShieldOtherLinkValidityPolicy.Evaluate(unavailableDistance).Valid,
                "Established-link validity must not depend on distance telemetry.");
        }

        internal static void CloseRangeScaling()
        {
            Assertions.Equal(25, ShieldOtherLinkValidityPolicy.CloseRangeFeet(1),
                "Caster level 1 close range changed.");
            Assertions.Equal(30, ShieldOtherLinkValidityPolicy.CloseRangeFeet(2),
                "Caster level 2 close range changed.");
            Assertions.Equal(35, ShieldOtherLinkValidityPolicy.CloseRangeFeet(5),
                "Caster level 5 close range changed.");
            ShieldOtherLinkValidityRequest boundary = Request();
            boundary.CasterLevel = 5; boundary.DistanceFeet = 35f;
            Assertions.True(ShieldOtherLinkValidityPolicy.Evaluate(boundary).Valid,
                "Established link must remain valid at the casting-range boundary.");
        }

        internal static void BlueprintIdentityAndContractSource()
        {
            string root = Environment.CurrentDirectory;
            string manifest = File.ReadAllText(Path.Combine(root, "blueprints",
                "blueprints.json"));
            string source = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints", "ShieldOtherBlueprints.cs"));
            foreach (string token in new[] {
                "KMG.Spells.ShieldOther.Ability",
                "6a8c4c1d2fbe4d6a9a724988c1348401",
                "KMG.Spells.ShieldOther.TargetBuff",
                "7bd92e3c44ad42e7b523ee8ed7afc602" })
                Assertions.True(manifest.Contains(token),
                    "Shield Other manifest token is missing: " + token);
            foreach (string token in new[] {
                "result.ComponentsArray = fx == null",
                "SpellSchool.Abjuration", "AbilityRange.Close",
                "result.CanTargetSelf = false", "DurationRate.Hours",
                "ContextRankBaseValueType.CasterLevel",
                "ModifierDescriptor.Deflection",
                "ModifierDescriptor.Resistance", "StackingType.Replace",
                "new BlueprintAbility.MaterialComponentData()",
                "ProjectAssetIcons.RequireIcon(\"shield-other\")",
                "ability.MaterialComponent == null",
                "ability.MaterialComponent.Item != null" })
                Assertions.True(source.Contains(token),
                    "Shield Other blueprint contract token is missing: " + token);
            Assertions.False(source.Contains("MaterialComponent = null"),
                "Shield Other must not expose a null material contract.");
            string icon = Path.Combine(root, "assets", "game", "icons",
                "shield-other.png");
            Assertions.True(File.Exists(icon),
                "Shield Other project icon is missing.");
            byte[] png = File.ReadAllBytes(icon);
            Assertions.True(png.Length > 24 && png[12] == 0x49 && png[13] == 0x48 &&
                png[14] == 0x44 && png[15] == 0x52 &&
                ReadBigEndian(png, 16) == 128 && ReadBigEndian(png, 20) == 128,
                "Shield Other production art must be a 128x128 PNG.");
        }

        internal static void SpellListMergeAndRollbackPolicy()
        {
            var foreign = new FakeSpell("foreign");
            var shield = new FakeSpell("shield");
            var duplicateGuid = new FakeSpell("shield");
            var current = new List<FakeSpell> { foreign, shield, duplicateGuid };
            List<FakeSpell> published = ShieldOtherSpellListMergePolicy.Merge(
                current, shield, value => value.Guid);
            Assertions.True(published.Count == 2 &&
                ReferenceEquals(published[0], foreign) &&
                ReferenceEquals(published[1], shield),
                "Publication must preserve foreign order and singularize by reference/GUID.");
            List<FakeSpell> second = ShieldOtherSpellListMergePolicy.Merge(
                published, shield, value => value.Guid);
            Assertions.True(ReferenceEquals(second, published) && second.Count == 2 &&
                ReferenceEquals(second[0], foreign) &&
                ReferenceEquals(second[1], shield),
                "Repeated reconciliation must be idempotent.");
            Assertions.True(ShieldOtherSpellListMergePolicy.CanRollback(
                published, published), "Unchanged published list must permit rollback.");
            var foreignReplacement = new List<FakeSpell>(published);
            Assertions.False(ShieldOtherSpellListMergePolicy.CanRollback(
                foreignReplacement, published),
                "Later foreign list replacement must refuse rollback.");
            Assertions.Throws<InvalidOperationException>(() =>
                ShieldOtherSpellListMergePolicy.Merge(
                    new List<FakeSpell> { null }, shield, value => value.Guid),
                "Null native/foreign entries must fail closed.");
        }

        internal static void BasePublicationSourceContract()
        {
            string root = Environment.CurrentDirectory;
            string publication = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "ShieldOtherSpellListPublication.cs"));
            foreach (string guid in new[] {
                "8443ce803d2d31347897a3d85cc32f53",
                "9f5be2f7ea64fe04eb40878347b147bc",
                "57c894665b7895c499b3dce058c284b3",
                "75576ed8cab010644a11f9ecd512a7f9",
                "93228f4df23d2d448a0db59141af8aed" })
                Assertions.True(publication.Contains(guid),
                    "Required level-2 base-list GUID is missing: " + guid);
            Assertions.True(publication.Contains("ReferenceEquals(value.Before, level.Spells)") &&
                publication.Contains("rollback refused") &&
                publication.Contains("m_SpellsFiltered"),
                "Publication must preserve physical aliases, clear caches, and refuse unsafe rollback.");
            string bootstrap = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Bootstrap", "BlueprintBootstrap.cs"));
            Assertions.True(bootstrap.Contains("publication.failed") &&
                bootstrap.Contains("other modules will continue"),
                "Shield Other publication failure must remain isolated from other modules.");
        }

        internal static void OptionalPublicationSourceContract()
        {
            string root = Environment.CurrentDirectory;
            string source = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Spells", "ShieldOther",
                "ShieldOtherFinalLiveReconciler.cs"));
            foreach (string token in new[] {
                "32c02466b2364c8a906e6e4761175099",
                "e119d84528144a7797ad34fd718b1f87",
                "359bbaacabc445499049b59d295194cb",
                "ReferenceEquals(book.CharacterClass, value)",
                "book.SpellList.MaxLevel >= 6",
                "book.Spontaneous == spontaneous",
                "book.IsArcane == arcane",
                "book.CastingAttribute == attribute",
                "if (named.Length == 0) return null",
                "duplicate.final-live" })
                Assertions.True(source.Contains(token),
                    "Optional final-live contract is missing: " + token);
            Assertions.True(source.Split(new[] { "ReconcileOptional(library, shieldOther);" },
                StringSplitOptions.None).Length == 3,
                "Final-live optional publication must run an idempotent second pass.");
        }

        internal static void LinkComponentSourceContract()
        {
            string source = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Spells", "ShieldOther",
                "ShieldOtherBuffComponent.cs"));
            foreach (string token in new[] {
                "OwnedGameLogicComponent<UnitDescriptor>, ITickEachRound",
                "buff.MaybeContext.MaybeCaster",
                "buff.MaybeContext.Params.CasterLevel",
                "caster.HPLeft > 0",
                "!caster.Descriptor.State.IsDead",
                "!caster.Descriptor.State.MarkedForDeath",
                "!caster.Descriptor.State.ForceKill",
                "subject.IsInGame && caster.IsInGame",
                "subject.DistanceTo(caster) * FeetPerMeter",
                "ShieldOtherLinkValidityPolicy.Evaluate",
                "buff.Remove()" })
                Assertions.True(source.Contains(token),
                    "Link lifecycle source contract is missing: " + token);
            string policy = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Spells", "ShieldOther",
                "ShieldOtherLinkValidityPolicy.cs"));
            Assertions.False(policy.Contains("out-of-range") ||
                policy.Contains("request.DistanceFeet"),
                "Established Shield Other link validity must not depend on distance.");
        }

        internal static void DamageRuntimeSourceContract()
        {
            string root = Path.Combine(Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "Spells", "ShieldOther");
            string patch = File.ReadAllText(Path.Combine(root,
                "ShieldOtherDamagePatch.cs"));
            string runtime = File.ReadAllText(Path.Combine(root,
                "ShieldOtherRuntime.cs"));
            foreach (string token in new[] {
                "ApplyDifficultyModifiers",
                "GetProperty(\"Damage\")",
                "GetProperty(\"LastHandledDamage\")",
                "typeof(UnitEntityData)",
                "new CodeInstruction(OpCodes.Ldarg_0, null)",
                "ShieldOtherRuntime" })
                Assertions.True(patch.Contains(token),
                    "Finalized-damage patch contract is missing: " + token);
            foreach (string token in new[] {
                "[ThreadStatic] private static int _transferDepth",
                "ShieldOtherBuffComponent.TryEvaluate",
                "ShieldOtherDamageSplitPolicy.Split",
                "SetDamage(damage, split.SubjectShare)",
                "IgnoreDamageReduction = true",
                "_forcedTransferredDamage = split.CasterShare",
                "finally",
                "SetDamage(damage, finalized)",
                "ShieldOtherCombatLog.Publish" })
                Assertions.True(runtime.Contains(token),
                    "Guarded transfer runtime contract is missing: " + token);
        }

        internal static void RuntimeModuleRequestSourceContract()
        {
            string root = Environment.CurrentDirectory;
            string request = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRequest.cs"));
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs"));
            foreach (string token in new[] { "request.Parameters.Count != 9",
                "Property(\"shieldOther\")", "[\"shieldOther\"].Type != JTokenType.Boolean",
                "Property(\"expandedSummoning\")",
                "[\"expandedSummoning\"].Type != JTokenType.Boolean",
                "Property(\"elvenBranchedSpears\")",
                "[\"elvenBranchedSpears\"].Type",
                "Property(\"easternWeapons\")",
                "[\"easternWeapons\"].Type",
                "Property(\"brownFurTransmuter\")",
                "[\"brownFurTransmuter\"].Type",
                "Property(\"urbanBarbarian\")",
                "[\"urbanBarbarian\"].Type",
                "Property(\"bodyguardFeats\")",
                "[\"bodyguardFeats\"].Type" })
                Assertions.True(request.Contains(token),
                    "Runtime module request contract is missing: " + token);
            foreach (string token in new[] { "Active.ShieldOther",
                "RegisteredBlueprintCount == BlueprintBootstrap.ExpectedRegisteredBlueprintCountForCurrentRuntime",
                "feature-module-shield-other-publication",
                "typed-physical-damage", "new PhysicalDamage(",
                "typed-energy-damage", "new EnergyDamage(",
                "shield-other-typed-physical-split",
                "shield-other-typed-energy-split",
                "shield-other-physical-mitigation-once",
                "AddDamageResistancePhysical",
                "shield-other-energy-mitigation-once",
                "AddDamageResistanceEnergy",
                "shield-other-target-energy-immunity",
                "AddEnergyImmunity",
                "area-termination", "\"IsInGame\", false",
                "shield-other-area-termination",
                "post-cast-range-preservation",
                "shield-other-post-cast-range-preservation",
                "close range constrains initial targeting only",
                "rangePreserved && rangedSubject == 1 && rangedCaster == 1",
                "shield-other-transfer-log", "\"13 entries\"",
                "ShieldOtherCombatLog.Attempts == logsBefore + 13",
                "caster-death-termination",
                "shield-other-caster-death-termination" })
                Assertions.True(runner.Contains(token),
                    "Runtime Shield Other observer contract is missing: " + token);
            foreach (string stale in new[] { "shield-other-range-termination",
                "caster-level close range round revalidation",
                "ShieldOtherCombatLog.Attempts == logsBefore + 12" })
                Assertions.False(runner.Contains(stale),
                    "Stale post-cast distance termination remains in the runtime contract: " +
                    stale);
            foreach (string token in new[] { "native-availability",
                "abilityData.RequireMaterialComponent", "abilityData.IsAvailable",
                "new Kingmaker.UnitLogic.Commands.UnitUseAbility(",
                "commandCanStart", "shield-other-native-availability" })
                Assertions.True(runner.Contains(token),
                    "Runtime Shield Other availability regression contract is missing: " +
                    token);
        }

        private sealed class FakeSpell
        {
            internal FakeSpell(string guid) { Guid = guid; }
            internal string Guid { get; private set; }
        }

        private static ShieldOtherLinkValidityRequest Request()
        {
            return new ShieldOtherLinkValidityRequest { SubjectPresent = true,
                CasterPresent = true, CasterAlive = true, SameArea = true,
                CasterLevel = 1, DistanceFeet = 25f };
        }

        private static int ReadBigEndian(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) |
                (bytes[offset + 2] << 8) | bytes[offset + 3];
        }

        private static void AssertInvalid(string status,
            Action<ShieldOtherLinkValidityRequest> mutate)
        {
            ShieldOtherLinkValidityRequest request = Request(); mutate(request);
            ShieldOtherLinkValidityDecision decision =
                ShieldOtherLinkValidityPolicy.Evaluate(request);
            Assertions.True(!decision.Valid && decision.Status == status,
                "Unexpected invalid-link decision for " + status + ".");
        }
    }
}
