using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class ContextActionApplySummonTemplateByCaster : ContextAction
    {
        public BlueprintBuff CelestialTemplate;
        public BlueprintBuff FiendishTemplate;
        public BlueprintBuff CelestialSmite;
        public BlueprintBuff FiendishSmite;
        public BlueprintBuff NeutralFiendishMode;
        public BlueprintBuff[] ReplacedNativeTemplateBuffs =
            Array.Empty<BlueprintBuff>();

        public override string GetCaption()
        {
            return "Apply caster-selected celestial or fiendish summon template";
        }

        public override void RunAction()
        {
            UnitEntityData target = Target == null ? null : Target.Unit;
            UnitEntityData caster = Context == null ? null : Context.MaybeCaster;
            if (target == null || target.Descriptor == null || caster == null ||
                caster.Descriptor == null || caster.Descriptor.Alignment == null ||
                target.Descriptor.Alignment == null || CelestialTemplate == null ||
                FiendishTemplate == null || CelestialSmite == null ||
                FiendishSmite == null) return;

            bool neutralFiendish = NeutralFiendishMode != null &&
                caster.Descriptor.Buffs.GetBuff(NeutralFiendishMode) != null;
            SummonAlignmentMode mode = SummonTemplateSelectionPolicy.Select(
                (int)caster.Descriptor.Alignment.Value, neutralFiendish);
            BlueprintBuff template = mode == SummonAlignmentMode.Celestial ?
                CelestialTemplate : FiendishTemplate;
            BlueprintBuff smite = mode == SummonAlignmentMode.Celestial ?
                CelestialSmite : FiendishSmite;

            Remove(target, ReplacedNativeTemplateBuffs);
            Remove(target, new[] { CelestialTemplate, FiendishTemplate,
                CelestialSmite, FiendishSmite });
            Apply(target, template);
            Apply(target, smite);

            int resolved;
            if (SummonAlignmentRuntimePolicy.TryResolve(mode,
                (int)target.Descriptor.Alignment.Value,
                (int)caster.Descriptor.Alignment.Value, out resolved))
                target.Descriptor.Alignment.Set((Alignment)resolved);
        }

        private void Remove(UnitEntityData target, BlueprintBuff[] blueprints)
        {
            foreach (BlueprintBuff blueprint in blueprints ??
                Array.Empty<BlueprintBuff>())
            {
                Kingmaker.UnitLogic.Buffs.Buff existing = blueprint == null ? null :
                    target.Descriptor.Buffs.GetBuff(blueprint);
                if (existing != null) target.Descriptor.Buffs.RemoveFact(existing);
            }
        }

        private void Apply(UnitEntityData target, BlueprintBuff blueprint)
        {
            Kingmaker.UnitLogic.Buffs.Buff applied = target.Descriptor.Buffs
                .AddBuff(blueprint, Context, null);
            if (applied != null) applied.IsNotDispelable = true;
        }
    }
}
