using System.Linq;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Reloading
{
    /// <summary>
    /// Shared evaluation rules for the Rapid Reload proficiency gate.
    /// Production builds the parent selection's OR-grouped prerequisites from
    /// <see cref="ParentGateKinds"/>, and the guarded runtime scenario and the
    /// domain suite score observed evidence with the same predicates, so a
    /// corrupted expectation cannot pass in one place and fail in the other.
    /// Proficiency identity is always a proficiency fact; class levels,
    /// archetypes and the legacy compatibility wrapper are never consulted.
    /// </summary>
    internal static class RapidReloadPrerequisiteRules
    {
        /// <summary>
        /// The currently published official firearm kinds. The parent
        /// selection carries exactly one proficiency check per entry;
        /// compatibility-only retired kinds are deliberately excluded so the
        /// gate cannot be satisfied by an unpublished choice.
        /// </summary>
        internal static FirearmKind[] ParentGateKinds
        {
            get { return OfficialFirearmSupport.Kinds; }
        }

        /// <summary>
        /// Parent requirement: proficient with at least one currently
        /// published official firearm kind. This answers only "can this
        /// character qualify for a firearm Rapid Reload choice?".
        /// </summary>
        internal static bool ParentQualifies(bool hasFullProficiency,
            bool hasOneHandedProficiency, bool hasTwoHandedProficiency)
        {
            return ParentGateKinds.Any(kind => ChildQualifies(kind,
                hasFullProficiency, hasOneHandedProficiency,
                hasTwoHandedProficiency));
        }

        /// <summary>
        /// Child requirement: proficiency covering that exact firearm. This
        /// stays kind-exact, so one-handed proficiency never unlocks a
        /// two-handed choice and vice versa.
        /// </summary>
        internal static bool ChildQualifies(FirearmKind kind,
            bool hasFullProficiency, bool hasOneHandedProficiency,
            bool hasTwoHandedProficiency)
        {
            return FirearmProficiencyPolicy.CanUse(1, kind, hasFullProficiency,
                hasOneHandedProficiency, hasTwoHandedProficiency);
        }
    }
}
