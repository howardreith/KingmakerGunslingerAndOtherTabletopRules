using System;
using System.Globalization;

namespace KingmakerGunslinger.Explosions
{
    /// <summary>
    /// Immutable runtime evidence for one affected unit's native save and damage.
    /// </summary>
    internal sealed class FirearmExplosionTargetResult
    {
        internal FirearmExplosionTargetResult(
            string target,
            string stableIdentity,
            float distanceMeters,
            bool isExactWielder,
            int reflexNaturalRoll,
            int reflexTotal,
            bool reflexPassed,
            bool halfBecauseSavingThrow,
            int damageBeforeDifficulty,
            int damageWithoutReduction,
            int appliedDamage,
            int hitPointsBefore,
            int hitPointsAfter)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException("A target display name is required.", "target");
            }

            if (string.IsNullOrWhiteSpace(stableIdentity))
            {
                throw new ArgumentException("A stable target identity is required.", "stableIdentity");
            }

            if (float.IsNaN(distanceMeters) ||
                float.IsInfinity(distanceMeters) ||
                distanceMeters < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    "distanceMeters",
                    distanceMeters,
                    "Target distance must be a finite nonnegative value.");
            }

            if (reflexNaturalRoll < 1 || reflexNaturalRoll > 20)
            {
                throw new ArgumentOutOfRangeException("reflexNaturalRoll");
            }

            if (halfBecauseSavingThrow != reflexPassed)
            {
                throw new ArgumentException(
                    "The native half-damage flag must match the Reflex save result.",
                    "halfBecauseSavingThrow");
            }

            if (damageBeforeDifficulty < 0 ||
                damageWithoutReduction < 0 ||
                appliedDamage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "appliedDamage",
                    "Native damage stages must be nonnegative.");
            }

            Target = target.Trim();
            StableIdentity = stableIdentity.Trim();
            DistanceMeters = distanceMeters;
            IsExactWielder = isExactWielder;
            ReflexNaturalRoll = reflexNaturalRoll;
            ReflexTotal = reflexTotal;
            ReflexPassed = reflexPassed;
            HalfBecauseSavingThrow = halfBecauseSavingThrow;
            DamageBeforeDifficulty = damageBeforeDifficulty;
            DamageWithoutReduction = damageWithoutReduction;
            AppliedDamage = appliedDamage;
            HitPointsBefore = hitPointsBefore;
            HitPointsAfter = hitPointsAfter;
        }

        internal string Target { get; private set; }
        internal string StableIdentity { get; private set; }
        internal float DistanceMeters { get; private set; }
        internal bool IsExactWielder { get; private set; }
        internal int ReflexNaturalRoll { get; private set; }
        internal int ReflexTotal { get; private set; }
        internal bool ReflexPassed { get; private set; }
        internal bool HalfBecauseSavingThrow { get; private set; }
        internal int DamageBeforeDifficulty { get; private set; }
        internal int DamageWithoutReduction { get; private set; }
        internal int AppliedDamage { get; private set; }
        internal int HitPointsBefore { get; private set; }
        internal int HitPointsAfter { get; private set; }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "target={0}; unitId={1}; distanceMeters={2:0.###}; exactWielder={3}; reflexNaturalD20={4}; reflexTotal={5}; reflexPassed={6}; halfBecauseSavingThrow={7}; damageBeforeDifficulty={8}; damageWithoutReduction={9}; appliedDamage={10}; hpBefore={11}; hpAfter={12}; hpLoss={13}",
                Target,
                StableIdentity,
                DistanceMeters,
                IsExactWielder,
                ReflexNaturalRoll,
                ReflexTotal,
                ReflexPassed,
                HalfBecauseSavingThrow,
                DamageBeforeDifficulty,
                DamageWithoutReduction,
                AppliedDamage,
                HitPointsBefore,
                HitPointsAfter,
                HitPointsBefore - HitPointsAfter);
        }
    }
}
