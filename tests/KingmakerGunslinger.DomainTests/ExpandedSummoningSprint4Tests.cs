using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Phase 1 Sprint 4 - Native Publication Pack II. Shambling Mound, Giant
    /// Flytrap and Purple Worm join the project-owned catalog on native
    /// donors; the shared summon grapple lifecycle (grab, hold, maintain,
    /// release, swallow, area safeguard) is bounded and pinned.
    /// </summary>
    internal static class ExpandedSummoningSprint4Tests
    {
        private static readonly string[] Keys = { "shambling-mound", "giant-flytrap", "purple-worm" };

        /// <summary>
        /// Identities Sprint 4 appended to the frozen ledger: three units, nine
        /// logical placements and seven grapple specials.
        /// </summary>
        internal const int AppendedLedgerIdentities = 19;

        internal static void PlacementsMatchTheCharterAndPropagate()
        {
            ExpandedSummoningCatalog.Validate();
            SummonCreatureSpec mound = Find("shambling-mound");
            SummonCreatureSpec flytrap = Find("giant-flytrap");
            SummonCreatureSpec worm = Find("purple-worm");
            Assertions.Equal(null, mound.MonsterTier, "Shambling Mound is not a Summon Monster creature.");
            Assertions.Equal(6, mound.NaturesAllyTier, "Shambling Mound is Summon Nature's Ally VI.");
            Assertions.Equal(null, flytrap.MonsterTier, "Giant Flytrap is not a Summon Monster creature.");
            Assertions.Equal(7, flytrap.NaturesAllyTier, "Giant Flytrap is Summon Nature's Ally VII.");
            Assertions.Equal(null, worm.MonsterTier, "Purple Worm is not a Summon Monster creature.");
            Assertions.Equal(8, worm.NaturesAllyTier, "Purple Worm is Summon Nature's Ally VIII.");
            foreach (string key in Keys)
            {
                Assertions.False(Find(key).MonsterTemplated, "Sprint 4 creatures take no template: " + key);
                IdealRosterEntry entry = ExpandedSummoningIdealRosterCatalog.Find(key);
                Assertions.True(entry != null && entry.Sprint == 4,
                    "The ideal roster must own the creature under Sprint 4: " + key);
                Assertions.Equal(entry.NaturesAllyTier, Find(key).NaturesAllyTier,
                    "Shipped Ally tier must match the plan: " + key);
            }
            AssertPropagation("shambling-mound", 6);
            AssertPropagation("giant-flytrap", 7);
            AssertPropagation("purple-worm", 8);
            Assertions.Equal(0, ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Count(value => Keys.Contains(value.Creature.Key)),
                "Sprint 4 creatures must not appear under Summon Monster.");
            int published = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                .Where(value => Keys.Contains(value.Creature.Key))
                .Count(SummonVisibilityCatalog.IsPublished);
            Assertions.Equal(9, published, "Sprint 4 adds nine published logical placements.");
            foreach (string key in Keys)
                Assertions.Equal(SummonFamilyCoverage.Published,
                    ExpandedSummoningCoveragePolicy.Coverage(key, SummonFamily.NaturesAlly,
                        Find(key).NaturesAllyTier),
                    "Coverage must derive Published for " + key);
        }

        internal static void DonorsAndProfilesAreExact()
        {
            ExpandedSummoningDonorCatalog.Validate();
            ExpandedSummoningNaturalProfiles.Validate();
            Assertions.Equal("b98ae409beb5e8543a75b82ecda082a7",
                ExpandedSummoningDonorCatalog.For("shambling-mound").Guid,
                "Shambling Mound uses the standard native mound body.");
            Assertions.False(ExpandedSummoningDonorCatalog.For("shambling-mound").DedicatedSummon,
                "The campaign mound is a body donor that must be sanitized.");
            Assertions.Equal("fb824352b7968fb4d8103ac439644633",
                ExpandedSummoningDonorCatalog.For("giant-flytrap").Guid,
                "Giant Flytrap uses the standard native flytrap body.");
            Assertions.False(ExpandedSummoningDonorCatalog.For("giant-flytrap").DedicatedSummon,
                "The campaign flytrap is a body donor that must be sanitized.");
            Assertions.Equal("bf2216f48b3f4d24c9c502007649340d",
                ExpandedSummoningDonorCatalog.For("purple-worm").Guid,
                "Purple Worm uses the native summoned worm.");
            Assertions.True(ExpandedSummoningDonorCatalog.For("purple-worm").DedicatedSummon,
                "The native summoned worm is a dedicated summon donor.");

            NaturalSummonProfile mound = ExpandedSummoningNaturalProfiles.For("shambling-mound");
            AssertScores(mound, "Plant", 9, "Large", 21, 10, 17, 7, 10, 9, 20, 10);
            Assertions.Equal("SlamPlant2d6", mound.PrimaryWeapon, "Mound slam changed.");
            Assertions.True(mound.AdditionalWeapons.SequenceEqual(new[] { "SlamPlant2d6" }),
                "Mound carries two 2d6 slams.");
            Assertions.True(mound.Facts.Contains("FireResistance10") &&
                mound.Facts.Contains("ElectricityImmunity") && mound.Facts.Contains("PowerAttack") &&
                mound.Facts.Contains("IronWill") && mound.Facts.Contains("LightningReflexes") &&
                mound.Facts.Contains("Cleave") && mound.Facts.Contains("WeaponFocusSlam"),
                "Mound resistances or feats changed.");
            Assertions.True(mound.Deviations.Any(value => value.Contains("constrict")) &&
                mound.Deviations.Any(value => value.Contains("poison aura")),
                "Mound deviations must record the constrict lifecycle and the uncarried native poison.");
            NaturalSummonProfile flytrap = ExpandedSummoningNaturalProfiles.For("giant-flytrap");
            AssertScores(flytrap, "Plant", 13, "Huge", 25, 18, 25, 1, 12, 6, 10, 10);
            Assertions.Equal("BiteLarge1d8", flytrap.PrimaryWeapon, "Flytrap bite changed.");
            Assertions.True(flytrap.AdditionalWeapons.SequenceEqual(
                    new[] { "BiteLarge1d8", "BiteLarge1d8", "BiteLarge1d8" }),
                "Flytrap carries four 1d8 bites.");
            Assertions.True(flytrap.Facts.Contains("AcidResistance20") &&
                flytrap.Facts.Contains("Blindsight") && flytrap.Facts.Contains("TripImmune") &&
                flytrap.Facts.Contains("WeaponFocusBite"),
                "Flytrap resistances, senses or feats changed.");
            Assertions.True(flytrap.Deviations.Any(value => value.Contains("one held target")) &&
                flytrap.Deviations.Any(value => value.Contains("Engulf")),
                "Flytrap deviations must record the single hold and the omitted engulf.");
            NaturalSummonProfile worm = ExpandedSummoningNaturalProfiles.For("purple-worm");
            AssertScores(worm, "MagicalBeast", 16, "Gargantuan", 35, 6, 25, 1, 8, 8, 20, 22);
            Assertions.Equal("PurpleWormBite", worm.PrimaryWeapon, "Worm bite changed.");
            Assertions.True(worm.AdditionalWeapons.SequenceEqual(new[] { "PurpleWormSting" }),
                "Worm carries its sting.");
            Assertions.True(worm.Facts.Contains("PurpleWormPoison") &&
                worm.Facts.Contains("TripImmune") && worm.Facts.Contains("CriticalFocus") &&
                worm.Facts.Contains("ImprovedCriticalBite"),
                "Worm poison, trip immunity or feats changed.");
            Assertions.True(worm.Deviations.Any(value => value.Contains("swallow")) &&
                worm.Deviations.Any(value => value.Contains("Burrow")),
                "Worm deviations must record the native swallow model and the omitted burrow.");
            Assertions.True(ExpandedSummoningNaturalProfiles.SupportedHitDieClasses
                .SequenceEqual(new[] { "Animal", "Vermin", "MagicalBeast", "Humanoid", "Plant" }),
                "Supported hit-die classes changed.");
            Assertions.True(ExpandedSummoningNaturalProfiles.For("owlbear").Deviations
                .Any(value => value.Contains("shared summon grapple lifecycle")),
                "The owlbear's claw grab must now ride the shared lifecycle.");

            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningNaturalBuilder.cs"));
            foreach (string token in new[] {
                "9393cc36ea29d084bab7433e3a28d40b", "case \"Plant\"",
                "27eee74857c42db499b3a6b20cfa6211", "ec35ef997ed5a984280e1a6d87ae80a8",
                "7e4b9b41a9358264d9e3c69c183ca0a2", "287cd06241fdaf8408410b226f744093",
                "4179c5c08d606a6439a62bf178b738e1", "eee672c8f6555b445a89dbbb91361d64",
                "24700a71dd3dc844ea585345f6dd18f6", "416386972c8de2e42953533c4946599a",
                "236ec7f226d3d784884f066aa4be1570", "728446b9d0bf47144a1b621169299c2a" })
                Assertions.True(builder.Contains(token),
                    "Sprint 4 natural builder contract is missing: " + token);
        }

        internal static void GrappleLifecycleIsBounded()
        {
            ExpandedSummoningSpecialProfiles.Validate();
            Assertions.Equal(4, ExpandedSummoningSpecialProfiles.SummonGrabManeuverBonus,
                "Grab carries the tabletop +4 grapple bonus.");
            Assertions.Equal(5, ExpandedSummoningSpecialProfiles.SummonHoldMaintainBonus,
                "Maintaining carries the tabletop +5.");
            Assertions.Equal(7, ExpandedSummoningSpecialProfiles.ConstrictBonus(5),
                "Constrict adds one and a half times the Strength modifier.");
            Assertions.Equal(ExpandedSummoningSpecialProfiles.ConstrictBonus(5),
                ExpandedSummoningSpecialProfiles.ShamblingMoundConstrictBonus,
                "The mound's constrict bonus derives from its Strength 21.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(
                true, true, false, false, false, false), "A clean hit with a grab weapon grabs.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(
                false, true, false, false, false, false), "A miss never grabs.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(
                true, false, false, false, false, false), "A hit with another weapon never grabs.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(
                true, true, true, false, false, false), "A summon already holding never grabs again.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(
                true, true, false, true, false, false), "A held target is never grabbed twice.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(
                true, true, false, false, true, false), "A swallowed target is never grabbed.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(
                true, true, false, false, false, true), "A summon never grabs itself.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldMaintainSummonHold(true, true),
                "A successful maintain check keeps the hold.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldMaintainSummonHold(true, false),
                "A failed maintain check releases.");
            Assertions.False(ExpandedSummoningSpecialProfiles.ShouldMaintainSummonHold(false, true),
                "A hold with no owned target is released.");
            var identities = ExpandedSummoningIdentityCatalog.Build();
            foreach (string symbol in new[] {
                "KMG.Summoning.Special.Grapple.Hold",
                "KMG.Summoning.Special.Grapple.Grappled",
                "KMG.Summoning.Special.Owlbear.CombatTraits",
                "KMG.Summoning.Special.ShamblingMound.CombatTraits",
                "KMG.Summoning.Special.GiantFlytrap.CombatTraits",
                "KMG.Summoning.Special.PurpleWorm.CombatTraits",
                "KMG.Summoning.Special.PurpleWorm.Swallowed" })
                Assertions.Equal(1, identities.Count(value => value.Symbol == symbol &&
                    value.PlannedType == "BlueprintBuff"),
                    "Grapple special identity missing or duplicated: " + symbol);
            string components = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSpecialCombatComponents.cs"));
            foreach (string token in new[] {
                "class SummonGrabComponent", "class SummonHoldComponent",
                "class SummonSwallowLifecycleComponent", "class SummonGrappleAreaSafeguard",
                "CombatManeuver.Grapple", "UnitPartGrappleInitiator", "UnitPartGrappleTarget",
                "UnitPartSwallowWhole", "SpitOut(true)", "IPartyLeaveAreaHandler",
                "IAreaLoadingStagesHandler", "public override void OnTurnOff()",
                "ShouldAttemptSummonGrab", "ShouldMaintainSummonHold" })
                Assertions.True(components.Contains(token),
                    "Grapple lifecycle component contract is missing: " + token);
            Assertions.False(components.Contains("Remove<UnitPartGrappleInitiator>()") &&
                components.IndexOf("Remove<UnitPartGrappleInitiator>()", StringComparison.Ordinal) <
                    components.IndexOf("class SummonGrappleAreaSafeguard", StringComparison.Ordinal),
                "The hold buff never removes its own initiator part (re-entrant removal); only the safeguard does.");
            string builder = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningSpecialBuilder.cs"));
            Assertions.True(builder.Contains("ConfigureGrapplers(library, bySymbol)") &&
                builder.Contains("368d1df7c1d0267459a584bf23ccadc8") &&
                builder.Contains("UnitCondition.Entangled") &&
                builder.Contains("SummonGrabManeuverBonus"),
                "The grapple pack must clone the native swallowed state, entangle the held target and carry the +4 bonus.");
            string main = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "src", "KingmakerGunslinger", "Main.cs"));
            Assertions.True(main.Contains("SummonGrappleAreaSafeguard.Attach();"),
                "The area safeguard must be attached at load.");
        }

        internal static void LedgerAndIconsCoverTheNewCreatures()
        {
            SummonIconCatalog.Validate();
            foreach (string key in Keys)
                Assertions.Equal(SummonProjectIconScope.KmgCatalog,
                    SummonIconCatalog.For(key).Scope, "Sprint 4 creature icon scope: " + key);
            string ledger = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json"));
            var entries = Newtonsoft.Json.Linq.JObject.Parse(ledger)["entries"]
                .Select(value => (string)value["symbol"]).ToArray();
            string[] appended = entries.Where(value =>
                value.Contains(".ShamblingMound") || value.Contains(".GiantFlytrap") ||
                value.Contains(".PurpleWorm") || value.Contains(".Grapple.") ||
                value == "KMG.Summoning.Special.Owlbear.CombatTraits").ToArray();
            Assertions.Equal(AppendedLedgerIdentities, appended.Length,
                "Sprint 4 must append exactly its own identities to the ledger.");
            // Append-only: the Sprint 4 block sits directly before the Sprint 5
            // block at the ledger's tail, and directly after the Sprint 3 block.
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities -
                    ExpandedSummoningSprint5Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint6Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint7Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint8Tests.AppendedLedgerIdentities)
                .Take(AppendedLedgerIdentities)
                .All(value => appended.Contains(value)),
                "The ledger is append-only: Sprint 4 identities sit directly before Sprint 5's.");
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities -
                    ExpandedSummoningSprint5Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint6Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint7Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint8Tests.AppendedLedgerIdentities -
                    ExpandedSummoningSprint3Tests.AppendedLedgerIdentities)
                .Take(ExpandedSummoningSprint3Tests.AppendedLedgerIdentities)
                .All(value => !appended.Contains(value)),
                "The Sprint 3 append stays exactly before the Sprint 4 append.");
            foreach (string symbol in new[] {
                "KMG.Summoning.Unit.ShamblingMound", "KMG.Summoning.Unit.GiantFlytrap",
                "KMG.Summoning.Unit.PurpleWorm",
                "KMG.Summoning.Ability.SNA.Tier6.ShamblingMound.One",
                "KMG.Summoning.Ability.SNA.Tier9.ShamblingMound.OneD4PlusOne",
                "KMG.Summoning.Ability.SNA.Tier7.GiantFlytrap.One",
                "KMG.Summoning.Ability.SNA.Tier8.PurpleWorm.One",
                "KMG.Summoning.Ability.SNA.Tier9.PurpleWorm.OneD3",
                "KMG.Summoning.Special.Grapple.Hold",
                "KMG.Summoning.Special.PurpleWorm.Swallowed" })
                Assertions.True(ledger.Contains("\"symbol\": \"" + symbol + "\""),
                    "The append-only ledger must carry " + symbol);
        }

        private static SummonCreatureSpec Find(string key)
        { return ExpandedSummoningCatalog.All.Single(value => value.Key == key); }

        private static void AssertPropagation(string key, int tier)
        {
            SummonVariantSpec[] variants = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.NaturesAlly)
                .Where(value => value.Creature.Key == key).OrderBy(value => value.ParentTier)
                .ToArray();
            Assertions.Equal(10 - tier, variants.Length,
                key + " must reach every parent from its tier to IX");
            for (int index = 0; index < variants.Length; index++)
            {
                int parent = tier + index;
                Assertions.Equal(parent, variants[index].ParentTier, "Parent order for " + key);
                Assertions.Equal(parent == tier ? SummonMultiplicity.One :
                    parent == tier + 1 ? SummonMultiplicity.OneD3 :
                    SummonMultiplicity.OneD4PlusOne, variants[index].Multiplicity,
                    "Quantity at parent " + parent + " for " + key);
            }
        }

        private static void AssertScores(NaturalSummonProfile profile, string hitDieClass,
            int hitDice, string size, int strength, int dexterity, int constitution,
            int intelligence, int wisdom, int charisma, int speed, int naturalArmor)
        {
            Assertions.Equal(hitDieClass, profile.HitDieClass, profile.Key + " hit-die class");
            Assertions.Equal(hitDice, profile.HitDice, profile.Key + " HD");
            Assertions.Equal(size, profile.Size, profile.Key + " size");
            Assertions.Equal(strength, profile.Strength, profile.Key + " Str");
            Assertions.Equal(dexterity, profile.Dexterity, profile.Key + " Dex");
            Assertions.Equal(constitution, profile.Constitution, profile.Key + " Con");
            Assertions.Equal(intelligence, profile.Intelligence, profile.Key + " Int");
            Assertions.Equal(wisdom, profile.Wisdom, profile.Key + " Wis");
            Assertions.Equal(charisma, profile.Charisma, profile.Key + " Cha");
            Assertions.Equal(speed, profile.SpeedFeet, profile.Key + " speed");
            Assertions.Equal(naturalArmor, profile.NaturalArmor, profile.Key + " natural armor");
        }
    }
}
