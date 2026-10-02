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
        /// children - and five mechanical ones: the soldier's sting, its poison
        /// feature and venom buff, the beetle's luminescence, and the soldier's
        /// grab traits carrier.
        /// </summary>
        internal const int AppendedLedgerIdentities = 104;

        private static readonly string[] InsectKeys =
            { "fire-beetle", "giant-ant-worker", "giant-ant-soldier" };

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
            if (mine.Any(SummonVisibilityCatalog.IsPublished))
                throw new InvalidOperationException(
                    "No Sprint 14 placement may be published before it qualifies.");
            if (SummonVisibilityCatalog.PublishedLogicalPlacementCount != 904)
                throw new InvalidOperationException(
                    "The published surface must stay at 904 while they are withheld.");
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
        /// The mesh may not weight anything to the donor's fourth foot.
        ///
        /// <para>The Giant Spider has four leg chains a side and an insect has
        /// three. The ants weight nothing to the fourth chain at all and the
        /// beetle weights its wings only to that chain's upper and lower bones,
        /// so no part of a wing can reach the ground. The runtime's allowed-bone
        /// list is what refuses a mesh that broke that, and this is what refuses
        /// a change to the list.</para>
        /// </summary>
        internal static void NoSprint14MeshMayBindTheDonorsFourthFoot()
        {
            string runtime = Source("Assets", "PteranodonAssetRuntime.cs");
            int start = runtime.IndexOf("AllowedGiantSpiderBones",
                StringComparison.Ordinal);
            if (start < 0)
                throw new InvalidOperationException(
                    "The Giant Spider bone allow-list must exist.");
            int end = runtime.IndexOf("};", start, StringComparison.Ordinal);
            string allowed = runtime.Substring(start, end - start);
            foreach (string bone in new[] { "L_Foot3", "R_Foot3" })
                if (allowed.Contains(bone))
                    throw new InvalidOperationException(
                        "The donor's fourth foot must stay unbindable: " + bone);
            foreach (string bone in new[] { "L_Leg3_Upper", "R_Leg3_Upper",
                "chelicera_L", "chelicera_R", "Tail1_M" })
                if (!allowed.Contains(bone))
                    throw new InvalidOperationException(
                        "A bone the Sprint 14 meshes need is not allowed: " + bone);
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
    }
}
