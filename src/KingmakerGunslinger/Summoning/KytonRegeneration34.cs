using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Units;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Summoning
{
    // Reimplement only these two native interfaces. The installed methods are
    // final interface implementations, not overridable virtual methods. Native
    // regeneration still owns healing, difficulty and its death-prevention flag.
    [Serializable]
    public sealed class KytonRegeneration34 : AddEffectRegeneration,
        ITickEachRound, ITargetRulebookHandler<RuleDealDamage>
    {
        // 0 active; 1 waiting for the following round; 2 inside that round.
        // Both suppressed states survive save hydration.
        [JsonProperty] private int suppressionRound;
        internal int SuppressionRound { get { return suppressionRound; } }
        internal bool FollowingRoundPending { get { return suppressionRound == 1; } }
        private static readonly MethodInfo SetActive = typeof(AddEffectRegeneration).GetMethod(
            "SetRegenerationActive", BindingFlags.Instance | BindingFlags.NonPublic,
            null, new[] { typeof(bool) }, null);

        internal static void VerifyContract()
        {
            if (SetActive == null || SetActive.ReturnType != typeof(void) || !SetActive.IsPrivate ||
                typeof(KytonRegeneration34).GetInterfaceMap(typeof(ITickEachRound)).TargetMethods
                    .Single().DeclaringType != typeof(KytonRegeneration34) ||
                typeof(KytonRegeneration34).GetInterfaceMap(typeof(IRulebookHandler<RuleDealDamage>)).TargetMethods
                    .Single(method => method.Name == "OnEventDidTrigger").DeclaringType != typeof(KytonRegeneration34))
                throw new InvalidOperationException("Installed Kyton regeneration callback contract differs.");
        }
        private bool EnabledForOwner => Owner?.Unit != null && InfernalMartialProfile34.For(Owner.Unit)?.Chain != null;
        private void SetNative(bool active) { SetActive.Invoke(this, new object[] { active }); }
        private void SuppressNative() { SetNative(false); }

        public override void OnFactActivate()
        {
            base.OnFactActivate();
            // Native activation otherwise restores its flag during cold hydration.
            if (!EnabledForOwner || suppressionRound != 0) SuppressNative();
        }
        public override void OnTurnOn()
        {
            base.OnTurnOn();
            if (!EnabledForOwner || suppressionRound != 0) SuppressNative();
        }
        public new void OnNewRound()
        {
            if (!EnabledForOwner) { SuppressNative(); return; }
            if (suppressionRound == 1) {
                suppressionRound = 2;
                // Keep the native flag disabled and skip healing for this whole
                // following round. The subsequent native callback restores it.
                return;
            }
            if (suppressionRound == 2) {
                suppressionRound = 0;
                SetNative(true);
            }
            base.OnNewRound();
        }
        public new void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (!EnabledForOwner || evt == null || !ReferenceEquals(evt.Target, Owner.Unit) || evt.Damage <= 0) return;
            base.OnEventDidTrigger(evt);
            bool typed = evt.ResultDamage != null && evt.ResultDamage.Any(result => result.FinalValue > 0 &&
                (result.Source.Alignments.Contains(DamageAlignment.Good) ||
                 (result.Source as PhysicalDamage)?.Materials.Contains(PhysicalDamageMaterial.Silver) == true));
            var ability = evt.SourceAbility ?? evt.Reason?.Ability?.Blueprint ?? evt.Reason?.Context?.SourceAbility;
            bool goodSpell = ability != null && (ability.Type == AbilityType.Spell || ability.Type == AbilityType.SpellLike) &&
                (ability.GetComponent<SpellDescriptorComponent>()?.Descriptor & SpellDescriptor.Good) == SpellDescriptor.Good;
            if (!typed && !goodSpell) return;
            // Native packet alignment handles good/silver weapons. A good spell
            // may instead have only its native source descriptor (Holy Smite).
            // Suppress this component, without rewriting damage or another fact.
            SuppressNative(); suppressionRound = 1;
        }
    }
}
