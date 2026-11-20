using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
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
    /// S5 T1 / DEF-05 representative native casts, second review. Save-free
    /// native first area (the summoning native profile), disposable casters
    /// levelled natively: a level-20 Sorcerer casting canonical Summon Monster
    /// and Nature's Ally children through its own spontaneous slots, and a Call
    /// of the Wild Master Summoner casting an adapter child through its real
    /// resource; one Sorcerer cast is cancelled before commit. Each case is
    /// judged by the engine-free contracts in InheritedTemplateCastContracts:
    /// the exact source debit (snapshots plus every native spend and restore on
    /// the caster during the cast window, and Cleanup2SourceDebit), the exact
    /// quantity expression read before the cast and the actual output, the
    /// exact duration per spawned unit, and the exact native/project template
    /// and smite sets per unit. Donor receipts and two unrelated published rows
    /// are compared before and after. Missing observations fail.
    /// </summary>
    internal sealed partial class RuntimeTestRunner
    {
        private const string InheritedSorcererClass = "b3a505fb61437dc4097f43c3f8f9a4cf";
        private const string InheritedSmIV = "7ed74a3ec8c458d4fb50b192fd7be6ef";
        private const string InheritedSmVI = "e740afbab0147944dab35d83faa0ae1c";
        private const string InheritedSmVII = "ab167fd8203c1314bac6568932f1752f";
        private const string InheritedSnaII = "298148133cdc3fd42889b99c82711986";
        /// <summary>The native Summon Monster and Nature's Ally duration at caster level 20: one round per level.</summary>
        private const double InheritedSorcererDurationSeconds = 120d;
        /// <summary>Superior Summoning, which adds to the ProjectilesCount rank; the disposable Sorcerer must not hold it.</summary>
        private const string InheritedSuperiorSummoning = "0477936c0f74841498b5c8753a8062a3";
        /// <summary>Tabletop d4+1 without Superior Summoning: the ProjectilesCount bonus is exactly 1.</summary>
        private const int InheritedD4PlusOneRank = 1;
        /// <summary>The controlled value of the one d4 roll of the d4+1 case.</summary>
        private const int InheritedForcedDie = 2;
        /// <summary>The disposable Master Summoner's exact Summoner class level.</summary>
        private const int InheritedMasterSummonerLevel = 7;

        private static List<string> _inheritedEvents;
        private static UnitEntityData _inheritedCaster;

        private sealed class InheritedCase
        {
            internal string Name;
            internal Alignment Alignment;
            internal int SpellLevel;
            internal string Parent;
            internal string ChildSymbol;
            internal string UnitSymbol;
            internal SummonFamily Family = SummonFamily.Monster;
            internal SummonMultiplicity Quantity;
            /// <summary>"none", "celestial" or "fiendish": the project template the cast must leave.</summary>
            internal string ProjectTemplate = "none";
            /// <summary>Force the caster's one d4 roll to InheritedForcedDie and prove the count from it.</summary>
            internal bool ControlDie;
        }

        private sealed class InheritedExpectation
        {
            internal string SourceKey;
            internal int Amount;
            internal string Event;
            internal Func<Cleanup2SourceDebit, bool> Native;
            internal Func<Cleanup2SourceDebit, JObject> Describe;
            internal int Branches;
            internal double DurationBase;
            internal double DurationBonus;
            internal string DurationSource;
            /// <summary>Master Summoner: the duration is judged from the host row's structure and the context's Default rank.</summary>
            internal SummonSpawn ClassRankHostSpawn;
            internal InheritedRankConfigView ClassRankHostConfig;
            internal string SummonerClass;
            internal int SummonerClassLevel;
        }

        /// <summary>
        /// Every RuleRollDice the exact caster triggers during the cast window. The first roll of the controlled
        /// die is forced; every roll of that die is recorded as relevant, every other roll by the caster as other.
        /// </summary>
        private sealed class InheritedDieObserver : IGlobalRulebookHandler<RuleRollDice>
        {
            internal UnitEntityData Initiator; internal DiceType Die; internal int Value;
            internal readonly List<InheritedDieRoll> Relevant = new List<InheritedDieRoll>();
            internal readonly List<string> Other = new List<string>();
            private readonly List<RuleRollDice> _overridden = new List<RuleRollDice>();
            public void OnEventAboutToTrigger(RuleRollDice evt)
            {
                if (evt == null || !ReferenceEquals(evt.Initiator, Initiator) || evt.DiceFormula.Dice != Die) return;
                if (_overridden.Count == 0) { evt.Override(Value); _overridden.Add(evt); }
            }
            public void OnEventDidTrigger(RuleRollDice evt)
            {
                if (evt == null || !ReferenceEquals(evt.Initiator, Initiator)) return;
                int result = evt.Result;
                if (evt.DiceFormula.Dice != Die) { Other.Add(evt.DiceFormula.Rolls + "x" + evt.DiceFormula.Dice + "=" + result); return; }
                Relevant.Add(new InheritedDieRoll(evt.DiceFormula.Dice.ToString(), evt.DiceFormula.Rolls, result,
                    _overridden.Any(value => ReferenceEquals(value, evt))));
            }
        }

        private static void InheritedSpendPostfix(Spellbook __instance, BlueprintAbility blueprint, bool doSpend, bool __result)
        {
            if (_inheritedEvents == null || _inheritedCaster == null || !doSpend || !__result) return;
            if (_inheritedCaster.Descriptor.Spellbooks.Contains(__instance))
                _inheritedEvents.Add("spend-spell|" + __instance.Blueprint.AssetGuid + "|1");
        }

        private static void InheritedRestoreSpontaneousPostfix(Spellbook __instance, int level, int count)
        {
            if (_inheritedEvents == null || _inheritedCaster == null) return;
            if (_inheritedCaster.Descriptor.Spellbooks.Contains(__instance))
                _inheritedEvents.Add("restore-spontaneous|" + __instance.Blueprint.AssetGuid + "|" + level + "|" + count);
        }

        private static void InheritedResourceSpendPostfix(UnitAbilityResourceCollection __instance,
            BlueprintScriptableObject blueprint, int amount)
        {
            if (_inheritedEvents == null || _inheritedCaster == null) return;
            if (ReferenceEquals(__instance, _inheritedCaster.Descriptor.Resources))
                _inheritedEvents.Add("spend-resource|" + (blueprint == null ? "<null>" : blueprint.AssetGuid) + "|" + amount);
        }

        private static void InheritedResourceRestorePostfix(UnitAbilityResourceCollection __instance,
            BlueprintScriptableObject blueprint, int amount, bool restoreFull)
        {
            if (_inheritedEvents == null || _inheritedCaster == null) return;
            if (ReferenceEquals(__instance, _inheritedCaster.Descriptor.Resources))
                _inheritedEvents.Add("restore-resource|" + (blueprint == null ? "<null>" : blueprint.AssetGuid) + "|" + amount + "|" + restoreFull);
        }

        /// <summary>The native spend and restore methods the cast window watches, with this observer's postfixes.</summary>
        private static List<KeyValuePair<MethodInfo, MethodInfo>> InheritedHooks()
        {
            const BindingFlags any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            const BindingFlags own = BindingFlags.Static | BindingFlags.NonPublic;
            var hooks = new List<KeyValuePair<MethodInfo, MethodInfo>>
            {
                new KeyValuePair<MethodInfo, MethodInfo>(typeof(Spellbook).GetMethods(any).Single(value => value.Name == "SpendInternal"),
                    typeof(RuntimeTestRunner).GetMethod("InheritedSpendPostfix", own)),
                new KeyValuePair<MethodInfo, MethodInfo>(typeof(Spellbook).GetMethod("RestoreSpontaneousSlots", any, null,
                    new[] { typeof(int), typeof(int) }, null), typeof(RuntimeTestRunner).GetMethod("InheritedRestoreSpontaneousPostfix", own)),
                new KeyValuePair<MethodInfo, MethodInfo>(typeof(UnitAbilityResourceCollection).GetMethod("Spend", any, null,
                    new[] { typeof(BlueprintScriptableObject), typeof(int) }, null), typeof(RuntimeTestRunner).GetMethod("InheritedResourceSpendPostfix", own)),
                new KeyValuePair<MethodInfo, MethodInfo>(typeof(UnitAbilityResourceCollection).GetMethod("Restore", any, null,
                    new[] { typeof(BlueprintScriptableObject), typeof(int), typeof(bool) }, null), typeof(RuntimeTestRunner).GetMethod("InheritedResourceRestorePostfix", own)),
            };
            if (hooks.Any(value => value.Key == null || value.Value == null))
                throw new MissingMethodException("A native spend/restore method or observer postfix is missing.");
            return hooks;
        }

        /// <summary>Every source counter and identity of the caster, for the engine-free ledger.</summary>
        private static Dictionary<string, int> InheritedSources(UnitEntityData caster)
        {
            var sources = new Dictionary<string, int>(StringComparer.Ordinal);
            Spellbook[] books = caster.Descriptor.Spellbooks.ToArray();
            sources["identity|books"] = string.Join(",", books.Select(book => book.Blueprint.AssetGuid).ToArray()).GetHashCode();
            for (int index = 0; index < books.Length; index++)
            {
                Spellbook book = books[index];
                string key = book.Blueprint.AssetGuid + "#" + index;
                for (int level = 0; level < 10; level++)
                {
                    if (book.Blueprint.Spontaneous) sources["spontaneous|" + book.Blueprint.AssetGuid + "|" + level] = book.GetSpontaneousSlots(level);
                    List<SpellSlot> slots = RawSlots(book, level);
                    for (int slot = 0; slot < slots.Count; slot++)
                        sources["slot|" + key + "|" + level + "|" + slot] = slots[slot].Available ? 1 : 0;
                    sources["identity|custom|" + key + "|" + level] = string.Join(",", book.GetCustomSpells(level)
                        .Select(spell => spell.Blueprint.AssetGuid).ToArray()).GetHashCode();
                }
                sources["identity|known|" + key] = string.Join(",", book.GetAllKnownSpells()
                    .Select(spell => spell.Blueprint.AssetGuid + ":" + spell.SpellLevel).ToArray()).GetHashCode();
            }
            var resources = new List<string>();
            foreach (BlueprintScriptableObject resource in caster.Descriptor.Resources)
            {
                resources.Add(resource.AssetGuid);
                sources["resource|" + resource.AssetGuid] = caster.Descriptor.Resources.GetResourceAmount(resource);
            }
            sources["identity|resources"] = string.Join(",", resources.ToArray()).GetHashCode();
            return sources;
        }

        private RuntimeTestResult RunInheritedTemplateCasts()
        {
            WriteLifecycleStage("inherited-template-casts-start");
            var assertions = new List<RuntimeTestAssertion>();
            var document = new JObject { ["scope"] = "Save-free native first area; disposable casters levelled by native LevelUpController visits; casts through spellbook slots or the Master Summoner resource and UnitUseAbility; exact contracts in InheritedTemplateCastContracts.", ["runId"] = _request.RunId };
            var cases = new JArray();
            document["cases"] = cases;
            ExpandedSummoningCorrectionFixture fixture = null;
            bool cleaned = false;
            IList<InheritedDonorWindowEntry> donorBefore = null, donorAfter = null;
            ICollection<string> donorAdapterGuids = null;
            var donorBeforeRecord = new JObject();
            var donorAfterRecord = new JObject();
            var unrelated = new Dictionary<string, InheritedTemplateDonorReceipt>(StringComparer.Ordinal);
            var unrelatedDifferences = new List<string>();
            List<KeyValuePair<MethodInfo, MethodInfo>> hooks = null;
            var installed = new List<KeyValuePair<MethodInfo, MethodInfo>>();
            try
            {
                AlternateSummoningFamilyStatus status = AlternateSummoningStatusRegistry.Find(
                    AlternateSummoningSourceCatalog.MasterSummonerKey);
                if (status == null || status.Disposition != AlternateSummoningDisposition.Published)
                    throw new InvalidOperationException("Master Summoner is not published: " +
                        (status == null ? "<no status>" : status.Disposition + " " + status.Reason));
                donorAdapterGuids = InheritedOwnedAdapterGuids();
                donorBefore = InheritedDonorWindow(donorAdapterGuids, donorBeforeRecord);
                foreach (string symbol in new[] { "KMG.Summoning.Ability.SM.Tier9.Lemure.OneD4PlusOne",
                    "KMG.Summoning.Ability.SNA.Tier4.Wolf.OneD4PlusOne" })
                    unrelated[symbol] = ExpandedSummoningAbilityBuilder.CaptureReceipt(InheritedAbility(symbol));
                hooks = InheritedHooks();
                foreach (KeyValuePair<MethodInfo, MethodInfo> hook in hooks)
                {
                    _context.Harmony.Patch(hook.Key, null, new HarmonyMethod(hook.Value), null);
                    installed.Add(hook);
                }
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
                var plan = new[]
                {
                    new InheritedCase { Name = "good-tier4-single-mephit", Alignment = Alignment.NeutralGood, SpellLevel = 4, Parent = InheritedSmIV,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier4.AirMephit.One", UnitSymbol = "KMG.Summoning.Unit.AirMephit", Quantity = SummonMultiplicity.One },
                    new InheritedCase { Name = "evil-tier4-single-mephit", Alignment = Alignment.NeutralEvil, SpellLevel = 4, Parent = InheritedSmIV,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier4.AirMephit.One", UnitSymbol = "KMG.Summoning.Unit.AirMephit", Quantity = SummonMultiplicity.One },
                    new InheritedCase { Name = "quantity-tier6-d4plus1-mephit", Alignment = Alignment.NeutralGood, SpellLevel = 6, Parent = InheritedSmVI,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier6.AirMephit.OneD4PlusOne", UnitSymbol = "KMG.Summoning.Unit.AirMephit", Quantity = SummonMultiplicity.OneD4PlusOne,
                        ControlDie = true },
                    new InheritedCase { Name = "celestial-project-template", Alignment = Alignment.NeutralGood, SpellLevel = 4, Parent = InheritedSmIV,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier4.DireWolf.One", UnitSymbol = "KMG.Summoning.Unit.DireWolf", Quantity = SummonMultiplicity.One, ProjectTemplate = "celestial" },
                    new InheritedCase { Name = "fiendish-project-template", Alignment = Alignment.NeutralEvil, SpellLevel = 4, Parent = InheritedSmIV,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier4.DireWolf.One", UnitSymbol = "KMG.Summoning.Unit.DireWolf", Quantity = SummonMultiplicity.One, ProjectTemplate = "fiendish" },
                    new InheritedCase { Name = "tier7-untemplated-no-inherited", Alignment = Alignment.NeutralGood, SpellLevel = 7, Parent = InheritedSmVII,
                        ChildSymbol = "KMG.Summoning.Ability.SM.Tier7.GreaterAirElemental.One", UnitSymbol = "KMG.Summoning.Unit.GreaterAirElemental", Quantity = SummonMultiplicity.One },
                    new InheritedCase { Name = "natures-ally-preservation", Alignment = Alignment.NeutralGood, SpellLevel = 2, Parent = InheritedSnaII, Family = SummonFamily.NaturesAlly,
                        ChildSymbol = "KMG.Summoning.Ability.SNA.Tier2.Wolf.One", UnitSymbol = "KMG.Summoning.Unit.Wolf", Quantity = SummonMultiplicity.One },
                };
                foreach (InheritedCase item in plan)
                {
                    var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, item.Parent, "native summon parent");
                    if (!book.GetKnownSpells(item.SpellLevel).Any(value => ReferenceEquals(value.Blueprint, parent)))
                        book.AddKnown(item.SpellLevel, parent, true);
                }
                book.Rest();
                document["sorcerer"] = new JObject { ["classLevel"] = sorcerer.Descriptor.Progression.GetClassLevel(sorcererClass),
                    ["casterLevel"] = book.CasterLevel, ["book"] = book.Blueprint.AssetGuid };

                foreach (InheritedCase item in plan.Concat(new[] { new InheritedCase { Name = "cancel-before-commit", Alignment = Alignment.NeutralGood,
                    SpellLevel = 4, Parent = InheritedSmIV, ChildSymbol = "KMG.Summoning.Ability.SM.Tier4.AirMephit.One",
                    UnitSymbol = "KMG.Summoning.Unit.AirMephit", Quantity = SummonMultiplicity.One } }))
                    InheritedTemplateSection(assertions, cases, item.Name, () =>
                    {
                        bool cancel = item.Name == "cancel-before-commit";
                        sorcerer.Descriptor.Alignment.Set(item.Alignment);
                        var parent = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, item.Parent, "native summon parent");
                        AbilityData known = book.GetKnownSpells(item.SpellLevel).Single(value => ReferenceEquals(value.Blueprint, parent));
                        BlueprintAbility child = InheritedAbility(item.ChildSymbol);
                        SummonVariantSpec placement = ExpandedSummoningCatalog.GenerateVariants(item.Family).Single(value =>
                            ExpandedSummoningIdentityCatalog.AbilitySymbol(value) == item.ChildSymbol);
                        int donorBranches = AlternateSummoningGraph.Objects<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionSpawnMonster>(
                            ExpandedSummoningAbilityBuilder.RecordedDonor(placement).Ability.ComponentsArray).Count;
                        int level = item.SpellLevel;
                        var expectation = new InheritedExpectation { Branches = donorBranches,
                            DurationBase = cancel ? 0 : InheritedSorcererDurationSeconds, DurationBonus = 0,
                            DurationSource = "native summon spell: one round per caster level at caster level 20" };
                        if (!cancel)
                        {
                            expectation.SourceKey = "spontaneous|" + book.Blueprint.AssetGuid + "|" + level;
                            expectation.Amount = 1;
                            expectation.Event = "spend-spell|" + book.Blueprint.AssetGuid + "|1";
                            expectation.Native = debit => debit.ExactlyOneSpontaneous(book, level);
                            expectation.Describe = debit => debit.DescribeSpontaneous(book, level);
                        }
                        else
                        {
                            expectation.Native = debit => debit.Unspent;
                            expectation.Describe = debit => debit.DescribeSpontaneous(book, level);
                        }
                        return InheritedCast(fixture, sorcerer, () => new AbilityData(known, child), parent, child, item, cancel, expectation, false);
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
                        return spawns.Length > 0 && spawns[0].Unit == mephit.AssetGuid && spawns[0].CountDice == "Zero";
                    });
                    // Expected duration and branch count from the untouched host row the child was built from.
                    BlueprintAbility host = AlternateSummoningCoordinator.OriginalHostChildren(family.Key, root.Blueprint.AssetGuid)
                        .FirstOrDefault(value => value != null && SummonSignatureReader.Read(value).Spawns().Any() &&
                            SummonSignatureReader.Read(value).Spawns().All(spawn => spawn.CountDice == "Zero"));
                    if (host == null) throw new InvalidOperationException("No untouched single-creature host row for Master Summoner tier IV.");
                    SummonSpawn hostSpawn = SummonSignatureReader.Read(host).Spawns().First();
                    InheritedRankConfigView[] hostDefaults = InheritedRankConfigs(host, AbilityRankType.Default);
                    var parentData = new AbilityData(root);
                    var expectation = new InheritedExpectation { Branches = SummonSignatureReader.Read(host).Spawns().Count(),
                        DurationSource = "untouched CotW host row " + host.AssetGuid + ": " + hostSpawn.DurationRate + " x " +
                            hostSpawn.DurationDice + hostSpawn.DurationDiceCount + "+" + hostSpawn.DurationBonus,
                        ClassRankHostSpawn = hostSpawn, ClassRankHostConfig = hostDefaults.Length == 1 ? hostDefaults[0] : null,
                        SummonerClass = summoner.AssetGuid,
                        SummonerClassLevel = caster.Descriptor.Progression.GetClassLevel(summoner) };
                    AbilityData probe = new AbilityData(parentData, child);
                    AbilityResourceLogic logic = child.ComponentsArray.OfType<AbilityResourceLogic>().SingleOrDefault();
                    int cost = logic == null ? 0 : logic.CalculateCost(probe);
                    expectation.SourceKey = "resource|" + resource.AssetGuid;
                    expectation.Amount = cost;
                    expectation.Event = "spend-resource|" + resource.AssetGuid + "|" + cost;
                    expectation.Native = debit => debit.ExactlyOneResource(resource, cost);
                    expectation.Describe = debit => debit.DescribeResource(resource, cost);
                    return InheritedCast(fixture, caster, () => new AbilityData(parentData, child), root.Blueprint, child,
                        new InheritedCase { Name = "master-summoner-tier4-mephit", Alignment = Alignment.NeutralGood, Quantity = SummonMultiplicity.One,
                            UnitSymbol = "KMG.Summoning.Unit.AirMephit", ChildSymbol = child.name }, false, expectation, true);
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
                _inheritedEvents = null;
                _inheritedCaster = null;
                foreach (KeyValuePair<MethodInfo, MethodInfo> hook in installed)
                    try { _context.Harmony.Unpatch(hook.Key, hook.Value); }
                    catch (Exception unpatch) { document["unpatchError"] = unpatch.Message; }
                object levelController = _playerPathLevelController;
                _playerPathLevelController = null;
                if (levelController != null)
                    try { levelController.GetType().GetMethod("Cancel").Invoke(levelController, null); }
                    catch (Exception) { }
                EndExpandedSummoningCorrectionFixture(fixture, out cleaned);
                foreach (BlueprintUnit blueprint in _playerPathBlueprints)
                    if (blueprint != null) UnityEngine.Object.Destroy(blueprint);
                _playerPathBlueprints.Clear();
                try { donorAfter = InheritedDonorWindow(donorAdapterGuids ?? InheritedOwnedAdapterGuids(), donorAfterRecord); }
                catch (Exception exception) { donorAfterRecord["exception"] = exception.GetType().Name + ": " + exception.Message; }
                foreach (KeyValuePair<string, InheritedTemplateDonorReceipt> entry in unrelated)
                    foreach (string difference in InheritedTemplateDonorReceipt.Differences(entry.Value,
                        ExpandedSummoningAbilityBuilder.CaptureReceipt(InheritedAbility(entry.Key))))
                        unrelatedDifferences.Add(entry.Key + ":" + difference);
            }
            const int expectedCases = 9;
            assertions.Add(Assertion("inherited-casts-all-cases-observed",
                "all nine representative cases produced a structured observation",
                "cases=" + cases.Count, cases.Count == expectedCases, "one record per planned case"));
            assertions.Add(Assertion("inherited-casts-hooks-removed",
                "every spend/restore observer hook was installed and removed",
                "installed=" + installed.Count + ";unpatchError=" + (document["unpatchError"] ?? "none"),
                hooks != null && installed.Count == hooks.Count && document["unpatchError"] == null, "Harmony12 Patch/Unpatch by method"));
            // Correction 4: the census donor validation on both sides of the cast window (48 = 46 unchanged + 2
            // approved WithholdCompleteReplacement, 0 mutated), then every carrier's post-publication receipt
            // compared field by field across the window.
            var donorFailures = new List<string>();
            donorFailures.AddRange(InheritedDonorWindowContract.Validation("before", donorBefore, 48, 2));
            donorFailures.AddRange(InheritedDonorWindowContract.Validation("after", donorAfter, 48, 2));
            IList<string> donorWindow = donorBefore == null || donorAfter == null ? new List<string> { "window:not-read" } :
                InheritedDonorWindowContract.Compare(donorBefore, donorAfter);
            donorFailures.AddRange(donorWindow.Select(value => "window:" + value));
            document["donorWindow"] = new JObject { ["before"] = donorBeforeRecord, ["after"] = donorAfterRecord,
                ["adapterIdentities"] = donorAdapterGuids == null ? 0 : donorAdapterGuids.Count,
                ["receiptDifferences"] = new JArray(donorWindow), ["failures"] = new JArray(donorFailures) };
            assertions.Add(Assertion("inherited-casts-donors-unmodified",
                "48 donor carriers (46 unchanged + 2 approved WithholdCompleteReplacement, 0 mutated) before and after all casts, " +
                    "and every carrier's post-publication receipt unchanged across the casts",
                "before=" + InheritedDonorCounts(donorBefore) + ";after=" + InheritedDonorCounts(donorAfter) +
                    ";receiptDifferences=[" + string.Join(",", donorWindow.ToArray()) + "];failures=" + donorFailures.Count,
                donorFailures.Count == 0, "census donor validation and post-publication receipts"));
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

        private static readonly BindingFlags InheritedPrivate = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static object InheritedField(object owner, string name)
        {
            FieldInfo field = owner.GetType().GetField(name, InheritedPrivate);
            if (field == null) throw new MissingFieldException(owner.GetType().FullName, name);
            return field.GetValue(owner);
        }

        /// <summary>The fields of every ContextRankConfig of one rank type on an ability, read from the engine.</summary>
        private static InheritedRankConfigView[] InheritedRankConfigs(BlueprintAbility ability, AbilityRankType type)
        {
            return (ability.ComponentsArray ?? new BlueprintComponent[0]).OfType<ContextRankConfig>()
                .Where(config => config.Type == type).Select(config =>
                {
                    var feature = InheritedField(config, "m_Feature") as BlueprintScriptableObject;
                    var classes = InheritedField(config, "m_Class") as BlueprintCharacterClass[];
                    return new InheritedRankConfigView {
                        Type = config.Type.ToString(),
                        BaseValueType = InheritedField(config, "m_BaseValueType").ToString(),
                        Progression = InheritedField(config, "m_Progression").ToString(),
                        Feature = feature == null ? null : feature.AssetGuid,
                        Classes = (classes ?? new BlueprintCharacterClass[0]).Select(value => value == null ? "<null>" : value.AssetGuid).ToArray(),
                        ExceptClasses = (bool)InheritedField(config, "m_ExceptClasses"),
                        StartLevel = (int)InheritedField(config, "m_StartLevel"), StepLevel = (int)InheritedField(config, "m_StepLevel"),
                        UseMin = (bool)InheritedField(config, "m_UseMin"), Min = (int)InheritedField(config, "m_Min"),
                        UseMax = (bool)InheritedField(config, "m_UseMax"), Max = (int)InheritedField(config, "m_Max") };
                }).ToArray();
        }

        private static JObject InheritedRankConfigRecord(InheritedRankConfigView view)
        {
            return view == null ? null : new JObject { ["type"] = view.Type, ["baseValueType"] = view.BaseValueType,
                ["progression"] = view.Progression, ["feature"] = view.Feature, ["classes"] = new JArray(view.Classes),
                ["exceptClasses"] = view.ExceptClasses, ["startLevel"] = view.StartLevel, ["stepLevel"] = view.StepLevel,
                ["useMin"] = view.UseMin, ["min"] = view.Min, ["useMax"] = view.UseMax, ["max"] = view.Max };
        }

        /// <summary>The exact execution context the command's own AbilityData creates for its own target, recalculated.</summary>
        private static JObject InheritedContextRecord(AbilityData data, MechanicsContext context, bool recalculated)
        {
            var chain = new JArray();
            for (AbilityData link = data.ConvertedFrom; link != null; link = link.ConvertedFrom)
                chain.Add(link.Blueprint.AssetGuid + ":" + link.Blueprint.name);
            var ranks = new JObject();
            foreach (AbilityRankType type in new[] { AbilityRankType.Default, AbilityRankType.ProjectilesCount })
                ranks[type.ToString()] = context[type];
            return new JObject {
                ["associatedBlueprint"] = context.AssociatedBlueprint == null ? null :
                    context.AssociatedBlueprint.AssetGuid + ":" + context.AssociatedBlueprint.name,
                ["abilityData"] = data.Blueprint.AssetGuid + ":" + data.Blueprint.name,
                ["convertedFrom"] = chain,
                ["spellbook"] = data.Spellbook == null ? null : data.Spellbook.Blueprint.AssetGuid,
                ["fact"] = data.Fact == null ? null : data.Fact.Blueprint.AssetGuid,
                ["caster"] = context.MaybeCaster == null ? null : context.MaybeCaster.UniqueId,
                ["contextType"] = context.GetType().Name,
                ["recalculated"] = recalculated,
                ["casterLevel"] = context.Params.CasterLevel,
                ["rankBonus"] = context.Params.RankBonus,
                ["spellLevel"] = context.Params.SpellLevel,
                ["ranks"] = ranks };
        }

        /// <summary>Every live, published KMG adapter ability identity, for the census's owned-row classification.</summary>
        private static ICollection<string> InheritedOwnedAdapterGuids()
        {
            var guids = new HashSet<string>(StringComparer.Ordinal);
            foreach (AlternateSummoningFamilySpec family in AlternateSummoningSourceCatalog.All)
                foreach (BlueprintAbility owned in AlternateSummoningCoordinator.OwnedIdentities(family.Key).Values)
                    if (owned != null) guids.Add(owned.AssetGuid);
            return guids;
        }

        /// <summary>
        /// The census donor validation over every recorded carrier, and each carrier's current post-publication receipt.
        /// </summary>
        private static IList<InheritedDonorWindowEntry> InheritedDonorWindow(ICollection<string> adapterGuids, JObject record)
        {
            var entries = new List<InheritedDonorWindowEntry>();
            var dispositions = new JObject();
            var transitions = new JArray();
            foreach (ExpandedSummoningAbilityBuilder.DonorRecord carrier in ExpandedSummoningAbilityBuilder.RecordedCarriers()
                .OrderBy(value => value.Ability.AssetGuid, StringComparer.Ordinal))
            {
                InheritedTierOneObservation view = CensusDonorView(carrier, adapterGuids);
                var failures = new List<string>();
                string disposition = InheritedTierOnePublicationContract.Judge(view, failures);
                bool withheld = view.Expectation != null && view.Expectation.Disposition == InheritedPreservedExposure.WithholdCompleteReplacement;
                entries.Add(new InheritedDonorWindowEntry(carrier.Ability.AssetGuid, disposition, withheld,
                    ExpandedSummoningAbilityBuilder.CaptureReceipt(carrier.Ability), failures));
                dispositions[carrier.Ability.AssetGuid] = disposition;
                if (disposition == "approved-tier-one-publication")
                    transitions.Add(new JObject {
                        ["parent"] = carrier.Ability.AssetGuid + ":" + carrier.Ability.name,
                        ["expectedExposure"] = view.Expectation == null ? null : view.Expectation.Disposition.ToString(),
                        ["replacementCoverageReason"] = view.Expectation == null ? null : view.Expectation.Reason,
                        ["preservedInMenuByGuid"] = view.MenuChildren.Count(row => row.Guid == view.PreservedGuid),
                        ["preservedInMenuByReference"] = view.MenuChildren.Count(row => ReferenceEquals(row.Reference, view.PreservedReference)),
                        ["requiredReplacements"] = new JArray((view.Expectation == null ? new List<InheritedRequiredReplacement>() :
                            view.Expectation.Required).Select(row =>
                            {
                                object reference;
                                view.RequiredReplacementReferences.TryGetValue(row.Symbol, out reference);
                                return new JObject { ["symbol"] = row.Symbol, ["guid"] = row.Guid,
                                    ["inMenuByGuid"] = view.MenuChildren.Count(menuRow => menuRow.Guid == row.Guid),
                                    ["inMenuByReference"] = view.MenuChildren.Count(menuRow => reference != null &&
                                        ReferenceEquals(menuRow.Reference, reference)) };
                            })),
                        ["menuChildren"] = view.MenuChildren.Count,
                        ["unexpectedOwnedMenuRows"] = view.MenuChildren.Count(row =>
                            InheritedTierOnePublicationContract.Classify(view, row) == "unexpected-owned"),
                        ["preCloneReceiptDifferences"] = new JArray(view.ReceiptDifferences),
                        ["differencesExcludingMenu"] = new JArray(view.DifferencesExcludingMenu ?? new List<string>()) });
            }
            record["counts"] = InheritedDonorCounts(entries);
            record["dispositions"] = dispositions;
            record["approvedTransitions"] = transitions;
            record["findings"] = new JArray(entries.SelectMany(entry => entry.Findings.Select(value => entry.Guid + ":" + value)));
            return entries;
        }

        private static string InheritedDonorCounts(IList<InheritedDonorWindowEntry> entries)
        {
            if (entries == null) return "<not read>";
            return "carriers=" + entries.Count + ",unchanged=" + entries.Count(value => value.Disposition == "unchanged") +
                ",approved=" + entries.Count(value => value.Disposition == "approved-tier-one-publication") +
                ",withheld=" + entries.Count(value => value.Disposition == "approved-tier-one-publication" && value.Withheld) +
                ",mutated=" + entries.Count(value => value.Disposition != "unchanged" && value.Disposition != "approved-tier-one-publication");
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

        private JObject InheritedCast(ExpandedSummoningCorrectionFixture fixture, UnitEntityData caster, Func<AbilityData> create,
            BlueprintAbility parent, BlueprintAbility child, InheritedCase item, bool cancel, InheritedExpectation expected, bool alternate)
        {
            var failures = new List<string>();
            var record = new JObject { ["case"] = item.Name, ["parent"] = parent.AssetGuid + ":" + parent.name,
                ["child"] = child.AssetGuid + ":" + child.name, ["casterAlignment"] = item.Alignment.ToString(), ["alternate"] = alternate };
            var nativeIds = new HashSet<string>(ExpandedSummoningAbilityBuilder.NativeTemplateIds, StringComparer.Ordinal);
            BlueprintScriptableObject expectedUnit;
            ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(item.UnitSymbol, out expectedUnit);
            string unitGuid = expectedUnit == null ? "<unresolved>" : expectedUnit.AssetGuid;
            record["expectedUnit"] = expectedUnit == null ? "<unresolved>" : unitGuid + ":" + expectedUnit.name;

            // Correction 1: the exact AbilityData the command uses, one TargetWrapper, and the execution context that
            // AbilityData creates for that target, recalculated. Ranks are read only from that context.
            AbilityData data = create();
            var target = new TargetWrapper(caster.Position + Vector3.forward * 2f);
            MechanicsContext context = data.CreateExecutionContext(target);
            context.Recalculate();
            bool recalculated = context.MaybeCaster != null && ReferenceEquals(context.MaybeCaster, caster);
            record["executionContext"] = InheritedContextRecord(data, context, recalculated);
            if (!recalculated) failures.Add("context:not-recalculated-for-this-caster");

            // Quantity: the exact graph expression, read before the cast.
            SummonSpawn[] spawns = SummonSignatureReader.Read(child).Spawns().ToArray();
            int rankBonus = spawns.Length > 0 && spawns[0].CountBonus != null && spawns[0].CountBonus.Kind == "Rank"
                ? context[(AbilityRankType)Enum.Parse(typeof(AbilityRankType), spawns[0].CountBonus.Rank)] : 0;
            InheritedDieObserver die = null;
            if (item.ControlDie)
            {
                // Correction 2: the d4+1 path, controlled. No Superior Summoning; the exact ProjectilesCount config.
                var superior = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                    InheritedSuperiorSummoning, "Superior Summoning");
                bool holdsSuperior = caster.Descriptor.HasFact(superior) || caster.Descriptor.Progression.Features.GetRank(superior) != 0;
                if (holdsSuperior) failures.Add("caster-holds-superior-summoning");
                InheritedRankConfigView[] projectiles = InheritedRankConfigs(child, AbilityRankType.ProjectilesCount);
                if (projectiles.Length != 1) failures.Add("projectiles-count-configs:" + projectiles.Length);
                record["controlledQuantity"] = new JObject { ["superiorSummoning"] = InheritedSuperiorSummoning,
                    ["casterHoldsSuperiorSummoning"] = holdsSuperior,
                    ["projectilesCountConfig"] = projectiles.Length == 1 ? InheritedRankConfigRecord(projectiles[0]) : null,
                    ["recalculatedProjectilesCount"] = context[AbilityRankType.ProjectilesCount],
                    ["expectedProjectilesCount"] = InheritedD4PlusOneRank, ["forcedDie"] = InheritedForcedDie };
                die = new InheritedDieObserver { Initiator = caster, Die = DiceType.D4, Value = InheritedForcedDie };
            }
            int minimum, maximum;
            IList<string> expression = InheritedQuantityContract.Expression(item.Quantity, unitGuid,
                spawns.Select(spawn => new InheritedSpawnExpression(spawn.Unit, spawn.CountDice,
                    spawn.CountDiceCount == null ? "" : spawn.CountDiceCount.Kind, spawn.CountDiceCount == null ? 0 : spawn.CountDiceCount.Value,
                    spawn.CountBonus == null ? "" : spawn.CountBonus.Kind, spawn.CountBonus == null ? 0 : spawn.CountBonus.Value,
                    spawn.CountBonus == null ? "" : spawn.CountBonus.Rank)).ToList(), expected.Branches, rankBonus, out minimum, out maximum);
            record["quantityExpression"] = new JArray(spawns.Select(spawn => spawn.Quantity()));
            record["evaluatedRankBonus"] = rankBonus;
            record["expectedBranches"] = expected.Branches;
            record["quantityRange"] = minimum + ".." + maximum;
            failures.AddRange(expression.Select(value => "expression:" + value));
            record["expectedDuration"] = new JObject { ["base"] = expected.DurationBase, ["bonus"] = expected.DurationBonus,
                ["source"] = expected.DurationSource };
            string expectTemplate = null, expectSmite = null;
            if (item.ProjectTemplate != "none")
            {
                // Third review: the deliberate template is derived independently of the child graph:
                // the band from the expected unit's own class levels through the restated HD contract,
                // the identities from the stable registered symbols. Then the graph is checked against it.
                var unitBlueprint = expectedUnit as BlueprintUnit;
                int hitDice = unitBlueprint == null ? -1 : (unitBlueprint.ComponentsArray ?? new BlueprintComponent[0])
                    .OfType<AddClassLevels>().Sum(component => component.Levels);
                string band = hitDice < 0 ? null : InheritedDeliberateTemplateContract.Band(hitDice);
                Func<string, string> registered = symbol =>
                {
                    BlueprintScriptableObject value;
                    return ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(symbol, out value) && value != null ? value.AssetGuid : null;
                };
                string celestialTemplate = band == null ? null : registered("KMG.Summoning.Template.Celestial." + band);
                string fiendishTemplate = band == null ? null : registered("KMG.Summoning.Template.Fiendish." + band);
                string celestialSmite = registered("KMG.Summoning.Smite.Celestial.Available");
                string fiendishSmite = registered("KMG.Summoning.Smite.Fiendish.Available");
                List<InheritedDynamicTemplateView> wired = AlternateSummoningGraph.Objects<ContextActionApplySummonTemplateByCaster>(child.ComponentsArray)
                    .Select(action => new InheritedDynamicTemplateView(action.CelestialTemplate?.AssetGuid, action.FiendishTemplate?.AssetGuid,
                        action.CelestialSmite?.AssetGuid, action.FiendishSmite?.AssetGuid,
                        (action.ReplacedNativeTemplateBuffs ?? new BlueprintBuff[0]).Select(buff => buff?.AssetGuid).ToArray())).ToList();
                failures.AddRange(InheritedDeliberateTemplateContract.Graph(wired, celestialTemplate, fiendishTemplate,
                    celestialSmite, fiendishSmite, nativeIds).Select(value => "deliberate-graph:" + value));
                bool celestial = item.ProjectTemplate == "celestial";
                expectTemplate = celestial ? celestialTemplate : fiendishTemplate;
                expectSmite = celestial ? celestialSmite : fiendishSmite;
                record["deliberateTemplate"] = new JObject {
                    ["unitHitDice"] = hitDice, ["band"] = band,
                    ["independentlyExpected"] = new JObject { ["celestialTemplate"] = celestialTemplate, ["fiendishTemplate"] = fiendishTemplate,
                        ["celestialSmite"] = celestialSmite, ["fiendishSmite"] = fiendishSmite, ["thisCast"] = expectTemplate + "+" + expectSmite },
                    ["childGraph"] = new JArray(wired.Select(view => new JObject { ["celestialTemplate"] = view.CelestialTemplate,
                        ["fiendishTemplate"] = view.FiendishTemplate, ["celestialSmite"] = view.CelestialSmite,
                        ["fiendishSmite"] = view.FiendishSmite, ["replacedNative"] = new JArray(view.ReplacedNative) })) };
            }
            string[] graphApplies = AlternateSummoningGraph.Objects<Kingmaker.UnitLogic.Mechanics.Actions.ContextActionApplyBuff>(child.ComponentsArray)
                .Where(action => action.Buff != null && nativeIds.Contains(action.Buff.AssetGuid))
                .Select(action => action.Buff.AssetGuid).Distinct().OrderBy(value => value, StringComparer.Ordinal).ToArray();
            record["childGraphNativeApplications"] = new JArray(graphApplies);
            record["childGraphRemovals"] = AlternateSummoningGraph.Objects<ContextActionRemoveInheritedSummonTemplates>(child.ComponentsArray).Count;

            object[] before = SnapshotReferences(fixture.SceneEntities);
            ExpandedSummoningRuleCapture.Clear();
            ExpandedSummoningRuleDurationCapture.Clear();
            bool converted = data.ConvertedFrom != null && ReferenceEquals(data.ConvertedFrom.Blueprint, parent) &&
                ReferenceEquals(data.Blueprint, child) && parent.HasVariant(child);
            record["sourceIsVariantOfParent"] = converted;
            if (!converted) failures.Add("the cast is not this child converted from this parent");

            // Debit: snapshots and the native spend/restore events of the caster during the window only.
            Dictionary<string, int> sourcesBefore = InheritedSources(caster);
            var debit = new Cleanup2SourceDebit(caster);
            var events = new List<string>();
            _inheritedCaster = caster;
            _inheritedEvents = events;
            string result = "NotStarted";
            bool started = false, acted = false;
            if (die != null) EventBus.Subscribe(die);
            try
            {
                UnitUseAbility command;
                var parameters = new Kingmaker.AreaLogic.Cutscenes.CutsceneParametersContext();
                using (parameters.Data) command = new UnitUseAbility(data, target);
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
            finally
            {
                _inheritedEvents = null;
                _inheritedCaster = null;
                if (die != null) EventBus.Unsubscribe(die);
            }
            Game.Instance.EntityCreator.Tick();
            Dictionary<string, int> sourcesAfter = InheritedSources(caster);
            IList<string> ledger = InheritedSourceLedger.Judge(events, sourcesBefore, sourcesAfter,
                cancel ? null : expected.SourceKey, cancel ? 0 : expected.Amount, cancel ? null : expected.Event);
            bool nativeDebit = expected.Native(debit);
            record["started"] = started; record["acted"] = acted; record["result"] = result;
            record["sourceEvents"] = new JArray(events);
            record["sourceKey"] = expected.SourceKey;
            record["expectedDebit"] = cancel ? 0 : expected.Amount;
            record["observedDebit"] = expected.SourceKey == null ? 0 :
                (sourcesBefore.ContainsKey(expected.SourceKey) ? sourcesBefore[expected.SourceKey] : 0) -
                (sourcesAfter.ContainsKey(expected.SourceKey) ? sourcesAfter[expected.SourceKey] : 0);
            record["sourceDebit"] = expected.Describe(debit);
            record["nativeDebitVerdict"] = nativeDebit;
            failures.AddRange(ledger.Select(value => "debit:" + value));
            if (!nativeDebit) failures.Add("debit:Cleanup2SourceDebit refused");

            UnitEntityData[] summoned = ExpandedSummoningRuleCapture.ToArray();
            record["summoned"] = summoned.Length;
            var durations = ExpandedSummoningRuleDurationCapture.Select(value => Tuple.Create(
                value.Unit == null ? "<null>" : value.Unit.UniqueId, value.BaseDuration.TotalSeconds, value.BonusDuration.TotalSeconds)).ToList();
            record["durations"] = new JArray(durations.Select(value => new JObject { ["unit"] = value.Item1,
                ["base"] = Math.Round(value.Item2, 2), ["bonus"] = Math.Round(value.Item3, 2) }));
            if (cancel)
            {
                if (!started) failures.Add("the command was not admitted before the cancellation");
                if (acted) failures.Add("the cancelled command acted");
                if (result != "Interrupt") failures.Add("result " + result + ", expected Interrupt");
                if (summoned.Length != 0) failures.Add("a cancelled cast summoned " + summoned.Length);
                if (durations.Count != 0) failures.Add("a cancelled cast produced a duration");
            }
            else
            {
                if (result != "Success") failures.Add("result " + result);
                bool[] live = summoned.Select(unit => unit != null && !unit.Destroyed && unit.IsInState &&
                    ContainsReference(fixture.SceneEntities, unit)).ToArray();
                List<string> unitBlueprints = summoned.Select(unit => unit == null ? "<null>" : unit.Blueprint.AssetGuid).ToList();
                List<string> unitIds = summoned.Select(unit => unit == null ? "<null>" : unit.UniqueId).ToList();
                failures.AddRange(InheritedQuantityContract.Output(minimum, maximum, unitGuid, unitBlueprints, unitIds, live)
                    .Select(value => "output:" + value));
                if (die != null)
                {
                    InheritedControlledQuantityResult controlled = InheritedControlledQuantityContract.Judge(recalculated,
                        InheritedD4PlusOneRank, context[AbilityRankType.ProjectilesCount], "D4", InheritedForcedDie, die.Relevant,
                        unitGuid, unitBlueprints, unitIds, live);
                    var controlledRecord = (JObject)record["controlledQuantity"];
                    controlledRecord["relevantRolls"] = new JArray(die.Relevant.Select(roll => new JObject { ["die"] = roll.Die,
                        ["rolls"] = roll.Rolls, ["result"] = roll.Result, ["overridden"] = roll.Overridden }));
                    controlledRecord["otherCasterRolls"] = new JArray(die.Other);
                    controlledRecord["expectedCount"] = controlled.ExpectedCount;
                    controlledRecord["actualCount"] = summoned.Length;
                    controlledRecord["distinctLiveUnits"] = unitIds.Distinct(StringComparer.Ordinal).Count();
                    controlledRecord["verdict"] = controlled.Verdict.ToString();
                    failures.AddRange(controlled.Failures.Select(value => "controlled-quantity:" + value));
                }
                if (expected.SummonerClass != null)
                {
                    int expectedRank; double expectedSeconds;
                    SummonSpawn host = expected.ClassRankHostSpawn;
                    failures.AddRange(InheritedClassRankDurationContract.Judge(host.DurationRate, host.DurationDice,
                        host.DurationBonus == null ? null : host.DurationBonus.Kind, host.DurationBonus == null ? null : host.DurationBonus.Rank,
                        expected.ClassRankHostConfig, expected.SummonerClass, expected.SummonerClassLevel, InheritedMasterSummonerLevel,
                        recalculated, context[AbilityRankType.Default], unitIds, durations, out expectedRank, out expectedSeconds)
                        .Select(value => "duration:" + value));
                    InheritedRankConfigView[] childDefaults = InheritedRankConfigs(child, AbilityRankType.Default);
                    record["classRankDuration"] = new JObject {
                        ["structural"] = new JObject { ["hostRate"] = host.DurationRate, ["hostDice"] = host.DurationDice,
                            ["hostBonus"] = host.DurationBonus == null ? null : host.DurationBonus.Kind + ":" + host.DurationBonus.Rank,
                            ["hostDefaultRankConfig"] = InheritedRankConfigRecord(expected.ClassRankHostConfig),
                            ["summonerClass"] = expected.SummonerClass, ["summonerClassLevel"] = expected.SummonerClassLevel,
                            ["requiredClassLevel"] = InheritedMasterSummonerLevel,
                            ["expectedRank"] = expectedRank, ["expectedSeconds"] = expectedSeconds },
                        ["native"] = new JObject { ["childDefaultRankConfigs"] = new JArray(childDefaults.Select(InheritedRankConfigRecord)),
                            ["recalculatedDefaultRank"] = context[AbilityRankType.Default] } };
                    record["expectedDuration"] = new JObject { ["base"] = expectedSeconds, ["bonus"] = 0,
                        ["source"] = expected.DurationSource + "; Default rank by Summoner class level " + expected.SummonerClassLevel };
                }
                else
                    failures.AddRange(InheritedDurationContract.Judge(unitIds, durations, expected.DurationBase, expected.DurationBonus)
                        .Select(value => "duration:" + value));
                InheritedObserveUnits(summoned, item, record, failures, nativeIds, expectTemplate, expectSmite);
            }
            foreach (UnitEntityData unit in summoned) CleanupExpandedSummoningUnit(unit);
            DrainExpandedSummoningDestroyQueue(fixture.SceneEntities, before);
            int lingering = SnapshotReferences(fixture.SceneEntities).Count(value => !before.Any(prior => ReferenceEquals(prior, value)));
            record["lingeringAfterCleanup"] = lingering;
            if (lingering != 0) failures.Add(lingering + " fixture entities lingered");
            ExpandedSummoningRuleCapture.Clear();
            ExpandedSummoningRuleDurationCapture.Clear();
            record["failures"] = new JArray(failures);
            record["pass"] = failures.Count == 0;
            return record;
        }

        private static void InheritedObserveUnits(UnitEntityData[] summoned, InheritedCase item,
            JObject record, List<string> failures, ICollection<string> nativeIds, string wantTemplate, string wantSmite)
        {
            Func<string, HashSet<string>> registered = prefix => new HashSet<string>(ExpandedSummoningBlueprints.Registered.BySymbol
                .Where(entry => entry.Key.StartsWith(prefix, StringComparison.Ordinal) && entry.Value != null)
                .Select(entry => entry.Value.AssetGuid), StringComparer.Ordinal);
            HashSet<string> projectTemplates = registered("KMG.Summoning.Template.");
            HashSet<string> celestialSmites = registered("KMG.Summoning.Smite.Celestial");
            HashSet<string> fiendishSmites = registered("KMG.Summoning.Smite.Fiendish");
            // The expected identities are the independently derived ones, never the child's own references.
            record["expectedProjectTemplate"] = wantTemplate ?? "none";
            record["expectedSmite"] = wantSmite ?? "none";
            BlueprintBuff marker = BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff;
            var units = new JArray();
            foreach (UnitEntityData unit in summoned.Where(value => value != null))
            {
                string[] buffs = unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value => value.Blueprint != null)
                    .Select(value => value.Blueprint.AssetGuid).ToArray();
                IList<string> sets = InheritedBuffSetContract.Judge(item.ProjectTemplate, buffs, nativeIds, projectTemplates,
                    celestialSmites, fiendishSmites, wantTemplate, wantSmite);
                BlueprintUnitFact[] innate = (unit.Blueprint.AddFacts ?? new BlueprintUnitFact[0]).Where(value => value != null).ToArray();
                string[] missingInnate = innate.Where(fact => !unit.Descriptor.HasFact(fact)).Select(fact => fact.name).ToArray();
                bool hasMarker = unit.Descriptor.HasFact(marker);
                units.Add(new JObject { ["unit"] = unit.UniqueId + ":" + unit.Blueprint.AssetGuid + ":" + unit.Blueprint.name,
                    ["nativeTemplates"] = new JArray(buffs.Where(nativeIds.Contains)),
                    ["projectTemplates"] = new JArray(buffs.Where(projectTemplates.Contains)),
                    ["celestialSmites"] = new JArray(buffs.Where(celestialSmites.Contains)),
                    ["fiendishSmites"] = new JArray(buffs.Where(fiendishSmites.Contains)),
                    ["innateFacts"] = innate.Length, ["missingInnate"] = new JArray(missingInnate), ["summonMarker"] = hasMarker,
                    ["buffCount"] = buffs.Length });
                failures.AddRange(sets.Select(value => unit.Blueprint.name + ":buffs:" + value));
                if (missingInnate.Length != 0) failures.Add(unit.Blueprint.name + " lost innate facts " + string.Join(",", missingInnate));
                if (!hasMarker) failures.Add(unit.Blueprint.name + " lost the native summon marker");
            }
            record["units"] = units;
        }
    }
}
