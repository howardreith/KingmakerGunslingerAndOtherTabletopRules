using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static BlueprintBuff ConfigureInfernalWound34(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string p = Barbazu34Prefix;
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                NativeBleed2d6Guid, "native bleed damage consumer");
            var buff = Require<BlueprintBuff>(symbols, p + "InfernalWound");
            CopyFields(donor, buff); buff.name = InternalName(p + "InfernalWound");
            buff.Stacking = StackingType.Stack; buff.Frequency = DurationRate.Rounds;
            SetBuffFlags(buff, true, false);
            var wound = ScriptableObject.CreateInstance<InfernalWound34>();
            var components = new List<BlueprintComponent>();
            var cotw = donor.ComponentsArray.SingleOrDefault(value =>
                value.GetType().FullName == InfernalWound34.CotwBleedType);
            if (cotw != null) {
                // This installed compatibility consumer owns the shared strongest-
                // bleed selector. Clone it privately; do not replace that policy.
                var bleed = ExpandedSummoningAbilityBuilder.DeepCloneComponent(cotw);
                SetField(bleed, "dice_value", new ContextDiceValue {
                    DiceType = DiceType.Zero, DiceCountValue = Simple(0), BonusValue = Simple(2) });
                components.Add(bleed);
            } else {
                var rounds = donor.GetComponent<AddFactContextActions>();
                // Take only the unconditional HP payload, not the ordinary
                // donor's out-of-combat removal or activation pruning.
                var damage = (ContextActionDealDamage)ExpandedSummoningAbilityBuilder.DeepCloneComponentObject(NativeBleedPayload34(donor));
                // The stock 2d6 donor names its ordinary nonstacking family by
                // removing the smaller native bleeds on activation. All these
                // dice are stronger than bleed2; retain our owned marker but
                // yield its tick while one of those native facts is active.
                wound.StrongerNativeBleeds = new[] { donor }.Concat(
                    (rounds.Activated?.Actions ?? Array.Empty<GameAction>()).OfType<ContextActionRemoveBuff>()
                        .Select(action => action.Buff)).Distinct().ToArray();
                foreach (var other in wound.StrongerNativeBleeds) {
                    var value = NativeBleedPayload34(other).Value;
                    if (value?.DiceCountValue == null || value.BonusValue == null ||
                        value.DiceCountValue.ValueType != ContextValueType.Simple || value.BonusValue.ValueType != ContextValueType.Simple)
                        throw new InvalidOperationException("Stock bleed family no longer has fixed native dice.");
                    var dice = new DiceFormula(value.DiceCountValue.Value, value.DiceType);
                    if (dice.MinValue(0, false) + dice.MaxValue(0, false) + 2 * value.BonusValue.Value <= 2 * InfernalRules34.WoundBleed)
                        throw new InvalidOperationException("Stock bleed family is no longer stronger than Infernal Wound.");
                }
                damage.Value = new ContextDiceValue { DiceType = DiceType.Zero,
                    DiceCountValue = Simple(0), BonusValue = Simple(2) };
                damage.Duration = new ContextDurationValue { Rate = DurationRate.Rounds,
                    DiceType = DiceType.Zero, DiceCountValue = Simple(0), BonusValue = Simple(0) };
                wound.BleedActions = new ActionList { Actions = new GameAction[] { damage } };
            }
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>(); descriptor.Descriptor = SpellDescriptor.Bleed;
            // Native bleed precedes closure, so deactivation can reselect a
            // still-owned sibling after the native selector releases this fact.
            components.Add(wound); components.Add(descriptor);
            components.Add(ScriptableObject.CreateInstance<InfernalEffectLifetime34>());
            buff.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(p + "InfernalWound.Name", "Infernal Wound"),
                LocalizationService.Create(p + "InfernalWound.Description",
                    "Bleed 2. Spell healing requires a DC16 caster-level check; failure restores no HP, while success heals normally and closes the bleeding. Ends when the summoned source or summoner leaves the area. The printed Heal DC17 option has no implemented skill action."),
                ExpandedSummoningProjectIcons.Require("bearded-devil"));
            return buff;
        }
        private static ContextActionDealDamage NativeBleedPayload34(BlueprintBuff blueprint)
        {
            var actions = blueprint?.GetComponent<AddFactContextActions>()?.NewRound?.Actions ?? Array.Empty<GameAction>();
            var payloads = actions.OfType<ContextActionDealDamage>().ToArray();
            if (payloads.Length != 1 || payloads[0].DamageType.Type != DamageType.Direct ||
                !(bool)typeof(ContextActionDealDamage).GetProperty("IsSimpleDamage",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(payloads[0], null))
                throw new InvalidOperationException("Native bleed must have one direct HP round payload: " + blueprint?.AssetGuid);
            return payloads[0];
        }
    }
}
