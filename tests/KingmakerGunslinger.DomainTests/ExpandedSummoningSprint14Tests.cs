using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 14's three insects: the Fire Beetle, the Giant Ant (Worker) and
    /// the Giant Ant (Soldier), all on the Giant Spider rig.
    ///
    /// <para>These assert behaviour rather than that a source file contains a
    /// token, except where the contract genuinely is a policy boundary that
    /// cannot be expressed behaviourally in a source-only test: the append-only
    /// ledger, and the fact that the three creatures are withheld.</para>
    /// </summary>
    internal static class ExpandedSummoningSprint14Tests
    {
        /// <summary>
        /// Ninety-nine structural identities - three units and their 48 logical
        /// placements with the Summon Monster side's celestial and fiendish
        /// children - and eight mechanical ones: the soldier's sting, its
        /// poison feature and venom buff, the beetle's luminescence, the
        /// soldier's grab traits carrier, the ants' racial Perception, and a
        /// unit type each for the beetle and the ants so neither is classified
        /// as the Giant Spider it borrows.
        /// </summary>
        internal const int AppendedLedgerIdentities = 107;

        private static readonly string[] InsectKeys =
            { "fire-beetle", "giant-ant-worker", "giant-ant-soldier" };

        private const string FireBeetleKey =
            Sprint14BonePolicy.FireBeetleKey;
        private const string WorkerKey = Sprint14BonePolicy.WorkerKey;
        private const string SoldierKey = Sprint14BonePolicy.SoldierKey;

        private static string Source(params string[] parts)
        {
            string[] all = new[] { Environment.CurrentDirectory, "src",
                "KingmakerGunslinger" }.Concat(parts).ToArray();
            return File.ReadAllText(Path.Combine(all));
        }

        /// <summary>
        /// The printed Fortitude DC 14 is what the standard formula produces on
        /// its own, and nothing about it is written down as a constant.
        ///
        /// <para>Ten, plus half of two hit dice, plus a +3 Constitution
        /// modifier, and no racial bonus - the Giant Wasp has one and the ant
        /// does not. If a later change to the chassis moved the creature's
        /// Constitution, a hard-coded fourteen would keep printing fourteen and
        /// quietly stop matching the stat block; this stops matching here
        /// instead.</para>
        /// </summary>
        internal static void AntPoisonDcDerivesToThePrintedFourteen()
        {
            if (GiantAntPoisonPolicy.DifficultyClass(3) != 14)
                throw new InvalidOperationException(
                    "A Constitution 17 soldier's sting must be Fortitude DC 14.");
            if (GiantAntPoisonPolicy.RacialDcBonus != 0)
                throw new InvalidOperationException(
                    "The Giant Ant has no racial bonus to its poison DC.");
            // It scales, which is the whole reason it is not a constant.
            if (GiantAntPoisonPolicy.DifficultyClass(0) != 11 ||
                GiantAntPoisonPolicy.DifficultyClass(5) != 16)
                throw new InvalidOperationException(
                    "The poison DC must follow the Constitution modifier.");
        }

        /// <summary>
        /// The printed frequency: 1/round for 4 rounds, cured by one save.
        /// Four exposures, not the Giant Wasp's six.
        /// </summary>
        internal static void AntPoisonCarriesThePrintedFrequency()
        {
            if (GiantAntPoisonPolicy.Exposures != 4)
                throw new InvalidOperationException(
                    "The printed poison is 1/round for 4 rounds.");
            if (GiantAntPoisonPolicy.SavesToCure != 1)
                throw new InvalidOperationException(
                    "The printed poison is cured by one successful save.");
            if (GiantAntPoisonPolicy.Exposures ==
                    GiantWaspPoisonPolicy.Exposures)
                throw new InvalidOperationException(
                    "The ant's poison must not inherit the wasp's frequency.");
        }

        /// <summary>
        /// The Worker template removes the sting and the grab, which leaves a
        /// bite alone. So the two castes share one chassis and differ in
        /// exactly one attack.
        /// </summary>
        internal static void TheWorkerIsTheSoldierWithoutItsSting()
        {
            NaturalSummonProfile soldier =
                ExpandedSummoningNaturalProfiles.For("giant-ant-soldier");
            NaturalSummonProfile worker =
                ExpandedSummoningNaturalProfiles.For("giant-ant-worker");
            foreach (var pair in new[] {
                Tuple.Create("strength", soldier.Strength, worker.Strength),
                Tuple.Create("constitution", soldier.Constitution,
                    worker.Constitution),
                Tuple.Create("hit dice", soldier.HitDice, worker.HitDice),
                Tuple.Create("natural armor", soldier.NaturalArmor,
                    worker.NaturalArmor),
                Tuple.Create("speed", soldier.SpeedFeet, worker.SpeedFeet) })
                if (pair.Item2 != pair.Item3)
                    throw new InvalidOperationException(
                        "The two ant castes share a chassis: " + pair.Item1);
            if (soldier.PrimaryWeapon != "Bite1d6" ||
                worker.PrimaryWeapon != "Bite1d6")
                throw new InvalidOperationException(
                    "Both castes bite with the printed 1d6.");
            if (worker.AdditionalWeapons.Count != 0)
                throw new InvalidOperationException(
                    "The Worker template removes the sting entirely.");
            if (soldier.AdditionalWeapons.Count != 1 ||
                soldier.AdditionalWeapons[0] != "AntSting1d4")
                throw new InvalidOperationException(
                    "The soldier stings with the printed 1d4 and nothing else.");
            if (worker.Facts.Contains("GiantAntPoison"))
                throw new InvalidOperationException(
                    "A worker has no poison, because it has nothing to deliver it.");
            if (!soldier.Facts.Contains("GiantAntPoison"))
                throw new InvalidOperationException(
                    "The soldier's sting must carry its printed poison.");
        }

        /// <summary>
        /// The sting is a distinct weapon so the poison can gate on its type.
        ///
        /// <para>This is the whole reason the weapon exists. Grab is gated by
        /// limb position and poison by weapon type, and the two gates only stay
        /// independent while the bite and the sting are different weapons. If
        /// the soldier's sting were ever made the same weapon as its bite, the
        /// poison would start firing on the attack that grabs.</para>
        /// </summary>
        internal static void TheStingIsItsOwnWeaponSoThePoisonCannotReachTheBite()
        {
            NaturalSummonProfile soldier =
                ExpandedSummoningNaturalProfiles.For("giant-ant-soldier");
            if (soldier.AdditionalWeapons.Contains(soldier.PrimaryWeapon))
                throw new InvalidOperationException(
                    "The soldier's sting must not be the same weapon as its bite.");
            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            // A policy boundary rather than a behaviour: the trigger is built
            // against Kingmaker types this suite cannot construct, so what can
            // be asserted here is that the gate is the sting's own type.
            if (!builder.Contains("trigger.WeaponType = sting.Type") ||
                !builder.Contains("ConfigureGiantAntPoison"))
                throw new InvalidOperationException(
                    "The ant's poison must gate on the sting's own weapon type.");
            string special = Source("Blueprints",
                "ExpandedSummoningSpecialBuilder.cs");
            if (!special.Contains("GiantAntSoldierUnitSymbol") ||
                !special.Contains("new GrabSpec { Primary = true, Hold = hold, Grappled = grappled }"))
                throw new InvalidOperationException(
                    "The soldier's grab must ride the shared lifecycle on the primary limb.");
        }

        /// <summary>
        /// Luminescence grants and denies nothing, because there is nothing for
        /// it to grant or deny against.
        ///
        /// <para>Sprint 13 established that Kingmaker has no mechanics-layer
        /// illumination model. The feature therefore carries no components at
        /// all, and its description says so in as many words rather than
        /// implying a rule the game does not have.</para>
        /// </summary>
        internal static void LuminescenceClaimsNoMechanicalEffect()
        {
            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            if (!builder.Contains(
                    "feature.ComponentsArray = Array.Empty<BlueprintComponent>()"))
                throw new InvalidOperationException(
                    "Luminescence must carry no components.");
            if (!builder.Contains("neither reveals nor conceals anything"))
                throw new InvalidOperationException(
                    "Luminescence must say plainly that it does nothing.");
            NaturalSummonProfile beetle =
                ExpandedSummoningNaturalProfiles.For("fire-beetle");
            if (!beetle.Facts.Contains("FireBeetleLuminescence"))
                throw new InvalidOperationException(
                    "The Fire Beetle must carry its luminescence feature.");
            if (beetle.Deviations.Count == 0 || !beetle.Deviations.Any(value =>
                    value.Contains("no mechanics-layer illumination model")))
                throw new InvalidOperationException(
                    "The engine limitation must be recorded on the creature.");
            // Its bite is a plain 1d4 and it has no fire damage anywhere.
            if (beetle.PrimaryWeapon != "Bite1d4" ||
                beetle.AdditionalWeapons.Count != 0)
                throw new InvalidOperationException(
                    "A fire beetle bites for 1d4 and does nothing else.");
        }

        /// <summary>
        /// All three are registered and all three are withheld, so the
        /// published surface does not move until they qualify.
        ///
        /// <para>A creature at tier T occupies parents T..9, so it is 10 - T
        /// placements per family: the beetle nine, the worker eight, the
        /// soldier seven, in each of two families. Forty-eight in all.</para>
        /// </summary>
        internal static void TheThreeInsectsAreRegisteredAndWithheld()
        {
            SummonVariantSpec[] all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            SummonVariantSpec[] mine = all.Where(value =>
                InsectKeys.Contains(value.Creature.Key)).ToArray();
            if (mine.Length != 48)

                throw new InvalidOperationException(
                    "The three insects register 48 placements, not " +
                    mine.Length + ".");
            // The per-creature split, because publication is per creature and
            // the arithmetic has to survive publishing one of the three
            // without the other two. A creature held back for a proven engine
            // barrier must subtract exactly its own placements and no others.
            foreach (var expected in new[] {
                new { Key = FireBeetleKey, Placements = 18 },
                new { Key = WorkerKey, Placements = 16 },
                new { Key = SoldierKey, Placements = 14 } })
            {
                int actual = mine.Count(value =>
                    value.Creature.Key == expected.Key);
                if (actual != expected.Placements)
                    throw new InvalidOperationException(expected.Key +
                        " registers " + actual + " placements, not " +
                        expected.Placements + ", so the publication " +
                        "arithmetic for a partial publication is wrong.");
            }
            // The Fire Beetle publishes; the two ant castes do not, and the
            // reason is an engine barrier rather than a failure of theirs.
            // Kingmaker has no scent mechanic at all, so their printed scent
            // cannot be implemented and must not be approximated with
            // blindsight, and the Sprint 14 order's instruction for that
            // outcome is to hold the affected creature pending one owner
            // ruling.
            string[] wronglyWithheld = mine
                .Where(value => value.Creature.Key == FireBeetleKey &&
                    !SummonVisibilityCatalog.IsPublished(value))
                .Select(value => value.StableKey).ToArray();
            if (wronglyWithheld.Length != 0)
                throw new InvalidOperationException(
                    "The Fire Beetle qualified and must be published: " +
                    string.Join(", ", wronglyWithheld));
            string[] wronglyPublished = mine
                .Where(value => value.Creature.Key != FireBeetleKey &&
                    SummonVisibilityCatalog.IsPublished(value))
                .Select(value => value.StableKey).ToArray();
            if (wronglyPublished.Length != 0)
                throw new InvalidOperationException(
                    "A Giant Ant caste is published while its printed scent " +
                    "has no engine representation and no owner ruling: " +
                    string.Join(", ", wronglyPublished));
            // 904 before Sprint 14, plus the Fire Beetle's 18.
            if (SummonVisibilityCatalog.PublishedLogicalPlacementCount != 922)
                throw new InvalidOperationException(
                    "The published surface must be 922: the 904 published " +
                    "before Sprint 14 plus the Fire Beetle's 18, with the " +
                    "two ant castes' 30 still withheld. It is " +
                    SummonVisibilityCatalog
                        .PublishedLogicalPlacementCount + ".");
            if (SummonVisibilityCatalog.RegisteredLogicalPlacementCount -
                    SummonVisibilityCatalog.SuppressedLogicalPlacementCount !=
                    SummonVisibilityCatalog.PublishedLogicalPlacementCount)
                throw new InvalidOperationException(
                    "The published surface must be the registered one less the withheld.");
            // Every creature that has already qualified is published, which is
            // the other direction of the same contract.
            foreach (string key in new[] { "shadow-mastiff", "wolverine",
                "poisonous-frog", "dire-rat", "goblin-dog" })
                if (!all.Where(value => value.Creature.Key == key)
                        .All(SummonVisibilityCatalog.IsPublished))
                    throw new InvalidOperationException(
                        "A qualified creature is withheld: " + key);
        }

        /// <summary>
        /// Each insect appears on both families from its own tier upward, with
        /// the printed quantity progression.
        /// </summary>
        internal static void EachInsectOccupiesItsTierUpward()
        {
            var expected = new Dictionary<string, int> {
                { "fire-beetle", 1 }, { "giant-ant-worker", 2 },
                { "giant-ant-soldier", 3 } };
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            {
                IReadOnlyList<SummonVariantSpec> variants =
                    ExpandedSummoningCatalog.GenerateVariants(family);
                foreach (var pair in expected)
                {
                    SummonVariantSpec[] rows = variants.Where(value =>
                        value.Creature.Key == pair.Key).OrderBy(value =>
                        value.ParentTier).ToArray();
                    if (rows.Length != 10 - pair.Value)
                        throw new InvalidOperationException(
                            pair.Key + " must occupy tiers " + pair.Value +
                            " to 9 in " + family + ".");
                    if (rows[0].ParentTier != pair.Value ||
                        rows[0].Multiplicity != SummonMultiplicity.One ||
                        rows[1].Multiplicity != SummonMultiplicity.OneD3 ||
                        rows.Skip(2).Any(value =>
                            value.Multiplicity != SummonMultiplicity.OneD4PlusOne))
                        throw new InvalidOperationException(
                            pair.Key + " must follow the printed quantity progression.");
                }
            }
        }

        /// <summary>
        /// The 104 new identities are declared, unique, active, and appended
        /// after everything that came before them. A reallocated identity would
        /// move a creature a player had already summoned.
        /// </summary>
        internal static void Sprint14IdentitiesAreDeclaredAndAppendOnly()
        {
            string path = Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json");
            JArray entries = (JArray)JObject.Parse(
                File.ReadAllText(path))["entries"];
            string[] symbols = entries.Select(value =>
                (string)value["symbol"]).ToArray();
            string[] mine = symbols.Where(value =>
                value.Contains(".FireBeetle") || value.Contains(".GiantAnt") ||
                value == "KMG.Summoning.Natural.AntSting1d4").ToArray();
            if (mine.Length != AppendedLedgerIdentities)
                throw new InvalidOperationException(
                    "Sprint 14 appends " + AppendedLedgerIdentities +
                    " identities, not " + mine.Length + ".");
            if (mine.Distinct(StringComparer.Ordinal).Count() != mine.Length)
                throw new InvalidOperationException(
                    "A Sprint 14 identity is declared twice.");
            int first = Array.IndexOf(symbols, mine[0]);
            if (first + mine.Length != symbols.Length)
                throw new InvalidOperationException(
                    "Sprint 14 identities must be the ledger's final append.");
            foreach (JToken entry in entries.Skip(first))
            {
                if ((string)entry["status"] != "active")
                    throw new InvalidOperationException(
                        "Sprint 14 identities must be registered in every module state.");
                string guid = (string)entry["guid"];
                if (guid == null || guid.Length != 32 ||
                    guid.Any(value => !Uri.IsHexDigit(value) ||
                        char.IsUpper(value)))
                    throw new InvalidOperationException(
                        "A Sprint 14 identity is not a lowercase 32-hex GUID.");
            }
        }

        /// <summary>
        /// No Sprint 14 creature may drive the donor's fourth foot, and the
        /// bones each kind does need are all present.
        ///
        /// <para>The allowlists moved out of the Unity-bound loader into pure
        /// policy so a corruption test could exercise the real rejection; this
        /// reads the same policy the loader calls.</para>
        /// </summary>
        internal static void NoSprint14MeshMayBindTheDonorsFourthFoot()
        {
            foreach (string key in InsectKeys)
            {
                string[] allowed = Sprint14BonePolicy.AllowedBones(key);
                foreach (string foot in Sprint14BonePolicy.FourthChainFeet)
                    if (allowed.Contains(foot))
                        throw new InvalidOperationException(
                            key + " may not drive " + foot + ".");
                foreach (string bone in new[] { "chelicera_L", "chelicera_R",
                    "Tail1_M", "L_Leg0_Upper", "R_Foot2" })
                    if (!allowed.Contains(bone))
                        throw new InvalidOperationException(
                            key + " needs " + bone + " and does not have it.");
            }
            string runtime = Source("Assets", "PteranodonAssetRuntime.cs");
            if (!runtime.Contains("Sprint14BonePolicy.AllowedBones(key)"))
                throw new InvalidOperationException(
                    "The loader must call the policy rather than keep its own list.");
            if (runtime.Contains("AllowedGiantSpiderBones"))
                throw new InvalidOperationException(
                    "The shared allowlist that could not enforce the ant contract must be gone.");
        }

        /// <summary>
        /// Each shipped mesh says how many legs it has, and all three say six.
        /// The generator computes it from the geometry rather than being told.
        /// </summary>
        internal static void EachShippedInsectMeshDeclaresSixVisibleLegs()
        {
            foreach (string key in InsectKeys)
            {
                string path = Path.Combine(Environment.CurrentDirectory,
                    "assets", "sprint14-insects", key + "-mesh.json");
                JObject payload = JObject.Parse(File.ReadAllText(path));
                string[] bones = payload["bones"].Values<string>().ToArray();
                // Recomputed from the bones the payload actually ships rather
                // than read from what it claims, so a mesh that declared six
                // legs while binding a seventh fails here.
                int full = 0;
                for (int chain = 0; chain <= 3; chain++)
                {
                    int sides = 0;
                    foreach (string side in new[] { "L", "R" })
                    {
                        bool upper = bones.Contains(
                            side + "_Leg" + chain + "_Upper");
                        bool lower = bones.Contains(
                            side + "_Leg" + chain + "_Lower");
                        bool foot = bones.Contains(side + "_Foot" + chain);
                        if (foot && !(upper && lower))
                            throw new InvalidOperationException(
                                key + " binds a foot without its leg: chain " +
                                chain + " on " + side + ".");
                        if (upper && lower && foot) sides++;
                        else if (upper || lower || foot)
                        {
                            // The beetle's wings ride the spare chain's upper
                            // and lower bones and nothing else. Any other
                            // partial binding is a defect.
                            if (chain != 3 || foot || key != "fire-beetle")
                                throw new InvalidOperationException(
                                    key + " binds chain " + chain + " on " +
                                    side + " partially.");
                        }
                    }
                    if (sides == 1)
                        throw new InvalidOperationException(
                            key + " binds leg chain " + chain +
                            " on one side only.");
                    if (sides == 2) full++;
                }
                if (full != 3)
                    throw new InvalidOperationException(
                        key + " must walk on three leg chains a side, not " +
                        full + ".");
                if ((int?)payload["visibleLegs"] != 6)
                    throw new InvalidOperationException(
                        key + " must declare the six legs it binds.");
                foreach (string foot in new[] { "L_Foot3", "R_Foot3" })
                    if (bones.Contains(foot))
                        throw new InvalidOperationException(
                            key + " may never bind the donor's fourth foot: " +
                            foot + ".");
            }
        }
        /// <summary>
        /// A six-legged ant may not drive any part of the donor's fourth leg
        /// chain, and the beetle may drive only its two reviewed wing bones.
        ///
        /// <para>One shared allowlist could only ever have excluded the fourth
        /// feet, because the beetle's wings legitimately ride that chain's
        /// upper and lower bones. It therefore enforced the beetle's contract
        /// and not the ant's: an ant mesh weighting an eighth leg from the knee
        /// up would have loaded, with nothing between it and a player but the
        /// offline generator, which does not run on the file the game
        /// reads.</para>
        /// </summary>
        internal static void EachInsectGetsOnlyTheBonesItsOwnKindMayDrive()
        {
            foreach (string key in new[] { WorkerKey, SoldierKey })
            {
                foreach (string bone in Sprint14BonePolicy.FourthChainWingDrivers)
                    if (Sprint14BonePolicy.IsPermitted(key, new[] { bone }))
                        throw new InvalidOperationException(
                            key + " must not be able to drive " + bone + ".");
                if (Sprint14BonePolicy.AllowedBones(key).Any(value =>
                        value.Contains("Leg3") || value.Contains("Foot3")))
                    throw new InvalidOperationException(
                        key + "'s allowlist must not mention the fourth chain.");
            }
            foreach (string bone in Sprint14BonePolicy.FourthChainWingDrivers)
                if (!Sprint14BonePolicy.IsPermitted(FireBeetleKey,
                        new[] { bone }))
                    throw new InvalidOperationException(
                        "The Fire Beetle's wings need " + bone + ".");
            // Neither kind, ever.
            foreach (string key in new[] { WorkerKey, SoldierKey,
                FireBeetleKey })
                foreach (string foot in Sprint14BonePolicy.FourthChainFeet)
                    if (Sprint14BonePolicy.IsPermitted(key, new[] { foot }))
                        throw new InvalidOperationException(
                            key + " must never drive " + foot + ".");
            // An unreviewed key gets the narrower list, not the wider one.
            if (Sprint14BonePolicy.AllowedBones("giant-ant-drone") !=
                    Sprint14BonePolicy.AntBones)
                throw new InvalidOperationException(
                    "An unreviewed creature must not inherit wing permission.");
        }

        /// <summary>
        /// The shipped meshes pass their own kind's allowlist, and a corrupted
        /// copy of each is refused by the same function the loader calls.
        /// </summary>
        internal static void ACorruptedSprint14MeshIsRefusedByItsOwnAllowlist()
        {
            foreach (string key in InsectKeys)
            {
                string[] bones = ShippedBones(key);
                string offender = Sprint14BonePolicy.FirstForbiddenBone(key,
                    bones);
                if (offender != null)
                    throw new InvalidOperationException(
                        key + " ships a bone it may not drive: " + offender);
                // An eighth leg from the knee up: accepted by the old shared
                // list for every creature, and refused now for the ants.
                string[] eighthLeg = bones.Concat(new[] { "L_Leg3_Upper" })
                    .ToArray();
                bool refused = !Sprint14BonePolicy.IsPermitted(key, eighthLeg);
                if (key == FireBeetleKey ? refused : !refused)
                    throw new InvalidOperationException(
                        key + " handled a fourth-chain upper bone wrongly.");
                // A fourth foot is refused for all three without exception.
                foreach (string foot in Sprint14BonePolicy.FourthChainFeet)
                    if (Sprint14BonePolicy.IsPermitted(key,
                            bones.Concat(new[] { foot }).ToArray()))
                        throw new InvalidOperationException(
                            key + " accepted " + foot + ".");
                // A bone from another donor entirely.
                if (Sprint14BonePolicy.IsPermitted(key,
                        bones.Concat(new[] { "Torso_Lower" }).ToArray()))
                    throw new InvalidOperationException(
                        key + " accepted a bone from another rig.");
            }
        }

        private static string[] ShippedBones(string key)
        {
            string path = Path.Combine(Environment.CurrentDirectory,
                "assets", "sprint14-insects", key + "-mesh.json");
            return JObject.Parse(File.ReadAllText(path))["bones"]
                .Values<string>().ToArray();
        }

        /// <summary>
        /// Both ant castes carry the printed trip defence and the printed
        /// racial bonus, and neither carries the feat that was standing in for
        /// it.
        ///
        /// <para>The builder reconstructs AddFacts from the profile rather than
        /// inheriting the donor's, so a fact left out of the profile is simply
        /// absent at runtime: without this the two castes would have had the
        /// Giant Spider's body and a quadruped's vulnerability to being
        /// tripped.</para>
        /// </summary>
        internal static void BothAntCastesCarryTheirPrintedTripDefenceAndSkills()
        {
            foreach (string key in new[] { WorkerKey, SoldierKey })
            {
                NaturalSummonProfile ant =
                    ExpandedSummoningNaturalProfiles.For(key);
                if (!ant.Facts.Contains("TripDefenseEightLegs"))
                    throw new InvalidOperationException(
                        key + " must carry the multi-legged trip defence.");
                if (!ant.Facts.Contains("GiantAntRacialSkills"))
                    throw new InvalidOperationException(
                        key + " must carry its printed racial Perception.");
                if (ant.Facts.Contains("SkillFocusPerception"))
                    throw new InvalidOperationException(
                        key + " must not carry a feat its stat block omits.");
                if (!ant.Facts.Contains("Toughness"))
                    throw new InvalidOperationException(
                        key + " keeps Toughness, its one printed feat.");
                if (!ant.Deviations.Any(value =>
                        value.Contains("Survival")))
                    throw new InvalidOperationException(
                        key + " must disclose the omitted Survival half.");
            }
            NaturalSummonProfile worker =
                ExpandedSummoningNaturalProfiles.For(WorkerKey);
            NaturalSummonProfile soldier =
                ExpandedSummoningNaturalProfiles.For(SoldierKey);
            foreach (string fact in new[] { "TripDefenseEightLegs",
                "GiantAntRacialSkills", "Toughness" })
                if (worker.Facts.Contains(fact) != soldier.Facts.Contains(fact))
                    throw new InvalidOperationException(
                        "The two castes must agree on " + fact + ".");
        }

        /// <summary>
        /// None of the three may keep the donor's unit type.
        ///
        /// <para>The builder replaces class levels, facts, body, stats and
        /// brain, but it leaves BlueprintUnitType alone unless a creature asks
        /// for its own - only the Giant Wasp used to. All three insects clone
        /// the Giant Spider, so without this a beetle and two ants would be
        /// classified as spiders to the player and to anything that consults
        /// unit type.</para>
        /// </summary>
        internal static void NoSprint14InsectKeepsTheDonorsUnitType()
        {
            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            foreach (string symbol in new[] { "FireBeetleUnitTypeSymbol",
                "GiantAntUnitTypeSymbol" })
                if (!builder.Contains("unit.Type = Require<BlueprintUnitType>(bySymbol,\r\n                    " + symbol) &&
                    !builder.Contains("unit.Type = Require<BlueprintUnitType>(bySymbol,\n                    " + symbol))
                    throw new InvalidOperationException(
                        "No creature assigns " + symbol + ".");
            // One type for both castes: they are one creature in two castes.
            if (!builder.Contains("profile.Key == \"giant-ant-worker\" ||") ||
                !builder.Contains("profile.Key == \"giant-ant-soldier\""))
                throw new InvalidOperationException(
                    "Both ant castes must share one unit type.");
            string path = Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json");
            string[] symbols = ((JArray)JObject.Parse(File.ReadAllText(path))
                ["entries"]).Select(value => (string)value["symbol"]).ToArray();
            foreach (string symbol in new[] {
                "KMG.Summoning.Natural.FireBeetle.UnitType",
                "KMG.Summoning.Natural.GiantAnt.UnitType",
                "KMG.Summoning.Natural.GiantAnt.RacialSkills" })
                if (!symbols.Contains(symbol))
                    throw new InvalidOperationException(
                        "The ledger is missing " + symbol + ".");
        }

        /// <summary>
        /// An injury poison needs an injury, not merely a hit.
        ///
        /// <para>The native graph this project clones fires on OnlyHit, which
        /// is a weaker test than the tabletop rule: an attack that connects but
        /// whose damage is reduced to nothing has hit without wounding. Sprint
        /// 12's bite diseases already gate on positive final damage; the
        /// poisons were left behind, so the Giant Wasp had the same defect and
        /// is corrected with the ant rather than left inconsistent.</para>
        /// </summary>
        internal static void AnInjuryPoisonNeedsAnActualWound()
        {
            if (!SummonInjuryPoisonPolicy.ShouldDeliver(true, true, 1))
                throw new InvalidOperationException(
                    "A wounding hit with the right weapon delivers the poison.");
            if (SummonInjuryPoisonPolicy.ShouldDeliver(true, false, 0))
                throw new InvalidOperationException(
                    "A miss delivers nothing.");
            if (SummonInjuryPoisonPolicy.ShouldDeliver(true, true, 0))
                throw new InvalidOperationException(
                    "A hit reduced to no damage has not wounded.");
            if (SummonInjuryPoisonPolicy.ShouldDeliver(false, true, 6))
                throw new InvalidOperationException(
                    "Another weapon's wound delivers nothing.");
            // The same gate Sprint 12 applies to its bite diseases.
            if (SummonInjuryDiseasePolicy.ShouldResolve(true, true, 0, true,
                    false))
                throw new InvalidOperationException(
                    "The disease gate and the poison gate must agree.");
            string builder = Source("Blueprints",
                "ExpandedSummoningNaturalBuilder.cs");
            if (builder.Split(new[] { "ContextActionOnlyIfWeaponWounded" },
                    StringSplitOptions.None).Length - 1 != 2)
                throw new InvalidOperationException(
                    "Both injury poisons must carry the wound gate.");
        }

        /// <summary>
        /// The beetle's light is matched to the body every frame and released
        /// in the frame it is asked for.
        ///
        /// <para>A Light on a child object is governed by nothing that governs
        /// the creature: not the fader, not the renderer, not the death
        /// dissolve, not culling. Left alone it would keep illuminating the
        /// scene from inside a beetle nobody can see.</para>
        /// </summary>
        internal static void TheBeetleGlowFollowsItsBodyAndReleasesInFrame()
        {
            string glow = Source("Summoning", "FireBeetleVisualGlow.cs");
            foreach (string token in new[] { "LateUpdate", "_view.IsVisible",
                "_renderer.enabled", "activeInHierarchy",
                "DestroyImmediate(carrier)", "_released" })
                if (!glow.Contains(token))
                    throw new InvalidOperationException(
                        "The glow lifecycle is missing " + token + ".");
            // Release is idempotent: it clears what it owns before destroying,
            // so a second call has nothing to destroy.
            if (!glow.Contains("if (carrier == null) return;"))
                throw new InvalidOperationException(
                    "Release must be idempotent.");
            string patch = Source("Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs");
            if (patch.Split(new[] { "BeetleGlow.Release(true)" },
                    StringSplitOptions.None).Length - 1 != 2)
                throw new InvalidOperationException(
                    "Both the release and rollback paths must free the carrier in-frame.");
            // And it still claims nothing mechanical.
            foreach (string forbidden in new[] { "Concealment", "Stealth",
                "Perception" })
                if (glow.Contains(forbidden))
                    throw new InvalidOperationException(
                        "The glow must not reach into " + forbidden + ".");
        }

        /// <summary>
        /// Every Expanded Summoning identity in the ledger is also in the
        /// catalog the game builds its blueprints from, and with the same
        /// planned type.
        ///
        /// <para>This is the guard that was missing. A symbol can be allocated
        /// in the ledger, referenced by a builder and validated by every
        /// offline gate while nothing ever creates its blueprint, because the
        /// catalog is a separate hand-maintained list. The first guarded launch
        /// after Sprint 14's mechanics went in failed at load with a
        /// KeyNotFoundException from Require, and the eight missing symbols had
        /// passed 1991 domain tests, a repository wrapper, an exact-reference
        /// build and a strict package validation on the way there. Nothing
        /// offline could have caught it, so now something does.</para>
        /// </summary>
        internal static void TheIdentityCatalogMatchesTheLedgerExactly()
        {
            string path = Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json");
            JToken[] entries = ((JArray)JObject.Parse(
                File.ReadAllText(path))["entries"]).ToArray();
            var ledger = entries
                .Where(value => ((string)value["symbol"]).StartsWith(
                    "KMG.Summoning.", StringComparison.Ordinal))
                .ToDictionary(value => (string)value["symbol"],
                    value => (string)value["plannedType"],
                    StringComparer.Ordinal);
            var catalog = ExpandedSummoningIdentityCatalog.Build()
                .ToDictionary(value => value.Symbol,
                    value => value.PlannedType, StringComparer.Ordinal);
            string[] uncreated = ledger.Keys.Except(catalog.Keys,
                StringComparer.Ordinal).OrderBy(value => value,
                StringComparer.Ordinal).ToArray();
            if (uncreated.Length != 0)
                throw new InvalidOperationException(
                    "The ledger allocates identities the catalog never creates, " +
                    "so nothing builds their blueprints: " +
                    string.Join(", ", uncreated));
            string[] unallocated = catalog.Keys.Except(ledger.Keys,
                StringComparer.Ordinal).OrderBy(value => value,
                StringComparer.Ordinal).ToArray();
            if (unallocated.Length != 0)
                throw new InvalidOperationException(
                    "The catalog creates identities the ledger never allocated: " +
                    string.Join(", ", unallocated));
            string[] mistyped = catalog.Keys
                .Where(value => catalog[value] != ledger[value])
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (mistyped.Length != 0)
                throw new InvalidOperationException(
                    "The catalog and the ledger disagree about a planned type: " +
                    string.Join(", ", mistyped));
            // And specifically: every Sprint 14 symbol a builder resolves.
            foreach (string symbol in new[] {
                "KMG.Summoning.Natural.AntSting1d4",
                "KMG.Summoning.Natural.GiantAnt.Poison",
                "KMG.Summoning.Natural.GiantAnt.Venom",
                "KMG.Summoning.Natural.GiantAnt.RacialSkills",
                "KMG.Summoning.Natural.GiantAnt.UnitType",
                "KMG.Summoning.Natural.FireBeetle.Luminescence",
                "KMG.Summoning.Natural.FireBeetle.UnitType",
                "KMG.Summoning.Special.GiantAntSoldier.Traits" })
                if (!catalog.ContainsKey(symbol))
                    throw new InvalidOperationException(
                        "A Sprint 14 builder resolves " + symbol +
                        " but nothing creates it.");
        }

        /// <summary>
        /// The printed skill totals come out exactly, and nothing arrives that
        /// the stat block never printed.
        ///
        /// <para>The first attempt implemented the ants' racial +4 Perception
        /// correctly and still produced the wrong creature: a Worker read
        /// Perception 7 against a printed +5, because the generic builder gives
        /// every reconstructed creature automatic Perception, Mobility and
        /// Stealth ranks through AddClassLevels. That is right for the animals
        /// it was written for and wrong for a vermin whose stat block prints no
        /// ranks at all. The racial bonus was never the problem; the total
        /// was.</para>
        ///
        /// <para>So this asserts the arithmetic that has to produce the printed
        /// number - zero ranks, plus the governing attribute, plus the racial
        /// bonus - and asserts that the default is still what it always was for
        /// every other creature, because correcting these three must not move
        /// anything already qualified. The compensating alternative, a negative
        /// racial modifier cancelling ranks that should not exist, would reach
        /// the same total through a stat block the game cannot show honestly,
        /// and is not what is implemented.</para>
        /// </summary>
        internal static void InsectSkillTotalsMatchThePrintedStatBlocks()
        {
            // The default is untouched, so no qualified creature moves.
            string[] expectedDefault = { "Perception", "Mobility", "Stealth" };
            if (!NaturalSummonProfile.DefaultSkills
                    .SequenceEqual(expectedDefault))
                throw new InvalidOperationException(
                    "The generic builder's skill set changed, which moves every " +
                    "previously qualified creature: " + string.Join(", ",
                        NaturalSummonProfile.DefaultSkills));
            string[] movedCreatures = ExpandedSummoningNaturalProfiles.All
                .Where(value => !InsectKeys.Contains(value.Key,
                    StringComparer.Ordinal) &&
                    !value.Skills.SequenceEqual(expectedDefault))
                .Select(value => value.Key).ToArray();
            if (movedCreatures.Length != 0)
                throw new InvalidOperationException(
                    "Correcting the three insects moved creatures that were " +
                    "already qualified: " + string.Join(", ", movedCreatures));

            foreach (var row in new[] {
                new { Key = FireBeetleKey, Perception = 0, Racial = 0 },
                new { Key = WorkerKey, Perception = 5,
                    Racial = NaturalSummonProfile
                        .GiantAntRacialPerceptionBonus },
                new { Key = SoldierKey, Perception = 5,
                    Racial = NaturalSummonProfile
                        .GiantAntRacialPerceptionBonus } })
            {
                NaturalSummonProfile profile =
                    ExpandedSummoningNaturalProfiles.For(row.Key);

                // No ranks at all: the stat block prints none.
                if (profile.Skills.Count != 0)
                    throw new InvalidOperationException(row.Key +
                        " still takes automatic class ranks the stat block " +
                        "never printed: " + string.Join(", ", profile.Skills));

                // Ranks, plus the attribute, plus the racial bonus.
                int wisdomModifier = (profile.Wisdom - 10) / 2;
                int derived = 0 + wisdomModifier + row.Racial;
                if (derived != row.Perception)
                    throw new InvalidOperationException(row.Key +
                        " derives Perception " + derived + " from Wisdom " +
                        profile.Wisdom + " and racial " + row.Racial +
                        ", but its stat block prints " + row.Perception + ".");

                // And the racial bonus is carried as a racial modifier rather
                // than folded into a rank, so the creature's own stat block
                // reads the way the printed one does.
                bool carriesRacialFeature = profile.Facts
                    .Contains("GiantAntRacialSkills");
                if (carriesRacialFeature != (row.Racial != 0))
                    throw new InvalidOperationException(row.Key +
                        " needs racial " + row.Racial +
                        " but carriesRacialSkills=" + carriesRacialFeature);
            }
        }


    }
}
