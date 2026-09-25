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
    /// the rig's colour slot (channels in [0, 1]) and an optional rim light
    /// colour (the shader's HDR glow, channels in [0, 8]) - on the mephit
    /// rigs the rim light is the translucent body's visible colour, and the
    /// native elements differ by exactly that. Domain-testable without the
    /// engine.
    /// </summary>
    internal sealed class SummonVisualTintProfile
    {
        internal const float RimChannelMaximum = 8f;

        internal SummonVisualTintProfile(string key, float tintRed, float tintGreen,
            float tintBlue, float? rimRed, float? rimGreen, float? rimBlue)
        {
            Key = key; TintRed = tintRed; TintGreen = tintGreen; TintBlue = tintBlue;
            HasRim = rimRed.HasValue && rimGreen.HasValue && rimBlue.HasValue;
            RimRed = rimRed ?? 0f; RimGreen = rimGreen ?? 0f; RimBlue = rimBlue ?? 0f;
        }
        internal string Key { get; private set; }
        internal float TintRed { get; private set; }
        internal float TintGreen { get; private set; }
        internal float TintBlue { get; private set; }
        internal bool HasRim { get; private set; }
        internal float RimRed { get; private set; }
        internal float RimGreen { get; private set; }
        internal float RimBlue { get; private set; }
        internal bool IsBounded
        {
            get
            {
                return !string.IsNullOrEmpty(Key) &&
                    new[] { TintRed, TintGreen, TintBlue }.All(value =>
                        value >= 0f && value <= 1f) &&
                    new[] { RimRed, RimGreen, RimBlue }.All(value =>
                        value >= 0f && value <= RimChannelMaximum);
            }
        }
    }

    /// <summary>What the Wind Wall does to one incoming attack roll.</summary>
    internal enum SummonWindWallOutcome { None, Deflected, MissChance }

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
        /// no native spell: under the correction order they are project-owned,
        /// bounded abilities (constants below); dehydrate and boiling rain are
        /// project bursts. The pale salt and steam mephits sit on the pale
        /// air and water rigs (a tint can only darken) with the earth and fire
        /// subtypes restored.
        /// </summary>
        internal static readonly MephitVariantProfile[] MephitVariants = {
            new MephitVariantProfile("dust-mephit", "air-mephit", "Slashing", 1, 4, true, "Blur", "WindWall"),
            new MephitVariantProfile("ice-mephit", "water-mephit", "Cold", 1, 4, true, "MagicMissile", "ChillMetal"),
            new MephitVariantProfile("magma-mephit", "fire-mephit", "Fire", 1, 8, false, "Pyrotechnics", "MagmaForm"),
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
        /// <summary>A web catches a creature up to one size category larger than the spinner.</summary>
        internal const int GiantSpiderWebMaxSizeDelta = 1;
        internal static bool IsWebTargetSizeAllowed(int targetSize, int spinnerSize,
            int maxSizeDelta)
        { return targetSize <= spinnerSize + maxSizeDelta; }
        internal const int MephitSickenedRounds = 3;
        internal const int MephitSpellLikeUses = 1;
        internal const int MephitBreathAiCooldownRounds = 4;
        internal const int MephitBurstRadiusFeet = 20;
        internal const int DehydrateDice = 2;
        internal const int DehydrateDieSides = 8;
        internal const int BoilingRainDice = 2;
        internal const int BoilingRainDieSides = 6;

        /// <summary>
        /// The chartered mephit roles (correction order), project-owned and
        /// bounded. Every spell-like DC is Charisma-based at the mephit's
        /// caster level 6 (the stat blocks' DC 14 at spell level 2). Wind
        /// wall: an allies-only 15-foot cylinder around the mephit for six
        /// rounds (one per level) in which arrows and bolts aimed at an ally
        /// are deflected and any other normal ranged weapon has a 30% miss
        /// chance; rays and touch deliveries pass as spells pass the tabletop
        /// wall. Chill metal: Will negates, seven rounds of cold - none, 1d4,
        /// 2d4, 2d4, 2d4, 1d4, none - in full against a creature in metal
        /// armor, the table's minimal 1 or 2 points against one carrying only
        /// a metal weapon, nothing against one carrying no metal (which is not
        /// a valid target). Pyrotechnics: fireworks blind every enemy within
        /// 20 feet for 1d4+1 rounds, Will negates. Magma form: five rounds as
        /// a pool of lava - damage reduction 20/magic, speed 10 feet, no
        /// attacks, breath and spell-like abilities intact; the brain fights
        /// three rounds before it may pool.
        /// </summary>
        internal const int MephitSpellLikeCasterLevel = 6;
        internal const int WindWallRounds = 6;
        internal const int WindWallRadiusFeet = 15;
        internal const int WindWallSpellLevel = 3;
        internal const int WindWallOtherRangedMissChance = 30;
        internal const int ChillMetalRounds = 7;
        internal const int ChillMetalSpellLevel = 2;
        internal const int PyrotechnicsSpellLevel = 2;
        internal const int PyrotechnicsBlindDieSides = 4;
        internal const int PyrotechnicsBlindBonusRounds = 1;
        internal const int MagmaFormRounds = 5;
        internal const int MagmaFormDamageReduction = 20;
        internal const int MagmaFormSpeedFeet = 10;
        internal const int MagmaFormAiStartCooldownRounds = 3;

        internal static SummonWindWallOutcome WindWallOutcome(bool isRangedAttack,
            bool isArrowOrBolt, bool isSpellDelivery)
        {
            if (!isRangedAttack || isSpellDelivery) return SummonWindWallOutcome.None;
            return isArrowOrBolt ? SummonWindWallOutcome.Deflected :
                SummonWindWallOutcome.MissChance;
        }

        /// <summary>d4s of cold in the given round of chill metal (round 1 is the casting round).</summary>
        internal static int ChillMetalDice(int round)
        {
            if (round == 2 || round == 6) return 1;
            if (round >= 3 && round <= 5) return 2;
            return 0;
        }

        /// <summary>The table's minimal damage (1 for 1d4, 2 for 2d4) for metal that is not armor.</summary>
        internal static int ChillMetalMinimalDamage(int round)
        { return ChillMetalDice(round); }

        /// <summary>2: full dice (metal armor); 1: minimal (a metal weapon only); 0: no metal, no target.</summary>
        internal static int ChillMetalTier(bool wearsMetalArmor, bool wieldsMetalWeapon)
        { return wearsMetalArmor ? 2 : wieldsMetalWeapon ? 1 : 0; }

        internal static MephitVariantProfile MephitVariant(string key)
        { return MephitVariants.Single(value => value.Key == key); }

        /// <summary>
        /// What makes each variant read as its element on the shared mephit
        /// body: the rim light colour, the HDR glow through which the
        /// translucent mephit body is seen (the native air mephit glows
        /// 1.3/1.2/1.13, the native fire mephit 4.16/1.51/0.32 - rounds 8-11
        /// showed that neither a tint on the rig's tint slot, which the
        /// game's material controller rewrites, nor a project main texture
        /// changes the frame). The tint multiplier stays white here. Plain
        /// numbers; the view patch turns them into colours.
        /// </summary>
        internal static readonly SummonVisualTintProfile[] MephitVisualTints = {
            // warm sand glow against the air mephit's white
            new SummonVisualTintProfile("dust-mephit", 1f, 1f, 1f, 2.4f, 1.7f, 0.7f),
            // cold cyan-white against the water mephit's blue
            new SummonVisualTintProfile("ice-mephit", 1f, 1f, 1f, 1.0f, 3.4f, 3.8f),
            // deep ember red, darker and redder than the fire mephit's orange
            new SummonVisualTintProfile("magma-mephit", 1f, 1f, 1f, 3.2f, 0.55f, 0.1f),
            // slime green
            new SummonVisualTintProfile("ooze-mephit", 1f, 1f, 1f, 0.6f, 2.8f, 0.4f),
            // crystalline bright white
            new SummonVisualTintProfile("salt-mephit", 1f, 1f, 1f, 3.0f, 3.0f, 2.9f),
            // pale grey-white vapour
            new SummonVisualTintProfile("steam-mephit", 1f, 1f, 1f, 2.0f, 2.0f, 2.1f)
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
        /// <summary>The chosen d20 result: a natural 20; the confirmation is an ordinary roll.</summary>
        internal const int CyclopsFlashOfInsightChosenRoll = 20;
        /// <summary>The stat block's hide armor, as an armor-descriptor bonus.</summary>
        internal const int CyclopsHideArmorBonus = 4;

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
        /// Attack identity (correction order): a limb grabs when it is the
        /// primary hand and the creature grabs with its bite, or one of the
        /// first grab-capable additional limbs (foreclaws, slams, bites);
        /// a rake slot - one of the last rake-limb-count limbs - never
        /// grabs, whatever weapon blueprint it shares with the foreclaws.
        /// </summary>
        internal static bool IsGrabLimb(bool isPrimaryHand, int additionalIndex,
            bool grabWithPrimaryHand, int grabAdditionalLimbCount,
            int additionalLimbCount, int rakeLimbCount)
        {
            if (isPrimaryHand) return grabWithPrimaryHand;
            if (additionalIndex < 0) return false;
            return additionalIndex < grabAdditionalLimbCount &&
                !IsRakeSlot(additionalIndex, additionalLimbCount, rakeLimbCount);
        }

        /// <summary>The last rake-limb-count additional limbs are the rake claws.</summary>
        internal static bool IsRakeSlot(int additionalIndex, int additionalLimbCount,
            int rakeLimbCount)
        {
            return rakeLimbCount > 0 && additionalIndex >= 0 &&
                additionalIndex >= additionalLimbCount - rakeLimbCount;
        }

        /// <summary>
        /// The universal grab rule: unless the stat block says otherwise, grab
        /// works only against a target of the same size or smaller. A
        /// creature's explicit exception is a positive delta.
        /// </summary>
        internal static bool IsGrabSizeAllowed(int targetSize, int holderSize,
            int maxTargetSizeDelta)
        { return targetSize <= holderSize + maxTargetSizeDelta; }

        /// <summary>
        /// Swallow whole and engulf: up to one size category smaller than the
        /// swallower unless the stat block names an absolute cap (the Giant
        /// Flytrap engulfs Medium or smaller).
        /// </summary>
        internal static bool IsSwallowSizeAllowed(int targetSize, int holderSize,
            bool absolute, int maxAbsoluteSize, int maxSizeDelta)
        {
            return absolute ? targetSize <= maxAbsoluteSize :
                targetSize <= holderSize + maxSizeDelta;
        }

        /// <summary>
        /// A grab starts only from a hit with a grab limb, by a summon with a
        /// free hold (a single-link holder holds or has swallowed no one; the
        /// flytrap has a bite free), against a live target of an allowed size
        /// that is neither held nor swallowed and is not the summon itself.
        /// </summary>
        internal static bool ShouldAttemptSummonGrab(bool isHit, bool isGrabLimb,
            bool holderFull, bool targetHeld, bool targetSwallowed,
            bool selfTarget, bool sizeAllowed)
        {
            return isHit && isGrabLimb && !holderFull && !targetHeld &&
                !targetSwallowed && !selfTarget && sizeAllowed;
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
        /// Swallow whole / engulf: a swallower that began its round with the
        /// target held (the held state has ticked at least once) uses its
        /// successful maintain check as though attempting to pin and swallows
        /// a target of an allowed size. Never on the grab itself.
        /// </summary>
        internal static bool ShouldSwallowOnMaintain(bool isSwallower, bool maintainSuccess,
            int roundsHeld, bool sizeAllowed)
        { return isSwallower && maintainSuccess && roundsHeld >= 1 && sizeAllowed; }

        /// <summary>
        /// Began its turn grappling: the owner holds exactly this target and
        /// the target's held state has ticked at least once since the grab.
        /// </summary>
        internal static bool IsHeldSinceRoundStart(bool ownerHoldsTarget, int roundsHeld)
        { return ownerHoldsTarget && roundsHeld >= 1; }

        /// <summary>
        /// Sprint 7, rebuilt: the cats' rake claws (the last two limbs of the
        /// body) attack only on a charge - Pounce makes the charge a full
        /// attack - or against the exact foe the cat holds and held when its
        /// round began. Any other attack with a rake claw never strikes: the
        /// attack sequence drops it, and a roll that still reaches the rule is
        /// an automatic, silent miss.
        /// </summary>
        internal const int CatRakeSlotCount = 2;
        internal static bool ShouldRakeApply(bool isRakeWeapon, bool isCharge,
            bool heldTargetSinceRoundStart)
        { return !isRakeWeapon || isCharge || heldTargetSinceRoundStart; }

        /// <summary>The Giant Flytrap: one held target per bite, engulf of Medium or smaller.</summary>
        internal const int GiantFlytrapBiteCount = 4;
        internal const int GiantFlytrapEngulfMaxSize = 4;
        internal const int GiantFlytrapEngulfDiceCount = 1;
        internal const int GiantFlytrapEngulfDieSides = 8;
        internal const int GiantFlytrapEngulfBonus = 7;
        internal const int GiantFlytrapEngulfAcidDiceCount = 1;
        internal const int GiantFlytrapEngulfAcidDieSides = 8;
        /// <summary>The Purple Worm swallows up to one size category smaller (Huge).</summary>
        internal const int PurpleWormSwallowSizeDelta = -1;

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
                MephitVisualTints.Any(value => !value.IsBounded || !value.HasRim))
                throw new InvalidOperationException(
                    "Sprint 5 mephit visual profile changed.");
            if (MephitVariants.Length != 6 || MephitVariants.Select(value => value.Key)
                    .Distinct(StringComparer.Ordinal).Count() != 6 ||
                MephitVariants.Any(value => !MephitKeys.Contains(value.DonorKey)) ||
                MephitSickenedRounds != 3 || MephitSpellLikeUses != 1 ||
                MephitBreathAiCooldownRounds != 4 ||
                MephitVariant("dust-mephit").SpellLikeTwo != "WindWall" ||
                MephitVariant("ice-mephit").SpellLikeTwo != "ChillMetal" ||
                MephitVariant("magma-mephit").SpellLikeOne != "Pyrotechnics" ||
                MephitVariant("magma-mephit").SpellLikeTwo != "MagmaForm")
                throw new InvalidOperationException(
                    "Sprint 5 mephit variant profile changed.");
            if (MephitSpellLikeCasterLevel != 6 || WindWallRounds != 6 || WindWallRadiusFeet != 15 ||
                WindWallSpellLevel != 3 || WindWallOtherRangedMissChance != 30 ||
                WindWallOutcome(false, true, false) != SummonWindWallOutcome.None ||
                WindWallOutcome(true, true, false) != SummonWindWallOutcome.Deflected ||
                WindWallOutcome(true, false, false) != SummonWindWallOutcome.MissChance ||
                WindWallOutcome(true, false, true) != SummonWindWallOutcome.None ||
                ChillMetalRounds != 7 || ChillMetalSpellLevel != 2 ||
                ChillMetalDice(1) != 0 || ChillMetalDice(2) != 1 || ChillMetalDice(3) != 2 ||
                ChillMetalDice(4) != 2 || ChillMetalDice(5) != 2 || ChillMetalDice(6) != 1 ||
                ChillMetalDice(7) != 0 || ChillMetalMinimalDamage(2) != 1 ||
                ChillMetalMinimalDamage(4) != 2 || ChillMetalMinimalDamage(7) != 0 ||
                ChillMetalTier(true, true) != 2 || ChillMetalTier(true, false) != 2 ||
                ChillMetalTier(false, true) != 1 || ChillMetalTier(false, false) != 0 ||
                PyrotechnicsSpellLevel != 2 || PyrotechnicsBlindDieSides != 4 ||
                PyrotechnicsBlindBonusRounds != 1 || MagmaFormRounds != 5 ||
                MagmaFormDamageReduction != 20 || MagmaFormSpeedFeet != 10 ||
                MagmaFormAiStartCooldownRounds != 3)
                throw new InvalidOperationException(
                    "Correction-order mephit role profile changed.");
            if (CheetahSprintUses != 1 || CheetahSprintRounds != 1 ||
                CheetahSprintBonusFeet != 30 || !TigerCoat.IsBounded || !CheetahCoat.IsBounded)
                throw new InvalidOperationException("Sprint 8 cat profile changed.");
            if (CatRakeSlotCount != 2 || !LionVisualTint.IsBounded ||
                ShouldRakeApply(true, false, false) || !ShouldRakeApply(true, true, false) ||
                !ShouldRakeApply(true, false, true) || !ShouldRakeApply(false, false, false) ||
                !IsRakeSlot(2, 4, 2) || IsRakeSlot(1, 4, 2) || IsGrabLimb(false, 2, true, 2, 4, 2) ||
                !IsGrabLimb(false, 1, false, 2, 4, 2) || !IsGrabLimb(true, -1, true, 0, 4, 2) ||
                IsGrabLimb(true, -1, false, 2, 4, 2) || !IsGrabSizeAllowed(4, 4, 0) ||
                IsGrabSizeAllowed(5, 4, 0) || !IsSwallowSizeAllowed(6, 7, false, 0, -1) ||
                IsSwallowSizeAllowed(7, 7, false, 0, -1) || !IsSwallowSizeAllowed(4, 6, true, 4, -1) ||
                IsSwallowSizeAllowed(5, 6, true, 4, -1) || ShouldSwallowOnMaintain(true, true, 0, true) ||
                !ShouldSwallowOnMaintain(true, true, 1, true) || IsHeldSinceRoundStart(true, 0) ||
                !IsHeldSinceRoundStart(true, 1) || GiantFlytrapBiteCount != 4 ||
                GiantFlytrapEngulfMaxSize != 4 || PurpleWormSwallowSizeDelta != -1)
                throw new InvalidOperationException("Sprint 7 rake / grapple identity profile changed.");
            if (GiantSpiderWebUses != 2 || GiantSpiderWebRangeFeet != 50 ||
                GiantSpiderWebRounds != 10 || GiantSpiderWebSpellLevel != 1 ||
                GiantSpiderWebMaxSizeDelta != 1 || !IsWebTargetSizeAllowed(5, 4, 1) ||
                IsWebTargetSizeAllowed(6, 4, 1))
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
                CyclopsFlashOfInsightChosenRoll != 20 || CyclopsHideArmorBonus != 4 ||
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
