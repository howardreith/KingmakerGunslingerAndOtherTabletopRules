using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    /// <summary>
    /// One mechanical predicate of the owned Elven Branched Spear weapon type,
    /// captured as invariant text so the verdict and its diagnostics do not
    /// depend on Kingmaker types.
    /// </summary>
    internal sealed class SpearProfileObservation
    {
        internal SpearProfileObservation(string predicate,
            string expectedField, string ownedField, string donorField,
            string engineExpected, string engineActual, string donorEngine,
            IEnumerable<string> getterPatchOwners, string patchInfo)
        {
            if (string.IsNullOrWhiteSpace(predicate))
                throw new ArgumentException("A spear profile predicate needs a name.",
                    "predicate");
            Predicate = predicate;
            ExpectedField = expectedField ?? "<null>";
            OwnedField = ownedField ?? "<null>";
            DonorField = donorField ?? "<null>";
            EngineExpected = engineExpected ?? "<null>";
            EngineActual = engineActual ?? "<null>";
            DonorEngine = donorEngine ?? "<null>";
            GetterPatchOwners = (getterPatchOwners ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            PatchInfo = string.IsNullOrWhiteSpace(patchInfo) ? "not-read" : patchInfo;
        }

        /// <summary>Stable predicate name, for example <c>attack-range</c>.</summary>
        internal string Predicate { get; private set; }

        /// <summary>Serialized value the owned clone must carry.</summary>
        internal string ExpectedField { get; private set; }

        /// <summary>Serialized value the owned clone actually carries.</summary>
        internal string OwnedField { get; private set; }

        /// <summary>The same serialized field on the native Longspear donor.</summary>
        internal string DonorField { get; private set; }

        /// <summary>
        /// What the unpatched Kingmaker 2.1.7b getter returns for the owned
        /// field (identity for plain getters; the reach formula for range).
        /// </summary>
        internal string EngineExpected { get; private set; }

        /// <summary>What the live getter returned for the owned clone.</summary>
        internal string EngineActual { get; private set; }

        /// <summary>What the live getter returned for the native donor.</summary>
        internal string DonorEngine { get; private set; }

        /// <summary>Harmony owners patching the getter, when it was read.</summary>
        internal string[] GetterPatchOwners { get; private set; }

        /// <summary><c>read</c>, <c>not-read</c>, <c>field</c> or an unavailable reason.</summary>
        internal string PatchInfo { get; private set; }

        internal bool OwnedFieldMatches
        {
            get { return string.Equals(ExpectedField, OwnedField, StringComparison.Ordinal); }
        }

        /// <summary>
        /// True when the effective value equals the engine contract, or when
        /// it was deliberately not read in this (serialized-only) validation.
        /// </summary>
        internal bool EngineMatches
        {
            get
            {
                return string.Equals(EngineActual, ElvenBranchedSpearProfilePolicy.Deferred,
                        StringComparison.Ordinal) ||
                    string.Equals(EngineExpected, EngineActual, StringComparison.Ordinal);
            }
        }
    }

    internal enum SpearProfileFindingKind
    {
        /// <summary>The owned clone's serialized field is wrong. Fatal.</summary>
        OwnedFieldMismatch = 0,

        /// <summary>
        /// The owned field is right but the live getter - what players and the
        /// engine actually use - disagrees with the engine contract for that
        /// field, or throws. Fatal whoever patches the getter: patch owners are
        /// reported as context, never as an exemption.
        /// </summary>
        EffectiveGetterMismatch = 1
    }

    internal sealed class SpearProfileFinding
    {
        internal SpearProfileFinding(SpearProfileFindingKind kind,
            SpearProfileObservation observation)
        {
            Kind = kind;
            Observation = observation;
        }

        internal SpearProfileFindingKind Kind { get; private set; }
        internal SpearProfileObservation Observation { get; private set; }
    }

    internal sealed class SpearProfileVerdict
    {
        internal SpearProfileVerdict(IEnumerable<SpearProfileFinding> findings,
            int checkedCount)
        {
            Findings = (findings ?? Enumerable.Empty<SpearProfileFinding>()).ToArray();
            CheckedCount = checkedCount;
        }

        internal SpearProfileFinding[] Findings { get; private set; }
        internal int CheckedCount { get; private set; }

        internal bool IsValid { get { return Findings.Length == 0; } }

        /// <summary>
        /// One bounded message naming every failed predicate of this
        /// validation with its expected, actual and donor values and the
        /// getter's patch owners.
        /// </summary>
        internal string DescribeFailure()
        {
            if (Findings.Length == 0) return string.Empty;
            return ElvenBranchedSpearProfilePolicy.FailurePrefix + ": " +
                Findings.Length.ToString(CultureInfo.InvariantCulture) + " of " +
                CheckedCount.ToString(CultureInfo.InvariantCulture) +
                " checks failed. " + ElvenBranchedSpearProfilePolicy.Describe(Findings);
        }
    }

    internal static class ElvenBranchedSpearProfilePolicy
    {
        internal const string FailurePrefix = "Elven Branched Spear type profile is invalid";

        /// <summary>
        /// Recorded for an effective value that a serialized-only validation
        /// deliberately does not read (the clone does not yet have its released
        /// identity). It is never judged; the definitive validation reads it.
        /// </summary>
        internal const string Deferred = "<deferred>";

        /// <summary>The spear's serialized reach, as on the native Longspear.</summary>
        internal const int ReachFeet = 10;

        /// <summary>Kingmaker 2.1.7b <c>GameConsts.MinWeaponRange</c>.</summary>
        internal const int EngineMinimumWeaponRangeFeet = 2;

        internal const int MaxDescribedValueLength = 96;
        internal const int MaxDescribedOwners = 8;
        internal const int MaxDescribedFindings = 24;

        /// <summary>
        /// Kingmaker 2.1.7b <c>BlueprintWeaponType.AttackRange</c>: a serialized
        /// range of at most 10 ft is reduced by 4 ft (never below the minimum
        /// weapon range); a longer range is returned unchanged.
        /// </summary>
        internal static int EngineAttackRangeFeet(float serializedFeet)
        {
            if (!(serializedFeet > 10f))
                return Math.Max(EngineMinimumWeaponRangeFeet, (int)serializedFeet - 4);
            return (int)serializedFeet;
        }

        /// <summary>
        /// Text recorded in place of a value whose getter threw. It never equals
        /// an expected value, so a throwing owned getter is a mismatch.
        /// </summary>
        internal static string Threw(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            return "<throws " + exception.GetType().Name + ": " + exception.Message + ">";
        }

        /// <summary>Text recorded for a donor comparison that could not be read.</summary>
        internal static string Unavailable(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException("exception");
            return "<unavailable " + exception.GetType().Name + ": " + exception.Message + ">";
        }

        /// <summary>Marker appended when a calculation denies the None wildcard.</summary>
        internal const string WildcardDenied = "!None";

        /// <summary>
        /// The sub-categories a calculation grants, as comparable text: the
        /// intended values it grants in their policy order, then every other
        /// value it grants in the order given (all supported values), and
        /// <see cref="WildcardDenied"/> when it denies the <paramref name="none"/>
        /// wildcard that every category grants. For an intended policy this is
        /// exactly the intended list, so any missing, extra or wildcard
        /// difference makes the texts differ.
        /// </summary>
        internal static string SubCategoryText<T>(IList<T> intended,
            IEnumerable<T> supported, T none, Func<T, bool> grants)
        {
            if (intended == null) throw new ArgumentNullException("intended");
            if (supported == null) throw new ArgumentNullException("supported");
            if (grants == null) throw new ArgumentNullException("grants");
            EqualityComparer<T> equality = EqualityComparer<T>.Default;
            var granted = new List<string>();
            foreach (T value in intended)
                if (grants(value)) granted.Add(value.ToString());
            foreach (T value in supported.Distinct())
            {
                if (equality.Equals(value, none) || intended.Contains(value)) continue;
                if (grants(value)) granted.Add(value.ToString());
            }
            if (!grants(none)) granted.Add(WildcardDenied);
            return string.Join(",", granted.ToArray());
        }

        /// <summary>
        /// The owned serialized field and the owned getter are the invariants.
        /// Donor values are comparison context only and never decide a verdict.
        /// </summary>
        internal static SpearProfileFindingKind? Classify(
            SpearProfileObservation observation)
        {
            if (observation == null) throw new ArgumentNullException("observation");
            if (!observation.OwnedFieldMatches)
                return SpearProfileFindingKind.OwnedFieldMismatch;
            if (observation.EngineMatches) return null;
            return SpearProfileFindingKind.EffectiveGetterMismatch;
        }

        internal static SpearProfileVerdict Evaluate(
            IEnumerable<SpearProfileObservation> observations)
        {
            if (observations == null) throw new ArgumentNullException("observations");
            SpearProfileObservation[] values = observations.ToArray();
            if (values.Length == 0 || values.Any(value => value == null))
                throw new ArgumentException("Spear profile observations are incomplete.",
                    "observations");
            if (values.Select(value => value.Predicate).Distinct(StringComparer.Ordinal)
                    .Count() != values.Length)
                throw new ArgumentException("Spear profile predicates are not unique.",
                    "observations");
            var findings = new List<SpearProfileFinding>();
            foreach (SpearProfileObservation value in values)
            {
                SpearProfileFindingKind? kind = Classify(value);
                if (kind.HasValue) findings.Add(new SpearProfileFinding(kind.Value, value));
            }
            return new SpearProfileVerdict(findings, values.Length);
        }

        internal static string Describe(IEnumerable<SpearProfileFinding> findings)
        {
            SpearProfileFinding[] values = (findings ??
                Enumerable.Empty<SpearProfileFinding>()).ToArray();
            var text = new StringBuilder();
            int described = Math.Min(values.Length, MaxDescribedFindings);
            for (int index = 0; index < described; index++)
            {
                SpearProfileObservation value = values[index].Observation;
                if (index > 0) text.Append(" | ");
                text.Append(value.Predicate).Append(" [").Append(Kind(values[index].Kind))
                    .Append("]: expected=").Append(Bound(value.ExpectedField))
                    .Append(" actual=").Append(Bound(value.OwnedField))
                    .Append(" donor=").Append(Bound(value.DonorField))
                    .Append("; getter expected=").Append(Bound(value.EngineExpected))
                    .Append(" actual=").Append(Bound(value.EngineActual))
                    .Append(" donor=").Append(Bound(value.DonorEngine))
                    .Append("; getterPatches=").Append(Owners(value));
            }
            if (values.Length > described)
                text.Append(" | (+").Append((values.Length - described)
                    .ToString(CultureInfo.InvariantCulture)).Append(" more)");
            return text.ToString();
        }

        private static string Kind(SpearProfileFindingKind kind)
        {
            return kind == SpearProfileFindingKind.OwnedFieldMismatch
                ? "owned-field" : "effective-getter";
        }

        private static string Owners(SpearProfileObservation value)
        {
            if (!string.Equals(value.PatchInfo, "read", StringComparison.Ordinal))
                return value.PatchInfo;
            if (value.GetterPatchOwners.Length == 0) return "none";
            string[] owners = value.GetterPatchOwners.Take(MaxDescribedOwners)
                .Select(Bound).ToArray();
            string joined = string.Join(",", owners);
            return value.GetterPatchOwners.Length > owners.Length
                ? joined + ",+" + (value.GetterPatchOwners.Length - owners.Length)
                    .ToString(CultureInfo.InvariantCulture)
                : joined;
        }

        private static string Bound(string value)
        {
            if (value == null) return "<null>";
            return value.Length <= MaxDescribedValueLength ? value :
                value.Substring(0, MaxDescribedValueLength) + "...";
        }
    }
}
