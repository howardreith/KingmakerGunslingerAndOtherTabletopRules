using System;
using System.Globalization;
using Kingmaker.Items;
using KingmakerGunslinger.Development;

namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Reads Kingmaker's inherited ItemEntity.UniqueId member from an exact
    /// ItemEntityWeapon. No fallback member is accepted for persistence and no
    /// identity is ever generated or assigned by the mod.
    /// </summary>
    internal sealed class KingmakerFirearmItemIdentityProvider : IFirearmItemIdentityProvider
    {
        private const string IdentityMemberName = "UniqueId";

        public bool TryGetIdentity(
            object itemInstance,
            out FirearmItemId identity,
            out string rejectionReason)
        {
            identity = null;
            if (itemInstance == null)
            {
                rejectionReason = "No runtime firearm item was supplied for identity resolution.";
                return false;
            }

            ItemEntityWeapon weapon = itemInstance as ItemEntityWeapon;
            if (weapon == null)
            {
                rejectionReason = string.Format(
                    CultureInfo.InvariantCulture,
                    "Runtime candidate type '{0}' is not an ItemEntityWeapon and cannot own firearm state.",
                    itemInstance.GetType().FullName);
                return false;
            }

            object raw;
            if (!ReflectionAccess.TryGetMember(
                weapon,
                IdentityMemberName,
                out raw) ||
                raw == null)
            {
                rejectionReason =
                    "The runtime ItemEntityWeapon exposes no readable non-null UniqueId member. Sprint 14 refuses to invent an item identity.";
                return false;
            }

            try
            {
                Guid guid;
                if (raw is Guid)
                {
                    guid = (Guid)raw;
                }
                else
                {
                    string text = raw as string;
                    if (text == null)
                    {
                        rejectionReason = string.Format(
                            CultureInfo.InvariantCulture,
                            "ItemEntityWeapon.UniqueId has unsupported runtime type '{0}'. Only System.Guid or System.String is accepted until the installed contract is inspected.",
                            raw.GetType().FullName);
                        return false;
                    }

                    identity = new FirearmItemId(text);
                    rejectionReason = null;
                    return true;
                }

                identity = new FirearmItemId(guid);
                rejectionReason = null;
                return true;
            }
            catch (Exception exception)
            {
                identity = null;
                rejectionReason = string.Format(
                    CultureInfo.InvariantCulture,
                    "ItemEntityWeapon.UniqueId was not a usable nonempty GUID: {0}",
                    exception.Message);
                return false;
            }
        }
    }
}
