using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Classes
{
    internal static class FirearmTrainingRuntime
    {
        private static readonly ConditionalWeakTable<RuleCalculateWeaponStats,
            object> Applied = new ConditionalWeakTable<RuleCalculateWeaponStats, object>();
        private static BlueprintFeature _pistolTraining;
        private static BlueprintFeature _musketTraining;

        internal static void Configure(BlueprintFeature pistolTraining,
            BlueprintFeature musketTraining)
        {
            _pistolTraining = pistolTraining;
            _musketTraining = musketTraining;
        }

        internal static FirearmTrainingEntitlement Resolve(UnitEntityData owner,
            FirearmKind kind)
        {
            if (owner == null || owner.Descriptor == null)
                return new FirearmTrainingEntitlement(false, 0, false);
            GunslingerClassBlueprintSet gunslinger = BlueprintBootstrap.GunslingerClass;
            bool exact = gunslinger != null && gunslinger.GunTraining != null &&
                GunTrainingPolicy.IsRecognizedKind(kind) && owner.Descriptor.HasFact(
                    gunslinger.GunTraining.ChoiceFor(kind));
            int pistolRank = _pistolTraining == null ? 0 :
                owner.Descriptor.Progression.Features.GetRank(_pistolTraining);
            int musketRank = _musketTraining == null ? 0 :
                owner.Descriptor.Progression.Features.GetRank(_musketTraining);
            return FirearmTrainingPolicy.Evaluate(kind,
                owner.Stats.Dexterity.Bonus, exact, pistolRank, musketRank);
        }

        internal static void ApplyDamageOnce(RuleCalculateWeaponStats evt)
        {
            if (evt == null || evt.Initiator == null || evt.Weapon == null ||
                evt.Weapon.Blueprint == null || evt.Weapon.Blueprint.Type == null)
                return;
            FirearmDefinitionComponent[] markers = evt.Weapon.Blueprint.Type
                .ComponentsArray.OfType<FirearmDefinitionComponent>().ToArray();
            if (markers.Length != 1) return;
            FirearmDefinition definition;
            try { definition = markers[0].Definition; }
            catch (Exception) { return; }
            FirearmTrainingEntitlement entitlement = Resolve(evt.Initiator,
                definition.Kind);
            if (!entitlement.Eligible || Applied.TryGetValue(evt, out _)) return;
            Applied.Add(evt, new object());
            if (entitlement.DamageBonus != 0)
                evt.AddBonusDamage(entitlement.DamageBonus);
        }
    }
}
