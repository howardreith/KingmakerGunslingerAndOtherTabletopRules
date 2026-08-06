using System; using System.Collections.Generic; using System.IO; using System.Linq; using Newtonsoft.Json; using KingmakerGunslinger.Firearms;
namespace KingmakerGunslinger.Audio
{
    internal static class FirearmSoundBankManifestLoader
    {
        internal static FirearmSoundBankManifest Load(string path) { if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Manifest path required.", "path"); return Parse(File.ReadAllText(path)); }
        internal static FirearmSoundBankManifest Parse(string json) { if (string.IsNullOrWhiteSpace(json)) throw new InvalidDataException("Manifest is empty."); FirearmSoundBankManifest value; try { value = JsonConvert.DeserializeObject<FirearmSoundBankManifest>(json); } catch (JsonException e) { throw new InvalidDataException("Manifest JSON is invalid.", e); } Validate(value); return value; }
        internal static void Validate(FirearmSoundBankManifest value)
        {
            if (value == null || value.SchemaVersion != 1) throw new InvalidDataException("Unsupported manifest schema.");
            if (value.BankName != FirearmSoundEventCatalog.BankName) throw new InvalidDataException("Unexpected bank name.");
            if (value.BankFileName != FirearmSoundEventCatalog.BankFileName || value.BankFileName.IndexOfAny(new[] {'/', '\\'}) >= 0 || value.BankFileName.Contains("..") || string.Equals(value.BankFileName, "Init.bnk", StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unexpected bank filename.");
            if (value.Platform != FirearmSoundEventCatalog.Platform) throw new InvalidDataException("Only Windows is supported.");
            if (string.IsNullOrWhiteSpace(value.WwiseVersion) || !value.WwiseVersion.StartsWith("2016.2.", StringComparison.Ordinal)) throw new InvalidDataException("Wwise 2016.2.x is required.");
            if (string.IsNullOrEmpty(value.Sha256) || value.Sha256.Length != 64 || value.Sha256.Any(c => !(c >= '0' && c <= '9') && !(c >= 'A' && c <= 'F'))) throw new InvalidDataException("Invalid SHA-256.");
            if (!value.MediaEmbedded) throw new InvalidDataException("Media must be embedded.");
            if (value.Events == null || value.Events.Count != 5) throw new InvalidDataException("Exactly five events are required.");
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<FirearmKind,string> expected in FirearmSoundEventCatalog.All) { string actual; if (!value.Events.TryGetValue(expected.Key.ToString(), out actual) || actual != expected.Value) throw new InvalidDataException("Mismatched event: " + expected.Key); if (!unique.Add(actual)) throw new InvalidDataException("Duplicate event."); }
        }
    }
}
