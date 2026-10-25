using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.PubSubSystem;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Creation-only adult Lemure racial bases. Native attributes and
    /// modifiers compose on top; no periodic stat or skill compensation.</summary>
    [Serializable]
    public sealed class SummonLemureRacialProfile :
        OwnedGameLogicComponent<UnitDescriptor>, IHandleEntityComponent<UnitEntityData>
    {
        public BlueprintUnit OwningBlueprint;
        public void OnEntityCreated(UnitEntityData unit)
        {
            if (unit == null || OwningBlueprint == null ||
                !ReferenceEquals(unit.Blueprint, OwningBlueprint))
                throw new InvalidOperationException("Lemure bases require their exact owner.");
            var stats = unit.Descriptor.Stats;
            stats.HitPoints.BaseValue = 11; // average 2d10, Constitution remains live
            stats.SaveFortitude.BaseValue = 3;
            stats.SaveReflex.BaseValue = 3;
            stats.SaveWill.BaseValue = 0;
        }
        public void OnEntityRemoved(UnitEntityData unit) { }
    }

    [Serializable]
    public sealed class SummonDretchRacialProfile :
        OwnedGameLogicComponent<UnitDescriptor>, IHandleEntityComponent<UnitEntityData>
    {
        public BlueprintUnit OwningBlueprint;
        public void OnEntityCreated(UnitEntityData unit)
        {
            if (unit == null || OwningBlueprint == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint))
                throw new InvalidOperationException("Dretch racial bases require their exact owner.");
            var stats = unit.Descriptor.Stats;
            stats.HitPoints.BaseValue = 11; // 2d10; native Constitution and Toughness remain live.
            stats.SaveFortitude.BaseValue = 3; stats.SaveReflex.BaseValue = 0; stats.SaveWill.BaseValue = 3;
            stats.SkillPerception.BaseValue = 2; stats.SkillStealth.BaseValue = 2;
        }
        public void OnEntityRemoved(UnitEntityData unit) { }
    }
}
