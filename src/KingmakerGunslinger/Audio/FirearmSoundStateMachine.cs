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
        internal string LastFailureStage { get; private set; }
        internal void MarkStaged() { if (State == FirearmSoundState.NotConfigured) State = FirearmSoundState.StagedWaitingForEngine; }
        internal bool EnsureReady()
        {
            if (State == FirearmSoundState.Ready) return true;
            if (State != FirearmSoundState.StagedWaitingForEngine) return false;
            try
            {
                if (!_engine.IsInitialized()) return false;
                State = FirearmSoundState.Loading;
                LoadAttempts++;
                _engine.LoadBank(FirearmSoundEventCatalog.BankName);
                LastFailureStage = null;
                LastFault = null;
                State = FirearmSoundState.Ready;
                return true;
            }
            catch (Exception e)
            {
                Fault("bank.loading", e);
                return false;
            }
        }
        internal uint TryPost(string eventName, object emitter)
        {
            PostAttempts++;
            if (string.IsNullOrWhiteSpace(eventName))
            {
                Reject("post-event.input", "PostEvent rejected: event name is empty.");
                return 0;
            }
            if (emitter == null)
            {
                Reject("post-event.input", "PostEvent rejected: emitter is null.");
                return 0;
            }
            if (!EnsureReady())
            {
                if (string.IsNullOrEmpty(LastFault))
                    Reject("post-event.not-ready", "PostEvent rejected: SoundBank is not ready.");
                return 0;
            }
            try
            {
                uint id = _engine.PostEvent(eventName, emitter);
                if (id == 0)
                {
                    Reject(
                        "post-event.rejected",
                        "PostEvent rejected: event=" + eventName +
                        "; playingId=0.");
                    return 0;
                }
                AcceptedPosts++;
                LastFailureStage = null;
                LastFault = null;
                return id;
            }
            catch (Exception e)
            {
                Reject(
                    "post-event.exception",
                    "PostEvent failed: " + e.GetType().Name + ": " + e.Message);
                return 0;
            }
        }
        internal void Fault(Exception error)
        {
            Fault("configuration", error);
        }

        internal void Fault(string stage, Exception error)
        {
            LastFailureStage = string.IsNullOrWhiteSpace(stage) ?
                "configuration" : stage;
            LastFault = error == null ? "Unknown audio fault." :
                error.GetType().Name + ": " + error.Message;
            State = FirearmSoundState.Faulted;
        }

        private void Reject(string stage, string fault)
        {
            LastFailureStage = stage;
            LastFault = fault;
        }
    }
}
