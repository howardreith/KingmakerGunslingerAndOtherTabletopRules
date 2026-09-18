using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Pure symbol/classification rules for the guarded native racial action
    /// fixture. The exact 39 catalog consumers are fixed here so focused tests
    /// can corrupt them independently of any Kingmaker runtime object.
    /// </summary>
    internal static class NativeRacialActionIconRules
    {
        internal const string RequestCaseParameter = "nativeActionCase";
        internal const string RequestCaseValue = "racial-actions";
        internal const string ControlGuid = "09d742e8b50b0214fb71acfc99cc00b3";
        internal const string ControlName = "FightDefensivelyToggleAbility";
        private const string Prefix = "KMG.ElementalRaces.";

        // The exact 39 action consumers from the canonical icon catalog:
        // 25 group parents, 11 parent-owned variants, 2 held-touch deliveries
        // and 1 activatable. Held-touch deliveries are parent-owned variant
        // rows that the catalog records separately; the classification is
        // derived from the runtime blueprint graph, not from this list.
        private static readonly string[][] RaceSymbols =
        {
            new[]
            {
                "Feats.BlazingAura.Ability", "Feats.ElementalStrike.Ability",
                "Feats.HydraulicManeuver.Ability", "Feats.HydraulicManeuver.BullRushAbility",
                "Feats.HydraulicManeuver.DirtyTrickBlindAbility", "Feats.HydraulicManeuver.DisarmAbility",
                "Feats.HydraulicManeuver.TripAbility", "Feats.ScorchingWeapons.Ability",
                "Feats.TritonPortal.Ability", "Ifrit.BurningHandsAbility",
                "Ifrit.Lavasoul.FirebellyAbility", "Ifrit.Sunsoul.FlareBurstAbility",
                "Traits.Ifrit.EfreetiMagic.Ability", "Traits.Ifrit.EfreetiMagic.EnlargePerson",
                "Traits.Ifrit.EfreetiMagic.ReducePerson"
            },
            new[]
            {
                "Oread.Gemsoul.ColorSprayAbility", "Oread.Ironsoul.UnerringWeaponAbility",
                "Oread.Ironsoul.UnerringWeaponPrimaryAbility", "Oread.Ironsoul.UnerringWeaponSecondaryAbility",
                "Oread.StoneFistAbility", "Traits.Oread.CrystallineForm.Mode",
                "Traits.Oread.TreacherousEarth.Ability"
            },
            new[]
            {
                "Sylph.FeatherStepAbility", "Sylph.Smokesoul.ExpeditiousRetreatAbility",
                "Sylph.Stormsoul.ShockingGraspAbility", "Sylph.Stormsoul.ShockingGraspDeliveryAbility",
                "Traits.Sylph.BreezeKissed.BullRush", "Traits.Sylph.BreezeKissed.CalmWinds",
                "Traits.Sylph.BreezeKissed.Gust", "Traits.Sylph.BreezeKissed.RenewWinds",
                "Traits.Sylph.BreezeKissed.Trip"
            },
            new[]
            {
                "Traits.Undine.AcidBreath.Ability", "Traits.Undine.NereidFascination.Ability",
                "Traits.Undine.NereidFascination.ShakeFreeAbility", "Traits.Undine.OozeBreath.Ability",
                "Undine.HydraulicPushAbility", "Undine.Mistsoul.BlurAbility",
                "Undine.Rimesoul.ChillTouchAbility", "Undine.Rimesoul.ChillTouchDeliveryAbility"
            }
        };

        private static readonly string[] Races = { "Ifrit", "Oread", "Sylph", "Undine" };

        internal static string[] SymbolsForRace(string race)
        {
            int index = Array.IndexOf(Races, race);
            return index < 0 ? new string[0] :
                RaceSymbols[index].Select(value => Prefix + value).ToArray();
        }

        internal static string[] AllSymbols() => RaceSymbols
            .SelectMany(value => value.Select(symbol => Prefix + symbol)).ToArray();

        /// <summary>The single activatable consumer; every other symbol is a
        /// BlueprintAbility regardless of painted or native-reuse art.</summary>
        internal static bool IsActivatableSymbol(string symbol) =>
            symbol == Prefix + "Traits.Oread.CrystallineForm.Mode";

        /// <summary>Rejects a proposed per-race set unless it is an exact
        /// ordered member of the fixed 39-symbol plan.</summary>
        internal static bool IsValidRaceSet(string race, IEnumerable<string> symbols)
        {
            int index = Array.IndexOf(Races, race);
            return index >= 0 && symbols != null &&
                symbols.SequenceEqual(RaceSymbols[index].Select(value => Prefix + value));
        }

        internal enum NativeActionRowKind { Parent, Variant, Activatable }

        /// <summary>Variant rows belong to a parent blueprint and are rendered
        /// from detached native AbilityData, never inserted as unit facts.</summary>
        internal static NativeActionRowKind Classify(bool isActivatable, bool hasParent) =>
            isActivatable ? NativeActionRowKind.Activatable :
            hasParent ? NativeActionRowKind.Variant : NativeActionRowKind.Parent;

        internal enum ModifierCachePlan { OwnAndRemoveIfStillEmpty, PreserveExisting, Reject }

        /// <summary>The request owns only a native modifier part that did not
        /// exist before menu rendering. An existing empty part is preserved;
        /// any populated part means the fixture must fail closed.</summary>
        internal static ModifierCachePlan PlanModifierCache(bool partExisted, int entryCount) =>
            entryCount > 0 ? ModifierCachePlan.Reject :
            partExisted ? ModifierCachePlan.PreserveExisting : ModifierCachePlan.OwnAndRemoveIfStillEmpty;

        internal enum ModifierCacheCleanupDecision { RemoveOwnedEmptyPart, NoRemovalNeeded, FailPopulated, FailReplacedPart }

        /// <summary>Cleanup decision for a cache that was absent before the
        /// fixture ran (the owned plan). A part that never materialized needs
        /// no removal; a materialized empty part may be removed only when it
        /// is the exact instance first observed under this request's exclusive
        /// paused observation. A populated or unexpectedly replaced part fails
        /// without any destructive cleanup.</summary>
        internal static ModifierCacheCleanupDecision DecideOwnedModifierCacheCleanup(
            bool partPresentAfter, bool partAfterIsFirstObservedInstance, int entriesAfter)
        {
            if (!partPresentAfter) return ModifierCacheCleanupDecision.NoRemovalNeeded;
            if (entriesAfter > 0) return ModifierCacheCleanupDecision.FailPopulated;
            return partAfterIsFirstObservedInstance ?
                ModifierCacheCleanupDecision.RemoveOwnedEmptyPart :
                ModifierCacheCleanupDecision.FailReplacedPart;
        }

        /// <summary>An existing accepted (preserved) cache must retain the
        /// same instance and the same ordered entry identities, not merely a
        /// reference. Entry keys pair each free-action ability GUID with its
        /// source fact reference so both are compared.</summary>
        internal static bool ExistingCachePreserved(bool sameReference,
            IEnumerable<string> entriesBefore, IEnumerable<string> entriesAfter) =>
            sameReference && entriesBefore.SequenceEqual(entriesAfter);

        /// <summary>Evaluates the final action-fixture evidence with the same
        /// logic the runtime assertion uses. The pinned control observation is
        /// required independently of the 39 consumers so it can neither
        /// inflate nor replace their coverage.</summary>
        internal static bool EvaluateNativeRacialActionEvidence(Newtonsoft.Json.Linq.JObject evidence,
            string race, IList<string> failures)
        {
            if (failures == null) throw new ArgumentNullException("failures");
            if (evidence == null) { failures.Add("evidence-missing"); return false; }
            bool pass = true;
            var expected = evidence["expectedGuids"] as Newtonsoft.Json.Linq.JArray;
            var captured = evidence["capturedGuids"] as Newtonsoft.Json.Linq.JArray;
            string[] symbols = SymbolsForRace(race);
            if (expected == null || expected.Count != symbols.Length)
            { failures.Add("expected-consumer-count"); pass = false; }
            if (expected == null || captured == null ||
                !Newtonsoft.Json.Linq.JToken.DeepEquals(expected, captured))
            { failures.Add("captured-sequence"); pass = false; }
            if ((bool?)evidence["restored"] != true)
            { failures.Add("restoration"); pass = false; }
            var control = evidence["controlRow"] as Newtonsoft.Json.Linq.JObject;
            if (control == null)
            { failures.Add("control-observation-missing"); pass = false; }
            else
            {
                if ((string)control["guid"] != ControlGuid || (string)control["name"] != ControlName)
                { failures.Add("control-identity"); pass = false; }
                if ((bool?)control["captured"] != true) { failures.Add("control-capture-record"); pass = false; }
                if ((bool?)control["spriteExact"] != true) { failures.Add("control-rendered-sprite"); pass = false; }
                if ((bool?)control["rowActive"] != true) { failures.Add("control-row-inactive"); pass = false; }
                if ((bool?)control["statePreserved"] != true) { failures.Add("control-initial-state"); pass = false; }
            }
            return pass;
        }
    }
}
