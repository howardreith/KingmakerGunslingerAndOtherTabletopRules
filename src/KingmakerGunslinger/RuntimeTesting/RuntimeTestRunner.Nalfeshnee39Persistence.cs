using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// Whether this persistence run carries the Sprint 39 scope.
        /// </summary>
        private bool Sprint39PersistenceScope
        {
            get
            {
                return _request != null && _request.Parameters != null &&
                    (string)_request.Parameters["persistenceScope"] ==
                        ExpandedSummoningRosterPersistencePolicy
                            .NalfeshneeScope;
            }
        }

        private static T Nalfeshnee39Saved<T>(string suffix)
            where T : BlueprintScriptableObject
        {
            return (T)ExpandedSummoningBlueprints.Registered.BySymbol[
                ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix + suffix];
        }

        private static UnitEntityData[] Nalfeshnee39SavedSources(
            UnitEntityData[] sources)
        {
            return sources.Where(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    NalfeshneeVisualPolicy39.Nalfeshnee)
                // A stable order so the same unit is the first owner in every
                // stage of an envelope, independent of enumeration.
                .OrderBy(unit => unit.UniqueId, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>
        /// What this creature actually persists, and why each row has to be
        /// here.
        ///
        /// <para>Unholy Nimbus is a DELAYED effect with three daily uses, so
        /// two things must cross a save or the ability is either free or
        /// broken: the spent use, and the pending windup with its own
        /// absolute deadline. The offline reading is what makes this the
        /// decisive envelope rather than a formality - the engine's own
        /// delayed-delivery mechanism keeps its pending state in a bare
        /// coroutine that is lost on a cold load, which is precisely why
        /// this implementation carries the deadline on a BUFF whose end time
        /// and context the engine does serialize, and why the component's
        /// own armed flag rides the per-component serialization bag.</para>
        ///
        /// <para>Unlike the released Sprint 38 envelope, the pending effect
        /// is deliberately NOT withdrawn before the save. That one had to
        /// withdraw its stun because a real stun stops an owned unit acting
        /// and the shared roster contract requires every owned unit to be
        /// able to act at the boundary. A nimbus windup imposes no
        /// restriction at all on its owner, which may keep acting normally
        /// while one is pending - so what crosses this save really is the
        /// pending windup itself, with no readiness waiver needed to get it
        /// there.</para>
        ///
        /// <para>Two owners, at deliberately DIFFERENT deadlines. One would
        /// have shown that a windup survives; two at distinct instants show
        /// that each fact carries its own state, which is the claim that
        /// would fail if the flags or the deadline were shared, static or
        /// recomputed on load rather than serialized per fact.</para>
        /// </summary>
        private IEnumerable<int> PrepareNalfeshneeSaved39(
            UnitEntityData caster, UnitEntityData[] sources)
        {
            UnitEntityData[] nalfeshnees = Nalfeshnee39SavedSources(sources);
            if (nalfeshnees.Length != 2)
                throw new InvalidOperationException(
                    "The Sprint 39 persistence scope requires its two summoned nalfeshnees.");
            var nimbus = Nalfeshnee39Saved<BlueprintAbility>("UnholyNimbus");
            var windup = Nalfeshnee39Saved<BlueprintBuff>(
                "UnholyNimbusWindup");
            var resource = Nalfeshnee39Saved<BlueprintAbilityResource>(
                "UnholyNimbusResource");
            foreach (UnitEntityData unit in nalfeshnees)
                if (unit.Descriptor.Resources.GetResourceAmount(resource) !=
                    NalfeshneeRules39Policy.NimbusUsesPerDay)
                    throw new InvalidOperationException(
                        "A Sprint 39 nalfeshnee does not hold its three printed uses before preparation.");

            var rows = new JArray();
            bool coherent = true;
            var deadlines = new List<double>();
            for (int index = 0; index < nalfeshnees.Length; index++)
            {
                UnitEntityData owner = nalfeshnees[index];
                if (index > 0)
                {
                    // A real gap in game time between the two activations, so
                    // the two absolute deadlines are genuinely different
                    // instants rather than the same number twice.
                    double start = Game.Instance.TimeController.GameTime
                        .TotalSeconds;
                    bool paused = Game.Instance.IsPaused;
                    Game.Instance.IsPaused = false;
                    int frames = 0;
                    while (frames++ < 600 &&
                        Game.Instance.TimeController.GameTime.TotalSeconds -
                            start < GameConsts.RoundDuration * 0.25)
                        yield return 0;
                    Game.Instance.IsPaused = paused;
                }

                // The production command, not a fixture buff application. A
                // windup planted by hand would carry no spent use and would
                // not prove the activation path serializes anything.
                UnitUseAbility command = null;
                foreach (int step in Sprint26Cast(owner, nimbus,
                    new TargetWrapper(owner), value => command = value))
                    yield return step;
                Buff pending = owner.Descriptor.Buffs.GetBuff(windup);
                int held = owner.Descriptor.Resources
                    .GetResourceAmount(resource);
                bool armed = NalfeshneeNimbusWindup39.ObservedArmed(owner,
                    windup);
                double remaining = pending == null ? 0 :
                    pending.TimeLeft.TotalSeconds;
                if (pending != null) deadlines.Add(
                    pending.EndTime.TotalSeconds);
                bool row = command != null &&
                    command.Result == Kingmaker.UnitLogic.Commands.Base
                        .UnitCommand.ResultType.Success &&
                    pending != null && armed && remaining > 0 &&
                    held == NalfeshneeRules39Policy.NimbusUsesPerDay - 1 &&
                    owner.Descriptor.Buffs.Enumerable.Count(value =>
                        ReferenceEquals(value.Blueprint, windup)) == 1 &&
                    // The readiness contract this envelope does NOT need a
                    // waiver for: a pending windup leaves its owner able to
                    // act and move, which is why it crosses the save intact.
                    owner.Descriptor.State.CanAct &&
                    owner.Descriptor.State.CanMove;
                coherent &= row;
                rows.Add(new JObject {
                    ["nalfeshnee"] = owner.UniqueId,
                    ["commandResult"] = command == null ? null :
                        command.Result.ToString(),
                    ["pendingCreated"] = pending != null,
                    ["pendingArmed"] = armed,
                    ["pendingCount"] = owner.Descriptor.Buffs.Enumerable
                        .Count(value => ReferenceEquals(value.Blueprint,
                            windup)),
                    ["deadlineGameSeconds"] = pending == null ? 0 :
                        pending.EndTime.TotalSeconds,
                    ["remainingGameSeconds"] = remaining,
                    ["usesRemaining"] = held,
                    ["printedUsesPerDay"] =
                        NalfeshneeRules39Policy.NimbusUsesPerDay,
                    ["canAct"] = owner.Descriptor.State.CanAct,
                    ["canMove"] = owner.Descriptor.State.CanMove,
                    ["coherent"] = row });
            }
            bool distinct = deadlines.Count == 2 &&
                Math.Abs(deadlines[0] - deadlines[1]) > 0.001;
            RosterPersistenceCheck("s39-nimbus-pending-armed", coherent &&
                    distinct,
                new JObject {
                    ["owners"] = rows,
                    ["deadlines"] = new JArray(deadlines),
                    ["deadlinesDistinct"] = distinct,
                    ["pendingWithdrawnBeforeSave"] = false,
                    ["whyItIsNotWithdrawn"] = "The Sprint 38 envelope had to withdraw its stun because a real stun stops an owned unit acting and the shared roster contract requires every owned unit to be able to act at the save boundary. A nimbus windup imposes no restriction on its owner, so it crosses this save intact and no readiness waiver is needed."
                }, "Two real activations leave each nalfeshnee with one armed pending windup at its own distinct absolute deadline and one spent use of three, both owners still able to act and move, so the pending state reaches the save without being withdrawn.");
            if (!(coherent && distinct))
                throw new InvalidOperationException(
                    "Sprint 39 pending nimbus state did not prepare cleanly.");
        }

        /// <summary>
        /// What survived the save.
        ///
        /// <para>Four separate things, and the first two are what a weaker
        /// implementation gets wrong. A windup that came back PRESENT but
        /// unarmed would never resolve while looking entirely healthy, so
        /// armedness is read off the live per-fact component rather than
        /// inferred from presence. A deadline that came back RESTARTED would
        /// hand the player a fresh round of warning on every load, so the
        /// two absolute deadlines are compared with the ones recorded before
        /// the save and with each other. Then the spent uses, because a use
        /// that came back would make a limited ability unlimited; and the
        /// printed constant aura, because it is a permanent self effect that
        /// has to be reinstated.</para>
        /// </summary>
        private void ObserveNalfeshneeSaved39(UnitEntityData caster,
            UnitEntityData[] sources, string phase)
        {
            var windup = Nalfeshnee39Saved<BlueprintBuff>(
                "UnholyNimbusWindup");
            var daze = Nalfeshnee39Saved<BlueprintBuff>("UnholyNimbusDaze");
            var aura = Nalfeshnee39Saved<BlueprintBuff>("UnholyAura");
            var condition = Nalfeshnee39Saved<BlueprintBuff>(
                "FeeblemindCondition");
            var resource = Nalfeshnee39Saved<BlueprintAbilityResource>(
                "UnholyNimbusResource");
            UnitEntityData[] nalfeshnees = Nalfeshnee39SavedSources(sources);
            if (nalfeshnees.Length != 2)
                throw new InvalidOperationException(
                    "Exact saved Sprint 39 nalfeshnee pair absent.");

            var rows = new JArray();
            bool coherent = true;
            var deadlines = new List<double>();
            foreach (UnitEntityData owner in nalfeshnees)
            {
                Buff pending = owner.Descriptor.Buffs.GetBuff(windup);
                bool armed = NalfeshneeNimbusWindup39.ObservedArmed(owner,
                    windup);
                int held = owner.Descriptor.Resources
                    .GetResourceAmount(resource);
                if (pending != null) deadlines.Add(
                    pending.EndTime.TotalSeconds);
                bool row = pending != null && armed &&
                    pending.TimeLeft > TimeSpan.Zero &&
                    held == NalfeshneeRules39Policy.NimbusUsesPerDay - 1 &&
                    owner.Descriptor.Buffs.Enumerable.Count(value =>
                        ReferenceEquals(value.Blueprint, windup)) == 1 &&
                    owner.Descriptor.Buffs.GetBuff(aura) != null &&
                    owner.Descriptor.State.CanAct &&
                    owner.Descriptor.State.CanMove;
                coherent &= row;
                rows.Add(new JObject {
                    ["nalfeshnee"] = owner.UniqueId,
                    ["pendingPresent"] = pending != null,
                    ["pendingArmed"] = armed,
                    ["pendingCount"] = owner.Descriptor.Buffs.Enumerable
                        .Count(value => ReferenceEquals(value.Blueprint,
                            windup)),
                    ["deadlineGameSeconds"] = pending == null ? 0 :
                        pending.EndTime.TotalSeconds,
                    ["remainingGameSeconds"] = pending == null ? 0 :
                        pending.TimeLeft.TotalSeconds,
                    ["usesRemaining"] = held,
                    ["auraReinstated"] =
                        owner.Descriptor.Buffs.GetBuff(aura) != null,
                    ["canAct"] = owner.Descriptor.State.CanAct,
                    ["canMove"] = owner.Descriptor.State.CanMove,
                    ["coherent"] = row });
            }
            bool distinct = deadlines.Count == 2 &&
                Math.Abs(deadlines[0] - deadlines[1]) > 0.001;
            // No burst may have happened: the windups are still pending, so
            // nothing may be dazed anywhere and no feeblemind clone may exist.
            int dazes = Game.Instance.State.Units.All
                .Where(unit => unit != null && unit.IsInState)
                .Sum(unit => unit.Buffs.Enumerable.Count(buff =>
                    ReferenceEquals(buff.Blueprint, daze)));
            int conditions = Game.Instance.State.Units.All
                .Where(unit => unit != null && unit.IsInState)
                .Sum(unit => unit.Buffs.Enumerable.Count(buff =>
                    ReferenceEquals(buff.Blueprint, condition)));
            bool exact = coherent && distinct && dazes == 0 &&
                conditions == 0;
            RosterPersistenceCheck("s39-state-" + phase, exact,
                new JObject {
                    ["owners"] = rows,
                    ["moduleEnabled"] =
                        _context.FeatureModules.Active.ExpandedSummoning,
                    ["deadlines"] = new JArray(deadlines),
                    ["deadlinesDistinct"] = distinct,
                    ["dazesAnywhere"] = dazes,
                    ["feeblemindConditionsAnywhere"] = conditions,
                    ["maxPendingPerOwner"] =
                        NalfeshneeRules39Policy.NimbusMaxPendingPerOwner,
                    ["armedReadFrom"] = "the live per-fact component instance, not the shared blueprint template"
                }, phase == "prepared"
                    ? "The prepared fixture carries one armed pending windup per nalfeshnee at two distinct absolute deadlines, one spent use of three each, the printed constant aura on both, and no burst has occurred anywhere."
                    : "A fresh load brings back both pending windups still ARMED - not merely present - at their own distinct absolute deadlines rather than restarted ones, keeps each owner's spent use spent, reinstates the printed constant aura, and shows no daze and no feeblemind anywhere, because neither windup has resolved yet.");
            if (!exact) throw new InvalidOperationException(
                "Sprint 39 saved pending, deadline, resource or aura state differs.");
        }

        /// <summary>
        /// Nothing of this creature's state may remain on a fresh or cleaned
        /// save, including on units it never touched.
        ///
        /// <para>The feeblemind clone is listed here for a reason specific to
        /// this creature: it is the one effect that deliberately has no
        /// duration and no caster-missing cleanup, so it is the one that
        /// would outlive a retirement if anything ever applied it outside a
        /// phase that owns its recipient.</para>
        /// </summary>
        private void NalfeshneeSavedAbsence39(UnitEntityData caster,
            string phase)
        {
            BlueprintBuff[] all = {
                Nalfeshnee39Saved<BlueprintBuff>("UnholyNimbusWindup"),
                Nalfeshnee39Saved<BlueprintBuff>("UnholyNimbusDaze"),
                Nalfeshnee39Saved<BlueprintBuff>("UnholyAura"),
                Nalfeshnee39Saved<BlueprintBuff>("FeeblemindCondition")
            };
            int effects = Game.Instance.State.Units.All
                .Where(unit => unit != null)
                .Sum(unit => unit.Buffs.Enumerable.Count(buff =>
                    all.Any(value => ReferenceEquals(buff.Blueprint, value))));
            int nalfeshnees = Game.Instance.State.Units.All
                .Count(unit => unit != null && !unit.Destroyed &&
                    unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                        NalfeshneeVisualPolicy39.Nalfeshnee);
            bool exact = effects == 0 && nalfeshnees == 0;
            RosterPersistenceCheck("s39-state-" + phase + "-absence", exact,
                new JObject {
                    ["nalfeshneeEffects"] = effects,
                    ["nalfeshnees"] = nalfeshnees,
                    ["observedBlueprints"] = new JArray(all
                        .Select(value => value.name))
                }, "No Sprint 39 pending windup, daze, constant aura or feeblemind condition and no summoned nalfeshnee remains after the cleanup save.");
            if (!exact) throw new InvalidOperationException(
                "Sprint 39 saved state remains.");
        }

        /// <summary>
        /// What each saved nalfeshnee owns. The released Sprint 36 to 38
        /// shape is reused unchanged - in Kingmaker a natural attack is
        /// itself a weapon item in the unit's own inventory, which is the
        /// mistake Sprint 37 recorded - with this creature's own count: three
        /// printed primary attacks, a bite and two claws, the two claws on
        /// additional limbs.
        /// </summary>
        private void ObserveNalfeshneeItems39(UnitEntityData[] sources,
            Kingmaker.Items.ItemEntity[] partyItems, int[] partyCounts)
        {
            UnitEntityData[] nalfeshnees = Nalfeshnee39SavedSources(sources);
            if (nalfeshnees.Length != 2)
                throw new InvalidOperationException(
                    "Exact saved Sprint 39 nalfeshnee pair absent for the item observation.");
            Kingmaker.Items.ItemEntity[] all = nalfeshnees.SelectMany(unit =>
                unit.Descriptor.Inventory.Items).ToArray();
            bool exact = all.Length != 0 &&
                all.Distinct().Count() == all.Length &&
                Sprint28PartyInventoryExact(partyItems, partyCounts);
            var rows = new JArray();
            foreach (UnitEntityData nalfeshnee in nalfeshnees)
            {
                var body = nalfeshnee.Blueprint.Body;
                string[] expected = new BlueprintItem[] {
                    body.PrimaryHand, body.SecondaryHand,
                    body.PrimaryHandAlternative1,
                    body.SecondaryHandAlternative1,
                    body.PrimaryHandAlternative2,
                    body.SecondaryHandAlternative2,
                    body.PrimaryHandAlternative3,
                    body.SecondaryHandAlternative3
                }.Where(value => value != null)
                    .Concat(body.AdditionalLimbs ??
                        Array.Empty<BlueprintItemWeapon>())
                    .Concat(body.AdditionalSecondaryLimbs ??
                        Array.Empty<BlueprintItemWeapon>())
                    .Select(value => value.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                Kingmaker.Items.ItemEntity[] items = nalfeshnee.Descriptor
                    .Inventory.Items.ToArray();
                bool row = items.Select(item => item.Blueprint.AssetGuid)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .SequenceEqual(expected) &&
                    items.All(item =>
                        ReferenceEquals(item.Owner, nalfeshnee.Descriptor) &&
                        item.IsNonRemovable && !item.IsLootable &&
                        item.Count == 1) &&
                    // Three project natural attacks - a bite and two claws -
                    // none of them a donor's, and no Glabrezu pincer. The core
                    // phase counts the same three on the live unit.
                    items.Length == 3 &&
                    items.All(item => item.Blueprint.name.StartsWith("KMG_",
                        StringComparison.Ordinal)) &&
                    nalfeshnee.Body.AdditionalLimbs
                        .Count(slot => slot.MaybeWeapon != null) == 2;
                exact &= row;
                rows.Add(new JObject {
                    ["nalfeshnee"] = nalfeshnee.UniqueId,
                    ["inventoryItems"] = items.Length,
                    ["inventoryBlueprints"] = new JArray(items
                        .Select(item => item.Blueprint.name)),
                    ["expectedBodyWeapons"] = expected.Length,
                    ["primaryWeapon"] = nalfeshnee.Body.PrimaryHand
                        .MaybeWeapon?.Blueprint?.name,
                    ["additionalArmedLimbs"] = nalfeshnee.Body.AdditionalLimbs
                        .Count(slot => slot.MaybeWeapon != null),
                    ["allOwnedNonRemovableSingle"] = items.All(item =>
                        ReferenceEquals(item.Owner, nalfeshnee.Descriptor) &&
                        item.IsNonRemovable && !item.IsLootable &&
                        item.Count == 1),
                    ["exact"] = row
                });
            }
            RosterPersistenceCheck("s39-items-owned", exact, new JObject {
                ["nalfeshnees"] = rows,
                ["distinctInstances"] = all.Distinct().Count(),
                ["totalInstances"] = all.Length,
                ["partyInventoryExact"] =
                    Sprint28PartyInventoryExact(partyItems, partyCounts)
            }, "Each saved nalfeshnee owns exactly its own three natural attacks as non-removable single items, shares no instance with the other, and the party's inventory is untouched.");
            if (!exact) throw new InvalidOperationException(
                "Sprint 39 saved item or limb state differs.");
        }
    }
}
