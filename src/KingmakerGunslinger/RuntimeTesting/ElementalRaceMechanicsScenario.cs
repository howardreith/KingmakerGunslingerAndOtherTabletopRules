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
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;
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
    /// Save-free production mechanics qualification over disposable native
    /// units. No character selector, player party, or save API is mutated.
    /// </summary>
    internal static class ElementalRaceMechanicsScenario
    {
        internal const string EvidenceFileName =
            "elemental-race-mechanics.json";
        private const string FighterClassGuid =
            "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string WizardClassGuid =
            "ba34257984f4c41408ce1dc2004e342e";

        private sealed class RaceEvidence
        {
            public string Race { get; set; }
            public string RaceGuid { get; set; }
            public string RaceId { get; set; }
            public string StatDeltas { get; set; }
            public int SpeedBefore { get; set; }
            public int SpeedAfter { get; set; }
            public int PerceptionDelta { get; set; }
            public int PerceptionRacialBonus { get; set; }
            public int MatchingDamage { get; set; }
            public int NonmatchingDamage { get; set; }
            public int CharacterLevel { get; set; }
            public int FighterLevel { get; set; }
            public int WizardLevel { get; set; }
            public int ResourceMaximum { get; set; }
            public int ResourceInitial { get; set; }
            public int ResourceAfterLevels { get; set; }
            public int AvailableCountInitial { get; set; }
            public bool AvailableInitial { get; set; }
            public bool SpellbookAbsent { get; set; }
            public bool MaterialAbsent { get; set; }
            public int CasterLevel { get; set; }
            public int SpellLevel { get; set; }
            public int CurrentCharismaModifier { get; set; }
            public int ExpectedSaveDc { get; set; }
            public int SaveDc { get; set; }
            public int SaveDcAfterCharisma { get; set; }
            public int CharismaModifierAfterBonus { get; set; }
            public int SaveDcAfterCharismaPenalty { get; set; }
            public int CharismaModifierAfterPenalty { get; set; }
            public int SaveDcAfterTemporaryCleanup { get; set; }
            public bool AffinityActiveForSla { get; set; }
            public bool CancelBeforeCommitNoSpend { get; set; }
            public int ResourceAfterSpend { get; set; }
            public int ResourceAfterSpentLevelUp { get; set; }
            public int CasterLevelAfterSpentLevelUp { get; set; }
            public int AvailableCountAfterSpend { get; set; }
            public bool AvailableAfterSpend { get; set; }
            public int ResourceAfterRest { get; set; }
            public int PersistedAmount { get; set; }
            public bool PersistedBlueprintExact { get; set; }
            public bool PersistedRecordDistinct { get; set; }
            public int PersistedJsonLength { get; set; }
            public string MatchingSpell { get; set; }
            public string NonmatchingSpell { get; set; }
            public int MatchingDcWithoutAffinity { get; set; }
            public int MatchingDcWithAffinity { get; set; }
            public int NonmatchingDcWithoutAffinity { get; set; }
            public int NonmatchingDcWithAffinity { get; set; }
            public bool RuleContract { get; set; }
            public bool DamageContract { get; set; }
            public bool ResourceContract { get; set; }
            public bool ParameterContract { get; set; }
            public bool AffinityContract { get; set; }
            public bool PersistenceContract { get; set; }

            public string Summary()
            {
                return Race + ":stats=" + StatDeltas + ";speed=" +
                    SpeedBefore + "->" + SpeedAfter + ";perception=" +
                    PerceptionDelta + "(racial=" + PerceptionRacialBonus +
                    ");damage=" + MatchingDamage + "/" +
                    NonmatchingDamage + ";levels=" + FighterLevel + "+" +
                    WizardLevel + "=" + CharacterLevel + ";resource=" +
                    ResourceInitial + "->" + ResourceAfterSpend + "->" +
                    ResourceAfterRest + ";params=CL" + CasterLevel + "/SL" +
                    SpellLevel + "/DC" + SaveDc + ";affinity=" +
                    MatchingDcWithoutAffinity + "->" +
                    MatchingDcWithAffinity + "/non=" +
                    NonmatchingDcWithoutAffinity + "->" +
                    NonmatchingDcWithAffinity + ";persisted=" +
                    PersistedAmount + ";cha=" + CurrentCharismaModifier +
                    "->" + CharismaModifierAfterBonus + "->" +
                    CharismaModifierAfterPenalty + ";tempDc=" +
                    SaveDcAfterCharisma + "/" +
                    SaveDcAfterCharismaPenalty + "/" +
                    SaveDcAfterTemporaryCleanup + ";spentLevel=" +
                    ResourceAfterSpentLevelUp + "/CL" +
                    CasterLevelAfterSpentLevelUp;
            }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool ElementalModuleActive { get; set; }
            public bool SaveStateTouched { get; set; }
            public List<RaceEvidence> Races { get; set; }
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
                SaveStateTouched = false,
                Races = new List<RaceEvidence>()
            };
            var created = new List<UnitEntityData>();
            UnitEntityData[] unitsBefore = Game.Instance.State.Units.All
                .ToArray();
            UnitEntityData attacker = null;
            string stage = "resolve-production-contract";
            try
            {
                ElementalRaceBlueprintSet set = BlueprintBootstrap
                    .ElementalRaces;
                if (set == null || set.Count !=
                    ElementalRaceIdentityCatalog.IdentityCount)
                    throw new InvalidOperationException(
                        "The production elemental race set is unavailable.");
                BlueprintCharacterClass fighter = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, FighterClassGuid,
                        "elemental mechanics Fighter multiclass fixture");
                BlueprintCharacterClass wizard = BlueprintLibraryLookup
                    .RequireExact<BlueprintCharacterClass>(
                        BlueprintBootstrap.Library, WizardClassGuid,
                        "elemental mechanics Wizard multiclass fixture");
                Dictionary<SpellDescriptor, BlueprintAbility> matchingSpells =
                    ResolveMatchingSpells(set, wizard);
                BlueprintAbility nonmatching = ResolveNonmatchingSpell(
                    matchingSpells.Values, wizard);

                stage = "create-damage-source";
                attacker = CreateUnit(created, Vector3.zero);
                foreach (ElementalRaceBlueprints race in
                    set.OrderedBlueprints())
                {
                    stage = "exercise-" + race.Definition.Kind;
                    evidence.Races.Add(Exercise(race, fighter, wizard,
                        matchingSpells[race.Definition.Affinity], nonmatching,
                        attacker, created));
                }
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
            diagnostics.Add("elementalMechanicsSha256=" + Hash(path));
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

        private static RaceEvidence Exercise(
            ElementalRaceBlueprints blueprints,
            BlueprintCharacterClass fighter,
            BlueprintCharacterClass wizard,
            BlueprintAbility matchingSpell,
            BlueprintAbility nonmatchingSpell,
            UnitEntityData attacker,
            ICollection<UnitEntityData> created)
        {
            UnitEntityData unit = CreateUnit(created,
                new Vector3(1f, 0f, 0f));
            UnitEntityData replacement = CreateUnit(created,
                new Vector3(2f, 0f, 0f));
            UnitDescriptor owner = unit.Descriptor;
            owner.Stats.Strength.BaseValue = 10;
            owner.Stats.Dexterity.BaseValue = 10;
            owner.Stats.Constitution.BaseValue = 10;
            owner.Stats.Intelligence.BaseValue = 10;
            owner.Stats.Wisdom.BaseValue = 10;
            owner.Stats.Charisma.BaseValue = 18;
            var beforeStats = blueprints.Definition.Stats.ToDictionary(
                value => value.Stat,
                value => owner.Stats.GetStat(value.Stat).ModifiedValue);
            int perceptionBefore = owner.Stats.GetStat(
                StatType.SkillPerception).ModifiedValue;
            int speedBefore = owner.Stats.Speed.ModifiedValue;

            EnsureFact(owner, blueprints.Race);
            foreach (BlueprintFeature feature in blueprints.Race.Features)
                EnsureFact(owner, feature);

            int[] deltas = blueprints.Definition.Stats.Select(value =>
                owner.Stats.GetStat(value.Stat).ModifiedValue -
                    beforeStats[value.Stat]).ToArray();
            BlueprintFeatureBase keen = blueprints.Race.Features.Single(value =>
                string.Equals(value.AssetGuid,
                    ElementalRaceIdentityCatalog.KeenSensesGuid,
                    StringComparison.Ordinal));
            ModifiableValue perception = owner.Stats.GetStat(
                StatType.SkillPerception);
            var result = new RaceEvidence
            {
                Race = blueprints.Definition.DisplayName,
                RaceGuid = blueprints.Race.AssetGuid,
                RaceId = blueprints.Race.RaceId.ToString(),
                StatDeltas = string.Join(",",
                    blueprints.Definition.Stats.Select((value, index) =>
                        value.Stat + ":" + deltas[index]).ToArray()),
                SpeedBefore = speedBefore,
                SpeedAfter = owner.Stats.Speed.ModifiedValue,
                PerceptionDelta = owner.Stats.GetStat(
                    StatType.SkillPerception).ModifiedValue - perceptionBefore,
                PerceptionRacialBonus = perception.Modifiers.Where(value =>
                    value.ModDescriptor == ModifierDescriptor.Racial &&
                    value.Source != null && value.Source.Blueprint != null &&
                    ReferenceEquals(value.Source.Blueprint, keen))
                    .Sum(value => value.ModValue),
                MatchingSpell = Identity(matchingSpell),
                NonmatchingSpell = Identity(nonmatchingSpell)
            };

            result.RuleContract = blueprints.Race.Size == Size.Medium &&
                blueprints.Race.RaceId == Race.Aasimar &&
                deltas.SequenceEqual(blueprints.Definition.Stats.Select(
                    value => value.Value)) &&
                result.SpeedBefore == 30 && result.SpeedAfter ==
                    (blueprints.Definition.SlowAndSteady ? 20 : 30) &&
                result.PerceptionRacialBonus == 2 &&
                blueprints.Race.Features.All(owner.HasFact);

            unit.Damage = 0;
            int hpBefore = unit.HPLeft;
            DealEnergy(attacker, unit, blueprints.Definition.Resistance, 8);
            result.MatchingDamage = hpBefore - unit.HPLeft;
            unit.Damage = 0;
            hpBefore = unit.HPLeft;
            DamageEnergyType nonmatchingEnergy = blueprints.Definition
                .Resistance == DamageEnergyType.Fire ?
                    DamageEnergyType.Cold : DamageEnergyType.Fire;
            DealEnergy(attacker, unit, nonmatchingEnergy, 8);
            result.NonmatchingDamage = hpBefore - unit.HPLeft;
            unit.Damage = 0;
            result.DamageContract = result.MatchingDamage == 3 &&
                result.NonmatchingDamage == 8;

            BlueprintAbilityResource resource = blueprints.SlaResource;
            result.ResourceMaximum = resource.GetMaxAmount(owner);
            result.ResourceInitial = owner.Resources.GetResourceAmount(resource);
            Advance(owner, fighter, 2);
            Advance(owner, wizard, 3);
            result.CharacterLevel = owner.Progression.CharacterLevel;
            result.FighterLevel = owner.Progression.GetClassLevel(fighter);
            result.WizardLevel = owner.Progression.GetClassLevel(wizard);
            result.ResourceAfterLevels = owner.Resources.GetResourceAmount(
                resource);
            Spellbook affinitySpellbook = owner.GetSpellbook(wizard);
            if (affinitySpellbook == null)
                throw new InvalidOperationException(result.Race +
                    " did not retain its native Wizard spellbook.");

            Ability granted = owner.Abilities.GetAbility(
                blueprints.SlaAbility);
            if (granted == null)
                throw new InvalidOperationException(result.Race +
                    " did not receive its production racial SLA.");
            var data = new AbilityData(granted);
            result.AvailableCountInitial = data.GetAvailableForCastCount();
            result.AvailableInitial = data.IsAvailable;
            result.SpellbookAbsent = data.Spellbook == null;
            result.MaterialAbsent = !data.RequireMaterialComponent;
            var execution = data.CreateExecutionContext(
                new TargetWrapper(unit));
            result.CasterLevel = execution.Params.CasterLevel;
            result.SpellLevel = execution.Params.SpellLevel;
            result.CurrentCharismaModifier = owner.Stats.Charisma.Bonus;
            result.ExpectedSaveDc = ElementalRacialSpellLikePolicy
                .DifficultyClass(result.SpellLevel,
                    result.CurrentCharismaModifier);
            result.SaveDc = execution.Params.DC;
            result.AffinityActiveForSla = owner.HasFact(blueprints.Affinity);

            BlueprintFeature charismaBonus = CreateCharismaAdjustment(
                "KMG_Runtime_ElementalSla_CharismaBonus", 2,
                ModifierDescriptor.Enhancement);
            EnsureFact(owner, charismaBonus);
            result.CharismaModifierAfterBonus = owner.Stats.Charisma.Bonus;
            result.SaveDcAfterCharisma = data.CreateExecutionContext(
                new TargetWrapper(unit)).Params.DC;
            owner.RemoveFact(charismaBonus);

            BlueprintFeature charismaPenalty = CreateCharismaAdjustment(
                "KMG_Runtime_ElementalSla_CharismaPenalty", -2,
                ModifierDescriptor.UntypedStackable);
            EnsureFact(owner, charismaPenalty);
            result.CharismaModifierAfterPenalty = owner.Stats.Charisma.Bonus;
            result.SaveDcAfterCharismaPenalty = data.CreateExecutionContext(
                new TargetWrapper(unit)).Params.DC;
            owner.RemoveFact(charismaPenalty);
            result.SaveDcAfterTemporaryCleanup = data.CreateExecutionContext(
                new TargetWrapper(unit)).Params.DC;

            int beforeCancel = owner.Resources.GetResourceAmount(resource);
            var canceled = new UnitUseAbility(data, new TargetWrapper(unit));
            unit.Commands.InterruptAll(true);
            result.CancelBeforeCommitNoSpend =
                owner.Resources.GetResourceAmount(resource) == beforeCancel &&
                !canceled.IsStarted;

            InvokeSpend(data);
            result.ResourceAfterSpend = owner.Resources.GetResourceAmount(
                resource);
            result.AvailableCountAfterSpend = data
                .GetAvailableForCastCount();
            result.AvailableAfterSpend = data.IsAvailable;
            Advance(owner, fighter, 1);
            result.ResourceAfterSpentLevelUp = owner.Resources
                .GetResourceAmount(resource);
            result.CasterLevelAfterSpentLevelUp = data.CreateExecutionContext(
                new TargetWrapper(unit)).Params.CasterLevel;
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            result.ResourceAfterRest = owner.Resources.GetResourceAmount(
                resource);

            result.ResourceContract = result.ResourceMaximum == 1 &&
                result.ResourceInitial == 1 &&
                result.ResourceAfterLevels == 1 &&
                result.AvailableCountInitial == 1 &&
                result.AvailableInitial && result.SpellbookAbsent &&
                result.MaterialAbsent && result.CancelBeforeCommitNoSpend &&
                result.ResourceAfterSpend == 0 &&
                result.AvailableCountAfterSpend == 0 &&
                !result.AvailableAfterSpend &&
                result.ResourceAfterSpentLevelUp == 0 &&
                result.ResourceAfterRest == 1;
            result.ParameterContract = result.CharacterLevel == 5 &&
                result.FighterLevel == 2 && result.WizardLevel == 3 &&
                result.CasterLevel == 5 && result.SpellLevel == 1 &&
                result.ExpectedSaveDc == result.SaveDc &&
                result.AffinityActiveForSla &&
                result.CharismaModifierAfterBonus ==
                    result.CurrentCharismaModifier + 1 &&
                result.SaveDcAfterCharisma == result.SaveDc + 1 &&
                result.CharismaModifierAfterPenalty ==
                    result.CurrentCharismaModifier - 1 &&
                result.SaveDcAfterCharismaPenalty == result.SaveDc - 1 &&
                result.SaveDcAfterTemporaryCleanup == result.SaveDc &&
                result.CasterLevelAfterSpentLevelUp == 6;

            InvokeSpend(data);
            UnitAbilityResource record = owner.Resources
                .PersistantResources.Single(value => value != null &&
                    ReferenceEquals(value.Blueprint, resource));
            string json = JsonConvert.SerializeObject(record,
                Formatting.None, Kingmaker.EntitySystem.Persistence
                    .JsonUtility.DefaultJsonSettings.DefaultSettings);
            UnitAbilityResource clone = JsonConvert.DeserializeObject<
                UnitAbilityResource>(json, Kingmaker.EntitySystem.Persistence
                    .JsonUtility.DefaultJsonSettings.DefaultSettings);
            result.PersistedJsonLength = json.Length;
            result.PersistedRecordDistinct = clone != null &&
                !ReferenceEquals(record, clone);
            result.PersistedBlueprintExact = clone != null &&
                ReferenceEquals(clone.Blueprint, resource);
            replacement.Descriptor.Resources.PersistantResources =
                new List<UnitAbilityResource> { clone };
            result.PersistedAmount = replacement.Descriptor.Resources
                .GetResourceAmount(resource);
            result.PersistenceContract = result.PersistedJsonLength > 0 &&
                result.PersistedRecordDistinct &&
                result.PersistedBlueprintExact &&
                result.PersistedAmount == 0;

            if (owner.HasFact(blueprints.Affinity))
                owner.RemoveFact(blueprints.Affinity);
            result.MatchingDcWithoutAffinity = CalculateDc(matchingSpell,
                affinitySpellbook, unit);
            result.NonmatchingDcWithoutAffinity = CalculateDc(
                nonmatchingSpell, affinitySpellbook, unit);
            EnsureFact(owner, blueprints.Affinity);
            result.MatchingDcWithAffinity = CalculateDc(matchingSpell,
                affinitySpellbook, unit);
            result.NonmatchingDcWithAffinity = CalculateDc(nonmatchingSpell,
                affinitySpellbook, unit);
            result.AffinityContract =
                result.MatchingDcWithAffinity ==
                    result.MatchingDcWithoutAffinity + 1 &&
                result.NonmatchingDcWithAffinity ==
                    result.NonmatchingDcWithoutAffinity;
            return result;
        }

        private static Dictionary<SpellDescriptor, BlueprintAbility>
            ResolveMatchingSpells(ElementalRaceBlueprintSet set,
                BlueprintCharacterClass wizard)
        {
            BlueprintAbility[] spells = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintAbility>().Where(value =>
                    value != null && value.Type == AbilityType.Spell &&
                    value.Parent == null &&
                    wizard.Spellbook.SpellList.Contains(value) &&
                    !string.IsNullOrWhiteSpace(value.AssetGuid) &&
                    !value.name.StartsWith("KMG_", StringComparison.Ordinal))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal)
                .ToArray();
            var result = new Dictionary<SpellDescriptor, BlueprintAbility>();
            foreach (SpellDescriptor descriptor in set.OrderedBlueprints()
                .Select(value => value.Definition.Affinity).Distinct())
            {
                BlueprintAbility match = spells.FirstOrDefault(value =>
                    (value.SpellDescriptor & descriptor) != 0);
                if (match == null)
                    throw new InvalidOperationException(
                        "No native spell exposes descriptor " + descriptor +
                        ".");
                result.Add(descriptor, match);
            }
            return result;
        }

        private static BlueprintAbility ResolveNonmatchingSpell(
            IEnumerable<BlueprintAbility> matching,
            BlueprintCharacterClass wizard)
        {
            SpellDescriptor elemental = SpellDescriptor.Fire |
                SpellDescriptor.Acid | SpellDescriptor.Electricity |
                SpellDescriptor.Cold;
            var excluded = new HashSet<BlueprintAbility>(matching);
            BlueprintAbility result = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintAbility>().Where(value =>
                    value != null && value.Type == AbilityType.Spell &&
                    value.Parent == null && !excluded.Contains(value) &&
                    wizard.Spellbook.SpellList.Contains(value) &&
                    (value.SpellDescriptor & elemental) == 0 &&
                    !value.name.StartsWith("KMG_", StringComparison.Ordinal))
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal)
                .FirstOrDefault();
            if (result == null)
                throw new InvalidOperationException(
                    "No native non-elemental spell fixture resolved.");
            return result;
        }

        private static UnitEntityData CreateUnit(
            ICollection<UnitEntityData> created, Vector3 position)
        {
            UnitEntityData result = new Kingmaker.UI.LevelUp.ChargenUnit(
                BlueprintRoot.Instance.DefaultPlayerCharacter).Unit;
            if (result == null || result.Descriptor == null ||
                result.Descriptor.Resources == null)
                throw new InvalidOperationException(
                    "A disposable elemental mechanics unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 100;
            result.Descriptor.State.Immortality.Retain();
            SetExactProperty(result, "Position", position);
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A disposable elemental mechanics unit could not be registered.");
            }
            created.Add(result);
            return result;
        }

        private static void EnsureFact(UnitDescriptor owner,
            BlueprintUnitFact blueprint)
        {
            if (owner.HasFact(blueprint)) return;
            if (owner.AddFact(blueprint) == null || !owner.HasFact(blueprint))
                throw new InvalidOperationException(
                    "Disposable unit rejected fact " + blueprint.AssetGuid +
                    ".");
        }

        private static BlueprintFeature CreateCharismaAdjustment(string name,
            int value, ModifierDescriptor descriptor)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = name;
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.Groups = Array.Empty<FeatureGroup>();
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.Stat = StatType.Charisma;
            bonus.Value = value;
            bonus.Descriptor = descriptor;
            feature.ComponentsArray = new BlueprintComponent[] { bonus };
            return feature;
        }

        private static void DealEnergy(UnitEntityData attacker,
            UnitEntityData target, DamageEnergyType energy, int amount)
        {
            Rulebook.Trigger(new RuleDealDamage(attacker, target,
                new DamageBundle(new EnergyDamage(
                    new DiceFormula(0, DiceType.D6), energy)
                {
                    PreRolledValue = amount
                }))
            {
                DisablePrecisionDamage = true
            });
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
            if (select == null || mechanics == null || apply == null ||
                cancel == null)
                throw new MissingMethodException(
                    "The exact native elemental level-up surface is unavailable.");
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
                            "Disposable class selection was rejected for " +
                            characterClass.name + " level " + (index + 1) +
                            ".");
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

        private static void InvokeSpend(AbilityData data)
        {
            MethodInfo spend = typeof(AbilityData).GetMethod("Spend",
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (spend == null)
                throw new MissingMethodException(typeof(AbilityData).FullName,
                    "Spend");
            spend.Invoke(data, new object[0]);
        }

        private static int CalculateDc(BlueprintAbility spell,
            Spellbook spellbook, UnitEntityData target)
        {
            var data = new AbilityData(spell, spellbook);
            return data.CreateExecutionContext(
                new TargetWrapper(target)).Params.DC;
        }

        private static string Identity(BlueprintAbility ability)
        {
            return ability.name + "[" + ability.AssetGuid + "]";
        }

        private static void SetExactProperty(object value, string name,
            object propertyValue)
        {
            PropertyInfo property = value.GetType().GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance);
            MethodInfo setter = property == null ? null :
                property.GetSetMethod(true);
            if (setter == null)
                throw new MissingMemberException(value.GetType().FullName,
                    name);
            setter.Invoke(value, new[] { propertyValue });
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

        private static void AddAssertions(
            ICollection<RuntimeTestAssertion> assertions, Evidence evidence)
        {
            Add(assertions, "elemental-race-mechanics-count", "4",
                evidence.Races.Count.ToString(), evidence.Races.Count == 4,
                "four exact production ElementalRaceBlueprints exercises");
            foreach (RaceEvidence race in evidence.Races)
            {
                string key = race.Race.ToLowerInvariant();
                string observed = race.Summary();
                Add(assertions, "elemental-" + key + "-base-rules",
                    "Medium Aasimar donor identity; exact stats, speed, and +2 Perception",
                    observed, race.RuleContract,
                    "actual BlueprintRace facts on a disposable native UnitDescriptor");
                Add(assertions, "elemental-" + key + "-resistance",
                    "8 matching energy becomes 3; 8 nonmatching remains 8",
                    observed, race.DamageContract,
                    "native RuleDealDamage and EnergyDamage resolution");
                Add(assertions, "elemental-" + key + "-sla-resource",
                    "one use, cancel 0, commit 1, zero blocks, spent level-up stays zero, rest restores one",
                    observed, race.ResourceContract,
                    "production SLA fact, AbilityData.Spend, and native ApplyRest");
                Add(assertions, "elemental-" + key + "-sla-parameters",
                    "2 Fighter + 3 Wizard => CL 5, exact DC 11 + current Charisma modifier, temporary +/-2 changes DC +/-1, affinity +0",
                    observed, race.ParameterContract,
                    "actual LevelUpController and AbilityExecutionContext params");
                Add(assertions, "elemental-" + key + "-affinity",
                    "+1 matching descriptor DC exactly once; nonmatching +0",
                    observed, race.AffinityContract,
                    "native RuleCalculateAbilityParams event through AbilityData context");
                Add(assertions, "elemental-" + key + "-resource-persistence",
                    "spent resource round-trips as exact blueprint with amount 0",
                    observed, race.PersistenceContract,
                    "native JsonUtility settings and UnitAbilityResource constructor");
            }
            Add(assertions, "elemental-mechanics-save-free",
                "no save API or player-party mutation", "saveStateTouched=" +
                evidence.SaveStateTouched, !evidence.SaveStateTouched,
                "request-local disposable fixtures only");
            Add(assertions, "elemental-mechanics-cleanup",
                "exact pre-run global-unit reference sequence",
                "cleanupExact=" + evidence.CleanupExact,
                evidence.CleanupExact,
                "finally interruption, removal, disposal, and exact snapshot comparison");
        }

        private static void Add(
            ICollection<RuntimeTestAssertion> assertions, string name,
            string expected, string observed, bool pass, string evidence)
        {
            assertions.Add(new RuntimeTestAssertion
            {
                Name = name,
                Expected = expected,
                Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass :
                    RuntimeTestStatuses.Fail,
                Evidence = evidence
            });
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
