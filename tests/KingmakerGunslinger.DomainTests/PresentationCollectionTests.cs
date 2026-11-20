using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.EasternWeapons;
using KingmakerGunslinger.ElvenBranchedSpear;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Presentation;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The collection the two presentation scenarios run before they
    /// evaluate anything: the production routines that read catalog entries,
    /// driven here with stand-in entries and items in place of live
    /// blueprints. Each test starts from raw catalog entries, some of them
    /// malformed, and reads what was collected and what was then reported.
    /// </summary>
    internal static class PresentationCollectionTests
    {
        private sealed class Spec
        {
            internal string Symbol;
            internal FirearmKind Kind;
            internal bool KindThrows;

            internal FirearmKind ReadKind()
            {
                if (KindThrows) throw new InvalidOperationException("kind was never set");
                return Kind;
            }
        }

        private sealed class Item
        {
            internal string Guid;
            internal string Name;
            internal string Model;
            internal string DisplayName;
            internal bool IdentityThrows;
        }

        private sealed class Entry
        {
            internal Spec Spec;
            internal Item Item;
            internal bool SpecThrows;
            internal bool ItemThrows;

            internal Spec ReadSpec()
            {
                if (SpecThrows) throw new InvalidOperationException("spec was disposed");
                return Spec;
            }

            internal Item ReadItem()
            {
                if (ItemThrows) throw new InvalidOperationException("item was unloaded");
                return Item;
            }
        }

        private static void Contains(string text, string fragment, string message)
        {
            Assertions.True(text != null && text.Contains(fragment),
                message + " Reported: " + text);
        }

        private static void Absent(string text, string fragment, string message)
        {
            Assertions.False(text != null && text.Contains(fragment),
                message + " Reported: " + text);
        }

        // ---- firearm item visuals ----

        // What the scenario's reader reports for one item, with the stand-in
        // item's model in place of a live prefab reference.
        private static FirearmVisualBindingObservation Observe(Item item,
            string symbol, FirearmKind kind)
        {
            var observation = new FirearmVisualBindingObservation
            {
                Symbol = symbol, Kind = kind, ItemPresent = item != null,
                ItemName = item == null ? null : item.Name
            };
            string variant;
            if (string.IsNullOrEmpty(symbol) ||
                !WeaponVisualVariantCatalog.TryGet(symbol, out variant))
            {
                observation.ModelName = item == null ? null : item.Model;
                return observation;
            }
            observation.PrefabAvailable = true;
            observation.PrefabName = variant;
            if (item == null) return observation;
            observation.HasVisualParameters = true;
            observation.ModelName = item.Model;
            observation.ModelIsVariantPrefab = item.Model == variant;
            return observation;
        }

        private static Item BoundFirearm(FirearmVisualRosterEntry entry)
        {
            return new Item
            {
                Name = entry.Symbol.Replace('.', '_'),
                Model = WeaponVisualVariantCatalog.Require(entry.Symbol)
            };
        }

        private static List<Entry> MagicEntries()
        {
            return ProjectItemRosters.MagicFirearms.Select(entry => new Entry
            {
                Spec = new Spec { Symbol = entry.Symbol, Kind = entry.Kind },
                Item = BoundFirearm(entry)
            }).ToList();
        }

        // The scenario's collection: the Test Musket and the production
        // firearms one by one, then the magic firearm catalog entry by entry.
        private static FirearmVisualRigAudit CollectFirearms(
            Func<IEnumerable<Entry>> magic,
            out List<FirearmVisualBindingObservation> observed)
        {
            observed = new List<FirearmVisualBindingObservation>();
            foreach (FirearmVisualRosterEntry entry in
                ProjectItemRosters.FirearmItemVisuals.Take(6))
            {
                FirearmVisualRosterEntry current = entry;
                observed.Add(FirearmVisualCollection.ReadItem(current.Source,
                    current.Symbol, current.Kind, () => BoundFirearm(current),
                    (Func<Item, string, FirearmKind,
                        FirearmVisualBindingObservation>)Observe));
            }
            FirearmVisualCollection.ReadEntries(observed, "magic firearms", magic,
                (Entry entry) => entry.ReadSpec(), (Spec spec) => spec.Symbol,
                (Spec spec) => spec.ReadKind(), (Entry entry) => entry.ReadItem(),
                (Func<Item, string, FirearmKind,
                    FirearmVisualBindingObservation>)Observe);
            return FirearmVisualRigAudit.Evaluate(
                ProjectItemRosters.FirearmItemVisuals, observed);
        }

        private static FirearmVisualBindingResult Binding(
            FirearmVisualRigAudit audit, string symbol)
        {
            return audit.Bindings.Single(value => value.Symbol == symbol);
        }

        internal static void FirearmEntriesHealthyCatalogCollectsEverything()
        {
            List<FirearmVisualBindingObservation> observed;
            List<Entry> magic = MagicEntries();
            FirearmVisualRigAudit audit = CollectFirearms(() => magic, out observed);
            Assertions.Equal(16, observed.Count, "Not every item was collected.");
            Assertions.True(observed.Skip(6).Select(value => value.EntryIndex)
                .SequenceEqual(Enumerable.Range(0, 10).Select(value => (int?)value)),
                "The catalog entries are not located by their positions.");
            Assertions.True(audit.Passed, "A healthy catalog did not pass: " +
                audit.Roster.Observed + " / " + audit.BindingOutcome.Observed);
            Contains(audit.Roster.Observed,
                "expected=16;observed=16;count=match;membership=match",
                "The census of a healthy catalog is not sixteen of sixteen.");
            Absent(audit.Roster.Observed, "malformedObserved",
                "A healthy catalog reports a malformed entry.");
            Absent(audit.Roster.Observed, "sourceFaults",
                "A healthy catalog reports a source fault.");
        }

        internal static void FirearmEntriesMalformedEntryDoesNotHideLaterBinding()
        {
            // A valid entry, then a null entry, then a readable item bound to
            // the wrong model; further on, an entry with no Spec whose item
            // can still be read.
            List<Entry> magic = MagicEntries();
            magic[1] = null;
            magic[2].Item.Model = "KMG_Firearm_Pistol_Service";
            string orphanItem = magic[4].Item.Name;
            magic[4].Spec = null;
            List<FirearmVisualBindingObservation> observed;
            FirearmVisualRigAudit audit = CollectFirearms(() => magic, out observed);

            // Every position was collected; nothing after the null entry was lost.
            Assertions.Equal(16, observed.Count,
                "Collection stopped at the malformed entry.");
            Assertions.True(Binding(audit, "KMG.Firearms.PistolPlus1Item").Bound,
                "The valid entry before the malformed one did not survive.");

            // The null entry and the entry without a Spec are identified by
            // where they are, and no identity is invented for either.
            FirearmVisualBindingObservation nullEntry = observed[7];
            Assertions.True(nullEntry.Malformed && nullEntry.Symbol == null &&
                nullEntry.Source == "magic firearms" && nullEntry.EntryIndex == 1,
                "The null entry is not recorded as malformed at its position.");
            Contains(Binding(audit, "magic firearms[1]").Failure,
                "malformed entry: the entry is null",
                "The null entry's reason is not reported.");
            FirearmVisualBindingObservation noSpec = observed[10];
            Assertions.True(noSpec.Malformed && noSpec.Symbol == null &&
                noSpec.EntryIndex == 4 && noSpec.ItemPresent &&
                noSpec.ItemName == orphanItem,
                "What could be read of the entry without a Spec was not retained.");
            Contains(Binding(audit, "magic firearms[4]").Failure,
                "malformed entry: the entry has no Spec; item read as " + orphanItem,
                "The missing Spec and the item that was read are not both reported.");

            // The later wrong model is reported with its real details.
            FirearmVisualBindingResult wrong = Binding(audit,
                "KMG.Firearms.BlunderbussPlus1Item");
            Assertions.False(wrong.Bound, "The wrong model was reported as bound.");
            Assertions.Equal("Blunderbuss.Service", wrong.Variant,
                "The wrongly bound item's approved variant is not reported.");
            Contains(wrong.Failure, "the item's model is KMG_Firearm_Pistol_Service",
                "The observed model is not in the failure.");
            Absent(wrong.Failure, "not observed",
                "The later item's failure was reduced to not observed.");

            // Every other entry after the malformed ones was inspected and is bound.
            foreach (int index in new[] { 3, 5, 6, 7, 8, 9 })
                Assertions.True(Binding(audit,
                    ProjectItemRosters.MagicFirearms[index].Symbol).Bound,
                    "The entry at " + index + " was not inspected.");

            // The census reports the two malformed entries as that, the two
            // identities nothing supplied as missing, and no source fault.
            Assertions.False(audit.Roster.Passed, "Malformed entries passed the census.");
            Assertions.True(audit.Census.MalformedObserved.SequenceEqual(new[] {
                "magic firearms[1](the entry is null)",
                "magic firearms[4](the entry has no Spec)" }),
                "The malformed entries are not located: " + audit.Roster.Observed);
            Assertions.True(audit.Census.Missing.SequenceEqual(new[] {
                "KMG.Firearms.MusketPlus1Item",
                "KMG.Firearms.RiverKingsMeasureItem" }),
                "The identities no entry supplied are not reported as missing: " +
                audit.Roster.Observed);
            Assertions.Equal(0, audit.Census.SourceFaults.Count,
                "A malformed entry was reported as a source fault.");
            Assertions.Equal(16, audit.Census.ObservedCount,
                "A malformed entry was not counted as an entry that was there.");
            Contains(audit.BindingOutcome.Observed, ";failed=5;",
                "Not exactly the two malformed entries, the wrong model and the two unsupplied identities failed.");
        }

        internal static void FirearmEntriesPartialMetadataAndItemFaults()
        {
            // An unreadable Spec, a Spec with no symbol, a Spec whose kind
            // cannot be read, and a sound entry whose item cannot be read.
            List<Entry> magic = MagicEntries();
            magic[0].SpecThrows = true;
            magic[3].Spec.Symbol = string.Empty;
            magic[5].Spec.KindThrows = true;
            magic[6].ItemThrows = true;
            magic[9].Item.Model = "CrossbowHeavy";
            List<FirearmVisualBindingObservation> observed;
            FirearmVisualRigAudit audit = CollectFirearms(() => magic, out observed);

            Assertions.Equal(16, observed.Count, "Collection did not reach the end.");
            Contains(Binding(audit, "magic firearms[0]").Failure,
                "the entry's Spec could not be read: InvalidOperationException: spec was disposed",
                "An unreadable Spec is not reported with its reason.");
            Contains(Binding(audit, "magic firearms[3]").Failure,
                "the Spec has no symbol",
                "A Spec without a symbol is not reported.");
            // The symbol that was read is kept even though the kind was not.
            FirearmVisualBindingResult noKind = Binding(audit,
                "KMG.Firearms.IrovettisOvationItem");
            Contains(noKind.Failure,
                "malformed entry: the Spec's kind could not be read: InvalidOperationException: kind was never set",
                "An unreadable kind is not reported against the symbol that was read.");
            Assertions.Equal(WeaponVisualVariantCatalog.Require(
                "KMG.Firearms.IrovettisOvationItem"), noKind.Variant,
                "The symbol that was read lost its mapping.");
            Assertions.True(audit.Census.Missing.SequenceEqual(new[] {
                "KMG.Firearms.PistolPlus1Item",
                "KMG.Firearms.DuelistsRebuttalItem" }),
                "Only the two entries whose symbol was not read should be missing: " +
                audit.Roster.Observed);

            // A sound entry whose item cannot be read keeps its identity.
            FirearmVisualBindingResult unread = Binding(audit,
                "KMG.Firearms.TheLastWordItem");
            Contains(unread.Failure,
                "the item could not be read: InvalidOperationException: item was unloaded",
                "An unreadable item is not reported against its own symbol.");
            Assertions.Equal("Pistol.LastWord", unread.Variant,
                "The unreadable item lost its approved variant.");

            // The last entry's real failure is still there after all four.
            Contains(Binding(audit, ProjectItemRosters.MagicFirearms[9].Symbol)
                .Failure, "CrossbowHeavy",
                "The last entry's wrong model is not reported.");
            Assertions.True(new[] { 1, 2, 4, 7, 8 }.All(index => Binding(audit,
                ProjectItemRosters.MagicFirearms[index].Symbol).Bound),
                "A sound entry between the faulted ones was not inspected.");
        }

        private static IEnumerable<Entry> StopsAfter(List<Entry> entries, int count)
        {
            for (int index = 0; index < count; index++) yield return entries[index];
            throw new InvalidOperationException("collection was modified");
        }

        internal static void FirearmEntriesSourceFaultsAreNotEntryFaults()
        {
            // The catalog cannot be obtained at all.
            List<FirearmVisualBindingObservation> observed;
            FirearmVisualRigAudit unavailable = CollectFirearms(delegate
            {
                throw new NullReferenceException("catalog was not registered");
            }, out observed);
            Assertions.Equal(7, observed.Count,
                "The six single items and the one source fault were not collected.");
            Assertions.True(observed[6].SourceUnavailable && !observed[6].Malformed,
                "An unobtainable catalog was recorded as a malformed entry.");
            Assertions.True(unavailable.Census.SourceFaults.SequenceEqual(new[] {
                "magic firearms(NullReferenceException: catalog was not registered)" }),
                "The source fault is not reported as one: " +
                unavailable.Roster.Observed);
            Assertions.Equal(0, unavailable.Census.MalformedObserved.Count,
                "A source fault was reported as a malformed entry.");
            Assertions.Equal(6, unavailable.Census.ObservedCount,
                "A source fault was counted as an observed identity.");
            Assertions.Equal(10, unavailable.Census.Missing.Count,
                "The ten identities the catalog should supply are not missing.");
            // The other sources were still read and inspected.
            Assertions.Equal(6, unavailable.Bindings.Count(value => value.Bound),
                "The items outside the catalog were not inspected.");
            Contains(unavailable.BindingOutcome.Observed,
                ";sourceFaults=magic firearms(NullReferenceException: catalog was not registered)",
                "The binding outcome does not say why ten items were not observed.");
            Assertions.False(unavailable.BindingOutcome.Passed,
                "An unobtainable catalog passed the bindings.");

            FirearmVisualRigAudit absent = CollectFirearms(() => null, out observed);
            Assertions.True(absent.Census.SourceFaults.SequenceEqual(new[] {
                "magic firearms(the source is null)" }),
                "A null catalog is not reported as a source fault.");

            // The catalog stops part-way: what was read before the stop is
            // kept, with its real binding results.
            List<Entry> magic = MagicEntries();
            magic[1].Item.Model = "KMG_Firearm_Pistol_Service";
            FirearmVisualRigAudit stopped = CollectFirearms(
                () => StopsAfter(magic, 3), out observed);
            Assertions.Equal(10, observed.Count,
                "The three entries read before the stop were not retained.");
            Assertions.True(Binding(stopped, "KMG.Firearms.PistolPlus1Item").Bound &&
                Binding(stopped, "KMG.Firearms.BlunderbussPlus1Item").Bound,
                "Entries read before the stop lost their results.");
            Contains(Binding(stopped, "KMG.Firearms.MusketPlus1Item").Failure,
                "the item's model is KMG_Firearm_Pistol_Service",
                "A binding failure read before the stop is not reported.");
            Assertions.True(stopped.Census.SourceFaults.SequenceEqual(new[] {
                "magic firearms(enumeration stopped after 3 entries: InvalidOperationException: collection was modified)" }),
                "The stop is not reported with how far it got: " +
                stopped.Roster.Observed);
            Assertions.Equal(7, stopped.Census.Missing.Count,
                "The seven identities after the stop are not missing.");
            Contains(Binding(stopped, "KMG.Firearms.TheLastWordItem").Failure,
                "not observed", "An identity after the stop is not reported as unobserved.");
        }

        // ---- project item presentation ----

        private const string CleanName = "Steel Blade";

        private static List<Entry> WeaponEntries(IEnumerable<string> symbols)
        {
            return symbols.Select(symbol => new Entry
            {
                Item = new Item
                {
                    Guid = "guid:" + symbol, Name = symbol.Replace('.', '_'),
                    DisplayName = CleanName
                }
            }).ToList();
        }

        private static AuditIdentity Identify(Item item)
        {
            if (item.IdentityThrows)
                throw new InvalidOperationException("asset guid unavailable");
            return new AuditIdentity(item.Guid, item.Name + "(guid)");
        }

        // The scenario's six sources, in its order, each with its own entries.
        private sealed class Sources
        {
            internal Func<IEnumerable<Entry>> Production, Magic, Eastern,
                NamedEastern, Spears, NamedSpears;

            internal static Sources Declared()
            {
                List<Entry> production = WeaponEntries(
                    ProjectItemRosters.PresentationWeapons.Take(5));
                List<Entry> magic = WeaponEntries(
                    ProjectItemRosters.MagicFirearms.Select(value => value.Symbol));
                List<Entry> eastern = WeaponEntries(
                    EasternWeaponCatalog.AllGenericItems.Select(value => value.Symbol));
                List<Entry> namedEastern = WeaponEntries(
                    EasternWeaponNamedCatalog.All.Select(value => value.Symbol));
                List<Entry> spears = WeaponEntries(
                    ElvenBranchedSpearCatalog.All.Select(value => value.Symbol));
                List<Entry> namedSpears = WeaponEntries(
                    ElvenBranchedSpearNamedCatalog.All.Select(value => value.Symbol));
                return new Sources
                {
                    Production = () => production, Magic = () => magic,
                    Eastern = () => eastern, NamedEastern = () => namedEastern,
                    Spears = () => spears, NamedSpears = () => namedSpears
                };
            }

            internal static List<Entry> Of(Func<IEnumerable<Entry>> source)
            {
                return (List<Entry>)source();
            }
        }

        private static ProjectItemObservations<Item> CollectWeapons(Sources sources)
        {
            var collected = new ProjectItemObservations<Item>();
            Func<Item, AuditIdentity> identify = Identify;
            ProjectItemCollection.ReadSource(collected, "production firearms",
                sources.Production, (Entry entry) => entry.ReadItem(), identify);
            ProjectItemCollection.ReadSource(collected, "magic firearms",
                sources.Magic, (Entry entry) => entry.ReadItem(), identify);
            ProjectItemCollection.ReadSource(collected, "Eastern weapons",
                sources.Eastern, (Entry entry) => entry.ReadItem(), identify);
            ProjectItemCollection.ReadSource(collected, "named Eastern weapons",
                sources.NamedEastern, (Entry entry) => entry.ReadItem(), identify);
            ProjectItemCollection.ReadSource(collected, "Elven branched spears",
                sources.Spears, (Entry entry) => entry.ReadItem(), identify);
            ProjectItemCollection.ReadSource(collected, "named Elven branched spears",
                sources.NamedSpears, (Entry entry) => entry.ReadItem(), identify);
            return collected;
        }

        // The checks that follow collection in the scenario: the text rule on
        // every item that was read, and the censuses against the declared
        // rosters.
        private static ProjectItemPresentationAudit Present(
            ProjectItemObservations<Item> collected)
        {
            var issues = new List<string>();
            foreach (Item item in collected.Items.Distinct())
                PlayerTextAudit.Append(issues, item.Name, "name", item.DisplayName,
                    true);
            List<AuditIdentity> expectedWeapons = ProjectItemRosters
                .PresentationWeapons.Select(symbol => new AuditIdentity(
                    "guid:" + symbol, symbol)).ToList();
            var cord = new AuditIdentity("guid:" + ProjectItemRosters.CordSymbol,
                ProjectItemRosters.CordSymbol);
            string[] enchantments = ProjectItemRosters.CustomWeaponEnchantments;
            return ProjectItemPresentationAudit.Evaluate(
                IdentityCensus.Compare(expectedWeapons, collected.Identities),
                IdentityCensus.Compare(expectedWeapons.Concat(new[] { cord }),
                    collected.Identities.Concat(new[] { cord })),
                IdentityCensus.Compare(enchantments.Select(symbol =>
                        new AuditIdentity(symbol, symbol)),
                    enchantments.Select(symbol => new AuditIdentity(symbol, symbol))),
                new PolicyEnchantmentObservation
                {
                    BlueprintAvailable = true, AmongWeaponEnchantments = true,
                    InternalName = "KMG_EasternWeapons_ProficiencyPolicyEnchantment",
                    Name = string.Empty, Description = string.Empty
                }, issues);
        }

        internal static void ItemSourcesHealthyCatalogsCollectEverything()
        {
            ProjectItemObservations<Item> collected = CollectWeapons(
                Sources.Declared());
            Assertions.Equal(57, collected.Items.Count, "Not every weapon was collected.");
            Assertions.Equal(0, collected.Faults.Count,
                "Healthy sources reported a fault.");
            ProjectItemPresentationAudit audit = Present(collected);
            Assertions.True(audit.Passed, "Healthy sources did not pass: " +
                audit.Aggregate.Observed);
            Contains(audit.Aggregate.Observed,
                "weapons=57/57;items=58/58;customEnchantments=12/12;issues=<none>;failed=<none>",
                "The aggregate of healthy sources is not complete.");
        }

        internal static void ItemSourcesKeepEntriesAroundAMalformedEntry()
        {
            // In one run: a null entry between readable ones, an entry with
            // no item, an entry whose item cannot be read, a later item with
            // blank text, a later item outside the roster, and a source that
            // cannot be obtained with a healthy source after it.
            Sources sources = Sources.Declared();
            List<Entry> production = Sources.Of(sources.Production);
            production[1] = null;
            List<Entry> magic = Sources.Of(sources.Magic);
            magic[3].Item = null;
            magic[8].ItemThrows = true;
            List<Entry> eastern = Sources.Of(sources.Eastern);
            string blank = eastern[5].Item.Name;
            eastern[5].Item.DisplayName = "  ";
            List<Entry> namedEastern = Sources.Of(sources.NamedEastern);
            string replaced = EasternWeaponNamedCatalog.All[2].Symbol;
            namedEastern[2].Item.Guid = "guid:KMG.EasternWeapons.Katana.Plus2Item";
            namedEastern[2].Item.Name = "KMG_EasternWeapons_Katana_Plus2Item";
            sources.Spears = delegate
            {
                throw new NullReferenceException("spear set was not registered");
            };
            ProjectItemObservations<Item> collected = CollectWeapons(sources);

            // The readable entries on both sides of each malformed entry are
            // among the items that go on to the later checks, in order.
            string[] names = collected.Items.Select(value => value.Name).ToArray();
            Assertions.True(names.Take(4).SequenceEqual(new[] {
                "KMG_Firearms_EarlyPistolItem", "KMG_Firearms_EarlyBlunderbussItem",
                "KMG_Firearms_AdvancedRifleItem", "KMG_Firearms_AdvancedRevolverItem" }),
                "The production firearms around the null entry were not retained: " +
                string.Join(",", names.Take(4)));
            Assertions.Equal(57 - 1 - 2 - 6, collected.Items.Count,
                "Not exactly the readable items were retained.");
            Assertions.True(names.Contains(magic[2].Item.Name) &&
                names.Contains(magic[4].Item.Name) && names.Contains(magic[9].Item.Name),
                "A magic firearm beside a malformed entry was not retained.");
            Assertions.True(ElvenBranchedSpearNamedCatalog.All.All(value =>
                names.Contains(value.Symbol.Replace('.', '_'))),
                "The source after the unavailable one was not read.");

            // Entry faults and the source fault are recorded as different things.
            Assertions.True(collected.Faults.Select(value => value.Kind + " " +
                value.Describe()).SequenceEqual(new[] {
                "MalformedEntry production firearms[1](the entry is null)",
                "MalformedEntry magic firearms[3](the entry has no item)",
                "MalformedEntry magic firearms[8](the entry's item could not be read: InvalidOperationException: item was unloaded)",
                "SourceUnavailable Elven branched spears(NullReferenceException: spear set was not registered)" }),
                "The faults are not recorded by kind and place: " + string.Join(" / ",
                    collected.Faults.Select(value => value.Kind + " " +
                        value.Describe()).ToArray()));

            ProjectItemPresentationAudit audit = Present(collected);
            AuditOutcome census = audit.Outcome(
                ProjectItemPresentationAudit.WeaponCensusName);
            Assertions.False(census.Passed, "Malformed sources passed the census.");
            // 57 positions, less the six the unavailable source never gave.
            Contains(census.Observed, "expected=57;observed=51;count=MISMATCH",
                "The observed count is not the positions that were read.");
            Contains(census.Observed,
                ";malformedObserved=production firearms[1](the entry is null)|magic firearms[3](the entry has no item)|magic firearms[8](the entry's item could not be read: InvalidOperationException: item was unloaded)",
                "The malformed entries are not reported by place.");
            Contains(census.Observed,
                ";sourceFaults=Elven branched spears(NullReferenceException: spear set was not registered)",
                "The unavailable source is not reported apart from the entries.");
            // The later membership failure is still visible beside them.
            Contains(census.Observed,
                ";unexpected=KMG_EasternWeapons_Katana_Plus2Item(guid)",
                "The later unexpected weapon is hidden by the malformed entries.");
            IdentityCensus weapons = IdentityCensus.Compare(
                ProjectItemRosters.PresentationWeapons.Select(symbol =>
                    new AuditIdentity("guid:" + symbol, symbol)), collected.Identities);
            Assertions.True(weapons.Missing.SequenceEqual(new[] {
                    "KMG.Firearms.EarlyMusketItem",
                    ProjectItemRosters.MagicFirearms[3].Symbol,
                    ProjectItemRosters.MagicFirearms[8].Symbol, replaced }
                .Concat(ElvenBranchedSpearCatalog.All.Select(value => value.Symbol))),
                "Not exactly the unsupplied identities are missing: " +
                string.Join(",", weapons.Missing.ToArray()));

            // The later text failure is reported in its own assertion.
            AuditOutcome text = audit.Outcome(ProjectItemPresentationAudit.TextName);
            Assertions.False(text.Passed, "The blank name passed.");
            Contains(text.Observed, "issues=" + blank + ":name=blank",
                "The readable item after the malformed entries did not reach its text check.");
            Contains(audit.Aggregate.Observed,
                "failed=project-item-weapon-census|project-item-census|project-item-player-facing-text",
                "The census and text failures are not both in the aggregate.");
        }

        internal static void ItemSourcesEnumerationStopAndUnreadableIdentity()
        {
            // A source that stops after two entries, and in the next source
            // an item whose identity cannot be read.
            Sources sources = Sources.Declared();
            List<Entry> magic = Sources.Of(sources.Magic);
            sources.Magic = () => StopsAfter(magic, 2);
            List<Entry> eastern = Sources.Of(sources.Eastern);
            eastern[0].Item.IdentityThrows = true;
            ProjectItemObservations<Item> collected = CollectWeapons(sources);

            string[] names = collected.Items.Select(value => value.Name).ToArray();
            Assertions.True(names.Skip(5).Take(2).SequenceEqual(new[] {
                magic[0].Item.Name, magic[1].Item.Name }),
                "The two entries read before the stop were not retained.");
            // The item whose identity is unreadable still goes on to its checks.
            Assertions.True(names.Contains(eastern[0].Item.Name),
                "A readable item with an unreadable identity was dropped.");
            Assertions.Equal(57 - 8, collected.Items.Count,
                "The sources after the stop were not read in full.");
            Assertions.True(collected.Faults.Select(value => value.Kind + " " +
                value.Describe()).SequenceEqual(new[] {
                "EnumerationStopped magic firearms(enumeration stopped after 2 entries: InvalidOperationException: collection was modified)",
                "MalformedEntry Eastern weapons[0](the item's identity could not be read: InvalidOperationException: asset guid unavailable)" }),
                "The stop and the unreadable identity are not recorded as what they are: " +
                string.Join(" / ", collected.Faults.Select(value => value.Kind + " " +
                    value.Describe()).ToArray()));

            AuditOutcome census = Present(collected).Outcome(
                ProjectItemPresentationAudit.WeaponCensusName);
            Contains(census.Observed, "expected=57;observed=49;count=MISMATCH",
                "The observed count is not what was read.");
            Contains(census.Observed,
                ";malformedObserved=Eastern weapons[0](the item's identity could not be read: InvalidOperationException: asset guid unavailable)",
                "The unreadable identity is not reported at its place.");
            Contains(census.Observed,
                ";sourceFaults=magic firearms(enumeration stopped after 2 entries: InvalidOperationException: collection was modified)",
                "The stop is not reported as a source fault with how far it got.");

            // The reader itself never raises and keeps one field's failure
            // to that field.
            SourceRead<Entry> none = SourceReader.Read<Entry>("nothing", null);
            Assertions.True(none.Slots.Count == 0 && none.SourceFault != null &&
                none.SourceFault.Kind == SourceFaultKind.SourceUnavailable &&
                !none.SourceFault.AboutEntry,
                "A missing source is not a source fault.");
            string nothing;
            string nothingFault;
            Assertions.True(SourceReader.TryRead(() => (string)null, out nothing,
                out nothingFault) && nothingFault == null,
                "Reading a null value was treated as a failure to read.");
        }
    }
}
