using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>The damage a mephit breath must deal (MISSION.md R4 table).</summary>
    internal enum MephitBreathDamageKind
    {
        Slashing,
        Bludgeoning,
        Fire,
        Cold,
        Acid
    }

    /// <summary>One of the ten summoned mephit breaths.</summary>
    internal sealed class MephitBreathRow
    {
        internal MephitBreathRow(string key, MephitBreathDamageKind required, string nativeBreathGuid,
            string iconSourceGuid, string iconSourceName, string description)
        {
            Key = key;
            Required = required;
            NativeBreathGuid = nativeBreathGuid;
            IconSourceGuid = iconSourceGuid;
            IconSourceName = iconSourceName;
            Description = description;
        }

        internal string Key { get; private set; }
        internal MephitBreathDamageKind Required { get; private set; }

        /// <summary>
        /// The shared native breath this KMG unit inherits from its donor, for
        /// the four native-backed mephits; null for the six project variants,
        /// whose breath is already a registered project clone.
        /// </summary>
        internal string NativeBreathGuid { get; private set; }

        internal bool NativeBacked { get { return NativeBreathGuid != null; } }

        /// <summary>The installed ability whose existing art the breath reuses.</summary>
        internal string IconSourceGuid { get; private set; }

        internal string IconSourceName { get; private set; }

        /// <summary>Breath text for the native-backed rows (the variants keep theirs).</summary>
        internal string Description { get; private set; }
    }

    /// <summary>
    /// Repair R4 (MEPH-01..04). The installed native summoned-mephit breaths
    /// that the four native-backed KMG mephits inherit deal electricity (air
    /// and water) and acid (earth); every breath, native or project clone,
    /// wears the native Burning Hands icon. Each native-backed KMG mephit gets
    /// an owned breath clone with the required damage, the matching cone and
    /// its own art, redirected only through that unit's own grant and an owned
    /// brain copy. The six project variants already deal their required
    /// damage; they change only their icon. Pyrotechnics gains one brief
    /// native flash per executed cast.
    /// </summary>
    internal static class MephitBreathPolicy
    {
        internal const string ColdConeProjectileGuid = "5af8b717a209fd444a1e4d077ed776f0";
        internal const string AcidConeProjectileGuid = "f6544caac8fe528489327cd86a84b025";
        internal const string FireConeProjectileGuid = "6dfc5e4c7d9ae3048984744222dbd0fa";

        internal const string NativeAirBreathGuid = "1f08438786937954aaa6022c7f5ad286";
        internal const string NativeEarthBreathGuid = "fa5ee5f4cd5c6394f8b497c773f8e14a";
        internal const string NativeFireBreathGuid = "ab0616beb567c2c4d8d3f7447a01a0c8";
        internal const string NativeWaterBreathGuid = "a54cd27999a5e8340976f3a40edfef3a";

        /// <summary>Native Flare Burst, whose blinding burst effect Pyrotechnics reuses.</summary>
        internal const string PyrotechnicsFlashDonorGuid = "39a602aa80cc96f4597778b6d4d49c0a";

        /// <summary>Native prefab Blinding00_10Feet_Aoe.</summary>
        internal const string PyrotechnicsFlashPrefabAssetId = "bb95a6177968e3f499f39e7c90c59fee";

        private static readonly MephitBreathRow[] RowsArray = {
            new MephitBreathRow("air-mephit", MephitBreathDamageKind.Slashing, NativeAirBreathGuid,
                "31f668b12011e344aa542aa07ab6c8d9", "AirBlastAbility",
                "Breath weapon: a 15-foot cone of cutting wind and grit."),
            new MephitBreathRow("earth-mephit", MephitBreathDamageKind.Bludgeoning, NativeEarthBreathGuid,
                "b28c336c10eb51c4a8ded0258d5742e1", "EarthBlastAbility",
                "Breath weapon: a 15-foot cone of hurled grit and stone."),
            new MephitBreathRow("fire-mephit", MephitBreathDamageKind.Fire, NativeFireBreathGuid,
                "3f31704e595e78942b3640cdc9b95d8b", "BloodlineDraconicRedBreathWeaponAbility",
                "Breath weapon: a 15-foot cone of fire."),
            new MephitBreathRow("water-mephit", MephitBreathDamageKind.Acid, NativeWaterBreathGuid,
                "1e65b0b2db777e24db96d8bc52cc9207", "BloodlineDraconicBlackBreathWeaponAbility",
                "Breath weapon: a 15-foot cone of caustic, acidic water."),
            new MephitBreathRow("dust-mephit", MephitBreathDamageKind.Slashing, null,
                "7b8a4a256d4f3dc4d99192bbaabcb307", "SandstormBlastAbility", null),
            new MephitBreathRow("ice-mephit", MephitBreathDamageKind.Cold, null,
                "84be529914c90664aa948d8266bb3fa6", "BloodlineDraconicWhiteBreathWeaponAbility", null),
            new MephitBreathRow("magma-mephit", MephitBreathDamageKind.Fire, null,
                "a0f05637428cbca4bab8bc9122b9e3b9", "MagmaBlastAbility", null),
            new MephitBreathRow("ooze-mephit", MephitBreathDamageKind.Acid, null,
                "1a40fc88aeac9da4aa2fbdbb88335f5d", "CorrosiveTouch", null),
            new MephitBreathRow("salt-mephit", MephitBreathDamageKind.Slashing, null,
                "d859e796f6177cf449679c677076c577", "FragmentationEarthBlastAbility", null),
            new MephitBreathRow("steam-mephit", MephitBreathDamageKind.Fire, null,
                "08eb2ade31670b843879d8841b32d629", "SteamBlastAbility", null)
        };

        internal static IReadOnlyList<MephitBreathRow> Rows
        { get { return Array.AsReadOnly(RowsArray); } }

        internal static IEnumerable<MephitBreathRow> NativeBackedRows
        { get { return RowsArray.Where(row => row.NativeBacked); } }

        internal static MephitBreathRow Row(string key)
        { return RowsArray.Single(row => row.Key == key); }

        internal static string[] NativeBreathGuids
        { get { return NativeBackedRows.Select(row => row.NativeBreathGuid).ToArray(); } }

        /// <summary>
        /// The variant profile's breath energy as a damage kind. Physical forms
        /// are explicit: an unlisted string is refused rather than falling back
        /// to an energy type.
        /// </summary>
        internal static MephitBreathDamageKind Parse(string energy)
        {
            switch (energy)
            {
                case "Slashing": return MephitBreathDamageKind.Slashing;
                case "Bludgeoning": return MephitBreathDamageKind.Bludgeoning;
                case "Fire": return MephitBreathDamageKind.Fire;
                case "Cold": return MephitBreathDamageKind.Cold;
                case "Acid": return MephitBreathDamageKind.Acid;
            }
            throw new ArgumentOutOfRangeException("energy", energy, "Unsupported mephit breath damage.");
        }

        internal static bool IsPhysical(MephitBreathDamageKind kind)
        {
            return kind == MephitBreathDamageKind.Slashing || kind == MephitBreathDamageKind.Bludgeoning;
        }

        /// <summary>Lower-case damage word for breath text.</summary>
        internal static string DamageWord(MephitBreathDamageKind kind)
        {
            return kind.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// The native 15-foot cone visual for the damage: fire, cold or acid for
        /// energy; the pale cold cone for the physical grit breaths (the
        /// existing dust/salt/air presentation). Delivery geometry is unchanged.
        /// </summary>
        internal static string ProjectileFor(MephitBreathDamageKind kind)
        {
            switch (kind)
            {
                case MephitBreathDamageKind.Fire: return FireConeProjectileGuid;
                case MephitBreathDamageKind.Acid: return AcidConeProjectileGuid;
                case MephitBreathDamageKind.Cold:
                case MephitBreathDamageKind.Slashing:
                case MephitBreathDamageKind.Bludgeoning: return ColdConeProjectileGuid;
            }
            throw new ArgumentOutOfRangeException("kind");
        }

        /// <summary>
        /// A copy of <paramref name="items"/> with the single entry matching
        /// <paramref name="match"/> replaced. Anything other than exactly one
        /// match is refused, so an unexpected or shared grant shape is never
        /// partially redirected. The input array is not written.
        /// </summary>
        internal static T[] ReplaceExactlyOne<T>(T[] items, Func<T, bool> match, T replacement, string what)
            where T : class
        {
            if (items == null) throw new InvalidOperationException(what + ": no entries.");
            if (replacement == null) throw new ArgumentNullException("replacement");
            int count = items.Count(value => value != null && match(value));
            if (count != 1)
                throw new InvalidOperationException(what + ": expected exactly one native entry, found " + count + ".");
            return items.Select(value => value != null && match(value) ? replacement : value).ToArray();
        }

        internal static string NativeBackedDescription(MephitBreathRow row, int dice, int sides)
        {
            if (row == null || !row.NativeBacked) throw new ArgumentException("Native-backed breath row required.");
            if (dice <= 0 || sides <= 0) throw new ArgumentOutOfRangeException("dice");
            return row.Description + " Every creature in the cone takes " + dice + "d" + sides + " " +
                DamageWord(row.Required) + " damage; a Reflex save halves it.";
        }
    }
}
