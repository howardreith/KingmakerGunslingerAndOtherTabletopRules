using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint11Tests
    {
        internal const int UngulateIdentityCount = 100;
        internal const int PowerfulChargeIdentityCount = 2;
        internal const int TrampleIdentityCount = 3;
        internal const int AppendedLedgerIdentities =
            UngulateIdentityCount + PowerfulChargeIdentityCount +
            TrampleIdentityCount;

        internal static void FourUngulatesPublishAtPrintedTiers()
        {
            var expected = new[] {
                new { Key = "aurochs", Tier = 3, Hd = 3, Strength = 23,
                    Speed = 40, Armor = 4, Weapon = "Gore1d8" },
                new { Key = "bison", Tier = 4, Hd = 5, Strength = 27,
                    Speed = 40, Armor = 8, Weapon = "Gore2d6" },
                new { Key = "rhinoceros", Tier = 4, Hd = 5, Strength = 22,
                    Speed = 40, Armor = 7, Weapon = "Gore2d6" },
                new { Key = "woolly-rhinoceros", Tier = 5, Hd = 8,
                    Strength = 28, Speed = 30, Armor = 10, Weapon = "Gore2d8" }
            };
            var all = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            foreach (var value in expected)
            {
                SummonCreatureSpec creature = ExpandedSummoningCatalog.All
                    .Single(item => item.Key == value.Key);
                NaturalSummonProfile profile = ExpandedSummoningNaturalProfiles
                    .For(value.Key);
                Assertions.True(creature.MonsterTier == value.Tier &&
                    creature.NaturesAllyTier == value.Tier &&
                    creature.MonsterTemplated && profile.HitDieClass == "Animal" &&
                    profile.HitDice == value.Hd && profile.Size == "Large" &&
                    profile.Strength == value.Strength &&
                    profile.SpeedFeet == value.Speed &&
                    profile.NaturalArmor == value.Armor &&
                    profile.PrimaryWeapon == value.Weapon &&
                    profile.Facts.Contains("ReducedReach"),
                    "The published " + value.Key + " has its own printed animal profile.");
                var variants = all.Where(item => item.Creature.Key == value.Key)
                    .ToArray();
                int perFamily = 10 - value.Tier;
                Assertions.True(variants.Length == perFamily * 2 &&
                    variants.All(SummonVisibilityCatalog.IsPublished),
                    "Every placement of " + value.Key +
                    " publishes after mechanics, art and lifecycle qualification.");
            }
            // The loop above owns the claim this test exists for: every
            // placement of every Sprint 11 ungulate publishes. What is left to
            // say is that the surface stays consistent around them - the
            // published count is the registered one less whatever a later
            // sprint is still withholding - and that none of what is withheld
            // is theirs. Pinning the totals themselves only recorded whatever
            // the roster happened to be on the day, and the comment here still
            // described a barrier that no longer exists.
            Assertions.True(
                all.Length == SummonVisibilityCatalog
                    .RegisteredLogicalPlacementCount &&
                all.Count(SummonVisibilityCatalog.IsPublished) ==
                    SummonVisibilityCatalog.PublishedLogicalPlacementCount &&
                all.Count(value => !SummonVisibilityCatalog.IsPublished(value)) ==
                    SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                "Sprint 11 stays published alongside every later qualified sprint.");
        }

        internal static void FourUngulatesPublishQuantityPlacementsInBothFamilies()
        {
            string[] keys = {
                "aurochs", "bison", "rhinoceros", "woolly-rhinoceros"
            };
            var variants = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Concat(
                    ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly)).ToArray();
            foreach (string key in keys)
                foreach (SummonFamily family in new[] {
                    SummonFamily.Monster, SummonFamily.NaturesAlly
                })
                    foreach (SummonMultiplicity quantity in new[] {
                        SummonMultiplicity.OneD3,
                        SummonMultiplicity.OneD4PlusOne
                    })
                    {
                        var placements = variants.Where(value =>
                            value.Creature.Key == key &&
                            value.Family == family &&
                            value.Multiplicity == quantity).ToArray();
                        Assertions.True(placements.Length > 0 &&
                            placements.All(SummonVisibilityCatalog.IsPublished),
                            key + " must publish its " + family + "/" +
                            quantity + " quantity route after live qualification.");
                        if (quantity == SummonMultiplicity.OneD4PlusOne)
                        {
                            SummonCreatureSpec creature = ExpandedSummoningCatalog.All
                                .Single(value => value.Key == key);
                            int ownTier = family == SummonFamily.Monster ?
                                creature.MonsterTier.Value :
                                creature.NaturesAllyTier.Value;
                            Assertions.True(placements.Any(value =>
                                value.ParentTier == ownTier + 2),
                                key + " must offer a published own-tier-plus-two " +
                                family + " crowd review route.");
                        }
                    }
        }

        internal static void PublishedRhinosOwnDistinctPowerfulChargeFacts()
        {
            string[] symbols = {
                "KMG.Summoning.Special.Rhinoceros.PowerfulCharge",
                "KMG.Summoning.Special.WoollyRhinoceros.PowerfulCharge"
            };
            var identities = ExpandedSummoningIdentityCatalog.Build();
            Assertions.True(symbols.All(symbol => identities.Count(item =>
                item.Symbol == symbol && item.PlannedType ==
                    "BlueprintFeature") == 1),
                "Each published Rhino must own one distinct charge feature identity.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            Assertions.True(builder.Contains(
                    "ConfigureUngulatePowerfulCharge(bySymbol, RhinocerosUnitSymbol,") &&
                builder.Contains(
                    "ConfigureUngulatePowerfulCharge(bySymbol, WoollyRhinocerosUnitSymbol,") &&
                builder.Contains("UngulateRulesPolicy.For(creatureKey)") &&
                builder.Contains("charge.Gore = gore;") &&
                builder.Contains("charge.AdditionalDiceRolls = rules.ChargeDiceIncrement;") &&
                builder.Contains("charge.AdditionalDamageBonus = rules.ChargeBonusIncrement;") &&
                builder.Contains(".Concat(new BlueprintUnitFact[] { feature }).ToArray()"),
                "Both owned facts must use the printed per-species increment and exact primary gore.");
            string charge = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "UngulatePowerfulCharge.cs"));
            Assertions.True(charge.Contains("evt.DoNotScaleDamage = true;") &&
                charge.Contains("ReferenceEquals(evt.Weapon.Blueprint, Gore)"),
                "The exact gore's printed charge dice must bypass Kingmaker's second size scale.");
            string fixture = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.ExpandedSummoningCorrection.cs"));
            Assertions.True(fixture.Contains("Rulebook.Trigger(first);") &&
                fixture.Contains("realHit && realDamage > 0") &&
                fixture.Contains("fixture.Hostile.Descriptor.Damage = damageBefore;"),
                "The guarded charge fixture must land a real attack and restore its disposable target.");
            Assertions.True(fixture.Contains(
                    "BeginExpandedSummoningQueuedRhinoCharge();") &&
                fixture.Contains("value.AssetGuid == \"c78506dd0e14f7c45a599990e4e65038\"") &&
                fixture.Contains("_rulesChargeCommand = BeginExpandedSummoningDetachedAbility(") &&
                fixture.Contains("_rulesChargeRhino.View.MovementAgent.TickMovement(delta);") &&
                fixture.Contains("_rulesChargeRhino.Commands.Raw") &&
                fixture.Contains("_rulesChargeAttackCommand.Start();") &&
                fixture.Contains("_rulesChargeObserver.FirstChargeHitWithMarker") &&
                fixture.Contains("CleanupExpandedSummoningQueuedRhinoCharge();"),
                "The guarded command case must drive native travel, observe its real charged gore, and restore request-local state.");
        }

        internal static void PublishedTrampleAbilitiesUseNativePathWithSummonRules()
        {
            string[] keys = { "Aurochs", "Bison", "WoollyRhinoceros" };
            var identities = ExpandedSummoningIdentityCatalog.Build();
            Assertions.True(keys.All(key => identities.Count(item =>
                item.Symbol == "KMG.Summoning.Special." + key + ".Trample" &&
                item.PlannedType == "BlueprintAbility") == 1),
                "Only the three printed tramplers receive distinct published abilities.");
            Assertions.False(identities.Any(item => item.Symbol ==
                "KMG.Summoning.Special.Rhinoceros.Trample"),
                "Ordinary Rhinoceros must not gain an invented trample.");
            string builder = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Blueprints", "ExpandedSummoningSpecialBuilder.cs"));
            string action = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "Summoning", "ContextActionUngulateTrample.cs"));
            Assertions.True(builder.Contains("overrun.AutoSuccess = true;") &&
                builder.Contains("overrun.FirstTargetOnly = false;") &&
                builder.Contains("ability.SetIsFullRoundAction(true);") &&
                builder.Contains("UngulateTramplePathChecker") &&
                action.Contains("target.IsEnemy(caster)") &&
                action.Contains("ledger.TryClaim(round, target.UniqueId)") &&
                action.Contains("CanSpendAttackOfOpportunity(defender, trampler") &&
                action.Contains("TrySpendAttackOfOpportunity(defender, trampler") &&
                action.Contains("new RuleAttackWithWeapon(defender, trampler,") &&
                action.Contains("hand.Weapon, 4)") &&
                action.Contains("IsAttackOfOpportunity = true") &&
                action.Contains("trampler.HPLeft > 0") &&
                action.Contains("ledger.Halt(round)") &&
                action.Contains("SuppressDuplicateMovementOpportunityAttack") &&
                action.Contains(
                    "UngulateStampedeRuntime.ActiveGroupSize(caster)") &&
                !action.Contains("const int activeStampedeGroup = 0") &&
                action.Contains("SavingThrowType.Reflex") &&
                action.Contains("damage.Half = half;") &&
                action.Contains("ReferenceEquals(caster.Blueprint, SourceUnit)"),
                "Native multi-contact movement must use a full-round, speed-bound action with exact owner, enemy, size, per-round, exclusive automatic-AoO/Reflex, stopping and duplicate-suppression gates.");
            Assertions.True(builder.Contains("automatically makes") &&
                builder.Contains(
                    "one at -4 before damage and receives no save") &&
                builder.Contains("An attack that stops the trampler") &&
                builder.Contains("prevents that contact's damage"),
                "The published tooltip must disclose the Kingmaker automatic-AoO adaptation.");
            Assertions.True(builder.Contains(
                    "UngulateStampedeRuntime.Register(unit, ability)") &&
                builder.Contains("at least three allied") &&
                builder.Contains("same combat round") &&
                builder.Contains("remain mutually ") &&
                builder.Contains("adjacent. In real time") &&
                builder.Contains("Nearby idle creatures never count") &&
                action.Contains(
                    "HarmonyPatch(typeof(UnitUseAbility), \"OnAction\")") &&
                action.Contains(
                    "HarmonyPatch(typeof(UnitCombatState), \"LeaveCombat\")") &&
                action.Contains(
                    "state.Command.Result == UnitCommand.ResultType.Success") &&
                action.Contains("HasRunningCommand(candidate, expected)") &&
                action.Contains("actor.IsAlly(candidate)") &&
                action.Contains("candidate.IsAlly(actor)") &&
                action.Contains("StampedeFormationPolicy.QualifiedGroupSize"),
                "Stampede must use exact registered owners, actual command execution, native round cleanup, mutual allies and a rechecked adjacent trio rather than nearby quantity.");
            string fixture = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningCorrection.cs"));
            foreach (string token in new[] {
                "ExerciseExpandedSummoningTrampleResponseMatrix",
                "overrun.Actions.Run();",
                "trample-spent-reflex",
                "trample-unable-reflex",
                "trample-nonthreatening-reflex",
                "trample-rtwp-aoo-hit",
                "trample-player-defender",
                "trample-turn-based-miss",
                "trample-combat-reflexes-hit",
                "trample-lethal-aoo-stops",
                "trample-native-path-aoo",
                "ExerciseExpandedSummoningStampedeCommandMatrix",
                "BeginExpandedSummoningStampedeCommand",
                "PlaceExpandedSummoningStampedeFormation",
                "ExpandedSummoningStampedeFixtureAdjacent",
                "No mutually adjacent Stampede formation had three valid native Trample paths",
                "new[] { 5f, 6f, 8f, 4f, 3f, 2.5f }",
                "TraceAlongNavmesh(unit.Position, candidate.Point)",
                "nativeTarget = overrun.CanTarget(unit, candidate)",
                "pathTarget = path.CanTarget(unit, candidate)",
                "string mode = turnBased ? \"turn-based\" : \"rtwp\"",
                "\"-idle-quantity\"",
                "\"-two-commands\"",
                "\"-aurochs-active\"",
                "\"-bison-active\"",
                "\"-adjacency-loss\"",
                "\"-command-ended\"",
                "commands[2].Interrupt(true);",
                "UngulateStampedeRuntime.ActiveGroupSize",
                "target.Descriptor.State.Size = Size.Large",
                "SummonMultiplicity.OneD3",
                "HandlePartyCombatStateChanged(true)",
                "DescribeAttackOfOpportunityState(target, trampler)",
                "trampler.CombatState.PreventAttacksOfOpporunityNextFrame = false;",
                "unit.CombatState.JoinCombat();"
            })
                Assertions.True(fixture.Contains(token),
                    "The guarded live trample discrimination is missing " +
                    token + ".");
            int responseStart = fixture.IndexOf(
                "private static void PrepareExpandedSummoningTrampleResponsePair(",
                StringComparison.Ordinal);
            int pathStart = responseStart < 0 ? -1 : fixture.IndexOf(
                "private void BeginExpandedSummoningTramplePath()",
                responseStart, StringComparison.Ordinal);
            Assertions.True(responseStart >= 0 && pathStart > responseStart,
                "The guarded response-matrix source boundary must remain inspectable.");
            string responseFixture = fixture.Substring(responseStart,
                pathStart - responseStart);
            Assertions.True(responseFixture.Contains(
                    "!defender.IsEnemy(trampler) || !trampler.IsEnemy(defender)") &&
                responseFixture.Contains(
                    "CastExpandedSummoningQuietUnit(_rulesFixture, \"wolf\",") &&
                responseFixture.Contains("_rulesFixture.Hostile"),
                "The response matrix must use fresh native hostile summon groups and prove mutual hostility.");
            Assertions.False(responseFixture.Contains("SwitchFactions(") ||
                responseFixture.Contains("UpdateAttackFactionsCache()"),
                "The response matrix must not rewrite a shared summon group's faction cache and contaminate later live cases.");
            string inventory = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.cs"));
            int whitelist = inventory.IndexOf(
                "private static bool ExpandedSummoningIsApprovedUngulateDirectFact(",
                StringComparison.Ordinal);
            int forbidden = inventory.IndexOf(
                "private static bool ExpandedSummoningIsForbiddenReference(",
                StringComparison.Ordinal);
            Assertions.True(whitelist >= 0 && forbidden > whitelist &&
                inventory.Substring(whitelist, forbidden - whitelist)
                    .Contains("owner == \"8c4a8e045ca844a8bd42f614707b8e74\"") &&
                inventory.Contains("!ExpandedSummoningIsApprovedUngulateDirectFact(") &&
                inventory.Contains("granted == \"0f12c70e9b264ae484fb720d0c6845aa\""),
                "The live inventory must allow only exact owner-to-trample fact pairs, leaving unrelated donor references prohibited.");
        }

        internal static void UngulateDonorSurveyRecordsNativeMechanicGraphs()
        {
            string source = File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "src", "KingmakerGunslinger",
                "RuntimeTesting", "RuntimeTestRunner.ExpandedSummoningNativeDonors.cs"));
            foreach (string name in new[] { "aurochs", "bison", "rhinoceros",
                "woolly", "mastodon", "elephant", "trample",
                "powerfulcharge", "TrampleFeature", "PowerfulChargeFeature",
                "FlyTrampleTest", "OverrunAbility", "MammothTrample",
                "PowerfulChargeSharedStrengthBuff" })
                Assertions.True(source.Contains("\"" + name + "\""),
                    "Native survey must cover " + name + " by exact search term.");
            Assertions.True(source.Contains("DescribeGraph(matches[0], 0,") &&
                source.Contains("native-donor-audit.json") &&
                source.Contains("DescribeNativeUnit(unit)"),
                "Survey records component graphs and physical donor profiles.");
        }

        internal static void UngulateProfilesMatchPrintedRoles()
        {
            UngulateRulesPolicy.Validate();
            UngulateRulesProfile aurochs = UngulateRulesPolicy.For("aurochs");
            UngulateRulesProfile bison = UngulateRulesPolicy.For("bison");
            UngulateRulesProfile rhino = UngulateRulesPolicy.For("rhinoceros");
            UngulateRulesProfile woolly = UngulateRulesPolicy.For("woolly-rhinoceros");
            Assertions.True(aurochs.HitDice == 3 && aurochs.Strength == 23 &&
                aurochs.GoreDiceCount == 1 && aurochs.GoreDieSides == 8 &&
                aurochs.GoreBonus == 9 && aurochs.TrampleDiceCount == 2 &&
                aurochs.TrampleDieSides == 6 && aurochs.TrampleBonus == 9 &&
                aurochs.TrampleDc == 17 && aurochs.Stampede,
                "Aurochs has the printed gore, trample and stampede profile.");
            Assertions.True(bison.HitDice == 5 && bison.Strength == 27 &&
                bison.GoreDiceCount == 2 && bison.GoreDieSides == 6 &&
                bison.GoreBonus == 12 && bison.TrampleBonus == 12 &&
                bison.TrampleDc == 20 && bison.Stampede,
                "Bison is the heavier printed herd profile.");
            Assertions.True(rhino.HitDice == 5 && rhino.Strength == 22 &&
                rhino.GoreBonus == 9 && !rhino.HasTrample &&
                rhino.ChargeDiceCount == 4 && rhino.ChargeDieSides == 6 &&
                rhino.ChargeBonus == 12 && rhino.ChargeDiceIncrement == 2 &&
                rhino.ChargeBonusIncrement == 3,
                "Ordinary Rhinoceros has charge but no invented trample.");
            Assertions.True(woolly.HitDice == 8 && woolly.Strength == 28 &&
                woolly.GoreDiceCount == 2 && woolly.GoreDieSides == 8 &&
                woolly.GoreBonus == 13 && woolly.ChargeDiceCount == 4 &&
                woolly.ChargeDieSides == 8 && woolly.ChargeBonus == 18 &&
                woolly.ChargeDiceIncrement == 2 &&
                woolly.ChargeBonusIncrement == 5 &&
                woolly.TrampleDiceCount == 2 && woolly.TrampleBonus == 13 &&
                woolly.TrampleDc == 23 && !woolly.Stampede,
                "Woolly Rhinoceros has its distinct charge and trample profile.");
            Assertions.True(rhino.AppliesPowerfulCharge(true, true, true, false),
                "A first gore charge uses the stronger stat-block damage.");
            Assertions.False(rhino.AppliesPowerfulCharge(true, false, true, false) ||
                rhino.AppliesPowerfulCharge(false, true, true, false) ||
                rhino.AppliesPowerfulCharge(true, true, false, false) ||
                rhino.AppliesPowerfulCharge(true, true, true, true) ||
                bison.AppliesPowerfulCharge(true, true, true, false),
                "Powerful charge never leaks to ordinary or unrelated attacks.");
        }

        internal static void TrampleTargetsAndRoundsAreBounded()
        {
            UngulateRulesProfile aurochs = UngulateRulesPolicy.For("aurochs");
            UngulateRulesProfile rhino = UngulateRulesPolicy.For("rhinoceros");
            Assertions.True(aurochs.CanTrample(4, 3, 1),
                "A Large aurochs can trample a Medium creature.");
            Assertions.False(aurochs.CanTrample(4, 4, 2),
                "A pair does not qualify for same-size stampede.");
            Assertions.True(aurochs.CanTrample(4, 4, 3) &&
                aurochs.TrampleSaveDc(3) == 19,
                "Three adjacent stampeding herd animals may trample Large foes at DC 19.");
            Assertions.False(aurochs.CanTrample(4, 5, 3) ||
                rhino.CanTrample(4, 3, 0),
                "Stampede does not reach larger creatures or grant Rhino trample.");
            string[] formation = { "actor", "left", "right", "idle" };
            Func<string, string, bool> allAdjacent = (left, right) =>
                !string.Equals(left, right, StringComparison.Ordinal);
            Assertions.Equal(3, StampedeFormationPolicy.QualifiedGroupSize(
                "actor", formation,
                value => value != "idle", allAdjacent),
                "Three independently active eligible allies form Stampede; an idle nearby body is irrelevant.");
            Assertions.Equal(0, StampedeFormationPolicy.QualifiedGroupSize(
                "actor", formation,
                value => value == "actor" || value == "left", allAdjacent),
                "Two active tramplers cannot receive Stampede from nearby quantity.");
            Assertions.Equal(0, StampedeFormationPolicy.QualifiedGroupSize(
                "actor", formation, value => value != "idle",
                (left, right) => allAdjacent(left, right) &&
                    !(left == "left" && right == "right") &&
                    !(left == "right" && right == "left")),
                "All three active members must remain mutually adjacent.");
            Assertions.Equal(0, StampedeFormationPolicy.QualifiedGroupSize(
                "actor", formation, value => value != "actor", allAdjacent),
                "An acting creature that is not itself executing Stampede cannot borrow the group benefit.");
            Assertions.True(TrampleTargetResponsePolicy
                    .HasLegalOpportunityAttack(true, true, true, true, true),
                "A fully legal contacted defender takes the automatic AoO branch.");
            foreach (bool[] unavailable in new[] {
                new[] { false, true, true, true, true },
                new[] { true, false, true, true, true },
                new[] { true, true, false, true, true },
                new[] { true, true, true, false, true },
                new[] { true, true, true, true, false }
            })
                Assertions.False(TrampleTargetResponsePolicy
                        .HasLegalOpportunityAttack(unavailable[0],
                            unavailable[1], unavailable[2], unavailable[3],
                            unavailable[4]),
                    "Any unavailable, unable, invalid, nonthreatening or native-forbidden response uses Reflex.");
            Assertions.True(TrampleTargetResponsePolicy.Resolve(true, true,
                    true) == TrampleTargetResponseDecision
                        .OpportunityAttackContinues &&
                TrampleTargetResponsePolicy.Resolve(true, true, false) ==
                    TrampleTargetResponseDecision.OpportunityAttackStops &&
                TrampleTargetResponsePolicy.Resolve(true, false, true) ==
                    TrampleTargetResponseDecision.ReflexSave &&
                TrampleTargetResponsePolicy.Resolve(false, false, true) ==
                    TrampleTargetResponseDecision.ReflexSave,
                "The prequalified response is exclusive: executed AoOs suppress Reflex, failed execution falls back, and stopping prevents contact damage.");
            TrampleRoundLedger ledger = new TrampleRoundLedger();
            Assertions.True(ledger.TryClaim(10, "target-a"),
                "First contact is eligible.");
            Assertions.False(ledger.TryClaim(10, "target-a"),
                "A path replay cannot damage the same target twice in a round.");
            Assertions.True(ledger.TryClaim(10, "target-b") &&
                ledger.TryClaim(11, "target-a"),
                "Another foe and a later round are eligible.");
            Assertions.False(ledger.TryClaim(10, "target-c") ||
                ledger.TryClaim(11, ""),
                "Time regression or missing target identity fails closed.");
            Assertions.True(ledger.HasClaim(11, "target-a"),
                "The exact claimed pair can suppress only its duplicate movement AoO.");
            ledger.Halt(11);
            Assertions.True(ledger.IsHalted(11) &&
                    !ledger.TryClaim(11, "target-c"),
                "A stopping AoO prevents every later contact in that round.");
            Assertions.True(ledger.TryClaim(12, "target-c"),
                "A later combat round clears the stopping marker.");
            ledger.Clear();
            Assertions.True(ledger.TryClaim(10, "target-a"),
                "Cleanup releases the request-local contact ledger.");
        }

        internal static void DonorRigCaptureUsesPrivateBindFrame()
        {
            string root = Environment.CurrentDirectory;
            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningCorrection.cs"));
            string project = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "KingmakerGunslinger.csproj"));
            Assertions.True(runner.Contains("CaptureExpandedSummoningUngulateDonorRig(_rulesTrampler,") &&
                runner.Contains("_rulesTrampleIndex == 0 ? \"horse\" : \"mastodon\"") &&
                runner.Contains("Matrix4x4 bind = poses[index].inverse;") &&
                runner.Contains("Path.Combine(_request.EvidenceDirectory, fileName)"),
                "Original-mesh authoring must measure both exact live donor bind frames in guarded local evidence.");
            Assertions.False(project.Contains("sprint11-horse-bind-rig.json") ||
                project.Contains("sprint11-mastodon-bind-rig.json"),
                "Measured native donor transforms must never enter the package.");
        }

        internal static void OriginalUngulateVisualsUseNativeBindFramesAndPackage()
        {
            string root = Environment.CurrentDirectory;
            string directory = Path.Combine(root, "assets", "ungulates");
            string[] kinds = { "aurochs", "bison", "rhinoceros",
                "woolly-rhinoceros" };
            string[] meshHashes = {
                "477354af79aefb21de544517c498a05b2d420fbb5ebbb1cb675613f66f09196e",
                "bd042f973a2a871283dea51614cbce56736f2d9d7dbc65fe6f3874ab4a193d56",
                "aa7e069491034cf8281ea19cfd228ac159e0ad2b778e14f10f1441663014e9ca",
                "45b3003b0184049921982d828faf71e95b90c2babf5acc0827f0095ec41aac52"
            };
            for (int item = 0; item < kinds.Length; item++)
            {
                string kind = kinds[item];
                string meshPath = Path.Combine(directory, kind + "-mesh.json");
                using (var sha = SHA256.Create())
                {
                    string actual = string.Concat(sha.ComputeHash(
                        File.ReadAllBytes(meshPath))
                        .Select(value => value.ToString("x2")));
                    Assertions.Equal(meshHashes[item], actual,
                        kind + " mesh bytes match the LF-normalized reviewed export.");
                }
                JObject mesh = JObject.Parse(File.ReadAllText(Path.Combine(
                    directory, kind + "-mesh.json")));
                Assertions.Equal(2, (int)mesh["schemaVersion"],
                    kind + " uses the shared skinned-mesh schema.");
                string[] bones = ((JArray)mesh["bones"])
                    .Select(value => (string)value).ToArray();
                Assertions.True(bones.Length >= 25 && bones.Length <= 30 &&
                    bones.Distinct(StringComparer.Ordinal).Count() == bones.Length &&
                    bones.Contains("Head") && bones.Contains("LowerTorso"),
                    kind + " binds only an unambiguous measured native rig.");
                Assertions.True(((string)mesh["space"]).Contains("donor renderer local") &&
                    ((string)mesh["rigSha256"]).Length == 64,
                    kind + " records its captured frame without shipping transforms.");
                int vertices = (int)mesh["vertexCount"];
                int triangles = (int)mesh["triangleCount"];
                Assertions.True(vertices >= 500 && triangles >= 390 &&
                    Convert.FromBase64String((string)mesh["data"]).Length ==
                    vertices * 64 + triangles * 12,
                    kind + " carries complete original geometry and weights.");
                JObject albedo = (JObject)mesh["albedo"];
                Assertions.Equal(kind + "-albedo.png", (string)albedo["file"],
                    kind + " names its own painting.");
                using (var sha = SHA256.Create())
                {
                    string actual = string.Concat(sha.ComputeHash(File.ReadAllBytes(
                        Path.Combine(directory, (string)albedo["file"])))
                        .Select(value => value.ToString("x2")));
                    Assertions.Equal((string)albedo["sha256"], actual,
                        kind + " painting matches its geometry manifest.");
                }
            }
            string source = Path.Combine(root, "assets-source", "original-models",
                "ungulates");
            Assertions.True(File.Exists(Path.Combine(source, "generate_ungulates.py")) &&
                File.Exists(Path.Combine(source, "paint_ungulate_albedo.py")),
                "Original editable source and reproducible exporters remain available.");
            string generator = File.ReadAllText(Path.Combine(source,
                "generate_ungulates.py"));
            Assertions.True(generator.Contains("def elliptical_tube(") &&
                generator.Contains("cap_start=True, cap_end=True") &&
                generator.Contains("def ellipsoid(") &&
                generator.Contains("def hoof(") &&
                generator.Contains("def articulated_leg(") &&
                generator.Contains("No face crosses a donor pivot") &&
                generator.Contains("widths[index] <= 0.0 and heights[index] <= 0.0") &&
                generator.Contains("if len(rings[index]) == 1") &&
                generator.Contains("if len(rings[index + 1]) == 1") &&
                generator.Contains("spans = ((0, 1, 0), (1, 2, 1), (2, 4, 2))") &&
                generator.Contains("overlap = min(length * 0.28") &&
                generator.Contains("min(radii[start_index], radii[end_index]) * 0.84") &&
                generator.Contains("if span_index > 0 else points[start_index]") &&
                generator.Contains("end = points[end_index] + along * overlap") &&
                generator.Contains("start_tip = 0.0 if span_index > 0 else start_radius * 0.90") &&
                generator.Contains("end_radius * 0.84, end_radius * 0.72, 0.0") &&
                generator.Contains("bones[bone_index], \"limbs\", segments)") &&
                generator.Contains("points[-1], names[-1]") &&
                generator.Contains("Vector((0, 0, 1)), 0.25 if bison else 0.22, 2") &&
                generator.Contains("Vector((0, 0, -1)), 0.52 if woolly else 0.48, 3") &&
                generator.Contains("Vector((0.72 * scale, 0.48 * scale, 0.82 * scale))") &&
                !generator.Contains("profile = (0.0, 0.32, 0.72, 0.90, 1.0)") &&
                !generator.Contains("ring_bones") &&
                !generator.Contains("segments, False, False") &&
                !generator.Contains("def aligned_ellipsoid(") &&
                !generator.Contains("body_drop = Vector((0, -0.75, 0))") &&
                !generator.Contains("Vector((0, -0.12, 0.07))") &&
                !generator.Contains("Vector((0, -0.06, -0.22))"),
                "The deterministic source must retain broad hoof profiles and three overlapping, independently controlled Rhinoceros spindle spans rather than the reviewed thin foot fans, stretched cross-pivot faces, open or broad-cap lower-joint pieces, stacked ankle barrels, displaced geometry, or rigid upper-control columns.");
            string loader = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Assets", "PteranodonAssetRuntime.cs"));
            string view = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Summoning",
                "ExpandedSummoningPteranodonViewPatch.cs"));
            Assertions.True(loader.Contains("ConfigureUngulates(context)") &&
                loader.Contains("AllowedHorseBones") &&
                loader.Contains("AllowedMastodonBones") &&
                loader.Contains("TryGetUngulateVisual") &&
                view.Contains("UngulateKeys.Contains(attachment.VisualKey)") &&
                view.Contains("TryResolveDonorBinding(donor, boneNames") &&
                view.Contains("Revert(attachment)") &&
                view.Contains("!UngulateKeys.Contains(attachment.VisualKey)") &&
                view.Contains("DestroyImmediate(attachment.Mesh)") &&
                view.Contains("DestroyImmediate(material)"),
                "Four original visuals use the validated native-bind instance swap and fallback.");
            string build = File.ReadAllText(Path.Combine(root, "scripts",
                "Build-Local.ps1"));
            string package = File.ReadAllText(Path.Combine(root, "scripts",
                "package.ps1"));
            Assertions.True(build.Contains("assets\\ungulates") &&
                package.Contains("assets\\ungulates") &&
                build.Contains("{ 347 } else { 345 }") &&
                package.Contains("{ 347 } else { 345 }"),
                "All eight ungulate asset files enter the strict standalone package.");
        }

        internal static void PublishedUngulatesUseGuardedCreatureViewReview()
        {
            string root = Environment.CurrentDirectory;
            string review = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningCreatureReview.cs"));
            string motion = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.PteranodonReview.cs"));
            // The Sprint 11 and Sprint 10 review hatches are gone, and the
            // publication guard is still the gate. The guard's exact line
            // gains a clause for each sprint whose own creatures are being
            // reviewed before they publish, so the pin names the guard and
            // its refusal rather than one sprint's spelling of the condition.
            Assertions.True(!review.Contains("suppressedSprint11Candidate") &&
                review.Contains("!SummonVisibilityCatalog.IsPublished(variant)") &&
                review.Contains(
                    "A suppressed creature cannot be reviewed through a parent") &&
                !review.Contains("suppressedSprint10Candidate") &&
                review.Contains("key == \"aurochs\" || key == \"bison\"") &&
                review.Contains("key == \"rhinoceros\" || key == \"woolly-rhinoceros\"") &&
                review.Contains("IsOriginalReviewKey(key)") &&
                review.Contains("OriginalReviewVisualName(key)") &&
                review.Contains("expanded-summoning-original-view-") &&
                review.Contains("Resources.FindObjectsOfTypeAll<Mesh>()") &&
                review.Contains(".FindObjectsOfTypeAll<Material>()") &&
                review.Contains("expanded-summoning-ungulate-travel-") &&
                review.Contains("MotionReviewTravelValid") &&
                motion.Contains("IsGuidedMotionReview(unit)") &&
                motion.Contains("PrepareSprint9FlightMovement(unit)") &&
                motion.Contains("unit.View.MovementAgent.TickMovement(delta)"),
                "The four published Sprint 11 units use the ordinary publication guard and request-local visual review with exact attached-mesh and resource-cleanup checks.");
            float rhino, woolly, unused;
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_Rhinoceros", out rhino) &&
                SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_WoollyRhinoceros", out woolly) &&
                rhino == 0.55f && woolly == 0.60f &&
                !SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_Pony", out unused) &&
                !SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_Horse", out unused),
                "The Huge donor is reduced only for the two Large original rhinoceroses; Pony and Horse remain native controls.");
        }
    }
}
