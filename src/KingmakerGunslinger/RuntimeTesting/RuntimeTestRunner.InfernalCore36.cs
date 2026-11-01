using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Assets;
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
        /// One save-free hidden-candidate run for the unfamiliar Sprint 36
        /// boundaries. It observes production registration, bodies, commands,
        /// rule events and exact-source effects. The only direct gameplay
        /// dispatch is SummonGrabComponent.TryGrab with a known qualifying hit;
        /// that established fixture seam still runs the real native grapple
        /// check and the production hold graph.
        /// </summary>
        private IEnumerable<int> ReviewInfernalCore36(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var trace = new InfernalEvents34();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint36InfernalCorePhase,
                ["finalQualification"] = false,
                ["cells"] = cells,
                ["events"] = trace.Rows
            };
            UnitEntityData babau = null;
            UnitEntityData hostileBabau = null;
            UnitEntityData hezrou = null;
            UnitEntityData bone = null;
            UnitEntityData target = fixture.Hostile;
            UnitEntityData ally = fixture.Caster;
            BlueprintFaction targetFaction = fixture.OwnedHostileFaction;
            BlueprintFaction[] oldAttackFactions = targetFaction?
                .AttackFactions;
            int oldTargetAc = target.Stats.AC.BaseValue;
            int oldTargetFort = target.Stats.SaveFortitude.BaseValue;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetDamage = target.Damage;
            Size oldTargetSize = target.Descriptor.State.Size;
            int oldAllyFort = ally.Stats.SaveFortitude.BaseValue;
            Vector3 oldTargetPosition = target.Position;
            Vector3 oldAllyPosition = ally.Position;
            float oldTargetOrientation = target.Orientation;
            float oldAllyOrientation = ally.Orientation;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint36-" + id, pass, row,
                        description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint36 infernal core cell failed: " + id);
                };

            try
            {
                InfernalCoreBootstrap36(check);

                babau = CastExpandedSummoningOwnTier(fixture, "babau");
                hostileBabau = CastExpandedSummoningOwnTier(fixture,
                    "babau");
                // Stench may evaluate as soon as the Hezrou enters the
                // spatial grid. Establish the legal failed-save precondition
                // before spawning its source, not after an earlier successful
                // exposure could have installed 24-hour source memory.
                ally.Stats.SaveFortitude.BaseValue = -100;
                hezrou = CastExpandedSummoningOwnTier(fixture, "hezrou");
                owned.Add(babau);
                owned.Add(hostileBabau);
                owned.Add(hezrou);
                foreach (UnitEntityData actor in owned)
                {
                    Sprint28SetBrainActivePreservingActions(actor, false);
                    actor.Commands.InterruptAll(true);
                    actor.Commands.RemoveFinishedAndUpdateQueue();
                    actor.Stats.HitPoints.BaseValue = 100000;
                    foreach (int frame in Outsider30NativeAppearance(actor,
                        actor.Blueprint.name)) yield return frame;
                }

                InfernalCoreProfiles36(babau, hezrou, check);

                if (targetFaction == null || !ReferenceEquals(target.Faction,
                        targetFaction))
                    throw new InvalidOperationException(
                        "Sprint36 hostility fixture does not own its faction.");
                // A summoned unit initially shares its summoner's combat
                // group.  Give only this disposable attacker its own group
                // before switching faction, then refresh the same per-unit
                // attack-faction cache used by the qualified Infernal34
                // maneuver observer.  Mutating the owned faction blueprint
                // alone does not refresh an existing group's relationship
                // cache and produced a false non-hostile precondition.
                // The fixture's request-owned hostile already has a real
                // native empty-hand weapon and no demon acid resistance. Use
                // that isolated disposable actor for the unarmed positive;
                // introducing an unrelated summon would add an unnecessary
                // body/appearance precondition to this mechanics observer.
                UnitEntityData[] attackers = { hostileBabau, target };
                foreach (UnitEntityData attacker in attackers)
                {
                    attacker.GroupId = attacker.UniqueId;
                    var hostileGroup = attacker.Group;
                    if (hostileGroup.IsPlayerParty ||
                        hostileGroup.Count != 1 ||
                        !ReferenceEquals(hostileGroup[0], attacker))
                        throw new InvalidOperationException(
                            "Sprint36 Slime attacker does not own a private group.");
                    attacker.Descriptor.SwitchFactions(targetFaction, true);
                    attacker.AttackFactions.Match(new[] { babau.Faction });
                    hostileGroup.ResetFactionSet();
                    hostileGroup.UpdateAttackFactionsCache();
                }
                bool hostile = attackers.All(attacker =>
                    attacker.IsEnemy(babau) && babau.IsEnemy(attacker));
                check("private-hostility", hostile, new JObject {
                    ["attacker"] = hostileBabau.UniqueId,
                    ["naturalAttacker"] = target.UniqueId,
                    ["defender"] = babau.UniqueId,
                    ["attackerFaction"] = hostileBabau.Faction.name,
                    ["defenderFaction"] = babau.Faction.name,
                    ["attackerIsEnemy"] = hostileBabau.IsEnemy(babau),
                    ["defenderIsEnemy"] = babau.IsEnemy(hostileBabau),
                    ["naturalAttackerIsEnemy"] =
                        target.IsEnemy(babau),
                    ["defenderIsNaturalAttackerEnemy"] =
                        babau.IsEnemy(target),
                    ["originalAttackFactions"] = new JArray(
                        (oldAttackFactions ?? Array.Empty<BlueprintFaction>())
                            .Select(value => value.name)),
                    ["activeAttackFactions"] = new JArray(
                        hostileBabau.AttackFactions.Select(value =>
                            value.name))
                }, "The request-owned faction alone establishes a real two-way enemy relation; no intake or production faction changes.");

                int babauAc = babau.Stats.AC.BaseValue;
                int attackerReflex = hostileBabau.Stats.SaveReflex.BaseValue;
                int naturalReflex = target.Stats.SaveReflex.BaseValue;
                babau.Stats.AC.BaseValue = -100;
                hostileBabau.Stats.SaveReflex.BaseValue = -100;
                target.Stats.SaveReflex.BaseValue = -100;
                try
                {
                    foreach (int frame in InfernalCoreSlime36(fixture,
                        hostileBabau, target, babau, trace,
                        check))
                        yield return frame;
                }
                finally
                {
                    babau.Stats.AC.BaseValue = babauAc;
                    hostileBabau.Stats.SaveReflex.BaseValue =
                        attackerReflex;
                    target.Stats.SaveReflex.BaseValue = naturalReflex;
                }

                bone = CastExpandedSummoningOwnTier(fixture, "bone-devil");
                owned.Add(bone);
                Sprint28SetBrainActivePreservingActions(bone, false);
                bone.Commands.InterruptAll(true);
                bone.Commands.RemoveFinishedAndUpdateQueue();
                foreach (int frame in Outsider30NativeAppearance(bone,
                    bone.Blueprint.name)) yield return frame;
                foreach (int frame in InfernalCoreDispel36(fixture,
                    hostileBabau, bone, trace, check))
                    yield return frame;

                PlaceExpandedSummoningUnit(hezrou, ally.Position +
                    Vector3.right * (hezrou.Corpulence + ally.Corpulence +
                        .3f));
                foreach (int frame in InfernalCoreConditions36(fixture,
                    hezrou, target, ally, trace, check))
                    yield return frame;
            }
            finally
            {
                target.Stats.AC.BaseValue = oldTargetAc;
                target.Stats.SaveFortitude.BaseValue = oldTargetFort;
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                target.Damage = oldTargetDamage;
                target.Descriptor.State.Size = oldTargetSize;
                ally.Stats.SaveFortitude.BaseValue = oldAllyFort;
                if (targetFaction != null)
                    targetFaction.AttackFactions = oldAttackFactions;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                target.Orientation = oldTargetOrientation;
                PlaceExpandedSummoningUnit(ally, oldAllyPosition);
                ally.Orientation = oldAllyOrientation;
                trace.Dispose();
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint36-infernal-core.json"), receipt.ToString(
                        Formatting.Indented));
                foreach (UnitEntityData actor in owned.Where(value =>
                    value != null && !value.Destroyed))
                {
                    actor.Commands.InterruptAll(true);
                    actor.Commands.RemoveFinishedAndUpdateQueue();
                    if (actor.IsInCombat) actor.LeaveCombat();
                }
                Game.Instance.Player.UpdateIsInCombat();
                DisposeExpandedSummoningUnits(fixture.Created, owned);
                ResetExpandedSummoningHostile(fixture);
            }
        }

        private static void InfernalCoreBootstrap36(
            Action<string, bool, JObject, string> check)
        {
            var routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster).Where(value =>
                    value.Creature.Key == "babau" ||
                    value.Creature.Key == "hezrou").OrderBy(value =>
                        value.Creature.Key).ThenBy(value =>
                            value.ParentTier).ToArray();
            var expected = ExpandedSummoningIdentityCatalog.Build().Where(
                value => value.Symbol.StartsWith(
                        ExpandedSummoningSpecialBuilder.Babau36Prefix,
                        StringComparison.Ordinal) ||
                    value.Symbol.StartsWith(
                        ExpandedSummoningSpecialBuilder.Hezrou36Prefix,
                        StringComparison.Ordinal)).OrderBy(value =>
                            value.Symbol, StringComparer.Ordinal).ToArray();
            var actual = ExpandedSummoningBlueprints.Registered.BySymbol
                .Where(pair => pair.Key.StartsWith(
                        ExpandedSummoningSpecialBuilder.Babau36Prefix,
                        StringComparison.Ordinal) || pair.Key.StartsWith(
                        ExpandedSummoningSpecialBuilder.Hezrou36Prefix,
                        StringComparison.Ordinal)).ToDictionary(pair =>
                            pair.Key, pair => pair.Value,
                            StringComparer.Ordinal);
            var missing = expected.Where(value =>
                !actual.ContainsKey(value.Symbol)).Select(value =>
                    value.Symbol).ToArray();
            var wrongTypes = expected.Where(value =>
                actual.ContainsKey(value.Symbol) &&
                (actual[value.Symbol] == null || !String.Equals(
                    actual[value.Symbol].GetType().Name,
                    value.PlannedType, StringComparison.Ordinal))).Select(
                        value => new JObject {
                            ["symbol"] = value.Symbol,
                            ["expected"] = value.PlannedType,
                            ["actual"] = actual[value.Symbol] == null ?
                                null : actual[value.Symbol].GetType().Name
                        }).ToArray();
            bool exactRoutes = routes.Length == 7 &&
                routes.Count(value => value.Creature.Key == "babau") == 5 &&
                routes.Count(value => value.Creature.Key == "hezrou") == 2 &&
                routes.All(value =>
                    !SummonVisibilityCatalog.IsPublished(value));
            check("hidden-bootstrap", exactRoutes &&
                actual.Count == expected.Length && missing.Length == 0 &&
                wrongTypes.Length == 0, new JObject {
                    ["routes"] = new JArray(routes.Select(value =>
                        new JObject {
                            ["key"] = value.Creature.Key,
                            ["tier"] = value.ParentTier,
                            ["quantity"] = value.Multiplicity.ToString(),
                            ["published"] = SummonVisibilityCatalog
                                .IsPublished(value)
                        })),
                    ["expectedRegistrationCount"] = expected.Length,
                    ["actualRegistrationCount"] = actual.Count,
                    ["missing"] = new JArray(missing),
                    ["wrongTypes"] = new JArray(wrongTypes)
                }, "All seven routes remain hidden while every active Sprint 36 private dependency resolves at its planned type.");
        }

        private static void InfernalCoreProfiles36(UnitEntityData babau,
            UnitEntityData hezrou,
            Action<string, bool, JObject, string> check)
        {
            InfernalMartialProfile36 bp =
                InfernalMartialProfile36.For(babau);
            InfernalMartialProfile36 hp =
                InfernalMartialProfile36.For(hezrou);
            Mesh bm, hm;
            string[] bb, hb;
            Texture2D bt, ht;
            string bs, hs;
            bool bc = PteranodonAssetRuntime.TryGetInfernalVisual36(
                InfernalVisualPolicy36.Babau, out bm, out bb, out bt,
                out bs);
            bool hc = PteranodonAssetRuntime.TryGetInfernalVisual36(
                InfernalVisualPolicy36.Hezrou, out hm, out hb, out ht,
                out hs);
            string bv = ExpandedSummoningPteranodonViewPatch.DescribeView(
                babau.View);
            string hv = ExpandedSummoningPteranodonViewPatch.DescribeView(
                hezrou.View);
            var grab = SummonGrabComponent.Find(hezrou);
            ItemEntityWeapon babauBite = bp?.Bite == null ? null :
                babau.Body.AdditionalLimbs.Select(value =>
                    value.MaybeWeapon).SingleOrDefault(value =>
                    value != null && ReferenceEquals(value.Blueprint,
                        bp.Bite));
            ItemEntityWeapon babauClaw = bp?.Claw == null ? null :
                babau.Body.AdditionalLimbs.Select(value =>
                    value.MaybeWeapon).SingleOrDefault(value =>
                    value != null && ReferenceEquals(value.Blueprint,
                        bp.Claw));
            ItemEntityWeapon[] hezrouWeapons = new[] {
                hezrou.Body.PrimaryHand.MaybeWeapon }.Concat(
                    hezrou.Body.AdditionalLimbs.Select(value =>
                        value.MaybeWeapon)).Where(value => value != null)
                    .ToArray();
            bool exact = bp?.Bite != null && bp.Claw != null &&
                bp.Longspear != null && hp?.Bite != null &&
                hp.Claw != null && babauBite != null &&
                babauClaw != null && grab != null &&
                babau.Body.PrimaryHand.MaybeWeapon.AttackRange.Value == 10 &&
                babauBite.AttackRange.Value == 5 &&
                babauClaw.AttackRange.Value == 5 &&
                hezrouWeapons.Length == 3 && hezrouWeapons.All(value =>
                    value.AttackRange.Value == 10) &&
                grab.GrabWithPrimaryHand &&
                grab.GrabAdditionalLimbCount == 2 &&
                grab.MaxHeldTargets == 3 && grab.MaxTargetSizeDelta == 0 &&
                !grab.HasImpale && grab.ConstrictDiceCount == 0 &&
                bc && hc && bm != null && hm != null &&
                !ReferenceEquals(bm, hm) &&
                bv.StartsWith("visual:attached;", StringComparison.Ordinal) &&
                hv.StartsWith("visual:attached;", StringComparison.Ordinal);
            check("profiles-and-bodies", exact, new JObject {
                ["babau"] = InfernalCoreProfileRow36(babau, bp),
                ["hezrou"] = InfernalCoreProfileRow36(hezrou, hp),
                ["babauView"] = bv,
                ["hezrouView"] = hv,
                ["babauCache"] = bs,
                ["hezrouCache"] = hs,
                ["babauVertices"] = bm?.vertexCount,
                ["hezrouVertices"] = hm?.vertexCount,
                ["distinctMeshes"] = bm != null && hm != null &&
                    !ReferenceEquals(bm, hm),
                ["babauReachFeet"] = new JArray(
                    babau.Body.PrimaryHand.MaybeWeapon.AttackRange.Value,
                    babauClaw.AttackRange.Value,
                    babauBite.AttackRange.Value),
                ["hezrouReachFeet"] = new JArray(hezrouWeapons.Select(
                    value => value.AttackRange.Value)),
                ["grabPrimary"] = grab?.GrabWithPrimaryHand,
                ["grabAdditional"] = grab?.GrabAdditionalLimbCount,
                ["maxHeld"] = grab?.MaxHeldTargets,
                ["impale"] = grab?.HasImpale,
                ["constrictDice"] = grab?.ConstrictDiceCount
            }, "Both exact hidden units instantiate distinct attached original bodies and their own alternative-routine or rider-free Grab profiles.");
        }

        private static JObject InfernalCoreProfileRow36(UnitEntityData unit,
            InfernalMartialProfile36 profile)
        {
            return new JObject {
                ["owner"] = unit.UniqueId,
                ["blueprint"] = unit.Blueprint.AssetGuid,
                ["size"] = unit.Descriptor.State.Size.ToString(),
                ["hp"] = unit.Stats.HitPoints.ModifiedValue,
                ["ac"] = unit.Stats.AC.ModifiedValue,
                ["bab"] = unit.Stats.BaseAttackBonus.ModifiedValue,
                ["fortitude"] = unit.Stats.SaveFortitude.ModifiedValue,
                ["reflex"] = unit.Stats.SaveReflex.ModifiedValue,
                ["will"] = unit.Stats.SaveWill.ModifiedValue,
                ["perception"] = unit.Stats.SkillPerception.ModifiedValue,
                ["primary"] = unit.Body.PrimaryHand.MaybeWeapon?
                    .Blueprint?.AssetGuid,
                ["additional"] = new JArray(unit.Body.AdditionalLimbs
                    .Select(value => value.MaybeWeapon?.Blueprint?.AssetGuid)),
                ["additionalReachFeet"] = new JArray(
                    unit.Body.AdditionalLimbs.Select(value =>
                        value.MaybeWeapon?.AttackRange.Value)),
                ["bite"] = profile?.Bite?.AssetGuid,
                ["claw"] = profile?.Claw?.AssetGuid,
                ["longspear"] = profile?.Longspear?.AssetGuid,
                ["primaryReachFeet"] = unit.Body.PrimaryHand.MaybeWeapon?
                    .AttackRange.Value,
                ["abilities"] = new JArray(unit.Descriptor.Abilities
                    .Enumerable.Select(value => value.Blueprint.name)
                    .Where(value => value.StartsWith("KMG_",
                        StringComparison.Ordinal)).OrderBy(value => value))
            };
        }

        private IEnumerable<int> InfernalCoreSlime36(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData manufacturedAttacker,
            UnitEntityData naturalAttacker, UnitEntityData defender,
            InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            InfernalMartialProfile36 profile =
                InfernalMartialProfile36.For(manufacturedAttacker);
            foreach (int frame in InfernalSwitch34(manufacturedAttacker,
                profile.Longspear)) yield return frame;
            foreach (int frame in InfernalCoreSlimeContact36(fixture,
                manufacturedAttacker, defender, trace, check,
                "manufactured", 8, false)) yield return frame;
            BlueprintItemWeapon natural = naturalAttacker.Body.PrimaryHand
                .MaybeWeapon?.Blueprint;
            if (natural == null || (!natural.IsNatural &&
                    !natural.IsUnarmed))
                throw new InvalidOperationException(
                    "Sprint36 Slime positive control lacks a natural or unarmed weapon.");
            int naturalLaneFeet = Math.Max(1,
                naturalAttacker.Body.PrimaryHand.MaybeWeapon.AttackRange
                    .Value - 1);
            foreach (int frame in InfernalCoreSlimeContact36(fixture,
                naturalAttacker, defender, trace, check, "natural",
                naturalLaneFeet, true)) yield return frame;
        }

        private IEnumerable<int> InfernalCoreSlimeContact36(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData attacker, UnitEntityData defender,
            InfernalEvents34 trace,
            Action<string, bool, JObject, string> check,
            string label, int feet, bool expectSlime)
        {
            var attempts = new JArray();
            bool observed = false;
            for (int attempt = 0; attempt < 4 && !observed; attempt++)
            {
                attacker.Commands.InterruptAll(true);
                attacker.Commands.RemoveFinishedAndUpdateQueue();
                InfernalCombatLane34(fixture, attacker, defender, feet);
                int start = trace.Rows.Count;
                var command = new UnitAttack(defender) {
                    IsSingleAttack = true,
                    ForceFullAttack = false
                };
                attacker.Commands.Run(command);
                foreach (int frame in WaitInfernalCommand34(command,
                    () => true, 25)) yield return frame;
                JObject[] attacks = trace.Rows.Skip(start).OfType<JObject>()
                    .Where(value => (string)value["event"] == "attack" &&
                    (string)value["owner"] == attacker.UniqueId &&
                    (string)value["target"] == defender.UniqueId).ToArray();
                bool hit = attacks.Any(value =>
                    (bool?)value["hit"] == true);
                JObject[] saves = trace.Rows.Skip(start).OfType<JObject>()
                    .Where(value => (string)value["event"] == "save" &&
                    (string)value["owner"] == defender.UniqueId &&
                    (string)value["target"] == attacker.UniqueId &&
                    (int?)value["dc"] ==
                        InfernalRules36Policy.BabauProtectiveSlimeDc)
                    .ToArray();
                JObject[] acid = trace.Rows.Skip(start).OfType<JObject>()
                    .Where(value => (string)value["event"] == "damage" &&
                    (string)value["owner"] == defender.UniqueId &&
                    (string)value["target"] == attacker.UniqueId &&
                    (bool?)value["hasAttackRoll"] == false &&
                    ((JArray)value["packets"]).Any(packet =>
                        (string)packet["energy"] == "Acid")).ToArray();
                var attemptRow = new JObject {
                    ["attempt"] = attempt + 1,
                    ["command"] = Sprint27AiCommand(command),
                    ["weapon"] = attacker.Body.PrimaryHand.MaybeWeapon?
                        .Blueprint?.AssetGuid,
                    ["weaponNatural"] = attacker.Body.PrimaryHand
                        .MaybeWeapon?.Blueprint?.IsNatural,
                    ["weaponUnarmed"] = attacker.Body.PrimaryHand
                        .MaybeWeapon?.Blueprint?.IsUnarmed,
                    ["attacks"] = new JArray(attacks),
                    ["saves"] = new JArray(saves),
                    ["acid"] = new JArray(acid)
                };
                attempts.Add(attemptRow);
                if (!hit) continue;
                observed = true;
                bool pass = command.Result ==
                        UnitCommand.ResultType.Success &&
                    attacks.Length == 1 &&
                    saves.Length == (expectSlime ? 1 : 0) &&
                    acid.Length == (expectSlime ? 1 : 0) &&
                    (!expectSlime || (bool?)saves[0]["passed"] == false) &&
                    (!expectSlime || (int?)acid[0]["actual"] > 0);
                check("protective-slime-" + label, pass,
                    new JObject {
                        ["attacker"] = attacker.UniqueId,
                        ["defender"] = defender.UniqueId,
                        ["expectSlime"] = expectSlime,
                        ["attempts"] = attempts,
                        ["forcedRolls"] = 0,
                        ["fixtureSaveBase"] = expectSlime ? -100 :
                            (int?)null
                    }, expectSlime ?
                    "A real natural hit reaches the target-side Slime consumer, makes one native DC18 Reflex save and deals one nonrecursive 1d8 acid packet on failure." :
                    "A real manufactured longspear hit causes no Slime save or wielder damage; the unsupported item branch is not replaced with Barbed Defense semantics.");
            }
            if (!observed) throw new InvalidOperationException(
                "Sprint36 " + label +
                " Slime contact precondition was unobserved after four native attempts.");
        }

        private IEnumerable<int> InfernalCoreDispel36(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData babau, UnitEntityData target,
            InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            string bonePrefix = ExpandedSummoningSpecialBuilder.Bone35Prefix;
            string babauPrefix = ExpandedSummoningSpecialBuilder.Babau36Prefix;
            var invisibility = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[bonePrefix + "Invisibility"];
            var invisible = (BlueprintBuff)ExpandedSummoningBlueprints
                .Registered.BySymbol[bonePrefix + "InvisibilityBuff"];
            var dispel = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[babauPrefix + "DispelMagic"];
            var ai = (BabauDispelAi36)ExpandedSummoningBlueprints.Registered
                .BySymbol[babauPrefix + "DispelAi"];
            InfernalCombatLane34(fixture, babau, target, 8);
            UnitUseAbility invisibilityCommand = null;
            foreach (int frame in Sprint26Cast(target, invisibility,
                new TargetWrapper(target), value =>
                    invisibilityCommand = value)) yield return frame;
            Buff effect = target.Buffs.Enumerable.SingleOrDefault(value =>
                ReferenceEquals(value.Blueprint, invisible) &&
                ReferenceEquals(value.Context?.MaybeCaster, target));
            var decision = new DecisionContext {
                Unit = babau,
                Target = new TargetInfo { Unit = target }
            };
            bool useful = effect != null && effect.IsFromSpell &&
                !effect.IsNotDispelable && effect.Context != null &&
                BabauDispelAi36.HasUsefulEffect(target);
            bool aiBefore = ai.IsAvailable(decision) &&
                ai.IsTargetValid(decision);
            bool resourceFree = !dispel.ComponentsArray.OfType<
                Kingmaker.UnitLogic.Abilities.Components.AbilityResourceLogic>()
                .Any();
            var attempts = new JArray();
            bool removed = false;
            int failedAttemptCount = 0;
            bool failedAttemptsThrottled = true;
            for (int attempt = 0; attempt < 8 && !removed; attempt++)
            {
                UnitCommand command = ai.CreateCommand(decision, target);
                int start = trace.Rows.Count;
                if (command != null) babau.Commands.Run(command);
                if (command != null)
                    foreach (int frame in WaitInfernalCommand34(command,
                        () => true, 30)) yield return frame;
                removed = target.Buffs.Enumerable.All(value =>
                    !ReferenceEquals(value, effect));
                bool immediatelyAvailable = ai.IsAvailable(decision);
                if (!removed)
                {
                    failedAttemptCount++;
                    failedAttemptsThrottled &= !immediatelyAvailable;
                }
                attempts.Add(new JObject {
                    ["attempt"] = attempt + 1,
                    ["command"] = Sprint27AiCommand(command),
                    ["immediatelyAvailable"] = immediatelyAvailable,
                    ["effectPresentAfter"] = !removed,
                    ["events"] = new JArray(trace.Rows.Skip(start)),
                    ["forcedRolls"] = 0
                });
                if (!removed)
                    foreach (int frame in InfernalBoneWaitGame35(6.1, 15))
                        yield return frame;
            }
            var granted = babau.Descriptor.Abilities.GetAbility(dispel);
            var playerData = granted == null ? null : new AbilityData(granted);
            bool mediumTarget = dispel.Range == AbilityRange.Medium &&
                playerData != null && playerData.CanTarget(
                    new TargetWrapper(target));
            bool exact = invisibilityCommand != null &&
                invisibilityCommand.Result == UnitCommand.ResultType.Success &&
                useful && aiBefore && removed && playerData != null &&
                playerData.IsAvailable && mediumTarget &&
                failedAttemptsThrottled &&
                resourceFree;
            check("useful-native-dispel", exact, new JObject {
                ["babau"] = babau.UniqueId,
                ["target"] = target.UniqueId,
                ["invisibilityCommand"] = Sprint27AiCommand(
                    invisibilityCommand),
                ["effect"] = effect == null ? null :
                    InfernalSavedEffect34(effect),
                ["useful"] = useful,
                ["aiAvailableBefore"] = aiBefore,
                ["attempts"] = attempts,
                ["removed"] = removed,
                ["abilityRange"] = dispel.Range.ToString(),
                ["playerCanTargetAtLane"] = mediumTarget,
                ["atWillPlayerAbilityAvailable"] = playerData?.IsAvailable,
                ["resourceCostComponentPresent"] = !resourceFree,
                ["failedAttemptCount"] = failedAttemptCount,
                ["failedAttemptThrottleApplicable"] =
                    failedAttemptCount > 0,
                ["failedAttemptsThrottled"] = failedAttemptsThrottled
            }, "A real visible spell effect makes the exact Babau AI action select native Dispel, real checks eventually remove it, any failed attempts are round-throttled, and the player's at-will ability remains available without a summon resource.");
        }

        private IEnumerable<int> InfernalCoreConditions36(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData hezrou, UnitEntityData target,
            UnitEntityData ally, InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            string prefix = ExpandedSummoningSpecialBuilder.Hezrou36Prefix;
            var sickened = (BlueprintBuff)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "StenchSickened"];
            var memory = (BlueprintBuff)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "StenchMemory"];
            var nausea = (BlueprintBuff)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "HeldNauseated"];
            DateTime stenchDeadline = DateTime.UtcNow.AddSeconds(15);
            Buff allySickened = null;
            Buff allyMemory = null;
            while (DateTime.UtcNow < stenchDeadline &&
                (allySickened == null || allyMemory == null))
            {
                Game.Instance.IsPaused = false;
                yield return 0;
                allySickened = InfernalCoreSourceBuff36(ally, sickened,
                    hezrou);
                allyMemory = InfernalCoreSourceBuff36(ally, memory,
                    hezrou);
            }
            bool stench = allySickened != null && allyMemory != null;
            check("stench-ally-exposure", stench, new JObject {
                ["source"] = hezrou.UniqueId,
                ["ally"] = ally.UniqueId,
                ["sickened"] = allySickened == null ? null :
                    InfernalSavedEffect34(allySickened),
                ["memory"] = allyMemory == null ? null :
                    InfernalSavedEffect34(allyMemory),
                ["allyIsEnemy"] = hezrou.IsEnemy(ally),
                ["fixtureFortBase"] = -100,
                ["forcedRolls"] = 0
            }, "The native area exposes a non-enemy ally, then keeps distinct source-owned sickened and re-entry memory effects; it is not rewritten enemy-only or nausea.");

            int fort = target.Stats.SaveFortitude.BaseValue;
            Size size = target.Descriptor.State.Size;
            int hp = target.Stats.HitPoints.BaseValue;
            try
            {
                target.Stats.SaveFortitude.BaseValue = -100;
                target.Descriptor.State.Size = Size.Medium;
                target.Stats.HitPoints.BaseValue = 100000;
                target.Damage = 0;
                PlaceExpandedSummoningUnit(target, hezrou.Position +
                    Vector3.forward * (hezrou.Corpulence +
                        target.Corpulence + .3f));
                var grab = SummonGrabComponent.Find(hezrou);
                bool held = false;
                var attempts = new JArray();
                for (int attempt = 0; attempt < 3 && !held; attempt++)
                {
                    int start = trace.Rows.Count;
                    held = grab != null && grab.TryGrab(target,
                        hezrou.Body.PrimaryHand.MaybeWeapon, true);
                    attempts.Add(new JObject {
                        ["attempt"] = attempt + 1,
                        ["held"] = held,
                        ["events"] = new JArray(trace.Rows.Skip(start)),
                        ["forcedAttackRoll"] = false,
                        ["knownQualifyingHit"] = true
                    });
                }
                if (!held) throw new InvalidOperationException(
                    "Hezrou hold precondition remained unobserved after three bounded native grapple checks.");
                var heldLogic = hezrou.Descriptor.Progression.Features
                    .Enumerable.SelectMany(value => value.SelectComponents<
                        HezrouHeldNausea36>()).Single();
                int nauseaStart = trace.Rows.Count;
                heldLogic.OnNewRound();
                Buff heldNausea = InfernalCoreSourceBuff36(target,
                    nausea, hezrou);
                JObject[] saves = trace.Rows.Skip(nauseaStart)
                    .OfType<JObject>().Where(value =>
                    (string)value["event"] == "save" &&
                    (string)value["owner"] == hezrou.UniqueId &&
                    (string)value["target"] == target.UniqueId &&
                    (int?)value["dc"] ==
                        InfernalRules36Policy.HezrouStenchDc).ToArray();
                bool distinct = heldNausea != null && saves.Length == 1 &&
                    InfernalCoreSourceBuff36(target, sickened, hezrou) !=
                        heldNausea &&
                    SummonHeldComponent.HolderOf(target,
                        grab.GrappledBuff) == hezrou;
                check("held-nausea-source", distinct, new JObject {
                    ["source"] = hezrou.UniqueId,
                    ["target"] = target.UniqueId,
                    ["grabAttempts"] = attempts,
                    ["saves"] = new JArray(saves),
                    ["nausea"] = heldNausea == null ? null :
                        InfernalSavedEffect34(heldNausea),
                    ["stenchSickenedPresent"] =
                        InfernalCoreSourceBuff36(target, sickened,
                            hezrou) != null,
                    ["holder"] = SummonHeldComponent.HolderOf(target,
                        grab.GrappledBuff)?.UniqueId,
                    ["impale"] = grab.HasImpale,
                    ["constrictDice"] = grab.ConstrictDiceCount,
                    ["forcedRolls"] = 0
                }, "The exact production hold makes its separate round save and source-owned nausea without Impale, constrict or fear semantics.");

                SummonHoldComponent.ReleaseLink(hezrou, target, grab,
                    true);
                var lifetime = heldNausea?.SelectComponents<
                    HezrouHeldNauseaDuration36>().SingleOrDefault();
                lifetime?.OnNewRound();
                double recovery = heldNausea == null ? -1 :
                    (heldNausea.EndTime - Game.Instance.TimeController
                        .GameTime).TotalSeconds;
                bool released = SummonHeldComponent.HolderOf(target,
                        grab.GrappledBuff) == null && heldNausea != null &&
                    recovery > 59 && recovery <= 61;
                check("held-nausea-release-recovery", released,
                    new JObject {
                        ["source"] = hezrou.UniqueId,
                        ["target"] = target.UniqueId,
                        ["holderAfterRelease"] =
                            SummonHeldComponent.HolderOf(target,
                                grab.GrappledBuff)?.UniqueId,
                        ["nauseaStillPresent"] = heldNausea != null &&
                            !heldNausea.IsDisposed,
                        ["recoverySeconds"] = recovery
                    }, "Release removes the exact hold but keeps only that source's nausea for its printed one-minute recovery clock.");
            }
            finally
            {
                target.Stats.SaveFortitude.BaseValue = fort;
                target.Descriptor.State.Size = size;
                target.Stats.HitPoints.BaseValue = hp;
            }
        }

        private static Buff InfernalCoreSourceBuff36(UnitEntityData owner,
            BlueprintBuff blueprint, UnitEntityData source)
        {
            return owner?.Buffs?.Enumerable.FirstOrDefault(value =>
                ReferenceEquals(value.Blueprint, blueprint) &&
                ReferenceEquals(value.Context?.MaybeCaster, source));
        }
    }
}
