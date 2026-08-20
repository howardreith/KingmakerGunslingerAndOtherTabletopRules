using System; using System.Collections.Generic; using System.IO; using System.Security.Cryptography; using System.Text;
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
            if (!File.Exists(source)) throw new FileNotFoundException("Packaged firearm bank missing.",source); string hash=Hash(source); if (hash != manifest.Sha256) throw new InvalidDataException("Source bank hash mismatch."); FirearmSoundBankBinaryValidator.Validate(source); Directory.CreateDirectory(destinationRoot);
            if (File.Exists(destination) && Hash(destination)==manifest.Sha256) return new FirearmSoundBankStageResult(FirearmSoundBankStageStatus.Skipped,source,destination,hash);
            string temporary=Path.Combine(destinationRoot,".KMG_Firearms."+Guid.NewGuid().ToString("N")+".tmp");
            try { File.Copy(source,temporary,false); if(Hash(temporary)!=manifest.Sha256) throw new InvalidDataException("Temporary bank hash mismatch."); if(File.Exists(destination)) File.Replace(temporary,destination,null); else File.Move(temporary,destination); if(Hash(destination)!=manifest.Sha256) throw new InvalidDataException("Destination bank hash mismatch."); return new FirearmSoundBankStageResult(FirearmSoundBankStageStatus.Copied,source,destination,hash); }
            finally { if(File.Exists(temporary)) File.Delete(temporary); }
        }
        internal static string Hash(string path) { using(SHA256 sha=SHA256.Create()) using(FileStream stream=File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-",string.Empty); }
        private static string Full(string path) { return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar); }
        private static void RequireChild(string root,string path) { if(!path.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Bank path escaped allowlisted root."); }
    }

    internal static class FirearmSoundBankBinaryValidator
    {
        private const int ExpectedMediaCount = 5;

        internal static void Validate(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A bank path is required.", "path");
            Validate(File.ReadAllBytes(path));
        }

        internal static void Validate(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            var chunks = new Dictionary<string, Chunk>(StringComparer.Ordinal);
            int cursor = 0;
            while (cursor < bytes.Length)
            {
                if (bytes.Length - cursor < 8) throw new InvalidDataException("Truncated SoundBank chunk header.");
                string id = Encoding.ASCII.GetString(bytes, cursor, 4);
                uint rawSize = BitConverter.ToUInt32(bytes, cursor + 4);
                if (rawSize > int.MaxValue) throw new InvalidDataException("Oversized SoundBank chunk.");
                int size = (int)rawSize;
                int payload = checked(cursor + 8);
                int end = checked(payload + size);
                if (end > bytes.Length) throw new InvalidDataException("SoundBank chunk exceeds the file boundary.");
                if (chunks.ContainsKey(id)) throw new InvalidDataException("Duplicate SoundBank chunk: " + id);
                chunks.Add(id, new Chunk(payload, size));
                cursor = end;
            }
            if (cursor != bytes.Length || chunks.Count != 4 || !chunks.ContainsKey("BKHD") ||
                !chunks.ContainsKey("DIDX") || !chunks.ContainsKey("DATA") || !chunks.ContainsKey("HIRC"))
                throw new InvalidDataException("The firearm SoundBank must contain exactly BKHD, DIDX, DATA, and HIRC chunks.");
            if (chunks["BKHD"].Size < 4 || chunks["HIRC"].Size == 0)
                throw new InvalidDataException("The firearm SoundBank header or object hierarchy is empty.");

            Chunk index = chunks["DIDX"], data = chunks["DATA"];
            if (index.Size != ExpectedMediaCount * 12 || data.Size == 0)
                throw new InvalidDataException("The firearm SoundBank must embed exactly five media payloads.");
            var mediaIds = new HashSet<uint>();
            ulong previousEnd = 0;
            for (int i = 0; i < ExpectedMediaCount; i++)
            {
                int entry = index.Offset + (i * 12);
                uint mediaId = BitConverter.ToUInt32(bytes, entry);
                uint offset = BitConverter.ToUInt32(bytes, entry + 4);
                uint size = BitConverter.ToUInt32(bytes, entry + 8);
                ulong end = (ulong)offset + size;
                if (mediaId == 0 || !mediaIds.Add(mediaId) || size == 0 ||
                    (ulong)offset < previousEnd || end > (ulong)data.Size)
                    throw new InvalidDataException("Invalid embedded firearm media entry at index " + i + ".");
                previousEnd = end;
            }
        }

        private struct Chunk
        {
            internal Chunk(int offset, int size) { Offset = offset; Size = size; }
            internal int Offset;
            internal int Size;
        }
    }
}
