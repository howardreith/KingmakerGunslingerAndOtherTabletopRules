namespace KingmakerGunslinger.Deeds
{
    internal sealed class LightningReloadDecision
    {
        internal LightningReloadDecision(LightningReloadStatus status,
            LightningReloadAction action = LightningReloadAction.Swift)
        {
            Status = status;
            Action = status == LightningReloadStatus.Available
                ? action : LightningReloadAction.Unknown;
        }

        public LightningReloadStatus Status { get; private set; }
        public bool IsAvailable { get { return Status == LightningReloadStatus.Available; } }
        public int RoundsToLoad { get { return IsAvailable ? 1 : 0; } }
        public int GritCost { get { return 0; } }
        public bool MarkUsedOnSuccess { get { return IsAvailable; } }
        public LightningReloadAction Action { get; private set; }
    }
}
