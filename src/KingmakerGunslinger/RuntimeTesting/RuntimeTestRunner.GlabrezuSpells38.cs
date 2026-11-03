using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The spell-like abilities that are not the single daily word: three
        /// at-will spells with no resource of any kind, and the two alignment
        /// bursts at their printed difficulty classes. All five are the
        /// installed native spells, so what is observed is that this creature
        /// delivers them at its own caster level and frequency - never that
        /// this project reimplemented their effects.
        /// </summary>
        private IEnumerable<int> ReviewGlabrezuSpells38(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint38GlabrezuSpellsPhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetWill = target.Stats.SaveWill.BaseValue;
            int oldTargetDamage = target.Damage;
            Vector3 oldTargetPosition = target.Position;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint38-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint38 glabrezu spells cell failed: " + id);
                };

            try
            {
                string p = ExpandedSummoningSpecialBuilder.Glabrezu38Prefix;
                var confusion = GlabrezuSpell38(p + "Confusion");
                var dispel = GlabrezuSpell38(p + "DispelMagic");
                var mirror = GlabrezuSpell38(p + "MirrorImage");
                var chaos = GlabrezuSpell38(p + "ChaosHammer");
                var unholy = GlabrezuSpell38(p + "UnholyBlight");

                GlabrezuAtWillDeclarations38(new[] { confusion, dispel,
                    mirror }, check);

                UnitEntityData glabrezu = CastExpandedSummoningOwnTier(
                    fixture, GlabrezuVisualPolicy38.Glabrezu);
                owned.Add(glabrezu);
                VrockQuiesce37(glabrezu);
                foreach (int frame in Outsider30NativeAppearance(glabrezu,
                    glabrezu.Blueprint.name)) yield return frame;

                target.Stats.HitPoints.BaseValue = 100000;
                target.Stats.SaveWill.BaseValue = -100;
                target.Damage = 0;
                PlaceExpandedSummoningUnit(target, glabrezu.Position +
                    Vector3.right *
                    (glabrezu.Corpulence + target.Corpulence + 1f));
                glabrezu.Memory.Add(target).Visible = true;
                target.Memory.Add(glabrezu).Visible = true;

                foreach (int frame in GlabrezuAtWillRepeats38(glabrezu,
                    target, confusion, mirror, check)) yield return frame;
                foreach (int frame in GlabrezuBurstDifficulty38(glabrezu,
                    target, chaos, unholy, check)) yield return frame;
            }
            finally
            {
                foreach (Kingmaker.UnitLogic.Buffs.Buff buff in
                    target.Buffs.Enumerable.ToArray())
                    if (buff.Blueprint != null && buff.Blueprint.name
                        .StartsWith("KMG_", StringComparison.Ordinal))
                        buff.Remove();
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                target.Stats.SaveWill.BaseValue = oldTargetWill;
                target.Damage = oldTargetDamage;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint38-glabrezu-spells.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        private static BlueprintAbility GlabrezuSpell38(string symbol)
        {
            return (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                .BySymbol[symbol];
        }

        /// <summary>
        /// At will means no resource at all. The Sprint 37 Vrock's single-use
        /// Mirror Image resource is deliberately not inherited: a previous
        /// creature's frequency is not this creature's, and the printed entry
        /// lists these three as at will.
        /// </summary>
        private static void GlabrezuAtWillDeclarations38(
            BlueprintAbility[] spells,
            Action<string, bool, JObject, string> check)
        {
            var rows = new JArray();
            bool coherent = true;
            foreach (BlueprintAbility spell in spells)
            {
                AbilityResourceLogic[] costs = spell
                    .GetComponents<AbilityResourceLogic>().ToArray();
                bool row = costs.Length == 0 &&
                    spell.Type == AbilityType.SpellLike;
                coherent &= row;
                rows.Add(new JObject {
                    ["ability"] = spell.name,
                    ["abilityGuid"] = spell.AssetGuid,
                    ["type"] = spell.Type.ToString(),
                    ["resourceComponents"] = costs.Length,
                    ["resources"] = new JArray(costs.Select(value =>
                        value.RequiredResource?.name)),
                    ["coherent"] = row
                });
            }
            check("spells-at-will-carry-no-resource", coherent, new JObject {
                ["observations"] = rows,
                ["contrast"] =
                    "The released Sprint 37 Vrock gives Mirror Image a single-use resource because its printed entry says once per day. This creature's printed entry says at will, so no resource is attached and none is inherited."
            }, "Confusion, dispel magic and mirror image are spell-like abilities with no resource component at all, as the printed at-will entry requires.");
        }

        /// <summary>
        /// At will, exercised rather than declared: each ability is cast twice
        /// in succession and must remain available throughout, with the single
        /// daily word's own resource untouched by any of it.
        /// </summary>
        private IEnumerable<int> GlabrezuAtWillRepeats38(
            UnitEntityData glabrezu, UnitEntityData target,
            BlueprintAbility confusion, BlueprintAbility mirror,
            Action<string, bool, JObject, string> check)
        {
            BlueprintAbilityResource stunResource =
                GlabrezuStunResource38();
            var rows = new JArray();
            bool coherent = true;
            var pairs = new[] {
                new { Label = "confusion", Ability = confusion,
                    Target = target },
                new { Label = "mirror-image", Ability = mirror,
                    Target = glabrezu }
            };
            foreach (var pair in pairs)
            {
                var casts = new JArray();
                bool row = true;
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    JObject before = VrockCastReadiness37(glabrezu,
                        pair.Ability, new TargetWrapper(pair.Target));
                    UnitUseAbility command = null;
                    foreach (int frame in Sprint26Cast(glabrezu, pair.Ability,
                        new TargetWrapper(pair.Target),
                        value => command = value)) yield return frame;
                    JObject after = VrockCastReadiness37(glabrezu,
                        pair.Ability, new TargetWrapper(pair.Target));
                    bool one = (bool)before["isAvailable"] &&
                        command != null && command.IsFinished &&
                        command.Result == UnitCommand.ResultType.Success &&
                        (bool)after["isAvailable"];
                    row &= one;
                    casts.Add(new JObject {
                        ["attempt"] = attempt,
                        ["readinessBefore"] = before,
                        ["command"] = command == null ? null :
                            Sprint27AiCommand(command),
                        ["availableAfter"] = after["isAvailable"],
                        ["coherent"] = one
                    });
                    // Mirror image stands on the caster and the released AI
                    // declines to re-queue over itself; the player path does
                    // not, so the effect is cleared between the two casts to
                    // observe availability rather than stacking.
                    if (ReferenceEquals(pair.Target, glabrezu))
                        foreach (Kingmaker.UnitLogic.Buffs.Buff buff in
                            glabrezu.Buffs.Enumerable.ToArray())
                            if (buff.Context != null && ReferenceEquals(
                                buff.Context.SourceAbility, pair.Ability))
                                buff.Remove();
                }
                coherent &= row;
                rows.Add(new JObject {
                    ["ability"] = pair.Label,
                    ["abilityGuid"] = pair.Ability.AssetGuid,
                    ["recipient"] = pair.Target.UniqueId,
                    ["casts"] = casts,
                    ["coherent"] = row
                });
            }
            int stunHeld = glabrezu.Descriptor.Resources
                .GetResourceAmount(stunResource);
            check("spells-at-will-repeat",
                coherent &&
                stunHeld == GlabrezuRules38Policy.PowerWordStunUses,
                new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["observations"] = rows,
                    ["powerWordStunRemaining"] = stunHeld,
                    ["expectedPowerWordStunRemaining"] =
                        GlabrezuRules38Policy.PowerWordStunUses
                }, "Each at-will spell casts twice in succession and stays available, and none of it touches the single Power Word Stun use.");
        }

        /// <summary>
        /// The two alignment bursts at their printed difficulty classes. The
        /// difficulty is derived - 10 + spell level + Charisma bonus - and the
        /// printed numbers are what the derivation has to reproduce, so a
        /// changed Charisma or spell level shows up here rather than silently
        /// matching a constant.
        /// </summary>
        private IEnumerable<int> GlabrezuBurstDifficulty38(
            UnitEntityData glabrezu, UnitEntityData target,
            BlueprintAbility chaos, BlueprintAbility unholy,
            Action<string, bool, JObject, string> check)
        {
            var rows = new JArray();
            bool coherent = true;
            var pairs = new[] {
                new { Label = "chaos-hammer", Ability = chaos,
                    Level = GlabrezuRules38Policy.ChaosHammerSpellLevel,
                    Printed = GlabrezuRules38Policy.PrintedChaosHammerDc },
                new { Label = "unholy-blight", Ability = unholy,
                    Level = GlabrezuRules38Policy.UnholyBlightSpellLevel,
                    Printed = GlabrezuRules38Policy.PrintedUnholyBlightDc }
            };
            foreach (var pair in pairs)
            {
                var watcher = new GlabrezuStunWatcher38(glabrezu);
                EventBus.Subscribe(watcher);
                UnitUseAbility command = null;
                try
                {
                    foreach (int frame in Sprint26Cast(glabrezu, pair.Ability,
                        new TargetWrapper(target), value => command = value))
                        yield return frame;
                }
                finally { EventBus.Unsubscribe(watcher); }
                int[] difficulties = watcher.Saves
                    .Select(value => value.DifficultyClass).ToArray();
                int[] casterLevels = watcher.Saves
                    .Select(value => value.Reason.Context == null ? -1 :
                        value.Reason.Context.Params.CasterLevel).ToArray();
                int derived = GlabrezuRules38Policy.SpellDifficultyClass(
                    pair.Level, GlabrezuRules38Policy.CharismaBonus);
                bool row = command != null && command.IsFinished &&
                    command.Result == UnitCommand.ResultType.Success &&
                    difficulties.Length >= 1 &&
                    difficulties.All(value => value == pair.Printed) &&
                    derived == pair.Printed &&
                    casterLevels.All(value => value ==
                        GlabrezuRules38Policy.SpellLikeCasterLevel);
                coherent &= row;
                rows.Add(new JObject {
                    ["ability"] = pair.Label,
                    ["abilityGuid"] = pair.Ability.AssetGuid,
                    ["spellLevel"] = pair.Level,
                    ["printedDifficultyClass"] = pair.Printed,
                    ["derivedDifficultyClass"] = derived,
                    ["observedDifficultyClasses"] = new JArray(difficulties),
                    ["effectCasterLevels"] = new JArray(casterLevels),
                    ["expectedCasterLevel"] =
                        GlabrezuRules38Policy.SpellLikeCasterLevel,
                    ["derivation"] =
                        "10 + spell level + Charisma bonus, with this creature's printed Charisma 20 giving a bonus of 5.",
                    ["command"] = command == null ? null :
                        Sprint27AiCommand(command),
                    ["coherent"] = row
                });
            }
            check("spells-bursts-at-printed-difficulty", coherent,
                new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["recipient"] = target.UniqueId,
                    ["observations"] = rows
                }, "Chaos hammer and unholy blight each roll a real saving throw at their printed difficulty class and at caster level 14, with the difficulty derived rather than written down.");
        }

        /// <summary>
        /// The correction this sprint exists for, observed with real
        /// candidates: Power Word Stun's suitability sits on the candidate
        /// path, not only on the deciding unit.
        ///
        /// <para>Two live enemies are presented, one the printed word can
        /// affect and one it cannot. An actor-only consideration would admit
        /// the action because SOME enemy is eligible and then let the brain
        /// select either, spending the single daily use on a creature the
        /// printed spell does nothing to. <c>AiAction.ScoreTarget</c>
        /// multiplies every target consideration into that candidate's own
        /// score, so the unaffectable candidate must score zero and be
        /// removed rather than merely discouraged.</para>
        /// </summary>
        private IEnumerable<int> ReviewGlabrezuAi38(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint38GlabrezuAiPhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetDamage = target.Damage;
            Vector3 oldTargetPosition = target.Position;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint38-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint38 glabrezu AI cell failed: " + id);
                };

            try
            {
                BlueprintAbility stun = GlabrezuStunAbility38();
                string p = ExpandedSummoningSpecialBuilder.Glabrezu38Prefix;
                var chaos = GlabrezuSpell38(p + "ChaosHammer");
                var unholy = GlabrezuSpell38(p + "UnholyBlight");
                var confusion = GlabrezuSpell38(p + "Confusion");
                var mirror = GlabrezuSpell38(p + "MirrorImage");
                BlueprintAbility[] watched = { stun, chaos, unholy, confusion,
                    mirror };

                UnitEntityData glabrezu = CastExpandedSummoningOwnTier(
                    fixture, GlabrezuVisualPolicy38.Glabrezu);
                owned.Add(glabrezu);
                VrockQuiesce37(glabrezu);
                foreach (int frame in Outsider30NativeAppearance(glabrezu,
                    glabrezu.Blueprint.name)) yield return frame;

                // A second real enemy, made hostile exactly the way the
                // released Sprint 34 maneuver fixture makes one: its own
                // private group, the fixture's owned hostile faction, and no
                // change to the summoner's shared group.
                UnitEntityData second = CastExpandedSummoningOwnTier(fixture,
                    GlabrezuVisualPolicy38.Glabrezu);
                owned.Add(second);
                VrockQuiesce37(second);
                JObject hostility = GlabrezuMakeHostile38(fixture, second);
                check("ai-second-enemy-is-real",
                    (bool)hostility["isEnemyOfOwner"] &&
                    (bool)hostility["privateGroup"] &&
                    (bool)hostility["summonerGroupUnchanged"],
                    hostility,
                    "The second candidate is a genuinely hostile unit in its own private group, and the summoner's shared group is untouched.");

                // Affectable: inside the printed bucket. Unaffectable: above
                // the printed ceiling, where the word does nothing at all.
                target.Stats.HitPoints.BaseValue =
                    GlabrezuRules38Policy.PowerWordStunLowThreshold;
                target.Damage = 0;
                second.Stats.HitPoints.BaseValue = 100000;
                second.Damage = 0;
                PlaceExpandedSummoningUnit(target, glabrezu.Position +
                    Vector3.right *
                    (glabrezu.Corpulence + target.Corpulence + 1f));
                PlaceExpandedSummoningUnit(second, glabrezu.Position +
                    Vector3.left *
                    (glabrezu.Corpulence + second.Corpulence + 1f));
                foreach (UnitEntityData enemy in new[] { target, second })
                {
                    glabrezu.Memory.Add(enemy).Visible = true;
                    enemy.Memory.Add(glabrezu).Visible = true;
                    if (glabrezu.Group != null)
                        glabrezu.Group.Memory.Add(enemy).Visible = true;
                }
                bool affectable = GlabrezuStunSuitability38
                    .IsUsableRecipient(glabrezu, target, stun);
                bool unaffectable = GlabrezuStunSuitability38
                    .IsUsableRecipient(glabrezu, second, stun);
                check("ai-candidates-differ",
                    affectable && !unaffectable,
                    new JObject {
                        ["owner"] = glabrezu.UniqueId,
                        ["affectableCandidate"] = target.UniqueId,
                        ["affectableHitPoints"] = target.HPLeft,
                        ["affectableIsUsable"] = affectable,
                        ["unaffectableCandidate"] = second.UniqueId,
                        ["unaffectableHitPoints"] = second.HPLeft,
                        ["unaffectableIsUsable"] = unaffectable,
                        ["ceiling"] = GlabrezuRules38Policy
                            .PowerWordStunHitPointCeiling
                    }, "One live enemy is inside the printed bucket and the other is above the printed ceiling, so the two candidates genuinely differ before the brain is asked.");

                foreach (int frame in GlabrezuAiStunTarget38(glabrezu, target,
                    second, watched, stun, check)) yield return frame;
            }
            finally
            {
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                target.Damage = oldTargetDamage;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory, "sprint38-glabrezu-ai.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        /// <summary>
        /// Makes one disposable fixture unit genuinely hostile, following the
        /// released Sprint 34 maneuver fixture exactly: a private group of its
        /// own, the fixture's owned hostile faction, and no mutation of the
        /// summoner's shared group.
        /// </summary>
        private static JObject GlabrezuMakeHostile38(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData unit)
        {
            if (fixture.OwnedHostileFaction == null)
                throw new InvalidOperationException(
                    "The fixture owns no hostile faction to switch into.");
            string casterGroup = fixture.Caster.GroupId;
            unit.GroupId = unit.UniqueId;
            var group = unit.Group;
            bool privateGroup = !group.IsPlayerParty && group.Count == 1 &&
                ReferenceEquals(group[0], unit);
            unit.Descriptor.SwitchFactions(fixture.OwnedHostileFaction, true);
            unit.AttackFactions.Match(
                new[] { fixture.Caster.Blueprint.Faction });
            group.ResetFactionSet();
            group.UpdateAttackFactionsCache();
            return new JObject {
                ["unit"] = unit.UniqueId,
                ["faction"] = unit.Faction?.name,
                ["privateGroup"] = privateGroup,
                ["summonerGroupUnchanged"] =
                    fixture.Caster.GroupId == casterGroup,
                ["isEnemyOfCaster"] = unit.IsEnemy(fixture.Caster),
                ["isEnemyOfOwner"] = unit.IsEnemy(fixture.Caster)
            };
        }

        /// <summary>
        /// Drives the real brain with both candidates present and records which
        /// one the chosen Power Word Stun command actually targeted. A chosen
        /// ability is not enough here: the whole correction is about which
        /// candidate it lands on.
        /// </summary>
        private IEnumerable<int> GlabrezuAiStunTarget38(
            UnitEntityData glabrezu, UnitEntityData affectable,
            UnitEntityData unaffectable, BlueprintAbility[] watched,
            BlueprintAbility stun,
            Action<string, bool, JObject, string> check)
        {
            var chosen = new List<UnitUseAbility>();
            var acted = new List<UnitCommand>();
            var watcher = new VrockCommandWatcher37(glabrezu, watched, chosen,
                acted);
            EventBus.Subscribe(watcher);
            JObject brain = VrockPrepareBrain37(glabrezu, affectable);
            if (glabrezu.Group != null)
                glabrezu.Group.Memory.Add(unaffectable).Visible = true;
            var queued = new JArray();
            Func<UnitUseAbility[]> stunPicks = () => chosen
                .Where(value => ReferenceEquals(value.Spell?.Blueprint, stun))
                .ToArray();
            try
            {
                foreach (int frame in VrockDriveBrain37(glabrezu,
                    () => stunPicks().Length > 0, 40, brain))
                    yield return frame;
                foreach (UnitCommand command in glabrezu.Commands.Raw
                    .Where(value => value != null).ToArray())
                    queued.Add(Sprint27AiCommand(command));
            }
            finally
            {
                EventBus.Unsubscribe(watcher);
                Sprint28SetBrainActivePreservingActions(glabrezu, false);
                glabrezu.Commands.InterruptAll(true);
                glabrezu.Commands.RemoveFinishedAndUpdateQueue();
            }
            UnitUseAbility[] picks = stunPicks();
            string[] stunTargets = picks
                .Select(value => value.Target?.Unit?.UniqueId)
                .Where(value => value != null).Distinct().ToArray();
            bool pass = picks.Length > 0 && acted.Count > 0 &&
                stunTargets.Length == 1 &&
                stunTargets[0] == affectable.UniqueId;
            check("ai-stun-picks-the-affectable-candidate", pass,
                new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["brain"] = brain,
                    ["affectableCandidate"] = affectable.UniqueId,
                    ["unaffectableCandidate"] = unaffectable.UniqueId,
                    ["stunCommands"] = picks.Length,
                    ["stunTargets"] = new JArray(stunTargets),
                    ["chosenInOrder"] = new JArray(chosen.Select(value =>
                        value.Spell?.Blueprint?.name + "->" +
                        value.Target?.Unit?.UniqueId)),
                    ["commandsStarted"] = acted.Count,
                    ["queuedAfter"] = queued,
                    ["whyTargetAndNotActor"] =
                        "The engine's own tooltip for ActorConsiderations is \"Considerations that only depend on deciding unit\". An actor-only suitability would be satisfied because SOME enemy is eligible and then let the brain select the other one, spending the single daily use on a creature the printed word does nothing to. AiAction.ScoreTarget multiplies each target consideration into that candidate's score, so a zero removes the candidate."
                }, "With one affectable and one unaffectable live enemy, the brain chooses Power Word Stun and targets the affectable one, never the candidate above the printed ceiling.");
        }

        /// <summary>
        /// The one public route, its medallion and the real player path through
        /// it. This is the phase that carries final qualification for the
        /// public surface.
        /// </summary>
        private void ReviewGlabrezuPublic38(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(route => route.Creature.Key ==
                    GlabrezuVisualPolicy38.Glabrezu)
                .OrderBy(route => route.ParentTier).ToArray();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint38GlabrezuPublicPhase,
                ["finalQualification"] = true,
                ["cells"] = cells
            };
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint38-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint38 glabrezu public cell failed: " + id);
                };
            try
            {
                bool exact = routes.Length == 1 &&
                    routes[0].ParentTier == 9 &&
                    routes[0].Multiplicity == SummonMultiplicity.One &&
                    SummonVisibilityCatalog.IsPublished(routes[0]) &&
                    !routes[0].Creature.MonsterTemplated &&
                    !ExpandedSummoningCatalog
                        .GenerateVariants(SummonFamily.NaturesAlly)
                        .Any(route => route.Creature.Key ==
                            GlabrezuVisualPolicy38.Glabrezu);
                check("public-route-scope", exact, new JObject {
                    ["key"] = GlabrezuVisualPolicy38.Glabrezu,
                    ["routes"] = new JArray(routes.Select(route =>
                        new JObject {
                            ["key"] = route.StableKey,
                            ["tier"] = route.ParentTier,
                            ["quantity"] = route.Multiplicity.ToString(),
                            ["published"] =
                                SummonVisibilityCatalog.IsPublished(route),
                            ["templated"] = route.Creature.MonsterTemplated
                        })),
                    ["naturesAllyRoutes"] = 0
                }, "Exactly one untemplated quantity-one Summon Monster IX route carries the Glabrezu, and no Nature's Ally route does.");
                if (!exact) throw new InvalidOperationException(
                    "Sprint38 public route scope is not ready.");

                var icon = ExpandedSummoningProjectIcons.Require(
                    GlabrezuVisualPolicy38.Glabrezu);
                var consumers = routes.Select(route => {
                    var root = ExpandedSummoningRoot(fixture.Blueprints,
                        route);
                    return new JObject {
                        ["route"] = route.StableKey,
                        ["ability"] = root.AssetGuid,
                        ["sprite"] = root.Icon?.GetInstanceID(),
                        ["expectedSprite"] = icon.GetInstanceID()
                    };
                }).ToArray();
                check("public-icon-glabrezu",
                    icon.rect.width == 128 && icon.rect.height == 128 &&
                    consumers.All(consumer =>
                        (int?)consumer["sprite"] == icon.GetInstanceID()) &&
                    ExpandedSummoningProjectIcons.FallbackCount == 0,
                    new JObject {
                        ["key"] = GlabrezuVisualPolicy38.Glabrezu,
                        ["consumers"] = new JArray(consumers),
                        ["loadedSprite"] = CaptureSprint26ReferenceIcon(icon,
                            "s38-glabrezu"),
                        ["captureScope"] =
                            "Actual loaded project sprite pixels; not menu navigation or owner aesthetic approval."
                    }, "The public route carries the Glabrezu's own 128px medallion without any fallback.");

                Sprint24PlayerRoutes(fixture, routes,
                    (id, pass, row, description) => check(
                        "public-" + id, pass, row, description));
            }
            finally
            {
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint38-glabrezu-public.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }
    }
}
