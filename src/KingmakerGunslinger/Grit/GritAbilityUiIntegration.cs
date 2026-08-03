using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;

namespace KingmakerGunslinger.Grit
{
    internal static class GritAbilityUiIntegration
    {
        internal static int Apply(LibraryScriptableObject library,
            BlueprintAbilityResource grit, BlueprintAbility dodge)
        {
            if (library == null || grit == null || dodge == null)
                throw new ArgumentNullException();
            int count = 0;
            foreach (BlueprintAbility ability in library.GetAllBlueprints()
                .OfType<BlueprintAbility>().Where(value => value != null &&
                    value.name != null && value.name.StartsWith("KMG_",
                        StringComparison.Ordinal)))
            {
                if (!ReferenceEquals(ability, dodge) &&
                    !(ability.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                        .Any(component => References(component, grit)) &&
                    !DescribesGritSpend(ability)) continue;
                if ((ability.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<AbilityResourceLogic>().Any(value =>
                        ReferenceEquals(value.RequiredResource, grit))) continue;
                var ui = UnityEngine.ScriptableObject.CreateInstance<
                    GritAbilityResourceUiLogic>();
                ui.name = "$KMG_SharedGritUi";
                ui.RequiredResource = grit;
                // Kingmaker's action bar reports a resource count only when this
                // flag is true. The derived component keeps native availability
                // and count behavior while its virtual Spend remains a no-op;
                // request-local deed transactions stay the atomic authority.
                ui.IsSpendResource = true;
                ui.CostIsCustom = false;
                ui.Amount = 1;
                ability.ComponentsArray = (ability.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).Concat(
                        new BlueprintComponent[] { ui }).ToArray();
                count++;
            }
            if (count == 0)
                throw new InvalidOperationException(
                    "No grit-consuming abilities received the shared resource UI contract.");
            return count;
        }

        private static bool DescribesGritSpend(BlueprintAbility ability)
        {
            string description = ability == null ? null : ability.Description;
            if (string.IsNullOrWhiteSpace(description)) return false;
            return description.IndexOf("grit", StringComparison.OrdinalIgnoreCase) >= 0 &&
                description.IndexOf("spend", StringComparison.OrdinalIgnoreCase) >= 0 &&
                description.IndexOf("without spending grit",
                    StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static bool References(BlueprintComponent component,
            BlueprintAbilityResource grit)
        {
            if (component == null) return false;
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance;
            return component.GetType().GetFields(flags).Any(field =>
                typeof(BlueprintAbilityResource).IsAssignableFrom(field.FieldType) &&
                ReferenceEquals(field.GetValue(component), grit));
        }
    }

    internal sealed class GritAbilityResourceUiLogic : AbilityResourceLogic
    {
        public override void Spend(Kingmaker.UnitLogic.Abilities.AbilityData ability)
        {
            // The deed's production transaction spends after all deed-specific
            // gates pass. Spending here would double-charge or charge reactions
            // that never trigger.
        }
    }
}
