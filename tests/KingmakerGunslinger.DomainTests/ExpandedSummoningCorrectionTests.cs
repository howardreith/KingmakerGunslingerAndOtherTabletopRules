using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The correction order (2026-09-25) on Sprints 3-8: attack and target
    /// identity for the cats' grab and rake, the universal grab size rule and
    /// the swallow limits, the Giant Flytrap's multi-link hold and engulf, the
    /// chartered mephit roles with the ally-safe cloud, the Cyclops armor
    /// class and Flash of Insight, the Web's ranged touch path, the docile
    /// hooves, and the visual variants' resource ownership. These are the
    /// pure decisions and the source contracts; the live proof is the
    /// correction rules and lifecycle scenarios.
    /// </summary>
    internal static class ExpandedSummoningCorrectionTests
    {
        /// <summary>
        /// Identities the correction appended to the frozen ledger: the
        /// multi-link hold and held states, the Flytrap's engulfed state, the
        /// Cyclops hide armor, the two docile-hoof carriers, the twelve
        /// mephit-role abilities, resources and cast actions, the four
        /// role states, the wind wall area, the ally-safe cloud area and the
        /// pyrotechnics blinded state.
        /// </summary>
        internal const int AppendedLedgerIdentities = 24;

        private static readonly string[] AppendedSymbols = {
            "KMG.Summoning.Special.Grapple.MultiHold", "KMG.Summoning.Special.Grapple.MultiHeld",
            "KMG.Summoning.Special.GiantFlytrap.Engulfed", "KMG.Summoning.Special.Cyclops.HideArmor",
            "KMG.Summoning.Special.Pony.CombatTraits", "KMG.Summoning.Special.Horse.CombatTraits",
            "KMG.Summoning.Special.DustMephit.SpellLikeTwo", "KMG.Summoning.Special.DustMephit.SpellLikeTwoResource",
            "KMG.Summoning.Special.DustMephit.SpellLikeTwoAi", "KMG.Summoning.Special.DustMephit.WindWallArea",
            "KMG.Summoning.Special.DustMephit.WindWallState", "KMG.Summoning.Special.IceMephit.SpellLikeTwo",
            "KMG.Summoning.Special.IceMephit.SpellLikeTwoResource", "KMG.Summoning.Special.IceMephit.SpellLikeTwoAi",
            "KMG.Summoning.Special.IceMephit.ChillMetalState", "KMG.Summoning.Special.MagmaMephit.SpellLikeOne",
            "KMG.Summoning.Special.MagmaMephit.SpellLikeOneResource", "KMG.Summoning.Special.MagmaMephit.SpellLikeOneAi",
            "KMG.Summoning.Special.MagmaMephit.SpellLikeTwo", "KMG.Summoning.Special.MagmaMephit.SpellLikeTwoResource",
            "KMG.Summoning.Special.MagmaMephit.SpellLikeTwoAi", "KMG.Summoning.Special.MagmaMephit.MagmaFormState",
            "KMG.Summoning.Special.OozeMephit.StinkingCloudArea",
            "KMG.Summoning.Special.MagmaMephit.PyrotechnicsBlindedState"
        };

        /// <summary>True for an identity the correction appended (the older sprint ledger filters exclude them).</summary>
        internal static bool IsCorrectionIdentity(string symbol)
        { return AppendedSymbols.Contains(symbol); }

        private static string Source(params string[] parts)
        {
            return File.ReadAllText(Path.Combine(new[] { Environment.CurrentDirectory }
                .Concat(parts).ToArray()));
        }

        private static void RequireTokens(string relativeDescription, string text, params string[] tokens)
        {
            foreach (string token in tokens)
                Assertions.True(text.Contains(token), relativeDescription + " is missing: " + token);
        }

        internal static void GrabAndRakeFollowAttackIdentity()
        {
            ExpandedSummoningSpecialProfiles.Validate();
            // Leopard shape: bite grabs, no claw grabs, two rake slots at the end.
            Assertions.True(ExpandedSummoningSpecialProfiles.IsGrabLimb(true, -1, true, 0, 4, 2),
                "The bite grabs where the stat block says bite plus grab.");
            Assertions.False(ExpandedSummoningSpecialProfiles.IsGrabLimb(false, 0, true, 0, 4, 2),
                "A leopard's claw never grabs.");
            // Tiger shape: bite and the first two claws grab, the rake pair never.
            Assertions.True(ExpandedSummoningSpecialProfiles.IsGrabLimb(false, 0, true, 2, 4, 2) &&
                ExpandedSummoningSpecialProfiles.IsGrabLimb(false, 1, true, 2, 4, 2),
                "A tiger's foreclaws grab.");
            Assertions.False(ExpandedSummoningSpecialProfiles.IsGrabLimb(false, 2, true, 4, 4, 2) ||
                ExpandedSummoningSpecialProfiles.IsGrabLimb(false, 3, true, 4, 4, 2),
                "A rake slot never grabs whatever the claw count says.");
            Assertions.True(ExpandedSummoningSpecialProfiles.IsRakeSlot(2, 4, 2) &&
                ExpandedSummoningSpecialProfiles.IsRakeSlot(3, 4, 2) &&
                !ExpandedSummoningSpecialProfiles.IsRakeSlot(1, 4, 2) &&
                !ExpandedSummoningSpecialProfiles.IsRakeSlot(0, 3, 0),
                "The last rake-count limbs are the rake; a creature without rake has none.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldRakeApply(true, true, false) &&
                ExpandedSummoningSpecialProfiles.ShouldRakeApply(true, false, true) &&
                !ExpandedSummoningSpecialProfiles.ShouldRakeApply(true, false, false) &&
                ExpandedSummoningSpecialProfiles.ShouldRakeApply(false, false, false),
                "Rake on a charge or against the exact foe held since the round began; other limbs untouched.");
            Assertions.True(ExpandedSummoningSpecialProfiles.IsHeldSinceRoundStart(true, 1) &&
                !ExpandedSummoningSpecialProfiles.IsHeldSinceRoundStart(true, 0) &&
                !ExpandedSummoningSpecialProfiles.IsHeldSinceRoundStart(false, 3),
                "Held since the round began means the held state has ticked at least once for this holder.");
            string components = Source("src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSpecialCombatComponents.cs");
            RequireTokens("Attack-identity component contract", components,
                "class SummonLimbs", "internal static SummonLimbKind Classify(",
                "class SummonGrappleLinks", "class SummonGrappleDamage",
                "class ExpandedSummoningRakeSequencePatch", "\"CreateFullAttack\"",
                "__result.RemoveAll(", "IsHeldSinceRoundStart(owner, target)",
                "evt.AutoMiss = true;", "evt.SuspendCombatLog = true;");
            string builder = Source("src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningSpecialBuilder.cs");
            RequireTokens("Cat builder contract", builder,
                "ConfigureGrabber(library, bySymbol, LeopardUnitSymbol, LeopardCombatTraitsSymbol",
                "ConfigureGrabber(library, bySymbol, LionUnitSymbol, LionCombatTraitsSymbol",
                "ConfigureGrabber(library, bySymbol, DireLionUnitSymbol, DireLionCombatTraitsSymbol",
                "ConfigureGrabber(library, bySymbol, TigerUnitSymbol, TigerCombatTraitsSymbol",
                "ConfigureGrabber(library, bySymbol, DireTigerUnitSymbol, DireTigerCombatTraitsSymbol");
            foreach (string key in new[] { "leopard", "lion", "dire-lion" })
                Assertions.True(ExpandedSummoningNaturalProfiles.For(key).Deviations.Any(value =>
                    value.Contains("grabs with its bite only")), "The bite-only grab must be recorded: " + key);
            foreach (string key in new[] { "tiger", "dire-tiger" })
                Assertions.True(ExpandedSummoningNaturalProfiles.For(key).Deviations.Any(value =>
                    value.Contains("bite and both foreclaws")), "The bite-and-foreclaw grab must be recorded: " + key);
            foreach (string key in new[] { "leopard", "lion", "dire-lion", "tiger", "dire-tiger" })
                Assertions.True(ExpandedSummoningNaturalProfiles.For(key).Deviations.Any(value =>
                    value.Contains("rake gate")), "The rake gate must be recorded: " + key);
        }

        internal static void GrabSizesAndSwallowLimitsAreUniversal()
        {
            ExpandedSummoningSpecialProfiles.Validate();
            Assertions.True(ExpandedSummoningSpecialProfiles.IsGrabSizeAllowed(4, 4, 0) &&
                ExpandedSummoningSpecialProfiles.IsGrabSizeAllowed(3, 4, 0) &&
                !ExpandedSummoningSpecialProfiles.IsGrabSizeAllowed(5, 4, 0) &&
                ExpandedSummoningSpecialProfiles.IsGrabSizeAllowed(5, 4, 1),
                "Grab works on the holder's size or smaller unless the stat block grants more.");
            Assertions.True(ExpandedSummoningSpecialProfiles.IsSwallowSizeAllowed(6, 7, false, 0, -1) &&
                !ExpandedSummoningSpecialProfiles.IsSwallowSizeAllowed(7, 7, false, 0, -1),
                "Swallow whole takes a foe one size smaller, never the swallower's own size.");
            Assertions.True(ExpandedSummoningSpecialProfiles.IsSwallowSizeAllowed(4, 6, true, 4, -1) &&
                !ExpandedSummoningSpecialProfiles.IsSwallowSizeAllowed(5, 6, true, 4, -1),
                "Engulf takes Medium or smaller whatever the flytrap's size.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldSwallowOnMaintain(true, true, 1, true) &&
                !ExpandedSummoningSpecialProfiles.ShouldSwallowOnMaintain(true, true, 0, true) &&
                !ExpandedSummoningSpecialProfiles.ShouldSwallowOnMaintain(true, false, 1, true) &&
                !ExpandedSummoningSpecialProfiles.ShouldSwallowOnMaintain(true, true, 1, false) &&
                !ExpandedSummoningSpecialProfiles.ShouldSwallowOnMaintain(false, true, 1, true),
                "A swallow follows a successful later-turn maintain check against an allowed size only.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(true, true, false, false, false, false, true) &&
                !ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(true, true, true, false, false, false, true) &&
                !ExpandedSummoningSpecialProfiles.ShouldAttemptSummonGrab(true, true, false, false, false, false, false),
                "A full holder or a busy limb, and a too-large target, refuse the grab.");
            Assertions.Equal(4, ExpandedSummoningSpecialProfiles.GiantFlytrapBiteCount, "Four bites, one link each.");
            Assertions.Equal(-1, ExpandedSummoningSpecialProfiles.PurpleWormSwallowSizeDelta, "The worm swallows one size smaller.");
            Assertions.Equal(4, ExpandedSummoningSpecialProfiles.SummonGrabManeuverBonus, "+4 on grapple checks from grab.");
            Assertions.Equal(5, ExpandedSummoningSpecialProfiles.SummonHoldMaintainBonus, "+5 more to maintain.");
            string components = Source("src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSpecialCombatComponents.cs");
            RequireTokens("Multi-link hold contract", components,
                "class SummonMultiHoldComponent", "class SummonHeldComponent", "bool limbBusy",
                "UnitHelper.TryBreakFree(", "internal static List<UnitEntityData> HeldTargets(",
                "SummonGrappleLinks.EstablishingWeapon(", "\"swallowed:\"", "class SummonSwallowLifecycleComponent");
        }

        internal static void MephitRolesAreProjectOwned()
        {
            ExpandedSummoningSpecialProfiles.Validate();
            Assertions.Equal("WindWall", ExpandedSummoningSpecialProfiles.MephitVariant("dust-mephit").SpellLikeTwo,
                "The dust mephit's wind wall is a project ability.");
            Assertions.Equal("ChillMetal", ExpandedSummoningSpecialProfiles.MephitVariant("ice-mephit").SpellLikeTwo,
                "The ice mephit's chill metal is a project ability.");
            Assertions.Equal("Pyrotechnics", ExpandedSummoningSpecialProfiles.MephitVariant("magma-mephit").SpellLikeOne,
                "The magma mephit's pyrotechnics is a project ability.");
            Assertions.Equal("MagmaForm", ExpandedSummoningSpecialProfiles.MephitVariant("magma-mephit").SpellLikeTwo,
                "The magma mephit's magma form is a project ability.");
            Assertions.True(ExpandedSummoningSpecialProfiles.WindWallOutcome(true, true, false) == SummonWindWallOutcome.Deflected &&
                ExpandedSummoningSpecialProfiles.WindWallOutcome(true, false, false) == SummonWindWallOutcome.MissChance &&
                ExpandedSummoningSpecialProfiles.WindWallOutcome(true, false, true) == SummonWindWallOutcome.None &&
                ExpandedSummoningSpecialProfiles.WindWallOutcome(false, true, false) == SummonWindWallOutcome.None,
                "Arrows and bolts are deflected, other ranged weapons roll a miss chance, rays and melee pass.");
            Assertions.Equal(30, ExpandedSummoningSpecialProfiles.WindWallOtherRangedMissChance, "The tabletop 30%.");
            Assertions.Equal(6, ExpandedSummoningSpecialProfiles.WindWallRounds, "One round per level at caster level 6.");
            Assertions.Equal("0,1,2,2,2,1,0", string.Join(",", Enumerable.Range(1, 7).Select(round =>
                ExpandedSummoningSpecialProfiles.ChillMetalDice(round).ToString()).ToArray()),
                "The chill metal table: none, 1d4, 2d4, 2d4, 2d4, 1d4, none.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ChillMetalMinimalDamage(3) == 2 &&
                ExpandedSummoningSpecialProfiles.ChillMetalMinimalDamage(2) == 1,
                "Minimal damage is 1 for 1d4 and 2 for 2d4.");
            Assertions.True(ExpandedSummoningSpecialProfiles.ChillMetalTier(true, false) == 2 &&
                ExpandedSummoningSpecialProfiles.ChillMetalTier(false, true) == 1 &&
                ExpandedSummoningSpecialProfiles.ChillMetalTier(false, false) == 0,
                "Metal armor takes the full table, a metal weapon only the minimal, no metal nothing.");
            Assertions.True(ExpandedSummoningSpecialProfiles.MagmaFormDamageReduction == 20 &&
                ExpandedSummoningSpecialProfiles.MagmaFormSpeedFeet == 10 &&
                ExpandedSummoningSpecialProfiles.MagmaFormRounds == 5 &&
                ExpandedSummoningSpecialProfiles.PyrotechnicsBlindDieSides == 4 &&
                ExpandedSummoningSpecialProfiles.PyrotechnicsBlindBonusRounds == 1 &&
                ExpandedSummoningSpecialProfiles.MephitSpellLikeCasterLevel == 6,
                "Magma form: DR 20/magic, speed 10, five rounds; pyrotechnics blinds 1d4+1 rounds; caster level 6.");
            string builder = Source("src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningSpecialBuilder.cs");
            RequireTokens("Mephit role builder contract", builder,
                "ConfigureWindWall(bySymbol, ability, prefix, token)",
                "ConfigureChillMetal(bySymbol, ability, prefix, token)",
                "ConfigurePyrotechnics(bySymbol, ability, prefix, token)",
                "ConfigureMagmaForm(bySymbol, ability, prefix, token, unit)",
                "MakeMephitCloudAllySafe(bySymbol, ability, prefix)", "MakeGlitterdustEnemyOnly(ability)",
                "UnitCondition.CanNotAttack", "ContextConditionIsAlly", "AbilityType.Supernatural",
                "IsProjectMephitAbility(", "MagmaFormAiStartCooldownRounds");
            string components = Source("src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSpecialCombatComponents.cs");
            RequireTokens("Mephit role component contract", components,
                "class SummonWindWallComponent", "ITargetRulebookHandler<RuleAttackRoll>",
                "evt.IncreaseMissChance(", "class SummonChillMetal", "class SummonChillMetalTargetChecker",
                "class SummonChillMetalComponent", "DamageEnergyType.Cold");
            string icons = Source("src", "KingmakerGunslinger", "Blueprints", "ExpandedSummoningIconBuilder.cs");
            RequireTokens("Icon builder", icons, "IsProjectMephitAbility(slot.Value)");
        }

        internal static void CyclopsWebAndHoovesAreExact()
        {
            ExpandedSummoningNaturalProfiles.Validate();
            ExpandedSummoningSpecialProfiles.Validate();
            NaturalSummonProfile cyclops = ExpandedSummoningNaturalProfiles.For("cyclops");
            Assertions.Equal(7, cyclops.NaturalArmor, "The Cyclops's natural armor is the stat block's +7.");
            Assertions.Equal(4, ExpandedSummoningSpecialProfiles.CyclopsHideArmorBonus, "+4 hide armor as a fact.");
            Assertions.True(cyclops.Deviations.Any(value => value.Contains("tabletop 19")),
                "The armor class 19 breakdown must be recorded.");
            Assertions.Equal(20, ExpandedSummoningSpecialProfiles.CyclopsFlashOfInsightChosenRoll,
                "Flash of Insight chooses a natural 20.");
            Assertions.Equal(1, ExpandedSummoningSpecialProfiles.GiantSpiderWebMaxSizeDelta,
                "A web catches a foe up to one size larger.");
            Assertions.True(ExpandedSummoningSpecialProfiles.IsWebTargetSizeAllowed(5, 4, 1) &&
                !ExpandedSummoningSpecialProfiles.IsWebTargetSizeAllowed(6, 4, 1), "The web size limit.");
            Assertions.True(ExpandedSummoningNaturalProfiles.For("giant-spider").Deviations.Any(value =>
                value.Contains("ranged touch attack")), "The web's touch path must be recorded.");
            foreach (string key in new[] { "pony", "horse" })
                Assertions.True(ExpandedSummoningNaturalProfiles.For(key).Deviations.Any(value =>
                    value.Contains("secondary attacks (Docile")), "The docile hooves must be recorded: " + key);
            string components = Source("src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningSpecialCombatComponents.cs");
            RequireTokens("Cyclops, web and hoof component contract", components,
                "class CyclopsFlashOfInsightComponent", "\"m_PreRolledResult\"", "ChosenResult = 20",
                "class SummonWebTargetSizeChecker", "class SummonDocileHoovesComponent",
                "hoof.ForceSecondary = secondary");
            string builder = Source("src", "KingmakerGunslinger", "Blueprints",
                "ExpandedSummoningSpecialBuilder.cs");
            RequireTokens("Cyclops, web and hoof builder contract", builder,
                "ConfigureCyclopsHideArmor(", "ModifierDescriptor.Armor",
                "deliver.NeedAttackRoll = true", "NativeRayWeaponGuid", "SummonWebTargetSizeChecker",
                "ConfigureDocileHooves(bySymbol, PonyUnitSymbol, PonyCombatTraitsSymbol, \"Pony\")",
                "ConfigureDocileHooves(bySymbol, HorseUnitSymbol, HorseCombatTraitsSymbol, \"Horse\")");
        }

        internal static void VisualVariantsOwnTheirResources()
        {
            string patch = Source("src", "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningVisualVariantPatch.cs");
            RequireTokens("Visual ownership contract", patch,
                "class SummonVisualOwnership", "[HarmonyPatch(typeof(UnitEntityView), \"OnDestroy\")]",
                "class ExpandedSummoningVisualTeardownPatch", "ownership.Materials.Add(material)",
                "ownership.Textures.Add(coat)", "ownership.Originals[renderer] = originals",
                "renderer.sharedMaterials = pair.Value", "internal static string ReleaseAll(",
                "internal static string ReleaseView(", "internal static string CountOwnedObjects(",
                "DestroyImmediate(", "\"attach-failed:\"", "\"attach-incomplete:\"",
                "FaultAfterRenderers");
            Assertions.False(patch.Contains("original.SetColor") || patch.Contains("sharedMaterial.SetColor"),
                "The shared donor material is never written; only the private clone is.");
        }

        internal static void LedgerAndScenariosAreWired()
        {
            string ledger = Source("blueprints", "blueprints.json");
            var entries = Newtonsoft.Json.Linq.JObject.Parse(ledger)["entries"]
                .Select(value => (string)value["symbol"]).ToArray();
            Assertions.Equal(AppendedLedgerIdentities, AppendedSymbols.Length,
                "The correction's identity list is complete.");
            Assertions.True(entries.Skip(entries.Length - AppendedLedgerIdentities)
                .SequenceEqual(AppendedSymbols),
                "The ledger is append-only: the correction identities sit at its tail in order.");
            var identities = ExpandedSummoningIdentityCatalog.Build();
            foreach (string symbol in AppendedSymbols)
                Assertions.Equal(1, identities.Count(value => value.Symbol == symbol),
                    "Catalog identity missing: " + symbol);
            string catalog = Source("src", "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestScenarioCatalog.cs");
            string harness = Source("scripts", "RuntimeAutomation.Common.ps1");
            string request = Source("src", "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRequest.cs");
            string runner = Source("src", "KingmakerGunslinger", "RuntimeTesting", "RuntimeTestRunner.cs");
            string project = Source("src", "KingmakerGunslinger", "KingmakerGunslinger.csproj");
            foreach (string name in new[] { "disposable-expanded-summoning-rules",
                "disposable-expanded-summoning-visual-lifecycle" })
            {
                Assertions.True(catalog.Contains("\"" + name + "\""), "Catalog constant missing: " + name);
                Assertions.True(harness.Contains("'" + name + "' = [pscustomobject]@{"), "Harness allowlist missing: " + name);
            }
            RequireTokens("Scenario wiring", request,
                "RuntimeTestScenarioCatalog.DisposableExpandedSummoningRules",
                "RuntimeTestScenarioCatalog.DisposableExpandedSummoningVisualLifecycle");
            RequireTokens("Runner dispatch", runner,
                "Complete(RunDisposableExpandedSummoningRules());", "PollExpandedSummoningVisualLifecycle();",
                "ArmExpandedSummoningPersistenceFlash(", "expanded-summoning-cyclops-flash-persistence");
            RequireTokens("Project file", project,
                "RuntimeTesting\\RuntimeTestRunner.ExpandedSummoningCorrection.cs");
        }
    }
}
