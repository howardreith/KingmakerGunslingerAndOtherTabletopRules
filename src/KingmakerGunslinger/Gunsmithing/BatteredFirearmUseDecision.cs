using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal sealed class BatteredFirearmUseDecision
    {
        internal BatteredFirearmUseDecision(bool isBattered,
            bool isOriginatingOwner, FirearmCondition actualCondition,
            FirearmCondition effectiveCondition, bool canFire, int saleValueGold)
        {
            if (!Enum.IsDefined(typeof(FirearmCondition), actualCondition) ||
                actualCondition == FirearmCondition.Unknown)
                throw new ArgumentOutOfRangeException("actualCondition");
            if (!Enum.IsDefined(typeof(FirearmCondition), effectiveCondition) ||
                effectiveCondition == FirearmCondition.Unknown)
                throw new ArgumentOutOfRangeException("effectiveCondition");
            if (saleValueGold < 0) throw new ArgumentOutOfRangeException("saleValueGold");
            IsBattered = isBattered;
            IsOriginatingOwner = isOriginatingOwner;
            ActualCondition = actualCondition;
            EffectiveCondition = effectiveCondition;
            CanFire = canFire;
            SaleValueGold = saleValueGold;
        }

        internal bool IsBattered { get; private set; }
        internal bool IsOriginatingOwner { get; private set; }
        internal FirearmCondition ActualCondition { get; private set; }
        internal FirearmCondition EffectiveCondition { get; private set; }
        internal bool CanFire { get; private set; }
        internal int SaleValueGold { get; private set; }
    }
}
