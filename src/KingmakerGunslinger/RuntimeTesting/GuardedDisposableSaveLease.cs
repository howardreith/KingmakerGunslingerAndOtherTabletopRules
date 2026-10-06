using System;
using System.IO;
using System.Reflection;
using Kingmaker.EntitySystem.Persistence;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Permits one new native manual save. Pre-existing files are never write targets.
    internal sealed class GuardedDisposableSaveLease
    {
        private readonly SaveInfo _requested;
        private readonly string _name, _directory;
        private readonly Action<SaveInfo> _prepared;
        private string _overwritePath, _overwriteHash;
        internal SaveInfo Saved { get; private set; }
        internal int RoutineCount { get; private set; }
        internal int StashedAreaCount { get; private set; }
        internal GuardedDisposableSaveLease(SaveInfo requested, string transaction, string phase,
            string directory, Action<SaveInfo> prepared)
            : this(requested, TeleportPersistenceIdentity.Name(transaction, phase), directory, prepared)
        { }
        internal GuardedDisposableSaveLease(SaveInfo requested, string expectedName,
            string directory, Action<SaveInfo> prepared)
        {
            _name = expectedName;
            if (requested == null || requested.Name != _name || requested.IsActuallySaved ||
                requested.Type != SaveInfo.SaveType.Manual || requested.Saver != null)
                throw new InvalidOperationException("Persistence requires a newly created exact native manual descriptor.");
            _requested = requested; _directory = Path.GetFullPath(directory); _prepared = prepared;
        }
        // Existing-save admission is restricted to the exact transaction-owned
        // descriptor and hash proved by the preceding fresh-process receipt.
        internal static GuardedDisposableSaveLease ForOwnedOverwrite(SaveInfo requested,
            string expectedName, string directory, string ownedPath, string ownedHash, Action<SaveInfo> prepared)
        {
            if (requested == null || !requested.IsActuallySaved || requested.Type != SaveInfo.SaveType.Manual ||
                requested.Name != expectedName || requested.Saver == null ||
                requested.Saver.GetType().FullName != "Kingmaker.EntitySystem.Persistence.ZipSaver" ||
                !ElementalCharacterTraitSaveContract.MatchesFile(expectedName, requested.FileName) ||
                !string.Equals(Path.GetFullPath(requested.FolderName), Path.GetFullPath(ownedPath), StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(Path.GetDirectoryName(Path.GetFullPath(ownedPath)), Path.GetFullPath(directory), StringComparison.OrdinalIgnoreCase) ||
                TeleportPersistencePlan.Hash(ownedPath) != ownedHash)
                throw new InvalidOperationException("Only the exact leased previously-created manual save may be overwritten.");
            return new GuardedDisposableSaveLease(requested, expectedName, directory, ownedPath, ownedHash, prepared);
        }
        private GuardedDisposableSaveLease(SaveInfo requested, string name, string directory,
            string path, string hash, Action<SaveInfo> prepared)
        { _requested=requested; _name=name; _directory=Path.GetFullPath(directory);
          _overwritePath=Path.GetFullPath(path); _overwriteHash=hash; _prepared=prepared; }
        internal static bool IsWrite(MethodBase method)
        {
            return method.DeclaringType == typeof(SaveManager) &&
                (method.Name == "SaveRoutine" || method.Name == "SaveStashedArea" ||
                method.Name == "DeleteSave" || method.Name == "RemoveSaveFromList" || method.Name == "PrepareSave");
        }
        internal bool Enter(MethodBase method, object[] args)
        {
            var descriptor = args == null || args.Length == 0 ? null : args[0] as SaveInfo;
            if (method.Name == "SaveRoutine")
            {
                if (RoutineCount != 0 || !ReferenceEquals(descriptor, _requested) ||
                    (_overwritePath == null ? descriptor.IsActuallySaved : !ExactOwnedOverwrite(descriptor)) || args.Length != 2 || !Equals(args[1], false)) return false;
                RoutineCount++; return true;
            }
            if (method.Name == "PrepareSave")
                return RoutineCount == 1 && Saved == null && descriptor != null && descriptor.Name == _name &&
                    descriptor.Type == SaveInfo.SaveType.Manual && (_overwritePath == null ?
                        !descriptor.IsActuallySaved && descriptor.Saver == null : ExactOwnedOverwrite(descriptor));
            if (method.Name == "SaveStashedArea" && ReferenceEquals(descriptor, Saved) && Saved != null && RoutineCount == 1)
            { StashedAreaCount++; return true; }
            return false;
        }
        private bool ExactOwnedOverwrite(SaveInfo save)
        { return ReferenceEquals(save,_requested) && save.Name==_name && save.IsActuallySaved &&
            Path.GetFullPath(save.FolderName)==_overwritePath && File.Exists(_overwritePath) &&
            TeleportPersistencePlan.Hash(_overwritePath)==_overwriteHash; }
        internal void Prepared(SaveInfo save)
        {
            if (Saved != null || RoutineCount != 1 || save == null || save.Name != _name ||
                save.Type != SaveInfo.SaveType.Manual || save.Saver == null ||
                save.Saver.GetType().FullName != "Kingmaker.EntitySystem.Persistence.ZipSaver" ||
                !TeleportPersistenceIdentity.MatchesFile(_name, save.FileName) ||
                !string.Equals(Path.GetDirectoryName(Path.GetFullPath(save.FolderName)), _directory, StringComparison.OrdinalIgnoreCase) ||
                (_overwritePath == null ? File.Exists(save.FolderName) || Directory.Exists(save.FolderName) : !ExactOwnedOverwrite(save)))
                throw new InvalidOperationException("Native preparation did not produce an absent transaction-owned save path.");
            Saved = save;
            // Durable ownership evidence is written BEFORE the native saver can Clear/Save.
            _prepared(save);
        }
    }
}
