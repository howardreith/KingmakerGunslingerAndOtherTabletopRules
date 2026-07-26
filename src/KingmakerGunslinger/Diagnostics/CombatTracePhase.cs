namespace KingmakerGunslinger.Diagnostics
{
    /// <summary>
    /// Whether a diagnostic snapshot was captured before or after an event's OnTrigger method.
    /// </summary>
    internal enum CombatTracePhase
    {
        Before = 0,
        After = 1
    }
}
