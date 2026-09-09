using System;
using System.Collections.Generic;
using System.IO;
using Kingmaker.EntitySystem.Persistence;
using Newtonsoft.Json;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Only the guarded autonomous loader constructs this adapter, after exact
    // catalog/descriptor correlation and before invoking the native slot action.
    internal sealed class GuardedReadOnlySave : ISaver
    {
        private readonly ISaver _native;
        private readonly GuardedSaveLoadCounter _counter;
        private readonly Action<string> _record;
        internal GuardedReadOnlySave(SaveInfo descriptor, Action<string> record)
            : this(descriptor.Saver, new GuardedSaveLoadCounter(
                JsonConvert.SerializeObject(descriptor)), record) { }
        private GuardedReadOnlySave(ISaver native, GuardedSaveLoadCounter counter,
            Action<string> record)
        {
            if (native == null || native.GetType().FullName !=
                "Kingmaker.EntitySystem.Persistence.ZipSaver")
                throw new InvalidOperationException("Only the proven native ZIP save is supported by the read-only loader.");
            _native = native; _counter = counter; _record = record;
        }
        internal ISaver Native { get { return _native; } }
        internal bool Complete { get { return _counter.Complete; } }
        public string ReadHeader() { return _native.ReadHeader(); }
        public string ReadJson(string name) { return _native.ReadJson(name); }
        public StreamReader ReadJsonStream(string name) { return _native.ReadJsonStream(name); }
        public byte[] ReadBytes(string name) { return _native.ReadBytes(name); }
        public List<string> GetAllFiles() { return _native.GetAllFiles(); }
        public void CopyToStash(string name) { _native.CopyToStash(name); }
        public ISaver Clone() { return new GuardedReadOnlySave(_native.Clone(), _counter, _record); }
        public void Dispose() { _native.Dispose(); }
        public void SaveJson(string name, string json)
        {
            _counter.SuppressHeader(name, json);
            _record("single-native-load-counter-update-suppressed;diskWrite=false");
        }
        public void Save()
        {
            _counter.SuppressCommit();
            _native.Dispose();
            _record("single-native-load-header-commit-suppressed;diskWrite=false");
        }
        public void SaveBytes(string name, byte[] bytes) { Reject("SaveBytes"); }
        public void CopyFromStash(string name) { Reject("CopyFromStash"); }
        public void Clear() { Reject("Clear"); }
        private void Reject(string operation)
        {
            _record("unexpected-read-only-save-operation=" + operation);
            throw new InvalidOperationException("A guarded native load cannot write campaign data: " + operation);
        }
    }
}
