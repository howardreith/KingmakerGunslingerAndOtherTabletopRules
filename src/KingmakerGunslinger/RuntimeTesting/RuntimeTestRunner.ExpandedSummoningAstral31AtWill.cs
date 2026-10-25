using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewAstral31AtWill(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            UnitEntityData target = fixture.Hostile;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;

                owner = CastExpandedSummoningOwnTier(fixture, "astral-deva");
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue();

                string[] suffixes = { "Aid", "DispelMagic", "HolyAura",
                    "HolyWord", "Invisibility", "RemoveCurse",
                    "RemoveDisease", "RemoveFear" };
                string[] nativeGuids = {
                    "03a9630394d10164a9410882d31572f0",
                    "92681f181b507b34ea87018e8f7a528a",
                    "808ab74c12df8784ab4eeaf6a107dbea",
                    "4737294a66c91b844842caee8cf505c8",
                    "89940cde01689fb46946b2f8cd7b66b7",
                    "b48674cef2bff5e478a007cf57d8345b",
                    "4093d5a0eb5cae94e909eb1e0e1a6b36",
                    "55a037e514c0ee14a8e3ed14b47061de" };
                int[] spellLevels = { 2, 3, 8, 7, 2, 3, 3, 1 };
                var mappingRows = new JArray();
                bool mapping = true;
                BlueprintAbility invisibility = null;
                AbilityData invisibilityData = null;
                for (int index = 0; index < suffixes.Length; index++)
                {
                    string symbol = ExpandedSummoningSpecialBuilder.Astral31Prefix +
                        suffixes[index];
                    var ability = (BlueprintAbility)ExpandedSummoningBlueprints
                        .Registered.BySymbol[symbol];
                    var native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                        BlueprintBootstrap.Library, nativeGuids[index],
                        "native Astral Deva " + suffixes[index] + " spell");
                    var granted = owner.Descriptor.Abilities.GetAbility(ability);
                    var data = granted == null ? null : new AbilityData(granted);
                    var parameters = data?.CalculateParams();
                    Type[] actualComponents = (ability.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Select(value =>
                            value.GetType()).ToArray();
                    Type[] nativeComponents = (native.ComponentsArray ??
                        Array.Empty<BlueprintComponent>()).Where(value =>
                            !(value is SpellListComponent)).Select(value =>
                                value.GetType()).ToArray();
                    bool selfOnly = suffixes[index] == "Invisibility";
                    bool targetShape = selfOnly ? ability.Range == AbilityRange.Personal &&
                        ability.CanTargetSelf && !ability.CanTargetFriends &&
                        !ability.CanTargetEnemies && !ability.CanTargetPoint :
                        ability.Range == native.Range &&
                        ability.CanTargetSelf == native.CanTargetSelf &&
                        ability.CanTargetFriends == native.CanTargetFriends &&
                        ability.CanTargetEnemies == native.CanTargetEnemies &&
                        ability.CanTargetPoint == native.CanTargetPoint;
                    int expectedDc = 10 + spellLevels[index] +
                        AstralDevaRulesPolicy.Modifier(
                            owner.Descriptor.Stats.Charisma.ModifiedValue);
                    bool exact = data != null &&
                        ability.Type == AbilityType.SpellLike &&
                        ability.ActionType == UnitCommand.CommandType.Standard &&
                        ability.ActionType == native.ActionType &&
                        ReferenceEquals(ability.Icon, native.Icon) &&
                        actualComponents.SequenceEqual(nativeComponents) &&
                        ability.GetComponent<AbilityResourceLogic>() == null &&
                        ability.GetComponent<SpellListComponent>() == null &&
                        targetShape && parameters != null &&
                        parameters.CasterLevel ==
                            AstralDevaRulesPolicy.SpellLikeCasterLevel &&
                        parameters.SpellLevel == spellLevels[index] &&
                        parameters.DC == expectedDc &&
                        !owner.Blueprint.Brain.Actions.OfType<BlueprintAiCastSpell>()
                            .Any(value => ReferenceEquals(value.Ability, ability));
                    mapping &= exact;
                    mappingRows.Add(new JObject {
                        ["suffix"] = suffixes[index],
                        ["ability"] = ability.AssetGuid,
                        ["native"] = native.AssetGuid,
                        ["granted"] = data != null,
                        ["type"] = ability.Type.ToString(),
                        ["action"] = ability.ActionType.ToString(),
                        ["range"] = ability.Range.ToString(),
                        ["cl"] = parameters?.CasterLevel,
                        ["spellLevel"] = parameters?.SpellLevel,
                        ["dc"] = parameters?.DC,
                        ["expectedDc"] = expectedDc,
                        ["components"] = new JArray(actualComponents.Select(
                            value => value.Name)),
                        ["nativeComponents"] = new JArray(nativeComponents.Select(
                            value => value.Name)),
                        ["resourceLogic"] = ability.GetComponent<
                            AbilityResourceLogic>() != null,
                        ["spellList"] = ability.GetComponent<
                            SpellListComponent>() != null,
                        ["aiActions"] = owner.Blueprint.Brain.Actions.OfType<
                            BlueprintAiCastSpell>().Count(value =>
                                ReferenceEquals(value.Ability, ability)),
                        ["selfOnlyAdaptation"] = selfOnly,
                        ["exact"] = exact });
                    if (selfOnly)
                    {
                        invisibility = ability;
                        invisibilityData = data;
                    }
                }
                Sprint25Check("astral-atwill-native-mapping", mapping,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["charisma"] = owner.Descriptor.Stats.Charisma.ModifiedValue,
                        ["abilities"] = mappingRows,
                        ["manualOnly"] = true,
                        ["atWillNoResource"] = true },
                    "All eight private at-will SLAs preserve their installed native component/icon graphs, use exact-owner CL13/live-Charisma parameters, omit donor spell-list/resource state, and remain manual-only.");
                if (!mapping) throw new InvalidOperationException(
                    "Astral at-will native mapping boundary failed.");

                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner,
                    target, 8f, out far, out lane, out direction,
                    new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, far);
                foreach (UnitEntityData unit in new[] { owner, target })
                {
                    unit.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                owner.JoinCombat(); target.JoinCombat();
                Game.Instance.Player.UpdateIsInCombat();
                for (int frame = 0; frame < 4; frame++) yield return 0;

                bool targetBoundary = invisibility != null &&
                    invisibilityData != null &&
                    invisibilityData.CanTarget(new TargetWrapper(owner)) &&
                    !invisibilityData.CanTarget(new TargetWrapper(fixture.Caster)) &&
                    !invisibilityData.CanTarget(new TargetWrapper(target)) &&
                    !invisibilityData.CanTarget(new TargetWrapper(owner.Position));
                Sprint25Check("astral-atwill-invisibility-targets",
                    targetBoundary, new JObject { ["owner"] = owner.UniqueId,
                        ["ally"] = fixture.Caster.UniqueId,
                        ["enemy"] = target.UniqueId,
                        ["self"] = invisibilityData?.CanTarget(
                            new TargetWrapper(owner)),
                        ["friend"] = invisibilityData?.CanTarget(
                            new TargetWrapper(fixture.Caster)),
                        ["enemyAllowed"] = invisibilityData?.CanTarget(
                            new TargetWrapper(target)),
                        ["point"] = invisibilityData?.CanTarget(
                            new TargetWrapper(owner.Position)) },
                    "The printed self-only adaptation accepts only the exact Astral; ally, enemy and point targets are refused by live AbilityData.");
                if (!targetBoundary) throw new InvalidOperationException(
                    "Astral self-only Invisibility target boundary failed.");

                var buffsBefore = owner.Buffs.Enumerable.ToArray();
                UnitUseAbility command = null;
                var costs = new JArray();
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    foreach (int frame in Sprint26Cast(owner, invisibility,
                        new TargetWrapper(owner), value => command = value))
                        yield return frame;
                    var added = owner.Buffs.Enumerable.Where(value =>
                        !buffsBefore.Contains(value) &&
                        ReferenceEquals(value.Context?.MaybeCaster, owner) &&
                        ReferenceEquals(value.Context?.SourceAbility,
                            invisibility)).ToArray();
                    var attributed = added.SingleOrDefault(value =>
                        value.Blueprint.AssetGuid ==
                            "525f980cb29bc2240b93e953974cb325");
                    bool exact = command != null && command.IsFinished &&
                        command.Result == UnitCommand.ResultType.Success &&
                        command.ExecutionProcess != null &&
                        command.ExecutionProcess.IsEnded &&
                        command.AiAction == null && !command.Cutscene &&
                        !command.IsIgnoreCooldown &&
                        economy.Count(command) == 1 &&
                        economy.Exact(command, false) &&
                        owner.Descriptor.State.HasCondition(
                            UnitCondition.Invisible) &&
                        attributed != null &&
                        attributed.Context.Params.CasterLevel == 13 &&
                        attributed.TimeLeft.TotalSeconds > 770d &&
                        attributed.TimeLeft.TotalSeconds <= 780.1d &&
                        owner.Get<Kingmaker.UnitLogic.Parts.UnitPartTouch>() == null;
                    Sprint25Check("astral-atwill-invisibility-command", exact,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["lane"] = lane,
                            ["command"] = command == null ? null :
                                Sprint27AiCommand(command),
                            ["costs"] = costs,
                            ["invisible"] = owner.Descriptor.State.HasCondition(
                                UnitCondition.Invisible),
                            ["addedBuffs"] = new JArray(added.Select(value =>
                                new JObject { ["buff"] = value.Blueprint.AssetGuid,
                                    ["source"] = value.Context?.SourceAbility?.AssetGuid,
                                    ["caster"] = value.Context?.MaybeCaster?.UniqueId,
                                    ["cl"] = value.Context?.Params?.CasterLevel,
                                    ["seconds"] = value.TimeLeft.TotalSeconds })),
                            ["touchHeld"] = owner.Get<
                                Kingmaker.UnitLogic.Parts.UnitPartTouch>() != null,
                            ["atWillResourceDebits"] = 0,
                            ["manualNativeCommand"] = true },
                        "A real standard-action CL13 command applies the attributed native 13-minute invisibility state to the Astral itself without a resource or retained touch delivery.");
                    if (!exact) throw new InvalidOperationException(
                        "Astral self-only Invisibility native command failed.");
                    foreach (var buff in added.Where(value => value != null &&
                        !value.IsDisposed).ToArray()) buff.Remove();
                }

                Sprint28SetBrainActivePreservingActions(owner, false);
                if (owner.View != null && owner.View.AgentASP != null)
                    owner.View.AgentASP.Stop();
                owner.Commands.InterruptAll(value => value != null);
                owner.Commands.RemoveFinishedAndUpdateQueue();
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner });
                string hostileReset = ResetExpandedSummoningHostile(fixture);
                int cleanupFrames = 0;
                DateTime cleanupDeadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < cleanupDeadline &&
                    cleanupFrames++ < 600 && Game.Instance.State.AreaEffects.All
                        .Any(value => value?.Context != null && ReferenceEquals(
                            value.Context.MaybeCaster, owner))) yield return 0;
                bool noOwnedAreas = !Game.Instance.State.AreaEffects.All.Any(
                    value => value?.Context != null && ReferenceEquals(
                        value.Context.MaybeCaster, owner));
                var retirementCommands = owner.Commands.Raw.Where(value =>
                    value != null && !value.IsFinished).ToArray();
                owner.Commands.InterruptAll(value => value != null);
                owner.Commands.RemoveFinishedAndUpdateQueue();
                var activeCommands = owner.Commands.Raw.Where(value =>
                    value != null && !value.IsFinished).ToArray();
                bool cleanup = owner.Destroyed && !owner.IsInState &&
                    noOwnedAreas && activeCommands.Length == 0 &&
                    target.Damage == fixture.HostileDamage &&
                    target.Descriptor.State.Size == fixture.HostileSize;
                Sprint25Check("astral-atwill-owned-cleanup", cleanup,
                    new JObject { ["ownerDestroyed"] = owner.Destroyed,
                        ["ownerAbsent"] = !owner.IsInState,
                        ["retirementCommands"] = new JArray(retirementCommands
                            .Select(value => value.GetType().Name).ToArray()),
                        ["activeCommands"] = new JArray(activeCommands.Select(
                            value => value.GetType().Name).ToArray()),
                        ["ownedAreasAbsent"] = noOwnedAreas,
                        ["cleanupFrames"] = cleanupFrames,
                        ["hostileReset"] = hostileReset,
                        ["hostileDamageRestored"] = target.Damage ==
                            fixture.HostileDamage,
                        ["hostileSizeRestored"] = target.Descriptor.State.Size ==
                            fixture.HostileSize },
                    "The isolated at-will slice removes its exact Astral, command, aura and invisibility state, then restores the disposable hostile boundary.");
                if (!cleanup) throw new InvalidOperationException(
                    "Astral at-will owned cleanup failed.");
            }
            finally
            {
                if (owner != null && !owner.Destroyed)
                {
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(true);
                    if (owner.IsInCombat) owner.LeaveCombat();
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    owner == null || owner.Destroyed ?
                        Array.Empty<UnitEntityData>() : new[] { owner });
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
