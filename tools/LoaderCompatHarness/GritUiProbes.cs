using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Newtonsoft.Json.Linq;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Observes the product's grit UI pass (every method named Apply on GritAbilityUiIntegration) from outside: which
    /// project ability descriptions it reads, and which component arrays it replaces. Two controlled description
    /// conditions act only while that pass runs, so nothing else in the game sees them.
    /// </summary>
    internal static class GritUiPass
    {
        public const string InjectedDescriptionFailureMessage = "KmgLoaderCompatHarness injected ability description failure";
        public const string GritSpendWords = "Spend 1 grit to test the description heuristic.";

        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string ProjectPrefix = "KMG_";

        private static int depth;
        private static bool sealedForReading;
        private static string[] throwingGuids = new string[0];
        private static string[] mentioningGuids = new string[0];
        private static Dictionary<string, BlueprintComponent[]> before;
        private static Dictionary<string, BlueprintComponent[]> after;

        public static bool Installed { get; private set; }
        public static int Opened { get; private set; }
        public static int Completed { get; private set; }
        public static readonly Dictionary<string, int> DescriptionReads = new Dictionary<string, int>();
        public static readonly List<string> Thrown = new List<string>();
        private static BlueprintAbility lastRead;
        public static readonly List<string> Substituted = new List<string>();

        public static JToken Install(HarmonyInstance harmony, Assembly product, string[] throwing, string[] mentioning)
        {
            throwingGuids = throwing;
            mentioningGuids = mentioning;
            Type integration = product.GetType("KingmakerGunslinger.Grit.GritAbilityUiIntegration", true);
            MethodInfo[] passes = integration.GetMethods(Any).Where(method => method.Name == "Apply" && method.IsStatic).ToArray();
            if (passes.Length == 0) throw new MissingMethodException(integration.FullName, "Apply");
            foreach (MethodInfo pass in passes)
                harmony.Patch(pass, new HarmonyMethod(typeof(GritUiPass).GetMethod(nameof(PassPrefix), Any)),
                    new HarmonyMethod(typeof(GritUiPass).GetMethod(nameof(PassPostfix), Any)));
            MethodInfo description = typeof(BlueprintAbility).GetProperty("Description", BindingFlags.Instance | BindingFlags.Public).GetGetMethod();
            if (description == null || description.DeclaringType != typeof(BlueprintAbility))
                throw new MissingMethodException(typeof(BlueprintAbility).FullName, "get_Description");
            harmony.Patch(description, new HarmonyMethod(typeof(GritUiPass).GetMethod(nameof(DescriptionPrefix), Any)), null);
            Installed = true;
            return new JObject
            {
                ["passMethods"] = new JArray(passes.Select(pass => (object)pass.ToString()).ToArray()),
                ["throwingDescriptions"] = new JArray(throwing.Cast<object>().ToArray()),
                ["gritSpendDescriptions"] = new JArray(mentioning.Cast<object>().ToArray())
            };
        }

        public static bool? ChangedByFirstPass(string guid)
        {
            if (before == null || after == null) return null;
            before.TryGetValue(guid, out BlueprintComponent[] first);
            after.TryGetValue(guid, out BlueprintComponent[] second);
            return !ReferenceEquals(first, second);
        }

        /// <summary>Ends observation before a probe reads descriptions itself. A pass that threw is still open here.</summary>
        public static void Seal()
        {
            if (sealedForReading) return;
            sealedForReading = true;
            OpenWhenSealed = depth > 0;
        }

        public static bool OpenWhenSealed { get; private set; }

        public static JObject Describe()
        {
            Seal();
            // A pass that never completed stopped on the last description it asked for.
            JObject stoppedOn = null;
            if (Opened > 0 && Completed == 0 && lastRead != null)
                stoppedOn = new JObject
                {
                    ["name"] = lastRead.name,
                    ["guid"] = lastRead.AssetGuid,
                    ["hidden"] = lastRead.Hidden,
                    ["description"] = GritUiProbes.Description(lastRead)
                };
            return new JObject
            {
                ["installed"] = Installed,
                ["firstPassStillOpen"] = OpenWhenSealed,
                ["firstPassStoppedOnDescriptionOf"] = stoppedOn,
                ["passesOpened"] = Opened,
                ["passesCompleted"] = Completed,
                ["projectAbilityDescriptionReads"] = DescriptionReads.Values.Sum(),
                ["projectAbilitiesRead"] = DescriptionReads.Count,
                ["thrown"] = new JArray(Thrown.Cast<object>().ToArray()),
                ["substituted"] = new JArray(Substituted.Cast<object>().ToArray())
            };
        }

        public static Dictionary<string, BlueprintComponent[]> Snapshot()
        {
            Dictionary<string, BlueprintComponent[]> arrays = new Dictionary<string, BlueprintComponent[]>();
            foreach (BlueprintAbility ability in GritUiProbes.ProjectAbilities()) arrays[ability.AssetGuid] = ability.ComponentsArray;
            return arrays;
        }

        private static void PassPrefix()
        {
            if (depth++ != 0) return;
            Opened++;
            if (Opened == 1) before = Snapshot();
        }

        // Harmony 1.2 runs no postfix when the original throws: a failed pass stays open and is reported as such.
        private static void PassPostfix()
        {
            if (--depth != 0) return;
            Completed++;
            if (Completed == 1 && Opened == 1) after = Snapshot();
        }

        private static bool DescriptionPrefix(BlueprintAbility __instance, ref string __result)
        {
            if (depth <= 0 || Completed >= 1 || sealedForReading || __instance == null) return true;
            string name = __instance.name ?? string.Empty;
            if (name.StartsWith(ProjectPrefix, StringComparison.Ordinal))
            {
                lastRead = __instance;
                DescriptionReads.TryGetValue(name, out int reads);
                DescriptionReads[name] = reads + 1;
            }

            string guid = __instance.AssetGuid;
            if (throwingGuids.Contains(guid))
            {
                Thrown.Add(name);
                throw new InvalidOperationException(InjectedDescriptionFailureMessage + ": " + name);
            }

            if (mentioningGuids.Contains(guid))
            {
                if (!Substituted.Contains(name)) Substituted.Add(name);
                __result = GritSpendWords;
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// The shared grit UI contract as the running game holds it, read from engine state: which project abilities carry
    /// the shared indicator, which carry their own grit resource logic, and what each ability's description stores and
    /// resolves to. The expected identities come from the session file, not from the product.
    /// </summary>
    internal static class GritUiProbes
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string ProjectPrefix = "KMG_";
        private const string SharedComponentName = "$KMG_SharedGritUi";
        private const string SharedComponentType = "GritAbilityResourceUiLogic";
        private static readonly FieldInfo DescriptionField = typeof(BlueprintUnitFact).GetField("m_Description", Any);

        public static BlueprintAbility[] ProjectAbilities()
        {
            LibraryScriptableObject library = ReproductionConditions.Library ?? ResourcesLibrary.LibraryObject;
            if (library == null) return new BlueprintAbility[0];
            return library.GetAllBlueprints().OfType<BlueprintAbility>()
                .Where(ability => ability != null && ability.name != null && ability.name.StartsWith(ProjectPrefix, StringComparison.Ordinal))
                .OrderBy(ability => ability.name, StringComparer.Ordinal).ToArray();
        }

        public static JObject Census(string gritGuid, string[] expectedShared, string[] expectedOwn, out string problem)
        {
            problem = null;
            GritUiPass.Seal();
            List<string> problems = new List<string>();
            JObject census = new JObject { ["gritResource"] = gritGuid, ["pass"] = GritUiPass.Describe() };
            BlueprintAbilityResource grit = ResourcesLibrary.TryGetBlueprint(gritGuid) as BlueprintAbilityResource;
            if (grit == null)
            {
                census["gritResolves"] = false;
                problem = "the grit resource " + gritGuid + " does not resolve; no ability can satisfy the contract";
                return census;
            }

            census["gritResolves"] = true;
            JArray abilities = new JArray();
            List<string> shared = new List<string>(), own = new List<string>(), changed = new List<string>(), unstored = new List<string>();
            Dictionary<string, string> names = new Dictionary<string, string>();
            foreach (BlueprintAbility ability in ProjectAbilities())
            {
                names[ability.AssetGuid] = ability.name;
                BlueprintComponent[] components = ability.ComponentsArray ?? new BlueprintComponent[0];
                AbilityResourceLogic[] gritLogic = components.OfType<AbilityResourceLogic>()
                    .Where(logic => ReferenceEquals(logic.RequiredResource, grit)).ToArray();
                string contract;
                if (gritLogic.Length == 0) contract = "none";
                else if (gritLogic.Length == 1 && gritLogic[0].name == SharedComponentName)
                    contract = gritLogic[0].GetType().Name == SharedComponentType && gritLogic[0].IsSpendResource && gritLogic[0].Amount == 1 &&
                        !gritLogic[0].CostIsCustom ? "shared" : "invalid";
                else if (gritLogic.Length == 1) contract = "own";
                else contract = "invalid";
                if (contract == "shared") shared.Add(ability.AssetGuid);
                if (contract == "own") own.Add(ability.AssetGuid);
                if (contract == "invalid") problems.Add(ability.name + " (" + ability.AssetGuid + ") carries an invalid grit resource contract");
                bool? changedByPass = GritUiPass.ChangedByFirstPass(ability.AssetGuid);
                if (changedByPass == true) changed.Add(ability.AssetGuid);
                JObject description = Description(ability);
                if (!(bool)description["stored"]) unstored.Add(ability.name);
                abilities.Add(new JObject
                {
                    ["name"] = ability.name,
                    ["guid"] = ability.AssetGuid,
                    ["hidden"] = ability.Hidden,
                    ["contract"] = contract,
                    ["components"] = new JArray(components.Select(component => (object)(component == null ? "<null>" : component.GetType().Name)).ToArray()),
                    ["gritResourceLogic"] = new JArray(gritLogic.Select(logic => (object)(logic.GetType().Name + " name=" + logic.name + " spend=" +
                        logic.IsSpendResource + " amount=" + logic.Amount + " custom=" + logic.CostIsCustom)).ToArray()),
                    ["gritReferences"] = new JArray(components.Where(component => References(component, grit))
                        .Select(component => (object)component.GetType().Name).ToArray()),
                    ["changedByFirstPass"] = changedByPass,
                    ["description"] = description
                });
            }

            census["abilities"] = abilities;
            census["projectAbilities"] = abilities.Count;
            census["shared"] = Named(shared, names);
            census["own"] = Named(own, names);
            census["changedByFirstPass"] = Named(changed, names);
            census["unstoredDescriptions"] = new JArray(unstored.Cast<object>().ToArray());
            Compare("shared indicator", shared, expectedShared, names, problems);
            Compare("own grit resource logic", own, expectedOwn, names, problems);
            if (GritUiPass.Installed)
            {
                if (GritUiPass.Opened < 1 || GritUiPass.Completed < 1)
                    problems.Add("the grit UI pass opened " + GritUiPass.Opened + " times and completed " + GritUiPass.Completed);
                else Compare("arrays replaced by the first pass", changed, shared.ToArray(), names, problems);
            }

            if (problems.Count > 0) problem = string.Join("; ", problems.ToArray());
            return census;
        }

        /// <summary>What the description field stores and resolves to, without letting a throwing getter escape.</summary>
        public static JObject Description(BlueprintAbility ability)
        {
            LocalizedString stored = (LocalizedString)DescriptionField.GetValue(ability);
            string raw, displayed;
            try
            {
                string text = stored;
                raw = text == null ? "null" : text.Length == 0 ? "empty" : "nonempty";
            }
            catch (Exception exception) { raw = "throws " + exception.GetType().Name; }

            try
            {
                string text = ability.Description;
                displayed = text == null ? "null" : text.Length == 0 ? "empty" : "nonempty";
            }
            catch (Exception exception) { displayed = "throws " + exception.GetType().Name; }

            return new JObject
            {
                ["stored"] = stored != null,
                ["key"] = stored == null ? null : stored.Key,
                ["rawResolution"] = raw,
                ["displayed"] = displayed
            };
        }

        /// <summary>
        /// Calls the product's own integration a second time on the live blueprints and then feeds its recipient-level
        /// entry point malformed recipients. A repeat must succeed and replace no component array; each malformed case
        /// must be refused with the offender's identity and leave every real recipient untouched.
        /// </summary>
        public static JObject Reapply(string gritGuid, out string problem)
        {
            problem = null;
            List<string> problems = new List<string>();
            JObject result = new JObject();
            Assembly product = ProductProbes.Entry().Assembly;
            Type bootstrap = product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true);
            object set = bootstrap.GetProperty("GunslingerClass", Any).GetValue(null, null);
            if (set == null) { problem = "the product holds no Gunslinger blueprint set"; return result; }
            Type integration = product.GetType("KingmakerGunslinger.Grit.GritAbilityUiIntegration", true);
            MethodInfo production = integration.GetMethod("Apply", Any, null, new[] { set.GetType() }, null);
            MethodInfo recipientsOf = integration.GetMethod("Recipients", Any, null, new[] { set.GetType() }, null);
            Type recipientType = product.GetType("KingmakerGunslinger.Grit.GritUiRecipient", false);
            Type contractType = product.GetType("KingmakerGunslinger.Grit.GritUiContract", false);
            if (production == null || recipientsOf == null || recipientType == null || contractType == null)
            {
                problem = "the product has no recipient-based grit UI integration (Apply(set), Recipients(set), GritUiRecipient, GritUiContract)";
                return result;
            }

            BlueprintAbilityResource grit = ResourcesLibrary.TryGetBlueprint(gritGuid) as BlueprintAbilityResource;
            MethodInfo core = integration.GetMethod("Apply", Any, null, new[] { typeof(BlueprintAbilityResource), recipientType.MakeArrayType() }, null);
            if (grit == null || core == null) { problem = "no grit resource or no Apply(resource, recipients)"; return result; }

            Dictionary<string, BlueprintComponent[]> before = GritUiPass.Snapshot();
            int bound = (int)production.Invoke(null, new[] { set });
            string[] replaced = Replaced(before);
            result["repeat"] = new JObject { ["returned"] = bound, ["arraysReplaced"] = new JArray(replaced.Cast<object>().ToArray()) };
            if (replaced.Length != 0) problems.Add("a repeated integration replaced " + replaced.Length + " component arrays: " + string.Join(", ", replaced));

            Array real = (Array)recipientsOf.Invoke(null, new[] { set });
            result["recipients"] = real.Length;
            ConstructorInfo create = recipientType.GetConstructors(Any).Single();
            object sharedContract = Enum.Parse(contractType, "SharedIndicator");
            object ownContract = Enum.Parse(contractType, "OwnResourceLogic");
            List<UnityEngine.Object> fabricated = new List<UnityEngine.Object>();
            JArray refusals = new JArray();
            try
            {
                Refusal(refusals, problems, core, grit, recipientType, new object[0], "no recipients", new string[0]);
                Refusal(refusals, problems, core, grit, recipientType,
                    With(real, create.Invoke(new[] { "Harness Missing Deed", null, sharedContract })), "missing required ability", new[] { "Harness Missing Deed" });

                BlueprintAbility foreign = Fabricate(fabricated, "KMG_Harness_ForeignGritLogic", "4b4d47484152000000000000000000a1");
                AbilityResourceLogic spending = UnityEngine.ScriptableObject.CreateInstance<AbilityResourceLogic>();
                fabricated.Add(spending);
                spending.name = "$Harness_ForeignSpend";
                spending.RequiredResource = grit;
                spending.IsSpendResource = true;
                spending.Amount = 1;
                foreign.ComponentsArray = new BlueprintComponent[] { spending };
                BlueprintComponent[] foreignArray = foreign.ComponentsArray;
                Refusal(refusals, problems, core, grit, recipientType, With(real, create.Invoke(new object[] { "Harness Foreign Logic", foreign, sharedContract })),
                    "shared recipient already carrying a spending grit component", new[] { foreign.name, foreign.AssetGuid });
                if (!ReferenceEquals(foreign.ComponentsArray, foreignArray)) problems.Add("a refused recipient's component array was replaced");

                BlueprintAbility bare = Fabricate(fabricated, "KMG_Harness_NoOwnLogic", "4b4d47484152000000000000000000a2");
                Refusal(refusals, problems, core, grit, recipientType, With(real, create.Invoke(new object[] { "Harness Own Logic", bare, ownContract })),
                    "own-logic recipient with no grit resource logic", new[] { bare.name, bare.AssetGuid });

                BlueprintAbility unlisted = Fabricate(fabricated, "KMG_Harness_Unregistered", "4b4d47484152000000000000000000a3");
                object unlistedRecipient = create.Invoke(new object[] { "Harness Twice", unlisted, sharedContract });
                Refusal(refusals, problems, core, grit, recipientType, With(With(real, unlistedRecipient), unlistedRecipient),
                    "the same ability listed twice", new[] { unlisted.name, unlisted.AssetGuid });
                if ((unlisted.ComponentsArray ?? new BlueprintComponent[0]).Length != 0) problems.Add("a refused duplicate recipient received a component");
            }
            finally
            {
                foreach (UnityEngine.Object value in fabricated) UnityEngine.Object.Destroy(value);
            }

            result["refusals"] = refusals;
            replaced = Replaced(before);
            result["arraysReplacedAfterRefusals"] = new JArray(replaced.Cast<object>().ToArray());
            if (replaced.Length != 0) problems.Add("refused integrations replaced " + replaced.Length + " real component arrays");
            if (problems.Count > 0) problem = string.Join("; ", problems.ToArray());
            return result;
        }

        private static BlueprintAbility Fabricate(List<UnityEngine.Object> owned, string name, string guid)
        {
            BlueprintAbility ability = UnityEngine.ScriptableObject.CreateInstance<BlueprintAbility>();
            owned.Add(ability);
            ability.name = name;
            ability.AssetGuid = guid;
            ability.ComponentsArray = new BlueprintComponent[0];
            return ability;
        }

        private static object[] With(Array recipients, object extra)
        {
            return recipients.Cast<object>().Concat(new[] { extra }).ToArray();
        }

        private static void Refusal(JArray refusals, List<string> problems, MethodInfo core, BlueprintAbilityResource grit, Type recipientType,
            object[] recipients, string label, string[] mustName)
        {
            Array typed = Array.CreateInstance(recipientType, recipients.Length);
            for (int index = 0; index < recipients.Length; index++) typed.SetValue(recipients[index], index);
            string message = null;
            try { core.Invoke(null, new object[] { grit, typed }); }
            catch (TargetInvocationException exception) { message = exception.InnerException.GetType().Name + ": " + exception.InnerException.Message; }
            refusals.Add(new JObject { ["case"] = label, ["refusedWith"] = message });
            if (message == null) { problems.Add(label + " was accepted"); return; }
            foreach (string wanted in mustName)
                if (message.IndexOf(wanted, StringComparison.Ordinal) < 0) problems.Add(label + " was refused without naming " + wanted + ": " + message);
        }

        private static string[] Replaced(Dictionary<string, BlueprintComponent[]> before)
        {
            return ProjectAbilities().Where(ability => !before.TryGetValue(ability.AssetGuid, out BlueprintComponent[] array) ||
                !ReferenceEquals(array, ability.ComponentsArray)).Select(ability => ability.name).ToArray();
        }

        private static bool References(BlueprintComponent component, BlueprintAbilityResource grit)
        {
            if (component == null) return false;
            return component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Any(field =>
                typeof(BlueprintAbilityResource).IsAssignableFrom(field.FieldType) && ReferenceEquals(field.GetValue(component), grit));
        }

        private static JArray Named(IEnumerable<string> guids, Dictionary<string, string> names)
        {
            return new JArray(guids.Select(guid => (object)(names[guid] + " " + guid)).OrderBy(text => (string)text, StringComparer.Ordinal).ToArray());
        }

        private static void Compare(string what, List<string> observed, string[] expected, Dictionary<string, string> names, List<string> problems)
        {
            string[] missing = expected.Except(observed).Select(guid => names.TryGetValue(guid, out string name) ? name : guid).ToArray();
            string[] extra = observed.Except(expected).Select(guid => names[guid]).ToArray();
            if (missing.Length > 0) problems.Add(what + " is missing on: " + string.Join(", ", missing));
            if (extra.Length > 0) problems.Add(what + " is unexpectedly on: " + string.Join(", ", extra));
        }
    }
}
