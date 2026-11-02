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
using Kingmaker.Blueprints.Classes;
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

                // --- safe: an enemy in the spread and NOTHING friendly in
                // it. Parking the one fixture ally was not enough: the vrock
                // is a player summon, so the party and every neutral
                // bystander are non-enemies too, and the previous run showed
                // the consideration vetoing the screech for the whole window
                // with six real AI ticks. This moves only the two disposable
                // units until the production consideration itself reports
                // nothing endangered.
                JObject isolation = VrockIsolate37(vrock, target, 1.0f);
                JObject safeVeto = VrockScreechVeto37(vrock);
                check("ai-safe-position-is-safe",
                    (bool)safeVeto["found"] &&
                    (int)safeVeto["endangeredCount"] == 0,
                    new JObject {
                        ["owner"] = vrock.UniqueId,
                        ["isolation"] = isolation,
                        ["screechEligibility"] = safeVeto,
                        ["targetDistanceMeters"] =
                            Vector3.Distance(vrock.Position, target.Position)
                    }, "The safe configuration is demonstrated: the production ally-safety consideration reports no endangered non-enemy inside the spread.");
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
                // The enemy is placed adjacent here, unlike the cells above.
                // With both self-buffs spent and both area effects vetoed by
                // the ally, the only move left is an ordinary attack, and the
                // fifth run showed the brain starting nothing at all against a
                // target eight metres away: it does not approach. An
                // unreachable target makes a refusal unfalsifiable - "chose
                // nothing" would pass for "correctly refused" - so the target
                // is put in reach, exactly as the released attack-fallback
                // observation does, and the refusal is then observed while the
                // brain is demonstrably attacking.
                PlaceExpandedSummoningUnit(target, vrock.Position +
                    Vector3.right * (vrock.Corpulence + target.Corpulence +
                        0.3f));
                foreach (int frame in VrockAiChoice37(vrock, target,
                    new[] { screech, spores }, "ai-ally-in-spread-no-buffs",
                    screech, false, check)) yield return frame;

                // --- self-support once both area effects are unavailable ----
                // On a FRESH vrock. The sixth run showed this cell was
                // unsatisfiable where it stood: the cells above deliberately
                // let the brain take both single-use self-buffs, so by the
                // time self-support was observed there was nothing left to
                // choose, and the receipt showed twenty-two ticks with no
                // command and both resources at zero. Observing self-support
                // requires a vrock that still holds its two uses.
                foreach (int frame in VrockAiReset37(vrock, target, ally,
                    recharge, cooldown)) yield return frame;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { vrock });
                owned.Remove(vrock);
                UnitEntityData rested = CastExpandedSummoningOwnTier(
                    fixture, "vrock");
                owned.Add(rested);
                VrockQuiesce37(rested);
                foreach (int frame in Outsider30NativeAppearance(rested,
                    rested.Blueprint.name)) yield return frame;
                PlaceExpandedSummoningUnit(target, rested.Position +
                    Vector3.right * (rested.Corpulence + target.Corpulence +
                        0.3f));
                // Both area effects held unavailable by their own markers, so
                // the self-buffs are the only things left for the brain to
                // weigh.
                rested.Descriptor.AddBuff(recharge, rested,
                    TimeSpan.FromHours(1));
                rested.Descriptor.AddBuff(cooldown, rested,
                    TimeSpan.FromSeconds(GameConsts.RoundDuration * 3));
                int heroismHeld = rested.Descriptor.Resources
                    .GetResourceAmount((BlueprintAbilityResource)
                        ExpandedSummoningBlueprints.Registered.BySymbol[
                            prefix + "HeroismResource"]);
                int mirrorHeld = rested.Descriptor.Resources
                    .GetResourceAmount((BlueprintAbilityResource)
                        ExpandedSummoningBlueprints.Registered.BySymbol[
                            prefix + "MirrorImageResource"]);
                check("ai-self-support-preconditions",
                    heroismHeld == 1 && mirrorHeld == 1 &&
                    rested.Buffs.GetBuff(recharge) != null &&
                    rested.Buffs.GetBuff(cooldown) != null,
                    new JObject {
                        ["owner"] = rested.UniqueId,
                        ["heroismHeld"] = heroismHeld,
                        ["mirrorHeld"] = mirrorHeld,
                        ["screechMarkerHeld"] =
                            rested.Buffs.GetBuff(recharge) != null,
                        ["sporeMarkerHeld"] =
                            rested.Buffs.GetBuff(cooldown) != null
                    }, "The self-support case starts from a vrock that still holds both single uses while both area effects are held unavailable.");
                foreach (int frame in VrockAiChoice37(rested, target,
                    new[] { heroism, mirror }, "ai-self-support", null, true,
                    check)) yield return frame;

                foreach (int frame in VrockAiReset37(rested, target, ally,
                    recharge, cooldown)) yield return frame;
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { rested });
                owned.Remove(rested);
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
            var acted = new List<UnitCommand>();
            var watcher = new VrockCommandWatcher37(vrock, watched, chosen,
                acted);
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
                    // demonstrably acting. The evidence has to be ANY command
                    // this owner started, not another watched ability: when
                    // the ally veto refuses both area effects and both
                    // self-buffs are already spent, the brain's remaining move
                    // is an ordinary attack, which is a UnitAttack and not a
                    // watched UnitUseAbility. Requiring a second watched pick
                    // made a correct double refusal look like a silent brain.
                    : !picked.Contains(expected) && acted.Count > 0;
            check(id, pass, new JObject {
                ["owner"] = vrock.UniqueId,
                ["target"] = target.UniqueId,
                ["brain"] = brain,
                ["screechEligibility"] = VrockScreechVeto37(vrock),
                ["targetDistanceMeters"] =
                    Vector3.Distance(vrock.Position, target.Position),
                ["watched"] = new JArray(watched.Select(value =>
                    value.name)),
                ["expected"] = expected?.name,
                ["shouldChoose"] = shouldChoose,
                ["chosen"] = new JArray(picked.Select(value => value.name)),
                ["chosenInOrder"] = new JArray(chosen.Select(value =>
                    value.Spell?.Blueprint?.name)),
                ["brainWasDeciding"] = acted.Count > 0,
                ["commandsStarted"] = acted.Count,
                ["commandTypesStarted"] = new JArray(acted
                    .Select(value => value.GetType().Name).Distinct()),
                ["assertion"] = expected == null
                    ? "some watched ability was chosen"
                    : shouldChoose
                        ? "the named ability was chosen somewhere in the window"
                        : "the named ability was never chosen while the brain was acting",
                ["commands"] = queued,
                ["forcedCommands"] = 0,
                ["scope"] =
                    "The real brain selected these; nothing was dispatched on its behalf."
            }, "Observed AI selection for '" + id + "'.");
        }

        /// <summary>
        /// The production ally-safety consideration's own answer, including
        /// every non-enemy it counts as endangered.
        ///
        /// <para>This is what the second AI observation needed and lacked. The
        /// vrock is a player summon, so the whole party and every neutral
        /// bystander are non-enemies: with any of them inside the thirty-foot
        /// spread the consideration scores zero, and the brain never selects
        /// the screech however high its base score. That is the ally-safe
        /// requirement working, not a defect - but a cell that calls a
        /// position "safe" has to prove nothing is endangered rather than
        /// assume it, and a veto has to name who caused it.</para>
        /// </summary>
        private static JObject VrockScreechVeto37(UnitEntityData vrock)
        {
            var actions = vrock.Brain == null ? null :
                vrock.Brain.AvailableActions;
            var eligibility = (actions == null
                    ? Enumerable.Empty<Kingmaker.UnitLogic.AiAction>()
                    : actions.AsEnumerable())
                .Select(action => action.Blueprint)
                .Where(blueprint => blueprint != null)
                .SelectMany(blueprint => blueprint.ActorConsiderations ??
                    Array.Empty<Kingmaker.Controllers.Brain.Blueprints
                        .Considerations.Consideration>())
                .OfType<VrockScreechEligibility37>().FirstOrDefault();
            if (eligibility == null)
                return new JObject { ["found"] = false };
            UnitEntityData[] endangered = eligibility
                .EndangeredNonEnemies(vrock).ToArray();
            return new JObject {
                ["found"] = true,
                ["endangeredCount"] = endangered.Length,
                ["endangered"] = new JArray(endangered.Select(unit =>
                    new JObject {
                        ["unit"] = unit.UniqueId,
                        ["blueprint"] = unit.Blueprint?.name,
                        ["faction"] = unit.Faction?.name,
                        ["distanceMeters"] =
                            Vector3.Distance(vrock.Position, unit.Position),
                        ["corpulence"] = unit.Corpulence
                    })),
                ["why"] = "A player summon's party and every neutral bystander are non-enemies; any inside the spread scores the screech at zero."
            };
        }

        /// <summary>
        /// Moves this observer's own two units away from every non-enemy until
        /// the production consideration reports nothing endangered, so a
        /// "safe" configuration is demonstrated rather than asserted. Only the
        /// disposable summon and the request-owned hostile are moved.
        /// </summary>
        private static JObject VrockIsolate37(UnitEntityData vrock,
            UnitEntityData target, float enemyGapMeters)
        {
            float radius = new Feet(
                VrockRules37Policy.ScreechRadiusFeet).Meters;
            var attempts = new JArray();
            for (int step = 1; step <= 6; step++)
            {
                Vector3 home = vrock.Position;
                UnitEntityData[] others = (Game.Instance?.State?.Units?.All ??
                    Enumerable.Empty<UnitEntityData>())
                    .Where(unit => unit != null && unit.IsInState &&
                        !ReferenceEquals(unit, vrock) &&
                        !ReferenceEquals(unit, target) &&
                        !vrock.IsEnemy(unit)).ToArray();
                if (others.Length == 0) break;
                // Step directly away from the nearest crowd of non-enemies.
                Vector3 crowd = others
                    .OrderBy(unit => Vector3.Distance(home, unit.Position))
                    .Take(8)
                    .Aggregate(Vector3.zero, (sum, unit) =>
                        sum + unit.Position) / Math.Min(8, others.Length);
                Vector3 away = home - crowd;
                if (away.sqrMagnitude < 0.01f) away = Vector3.right;
                away = away.normalized;
                Vector3 destination = home + away * (radius * 1.5f);
                PlaceExpandedSummoningUnit(vrock, destination);
                PlaceExpandedSummoningUnit(target, destination +
                    away * 0f + Vector3.right *
                        (vrock.Corpulence + target.Corpulence +
                            enemyGapMeters));
                float nearest = others
                    .Select(unit => Vector3.Distance(vrock.Position,
                        unit.Position) - Math.Max(0f, unit.Corpulence))
                    .DefaultIfEmpty(float.PositiveInfinity).Min();
                attempts.Add(new JObject {
                    ["step"] = step,
                    ["nearestNonEnemyEdgeMeters"] = nearest,
                    ["radiusMeters"] = radius
                });
                if (nearest > radius) break;
            }
            return new JObject {
                ["movedUnits"] = new JArray(vrock.UniqueId, target.UniqueId),
                ["attempts"] = attempts
            };
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
            // Which limb of HarmfulToOwner is the operative one, observed
            // rather than assumed. The predicate calls an effect harmful if
            // the blueprint's own Harmful flag is set OR its caster is an
            // enemy of its owner. The first attempt looked for a
            // Harmful-flagged visible spell buff and found none at all among
            // 187 candidates in this installation, so that flag cannot carry
            // the distinction here and the caster relationship is what does -
            // which is exactly the case the Sprint 36 review raised: an enemy
            // whose only dispellable magic is harmful to it.
            BlueprintBuff[] flagged = candidates
                .Where(value => value.Harmful).ToArray();
            BlueprintBuff carrier = candidates.FirstOrDefault();
            check("babau-dispel-candidates", carrier != null, new JObject {
                    ["searchedBuffs"] = candidates.Length,
                    ["harmfulFlaggedFound"] = flagged.Length,
                    ["harmfulFlaggedNames"] = new JArray(flagged
                        .Select(value => value.name).Take(8)),
                    ["carrier"] = carrier?.name,
                    ["carrierGuid"] = carrier?.AssetGuid,
                    ["carrierHarmfulFlag"] = carrier?.Harmful,
                    ["searchScope"] =
                        "Loaded non-KMG spell buffs of this installation that are visible and dispellable.",
                    ["method"] =
                        "One carrier effect is used for both controls and only its caster varies, so the observation isolates the caster-relationship limb of HarmfulToOwner instead of relying on a blueprint flag this installation does not set on visible spell buffs."
                }, "A visible dispellable spell effect exists to carry both controls, and the Harmful-flag population is recorded rather than assumed.");

            // Same effect in all three; only its presence and its caster
            // vary. Enemy-cast is harmful to its owner and must not be a
            // dispel target; self-cast is not, and must be.
            var observations = new[] {
                new { Label = "no-useful-effect", Effect = (BlueprintBuff)null,
                    Caster = (UnitEntityData)null, Expect = false },
                new { Label = "harmful-enemy-cast", Effect = carrier,
                    Caster = ally, Expect = false },
                new { Label = "beneficial-self-cast", Effect = carrier,
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
            private readonly List<UnitCommand> _acted;

            internal VrockCommandWatcher37(UnitEntityData owner,
                BlueprintAbility[] watched, List<UnitUseAbility> chosen,
                List<UnitCommand> acted)
            {
                _owner = owner; _watched = watched; _chosen = chosen;
                _acted = acted;
            }

            public void HandleUnitCommandDidStart(UnitCommand command)
            {
                if (command == null ||
                    !ReferenceEquals(command.Executor, _owner)) return;
                _acted.Add(command);
                var use = command as UnitUseAbility;
                if (use == null) return;
                BlueprintAbility blueprint = use.Spell?.Blueprint;
                if (blueprint != null && _watched.Contains(blueprint))
                    _chosen.Add(use);
            }

            public void HandleUnitCommandDidEnd(UnitCommand command) { }
        }
    }
}
