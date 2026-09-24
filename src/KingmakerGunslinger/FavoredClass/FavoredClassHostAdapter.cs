using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using UnityModManagerNet;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>Live reflection handles for a ready host. Never persisted.</summary>
    internal sealed class FavoredClassHostHandles
    {
        internal FavoredClassHostHandles(FavoredClassHostDecision decision,
            FavoredClassHostDecision gunslingerDecision,
            FavoredClassHostObservation binary, FavoredClassHostReadinessObservation readiness,
            IDictionary bonusSelections, BlueprintFeatureSelection gunslingerSelection,
            Type prerequisiteRaceType, FieldInfo prerequisiteRaceField,
            BlueprintFeature genericHitPoint, BlueprintFeature genericSkillFull,
            BlueprintFeature genericSkillPartial)
        {
            Decision = decision;
            GunslingerDecision = gunslingerDecision;
            Binary = binary;
            Readiness = readiness;
            _bonusSelections = bonusSelections;
            GunslingerSelection = gunslingerSelection;
            PrerequisiteRaceType = prerequisiteRaceType;
            PrerequisiteRaceField = prerequisiteRaceField;
            GenericHitPoint = genericHitPoint;
            GenericSkillFull = genericSkillFull;
            GenericSkillPartial = genericSkillPartial;
        }

        private readonly IDictionary _bonusSelections;

        /// <summary>Host-wide decision: binary gate plus complete initialization.</summary>
        internal FavoredClassHostDecision Decision { get; private set; }

        /// <summary>Gunslinger favored-class entry decision (scan, identity, shape).</summary>
        internal FavoredClassHostDecision GunslingerDecision { get; private set; }
        internal FavoredClassHostObservation Binary { get; private set; }
        internal FavoredClassHostReadinessObservation Readiness { get; private set; }
        internal BlueprintFeatureSelection GunslingerSelection { get; private set; }
        internal Type PrerequisiteRaceType { get; private set; }
        internal FieldInfo PrerequisiteRaceField { get; private set; }
        internal BlueprintFeature GenericHitPoint { get; private set; }
        internal BlueprintFeature GenericSkillFull { get; private set; }
        internal BlueprintFeature GenericSkillPartial { get; private set; }

        /// <summary>Every scanned class GUID with its host bonus selection.</summary>
        internal IEnumerable<KeyValuePair<string, BlueprintFeatureSelection>> BonusSelections
        {
            get
            {
                if (_bonusSelections == null)
                    yield break;
                foreach (DictionaryEntry entry in _bonusSelections)
                    yield return new KeyValuePair<string, BlueprintFeatureSelection>(
                        entry.Key as string, entry.Value as BlueprintFeatureSelection);
            }
        }

        /// <summary>The host's per-class bonus selection, or null when not scanned.</summary>
        internal BlueprintFeatureSelection BonusSelectionFor(string classGuid)
        {
            if (_bonusSelections == null || string.IsNullOrEmpty(classGuid) ||
                !_bonusSelections.Contains(classGuid))
                return null;
            return _bonusSelections[classGuid] as BlueprintFeatureSelection;
        }
    }

    /// <summary>
    /// Reflection-only resolution of the optional Favored Class host. It
    /// reads UMM entries, assembly identities, member shapes, method bodies
    /// and static fields; it executes no host method. Host statics are read
    /// only after <c>Main.library</c> is observed non-null, because reading
    /// any <c>Core</c> static runs <c>Core</c>'s type initializer, which
    /// dereferences that library.
    /// </summary>
    internal static class FavoredClassHostAdapter
    {
        private const BindingFlags AnyStatic =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags AnyInstance =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static FavoredClassHostHandles Resolve(UnityModManager.ModEntry current,
            string gunslingerClassGuid)
        {
            var observed = new FavoredClassHostObservation();
            UnityModManager.ModEntry[] entries = ReadEntries(current);
            UnityModManager.ModEntry host = Single(entries, FavoredClassHostContract.HostModId);
            UnityModManager.ModEntry cotw = Single(entries, "CallOfTheWild");
            observed.HostModInstalled = host != null;
            if (host != null)
            {
                observed.HostModEnabled = host.Enabled;
                observed.HostModVersion = host.Info == null ? null : host.Info.Version;
                observed.HostAssemblyLoaded = host.Loaded && host.Active && host.HasAssembly &&
                    !host.ErrorOnLoading && host.Assembly != null;
            }
            if (cotw != null)
                observed.CallOfTheWildAssemblyLoaded = cotw.Loaded && cotw.Active &&
                    cotw.HasAssembly && !cotw.ErrorOnLoading && cotw.Assembly != null;

            Assembly hostAssembly = observed.HostAssemblyLoaded ? host.Assembly : null;
            Assembly cotwAssembly = observed.CallOfTheWildAssemblyLoaded ? cotw.Assembly : null;
            Type prerequisiteRace = null;
            FieldInfo raceField = null;
            Type core = null;
            Type main = null;
            if (hostAssembly != null)
            {
                observed.HostAssemblyName = hostAssembly.GetName().Name;
                observed.HostModuleVersionId = hostAssembly.ManifestModule.ModuleVersionId.ToString("D");
                observed.HostFileSha256 = FileSha256(hostAssembly.Location);
            }
            if (cotwAssembly != null)
            {
                observed.CallOfTheWildModuleVersionId =
                    cotwAssembly.ManifestModule.ModuleVersionId.ToString("D");
                observed.CallOfTheWildFileSha256 = FileSha256(cotwAssembly.Location);
            }
            if (hostAssembly != null && cotwAssembly != null)
            {
                core = hostAssembly.GetType(FavoredClassHostContract.CoreTypeName, false, false);
                main = hostAssembly.GetType(FavoredClassHostContract.MainTypeName, false, false);
                prerequisiteRace = hostAssembly.GetType(
                    FavoredClassHostContract.PrerequisiteRaceTypeName, false, false);
                Type fullRank = cotwAssembly.GetType(
                    FavoredClassHostContract.FullRankPrerequisiteTypeName, false, false);
                raceField = prerequisiteRace == null ? null :
                    prerequisiteRace.GetField("race", AnyInstance);
                observed.MissingMember = CheckMembers(core, main, prerequisiteRace, raceField,
                    fullRank, observed);
            }

            FavoredClassHostDecision decision = FavoredClassHostContract.EvaluateBinary(observed);
            if (!decision.IsReady)
                return new FavoredClassHostHandles(decision, decision, observed, null, null, null,
                    null, null, null, null, null);

            var readiness = new FavoredClassHostReadinessObservation
            {
                GunslingerClassGuid = gunslingerClassGuid
            };
            object library = main.GetField(FavoredClassHostContract.LibraryField, AnyStatic)
                .GetValue(null);
            readiness.LibraryAssigned = library != null;
            IDictionary bonusMap = null;
            BlueprintFeatureSelection gunslingerSelection = null;
            BlueprintFeature hitPoint = null;
            BlueprintFeature skillFull = null;
            BlueprintFeature skillPartial = null;
            if (readiness.LibraryAssigned)
            {
                readiness.CoreLoadCompleted = ReadStatic(core,
                    FavoredClassHostContract.PrestigiousSpellcasterField) != null;
                var favoredClassSelection = ReadStatic(core,
                    FavoredClassHostContract.FavoredClassSelectionField) as BlueprintFeatureSelection;
                readiness.FavoredClassSelectionPresent = favoredClassSelection != null;
                var progressionMap = ReadStatic(core,
                    FavoredClassHostContract.ProgressionMapField) as IDictionary;
                bonusMap = ReadStatic(core, FavoredClassHostContract.BonusSelectionMapField)
                    as IDictionary;
                BlueprintProgression progression = progressionMap != null &&
                    progressionMap.Contains(gunslingerClassGuid)
                    ? progressionMap[gunslingerClassGuid] as BlueprintProgression : null;
                gunslingerSelection = bonusMap != null && bonusMap.Contains(gunslingerClassGuid)
                    ? bonusMap[gunslingerClassGuid] as BlueprintFeatureSelection : null;
                readiness.GunslingerProgressionGuid = progression == null ? null : progression.AssetGuid;
                readiness.GunslingerBonusSelectionGuid =
                    gunslingerSelection == null ? null : gunslingerSelection.AssetGuid;
                readiness.GunslingerProgressionOffered = progression != null &&
                    favoredClassSelection != null &&
                    favoredClassSelection.AllFeatures.Contains(progression);
                if (progression != null && progression.LevelEntries != null)
                {
                    readiness.GunslingerProgressionLevels = progression.LevelEntries.Length;
                    readiness.GunslingerLevelsGrantBonusSelection = gunslingerSelection != null &&
                        progression.LevelEntries.All(entry => entry != null &&
                            entry.Features.Contains(gunslingerSelection));
                }
                hitPoint = ReadFavoredFeature(core, FavoredClassHostContract.FavoredHitPointsField,
                    "full");
                skillFull = ReadFavoredFeature(core, FavoredClassHostContract.FavoredSkillField,
                    "full");
                skillPartial = ReadFavoredFeature(core, FavoredClassHostContract.FavoredSkillField,
                    "partial");
                readiness.GenericHitPointLeafPresent = gunslingerSelection != null &&
                    hitPoint != null && gunslingerSelection.AllFeatures.Contains(hitPoint);
                readiness.GenericSkillLeavesPresent = gunslingerSelection != null &&
                    skillFull != null && skillPartial != null &&
                    gunslingerSelection.AllFeatures.Contains(skillFull) &&
                    gunslingerSelection.AllFeatures.Contains(skillPartial);
            }
            FavoredClassHostDecision hostDecision =
                FavoredClassHostContract.EvaluateHost(decision, readiness);
            FavoredClassHostDecision gunslingerDecision =
                FavoredClassHostContract.EvaluateGunslinger(hostDecision, readiness);
            return new FavoredClassHostHandles(hostDecision, gunslingerDecision, observed, readiness,
                hostDecision.IsReady ? bonusMap : null,
                gunslingerDecision.IsReady ? gunslingerSelection : null,
                prerequisiteRace, raceField, hitPoint, skillFull, skillPartial);
        }

        private static string CheckMembers(Type core, Type main, Type prerequisiteRace,
            FieldInfo raceField, Type fullRank, FavoredClassHostObservation observed)
        {
            if (core == null || main == null || prerequisiteRace == null || fullRank == null)
                return "required-types";
            if (raceField == null || raceField.FieldType != typeof(BlueprintRace))
                return "PrerequisiteRace.race";
            if (!typeof(Kingmaker.Blueprints.Classes.Prerequisites.Prerequisite)
                    .IsAssignableFrom(prerequisiteRace))
                return "PrerequisiteRace base type";
            FieldInfo library = main.GetField(FavoredClassHostContract.LibraryField, AnyStatic);
            if (library == null || library.FieldType != typeof(LibraryScriptableObject))
                return "Main.library";
            string missing =
                MissingStatic(core, FavoredClassHostContract.ProgressionMapField,
                    typeof(Dictionary<string, BlueprintProgression>)) ??
                MissingStatic(core, FavoredClassHostContract.BonusSelectionMapField,
                    typeof(Dictionary<string, BlueprintFeatureSelection>)) ??
                MissingStatic(core, FavoredClassHostContract.FavoredClassSelectionField,
                    typeof(BlueprintFeatureSelection)) ??
                MissingStatic(core, FavoredClassHostContract.PrestigiousSpellcasterField,
                    typeof(BlueprintFeatureSelection));
            if (missing != null)
                return missing;
            FieldInfo favoredHp = core.GetField(FavoredClassHostContract.FavoredHitPointsField, AnyStatic);
            FieldInfo favoredSkill = core.GetField(FavoredClassHostContract.FavoredSkillField, AnyStatic);
            if (favoredHp == null || favoredSkill == null || favoredHp.FieldType != favoredSkill.FieldType ||
                favoredHp.FieldType.GetField("full", AnyInstance) == null ||
                favoredHp.FieldType.GetField("partial", AnyInstance) == null)
                return "Core.FavoredClassFeature";

            MethodInfo check = prerequisiteRace.GetMethod("Check", AnyInstance, null,
                new[] { typeof(FeatureSelectionState), typeof(UnitDescriptor), typeof(LevelUpState) },
                null);
            MethodInfo fullRankCheck = fullRank.GetMethod("Check", AnyInstance, null,
                new[] { typeof(FeatureSelectionState), typeof(UnitDescriptor), typeof(LevelUpState) },
                null);
            MethodInfo load = core.GetMethod("load", AnyStatic, null, Type.EmptyTypes, null);
            MethodInfo add = core.GetMethod("addFavoredClassBonus", AnyStatic, null,
                new[] { typeof(BlueprintFeature), typeof(BlueprintFeature),
                    typeof(BlueprintCharacterClass[]), typeof(int), typeof(BlueprintRace[]) }, null);
            MethodInfo custom = core.GetMethod("loadCustomFeature", AnyStatic, null,
                new[] { typeof(string) }, null);
            Type patch = main.GetNestedType("LibraryScriptableObject_LoadDictionary_Patch", AnyStatic);
            MethodInfo postfix = patch == null ? null : patch.GetMethod("Postfix", AnyStatic);
            if (check == null || check.ReturnType != typeof(bool))
                return "PrerequisiteRace.Check";
            if (fullRankCheck == null)
                return "PrerequisiteFeatureFullRank.Check";
            if (load == null || add == null || custom == null || postfix == null)
                return "Core lifecycle methods";
            observed.MethodIlSha256[FavoredClassHostContract.PrerequisiteRaceCheckKey] = IlSha256(check);
            observed.MethodIlSha256[FavoredClassHostContract.FullRankCheckKey] = IlSha256(fullRankCheck);
            observed.MethodIlSha256[FavoredClassHostContract.CoreLoadKey] = IlSha256(load);
            observed.MethodIlSha256[FavoredClassHostContract.AddFavoredClassBonusKey] = IlSha256(add);
            observed.MethodIlSha256[FavoredClassHostContract.LoadCustomFeatureKey] = IlSha256(custom);
            observed.MethodIlSha256[FavoredClassHostContract.LoadDictionaryPostfixKey] = IlSha256(postfix);
            return null;
        }

        private static string MissingStatic(Type type, string name, Type expected)
        {
            FieldInfo field = type.GetField(name, AnyStatic);
            return field == null || field.FieldType != expected ? type.Name + "." + name : null;
        }

        private static object ReadStatic(Type type, string name)
        {
            return type.GetField(name, AnyStatic).GetValue(null);
        }

        private static BlueprintFeature ReadFavoredFeature(Type core, string field, string member)
        {
            object feature = ReadStatic(core, field);
            if (feature == null)
                return null;
            FieldInfo value = feature.GetType().GetField(member, AnyInstance);
            return value == null ? null : value.GetValue(feature) as BlueprintFeature;
        }

        private static UnityModManager.ModEntry Single(UnityModManager.ModEntry[] entries, string id)
        {
            UnityModManager.ModEntry[] matches = entries.Where(value => value.Info != null &&
                string.Equals(value.Info.Id, id, StringComparison.Ordinal)).ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }

        private static UnityModManager.ModEntry[] ReadEntries(UnityModManager.ModEntry current)
        {
            Type manager = current == null ? null : current.GetType().DeclaringType;
            FieldInfo field = manager == null ? null : manager.GetField("modEntries", AnyStatic);
            IEnumerable values = field == null ? null : field.GetValue(null) as IEnumerable;
            if (values == null)
                throw new InvalidOperationException("The live UMM modEntries collection was unavailable.");
            return values.Cast<object>().Select(value => value as UnityModManager.ModEntry)
                .Where(value => value != null).ToArray();
        }

        internal static string IlSha256(MethodInfo method)
        {
            MethodBody body = method == null ? null : method.GetMethodBody();
            byte[] il = body == null ? null : body.GetILAsByteArray();
            if (il == null)
                return null;
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(il)).Replace("-", string.Empty)
                    .ToLowerInvariant();
        }

        internal static string FileSha256(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return "unavailable";
                using (SHA256 sha = SHA256.Create())
                using (FileStream stream = File.OpenRead(path))
                    return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty)
                        .ToLowerInvariant();
            }
            catch (Exception)
            {
                return "unavailable";
            }
        }
    }
}
