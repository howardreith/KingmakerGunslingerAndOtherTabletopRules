using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum ComposedActionKind { Template, Alignment, Other }

    /// <summary>
    /// One after-spawn action of a summon, reduced to the values its
    /// behavior depends on. Only this module's two composed actions carry
    /// values: the caster-selected template (its four template and smite
    /// buffs, the neutral-mode buff and the native template buffs it
    /// replaces) and the summon alignment (its mode). Any other action is
    /// kept as its exact type.
    /// </summary>
    internal sealed class ComposedActionView
    {
        internal ComposedActionKind Kind;
        internal string Type;
        internal string CelestialTemplate;
        internal string FiendishTemplate;
        internal string CelestialSmite;
        internal string FiendishSmite;
        internal string NeutralFiendishMode;
        internal string[] ReplacedNativeTemplateBuffs = new string[0];
        internal string AlignmentMode;

        internal static ComposedActionView Other(string type)
        { return new ComposedActionView { Kind = ComposedActionKind.Other, Type = type }; }
    }

    /// <summary>
    /// The specific contract for the after-spawn actions this module composes
    /// into an adapter creature child, checked against the canonical child of
    /// the same placement. It is deliberately narrow: it covers the values the
    /// summon signature records only by type, and nothing else.
    /// <list type="bullet">
    /// <item>Every spawn branch ends with exactly one composed action of the
    /// canonical kind, and no branch carries a composed action of the other
    /// kind (placement).</item>
    /// <item>A template action's celestial and fiendish template, smite and
    /// neutral-mode buffs are the canonical ones.</item>
    /// <item>Its replaced native template buffs are the canonical set. The
    /// action removes each listed buff that is present, so order and repeats
    /// carry no behavior and are not compared; a missing, extra or null entry
    /// is a difference.</item>
    /// <item>An alignment action's mode is the canonical mode.</item>
    /// </list>
    /// A child that should compose nothing (a retained native option or a
    /// preserved cast) may carry no composed action at all.
    /// </summary>
    internal static class AlternateSummoningComposition
    {
        /// <summary>The single composed action every spawn of a canonical
        /// child carries, or null when it composes none; a canonical child
        /// whose branches disagree is itself refused.</summary>
        internal static ComposedActionView Expected(
            IReadOnlyList<IReadOnlyList<ComposedActionView>> canonicalBranches,
            out string inconsistency)
        {
            inconsistency = null;
            if (canonicalBranches == null || canonicalBranches.Count == 0)
            {
                inconsistency = "canonical-has-no-spawn-branch";
                return null;
            }
            ComposedActionView[] composed = canonicalBranches.SelectMany(branch =>
                branch.Where(value => value.Kind != ComposedActionKind.Other)).ToArray();
            if (composed.Length == 0) return null;
            ComposedActionView first = composed[0];
            if (composed.Any(value => Describe(value) != Describe(first)) ||
                canonicalBranches.Any(branch => branch.Count(value =>
                    value.Kind != ComposedActionKind.Other) != 1))
            {
                inconsistency = "canonical-composition-inconsistent";
                return null;
            }
            return first;
        }

        /// <summary>Null when the child keeps the contract, otherwise the
        /// first precise difference.</summary>
        internal static string Verify(IReadOnlyList<IReadOnlyList<ComposedActionView>> branches,
            ComposedActionView expected)
        {
            if (branches == null || branches.Count == 0) return "no-spawn-branch";
            for (int index = 0; index < branches.Count; index++)
            {
                IReadOnlyList<ComposedActionView> branch = branches[index];
                ComposedActionView[] composed = branch.Where(value =>
                    value.Kind != ComposedActionKind.Other).ToArray();
                if (expected == null)
                {
                    if (composed.Length != 0)
                        return "placement:branch" + index + ":unexpected-" +
                            composed[0].Kind.ToString().ToLowerInvariant();
                    continue;
                }
                if (composed.Length != 1 || composed[0].Kind != expected.Kind ||
                    !ReferenceEquals(branch[branch.Count - 1], composed[0]))
                    return "placement:branch" + index + ":expected-one-final-" +
                        expected.Kind.ToString().ToLowerInvariant() + ";actual=[" +
                        string.Join(",", branch.Select(value =>
                            value.Kind == ComposedActionKind.Other ? value.Type :
                                value.Kind.ToString().ToLowerInvariant()).ToArray()) + "]";
                string difference = Compare(expected, composed[0]);
                if (difference != null) return "branch" + index + ":" + difference;
            }
            return null;
        }

        private static string Compare(ComposedActionView expected, ComposedActionView actual)
        {
            if (expected.Kind == ComposedActionKind.Alignment)
                return string.Equals(expected.AlignmentMode, actual.AlignmentMode,
                    StringComparison.Ordinal) ? null : "alignment-mode:expected=" +
                        expected.AlignmentMode + ";actual=" + actual.AlignmentMode;
            foreach (var field in new[] {
                Tuple.Create("celestial-template", expected.CelestialTemplate, actual.CelestialTemplate),
                Tuple.Create("fiendish-template", expected.FiendishTemplate, actual.FiendishTemplate),
                Tuple.Create("celestial-smite", expected.CelestialSmite, actual.CelestialSmite),
                Tuple.Create("fiendish-smite", expected.FiendishSmite, actual.FiendishSmite),
                Tuple.Create("neutral-fiendish-mode", expected.NeutralFiendishMode,
                    actual.NeutralFiendishMode) })
                if (!string.Equals(field.Item2, field.Item3, StringComparison.Ordinal))
                    return "template-buffs:" + field.Item1 + ":expected=" + field.Item2 +
                        ";actual=" + field.Item3;
            string[] actualReplaced = actual.ReplacedNativeTemplateBuffs ?? new string[0];
            if (actualReplaced.Any(value => value == null))
                return "replaced-native-template-buffs:null-entry";
            var want = new HashSet<string>(expected.ReplacedNativeTemplateBuffs ??
                new string[0], StringComparer.Ordinal);
            var have = new HashSet<string>(actualReplaced, StringComparer.Ordinal);
            if (!want.SetEquals(have))
                return "replaced-native-template-buffs:missing=[" + string.Join(",",
                    want.Except(have).OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray()) + "];extra=[" + string.Join(",", have.Except(want)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray()) + "]";
            return null;
        }

        internal static string Describe(ComposedActionView value)
        {
            if (value == null) return "<none>";
            if (value.Kind == ComposedActionKind.Other) return value.Type;
            if (value.Kind == ComposedActionKind.Alignment) return "alignment{mode=" +
                value.AlignmentMode + "}";
            return "template{celestial=" + value.CelestialTemplate + ";fiendish=" +
                value.FiendishTemplate + ";celestialSmite=" + value.CelestialSmite +
                ";fiendishSmite=" + value.FiendishSmite + ";neutral=" +
                value.NeutralFiendishMode + ";replaced=[" + string.Join(",",
                    (value.ReplacedNativeTemplateBuffs ?? new string[0])
                    .Where(item => item != null).Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal).ToArray()) + "]}";
        }
    }
}
