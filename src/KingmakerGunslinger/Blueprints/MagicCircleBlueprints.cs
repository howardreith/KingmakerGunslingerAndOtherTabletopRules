using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.ProtectionFromAlignment;
using KingmakerGunslinger.Spells.MagicCircle;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class MagicCircleBlueprintSet
    {
        internal MagicCircleBlueprintSet(string alignment, BlueprintAbility spell,
            BlueprintBuff carrier, BlueprintAbilityAreaEffect area, BlueprintBuff recipient)
        { Alignment = alignment; Spell = spell; Carrier = carrier; Area = area; Recipient = recipient; }
        internal string Alignment { get; private set; }
        internal BlueprintAbility Spell { get; private set; }
        internal BlueprintBuff Carrier { get; private set; }
        internal BlueprintAbilityAreaEffect Area { get; private set; }
        internal BlueprintBuff Recipient { get; private set; }
    }

    internal static class MagicCircleBlueprints
    {
        internal const string Prefix = "KMG.Spells.MagicCircle.";
        internal const int IdentityCount = 4;

        // Start with the Evil vertical slice. The other alignments use this same
        // construction contract after native ownership/casting qualification.
        internal static MagicCircleBlueprintSet[] Register(LibraryScriptableObject library,
            BlueprintRegistry registry, bool sharedControlEnabled)
        {
            return new[] { RegisterOne(library, registry, "Evil", AlignmentComponent.Evil,
                SpellDescriptor.Good, "eee384c813b6d74498d1b9cc720d61f4",
                "4a6911969911ce9499bf27dde9bfcedc", sharedControlEnabled) };
        }

        private static MagicCircleBlueprintSet RegisterOne(LibraryScriptableObject library,
            BlueprintRegistry registry, string alignment, AlignmentComponent opposed,
            SpellDescriptor descriptor, string abilityId, string buffId, bool control)
        {
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                abilityId, "native Protection spell presentation");
            var donorBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                buffId, "native Protection buff presentation");
            string symbol = Prefix + alignment + ".";
            // Provisional presentation only; original painted circle art replaces
            // this donor before the feature can receive visual acceptance.
            Sprite icon = donor.Icon;
            var recipient = registry.Register<BlueprintBuff>(symbol + "Recipient",
                () => CreateRecipient(donorBuff, alignment, opposed, icon, control));
            var area = registry.Register<BlueprintAbilityAreaEffect>(symbol + "Area",
                () => CreateArea(alignment, recipient));
            var carrier = registry.Register<BlueprintBuff>(symbol + "Carrier",
                () => CreateCarrier(donorBuff, alignment, icon, area, control));
            area.GetComponent<MagicCircleAreaLifetime>().Carrier = carrier;
            var spell = registry.Register<BlueprintAbility>(symbol + "Ability",
                () => CreateSpell(donor, alignment, descriptor, icon, carrier, control));
            return new MagicCircleBlueprintSet(alignment, spell, carrier, area, recipient);
        }

        private static BlueprintBuff CreateRecipient(BlueprintBuff donor, string alignment,
            AlignmentComponent opposed, Sprite icon, bool control)
        {
            var buff = BlueprintCloneService.Clone(donor, "KMG_MagicCircle_" + alignment + "_Recipient");
            buff.Stacking = StackingType.Stack;
            buff.IsClassFeature = false;
            var ac = ScriptableObject.CreateInstance<ArmorClassBonusAgainstAlignment>();
            ac.name = "$KMG_MagicCircle_Deflection";
            ac.alignment = opposed;
            ac.Value = 2;
            ac.Descriptor = ModifierDescriptor.Deflection;
            var saves = ScriptableObject.CreateInstance<SavingThrowBonusAgainstAlignment>();
            saves.name = "$KMG_MagicCircle_Resistance";
            saves.Alignment = opposed;
            saves.Value = 2;
            saves.Descriptor = ModifierDescriptor.Resistance;
            var ownership = ScriptableObject.CreateInstance<MagicCircleRecipientComponent>();
            ownership.name = "$KMG_MagicCircle_Derivative";
            var components = new List<BlueprintComponent> { ac, saves, ownership };
            if (control)
            {
                var immunity = ScriptableObject.CreateInstance<ProtectionFromAlignmentControlImmunityComponent>();
                immunity.name = "$KMG_MagicCircle_SharedProtection";
                immunity.ProtectedAgainstAlignment = opposed;
                components.Add(immunity);
            }
            buff.ComponentsArray = components.ToArray();
            Configure(buff, alignment, "Recipient", "Magic Circle against " + alignment + " — Within Circle",
                "Protection lasts only while within this circle's 10-foot emanation. Leaving removes only this circle's contribution. " +
                Benefits(alignment, control) + " Dispel the bearer’s timed circle to end its emanation.", icon);
            return buff;
        }

        private static BlueprintAbilityAreaEffect CreateArea(string alignment, BlueprintBuff recipient)
        {
            var area = ScriptableObject.CreateInstance<BlueprintAbilityAreaEffect>();
            area.name = "KMG_MagicCircle_" + alignment + "_Area";
            area.Shape = AreaEffectShape.Cylinder;
            area.Size = 10.Feet();
            area.AffectEnemies = true;
            area.AggroEnemies = false;
            area.AffectDead = false;
            area.IgnoreSleepingUnits = false;
            area.SpellResistance = false;
            area.Fx = new PrefabLink();
            var delivery = ScriptableObject.CreateInstance<AbilityAreaEffectBuff>();
            delivery.name = "$KMG_MagicCircle_AllCoveredCreatures";
            delivery.Buff = recipient;
            delivery.Condition = new ConditionsChecker { Conditions = Array.Empty<Condition>() };
            MagicCircleAreaLifetime.VerifyContract();
            var lifetime = ScriptableObject.CreateInstance<MagicCircleAreaLifetime>();
            lifetime.name = "$KMG_MagicCircle_OriginalCarrierLifetime";
            area.ComponentsArray = new BlueprintComponent[] { delivery, lifetime };
            return area;
        }

        private static BlueprintBuff CreateCarrier(BlueprintBuff donor, string alignment,
            Sprite icon, BlueprintAbilityAreaEffect area, bool control)
        {
            var buff = BlueprintCloneService.Clone(donor, "KMG_MagicCircle_" + alignment + "_Carrier");
            buff.Stacking = StackingType.Stack;
            buff.IsClassFeature = false;
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>();
            emanation.name = "$KMG_MagicCircle_MovingEmanation";
            emanation.AreaEffect = area;
            buff.ComponentsArray = new BlueprintComponent[] { emanation };
            if (buff.StayOnDeath) throw new InvalidOperationException("Magic Circle carrier must end on bearer death.");
            Configure(buff, alignment, "Carrier", "Magic Circle against " + alignment,
                "A 10-foot protective emanation follows this creature for the remaining spell duration. It ends if this creature dies. " + Benefits(alignment, control), icon);
            return buff;
        }

        private static BlueprintAbility CreateSpell(BlueprintAbility donor, string alignment,
            SpellDescriptor descriptor, Sprite icon, BlueprintBuff carrier, bool control)
        {
            var spell = BlueprintCloneService.Clone(donor, "KMG_MagicCircle_" + alignment + "_Ability");
            Configure(spell, alignment, "Ability", "Magic Circle against " + alignment,
                "Touch a creature to create a moving 10-foot emanation for 10 minutes per caster level. " +
                "Every covered creature, including the bearer and enemies, receives its protection. " +
                Benefits(alignment, control) + " This spell does not remove or suppress existing control, exclude summoned creatures, or create a binding circle.", icon);
            spell.Parent = null;
            spell.Type = AbilityType.Spell;
            spell.Range = AbilityRange.Touch;
            spell.CanTargetPoint = false;
            spell.CanTargetFriends = true;
            spell.CanTargetSelf = true;
            spell.CanTargetEnemies = false;
            spell.SpellResistance = false;
            spell.ActionType = UnitCommand.CommandType.Standard;
            spell.Hidden = false;
            spell.ActionBarAutoFillIgnored = false;
            spell.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            spell.EffectOnEnemy = AbilityEffectOnUnit.None;
            spell.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            spell.LocalizedDuration = LocalizationService.Create("KMG.MagicCircle.Duration", "10 minutes/level");
            spell.LocalizedSavingThrow = LocalizationService.Create("KMG.MagicCircle.Save", "Will negates (harmless)");
            spell.AvailableMetamagic = Metamagic.Extend | Metamagic.Quicken | Metamagic.Heighten | Metamagic.Reach;
            var rank = ScriptableObject.CreateInstance<ContextRankConfig>();
            rank.name = "$KMG_MagicCircle_OriginalCasterLevel";
            SetPrivate(rank, "m_Type", AbilityRankType.Default);
            SetPrivate(rank, "m_BaseValueType", ContextRankBaseValueType.CasterLevel);
            SetPrivate(rank, "m_Progression", ContextRankProgression.AsIs);
            var apply = new ContextActionApplyBuff {
                Buff = carrier, IsFromSpell = true, IsNotDispelable = false,
                DurationValue = new ContextDurationValue {
                    Rate = DurationRate.TenMinutes, DiceType = DiceType.Zero,
                    DiceCountValue = 0, BonusValue = new ContextValue {
                        ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.Default } } };
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.name = "$KMG_MagicCircle_ApplyCarrier";
            effect.Actions = new ActionList { Actions = new GameAction[] { apply } };
            var school = ScriptableObject.CreateInstance<SpellComponent>();
            school.name = "$KMG_MagicCircle_Abjuration";
            school.School = SpellSchool.Abjuration;
            var descriptors = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptors.name = "$KMG_MagicCircle_Descriptor";
            descriptors.Descriptor = descriptor;
            var checker = ScriptableObject.CreateInstance<MagicCircleCasterChecker>();
            checker.name = "$KMG_MagicCircle_StartupSetting";
            var components = new List<BlueprintComponent> { school, descriptors, rank, effect, checker };
            // Only native cast presentation is inherited. No list, selector,
            // delivery, resource, immunity, faction, or expiration components.
            components.AddRange(donor.ComponentsArray.OfType<AbilitySpawnFx>());
            spell.ComponentsArray = components.ToArray();
            return spell;
        }

        private static string Benefits(string alignment, bool control)
        {
            return "A +2 deflection bonus to AC and a +2 resistance bonus on saves against " +
                alignment.ToLowerInvariant() + " attacks and effects, using native alignment conditions. " +
                (control ? "It also prevents new qualifying attempts to possess or exercise ongoing mental control from a matching-alignment controller, as Protection from Alignment does. Existing control remains active." :
                    "Expanded control immunity is disabled in the current startup configuration.");
        }

        private static void Configure(Kingmaker.Blueprints.Facts.BlueprintUnitFact fact,
            string alignment, string role, string name, string description, Sprite icon)
        {
            string key = "KMG.MagicCircle." + alignment + "." + role;
            BlueprintUnitFactAccess.Resolve().Configure(fact,
                LocalizationService.Create(key + ".Name", name),
                LocalizationService.Create(key + ".Description", description), icon);
        }

        private static void SetPrivate(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException(target.GetType().FullName, name);
            field.SetValue(target, value);
        }
    }

    // The native area owns this derivative instance. Only the timed carrier is
    // independently dispellable; entry is not another spell cast or resource debit.
    internal sealed class MagicCircleRecipientComponent : BuffLogic
    {
        public override void OnFactActivate() { Buff.IsNotDispelable = true; }
    }

    internal sealed class MagicCircleCasterChecker : BlueprintComponent, IAbilityCasterChecker
    {
        public bool CorrectCaster(UnitEntityData caster)
        { return caster != null && BlueprintBootstrap.MagicCirclePublication != null; }
        public string GetReason()
        { return LocalizationService.Create("KMG.MagicCircle.Disabled", "Magic Circle spells are disabled in the current startup configuration."); }
    }
}
