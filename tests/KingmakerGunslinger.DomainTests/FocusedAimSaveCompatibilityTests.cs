namespace KingmakerGunslinger.DomainTests
{
    internal static partial class Program
    {
        private static void MysteriousStrangerFocusedAimSaveCompatibility()
        {
            string source = ThirdPlaytestSource(
                "src/KingmakerGunslinger/Blueprints/MysteriousStrangerBlueprints.cs");
            Assertions.True(source.Contains(
                "b.FxOnStart=new Kingmaker.ResourceLinks.PrefabLink();"),
                "Saved deed markers must publish an explicit empty start-FX link.");
            Assertions.True(source.Contains(
                "b.FxOnRemove=new Kingmaker.ResourceLinks.PrefabLink();"),
                "Saved deed markers must publish an explicit empty remove-FX link.");
            Assertions.True(source.Contains(
                "b.ResourceAssetIds=Array.Empty<string>();"),
                "Saved deed markers must publish an empty resource dependency set.");
        }
    }
}
