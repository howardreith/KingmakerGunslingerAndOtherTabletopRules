using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    // Desktop Elementals chooser admission, lifetime and keyboard recovery.
    // Every decision comes from the production ElementalChooserGate,
    // ElementalKeyboardLease, ElementalSourceSelection and StageLauncherRows.
    // Desk only replays the call order of the Unity glue that cannot compile
    // here: ElementalSourceChooserRuntime.Begin/End (fill), launcher OnClick ->
    // ElementalSourceChooserView.Open (click), View.Update (frame) and
    // View.Close (cancel/close). Frame calls the production lazy KeepOpen and
    // Close calls the production CloseNoSpend, as the view does. Desk does
    // NOT run ElementalSourceChooserRuntime.Dispatch: it records the
    // selection's handoff of a Row only. It neither builds the native
    // converted-spell slot, nor reproduces production's release-before-
    // native-handoff order, nor performs any spell expenditure; these tests
    // prove no native preparation spend. Guard mirrors the decompiled native
    // Kingmaker.Utility.CountingGuard used by Game.Keyboard.Disabled
    // (canGoNegative=false; Value=true/false adds/removes one count).
    internal static class Cleanup2ElementalChooserLifecycleTests
    {
        private sealed class Guard
        {
            internal int Count;
            internal int Writes;
            internal bool SwallowRelease;
            internal void Acquire() { Writes++; Count++; }
            internal void Release() { Writes++; if (!SwallowRelease && Count > 0) Count--; }
            internal void Reset() { Count = 0; }
        }

        // Stand-ins for the live reads View.Update makes. Reading a destroyed
        // owner's gameObject throws, as Unity's MissingReferenceException does.
        private sealed class GameState { internal bool Gamepad = false; internal string Mode = "Default"; }
        private sealed class Owner
        {
            internal bool Destroyed, Active = true;
            internal int Reads;
            internal bool GameObjectActive
            { get { Reads++; if (Destroyed) throw new InvalidOperationException("Destroyed action-bar owner was dereferenced."); return Active; } }
        }

        private sealed class Keyboard
        {
            internal readonly Guard Disabled = new Guard();
            internal ElementalKeyboardContext Live()
            {
                var guard = Disabled;
                return new ElementalKeyboardContext(this, guard, () => guard.Count, guard.Acquire, guard.Release);
            }
        }

        private class Row
        {
            internal readonly string Guid, Name;
            internal Row(string guid, string name) { Guid = guid; Name = name; }
            public override string ToString() { return Name; }
        }
        private sealed class Launcher : Row { internal Launcher() : base(null, "Elementals") { } }

        private const string MediumElemental = "e42b1dbff4262c6469a9ff0a6ce730e3";
        private static readonly string SummonMonsterIV = ElementalSourceChooserPolicy.Rank(4).MonsterParentGuid;

        // Every native GameModeType name (decompiled KeyboardAccess
        // GameModesGroup.All, KM 2.1.7b MVID 07fa1e4d).
        private static readonly string[] NativeModes = { "None", "Default", "Dialog", "Pause", "FullScreenUi", "GlobalMap",
            "Cutscene", "GameOver", "BugReport", "Rest", "EscMode", "Kingdom", "KingdomSettlement", "CutsceneGlobalMap" };

        private sealed class Desk
        {
            internal Keyboard Keyboard = new Keyboard();
            internal GameState Game = new GameState();
            internal Owner Owner = new Owner();
            internal readonly ElementalKeyboardLease Lease = new ElementalKeyboardLease();
            internal readonly Row Legacy = new Row(MediumElemental, "Summon Medium Elemental");
            internal readonly Row SummonMonster = new Row(SummonMonsterIV, "Summon Monster IV");
            internal Row[] NativeRow;
            internal Row[] Shown;
            internal ElementalSourceSelection<Row> View;
            internal bool SessionCurrent;
            internal string NativeRowsRequest;
            internal string LastOutcome;
            internal readonly List<Row> Dispatched = new List<Row>();
            internal int Warnings;

            internal Desk(bool legacyPrepared)
            {
                NativeRow = legacyPrepared ? new[] { new Row("a", "Spell A"), SummonMonster, Legacy } :
                    new[] { new Row("a", "Spell A"), new Row("b", "Spell B"), new Row("c", "Spell C"), SummonMonster };
            }

            // Begin -> native FillSlots -> End.
            internal string Fill(string mode)
            {
                Close("native-tab-rebuilt");
                string request = NativeRowsRequest; NativeRowsRequest = null;
                string kept = ElementalChooserGate.Consolidation(true, false, mode, true, request, () => Lease.Admit(Keyboard.Live()));
                SessionCurrent = kept == null;
                if (kept != null) { Shown = NativeRow.ToArray(); return kept; }
                var consolidated = NativeRow.Where(value => ElementalSourceChooserPolicy.Parent(value.Guid) != null).ToList();
                var staged = ElementalSourceChooserPolicy.StageLauncherRows<Row>(new List<Row[]> { NativeRow }, 0, consolidated, () => new Launcher());
                Shown = staged == null ? NativeRow.ToArray() : staged[0];
                return null;
            }

            internal bool LauncherShown { get { return Shown.OfType<Launcher>().Any(); } }

            // Launcher OnClick -> View.Open.
            internal ElementalLauncherRoute Click(string mode)
            {
                Close("reopen");
                string reason;
                var route = ElementalChooserGate.Launcher(SessionCurrent, false, mode, true,
                    SessionCurrent ? Lease.Admit(Keyboard.Live()) : null, out reason);
                LastOutcome = reason;
                if (route == ElementalLauncherRoute.NativeRows) { NativeRowsRequest = reason; Warnings++; }
                if (route != ElementalLauncherRoute.Chooser) return route;
                if (!Lease.Acquire(Keyboard.Live())) throw new InvalidOperationException("Admitted keyboard was not acquired: " + Lease.Outcome);
                View = new ElementalSourceSelection<Row>(new[] { Legacy, SummonMonster }, () => SessionCurrent,
                    value => null, value => Dispatched.Add(value));
                return route;
            }

            // LiveKeyboard(): no context without a game.
            internal ElementalKeyboardContext Live() { return Game == null ? null : Keyboard.Live(); }

            internal void Frame(string mode) { if (Game != null) Game.Mode = mode; Frame(); }

            // View.Update. The reads are the b7029441cf call site's unguarded
            // expressions (game.Gamepad, owner.gameObject), so only the gate's
            // own short-circuit order keeps them from being evaluated.
            internal void Frame()
            {
                if (View == null) return;
                var game = Game; var owner = Owner; var view = View;
                if (!ElementalChooserGate.KeepOpen(
                    () => game != null && view != null && SessionCurrent && !owner.Destroyed,
                    () => game.Gamepad,
                    () => game.Mode,
                    () => owner.GameObjectActive))
                    Close("source-context-changed-no-spend");
            }

            // View.Close (cancel, Esc, context change, reopen, rebuild).
            internal bool Close(string reason)
            {
                if (View == null) return true;
                var view = View; View = null;
                LastOutcome = reason;
                return ElementalChooserGate.CloseNoSpend(view, null, Lease, Live);
            }

            // Selection handoff only (see the class comment): production
            // Dispatch's native slot, release-before-handoff and expenditure
            // are not exercised.
            internal bool Cast()
            {
                var view = View;
                return view != null && view.Cast();
            }
        }

        internal static void NoOriginalTileKeepsNativeRowsInEveryWorldMode()
        {
            foreach (string mode in ElementalChooserGate.WorldModes)
            {
                var desk = new Desk(false);
                var native = desk.NativeRow.ToArray();
                Assertions.True(desk.Fill(mode) == null, mode + ": the fill is admitted.");
                Assertions.False(desk.LauncherShown, mode + ": with no original tile there is no launcher.");
                Assertions.True(desk.Shown.SequenceEqual(native) && desk.NativeRow.SequenceEqual(native),
                    mode + ": native rows (including Summon Monster IV) are unchanged.");
                Assertions.Equal(0, desk.Keyboard.Disabled.Writes, mode + ": filling never writes the keyboard.");
            }
        }

        internal static void RetainedOriginalOpensWhilePausedAndUnpaused()
        {
            Assertions.True(NativeModes.Where(ElementalChooserGate.WorldMode).SequenceEqual(new[] { "Default", "Pause" }),
                "Exactly the native World modes admit the chooser.");
            foreach (string mode in new[] { "Default", "Pause" })
            {
                var desk = new Desk(true);
                Assertions.True(desk.Fill(mode) == null, mode + ": a legacy prepared original is consolidated.");
                Assertions.Equal("Spell A, Summon Monster IV, Elementals", string.Join(", ", desk.Shown.Select(value => value.ToString())),
                    mode + ": the launcher stands in the original tile's place.");
                Assertions.Equal(ElementalLauncherRoute.Chooser, desk.Click(mode), mode + ": the launcher opens the chooser.");
                Assertions.True(desk.Lease.Held && desk.Keyboard.Disabled.Count == 1, mode + ": the chooser holds exactly one keyboard count.");
                Assertions.True(desk.View.Select(desk.Legacy) && desk.Cast(), mode + ": the original source can be cast.");
                Assertions.True(desk.Dispatched.Count == 1 && ReferenceEquals(desk.Dispatched[0], desk.Legacy),
                    mode + ": the selection hands off exactly once, with the selected original source (no native spend is exercised).");
                Assertions.False(desk.Cast(), mode + ": a repeated Cast cannot hand off twice.");
                Assertions.True(desk.Close("cast-native-source") && desk.Keyboard.Disabled.Count == 0 && desk.Lease.Restored,
                    mode + ": the view releases exactly its count.");
            }
            foreach (string mode in NativeModes.Where(value => !ElementalChooserGate.WorldMode(value)))
            {
                int probes = 0;
                Assertions.True(ElementalChooserGate.Consolidation(true, false, mode, true, null, () => { probes++; return null; }) != null,
                    mode + ": no launcher is built outside Default/Pause.");
                Assertions.Equal(0, probes, mode + ": the keyboard is not probed for a fill that cannot consolidate.");
                string reason;
                Assertions.Equal(ElementalLauncherRoute.NativeRows, ElementalChooserGate.Launcher(true, false, mode, true, null, out reason),
                    mode + ": a launcher clicked in an unsupported mode restores native rows instead of doing nothing.");
                string current = mode;
                Assertions.False(ElementalChooserGate.KeepOpen(() => true, () => false, () => current, () => true), mode + ": an open view closes.");
            }
        }

        internal static void PauseTransitionsKeepOneViewAndReopenWithoutStaleUi()
        {
            var desk = new Desk(true);
            desk.Fill("Default");
            desk.Click("Default");
            var first = desk.View;
            desk.Frame("Pause"); desk.Frame("Default"); desk.Frame("Pause");
            Assertions.True(ReferenceEquals(desk.View, first) && !first.Closed && desk.Keyboard.Disabled.Count == 1,
                "Default/Pause transitions keep the same view, session and single keyboard count.");
            desk.Frame("Dialog");
            Assertions.True(desk.View == null && first.Closed && desk.Keyboard.Disabled.Count == 0 && desk.Lease.Restored,
                "Leaving the World modes closes the view and releases its count.");
            Assertions.Equal(ElementalLauncherRoute.Chooser, desk.Click("Pause"), "The same launcher reopens while paused.");
            var second = desk.View;
            Assertions.Equal(ElementalLauncherRoute.Chooser, desk.Click("Pause"), "A second click replaces, never stacks, the view.");
            Assertions.True(second.Closed && !ReferenceEquals(desk.View, second) && desk.Keyboard.Disabled.Count == 1,
                "Reopen closes the previous view first; one view and one count remain.");
            desk.Fill("Pause");
            Assertions.True(desk.View == null && desk.Keyboard.Disabled.Count == 0 && desk.LauncherShown,
                "A native refill closes the view and stages a fresh launcher.");
            desk.SessionCurrent = false;
            Assertions.Equal(ElementalLauncherRoute.Stale, desk.Click("Pause"), "A launcher from a superseded tab does nothing.");
            Assertions.Equal(0, desk.Keyboard.Disabled.Count, "A stale launcher takes no keyboard count.");
            Assertions.Equal(0, desk.Dispatched.Count, "No transition or reopen spends anything.");
        }

        internal static void CancellationSpendsNothingAndReleasesExactlyItsCount()
        {
            foreach (string mode in ElementalChooserGate.WorldModes)
            {
                var desk = new Desk(true);
                desk.Fill(mode);
                desk.Click(mode);
                var view = desk.View;
                Assertions.True(view.Select(desk.Legacy), mode + ": a source is selected.");
                Assertions.True(desk.Close("cancelled-no-spend"), mode + ": cancel verifies its release.");
                Assertions.True(view.Closed && view.Selected == null && view.Outcome == "cancelled-no-spend",
                    mode + ": the selection is cancelled.");
                Assertions.False(view.Cast(), mode + ": a cancelled view cannot cast.");
                Assertions.Equal(0, desk.Dispatched.Count, mode + ": cancellation spends nothing.");
                Assertions.True(desk.Keyboard.Disabled.Count == 0 && desk.Lease.Restored, mode + ": exactly the chooser's count is released.");
            }
        }

        internal static void FailedRestorationRecoversOnlyOnAProvablySafeContext()
        {
            // The reported lockout: a release fails, the view is destroyed,
            // and every later Open refused before reaching a fresh context.
            var desk = new Desk(true);
            desk.Fill("Default"); desk.Click("Default");
            desk.Keyboard.Disabled.Reset();
            Assertions.False(desk.Close("chooser-destroyed-no-spend"), "A cleared guard leaves the release unverified.");
            Assertions.True(desk.Lease.Unresolved && !desk.Lease.Restored, "The unresolved release is recorded with its context.");
            int writes = desk.Keyboard.Disabled.Writes;
            Assertions.Equal(ElementalLauncherRoute.Chooser, desk.Click("Pause"),
                "The same context with no count anywhere is provably free: the chooser opens again.");
            Assertions.True(desk.Keyboard.Disabled.Count == 1 && desk.Keyboard.Disabled.Writes == writes + 1,
                "Recovery acquires one fresh count and writes nothing else.");
            desk.Close("cancelled-no-spend");

            // Replaced keyboard context: the old guard is never touched.
            desk.Click("Default");
            var old = desk.Keyboard;
            int oldWrites = old.Disabled.Writes;
            desk.Keyboard = new Keyboard();
            Assertions.False(desk.Close("chooser-destroyed-no-spend"), "A replaced context leaves the release unresolved.");
            Assertions.True(old.Disabled.Count == 1 && old.Disabled.Writes == oldWrites, "No write to a guard that no longer gates input.");
            Assertions.Equal(ElementalLauncherRoute.Chooser, desk.Click("Default"), "A fresh, free keyboard context admits the chooser.");
            Assertions.True(desk.Keyboard.Disabled.Count == 1 && old.Disabled.Count == 1 && old.Disabled.Writes == oldWrites,
                "One count on the new guard; the retired guard is never written.");
            desk.Close("cancelled-no-spend");

            // Same context, release issued but not observed, count still present:
            // it may be the chooser's own count, so the lease refuses and writes nothing.
            desk.Click("Default");
            desk.Keyboard.Disabled.SwallowRelease = true;
            Assertions.False(desk.Close("cancelled-no-spend"), "An unverified release is reported.");
            desk.Keyboard.Disabled.SwallowRelease = false;
            writes = desk.Keyboard.Disabled.Writes;
            Assertions.Equal(ElementalLauncherRoute.NativeRows, desk.Click("Default"),
                "An unresolved release on the same context with a count present refuses the chooser.");
            Assertions.True(desk.LastOutcome.Contains("unresolved-release-on-same-keyboard-context"), "The refusal names the unresolved release.");
            Assertions.True(desk.Keyboard.Disabled.Count == 1 && desk.Keyboard.Disabled.Writes == writes,
                "No count is guessed to be the chooser's and released.");
            desk.Fill("Default");
            string kept = desk.Fill("Default");
            Assertions.True(kept != null && kept.Contains("unresolved-release-on-same-keyboard-context") &&
                !desk.LauncherShown && desk.Shown.Contains(desk.Legacy),
                "Meanwhile later fills keep the original spell in its native tile: " + kept);
            Assertions.Equal(writes, desk.Keyboard.Disabled.Writes, "Fills only read the guard.");
        }

        internal static void ForeignKeyboardOwnerIsNeverStolenOrCleared()
        {
            var desk = new Desk(true);
            desk.Keyboard.Disabled.Acquire();
            int writes = desk.Keyboard.Disabled.Writes;
            string kept = desk.Fill("Pause");
            Assertions.True(kept != null && kept.Contains("keyboard-owned-by-another-ui"), "A fill while another UI owns input keeps native rows.");
            Assertions.True(!desk.LauncherShown && desk.Shown.Contains(desk.Legacy), "The original spell keeps its native tile.");
            desk.Keyboard.Disabled.Release(); writes++;
            desk.Fill("Pause");
            desk.Keyboard.Disabled.Acquire(); writes++;
            Assertions.Equal(ElementalLauncherRoute.NativeRows, desk.Click("Pause"),
                "Another UI taking input after the fill sends the click to native rows.");
            Assertions.True(desk.Keyboard.Disabled.Count == 1 && desk.Keyboard.Disabled.Writes == writes && !desk.Lease.Held,
                "The foreign count is neither stolen, cleared nor decremented.");
            Assertions.True(desk.Lease.Release(desk.Keyboard.Live()) && desk.Keyboard.Disabled.Writes == writes,
                "Releasing with nothing held writes nothing.");
            desk.Keyboard.Disabled.Release(); writes++;

            desk.Fill("Default");
            Assertions.True(desk.Fill("Default") == null, "Once input is free again, a later fill consolidates.");
            Assertions.Equal(ElementalLauncherRoute.Chooser, desk.Click("Default"), "The chooser opens on free input.");
            desk.Keyboard.Disabled.Acquire();
            Assertions.True(desk.Close("cancelled-no-spend") && desk.Keyboard.Disabled.Count == 1,
                "Closing releases only the chooser's count; the later foreign count stays.");
            Assertions.False(new ElementalKeyboardLease().Acquire(desk.Keyboard.Live()), "Acquisition refuses while any count is present.");
            Assertions.Equal(1, desk.Keyboard.Disabled.Count, "The refused acquisition wrote nothing.");
        }

        internal static void RefusedLauncherRestoresTheOriginalNativeTileOnce()
        {
            var desk = new Desk(true);
            desk.Fill("Default");
            desk.Keyboard.Disabled.Acquire();
            Assertions.Equal(ElementalLauncherRoute.NativeRows, desk.Click("Default"), "A refused launcher requests native rows.");
            Assertions.Equal(1, desk.Warnings, "The player is told why the row changed.");
            desk.Keyboard.Disabled.Release();
            string kept = desk.Fill("Default");
            Assertions.True(kept != null && kept.StartsWith("native-rows-requested:"), "The next fill keeps native rows: " + kept);
            Assertions.True(desk.Shown.SequenceEqual(desk.NativeRow) && ReferenceEquals(desk.Shown[2], desk.Legacy),
                "The exact original tile object returns in place; casting it is then entirely native (not exercised here).");
            Assertions.Equal(0, desk.Dispatched.Count, "The fallback dispatches nothing itself.");
            Assertions.True(desk.Fill("Default") == null && desk.LauncherShown, "The request is consumed once; a later fill consolidates again.");
        }

        internal static void DestroyedOwnerOrMissingGameClosesWithoutDereference()
        {
            foreach (string mode in ElementalChooserGate.WorldModes)
            {
                // The tab owner is destroyed while the root-canvas overlay is
                // open, after another UI also took a keyboard count.
                var desk = new Desk(true);
                desk.Game.Mode = mode;
                desk.Fill(mode); desk.Click(mode);
                var view = desk.View;
                Assertions.True(view.Select(desk.Legacy), mode + ": a source is selected.");
                desk.Keyboard.Disabled.Acquire();
                desk.Owner.Destroyed = true; desk.SessionCurrent = false;
                int reads = desk.Owner.Reads;
                desk.Frame();
                Assertions.Equal(reads, desk.Owner.Reads, mode + ": the destroyed owner is not dereferenced.");
                Assertions.True(desk.View == null && view.Closed && view.Selected == null && view.Outcome == "cancelled-no-spend",
                    mode + ": the view closes and cancels its selection.");
                Assertions.Equal(0, desk.Dispatched.Count, mode + ": nothing is handed off.");
                Assertions.True(desk.Keyboard.Disabled.Count == 1 && desk.Lease.Restored,
                    mode + ": exactly the chooser's count is released; the other UI's count stays.");
                desk.Frame();
                Assertions.Equal(1, desk.Keyboard.Disabled.Count, mode + ": a later frame writes nothing.");
            }

            // An inactive (not destroyed) owner is read, then closes.
            var hidden = new Desk(true);
            hidden.Fill("Pause"); hidden.Click("Pause");
            hidden.Owner.Active = false;
            hidden.Frame();
            Assertions.True(hidden.View == null && hidden.Keyboard.Disabled.Count == 0 && hidden.Lease.Restored,
                "A hidden owner closes the view and releases its count.");

            // No game: nothing is read and the view still closes. Without a
            // live keyboard context the release cannot be verified, so it is
            // recorded as unresolved and the guard is not written.
            var gone = new Desk(true);
            gone.Fill("Default"); gone.Click("Default");
            var orphan = gone.View;
            int writes = gone.Keyboard.Disabled.Writes;
            gone.Game = null;
            gone.Frame();
            Assertions.True(gone.View == null && orphan.Closed && orphan.Outcome == "cancelled-no-spend" && gone.Dispatched.Count == 0,
                "Without a game the view closes and cancels without a handoff.");
            Assertions.True(gone.Lease.Unresolved && gone.Keyboard.Disabled.Writes == writes,
                "The unverifiable release is recorded, not guessed.");

            // A valid paused context keeps the view (shared Default/Pause policy).
            var valid = new Desk(true);
            valid.Fill("Pause"); valid.Click("Pause");
            var kept = valid.View;
            valid.Frame("Pause"); valid.Frame("Default"); valid.Frame("Pause");
            Assertions.True(ReferenceEquals(valid.View, kept) && valid.Keyboard.Disabled.Count == 1,
                "A valid owner keeps the view across Default/Pause.");
        }
    }
}
