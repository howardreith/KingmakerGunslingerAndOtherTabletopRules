using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Gunsmithing
{
    internal static class BatteredFirearmUsePolicy
    {
        internal const int FixedExpectedScrapValueGold = 22;

        internal static BatteredFirearmUseDecision Evaluate(bool isBattered,
            bool isOriginatingOwner, FirearmCondition actualCondition,
            int ordinarySaleValueGold)
        {
            if (!Enum.IsDefined(typeof(FirearmCondition), actualCondition) ||
                actualCondition == FirearmCondition.Unknown)
                throw new ArgumentOutOfRangeException("actualCondition");
            if (ordinarySaleValueGold < 0)
                throw new ArgumentOutOfRangeException("ordinarySaleValueGold");

            if (!isBattered)
                return new BatteredFirearmUseDecision(false, false,
                    actualCondition, actualCondition,
                    actualCondition != FirearmCondition.Wrecked,
                    ordinarySaleValueGold);

            if (isOriginatingOwner)
                return new BatteredFirearmUseDecision(true, true,
                    actualCondition, actualCondition,
                    actualCondition != FirearmCondition.Wrecked,
                    FixedExpectedScrapValueGold);

            FirearmCondition effective = actualCondition == FirearmCondition.Normal
                ? FirearmCondition.Broken
                : FirearmCondition.Wrecked;
            return new BatteredFirearmUseDecision(true, false,
                actualCondition, effective,
                effective != FirearmCondition.Wrecked,
                FixedExpectedScrapValueGold);
        }
    }
}
