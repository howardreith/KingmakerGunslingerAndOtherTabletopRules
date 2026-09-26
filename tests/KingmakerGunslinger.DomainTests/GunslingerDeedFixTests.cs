using System;
using System.IO;
using KingmakerGunslinger.Classes;
using KingmakerGunslinger.Deeds;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Pre-existing Gunslinger defects fixed by the tabletop rules: D1 (the
    /// level-17 Gun Training pick), D2 (Dead Shot critical confirmation) and a
    /// True Grit Gunslinger's Dodge usable at 0 grit.
    /// </summary>
    internal static class GunslingerDeedFixTests
    {
        private static string Source(params string[] parts)
        {
            string path = Environment.CurrentDirectory;
            foreach (string part in new[] { "src", "KingmakerGunslinger" })
                path = Path.Combine(path, part);
            foreach (string part in parts)
                path = Path.Combine(path, part);
            return File.ReadAllText(path).Replace("\r\n", "\n");
        }

        // D1: the native completion gate treats an obligatory selection as
        // always selectable, so an obligatory Gun Training pick with every
        // official type trained (a base Gunslinger's 17th) blocked the level
        // forever. A non-obligatory pick is still required while a type is
        // untrained (FeatureSelectionState.CanSelectAnything).
        internal static void GunTrainingEmptyPickCompletesTheLevel()
        {
            string source = Source("Blueprints", "GunTrainingBlueprints.cs");
            Assertions.True(source.Contains("selection.Obligatory = false;") &&
                !source.Contains("selection.Obligatory = true;"),
                "The Gun Training selection must not be obligatory.");
            Assertions.Equal(3, OfficialFirearmSupport.Kinds.Length,
                "Gun Training offers exactly the three official types.");
            Assertions.Equal(4, GunTrainingProgression.Levels.Length,
                "Four Gun Training picks against three official types.");
        }

        // D2: a probe threatens when it hits with a natural roll at or above
        // the weapon's critical edge; a misfire never threatens.
        internal static void DeadShotThreatRule()
        {
            Assertions.True(DeadShotConfirmationPolicy.IsThreat(true, false, 20, 20),
                "A natural 20 hit at edge 20 must threaten.");
            Assertions.True(!DeadShotConfirmationPolicy.IsThreat(true, false, 19, 20),
                "A 19 must not threaten at edge 20.");
            Assertions.True(DeadShotConfirmationPolicy.IsThreat(true, false, 19, 19),
                "A 19 must threaten at edge 19 (Improved Critical).");
            Assertions.True(!DeadShotConfirmationPolicy.IsThreat(false, false, 20, 20),
                "A miss never threatens.");
            Assertions.True(!DeadShotConfirmationPolicy.IsThreat(false, true, 1, 1),
                "A misfire never threatens.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                DeadShotConfirmationPolicy.IsThreat(true, false, 21, 20), "A roll above 20 was accepted.");
        }

        // D2: one confirmation, natural roll + attack bonus + every
        // confirmation bonus (the Dead Shot penalty included) against the
        // target's critical AC.
        internal static void DeadShotConfirmationRule()
        {
            Assertions.True(DeadShotConfirmationPolicy.Confirms(10, 8, -5, 13),
                "10 + 8 - 5 = 13 must confirm against 13.");
            Assertions.True(!DeadShotConfirmationPolicy.Confirms(10, 8, -5, 14),
                "10 + 8 - 5 = 13 must not confirm against 14.");
            // Critical Focus +4 and a -4 penalty (two threats) add, never replace.
            Assertions.True(DeadShotConfirmationPolicy.Confirms(6, 8, 4 - 4, 14),
                "6 + 8 + 0 must confirm against 14.");
            var record = new DeadShotConfirmationRecord(12, 8, 0, -4, 20);
            Assertions.True(record.Confirmed && record.Blocked == null && record.Penalty == -4,
                "The record must hold the confirmation it made.");
            var blocked = new DeadShotConfirmationRecord("target immune to critical hits");
            Assertions.True(!blocked.Confirmed && blocked.Blocked != null,
                "A blocked confirmation never confirms.");
            Assertions.Throws<ArgumentOutOfRangeException>(() =>
                DeadShotConfirmationPolicy.Confirms(0, 8, 0, 10), "A roll of 0 was accepted.");
        }

        // D2 wiring: probes stay free of native confirmations; the delivery
        // confirms once with the native rules, adding the penalty.
        internal static void DeadShotConfirmationWiring()
        {
            string source = Source("Deeds", "DeadShotRuntime.cs");
            Assertions.True(source.Contains("probe.ImmuneToCriticalHit = true;") &&
                source.Contains("DeadShotConfirmationPolicy.IsThreat(attackRoll.IsHit,") &&
                source.Contains("attackRoll.WeaponStats.CriticalEdge"),
                "Probes report a threat from the natural roll and the critical edge.");
            Assertions.True(source.Contains("attackRoll.CriticalConfirmationBonus += marker.ConfirmationPenalty;") &&
                !source.Contains("attackRoll.CriticalConfirmationBonus = marker.ConfirmationPenalty;"),
                "The Dead Shot penalty is added to every other confirmation bonus.");
            Assertions.True(source.Contains("new RuleCalculateAttackBonus(attackRoll.Initiator,") &&
                source.Contains("{ IsCritical = true }).TargetAC") &&
                source.Contains("if (attackRoll.ImmuneToCriticalHit)") &&
                source.Contains("Difficulty.CritsOnParty") &&
                source.Contains("attackRoll.AutoCriticalConfirmation = confirmation.Confirmed;"),
                "The delivery confirms once with the native rules, immunity and party setting.");
            Assertions.True(source.Contains("\"confirmation failed: \" + exception.GetType().Name"),
                "A failed confirmation is contained (the prefix never throws with the AC frame pushed).");

            // The critical AC must take the firearm touch-AC rule, whose frame
            // FirearmArmorClassRuntime.BeforeAttackRoll pushes in the same prefix.
            string prefix = Source("Diagnostics", "CombatTracePatches.cs");
            int configure = prefix.IndexOf("DeadShotRuntime.ConfigureDelivery(", StringComparison.Ordinal);
            int frame = prefix.IndexOf("FirearmArmorClassRuntime.BeforeAttackRoll(__instance);", StringComparison.Ordinal);
            int confirm = prefix.IndexOf("DeadShotRuntime.ConfirmDelivery(", StringComparison.Ordinal);
            Assertions.True(configure >= 0 && frame > configure && confirm > frame &&
                prefix.IndexOf("DeadShotRuntime.ConfirmDelivery(", confirm + 1, StringComparison.Ordinal) < 0,
                "The confirmation runs once, after the firearm AC frame is pushed.");
        }

        // The native resource check compares a cost with the pool only, so a
        // True Grit cost reduced to 0 must still fail it at 0 grit (the Dodge
        // cost calculator reports NativeCheckCost).
        internal static void TrueGritNativeCheckCost()
        {
            Assertions.Equal(0, TrueGrit(1, 1, true).NativeCheckCost,
                "An available reduced cost is spent as 0.");
            Assertions.Equal(1, TrueGrit(0, 1, true).NativeCheckCost,
                "A reduced cost at 0 grit must fail the native check (report 1).");
            Assertions.Equal(1, TrueGrit(1, 1, false).NativeCheckCost,
                "An unselected one-cost deed costs 1.");
            Assertions.Equal(1, TrueGrit(0, 1, false).NativeCheckCost,
                "An unselected one-cost deed at 0 grit reports 1 (refused).");
            string source = Source("Deeds", "GunslingerDodgeProneAbilityLogic.cs");
            Assertions.True(source.Contains(".NativeCheckCost;") && !source.Contains(".EffectiveCost;"),
                "The Dodge cost calculator reports the native check cost.");
        }

        private static TrueGritDecision TrueGrit(int current, int ordinary, bool selected)
        {
            return new TrueGritService().Evaluate(new TrueGritRequest(current, ordinary, selected, false));
        }
    }
}
