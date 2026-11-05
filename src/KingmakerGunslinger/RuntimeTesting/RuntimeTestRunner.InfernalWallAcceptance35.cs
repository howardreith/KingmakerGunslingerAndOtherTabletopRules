using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Read-only exact-section damage record, including what the native rule
        // actually calculated, so object halving and fire are never inferred.
        private sealed class InfernalWallDamageObservation35 : IDisposable,
            IGlobalRulebookHandler<RuleDealDamage>
        {
            private readonly UnitEntityData _section;
            private readonly Dictionary<RuleDealDamage, JObject> _open =
                new Dictionary<RuleDealDamage, JObject>();
            internal readonly JArray Rows = new JArray();

            internal InfernalWallDamageObservation35(UnitEntityData section)
            { _section = section; EventBus.Subscribe(this); }

            public void OnEventAboutToTrigger(RuleDealDamage evt)
            {
                if (!ReferenceEquals(evt.Target, _section)) return;
                var row = new JObject { ["initiator"] = evt.Initiator?.UniqueId,
                    ["target"] = _section.UniqueId, ["before"] = _section.Damage,
                    ["hitPoints"] = _section.Stats.HitPoints.ModifiedValue,
                    ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds };
                _open[evt] = row; Rows.Add(row);
            }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                JObject row;
                if (!_open.TryGetValue(evt, out row)) return;
                row["after"] = _section.Damage; row["ruleDamage"] = evt.Damage;
                row["weapon"] = evt.DamageBundle.Weapon?.Blueprint?.AssetGuid;
                row["hasAttackRoll"] = evt.AttackRoll != null;
                row["sectionIsPlayersEnemy"] = _section.IsPlayersEnemy;
                row["dead"] = _section.Descriptor.State.IsDead;
                row["packets"] = new JArray((evt.ResultDamage ?? new List<DamageValue>()).Select(value =>
                    new JObject { ["type"] = value.Source?.GetType().Name,
                        ["energy"] = (value.Source as EnergyDamage)?.EnergyType.ToString(),
                        ["half"] = value.Source != null && value.Source.Half,
                        ["durability"] = value.Source == null ? (float?)null : value.Source.Durability,
                        ["dice"] = value.Source?.Dice.ToString(), ["bonus"] = value.Source?.Bonus,
                        ["rolled"] = value.RolledValue, ["withoutReduction"] = value.ValueWithoutReduction,
                        ["final"] = value.FinalValue }));
            }

            public void Dispose() { EventBus.Unsubscribe(this); }
        }

        private sealed class InfernalWallRun35
        {
            internal ExpandedSummoningCorrectionFixture Fixture;
            internal UnitEntityData Bone, Attacker;
            internal List<UnitEntityData> Owned;
            internal InfernalEvents34 Trace;
            internal Action<string, bool, JObject, string> Check;
            internal AbilityData Data;
            internal BlueprintAbility Wall;
            internal BlueprintUnit Section;
            internal BlueprintAbilityAreaEffect Residual, Fog;
            internal BlueprintBuff Frightened;
            internal readonly JObject Receipt = new JObject();
        }

        private sealed class InfernalWallFormed35
        {
            internal string Label;
            internal InfernalWallFixturePlan35 Plan;
            internal UnitEntityData Section;
            internal JObject Creation;
            internal InfernalWallNavmesh35 Binding;
            internal Pathfinding.NavmeshCut Cut;
            internal KeyValuePair<Pathfinding.GraphNode, bool>[] Nodes;
            internal JObject PathBefore;
            internal int NodesChanged;
            internal long LifetimeEndTicks;
        }

        private void InfernalWallPersist35(InfernalWallRun35 run, string name, JToken row)
        {
            run.Receipt[name] = row.DeepClone();
            RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                "s35-wall-acceptance.json"), run.Receipt.ToString(Formatting.Indented));
        }

        // Each retirement outcome owns a section on its own lane, so one area's
        // residual cold or steam cannot be credited to another section.
        private IEnumerable<int> InfernalWallAcceptance35(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData bone,
            List<UnitEntityData> owned, InfernalEvents34 trace,
            Action<string, bool, JObject, string> check,
            InfernalWallFixturePlan35 physicalPlan)
        {
            var wall = fixture.Blueprints.OfType<BlueprintAbility>().Single(value =>
                value.name == "KMG_Summoning_Infernal_BoneDevil_WallOfIce");
            var grant = bone.Descriptor.Abilities.GetAbility(wall);
            if (grant == null) throw new InvalidOperationException("Hidden Bone Devil lacks its registered wall grant.");
            var run = new InfernalWallRun35 {
                Fixture = fixture, Bone = bone, Owned = owned, Trace = trace, Check = check,
                Wall = wall, Data = new AbilityData(grant),
                Section = fixture.Blueprints.OfType<BlueprintUnit>().Single(value =>
                    value.name == "KMG_Summoning_Infernal_BoneDevil_WallSectionUnit"),
                Residual = fixture.Blueprints.OfType<BlueprintAbilityAreaEffect>().Single(value =>
                    value.name == "KMG_Summoning_Infernal_BoneDevil_WallResidualArea"),
                Fog = fixture.Blueprints.OfType<BlueprintAbilityAreaEffect>().Single(value =>
                    value.AssetGuid == "e09010a73354a794293ebc7b33c2d130"),
                Frightened = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.Bone35Prefix + "FearFrightened"],
                Attacker = owned.FirstOrDefault(value => value != null && !value.Destroyed &&
                    value.IsInGame && value.IsInState && !value.Descriptor.State.IsDead &&
                    InfernalMartialProfile34.For(value)?.Glaive != null)
            };
            if (run.Attacker == null) throw new InvalidOperationException("No exact Glaive-armed physical attack witness.");
            InfernalWallPersist35(run, "scope", new JObject {
                ["phase"] = RuntimeTestRequestParser.Sprint32ReviewPhase(_request),
                ["controlledSetup"] = new JArray(
                    "Disposable save-free witnesses are placed through native Translocate before, never during, a measured movement or attack.",
                    "Formation feedback raises one witness's Reflex or spell resistance so each disruption branch is exercised; these never touch a section outcome.",
                    "Fire damage is a controlled native RuleDealDamage fire packet; no hit points are written directly.",
                    "Expiry uses the established controlled Player.GameTime seam at the original deadline; it is not a twelve-minute real-time soak."),
                ["forcedHits"] = 0, ["losOverride"] = false, ["reachOverride"] = false });

            foreach (int frame in InfernalWallFeedback35(run, physicalPlan)) yield return frame;

            // Physical destruction on the accepted lane.
            var physical = new InfernalWallFormed35 { Label = "physical", Plan = physicalPlan };
            foreach (int frame in InfernalWallCast35(run, physical)) yield return frame;
            foreach (int frame in InfernalWallLiveMover35(run, physical)) yield return frame;
            var physicalArea = new List<AreaEffectEntityData>();
            foreach (int frame in InfernalWallPhysicalDestruction35(run, physical, physicalArea)) yield return frame;
            foreach (int frame in InfernalWallRestored35(run, physical, physicalArea.FirstOrDefault(), null)) yield return frame;

            // Fire destruction, steam and residual on a second clear lane.
            var firePlan = InfernalWallNextLane35(run, "fire", new[] { physicalPlan });
            foreach (int frame in InfernalWallFixturePrerequisite35(fixture, bone, owned, firePlan, check, "fire")) yield return frame;
            var fire = new InfernalWallFormed35 { Label = "fire", Plan = firePlan };
            foreach (int frame in InfernalWallCast35(run, fire)) yield return frame;
            foreach (int frame in InfernalWallLiveMover35(run, fire)) yield return frame;
            var fireAreas = new List<AreaEffectEntityData>();
            foreach (int frame in InfernalWallFireDestruction35(run, fire, fireAreas)) yield return frame;
            foreach (int frame in InfernalWallRestored35(run, fire, fireAreas.FirstOrDefault(value =>
                ReferenceEquals(value.Blueprint, run.Residual)), fireAreas.FirstOrDefault(value =>
                ReferenceEquals(value.Blueprint, run.Fog)))) yield return frame;
            foreach (int frame in InfernalWallSteamExit35(run, fire, physicalPlan)) yield return frame;

            // Original-deadline controlled expiry on a third clear lane.
            var expiryPlan = InfernalWallNextLane35(run, "expiry", new[] { physicalPlan, firePlan });
            foreach (int frame in InfernalWallFixturePrerequisite35(fixture, bone, owned, expiryPlan, check, "expiry")) yield return frame;
            var expiry = new InfernalWallFormed35 { Label = "expiry", Plan = expiryPlan };
            foreach (int frame in InfernalWallCast35(run, expiry)) yield return frame;
            foreach (int frame in InfernalWallLiveMover35(run, expiry)) yield return frame;
            foreach (int frame in InfernalWallControlledExpiry35(run, expiry)) yield return frame;
            foreach (int frame in InfernalWallRestored35(run, expiry, null, null)) yield return frame;
        }

        private InfernalWallFixturePlan35 InfernalWallNextLane35(InfernalWallRun35 run,
            string label, InfernalWallFixturePlan35[] earlier)
        {
            // Stay outside earlier breach areas (steam reaches 20 feet) and the
            // parked hostile, so this lane's route measures only its own section.
            var avoid = earlier.Select(plan => new KeyValuePair<Vector3, float>(plan.Midpoint, 7f)).ToList();
            avoid.Add(new KeyValuePair<Vector3, float>(run.Fixture.Hostile.Position, 3f));
            Vector3 start, midpoint, destination, casterPoint; JArray attempts; JObject survey;
            if (!InfernalEarlyWallLane35(run.Fixture, run.Owned.Where(value => value != null && !value.Destroyed &&
                    value.IsInState && !value.Descriptor.State.IsDead &&
                    value.Blueprint?.AssetGuid != InfernalRules35.WallSectionUnitGuid).ToList(),
                    out start, out midpoint, out destination, out casterPoint, out attempts, out survey,
                    avoid, "s35-wall-native-lane-survey-" + label + ".json"))
                throw new InvalidOperationException("No separate native lane for the " + label + " Wall section: " + attempts.ToString(Formatting.None));
            return new InfernalWallFixturePlan35(start, destination, midpoint, casterPoint, survey);
        }

        private JObject InfernalWallFeedbackState35(string caseName, long attempts, long published, long faults)
        {
            return new JObject { ["case"] = caseName,
                ["attemptsDelta"] = NativeCombatLog.Attempts - attempts,
                ["publishedDelta"] = NativeCombatLog.Published - published,
                ["faultsDelta"] = NativeCombatLog.Faults - faults,
                ["lastMessage"] = NativeCombatLog.LastMessage,
                ["nativeLogViewPresent"] = Game.Instance.UI?.BattleLogManager?.LogView != null };
        }

        private IEnumerable<int> InfernalWallFeedback35(InfernalWallRun35 run, InfernalWallFixturePlan35 plan)
        {
            var fixture = run.Fixture; var bone = run.Bone;
            var witness = fixture.Hostile;
            Vector3 witnessHome = witness.Position;
            var target = new TargetWrapper(plan.Midpoint);
            var summoner = bone.Get<UnitPartSummonedMonster>()?.Summoner;
            var cases = new JArray();
            var row = new JObject { ["casterIsPlayerFaction"] = bone.IsPlayerFaction,
                ["casterFaction"] = bone.Descriptor.Faction?.name,
                ["summoner"] = summoner?.UniqueId,
                ["summonerIsPlayerFaction"] = summoner != null && summoner.IsPlayerFaction,
                ["cases"] = cases };
            InfernalWallPersist35(run, "feedback", row);

            // Repeated validity polling is query-only on both outcomes.
            long attempts = NativeCombatLog.Attempts, published = NativeCombatLog.Published, faults = NativeCombatLog.Faults;
            string message = NativeCombatLog.LastMessage;
            int clear = 0, occupied = 0;
            for (int poll = 0; poll < 240; poll++) if (run.Data.CanTarget(target)) clear++;
            PlaceExpandedSummoningUnit(witness, plan.Midpoint);
            yield return 0;
            for (int poll = 0; poll < 240; poll++) if (run.Data.CanTarget(target)) occupied++;
            var polling = InfernalWallFeedbackState35("target-polling", attempts, published, faults);
            polling["validPollsAccepted"] = clear; polling["occupiedPollsAccepted"] = occupied;
            polling["pollsEach"] = 240; polling["messageUnchanged"] = NativeCombatLog.LastMessage == message;
            cases.Add(polling); InfernalWallPersist35(run, "feedback", row);
            run.Check("wall-feedback-polling-silent", clear == 240 && occupied == 0 &&
                NativeCombatLog.Attempts == attempts && NativeCombatLog.LastMessage == message, polling,
                "Target validity answers correctly for clear and occupied points and publishes nothing while polled.");

            // Controlled case: execute the real cast effect at a point that is
            // no longer clear, the moment the existing message exists for.
            var prior = new HashSet<string>(Game.Instance.State.Units.Where(value =>
                ReferenceEquals(value.Blueprint, run.Section)).Select(value => value.UniqueId));
            attempts = NativeCombatLog.Attempts; published = NativeCombatLog.Published; faults = NativeCombatLog.Faults;
            Rulebook.Trigger(new RuleCastSpell(run.Data, target));
            DateTime deadline = DateTime.UtcNow.AddSeconds(6);
            while (DateTime.UtcNow < deadline && NativeCombatLog.Attempts == attempts)
            { Game.Instance.IsPaused = false; yield return 0; }
            PlaceExpandedSummoningUnit(witness, witnessHome);
            for (int frame = 0; frame < 20; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            var changed = InfernalWallFeedbackState35("placement-changed", attempts, published, faults);
            int sectionsAfterChanged = Game.Instance.State.Units.Count(value =>
                ReferenceEquals(value.Blueprint, run.Section) && !prior.Contains(value.UniqueId));
            changed["newSections"] = sectionsAfterChanged;
            changed["controlledSetup"] = "Native RuleCastSpell executes the Wall effect at a point a disposable witness occupies.";
            cases.Add(changed); InfernalWallPersist35(run, "feedback", row);
            run.Check("wall-feedback-placement-changed", NativeCombatLog.Attempts == attempts + 1 &&
                NativeCombatLog.LastMessage == ContextActionFormInfernalWall35.PlacementChangedMessage &&
                sectionsAfterChanged == 0, changed,
                "A cast whose point is no longer clear forms nothing and tells the player why, exactly once.");

            // Ordinary commands disrupted by a real adjacent creature.
            var adjacent = fixture.Caster;
            int oldReflex = adjacent.Stats.SaveReflex.BaseValue;
            UnitPartSpellResistance resistance = null; int resistanceId = -1;
            bool resistanceAdded = false;
            try
            {
                Vector3 side = (plan.CastingPoint - plan.Midpoint); side.y = 0f; side.Normalize();
                Vector3 farSide = plan.Midpoint - side * 1.2f;
                Vector3 place = InfernalWallGroundWitness35(adjacent, farSide);
                InfernalBoneRemoveEffects35(adjacent, new[] { bone }, run.Frightened);
                foreach (int frame in InfernalWallPlaceAndSettle35(adjacent, place, plan, "feedback-adjacent")) yield return frame;
                // Reflex first; a natural 1 still fails, so retry it boundedly.
                adjacent.Stats.SaveReflex.BaseValue = 100;
                bool reflexPassed = false; JObject reflexCase = null;
                for (int attempt = 0; attempt < 4 && !reflexPassed; attempt++)
                {
                    foreach (int frame in InfernalWallFeedbackCommand35(run, plan, "reflex-disruption", cases, row)) yield return frame;
                    reflexCase = (JObject)cases.Last;
                    reflexPassed = (int?)reflexCase["newSections"] == 0;
                }
                run.Check("wall-feedback-reflex-disruption", reflexPassed &&
                    (string)reflexCase["lastMessage"] == ContextActionFormInfernalWall35.ReflexDisruptionMessage &&
                    (long?)reflexCase["attemptsDelta"] == 1 && (string)reflexCase["command"]["result"] ==
                        UnitCommand.ResultType.Success.ToString() &&
                    reflexCase["saves"].Count() == 1 && (bool?)reflexCase["saves"][0]["passed"] == true, reflexCase,
                    "An ordinary command disrupted by an adjacent creature's successful Reflex save forms nothing and tells the player why.");
                adjacent.Stats.SaveReflex.BaseValue = oldReflex;

                // Spell resistance is exercised by a creature that is actually
                // hostile to the Bone Devil. The installed KingmakerBugfixes
                // quality-of-life postfix on RuleSpellResistanceCheck.HasResistanceRoll
                // waives the roll out of combat between non-enemies, so an allied
                // witness measures that third-party rule rather than this Wall.
                // The ally leaves adjacency first so only the hostile is tested.
                foreach (int frame in InfernalWallPlaceAndSettle35(adjacent, plan.Start, plan, "feedback-ally-clear")) yield return frame;
                Vector3 hostilePlace = InfernalWallGroundWitness35(witness, farSide);
                foreach (int frame in InfernalWallPlaceAndSettle35(witness, hostilePlace, plan, "feedback-hostile-adjacent")) yield return frame;
                bool witnessIsEnemy = bone.Group != null && witness.Group != null && bone.Group.IsEnemy(witness.Group);
                bool inCombatBefore = Game.Instance.Player.IsInCombat;
                resistance = witness.Ensure<UnitPartSpellResistance>();
                resistanceId = resistance.AddResistance(1000, null, null);
                resistanceAdded = true;
                foreach (int frame in InfernalWallFeedbackCommand35(run, plan, "spell-resistance", cases, row)) yield return frame;
                var resisted = (JObject)cases.Last;
                resisted["resistingWitness"] = witness.UniqueId;
                resisted["witnessIsEnemyOfCaster"] = witnessIsEnemy;
                resisted["partyInCombatBefore"] = inCombatBefore;
                resisted["allyClearOfAdjacency"] = InfernalEarlyDistanceToSegment35(adjacent.Position, plan.Midpoint, plan.Midpoint) >
                    InfernalRules35.WallOfIceAdjacencyMeters + InfernalRules35.WallOfIceSectionLengthMeters * .5f;
                resisted["controlledSetup"] = "A hostile disposable witness adjacent to the planned section carries SR 1000; no allied unit is adjacent.";
                InfernalWallPersist35(run, "feedback", row);
                run.Check("wall-feedback-spell-resistance", witnessIsEnemy && (bool)resisted["allyClearOfAdjacency"] &&
                    (int?)resisted["newSections"] == 0 &&
                    (string)resisted["lastMessage"] == ContextActionFormInfernalWall35.ResistedMessage &&
                    (long?)resisted["attemptsDelta"] == 1 && resisted["saves"].Count() == 0 &&
                    resisted["spellResistance"].Any(value => (string)value["target"] == witness.UniqueId &&
                        (bool?)value["rollRequired"] == true && (bool?)value["resisted"] == true), resisted,
                    "An ordinary command disrupted by an adjacent hostile creature's spell resistance forms nothing and tells the player why.");
            }
            finally
            {
                adjacent.Stats.SaveReflex.BaseValue = oldReflex;
                if (resistanceAdded) resistance.Remove(resistanceId);
            }
            PlaceExpandedSummoningUnit(witness, witnessHome);
            for (int frame = 0; frame < 20; frame++) { Game.Instance.IsPaused = false; yield return 0; }
        }

        private IEnumerable<int> InfernalWallFeedbackCommand35(InfernalWallRun35 run,
            InfernalWallFixturePlan35 plan, string caseName, JArray cases, JObject row)
        {
            foreach (int frame in InfernalBoneWaitGame35(6.2, 15)) yield return frame;
            var bone = run.Bone;
            bone.Commands.InterruptAll(true);
            bone.Commands.RemoveFinishedAndUpdateQueue();
            var target = new TargetWrapper(plan.Midpoint);
            var existing = new HashSet<string>(Game.Instance.State.Units.Where(value =>
                ReferenceEquals(value.Blueprint, run.Section)).Select(value => value.UniqueId));
            bool available = run.Data.IsAvailable && run.Data.CanTarget(target);
            long attempts = NativeCombatLog.Attempts, published = NativeCombatLog.Published, faults = NativeCombatLog.Faults;
            int first = run.Trace.Rows.Count;
            var command = new UnitUseAbility(run.Data, target);
            bone.Commands.Run(command);
            foreach (int frame in WaitInfernalCommand34(command, () => true, 25)) yield return frame;
            for (int frame = 0; frame < 30; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            UnitEntityData formed = Game.Instance.State.Units.FirstOrDefault(value =>
                ReferenceEquals(value.Blueprint, run.Section) && !existing.Contains(value.UniqueId));
            var state = InfernalWallFeedbackState35(caseName, attempts, published, faults);
            state["available"] = available;
            state["command"] = Sprint27AiCommand(command);
            state["newSections"] = formed == null ? 0 : 1;
            state["spellResistance"] = new JArray(run.Trace.Rows.Skip(first).Where(value =>
                (string)value["event"] == "spell-resistance" &&
                (string)value["contextAbility"] == run.Wall.AssetGuid));
            state["saves"] = new JArray(run.Trace.Rows.Skip(first).Where(value =>
                (string)value["event"] == "save" && (string)value["type"] == StatType.SaveReflex.ToString()));
            cases.Add(state); InfernalWallPersist35(run, "feedback", row);
            if (formed != null)
            {
                // A natural-1 Reflex let this one form. Retire it from this
                // disposable retry; it supplies no acceptance verdict.
                run.Owned.Add(formed);
                formed.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff)?.Remove();
                DateTime deadline = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < deadline && InfernalWallNavmesh35.ActiveGridClaimCount != 0)
                { Game.Instance.IsPaused = false; yield return 0; }
            }
        }

        private IEnumerable<int> InfernalWallCast35(InfernalWallRun35 run, InfernalWallFormed35 formed)
        {
            var fixture = run.Fixture; var bone = run.Bone; var plan = formed.Plan;
            string label = formed.Label;
            foreach (int frame in InfernalBoneWaitGame35(6.2, 15)) yield return frame;
            bone.Commands.InterruptAll(true);
            bone.Commands.RemoveFinishedAndUpdateQueue();
            PlaceExpandedSummoningUnit(bone, plan.CastingPoint);
            InfernalBoneRemoveEffects35(fixture.Caster, new[] { bone }, run.Frightened);
            foreach (int frame in InfernalWallPlaceAndSettle35(fixture.Caster, plan.Start, plan, "before-cast-" + label)) yield return frame;
            int claimsBeforeCast = InfernalWallNavmesh35.ActiveGridClaimCount;
            var before = InfernalEarlyPath35(fixture.Caster, plan.Destination);
            formed.PathBefore = before;
            bool losBefore = Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(plan.Start, plan.Destination, 0);
            var target = new TargetWrapper(plan.Midpoint);
            bool targetable = run.Data.IsAvailable && run.Data.CanTarget(target);
            var prior = new HashSet<string>(Game.Instance.State.Units.Where(value =>
                ReferenceEquals(value.Blueprint, run.Section)).Select(value => value.UniqueId));
            long feedbackAttempts = NativeCombatLog.Attempts;
            var command = new UnitUseAbility(run.Data, target);
            UnitEntityData section = null; JObject creation; int matches;
            using (var observation = new InfernalWallCreationObservation35(bone, run.Section))
            {
                bone.Commands.Run(command);
                DateTime castDeadline = DateTime.UtcNow.AddSeconds(25);
                while (DateTime.UtcNow < castDeadline && (!command.IsFinished || section == null))
                {
                    Game.Instance.IsPaused = false; yield return 0;
                    section = Game.Instance.State.Units.FirstOrDefault(value =>
                        ReferenceEquals(value.Blueprint, run.Section) && !prior.Contains(value.UniqueId));
                }
                creation = observation.Receipt; matches = observation.Matches;
            }
            if (section != null) run.Owned.Add(section);
            formed.Section = section; formed.Creation = creation;

            var samples = new JArray();
            var withWall = InfernalEarlyPath35(fixture.Caster, plan.Destination);
            bool obstruction = false;
            DateTime pathDeadline = DateTime.UtcNow.AddSeconds(15);
            int nextSample = Time.frameCount;
            while (section != null && DateTime.UtcNow < pathDeadline && !obstruction)
            {
                Game.Instance.IsPaused = false; yield return 0;
                if (Time.frameCount < nextSample) continue;
                nextSample = Time.frameCount + 10;
                withWall = InfernalEarlyPath35(fixture.Caster, plan.Destination);
                obstruction = InfernalEarlyWallObstructs35(before, withWall);
                if (samples.Count < 16) samples.Add(new JObject { ["frame"] = Time.frameCount, ["path"] = withWall });
            }

            var binding = section?.View == null ? null : section.View.GetComponent<InfernalWallNavmesh35>();
            formed.Binding = binding;
            var cut = binding?.Cut; formed.Cut = cut;
            Renderer wallRenderer = binding?.WallVisual == null ? null : binding.WallVisual.GetComponent<Renderer>();
            Bounds? wallBounds = wallRenderer == null ? (Bounds?)null : wallRenderer.bounds;
            string image = null;
            if (section != null && wallRenderer != null && wallRenderer.enabled)
            {
                foreach (int frame in SummonOverhaulSettlePartyCamera(section)) yield return frame;
                image = WriteExpandedSummoningPartyCameraCapture(section, _request.EvidenceDirectory,
                    "s35-wall-formed-" + label + ".png");
            }
            Vector3 groundRoot = Vector3.zero; JObject ground = null;
            bool grounded = section != null && Cleanup2CreatureNativeGroundRoot(section, section.Position, out groundRoot, out ground);
            float groundGap = grounded ? Math.Abs(section.Position.y - groundRoot.y) : float.PositiveInfinity;
            float visualBottomGap = wallBounds.HasValue && section != null ?
                Math.Abs(wallBounds.Value.min.y - section.Position.y) : float.PositiveInfinity;
            Vector3 laneDirection = (plan.Destination - plan.Start).normalized;
            Vector3 along = section == null ? Vector3.zero : Vector3.Cross(Vector3.up, section.OrientationDirection).normalized;
            float laneAlignment = Math.Abs(Vector3.Dot(along, laneDirection));
            var casterToPoint = plan.Midpoint - plan.CastingPoint; casterToPoint.y = 0f;
            float faceAlignment = section == null ? 0f : Math.Abs(Vector3.Dot(casterToPoint.normalized, section.OrientationDirection));
            var nodes = binding == null ? new KeyValuePair<Pathfinding.GraphNode, bool>[0] : binding.GridNodeSnapshot();
            formed.Nodes = nodes; formed.NodesChanged = binding == null ? 0 : binding.GridNodesChanged;
            Buff lifetime = section?.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            formed.LifetimeEndTicks = lifetime == null ? 0L : lifetime.EndTime.Ticks;
            var startNode = AstarPath.active.GetNearest(plan.Start);
            int graphIndex = startNode.node == null ? -1 : (int)startNode.node.GraphIndex;
            var row = new JObject {
                ["label"] = label, ["owner"] = bone.UniqueId, ["ability"] = run.Wall.AssetGuid,
                ["command"] = Sprint27AiCommand(command), ["targetable"] = targetable,
                ["plan"] = plan.Document(), ["section"] = section?.UniqueId,
                ["sectionBlueprint"] = section?.Blueprint?.AssetGuid,
                ["sectionPosition"] = section == null ? null : SurveyVector(section.Position),
                ["sectionHitPoints"] = section?.Stats.HitPoints.ModifiedValue,
                ["creation"] = creation, ["creationMatches"] = matches,
                ["lifetimePresent"] = lifetime != null,
                ["image"] = image, ["wallVisualEnabled"] = wallRenderer != null && wallRenderer.enabled,
                ["wallVisualBounds"] = wallBounds == null ? null : new JObject {
                    ["center"] = SurveyVector(wallBounds.Value.center), ["size"] = SurveyVector(wallBounds.Value.size) },
                ["ground"] = ground, ["groundGap"] = grounded ? groundGap : (float?)null,
                ["visualBottomGap"] = wallBounds.HasValue ? visualBottomGap : (float?)null,
                ["hiddenDonorRenderers"] = binding?.HiddenDonorRendererCount,
                ["donorRendererSuppressions"] = binding?.DonorRendererSuppressions,
                ["visibleDonorRenderers"] = binding?.VisibleDonorRendererCount(),
                ["casterIsPlayerFaction"] = bone.IsPlayerFaction,
                ["sectionIsPlayersEnemy"] = section?.IsPlayersEnemy,
                ["difficultyStatsAdjustments"] = Game.Instance.Player.Difficulty.StatsAdjustmentsType.ToString(),
                ["orientation"] = section?.Orientation,
                ["sectionForward"] = section == null ? null : SurveyVector(section.OrientationDirection),
                ["laneAlignment"] = laneAlignment, ["faceAlignmentToCaster"] = faceAlignment,
                ["cutExactOwner"] = binding != null && ReferenceEquals(binding.Owner, section),
                ["cutType"] = cut?.type.ToString(),
                ["cutRectangle"] = cut == null ? null : new JArray(cut.rectangleSize.x, cut.rectangleSize.y),
                ["cutHeight"] = cut == null ? (float?)null : cut.height,
                ["cutRegistered"] = cut != null && Pathfinding.NavmeshCut.GetAll().Contains(cut),
                ["updateRoute"] = binding?.UpdateRoute, ["recastApplyRequests"] = binding?.RecastApplyRequests,
                ["gridGraphIndex"] = binding?.GridGraphIndex, ["gridNodesConsidered"] = binding?.GridNodesConsidered,
                ["gridNodesClaimed"] = binding?.GridNodesClaimed, ["gridNodesChanged"] = binding?.GridNodesChanged,
                ["gridApplyCompleted"] = binding?.GridApplyCompleted, ["gridFailure"] = binding?.GridFailure,
                ["claimsBeforeCast"] = claimsBeforeCast, ["claimsWithWall"] = InfernalWallNavmesh35.ActiveGridClaimCount,
                ["nodes"] = InfernalWallNodeRows35(nodes),
                ["moverPathGraphIndex"] = graphIndex,
                ["moverPathGraphType"] = graphIndex < 0 ? null : AstarPath.active.graphs[graphIndex]?.GetType().FullName,
                ["pathBefore"] = before, ["pathWithWall"] = withWall, ["pathSamples"] = samples,
                ["pathQueryChanged"] = obstruction,
                ["lineOfSightObstacleBefore"] = losBefore,
                ["lineOfSightObstacleWithWall"] = Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance.HasObstacle(plan.Start, plan.Destination, 0),
                ["feedbackAttemptsDelta"] = NativeCombatLog.Attempts - feedbackAttempts
            };
            // Saved before any verdict can abort, including the pre-breach record.
            File.WriteAllText(Path.Combine(_request.EvidenceDirectory, "s35-wall-navigation-before-breach-" + label + ".json"),
                row.ToString(Formatting.Indented));
            InfernalWallPersist35(run, "formation-" + label, row);
            run.Check("wall-formation-" + label, targetable && command.Result == UnitCommand.ResultType.Success &&
                section != null && matches == 1 && (string)creation["section"] == section.UniqueId &&
                (int?)creation["hitPointsAtCreation"] == InfernalRules35.WallOfIceSectionHitPoints &&
                (double?)creation["nativeRuleDurationSeconds"] == 720d && (double?)creation["nativeRuleBonusSeconds"] == 0d &&
                Math.Abs(((double?)creation["nativeLifetimeSecondsAtCreation"] ?? -1d) - 720d) < .01d &&
                lifetime != null && InfernalEarlyDistanceToSegment35(section.Position, plan.Midpoint, plan.Midpoint) <= .75f &&
                wallRenderer != null && wallRenderer.enabled && visualBottomGap < .02f && groundGap < .15f &&
                Math.Abs(wallBounds.Value.size.y - InfernalWallNavmesh35.SectionHeightMeters) < .25f &&
                binding.VisibleDonorRendererCount() == 0 && laneAlignment >= .9f && faceAlignment >= .9f &&
                cut != null && ReferenceEquals(binding.Owner, section) &&
                cut.type == Pathfinding.NavmeshCut.MeshType.Rectangle && Pathfinding.NavmeshCut.GetAll().Contains(cut) &&
                binding.GridApplyCompleted && binding.GridFailure == null && binding.GridNodesClaimed > 0 &&
                binding.RecastApplyRequests > 0 && obstruction && NativeCombatLog.Attempts == feedbackAttempts,
                row, "An ordinary Bone Devil command at a valid point forms one original 36-HP, 720-second, grounded, visible section facing its caster, with no donor body drawn and its navigation claims live.");
        }

        private static JArray InfernalWallNodeRows35(KeyValuePair<Pathfinding.GraphNode, bool>[] nodes)
        {
            return new JArray(nodes.Select(value => {
                Vector3 position = (Vector3)value.Key.position;
                UnitEntityData nearest = Game.Instance.State.Units.Where(unit => unit != null && !unit.Destroyed &&
                    unit.IsInState && !unit.Descriptor.State.IsDead).OrderBy(unit => Vector3.Distance(unit.Position, position)).FirstOrDefault();
                return new JObject { ["position"] = SurveyVector(position), ["originalWalkable"] = value.Value,
                    ["currentWalkable"] = value.Key.Walkable, ["claimOwners"] = InfernalWallNavmesh35.ClaimOwnerCount(value.Key),
                    ["nearestLiveUnit"] = nearest?.UniqueId,
                    ["nearestLiveUnitDistance"] = nearest == null ? (float?)null : Vector3.Distance(nearest.Position, position) };
            }));
        }

        private IEnumerable<int> InfernalWallLiveMover35(InfernalWallRun35 run, InfernalWallFormed35 formed)
        {
            var plan = formed.Plan;
            foreach (int frame in InfernalEarlyWallMove35(run.Fixture, plan.Start, plan.Destination, plan.Midpoint,
                true, "live-" + formed.Label, run.Check, plan, true, formed.Section,
                "s35-wall-live-mover-" + formed.Label + ".png")) yield return frame;
        }

        // Native eligibility is mechanics distance and NeedLoS. A reach weapon
        // also cannot strike an adjacent creature (tabletop; the installed
        // ProperFlanking2 reach dead zone moves UnitAttack's approach point
        // away from such a target), so a reach witness stands inside reach and
        // outside five feet. The historical 2.5 m point is evaluated first so
        // its verdict is kept.
        private static bool InfernalWallAttackWitnessSurvey35(UnitEntityData attacker, UnitEntityData bone,
            UnitEntityData section, out Vector3 selected, out JObject survey)
        {
            Vector3 forward = section.OrientationDirection; forward.y = 0f; forward.Normalize();
            Vector3 along = Vector3.Cross(Vector3.up, forward).normalized;
            var probe = new UnitAttack(section); probe.Init(attacker);
            int losObjectId = probe.GetTargetLOSObjectId();
            var weapon = attacker.Body.PrimaryHand.MaybeWeapon;
            bool reachWeapon = weapon != null && weapon.Blueprint.IsMelee &&
                weapon.Blueprint.Type.AttackRange > GameConsts.MinWeaponRange;
            float bodies = attacker.View.Corpulence + section.View.Corpulence;
            float deadZone = reachWeapon ? GameConsts.MinWeaponRange.Meters : 0f;
            // Middle of the legal band [bodies + dead zone, approach radius].
            float standoff = (bodies + deadZone + probe.ApproachRadius) * .5f;
            float nearStandoff = Math.Max(bodies + deadZone + .25f, standoff - .35f);
            var candidates = new[] {
                new KeyValuePair<string, Vector3>("historic-far-face-2.5m", forward * 2.5f),
                new KeyValuePair<string, Vector3>("far-face-standoff", forward * standoff),
                new KeyValuePair<string, Vector3>("caster-face-standoff", -forward * standoff),
                new KeyValuePair<string, Vector3>("far-face-near-standoff", forward * nearStandoff),
                new KeyValuePair<string, Vector3>("caster-face-near-standoff", -forward * nearStandoff),
                new KeyValuePair<string, Vector3>("far-diagonal-a", (forward * 2f + along).normalized * standoff),
                new KeyValuePair<string, Vector3>("far-diagonal-b", (forward * 2f - along).normalized * standoff),
                new KeyValuePair<string, Vector3>("caster-diagonal-a", (-forward * 2f + along).normalized * standoff),
                new KeyValuePair<string, Vector3>("caster-diagonal-b", (-forward * 2f - along).normalized * standoff)
            };
            Vector3 eyeShift = attacker.EyePosition - attacker.Position;
            var rows = new JArray(); bool found = false; selected = Vector3.zero;
            foreach (var candidate in candidates)
            {
                Vector3 desired = section.Position + candidate.Value, root; JObject ground;
                bool supported = Cleanup2CreatureNativeGroundRoot(attacker, desired, out root, out ground);
                Vector3 position = supported ? root : desired;
                Pathfinding.NNInfo near = AstarPath.active.GetNearest(position);
                bool walkable = near.node != null && near.node.Walkable &&
                    Vector3.Distance(near.clampedPosition, position) <= .6f;
                float distanceSquared = GeometryUtils.SqrMechanicsDistance(section.Position, position);
                bool distanceAdmitted = distanceSquared <= probe.ApproachRadius * probe.ApproachRadius;
                float edgeGap = (float)Math.Sqrt(distanceSquared) - bodies;
                bool outsideDeadZone = edgeGap >= deadZone + .1f;
                bool losBlocked = probe.NeedLoS && Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance
                    .HasObstacle(position + eyeShift, section.Position, losObjectId);
                float boneGap = Vector3.Distance(position, bone.Position);
                bool clearOfAura = boneGap >= InfernalRules35.WallOfIceAdjacencyMeters + bone.Corpulence + attacker.Corpulence + .5f;
                bool selectable = supported && walkable && distanceAdmitted && outsideDeadZone && !losBlocked && clearOfAura;
                rows.Add(new JObject { ["name"] = candidate.Key, ["position"] = SurveyVector(position),
                    ["nativeGroundSupported"] = supported, ["walkable"] = walkable,
                    ["mechanicsDistanceSquared"] = distanceSquared, ["approachRadius"] = probe.ApproachRadius,
                    ["edgeGap"] = edgeGap, ["outsideReachDeadZone"] = outsideDeadZone,
                    ["distanceAdmitted"] = distanceAdmitted, ["needLoS"] = probe.NeedLoS,
                    ["targetLosObjectId"] = losObjectId, ["eyePosition"] = SurveyVector(position + eyeShift),
                    ["losBlocked"] = losBlocked, ["boneDistance"] = boneGap, ["clearOfSourceAura"] = clearOfAura,
                    ["selectable"] = selectable, ["selected"] = selectable && !found });
                if (!selectable || found) continue;
                found = true; selected = position;
            }
            survey = new JObject { ["attacker"] = attacker.UniqueId, ["section"] = section.UniqueId,
                ["weapon"] = attacker.Body.PrimaryHand.MaybeWeapon?.Blueprint?.AssetGuid,
                ["weaponReachFeet"] = attacker.Body.PrimaryHand.MaybeWeapon?.AttackRange.Value,
                ["approachPoint"] = SurveyVector(probe.ApproachPoint), ["candidates"] = rows,
                ["reachWeapon"] = reachWeapon, ["bodiesMeters"] = bodies, ["reachDeadZoneMeters"] = deadZone,
                ["standoffMeters"] = standoff,
                ["contract"] = "Installed UnitCommand.IsUnitEnoughClose: SqrMechanicsDistance(ApproachPoint, Executor.Position) <= ApproachRadius^2 and, when NeedLoS, !LineOfSightGeometry.HasObstacle(Executor.EyePosition, ApproachPoint, GetTargetLOSObjectId()). UnitAttack does not override the LOS object ID (0)." };
            return found;
        }

        private IEnumerable<int> InfernalWallPhysicalDestruction35(InfernalWallRun35 run, InfernalWallFormed35 formed,
            List<AreaEffectEntityData> areas)
        {
            var attacker = run.Attacker; var section = formed.Section; var bone = run.Bone; var plan = formed.Plan;
            var appear = BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff;
            DateTime appearDeadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < appearDeadline && (section.Buffs.GetBuff(appear) != null ||
                (bool)section.Descriptor.State.IsUntargetable))
            { Game.Instance.IsPaused = false; yield return 0; }
            InfernalBoneRemoveEffects35(attacker, new[] { bone }, run.Frightened);
            Vector3 selected; JObject survey;
            bool anySelectable = InfernalWallAttackWitnessSurvey35(attacker, bone, section, out selected, out survey);
            InfernalWallPersist35(run, "attack-witness-survey", survey);
            if (!anySelectable)
                run.Check("wall-attack-witness-survey", false, survey,
                    "At least one supported, reachable, unobstructed native attack position exists; no reach or LOS override.");
            attacker.Commands.InterruptAll(true);
            attacker.Commands.RemoveFinishedAndUpdateQueue();
            PlaceExpandedSummoningUnit(attacker, selected);
            foreach (UnitEntityData actor in new[] { attacker, section })
            {
                actor.Wake(0f);
                if (!Game.Instance.State.AwakeUnits.Contains(actor)) Game.Instance.State.AwakeUnits.Add(actor);
                actor.SightCache.ClearIfNeeded();
            }
            attacker.Memory.Add(section).Visible = true;
            section.Memory.Add(attacker).Visible = true;
            attacker.JoinCombat(); section.JoinCombat();
            Game.Instance.Player.UpdateIsInCombat();
            new UnitCombatJoinController().Tick();
            new UnitCombatPrepareController().Tick();
            // Native combat preparation can return a unit to its earlier point;
            // place this stationary witness only after preparation completes.
            attacker.View.MovementAgent.Stop();
            foreach (int frame in InfernalWallPlaceAndSettle35(attacker, selected, plan, "physical-attacker")) yield return frame;

            var probe = new UnitAttack(section); probe.Init(attacker);
            Vector3 approach = probe.ApproachPoint;
            float distanceSquared = GeometryUtils.SqrMechanicsDistance(approach, attacker.Position);
            int losObjectId = probe.GetTargetLOSObjectId();
            bool losBlocked = probe.NeedLoS && Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance
                .HasObstacle(attacker.EyePosition, approach, losObjectId);
            bool eligible = probe.IsUnitEnoughClose;
            var eligibility = new JObject { ["attacker"] = attacker.UniqueId, ["attackerPosition"] = SurveyVector(attacker.Position),
                ["attackerEye"] = SurveyVector(attacker.EyePosition), ["sectionPosition"] = SurveyVector(section.Position),
                ["approachPoint"] = SurveyVector(approach), ["mechanicsDistanceSquared"] = distanceSquared,
                ["approachPointIsTarget"] = Vector3.Distance(approach, section.Position) < .01f,
                ["approachRadius"] = probe.ApproachRadius,
                ["distanceAdmitted"] = distanceSquared <= probe.ApproachRadius * probe.ApproachRadius,
                ["needLoS"] = probe.NeedLoS, ["targetLosObjectId"] = losObjectId, ["losBlocked"] = losBlocked,
                ["isUnitEnoughClose"] = eligible, ["canAttack"] = attacker.CanAttack(section),
                ["isEnemy"] = attacker.IsEnemy(section), ["canAct"] = attacker.Descriptor.State.CanAct,
                ["greenNpcGuard"] = Kingmaker.Designers.GameHelper.IsAttackingGreenNPC(attacker, section),
                ["witnessSurvey"] = survey, ["probe"] = Sprint27AiCommand(probe),
                ["reachOverride"] = false, ["losOverride"] = false };
            InfernalWallPersist35(run, "physical-eligibility", eligibility);
            run.Check("wall-physical-native-eligibility", eligible && attacker.CanAttack(section) &&
                attacker.Descriptor.State.CanAct, eligibility,
                "A legitimately positioned attacker passes the unmodified native distance and LOS eligibility for an ordinary attack.");
            foreach (int frame in SummonOverhaulSettlePartyCamera(section)) yield return frame;
            string attackImage = WriteExpandedSummoningPartyCameraCapture(section, _request.EvidenceDirectory,
                "s35-wall-physical-attack-position.png");

            int first = run.Trace.Rows.Count;
            var commands = new JArray();
            Vector3 attackerStart = attacker.Position;
            float attackerTravel = 0f;
            using (var damage = new InfernalWallDamageObservation35(section))
            {
                for (int index = 0; index < 24 && !section.Destroyed && !section.Descriptor.State.IsDead; index++)
                {
                    attacker.Commands.InterruptAll(true);
                    attacker.Commands.RemoveFinishedAndUpdateQueue();
                    var attack = new UnitAttack(section);
                    attacker.Commands.Run(attack);
                    foreach (int frame in WaitInfernalCommand34(attack, () => true, 25)) yield return frame;
                    attackerTravel = Math.Max(attackerTravel, Vector3.Distance(attackerStart, attacker.Position));
                    commands.Add(new JObject { ["index"] = index, ["reference"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(attack),
                        ["command"] = Sprint27AiCommand(attack), ["plannedAttacks"] = attack.AllAttacks.Count,
                        ["sectionDamageAfter"] = section.Damage, ["sectionDead"] = section.Descriptor.State.IsDead });
                    InfernalWallPersist35(run, "physical-commands", commands);
                }
                DateTime settled = DateTime.UtcNow.AddSeconds(10);
                AreaEffectEntityData residual = null, fog = null;
                while (DateTime.UtcNow < settled && (residual == null || InfernalWallNavmesh35.ActiveGridClaimCount != 0))
                {
                    Game.Instance.IsPaused = false; yield return 0;
                    residual = InfernalWallBreachArea35(run, run.Residual, section);
                    fog = InfernalWallBreachArea35(run, run.Fog, section);
                }
                if (residual != null) areas.Add(residual);
                JObject presentation = null;
                foreach (int frame in InfernalWallRemovalPresentation35(run, formed, attacker, "physical", value => presentation = value)) yield return frame;
                var attackRows = run.Trace.Rows.Skip(first).Where(value => (string)value["event"] == "attack" &&
                    (string)value["owner"] == attacker.UniqueId && (string)value["target"] == section.UniqueId).ToArray();
                var rollRows = run.Trace.Rows.Skip(first).Where(value => (string)value["event"] == "attack-roll" &&
                    (string)value["owner"] == attacker.UniqueId && (string)value["target"] == section.UniqueId).ToArray();
                var damageRows = damage.Rows.OfType<JObject>().ToArray();
                int[] before = damageRows.Select(value => (int?)value["before"] ?? -1).ToArray();
                int[] after = damageRows.Select(value => (int?)value["after"] ?? -1).ToArray();
                bool destroyedInSequence = InfernalRules35.WallDamageSequenceDestroys(before, after, InfernalRules35.WallOfIceSectionHitPoints);
                bool allFromAttacker = damageRows.Length > 0 && damageRows.All(value =>
                    (string)value["initiator"] == attacker.UniqueId && (bool?)value["hasAttackRoll"] == true &&
                    !string.IsNullOrEmpty((string)value["weapon"]));
                bool allHalved = damageRows.Length > 0 && damageRows.All(value => value["packets"] != null &&
                    value["packets"].Any() && value["packets"].All(packet => (bool?)packet["half"] == true));
                bool retired = section.Destroyed || !section.IsInState || section.Descriptor.State.IsDead;
                bool autoHits = rollRows.Length >= damageRows.Length && rollRows.All(value => (bool?)value["hit"] == true && (bool?)value["autoHit"] == true);
                double lifetimeSeconds = TimeSpan.FromTicks(formed.LifetimeEndTicks).TotalSeconds;
                double breachSeconds = damageRows.Length == 0 ? 0d : (double?)damageRows.Last()["gameSeconds"] ?? 0d;
                double residualSeconds = residual == null ? 0d : Sprint29AreaDuration(residual);
                bool residualOwned = residual != null && ReferenceEquals(residual.Context?.MaybeCaster, bone) &&
                    ReferenceEquals(residual.Context?.SourceAbility, run.Wall) && !residual.IsEnded;
                bool residualLifetime = residual != null && Math.Abs(residualSeconds - (lifetimeSeconds - breachSeconds)) <= 2d;
                var row = new JObject { ["section"] = section.UniqueId, ["attacker"] = attacker.UniqueId,
                    ["attackImage"] = attackImage, ["commands"] = commands, ["attackEvents"] = new JArray(attackRows),
                    ["attackRolls"] = new JArray(rollRows), ["damageRows"] = new JArray(damageRows),
                    ["damagingHits"] = damageRows.Length, ["hitPointSequenceDestroys"] = destroyedInSequence,
                    ["allDamageFromAttackerWeapon"] = allFromAttacker, ["allPacketsObjectHalved"] = allHalved,
                    ["allRollsAutomaticHits"] = autoHits, ["attackerTravelDuringAttacks"] = attackerTravel,
                    ["sectionDamage"] = section.Damage, ["sectionDead"] = section.Descriptor.State.IsDead,
                    ["sectionDestroyed"] = section.Destroyed, ["residualArea"] = residual?.UniqueId,
                    ["residualOwnedByExactCast"] = residualOwned, ["residualDurationSeconds"] = residualSeconds,
                    ["remainingWallSecondsAtBreach"] = lifetimeSeconds - breachSeconds, ["residualLifetimeMatches"] = residualLifetime,
                    ["residualPosition"] = residual == null ? null : SurveyVector(residual.Position),
                    ["steamArea"] = fog?.UniqueId, ["removalPresentation"] = presentation,
                    ["forcedRolls"] = 0, ["positionWritesDuringAttacks"] = 0 };
                InfernalWallPersist35(run, "physical-destruction", row);
                run.Check("wall-physical-destruction", damageRows.Length >= 2 && destroyedInSequence && allFromAttacker &&
                    allHalved && autoHits && attackRows.Length >= damageRows.Length && retired &&
                    residualOwned && residualLifetime && fog == null && attackerTravel < .25f &&
                    (bool)presentation["pass"], row,
                    "Repeated ordinary native weapon attacks auto-hit, deal object-halved damage tracked hit by hit, and destroy the section, which leaves its owned cold residual for the remaining duration, no steam, and no donor body.");
            }
        }

        private static AreaEffectEntityData InfernalWallBreachArea35(InfernalWallRun35 run,
            BlueprintAbilityAreaEffect blueprint, UnitEntityData section)
        {
            return Game.Instance.State.AreaEffects.All.FirstOrDefault(value => !value.IsEnded &&
                ReferenceEquals(value.Blueprint, blueprint) && ReferenceEquals(value.Context?.MaybeCaster, run.Bone) &&
                ReferenceEquals(value.Context?.SourceAbility, run.Wall) &&
                InfernalEarlyDistanceToSegment35(value.Position, section.Position, section.Position) < 1f);
        }

        // A retired section must not reveal its donor body, in frames or image.
        private IEnumerable<int> InfernalWallRemovalPresentation35(InfernalWallRun35 run, InfernalWallFormed35 formed,
            UnitEntityData fallbackAnchor, string label, Action<JObject> result)
        {
            var binding = formed.Binding; var section = formed.Section;
            int frames = 0, maxVisibleDonors = 0;
            bool wallVisualGone = binding == null || binding.WallVisual == null;
            for (int frame = 0; frame < 45; frame++)
            {
                Game.Instance.IsPaused = false; yield return 0; frames++;
                if (binding != null) maxVisibleDonors = Math.Max(maxVisibleDonors, binding.VisibleDonorRendererCount());
                wallVisualGone = binding == null || binding.WallVisual == null;
            }
            UnitEntityData anchor = section?.View != null ? section : fallbackAnchor;
            foreach (int frame in SummonOverhaulSettlePartyCamera(anchor)) yield return frame;
            string image = WriteExpandedSummoningPartyCameraCapture(anchor, _request.EvidenceDirectory,
                "s35-wall-removed-" + label + ".png");
            if (binding != null) maxVisibleDonors = Math.Max(maxVisibleDonors, binding.VisibleDonorRendererCount());
            result(new JObject { ["framesObserved"] = frames, ["maximumVisibleDonorRenderers"] = maxVisibleDonors,
                ["wallVisualGone"] = wallVisualGone, ["image"] = image, ["viewStillPresent"] = section?.View != null,
                ["anchor"] = anchor.UniqueId, ["pass"] = maxVisibleDonors == 0 && wallVisualGone });
        }

        private IEnumerable<int> InfernalWallFireDestruction35(InfernalWallRun35 run, InfernalWallFormed35 formed,
            List<AreaEffectEntityData> areas)
        {
            var section = formed.Section; var source = run.Fixture.Hostile; var bone = run.Bone;
            var appear = BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff;
            DateTime appearDeadline = DateTime.UtcNow.AddSeconds(10);
            while (DateTime.UtcNow < appearDeadline && (section.Buffs.GetBuff(appear) != null ||
                (bool)section.Descriptor.State.IsUntargetable))
            { Game.Instance.IsPaused = false; yield return 0; }
            bool greenGuard = Kingmaker.Designers.GameHelper.IsAttackingGreenNPC(source, section);
            using (var damage = new InfernalWallDamageObservation35(section))
            {
                Rulebook.Trigger(new RuleDealDamage(source, section, new DamageBundle(
                    new EnergyDamage(new DiceFormula(6, DiceType.One), DamageEnergyType.Fire))));
                yield return 0;
                int afterMeasured = section.Damage;
                bool aliveAfterMeasured = !section.Destroyed && !section.Descriptor.State.IsDead;
                var measuredPath = InfernalEarlyPath35(run.Fixture.Caster, formed.Plan.Destination);
                bool stillObstructing = InfernalEarlyWallObstructs35(formed.PathBefore, measuredPath) &&
                    InfernalWallNavmesh35.ActiveGridClaimCount > 0;
                Rulebook.Trigger(new RuleDealDamage(source, section, new DamageBundle(
                    new EnergyDamage(new DiceFormula(40, DiceType.One), DamageEnergyType.Fire))));
                DateTime settled = DateTime.UtcNow.AddSeconds(10);
                AreaEffectEntityData residual = null, fog = null;
                while (DateTime.UtcNow < settled && (residual == null || fog == null || InfernalWallNavmesh35.ActiveGridClaimCount != 0))
                {
                    Game.Instance.IsPaused = false; yield return 0;
                    residual = InfernalWallBreachArea35(run, run.Residual, section);
                    fog = InfernalWallBreachArea35(run, run.Fog, section);
                }
                if (residual != null) areas.Add(residual);
                if (fog != null) areas.Add(fog);
                JObject presentation = null;
                foreach (int frame in InfernalWallRemovalPresentation35(run, formed, run.Fixture.Caster, "fire", value => presentation = value)) yield return frame;
                var rows = damage.Rows.OfType<JObject>().ToArray();
                bool measuredFull = rows.Length == 2 && (int?)rows[0]["before"] == 0 && afterMeasured == 6 &&
                    rows[0]["packets"].Count() == 1 && (string)rows[0]["packets"][0]["energy"] == DamageEnergyType.Fire.ToString() &&
                    (bool?)rows[0]["packets"][0]["half"] == false && (int?)rows[0]["packets"][0]["final"] == 6;
                double lifetimeSeconds = TimeSpan.FromTicks(formed.LifetimeEndTicks).TotalSeconds;
                double breachSeconds = rows.Length == 0 ? 0d : (double?)rows.Last()["gameSeconds"] ?? 0d;
                double residualSeconds = residual == null ? 0d : Sprint29AreaDuration(residual);
                double fogSeconds = fog == null ? 0d : Sprint29AreaDuration(fog);
                bool residualOwned = residual != null && !residual.IsEnded;
                bool residualLifetime = residual != null && Math.Abs(residualSeconds - (lifetimeSeconds - breachSeconds)) <= 2d;
                bool fogOwned = fog != null && !fog.IsEnded;
                bool fogLifetime = fog != null && Math.Abs(fogSeconds - 600d) <= 2d;
                bool retired = section.Destroyed || !section.IsInState || section.Descriptor.State.IsDead;
                var row = new JObject { ["section"] = section.UniqueId, ["fireSource"] = source.UniqueId,
                    ["controlledSource"] = "Controlled native RuleDealDamage fire packets from the disposable hostile; no direct hit-point write.",
                    ["greenNpcGuard"] = greenGuard, ["damageRows"] = new JArray(rows), ["damageAfterMeasuredFire"] = afterMeasured,
                    ["aliveAfterMeasuredFire"] = aliveAfterMeasured, ["measuredFireFull"] = measuredFull,
                    ["stillObstructingAfterMeasuredFire"] = stillObstructing, ["sectionDead"] = section.Descriptor.State.IsDead,
                    ["residualArea"] = residual?.UniqueId, ["residualDurationSeconds"] = residualSeconds,
                    ["remainingWallSecondsAtBreach"] = lifetimeSeconds - breachSeconds, ["residualLifetimeMatches"] = residualLifetime,
                    ["steamArea"] = fog?.UniqueId, ["steamDurationSeconds"] = fogSeconds, ["steamLifetimeMatches"] = fogLifetime,
                    ["removalPresentation"] = presentation };
                InfernalWallPersist35(run, "fire-destruction", row);
                run.Check("wall-fire-destruction", !greenGuard && measuredFull && aliveAfterMeasured && stillObstructing &&
                    retired && residualOwned && residualLifetime && fogOwned && fogLifetime &&
                    (bool)presentation["pass"], row,
                    "Fire takes full damage without the object halving, destroys the section, and leaves both the owned cold residual for the remaining duration and ten-minute steam, with no donor body.");
            }
        }

        private IEnumerable<int> InfernalWallRestored35(InfernalWallRun35 run, InfernalWallFormed35 formed,
            AreaEffectEntityData residual, AreaEffectEntityData fog)
        {
            var fixture = run.Fixture; var plan = formed.Plan; string label = formed.Label;
            var binding = formed.Binding; var mover = fixture.Caster;
            int claimsAtRetirement = InfernalWallNavmesh35.ActiveGridClaimCount;
            var nodesAtRetirement = InfernalWallNodeRows35(formed.Nodes);
            // Keep witness occupancy separate from the section's own claims.
            foreach (var actor in run.Owned.Where(value => value != null && !value.Destroyed && value.IsInState &&
                !value.Descriptor.State.IsDead && value.Blueprint?.AssetGuid != InfernalRules35.WallSectionUnitGuid))
            {
                actor.Commands.InterruptAll(true);
                actor.Commands.RemoveFinishedAndUpdateQueue();
                PlaceExpandedSummoningUnit(actor, plan.CastingPoint);
            }
            InfernalBoneRemoveEffects35(mover, new[] { run.Bone }, run.Frightened);
            foreach (int frame in InfernalWallPlaceAndSettle35(mover, plan.Start, plan, "restoration-" + label)) yield return frame;
            var after = InfernalEarlyPath35(mover, plan.Destination);
            Func<bool> nodesRestored = () => formed.Nodes.Length > 0 && formed.Nodes.All(value =>
                value.Key.Walkable == value.Value && InfernalWallNavmesh35.ClaimOwnerCount(value.Key) == 0);
            DateTime deadline = DateTime.UtcNow.AddSeconds(15);
            int nextSample = Time.frameCount;
            while (DateTime.UtcNow < deadline && (!InfernalEarlyWallRestored35(formed.PathBefore, after) || !nodesRestored()))
            {
                Game.Instance.IsPaused = false; yield return 0;
                if (Time.frameCount < nextSample) continue;
                nextSample = Time.frameCount + 10;
                after = InfernalEarlyPath35(mover, plan.Destination);
            }
            var retiredView = formed.Section.View;
            GameObject retiredObject = retiredView != null ? retiredView.gameObject : null;
            bool cutRetired = formed.Cut == null && (retiredObject == null ||
                !Pathfinding.NavmeshCut.GetAll().Any(value => value != null &&
                    ReferenceEquals(value.gameObject, retiredObject)));
            bool restoredPath = InfernalEarlyWallRestored35(formed.PathBefore, after);
            var row = new JObject { ["label"] = label, ["section"] = formed.Section.UniqueId,
                ["sectionDead"] = formed.Section.Descriptor.State.IsDead, ["sectionDestroyed"] = formed.Section.Destroyed,
                ["claimsAtRetirement"] = claimsAtRetirement, ["nodesAtRetirementWithWitnessesNearby"] = nodesAtRetirement,
                ["claimsAfter"] = InfernalWallNavmesh35.ActiveGridClaimCount,
                ["nodesAfterWitnessesParked"] = InfernalWallNodeRows35(formed.Nodes),
                ["nodesRestored"] = nodesRestored(), ["gridRestoreQueued"] = binding?.GridRestoreQueued,
                ["gridRestoreCompleted"] = binding?.GridRestoreCompleted, ["gridNodesRestored"] = binding?.GridNodesRestored,
                ["gridNodesChangedAtFormation"] = formed.NodesChanged, ["gridFailure"] = binding?.GridFailure,
                ["cutRetired"] = cutRetired, ["pathBefore"] = formed.PathBefore, ["pathAfter"] = after, ["pathRestored"] = restoredPath };
            InfernalWallPersist35(run, "navigation-restored-" + label, row);
            run.Check("wall-navigation-restored-" + label, InfernalWallNavmesh35.ActiveGridClaimCount == 0 && nodesRestored() &&
                binding.GridRestoreCompleted && binding.GridNodesRestored >= formed.NodesChanged && binding.GridFailure == null &&
                cutRetired && restoredPath, row,
                "This exact section released its cut and every claim, each affected node regained its original state with witnesses parked, and the route query returned.");

            int first = run.Trace.Rows.Count;
            foreach (int frame in InfernalEarlyWallMove35(fixture, plan.Start, plan.Destination, plan.Midpoint,
                false, "restored-" + label, run.Check, plan, true)) yield return frame;
            var cold = run.Trace.Rows.Skip(first).Where(value => (string)value["event"] == "damage" &&
                (string)value["owner"] == run.Bone.UniqueId && (string)value["target"] == mover.UniqueId &&
                (string)value["ability"] == run.Wall.AssetGuid).ToArray();
            bool coldExact = cold.Length > 0 && cold.All(value => (int?)value["actual"] > 0 &&
                value["packets"].Count() == 1 && (string)value["packets"][0]["energy"] == DamageEnergyType.Cold.ToString() &&
                (string)value["packets"][0]["dice"] == "1d6" && (int?)value["packets"][0]["bonus"] == InfernalRules35.WallOfIceResidualBonus);
            var consumer = new JObject { ["label"] = label, ["residualArea"] = residual?.UniqueId,
                ["coldRows"] = new JArray(cold), ["moverCrossedRestoredRoute"] = true };
            if (fog != null)
            {
                Buff steam = InfernalWallSteamEffect35(run, mover);
                consumer["steamBuff"] = steam?.Blueprint?.AssetGuid;
                consumer["steamBuffName"] = steam?.Blueprint?.name;
                consumer["steamOnMover"] = steam != null;
                consumer["steamSource"] = steam?.Context?.MaybeCaster?.UniqueId;
                consumer["steamArea"] = fog.UniqueId;
                consumer["unitsInSteam"] = fog.UnitsInside.Count();
            }
            InfernalWallPersist35(run, "breach-consumers-" + label, consumer);
            if (residual != null)
                run.Check("wall-residual-cold-consumer-" + label, coldExact, consumer,
                    "A real mover entering the breach takes 1d6+12 cold from the exact Bone Devil and Wall ability.");
            else
                run.Check("wall-no-breach-consumer-" + label, cold.Length == 0 &&
                    InfernalWallBreachArea35(run, run.Residual, formed.Section) == null &&
                    InfernalWallBreachArea35(run, run.Fog, formed.Section) == null, consumer,
                    "Native expiry retires quietly: no breach cold or steam appears on the crossed route.");
        }

        private static Buff InfernalWallSteamEffect35(InfernalWallRun35 run, UnitEntityData unit)
        {
            return unit.Buffs.Enumerable.FirstOrDefault(value =>
                ReferenceEquals(value.Context?.MaybeCaster, run.Bone) &&
                ReferenceEquals(value.Context?.SourceAbility, run.Wall));
        }

        // Steam's effect belongs to membership: it holds inside and ends on exit.
        private IEnumerable<int> InfernalWallSteamExit35(InfernalWallRun35 run, InfernalWallFormed35 formed,
            InfernalWallFixturePlan35 outsidePlan)
        {
            var mover = run.Fixture.Caster;
            BlueprintBuff configured = run.Fog.GetComponent<AbilityAreaEffectBuff>()?.Buff;
            AreaEffectEntityData fog = InfernalWallBreachArea35(run, run.Fog, formed.Section);
            Buff inside = null;
            DateTime insideDeadline = DateTime.UtcNow.AddSeconds(6);
            while (DateTime.UtcNow < insideDeadline && (inside = InfernalWallSteamEffect35(run, mover)) == null)
            { Game.Instance.IsPaused = false; yield return 0; }
            float insideGap = InfernalEarlyDistanceToSegment35(mover.Position, formed.Plan.Midpoint, formed.Plan.Midpoint);
            string insideName = inside?.Blueprint?.name;
            string insideGuid = inside?.Blueprint?.AssetGuid;
            foreach (int frame in SummonOverhaulSettlePartyCamera(mover)) yield return frame;
            string image = WriteExpandedSummoningPartyCameraCapture(mover, _request.EvidenceDirectory, "s35-wall-steam.png");
            Vector3 mid = formed.Plan.Midpoint;
            Vector3 outside = Vector3.Distance(outsidePlan.Start, mid) >= Vector3.Distance(outsidePlan.Destination, mid) ?
                outsidePlan.Start : outsidePlan.Destination;
            PlaceExpandedSummoningUnit(mover, outside);
            DateTime exitDeadline = DateTime.UtcNow.AddSeconds(8);
            while (DateTime.UtcNow < exitDeadline && InfernalWallSteamEffect35(run, mover) != null)
            { Game.Instance.IsPaused = false; yield return 0; }
            bool afterExit = InfernalWallSteamEffect35(run, mover) != null;
            var row = new JObject { ["steamArea"] = fog?.UniqueId, ["areaRadiusFeet"] = run.Fog.Size.Value,
                ["configuredBuff"] = configured?.AssetGuid, ["effectBuff"] = insideGuid, ["effectBuffName"] = insideName,
                ["image"] = image, ["areaOwnerSource"] = fog?.Context?.MaybeCaster?.UniqueId,
                ["moverDistanceInside"] = insideGap, ["effectInside"] = inside != null, ["effectAfterExit"] = afterExit,
                ["moverDistanceAfterExit"] = InfernalEarlyDistanceToSegment35(mover.Position, mid, mid),
                ["areaStillLive"] = fog != null && !fog.IsEnded };
            InfernalWallPersist35(run, "steam-consumer", row);
            run.Check("wall-steam-consumer", fog != null && inside != null && !afterExit && !fog.IsEnded &&
                (configured == null || insideGuid == configured.AssetGuid), row,
                "A creature inside the steam carries its owned native effect, which ends on leaving while the area continues.");
        }

        private IEnumerable<int> InfernalWallControlledExpiry35(InfernalWallRun35 run, InfernalWallFormed35 formed)
        {
            var section = formed.Section; var plan = formed.Plan;
            Buff lifetime = section.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            if (lifetime == null) throw new InvalidOperationException("Controlled expiry lacks the original native lifetime.");
            var mover = run.Fixture.Caster;
            InfernalBoneRemoveEffects35(mover, new[] { run.Bone }, run.Frightened);
            foreach (int frame in InfernalWallPlaceAndSettle35(mover, plan.Start, plan, "before-expiry")) yield return frame;
            var whilePresent = InfernalEarlyPath35(mover, plan.Destination);
            int claimsBefore = InfernalWallNavmesh35.ActiveGridClaimCount;
            bool obstructing = InfernalEarlyWallObstructs35(formed.PathBefore, whilePresent) && claimsBefore > 0;
            long deadlineBefore = lifetime.EndTime.Ticks;
            int damageBefore = section.Damage;
            JObject expiry = InfernalWallOriginalDeadlineExpiry35(section, lifetime);
            DateTime finished = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < finished && (InfernalWallNavmesh35.ActiveGridClaimCount != 0 ||
                !(section.Destroyed || !section.IsInState || section.Descriptor.State.IsDead)))
            { Game.Instance.IsPaused = false; yield return 0; }
            JObject presentation = null;
            foreach (int frame in InfernalWallRemovalPresentation35(run, formed, mover, "expiry", value => presentation = value)) yield return frame;
            bool retired = section.Destroyed || !section.IsInState || section.Descriptor.State.IsDead;
            var row = new JObject { ["section"] = section.UniqueId, ["originalCreation"] = formed.Creation,
                ["originalDeadlineTicks"] = deadlineBefore, ["formationDeadlineTicks"] = formed.LifetimeEndTicks,
                ["deadlineUnchanged"] = deadlineBefore == formed.LifetimeEndTicks,
                ["pathBefore"] = formed.PathBefore, ["pathWhilePresent"] = whilePresent, ["obstructingBeforeExpiry"] = obstructing,
                ["claimsBefore"] = claimsBefore, ["claimsAfter"] = InfernalWallNavmesh35.ActiveGridClaimCount,
                ["damageBefore"] = damageBefore, ["expiry"] = expiry, ["sectionRetired"] = retired,
                ["sectionDestroyed"] = section.Destroyed, ["sectionDead"] = section.Descriptor.State.IsDead,
                ["removalPresentation"] = presentation,
                ["label"] = "controlled original-deadline expiry", ["original720SecondRealTimeWait"] = false,
                ["scope"] = "Established Player.GameTime controlled-clock seam advances synchronously past the original deadline, ticks only the owned section's native buff scheduler, then restores the exact clock before yielding. Deadline not shortened; no direct section, buff, collider or grid removal." };
            InfernalWallPersist35(run, "controlled-expiry", row);
            run.Check("wall-controlled-expiry", obstructing && deadlineBefore == formed.LifetimeEndTicks && damageBefore == 0 &&
                (bool?)expiry["removedByNativeScheduler"] == true && (bool?)expiry["clockRestored"] == true &&
                (bool?)expiry["deadlineShortened"] == false && retired && InfernalWallNavmesh35.ActiveGridClaimCount == 0 &&
                (bool)presentation["pass"], row,
                "A separate undamaged section obstructing movement retires through its native scheduler at the original deadline and releases its navigation, with no donor body.");
        }

        private IEnumerable<int> InfernalWallSourceCleanup35(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData bone, Action<string, bool, JObject, string> check)
        {
            var residual = fixture.Blueprints.OfType<BlueprintAbilityAreaEffect>().Single(value =>
                value.name == "KMG_Summoning_Infernal_BoneDevil_WallResidualArea");
            var steam = fixture.Blueprints.OfType<BlueprintAbilityAreaEffect>().Single(value =>
                value.AssetGuid == "e09010a73354a794293ebc7b33c2d130");
            var areas = Game.Instance.State.AreaEffects.All.Where(value => !value.IsEnded &&
                ReferenceEquals(value.Context?.MaybeCaster, bone) &&
                (ReferenceEquals(value.Blueprint, residual) || ReferenceEquals(value.Blueprint, steam))).ToArray();
            var before = new JArray(areas.Select(value => new JObject { ["area"] = value.UniqueId,
                ["blueprint"] = value.Blueprint.AssetGuid, ["durationSeconds"] = Sprint29AreaDuration(value) }));
            Buff lifetime = bone.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            if (lifetime == null) throw new InvalidOperationException("Bone Devil has no native summon lifetime to retire.");
            lifetime.Remove();
            DateTime deadline = DateTime.UtcNow.AddSeconds(8);
            while (DateTime.UtcNow < deadline && areas.Any(value => !value.IsEnded))
            { Game.Instance.IsPaused = false; yield return 0; }
            check("wall-source-retirement-cleanup", areas.Length >= 3 && areas.Count(value =>
                    ReferenceEquals(value.Blueprint, steam)) == 1 && areas.All(value => value.IsEnded) &&
                InfernalWallNavmesh35.ActiveGridClaimCount == 0,
                new JObject { ["source"] = bone.UniqueId, ["areasBefore"] = before,
                    ["areasEnded"] = areas.Count(value => value.IsEnded), ["claims"] = InfernalWallNavmesh35.ActiveGridClaimCount,
                    ["scope"] = "Retiring the source's own summon lifetime is separate from section expiry, which keeps its own deadline proof." },
                "Retiring the exact Bone Devil ends only its surviving Wall residual and steam areas.");
        }
    }
}
