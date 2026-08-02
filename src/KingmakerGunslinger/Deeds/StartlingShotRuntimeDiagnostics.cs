namespace KingmakerGunslinger.Deeds
{
    internal static class StartlingShotRuntimeDiagnostics
    {
        internal static int Applied { get; private set; }
        internal static int Rejected { get; private set; }
        internal static int Faults { get; private set; }
        internal static void RecordApplied() { Applied++; }
        internal static void RecordRejected() { Rejected++; }
        internal static void RecordFault() { Faults++; }
        internal static void Reset() { Applied = Rejected = Faults = 0; }
    }
}
