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
            Assertions.Equal(17, rows.Length, "All observed qualified Phase2B references, no blanket prefix exception.");
            Assertions.Equal(17, rows.Select(r => string.Join("|", r)).Distinct().Count(), "No duplicate ownership rows.");
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
