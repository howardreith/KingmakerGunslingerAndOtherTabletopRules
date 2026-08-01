namespace KingmakerGunslinger.Deeds
{
    internal static class PistolWhipRuntimeDiagnostics
    {
        internal static int Applied { get; private set; }
        internal static int Rejected { get; private set; }
        internal static int Hits { get; private set; }
        internal static int Trips { get; private set; }
        internal static int Faults { get; private set; }

        internal static void RecordRejected() { Rejected++; }
        internal static void RecordApplied(bool hit, bool trip)
        {
            Applied++;
            if (hit) Hits++;
            if (trip) Trips++;
        }
        internal static void RecordFault() { Faults++; }
        internal static void Reset()
        {
            Applied = Rejected = Hits = Trips = Faults = 0;
        }
    }
}
