using System; using System.IO; using System.Security.Cryptography;
namespace KingmakerGunslinger.Audio
{
    internal enum FirearmSoundBankStageStatus { Copied, Skipped }
    internal sealed class FirearmSoundBankStageResult
    {
        internal FirearmSoundBankStageResult(FirearmSoundBankStageStatus status, string source, string destination, string hash) { Status=status; SourcePath=source; DestinationPath=destination; ObservedHash=hash; }
        internal FirearmSoundBankStageStatus Status { get; private set; } internal string SourcePath { get; private set; } internal string DestinationPath { get; private set; } internal string ObservedHash { get; private set; }
    }
    internal sealed class FirearmSoundBankStager
    {
        internal FirearmSoundBankStageResult Stage(string modRoot, string dataPath, FirearmSoundBankManifest manifest)
        {
            FirearmSoundBankManifestValidator.Validate(manifest);
            string sourceRoot=Full(Path.Combine(modRoot,"assets","soundbanks")), destinationRoot=Full(Path.Combine(dataPath,"StreamingAssets","Audio","GeneratedSoundBanks","Windows"));
            string source=Full(Path.Combine(sourceRoot,FirearmSoundEventCatalog.BankFileName)), destination=Full(Path.Combine(destinationRoot,FirearmSoundEventCatalog.BankFileName)); RequireChild(sourceRoot,source); RequireChild(destinationRoot,destination);
            if (!File.Exists(source)) throw new FileNotFoundException("Packaged firearm bank missing.",source); string hash=Hash(source); if (hash != manifest.Sha256) throw new InvalidDataException("Source bank hash mismatch."); Directory.CreateDirectory(destinationRoot);
            if (File.Exists(destination) && Hash(destination)==manifest.Sha256) return new FirearmSoundBankStageResult(FirearmSoundBankStageStatus.Skipped,source,destination,hash);
            string temporary=Path.Combine(destinationRoot,".KMG_Firearms."+Guid.NewGuid().ToString("N")+".tmp");
            try { File.Copy(source,temporary,false); if(Hash(temporary)!=manifest.Sha256) throw new InvalidDataException("Temporary bank hash mismatch."); if(File.Exists(destination)) File.Replace(temporary,destination,null); else File.Move(temporary,destination); if(Hash(destination)!=manifest.Sha256) throw new InvalidDataException("Destination bank hash mismatch."); return new FirearmSoundBankStageResult(FirearmSoundBankStageStatus.Copied,source,destination,hash); }
            finally { if(File.Exists(temporary)) File.Delete(temporary); }
        }
        internal static string Hash(string path) { using(SHA256 sha=SHA256.Create()) using(FileStream stream=File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-",string.Empty); }
        private static string Full(string path) { return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar); }
        private static void RequireChild(string root,string path) { if(!path.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Bank path escaped allowlisted root."); }
    }
}
