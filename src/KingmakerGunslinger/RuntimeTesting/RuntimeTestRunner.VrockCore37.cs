using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
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
        /// The Vrock's own body and its two native self-buffs, observed on a
        /// live unit: five primary natural attacks at one reach from three
        /// exact weapon identities, an attached original mesh, the absence of
        /// the routines this creature does not have, and two single-use
        /// resources that are genuinely independent of one another.
        /// </summary>
        private IEnumerable<int> ReviewVrockCore37(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint37VrockCorePhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            int oldTargetAc = target.Stats.AC.BaseValue;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetDamage = target.Damage;
            Vector3 oldTargetPosition = target.Position;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint37-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint37 vrock core cell failed: " + id);
                };

            try
            {
                VrockCoreBootstrap37(check);

                UnitEntityData vrock = CastExpandedSummoningOwnTier(
                    fixture, "vrock");
                owned.Add(vrock);
                VrockQuiesce37(vrock);
                foreach (int frame in Outsider30NativeAppearance(vrock,
                    vrock.Blueprint.name)) yield return frame;

                VrockCoreProfile37(vrock, check);
                foreach (int frame in VrockSelfBuffs37(fixture, vrock, target,
                    check)) yield return frame;
            }
            finally
            {
                target.Stats.AC.BaseValue = oldTargetAc;
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                target.Damage = oldTargetDamage;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory, "sprint37-vrock-core.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        /// <summary>
        /// Every private Sprint 37 dependency resolves at its planned type
        /// while the three routes are still hidden.
        /// </summary>
        private static void VrockCoreBootstrap37(
            Action<string, bool, JObject, string> check)
        {
            string prefix = ExpandedSummoningSpecialBuilder.Vrock37Prefix;
            var routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key == "vrock")
                .OrderBy(value => value.ParentTier).ToArray();
            var expected = ExpandedSummoningIdentityCatalog.Build()
                .Where(value => value.Symbol.StartsWith(prefix,
                    StringComparison.Ordinal))
                .OrderBy(value => value.Symbol, StringComparer.Ordinal)
                .ToArray();
            var actual = ExpandedSummoningBlueprints.Registered.BySymbol
                .Where(pair => pair.Key.StartsWith(prefix,
                    StringComparison.Ordinal))
                .ToDictionary(pair => pair.Key, pair => pair.Value,
                    StringComparer.Ordinal);
            string[] missing = expected
                .Where(value => !actual.ContainsKey(value.Symbol))
                .Select(value => value.Symbol).ToArray();
            var wrongTypes = expected.Where(value =>
                actual.ContainsKey(value.Symbol) &&
                (actual[value.Symbol] == null || !string.Equals(
                    actual[value.Symbol].GetType().Name, value.PlannedType,
                    StringComparison.Ordinal)))
                .Select(value => new JObject {
                    ["symbol"] = value.Symbol,
                    ["expected"] = value.PlannedType,
                    ["actual"] = actual[value.Symbol] == null ? null :
                        actual[value.Symbol].GetType().Name
                }).ToArray();
            check("core-hidden-bootstrap",
                routes.Length == 3 && actual.Count == expected.Length &&
                missing.Length == 0 && wrongTypes.Length == 0 &&
                routes.All(value =>
                    !SummonVisibilityCatalog.IsPublished(value)),
                new JObject {
                    ["routes"] = new JArray(routes.Select(value =>
                        new JObject {
                            ["tier"] = value.ParentTier,
                            ["quantity"] = value.Multiplicity.ToString(),
                            ["published"] =
                                SummonVisibilityCatalog.IsPublished(value)
                        })),
                    ["expectedRegistrationCount"] = expected.Length,
                    ["actualRegistrationCount"] = actual.Count,
                    ["missing"] = new JArray(missing),
                    ["wrongTypes"] = new JArray(wrongTypes)
                }, "All three Vrock routes remain hidden while every Sprint 37 private dependency resolves at its planned type.");
        }

        /// <summary>
        /// The live profile: five primary limbs from three weapon identities
        /// at one reach, the project's own attached mesh, and the explicit
        /// absence of Pounce, Grab, Rend and Great Cleave.
        /// </summary>
        private static void VrockCoreProfile37(UnitEntityData vrock,
            Action<string, bool, JObject, string> check)
        {
            ItemEntityWeapon primary = vrock.Body.PrimaryHand.MaybeWeapon;
            ItemEntityWeapon[] limbs = vrock.Body.AdditionalLimbs
                .Select(value => value.MaybeWeapon)
                .Where(value => value != null).ToArray();
            ItemEntityWeapon[] all = new[] { primary }.Concat(limbs)
                .Where(value => value != null).ToArray();
            string[] identities = all
                .Select(value => value.Blueprint.AssetGuid)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Mesh mesh;
            string[] bones;
            Texture2D albedo;
            string status;
            bool visual = PteranodonAssetRuntime.TryGetVrockVisual37(
                VrockVisualPolicy37.Vrock, out mesh, out bones, out albedo,
                out status);
            string view = ExpandedSummoningPteranodonViewPatch.DescribeView(
                vrock.View);
            var grab = SummonGrabComponent.Find(vrock);
            // The routines this creature does not have. Each is named rather
            // than left to a reader's assumption.
            string[] absentRoutines = vrock.Descriptor.Progression.Features
                .Enumerable.Select(value => value.Blueprint.name)
                .Concat(vrock.Descriptor.Buffs.Enumerable.Select(value =>
                    value.Blueprint.name))
                .Where(value => value.IndexOf("Pounce",
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("Rend",
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("GreatCleave",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            bool exact = all.Length == 5 && identities.Length == 3 &&
                all.All(value => value.AttackRange.Value ==
                    VrockRules37Policy.ReachFeet) &&
                grab == null && absentRoutines.Length == 0 &&
                visual && mesh != null && albedo != null &&
                vrock.Descriptor.State.Size == Size.Large &&
                view.StartsWith("visual:attached;",
                    StringComparison.Ordinal);
            check("core-five-primary-attacks", exact, new JObject {
                ["owner"] = vrock.UniqueId,
                ["blueprint"] = vrock.Blueprint.AssetGuid,
                ["size"] = vrock.Descriptor.State.Size.ToString(),
                ["limbCount"] = all.Length,
                ["distinctWeaponIdentities"] = new JArray(identities),
                ["reachFeet"] = new JArray(all.Select(value =>
                    value.AttackRange.Value)),
                ["printedReachFeet"] = VrockRules37Policy.ReachFeet,
                ["grabProfile"] = grab == null ? null : "present",
                ["pounceRendCleaveFacts"] = new JArray(absentRoutines),
                ["view"] = view,
                ["visualStatus"] = status,
                ["visualPublished"] = status == "visual:published",
                ["visualFailureMeaning"] =
                    status == "donor-visual:missing-mesh-file"
                        ? "The asset files are not in the installed mod; packaging, not the mesh."
                        : status == "donor-visual:invalid-mesh-data"
                            ? "The asset files are installed but failed the anatomy, rig or albedo contract."
                            : null,
                ["meshVertices"] = mesh?.vertexCount,
                ["albedoWidth"] = albedo?.width,
                ["hitPoints"] = vrock.Stats.HitPoints.ModifiedValue,
                ["armourClass"] = vrock.Stats.AC.ModifiedValue,
                ["fortitude"] = vrock.Stats.SaveFortitude.ModifiedValue,
                ["reflex"] = vrock.Stats.SaveReflex.ModifiedValue,
                ["will"] = vrock.Stats.SaveWill.ModifiedValue,
                ["perception"] = vrock.Stats.SkillPerception.ModifiedValue,
                ["speed"] = vrock.Stats.Speed.ModifiedValue,
                ["printedFlySpeedFeet"] =
                    VrockRules37Policy.EffectiveFlySpeedFeet,
                ["omittedLandSpeedFeet"] =
                    VrockRules37Policy.OmittedLandSpeedFeet,
                ["projectAbilities"] = new JArray(vrock.Descriptor.Abilities
                    .Enumerable.Select(value => value.Blueprint.name)
                    .Where(value => value.StartsWith("KMG_",
                        StringComparison.Ordinal))
                    .OrderBy(value => value, StringComparer.Ordinal))
            }, "The live Vrock fields five primary natural attacks from three weapon identities at one printed reach on its own attached body, and carries no Pounce, Grab, Rend or Great Cleave routine.");
        }

        /// <summary>
        /// Heroism and Mirror Image: the installed native spells, cast at the
        /// recorded caster level, each spending only its own single-use
        /// resource, each refused once its own effect already stands, and the
        /// brain falling back to a real attack when both are satisfied.
        /// </summary>
        private IEnumerable<int> VrockSelfBuffs37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData vrock,
            UnitEntityData target, Action<string, bool, JObject, string> check)
        {
            string prefix = ExpandedSummoningSpecialBuilder.Vrock37Prefix;
            var heroism = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "Heroism"];
            var mirror = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[prefix + "MirrorImage"];
            var heroismResource = (BlueprintAbilityResource)
                ExpandedSummoningBlueprints.Registered.BySymbol[
                    prefix + "HeroismResource"];
            var mirrorResource = (BlueprintAbilityResource)
                ExpandedSummoningBlueprints.Registered.BySymbol[
                    prefix + "MirrorImageResource"];

            var rows = new JArray();
            bool coherent = true;
            var pairs = new[] {
                new { Label = "heroism", Ability = heroism,
                    Own = heroismResource, Other = mirrorResource },
                new { Label = "mirror-image", Ability = mirror,
                    Own = mirrorResource, Other = heroismResource }
            };
            foreach (var pair in pairs)
            {
                int ownBefore = vrock.Descriptor.Resources
                    .GetResourceAmount(pair.Own);
                int otherBefore = vrock.Descriptor.Resources
                    .GetResourceAmount(pair.Other);
                // Why this ability is or is not castable, recorded before the
                // attempt. A shared cast helper throws on an unavailable
                // ability, which spends a whole guarded transaction to say
                // only "unavailable"; this names the predicate instead.
                JObject readiness = VrockCastReadiness37(vrock, pair.Ability,
                    new TargetWrapper(vrock));
                check("core-castable-" + pair.Label,
                    (bool)readiness["isAvailable"] &&
                    (bool)readiness["canTarget"] && ownBefore == 1,
                    (JObject)readiness.DeepClone(),
                    "The once-per-summoning " + pair.Label +
                    " is granted, holds its single use, and is castable at the vrock itself.");
                // Astral31SmiteEvents matches applications by buff
                // identity, and this project does not choose the effect: the
                // installed native spell owns it. Passing a null buff to that
                // watcher silently records nothing, which is what made an
                // otherwise correct cast look effectless. This watcher matches
                // on the context's source ability instead, so the effect's
                // blueprint is what the observation discovers.
                var applications = new List<RuleApplyBuff>();
                var observed = new VrockEffectWatcher37(vrock, pair.Ability,
                    applications);
                EventBus.Subscribe(observed);
                UnitUseAbility command = null;
                try
                {
                    foreach (int frame in Sprint26Cast(vrock, pair.Ability,
                        new TargetWrapper(vrock), value => command = value))
                        yield return frame;
                    int ownAfter = vrock.Descriptor.Resources
                        .GetResourceAmount(pair.Own);
                    int otherAfter = vrock.Descriptor.Resources
                        .GetResourceAmount(pair.Other);
                    var granted = new AbilityData(
                        vrock.Descriptor.Abilities.GetAbility(pair.Ability));
                    var parameters = granted.CalculateParams();
                    RuleApplyBuff[] applied = applications
                        .Where(value => value.AppliedBuff != null).ToArray();
                    // The second use must be refused, observed by asking
                    // the engine rather than by attempting a cast: the shared
                    // cast helper throws on an unavailable ability, so using
                    // it to observe a refusal cannot distinguish the refusal
                    // this phase wants from a setup failure it does not.
                    JObject refusal = VrockCastReadiness37(vrock,
                        pair.Ability, new TargetWrapper(vrock));
                    // The caster level the printed entry gives is the one the
                    // resolving effect carries. AbilityData.CalculateParams
                    // triggers RuleCalculateAbilityParams, which honours only
                    // ContextSetAbilityParams and otherwise derives the level
                    // from the unit, so for this ability it reports the hit
                    // dice. The ContextCalculateAbilityParams this project
                    // attaches is read by MechanicsContext itself
                    // (m_ParamsCalculator, set from the AssociatedBlueprint),
                    // so the context of the applied effect is where the
                    // replacement actually lands. Both are recorded, and the
                    // assertion is on the effect's own context.
                    int[] effectCasterLevels = applied
                        .Select(value => value.Context == null ? -1 :
                            value.Context.Params.CasterLevel).ToArray();
                    bool row = command != null && command.IsFinished &&
                        command.Result == UnitCommand.ResultType.Success &&
                        ownBefore == 1 && ownAfter == 0 &&
                        otherBefore == otherAfter &&
                        effectCasterLevels.Length >= 1 &&
                        effectCasterLevels.All(value => value ==
                            VrockRules37Policy.SpellLikeCasterLevel) &&
                        applied.Length >= 1 &&
                        applied.All(value => value.Duration.HasValue &&
                            value.Duration.Value.TotalSeconds > 0) &&
                        !(bool)refusal["isAvailable"];
                    coherent &= row;
                    rows.Add(new JObject {
                        ["ability"] = pair.Label,
                        ["abilityGuid"] = pair.Ability.AssetGuid,
                        ["command"] = command == null ? null :
                            Sprint27AiCommand(command),
                        ["ownResourceBefore"] = ownBefore,
                        ["ownResourceAfter"] = ownAfter,
                        ["otherResourceBefore"] = otherBefore,
                        ["otherResourceAfter"] = otherAfter,
                        ["preCastEstimateCasterLevel"] =
                            parameters.CasterLevel,
                        ["effectContextCasterLevels"] =
                            new JArray(effectCasterLevels),
                        ["expectedCasterLevel"] =
                            VrockRules37Policy.SpellLikeCasterLevel,
                        ["whyTheyDiffer"] =
                            "RuleCalculateAbilityParams honours only ContextSetAbilityParams; ContextCalculateAbilityParams is read by MechanicsContext from the AssociatedBlueprint, so it lands on the resolving effect's context.",
                        ["observedApplications"] = applications.Count,
                        ["appliedEffects"] = new JArray(applied.Select(
                            value => new JObject {
                                ["blueprint"] = value.AppliedBuff
                                    ?.Blueprint?.name,
                                ["blueprintGuid"] = value.AppliedBuff
                                    ?.Blueprint?.AssetGuid,
                                ["isNativeEffect"] = value.AppliedBuff
                                    ?.Blueprint?.name != null &&
                                    !value.AppliedBuff.Blueprint.name
                                        .StartsWith("KMG_",
                                            StringComparison.Ordinal),
                                ["canApply"] = value.CanApply,
                                ["immunity"] = value.Immunity,
                                ["durationSeconds"] =
                                    value.Duration?.TotalSeconds,
                                ["contextCasterLevel"] =
                                    value.Context?.Params?.CasterLevel,
                                ["caster"] =
                                    value.Context?.MaybeCaster?.UniqueId
                            })),
                        ["refusalAfterUse"] = refusal,
                        ["availableAfter"] = refusal["isAvailable"],
                        ["coherent"] = row
                    });
                }
                finally { EventBus.Unsubscribe(observed); }
            }
            check("core-independent-self-buffs", coherent, new JObject {
                ["owner"] = vrock.UniqueId,
                ["observations"] = rows
            }, "Each native self-buff casts at the recorded caster level, applies a real finite effect, debits only its own single use, leaves the other untouched, and is refused once satisfied.");

            // With both single uses spent, the brain must still act: the
            // fallback is the native attack action, not a stall.
            target.Stats.AC.BaseValue = -100;
            target.Stats.HitPoints.BaseValue = 100000;
            PlaceExpandedSummoningUnit(target, vrock.Position +
                Vector3.right * (vrock.Corpulence + target.Corpulence + 0.5f));
            JObject brain = VrockPrepareBrain37(vrock, target);
            var attacks = new List<RuleAttackWithWeapon>();
            var watcher = new VrockAttackWatcher37(vrock, attacks);
            EventBus.Subscribe(watcher);
            var commands = new JArray();
            try
            {
                foreach (int frame in VrockDriveBrain37(vrock,
                    () => attacks.Count > 0, 40, brain)) yield return frame;
                foreach (UnitCommand command in vrock.Commands.Raw
                    .Where(value => value != null).ToArray())
                    commands.Add(Sprint27AiCommand(command));
            }
            finally
            {
                EventBus.Unsubscribe(watcher);
                Sprint28SetBrainActivePreservingActions(vrock, false);
                vrock.Commands.InterruptAll(true);
                vrock.Commands.RemoveFinishedAndUpdateQueue();
            }
            check("core-attack-fallback", attacks.Count > 0, new JObject {
                ["owner"] = vrock.UniqueId,
                ["target"] = target.UniqueId,
                ["brain"] = brain,
                ["attackRules"] = attacks.Count,
                ["attackWeapons"] = new JArray(attacks.Select(value =>
                    value.Weapon?.Blueprint?.AssetGuid).Distinct()),
                ["commands"] = commands,
                ["heroismRemaining"] = vrock.Descriptor.Resources
                    .GetResourceAmount(heroismResource),
                ["mirrorRemaining"] = vrock.Descriptor.Resources
                    .GetResourceAmount(mirrorResource)
            }, "With both single uses spent the brain falls through to real weapon attacks rather than stalling on an unavailable spell.");
        }

        /// <summary>
        /// Everything the engine consults before it will let an ability be
        /// cast, read without casting anything: the granted fact, its own
        /// caster checkers, the resource it spends, and the target test.
        /// </summary>
        private static JObject VrockCastReadiness37(UnitEntityData owner,
            BlueprintAbility blueprint, TargetWrapper target)
        {
            Ability granted = owner.Descriptor.Abilities
                .GetAbility(blueprint);
            var row = new JObject {
                ["owner"] = owner.UniqueId,
                ["ability"] = blueprint.name,
                ["abilityGuid"] = blueprint.AssetGuid,
                ["granted"] = granted != null,
                ["factActive"] = granted != null && granted.Active,
                ["type"] = blueprint.Type.ToString(),
                ["range"] = blueprint.Range.ToString(),
                ["actionType"] = blueprint.ActionType.ToString(),
                ["canTargetSelf"] = blueprint.CanTargetSelf,
                ["canTargetFriends"] = blueprint.CanTargetFriends,
                ["canTargetEnemies"] = blueprint.CanTargetEnemies,
                ["spellCastingForbidden"] =
                    (bool)owner.Descriptor.State.SpellCastingForbidden,
                ["casterCanAct"] = owner.Descriptor.State.CanAct,
                ["polymorphed"] = owner.Body.IsPolymorphed
            };
            var checkers = new JArray();
            foreach (IAbilityCasterChecker checker in
                blueprint.CasterCheckers ??
                Array.Empty<IAbilityCasterChecker>())
            {
                checkers.Add(new JObject {
                    ["checker"] = checker.GetType().Name,
                    ["correctCaster"] = checker.CorrectCaster(owner)
                });
            }
            row["casterCheckers"] = checkers;
            var costs = new JArray();
            foreach (AbilityResourceLogic cost in
                blueprint.GetComponents<AbilityResourceLogic>())
            {
                costs.Add(new JObject {
                    ["resource"] = cost.RequiredResource?.name,
                    ["resourceGuid"] = cost.RequiredResource?.AssetGuid,
                    ["spends"] = cost.IsSpendResource,
                    ["amount"] = cost.Amount,
                    ["heldByCaster"] = cost.RequiredResource == null ? null :
                        (int?)owner.Descriptor.Resources.GetResourceAmount(
                            cost.RequiredResource),
                    ["casterHasResourceFact"] =
                        cost.RequiredResource != null &&
                        owner.Descriptor.Resources.HasEnoughResource(
                            cost.RequiredResource, cost.Amount)
                });
            }
            row["resourceCosts"] = costs;
            if (granted == null)
            {
                row["isAvailable"] = false;
                row["canTarget"] = false;
                return row;
            }
            var data = new AbilityData(granted);
            row["isAvailable"] = data.IsAvailable;
            row["canTarget"] = data.CanTarget(target);
            row["casterLevel"] = data.CalculateParams().CasterLevel;
            return row;
        }

        /// <summary>
        /// Brings one disposable owner to the state the engine requires before
        /// its real brain will decide anything, following the already
        /// qualified autonomous-action fixture rather than inventing a second
        /// one: mutual memory visibility, the three native caches the AI reads
        /// rather than maintains, the owner's restored action list, and a
        /// decision clock that is actually due.
        ///
        /// <para>Direct casts earlier in a grouped phase do not drive any of
        /// this, which is why simply switching the brain on and waiting
        /// observes nothing and says nothing about the AI.</para>
        /// </summary>
        private static JObject VrockPrepareBrain37(UnitEntityData owner,
            UnitEntityData enemy)
        {
            owner.Commands.InterruptAll(true);
            owner.Commands.RemoveFinishedAndUpdateQueue();
            owner.Memory.Add(enemy).Visible = true;
            enemy.Memory.Add(owner).Visible = true;
            // Unit memory and GROUP memory are different stores, and the AI
            // reads the group's. The fourth AI run showed the difference
            // decisively: the one failing cell had targetInGroupMemory false
            // while the passing cells had it true, and its owner sat through
            // twenty-three real AI ticks starting no command at all, because
            // its group had no enemy to act on. The released fixture only
            // checks this value; after a long phase that has moved units
            // about, it has to be re-established.
            if (owner.Group != null) owner.Group.Memory.Add(enemy).Visible =
                true;
            if (enemy.Group != null) enemy.Group.Memory.Add(owner).Visible =
                true;
            new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
            new UnitCombatJoinController().Tick();
            new UnitCombatPrepareController().Tick();
            // The sixth observation recorded ownerInCombat true and
            // enemyInCombat false, with the attack action available and no
            // command ever issued: the join controller does not enrol an
            // unprovoked target, and a brain with no opponent in combat has
            // nothing to attack. Both sides are enrolled explicitly, which is
            // what the released elemental and bodyguard fixtures also do.
            if (!owner.CombatState.IsInCombat) owner.CombatState.JoinCombat();
            if (!enemy.CombatState.IsInCombat) enemy.CombatState.JoinCombat();
            new UnitCombatJoinController().Tick();
            new UnitCombatPrepareController().Tick();
            Sprint28SetBrainActivePreservingActions(owner, true);
            owner.Brain.RestoreAvailableActions();
            float before = owner.CombatState.AIData.NextCommandTime;
            owner.CombatState.AIData.NextCommandTime = Time.time - 0.01f;
            bool targetInGroupMemory = owner.Group != null &&
                owner.Group.Memory.Enemies.Any(value =>
                    ReferenceEquals(value.Unit, enemy));
            return new JObject {
                ["ownerGroupInCombat"] =
                    owner.Group != null && owner.Group.IsInCombat,
                ["ownerGroupAwake"] = owner.Group != null &&
                    Game.Instance.AwakeUnitGroups.Contains(owner.Group),
                ["targetInGroupMemory"] = targetInGroupMemory,
                ["ownerBrainActive"] = owner.IsBrainActive,
                ["targetCanAct"] = enemy.Descriptor.State.CanAct,
                ["owner"] = owner.UniqueId,
                ["enemy"] = enemy.UniqueId,
                ["ownerInCombat"] = owner.IsInCombat,
                ["enemyInCombat"] = enemy.IsInCombat,
                ["ownerSeesEnemy"] = owner.Memory.ContainsVisible(enemy),
                ["ownerHasLineOfSight"] = owner.HasLOS(enemy),
                ["ownerIsEnemyOfTarget"] = owner.IsEnemy(enemy),
                ["nextCommandTimeBefore"] = before,
                ["nextCommandTimeNow"] =
                    owner.CombatState.AIData.NextCommandTime,
                ["brainActions"] = new JArray(
                    (owner.Blueprint.Brain?.Actions ??
                        Array.Empty<Kingmaker.Controllers.Brain.Blueprints
                            .BlueprintAiAction>())
                    .Select(value => value.GetType().Name + ":" +
                        value.name)),
                ["availableActions"] = new JArray(owner.Brain.AvailableActions
                    .Select(value => new JObject {
                        ["type"] = value.Blueprint.GetType().Name,
                        ["name"] = value.Blueprint.name,
                        ["baseScore"] = value.BaseScore
                    }))
            };
        }

        /// <summary>
        /// Drives real frames until the observation's own condition holds or
        /// the window closes, keeping the game unpaused and this one owner's
        /// decision clock due. Nothing is dispatched on the brain's behalf:
        /// re-arming the clock only lets the brain decide again, it does not
        /// decide for it.
        /// </summary>
        private IEnumerable<int> VrockDriveBrain37(UnitEntityData owner,
            Func<bool> satisfied, int seconds, JObject record)
        {
            // The seventh observation had the owner in combat, the enemy in
            // combat, visibility, line of sight, hostility and the attack
            // action available, drove 1238 frames and re-armed the clock 39
            // times, and still saw no command. Waiting was the error: the game
            // does not tick a party summon's brain, so an observer that only
            // waits watches a brain that is never asked. This drives the
            // already qualified exact-owner AI tick, which refuses outright if
            // this owner is not an eligible candidate and reports every other
            // unit it declined to tick.
            var ambient = new HashSet<string>(StringComparer.Ordinal);
            DateTime deadline = DateTime.UtcNow.AddSeconds(seconds);
            int frames = 0;
            int aiTicks = 0;
            while (!satisfied() && DateTime.UtcNow < deadline)
            {
                if (frames % 60 == 0 && !owner.Commands.Raw.Any(value =>
                        value != null && !value.IsFinished))
                {
                    owner.CombatState.AIData.NextCommandTime =
                        Time.time - 0.01f;
                    foreach (string candidate in Sprint24TickExactAi(owner))
                        if (candidate != owner.UniqueId)
                            ambient.Add(candidate);
                    aiTicks++;
                }
                frames++;
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            record["drivenFrames"] = frames;
            record["aiTicks"] = aiTicks;
            record["windowSeconds"] = seconds;
            record["satisfied"] = satisfied();
            record["ownerInCombatAfter"] = owner.IsInCombat;
            record["ambientCandidatesNeverTicked"] = new JArray(ambient);
            record["fixtureIssuedCommands"] = 0;
        }

        /// <summary>
        /// Every buff application whose context came from one exact ability on
        /// one exact owner. The effect's own blueprint is the result, not a
        /// precondition, because the installed native spell owns it.
        /// </summary>
        private sealed class VrockEffectWatcher37 :
            IGlobalRulebookHandler<RuleApplyBuff>
        {
            private readonly UnitEntityData _owner;
            private readonly BlueprintAbility _ability;
            private readonly List<RuleApplyBuff> _rules;

            internal VrockEffectWatcher37(UnitEntityData owner,
                BlueprintAbility ability, List<RuleApplyBuff> rules)
            { _owner = owner; _ability = ability; _rules = rules; }

            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }

            public void OnEventDidTrigger(RuleApplyBuff evt)
            {
                if (evt.Context == null ||
                    !ReferenceEquals(evt.Context.MaybeCaster, _owner) ||
                    !ReferenceEquals(evt.Context.SourceAbility, _ability))
                    return;
                _rules.Add(evt);
            }
        }

        private sealed class VrockAttackWatcher37 :
            IGlobalRulebookHandler<RuleAttackWithWeapon>
        {
            private readonly UnitEntityData _owner;
            private readonly List<RuleAttackWithWeapon> _rules;

            internal VrockAttackWatcher37(UnitEntityData owner,
                List<RuleAttackWithWeapon> rules)
            { _owner = owner; _rules = rules; }

            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                if (ReferenceEquals(evt.Initiator, _owner)) _rules.Add(evt);
            }
        }
    }
}
