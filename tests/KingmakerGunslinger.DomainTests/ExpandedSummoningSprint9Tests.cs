using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint9Tests
    {
        internal static void DireBatSenseHasASeparateBoundedIdentity()
        {
            var bat = ExpandedSummoningNaturalProfiles.For("dire-bat");
            Assertions.True(bat.Facts.Contains("DireBatBlindsense"),
                "Dire Bat needs its own imprecise blindsense fact.");
            Assertions.Equal(1, ExpandedSummoningIdentityCatalog.Build().Count(value =>
                value.Symbol == "KMG.Summoning.Natural.DireBat.Blindsense" &&
                value.PlannedType == "BlueprintFeature"),
                "The sense needs one append-only feature identity.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningNaturalBuilder.cs"));
            Assertions.True(builder.Contains("nativeSense.Blindsight = false") &&
                builder.Contains("nativeSense.Range = new Feet(40)"),
                "Dire Bat senses creatures within 40 feet without precise blindsight.");
            string runner = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            Assertions.True(runner.Contains("expanded-summoning-dire-bat-blindsense") &&
                runner.Contains("part.Reach(caster)"),
                "The guarded scenario checks the live sense part and donor-sharing birds.");
        }
    }
}
