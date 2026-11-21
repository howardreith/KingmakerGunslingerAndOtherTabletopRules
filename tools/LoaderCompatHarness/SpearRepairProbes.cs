using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
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
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Test-only conditions for the spear startup repair. Each stands in for something another mod could do before
    /// the product initializes; none is claimed to be what the reporter's installation did.
    /// </summary>
    internal static class SpearRepairConditions
    {
        /// <summary>"field=value;..." written onto the live native Longspear type before the product runs (simulated donor mutation).</summary>
        public const string MutateNativeLongspear = "mutateNativeLongspear";

        /// <summary>Weapon types (guid list) whose serialized profile is recorded before the product runs.</summary>
        public const string CaptureWeaponTypes = "captureWeaponTypes";

        /// <summary>"field=value;..." written onto the product's owned spear clone each time the product validates it (invalid owned profile).</summary>
        public const string CorruptOwnedSpearProfile = "corruptOwnedSpearProfile";

        /// <summary>BlueprintWeaponType getters (property names) given an identity postfix owned by the harness.</summary>
        public const string PatchWeaponTypeGetters = "patchWeaponTypeGetters";

        /// <summary>A harness postfix on the shared BlueprintWeaponType.AttackRange getter adds 1 ft for the Longspear family.</summary>
        public const string ExternalAttackRangePatch = "externalAttackRangePatch";

        public const string NativeLongspearTypeGuid = "fa2dd17cbde7d3f4aa918d467c30516e";
        public const string HarnessId = "KmgLoaderCompatHarness";

        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static Dictionary<string, string> longspearMutations = new Dictionary<string, string>();
        private static Dictionary<string, string> ownedCorruptions = new Dictionary<string, string>();
        private static string[] captureGuids = new string[0];

        public static readonly string[] Known = { MutateNativeLongspear, CaptureWeaponTypes, CorruptOwnedSpearProfile, PatchWeaponTypeGetters, ExternalAttackRangePatch };
        public static readonly List<string> Events = new List<string>();
        public static readonly Dictionary<string, JObject> Captured = new Dictionary<string, JObject>();
        public static HarmonyInstance Harmony { get; private set; }
        public static int CorruptionsApplied { get; private set; }

        public static bool NeedsLibraryHook => longspearMutations.Count > 0 || captureGuids.Length > 0;

        public static void Apply(HarmonyInstance harmony, IDictionary<string, string> conditions, JObject applied)
        {
            Harmony = harmony;
            longspearMutations = Pairs(conditions, MutateNativeLongspear);
            ownedCorruptions = Pairs(conditions, CorruptOwnedSpearProfile);
            captureGuids = (conditions.TryGetValue(CaptureWeaponTypes, out string captured) && captured != null
                ? captured.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()) : Enumerable.Empty<string>())
                .Concat(longspearMutations.Count > 0 ? new[] { NativeLongspearTypeGuid } : new string[0]).Distinct().ToArray();
            if (longspearMutations.Count > 0) applied[MutateNativeLongspear] = ToJson(longspearMutations);
            if (captureGuids.Length > 0) applied[CaptureWeaponTypes] = new JArray(captureGuids.Cast<object>().ToArray());

            if (ownedCorruptions.Count > 0)
            {
                Type access = ProductProbes.Entry().Assembly.GetType("KingmakerGunslinger.Blueprints.SpearWeaponTypeAccess", true);
                MethodInfo[] validators = access.GetMethods(Any).Where(m => m.Name == "Validate" && m.GetParameters().Length >= 1 &&
                    m.GetParameters()[0].ParameterType == typeof(BlueprintWeaponType)).ToArray();
                if (validators.Length == 0) throw new MissingMethodException(access.FullName, "Validate(BlueprintWeaponType,...)");
                foreach (MethodInfo validator in validators)
                    harmony.Patch(validator, new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(CorruptPrefix), Any)), null);
                applied[CorruptOwnedSpearProfile] = ToJson(ownedCorruptions);
            }

            if (conditions.TryGetValue(PatchWeaponTypeGetters, out string getters) && !string.IsNullOrWhiteSpace(getters))
            {
                var names = getters.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToArray();
                foreach (string name in names)
                {
                    MethodInfo getter = typeof(BlueprintWeaponType).GetProperty(name, BindingFlags.Instance | BindingFlags.Public)?.GetGetMethod();
                    if (getter == null) throw new MissingMethodException(typeof(BlueprintWeaponType).FullName, "get_" + name);
                    harmony.Patch(getter, null, new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(IdentityPostfix), Any)));
                }

                applied[PatchWeaponTypeGetters] = new JArray(names.Cast<object>().ToArray());
            }

            if (conditions.TryGetValue(ExternalAttackRangePatch, out string range) && !string.Equals(range, "false", StringComparison.OrdinalIgnoreCase))
            {
                MethodInfo getter = typeof(BlueprintWeaponType).GetProperty("AttackRange").GetGetMethod();
                harmony.Patch(getter, null, new HarmonyMethod(typeof(SpearRepairConditions).GetMethod(nameof(AttackRangePlusOnePostfix), Any)));
                applied[ExternalAttackRangePatch] = "postfix on BlueprintWeaponType.get_AttackRange adds 1 ft for the Longspear and Elven Branched Spear types";
            }
        }

        /// <summary>Runs from the harness LoadDictionary postfix, which is ordered before the product's own.</summary>
        public static void BeforeProduct(LibraryScriptableObject library)
        {
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
        }

        private static BlueprintWeaponType Type(LibraryScriptableObject library, string guid)
        {
            return library != null && library.BlueprintsByAssetId != null && library.BlueprintsByAssetId.TryGetValue(guid, out BlueprintScriptableObject value)
                ? value as BlueprintWeaponType : null;
        }

        private static void CorruptPrefix(BlueprintWeaponType __0)
        {
            if (__0 == null || __0.name != WeaponTypeProfiles.OwnedSpearTypeName) return;
            foreach (KeyValuePair<string, string> pair in ownedCorruptions) WeaponTypeProfiles.Write(__0, pair.Key, pair.Value);
            CorruptionsApplied++;
        }

        private static void IdentityPostfix()
        {
        }

        // A reach-extension stand-in on the shared getter, limited to the Longspear family so that unrelated
        // weapons (whose own validations read the same getter) keep their native range.
        private static void AttackRangePlusOnePostfix(BlueprintWeaponType __instance, ref Feet __result)
        {
            if (__instance == null || (__instance.AssetGuid != NativeLongspearTypeGuid && __instance.name != WeaponTypeProfiles.OwnedSpearTypeName)) return;
            __result = new Feet(__result.Value + 1);
        }

        private static Dictionary<string, string> Pairs(IDictionary<string, string> conditions, string name)
        {
            var result = new Dictionary<string, string>();
            if (!conditions.TryGetValue(name, out string value) || string.IsNullOrWhiteSpace(value)) return result;
            foreach (string pair in value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = pair.Split(new[] { '=' }, 2);
                if (parts.Length != 2) throw new InvalidOperationException(name + " needs field=value pairs: " + value);
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
                ["events"] = new JArray(Events.Cast<object>().ToArray()), ["corruptionsApplied"] = CorruptionsApplied,
                ["captured"] = captured
            };
        }

        // Built by hand: another mod may change Newtonsoft's process-wide defaults (dictionaries as arrays).
        private static JObject ToJson(Dictionary<string, string> values)
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
                try { result[name] = FieldText(typeof(BlueprintWeaponType).GetProperty(name).GetValue(type, null)); }
                catch (Exception exception) { result[name] = "<throws " + exception.GetType().Name + ">"; }
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
        /// kmgWeaponTypeProfile. match: "owned" (the owned spear exists with exactly the intended profile), "absent"
        /// (no owned spear: a rolled-back startup) or empty (record only). type: weapon types that must still hold
        /// the profile captured before the product ran. expect: the old-release predicate failures the native
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
                result["ownedSpear"] = new JObject { ["fields"] = fields, ["getters"] = Getters(owned), ["guid"] = owned.AssetGuid };
                if (match == "owned")
                {
                    foreach (KeyValuePair<string, string> pair in Intended)
                        if ((string)fields[pair.Key] != pair.Value) problems.Add("owned spear " + pair.Key + " is " + fields[pair.Key] + ", expected " + pair.Value);
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
    /// kmgItemPricing. Creates ordinary weapons (type: item guids) and queries Kingmaker's sale price and the item
    /// tooltip fill exactly as the inventory tooltip does (UIUtilityItem.FillTooltipData calls
    /// VendorLogic.GetItemSellPrice). expect "native": nothing throws and every price is the native
    /// (long)(Cost * Vendors.SellModifier); expect "throws": every query throws with text containing each "contains".
    /// match "battered": a production pistol bound as a battered firearm to the main character must sell for the
    /// product's fixed scrap value. invariants.repeat: queries per item (default 20).
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
            try
            {
                Patches patches = (SpearRepairConditions.Harmony ?? HarmonyInstance.Create(SpearRepairConditions.HarnessId + ".reader"))
                    .GetPatchInfo(typeof(VendorLogic).GetMethod("GetItemSellPrice", new[] { typeof(ItemEntity) }));
                result["sellPricePatchOwners"] = patches == null ? "none" :
                    string.Join(",", patches.Postfixes.Concat(patches.Prefixes).Select(p => p.owner).Distinct().ToArray());
            }
            catch (Exception exception) { result["sellPricePatchOwners"] = "unavailable:" + exception.GetType().Name; }

            var items = new JArray();
            foreach (string guid in itemGuids)
            {
                BlueprintItemWeapon blueprint = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(guid);
                if (blueprint == null) { problems.Add("item " + guid + " does not resolve"); continue; }
                var item = new ItemEntityWeapon(blueprint);
                JObject row = Query(item, repeat, (long)(item.Cost * modifier));
                row["guid"] = guid;
                items.Add(row);
                Judge(row, expect, contains, problems);
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

        private static JObject Query(ItemEntityWeapon item, int repeat, long native)
        {
            var prices = new HashSet<long>();
            var tooltipPrices = new HashSet<string>();
            int priceThrows = 0, tooltipThrows = 0;
            string firstPriceError = null, firstTooltipError = null;
            for (int index = 0; index < repeat; index++)
            {
                try { prices.Add(Game.Instance.Vendor.GetItemSellPrice(item)); }
                catch (Exception exception) { priceThrows++; firstPriceError = firstPriceError ?? exception.GetType().Name + ": " + exception.Message; }

                try
                {
                    TooltipData data = UIUtilityItem.FillTooltipData(item, new TooltipData());
                    tooltipPrices.Add(data.Texts.TryGetValue(TooltipElement.SellPrice, out string sell) ? sell : "<missing>");
                }
                catch (Exception exception) { tooltipThrows++; firstTooltipError = firstTooltipError ?? exception.GetType().Name + ": " + exception.Message; }
            }

            return new JObject
            {
                ["name"] = item.Blueprint.name, ["cost"] = item.Cost, ["nativeSellPrice"] = native,
                ["sellPrices"] = new JArray(prices.Cast<object>().ToArray()), ["tooltipSellPrices"] = new JArray(tooltipPrices.Cast<object>().ToArray()),
                ["priceThrows"] = priceThrows, ["tooltipThrows"] = tooltipThrows, ["firstPriceError"] = firstPriceError, ["firstTooltipError"] = firstTooltipError
            };
        }

        private static void Judge(JObject row, string expect, IList<string> contains, List<string> problems)
        {
            string name = (string)row["name"];
            int repeat = ((JArray)row["sellPrices"]).Count;
            if (expect == "throws")
            {
                if ((int)row["priceThrows"] == 0 || (int)row["tooltipThrows"] == 0)
                    problems.Add(name + " did not throw: priceThrows=" + row["priceThrows"] + " tooltipThrows=" + row["tooltipThrows"]);
                foreach (string needle in contains)
                    if (((string)row["firstPriceError"] ?? string.Empty).IndexOf(needle, StringComparison.Ordinal) < 0)
                        problems.Add(name + " price error lacks '" + needle + "': " + row["firstPriceError"]);
                return;
            }

            if ((int)row["priceThrows"] != 0 || (int)row["tooltipThrows"] != 0)
                problems.Add(name + " threw: " + (row["firstPriceError"] ?? row["firstTooltipError"]));
            long native = (long)row["nativeSellPrice"];
            JArray prices = (JArray)row["sellPrices"];
            if (prices.Count != 1 || (long)prices[0] != native) problems.Add(name + " sell prices " + prices.ToString(Newtonsoft.Json.Formatting.None) + ", native " + native);
            JArray tooltip = (JArray)row["tooltipSellPrices"];
            if (tooltip.Count != 1 || (string)tooltip[0] != native.ToString(CultureInfo.InvariantCulture))
                problems.Add(name + " tooltip sell prices " + tooltip.ToString(Newtonsoft.Json.Formatting.None) + ", native " + native);
        }

        private static JObject Battered(int repeat, List<string> problems)
        {
            Assembly product = ProductProbes.Entry().Assembly;
            object catalog = product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true).GetProperty("ProductionFirearms", Any).GetValue(null, null);
            if (catalog == null) { problems.Add("no production firearm catalog for the battered check"); return null; }
            object pistolSet = catalog.GetType().GetProperty("Pistol", Any).GetValue(catalog, null);
            BlueprintItemWeapon pistol = (BlueprintItemWeapon)pistolSet.GetType().GetProperty("Item", Any).GetValue(pistolSet, null);
            object fixedValue = product.GetType("KingmakerGunslinger.Firearms.BatteredFirearmUsePolicy", true)
                .GetField("FixedExpectedScrapValueGold", Any).GetValue(null);
            long expected = Convert.ToInt64(fixedValue, CultureInfo.InvariantCulture);
            UnitEntityData owner = Game.Instance.Player.MainCharacter.Value;
            var item = new ItemEntityWeapon(pistol);
            var ordinary = new ItemEntityWeapon(pistol);
            product.GetType("KingmakerGunslinger.Gunsmithing.BatteredFirearmOriginRuntime", true).GetMethod("Bind", Any)
                .Invoke(null, new object[] { item, owner });
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

        private static JToken ProductBool(string typeName, string property)
        {
            Type type = ProductProbes.Entry().Assembly.GetType(typeName, false);
            PropertyInfo info = type?.GetProperty(property, Any);
            return info == null ? (JToken)"<absent>" : (bool)info.GetValue(null, null);
        }
    }

    /// <summary>
    /// kmgSorcererSummon. In the loaded save: a fresh disposable character takes Sorcerer level 1 through the native
    /// level-up controller in character-generation mode and learns Summon Monster I through the controller's own spell
    /// selection. It joins the party and is selected the way a portrait click selects; the action bar's spell group is
    /// opened through its own click handler, the Summon Monster I slot the game built is clicked, and the creature menu
    /// the game filled is read (names, icons, visibility). The chosen entry ("contains" picks by name; otherwise the
    /// first entry) is cast with the AbilityData that slot holds, and a summoned creature must appear. Everything the
    /// probe created is removed. expect "expanded": Summon Monster I must carry the product's published menu. Otherwise,
    /// when Summon Monster I is Kingmaker's own single spell (no variants, as after a failed product startup), the slot
    /// must not open a menu and the spell itself is cast.
    /// </summary>
    internal sealed class SorcererSummonProbe : ScreenProbe
    {
        private enum Stage { Start, Selected, GroupOpened, MenuOpened, MenuRead, Casting, Done }

        private const string SorcererGuid = "b3a505fb61437dc4097f43c3f8f9a4cf";
        private const string SummonMonsterOneGuid = "8fd74eddd9b6c224693d9ab241f25e84";
        private readonly string expect;
        private readonly string[] pick;
        private Stage stage = Stage.Start;
        private BlueprintUnit clone;
        private UnitEntityData caster;
        private bool addedToParty;
        private UnitEntityData originalSelection;
        private BlueprintAbility summonMonsterOne;
        private ActionBarSpellsGroup subGroup;
        private AbilityData chosen;
        private AbilityData sourceSpell;
        private UnitUseAbility command;
        private bool pausedBefore;
        private double castAt;
        private readonly List<UnitEntityData> summoned = new List<UnitEntityData>();

        public SorcererSummonProbe(string directory, string label, string expect, IEnumerable<string> pick) : base(directory, label)
        {
            this.expect = expect;
            this.pick = pick.ToArray();
        }

        protected override string Step()
        {
            try
            {
                string outcome = Advance();
                if (outcome != null) Cleanup();
                return outcome;
            }
            catch
            {
                Cleanup();
                throw;
            }
        }

        private string Advance()
        {
            switch (stage)
            {
                case Stage.Start:
                {
                    summonMonsterOne = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(SummonMonsterOneGuid);
                    BlueprintCharacterClass sorcerer = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>(SorcererGuid);
                    if (summonMonsterOne == null || sorcerer == null) return "Summon Monster I or the Sorcerer class does not resolve";
                    Record["summonMonsterI"] = Describe(summonMonsterOne);
                    Record["publishedExpandedParent"] = PublishedExpandedParent(summonMonsterOne);
                    SelectionManager selection = SelectionManager.Instance;
                    originalSelection = selection == null ? null : selection.GetSingleSelectedUnit();
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
                    string[] known = book.GetKnownSpells(1).Select(s => s.Blueprint.name).ToArray();
                    Record["knownLevelOne"] = new JArray(known.Cast<object>().ToArray());
                    Record["spellsPerDayLevelOne"] = book.GetSpellsPerDay(1);
                    if (!book.GetKnownSpells(1).Any(s => ReferenceEquals(s.Blueprint, summonMonsterOne)))
                        return "the new sorcerer did not learn Summon Monster I through level-up";
                    Game.Instance.Player.PartyCharacters.Add(caster);
                    addedToParty = true;
                    Game.Instance.Player.InvalidateCharacterLists();
                    Game.Instance.Player.UpdateCharacterLists();
                    SelectionManager.Instance.SelectUnit(caster.View, true, true, false);
                    stage = Stage.Selected;
                    Wait(12);
                    return null;
                }

                case Stage.Selected:
                {
                    ActionBarManager bar = ActionBarManager.Instance;
                    if (bar == null || bar.Group == null) return "the action bar is not available";
                    ActionBarGroupElement spells = bar.Group.GroupElements.FirstOrDefault(e => e != null && e.SlotType == ActionBarSlotType.Spell);
                    if (spells == null) return "the action bar has no spell group";
                    Record["selectedUnit"] = SelectionManager.Instance.GetSingleSelectedUnit()?.Blueprint?.name;
                    if (!spells.ToggleState) spells.OnClick();
                    stage = Stage.GroupOpened;
                    Wait(12);
                    return null;
                }

                case Stage.GroupOpened:
                {
                    ActionBarGroupElement spells = ActionBarManager.Instance.Group.GroupElements.First(e => e.SlotType == ActionBarSlotType.Spell);
                    Record["spellGroupOpen"] = spells.ToggleState;
                    List<ActionBarGroupSlot> slots = Field<List<ActionBarGroupSlot>>(spells, "m_Slots");
                    var seen = new JArray();
                    ActionBarGroupSlot target = null;
                    foreach (ActionBarGroupSlot slot in slots)
                    {
                        AbilityData spell = (Mechanic(slot) as MechanicActionBarSlotSpontaneusSpell)?.Spell;
                        if (spell == null) continue;
                        seen.Add(spell.Blueprint.name + (slot.gameObject.activeInHierarchy ? "" : " (hidden)"));
                        if (ReferenceEquals(spell.Blueprint, summonMonsterOne) && slot.gameObject.activeInHierarchy) target = slot;
                    }

                    Record["spellSlots"] = seen;
                    if (target == null) return "no visible Summon Monster I slot on the new sorcerer's spell bar";
                    sourceSpell = (Mechanic(target) as MechanicActionBarSlotSpontaneusSpell).Spell;
                    subGroup = Field<ActionBarSpellsGroup>(target, "SubGroup");
                    target.OnClick();
                    stage = Stage.MenuOpened;
                    Wait(10);
                    return null;
                }

                case Stage.MenuOpened:
                {
                    List<ActionBarSpontaneousConvertedSlot> entries = Field<List<ActionBarSpontaneousConvertedSlot>>(subGroup, "m_Slots");
                    var rows = new JArray();
                    foreach (ActionBarSpontaneousConvertedSlot entry in entries)
                    {
                        AbilityData data = (Mechanic(entry) as MechanicActionBarSlotSpontaneusConvertedSpell)?.Spell;
                        rows.Add(new JObject
                        {
                            ["name"] = data?.Name, ["blueprint"] = data?.Blueprint?.name, ["guid"] = data?.Blueprint?.AssetGuid,
                            ["icon"] = data?.Icon != null ? data.Icon.name : null, ["visible"] = entry.gameObject.activeInHierarchy,
                            ["nestedVariants"] = data?.Blueprint?.Variants?.Length ?? 0
                        });
                    }

                    bool menuOpen = subGroup.gameObject.activeInHierarchy;
                    Record["menuOpen"] = menuOpen;
                    Record["menuEntries"] = rows;
                    Record["menuEntryCount"] = rows.Count;
                    Capture("summon-monster-i-menu");
                    bool nativeSingle = summonMonsterOne.Variants == null || summonMonsterOne.Variants.Length == 0;
                    Record["nativeSingleSpell"] = nativeSingle;
                    if (nativeSingle && expect != "expanded")
                    {
                        // Kingmaker's own Summon Monster I has no variants: the slot does not expand, and its click
                        // only starts targeting. The spell itself is what this player casts.
                        if (menuOpen || rows.Count != 0) return "a native single Summon Monster I opened a menu with " + rows.Count + " entries";
                        chosen = sourceSpell;
                        Record["chosen"] = new JObject { ["name"] = chosen.Name, ["blueprint"] = chosen.Blueprint.name, ["available"] = chosen.IsAvailableForCast, ["route"] = "native-single-spell" };
                        stage = Stage.MenuRead;
                        return null;
                    }

                    if (!menuOpen) return "the Summon Monster I creature menu did not open";
                    if (rows.Count < 2) return "the Summon Monster I menu shows " + rows.Count + " creature entries";
                    JObject[] bad = rows.Cast<JObject>().Where(r => !(bool)r["visible"] || string.IsNullOrWhiteSpace((string)r["name"]) || r["icon"].Type == JTokenType.Null).ToArray();
                    if (bad.Length > 0) return "menu entries are not visible with a name and icon: " + string.Join(", ", bad.Select(b => b.ToString(Newtonsoft.Json.Formatting.None)).ToArray());
                    if (expect == "expanded" && !(bool)Record["publishedExpandedParent"]) return "Summon Monster I does not carry the product's published menu";
                    ActionBarSpontaneousConvertedSlot choice = entries.FirstOrDefault(e => Matches(e)) ?? (pick.Length == 0 ? entries.FirstOrDefault(e => NoNested(e)) : null);
                    if (choice == null) return "no menu entry matches " + string.Join(",", pick);
                    chosen = (Mechanic(choice) as MechanicActionBarSlotSpontaneusConvertedSpell).Spell;
                    Record["chosen"] = new JObject { ["name"] = chosen.Name, ["blueprint"] = chosen.Blueprint.name, ["available"] = chosen.IsAvailableForCast };
                    stage = Stage.MenuRead;
                    return null;
                }

                case Stage.MenuRead:
                {
                    if (AwaitingScreenshots()) return null;
                    subGroup.Hide(true);
                    pausedBefore = Game.Instance.IsPaused;
                    if (pausedBefore) Game.Instance.IsPaused = false;
                    Vector3 point = caster.Position + caster.OrientationDirection * 2.5f;
                    caster.Commands.InterruptAll(true);
                    command = new UnitUseAbility(chosen, new TargetWrapper(point));
                    caster.Commands.Run(command);
                    castAt = Time.realtimeSinceStartup;
                    stage = Stage.Casting;
                    return null;
                }

                case Stage.Casting:
                {
                    summoned.Clear();
                    summoned.AddRange(Game.Instance.State.Units.Where(u => u != null && u.Get<UnitPartSummonedMonster>()?.Summoner == caster));
                    bool finished = command.IsFinished;
                    if (summoned.Count == 0 || !finished)
                    {
                        if (Time.realtimeSinceStartup - castAt > 40) return "no summoned creature after 40 s (command result " + command.Result + ", finished " + finished + ")";
                        return null;
                    }

                    Record["commandResult"] = command.Result.ToString();
                    Record["summoned"] = new JArray(summoned.Select(u => (object)(u.Blueprint.name + "|" + u.CharacterName)).ToArray());
                    stage = Stage.Done;
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        private bool Matches(ActionBarSpontaneousConvertedSlot entry)
        {
            AbilityData data = (Mechanic(entry) as MechanicActionBarSlotSpontaneusConvertedSpell)?.Spell;
            return pick.Length > 0 && data != null && pick.Any(p => (data.Name ?? string.Empty).IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0) && NoNested(entry);
        }

        private static bool NoNested(ActionBarSpontaneousConvertedSlot entry)
        {
            AbilityData data = (Mechanic(entry) as MechanicActionBarSlotSpontaneusConvertedSpell)?.Spell;
            return data != null && (data.Blueprint.Variants == null || data.Blueprint.Variants.Length == 0);
        }

        private static MechanicActionBarSlot Mechanic(ActionBarSlot slot)
        {
            PropertyInfo property = typeof(ActionBarSlot).GetProperty("MechanicSlot", Any);
            return property?.GetValue(slot, null) as MechanicActionBarSlot;
        }

        private static void LevelSorcerer(UnitEntityData unit, BlueprintCharacterClass sorcerer, JObject record)
        {
            MethodInfo apply = typeof(LevelUpController).GetMethod("ApplyLevelup", Any);
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
            MethodInfo check = publisher?.GetMethod("IsPublishedExpandedParent", Any);
            return check != null && (bool)check.Invoke(null, new object[] { ability });
        }

        private static JObject Describe(BlueprintAbility ability)
        {
            return new JObject
            {
                ["name"] = ability.name, ["variants"] = ability.Variants?.Length ?? 0,
                ["variantNames"] = new JArray((ability.Variants ?? new BlueprintAbility[0]).Where(v => v != null).Select(v => (object)v.name).ToArray())
            };
        }

        private bool cleaned;

        private void Cleanup()
        {
            if (cleaned) return;
            cleaned = true;
            var failures = new JArray();
            Try(failures, "menu", () => { if (subGroup != null) subGroup.Hide(true); });
            Try(failures, "spell-group", () =>
            {
                ActionBarGroupElement spells = ActionBarManager.Instance?.Group?.GroupElements.FirstOrDefault(e => e != null && e.SlotType == ActionBarSlotType.Spell);
                if (spells != null && spells.ToggleState) spells.OnClick();
            });
            foreach (UnitEntityData unit in Game.Instance.State.Units.Where(u => u != null && caster != null && u.Get<UnitPartSummonedMonster>()?.Summoner == caster).ToArray())
                Try(failures, "summon " + unit.Blueprint.name, () => unit.Destroy());
            Try(failures, "party", () =>
            {
                if (addedToParty && caster != null)
                {
                    Game.Instance.Player.PartyCharacters.RemoveAll(r => r.Value == caster);
                    Game.Instance.Player.InvalidateCharacterLists();
                    Game.Instance.Player.UpdateCharacterLists();
                }
            });
            Try(failures, "caster", () => { if (caster != null) { if (caster.IsInState) caster.Destroy(); else caster.Dispose(); } });
            Try(failures, "destroyer", () => Game.Instance.EntityDestroyer.Tick());
            Try(failures, "selection", () =>
            {
                UnitEntityData back = originalSelection ?? Game.Instance.Player.MainCharacter.Value;
                if (back != null && back.View != null) SelectionManager.Instance.SelectUnit(back.View, true, true, false);
            });
            Try(failures, "pause", () => { if (pausedBefore) Game.Instance.IsPaused = true; });
            Try(failures, "blueprint", () => { if (clone != null) UnityEngine.Object.Destroy(clone); });
            Record["cleanupFailures"] = failures;
        }

        private static void Try(JArray failures, string what, Action action)
        {
            try { action(); }
            catch (Exception exception) { failures.Add(what + ": " + exception.GetType().Name + ": " + exception.Message); }
        }
    }
}
