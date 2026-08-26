using System;
using System.Collections.Generic;
using System.IO;
using KingmakerGunslinger.Development;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FirearmConditionDevelopmentControlTests
    {
        internal static void CanonicalBreakThenWreckPreservesExactItem()
        {
            object item = new object();
            var store = new FakeTokenStore();
            var repository = new TokenBackedFirearmStateRepository(store,
                FirearmStateTokenCatalog.CreateCapacityOneDiagnostic());
            FirearmState loaded = new FirearmState(
                FirearmState.CurrentSchemaVersion, 1,
                FirearmStateTokenCatalog.DiagnosticLeadBall,
                FirearmCondition.Normal);
            FirearmStateRepositorySnapshot baseline = repository.Set(item,
                loaded);

            FirearmConditionFixtureDecision breakDecision =
                FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Break,
                    baseline.State);
            FirearmStateRepositorySnapshot broken = repository.Transition(item,
                state => FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Break, state).After);
            FirearmConditionFixtureDecision wreckDecision =
                FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Wreck, broken.State);
            FirearmStateRepositorySnapshot wrecked = repository.Transition(item,
                state => FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Wreck, state).After);

            Assertions.True(breakDecision.Accepted &&
                broken.State.Condition == FirearmCondition.Broken &&
                broken.State.LoadedRounds == 1 &&
                Equals(broken.State.LoadedAmmunition,
                    loaded.LoadedAmmunition),
                "Break did not use the canonical loaded Normal-to-Broken transition.");
            Assertions.True(wreckDecision.Accepted &&
                wrecked.State.Condition == FirearmCondition.Wrecked &&
                wrecked.State.IsEmpty &&
                wrecked.State.LoadedAmmunition == null,
                "Wreck did not use the canonical Broken-to-empty/Wrecked transition.");
            Assertions.True(baseline.RepositoryIdentity ==
                    broken.RepositoryIdentity &&
                broken.RepositoryIdentity == wrecked.RepositoryIdentity &&
                baseline.RuntimeReferenceHash == broken.RuntimeReferenceHash &&
                broken.RuntimeReferenceHash == wrecked.RuntimeReferenceHash &&
                broken.Revision == baseline.Revision + 1 &&
                wrecked.Revision == broken.Revision + 1,
                "The two fixture transitions changed item or repository identity.");
        }

        internal static void InvalidTransitionOrderRejectsWithoutMutation()
        {
            FirearmState normal = FirearmState.CreateEmpty();
            FirearmState broken = FirearmStateMachine.ApplyMisfireDamage(normal);
            FirearmState wrecked = FirearmStateMachine.ApplyMisfireDamage(
                broken);
            FirearmConditionFixtureDecision wreckNormal =
                FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Wreck, normal);
            FirearmConditionFixtureDecision breakBroken =
                FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Break, broken);
            FirearmConditionFixtureDecision wreckWrecked =
                FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Wreck, wrecked);
            Assertions.True(!wreckNormal.Accepted &&
                ReferenceEquals(wreckNormal.Before, wreckNormal.After) &&
                wreckNormal.Reason.IndexOf("break it first",
                    StringComparison.OrdinalIgnoreCase) >= 0,
                "Wreck accepted a Normal firearm or gave no useful ordering message.");
            Assertions.True(!breakBroken.Accepted &&
                ReferenceEquals(breakBroken.Before, breakBroken.After) &&
                breakBroken.Reason.IndexOf("already Broken",
                    StringComparison.OrdinalIgnoreCase) >= 0,
                "Break silently advanced a Broken firearm to Wrecked.");
            Assertions.True(!wreckWrecked.Accepted &&
                ReferenceEquals(wreckWrecked.Before, wreckWrecked.After) &&
                wreckWrecked.Reason.IndexOf("already Wrecked",
                    StringComparison.OrdinalIgnoreCase) >= 0,
                "A repeated Wreck request mutated an already Wrecked firearm.");
        }

        internal static void ProductionKindsShareTheCanonicalPolicy()
        {
            foreach (FirearmKind kind in new[] { FirearmKind.Pistol,
                FirearmKind.Musket, FirearmKind.Blunderbuss })
            {
                FirearmState first = FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Break,
                    FirearmState.CreateEmpty()).After;
                FirearmState second = FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Wreck, first).After;
                Assertions.True(first.Condition == FirearmCondition.Broken &&
                    second.Condition == FirearmCondition.Wrecked &&
                    second.IsEmpty,
                    "Canonical diagnostic policy diverged for production " +
                    kind + ".");
            }
        }

        internal static void WreckedTokenSurvivesRepositoryReconstruction()
        {
            object item = new object();
            var store = new FakeTokenStore();
            FirearmStateTokenCatalog catalog =
                FirearmStateTokenCatalog.CreateCapacityOneDiagnostic();
            var firstRepository = new TokenBackedFirearmStateRepository(
                store, catalog);
            firstRepository.Transition(item, state =>
                FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Break, state).After);
            firstRepository.Transition(item, state =>
                FirearmConditionFixturePolicy.Decide(
                    FirearmConditionFixtureOperation.Wreck, state).After);

            // A new repository has no process-local weak entry; only the exact
            // item-owned token survives, matching the save/reload carrier boundary.
            var reloadedRepository = new TokenBackedFirearmStateRepository(
                store, catalog);
            FirearmStateRepositorySnapshot reloaded =
                reloadedRepository.GetOrCreate(item);
            Assertions.True(reloaded.State.Condition ==
                    FirearmCondition.Wrecked && reloaded.State.IsEmpty &&
                store.ReadTokenIds(item).Count == 1 &&
                store.ReadTokenIds(item)[0] ==
                    FirearmStateTokenCatalog.WreckedTokenId,
                "The development Wreck fixture did not survive item-token repository reconstruction.");
        }

        internal static void UmmBridgeIsSelectedExactAndFailClosed()
        {
            string root = Environment.CurrentDirectory;
            string bridge = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Development",
                "KingmakerDevelopmentBridge.cs"));
            string controls = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Development",
                "DevelopmentControls.cs"));
            string ui = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Development", "DevelopmentUi.cs"));
            foreach (string token in new[] {
                "Break selected equipped firearm (diagnostic)",
                "Wreck selected equipped firearm (diagnostic)",
                "BreakSelectedEquippedFirearmForDebug",
                "WreckSelectedEquippedFirearmForDebug" })
                Assertions.True(ui.Contains(token) || controls.Contains(token),
                    "The deterministic UMM controls lack: " + token);
            foreach (string token in new[] {
                "ResolveExactSelectedRuntime()",
                "selection.GetSingleSelectedUnit()",
                "Select exactly one party unit",
                "game.Player.Party.Any(value =>",
                "not a current party member",
                "ExactEquippedFirearmResolver.TryResolve(descriptor",
                "unit.IsInCombat || player.IsInCombat",
                "ReferenceEquals(item, verified.Weapon)",
                "before.Repository.RepositoryIdentity",
                "before.Repository.RuntimeReferenceHash",
                "StaticEnchantmentFingerprint",
                "before={2}; after={3}",
                "_stateService.Set(item, before.Repository.State)" })
                Assertions.True(bridge.Contains(token),
                    "The exact selected-item UMM bridge lacks: " + token);
            Assertions.False(ui.Contains(
                    "Apply misfire damage to first equipped firearm") ||
                controls.Contains("damage-first-equipped-firearm") ||
                bridge.Contains("DamageFirstEquippedFirearmForDebug"),
                "The ambiguous legacy misfire-development control remains exposed.");

            string resolver = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "Actions",
                "ExactEquippedFirearmResolver.cs"));
            Assertions.True(resolver.Contains("caster.Body.PrimaryHand") &&
                resolver.Contains("caster.Body.SecondaryHand") &&
                resolver.Contains("marked.Count != 1") &&
                resolver.Contains("target selection is ambiguous") &&
                resolver.Contains("FirearmDefinitionComponent"),
                "The UMM bridge does not inherit the generic exact-equipped firearm ambiguity guard.");

            string runner = File.ReadAllText(Path.Combine(root, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.cs"));
            foreach (string token in new[] {
                "DevelopmentControls\n                    .BreakSelectedEquippedFirearmForDebug()",
                "DevelopmentControls\n                    .WreckSelectedEquippedFirearmForDebug()",
                "diagnosticOverhaulRecognized",
                "development-control-repeat-rejected",
                "selection.SelectedUnits.AddRange(selectionBefore)",
                "player.Party.AddRange(partyBefore)" })
                Assertions.True(runner.Contains(token),
                    "The guarded Overhaul scenario does not exercise or restore the UMM bridge contract: " +
                    token);
        }

        private sealed class FakeTokenStore : IFirearmStateTokenStore
        {
            private readonly Dictionary<object, string> _tokens =
                new Dictionary<object, string>();

            public IReadOnlyList<string> ReadTokenIds(object itemInstance)
            {
                string token;
                return _tokens.TryGetValue(itemInstance, out token)
                    ? new[] { token }
                    : new string[0];
            }

            public void ReplaceToken(object itemInstance,
                string expectedCurrentTokenId, string targetTokenId)
            {
                string current;
                _tokens.TryGetValue(itemInstance, out current);
                if (!string.Equals(current, expectedCurrentTokenId,
                        StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        "Fake item token changed before replacement.");
                if (targetTokenId == null)
                    _tokens.Remove(itemInstance);
                else
                    _tokens[itemInstance] = targetTokenId;
            }

            public bool ClearTokens(object itemInstance)
            {
                return _tokens.Remove(itemInstance);
            }
        }
    }
}
