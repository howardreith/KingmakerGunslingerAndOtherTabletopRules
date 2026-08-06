using System.Collections.Generic;
namespace KingmakerGunslinger.Audio
{
    internal sealed class FirearmSoundBankManifest
    {
        public int SchemaVersion { get; set; }
        public string BankName { get; set; }
        public string BankFileName { get; set; }
        public string Platform { get; set; }
        public string WwiseVersion { get; set; }
        public string Sha256 { get; set; }
        public bool MediaEmbedded { get; set; }
        public IDictionary<string, string> Events { get; set; }
    }
}
