using KingmakerGunslinger.Firearms;
namespace KingmakerGunslinger.Audio
{
    internal sealed class FirearmSoundPostResult
    {
        internal FirearmSoundPostResult(bool accepted, FirearmKind kind, string eventName, string source, string emitter, uint playingId, string fault)
        { Accepted=accepted; Kind=kind; EventName=eventName; Source=source; Emitter=emitter; PlayingId=playingId; Fault=fault; }
        internal bool Accepted { get; private set; } internal FirearmKind Kind { get; private set; }
        internal string EventName { get; private set; } internal string Source { get; private set; }
        internal string Emitter { get; private set; } internal uint PlayingId { get; private set; } internal string Fault { get; private set; }
    }
}
