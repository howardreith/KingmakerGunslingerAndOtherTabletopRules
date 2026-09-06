using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Read-only evidence for the remaining Crystalline Form implementation.
    /// WeaponCategory.Ray is an engine attack abstraction, not rules authority.
    /// Inventory every projectile delivery (including non-ray controls) before
    /// deciding whether an exact semantic catalog is required. This probe does
    /// not grant the trait, alter a roll, or claim deflection qualification.
    /// </summary>
    internal static class ElementalCrystallineFormNativeAuditScenario
    {
        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            BlueprintScriptableObject[] before = BlueprintBootstrap.Library
                .GetAllBlueprints().Where(value => value != null).ToArray();
            BlueprintAbility[] abilities = before.OfType<BlueprintAbility>()
                .OrderBy(value => value.AssetGuid, StringComparer.Ordinal).ToArray();
            BlueprintComponent[][] components = abilities.Select(value =>
                value.ComponentsArray).ToArray();
            var rows = new JArray();
            foreach (BlueprintAbility ability in abilities)
                foreach (AbilityDeliverProjectile delivery in
                    (ability.ComponentsArray ?? new BlueprintComponent[0])
                        .OfType<AbilityDeliverProjectile>())
                {
                    rows.Add(new JObject {
                        { "abilityGuid", ability.AssetGuid },
                        { "internalName", ability.name },
                        { "displayName", ability.Name },
                        { "description", ability.Description },
                        { "abilityType", ability.Type.ToString() },
                        { "parentGuid", ability.Parent == null ? null : ability.Parent.AssetGuid },
                        { "projectileType", delivery.Type.ToString() },
                        { "needAttackRoll", delivery.NeedAttackRoll },
                        { "handOfApprentice", delivery.IsHandOfTheApprentice },
                        { "weaponGuid", delivery.Weapon == null ? null : delivery.Weapon.AssetGuid },
                        { "weaponCategory", delivery.Weapon == null ? null : delivery.Weapon.Category.ToString() },
                        { "projectiles", new JArray((delivery.Projectiles ?? new BlueprintProjectile[0])
                            .Select(value => value == null ? null : new JObject {
                                { "guid", value.AssetGuid }, { "internalName", value.name }
                            })) },
                        { "componentTypes", new JArray((ability.ComponentsArray ?? new BlueprintComponent[0])
                            .Where(value => value != null).Select(value => value.GetType().FullName)) }
                    });
                }

            // Exact installed native witnesses; names are observation labels,
            // never a production eligibility predicate.
            string[] rayWitnesses = {
                "9af2ab69df6538f4793b2f9c3cc85603", // Ray of Frost
                "cdb106d53c65bbc4086183d54c3b97c7", // Scorching Ray
                "bf0accce250381a44b857d4af6c8e10d", // Searing Light
                "17696c144a0194c478cbe402b496cb23"  // Polar Ray
            };
            foreach (string guid in rayWitnesses)
            {
                JObject[] witness = rows.OfType<JObject>().Where(value =>
                    (string)value["abilityGuid"] == guid).ToArray();
                Check(assertions, "ray-witness-" + guid,
                    witness.Length == 1 && (bool)witness[0]["needAttackRoll"] &&
                        (string)witness[0]["weaponCategory"] == "Ray",
                    "exact native ray has one attack-roll projectile delivery; count=" + witness.Length);
            }
            string[] controls = { "AcidSplash", "Snowball", "BatteringBlast", "MagicMissile" };
            foreach (string name in controls)
                Check(assertions, "non-ray-witness-" + name,
                    rows.OfType<JObject>().Any(value => (string)value["internalName"] == name),
                    "native non-ray control is inventoried without treating its weapon category as rules authority");

            MethodInfo replace = typeof(RuleAttackRoll).GetMethod("SetFake",
                BindingFlags.Public | BindingFlags.Instance, null,
                new[] { typeof(AttackResult) }, null);
            Check(assertions, "native-result-replacement-contract",
                replace != null && replace.ReturnType == typeof(void),
                "public native SetFake(AttackResult) exists; no private-field mutation or global patch required by this API");
            BlueprintScriptableObject[] after = BlueprintBootstrap.Library
                .GetAllBlueprints().Where(value => value != null).ToArray();
            bool unchanged = before.SequenceEqual(after) && abilities.Select((value, index) =>
                ReferenceEquals(value.ComponentsArray, components[index])).All(value => value);
            Check(assertions, "read-only-catalog", unchanged,
                "exact catalog references/order and every observed ability component-array reference retained");
            string path = Path.Combine(request.EvidenceDirectory,
                "elemental-crystalline-form-native-audit.json");
            File.WriteAllText(path, new JObject {
                { "schemaVersion", 1 }, { "saveStateTouched", false },
                { "traitMechanicQualified", false }, { "catalogUnchanged", unchanged },
                { "blueprintCount", before.Length }, { "projectileDeliveryCount", rows.Count },
                { "deliveries", rows }
            }.ToString(Formatting.Indented));
            files.Add(path);
        }

        private static void Check(ICollection<RuntimeTestAssertion> assertions,
            string name, bool pass, string observed)
        {
            assertions.Add(new RuntimeTestAssertion {
                Name = "elemental-crystalline-native-audit-" + name,
                Expected = "true", Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "read-only exact installed projectile/attack-result contract; not trait-mechanics proof"
            });
        }
    }
}
