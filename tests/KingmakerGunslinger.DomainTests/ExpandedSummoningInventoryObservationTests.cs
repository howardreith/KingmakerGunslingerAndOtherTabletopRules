using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningInventoryObservationTests
    {
        internal static void QualifiedReferencesAreExactManifestOwnedRows()
        {
            var rows = ExpandedSummoningInventoryObservationPolicy.QualifiedReferenceRows;
            // Seventeen through Sprint 17, and twenty-four more for Sprints 18
            // through 21: three for the apes, four for the Girallon and Xill,
            // six for the Giant Scorpion and eleven for this sprint's Giant
            // Crab and overhauled Bebelith, two exact Sprint 22 Ankylosaurus
            // rows, then five exact Sprint 23 rows: Triceratops chassis,
            // charge and trample plus the Stegosaurus chassis/trip facts.
            // The table went four sprints stale
            // because the only thing that reads it is the inventory observer,
            // and the only thing that runs the inventory observer is the
            // compatibility matrix - a tranche-closure gate.
            //
            // The count is the cheap half of this test. The per-row checks
            // below are the real one: every GUID and every internal name must
            // resolve to exactly one ledger symbol, so a row cannot qualify a
            // reference that does not exist or name one thing while pointing
            // at another.
            // Sprint 24 publication adds five more exact rows: the
            // Deinonychus and Tyrannosaurus chassis facts plus the
            // Tyrannosaurus Powerful Bite, Bleeding Critical and combat
            // traits carriers, all granted on their own units.
            int giantRows = rows.Count(r => r[1] == "KMG_Summoning_Unit_Brachiosaurus" || r[1] == "KMG_Summoning_Unit_Elephant");
            Assertions.Equal(6, giantRows, "Only the four Brachiosaurus carriers and Elephant's exact Trample/chassis are admitted.");
            Assertions.Equal(53, rows.Length - giantRows, "The released interpretation prefix through Sprint 24 is preserved.");
            Assertions.Equal(rows.Length, rows.Select(r => string.Join("|", r)).Distinct().Count(), "No duplicate ownership rows.");
            var entries = (JArray)JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"];
            foreach (var r in rows)
            {
                Assertions.True(ExpandedSummoningInventoryObservationPolicy.QualifiedProjectReference(r[0],r[1],r[2],r[3],r[4]), "Exact qualified pair/carrier accepted.");
                foreach (int offset in new[] { 0, 2 })
                    Assertions.Equal(1, entries.Count(e => (string)e["guid"] == r[offset] &&
                        ((string)e["symbol"]).Replace('.', '_') == r[offset+1]), "Stable manifest GUID and exact internal name.");
            }
            rows[0][2] = "foreign";
            Assertions.False(ExpandedSummoningInventoryObservationPolicy.QualifiedReferenceRows[0][2] == "foreign", "Caller cannot rewrite authority.");
            Assertions.True(ExpandedSummoningInventoryObservationPolicy.NaturalProfileFactComparable(
                "KMG_Summoning_Special_Ankylosaurus_Stun", true),
                "A profile-declared special fact remains in exact chassis comparison.");
            Assertions.False(ExpandedSummoningInventoryObservationPolicy.NaturalProfileFactComparable(
                "KMG_Summoning_Special_Foreign", false),
                "An unrelated special stays outside the chassis comparison for its dedicated observer.");
            Assertions.True(ExpandedSummoningInventoryObservationPolicy.NaturalProfileFactComparable(
                "KMG_Summoning_Natural_Ankylosaurus_Chassis", false),
                "Ordinary natural facts remain comparable even when not expected.");
        }

        internal static void QualifiedReferencesRejectSpoofingAndOtherCarriers()
        {
            foreach (var r in ExpandedSummoningInventoryObservationPolicy.QualifiedReferenceRows)
                for (int i=0;i<r.Length;i++) foreach (string bad in new[] { null, "", "foreign", r[i].ToUpperInvariant() })
                {
                    var wrong=(string[])r.Clone();wrong[i]=bad;
                    Assertions.False(ExpandedSummoningInventoryObservationPolicy.QualifiedProjectReference(wrong[0],wrong[1],wrong[2],wrong[3],wrong[4]), "Wrong owner/GUID/name/carrier never bypasses prohibited-reference census.");
                }
            Assertions.False(ExpandedSummoningInventoryObservationPolicy.QualifiedProjectReference("unit","KMG_Summoning_Unit_FireBeetle","fact","KMG_Summoning_Natural_FireBeetle_Teleport","AddFacts"), "Project-looking names cannot hide hazardous behavior.");
        }

        internal static void BebelithBiteRequiresItsIdentityAndObservedDice()
        {
            const string bite = "d2f99947db522e24293a7ec4eded453f";
            Func<string, int, int, bool> check = ExpandedSummoningInventoryObservationPolicy.BebelithPrimaryBite;
            Assertions.True(check(bite, 2, 6), "The actual primary bite must carry its native identity and 2d6 base damage.");
            foreach (string wrong in new[] { null, "", "foreign", bite.ToUpperInvariant() })
                Assertions.False(check(wrong, 2, 6), "Correct dice cannot qualify a different primary weapon.");
            Assertions.False(check(bite, 1, 6), "The correct GUID alone cannot prove the number of dice.");
            Assertions.False(check(bite, 2, 4), "The correct GUID alone cannot prove die size.");
        }

        internal static void SalamanderCarriersDoNotExpectLegacyOnHitRiders()
        {
            Func<bool,int,bool,int,int,bool,int,bool,int,bool,int,int,bool> check = ExpandedSummoningInventoryObservationPolicy.SalamanderCarriers;
            Assertions.True(check(false,1,true,2,6,true,1,true,1,true,0,0), "Manufactured spear, native secondary tail and exact owned live heat/grab carriers.");
            Assertions.False(check(true,1,true,2,6,true,1,true,1,true,0,0), "Legacy natural-spear expectation is not the qualified profile.");
            Assertions.False(check(false,2,true,2,6,true,1,true,1,true,0,0), "No extra tail.");
            Assertions.False(check(false,1,false,2,6,true,1,true,1,true,0,0), "Tail remains natural.");
            Assertions.False(check(false,1,true,3,6,true,1,true,1,true,0,0), "Exact base tail dice.");
            Assertions.False(check(false,1,true,2,6,false,1,true,1,true,0,0), "Racial owner reference required.");
            Assertions.False(check(false,1,true,2,6,true,2,true,1,true,0,0), "No duplicate heat carrier.");
            Assertions.False(check(false,1,true,2,6,true,1,false,1,true,0,0), "Heat belongs to exact owner/weapons.");
            Assertions.False(check(false,1,true,2,6,true,1,true,1,false,0,0), "Grab is exact owner and tail only.");
            Assertions.False(check(false,1,true,2,6,true,1,true,1,true,2,0), "Legacy attack-trigger replay is forbidden.");
            Assertions.False(check(false,1,true,2,6,true,1,true,1,true,0,2), "Legacy damage action replay is forbidden.");
        }
    }
}
