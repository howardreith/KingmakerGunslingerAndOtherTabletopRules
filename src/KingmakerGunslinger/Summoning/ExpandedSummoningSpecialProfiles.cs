using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    internal enum SummonCoatPattern { Stripes, Spots }

    /// <summary>
    /// A procedural coat for a shared rig: a base colour, a marking colour, a
    /// belly colour and a marking frequency, all channels in [0, 1]. The view
    /// patch rasterizes the rig's own mesh into a texture with these colours;
    /// nothing of the game's art is read or stored.
    /// </summary>
    internal sealed class SummonCoatProfile
    {
        internal SummonCoatProfile(string key, SummonCoatPattern pattern,
            float baseRed, float baseGreen, float baseBlue,
            float markRed, float markGreen, float markBlue,
            float bellyRed, float bellyGreen, float bellyBlue, float frequency)
        {
            Key = key; Pattern = pattern;
            BaseRed = baseRed; BaseGreen = baseGreen; BaseBlue = baseBlue;
            MarkRed = markRed; MarkGreen = markGreen; MarkBlue = markBlue;
            BellyRed = bellyRed; BellyGreen = bellyGreen; BellyBlue = bellyBlue;
            Frequency = frequency;
        }
        internal string Key { get; private set; }
        internal SummonCoatPattern Pattern { get; private set; }
        internal float BaseRed { get; private set; }
        internal float BaseGreen { get; private set; }
        internal float BaseBlue { get; private set; }
        internal float MarkRed { get; private set; }
        internal float MarkGreen { get; private set; }
        internal float MarkBlue { get; private set; }
        internal float BellyRed { get; private set; }
        internal float BellyGreen { get; private set; }
        internal float BellyBlue { get; private set; }
        /// <summary>Markings per body length (stripes) or per body width (spots).</summary>
        internal float Frequency { get; private set; }
        internal bool IsBounded
        {
            get
            {
                return !string.IsNullOrEmpty(Key) && Frequency > 0f && Frequency <= 64f &&
                    new[] { BaseRed, BaseGreen, BaseBlue, MarkRed, MarkGreen, MarkBlue,
                        BellyRed, BellyGreen, BellyBlue }.All(value => value >= 0f && value <= 1f);
            }
        }
    }

    /// <summary>
    /// A bounded visual variant on a shared native rig: a tint multiplier on
    /// the rig's colour slot and an optional emission colour, all channels in
    /// [0, 1]. Domain-testable without the engine.
    /// </summary>
    internal sealed class SummonVisualTintProfile
    {
        internal SummonVisualTintProfile(string key, float tintRed, float tintGreen,
            float tintBlue, float? emissionRed, float? emissionGreen,
            float? emissionBlue)
        {
            Key = key; TintRed = tintRed; TintGreen = tintGreen; TintBlue = tintBlue;
            HasEmission = emissionRed.HasValue && emissionGreen.HasValue &&
                emissionBlue.HasValue;
            EmissionRed = emissionRed ?? 0f; EmissionGreen = emissionGreen ?? 0f;
            EmissionBlue = emissionBlue ?? 0f;
        }
        internal string Key { get; private set; }
        internal float TintRed { get; private set; }
        internal float TintGreen { get; private set; }
        internal float TintBlue { get; private set; }
        internal bool HasEmission { get; private set; }
        internal float EmissionRed { get; private set; }
        internal float EmissionGreen { get; private set; }
        internal float EmissionBlue { get; private set; }
        internal bool IsBounded
        {
            get
            {
                return !string.IsNullOrEmpty(Key) &&
                    new[] { TintRed, TintGreen, TintBlue, EmissionRed, EmissionGreen,
                        EmissionBlue }.All(value => value >= 0f && value <= 1f);
            }
        }
    }

    internal sealed class MephitVariantProfile
    {
        internal MephitVariantProfile(string key, string donorKey, string breathEnergy,
            int breathDice, int breathDieSides, bool breathSickens,
            string spellLikeOne, string spellLikeTwo)
        {
            Key = key; DonorKey = donorKey; BreathEnergy = breathEnergy;
            BreathDice = breathDice; BreathDieSides = breathDieSides;
            BreathSickens = breathSickens; SpellLikeOne = spellLikeOne;
            SpellLikeTwo = spellLikeTwo;
        }
        internal string Key { get; private set; }
        internal string DonorKey { get; private set; }
        /// <summary>Slashing (a physical breath), Cold, Fire or Acid.</summary>
        internal string BreathEnergy { get; private set; }
        internal int BreathDice { get; private set; }
        internal int BreathDieSides { get; private set; }
        internal bool BreathSickens { get; private set; }
        internal string SpellLikeOne { get; private set; }
        internal string SpellLikeTwo { get; private set; }
    }

    internal static class ExpandedSummoningSpecialProfiles
    {
        private static readonly string[] ElementalKeys = BuildElementalKeys();
        private static readonly string[] MephitKeys = {
            "air-mephit", "earth-mephit", "fire-mephit", "water-mephit"
        };
        /// <summary>
        /// Sprint 5 mephits, each on the nearest native summoned mephit as its
        /// donor: (key, donor key, breath energy, breath dice, breath sickens,
        /// first spell-like ability, second spell-like ability). Breath is a
        /// 15-foot cone, Reflex DC 10 + 2 + Constitution as the native mephit
        /// breaths are; a failed save also sickens for three rounds where the
        /// tabletop breath does. Each spell-like ability is one use per
        /// summoning (the tabletop once per hour or per day both exceed a
        /// summoning). Wind wall, chill metal, pyrotechnics and magma form have
        /// no native spell and are omitted; dehydrate and boiling rain are
        /// project bursts. The pale salt and steam mephits sit on the pale
        /// air and water rigs (a tint can only darken) with the earth and fire
        /// subtypes restored.
        /// </summary>
        internal static readonly MephitVariantProfile[] MephitVariants = {
            new MephitVariantProfile("dust-mephit", "air-mephit", "Slashing", 1, 4, true, "Blur", null),
            new MephitVariantProfile("ice-mephit", "water-mephit", "Cold", 1, 4, true, "MagicMissile", null),
            new MephitVariantProfile("magma-mephit", "fire-mephit", "Fire", 1, 8, false, null, null),
            new MephitVariantProfile("ooze-mephit", "water-mephit", "Acid", 1, 4, true, "AcidArrow", "StinkingCloud"),
            new MephitVariantProfile("salt-mephit", "air-mephit", "Slashing", 1, 4, true, "Glitterdust", "Dehydrate"),
            new MephitVariantProfile("steam-mephit", "water-mephit", "Fire", 1, 4, true, "Blur", "BoilingRain")
        };
        /// <summary>
        /// Sprint 6 Giant Spider web: a 50-foot ranged web (the tabletop 50
        /// feet), Reflex DC 10 + half hit dice + Constitution as the native
        /// spider poison scales, entangling one foe through the native
        /// web-grappled state for at most ten rounds (its own per-round
        /// break-free ends it sooner); two uses per summoning (the tabletop
        /// four per day exceeds a summoning).
        /// </summary>
        internal const int GiantSpiderWebUses = 2;
        internal const int GiantSpiderWebRangeFeet = 50;
        internal const int GiantSpiderWebRounds = 10;
        internal const int GiantSpiderWebSpellLevel = 1;
        internal const int MephitSickenedRounds = 3;
        internal const int MephitSpellLikeUses = 1;
        internal const int MephitBreathAiCooldownRounds = 4;
        internal const int MephitBurstRadiusFeet = 20;
        internal const int DehydrateDice = 2;
        internal const int DehydrateDieSides = 8;
        internal const int BoilingRainDice = 2;
        internal const int BoilingRainDieSides = 6;

        internal static MephitVariantProfile MephitVariant(string key)
        { return MephitVariants.Single(value => value.Key == key); }

        /// <summary>
        /// The tint (a multiplier on the native rig's colour slot, so it can
        /// only darken or shift, never brighten) and optional inner glow that
        /// make each variant read as its element on the shared mephit body.
        /// Plain numbers here; the view patch turns them into colours.
        /// </summary>
        internal static readonly SummonVisualTintProfile[] MephitVisualTints = {
            new SummonVisualTintProfile("dust-mephit", 0.85f, 0.75f, 0.50f, null, null, null),
            new SummonVisualTintProfile("ice-mephit", 0.55f, 0.72f, 1.00f, null, null, null),
            new SummonVisualTintProfile("magma-mephit", 0.45f, 0.25f, 0.20f, 0.70f, 0.22f, 0.04f),
            new SummonVisualTintProfile("ooze-mephit", 0.55f, 0.75f, 0.35f, null, null, null),
            new SummonVisualTintProfile("salt-mephit", 0.98f, 0.92f, 0.72f, null, null, null),
            new SummonVisualTintProfile("steam-mephit", 0.88f, 0.80f, 0.72f, 0.25f, 0.12f, 0.06f)
        };

        internal static SummonVisualTintProfile MephitVisualTint(string key)
        { return MephitVisualTints.Single(value => value.Key == key); }

        internal static IReadOnlyList<string> NativeElementalKeys
        { get { return Array.AsReadOnly(ElementalKeys); } }
        internal static IReadOnlyList<string> NativeMephitKeys
        { get { return Array.AsReadOnly(MephitKeys); } }

        internal const int LanternHitDice = 2;
        internal const int LanternStrength = 1;
        internal const int LanternDexterity = 11;
        internal const int LanternConstitution = 12;
        internal const int LanternIntelligence = 6;
        internal const int LanternWisdom = 11;
        internal const int LanternCharisma = 10;
        internal const int LanternSpeedFeet = 60;
        internal const int LanternRayRangeFeet = 30;
        internal const int LanternRayProjectiles = 2;
        internal const int LanternRayDiceCount = 1;
        internal const int LanternRayDieSides = 6;
        internal const int LanternDamageReduction = 10;
        internal const int LanternPoisonSaveBonus = 4;
        internal const int LanternEvilSaveAndAcBonus = 2;

        internal const int InvisibleStalkerHitDice = 7;
        internal const int InvisibleStalkerStrength = 18;
        internal const int InvisibleStalkerDexterity = 19;
        internal const int InvisibleStalkerConstitution = 22;
        internal const int InvisibleStalkerIntelligence = 14;
        internal const int InvisibleStalkerWisdom = 15;
        internal const int InvisibleStalkerCharisma = 11;
        internal const int InvisibleStalkerSpeedFeet = 30;

        internal const int ErinyesHitDice = 9;
        internal const int ErinyesStrength = 20;
        internal const int ErinyesDexterity = 23;
        internal const int ErinyesConstitution = 21;
        internal const int ErinyesIntelligence = 14;
        internal const int ErinyesWisdom = 18;
        internal const int ErinyesCharisma = 21;
        internal const int ErinyesSpeedFeet = 50;

        internal const int ShadowDemonHitDice = 7;
        internal const int ShadowDemonStrength = 17;
        internal const int ShadowDemonDexterity = 20;
        internal const int ShadowDemonConstitution = 14;
        internal const int ShadowDemonIntelligence = 14;
        internal const int ShadowDemonWisdom = 13;
        internal const int ShadowDemonCharisma = 17;
        internal const int ShadowDemonSpeedFeet = 40;
        internal const int ShadowDemonDamageReduction = 10;
        internal const int ShadowDemonEnergyResistance = 10;
        internal const int ShadowDemonSpellResistance = 17;
        internal const int ShadowDemonColdDamageDice = 1;

        internal const int SalamanderHitDice = 8;
        internal const int SalamanderStrength = 16;
        internal const int SalamanderDexterity = 13;
        internal const int SalamanderConstitution = 18;
        internal const int SalamanderIntelligence = 14;
        internal const int SalamanderWisdom = 15;
        internal const int SalamanderCharisma = 13;
        internal const int SalamanderSpeedFeet = 20;
        internal const int SalamanderHeatDice = 1;
        internal const int SalamanderConstrictDice = 2;
        internal const int SalamanderConstrictBonus = 4;

        internal const int SuccubusHitDice = 8;
        internal const int SuccubusStrength = 13;
        internal const int SuccubusDexterity = 17;
        internal const int SuccubusConstitution = 14;
        internal const int SuccubusIntelligence = 18;
        internal const int SuccubusWisdom = 13;
        internal const int SuccubusCharisma = 27;
        internal const int SuccubusSpeedFeet = 30;
        internal const int SuccubusDamageReduction = 10;
        internal const int SuccubusEnergyResistance = 10;
        internal const int SuccubusSpellResistance = 18;
        internal const int SuccubusDominateRounds = 3;
        internal const int SuccubusEnergyDrainRounds = 1;

        internal const int BebelithHitDice = 12;
        internal const int BebelithStrength = 28;
        internal const int BebelithDexterity = 12;
        internal const int BebelithConstitution = 24;
        internal const int BebelithIntelligence = 11;
        internal const int BebelithWisdom = 13;
        internal const int BebelithCharisma = 13;
        internal const int BebelithSpeedFeet = 40;
        internal const int BebelithDamageReduction = 10;
        internal const int BebelithDismantleReflexDc = 25;
        internal const int BebelithDismantleAcPenalty = 2;
        internal const int BebelithDismantleRounds = 1;
        internal const int BebelithDemonHunterBonus = 2;

        internal const int PixieHitDice = 4;
        internal const int PixieStrength = 7;
        internal const int PixieDexterity = 21;
        internal const int PixieConstitution = 12;
        internal const int PixieIntelligence = 16;
        internal const int PixieWisdom = 15;
        internal const int PixieCharisma = 16;
        internal const int PixieSpeedFeet = 60;
        internal const int PixieDamageReduction = 10;
        internal const int PixieSpellResistance = 15;
        internal const int PixieSleepArrowUses = 16;
        internal const int PixieSleepArrowWillDc = 15;
        internal const int PixieSleepArrowRounds = 50;
        internal const int PixieDanceUses = 1;
        internal const int PixieDanceCasterLevel = 8;
        internal const int PixieDanceSpellLevel = 6;

        /// <summary>
        /// Cyclops Flash of Insight, bounded (Sprint 3). Tabletop: once per
        /// day, as an immediate action, the cyclops chooses the exact result
        /// of one of its own die rolls. Here: once per summoning, as a swift
        /// action, its next attack in the round is an automatic critical hit:
        /// Kingmaker's automatic-hit path never rolls, so the threat and its
        /// confirmation can only be granted together there.
        /// </summary>
        internal const int CyclopsFlashOfInsightUses = 1;
        internal const int CyclopsFlashOfInsightRounds = 1;

        /// <summary>
        /// Shared summon grapple lifecycle (Sprint 4). A grab is the game's own
        /// grapple check after a hit with a grab weapon, with the tabletop +4
        /// grab bonus; the native hold parts carry the state. Each new round
        /// the holder maintains with a grapple check at the tabletop +5,
        /// dealing the grab weapon's damage (plus constrict) on success and
        /// releasing on failure. Constrict is 1.5 x the Strength modifier on
        /// top of its dice. The worm swallows on the grab check instead of
        /// holding, as Kingmaker's own worm does.
        /// </summary>
        internal const int SummonGrabManeuverBonus = 4;
        internal const int SummonHoldMaintainBonus = 5;
        internal const int ShamblingMoundConstrictDice = 2;
        internal const int ShamblingMoundConstrictBonus = 7;

        internal static bool ShouldAttemptBebelithDismantle(bool isClaw,
            bool isHit, bool targetHasArmor, int priorClawHits,
            bool alreadyAttempted)
        {
            return isClaw && isHit && targetHasArmor && priorClawHits == 1 &&
                !alreadyAttempted;
        }

        internal static bool IsBebelithDemonHuntingTarget(bool isOutsider,
            int alignment)
        { return isOutsider && alignment == 20; }

        internal static bool ShouldSpendPixieSleepArrow(bool isSleepBow,
            bool isHit, int remainingUses)
        { return isSleepBow && isHit && remainingUses > 0; }

        /// <summary>
        /// The armed Flash of Insight state converts exactly the owner's own
        /// attack rolls; rolls it merely witnesses (a defender's, an ally's)
        /// are untouched.
        /// </summary>
        internal static bool ShouldApplyFlashOfInsight(bool isOwnerAttackRoll,
            bool stateArmed)
        { return isOwnerAttackRoll && stateArmed; }

        /// <summary>
        /// A grab starts only from a hit with a grab weapon, by a summon that
        /// neither holds nor has swallowed anyone, against a live target that
        /// is neither held nor swallowed and is not the summon itself.
        /// </summary>
        internal static bool ShouldAttemptSummonGrab(bool isHit, bool isGrabWeapon,
            bool ownerHolding, bool targetHeld, bool targetSwallowed,
            bool selfTarget)
        {
            return isHit && isGrabWeapon && !ownerHolding && !targetHeld &&
                !targetSwallowed && !selfTarget;
        }

        /// <summary>
        /// A hold survives a new round only while the target is still the one
        /// the summon owns and the maintain check succeeds; otherwise it is
        /// released.
        /// </summary>
        internal static bool ShouldMaintainSummonHold(bool targetOwned,
            bool maintainSuccess)
        { return targetOwned && maintainSuccess; }

        /// <summary>
        /// Sprint 7: the cats' rake claws (the last two additional limbs of
        /// the body; the game lists secondary limbs after the additional
        /// ones) attack only on a charge - Pounce makes the charge a full
        /// attack - or while the cat holds a grappled foe. Any other attack
        /// with a rake claw is an automatic, silent miss: no roll, no damage,
        /// no combat-log line.
        /// </summary>
        internal const int CatRakeSlotCount = 2;
        internal static bool ShouldRakeApply(bool isRakeWeapon, bool isCharge,
            bool isHolding)
        { return !isRakeWeapon || isCharge || isHolding; }

        /// <summary>
        /// Sprint 8: the Cheetah's sprint - a swift, once-per-summoning burst
        /// of +30 feet for one round (the game's own speed cap still applies);
        /// the tabletop once-per-hour tenfold sprint cannot repeat within one
        /// summoning either way.
        /// </summary>
        internal const int CheetahSprintUses = 1;
        internal const int CheetahSprintRounds = 1;
        internal const int CheetahSprintBonusFeet = 30;
        /// <summary>
        /// Sprint 8: the procedural coats generated in the leopard rig's own
        /// texture space at view attach. Stripes for the tiger, spots for the
        /// cheetah; plain numbers here, pixels only at runtime.
        /// </summary>
        internal static readonly SummonCoatProfile TigerCoat = new SummonCoatProfile(
            "tiger", SummonCoatPattern.Stripes, 0.86f, 0.46f, 0.12f, 0.12f, 0.08f, 0.05f,
            0.96f, 0.92f, 0.82f, 11f);
        internal static readonly SummonCoatProfile CheetahCoat = new SummonCoatProfile(
            "cheetah", SummonCoatPattern.Spots, 0.88f, 0.70f, 0.38f, 0.16f, 0.10f, 0.06f,
            0.97f, 0.94f, 0.85f, 26f);

        /// <summary>
        /// The Lion's bounded visual: the leopard rig warmed to a tawny coat.
        /// </summary>
        internal static readonly SummonVisualTintProfile LionVisualTint =
            new SummonVisualTintProfile("lion", 0.90f, 0.72f, 0.42f, null, null, null);

        /// <summary>
        /// Constrict adds one and a half times the Strength modifier to its
        /// dice, the tabletop bonus for a constricting natural attack.
        /// </summary>
        internal static int ConstrictBonus(int strengthModifier)
        { return strengthModifier + strengthModifier / 2; }

        internal static void Validate()
        {
            if (MephitVisualTints.Length != 6 ||
                !MephitVisualTints.Select(value => value.Key).SequenceEqual(
                    MephitVariants.Select(value => value.Key)) ||
                MephitVisualTints.Any(value => !value.IsBounded))
                throw new InvalidOperationException(
                    "Sprint 5 mephit visual tint profile changed.");
            if (MephitVariants.Length != 6 || MephitVariants.Select(value => value.Key)
                    .Distinct(StringComparer.Ordinal).Count() != 6 ||
                MephitVariants.Any(value => !MephitKeys.Contains(value.DonorKey)) ||
                MephitSickenedRounds != 3 || MephitSpellLikeUses != 1 ||
                MephitBreathAiCooldownRounds != 4)
                throw new InvalidOperationException(
                    "Sprint 5 mephit variant profile changed.");
            if (CheetahSprintUses != 1 || CheetahSprintRounds != 1 ||
                CheetahSprintBonusFeet != 30 || !TigerCoat.IsBounded || !CheetahCoat.IsBounded)
                throw new InvalidOperationException("Sprint 8 cat profile changed.");
            if (CatRakeSlotCount != 2 || !LionVisualTint.IsBounded ||
                ShouldRakeApply(true, false, false) || !ShouldRakeApply(true, true, false) ||
                !ShouldRakeApply(true, false, true) || !ShouldRakeApply(false, false, false))
                throw new InvalidOperationException("Sprint 7 rake profile changed.");
            if (GiantSpiderWebUses != 2 || GiantSpiderWebRangeFeet != 50 ||
                GiantSpiderWebRounds != 10 || GiantSpiderWebSpellLevel != 1)
                throw new InvalidOperationException(
                    "Sprint 5 mephit variant profile changed.");
            if (ElementalKeys.Length != 24 || MephitKeys.Length != 4)
                throw new InvalidOperationException(
                    "Native elemental/mephit profile count changed.");
            string[] keys = ElementalKeys.Concat(MephitKeys).ToArray();
            if (keys.Distinct(StringComparer.Ordinal).Count() != keys.Length)
                throw new InvalidOperationException("Native reuse keys are duplicated.");
            foreach (string key in keys)
            {
                SummonCreatureSpec creature = ExpandedSummoningCatalog.All.SingleOrDefault(
                    value => value.Key == key);
                if (creature == null || !ExpandedSummoningDonorCatalog.For(key)
                    .DedicatedSummon)
                    throw new InvalidOperationException(
                        "Native reuse requires an exact dedicated donor: " + key + ".");
            }
            if (ErinyesHitDice != 9 || ErinyesDexterity != 23 ||
                ErinyesSpeedFeet != 50 || BebelithHitDice != 12 ||
                BebelithDismantleReflexDc != 25 ||
                BebelithDismantleAcPenalty < 1 || BebelithDismantleRounds != 1 ||
                PixieHitDice != 4 || PixieSleepArrowUses != 16 ||
                PixieSleepArrowWillDc != 15 || PixieSleepArrowRounds != 50 ||
                PixieDanceUses != 1 || PixieDanceCasterLevel != 8 ||
                CyclopsFlashOfInsightUses != 1 ||
                CyclopsFlashOfInsightRounds != 1 ||
                SummonGrabManeuverBonus != 4 || SummonHoldMaintainBonus != 5 ||
                ShamblingMoundConstrictDice != 2 ||
                ShamblingMoundConstrictBonus != ConstrictBonus(5))
                throw new InvalidOperationException(
                    "Bebelith/Pixie/Cyclops/grapple bounded special profile changed.");
        }

        private static string[] BuildElementalKeys()
        {
            string[] sizes = { "small", "medium", "large", "huge", "greater", "elder" };
            string[] elements = { "air", "earth", "fire", "water" };
            return sizes.SelectMany(size => elements.Select(element =>
                size + "-" + element + "-elemental")).ToArray();
        }
    }
}
