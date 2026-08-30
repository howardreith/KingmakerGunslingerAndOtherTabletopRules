using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    internal sealed class ProtectionFromAlignmentPublication
    {
        private readonly List<Mutation> _mutations;

        private ProtectionFromAlignmentPublication(List<Mutation> mutations,
            ProtectionFromAlignmentPublicationSummary summary)
        {
            _mutations = mutations;
            Summary = summary;
        }

        internal ProtectionFromAlignmentPublicationSummary Summary
        { get; private set; }

        internal static ProtectionFromAlignmentPublication Publish(
            LibraryScriptableObject library, ModLogger logger, bool enabled)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (logger == null) throw new ArgumentNullException("logger");
            MentalControlCatalog catalog = MentalControlCatalogDefaults.Create();
            var requiredIssues = new List<string>();
            var optionalIssues = new List<string>();
            var protections = new List<ResolvedProtection>();
            int optionalResolved = 0;
            int optionalCount = 0;

            foreach (ProtectionBuffSpec spec in ProtectionBuffs)
            {
                BlueprintScriptableObject resolved;
                string issue;
                if (TryResolveExact(library, spec.Guid, typeof(BlueprintBuff),
                    spec.Name, out resolved, out issue))
                    protections.Add(new ResolvedProtection(
                        (BlueprintBuff)resolved, spec));
                else
                    requiredIssues.Add(issue);
            }
            foreach (MentalControlCatalogEntry entry in catalog.Entries)
            {
                BlueprintScriptableObject resolved;
                string issue;
                Type expected = entry.Kind == MentalControlBlueprintKind.Ability ?
                    typeof(BlueprintAbility) : typeof(BlueprintBuff);
                bool found = TryResolveExact(library, entry.Guid, expected,
                    entry.BlueprintName, out resolved, out issue);
                if (entry.Required)
                {
                    if (!found) requiredIssues.Add(issue);
                }
                else
                {
                    optionalCount++;
                    if (found) optionalResolved++;
                    else optionalIssues.Add(issue);
                }
            }

            ProtectionFromAlignmentRuntime.Configure(catalog, logger);
            if (requiredIssues.Count > 0)
            {
                var failed = new ProtectionFromAlignmentPublicationSummary(enabled,
                    protections.Count, 0, 0, catalog.AbilityCount,
                    catalog.BuffCount, requiredIssues, optionalIssues,
                    optionalCount > 0 && optionalResolved == optionalCount);
                LogSummary(logger, failed);
                if (enabled)
                    throw new InvalidOperationException(
                        "Protection from Alignment required blueprint audit failed: " +
                        string.Join(" | ", requiredIssues.ToArray()));
                return new ProtectionFromAlignmentPublication(new List<Mutation>(),
                    failed);
            }

            var mutations = new List<Mutation>();
            int patched = 0;
            int alreadyPatched = 0;
            try
            {
                if (enabled)
                {
                    foreach (ResolvedProtection protection in protections)
                    {
                        BlueprintComponent[] original =
                            protection.Buff.ComponentsArray;
                        BlueprintComponent[] before = original ??
                            Array.Empty<BlueprintComponent>();
                        ProtectionFromAlignmentControlImmunityComponent[] owned =
                            before.OfType<
                                ProtectionFromAlignmentControlImmunityComponent>()
                                .ToArray();
                        int exact = owned.Count(value =>
                            value.ProtectedAgainstAlignment ==
                                protection.Spec.ProtectedAgainst);
                        if (owned.Length != exact)
                            throw new InvalidOperationException(
                                protection.Spec.Name +
                                " already has a protection-control component for " +
                                "an unexpected alignment.");
                        ProtectionComponentPublicationDecision decision =
                            ProtectionComponentPublicationPolicy.Decide(exact);
                        if (decision ==
                            ProtectionComponentPublicationDecision.AlreadyPatched)
                        {
                            alreadyPatched++;
                            Validate(protection);
                            continue;
                        }
                        var component = ScriptableObject.CreateInstance<
                            ProtectionFromAlignmentControlImmunityComponent>();
                        component.name =
                            "KMG_ProtectionFromAlignmentControlImmunity_" +
                            protection.Spec.ProtectedAgainst;
                        component.ProtectedAgainstAlignment =
                            protection.Spec.ProtectedAgainst;
                        BlueprintComponent[] published = before.Concat(
                            new BlueprintComponent[] { component }).ToArray();
                        protection.Buff.ComponentsArray = published;
                        mutations.Add(new Mutation(protection.Buff, original,
                            published));
                        patched++;
                        Validate(protection);
                    }
                }
                var summary = new ProtectionFromAlignmentPublicationSummary(enabled,
                    protections.Count, patched, alreadyPatched,
                    catalog.AbilityCount, catalog.BuffCount, requiredIssues,
                    optionalIssues,
                    optionalCount > 0 && optionalResolved == optionalCount);
                LogSummary(logger, summary);
                return new ProtectionFromAlignmentPublication(mutations, summary);
            }
            catch (Exception publicationException)
            {
                try { RollbackAll(mutations); }
                catch (Exception rollbackException)
                {
                    throw new AggregateException(
                        "Protection from Alignment publication failed and rollback was refused.",
                        publicationException, rollbackException);
                }
                throw;
            }
        }

        internal void Rollback()
        { RollbackAll(_mutations); }

        internal static ProtectionFromAlignmentPublicationObservation Observe(
            LibraryScriptableObject library)
        {
            if (library == null) throw new ArgumentNullException("library");
            int resolved = 0;
            int published = 0;
            int invalid = 0;
            foreach (ProtectionBuffSpec spec in ProtectionBuffs)
            {
                BlueprintScriptableObject value;
                string issue;
                if (!TryResolveExact(library, spec.Guid, typeof(BlueprintBuff),
                    spec.Name, out value, out issue)) continue;
                resolved++;
                ProtectionFromAlignmentControlImmunityComponent[] components =
                    (((BlueprintBuff)value).ComponentsArray ??
                        Array.Empty<BlueprintComponent>())
                        .OfType<
                            ProtectionFromAlignmentControlImmunityComponent>()
                        .ToArray();
                int exact = components.Count(component =>
                    component.ProtectedAgainstAlignment == spec.ProtectedAgainst);
                published += exact;
                invalid += components.Length - exact + Math.Max(0, exact - 1);
            }
            return new ProtectionFromAlignmentPublicationObservation(
                ProtectionBuffs.Length, resolved, published, invalid);
        }

        private static bool TryResolveExact(LibraryScriptableObject library,
            string guid, Type expectedType, string role,
            out BlueprintScriptableObject resolved, out string issue)
        {
            resolved = null;
            BlueprintId id = BlueprintId.Parse(guid, "guid");
            if (library.BlueprintsByAssetId == null)
            {
                issue = role + "(" + id.Value +
                    "): blueprint dictionary unavailable";
                return false;
            }
            if (!library.BlueprintsByAssetId.TryGetValue(id.Value, out resolved) ||
                resolved == null)
            {
                issue = role + "(" + id.Value + "): missing";
                resolved = null;
                return false;
            }
            if (resolved.GetType() != expectedType)
            {
                issue = role + "(" + id.Value + "): expected " +
                    expectedType.FullName + ", observed " +
                    resolved.GetType().FullName;
                resolved = null;
                return false;
            }
            if (string.IsNullOrWhiteSpace(resolved.name))
            {
                issue = role + "(" + id.Value + "): internal name missing";
                resolved = null;
                return false;
            }
            issue = string.Empty;
            return true;
        }

        private static void Validate(ResolvedProtection protection)
        {
            ProtectionFromAlignmentControlImmunityComponent[] components =
                (protection.Buff.ComponentsArray ??
                    Array.Empty<BlueprintComponent>()).OfType<
                        ProtectionFromAlignmentControlImmunityComponent>()
                    .ToArray();
            if (components.Length != 1 ||
                components[0].ProtectedAgainstAlignment !=
                    protection.Spec.ProtectedAgainst)
                throw new InvalidOperationException(protection.Spec.Name +
                    " does not contain exactly one control-immunity component for " +
                    protection.Spec.ProtectedAgainst + ".");
        }

        private static void RollbackAll(IList<Mutation> mutations)
        {
            for (int index = mutations.Count - 1; index >= 0; index--)
            {
                Mutation mutation = mutations[index];
                if (!ReferenceEquals(mutation.Buff.ComponentsArray,
                    mutation.Published))
                    throw new InvalidOperationException(
                        mutation.Buff.name +
                        " changed after protection-control publication; rollback refused.");
                mutation.Buff.ComponentsArray = mutation.Original;
            }
        }

        private static void LogSummary(ModLogger logger,
            ProtectionFromAlignmentPublicationSummary summary)
        {
            logger.Info("protection-from-alignment", "publication.summary",
                string.Format(CultureInfo.InvariantCulture,
                    "enabled={0};protection-buffs-resolved={1};patched={2};" +
                    "already-patched-skipped={3};control-abilities={4};" +
                    "control-buffs={5};missing-required={6};missing-optional={7};" +
                    "cotw-registration-available={8};required-detail={9};optional-detail={10}",
                    summary.Enabled, summary.ProtectionBuffsResolved,
                    summary.ProtectionBuffsPatched,
                    summary.ProtectionBuffsAlreadyPatched,
                    summary.RegisteredControlAbilities,
                    summary.RegisteredControlBuffs,
                    summary.MissingRequiredAssets.Count,
                    summary.MissingOptionalAssets.Count,
                    summary.CallOfTheWildRegistrationAvailable,
                    JoinDetails(summary.MissingRequiredAssets),
                    JoinDetails(summary.MissingOptionalAssets)));
        }

        private static string JoinDetails(IReadOnlyList<string> values)
        {
            return values.Count == 0 ? "<none>" :
                string.Join("|", values.ToArray());
        }

        private sealed class Mutation
        {
            internal Mutation(BlueprintBuff buff, BlueprintComponent[] original,
                BlueprintComponent[] published)
            {
                Buff = buff;
                Original = original;
                Published = published;
            }
            internal BlueprintBuff Buff { get; private set; }
            internal BlueprintComponent[] Original { get; private set; }
            internal BlueprintComponent[] Published { get; private set; }
        }

        private sealed class ResolvedProtection
        {
            internal ResolvedProtection(BlueprintBuff buff,
                ProtectionBuffSpec spec)
            { Buff = buff; Spec = spec; }
            internal BlueprintBuff Buff { get; private set; }
            internal ProtectionBuffSpec Spec { get; private set; }
        }

        private sealed class ProtectionBuffSpec
        {
            internal ProtectionBuffSpec(string name, string guid,
                AlignmentComponent protectedAgainst)
            {
                Name = name;
                Guid = BlueprintId.Parse(guid, "guid").Value;
                ProtectedAgainst = protectedAgainst;
            }
            internal string Name { get; private set; }
            internal string Guid { get; private set; }
            internal AlignmentComponent ProtectedAgainst { get; private set; }
        }

        private static readonly ProtectionBuffSpec[] ProtectionBuffs = {
            new ProtectionBuffSpec("ProtectionFromEvilBuff",
                "4a6911969911ce9499bf27dde9bfcedc", AlignmentComponent.Evil),
            new ProtectionBuffSpec("ProtectionFromGoodBuff",
                "b19e788487556aa4397080ef3dbb3619", AlignmentComponent.Good),
            new ProtectionBuffSpec("ProtectionFromLawBuff",
                "744bec63273df53438c6b76aaaa78382", AlignmentComponent.Lawful),
            new ProtectionBuffSpec("ProtectionFromChaosBuff",
                "92150879041b1fb48acfbcf7034e8b33", AlignmentComponent.Chaotic),
            new ProtectionBuffSpec("AuraOfProtectionFromEvilEffectBuff",
                "8deb9d5cef3472646ac5199eb9edfb87", AlignmentComponent.Evil)
        };
    }

    internal sealed class ProtectionFromAlignmentPublicationSummary
    {
        internal ProtectionFromAlignmentPublicationSummary(bool enabled,
            int resolved, int patched, int alreadyPatched, int abilities, int buffs,
            IList<string> missingRequired, IList<string> missingOptional,
            bool callOfTheWildAvailable)
        {
            Enabled = enabled;
            ProtectionBuffsResolved = resolved;
            ProtectionBuffsPatched = patched;
            ProtectionBuffsAlreadyPatched = alreadyPatched;
            RegisteredControlAbilities = abilities;
            RegisteredControlBuffs = buffs;
            MissingRequiredAssets = new List<string>(
                missingRequired).AsReadOnly();
            MissingOptionalAssets = new List<string>(
                missingOptional).AsReadOnly();
            CallOfTheWildRegistrationAvailable = callOfTheWildAvailable;
        }
        internal bool Enabled { get; private set; }
        internal int ProtectionBuffsResolved { get; private set; }
        internal int ProtectionBuffsPatched { get; private set; }
        internal int ProtectionBuffsAlreadyPatched { get; private set; }
        internal int RegisteredControlAbilities { get; private set; }
        internal int RegisteredControlBuffs { get; private set; }
        internal IReadOnlyList<string> MissingRequiredAssets { get; private set; }
        internal IReadOnlyList<string> MissingOptionalAssets { get; private set; }
        internal bool CallOfTheWildRegistrationAvailable { get; private set; }
    }

    internal sealed class ProtectionFromAlignmentPublicationObservation
    {
        internal ProtectionFromAlignmentPublicationObservation(int expected,
            int resolved, int published, int invalid)
        {
            ExpectedProtectionBuffs = expected;
            ResolvedProtectionBuffs = resolved;
            PublishedComponents = published;
            InvalidComponents = invalid;
        }
        internal int ExpectedProtectionBuffs { get; private set; }
        internal int ResolvedProtectionBuffs { get; private set; }
        internal int PublishedComponents { get; private set; }
        internal int InvalidComponents { get; private set; }
    }
}
