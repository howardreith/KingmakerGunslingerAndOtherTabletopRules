using System;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Which rider a successful grapple maintain resolves. Exactly one, ever.
    /// </summary>
    internal enum CrocodilianMaintainRider
    {
        None = 0,
        DeathRoll = 1,
        SwallowWhole = 2
    }

    /// <summary>
    /// One crocodilian's rules numbers, derived from its stat block rather
    /// than copied out of it.
    ///
    /// <para>The distinction this type exists for is that a death roll is not
    /// a second bite. The printed blocks separate them - the Crocodile bites
    /// for 1d8+4 and death rolls for 1d8+6, the Dire Crocodile bites for
    /// 3d6+13 and death rolls for 3d6+19 - because an ordinary natural attack
    /// adds the Strength modifier and a death roll adds one and a half times
    /// it, the same bonus a constricting attack gets. Replaying the bite's
    /// damage would under-report both creatures, and hard-coding the printed
    /// flat bonus would hide the derivation that makes it right.</para>
    /// </summary>
    internal sealed class CrocodilianRulesProfile
    {
        internal CrocodilianRulesProfile(string key, int strength,
            int deathRollDiceCount, int deathRollDieSides,
            bool hasSwallow, int swallowDiceCount, int swallowDieSides,
            int swallowInteriorArmorClass, int swallowInteriorHitPoints,
            int swallowSizeDelta, int sprintBonusFeet, int sprintRounds,
            int sprintCooldownRounds)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
            Key = key;
            Strength = strength;
            DeathRollDiceCount = deathRollDiceCount;
            DeathRollDieSides = deathRollDieSides;
            HasSwallow = hasSwallow;
            SwallowDiceCount = swallowDiceCount;
            SwallowDieSides = swallowDieSides;
            SwallowInteriorArmorClass = swallowInteriorArmorClass;
            SwallowInteriorHitPoints = swallowInteriorHitPoints;
            SwallowSizeDelta = swallowSizeDelta;
            SprintBonusFeet = sprintBonusFeet;
            SprintRounds = sprintRounds;
            SprintCooldownRounds = sprintCooldownRounds;
        }

        internal string Key { get; private set; }
        internal int Strength { get; private set; }
        internal int StrengthModifier { get { return (Strength - 10) / 2; } }

        internal int DeathRollDiceCount { get; private set; }
        internal int DeathRollDieSides { get; private set; }

        /// <summary>
        /// One and a half times the Strength modifier, the tabletop bonus for
        /// a constricting or crushing natural attack. Shared with
        /// <see cref="ExpandedSummoningSpecialProfiles.ConstrictBonus"/> so the
        /// two cannot drift apart.
        /// </summary>
        internal int DeathRollBonus
        {
            get
            {
                return ExpandedSummoningSpecialProfiles.ConstrictBonus(
                    StrengthModifier);
            }
        }

        /// <summary>
        /// A death roll works on a foe of the crocodilian's own size or
        /// smaller, which is a delta of zero - unlike swallow whole, which
        /// needs one size category smaller.
        /// </summary>
        internal int DeathRollTargetSizeDelta { get { return 0; } }

        internal bool HasSwallow { get; private set; }
        internal int SwallowDiceCount { get; private set; }
        internal int SwallowDieSides { get; private set; }

        /// <summary>
        /// The swallow's crushing damage adds the ordinary Strength modifier,
        /// not the death roll's one-and-a-half: the Dire Crocodile's printed
        /// 3d6+13 is its Strength modifier exactly, the same bonus its bite
        /// carries.
        /// </summary>
        internal int SwallowBonus { get { return StrengthModifier; } }

        internal int SwallowInteriorArmorClass { get; private set; }
        internal int SwallowInteriorHitPoints { get; private set; }
        internal int SwallowSizeDelta { get; private set; }

        internal int SprintBonusFeet { get; private set; }
        internal int SprintRounds { get; private set; }
        internal int SprintCooldownRounds { get; private set; }

        internal string DeathRollDamage
        {
            get
            {
                return DeathRollDiceCount + "d" + DeathRollDieSides + "+" +
                    DeathRollBonus;
            }
        }

        internal string SwallowDamage
        {
            get
            {
                return !HasSwallow ? "<none>" :
                    SwallowDiceCount + "d" + SwallowDieSides + "+" +
                    SwallowBonus;
            }
        }
    }

    /// <summary>
    /// The crocodilians' signature rules, in one place so both creatures and
    /// every test read the same derivation.
    ///
    /// <para>The Crocodile has shipped since Phase 1 with its whole signature
    /// recorded as absent - "Grab, death roll, sprint, and hold breath are
    /// omitted because no duration-bound summon-safe native graph was proven".
    /// Three of those four have carriers this project already drives, and this
    /// is where their numbers come from.</para>
    /// </summary>
    internal static class CrocodilianRulesPolicy
    {
        /// <summary>
        /// Both crocodilians sprint from 20 feet to 40 feet for one round,
        /// once per minute. A minute is ten rounds, so the recharge is ten
        /// rounds rather than the Cheetah's once-per-summoning burst - which
        /// is a different limit for a different creature and must not be
        /// copied onto these two.
        /// </summary>
        internal const int SprintBonusFeet = 20;
        internal const int SprintRounds = 1;
        internal const int SprintCooldownRounds = 10;

        private static readonly CrocodilianRulesProfile[] Values = {
            // Crocodile: bite 1d8+4, death roll 1d8+6 plus trip. No swallow.
            new CrocodilianRulesProfile("crocodile", 19, 1, 8,
                false, 0, 0, 0, 0, 0,
                SprintBonusFeet, SprintRounds, SprintCooldownRounds),
            // Dire Crocodile: bite 3d6+13, death roll 3d6+19 plus trip,
            // swallow whole 3d6+13 behind AC 16 and 13 hit points, up to one
            // size category smaller than itself.
            new CrocodilianRulesProfile("dire-crocodile", 37, 3, 6,
                true, 3, 6, 16, 13, -1,
                SprintBonusFeet, SprintRounds, SprintCooldownRounds)
        };

        internal static CrocodilianRulesProfile For(string key)
        {
            CrocodilianRulesProfile profile = Values.FirstOrDefault(
                value => string.Equals(value.Key, key, StringComparison.Ordinal));
            if (profile == null)
                throw new InvalidOperationException(
                    "No crocodilian rules profile for " + key + ".");
            return profile;
        }

        internal static bool Has(string key)
        {
            return Values.Any(value =>
                string.Equals(value.Key, key, StringComparison.Ordinal));
        }

        /// <summary>
        /// Whether a death roll may apply at all, before any roll is made.
        ///
        /// <para>Every clause is a separate way for it to be refused, which is
        /// what the tests exercise one at a time: the creature must still hold
        /// this exact target, the target must have been held since the round
        /// began rather than seized this instant, the maintain check must have
        /// succeeded, and the target must be the crocodilian's own size or
        /// smaller.</para>
        /// </summary>
        internal static bool ShouldDeathRollOnMaintain(bool hasDeathRoll,
            bool targetOwned, int roundsHeld, bool maintainSuccess,
            bool sizeAllowed)
        {
            return hasDeathRoll && targetOwned && maintainSuccess &&
                ExpandedSummoningSpecialProfiles.IsHeldSinceRoundStart(
                    targetOwned, roundsHeld) && sizeAllowed;
        }

        /// <summary>
        /// Which single rider a successful maintain resolves.
        ///
        /// <para>The Dire Crocodile has both abilities, and one successful
        /// grapple check must never resolve both. The choice is deterministic
        /// and made from what is true before the check's outcome is applied,
        /// so no future result decides it: a target small enough to swallow is
        /// swallowed, and a target too large to swallow but no larger than the
        /// crocodilian is death rolled. That leaves both abilities reachable -
        /// swallow against a smaller foe, death roll against one of its own
        /// size - and makes which one happened predictable from the target's
        /// size alone.</para>
        ///
        /// <para>Returning a single value rather than two independent booleans
        /// is the point: there is no state in which a caller can act on both.
        /// </para>
        /// </summary>
        internal static CrocodilianMaintainRider SelectMaintainRider(
            bool maintainSuccess, bool targetOwned, int roundsHeld,
            bool hasDeathRoll, bool deathRollSizeAllowed,
            bool hasSwallow, bool swallowSizeAllowed, bool swallowAvailable)
        {
            if (!maintainSuccess || !targetOwned ||
                    !ExpandedSummoningSpecialProfiles.IsHeldSinceRoundStart(
                        targetOwned, roundsHeld))
                return CrocodilianMaintainRider.None;
            if (hasSwallow && swallowAvailable && swallowSizeAllowed)
                return CrocodilianMaintainRider.SwallowWhole;
            if (hasDeathRoll && deathRollSizeAllowed)
                return CrocodilianMaintainRider.DeathRoll;
            return CrocodilianMaintainRider.None;
        }

        /// <summary>
        /// A death roll keeps the hold. The tabletop text is explicit that the
        /// crocodile maintains its grapple, so a rider that released the
        /// target would turn a signature ability into a way of escaping one.
        /// </summary>
        internal static bool KeepsGrappleAfterDeathRoll { get { return true; } }

        /// <summary>
        /// Sprint's own gate. It is a once-per-minute burst, so it needs its
        /// resource, and a creature that cannot act cannot spend it.
        /// </summary>
        internal static bool MaySprint(bool alive, bool canAct, int cooldownRemaining)
        {
            return alive && canAct && cooldownRemaining <= 0;
        }

        /// <summary>
        /// The cooldown after a use, and after each round of waiting. A
        /// separate function from <see cref="MaySprint"/> so a save and reload
        /// in the middle of either can be checked against the same arithmetic.
        /// </summary>
        internal static int AdvanceSprintCooldown(int cooldownRemaining,
            int rounds)
        {
            if (rounds < 0) throw new ArgumentOutOfRangeException("rounds");
            int remaining = cooldownRemaining - rounds;
            return remaining < 0 ? 0 : remaining;
        }

        internal static void Validate()
        {
            if (Values.Length != 2 ||
                Values.Select(value => value.Key)
                    .Distinct(StringComparer.Ordinal).Count() != Values.Length)
                throw new InvalidOperationException(
                    "The crocodilian rules catalog is incomplete or duplicated.");
            // The printed lines, derived. A literal from a stat block would
            // pass this without proving anything.
            CrocodilianRulesProfile crocodile = For("crocodile");
            CrocodilianRulesProfile dire = For("dire-crocodile");
            if (crocodile.DeathRollDamage != "1d8+6" ||
                dire.DeathRollDamage != "3d6+19" ||
                dire.SwallowDamage != "3d6+13" ||
                crocodile.HasSwallow || !dire.HasSwallow ||
                dire.SwallowInteriorArmorClass != 16 ||
                dire.SwallowInteriorHitPoints != 13 ||
                dire.SwallowSizeDelta != -1 ||
                crocodile.DeathRollTargetSizeDelta != 0 ||
                dire.DeathRollTargetSizeDelta != 0)
                throw new InvalidOperationException(
                    "A crocodilian's derived signature numbers changed.");
            foreach (CrocodilianRulesProfile profile in Values)
                if (profile.SprintBonusFeet != SprintBonusFeet ||
                    profile.SprintRounds != SprintRounds ||
                    profile.SprintCooldownRounds != SprintCooldownRounds)
                    throw new InvalidOperationException(
                        "A crocodilian's sprint limit changed.");
        }
    }
}
