using System;
using System.Collections.Generic;
using System.Linq;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.ElementalRaces
{
    public sealed class UnitPartElementalFeatTransientState : UnitPart
    {
        [JsonProperty]
        private int _schemaVersion =
            ElementalFeatTransientPolicy.CurrentSchemaVersion;
        [JsonProperty]
        private long _elementalStrikeEndTimeTicks;
        [JsonProperty]
        private long _scorchingWeaponsEndTimeTicks;
        [JsonProperty]
        private int _scorchingWeaponCount;
        [JsonProperty]
        private ItemEntityWeapon _scorchingPrimary;
        [JsonProperty]
        private ItemEntityWeapon _scorchingSecondary;

        internal long ElementalStrikeEndTimeTicks
        {
            get { return _elementalStrikeEndTimeTicks; }
        }

        internal long ScorchingWeaponsEndTimeTicks
        {
            get { return _scorchingWeaponsEndTimeTicks; }
        }

        internal int ScorchingWeaponCount
        {
            get { return _scorchingWeaponCount; }
        }

        internal void BeginElementalStrike(TimeSpan endTime)
        {
            _schemaVersion = ElementalFeatTransientPolicy
                .CurrentSchemaVersion;
            _elementalStrikeEndTimeTicks = ValidEndTimeTicks(endTime);
        }

        internal void EndElementalStrike(long expectedEndTimeTicks)
        {
            if (expectedEndTimeTicks > 0L &&
                expectedEndTimeTicks != _elementalStrikeEndTimeTicks)
                return;
            _elementalStrikeEndTimeTicks = 0L;
        }

        internal void BeginScorchingWeapons(TimeSpan endTime,
            IEnumerable<ItemEntityWeapon> weapons)
        {
            ItemEntityWeapon[] exact = (weapons ??
                    Enumerable.Empty<ItemEntityWeapon>())
                .Where(value => value != null).Distinct().Take(3).ToArray();
            if (exact.Length > 2)
                throw new InvalidOperationException(
                    "Scorching Weapons cannot persist more than two items.");
            _schemaVersion = ElementalFeatTransientPolicy
                .CurrentSchemaVersion;
            _scorchingWeaponsEndTimeTicks = ValidEndTimeTicks(endTime);
            _scorchingWeaponCount = exact.Length;
            _scorchingPrimary = exact.Length > 0 ? exact[0] : null;
            _scorchingSecondary = exact.Length > 1 ? exact[1] : null;
        }

        internal void EndScorchingWeapons(long expectedEndTimeTicks)
        {
            if (expectedEndTimeTicks > 0L &&
                expectedEndTimeTicks != _scorchingWeaponsEndTimeTicks)
                return;
            ClearScorchingWeapons();
        }

        internal ItemEntityWeapon[] ScorchingWeapons()
        {
            EnsureValid();
            if (_scorchingWeaponCount == 0)
                return new ItemEntityWeapon[0];
            if (_scorchingWeaponCount == 1)
                return new[] { _scorchingPrimary };
            return new[] { _scorchingPrimary, _scorchingSecondary };
        }

        public override void PreSave()
        {
            EnsureValid();
            base.PreSave();
        }

        public override void PostLoad()
        {
            base.PostLoad();
            EnsureValid();
        }

        private static long ValidEndTimeTicks(TimeSpan endTime)
        {
            if (endTime <= TimeSpan.Zero || endTime == TimeSpan.MaxValue)
                throw new ArgumentOutOfRangeException("endTime");
            return endTime.Ticks;
        }

        private void EnsureValid()
        {
            if (_schemaVersion != ElementalFeatTransientPolicy
                    .CurrentSchemaVersion)
            {
                _schemaVersion = ElementalFeatTransientPolicy
                    .CurrentSchemaVersion;
                _elementalStrikeEndTimeTicks = 0L;
                ClearScorchingWeapons();
                return;
            }
            if (_elementalStrikeEndTimeTicks < 0L)
                _elementalStrikeEndTimeTicks = 0L;
            if (_scorchingWeaponsEndTimeTicks <= 0L ||
                _scorchingWeaponCount < 0 || _scorchingWeaponCount > 2 ||
                _scorchingWeaponCount == 2 && _scorchingPrimary != null &&
                    ReferenceEquals(_scorchingPrimary, _scorchingSecondary))
                ClearScorchingWeapons();
            else if (_scorchingWeaponCount == 0)
            {
                _scorchingPrimary = null;
                _scorchingSecondary = null;
            }
            else if (_scorchingWeaponCount == 1)
                _scorchingSecondary = null;
        }

        private void ClearScorchingWeapons()
        {
            _scorchingWeaponsEndTimeTicks = 0L;
            _scorchingWeaponCount = 0;
            _scorchingPrimary = null;
            _scorchingSecondary = null;
        }
    }

    [Serializable]
    public sealed class ElementalFeatTransientFeatureController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public bool ScorchingWeapons;

        public override void OnTurnOn()
        {
            ElementalFeatTransientRuntime.ReconcileFeatureActivation(Owner,
                ScorchingWeapons);
        }

        // Unit facts are turned off as part of Kingmaker's ordinary save and
        // load reconstruction. Treating this ambiguous callback as permanent
        // feat removal erased the authoritative UnitPart before serialization.
        // The exact transient buff owns expiry/death cleanup; every mechanical
        // use also rechecks the live feat and race prerequisite and is inert as
        // soon as either prerequisite is absent.
        public override void OnTurnOff() { }
    }

    [Serializable]
    public sealed class ElementalStrikeTransientBuffController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            ElementalFeatTransientRuntime.BeginElementalStrike(Owner,
                Fact as Buff);
        }

        public override void OnTurnOff()
        {
            ElementalFeatTransientRuntime.EndElementalStrike(Owner,
                Fact as Buff);
        }
    }

    [Serializable]
    public sealed class ElementalScorchingTransientBuffController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn() { }

        public override void OnTurnOff()
        {
            ElementalFeatTransientRuntime.EndScorchingWeapons(Owner,
                Fact as Buff);
        }
    }

    internal static class ElementalFeatTransientRuntime
    {
        private static readonly HashSet<UnitDescriptor> Reconciling =
            new HashSet<UnitDescriptor>();
        private static BlueprintFeature _elementalStrikeFeat;
        private static BlueprintFeature _scorchingWeaponsFeat;
        private static BlueprintAbility _elementalStrikeAbility;
        private static BlueprintAbility _scorchingWeaponsAbility;
        private static BlueprintBuff _elementalStrikeBuff;
        private static BlueprintBuff _scorchingWeaponsBuff;
        private static BlueprintWeaponEnchantment _scorchingEnchantment;
        private static BlueprintRace _ifrit;
        private static BlueprintRace _oread;
        private static BlueprintRace _sylph;
        private static BlueprintRace _undine;

        internal static void Configure(BlueprintFeature elementalStrikeFeat,
            BlueprintFeature scorchingWeaponsFeat,
            BlueprintAbility elementalStrikeAbility,
            BlueprintAbility scorchingWeaponsAbility,
            BlueprintBuff elementalStrikeBuff,
            BlueprintBuff scorchingWeaponsBuff,
            BlueprintWeaponEnchantment scorchingEnchantment,
            ElementalRaceBlueprintSet races)
        {
            if (elementalStrikeFeat == null || scorchingWeaponsFeat == null ||
                elementalStrikeAbility == null || scorchingWeaponsAbility ==
                    null || elementalStrikeBuff == null ||
                scorchingWeaponsBuff == null || scorchingEnchantment == null ||
                races == null)
                throw new ArgumentNullException();
            _elementalStrikeFeat = elementalStrikeFeat;
            _scorchingWeaponsFeat = scorchingWeaponsFeat;
            _elementalStrikeAbility = elementalStrikeAbility;
            _scorchingWeaponsAbility = scorchingWeaponsAbility;
            _elementalStrikeBuff = elementalStrikeBuff;
            _scorchingWeaponsBuff = scorchingWeaponsBuff;
            _scorchingEnchantment = scorchingEnchantment;
            _ifrit = races.Ifrit.Race;
            _oread = races.Oread.Race;
            _sylph = races.Sylph.Race;
            _undine = races.Undine.Race;
        }

        internal static void BeginElementalStrike(UnitDescriptor owner,
            Buff buff)
        {
            if (owner == null || buff == null || _elementalStrikeBuff == null ||
                !ReferenceEquals(buff.Blueprint, _elementalStrikeBuff) ||
                !HasElementalStrikePrerequisite(owner)) return;
            owner.Ensure<UnitPartElementalFeatTransientState>()
                .BeginElementalStrike(buff.EndTime);
        }

        internal static void EndElementalStrike(UnitDescriptor owner,
            Buff buff)
        {
            if (owner == null) return;
            UnitPartElementalFeatTransientState state = owner.Get<
                UnitPartElementalFeatTransientState>();
            if (state == null) return;
            long expected = buff == null ? 0L : buff.EndTime.Ticks;
            if (ElementalFeatTransientPolicy.PreserveDuringBuffTeardown(
                    owner.State != null && owner.State.IsDead,
                    state.ElementalStrikeEndTimeTicks, NowTicks()))
                return;
            state.EndElementalStrike(expected);
        }

        internal static void BeginScorchingWeapons(UnitDescriptor owner,
            Buff marker, IEnumerable<ItemEntityWeapon> weapons)
        {
            if (owner == null || marker == null ||
                _scorchingWeaponsBuff == null ||
                !ReferenceEquals(marker.Blueprint, _scorchingWeaponsBuff) ||
                !HasScorchingPrerequisite(owner)) return;
            owner.Ensure<UnitPartElementalFeatTransientState>()
                .BeginScorchingWeapons(marker.EndTime, weapons);
        }

        internal static void EndScorchingWeapons(UnitDescriptor owner,
            Buff marker)
        {
            if (owner == null) return;
            UnitPartElementalFeatTransientState state = owner.Get<
                UnitPartElementalFeatTransientState>();
            if (state == null) return;
            long expected = marker == null ? 0L : marker.EndTime.Ticks;
            if (expected > 0L && expected !=
                    state.ScorchingWeaponsEndTimeTicks)
                return;
            if (ElementalFeatTransientPolicy.PreserveDuringBuffTeardown(
                    owner.State != null && owner.State.IsDead,
                    state.ScorchingWeaponsEndTimeTicks, NowTicks()))
                return;
            ItemEntityWeapon[] weapons = state.ScorchingWeapons();
            state.EndScorchingWeapons(expected);
            foreach (ItemEntityWeapon weapon in weapons.Where(value =>
                value != null).Distinct())
                RemoveEnchantments(weapon);
        }

        internal static bool IsElementalStrikeActive(UnitDescriptor owner)
        {
            UnitPartElementalFeatTransientState state = owner == null ? null :
                owner.Get<UnitPartElementalFeatTransientState>();
            return state != null && HasElementalStrikePrerequisite(owner) &&
                ElementalFeatTransientPolicy.Remaining(
                    state.ElementalStrikeEndTimeTicks, NowTicks()) >
                    TimeSpan.Zero;
        }

        internal static bool IsScorchingWeaponsActive(UnitDescriptor owner,
            ItemEntityWeapon weapon)
        {
            UnitPartElementalFeatTransientState state = owner == null ? null :
                owner.Get<UnitPartElementalFeatTransientState>();
            return state != null && weapon != null &&
                HasScorchingPrerequisite(owner) &&
                ElementalFeatTransientPolicy.Remaining(
                    state.ScorchingWeaponsEndTimeTicks, NowTicks()) >
                    TimeSpan.Zero && state.ScorchingWeapons().Any(value =>
                        ReferenceEquals(value, weapon));
        }

        internal static bool ReconcileAfterUnitLoad(UnitDescriptor owner)
        {
            return Reconcile(owner, null, true, false);
        }

        internal static bool ReconcileFeatureActivation(UnitDescriptor owner,
            bool scorchingWeapons)
        {
            // This component exists only on the exact project feat. Kingmaker
            // invokes OnTurnOn before owner.HasFact reports the activating
            // fact, so the callback itself is the authoritative feat identity.
            return Reconcile(owner, scorchingWeapons, false, true);
        }

        private static bool Reconcile(UnitDescriptor owner,
            bool? scorchingWeaponsOnly,
            bool preserveFutureStateUntilFactsActivate,
            bool activatingFeatureIsExact)
        {
            if (owner == null || !Configured() || !Reconciling.Add(owner))
                return false;
            try
            {
                UnitPartElementalFeatTransientState state = owner.Get<
                    UnitPartElementalFeatTransientState>();
                if (state == null) return false;
                long now = NowTicks();
                if (!scorchingWeaponsOnly.HasValue ||
                    !scorchingWeaponsOnly.Value)
                    ReconcileElementalStrike(owner, state, now,
                        preserveFutureStateUntilFactsActivate,
                        activatingFeatureIsExact);
                if (!scorchingWeaponsOnly.HasValue ||
                    scorchingWeaponsOnly.Value)
                    ReconcileScorchingWeapons(owner, state, now,
                        preserveFutureStateUntilFactsActivate,
                        activatingFeatureIsExact);
                return true;
            }
            catch (Exception exception)
            {
                Fault("reconcile", owner, exception);
                return false;
            }
            finally { Reconciling.Remove(owner); }
        }

        internal static void RemoveElementalStrike(UnitDescriptor owner)
        {
            if (owner == null) return;
            UnitPartElementalFeatTransientState state = owner.Get<
                UnitPartElementalFeatTransientState>();
            if (state != null) state.EndElementalStrike(0L);
            RemoveBuff(owner, _elementalStrikeBuff);
        }

        internal static void RemoveScorchingWeapons(UnitDescriptor owner)
        {
            if (owner == null) return;
            UnitPartElementalFeatTransientState state = owner.Get<
                UnitPartElementalFeatTransientState>();
            ItemEntityWeapon[] weapons = state == null ?
                new ItemEntityWeapon[0] : state.ScorchingWeapons();
            if (state != null) state.EndScorchingWeapons(0L);
            foreach (ItemEntityWeapon weapon in weapons.Where(value =>
                value != null).Distinct())
                RemoveEnchantments(weapon);
            RemoveBuff(owner, _scorchingWeaponsBuff);
        }

        private static void ReconcileElementalStrike(UnitDescriptor owner,
            UnitPartElementalFeatTransientState state, long now,
            bool preserveFutureStateUntilFactsActivate,
            bool activatingFeatureIsExact)
        {
            bool prerequisite = activatingFeatureIsExact
                ? HasElementalStrikeRace(owner)
                : HasElementalStrikePrerequisite(owner);
            if (preserveFutureStateUntilFactsActivate && !prerequisite &&
                ElementalFeatTransientPolicy.Remaining(
                    state.ElementalStrikeEndTimeTicks, now) > TimeSpan.Zero)
                return;
            ElementalFeatTransientRestoreDecision decision =
                ElementalFeatTransientPolicy.Decide(
                    prerequisite,
                    state.ElementalStrikeEndTimeTicks, now, 0, 0);
            if (decision == ElementalFeatTransientRestoreDecision.Clear)
            {
                RemoveElementalStrike(owner);
                return;
            }
            TimeSpan remaining = ElementalFeatTransientPolicy.Remaining(
                state.ElementalStrikeEndTimeTicks, now);
            EnsureBuff(owner, _elementalStrikeAbility, _elementalStrikeBuff,
                state.ElementalStrikeEndTimeTicks, remaining);
        }

        private static void ReconcileScorchingWeapons(UnitDescriptor owner,
            UnitPartElementalFeatTransientState state, long now,
            bool preserveFutureStateUntilFactsActivate,
            bool activatingFeatureIsExact)
        {
            bool prerequisite = activatingFeatureIsExact
                ? HasScorchingRace(owner)
                : HasScorchingPrerequisite(owner);
            if (preserveFutureStateUntilFactsActivate && !prerequisite &&
                ElementalFeatTransientPolicy.Remaining(
                    state.ScorchingWeaponsEndTimeTicks, now) > TimeSpan.Zero)
                return;
            ItemEntityWeapon[] weapons = state.ScorchingWeapons();
            int resolved = weapons.Count(value => value != null);
            ElementalFeatTransientRestoreDecision decision =
                ElementalFeatTransientPolicy.Decide(
                    prerequisite,
                    state.ScorchingWeaponsEndTimeTicks, now,
                    state.ScorchingWeaponCount, resolved);
            if (decision == ElementalFeatTransientRestoreDecision.Clear)
            {
                RemoveScorchingWeapons(owner);
                return;
            }
            if (decision == ElementalFeatTransientRestoreDecision
                    .WaitForOwnedItems)
                return;

            TimeSpan remaining = ElementalFeatTransientPolicy.Remaining(
                state.ScorchingWeaponsEndTimeTicks, now);
            var additions = new List<ItemEnchantment>();
            Buff markerBefore = owner.Buffs.GetBuff(_scorchingWeaponsBuff);
            try
            {
                EnsureBuff(owner, _scorchingWeaponsAbility,
                    _scorchingWeaponsBuff,
                    state.ScorchingWeaponsEndTimeTicks, remaining);
                var context = new MechanicsContext(owner.Unit, owner,
                    _scorchingWeaponsAbility, null,
                    new TargetWrapper(owner.Unit));
                foreach (ItemEntityWeapon weapon in weapons)
                {
                    ItemEnchantment[] existing = ExactEnchantments(weapon);
                    for (int index = 1; index < existing.Length; index++)
                        weapon.RemoveEnchantment(existing[index]);
                    ItemEnchantment effect = existing.FirstOrDefault();
                    if (effect == null)
                    {
                        effect = weapon.AddEnchantment(
                            _scorchingEnchantment, context, new Rounds(1));
                        if (effect == null)
                            throw new InvalidOperationException(
                                "Kingmaker rejected a persisted Scorching Weapons enchantment.");
                        additions.Add(effect);
                    }
                    effect.RemoveOnUnequipItem = false;
                    effect.EndTime = TimeSpan.FromTicks(
                        state.ScorchingWeaponsEndTimeTicks);
                }
            }
            catch
            {
                for (int index = additions.Count - 1; index >= 0; index--)
                {
                    ItemEnchantment effect = additions[index];
                    if (effect != null && effect.Owner != null)
                        effect.Owner.RemoveEnchantment(effect);
                }
                if (markerBefore == null)
                    RemoveBuff(owner, _scorchingWeaponsBuff);
                throw;
            }
        }

        private static Buff EnsureBuff(UnitDescriptor owner,
            BlueprintAbility source, BlueprintBuff blueprint,
            long endTimeTicks, TimeSpan remaining)
        {
            Buff[] existing = owner.Buffs.Enumerable.Where(value =>
                value != null && ReferenceEquals(value.Blueprint,
                    blueprint)).ToArray();
            for (int index = 1; index < existing.Length; index++)
                owner.Buffs.RemoveFact(existing[index]);
            Buff result = existing.FirstOrDefault();
            if (result == null)
            {
                var context = new MechanicsContext(owner.Unit, owner, source,
                    null, new TargetWrapper(owner.Unit));
                result = owner.Buffs.AddBuff(blueprint, context, remaining);
                if (result == null)
                    throw new InvalidOperationException(
                        "Kingmaker rejected an elemental feat transient buff.");
            }
            result.EndTime = TimeSpan.FromTicks(endTimeTicks);
            return result;
        }

        private static ItemEnchantment[] ExactEnchantments(
            ItemEntityWeapon weapon)
        {
            if (weapon == null || _scorchingEnchantment == null)
                return new ItemEnchantment[0];
            return weapon.Enchantments.Where(value => value != null &&
                !value.IsEnded && ReferenceEquals(value.Blueprint,
                    _scorchingEnchantment)).ToArray();
        }

        private static void RemoveEnchantments(ItemEntityWeapon weapon)
        {
            foreach (ItemEnchantment effect in ExactEnchantments(weapon))
                weapon.RemoveEnchantment(effect);
        }

        private static void RemoveBuff(UnitDescriptor owner,
            BlueprintBuff blueprint)
        {
            if (owner == null || blueprint == null) return;
            foreach (Buff buff in owner.Buffs.Enumerable.Where(value =>
                value != null && ReferenceEquals(value.Blueprint, blueprint))
                .ToArray())
                owner.Buffs.RemoveFact(buff);
        }

        private static bool HasElementalStrikePrerequisite(
            UnitDescriptor owner)
        {
            return owner != null && _elementalStrikeFeat != null &&
                owner.HasFact(_elementalStrikeFeat) &&
                HasElementalStrikeRace(owner);
        }

        private static bool HasScorchingPrerequisite(UnitDescriptor owner)
        {
            return owner != null && _scorchingWeaponsFeat != null &&
                owner.HasFact(_scorchingWeaponsFeat) && HasScorchingRace(owner);
        }

        private static bool HasElementalStrikeRace(UnitDescriptor owner)
        {
            BlueprintRace race = owner == null || owner.Progression == null ?
                null : owner.Progression.Race;
            return ReferenceEquals(race, _ifrit) ||
                ReferenceEquals(race, _oread) ||
                ReferenceEquals(race, _sylph) ||
                ReferenceEquals(race, _undine);
        }

        private static bool HasScorchingRace(UnitDescriptor owner)
        {
            return owner != null && owner.Progression != null &&
                ReferenceEquals(owner.Progression.Race, _ifrit);
        }

        private static long NowTicks()
        {
            Game game = Game.Instance;
            return game == null || game.TimeController == null ? 0L :
                game.TimeController.GameTime.Ticks;
        }

        private static bool Configured()
        {
            return _elementalStrikeFeat != null &&
                _scorchingWeaponsFeat != null &&
                _elementalStrikeAbility != null &&
                _scorchingWeaponsAbility != null &&
                _elementalStrikeBuff != null &&
                _scorchingWeaponsBuff != null &&
                _scorchingEnchantment != null && _ifrit != null &&
                _oread != null && _sylph != null && _undine != null;
        }

        private static void Fault(string operation, UnitDescriptor owner,
            Exception exception)
        {
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            context.Logger.Failure("elemental-races",
                "feat-transient." + operation + ".failed",
                "unit=" + (owner == null || owner.Unit == null ? "<none>" :
                    owner.Unit.UniqueId), exception);
        }
    }

    /// <summary>
    /// Kingmaker reconstructs UnitParts and feature components before it has
    /// finished replacing the native BuffCollection. Rehydrating from either
    /// earlier callback creates an effect that the remainder of PostLoad then
    /// removes. This exact native end-of-unit-load seam runs only for units
    /// carrying the project-owned transient state.
    /// </summary>
    [HarmonyPatch(typeof(UnitEntityData), "PostLoad", new Type[0])]
    internal static class ElementalFeatTransientPostLoadPatch
    {
        private static void Postfix(UnitEntityData __instance)
        {
            if (__instance == null || __instance.Descriptor == null ||
                __instance.Descriptor.Get<
                    UnitPartElementalFeatTransientState>() == null)
                return;
            ElementalFeatTransientRuntime.ReconcileAfterUnitLoad(
                __instance.Descriptor);
        }
    }
}
