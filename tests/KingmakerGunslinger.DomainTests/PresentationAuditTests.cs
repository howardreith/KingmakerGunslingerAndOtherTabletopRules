using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Acquisition;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Presentation;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The firearm visual-rig check and the project item presentation check,
    /// on the production evaluation the two runtime scenarios use, with
    /// stand-in observations in place of live blueprints. Each test reads the
    /// reported identities and outcomes.
    /// </summary>
    internal static class PresentationAuditTests
    {
        // The manifest matches a planned type by its type name only; these
        // stand in for the engine types of the same names.
        private sealed class BlueprintItemWeapon { }
        private sealed class BlueprintItemEquipmentBelt { }
        private sealed class BlueprintWeaponEnchantment { }

        private static readonly string[] FirearmFamilies =
            { "Pistol.", "Musket.", "Blunderbuss.", "Rifle.", "Revolver." };

        private static FirearmVisualBindingObservation BoundItem(
            FirearmVisualRosterEntry entry)
        {
            string variant = WeaponVisualVariantCatalog.Require(entry.Symbol);
            return new FirearmVisualBindingObservation
            {
                Symbol = entry.Symbol, Kind = entry.Kind, ItemPresent = true,
                ItemName = entry.Symbol.Replace('.', '_'), PrefabAvailable = true,
                PrefabName = variant, HasVisualParameters = true,
                ModelName = variant, ModelIsVariantPrefab = true
            };
        }

        private static List<FirearmVisualBindingObservation> BoundRoster()
        {
            return ProjectItemRosters.FirearmItemVisuals.Select(BoundItem).ToList();
        }

        private static FirearmVisualBindingObservation Find(
            List<FirearmVisualBindingObservation> items, string symbol)
        {
            return items.Single(value => value.Symbol == symbol);
        }

        private static void Contains(string text, string fragment, string message)
        {
            Assertions.True(text != null && text.Contains(fragment),
                message + " Reported: " + text);
        }

        // ---- the declared rosters ----

        internal static void RostersAreTheDeclaredIdentities()
        {
            FirearmVisualRosterEntry[] firearms = ProjectItemRosters.FirearmItemVisuals;
            Assertions.Equal(16, firearms.Length,
                "The firearm visual roster is not test musket 1 + production 5 + magic 8 + midgame 2.");
            Assertions.Equal(1, firearms.Count(value => value.Source == "development"),
                "The development Test Musket is not in the roster exactly once.");
            Assertions.Equal(5, firearms.Count(value => value.Source == "production"),
                "The roster does not hold the five production firearms.");
            Assertions.Equal(8, firearms.Count(value => value.Source == "magic"),
                "The roster does not hold the eight declared magic firearms.");
            Assertions.True(firearms.Where(value => value.Source == "midgame")
                .Select(value => value.Symbol).SequenceEqual(
                    MidgameFirearmCatalog.Entries.Select(value => value.Symbol)),
                "The roster's midgame firearms are not the midgame catalog's.");
            Assertions.Equal(firearms.Length, firearms.Select(value => value.Symbol)
                .Distinct(StringComparer.Ordinal).Count(),
                "The firearm visual roster declares an identity twice.");

            // Every roster firearm has an approved variant in its own family.
            foreach (FirearmVisualRosterEntry entry in firearms)
            {
                string variant;
                Assertions.True(WeaponVisualVariantCatalog.TryGet(entry.Symbol,
                    out variant) && variant.StartsWith(entry.Kind + ".",
                        StringComparison.Ordinal),
                    entry.Symbol + " has no approved variant in the " + entry.Kind +
                    " family.");
            }

            // The visual catalog's firearm mappings are this roster plus the
            // generic progression firearms. Those receive their item variants
            // through the same registration helper; the scenario does not read
            // them, so they are not part of its roster.
            string[] catalogFirearms = WeaponVisualVariantCatalog.Snapshot()
                .Where(pair => FirearmFamilies.Any(family =>
                    pair.Value.StartsWith(family, StringComparison.Ordinal)))
                .Select(pair => pair.Key).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            string[] progression = ProgressionWeaponCatalog.All.Where(value =>
                    value.IsFirearm && !value.ReusesCanonicalItem)
                .Select(value => value.Symbol).ToArray();
            Assertions.True(catalogFirearms.SequenceEqual(firearms.Select(
                    value => value.Symbol).Concat(progression).OrderBy(
                    value => value, StringComparer.Ordinal)),
                "The visual catalog's firearm mappings are not exactly the roster and the progression firearms.");
            Assertions.Equal(0, firearms.Select(value => value.Symbol)
                .Intersect(progression).Count(),
                "A progression firearm is in the item visual roster.");

            string[] weapons = ProjectItemRosters.PresentationWeapons;
            Assertions.Equal(5 + 10 + EasternWeaponCatalog.AllGenericItems.Length +
                EasternWeaponNamedCatalog.All.Length +
                ElvenBranchedSpearCatalog.All.Length +
                ElvenBranchedSpearNamedCatalog.All.Length, weapons.Length,
                "The presentation weapons are not the six catalogs' declarations.");
            Assertions.Equal(57, weapons.Length,
                "The presentation weapon roster is not 5 + 10 + 12 + 18 + 6 + 6.");
            Assertions.Equal(weapons.Length, weapons.Distinct(
                StringComparer.Ordinal).Count(),
                "The presentation weapon roster declares an identity twice.");
            Assertions.False(weapons.Contains(ProjectItemRosters.TestMusketSymbol),
                "The development Test Musket is a presentation weapon.");
            Assertions.Equal(58, ProjectItemRosters.PresentationItems.Length,
                "The presentation items are not the weapons and the Cord.");
            Assertions.Equal(ProjectItemRosters.CordSymbol,
                ProjectItemRosters.PresentationItems.Last().Key,
                "The Cord is not the last presentation item.");
            Assertions.Equal(12, ProjectItemRosters.CustomWeaponEnchantments.Length,
                "The custom weapon enchantments are not the declared twelve.");
            Assertions.Equal(1, ProjectItemRosters.CustomWeaponEnchantments.Count(
                value => value == ProjectItemRosters.PolicyOnlyEnchantmentSymbol),
                "The policy-only enchantment is not among the twelve exactly once.");

            // Every declared identity is an active entry of its type in the
            // blueprint manifest the game deploys.
            BlueprintManifest manifest = BlueprintManifest.Load(
                Environment.CurrentDirectory);
            var guids = new HashSet<string>(StringComparer.Ordinal);
            foreach (FirearmVisualRosterEntry entry in firearms)
                guids.Add(manifest.ResolveActive(entry.Symbol,
                    typeof(BlueprintItemWeapon)).Id.Value);
            foreach (KeyValuePair<string, string> item in
                ProjectItemRosters.PresentationItems)
                guids.Add(manifest.ResolveActive(item.Key,
                    item.Value == ProjectItemRosters.BeltType
                        ? typeof(BlueprintItemEquipmentBelt)
                        : typeof(BlueprintItemWeapon)).Id.Value);
            foreach (string symbol in ProjectItemRosters.CustomWeaponEnchantments)
                guids.Add(manifest.ResolveActive(symbol,
                    typeof(BlueprintWeaponEnchantment)).Id.Value);
            // 58 items, the Test Musket and 12 enchantments: 71 distinct identities.
            Assertions.Equal(71, guids.Count,
                "The declared identities do not resolve to 71 distinct manifest GUIDs.");
        }

        // ---- firearm item visuals ----

        internal static void FirearmVisualsValidRosterPasses()
        {
            FirearmVisualRigAudit audit = FirearmVisualRigAudit.Evaluate(
                ProjectItemRosters.FirearmItemVisuals, BoundRoster());
            Assertions.True(audit.Roster.Passed && audit.BindingOutcome.Passed &&
                audit.Passed, "The supported roster, fully bound, did not pass: " +
                audit.Roster.Observed + " / " + audit.BindingOutcome.Observed);
            Assertions.Equal(16, audit.Bindings.Count, "Not every item was inspected.");
            Assertions.True(audit.Bindings.All(value => value.Bound),
                "A bound item was reported as failing.");
            Contains(audit.Roster.Observed, "expected=16;observed=16;count=match;membership=match",
                "The census does not report the matching sets.");
            Contains(audit.BindingOutcome.Observed, "inspected=16;bound=16;failed=0",
                "The binding outcome does not report sixteen bound items.");
            Contains(audit.BindingOutcome.Observed,
                "KMG.Firearms.DuelistsRebuttalItem=Pistol.Duelist:bound",
                "An item's own variant is not in the binding evidence.");
            Assertions.Equal(FirearmVisualRigAudit.RosterName, audit.Roster.Name,
                "The census outcome is not under its own name.");
            Assertions.Equal(FirearmVisualRigAudit.BindingsName,
                audit.BindingOutcome.Name,
                "The binding outcome is not under its own name.");
        }

        internal static void FirearmVisualsCountMismatchAndBadBindingBothReported()
        {
            // One identity absent (a count mismatch) and, at the same time, a
            // different item bound to the wrong model.
            List<FirearmVisualBindingObservation> items = BoundRoster();
            items.RemoveAll(value => value.Symbol == "KMG.Firearms.RoadwardenItem");
            FirearmVisualBindingObservation wrong = Find(items,
                "KMG.Firearms.TheLastWordItem");
            wrong.ModelIsVariantPrefab = false;
            wrong.ModelName = "KMG_Firearm_Pistol_Service";
            FirearmVisualRigAudit audit = FirearmVisualRigAudit.Evaluate(
                ProjectItemRosters.FirearmItemVisuals, items);

            Assertions.False(audit.Roster.Passed, "A count mismatch passed the census.");
            Assertions.False(audit.Census.CountMatches, "15 of 16 counted as a match.");
            Contains(audit.Roster.Observed, "expected=16;observed=15;count=MISMATCH",
                "The census does not report the two counts.");
            Assertions.True(audit.Census.Missing.SequenceEqual(new[] {
                "KMG.Firearms.RoadwardenItem" }), "The missing identity is not named.");

            // The census failure did not stop the bindings being inspected.
            Assertions.False(audit.BindingOutcome.Passed,
                "A wrong model passed the binding check.");
            FirearmVisualBindingResult lastWord = audit.Bindings.Single(value =>
                value.Symbol == "KMG.Firearms.TheLastWordItem");
            Assertions.False(lastWord.Bound, "The wrongly bound item was reported as bound.");
            Assertions.Equal("Pistol.LastWord", lastWord.Variant,
                "The expected variant of the wrongly bound item is not reported.");
            Contains(lastWord.Failure, "KMG_Firearm_Pistol_Service",
                "The observed model is not in the failure.");
            Contains(lastWord.Failure, "Pistol.LastWord",
                "The expected prefab is not in the failure.");
            Contains(audit.BindingOutcome.Observed,
                "KMG.Firearms.TheLastWordItem=Pistol.LastWord:FAIL(",
                "The binding outcome does not locate the failing item.");
            Assertions.Equal(14, audit.Bindings.Count(value => value.Bound),
                "The fourteen sound items were not all inspected and found bound.");
            Assertions.False(audit.Passed, "The aggregate passed.");
        }

        internal static void FirearmVisualsInspectionContinuesAfterABadBinding()
        {
            // Three bad bindings of different kinds, the first of them on the
            // very first item: every one is reported, and so is every item
            // after them.
            List<FirearmVisualBindingObservation> items = BoundRoster();
            Find(items, ProjectItemRosters.TestMusketSymbol).HasVisualParameters = false;
            Find(items, "KMG.Firearms.EarlyBlunderbussItem").PrefabAvailable = false;
            FirearmVisualBindingObservation wrong = Find(items,
                "KMG.Firearms.DeadReckoningItem");
            wrong.ModelIsVariantPrefab = false;
            wrong.ModelName = "CrossbowHeavy";
            FirearmVisualRigAudit audit = FirearmVisualRigAudit.Evaluate(
                ProjectItemRosters.FirearmItemVisuals, items);

            Assertions.True(audit.Roster.Passed,
                "Bad bindings changed the census: " + audit.Roster.Observed);
            Assertions.False(audit.BindingOutcome.Passed, "Three bad bindings passed.");
            string[] failed = audit.Bindings.Where(value => !value.Bound)
                .Select(value => value.Symbol).ToArray();
            Assertions.True(failed.SequenceEqual(new[] {
                ProjectItemRosters.TestMusketSymbol,
                "KMG.Firearms.EarlyBlunderbussItem",
                "KMG.Firearms.DeadReckoningItem" }),
                "Not exactly the three bad bindings were reported: " +
                string.Join(",", failed));
            Assertions.Equal(16, audit.Bindings.Count,
                "Inspection stopped before the last item.");
            Contains(audit.Bindings[0].Failure, "no visual parameters",
                "The first item's reason is wrong.");
            Contains(audit.Bindings.Single(value => value.Symbol ==
                "KMG.Firearms.EarlyBlunderbussItem").Failure, "prefab is not loaded",
                "The missing prefab is not the reported reason.");
            Contains(audit.Bindings.Last().Failure, "CrossbowHeavy",
                "The last item's observed model is not reported.");
            Contains(audit.BindingOutcome.Observed, "inspected=16;bound=13;failed=3",
                "The binding outcome does not count all three.");
        }

        internal static void FirearmVisualsMissingUnexpectedAndReplacement()
        {
            // A replacement that keeps the total at sixteen: one expected
            // identity is gone and an identity outside the roster is there.
            List<FirearmVisualBindingObservation> items = BoundRoster();
            items.RemoveAll(value => value.Symbol == "KMG.Firearms.MusketPlus1Item");
            items.Add(new FirearmVisualBindingObservation
            {
                Symbol = "KMG.Firearms.MusketPlus2Item", Kind = FirearmKind.Musket,
                ItemPresent = true, PrefabAvailable = true, PrefabName = "Musket.Service",
                HasVisualParameters = true, ModelName = "Musket.Service",
                ModelIsVariantPrefab = true
            });
            FirearmVisualRigAudit replaced = FirearmVisualRigAudit.Evaluate(
                ProjectItemRosters.FirearmItemVisuals, items);
            Assertions.True(replaced.Census.CountMatches,
                "The replacement changed the count.");
            Assertions.False(replaced.Roster.Passed,
                "A same-count replacement passed the census.");
            Contains(replaced.Roster.Observed,
                "count=match;membership=MISMATCH;missing=KMG.Firearms.MusketPlus1Item;unexpected=KMG.Firearms.MusketPlus2Item",
                "The replacement is not reported as one missing and one unexpected identity.");
            // The unexpected item was still inspected, and the missing one is
            // a binding failure of its own.
            Assertions.True(replaced.Bindings.Single(value => value.Symbol ==
                "KMG.Firearms.MusketPlus2Item").Bound,
                "The unexpected item was not inspected.");
            Contains(replaced.Bindings.Single(value => value.Symbol ==
                "KMG.Firearms.MusketPlus1Item").Failure, "not observed",
                "The missing item is not a binding failure.");
            Assertions.False(replaced.BindingOutcome.Passed,
                "A roster with an unbound expected item passed its bindings.");

            // An identity observed twice, and one more than expected.
            List<FirearmVisualBindingObservation> doubled = BoundRoster();
            doubled.Add(BoundItem(ProjectItemRosters.FirearmItemVisuals[1]));
            FirearmVisualRigAudit twice = FirearmVisualRigAudit.Evaluate(
                ProjectItemRosters.FirearmItemVisuals, doubled);
            Contains(twice.Roster.Observed,
                "expected=16;observed=17;count=MISMATCH;membership=MISMATCH;observedMoreThanOnce=KMG.Firearms.EarlyPistolItem x2",
                "A duplicate is not reported.");
            Assertions.Equal(17, twice.Bindings.Count,
                "The duplicate was not inspected as well.");
        }

        internal static void FirearmVisualsUnreadableAndUnmappedInputsAreReported()
        {
            List<FirearmVisualBindingObservation> items = BoundRoster();
            // An item that could not be read, an item the game does not have,
            // a symbol with no approved variant, a kind outside the mapped
            // family, and a source that could not be read at all.
            Find(items, "KMG.Firearms.EarlyMusketItem").Fault =
                "production firearms unavailable: catalog was not registered";
            Find(items, "KMG.Firearms.AdvancedRifleItem").ItemPresent = false;
            items.Add(new FirearmVisualBindingObservation
            {
                Symbol = "KMG.Firearms.NotAFirearmItem", Kind = FirearmKind.Pistol,
                ItemPresent = true
            });
            Find(items, "KMG.Firearms.AdvancedRevolverItem").Kind = FirearmKind.Pistol;
            items.Add(new FirearmVisualBindingObservation
            {
                Fault = "magic firearm catalog unavailable: null"
            });
            FirearmVisualRigAudit audit = FirearmVisualRigAudit.Evaluate(
                ProjectItemRosters.FirearmItemVisuals, items);

            Func<string, string> failure = symbol => audit.Bindings.Single(
                value => value.Symbol == symbol).Failure;
            Contains(failure("KMG.Firearms.EarlyMusketItem"),
                "could not be read: production firearms unavailable",
                "An unreadable item's fault is not reported.");
            Contains(failure("KMG.Firearms.AdvancedRifleItem"), "has no item",
                "An absent item is not reported.");
            Contains(failure("KMG.Firearms.NotAFirearmItem"),
                "no approved visual variant", "An unmapped symbol is not reported.");
            Contains(failure("KMG.Firearms.AdvancedRevolverItem"),
                "outside the Pistol family",
                "A mapping across the family boundary is not reported.");
            Contains(audit.Roster.Observed, "malformedObserved=",
                "The unreadable source is not in the census.");
            Contains(audit.Roster.Observed, "magic firearm catalog unavailable",
                "The unreadable source's reason is not in the census.");
            // The thirteen untouched items were still inspected and are bound.
            Assertions.Equal(13, audit.Bindings.Count(value => value.Bound),
                "The sound items were not all inspected: " +
                audit.BindingOutcome.Observed);
            Assertions.True(FirearmVisualRigAudit.Inspect(null).Failure != null,
                "A missing observation was treated as bound.");
        }

        // ---- project item presentation ----

        private static List<AuditIdentity> Expected(IEnumerable<string> symbols)
        {
            // Stand-in keys: the scenario uses each symbol's manifest GUID.
            return symbols.Select(symbol => new AuditIdentity("guid:" + symbol,
                symbol)).ToList();
        }

        private static List<AuditIdentity> Observed(IEnumerable<string> symbols)
        {
            return symbols.Select(symbol => new AuditIdentity("guid:" + symbol,
                symbol.Replace('.', '_') + "(guid)")).ToList();
        }

        private static PolicyEnchantmentObservation BlankPolicy()
        {
            return new PolicyEnchantmentObservation
            {
                BlueprintAvailable = true, AmongWeaponEnchantments = true,
                InternalName = "KMG_EasternWeapons_ProficiencyPolicyEnchantment",
                Name = string.Empty, Description = string.Empty
            };
        }

        private static ProjectItemPresentationAudit Presentation(
            IEnumerable<string> weapons, IEnumerable<string> items,
            IEnumerable<string> enchantments, PolicyEnchantmentObservation policy,
            params string[] issues)
        {
            return ProjectItemPresentationAudit.Evaluate(
                IdentityCensus.Compare(Expected(ProjectItemRosters.PresentationWeapons),
                    Observed(weapons)),
                IdentityCensus.Compare(Expected(ProjectItemRosters.PresentationItems
                    .Select(value => value.Key)), Observed(items)),
                IdentityCensus.Compare(Expected(
                    ProjectItemRosters.CustomWeaponEnchantments), Observed(enchantments)),
                policy, issues);
        }

        private static string[] Items()
        {
            return ProjectItemRosters.PresentationItems.Select(value => value.Key)
                .ToArray();
        }

        internal static void ItemPresentationValidRosterPasses()
        {
            ProjectItemPresentationAudit audit = Presentation(
                ProjectItemRosters.PresentationWeapons, Items(),
                ProjectItemRosters.CustomWeaponEnchantments, BlankPolicy());
            Assertions.True(audit.Passed && audit.Outcomes.All(value => value.Passed),
                "The supported rosters with clean text did not pass: " +
                audit.Aggregate.Observed);
            Assertions.True(audit.Outcomes.Select(value => value.Name).SequenceEqual(
                new[] { ProjectItemPresentationAudit.WeaponCensusName,
                    ProjectItemPresentationAudit.ItemCensusName,
                    ProjectItemPresentationAudit.EnchantmentCensusName,
                    ProjectItemPresentationAudit.PolicyName,
                    ProjectItemPresentationAudit.TextName }),
                "The five results are not reported under their own names.");
            Contains(audit.Outcome(ProjectItemPresentationAudit.WeaponCensusName).Observed,
                "expected=57;observed=57;count=match;membership=match",
                "The weapon census does not report 57 of 57.");
            Contains(audit.Outcome(ProjectItemPresentationAudit.ItemCensusName).Observed,
                "expected=58;observed=58", "The item census does not report 58 of 58.");
            Contains(audit.Outcome(ProjectItemPresentationAudit.EnchantmentCensusName)
                .Observed, "expected=12;observed=12",
                "The enchantment census does not report 12 of 12.");
            Contains(audit.Aggregate.Observed,
                "weapons=57/57;items=58/58;customEnchantments=12/12;issues=<none>;failed=<none>",
                "The aggregate does not summarize the parts.");
            Assertions.Equal(ProjectItemPresentationAudit.AggregateName,
                audit.Aggregate.Name, "The aggregate lost its established name.");
        }

        internal static void ItemPresentationCountMismatchDoesNotHidePolicyOrMembership()
        {
            // The stale-pin situation and a real defect at once: two weapons
            // fewer than declared, and the policy enchantment not on any weapon.
            string[] weapons = ProjectItemRosters.PresentationWeapons.Where(value =>
                value != "KMG.Firearms.RoadwardenItem" &&
                value != "KMG.Firearms.DeadReckoningItem").ToArray();
            string[] items = Items().Where(value =>
                value != "KMG.Firearms.RoadwardenItem" &&
                value != "KMG.Firearms.DeadReckoningItem").ToArray();
            string[] enchantments = ProjectItemRosters.CustomWeaponEnchantments
                .Where(value => value != ProjectItemRosters.PolicyOnlyEnchantmentSymbol)
                .ToArray();
            PolicyEnchantmentObservation policy = BlankPolicy();
            policy.AmongWeaponEnchantments = false;
            ProjectItemPresentationAudit audit = Presentation(weapons, items,
                enchantments, policy);

            AuditOutcome weaponCensus = audit.Outcome(
                ProjectItemPresentationAudit.WeaponCensusName);
            Assertions.False(weaponCensus.Passed, "55 of 57 weapons passed.");
            Contains(weaponCensus.Observed,
                "expected=57;observed=55;count=MISMATCH;membership=MISMATCH;missing=KMG.Firearms.RoadwardenItem|KMG.Firearms.DeadReckoningItem",
                "The weapon census does not name the two missing weapons.");
            Assertions.False(audit.Outcome(ProjectItemPresentationAudit.ItemCensusName)
                .Passed, "56 of 58 items passed.");

            // Neither count stopped the policy and membership results.
            AuditOutcome policyOutcome = audit.Outcome(
                ProjectItemPresentationAudit.PolicyName);
            Assertions.False(policyOutcome.Passed,
                "A policy enchantment that is on no weapon passed.");
            Contains(policyOutcome.Observed, "amongWeaponEnchantments=False",
                "The policy result does not say it is absent.");
            Contains(policyOutcome.Observed,
                "not among the custom enchantments on the weapons or their types",
                "The policy result does not give its reason.");
            AuditOutcome enchantmentCensus = audit.Outcome(
                ProjectItemPresentationAudit.EnchantmentCensusName);
            Assertions.False(enchantmentCensus.Passed, "11 of 12 enchantments passed.");
            Contains(enchantmentCensus.Observed,
                "missing=" + ProjectItemRosters.PolicyOnlyEnchantmentSymbol,
                "The enchantment census does not name the missing enchantment.");
            Assertions.True(audit.Outcome(ProjectItemPresentationAudit.TextName).Passed,
                "Clean text was reported as failing.");
            Contains(audit.Aggregate.Observed,
                "failed=project-item-weapon-census|project-item-census|project-custom-enchantment-census|project-policy-enchantment",
                "The aggregate does not name each failed part.");
            Assertions.False(audit.Passed, "The aggregate passed.");

            // The policy enchantment present but with visible text.
            PolicyEnchantmentObservation visible = BlankPolicy();
            visible.Name = "Proficiency Policy";
            visible.Description = "Internal.";
            AuditOutcome visibleOutcome = Presentation(
                ProjectItemRosters.PresentationWeapons, Items(),
                ProjectItemRosters.CustomWeaponEnchantments, visible)
                .Outcome(ProjectItemPresentationAudit.PolicyName);
            Assertions.False(visibleOutcome.Passed, "Visible policy text passed.");
            Contains(visibleOutcome.Observed, "its name is visible: Proficiency Policy",
                "The visible name is not reported.");
            Contains(visibleOutcome.Observed, "its description is visible: Internal.",
                "The visible description is not reported.");
            Assertions.False(Presentation(ProjectItemRosters.PresentationWeapons,
                Items(), ProjectItemRosters.CustomWeaponEnchantments, null)
                .Outcome(ProjectItemPresentationAudit.PolicyName).Passed,
                "An unread policy enchantment passed.");
        }

        internal static void ItemPresentationTextFailuresStayVisibleWhenCensusFails()
        {
            string[] weapons = ProjectItemRosters.PresentationWeapons.Take(55).ToArray();
            ProjectItemPresentationAudit audit = Presentation(weapons,
                weapons.Concat(new[] { ProjectItemRosters.CordSymbol }),
                ProjectItemRosters.CustomWeaponEnchantments, BlankPolicy(),
                "KMG_Katana_WinterReed_Item:flavor=blank",
                "KMG_FallingPetal_Enchantment:description=internal-token");
            Assertions.False(audit.Outcome(
                ProjectItemPresentationAudit.WeaponCensusName).Passed,
                "A short weapon census passed.");
            AuditOutcome text = audit.Outcome(ProjectItemPresentationAudit.TextName);
            Assertions.False(text.Passed, "Text issues passed.");
            Contains(text.Observed,
                "issues=KMG_Katana_WinterReed_Item:flavor=blank|KMG_FallingPetal_Enchantment:description=internal-token",
                "The individual text issues are not reported beside the census failure.");
            Assertions.True(audit.Outcome(ProjectItemPresentationAudit.PolicyName)
                .Passed && audit.Outcome(
                    ProjectItemPresentationAudit.EnchantmentCensusName).Passed,
                "Unrelated results were dragged down by the census or the text.");
            Contains(audit.Aggregate.Observed,
                "failed=project-item-weapon-census|project-item-census|project-item-player-facing-text",
                "The aggregate does not name the census and the text together.");

            // Text alone fails the aggregate with every census intact.
            ProjectItemPresentationAudit textOnly = Presentation(
                ProjectItemRosters.PresentationWeapons, Items(),
                ProjectItemRosters.CustomWeaponEnchantments, BlankPolicy(),
                "KMG_CordOfStubbornResolve_Item:name=blank");
            Assertions.False(textOnly.Passed, "A text issue alone passed the aggregate.");
            Contains(textOnly.Aggregate.Observed,
                "failed=project-item-player-facing-text",
                "A text issue alone is not the only failed part.");
        }

        internal static void ItemPresentationReplacementDuplicatesAndUnresolved()
        {
            // A replacement that keeps 57 and 58: a declared weapon is gone
            // and a weapon outside the roster is there.
            string[] weapons = ProjectItemRosters.PresentationWeapons.Select(value =>
                value == "KMG.EasternWeapons.Katana.HeavensMeasure"
                    ? "KMG.EasternWeapons.Katana.Plus2Item" : value).ToArray();
            ProjectItemPresentationAudit replaced = Presentation(weapons,
                weapons.Concat(new[] { ProjectItemRosters.CordSymbol }),
                ProjectItemRosters.CustomWeaponEnchantments, BlankPolicy());
            AuditOutcome census = replaced.Outcome(
                ProjectItemPresentationAudit.WeaponCensusName);
            Assertions.False(census.Passed, "A same-count replacement passed.");
            Contains(census.Observed,
                "expected=57;observed=57;count=match;membership=MISMATCH;missing=KMG.EasternWeapons.Katana.HeavensMeasure;unexpected=KMG_EasternWeapons_Katana_Plus2Item(guid)",
                "The replacement is not reported as one missing and one unexpected weapon.");

            // The Cord missing, a weapon listed twice, and an extra enchantment.
            ProjectItemPresentationAudit mixed = Presentation(
                ProjectItemRosters.PresentationWeapons.Concat(new[] {
                    "KMG.Firearms.EarlyPistolItem" }),
                ProjectItemRosters.PresentationWeapons,
                ProjectItemRosters.CustomWeaponEnchantments.Concat(new[] {
                    "KMG.Firearms.BatteredOriginEnchantment" }), BlankPolicy());
            Contains(mixed.Outcome(ProjectItemPresentationAudit.WeaponCensusName).Observed,
                "observedMoreThanOnce=KMG.Firearms.EarlyPistolItem x2",
                "A weapon read twice is not reported.");
            Contains(mixed.Outcome(ProjectItemPresentationAudit.ItemCensusName).Observed,
                "expected=58;observed=57;count=MISMATCH;membership=MISMATCH;missing=" +
                    ProjectItemRosters.CordSymbol,
                "The missing Cord is not reported.");
            Contains(mixed.Outcome(ProjectItemPresentationAudit.EnchantmentCensusName)
                .Observed, "expected=12;observed=13;count=MISMATCH;membership=MISMATCH;unexpected=KMG_Firearms_BatteredOriginEnchantment(guid)",
                "An extra enchantment is not reported.");

            // An expected identity the manifest could not resolve, and an
            // observed entry with no identity, are both reported and neither
            // is silently dropped.
            List<AuditIdentity> expected = Expected(new[] { "A", "B" });
            expected.Add(new AuditIdentity(null, "C", "not present in the deployed manifest"));
            List<AuditIdentity> observed = Observed(new[] { "A", "B" });
            observed.Add(new AuditIdentity(null, "named Eastern weapons",
                "catalog unavailable: null"));
            IdentityCensus unresolved = IdentityCensus.Compare(expected, observed);
            Assertions.True(unresolved.CountMatches && !unresolved.Exact,
                "An unresolved expectation and a malformed observation cancelled out.");
            Contains(unresolved.Describe(),
                "unresolvedExpected=C(not present in the deployed manifest);malformedObserved=named Eastern weapons(catalog unavailable: null)",
                "The unresolved and malformed identities are not reported.");
            Assertions.Throws<ArgumentNullException>(() =>
                IdentityCensus.Compare(null, observed), "A missing expectation was accepted.");
        }
    }
}
