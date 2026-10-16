using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint25Tests
    {
        internal const int AppendedLedgerIdentities =
            GiantBodyRulesPolicy.AppendedIdentityCount;

        private static readonly string[] PrivateIdentityTail = {
            GiantBodyRulesPolicy.BrachiosaurusTailSymbol,
            GiantBodyRulesPolicy.BrachiosaurusTailTypeSymbol,
            GiantBodyRulesPolicy.BrachiosaurusUnitTypeSymbol,
            GiantBodyRulesPolicy.BrachiosaurusChassisSymbol,
            GiantBodyRulesPolicy.BrachiosaurusTrampleSymbol,
            GiantBodyRulesPolicy.ElephantTrampleSymbol
        };

        internal static void RoutesAreExactlyThreeNatureAllyRootsAndNoMonsterRow()
        {
            ExpandedSummoningCatalog.Validate();
            SummonVisibilityCatalog.Validate();
            GiantBodyRulesPolicy.Validate();

            SummonVariantSpec[] monster = NewRoutes(SummonFamily.Monster);
            SummonVariantSpec[] ally = NewRoutes(SummonFamily.NaturesAlly);
            Assertions.Equal(0, monster.Length,
                "Brachiosaurus has no Summon Monster row, so this sprint adds no Monster root.");
            Assertions.Equal(3, ally.Length,
                "Brachiosaurus adds exactly three Nature's Ally roots.");

            int[] tiers = ally.Select(value => value.ParentTier)
                .OrderBy(value => value).ToArray();
            Assertions.True(tiers.SequenceEqual(new[] { 7, 8, 9 }),
                "The three roots are parents VII, VIII and IX.");
            Assertions.True(ally.All(value =>
                value.Creature.Key == GiantBodyRulesPolicy.BrachiosaurusKey),
                "Only the new creature owns a new root.");
            Assertions.True(ally.All(value => value.SourceTier == 7),
                "Every root derives from the printed Nature's Ally VII entry.");
            Assertions.Equal(SummonMultiplicity.One,
                ally.Single(value => value.ParentTier == 7).Multiplicity,
                "Parent VII summons one.");
            Assertions.Equal(SummonMultiplicity.OneD3,
                ally.Single(value => value.ParentTier == 8).Multiplicity,
                "Parent VIII summons 1d3.");
            Assertions.Equal(SummonMultiplicity.OneD4PlusOne,
                ally.Single(value => value.ParentTier == 9).Multiplicity,
                "Parent IX summons 1d4+1.");
            Assertions.False(ally.Any(value => value.Creature.MonsterTemplated),
                "An untemplated creature owns no celestial or fiendish execution child.");
        }

        internal static void ElephantIsCompletedAndNeverReRegisteredOrSuppressed()
        {
            SummonCreatureSpec elephant = ExpandedSummoningCatalog.All.Single(
                value => value.Key == GiantBodyRulesPolicy.ElephantKey);
            Assertions.Equal(6, elephant.MonsterTier ?? 0,
                "Elephant keeps its released Summon Monster VI tier.");
            Assertions.Equal(6, elephant.NaturesAllyTier ?? 0,
                "Elephant keeps its released Summon Nature's Ally VI tier.");
            Assertions.True(elephant.MonsterTemplated,
                "Elephant keeps its released celestial and fiendish children.");

            SummonVariantSpec[] roots = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly))
                .Where(value => value.Creature.Key ==
                    GiantBodyRulesPolicy.ElephantKey)
                .ToArray();
            Assertions.Equal(GiantBodyRulesPolicy.ElephantExistingRoots,
                roots.Length,
                "Elephant's eight existing roots are completed, not re-registered.");
            Assertions.Equal(GiantBodyRulesPolicy.ElephantExistingMonsterRoots,
                roots.Count(value => value.Family == SummonFamily.Monster),
                "Four existing Monster roots.");
            Assertions.Equal(GiantBodyRulesPolicy.ElephantExistingNatureAllyRoots,
                roots.Count(value => value.Family == SummonFamily.NaturesAlly),
                "Four existing Nature's Ally roots.");
            Assertions.True(roots.All(SummonVisibilityCatalog.IsPublished),
                "A released Elephant route is never suppressed while a development package exists.");

            // The actual Mastodon stays a separate published creature.
            SummonCreatureSpec mastodon = ExpandedSummoningCatalog.All.Single(
                value => value.Key == "mastodon");
            Assertions.Equal(7, mastodon.MonsterTier ?? 0,
                "The real Mastodon is untouched at Summon Monster VII.");
            Assertions.True(ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == "mastodon")
                .All(SummonVisibilityCatalog.IsPublished),
                "No Mastodon route is suppressed by this sprint.");
        }

        internal static void OnlyTheNewCreatureIsWithheld()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(
                    SummonFamily.NaturesAlly)).ToArray();
            SummonVariantSpec[] withheld = all.Where(value =>
                !SummonVisibilityCatalog.IsPublished(value)).ToArray();

            Assertions.Equal(GiantBodyRulesPolicy.HiddenLogicalPlacements,
                withheld.Length,
                "Exactly the three Brachiosaurus roots are withheld.");
            Assertions.True(withheld.All(value =>
                GiantBodyRulesPolicy.IsWithheldThisSprint(value.Creature.Key)),
                "Nothing but the new creature is withheld.");
            Assertions.Equal(
                GiantBodyRulesPolicy.RegisteredLogicalPlacements,
                SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                "Registration allocated every placement once.");
            Assertions.Equal(
                GiantBodyRulesPolicy.HiddenPublishedLogicalPlacements,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "The published surface retains every pre-Sprint-25 placement, which is "
                + "this test's real subject: a later sprint must not reach back and "
                + "suppress a published creature.");
            Assertions.Equal(
                TheropodRulesPolicy.FinalPublishedLogicalPlacements,
                SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                "The withheld candidate leaves the released v0.0.155 surface exactly where it was.");

            foreach (SummonVariantSpec variant in all.Where(value =>
                !GiantBodyRulesPolicy.IsWithheldThisSprint(value.Creature.Key)))
                Assertions.True(SummonVisibilityCatalog.IsPublished(variant),
                    "Every pre-Sprint-25 placement is published, this creature's included: "
                    + variant.StableKey);
        }

        internal static void FrozenAdultArithmeticIsDerivedNotStated()
        {
            GiantBodyRulesPolicy.Validate();

            // The recorded source discrepancy and its resolution.
            Assertions.Equal(23,
                GiantBodyRulesPolicy.BrachiosaurusSelectedTailAttack,
                "The selected tail bonus is the legacy +23.");
            Assertions.Equal(22,
                GiantBodyRulesPolicy.BrachiosaurusPrintedTailAttackCurrentAon,
                "The rejected current-AoN reading is recorded, not discarded.");
            Assertions.Equal(
                GiantBodyRulesPolicy.BrachiosaurusSelectedTailAttack,
                GiantBodyRulesPolicy.AttackBonus(
                    GiantBodyRulesPolicy.BrachiosaurusBaseAttackBonus,
                    GiantBodyRulesPolicy.AbilityModifier(
                        GiantBodyRulesPolicy.BrachiosaurusStrength),
                    GiantBodyRulesPolicy.BrachiosaurusSizeAttackModifier,
                    GiantBodyRulesPolicy.BrachiosaurusWeaponFocusTailBonus),
                "The selected bonus is what the shared printed inputs derive.");

            // The lone tail takes one and a half Strength; Elephant's two
            // primaries take it once each.
            Assertions.Equal(19,
                GiantBodyRulesPolicy.LoneNaturalWeaponStrengthContribution(13),
                "Strength 37 gives the printed 4d6+19 on a single natural weapon.");
            Assertions.Equal(10,
                GiantBodyRulesPolicy
                    .MultipleNaturalPrimaryStrengthContribution(10),
                "Strength 30 gives the printed 2d8+10 and 2d6+10 on two primaries.");
            Assertions.True(GiantBodyRulesPolicy.UsesLoneNaturalWeaponStrength(
                    GiantBodyRulesPolicy.BrachiosaurusKey),
                "Only the one-weapon creature uses the one-and-a-half routine.");
            Assertions.False(GiantBodyRulesPolicy.UsesLoneNaturalWeaponStrength(
                    GiantBodyRulesPolicy.ElephantKey),
                "The two-weapon creature does not.");

            // Zero and penalties are never multiplied.
            foreach (int modifier in new[] { 0, -1, -3, -5 })
                Assertions.Equal(modifier,
                    GiantBodyRulesPolicy
                        .LoneNaturalWeaponStrengthContribution(modifier),
                    "A zero or penalty Strength modifier stays exact: " + modifier);

            // Odd and even positive Strength both round the native way.
            Assertions.Equal(13,
                GiantBodyRulesPolicy.LoneNaturalWeaponStrengthContribution(9),
                "An odd positive modifier rounds down.");
            Assertions.Equal(15,
                GiantBodyRulesPolicy.LoneNaturalWeaponStrengthContribution(10),
                "An even positive modifier is exact.");

            // Trample DCs derive from live hit dice and Strength.
            Assertions.Equal(32,
                GiantBodyRulesPolicy.TrampleSaveDifficulty(
                    GiantBodyRulesPolicy.BrachiosaurusHitDice, 13),
                "Brachiosaurus trample is DC 32 derived, not stated.");
            Assertions.Equal(25,
                GiantBodyRulesPolicy.TrampleSaveDifficulty(
                    GiantBodyRulesPolicy.ElephantHitDice, 10),
                "Elephant trample is DC 25 derived, not stated.");

            // Space and reach, including the one that must not generalise.
            Assertions.Equal(15, GiantBodyRulesPolicy.ElephantSpaceFeet,
                "Elephant occupies fifteen feet.");
            Assertions.Equal(10, GiantBodyRulesPolicy.ElephantReachFeet,
                "Elephant reaches ten feet, which a Huge body does not imply.");
            Assertions.Equal(GiantBodyRulesPolicy.BrachiosaurusSpaceFeet,
                GiantBodyRulesPolicy.BrachiosaurusReachFeet,
                "Brachiosaurus is the creature whose reach equals its space.");
        }

        /// <summary>
        /// The signed-Strength trample contract, read through the same
        /// UngulateRulesProfile the damage path uses at
        /// ContextActionUngulateTrample line 122. Two formulas disagreed
        /// here: the record helper applied one and a half times a Strength
        /// BONUS, while the live branch multiplied unconditionally, so at
        /// a live modifier of -4 the record said -4 and the damage path
        /// said -6. The positive printed baselines agreed, which is why it
        /// survived. This table covers the cases the baselines do not.
        /// </summary>
        internal static void TrampleStrengthArithmeticIsOneRule()
        {
            foreach (string key in new[] {
                GiantBodyRulesPolicy.BrachiosaurusKey,
                GiantBodyRulesPolicy.ElephantKey })
            {
                UngulateRulesProfile rules = UngulateRulesPolicy.For(key);
                Assertions.True(rules.LiveTrampleMath,
                    key + " derives its trample live, which is the branch "
                        + "the disagreement was in.");
                // modifier -> expected contribution. A penalty is not a
                // bonus, so it is never multiplied.
                foreach (int[] row in new[] {
                    new[] { 13, 19 }, new[] { 10, 15 }, new[] { 8, 12 },
                    new[] { 7, 10 }, new[] { 5, 7 }, new[] { 1, 1 },
                    new[] { 0, 0 }, new[] { -1, -1 }, new[] { -2, -2 },
                    new[] { -3, -3 }, new[] { -4, -4 }, new[] { -5, -5 } })
                {
                    Assertions.Equal(row[1],
                        rules.TrampleBonusFor(row[0]),
                        key + " trample Strength contribution at modifier "
                            + row[0]);
                    // The record helper and the damage path are now one
                    // implementation; this proves they cannot drift.
                    Assertions.Equal(rules.TrampleBonusFor(row[0]),
                        GiantBodyRulesPolicy.TrampleStrengthContribution(
                            row[0]),
                        "record helper and damage path agree at " + row[0]);
                }
            }

            // The live save DC for each creature, from live hit dice and
            // live Strength, with no stampede adjacency.
            UngulateRulesProfile brach = UngulateRulesPolicy.For(
                GiantBodyRulesPolicy.BrachiosaurusKey);
            Assertions.Equal(32, brach.TrampleSaveDc(0,
                    GiantBodyRulesPolicy.BrachiosaurusHitDice, 13),
                "Brachiosaurus printed DC 32 derives from 18 HD and +13.");
            Assertions.Equal(28, brach.TrampleSaveDc(0,
                    GiantBodyRulesPolicy.BrachiosaurusHitDice, 9),
                "A debuffed Brachiosaurus DC falls with live Strength.");
            Assertions.Equal(15, brach.TrampleSaveDc(0,
                    GiantBodyRulesPolicy.BrachiosaurusHitDice, -4),
                "A negative live modifier lowers the DC by its face value.");

            UngulateRulesProfile eleph = UngulateRulesPolicy.For(
                GiantBodyRulesPolicy.ElephantKey);
            Assertions.Equal(25, eleph.TrampleSaveDc(0,
                    GiantBodyRulesPolicy.ElephantHitDice, 10),
                "Elephant printed DC 25 derives from 11 HD and +10.");
            Assertions.Equal(11, eleph.TrampleSaveDc(0,
                    GiantBodyRulesPolicy.ElephantHitDice, -4),
                "Elephant DC at a negative live modifier.");

            // The two creatures stay distinct, and neither gains the
            // other's dice.
            Assertions.Equal(2, brach.TrampleDiceCount, "Brachiosaurus 2d6.");
            Assertions.Equal(6, brach.TrampleDieSides, "Brachiosaurus d6.");
            Assertions.Equal(2, eleph.TrampleDiceCount, "Elephant 2d8.");
            Assertions.Equal(8, eleph.TrampleDieSides, "Elephant d8.");

            // The released frozen-math tramplers are the changed
            // dependency's control: they never take the live branch, so
            // their behaviour is unchanged at any live modifier.
            foreach (string released in new[] { "aurochs", "bison",
                "woolly-rhinoceros", "giant-stag-beetle" })
            {
                UngulateRulesProfile frozen = UngulateRulesPolicy.For(
                    released);
                Assertions.False(frozen.LiveTrampleMath,
                    released + " keeps its frozen released arithmetic.");
                Assertions.Equal(frozen.TrampleBonus,
                    frozen.TrampleBonusFor(-4),
                    released + " ignores a live modifier entirely.");
            }

            // Triceratops IS on the live branch, so it is the released
            // creature this repair actually touches. Its printed positive
            // values are unchanged; only the penalty case moves, and it
            // moves to the rule the record always stated.
            UngulateRulesProfile tri = UngulateRulesPolicy.For(
                "triceratops");
            Assertions.True(tri.LiveTrampleMath,
                "Triceratops derives live, so it shares this repair.");
            Assertions.Equal(12, tri.TrampleBonusFor(8),
                "Triceratops positive contribution is unchanged.");
            Assertions.Equal(15, tri.TrampleBonusFor(10),
                "Triceratops positive contribution is unchanged.");
            Assertions.Equal(-4, tri.TrampleBonusFor(-4),
                "Triceratops no longer doubles a Strength penalty.");
        }
        internal static void IdentityAppendOwnsExactlyFivePrivateCarriers()
        {
            // Six since 2026-10-16: the five Brachiosaurus carriers plus
            // the released Elephant trample, which was appended only
            // after the bounded observation of that creature recorded six
            // grants and no trample ability or component.
            Assertions.Equal(6, PrivateIdentityTail.Length,
                "Sprint 25 appends six private carriers.");
            Assertions.Equal(GiantBodyRulesPolicy.AppendedPrivateIdentityCount,
                PrivateIdentityTail.Length,
                "The policy's private count is the list's length.");
            Assertions.Equal(PrivateIdentityTail.Length,
                PrivateIdentityTail.Distinct(StringComparer.Ordinal).Count(),
                "No private carrier is allocated twice.");

            IReadOnlyList<SummoningIdentitySpec> identities =
                ExpandedSummoningIdentityCatalog.Build();
            string[] tail = identities.Skip(identities.Count -
                PrivateIdentityTail.Length).Select(value => value.Symbol)
                .ToArray();
            Assertions.True(tail.SequenceEqual(PrivateIdentityTail,
                    StringComparer.Ordinal),
                "The six private carriers append last, in frozen order.");

            Assertions.Equal(GiantBodyRulesPolicy.RegisteredCreatureCount,
                ExpandedSummoningIdentityCatalog.UnitCount,
                "One unit identity joins for the one new creature.");
            Assertions.Equal(
                GiantBodyRulesPolicy.RegisteredLogicalPlacements,
                ExpandedSummoningIdentityCatalog.LogicalAbilityCount,
                "Three logical ability identities join.");
            Assertions.Equal(
                GiantBodyRulesPolicy.RegisteredTemplatedPlacements,
                ExpandedSummoningIdentityCatalog.TemplatedPlacementCount,
                "No templated placement joins, because the new creature has no Monster row.");

            // Every appended symbol must resolve to exactly one manifest row
            // of the planned type, so an identity cannot be named here and
            // missing, or point at something else.
            var entries = (JArray)JObject.Parse(File.ReadAllText(
                Path.Combine(Environment.CurrentDirectory, "blueprints",
                    "blueprints.json")))["entries"];
            foreach (string symbol in PrivateIdentityTail.Concat(new[] {
                    "KMG.Summoning.Unit.Brachiosaurus",
                    "KMG.Summoning.Ability.SNA.Tier7.Brachiosaurus.One",
                    "KMG.Summoning.Ability.SNA.Tier8.Brachiosaurus.OneD3",
                    "KMG.Summoning.Ability.SNA.Tier9.Brachiosaurus.OneD4PlusOne" }))
                Assertions.Equal(1, entries.Count(entry =>
                        (string)entry["symbol"] == symbol &&
                        (string)entry["status"] == "active"),
                    "Exactly one active manifest row: " + symbol);

            Assertions.Equal(0, entries.Count(entry =>
                    ((string)entry["symbol"]).Contains("Brachiosaurus") &&
                    (((string)entry["symbol"]).EndsWith(".Celestial") ||
                     ((string)entry["symbol"]).EndsWith(".Fiendish"))),
                "No Brachiosaurus execution child exists in the ledger.");
            Assertions.Equal(0, entries.Count(entry =>
                    ((string)entry["symbol"]).Contains("Brachiosaurus") &&
                    ((string)entry["symbol"]).Contains(".Ability.SM.")),
                "No Brachiosaurus Summon Monster ability exists in the ledger.");
        }

        internal static void ElephantProfileIsCorrectedToItsPrintedRoutine()
        {
            NaturalSummonProfile elephant = ExpandedSummoningNaturalProfiles.For(
                GiantBodyRulesPolicy.ElephantKey);

            // The profile row has landed, together with the builder that
            // resolves its weapon key. This replaces the tripwire that held
            // its place: the row was absent on purpose, because a profile
            // whose weapon cannot resolve makes blueprint initialization
            // throw and roll back every registration, so the row and its
            // carriers had to be one change. The contract asserted now is
            // the positive one.
            NaturalSummonProfile brachiosaurus =
                ExpandedSummoningNaturalProfiles.For(
                    GiantBodyRulesPolicy.BrachiosaurusKey);
            Assertions.Equal("BrachiosaurusTail4d6",
                brachiosaurus.PrimaryWeapon,
                "The frozen primary weapon key is the one the builder resolves.");
            Assertions.Equal(GiantBodyRulesPolicy.BrachiosaurusHitDice,
                brachiosaurus.HitDice, "Eighteen racial hit dice.");
            Assertions.Equal(GiantBodyRulesPolicy.BrachiosaurusStrength,
                brachiosaurus.Strength, "Printed Strength.");
            Assertions.Equal("Gargantuan", brachiosaurus.Size,
                "The frozen adult size.");
            Assertions.Equal(0, brachiosaurus.AdditionalWeapons.Count,
                "A single natural attack, which is what the lone-weapon "
                    + "Strength routine depends on.");
            Assertions.Equal(0,
                brachiosaurus.AdditionalSecondaryWeapons.Count,
                "Nothing on this creature is a secondary natural attack.");
            Assertions.True(brachiosaurus.Facts.Contains(
                    "BrachiosaurusChassis"),
                "The chassis carrying the final five feet of reach is carried.");
            Assertions.True(brachiosaurus.Facts.Contains("WeaponFocusTail"),
                "Weapon Focus (tail) is carried; the selected +23 depends on it.");
            // Measured present in the installed library by the 2026-10-16
            // census - GreatCleaveFeature cc9c862e... and
            // GreaterOverrunFeature ee24c02c... - so the omission is a
            // recorded owner decision rather than a missing engine
            // capability. Neither may be carried without that decision.
            foreach (string open in new[] { "GreatCleave", "GreaterOverrun" })
                Assertions.False(brachiosaurus.Facts.Contains(open),
                    "Open printed feat is not silently carried: " + open);
            Assertions.True(brachiosaurus.Deviations.Any(value =>
                    value.Contains("Registered is not qualified")),
                "The trample is registered and still refuses to claim "
                    + "native qualification, which is a separate thing.");
            Assertions.True(UngulateRulesPolicy.All.Any(value =>
                    value.Key == GiantBodyRulesPolicy.BrachiosaurusKey &&
                    value.TrampleDiceCount == GiantBodyRulesPolicy
                        .BrachiosaurusTrampleDiceCount &&
                    value.TrampleDieSides == GiantBodyRulesPolicy
                        .BrachiosaurusTrampleDieSize &&
                    value.TrampleBonus == 19 && value.TrampleDc == 32 &&
                    !value.HasPowerfulCharge && !value.Stampede),
                "The trample derives 2d6+19 and DC 32 from the frozen "
                    + "Strength and hit dice, and grants nothing else.");

            // Elephant: the Sprint 25 correction. Both printed attacks are
            // primary, so neither takes the secondary -5 and half Strength.
            Assertions.Equal("Gore2d8", elephant.PrimaryWeapon,
                "The gore stays the primary weapon.");
            Assertions.True(elephant.AdditionalWeapons.Contains("Slam2d6"),
                "The slam is a primary attack, as printed.");
            Assertions.Equal(0, elephant.AdditionalSecondaryWeapons.Count,
                "Nothing on this creature is a secondary natural attack.");
            Assertions.Equal(GiantBodyRulesPolicy.ElephantHitDice,
                elephant.HitDice, "Eleven racial hit dice.");
            Assertions.Equal(GiantBodyRulesPolicy.ElephantStrength,
                elephant.Strength, "Printed Strength.");
            Assertions.Equal(GiantBodyRulesPolicy.ElephantSpeedFeet,
                elephant.SpeedFeet, "Forty-foot land speed.");
            Assertions.True(elephant.Facts.Contains("TripDefenseFourLegs"),
                "The printed quadruped defence against trip is carried.");
            Assertions.True(elephant.Facts.Contains("ImprovedBullRush"),
                "Improved Bull Rush is carried now that its carrier is proven.");
            foreach (string absent in new[] { "PowerfulCharge", "Grab",
                "Pounce" })
                Assertions.False(elephant.Facts.Contains(absent),
                    "No invented or unproven Elephant carrier: " + absent);
            // All six printed Elephant feats are carried. Endurance was
            // wrongly recorded as unproven: the builder has mapped it to a
            // native feat since Sprint 4, the Sprint 24 bounded native census
            // recorded it as resolved, and the released Tyrannosaurus ships
            // it. Every printed feat now resolves through a real carrier.
            foreach (string printed in new[] { "Endurance", "GreatFortitude",
                "ImprovedBullRush", "IronWill", "PowerAttack",
                "SkillFocusPerception" })
                Assertions.True(elephant.Facts.Contains(printed),
                    "Printed Elephant feat carried: " + printed);
            // The trample is registered now, and the record still refuses
            // to call registration qualification.
            Assertions.True(elephant.Deviations.Any(value =>
                    value.Contains("Registered is not qualified")),
                "Trample registration is recorded without claiming native "
                    + "qualification, which is a separate thing.");
            Assertions.True(UngulateRulesPolicy.All.Any(value =>
                    value.Key == GiantBodyRulesPolicy.ElephantKey &&
                    value.TrampleDiceCount == GiantBodyRulesPolicy
                        .ElephantTrampleDiceCount &&
                    value.TrampleDieSides == GiantBodyRulesPolicy
                        .ElephantTrampleDieSize &&
                    value.TrampleBonus == 15 && value.TrampleDc == 25 &&
                    !value.HasPowerfulCharge && !value.Stampede),
                "The Elephant trample derives 2d8+15 and DC 25 from its "
                    + "frozen Strength and hit dice, distinct from the "
                    + "Brachiosaurus 2d6+19 and DC 32, and grants nothing else.");

            // The real Mastodon keeps its own distinct profile.
            NaturalSummonProfile mastodon =
                ExpandedSummoningNaturalProfiles.For("mastodon");
            Assertions.Equal(14, mastodon.HitDice,
                "The Mastodon is untouched by this sprint.");
            Assertions.Equal(34, mastodon.Strength,
                "The Mastodon keeps its own Strength.");
        }

        internal static void TheDonorIsTheMeasuredQuadrupedRigAndNotAnIdentity()
        {
            SummonDonorSpec donor = ExpandedSummoningDonorCatalog.For(
                GiantBodyRulesPolicy.BrachiosaurusKey);
            SummonDonorSpec triceratops =
                ExpandedSummoningDonorCatalog.For("triceratops");
            Assertions.Equal(triceratops.Guid, donor.Guid,
                "Sprint 25 reuses the quadruped rig Sprint 23 already measured.");
            Assertions.False(donor.DedicatedSummon,
                "The rig is borrowed for animation only; the body is project-owned.");
            Assertions.Equal(
                ExpandedSummoningDonorCatalog.For(
                    GiantBodyRulesPolicy.ElephantKey).Guid,
                donor.Guid,
                "It is the same rig the released Elephant already rides.");
            ExpandedSummoningDonorCatalog.Validate();
        }

        private static SummonVariantSpec[] NewRoutes(SummonFamily family)
        {
            return ExpandedSummoningCatalog.GenerateVariants(family)
                .Where(value => GiantBodyRulesPolicy.IsWithheldThisSprint(
                    value.Creature.Key))
                .ToArray();
        }
    }
}
