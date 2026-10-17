using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningNaturalBuilder
    {
        /// <summary>
        /// The Sprint 25 giant bodies. Only Brachiosaurus builds private
        /// carriers here: the released Elephant already rides the native
        /// Mastodon weapons and needs no new blueprint, and this sprint
        /// corrected its attack lines rather than its identities.
        ///
        /// <para>This runs before the profile loop because the profile row
        /// names weapon key "BrachiosaurusTail4d6", and a profile whose weapon
        /// cannot resolve makes blueprint initialization throw and roll back
        /// every registration. The row and these carriers are one change.</para>
        /// </summary>
        private static void ConfigureGiantBodies(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            GiantBodyRulesPolicy.Validate();

            // The tail is 4d6, which is the same dice and the same native
            // animated-tail donor the released Stegosaurus tail uses. Reusing
            // the measured donor is deliberate: nothing new is borrowed to
            // animate a tail this project has already animated once.
            BlueprintItemWeapon tail = Require<BlueprintItemWeapon>(bySymbol,
                GiantBodyRulesPolicy.BrachiosaurusTailSymbol);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                    BlueprintItemWeapon>(library, NativeTail1d8Guid,
                        "native animated tail weapon"), tail,
                GiantBodyRulesPolicy.BrachiosaurusTailSymbol,
                GiantBodyRulesPolicy.BrachiosaurusTailDiceCount,
                Kingmaker.RuleSystem.DiceType.D6);

            // Its own weapon type, for the same reason the armored dinosaurs
            // needed one: the shared native tail type cannot carry a reach
            // this creature does not share with its donor. The guard inside
            // the shared routine still requires the stored nine to expose
            // five feet before body reach.
            ConfigureArmoredDinosaurWeaponType(tail,
                Require<BlueprintWeaponType>(bySymbol,
                    GiantBodyRulesPolicy.BrachiosaurusTailTypeSymbol),
                GiantBodyRulesPolicy.BrachiosaurusTailTypeSymbol,
                GiantBodyRulesPolicy.BrachiosaurusTailTypeStoredRangeFeet);

            ConfigureBrachiosaurusChassis(library,
                Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit.Brachiosaurus"),
                tail, Require<BlueprintFeature>(bySymbol,
                    GiantBodyRulesPolicy.BrachiosaurusChassisSymbol));

            // The unit type carries no signature ability. Brachiosaurus is
            // untemplated and owns no execution children, and its trample is
            // registered as a command by the special builder rather than
            // advertised here.
            ConfigureTheropodUnitType(Require<BlueprintUnitType>(bySymbol,
                    GiantBodyRulesPolicy.BrachiosaurusUnitTypeSymbol),
                GiantBodyRulesPolicy.BrachiosaurusUnitTypeSymbol,
                "Brachiosaurus",
                "A gargantuan long-necked sauropod that browses high canopy, "
                    + "built on columnar legs and balanced by a heavy tail.",
                Array.Empty<BlueprintUnitFact>());
        }

        /// <summary>
        /// Carries the final five feet of the adult's twenty-foot reach on top
        /// of the fifteen a Gargantuan body already exposes, the quadruped
        /// defence against trip, and the high-rank half of Skill Focus
        /// (Perception) that ranks alone cannot express.
        /// </summary>
        private static void ConfigureBrachiosaurusChassis(
            LibraryScriptableObject library, BlueprintUnit owner,
            BlueprintItemWeapon tail, BlueprintFeature feature)
        {
            var reach = ScriptableObject.CreateInstance<AddStatBonus>();
            reach.Stat = StatType.Reach;
            reach.Value = GiantBodyRulesPolicy.BrachiosaurusReachBonusFeet;
            reach.Descriptor = ModifierDescriptor.UntypedStackable;
            // As with Stegosaurus, the private weapon type cannot match the
            // native WeaponFocusTail type reference. Carry that same +1 on
            // this creature's private tail type; do not mutate the native feat.
            var focus = ScriptableObject.CreateInstance<WeaponFocus>();
            focus.WeaponType = tail.Type;
            focus.AttackBonus = GiantBodyRulesPolicy.BrachiosaurusWeaponFocusTailBonus;
            var powerAttack = BlueprintLibraryLookup.RequireExact<
                Kingmaker.UnitLogic.ActivatableAbilities.BlueprintActivatableAbility>(
                    library, "a7b339e4f6ff93a4697df5d7a87ff619", "native Power Attack toggle");
            if (powerAttack.Buff == null)
                throw new InvalidOperationException("Native Power Attack toggle has no buff.");
            var power = ScriptableObject.CreateInstance<BrachiosaurusPowerAttackCorrection>();
            power.OwningBlueprint = owner;
            power.Tail = tail;
            power.PowerAttackBuff = powerAttack.Buff;
            feature.name = InternalName(
                GiantBodyRulesPolicy.BrachiosaurusChassisSymbol);
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.ComponentsArray = new BlueprintComponent[] {
                reach, focus, power,
                UntypedSkill(StatType.SkillPerception,
                    GiantBodyRulesPolicy
                        .BrachiosaurusSkillFocusCompletionBonus)
            };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Brachiosaurus.Chassis.Name",
                    "Brachiosaurus Chassis"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Brachiosaurus.Chassis.Description",
                    "Carries the adult brachiosaurus's twenty-foot reach and "
                        + "the high-rank half of Skill Focus (Perception)."),
                null);
        }
    }
}
