using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static BlueprintAbility[][] LillendKnownSpells32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            return new[] {
                new[] { ConfigureLillendDaze32(library, bySymbol),
                    ConfigureLillendLullaby32(library, bySymbol) },
                new[] { LillendOwnedControlSpell32(library, bySymbol, false),
                    LillendNativeSpell32(library, "5590652e1c2225c4ca30c4a699ab3649", "Cure Light Wounds"),
                    LillendOwnedControlSpell32(library, bySymbol, true) },
                new[] { LillendOwnedHoldPerson32(library, bySymbol),
                    LillendNativeSpell32(library, "89940cde01689fb46946b2f8cd7b66b7", "Invisibility"),
                    ConfigureLillendSoundBurst32(library, bySymbol) },
                new[] { ConfigureLillendCharmMonster32(library, bySymbol),
                    LillendNativeSpell32(library, "3361c5df793b4c8448756146a88026ad", "Cure Serious Wounds") }
            };
        }

        private static BlueprintAbility LillendNativeSpell32(LibraryScriptableObject library, string guid, string title)
        {
            var spell = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library, guid, "captured Lillend " + title);
            if (spell.Type != AbilityType.Spell) throw new InvalidOperationException(title + " must remain a real spell.");
            return spell;
        }

        private static BlueprintAbility ConfigureLillendSoundBurst32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            string symbol = Lillend32Prefix + "SoundBurst";
            var ability = Require<BlueprintAbility>(bySymbol, symbol);
            CloneMephitSpell(library, ability, "c3893092a333b93499fd0a21845aa265", "native Sound Burst delivery");
            ability.name = InternalName(symbol);
            var run = ability.GetComponents<AbilityEffectRunAction>().Single();
            var saved = run.Actions.Actions.OfType<ContextActionConditionalSaved>().Single();
            var damage = run.Actions.Actions.OfType<ContextActionDealDamage>().Single();
            var stun = saved.Failed.Actions.OfType<ContextActionApplyBuff>().Single();
            // Native67 captured one unconditional1d12 sonic damage leaf and
            // one failed-Fort one-round stun. Change only this private damage
            // die to PF1's1d8. Admit only that canonical die or observedD12;
            // profile attribution ofD12 is unknown. The native spell is untouched.
            if (ability.Type != AbilityType.Spell || run.SavingThrowType != Kingmaker.EntitySystem.Stats.SavingThrowType.Fortitude ||
                run.Actions.Actions.Length != 2 || saved.Succeed.Actions.Length != 0 || saved.Failed.Actions.Length != 1 ||
                damage.DamageType.Type != Kingmaker.RuleSystem.Rules.Damage.DamageType.Energy ||
                damage.DamageType.Energy != Kingmaker.Enums.Damage.DamageEnergyType.Sonic ||
                (damage.Value.DiceType != Kingmaker.RuleSystem.DiceType.D12 && damage.Value.DiceType != Kingmaker.RuleSystem.DiceType.D8) ||
                damage.Value.DiceCountValue.ValueType != ContextValueType.Simple || damage.Value.DiceCountValue.Value != 1 ||
                damage.Value.BonusValue.ValueType != ContextValueType.Simple || damage.Value.BonusValue.Value != 0 ||
                damage.Half || damage.HalfIfSaved || damage.AlreadyHalved || !damage.IsAoE ||
                stun.Buff?.AssetGuid != "09d39b38bb7c6014394b6daced9bacd3" || stun.Permanent || stun.UseDurationSeconds || stun.IsFromSpell ||
                stun.DurationValue.Rate != DurationRate.Rounds || stun.DurationValue.DiceType != Kingmaker.RuleSystem.DiceType.Zero ||
                stun.DurationValue.BonusValue.ValueType != ContextValueType.Simple || stun.DurationValue.BonusValue.Value != 1)
                throw new InvalidOperationException("Observed native Sound Burst damage/save/stun graph changed.");
            damage.Value.DiceType = Kingmaker.RuleSystem.DiceType.D8;
            // Instantaneous spell consequences keep their native one-round
            // duration, including after source loss; no ongoing-control cleanup.
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", "Sound Burst"),
                LocalizationService.Create(symbol + ".Description",
                    "Deal 1d8 sonic damage in a 10-foot burst. A failed Fortitude save also stuns for 1 round. Spell resistance applies."), ability.Icon);
            return ability;
        }

        private static BlueprintAbility ConfigureLillendCharmMonster32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            string symbol = Lillend32Prefix + "CharmMonster";
            var ability = Require<BlueprintAbility>(bySymbol, symbol);
            CloneMephitSpell(library, ability, "1af9d5995090e5a4185a30decf0959ad", "native Charm Person control mapping");
            ability.name = InternalName(symbol); ability.Type = AbilityType.Spell;
            // Charm Monster broadens only the humanoid type restriction.
            // Retain the native mindless/undead checks and caster exceptions.
            var allowedTypes = new HashSet<string>(StringComparer.Ordinal) {
                "3bec99efd9a363242a6c8d9957b75e91", "455ac88e22f55804ab87c2467deff1d6",
                "018af8005220ac94a9a4f47b3e9c2b4e", "9054d3988d491d944ac144e27b6bc318",
                "a95311b3dc996964cbaa30ff9965aaf6", "57614b50e8d86b24395931fffc5e409b",
                "625827490ea69d84d8e599a33929fdc6" };
            int removed = 0;
            foreach (var check in ability.GetComponents<AbilityTargetHasFact>())
            {
                if (!check.Inverted) throw new InvalidOperationException("Unexpected positive Charm type checker.");
                removed += check.CheckedFacts.Count(fact => allowedTypes.Contains(fact.AssetGuid));
                check.CheckedFacts = check.CheckedFacts.Where(fact => !allowedTypes.Contains(fact.AssetGuid)).ToArray();
            }
            foreach (var check in ability.GetComponents<AbilityTargetHasNoFactUnless>())
            {
                removed += check.CheckedFacts.Count(fact => allowedTypes.Contains(fact.AssetGuid));
                check.CheckedFacts = check.CheckedFacts.Where(fact => !allowedTypes.Contains(fact.AssetGuid)).ToArray();
            }
            if (removed != allowedTypes.Count) throw new InvalidOperationException("Native Charm type contract changed.");
            BindFeyCelestialCharm32(ability, symbol, bySymbol, DurationRate.Days);
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", "Charm Monster"),
                LocalizationService.Create(symbol + ".Description",
                    "Charm any eligible living creature using the native Charm control behavior. Will negates; spell resistance applies. Duration: one day per caster level."), ability.Icon);
            return ability;
        }

        private static IEnumerable<ContextActionApplyBuff> LillendCharmApplications32(ActionList list)
        {
            foreach (var action in list.Actions)
            {
                var apply = action as ContextActionApplyBuff;
                if (apply != null) { yield return apply; continue; }
                var conditional = action as Conditional;
                var save = action as ContextActionSavingThrow;
                var saved = action as ContextActionConditionalSaved;
                ActionList[] children = conditional != null ? new[] { conditional.IfTrue, conditional.IfFalse } :
                    save != null ? new[] { save.Actions } : saved != null ? new[] { saved.Succeed, saved.Failed } : null;
                if (children == null) throw new InvalidOperationException("Unexpected native Charm action " + action.GetType().Name);
                foreach (var child in children)
                    foreach (var leaf in LillendCharmApplications32(child)) yield return leaf;
            }
        }

        private static void AddLillendSpellsAndAi32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintUnit unit, BlueprintSpellbook book,
            BlueprintAbility[][] spells)
        {
            string slaSymbol = Lillend32Prefix + "CharmPersonSla";
            var sla = Require<BlueprintAbility>(bySymbol, slaSymbol);
            var resource = Require<BlueprintAbilityResource>(bySymbol, Lillend32Prefix + "CharmPersonSlaResource");
            ConfigureNamedResource(resource, Lillend32Prefix + "CharmPersonSlaResource", Lillend32Prefix + "CharmPersonSlaResource",
                "Charm Person", "One innate use for this Lillend, independent of bard spell slots.", 1);
            Sprint29LimitedNativeSpell(library, sla, slaSymbol, "1af9d5995090e5a4185a30decf0959ad", 7, 1, resource, "Charm Person");
            BindFeyCelestialCharm32(sla, slaSymbol, bySymbol, DurationRate.Hours);
            var traits = Require<Kingmaker.Blueprints.Classes.BlueprintFeature>(bySymbol, Lillend32Prefix + "Traits");
            var parameters = ScriptableObject.CreateInstance<Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit; parameters.Abilities = new[] { sla };
            parameters.SpellLevels = new[] { 1 }; parameters.CasterLevel = 7;
            if (spells[0].Any(spell => spell.Type != AbilityType.Spell || spell.GetComponent<CantripComponent>() == null))
                throw new InvalidOperationException("Lillend level-zero spells must be native Spell/Cantrip facts.");
            var cantrips = ScriptableObject.CreateInstance<LillendCantripParameters32>();
            cantrips.OwningBlueprint = unit; cantrips.Book = book; cantrips.Cantrips = spells[0];
            traits.ComponentsArray = traits.ComponentsArray.Concat(new BlueprintComponent[] { AddResource(resource), parameters, cantrips }).ToArray();
            unit.AddFacts = unit.AddFacts.Concat(new BlueprintUnitFact[] { sla }).Concat(spells[0]).ToArray();
            var actions = new List<BlueprintAiAction>();
            var choices = new[] { LillendSpellChoice32.Daze, LillendSpellChoice32.CharmPerson, LillendSpellChoice32.CureLight,
                LillendSpellChoice32.Sleep, LillendSpellChoice32.HoldPerson, LillendSpellChoice32.Invisibility,
                LillendSpellChoice32.SoundBurst, LillendSpellChoice32.CharmMonster, LillendSpellChoice32.CharmPersonSla,
                LillendSpellChoice32.Lullaby };
            var selected = new[] { spells[0][0], spells[1][0], spells[1][1], spells[1][2], spells[2][0], spells[2][1],
                spells[2][2], spells[3][0], sla, spells[0][1] };
            int[] levels = { 0, 1, 1, 1, 2, 2, 2, 3, 1, 0 };
            int[] scores = { 2, 3, 6, 3, 4, 7, 4, 3, 4, 4 };
            for (int i = 0; i < choices.Length; i++)
            {
                string key = Lillend32Prefix + choices[i];
                var ai = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol, key + "Ai"), key + "Ai", selected[i], 1);
                var eligibility = Require<LillendSpellEligibility32>(bySymbol, key + "Eligibility");
                eligibility.OwningBlueprint = unit; eligibility.Book = book; eligibility.Spell = selected[i];
                eligibility.Level = levels[i]; eligibility.Choice = choices[i];
                eligibility.ControlBuffGuid = Require<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>(bySymbol, FeyCharmBuff32).AssetGuid;
                if (choices[i] == LillendSpellChoice32.Daze)
                    eligibility.RefusedBuff = Require<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>(bySymbol,
                        Lillend32Prefix + "DazeImmunity");
                if (choices[i] == LillendSpellChoice32.Lullaby)
                {
                    eligibility.RefusedBuff = Require<Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff>(bySymbol,
                        Lillend32Prefix + "LullabyBuff");
                    eligibility.SleepSpell = spells[1][2];
                }
                ai.BaseScore = scores[i]; ai.TargetConsiderations = new Consideration[] { eligibility };
                actions.Add(ai);
            }
            unit.Brain.Actions = unit.Brain.Actions.Concat(actions).ToArray();
        }
    }
}
