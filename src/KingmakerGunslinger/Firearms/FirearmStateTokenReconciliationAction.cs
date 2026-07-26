namespace KingmakerGunslinger.Firearms
{
    /// <summary>
    /// Pure decision for preserving a state token across Kingmaker's native
    /// ItemEntity.ApplyEnchantments reconciliation pass.
    /// </summary>
    internal enum FirearmStateTokenReconciliationAction
    {
        NoToken = 1,
        Preserved = 2,
        RestoreMissing = 3,
        Conflict = 4
    }
}
