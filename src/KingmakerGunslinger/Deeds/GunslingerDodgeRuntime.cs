using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Rules;

namespace KingmakerGunslinger.Deeds
{
    internal static class GunslingerDodgeRuntime
    {
        [ThreadStatic] private static Stack<AttackFrame> _frames;
        private static readonly GunslingerDodgeService Service =
            new GunslingerDodgeService();

        internal static void BeforeAttackRoll(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null) return;
            bool authorized = false;
            try
            {
                UnitEntityData target = attackRoll.Target;
                GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
                GunslingerDodgeBlueprintSet dodge = gunslinger == null ? null : gunslinger.Dodge;
                if (target != null && target.Descriptor != null && dodge != null &&
                    target.Descriptor.HasFact(dodge.ArmedProneMarker))
                {
                    // Migration cleanup only: 0.0.65 persisted an armed marker.
                    // The 0.0.66 ability spends and applies AC at activation.
                    target.Descriptor.RemoveFact(dodge.ArmedProneMarker);
                }
            }
            catch (Exception exception)
            {
                GunslingerDodgeRuntimeDiagnostics.RecordFault(exception);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("dodge", "reaction.failed",
                        "Gunslinger's Dodge failed closed before AC resolution.", exception);
            }
            GetFrames().Push(new AttackFrame(RuntimeHelpers.GetHashCode(attackRoll),
                authorized));
        }

        internal static void AfterCalculateArmorClass(object ruleCalculateArmorClass)
        {
            if (ruleCalculateArmorClass == null || _frames == null ||
                _frames.Count == 0) return;
            AttackFrame frame = _frames.Peek();
            if (!frame.Authorized) return;
            if (frame.Applied)
            {
                GunslingerDodgeRuntimeDiagnostics.RecordDuplicate();
                return;
            }
            try
            {
                // The timed buff contributes +2 through Kingmaker's native stat
                // pipeline. This frame only proves pre-AC reaction timing.
                frame.Applied = true;
            }
            catch (Exception exception)
            {
                GunslingerDodgeRuntimeDiagnostics.RecordFault(exception);
            }
        }

        internal static void AfterAttackRoll(RuleAttackRoll attackRoll)
        {
            if (attackRoll == null || _frames == null) return;
            int identity = RuntimeHelpers.GetHashCode(attackRoll);
            if (_frames.Count == 0 || _frames.Peek().Identity != identity)
            {
                _frames.Clear(); _frames = null; return;
            }
            _frames.Pop(); if (_frames.Count == 0) _frames = null;
        }

        private static GunslingerDodgeArmor ReadArmor(UnitDescriptor descriptor)
        {
            if (descriptor.Body == null || descriptor.Body.Armor == null ||
                !descriptor.Body.Armor.HasArmor || descriptor.Body.Armor.Armor == null ||
                descriptor.Body.Armor.Armor.Blueprint == null ||
                descriptor.Body.Armor.Armor.Blueprint.Type == null)
                return GunslingerDodgeArmor.None;
            ArmorProficiencyGroup group =
                descriptor.Body.Armor.Armor.Blueprint.Type.ProficiencyGroup;
            if (group == ArmorProficiencyGroup.Light) return GunslingerDodgeArmor.Light;
            if (group == ArmorProficiencyGroup.Medium) return GunslingerDodgeArmor.Medium;
            if (group == ArmorProficiencyGroup.Heavy) return GunslingerDodgeArmor.Heavy;
            return GunslingerDodgeArmor.None;
        }

        private static GunslingerDodgeLoad ReadLoad(UnitDescriptor descriptor)
        {
            if (descriptor.Encumbrance == Encumbrance.Light)
                return GunslingerDodgeLoad.Light;
            if (descriptor.Encumbrance == Encumbrance.Medium)
                return GunslingerDodgeLoad.Medium;
            return GunslingerDodgeLoad.Heavy;
        }

        private static Stack<AttackFrame> GetFrames()
        {
            if (_frames == null) _frames = new Stack<AttackFrame>();
            return _frames;
        }

        private sealed class AttackFrame
        {
            internal AttackFrame(int identity, bool authorized)
            { Identity = identity; Authorized = authorized; }
            internal int Identity { get; private set; }
            internal bool Authorized { get; private set; }
            internal bool Applied { get; set; }
        }
    }
}
