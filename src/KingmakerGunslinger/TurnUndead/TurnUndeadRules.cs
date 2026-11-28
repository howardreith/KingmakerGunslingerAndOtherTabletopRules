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
        internal const int DurationMinutes = 1;

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

    internal enum TurnUndeadStartupDecision
    {
        /// <summary>Core blueprints are not ready yet; check again next update.</summary>
        Wait,
        /// <summary>Core blueprints are ready: register now.</summary>
        Run,
        /// <summary>Core initialization reached its terminal failure state.</summary>
        Blocked
    }

    internal sealed class TurnUndeadInstallOutcome
    {
        internal TurnUndeadInstallOutcome(bool registered, bool published,
            Exception registrationFailure, Exception publicationFailure)
        {
            Registered = registered;
            Published = published;
            RegistrationFailure = registrationFailure;
            PublicationFailure = publicationFailure;
        }

        internal bool Registered { get; private set; }
        internal bool Published { get; private set; }
        internal Exception RegistrationFailure { get; private set; }
        internal Exception PublicationFailure { get; private set; }
    }

    /// <summary>
    /// Startup and install lifecycle. Registration of the save-referenced
    /// identities waits only for the real library-ready or terminal-failure
    /// state (never an update count), and it is a separate phase from the
    /// publication steps that follow: a failed publication rolls back only its
    /// own foreign changes and leaves the registered identities resolvable.
    /// </summary>
    internal static class TurnUndeadLifecyclePolicy
    {
        internal static TurnUndeadStartupDecision Decide(bool coreInitialized, bool coreFailed)
        {
            if (coreInitialized) return TurnUndeadStartupDecision.Run;
            return coreFailed ? TurnUndeadStartupDecision.Blocked : TurnUndeadStartupDecision.Wait;
        }

        internal static TurnUndeadInstallOutcome Install(Action register, Action rollbackRegistration,
            Action publish, Action rollbackPublication)
        {
            if (register == null) throw new ArgumentNullException("register");
            if (rollbackRegistration == null) throw new ArgumentNullException("rollbackRegistration");
            if (publish == null) throw new ArgumentNullException("publish");
            if (rollbackPublication == null) throw new ArgumentNullException("rollbackPublication");
            try { register(); }
            catch (Exception failure)
            {
                rollbackRegistration();
                return new TurnUndeadInstallOutcome(false, false, failure, null);
            }
            try { publish(); }
            catch (Exception failure)
            {
                rollbackPublication();
                return new TurnUndeadInstallOutcome(true, false, null, failure);
            }
            return new TurnUndeadInstallOutcome(true, true, null, null);
        }
    }

    internal enum TurnUndeadSeamState
    {
        /// <summary>The member exists with the expected shape and holds a value: integrate.</summary>
        Apply,
        /// <summary>The member exists with the expected shape but its optional value was never created.</summary>
        OptionalAbsent,
        /// <summary>A required member, its shape or a required component is missing: contract drift.</summary>
        Drift
    }

    /// <summary>
    /// Classifies one reflected Call of the Wild seam. The member's existence
    /// and type are required contract; its value is optional (CotW creates some
    /// holders only for some configurations); a present holder must carry the
    /// component it is integrated through.
    /// </summary>
    internal static class TurnUndeadCotwSeamPolicy
    {
        internal static TurnUndeadSeamState Classify(bool memberFound, bool memberTypeMatches,
            bool valuePresent, int matchingComponents)
        {
            if (!memberFound || !memberTypeMatches) return TurnUndeadSeamState.Drift;
            if (!valuePresent) return TurnUndeadSeamState.OptionalAbsent;
            return matchingComponents > 0 ? TurnUndeadSeamState.Apply : TurnUndeadSeamState.Drift;
        }
    }
}
