using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Deeds
{
    public sealed class EvasiveGrantController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintAbilityResource Grit;
        public BlueprintCharacterClass GunslingerClass;
        public BlueprintFeature EvasionBenefit;
        public BlueprintFeature UncannyDodgeBenefit;
        public BlueprintFeature ImprovedUncannyDodgeBenefit;

        public override void OnTurnOn() { Refresh(); }
        public override void OnTurnOff() { SetBenefits(false); }

        internal void Refresh()
        {
            Validate();
            int level = Owner.Progression.GetClassLevel(GunslingerClass);
            int grit = Owner.Resources.GetResourceAmount(Grit);
            bool active = Has(EvasionBenefit) && Has(UncannyDodgeBenefit) &&
                Has(ImprovedUncannyDodgeBenefit);
            EvasiveDecision decision = new EvasiveService().Evaluate(
                new EvasiveRequest(level, grit, active));
            if (decision.StateChanges) SetBenefits(decision.ShouldBeActive);
        }

        private void SetBenefits(bool active)
        {
            if (Owner == null) return;
            BlueprintFeature[] benefits = { EvasionBenefit, UncannyDodgeBenefit,
                ImprovedUncannyDodgeBenefit };
            foreach (BlueprintFeature benefit in benefits)
            {
                if (benefit == null) continue;
                if (active && !Owner.HasFact(benefit)) Owner.AddFact(benefit);
                else if (!active && Owner.HasFact(benefit)) Owner.RemoveFact(benefit);
            }
        }

        private bool Has(BlueprintFeature benefit)
        { return Owner != null && benefit != null && Owner.HasFact(benefit); }

        internal void Validate()
        {
            if (Grit == null || GunslingerClass == null || EvasionBenefit == null ||
                UncannyDodgeBenefit == null || ImprovedUncannyDodgeBenefit == null)
                throw new InvalidOperationException(
                    "Evasive conditional grants are not fully configured.");
        }
    }
}
