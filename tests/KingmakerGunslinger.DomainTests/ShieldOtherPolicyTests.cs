using System;
using System.IO;
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
                "Complete in-range link must be valid.");
            AssertInvalid("subject-missing", value => value.SubjectPresent = false);
            AssertInvalid("caster-missing", value => value.CasterPresent = false);
            AssertInvalid("caster-dead", value => value.CasterAlive = false);
            AssertInvalid("different-area", value => value.SameArea = false);
            AssertInvalid("out-of-range", value => value.DistanceFeet = 30.001f);
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
                "Exact close-range boundary must remain valid.");
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
                "result.MaterialComponent = null" })
                Assertions.True(source.Contains(token),
                    "Shield Other blueprint contract token is missing: " + token);
        }

        private static ShieldOtherLinkValidityRequest Request()
        {
            return new ShieldOtherLinkValidityRequest { SubjectPresent = true,
                CasterPresent = true, CasterAlive = true, SameArea = true,
                CasterLevel = 1, DistanceFeet = 25f };
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
