using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// What the real brain actually chooses, rather than what the base
        /// scores suggest: a safe screech, a declined screech with an allied
        /// non-demon in the spread, a self-buff once the area effects are
        /// unavailable, and the bounded Sprint 36 Babau dispel follow-up with
        /// the real AI selector rather than a direct component call.
        /// </summary>
        private IEnumerable<int> ReviewVrockAi37(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint37VrockAiPhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            UnitEntityData ally = fixture.Caster;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetFort = target.Stats.SaveFortitude.BaseValue;
            int oldAllyFort = ally.Stats.SaveFortitude.BaseValue;
            Vector3 oldTargetPosition = target.Position;
            Vector3 oldAllyPosition = ally.Position;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint37-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint37 vrock AI cell failed: " + id);
                };

            try
            {
                string prefix = ExpandedSummoningSpecialBuilder.Vrock37Prefix;
                var screech = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "Screech"];
                var spores = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "Spores"];
                var heroism = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "Heroism"];
                var mirror = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "MirrorImage"];
                var recharge = (BlueprintBuff)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "ScreechRecharge"];
                var cooldown = (BlueprintBuff)ExpandedSummoningBlueprints
                    .Registered.BySymbol[prefix + "SporeCooldown"];

                target.Stats.HitPoints.BaseValue = 100000;
                target.Stats.SaveFortitude.BaseValue = -100;
                ally.Stats.SaveFortitude.BaseValue = -100;

                UnitEntityData vrock = CastExpandedSummoningOwnTier(
                    fixture, "vrock");
                owned.Add(vrock);
                VrockQuiesce37(vrock);
                foreach (int frame in Outsider30NativeAppearance(vrock,
                    vrock.Blueprint.name)) yield return frame;

                float screechRadius = new Feet(
                    VrockRules37Policy.ScreechRadiusFeet).Meters;

                // --- safe: an enemy in the spread and the ally well clear ---
                PlaceExpandedSummoningUnit(target, vrock.Position +
                    Vector3.right * (screechRadius - 1f));
                PlaceExpandedSummoningUnit(ally, vrock.Position -
                    Vector3.right * (screechRadius + 6f));
                foreach (int frame in VrockAiChoice37(vrock, target,
                    new[] { screech, spores, heroism, mirror },
                    "ai-safe-screech", screech, true, check))
                    yield return frame;

                // --- unsafe: an allied non-demon inside the same spread -----
                foreach (int frame in VrockAiReset37(vrock, target, ally,
                    recharge, cooldown)) yield return frame;
                PlaceExpandedSummoningUnit(target, vrock.Position +
                    Vector3.right * (screechRadius - 1f));
                PlaceExpandedSummoningUnit(ally, vrock.Position +
                    Vector3.forward * (screechRadius - 3f));
                foreach (int frame in VrockAiChoice37(vrock, target,
                    new[] { screech, spores, heroism, mirror },
                    "ai-ally-in-spread", screech, false, check))
                    yield return frame;
                // And with the ally still in the spread but both self-buffs
                // already spent, the refusal must hold when the screech is
                // the highest-scoring action left.
                foreach (int frame in VrockAiReset37(vrock, target, ally,
                    recharge, cooldown)) yield return frame;
                PlaceExpandedSummoningUnit(ally, vrock.Position +
                    Vector3.forward * (screechRadius - 3f));
                PlaceExpandedSummoningUnit(target, vrock.Position +
                    Vector3.right * (screechRadius - 1f));
                foreach (int frame in VrockAiChoice37(vrock, target,
                    new[] { screech, spores }, "ai-ally-in-spread-no-buffs",
                    screech, false, check)) yield return frame;

                // --- self-support once both area effects are unavailable ----
                foreach (int frame in VrockAiReset37(vrock, target, ally,
                    recharge, cooldown)) yield return frame;
                PlaceExpandedSummoningUnit(ally, vrock.Position +
                    Vector3.forward * (screechRadius - 3f));
                PlaceExpandedSummoningUnit(target, vrock.Position +
                    Vector3.right * (screechRadius - 1f));
                vrock.Descriptor.AddBuff(recharge, vrock,
                    TimeSpan.FromHours(1));
                vrock.Descriptor.AddBuff(cooldown, vrock,
                    TimeSpan.FromSeconds(GameConsts.RoundDuration * 3));
                foreach (int frame in VrockAiChoice37(vrock, target,
                    new[] { heroism, mirror }, "ai-self-support", null, true,
                    check)) yield return frame;

                foreach (int frame in VrockAiReset37(vrock, target, ally,
                    recharge, cooldown)) yield return frame;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { vrock });
                owned.Remove(vrock);
                ResetExpandedSummoningHostile(fixture);
                target = fixture.Hostile;
                target.Stats.HitPoints.BaseValue = 100000;

                foreach (int frame in VrockBabauDispelFollowUp37(fixture,
                    target, ally, owned, check)) yield return frame;
            }
            finally
            {
                foreach (Buff buff in new[] { fixture.Hostile, ally }
                    .SelectMany(unit => unit.Buffs.Enumerable).ToArray())
                    if (buff.Blueprint != null && buff.Blueprint.name
                        .StartsWith("KMG_", StringComparison.Ordinal))
                        buff.Remove();
                fixture.Hostile.Stats.HitPoints.BaseValue = oldTargetHp;
                fixture.Hostile.Stats.SaveFortitude.BaseValue = oldTargetFort;
                ally.Stats.SaveFortitude.BaseValue = oldAllyFort;
                PlaceExpandedSummoningUnit(fixture.Hostile,
                    oldTargetPosition);
                PlaceExpandedSummoningUnit(ally, oldAllyPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory, "sprint37-vrock-ai.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        /// <summary>
        /// Lets the real brain run for a full window and records every watched
        /// ability it chose, in order.
        ///
        /// <para><b>What this does and does not assert.</b> The first version
        /// required the brain's very first pick to be one named ability, and
        /// the observation refuted that: with every action available and an
        /// enemy inside the spread, the vrock chose Heroism at base score four
        /// over the screech at seven, because considerations scale base scores
        /// rather than merely breaking ties. Nothing printed or chartered
        /// requires a particular opening move, and a demon buffing itself
        /// before a control effect is not a defect - so that assertion was
        /// mine, not the brief's.</para>
        ///
        /// <para>What the brief does require is ally safety and genuine
        /// reachability, so this observes those instead: when the screech is
        /// expected, it must be chosen somewhere inside the window, with other
        /// finite picks allowed before it; when it is not expected, it must
        /// never be chosen while the brain is demonstrably still choosing
        /// other things. A silent brain can no longer pass a refusal
        /// cell.</para>
        /// </summary>
        private IEnumerable<int> VrockAiChoice37(UnitEntityData vrock,
            UnitEntityData target, BlueprintAbility[] watched, string id,
            BlueprintAbility expected, bool shouldChoose,
            Action<string, bool, JObject, string> check)
        {
            // Three decision windows on one summon add up, and an expired
            // summon otherwise presents as an unregistered ability.
            foreach (BlueprintAbility ability in watched)
                VrockRequireLiveSubject37(vrock, ability, id);
            var chosen = new List<UnitUseAbility>();
            var watcher = new VrockCommandWatcher37(vrock, watched, chosen);
            EventBus.Subscribe(watcher);
            // The AI reads caches it does not maintain, so an observation that
            // only switches the brain on watches a brain with nothing to
            // decide from.
            JObject brain = VrockPrepareBrain37(vrock, target);
            var queued = new JArray();
            Func<BlueprintAbility[]> picks = () => chosen
                .Select(value => value.Spell?.Blueprint)
                .Where(value => value != null).Distinct().ToArray();
            try
            {
                // A refusal has to be watched for the whole window: stopping
                // at the first pick would let one unrelated choice stand in
                // for "never chose the screech".
                Func<bool> satisfied = expected != null && shouldChoose
                    ? (Func<bool>)(() => picks().Contains(expected))
                    : expected == null
                        ? (Func<bool>)(() => picks().Any())
                        : (Func<bool>)(() => false);
                foreach (int frame in VrockDriveBrain37(vrock, satisfied, 40,
                    brain)) yield return frame;
                foreach (UnitCommand command in vrock.Commands.Raw
                    .Where(value => value != null).ToArray())
                    queued.Add(Sprint27AiCommand(command));
            }
            finally
            {
                EventBus.Unsubscribe(watcher);
                Sprint28SetBrainActivePreservingActions(vrock, false);
                vrock.Commands.InterruptAll(true);
                vrock.Commands.RemoveFinishedAndUpdateQueue();
            }
            BlueprintAbility[] picked = picks();
            bool pass = expected == null
                ? shouldChoose == picked.Any()
                : shouldChoose
                    ? picked.Contains(expected)
                    // A refusal only means something while the brain was
                    // demonstrably deciding other things.
                    : !picked.Contains(expected) && picked.Any();
            check(id, pass, new JObject {
                ["owner"] = vrock.UniqueId,
                ["target"] = target.UniqueId,
                ["brain"] = brain,
                ["targetDistanceMeters"] =
                    Vector3.Distance(vrock.Position, target.Position),
                ["watched"] = new JArray(watched.Select(value =>
                    value.name)),
                ["expected"] = expected?.name,
                ["shouldChoose"] = shouldChoose,
                ["chosen"] = new JArray(picked.Select(value => value.name)),
                ["chosenInOrder"] = new JArray(chosen.Select(value =>
                    value.Spell?.Blueprint?.name)),
                ["brainWasDeciding"] = picked.Any(),
                ["assertion"] = expected == null
                    ? "some watched ability was chosen"
                    : shouldChoose
                        ? "the named ability was chosen somewhere in the window"
                        : "the named ability was never chosen while others were",
                ["commands"] = queued,
                ["forcedCommands"] = 0,
                ["scope"] =
                    "The real brain selected these; nothing was dispatched on its behalf."
            }, "Observed AI selection for '" + id + "'.");
        }

        private IEnumerable<int> VrockAiReset37(UnitEntityData vrock,
            UnitEntityData target, UnitEntityData ally,
            BlueprintBuff recharge, BlueprintBuff cooldown)
        {
            foreach (Buff buff in vrock.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, recharge) ||
                ReferenceEquals(value.Blueprint, cooldown)).ToArray())
                buff.Remove();
            foreach (Buff buff in new[] { target, ally }
                .SelectMany(unit => unit.Buffs.Enumerable).ToArray())
                if (buff.Blueprint != null && buff.Blueprint.name
                    .StartsWith("KMG_", StringComparison.Ordinal))
                    buff.Remove();
            target.Damage = 0;
            DateTime deadline = DateTime.UtcNow.AddSeconds(15);
            while (vrock.CombatState.Cooldown.StandardAction > 0f &&
                DateTime.UtcNow < deadline)
            {
                Game.Instance.IsPaused = false;
                yield return 0;
            }
        }

        /// <summary>
        /// The bounded Sprint 36 follow-up. The Babau's dispel selector now
        /// refuses an effect that is harmful to the creature carrying it, and
        /// this observes that through the real AI rather than by calling the
        /// predicate: an enemy whose only dispellable magic hurts it is not a
        /// dispel target, the same enemy with a beneficial effect is, and an
        /// enemy with no dispellable magic at all is not.
        /// </summary>
        private IEnumerable<int> VrockBabauDispelFollowUp37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData target,
            UnitEntityData ally, List<UnitEntityData> owned,
            Action<string, bool, JObject, string> check)
        {
            var dispel = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                    .Babau36Prefix + "DispelMagic"];
            UnitEntityData babau = CastExpandedSummoningOwnTier(fixture,
                "babau");
            owned.Add(babau);
            VrockQuiesce37(babau);
            foreach (int frame in Outsider30NativeAppearance(babau,
                babau.Blueprint.name)) yield return frame;
            PlaceExpandedSummoningUnit(target, babau.Position +
                Vector3.right * (babau.Corpulence + target.Corpulence + 2f));

            // Candidate effects, found in the installed library by the exact
            // properties the selector reads: visible, from a spell, and
            // dispellable, split by the blueprint's own Harmful flag.
            BlueprintBuff[] candidates = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintBuff>()
                .Where(value => value != null &&
                    !value.name.StartsWith("KMG_",
                        StringComparison.Ordinal) &&
                    value.IsFromSpell && !value.IsHiddenInUI)
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .ToArray();
            BlueprintBuff harmful = candidates
                .FirstOrDefault(value => value.Harmful);
            BlueprintBuff beneficial = candidates
                .FirstOrDefault(value => !value.Harmful);
            check("babau-dispel-candidates",
                harmful != null && beneficial != null, new JObject {
                    ["searchedBuffs"] = candidates.Length,
                    ["harmful"] = harmful?.name,
                    ["harmfulGuid"] = harmful?.AssetGuid,
                    ["beneficial"] = beneficial?.name,
                    ["beneficialGuid"] = beneficial?.AssetGuid,
                    ["searchScope"] =
                        "Loaded non-KMG spell buffs of this installation that are visible and dispellable, split by the blueprint's own Harmful flag."
                }, "Both a harmful and a beneficial visible spell effect exist in this installation to use as controls.");

            var observations = new[] {
                new { Label = "no-useful-effect", Effect = (BlueprintBuff)null,
                    Caster = (UnitEntityData)null, Expect = false },
                new { Label = "harmful-only", Effect = harmful,
                    Caster = ally, Expect = false },
                new { Label = "beneficial", Effect = beneficial,
                    Caster = target, Expect = true }
            };
            foreach (var observation in observations)
            {
                foreach (Buff buff in target.Buffs.Enumerable.ToArray())
                    if (buff.Blueprint != null && buff.Blueprint.IsFromSpell)
                        buff.Remove();
                Buff applied = null;
                if (observation.Effect != null)
                    applied = target.Descriptor.AddBuff(observation.Effect,
                        observation.Caster,
                        TimeSpan.FromSeconds(GameConsts.RoundDuration * 50));
                bool useful = BabauDispelAi36.HasUsefulEffect(target);
                foreach (int frame in VrockAiChoice37(babau, target,
                    new[] { dispel },
                    "babau-dispel-" + observation.Label, dispel,
                    observation.Expect, (id, pass, row, description) => {
                        row["appliedEffect"] = observation.Effect?.name;
                        row["appliedEffectHarmfulFlag"] =
                            observation.Effect?.Harmful;
                        row["appliedEffectCaster"] =
                            observation.Caster?.UniqueId;
                        row["casterIsEnemyOfOwner"] =
                            observation.Caster != null &&
                            observation.Caster.IsEnemy(target);
                        row["liveEffect"] = applied == null ? null :
                            InfernalSavedEffect34(applied);
                        row["selectorSaysUseful"] = useful;
                        check(id, pass && useful == observation.Expect, row,
                            description);
                    })) yield return frame;
            }
            foreach (Buff buff in target.Buffs.Enumerable.ToArray())
                if (buff.Blueprint != null && buff.Blueprint.IsFromSpell)
                    buff.Remove();
        }

        private sealed class VrockCommandWatcher37 :
            IUnitCommandStartHandler
        {
            private readonly UnitEntityData _owner;
            private readonly BlueprintAbility[] _watched;
            private readonly List<UnitUseAbility> _chosen;

            internal VrockCommandWatcher37(UnitEntityData owner,
                BlueprintAbility[] watched, List<UnitUseAbility> chosen)
            { _owner = owner; _watched = watched; _chosen = chosen; }

            public void HandleUnitCommandDidStart(UnitCommand command)
            {
                var use = command as UnitUseAbility;
                if (use == null ||
                    !ReferenceEquals(use.Executor, _owner)) return;
                BlueprintAbility blueprint = use.Spell?.Blueprint;
                if (blueprint != null && _watched.Contains(blueprint))
                    _chosen.Add(use);
            }

            public void HandleUnitCommandDidEnd(UnitCommand command) { }
        }
    }
}
