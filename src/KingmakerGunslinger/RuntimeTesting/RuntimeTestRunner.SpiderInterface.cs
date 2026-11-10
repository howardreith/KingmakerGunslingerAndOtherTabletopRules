using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Rendered spider menus and Herald casts in the cleric phase's native
    /// Default fixture (RU-15, RU-10, RU-09):
    /// <list type="bullet">
    /// <item>the real Summon Nature's Ally V and Druid conversion popups of a
    /// naturally levelled Drow Druid that selected the feat, and of a Druid
    /// without it;</item>
    /// <item>Crab I and Black Widow IV in the Herald popups over unrelated
    /// prepared spells;</item>
    /// <item>representative metamagic: a spider cast from a Summon Monster V
    /// prepared with Extend Spell, and a Herald spider cast whose unrelated
    /// source was prepared with Extend Spell;</item>
    /// <item>a natural cleric/wizard Herald Caller's spider cast from its cleric
    /// book, its wizard book unchanged.</item>
    /// </list>
    /// Every cast asserts the caster level and the summon duration from the
    /// engine's own summon rule alongside its source chain and debit.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class SpiderInterfaceCasters
        {
            internal UnitEntityData Druid, PlainDruid, Herald;
            internal Spellbook DruidBook, PlainBook, HeraldBook, HeraldWizardBook;
            internal readonly List<UnitEntityData> AddedToParty = new List<UnitEntityData>();
            internal JObject Setup = new JObject();
        }

        /// <summary>Creates and levels the three extra casters and adds them to
        /// the party before the native Default scope is entered.</summary>
        private SpiderInterfaceCasters SpiderInterfaceSetup(ExpandedSummoningCorrectionFixture fixture)
        {
            var library = BlueprintBootstrap.Library;
            BlueprintScriptableObject found;
            var feature = library.BlueprintsByAssetId.TryGetValue(SummonForeignRowClassifier.SpiderSummonerFeatureGuid, out found) ? found as BlueprintFeature : null;
            if (feature == null) return null;
            Func<string, BlueprintCharacterClass> cls = guid => BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, guid, "spider interface class");
            var casters = new SpiderInterfaceCasters();
            casters.Druid = SpiderRouteCaster(fixture, "KMG_Runtime_Spider_UiDruid", Vector3.left * 2f + Vector3.forward * 2f, Alignment.TrueNeutral);
            casters.Druid.Stats.Wisdom.BaseValue = 30;
            casters.Setup["druid"] = SpiderNaturalLevels(casters.Druid, cls(SpiderDruidClass), 13,
                BlueprintLibraryLookup.RequireExact<BlueprintRace>(library, SpiderDrowRace, "Drow race"), null, feature, Alignment.TrueNeutral);
            casters.PlainDruid = SpiderRouteCaster(fixture, "KMG_Runtime_Spider_UiPlainDruid", Vector3.left * 2f + Vector3.back * 2f, Alignment.TrueNeutral);
            casters.PlainDruid.Stats.Wisdom.BaseValue = 30;
            casters.Setup["plainDruid"] = SpiderNaturalLevels(casters.PlainDruid, cls(SpiderDruidClass), 9, null, null, null, Alignment.TrueNeutral);
            casters.Herald = SpiderRouteCaster(fixture, "KMG_Runtime_Spider_UiHerald", Vector3.right * 2f + Vector3.back * 2f, Alignment.NeutralGood);
            casters.Herald.Stats.Wisdom.BaseValue = 30;
            casters.Setup["heraldCleric"] = SpiderNaturalLevels(casters.Herald, cls(SpiderClericClass), 9, null,
                BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library, SpiderHeraldArchetype, "Herald Caller"), null, Alignment.NeutralGood);
            casters.Setup["heraldWizard"] = SpiderNaturalLevels(casters.Herald, cls(SpiderWizardClass), 1, null, null, null, Alignment.NeutralGood);
            casters.Herald.Descriptor.AddFact(feature);
            // Native level-up may take companions; they are not part of this fixture.
            var masters = new HashSet<UnitEntityData> { casters.Druid, casters.PlainDruid, casters.Herald };
            UnitEntityData[] pets = Game.Instance.State.Units.All.Where(value => value != null && masters.Contains(value.Descriptor.Master.Value)).ToArray();
            foreach (UnitEntityData pet in pets) if (!fixture.Created.Contains(pet)) fixture.Created.Add(pet);
            if (pets.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, pets);
            casters.Setup["petsDisposed"] = pets.Length;
            casters.DruidBook = casters.Druid.Descriptor.GetSpellbook(cls(SpiderDruidClass));
            casters.PlainBook = casters.PlainDruid.Descriptor.GetSpellbook(cls(SpiderDruidClass));
            casters.HeraldBook = casters.Herald.Descriptor.GetSpellbook(cls(SpiderClericClass));
            casters.HeraldWizardBook = casters.Herald.Descriptor.GetSpellbook(cls(SpiderWizardClass));
            foreach (Spellbook book in new[] { casters.DruidBook, casters.PlainBook, casters.HeraldBook, casters.HeraldWizardBook }.Where(value => value != null))
                book.UpdateAllSlotsSize(false);
            var player = Game.Instance.Player;
            foreach (UnitEntityData unit in new[] { casters.Druid, casters.PlainDruid, casters.Herald })
                if (!player.Party.Contains(unit)) { player.PartyCharacters.Add(unit); casters.AddedToParty.Add(unit); }
            player.InvalidateCharacterLists(); player.UpdateCharacterLists();
            casters.Setup["druidHasFeat"] = SpiderHasFeature(casters.Druid, feature);
            casters.Setup["plainDruidHasFeat"] = SpiderHasFeature(casters.PlainDruid, feature);
            casters.Setup["heraldArchetype"] = casters.Herald.Descriptor.Progression.IsArchetype(
                BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library, SpiderHeraldArchetype, "Herald Caller"));
            casters.Setup["heraldConversion"] = casters.Herald.Descriptor.HasFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SpiderHeraldConversion, "Herald conversion"));
            casters.Setup["heraldBooks"] = new JArray(casters.Herald.Descriptor.Spellbooks.Select(value => value.Blueprint.AssetGuid));
            return casters;
        }

        /// <summary>Removes the extra casters from the party and disposes them.</summary>
        private void SpiderInterfaceTeardown(ExpandedSummoningCorrectionFixture fixture, SpiderInterfaceCasters casters)
        {
            if (casters == null)
            {
                foreach (BlueprintUnit blueprint in _spiderRouteBlueprints) UnityEngine.Object.Destroy(blueprint);
                _spiderRouteBlueprints.Clear();
                return;
            }
            var player = Game.Instance.Player;
            foreach (UnitEntityData unit in casters.AddedToParty) player.PartyCharacters.Remove(unit);
            player.InvalidateCharacterLists(); player.UpdateCharacterLists();
            var masters = new HashSet<UnitEntityData> { casters.Druid, casters.PlainDruid, casters.Herald };
            UnitEntityData[] owned = Game.Instance.State.Units.All.Where(value => value != null && !value.Destroyed &&
                (masters.Contains(value) || masters.Contains(value.Descriptor.Master.Value))).ToArray();
            foreach (UnitEntityData unit in owned) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
            if (owned.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, owned);
            foreach (BlueprintUnit blueprint in _spiderRouteBlueprints) UnityEngine.Object.Destroy(blueprint);
            _spiderRouteBlueprints.Clear();
        }

        /// <summary>Runs one cast with the engine's summon rules captured.</summary>
        private IEnumerable<int> SpiderSummonRuleScope(Func<IEnumerable<int>> cast, Action<JArray> report)
        {
            var hooks = HarmonyInstance.Create("kmg.spider.summon-rule." + _request.RunId + "." + Time.frameCount);
            var records = new JArray();
            try
            {
                hooks.Patch(typeof(RuleSummonUnit).GetMethod("OnTrigger", BindingFlags.Public | BindingFlags.Instance), null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("SpiderSummonRulePostfix", BindingFlags.Static | BindingFlags.NonPublic)), null);
                SpiderSummonRules.Clear(); _spiderSummonRuleCapture = true;
                foreach (int frame in cast()) yield return frame;
            }
            finally
            {
                _spiderSummonRuleCapture = false; hooks.UnpatchAll(hooks.Id);
                foreach (RuleSummonUnit rule in SpiderSummonRules) records.Add(SpiderSummonRuleRecord(rule));
                SpiderSummonRules.Clear();
            }
            report(records);
        }

        private static BlueprintAbility SpiderPlainSpell(Spellbook book, int level, int index)
        {
            return book.Blueprint.SpellList.GetSpells(level).Where(value => value != null && (value.Variants == null || value.Variants.Length == 0) &&
                !ExpandedSummoningPublisher.CanonicalParentGuids.Contains(value.AssetGuid)).OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ElementAt(index);
        }

        /// <summary>Prepares this spell with Extend Spell in its raised slot.</summary>
        private static AbilityData SpiderPrepareExtended(Spellbook book, BlueprintAbility spell, int baseLevel)
        {
            var metamagic = new MetamagicData { SpellLevelCost = Metamagic.Extend.DefaultCost() };
            metamagic.Add(Metamagic.Extend);
            if (book.GetSpellLevel(spell) < 0) book.AddKnown(baseLevel, spell, true);
            if (!book.Memorize(new AbilityData(spell, book) { MetamagicData = metamagic }, null))
                throw new InvalidOperationException("The native spellbook rejected an extended " + spell.name + ".");
            int slotLevel = baseLevel + metamagic.SpellLevelCost;
            SpellSlot slot = book.GetMemorizedSpellSlots(slotLevel).FirstOrDefault(value => value?.Spell != null && ReferenceEquals(value.Spell.Blueprint, spell) &&
                value.Spell.MetamagicData != null && value.Spell.MetamagicData.Has(Metamagic.Extend));
            if (slot == null) throw new InvalidOperationException("No extended slot was memorized for " + spell.name + ".");
            slot.Available = true; slot.Spell.ParamSpellSlot = slot;
            return slot.Spell;
        }

        private IEnumerable<int> SpiderInterfaceSelect(UnitEntityData unit)
        {
            Game.Instance.UI.SelectionManagerPC.SelectUnit(unit.View, true, true, false);
            GroupController.Instance.SelectUnit(unit);
            for (int frame = 0; frame < 12; frame++) yield return 0;
        }

        private IEnumerable<int> SpiderInterfaceCells(ExpandedSummoningCorrectionFixture fixture, SpiderInterfaceCasters casters, Spellbook book,
            BlueprintFeature herald, Cleanup2NativeDefaultScope mode, NativeIconScreenEvidence screens, ActionBarGroupElement spellGroup,
            ActionBarSubGroupLevels levels, Func<int, BlueprintAbility> smParent, Action<BlueprintFeature> setDeity, BlueprintFeature desna)
        {
            var library = BlueprintBootstrap.Library;
            var watch = Stopwatch.StartNew();
            Func<int, BlueprintAbility> snaParent = tier => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, ExpandedSummoningPublisher.CanonicalParentGuids[9 + tier - 1], "Nature's Ally parent");
            Func<SummonFamily, string, BlueprintAbility> original = (family, key) => (BlueprintAbility)library.BlueprintsByAssetId[SpiderSummonerCatalog.RowIdentity(family, SpiderSummonerCatalog.Find(key))];
            BlueprintAbility ogre = original(SummonFamily.Monster, "ogre-spider");
            string ogreUnit = SpiderSummonerCatalog.Find("ogre-spider").UnitGuid;

            // RU-10 on the granted-feature Herald cleric (caster level 20), chaotic good under Desna.
            fixture.Caster.Descriptor.Alignment.Set(Alignment.ChaoticGood);
            setDeity(desna);
            var heraldFact = fixture.Caster.Descriptor.AddFact(herald);
            try
            {
                // Crab I and Black Widow IV admission in the Herald popups over unrelated prepared spells.
                foreach (var admission in new[] { new { Tier = 1, Key = "giant-crab-spider" }, new { Tier = 4, Key = "giant-black-widow" } })
                {
                    var scoped = admission;
                    BlueprintAbility unrelated = SpiderPlainSpell(book, scoped.Tier, 0);
                    PrepareExpandedSummoningPlayerPathSpell(book, unrelated, unrelated, scoped.Tier);
                    book.Rest();
                    AbilityData source = SpiderPrepared(book, unrelated, scoped.Tier);
                    AlphaMenu menu = null; ActionBarGroupSlot popupSource = null;
                    foreach (int frame in R6Cell("spider-herald-admission-" + scoped.Tier, () => ClericOpenMenu("spider-herald-admission-tier" + scoped.Tier, spellGroup, levels, scoped.Tier, book,
                        smParent(scoped.Tier), source, mode, watch, null, (value, slot) => { menu = value; popupSource = slot; }, false))) yield return frame;
                    BlueprintAbility row = original(SummonFamily.Monster, scoped.Key);
                    AbilityData child = menu?.Data.FirstOrDefault(value => ReferenceEquals(value?.Blueprint, row));
                    bool admitted = child != null && ReferenceEquals(child.Spellbook, book) &&
                        (ReferenceEquals(child.ConvertedFrom, source) || ReferenceEquals(child.ConvertedFrom?.ConvertedFrom, source)) && row.Icon != null;
                    Sprint25Check("spider-herald-admission-" + scoped.Key + "-" + scoped.Tier, admitted, new JObject { ["tier"] = scoped.Tier, ["row"] = row.AssetGuid,
                        ["rows"] = menu == null ? 0 : menu.Rows.Length, ["chain"] = child == null ? null : new JArray(child.ConvertedFrom?.Blueprint?.AssetGuid, child.ConvertedFrom?.ConvertedFrom?.Blueprint?.AssetGuid),
                        ["conversionReceipt"] = child != null && DruidSummonConversionVariantPatch.For(child) != null, ["icon"] = row.Icon != null },
                        "The Herald popup over an unrelated prepared spell of this level offers the original " + scoped.Key + " row with an icon, on the cleric book, chained to that source.");
                    if (popupSource != null) Cleanup2MenuAdapterHide(popupSource, source);
                    book.Rest();
                }

                // Representative metamagic: a Summon Monster V prepared with Extend Spell. The spider is
                // a variant of the prepared spell, so it keeps the Extend and the duration doubles.
                AbilityData extendedParent = SpiderPrepareExtended(book, smParent(5), 5);
                book.Rest();
                AlphaMenu extendedMenu = null; ActionBarGroupSlot extendedSource = null;
                foreach (int frame in R6Cell("spider-extended-popup", () => ClericOpenMenu("spider-extended-sm5", spellGroup, levels, 6, book, smParent(5), extendedParent, mode, watch, null,
                    (value, slot) => { extendedMenu = value; extendedSource = slot; }, false))) yield return frame;
                int extendedIndex = extendedMenu == null ? -1 : Array.FindIndex(extendedMenu.Data, value => ReferenceEquals(value?.Blueprint, ogre));
                if (extendedIndex >= 0 && extendedSource != null)
                {
                    AbilityData child = extendedMenu.Data[extendedIndex];
                    var debit = new Cleanup2SourceDebit(fixture.Caster); JArray rules = null; ActionBarGroupSlot close = extendedSource;
                    foreach (int frame in SpiderSummonRuleScope(() => R6Cell("spider-extended-cast", () => ClericLegalCast(fixture, extendedMenu.Rows[extendedIndex], child, extendedParent, debit,
                        () => Cleanup2MenuAdapterHide(close, extendedParent), mode, watch, "spider-extended-ogre", ogreUnit)), value => rules = value)) yield return frame;
                    string problem = SpiderDurationProblem(rules, book.CasterLevel, true);
                    Sprint25Check("spider-extended-caster-level-duration", problem == null && book.CasterLevel == 20 && child.MetamagicData != null && child.MetamagicData.Has(Metamagic.Extend) &&
                        extendedParent.ParamSpellSlot != null && !extendedParent.ParamSpellSlot.Available,
                        new JObject { ["rules"] = rules, ["problem"] = problem, ["bookCasterLevel"] = book.CasterLevel, ["childMetamagic"] = child.MetamagicData?.MetamagicMask.ToString(),
                            ["slotLevel"] = extendedParent.ParamSpellSlot?.SpellLevel, ["slotSpent"] = extendedParent.ParamSpellSlot != null && !extendedParent.ParamSpellSlot.Available },
                        "The Ogre cast from a Summon Monster V prepared with Extend Spell carries Extend, and the engine's summon rule gives 40 rounds (twice caster level 20); its level 6 slot is the one spent.");
                }
                else Sprint25Check("spider-extended-row-present", false, new JObject { ["rows"] = extendedMenu?.Rows.Length ?? 0 }, "The Ogre is offered under the extended Summon Monster V.");
                if (extendedSource != null) Cleanup2MenuAdapterHide(extendedSource, extendedParent);
                book.Rest();

                // The Herald conversion of an unrelated spell prepared with Extend Spell. A native
                // spontaneous conversion does not carry the source's metamagic (AbilityData copies it only
                // to a variant of the same spell), so the projected spider has the native conversion's own
                // metamagic: none, and the caster level's rounds. Its debit is the raised slot.
                BlueprintAbility unrelatedFour = SpiderPlainSpell(book, 4, 1);
                AbilityData extendedSourceSpell = SpiderPrepareExtended(book, unrelatedFour, 4);
                book.Rest();
                AlphaMenu conversionMenu = null; ActionBarGroupSlot conversionSource = null;
                foreach (int frame in R6Cell("spider-herald-extended-source-popup", () => ClericOpenMenu("spider-herald-extended-source-tier5", spellGroup, levels, 5, book, smParent(5),
                    extendedSourceSpell, mode, watch, null, (value, slot) => { conversionMenu = value; conversionSource = slot; }, false))) yield return frame;
                int conversionIndex = conversionMenu == null ? -1 : Array.FindIndex(conversionMenu.Data, value => ReferenceEquals(value?.Blueprint, ogre));
                if (conversionIndex >= 0 && conversionSource != null)
                {
                    AbilityData child = conversionMenu.Data[conversionIndex];
                    var debit = new Cleanup2SourceDebit(fixture.Caster); JArray rules = null; ActionBarGroupSlot close = conversionSource;
                    foreach (int frame in SpiderSummonRuleScope(() => R6Cell("spider-herald-extended-source-cast", () => Cleanup2MenuAdapterDruidCast(fixture, child, extendedSourceSpell, smParent(5),
                        conversionMenu.Rows[conversionIndex], debit, mode, watch, () => Cleanup2MenuAdapterHide(close, extendedSourceSpell), "spider-herald-extended-source")), value => rules = value)) yield return frame;
                    bool nativeMetamagic = ReferenceEquals(child.MetamagicData, child.ConvertedFrom?.MetamagicData) && (child.MetamagicData == null || !child.MetamagicData.Has(Metamagic.Extend));
                    string problem = SpiderDurationProblem(rules, book.CasterLevel, false);
                    Sprint25Check("spider-herald-extended-source-caster-level-duration", problem == null && nativeMetamagic && extendedSourceSpell.ParamSpellSlot.SpellLevel == 5,
                        new JObject { ["rules"] = rules, ["problem"] = problem, ["childMetamagic"] = child.MetamagicData?.MetamagicMask.ToString(),
                            ["conversionMetamagic"] = child.ConvertedFrom?.MetamagicData?.MetamagicMask.ToString(), ["sourceMetamagic"] = extendedSourceSpell.MetamagicData?.MetamagicMask.ToString(),
                            ["sourceSlotLevel"] = extendedSourceSpell.ParamSpellSlot.SpellLevel },
                        "A Herald spider converted from an unrelated level 4 spell prepared with Extend Spell is a Summon Monster V from that level 5 slot; like the native conversion it carries no Extend, and its summon rule gives 20 rounds at caster level 20.");
                }
                else Sprint25Check("spider-herald-extended-source-row-present", false, new JObject { ["rows"] = conversionMenu?.Rows.Length ?? 0 }, "The Ogre is offered in the Herald popup over the extended source.");
                if (conversionSource != null) Cleanup2MenuAdapterHide(conversionSource, extendedSourceSpell);
                book.Rest();
            }
            finally { if (heraldFact != null) fixture.Caster.Descriptor.RemoveFact(heraldFact); }

            // RU-10 and RU-09: the naturally levelled cleric/wizard Herald Caller casts from its cleric book.
            if (casters.HeraldBook != null && casters.HeraldWizardBook != null)
            {
                Spellbook clericBook = casters.HeraldBook, wizardBook = casters.HeraldWizardBook;
                BlueprintAbility unrelated = SpiderUnrelated(clericBook, 5);
                PrepareExpandedSummoningPlayerPathSpell(clericBook, unrelated, unrelated, 5);
                clericBook.Rest(); wizardBook.Rest();
                AbilityData source = SpiderPrepared(clericBook, unrelated, 5);
                foreach (int frame in SpiderInterfaceSelect(casters.Herald)) yield return frame;
                AlphaMenu menu = null; ActionBarGroupSlot popupSource = null;
                foreach (int frame in R6Cell("spider-natural-herald-popup", () => ClericOpenMenu("spider-natural-herald-tier5", spellGroup, levels, 5, clericBook, smParent(5), source, mode, watch, null,
                    (value, slot) => { menu = value; popupSource = slot; }, false))) yield return frame;
                int index = menu == null ? -1 : Array.FindIndex(menu.Data, value => ReferenceEquals(value?.Blueprint, ogre));
                if (index >= 0 && popupSource != null)
                {
                    AbilityData child = menu.Data[index];
                    bool[] wizardBefore = Enumerable.Range(0, 10).SelectMany(level => wizardBook.GetMemorizedSpellSlots(level)).Select(value => value.Available).ToArray();
                    UnitEntityData fixtureCaster = fixture.Caster;
                    var debit = new Cleanup2SourceDebit(casters.Herald); JArray rules = null; ActionBarGroupSlot close = popupSource;
                    fixture.Caster = casters.Herald;
                    try
                    {
                        foreach (int frame in SpiderSummonRuleScope(() => R6Cell("spider-natural-herald-cast", () => Cleanup2MenuAdapterDruidCast(fixture, child, source, smParent(5), menu.Rows[index], debit,
                            mode, watch, () => Cleanup2MenuAdapterHide(close, source), "spider-natural-herald")), value => rules = value)) yield return frame;
                    }
                    finally { fixture.Caster = fixtureCaster; }
                    bool[] wizardAfter = Enumerable.Range(0, 10).SelectMany(level => wizardBook.GetMemorizedSpellSlots(level)).Select(value => value.Available).ToArray();
                    string problem = SpiderDurationProblem(rules, clericBook.CasterLevel, false);
                    bool exact = problem == null && clericBook.CasterLevel == 9 && ReferenceEquals(child.Spellbook, clericBook) && wizardBefore.SequenceEqual(wizardAfter) &&
                        source.ParamSpellSlot != null && !source.ParamSpellSlot.Available;
                    Sprint25Check("spider-natural-herald-cleric-book", exact, new JObject { ["rules"] = rules, ["problem"] = problem, ["clericCasterLevel"] = clericBook.CasterLevel,
                        ["childBook"] = child.Spellbook?.Blueprint?.AssetGuid, ["clericBook"] = clericBook.Blueprint.AssetGuid, ["wizardBook"] = wizardBook.Blueprint.AssetGuid,
                        ["wizardSlotsUnchanged"] = wizardBefore.SequenceEqual(wizardAfter), ["wizardSlots"] = wizardBefore.Length, ["setup"] = casters.Setup },
                        "The naturally levelled cleric 9/wizard 1 Herald Caller casts the Ogre through its own Herald popup from its cleric book: 9 rounds at caster level 9, its cleric slot spent and its wizard book unchanged.");
                }
                else Sprint25Check("spider-natural-herald-row-present", false, new JObject { ["rows"] = menu?.Rows.Length ?? 0, ["setup"] = casters.Setup }, "The natural Herald popup offers the Ogre.");
                if (popupSource != null) Cleanup2MenuAdapterHide(popupSource, source);
            }
            else Sprint25Check("spider-natural-herald-books", false, casters.Setup, "The natural Herald Caller has a cleric book and a wizard book.");

            // RU-15: rendered Summon Nature's Ally V and Druid conversion popups, with and without the feat.
            foreach (var druidCase in new[] { new { Key = "feat", Unit = casters.Druid, Book = casters.DruidBook, Feat = true },
                new { Key = "no-feat", Unit = casters.PlainDruid, Book = casters.PlainBook, Feat = false } })
            {
                var scoped = druidCase;
                if (scoped.Book == null) { Sprint25Check("spider-druid-" + scoped.Key + "-book", false, casters.Setup, "The Druid has its book."); continue; }
                BlueprintAbility parent = snaParent(5);
                BlueprintAbility[] spiderRows = (parent.Variants ?? new BlueprintAbility[0]).Where(SpiderRowIdentity).ToArray();
                AbilityData prepared = PrepareExpandedSummoningPlayerPathSpell(scoped.Book, parent, parent, 5);
                BlueprintAbility unrelated = SpiderUnrelated(scoped.Book, 5);
                PrepareExpandedSummoningPlayerPathSpell(scoped.Book, unrelated, unrelated, 5);
                scoped.Book.Rest();
                prepared = SpiderPrepared(scoped.Book, parent, 5);
                AbilityData source = SpiderPrepared(scoped.Book, unrelated, 5);
                foreach (int frame in SpiderInterfaceSelect(scoped.Unit)) yield return frame;
                foreach (var popup in new[] { new { Key = "sna", Source = prepared }, new { Key = "conversion", Source = source } })
                {
                    var view = popup; AlphaMenu menu = null;
                    foreach (int frame in R6Cell("spider-druid-" + scoped.Key + "-" + view.Key, () => ClericOpenMenu("spider-druid-" + scoped.Key + "-" + view.Key + "-tier5", spellGroup, levels, 5,
                        scoped.Book, parent, view.Source, mode, watch, screens, (value, slot) => menu = value, true))) yield return frame;
                    AbilityData[] offered = menu == null ? new AbilityData[0] : menu.Data.Where(value => value != null && SpiderRowIdentity(value.Blueprint)).ToArray();
                    bool exact = menu != null && (scoped.Feat
                        ? spiderRows.Length > 0 && new HashSet<BlueprintAbility>(offered.Select(value => value.Blueprint)).SetEquals(spiderRows) && offered.Length == spiderRows.Length &&
                            offered.All(value => value.Blueprint.Icon != null)
                        : offered.Length == 0);
                    Sprint25Check("spider-druid-" + scoped.Key + "-" + view.Key + "-rendered", exact, new JObject { ["popup"] = view.Key, ["feat"] = scoped.Feat,
                        ["rows"] = menu == null ? 0 : menu.Rows.Length, ["spiderRowsPublished"] = new JArray(spiderRows.Select(value => value.Name)),
                        ["spiderRowsOffered"] = new JArray(offered.Select(value => value.Name)), ["blankIcons"] = offered.Count(value => value.Blueprint.Icon == null),
                        ["rendered"] = menu == null ? null : AlphaRowsEvidence(menu) },
                        scoped.Feat ? "The Drow Druid that selected the feat sees every published Summon Nature's Ally V spider row once, each with an icon, in the rendered " + view.Key + " popup."
                            : "A Druid without the feat sees no spider row in the rendered " + view.Key + " popup.");
                }
            }
            foreach (int frame in SpiderInterfaceSelect(fixture.Caster)) yield return frame;
        }
    }
}
