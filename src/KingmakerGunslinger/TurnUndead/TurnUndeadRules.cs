using System;

namespace KingmakerGunslinger.TurnUndead
{
    internal enum TurnUndeadRoundDecision
    {
        /// <summary>The effect was never initialized by a Turn Undead application.</summary>
        NotOwned,
        /// <summary>Mindless undead receive only the initial save.</summary>
        NoRepeatSave,
        /// <summary>Intelligent undead roll a new Will save this round.</summary>
        RollRepeatSave
    }

    /// <summary>Engine-free Turn Undead rules shared by blueprints and tests.</summary>
    internal static class TurnUndeadRules
    {
        internal const float RadiusFeet = 30f;
        internal const int DurationMinutes = 1;

        /// <summary>
        /// 10 + floor(source class level / 2) + the source statistic's modifier
        /// + genuine DC bonuses. This is exactly what the native rule computes
        /// when the source class level replaces caster level and half of it
        /// replaces spell level (ContextCalculateAbilityParamsBasedOnClass);
        /// unrelated class levels never enter.
        /// </summary>
        internal static int SaveDc(int sourceClassLevel, int statModifier, int genuineBonusDc)
        {
            if (sourceClassLevel < 1)
                throw new ArgumentOutOfRangeException("sourceClassLevel",
                    "A Turn Undead source requires at least one level in its class.");
            return 10 + sourceClassLevel / 2 + statModifier + genuineBonusDc;
        }

        internal static TurnUndeadRoundDecision DecideRound(bool initialized, bool mindless)
        {
            if (!initialized) return TurnUndeadRoundDecision.NotOwned;
            return mindless ? TurnUndeadRoundDecision.NoRepeatSave :
                TurnUndeadRoundDecision.RollRepeatSave;
        }
    }

    /// <summary>
    /// Idempotent append used for foreign registration lists (channel
    /// resistance, optional DC-bonus lists, variant lists). Existing entries
    /// keep their order and references; only missing additions are appended,
    /// each once. Returns null when nothing is missing.
    /// </summary>
    internal static class TurnUndeadListPolicy
    {
        internal static T[] AppendMissing<T>(T[] current, T[] additions) where T : class
        {
            if (additions == null) throw new ArgumentNullException("additions");
            T[] before = current ?? new T[0];
            var result = new System.Collections.Generic.List<T>(before);
            foreach (T addition in additions)
            {
                if (addition == null) throw new ArgumentException("A list addition is null.", "additions");
                bool present = false;
                foreach (T existing in result)
                    if (ReferenceEquals(existing, addition)) { present = true; break; }
                if (!present) result.Add(addition);
            }
            return result.Count == before.Length ? null : result.ToArray();
        }
    }
}
