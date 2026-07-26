using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace KingmakerGunslinger.Persistence
{
    internal static class PersistenceMatrixCatalog
    {
        private static readonly ReadOnlyCollection<PersistenceMatrixStepDefinition> Definitions =
            new ReadOnlyCollection<PersistenceMatrixStepDefinition>(new[]
            {
                C("I01", "Clean launch and blueprint bootstrap", "One bootstrap, eight custom blueprints, no collisions or type errors"),
                C("I02", "Runtime-contract inspection", "Exactly one readable inherited UniqueId; value type Guid or string"),
                C("I03", "Create A-D", "Four distinct nonempty item IDs; A-C records match; D has no record", true),
                C("I04", "Equip/unequip", "IDs and states unchanged"),
                C("I05", "Switch weapon sets", "IDs and states unchanged"),
                C("I06", "Transfer between companions", "IDs and states follow the physical guns, not wielders"),
                C("I07", "Move to/from shared stash", "IDs and states unchanged"),
                C("I08", "Area transition", "IDs and states unchanged"),
                C("I09", "Rest", "IDs and states unchanged"),
                C("I10", "Save/load in same process", "IDs and states reconstructed exactly", true),
                C("I11", "Exit to desktop, restart, reload", "Same item IDs and states restored; new runtime objects may differ", true),
                C("I12", "Repeat three restart cycles", "No drift, duplication, or record growth"),
                C("I13", "Sell A, save, restart, repurchase A", "Same physical gun retains identity and state", true),
                C("I14", "Sell A to one vendor and buy later", "No state transfer to another item"),
                C("I15", "Duplicate/copy a Test Musket through any available engine path", "Duplicate receives a different ID and begins empty/Normal unless copying semantics are explicitly adopted later", true),
                C("I16", "Delete/destroy B, save, restart", "No surviving gun receives B's identity or state"),
                C("I17", "Respec wielder", "Gun IDs/states unchanged"),
                C("I18", "Load save made before Sprint 14 with ordinary Test Muskets", "Items without records begin empty/Normal; no invented state"),
                C("I19", "Sprint 13 fixture: direct-reference Broken/Empty", "Next observation writes matching identity record and removes legacy record", true),
                C("I20", "Sprint 13 equivalent duplicate", "Legacy record removed; identity record retained"),
                C("I21", "Sprint 13 conflict", "Both carriers preserved; access fails closed; no partial migration"),
                C("I22", "Sprint 13 null/unresolved reference", "Evidence preserved; no unrelated state loss"),
                C("I23", "Sprint 12 token fixture", "Token migrates into identity record, verifies, then clears", true),
                C("I24", "Sprint 12 token conflict", "Both carriers preserved; no overwrite"),
                C("I25", "Native Heavy Crossbow negative control", "No identity-vault record and ordinary AC"),
                C("I26", "Two blueprint-identical Test Muskets", "Distinct IDs and independent state across restart"),
                C("I27", "Missing/malformed ID diagnostic build or fixture", "State access fails; no implicit empty record or generated ID"),
                C("I28", "Duplicate identity-record corruption fixture", "Load/read fails closed; records preserved"),
                C("I29", "Unsupported record schema fixture", "Load/read fails closed; records preserved"),
                C("I30", "Remove mod after save backup", "Documented dependency warning; never claim safe uninstall"),
                H("I31", "Item tooltip and value", "Identity records add no visible enchantment, price, or combat modifier"),
                H("I32", "Save-size measurement over 100 writes/resets", "Record count returns to expected value; no unbounded growth"),
                H("I33", "Call of the Wild compatibility pass", "No duplicate lifecycle initialization or UnitPart conflict"),
                H("I34", "Craft Magic Items compatibility pass", "No identity/state loss when enchanting a firearm copy, if supported"),
                H("I35", "Proper Flanking/combat-rule compatibility", "Touch-AC behavior remains scoped to exact firearms")
            });

        internal static IReadOnlyList<PersistenceMatrixStepDefinition> All
        {
            get { return Definitions; }
        }

        internal static PersistenceMatrixStepDefinition Require(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A persistence-matrix step ID is required.", "id");
            }

            PersistenceMatrixStepDefinition match = Definitions.SingleOrDefault(
                definition => string.Equals(definition.Id, id.Trim(), StringComparison.Ordinal));
            if (match == null)
            {
                throw new KeyNotFoundException("Unknown persistence-matrix step ID: " + id);
            }

            return match;
        }

        private static PersistenceMatrixStepDefinition C(
            string id,
            string operation,
            string requiredResult,
            bool reproduce = false)
        {
            return new PersistenceMatrixStepDefinition(
                id,
                PersistenceEvidenceSeverity.Critical,
                operation,
                requiredResult,
                reproduce);
        }

        private static PersistenceMatrixStepDefinition H(
            string id,
            string operation,
            string requiredResult)
        {
            return new PersistenceMatrixStepDefinition(
                id,
                PersistenceEvidenceSeverity.High,
                operation,
                requiredResult,
                false);
        }
    }
}
