using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Presentation
{
    /// <summary>One independently evaluated result of a presentation observer.</summary>
    internal sealed class AuditOutcome
    {
        internal AuditOutcome(string name, string expected, string observed,
            bool passed)
        {
            Name = name; Expected = expected; Observed = observed; Passed = passed;
        }

        internal string Name { get; private set; }
        internal string Expected { get; private set; }
        internal string Observed { get; private set; }
        internal bool Passed { get; private set; }
    }

    /// <summary>
    /// One identity on either side of a census. The key is what the two sides
    /// are matched on; the label is what a reader sees. An identity whose key
    /// could not be established carries the reason instead.
    /// </summary>
    internal sealed class AuditIdentity
    {
        internal AuditIdentity(string key, string label)
            : this(key, label, null) { }

        internal AuditIdentity(string key, string label, string fault)
        {
            Key = key; Label = label ?? "<unnamed>"; Fault = fault;
        }

        internal string Key { get; private set; }
        internal string Label { get; private set; }
        internal string Fault { get; private set; }
    }

    /// <summary>
    /// Expected identities against observed identities. The count and the
    /// membership are separate results: a wrong count does not stop the sets
    /// being compared, and a matching count does not excuse a replacement.
    /// </summary>
    internal sealed class IdentityCensus
    {
        private IdentityCensus() { }

        internal int ExpectedCount { get; private set; }
        internal int ObservedCount { get; private set; }
        /// <summary>Expected identities that were not observed.</summary>
        internal IList<string> Missing { get; private set; }
        /// <summary>Observed identities that were not expected.</summary>
        internal IList<string> Unexpected { get; private set; }
        /// <summary>Identities observed more than once, with how often.</summary>
        internal IList<string> DuplicateObserved { get; private set; }
        /// <summary>Identities declared more than once in the expectation.</summary>
        internal IList<string> DuplicateExpected { get; private set; }
        /// <summary>Expected identities whose key could not be established.</summary>
        internal IList<string> UnresolvedExpected { get; private set; }
        /// <summary>Observed entries that had no usable identity.</summary>
        internal IList<string> MalformedObserved { get; private set; }

        internal bool CountMatches { get { return ExpectedCount == ObservedCount; } }
        internal bool MembershipMatches
        {
            get
            {
                return Missing.Count == 0 && Unexpected.Count == 0 &&
                    DuplicateObserved.Count == 0 && DuplicateExpected.Count == 0 &&
                    UnresolvedExpected.Count == 0 && MalformedObserved.Count == 0;
            }
        }
        internal bool Exact { get { return CountMatches && MembershipMatches; } }

        internal static IdentityCensus Compare(IEnumerable<AuditIdentity> expected,
            IEnumerable<AuditIdentity> observed)
        {
            if (expected == null) throw new ArgumentNullException("expected");
            if (observed == null) throw new ArgumentNullException("observed");
            AuditIdentity[] wanted = expected.ToArray();
            AuditIdentity[] seen = observed.ToArray();
            var census = new IdentityCensus
            {
                ExpectedCount = wanted.Length, ObservedCount = seen.Length,
                Missing = new List<string>(), Unexpected = new List<string>(),
                DuplicateObserved = new List<string>(),
                DuplicateExpected = new List<string>(),
                UnresolvedExpected = new List<string>(),
                MalformedObserved = new List<string>()
            };
            var wantedByKey = new Dictionary<string, AuditIdentity>(StringComparer.Ordinal);
            foreach (AuditIdentity identity in wanted)
            {
                if (identity == null || string.IsNullOrEmpty(identity.Key))
                {
                    census.UnresolvedExpected.Add(identity == null ? "<null>" :
                        identity.Label + "(" + (identity.Fault ?? "no identity") + ")");
                    continue;
                }
                if (wantedByKey.ContainsKey(identity.Key))
                    census.DuplicateExpected.Add(identity.Label);
                else wantedByKey.Add(identity.Key, identity);
            }
            var seenCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var seenLabels = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (AuditIdentity identity in seen)
            {
                if (identity == null || string.IsNullOrEmpty(identity.Key))
                {
                    census.MalformedObserved.Add(identity == null ? "<null>" :
                        identity.Label + "(" + (identity.Fault ?? "no identity") + ")");
                    continue;
                }
                int count;
                seenCounts.TryGetValue(identity.Key, out count);
                seenCounts[identity.Key] = count + 1;
                if (count == 0) seenLabels.Add(identity.Key, identity.Label);
            }
            foreach (AuditIdentity identity in wanted)
                if (identity != null && !string.IsNullOrEmpty(identity.Key) &&
                    ReferenceEquals(wantedByKey[identity.Key], identity) &&
                    !seenCounts.ContainsKey(identity.Key))
                    census.Missing.Add(identity.Label);
            foreach (KeyValuePair<string, int> pair in seenCounts)
            {
                AuditIdentity match;
                bool known = wantedByKey.TryGetValue(pair.Key, out match);
                if (!known) census.Unexpected.Add(seenLabels[pair.Key]);
                if (pair.Value > 1)
                    census.DuplicateObserved.Add((known ? match.Label :
                        seenLabels[pair.Key]) + " x" + pair.Value);
            }
            return census;
        }

        internal string Describe()
        {
            return "expected=" + ExpectedCount + ";observed=" + ObservedCount +
                ";count=" + (CountMatches ? "match" : "MISMATCH") +
                ";membership=" + (MembershipMatches ? "match" : "MISMATCH") +
                List(";missing=", Missing) + List(";unexpected=", Unexpected) +
                List(";observedMoreThanOnce=", DuplicateObserved) +
                List(";declaredMoreThanOnce=", DuplicateExpected) +
                List(";unresolvedExpected=", UnresolvedExpected) +
                List(";malformedObserved=", MalformedObserved);
        }

        private static string List(string prefix, IList<string> values)
        {
            return values.Count == 0 ? string.Empty :
                prefix + string.Join("|", values.ToArray());
        }
    }

    /// <summary>
    /// What was read from one firearm item about its visual. The reader fills
    /// in what it could; anything it could not read stays at its default and,
    /// if reading raised, <see cref="Fault"/> says how.
    /// </summary>
    internal sealed class FirearmVisualBindingObservation
    {
        internal string Symbol;
        internal FirearmKind Kind;
        internal bool ItemPresent;
        internal string ItemName;
        internal bool PrefabAvailable;
        internal string PrefabName;
        internal bool HasVisualParameters;
        internal string ModelName;
        internal bool ModelIsVariantPrefab;
        internal string Fault;
    }

    /// <summary>The outcome of inspecting one firearm item's visual binding.</summary>
    internal sealed class FirearmVisualBindingResult
    {
        internal FirearmVisualBindingResult(string symbol, string variant,
            string failure)
        {
            Symbol = symbol; Variant = variant; Failure = failure;
        }

        internal string Symbol { get; private set; }
        /// <summary>The approved variant the catalog maps the symbol to, if any.</summary>
        internal string Variant { get; private set; }
        /// <summary>Null when the item is bound to its approved variant.</summary>
        internal string Failure { get; private set; }
        internal bool Bound { get { return Failure == null; } }

        internal string Describe()
        {
            return Symbol + "=" + (Variant ?? "<unmapped>") + ":" +
                (Bound ? "bound" : "FAIL(" + Failure + ")");
        }
    }

    /// <summary>
    /// The firearm item visual check, in two independent parts: which
    /// identities are present, and whether each one is bound to its approved
    /// variant. Every item is inspected whatever the census says and whatever
    /// happened to the item before it.
    /// </summary>
    internal sealed class FirearmVisualRigAudit
    {
        internal const string RosterName = "firearm-item-visual-roster";
        internal const string BindingsName = "firearm-item-visual-bindings";

        private FirearmVisualRigAudit() { }

        internal IdentityCensus Census { get; private set; }
        internal IList<FirearmVisualBindingResult> Bindings { get; private set; }
        internal AuditOutcome Roster { get; private set; }
        internal AuditOutcome BindingOutcome { get; private set; }
        internal bool Passed { get { return Roster.Passed && BindingOutcome.Passed; } }

        /// <summary>
        /// One item against the catalog's approved variant for its symbol.
        /// Never throws: an unmapped symbol, a mapping across the family
        /// boundary, a missing prefab or a faulted read is the result.
        /// </summary>
        internal static FirearmVisualBindingResult Inspect(
            FirearmVisualBindingObservation item)
        {
            if (item == null) return new FirearmVisualBindingResult("<null>", null,
                "no observation");
            string variant;
            if (string.IsNullOrEmpty(item.Symbol) ||
                !WeaponVisualVariantCatalog.TryGet(item.Symbol, out variant))
                return new FirearmVisualBindingResult(item.Symbol ?? "<no symbol>",
                    null, "no approved visual variant is mapped for this symbol");
            if (!variant.StartsWith(item.Kind + ".", StringComparison.Ordinal))
                return new FirearmVisualBindingResult(item.Symbol, variant,
                    "the mapped variant is outside the " + item.Kind + " family");
            if (item.Fault != null)
                return new FirearmVisualBindingResult(item.Symbol, variant,
                    "the item could not be read: " + item.Fault);
            if (!item.ItemPresent)
                return new FirearmVisualBindingResult(item.Symbol, variant,
                    "the game has no item for this symbol");
            if (!item.PrefabAvailable)
                return new FirearmVisualBindingResult(item.Symbol, variant,
                    "the variant's prefab is not loaded");
            if (!item.HasVisualParameters)
                return new FirearmVisualBindingResult(item.Symbol, variant,
                    "the item has no visual parameters");
            if (!item.ModelIsVariantPrefab)
                return new FirearmVisualBindingResult(item.Symbol, variant,
                    "the item's model is " + (item.ModelName ?? "<none>") +
                    ", not the variant prefab " + (item.PrefabName ?? "<unnamed>"));
            return new FirearmVisualBindingResult(item.Symbol, variant, null);
        }

        internal static FirearmVisualRigAudit Evaluate(
            IEnumerable<FirearmVisualRosterEntry> expected,
            IEnumerable<FirearmVisualBindingObservation> observed)
        {
            if (expected == null) throw new ArgumentNullException("expected");
            if (observed == null) throw new ArgumentNullException("observed");
            FirearmVisualRosterEntry[] roster = expected.ToArray();
            FirearmVisualBindingObservation[] items = observed.ToArray();
            IdentityCensus census = IdentityCensus.Compare(
                roster.Select(value => new AuditIdentity(value.Symbol, value.Symbol)),
                items.Select(value => value == null
                    ? new AuditIdentity(null, "<null>", "no observation")
                    : new AuditIdentity(value.Symbol, value.Symbol ??
                        value.ItemName ?? "<unreadable source>",
                        value.Fault ?? "no symbol")));

            // Every observed item is inspected. An expected identity that was
            // not observed is a binding failure of its own as well: nothing
            // was there to bind.
            var bindings = new List<FirearmVisualBindingResult>();
            var kindBySymbol = new Dictionary<string, FirearmKind>(StringComparer.Ordinal);
            foreach (FirearmVisualRosterEntry entry in roster)
                kindBySymbol[entry.Symbol] = entry.Kind;
            var inspected = new HashSet<string>(StringComparer.Ordinal);
            foreach (FirearmVisualBindingObservation item in items)
            {
                FirearmVisualBindingResult result = Inspect(item);
                FirearmKind declared;
                if (result.Bound && item != null &&
                    kindBySymbol.TryGetValue(item.Symbol, out declared) &&
                    declared != item.Kind)
                    result = new FirearmVisualBindingResult(item.Symbol,
                        result.Variant, "the game reports kind " + item.Kind +
                            ", the roster declares " + declared);
                bindings.Add(result);
                if (item != null && item.Symbol != null) inspected.Add(item.Symbol);
            }
            foreach (FirearmVisualRosterEntry entry in roster)
                if (!inspected.Contains(entry.Symbol))
                {
                    string variant;
                    WeaponVisualVariantCatalog.TryGet(entry.Symbol, out variant);
                    bindings.Add(new FirearmVisualBindingResult(entry.Symbol, variant,
                        "not observed, so nothing was bound"));
                }

            FirearmVisualBindingResult[] failed = bindings.Where(value =>
                !value.Bound).ToArray();
            return new FirearmVisualRigAudit
            {
                Census = census, Bindings = bindings,
                Roster = new AuditOutcome(RosterName,
                    "exactly the " + roster.Length +
                        " firearm items that carry their own visual variant: " +
                        string.Join("|", roster.Select(value => value.Symbol).ToArray()),
                    census.Describe(), census.Exact),
                BindingOutcome = new AuditOutcome(BindingsName,
                    "every firearm item's model is the prefab of the variant approved for its symbol, inside its own firearm family",
                    "inspected=" + bindings.Count + ";bound=" +
                        (bindings.Count - failed.Length) + ";failed=" + failed.Length +
                        (failed.Length == 0 ? string.Empty : ";failures=" +
                            string.Join("|", failed.Select(value =>
                                value.Describe()).ToArray())) +
                        ";all=" + string.Join("|", bindings.Select(value =>
                            value.Describe()).ToArray()),
                    failed.Length == 0 && bindings.Count != 0)
            };
        }
    }

    /// <summary>What was read about the policy-only weapon enchantment.</summary>
    internal sealed class PolicyEnchantmentObservation
    {
        /// <summary>The game has the policy enchantment blueprint at all.</summary>
        internal bool BlueprintAvailable;
        /// <summary>It is among the custom enchantments found on the weapons and their types.</summary>
        internal bool AmongWeaponEnchantments;
        internal string InternalName;
        internal string Name;
        internal string Description;
    }

    /// <summary>
    /// The project item presentation check as separate results: the weapon
    /// census, the item census, the custom enchantment census, the
    /// policy-only enchantment, and the player-facing text. None depends on
    /// another having passed.
    /// </summary>
    internal sealed class ProjectItemPresentationAudit
    {
        internal const string WeaponCensusName = "project-item-weapon-census";
        internal const string ItemCensusName = "project-item-census";
        internal const string EnchantmentCensusName =
            "project-custom-enchantment-census";
        internal const string PolicyName = "project-policy-enchantment";
        internal const string TextName = "project-item-player-facing-text";
        internal const string AggregateName =
            "project-item-player-facing-presentation";

        private ProjectItemPresentationAudit() { }

        internal IList<AuditOutcome> Outcomes { get; private set; }
        internal AuditOutcome Aggregate { get; private set; }
        internal bool Passed { get { return Aggregate.Passed; } }

        internal AuditOutcome Outcome(string name)
        {
            return Outcomes.Single(value => value.Name == name);
        }

        internal static ProjectItemPresentationAudit Evaluate(
            IdentityCensus weapons, IdentityCensus items,
            IdentityCensus enchantments, PolicyEnchantmentObservation policy,
            IEnumerable<string> textIssues)
        {
            if (weapons == null) throw new ArgumentNullException("weapons");
            if (items == null) throw new ArgumentNullException("items");
            if (enchantments == null) throw new ArgumentNullException("enchantments");
            if (textIssues == null) throw new ArgumentNullException("textIssues");
            string[] issues = textIssues.ToArray();
            var policyProblems = new List<string>();
            if (policy == null || !policy.BlueprintAvailable)
                policyProblems.Add("the game has no policy enchantment blueprint");
            else
            {
                if (!policy.AmongWeaponEnchantments)
                    policyProblems.Add("it is not among the custom enchantments on the weapons or their types");
                if (!string.IsNullOrEmpty(policy.Name))
                    policyProblems.Add("its name is visible: " + policy.Name);
                if (!string.IsNullOrEmpty(policy.Description))
                    policyProblems.Add("its description is visible: " + policy.Description);
            }
            var outcomes = new List<AuditOutcome>
            {
                new AuditOutcome(WeaponCensusName,
                    "exactly the " + weapons.ExpectedCount + " declared project weapons",
                    weapons.Describe(), weapons.Exact),
                new AuditOutcome(ItemCensusName,
                    "exactly the " + items.ExpectedCount +
                        " declared project items: the weapons and the Cord of Stubborn Resolve",
                    items.Describe(), items.Exact),
                new AuditOutcome(EnchantmentCensusName,
                    "exactly the " + enchantments.ExpectedCount +
                        " declared custom weapon enchantments on those weapons and their types",
                    enchantments.Describe(), enchantments.Exact),
                new AuditOutcome(PolicyName,
                    "the policy-only enchantment is among the weapons' enchantments and its name and description are blank",
                    (policy == null ? "<not read>" : (policy.InternalName ?? "<unnamed>") +
                        ";available=" + policy.BlueprintAvailable + ";amongWeaponEnchantments=" +
                        policy.AmongWeaponEnchantments + ";nameLength=" +
                        (policy.Name ?? string.Empty).Length + ";descriptionLength=" +
                        (policy.Description ?? string.Empty).Length) +
                        (policyProblems.Count == 0 ? string.Empty : ";problems=" +
                            string.Join("|", policyProblems.ToArray())),
                    policyProblems.Count == 0),
                new AuditOutcome(TextName,
                    "every observed item and every visible custom enchantment has complete clean player text",
                    "issues=" + (issues.Length == 0 ? "<none>" :
                        string.Join("|", issues)), issues.Length == 0)
            };
            string[] failed = outcomes.Where(value => !value.Passed)
                .Select(value => value.Name).ToArray();
            return new ProjectItemPresentationAudit
            {
                Outcomes = outcomes,
                Aggregate = new AuditOutcome(AggregateName,
                    "the weapon, item and custom enchantment censuses, the policy-only enchantment and the player text all hold",
                    "weapons=" + weapons.ObservedCount + "/" + weapons.ExpectedCount +
                        ";items=" + items.ObservedCount + "/" + items.ExpectedCount +
                        ";customEnchantments=" + enchantments.ObservedCount + "/" +
                        enchantments.ExpectedCount + ";issues=" +
                        (issues.Length == 0 ? "<none>" : issues.Length.ToString()) +
                        ";failed=" + (failed.Length == 0 ? "<none>" :
                            string.Join("|", failed)),
                    failed.Length == 0)
            };
        }
    }
}
