using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal static string NativeMephitBreathGrants { get; private set; }

        internal static string NativeMephitBreathSymbol(string key)
        { return "KMG.Summoning.Special." + MephitToken(key) + ".Breath"; }

        /// <summary>
        /// Repair R4 MEPH-01..03. The four native-backed KMG mephits are clones
        /// of the native summoned mephits and receive the shared native breath
        /// through their own AddFacts array (CloneUnitShell builds a new array;
        /// only the entries are shared) and the shared native brain's one breath
        /// cast action. The installed breaths deal electricity (air, water) and
        /// acid (earth), and wear Burning Hands. Each unit gets an owned clone
        /// (same delivery, cone length, save, dice, targeting and action cost)
        /// with the required damage, the matching native cone and its own art,
        /// redirected only through that unit's AddFacts entry and an owned brain
        /// copy whose single native breath action is replaced. The native
        /// breaths, their AI actions, the native brains, the donor units and
        /// every other action are never written. Any other grant shape fails
        /// closed.
        /// </summary>
        private static void ConfigureNativeMephitBreaths(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            var log = new List<string>();
            foreach (MephitBreathRow row in MephitBreathPolicy.NativeBackedRows)
            {
                string token = MephitToken(row.Key);
                string prefix = "KMG.Summoning.Special." + token + ".";
                BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit." + token);
                BlueprintUnit donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library,
                    ExpandedSummoningDonorCatalog.For(row.Key).Guid, "native summoned " + row.Key + " donor");
                BlueprintAbility native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                    row.NativeBreathGuid, "native " + row.Key + " breath");
                BlueprintAbility breath = Require<BlueprintAbility>(bySymbol, prefix + "Breath");
                BlueprintAiCastSpell ai = Require<BlueprintAiCastSpell>(bySymbol, prefix + "BreathAi");
                BlueprintBrain brain = Require<BlueprintBrain>(bySymbol, prefix + "Brain");

                BlueprintUnitFact[] facts = unit.AddFacts ?? Array.Empty<BlueprintUnitFact>();
                if (donor.AddFacts != null && ReferenceEquals(donor.AddFacts, unit.AddFacts))
                    throw new InvalidOperationException("KMG " + row.Key + " shares its AddFacts array with the native donor; refusing to write it.");
                if ((unit.ComponentsArray ?? Array.Empty<BlueprintComponent>()).Any(component =>
                        component is AddFacts && ((AddFacts)component).Facts != null &&
                            ((AddFacts)component).Facts.Any(value => ReferenceEquals(value, native)) ||
                        component is AddAbilityToCharacterComponent && ((AddAbilityToCharacterComponent)component).Abilities != null &&
                            ((AddAbilityToCharacterComponent)component).Abilities.Any(value => ReferenceEquals(value, native))))
                    throw new InvalidOperationException("KMG " + row.Key + " has an unexpected component grant of the native breath.");
                BlueprintBrain nativeBrain = unit.Brain;
                if (nativeBrain == null || !ReferenceEquals(nativeBrain, donor.Brain) || nativeBrain.Actions == null)
                    throw new InvalidOperationException("KMG " + row.Key + " no longer runs its donor's native brain.");
                BlueprintAiCastSpell[] nativeAis = nativeBrain.Actions.OfType<BlueprintAiCastSpell>()
                    .Where(action => ReferenceEquals(action.Ability, native)).ToArray();
                if (nativeAis.Length != 1)
                    throw new InvalidOperationException("KMG " + row.Key + "'s brain must hold exactly one native breath action.");
                BlueprintAiCastSpell nativeAi = nativeAis[0];

                ExpandedSummoningAbilityBuilder.CopyFields(native, breath);
                breath.name = InternalName(prefix + "Breath");
                breath.ComponentsArray = (native.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .Where(value => value != null)
                    .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
                AbilityDeliverProjectile deliver = breath.ComponentsArray.OfType<AbilityDeliverProjectile>().Single();
                if (deliver.Type != AbilityProjectileType.Cone || deliver.Projectiles == null || deliver.Projectiles.Length != 1)
                    throw new InvalidOperationException("Native " + row.Key + " breath is no longer a single cone.");
                deliver.Projectiles = new[] { BlueprintLibraryLookup.RequireExact<BlueprintProjectile>(library,
                    MephitBreathPolicy.ProjectileFor(row.Required), "native breath cone projectile") };
                AbilityEffectRunAction run = breath.ComponentsArray.OfType<AbilityEffectRunAction>().Single();
                if (run.SavingThrowType != SavingThrowType.Reflex)
                    throw new InvalidOperationException("Native " + row.Key + " breath is no longer Reflex-based.");
                ContextActionDealDamage damage = FindDamage(new BlueprintComponent[] { run });
                ContextDiceValue dice = damage.Value;
                if (!damage.HalfIfSaved || dice == null || dice.DiceCountValue == null ||
                    dice.DiceCountValue.ValueType != ContextValueType.Simple || dice.DiceCountValue.Value <= 0 ||
                    (int)dice.DiceType < 2 || dice.BonusValue != null &&
                        (dice.BonusValue.ValueType != ContextValueType.Simple || dice.BonusValue.Value != 0))
                    throw new InvalidOperationException("Native " + row.Key + " breath dice or save changed.");
                damage.DamageType = MephitBreathDamageType(row.Required);
                SpellDescriptorComponent descriptor = breath.ComponentsArray.OfType<SpellDescriptorComponent>().Single();
                descriptor.Descriptor = new SpellDescriptorWrapper(MephitBreathDescriptor(row.Required));
                BlueprintUnitFactAccess.Resolve().Configure(breath,
                    LocalizationService.Create("KMG.ExpandedSummoning." + token + ".Breath.Name",
                        MephitDisplayName(row.Key) + " Breath"),
                    LocalizationService.Create("KMG.ExpandedSummoning." + token + ".Breath.Description",
                        MephitBreathPolicy.NativeBackedDescription(row, dice.DiceCountValue.Value, (int)dice.DiceType)),
                    NativeAbilityIcon(library, row.IconSourceGuid, "native " + row.IconSourceName + " icon"));
                AlternateSummoningAdapterBuilder.ResetComponentCaches(breath);

                CopyFields(nativeAi, ai);
                ai.name = InternalName(prefix + "BreathAi");
                ai.Ability = breath;

                unit.AddFacts = MephitBreathPolicy.ReplaceExactlyOne(facts,
                    fact => ReferenceEquals(fact, native), (BlueprintUnitFact)breath,
                    "KMG " + row.Key + " AddFacts native breath");
                CopyFields(nativeBrain, brain);
                brain.name = InternalName(prefix + "Brain");
                brain.Actions = MephitBreathPolicy.ReplaceExactlyOne(nativeBrain.Actions,
                    action => ReferenceEquals(action, nativeAi), (BlueprintAiAction)ai,
                    "KMG " + row.Key + " brain native breath action");
                unit.Brain = brain;
                log.Add(row.Key + ":grant=AddFacts;swaps=1;nativeBrain=" + nativeBrain.name +
                    ";ownedBrainActions=" + brain.Actions.Length + ";damage=" + row.Required);
            }
            NativeMephitBreathGrants = string.Join("|", log.ToArray());
            ModContext context;
            if (ModContext.TryGet(out context))
                context.Logger.Info("summoning", "r4.native-mephit-breaths", NativeMephitBreathGrants);
        }

        /// <summary>
        /// Repair R4 MEPH-04: the native Flare Burst blinding burst, retimed to
        /// the ability's single non-target OnApplyEffect spawn and anchored on
        /// the caster. AbilityExecutionProcess spawns it once per executed cast
        /// (after the target loop; per-target calls skip a non-target spawner),
        /// never for a queued or cancelled command and regardless of saves.
        /// The native Flare Burst component is never written.
        /// </summary>
        private static AbilitySpawnFx PyrotechnicsFlash(LibraryScriptableObject library)
        {
            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                MephitBreathPolicy.PyrotechnicsFlashDonorGuid, "native Flare Burst flash donor");
            AbilitySpawnFx[] spawns = (donor.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .OfType<AbilitySpawnFx>().Where(value => value.PrefabLink != null &&
                    value.PrefabLink.AssetId == MephitBreathPolicy.PyrotechnicsFlashPrefabAssetId).ToArray();
            if (spawns.Length != 1)
                throw new InvalidOperationException("Native Flare Burst blinding burst effect not found exactly once.");
            var flash = (AbilitySpawnFx)ExpandedSummoningAbilityBuilder.DeepCloneComponent(spawns[0]);
            flash.Time = AbilitySpawnFxTime.OnApplyEffect;
            flash.Anchor = AbilitySpawnFxAnchor.Caster;
            flash.PositionAnchor = AbilitySpawnFxAnchor.None;
            flash.OrientationAnchor = AbilitySpawnFxAnchor.None;
            flash.WeaponTarget = AbilitySpawnFxWeaponTarget.None;
            flash.DestroyOnCast = false;
            flash.Delay = 0f;
            if (flash.UsesSelectedTarget || flash.PrefabLink == null ||
                flash.PrefabLink.AssetId != MephitBreathPolicy.PyrotechnicsFlashPrefabAssetId)
                throw new InvalidOperationException("Pyrotechnics flash is not a single caster-anchored spawn.");
            return flash;
        }
    }
}
