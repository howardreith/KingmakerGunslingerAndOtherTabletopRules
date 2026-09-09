using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class TeleportationTravelers
    {
        private readonly UnitEntityData[] _party;
        internal readonly UnitEntityData[] Units;
        private readonly UnitEntityData[] _masters;
        private TeleportationTravelers(UnitEntityData[] party, UnitEntityData[] units)
        { _party = party; Units = units; _masters = units.Select(value => value.Descriptor.Master.Value).ToArray(); }

        internal static TeleportationTravelers Read(Player player)
        {
            if (player == null) throw new ArgumentNullException("player");
            UnitEntityData[] party = player.Party.ToArray();
            // Native AllCharacters includes canonical cross-scene pets. The local
            // ControllableCharacters collection excludes off-scene travelers.
            UnitEntityData[] units = player.AllCharacters.Concat(party).Distinct().ToArray();
            if (units.Any(value => value == null || value.Descriptor == null))
                throw new InvalidOperationException("A native canonical unit has no descriptor.");
            string[] ids = TeleportTravelerRosterPolicy.Resolve(party.Select(value => value.UniqueId), units.Select(value =>
                new TeleportCanonicalUnit(value.UniqueId, value.Descriptor.Master.Value == null ? null :
                    value.Descriptor.Master.Value.UniqueId, value.IsDetached))).ToArray();
            UnitEntityData[] traveling = ids.Select(id => units.Single(value => value.UniqueId == id)).ToArray();
            foreach (UnitEntityData owner in traveling)
            {
                UnitEntityData pet = owner.Descriptor.Pet;
                if (pet != null && !pet.IsDetached && (!traveling.Any(value => ReferenceEquals(value, pet)) ||
                    !ReferenceEquals(pet.Descriptor.Master.Value, owner)))
                    throw new InvalidOperationException("A native traveling pet lacks a reciprocal canonical master association.");
            }
            return new TeleportationTravelers(party, traveling);
        }

        internal bool Matches(TeleportationTravelers current)
        {
            return current != null && _party.SequenceEqual(current._party) && Units.SequenceEqual(current.Units) &&
                _masters.SequenceEqual(current._masters);
        }
        internal object Evidence()
        {
            return new { partyIds = _party.Select(value => value.UniqueId).ToArray(),
                associatedIds = Units.Except(_party).Select(value => value.UniqueId).ToArray(),
                travelers = Units.Select((value, index) => new { id = value.UniqueId,
                    masterId = _masters[index] == null ? null : _masters[index].UniqueId,
                    petId = value.Descriptor.Pet == null ? null : value.Descriptor.Pet.UniqueId,
                    isPet = value.Descriptor.IsPet, detached = value.IsDetached,
                    living = !value.Descriptor.State.IsDead, damage = value.Damage }).ToArray() };
        }
        internal IReadOnlyList<TeleportTravelerSnapshot> OutcomeSnapshots()
        { return Array.AsReadOnly(Units.Select(value => new TeleportTravelerSnapshot(value.UniqueId, !value.Descriptor.State.IsDead, true)).ToArray()); }
    }
}
