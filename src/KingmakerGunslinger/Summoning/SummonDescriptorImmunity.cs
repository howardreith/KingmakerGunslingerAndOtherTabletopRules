using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Asks the installed engine whether a unit is already immune to the
    /// effect a context describes, without executing anything.
    ///
    /// <para><b>Why both predicates.</b> Kingmaker adjudicates descriptor
    /// immunity in two separate places and neither implies the other.
    /// <c>SpellImmunityToSpellDescriptor</c> registers an entry in
    /// <c>UnitPartSpellResistance</c>, answered by that part's
    /// <c>IsImmune</c>. <c>BuffDescriptorImmunity</c> never touches that part
    /// at all: it is an <c>IInitiatorRulebookHandler&lt;RuleApplyBuff&gt;</c>
    /// that cancels the application as it happens. A caller that consults only
    /// the first will mis-answer a creature protected solely by the second —
    /// which is exactly the gap this closes for the Vrock's spores, where the
    /// opening damage packet is decided before any buff is applied.</para>
    ///
    /// <para>The released <c>SatyrControlEligibility.ControlTargetIsEligible</c>
    /// already asks this same pair for its own target choice. This is that
    /// small predicate pair named once; none of the Satyr's hostile-control,
    /// fear, hit-dice or faction restrictions comes with it.</para>
    ///
    /// <para><b>What it deliberately does not consult.</b> Numerical spell
    /// resistance. <c>UnitPartSpellResistance.IsImmune</c> enumerates only
    /// <c>m_SpellImmunities</c> and calls <c>SpellImmunity.CanApply</c>; the
    /// numerical <c>m_SRs</c> list is reached only through <c>GetValue</c>,
    /// which nothing here calls. So an extraordinary effect that bypasses
    /// spell resistance cannot be stopped by an SR score, and a creature's
    /// poison immunity cannot answer for a disease descriptor, because
    /// <c>CanApply</c> matches its own immunity type and descriptor.</para>
    ///
    /// <para>Calling the engine's own predicates rather than testing
    /// descriptor bits is what preserves the conditional parts of an immunity:
    /// <c>SpellImmunity.CasterIgnoreImmunityFact</c> and
    /// <c>BuffDescriptorImmunity</c>'s <c>IgnoreFeature</c>, <c>CheckFact</c>
    /// and <c>FactToCheck</c> all keep working, including a caster that is
    /// meant to bypass the immunity.</para>
    /// </summary>
    internal static class SummonDescriptorImmunity
    {
        // Installed 2.1.7b keeps this predicate private, as the released
        // Satyr selector also records. Calling it preserves caster exceptions
        // and conditional facts, unlike testing descriptor flags alone.
        private static readonly MethodInfo BuffImmunity = AccessTools.Method(
            typeof(BuffDescriptorImmunity), "IsImmune",
            new[] { typeof(MechanicsContext) }) ??
            throw new MissingMethodException(
                typeof(BuffDescriptorImmunity).FullName, "IsImmune");

        /// <summary>
        /// Whether the engine would already refuse <paramref name="effect"/>
        /// on <paramref name="target"/>. Executes no rule, applies no buff and
        /// debits no resource.
        /// </summary>
        internal static bool IsImmune(UnitEntityData target,
            MechanicsContext effect)
        {
            if (target == null || target.Descriptor == null || effect == null)
                return false;
            UnitPartSpellResistance registered =
                target.Get<UnitPartSpellResistance>();
            if (registered != null && registered.IsImmune(effect)) return true;
            // Only facts that are actually switched on can protect anything,
            // which is what keeps a dormant or suppressed immunity honest.
            Fact[] active = target.Descriptor.Progression.Features.Enumerable
                .Cast<Fact>().Concat(target.Descriptor.Buffs.Enumerable)
                .Where(fact => fact != null && fact.IsTurnedOn).ToArray();
            return active
                .SelectMany(fact => fact.SelectComponents<BuffDescriptorImmunity>())
                .Any(component =>
                    (bool)BuffImmunity.Invoke(component, new object[] { effect }));
        }
    }
}
