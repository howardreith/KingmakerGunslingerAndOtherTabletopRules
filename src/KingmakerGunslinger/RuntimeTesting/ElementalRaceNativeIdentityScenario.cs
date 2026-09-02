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
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
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
    /// Save-free qualification of Oread's exact native Dwarf movement
    /// contract and the installed Aasimar/Tiefling outsider precedent.
    /// </summary>
    internal static class ElementalRaceNativeIdentityScenario
    {
        internal const string EvidenceFileName =
            "elemental-race-native-identity.json";

        private const string HumanRaceGuid =
            "0a5d473ead98b0646b94495af250fdc4";
        private const string AasimarRaceGuid =
            "b7f02ba92b363064fb873963bec275ee";
        private const string TieflingRaceGuid =
            "5c4e42124dc2b4647af6e36cf2590500";
        private const string DwarfRaceGuid =
            "c4faf439f0e70bd40b5e36ee80d06be7";
        private const string HoldPersonGuid =
            "c7104f7526c4c524f91474614054547e";
        private const string CharmPersonGuid =
            "1af9d5995090e5a4185a30decf0959ad";
        private const string EnlargePersonGuid =
            "c60969e7f264e6d4b84a1499fdcf9039";
        private const string ReducePersonGuid =
            "4e0e9aba6447d514f88eff1464cc4763";
        private const string HeavyArmorGuid =
            "559b0b6f194656c428c403a000ceee78";

        private sealed class MovementEvidence
        {
            public string Race { get; set; }
            public string RaceGuid { get; set; }
            public int UnarmoredSpeed { get; set; }
            public int MediumArmorSpeed { get; set; }
            public int HeavyArmorSpeed { get; set; }
            public int HeavyEncumbranceSpeed { get; set; }
            public int RestoredSpeed { get; set; }
            public int NativeHeavyPenalty { get; set; }
            public string EncumbranceBefore { get; set; }
            public string EncumbranceDuring { get; set; }
            public string EncumbranceAfter { get; set; }
            public string LoadItem { get; set; }
            public int LoadCount { get; set; }
            public float WeightBefore { get; set; }
            public float WeightDuring { get; set; }
            public int MediumCapacity { get; set; }
            public int HeavyCapacity { get; set; }
            public int SpeedWithBonus { get; set; }
            public int SpeedWithBonusAndPenalty { get; set; }

            public string Summary()
            {
                return Race + ":speed=" + UnarmoredSpeed + "/" +
                    MediumArmorSpeed + "/" + HeavyArmorSpeed + "/" +
                    HeavyEncumbranceSpeed + "->" + RestoredSpeed +
                    ";nativePenalty=" + NativeHeavyPenalty +
                    ";encumbrance=" + EncumbranceBefore + "->" +
                    EncumbranceDuring + "->" + EncumbranceAfter +
                    ";generic=" + SpeedWithBonus + "/" +
                    SpeedWithBonusAndPenalty;
            }
        }

        private sealed class IdentityEvidence
        {
            public string Race { get; set; }
            public string RaceGuid { get; set; }
            public string RaceId { get; set; }
            public bool OutsiderFact { get; set; }
            public string HeritageGuid { get; set; }
            public bool HoldPersonTargetable { get; set; }
            public bool CharmPersonTargetable { get; set; }
            public bool EnlargePersonTargetable { get; set; }
            public bool ReducePersonTargetable { get; set; }
            public bool OutsiderPrerequisite { get; set; }
            public bool NoOutsiderPrerequisite { get; set; }
            public bool SelfRacePrerequisite { get; set; }
            public bool AasimarRacePrerequisite { get; set; }
            public bool IfritRacePrerequisite { get; set; }

            public string TargetVector()
            {
                return HoldPersonTargetable + "," +
                    CharmPersonTargetable + "," +
                    EnlargePersonTargetable + "," +
                    ReducePersonTargetable;
            }

            public string Summary()
            {
                return Race + ":outsider=" + OutsiderFact +
                    ";targets=" + TargetVector() + ";prerequisites=" +
                    OutsiderPrerequisite + "/" +
                    NoOutsiderPrerequisite + "/" +
                    SelfRacePrerequisite + "/" +
                    AasimarRacePrerequisite + "/" +
                    IfritRacePrerequisite + ";heritage=" +
                    HeritageGuid;
            }
        }

        private sealed class Evidence
        {
            public int SchemaVersion { get; set; }
            public bool SaveStateTouched { get; set; }
            public string SlowAndSteadyGuid { get; set; }
            public string SlowAndSteadyComponents { get; set; }
            public string MediumArmor { get; set; }
            public string HeavyArmor { get; set; }
            public List<MovementEvidence> Movement { get; set; }
            public List<IdentityEvidence> Identity { get; set; }
            public bool CleanupExact { get; set; }
        }

        internal static RuntimeTestResult Run(ModContext context,
            RuntimeTestRequest request)
        {
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var evidenceFiles = new List<string>();
            var created = new List<UnitEntityData>();
            var transient = new List<UnityEngine.Object>();
            UnitEntityData[] unitsBefore = Game.Instance.State.Units.All
                .ToArray();
            var evidence = new Evidence
            {
                SchemaVersion = 1,
                SaveStateTouched = false,
                Movement = new List<MovementEvidence>(),
                Identity = new List<IdentityEvidence>()
            };
            string stage = "resolve-production-contract";
            try
            {
                LibraryScriptableObject library = BlueprintBootstrap.Library;
                ElementalRaceBlueprintSet set = BlueprintBootstrap
                    .ElementalRaces;
                if (library == null || set == null || set.Count !=
                    ElementalRaceIdentityCatalog.IdentityCount)
                    throw new InvalidOperationException(
                        "The live production elemental race set is unavailable.");

                BlueprintRace human = Require<BlueprintRace>(library,
                    HumanRaceGuid, "Human race");
                BlueprintRace aasimar = Require<BlueprintRace>(library,
                    AasimarRaceGuid, "Aasimar race");
                BlueprintRace tiefling = Require<BlueprintRace>(library,
                    TieflingRaceGuid, "Tiefling race");
                BlueprintRace dwarf = Require<BlueprintRace>(library,
                    DwarfRaceGuid, "Dwarf race");
                BlueprintFeature outsider = Require<BlueprintFeature>(library,
                    ElementalRaceIdentityCatalog.OutsiderTypeGuid,
                    "Outsider type");
                BlueprintFeature slow = Require<BlueprintFeature>(library,
                    ElementalRaceIdentityCatalog.SlowAndSteadyGuid,
                    "Dwarf Slow and Steady");
                BlueprintItemArmor medium = library.GetAllBlueprints()
                    .OfType<BlueprintItemArmor>().Where(value =>
                        value != null && value.Type != null &&
                        value.Type.IsArmor && value.Type.ProficiencyGroup ==
                            ArmorProficiencyGroup.Medium)
                    .OrderBy(value => value.AssetGuid,
                        StringComparer.Ordinal).FirstOrDefault();
                BlueprintItemArmor heavy = Require<BlueprintItemArmor>(
                    library, HeavyArmorGuid, "audited heavy armor");
                if (medium == null || heavy.Type == null ||
                    heavy.Type.ProficiencyGroup !=
                        ArmorProficiencyGroup.Heavy)
                    throw new InvalidOperationException(
                        "Native medium/heavy armor fixtures are unavailable.");

                evidence.SlowAndSteadyGuid = slow.AssetGuid;
                evidence.SlowAndSteadyComponents = string.Join(",",
                    (slow.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .Select(value => value == null ? "<null>" :
                        value.GetType().FullName).ToArray());
                evidence.MediumArmor = Identity(medium);
                evidence.HeavyArmor = Identity(heavy);

                stage = "movement-oread";
                evidence.Movement.Add(ExerciseMovement(set.Oread.Race,
                    medium, heavy, created, transient));
                stage = "movement-dwarf";
                evidence.Movement.Add(ExerciseMovement(dwarf, medium,
                    heavy, created, transient));
                stage = "movement-human";
                evidence.Movement.Add(ExerciseMovement(human, medium,
                    heavy, created, transient));

                stage = "native-identity";
                ExerciseIdentity(library, set, human, aasimar, tiefling,
                    outsider, evidence.Identity, created, transient);
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
                foreach (UnityEngine.Object value in transient.AsEnumerable()
                    .Reverse().ToArray())
                    if (value != null)
                        UnityEngine.Object.DestroyImmediate(value);
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
            diagnostics.Add("elementalNativeIdentitySha256=" + Hash(path));
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

        private static MovementEvidence ExerciseMovement(BlueprintRace race,
            BlueprintItemArmor medium, BlueprintItemArmor heavy,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient)
        {
            UnitEntityData unit = CreateUnit(null, created, transient,
                new Vector3(created.Count * 3f, 0f, 0f), "Movement");
            UnitDescriptor owner = unit.Descriptor;
            ApplyRace(owner, race, null);
            var result = new MovementEvidence
            {
                Race = SafeName(race),
                RaceGuid = race.AssetGuid,
                UnarmoredSpeed = owner.Stats.Speed.ModifiedValue
            };

            result.MediumArmorSpeed = SpeedWithArmor(unit, medium);
            result.HeavyArmorSpeed = SpeedWithArmor(unit, heavy);
            result.NativeHeavyPenalty = UnitPartEncumbrance.GetSpeedPenalty(
                owner, Encumbrance.Heavy);

            BlueprintFeature bonus = CreateSpeedFeature(
                "KMG_Runtime_ElementalRace_SpeedBonus", 10, transient);
            BlueprintFeature penalty = CreateSpeedFeature(
                "KMG_Runtime_ElementalRace_SpeedPenalty", -5, transient);
            EnsureFact(owner, bonus);
            result.SpeedWithBonus = owner.Stats.Speed.ModifiedValue;
            EnsureFact(owner, penalty);
            result.SpeedWithBonusAndPenalty = owner.Stats.Speed.ModifiedValue;
            owner.RemoveFact(penalty);
            owner.RemoveFact(bonus);

            SetStrengthForHeavyArmor(owner, heavy.Weight);
            var capacity = EncumbranceHelper.GetCarryingCapacity(owner);
            result.EncumbranceBefore = EncumbranceHelper.GetEncumbrance(owner)
                .ToString();
            result.WeightBefore = capacity.CurrentWeight;
            result.MediumCapacity = capacity.Medium;
            result.HeavyCapacity = capacity.Heavy;
            result.LoadItem = Identity(heavy);
            result.LoadCount = 1;
            ItemEntityArmor loadArmor = null;
            try
            {
                loadArmor = new ItemEntityArmor(heavy);
                unit.Body.Armor.InsertItem(loadArmor);
                var loaded = EncumbranceHelper.GetCarryingCapacity(owner);
                result.WeightDuring = loaded.CurrentWeight;
                ApplyCalculatedEncumbrance(owner);
                result.EncumbranceDuring = owner.Encumbrance.ToString();
                unit.Body.Armor.RemoveItem(false);
                loadArmor.Dispose();
                loadArmor = null;
                result.HeavyEncumbranceSpeed =
                    owner.Stats.Speed.ModifiedValue;
            }
            finally
            {
                if (unit.Body.Armor.HasArmor)
                    unit.Body.Armor.RemoveItem(false);
                if (loadArmor != null) loadArmor.Dispose();
            }
            ApplyCalculatedEncumbrance(owner);
            result.EncumbranceAfter = owner.Encumbrance.ToString();
            result.RestoredSpeed = owner.Stats.Speed.ModifiedValue;
            return result;
        }

        private static void ExerciseIdentity(
            LibraryScriptableObject library, ElementalRaceBlueprintSet set,
            BlueprintRace human, BlueprintRace aasimar,
            BlueprintRace tiefling, BlueprintFeature outsider,
            ICollection<IdentityEvidence> results,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient)
        {
            BlueprintAbility hold = Require<BlueprintAbility>(library,
                HoldPersonGuid, "Hold Person");
            BlueprintAbility charm = Require<BlueprintAbility>(library,
                CharmPersonGuid, "Charm Person");
            BlueprintAbility enlarge = Require<BlueprintAbility>(library,
                EnlargePersonGuid, "Enlarge Person");
            BlueprintAbility reduce = Require<BlueprintAbility>(library,
                ReducePersonGuid, "Reduce Person");

            BlueprintFaction hostileFaction;
            BlueprintFaction targetFaction;
            CreateFactionPair(transient, out hostileFaction,
                out targetFaction);
            UnitEntityData hostileCaster = CreateUnit(hostileFaction,
                created, transient, new Vector3(40f, 0f, 0f),
                "HostileCaster");
            UnitEntityData friendlyCaster = CreateUnit(targetFaction,
                created, transient, new Vector3(40f, 0f, 0f),
                "FriendlyCaster");
            BlueprintRace[] races = new[]
            {
                human, aasimar, tiefling, set.Ifrit.Race,
                set.Oread.Race, set.Sylph.Race, set.Undine.Race
            };
            for (int index = 0; index < races.Length; index++)
            {
                BlueprintRace race = races[index];
                UnitEntityData target = CreateUnit(targetFaction, created,
                    transient, new Vector3(40f, 0f, 2f + index),
                    "IdentityTarget" + index);
                string heritageGuid = ApplyRace(target.Descriptor, race,
                    outsider);
                if (!hostileCaster.IsEnemy(target) ||
                    !target.IsEnemy(hostileCaster) ||
                    friendlyCaster.IsEnemy(target) ||
                    target.IsEnemy(friendlyCaster))
                    throw new InvalidOperationException(
                        "Request-local faction relationships are ambiguous for " +
                        SafeName(race) + ".");

                UnitDescriptor owner = target.Descriptor;
                var wrapper = new TargetWrapper(target);
                results.Add(new IdentityEvidence
                {
                    Race = SafeName(race),
                    RaceGuid = race.AssetGuid,
                    RaceId = race.RaceId.ToString(),
                    OutsiderFact = owner.HasFact(outsider),
                    HeritageGuid = heritageGuid,
                    HoldPersonTargetable = new AbilityData(hold,
                        hostileCaster.Descriptor).CanTarget(wrapper),
                    CharmPersonTargetable = new AbilityData(charm,
                        hostileCaster.Descriptor).CanTarget(wrapper),
                    EnlargePersonTargetable = new AbilityData(enlarge,
                        friendlyCaster.Descriptor).CanTarget(wrapper),
                    ReducePersonTargetable = new AbilityData(reduce,
                        friendlyCaster.Descriptor).CanTarget(wrapper),
                    OutsiderPrerequisite = CheckFeature(owner, outsider,
                        false, transient),
                    NoOutsiderPrerequisite = CheckFeature(owner, outsider,
                        true, transient),
                    SelfRacePrerequisite = CheckFeature(owner, race, false,
                        transient),
                    AasimarRacePrerequisite = CheckFeature(owner, aasimar,
                        false, transient),
                    IfritRacePrerequisite = CheckFeature(owner,
                        set.Ifrit.Race, false, transient)
                });
            }
        }

        private static string ApplyRace(UnitDescriptor owner,
            BlueprintRace race, BlueprintFeature outsider)
        {
            EnsureFact(owner, race);
            foreach (BlueprintFeature feature in race.Features ??
                Array.Empty<BlueprintFeature>())
                EnsureFact(owner, feature);

            if (outsider == null || owner.HasFact(outsider))
                return string.Empty;
            if (!string.Equals(race.AssetGuid, AasimarRaceGuid,
                    StringComparison.Ordinal) &&
                !string.Equals(race.AssetGuid, TieflingRaceGuid,
                    StringComparison.Ordinal))
                return string.Empty;

            BlueprintFeatureSelection selection = (race.Features ??
                Array.Empty<BlueprintFeature>()).OfType<
                    BlueprintFeatureSelection>().SingleOrDefault();
            BlueprintFeature heritage = selection == null ? null :
                (selection.AllFeatures ?? Array.Empty<BlueprintFeature>())
                .FirstOrDefault();
            if (heritage == null)
                throw new InvalidOperationException(SafeName(race) +
                    " has no native heritage fixture.");
            EnsureFact(owner, heritage);
            return heritage.AssetGuid;
        }

        private static int SpeedWithArmor(UnitEntityData unit,
            BlueprintItemArmor blueprint)
        {
            if (unit.Body.Armor.HasArmor)
                throw new InvalidOperationException(
                    "Disposable movement unit unexpectedly has armor.");
            ItemEntityArmor item = null;
            try
            {
                item = new ItemEntityArmor(blueprint);
                unit.Body.Armor.InsertItem(item);
                if (!ReferenceEquals(unit.Body.Armor.Armor, item))
                    throw new InvalidOperationException(
                        "Native armor slot rejected " + Identity(blueprint) +
                        ".");
                return unit.Descriptor.Stats.Speed.ModifiedValue;
            }
            finally
            {
                if (unit.Body.Armor.HasArmor)
                    unit.Body.Armor.RemoveItem(false);
                if (item != null) item.Dispose();
            }
        }

        private static void SetStrengthForHeavyArmor(UnitDescriptor owner,
            float armorWeight)
        {
            int modifier = owner.Stats.Strength.ModifiedValue -
                owner.Stats.Strength.BaseValue;
            for (int strength = 1; strength <= 30; strength++)
            {
                owner.Stats.Strength.BaseValue = strength - modifier;
                var capacity = EncumbranceHelper.GetCarryingCapacity(owner);
                if (armorWeight > capacity.Medium &&
                    armorWeight <= capacity.Heavy)
                    return;
            }
            throw new InvalidOperationException(
                "The audited heavy armor cannot establish exact Heavy encumbrance.");
        }

        private static void ApplyCalculatedEncumbrance(UnitDescriptor owner)
        {
            Encumbrance encumbrance = EncumbranceHelper.GetEncumbrance(owner);
            owner.Encumbrance = encumbrance;
            owner.Ensure<UnitPartEncumbrance>().Init(encumbrance);
        }

        private static BlueprintFeature CreateSpeedFeature(string name,
            int value, ICollection<UnityEngine.Object> transient)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = name;
            feature.Ranks = 1;
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.Groups = Array.Empty<FeatureGroup>();
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.Stat = StatType.Speed;
            bonus.Value = value;
            bonus.Descriptor = ModifierDescriptor.UntypedStackable;
            feature.ComponentsArray = new BlueprintComponent[] { bonus };
            transient.Add(feature);
            transient.Add(bonus);
            return feature;
        }

        private static bool CheckFeature(UnitDescriptor owner,
            BlueprintFeature feature, bool inverse,
            ICollection<UnityEngine.Object> transient)
        {
            Prerequisite prerequisite;
            if (inverse)
            {
                var value = ScriptableObject.CreateInstance<
                    PrerequisiteNoFeature>();
                value.Feature = feature;
                prerequisite = value;
            }
            else
            {
                var value = ScriptableObject.CreateInstance<
                    PrerequisiteFeature>();
                value.Feature = feature;
                prerequisite = value;
            }
            transient.Add(prerequisite);
            return prerequisite.Check(null, owner, null);
        }

        private static void CreateFactionPair(
            ICollection<UnityEngine.Object> transient,
            out BlueprintFaction actor, out BlueprintFaction target)
        {
            BlueprintFaction source = BlueprintRoot.Instance
                .DefaultPlayerCharacter.Faction;
            if (source == null)
                throw new InvalidOperationException(
                    "The default character faction is unavailable.");
            actor = UnityEngine.Object.Instantiate(source);
            target = UnityEngine.Object.Instantiate(source);
            actor.name = "KMG_Runtime_ElementalIdentity_ActorFaction";
            target.name = "KMG_Runtime_ElementalIdentity_TargetFaction";
            ConfigureFaction(actor, target);
            ConfigureFaction(target, actor);
            transient.Add(actor);
            transient.Add(target);
        }

        private static void ConfigureFaction(BlueprintFaction faction,
            BlueprintFaction enemy)
        {
            faction.Peaceful = false;
            faction.AlwaysEnemy = false;
            faction.Neutral = false;
            faction.IsDirectlyControllable = false;
            faction.Dummy = null;
            faction.AttackFactions = new[] { enemy };
        }

        private static UnitEntityData CreateUnit(BlueprintFaction faction,
            ICollection<UnitEntityData> created,
            ICollection<UnityEngine.Object> transient, Vector3 position,
            string suffix)
        {
            BlueprintUnit donor = BlueprintRoot.Instance
                .DefaultPlayerCharacter;
            BlueprintUnit blueprint = UnityEngine.Object.Instantiate(donor);
            blueprint.name = "KMG_Runtime_ElementalIdentity_" + suffix +
                "_" + created.Count;
            if (faction != null) blueprint.Faction = faction;
            blueprint.Brain = null;
            blueprint.IsCheater = true;
            transient.Add(blueprint);
            UnitEntityData result = new Kingmaker.UI.LevelUp.ChargenUnit(
                blueprint).Unit;
            if (result == null || result.Descriptor == null ||
                result.Descriptor.Inventory == null)
                throw new InvalidOperationException(
                    "A disposable elemental identity unit was unavailable.");
            result.Descriptor.Stats.HitPoints.BaseValue = 100;
            result.Descriptor.State.Immortality.Retain();
            SetExactProperty(result, "Position", position);
            if (!Game.Instance.State.Units.All.Add(result))
            {
                result.Dispose();
                throw new InvalidOperationException(
                    "A disposable elemental identity unit could not be registered.");
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

        private static void AddAssertions(
            ICollection<RuntimeTestAssertion> assertions, Evidence evidence)
        {
            Add(assertions, "elemental-native-movement-fixtures", "3",
                evidence.Movement.Count.ToString(),
                evidence.Movement.Count == 3 &&
                !string.IsNullOrWhiteSpace(evidence.MediumArmor) &&
                !string.IsNullOrWhiteSpace(evidence.HeavyArmor),
                "three exact production/native races and native armor blueprints");
            Add(assertions, "elemental-slow-and-steady-native-donor",
                "exact Dwarf feature with native armor immunity and speed modifier",
                evidence.SlowAndSteadyGuid + ";" +
                    evidence.SlowAndSteadyComponents,
                string.Equals(evidence.SlowAndSteadyGuid,
                    ElementalRaceIdentityCatalog.SlowAndSteadyGuid,
                    StringComparison.Ordinal) &&
                (evidence.SlowAndSteadyComponents ?? string.Empty).Contains(
                    "AddMechanicsFeature") &&
                (evidence.SlowAndSteadyComponents ?? string.Empty).Contains(
                    "AddStatBonus"),
                "live exact-GUID native feature components");

            MovementEvidence oread = Movement(evidence, "Oread");
            MovementEvidence dwarf = MovementByGuid(evidence,
                DwarfRaceGuid);
            MovementEvidence human = MovementByGuid(evidence,
                HumanRaceGuid);
            Add(assertions, "elemental-oread-armor-speed",
                "20 feet in no, medium, and heavy armor",
                Summary(oread), oread != null &&
                    oread.UnarmoredSpeed == 20 &&
                    oread.MediumArmorSpeed == 20 &&
                    oread.HeavyArmorSpeed == 20 &&
                    oread.NativeHeavyPenalty == -10,
                "actual armor slot insertion and native speed stat");
            Add(assertions, "elemental-dwarf-armor-reference",
                "native Dwarf remains at 20 feet in armor",
                Summary(dwarf), dwarf != null &&
                    dwarf.UnarmoredSpeed == 20 &&
                    dwarf.MediumArmorSpeed == 20 &&
                    dwarf.HeavyArmorSpeed == 20 &&
                    dwarf.NativeHeavyPenalty == -10,
                "same native Slow and Steady feature on Dwarf control");
            Add(assertions, "elemental-human-armor-control",
                "native Human armor reduces 30-foot speed",
                Summary(human), human != null &&
                    human.UnarmoredSpeed == 30 &&
                    human.MediumArmorSpeed < human.UnarmoredSpeed &&
                    human.HeavyArmorSpeed < human.UnarmoredSpeed,
                "native Human negative control with the same armor fixtures");
            Add(assertions, "elemental-oread-encumbrance-speed",
                "actual Heavy equipped load leaves Oread at 20 feet",
                Summary(oread), EncumbranceExact(oread) &&
                    oread.HeavyEncumbranceSpeed == 20 &&
                    oread.RestoredSpeed == 20,
                "native equipped weight, EncumbranceHelper, and UnitPartEncumbrance.Init");
            Add(assertions, "elemental-dwarf-encumbrance-reference",
                "actual Heavy equipped load leaves Dwarf at 20 feet",
                Summary(dwarf), EncumbranceExact(dwarf) &&
                    dwarf.HeavyEncumbranceSpeed == 20 &&
                    dwarf.RestoredSpeed == 20,
                "same native Dwarf mechanism and calculated equipped load");
            Add(assertions, "elemental-human-encumbrance-control",
                "actual Heavy equipped load reduces Human speed and removal restores it",
                Summary(human), EncumbranceExact(human) &&
                    human.HeavyEncumbranceSpeed < human.UnarmoredSpeed &&
                    human.RestoredSpeed == human.UnarmoredSpeed,
                "native Human negative control and exact load removal");
            Add(assertions, "elemental-slow-and-steady-generic-modifiers",
                "+10 then -5 interact normally for all three races",
                string.Join("|", evidence.Movement.Select(Summary).ToArray()),
                evidence.Movement.Count == 3 && evidence.Movement.All(value =>
                    value.SpeedWithBonus == value.UnarmoredSpeed + 10 &&
                    value.SpeedWithBonusAndPenalty ==
                        value.UnarmoredSpeed + 5),
                "request-local UntypedStackable speed facts");

            Add(assertions, "elemental-native-identity-fixtures", "7",
                evidence.Identity.Count.ToString(), evidence.Identity.Count == 7,
                "Human, Aasimar, Tiefling, and four production elemental races");
            IdentityEvidence humanIdentity = IdentityByGuid(evidence,
                HumanRaceGuid);
            IdentityEvidence aasimarIdentity = IdentityByGuid(evidence,
                AasimarRaceGuid);
            IdentityEvidence tieflingIdentity = IdentityByGuid(evidence,
                TieflingRaceGuid);
            IdentityEvidence ifritIdentity = evidence.Identity
                .FirstOrDefault(value => string.Equals(value.Race, "Ifrit",
                    StringComparison.OrdinalIgnoreCase));
            IdentityEvidence[] elementals = evidence.Identity.Where(value =>
                !string.Equals(value.RaceGuid, HumanRaceGuid,
                    StringComparison.Ordinal) &&
                !string.Equals(value.RaceGuid, AasimarRaceGuid,
                    StringComparison.Ordinal) &&
                !string.Equals(value.RaceGuid, TieflingRaceGuid,
                    StringComparison.Ordinal)).ToArray();
            bool nativeDonorTypes = aasimarIdentity != null &&
                tieflingIdentity != null && !aasimarIdentity.OutsiderFact &&
                !tieflingIdentity.OutsiderFact &&
                !string.IsNullOrWhiteSpace(aasimarIdentity.HeritageGuid) &&
                !string.IsNullOrWhiteSpace(tieflingIdentity.HeritageGuid);
            Add(assertions, "elemental-native-outsider-precedent",
                "installed Aasimar and Tiefling heritages do not grant Outsider",
                Summary(aasimarIdentity) + "|" + Summary(tieflingIdentity),
                nativeDonorTypes,
                "first exact native heritage from each installed selection");
            Add(assertions, "elemental-outsider-person-spells",
                "all four elementals match targetable Aasimar/Tiefling and Human controls",
                string.Join("|", evidence.Identity.Select(value =>
                    value.Race + "=" + value.TargetVector()).ToArray()),
                humanIdentity != null && aasimarIdentity != null &&
                tieflingIdentity != null && elementals.Length == 4 &&
                humanIdentity.HoldPersonTargetable &&
                humanIdentity.CharmPersonTargetable &&
                humanIdentity.EnlargePersonTargetable &&
                humanIdentity.ReducePersonTargetable &&
                string.Equals(aasimarIdentity.TargetVector(),
                    tieflingIdentity.TargetVector(), StringComparison.Ordinal) &&
                elementals.All(value => !value.OutsiderFact && string.Equals(
                    value.TargetVector(), aasimarIdentity.TargetVector(),
                    StringComparison.Ordinal)),
                "native AbilityData.CanTarget with hostile/friendly faction controls");
            Add(assertions, "elemental-outsider-prerequisites",
                "Outsider and inverse prerequisites exactly mirror the live type fact",
                string.Join("|", evidence.Identity.Select(Summary).ToArray()),
                evidence.Identity.Count == 7 && evidence.Identity.All(value =>
                    value.OutsiderPrerequisite == value.OutsiderFact &&
                    value.NoOutsiderPrerequisite != value.OutsiderFact),
                "native PrerequisiteFeature and PrerequisiteNoFeature checks");
            Add(assertions, "elemental-exact-race-prerequisites",
                "self always passes; Aasimar and Ifrit pass only exact BlueprintRace identity",
                string.Join("|", evidence.Identity.Select(Summary).ToArray()),
                evidence.Identity.Count == 7 && evidence.Identity.All(value =>
                    value.SelfRacePrerequisite &&
                    value.AasimarRacePrerequisite == string.Equals(
                        value.RaceGuid, AasimarRaceGuid,
                        StringComparison.Ordinal) &&
                    value.IfritRacePrerequisite == string.Equals(
                        value.RaceGuid, ifritIdentity == null ? string.Empty :
                            ifritIdentity.RaceGuid, StringComparison.Ordinal)),
                "native exact BlueprintRace prerequisite components despite donor RaceId");
            Add(assertions, "elemental-native-identity-save-free",
                "no save API or player-party mutation", "saveStateTouched=" +
                    evidence.SaveStateTouched, !evidence.SaveStateTouched,
                "request-local disposable fixtures only");
            Add(assertions, "elemental-native-identity-cleanup",
                "exact pre-run global-unit reference sequence",
                "cleanupExact=" + evidence.CleanupExact,
                evidence.CleanupExact,
                "finally interruption, removal, disposal, and exact snapshot comparison");
        }

        private static bool EncumbranceExact(MovementEvidence value)
        {
            return value != null && string.Equals(value.EncumbranceBefore,
                    Encumbrance.Light.ToString(), StringComparison.Ordinal) &&
                string.Equals(value.EncumbranceDuring,
                    Encumbrance.Heavy.ToString(), StringComparison.Ordinal) &&
                string.Equals(value.EncumbranceAfter,
                    Encumbrance.Light.ToString(), StringComparison.Ordinal) &&
                value.WeightDuring > value.MediumCapacity &&
                value.WeightDuring <= value.HeavyCapacity &&
                value.LoadCount > 0;
        }

        private static MovementEvidence Movement(Evidence evidence,
            string name)
        {
            return evidence.Movement.FirstOrDefault(value => string.Equals(
                value.Race, name, StringComparison.OrdinalIgnoreCase));
        }

        private static MovementEvidence MovementByGuid(Evidence evidence,
            string guid)
        {
            return evidence.Movement.FirstOrDefault(value => string.Equals(
                value.RaceGuid, guid, StringComparison.Ordinal));
        }

        private static IdentityEvidence IdentityByGuid(Evidence evidence,
            string guid)
        {
            return evidence.Identity.FirstOrDefault(value => string.Equals(
                value.RaceGuid, guid, StringComparison.Ordinal));
        }

        private static string Summary(MovementEvidence value)
        {
            return value == null ? "<missing>" : value.Summary();
        }

        private static string Summary(IdentityEvidence value)
        {
            return value == null ? "<missing>" : value.Summary();
        }

        private static T Require<T>(LibraryScriptableObject library,
            string guid, string purpose) where T : BlueprintScriptableObject
        {
            return BlueprintLibraryLookup.RequireExact<T>(library, guid,
                "elemental native identity " + purpose);
        }

        private static string Identity(BlueprintScriptableObject value)
        {
            return value == null ? "<null>" : (value.name ?? string.Empty) +
                "[" + value.AssetGuid + "]";
        }

        private static string SafeName(BlueprintRace race)
        {
            try { return race.Name ?? race.name ?? string.Empty; }
            catch { return race.name ?? string.Empty; }
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
