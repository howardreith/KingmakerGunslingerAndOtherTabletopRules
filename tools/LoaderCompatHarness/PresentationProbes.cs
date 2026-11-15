using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Root;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Newtonsoft.Json.Linq;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// Independent observations of the product's player-facing unit facts in the running game: the raw text fields
    /// Kingmaker stores, what they resolve to in the active localization pack, and native UI visibility. The harness
    /// reads engine state only; it does not call the product's own validation.
    /// </summary>
    internal static class PresentationProbes
    {
        private const BindingFlags Any = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string ProjectPrefix = "KMG_";
        private static readonly FieldInfo NameField = typeof(BlueprintUnitFact).GetField("m_DisplayName", Any);
        private static readonly FieldInfo DescriptionField = typeof(BlueprintUnitFact).GetField("m_Description", Any);

        /// <summary>How this process resolves text that was never assigned, and which locale is active.</summary>
        public static JObject Environment()
        {
            LocalizedString unassigned = null;
            LocalizedString emptyKey = new LocalizedString();
            LocalizedString unknownKey = new LocalizedString();
            typeof(LocalizedString).GetField("m_Key", Any).SetValue(unknownKey, "KmgLoaderCompatHarness.UnknownKeyProbe");
            LocalizationPack pack = LocalizationManager.CurrentPack;
            return new JObject
            {
                ["currentLocale"] = LocalizationManager.CurrentLocale.ToString(),
                ["currentPackLocale"] = pack == null ? null : pack.Locale.ToString(),
                ["currentPackStrings"] = pack == null || pack.Strings == null ? -1 : pack.Strings.Count,
                ["unassignedStringResolvesTo"] = (string)unassigned,
                ["emptyKeyResolvesTo"] = (string)emptyKey,
                ["unknownKeyResolvesTo"] = (string)unknownKey,
                ["conditions"] = ReproductionConditions.Describe()
            };
        }

        /// <summary>The product's loader, context and blueprint-bootstrap state, with the retained failure chain.</summary>
        public static JObject InitializationState()
        {
            Assembly product = ProductProbes.Entry().Assembly;
            if (product == null) throw new InvalidOperationException("the product has no loaded assembly");
            Type bootstrap = product.GetType("KingmakerGunslinger.Bootstrap.BlueprintBootstrap", true);
            Type contextType = product.GetType("KingmakerGunslinger.Bootstrap.ModContext", true);
            object context = contextType.GetField("_current", Any).GetValue(null);
            JObject state = new JObject
            {
                ["loaderEntryActive"] = ProductProbes.Entry().Active,
                ["mainState"] = product.GetType("KingmakerGunslinger.Main", true).GetField("_state", Any).GetValue(null).ToString(),
                ["blueprintsInitialized"] = (bool)bootstrap.GetProperty("IsInitialized", Any).GetValue(null, null),
                ["blueprintsFailed"] = (bool)bootstrap.GetProperty("IsFailed", Any).GetValue(null, null),
                ["contextFailed"] = context != null && (bool)contextType.GetProperty("IsFailed", Any).GetValue(context, null)
            };
            JArray chain = new JArray();
            Exception failure = context == null ? null : (Exception)contextType.GetProperty("Failure", Any).GetValue(context, null);
            for (int depth = 0; failure != null && depth < 8; failure = failure.InnerException, depth++)
            {
                chain.Add(failure.GetType().FullName + ": " + failure.Message);
            }

            state["failureChain"] = chain;
            return state;
        }

        public static string AssertInitialization(JObject state, string expect, string rootCauseContains)
        {
            bool initialized = (bool)state["blueprintsInitialized"];
            bool failed = (bool)state["blueprintsFailed"] || (bool)state["contextFailed"];
            string chain = string.Join(" <- ", ((JArray)state["failureChain"]).Select(t => (string)t).ToArray());
            if (string.Equals(expect, "completed", StringComparison.OrdinalIgnoreCase))
            {
                return initialized && !failed ? null : "blueprint initialization did not complete: initialized=" + initialized + " failed=" + failed + " chain=" + chain;
            }

            if (string.Equals(expect, "failed", StringComparison.OrdinalIgnoreCase))
            {
                if (initialized || !failed) return "blueprint initialization was expected to fail: initialized=" + initialized + " failed=" + failed;
                if (!string.IsNullOrEmpty(rootCauseContains) && chain.IndexOf(rootCauseContains, StringComparison.Ordinal) < 0)
                    return "failure chain does not contain '" + rootCauseContains + "': " + chain;
                return null;
            }

            return "kmgInitialization needs expect=completed or expect=failed";
        }

        /// <summary>How often a class is published in the native class catalog and whether its blueprint resolves.</summary>
        public static JObject ClassCatalog(string guid)
        {
            BlueprintCharacterClass[] published = BlueprintRoot.Instance.Progression.CharacterClasses;
            BlueprintScriptableObject blueprint = ResourcesLibrary.TryGetBlueprint(guid);
            return new JObject
            {
                ["guid"] = guid,
                ["publishedCount"] = published.Count(c => c != null && c.AssetGuid == guid),
                ["blueprintResolves"] = blueprint != null,
                ["blueprintName"] = blueprint == null ? null : blueprint.name,
                ["characterClassTotal"] = published.Length
            };
        }

        /// <summary>
        /// Every unit fact reachable from the class progression and its archetypes through level entries, selection
        /// lists and public component references, with the raw and resolved text of each project-owned one.
        /// </summary>
        public static JObject Census(string classGuid, IEnumerable<string> requiredVisibleAssigned, IEnumerable<string> requiredHidden,
            IDictionary<string, string> invariants, out string failure)
        {
            failure = null;
            BlueprintCharacterClass characterClass = ResourcesLibrary.TryGetBlueprint(classGuid) as BlueprintCharacterClass;
            if (characterClass == null)
            {
                failure = "character class " + classGuid + " is not registered";
                return new JObject { ["environment"] = Environment() };
            }

            var visited = new HashSet<BlueprintUnitFact>();
            var order = new List<BlueprintUnitFact>();
            if (characterClass.Progression != null)
            {
                foreach (LevelEntry entry in characterClass.Progression.LevelEntries ?? new LevelEntry[0])
                    foreach (BlueprintFeatureBase feature in entry == null || entry.Features == null ? new List<BlueprintFeatureBase>() : entry.Features)
                        Visit(feature, visited, order);
            }

            foreach (BlueprintArchetype archetype in characterClass.Archetypes ?? new BlueprintArchetype[0])
                foreach (LevelEntry entry in archetype == null || archetype.AddFeatures == null ? new LevelEntry[0] : archetype.AddFeatures)
                    foreach (BlueprintFeatureBase feature in entry == null || entry.Features == null ? new List<BlueprintFeatureBase>() : entry.Features)
                        Visit(feature, visited, order);

            JArray project = new JArray();
            JArray offenders = new JArray();
            int visibleProject = 0;
            int hiddenProject = 0;
            foreach (BlueprintUnitFact fact in order)
            {
                if (fact.name == null || !fact.name.StartsWith(ProjectPrefix, StringComparison.Ordinal)) continue;
                JObject record = Describe(fact);
                project.Add(record);
                if ((bool)record["hiddenInUi"])
                {
                    hiddenProject++;
                    continue;
                }

                visibleProject++;
                if ((string)record["nameState"] != "assigned" || (string)record["descriptionState"] != "assigned") offenders.Add(record);
            }

            JArray required = new JArray();
            var problems = new List<string>();
            foreach (string guid in requiredVisibleAssigned.Where(g => !string.IsNullOrWhiteSpace(g)))
            {
                BlueprintUnitFact fact = order.FirstOrDefault(candidate => candidate.AssetGuid == guid);
                if (fact == null)
                {
                    problems.Add(guid + " is not reachable from the class presentation traversal");
                    continue;
                }

                JObject record = Describe(fact);
                required.Add(record);
                if ((bool)record["hiddenInUi"]) problems.Add(fact.name + " is hidden in the native UI");
                if ((string)record["nameState"] != "assigned") problems.Add(fact.name + " name is " + record["nameState"]);
                if ((string)record["descriptionState"] != "assigned") problems.Add(fact.name + " description is " + record["descriptionState"]);
                if (!(bool)record["hasIcon"]) problems.Add(fact.name + " has no icon");
            }

            foreach (string guid in requiredHidden.Where(g => !string.IsNullOrWhiteSpace(g)))
            {
                BlueprintUnitFact fact = order.FirstOrDefault(candidate => candidate.AssetGuid == guid);
                if (fact == null)
                {
                    problems.Add(guid + " is not reachable from the class presentation traversal");
                    continue;
                }

                JObject record = Describe(fact);
                record["requiredHidden"] = true;
                required.Add(record);
                if (!(bool)record["hiddenInUi"]) problems.Add(fact.name + " is not hidden in the native UI");
            }

            // Mechanics that must not move: "guid": "field=value,field=value" against the census record.
            foreach (KeyValuePair<string, string> invariant in invariants ?? new Dictionary<string, string>())
            {
                BlueprintUnitFact fact = order.FirstOrDefault(candidate => candidate.AssetGuid == invariant.Key);
                if (fact == null)
                {
                    problems.Add(invariant.Key + " (invariant) is not reachable from the class presentation traversal");
                    continue;
                }

                JObject record = Describe(fact);
                foreach (string pair in invariant.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = pair.Split('=');
                    string observed = record[parts[0].Trim()] == null ? "<absent>" : record[parts[0].Trim()].ToString();
                    if (!string.Equals(observed, parts[1].Trim(), StringComparison.Ordinal))
                        problems.Add(fact.name + " " + parts[0].Trim() + " is " + observed + ", expected " + parts[1].Trim());
                }
            }

            foreach (JObject offender in offenders)
            {
                string line = offender["name"] + " name=" + offender["nameState"] + " description=" + offender["descriptionState"];
                if (!problems.Any(problem => problem.StartsWith((string)offender["name"] + " ", StringComparison.Ordinal))) problems.Add(line);
            }

            if (problems.Count > 0) failure = problems.Count + " presentation problem(s): " + string.Join("; ", problems.ToArray());
            return new JObject
            {
                ["environment"] = Environment(),
                ["class"] = characterClass.name,
                ["visitedFacts"] = order.Count,
                ["projectFacts"] = project.Count,
                ["visibleProjectFacts"] = visibleProject,
                ["hiddenProjectFacts"] = hiddenProject,
                ["offenderCount"] = offenders.Count,
                ["offenders"] = offenders,
                ["required"] = required,
                ["project"] = project
            };
        }

        /// <summary>
        /// After a failed initialization: every recorded unit fact holds exactly the description and component array it
        /// had before the product ran, except where another party replaced the value after publication; that foreign
        /// value must still be in place.
        /// </summary>
        public static JObject RollbackResidue(out string failure)
        {
            var problems = new List<string>();
            JArray entries = new JArray();
            foreach (KeyValuePair<string, LocalizedString> pair in ReproductionConditions.OriginalDescriptions)
            {
                BlueprintUnitFact fact = ReproductionConditions.Fact(pair.Key);
                if (fact == null)
                {
                    problems.Add(pair.Key + " no longer resolves");
                    continue;
                }

                LocalizedString current = ReproductionConditions.CurrentDescription(fact);
                BlueprintComponent[] components = fact.ComponentsArray;
                string description;
                if (ReproductionConditions.ForeignDescriptions.TryGetValue(pair.Key, out LocalizedString foreignDescription))
                    description = ReferenceEquals(current, foreignDescription) ? "foreign-preserved" : "foreign-overwritten";
                else if (ReferenceEquals(current, pair.Value))
                    description = ReproductionConditions.BlankDescriptions.ContainsKey(pair.Key) ? "blank-original-restored" : "original-restored";
                else
                    description = "not-restored";

                string componentState;
                if (ReproductionConditions.ForeignComponents.TryGetValue(pair.Key, out BlueprintComponent[] foreignComponents))
                    componentState = ReferenceEquals(components, foreignComponents) ? "foreign-preserved" : "foreign-overwritten";
                else if (ReferenceEquals(components, ReproductionConditions.OriginalComponents[pair.Key]))
                    componentState = "original-restored";
                else
                    componentState = "not-restored";

                entries.Add(new JObject
                {
                    ["guid"] = pair.Key,
                    ["name"] = fact.name,
                    ["description"] = description,
                    ["descriptionKey"] = current == null ? null : current.Key,
                    ["components"] = componentState,
                    ["componentCount"] = components == null ? -1 : components.Length
                });
                if (description == "not-restored" || description == "foreign-overwritten") problems.Add(fact.name + " description " + description);
                if (componentState == "not-restored" || componentState == "foreign-overwritten") problems.Add(fact.name + " components " + componentState);
            }

            if (entries.Count == 0) problems.Add("no original state was captured before the product ran");
            // A session that names a fact expects it judged: an unresolved guid must not shrink the check silently.
            string[] uncaptured = ReproductionConditions.RequestedCaptures.Where(guid => !ReproductionConditions.OriginalDescriptions.ContainsKey(guid)).ToArray();
            if (uncaptured.Length > 0) problems.Add("requested originals were not captured: " + string.Join(", ", uncaptured));
            failure = problems.Count == 0 ? null : problems.Count + " rollback residue problem(s): " + string.Join("; ", problems.ToArray());
            return new JObject { ["entries"] = entries, ["conditions"] = ReproductionConditions.Describe() };
        }

        /// <summary>
        /// Negative and positive controls run through the product's own PlayerFacingPresentation.Apply on throwaway
        /// unit facts built from real engine types: visible project content without assigned text must be refused,
        /// assigned text must pass, and Kingmaker's own visibility flags (not a naming convention) decide what is
        /// exempt. The facts are never registered in the blueprint library.
        /// </summary>
        public static JObject PresentationContract(out string failure)
        {
            Assembly product = ProductProbes.Entry().Assembly;
            Type presentation = product.GetType("KingmakerGunslinger.Blueprints.PlayerFacingPresentation", true);
            MethodInfo apply = presentation.GetMethod("Apply", Any, null, new[] { typeof(BlueprintProgression), typeof(UnityEngine.Sprite) }, null);
            if (apply == null) throw new MissingMethodException(presentation.FullName, "Apply(BlueprintProgression, Sprite)");
            // Any native icon serves as the fallback; the product's own class may be absent after a failed start.
            UnityEngine.Sprite icon = ResourcesLibrary.GetBlueprints<BlueprintFeature>().Where(f => f != null && f.Icon != null).Select(f => f.Icon).FirstOrDefault();
            if (icon == null) throw new InvalidOperationException("no native feature icon is available as a fallback icon");
            LocalizationPack pack = LocalizationManager.CurrentPack;
            if (pack == null || pack.Strings == null) throw new InvalidOperationException("no current localization pack");
            const string nameKey = "KmgLoaderCompatHarness.Contract.Name";
            const string descriptionKey = "KmgLoaderCompatHarness.Contract.Description";
            pack.Strings[nameKey] = "Harness contract name";
            pack.Strings[descriptionKey] = "Harness contract description.";
            FieldInfo flags = typeof(BlueprintBuff).GetField("m_Flags", Any);
            var created = new List<UnityEngine.Object>();
            JArray cases = new JArray();
            var problems = new List<string>();
            try
            {
                Func<string, BlueprintBuff> buff = name =>
                {
                    BlueprintBuff value = UnityEngine.ScriptableObject.CreateInstance<BlueprintBuff>();
                    value.name = name;
                    value.ComponentsArray = new BlueprintComponent[0];
                    created.Add(value);
                    return value;
                };
                Func<string, bool, BlueprintFeature> feature = (name, hide) =>
                {
                    BlueprintFeature value = UnityEngine.ScriptableObject.CreateInstance<BlueprintFeature>();
                    value.name = name;
                    value.HideInUI = hide;
                    value.ComponentsArray = new BlueprintComponent[0];
                    created.Add(value);
                    return value;
                };
                Action<BlueprintUnitFact, string, string> text = (fact, name, description) =>
                {
                    NameField.SetValue(fact, Keyed(name));
                    DescriptionField.SetValue(fact, Keyed(description));
                };

                BlueprintBuff unassigned = buff("KMG_HarnessContract_VisibleUnassigned");
                BlueprintBuff emptyKey = buff("KMG_HarnessContract_VisibleEmptyKey");
                text(emptyKey, string.Empty, string.Empty);
                BlueprintBuff unknownKey = buff("KMG_HarnessContract_VisibleUnknownKey");
                text(unknownKey, "KmgLoaderCompatHarness.Contract.MissingName", "KmgLoaderCompatHarness.Contract.MissingDescription");
                BlueprintBuff namedOnly = buff("KMG_HarnessContract_VisibleNameOnly");
                NameField.SetValue(namedOnly, Keyed(nameKey));
                BlueprintBuff assigned = buff("KMG_HarnessContract_VisibleAssigned");
                text(assigned, nameKey, descriptionKey);
                BlueprintBuff hidden = buff("KMG_HarnessContract_HiddenUnassigned");
                flags.SetValue(hidden, Enum.Parse(flags.FieldType, "HiddenInUi"));
                BlueprintBuff armedNamedVisible = buff("KMG_HarnessContract_Internal_Armed");
                BlueprintBuff foreign = buff("HarnessContract_NotProject_Unassigned");
                BlueprintFeature visibleFeature = feature("KMG_HarnessContract_VisibleFeatureUnassigned", false);
                BlueprintFeature hiddenFeature = feature("KMG_HarnessContract_HiddenFeatureUnassigned", true);

                var expectations = new[]
                {
                    new { Subject = (BlueprintUnitFact)unassigned, Reject = true, Why = "visible project buff with no stored text" },
                    new { Subject = (BlueprintUnitFact)emptyKey, Reject = true, Why = "visible project buff with empty-key text" },
                    new { Subject = (BlueprintUnitFact)unknownKey, Reject = true, Why = "visible project buff whose keys are not in the pack" },
                    new { Subject = (BlueprintUnitFact)namedOnly, Reject = true, Why = "visible project buff with a name but no description" },
                    new { Subject = (BlueprintUnitFact)assigned, Reject = false, Why = "visible project buff with assigned text" },
                    new { Subject = (BlueprintUnitFact)hidden, Reject = false, Why = "project buff hidden by Kingmaker's HiddenInUi flag" },
                    new { Subject = (BlueprintUnitFact)armedNamedVisible, Reject = true, Why = "a name ending in _Armed is not a visibility rule" },
                    new { Subject = (BlueprintUnitFact)foreign, Reject = false, Why = "not project-owned" },
                    new { Subject = (BlueprintUnitFact)visibleFeature, Reject = true, Why = "visible project feature with no stored text" },
                    new { Subject = (BlueprintUnitFact)hiddenFeature, Reject = false, Why = "project feature hidden by HideInUI" }
                };
                foreach (var expectation in expectations)
                {
                    BlueprintFeature carrier = feature("HarnessContract_Carrier", true);
                    Kingmaker.UnitLogic.FactLogic.AddFacts grant = UnityEngine.ScriptableObject.CreateInstance<Kingmaker.UnitLogic.FactLogic.AddFacts>();
                    grant.Facts = new[] { expectation.Subject };
                    created.Add(grant);
                    carrier.ComponentsArray = new BlueprintComponent[] { grant };
                    BlueprintProgression progression = UnityEngine.ScriptableObject.CreateInstance<BlueprintProgression>();
                    progression.name = "HarnessContract_Progression";
                    progression.ComponentsArray = new BlueprintComponent[0];
                    LevelEntry entry = new LevelEntry { Level = 1 };
                    if (entry.Features == null) entry.Features = new List<BlueprintFeatureBase>();
                    entry.Features.Add(carrier);
                    progression.LevelEntries = new[] { entry };
                    created.Add(progression);

                    string outcome;
                    string message = null;
                    try
                    {
                        apply.Invoke(null, new object[] { progression, icon });
                        outcome = "accepted";
                    }
                    catch (TargetInvocationException exception)
                    {
                        message = exception.InnerException == null ? exception.Message : exception.InnerException.Message;
                        outcome = message.IndexOf("incomplete localization", StringComparison.Ordinal) >= 0 &&
                            message.IndexOf(expectation.Subject.name, StringComparison.Ordinal) >= 0 ? "rejected" : "unexpected-error";
                    }

                    string wanted = expectation.Reject ? "rejected" : "accepted";
                    cases.Add(new JObject
                    {
                        ["subject"] = expectation.Subject.name, ["why"] = expectation.Why, ["expected"] = wanted, ["observed"] = outcome,
                        ["message"] = message, ["iconAfter"] = expectation.Subject.Icon != null
                    });
                    if (outcome != wanted) problems.Add(expectation.Subject.name + " was " + outcome + ", expected " + wanted + " (" + expectation.Why + ")");
                }

                if (assigned.Icon == null) problems.Add("an accepted visible project buff received no fallback icon");
            }
            finally
            {
                pack.Strings.Remove(nameKey);
                pack.Strings.Remove(descriptionKey);
                foreach (UnityEngine.Object value in created) UnityEngine.Object.Destroy(value);
            }

            failure = problems.Count == 0 ? null : problems.Count + " presentation contract problem(s): " + string.Join("; ", problems.ToArray());
            return new JObject { ["environment"] = Environment(), ["cases"] = cases };
        }

        private static LocalizedString Keyed(string key)
        {
            LocalizedString value = new LocalizedString();
            typeof(LocalizedString).GetField("m_Key", Any).SetValue(value, key);
            return value;
        }

        private static JObject Describe(BlueprintUnitFact fact)
        {
            var feature = fact as BlueprintFeature;
            var ability = fact as BlueprintAbility;
            var buff = fact as BlueprintBuff;
            bool hidden = (feature != null && feature.HideInUI) || (ability != null && ability.Hidden) || (buff != null && buff.IsHiddenInUI);
            string name = fact.Name;
            string description = fact.Description;
            JObject record = new JObject
            {
                ["name"] = fact.name,
                ["guid"] = fact.AssetGuid,
                ["type"] = fact.GetType().Name,
                ["hiddenInUi"] = hidden,
                ["nameField"] = Raw((LocalizedString)NameField.GetValue(fact)),
                ["descriptionField"] = Raw((LocalizedString)DescriptionField.GetValue(fact)),
                ["resolvedName"] = name,
                ["resolvedDescription"] = description,
                ["nameState"] = State((LocalizedString)NameField.GetValue(fact), name),
                ["descriptionState"] = State((LocalizedString)DescriptionField.GetValue(fact), description),
                ["hasIcon"] = fact.Icon != null,
                ["icon"] = fact.Icon == null ? null : fact.Icon.name
            };
            if (buff != null)
            {
                record["isClassFeature"] = buff.IsClassFeature;
                record["stacking"] = buff.Stacking.ToString();
                record["componentCount"] = buff.ComponentsArray == null ? -1 : buff.ComponentsArray.Length;
            }

            return record;
        }

        private static JToken Raw(LocalizedString value)
        {
            if (value == null) return JValue.CreateNull();
            LocalizationPack pack = LocalizationManager.CurrentPack;
            string text = null;
            bool inPack = pack != null && pack.Strings != null && !string.IsNullOrEmpty(value.Key) && pack.Strings.TryGetValue(value.Key, out text);
            return new JObject { ["key"] = value.Key, ["shared"] = value.Shared != null, ["inCurrentPack"] = inPack, ["packTextBlank"] = inPack && string.IsNullOrWhiteSpace(text) };
        }

        // "assigned" needs a stored string whose key the active pack maps to real text. Everything else names how the
        // engine or the environment filled the gap.
        private static string State(LocalizedString field, string resolved)
        {
            if (field == null) return string.IsNullOrWhiteSpace(resolved) ? "unassigned-blank" : "unassigned-placeholder";
            if (field.Shared == null && string.IsNullOrEmpty(field.Key)) return "empty-key";
            if (string.IsNullOrWhiteSpace(resolved)) return "blank";
            if (resolved.StartsWith("[unknown key: ", StringComparison.Ordinal)) return "unknown-key-placeholder";
            return "assigned";
        }

        private static void Visit(BlueprintUnitFact fact, HashSet<BlueprintUnitFact> visited, List<BlueprintUnitFact> order)
        {
            if (fact == null || !visited.Add(fact)) return;
            order.Add(fact);
            var selection = fact as BlueprintFeatureSelection;
            if (selection != null && selection.AllFeatures != null)
                foreach (BlueprintFeature child in selection.AllFeatures) Visit(child, visited, order);
            if (fact.ComponentsArray == null) return;
            foreach (BlueprintComponent component in fact.ComponentsArray)
            {
                if (component == null) continue;
                foreach (FieldInfo field in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
                {
                    object value = field.GetValue(component);
                    if (value is BlueprintUnitFact single) Visit(single, visited, order);
                    else if (value is IEnumerable<BlueprintUnitFact> many)
                        foreach (BlueprintUnitFact child in many) Visit(child, visited, order);
                }
            }
        }
    }
}
