using System;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects.SriptZones;

namespace KingmakerGunslinger.Summoning
{
    // This one prepared spell uses a native moving area and native condition
    // immunity. Underlying invisibility buffs retain their source and deadline;
    // leaving removes only this area's suppression and can reveal them again.
    [Serializable]
    public sealed class TrumpetInvisibilityPurgeArea : AbilityAreaEffectLogic
    {
        public BlueprintAbilityAreaEffect OwningArea;
        public BlueprintBuff Recipient;
        public BlueprintBuff NaturalInvisibility;

        private bool SetRadius(MechanicsContext context, AreaEffectEntityData area)
        {
            if (area == null || !ReferenceEquals(area.Blueprint, OwningArea)) return false;
            var cylinder = area.View?.Shape as ScriptZoneCylinder;
            if (cylinder == null) throw new InvalidOperationException("Private Invisibility Purge lacks its native cylinder.");
            cylinder.Radius = new Feet(Math.Max(0, context.Params.CasterLevel) * 5).Meters;
            return true;
        }
        private void Reconcile(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            if (unit == null || !SetRadius(context, area)) return;
            // Native membership supplies line of effect and target admission.
            // Recheck the per-instance scaled shape for the first entry tick.
            if (area.IsEnded || unit.View == null || !area.View.Shape.Contains(unit.Position, unit.View.Corpulence)) { OnUnitExit(context, area, unit); return; }
            // Natural invisibility survives this spell. Remove only this area's
            // recipient, leaving all invisibility sources and other areas untouched.
            if (NaturalInvisibility != null && unit.Buffs.Enumerable.Any(buff =>
                ReferenceEquals(buff.Blueprint, NaturalInvisibility) && buff.IsTurnedOn))
            { OnUnitExit(context, area, unit); return; }
            if (unit.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, Recipient) &&
                buff.SourceAreaEffectId == area.UniqueId)) return;
            var grant = unit.Descriptor.AddBuff(Recipient, context, null);
            if (grant != null) { grant.SourceAreaEffectId = area.UniqueId; grant.IsNotDispelable = true; }
        }
        protected override void OnUnitEnter(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        { Reconcile(context, area, unit); }
        protected override void OnUnitMove(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        { Reconcile(context, area, unit); }
        protected override void OnTick(MechanicsContext context, AreaEffectEntityData area)
        {
            if (!SetRadius(context, area)) return;
            foreach (var unit in area.UnitsInside.ToArray()) Reconcile(context, area, unit);
        }
        protected override void OnUnitExit(MechanicsContext context, AreaEffectEntityData area, UnitEntityData unit)
        {
            if (!ReferenceEquals(area.Blueprint, OwningArea) || unit == null) return;
            foreach (var buff in unit.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, Recipient) &&
                buff.SourceAreaEffectId == area.UniqueId).ToArray()) buff.Remove();
        }
    }
}
