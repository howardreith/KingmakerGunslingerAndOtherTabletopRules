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
                "665cfd3718c4f284d80538d85a2791c9", "MetalBlastAbility", null),
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

        /// <summary>
        /// VIS-05: the owned Sprint 5 Dust Mephit unit. Its breath keeps the
        /// shared native cold cone for delivery and geometry; only the cone
        /// instance one of these units launches is presented as dust.
        /// </summary>
        internal const string DustMephitUnitGuid = "30f6615badf14d21b41d6468d5c93ac3";

        /// <summary>The owned Dust Mephit breath ability (KMG.Summoning.Special.DustMephit.Breath).</summary>
        internal const string DustMephitBreathGuid = "9bf56be04a0c452fa628136a7a454b22";

        /// <summary>
        /// The shared cold cone is dust only when the exact owned Dust Mephit
        /// launches it while executing its own breath. Every other launcher of
        /// the same native cone (Air, Earth, Salt and Ice Mephits, native spells
        /// and monsters), any other ability the Dust Mephit executes and any
        /// launch outside an ability execution keep the native look.
        /// </summary>
        internal static bool PresentsDustBreath(string launcherBlueprintGuid, string executingAbilityGuid, string projectileGuid)
        {
            return string.Equals(launcherBlueprintGuid, DustMephitUnitGuid, StringComparison.Ordinal) &&
                string.Equals(executingAbilityGuid, DustMephitBreathGuid, StringComparison.Ordinal) &&
                string.Equals(projectileGuid, ColdConeProjectileGuid, StringComparison.Ordinal);
        }

        /// <summary>
        /// The dust palette, linear RGB in [0, 1]: a pale sandy ochre and a
        /// darker umber. Each original colour keeps its own alpha and its own
        /// brightness, so the cone's opacity and glow structure are kept and
        /// only the hue changes from ice to dust.
        /// </summary>
        internal static readonly float[] DustLight = { .78f, .66f, .46f };
        internal static readonly float[] DustDark = { .46f, .36f, .24f };

        /// <summary>
        /// One original colour presented as dust: its luminance picks a point
        /// between the dark and light dust tones and scales it back to the same
        /// brightness; the alpha is unchanged. Non-finite input is returned as
        /// the dark tone at the original alpha.
        /// </summary>
        internal static float[] DustTint(float r, float g, float b, float a)
        {
            return Tone(r, g, b, a, DustDark, DustLight);
        }

        /// <summary>
        /// A subtractive layer darkens the scene by its colour, so the
        /// complement of the dust tone is what leaves warm dust behind it. The
        /// same luminance-preserving mapping with the complementary palette.
        /// </summary>
        internal static float[] DustTintSubtractive(float r, float g, float b, float a)
        {
            return Tone(r, g, b, a, new[] { 1f - DustDark[0], 1f - DustDark[1], 1f - DustDark[2] },
                new[] { 1f - DustLight[0], 1f - DustLight[1], 1f - DustLight[2] });
        }

        /// <summary>Whether a material subtracts its colour from the scene (the native names say so).</summary>
        internal static bool IsSubtractiveMaterial(string name)
        {
            return name != null && (name.IndexOf("Substract", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Subtract", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static float[] Tone(float r, float g, float b, float a, float[] dark, float[] light)
        {
            float luminance = .2126f * r + .7152f * g + .0722f * b;
            if (float.IsNaN(luminance) || float.IsInfinity(luminance) || luminance < 0f)
                return new[] { dark[0], dark[1], dark[2], a };
            float t = Math.Min(1f, luminance);
            var tone = new float[3];
            for (int k = 0; k < 3; k++) tone[k] = dark[k] + (light[k] - dark[k]) * t;
            float toneLuminance = .2126f * tone[0] + .7152f * tone[1] + .0722f * tone[2];
            float scale = luminance / toneLuminance;
            return new[] { tone[0] * scale, tone[1] * scale, tone[2] * scale, a };
        }

        internal static string NativeBackedDescription(MephitBreathRow row, int dice, int sides)
        {
            if (row == null || !row.NativeBacked) throw new ArgumentException("Native-backed breath row required.");
            if (dice <= 0 || sides <= 0) throw new ArgumentOutOfRangeException("dice");
            return row.Description + " Only the mephit's enemies in the cone are affected: " + dice + "d" + sides + " " +
                DamageWord(row.Required) + " damage; a Reflex save halves it.";
        }
    }

    /// <summary>
    /// VIS-05: which projectile creation a cast-effect spawn belongs to.
    /// Creations are tracked only inside an ability execution scope (the
    /// native AbilityExecutionProcess.Tick, which catches every exception
    /// raised by its delivery, so the scope always ends). Every creation in a
    /// scope is pushed, owned or not, so a nested foreign creation shadows an
    /// enclosing owned one; a spawn adopts only the innermost creation of the
    /// innermost scope of the current frame, at most once. A creation that
    /// throws never ends itself: the enclosing creation's end or the scope's
    /// end discards it, and nothing outside a scope can adopt. A scope left
    /// behind by an earlier frame is discarded when the next one begins.
    /// </summary>
    internal sealed class DustCreationTracker
    {
        private sealed class Scope { internal int Frame; internal readonly List<Entry> Entries = new List<Entry>(); }

        private sealed class Entry { internal Scope Scope; internal object Token; internal bool Owned; internal bool Adopted; }

        private readonly List<Scope> _scopes = new List<Scope>();

        internal int ScopeDepth { get { return _scopes.Count; } }

        internal object BeginScope(int frame)
        {
            if (_scopes.Count > 0 && _scopes[_scopes.Count - 1].Frame != frame) _scopes.Clear();
            var scope = new Scope { Frame = frame };
            _scopes.Add(scope);
            return scope;
        }

        internal void EndScope(object handle)
        {
            int index = _scopes.IndexOf(handle as Scope);
            if (index >= 0) _scopes.RemoveRange(index, _scopes.Count - index);
        }

        /// <summary>Starts tracking one creation; null outside an execution scope of this frame.</summary>
        internal object BeginCreation(object token, bool owned, int frame)
        {
            if (_scopes.Count == 0) return null;
            Scope scope = _scopes[_scopes.Count - 1];
            if (scope.Frame != frame) return null;
            var entry = new Entry { Scope = scope, Token = token, Owned = owned };
            scope.Entries.Add(entry);
            return entry;
        }

        /// <summary>Ends one creation and discards any interrupted creation nested inside it.</summary>
        internal bool EndCreation(object handle)
        {
            var entry = handle as Entry;
            if (entry == null) return false;
            int index = entry.Scope.Entries.IndexOf(entry);
            if (index >= 0) entry.Scope.Entries.RemoveRange(index, entry.Scope.Entries.Count - index);
            return entry.Owned && !entry.Adopted;
        }

        private Entry Innermost(int frame)
        {
            if (_scopes.Count == 0) return null;
            Scope scope = _scopes[_scopes.Count - 1];
            if (scope.Frame != frame || scope.Entries.Count == 0) return null;
            return scope.Entries[scope.Entries.Count - 1];
        }

        /// <summary>The owned, not yet adopted creation a spawn in this frame would belong to.</summary>
        internal object Peek(int frame)
        {
            Entry entry = Innermost(frame);
            return entry != null && entry.Owned && !entry.Adopted ? entry.Token : null;
        }

        internal object Adopt(int frame)
        {
            Entry entry = Innermost(frame);
            if (entry == null || !entry.Owned || entry.Adopted) return null;
            entry.Adopted = true;
            return entry.Token;
        }
    }
}
