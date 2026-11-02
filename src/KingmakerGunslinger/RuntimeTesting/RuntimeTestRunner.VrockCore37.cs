using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
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
                var observed = new Astral31SmiteEvents(vrock, pair.Ability,
                    null, null, new[] { vrock });
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
                    RuleApplyBuff[] applied = observed.Applications
                        .Where(value => ReferenceEquals(value.Initiator,
                            vrock) && value.AppliedBuff != null).ToArray();
                    // The second attempt must be refused: the resource is
                    // spent and the released already-active refusal stands.
                    UnitUseAbility second = null;
                    foreach (int frame in Sprint26Cast(vrock, pair.Ability,
                        new TargetWrapper(vrock), value => second = value))
                        yield return frame;
                    var after = new AbilityData(
                        vrock.Descriptor.Abilities.GetAbility(pair.Ability));
                    bool row = command != null && command.IsFinished &&
                        command.Result == UnitCommand.ResultType.Success &&
                        ownBefore == 1 && ownAfter == 0 &&
                        otherBefore == otherAfter &&
                        parameters.CasterLevel ==
                            VrockRules37Policy.SpellLikeCasterLevel &&
                        applied.Length >= 1 &&
                        applied.All(value => value.Duration.HasValue &&
                            value.Duration.Value.TotalSeconds > 0) &&
                        !after.IsAvailable;
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
                        ["casterLevel"] = parameters.CasterLevel,
                        ["expectedCasterLevel"] =
                            VrockRules37Policy.SpellLikeCasterLevel,
                        ["appliedEffects"] = new JArray(applied.Select(
                            value => new JObject {
                                ["blueprint"] = value.AppliedBuff
                                    ?.Blueprint?.AssetGuid,
                                ["durationSeconds"] =
                                    value.Duration?.TotalSeconds,
                                ["caster"] =
                                    value.Context?.MaybeCaster?.UniqueId
                            })),
                        ["secondAttempt"] = second == null ? null :
                            Sprint27AiCommand(second),
                        ["availableAfter"] = after.IsAvailable,
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
            Sprint28SetBrainActivePreservingActions(vrock, true);
            var attacks = new List<RuleAttackWithWeapon>();
            var watcher = new VrockAttackWatcher37(vrock, attacks);
            EventBus.Subscribe(watcher);
            var commands = new JArray();
            try
            {
                DateTime deadline = DateTime.UtcNow.AddSeconds(30);
                while (attacks.Count == 0 && DateTime.UtcNow < deadline)
                {
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
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
