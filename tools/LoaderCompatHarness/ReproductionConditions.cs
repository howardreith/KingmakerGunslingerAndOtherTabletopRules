using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using Newtonsoft.Json.Linq;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Controlled, test-only conditions applied before the product initializes its blueprints. Each one stands in for
    /// something another mod or a translation could do to a real installation; none of them is claimed to be what a
    /// particular reporter's installation did. They exist so one session can fail on unrepaired bytes and pass on
    /// repaired bytes under the same condition.
    /// </summary>
    internal static class ReproductionConditions
    {
        /// <summary>An unassigned (null) LocalizedString resolves to an empty string instead of Kingmaker's "&lt;null&gt;".</summary>
        public const string UnassignedTextResolvesBlank = "unassignedTextResolvesBlank";

        /// <summary>
        /// An unassigned (null) LocalizedString resolves to a null string. Kingmaker's own ability description getter
        /// passes that straight to UIUtilityTexts.GetLongOrShortText, which dereferences it.
        /// </summary>
        public const string UnassignedTextResolvesNull = "unassignedTextResolvesNull";

        /// <summary>Records which project ability descriptions the product's grit UI pass reads and which component arrays it replaces.</summary>
        public const string ObserveGritUiPass = "observeGritUiPass";

        /// <summary>Abilities (guid list) whose description getter throws while the product's grit UI pass runs. Implies observeGritUiPass.</summary>
        public const string AbilityDescriptionThrowsAtGritUi = "abilityDescriptionThrowsAtGritUi";

        /// <summary>Abilities (guid list) whose description reads "Spend 1 grit ..." while the product's grit UI pass runs. Implies observeGritUiPass.</summary>
        public const string AbilityDescriptionMentionsGritSpend = "abilityDescriptionMentionsGritSpend";

        /// <summary>Unit facts (guid list) whose description is replaced by an empty-key LocalizedString before the product runs.</summary>
        public const string BlankOriginalDescription = "blankOriginalDescription";

        /// <summary>Unit facts (guid list) whose description another party replaces right after the product's protection publication.</summary>
        public const string ForeignDescriptionAfterProtectionPublish = "foreignDescriptionAfterProtectionPublish";

        /// <summary>Unit facts (guid list) whose component array another party replaces right after the product's protection publication.</summary>
        public const string ForeignComponentsAfterProtectionPublish = "foreignComponentsAfterProtectionPublish";

        /// <summary>Unit facts (guid list) whose description and component array identities are recorded before the product runs.</summary>
        public const string CaptureOriginals = "captureOriginals";

        /// <summary>The product's archetype presentation step throws, so the bootstrap's failure cleanup runs.</summary>
        public const string FailAtArchetypePresentation = "failAtArchetypePresentation";

        /// <summary>
        /// Unit facts (guid list) for which the product's validating description setter throws after it has stored
        /// the new description, as a failure between the write and the setter's return would.
        /// </summary>
        public const string FailProtectionDescriptionWrite = "failProtectionDescriptionWrite";

        public const string InjectedFailureMessage = "KmgLoaderCompatHarness injected initialization failure";
        public const string InjectedWriteFailureMessage = "KmgLoaderCompatHarness injected description write failure";

        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly FieldInfo DescriptionField = typeof(BlueprintUnitFact).GetField("m_Description", Any);

        private static string[] blankGuids = new string[0];
        private static string[] foreignDescriptionGuids = new string[0];
        private static string[] foreignComponentGuids = new string[0];
        private static string[] captureGuids = new string[0];
        private static string[] failingWriteGuids = new string[0];
        private static LibraryScriptableObject library;
        private static bool libraryPrepared;

        public static readonly Dictionary<string, LocalizedString> OriginalDescriptions = new Dictionary<string, LocalizedString>();
        public static readonly Dictionary<string, BlueprintComponent[]> OriginalComponents = new Dictionary<string, BlueprintComponent[]>();
        public static readonly Dictionary<string, LocalizedString> BlankDescriptions = new Dictionary<string, LocalizedString>();
        public static readonly Dictionary<string, LocalizedString> ForeignDescriptions = new Dictionary<string, LocalizedString>();
        public static readonly Dictionary<string, BlueprintComponent[]> ForeignComponents = new Dictionary<string, BlueprintComponent[]>();
        public static readonly List<string> Events = new List<string>();

        /// <summary>Each recorded fact's description as the installation had it, before any condition touched it.</summary>
        public static readonly JArray NativeDescriptions = new JArray();

        public static int BlankResolutions { get; private set; }

        public static int NullResolutions { get; private set; }

        /// <summary>The library the game loaded, once its dictionary is ready; null before that or when no condition asked for it.</summary>
        public static LibraryScriptableObject Library => library;

        /// <summary>Every fact a session asked to have recorded before the product ran.</summary>
        public static string[] RequestedCaptures => captureGuids;

        public static JObject Apply(HarmonyInstance harmony, IDictionary<string, string> conditions)
        {
            JObject applied = new JObject();
            if (conditions == null || conditions.Count == 0) return applied;
            if (DescriptionField == null) throw new MissingFieldException(typeof(BlueprintUnitFact).FullName, "m_Description");
            string[] known =
            {
                UnassignedTextResolvesBlank, BlankOriginalDescription, ForeignDescriptionAfterProtectionPublish,
                ForeignComponentsAfterProtectionPublish, CaptureOriginals, FailAtArchetypePresentation, FailProtectionDescriptionWrite,
                UnassignedTextResolvesNull, ObserveGritUiPass, AbilityDescriptionThrowsAtGritUi, AbilityDescriptionMentionsGritSpend
            };
            foreach (string name in conditions.Keys)
            {
                if (!known.Contains(name) && !SpearRepairConditions.Known.Contains(name) && !WeightConditions.Known.Contains(name) && name != ControlledRageObserver.Condition &&
                    name != BrownFurObserver.Condition && name != HoldLifecycleObserver.Condition)
                    throw new InvalidOperationException("unknown harness condition " + name);
            }

            if (Enabled(conditions, ControlledRageObserver.Condition))
            {
                // Installed after every mod's Load (HarnessMain.OnUpdate), when the product type is resolvable.
                ControlledRageObserver.Request();
                applied[ControlledRageObserver.Condition] = "record UnitPartControlledRageSelection PreSave/PostLoad and Urban Barbarian exceptions";
            }

            if (Enabled(conditions, BrownFurObserver.Condition))
            {
                BrownFurObserver.Request();
                applied[BrownFurObserver.Condition] = "record UnitPartBrownFurModifierPersistence PreSave/PostLoad/ResolveIncrease order and Brown-Fur exceptions";
            }

            if (Enabled(conditions, HoldLifecycleObserver.Condition))
            {
                HoldLifecycleObserver.Request();
                applied[HoldLifecycleObserver.Condition] = "record summon hold lifecycle callbacks (unload/arrival/turn-off/deactivate/release) read-only";
            }

            SpearRepairConditions.Apply(harmony, conditions, applied);
            WeightConditions.Apply(harmony, conditions, applied);

            if (Enabled(conditions, UnassignedTextResolvesBlank))
            {
                MethodInfo conversion = typeof(LocalizedString).GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public, null,
                    new[] { typeof(LocalizedString) }, null);
                if (conversion == null || conversion.ReturnType != typeof(string)) throw new MissingMethodException(typeof(LocalizedString).FullName, "op_Implicit");
                harmony.Patch(conversion, null, new HarmonyMethod(typeof(ReproductionConditions).GetMethod(nameof(BlankUnassignedPostfix), Any)));
                applied[UnassignedTextResolvesBlank] = "postfix on LocalizedString.op_Implicit: a null string resolves to \"\"";
            }

            if (Enabled(conditions, UnassignedTextResolvesNull))
            {
                if (Enabled(conditions, UnassignedTextResolvesBlank)) throw new InvalidOperationException("unassigned text cannot resolve both blank and null");
                MethodInfo conversion = typeof(LocalizedString).GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public, null,
                    new[] { typeof(LocalizedString) }, null);
                if (conversion == null || conversion.ReturnType != typeof(string)) throw new MissingMethodException(typeof(LocalizedString).FullName, "op_Implicit");
                harmony.Patch(conversion, null, new HarmonyMethod(typeof(ReproductionConditions).GetMethod(nameof(NullUnassignedPostfix), Any)));
                applied[UnassignedTextResolvesNull] = "postfix on LocalizedString.op_Implicit: a null string resolves to null";
            }

            blankGuids = List(conditions, BlankOriginalDescription);
            foreignDescriptionGuids = List(conditions, ForeignDescriptionAfterProtectionPublish);
            foreignComponentGuids = List(conditions, ForeignComponentsAfterProtectionPublish);
            captureGuids = List(conditions, CaptureOriginals).Concat(blankGuids).Concat(foreignDescriptionGuids).Concat(foreignComponentGuids)
                .Distinct().ToArray();
            string[] throwingDescriptions = List(conditions, AbilityDescriptionThrowsAtGritUi);
            string[] gritSpendDescriptions = List(conditions, AbilityDescriptionMentionsGritSpend);
            bool observeGritUi = Enabled(conditions, ObserveGritUiPass) || throwingDescriptions.Length + gritSpendDescriptions.Length > 0;
            if (captureGuids.Length > 0 || observeGritUi || SpearRepairConditions.NeedsLibraryHook)
            {
                MethodInfo load = typeof(LibraryScriptableObject).GetMethod("LoadDictionary", Any);
                if (load == null) throw new MissingMethodException(typeof(LibraryScriptableObject).FullName, "LoadDictionary");
                harmony.Patch(load, null, new HarmonyMethod(typeof(ReproductionConditions).GetMethod(nameof(BeforeProductPostfix), Any)));
                applied[CaptureOriginals] = new JArray(captureGuids.Cast<object>().ToArray());
                applied[BlankOriginalDescription] = new JArray(blankGuids.Cast<object>().ToArray());
            }

            Assembly product = ProductProbes.Entry().Assembly;
            if (product == null) throw new InvalidOperationException("the product assembly is not loaded before the harness");
            if (observeGritUi) applied[ObserveGritUiPass] = GritUiPass.Install(harmony, product, throwingDescriptions, gritSpendDescriptions);
            if (foreignDescriptionGuids.Length + foreignComponentGuids.Length > 0)
            {
                Type publication = product.GetType("KingmakerGunslinger.Spells.ProtectionFromAlignment.ProtectionFromAlignmentPublication", true);
                MethodInfo publish = publication.GetMethod("Publish", Any);
                if (publish == null) throw new MissingMethodException(publication.FullName, "Publish");
                harmony.Patch(publish, null, new HarmonyMethod(typeof(ReproductionConditions).GetMethod(nameof(AfterProtectionPublishPostfix), Any)));
                applied[ForeignDescriptionAfterProtectionPublish] = new JArray(foreignDescriptionGuids.Cast<object>().ToArray());
                applied[ForeignComponentsAfterProtectionPublish] = new JArray(foreignComponentGuids.Cast<object>().ToArray());
            }

            failingWriteGuids = List(conditions, FailProtectionDescriptionWrite);
            if (failingWriteGuids.Length > 0)
            {
                Type access = product.GetType("KingmakerGunslinger.Blueprints.BlueprintUnitFactAccess", true);
                MethodInfo setter = access.GetMethod("SetDescription", Any, null, new[] { typeof(BlueprintUnitFact), typeof(LocalizedString) }, null);
                if (setter == null) throw new MissingMethodException(access.FullName, "SetDescription");
                harmony.Patch(setter, null, new HarmonyMethod(typeof(ReproductionConditions).GetMethod(nameof(FailingWritePostfix), Any)));
                applied[FailProtectionDescriptionWrite] = new JArray(failingWriteGuids.Cast<object>().ToArray());
            }

            if (Enabled(conditions, FailAtArchetypePresentation))
            {
                Type presentation = product.GetType("KingmakerGunslinger.Blueprints.PlayerFacingPresentation", true);
                MethodInfo archetypes = presentation.GetMethod("ApplyArchetypes", Any);
                if (archetypes == null) throw new MissingMethodException(presentation.FullName, "ApplyArchetypes");
                harmony.Patch(archetypes, new HarmonyMethod(typeof(ReproductionConditions).GetMethod(nameof(InjectedFailurePrefix), Any)), null);
                applied[FailAtArchetypePresentation] = "prefix on PlayerFacingPresentation.ApplyArchetypes throws";
            }

            return applied;
        }

        public static JObject Describe()
        {
            return new JObject
            {
                ["blankResolutions"] = BlankResolutions,
                ["nullResolutions"] = NullResolutions,
                ["gritUiPass"] = GritUiPass.Describe(),
                ["libraryPrepared"] = libraryPrepared,
                ["nativeDescriptions"] = NativeDescriptions,
                ["events"] = new JArray(Events.Cast<object>().ToArray())
            };
        }

        public static LocalizedString CurrentDescription(BlueprintUnitFact fact)
        {
            return (LocalizedString)DescriptionField.GetValue(fact);
        }

        private static bool Enabled(IDictionary<string, string> conditions, string name)
        {
            return conditions.TryGetValue(name, out string value) && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
        }

        private static string[] List(IDictionary<string, string> conditions, string name)
        {
            return conditions.TryGetValue(name, out string value) && value != null
                ? value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim()).ToArray()
                : new string[0];
        }

        private static void BlankUnassignedPostfix(LocalizedString __0, ref string __result)
        {
            if (__0 != null) return;
            __result = string.Empty;
            BlankResolutions++;
        }

        private static void NullUnassignedPostfix(LocalizedString __0, ref string __result)
        {
            if (__0 != null) return;
            __result = null;
            NullResolutions++;
        }

        // Runs before the product's own LoadDictionary postfix (priority 400), so the product sees these values as the
        // installation's original state.
        [HarmonyPriority(Priority.First)]
        private static void BeforeProductPostfix(LibraryScriptableObject __instance)
        {
            if (libraryPrepared || __instance == null || __instance.BlueprintsByAssetId == null) return;
            libraryPrepared = true;
            library = __instance;
            SpearRepairConditions.BeforeProduct(__instance);
            foreach (string guid in captureGuids)
            {
                BlueprintUnitFact fact = Fact(guid);
                if (fact == null)
                {
                    Events.Add("missing before product: " + guid);
                    continue;
                }

                LocalizedString native = CurrentDescription(fact);
                string nativeText = fact.Description;
                NativeDescriptions.Add(new JObject
                {
                    ["guid"] = guid,
                    ["name"] = fact.name,
                    ["type"] = fact.GetType().Name,
                    ["stored"] = native != null,
                    ["key"] = native == null ? null : native.Key,
                    ["shared"] = native != null && native.Shared != null,
                    ["resolvesBlank"] = string.IsNullOrWhiteSpace(nativeText),
                    ["resolvedLength"] = nativeText == null ? -1 : nativeText.Length
                });
                if (blankGuids.Contains(guid))
                {
                    LocalizedString blank = new LocalizedString();
                    DescriptionField.SetValue(fact, blank);
                    BlankDescriptions[guid] = blank;
                    Events.Add("blank original description: " + fact.name);
                }

                OriginalDescriptions[guid] = CurrentDescription(fact);
                OriginalComponents[guid] = fact.ComponentsArray;
            }
        }

        private static void AfterProtectionPublishPostfix()
        {
            foreach (string guid in foreignDescriptionGuids)
            {
                BlueprintUnitFact fact = Fact(guid);
                if (fact == null) continue;
                LocalizedString foreign = new LocalizedString();
                typeof(LocalizedString).GetField("m_Key", Any).SetValue(foreign, "KmgLoaderCompatHarness.ForeignDescription." + guid);
                DescriptionField.SetValue(fact, foreign);
                ForeignDescriptions[guid] = foreign;
                Events.Add("foreign description after publication: " + fact.name);
            }

            foreach (string guid in foreignComponentGuids)
            {
                BlueprintUnitFact fact = Fact(guid);
                if (fact == null) continue;
                BlueprintComponent[] foreign = (fact.ComponentsArray ?? new BlueprintComponent[0]).ToArray();
                fact.ComponentsArray = foreign;
                ForeignComponents[guid] = foreign;
                Events.Add("foreign component array after publication: " + fact.name);
            }
        }

        // A postfix runs after the setter's body: the field is already stored when this throws.
        private static void FailingWritePostfix(BlueprintUnitFact __0, LocalizedString __1)
        {
            if (__0 == null || !failingWriteGuids.Contains(__0.AssetGuid)) return;
            if (!ReferenceEquals(CurrentDescription(__0), __1)) return;
            Events.Add("description write failure thrown after the write: " + __0.name);
            throw new InvalidOperationException(InjectedWriteFailureMessage);
        }

        private static bool InjectedFailurePrefix()
        {
            Events.Add("injected failure thrown");
            throw new InvalidOperationException(InjectedFailureMessage);
        }

        public static BlueprintUnitFact Fact(string guid)
        {
            if (library == null || library.BlueprintsByAssetId == null) return ResourcesLibrary.TryGetBlueprint(guid) as BlueprintUnitFact;
            return library.BlueprintsByAssetId.TryGetValue(guid, out BlueprintScriptableObject blueprint) ? blueprint as BlueprintUnitFact : null;
        }
    }
}
