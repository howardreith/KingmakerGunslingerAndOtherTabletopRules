using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
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
        /// The Vrock's spores, observed on live units rather than argued from
        /// constants. Five protection states are kept apart — susceptible
        /// exposure, pre-existing disease protection, poison-only immunity, a
        /// real native cure after exposure, and protection acquired after
        /// exposure — and the spatial relation is measured, with every
        /// candidate the native collector returned recorded next to the set
        /// that was actually exposed.
        ///
        /// <para>No disease feature is hard-coded. The protection facts are
        /// discovered in the installed library by component shape, which is
        /// also what keeps the two native immunity paths distinguishable: a
        /// fact carrying only <c>BuffDescriptorImmunity</c> is exactly the
        /// case that used to slip past the exposure gate.</para>
        /// </summary>
        private IEnumerable<int> ReviewVrockSpores37(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint37VrockSporesPhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            UnitEntityData ally = fixture.Caster;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetDamage = target.Damage;
            int oldAllyDamage = ally.Damage;
            Vector3 oldTargetPosition = target.Position;
            Vector3 oldAllyPosition = ally.Position;
            var grantedFacts = new List<Fact>();
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint37-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint37 vrock spore cell failed: " + id);
                };

            try
            {
                var spores = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                        .Vrock37Prefix + "Spores"];
                var vines = (BlueprintBuff)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                        .Vrock37Prefix + "SporeVines"];
                var cooldown = (BlueprintBuff)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                        .Vrock37Prefix + "SporeCooldown"];

                VrockProtectionFacts37 protection =
                    VrockDiscoverProtection37(check);

                target.Stats.HitPoints.BaseValue = 100000;
                UnitEntityData vrock = CastExpandedSummoningOwnTier(
                    fixture, "vrock");
                owned.Add(vrock);
                Sprint28SetBrainActivePreservingActions(vrock, false);
                vrock.Commands.InterruptAll(true);
                vrock.Commands.RemoveFinishedAndUpdateQueue();
                vrock.Stats.HitPoints.BaseValue = 100000;
                foreach (int frame in Outsider30NativeAppearance(vrock,
                    vrock.Blueprint.name)) yield return frame;

                // The descriptor the native cure and the native immunity both
                // read is the one on the ABILITY's context, so record it here
                // before any resolution: a Disease bit present only on the
                // vine blueprint would leave every counter inert.
                SpellDescriptor abilityDescriptor = VrockDescriptorOf37(spores);
                SpellDescriptor vineDescriptor = VrockDescriptorOf37(vines);
                check("spore-descriptor-placement",
                    (abilityDescriptor & SpellDescriptor.Disease) != 0 &&
                    (vineDescriptor & SpellDescriptor.Disease) != 0 &&
                    (abilityDescriptor & SpellDescriptor.Poison) == 0 &&
                    (vineDescriptor & SpellDescriptor.Poison) == 0,
                    new JObject {
                        ["ability"] = spores.AssetGuid,
                        ["abilityDescriptor"] = (long)abilityDescriptor,
                        ["vines"] = vines.AssetGuid,
                        ["vineDescriptor"] = (long)vineDescriptor,
                        ["diseaseBit"] = (long)SpellDescriptor.Disease,
                        ["poisonBit"] = (long)SpellDescriptor.Poison,
                        ["why"] = "Every native consumer reads MechanicsContext.SpellDescriptor, and a Buff keeps the context it was handed, so the ability must carry the bit."
                    }, "The Disease descriptor sits on the ability whose context the vines carry, and neither blueprint claims the poison descriptor.");

                // --- case 1: susceptible ------------------------------------
                foreach (int frame in VrockSporeExposure37Case(fixture, vrock,
                    target, spores, vines, cooldown, "susceptible", true,
                    true, check)) yield return frame;

                // --- case 2: pre-existing disease protection -----------------
                foreach (int frame in VrockSporeResetCase37(fixture, vrock,
                    target, vines, cooldown)) yield return frame;
                Fact diseaseFact = target.Descriptor.AddFact(
                    protection.Disease);
                grantedFacts.Add(diseaseFact);
                foreach (int frame in VrockSporeExposure37Case(fixture, vrock,
                    target, spores, vines, cooldown, "disease-immune", false,
                    false, check)) yield return frame;
                target.Descriptor.RemoveFact(diseaseFact);
                grantedFacts.Remove(diseaseFact);

                // --- case 3: poison immunity is not disease protection -------
                foreach (int frame in VrockSporeResetCase37(fixture, vrock,
                    target, vines, cooldown)) yield return frame;
                Fact poisonFact = target.Descriptor.AddFact(
                    protection.Poison);
                grantedFacts.Add(poisonFact);
                foreach (int frame in VrockSporeExposure37Case(fixture, vrock,
                    target, spores, vines, cooldown, "poison-immune-only",
                    true, true, check)) yield return frame;
                target.Descriptor.RemoveFact(poisonFact);
                grantedFacts.Remove(poisonFact);

                // --- case 4: the real native cure after exposure -------------
                foreach (int frame in VrockSporeCure37(fixture, vrock, target,
                    spores, vines, cooldown, check)) yield return frame;

                // --- case 5: protection acquired after exposure --------------
                foreach (int frame in VrockSporeLateImmunity37(fixture, vrock,
                    target, spores, vines, cooldown, protection, grantedFacts,
                    check)) yield return frame;

                // --- the spatial relation, measured -------------------------
                foreach (int frame in VrockSporeGeometry37(fixture, vrock,
                    target, ally, spores, vines, cooldown, check))
                    yield return frame;
            }
            finally
            {
                foreach (Fact fact in grantedFacts.ToArray())
                    if (fact != null && !fact.IsDisposed)
                        target.Descriptor.RemoveFact(fact);
                foreach (Buff buff in new[] { target, ally }
                    .SelectMany(unit => unit.Buffs.Enumerable).ToArray())
                    if (buff.Blueprint != null && buff.Blueprint.name
                        .StartsWith("KMG_", StringComparison.Ordinal))
                        buff.Remove();
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                target.Damage = oldTargetDamage;
                ally.Damage = oldAllyDamage;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                PlaceExpandedSummoningUnit(ally, oldAllyPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory, "sprint37-vrock-spores.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        private sealed class VrockProtectionFacts37
        {
            internal BlueprintUnitFact Disease;
            internal BlueprintUnitFact Poison;
            internal bool DiseaseIsBuffOnly;
        }

        /// <summary>
        /// Finds the protection facts in the installed library by the shape of
        /// their components, never by a memorised identifier. A fact whose only
        /// disease component is <c>BuffDescriptorImmunity</c> is preferred,
        /// because that is precisely the creature the exposure gate used to
        /// mis-answer.
        /// </summary>
        private static VrockProtectionFacts37 VrockDiscoverProtection37(
            Action<string, bool, JObject, string> check)
        {
            BlueprintUnitFact[] all = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintUnitFact>()
                .Where(value => value != null &&
                    !value.name.StartsWith("KMG_", StringComparison.Ordinal))
                .ToArray();
            Func<BlueprintUnitFact, bool> buffOnlyDisease = fact =>
                fact.GetComponents<BuffDescriptorImmunity>().Any(value =>
                    (value.Descriptor.Value & SpellDescriptor.Disease) != 0) &&
                !fact.GetComponents<SpellImmunityToSpellDescriptor>().Any(
                    value => (value.Descriptor.Value &
                        SpellDescriptor.Disease) != 0);
            Func<BlueprintUnitFact, bool> anyDisease = fact =>
                fact.GetComponents<BuffDescriptorImmunity>().Any(value =>
                    (value.Descriptor.Value & SpellDescriptor.Disease) != 0) ||
                fact.GetComponents<SpellImmunityToSpellDescriptor>().Any(
                    value => (value.Descriptor.Value &
                        SpellDescriptor.Disease) != 0);
            BlueprintUnitFact[] buffOnly = all.Where(buffOnlyDisease)
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .ToArray();
            BlueprintUnitFact[] disease = all.Where(anyDisease)
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .ToArray();
            BlueprintUnitFact[] poison = all.Where(fact =>
                    !anyDisease(fact) &&
                    (fact.GetComponents<BuffDescriptorImmunity>().Any(value =>
                        (value.Descriptor.Value &
                            SpellDescriptor.Poison) != 0) ||
                     fact.GetComponents<SpellImmunityToSpellDescriptor>()
                        .Any(value => (value.Descriptor.Value &
                            SpellDescriptor.Poison) != 0)))
                .OrderBy(value => value.name, StringComparer.Ordinal)
                .ToArray();
            var result = new VrockProtectionFacts37 {
                Disease = buffOnly.FirstOrDefault() ??
                    disease.FirstOrDefault(),
                Poison = poison.FirstOrDefault(),
                DiseaseIsBuffOnly = buffOnly.Length > 0
            };
            check("spore-protection-facts",
                result.Disease != null && result.Poison != null,
                new JObject {
                    ["searchedFacts"] = all.Length,
                    ["diseaseCandidates"] = new JArray(disease
                        .Select(value => value.name).Take(32)),
                    ["buffOnlyDiseaseCandidates"] = new JArray(buffOnly
                        .Select(value => value.name).Take(32)),
                    ["poisonOnlyCandidates"] = new JArray(poison
                        .Select(value => value.name).Take(32)),
                    ["selectedDisease"] = result.Disease?.name,
                    ["selectedDiseaseGuid"] = result.Disease?.AssetGuid,
                    ["selectedDiseaseIsBuffDescriptorOnly"] =
                        result.DiseaseIsBuffOnly,
                    ["selectedPoison"] = result.Poison?.name,
                    ["selectedPoisonGuid"] = result.Poison?.AssetGuid,
                    ["searchScope"] =
                        "Loaded non-KMG BlueprintUnitFacts of this installation, selected by component shape; this records what that scan found and claims nothing about other installations or other mods."
                }, "Real installed facts grant disease protection and poison immunity, found by component shape rather than by a memorised identifier.");
            return result;
        }

        private static SpellDescriptor VrockDescriptorOf37(
            BlueprintScriptableObject blueprint)
        {
            var component = blueprint
                .GetComponents<SpellDescriptorComponent>().FirstOrDefault();
            return component == null ? SpellDescriptor.None :
                component.Descriptor.Value;
        }

        /// <summary>
        /// One exposure, with the native collector's own candidate list, the
        /// opening damage rule and every vine application recorded, then
        /// compared with what the protection state should allow.
        /// </summary>
        private IEnumerable<int> VrockSporeExposure37Case(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData vrock,
            UnitEntityData target, BlueprintAbility spores,
            BlueprintBuff vines, BlueprintBuff cooldown, string label,
            bool expectImpact, bool expectVines,
            Action<string, bool, JObject, string> check)
        {
            VrockPlaceAdjacent37(vrock, target, 0.3f);
            JObject geometry = VrockGeometryRow37(vrock, target, spores);
            var observed = new Astral31SmiteEvents(vrock, spores, vines, null,
                new[] { target });
            EventBus.Subscribe(observed);
            UnitUseAbility command = null;
            try
            {
                int before = target.Damage;
                foreach (int frame in Sprint26Cast(vrock, spores,
                    new TargetWrapper(vrock), value => command = value))
                    yield return frame;
                RuleDealDamage impact = observed.Damage(target);
                Buff grown = target.Buffs.Enumerable.SingleOrDefault(value =>
                    ReferenceEquals(value.Blueprint, vines) &&
                    ReferenceEquals(value.Context?.MaybeCaster, vrock));
                RuleApplyBuff[] applications = observed.Applications.Where(
                    value => ReferenceEquals(value.Initiator, target))
                    .ToArray();
                bool commandExact = command != null && command.IsFinished &&
                    command.Result == UnitCommand.ResultType.Success &&
                    command.ExecutionProcess != null &&
                    command.ExecutionProcess.IsEnded;
                bool impactSeen = impact != null && impact.Damage > 0;
                bool coherent = commandExact &&
                    impactSeen == expectImpact &&
                    (grown != null) == expectVines &&
                    // The gate is one decision: a refused creature must not be
                    // damaged first and refused afterwards.
                    (expectImpact || target.Damage == before) &&
                    (expectVines || applications.Length == 0) &&
                    vrock.Buffs.GetBuff(cooldown) != null;
                check("spore-" + label, coherent, new JObject {
                    ["owner"] = vrock.UniqueId,
                    ["target"] = target.UniqueId,
                    ["command"] = command == null ? null :
                        Sprint27AiCommand(command),
                    ["geometry"] = geometry,
                    ["row"] = observed.Row(target),
                    ["damageBefore"] = before,
                    ["damageAfter"] = target.Damage,
                    ["impactDamage"] = impact?.Damage,
                    ["vineApplications"] = applications.Length,
                    ["vines"] = grown == null ? null :
                        InfernalSavedEffect34(grown),
                    ["expectedImpact"] = expectImpact,
                    ["expectedVines"] = expectVines,
                    ["cooldownHeld"] = vrock.Buffs.GetBuff(cooldown) != null
                }, "Exposure state '" + label +
                    "' resolves one decision for both the opening packet and the growth, and spends the cadence exactly once.");
            }
            finally { EventBus.Unsubscribe(observed); }
        }

        /// <summary>
        /// Returns the fixture to a clean pre-exposure state between cases:
        /// the vines gone, damage cleared, and the three-round cadence waited
        /// out rather than cancelled, so the next case observes a real
        /// availability rather than a forced one.
        /// </summary>
        private IEnumerable<int> VrockSporeResetCase37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData vrock,
            UnitEntityData target, BlueprintBuff vines, BlueprintBuff cooldown)
        {
            foreach (Buff buff in target.Buffs.Enumerable.Where(value =>
                ReferenceEquals(value.Blueprint, vines)).ToArray())
                buff.Remove();
            target.Damage = 0;
            DateTime deadline = DateTime.UtcNow.AddSeconds(90);
            while (vrock.Buffs.GetBuff(cooldown) != null &&
                DateTime.UtcNow < deadline)
            {
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            if (vrock.Buffs.GetBuff(cooldown) != null)
                throw new InvalidOperationException(
                    "The vrock spore cadence did not expire within its own wait.");
        }

        /// <summary>
        /// The printed disease-removal counter, exercised with the real native
        /// Remove Disease spell cast by a real summoned Astral Deva. Nothing
        /// here removes the vines on the observer's behalf.
        /// </summary>
        private IEnumerable<int> VrockSporeCure37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData vrock,
            UnitEntityData target, BlueprintAbility spores,
            BlueprintBuff vines, BlueprintBuff cooldown,
            Action<string, bool, JObject, string> check)
        {
            foreach (int frame in VrockSporeResetCase37(fixture, vrock,
                target, vines, cooldown)) yield return frame;
            foreach (int frame in VrockSporeExposure37Case(fixture, vrock,
                target, spores, vines, cooldown, "before-cure", true, true,
                check)) yield return frame;
            Buff grown = target.Buffs.Enumerable.Single(value =>
                ReferenceEquals(value.Blueprint, vines) &&
                ReferenceEquals(value.Context?.MaybeCaster, vrock));
            UnitEntityData healer = CastExpandedSummoningOwnTier(fixture,
                "astral-deva");
            try
            {
                Sprint28SetBrainActivePreservingActions(healer, false);
                healer.Commands.InterruptAll(true);
                foreach (int frame in WaitInfernalSupport34(healer))
                    yield return frame;
                var cure = (BlueprintAbility)ExpandedSummoningBlueprints
                    .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                        .Astral31Prefix + "RemoveDisease"];
                PlaceExpandedSummoningUnit(healer, target.Position +
                    Vector3.right * (healer.Corpulence + target.Corpulence +
                        0.3f));
                var attempts = new JArray();
                for (int attempt = 0; attempt < 8 && !grown.IsDisposed;
                    attempt++)
                {
                    UnitUseAbility command = null;
                    foreach (int frame in Sprint26Cast(healer, cure,
                        new TargetWrapper(target), value => command = value))
                        yield return frame;
                    attempts.Add(new JObject {
                        ["attempt"] = attempt + 1,
                        ["command"] = command == null ? null :
                            Sprint27AiCommand(command),
                        ["disposed"] = grown.IsDisposed
                    });
                }
                check("spore-native-cure", grown.IsDisposed &&
                    !target.Buffs.Enumerable.Any(value =>
                        ReferenceEquals(value.Blueprint, vines)),
                    new JObject {
                        ["healer"] = healer.UniqueId,
                        ["target"] = target.UniqueId,
                        ["cure"] = cure.AssetGuid,
                        ["cureIsNativeSpell"] = true,
                        ["attempts"] = attempts,
                        ["dispelableFlagOnVines"] = false,
                        ["scope"] =
                            "The real native cure consumer removed the growth; the observer applied no removal of its own."
                    }, "Real Remove Disease strips the growing vines, which the IsNotDispelable flag would have prevented before the descriptor was ever read.");
            }
            finally
            {
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { healer });
            }
        }

        /// <summary>
        /// Protection that arrives while the vines are already growing.
        /// Neither native immunity component sweeps existing facts, so this is
        /// the one case the narrow per-round hook exists for: the growth must
        /// stop, and it must stop without the observer removing anything.
        /// </summary>
        private IEnumerable<int> VrockSporeLateImmunity37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData vrock,
            UnitEntityData target, BlueprintAbility spores,
            BlueprintBuff vines, BlueprintBuff cooldown,
            VrockProtectionFacts37 protection, List<Fact> grantedFacts,
            Action<string, bool, JObject, string> check)
        {
            foreach (int frame in VrockSporeResetCase37(fixture, vrock,
                target, vines, cooldown)) yield return frame;
            foreach (int frame in VrockSporeExposure37Case(fixture, vrock,
                target, spores, vines, cooldown, "before-late-immunity", true,
                true, check)) yield return frame;
            Buff grown = target.Buffs.Enumerable.Single(value =>
                ReferenceEquals(value.Blueprint, vines) &&
                ReferenceEquals(value.Context?.MaybeCaster, vrock));
            int damageAtGrant = target.Damage;
            Fact late = target.Descriptor.AddFact(protection.Disease);
            grantedFacts.Add(late);
            var ticks = new JArray();
            double start = Game.Instance.TimeController.GameTime.TotalSeconds;
            DateTime deadline = DateTime.UtcNow.AddSeconds(45);
            while (!grown.IsDisposed && DateTime.UtcNow < deadline &&
                Game.Instance.TimeController.GameTime.TotalSeconds - start <
                    GameConsts.RoundDuration * 3)
            {
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            ticks.Add(new JObject {
                ["elapsedGameSeconds"] =
                    Game.Instance.TimeController.GameTime.TotalSeconds - start,
                ["roundDuration"] = GameConsts.RoundDuration,
                ["disposed"] = grown.IsDisposed,
                ["damageAtGrant"] = damageAtGrant,
                ["damageNow"] = target.Damage
            });
            // One already-begun round may still land its 1d4 before the next
            // new-round check, which is what "halted" means for a per-round
            // growth. What must not happen is unbounded continuation.
            bool halted = grown.IsDisposed;
            check("spore-late-immunity", halted, new JObject {
                ["owner"] = vrock.UniqueId,
                ["target"] = target.UniqueId,
                ["grantedFact"] = protection.Disease.name,
                ["grantedFactGuid"] = protection.Disease.AssetGuid,
                ["grantedFactIsBuffDescriptorOnly"] =
                    protection.DiseaseIsBuffOnly,
                ["observations"] = ticks,
                ["removedByObserver"] = false,
                ["why"] =
                    "BuffDescriptorImmunity only handles RuleApplyBuff as it happens and SpellImmunityToSpellDescriptor only registers on turn-on; neither cancels a growth that already exists."
            }, "Disease protection acquired after exposure halts the growth within the next round, through the narrow per-round hook rather than an observer removal.");
            target.Descriptor.RemoveFact(late);
            grantedFacts.Remove(late);
            target.Damage = 0;
        }

        /// <summary>
        /// The spatial relation, measured. Three placements — just inside
        /// adjacency, just outside it, and inside the broad-phase collection
        /// radius but well outside adjacency — are each recorded with both
        /// corpulences, the centre distance, the edge distance, the native
        /// collector's own candidate list and the set that was finally
        /// exposed.
        /// </summary>
        private IEnumerable<int> VrockSporeGeometry37(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData vrock,
            UnitEntityData target, UnitEntityData ally,
            BlueprintAbility spores, BlueprintBuff vines,
            BlueprintBuff cooldown,
            Action<string, bool, JObject, string> check)
        {
            float tolerance = SummonFootprintGeometry.AdjacencyToleranceMeters;
            var placements = new[] {
                new { Label = "inside-edge", Gap = tolerance - 0.1f,
                    Expect = true },
                new { Label = "outside-edge", Gap = tolerance + 0.3f,
                    Expect = false },
                new { Label = "collected-not-adjacent",
                    Gap = tolerance + 1.2f, Expect = false }
            };
            var rows = new JArray();
            bool allCoherent = true;
            foreach (var placement in placements)
            {
                foreach (int frame in VrockSporeResetCase37(fixture, vrock,
                    target, vines, cooldown)) yield return frame;
                // Park the ally far away so one target at a time is decided.
                PlaceExpandedSummoningUnit(ally, vrock.Position +
                    Vector3.forward * 40f);
                VrockPlaceAdjacent37(vrock, target, placement.Gap);
                JObject geometry = VrockGeometryRow37(vrock, target, spores);
                var observed = new Astral31SmiteEvents(vrock, spores, vines,
                    null, new[] { target, ally });
                EventBus.Subscribe(observed);
                UnitUseAbility command = null;
                try
                {
                    foreach (int frame in Sprint26Cast(vrock, spores,
                        new TargetWrapper(vrock), value => command = value))
                        yield return frame;
                    bool exposed = observed.Damage(target) != null ||
                        target.Buffs.Enumerable.Any(value =>
                            ReferenceEquals(value.Blueprint, vines));
                    bool collected = (bool)geometry["collected"];
                    bool predicted = SummonFootprintGeometry.AreAdjacent(
                        vrock, target);
                    bool coherent = command != null && command.IsFinished &&
                        exposed == placement.Expect &&
                        predicted == placement.Expect &&
                        // Every placement is inside the broad-phase bound, so
                        // a refusal is the footprint relation's doing and not
                        // the collector dropping the target.
                        collected;
                    allCoherent &= coherent;
                    geometry["label"] = placement.Label;
                    geometry["requestedGapMeters"] = placement.Gap;
                    geometry["expectedExposed"] = placement.Expect;
                    geometry["observedExposed"] = exposed;
                    geometry["productionPredicate"] = predicted;
                    geometry["coherent"] = coherent;
                    geometry["row"] = observed.Row(target);
                    rows.Add(geometry);
                }
                finally { EventBus.Unsubscribe(observed); }
            }
            check("spore-footprint-boundary", allCoherent, new JObject {
                ["owner"] = vrock.UniqueId,
                ["target"] = target.UniqueId,
                ["toleranceMeters"] = tolerance,
                ["collectionRadiusFeet"] =
                    VrockRules37Policy.SporeCollectionRadiusFeet,
                ["reachFeet"] = VrockRules37Policy.ReachFeet,
                ["placements"] = rows,
                ["scope"] =
                    "Measured on these live footprints at these positions; the collector candidates are the native GetTargetsAround result, not a reimplementation."
            }, "Exposure follows the measured footprint relation on both sides of the one-square boundary, and a creature the native collector returned but which is not adjacent is still refused.");
        }

        /// <summary>
        /// Places a unit so the gap between the two footprint edges is the
        /// requested distance, which is the quantity the relation is about.
        /// </summary>
        private static void VrockPlaceAdjacent37(UnitEntityData vrock,
            UnitEntityData target, float edgeGapMeters)
        {
            Vector3 position = vrock.Position + Vector3.right *
                (vrock.Corpulence + target.Corpulence + edgeGapMeters);
            PlaceExpandedSummoningUnit(target, position);
        }

        private static JObject VrockGeometryRow37(UnitEntityData vrock,
            UnitEntityData target, BlueprintAbility spores)
        {
            float radius = new Feet(
                VrockRules37Policy.SporeCollectionRadiusFeet).Meters;
            UnitEntityData[] candidates = (GameHelper.GetTargetsAround(
                vrock.EyePosition, new Feet(
                    VrockRules37Policy.SporeCollectionRadiusFeet), true,
                false) ?? Enumerable.Empty<UnitEntityData>())
                .Where(value => value != null).ToArray();
            return new JObject {
                ["vrock"] = vrock.UniqueId,
                ["vrockCorpulence"] = vrock.Corpulence,
                ["target"] = target.UniqueId,
                ["targetCorpulence"] = target.Corpulence,
                ["centreDistanceMeters"] =
                    Vector3.Distance(vrock.Position, target.Position),
                ["eyeDistanceMeters"] =
                    Vector3.Distance(vrock.EyePosition, target.Position),
                ["edgeDistanceMeters"] =
                    SummonFootprintGeometry.EdgeDistanceMeters(vrock, target),
                ["collectionRadiusMeters"] = radius,
                ["collected"] = candidates.Contains(target),
                ["candidates"] = new JArray(candidates.Select(value =>
                    new JObject {
                        ["unit"] = value.UniqueId,
                        ["corpulence"] = value.Corpulence,
                        ["centreDistanceMeters"] = Vector3.Distance(
                            vrock.Position, value.Position),
                        ["eyeDistanceMeters"] = Vector3.Distance(
                            vrock.EyePosition, value.Position),
                        ["edgeDistanceMeters"] = SummonFootprintGeometry
                            .EdgeDistanceMeters(vrock, value),
                        ["adjacent"] = SummonFootprintGeometry.AreAdjacent(
                            vrock, value)
                    }))
            };
        }
    }
}
