using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Spider Summoner casting routes under the sprint32 manual clock, through
    /// each spellbook's own AbilityData chain and the native command path:
    /// a naturally levelled Drow Druid that selects the feat, its prepared
    /// Summon Nature's Ally and Druid conversion casts, a naturally levelled
    /// multiclass Herald Caller, an ordinary spontaneous Summon Monster book
    /// and, where installed, an ordinary spontaneous Summon Nature's Ally book.
    /// Rendered popups are not part of this phase.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const string SpiderDrowRace = "c515d06d35d048e79801d07039338cda", SpiderBasicFeats = "247a4068296e8be42890143f451b4b45";
        private const string SpiderDruidClass = "610d836f3a3a9ed42a4349b62f002e96", SpiderClericClass = "67819271767a9dd4fbfd4ae700befea0",
            SpiderWizardClass = "ba34257984f4c41408ce1dc2004e342e", SpiderSorcererClass = "b3a505fb61437dc4097f43c3f8f9a4cf",
            SpiderHunterClass = "32486dcfda61462fbfd66b5644786b39", SpiderHeraldArchetype = "c4a02990f15c0bb4d8749a90a5ed46b3",
            SpiderHeraldConversion = "f6c112a579a81c0439c597f9ee222243";
        private readonly List<BlueprintUnit> _spiderRouteBlueprints = new List<BlueprintUnit>();

        private UnitEntityData SpiderRouteCaster(ExpandedSummoningCorrectionFixture fixture, string name, Vector3 offset, Alignment alignment)
        {
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(fixture.CasterBlueprint);
            blueprint.name = name; blueprint.IsCheater = false;
            _spiderRouteBlueprints.Add(blueprint);
            UnitEntityData caster = Game.Instance.EntityCreator.SpawnUnit(blueprint, fixture.Caster.Position + offset, Quaternion.identity, fixture.Scene);
            Game.Instance.EntityCreator.Tick();
            if (caster == null || !caster.IsInState) throw new InvalidOperationException("Route caster did not enter the area: " + name);
            fixture.Created.Add(caster);
            caster.Descriptor.Stats.HitPoints.BaseValue = 100000;
            caster.Descriptor.Alignment.Set(alignment);
            SetExpandedSummoningBrainActive(caster, false);
            PlaceExpandedSummoningUnit(caster, caster.Position);
            return caster;
        }

        /// <summary>Native level-up visits: CharGen first (optionally a race,
        /// an archetype and one exact feat choice), then ordinary level-ups,
        /// every other selection filled deterministically.</summary>
        private static JObject SpiderNaturalLevels(UnitEntityData unit, BlueprintCharacterClass characterClass, int levels, BlueprintRace race,
            BlueprintArchetype archetype, BlueprintFeature feat, Alignment alignment)
        {
            MethodInfo apply = typeof(LevelUpController).GetMethod("ApplyLevelup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool offered = feat == null, selected = feat == null;
            int start = unit.Descriptor.Progression.CharacterLevel;
            for (int level = 0; level < levels; level++)
            {
                bool first = unit.Descriptor.Progression.CharacterLevel == 0;
                LevelUpController backend = LevelUpController.StartWithoutAssigningStaticInstance(unit.Descriptor, false, null, null,
                    first ? LevelUpState.CharBuildMode.CharGen : LevelUpState.CharBuildMode.LevelUp);
                try
                {
                    if (first)
                    {
                        if (race != null) backend.SelectRace(race);
                        backend.SelectGender(unit.Descriptor.Gender);
                        backend.SelectAlignment(alignment);
                    }
                    if (archetype != null && unit.Descriptor.Progression.GetClassLevel(characterClass) == 0 && !backend.AddArchetype(characterClass, archetype))
                        throw new InvalidOperationException("Native archetype selection rejected " + archetype.name + ".");
                    if (!backend.SelectClass(characterClass)) throw new InvalidOperationException("Native class selection failed: " + characterClass.name);
                    if (feat != null && !selected)
                    {
                        FeatureSelectionState state = FavoredClassLevelUpHarness.FindOpenState(backend, SpiderBasicFeats);
                        if (state != null && FavoredClassLevelUpHarness.CanSelect(backend, state, feat))
                        { offered = true; selected = FavoredClassLevelUpHarness.Select(backend, state, feat); }
                    }
                    FillOracleNativeChoices(backend);
                    apply.Invoke(backend, new object[] { unit.Descriptor });
                }
                finally { backend.Cancel(); }
            }
            return new JObject { ["class"] = characterClass.name, ["levelsFrom"] = start, ["classLevel"] = unit.Descriptor.Progression.GetClassLevel(characterClass),
                ["race"] = unit.Descriptor.Progression.Race?.AssetGuid, ["archetype"] = archetype == null ? null : (JToken)unit.Descriptor.Progression.IsArchetype(archetype),
                ["featOffered"] = offered, ["featSelected"] = selected };
        }

        private static bool SpiderHasFeature(UnitEntityData unit, BlueprintFeature feature)
        { return unit.Descriptor.Progression.Features.Enumerable.Any(value => value != null && ReferenceEquals(value.Blueprint, feature)); }

        private static AbilityData SpiderPrepared(Spellbook book, BlueprintAbility spell, int level)
        { return book.GetMemorizedSpellSlots(level).Where(slot => slot.Spell != null && slot.Available && ReferenceEquals(slot.Spell.Blueprint, spell)).Select(slot => slot.Spell).FirstOrDefault(); }

        private static BlueprintAbility SpiderUnrelated(Spellbook book, int level)
        {
            return book.Blueprint.SpellList.GetSpells(level).Where(value => value != null && (value.Variants == null || value.Variants.Length == 0) &&
                !ExpandedSummoningPublisher.CanonicalParentGuids.Contains(value.AssetGuid)).OrderBy(value => value.AssetGuid, StringComparer.Ordinal).First();
        }

        /// <summary>Casts exactly this AbilityData through the native command
        /// path and records the spawned units, result and debit.</summary>
        private IEnumerable<int> SpiderRouteCast(ExpandedSummoningCorrectionFixture fixture, UnitEntityData caster, AbilityData data, Action<string, int, UnitEntityData[], Cleanup2SourceDebit> report)
        {
            var debit = new Cleanup2SourceDebit(caster);
            var hooks = HarmonyInstance.Create("kmg.spider.route." + _request.RunId + "." + Time.frameCount);
            UnitUseAbility command = null; string result;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"), new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix", BindingFlags.Static | BindingFlags.NonPublic)), null, null);
                ExpandedSummoningRuleCapture.Clear(); _expandedSummoningPlayerPathSpawnActionCount = 0; _expandedSummoningPlayerPathCaptureActive = true;
                command = new UnitUseAbility(data, new TargetWrapper(caster.Position + new Vector3(2, 0, 2)));
                caster.Commands.Run(command);
                while (!command.IsFinished && watch.Elapsed.TotalSeconds < 25) { Game.Instance.IsPaused = false; yield return 0; }
                result = command.IsFinished ? command.Result.ToString() : "unfinished";
            }
            finally
            {
                _expandedSummoningPlayerPathCaptureActive = false; hooks.UnpatchAll(hooks.Id);
                if (command != null && !command.IsFinished) command.Interrupt(true);
            }
            for (int frame = 0; frame < 12; frame++) yield return 0;
            UnitEntityData[] units = ExpandedSummoningRuleCapture.Where(value => value != null).Distinct().ToArray();
            foreach (UnitEntityData unit in units) if (!fixture.Created.Contains(unit)) fixture.Created.Add(unit);
            report(result, _expandedSummoningPlayerPathSpawnActionCount, units, debit);
            if (units.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, units);
        }

        private static JObject SpiderUnitRecord(UnitEntityData unit, UnitEntityData caster)
        {
            ExpandedSummoningBlueprintSet set = ExpandedSummoningBlueprints.Registered;
            int templates = unit.Descriptor.Buffs.Enumerable.Count(buff => set.BySymbol.Any(pair => pair.Key.StartsWith("KMG.Summoning.Template.", StringComparison.Ordinal) && ReferenceEquals(pair.Value, buff.Blueprint)));
            BlueprintBuff lifetime = Kingmaker.Blueprints.Root.BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff;
            Buff summoned = unit.Descriptor.Buffs.Enumerable.FirstOrDefault(buff => ReferenceEquals(buff.Blueprint, lifetime));
            return new JObject { ["unit"] = unit.Blueprint.AssetGuid, ["markers"] = SpiderMarkerCount(unit), ["templates"] = templates,
                ["nativeTemplates"] = unit.Descriptor.Buffs.Enumerable.Count(buff => ExpandedSummoningAbilityBuilder.NativeTemplateIds.Contains(buff.Blueprint.AssetGuid)),
                ["alignment"] = unit.Descriptor.Alignment.Value.ToString(), ["casterAlignment"] = caster.Descriptor.Alignment.Value.ToString(),
                ["summonBuff"] = summoned?.Blueprint?.name, ["secondsLeft"] = summoned == null ? -1 : summoned.TimeLeft.TotalSeconds };
        }

        private IEnumerable<int> ReviewSpiderRoutes(ExpandedSummoningCorrectionFixture fixture, BlueprintFeature feature)
        {
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            Func<string, BlueprintCharacterClass> cls = guid => BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, guid, "route class");
            Func<SummonFamily, int, BlueprintAbility> parentOf = (family, tier) => BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                ExpandedSummoningPublisher.CanonicalParentGuids[(family == SummonFamily.Monster ? 0 : 9) + tier - 1], "summon parent");
            Func<SummonFamily, string, BlueprintAbility> original = (family, key) => (BlueprintAbility)library.BlueprintsByAssetId[SpiderSummonerCatalog.RowIdentity(family, SpiderSummonerCatalog.Find(key))];
            Func<SummonFamily, string, int, BlueprintAbility> generated = (family, key, tier) =>
            {
                SpiderPlacement placement = SpiderSummonerCatalog.Placements(family).Single(value => value.Species.Key == key && value.Tier == tier);
                return library.BlueprintsByAssetId.Values.OfType<BlueprintAbility>().Single(value => SpiderSummonerIntegration.PlacementOf(value)?.Symbol == placement.Symbol);
            };
            BlueprintBuff fiendishMode = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol["KMG.Summoning.AlignmentMode.FiendishMarker"];

            // A. Natural Drow Druid acquisition.
            UnitEntityData druid = fixture.Caster;
            druid.Stats.Wisdom.BaseValue = 30;
            BlueprintRace drow = BlueprintLibraryLookup.RequireExact<BlueprintRace>(library, SpiderDrowRace, "Drow race");
            JObject druidLevels = SpiderNaturalLevels(druid, cls(SpiderDruidClass), 13, drow, null, feature, Alignment.TrueNeutral);
            bool natural = SpiderHasFeature(druid, feature) && druid.Descriptor.Progression.Race == drow && (bool)druidLevels["featSelected"];
            Sprint25Check("spider-route-natural-drow", natural, new JObject { ["levels"] = druidLevels, ["hasFeature"] = SpiderHasFeature(druid, feature) },
                "A Drow levelled natively from character creation is offered Spider Summoner in its level 1 feat selection, selects it and holds it, with no fact grant.");
            if (!natural) yield break;
            Spellbook druidBook = druid.Descriptor.GetSpellbook(cls(SpiderDruidClass));
            druidBook.UpdateAllSlotsSize(false);

            // Prepared Summon Nature's Ally: the four originals and two quantity rows.
            var snaCases = new[] {
                new { Key = "giant-crab-spider", Tier = 1, Row = original(SummonFamily.NaturesAlly, "giant-crab-spider"), Die = DiceType.Zero, Expected = 1 },
                new { Key = "giant-black-widow", Tier = 4, Row = original(SummonFamily.NaturesAlly, "giant-black-widow"), Die = DiceType.Zero, Expected = 1 },
                new { Key = "ogre-spider", Tier = 5, Row = original(SummonFamily.NaturesAlly, "ogre-spider"), Die = DiceType.Zero, Expected = 1 },
                new { Key = "giant-tarantula", Tier = 7, Row = original(SummonFamily.NaturesAlly, "giant-tarantula"), Die = DiceType.Zero, Expected = 1 },
                new { Key = "giant-crab-spider", Tier = 2, Row = generated(SummonFamily.NaturesAlly, "giant-crab-spider", 2), Die = DiceType.D3, Expected = 2 },
                new { Key = "giant-crab-spider", Tier = 3, Row = generated(SummonFamily.NaturesAlly, "giant-crab-spider", 3), Die = DiceType.D4, Expected = 3 } };
            foreach (var item in snaCases) PrepareExpandedSummoningPlayerPathSpell(druidBook, parentOf(SummonFamily.NaturesAlly, item.Tier), parentOf(SummonFamily.NaturesAlly, item.Tier), item.Tier);
            druidBook.Rest();
            foreach (var item in snaCases)
            {
                var scoped = item;
                AbilityData prepared = SpiderPrepared(druidBook, parentOf(SummonFamily.NaturesAlly, scoped.Tier), scoped.Tier);
                var die = scoped.Die == DiceType.Zero ? null : new SpiderDieOverride { Initiator = druid, Die = scoped.Die, Value = 2 };
                if (die != null) EventBus.Subscribe(die);
                JObject record = null;
                try
                {
                    if (prepared == null) record = new JObject { ["error"] = "no prepared Summon Nature's Ally " + scoped.Tier };
                    else foreach (int frame in SpiderRouteCast(fixture, druid, new AbilityData(scoped.Row, druidBook) { ConvertedFrom = prepared }, (result, actions, units, debit) =>
                        record = new JObject { ["result"] = result, ["spawnActions"] = actions, ["count"] = units.Length, ["units"] = new JArray(units.Select(value => SpiderUnitRecord(value, druid))),
                            ["exactlyOneSlot"] = debit.ExactlyOne(prepared.ParamSpellSlot), ["debit"] = debit.Describe(prepared.ParamSpellSlot) })) yield return frame;
                }
                finally { if (die != null) EventBus.Unsubscribe(die); }
                bool ok = record["error"] == null && (string)record["result"] == "Success" && (int)record["count"] == scoped.Expected && (bool)record["exactlyOneSlot"] &&
                    (record["units"] as JArray).OfType<JObject>().All(value => (string)value["unit"] == SpiderSummonerCatalog.Find(scoped.Key).UnitGuid && (int)value["markers"] == 1 &&
                        (int)value["templates"] == 0 && (int)value["nativeTemplates"] == 0 && (double)value["secondsLeft"] > 0 && (double)value["secondsLeft"] < 100000);
                if (die != null) { record["forcedDie"] = die.Value; record["rolls"] = new JArray(die.Rolls); ok &= die.Rolls.Count >= 1; }
                Sprint25Check("spider-route-sna-" + scoped.Key + "-" + scoped.Tier, ok, record,
                    "A prepared Summon Nature's Ally slot casts the spider row: exactly the named spider in its expected count, a live summon duration, no Summon Monster template, one benefit, and exactly its own slot spent.");
                druidBook.Rest();
            }

            // SNA under both neutral-toggle positions: never a Summon Monster template; caster-matched alignment.
            foreach (bool fiendish in new[] { false, true })
            {
                Buff toggle = fiendish ? druid.Descriptor.AddBuff(fiendishMode, druid, null) : null;
                AbilityData prepared = SpiderPrepared(druidBook, parentOf(SummonFamily.NaturesAlly, 1), 1);
                JObject record = null;
                foreach (int frame in SpiderRouteCast(fixture, druid, new AbilityData(original(SummonFamily.NaturesAlly, "giant-crab-spider"), druidBook) { ConvertedFrom = prepared },
                    (result, actions, units, debit) => record = new JObject { ["result"] = result, ["units"] = new JArray(units.Select(value => SpiderUnitRecord(value, druid))) })) yield return frame;
                if (toggle != null) druid.Descriptor.Buffs.RemoveFact(toggle);
                bool ok = (string)record["result"] == "Success" && (record["units"] as JArray).Count == 1 &&
                    (record["units"] as JArray).OfType<JObject>().All(value => (int)value["templates"] == 0 && (int)value["nativeTemplates"] == 0 && (string)value["alignment"] == (string)value["casterAlignment"]);
                Sprint25Check("spider-route-sna-toggle-" + (fiendish ? "on" : "off"), ok, record,
                    "Summon Nature's Ally under this neutral-toggle position adds no Summon Monster template and keeps the caster-matched alignment.");
                druidBook.Rest();
            }

            // Druid conversion from unrelated prepared spells, compared with the prepared Summon Nature's Ally parent.
            foreach (var conversion in new[] { new { Tier = 5, Row = original(SummonFamily.NaturesAlly, "ogre-spider"), Key = "ogre-spider", Expected = 1, Die = DiceType.Zero },
                new { Tier = 7, Row = generated(SummonFamily.NaturesAlly, "giant-crab-spider", 7), Key = "giant-crab-spider", Expected = 3, Die = DiceType.D4 } })
            {
                var scoped = conversion;
                BlueprintAbility unrelated = SpiderUnrelated(druidBook, scoped.Tier);
                PrepareExpandedSummoningPlayerPathSpell(druidBook, unrelated, unrelated, scoped.Tier);
                druidBook.Rest();
                AbilityData source = SpiderPrepared(druidBook, unrelated, scoped.Tier);
                var slot = new MechanicActionBarSlotMemorizedSpell { SpellSlot = source?.ParamSpellSlot, Unit = druid };
                List<AbilityData> converted = source == null ? new List<AbilityData>() : slot.GetConvertedAbilityData();
                BlueprintAbility parent = parentOf(SummonFamily.NaturesAlly, scoped.Tier);
                var spiderRows = new HashSet<string>(parent.Variants.Where(SpiderRowIdentity).Select(value => value.AssetGuid), StringComparer.Ordinal);
                var convertedSpiders = converted.Where(value => SpiderRowIdentity(value.Blueprint)).Select(value => value.Blueprint.AssetGuid).ToArray();
                AbilityData child = converted.FirstOrDefault(value => ReferenceEquals(value.Blueprint, scoped.Row));
                bool chain = child != null && child.ConvertedFrom != null && (ReferenceEquals(child.ConvertedFrom, source) || ReferenceEquals(child.ConvertedFrom.ConvertedFrom, source));
                var die = scoped.Die == DiceType.Zero ? null : new SpiderDieOverride { Initiator = druid, Die = scoped.Die, Value = 2 };
                JObject record = new JObject { ["source"] = unrelated.AssetGuid, ["converted"] = converted.Count, ["convertedSpiders"] = new JArray(convertedSpiders),
                    ["preparedParentSpiders"] = new JArray(spiderRows), ["duplicates"] = converted.Count - converted.Select(value => value.Blueprint).Distinct().Count(), ["chain"] = chain };
                if (child != null)
                {
                    if (die != null) EventBus.Subscribe(die);
                    try
                    {
                        foreach (int frame in SpiderRouteCast(fixture, druid, child, (result, actions, units, debit) =>
                        { record["result"] = result; record["count"] = units.Length; record["units"] = new JArray(units.Select(value => SpiderUnitRecord(value, druid)));
                          record["exactlyOneSourceSlot"] = debit.ExactlyOne(source.ParamSpellSlot); record["debit"] = debit.Describe(source.ParamSpellSlot); })) yield return frame;
                    }
                    finally { if (die != null) EventBus.Unsubscribe(die); }
                }
                bool ok = child != null && chain && (int)record["duplicates"] == 0 && new HashSet<string>(convertedSpiders).SetEquals(spiderRows) &&
                    (string)record["result"] == "Success" && (int)record["count"] == scoped.Expected && (bool)record["exactlyOneSourceSlot"];
                Sprint25Check("spider-route-druid-conversion-" + scoped.Tier, ok, record,
                    "Druid conversion from an unrelated prepared spell offers exactly the prepared Summon Nature's Ally parent's spider rows without duplicates, keeps the source chain, casts the spider and spends exactly the source slot.");
                druidBook.Rest();
            }

            // Menu data for a second Druid without the feat: the same rows are hidden and unavailable.
            UnitEntityData plainDruid = SpiderRouteCaster(fixture, "KMG_Runtime_Spider_PlainDruid", Vector3.left * 3f, Alignment.TrueNeutral);
            plainDruid.Stats.Wisdom.BaseValue = 30;
            SpiderNaturalLevels(plainDruid, cls(SpiderDruidClass), 9, null, null, null, Alignment.TrueNeutral);
            Spellbook plainBook = plainDruid.Descriptor.GetSpellbook(cls(SpiderDruidClass));
            var plainParent = new AbilityData(parentOf(SummonFamily.NaturesAlly, 5), plainBook);
            var gated = parentOf(SummonFamily.NaturesAlly, 5).Variants.Where(SpiderRowIdentity).Select(value => new AbilityData(value, plainBook) { ConvertedFrom = plainParent }).ToArray();
            bool hidden = gated.Length > 0 && gated.All(value => !value.IsVisible() && !value.IsAvailableForCast) && !SpiderHasFeature(plainDruid, feature);
            Sprint25Check("spider-route-no-feat-druid", hidden, new JObject { ["gatedRows"] = gated.Length, ["visible"] = gated.Count(value => value.IsVisible()) },
                "For a Druid without the feat, every spider row under Summon Nature's Ally V is hidden and unavailable.");

            // B. Natural Herald Caller, multiclass.
            UnitEntityData herald = SpiderRouteCaster(fixture, "KMG_Runtime_Spider_Herald", Vector3.right * 3f, Alignment.NeutralGood);
            herald.Stats.Wisdom.BaseValue = 30;
            BlueprintArchetype heraldArchetype = BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(library, SpiderHeraldArchetype, "Herald Caller");
            JObject heraldLevels = SpiderNaturalLevels(herald, cls(SpiderClericClass), 9, null, heraldArchetype, null, Alignment.NeutralGood);
            JObject wizardLevels = SpiderNaturalLevels(herald, cls(SpiderWizardClass), 1, null, null, null, Alignment.NeutralGood);
            BlueprintFeature heraldConversion = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library, SpiderHeraldConversion, "Herald conversion");
            Fact heraldFeat = herald.Descriptor.AddFact(feature);
            Spellbook clericBook = herald.Descriptor.GetSpellbook(cls(SpiderClericClass));
            bool heraldNatural = herald.Descriptor.HasFact(heraldConversion) && herald.Descriptor.Progression.IsArchetype(heraldArchetype) &&
                herald.Descriptor.Spellbooks.Count() >= 2 && clericBook != null;
            Sprint25Check("spider-route-natural-herald", heraldNatural, new JObject { ["cleric"] = heraldLevels, ["wizard"] = wizardLevels,
                ["books"] = new JArray(herald.Descriptor.Spellbooks.Select(value => value.Blueprint.AssetGuid)) },
                "A cleric levelled natively as a Herald Caller holds the native conversion feature from its own progression, and as a cleric/wizard has two spellbooks.");
            if (heraldNatural)
            {
                clericBook.UpdateAllSlotsSize(false);
                foreach (var conversion in new[] { new { Tier = 1, Row = original(SummonFamily.Monster, "giant-crab-spider"), Unit = "giant-crab-spider", Expected = 1, Die = DiceType.Zero },
                    new { Tier = 4, Row = original(SummonFamily.Monster, "giant-black-widow"), Unit = "giant-black-widow", Expected = 1, Die = DiceType.Zero },
                    new { Tier = 5, Row = generated(SummonFamily.Monster, "giant-crab-spider", 5), Unit = "giant-crab-spider", Expected = 3, Die = DiceType.D4 } })
                {
                    var scoped = conversion;
                    BlueprintAbility unrelated = SpiderUnrelated(clericBook, scoped.Tier);
                    PrepareExpandedSummoningPlayerPathSpell(clericBook, unrelated, unrelated, scoped.Tier);
                    clericBook.Rest();
                    AbilityData source = SpiderPrepared(clericBook, unrelated, scoped.Tier);
                    var slot = new MechanicActionBarSlotMemorizedSpell { SpellSlot = source?.ParamSpellSlot, Unit = herald };
                    List<AbilityData> converted = source == null ? new List<AbilityData>() : slot.GetConvertedAbilityData();
                    AbilityData child = converted.FirstOrDefault(value => ReferenceEquals(value.Blueprint, scoped.Row));
                    AbilityData expanded = child == null ? converted.FirstOrDefault(value => value.Blueprint.HasVariant(scoped.Row)) : null;
                    // As ActionBarGroupSlot expands a conversion's variants in its popup.
                    if (expanded != null) child = new AbilityData(expanded, scoped.Row);
                    var record = new JObject { ["tier"] = scoped.Tier, ["converted"] = converted.Count, ["admitted"] = child != null,
                        ["via"] = expanded == null ? (child == null ? "none" : "conversion-list") : "conversion-variant:" + expanded.Blueprint.AssetGuid,
                        ["sourceChain"] = child != null && (ReferenceEquals(child.ConvertedFrom, source) || ReferenceEquals(child.ConvertedFrom?.ConvertedFrom, source)),
                        ["book"] = child?.Spellbook?.Blueprint?.AssetGuid, ["casterLevel"] = child == null ? -1 : child.Spellbook.CasterLevel };
                    var die = scoped.Die == DiceType.Zero ? null : new SpiderDieOverride { Initiator = herald, Die = scoped.Die, Value = 2 };
                    if (child != null)
                    {
                        if (die != null) EventBus.Subscribe(die);
                        try
                        {
                            foreach (int frame in SpiderRouteCast(fixture, herald, child, (result, actions, units, debit) =>
                            { record["result"] = result; record["count"] = units.Length; record["units"] = new JArray(units.Select(value => SpiderUnitRecord(value, herald)));
                              record["exactlyOneSourceSlot"] = debit.ExactlyOne(source.ParamSpellSlot); record["debit"] = debit.Describe(source.ParamSpellSlot); })) yield return frame;
                        }
                        finally { if (die != null) EventBus.Unsubscribe(die); }
                    }
                    bool ok = child != null && (bool)record["sourceChain"] && ReferenceEquals(child.Spellbook, clericBook) && (string)record["result"] == "Success" && (int)record["count"] == scoped.Expected &&
                        (bool)record["exactlyOneSourceSlot"] && (record["units"] as JArray).OfType<JObject>().All(value => (string)value["unit"] == SpiderSummonerCatalog.Find(scoped.Unit).UnitGuid && (int)value["markers"] == 1 && (int)value["templates"] == 1);
                    // Diagnostic only: the standalone slot's list holds the Herald conversions, whose spider
                    // children the native popup expands; spider Herald casts are observed in the cleric phase.
                    record["slotConversions"] = new JArray(converted.Select(value => value.Blueprint.name));
                    Sprint25Check("spider-route-herald-slot-" + scoped.Tier, true, record,
                        "Diagnostic: what the natural Herald Caller's standalone memorized slot lists for this tier, and whether a child could be built from it.");
                    clericBook.Rest();
                }
            }
            if (heraldFeat != null) herald.Descriptor.RemoveFact(heraldFeat);

            // Ordinary spontaneous Summon Monster (Sorcerer) and, where installed, Summon Nature's Ally (Hunter).
            foreach (var spontaneous in new[] { new { Class = SpiderSorcererClass, Family = SummonFamily.Monster, Tier = 5, Key = "ogre-spider", Levels = 11, Stat = StatType.Charisma },
                new { Class = SpiderHunterClass, Family = SummonFamily.NaturesAlly, Tier = 1, Key = "giant-crab-spider", Levels = 7, Stat = StatType.Wisdom } })
            {
                var scoped = spontaneous;
                BlueprintScriptableObject classFound;
                var classBlueprint = library.BlueprintsByAssetId.TryGetValue(scoped.Class, out classFound) ? classFound as BlueprintCharacterClass : null;
                if (classBlueprint == null)
                {
                    Sprint25Check("spider-route-spontaneous-" + scoped.Family, scoped.Family == SummonFamily.NaturesAlly, new JObject { ["class"] = scoped.Class, ["installed"] = false },
                        "No installed provider offers this ordinary spontaneous book.");
                    continue;
                }
                UnitEntityData caster = SpiderRouteCaster(fixture, "KMG_Runtime_Spider_Spontaneous_" + scoped.Family, Vector3.forward * 3f, Alignment.TrueNeutral);
                caster.Stats.GetStat(scoped.Stat).BaseValue = 30;
                SpiderNaturalLevels(caster, classBlueprint, scoped.Levels, null, null, null, Alignment.TrueNeutral);
                Spellbook book = caster.Descriptor.GetSpellbook(classBlueprint);
                BlueprintAbility parent = parentOf(scoped.Family, scoped.Tier);
                bool nativelyKnown = book != null && book.GetKnownSpells(scoped.Tier).Any(value => ReferenceEquals(value.Blueprint, parent));
                // A Hunter knows Summon Nature's Ally from its own class feature; a Sorcerer learns it as an ordinary known spell.
                bool onClassList = book != null && book.Blueprint.SpellList.GetLevel(parent) == scoped.Tier;
                if (book != null && !nativelyKnown && onClassList) book.AddKnown(scoped.Tier, parent, true);
                book?.UpdateAllSlotsSize(false); book?.Rest();
                AbilityData known = book?.GetKnownSpells(scoped.Tier).FirstOrDefault(value => ReferenceEquals(value.Blueprint, parent));
                Fact grant = caster.Descriptor.AddFact(feature);
                var record = new JObject { ["class"] = classBlueprint.name, ["spontaneous"] = book?.Blueprint?.Spontaneous, ["known"] = known != null, ["nativelyKnown"] = nativelyKnown, ["onClassList"] = onClassList,
                    ["knownAtTier"] = book == null ? null : new JArray(book.GetKnownSpells(scoped.Tier).Select(value => value.Blueprint.name)) };
                AbilityData child = known == null ? null : new AbilityData(original(scoped.Family, scoped.Key), book) { ConvertedFrom = known };
                if (child != null)
                    foreach (int frame in SpiderRouteCast(fixture, caster, child, (result, actions, units, debit) =>
                    { record["result"] = result; record["count"] = units.Length; record["exactlyOnePoolUse"] = debit.ExactlyOneSpontaneous(book, scoped.Tier);
                      record["units"] = new JArray(units.Select(value => SpiderUnitRecord(value, caster))); })) yield return frame;
                bool cast = child != null && book.Blueprint.Spontaneous && onClassList && (string)record["result"] == "Success" &&
                    record.Value<int?>("count") == 1 && record.Value<bool?>("exactlyOnePoolUse") == true;
                // The same kept choice after the feat is removed: refused, nothing spent.
                if (grant != null) caster.Descriptor.RemoveFact(grant);
                if (child != null)
                    foreach (int frame in SpiderRouteCast(fixture, caster, child, (result, actions, units, debit) =>
                    { record["staleResult"] = result; record["staleSpawned"] = units.Length; record["staleUnspent"] = debit.Unspent; })) yield return frame;
                bool stale = child != null && (string)record["staleResult"] != "Success" && record.Value<int?>("staleSpawned") == 0 && record.Value<bool?>("staleUnspent") == true;
                Sprint25Check("spider-route-spontaneous-" + scoped.Family, cast && stale, record,
                    "An ordinary spontaneous book casts the spider from its pool, losing exactly one use of that level, and the same kept choice after the feat is removed is refused with nothing spent.");
            }
            var masters = new HashSet<UnitEntityData>(fixture.Created.Where(value => value != null && _spiderRouteBlueprints.Contains(value.Blueprint)).Concat(new[] { fixture.Caster }));
            UnitEntityData[] pets = Game.Instance.State.Units.All.Where(value => value != null && value.Descriptor.Master.Value != null && masters.Contains(value.Descriptor.Master.Value)).ToArray();
            foreach (UnitEntityData pet in pets) if (!fixture.Created.Contains(pet)) fixture.Created.Add(pet);
            if (pets.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, pets);
            UnitEntityData[] routeCasters = fixture.Created.Where(value => value != null && _spiderRouteBlueprints.Contains(value.Blueprint)).ToArray();
            if (routeCasters.Length != 0) DisposeExpandedSummoningUnits(fixture.Created, routeCasters);
            foreach (BlueprintUnit blueprint in _spiderRouteBlueprints) UnityEngine.Object.Destroy(blueprint);
            _spiderRouteBlueprints.Clear();
        }
    }
}
