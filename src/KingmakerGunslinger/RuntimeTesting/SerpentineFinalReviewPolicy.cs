using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Fixed request-local evidence surface; no production gameplay consumers.
    internal static class SerpentineFinalReviewPolicy
    {
        internal static bool ValidExit(string scenario, bool exitAfterCompletion)
        {
            return scenario != RuntimeTestScenarioCatalog.DisposableExpandedSummoningSnakeFinalReview ||
                exitAfterCompletion;
        }

        internal static SummonVariantSpec[] Routes()
        {
            return ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                .Concat(ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly))
                .Where(value => value.Creature.Key == "viper" ||
                    value.Creature.Key == "constrictor-snake").ToArray();
        }

        internal static bool Quantity(SummonMultiplicity kind, int count)
        {
            switch (kind)
            {
                case SummonMultiplicity.One: return count == 1;
                case SummonMultiplicity.OneD3: return count >= 1 && count <= 3;
                case SummonMultiplicity.OneD4PlusOne: return count >= 2 && count <= 5;
                default: return false;
            }
        }

        internal static bool PlayedNativeClip(bool exactNativeAction, bool started,
            string clipName, float duration, double time, float weight)
        {
            return exactNativeAction && started && !string.IsNullOrEmpty(clipName) &&
                !float.IsNaN(duration) && !float.IsInfinity(duration) && duration > 0 &&
                !double.IsNaN(time) && !double.IsInfinity(time) && time > 0 &&
                !float.IsNaN(weight) && !float.IsInfinity(weight) && weight > 0 && weight <= 1.001f;
        }

        // Native RuleSummonUnit adds six seconds only in turn-based mode
        // for a full-round non-trap caster. This closed paused RTWP route
        // measures Duration+BonusDuration directly; never borrow the TB gate.
        internal static bool PrivateRtwpDuration(int captures, int facts, bool turnBased,
            bool ownedContext, int casterLevel, bool permanent,
            double baseSeconds, double bonusSeconds, double remainingSeconds)
        { return PrivateRtwpDuration(captures, facts, turnBased, ownedContext, casterLevel, permanent,
            baseSeconds, bonusSeconds, remainingSeconds, 0); }

        internal static bool PrivateRtwpDuration(int captures, int facts, bool turnBased,
            bool ownedContext, int casterLevel, bool permanent,
            double baseSeconds, double bonusSeconds, double remainingSeconds, double nativeElapsedSeconds)
        {
            return captures == 1 && facts == 1 && !turnBased && ownedContext &&
                casterLevel == 20 && !permanent &&
                new[] { baseSeconds, bonusSeconds, remainingSeconds, nativeElapsedSeconds }.All(value =>
                    !double.IsNaN(value) && !double.IsInfinity(value)) &&
                Math.Abs(baseSeconds - 120d) <= .001d && bonusSeconds >= 0 &&
                nativeElapsedSeconds >= 0 && nativeElapsedSeconds < baseSeconds + bonusSeconds &&
                Math.Abs(remainingSeconds + nativeElapsedSeconds - (baseSeconds + bonusSeconds)) <= .001d;
        }

        internal static bool IsolatedLifecyclePair(bool distinctOwnedActors,
            bool eitherPlayerFaction, bool eitherPartyGroup, bool ownerEnemy,
            bool attackerEnemy, int foreignEnemyRelations)
        {
            // Receiving a hit does not require player command authority.
            // Both actors must nevertheless remain isolated native enemies.
            return distinctOwnedActors && !eitherPlayerFaction && !eitherPartyGroup &&
                ownerEnemy && attackerEnemy && foreignEnemyRelations == 0;
        }

        internal static bool NativeFrontalHit(bool attackHit, float torsoFacingDot)
        {
            // UnitHitFxManager.HandleMeleeAttackHit promotes the native float
            // dot product to double before comparing with the literal 0.3.
            // This is fixture eligibility, never evidence of actual playback.
            return attackHit && !float.IsNaN(torsoFacingDot) && !float.IsInfinity(torsoFacingDot) &&
                torsoFacingDot > .3d && torsoFacingDot <= 1.001f;
        }

        internal static bool FaithfulHitLifecycle(bool exactNativeSet, bool nativeControlHasHit,
            bool candidateHasHit, bool actualFrontalWound, bool finitePose, bool alive, bool playedHit)
        {
            // The visual lifecycle contract requires stability under actual
            // damage, not inventing a flinch absent from the native rig.
            // A present native carrier still requires actual clip playback.
            return exactNativeSet && nativeControlHasHit == candidateHasHit && actualFrontalWound &&
                finitePose && alive && (nativeControlHasHit ? playedHit : !playedHit);
        }
    }
}
