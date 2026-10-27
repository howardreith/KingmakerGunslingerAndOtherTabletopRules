using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
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
        // One fixed save-free mixed-emitter case. Uses the canonical circle's
        // real recipient and native AC/save/control consumers; no copied rules.
        private IEnumerable<int> ReviewSprints3031Circles(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            CreateExpandedSummoningCorrectionHostile(fixture);
            var controller = fixture.Hostile;
            var oldAlignment = controller.Descriptor.Alignment.Value;
            var oldPosition = controller.Position;
            var areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
            var owners = new List<UnitEntityData>();
            var localActors = new List<UnitEntityData>();
            var prototypes = new List<BlueprintUnit>();
            UnitEntityData recipient = null;
            Buff control = null;
            CircleControlCastCapture capture = null;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                var center = fixture.Caster.Position + Vector3.forward * 3f;
                PlaceExpandedSummoningUnit(controller, center + Vector3.forward * 30f);
                recipient = CircleSpawn("S3031_OwnedRecipient", center, fixture.Caster,
                    localActors, prototypes, fixture.Caster.Blueprint.Faction);
                fixture.Created.Add(recipient);
                PlaceExpandedSummoningUnit(recipient, center);
                var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
                var dominate = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    "d7cbd2004ce66a042aeab2e95a3c5c61", "shared-circle native control ability");
                var dominated = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                    "c0f4e1c24c9cd334ca988ed1bd9d201f", "shared-circle native terminal control buff");
                capture = new CircleControlCastCapture(recipient, dominated);
                var startingBuffs = recipient.Buffs.Enumerable.ToArray();
                controller.Descriptor.Alignment.Set(Alignment.LawfulGood);
                int baseAc = CircleAttackAC(controller, recipient);
                int baseSave = CircleSave(controller, recipient, dominate);
                foreach (var spec in new[] {
                    new { Key="hound-archon", Cl=6, Bonus=2 },
                    new { Key="trumpet-archon", Cl=14, Bonus=2 },
                    new { Key="astral-deva", Cl=15, Bonus=4 } })
                {
                    var owner = CastExpandedSummoningOwnTier(fixture, spec.Key); owners.Add(owner);
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    PlaceExpandedSummoningUnit(owner, center + Vector3.right * (owners.Count * .4f));
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    var actors = new[] { recipient, controller, fixture.Caster }.Concat(owners).ToList();
                    Astral31RefreshAreas(Game.Instance.State.AreaEffects.All.Except(areasBefore).ToArray(), actors);
                    var area = Sprint29OwnedAreas(owner, circle.Area).Single();
                    var grant = recipient.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, circle.Recipient) &&
                        value.SourceAreaEffectId == area.UniqueId);
                    var canonical = owner.Blueprint.AddFacts.SelectMany(value => value.ComponentsArray)
                        .OfType<ArchonConstantMagicCircle>().Single(value => ReferenceEquals(value.Carrier, circle.Carrier));
                    controller.Descriptor.Alignment.Set(Alignment.LawfulGood);
                    int goodAc = CircleAttackAC(controller, recipient), goodSave = CircleSave(controller, recipient, dominate);
                    controller.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                    int evilAc = CircleAttackAC(controller, recipient), evilSave = CircleSave(controller, recipient, dominate);
                    bool exact = ReferenceEquals(canonical.Spell, circle.Spell) && canonical.CasterLevel == spec.Cl &&
                        ReferenceEquals(canonical.OwningBlueprint, owner.Blueprint) &&
                        ReferenceEquals(area.Context.MaybeCaster, owner) && area.Context.Params.CasterLevel == spec.Cl &&
                        ReferenceEquals(grant.Context.MaybeCaster, owner) && grant.Context.Params.CasterLevel == spec.Cl &&
                        CircleBuffs(recipient, circle.Recipient).Length == owners.Count &&
                        goodAc == baseAc && goodSave == baseSave && evilAc == baseAc + spec.Bonus && evilSave == baseSave + spec.Bonus;
                    Sprint25Check("shared-circles-" + spec.Key, exact, new JObject {
                        ["owner"] = owner.UniqueId, ["recipient"] = recipient.UniqueId, ["area"] = area.UniqueId,
                        ["recipientSourceArea"] = grant.SourceAreaEffectId, ["sourceCl"] = grant.Context.Params.CasterLevel,
                        ["canonicalSpell"] = canonical.Spell.AssetGuid, ["recipientInstances"] = CircleBuffs(recipient, circle.Recipient).Length,
                        ["baseAc"] = baseAc, ["baseSave"] = baseSave, ["goodAc"] = goodAc, ["goodSave"] = goodSave,
                        ["evilAc"] = evilAc, ["evilSave"] = evilSave, ["expectedTypedBonus"] = spec.Bonus },
                        "Each creature consumes the canonical circle with its own casting level; native typed defenses do not stack, and the separate angel aura contributes its larger +4.");
                    if (!exact) throw new InvalidOperationException("Shared circle native source/defense mismatch: " + spec.Key);
                }
                EventBus.Subscribe(capture);
                foreach (var alignment in new[] { Alignment.LawfulEvil, Alignment.LawfulGood })
                {
                    controller.Descriptor.Alignment.Set(alignment); capture.Clear();
                    var controlContext = new AbilityExecutionContext(new AbilityData(dominate, controller.Descriptor),
                        new AbilityParams { CasterLevel = 20, SpellLevel = 5, DC = 25 },
                        new TargetWrapper(recipient), Rulebook.CurrentContext);
                    control = recipient.Buffs.AddBuff(dominated, controlContext, TimeSpan.FromMinutes(1));
                    bool block = alignment == Alignment.LawfulEvil && _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity;
                    bool exact = capture.Applications == 1 && (block ? control == null && !capture.CanApply : control != null && capture.CanApply);
                    Sprint25Check("shared-circles-control-" + alignment, exact, new JObject {
                        ["controller"] = controller.UniqueId, ["recipient"] = recipient.UniqueId,
                        ["alignment"] = alignment.ToString(), ["enhancementEnabled"] = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity,
                        ["applicationCount"] = capture.Applications, ["canApply"] = capture.CanApply, ["applied"] = control != null,
                        ["terminalProbeCl"] = controlContext.Params.CasterLevel, ["terminalProbeSpellLevel"] = controlContext.Params.SpellLevel,
                        ["expectedBlocked"] = block }, "Actual native terminal control application follows the shared configured alignment-immunity contract.");
                    control?.Remove(); control = null;
                    if (!exact) throw new InvalidOperationException("Shared circle control consumer mismatch.");
                }
                EventBus.Unsubscribe(capture);
                // Remove angel first: the larger protection ends but both
                // canonical archon contributions must remain independently.
                foreach (var owner in owners.AsEnumerable().Reverse().ToArray())
                {
                    owner.Commands.InterruptAll(true);
                    if (owner.IsInCombat) owner.LeaveCombat();
                    foreach (var pending in owner.Commands.Raw.Where(value => value != null && !value.IsFinished).ToArray()) pending.Interrupt(true);
                    owner.Commands.RemoveFinishedAndUpdateQueue();
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    var live = owners.Where(value => !value.Destroyed).ToArray();
                    int expected = live.Length == 0 ? 0 : 2;
                    controller.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                    int ac = CircleAttackAC(controller, recipient), save = CircleSave(controller, recipient, dominate);
                    var grants = CircleBuffs(recipient, circle.Recipient);
                    bool exact = owner.Destroyed && !owner.IsInState && grants.Length == live.Length &&
                        grants.All(value => live.Any(source => ReferenceEquals(value.Context.MaybeCaster, source))) &&
                        ac == baseAc + expected && save == baseSave + expected;
                    Sprint25Check("shared-circles-remove-" + owner.Blueprint.name, exact, new JObject {
                        ["removedOwner"] = owner.UniqueId, ["remainingSources"] = new JArray(grants.Select(value => value.Context.MaybeCaster.UniqueId)),
                        ["ac"] = ac, ["save"] = save, ["expectedBonus"] = expected },
                        "Removing one emitter removes only its grants; the remaining exact sources retain the native protection.");
                    if (!exact) throw new InvalidOperationException("Shared circle source cleanup mismatch.");
                }
                foreach (var actor in new[] { recipient, controller })
                {
                    actor.Commands.InterruptAll(true);
                    if (actor.IsInCombat) actor.LeaveCombat();
                    foreach (var pending in actor.Commands.Raw.Where(value => value != null && !value.IsFinished).ToArray()) pending.Interrupt(true);
                    actor.Commands.RemoveFinishedAndUpdateQueue();
                }
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var pendingCommands = new JArray(owners.Concat(new[] { recipient, controller }).Select(actor => new JObject {
                    ["actor"] = actor.UniqueId, ["pending"] = new JArray(actor.Commands.Raw.Where(value => value != null && !value.IsFinished).Select(Sprint27AiCommand)) }));
                bool reset = !Game.Instance.State.AreaEffects.All.Except(areasBefore).Any() &&
                    !recipient.Buffs.Enumerable.Except(startingBuffs).Any() &&
                    owners.All(value => value.Destroyed && !value.IsInState && !fixture.Created.Contains(value)) &&
                    owners.Concat(new[] { recipient, controller }).All(actor => !actor.Commands.Raw.Any(value => value != null && !value.IsFinished));
                Sprint25Check("shared-circles-reset", reset, new JObject { ["areasExact"] = !Game.Instance.State.AreaEffects.All.Except(areasBefore).Any(),
                    ["recipientBuffsExact"] = !recipient.Buffs.Enumerable.Except(startingBuffs).Any(), ["ownedSourcesAbsent"] = owners.All(value => !value.IsInState),
                    ["ownedQueues"] = pendingCommands },
                    "The mixed-emitter case ends with actual baseline areas, recipient buffs and no owned source actors.");
                if (!reset) throw new InvalidOperationException("Shared circle reset failed.");
                foreach (int frame in ReviewSprints3031CircleReach(fixture, controller, center, circle, dominate,
                    dominated, areasBefore, owners, localActors, prototypes)) yield return frame;
            }
            finally
            {
                if (capture != null) EventBus.Unsubscribe(capture); control?.Remove();
                DisposeExpandedSummoningUnits(fixture.Created, owners.Concat(localActors).Where(value => value != null && !value.Destroyed).ToArray());
                foreach (var prototype in prototypes) UnityEngine.Object.Destroy(prototype);
                controller.Descriptor.Alignment.Set(oldAlignment); PlaceExpandedSummoningUnit(controller, oldPosition);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        // PF1 Protective Aura carries the angel's circle protection to 20 feet;
        // Hound and Trumpet circles keep the canonical 10 feet. Native cylinder
        // membership is 2D distance <= radius + unit corpulence, so the fixed
        // probes sit well inside 10 feet, between 10 and 20 feet, and beyond
        // 20 feet. The consumer is the real native terminal control application.
        private IEnumerable<int> ReviewSprints3031CircleReach(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData controller, Vector3 center, MagicCircleBlueprintSet circle, BlueprintAbility dominate,
            BlueprintBuff dominated, AreaEffectEntityData[] areasBefore, List<UnitEntityData> owners,
            List<UnitEntityData> localActors, List<BlueprintUnit> prototypes)
        {
            var aura = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol[
                ExpandedSummoningSpecialBuilder.Astral31Prefix + "ProtectiveRecipient"];
            var probes = new[] { new { Id = "inner", Meters = 1.5f }, new { Id = "band", Meters = 4.6f },
                new { Id = "beyond", Meters = 9f } };
            var recipients = new UnitEntityData[probes.Length];
            for (int index = 0; index < probes.Length; index++)
            {
                var position = center + Vector3.left * probes[index].Meters;
                recipients[index] = CircleSpawn("S3031_Reach_" + probes[index].Id, position, fixture.Caster,
                    localActors, prototypes, fixture.Caster.Blueprint.Faction);
                fixture.Created.Add(recipients[index]);
                PlaceExpandedSummoningUnit(recipients[index], position);
            }
            var startingBuffs = recipients.Select(unit => unit.Buffs.Enumerable.ToArray()).ToArray();
            // Grants are {canonical circle, angel aura} per probe; protection
            // is the expected control block when the shared module is enabled.
            var cases = new[] {
                new { Id = "hound-archon", Keys = new[] { "hound-archon" },
                    Canonical = new[] { 1, 0, 0 }, Aura = new[] { 0, 0, 0 }, Protected = new[] { true, false, false } },
                new { Id = "trumpet-archon", Keys = new[] { "trumpet-archon" },
                    Canonical = new[] { 1, 0, 0 }, Aura = new[] { 0, 0, 0 }, Protected = new[] { true, false, false } },
                new { Id = "astral-deva", Keys = new[] { "astral-deva" },
                    Canonical = new[] { 1, 0, 0 }, Aura = new[] { 1, 1, 0 }, Protected = new[] { true, true, false } },
                new { Id = "astral-deva-and-hound-archon", Keys = new[] { "astral-deva", "hound-archon" },
                    Canonical = new[] { 2, 0, 0 }, Aura = new[] { 1, 1, 0 }, Protected = new[] { true, true, false } } };
            foreach (var spec in cases)
            {
                var live = new List<UnitEntityData>();
                foreach (string key in spec.Keys)
                {
                    var owner = CastExpandedSummoningOwnTier(fixture, key); owners.Add(owner); live.Add(owner);
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    PlaceExpandedSummoningUnit(owner, center + Vector3.right * (.4f * (live.Count - 1)));
                }
                for (int frame = 0; frame < 8; frame++) yield return 0;
                var actors = recipients.Concat(new[] { controller, fixture.Caster }).Concat(live).ToList();
                Astral31RefreshAreas(Game.Instance.State.AreaEffects.All.Except(areasBefore).ToArray(), actors);
                var rows = new JArray();
                for (int index = 0; index < probes.Length; index++)
                    rows.Add(Sprints3031ReachProbe(controller, recipients[index], probes[index].Id, center, circle, aura,
                        dominate, dominated, Alignment.LawfulEvil, spec.Protected[index], spec.Canonical[index], spec.Aura[index]));
                // Alignment-specific: a good controller is never blocked.
                rows.Add(Sprints3031ReachProbe(controller, recipients[1], "band-good-controller", center, circle, aura,
                    dominate, dominated, Alignment.LawfulGood, false, spec.Canonical[1], spec.Aura[1]));
                bool exact = rows.All(row => (bool)row["exact"]);
                Sprint25Check("shared-circles-reach-" + spec.Id, exact, new JObject {
                    ["owners"] = new JArray(live.Select(value => value.UniqueId)), ["probes"] = rows,
                    ["enhancementEnabled"] = _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity,
                    ["canonicalRadiusMeters"] = circle.Area.Size.Meters, ["angelAuraRadiusMeters"] = AstralDevaRulesPolicy.AuraRadiusMeters },
                    "Actual native area grants and terminal control: Hound and Trumpet protect only inside 10 feet; the angel's own 20-foot aura carries the configured circle control protection between 10 and 20 feet; nothing reaches beyond 20 feet.");
                if (!exact) throw new InvalidOperationException("Shared circle reach mismatch: " + spec.Id);
                if (live.Count == 2)
                {
                    // Remove the angel first: only the Hound's 10-foot circle remains.
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { live[0] });
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    var remaining = new JArray();
                    for (int index = 0; index < probes.Length; index++)
                        remaining.Add(Sprints3031ReachProbe(controller, recipients[index], probes[index].Id, center, circle, aura,
                            dominate, dominated, Alignment.LawfulEvil, index == 0, index == 0 ? 1 : 0, 0));
                    bool sourceExact = live[0].Destroyed && !live[0].IsInState && remaining.All(row => (bool)row["exact"]);
                    Sprint25Check("shared-circles-reach-remove-angel", sourceExact, new JObject {
                        ["removedOwner"] = live[0].UniqueId, ["remainingOwner"] = live[1].UniqueId, ["probes"] = remaining },
                        "Removing the angel ends only its 20-foot grant; the Hound's own canonical circle still protects inside 10 feet.");
                    if (!sourceExact) throw new InvalidOperationException("Shared circle reach source removal mismatch.");
                }
                DisposeExpandedSummoningUnits(fixture.Created, live.Where(value => !value.Destroyed).ToArray());
                for (int frame = 0; frame < 8; frame++) yield return 0;
                bool clean = live.All(value => value.Destroyed && !value.IsInState) &&
                    !Game.Instance.State.AreaEffects.All.Except(areasBefore).Any() &&
                    Enumerable.Range(0, probes.Length).All(index =>
                        !recipients[index].Buffs.Enumerable.Except(startingBuffs[index]).Any());
                Sprint25Check("shared-circles-reach-" + spec.Id + "-removed", clean, new JObject {
                    ["areasExact"] = !Game.Instance.State.AreaEffects.All.Except(areasBefore).Any(),
                    ["recipientBuffsExact"] = new JArray(Enumerable.Range(0, probes.Length).Select(index =>
                        !recipients[index].Buffs.Enumerable.Except(startingBuffs[index]).Any())) },
                    "Removing the emitters ends their areas and every recipient grant before the next fixed case.");
                if (!clean) throw new InvalidOperationException("Shared circle reach cleanup failed: " + spec.Id);
            }
            DisposeExpandedSummoningUnits(fixture.Created, recipients);
            for (int frame = 0; frame < 8; frame++) yield return 0;
        }

        private JObject Sprints3031ReachProbe(UnitEntityData controller, UnitEntityData recipient, string id,
            Vector3 center, MagicCircleBlueprintSet circle, BlueprintBuff aura, BlueprintAbility dominate,
            BlueprintBuff dominated, Alignment alignment, bool covered, int canonical, int angel)
        {
            var capture = new CircleControlCastCapture(recipient, dominated);
            Buff control = null;
            controller.Descriptor.Alignment.Set(alignment);
            EventBus.Subscribe(capture);
            try
            {
                int canonicalGrants = CircleBuffs(recipient, circle.Recipient).Length;
                int angelGrants = CircleBuffs(recipient, aura).Length;
                var context = new AbilityExecutionContext(new AbilityData(dominate, controller.Descriptor),
                    new AbilityParams { CasterLevel = 20, SpellLevel = 5, DC = 25 },
                    new TargetWrapper(recipient), Rulebook.CurrentContext);
                control = recipient.Buffs.AddBuff(dominated, context, TimeSpan.FromMinutes(1));
                bool block = covered && alignment == Alignment.LawfulEvil &&
                    _context.FeatureModules.Active.ProtectionFromAlignmentControlImmunity;
                bool exact = canonicalGrants == canonical && angelGrants == angel && capture.Applications == 1 &&
                    (block ? control == null && !capture.CanApply : control != null && capture.CanApply);
                var offset = recipient.Position - center;
                return new JObject {
                    ["probe"] = id, ["recipient"] = recipient.UniqueId, ["controllerAlignment"] = alignment.ToString(),
                    ["distance2DMeters"] = new Vector2(offset.x, offset.z).magnitude, ["corpulence"] = recipient.Corpulence,
                    ["canonicalCircleGrants"] = canonicalGrants, ["expectedCanonicalCircleGrants"] = canonical,
                    ["angelAuraGrants"] = angelGrants, ["expectedAngelAuraGrants"] = angel,
                    ["applications"] = capture.Applications, ["canApply"] = capture.CanApply,
                    ["applied"] = control != null, ["expectedBlocked"] = block, ["exact"] = exact };
            }
            finally { EventBus.Unsubscribe(capture); control?.Remove(); }
        }
    }
}
