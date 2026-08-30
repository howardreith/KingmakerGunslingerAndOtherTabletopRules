using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Spells.ProtectionFromAlignment;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ProtectionFromAlignmentControlImmunityTests
    {
        private const string DominatePersonAbility =
            "d7cbd2004ce66a042aeab2e95a3c5c61";
        private const string DominatePersonBuff =
            "c0f4e1c24c9cd334ca988ed1bd9d201f";
        private const string KmgSuccubusAbility =
            "1662d63944d94cdeaa62562dc9ac9349";
        private const string KmgSuccubusBuff =
            "6e1f6eb3e773451dbda9e0ecd07486d9";

        internal static void MatchingAndMismatchedAlignmentsAreExact()
        {
            MentalControlCatalog catalog = MentalControlCatalogDefaults.Create();
            Assertions.True(Evaluate(catalog, ProtectionAlignment.Evil,
                DominatePersonAbility, null, true, ProtectionAlignment.Evil)
                .ShouldBlock,
                "Protection from Evil must block registered evil domination.");
            Assertions.False(Evaluate(catalog, ProtectionAlignment.Evil,
                DominatePersonAbility, null, true, ProtectionAlignment.Good)
                .ShouldBlock,
                "Protection from Evil must not block a good source.");
            Assertions.False(Evaluate(catalog, ProtectionAlignment.Evil,
                DominatePersonAbility, null, true, ProtectionAlignment.None)
                .ShouldBlock,
                "Protection from Evil must not block a classified neutral source.");

            ProtectionAlignment lawfulEvil = ProtectionAlignment.Law |
                ProtectionAlignment.Evil;
            Assertions.True(Evaluate(catalog, ProtectionAlignment.Evil,
                DominatePersonAbility, null, true, lawfulEvil).ShouldBlock,
                "A lawful evil source must match Protection from Evil.");
            Assertions.True(Evaluate(catalog, ProtectionAlignment.Law,
                DominatePersonAbility, null, true, lawfulEvil).ShouldBlock,
                "A lawful evil source must also match Protection from Law.");
            Assertions.True(Evaluate(catalog, ProtectionAlignment.Good,
                DominatePersonAbility, null, true, ProtectionAlignment.Good)
                .ShouldBlock,
                "Protection from Good must use the good source component.");
            Assertions.True(Evaluate(catalog, ProtectionAlignment.Law,
                DominatePersonAbility, null, true, ProtectionAlignment.Law)
                .ShouldBlock,
                "Protection from Law must use the lawful source component.");
            Assertions.True(Evaluate(catalog, ProtectionAlignment.Chaos,
                DominatePersonAbility, null, true, ProtectionAlignment.Chaos)
                .ShouldBlock,
                "Protection from Chaos must use the chaotic source component.");
            Assertions.False(Evaluate(catalog, ProtectionAlignment.Chaos,
                DominatePersonAbility, null, true, ProtectionAlignment.Law)
                .ShouldBlock,
                "Opposite-axis protection must not infer a match.");
        }

        internal static void TerminalAndExpandedSuccubusRegistrationsBlock()
        {
            MentalControlCatalog catalog = MentalControlCatalogDefaults.Create();
            ProtectionControlImmunityDecision direct = Evaluate(catalog,
                ProtectionAlignment.Evil, null, DominatePersonBuff, true,
                ProtectionAlignment.Evil);
            Assertions.True(direct.QualifyingControl && direct.ShouldBlock,
                "A registered terminal domination buff must be authoritative without an ability.");

            MentalControlCatalogEntry ability;
            MentalControlCatalogEntry buff;
            Assertions.True(catalog.TryGetAbility(KmgSuccubusAbility, out ability) &&
                catalog.TryGetBuff(KmgSuccubusBuff, out buff),
                "Expanded Summoning Succubus delivery and terminal buff are not registered.");
            Assertions.True(Evaluate(catalog, ProtectionAlignment.Evil,
                KmgSuccubusAbility, KmgSuccubusBuff, true,
                ProtectionAlignment.Evil).ShouldBlock,
                "The mod's evil Succubus domination must be blocked.");
        }

        internal static void BroadDescriptorsAndUnrelatedEffectsRemainAllowed()
        {
            MentalControlCatalog catalog = MentalControlCatalogDefaults.Create();
            string[] unregistered = {
                "11111111111111111111111111111111",
                "22222222222222222222222222222222",
                "33333333333333333333333333333333",
                "44444444444444444444444444444444",
                "55555555555555555555555555555555"
            };
            string[] labels = { "mind-affecting", "fear", "sleep", "confusion",
                "beneficial mind-affecting" };
            for (int index = 0; index < unregistered.Length; index++)
            {
                ProtectionControlImmunityDecision decision = Evaluate(catalog,
                    ProtectionAlignment.Evil, unregistered[index], null, true,
                    ProtectionAlignment.Evil);
                Assertions.False(decision.QualifyingControl || decision.ShouldBlock,
                    "An unregistered " + labels[index] + " effect was blocked.");
            }

            string root = SourceRoot();
            string adapter = File.ReadAllText(Path.Combine(root,
                "ProtectionFromAlignmentControlImmunityComponent.cs"));
            string policy = File.ReadAllText(Path.Combine(root,
                "ProtectionControlImmunityPolicy.cs"));
            string combined = adapter + policy;
            foreach (string prohibited in new[] { "SpellDescriptor", "MindAffecting",
                "Compulsion", "DescriptorImmunity", "Fear", "Emotion" })
                Assertions.False(combined.Contains(prohibited),
                    "Runtime qualification must not infer control from " + prohibited + ".");
        }

        internal static void UnknownSourcesFailOpenUnlessTrusted()
        {
            MentalControlCatalog defaults = MentalControlCatalogDefaults.Create();
            ProtectionControlImmunityDecision unresolved = Evaluate(defaults,
                ProtectionAlignment.Evil, DominatePersonAbility,
                DominatePersonBuff, false, ProtectionAlignment.None);
            Assertions.True(unresolved.QualifyingControl &&
                !unresolved.MatchingAlignment && !unresolved.ShouldBlock &&
                !unresolved.UsedTrustedAlignment,
                "An unresolved ordinary source must fail open.");

            const string trustedGuid = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var trusted = new MentalControlCatalog();
            trusted.Register(new MentalControlCatalogEntry("TrustedFixture",
                trustedGuid, MentalControlBlueprintKind.Ability,
                MentalControlContentSource.VanillaKingmaker,
                "Test-only source-less effect with audited alignment.", true,
                ProtectionAlignment.Evil | ProtectionAlignment.Law));
            ProtectionControlImmunityDecision evil = Evaluate(trusted,
                ProtectionAlignment.Evil, trustedGuid, null, false,
                ProtectionAlignment.None);
            ProtectionControlImmunityDecision law = Evaluate(trusted,
                ProtectionAlignment.Law, trustedGuid, null, false,
                ProtectionAlignment.None);
            ProtectionControlImmunityDecision good = Evaluate(trusted,
                ProtectionAlignment.Good, trustedGuid, null, false,
                ProtectionAlignment.None);
            Assertions.True(evil.ShouldBlock && evil.UsedTrustedAlignment &&
                law.ShouldBlock && law.UsedTrustedAlignment,
                "Exact trusted alignment metadata must support each audited component.");
            Assertions.False(good.ShouldBlock || good.UsedTrustedAlignment,
                "Trusted metadata must not invent an unregistered alignment.");
        }

        internal static void CatalogIsExplicitCompleteAndIdempotent()
        {
            MentalControlCatalog first = MentalControlCatalogDefaults.Create();
            MentalControlCatalog second = MentalControlCatalogDefaults.Create();
            Assertions.Equal(14, first.AbilityCount,
                "Mental-control ability inventory changed unexpectedly.");
            Assertions.Equal(8, first.BuffCount,
                "Mental-control terminal-buff inventory changed unexpectedly.");
            Assertions.Equal(first.AbilityCount, second.AbilityCount,
                "Repeated catalog initialization duplicated abilities.");
            Assertions.Equal(first.BuffCount, second.BuffCount,
                "Repeated catalog initialization duplicated buffs.");
            Assertions.Equal(13, first.Entries.Count(value => value.ContentSource ==
                MentalControlContentSource.VanillaKingmaker),
                "Vanilla mental-control inventory changed unexpectedly.");
            Assertions.Equal(2, first.Entries.Count(value => value.ContentSource ==
                MentalControlContentSource.KingmakerGunslinger),
                "Gunslinger mental-control inventory changed unexpectedly.");
            Assertions.Equal(7, first.Entries.Count(value => value.ContentSource ==
                MentalControlContentSource.CallOfTheWild),
                "Optional Call of the Wild inventory changed unexpectedly.");
            Assertions.True(first.Entries.All(value =>
                !string.IsNullOrWhiteSpace(value.BlueprintName) &&
                !string.IsNullOrWhiteSpace(value.Reason)),
                "Every catalog entry must retain its name and inclusion reason.");

            MentalControlCatalogEntry exact = MentalControlCatalogDefaults.All[0];
            Assertions.False(first.Register(exact),
                "An exact repeated registration must be an idempotent no-op.");
            Assertions.Equal(14, first.AbilityCount,
                "An exact repeated registration changed the ability count.");
            Assertions.Throws<InvalidOperationException>(() => first.Register(
                new MentalControlCatalogEntry(exact.BlueprintName, exact.Guid,
                    exact.Kind, exact.ContentSource, "Conflicting reason.",
                    exact.Required)),
                "A conflicting duplicate GUID must fail closed.");
        }

        internal static void ProtectionPublicationPolicyIsIdempotent()
        {
            Assertions.Equal(ProtectionComponentPublicationDecision.Append,
                ProtectionComponentPublicationPolicy.Decide(0),
                "An unpatched protection buff must append one component.");
            Assertions.Equal(ProtectionComponentPublicationDecision.AlreadyPatched,
                ProtectionComponentPublicationPolicy.Decide(1),
                "A singly patched protection buff must be skipped.");
            Assertions.Throws<InvalidOperationException>(() =>
                ProtectionComponentPublicationPolicy.Decide(2),
                "Duplicate protection components must fail closed.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                ProtectionComponentPublicationPolicy.Decide(-1),
                "A negative component count must fail closed.");
        }

        internal static void KingmakerEventAdapterUsesTheNativeTargetSideVeto()
        {
            string root = SourceRoot();
            string adapter = File.ReadAllText(Path.Combine(root,
                "ProtectionFromAlignmentControlImmunityComponent.cs"));
            foreach (string token in new[] {
                "RuleInitiatorLogicComponent<RuleApplyBuff>",
                "evt.Blueprint", "context.SourceAbility", "context.MaybeCaster",
                "source.Descriptor.Alignment.Value", "evt.CanApply = false",
                "ReportUnresolvedSourceOnce" })
                Assertions.True(adapter.Contains(token),
                    "Kingmaker RuleApplyBuff adapter is missing: " + token);
            Assertions.False(adapter.Contains("Harmony") ||
                adapter.Contains("OnEventDidTrigger(RuleApplyBuff evt) { evt"),
                "The feature must use the native pre-application target event only.");

            string runtime = File.ReadAllText(Path.Combine(root,
                "ProtectionFromAlignmentRuntime.cs"));
            Assertions.True(runtime.Contains("HashSet<string>") &&
                runtime.Contains("ReportedUnresolvedSources.Add(key)") &&
                runtime.Contains("logger.Debug") &&
                runtime.Contains("outcome="),
                "Unresolved-source diagnostics are not debug-level and deduplicated.");
        }

        internal static void ProtectionAndWrapperInventoryIsCompleteAndLifecycleScoped()
        {
            string root = Environment.CurrentDirectory;
            string publication = File.ReadAllText(Path.Combine(SourceRoot(),
                "ProtectionFromAlignmentPublication.cs"));
            foreach (string guid in new[] {
                "4a6911969911ce9499bf27dde9bfcedc",
                "b19e788487556aa4397080ef3dbb3619",
                "744bec63273df53438c6b76aaaa78382",
                "92150879041b1fb48acfbcf7034e8b33",
                "8deb9d5cef3472646ac5199eb9edfb87" })
                Assertions.True(publication.Contains(guid),
                    "Protection terminal-buff publication inventory lacks " + guid + ".");

            string documentation = File.ReadAllText(Path.Combine(root, "docs",
                "PROTECTION-FROM-ALIGNMENT-CONTROL-IMMUNITY.md"));
            foreach (string guid in WrapperGuids)
                Assertions.True(documentation.Contains(guid),
                    "Documented protection wrapper inventory lacks " + guid + ".");
            Assertions.True(documentation.Contains(
                "already-active domination remains active") &&
                documentation.Contains("Wrath parity") &&
                documentation.Contains("manual in-game validation"),
                "Lifecycle limitation or manual validation boundary is not documented.");

            string adapter = File.ReadAllText(Path.Combine(SourceRoot(),
                "ProtectionFromAlignmentControlImmunityComponent.cs"));
            foreach (string prohibited in new[] { "OnTurnOn", "OnTurnOff",
                "OnActivate", "OnDeactivate", "RemoveFact", "RemoveBuff",
                "UnitPart", "JsonProperty" })
                Assertions.False(adapter.Contains(prohibited),
                    "The application-only adapter crossed the existing-effect boundary: " +
                    prohibited);
        }

        internal static void OptionalCotwAndModuleIsolationContractsAreSafe()
        {
            MentalControlCatalog catalog = MentalControlCatalogDefaults.Create();
            MentalControlCatalogEntry[] optional = catalog.Entries.Where(value =>
                value.ContentSource == MentalControlContentSource.CallOfTheWild)
                .ToArray();
            Assertions.Equal(7, optional.Length,
                "The audited optional Call of the Wild inventory must be exact.");
            Assertions.True(optional.All(value => !value.Required),
                "Call of the Wild entries must never become required assets.");

            string publication = File.ReadAllText(Path.Combine(SourceRoot(),
                "ProtectionFromAlignmentPublication.cs"));
            Assertions.True(publication.Contains("if (entry.Required)") &&
                publication.Contains("optionalIssues.Add(issue)") &&
                publication.Contains("if (enabled)") &&
                publication.Contains("if (enabled)\n                    throw"),
                "Optional resolution or disabled-module fail-open publication is missing.");

            string bootstrap = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Bootstrap", "BlueprintBootstrap.cs"));
            int summon = bootstrap.IndexOf(
                "ExpandedSummoningBlueprints.Register", StringComparison.Ordinal);
            int protection = bootstrap.IndexOf(
                "ProtectionFromAlignmentPublication.Publish", StringComparison.Ordinal);
            int shield = bootstrap.IndexOf("ShieldOtherBlueprints.Register",
                StringComparison.Ordinal);
            Assertions.True(summon >= 0 && protection > summon && shield > protection &&
                bootstrap.Contains("protectionPublicationException") &&
                bootstrap.Contains("other modules will continue"),
                "Protection publication must be isolated after Succubus registration and before Shield Other.");
        }

        private static ProtectionControlImmunityDecision Evaluate(
            MentalControlCatalog catalog, ProtectionAlignment protectedAgainst,
            string ability, string buff, bool sourceClassified,
            ProtectionAlignment sourceAlignment)
        {
            return ProtectionControlImmunityPolicy.Evaluate(catalog,
                new ProtectionControlImmunityRequest(protectedAgainst, ability,
                    buff, sourceClassified, sourceAlignment));
        }

        private static string SourceRoot()
        {
            return Path.Combine(Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "Spells", "ProtectionFromAlignment");
        }

        private static readonly string[] WrapperGuids = {
            "eee384c813b6d74498d1b9cc720d61f4",
            "2ac7637daeb2aa143a3bae860095b63e",
            "c3aafbbb6e8fc754fb8c82ede3280051",
            "1eaf1020e82028d4db55e6e464269e00",
            "93f391b0c5a99e04e83bbfbe3bb6db64",
            "5bfd4cce1557d5744914f8f6d85959a4",
            "8b8ccc9763e3cc74bbf5acc9c98557b9",
            "0ec75ec95d9e39d47a23610123ba1bad",
            "433b1faf4d02cc34abb0ade5ceda47c4",
            "2cadf6c6350e4684baa109d067277a45",
            "07dccc8e4c4489c4d9de721dddaf12cc",
            "b70104f09b3da794da923fbf248befc5",
            "c28f7234f5fb8c943a77621ad96ad8f9",
            "224f03e74d1dd4648a81242c01e65f41",
            "b6da529f710491b4fa789a5838c1ae8f",
            "3026de673d4d8fe45baf40e0b5edd718",
            "1871a2eb5a1ed024bbd86a04bd9b0ca5",
            "ec487c0ecc801e048aed50851d937fd8",
            "de000ebb9b86c8f48b77576965303183",
            "e5e2567210888184cb3c552c02e86b89",
            "31a74f20fcba2c9419738a94f6727dd6",
            "59110d30bb15dcd4d89f762b6aa9db9b",
            "96eb7a498b4db2c4a9fcfb632064b948",
            "c75c69797fd6ee24d84b12796c0c3d45",
            "6dad6628ecc36c7428f6e877975a1041",
            "a2af3233183a22a4693e3de034068d29",
            "7a7cb3118fdb3274a90fc34dd21457f6",
            "eb776c7c1a2ffc3498adab069588b70c",
            "0afbc5cbd6165a64ea79b0a87058f6c1",
            "915d6ff0a30fe974ca843dde14b1619a"
        };
    }
}
