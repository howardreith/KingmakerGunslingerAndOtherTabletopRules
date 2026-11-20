using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The one authorized exception to the historical manifest prefix: the
    /// corrected Gunsmith's Kit note (INT-07). Every case here runs the
    /// check the suite runs on the working manifest,
    /// <see cref="BetterVendorsProgressionTests.ManifestPrefixProblem"/>,
    /// on a copy of the manifest's records held in memory and changed in
    /// one stated way. The working manifest is never written.
    /// </summary>
    internal static class ManifestNoteExceptionTests
    {
        // Stated here on their own, so that a change to the guard's own
        // constants is itself caught.
        private const string HistoricalSha256 =
            "43909eb0094d38862dfe26c61345554b5d364d523a2c1ce1b91b996760a52fd2";
        private const int Preserved = 1913;
        private const int KitPosition = 206;
        private const string KitSymbol = "KMG.Gunsmithing.GunsmithKit";
        private const string KitGuid = "d52aacb753434691b1ed85a16cc87104";
        private const string StaleNote =
            "Stable non-consumable Gunsmith's Kit required for basic ammunition crafting and for unified Broken-or-Wrecked-to-Normal firearm repair.";
        private const string ApprovedNote =
            "Stable non-consumable Gunsmith's Kit required for basic ammunition crafting and for Broken-to-Normal field repair. Wrecked firearms are restored only by a completed full rest.";
        private const string PrefixChanged = "The preserved 1913-entry manifest prefix changed.";

        private static List<JToken> Copy()
        {
            return ((JArray)JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                "blueprints", "blueprints.json")))["entries"]).Select(value => value.DeepClone()).ToList();
        }

        private static string After(Action<List<JToken>> change)
        {
            List<JToken> copy = Copy();
            change(copy);
            return BetterVendorsProgressionTests.ManifestPrefixProblem(copy);
        }

        private static string Hash(IEnumerable<JToken> entries, bool kitNoteFrozen)
        {
            string text = string.Concat(entries.Take(Preserved).Select((value, index) => string.Join("|", new[] {
                (string)value["symbol"], (string)value["guid"], (string)value["plannedType"], (string)value["status"],
                (string)value["milestone"],
                kitNoteFrozen && index == KitPosition ? StaleNote : (string)value["notes"] }) + "\n"));
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", string.Empty)
                    .ToLowerInvariant();
        }

        internal static void ApprovedNotePassesAndTheHistoricalHashHolds()
        {
            List<JToken> manifest = Copy();
            Assertions.True(BetterVendorsProgressionTests.ManifestPrefixProblem(manifest) == null,
                "The working manifest does not pass the prefix guard: " +
                BetterVendorsProgressionTests.ManifestPrefixProblem(manifest));
            JToken kit = manifest[KitPosition];
            Assertions.True((string)kit["symbol"] == KitSymbol && (string)kit["guid"] == KitGuid &&
                (string)kit["plannedType"] == "BlueprintItem" && (string)kit["status"] == "active" &&
                (string)kit["milestone"] == "Sixth Playtest 0.0.66",
                "The Gunsmith's Kit record is not the established record at its established position.");
            Assertions.Equal(ApprovedNote, (string)kit["notes"],
                "The working manifest does not hold the approved Gunsmith's Kit note.");

            // The historical hash is the old one, reached by putting the
            // frozen note back for that one record and nothing else.
            Assertions.Equal(HistoricalSha256, Hash(manifest, true),
                "The historical prefix hash no longer holds after the one authorized substitution.");
            Assertions.False(Hash(manifest, false) == HistoricalSha256,
                "The prefix hashes to the historical value with no substitution, so the note was not changed.");

            // The guard's own record of the exception is the authorized one.
            Assertions.True(BetterVendorsProgressionTests.AuthorizedNoteException.Position == KitPosition &&
                BetterVendorsProgressionTests.AuthorizedNoteException.Symbol == KitSymbol &&
                BetterVendorsProgressionTests.AuthorizedNoteException.Guid == KitGuid &&
                BetterVendorsProgressionTests.AuthorizedNoteException.FrozenNote == StaleNote &&
                BetterVendorsProgressionTests.AuthorizedNoteException.ApprovedNote == ApprovedNote,
                "The guard's exception is not the authorized record, position and wording.");
        }

        internal static void StaleOrUnapprovedNoteFails()
        {
            // The old wording is not an alternative to the new one.
            string problem = After(copy => copy[KitPosition]["notes"] = StaleNote);
            Assertions.True(problem != null && problem.Contains("stale frozen wording"),
                "Reverting to the stale note passed: " + problem);

            foreach (string note in new[] {
                ApprovedNote + " ", ApprovedNote.ToLowerInvariant(),
                ApprovedNote.Replace("only by a completed full rest", "by a full rest or field repair"),
                "Stable non-consumable Gunsmith's Kit.", string.Empty })
            {
                problem = After(copy => copy[KitPosition]["notes"] = note);
                Assertions.True(problem != null && problem.Contains("not the approved wording"),
                    "An unapproved Gunsmith's Kit note passed: '" + note + "' -> " + problem);
            }
        }

        internal static void TheExceptionCoversOneNoteOfOneRecord()
        {
            // Any other protected note.
            foreach (int index in new[] { 0, 36, KitPosition - 1, KitPosition + 1, Preserved - 1 })
            {
                string problem = After(copy => copy[index]["notes"] = (string)copy[index]["notes"] + " changed");
                Assertions.Equal(PrefixChanged, problem, "A changed note at position " + index + " passed.");
            }

            // The approved wording on another record earns nothing, and
            // neither does the frozen wording there.
            Assertions.Equal(PrefixChanged, After(copy => copy[KitPosition + 1]["notes"] = ApprovedNote),
                "The approved wording was accepted on another record.");
            Assertions.Equal(PrefixChanged, After(copy => copy[KitPosition + 1]["notes"] = StaleNote),
                "The frozen wording was accepted on another record.");

            // The rest of the Gunsmith's Kit record is as protected as ever.
            foreach (string field in new[] { "plannedType", "status", "milestone" })
                Assertions.Equal(PrefixChanged,
                    After(copy => copy[KitPosition][field] = (string)copy[KitPosition][field] + "x"),
                    "A changed " + field + " on the Gunsmith's Kit record passed.");
            foreach (string field in new[] { "symbol", "guid" })
            {
                string problem = After(copy => copy[KitPosition][field] = field == "guid" ?
                    "d52aacb753434691b1ed85a16cc87105" : "KMG.Gunsmithing.GunsmithKit2");
                Assertions.True(problem != null && problem.Contains("not at position 206"),
                    "A changed " + field + " on the Gunsmith's Kit record passed: " + problem);
            }

            // Identity and metadata of any other protected record.
            foreach (string field in new[] { "symbol", "guid", "plannedType", "status", "milestone" })
                Assertions.Equal(PrefixChanged,
                    After(copy => copy[40][field] = (string)copy[40][field] + "x"),
                    "A changed " + field + " on another protected record passed.");
        }

        internal static void TheSecondHistoricalLedgerKeepsItsHashUnderTheSameException()
        {
            // The Sprint 17 ledger guard hashes whole records, notes
            // included, over 2,836 historical records. It reaches its old
            // value through the same one-record substitution.
            Assertions.Equal(SerpentineRulesTests.HistoricalLedgerSha256,
                SerpentineRulesTests.HistoricalLedgerDigest(Copy()),
                "The Sprint 17 historical ledger digest no longer holds.");
            Assertions.Equal("bf46e4e3d2d709935f2a27fa32f2bd8ad640098801174680c755c89287ea0570",
                SerpentineRulesTests.HistoricalLedgerSha256, "The Sprint 17 historical digest was replaced.");

            // The stale note, another wording and a moved record are refused
            // before any digest is taken.
            foreach (Action<List<JToken>> change in new Action<List<JToken>>[] {
                copy => copy[KitPosition]["notes"] = StaleNote,
                copy => copy[KitPosition]["notes"] = ApprovedNote + " (and Wrecked in the field)",
                copy =>
                {
                    JToken kit = copy[KitPosition];
                    copy[KitPosition] = copy[KitPosition + 1];
                    copy[KitPosition + 1] = kit;
                } })
            {
                List<JToken> copy = Copy();
                change(copy);
                Assertions.Throws<InvalidOperationException>(
                    () => SerpentineRulesTests.HistoricalLedgerDigest(copy),
                    "The Sprint 17 ledger digest was taken over a manifest outside the authorized exception.");
            }

            // Everything else it protected, it still protects.
            foreach (Action<List<JToken>> change in new Action<List<JToken>>[] {
                copy => copy[KitPosition + 1]["notes"] = (string)copy[KitPosition + 1]["notes"] + " changed",
                copy => copy[KitPosition]["milestone"] = "Seventh Playtest",
                copy => copy[2000]["notes"] = (string)copy[2000]["notes"] + " changed",
                copy => copy[2000]["guid"] = "0f0e0d0c0b0a09080706050403020100",
                copy => copy[2000]["status"] = "retired",
                copy =>
                {
                    JToken first = copy[2000];
                    copy[2000] = copy[2001];
                    copy[2001] = first;
                } })
            {
                List<JToken> copy = Copy();
                change(copy);
                Assertions.False(SerpentineRulesTests.HistoricalLedgerDigest(copy) ==
                    SerpentineRulesTests.HistoricalLedgerSha256,
                    "A change to a protected historical record kept the Sprint 17 ledger digest.");
            }
        }

        internal static void ReorderedOrMovedRecordsFail()
        {
            // Two protected records exchanged.
            Assertions.Equal(PrefixChanged, After(copy =>
            {
                JToken first = copy[10];
                copy[10] = copy[11];
                copy[11] = first;
            }), "Two exchanged protected records passed.");

            // The Gunsmith's Kit record moved: the exception does not follow it.
            string problem = After(copy =>
            {
                JToken kit = copy[KitPosition];
                copy[KitPosition] = copy[KitPosition + 1];
                copy[KitPosition + 1] = kit;
            });
            Assertions.True(problem != null && problem.Contains("not at position 206"),
                "The Gunsmith's Kit record at another position passed: " + problem);

            // A record removed from, or added to, the protected block.
            problem = After(copy => copy.RemoveAt(5));
            Assertions.True(problem != null, "A removed protected record passed.");
            problem = After(copy => copy.Insert(300, copy[300].DeepClone()));
            Assertions.Equal(PrefixChanged, problem, "An inserted record inside the protected block passed.");

            // Fewer records than the block.
            problem = After(copy => copy.RemoveRange(Preserved - 1, copy.Count - (Preserved - 1)));
            Assertions.True(problem != null && problem.Contains("fewer than"), "A truncated manifest passed: " + problem);
        }
    }
}
