using System;

namespace KingmakerGunslinger.Audio
{
    internal enum FirearmSoundState { NotConfigured, StagedWaitingForEngine, Loading, Ready, Faulted }
    internal interface IFirearmSoundEngine
    {
        bool IsInitialized();
        void LoadBank(string bankName);
        uint PostEvent(string eventName, object emitter);
    }
    internal sealed class FirearmSoundStateMachine
    {
        private readonly IFirearmSoundEngine _engine;
        internal FirearmSoundStateMachine(IFirearmSoundEngine engine) { _engine = engine ?? throw new ArgumentNullException("engine"); State = FirearmSoundState.NotConfigured; }
        internal FirearmSoundState State { get; private set; }
        internal int LoadAttempts { get; private set; }
        internal int PostAttempts { get; private set; }
        internal int AcceptedPosts { get; private set; }
        internal string LastFault { get; private set; }
        internal void MarkStaged() { if (State == FirearmSoundState.NotConfigured) State = FirearmSoundState.StagedWaitingForEngine; }
        internal bool EnsureReady()
        {
            if (State == FirearmSoundState.Ready) return true;
            if (State != FirearmSoundState.StagedWaitingForEngine) return false;
            try { if (!_engine.IsInitialized()) return false; State = FirearmSoundState.Loading; LoadAttempts++; _engine.LoadBank(FirearmSoundEventCatalog.BankName); State = FirearmSoundState.Ready; return true; }
            catch (Exception e) { Fault(e); return false; }
        }
        internal uint TryPost(string eventName, object emitter)
        {
            PostAttempts++;
            if (string.IsNullOrWhiteSpace(eventName) || emitter == null || !EnsureReady()) return 0;
            try { uint id = _engine.PostEvent(eventName, emitter); if (id != 0) AcceptedPosts++; return id; }
            catch (Exception e) { LastFault = e.GetType().Name + ": " + e.Message; return 0; }
        }
        internal void Fault(Exception error) { LastFault = error == null ? "Unknown audio fault." : error.GetType().Name + ": " + error.Message; State = FirearmSoundState.Faulted; }
    }
}
