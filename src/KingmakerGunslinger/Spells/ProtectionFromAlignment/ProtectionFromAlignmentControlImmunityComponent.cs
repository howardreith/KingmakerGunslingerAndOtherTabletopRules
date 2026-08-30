using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    [AllowMultipleComponents]
    public sealed class ProtectionFromAlignmentControlImmunityComponent :
        RuleInitiatorLogicComponent<RuleApplyBuff>
    {
        public AlignmentComponent ProtectedAgainstAlignment;

        public override void OnEventAboutToTrigger(RuleApplyBuff evt)
        {
            if (evt == null || !evt.CanApply || evt.Blueprint == null) return;
            ProtectionAlignment protectedAgainst;
            if (!ProtectionFromAlignmentRuntime.TryMapProtectedAlignment(
                ProtectedAgainstAlignment, out protectedAgainst)) return;

            MechanicsContext context = evt.Context;
            BlueprintAbility ability = context == null ? null : context.SourceAbility;
            UnitEntityData source = context == null ? null : context.MaybeCaster;
            bool sourceClassified = source != null && source.Descriptor != null &&
                source.Descriptor.Alignment != null;
            ProtectionAlignment sourceAlignment = sourceClassified ?
                ProtectionFromAlignmentRuntime.FromNativeAlignment(
                    source.Descriptor.Alignment.Value) : ProtectionAlignment.None;
            string abilityGuid = ability == null ? null : ability.AssetGuid;
            string buffGuid = evt.Blueprint.AssetGuid;
            ProtectionControlImmunityDecision decision =
                ProtectionFromAlignmentRuntime.Evaluate(
                    new ProtectionControlImmunityRequest(protectedAgainst,
                        abilityGuid, buffGuid, sourceClassified, sourceAlignment));
            if (decision.QualifyingControl && !sourceClassified)
                ProtectionFromAlignmentRuntime.ReportUnresolvedSourceOnce(
                    abilityGuid, buffGuid, decision);
            if (decision.ShouldBlock)
                evt.CanApply = false;
        }

        public override void OnEventDidTrigger(RuleApplyBuff evt) { }
    }
}
