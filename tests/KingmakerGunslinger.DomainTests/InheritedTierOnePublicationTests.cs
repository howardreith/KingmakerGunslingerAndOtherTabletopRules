using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// DEF-05 owner decisions: the one approved donor change (the established tier-one menu publication of the two
    /// native direct parents) and the revised condition 3 (the preserved child's live-menu exposure follows an
    /// independent replacement expectation). These controls call InheritedTierOneReplacementContract.Derive and
    /// InheritedTierOnePublicationContract.Judge, the same contracts the native census calls once per donor carrier.
    /// </summary>
    internal static class InheritedTierOnePublicationTests
    {
        private const string Menu = "Kingmaker.UnitLogic.Abilities.Components.AbilityVariants";

        // The two current exact replacement relations, stated so catalog drift is visible.
        private const string MonsterPreserved = "6c7915c9dc494849918e958618f61db0";         // KMG.Summoning.Native.SM.Tier1
        private const string MonsterReplacementSymbol = "KMG.Summoning.Ability.SM.Tier1.Dog.One";
        private const string MonsterReplacementGuid = "d59150382b2c426aa3111d4a6b53acb0";
        private const string AllyPreserved = "b5b7cf07ffeb4533a1320fbd06072cc5";            // KMG.Summoning.Native.SNA.Tier1
        private const string AllyReplacementSymbol = "KMG.Summoning.NativeOption.SNA.Tier1.Mite.One";
        private const string AllyReplacementGuid = "a274c1f96fee44668504b0d4484c8d57";

        // Production umbrellas whose generated requirement is not an EquivalentCreatureKey (outside the tier-one contract).
        private static readonly string[] Umbrellas = {
            "0964bf88b582bed41b74e79596c4f6d9", "237d76aebbb28334e95d83475611cb47",
            "6e805a9e3ff445146a6386f5a704d4bc", "e96593e67d206ab49ad1b567327d1e75" };

        private static IDictionary<string, string> Registry()
        {
            JArray entries = (JArray)JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"];
            return entries.ToDictionary(value => (string)value["symbol"], value => (string)value["guid"], StringComparer.Ordinal);
        }

        private static SummonVariantSpec[] Published(SummonFamily family, int tier)
        {
            return ExpandedSummoningCatalog.GenerateVariants(family)
                .Where(value => value.ParentTier == tier && SummonVisibilityCatalog.IsPublished(value)).ToArray();
        }

        private static SummonNativeOptionSpec TierOneOption(SummonFamily family)
        {
            return SummonNativeOptionCatalog.All.Single(value => value.Family == family && value.Tier == 1);
        }

        private static InheritedReplacementExpectation Derive(SummonFamily family, string preserved,
            IEnumerable<SummonVariantSpec> generated, IEnumerable<SummonNativeExpansionSpec> native,
            IDictionary<string, string> registry = null, SummonNativeOptionSpec option = null)
        {
            return InheritedTierOneReplacementContract.Derive(option ?? TierOneOption(family), family, 1, preserved,
                generated, native, SummonNativeExpansionCatalog.All, ExpandedSummoningIdentityCatalog.AbilitySymbol,
                registry ?? Registry());
        }

        private static SummonCreatureSpec Creature(string key)
        { return ExpandedSummoningCatalog.All.Single(value => value.Key == key); }

        internal static void CurrentCatalogsWithholdWithExactReplacements()
        {
            InheritedReplacementExpectation monster = Derive(SummonFamily.Monster, MonsterPreserved,
                Published(SummonFamily.Monster, 1), SummonNativeExpansionCatalog.For(SummonFamily.Monster, 1));
            Assertions.Equal(MonsterPreserved, TierOneOption(SummonFamily.Monster).Guid, "The SM I native option is the preserved child.");
            Assertions.Equal(InheritedPreservedExposure.WithholdCompleteReplacement, monster.Disposition,
                "Summon Monster I withholds its preserved child: " + monster.Reason);
            Assertions.Equal(1, monster.Required.Count, "Summon Monster I has exactly one required replacement.");
            Assertions.Equal("generated|" + MonsterReplacementSymbol + "|" + MonsterReplacementGuid,
                monster.Required[0].Kind + "|" + monster.Required[0].Symbol + "|" + monster.Required[0].Guid,
                "Summon Monster I is replaced by the published single-Dog placement.");
            InheritedReplacementExpectation ally = Derive(SummonFamily.NaturesAlly, AllyPreserved,
                Published(SummonFamily.NaturesAlly, 1), SummonNativeExpansionCatalog.For(SummonFamily.NaturesAlly, 1));
            Assertions.Equal(AllyPreserved, TierOneOption(SummonFamily.NaturesAlly).Guid, "The SNA I native option is the preserved child.");
            Assertions.Equal(InheritedPreservedExposure.WithholdCompleteReplacement, ally.Disposition,
                "Summon Nature's Ally I withholds its preserved child: " + ally.Reason);
            Assertions.Equal(1, ally.Required.Count, "Summon Nature's Ally I has exactly one required replacement.");
            Assertions.Equal("native-expansion|" + AllyReplacementSymbol + "|" + AllyReplacementGuid,
                ally.Required[0].Kind + "|" + ally.Required[0].Symbol + "|" + ally.Required[0].Guid,
                "Summon Nature's Ally I is replaced by the native Mite expansion sourced from the frozen SNA I ability.");
            // Cross-check only (never the runtime verdict): the independent derivation agrees with production.
            IDictionary<string, string> registry = Registry();
            foreach (SummonNativeOptionSpec option in SummonNativeOptionCatalog.All.Where(value => !Umbrellas.Contains(value.Guid)))
            {
                SummonVariantSpec[] generated = Published(option.Family, option.Tier);
                IReadOnlyList<SummonNativeExpansionSpec> native = SummonNativeExpansionCatalog.For(option.Family, option.Tier);
                InheritedReplacementExpectation derived = InheritedTierOneReplacementContract.Derive(option, option.Family,
                    option.Tier, option.Guid, generated, native, SummonNativeExpansionCatalog.All,
                    ExpandedSummoningIdentityCatalog.AbilitySymbol, registry);
                Assertions.True(derived.Disposition != InheritedPreservedExposure.Invalid, "No current option is ambiguous: " +
                    option.Guid + " " + derived.Reason);
                Assertions.Equal(SummonNativeOptionCatalog.ReplacementCoverageIsComplete(option, generated, native),
                    derived.Disposition == InheritedPreservedExposure.WithholdCompleteReplacement,
                    "Independent coverage agrees with production for " + option.Guid + ": " + derived.Reason);
            }
        }

        internal static void AmbiguousOrWrongReplacementsAreNeverComplete()
        {
            SummonVariantSpec[] monster = Published(SummonFamily.Monster, 1);
            SummonVariantSpec dog = monster.Single(value => value.Creature.Key == "dog" && value.Multiplicity == SummonMultiplicity.One);
            IReadOnlyList<SummonNativeExpansionSpec> monsterNative = SummonNativeExpansionCatalog.For(SummonFamily.Monster, 1);
            Expect(InheritedPreservedExposure.Invalid, "a duplicate required generated replacement",
                Derive(SummonFamily.Monster, MonsterPreserved, monster.Concat(new[] { dog }), monsterNative), "generated-duplicate");
            SummonNativeExpansionSpec mite = SummonNativeExpansionCatalog.For(SummonFamily.NaturesAlly, 1).Single(value => value.Symbol == AllyReplacementSymbol);
            SummonVariantSpec[] ally = Published(SummonFamily.NaturesAlly, 1);
            Expect(InheritedPreservedExposure.Invalid, "a duplicate required native replacement",
                Derive(SummonFamily.NaturesAlly, AllyPreserved, ally, SummonNativeExpansionCatalog.For(SummonFamily.NaturesAlly, 1).Concat(new[] { mite })),
                "native-duplicate-replacement");
            Expect(InheritedPreservedExposure.Invalid, "a wrong-family generated row",
                Derive(SummonFamily.Monster, MonsterPreserved, monster.Concat(ally.Take(1)), monsterNative), "generated-wrong-family");
            Expect(InheritedPreservedExposure.Invalid, "a wrong-tier generated row",
                Derive(SummonFamily.Monster, MonsterPreserved, monster.Concat(Published(SummonFamily.Monster, 2).Take(1)), monsterNative), "generated-wrong-tier");
            SummonVariantSpec[] withoutDog = monster.Where(value => value != dog).ToArray();
            Expect(InheritedPreservedExposure.KeepIncompleteReplacement, "a wrong-multiplicity Dog",
                Derive(SummonFamily.Monster, MonsterPreserved, withoutDog.Concat(new[] {
                    new SummonVariantSpec(SummonFamily.Monster, 1, Creature("dog"), 1, SummonMultiplicity.OneD3) }), monsterNative), "incomplete-replacement");
            Expect(InheritedPreservedExposure.KeepIncompleteReplacement, "a wrong creature key",
                Derive(SummonFamily.Monster, MonsterPreserved, withoutDog, monsterNative), "incomplete-replacement");
            var wrongSource = new SummonNativeExpansionSpec(SummonFamily.NaturesAlly, 1, mite.CreatureKey, mite.IconKey,
                mite.DisplayName, mite.Multiplicity, "00000000000000000000000000000001", mite.UnitGuid, mite.Branch);
            Expect(InheritedPreservedExposure.Invalid, "a wrong-source native replacement",
                Derive(SummonFamily.NaturesAlly, AllyPreserved, ally, SummonNativeExpansionCatalog.For(SummonFamily.NaturesAlly, 1)
                    .Where(value => value != mite).Concat(new[] { wrongSource })), "native-wrong-source");
            Expect(InheritedPreservedExposure.KeepIncompleteReplacement, "an absent native replacement",
                Derive(SummonFamily.NaturesAlly, AllyPreserved, ally, SummonNativeExpansionCatalog.For(SummonFamily.NaturesAlly, 1)
                    .Where(value => value != mite)), "incomplete-replacement");
            Expect(InheritedPreservedExposure.Invalid, "a preserved source GUID that is not the native option",
                Derive(SummonFamily.Monster, AllyPreserved, monster, monsterNative), "option-wrong-source");
            Expect(InheritedPreservedExposure.Invalid, "a wrong-family native option",
                Derive(SummonFamily.Monster, MonsterPreserved, monster, monsterNative, null, TierOneOption(SummonFamily.NaturesAlly)), "option-wrong-family");
            IDictionary<string, string> unregistered = Registry();
            unregistered.Remove(MonsterReplacementSymbol);
            Expect(InheritedPreservedExposure.Invalid, "an unregistered replacement",
                Derive(SummonFamily.Monster, MonsterPreserved, monster, monsterNative, unregistered), "replacement-unregistered");
            var noRequirement = new SummonNativeOptionSpec(SummonFamily.Monster, 1, MonsterPreserved, SummonMultiplicity.One, null);
            Expect(InheritedPreservedExposure.Keep, "an option with no required replacement",
                Derive(SummonFamily.Monster, MonsterPreserved, monster, monsterNative, null, noRequirement), "no-required-replacement");
        }

        private static void Expect(InheritedPreservedExposure expected, string label, InheritedReplacementExpectation actual, string reason)
        {
            Assertions.Equal(expected, actual.Disposition, label + ": " + actual.Reason);
            Assertions.True(actual.Reason.Contains(reason), label + ": expected reason '" + reason + "', got " + actual.Reason);
        }

        // ---- Menu verdict (Judge) ----

        private sealed class Fixture
        {
            internal readonly object Preserved = new object(), Replacement = new object(), OwnedA = new object(),
                Native = new object(), Unrelated = new object(), Foreign = new object(), First = new object(), Second = new object();
        }

        private static InheritedTierOneObservation Approved(string parent, InheritedPreservedExposure exposure, Fixture f)
        {
            var expectation = new InheritedReplacementExpectation { Disposition = exposure, Reason = exposure.ToString() };
            if (exposure == InheritedPreservedExposure.WithholdCompleteReplacement)
                expectation.Required.Add(new InheritedRequiredReplacement("generated", "Sym.Replacement", "replacement", 1));
            if (exposure == InheritedPreservedExposure.KeepIncompleteReplacement)
                expectation.Required.Add(new InheritedRequiredReplacement("generated", "generated:dog.One", null, 0));
            var menu = new List<InheritedMenuRow>();
            if (exposure != InheritedPreservedExposure.WithholdCompleteReplacement) menu.Add(new InheritedMenuRow(f.Preserved, "preserved"));
            menu.Add(new InheritedMenuRow(f.Replacement, "replacement"));
            menu.Add(new InheritedMenuRow(f.OwnedA, "ownedA"));
            menu.Add(new InheritedMenuRow(f.Native, "nativeExpansion"));
            var owned = new HashSet<string>(StringComparer.Ordinal) { "preserved", "replacement", "ownedA", "nativeExpansion", "unrelatedOwned" };
            return new InheritedTierOneObservation
            {
                ParentGuid = parent, ParentReference = new object(),
                ReceiptDifferences = new List<string> { "components-array", "components" },
                OriginalComponents = new List<object> { f.First, f.Second },
                CurrentComponents = new List<InheritedComponentView> {
                    new InheritedComponentView(f.First, "SpellComponent", "a"),
                    new InheritedComponentView(f.Second, "AbilityEffectRunAction", "b"),
                    new InheritedComponentView(new object(), Menu, InheritedTierOnePublicationContract.MenuComponentName) },
                MenuChildren = menu,
                PreservedSymbol = "KMG.Summoning.Native.SM.Tier1", PreservedGuid = "preserved", PreservedReference = f.Preserved,
                PreservedIsAbility = true, Expectation = expectation,
                RequiredReplacementReferences = new Dictionary<string, object>(StringComparer.Ordinal) { { "Sym.Replacement", f.Replacement } },
                OwnedIdentityGuids = owned,
                DonorSignature = "ability type=Spell\nspawn unit=native", PreservedSignature = "ability type=Spell\nspawn unit=native",
                DonorActionTypes = new List<string> { "Conditional", "ContextActionSpawnMonster" },
                PreservedActionTypes = new List<string> { "Conditional", "ContextActionSpawnMonster" },
                DonorApplyBuffIds = new List<string> { "celestialI", "fiendishI" },
                PreservedApplyBuffIds = new List<string> { "celestialI", "fiendishI" },
                ExpectedOwnedChildren = new List<string> { "replacement", "ownedA", "nativeExpansion" },
            };
        }

        private static InheritedTierOneObservation Withheld(string parent = InheritedTierOnePublicationContract.SummonMonsterI)
        { return Approved(parent, InheritedPreservedExposure.WithholdCompleteReplacement, new Fixture()); }

        private static string Judge(InheritedTierOneObservation value, out IList<string> failures)
        {
            failures = new List<string>();
            return InheritedTierOnePublicationContract.Judge(value, failures);
        }

        private static void Passes(string label, InheritedTierOneObservation value)
        {
            IList<string> failures;
            Assertions.Equal("approved-tier-one-publication", Judge(value, out failures), label + ": " + string.Join(", ", failures.ToArray()));
        }

        private static void Rejected(string label, InheritedTierOneObservation value, string finding)
        {
            IList<string> failures;
            string disposition = Judge(value, out failures);
            Assertions.Equal("mutated", disposition, label + " must not be approved.");
            Assertions.True(failures.Any(item => item.Contains(finding)),
                label + ": expected '" + finding + "', got " + string.Join(", ", failures.ToArray()));
        }

        internal static void ExposureFollowsTheExpectation()
        {
            IList<string> failures;
            Assertions.Equal("unchanged", Judge(new InheritedTierOneObservation { ParentGuid = "7ab27a0d547742741beb5d089f1c3852" }, out failures),
                "An ordinary unchanged donor passes.");
            Passes("Summon Monster I, complete coverage, preserved child absent", Withheld());
            Passes("Summon Nature's Ally I, complete coverage, preserved child absent", Withheld(InheritedTierOnePublicationContract.SummonNaturesAllyI));
            Rejected("The same transition on a non-tier-one donor", Withheld("7ab27a0d547742741beb5d089f1c3852"), "donor-mutated:components");
            var f = new Fixture();
            InheritedTierOneObservation present = Approved(InheritedTierOnePublicationContract.SummonMonsterI, InheritedPreservedExposure.WithholdCompleteReplacement, f);
            present.MenuChildren.Add(new InheritedMenuRow(f.Preserved, "preserved"));
            Rejected("Complete coverage with the preserved child present", present, "preserved-child-withheld-but-in-menu:guid=1,reference=1");
            InheritedTierOneObservation impostor = Withheld();
            impostor.MenuChildren.Add(new InheritedMenuRow(new object(), "preserved"));
            Rejected("Complete coverage with another object carrying the preserved GUID", impostor, "preserved-child-withheld-but-in-menu:guid=1,reference=0");
            Passes("Incomplete coverage with the preserved child once",
                Approved(InheritedTierOnePublicationContract.SummonMonsterI, InheritedPreservedExposure.KeepIncompleteReplacement, new Fixture()));
            Passes("No required replacement with the preserved child once",
                Approved(InheritedTierOnePublicationContract.SummonMonsterI, InheritedPreservedExposure.Keep, new Fixture()));
            InheritedTierOneObservation absent = Approved(InheritedTierOnePublicationContract.SummonMonsterI, InheritedPreservedExposure.KeepIncompleteReplacement, new Fixture());
            absent.MenuChildren.RemoveAt(0);
            Rejected("Incomplete coverage with the preserved child absent", absent, "preserved-child-in-menu:guid=0,reference=0,expected=1");
            f = new Fixture();
            InheritedTierOneObservation wrongPreservedReference = Approved(InheritedTierOnePublicationContract.SummonMonsterI, InheritedPreservedExposure.KeepIncompleteReplacement, f);
            wrongPreservedReference.MenuChildren[0] = new InheritedMenuRow(new object(), "preserved");
            Rejected("The preserved GUID on the wrong object", wrongPreservedReference, "preserved-child-in-menu:guid=1,reference=0");
            InheritedTierOneObservation invalid = Withheld();
            invalid.Expectation = new InheritedReplacementExpectation { Disposition = InheritedPreservedExposure.Invalid,
                Reason = "invalid", Findings = new List<string> { "generated-duplicate-replacement:dog.One" } };
            Rejected("Invalid replacement coverage", invalid, "replacement-coverage:generated-duplicate-replacement:dog.One");
            InheritedTierOneObservation noExpectation = Withheld();
            noExpectation.Expectation = null;
            Rejected("A missing replacement expectation", noExpectation, "replacement-expectation-missing");
        }

        internal static void RequiredReplacementsAreExactRows()
        {
            InheritedTierOneObservation wrongReference = Withheld();
            wrongReference.MenuChildren[0] = new InheritedMenuRow(new object(), "replacement");
            Rejected("The replacement GUID on the wrong object", wrongReference, "replacement-in-menu:Sym.Replacement:guid=1,reference=0");
            InheritedTierOneObservation absent = Withheld();
            absent.MenuChildren.RemoveAt(0);
            Rejected("A required replacement absent from the menu", absent, "replacement-in-menu:Sym.Replacement:guid=0,reference=0");
            var f = new Fixture();
            InheritedTierOneObservation duplicate = Approved(InheritedTierOnePublicationContract.SummonMonsterI, InheritedPreservedExposure.WithholdCompleteReplacement, f);
            duplicate.MenuChildren.Add(new InheritedMenuRow(f.Replacement, "replacement"));
            Rejected("A required replacement duplicated in the menu", duplicate, "replacement-in-menu:Sym.Replacement:guid=2,reference=2");
            InheritedTierOneObservation unresolved = Withheld();
            unresolved.RequiredReplacementReferences.Clear();
            Rejected("A required replacement without a registered object", unresolved, "replacement-unresolved:Sym.Replacement");
            InheritedTierOneObservation unexpected = Withheld();
            unexpected.MenuChildren.Add(new InheritedMenuRow(new object(), "unrelatedOwned"));
            Rejected("Every expected child plus one unrelated KMG-owned summon ability", unexpected, "unexpected-owned-child:unrelatedOwned");
            InheritedTierOneObservation foreign = Withheld();
            foreign.MenuChildren.Add(new InheritedMenuRow(new object(), "foreignRow"));
            Passes("A compatible foreign row", foreign);
            InheritedTierOneObservation reordered = Withheld();
            reordered.MenuChildren = new List<InheritedMenuRow> { new InheritedMenuRow(new object(), "foreignRow"), reordered.MenuChildren[2],
                reordered.MenuChildren[0], new InheritedMenuRow(new object(), "anotherForeign"), reordered.MenuChildren[1] };
            Passes("Foreign rows and a reordered menu", reordered);
            InheritedTierOneObservation missingOwned = Withheld();
            missingOwned.MenuChildren.RemoveAt(1);
            Rejected("An expected owned menu child missing", missingOwned, "owned-child:ownedA:0");
            InheritedTierOneObservation duplicateOwned = Withheld();
            duplicateOwned.MenuChildren.Add(new InheritedMenuRow(duplicateOwned.MenuChildren[1].Reference, "ownedA"));
            Rejected("An expected owned menu child duplicated", duplicateOwned, "owned-child:ownedA:2");
        }

        internal static void ComponentChangesAreRejected()
        {
            InheritedTierOneObservation replaced = Withheld();
            replaced.CurrentComponents[1] = new InheritedComponentView(new object(), "AbilityEffectRunAction", "b");
            Rejected("One original component replaced", replaced, "original-components-not-kept-in-order");
            InheritedTierOneObservation reordered = Withheld();
            reordered.CurrentComponents = new List<InheritedComponentView> { reordered.CurrentComponents[1], reordered.CurrentComponents[0], reordered.CurrentComponents[2] };
            Rejected("Original components reordered", reordered, "original-components-not-kept-in-order");
            InheritedTierOneObservation removed = Withheld();
            removed.CurrentComponents.RemoveAt(1);
            Rejected("One original component removed", removed, "component-count");
            InheritedTierOneObservation twoMenus = Withheld();
            twoMenus.CurrentComponents.Add(new InheritedComponentView(new object(), Menu, InheritedTierOnePublicationContract.MenuComponentName));
            Rejected("Two AbilityVariants components", twoMenus, "variants-components:2");
            InheritedTierOneObservation wrongName = Withheld();
            wrongName.CurrentComponents[2] = new InheritedComponentView(wrongName.CurrentComponents[2].Reference, Menu, "$Foreign_Variants");
            Rejected("A wrong variants-component name", wrongName, "added-component-name");
            InheritedTierOneObservation extra = Withheld();
            extra.CurrentComponents.Add(new InheritedComponentView(new object(), "SpellDescriptorComponent", "extra"));
            Rejected("An unrelated extra component", extra, "added-components:2");
            InheritedTierOneObservation inPlace = Withheld();
            inPlace.DifferencesExcludingMenu = new List<string> { "apply-buff-ids" };
            Rejected("An original action or native-template identity changed in place", inPlace, "parent-graph-changed:apply-buff-ids");
            InheritedTierOneObservation record = Withheld();
            record.RecordDisagreement = "original-components-differ-from-receipt";
            Rejected("A publication record that disagrees with the receipt", record, "publication-record:");
        }

        internal static void PreservedChildIsProvedEvenWhenWithheld()
        {
            string parent = InheritedTierOnePublicationContract.SummonNaturesAllyI;
            InheritedTierOneObservation unresolved = Withheld(parent);
            unresolved.PreservedReference = null;
            Rejected("An unresolved preserved child", unresolved, "preserved-child-unresolved");
            InheritedTierOneObservation notAbility = Withheld(parent);
            notAbility.PreservedIsAbility = false;
            Rejected("A preserved object that is not an ability", notAbility, "preserved-child-unresolved");
            InheritedTierOneObservation itself = Withheld(parent);
            itself.PreservedGuid = parent;
            Rejected("The parent itself standing in for the preserved child", itself, "preserved-child-is-the-parent");
            InheritedTierOneObservation unit = Withheld(parent);
            unit.PreservedSignature = "ability type=Spell\nspawn unit=otherNative";
            Rejected("A withheld preserved child with a changed spawned unit", unit, "preserved-signature-differs");
            InheritedTierOneObservation quantity = Withheld(parent);
            quantity.PreservedSignature = "ability type=Spell\nspawn unit=native;count=D3";
            Rejected("A withheld preserved child with a changed quantity expression", quantity, "preserved-signature-differs");
            InheritedTierOneObservation duration = Withheld(parent);
            duration.PreservedSignature = "ability type=Spell\nresource cost=2\nspawn unit=native";
            Rejected("A withheld preserved child with a changed duration or resource contract", duration, "preserved-signature-differs");
            InheritedTierOneObservation shared = Withheld(parent);
            shared.PreservedSharesInstances = true;
            Rejected("A withheld preserved child sharing mutable instances with the donor", shared, "preserved-shares-instances");
            InheritedTierOneObservation actions = Withheld(parent);
            actions.PreservedActionTypes = new List<string> { "ContextActionSpawnMonster" };
            Rejected("A withheld preserved child with different action types", actions, "preserved-action-types-differ");
            InheritedTierOneObservation buffs = Withheld(parent);
            buffs.PreservedApplyBuffIds = new List<string> { "celestialI" };
            Rejected("A withheld preserved child with different native-template applications", buffs, "preserved-buff-identities-differ");
        }
    }
}
