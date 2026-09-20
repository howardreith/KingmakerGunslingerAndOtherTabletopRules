using System;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Pure resolution decision for the optional Favored Class per-level
    // feature target. Production maps the installed blueprint children into
    // these game-free candidates and executes exactly this policy, so the
    // deterministic domain suite exercises the real absence, malformed and
    // ambiguity behavior instead of source tokens.
    internal static class FavoredClassTargetPolicy
    {
        internal enum Outcome
        {
            // The optional integration is not installed at all: a safe no-op.
            Absent,
            // Exactly one structurally complete candidate at the required
            // spell level with a valid grant configuration.
            Resolved,
            // More than one candidate matches: the graph is ambiguous and
            // must fail closed without mutation.
            Ambiguous,
            // The integration is present but its structure or grant
            // configuration is incompatible: fail closed without mutation.
            Malformed
        }

        internal sealed class Candidate
        {
            internal string Name;
            internal object SpellList;
            internal object SpellcasterClass;
            internal bool IsLearnSpellParameter;
            internal int SpellLevel;
            // The selection-side contract of the intended per-level child: a
            // specific (non-penalty) spell level on the feature itself plus
            // the expected class-spell-level prerequisite that gates which
            // class levels may pick it.
            internal bool HasValidSelectionContract;
            // The child carries exactly one grant component configured for
            // this class, this shared list and this spell level.
            internal bool HasValidGrantConfiguration;
        }

        internal sealed class Decision
        {
            internal Decision(Outcome outcome, int index, string detail)
            { Outcome = outcome; Index = index; Detail = detail; }
            internal Outcome Outcome { get; private set; }
            // Valid only for Resolved.
            internal int Index { get; private set; }
            internal string Detail { get; private set; }
        }

        // Children may be null (malformed entry). A candidate qualifies only
        // when every structural binding matches the resolved Oracle class
        // contract AND its grant component configuration is valid. The first
        // matching child is never privileged: two qualifying candidates are
        // ambiguous.
        internal static Decision Resolve(Candidate[] children,
            object classSpellList, object spellcasterClass, int spellLevel)
        {
            if (children == null || children.Length == 0)
                return new Decision(Outcome.Malformed, -1,
                    "The Favored Class Oracle selection has no per-level children.");
            int resolved = -1;
            string malformed = null;
            for (int index = 0; index < children.Length; index++)
            {
                Candidate candidate = children[index];
                if (candidate == null)
                {
                    malformed = malformed ?? ("Child " + index + " is null.");
                    continue;
                }
                bool binding = ReferenceEquals(candidate.SpellList, classSpellList) &&
                    ReferenceEquals(candidate.SpellcasterClass, spellcasterClass) &&
                    candidate.IsLearnSpellParameter && candidate.SpellLevel == spellLevel;
                if (!binding) continue;
                if (!candidate.HasValidSelectionContract)
                {
                    malformed = "Child " + index + " (" +
                        (candidate.Name ?? "unnamed") + ") lacks the intended per-level selection contract (specific spell level, zero spell-level penalty, class spell-level prerequisite).";
                    continue;
                }
                if (!candidate.HasValidGrantConfiguration)
                {
                    malformed = "Child " + index + " (" +
                        (candidate.Name ?? "unnamed") + ") lacks the exact LearnSpellParametrized grant configuration.";
                    continue;
                }
                if (resolved >= 0)
                    return new Decision(Outcome.Ambiguous, -1,
                        "Children " + resolved + " and " + index + " both bind to the Oracle class spell list at spell level " + spellLevel + ".");
                resolved = index;
            }
            if (resolved >= 0)
                return new Decision(Outcome.Resolved, resolved,
                    children[resolved].Name ?? "unnamed");
            return new Decision(Outcome.Malformed, -1, malformed ??
                "No per-level child binds to the Oracle class spell list at spell level " + spellLevel + ".");
        }
    }
}
