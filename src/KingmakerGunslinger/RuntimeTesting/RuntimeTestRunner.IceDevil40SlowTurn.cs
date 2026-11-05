using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewIceDevilSlowTurn40(ExpandedSummoningCorrectionFixture fixture)
        {
            CreateSprint32TurnHostile(fixture);
            var owned = new List<UnitEntityData>(); var trace = new InfernalEvents34();
            var cells = new JArray(); var receipt = new JObject { ["cells"] = cells, ["events"] = trace.Rows,
                ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilSlowTurnPhase };
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Sprint40 Slow turn: " + id);
            };
            BlueprintFeature resistance = null; AddDamageResistancePhysical dr = null;
            var anchor = Sprint32OwnedQuietAnchor(fixture, "s40-slow-turn");
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                foreach (int index in new[] { 0, 1 })
                {
                    var unit = CastExpandedSummoningOwnTier(fixture, IceDevilVisualPolicy40.Key);
                    owned.Add(unit); Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Descriptor.State.AddConditionImmunity(UnitCondition.Frightened);
                    foreach (int frame in Outsider30NativeAppearance(unit, "s40-slow-turn-" + index)) yield return frame;
                }
                var source = owned[0]; var victim = owned[1];
                // This is a new disposable recipient. Establish its own group
                // before switching faction; never contaminate the party group.
                victim.GroupId = victim.UniqueId;
                var hostility = Nalfeshnee39EstablishHostility(fixture, source, victim);
                check("slow-turn-hostility", source.IsEnemy(victim) && victim.IsEnemy(source) &&
                    !ReferenceEquals(source.Group, victim.Group), hostility, "Exact independent Ice recipient is actually hostile.");
                victim.Stats.SaveFortitude.BaseValue = -100; victim.Stats.AC.BaseValue = -100;
                dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
                dr.Value = new ContextValue { ValueType = ContextValueType.Simple, Value = 1000 };
                resistance = ScriptableObject.CreateInstance<BlueprintFeature>();
                resistance.name = "KMG_Runtime_S40_SlowTurnRecipientResistance"; resistance.Ranks = 1;
                resistance.ComponentsArray = new BlueprintComponent[] { dr }; victim.Descriptor.AddFact(resistance);
                InfernalCombatLane34(fixture, source, victim, 3);
                var baseline = new UnitAttack(source) { IsSingleAttack = false, ForceFullAttack = true };
                baseline.Init(victim);
                // AllAttacks is empty until native initialization. Ask the
                // verified native enumeration directly for this diagnostic
                // baseline; this queues or emits no attack.
                int nativeUnrestrictedPlan = baseline.CreateFullAttack().Count;
                var rider = source.Descriptor.Progression.Features.Enumerable.SelectMany(x =>
                    x.SelectComponents<IceDevilSlowOnHit40>()).Single();
                BlueprintBuff slow = rider.Slowed;
                foreach (int frame in IceDevilSpearUntil40(source, victim, () =>
                    victim.Buffs.GetBuff(slow)?.TimeLeft.TotalSeconds >= 23)) yield return frame;
                var applied = victim.Buffs.GetBuff(slow);
                check("slow-turn-real-hit-precondition", nativeUnrestrictedPlan == 5 && applied != null &&
                    ReferenceEquals(applied.Context.MaybeCaster, source) &&
                    !trace.Rows.Any(x => (string)x["event"] == "spell-resistance" &&
                        (string)x["owner"] == source.UniqueId && (string)x["target"] == victim.UniqueId) &&
                    victim.Descriptor.State.HasCondition(UnitCondition.Staggered),
                    new JObject { ["unrestrictedNativePlan"] = nativeUnrestrictedPlan,
                        ["deadline"] = applied?.EndTime.ToString(), ["forcedRolls"] = 0,
                        ["recipientBlueprint"] = victim.Blueprint.AssetGuid,
                        ["riderSpellResistanceChecks"] = 0,
                        ["defenderBoundary"] = "Private recipient has native physical resistance and low AC/Fort; no attacks or dice are manufactured." },
                    "A real spear hit and failed native Fort save slow an existing armed iterative creature; its own five-attack profile supplies the meaningful control.");
                var admission = new JObject(); var passed = new JArray();
                foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { victim }, source, admission)) yield return frame;
                DateTime ready = DateTime.UtcNow.AddSeconds(30);
                while (DateTime.UtcNow < ready && !Sprint18OwnerIsActing(victim))
                {
                    PassInfernalWitness34(victim, new[] { fixture.Caster, fixture.Hostile, source, anchor }, passed);
                    Game.Instance.IsPaused = false; yield return 0;
                }
                check("slow-turn-native-admission", (bool?)admission["admitted"] == true &&
                    Sprint18OwnerIsActing(victim) && victim.Buffs.GetBuff(slow) != null,
                    admission, "The actually slowed recipient receives its native turn before the real finite condition expires.");
                int first = trace.Rows.Count; var costs = new JArray();
                // Neither single attack nor full attack is forced. Native
                // IsFullAttackRestricted reads this actor's actual move-action
                // restriction and selects the available ordinary action.
                var command = new UnitAttack(source) { IsSingleAttack = false, ForceFullAttack = false };
                using (var economy = new FirearmReviewCosts(victim, costs))
                {
                    victim.Commands.Run(command);
                    foreach (int frame in WaitInfernalCommand34(command, () => true, 25)) yield return frame;
                    var attacks = trace.Rows.Skip(first).Where(x => (string)x["event"] == "attack" &&
                        (string)x["owner"] == victim.UniqueId).ToArray();
                    check("slow-turn-native-single-action", attacks.Length == 1 && command.AllAttacks.Count == 1 &&
                        victim.Buffs.GetBuff(slow) != null && command.Result == Kingmaker.UnitLogic.Commands.Base.UnitCommand.ResultType.Success &&
                        economy.Count(command) == 1 && economy.Exact(command, true),
                        new JObject { ["ordinaryCommand"] = Sprint27AiCommand(command), ["attacks"] = new JArray(attacks),
                            ["costs"] = costs, ["nativeUnrestrictedPlan"] = nativeUnrestrictedPlan,
                            ["nativeSlowedPlan"] = command.AllAttacks.Count, ["idleTurnsPassed"] = passed,
                            ["forcedSingleAttack"] = false, ["fixtureExtraAttacks"] = 0 },
                        "Native Slow/Staggered limits the real armed iterative recipient to one ordinary attack on its actual TB turn, paying its native action cost.");
                }
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                foreach (var unit in owned.Where(x => !x.Destroyed))
                    unit.Descriptor.State.RemoveConditionImmunity(UnitCondition.Frightened);
                trace.Dispose(); DisposeExpandedSummoningUnits(fixture.Created, owned);
                if (resistance != null) UnityEngine.Object.Destroy(resistance);
                if (dr != null) UnityEngine.Object.Destroy(dr);
                ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "sprint40-ice-devil-slow-turn.json"), receipt.ToString(Formatting.Indented));
            }
        }
    }
}
