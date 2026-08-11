using System;
using System.Linq;
using System.Reflection;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.ShieldOther
{
    internal static class ShieldOtherRuntime
    {
        private static readonly MethodInfo DamageSetter = typeof(RuleDealDamage)
            .GetProperty("Damage").GetSetMethod(true);
        [ThreadStatic] private static int _transferDepth;
        [ThreadStatic] private static int _forcedTransferredDamage;

        internal static bool IsTransferredEvent { get { return _transferDepth > 0; } }

        internal static void AfterFinalDamage(RuleDealDamage damage)
        {
            if (damage == null) return;
            if (IsTransferredEvent)
            {
                SetDamage(damage, Math.Max(0, _forcedTransferredDamage));
                return;
            }

            int finalized = Math.Max(0, damage.Damage);
            UnitEntityData subject = damage.Target;
            Buff link;
            UnitEntityData caster;
            bool valid = TryResolveValidLink(subject, out link, out caster);
            ShieldOtherDamageSplit split = ShieldOtherDamageSplitPolicy.Split(
                finalized, valid, false);
            if (!split.Transfers) return;

            SetDamage(damage, split.SubjectShare);
            try
            {
                var packet = new DirectDamage(new DiceFormula(0, DiceType.D6),
                    split.CasterShare);
                var transfer = new RuleDealDamage(subject, caster,
                    new DamageBundle(packet)) {
                    DisablePrecisionDamage = true,
                    IgnoreDamageReduction = true
                };
                _transferDepth++;
                _forcedTransferredDamage = split.CasterShare;
                try
                {
                    Rulebook.Trigger(transfer);
                }
                finally
                {
                    _forcedTransferredDamage = 0;
                    _transferDepth--;
                }
                ShieldOtherCombatLog.Publish(subject.CharacterName,
                    caster.CharacterName, split.CasterShare);
            }
            catch (Exception exception)
            {
                SetDamage(damage, finalized);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("shield-other", "transfer.failed",
                        "Shield Other restored full subject damage after the guarded transfer failed.",
                        exception);
            }
        }

        private static void SetDamage(RuleDealDamage damage, int value)
        {
            if (DamageSetter == null)
                throw new MissingMethodException(typeof(RuleDealDamage).FullName,
                    "set_Damage");
            DamageSetter.Invoke(damage, new object[] { value });
        }

        private static bool TryResolveValidLink(UnitEntityData subject,
            out Buff link, out UnitEntityData caster)
        {
            link = null;
            caster = null;
            if (subject == null || subject.Descriptor == null ||
                BlueprintBootstrap.ShieldOther == null) return false;
            Buff[] links = subject.Descriptor.Buffs.RawFacts.OfType<Buff>()
                .Where(value => ReferenceEquals(value.Blueprint,
                    BlueprintBootstrap.ShieldOther.TargetBuff)).Take(2).ToArray();
            if (links.Length != 1) return false;
            link = links[0];
            ShieldOtherLinkValidityDecision decision;
            ShieldOtherBuffComponent.TryEvaluate(link, out decision);
            if (decision == null || !decision.Valid)
            {
                link.Remove();
                link = null;
                return false;
            }
            caster = link.MaybeContext == null ? null : link.MaybeContext.MaybeCaster;
            return caster != null;
        }
    }
}
