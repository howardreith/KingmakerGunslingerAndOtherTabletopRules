using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class VrockRules37Tests
    {
        internal static void PublicRoutesHaveExactScopeAndQuantity()
        {
            var vrock = ExpandedSummoningCatalog.All.Single(value =>
                value.Key == "vrock");
            Assertions.False(vrock.NaturesAllyTier.HasValue ||
                vrock.MonsterTemplated,
                "Vrock is Monster-only and untemplated.");
            var routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == vrock.Key)
                .OrderBy(value => value.ParentTier).ToArray();
            Assertions.True(routes.Select(value => value.ParentTier)
                .SequenceEqual(new[] { 7, 8, 9 }),
                "Vrock has exactly Monster VII through IX.");
            Assertions.True(routes.Select(value => value.Multiplicity)
                .SequenceEqual(new[] { SummonMultiplicity.One,
                    SummonMultiplicity.OneD3,
                    SummonMultiplicity.OneD4PlusOne }),
                "Vrock quantities are one, 1d3, then 1d4+1.");
            Assertions.False(ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.NaturesAlly)
                .Any(value => value.Creature.Key == vrock.Key),
                "Vrock registers no Nature's Ally route.");
            Assertions.True(routes.All(
                SummonVisibilityCatalog.IsPublished),
                "All three routes are published together after the creature's six guarded mechanics phases.");

            var identities = ExpandedSummoningIdentityCatalog.Build();
            string[] routeSymbols = routes.Select(
                ExpandedSummoningIdentityCatalog.AbilitySymbol).ToArray();
            Assertions.True(routeSymbols.All(symbol => identities.Any(value =>
                value.Symbol == symbol)),
                "All three executable routes keep the identities they were allocated while hidden, so publication moved none of them.");
            Assertions.True(identities.Any(value => value.Symbol ==
                    ExpandedSummoningIdentityCatalog.UnitSymbol(vrock)),
                "The exact unit identity is the one registered before publication.");
            var privateIdentities = identities.Where(value =>
                value.Symbol.StartsWith("KMG.Summoning.Demon.Vrock.",
                    StringComparison.Ordinal)).ToArray();
            Assertions.Equal(
                SummoningCurrentSurface.Sprint37PrivateIdentityCount,
                privateIdentities.Length,
                "Only dependency-complete configured private identities are active.");
            var expected = new[] {
                new { Type = "BlueprintUnitType", Count = 1 },
                new { Type = "BlueprintFeature", Count = 1 },
                new { Type = "BlueprintItemWeapon", Count = 3 },
                new { Type = "BlueprintWeaponType", Count = 3 },
                new { Type = "BlueprintAbility", Count = 4 },
                new { Type = "BlueprintBuff", Count = 4 },
                new { Type = "BlueprintAbilityResource", Count = 2 },
                new { Type = "BlueprintBrain", Count = 1 },
                new { Type = "BlueprintAiCastSpell", Count = 2 },
                new { Type = "VrockScreechEligibility37", Count = 1 },
                new { Type = "VrockSporeEligibility37", Count = 1 },
                new { Type = "InfernalSpellAi35", Count = 2 },
            };
            foreach (var row in expected)
                Assertions.Equal(row.Count, privateIdentities.Count(value =>
                    value.PlannedType == row.Type),
                    "Vrock private identity shape for " + row.Type + ".");
            Assertions.Equal(expected.Sum(row => row.Count),
                privateIdentities.Length,
                "The private identity shape accounts for every Vrock dependency.");
        }

        internal static void ScreechDifficultyClassFollowsLiveConstitution()
        {
            // The printed DC 21 is 10 + half the racial Hit Dice + the
            // Constitution bonus, which is the released Sprint 36 formula, so
            // it is shared rather than repeated. A different live score has to
            // move the DC: a hard-coded 21 would pass the first check alone.
            Assertions.Equal(VrockRules37Policy.PrintedScreechDc,
                VrockRules37Policy.ScreechDifficultyClass(7),
                "The adult Constitution 25 profile yields the printed DC 21.");
            Assertions.Equal(VrockRules37Policy.PrintedScreechDc + 2,
                VrockRules37Policy.ScreechDifficultyClass(9),
                "A higher live Constitution raises the save DC.");
            Assertions.Equal(VrockRules37Policy.PrintedScreechDc - 3,
                VrockRules37Policy.ScreechDifficultyClass(4),
                "A lower live Constitution lowers the save DC.");
            Assertions.Equal(
                InfernalRules36Policy.ConstitutionDc(
                    VrockRules37Policy.VrockHitDice, 7),
                VrockRules37Policy.ScreechDifficultyClass(7),
                "The screech reuses the released Constitution-based formula.");
        }

        internal static void RechargingAbilitiesRefuseWhileTheirMarkerStands()
        {
            Assertions.True(VrockRules37Policy.CanActivate(true, false, true),
                "The exact owner may screech while recharged and the ability is available.");
            Assertions.False(VrockRules37Policy.CanActivate(true, true, true),
                "A standing recharge marker refuses the next activation.");
            Assertions.False(VrockRules37Policy.CanActivate(false, false, true),
                "Another creature cannot use this vrock's racial ability.");
            Assertions.False(VrockRules37Policy.CanActivate(true, false, false),
                "An unavailable granted ability is not activated by the marker alone.");
            Assertions.Equal(1, VrockRules37Policy.ScreechRechargeHours,
                "The screech recharges once per hour, not once per encounter or summoning.");
            Assertions.Equal(3, VrockRules37Policy.SporeCadenceRounds,
                "Spores return every three rounds.");
        }

        internal static void ScreechExemptsDemonsAndSporesHonourTheirCounters()
        {
            Assertions.True(VrockRules37Policy.ScreechReaches(true, false),
                "A non-demon inside the radius is affected.");
            Assertions.False(VrockRules37Policy.ScreechReaches(true, true),
                "A demon is exempt from the screech, as printed.");
            Assertions.False(VrockRules37Policy.ScreechReaches(false, false),
                "Only this vrock's own screech resolves through its component.");
            // The printed spore cloud has no *subtype* exemption, so an ally
            // and another demon are both eligible; ally safety is an AI
            // selection rule, not an eligibility rule. It does have disease
            // protection as a gate, which the last case below covers and the
            // native observation distinguishes from the later cures.
            Assertions.True(VrockRules37Policy.SporesReach(
                true, true, true, false),
                "Every live adjacent creature takes the spores.");
            Assertions.False(VrockRules37Policy.SporesReach(
                true, false, true, false),
                "A destroyed or dead target takes nothing.");
            Assertions.False(VrockRules37Policy.SporesReach(
                false, true, true, false),
                "Only this vrock's own spores resolve through its component.");
            Assertions.False(VrockRules37Policy.SporesReach(
                true, true, false, false),
                "A creature inside the collection radius but not adjacent is not exposed.");
            Assertions.False(VrockRules37Policy.SporesReach(
                true, true, true, true),
                "A creature already immune to disease is not exposed at all, impact included.");
            // Adjacency is NOT the melee reach. This comparison only says
            // the collector's broad-phase bound is wider than the reach; it
            // establishes nothing about the footprint relation itself, which
            // SpatialRelationIsFootprintNotReach exercises directly and the
            // native case measures on real units.
            Assertions.True(VrockRules37Policy.SporeCollectionRadiusFeet >
                VrockRules37Policy.ReachFeet,
                "The spore collection radius is a broad-phase bound wider than the reach.");
            Assertions.Equal(30, VrockRules37Policy.ScreechRadiusFeet,
                "The screech is a 30-foot-radius spread.");
            Assertions.Equal(1, VrockRules37Policy.ScreechStunRounds,
                "A failed save stuns for one round.");
            Assertions.Equal(10, VrockRules37Policy.SporeVineRounds,
                "The vines grow for ten rounds.");
        }

        internal static void DemonExemptionResolvesRealIdentities()
        {
            SummonDemonIdentityCatalog.Validate();
            var keys = SummonDemonIdentityCatalog.CreatureKeys;

            // Positive controls, including the two released species the first
            // hand-written four-entry set silently omitted.
            // Sprint 38 appends the Glabrezu, whose own printed type line
            // is "CE Huge outsider (chaotic, demon, evil, extraplanar)".
            // Sprint 39 appends the Nalfeshnee, whose own printed type line
            // is also "CE Huge outsider (chaotic, demon, evil, extraplanar)".
            // The Vrock's screech and the Nalfeshnee's nimbus now resolve
            // their exemptions from this same set, so it cannot drift between
            // the two.
            foreach (string key in new[] { "succubus", "shadow-demon",
                "dretch", "babau", "hezrou", "vrock", "glabrezu",
                "nalfeshnee" })
                Assertions.True(SummonDemonIdentityCatalog.IsDemonKey(key),
                    "A printed demon is in the exemption set: " + key);
            Assertions.Equal(8, keys.Count,
                "The exemption covers exactly this project's eight printed demons.");

            // Negative controls. The Bebilith is the one that matters most:
            // it is a chaotic evil extraplanar outsider with no demon subtype,
            // so an alignment or outsider test would wrongly exempt it.
            foreach (string key in new[] { "bebelith", "lemure",
                "erinyes-devil", "bearded-devil", "barbed-devil",
                "bone-devil", "kyton-chain-devil", "hell-hound",
                "shadow-mastiff", "xill", "astral-deva", "trumpet-archon",
                "lillend-azata", "fire-mephit", "hezrou-not-a-key" })
                Assertions.False(SummonDemonIdentityCatalog.IsDemonKey(key),
                    "A devil, daemon, kyton, archon, azata, mephit or other non-demon is never exempt: " + key);

            // Every member resolves to a real registered creature and to one
            // allocated unit identity, so the screech and its AI consideration
            // are wired from actual blueprints rather than from names.
            var identities = ExpandedSummoningIdentityCatalog.Build();
            string[] symbols = SummonDemonIdentityCatalog.UnitSymbols();
            Assertions.Equal(keys.Count, symbols.Length,
                "Every demon key resolves to exactly one unit symbol.");
            foreach (string symbol in symbols)
                Assertions.True(identities.Any(value =>
                    value.Symbol == symbol &&
                    value.PlannedType == "BlueprintUnit"),
                    "The exemption resolves an allocated unit identity: " + symbol);

            // Membership is independent of the native view donor a project
            // creature borrows: the Succubus rides a nymph and the Shadow
            // Demon a tiefling, and neither says anything about its subtype.
            foreach (string key in keys)
                Assertions.True(ExpandedSummoningCatalog.All.Any(value =>
                    value.Key == key),
                    "The exemption never names an unregistered creature: " + key);
        }

        internal static void SpatialRelationIsFootprintNotReach()
        {
            // Representative engine corpulences in metres: roughly a Medium
            // humanoid, a Large creature such as the Vrock, and a Huge one.
            const float medium = 0.5f;
            const float large = 1.0f;
            const float huge = 1.6f;
            const float tolerance =
                SummonFootprintGeometryPolicy.AdjacencyToleranceMeters;

            // The edge distance subtracts BOTH footprints, which is the whole
            // point: one centre distance means different things for different
            // pairs, so no single radius constant can express this relation.
            Assertions.Equal(0f, SummonFootprintGeometryPolicy.EdgeDistanceMeters(
                large + medium, large, medium),
                "Touching Large and Medium footprints are zero apart.");
            Assertions.Equal(0f, SummonFootprintGeometryPolicy.EdgeDistanceMeters(
                huge + huge, huge, huge),
                "Two touching Huge footprints are also zero apart, at over twice the centre distance.");

            // Just inside and just outside the threshold.
            Assertions.True(SummonFootprintGeometryPolicy.IsAdjacentDistance(
                large + medium + tolerance - 0.02f, large, medium),
                "A creature just inside the one-square threshold is adjacent.");
            Assertions.False(SummonFootprintGeometryPolicy.IsAdjacentDistance(
                large + medium + tolerance + 0.02f, large, medium),
                "A creature just outside the one-square threshold is not adjacent.");

            // Differing footprints at one fixed centre distance: the Huge
            // target's own footprint reaches into adjacency where the Medium
            // one does not.
            const float centres = large + medium + tolerance + 0.4f;
            Assertions.False(SummonFootprintGeometryPolicy.IsAdjacentDistance(
                centres, large, medium),
                "At this separation a Medium target is beyond adjacency.");
            Assertions.True(SummonFootprintGeometryPolicy.IsAdjacentDistance(
                centres, large, huge),
                "At the same separation a Huge target is adjacent.");

            // A target inside the Vrock's ten-foot weapon reach yet outside
            // adjacency. Reach is measured to the target; adjacency subtracts
            // both footprints, so these are genuinely different quantities and
            // the earlier constant comparison proves nothing about either.
            const float reachMeters = VrockRules37Policy.ReachFeet *
                SummonFootprintGeometryPolicy.FeetToMeters;
            const float withinReach = large + reachMeters - 0.1f;
            Assertions.True(withinReach <= large + reachMeters,
                "The probe sits inside the Vrock's melee reach.");
            Assertions.False(SummonFootprintGeometryPolicy.IsAdjacentDistance(
                withinReach, large, medium),
                "Being inside melee reach does not make a creature adjacent.");

            // Collector completeness: the broad-phase radius must not drop a
            // target this relation would call adjacent, for any footprint the
            // game actually fields.
            const float radiusMeters = VrockRules37Policy.SporeCollectionRadiusFeet *
                SummonFootprintGeometryPolicy.FeetToMeters;
            foreach (float target in new[] { 0.3f, medium, large, huge, 2.5f })
            {
                Assertions.True(
                    SummonFootprintGeometryPolicy.CollectionRadiusAdmitsAdjacent(
                        radiusMeters, large, target),
                    "The spore collection radius still admits the furthest adjacent target of this footprint.");
            }

            // And the bound is a real bound, not a tautology: a radius smaller
            // than the Vrock's own footprint plus one square would miss an
            // adjacent creature, which is why the constant is not the reach.
            Assertions.False(
                SummonFootprintGeometryPolicy.CollectionRadiusAdmitsAdjacent(
                    large + tolerance - 0.1f, large, medium),
                "Too small a collection radius drops an adjacent target.");
        }

        internal static void MovementRecordsBothPrintedSpeeds()
        {
            // Kingmaker exposes one movement speed. The effective flying speed
            // is used and the land speed is recorded and omitted; neither is
            // summed into the other, and a future reader can see both.
            Assertions.Equal(50, VrockRules37Policy.EffectiveFlySpeedFeet,
                "The effective speed is the printed 50-foot fly speed.");
            Assertions.Equal(30, VrockRules37Policy.OmittedLandSpeedFeet,
                "The printed 30-foot land speed is recorded, not discarded.");
            Assertions.False(VrockRules37Policy.EffectiveFlySpeedFeet ==
                VrockRules37Policy.OmittedLandSpeedFeet ||
                VrockRules37Policy.EffectiveFlySpeedFeet ==
                    VrockRules37Policy.OmittedLandSpeedFeet +
                    VrockRules37Policy.EffectiveFlySpeedFeet,
                "The two printed speeds are neither conflated nor summed.");
        }

        internal static void PrintedDefencesDeriveFromTheirBaseValues()
        {
            // The blueprint carries base save values; the printed totals are
            // those bases plus the live ability and feat modifiers. Recording
            // both keeps a genuine native difficulty modifier distinguishable
            // from a printed-profile comparison instead of being compensated
            // for in the gameplay numbers.
            Assertions.Equal(VrockRules37Policy.PrintedFortitude,
                VrockRules37Policy.BaseFortitude + 7,
                "Fortitude is its base plus the Constitution 25 bonus.");
            Assertions.Equal(VrockRules37Policy.PrintedReflex,
                VrockRules37Policy.BaseReflex + 2 + 2,
                "Reflex is its base plus Dexterity 15 and Lightning Reflexes.");
            Assertions.Equal(VrockRules37Policy.PrintedWill,
                VrockRules37Policy.BaseWill + 3,
                "Will is its base plus the Wisdom 16 bonus.");
            Assertions.Equal(VrockRules37Policy.PrintedPerception,
                VrockRules37Policy.SkillRanks + 3 + 3 +
                    VrockRules37Policy.RacialPerceptionBonus,
                "Perception is ranks, class skill, Wisdom and the racial +8.");
            Assertions.Equal(11, VrockRules37Policy.NaturalArmor,
                "AC 22 at touch 11 needs exactly +11 natural armour.");
            Assertions.Equal(20, VrockRules37Policy.SpellResistance,
                "Spell resistance is the printed 20, not the Hezrou's 22.");
            VrockRules37Policy.Validate();
        }

        /// <summary>
        /// The persistence scope needs two vrocks, not one. The printed spore
        /// growth lands on an adjacent creature rather than on the vrock
        /// itself, and the fixture may not write to a party member or to any
        /// unit it does not own, so a one-creature scope leaves the prepare
        /// step with no faithful victim and throws. This pins the pair so a
        /// later tidy-up cannot quietly reduce it to one.
        /// </summary>
        internal static void PersistenceScopeCarriesTwoOwnedVrocks()
        {
            string[] keys = ExpandedSummoningRosterPersistencePolicy.VrockKeys;
            Assertions.Equal(2, keys.Length,
                "The scope summons two vrocks so the growth has an adjacent victim it owns.");
            foreach (string key in keys)
                Assertions.Equal(VrockVisualPolicy37.Vrock, key,
                    "Both sources are the registered vrock, not a stand-in creature.");
            Assertions.True(ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(
                ExpandedSummoningRosterPersistencePolicy.VrockScope, keys),
                "The pair is what the scope admits.");
            Assertions.False(ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(
                ExpandedSummoningRosterPersistencePolicy.VrockScope,
                keys.Take(1)), "One vrock cannot satisfy the scope.");
            Assertions.False(ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(
                ExpandedSummoningRosterPersistencePolicy.VrockScope,
                keys.Concat(new[] { VrockVisualPolicy37.Vrock })),
                "A third vrock is not the scope either.");
            Assertions.False(ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(
                ExpandedSummoningRosterPersistencePolicy.VrockScope,
                new[] { VrockVisualPolicy37.Vrock, "hezrou" }),
                "Another creature cannot stand in as the victim.");
        }

        internal static void OwnedBodyDecodesAndIsDistinct()
        {
            string path = "assets/sprint37-vrock/vrock-mesh.json";
            var payload = JObject.Parse(File.ReadAllText(path));
            OriginalMeshOrientation.AssertShippedBody(path,
                VrockVisualPolicy37.Vrock);
            byte[] data = Convert.FromBase64String((string)payload["data"]);
            int vertices = (int)payload["vertexCount"];
            int triangles = (int)payload["triangleCount"];
            Assertions.Equal(vertices * 64 + triangles * 12, data.Length,
                "The Sprint 37 decoder layout is complete.");
            for (int index = 0; index < vertices * 8; index++)
            {
                float value = BitConverter.ToSingle(data, index * 4);
                Assertions.True(!float.IsNaN(value) &&
                    !float.IsInfinity(value),
                    "Sprint 37 vertex, normal and UV values are finite.");
            }
            Assertions.True(payload["bones"].Values<string>()
                .SequenceEqual(InfernalVisualPolicy36.Bones),
                "The body rides the same measured rig order the released outsiders use.");
            Assertions.Equal(InfernalVisualPolicy36.RigSha256,
                (string)payload["rigSha256"],
                "The exact measured capture behind the released bodies is the one used here.");
            var albedo = payload["albedo"];
            using (var hash = SHA256.Create())
            {
                string actual = string.Concat(hash.ComputeHash(
                    File.ReadAllBytes(Path.Combine(
                        Path.GetDirectoryName(path),
                        (string)albedo["file"]))).Select(value =>
                            value.ToString("x2")));
                Assertions.Equal((string)albedo["sha256"], actual,
                    "The runtime albedo agrees with its Sprint 37 mesh.");
            }
            Assertions.Equal("vulture-headed-winged-demon",
                (string)payload["distinctSilhouette"],
                "The Vrock keeps its own silhouette rather than a Sprint 36 demon's.");
            Assertions.True((bool)payload["vultureHead"] &&
                (bool)payload["hookedBeak"] &&
                (int)payload["visibleWings"] == 2 &&
                (int)payload["visibleTalons"] == 2 &&
                (int)payload["visiblePrimaries"] == 14,
                "The body carries its vulture head, hooked beak, two wings and taloned feet.");
            Assertions.True((bool)payload["rigidWings"] &&
                !(bool)payload["articulatedWingBones"],
                "The wings are recorded as rigid rather than claimed as articulated flight.");

            // Distinct bodies, not one shape used three times.
            foreach (string other in new[] {
                "assets/sprint36-demons/babau-mesh.json",
                "assets/sprint36-demons/hezrou-mesh.json" })
            {
                var released = JObject.Parse(File.ReadAllText(other));
                Assertions.False((string)released["data"] ==
                    (string)payload["data"],
                    "The Vrock body is not a copy of a released demon body.");
                Assertions.False(
                    (string)released["distinctSilhouette"] ==
                    (string)payload["distinctSilhouette"],
                    "The Vrock silhouette label is its own.");
                Assertions.False(
                    (string)released["albedo"]["sha256"] ==
                    (string)albedo["sha256"],
                    "The Vrock albedo is its own painting.");
            }
        }
    }
}
