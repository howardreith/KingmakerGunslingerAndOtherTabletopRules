using System;
using System.IO;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint10Tests
    {
        internal static void NativeFlyingVerminSurveyStaysMetadataOnly()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            Assertions.True(source.Contains("\"wasp\", \"stirge\"") &&
                source.Contains("\"mosquito\"") &&
                source.Contains("\"beetle\"") &&
                source.Contains("\"mantis\"") &&
                source.Contains("56ec8788092b6314e8f3c1c502e8433f") &&
                source.Contains("\"blood\"") &&
                source.Contains("\"attach\"") &&
                source.Contains("\"drain\"") &&
                source.Contains("GetAllBlueprints().Where(value => value != null)") &&
                source.Contains("units.Add(DescribeNativeUnit(unit))") &&
                source.Contains("native-donor-audit.json") &&
                !source.Contains("AssetBundle.LoadFromFile") &&
                !source.Contains("Texture2D.EncodeToPNG"),
                "Sprint 10 intake must inventory installed native metadata for both flying vermin and their signature seams without exporting game art.");
        }
    }
}
