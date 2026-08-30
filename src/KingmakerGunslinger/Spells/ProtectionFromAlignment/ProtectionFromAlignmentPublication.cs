using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    internal sealed class ProtectionFromAlignmentPublication
    {
        private readonly List<ComponentMutation> _componentMutations;
        private readonly List<DescriptionMutation> _descriptionMutations;

        private ProtectionFromAlignmentPublication(
            List<ComponentMutation> componentMutations,
            List<DescriptionMutation> descriptionMutations,
            ProtectionFromAlignmentPublicationSummary summary)
        {
            _componentMutations = componentMutations;
            _descriptionMutations = descriptionMutations;
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
            var descriptions = new List<ResolvedDescription>();
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
            foreach (ProtectionDescriptionSpec spec in DescriptionTargets)
            {
                BlueprintScriptableObject resolved;
                string issue;
                if (TryResolveExact(library, spec.Guid, spec.ExpectedType,
                    spec.Name, out resolved, out issue))
                    descriptions.Add(new ResolvedDescription(
                        (BlueprintUnitFact)resolved, spec));
                else if (!requiredIssues.Contains(issue))
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
                    protections.Count, 0, 0, descriptions.Count, 0, 0,
                    catalog.AbilityCount, catalog.BuffCount,
                    requiredIssues, optionalIssues,
                    optionalCount > 0 && optionalResolved == optionalCount);
                LogSummary(logger, failed);
                if (enabled)
                    throw new InvalidOperationException(
                        "Protection from Alignment required blueprint audit failed: " +
                        string.Join(" | ", requiredIssues.ToArray()));
                return new ProtectionFromAlignmentPublication(
                    new List<ComponentMutation>(),
                    new List<DescriptionMutation>(), failed);
            }

            var componentMutations = new List<ComponentMutation>();
            var descriptionMutations = new List<DescriptionMutation>();
            int patched = 0;
            int alreadyPatched = 0;
            int descriptionsPatched = 0;
            int descriptionsAlreadyPatched = 0;
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
                        componentMutations.Add(new ComponentMutation(
                            protection.Buff, original, published));
                        patched++;
                        Validate(protection);
                    }

                    BlueprintUnitFactAccess factAccess =
                        BlueprintUnitFactAccess.Resolve();
                    foreach (ResolvedDescription description in descriptions)
                    {
                        LocalizedString original =
                            factAccess.GetDescription(description.Fact);
                        if (original == null)
                            throw new InvalidOperationException(
                                description.Spec.Name +
                                " has no original description to preserve for rollback.");
                        LocalizedString published = LocalizationService.Create(
                            description.Spec.LocalizationKey,
                            description.Spec.Description);
                        string currentKey = original == null ? null : original.Key;
                        ProtectionDescriptionPublicationDecision decision =
                            ProtectionDescriptionPublicationPolicy.Decide(
                                currentKey,
                                description.Spec.LocalizationKey);
                        if (decision ==
                            ProtectionDescriptionPublicationDecision
                                .AlreadyPublished)
                        {
                            descriptionsAlreadyPatched++;
                            Validate(description);
                            continue;
                        }
                        factAccess.SetDescription(description.Fact, published);
                        descriptionMutations.Add(new DescriptionMutation(
                            description.Fact, original, published));
                        descriptionsPatched++;
                        Validate(description);
                    }
                }
                var summary = new ProtectionFromAlignmentPublicationSummary(enabled,
                    protections.Count, patched, alreadyPatched,
                    descriptions.Count, descriptionsPatched,
                    descriptionsAlreadyPatched,
                    catalog.AbilityCount, catalog.BuffCount, requiredIssues,
                    optionalIssues,
                    optionalCount > 0 && optionalResolved == optionalCount);
                LogSummary(logger, summary);
                return new ProtectionFromAlignmentPublication(
                    componentMutations, descriptionMutations, summary);
            }
            catch (Exception publicationException)
            {
                try
                {
                    RollbackAll(componentMutations, descriptionMutations);
                }
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
        {
            RollbackAll(_componentMutations, _descriptionMutations);
        }

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

            int descriptionsResolved = 0;
            int descriptionsPublished = 0;
            int descriptionsInvalid = 0;
            BlueprintUnitFactAccess factAccess = BlueprintUnitFactAccess.Resolve();
            foreach (ProtectionDescriptionSpec spec in DescriptionTargets)
            {
                BlueprintScriptableObject value;
                string issue;
                if (!TryResolveExact(library, spec.Guid, spec.ExpectedType,
                    spec.Name, out value, out issue)) continue;
                descriptionsResolved++;
                var fact = (BlueprintUnitFact)value;
                LocalizedString current = factAccess.GetDescription(fact);
                if (current != null && string.Equals(current.Key,
                    spec.LocalizationKey, StringComparison.Ordinal))
                {
                    descriptionsPublished++;
                    if (!string.Equals(fact.Description, spec.Description,
                        StringComparison.Ordinal)) descriptionsInvalid++;
                }
                else if (current != null &&
                    ProtectionDescriptionPublicationPolicy.IsOwnedKey(
                        current.Key))
                    descriptionsInvalid++;
            }
            return new ProtectionFromAlignmentPublicationObservation(
                ProtectionBuffs.Length, resolved, published, invalid,
                DescriptionTargets.Length, descriptionsResolved,
                descriptionsPublished, descriptionsInvalid);
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
                string candidates = DescribeNameCandidates(library, role);
                if (!string.IsNullOrEmpty(candidates))
                    issue += "; name-candidates=" + candidates;
                resolved = null;
                return false;
            }
            if (resolved.GetType() != expectedType)
            {
                issue = role + "(" + id.Value + "): expected " +
                    expectedType.FullName + ", observed " +
                    resolved.GetType().FullName + "; actual-name=" +
                    resolved.name + "; components=" +
                    DescribeComponents(resolved);
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

        private static string DescribeNameCandidates(
            LibraryScriptableObject library, string role)
        {
            if (library.BlueprintsByAssetId == null ||
                string.IsNullOrWhiteSpace(role)) return string.Empty;
            string stem = role.EndsWith("Buff", StringComparison.Ordinal) ?
                role.Substring(0, role.Length - 4) : role;
            return string.Join(",", library.BlueprintsByAssetId
                .Where(pair => pair.Value != null &&
                    !string.IsNullOrWhiteSpace(pair.Value.name) &&
                    pair.Value.name.IndexOf(stem,
                        StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(pair => pair.Value.name, StringComparer.Ordinal)
                .ThenBy(pair => pair.Key, StringComparer.Ordinal)
                .Take(12)
                .Select(pair => pair.Value.name + "@" + pair.Key + ":" +
                    pair.Value.GetType().FullName).ToArray());
        }

        private static string DescribeComponents(BlueprintScriptableObject value)
        {
            var fact = value as BlueprintUnitFact;
            if (fact == null) return "<not-a-unit-fact>";
            BlueprintComponent[] components = fact.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            if (components.Length == 0) return "<none>";
            return string.Join(",", components.Where(component =>
                    component != null).Select(DescribeComponent).ToArray());
        }

        private static string DescribeComponent(BlueprintComponent component)
        {
            var references = new List<string>();
            System.Reflection.FieldInfo[] fields = component.GetType().GetFields(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                try
                {
                    var referenced = field.GetValue(component) as
                        BlueprintScriptableObject;
                    if (referenced != null)
                        references.Add(field.Name + "=" + referenced.name + "@" +
                            referenced.AssetGuid + ":" +
                            referenced.GetType().FullName);
                }
                catch
                {
                    references.Add(field.Name + "=<unreadable>");
                }
            }
            return component.GetType().FullName +
                (references.Count == 0 ? string.Empty : "[" +
                    string.Join(",", references.ToArray()) + "]");
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

        private static void Validate(ResolvedDescription description)
        {
            LocalizedString current = BlueprintUnitFactAccess.Resolve()
                .GetDescription(description.Fact);
            if (current == null || !string.Equals(current.Key,
                    description.Spec.LocalizationKey, StringComparison.Ordinal) ||
                !string.Equals(description.Fact.Description,
                    description.Spec.Description, StringComparison.Ordinal))
                throw new InvalidOperationException(description.Spec.Name +
                    " does not contain the expected player-facing protection description.");
        }

        private static void RollbackAll(
            IList<ComponentMutation> componentMutations,
            IList<DescriptionMutation> descriptionMutations)
        {
            BlueprintUnitFactAccess factAccess = BlueprintUnitFactAccess.Resolve();
            for (int index = descriptionMutations.Count - 1; index >= 0; index--)
            {
                DescriptionMutation mutation = descriptionMutations[index];
                if (!ReferenceEquals(factAccess.GetDescription(mutation.Fact),
                    mutation.Published))
                    throw new InvalidOperationException(
                        mutation.Fact.name +
                        " changed after protection-description publication; rollback refused.");
                factAccess.SetDescription(mutation.Fact, mutation.Original);
            }
            for (int index = componentMutations.Count - 1; index >= 0; index--)
            {
                ComponentMutation mutation = componentMutations[index];
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
                    "already-patched-skipped={3};descriptions-resolved={4};" +
                    "descriptions-patched={5};descriptions-already-patched-skipped={6};" +
                    "control-abilities={7};control-buffs={8};missing-required={9};" +
                    "missing-optional={10};cotw-registration-available={11};" +
                    "required-detail={12};optional-detail={13}",
                    summary.Enabled, summary.ProtectionBuffsResolved,
                    summary.ProtectionBuffsPatched,
                    summary.ProtectionBuffsAlreadyPatched,
                    summary.DescriptionsResolved,
                    summary.DescriptionsPatched,
                    summary.DescriptionsAlreadyPatched,
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

        private sealed class ComponentMutation
        {
            internal ComponentMutation(BlueprintBuff buff,
                BlueprintComponent[] original,
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

        private sealed class DescriptionMutation
        {
            internal DescriptionMutation(BlueprintUnitFact fact,
                LocalizedString original, LocalizedString published)
            {
                Fact = fact;
                Original = original;
                Published = published;
            }
            internal BlueprintUnitFact Fact { get; private set; }
            internal LocalizedString Original { get; private set; }
            internal LocalizedString Published { get; private set; }
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

        private sealed class ResolvedDescription
        {
            internal ResolvedDescription(BlueprintUnitFact fact,
                ProtectionDescriptionSpec spec)
            { Fact = fact; Spec = spec; }
            internal BlueprintUnitFact Fact { get; private set; }
            internal ProtectionDescriptionSpec Spec { get; private set; }
        }

        private sealed class ProtectionDescriptionSpec
        {
            internal ProtectionDescriptionSpec(string name, string guid,
                Type expectedType, string localizationKey, string description)
            {
                Name = name;
                Guid = BlueprintId.Parse(guid, "guid").Value;
                ExpectedType = expectedType;
                LocalizationKey = localizationKey;
                Description = description;
            }
            internal string Name { get; private set; }
            internal string Guid { get; private set; }
            internal Type ExpectedType { get; private set; }
            internal string LocalizationKey { get; private set; }
            internal string Description { get; private set; }
        }

        private static readonly ProtectionBuffSpec[] ProtectionBuffs = {
            new ProtectionBuffSpec("ProtectionFromEvilBuff",
                "4a6911969911ce9499bf27dde9bfcedc", AlignmentComponent.Evil),
            new ProtectionBuffSpec("ProtectionFromGoodBuff",
                "b19e788487556aa4397080ef3dbb3619", AlignmentComponent.Good),
            new ProtectionBuffSpec("ProtectionFromLawBuff",
                "744bec63273df53438c6b76aaaa78382", AlignmentComponent.Lawful),
            new ProtectionBuffSpec("ProtectionFromChaosBuff",
                "a4742d7afde0f4f47b380abed025b219", AlignmentComponent.Chaotic),
            new ProtectionBuffSpec("AuraOfProtectionFromEvilEffectBuff",
                "8deb9d5cef3472646ac5199eb9edfb87", AlignmentComponent.Evil)
        };

        private static readonly ProtectionDescriptionSpec[] DescriptionTargets = {
            new ProtectionDescriptionSpec("ProtectionFromAlignment",
                "433b1faf4d02cc34abb0ade5ceda47c4", typeof(BlueprintAbility),
                "KMG.ProtectionFromAlignment.Ability.Generic.Description",
                ProtectionFromAlignmentDescriptions.GenericSpell(false)),
            new ProtectionDescriptionSpec("ProtectionFromEvil",
                "eee384c813b6d74498d1b9cc720d61f4", typeof(BlueprintAbility),
                "KMG.ProtectionFromAlignment.Ability.Evil.Description",
                ProtectionFromAlignmentDescriptions.SpecificSpell(
                    ProtectionAlignment.Evil, false)),
            new ProtectionDescriptionSpec("ProtectionFromGood",
                "2ac7637daeb2aa143a3bae860095b63e", typeof(BlueprintAbility),
                "KMG.ProtectionFromAlignment.Ability.Good.Description",
                ProtectionFromAlignmentDescriptions.SpecificSpell(
                    ProtectionAlignment.Good, false)),
            new ProtectionDescriptionSpec("ProtectionFromLaw",
                "c3aafbbb6e8fc754fb8c82ede3280051", typeof(BlueprintAbility),
                "KMG.ProtectionFromAlignment.Ability.Law.Description",
                ProtectionFromAlignmentDescriptions.SpecificSpell(
                    ProtectionAlignment.Law, false)),
            new ProtectionDescriptionSpec("ProtectionFromChaos",
                "1eaf1020e82028d4db55e6e464269e00", typeof(BlueprintAbility),
                "KMG.ProtectionFromAlignment.Ability.Chaos.Description",
                ProtectionFromAlignmentDescriptions.SpecificSpell(
                    ProtectionAlignment.Chaos, false)),
            new ProtectionDescriptionSpec("ProtectionFromAlignmentCommunal",
                "2cadf6c6350e4684baa109d067277a45", typeof(BlueprintAbility),
                "KMG.ProtectionFromAlignment.Ability.GenericCommunal.Description",
                ProtectionFromAlignmentDescriptions.GenericSpell(true)),
            new ProtectionDescriptionSpec("ProtectionFromEvilCommunal",
                "93f391b0c5a99e04e83bbfbe3bb6db64", typeof(BlueprintAbility),
                "KMG.ProtectionFromAlignment.Ability.EvilCommunal.Description",
                ProtectionFromAlignmentDescriptions.SpecificSpell(
                    ProtectionAlignment.Evil, true)),
            new ProtectionDescriptionSpec("ProtectionFromGoodCommunal",
                "5bfd4cce1557d5744914f8f6d85959a4", typeof(BlueprintAbility),
                "KMG.ProtectionFromAlignment.Ability.GoodCommunal.Description",
                ProtectionFromAlignmentDescriptions.SpecificSpell(
                    ProtectionAlignment.Good, true)),
            new ProtectionDescriptionSpec("ProtectionFromLawCommunal",
                "8b8ccc9763e3cc74bbf5acc9c98557b9", typeof(BlueprintAbility),
                "KMG.ProtectionFromAlignment.Ability.LawCommunal.Description",
                ProtectionFromAlignmentDescriptions.SpecificSpell(
                    ProtectionAlignment.Law, true)),
            new ProtectionDescriptionSpec("ProtectionFromChaosCommunal",
                "0ec75ec95d9e39d47a23610123ba1bad", typeof(BlueprintAbility),
                "KMG.ProtectionFromAlignment.Ability.ChaosCommunal.Description",
                ProtectionFromAlignmentDescriptions.SpecificSpell(
                    ProtectionAlignment.Chaos, true)),
            new ProtectionDescriptionSpec("ProtectionFromEvilBuff",
                "4a6911969911ce9499bf27dde9bfcedc", typeof(BlueprintBuff),
                "KMG.ProtectionFromAlignment.Buff.Evil.Description",
                ProtectionFromAlignmentDescriptions.Buff(
                    ProtectionAlignment.Evil)),
            new ProtectionDescriptionSpec("ProtectionFromGoodBuff",
                "b19e788487556aa4397080ef3dbb3619", typeof(BlueprintBuff),
                "KMG.ProtectionFromAlignment.Buff.Good.Description",
                ProtectionFromAlignmentDescriptions.Buff(
                    ProtectionAlignment.Good)),
            new ProtectionDescriptionSpec("ProtectionFromLawBuff",
                "744bec63273df53438c6b76aaaa78382", typeof(BlueprintBuff),
                "KMG.ProtectionFromAlignment.Buff.Law.Description",
                ProtectionFromAlignmentDescriptions.Buff(
                    ProtectionAlignment.Law)),
            new ProtectionDescriptionSpec("ProtectionFromChaosBuff",
                "a4742d7afde0f4f47b380abed025b219", typeof(BlueprintBuff),
                "KMG.ProtectionFromAlignment.Buff.Chaos.Description",
                ProtectionFromAlignmentDescriptions.Buff(
                    ProtectionAlignment.Chaos)),
            new ProtectionDescriptionSpec("AuraOfProtectionFromEvilEffectBuff",
                "8deb9d5cef3472646ac5199eb9edfb87", typeof(BlueprintBuff),
                "KMG.ProtectionFromAlignment.Buff.PaladinEvil.Description",
                ProtectionFromAlignmentDescriptions.Buff(
                    ProtectionAlignment.Evil))
        };
    }

    internal sealed class ProtectionFromAlignmentPublicationSummary
    {
        internal ProtectionFromAlignmentPublicationSummary(bool enabled,
            int resolved, int patched, int alreadyPatched,
            int descriptionsResolved, int descriptionsPatched,
            int descriptionsAlreadyPatched, int abilities, int buffs,
            IList<string> missingRequired, IList<string> missingOptional,
            bool callOfTheWildAvailable)
        {
            Enabled = enabled;
            ProtectionBuffsResolved = resolved;
            ProtectionBuffsPatched = patched;
            ProtectionBuffsAlreadyPatched = alreadyPatched;
            DescriptionsResolved = descriptionsResolved;
            DescriptionsPatched = descriptionsPatched;
            DescriptionsAlreadyPatched = descriptionsAlreadyPatched;
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
        internal int DescriptionsResolved { get; private set; }
        internal int DescriptionsPatched { get; private set; }
        internal int DescriptionsAlreadyPatched { get; private set; }
        internal int RegisteredControlAbilities { get; private set; }
        internal int RegisteredControlBuffs { get; private set; }
        internal IReadOnlyList<string> MissingRequiredAssets { get; private set; }
        internal IReadOnlyList<string> MissingOptionalAssets { get; private set; }
        internal bool CallOfTheWildRegistrationAvailable { get; private set; }
    }

    internal sealed class ProtectionFromAlignmentPublicationObservation
    {
        internal ProtectionFromAlignmentPublicationObservation(int expected,
            int resolved, int published, int invalid,
            int expectedDescriptions, int resolvedDescriptions,
            int publishedDescriptions, int invalidDescriptions)
        {
            ExpectedProtectionBuffs = expected;
            ResolvedProtectionBuffs = resolved;
            PublishedComponents = published;
            InvalidComponents = invalid;
            ExpectedDescriptions = expectedDescriptions;
            ResolvedDescriptions = resolvedDescriptions;
            PublishedDescriptions = publishedDescriptions;
            InvalidDescriptions = invalidDescriptions;
        }
        internal int ExpectedProtectionBuffs { get; private set; }
        internal int ResolvedProtectionBuffs { get; private set; }
        internal int PublishedComponents { get; private set; }
        internal int InvalidComponents { get; private set; }
        internal int ExpectedDescriptions { get; private set; }
        internal int ResolvedDescriptions { get; private set; }
        internal int PublishedDescriptions { get; private set; }
        internal int InvalidDescriptions { get; private set; }
    }
}
