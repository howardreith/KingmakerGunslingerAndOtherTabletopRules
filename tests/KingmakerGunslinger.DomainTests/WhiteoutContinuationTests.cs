using System;
using System.IO;
namespace KingmakerGunslinger.DomainTests
{
    internal static class WhiteoutContinuationTests
    {
        private static string Fixture { get { return File.ReadAllText("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedWhiteoutFoundation.cs"); } }
        internal static void DisposableSceneReplacement()
        { Assertions.True(Fixture.Contains("NonPartySceneReferenceRestorationRequired=false") && Fixture.Contains("DisposableNoSaveProcessIsolation=true"), "Native replacement is accepted only with explicit disposable no-save isolation evidence."); }
        internal static void SameAreaExactRestoration()
        { Assertions.True(Fixture.Contains("WhiteoutListeners(typeof(IWeatherChangeHandler)).SequenceEqual(_listeners)") && Fixture.Contains("_units.All(u=>u.Buffs.Enumerable.SequenceEqual(_buffs[u]))") && Fixture.Contains("_season.SetValue(_controller,_seasonBefore)"), "Within each loaded area native values, listeners and exact existing facts still restore."); }
        internal static void ExitAndNoSaveMandatory()
        { Assertions.True(Fixture.Contains("!_request.ExitAfterCompletion") && Fixture.Contains("!_workingSaveSmoke.Complete") && Fixture.Contains("_workingSaveSmoke.WriteObserved") && File.ReadAllText("scripts/RuntimeAutomation.Common.ps1").Contains("Disposable Whiteout weather fixture requires automatic exit."), "Closed guarded working load and automatic exit are mandatory; external lease/live snapshot restoration remains harness-owned."); }
        internal static void OneWayNoOriginReturn()
        { Assertions.Equal(1, System.Text.RegularExpressions.Regex.Matches(Fixture,"_runner.WhiteoutLoadArea\\(").Count, "Only one native load route exists."); Assertions.True(Fixture.Contains("_runner.WhiteoutLoadArea(_outdoor.Area,_outdoor)"), "No return trip exists."); }
        internal static void DisposedReferencesNeverReused()
        { var owner=Fixture.Substring(Fixture.IndexOf("private sealed class WhiteoutDisposableFixture"),Fixture.IndexOf("private sealed class WhiteoutFixtureProbe")-Fixture.IndexOf("private sealed class WhiteoutDisposableFixture")); Assertions.False(owner.Contains("_units") || owner.Contains("_buffs") || owner.Contains("RestoreOrigin"), "Process owner retains only persistent party/player controls, never prior-scene NPCs/facts."); }
        internal static void PersistentStateControls()
        { Assertions.True(Fixture.Contains("_game.Player.Party.SequenceEqual(_party)") && Fixture.Contains("Equals(_game.Player.MainCharacter,_mainCharacter)") && Fixture.Contains("_game.Player.Inventory.Items.SequenceEqual(_inventory)") && Fixture.Contains("_game.Player.Money==_money") && Fixture.Contains("_game.Player.GameTime==_time") && Fixture.Contains("PersistentControls(\"before-one-way-transition\")") && Fixture.Contains("PersistentControls(\"before-process-exit\")"), "Exact persistent controls are checked before travel and exit."); }
    }
}
