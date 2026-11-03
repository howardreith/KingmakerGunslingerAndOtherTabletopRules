using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
        /// Whether this persistence run carries the Sprint 38 scope.
        /// </summary>
        private bool Sprint38PersistenceScope
        {
            get
            {
                return _request != null && _request.Parameters != null &&
                    (string)_request.Parameters["persistenceScope"] ==
                        ExpandedSummoningRosterPersistencePolicy.GlabrezuScope;
            }
        }

        private static T GlabrezuSavedBlueprint38<T>(string suffix)
            where T : BlueprintScriptableObject
        {
            return (T)ExpandedSummoningBlueprints.Registered.BySymbol[
                ExpandedSummoningSpecialBuilder.Glabrezu38Prefix + suffix];
        }

        private static BlueprintBuff GlabrezuTrueSeeing38()
        {
            return BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                BlueprintBootstrap.Library,
                ExpandedSummoningSpecialBuilder.NativeTrueSeeingBuff38,
                "native constant true seeing effect");
        }

        private static UnitEntityData[] GlabrezuSavedSources38(
            UnitEntityData[] sources)
        {
            return sources.Where(unit =>
                unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                    GlabrezuVisualPolicy38.Glabrezu)
                // A stable order so the same unit is the owner in every stage
                // of an envelope, independent of enumeration.
                .OrderBy(unit => unit.UniqueId, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>
        /// The owner of the prepared state, read rather than assumed. Before
        /// preparation it is the first of the two in a stable order;
        /// afterwards it is the one whose single Power Word Stun use is spent,
        /// which is the fact under observation.
        /// </summary>
        private static UnitEntityData GlabrezuSavedOwner38(
            UnitEntityData[] glabrezus, BlueprintAbilityResource stun)
        {
            UnitEntityData[] spent = glabrezus
                .Where(unit => unit.Descriptor.Resources
                    .GetResourceAmount(stun) == 0)
                .ToArray();
            if (spent.Length > 1) throw new InvalidOperationException(
                "More than one Sprint 38 glabrezu has spent its single use.");
            return spent.Length == 1 ? spent[0] : null;
        }

        /// <summary>
        /// The maximum stun the printed low bucket can produce, in rounds. The
        /// fixture attaches the bound rather than a roll, so the observation
        /// has a fixed ceiling to measure the loaded remainder against; the
        /// roll itself is the live mechanics phase's subject, not the save's.
        /// </summary>
        private static int GlabrezuStunBoundRounds38()
        {
            return GlabrezuRules38Policy.PowerWordStunLowDice *
                GlabrezuRules38Policy.PowerWordStunDieSides;
        }

        /// <summary>
        /// The state this creature actually persists, and why these rows
        /// exist. Power Word Stun is once per summoning, so a use that came
        /// back on load would be an unlimited save-scummed no-save stun at no
        /// cost at all.
        ///
        /// <para>The stun itself is applied, observed and then <b>withdrawn
        /// before the save</b>, which is a correction rather than a
        /// convenience. A guarded run showed why: the shared roster readiness
        /// contract requires every owned unit to be able to act and move at
        /// the save boundary, and this fixture may write only to units it
        /// owns, so the only available recipient is a second owned glabrezu -
        /// which a real stun then makes legitimately unable to act. Leaving it
        /// stunned would have meant either a failing mandatory row or widening
        /// a released contract every other scope depends on. What the stun has
        /// to prove is proven while it stands: one finite effect, inside the
        /// printed bucket's own dice, attributed to its owner, genuinely
        /// preventing action. Its serialized duration is ordinary native buff
        /// behaviour that the released Sprint 36 and 37 envelopes already
        /// exercise.</para>
        ///
        /// <para>What crosses the save is therefore the state that is this
        /// creature's own: the spent single use, the recipient's hit points so
        /// the printed bucket is re-derived from live state rather than
        /// remembered, and the printed constant true seeing. The rend's round
        /// state is deliberately not prepared either - it lives in a
        /// request-local table keyed by unit and rebuilt from the round
        /// boundary - so what the envelope shows is that no armed rend and no
        /// attack-pair history survive a load.</para>
        /// </summary>
        private IEnumerable<int> PrepareGlabrezuSaved38(UnitEntityData caster,
            UnitEntityData[] sources)
        {
            UnitEntityData[] glabrezus = GlabrezuSavedSources38(sources);
            if (glabrezus.Length != 2) throw new InvalidOperationException(
                "The Sprint 38 persistence scope requires its two summoned glabrezus.");
            UnitEntityData glabrezu = glabrezus[0];
            var carrier = GlabrezuSavedBlueprint38<BlueprintBuff>(
                "PowerWordStunCarrier");
            var stun = GlabrezuSavedBlueprint38<BlueprintAbilityResource>(
                "PowerWordStunResource");
            if (glabrezu.Descriptor.Resources.GetResourceAmount(stun) !=
                    GlabrezuRules38Policy.PowerWordStunUses)
                throw new InvalidOperationException(
                    "The glabrezu does not hold its single printed use before preparation.");

            // The printed word lands on another creature and never on itself,
            // so the second owned glabrezu is the recipient; no party member
            // and no unit this fixture does not own is touched.
            UnitEntityData victim = glabrezus
                .Where(unit => !ReferenceEquals(unit, glabrezu) &&
                    unit.IsInState && !unit.Descriptor.State.IsDead)
                .OrderBy(unit => GlabrezuDistance38(glabrezu, unit))
                .FirstOrDefault();
            if (victim == null) throw new InvalidOperationException(
                "The Sprint 38 persistence scope found no recipient for the stun.");
            if (victim.Buffs.GetBuff(carrier) != null)
                throw new InvalidOperationException(
                    "The recipient already carries a stun before preparation.");

            // A full-strength glabrezu has far more than the printed ceiling
            // of 150 hit points, and the printed word does nothing at all
            // above it. Rather than attach an effect the rules would refuse,
            // the fixture brings the owned recipient into the printed LOW
            // bucket first - a mutation to a unit this fixture created - and
            // then attaches what that bucket actually produces. The hit points
            // are left where the load can read them, so the bucket is
            // re-derived after the save instead of remembered.
            int target = GlabrezuRules38Policy.PowerWordStunLowThreshold;
            if (victim.MaxHP <= target) throw new InvalidOperationException(
                "The Sprint 38 recipient cannot be brought into the printed low bucket.");
            victim.Descriptor.Damage = victim.MaxHP - target;
            if (victim.HPLeft != target ||
                GlabrezuRules38Policy.PowerWordStunDiceCount(victim.HPLeft) !=
                    GlabrezuRules38Policy.PowerWordStunLowDice)
                throw new InvalidOperationException(
                    "The Sprint 38 recipient is not in the printed low bucket.");

            Kingmaker.UnitLogic.Buffs.Buff applied =
                victim.Descriptor.AddBuff(carrier, glabrezu,
                    TimeSpan.FromSeconds(GameConsts.RoundDuration *
                        GlabrezuStunBoundRounds38()));
            if (applied == null)
                throw new InvalidOperationException(
                    "Sprint 38 saved state could not be attached.");
            glabrezu.Descriptor.Resources.Spend(stun, 1);
            double stunRounds = applied.TimeLeft.TotalSeconds /
                GameConsts.RoundDuration;
            bool stunStood = applied.Active && !applied.IsDisposed &&
                stunRounds > 0 &&
                stunRounds <= GlabrezuStunBoundRounds38() + 1 &&
                ReferenceEquals(applied.Owner?.Unit, victim) &&
                !victim.Descriptor.State.CanAct;
            // Observed while it stands, then withdrawn: the save boundary
            // requires every owned unit to be able to act, and the only
            // recipient this fixture may write to is a unit it owns.
            applied.Remove();
            bool removed = victim.Buffs.GetBuff(carrier) == null &&
                victim.Descriptor.State.CanAct &&
                victim.Descriptor.State.CanMove;
            RosterPersistenceCheck("s38-stun-stood-then-withdrawn",
                stunStood && removed,
                new JObject {
                    ["glabrezu"] = glabrezu.UniqueId,
                    ["recipient"] = victim.UniqueId,
                    ["recipientHitPoints"] = victim.HPLeft,
                    ["stunRoundsWhileStanding"] = stunRounds,
                    ["stunBoundRounds"] = GlabrezuStunBoundRounds38(),
                    ["stunOwnerWhileStanding"] = applied.Owner?.Unit?.UniqueId,
                    ["stunPreventedActionWhileStanding"] = stunStood,
                    ["stunWithdrawnBeforeSave"] = removed,
                    ["recipientCanActAfterWithdrawal"] =
                        victim.Descriptor.State.CanAct,
                    ["recipientCanMoveAfterWithdrawal"] =
                        victim.Descriptor.State.CanMove,
                    ["whyItIsWithdrawn"] =
                        "The shared roster readiness contract requires every owned unit to be able to act and move at the save boundary, and this fixture may write only to units it owns, so a real stun on the only available recipient would have meant either a failing mandatory row or widening a released contract every other scope depends on. The stun is proven while it stands; its serialized duration is ordinary native buff behaviour the released Sprint 36 and 37 envelopes already exercise."
                }, "Power Word Stun leaves one finite effect on the recipient inside the printed bucket dice, attributed to its owner and genuinely preventing action, and it is then withdrawn so no owned unit reaches the save unable to act.");
            if (!(stunStood && removed)) throw new InvalidOperationException(
                "Sprint 38 stun did not stand and withdraw cleanly.");
            yield break;
        }

        private static float GlabrezuDistance38(UnitEntityData left,
            UnitEntityData right)
        {
            return UnityEngine.Vector3.Distance(left.Position, right.Position);
        }

        /// <summary>
        /// Whether a unit carries no live rend round state.
        ///
        /// <para>"No entry at all" would be the wrong assertion: the gate's
        /// own round boundary creates an empty entry for every live glabrezu
        /// once a round passes, so an absent entry is not what a correct load
        /// looks like a few seconds later. What must never survive is the
        /// CONTENT - an armed rend, a spent round, or any remembered attack
        /// pair - because that is the state a serialized tracker would bring
        /// back and hand a player a second rend out of one round.</para>
        /// </summary>
        private static bool GlabrezuRendRoundClean38(UnitEntityData unit,
            out JObject row)
        {
            GlabrezuRendRound38 round =
                GlabrezuRendGate38.ObservedState(unit);
            row = new JObject {
                ["unit"] = unit?.UniqueId,
                ["roundStatePresent"] = round != null,
                ["armed"] = round != null && round.Armed,
                ["spentThisRound"] = round != null && round.SpentThisRound,
                ["trackedVictims"] = round == null ? 0 :
                    round.TrackedVictimCount,
                ["roundStamp"] = round == null ? (long?)null :
                    round.RoundStamp
            };
            return round == null || (!round.Armed && !round.SpentThisRound &&
                round.TrackedVictimCount == 0);
        }

        /// <summary>
        /// What survived the save. A remaining stun inside the printed
        /// bucket's bound proves the clock is serialized rather than
        /// restarted; a spent single use proves the resource is; the constant
        /// true seeing proves a printed permanent sense is reinstated; and an
        /// empty rend round proves that state is request-local, as intended.
        /// </summary>
        private void ObserveGlabrezuSaved38(UnitEntityData caster,
            UnitEntityData[] sources, string phase)
        {
            var carrier = GlabrezuSavedBlueprint38<BlueprintBuff>(
                "PowerWordStunCarrier");
            var stun = GlabrezuSavedBlueprint38<BlueprintAbilityResource>(
                "PowerWordStunResource");
            BlueprintBuff seeing = GlabrezuTrueSeeing38();
            UnitEntityData[] glabrezus = GlabrezuSavedSources38(sources);
            if (glabrezus.Length != 2) throw new InvalidOperationException(
                "Exact saved Sprint 38 glabrezu pair absent.");
            UnitEntityData glabrezu = GlabrezuSavedOwner38(glabrezus, stun);
            if (glabrezu == null) throw new InvalidOperationException(
                "No saved Sprint 38 glabrezu has spent its single use.");
            UnitEntityData victim = glabrezus
                .Single(unit => !ReferenceEquals(unit, glabrezu));

            // No stun may remain anywhere: the prepare stage withdrew it
            // before the save on purpose, so a survivor here would mean the
            // withdrawal did not take or something re-applied it.
            Buff[] stuns = Game.Instance.State.Units.All
                .Where(unit => unit != null && unit.IsInState)
                .SelectMany(unit => unit.Buffs.Enumerable)
                .Where(buff => ReferenceEquals(buff.Blueprint, carrier))
                .ToArray();
            int ownerHeld = glabrezu.Descriptor.Resources
                .GetResourceAmount(stun);
            int victimHeld = victim.Descriptor.Resources
                .GetResourceAmount(stun);
            int victimHitPoints = victim.HPLeft;
            int derivedDice = GlabrezuRules38Policy
                .PowerWordStunDiceCount(victimHitPoints);
            bool seeingBoth = glabrezus.All(unit =>
                unit.Buffs.GetBuff(seeing) != null);
            JObject ownerRound, recipientRound;
            // Both rows are built every time: && would short-circuit the
            // second call and leave the failing unit's state unreported, which
            // is the half of the evidence a reader needs most.
            bool ownerRendClean = GlabrezuRendRoundClean38(glabrezu,
                out ownerRound);
            bool recipientRendClean = GlabrezuRendRoundClean38(victim,
                out recipientRound);
            bool rendClean = ownerRendClean && recipientRendClean;
            // The owner carries no stun of its own. With the recipient's own
            // single use still unspent beside it, the spent one is
            // demonstrably this owner's rather than the creature's in general.
            bool ownerClean = glabrezu.Buffs.GetBuff(carrier) == null;
            bool exact = stuns.Length == 0 && ownerHeld == 0 &&
                victimHeld == GlabrezuRules38Policy.PowerWordStunUses &&
                victimHitPoints ==
                    GlabrezuRules38Policy.PowerWordStunLowThreshold &&
                derivedDice == GlabrezuRules38Policy.PowerWordStunLowDice &&
                victim.Descriptor.State.CanAct &&
                victim.Descriptor.State.CanMove &&
                seeingBoth && rendClean && ownerClean;
            RosterPersistenceCheck("s38-state-" + phase, exact,
                new JObject {
                    ["glabrezu"] = glabrezu.UniqueId,
                    ["moduleEnabled"] =
                        _context.FeatureModules.Active.ExpandedSummoning,
                    ["remainingStuns"] = stuns.Length,
                    ["stunWithdrawnBeforeSave"] = true,
                    ["recipientCanAct"] = victim.Descriptor.State.CanAct,
                    ["recipientCanMove"] = victim.Descriptor.State.CanMove,
                    ["powerWordStunRemaining"] = ownerHeld,
                    ["ownerCarriesNoStun"] = ownerClean,
                    ["recipient"] = victim.UniqueId,
                    ["recipientHitPoints"] = victimHitPoints,
                    ["recipientMaxHitPoints"] = victim.MaxHP,
                    ["recipientDerivedStunDice"] = derivedDice,
                    ["recipientPowerWordStunRemaining"] = victimHeld,
                    ["trueSeeingOnBoth"] = seeingBoth,
                    ["rendRound"] = new JArray(ownerRound, recipientRound)
                }, phase == "prepared"
                    ? "The prepared fixture has its single Power Word Stun use spent, a recipient really inside the printed low hit-point bucket with its own use untouched and able to act once the stun was withdrawn, constant true seeing on both, and no rend round state."
                    : "A fresh load preserves the spent single use and the recipient hit points - so the printed bucket is re-derived from live state rather than remembered - reinstates the printed constant true seeing, leaves no stun behind, and carries no armed rend or remembered attack pair, because that state is request-local and must not be saved.");
            if (!exact) throw new InvalidOperationException(
                "Sprint 38 saved resource, bucket, sense, stun-absence or rend-round state differs.");
        }

        /// <summary>
        /// Nothing of this creature's state may remain on a fresh or cleaned
        /// save, including on units it never touched.
        /// </summary>
        private void GlabrezuSavedAbsence38(UnitEntityData caster, string phase)
        {
            var carrier = GlabrezuSavedBlueprint38<BlueprintBuff>(
                "PowerWordStunCarrier");
            BlueprintBuff[] all = { carrier };
            int effects = Game.Instance.State.Units.All
                .Where(unit => unit != null)
                .Sum(unit => unit.Buffs.Enumerable.Count(buff =>
                    all.Any(value => ReferenceEquals(buff.Blueprint, value))));
            int glabrezus = Game.Instance.State.Units.All
                .Count(unit => unit != null && !unit.Destroyed &&
                    unit.Get<UnitPartRelease143RosterReceipt>()?.Key ==
                        GlabrezuVisualPolicy38.Glabrezu);
            // The printed constant sense is the creature's own fact, so its
            // absence here is not a separate claim: with no glabrezu present
            // there is nothing to carry it. What this row does assert is that
            // no stun this creature applied outlives it anywhere, which is
            // also the other half of the prepare stage's withdrawal.
            bool exact = effects == 0 && glabrezus == 0;
            RosterPersistenceCheck("s38-state-" + phase + "-absence", exact,
                new JObject {
                    ["glabrezuEffects"] = effects,
                    ["glabrezus"] = glabrezus,
                    ["observedBlueprints"] = new JArray(all
                        .Select(value => value.name))
                }, "No Sprint 38 stun or summoned glabrezu remains after the cleanup save.");
            if (!exact) throw new InvalidOperationException(
                "Sprint 38 saved state remains.");
        }

        /// <summary>
        /// What each saved glabrezu owns. The released Sprint 36/37 shape is
        /// reused unchanged - in Kingmaker a natural attack is itself a weapon
        /// item in the unit's own inventory, which is the mistake Sprint 37
        /// recorded - with this creature's own count: five printed attacks,
        /// two pincers, two claws and a bite, four of them on additional limbs.
        /// </summary>
        private void ObserveGlabrezuItems38(UnitEntityData[] sources,
            Kingmaker.Items.ItemEntity[] partyItems, int[] partyCounts)
        {
            UnitEntityData[] glabrezus = GlabrezuSavedSources38(sources);
            if (glabrezus.Length != 2) throw new InvalidOperationException(
                "Exact saved Sprint 38 glabrezu pair absent for the item observation.");
            Kingmaker.Items.ItemEntity[] all = glabrezus.SelectMany(unit =>
                unit.Descriptor.Inventory.Items).ToArray();
            bool exact = all.Length != 0 &&
                all.Distinct().Count() == all.Length &&
                Sprint28PartyInventoryExact(partyItems, partyCounts);
            var rows = new JArray();
            foreach (UnitEntityData glabrezu in glabrezus)
            {
                var body = glabrezu.Blueprint.Body;
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
                Kingmaker.Items.ItemEntity[] items = glabrezu.Descriptor
                    .Inventory.Items.ToArray();
                bool row = items.Select(item => item.Blueprint.AssetGuid)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .SequenceEqual(expected) &&
                    items.All(item =>
                        ReferenceEquals(item.Owner, glabrezu.Descriptor) &&
                        item.IsNonRemovable && !item.IsLootable &&
                        item.Count == 1) &&
                    // Five project natural attacks - two pincers, two claws
                    // and a bite - none of them a donor's. The core phase
                    // counts the same five on the live unit.
                    items.Length == 5 &&
                    items.All(item => item.Blueprint.name.StartsWith("KMG_",
                        StringComparison.Ordinal)) &&
                    glabrezu.Body.AdditionalLimbs
                        .Count(slot => slot.MaybeWeapon != null) == 4;
                exact &= row;
                rows.Add(new JObject {
                    ["glabrezu"] = glabrezu.UniqueId,
                    ["inventoryItems"] = items.Length,
                    ["inventoryBlueprints"] = new JArray(items
                        .Select(item => item.Blueprint.name)),
                    ["expectedBodyWeapons"] = expected.Length,
                    ["primaryWeapon"] = glabrezu.Body.PrimaryHand.MaybeWeapon
                        ?.Blueprint?.name,
                    ["additionalArmedLimbs"] = glabrezu.Body.AdditionalLimbs
                        .Count(slot => slot.MaybeWeapon != null),
                    ["allOwnedNonRemovableSingle"] = items.All(item =>
                        ReferenceEquals(item.Owner, glabrezu.Descriptor) &&
                        item.IsNonRemovable && !item.IsLootable &&
                        item.Count == 1),
                    ["exact"] = row
                });
            }
            RosterPersistenceCheck("s38-items-owned", exact, new JObject {
                ["glabrezus"] = rows,
                ["distinctInstances"] = all.Distinct().Count(),
                ["totalInstances"] = all.Length,
                ["partyInventoryExact"] =
                    Sprint28PartyInventoryExact(partyItems, partyCounts)
            }, "Each saved glabrezu owns exactly its own five natural attacks as non-removable single items, shares no instance with the other, and the party's inventory is untouched.");
            if (!exact) throw new InvalidOperationException(
                "Sprint 38 saved item or limb state differs.");
        }
    }
}
