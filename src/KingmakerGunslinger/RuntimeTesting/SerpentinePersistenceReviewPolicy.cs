using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.RuntimeTesting
{
    // A scene-visible state is not yet a completed native load. Correlate the
    // actual callback return to the exact state/area; never infer readiness
    // from elapsed frames or from an already-clean relationship.
    internal sealed class SerpentineLoadBoundaryReadiness
    {
        private object _state, _area;
        private int _scenesFrame = -1;
        internal int CompletedFrame { get; private set; } = -1;

        internal void ScenesLoaded(object state, object area, int frame)
        {
            _state = state; _area = area; _scenesFrame = frame;
            CompletedFrame = -1;
        }

        internal void Completed(object state, object area, int frame)
        {
            CompletedFrame = state != null && area != null && _scenesFrame >= 0 &&
                frame >= _scenesFrame && ReferenceEquals(state, _state) &&
                ReferenceEquals(area, _area) ? frame : -1;
        }

        internal bool Ready(object state, object area)
        {
            return CompletedFrame >= 0 && state != null && area != null &&
                ReferenceEquals(state, _state) && ReferenceEquals(area, _area);
        }
    }

    // Closed test-fixture policy, not a gameplay serialization subsystem.
    internal static class SerpentinePersistenceReviewPolicy
    {
        internal const string Scope = "snakes";
        internal const string ReceiptScope = "KMG_Sprint17_SnakePersistence_v1";
        internal static bool ObserveLoad(string scenario, string scope)
        {
            return scope == Scope &&
                (scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningPrepare ||
                 scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyCleanup ||
                 scenario == RuntimeTestScenarioCatalog.WorkingSaveExpandedSummoningVerifyAbsent);
        }

        internal static string[] Roles
        { get { return new[] { "viper", "constrictor-snake", "venom-target", "hold-target", "salamander", "salamander-hold-target" }; } }

        internal static string CreatureKey(string role)
        {
            switch (role)
            {
                case "viper": return "viper";
                case "constrictor-snake": return "constrictor-snake";
                case "salamander": return "salamander";
                case "venom-target": case "hold-target": case "salamander-hold-target": return "wolf";
                default: return null;
            }
        }

        internal static bool Owns(string scope, string role, string storedUnit, string actualUnit,
            string storedCaster, string actualCaster, string expectedBlueprint, string actualBlueprint)
        {
            return scope == ReceiptScope && CreatureKey(role) != null &&
                !string.IsNullOrWhiteSpace(storedUnit) && storedUnit == actualUnit &&
                !string.IsNullOrWhiteSpace(storedCaster) && storedCaster == actualCaster &&
                !string.IsNullOrWhiteSpace(expectedBlueprint) && expectedBlueprint == actualBlueprint;
        }

        internal static bool PreservedVenom(int applications, int dc, int ticks, int saves,
            int savedDc, int savedTicks, int savedSaves)
        {
            return applications == 1 && dc == 13 && dc == savedDc &&
                ticks >= 1 && ticks < 6 && ticks == savedTicks && saves == 0 && saves == savedSaves;
        }

        internal static bool NativeAppearanceReady(bool fadedIn, float dissolve,
            bool canAct, bool canMove, bool appearanceLock)
        {
            return fadedIn && dissolve >= 0f && dissolve <= .02f &&
                canAct && canMove && !appearanceLock;
        }

        // Read-only decomposition of the existing strict reload predicate.
        // A label is evidence, never authority to remove a part/buff/condition.
        internal static string[] SessionResetFailures(bool grabPresent,
            bool initiatorPart, bool targetPart, int storedLinks, bool holdBuff,
            bool grappledBuff, bool cantAct, bool cantMove)
        {
            var failures = new List<string>();
            if (!grabPresent) failures.Add("grab-missing");
            if (initiatorPart) failures.Add("initiator-part");
            if (targetPart) failures.Add("target-part");
            if (storedLinks != 0) failures.Add(storedLinks < 0 ? "invalid-link-count" : "stored-links");
            if (holdBuff) failures.Add("hold-buff");
            if (grappledBuff) failures.Add("grappled-buff");
            if (cantAct) failures.Add("cant-act");
            if (cantMove) failures.Add("cant-move");
            return failures.ToArray();
        }

        // Diagnostic labels only: never force a hit/save or authorize a write.
        internal static string ArmingObservation(bool hit, int? damage, int injurySaves,
            bool nativeSavePassed, bool venomPresent)
        {
            if (!hit) return "bite-did-not-hit";
            if (!damage.HasValue) return "no-melee-damage-event";
            if (damage.Value <= 0) return "bite-did-not-wound";
            if (injurySaves != 1) return "injury-save-census-not-one";
            if (nativeSavePassed) return venomPresent ? "venom-after-successful-save" : "native-injury-save-succeeded";
            return venomPresent ? "wounding-bite-failed-save-venom-present" : "venom-missing-after-failed-save";
        }
    }
}
