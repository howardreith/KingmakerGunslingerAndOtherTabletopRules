using System.IO;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationWorldMapTests
    {
        internal static void PrebuiltSettlementDoesNotEstablishCapital()
        {
            foreach (bool owned in new[] { false, true })
            {
                var value = WordOfRecallCapitalState.Resolve(false, owned, WordOfRecallDestinationPolicy.CapitalId);
                Assertions.True(value.Known && !value.Established, "Unclaimed capital region remains before establishment even with a prebuilt settlement.");
                Assertions.Equal(WordOfRecallDestinationPolicy.OlegId, value.DestinationId, "Recall uses Oleg before establishment.");
            }
        }
        internal static void EstablishedCapitalRequiresExactOwnedAnchor()
        {
            var value = WordOfRecallCapitalState.Resolve(true, true, WordOfRecallDestinationPolicy.CapitalId);
            Assertions.True(value.Known && value.Established, "Claimed native capital region establishes its capital.");
            Assertions.Equal(WordOfRecallDestinationPolicy.CapitalId, value.DestinationId, "Exact native capital identity.");
            foreach (string wrong in new[] { null, "Capital", WordOfRecallDestinationPolicy.OlegId })
            {
                value = WordOfRecallCapitalState.Resolve(true, true, wrong);
                Assertions.True(value.Established && value.DestinationId == null, "Invalid capital never falls back to Oleg or a display name.");
            }
            Assertions.True(WordOfRecallCapitalState.Resolve(true, false, WordOfRecallDestinationPolicy.CapitalId).DestinationId == null,
                "A settlement belonging to another region cannot resolve Recall.");
        }
        internal static void UnknownCapitalDoesNotGuess()
        {
            Assertions.False(WordOfRecallCapitalState.Unknown.Known, "Missing or ambiguous native capital state is explicitly unknown.");
            Assertions.True(WordOfRecallCapitalState.Unknown.DestinationId == null, "No guessed before-capital fallback.");
        }
        internal static void DestinationReadsCannotCreateCampaignState()
        {
            string source = File.ReadAllText("src/KingmakerGunslinger/Spells/Teleportation/TeleportationWorldMapAdapter.cs");
            foreach (string mutation in new[] { ".GetLocationData(", ".Ensure<", ".EnsureLedger(", ".MigrateLegacy(",
                ".RecordOrdinaryArrival(", ".CalculatePathToLocation(", ".SetCurrentPosition(", ".Reveal(", ".Spend(" })
                Assertions.False(source.Contains(mutation), "Destination composition has no native campaign mutation: " + mutation);
            Assertions.True(source.Contains("context.Map.Locations.TryGetValue(blueprint, out data)") &&
                source.Contains("GetComponents<LocationRestriction>()"), "Read existing records and native restrictions.");
        }
        internal static void NativeContextRequiresCurrentWorldMapAndParty()
        {
            string source = File.ReadAllText("src/KingmakerGunslinger/Spells/Teleportation/TeleportationWorldMapAdapter.cs");
            foreach (string required in new[] { "GameModeType.GlobalMap", "GetStaticScene().SceneName", "IsLoadingInProcess", "IsLoadingScreenActive",
                "game.DialogController.Dialog", "game.Player.Dialog.Scheduled", "game.State.Cutscenes", "GameModeType.Kingdom",
                "result.Map.CurrentEncounterData", "result.Map.TravelData", "position.Edge != null", "result.Rules.Pawn.Position",
                "value.Blueprint.AssetGuid == blueprint.AssetGuid", "TeleportCastBlock.RelocationPending" })
                Assertions.True(source.Contains(required), "Native context binds required current-state gate: " + required);
        }
    }
}
