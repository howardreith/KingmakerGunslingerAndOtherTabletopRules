using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Development;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Resolves the current player character and accesses the custom UnitPart. It
    /// never anchors state to the selected wielder; the main character is only the
    /// save-owned vault host for direct item references.
    /// </summary>
    internal sealed class KingmakerFirearmStateVaultPartProvider
    {
        private static readonly string[] MainCharacterMembers =
        {
            "MainCharacterEntity",
            "MainCharacter",
            "MainCharacterUnit"
        };

        private static readonly string[] EntityMembers =
        {
            "Entity",
            "Unit",
            "Value",
            "UnitEntityData",
            "EntityData"
        };

        internal bool TryGetExisting(out UnitPartFirearmStateVault vault)
        {
            UnitEntityData mainCharacter;
            if (!TryResolveMainCharacter(out mainCharacter))
            {
                vault = null;
                return false;
            }

            vault = mainCharacter.Get<UnitPartFirearmStateVault>();
            return vault != null;
        }

        internal UnitPartFirearmStateVault RequireForWrite()
        {
            UnitEntityData mainCharacter;
            if (!TryResolveMainCharacter(out mainCharacter))
            {
                throw new InvalidOperationException(
                    "No active Kingmaker main-character entity is available for the firearm-state vault. Load a disposable campaign first.");
            }

            UnitPartFirearmStateVault vault =
                mainCharacter.Ensure<UnitPartFirearmStateVault>();
            if (vault == null)
            {
                throw new InvalidOperationException(
                    "Kingmaker did not return the requested firearm-state UnitPart.");
            }

            return vault;
        }

        private static bool TryResolveMainCharacter(out UnitEntityData entity)
        {
            Type gameType = typeof(BlueprintScriptableObject).Assembly.GetType(
                "Kingmaker.Game",
                false);
            if (gameType == null)
            {
                entity = null;
                return false;
            }

            object game;
            if (!ReflectionAccess.TryGetMember(gameType, "Instance", out game) ||
                game == null)
            {
                entity = null;
                return false;
            }

            object player;
            if (!ReflectionAccess.TryGetMember(game, "Player", out player) ||
                player == null)
            {
                entity = null;
                return false;
            }

            object mainCharacter;
            string ignored;
            if (!ReflectionAccess.TryGetFirstNonNullMember(
                player,
                MainCharacterMembers,
                out mainCharacter,
                out ignored) ||
                mainCharacter == null)
            {
                entity = null;
                return false;
            }

            entity = mainCharacter as UnitEntityData;
            if (entity != null)
            {
                return true;
            }

            foreach (string member in EntityMembers)
            {
                object candidate;
                if (ReflectionAccess.TryGetMember(
                    mainCharacter,
                    member,
                    out candidate))
                {
                    entity = candidate as UnitEntityData;
                    if (entity != null)
                    {
                        return true;
                    }
                }
            }

            entity = null;
            return false;
        }
    }
}
