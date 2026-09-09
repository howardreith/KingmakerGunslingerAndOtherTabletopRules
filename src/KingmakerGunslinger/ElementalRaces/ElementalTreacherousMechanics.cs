using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;

namespace KingmakerGunslinger.ElementalRaces
{
    [Serializable]
    public sealed class ElementalTreacherousGroundTarget : BlueprintComponent, IAbilityTargetChecker
    {
        public BlueprintAbility Ability;
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            ElementalGroundPoint ground;
            return ElementalTreacherousGroundRuntime.TryTarget(caster, target, Ability, out ground);
        }
    }

    [Serializable]
    public sealed class ElementalTreacherousParameters : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>,
        IUnitLostFactHandler
    {
        public BlueprintAbility Ability;
        public BlueprintAbilityAreaEffect Area;
        public BlueprintFeature Marker;
        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt == null || Owner == null || !ReferenceEquals(evt.Blueprint, Ability)) return;
            evt.ReplaceCasterLevel = Math.Max(1, Owner.Progression.CharacterLevel);
            evt.ReplaceSpellLevel = 0;
        }
        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
        public void HandleUnitLostFact(Fact fact)
        {
            Feature feature = fact as Feature;
            if (feature != null && ReferenceEquals(feature.Owner, Owner) && ReferenceEquals(feature.Blueprint, Marker))
                EndOwnedAreas();
        }
        public override void OnFactDeactivate()
        {
            if (Owner != null && Marker != null && !Owner.HasFact(Marker)) EndOwnedAreas();
        }
        private void EndOwnedAreas()
        {
            if (Game.Instance == null || Game.Instance.State == null || Owner == null) return;
            foreach (var area in Game.Instance.State.AreaEffects.All.Where(value =>
                ReferenceEquals(value.Blueprint, Area) && value.Context != null &&
                ReferenceEquals(value.Context.MaybeCaster, Owner.Unit)).ToArray())
                area.ForceEnd();
        }
    }

    [Serializable]
    public sealed class ElementalTreacherousActivate : ContextAction
    {
        public BlueprintAbility Ability;
        public BlueprintAbilityAreaEffect Area;
        public override string GetCaption() { return "Create the owned, timed difficult-terrain patch"; }
        public override void RunAction()
        {
            UnitEntityData caster = Context.MaybeCaster;
            if (caster == null || caster.Descriptor.State.IsDead || Area == null) return;
            ElementalGroundPoint ground;
            if (!ElementalTreacherousGroundRuntime.TryTarget(caster, Target, Ability, out ground)) return;
            // Native commitment has already occurred once. Revalidate moving
            // obstacles/ground before spawning; never refund an accepted use.
            Spawn(Context, Area, new TargetWrapper(ground.Position));
        }
        internal static AreaEffectEntityData Spawn(MechanicsContext context,
            BlueprintAbilityAreaEffect area, TargetWrapper point)
        {
            // Native fixed area serialization owns position, caster context,
            // creation time and remaining duration. No buff recreates its timer.
            return AreaEffectsController.Spawn(context, area, point,
                TimeSpan.FromMinutes(ElementalTreacherousPolicy.DurationMinutes(
                    context.MaybeCaster.Descriptor.Progression.CharacterLevel)));
        }
    }

    [Serializable]
    public sealed class ElementalTreacherousArea : AbilityAreaEffectLogic
    {
        public BlueprintBuff Terrain;
        public BlueprintFeature Marker;
        private bool Owned(MechanicsContext context)
        {
            return context != null && context.MaybeCaster != null && Marker != null &&
                context.MaybeCaster.Descriptor.HasFact(Marker);
        }
        protected override void OnUnitEnter(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            ReconcileGround(context, area, unit);
        }
        protected override void OnUnitMove(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            ReconcileGround(context, area, unit);
        }
        private void ReconcileGround(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            if (!Owned(context) || !ElementalTreacherousGroundRuntime.SamePatchGround(area.Position, unit.Position))
            {
                OnUnitExit(context, area, unit);
                return;
            }
            if (unit.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, Terrain) &&
                value.SourceAreaEffectId == area.UniqueId)) return;
            // Native Ground immunity and condition stacking stay authoritative.
            Buff buff = unit.Descriptor.AddBuff(Terrain, context, null);
            if (buff != null) { buff.SourceAreaEffectId = area.UniqueId; buff.IsNotDispelable = true; }
        }
        protected override void OnUnitExit(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            foreach (var buff in unit.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, Terrain) && value.SourceAreaEffectId == area.UniqueId).ToArray())
                buff.Remove();
        }
        protected override void OnTick(MechanicsContext context, AreaEffectEntityData area)
        {
            if (!Owned(context)) { area.ForceEnd(); return; }
            foreach (var unit in area.UnitsInside.ToArray()) ReconcileGround(context, area, unit);
        }
    }
}
