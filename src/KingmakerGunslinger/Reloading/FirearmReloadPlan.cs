using System;
using KingmakerGunslinger.Ammunition;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    internal sealed class FirearmReloadPlan
    {
        internal FirearmReloadPlan(FirearmReloadPlanStatus status, string reason,
            object unit, object exactItem, FirearmDefinition definition, FirearmState state,
            ReloadAmmunitionProfile profile, ReloadAmmunitionInventorySnapshot inventory,
            int roundsRequested, int roundsLoadable, EffectiveReloadAction action)
        {
            if (!Enum.IsDefined(typeof(FirearmReloadPlanStatus), status) || status == FirearmReloadPlanStatus.Unknown) throw new ArgumentOutOfRangeException("status");
            if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("A reload-plan reason is required.", "reason");
            Status = status; Reason = reason; Unit = unit; ExactItem = exactItem; Definition = definition;
            State = state; Profile = profile; Inventory = inventory; RoundsRequested = roundsRequested;
            RoundsLoadable = roundsLoadable; Action = action;
            if (status == FirearmReloadPlanStatus.Available && (unit == null || exactItem == null ||
                definition == null || state == null || profile == null || inventory == null ||
                roundsRequested <= 0 || roundsLoadable <= 0 || action == EffectiveReloadAction.Unknown))
                throw new ArgumentException("An available reload plan is incomplete.");
        }
        internal FirearmReloadPlanStatus Status { get; private set; }
        internal string Reason { get; private set; }
        internal object Unit { get; private set; }
        internal object ExactItem { get; private set; }
        internal FirearmDefinition Definition { get; private set; }
        internal FirearmState State { get; private set; }
        internal ReloadAmmunitionProfile Profile { get; private set; }
        internal ReloadAmmunitionInventorySnapshot Inventory { get; private set; }
        internal int RoundsRequested { get; private set; }
        internal int RoundsLoadable { get; private set; }
        internal EffectiveReloadAction Action { get; private set; }
        internal bool IsAvailable { get { return Status == FirearmReloadPlanStatus.Available; } }
    }
}
