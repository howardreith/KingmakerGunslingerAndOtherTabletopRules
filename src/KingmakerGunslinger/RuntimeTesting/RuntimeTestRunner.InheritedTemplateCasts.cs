using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// S5 T1 / DEF-05 representative native casts. Save-free native first area
    /// (the summoning native profile), disposable casters levelled natively:
    /// a level-20 Sorcerer casting canonical Summon Monster and Nature's Ally
    /// children through its own spontaneous slots (good or evil alignment as
    /// each case needs), and a Call of the Wild Master Summoner casting an
    /// adapter child through its real resource. Each completed cast records
    /// the source parent and child, the committed debit, quantity, unit
    /// identities, the four native template buffs and the project template by
    /// exact identity, duration, innate facts, the native summon marker as the
    /// unrelated-effect control, and the donor graph's own applications; one
    /// cast is cancelled before commit. Donor receipts and two unrelated
    /// published rows are compared before and after. Missing observations fail.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const string InheritedSorcererClass = "b3a505fb61437dc4097f43c3f8f9a4cf";
        private const string InheritedSmIV = "7ed74a3ec8c458d4fb50b192fd7be6ef";
        private const string InheritedSmVI = "e740afbab0147944dab35d83faa0ae1c";
        private const string InheritedSmVII = "ab167fd8203c1314bac6568932f1752f";
        private const string InheritedSnaII = "298148133cdc3fd42889b99c82711986";

        private sealed class InheritedCase
        {
            internal string Name;
            internal Alignment Alignment;
            internal int SpellLevel;
            internal string Parent;
            internal string ChildSymbol;
            internal string UnitSymbol;
            internal SummonMultiplicity Quantity;
            /// <summary>"none", "celestial" or "fiendish": the project template the cast must leave.</summary>
            internal string ProjectTemplate = "none";
        }

        private RuntimeTestResult RunInheritedTemplateCasts()
        {
            WriteLifecycleStage("inherited-template-casts-start");
            var assertions = new List<RuntimeTestAssertion>();
            var document = new JObject { ["scope"] = "Save-free native first area; disposable casters levelled by native LevelUpController visits; casts through spellbook slots or the Master Summoner resource and UnitUseAbility.", ["runId"] = _request.RunId };
            var cases = new JArray();
            document["cases"] = cases;
            ExpandedSummoningCorrectionFixture fixture = null;
            bool cleaned = false;
            IList<string> donorBefore = null, donorAfter = null;
            var unrelated = new Dictionary<string, InheritedTemplateDonorReceipt>(StringComparer.Ordinal);
            var unrelatedDifferences = new List<string>();
            try
            {
                AlternateSummoningFamilyStatus status = AlternateSummoningStatusRegistry.Find(
                    AlternateSummoningSourceCatalog.MasterSummonerKey);
                if (status == null || status.Disposition != AlternateSummoningDisposition.Published)
                    throw new InvalidOperationException("Master Summoner is not published: " +
                        (status == null ? "<no status>" : status.Disposition + " " + status.Reason));
                donorBefore = ExpandedSummoningAbilityBuilder.DonorReceiptDifferences();
                foreach (string symbol in new[] { "KMG.Summoning.Ability.SM.Tier9.Lemure.OneD4PlusOne",
                    "KMG.Summoning.Ability.SNA.Tier4.Wolf.OneD4PlusOne" })
                    unrelated[symbol] = ExpandedSummoningAbilityBuilder.CaptureReceipt(InheritedAbility(symbol));
                fixture = BeginExpandedSummoningCorrectionFixture("KMG_Runtime_InheritedTemplate_Casts");
                fixture.CasterBlueprint.IsCheater = false;

                UnitEntityData sorcerer = PlayerPathSecondCaster(fixture, "KMG_Runtime_InheritedTemplate_Sorcerer");
                sorcerer.Descriptor.Stats.Charisma.BaseValue = 30;
                var sorcererClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                    BlueprintBootstrap.Library, InheritedSorcererClass, "native Sorcerer");
                AdvanceDisposableArchetypeCaster(sorcerer.Descriptor, sorcererClass, null, 20, ref _playerPathLevelController);
                Spellbook book = sorcerer.Descriptor.Spellbooks.Single(value => ReferenceEquals(value.Blueprint, sorcererClass.Spellbook));
                while (book.CasterLevel < 20) book.AddCasterLevel();
                book.UpdateAllSlotsSize(false);
                book.Rest();
                document["sorcerer"] = new JObject { ["classLevel"] = sorcerer.Descriptor.Progression.GetClassLevel(sorcererClass), ["casterLevel"] = book.CasterLevel };

                var plan = new[]
                {
                    new InheritedCase { Name = "good-tier4-single-mephit", Alignment = Alignment.NeutralGood, SpellLevel = 4, Parent = InheritedSmIV,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier4.AirMephit.One", UnitSymbol = "KMG.Summoning.Unit.AirMephit", Quantity = SummonMultiplicity.One },
                    new InheritedCase { Name = "evil-tier4-single-mephit", Alignment = Alignment.NeutralEvil, SpellLevel = 4, Parent = InheritedSmIV,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier4.AirMephit.One", UnitSymbol = "KMG.Summoning.Unit.AirMephit", Quantity = SummonMultiplicity.One },
                    new InheritedCase { Name = "quantity-tier6-d4plus1-mephit", Alignment = Alignment.NeutralGood, SpellLevel = 6, Parent = InheritedSmVI,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier6.AirMephit.OneD4PlusOne", UnitSymbol = "KMG.Summoning.Unit.AirMephit", Quantity = SummonMultiplicity.OneD4PlusOne },
                    new InheritedCase { Name = "celestial-project-template", Alignment = Alignment.NeutralGood, SpellLevel = 4, Parent = InheritedSmIV,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier4.DireWolf.One", UnitSymbol = "KMG.Summoning.Unit.DireWolf", Quantity = SummonMultiplicity.One, ProjectTemplate = "celestial" },
                    new InheritedCase { Name = "fiendish-project-template", Alignment = Alignment.NeutralEvil, SpellLevel = 4, Parent = InheritedSmIV,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier4.DireWolf.One", UnitSymbol = "KMG.Summoning.Unit.DireWolf", Quantity = SummonMultiplicity.One, ProjectTemplate = "fiendish" },
                    new InheritedCase { Name = "tier7-untemplated-no-inherited", Alignment = Alignment.NeutralGood, SpellLevel = 7, Parent = InheritedSmVII,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier7.GreaterAirElemental.One", UnitSymbol = "KMG.Summoning.Unit.GreaterAirElemental", Quantity = SummonMultiplicity.One },
                    new InheritedCase { Name = "natures-ally-preservation", Alignment = Alignment.NeutralGood, SpellLevel = 2, Parent = InheritedSnaII,
                        ChildSymbol = "KMG.Summoning.Ability.SNA.Tier2.Wolf.One", UnitSymbol = "KMG.Summoning.Unit.Wolf", Quantity = SummonMultiplicity.One },
                };
                foreach (InheritedCase item in plan)
                {
                    var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, item.Parent, "native summon parent");
                    if (!book.GetKnownSpells(item.SpellLevel).Any(value => ReferenceEquals(value.Blueprint, parent)))
                        book.AddKnown(item.SpellLevel, parent, true);
                }
                foreach (InheritedCase item in plan)
                    InheritedTemplateSection(assertions, cases, item.Name, () =>
                    {
                        sorcerer.Descriptor.Alignment.Set(item.Alignment);
                        var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, item.Parent, "native summon parent");
                        AbilityData known = book.GetKnownSpells(item.SpellLevel).Single(value => ReferenceEquals(value.Blueprint, parent));
                        BlueprintAbility child = InheritedAbility(item.ChildSymbol);
                        int level = item.SpellLevel;
                        return InheritedCast(fixture, sorcerer, new AbilityData(known, child), parent, child, () => AvailableSlotsOf(book, level), 1, item, false);
                    });

                InheritedTemplateSection(assertions, cases, "cancel-before-commit", () =>
                {
                    sorcerer.Descriptor.Alignment.Set(Alignment.NeutralGood);
                    var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, InheritedSmIV, "native summon parent");
                    AbilityData known = book.GetKnownSpells(4).Single(value => ReferenceEquals(value.Blueprint, parent));
                    BlueprintAbility child = InheritedAbility("KMG.Summoning.Ability.SM.Tier4.AirMephit.One");
                    return InheritedCast(fixture, sorcerer, new AbilityData(known, child), parent, child, () => AvailableSlotsOf(book, 4), 0,
                        new InheritedCase { Name = "cancel-before-commit", Quantity = SummonMultiplicity.One, UnitSymbol = "KMG.Summoning.Unit.AirMephit" }, true);
                });

                InheritedTemplateSection(assertions, cases, "master-summoner-tier4-mephit", () =>
                {
                    UnitEntityData caster = fixture.Caster;
                    caster.Descriptor.Stats.Charisma.BaseValue = 18;
                    caster.Descriptor.Alignment.Set(Alignment.NeutralGood);
                    var summoner = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(BlueprintBootstrap.Library,
                        AlternateSummoningContracts.SummonerClass, "CotW Summoner class");
                    var archetype = BlueprintLibraryLookup.RequireExact<BlueprintArchetype>(BlueprintBootstrap.Library,
                        PlayerPathMasterSummonerArchetype, "CotW Master Summoner archetype");
                    AdvanceDisposableArchetypeCaster(caster.Descriptor, summoner, archetype, 7, ref _playerPathLevelController);
                    var resource = BlueprintLibraryLookup.RequireExact<BlueprintAbilityResource>(BlueprintBootstrap.Library,
                        AlternateSummoningContracts.MasterSummonerResource, "Master Summoner resource");
                    caster.Descriptor.Resources.Restore(resource);
                    AlternateSummoningFamilySpec family = AlternateSummoningSourceCatalog.Find(AlternateSummoningSourceCatalog.MasterSummonerKey);
                    Ability root = PlayerPathRootFact(caster, family, 4);
                    BlueprintUnit mephit = (BlueprintUnit)ExpandedSummoningBlueprints.Registered.BySymbol["KMG.Summoning.Unit.AirMephit"];
                    BlueprintAbility child = root.Blueprint.ComponentsArray.OfType<AbilityVariants>().Single().Variants.Single(value =>
                    {
                        if (value == null || !AlternateSummoningExposure.IsExposed(value.AssetGuid)) return false;
                        SummonSpawn[] spawns = SummonSignatureReader.Read(value).Spawns().ToArray();
                        return spawns.Length > 0 && spawns[0].Unit == mephit.AssetGuid &&
                            (spawns[0].CountDice == "Zero" || spawns[0].CountDice == "One");
                    });
                    var parentData = new AbilityData(root);
                    var data = new AbilityData(parentData, child);
                    AbilityResourceLogic logic = child.ComponentsArray.OfType<AbilityResourceLogic>().SingleOrDefault();
                    int cost = logic == null ? 0 : logic.CalculateCost(data);
                    return InheritedCast(fixture, caster, data, root.Blueprint, child, () => caster.Descriptor.Resources.GetResourceAmount(resource), cost,
                        new InheritedCase { Name = "master-summoner-tier4-mephit", Alignment = Alignment.NeutralGood, Quantity = SummonMultiplicity.One,
                            UnitSymbol = "KMG.Summoning.Unit.AirMephit", ChildSymbol = child.name }, false, true);
                });
            }
            catch (Exception exception)
            {
                document["exception"] = exception.ToString();
                assertions.Add(Assertion("inherited-template-casts", "fixture completes",
                    exception.GetType().Name + ":" + exception.Message, false, "unknown state is not qualification"));
            }
            finally
            {
                object levelController = _playerPathLevelController;
                _playerPathLevelController = null;
                if (levelController != null)
                    try { levelController.GetType().GetMethod("Cancel").Invoke(levelController, null); }
                    catch (Exception) { }
                EndExpandedSummoningCorrectionFixture(fixture, out cleaned);
                foreach (BlueprintUnit blueprint in _playerPathBlueprints)
                    if (blueprint != null) UnityEngine.Object.Destroy(blueprint);
                _playerPathBlueprints.Clear();
                donorAfter = ExpandedSummoningAbilityBuilder.DonorReceiptDifferences();
                foreach (KeyValuePair<string, InheritedTemplateDonorReceipt> entry in unrelated)
                    foreach (string difference in InheritedTemplateDonorReceipt.Differences(entry.Value,
                        ExpandedSummoningAbilityBuilder.CaptureReceipt(InheritedAbility(entry.Key))))
                        unrelatedDifferences.Add(entry.Key + ":" + difference);
            }
            int expectedCases = 9;
            assertions.Add(Assertion("inherited-casts-all-cases-observed",
                "all nine representative cases produced a structured observation",
                "cases=" + cases.Count, cases.Count == expectedCases, "one record per planned case"));
            assertions.Add(Assertion("inherited-casts-donors-unmodified",
                "every recorded donor carrier matches its pre-clone receipt before and after all casts",
                "before=[" + (donorBefore == null ? "<not read>" : string.Join(",", donorBefore.ToArray())) + "];after=[" +
                    (donorAfter == null ? "<not read>" : string.Join(",", donorAfter.ToArray())) + "]",
                donorBefore != null && donorAfter != null && donorBefore.Count == 0 && donorAfter.Count == 0,
                "ExpandedSummoningAbilityBuilder donor receipts"));
            assertions.Add(Assertion("inherited-casts-unrelated-rows-unchanged",
                "two published rows that no case casts are unchanged by the casts",
                "rows=" + unrelated.Count + ";differences=[" + string.Join(",", unrelatedDifferences.ToArray()) + "]",
                unrelated.Count == 2 && unrelatedDifferences.Count == 0, "receipts of the registered rows"));
            assertions.Add(Assertion("inherited-casts-cleanup",
                "every disposable caster and summoned unit is gone and the native census is unchanged",
                "cleaned=" + cleaned, cleaned, "exact unit/party/area reference snapshots"));
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, "inherited-template-casts.json"),
                document.ToString(Formatting.Indented));
            WriteLifecycleStage("inherited-template-casts-written");
            assertions.Add(Assertion("loaded-mod-version", _request.ExpectedModVersion, _context.ModEntry.Info.Version,
                _context.ModEntry.Info.Version == _request.ExpectedModVersion, "Unity Mod Manager ModEntry.Info.Version"));
            return CreateResult(assertions.All(value => value.Status == "PASS") ? "PASS" : "FAIL", assertions, null);
        }

        private static BlueprintAbility InheritedAbility(string symbol)
        {
            BlueprintScriptableObject value;
            if (!ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(symbol, out value) || !(value is BlueprintAbility))
                throw new InvalidOperationException("Registered ability missing: " + symbol);
            return (BlueprintAbility)value;
        }

        private static int AvailableSlotsOf(Spellbook book, int level)
        {
            return book.Blueprint.Spontaneous ? book.GetSpontaneousSlots(level) :
                book.GetMemorizedSpellSlots(level).Count(value => value != null && value.Available);
        }

        private void InheritedTemplateSection(List<RuntimeTestAssertion> assertions, JArray cases, string name,
            Func<JObject> run)
        {
            JObject record;
            try { record = run(); }
            catch (Exception exception)
            {
                record = new JObject { ["case"] = name, ["pass"] = false,
                    ["failures"] = new JArray(exception.GetType().Name + ": " + exception.Message) };
            }
            cases.Add(record);
            assertions.Add(Assertion("inherited-case-" + name, "every observation of the case holds",
                record.ToString(Formatting.None), (bool)record["pass"], "native cast; structured observation"));
        }

        private JObject InheritedCast(ExpandedSummoningCorrectionFixture fixture, UnitEntityData caster, AbilityData data,
            BlueprintAbility parent, BlueprintAbility child, Func<int> resource, int expectedCost, InheritedCase item,
            bool cancel, bool alternate = false)
        {
            var failures = new List<string>();
            var record = new JObject { ["case"] = item.Name, ["parent"] = parent.AssetGuid + ":" + parent.name,
                ["child"] = child.AssetGuid + ":" + child.name, ["caster"] = caster.Descriptor.Alignment.Value.ToString(),
                ["alternate"] = alternate };
            object[] before = SnapshotReferences(fixture.SceneEntities);
            ExpandedSummoningRuleCapture.Clear();
            ExpandedSummoningRuleDurationCapture.Clear();
            bool converted = data.ConvertedFrom != null && ReferenceEquals(data.ConvertedFrom.Blueprint, parent) &&
                ReferenceEquals(data.Blueprint, child) && parent.HasVariant(child);
            record["sourceIsVariantOfParent"] = converted;
            if (!converted) failures.Add("the cast is not this child converted from this parent");
            SummonSpawn[] spawnSpec = SummonSignatureReader.Read(child).Spawns().ToArray();
            record["countDice"] = spawnSpec.Length == 0 ? "<none>" : spawnSpec[0].CountDice;
            int debitBefore = resource();
            record["resourceBefore"] = debitBefore;
            string result = "NotStarted";
            bool started = false, acted = false;
            try
            {
                UnitUseAbility command;
                var parameters = new Kingmaker.AreaLogic.Cutscenes.CutsceneParametersContext();
                using (parameters.Data) command = new UnitUseAbility(data, new TargetWrapper(caster.Position + Vector3.forward * 2f));
                record["available"] = data.IsAvailable;
                if (!data.IsAvailable || !command.CanStart) failures.Add("the cast was not available");
                else
                {
                    command.IgnoreCooldown(TimeSpan.Zero);
                    caster.Commands.Run(command);
                    command.Start();
                    started = command.IsRunning;
                    if (cancel)
                    {
                        command.Interrupt();
                        if (!command.IsFinished) command.Tick();
                    }
                    else if (command.IsRunning)
                    {
                        if (command.Animation != null) command.Animation.IsActed = true;
                        command.Tick();
                        if (command.ExecutionProcess != null)
                            for (int tick = 0; tick < 5000 && !command.ExecutionProcess.IsEnded; tick++)
                                command.ExecutionProcess.Tick();
                        if (command.Animation != null) FinishExpandedSummoningAnimation(command.Animation);
                        if (!command.IsFinished) command.Tick();
                    }
                    acted = command.IsActed;
                    result = command.Result.ToString();
                }
            }
            catch (Exception exception) { failures.Add("cast threw " + exception.GetType().Name + ": " + exception.Message); }
            Game.Instance.EntityCreator.Tick();
            int debitAfter = resource();
            UnitEntityData[] summoned = ExpandedSummoningRuleCapture.ToArray();
            record["started"] = started; record["acted"] = acted; record["result"] = result;
            record["resourceAfter"] = debitAfter; record["debit"] = debitBefore - debitAfter; record["expectedCost"] = expectedCost;
            record["summoned"] = summoned.Length;
            if (cancel)
            {
                if (!started) failures.Add("the command was not admitted before the cancellation");
                if (result != "Interrupt") failures.Add("result " + result + ", expected Interrupt");
                if (debitBefore != debitAfter) failures.Add("a cancelled cast debited " + (debitBefore - debitAfter));
                if (summoned.Length != 0) failures.Add("a cancelled cast summoned " + summoned.Length);
            }
            else
            {
                if (result != "Success") failures.Add("result " + result);
                if (debitBefore - debitAfter != expectedCost || expectedCost < 1)
                    failures.Add("debit " + (debitBefore - debitAfter) + ", expected exactly " + expectedCost + " once");
                int min = item.Quantity == SummonMultiplicity.One ? 1 : item.Quantity == SummonMultiplicity.OneD3 ? 1 : 2;
                int max = item.Quantity == SummonMultiplicity.One ? 1 : item.Quantity == SummonMultiplicity.OneD3 ? 3 : 5;
                record["quantityContract"] = min + ".." + max;
                if (summoned.Length < min || summoned.Length > max) failures.Add("summoned " + summoned.Length + " outside " + min + ".." + max);
                if (summoned.Distinct().Count() != summoned.Length) failures.Add("a unit was captured twice");
                InheritedObserveUnits(fixture, summoned, child, item, record, failures);
            }
            foreach (UnitEntityData unit in summoned) CleanupExpandedSummoningUnit(unit);
            DrainExpandedSummoningDestroyQueue(fixture.SceneEntities, before);
            int lingering = SnapshotReferences(fixture.SceneEntities).Count(value => !before.Any(prior => ReferenceEquals(prior, value)));
            record["lingeringAfterCleanup"] = lingering;
            if (lingering != 0) failures.Add(lingering + " fixture entities lingered");
            ExpandedSummoningRuleCapture.Clear();
            record["failures"] = new JArray(failures);
            record["pass"] = failures.Count == 0;
            return record;
        }

        private static void InheritedObserveUnits(ExpandedSummoningCorrectionFixture fixture, UnitEntityData[] summoned,
            BlueprintAbility child, InheritedCase item, JObject record, List<string> failures)
        {
            var nativeIds = new HashSet<string>(ExpandedSummoningAbilityBuilder.NativeTemplateIds, StringComparer.Ordinal);
            BlueprintScriptableObject expectedUnit;
            ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(item.UnitSymbol, out expectedUnit);
            var projectTemplates = new HashSet<string>(ExpandedSummoningBlueprints.Registered.BySymbol
                .Where(entry => entry.Key.StartsWith("KMG.Summoning.Template.", StringComparison.Ordinal) && entry.Value != null)
                .Select(entry => entry.Value.AssetGuid), StringComparer.Ordinal);
            // The project template the child's own graph names for this caster.
            string wantTemplate = null, wantSmite = null;
            var byCaster = AlternateSummoningGraph.Objects<ContextActionApplySummonTemplateByCaster>(child.ComponentsArray).FirstOrDefault();
            if (item.ProjectTemplate != "none")
            {
                if (byCaster == null) failures.Add("the templated child names no caster-chosen project template");
                else
                {
                    bool celestial = item.ProjectTemplate == "celestial";
                    wantTemplate = (celestial ? byCaster.CelestialTemplate : byCaster.FiendishTemplate)?.AssetGuid;
                    wantSmite = (celestial ? byCaster.CelestialSmite : byCaster.FiendishSmite)?.AssetGuid;
                }
            }
            string[] graphApplies = AlternateSummoningGraph.Objects<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff>(child.ComponentsArray)
                .Where(action => action.Buff != null && nativeIds.Contains(action.Buff.AssetGuid))
                .Select(action => action.Buff.AssetGuid).Distinct().OrderBy(value => value, StringComparer.Ordinal).ToArray();
            record["childGraphNativeApplications"] = new JArray(graphApplies);
            record["childGraphRemoval"] = AlternateSummoningGraph.Objects<ContextActionRemoveInheritedSummonTemplates>(child.ComponentsArray).Count;
            record["expectedUnit"] = expectedUnit == null ? "<unresolved>" : expectedUnit.AssetGuid + ":" + expectedUnit.name;
            BlueprintBuff marker = BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff;
            var durations = new JArray();
            foreach (var capture in ExpandedSummoningRuleDurationCapture)
                durations.Add(Math.Round((capture.BaseDuration + capture.BonusDuration).TotalSeconds, 1));
            record["durationSeconds"] = durations;
            if (durations.Count != summoned.Length || durations.Any(value => (double)value <= 0))
                failures.Add("duration missing or not positive for a summoned unit");
            var units = new JArray();
            foreach (UnitEntityData unit in summoned)
            {
                Buff[] buffs = unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => value.Blueprint != null).ToArray();
                string[] native = buffs.Where(value => nativeIds.Contains(value.Blueprint.AssetGuid)).Select(value => value.Blueprint.AssetGuid).ToArray();
                string[] project = buffs.Where(value => projectTemplates.Contains(value.Blueprint.AssetGuid)).Select(value => value.Blueprint.AssetGuid).ToArray();
                BlueprintUnitFact[] innate = (unit.Blueprint.AddFacts ?? new BlueprintUnitFact[0]).Where(value => value != null).ToArray();
                string[] missingInnate = innate.Where(fact => !unit.Descriptor.HasFact(fact)).Select(fact => fact.name).ToArray();
                bool hasMarker = unit.Descriptor.HasFact(marker);
                bool live = !unit.Destroyed && unit.IsInState && ContainsReference(fixture.SceneEntities, unit);
                units.Add(new JObject { ["unit"] = unit.Blueprint.AssetGuid + ":" + unit.Blueprint.name, ["live"] = live,
                    ["nativeTemplates"] = new JArray(native), ["projectTemplates"] = new JArray(project),
                    ["innateFacts"] = innate.Length, ["missingInnate"] = new JArray(missingInnate), ["summonMarker"] = hasMarker });
                if (expectedUnit == null || !ReferenceEquals(unit.Blueprint, expectedUnit)) failures.Add("unit " + unit.Blueprint.name + " is not the expected unit");
                if (!live) failures.Add("a summoned unit is not live in the area");
                if (native.Length != 0) failures.Add(unit.Blueprint.name + " carries native template " + string.Join(",", native));
                if (missingInnate.Length != 0) failures.Add(unit.Blueprint.name + " lost innate facts " + string.Join(",", missingInnate));
                if (!hasMarker) failures.Add(unit.Blueprint.name + " lost the native summon marker");
                if (item.ProjectTemplate == "none" && project.Length != 0) failures.Add(unit.Blueprint.name + " carries a project template " + string.Join(",", project));
                if (item.ProjectTemplate != "none")
                {
                    if (wantTemplate == null || !buffs.Any(value => value.Blueprint.AssetGuid == wantTemplate))
                        failures.Add(unit.Blueprint.name + " lacks the " + item.ProjectTemplate + " project template " + wantTemplate);
                    if (wantSmite == null || !buffs.Any(value => value.Blueprint.AssetGuid == wantSmite))
                        failures.Add(unit.Blueprint.name + " lacks the " + item.ProjectTemplate + " smite " + wantSmite);
                    string other = item.ProjectTemplate == "celestial" ? byCaster?.FiendishTemplate?.AssetGuid : byCaster?.CelestialTemplate?.AssetGuid;
                    if (other != null && buffs.Any(value => value.Blueprint.AssetGuid == other))
                        failures.Add(unit.Blueprint.name + " carries the opposite project template");
                }
            }
            record["units"] = units;
            record["expectedProjectTemplate"] = wantTemplate ?? "none";
        }
    }
}
