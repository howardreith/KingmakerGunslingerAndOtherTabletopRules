using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Common;
using Kingmaker.UI.Group;
using Kingmaker.UI.Tooltip;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// S5 T5 / POL-06, in the cleric phase's native Default fixture (or its
    /// narrow <c>summon-pol06-cotw</c> lane, which keeps the same setup and
    /// cleanup and skips the unrelated earlier cells): Call of the Wild's
    /// metamagic conversion rows (its "metamagic for spontaneous spell
    /// conversion" setting) projected through the creature menu.
    /// <para>Every popup verdict is independent of the projection under test.
    /// The expected conversions come from Call of the Wild's own row builder;
    /// each conversion's tier from the book's native conversion lists and its
    /// quantity from its own spawn expression; the creatures each conversion
    /// should own from the placement, native-option, third-party and Spider
    /// Summoner contracts, cross-checked with each creature's own spawn
    /// expression. The product's receipts are never the expectation.</para>
    /// <list type="bullet">
    /// <item>Herald (fixture cleric 20) over an unrelated level 4 spell prepared
    /// with Extend Spell in a level 5 slot: popup, a Single cast (the Black
    /// Widow), a stale retained choice and a forbidden creature refused, the
    /// setting off; then a controlled d4+1 cast; then a second metamagic feat
    /// (Heighten where installed and accepted) with its popup size, reachability,
    /// tooltips and a cast;</item>
    /// <item>Druid over the same kind of source: popup, a controlled d3 cast,
    /// and a retained choice refused after its source slot is spent;</item>
    /// <item>the natural cleric/wizard Herald over a plain level 5 source with
    /// no metamagic, its wizard book populated: popup and a projected Extend
    /// cast, exactly one cleric slot spent and the wizard book unchanged.</item>
    /// </list>
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const string Pol06Projected = "pol06-cotw-additional-projected";
        private const string Pol06Raw = "pol06-cotw-additional-raw";
        /// <summary>Superior Summoning adds to the ProjectilesCount rank; the
        /// controlled-count casters must not hold it.</summary>
        private const string Pol06SuperiorSummoning = "0477936c0f74841498b5c8753a8062a3";
        /// <summary>The controlled value of the one quantity die of a d3 or d4+1 cast.</summary>
        private const int Pol06ForcedDie = 2;

        /// <summary>True for the narrow POL-06 lane of the cleric phase.</summary>
        private bool _pol06Lane;

        /// <summary>Classifies a popup row against one prepared source: a child
        /// projected from a Call of the Wild metamagic conversion row, such a row
        /// left raw, or neither (null).</summary>
        private static Func<AbilityData, string> Pol06Classify(AbilityData source)
        {
            return row =>
            {
                if (row == null || row.Blueprint == null) return null;
                AbilityData conversion = row.ConvertedFrom;
                if (conversion != null && ReferenceEquals(conversion.ConvertedFrom, source) && Pol06HasMetamagic(conversion) &&
                    SummonNativeOptionCatalog.All.Any(value => value.Guid == conversion.Blueprint?.AssetGuid)) return Pol06Projected;
                if (ReferenceEquals(conversion, source) && Pol06HasMetamagic(row) &&
                    SummonNativeOptionCatalog.All.Any(value => value.Guid == row.Blueprint.AssetGuid)) return Pol06Raw;
                return null;
            };
        }

        private static bool Pol06HasMetamagic(AbilityData data)
        { return data?.MetamagicData != null && data.MetamagicData.MetamagicMask != 0; }

        /// <summary>Call of the Wild's live settings object and the backing field
        /// of its "metamagic for spontaneous spell conversion" value, or null.</summary>
        private static Tuple<object, FieldInfo> Pol06CotwSetting()
        {
            Assembly cotw = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == "CallOfTheWild");
            Type main = cotw?.GetType("CallOfTheWild.Main");
            FieldInfo settingsField = main?.GetField("settings", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            object settings = settingsField?.GetValue(null);
            FieldInfo field = settings?.GetType().GetField("<metamagic_for_spontaneous_spell_conversion>k__BackingField",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return settings == null || field == null || field.FieldType != typeof(bool) ? null : Tuple.Create(settings, field);
        }

        /// <summary>Call of the Wild's own conversion list for this slot and
        /// source (its public getAdditionalToggleSpells, which its prefixes
        /// return as the popup list), built fresh; null when unavailable.</summary>
        private static List<AbilityData> Pol06CotwRows(MechanicActionBarSlot slot, Spellbook book, AbilityData source)
        {
            Assembly cotw = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == "CallOfTheWild");
            MethodInfo builder = cotw?.GetType("CallOfTheWild.ActionBarGroupSlot__SetToggleAdditionalSpells__Patch")?.GetMethod("getAdditionalToggleSpells",
                BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(MechanicActionBarSlot), typeof(Spellbook), typeof(AbilityData) }, null);
            return builder == null || slot == null ? null : builder.Invoke(null, new object[] { slot, book, source }) as List<AbilityData>;
        }

        /// <summary>The exact identity of a conversion row: its spell and metamagic.</summary>
        private static string Pol06Key(AbilityData row)
        {
            var metamagic = row?.MetamagicData;
            return (row?.Blueprint?.AssetGuid ?? "null") + "|" + (metamagic == null ? 0L : Convert.ToInt64(metamagic.MetamagicMask)) + "|" +
                (metamagic == null ? 0 : metamagic.HeightenLevel) + "|" + (metamagic == null ? 0 : metamagic.SpellLevelCost);
        }

        private static string Pol06RetainedKey(AbilityData row)
        { return (row.ConvertedFrom == null ? "ability:" : "conversion:") + Pol06Key(row); }

        private static readonly Dictionary<BlueprintAbility, SummonMultiplicity?> Pol06SpawnQuantities = new Dictionary<BlueprintAbility, SummonMultiplicity?>();

        /// <summary>The quantity a summon ability's own spawn expression states, or null.</summary>
        private static SummonMultiplicity? Pol06SpawnQuantity(BlueprintAbility ability)
        {
            if (ability == null) return null;
            SummonMultiplicity? cached;
            if (Pol06SpawnQuantities.TryGetValue(ability, out cached)) return cached;
            try
            {
                SummonMultiplicity?[] quantities = SummonSignatureReader.Read(ability).Spawns()
                    .Select(value => Pol06ConversionObservationVerdict.QuantityOfCountDice(value.CountDice)).Distinct().ToArray();
                cached = quantities.Length == 1 ? quantities[0] : null;
            }
            catch (Exception) { cached = null; }
            Pol06SpawnQuantities[ability] = cached;
            return cached;
        }

        private static bool Pol06IsSummon(BlueprintAbility ability)
        {
            if (ability == null) return false;
            try { return SummonSignatureReader.Read(ability).Spawns().Any() || (ability.Variants != null && ability.Variants.Length != 0); }
            catch (Exception) { return true; }
        }

        /// <summary>The single level at which this book's native conversion
        /// lists hold the spell exactly once, or zero.</summary>
        private static int Pol06NativeTier(Spellbook book, BlueprintAbility spell)
        {
            int[] levels = Enumerable.Range(1, 9).Where(level => book.GetSpontaneousConversionSpells(level).Count(value => ReferenceEquals(value, spell)) == 1).ToArray();
            return levels.Length == 1 ? levels[0] : 0;
        }

        /// <summary>A creature row's quantity at this family and tier from the
        /// contracts that place it; null when none or several disagree.</summary>
        private static object _pol06PlacementRegistry;
        private static readonly Dictionary<string, List<Tuple<string, SummonMultiplicity>>> Pol06Placements = new Dictionary<string, List<Tuple<string, SummonMultiplicity>>>();

        /// <summary>The owned placement contracts, indexed once per registry by
        /// family, tier and registered row.</summary>
        private static List<Tuple<string, SummonMultiplicity>> Pol06OwnedPlacements(SummonFamily family, int tier, BlueprintAbility row)
        {
            var registered = ExpandedSummoningBlueprints.Registered;
            if (registered == null) return new List<Tuple<string, SummonMultiplicity>>();
            if (!ReferenceEquals(_pol06PlacementRegistry, registered))
            {
                Pol06Placements.Clear(); _pol06PlacementRegistry = registered;
                BlueprintScriptableObject value;
                foreach (SummonFamily each in new[] { SummonFamily.Monster, SummonFamily.NaturesAlly })
                {
                    foreach (SummonVariantSpec spec in ExpandedSummoningCatalog.GenerateVariants(each))
                        if (registered.BySymbol.TryGetValue(ExpandedSummoningIdentityCatalog.AbilitySymbol(spec), out value) && value != null)
                            Pol06AddPlacement(each, spec.ParentTier, value.AssetGuid, "placement", spec.Multiplicity);
                    for (int level = 1; level <= 9; level++)
                        foreach (var expansion in SummonNativeExpansionCatalog.For(each, level))
                            if (registered.BySymbol.TryGetValue(expansion.Symbol, out value) && value != null)
                                Pol06AddPlacement(each, level, value.AssetGuid, "native-expansion", expansion.Multiplicity);
                }
            }
            List<Tuple<string, SummonMultiplicity>> found;
            return Pol06Placements.TryGetValue(family + "|" + tier + "|" + row.AssetGuid, out found) ? new List<Tuple<string, SummonMultiplicity>>(found) : new List<Tuple<string, SummonMultiplicity>>();
        }

        private static void Pol06AddPlacement(SummonFamily family, int tier, string guid, string authority, SummonMultiplicity quantity)
        {
            string key = family + "|" + tier + "|" + guid; List<Tuple<string, SummonMultiplicity>> list;
            if (!Pol06Placements.TryGetValue(key, out list)) Pol06Placements.Add(key, list = new List<Tuple<string, SummonMultiplicity>>());
            list.Add(Tuple.Create(authority, quantity));
        }

        private static SummonMultiplicity? Pol06PlacementQuantity(SummonFamily family, int tier, BlueprintAbility row, out string authority)
        {
            var found = Pol06OwnedPlacements(family, tier, row);
            foreach (var option in SummonNativeOptionCatalog.All.Where(item => item.Family == family && item.Tier == tier && item.Guid == row.AssetGuid))
                found.Add(Tuple.Create("native-option", option.Multiplicity));
            SpiderPlacement spider = SpiderSummonerIntegration.PlacementOf(row);
            if (spider != null && spider.Family == family && spider.Tier == tier) found.Add(Tuple.Create("spider", spider.Quantity));
            SummonForeignRowResult foreign = SummonForeignRowRuntime.Resolve(row);
            if (foreign != null && foreign.IsRepaired && foreign.Contract != null && foreign.Contract.Family == family && foreign.Contract.Tier == tier)
                found.Add(Tuple.Create("third-party", foreign.Contract.Multiplicity));
            authority = string.Join("+", found.Select(item => item.Item1).ToArray());
            SummonMultiplicity[] quantities = found.Select(item => item.Item2).Distinct().ToArray();
            return quantities.Length == 1 ? quantities[0] : (SummonMultiplicity?)null;
        }

        private static string Pol06MetamagicText(AbilityData data)
        {
            if (data?.MetamagicData == null || data.MetamagicData.MetamagicMask == 0) return "";
            return Regex.Replace(UIUtilityTexts.GetMetamagicList(data.MetamagicData.MetamagicMask) ?? "", "<[^>]+>", "").Trim();
        }

        /// <summary>One popup's independent verdict. <paramref name="parentOf"/>
        /// gives the route's published parent of a tier.</summary>
        private static JObject Pol06PopupVerdict(AlphaMenu menu, ActionBarGroupSlot popupSource, AbilityData source, SummonFamily family,
            Func<int, BlueprintAbility> parentOf, out bool exact)
        {
            exact = false;
            Spellbook book = source.Spellbook;
            int sourceLevel = book.GetSpellLevel(source);
            var evidence = new JObject { ["sourceSpell"] = source.Blueprint.name + ":" + source.Blueprint.AssetGuid, ["sourceLevel"] = sourceLevel,
                ["sourceMetamagic"] = Pol06Key(source), ["conversionAdapter"] = DruidSummonConversionVariantPatch.Outcome };
            List<AbilityData> cotw = Pol06CotwRows(popupSource?.MechanicSlot, book, source);
            if (menu == null || cotw == null) { evidence["error"] = menu == null ? "no-popup" : "call-of-the-wild-row-builder-unavailable"; return evidence; }
            Func<AbilityData, bool> summonConversion = row => row?.Blueprint != null && ReferenceEquals(row.ConvertedFrom, source) &&
                Pol06NativeTier(book, row.Blueprint) > 0 && Pol06SpawnQuantity(row.Blueprint) != null;
            AbilityData[] ordinaryConversions = cotw.Where(row => summonConversion(row) && !Pol06HasMetamagic(row)).ToArray();
            AbilityData[] additional = cotw.Where(row => summonConversion(row) && Pol06HasMetamagic(row)).ToArray();
            string[] expectedRetained = cotw.Where(row => row?.Blueprint != null && !summonConversion(row)).Select(Pol06RetainedKey).ToArray();
            bool duplicates = additional.GroupBy(Pol06Key).Any(group => group.Count() != 1);
            var placementProblems = new JArray();
            var expected = new List<Pol06ConversionObservationVerdict.ExpectedConversion>();
            foreach (AbilityData row in additional)
            {
                int tier = Pol06NativeTier(book, row.Blueprint);
                SummonMultiplicity quantity = Pol06SpawnQuantity(row.Blueprint).Value;
                BlueprintAbility parent = parentOf(tier);
                bool raw = duplicates || ReferenceEquals(parent, source.Blueprint);
                string[] children = new string[0];
                if (!raw)
                    children = SpiderPermitted(parent, book).Where(child =>
                    {
                        string authority;
                        SummonMultiplicity? placed = Pol06PlacementQuantity(family, tier, child, out authority);
                        SummonMultiplicity? spawned = Pol06SpawnQuantity(child);
                        if (placed == null || spawned != placed)
                            placementProblems.Add(new JObject { ["tier"] = tier, ["child"] = child.AssetGuid, ["name"] = child.Name, ["placed"] = placed?.ToString(),
                                ["authority"] = authority, ["spawnQuantity"] = spawned?.ToString() });
                        return placed == quantity;
                    }).Select(child => child.AssetGuid).ToArray();
                expected.Add(new Pol06ConversionObservationVerdict.ExpectedConversion { Key = Pol06Key(row), Tier = tier, Quantity = quantity, Children = children, ExpectRaw = raw });
            }
            // Placement problems are recorded once per creature.
            placementProblems = new JArray(placementProblems.GroupBy(value => (string)value["child"] + "|" + (int)value["tier"]).Select(group => group.First()));

            AbilityData[] data = menu.Data;
            var conversions = new List<AbilityData>();
            var projected = new List<Pol06ConversionObservationVerdict.ProjectedChild>();
            var childProblems = new JArray();
            var raws = new List<string>(); var retained = new List<string>();
            var ordinaryIndices = new List<int>(); var projectedIndices = new List<int>();
            for (int index = 0; index < data.Length; index++)
            {
                AbilityData row = data[index];
                if (row?.Blueprint == null) { childProblems.Add(new JObject { ["index"] = index, ["problem"] = "null-row" }); continue; }
                AbilityData conversion = row.ConvertedFrom;
                if (conversion != null && ReferenceEquals(conversion.ConvertedFrom, source) && Pol06HasMetamagic(conversion))
                {
                    int instance = conversions.FindIndex(value => ReferenceEquals(value, conversion));
                    if (instance < 0) { conversions.Add(conversion); instance = conversions.Count - 1; }
                    projected.Add(new Pol06ConversionObservationVerdict.ProjectedChild { ConversionInstance = instance, ConversionKey = Pol06Key(conversion), Child = row.Blueprint.AssetGuid });
                    projectedIndices.Add(index);
                    bool semantic = ReferenceEquals(row.MetamagicData, conversion.MetamagicData) && row.SpellLevel == sourceLevel &&
                        ReferenceEquals(row.Spellbook, book) && ReferenceEquals(row.Caster, source.Caster) && ReferenceEquals(row.Fact, conversion.Fact);
                    if (!semantic) childProblems.Add(new JObject { ["index"] = index, ["child"] = row.Blueprint.AssetGuid, ["conversion"] = Pol06Key(conversion),
                        ["sameMetamagicObject"] = ReferenceEquals(row.MetamagicData, conversion.MetamagicData), ["spellLevel"] = row.SpellLevel,
                        ["book"] = ReferenceEquals(row.Spellbook, book), ["caster"] = ReferenceEquals(row.Caster, source.Caster) });
                }
                else if (summonConversion(row) && Pol06HasMetamagic(row)) raws.Add(Pol06Key(row));
                else if (row.ConvertedFrom == null || !Pol06IsSummon(row.Blueprint)) retained.Add(Pol06RetainedKey(row));
                else ordinaryIndices.Add(index);
            }
            var reading = new Pol06ConversionObservationVerdict.Reading { Expected = expected.ToArray(), Projected = projected.ToArray(), Raw = raws.ToArray(),
                ExpectedRetained = expectedRetained, ActualRetained = retained.ToArray() };
            IReadOnlyList<string> problems = Pol06ConversionObservationVerdict.AssociationProblems(reading);
            bool aggregate = Pol06ConversionObservationVerdict.AggregateComplete(reading);
            BlueprintAbility[] ordinaryExpected = SpiderPermitted(parentOf(sourceLevel), book);
            BlueprintAbility[] ordinary = ordinaryIndices.Select(index => data[index].Blueprint).ToArray();
            bool ordinaryExact = ordinary.SequenceEqual(ordinaryExpected) && ordinaryConversions.Length > 0;
            bool afterOrdinary = projectedIndices.Count == 0 || ordinaryIndices.Count == 0 || projectedIndices.Min() > ordinaryIndices.Max();
            bool anyProjected = expected.Any(value => !value.ExpectRaw && value.Children.Length > 0);
            exact = problems.Count == 0 && childProblems.Count == 0 && placementProblems.Count == 0 && aggregate && ordinaryExact && afterOrdinary && anyProjected;
            evidence["rows"] = data.Length; evidence["ordinary"] = ordinary.Length; evidence["ordinaryExpected"] = ordinaryExpected.Length; evidence["ordinaryExact"] = ordinaryExact;
            evidence["projected"] = projected.Count; evidence["raw"] = raws.Count; evidence["retained"] = new JArray(retained); evidence["expectedRetained"] = new JArray(expectedRetained);
            evidence["projectedAfterOrdinary"] = afterOrdinary; evidence["aggregateComplete"] = aggregate;
            evidence["conversions"] = new JArray(expected.Select(value => new JObject { ["key"] = value.Key, ["tier"] = value.Tier, ["quantity"] = value.Quantity.ToString(),
                ["expectRaw"] = value.ExpectRaw, ["expectedChildren"] = value.Children.Length,
                ["actualChildren"] = projected.Count(child => child.ConversionKey == value.Key),
                ["metamagicText"] = Pol06MetamagicText(additional.First(row => Pol06Key(row) == value.Key)) }));
            evidence["associationProblems"] = new JArray(problems.Take(80));
            evidence["childProblems"] = childProblems; evidence["placementProblems"] = placementProblems;
            evidence["expectationSource"] = "Call of the Wild getAdditionalToggleSpells rows; native conversion-list tier; spawn-expression quantity; " +
                "placement/native-option/third-party/Spider contracts for the creatures, cross-checked with each creature's spawn expression; product receipts not used";
            return evidence;
        }

        /// <summary>Prepared slots of the source's level whose spell the native
        /// spend would treat as this source (AbilityData equality).</summary>
        private static int Pol06EqualPreparedCopies(Spellbook book, AbilityData source)
        {
            return book.GetMemorizedSpellSlots(book.GetSpellLevel(source)).Count(slot => slot?.Spell != null && source.Equals(slot.Spell));
        }

        /// <summary>A projected child of the popup whose own placement and spawn
        /// state the wanted quantity, preferring the named creature.</summary>
        private static AbilityData Pol06Pick(AlphaMenu menu, AbilityData source, SummonFamily family, SummonMultiplicity quantity, Func<AbilityData, bool> extra = null,
            BlueprintAbility preferred = null)
        {
            if (menu == null) return null;
            Spellbook book = source.Spellbook;
            AbilityData[] candidates = menu.Data.Where(row => Pol06Classify(source)(row) == Pol06Projected && (extra == null || extra(row)) &&
                row.IsAvailableForCast && row.GetAvailableForCastCount() > 0).Where(row =>
                {
                    int tier = Pol06NativeTier(book, row.ConvertedFrom.Blueprint); string authority;
                    return tier > 0 && Pol06PlacementQuantity(family, tier, row.Blueprint, out authority) == quantity && Pol06SpawnQuantity(row.Blueprint) == quantity &&
                        SummonSignatureReader.Read(row.Blueprint).Spawns().Count() == 1;
                }).ToArray();
            return candidates.FirstOrDefault(row => ReferenceEquals(row.Blueprint, preferred)) ??
                candidates.FirstOrDefault(row => !(row.Blueprint.name ?? "").StartsWith("KMG_Summoning_Spider_", StringComparison.Ordinal)) ?? candidates.FirstOrDefault();
        }

        /// <summary>One projected cast through the real popup row, with the
        /// quantity die controlled, the engine's summon rules captured and the
        /// exact debit observed by the shared cast observer.</summary>
        private IEnumerable<int> Pol06Cast(ExpandedSummoningCorrectionFixture fixture, UnitEntityData caster, AlphaMenu menu, ActionBarGroupSlot popupSource,
            AbilityData source, AbilityData chosen, SummonFamily family, Func<int, BlueprintAbility> parentOf, Cleanup2NativeDefaultScope mode, Stopwatch watch,
            string label, Action<JObject> report)
        {
            int index = Array.IndexOf(menu.Data, chosen);
            int tier = Pol06NativeTier(source.Spellbook, chosen.ConvertedFrom.Blueprint); string authority;
            SummonMultiplicity quantity = Pol06PlacementQuantity(family, tier, chosen.Blueprint, out authority).Value;
            BlueprintFeature superior = BlueprintBootstrap.Library.BlueprintsByAssetId.ContainsKey(Pol06SuperiorSummoning) ?
                BlueprintBootstrap.Library.BlueprintsByAssetId[Pol06SuperiorSummoning] as BlueprintFeature : null;
            bool superiorHeld = superior == null || caster.Descriptor.HasFact(superior);
            int expectedCount = Pol06ConversionObservationVerdict.ExpectedCount(quantity, Pol06ForcedDie);
            DiceType die = quantity == SummonMultiplicity.OneD3 ? DiceType.D3 : quantity == SummonMultiplicity.OneD4PlusOne ? DiceType.D4 : DiceType.Zero;
            var dieOverride = die == DiceType.Zero ? null : new SpiderDieOverride { Initiator = caster, Die = die, Value = Pol06ForcedDie };
            var debit = new Cleanup2SourceDebit(caster); JArray rules = null; ActionBarGroupSlot close = popupSource;
            UnitEntityData fixtureCaster = fixture.Caster;
            var record = new JObject { ["child"] = chosen.Blueprint.Name, ["childGuid"] = chosen.Blueprint.AssetGuid, ["conversion"] = Pol06Key(chosen.ConvertedFrom),
                ["conversionTier"] = tier, ["quantity"] = quantity.ToString(), ["placementAuthority"] = authority, ["forcedDie"] = die == DiceType.Zero ? null : (JToken)Pol06ForcedDie,
                ["expectedCount"] = expectedCount, ["superiorSummoningHeld"] = superiorHeld, ["childMetamagic"] = Pol06Key(chosen),
                ["childMetamagicText"] = Pol06MetamagicText(chosen), ["childSpellLevel"] = chosen.SpellLevel, ["heightenLevel"] = chosen.MetamagicData?.HeightenLevel ?? 0,
                ["spellLevelCost"] = chosen.MetamagicData?.SpellLevelCost ?? 0, ["sourceSlotLevel"] = source.ParamSpellSlot?.SpellLevel,
                ["equalPreparedCopies"] = Pol06EqualPreparedCopies(source.Spellbook, source), ["casterLevel"] = source.Spellbook.CasterLevel };
            if (index < 0 || superiorHeld) { record["error"] = index < 0 ? "row-not-in-popup" : "superior-summoning-held-or-unresolved"; report(record); yield break; }
            if (dieOverride != null) EventBus.Subscribe(dieOverride);
            fixture.Caster = caster;
            try
            {
                foreach (int frame in SpiderSummonRuleScope(() => R6Cell(label + "-cast", () => Cleanup2MenuAdapterDruidCast(fixture, chosen, source, parentOf(tier),
                    menu.Rows[index], debit, mode, watch, () => Cleanup2MenuAdapterHide(close, source), label, count => count == expectedCount)), value => rules = value)) yield return frame;
            }
            finally
            {
                fixture.Caster = fixtureCaster;
                if (dieOverride != null) EventBus.Unsubscribe(dieOverride);
            }
            record["rules"] = rules; record["summoned"] = rules?.Count ?? 0;
            record["rolls"] = dieOverride == null ? null : new JArray(dieOverride.Rolls);
            record["slotSpent"] = source.ParamSpellSlot != null && !source.ParamSpellSlot.Available;
            record["debit"] = debit.Describe(source.ParamSpellSlot); record["exactlyOneSourceSlot"] = debit.ExactlyOne(source.ParamSpellSlot);
            report(record);
        }

        /// <summary>Opens one route popup over the source, then the verdict.</summary>
        private IEnumerable<int> Pol06Popup(string label, ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, Spellbook book, AbilityData source,
            SummonFamily family, Func<int, BlueprintAbility> parentOf, Cleanup2NativeDefaultScope mode, Stopwatch watch, string claim,
            Action<AlphaMenu, ActionBarGroupSlot, JObject, bool> result)
        {
            int sourceLevel = book.GetSpellLevel(source);
            AlphaMenu menu = null; ActionBarGroupSlot popupSource = null;
            foreach (int frame in R6Cell(label + "-popup", () => ClericOpenMenu(label, spellGroup, levels, sourceLevel, book, parentOf(sourceLevel), source, mode, watch, null,
                (value, slot) => { menu = value; popupSource = slot; }, false, SpiderPermitted(parentOf(sourceLevel), book), row =>
                {
                    string kind = Pol06Classify(source)(row);
                    if (kind != null) return kind;
                    return row.ConvertedFrom == null || !Pol06IsSummon(row.Blueprint) ? "retained-non-summon-or-ability-row" : null;
                }))) yield return frame;
            bool exact;
            JObject verdict = Pol06PopupVerdict(menu, popupSource, source, family, parentOf, out exact);
            Sprint25Check(label + "-per-conversion-projection", exact, verdict, claim);
            result(menu, popupSource, verdict, exact);
        }

        private static BlueprintFeature Pol06MetamagicFeat(Kingmaker.UnitLogic.Abilities.Metamagic metamagic)
        {
            return BlueprintBootstrap.Library.BlueprintsByAssetId.Values.OfType<BlueprintFeature>().Where(value =>
                (value.ComponentsArray ?? new BlueprintComponent[0]).OfType<AddMetamagicFeat>().Any(component => component.Metamagic == metamagic))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).FirstOrDefault();
        }

        private IEnumerable<int> Pol06HeraldCells(ExpandedSummoningCorrectionFixture fixture, Spellbook book, Cleanup2NativeDefaultScope mode,
            ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, Func<int, BlueprintAbility> smParent, Func<SummonFamily, string, BlueprintAbility> original)
        {
            var watch = Stopwatch.StartNew();
            var setting = Pol06CotwSetting();
            bool settingOn = setting != null && (bool)setting.Item2.GetValue(setting.Item1);
            Sprint25Check("pol06-cotw-setting-present", setting != null && settingOn, new JObject { ["settingFound"] = setting != null, ["value"] = settingOn },
                "Call of the Wild is loaded in this profile and its metamagic-for-spontaneous-conversion setting is on.");
            if (setting == null || !settingOn) yield break;

            // A level 4 spell no earlier cell prepared: the native spend takes the
            // last available slot whose spell equals the source, so a second
            // identical extended copy would be spent instead.
            BlueprintAbility unrelatedFour = SpiderPlainSpell(book, 4, 1, true);
            AbilityData source = SpiderPrepareExtended(book, unrelatedFour, 4);
            book.Rest();
            int sourceLevel = book.GetSpellLevel(source);
            int equalCopies = Pol06EqualPreparedCopies(book, source);
            Sprint25Check("pol06-herald-source-unique", equalCopies == 1 && sourceLevel == 5, new JObject { ["sourceSpell"] = unrelatedFour.name + ":" + unrelatedFour.AssetGuid,
                ["equalPreparedCopies"] = equalCopies, ["slotLevel"] = source.ParamSpellSlot?.SpellLevel, ["sourceLevel"] = sourceLevel },
                "The Herald POL-06 source is the only prepared slot whose spell equals it, so this fixture's debit and staleness are exact.");
            BlueprintAbility[] ordinaryExpected = SpiderPermitted(smParent(5), book);
            // Owned generated rows only: like the phase's forbidden selected child,
            // the gate hides them and refuses them (visible false, unavailable).
            BlueprintAbility[] forbidden = (smParent(4).Variants ?? new BlueprintAbility[0]).Where(value => value.name != null &&
                value.name.StartsWith("KMG_Summoning_Ability_", StringComparison.Ordinal) &&
                !SummonClericAlignmentRuntime.PermitsRow(value, book.Owner, book)).OrderBy(value => value.name, StringComparer.Ordinal).ToArray();

            AlphaMenu menu = null; ActionBarGroupSlot popupSource = null;
            foreach (int frame in Pol06Popup("pol06-herald-cotw-on", spellGroup, levels, book, source, SummonFamily.Monster, smParent, mode, watch,
                "Over an unrelated level 4 spell prepared with Extend Spell (level 5 slot), the real Herald popup keeps the ordinary Summon Monster V rows exactly and " +
                "each Call of the Wild Summon Monster IV metamagic conversion owns exactly this caster's permitted creatures of its own quantity, after them, each carrying " +
                "that conversion's own metamagic object at spell level 5; none stays raw and the other rows are retained.",
                (value, slot, verdict, exact) => { menu = value; popupSource = slot; verdict["forbiddenForThisCaster"] = new JArray(forbidden.Select(item => item.Name)); })) yield return frame;

            BlueprintAbility widow = original(SummonFamily.Monster, "giant-black-widow");
            AbilityData chosen = Pol06Pick(menu, source, SummonFamily.Monster, SummonMultiplicity.One, null, widow);
            AbilityData retained = chosen == null ? null : menu.Data.FirstOrDefault(value => !ReferenceEquals(value, chosen) && ReferenceEquals(value.ConvertedFrom, chosen.ConvertedFrom));
            if (chosen != null && popupSource != null)
            {
                JObject cast = null;
                foreach (int frame in Pol06Cast(fixture, fixture.Caster, menu, popupSource, source, chosen, SummonFamily.Monster, smParent, mode, watch, "pol06-herald-additional",
                    value => cast = value)) yield return frame;
                popupSource = null;
                string problem = SpiderDurationProblem(cast["rules"] as JArray, book.CasterLevel, true);
                cast["durationProblem"] = problem; cast["spider"] = ReferenceEquals(chosen.Blueprint, widow);
                Sprint25Check("pol06-herald-additional-extend-duration-debit", cast["error"] == null && problem == null && book.CasterLevel == 20 &&
                    (bool)cast["exactlyOneSourceSlot"] && chosen.MetamagicData.Has(Kingmaker.UnitLogic.Abilities.Metamagic.Extend), cast,
                    "The projected Single Summon Monster IV choice (the Black Widow where offered) keeps Call of the Wild's Extend: the engine's summon rule gives 40 rounds at caster level 20, " +
                    "one creature is summoned, and exactly the level 5 source slot is spent (the only equal preparation).");

                if (retained != null)
                    foreach (int frame in R6Cell("pol06-herald-stale-refused", () => ClericRefusedAttempt(fixture, retained, source, mode, watch, "pol06-herald-stale-choice-refused", "pol06-stale",
                        true, "a projected choice retained from the popup, after its source slot was spent; native UnitUseAbility",
                        "A projected Call of the Wild choice retained after its source slot was spent has no cast left and is refused with nothing spent and nothing summoned.",
                        true))) yield return frame;
                else Sprint25Check("pol06-herald-stale-choice-present", false, new JObject { ["rows"] = menu.Rows.Length }, "A second projected choice of the same conversion exists to retain.");
            }
            else Sprint25Check("pol06-herald-additional-row-present", false, new JObject { ["rows"] = menu?.Rows.Length ?? 0 },
                "A projected Single Call of the Wild choice is offered.");
            if (popupSource != null) Cleanup2MenuAdapterHide(popupSource, source);
            book.Rest();

            // An evil Summon Monster IV creature this chaotic good Herald may not
            // call, built under the same Call of the Wild row as a saved shortcut would hold it.
            AbilityData row = chosen?.ConvertedFrom;
            if (row != null && forbidden.Length > 0)
            {
                var forbiddenChild = new AbilityData(forbidden[0], book) { ConvertedFrom = row };
                foreach (int frame in R6Cell("pol06-herald-forbidden-refused", () => ClericRefusedAttempt(fixture, forbiddenChild, source, mode, watch, "pol06-herald-forbidden-choice-refused",
                    "pol06-forbidden", false, "a forbidden-alignment Summon Monster IV creature under the Call of the Wild Extend row; native UnitUseAbility",
                    "A forbidden-alignment creature under a Call of the Wild metamagic row is refused before any slot is spent, and nothing is summoned."))) yield return frame;
                book.Rest();
            }
            else Sprint25Check("pol06-herald-forbidden-choice-present", false, new JObject { ["row"] = row != null, ["forbidden"] = forbidden.Length },
                "A forbidden-alignment Summon Monster IV creature exists for this caster.");

            // The setting off: CotW reads this value on every popup fill.
            setting.Item2.SetValue(setting.Item1, false);
            try
            {
                AlphaMenu offMenu = null; ActionBarGroupSlot offSource = null;
                foreach (int frame in R6Cell("pol06-herald-cotw-off-popup", () => ClericOpenMenu("pol06-herald-cotw-off-tier5", spellGroup, levels, 5, book, smParent(5), source, mode, watch, null,
                    (value, slot) => { offMenu = value; offSource = slot; }, false, ordinaryExpected, Pol06Classify(source)))) yield return frame;
                int additionalRows = offMenu == null ? -1 : offMenu.Data.Count(value => Pol06Classify(source)(value) != null);
                Sprint25Check("pol06-herald-cotw-off-ordinary-only", offMenu != null && additionalRows == 0 && offMenu.Data.Select(value => value?.Blueprint).SequenceEqual(ordinaryExpected),
                    new JObject { ["rows"] = offMenu?.Rows.Length ?? 0, ["additionalRows"] = additionalRows, ["ordinaryExpected"] = ordinaryExpected.Length,
                        ["conversionAdapter"] = DruidSummonConversionVariantPatch.Outcome, ["toggle"] = "CallOfTheWild.Main.settings in-memory value; settings file untouched" },
                    "With Call of the Wild's setting off, the same Herald popup lists exactly the ordinary Summon Monster V rows and no additional row.");
                if (offSource != null) Cleanup2MenuAdapterHide(offSource, source);
            }
            finally { setting.Item2.SetValue(setting.Item1, true); }
            book.Rest();

            foreach (int frame in R6Cell("pol06-herald-d4", () => Pol06HeraldQuantityCells(fixture, book, source, mode, spellGroup, levels, smParent))) yield return frame;
            foreach (int frame in R6Cell("pol06-herald-second-metamagic", () => Pol06HeraldSecondMetamagicCells(fixture, book, source, mode, spellGroup, levels, smParent))) yield return frame;
        }

        /// <summary>A projected d4+1 Herald cast with its d4 controlled.</summary>
        private IEnumerable<int> Pol06HeraldQuantityCells(ExpandedSummoningCorrectionFixture fixture, Spellbook book, AbilityData source, Cleanup2NativeDefaultScope mode,
            ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, Func<int, BlueprintAbility> smParent)
        {
            var watch = Stopwatch.StartNew();
            AlphaMenu menu = null; ActionBarGroupSlot popupSource = null;
            foreach (int frame in R6Cell("pol06-herald-d4-popup", () => ClericOpenMenu("pol06-herald-d4-tier5", spellGroup, levels, 5, book, smParent(5), source, mode, watch, null,
                (value, slot) => { menu = value; popupSource = slot; }, false, SpiderPermitted(smParent(5), book), Pol06Classify(source)))) yield return frame;
            AbilityData chosen = Pol06Pick(menu, source, SummonFamily.Monster, SummonMultiplicity.OneD4PlusOne);
            if (chosen != null && popupSource != null)
            {
                JObject cast = null;
                foreach (int frame in Pol06Cast(fixture, fixture.Caster, menu, popupSource, source, chosen, SummonFamily.Monster, smParent, mode, watch, "pol06-herald-d4",
                    value => cast = value)) yield return frame;
                popupSource = null;
                string problem = SpiderDurationProblem(cast["rules"] as JArray, book.CasterLevel, true);
                cast["durationProblem"] = problem;
                Sprint25Check("pol06-herald-d4-count-duration-debit", cast["error"] == null && problem == null &&
                    (int)cast["summoned"] == 3 && (bool)cast["exactlyOneSourceSlot"] && (cast["rolls"] as JArray)?.Count >= 1, cast,
                    "A projected d4+1 Summon Monster IV choice under Call of the Wild's Extend row, its d4 forced to 2, summons exactly three creatures (the contract's d4+1, " +
                    "not the observed count), each for 40 rounds at caster level 20, and spends exactly the source slot.");
            }
            else Sprint25Check("pol06-herald-d4-row-present", false, new JObject { ["rows"] = menu?.Rows.Length ?? 0 }, "A projected d4+1 Call of the Wild choice is offered.");
            if (popupSource != null) Cleanup2MenuAdapterHide(popupSource, source);
            book.Rest();
        }

        /// <summary>A second applicable metamagic feat on the Herald: the popup's
        /// per-conversion verdict and size, reachability and tooltips of the same
        /// creature under two metamagic conversions, and one cast of the second.</summary>
        private IEnumerable<int> Pol06HeraldSecondMetamagicCells(ExpandedSummoningCorrectionFixture fixture, Spellbook book, AbilityData source, Cleanup2NativeDefaultScope mode,
            ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, Func<int, BlueprintAbility> smParent)
        {
            var watch = Stopwatch.StartNew();
            BlueprintAbility[] tierFourQuantity = Enumerable.Range(1, 4).SelectMany(level => book.GetSpontaneousConversionSpells(level))
                .Where(value => Pol06NativeTier(book, value) == 4 && Pol06SpawnQuantity(value) != null).Distinct().ToArray();
            var candidates = new[] { Kingmaker.UnitLogic.Abilities.Metamagic.Heighten, Kingmaker.UnitLogic.Abilities.Metamagic.Reach,
                Kingmaker.UnitLogic.Abilities.Metamagic.Empower, Kingmaker.UnitLogic.Abilities.Metamagic.Quicken };
            var survey = new JArray();
            Kingmaker.UnitLogic.Abilities.Metamagic? selected = null; BlueprintFeature feat = null;
            foreach (var metamagic in candidates)
            {
                BlueprintFeature grant = Pol06MetamagicFeat(metamagic);
                bool accepted = tierFourQuantity.Length > 0 && tierFourQuantity.All(value => (value.AvailableMetamagic & metamagic) != 0);
                survey.Add(new JObject { ["metamagic"] = metamagic.ToString(), ["feat"] = grant == null ? null : grant.name + ":" + grant.AssetGuid, ["acceptedByTierFourQuantitySpells"] = accepted,
                    ["defaultCost"] = metamagic.DefaultCost() });
                if (selected == null && grant != null && accepted) { selected = metamagic; feat = grant; }
            }
            Sprint25Check("pol06-second-metamagic-selected", selected != null, new JObject { ["selected"] = selected?.ToString(), ["survey"] = survey,
                ["tierFourQuantitySpells"] = new JArray(tierFourQuantity.Select(value => value.name)), ["rule"] = "Heighten preferred; otherwise the first installed metamagic feat every Summon Monster IV quantity spell accepts." },
                "A second metamagic feat, Heighten where installed and accepted, is available to the Herald besides Extend Spell.");
            if (selected == null) yield break;
            bool granted = !book.Owner.HasFact(feat);
            if (granted) book.Owner.AddFact(feat);
            try
            {
                AlphaMenu menu = null; ActionBarGroupSlot popupSource = null; JObject verdict = null;
                foreach (int frame in Pol06Popup("pol06-herald-two-feats", spellGroup, levels, book, source, SummonFamily.Monster, smParent, mode, watch,
                    "With Extend Spell and a second metamagic feat, every Call of the Wild conversion over the extended level 4 source (level 5 slot) owns exactly the permitted " +
                    "creatures of its own tier and quantity with its own metamagic at spell level 5; the ordinary rows stay exact and the other rows are retained.",
                    (value, slot, result, exact) => { menu = value; popupSource = slot; verdict = result; })) yield return frame;
                if (menu != null && popupSource != null)
                {
                    // Presentation: the popup size, and the same creature under the two tier 4 conversions.
                    Func<AbilityData, bool> isSecond = value => value.ConvertedFrom?.MetamagicData != null && value.ConvertedFrom.MetamagicData.Has(selected.Value);
                    Func<AbilityData, bool> isExtendOnly = value => value.ConvertedFrom?.MetamagicData != null &&
                        value.ConvertedFrom.MetamagicData.MetamagicMask == Kingmaker.UnitLogic.Abilities.Metamagic.Extend;
                    AbilityData[] projected = menu.Data.Where(value => Pol06Classify(source)(value) == Pol06Projected).ToArray();
                    AbilityData second = projected.FirstOrDefault(value => isSecond(value) && Pol06NativeTier(book, value.ConvertedFrom.Blueprint) == 4 &&
                        projected.Any(other => isExtendOnly(other) && ReferenceEquals(other.Blueprint, value.Blueprint)) && SummonSignatureReader.Read(value.Blueprint).Spawns().Count() == 1 &&
                        Pol06SpawnQuantity(value.Blueprint) == SummonMultiplicity.One);
                    AbilityData extendTwin = second == null ? null : projected.First(value => isExtendOnly(value) && ReferenceEquals(value.Blueprint, second.Blueprint));
                    var labels = menu.Data.Where(value => value != null).Select(value => Tuple.Create(value.Name ?? "", Pol06MetamagicText(value),
                        value.Blueprint.AssetGuid + "|" + (value.ConvertedFrom == null ? "" : Pol06Key(value.ConvertedFrom)))).ToArray();
                    IReadOnlyList<string> indistinguishable = Pol06ConversionObservationVerdict.IndistinguishableRows(labels);
                    var presentation = new JObject { ["rows"] = menu.Rows.Length, ["ordinary"] = verdict?["ordinary"], ["projected"] = verdict?["projected"],
                        ["conversions"] = verdict?["conversions"], ["metamagicFeats"] = new JArray("Extend Spell", feat.name), ["indistinguishable"] = new JArray(indistinguishable),
                        ["sameCreatureTwoConversions"] = second == null ? null : second.Name, ["metamagicTexts"] = second == null ? null : new JArray(Pol06MetamagicText(extendTwin), Pol06MetamagicText(second)) };
                    Sprint25Check("pol06-two-feats-presentation-distinguishable", second != null && indistinguishable.Count == 0, presentation,
                        "In the larger popup no two rows with different semantics share both a label and the native metamagic text; the same creature appears under the Extend and the second conversion.");
                    if (second != null)
                    {
                        foreach (int frame in R6Cell("pol06-two-feats-reach", () => Pol06Reachability(menu, popupSource, source, new[] { extendTwin, second, menu.Data.Last(value => value != null) }, mode))) yield return frame;
                        foreach (var target in new[] { new { Key = "extend", Row = extendTwin }, new { Key = "second", Row = second } })
                        {
                            var scoped = target;
                            foreach (int frame in R6Cell("pol06-two-feats-tooltip-" + scoped.Key, () => Pol06RowTooltip("pol06-two-feats-" + scoped.Key, menu, popupSource, source, scoped.Row, mode))) yield return frame;
                        }
                        JObject cast = null;
                        foreach (int frame in Pol06Cast(fixture, fixture.Caster, menu, popupSource, source, second, SummonFamily.Monster, smParent, mode, watch, "pol06-herald-second",
                            value => cast = value)) yield return frame;
                        popupSource = null;
                        bool extendExpected = second.MetamagicData.Has(Kingmaker.UnitLogic.Abilities.Metamagic.Extend);
                        string problem = SpiderDurationProblem(cast["rules"] as JArray, book.CasterLevel, extendExpected);
                        JArray rules = cast["rules"] as JArray;
                        bool ruleMetamagic = rules != null && rules.Count == 1 && ((string)rules[0]["metamagic"] ?? "").Contains(selected.Value.ToString());
                        cast["durationProblem"] = problem; cast["ruleCarriesSecondMetamagic"] = ruleMetamagic;
                        bool fields = selected.Value != Kingmaker.UnitLogic.Abilities.Metamagic.Heighten ||
                            (second.MetamagicData.HeightenLevel >= 1 && second.MetamagicData.SpellLevelCost == 1);
                        Sprint25Check("pol06-herald-second-metamagic-cast", cast["error"] == null && problem == null && ruleMetamagic && fields &&
                            second.SpellLevel == 5 && (int)cast["summoned"] == 1 && (bool)cast["exactlyOneSourceSlot"], cast,
                            "The projected tier 4 Single choice under the second metamagic conversion casts with that conversion's own metamagic (for Heighten, its heighten level and " +
                            "level cost recorded) at spell level 5, the engine's summon rule carrying it, one creature, and exactly the source slot spent.");
                    }
                }
                if (popupSource != null) Cleanup2MenuAdapterHide(popupSource, source);
            }
            finally { if (granted && book.Owner.HasFact(feat)) book.Owner.RemoveFact(feat); }
            book.Rest();
        }

        /// <summary>Brings each row fully into the popup's own view through the
        /// existing reveal path and records whether it lies on screen.</summary>
        private IEnumerable<int> Pol06Reachability(AlphaMenu menu, ActionBarGroupSlot popupSource, AbilityData source, AbilityData[] targets, Cleanup2NativeDefaultScope mode)
        {
            var popup = menu.Popup;
            Func<bool> owns = () => mode.Current && Cleanup2NoNarrativeOverlay() && popup != null && popup.gameObject.activeInHierarchy &&
                ReferenceEquals(popupSource.MechanicSlot?.GetContentData(), source) && Game.Instance.SelectedAbilityHandler.Ability == null;
            ScrollRect[] scrolls = popup.GetComponents<ScrollRect>();
            ScrollRect scrollRect = scrolls.Length == 1 && scrolls[0].isActiveAndEnabled ? scrolls[0] : null;
            var scroll = scrollRect != null ? new NativeIconScreenEvidence.NativeRowScroll(scrollRect, owns) :
                NativeIconScreenEvidence.NativeRowScroll.StationaryPopup((RectTransform)popup.transform, owns);
            var canvas = popup.GetComponentInParent<Canvas>()?.rootCanvas;
            Camera camera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var records = new JArray(); bool all = true;
            foreach (AbilityData target in targets.Where(value => value != null).Distinct())
            {
                int index = Array.IndexOf(menu.Data, target);
                if (index < 0) { all = false; records.Add(new JObject { ["row"] = target.Name, ["error"] = "not-in-popup" }); continue; }
                ActionBarSlot row = menu.Rows[index];
                if (!scroll.IsStationary) foreach (int frame in Cleanup2RevealPopupRow(scroll, row, owns)) yield return frame;
                var corners = new Vector3[4]; ((RectTransform)row.transform).GetWorldCorners(corners);
                Vector2[] screen = corners.Select(corner => RectTransformUtility.WorldToScreenPoint(camera, corner)).ToArray();
                bool onScreen = screen.All(point => point.x >= -0.5f && point.y >= -0.5f && point.x <= Screen.width + 0.5f && point.y <= Screen.height + 0.5f);
                bool visible = Cleanup2FullyVisible(scroll, row) && onScreen;
                all &= visible;
                records.Add(new JObject { ["row"] = target.Name, ["index"] = index, ["conversion"] = target.ConvertedFrom == null ? null : Pol06Key(target.ConvertedFrom),
                    ["fullyVisibleInPopup"] = Cleanup2FullyVisible(scroll, row), ["onScreen"] = onScreen,
                    ["screenRect"] = new JArray(screen.Min(point => point.x), screen.Min(point => point.y), screen.Max(point => point.x), screen.Max(point => point.y)) });
            }
            Sprint25Check("pol06-two-feats-rows-reachable", all, new JObject { ["rows"] = menu.Rows.Length, ["scroll"] = scroll.Api, ["stationary"] = scroll.IsStationary,
                ["screen"] = new JArray(Screen.width, Screen.height), ["targets"] = records },
                "The selected rows of the larger popup (the same creature under both conversions, and the popup's last row) can each be brought fully into view on screen.");
        }

        /// <summary>Shows one row's native tooltip through its own trigger, reads
        /// the rendered text for the row's native metamagic list, and closes it.</summary>
        private IEnumerable<int> Pol06RowTooltip(string label, AlphaMenu menu, ActionBarGroupSlot popupSource, AbilityData source, AbilityData target, Cleanup2NativeDefaultScope mode)
        {
            int index = Array.IndexOf(menu.Data, target);
            if (index < 0) throw new InvalidOperationException("Tooltip target row absent: " + label);
            ActionBarSlot row = menu.Rows[index];
            var tips = Game.Instance.UI.TooltipsController;
            var close = new Cleanup2TooltipCloseState { Controller = tips, Constructor = tips.TooltipConstructor };
            Func<bool> owns = () => mode.Current && Cleanup2NoNarrativeOverlay() && menu.Popup.gameObject.activeInHierarchy &&
                ReferenceEquals(popupSource.MechanicSlot?.GetContentData(), source) && ReferenceEquals(row.MechanicSlot?.GetContentData(), target);
            TooltipTrigger trigger = row.GetComponentsInChildren<TooltipTrigger>(true).SingleOrDefault(value =>
                value.gameObject.activeInHierarchy && ReferenceEquals(value.GetComponentInParent<ActionBarSlot>(), row));
            if (trigger == null || trigger.Disable || !trigger.isActiveAndEnabled || tips.ContextTooltipData != null)
                throw new InvalidOperationException("Exact native row tooltip trigger is unavailable or a foreign tooltip is open: " + label);
            string expected = Pol06MetamagicText(target);
            JObject card = null; TooltipData data = null; Exception failure = null;
            var iterator = Pol06TooltipBody(trigger, target, expected, owns, close, value => { card = value; data = close.Data; }).GetEnumerator();
            try
            {
                while (true)
                {
                    bool next = false; try { next = iterator.MoveNext(); } catch (Exception error) { failure = error; }
                    if (!next) break; yield return iterator.Current;
                }
            }
            finally { try { iterator.Dispose(); } catch (Exception error) { failure = failure ?? error; } }
            var watch = Stopwatch.StartNew(); JObject state = Cleanup2TooltipClosedState(close);
            for (int frame = 0; frame < 600 && watch.Elapsed.TotalSeconds < 8 && !(bool)state["closed"] && !(bool)state["foreignOwner"]; frame++)
            { yield return 0; state = Cleanup2TooltipClosedState(close); }
            card = card ?? new JObject();
            card["closed"] = state; card["originalFailure"] = failure?.ToString();
            Sprint25Check(label + "-tooltip", failure == null && card["passed"] != null && (bool)card["passed"] && (bool)state["closed"], card,
                "The row's own native tooltip renders its native metamagic list, which tells this conversion's semantics apart, and closes without a foreign tooltip.");
        }

        private static IEnumerable<int> Pol06TooltipBody(TooltipTrigger trigger, AbilityData target, string expected, Func<bool> owns, Cleanup2TooltipCloseState close, Action<JObject> report)
        {
            try
            {
                trigger.ShowTooltipManual(true); close.Data = trigger.Data;
                if (close.Data == null || !ReferenceEquals(close.Data.AbilityData, target)) throw new InvalidOperationException("Native tooltip did not collect the exact row ability.");
                var watch = Stopwatch.StartNew(); string[] texts = new string[0]; bool found = false;
                for (int frame = 0; frame < 600 && watch.Elapsed.TotalSeconds < 8; frame++)
                {
                    if (!owns() || !ReferenceEquals(Game.Instance.UI.TooltipsController.ContextTooltipData, close.Data))
                        throw new InvalidOperationException("Tooltip/source ownership changed.");
                    var constructor = Game.Instance.UI.TooltipsController.TooltipConstructor;
                    texts = constructor == null ? new string[0] : constructor.GetComponentsInChildren<TextMeshProUGUI>(false)
                        .Where(value => value.isActiveAndEnabled && value.canvasRenderer != null && value.canvasRenderer.GetInheritedAlpha() > 0f)
                        .Select(value => value.GetParsedText() ?? "").Where(value => value.Length != 0).ToArray();
                    found = expected.Length != 0 && texts.Any(value => value.Contains(expected));
                    if (found && frame >= 5) break;
                    yield return 0;
                }
                report(new JObject { ["row"] = target.Name, ["conversion"] = target.ConvertedFrom == null ? null : Pol06Key(target.ConvertedFrom),
                    ["expectedMetamagicText"] = expected, ["metamagicTextRendered"] = found, ["passed"] = found,
                    ["texts"] = new JArray(texts.Take(40)), ["source"] = "Native TooltipTrigger.ShowTooltipManual(true) on the row; DescriptionTemplatesAbility renders UIUtilityTexts.GetMetamagicList(mask); read from the live tooltip constructor" });
            }
            finally
            {
                var context = Game.Instance.UI.TooltipsController.ContextTooltipData;
                if (close.Data != null)
                {
                    if (context != null && !ReferenceEquals(context, close.Data)) throw new InvalidOperationException("Foreign tooltip is not hidden.");
                    trigger.ShowTooltipManual(false); close.CloseRequested = true;
                }
            }
        }

        private IEnumerable<int> Pol06DruidCells(ExpandedSummoningCorrectionFixture fixture, SpiderInterfaceCasters casters, Cleanup2NativeDefaultScope mode,
            ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, Func<int, BlueprintAbility> snaParent)
        {
            var watch = Stopwatch.StartNew();
            Spellbook book = casters.DruidBook;
            if (book == null || Pol06CotwSetting() == null) { Sprint25Check("pol06-druid-prerequisites", false, casters.Setup, "The Druid and Call of the Wild's setting exist."); yield break; }
            BlueprintAbility unrelatedFour = SpiderPlainSpell(book, 4, 0, true);
            AbilityData source = SpiderPrepareExtended(book, unrelatedFour, 4);
            book.Rest();
            int equalCopies = Pol06EqualPreparedCopies(book, source);
            Sprint25Check("pol06-druid-source-unique", equalCopies == 1 && book.GetSpellLevel(source) == 5, new JObject { ["sourceSpell"] = unrelatedFour.name + ":" + unrelatedFour.AssetGuid,
                ["equalPreparedCopies"] = equalCopies }, "The Druid POL-06 source is the only prepared slot whose spell equals it.");
            foreach (int frame in SpiderInterfaceSelect(casters.Druid)) yield return frame;
            AlphaMenu menu = null; ActionBarGroupSlot popupSource = null;
            foreach (int frame in Pol06Popup("pol06-druid-cotw-on", spellGroup, levels, book, source, SummonFamily.NaturesAlly, snaParent, mode, watch,
                "The Druid's popup over an unrelated level 4 spell prepared with Extend Spell keeps the ordinary Summon Nature's Ally V rows exactly and each Call of the Wild " +
                "Summon Nature's Ally IV metamagic conversion owns exactly the published creatures of its own quantity, each with that conversion's own metamagic object at spell level 5; none stays raw.",
                (value, slot, verdict, exact) => { menu = value; popupSource = slot; })) yield return frame;
            AbilityData chosen = Pol06Pick(menu, source, SummonFamily.NaturesAlly, SummonMultiplicity.OneD3);
            AbilityData retained = chosen == null ? null : menu.Data.FirstOrDefault(value => !ReferenceEquals(value, chosen) && ReferenceEquals(value.ConvertedFrom, chosen.ConvertedFrom));
            if (chosen != null && popupSource != null)
            {
                JObject cast = null;
                foreach (int frame in Pol06Cast(fixture, casters.Druid, menu, popupSource, source, chosen, SummonFamily.NaturesAlly, snaParent, mode, watch, "pol06-druid-d3",
                    value => cast = value)) yield return frame;
                popupSource = null;
                string problem = SpiderDurationProblem(cast["rules"] as JArray, book.CasterLevel, true);
                cast["durationProblem"] = problem;
                Sprint25Check("pol06-druid-d3-count-duration-debit", cast["error"] == null && problem == null &&
                    (int)cast["summoned"] == 2 && (bool)cast["exactlyOneSourceSlot"] && (cast["rolls"] as JArray)?.Count >= 1, cast,
                    "A projected d3 Summon Nature's Ally IV choice under Call of the Wild's Extend row, its d3 forced to 2, summons exactly two creatures (the contract, not the " +
                    "observed count), each for twice the Druid's caster level in rounds, and spends exactly the source slot.");
                if (retained != null)
                {
                    UnitEntityData fixtureCaster = fixture.Caster; fixture.Caster = casters.Druid;
                    try
                    {
                        foreach (int frame in R6Cell("pol06-druid-stale-refused", () => ClericRefusedAttempt(fixture, retained, source, mode, watch, "pol06-druid-stale-choice-refused", "pol06-druid-stale",
                            true, "a projected Druid choice retained from the popup, after its source slot was spent; native UnitUseAbility",
                            "A projected Druid choice retained after its source slot was spent has no cast left and is refused with nothing spent and nothing summoned.", true))) yield return frame;
                    }
                    finally { fixture.Caster = fixtureCaster; }
                }
                else Sprint25Check("pol06-druid-stale-choice-present", false, new JObject { ["rows"] = menu.Rows.Length }, "A second projected Druid choice of the same conversion exists to retain.");
            }
            else Sprint25Check("pol06-druid-d3-row-present", false, new JObject { ["rows"] = menu?.Rows.Length ?? 0 }, "A projected d3 Druid choice is offered.");
            if (popupSource != null) Cleanup2MenuAdapterHide(popupSource, source);
            book.Rest();
            foreach (int frame in SpiderInterfaceSelect(fixture.Caster)) yield return frame;
        }

        /// <summary>The natural cleric 9/wizard 1 Herald over a plain level 5
        /// source with no metamagic: Call of the Wild's Extend rows still project
        /// with Extend, and a cast spends one cleric slot, the wizard book unchanged.</summary>
        private IEnumerable<int> Pol06NaturalHeraldCells(ExpandedSummoningCorrectionFixture fixture, SpiderInterfaceCasters casters, Cleanup2NativeDefaultScope mode,
            ActionBarGroupElement spellGroup, ActionBarSubGroupLevels levels, Func<int, BlueprintAbility> smParent)
        {
            var watch = Stopwatch.StartNew();
            Spellbook clericBook = casters.HeraldBook, wizardBook = casters.HeraldWizardBook;
            if (clericBook == null || wizardBook == null || Pol06CotwSetting() == null)
            { Sprint25Check("pol06-natural-herald-prerequisites", false, casters.Setup, "The natural Herald has its cleric and wizard books and Call of the Wild's setting exists."); yield break; }
            BlueprintFeature extendFeat = SpiderExtendSpellFeat();
            bool extendGranted = !casters.Herald.Descriptor.HasFact(extendFeat);
            if (extendGranted) casters.Herald.Descriptor.AddFact(extendFeat);
            try
            {
                BlueprintAbility plainFive = SpiderPlainSpell(clericBook, 5, 2);
                PrepareExpandedSummoningPlayerPathSpell(clericBook, plainFive, plainFive, 5);
                // The wizard book is populated: one prepared level 1 spell.
                BlueprintAbility wizardSpell = wizardBook.Blueprint.SpellList.GetSpells(1).Where(value => value != null && (value.Variants == null || value.Variants.Length == 0))
                    .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).First();
                if (wizardBook.GetSpellLevel(wizardSpell) < 0) wizardBook.AddKnown(1, wizardSpell, true);
                wizardBook.Memorize(new AbilityData(wizardSpell, wizardBook), null);
                clericBook.Rest(); wizardBook.Rest();
                AbilityData source = clericBook.GetMemorizedSpellSlots(5).Where(value => value.Spell != null && ReferenceEquals(value.Spell.Blueprint, plainFive)).Select(value => value.Spell).FirstOrDefault();
                SpellSlot[] wizardSlots = Enumerable.Range(0, 10).SelectMany(level => wizardBook.GetMemorizedSpellSlots(level)).ToArray();
                Func<JArray> wizardState = () => new JArray(Enumerable.Range(0, 10).SelectMany(level => wizardBook.GetMemorizedSpellSlots(level))
                    .Select(value => (value.Spell?.Blueprint?.AssetGuid ?? "empty") + ":" + value.Available));
                int wizardPrepared = wizardSlots.Count(value => value.Spell != null && value.Available);
                bool plain = source != null && !Pol06HasMetamagic(source) && clericBook.GetSpellLevel(source) == 5 && Pol06EqualPreparedCopies(clericBook, source) == 1;
                Sprint25Check("pol06-natural-herald-plain-source", plain && wizardPrepared >= 1 && casters.Herald.Descriptor.HasFact(extendFeat), new JObject {
                    ["sourceSpell"] = plainFive.name + ":" + plainFive.AssetGuid, ["sourceMetamagic"] = source == null ? null : Pol06Key(source),
                    ["equalPreparedCopies"] = source == null ? 0 : Pol06EqualPreparedCopies(clericBook, source), ["wizardPreparedAvailable"] = wizardPrepared,
                    ["wizardSpell"] = wizardSpell.name, ["extendFeatHeld"] = casters.Herald.Descriptor.HasFact(extendFeat), ["extendFeatGranted"] = extendGranted, ["setup"] = casters.Setup },
                    "The natural Herald's source is a plain level 5 cleric spell with no metamagic, the only equal preparation; its wizard book holds a prepared spell; it holds Extend Spell.");
                if (!plain) yield break;
                foreach (int frame in SpiderInterfaceSelect(casters.Herald)) yield return frame;
                AlphaMenu menu = null; ActionBarGroupSlot popupSource = null;
                foreach (int frame in Pol06Popup("pol06-natural-herald-plain", spellGroup, levels, clericBook, source, SummonFamily.Monster, smParent, mode, watch,
                    "Over a plain level 5 source with no metamagic, the natural Herald's popup keeps its ordinary rows and its cure rows, and each Call of the Wild Summon Monster IV " +
                    "Extend conversion owns exactly the permitted creatures of its own quantity with Extend at spell level 5: the conversion's metamagic does not come from the source.",
                    (value, slot, verdict, exact) => { menu = value; popupSource = slot; })) yield return frame;
                AbilityData chosen = Pol06Pick(menu, source, SummonFamily.Monster, SummonMultiplicity.One, value => value.MetamagicData.Has(Kingmaker.UnitLogic.Abilities.Metamagic.Extend));
                if (chosen != null && popupSource != null)
                {
                    JArray wizardBefore = wizardState(); JObject cast = null;
                    foreach (int frame in Pol06Cast(fixture, casters.Herald, menu, popupSource, source, chosen, SummonFamily.Monster, smParent, mode, watch, "pol06-natural-herald",
                        value => cast = value)) yield return frame;
                    popupSource = null;
                    JArray wizardAfter = wizardState();
                    string problem = SpiderDurationProblem(cast["rules"] as JArray, clericBook.CasterLevel, true);
                    bool wizardUnchanged = JToken.DeepEquals(wizardBefore, wizardAfter);
                    cast["durationProblem"] = problem; cast["wizardBefore"] = wizardBefore; cast["wizardUnchanged"] = wizardUnchanged;
                    cast["childBook"] = chosen.Spellbook?.Blueprint?.AssetGuid; cast["clericBook"] = clericBook.Blueprint.AssetGuid; cast["sourceHasMetamagic"] = Pol06HasMetamagic(source);
                    Sprint25Check("pol06-natural-herald-plain-source-extend-cast", cast["error"] == null && problem == null &&
                        clericBook.CasterLevel == 9 && ReferenceEquals(chosen.Spellbook, clericBook) && wizardUnchanged && (int)cast["summoned"] == 1 && (bool)cast["exactlyOneSourceSlot"] &&
                        !Pol06HasMetamagic(source), cast,
                        "From the plain source, the projected Call of the Wild Extend choice is Extended (18 rounds at cleric caster level 9), one creature; exactly the cleric source slot is spent " +
                        "and every wizard slot and preparation is unchanged.");
                }
                else Sprint25Check("pol06-natural-herald-row-present", false, new JObject { ["rows"] = menu?.Rows.Length ?? 0 }, "A projected Single Extend choice is offered to the natural Herald.");
                if (popupSource != null) Cleanup2MenuAdapterHide(popupSource, source);
                clericBook.Rest(); wizardBook.Rest();
            }
            finally { if (extendGranted && casters.Herald.Descriptor.HasFact(extendFeat)) casters.Herald.Descriptor.RemoveFact(extendFeat); }
            foreach (int frame in SpiderInterfaceSelect(fixture.Caster)) yield return frame;
        }
    }
}
