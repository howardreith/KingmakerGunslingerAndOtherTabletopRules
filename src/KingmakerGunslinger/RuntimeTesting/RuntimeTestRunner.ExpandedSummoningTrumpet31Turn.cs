using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewTrumpet31BookTurn(ExpandedSummoningCorrectionFixture fixture)
        {
            CaptureCelestial31CasterInputs();
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null;
            CreateExpandedSummoningCorrectionHostile(fixture); var target = fixture.Hostile;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 40f);
                owner = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                sibling = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                foreach (var unit in new[] { owner, sibling })
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                var creation = owner.Blueprint.GetComponent<TrumpetPreparedBook>();
                var book = owner.Descriptor.GetSpellbook(creation.Book); var siblingBook = sibling.Descriptor.GetSpellbook(creation.Book);
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction, new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(sibling, origin + Vector3.right);
                PlaceExpandedSummoningUnit(target, far); owner.Damage = 100;
                owner.Memory.Add(target); target.Memory.Add(owner); owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                if (owner.HasLOS(target)) owner.Memory.Add(target).Visible = true;
                owner.JoinCombat(); target.JoinCombat();
                var admission = new JObject();
                foreach (int frame in Sprint22EnrollTurnBasedOwners(new[] { owner }, target, admission)) yield return frame;
                int waited = 0; DateTime turnDeadline = DateTime.UtcNow.AddSeconds(30);
                while (waited++ < 500 && DateTime.UtcNow < turnDeadline && !Sprint18OwnerIsActing(owner)) yield return 0;
                bool acting = (bool)admission["admitted"] && owner.IsVisibleForPlayer && Sprint18OwnerIsActing(owner) &&
                    Trumpet31HealSlots(book, creation.Heal) == 2 && Trumpet31HealSlots(siblingBook, creation.Heal) == 2;
                Sprint25Check("trumpet-book-turn-admission", acting, new JObject { ["owner"] = owner.UniqueId,
                    ["admission"] = admission, ["turn"] = Sprint22TurnState(owner, target), ["waitedFrames"] = waited },
                    "The exact request-owned Trumpet receives its real native active turn with independent prepared slots intact.");
                if (!acting) throw new InvalidOperationException("Trumpet native turn admission failed.");
                int damageBeforeAi = owner.Damage;
                Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                var commands = new HashSet<UnitCommand>(); var costs = new JArray(); int nativeTurnFrames = 0;
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    DateTime deadline = DateTime.UtcNow.AddSeconds(35);
                    for (int frame = 0; frame < 3000 && DateTime.UtcNow < deadline; frame++)
                    {
                        if (Sprint18OwnerIsActing(owner)) nativeTurnFrames++;
                        foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                        if (commands.OfType<UnitUseAbility>().Any(value => value.AiAction != null && value.IsFinished &&
                            value.ExecutionProcess != null && value.ExecutionProcess.IsEnded) && owner.Damage == 0) break;
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    var cast = commands.OfType<UnitUseAbility>().FirstOrDefault(value =>
                        value.AiAction?.name == "KMG_Summoning_OutsiderCelestial_TrumpetArchon_HealAi" &&
                        value.IsFinished && value.Result == UnitCommand.ResultType.Success);
                    bool exact = cast != null && nativeTurnFrames > 0 && Sprint18OwnerIsActing(owner) &&
                        !cast.Cutscene && !cast.IsIgnoreCooldown && economy.Count(cast) == 1 && economy.Exact(cast, true) &&
                        owner.Damage == 0 && Trumpet31HealSlots(book, creation.Heal) == 1 &&
                        Trumpet31HealSlots(siblingBook, creation.Heal) == 2 &&
                        commands.Count(value => value.AiAction != null && value.IsFinished) == 1;
                    Sprint25Check("trumpet-book-turn-native-ai", exact, new JObject { ["owner"] = owner.UniqueId,
                        ["nativeTurnFrames"] = nativeTurnFrames, ["sameTurn"] = Sprint18OwnerIsActing(owner),
                        ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["costs"] = costs,
                        ["damageBeforeAi"] = damageBeforeAi, ["damageAfter"] = owner.Damage,
                        ["cutscene"] = cast?.Cutscene, ["ignoreCooldown"] = cast?.IsIgnoreCooldown,
                        ["nativeCostApplications"] = cast == null ? 0 : economy.Count(cast),
                        ["turnBasedCostExact"] = cast != null && economy.Exact(cast, true), ["ownerHealSlots"] = Trumpet31HealSlots(book, creation.Heal),
                        ["siblingHealSlots"] = Trumpet31HealSlots(siblingBook, creation.Heal), ["turn"] = Sprint22TurnState(owner, target),
                        ["fixtureCasts"] = 0, ["fixtureAiTicks"] = 0, ["fixtureSlotDebits"] = 0, ["forcedRolls"] = 0 },
                        "The native turn controller selects and resolves one prepared Heal on the exact actor's turn with its own slot and standard-action cost.");
                    if (!exact) throw new InvalidOperationException("Trumpet native turn AI/slot/action failed.");
                }
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false; Game.Instance.TurnBasedCombatController.Activate();
                foreach (var unit in new[] { owner, sibling, target }.Where(value => value != null && !value.Destroyed))
                {
                    if (unit != target) Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true); if (unit.IsInCombat) unit.LeaveCombat();
                }
                Game.Instance.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode; Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
