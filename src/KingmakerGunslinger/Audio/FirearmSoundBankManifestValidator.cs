using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Audio
{
    internal static class FirearmSoundBankManifestValidator
    {
        internal static void Validate(FirearmSoundBankManifest value)
        {
            if (value == null)
                throw Failure("manifest is null.");
            if (value.SchemaVersion != 1)
                throw Failure("expected schemaVersion=1; observed=" +
                    value.SchemaVersion + ".");
            if (value.BankName != FirearmSoundEventCatalog.BankName)
                throw Failure("unexpected bank name.");
            string file = value.BankFileName;
            if (file != FirearmSoundEventCatalog.BankFileName || Path.IsPathRooted(file ?? string.Empty) ||
                file.IndexOfAny(new[] { '/', '\\' }) >= 0 || file.Contains("..") ||
                string.Equals(file, "Init.bnk", StringComparison.OrdinalIgnoreCase))
                throw Failure("unexpected bank filename.");
            if (value.Platform != FirearmSoundEventCatalog.Platform)
                throw Failure("only Windows is supported.");
            if (string.IsNullOrWhiteSpace(value.WwiseVersion) ||
                !value.WwiseVersion.StartsWith("2016.2.", StringComparison.Ordinal))
                throw Failure("Wwise 2016.2.x is required.");
            if (string.IsNullOrEmpty(value.Sha256) ||
                value.Sha256.Length != 64 ||
                value.Sha256.Any(c => !(c >= '0' && c <= '9') &&
                    !(c >= 'A' && c <= 'F')))
                throw Failure("invalid SHA-256.");
            if (!value.MediaEmbedded)
                throw Failure("media must be embedded.");
            if (value.Events == null || value.Events.Count != 5)
                throw Failure("exactly five events are required.");
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<FirearmKind, string> expected in FirearmSoundEventCatalog.All)
            {
                string actual;
                if (!value.Events.TryGetValue(expected.Key.ToString(), out actual) ||
                    actual != expected.Value)
                    throw Failure("mismatched event: " + expected.Key + ".");
                if (!unique.Add(actual))
                    throw Failure("duplicate event.");
            }
        }

        private static InvalidDataException Failure(string detail)
        {
            return new InvalidDataException(
                "Manifest semantic validation failed: " + detail);
        }
    }
}
