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

        internal const string GhaeleDonorGuid = "bc8ca1437c0f48948b317b7e64febf0d";

        /// <summary>
        /// Repair R3 UI-03. The KMG Ghaele is cloned from native
        /// AzataGhaelSummoned and receives native AzataGhaelLightRay
        /// (33e89979..., no name, description or icon) through the unit's own
        /// <c>AddAbilityToCharacterComponent.Abilities</c>: UnitDescriptor adds
        /// every listed ability at unit creation, unconditionally. CloneUnitShell
        /// deep-clones each direct unit component and its arrays, so that
        /// component and array belong to the KMG unit; only the blueprint
        /// references inside are shared. This redirects exactly that one KMG
        /// entry to an owned clone (same components and therefore the same
        /// action cost, targeting, range, delivery, damage, resistance and
        /// conditions) with a name, accurate description and native Searing
        /// Light art. The native brain is shared, so the KMG unit gets an owned
        /// copy whose one native ray action is replaced by the matching owned AI
        /// action; every other action and scorer is kept. The donor's component,
        /// its array, the native ray, its AI action and the native brain are
        /// never written. Any other grant shape fails closed.
        /// </summary>
        private static void ConfigureGhaeleLightRay(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, GhaeleUnitSymbol);
            BlueprintUnit donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(
                library, GhaeleDonorGuid, "native AzataGhaelSummoned donor");
            BlueprintAbility native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, NativeRayGuid, "native Ghaele two-projectile light ray");
            BlueprintAiCastSpell nativeAi = BlueprintLibraryLookup.RequireExact<BlueprintAiCastSpell>(
                library, NativeRayAiGuid, "native Ghaele light-ray AI action");
            BlueprintAbility ray = Require<BlueprintAbility>(bySymbol, GhaeleRaySymbol);
            BlueprintAiCastSpell ai = Require<BlueprintAiCastSpell>(bySymbol, GhaeleRayAiSymbol);
            BlueprintBrain brain = Require<BlueprintBrain>(bySymbol, GhaeleBrainSymbol);

            BlueprintComponent[] donorComponents = donor.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            AddAbilityToCharacterComponent[] grants = (unit.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AddAbilityToCharacterComponent>()
                .Where(component => component.Abilities != null &&
                    component.Abilities.Any(value => ReferenceEquals(value, native))).ToArray();
            if (grants.Length != 1 ||
                grants[0].Abilities.Count(value => ReferenceEquals(value, native)) != 1)
                throw new InvalidOperationException(
                    "KMG Ghaele must receive the native light ray through exactly one owned AddAbilityToCharacterComponent entry.");
            AddAbilityToCharacterComponent grant = grants[0];
            if (donorComponents.Any(component => ReferenceEquals(component, grant)) ||
                donorComponents.OfType<AddAbilityToCharacterComponent>().Any(component =>
                    ReferenceEquals(component.Abilities, grant.Abilities)))
                throw new InvalidOperationException("KMG Ghaele's ray grant is shared with the native donor; refusing to write it.");
            if ((unit.AddFacts ?? Array.Empty<BlueprintUnitFact>()).Any(value => ReferenceEquals(value, native)) ||
                (unit.ComponentsArray ?? Array.Empty<BlueprintComponent>()).OfType<AddFacts>().Any(component =>
                    component.Facts != null && component.Facts.Any(value => ReferenceEquals(value, native))))
                throw new InvalidOperationException("KMG Ghaele has an unexpected second native light-ray grant.");
            BlueprintBrain nativeBrain = unit.Brain;
            if (nativeBrain == null || nativeBrain.Actions == null ||
                nativeBrain.Actions.Count(action => ReferenceEquals(action, nativeAi)) != 1 ||
                !ReferenceEquals(nativeAi.Ability, native))
                throw new InvalidOperationException("KMG Ghaele's brain must hold exactly one native light-ray AI action.");

            ExpandedSummoningAbilityBuilder.CopyFields(native, ray);
            ray.name = InternalName(GhaeleRaySymbol);
            ray.ComponentsArray = (native.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(ray,
                LocalizationService.Create(GhaeleRaySymbol + ".Name", "Light Ray"),
                LocalizationService.Create(GhaeleRaySymbol + ".Description", GhaeleRayDescription(native)),
                NativeAbilityIcon(library, NativeSearingLightGuid, "native Searing Light icon"));
            // Instantaneous, and no saving throw applies: supported omission
            // (an empty string), never the literal <null> a missing string shows.
            ray.LocalizedDuration = LocalizationService.Create(GhaeleRaySymbol + ".Duration", "");
            ray.LocalizedSavingThrow = LocalizationService.Create(GhaeleRaySymbol + ".SavingThrow", "");
            AlternateSummoningAdapterBuilder.ResetComponentCaches(ray);

            CopyFields(nativeAi, ai);
            ai.name = InternalName(GhaeleRayAiSymbol);
            ai.Ability = ray;

            grant.Abilities = grant.Abilities.Select(value =>
                ReferenceEquals(value, native) ? ray : value).ToArray();
            CopyFields(nativeBrain, brain);
            brain.name = InternalName(GhaeleBrainSymbol);
            brain.Actions = nativeBrain.Actions.Select(action =>
                ReferenceEquals(action, nativeAi) ? (BlueprintAiAction)ai : action).ToArray();
            unit.Brain = brain;
            GhaeleRayGrant = "path=AddAbilityToCharacterComponent.Abilities;swaps=1;nativeBrain=" +
                nativeBrain.name + ";ownedBrainActions=" + brain.Actions.Length;
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
