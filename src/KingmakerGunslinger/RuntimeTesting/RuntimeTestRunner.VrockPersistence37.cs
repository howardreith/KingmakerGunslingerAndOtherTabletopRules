using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
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

        private static UnitEntityData VrockSavedSource37(
            UnitEntityData[] sources)
        {
            return sources.SingleOrDefault(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    VrockVisualPolicy37.Vrock);
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
            UnitEntityData vrock = VrockSavedSource37(sources);
            if (vrock == null) throw new InvalidOperationException(
                "The Sprint 37 persistence scope found no summoned vrock.");
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

            // A victim for the growth. Any adjacent live unit is exposed by
            // the printed ability, so another roster source serves and no
            // party member is touched.
            UnitEntityData victim = sources
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
            UnitEntityData vrock = VrockSavedSource37(sources);
            if (vrock == null) throw new InvalidOperationException(
                "Exact saved Sprint 37 vrock absent.");
            var recharge = VrockSavedBlueprint37<BlueprintBuff>(
                "ScreechRecharge");
            var cooldown = VrockSavedBlueprint37<BlueprintBuff>(
                "SporeCooldown");
            var vines = VrockSavedBlueprint37<BlueprintBuff>("SporeVines");
            var heroism = VrockSavedBlueprint37<BlueprintAbilityResource>(
                "HeroismResource");
            var mirror = VrockSavedBlueprint37<BlueprintAbilityResource>(
                "MirrorImageResource");
            Buff marker = vrock.Buffs.GetBuff(recharge);
            Buff cadence = vrock.Buffs.GetBuff(cooldown);
            Buff[] growth = Game.Instance.State.Units.All
                .Where(unit => unit != null && unit.IsInState)
                .SelectMany(unit => unit.Buffs.Enumerable)
                .Where(buff => ReferenceEquals(buff.Blueprint, vines) &&
                    ReferenceEquals(buff.Context?.MaybeCaster, vrock))
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
                heroismHeld == 0 && mirrorHeld == 0;
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
                    ["mirrorRemaining"] = mirrorHeld
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
        /// This creature fights with five natural attacks and carries no
        /// equipment, so the owned-item observation is that it owns none - the
        /// released Babau's longspear made that worth asserting rather than
        /// assuming.
        /// </summary>
        private void ObserveVrockItems37(UnitEntityData[] sources)
        {
            UnitEntityData vrock = VrockSavedSource37(sources);
            if (vrock == null) throw new InvalidOperationException(
                "Exact saved Sprint 37 vrock absent for the item observation.");
            int inventory = vrock.Descriptor.Inventory == null ? 0 :
                vrock.Descriptor.Inventory.Items.Count();
            string primary = vrock.Body.PrimaryHand.MaybeWeapon?.Blueprint
                ?.name;
            int limbs = vrock.Body.AdditionalLimbs
                .Count(slot => slot.MaybeWeapon != null);
            bool exact = inventory == 0 && limbs == 4 &&
                primary != null && primary.StartsWith("KMG_",
                    StringComparison.Ordinal);
            RosterPersistenceCheck("s37-items-owned", exact, new JObject {
                ["vrock"] = vrock.UniqueId,
                ["inventoryItems"] = inventory,
                ["primaryWeapon"] = primary,
                ["additionalArmedLimbs"] = limbs
            }, "The saved vrock carries no inventory item and keeps its five project natural attacks.");
            if (!exact) throw new InvalidOperationException(
                "Sprint 37 saved item or limb state differs.");
        }
    }
}
