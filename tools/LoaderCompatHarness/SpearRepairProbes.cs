using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Common;
using Kingmaker.UI.Selection;
using Kingmaker.UI.Tooltip;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Test-only conditions for the spear startup repair. Each stands in for something another mod could do before or
    /// while the product initializes; none is claimed to be what the reporter's installation did.
    /// </summary>
    internal static class SpearRepairConditions
    {
        /// <summary>"field=value;..." written onto the live native Longspear type before the product runs (simulated donor mutation).</summary>
        public const string MutateNativeLongspear = "mutateNativeLongspear";

        /// <summary>Weapon types (guid list) whose serialized profile is recorded before the product runs.</summary>
        public const string CaptureWeaponTypes = "captureWeaponTypes";

        /// <summary>"field=value;..." written onto the product's owned spear clone when the product validates it (invalid owned profile).</summary>
        public const string CorruptOwnedSpearProfile = "corruptOwnedSpearProfile";

        /// <summary>The product validation call (1-based, owned spear only) on which the corruption is written; empty: every call.</summary>
        public const string CorruptOwnedSpearProfileOnCall = "corruptOwnedSpearProfileOnCall";

        /// <summary>BlueprintWeaponType getters (property names) given an identity postfix owned by the harness.</summary>
        public const string PatchWeaponTypeGetters = "patchWeaponTypeGetters";

        /// <summary>A harness postfix on the shared AttackRange getter adds 1 ft for the owned spear only; its field and donor stay correct.</summary>
        public const string OwnedSpearAttackRangePatch = "ownedSpearAttackRangePatch";

        /// <summary>"donor:Getter;owned:Getter" - that getter throws for that type while the product's spear validation reads it.</summary>
        public const string ThrowingWeaponTypeGetters = "throwingWeaponTypeGetters";

        /// <summary>The product's named-spear registration throws after the owned spear type has been registered.</summary>
        public const string FailNamedSpearRegistration = "failNamedSpearRegistration";

        /// <summary>Records the native spear selectors, familiarity components and selector runtime before the product runs.</summary>
        public const string CaptureSpearSelectors = "captureSpearSelectors";

        /// <summary>"guid=+delta;guid=value" - AttackRange of the type with that asset guid gains delta ft or returns value ft (identity by guid, as a mod keyed on a blueprint guid would).</summary>
        public const string AttackRangeByGuid = "attackRangeByGuid";

        /// <summary>"guid:Getter;..." - that getter throws whenever it is read for the type with that asset guid, anywhere in the game.</summary>
        public const string ThrowingGettersByGuid = "throwingGettersByGuid";

        /// <summary>Sub-categories (list) a late WeaponCategoryExtension.HasSubCategory postfix withholds from the Elven Branched Spear category only.</summary>
        public const string SpearSubCategoryRemoval = "spearSubCategoryRemoval";

        public const string NativeLongspearTypeGuid = "fa2dd17cbde7d3f4aa918d467c30516e";
        public const string HarnessId = "KmgLoaderCompatHarness";
        public const string InjectedNamedFailure = "KmgLoaderCompatHarness injected named-spear registration failure";
        public const string InjectedGetterFailure = "KmgLoaderCompatHarness injected getter failure";

        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static Dictionary<string, string> longspearMutations = new Dictionary<string, string>();
        private static Dictionary<string, string> ownedCorruptions = new Dictionary<string, string>();
        private static int corruptOnCall;
        private static string[] captureGuids = new string[0];
        private static readonly Dictionary<string, string> throwingGetters = new Dictionary<string, string>();
        private static bool captureSelectors;
        private static BlueprintWeaponType donorReference;

        public static readonly string[] Known =
        {
            MutateNativeLongspear, CaptureWeaponTypes, CorruptOwnedSpearProfile, CorruptOwnedSpearProfileOnCall, PatchWeaponTypeGetters,
            OwnedSpearAttackRangePatch, ThrowingWeaponTypeGetters, FailNamedSpearRegistration, CaptureSpearSelectors,
            AttackRangeByGuid, ThrowingGettersByGuid, SpearSubCategoryRemoval
        };

        private static readonly Dictionary<string, string> rangeByGuid = new Dictionary<string, string>();
        private static readonly List<KeyValuePair<string, string>> throwingByGuid = new List<KeyValuePair<string, string>>();
        private static WeaponSubCategory[] removedSubCategories = new WeaponSubCategory[0];
        public static int RangeOverrides { get; private set; }
        public static int GuidGetterThrows { get; private set; }
        public static int SubCategoryRemovals { get; private set; }

        public static readonly List<string> Events = new List<string>();
        public static readonly Dictionary<string, JObject> Captured = new Dictionary<string, JObject>();
        public static HarmonyInstance Harmony { get; private set; }
        public static int OwnedValidations { get; private set; }
        public static int CorruptionsApplied { get; private set; }
        public static int GetterThrows { get; private set; }

        public static bool NeedsLibraryHook => longspearMutations.Count > 0 || captureGuids.Length > 0 || captureSelectors || throwingGetters.Count > 0;

        public static void Apply(HarmonyInstance harmony, IDictionary<string, string> conditions, JObject applied)
        {
            Harmony = harmony;
            longspearMutations = Pairs(conditions, MutateNativeLongspear, '=');
            ownedCorruptions = Pairs(conditions, CorruptOwnedSpearProfile, '=');
            corruptOnCall = conditions.TryGetValue(CorruptOwnedSpearProfileOnCall, out string onCall) && !string.IsNullOrWhiteSpace(onCall)
                ? int.Parse(onCall, CultureInfo.InvariantCulture) : 0;
            captureGuids = (conditions.TryGetValue(CaptureWeaponTypes, out string captured) && captured != null
                ? captured.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()) : Enumerable.Empty<string>())
                .Concat(longspearMutations.Count > 0 ? new[] { NativeLongspearTypeGuid } : new string[0]).Distinct().ToArray();
            captureSelectors = Enabled(conditions, CaptureSpearSelectors);
            if (longspearMutations.Count > 0) applied[MutateNativeLongspear] = ToJson(longspearMutations);
            if (captureGuids.Length > 0) applied[CaptureWeaponTypes] = new JArray(captureGuids.Cast<object>().ToArray());
            if (captureSelectors) applied[CaptureSpearSelectors] = true;
            Assembly product = ProductProbes.Entry().Assembly;

            if (ownedCorruptions.Count > 0)
            {
                Type access = product.GetType("KingmakerGunslinger.Blueprints.SpearWeaponTypeAccess", true);
                MethodInfo[] validators = access.GetMethods(Any).Where(m => m.Name == "Validate" && m.GetParameters().Length >= 1 &&
                    m.GetParameters()[0].ParameterType == typeof(BlueprintWeaponType)).ToArray();
                if (validators.Length == 0) throw new MissingMethodException(access.FullName, "Validate(BlueprintWeaponType,...)");
                foreach (MethodInfo validator in validators)
                    harmony.Patch(validator, new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(CorruptPrefix), Any)), null);
                applied[CorruptOwnedSpearProfile] = ToJson(ownedCorruptions);
                applied[CorruptOwnedSpearProfileOnCall] = corruptOnCall == 0 ? "every call" : corruptOnCall.ToString(CultureInfo.InvariantCulture);
            }

            if (conditions.TryGetValue(PatchWeaponTypeGetters, out string getters) && !string.IsNullOrWhiteSpace(getters))
            {
                var names = getters.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToArray();
                foreach (string name in names)
                    harmony.Patch(Getter(name), null, new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(IdentityPostfix), Any)));
                applied[PatchWeaponTypeGetters] = new JArray(names.Cast<object>().ToArray());
            }

            if (Enabled(conditions, OwnedSpearAttackRangePatch))
            {
                harmony.Patch(Getter("AttackRange"), null, new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(OwnedSpearAttackRangePostfix), Any)));
                applied[OwnedSpearAttackRangePatch] = "postfix on BlueprintWeaponType.get_AttackRange adds 1 ft for the owned spear only";
            }

            throwingGetters.Clear();
            foreach (KeyValuePair<string, string> pair in Pairs(conditions, ThrowingWeaponTypeGetters, ':'))
            {
                if (pair.Key != "donor" && pair.Key != "owned") throw new InvalidOperationException(ThrowingWeaponTypeGetters + " targets donor or owned, not " + pair.Key);
                throwingGetters[pair.Key] = pair.Value;
                harmony.Patch(Getter(pair.Value), new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(ThrowingPrefix), Any)), null);
            }

            if (throwingGetters.Count > 0) applied[ThrowingWeaponTypeGetters] = ToJson(throwingGetters);

            rangeByGuid.Clear();
            foreach (KeyValuePair<string, string> pair in Pairs(conditions, AttackRangeByGuid, '=')) rangeByGuid[pair.Key] = pair.Value;
            if (rangeByGuid.Count > 0)
            {
                harmony.Patch(Getter("AttackRange"), null, new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(AttackRangeByGuidPostfix), Any)));
                applied[AttackRangeByGuid] = ToJson(rangeByGuid);
            }

            throwingByGuid.Clear();
            foreach (KeyValuePair<string, string> pair in Pairs(conditions, ThrowingGettersByGuid, ':'))
            {
                throwingByGuid.Add(pair);
                harmony.Patch(Getter(pair.Value), new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(ThrowingByGuidPrefix), Any)), null);
            }

            if (throwingByGuid.Count > 0)
                applied[ThrowingGettersByGuid] = new JArray(throwingByGuid.Select(p => (object)(p.Key + ":" + p.Value)).ToArray());

            if (conditions.TryGetValue(SpearSubCategoryRemoval, out string removal) && !string.IsNullOrWhiteSpace(removal))
            {
                removedSubCategories = removal.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => (WeaponSubCategory)Enum.Parse(typeof(WeaponSubCategory), v.Trim())).ToArray();
                MethodInfo has = typeof(Kingmaker.Enums.WeaponCategoryExtension).GetMethod("HasSubCategory", BindingFlags.Static | BindingFlags.Public, null,
                    new[] { typeof(Kingmaker.Enums.WeaponCategory), typeof(WeaponSubCategory) }, null) ??
                    throw new MissingMethodException("WeaponCategoryExtension.HasSubCategory");
                harmony.Patch(has, null, new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(SubCategoryRemovalPostfix), Any)) { prioritiy = Priority.Last });
                applied[SpearSubCategoryRemoval] = new JArray(removedSubCategories.Select(v => (object)v.ToString()).ToArray());
            }

            if (Enabled(conditions, FailNamedSpearRegistration))
            {
                Type named = product.GetType("KingmakerGunslinger.Blueprints.ElvenBranchedSpearNamedBlueprints", true);
                MethodInfo register = named.GetMethod("Register", Any) ?? throw new MissingMethodException(named.FullName, "Register");
                harmony.Patch(register, new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(FailNamedPrefix), Any)), null);
                applied[FailNamedSpearRegistration] = "prefix on ElvenBranchedSpearNamedBlueprints.Register throws";
            }
        }

        /// <summary>Runs from the harness LoadDictionary postfix, which is ordered before the product's own.</summary>
        public static void BeforeProduct(LibraryScriptableObject library)
        {
            donorReference = Type(library, NativeLongspearTypeGuid);
            if (longspearMutations.Count > 0)
            {
                BlueprintWeaponType longspear = Type(library, NativeLongspearTypeGuid);
                if (longspear == null) Events.Add("native Longspear type missing before product");
                else
                {
                    foreach (KeyValuePair<string, string> pair in longspearMutations) WeaponTypeProfiles.Write(longspear, pair.Key, pair.Value);
                    Events.Add("simulated donor mutation on native Longspear: " + string.Join(";", longspearMutations.Select(p => p.Key + "=" + p.Value).ToArray()));
                }
            }

            foreach (string guid in captureGuids)
            {
                BlueprintWeaponType type = Type(library, guid);
                if (type == null) { Events.Add("weapon type missing before product: " + guid); continue; }
                Captured[guid] = WeaponTypeProfiles.Fields(type);
            }

            if (captureSelectors) SpearSelectorState.Capture(library);
        }

        private static MethodInfo Getter(string name)
        {
            return typeof(BlueprintWeaponType).GetProperty(name, BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod() ??
                throw new MissingMethodException(typeof(BlueprintWeaponType).FullName, "get_" + name);
        }

        private static BlueprintWeaponType Type(LibraryScriptableObject library, string guid)
        {
            return library != null && library.BlueprintsByAssetId != null && library.BlueprintsByAssetId.TryGetValue(guid, out BlueprintScriptableObject value)
                ? value as BlueprintWeaponType : null;
        }

        private static void CorruptPrefix(BlueprintWeaponType __0)
        {
            if (__0 == null || __0.name != WeaponTypeProfiles.OwnedSpearTypeName) return;
            OwnedValidations++;
            if (corruptOnCall != 0 && OwnedValidations != corruptOnCall) return;
            foreach (KeyValuePair<string, string> pair in ownedCorruptions) WeaponTypeProfiles.Write(__0, pair.Key, pair.Value);
            CorruptionsApplied++;
            Events.Add("owned spear corrupted on validation call " + OwnedValidations);
        }

        private static void IdentityPostfix()
        {
        }

        private static void OwnedSpearAttackRangePostfix(BlueprintWeaponType __instance, ref Feet __result)
        {
            if (__instance == null || __instance.name != WeaponTypeProfiles.OwnedSpearTypeName) return;
            __result = new Feet(__result.Value + 1);
        }

        // Throws only inside the product's spear validation, so the rest of the game keeps reading these getters.
        private static void ThrowingPrefix(BlueprintWeaponType __instance, MethodBase __originalMethod)
        {
            if (__instance == null || !InsideSpearValidation()) return;
            // By reference: while the product configures its clone, the clone still carries the donor's asset guid.
            string target = ReferenceEquals(__instance, donorReference) ? "donor" :
                __instance.name == WeaponTypeProfiles.OwnedSpearTypeName ? "owned" : null;
            if (target == null || !throwingGetters.TryGetValue(target, out string getter) || "get_" + getter != __originalMethod.Name) return;
            GetterThrows++;
            throw new InvalidOperationException(InjectedGetterFailure + " (" + target + " " + getter + ")");
        }

        private static bool InsideSpearRegistration()
        {
            return new System.Diagnostics.StackTrace().GetFrames().Any(frame =>
            {
                string type = frame.GetMethod()?.DeclaringType?.FullName ?? string.Empty;
                return type.StartsWith("KingmakerGunslinger.Blueprints.ElvenBranchedSpearBlueprints", StringComparison.Ordinal) ||
                    type.StartsWith("KingmakerGunslinger.Blueprints.SpearWeaponTypeAccess", StringComparison.Ordinal);
            });
        }

        private static bool InsideSpearValidation()
        {
            return new System.Diagnostics.StackTrace().GetFrames().Any(frame =>
                (frame.GetMethod()?.DeclaringType?.FullName ?? string.Empty).StartsWith("KingmakerGunslinger.Blueprints.SpearWeaponTypeAccess", StringComparison.Ordinal));
        }

        private static void AttackRangeByGuidPostfix(BlueprintWeaponType __instance, ref Feet __result)
        {
            if (__instance == null || !rangeByGuid.TryGetValue(__instance.AssetGuid ?? string.Empty, out string spec)) return;
            float current = (float)typeof(Feet).GetField("m_Value", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__result);
            float value = spec.StartsWith("+", StringComparison.Ordinal)
                ? current + float.Parse(spec.Substring(1), CultureInfo.InvariantCulture)
                : float.Parse(spec, CultureInfo.InvariantCulture);
            __result = new Feet(value);
            RangeOverrides++;
        }

        // Keyed on the asset guid, active for every read made anywhere inside the product's spear registration
        // (ElvenBranchedSpearBlueprints.Register and the spear type adapter), so the rest of the game is unaffected.
        private static void ThrowingByGuidPrefix(BlueprintWeaponType __instance, MethodBase __originalMethod)
        {
            if (__instance == null || !InsideSpearRegistration()) return;
            foreach (KeyValuePair<string, string> pair in throwingByGuid)
            {
                if (__instance.AssetGuid != pair.Key || "get_" + pair.Value != __originalMethod.Name) continue;
                GuidGetterThrows++;
                throw new InvalidOperationException(InjectedGetterFailure + " (guid " + pair.Key + " " + pair.Value + ")");
            }
        }

        private static void SubCategoryRemovalPostfix(Kingmaker.Enums.WeaponCategory __0, WeaponSubCategory __1, ref bool __result)
        {
            if ((int)__0 != WeaponTypeProfiles.OwnedSpearCategory || !__result || Array.IndexOf(removedSubCategories, __1) < 0) return;
            __result = false;
            SubCategoryRemovals++;
        }

        private static void FailNamedPrefix()
        {
            Events.Add("named-spear registration failure injected");
            throw new InvalidOperationException(InjectedNamedFailure);
        }

        private static bool Enabled(IDictionary<string, string> conditions, string name)
        {
            return conditions.TryGetValue(name, out string value) && !string.IsNullOrWhiteSpace(value) &&
                !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string> Pairs(IDictionary<string, string> conditions, string name, char separator)
        {
            var result = new Dictionary<string, string>();
            if (!conditions.TryGetValue(name, out string value) || string.IsNullOrWhiteSpace(value)) return result;
            foreach (string pair in value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = pair.Split(new[] { separator }, 2);
                if (parts.Length != 2) throw new InvalidOperationException(name + " needs key" + separator + "value pairs: " + value);
                result[parts[0].Trim()] = parts[1].Trim();
            }

            return result;
        }

        public static JObject Describe()
        {
            var captured = new JObject();
            foreach (KeyValuePair<string, JObject> pair in Captured) captured[pair.Key] = pair.Value;
            return new JObject
            {
                ["events"] = new JArray(Events.Cast<object>().ToArray()), ["ownedValidations"] = OwnedValidations,
                ["corruptionsApplied"] = CorruptionsApplied, ["getterThrows"] = GetterThrows, ["guidGetterThrows"] = GuidGetterThrows,
                ["rangeOverrides"] = RangeOverrides, ["subCategoryRemovals"] = SubCategoryRemovals, ["captured"] = captured
            };
        }

        // Built by hand: another mod may change Newtonsoft's process-wide defaults (dictionaries as arrays).
        public static JObject ToJson(Dictionary<string, string> values)
        {
            var result = new JObject();
            foreach (KeyValuePair<string, string> pair in values) result[pair.Key] = pair.Value;
            return result;
        }
    }

    /// <summary>Reads and (for conditions only) writes the serialized profile of Kingmaker weapon types.</summary>
    internal static class WeaponTypeProfiles
    {
        public const string OwnedSpearTypeName = "KMG_ElvenBranchedSpear_WeaponType";
        public const int OwnedSpearCategory = 0x004b4d47;
        private const BindingFlags Fields_ = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly FieldInfo FeetValue = typeof(Feet).GetField("m_Value", Fields_);

        private static readonly string[] Getters_ =
        {
            "AttackType", "AttackRange", "BaseDamage", "DamageType", "CriticalRollEdge", "CriticalModifier", "FighterGroup", "IsTwoHanded",
            "IsLight", "IsMonk", "IsNatural", "IsUnarmed", "AttackBonusStat", "Weight"
        };

        /// <summary>The owned spear's intended serialized profile.</summary>
        public static readonly Dictionary<string, string> Intended = new Dictionary<string, string>
        {
            ["Category"] = OwnedSpearCategory.ToString(CultureInfo.InvariantCulture), ["m_AttackType"] = "Melee", ["m_AttackRange"] = "10",
            ["m_BaseDamage"] = "1xD8", ["m_DamageType"] = "Physical/Piercing/0", ["m_CriticalRollEdge"] = "20", ["m_CriticalModifier"] = "X3",
            ["m_FighterGroup"] = "Spears", ["m_IsTwoHanded"] = "True", ["m_IsLight"] = "False", ["m_IsMonk"] = "False", ["m_IsNatural"] = "False",
            ["m_IsUnarmed"] = "False", ["m_OverrideAttackBonusStat"] = "False", ["m_Weight"] = "10"
        };

        /// <summary>The owned spear's intended effective (getter) profile on an unpatched 2.1.7b engine.</summary>
        public static readonly Dictionary<string, string> IntendedGetters = new Dictionary<string, string>
        {
            ["AttackType"] = "Melee", ["AttackRange"] = "6", ["BaseDamage"] = "1xD8", ["DamageType"] = "Physical/Piercing/0", ["CriticalRollEdge"] = "20",
            ["CriticalModifier"] = "X3", ["FighterGroup"] = "Spears", ["IsTwoHanded"] = "True", ["IsLight"] = "False", ["IsMonk"] = "False",
            ["IsNatural"] = "False", ["IsUnarmed"] = "False", ["AttackBonusStat"] = "Strength", ["Weight"] = "10"
        };

        public static JObject Fields(BlueprintWeaponType type)
        {
            var result = new JObject { ["Category"] = ((int)type.Category).ToString(CultureInfo.InvariantCulture) };
            foreach (string name in Intended.Keys.Where(k => k != "Category")) result[name] = FieldText(Field(name).GetValue(type));
            result["m_DamageTypeIdentity"] = Identity(Field("m_DamageType").GetValue(type));
            result["m_EnchantmentsIdentity"] = Identity(Field("m_Enchantments").GetValue(type));
            return result;
        }

        public static JObject Getters(BlueprintWeaponType type)
        {
            var result = new JObject();
            foreach (string name in Getters_)
            {
                try
                {
                    object value = typeof(BlueprintWeaponType).GetProperty(name).GetValue(type, null);
                    // Full precision: Feet.Value truncates (a 6.25 ft effective reach would read 6), so the observer
                    // reads the private float through the same reader the serialized fields use.
                    result[name] = FieldText(value);
                }
                catch (Exception exception) { result[name] = "<throws " + (exception.InnerException ?? exception).GetType().Name + ">"; }
            }

            return result;
        }

        /// <summary>Harmony owners of every getter the spear validation reads, as the installed Harmony12 facade reports them.</summary>
        public static JObject GetterPatches()
        {
            var result = new JObject();
            HarmonyInstance harmony = SpearRepairConditions.Harmony ?? HarmonyInstance.Create(SpearRepairConditions.HarnessId + ".reader");
            foreach (string name in Getters_)
            {
                MethodInfo getter = typeof(BlueprintWeaponType).GetProperty(name).GetGetMethod();
                try
                {
                    Patches patches = harmony.GetPatchInfo(getter);
                    result[name] = patches == null ? "none" : string.Join(",", patches.Prefixes.Concat(patches.Postfixes).Concat(patches.Transpilers)
                        .Select(p => p.owner).Distinct().OrderBy(o => o, StringComparer.Ordinal).ToArray());
                }
                catch (Exception exception) { result[name] = "unavailable:" + exception.GetType().Name; }
            }

            return result;
        }

        /// <summary>
        /// The predicates of the v0.0.188-v0.0.192 spear validation that a clone of this type would fail. Those
        /// releases changed only category, text, weight and enchantments on the clone, so every other predicate was
        /// the donor's getter value.
        /// </summary>
        public static string[] OldClonePredicateFailures(BlueprintWeaponType donor)
        {
            var failures = new List<string>();
            if (donor.AttackType != Kingmaker.RuleSystem.AttackType.Melee) failures.Add("attack-type");
            if (!donor.BaseDamage.Equals(new Kingmaker.RuleSystem.DiceFormula(1, Kingmaker.RuleSystem.DiceType.D8))) failures.Add("base-damage");
            if (donor.DamageType == null || donor.DamageType.Type != Kingmaker.RuleSystem.Rules.Damage.DamageType.Physical ||
                donor.DamageType.Physical.Form != Kingmaker.Enums.Damage.PhysicalDamageForm.Piercing) failures.Add("damage-type");
            if (donor.CriticalRollEdge != 20) failures.Add("critical-edge");
            if (donor.CriticalModifier != Kingmaker.Enums.Damage.DamageCriticalModifierType.X3) failures.Add("critical-modifier");
            if (donor.FighterGroup != WeaponFighterGroup.Spears) failures.Add("fighter-group");
            if (!donor.IsTwoHanded) failures.Add("two-handed");
            if (donor.IsLight) failures.Add("light");
            if (donor.IsNatural) failures.Add("natural");
            if (donor.AttackRange.Value != 6) failures.Add("attack-range");
            return failures.ToArray();
        }

        public static BlueprintWeaponType OwnedSpear()
        {
            return ResourcesLibrary.GetBlueprints<BlueprintWeaponType>().FirstOrDefault(t => t != null && t.name == OwnedSpearTypeName);
        }

        public static void Write(BlueprintWeaponType type, string field, string value)
        {
            FieldInfo info = Field(field);
            object parsed;
            if (info.FieldType == typeof(Feet)) parsed = new Feet(float.Parse(value, CultureInfo.InvariantCulture));
            else if (info.FieldType.IsEnum) parsed = Enum.Parse(info.FieldType, value);
            else if (info.FieldType == typeof(bool)) parsed = bool.Parse(value);
            else if (info.FieldType == typeof(int)) parsed = int.Parse(value, CultureInfo.InvariantCulture);
            else if (info.FieldType == typeof(float)) parsed = float.Parse(value, CultureInfo.InvariantCulture);
            else throw new NotSupportedException("condition cannot write " + field + " of type " + info.FieldType.Name);
            info.SetValue(type, parsed);
        }

        private static FieldInfo Field(string name)
        {
            return typeof(BlueprintWeaponType).GetField(name, Fields_) ?? throw new MissingFieldException(typeof(BlueprintWeaponType).FullName, name);
        }

        private static string FieldText(object value)
        {
            switch (value)
            {
                case null: return "<null>";
                case Feet feet: return ((float)FeetValue.GetValue(feet)).ToString("R", CultureInfo.InvariantCulture);
                case Kingmaker.RuleSystem.DiceFormula dice: return dice.Rolls.ToString(CultureInfo.InvariantCulture) + "x" + dice.Dice;
                case Kingmaker.RuleSystem.Rules.Damage.DamageTypeDescription damage:
                    return damage.Type + "/" + (damage.Physical == null ? "<null>" : damage.Physical.Form + "/" + (int)damage.Physical.Material);
                case float number: return number.ToString("R", CultureInfo.InvariantCulture);
                default: return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        private static string Identity(object value)
        {
            return value == null ? "<null>" : value.GetType().Name + "#" +
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value).ToString("X8", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// kmgWeaponTypeProfile. match: "owned" (the owned spear exists with exactly the intended serialized and effective
        /// profile), "absent" (no owned spear: a rolled-back startup) or empty (record only). type: weapon types that must
        /// still hold the profile captured before the product ran. expect: the old-release predicate failures the native
        /// Longspear must show ("none" or a comma list); empty records only.
        /// </summary>
        public static JObject Probe(string match, string[] unchanged, string expect, out string problem)
        {
            var problems = new List<string>();
            var result = new JObject();
            BlueprintWeaponType longspear = ResourcesLibrary.TryGetBlueprint<BlueprintWeaponType>(SpearRepairConditions.NativeLongspearTypeGuid);
            if (longspear == null) problems.Add("native Longspear type does not resolve");
            else
            {
                string[] old = OldClonePredicateFailures(longspear);
                result["nativeLongspear"] = new JObject
                {
                    ["fields"] = Fields(longspear), ["getters"] = Getters(longspear),
                    ["oldReleaseClonePredicateFailures"] = new JArray(old.Cast<object>().ToArray())
                };
                string observed = old.Length == 0 ? "none" : string.Join(",", old);
                if (!string.IsNullOrEmpty(expect) && observed != expect)
                    problems.Add("native Longspear old-release predicate failures are " + observed + ", expected " + expect);
            }

            result["getterPatchOwners"] = GetterPatches();
            BlueprintWeaponType owned = OwnedSpear();
            result["ownedSpearPresent"] = owned != null;
            if (owned != null)
            {
                JObject fields = Fields(owned);
                JObject getters = Getters(owned);
                result["ownedSpear"] = new JObject { ["fields"] = fields, ["getters"] = getters, ["guid"] = owned.AssetGuid };
                if (match == "owned")
                {
                    foreach (KeyValuePair<string, string> pair in Intended)
                        if ((string)fields[pair.Key] != pair.Value) problems.Add("owned spear " + pair.Key + " is " + fields[pair.Key] + ", expected " + pair.Value);
                    foreach (KeyValuePair<string, string> pair in IntendedGetters)
                        if ((string)getters[pair.Key] != pair.Value) problems.Add("owned spear getter " + pair.Key + " is " + getters[pair.Key] + ", expected " + pair.Value);
                }
            }

            if (match == "owned" && owned == null) problems.Add("the owned spear weapon type is not registered");
            if (match == "absent" && owned != null) problems.Add("an owned spear weapon type is registered after a failed startup");

            var compared = new JObject();
            foreach (string guid in unchanged)
            {
                BlueprintWeaponType type = ResourcesLibrary.TryGetBlueprint<BlueprintWeaponType>(guid);
                if (!SpearRepairConditions.Captured.TryGetValue(guid, out JObject before)) { problems.Add("no pre-product capture for " + guid); continue; }
                if (type == null) { problems.Add("weapon type " + guid + " does not resolve"); continue; }
                JObject now = Fields(type);
                string[] changed = before.Properties().Where(p => (string)now[p.Name] != (string)p.Value).Select(p => p.Name + ":" + p.Value + "->" + now[p.Name]).ToArray();
                compared[guid] = new JObject { ["name"] = type.name, ["changed"] = new JArray(changed.Cast<object>().ToArray()) };
                if (changed.Length > 0) problems.Add(type.name + " changed after the product ran: " + string.Join(", ", changed));
            }

            result["unchangedSincePreProduct"] = compared;
            result["conditions"] = SpearRepairConditions.Describe();
            problem = problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
            return result;
        }
    }

    /// <summary>
    /// The native state the spear publication changes: the Exotic Weapon Proficiency and Rogue Finesse Training
    /// selections (Features and AllFeatures), Elven Weapon Familiarity's components, and the product's custom-weapon
    /// selector runtime. Captured before the product runs (captureSpearSelectors) and compared by kmgSpearSelectorState.
    /// </summary>
    internal static class SpearSelectorState
    {
        private const string EwpSelectionGuid = "9a01b6815d6c3684cb25f30b8bf20932";
        private const string FinesseSelectionGuid = "b78d146cea711a84598f0acef69462ea";
        private const string FamiliarityGuid = "03fd1e043fc678a4baf73fe67c3780ce";
        private const string SelectorSourceKey = "elven-branched-spear";
        private static readonly Dictionary<string, object> Before = new Dictionary<string, object>();
        private static readonly Dictionary<string, string> BeforeText = new Dictionary<string, string>();
        private static bool captured;

        public static void Capture(LibraryScriptableObject library)
        {
            foreach (KeyValuePair<string, Func<object>> part in Parts(guid =>
                library.BlueprintsByAssetId.TryGetValue(guid, out BlueprintScriptableObject value) ? value : null))
            {
                object value = part.Value();
                Before[part.Key] = value;
                BeforeText[part.Key] = Text(value);
            }

            BeforeText["selectorRuntime"] = RuntimeHasSpear() ? "configured" : "absent";
            captured = true;
        }

        /// <summary>expect "restored": every part is the same object with the same contents and the runtime has no spear source; "published": the spear options are present.</summary>
        public static JObject Probe(string expect, out string problem)
        {
            var problems = new List<string>();
            var result = new JObject { ["captured"] = captured };
            if (!captured) problems.Add("the spear selectors were not captured before the product ran (captureSpearSelectors)");
            foreach (KeyValuePair<string, Func<object>> part in Parts(guid => ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(guid)))
            {
                object now = part.Value();
                string text = Text(now);
                bool same = Before.TryGetValue(part.Key, out object before) && ReferenceEquals(before, now);
                bool sameText = BeforeText.TryGetValue(part.Key, out string beforeText) && beforeText == text;
                result[part.Key] = new JObject { ["sameObject"] = same, ["sameContents"] = sameText, ["now"] = text, ["before"] = beforeText };
                if (expect == "restored" && (!same || !sameText)) problems.Add(part.Key + " differs from its pre-product state: " + beforeText + " -> " + text);
                if (expect == "published" && part.Key == "ewp.AllFeatures" && text.IndexOf("KMG_ExoticWeaponProficiency_ElvenBranchedSpear", StringComparison.Ordinal) < 0)
                    problems.Add("the spear proficiency is not published in the Exotic Weapon Proficiency selection");
            }

            string runtime = RuntimeHasSpear() ? "configured" : "absent";
            result["selectorRuntime"] = runtime;
            if (expect == "restored" && runtime != "absent") problems.Add("the custom-weapon selector runtime still holds the spear source");
            if (expect == "published" && runtime != "configured") problems.Add("the custom-weapon selector runtime has no spear source");
            problem = problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
            return result;
        }

        private static IEnumerable<KeyValuePair<string, Func<object>>> Parts(Func<string, BlueprintScriptableObject> resolve)
        {
            yield return new KeyValuePair<string, Func<object>>("ewp.Features", () => (resolve(EwpSelectionGuid) as BlueprintFeatureSelection)?.Features);
            yield return new KeyValuePair<string, Func<object>>("ewp.AllFeatures", () => (resolve(EwpSelectionGuid) as BlueprintFeatureSelection)?.AllFeatures);
            yield return new KeyValuePair<string, Func<object>>("finesse.Features", () => (resolve(FinesseSelectionGuid) as BlueprintFeatureSelection)?.Features);
            yield return new KeyValuePair<string, Func<object>>("finesse.AllFeatures", () => (resolve(FinesseSelectionGuid) as BlueprintFeatureSelection)?.AllFeatures);
            yield return new KeyValuePair<string, Func<object>>("familiarity.Components", () => (resolve(FamiliarityGuid) as BlueprintFeature)?.ComponentsArray);
        }

        private static string Text(object value)
        {
            if (value == null) return "<null>";
            if (!(value is IEnumerable items)) return value.ToString();
            return string.Join(",", items.Cast<object>().Select(item => item == null ? "<null>" :
                item is BlueprintScriptableObject blueprint ? blueprint.name :
                item is UnityEngine.Object unity ? unity.GetType().Name + ":" + unity.name : item.ToString()).ToArray());
        }

        private static bool RuntimeHasSpear()
        {
            Type runtime = ProductProbes.Entry().Assembly.GetType("KingmakerGunslinger.CustomWeapons.CustomWeaponSelectorRuntime", true);
            FieldInfo sources = runtime.GetField("Sources", BindingFlags.Static | BindingFlags.NonPublic) ??
                throw new MissingFieldException(runtime.FullName, "Sources");
            return ((IDictionary)sources.GetValue(null)).Contains(SelectorSourceKey);
        }
    }

    /// <summary>
    /// kmgItemPricing. Creates ordinary weapons (type: item guids) and queries Kingmaker's sale price and the item
    /// tooltip fill exactly as the inventory tooltip does (UIUtilityItem.FillTooltipData calls
    /// VendorLogic.GetItemSellPrice). expect "native": nothing throws and every price is the native
    /// (long)(Cost * Vendors.SellModifier); expect "throws": every one of the repeated price queries and every tooltip
    /// fill throws, and every one of those exceptions contains each "contains" text. match "battered": a production
    /// pistol bound as a battered firearm to the main character must sell for the product's fixed scrap value.
    /// invariants.repeat: queries per item (default 20).
    /// </summary>
    internal static class ItemPricingProbe
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static JObject Run(string[] itemGuids, string expect, string match, IList<string> contains, IDictionary<string, string> invariants,
            out string problem)
        {
            var problems = new List<string>();
            int repeat = invariants != null && invariants.TryGetValue("repeat", out string text) ? int.Parse(text, CultureInfo.InvariantCulture) : 20;
            double modifier = Game.Instance.BlueprintRoot.Vendors.SellModifier;
            var result = new JObject { ["sellModifier"] = modifier, ["repeat"] = repeat, ["trading"] = Game.Instance.Vendor.IsTrading };
            result["sellPricePatchOwners"] = SellPricePatchOwners();
            var items = new JArray();
            foreach (string guid in itemGuids)
            {
                BlueprintItemWeapon blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(guid);
                if (blueprint == null) { problems.Add("item " + guid + " does not resolve"); continue; }
                var item = new ItemEntityWeapon(blueprint);
                JObject row = Query(item, repeat, (long)(item.Cost * modifier), contains);
                row["guid"] = guid;
                items.Add(row);
                Judge(row, expect, contains, repeat, problems);
            }

            result["ordinary"] = items;
            if (match == "battered") result["battered"] = Battered(repeat, problems);
            result["batteredInspectionUnavailableReported"] = ProductBool("KingmakerGunslinger.Gunsmithing.BatteredFirearmOriginRuntime", "UnavailableReported");
            result["bootstrap"] = new JObject
            {
                ["initialized"] = ProductBool("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", "IsInitialized"),
                ["failed"] = ProductBool("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", "IsFailed")
            };
            problem = problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
            return result;
        }

        public static JToken SellPricePatchOwners()
        {
            try
            {
                Patches patches = (SpearRepairConditions.Harmony ?? HarmonyInstance.Create(SpearRepairConditions.HarnessId + ".reader"))
                    .GetPatchInfo(typeof(VendorLogic).GetMethod("GetItemSellPrice", new[] { typeof(ItemEntity) }));
                return patches == null ? "none" : string.Join(",", patches.Postfixes.Concat(patches.Prefixes).Select(p => p.owner).Distinct().ToArray());
            }
            catch (Exception exception) { return "unavailable:" + exception.GetType().Name; }
        }

        public static JObject Query(ItemEntity item, int repeat, long native, IList<string> causes = null)
        {
            var prices = new HashSet<long>();
            var tooltipPrices = new HashSet<string>();
            var priceErrors = new HashSet<string>();
            var tooltipErrors = new HashSet<string>();
            int priceThrows = 0, tooltipThrows = 0, priceCauseMatches = 0, tooltipCauseMatches = 0;
            string[] needles = (causes ?? new string[0]).ToArray();
            for (int index = 0; index < repeat; index++)
            {
                try { prices.Add(Game.Instance.Vendor.GetItemSellPrice(item)); }
                catch (Exception exception)
                {
                    priceThrows++;
                    string chain = Chain(exception);
                    priceErrors.Add(chain);
                    if (needles.Length > 0 && needles.All(n => chain.IndexOf(n, StringComparison.Ordinal) >= 0)) priceCauseMatches++;
                }

                try
                {
                    TooltipData data = UIUtilityItem.FillTooltipData(item, new TooltipData());
                    tooltipPrices.Add(data.Texts.TryGetValue(TooltipElement.SellPrice, out string sell) ? sell : "<missing>");
                }
                catch (Exception exception)
                {
                    tooltipThrows++;
                    string chain = Chain(exception);
                    tooltipErrors.Add(chain);
                    if (needles.Length > 0 && needles.All(n => chain.IndexOf(n, StringComparison.Ordinal) >= 0)) tooltipCauseMatches++;
                }
            }

            return new JObject
            {
                ["name"] = item.Blueprint.name, ["cost"] = item.Cost, ["nativeSellPrice"] = native, ["queries"] = repeat,
                ["sellPrices"] = new JArray(prices.Cast<object>().ToArray()), ["tooltipSellPrices"] = new JArray(tooltipPrices.Cast<object>().ToArray()),
                ["priceThrows"] = priceThrows, ["tooltipThrows"] = tooltipThrows,
                ["priceCauseMatches"] = priceCauseMatches, ["tooltipCauseMatches"] = tooltipCauseMatches,
                ["distinctPriceErrors"] = new JArray(priceErrors.Cast<object>().ToArray()),
                ["distinctTooltipErrors"] = new JArray(tooltipErrors.Cast<object>().ToArray())
            };
        }

        private static string Chain(Exception exception)
        {
            var parts = new List<string>();
            for (Exception current = exception; current != null; current = current.InnerException)
                parts.Add(current.GetType().Name + ": " + current.Message);
            return string.Join(" <- ", parts.ToArray());
        }

        private static void Judge(JObject row, string expect, IList<string> contains, int repeat, List<string> problems)
        {
            string name = (string)row["name"];
            if (expect == "throws")
            {
                if (contains == null || contains.Count == 0) problems.Add("expect=throws needs the cause text in contains");
                if ((int)row["priceThrows"] != repeat || (int)row["priceCauseMatches"] != repeat)
                    problems.Add(name + " price queries: " + row["priceThrows"] + " of " + repeat + " threw, " + row["priceCauseMatches"] + " with the expected cause");
                if ((int)row["tooltipThrows"] != repeat || (int)row["tooltipCauseMatches"] != repeat)
                    problems.Add(name + " tooltip fills: " + row["tooltipThrows"] + " of " + repeat + " threw, " + row["tooltipCauseMatches"] + " with the expected cause");
                return;
            }

            if ((int)row["priceThrows"] != 0 || (int)row["tooltipThrows"] != 0)
                problems.Add(name + " threw: " + row["distinctPriceErrors"].ToString(Newtonsoft.Json.Formatting.None) + " " +
                    row["distinctTooltipErrors"].ToString(Newtonsoft.Json.Formatting.None));
            long native = (long)row["nativeSellPrice"];
            JArray prices = (JArray)row["sellPrices"];
            if (prices.Count != 1 || (long)prices[0] != native) problems.Add(name + " sell prices " + prices.ToString(Newtonsoft.Json.Formatting.None) + ", native " + native);
            JArray tooltip = (JArray)row["tooltipSellPrices"];
            if (tooltip.Count != 1 || (string)tooltip[0] != native.ToString(CultureInfo.InvariantCulture))
                problems.Add(name + " tooltip sell prices " + tooltip.ToString(Newtonsoft.Json.Formatting.None) + ", native " + native);
        }

        private static JObject Battered(int repeat, List<string> problems)
        {
            BlueprintItemWeapon pistol = ProductItems.Pistol();
            long expected = ProductItems.FixedScrapValue();
            UnitEntityData owner = Game.Instance.Player.MainCharacter.Value;
            var item = new ItemEntityWeapon(pistol);
            var ordinary = new ItemEntityWeapon(pistol);
            ProductItems.BindBattered(item, owner);
            JObject battered = Query(item, repeat, expected);
            JObject plain = Query(ordinary, repeat, (long)(ordinary.Cost * Game.Instance.BlueprintRoot.Vendors.SellModifier));
            JArray prices = (JArray)battered["sellPrices"];
            if ((int)battered["priceThrows"] != 0 || prices.Count != 1 || (long)prices[0] != expected)
                problems.Add("battered pistol sold for " + prices.ToString(Newtonsoft.Json.Formatting.None) + " (throws " + battered["priceThrows"] + "), expected fixed " + expected);
            JArray plainPrices = (JArray)plain["sellPrices"];
            if ((int)plain["priceThrows"] != 0 || plainPrices.Count != 1 || (long)plainPrices[0] != (long)plain["nativeSellPrice"])
                problems.Add("unbound pistol sold for " + plainPrices.ToString(Newtonsoft.Json.Formatting.None) + ", native " + plain["nativeSellPrice"]);
            return new JObject { ["pistol"] = pistol.AssetGuid, ["fixedScrapValue"] = expected, ["batteredQuery"] = battered, ["unboundQuery"] = plain };
        }

        public static JToken ProductBool(string typeName, string property)
        {
            Type type = ProductProbes.Entry().Assembly.GetType(typeName, false);
            PropertyInfo info = type?.GetProperty(property, Any);
            return info == null ? (JToken)"<absent>" : (bool)info.GetValue(null, null);
        }
    }

    /// <summary>Product items and battered-origin calls the probes reach through reflection.</summary>
    internal static class ProductItems
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static Type Product(string name) => ProductProbes.Entry().Assembly.GetType(name, true);

        public static BlueprintItemWeapon Pistol()
        {
            object catalog = Product("KingmakerGunslinger.Bootstrap.BlueprintBootstrap").GetProperty("ProductionFirearms", Any).GetValue(null, null) ??
                throw new InvalidOperationException("no production firearm catalog (initialization did not complete)");
            object pistolSet = catalog.GetType().GetProperty("Pistol", Any).GetValue(catalog, null);
            return (BlueprintItemWeapon)pistolSet.GetType().GetProperty("Item", Any).GetValue(pistolSet, null);
        }

        public static BlueprintItemWeapon FoundationSpear()
        {
            object set = Product("KingmakerGunslinger.Bootstrap.BlueprintBootstrap").GetProperty("ElvenBranchedSpears", Any).GetValue(null, null) ??
                throw new InvalidOperationException("no Elven Branched Spear set (initialization did not complete)");
            Array entries = (Array)set.GetType().GetProperty("Entries", Any).GetValue(set, null);
            object first = entries.GetValue(0);
            return (BlueprintItemWeapon)first.GetType().GetProperty("Item", Any).GetValue(first, null);
        }

        public static long FixedScrapValue()
        {
            return Convert.ToInt64(Product("KingmakerGunslinger.Gunsmithing.BatteredFirearmUsePolicy").GetField("FixedExpectedScrapValueGold", Any).GetValue(null),
                CultureInfo.InvariantCulture);
        }

        public static void BindBattered(ItemEntityWeapon item, UnitEntityData owner)
        {
            try { Product("KingmakerGunslinger.Gunsmithing.BatteredFirearmOriginRuntime").GetMethod("Bind", Any).Invoke(null, new object[] { item, owner }); }
            catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
        }

        /// <summary>The product's own strict owner lookup; null when the item carries no battered origin.</summary>
        public static UnitEntityData BatteredOwner(ItemEntityWeapon item)
        {
            MethodInfo tryGet = Product("KingmakerGunslinger.Gunsmithing.BatteredFirearmOriginRuntime").GetMethod("TryGetOwner", Any);
            object[] arguments = { item, null };
            try { return (bool)tryGet.Invoke(null, arguments) ? (UnitEntityData)arguments[1] : null; }
            catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
        }
    }

    /// <summary>
    /// Fresh-process persistence of affected items. kmgSpearPersistFixture (a loaded save, initialization complete)
    /// puts a released foundation spear, a production pistol bound as battered to the main character, an unbound pistol
    /// and a Standard Longsword into the party inventory and writes their blueprint identities, inventory counts and
    /// the battered owner beside the session profile; the save is then written by saveGame. kmgSpearPersistedItems, in
    /// a later process that loaded that save, matches the items by blueprint identity and count (Kingmaker 2.1.7b items
    /// carry no unique id) and checks the spear's resolved weapon-type profile, the battered owner (the main
    /// character's unique id) and token through the product's own strict lookup, the fixed battered price and the
    /// native control prices.
    /// </summary>
    internal static class SpearPersistence
    {
        private const string FileName = "spear-persistence-fixture.json";
        private const string LongswordGuid = "6fd0a849531617844b195f452661b2cd";

        public static JObject Fixture(string profileDir)
        {
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            ItemEntityWeapon spear = Give(ProductItems.FoundationSpear());
            ItemEntityWeapon battered = Give(ProductItems.Pistol());
            ProductItems.BindBattered(battered, main);
            ItemEntityWeapon plain = Give(ProductItems.Pistol());
            ItemEntityWeapon longsword = Give(ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(LongswordGuid));
            // Kingmaker 2.1.7b items carry no unique id; the reload identifies them by blueprint, by the counts held
            // after this fixture, and by the battered origin the product's own lookup reports.
            var record = new JObject
            {
                ["owner"] = main.UniqueId, ["ownerName"] = main.CharacterName,
                ["spear"] = Describe(spear), ["battered"] = Describe(battered), ["plainPistol"] = Describe(plain), ["longsword"] = Describe(longsword),
                ["spearCount"] = Count(spear.Blueprint.AssetGuid), ["pistolCount"] = Count(battered.Blueprint.AssetGuid),
                ["batteredPistolCount"] = Battered(battered.Blueprint.AssetGuid).Length,
                ["batteredOwnerBeforeSave"] = ProductItems.BatteredOwner(battered)?.UniqueId
            };
            File.WriteAllText(Path.Combine(profileDir, FileName), record.ToString(Newtonsoft.Json.Formatting.Indented));
            return record;
        }

        private static ItemEntityWeapon[] Held(string guid)
        {
            return Game.Instance.Player.Inventory.OfType<ItemEntityWeapon>().Where(i => i.Blueprint != null && i.Blueprint.AssetGuid == guid).ToArray();
        }

        private static int Count(string guid) => Held(guid).Length;

        private static ItemEntityWeapon[] Battered(string guid) => Held(guid).Where(i => ProductItems.BatteredOwner(i) != null).ToArray();

        public static JObject Verify(string profileDir, out string problem)
        {
            var problems = new List<string>();
            string path = Path.Combine(profileDir, FileName);
            if (!File.Exists(path)) { problem = "no persistence fixture record at " + path; return new JObject(); }
            JObject fixture = JObject.Parse(File.ReadAllText(path));
            var result = new JObject { ["fixture"] = fixture, ["sellPricePatchOwners"] = ItemPricingProbe.SellPricePatchOwners() };
            double modifier = Game.Instance.BlueprintRoot.Vendors.SellModifier;
            var found = new Dictionary<string, ItemEntityWeapon>();
            string spearGuid = (string)fixture["spear"]["blueprint"], pistolGuid = (string)fixture["battered"]["blueprint"];
            string longswordGuid = (string)fixture["longsword"]["blueprint"];
            ItemEntityWeapon[] spears = Held(spearGuid), pistols = Held(pistolGuid), batteredPistols = Battered(pistolGuid);
            result["counts"] = new JObject
            {
                ["spear"] = spears.Length, ["pistol"] = pistols.Length, ["batteredPistol"] = batteredPistols.Length, ["longsword"] = Count(longswordGuid)
            };
            if (spears.Length != (int)fixture["spearCount"]) problems.Add("the loaded inventory holds " + spears.Length + " spears, saved " + fixture["spearCount"]);
            if (pistols.Length != (int)fixture["pistolCount"]) problems.Add("the loaded inventory holds " + pistols.Length + " pistols, saved " + fixture["pistolCount"]);
            if (batteredPistols.Length != (int)fixture["batteredPistolCount"])
                problems.Add("the loaded inventory holds " + batteredPistols.Length + " battered pistols, saved " + fixture["batteredPistolCount"]);
            if (spears.Length > 0) found["spear"] = spears[0];
            if (batteredPistols.Length > 0) found["battered"] = batteredPistols[0];
            ItemEntityWeapon plainPistol = pistols.FirstOrDefault(i => ProductItems.BatteredOwner(i) == null);
            if (plainPistol != null) found["plainPistol"] = plainPistol; else problems.Add("no unbound pistol in the loaded inventory");
            ItemEntityWeapon sword = Held(longswordGuid).FirstOrDefault();
            if (sword != null) found["longsword"] = sword; else problems.Add("no Standard Longsword in the loaded inventory");

            foreach (ItemEntityWeapon each in spears)
            {
                BlueprintWeaponType eachType = each.Blueprint.Type;
                if (eachType == null || eachType.name != WeaponTypeProfiles.OwnedSpearTypeName) problems.Add("a loaded spear resolved to weapon type " + eachType?.name);
            }

            if (found.TryGetValue("spear", out ItemEntityWeapon spear))
            {
                BlueprintWeaponType type = spear.Blueprint.Type;
                JObject fields = WeaponTypeProfiles.Fields(type), getters = WeaponTypeProfiles.Getters(type);
                result["spear"] = new JObject { ["item"] = Describe(spear), ["type"] = type.name, ["typeGuid"] = type.AssetGuid, ["fields"] = fields, ["getters"] = getters };
                if (type.name != WeaponTypeProfiles.OwnedSpearTypeName) problems.Add("the loaded spear's weapon type is " + type.name);
                foreach (KeyValuePair<string, string> pair in WeaponTypeProfiles.Intended)
                    if ((string)fields[pair.Key] != pair.Value) problems.Add("loaded spear " + pair.Key + " is " + fields[pair.Key] + ", expected " + pair.Value);
                foreach (KeyValuePair<string, string> pair in WeaponTypeProfiles.IntendedGetters)
                    if ((string)getters[pair.Key] != pair.Value) problems.Add("loaded spear getter " + pair.Key + " is " + getters[pair.Key] + ", expected " + pair.Value);
            }

            if (found.TryGetValue("battered", out ItemEntityWeapon battered))
            {
                UnitEntityData owner = ProductItems.BatteredOwner(battered);
                long fixedValue = ProductItems.FixedScrapValue();
                JObject query = ItemPricingProbe.Query(battered, 10, fixedValue);
                result["battered"] = new JObject { ["item"] = Describe(battered), ["owner"] = owner?.UniqueId, ["ownerName"] = owner?.CharacterName, ["query"] = query };
                if (owner == null || owner.UniqueId != (string)fixture["owner"]) problems.Add("the loaded battered pistol's owner is " + (owner?.UniqueId ?? "<none>") + ", saved " + fixture["owner"]);
                RequirePrice("battered pistol", query, fixedValue, problems);
            }

            foreach (string key in new[] { "plainPistol", "longsword" })
            {
                if (!found.TryGetValue(key, out ItemEntityWeapon item)) continue;
                long native = (long)(item.Cost * modifier);
                JObject query = ItemPricingProbe.Query(item, 10, native);
                UnitEntityData owner = ProductItems.BatteredOwner(item);
                result[key] = new JObject { ["item"] = Describe(item), ["batteredOwner"] = owner?.UniqueId, ["query"] = query };
                if (owner != null) problems.Add(key + " carries a battered origin");
                RequirePrice(key, query, native, problems);
            }

            problem = problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
            return result;
        }

        private static void RequirePrice(string name, JObject query, long expected, List<string> problems)
        {
            JArray prices = (JArray)query["sellPrices"];
            JArray tooltip = (JArray)query["tooltipSellPrices"];
            if ((int)query["priceThrows"] != 0 || (int)query["tooltipThrows"] != 0 || prices.Count != 1 || (long)prices[0] != expected ||
                tooltip.Count != 1 || (string)tooltip[0] != expected.ToString(CultureInfo.InvariantCulture))
                problems.Add(name + " sold for " + prices.ToString(Newtonsoft.Json.Formatting.None) + " (tooltip " + tooltip.ToString(Newtonsoft.Json.Formatting.None) +
                    ", throws " + query["priceThrows"] + "/" + query["tooltipThrows"] + "), expected " + expected);
        }

        private static ItemEntityWeapon Give(BlueprintItemWeapon blueprint)
        {
            if (blueprint == null) throw new InvalidOperationException("a fixture weapon blueprint does not resolve");
            return Game.Instance.Player.Inventory.Add(blueprint) as ItemEntityWeapon ??
                throw new InvalidOperationException("inventory add did not yield a weapon for " + blueprint.name);
        }

        private static JObject Describe(ItemEntityWeapon item)
        {
            return new JObject
            {
                ["blueprint"] = item.Blueprint.AssetGuid, ["name"] = item.Blueprint.name,
                ["enchantments"] = new JArray(item.Enchantments.Where(e => e != null && e.Blueprint != null).Select(e => (object)e.Blueprint.name).ToArray())
            };
        }
    }

    /// <summary>
    /// The desktop party selection (SelectionManager.SelectedUnits) and the group's current character: fixture setup
    /// (kmgSetSelection), observation (kmgSelectionState) and the exact restoration the sorcerer probe uses.
    /// </summary>
    internal static class SelectionFixture
    {
        public static JObject Describe(UnitEntityData[] units, UnitEntityData current)
        {
            return new JObject
            {
                ["count"] = units.Length,
                ["units"] = new JArray(units.Select(u => (object)Name(u)).ToArray()),
                ["current"] = current == null ? "<none>" : Name(current)
            };
        }

        public static JObject Current()
        {
            SelectionManager manager = SelectionManager.Instance ?? throw new InvalidOperationException("Kingmaker's selection manager is unavailable");
            return Describe(manager.SelectedUnits.ToArray(), Kingmaker.UI.Group.GroupController.Instance?.GetCurrentCharacter());
        }

        /// <summary>"empty": unselect every selected unit through the native manager; "main": select the main character alone.</summary>
        public static JObject Set(string mode)
        {
            SelectionManager manager = SelectionManager.Instance ?? throw new InvalidOperationException("Kingmaker's selection manager is unavailable");
            if (mode == "empty") foreach (UnitEntityData unit in manager.SelectedUnits.ToArray()) manager.UnselectUnit(unit);
            else if (mode == "main") manager.SelectUnit(Game.Instance.Player.MainCharacter.Value.View, true, true, false);
            else throw new ArgumentException("kmgSetSelection takes empty or main, not " + mode);
            return Current();
        }

        /// <summary>Puts back exactly the captured selection (any size, empty included) and current character.</summary>
        public static void Restore(UnitEntityData[] selection, UnitEntityData current)
        {
            SelectionManager manager = SelectionManager.Instance ?? throw new InvalidOperationException("Kingmaker's selection manager is unavailable");
            Kingmaker.UI.Group.GroupController group = Kingmaker.UI.Group.GroupController.Instance;
            if (group != null && current != null && current.IsInState && !ReferenceEquals(group.GetCurrentCharacter(), current)) group.SelectUnit(current);
            if (selection.Length > 0) manager.MultiSelect(selection.Select(u => u.View).ToArray(), false);
            else foreach (UnitEntityData unit in manager.SelectedUnits.ToArray()) manager.UnselectUnit(unit);
        }

        /// <summary>Null when the selection holds exactly the expected units (as a set, by reference) and the current character matches.</summary>
        public static string Mismatch(UnitEntityData[] expected, UnitEntityData expectedCurrent, UnitEntityData[] actual, UnitEntityData actualCurrent)
        {
            var problems = new List<string>();
            UnitEntityData[] missing = expected.Where(u => !actual.Any(a => ReferenceEquals(a, u))).ToArray();
            UnitEntityData[] extra = actual.Where(a => !expected.Any(u => ReferenceEquals(a, u))).ToArray();
            if (missing.Length > 0 || extra.Length > 0 || expected.Length != actual.Length)
                problems.Add("selected " + actual.Length + " [" + string.Join(",", actual.Select(Name).ToArray()) + "], expected " + expected.Length +
                    " [" + string.Join(",", expected.Select(Name).ToArray()) + "]");
            if (!ReferenceEquals(expectedCurrent, actualCurrent))
                problems.Add("current character " + (actualCurrent == null ? "<none>" : Name(actualCurrent)) + ", expected " +
                    (expectedCurrent == null ? "<none>" : Name(expectedCurrent)));
            return problems.Count == 0 ? null : string.Join("; ", problems.ToArray());
        }

        private static string Name(UnitEntityData unit)
        {
            return unit == null ? "<null>" : (unit.Blueprint?.name ?? "<no blueprint>") + "#" + unit.UniqueId;
        }
    }

    /// <summary>
    /// kmgSorcererSummon, an owned probe. In the loaded save a fresh disposable character takes Sorcerer level 1 through
    /// the native level-up controller in character-generation mode (the native class path) and learns Summon Monster I
    /// through the controller's own spell selection. It joins the party and is selected the way a portrait click
    /// selects; the action bar's spell group is opened through its own click handler and the Summon Monster I slot the
    /// game built is clicked.
    ///
    /// expect "expanded": the creature menu the game filled must be open with visible named entries carrying icons; the
    /// chosen entry ("contains" picks by name, otherwise the first) is clicked through its own slot, which must put
    /// exactly that entry's ability into Kingmaker's ability-targeting handler. expect "native-single": Summon Monster I
    /// must be Kingmaker's own spell with no variants, no menu may open, and the slot click must put the spell itself
    /// into the targeting handler. In both, the targeting handler's own ground click casts; the command it issued must
    /// carry the selected ability and succeed, and a creature that ability's actions spawn must appear as the caster's
    /// summon. Release gives back everything the probe created: the summons, the party entry, the caster, the
    /// selection, the pause state, the targeting handler and the blueprint clone.
    /// </summary>
    internal sealed class SorcererSummonProbe : ScreenProbe, IOwnedProbe
    {
        private enum Stage { Start, Selected, GroupOpened, MenuOpened, ClickEntry, EntryClicked, GroundClick, Casting, Stalled }

        private const string SorcererGuid = "b3a505fb61437dc4097f43c3f8f9a4cf";
        private const string SummonMonsterOneGuid = "8fd74eddd9b6c224693d9ab241f25e84";
        private const int MaxAttempts = 3;
        private readonly string expect;
        private readonly string[] pick;
        private readonly string stallAfter;
        private readonly string failRelease;
        private readonly JArray attempts = new JArray();
        private Stage stage = Stage.Start;
        private BlueprintUnit clone;
        private UnitEntityData caster;
        private bool addedToParty;
        private bool pauseChanged;
        private bool pausedBefore;
        // The incoming party selection (any size, including empty) and the group's current character, captured before
        // the probe changes anything; Release restores exactly these and verifies them.
        private UnitEntityData[] incomingSelection;
        private UnitEntityData incomingCurrent;
        private BlueprintAbility summonMonsterOne;
        private ActionBarSpellsGroup subGroup;
        private AbilityData sourceSpell;
        private AbilityData chosen;
        private UnitUseAbility command;
        private double castAt;
        private readonly string expectAbility;
        private readonly string expectUnit;

        // What this probe acquired; Release gives back exactly these and nothing it found already in place.
        private bool selectionChanged;
        private bool openedSpellGroup;
        private bool acquiredTargeting;

        public SorcererSummonProbe(string directory, string label, string expect, IEnumerable<string> pick, IDictionary<string, string> invariants)
            : base(directory, label)
        {
            if (expect != "expanded" && expect != "native-single") throw new ArgumentException("kmgSorcererSummon expects expanded or native-single", "expect");
            this.expect = expect;
            this.pick = pick.ToArray();
            stallAfter = invariants != null && invariants.TryGetValue("stallAfter", out string stall) ? stall : null;
            failRelease = invariants != null && invariants.TryGetValue("failRelease", out string fail) ? fail : null;
            // Independent identities for the chosen row: the released ability and unit (guid or blueprint name), taken
            // from the session, never from the ability under test.
            expectAbility = invariants != null && invariants.TryGetValue("expectAbility", out string ability) ? ability : null;
            expectUnit = invariants != null && invariants.TryGetValue("expectUnit", out string unit) ? unit : null;
            if (stallAfter == null && (expectAbility == null || expectUnit == null))
                throw new ArgumentException("kmgSorcererSummon needs invariants expectAbility and expectUnit for the chosen row");
            if (stallAfter != null) Record["testOnlyStallAfter"] = stallAfter;
            if (failRelease != null) Record["testOnlyFailRelease"] = failRelease;
        }

        object IOwnedProbe.Result => Record;

        protected override string Step()
        {
            switch (stage)
            {
                case Stage.Start: return Start();
                case Stage.Selected: return OpenSpellGroup();
                case Stage.GroupOpened: return ClickSummonMonsterSlot();
                case Stage.MenuOpened: return ReadMenu();
                case Stage.ClickEntry: return ClickEntry();
                case Stage.EntryClicked: return ConfirmEntrySelection();
                case Stage.GroundClick: return GroundClick();
                case Stage.Casting: return AwaitSummon();
                default: return null;
            }
        }

        private string Start()
        {
            summonMonsterOne = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(SummonMonsterOneGuid);
            BlueprintCharacterClass sorcerer = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>(SorcererGuid);
            if (summonMonsterOne == null || sorcerer == null) return "Summon Monster I or the Sorcerer class does not resolve";
            Record["summonMonsterI"] = new JObject
            {
                ["name"] = summonMonsterOne.name, ["variants"] = summonMonsterOne.Variants?.Length ?? 0,
                ["variantNames"] = new JArray((summonMonsterOne.Variants ?? new BlueprintAbility[0]).Where(v => v != null).Select(v => (object)v.name).ToArray())
            };
            Record["publishedExpandedParent"] = PublishedExpandedParent(summonMonsterOne);
            Record["partyBefore"] = Game.Instance.Player.PartyCharacters.Count;
            // The incoming selection, captured before anything else so even a refusal is checked against it.
            SelectionManager manager = SelectionManager.Instance;
            if (manager == null) return "Kingmaker's selection manager is unavailable; nothing was changed";
            incomingSelection = manager.SelectedUnits.ToArray();
            incomingCurrent = Kingmaker.UI.Group.GroupController.Instance?.GetCurrentCharacter();
            Record["incomingSelection"] = SelectionFixture.Describe(incomingSelection, incomingCurrent);
            if (incomingSelection.Any(u => u == null || !u.IsInState || u.View == null))
                return "the incoming selection holds a unit without a live view, which cannot be restored natively; nothing was changed";
            // Precondition: an idle UI. A probe that inherits active targeting or an open action-bar group refuses
            // before it changes anything, so the fixture's state stays exactly as it was.
            string incomingTargeting = Game.Instance.SelectedAbilityHandler?.Ability?.Blueprint?.name;
            string[] openGroups = (ActionBarManager.Instance?.Group?.GroupElements ?? new List<ActionBarGroupElement>())
                .Where(e => e != null && e.ToggleState).Select(e => e.SlotType.ToString()).ToArray();
            Record["incomingUi"] = new JObject { ["targeting"] = incomingTargeting, ["openGroups"] = new JArray(openGroups.Cast<object>().ToArray()) };
            if (incomingTargeting != null || openGroups.Length > 0)
                return "the UI is not idle before the probe (targeting " + (incomingTargeting ?? "none") + ", open groups " +
                    (openGroups.Length == 0 ? "none" : string.Join(",", openGroups)) + "); nothing was changed";
            pausedBefore = Game.Instance.IsPaused;
            clone = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
            clone.name = "KmgLoaderCompat_NewSorcerer";
            UnitEntityData main = Game.Instance.Player.MainCharacter.Value;
            caster = Game.Instance.EntityCreator.SpawnUnit(clone, main.Position + main.OrientationDirection * 1.5f, Quaternion.identity,
                Game.Instance.State.LoadedAreaState.MainState);
            if (caster == null || caster.View == null) return "the new sorcerer was not spawned";
            caster.Descriptor.Stats.Charisma.BaseValue = 20;
            var levelUp = new JObject();
            Record["levelUp"] = levelUp;
            LevelSorcerer(caster, sorcerer, levelUp);
            Spellbook book = caster.Descriptor.GetSpellbook(sorcerer);
            if (book == null) return "the new sorcerer has no spellbook";
            book.Rest();
            Record["knownLevelOne"] = new JArray(book.GetKnownSpells(1).Select(s => (object)s.Blueprint.name).ToArray());
            Record["spellsPerDayLevelOne"] = book.GetSpellsPerDay(1);
            if (!book.GetKnownSpells(1).Any(s => ReferenceEquals(s.Blueprint, summonMonsterOne)))
                return "the new sorcerer did not learn Summon Monster I through level-up";
            Game.Instance.Player.PartyCharacters.Add(caster);
            addedToParty = true;
            Game.Instance.Player.InvalidateCharacterLists();
            Game.Instance.Player.UpdateCharacterLists();
            if (stallAfter == "party")
            {
                // Test-only: hold the step after the party insertion so a step timeout has state to release.
                stage = Stage.Stalled;
                return null;
            }

            selectionChanged = true;
            SelectionManager.Instance.SelectUnit(caster.View, true, true, false);
            stage = Stage.Selected;
            Wait(30);
            return null;
        }

        private string OpenSpellGroup()
        {
            ActionBarGroupElement spells = SpellGroup();
            if (spells == null) return "the action bar has no spell group";
            Record["selectedUnit"] = SelectionManager.Instance.GetSingleSelectedUnit()?.Blueprint?.name;
            if (!BarShowsCaster()) return Reselect("the action bar did not show the new sorcerer after selection");
            if (!spells.ToggleState) { spells.OnClick(); openedSpellGroup = true; }
            stage = Stage.GroupOpened;
            Wait(12);
            return null;
        }

        private string ClickSummonMonsterSlot()
        {
            if (!BarShowsCaster()) return Reselect("the action bar left the new sorcerer before the slot click");
            ActionBarGroupElement spells = SpellGroup();
            Record["spellGroupOpen"] = spells.ToggleState;
            var seen = new JArray();
            ActionBarGroupSlot target = null;
            foreach (ActionBarGroupSlot slot in Field<List<ActionBarGroupSlot>>(spells, "m_Slots"))
            {
                AbilityData spell = (slot.MechanicSlot as MechanicActionBarSlotSpontaneusSpell)?.Spell;
                if (spell == null) continue;
                seen.Add(spell.Blueprint.name + (slot.gameObject.activeInHierarchy ? "" : " (hidden)"));
                if (ReferenceEquals(spell.Blueprint, summonMonsterOne) && slot.gameObject.activeInHierarchy) target = slot;
            }

            Record["spellSlots"] = seen;
            if (target == null) return "no visible Summon Monster I slot on the new sorcerer's spell bar";
            sourceSpell = ((MechanicActionBarSlotSpontaneusSpell)target.MechanicSlot).Spell;
            subGroup = Field<ActionBarSpellsGroup>(target, "SubGroup");
            acquiredTargeting = true;
            target.OnClick();
            stage = Stage.MenuOpened;
            Wait(10);
            return null;
        }

        private string ReadMenu()
        {
            List<ActionBarSpontaneousConvertedSlot> entries = Field<List<ActionBarSpontaneousConvertedSlot>>(subGroup, "m_Slots");
            var rows = new JArray();
            foreach (ActionBarSpontaneousConvertedSlot entry in entries)
            {
                AbilityData data = (entry.MechanicSlot as MechanicActionBarSlotSpontaneusConvertedSpell)?.Spell;
                rows.Add(new JObject
                {
                    ["name"] = data?.Name, ["blueprint"] = data?.Blueprint?.name, ["guid"] = data?.Blueprint?.AssetGuid,
                    ["icon"] = data?.Icon != null ? data.Icon.name : null, ["visible"] = entry.gameObject.activeInHierarchy,
                    ["nestedVariants"] = data?.Blueprint?.Variants?.Length ?? 0
                });
            }

            bool menuOpen = subGroup.gameObject.activeInHierarchy;
            bool nativeSingle = summonMonsterOne.Variants == null || summonMonsterOne.Variants.Length == 0;
            Record["menuOpen"] = menuOpen;
            Record["menuEntries"] = rows;
            Record["menuEntryCount"] = rows.Count;
            Record["nativeSingleSpell"] = nativeSingle;
            Capture("summon-monster-i-menu");
            if (expect == "native-single")
            {
                if (!nativeSingle) return "Summon Monster I carries " + summonMonsterOne.Variants.Length + " variants; the native unexpanded spell was expected";
                if (menuOpen || rows.Count != 0) return "a native single Summon Monster I opened a menu with " + rows.Count + " entries";
                if (Game.Instance.SelectedAbilityHandler.Ability != sourceSpell)
                    return "the Summon Monster I slot click did not put the spell into the targeting handler (it holds " +
                        Game.Instance.SelectedAbilityHandler.Ability?.Blueprint?.name + ")";
                chosen = sourceSpell;
                Record["chosen"] = new JObject { ["route"] = "native-single-spell", ["name"] = chosen.Name, ["blueprint"] = chosen.Blueprint.name };
                Record["uiSelectedAbility"] = Game.Instance.SelectedAbilityHandler.Ability.Blueprint.name;
                stage = Stage.GroundClick;
                return null;
            }

            if (nativeSingle) return "Summon Monster I has no variants; the product's published creature menu was expected";
            if (!(bool)Record["publishedExpandedParent"]) return "Summon Monster I does not carry the product's published menu";
            if (!menuOpen) return Reselect("the Summon Monster I creature menu was not open " + (BarShowsCaster()
                ? "while the bar still showed the new sorcerer" : "because the bar no longer showed the new sorcerer"));
            if (rows.Count < 2) return "the Summon Monster I menu shows " + rows.Count + " creature entries";
            JObject[] bad = rows.Cast<JObject>().Where(r => !(bool)r["visible"] || string.IsNullOrWhiteSpace((string)r["name"]) || string.IsNullOrEmpty((string)r["icon"])).ToArray();
            if (bad.Length > 0) return "menu entries are not visible with a name and icon: " + string.Join(", ", bad.Select(b => b.ToString(Newtonsoft.Json.Formatting.None)).ToArray());
            int index = entries.FindIndex(e => Matches(e));
            if (index < 0 && pick.Length == 0) index = entries.FindIndex(NoNested);
            if (index < 0) return "no menu entry matches " + string.Join(",", pick);
            ActionBarSpontaneousConvertedSlot choice = entries[index];
            chosen = ((MechanicActionBarSlotSpontaneusConvertedSpell)choice.MechanicSlot).Spell;
            Record["chosen"] = new JObject { ["route"] = "creature-menu-entry", ["row"] = index, ["name"] = chosen.Name, ["blueprint"] = chosen.Blueprint.name };
            choiceSlot = choice;
            stage = Stage.ClickEntry;
            return null;
        }

        private ActionBarSpontaneousConvertedSlot choiceSlot;

        // After the menu picture is written, the chosen row is clicked through its own slot.
        private string ClickEntry()
        {
            if (AwaitingScreenshots()) return null;
            acquiredTargeting = true;
            choiceSlot.OnClick();
            stage = Stage.EntryClicked;
            Wait(5);
            return null;
        }

        private string ConfirmEntrySelection()
        {
            AbilityData selected = Game.Instance.SelectedAbilityHandler.Ability;
            Record["uiSelectedAbility"] = selected?.Blueprint?.name;
            if (selected != chosen)
                return "clicking the menu entry " + chosen.Blueprint.name + " put " + (selected?.Blueprint?.name ?? "nothing") + " into the targeting handler";
            stage = Stage.GroundClick;
            return null;
        }

        private string GroundClick()
        {
            if (AwaitingScreenshots()) return null;
            if (subGroup != null && subGroup.gameObject.activeInHierarchy) subGroup.Hide(true);
            if (Game.Instance.IsPaused) { Game.Instance.IsPaused = false; pauseChanged = true; }
            Vector3 point = caster.Position + caster.OrientationDirection * 2.5f;
            int before = Commands().Count;
            bool accepted = Game.Instance.SelectedAbilityHandler.OnClick(null, point, 0);
            Record["groundClickAccepted"] = accepted;
            if (!accepted) return "the targeting handler refused the ground click for " + chosen.Blueprint.name;
            command = Commands().OfType<UnitUseAbility>().LastOrDefault();
            Record["commandsIssued"] = Commands().Count - before;
            Record["submittedAbility"] = command?.Spell?.Blueprint?.name;
            if (command == null) return "the ground click issued no ability command";
            if (command.Spell.Blueprint != chosen.Blueprint) return "the ground click cast " + command.Spell.Blueprint.name + ", not the selected " + chosen.Blueprint.name;
            castAt = Time.realtimeSinceStartup;
            stage = Stage.Casting;
            return null;
        }

        private string AwaitSummon()
        {
            UnitEntityData[] summoned = Summons();
            if (!command.IsFinished || summoned.Length == 0)
            {
                if (Time.realtimeSinceStartup - castAt > 40)
                    return "no finished cast with a summon after 40 s (command result " + command.Result + ", finished " + command.IsFinished + ", summons " + summoned.Length + ")";
                return null;
            }

            // Recorded only: the creatures the chosen ability's own actions name. The verdict uses the session's
            // independent expectation (expectAbility / expectUnit) instead.
            BlueprintUnit[] fromActions = ExpectedSummons(chosen.Blueprint);
            Record["commandResult"] = command.Result.ToString();
            Record["actionCreatures"] = new JArray(fromActions.Select(u => (object)(u.name + "|" + u.AssetGuid)).ToArray());
            Record["summoned"] = new JArray(summoned.Select(u => (object)(u.Blueprint.name + "|" + u.Blueprint.AssetGuid + "|" + u.CharacterName)).ToArray());
            Record["expected"] = new JObject { ["ability"] = expectAbility, ["unit"] = expectUnit };
            Record["chosenIdentity"] = chosen.Blueprint.name + "|" + chosen.Blueprint.AssetGuid;
            Record["submittedIdentity"] = command.Spell.Blueprint.name + "|" + command.Spell.Blueprint.AssetGuid;
            if (command.Result != UnitCommand.ResultType.Success) return "the cast of " + chosen.Blueprint.name + " ended " + command.Result;
            if (!Is(chosen.Blueprint, expectAbility) || !Is(command.Spell.Blueprint, expectAbility))
                return "the chosen row and submitted command are " + Record["chosenIdentity"] + " / " + Record["submittedIdentity"] + ", expected " + expectAbility;
            if (!summoned.Any(u => Is(u.Blueprint, expectUnit)))
                return "the summoned " + string.Join(",", summoned.Select(u => u.Blueprint.name + "|" + u.Blueprint.AssetGuid).ToArray()) + " is not the expected " + expectUnit;
            return string.Empty;
        }

        private static bool Is(BlueprintScriptableObject blueprint, string identity)
        {
            return blueprint != null && identity != null && (string.Equals(blueprint.AssetGuid, identity, StringComparison.Ordinal) ||
                string.Equals(blueprint.name, identity, StringComparison.Ordinal));
        }

        private List<UnitCommand> Commands()
        {
            var commands = new List<UnitCommand>();
            if (caster.Commands.Standard != null) commands.Add(caster.Commands.Standard);
            commands.AddRange(caster.Commands.Queue);
            return commands;
        }

        private UnitEntityData[] Summons()
        {
            return Game.Instance.State.Units.Where(u => u != null && caster != null && u.Get<UnitPartSummonedMonster>()?.Summoner == caster).ToArray();
        }

        /// <summary>Creature blueprints an ability's actions spawn, found by walking its components and nested actions.</summary>
        private static BlueprintUnit[] ExpectedSummons(BlueprintAbility ability)
        {
            var found = new List<BlueprintUnit>();
            var seen = new HashSet<object>();
            foreach (BlueprintComponent component in ability.ComponentsArray ?? new BlueprintComponent[0]) Walk(component, found, seen, 0);
            return found.Distinct().ToArray();
        }

        private static void Walk(object value, List<BlueprintUnit> found, HashSet<object> seen, int depth)
        {
            if (value == null || depth > 10 || value is string || value.GetType().IsPrimitive || value.GetType().IsEnum || !seen.Add(value)) return;
            if (value is BlueprintUnit unit) { found.Add(unit); return; }
            if (value is BlueprintScriptableObject && !(value is BlueprintAbility)) return;
            if (value is BlueprintAbility nested)
            {
                foreach (BlueprintComponent component in nested.ComponentsArray ?? new BlueprintComponent[0]) Walk(component, found, seen, depth + 1);
                return;
            }

            if (value is IEnumerable sequence)
            {
                foreach (object item in sequence) Walk(item, found, seen, depth + 1);
                return;
            }

            if (!(value is BlueprintComponent || value is GameAction || value is ActionList || value is Element)) return;
            for (Type type = value.GetType(); type != null && type != typeof(object); type = type.BaseType)
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType.IsPrimitive || field.FieldType.IsEnum || field.FieldType == typeof(string)) continue;
                    Walk(field.GetValue(value), found, seen, depth + 1);
                }
        }

        private bool Matches(ActionBarSpontaneousConvertedSlot entry)
        {
            AbilityData data = (entry.MechanicSlot as MechanicActionBarSlotSpontaneusConvertedSpell)?.Spell;
            return pick.Length > 0 && data != null && pick.Any(p => (data.Name ?? string.Empty).IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0) && NoNested(entry);
        }

        private static bool NoNested(ActionBarSpontaneousConvertedSlot entry)
        {
            AbilityData data = (entry.MechanicSlot as MechanicActionBarSlotSpontaneusConvertedSpell)?.Spell;
            return data != null && (data.Blueprint.Variants == null || data.Blueprint.Variants.Length == 0);
        }

        private static ActionBarGroupElement SpellGroup()
        {
            return ActionBarManager.Instance?.Group?.GroupElements.FirstOrDefault(e => e != null && e.SlotType == ActionBarSlotType.Spell);
        }

        /// <summary>Whether the action bar the game built is the new sorcerer's (its private selected unit).</summary>
        private bool BarShowsCaster()
        {
            ActionBarManager bar = ActionBarManager.Instance;
            return bar != null && ReferenceEquals(Field<UnitEntityData>(bar, "m_Selected"), caster);
        }

        /// <summary>
        /// Records why this attempt did not reach the menu and selects the sorcerer again the way a portrait click
        /// does, up to three attempts; returns the failure once they are spent.
        /// </summary>
        private string Reselect(string reason)
        {
            attempts.Add(reason);
            Record["attempts"] = attempts;
            if (attempts.Count >= MaxAttempts) return reason + " (after " + attempts.Count + " attempts)";
            if (subGroup != null && subGroup.gameObject.activeInHierarchy) subGroup.Hide(true);
            ActionBarGroupElement spells = SpellGroup();
            if (openedSpellGroup && spells != null && spells.ToggleState) { spells.OnClick(); openedSpellGroup = false; }
            selectionChanged = true;
            SelectionManager.Instance.SelectUnit(caster.View, true, true, false);
            stage = Stage.Selected;
            Wait(30);
            return null;
        }

        private static void LevelSorcerer(UnitEntityData unit, BlueprintCharacterClass sorcerer, JObject record)
        {
            MethodInfo apply = typeof(LevelUpController).GetMethod("ApplyLevelup", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (apply == null) throw new MissingMethodException("LevelUpController.ApplyLevelup");
            BlueprintAbility summon = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(SummonMonsterOneGuid);
            LevelUpController controller = LevelUpController.StartWithoutAssigningStaticInstance(unit.Descriptor, false, null, null,
                LevelUpState.CharBuildMode.CharGen);
            try
            {
                // The native class screen's path: class mechanics, spellbook (which opens the known-spell selection)
                // and skill points.
                if (!controller.SelectClass(sorcerer, true)) throw new InvalidOperationException("the level-up controller refused Sorcerer");
                BlueprintSpellbook spellbook = sorcerer.Spellbook;
                BlueprintSpellList list = spellbook.SpellList;
                record["spellSelections"] = new JArray(controller.State.SpellSelections.Select(s => (object)(s.Spellbook.name + "/" + s.SpellList.name)).ToArray());
                record["listLevel"] = list.GetLevel(summon);
                bool selected = controller.SelectSpell(spellbook, list, 1, summon, 0);
                record["selectSpell"] = selected;
                if (!selected) throw new InvalidOperationException("the level-up controller refused Summon Monster I as a level-1 known spell");
                apply.Invoke(controller, new object[] { unit.Descriptor });
            }
            finally
            {
                controller.Cancel();
            }

            record["sorcererLevel"] = unit.Descriptor.Progression.GetClassLevel(sorcerer);
        }

        private static bool PublishedExpandedParent(BlueprintAbility ability)
        {
            Type publisher = ProductProbes.Entry().Assembly.GetType("KingmakerGunslinger.Blueprints.ExpandedSummoningPublisher", false);
            MethodInfo check = publisher?.GetMethod("IsPublishedExpandedParent", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            return check != null && (bool)check.Invoke(null, new object[] { ability });
        }

        public CleanupReport Release(string testFailure)
        {
            var releases = new List<KeyValuePair<string, Action>>
            {
                // Only what this probe acquired: targeting it started, the menu of the group it opened, that group,
                // and a selection it changed. Nothing it found in place is touched.
                new KeyValuePair<string, Action>("targeting", () =>
                {
                    AbilityData current = Game.Instance.SelectedAbilityHandler?.Ability;
                    if (acquiredTargeting && current != null && (current == chosen || current == sourceSpell))
                        Game.Instance.SelectedAbilityHandler.DropAbility();
                }),
                new KeyValuePair<string, Action>("menu", () =>
                {
                    if (openedSpellGroup && subGroup != null && subGroup.gameObject.activeInHierarchy) subGroup.Hide(true);
                }),
                new KeyValuePair<string, Action>("spell-group", () =>
                {
                    ActionBarGroupElement spells = SpellGroup();
                    if (openedSpellGroup && spells != null && spells.ToggleState) spells.OnClick();
                }),
                new KeyValuePair<string, Action>("selection", () =>
                {
                    // Before the caster goes, so destroying a selected unit cannot trigger the game's own reselection.
                    // Restores the incoming selection exactly, empty and multi-unit included; never adds a unit that
                    // was not selected.
                    if (!selectionChanged || incomingSelection == null) return;
                    SelectionFixture.Restore(incomingSelection, incomingCurrent);
                }),
                new KeyValuePair<string, Action>("summons", () =>
                {
                    foreach (UnitEntityData unit in caster == null ? new UnitEntityData[0] : Summons()) unit.Destroy();
                }),
                new KeyValuePair<string, Action>("party", () =>
                {
                    if (!addedToParty || caster == null) return;
                    Game.Instance.Player.PartyCharacters.RemoveAll(r => r.Value == caster);
                    Game.Instance.Player.InvalidateCharacterLists();
                    Game.Instance.Player.UpdateCharacterLists();
                    addedToParty = false;
                }),
                new KeyValuePair<string, Action>("caster", () =>
                {
                    if (caster == null) return;
                    if (caster.IsInState) caster.Destroy(); else caster.Dispose();
                    Game.Instance.EntityDestroyer.Tick();
                }),
                new KeyValuePair<string, Action>("pause", () => { if (pauseChanged) Game.Instance.IsPaused = pausedBefore; }),
                new KeyValuePair<string, Action>("blueprint", () => { if (clone != null) UnityEngine.Object.Destroy(clone); }),
                new KeyValuePair<string, Action>("verify", () =>
                {
                    if (Game.Instance.State.Units.Any(u => u != null && u.Blueprint != null && u.Blueprint.name == "KmgLoaderCompat_NewSorcerer" && u.IsInState))
                        throw new InvalidOperationException("the new sorcerer is still in the area");
                    if (Game.Instance.Player.PartyCharacters.Any(r => r.Value == caster)) throw new InvalidOperationException("the new sorcerer is still in the party");
                    // Whatever path ran (success, failure, refusal, timeout), the selection must be the incoming one.
                    if (incomingSelection != null)
                    {
                        UnitEntityData[] selectedNow = SelectionManager.Instance.SelectedUnits.ToArray();
                        UnitEntityData currentNow = Kingmaker.UI.Group.GroupController.Instance?.GetCurrentCharacter();
                        Record["outgoingSelection"] = SelectionFixture.Describe(selectedNow, currentNow);
                        string mismatch = SelectionFixture.Mismatch(incomingSelection, incomingCurrent, selectedNow, currentNow);
                        if (mismatch != null) throw new InvalidOperationException("the selection is not back to its incoming state: " + mismatch);
                    }

                    // The precondition was an idle UI (or the probe refused before acquiring anything): it must be idle again.
                    if (Record["incomingUi"] != null && (Record["incomingUi"]["targeting"].Type == JTokenType.String &&
                        !string.IsNullOrEmpty((string)Record["incomingUi"]["targeting"]))) return;
                    string targeting = Game.Instance.SelectedAbilityHandler?.Ability?.Blueprint?.name;
                    string[] open = (ActionBarManager.Instance?.Group?.GroupElements ?? new List<ActionBarGroupElement>())
                        .Where(e => e != null && e.ToggleState).Select(e => e.SlotType.ToString()).ToArray();
                    Record["outgoingUi"] = new JObject { ["targeting"] = targeting, ["openGroups"] = new JArray(open.Cast<object>().ToArray()) };
                    bool incomingIdle = Record["incomingUi"] != null && string.IsNullOrEmpty((string)Record["incomingUi"]["targeting"]) &&
                        !((JArray)Record["incomingUi"]["openGroups"]).Any();
                    if (incomingIdle && (targeting != null || open.Length > 0))
                        throw new InvalidOperationException("the UI is not back to its idle incoming state (targeting " + (targeting ?? "none") + ", open groups " + string.Join(",", open) + ")");
                })
            };
            if (failRelease != null)
                releases.Add(new KeyValuePair<string, Action>("injected-" + failRelease,
                    () => throw new InvalidOperationException("test-only injected release failure after " + failRelease)));
            CleanupReport report = ProbeCleanup.RunAll(releases);
            Record["releaseReason"] = testFailure ?? string.Empty;
            Record["releaseAttempted"] = new JArray(report.Attempted.Cast<object>().ToArray());
            Record["releaseFailures"] = new JArray(report.Failures.Cast<object>().ToArray());
            Record["partyAfterRelease"] = Game.Instance.Player.PartyCharacters.Count;
            return report;
        }
    }
}
