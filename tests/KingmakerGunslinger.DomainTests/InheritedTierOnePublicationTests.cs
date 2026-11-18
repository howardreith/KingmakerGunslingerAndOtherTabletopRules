using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// DEF-05 owner decision: the one approved donor change, the established
    /// tier-one menu publication of the two native direct parents. These
    /// controls exercise InheritedTierOnePublicationContract.Judge, the same
    /// judge the native census calls once per recorded donor carrier.
    /// </summary>
    internal static class InheritedTierOnePublicationTests
    {
        private const string Menu = "Kingmaker.UnitLogic.Abilities.Components.AbilityVariants";

        private static InheritedTierOneObservation Approved(string parent)
        {
            object first = new object(), second = new object(), menu = new object();
            var value = new InheritedTierOneObservation
            {
                ParentGuid = parent,
                ReceiptDifferences = new List<string> { "components-array", "components" },
                OriginalComponents = new List<object> { first, second },
                CurrentComponents = new List<InheritedComponentView> {
                    new InheritedComponentView(first, "SpellComponent", "a"),
                    new InheritedComponentView(second, "AbilityEffectRunAction", "b"),
                    new InheritedComponentView(menu, Menu, InheritedTierOnePublicationContract.MenuComponentName) },
                MenuChildren = new List<string> { "preserved", "ownedA", "ownedB", "nativeExpansion" },
                PreservedGuid = "preserved", PreservedIsAbility = true,
                DonorSignature = "ability type=Spell\nspawn unit=native", PreservedSignature = "ability type=Spell\nspawn unit=native",
                DonorActionTypes = new List<string> { "Conditional", "ContextActionSpawnMonster" },
                PreservedActionTypes = new List<string> { "Conditional", "ContextActionSpawnMonster" },
                DonorApplyBuffIds = new List<string> { "celestialI", "fiendishI" },
                PreservedApplyBuffIds = new List<string> { "celestialI", "fiendishI" },
                ExpectedOwnedChildren = new List<string> { "ownedA", "ownedB", "nativeExpansion" },
            };
            return value;
        }

        private static string Judge(InheritedTierOneObservation value, out IList<string> failures)
        {
            failures = new List<string>();
            return InheritedTierOnePublicationContract.Judge(value, failures);
        }

        private static void Rejected(string label, InheritedTierOneObservation value, string finding)
        {
            IList<string> failures;
            string disposition = Judge(value, out failures);
            Assertions.Equal("mutated", disposition, label + " must not be approved.");
            Assertions.True(failures.Any(item => item.Contains(finding)),
                label + ": expected '" + finding + "', got " + string.Join(", ", failures.ToArray()));
        }

        internal static void ApprovedTransitionsAndOrdinaryDonors()
        {
            IList<string> failures;
            var unchanged = new InheritedTierOneObservation { ParentGuid = "7ab27a0d547742741beb5d089f1c3852" };
            Assertions.Equal("unchanged", Judge(unchanged, out failures), "An ordinary unchanged donor passes.");
            Assertions.Equal("approved-tier-one-publication", Judge(Approved(InheritedTierOnePublicationContract.SummonMonsterI), out failures),
                "The exact Summon Monster I publication transition is approved: " + string.Join(",", failures.ToArray()));
            Assertions.Equal("approved-tier-one-publication", Judge(Approved(InheritedTierOnePublicationContract.SummonNaturesAllyI), out failures),
                "The exact Summon Nature's Ally I publication transition is approved: " + string.Join(",", failures.ToArray()));
            Rejected("The same transition on a non-tier-one donor", Approved("7ab27a0d547742741beb5d089f1c3852"), "donor-mutated:components");
            InheritedTierOneObservation foreign = Approved(InheritedTierOnePublicationContract.SummonMonsterI);
            foreign.MenuChildren = new List<string> { "foreignRow", "nativeExpansion", "ownedB", "preserved", "ownedA", "anotherForeign" };
            Assertions.Equal("approved-tier-one-publication", Judge(foreign, out failures),
                "Compatible foreign rows and a reordered menu are allowed: " + string.Join(",", failures.ToArray()));
        }

        internal static void ComponentChangesAreRejected()
        {
            string parent = InheritedTierOnePublicationContract.SummonMonsterI;
            InheritedTierOneObservation replaced = Approved(parent);
            replaced.CurrentComponents[1] = new InheritedComponentView(new object(), "AbilityEffectRunAction", "b");
            Rejected("One original component replaced", replaced, "original-components-not-kept-in-order");
            InheritedTierOneObservation reordered = Approved(parent);
            reordered.CurrentComponents = new List<InheritedComponentView> { reordered.CurrentComponents[1], reordered.CurrentComponents[0], reordered.CurrentComponents[2] };
            Rejected("Original components reordered", reordered, "original-components-not-kept-in-order");
            InheritedTierOneObservation removed = Approved(parent);
            removed.CurrentComponents.RemoveAt(1);
            Rejected("One original component removed", removed, "component-count");
            InheritedTierOneObservation twoMenus = Approved(parent);
            twoMenus.CurrentComponents.Add(new InheritedComponentView(new object(), Menu, InheritedTierOnePublicationContract.MenuComponentName));
            Rejected("Two AbilityVariants components", twoMenus, "variants-components:2");
            InheritedTierOneObservation wrongName = Approved(parent);
            wrongName.CurrentComponents[2] = new InheritedComponentView(wrongName.CurrentComponents[2].Reference, Menu, "$Foreign_Variants");
            Rejected("A wrong variants-component name", wrongName, "added-component-name");
            InheritedTierOneObservation extra = Approved(parent);
            extra.CurrentComponents.Add(new InheritedComponentView(new object(), "SpellDescriptorComponent", "extra"));
            Rejected("An unrelated extra component", extra, "added-components:2");
            InheritedTierOneObservation inPlace = Approved(parent);
            inPlace.DifferencesExcludingMenu = new List<string> { "apply-buff-ids" };
            Rejected("An original action or native-template identity changed in place", inPlace, "parent-graph-changed:apply-buff-ids");
        }

        internal static void PreservedChildIsProved()
        {
            string parent = InheritedTierOnePublicationContract.SummonNaturesAllyI;
            InheritedTierOneObservation missing = Approved(parent);
            missing.MenuChildren.Remove("preserved");
            Rejected("A missing preserved child", missing, "preserved-child-in-menu:0");
            InheritedTierOneObservation duplicate = Approved(parent);
            duplicate.MenuChildren.Add("preserved");
            Rejected("A duplicated preserved child", duplicate, "preserved-child-in-menu:2");
            InheritedTierOneObservation wrongIdentity = Approved(parent);
            wrongIdentity.PreservedGuid = "someOtherAbility";
            Rejected("A wrong preserved stable identity", wrongIdentity, "preserved-child-in-menu:0");
            InheritedTierOneObservation itself = Approved(parent);
            itself.PreservedGuid = parent; itself.MenuChildren.Add(parent);
            Rejected("The parent itself standing in for the preserved child", itself, "preserved-child-is-the-parent");
            InheritedTierOneObservation unit = Approved(parent);
            unit.PreservedSignature = "ability type=Spell\nspawn unit=otherNative";
            Rejected("A preserved child with a changed spawned unit", unit, "preserved-signature-differs");
            InheritedTierOneObservation quantity = Approved(parent);
            quantity.PreservedSignature = "ability type=Spell\nspawn unit=native;count=D3";
            Rejected("A preserved child with a changed quantity expression", quantity, "preserved-signature-differs");
            InheritedTierOneObservation duration = Approved(parent);
            duration.PreservedSignature = "ability type=Spell\nresource cost=2\nspawn unit=native";
            Rejected("A preserved child with a changed duration or resource contract", duration, "preserved-signature-differs");
            InheritedTierOneObservation shared = Approved(parent);
            shared.PreservedSharesInstances = true;
            Rejected("A preserved child sharing mutable instances with the donor", shared, "preserved-shares-instances");
            InheritedTierOneObservation buffs = Approved(parent);
            buffs.PreservedApplyBuffIds = new List<string> { "celestialI" };
            Rejected("A preserved child with different native-template applications", buffs, "preserved-buff-identities-differ");
        }

        internal static void OwnedMenuChildrenAndRecordAreChecked()
        {
            string parent = InheritedTierOnePublicationContract.SummonMonsterI;
            InheritedTierOneObservation missing = Approved(parent);
            missing.MenuChildren.Remove("ownedA");
            Rejected("An expected owned menu child missing", missing, "owned-child:ownedA:0");
            InheritedTierOneObservation duplicate = Approved(parent);
            duplicate.MenuChildren.Add("ownedB");
            Rejected("An expected owned menu child duplicated", duplicate, "owned-child:ownedB:2");
            InheritedTierOneObservation record = Approved(parent);
            record.RecordDisagreement = "original-components-differ-from-receipt";
            Rejected("A publication record that disagrees with the receipt", record, "publication-record:");
        }
    }
}
