using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string GhaeleUnitSymbol = "KMG.Summoning.Unit.GhaeleAzata";
        internal const string GhaeleRaySymbol = "KMG.Summoning.Special.GhaeleAzata.LightRay";
        internal const string GhaeleRayAiSymbol = "KMG.Summoning.Special.GhaeleAzata.LightRayAi";
        internal const string GhaeleBrainSymbol = "KMG.Summoning.Special.GhaeleAzata.Brain";

        /// <summary>
        /// Repair R3 UI-03. The KMG Ghaele is cloned from native
        /// AzataGhaelSummoned and inherited its native light ray
        /// (33e89979...), which has no name, description or icon: the owner's
        /// blank "Special" action. The native ray stays untouched; the KMG
        /// Ghaele alone gets an owned clone (same mechanics) with a name, an
        /// accurate description read from the ray's own data, and native
        /// Searing Light art, plus the matching AI action and brain so it still
        /// uses the ray. Only references the KMG unit itself owns are swapped;
        /// if the grant path differs nothing is swapped and the reason is
        /// logged for the native observation.
        /// </summary>
        private static void ConfigureGhaeleLightRay(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, GhaeleUnitSymbol);
            BlueprintAbility native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, NativeRayGuid, "native Ghaele two-projectile light ray");
            BlueprintAiCastSpell nativeAi = BlueprintLibraryLookup.RequireExact<BlueprintAiCastSpell>(
                library, NativeRayAiGuid, "native Ghaele light-ray AI action");
            BlueprintAbility ray = Require<BlueprintAbility>(bySymbol, GhaeleRaySymbol);
            BlueprintAiCastSpell ai = Require<BlueprintAiCastSpell>(bySymbol, GhaeleRayAiSymbol);
            BlueprintBrain brain = Require<BlueprintBrain>(bySymbol, GhaeleBrainSymbol);

            ExpandedSummoningAbilityBuilder.CopyFields(native, ray);
            ray.name = InternalName(GhaeleRaySymbol);
            ray.ComponentsArray = (native.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(ray,
                LocalizationService.Create(GhaeleRaySymbol + ".Name", "Light Ray"),
                LocalizationService.Create(GhaeleRaySymbol + ".Description", GhaeleRayDescription(native)),
                NativeAbilityIcon(library, NativeSearingLightGuid, "native Searing Light icon"));
            AlternateSummoningAdapterBuilder.ResetComponentCaches(ray);

            CopyFields(nativeAi, ai);
            ai.name = InternalName(GhaeleRayAiSymbol);
            ai.Ability = ray;

            int factSwaps = 0;
            if (unit.AddFacts != null)
                for (int index = 0; index < unit.AddFacts.Length; index++)
                    if (ReferenceEquals(unit.AddFacts[index], native)) { unit.AddFacts[index] = ray; factSwaps++; }
            foreach (AddFacts component in (unit.ComponentsArray ?? Array.Empty<BlueprintComponent>()).OfType<AddFacts>())
                if (component.Facts != null)
                    for (int index = 0; index < component.Facts.Length; index++)
                        if (ReferenceEquals(component.Facts[index], native)) { component.Facts[index] = ray; factSwaps++; }

            BlueprintBrain nativeBrain = unit.Brain;
            bool brainSwap = false;
            if (factSwaps > 0 && nativeBrain != null && nativeBrain.Actions != null &&
                nativeBrain.Actions.Any(action => ReferenceEquals(action, nativeAi)))
            {
                CopyFields(nativeBrain, brain);
                brain.name = InternalName(GhaeleBrainSymbol);
                brain.Actions = nativeBrain.Actions.Select(action =>
                    ReferenceEquals(action, nativeAi) ? (BlueprintAiAction)ai : action).ToArray();
                unit.Brain = brain;
                brainSwap = true;
            }
            GhaeleRayGrant = "factSwaps=" + factSwaps + ";brainSwap=" + brainSwap +
                ";nativeBrain=" + (nativeBrain == null ? "<none>" : nativeBrain.name);
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("summoning", "r3.ghaele-light-ray", GhaeleRayGrant);
        }

        internal static string GhaeleRayGrant { get; private set; }

        private static string GhaeleRayDescription(BlueprintAbility native)
        {
            BlueprintComponent[] components = native.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            AbilityDeliverProjectile delivery = components.OfType<AbilityDeliverProjectile>().FirstOrDefault();
            int rays = delivery?.Projectiles?.Length ?? 0;
            string text = rays > 1 ? "Fires " + rays + " ranged-touch rays of light" : "Fires a ranged-touch ray of light";
            text += " at one enemy within " + (native.Range == AbilityRange.Long ? "long" :
                native.Range.ToString().ToLowerInvariant()) + " range";
            try
            {
                ContextActionDealDamage damage = FindDamage(components);
                ContextDiceValue value = damage.Value;
                if (value != null && value.DiceCountValue != null &&
                    value.DiceCountValue.ValueType == ContextValueType.Simple &&
                    (value.BonusValue == null || value.BonusValue.ValueType == ContextValueType.Simple))
                {
                    int count = value.DiceCountValue.Value;
                    int bonus = value.BonusValue == null ? 0 : value.BonusValue.Value;
                    string dice = count + value.DiceType.ToString().ToLowerInvariant() +
                        (bonus > 0 ? "+" + bonus : "");
                    text += (rays > 1 ? "; each ray deals " : "; it deals ") + dice + " damage";
                }
            }
            catch (InvalidOperationException) { }
            return text + (native.SpellResistance ? "." : ". Spell resistance does not apply.");
        }
    }
}
