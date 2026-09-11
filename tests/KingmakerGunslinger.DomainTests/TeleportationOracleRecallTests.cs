using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Spells.ShieldOther;

namespace KingmakerGunslinger.DomainTests
{
    // Regression coverage for the optional Call of the Wild Oracle Word of
    // Recall scroll eligibility repair: the level-6 merge behavior through the
    // shared publication policy, and the source contracts that keep the
    // final-live reconciliation optional, additive, idempotent, and isolated
    // from the native Cleric 6 / Druid 8 base publication.
    internal static class TeleportationOracleRecallTests
    {
        private sealed class FakeSpell
        {
            internal FakeSpell(string guid) { Guid = guid; }
            internal string Guid { get; private set; }
        }

        internal static void OracleLevelSixMergePreservesForeignEntriesOnce()
        {
            var heal = new FakeSpell("heal");
            var recall = new FakeSpell("word-of-recall");
            var foreignDuplicate = new FakeSpell("word-of-recall");
            var current = new List<FakeSpell> { heal, foreignDuplicate };
            List<FakeSpell> published = ShieldOtherSpellListMergePolicy.Merge(
                current, recall, value => value.Guid);
            Assertions.True(published.Count == 2 &&
                ReferenceEquals(published[0], heal) &&
                ReferenceEquals(published[1], recall),
                "The Oracle level-6 publication must preserve foreign order and singularize the canonical Word of Recall by reference/GUID.");
            List<FakeSpell> second = ShieldOtherSpellListMergePolicy.Merge(
                published, recall, value => value.Guid);
            Assertions.True(ReferenceEquals(second, published) && second.Count == 2,
                "Repeated Oracle reconciliation must be idempotent with no duplicate entries.");
            List<FakeSpell> already = new List<FakeSpell> { heal, recall };
            List<FakeSpell> unchanged = ShieldOtherSpellListMergePolicy.Merge(
                already, recall, value => value.Guid);
            Assertions.True(ReferenceEquals(unchanged, already),
                "An already-published Oracle list must not be rebuilt or reordered.");
        }

        internal static void OracleMergeFailsClosedOnNullEntries()
        {
            var recall = new FakeSpell("word-of-recall");
            Assertions.Throws<InvalidOperationException>(() =>
                ShieldOtherSpellListMergePolicy.Merge(
                    new List<FakeSpell> { new FakeSpell("heal"), null }, recall, value => value.Guid),
                "A malformed optional Oracle list must fail closed instead of publishing.");
        }

        internal static void ReconcilerSourceContract()
        {
            string root = Environment.CurrentDirectory;
            string source = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Spells", "Teleportation",
                "TeleportationFinalLiveReconciler.cs"));
            foreach (string token in new[] {
                "32c02466b2364c8a906e6e4761175099",
                "OracleWordOfRecallLevel = 6",
                "ReferenceEquals(book.CharacterClass, value)",
                "book.SpellList.MaxLevel >= OracleWordOfRecallLevel",
                "book.Spontaneous && !book.IsArcane",
                "book.CastingAttribute == StatType.Charisma",
                "string.Equals(value.AssetGuid, OracleClassId, StringComparison.Ordinal)",
                "duplicate.final-live",
                "reconcile.disabled",
                "reconcile.failed",
                "m_SpellsFiltered",
                "restoration refused" })
                Assertions.True(source.Contains(token),
                    "Oracle final-live reconciler contract is missing: " + token);
            Assertions.True(source.Split(new[] { "ReconcileOptional(library, wordOfRecall);" },
                StringSplitOptions.None).Length == 3,
                "The Oracle reconciliation must run an idempotent second pass.");
            Assertions.True(source.Contains("context.FeatureModules.Active.TeleportationSpells") &&
                source.Contains("BlueprintBootstrap.TeleportationPublication == null"),
                "The reconciliation must stay gated on the module flag and a successful base publication.");
        }

        internal static void BasePublicationPreservedContract()
        {
            string root = Environment.CurrentDirectory;
            string publication = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints",
                "TeleportationSpellListPublication.cs"));
            Assertions.True(publication.Contains("Resolve(library, ClericListId, 6, spells.WordOfRecall)") &&
                publication.Contains("Resolve(library, DruidListId, 8, spells.WordOfRecall)"),
                "The native Cleric 6 / Druid 8 Word of Recall registration must remain unchanged.");
            Assertions.True(publication.Contains("RollbackAll") && publication.Contains("m_SpellsFiltered"),
                "The existing rollback and cache-clearing behavior must remain.");
            string main = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Main.cs"));
            Assertions.True(main.Contains("Spells.Teleportation.TeleportationFinalLiveReconciler.AttachFirstUpdate(context);"),
                "The reconciler must attach once from the composition root after Shield Other's.");
        }

        internal static void ScrollIdentityPreservedContract()
        {
            string root = Environment.CurrentDirectory;
            string scrolls = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Blueprints", "TeleportationScrollBlueprints.cs"));
            Assertions.True(scrolls.Contains("WordOfRecallDonorId") &&
                scrolls.Contains("scroll.Ability = spell") &&
                scrolls.Contains("CopyScroll { CustomSpell = spell }"),
                "The canonical Word of Recall scroll identity and teaching association must remain unchanged.");
        }
    }
}
