using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Read-only engine evidence for the five remaining trait mechanics.
    /// Names select diagnostic observations only, never production eligibility.</summary>
    internal static class ElementalRemainingTraitNativeAuditScenario
    {
        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            BlueprintScriptableObject[] before = BlueprintBootstrap.Library.GetAllBlueprints()
                .Where(value => value != null).ToArray();
            BlueprintComponent[][] components = before.Select(value => value.ComponentsArray).ToArray();
            string[] exact = {
                "fa5ee5f4cd5c6394f8b497c773f8e14a", // native earth mephit acid cone
                "a54cd27999a5e8340976f3a40edfef3a", // water mephit (observe actual energy)
                "993908ad3fb81f34ba0ed168b7c61f58", // Bard Fascinate mode
                "a4fc1c0798359974e99e1d790935501d", // Bard Fascinate area
                "9c70d2ae017665b4b845e6c299cb7439", // native fascinated target buff
                "a50373fa77d30d34c8c6efb198b36921", // native repeat immunity
                "555930f121b364a4e82670b433028728"  // Bard Fascinate caster aura
            };
            var selected = before.Where(value => exact.Contains(value.AssetGuid) ||
                new[] { "Fascinat", "Sickened", "DifficultTerrain", "StoneCall", "Entangle", "Grease" }
                    .Any(term => (value.name ?? string.Empty).IndexOf(term,
                        StringComparison.OrdinalIgnoreCase) >= 0)).OrderBy(value => value.AssetGuid,
                            StringComparer.Ordinal).ToArray();
            var rows = new JArray(selected.Select(value => {
                // Game-wide save serialization uses an opt-in resolver. These
                // ordinary evidence DTOs must explicitly retain their fields.
                JObject row = JObject.FromObject(ElementalFeatNativeAuditScenario.SnapshotContract(value),
                    new JsonSerializer { ContractResolver = new DefaultContractResolver() });
                BlueprintAbility ability = value as BlueprintAbility;
                if (ability != null)
                {
                    row["abilityType"] = ability.Type.ToString();
                    row["range"] = ability.Range.ToString();
                    row["actionType"] = ability.ActionType.ToString();
                    row["description"] = ability.Description;
                    row["spellResistance"] = ability.SpellResistance;
                }
                return row;
            }));
            foreach (string guid in exact)
                Check(assertions, "exact-witness-" + guid, before.Count(value => value.AssetGuid == guid) == 1,
                    "exact installed donor identity observed; this does not qualify using its mechanics");
            Check(assertions, "serialized-contracts-complete", rows.Count == selected.Length &&
                rows.OfType<JObject>().Select((row, index) => (string)row["Guid"] == selected[index].AssetGuid &&
                    row["Components"] is JArray && ((JArray)row["Components"]).Count ==
                        (selected[index].ComponentsArray ?? new BlueprintComponent[0]).Count(value => value != null))
                    .All(value => value), "explicit evidence serializer retains exact IDs and every component contract");
            object surface = BlueprintRoot.Instance.SurfaceTypeData;
            var surfaceRows = new JObject();
            foreach (string fieldName in new[] { "Types", "Settings", "SortedMaskNames" })
            {
                var field = surface == null ? null : surface.GetType().GetField(fieldName);
                surfaceRows[fieldName] = field == null ? "<absent>" :
                    ElementalFeatNativeAuditScenario.SnapshotFields(field.GetValue(surface), 8);
            }
            bool unchanged = before.SequenceEqual(BlueprintBootstrap.Library.GetAllBlueprints()
                .Where(value => value != null)) && before.Select((value, index) =>
                    ReferenceEquals(value.ComponentsArray, components[index])).All(value => value);
            Check(assertions, "read-only-catalog", unchanged,
                "all native/project/foreign blueprint references, order and component-array references unchanged");
            Check(assertions, "native-surface-data-observed", surface != null,
                "native root surface catalog observed; no material eligibility inferred from audio names alone");
            string path = Path.Combine(request.EvidenceDirectory, "elemental-remaining-trait-native-audit.json");
            File.WriteAllText(path, new JObject {
                { "schemaVersion", 1 }, { "saveStateTouched", false }, { "mechanicsQualified", false },
                { "catalogUnchanged", unchanged }, { "selectedBlueprints", rows },
                { "nativeSurfaceData", surfaceRows }
            }.ToString(Formatting.Indented));
            files.Add(path);
        }

        private static void Check(ICollection<RuntimeTestAssertion> assertions, string name,
            bool pass, string observed)
        {
            assertions.Add(new RuntimeTestAssertion {
                Name = "elemental-remaining-trait-audit-" + name, Expected = "true",
                Observed = observed, Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "read-only exact installed donor/surface contract; not gameplay proof"
            });
        }
    }
}
