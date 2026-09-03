using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UI.LevelUp;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Save-free native RuleCalculateAbilityParams qualification for the
    /// spell-only elemental-affinity boundary.
    /// </summary>
    internal static class ElementalSpellAffinityScenario
    {
        internal const string EvidenceFileName =
            "elemental-spell-affinity.json";
        private const string FighterClassGuid =
            "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string WizardClassGuid =
            "ba34257984f4c41408ce1dc2004e342e";

        private sealed class DcEvidence
        {
            public string Source { get; set; }
            public string Blueprint { get; set; }
            public string Type { get; set; }
            public string ParentType { get; set; }
            public string Descriptor { get; set; }
            public bool HasSpellbook { get; set; }
            public bool HasSourceItem { get; set; }
            public int WithoutAffinity { get; set; }
            public int WithAffinity { get; set; }
            public int Delta { get { return WithAffinity - WithoutAffinity; } }

            public string Summary()
            {
                return Source + ":blueprint=" + Blueprint + ";type=" + Type +
                    ";parentType=" + ParentType + ";descriptor=" +
                    Descriptor + ";spellbook=" + HasSpellbook +
                    ";sourceItem=" + HasSourceItem + ";dc=" +
                    WithoutAffinity + "->" + WithAffinity + ";delta=" +
                    Delta;
            }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool ElementalModuleActive { get; set; }
            public bool SaveStateTouched { get; set; }
            public bool AddBonusDcIntOnly { get; set; }
            public string MatchingNativeSpell { get; set; }
            public string NonmatchingNativeSpell { get; set; }
            public DcEvidence MatchingOrdinarySpell { get; set; }
            public DcEvidence NonmatchingOrdinarySpell { get; set; }
            public DcEvidence MatchingParentVariant { get; set; }
            public DcEvidence MatchingParentAndVariant { get; set; }
            public DcEvidence IfritBurningHandsSla { get; set; }
            public DcEvidence StormsoulShockingGraspShape { get; set; }
            public DcEvidence ItemCastMatchingAbility { get; set; }
            public DcEvidence NonspellMatchingAbility { get; set; }
            public bool CleanupExact { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var evidence = new Evidence
            {
                SchemaVersion = 1,
                ElementalModuleActive = context.FeatureModules.Active
                    .ElementalRaces,
                SaveStateTouched = false
            };
            var created = new List<UnitEntityData>();
            UnitEntityData[] unitsBefore = Game.Instance.State.Units.All
                .ToArray();
            string stage = "inspect-dc-api";
            try
            {
                MethodInfo[] dcMethods = typeof(RuleCalculateAbilityParams)
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(value => value.Name == "AddBonusDC").ToArray();
                evidence.AddBonusDcIntOnly = dcMethods.Length == 1 &&
                    dcMethods[0].GetParameters().Length == 1 &&
                    dcMethods[0].GetParameters()[0].ParameterType ==
                        typeof(int);
                stage = "exercise-native-events";
                Exercise(evidence, created);
            }
            catch (Exception exception)
            {
                diagnostics.Add("stage=" + stage + ";exception=" +
                    exception);
            }
            finally
            {
                foreach (UnitEntityData unit in created.AsEnumerable()
                    .Reverse().ToArray())
                {
                    if (unit == null) continue;
                    unit.Commands.InterruptAll(true);
                    Game.Instance.State.Units.All.Remove(unit);
                    unit.Dispose();
                }
                evidence.CleanupExact = SameReferences(unitsBefore,
                    Game.Instance.State.Units.All.ToArray()) &&
                    created.All(value => value == null ||
                        !Game.Instance.State.Units.All.Contains(value));
            }

            AddAssertions(assertions, evidence);
            string path = Path.Combine(request.EvidenceDirectory,
                EvidenceFileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(evidence,
                Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver(),
                    PreserveReferencesHandling =
                        PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Error
                }));
            evidenceFiles.Add(path);
            diagnostics.Add("elementalSpellAffinitySha256=" + Hash(path));
            bool pass = assertions.All(value => value.Status ==
                RuntimeTestStatuses.Pass);
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult
            {
                SchemaVersion = 1,
                RunId = request.RunId,
                Scenario = request.Scenario,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" +
                    assembly.ManifestModule.ModuleVersionId + ";sha256=" +
                    Hash(assembly.Location) + ";pid=" +
                    Process.GetCurrentProcess().Id,
                GitCommit = Metadata(assembly, "GitCommit"),
                GameVersion = Application.version ?? string.Empty,
                StartUtc = DateTime.UtcNow.ToString("o"),
                EndUtc = string.Empty,
                Assertions = assertions,
                Diagnostics = diagnostics,
                Warnings = new List<string>(),
                ExceptionSummary = string.Empty,
                EvidenceFiles = evidenceFiles,
                AutomaticExitRequested = request.ExitAfterCompletion,
                EvidenceDirectory = request.EvidenceDirectory
            };
        }

        private static void Exercise(Evidence evidence,
            ICollection<UnitEntityData> created)
        {
            ElementalRaceBlueprintSet set = BlueprintBootstrap.ElementalRaces;
            if (set == null)
                throw new InvalidOperationException(
                    "The production elemental race set is unavailable.");
            BlueprintCharacterClass fighter = BlueprintLibraryLookup
                .RequireExact<BlueprintCharacterClass>(BlueprintBootstrap
                    .Library, FighterClassGuid,
                    "affinity Fighter multiclass fixture");
            BlueprintCharacterClass wizard = BlueprintLibraryLookup
                .RequireExact<BlueprintCharacterClass>(BlueprintBootstrap
                    .Library, WizardClassGuid,
                    "affinity Wizard spellbook fixture");
            BlueprintAbility matching = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(BlueprintBootstrap.Library,
                    ElementalRaceIdentityCatalog.BurningHandsGuid,
                    "native Burning Hands affinity fixture");
            if (wizard.Spellbook == null || wizard.Spellbook.SpellList == null ||
                !wizard.Spellbook.SpellList.Contains(matching) ||
                matching.Type != AbilityType.Spell ||
                (matching.SpellDescriptor & SpellDescriptor.Fire) == 0)
                throw new InvalidOperationException(
                    "Native Burning Hands is not an ordinary Fire spell on the Wizard list.");
            BlueprintAbility nonmatching = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintAbility>().Where(value =>
                    value != null && value.Type == AbilityType.Spell &&
                    value.Parent == null &&
                    wizard.Spellbook.SpellList.Contains(value) &&
                    (value.SpellDescriptor & SpellDescriptor.Fire) == 0 &&
                    !value.name.StartsWith("KMG_", StringComparison.Ordinal))
                .OrderBy(value => wizard.Spellbook.SpellList.GetLevel(value))
                .ThenBy(value => value.AssetGuid, StringComparer.Ordinal)
                .FirstOrDefault();
            if (nonmatching == null)
                throw new InvalidOperationException(
                    "No non-Fire ordinary Wizard spell fixture resolved.");

            UnitEntityData unit = CreateUnit(created);
            UnitDescriptor owner = unit.Descriptor;
            owner.Stats.Charisma.BaseValue = 18;
            EnsureFact(owner, set.Ifrit.Race);
            foreach (BlueprintFeature feature in set.Ifrit.Race.Features)
                EnsureFact(owner, feature);
            Advance(owner, fighter, 2);
            Advance(owner, wizard, 3);
            Spellbook spellbook = owner.GetSpellbook(wizard);
            if (spellbook == null)
                throw new InvalidOperationException(
                    "The disposable multiclass unit did not acquire its owned Wizard spellbook.");
            int wizardLevel = owner.Progression.GetClassLevel(wizard);
            while (spellbook.CasterLevel < wizardLevel)
                spellbook.AddCasterLevel();
            spellbook.UpdateAllSlotsSize(false);
            spellbook.Rest();
            if (wizardLevel != 3 || spellbook.CasterLevel != 3)
                throw new InvalidOperationException(
                    "The owned Wizard spellbook did not reach the committed three caster levels.");
            spellbook.AddKnown(wizard.Spellbook.SpellList.GetLevel(matching),
                matching, true);
            spellbook.AddKnown(wizard.Spellbook.SpellList.GetLevel(nonmatching),
                nonmatching, true);

            evidence.MatchingNativeSpell = Identity(matching);
            evidence.NonmatchingNativeSpell = Identity(nonmatching);
            evidence.MatchingOrdinarySpell = Measure(owner,
                set.Ifrit.Affinity, new AbilityData(matching, spellbook),
                "matching ordinary spell");
            evidence.NonmatchingOrdinarySpell = Measure(owner,
                set.Ifrit.Affinity, new AbilityData(nonmatching, spellbook),
                "nonmatching ordinary spell");

            BlueprintAbility variantParent = CreateAbility(
                "KMG_Runtime_AffinityVariant_ParentFire",
                AbilityType.Spell, SpellDescriptor.Fire, null);
            BlueprintAbility variantChild = CreateAbility(
                "KMG_Runtime_AffinityVariant_ChildNeutral",
                AbilityType.Spell, SpellDescriptor.None, variantParent);
            evidence.MatchingParentVariant = Measure(owner,
                set.Ifrit.Affinity,
                VariantData(variantParent, variantChild, spellbook),
                "matching parent ordinary spell variant");

            BlueprintAbility bothParent = CreateAbility(
                "KMG_Runtime_AffinityVariant_BothParentFire",
                AbilityType.Spell, SpellDescriptor.Fire, null);
            BlueprintAbility bothChild = CreateAbility(
                "KMG_Runtime_AffinityVariant_BothChildFire",
                AbilityType.Spell, SpellDescriptor.Fire, bothParent);
            evidence.MatchingParentAndVariant = Measure(owner,
                set.Ifrit.Affinity,
                VariantData(bothParent, bothChild, spellbook),
                "matching parent and child ordinary spell variant");

            EnsureFact(owner, set.Ifrit.SlaFeature);
            Ability ifritSla = owner.Abilities.GetAbility(
                set.Ifrit.SlaAbility);
            if (ifritSla == null)
                throw new InvalidOperationException(
                    "The production Ifrit Burning Hands SLA was not granted.");
            evidence.IfritBurningHandsSla = Measure(owner,
                set.Ifrit.Affinity, new AbilityData(ifritSla),
                "project Ifrit Burning Hands SLA");

            var itemBlueprint = ScriptableObject.CreateInstance<
                BlueprintItemEquipmentUsable>();
            itemBlueprint.name = "KMG_Runtime_Affinity_FireItem";
            itemBlueprint.Ability = matching;
            itemBlueprint.CasterLevel = 1;
            itemBlueprint.SpellLevel = 1;
            itemBlueprint.DC = 11;
            itemBlueprint.Charges = 1;
            var item = new ItemEntityUsable(itemBlueprint);
            var itemFact = new Ability(matching, owner)
            {
                SourceItem = item
            };
            evidence.ItemCastMatchingAbility = Measure(owner,
                set.Ifrit.Affinity, new AbilityData(itemFact),
                "native item-cast matching ability");

            BlueprintAbility nonspell = CreateAbility(
                "KMG_Runtime_Affinity_FireSupernatural",
                AbilityType.Supernatural, SpellDescriptor.Fire, null);
            evidence.NonspellMatchingAbility = Measure(owner,
                set.Ifrit.Affinity, new AbilityData(nonspell, owner),
                "matching supernatural nonspell ability");

            if (owner.HasFact(set.Ifrit.Affinity))
                owner.RemoveFact(set.Ifrit.Affinity);
            BlueprintAbility stormsoul = CreateAbility(
                "KMG_Runtime_Stormsoul_ShockingGrasp_SpellLike",
                AbilityType.SpellLike, SpellDescriptor.Electricity, null);
            evidence.StormsoulShockingGraspShape = Measure(owner,
                set.Sylph.Affinity, new AbilityData(stormsoul, owner),
                "unregistered Stormsoul Shocking Grasp SLA shape");
        }

        private static AbilityData VariantData(BlueprintAbility parent,
            BlueprintAbility child, Spellbook spellbook)
        {
            var canonical = new AbilityData(parent, spellbook)
            {
                OverrideSpellLevel = 1
            };
            return new AbilityData(canonical, child)
            {
                OverrideSpellLevel = 1
            };
        }

        private static BlueprintAbility CreateAbility(string name,
            AbilityType type, SpellDescriptor descriptor,
            BlueprintAbility parent)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = name;
            result.Type = type;
            result.Parent = parent;
            if (descriptor == SpellDescriptor.None)
            {
                result.ComponentsArray = Array.Empty<BlueprintComponent>();
                return result;
            }
            var component = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            component.Descriptor = descriptor;
            result.ComponentsArray = new BlueprintComponent[] { component };
            return result;
        }

        private static DcEvidence Measure(UnitDescriptor owner,
            BlueprintFeature affinity, AbilityData data, string source)
        {
            if (owner == null || affinity == null || data == null ||
                data.Blueprint == null)
                throw new ArgumentException(
                    "A complete affinity measurement is required.");
            if (owner.HasFact(affinity)) owner.RemoveFact(affinity);
            int without = data.CalculateParams().DC;
            EnsureFact(owner, affinity);
            int with = data.CalculateParams().DC;
            return new DcEvidence
            {
                Source = source,
                Blueprint = Identity(data.Blueprint),
                Type = data.Blueprint.Type.ToString(),
                ParentType = data.Blueprint.Parent == null ? "<none>" :
                    data.Blueprint.Parent.Type.ToString(),
                Descriptor = data.Blueprint.SpellDescriptor.ToString(),
                HasSpellbook = data.Spellbook != null,
                HasSourceItem = data.SourceItem != null,
                WithoutAffinity = without,
                WithAffinity = with
            };
        }

        private static UnitEntityData CreateUnit(
            ICollection<UnitEntityData> created)
        {
            UnitEntityData result = new ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            if (result == null || result.Descriptor == null)
                throw new InvalidOperationException(
                    "A disposable affinity unit was unavailable.");
            result.Descriptor.State.Immortality.Retain();
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "The disposable affinity unit could not be registered.");
            }
            created.Add(result);
            return result;
        }

        private static void Advance(UnitDescriptor owner,
            BlueprintCharacterClass characterClass, int levels)
        {
            Type type = typeof(LevelUpController);
            MethodInfo start = type.GetMethods(BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Static).Single(value =>
                    value.Name == "StartWithoutAssigningStaticInstance" &&
                    value.GetParameters().Length == 5);
            MethodInfo select = type.GetMethod("SelectClass",
                BindingFlags.Public | BindingFlags.Instance, null,
                new[] { typeof(BlueprintCharacterClass), typeof(bool) }, null);
            MethodInfo mechanics = type.GetMethod("ApplyClassMechanics",
                BindingFlags.Public | BindingFlags.Instance);
            MethodInfo apply = type.GetMethod("ApplyLevelup",
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance);
            MethodInfo cancel = type.GetMethod("Cancel", BindingFlags.Public |
                BindingFlags.Instance);
            if (start == null || select == null || mechanics == null ||
                apply == null || cancel == null)
                throw new MissingMethodException(
                    "The exact native affinity level-up surface is unavailable.");
            object charGen = Enum.Parse(start.GetParameters()[4].ParameterType,
                "CharGen", false);
            object controller = null;
            try
            {
                for (int index = 0; index < levels; index++)
                {
                    controller = start.Invoke(null, new object[]
                    {
                        owner, false, null, null, charGen
                    });
                    if (!(bool)select.Invoke(controller,
                            new object[] { characterClass, false }))
                        throw new InvalidOperationException(
                            "Disposable Wizard selection was rejected at level " +
                            (index + 1) + ".");
                    mechanics.Invoke(controller, null);
                    apply.Invoke(controller, new object[] { owner });
                    cancel.Invoke(controller, null);
                    controller = null;
                }
            }
            finally
            {
                if (controller != null) cancel.Invoke(controller, null);
            }
        }

        private static void EnsureFact(UnitDescriptor owner,
            BlueprintFeature feature)
        {
            if (owner.HasFact(feature)) return;
            if (owner.AddFact(feature) == null || !owner.HasFact(feature))
                throw new InvalidOperationException(
                    "The disposable unit rejected feature " +
                    feature.AssetGuid + ".");
        }

        private static void AddAssertions(
            ICollection<RuntimeTestAssertion> assertions, Evidence evidence)
        {
            Add(assertions, "elemental-affinity-module-active", "true",
                evidence.ElementalModuleActive.ToString(),
                evidence.ElementalModuleActive,
                "active feature-module snapshot");
            Add(assertions, "elemental-affinity-matching-spell", "+1 DC",
                Summary(evidence.MatchingOrdinarySpell),
                Is(evidence.MatchingOrdinarySpell, 1, true, false,
                    AbilityType.Spell),
                "native Wizard-list Fire spell through spellbook-backed AbilityData");
            Add(assertions, "elemental-affinity-nonmatching-spell", "+0 DC",
                Summary(evidence.NonmatchingOrdinarySpell),
                Is(evidence.NonmatchingOrdinarySpell, 0, true, false,
                    AbilityType.Spell),
                "native non-Fire Wizard-list spell through the same spellbook");
            Add(assertions, "elemental-affinity-parent-variant", "+1 total",
                Summary(evidence.MatchingParentVariant),
                IsVariant(evidence.MatchingParentVariant),
                "native AbilityData converted/variant chain with a matching parent");
            Add(assertions, "elemental-affinity-parent-and-variant",
                "+1 total", Summary(evidence.MatchingParentAndVariant),
                IsVariant(evidence.MatchingParentAndVariant),
                "matching descriptors on both nodes of one spellbook variant chain");
            Add(assertions, "elemental-affinity-ifrit-sla", "+0 DC",
                Summary(evidence.IfritBurningHandsSla),
                Is(evidence.IfritBurningHandsSla, 0, false, false,
                    AbilityType.SpellLike),
                "exact production Ifrit Burning Hands racial SLA AbilityData");
            Add(assertions, "elemental-affinity-stormsoul-sla-shape", "+0 DC",
                Summary(evidence.StormsoulShockingGraspShape),
                Is(evidence.StormsoulShockingGraspShape, 0, false, false,
                    AbilityType.SpellLike),
                "unregistered pre-heritage Electricity SpellLike fixture; production Stormsoul identity is deferred to Release A");
            Add(assertions, "elemental-affinity-item-cast", "+0 DC",
                Summary(evidence.ItemCastMatchingAbility),
                Is(evidence.ItemCastMatchingAbility, 0, false, true,
                    AbilityType.Spell),
                "native Ability fact with ItemEntityUsable source; item parameters bypass RuleCalculateAbilityParams");
            Add(assertions, "elemental-affinity-nonspell", "+0 DC",
                Summary(evidence.NonspellMatchingAbility),
                Is(evidence.NonspellMatchingAbility, 0, false, false,
                    AbilityType.Supernatural),
                "descriptor-bearing Supernatural AbilityData without a spellbook");
            Add(assertions, "elemental-affinity-dc-api",
                "AddBonusDC(Int32) only; no modifier descriptor overload",
                "intOnly=" + evidence.AddBonusDcIntOnly,
                evidence.AddBonusDcIntOnly,
                "reflection over installed RuleCalculateAbilityParams public API");
            Add(assertions, "elemental-affinity-save-free", "false",
                evidence.SaveStateTouched.ToString(),
                !evidence.SaveStateTouched,
                "request-local units and unregistered blueprints only");
            Add(assertions, "elemental-affinity-cleanup",
                "exact pre-run global-unit reference sequence",
                "cleanupExact=" + evidence.CleanupExact,
                evidence.CleanupExact,
                "finally interruption, removal, disposal, and reference comparison");
        }

        private static bool Is(DcEvidence value, int delta,
            bool hasSpellbook, bool hasSourceItem, AbilityType type)
        {
            return value != null && value.Delta == delta &&
                value.HasSpellbook == hasSpellbook &&
                value.HasSourceItem == hasSourceItem &&
                string.Equals(value.Type, type.ToString(),
                    StringComparison.Ordinal);
        }

        private static bool IsVariant(DcEvidence value)
        {
            return Is(value, 1, true, false, AbilityType.Spell) &&
                string.Equals(value.ParentType, AbilityType.Spell.ToString(),
                    StringComparison.Ordinal);
        }

        private static string Summary(DcEvidence value)
        {
            return value == null ? "<not-observed>" : value.Summary();
        }

        private static void Add(
            ICollection<RuntimeTestAssertion> assertions, string name,
            string expected, string observed, bool pass, string source)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = source
            });
        }

        private static string Identity(BlueprintAbility ability)
        {
            return ability.name + "[" +
                (string.IsNullOrWhiteSpace(ability.AssetGuid) ?
                    "unregistered" : ability.AssetGuid) + "]";
        }

        private static bool SameReferences<T>(IList<T> expected,
            IList<T> actual) where T : class
        {
            if (expected == null || actual == null ||
                expected.Count != actual.Count) return false;
            for (int index = 0; index < expected.Count; index++)
                if (!ReferenceEquals(expected[index], actual[index]))
                    return false;
            return true;
        }

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string Metadata(Assembly assembly, string key)
        {
            AssemblyMetadataAttribute value = assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute), false)
                .Cast<AssemblyMetadataAttribute>().FirstOrDefault(item =>
                    string.Equals(item.Key, key,
                        StringComparison.Ordinal));
            return value == null ? string.Empty : value.Value;
        }
    }
}
