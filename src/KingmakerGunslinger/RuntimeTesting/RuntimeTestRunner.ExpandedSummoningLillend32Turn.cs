using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewLillend32Turn(ExpandedSummoningCorrectionFixture fixture, bool wounded)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue, oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null;
            CreateExpandedSummoningCorrectionHostile(fixture); var target = fixture.Hostile;
            BlueprintFeature immunity = null; bool addedManeuverImmunity = false;
            int oldAc = target.Stats.AC.BaseValue;
            string id = wounded ? "lillend-healing-turn" : "lillend-courage-fallback-turn";
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                owner = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                sibling = CastExpandedSummoningOwnTier(fixture, "lillend-azata");
                foreach (var unit in new[] { owner, sibling })
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                var creation = owner.Blueprint.GetComponents<LillendBardBook32>().Single();
                var book = owner.Descriptor.GetSpellbook(creation.Book); var siblingBook = sibling.Descriptor.GetSpellbook(creation.Book);
                var resource = fixture.Blueprints.OfType<BlueprintAbilityResource>().Single(value => value.name == "KMG_Summoning_FeyCelestial_LillendAzata_PerformanceResource");
                var performAi = owner.Blueprint.Brain.Actions.OfType<LillendCourageAi32>().Single();
                var performance = owner.Descriptor.ActivatableAbilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, performAi.Courage));
                var immune = ScriptableObject.CreateInstance<BuffDescriptorImmunity>(); immune.Descriptor = SpellDescriptor.MindAffecting;
                immunity = ScriptableObject.CreateInstance<BlueprintFeature>(); immunity.name = "KMG_Runtime_Lillend32_TurnImmunity";
                immunity.Ranks = 1; immunity.ComponentsArray = new BlueprintComponent[] { immune }; target.Descriptor.AddFact(immunity);
                if (target.Descriptor.State.HasCondition(UnitCondition.ImmuneToCombatManeuvers))
                    throw new InvalidOperationException("Disposable turn target already has maneuver immunity.");
                target.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers, null); addedManeuverImmunity = true;
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 1.4f, out far, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(sibling, origin + Vector3.right);
                PlaceExpandedSummoningUnit(target, far); target.Stats.AC.BaseValue = -100;
                owner.Damage = wounded ? 18 : 0; int allyAttack = sibling.Stats.AdditionalAttackBonus.ModifiedValue;
                owner.Memory.Add(target); target.Memory.Add(owner); owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                if (owner.HasLOS(target)) owner.Memory.Add(target).Visible = true;
                owner.JoinCombat(); target.JoinCombat();
                var admission = new JObject();
                foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                int waited = 0; DateTime turnDeadline = DateTime.UtcNow.AddSeconds(30);
                while (waited++ < 500 && DateTime.UtcNow < turnDeadline && !Sprint18OwnerIsActing(owner)) yield return 0;
                bool ready = (bool)admission["admitted"] && owner.IsVisibleForPlayer && Sprint18OwnerIsActing(owner) &&
                    owner.Descriptor.State.CanAct && owner.HasLOS(target) && owner.HasLOS(sibling) &&
                    owner.IsEnemy(target) && !owner.IsEnemy(sibling) && owner.DistanceTo(target) < owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Meters &&
                    book.GetSpontaneousSlots(3) == 2 && siblingBook.GetSpontaneousSlots(3) == 2 &&
                    owner.Descriptor.Resources.GetResourceAmount(resource) == 20 && sibling.Descriptor.Resources.GetResourceAmount(resource) == 20;
                Sprint25Check(id + "-ready", ready, new JObject { ["owner"] = owner.UniqueId, ["admission"] = admission,
                    ["lane"] = lane, ["turn"] = Sprint22TurnState(owner, target), ["damage"] = owner.Damage,
                    ["fixtureMindImmune"] = true, ["fixtureManeuverImmune"] = addedManeuverImmunity },
                    "Fresh exact Lillend owns its native active turn, independent pools, a visible eligible ally and an immune close hostile for ordinary fallback.");
                if (!ready) throw new InvalidOperationException("Lillend turn fixture admission failed.");
                var commands = new HashSet<UnitCommand>(); var costs = new JArray();
                var starts = new Lillend32CommandStarts { Owner = owner };
                var attacks = new Sprint24AttackObserver { Owner = owner, Target = target };
                EventBus.Subscribe(starts); EventBus.Subscribe(attacks);
                try
                {
                    Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    {
                        DateTime deadline = DateTime.UtcNow.AddSeconds(45); int nativeFrames = 0;
                        UnitCommand resolved = null; UnitActivateAbility activation = null; UnitAttack attack = null;
                        while (DateTime.UtcNow < deadline)
                        {
                            if (Sprint18OwnerIsActing(owner)) nativeFrames++;
                            foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                            foreach (var command in starts.Commands) commands.Add(command);
                            activation = commands.OfType<UnitActivateAbility>().FirstOrDefault(value => value.IsFinished && value.Result == UnitCommand.ResultType.Success &&
                                ReferenceEquals(value.Ability, performance) && value.AiAction == performAi);
                            attack = commands.OfType<UnitAttack>().FirstOrDefault(value => value.IsFinished && value.Result == UnitCommand.ResultType.Success && value.AiAction != null);
                            resolved = wounded ? (UnitCommand)commands.OfType<UnitUseAbility>().FirstOrDefault(value => value.IsFinished &&
                                value.Result == UnitCommand.ResultType.Success && value.AiAction?.name == "KMG_Summoning_FeyCelestial_LillendAzata_HealAi" &&
                                value.ExecutionProcess != null && value.ExecutionProcess.IsEnded) : activation;
                            if (resolved != null && (wounded || attack != null && performance.IsRunning &&
                                owner.Descriptor.Resources.GetResourceAmount(resource) == 19 && sibling.Stats.AdditionalAttackBonus.ModifiedValue == allyAttack + 2)) break;
                            Game.Instance.IsPaused = false; yield return 0;
                        }
                        bool exact = nativeFrames > 0 && resolved != null && economy.Count(resolved) == 1 && economy.Exact(resolved, true) &&
                            siblingBook.GetSpontaneousSlots(3) == 2 && sibling.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                            (wounded ? resolved.Type == UnitCommand.CommandType.Standard && owner.Damage < 18 && book.GetSpontaneousSlots(3) == 1 :
                                resolved.Type == UnitCommand.CommandType.Move && performance.IsRunning &&
                                owner.Descriptor.Resources.GetResourceAmount(resource) == 19 && sibling.Stats.AdditionalAttackBonus.ModifiedValue == allyAttack + 2 &&
                                commands.OfType<UnitActivateAbility>().Count(value => value.IsStarted) == 1 &&
                                attack != null && economy.Count(attack) == 1 && economy.Exact(attack, true) && attacks.WeaponAttacks.Count > 0 && book.GetSpontaneousSlots(3) == 2);
                        Sprint25Check(id + "-resolved", exact, new JObject { ["owner"] = owner.UniqueId,
                            ["nativeTurnFrames"] = nativeFrames, ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                            ["nativeCommandStarts"] = starts.Rows, ["damageBefore"] = wounded ? 18 : 0, ["damageAfter"] = owner.Damage,
                            ["ownLevel3Slots"] = book.GetSpontaneousSlots(3), ["siblingLevel3Slots"] = siblingBook.GetSpontaneousSlots(3),
                            ["performanceRunning"] = performance.IsRunning, ["performanceRounds"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                            ["siblingPerformanceRounds"] = sibling.Descriptor.Resources.GetResourceAmount(resource),
                            ["allyAttackBefore"] = allyAttack, ["allyAttackAfter"] = sibling.Stats.AdditionalAttackBonus.ModifiedValue,
                            ["weaponEvents"] = new JArray(attacks.WeaponAttacks.Select(value => value.Weapon.Blueprint.AssetGuid)),
                            ["turn"] = Sprint22TurnState(owner, target), ["fixtureAiTicks"] = 0, ["fixtureCastCommands"] = 0,
                            ["fixtureAttackCommands"] = 0, ["fixtureSlotDebits"] = 0, ["forcedRolls"] = 0 },
                            wounded ? "Native turn AI spends a real standard action and one own bard slot to heal meaningful damage." :
                                "Native turn AI spends one Move action for Courage and a legal ordinary attack cost, debits one performance round, and preserves sibling pools.");
                        if (!exact) throw new InvalidOperationException("Lillend native turn outcome failed: " + id);
                    }
                }
                finally { EventBus.Unsubscribe(attacks); EventBus.Unsubscribe(starts); }
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; Game.Instance.TurnBasedCombatController.Activate();
                foreach (var unit in new[] { owner, sibling, target }.Where(value => value != null && !value.Destroyed))
                { if (unit != target) Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); if (unit.IsInCombat) unit.LeaveCombat(); }
                Game.Instance.Player.UpdateIsInCombat(); target.Stats.AC.BaseValue = oldAc;
                if (addedManeuverImmunity) target.Descriptor.State.RemoveCondition(UnitCondition.ImmuneToCombatManeuvers);
                if (immunity != null)
                { target.Descriptor.RemoveFact(immunity); foreach (var component in immunity.ComponentsArray) UnityEngine.Object.DestroyImmediate(component); UnityEngine.Object.DestroyImmediate(immunity); }
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
