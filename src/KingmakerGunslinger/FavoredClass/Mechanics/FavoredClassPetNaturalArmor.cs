using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>Which pet a pet-armor counter improves.</summary>
    internal static class FavoredClassPets
    {
        /// <summary>
        /// The pet is of the counter's pet class: its blueprint levels in that
        /// class (native AddPet levels it through AddClassLevels) or it already
        /// has levels in it.
        /// </summary>
        internal static bool IsQualified(UnitEntityData pet, string petClassGuid)
        {
            if (pet == null || pet.Descriptor == null || string.IsNullOrEmpty(petClassGuid))
                return false;
            AddClassLevels levels = pet.Blueprint == null ? null : pet.Blueprint.GetComponent<AddClassLevels>();
            if (levels != null && levels.CharacterClass != null &&
                string.Equals(levels.CharacterClass.AssetGuid, petClassGuid, StringComparison.Ordinal))
                return true;
            return pet.Descriptor.Progression.Classes.Any(value => value.CharacterClass != null &&
                string.Equals(value.CharacterClass.AssetGuid, petClassGuid, StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// O07/O08, pet side: the hidden pet feature's natural armor bonus is the
    /// master's earned steps, recomputed whenever the master's counter or pet
    /// changes, and zero unless this pet is still its master's current pet
    /// of the counter's pet class. Natural armor stacks with the pet's own
    /// natural armor and is excluded from touch AC natively; enhancement to
    /// natural armor (Barkskin) is a separate descriptor.
    /// </summary>
    public sealed class FavoredClassPetNaturalArmor : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintFeature MasterFeature;
        public int Divisor = 4;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        public string PetClassGuid;

        private ModifiableValue.Modifier m_Modifier;

        public override void OnTurnOn()
        {
            Apply();
        }

        public override void OnTurnOff()
        {
            Remove();
        }

        internal int AppliedValue
        {
            get { return m_Modifier == null ? 0 : m_Modifier.ModValue; }
        }

        internal void Refresh()
        {
            Remove();
            if (Fact != null && Fact.IsTurnedOn)
                Apply();
        }

        internal int EarnedSteps()
        {
            if (!FavoredClassRuntime.MechanicsEnabled || MasterFeature == null || Owner == null)
                return 0;
            UnitEntityData master = Owner.Master.Value;
            if (master == null || !ReferenceEquals(master.Descriptor.Pet, Owner.Unit) ||
                !FavoredClassPets.IsQualified(Owner.Unit, PetClassGuid))
                return 0;
            return FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null),
                master.Descriptor.Progression.Features.GetRank(MasterFeature));
        }

        private void Apply()
        {
            if (m_Modifier != null || Owner == null)
                return;
            int steps = EarnedSteps();
            if (steps > 0)
                m_Modifier = Owner.Stats.AC.AddModifier(steps, this, ModifierDescriptor.NaturalArmor);
        }

        private void Remove()
        {
            if (m_Modifier != null)
                m_Modifier.Remove();
            m_Modifier = null;
        }
    }

    /// <summary>
    /// O07/O08, master side, on the full leaf: projects the hidden pet
    /// feature onto the master's current qualified pet (on activation, on
    /// load and whenever a pet is linked to this master) and removes it from
    /// a previously projected pet, so a replaced pet keeps no orphaned bonus
    /// and a pet never receives two copies. The investment stays on the
    /// master; no favored-class progression is added to the pet's class.
    /// </summary>
    public sealed class FavoredClassPetArmorProjection : OwnedGameLogicComponent<UnitDescriptor>,
        IUnitFactionHandler
    {
        public BlueprintFeature PetFeature;
        public string PetClassGuid;

        [JsonProperty]
        private UnitReference m_ProjectedPet;

        public override void OnFactActivate()
        {
            Sync();
        }

        public override void OnTurnOn()
        {
            Sync();
        }

        public override void OnFactDeactivate()
        {
            // A rank change reactivates the fact; activation re-synchronizes.
            if (IsReapplying)
                return;
            Unproject(m_ProjectedPet.Value);
            m_ProjectedPet = null;
        }

        public void HandleFactionChanged(UnitEntityData unit)
        {
            // Native SetMaster raises this when a pet is linked to its master.
            if (unit != null && Owner != null && unit.Descriptor != null &&
                ReferenceEquals(unit.Descriptor.Master.Value, Owner.Unit))
                Sync();
        }

        internal void Sync()
        {
            if (Owner == null || PetFeature == null)
                return;
            UnitEntityData current = Owner.Pet;
            UnitEntityData previous = m_ProjectedPet.Value;
            if (previous != null && !ReferenceEquals(previous, current))
                Unproject(previous);
            if (current == null || !FavoredClassPets.IsQualified(current, PetClassGuid))
            {
                m_ProjectedPet = null;
                return;
            }
            Fact existing = current.Descriptor.Progression.Features.GetFact(PetFeature);
            if (existing == null)
                current.Descriptor.Progression.Features.AddFact(PetFeature, null);
            else
                existing.CallComponents<FavoredClassPetNaturalArmor>(component => component.Refresh());
            m_ProjectedPet = current;
        }

        private void Unproject(UnitEntityData pet)
        {
            if (pet != null && pet.Descriptor != null &&
                pet.Descriptor.Progression.Features.HasFact(PetFeature))
                pet.Descriptor.Progression.Features.RemoveFact(PetFeature);
        }
    }
}
