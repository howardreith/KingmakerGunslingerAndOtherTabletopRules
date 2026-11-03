using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// Whether this persistence run carries the Sprint 37 scope.
        /// </summary>
        private bool Sprint37PersistenceScope
        {
            get
            {
                return _request != null && _request.Parameters != null &&
                    (string)_request.Parameters["persistenceScope"] ==
                        ExpandedSummoningRosterPersistencePolicy.VrockScope;
            }
        }

        private static T VrockSavedBlueprint37<T>(string suffix)
            where T : BlueprintScriptableObject
        {
            return (T)ExpandedSummoningBlueprints.Registered.BySymbol[
                ExpandedSummoningSpecialBuilder.Vrock37Prefix + suffix];
        }

        private static UnitEntityData[] VrockSavedSources37(
            UnitEntityData[] sources)
        {
            return sources.Where(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    VrockVisualPolicy37.Vrock)
                // A stable order so the same unit is the owner in every stage
                // of an envelope, independent of enumeration.
                .OrderBy(unit => unit.UniqueId, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>
        /// The owner of the prepared state. Before preparation it is the first
        /// of the two in a stable order; afterwards, and on every later stage,
        /// it is the one actually carrying the hour marker - which is the fact
        /// under observation, so it is read rather than assumed.
        /// </summary>
        private static UnitEntityData VrockSavedOwner37(
            UnitEntityData[] vrocks, BlueprintBuff recharge)
        {
            UnitEntityData[] marked = vrocks
                .Where(unit => unit.Buffs.GetBuff(recharge) != null)
                .ToArray();
            if (marked.Length > 1) throw new InvalidOperationException(
                "More than one Sprint 37 vrock carries the hour marker.");
            return marked.Length == 1 ? marked[0] : null;
        }

        /// <summary>
        /// The state this creature actually persists, and the reason this row
        /// exists: the printed screech is "once per hour", and the charter
        /// required that to be a persisted native timer rather than an
        /// in-memory cooldown. A marker that silently reset on load would give
        /// a player an unlimited save-scummed control effect.
        ///
        /// <para>Four pieces of saved state are prepared on the summoned
        /// vrock and its victim exactly as production attaches them: the
        /// hour-long screech marker, the three-round spore cadence marker, the
        /// ten-round vines on an adjacent unit, and both single-use resources
        /// spent.</para>
        /// </summary>
        private IEnumerable<int> PrepareVrockSaved37(UnitEntityData caster,
            UnitEntityData[] sources)
        {
            UnitEntityData[] vrocks = VrockSavedSources37(sources);
            if (vrocks.Length != 2) throw new InvalidOperationException(
                "The Sprint 37 persistence scope requires its two summoned vrocks.");
            UnitEntityData vrock = vrocks[0];
            var recharge = VrockSavedBlueprint37<BlueprintBuff>(
                "ScreechRecharge");
            var cooldown = VrockSavedBlueprint37<BlueprintBuff>(
                "SporeCooldown");
            var vines = VrockSavedBlueprint37<BlueprintBuff>("SporeVines");
            var heroism = VrockSavedBlueprint37<BlueprintAbilityResource>(
                "HeroismResource");
            var mirror = VrockSavedBlueprint37<BlueprintAbilityResource>(
                "MirrorImageResource");
            if (vrock.Buffs.GetBuff(recharge) != null ||
                vrock.Buffs.GetBuff(cooldown) != null)
                throw new InvalidOperationException(
                    "The vrock already carries its own markers before preparation.");

            // A victim for the growth. The printed ability exposes adjacent
            // creatures and not the vrock itself, so the second owned vrock is
            // the victim; no party member and no unit this fixture does not
            // own is touched.
            UnitEntityData victim = vrocks
                .Where(unit => !ReferenceEquals(unit, vrock) &&
                    unit.IsInState && !unit.Descriptor.State.IsDead)
                .OrderBy(unit => Vector3Distance(vrock, unit))
                .FirstOrDefault();
            if (victim == null) throw new InvalidOperationException(
                "The Sprint 37 persistence scope found no victim for the growth.");

            // Attached with the owner as source and the production durations,
            // which is the same shape VrockRechargeOnCast37 uses for an
            // internal clock.
            if (vrock.Descriptor.AddBuff(recharge, vrock,
                    TimeSpan.FromHours(
                        VrockRules37Policy.ScreechRechargeHours)) == null ||
                vrock.Descriptor.AddBuff(cooldown, vrock,
                    TimeSpan.FromSeconds(GameConsts.RoundDuration *
                        VrockRules37Policy.SporeCadenceRounds)) == null ||
                victim.Descriptor.AddBuff(vines, vrock,
                    TimeSpan.FromSeconds(GameConsts.RoundDuration *
                        VrockRules37Policy.SporeVineRounds)) == null)
                throw new InvalidOperationException(
                    "Sprint 37 saved state could not be attached.");
            vrock.Descriptor.Resources.Spend(heroism, 1);
            vrock.Descriptor.Resources.Spend(mirror, 1);
            yield break;
        }

        private static float Vector3Distance(UnitEntityData left,
            UnitEntityData right)
        {
            return UnityEngine.Vector3.Distance(left.Position, right.Position);
        }

        /// <summary>
        /// What survived the save. The markers are the point: a remaining time
        /// inside its printed bound proves the clock is serialized rather than
        /// restarted, and a spent single use proves the resource is too.
        /// </summary>
        private void ObserveVrockSaved37(UnitEntityData caster,
            UnitEntityData[] sources, string phase)
        {
            var recharge = VrockSavedBlueprint37<BlueprintBuff>(
                "ScreechRecharge");
            UnitEntityData[] vrocks = VrockSavedSources37(sources);
            if (vrocks.Length != 2) throw new InvalidOperationException(
                "Exact saved Sprint 37 vrock pair absent.");
            UnitEntityData vrock = VrockSavedOwner37(vrocks, recharge);
            if (vrock == null) throw new InvalidOperationException(
                "No saved Sprint 37 vrock carries the hour marker.");
            UnitEntityData bystander = vrocks
                .Single(unit => !ReferenceEquals(unit, vrock));
            var cooldown = VrockSavedBlueprint37<BlueprintBuff>(
                "SporeCooldown");
            var vines = VrockSavedBlueprint37<BlueprintBuff>("SporeVines");
            var heroism = VrockSavedBlueprint37<BlueprintAbilityResource>(
                "HeroismResource");
            var mirror = VrockSavedBlueprint37<BlueprintAbilityResource>(
                "MirrorImageResource");
            Buff marker = vrock.Buffs.GetBuff(recharge);
            Buff cadence = vrock.Buffs.GetBuff(cooldown);
            // A save does not serialize the live caster reference, so after a
            // cold load MaybeCaster is null and the attribution has to be read
            // from the serialized reference - the lesson Sprint 36's held
            // nausea recorded. Either answer identifies the owner; neither is
            // allowed to name a different unit.
            Buff[] growth = Game.Instance.State.Units.All
                .Where(unit => unit != null && unit.IsInState)
                .SelectMany(unit => unit.Buffs.Enumerable)
                .Where(buff => ReferenceEquals(buff.Blueprint, vines) &&
                    (ReferenceEquals(buff.Context?.MaybeCaster, vrock) ||
                        (buff.Context != null &&
                            CircleOriginalCasterId(buff.Context) ==
                                vrock.UniqueId)))
                .ToArray();
            double markerLeft = marker == null ? -1 :
                marker.TimeLeft.TotalSeconds;
            double cadenceLeft = cadence == null ? -1 :
                cadence.TimeLeft.TotalSeconds;
            double growthLeft = growth.Length == 1 ? -1 :
                growth.Length == 0 ? -1 : 0;
            if (growth.Length == 1) growthLeft =
                growth[0].TimeLeft.TotalSeconds;
            int heroismHeld = vrock.Descriptor.Resources
                .GetResourceAmount(heroism);
            int mirrorHeld = vrock.Descriptor.Resources
                .GetResourceAmount(mirror);
            // The second vrock stood beside the first through the same save.
            // Its clean marker state and unspent uses are what make the
            // owner's state its own rather than the creature's in general.
            bool bystanderClean = bystander.Buffs.GetBuff(recharge) == null &&
                bystander.Buffs.GetBuff(cooldown) == null &&
                bystander.Descriptor.Resources.GetResourceAmount(heroism) == 1 &&
                bystander.Descriptor.Resources.GetResourceAmount(mirror) == 1;
            double hour = 3600d;
            double cadenceBound = GameConsts.RoundDuration *
                VrockRules37Policy.SporeCadenceRounds;
            double growthBound = GameConsts.RoundDuration *
                VrockRules37Policy.SporeVineRounds;
            bool exact =
                // The hour marker survived with a real remaining time. Not
                // "present": present-but-restarted would also be a defect, so
                // the bound is the printed hour and nothing longer.
                marker != null && marker.Active && !marker.IsDisposed &&
                markerLeft > 0 && markerLeft <= hour + 1 &&
                cadence != null && cadenceLeft > 0 &&
                cadenceLeft <= cadenceBound + 1 &&
                growth.Length == 1 && growthLeft > 0 &&
                growthLeft <= growthBound + 1 &&
                ReferenceEquals(growth[0].Owner?.Unit, bystander) &&
                heroismHeld == 0 && mirrorHeld == 0 && bystanderClean;
            RosterPersistenceCheck("s37-state-" + phase, exact,
                new JObject {
                    ["vrock"] = vrock.UniqueId,
                    ["moduleEnabled"] =
                        _context.FeatureModules.Active.ExpandedSummoning,
                    ["screechMarkerSecondsLeft"] = markerLeft,
                    ["screechMarkerBound"] = hour,
                    ["sporeCadenceSecondsLeft"] = cadenceLeft,
                    ["sporeCadenceBound"] = cadenceBound,
                    ["growthCount"] = growth.Length,
                    ["growthSecondsLeft"] = growthLeft,
                    ["growthBound"] = growthBound,
                    ["growthOwner"] = growth.Length == 1 ?
                        growth[0].Owner?.Unit?.UniqueId : null,
                    ["growthSerializedCaster"] = growth.Length == 1 ?
                        growth[0].Context?.MaybeCaster?.UniqueId : null,
                    ["heroismRemaining"] = heroismHeld,
                    ["mirrorRemaining"] = mirrorHeld,
                    ["bystander"] = bystander.UniqueId,
                    ["bystanderMarker"] =
                        bystander.Buffs.GetBuff(recharge) != null,
                    ["bystanderCadence"] =
                        bystander.Buffs.GetBuff(cooldown) != null,
                    ["bystanderHeroism"] = bystander.Descriptor.Resources
                        .GetResourceAmount(heroism),
                    ["bystanderMirror"] = bystander.Descriptor.Resources
                        .GetResourceAmount(mirror),
                    ["bystanderClean"] = bystanderClean
                }, phase == "prepared"
                    ? "The prepared fixture owns the vrock's hour screech marker, its three-round spore cadence, one ten-round growth attributed to it, and both single uses spent."
                    : "A fresh load preserves the serialized hour marker, the spore cadence, the growth and its caster attribution, and both spent single uses: the once-per-hour screech is a saved clock rather than one that restarts on load.");
            if (!exact) throw new InvalidOperationException(
                "Sprint 37 saved marker, growth or resource state differs.");
        }

        /// <summary>
        /// Nothing of this creature's state may remain on a fresh or cleaned
        /// save, including on units it never touched.
        /// </summary>
        private void VrockSavedAbsence37(UnitEntityData caster, string phase)
        {
            var recharge = VrockSavedBlueprint37<BlueprintBuff>(
                "ScreechRecharge");
            var cooldown = VrockSavedBlueprint37<BlueprintBuff>(
                "SporeCooldown");
            var vines = VrockSavedBlueprint37<BlueprintBuff>("SporeVines");
            var stunned = VrockSavedBlueprint37<BlueprintBuff>("Stunned");
            BlueprintBuff[] all = { recharge, cooldown, vines, stunned };
            int effects = Game.Instance.State.Units.All
                .Where(unit => unit != null)
                .Sum(unit => unit.Buffs.Enumerable.Count(buff =>
                    all.Any(value => ReferenceEquals(buff.Blueprint, value))));
            int vrocks = Game.Instance.State.Units.All
                .Count(unit => unit != null && !unit.Destroyed &&
                    unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                        VrockVisualPolicy37.Vrock);
            bool exact = effects == 0 && vrocks == 0;
            RosterPersistenceCheck("s37-state-" + phase + "-absence", exact,
                new JObject {
                    ["vrockEffects"] = effects,
                    ["vrocks"] = vrocks,
                    ["observedBlueprints"] = new JArray(all
                        .Select(value => value.name))
                }, "No Sprint 37 marker, growth, stun or summoned vrock remains after the cleanup save.");
            if (!exact) throw new InvalidOperationException(
                "Sprint 37 saved state remains.");
        }

        /// <summary>
        /// What each saved vrock owns. The first version of this row asserted
        /// an empty inventory, reasoning that a creature with five natural
        /// attacks carries no equipment - but in Kingmaker a natural attack is
        /// itself a weapon item in the unit's own inventory, so both vrocks
        /// legitimately held five and a correct creature failed. The released
        /// Sprint 36 shape is the right one and is used here: the inventory is
        /// exactly the blueprint's own body weapons, each owned by that unit,
        /// non-removable, non-lootable and single, no instance is shared
        /// between the two, and the party's inventory is untouched.
        /// </summary>
        private void ObserveVrockItems37(UnitEntityData[] sources,
            Kingmaker.Items.ItemEntity[] partyItems, int[] partyCounts)
        {
            UnitEntityData[] vrocks = VrockSavedSources37(sources);
            if (vrocks.Length != 2) throw new InvalidOperationException(
                "Exact saved Sprint 37 vrock pair absent for the item observation.");
            Kingmaker.Items.ItemEntity[] all = vrocks.SelectMany(unit =>
                unit.Descriptor.Inventory.Items).ToArray();
            bool exact = all.Length != 0 &&
                all.Distinct().Count() == all.Length &&
                Sprint28PartyInventoryExact(partyItems, partyCounts);
            var rows = new JArray();
            foreach (UnitEntityData vrock in vrocks)
            {
                var body = vrock.Blueprint.Body;
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
                Kingmaker.Items.ItemEntity[] items = vrock.Descriptor
                    .Inventory.Items.ToArray();
                bool row = items.Select(item => item.Blueprint.AssetGuid)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .SequenceEqual(expected) &&
                    items.All(item =>
                        ReferenceEquals(item.Owner, vrock.Descriptor) &&
                        item.IsNonRemovable && !item.IsLootable &&
                        item.Count == 1) &&
                    // Five project natural attacks - a bite, two claws and
                    // two talons - none of them a donor's. The core phase
                    // counts the same five on the live unit.
                    items.Length == 5 &&
                    items.All(item => item.Blueprint.name.StartsWith("KMG_",
                        StringComparison.Ordinal)) &&
                    vrock.Body.AdditionalLimbs
                        .Count(slot => slot.MaybeWeapon != null) == 4;
                exact &= row;
                rows.Add(new JObject {
                    ["vrock"] = vrock.UniqueId,
                    ["inventoryItems"] = items.Length,
                    ["inventoryBlueprints"] = new JArray(items
                        .Select(item => item.Blueprint.name)),
                    ["expectedBodyWeapons"] = expected.Length,
                    ["primaryWeapon"] = vrock.Body.PrimaryHand.MaybeWeapon
                        ?.Blueprint?.name,
                    ["additionalArmedLimbs"] = vrock.Body.AdditionalLimbs
                        .Count(slot => slot.MaybeWeapon != null),
                    ["allOwnedNonRemovableSingle"] = items.All(item =>
                        ReferenceEquals(item.Owner, vrock.Descriptor) &&
                        item.IsNonRemovable && !item.IsLootable &&
                        item.Count == 1),
                    ["exact"] = row
                });
            }
            RosterPersistenceCheck("s37-items-owned", exact, new JObject {
                ["vrocks"] = rows,
                ["distinctInstances"] = all.Distinct().Count(),
                ["totalInstances"] = all.Length,
                ["partyInventoryExact"] =
                    Sprint28PartyInventoryExact(partyItems, partyCounts)
            }, "Each saved vrock owns exactly its own five natural attacks as non-removable single items, shares no instance with the other, and the party's inventory is untouched.");
            if (!exact) throw new InvalidOperationException(
                "Sprint 37 saved item or limb state differs.");
        }
    }
}
