using System; using System.IO; using Newtonsoft.Json;
namespace KingmakerGunslinger.Audio
{
    internal static class FirearmSoundBankManifestLoader
    {
        internal static FirearmSoundBankManifest Load(string path) { if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Manifest path required.", "path"); return Parse(File.ReadAllText(path)); }
        internal static FirearmSoundBankManifest Parse(string json) { if (string.IsNullOrWhiteSpace(json)) throw new InvalidDataException("Manifest is empty."); FirearmSoundBankManifest value; try { value = JsonConvert.DeserializeObject<FirearmSoundBankManifest>(json); } catch (JsonException e) { throw new InvalidDataException("Manifest JSON is invalid.", e); } Validate(value); return value; }
        internal static void Validate(FirearmSoundBankManifest value) { FirearmSoundBankManifestValidator.Validate(value); }
    }
}
