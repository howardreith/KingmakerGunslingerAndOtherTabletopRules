namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Primitive persistence boundary keyed only by a canonical engine item identity.
    /// The store contains no runtime item references in its primary Sprint 14 records.
    /// </summary>
    internal interface IFirearmStateIdentityRecordStore
    {
        bool TryRead(FirearmItemId itemId, out FirearmStateData data);

        void Replace(
            FirearmItemId itemId,
            FirearmStateData expectedData,
            FirearmStateData targetData);

        bool Remove(FirearmItemId itemId);

        int RecordCount { get; }
    }
}
