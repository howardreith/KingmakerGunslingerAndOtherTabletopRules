using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using KingmakerGunslinger.RuntimeTesting;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Sprint22RulesTests
    {
        internal const int AppendedLedgerIdentities = 6;
        internal static void OriginalAssetHasValidGeometryAndReviewedDrivers()
        {
            var data=JObject.Parse(File.ReadAllText("assets/ankylosaurus/ankylosaurus-mesh.json"));
            var names=data["bones"].Values<string>().ToArray();
            Assertions.True(AnkylosaurusVisualPolicy.IsPermitted((string)data["creature"],names),"All 24 driver names and exact creature identity.");
            int vertices=(int)data["vertexCount"],triangles=(int)data["triangleCount"];
            byte[] blob=Convert.FromBase64String((string)data["data"]);
            Assertions.Equal(vertices*64+triangles*12,blob.Length,"Complete geometry, normal, UV, triangle and four-weight payload.");
            for (int offset=0;offset<vertices*32;offset+=4)
                Assertions.True(!float.IsNaN(BitConverter.ToSingle(blob,offset)) && !float.IsInfinity(BitConverter.ToSingle(blob,offset)),"Finite geometry/normal/UV.");
            int at=vertices*32;
            for (int i=0;i<triangles*3;i++,at+=4)
                Assertions.True(BitConverter.ToInt32(blob,at)>=0 && BitConverter.ToInt32(blob,at)<vertices,"Triangle references an owned vertex.");
            var used=new System.Collections.Generic.HashSet<int>();
            for (int i=0;i<vertices;i++)
            {
                float total=0;
                for (int slot=0;slot<4;slot++,at+=8)
                {
                    int bone=BitConverter.ToInt32(blob,at);float weight=BitConverter.ToSingle(blob,at+4); total+=weight;
                    Assertions.True(weight>=0 && weight<=1,"Normalized nonnegative influence.");
                    if (weight>0) { Assertions.True(bone>=0 && bone<names.Length,"Only reviewed drivers.");used.Add(bone); }
                }
                Assertions.True(Math.Abs(total-1f)<.001f,"Each vertex has normalized weights.");
            }
            Assertions.Equal(names.Length,used.Count,"Every declared driver carries original geometry.");
            using(var hash=SHA256.Create())
            {
                string actual=BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes("assets/ankylosaurus/ankylosaurus-albedo.png"))).Replace("-","").ToLowerInvariant();
                Assertions.Equal((string)data["albedo"]["sha256"],actual,"Paint and geometry declare the same original painting.");
            }
            Assertions.True((bool)data["terminalClub"] && (int)data["visibleLegs"]==4 && (int)data["tailJoints"]==6,"Original anatomy contract.");
        }

        internal static void DiagnosticValuesSurviveSerialization()
        {
            var row=new JObject { ["unit"]="owned-anky",["predicate"]="one-save",["event"]="native-tail-hit",
                ["owner"]="private-rider",["d20"]=17,["dc"]=23,["passed"]=false,["remaining"]=5.75 };
            var read=JObject.Parse(BoundaryObservationPolicy.Serialize(row));
            Assertions.Equal(23,(int)read["dc"],"Primitive integers survive repeated diagnostics.");
            Assertions.True(!(bool)read["passed"] && (double)read["remaining"]==5.75,"Native outcomes and fractional lifetime remain values.");
            Assertions.True(new[] {"unit","predicate","event","owner"}.All(key=>!string.IsNullOrEmpty((string)read[key])),"Failure names unit, predicate, event and owner.");
        }
        internal static void HitsAndCriticalsAreAlternatives()
        {
            Assertions.Equal(AnkylosaurusControl.Dazed, AnkylosaurusRulesPolicy.Decide(true, true, true, true, true, false, false), "Ordinary hit has one daze rider.");
            Assertions.Equal(AnkylosaurusControl.Stunned, AnkylosaurusRulesPolicy.Decide(true, true, true, true, true, true, false), "Confirmed critical replaces daze.");
            Assertions.Equal(AnkylosaurusControl.Dazed, AnkylosaurusRulesPolicy.Decide(true, true, true, true, true, true, true), "Critical immunity retains the ordinary hit result.");
            Assertions.Equal(1, AnkylosaurusRulesPolicy.Rounds(AnkylosaurusControl.Dazed, 4), "Ordinary duration uses no critical die.");
            for (int roll = 1; roll <= 4; roll++)
                Assertions.Equal(roll, AnkylosaurusRulesPolicy.Rounds(AnkylosaurusControl.Stunned, roll), "Native d4 is not multiplied.");
        }

        internal static void NegativeControlsCannotManufactureControl()
        {
            Assertions.Equal(AnkylosaurusControl.None, AnkylosaurusRulesPolicy.Decide(false,true,true,true,true,true,false), "Foreign owner.");
            Assertions.Equal(AnkylosaurusControl.None, AnkylosaurusRulesPolicy.Decide(true,false,true,true,true,true,false), "Foreign weapon.");
            Assertions.Equal(AnkylosaurusControl.None, AnkylosaurusRulesPolicy.Decide(true,true,false,true,true,true,false), "Miss, even with stale confirmation.");
            Assertions.Equal(AnkylosaurusControl.None, AnkylosaurusRulesPolicy.Decide(true,true,true,false,true,true,false), "No living victim.");
            Assertions.Equal(AnkylosaurusControl.None, AnkylosaurusRulesPolicy.Decide(true,true,true,true,false,true,false), "Duplicate callback.");
        }

        internal static void LiveStrengthAndRacialDiceDriveTheSave()
        {
            Assertions.Equal(23, AnkylosaurusRulesPolicy.DifficultyClass(10,8), "Unbuffed Strength 27.");
            Assertions.Equal(25, AnkylosaurusRulesPolicy.DifficultyClass(10,10), "Strength increase reaches the same production policy.");
            Assertions.Equal(19, AnkylosaurusRulesPolicy.DifficultyClass(10,4), "Strength loss lowers the DC.");
            Assertions.Equal(21, AnkylosaurusRulesPolicy.DifficultyClass(6,8), "Only racial HD contribute.");
            Assertions.Equal(3, AnkylosaurusRulesPolicy.ReachBonusFeet,
                "Measured native twelve-foot reach gets only the missing three feet.");
            Assertions.Equal(9, AnkylosaurusRulesPolicy.TailTypeStoredRangeFeet,
                "Native weapon-range transform exposes five feet from the private tail type.");
            Assertions.Equal(2, AnkylosaurusRulesPolicy.PowerAttackAdditionalDamage,
                "Native lone-tail Power Attack gets only its missing 1.5x increment.");
        }

        internal static void ReplaysAndDistinctStrikesAreDiscriminated()
        {
            var events = new ReferenceEventGate();
            var strike = new object();
            Assertions.True(events.TryMark(strike), "First callback.");
            Assertions.True(!events.TryMark(strike), "Same event cannot reroll or refresh.");
            Assertions.True(events.TryMark(new object()), "A distinct later hit remains legal.");
            Assertions.True(events.TryMark(new object()), "Another source's strike remains legal.");
        }

        internal static void TheProfileHasItsPrintedCarriers()
        {
            var profile = ExpandedSummoningNaturalProfiles.All.Single(value => value.Key == AnkylosaurusRulesPolicy.Key);
            Assertions.Equal("Animal", profile.HitDieClass, "Racial chassis.");
            Assertions.Equal(10, profile.HitDice, "Racial HD.");
            Assertions.Equal("Huge", profile.Size, "Published size.");
            Assertions.Equal("Tail3d6", profile.PrimaryWeapon, "Published private tail.");
            Assertions.Equal(0, profile.AdditionalWeapons.Count + profile.AdditionalSecondaryWeapons.Count, "One natural weapon.");
            Assertions.True(profile.Facts.OrderBy(value => value).SequenceEqual(new[] {"GreatFortitude","PowerAttack","ImprovedBullRush","WeaponFocusTail","TripDefenseFourLegs","AnkylosaurusChassis","AnkylosaurusStun"}.OrderBy(value => value)), "All supported printed carriers, private reach chassis and mandatory rider.");
            Assertions.True(profile.Skills.SequenceEqual(new[] { "Perception" }), "Exact printed skill allocation.");
        }
        internal static void PublishedRoutesRemainAtFiveThroughNine()
        {
            ExpandedSummoningCatalog.Validate();
            var identities=ExpandedSummoningIdentityCatalog.Build();
            var tail=identities.Skip(identities.Count-AppendedLedgerIdentities).Select(value=>value.Symbol).ToArray();
            Assertions.True(tail.SequenceEqual(new[] { AnkylosaurusRulesPolicy.UnitTypeSymbol,
                AnkylosaurusRulesPolicy.StunFeatureSymbol,AnkylosaurusRulesPolicy.DazedSymbol,
                AnkylosaurusRulesPolicy.StunnedSymbol,AnkylosaurusRulesPolicy.ChassisFeatureSymbol,
                AnkylosaurusRulesPolicy.TailTypeSymbol }),"Exactly six private carrier identities append in order; live-readback corrections add no unit, route or weapon.");
            foreach (var family in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
            {
                var routes = ExpandedSummoningCatalog.GenerateVariants(family).Where(value => value.Creature.Key == "ankylosaurus").OrderBy(value => value.ParentTier).ToArray();
                Assertions.Equal(5, routes.Length, "Five existing routes in " + family);
                for (int i = 0; i < routes.Length; i++)
                {
                    Assertions.Equal(i + 5, routes[i].ParentTier, "Existing parent level.");
                    Assertions.Equal(5, routes[i].SourceTier, "Creature remains at tier V.");
                    Assertions.Equal(i == 0 ? SummonMultiplicity.One : i == 1 ? SummonMultiplicity.OneD3 : SummonMultiplicity.OneD4PlusOne, routes[i].Multiplicity, "Existing quantities.");
                }
            }
            var creature = ExpandedSummoningCatalog.All.Single(value => value.Key == "ankylosaurus");
            Assertions.Equal(SummonTemplatePolicy.CelestialOrFiendish, creature.TemplatePolicy(SummonFamily.Monster), "SM children retained.");
            Assertions.Equal(SummonTemplatePolicy.CasterAlignment, creature.TemplatePolicy(SummonFamily.NaturesAlly), "SNA behavior retained.");
        }

        internal static void DonorProbeHasAClosedSelection()
        {
            Sprint22DonorSurveyPolicy.Validate();
            Assertions.True(Sprint22DonorSurveyPolicy.Donors.Select(value => value[0]).SequenceEqual(new[] { "hodag", "monitor", "mastodon" }), "Only relevant quadrupeds.");
            Assertions.True(RuntimeTestScenarioCatalog.IsAllowed(RuntimeTestScenarioCatalog.ObserveExpandedSummoningSprint22Contracts), "Production request allowlist resolves.");
            Assertions.True(!RuntimeTestScenarioCatalog.IsAllowed("observe-expanded-summoning-sprint22-census-all"), "Unselected broad census rejected.");
        }
    }
}
