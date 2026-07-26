namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Object-facing persistence boundary used by firearm services. Sprint 14's
    /// implementation resolves each concrete runtime item to a verified engine-issued
    /// identity before touching primitive UnitPart records. Missing identity fails closed.
    /// </summary>
    internal interface IFirearmStateVaultStore
    {
        bool TryRead(object itemInstance, out FirearmStateData data);

        /// <summary>
        /// Atomically replaces the resolved item's record. Null expectedData means that
        /// no record must exist. Null targetData means that the record is removed.
        /// A failed compare or write must preserve the previously observable record.
        /// </summary>
        void Replace(
            object itemInstance,
            FirearmStateData expectedData,
            FirearmStateData targetData);

        bool Remove(object itemInstance);

        int RecordCount { get; }
    }
}
