using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// DEF-05 second review, engine-free judges for the representative native
    /// casts and the alternate child census. Each returns the list of failures
    /// (empty when the contract holds). The runtime observers feed them what
    /// they read from the engine; the domain tests feed them mutations.
    /// </summary>
    internal static class InheritedSourceLedger
    {
        /// <summary>
        /// Correction 1. events: every native spend or restore on the caster's
        /// own sources during the cast window, as "kind|key|amount" with kind one
        /// of spend-spell, restore-spontaneous, spend-resource, restore-resource.
        /// before/after: every source counter ("spontaneous|book|level",
        /// "slot|book|index", "resource|guid") and identity ("identity|..." as
        /// a value hash). expectedKey: the one counter that must fall, by
        /// expectedAmount, through exactly one spend event; null for "nothing".
        /// </summary>
        internal static IList<string> Judge(IList<string> events, IDictionary<string, int> before,
            IDictionary<string, int> after, string expectedKey, int expectedAmount, string expectedEvent)
        {
            var failures = new List<string>();
            if (events == null || before == null || after == null) { failures.Add("missing-observation"); return failures; }
            foreach (string key in before.Keys.Union(after.Keys).OrderBy(value => value, StringComparer.Ordinal))
            {
                int was, now;
                bool hadBefore = before.TryGetValue(key, out was), hasAfter = after.TryGetValue(key, out now);
                if (!hadBefore || !hasAfter) { failures.Add("source-identity-changed:" + key); continue; }
                int expectedDelta = key == expectedKey ? expectedAmount : 0;
                if (was - now != expectedDelta) failures.Add("source-changed:" + key + ":" + was + "->" + now);
            }
            if (expectedKey == null)
            {
                foreach (string value in events) failures.Add("unexpected-event:" + value);
                return failures;
            }
            if (expectedAmount < 1) failures.Add("non-positive-expected-debit:" + expectedAmount);
            if (!before.ContainsKey(expectedKey) || before[expectedKey] < expectedAmount)
                failures.Add("expected-source-unavailable:" + expectedKey);
            int matching = events.Count(value => value == expectedEvent);
            if (matching != 1) failures.Add("expected-spend-events:" + matching);
            foreach (string value in events.Where(value => value != expectedEvent)) failures.Add("unexpected-event:" + value);
            return failures;
        }
    }

    /// <summary>One spawn action's count expression and unit, as read by SummonSignatureReader.</summary>
    internal sealed class InheritedSpawnExpression
    {
        internal InheritedSpawnExpression(string unit, string dice, string countKind, int countValue,
            string bonusKind, int bonusValue, string bonusRank)
        {
            Unit = unit; Dice = dice; CountKind = countKind; CountValue = countValue;
            BonusKind = bonusKind; BonusValue = bonusValue; BonusRank = bonusRank;
        }

        internal string Unit { get; private set; }
        internal string Dice { get; private set; }
        internal string CountKind { get; private set; }
        internal int CountValue { get; private set; }
        internal string BonusKind { get; private set; }
        internal int BonusValue { get; private set; }
        internal string BonusRank { get; private set; }
    }

    internal static class InheritedQuantityContract
    {
        /// <summary>
        /// Correction 2. The native forms (memory of the 2.1.7b library, and
        /// the retained player paths): a single row is Zero dice with a Simple
        /// bonus of 1; a quantity row is one D3 or one D4 (Simple count 1) plus
        /// the Rank bonus ProjectilesCount (where Superior Summoning applies),
        /// whose value for this caster is evaluated natively before the cast:
        /// 0 for d3 and 1 for d4+1. Returns the expression's inclusive range.
        /// </summary>
        internal static IList<string> Expression(SummonMultiplicity multiplicity, string expectedUnit,
            IList<InheritedSpawnExpression> spawns, int expectedBranches, int evaluatedRankBonus,
            out int minimum, out int maximum)
        {
            var failures = new List<string>();
            minimum = 0; maximum = 0;
            if (spawns == null || spawns.Count == 0) { failures.Add("no-spawn-branch"); return failures; }
            if (spawns.Count != expectedBranches)
                failures.Add("spawn-branches:" + spawns.Count + "/" + expectedBranches);
            for (int index = 0; index < spawns.Count; index++)
            {
                InheritedSpawnExpression spawn = spawns[index];
                string where = "branch" + index + ":";
                if (!string.Equals(spawn.Unit, expectedUnit, StringComparison.Ordinal))
                    failures.Add(where + "wrong-unit:" + spawn.Unit);
                if (multiplicity == SummonMultiplicity.One)
                {
                    if (spawn.Dice != "Zero" || spawn.BonusKind != "Simple" || spawn.BonusValue != 1)
                        failures.Add(where + "not-one:" + spawn.Dice + "+" + spawn.BonusKind + spawn.BonusValue);
                    continue;
                }
                string die = multiplicity == SummonMultiplicity.OneD3 ? "D3" : "D4";
                int bonus = multiplicity == SummonMultiplicity.OneD3 ? 0 : 1;
                if (spawn.Dice != die || spawn.CountKind != "Simple" || spawn.CountValue != 1)
                    failures.Add(where + "wrong-die:" + spawn.CountKind + spawn.CountValue + spawn.Dice + "/1" + die);
                if (spawn.BonusKind != "Rank" || spawn.BonusRank != "ProjectilesCount")
                    failures.Add(where + "wrong-bonus-form:" + spawn.BonusKind + ":" + spawn.BonusRank);
                if (evaluatedRankBonus != bonus)
                    failures.Add(where + "wrong-bonus-value:" + evaluatedRankBonus + "/" + bonus);
            }
            if (multiplicity == SummonMultiplicity.One) { minimum = 1; maximum = 1; }
            else if (multiplicity == SummonMultiplicity.OneD3) { minimum = 1 + evaluatedRankBonus; maximum = 3 + evaluatedRankBonus; }
            else { minimum = 1 + evaluatedRankBonus; maximum = 4 + evaluatedRankBonus; }
            return failures;
        }

        /// <summary>The actual output: within the expression's range, distinct live units of the expected blueprint, nothing foreign.</summary>
        internal static IList<string> Output(int minimum, int maximum, string expectedUnit,
            IList<string> capturedUnits, IList<string> capturedIds, IList<bool> live)
        {
            var failures = new List<string>();
            int count = capturedUnits == null ? 0 : capturedUnits.Count;
            if (count < minimum || count > maximum) failures.Add("quantity:" + count + " outside " + minimum + ".." + maximum);
            if (capturedIds != null && capturedIds.Distinct(StringComparer.Ordinal).Count() != capturedIds.Count)
                failures.Add("duplicate-spawn-event");
            for (int index = 0; index < count; index++)
            {
                if (!string.Equals(capturedUnits[index], expectedUnit, StringComparison.Ordinal))
                    failures.Add("foreign-unit:" + capturedUnits[index]);
                if (live == null || index >= live.Count || !live[index]) failures.Add("not-live:" + index);
            }
            return failures;
        }
    }

    internal static class InheritedDurationContract
    {
        /// <summary>
        /// Correction 3. observations: (unit id, base seconds, bonus seconds)
        /// from the summon rule, one per spawned unit. An empty expectation
        /// (cancellation) requires no observation.
        /// </summary>
        internal static IList<string> Judge(IList<string> spawnedUnitIds,
            IList<Tuple<string, double, double>> observations, double expectedBase, double expectedBonus)
        {
            var failures = new List<string>();
            observations = observations ?? new List<Tuple<string, double, double>>();
            spawnedUnitIds = spawnedUnitIds ?? new List<string>();
            if (observations.Count != spawnedUnitIds.Count)
                failures.Add("duration-observations:" + observations.Count + "/" + spawnedUnitIds.Count);
            foreach (string unit in spawnedUnitIds)
            {
                Tuple<string, double, double>[] mine = observations.Where(value => value.Item1 == unit).ToArray();
                if (mine.Length != 1) { failures.Add("duration-for-unit:" + unit + ":" + mine.Length); continue; }
                if (Math.Abs(mine[0].Item2 - expectedBase) > 0.05 || Math.Abs(mine[0].Item3 - expectedBonus) > 0.05)
                    failures.Add("duration:" + unit + ":" + mine[0].Item2 + "+" + mine[0].Item3 + "/" + expectedBase + "+" + expectedBonus);
            }
            foreach (Tuple<string, double, double> value in observations.Where(value => !spawnedUnitIds.Contains(value.Item1)))
                failures.Add("duration-for-other-unit:" + value.Item1);
            return failures;
        }
    }

    internal static class InheritedBuffSetContract
    {
        /// <summary>
        /// Correction 4. expectation: "none", "celestial" or "fiendish".
        /// buffs: every buff identity on the unit, with repeats.
        /// projectTemplates and the two smite sets: every registered KMG
        /// summon template and celestial/fiendish smite identity.
        /// </summary>
        internal static IList<string> Judge(string expectation, IList<string> buffs, ICollection<string> nativeIds,
            ICollection<string> projectTemplates, ICollection<string> celestialSmites, ICollection<string> fiendishSmites,
            string expectedTemplate, string expectedSmite)
        {
            var failures = new List<string>();
            buffs = buffs ?? new List<string>();
            string[] native = buffs.Where(nativeIds.Contains).ToArray();
            string[] templates = buffs.Where(projectTemplates.Contains).ToArray();
            string[] celestial = buffs.Where(celestialSmites.Contains).ToArray();
            string[] fiendish = buffs.Where(fiendishSmites.Contains).ToArray();
            if (native.Length != 0) failures.Add("native-template:" + string.Join(",", native));
            if (expectation == "none")
            {
                if (templates.Length != 0) failures.Add("project-template:" + string.Join(",", templates));
                if (celestial.Length + fiendish.Length != 0) failures.Add("project-smite:" + string.Join(",", celestial.Concat(fiendish).ToArray()));
                return failures;
            }
            string[] ownSmites = expectation == "celestial" ? celestial : fiendish;
            string[] otherSmites = expectation == "celestial" ? fiendish : celestial;
            if (string.IsNullOrEmpty(expectedTemplate) || !templates.SequenceEqual(new[] { expectedTemplate }))
                failures.Add("template-set:" + string.Join(",", templates) + "/" + expectedTemplate);
            if (string.IsNullOrEmpty(expectedSmite) || !ownSmites.SequenceEqual(new[] { expectedSmite }))
                failures.Add("smite-set:" + string.Join(",", ownSmites) + "/" + expectedSmite);
            if (otherSmites.Length != 0) failures.Add("opposite-smite:" + string.Join(",", otherSmites));
            return failures;
        }
    }

    /// <summary>The identities one caster-chosen project template action wires, as read from a child graph.</summary>
    internal sealed class InheritedDynamicTemplateView
    {
        internal InheritedDynamicTemplateView(string celestialTemplate, string fiendishTemplate, string celestialSmite,
            string fiendishSmite, string[] replacedNative)
        {
            CelestialTemplate = celestialTemplate; FiendishTemplate = fiendishTemplate;
            CelestialSmite = celestialSmite; FiendishSmite = fiendishSmite;
            ReplacedNative = replacedNative ?? new string[0];
        }

        internal string CelestialTemplate { get; private set; }
        internal string FiendishTemplate { get; private set; }
        internal string CelestialSmite { get; private set; }
        internal string FiendishSmite { get; private set; }
        internal string[] ReplacedNative { get; private set; }
    }

    /// <summary>
    /// DEF-05 third review: the deliberate project template expected for a
    /// templated control, derived independently of the child graph under test.
    /// The band is restated from the established unit-HD contract (below 5 Low,
    /// 5 to 10 Mid, above 10 High; SummonTemplateBandPolicy agrees, checked by a
    /// domain test) from the unit's own class levels, and the identities are the
    /// stable registered symbols KMG.Summoning.Template.{Celestial,Fiendish}.Band
    /// and KMG.Summoning.Smite.{Celestial,Fiendish}.Available. The graph check
    /// and the live buff-set check (InheritedBuffSetContract, fed these expected
    /// identities) fail independently.
    /// </summary>
    internal static class InheritedDeliberateTemplateContract
    {
        internal static string Band(int hitDice)
        {
            if (hitDice < 0) throw new ArgumentOutOfRangeException("hitDice");
            return hitDice < 5 ? "Low" : hitDice <= 10 ? "Mid" : "High";
        }

        internal static IList<string> Graph(IList<InheritedDynamicTemplateView> actions, string celestialTemplate,
            string fiendishTemplate, string celestialSmite, string fiendishSmite, ICollection<string> nativeIds)
        {
            var failures = new List<string>();
            if (actions == null || actions.Count != 1)
            {
                failures.Add("dynamic-template-actions:" + (actions == null ? 0 : actions.Count));
                if (actions == null || actions.Count == 0) return failures;
            }
            InheritedDynamicTemplateView action = actions[0];
            Action<string, string, string> same = (name, actual, expected) =>
            {
                if (string.IsNullOrEmpty(expected) || !string.Equals(actual, expected, StringComparison.Ordinal))
                    failures.Add(name + ":" + (actual ?? "<null>") + "/" + (expected ?? "<unresolved>"));
            };
            same("celestial-template", action.CelestialTemplate, celestialTemplate);
            same("fiendish-template", action.FiendishTemplate, fiendishTemplate);
            same("celestial-smite", action.CelestialSmite, celestialSmite);
            same("fiendish-smite", action.FiendishSmite, fiendishSmite);
            if (action.ReplacedNative.Length != nativeIds.Count || action.ReplacedNative.Any(id => id == null || !nativeIds.Contains(id)) ||
                action.ReplacedNative.Distinct(StringComparer.Ordinal).Count() != action.ReplacedNative.Length)
                failures.Add("replaced-native:" + string.Join(",", action.ReplacedNative.Select(id => id ?? "<null>").ToArray()));
            return failures;
        }
    }

    /// <summary>Policy-dependent live-menu exposure of the preserved native tier-one child.</summary>
    internal enum InheritedPreservedExposure
    {
        Keep = 1,
        WithholdCompleteReplacement = 2,
        KeepIncompleteReplacement = 3,
        Invalid = 4
    }

    /// <summary>One replacement row the source catalogs require for a preserved native option.</summary>
    internal sealed class InheritedRequiredReplacement
    {
        internal InheritedRequiredReplacement(string kind, string symbol, string guid, int published)
        { Kind = kind; Symbol = symbol; Guid = guid; Published = published; }

        /// <summary>"generated" or "native-expansion".</summary>
        internal string Kind { get; private set; }
        internal string Symbol { get; private set; }
        /// <summary>The registered identity of the row, or null when unregistered or not published.</summary>
        internal string Guid { get; private set; }
        /// <summary>How many published catalog rows represent it.</summary>
        internal int Published { get; private set; }
    }

    internal sealed class InheritedReplacementExpectation
    {
        internal InheritedPreservedExposure Disposition;
        internal string Reason;
        internal IList<InheritedRequiredReplacement> Required = new List<InheritedRequiredReplacement>();
        internal IList<string> Findings = new List<string>();
    }

    /// <summary>
    /// DEF-05 owner decision (condition 3 revised): whether the publication keeps or withholds the preserved
    /// native child, derived independently from the source catalogs. It never reads the publication record,
    /// the live menu or the production coverage method. Ambiguity is Invalid, never incomplete coverage.
    /// </summary>
    internal static class InheritedTierOneReplacementContract
    {
        internal static InheritedReplacementExpectation Derive(SummonNativeOptionSpec original,
            SummonFamily family, int tier, string preservedGuid,
            IEnumerable<SummonVariantSpec> publishedGenerated,
            IEnumerable<SummonNativeExpansionSpec> publishedNative,
            IEnumerable<SummonNativeExpansionSpec> nativeCatalog,
            Func<SummonVariantSpec, string> generatedSymbol,
            IDictionary<string, string> registeredGuidBySymbol)
        {
            if (publishedGenerated == null || publishedNative == null || nativeCatalog == null ||
                generatedSymbol == null || registeredGuidBySymbol == null) throw new ArgumentNullException();
            var result = new InheritedReplacementExpectation();
            IList<string> findings = result.Findings;
            if (original == null)
            {
                findings.Add("native-option-missing");
                return Finish(result);
            }
            if (original.Family != family) findings.Add("option-wrong-family:" + original.Family);
            if (original.Tier != tier) findings.Add("option-wrong-tier:" + original.Tier);
            if (!string.Equals(original.Guid, preservedGuid, StringComparison.Ordinal))
                findings.Add("option-wrong-source:" + original.Guid + "/" + preservedGuid);
            SummonVariantSpec[] generated = publishedGenerated.ToArray();
            SummonNativeExpansionSpec[] native = publishedNative.ToArray();
            SummonNativeExpansionSpec[] catalog = nativeCatalog.ToArray();
            // The inputs are this parent's own published rows: anything else is malformed input.
            foreach (SummonVariantSpec value in generated)
            {
                if (value.Family != family) findings.Add("generated-wrong-family:" + value.StableKey);
                if (value.ParentTier != tier) findings.Add("generated-wrong-tier:" + value.StableKey);
            }
            foreach (string symbol in generated.Select(generatedSymbol).GroupBy(value => value, StringComparer.Ordinal)
                .Where(group => group.Count() > 1).Select(group => group.Key))
                findings.Add("generated-duplicate-row:" + symbol);
            foreach (SummonNativeExpansionSpec value in native)
            {
                if (value.Family != family) findings.Add("native-wrong-family:" + value.Symbol);
                if (value.Tier != tier) findings.Add("native-wrong-tier:" + value.Symbol);
            }
            foreach (string symbol in catalog.Select(value => value.Symbol).GroupBy(value => value, StringComparer.Ordinal)
                .Where(group => group.Count() > 1).Select(group => group.Key))
                findings.Add("native-catalog-duplicate:" + symbol);
            // 1. The generated replacement: exactly one published placement of the exact equivalent creature.
            if (original.EquivalentCreatureKey != null)
            {
                SummonVariantSpec[] matches = generated.Where(value => value.Family == original.Family &&
                    value.ParentTier == original.Tier && value.Multiplicity == original.Multiplicity &&
                    value.Creature.Key == original.EquivalentCreatureKey).ToArray();
                if (matches.Length > 1)
                    findings.Add("generated-duplicate-replacement:" + original.EquivalentCreatureKey + "." + original.Multiplicity);
                string symbol = matches.Length == 1 ? generatedSymbol(matches[0]) :
                    "generated:" + original.EquivalentCreatureKey + "." + original.Multiplicity;
                result.Required.Add(new InheritedRequiredReplacement("generated", symbol,
                    matches.Length == 1 ? Registered(registeredGuidBySymbol, symbol) : null, matches.Length));
            }
            // 2. Native-expansion replacements: sourced from this exact option, not unit-replacing, each once.
            SummonNativeExpansionSpec[] requiredNative = catalog.Where(value => value.Family == original.Family &&
                value.Tier == original.Tier && string.Equals(value.SourceAbilityGuid, original.Guid, StringComparison.Ordinal) &&
                !value.ReplacesSpawnUnit).ToArray();
            foreach (SummonNativeExpansionSpec expected in requiredNative)
            {
                SummonNativeExpansionSpec[] matches = native.Where(value => value.Symbol == expected.Symbol).ToArray();
                if (matches.Length > 1) findings.Add("native-duplicate-replacement:" + expected.Symbol);
                foreach (SummonNativeExpansionSpec match in matches)
                {
                    if (!string.Equals(match.SourceAbilityGuid, original.Guid, StringComparison.Ordinal))
                        findings.Add("native-wrong-source:" + match.Symbol + ":" + match.SourceAbilityGuid);
                    if (match.ReplacesSpawnUnit) findings.Add("native-replaces-spawn-unit:" + match.Symbol);
                    if (match.Multiplicity != expected.Multiplicity) findings.Add("native-wrong-quantity:" + match.Symbol);
                }
                result.Required.Add(new InheritedRequiredReplacement("native-expansion", expected.Symbol,
                    matches.Length == 1 ? Registered(registeredGuidBySymbol, expected.Symbol) : null, matches.Length));
            }
            // A published row claiming this source that the catalog does not list as required is ambiguous.
            foreach (SummonNativeExpansionSpec value in native.Where(value => value.Family == original.Family &&
                value.Tier == original.Tier && string.Equals(value.SourceAbilityGuid, original.Guid, StringComparison.Ordinal) &&
                !value.ReplacesSpawnUnit && !requiredNative.Any(expected => expected.Symbol == value.Symbol)))
                findings.Add("native-uncatalogued-source-row:" + value.Symbol);
            // 3. Registered identities of the represented rows must be exact and distinct.
            foreach (InheritedRequiredReplacement row in result.Required.Where(row => row.Published == 1))
            {
                if (row.Guid == null) findings.Add("replacement-unregistered:" + row.Symbol);
                else if (string.Equals(row.Guid, preservedGuid, StringComparison.Ordinal))
                    findings.Add("replacement-is-preserved:" + row.Symbol);
            }
            foreach (string guid in result.Required.Where(row => row.Guid != null).GroupBy(row => row.Guid, StringComparer.Ordinal)
                .Where(group => group.Count() > 1).Select(group => group.Key))
                findings.Add("replacement-guid-duplicate:" + guid);
            return Finish(result);
        }

        private static string Registered(IDictionary<string, string> registry, string symbol)
        {
            string guid;
            return registry.TryGetValue(symbol, out guid) && !string.IsNullOrEmpty(guid) ? guid : null;
        }

        private static InheritedReplacementExpectation Finish(InheritedReplacementExpectation result)
        {
            if (result.Findings.Count > 0)
            {
                result.Disposition = InheritedPreservedExposure.Invalid;
                result.Reason = "invalid:" + string.Join(",", result.Findings.ToArray());
            }
            else if (result.Required.Count == 0)
            {
                result.Disposition = InheritedPreservedExposure.Keep;
                result.Reason = "no-required-replacement";
            }
            else if (result.Required.All(row => row.Published == 1))
            {
                result.Disposition = InheritedPreservedExposure.WithholdCompleteReplacement;
                result.Reason = "complete-replacement:" + string.Join(",", result.Required.Select(row => row.Symbol).ToArray());
            }
            else
            {
                result.Disposition = InheritedPreservedExposure.KeepIncompleteReplacement;
                result.Reason = "incomplete-replacement:missing=" + string.Join(",", result.Required
                    .Where(row => row.Published == 0).Select(row => row.Symbol).ToArray());
            }
            return result;
        }
    }

    /// <summary>One live menu row: the exact object and its asset identity.</summary>
    internal sealed class InheritedMenuRow
    {
        internal InheritedMenuRow(object reference, string guid) { Reference = reference; Guid = guid ?? "<null>"; }
        internal object Reference { get; private set; }
        internal string Guid { get; private set; }
    }

    /// <summary>One current component of a donor carrier, as seen by the census.</summary>
    internal sealed class InheritedComponentView
    {
        internal InheritedComponentView(object reference, string type, string name)
        { Reference = reference; Type = type ?? ""; Name = name ?? ""; }

        internal object Reference { get; private set; }
        internal string Type { get; private set; }
        internal string Name { get; private set; }
    }

    /// <summary>Everything the census reads about one recorded donor carrier, once per carrier.</summary>
    internal sealed class InheritedTierOneObservation
    {
        internal string ParentGuid;
        internal object ParentReference;
        /// <summary>Differences between the pre-clone receipt and the donor now (all components).</summary>
        internal IList<string> ReceiptDifferences = new List<string>();
        /// <summary>The receipt's component references, in order.</summary>
        internal IList<object> OriginalComponents = new List<object>();
        internal IList<InheritedComponentView> CurrentComponents = new List<InheritedComponentView>();
        /// <summary>Receipt differences when the one added menu component is excluded from the current donor.</summary>
        internal IList<string> DifferencesExcludingMenu = new List<string>();
        /// <summary>The added menu component's rows, in order: exact objects and identities.</summary>
        internal IList<InheritedMenuRow> MenuChildren = new List<InheritedMenuRow>();
        internal string PreservedSymbol;
        internal string PreservedGuid;
        /// <summary>The exact registered preserved object.</summary>
        internal object PreservedReference;
        internal bool PreservedIsAbility;
        /// <summary>The independently derived keep/withhold expectation for the preserved child.</summary>
        internal InheritedReplacementExpectation Expectation;
        /// <summary>The exact registered object of each required replacement row, by symbol.</summary>
        internal IDictionary<string, object> RequiredReplacementReferences = new Dictionary<string, object>(StringComparer.Ordinal);
        /// <summary>Every registered KMG Expanded Summoning ability identity (canonical, native, preserved, adapter).</summary>
        internal ICollection<string> OwnedIdentityGuids = new HashSet<string>(StringComparer.Ordinal);
        internal string DonorSignature;
        internal string PreservedSignature;
        internal IList<string> DonorActionTypes = new List<string>();
        internal IList<string> PreservedActionTypes = new List<string>();
        internal IList<string> DonorApplyBuffIds = new List<string>();
        internal IList<string> PreservedApplyBuffIds = new List<string>();
        internal bool PreservedSharesInstances;
        internal IList<string> ExpectedOwnedChildren = new List<string>();
        /// <summary>Null when the publisher's own record agrees with the receipt and the live parent; otherwise why not.</summary>
        internal string RecordDisagreement;
    }

    /// <summary>
    /// DEF-05 owner decision: the one approved donor change. The established
    /// tier-one menu publication (47c6a4ab04) turns the two native direct
    /// tier-one parents into menus after the canonical spells are cloned from
    /// them. Every other donor must match its receipt exactly. For the two exact
    /// parents a difference is an approved transition only when every check
    /// below holds; anything else stays a donor mutation.
    /// </summary>
    internal static class InheritedTierOnePublicationContract
    {
        internal const string SummonMonsterI = "8fd74eddd9b6c224693d9ab241f25e84";
        internal const string SummonNaturesAllyI = "c6147854641924442a3bb736080cfeb6";
        internal const string MenuComponentName = "$KMG_ExpandedSummoning_Variants";
        internal const string MenuComponentType = "Kingmaker.UnitLogic.Abilities.Components.AbilityVariants";

        /// <summary>"preserved", "expected-owned", "unexpected-owned" or "foreign".</summary>
        internal static string Classify(InheritedTierOneObservation value, InheritedMenuRow row)
        {
            if (row.Guid == value.PreservedGuid || ReferenceEquals(row.Reference, value.PreservedReference)) return "preserved";
            if (value.ExpectedOwnedChildren.Contains(row.Guid)) return "expected-owned";
            return value.OwnedIdentityGuids.Contains(row.Guid) ? "unexpected-owned" : "foreign";
        }

        /// <summary>"unchanged", "approved-tier-one-publication" or "mutated"; failures say why it is not approved.</summary>
        internal static string Judge(InheritedTierOneObservation value, IList<string> failures)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (value.ReceiptDifferences.Count == 0) return "unchanged";
            if (value.ParentGuid != SummonMonsterI && value.ParentGuid != SummonNaturesAllyI)
            {
                foreach (string difference in value.ReceiptDifferences) failures.Add("donor-mutated:" + difference);
                return "mutated";
            }
            int before = failures.Count;
            // 1 and 2: the original components kept in order, plus exactly one owned menu component.
            InheritedComponentView[] added = value.CurrentComponents
                .Where(component => !value.OriginalComponents.Any(original => ReferenceEquals(original, component.Reference))).ToArray();
            object[] kept = value.CurrentComponents.Where(component => !added.Contains(component)).Select(component => component.Reference).ToArray();
            if (value.CurrentComponents.Count != value.OriginalComponents.Count + 1)
                failures.Add("component-count:" + value.CurrentComponents.Count + "/" + (value.OriginalComponents.Count + 1));
            if (kept.Length != value.OriginalComponents.Count ||
                kept.Where((reference, index) => !ReferenceEquals(reference, value.OriginalComponents[index])).Any())
                failures.Add("original-components-not-kept-in-order");
            if (added.Length != 1) failures.Add("added-components:" + added.Length);
            else
            {
                if (added[0].Type != MenuComponentType) failures.Add("added-component-type:" + added[0].Type);
                if (added[0].Name != MenuComponentName) failures.Add("added-component-name:" + added[0].Name);
            }
            int variants = value.CurrentComponents.Count(component => component.Type == MenuComponentType);
            if (variants != 1) failures.Add("variants-components:" + variants);
            // 3 (revised by the owner): the stable preserved child always resolves and is proved (4); its live-menu
            // exposure follows the independently derived replacement expectation.
            if (string.IsNullOrEmpty(value.PreservedGuid) || !value.PreservedIsAbility || value.PreservedReference == null)
                failures.Add("preserved-child-unresolved");
            else
            {
                if (value.PreservedGuid == value.ParentGuid || ReferenceEquals(value.PreservedReference, value.ParentReference))
                    failures.Add("preserved-child-is-the-parent");
                int byGuid = value.MenuChildren.Count(row => row.Guid == value.PreservedGuid);
                int byReference = value.MenuChildren.Count(row => ReferenceEquals(row.Reference, value.PreservedReference));
                InheritedReplacementExpectation expectation = value.Expectation;
                if (expectation == null) failures.Add("replacement-expectation-missing");
                else if (expectation.Disposition == InheritedPreservedExposure.Invalid)
                {
                    failures.Add("replacement-coverage-invalid");
                    foreach (string finding in expectation.Findings) failures.Add("replacement-coverage:" + finding);
                }
                else if (expectation.Disposition == InheritedPreservedExposure.WithholdCompleteReplacement)
                {
                    if (byGuid != 0 || byReference != 0)
                        failures.Add("preserved-child-withheld-but-in-menu:guid=" + byGuid + ",reference=" + byReference);
                    foreach (InheritedRequiredReplacement row in expectation.Required)
                    {
                        object reference;
                        value.RequiredReplacementReferences.TryGetValue(row.Symbol, out reference);
                        if (row.Guid == null || reference == null) { failures.Add("replacement-unresolved:" + row.Symbol); continue; }
                        int rowGuid = value.MenuChildren.Count(menuRow => menuRow.Guid == row.Guid);
                        int rowReference = value.MenuChildren.Count(menuRow => ReferenceEquals(menuRow.Reference, reference));
                        if (rowGuid != 1 || rowReference != 1)
                            failures.Add("replacement-in-menu:" + row.Symbol + ":guid=" + rowGuid + ",reference=" + rowReference);
                    }
                }
                else if (byGuid != 1 || byReference != 1)
                    failures.Add("preserved-child-in-menu:guid=" + byGuid + ",reference=" + byReference + ",expected=1");
            }
            // 4: the preserved child is the original direct cast, semantically, with no shared instances.
            if (string.IsNullOrEmpty(value.DonorSignature) || value.PreservedSignature != value.DonorSignature)
                failures.Add("preserved-signature-differs");
            if (!value.PreservedActionTypes.SequenceEqual(value.DonorActionTypes, StringComparer.Ordinal))
                failures.Add("preserved-action-types-differ");
            if (!value.PreservedApplyBuffIds.SequenceEqual(value.DonorApplyBuffIds, StringComparer.Ordinal))
                failures.Add("preserved-buff-identities-differ");
            if (value.PreservedSharesInstances) failures.Add("preserved-shares-instances");
            // 5: with the menu excluded, the live parent is still exactly the receipt.
            foreach (string difference in value.DifferencesExcludingMenu) failures.Add("parent-graph-changed:" + difference);
            // 6: every expected owned child exactly once, no other KMG-owned row; genuine foreign rows and order are free.
            foreach (string child in value.ExpectedOwnedChildren)
            {
                int count = value.MenuChildren.Count(row => row.Guid == child);
                if (count != 1) failures.Add("owned-child:" + child + ":" + count);
            }
            foreach (InheritedMenuRow row in value.MenuChildren)
                if (InheritedTierOnePublicationContract.Classify(value, row) == "unexpected-owned")
                    failures.Add("unexpected-owned-child:" + row.Guid);
            // 7: the publisher's own record, cross-checked independently.
            if (value.RecordDisagreement != null) failures.Add("publication-record:" + value.RecordDisagreement);
            return failures.Count == before ? "approved-tier-one-publication" : "mutated";
        }
    }

    internal static class InheritedChildSetContract
    {
        /// <summary>
        /// Correction 5. expected: projection symbol to (placement key, spell
        /// family). actual: builder creature-child symbol to (placement key or
        /// null, spell family or null).
        /// </summary>
        internal static IList<string> Compare(string family, SummonFamily spellFamily,
            IDictionary<string, Tuple<string, SummonFamily?>> expected,
            IDictionary<string, Tuple<string, SummonFamily?>> actual)
        {
            var failures = new List<string>();
            foreach (string symbol in expected.Keys.Where(value => !actual.ContainsKey(value)).OrderBy(value => value, StringComparer.Ordinal))
                failures.Add(family + "|" + symbol + ":missing-child");
            foreach (string symbol in actual.Keys.Where(value => !expected.ContainsKey(value)).OrderBy(value => value, StringComparer.Ordinal))
                failures.Add(family + "|" + symbol + ":extra-child");
            foreach (var entry in actual.OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                if (entry.Value == null || entry.Value.Item1 == null) failures.Add(family + "|" + entry.Key + ":null-placement");
                else if (entry.Value.Item2 != spellFamily) failures.Add(family + "|" + entry.Key + ":wrong-placement-family");
            }
            // A family may map two of its tiers to one placement; a duplicate is more actual children
            // for a placement than the projection expects.
            Dictionary<string, int> expectedPerPlacement = expected.Values.Where(value => value != null && value.Item1 != null)
                .GroupBy(value => value.Item1).ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
            foreach (var shared in actual.Where(entry => entry.Value != null && entry.Value.Item1 != null)
                .GroupBy(entry => entry.Value.Item1))
            {
                int allowed;
                if (!expectedPerPlacement.TryGetValue(shared.Key, out allowed)) allowed = 1;
                if (shared.Count() > allowed)
                    failures.Add(family + "|" + shared.Key + ":duplicate-placement:" + string.Join(",", shared.Select(entry => entry.Key).ToArray()));
            }
            return failures;
        }
    }
}
