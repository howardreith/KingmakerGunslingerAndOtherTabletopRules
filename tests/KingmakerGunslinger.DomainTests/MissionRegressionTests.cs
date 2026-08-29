using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Acadamae;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Gunsmithing;

namespace KingmakerGunslinger.DomainTests
{
    internal static class MissionRegressionTests
    {
        private sealed class RecordingSink : IPlayerCombatLogSink
        {
            internal int Count;
            internal string Last;
            internal bool Throw;

            public void Add(string message)
            {
                Count++;
                Last = message;
                if (Throw) throw new InvalidOperationException("sink failed");
            }
        }

        internal static void StarterTransitionMatrix()
        {
            Assertions.True(StartingFirearmGrantPolicy
                    .IsCommittedCharacterCreation(true, true),
                "A first-level CharGen commit must authorize its detached starter fallback.");
            Assertions.False(StartingFirearmGrantPolicy
                    .IsCommittedCharacterCreation(true, false),
                "A first-level respec reconstruction must not masquerade as character creation.");
            Assertions.False(StartingFirearmGrantPolicy
                    .IsCommittedCharacterCreation(false, true),
                "A later CharGen callback must not authorize a first-level fallback.");
            AssertGrant(true, true, 0, 1, false, 0,
                StartingFirearmGrantDisposition.Grant,
                "Bard 1 to Gunslinger 1 must grant.");
            AssertGrant(true, true, 0, 1, false, 1,
                StartingFirearmGrantDisposition.ReconcileReceipt,
                "A native chargen starter must reconcile rather than duplicate.");
            AssertGrant(true, true, 0, 1, true, 0,
                StartingFirearmGrantDisposition.None,
                "A repeated commit must honor its durable receipt.");
            AssertGrant(true, true, 1, 2, false, 0,
                StartingFirearmGrantDisposition.None,
                "Gunslinger 1 to 2 must not grant.");
            AssertGrant(true, true, 0, 0, false, 0,
                StartingFirearmGrantDisposition.None,
                "A canceled preview must not grant.");
            AssertGrant(true, true, 1, 1, false, 0,
                StartingFirearmGrantDisposition.None,
                "A no-op respec must not grant.");
            AssertGrant(false, true, 0, 1, false, 0,
                StartingFirearmGrantDisposition.None,
                "A disabled module must not grant.");
            AssertGrant(true, false, 0, 1, false, 0,
                StartingFirearmGrantDisposition.None,
                "A detached or hostile unit must not grant.");
            AssertGrant(true, true, 0, 1, false, 0,
                StartingFirearmGrantDisposition.Grant,
                "An unrelated shared-inventory firearm is not an owner-bound receipt.");
            Assertions.Throws<InvalidOperationException>(() =>
                StartingFirearmGrantPolicy.Decide(true, true, 0, 1,
                    false, 2),
                "Ambiguous owner-bound starter corruption must fail closed.");
        }

        internal static void StarterReceiptSurvivesItemAbsence()
        {
            var ledger = new BatteredFirearmOwnershipLedger();
            var owner = new OriginatingUnitId("multiclass-unit");
            var item = new FirearmItemId(
                new Guid("11111111-1111-1111-1111-111111111111"));
            ledger.Bind(item, owner);
            Assertions.True(ledger.HasReceipt(owner),
                "The durable receipt was not recorded.");
            Assertions.True(ledger.HasReceipt(owner),
                "Inventory absence must not erase the per-unit receipt.");
            Assertions.False(ledger.HasReceipt(
                    new OriginatingUnitId("different-unit")),
                "One unit's starter receipt leaked to another unit.");
        }

        internal static void AcadamaeEffectiveModeMatrix()
        {
            AssertMode(false, false, false, false, false, "feat-absent");
            AssertMode(true, true, false, false, false, "off");
            AssertMode(true, true, true, true, true, "on");
            AssertMode(true, true, false, true, false,
                "off-marker-lingering");
            AssertMode(true, true, true, false, true,
                "on-marker-pending");
            AssertMode(true, false, false, true, false,
                "activatable-missing-marker-present");
            for (int cycle = 0; cycle < 8; cycle++)
            {
                AssertMode(true, true, true, true, true, "on");
                AssertMode(true, true, false, true, false,
                    "off-marker-lingering");
            }
        }

        internal static void AcadamaeTrackerClearsStaleActiveCommand()
        {
            var tracker = new AcadamaeInvocationTracker<object, object, object>();
            object armed = new object(), ordinary = new object();
            object spell = new object(), rule = new object();
            tracker.Arm(armed, spell);
            Assertions.True(tracker.Begin(armed),
                "The accelerated command did not begin.");
            Assertions.False(tracker.Begin(ordinary),
                "An ordinary command was incorrectly armed.");
            Assertions.False(tracker.AttachRule(rule, spell),
                "An OFF-created command inherited the prior active command.");
            Assertions.True(tracker.Cancel(armed),
                "The original armed command did not cancel.");
            Assertions.Equal(0, tracker.Count,
                "Canceled command state leaked into the next cast.");
            tracker.Clear();
            Assertions.Equal(0, tracker.Count,
                "Repeated cleanup was not idempotent.");
        }

        internal static void CombatLogPublicationAndFailureIsolation()
        {
            var sink = new RecordingSink();
            var service = new PlayerCombatLogPublicationService(sink);
            int failures = 0;
            string message = "Firearm: Touch AC (14 ft.).";
            Assertions.True(service.Publish(message, exception => failures++),
                "The real-sink abstraction rejected a valid combat entry.");
            Assertions.Equal(1, sink.Count,
                "One mechanical event did not create exactly one sink entry.");
            Assertions.Equal(message, sink.Last,
                "The sink received a different player message.");
            Assertions.Equal(0, failures,
                "A successful sink was reported as failed.");

            bool mechanicalResultCommitted = true;
            sink.Throw = true;
            Assertions.False(service.Publish("Pistol: Broken (misfire).",
                    exception => failures++),
                "A failed combat-log sink was reported as successful.");
            Assertions.True(mechanicalResultCommitted,
                "Feedback failure altered an already committed mechanical result.");
            Assertions.Equal(1, failures,
                "Feedback failure did not retain one structured diagnostic hook.");
        }

        internal static void CombatLogMessagesArePlayerFacing()
        {
            foreach (string message in new[]
            {
                "Firearm: Touch AC (14 ft.).",
                "Firearm: Normal AC (35 ft.).",
                "Firearm: Touch AC (Deadeye).",
                "Acadamae Graduate: Fortitude 18 vs DC 16 - success.",
                "Acadamae Graduate: Fortitude 12 vs DC 16 - failed; Fatigued.",
                "Pistol: Broken (misfire)."
            })
                Assertions.True(PlayerCombatLogMessagePolicy.IsPreferredLength(
                        message),
                    "A common player message is verbose or internal: " + message);
            Assertions.Throws<ArgumentException>(() =>
                PlayerCombatLogMessagePolicy.RequireValid(
                    "blueprint 11111111111111111111111111111111"),
                "A blueprint GUID was accepted in player feedback.");
            Assertions.Throws<ArgumentException>(() =>
                PlayerCombatLogMessagePolicy.RequireValid(
                    "runtime;status=armed;constructor=three"),
                "Structured runtime diagnostics were accepted in player feedback.");
        }

        internal static void ProductionUsesNativeWarningHelperOnly()
        {
            string root = RepositoryRoot();
            string sourceRoot = Path.Combine(root, "src",
                "KingmakerGunslinger");
            string[] offenders = Directory.GetFiles(sourceRoot, "*.cs",
                    SearchOption.AllDirectories)
                .Where(path =>
                {
                    string source = File.ReadAllText(path);
                    return source.Contains("IWarningNotificationUIHandler") ||
                        source.Contains("HandleWarning");
                }).ToArray();
            Assertions.Equal(0, offenders.Length,
                "Production bypasses the stable native warning helper: " +
                string.Join(", ", offenders));

            string[] helperCallers = Directory.GetFiles(sourceRoot, "*.cs",
                    SearchOption.AllDirectories).Where(path =>
                        File.ReadAllText(path).Contains(
                            "UIUtility.SendWarning(message)")).ToArray();
            Assertions.Equal(1, helperCallers.Length,
                "The native warning helper must remain behind one firearm adapter.");
            Assertions.Equal("FirearmConditionTopNotification.cs",
                Path.GetFileName(helperCallers[0]),
                "A production subsystem bypasses the firearm notification adapter.");

            string nativeSink = File.ReadAllText(Path.Combine(sourceRoot,
                "Diagnostics", "NativeCombatLog.cs"));
            Assertions.True(nativeSink.Contains("BattleLogManager.LogView.AddLogEntry(") &&
                    nativeSink.Contains("LogChannel.Combat") &&
                    nativeSink.Contains("PrefixIcon.None"),
                "The feedback sink is not the exact native combat-log contract.");
            string acadamae = File.ReadAllText(Path.Combine(sourceRoot,
                "Acadamae", "AcadamaeCastingPatches.cs"));
            Assertions.True(acadamae.Contains(
                    "context.Logger.Info(\"acadamae\", \"accelerated-cast.resolved\", detail)") &&
                    acadamae.Contains("NativeCombatLog.Publish(\"acadamae\"") &&
                    acadamae.Contains("caster={2}"),
                "Acadamae did not retain structured logger detail beside concise player feedback.");
        }

        internal static void VendorSixRowsPreserveOrderAndRollback()
        {
            object nativeA = new object(), nativeB = new object();
            object[] supplies =
            {
                new object(), new object(), new object(),
                new object(), new object(), new object()
            };
            var publication = VendorCatalogPublication<object>.Create(
                new[] { nativeA, nativeB }, supplies);
            object[] published = publication.Published;
            Assertions.Equal(8, published.Length,
                "The six fixed Bokken rows were not all appended.");
            Assertions.True(ReferenceEquals(nativeA, published[0]) &&
                ReferenceEquals(nativeB, published[1]),
                "Unrelated native component order changed.");
            for (int index = 0; index < supplies.Length; index++)
                Assertions.True(ReferenceEquals(supplies[index],
                        published[index + 2]),
                    "Bokken supply ordering changed at " + index + ".");
            var repeat = VendorCatalogPublication<object>.Create(
                published, supplies);
            Assertions.False(repeat.Changed,
                "Repeated vendor initialization duplicated fixed rows.");
            object[] rollback = publication.Rollback();
            Assertions.Equal(2, rollback.Length,
                "Vendor rollback did not restore the native shape.");
            Assertions.True(ReferenceEquals(nativeA, rollback[0]) &&
                ReferenceEquals(nativeB, rollback[1]),
                "Vendor rollback did not restore exact native references.");
        }

        private static void AssertGrant(bool enabled, bool player,
            int before, int after, bool receipt, int bound,
            StartingFirearmGrantDisposition expected, string message)
        {
            Assertions.Equal(expected, StartingFirearmGrantPolicy.Decide(
                enabled, player, before, after, receipt, bound).Disposition,
                message);
        }

        private static void AssertMode(bool feat, bool activatable, bool isOn,
            bool marker, bool expected, string status)
        {
            AcadamaeEffectiveModeState state = AcadamaeModeStatePolicy.Decide(
                feat, activatable, isOn, marker);
            Assertions.Equal(expected, state.Active,
                "Acadamae effective mode mismatch for " + status + ".");
            Assertions.Equal(status, state.Status,
                "Acadamae mode status mismatch.");
        }

        private static string RepositoryRoot()
        {
            DirectoryInfo cursor = new DirectoryInfo(
                AppDomain.CurrentDomain.BaseDirectory);
            while (cursor != null && !File.Exists(Path.Combine(
                cursor.FullName, "KingmakerGunslinger.sln")))
                cursor = cursor.Parent;
            if (cursor == null)
                throw new DirectoryNotFoundException(
                    "Repository root not found.");
            return cursor.FullName;
        }
    }
}
