using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum InheritedTemplateDisposition
    {
        /// <summary>An untemplated Summon Monster placement whose donor applies a native celestial or fiendish template after spawn: the owned graph removes exactly the native template identities after the donor's applications.</summary>
        RemoveInherited = 1,
        /// <summary>A Summon Monster placement whose creature is deliberately templated: the project template replaces the native one and is kept.</summary>
        PreserveDeliberateTemplate = 2,
        /// <summary>An untemplated Summon Monster placement whose donor applies no native template: nothing to remove.</summary>
        NoInheritedTemplate = 3,
        /// <summary>A Nature's Ally placement: outside the Summon Monster template contract; its graph is preserved.</summary>
        OutsideSummonMonster = 4
    }

    /// <summary>
    /// S5 T1 / DEF-05: inherited-template cleanup decided per owned placement
    /// from the donor graph that placement actually clones, not from a list of
    /// creature names. The donor is the native Summon Monster carrier selected by
    /// the placement's parent tier and quantity; the same creature therefore
    /// needs removal on some placements and not on others (the v0.0.186 census:
    /// 81 of 281 untemplated placements apply a native template, and no creature
    /// applies on all of its placements). The approved per-creature contract is
    /// unchanged: an untemplated Summon Monster creature receives no celestial or
    /// fiendish template, and a templated one keeps its project template.
    ///
    /// Canonical spells and alternate-caller children both call Decide with the
    /// placement's canonical donor, so they cannot diverge. On an alternate
    /// child whose own host graph applies no native template (PR #138 measured
    /// this; its "kept the donor's template" allegation stays withdrawn) the
    /// mirrored removal is a deliberate, harmless redundancy: the run-time
    /// action removes only those of the four native identities actually present.
    /// </summary>
    internal static class InheritedTemplatePolicy
    {
        internal static InheritedTemplateDisposition Decide(SummonFamily family, bool monsterTemplated,
            IEnumerable<string> donorAppliedTemplateIds, ICollection<string> nativeTemplateIds)
        {
            if (donorAppliedTemplateIds == null) throw new ArgumentNullException("donorAppliedTemplateIds");
            if (nativeTemplateIds == null || nativeTemplateIds.Count == 0)
                throw new ArgumentException("The native template identities are required.", "nativeTemplateIds");
            if (family != SummonFamily.Monster) return InheritedTemplateDisposition.OutsideSummonMonster;
            if (monsterTemplated) return InheritedTemplateDisposition.PreserveDeliberateTemplate;
            return donorAppliedTemplateIds.Any(nativeTemplateIds.Contains)
                ? InheritedTemplateDisposition.RemoveInherited
                : InheritedTemplateDisposition.NoInheritedTemplate;
        }
    }
}
