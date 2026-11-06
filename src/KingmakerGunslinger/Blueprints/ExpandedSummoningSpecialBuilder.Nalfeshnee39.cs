using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Nalfeshnee39Prefix =
            "KMG.Summoning.Demon.Nalfeshnee.";

        // Named by the Sprint 39 installed-library observation
        // (run 20261104T0415356825476Z) rather than guessed. Every one of
        // these was returned as an exact name match in that receipt.
        private const string NativeFeeblemind39 =
            "444eed6e26f773a40ab6e4d160c67faa";
        // Internal rather than private for the same reason native Heal is:
        // the integration review attaches this exact native condition and
        // then asks the native cure to remove it beside this project's clone,
        // and it must name the same identity the build attached to rather
        // than a second copy of it.
        internal const string NativeFeeblemindBuff39 =
            "8b3b4c225fe0fb046bfa8881c3ddad0d";
        private const string NativeUnholyAuraBuff39 =
            "9eda82a1f78558747a03c17e0e9a1a68";
        private const string NativeGreaterDispelTarget39 =
            "6d490c80598f1d34bb277735b52d52c1";
        private const string NativeCallLightning39 =
            "2a9ef0e0b5822a24d88b16673a267456";
        private const string NativeSlowSpell39 =
            "f492622e473d34747806bdb39356eb89";
        // The native daze CONDITION, already consumed at this identity by the
        // released Sprint 32 Lillend. Only the condition is reused: Sprint 32
        // also attaches a one-minute daze immunity because the Daze cantrip's
        // own printed rule says so, and Unholy Nimbus has no such clause, so
        // that marker is deliberately not copied here.
        private const string NativeDazeCondition39 =
            "9934fedff1b14994ea90205d189c8759";
        // The printed feeblemind cure that this game actually ships. Internal
        // rather than private because the spells review casts this exact
        // native spell to observe the cure, and it must name the same
        // identity the build attached to rather than a second copy of it.
        internal const string NativeHeal39 =
            "ff8f1534f66559c478448723e16b6624";

        /// <summary>
        /// Sprint 39. One adult, untemplated Nalfeshnee at Summon Monster IX,
        /// reusing the released Sprint 36 demon chassis, the released spell
        /// adapters and the released equipment and view policies.
        ///
        /// <para>Scope is the printed profile, its three primary natural
        /// attacks, the three-times-daily delayed Unholy Nimbus, four at-will
        /// spell-likes, constant true seeing and constant unholy aura, and a
        /// distinct original corpulent boar-headed winged body and icon.
        /// Greater teleport and nested summoning are excluded by the Sprint 39
        /// charter row itself and by the existing summon sanitization
        /// policy.</para>
        ///
        /// <para><b>This creature is not the Glabrezu.</b> Fourteen hit dice
        /// against twelve, caster level 12 against 14, three limbs against
        /// five, and no rend, no Great Cleave and no Vital Strike. Each of
        /// those would arrive automatically from cloning the sibling demon and
        /// each would be wrong.</para>
        /// </summary>
        private static void ConfigureNalfeshnee39(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            NalfeshneeRules39Policy.Validate();
            string p = Nalfeshnee39Prefix;
            var unit = InfernalProfile36(library, symbols, "Nalfeshnee",
                "nalfeshnee", p, "Nalfeshnee",
                NalfeshneeRules39Policy.NalfeshneeHitDice,
                NalfeshneeRules39Policy.HitPoints,
                NalfeshneeRules39Policy.GoodSaveBase,
                NalfeshneeRules39Policy.PoorSaveBase,
                NalfeshneeRules39Policy.GoodSaveBase,
                NalfeshneeRules39Policy.SkillRanks,
                NalfeshneeRules39Policy.SkillRanks, 0);

            // Huge, and ONE movement speed. Kingmaker exposes a single speed,
            // so the established representation every released flying summon
            // uses applies: the effective speed is the printed 40-foot FLY
            // speed, carried with the released Airborne navigation fact added
            // below, and the printed 30-foot land speed is recorded as omitted
            // rather than summed or substituted. Wiring the land speed here
            // would silently drop flight.
            ConfigureUnitCore(unit, "Nalfeshnee", "Nalfeshnee",
                Alignment.ChaoticEvil, Size.Huge,
                NalfeshneeRules39Policy.Strength,
                NalfeshneeRules39Policy.Dexterity,
                NalfeshneeRules39Policy.Constitution,
                NalfeshneeRules39Policy.Intelligence,
                NalfeshneeRules39Policy.Wisdom,
                NalfeshneeRules39Policy.Charisma,
                NalfeshneeRules39Policy.EffectiveFlySpeedFeet);

            // Three primary natural attacks from two exact weapon identities
            // at the printed Huge 15-foot reach. The bite carries the printed
            // 19-20 threat range as its own critical roll edge, which is how
            // printed Improved Critical (bite) is delivered: the installed
            // ImprovedCritical is a BlueprintParametrizedFeature - the player
            // feat parametrised by weapon category - so the feat is NOT also
            // attached. Exactly one mechanism, never both.
            var bite = Sprint29Weapon(library, symbols, p + "Bite",
                p + "BiteType", NativeBite1d6Guid, Size.Huge,
                NalfeshneeRules39Policy.BiteDiceCount, DiceType.D8,
                NalfeshneeRules39Policy.ReachFeet,
                NalfeshneeRules39Policy.BiteCriticalRollEdge);
            var claw = Sprint29Weapon(library, symbols, p + "Claw",
                p + "ClawType", NativeClaw1d4Guid, Size.Huge,
                NalfeshneeRules39Policy.ClawDiceCount, DiceType.D6,
                NalfeshneeRules39Policy.ReachFeet);
            // One bite in the primary hand and two claws as additional limbs.
            // There is no rend here and so no limb-ORDER contract of the kind
            // Sprint 38 needed: nothing reads the additional-limb index.
            unit.Body = NaturalBody(bite, new[] { claw, claw },
                Array.Empty<BlueprintItemWeapon>());

            // Natural armour 16 and the creature's OWN general spell
            // resistance 25, which is CR 14 + 11. The aura's conditional
            // resistance against good is a separate fact carried by the aura
            // buff; it merely prints the same number for this creature.
            List<BlueprintComponent> parts = InfernalDefenses36(unit,
                NalfeshneeRules39Policy.NaturalArmor,
                NalfeshneeRules39Policy.SpellResistance, false);

            // Every limb carries the plain Strength bonus (+11), not one and a
            // half times it, because this creature has more than one natural
            // attack. The released correction takes the engine's
            // lone-natural-weapon multiplier back off the one primary-hand
            // limb it inflates.
            var strength = ScriptableObject.CreateInstance<
                DeinonychusPrimaryTalonStrength>();
            strength.OwningBlueprint = unit;
            strength.Talon = bite;
            var ownership = ScriptableObject.CreateInstance<
                SummonGiantEquipmentOwnership>();
            ownership.OwningBlueprint = unit;
            ownership.GreatWeapon = bite;
            ownership.Slam = claw;
            ownership.Rock = claw;
            // Printed racial +8 Perception. The printed racial Bluff,
            // Diplomacy and Intimidate are deliberately not applied: Kingmaker
            // folds all three into one Persuasion skill, so a bonus to one
            // cannot be expressed without inflating the others, and the
            // released racial profile carries no Persuasion ranks. Recorded as
            // unrepresented rather than approximated.
            var perception = ScriptableObject.CreateInstance<AddStatBonus>();
            perception.Stat = StatType.SkillPerception;
            perception.Value = NalfeshneeRules39Policy.RacialPerceptionBonus;
            perception.Descriptor = ModifierDescriptor.Racial;
            parts.AddRange(new BlueprintComponent[] { strength, ownership,
                perception });

            BlueprintBuff aura = ConfigureNalfeshneeUnholyAura39(library,
                symbols);
            BlueprintAbilityResource nimbusResource =
                ConfigureNalfeshneeDailyResource39(symbols,
                    p + "UnholyNimbusResource", "Unholy Nimbus",
                    "Three Unholy Nimbus activations for this summoned Nalfeshnee.");
            BlueprintAbility nimbus = ConfigureNalfeshneeNimbus39(library,
                symbols, unit, nimbusResource);
            BlueprintAbility feeblemind = ConfigureNalfeshneeFeeblemind39(
                library, symbols, unit);
            BlueprintAbility dispel = ConfigureNalfeshneeGreaterDispel39(
                library, symbols, unit);
            BlueprintAbility lightning = ConfigureNalfeshneeAtWillSpell39(
                library, symbols, p + "CallLightning", NativeCallLightning39,
                "Call Lightning",
                NalfeshneeRules39Policy.CallLightningSpellLevel,
                "At will at caster level 12. The installed native spell owns its own bolt supply, the actions each later bolt costs, its damage and its Reflex save; no unlimited free bolts are added and no weather system is introduced.");
            BlueprintAbility slow = ConfigureNalfeshneeAtWillSpell39(library,
                symbols, p + "Slow", NativeSlowSpell39, "Slow",
                NalfeshneeRules39Policy.SlowSpellLevel,
                "At will at caster level 12. The installed native spell owns its own target selection, Will save, spell resistance, duration and action penalties.");
            // DERIVED DISTANCES, not inherited range categories. The Sprint 38
            // finding applies to these two as much as to feeblemind and
            // greater dispel, which already carry their own: a range CATEGORY
            // does not read a caster level at all, so a cloned native spell
            // reaches whatever flat distance the engine's own category
            // happens to be rather than this creature's printed reach.
            //
            // The first guarded spells run measured exactly that - Call
            // Lightning arrived as the flat Medium category and Slow as the
            // flat Close one - while this project's own policy had already
            // recorded both intended distances and the domain suite had
            // already been testing both formulas against nothing. So the
            // formulas are applied here rather than declared and unused.
            lightning.Range = AbilityRange.Custom;
            lightning.CustomRange = new Feet(
                NalfeshneeRules39Policy.LongRangeFeet(
                    NalfeshneeRules39Policy.SpellLikeCasterLevel));
            slow.Range = AbilityRange.Custom;
            slow.CustomRange = new Feet(
                NalfeshneeRules39Policy.SlowRangeFeet(
                    NalfeshneeRules39Policy.SpellLikeCasterLevel));
            AlternateSummoningAdapterBuilder.ResetComponentCaches(lightning);
            AlternateSummoningAdapterBuilder.ResetComponentCaches(slow);
            parts.Add(AddResource(nimbusResource));

            var traits = Require<BlueprintFeature>(symbols, p + "Traits");
            BindInfernalTraits35(traits, p, "Nalfeshnee",
                "DR10/good; electricity and poison immunity; acid, cold and fire resistance 10; SR25. Three primary natural attacks - one bite and two claws - all at 15-foot reach, with the bite threatening a critical on 19-20. Unholy Nimbus is three times a day: a free action that bursts one round later in a 60-foot radius, and any non-demon caught there must make a Will save or be dazed for 1d10 rounds; at most one nimbus may be pending at a time. Feeblemind, greater dispel magic, call lightning and slow are at will; true seeing and unholy aura are constant. Kingmaker exposes one movement speed, so the 40-foot flying speed is used with airborne navigation and the printed 30-foot land speed is omitted. Greater teleport and nested summoning are excluded for every summon. Bluff, Diplomacy, Intimidate, Knowledge, Sense Motive, Use Magic Device, darkvision and telepathy have no supported summon consumer and are not modelled.",
                parts);

            unit.AddFacts = InfernalFacts36(library, traits).Concat(
                new BlueprintUnitFact[] {
                    // The released private ordinary Cleave delivery and the
                    // released Sprint 29 Awesome Blow, reused at their own
                    // identities rather than reimplemented. Great Cleave and
                    // Vital Strike are NOT here: this creature's printed feat
                    // list has neither, and both would arrive from cloning the
                    // Glabrezu.
                    Require<BlueprintFeature>(symbols, GiantCleaveFeature),
                    Require<BlueprintFeature>(symbols, Awesome29Feature),
                    Feature(library, NativeImprovedBullRush29,
                        "Improved Bull Rush"),
                    Feature(library, PowerAttackGuid, "Power Attack"),
                    Feature(library, IronWillGuid, "Iron Will"),
                    Feature(library, ImprovedInitiativeGuid,
                        "Improved Initiative"),
                    // The established ordinary-map airborne navigation that
                    // carries the effective flying speed wired above.
                    Feature(library, AirborneGuid,
                        "established airborne navigation"),
                    nimbus, feeblemind, dispel, lightning, slow,
                    // Printed CONSTANT true seeing, at the identity the
                    // Sprint 38 probe named. A spell-like ability has no
                    // material component, so the charter's costly-component
                    // boundary does not reach it; the native effect buff is
                    // carried directly on the unit, which is how the installed
                    // game expresses a permanent creature sense.
                    BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                        library, NativeTrueSeeingBuff38,
                        "native constant true seeing effect"),
                    // Printed CONSTANT unholy aura, as an exact-source self
                    // effect carried the same way. Owner scoping decision of
                    // 2026-11-04: this protects the Nalfeshnee itself and is
                    // deliberately NOT extended to nearby allies. The native
                    // UnholyAura ABILITY is the spell's ordinary cast -
                    // AbilityTargetsAround, 20-foot radius, Ally - and a
                    // printed constant creature aura is not that cast. The
                    // stat block establishes only that this creature benefits
                    // from its own aura: its AC, all three saves and its CMD
                    // all include the +4s. Extending +4 deflection, +4 on
                    // every save and SR 25 against good to every party member
                    // standing near a summon would be a material power
                    // addition the entry does not grant.
                    aura
                    }).ToArray();

            // Unholy Nimbus outranks the at-will spells: it is the finite
            // daily effect and it is a free action, so taking it costs the
            // creature nothing it could otherwise spend. Its own consideration
            // refuses while one is already pending and whenever the burst
            // would catch a non-demon ally, so the brain falls through to the
            // at-will spells and then to the native attack action.
            var nimbusAi = Require<BlueprintAiCastSpell>(symbols,
                p + "UnholyNimbusAi");
            ConfigureMephitAiAction(nimbusAi, p + "UnholyNimbusAi", nimbus, 0);
            var nimbusEligibility = Require<NalfeshneeNimbusEligibility39>(
                symbols, p + "UnholyNimbusEligibility");
            nimbusEligibility.SourceUnit = unit;
            nimbusEligibility.Nimbus = nimbus;
            nimbusEligibility.Windup = Require<BlueprintBuff>(symbols,
                p + "UnholyNimbusWindup");
            nimbusEligibility.ExemptDemons = NalfeshneeDemons39(symbols);
            nimbusAi.ActorConsiderations = new Consideration[] {
                nimbusEligibility };
            // No target consideration: the ability has no target. It is a free
            // action on the creature itself whose burst is centred on the
            // creature a round later, so every decision it can make is an
            // actor decision and there is no candidate to score.
            nimbusAi.BaseScore = 9;

            // Feeblemind needs both halves of the released split. The actor
            // consideration can only say that SOME enemy is worth it - the
            // engine's own tooltip for ActorConsiderations is "Considerations
            // that only depend on deciding unit" - so without a per-candidate
            // one the brain could be admitted because enemy A casts spells and
            // then select enemy B, which casts none.
            var feebleAi = Require<BlueprintAiCastSpell>(symbols,
                p + "FeeblemindAi");
            ConfigureMephitAiAction(feebleAi, p + "FeeblemindAi", feeblemind,
                0);
            var feebleCondition = Require<BlueprintBuff>(symbols,
                p + "FeeblemindCondition");
            var feebleEligibility =
                Require<NalfeshneeFeeblemindEligibility39>(symbols,
                    p + "FeeblemindEligibility");
            feebleEligibility.SourceUnit = unit;
            feebleEligibility.Feeblemind = feeblemind;
            feebleEligibility.Condition = feebleCondition;
            feebleAi.ActorConsiderations = new Consideration[] {
                feebleEligibility };
            var feebleTargetEligibility =
                Require<NalfeshneeFeeblemindTargetEligibility39>(symbols,
                    p + "FeeblemindTargetEligibility");
            feebleTargetEligibility.SourceUnit = unit;
            feebleTargetEligibility.Feeblemind = feeblemind;
            feebleTargetEligibility.Condition = feebleCondition;
            feebleAi.TargetConsiderations = new Consideration[] {
                feebleTargetEligibility };
            feebleAi.BaseScore = 7;

            // The released Sprint 36 dispel selector, given this creature as
            // its owner rather than reimplemented. Its useful-enemy-buff
            // correction is reused unchanged and is not reopened.
            var dispelAi = Require<BabauDispelAi36>(symbols,
                p + "GreaterDispelAi");
            ConfigureMephitAiAction(dispelAi, p + "GreaterDispelAi", dispel,
                0);
            dispelAi.OwningBlueprint = unit;
            dispelAi.BaseScore = 5;

            InfernalSpellAi35 lightningAi = ConfigureInfernalAi35(symbols,
                p + "CallLightningAi", unit, lightning, false, true, 4);
            lightningAi.AvoidFriendlyArea = true;
            lightningAi.SharedSpellCooldownRounds = 2;
            InfernalSpellAi35 slowAi = ConfigureInfernalAi35(symbols,
                p + "SlowAi", unit, slow, false, true, 3);
            slowAi.SharedSpellCooldownRounds = 2;

            unit.Brain = InfernalBrain35(library, symbols, p, nimbusAi,
                feebleAi, dispelAi, lightningAi, slowAi);

            foreach (var pair in symbols.Where(pair =>
                pair.Value is BlueprintUnitFact &&
                pair.Key.StartsWith(Nalfeshnee39Prefix,
                    StringComparison.Ordinal)))
            {
                var fact = (BlueprintUnitFact)pair.Value;
                if (fact.name != InternalName(pair.Key)) continue;
                NamePrivateSummonFactComponents(fact, pair.Key, "Sprint39");
            }
        }

        /// <summary>
        /// The exact registered demon identities the nimbus spares, resolved
        /// from the one authoritative <see cref="SummonDemonIdentityCatalog"/>
        /// so this exemption and the released Vrock screech's can never
        /// disagree. The catalog now carries eight project demons, this
        /// creature included.
        /// </summary>
        private static BlueprintUnit[] NalfeshneeDemons39(
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            SummonDemonIdentityCatalog.Validate();
            return SummonDemonIdentityCatalog.UnitSymbols()
                .Select(symbol => Require<BlueprintUnit>(symbols, symbol))
                .ToArray();
        }

        private static BlueprintAbilityResource
            ConfigureNalfeshneeDailyResource39(
                IDictionary<string, BlueprintScriptableObject> symbols,
                string symbol, string title, string description)
        {
            var resource = Require<BlueprintAbilityResource>(symbols, symbol);
            ConfigureNamedResource(resource, symbol, symbol, title,
                description, NalfeshneeRules39Policy.NimbusUsesPerDay);
            return resource;
        }

        /// <summary>
        /// One at-will owned spell-like: no resource and no cooldown buff, so
        /// the only limit is the action it costs and the AI's own admission.
        /// </summary>
        private static BlueprintAbility ConfigureNalfeshneeAtWillSpell39(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            string symbol, string nativeGuid, string title, int spellLevel,
            string description)
        {
            BlueprintAbility ability = ConfigureOwnedSpell35(library,
                symbols, symbol, nativeGuid, title, spellLevel,
                UnitCommand.CommandType.Standard,
                NalfeshneeRules39Policy.SpellLikeCasterLevel, description);
            // VISIBLE ON THE ACTION BAR. The shared ability shell is created
            // hidden and auto-fill-excluded, which is right for the private
            // adapter shells it was written for and wrong for a creature's own
            // printed ability: a hidden ability is still castable by the brain,
            // so the creature would fight normally and the player would simply
            // never see a button. Every released sibling clears these two at
            // its own call site rather than relying on a donor to carry them,
            // and the Sprint 39 nimbus phase measures them on the live unit.
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            return ability;
        }

        /// <summary>
        /// The constant unholy aura, cloned from the one native consumer.
        ///
        /// <para>The Sprint 39 observation settled which blueprint that is,
        /// and it corrected the offline reading. <c>UnholyGrace</c> is a
        /// standalone feature carrying only an armour-class and a Reflex
        /// modifier; the aura's actual consumer is <c>UnholyAuraBuff</c>, and
        /// it carries every printed branch: <c>AddStatBonus</c> Deflection 4
        /// to armour class, <c>BuffAllSavesBonus</c> Resistance 4 to all three
        /// saves, <c>BuffDescriptorImmunity</c> and
        /// <c>SpellImmunityToSpellDescriptor</c> for the printed protection
        /// from mental control, <c>AddTargetAttackRollTrigger</c> whose
        /// attacker actions are conditional on a good alignment and then take
        /// a Fortitude save for the printed Strength damage, and
        /// <c>SpellResistanceAgainstAlignment</c> 25 against Good.</para>
        ///
        /// <para>So this clones that buff rather than assembling the branches
        /// by hand, which keeps the native bonus TYPES - Deflection and
        /// Resistance - and therefore the printed non-stacking behaviour
        /// against an equivalent source, and keeps the retaliation's own
        /// attribution and save. The amounts are exactly the ones the printed
        /// totals already contain: racial armour class 25 plus 4 is the
        /// printed 29, and racial saves 18/5/17 plus 4 are the printed
        /// 22/9/21.</para>
        /// </summary>
        private static BlueprintBuff ConfigureNalfeshneeUnholyAura39(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            string symbol = Nalfeshnee39Prefix + "UnholyAura";
            var native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, NativeUnholyAuraBuff39, "native unholy aura effect");
            var aura = Require<BlueprintBuff>(symbols, symbol);
            CopyFields(native, aura);
            aura.name = InternalName(symbol);
            aura.IsClassFeature = true;
            aura.Stacking = StackingType.Replace;
            aura.ComponentsArray = native.ComponentsArray
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                .ToArray();
            // A constant creature feature is not a dispelable spell effect.
            SetBuffFlags(aura, false, true);
            BlueprintUnitFactAccess.Resolve().Configure(aura,
                LocalizationService.Create(symbol + ".Name", "Unholy Aura"),
                LocalizationService.Create(symbol + ".Description",
                    "A constant unholy aura: +4 deflection to Armor Class, +4 resistance on all saving throws, protection from mental control, spell resistance 25 against good spells and spells cast by good creatures, and 1d6 Strength damage to a good creature that strikes it in melee unless that creature succeeds on a Fortitude save. It protects the nalfeshnee itself; it is not extended to nearby allies."),
                null);
            return aura;
        }

        /// <summary>
        /// Unholy Nimbus: a free action, three times a day, that starts a
        /// one-round windup and then bursts in a 60-foot radius.
        ///
        /// <para>The windup is a project-owned buff on the Nalfeshnee, because
        /// that is the only pending state in this engine that actually crosses
        /// a save: <c>Buff.m_EndTime</c> and <c>m_Context</c> are
        /// <c>[JsonProperty]</c> and <c>BuffCollection</c> re-arms expiry in
        /// <c>PostLoad</c>, whereas the engine's own
        /// <c>AbilityDeliverDelay</c> lives in a non-serialized coroutine
        /// inside <c>AbilityExecutionProcess</c> and would simply be lost.</para>
        ///
        /// <para>The daze the burst applies is the native condition at the
        /// identity Sprint 32 already consumes. Sprint 32's one-minute daze
        /// IMMUNITY marker is deliberately not reused: it exists because the
        /// Daze cantrip's printed rule grants it, and Unholy Nimbus has no
        /// such clause, so copying it would invent a restriction this creature
        /// does not have.</para>
        /// </summary>
        private static BlueprintAbility ConfigureNalfeshneeNimbus39(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            BlueprintUnit unit, BlueprintAbilityResource resource)
        {
            string p = Nalfeshnee39Prefix;
            string symbol = p + "UnholyNimbus";

            // The condition the burst applies: the native daze, cloned so the
            // project owns its lifetime, with the caster-missing cleanup the
            // released fey control uses. A dazed victim is a finite timed
            // condition, so it legitimately ends with its source.
            var nativeDaze = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, NativeDazeCondition39, "native Daze condition");
            var daze = Require<BlueprintBuff>(symbols, p + "UnholyNimbusDaze");
            CopyFields(nativeDaze, daze);
            daze.name = InternalName(p + "UnholyNimbusDaze");
            daze.IsClassFeature = false;
            // One finite effect per burst. A second nalfeshnee's nimbus
            // neither replaces nor extends this one.
            daze.Stacking = StackingType.Stack;
            daze.ComponentsArray = nativeDaze.ComponentsArray
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                .Concat(new BlueprintComponent[] {
                    ScriptableObject.CreateInstance<
                        RemoveBuffIfCasterIsMissing>() }).ToArray();
            SetBuffFlags(daze, true);
            BlueprintUnitFactAccess.Resolve().Configure(daze,
                LocalizationService.Create(p + "UnholyNimbusDaze.Name",
                    "Dazed"),
                LocalizationService.Create(p + "UnholyNimbusDaze.Description",
                    "Visions of madness hound this creature: it takes no actions."),
                null);

            // The pending windup. Its duration IS the deadline, and that is
            // what the save carries.
            var windup = Require<BlueprintBuff>(symbols,
                p + "UnholyNimbusWindup");
            windup.name = InternalName(p + "UnholyNimbusWindup");
            windup.IsClassFeature = true;
            // Exactly one may be pending, so a second application must never
            // silently stack a second deadline. The activating action refuses
            // first; this is the belt beside that brace.
            windup.Stacking = StackingType.Replace;
            var pending = ScriptableObject.CreateInstance<
                NalfeshneeNimbusWindup39>();
            pending.OwningBlueprint = unit;
            pending.Daze = daze;
            pending.ExemptDemons = NalfeshneeDemons39(symbols);
            windup.ComponentsArray = new BlueprintComponent[] { pending };
            SetBuffFlags(windup, false);
            BlueprintUnitFactAccess.Resolve().Configure(windup,
                LocalizationService.Create(p + "UnholyNimbusWindup.Name",
                    "Unholy Nimbus"),
                LocalizationService.Create(
                    p + "UnholyNimbusWindup.Description",
                    "Nauseating beams of writhing colour play around this creature. One round later the light bursts in a 60-foot radius."),
                null);

            // The ability itself: a free action with no target, three uses a
            // day, carrying the Charisma-derived supernatural difficulty.
            var ability = Require<BlueprintAbility>(symbols, symbol);
            ability.name = InternalName(symbol);
            // SUPERNATURAL, not spell-like. The printed entry marks Unholy
            // Nimbus (Su), and the two classifications are not
            // interchangeable: a supernatural ability is not a spell, so it
            // is not subject to spell resistance, does not provoke as a spell
            // and is not counterspelled, and its difficulty class derives
            // from half the hit dice rather than a spell level. Declaring it
            // SpellLike would have quietly given it the wrong half of all of
            // that while still looking correct in the action bar.
            ability.Type = AbilityType.Supernatural;
            // VISIBLE ON THE ACTION BAR. The shared ability shell is created
            // hidden and auto-fill-excluded, which is right for the private
            // adapter shells it was written for and wrong for a creature's own
            // printed ability: a hidden ability is still castable by the brain,
            // so the creature would fight normally and the player would simply
            // never see a button. Every released sibling clears these two at
            // its own call site rather than relying on a donor to carry them,
            // and the Sprint 39 nimbus phase measures them on the live unit.
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.Range = AbilityRange.Personal;
            ability.CanTargetSelf = true;
            ability.CanTargetEnemies = false;
            ability.CanTargetFriends = false;
            ability.CanTargetPoint = false;
            ability.EffectOnAlly = AbilityEffectOnUnit.None;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.ActionType = UnitCommand.CommandType.Free;
            ability.SetIsFullRoundAction(false);
            ability.Animation =
                Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell
                    .CastAnimationStyle.Omni;

            var activate = ScriptableObject.CreateInstance<
                ContextActionNalfeshneeNimbus39>();
            activate.SourceUnit = unit;
            activate.Nimbus = ability;
            activate.Windup = windup;
            pending.Nimbus = ability;
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            run.Actions = new ActionList {
                Actions = new GameAction[] { activate } };

            // Supernatural, so the difficulty is ten plus half the hit dice
            // plus Charisma - a DIFFERENT formula from the spell-like one, and
            // the project's own calculator supplies it so the live Charisma
            // drives it rather than a pinned 22.
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel =
                Simple(NalfeshneeRules39Policy.SpellLikeCasterLevel);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(
                NalfeshneeRules39Policy.NalfeshneeHitDice / 2);

            var cost = ScriptableObject.CreateInstance<
                AbilityResourceLogic>();
            cost.RequiredResource = resource;
            cost.IsSpendResource = true;
            cost.Amount = 1;

            // THE ONE-PENDING RULE, enforced where the engine actually asks
            // before it spends. Traced rather than assumed:
            // AbilityData.IsAvailable is IsAvailableInSpellbook &&
            // IsAvailableForCast; IsAvailableForCast walks
            // BlueprintAbility.CasterCheckers calling
            // IAbilityCasterChecker.CorrectCaster; and UnitUseAbility
            // consults IsAvailable BEFORE it calls AbilityData.Spend, which
            // is what reaches AbilityResourceLogic.Spend. So a caster checker
            // refuses the second activation on the real action-bar command
            // path with nothing spent - rather than the effect action
            // returning after the use has already gone and a refund being
            // needed to manufacture the right answer.
            //
            // AbilityCasterHasNoFacts.CorrectCaster returns false as soon as
            // the caster HAS any listed fact, so listing the windup makes the
            // ability unavailable exactly while one is pending.
            var pendingGate = ScriptableObject.CreateInstance<
                AbilityCasterHasNoFacts>();
            pendingGate.Facts = new BlueprintUnitFact[] { windup };

            ability.ComponentsArray = new BlueprintComponent[] {
                run, parameters, cost, pendingGate };
            AlternateSummoningAdapterBuilder.ResetComponentCaches(ability);
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", "Unholy Nimbus"),
                LocalizationService.Create(symbol + ".Description",
                    "Three times per day as a free action, create a nimbus of unholy light. One round later it bursts in a 60-foot radius; any non-demon caught in the area must succeed on a Will save or be dazed for 1d10 rounds. At most one nimbus may be pending at a time: activating again while one is winding up is refused and costs no use. The burst catches allies as readily as enemies."),
                null);
            return ability;
        }

        /// <summary>
        /// Feeblemind, from the native graph, with one repair.
        ///
        /// <para><b>What the native graph already gets right</b> and is kept:
        /// a Will saving throw, and a <c>ConditionalDCIncrease</c> of 4 gated
        /// on <c>ContextConditionTargetIsArcaneCaster</c>. That is the printed
        /// arcane penalty, implemented as a difficulty increase rather than a
        /// save penalty, and the engine owns the judgement of what an arcane
        /// caster is - so nothing is inferred from a high Intelligence, a
        /// class label or the presence of spell-like abilities. Only one of
        /// the two is applied: there is no separate -4 anywhere here.</para>
        ///
        /// <para><b>What it gets wrong, and the one repair.</b>
        /// <c>FeeblemindBuff</c> carries only Intelligence -99 and
        /// Charisma -99, and it is absent from the ten shipped carriers of
        /// <c>ForbidSpellCasting</c>. The printed spell says the subject
        /// cannot cast spells; two reduced ability scores do not deliver that
        /// for a Wisdom-based divine caster, which loses nothing it needs. So
        /// the project's clone adds <c>ForbidSpellCasting</c>. The native buff
        /// is not modified.</para>
        ///
        /// <para><c>ForbidMagicItems</c> is deliberately left FALSE. The
        /// component offers the flag, but the printed spell prohibits
        /// spellcasting and coherent communication - it does not state a
        /// blanket ban on every magic item, and the native
        /// spell-completion and spell-trigger restrictions that do apply are
        /// already the engine's own. Setting it because it is available would
        /// be a rules change, and classifying items would be a subsystem this
        /// creature does not need.</para>
        /// </summary>
        private static BlueprintAbility ConfigureNalfeshneeFeeblemind39(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            BlueprintUnit unit)
        {
            string p = Nalfeshnee39Prefix;
            string symbol = p + "Feeblemind";
            var nativeCondition =
                BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                    NativeFeeblemindBuff39, "native Feeblemind condition");
            var condition = Require<BlueprintBuff>(symbols,
                p + "FeeblemindCondition");
            CopyFields(nativeCondition, condition);
            condition.name = InternalName(p + "FeeblemindCondition");
            condition.IsClassFeature = false;
            condition.Stacking = StackingType.Replace;
            var forbid = ScriptableObject.CreateInstance<ForbidSpellCasting>();
            // Left false deliberately; see the summary above.
            forbid.ForbidMagicItems = false;
            condition.ComponentsArray = nativeCondition.ComponentsArray
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                .Concat(new BlueprintComponent[] { forbid }).ToArray();
            // The printed spell is instantaneous with a LASTING result, cured
            // only by one of its named cures. So this carries no duration and
            // no RemoveBuffIfCasterIsMissing: a correctly resolved result is
            // not erased because the summon that caused it went away, and it
            // does not expire on an invented timer either. Attribution is kept
            // stable through the released victim-effect context rather than by
            // holding a live reference to a destroyed caster.
            SetBuffFlags(condition, true);
            BlueprintUnitFactAccess.Resolve().Configure(condition,
                LocalizationService.Create(p + "FeeblemindCondition.Name",
                    "Feeblemind"),
                LocalizationService.Create(
                    p + "FeeblemindCondition.Description",
                    "This creature's Intelligence and Charisma have collapsed. It cannot cast spells. The effect lasts until a heal spell removes it."),
                null);

            // The real supported cure. Native Heal removes feeblemind by
            // IDENTITY - a ContextActionRemoveBuff naming
            // 8b3b4c225fe0fb046bfa8881c3ddad0d - not by descriptor, so a clone
            // with a new identity would simply never be cured. One additional
            // removal action naming this exact clone is appended to Heal's own
            // action list. That is exact-clone support for the printed cure,
            // not a rewrite: no existing action is altered or removed, every
            // other condition Heal cures is untouched, and no other cure is
            // introduced.
            GrantNalfeshneeHealCure39(library, condition);

            var ability = Require<BlueprintAbility>(symbols, symbol);
            CloneMephitSpell(library, ability, NativeFeeblemind39,
                "native Feeblemind");
            ability.name = InternalName(symbol);
            ability.Parent = null;
            ability.Type = AbilityType.SpellLike;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.SetIsFullRoundAction(false);
            // VISIBLE ON THE ACTION BAR. The shared ability shell is created
            // hidden and auto-fill-excluded, which is right for the private
            // adapter shells it was written for and wrong for a creature's own
            // printed ability: a hidden ability is still castable by the brain,
            // so the creature would fight normally and the player would simply
            // never see a button. Every released sibling clears these two at
            // its own call site rather than relying on a donor to carry them,
            // and the Sprint 39 nimbus phase measures them on the live unit.
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            // The printed medium range at this creature's own caster level,
            // 100 + 10 per level. A fixed range CATEGORY does not read a
            // caster level at all - the Sprint 38 finding about Close - so the
            // distance is derived and shipped explicitly.
            ability.Range = AbilityRange.Custom;
            ability.CustomRange = new Feet(
                NalfeshneeRules39Policy.MediumRangeFeet(
                    NalfeshneeRules39Policy.SpellLikeCasterLevel));

            // Redirect the native failed-save branch at the project's clone,
            // leaving the save, the arcane difficulty increase and spell
            // resistance exactly as the native graph has them.
            var run = ability.GetComponents<AbilityEffectRunAction>().Single();
            var saved = run.Actions.Actions.OfType<ContextActionSavingThrow>()
                .Single();
            var conditional = saved.Actions.Actions
                .OfType<ContextActionConditionalSaved>().Single();
            var apply = conditional.Failed.Actions
                .OfType<ContextActionApplyBuff>().Single();
            if (saved.Type != SavingThrowType.Will ||
                !ReferenceEquals(apply.Buff, nativeCondition))
                throw new InvalidOperationException(
                    "Native Feeblemind failed-save effect contract changed.");
            apply.Buff = condition;
            apply.Permanent = true;
            apply.UseDurationSeconds = false;
            apply.AsChild = false;

            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel =
                Simple(NalfeshneeRules39Policy.SpellLikeCasterLevel);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel =
                Simple(NalfeshneeRules39Policy.FeeblemindSpellLevel);
            ability.ComponentsArray = ability.ComponentsArray
                .Where(value => !(value is SpellListComponent) &&
                    !(value is ContextCalculateAbilityParams))
                .Concat(new BlueprintComponent[] { parameters }).ToArray();
            AlternateSummoningAdapterBuilder.ResetComponentCaches(ability);
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", "Feeblemind"),
                LocalizationService.Create(symbol + ".Description",
                    "At will at caster level 12. The target's Intelligence and Charisma collapse and it can no longer cast spells. A Will save negates, and an arcane caster takes a penalty on that save; spell resistance applies. The effect lasts until a heal spell removes it."),
                ability.Icon);
            return ability;
        }

        /// <summary>
        /// Teaches the native <i>heal</i> spell to cure this project's own
        /// feeblemind clone, by inserting one removal action <b>beside the
        /// native one</b> - in the same action list, under the same
        /// condition.
        ///
        /// <para><b>Why beside, and not appended at the top.</b> Heal's
        /// effect actions are not a flat list. Its one top-level action is a
        /// <c>Conditional</c> whose checker is
        /// <c>ContextConditionHasNegativeEnergyAffinity</c> with
        /// <c>Not = true</c>, so its <c>IfTrue</c> branch is "the target is
        /// not undead" and every cure Heal performs - daze, stunned,
        /// blindness, dazzled, insanity and feeblemind - lives inside that
        /// branch, while the <c>IfFalse</c> branch harms undead instead.
        /// Appending to the top-level list would therefore have cured this
        /// clone <i>unconditionally</i>, including on a target the native
        /// feeblemind cure deliberately skips. Locating the native removal
        /// and inserting next to it inherits whatever condition encloses it,
        /// now and if Owlcat ever moves it.</para>
        ///
        /// <para>Narrow on purpose: no existing action is altered or removed,
        /// every other condition Heal cures is untouched, the insertion is
        /// idempotent, and the array is replaced rather than any shared
        /// element being mutated. If the native removal cannot be found the
        /// contract has changed and this throws rather than silently curing
        /// in the wrong place.</para>
        /// </summary>
        private static void GrantNalfeshneeHealCure39(
            LibraryScriptableObject library, BlueprintBuff condition)
        {
            var heal = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, NativeHeal39, "native Heal");
            var nativeCondition =
                BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                    NativeFeeblemindBuff39, "native Feeblemind condition");
            bool placed = false;
            foreach (AbilityEffectRunAction run in
                heal.GetComponents<AbilityEffectRunAction>())
                if (PlaceNalfeshneeHealCure39(run.Actions, nativeCondition,
                    condition)) placed = true;
            if (!placed) throw new InvalidOperationException(
                "Native Heal no longer removes the native Feeblemind condition; the Sprint 39 cure has nowhere faithful to attach.");
            AlternateSummoningAdapterBuilder.ResetComponentCaches(heal);
        }

        /// <summary>
        /// Finds the action list that removes <paramref name="nativeBuff"/>
        /// and puts a removal of <paramref name="clone"/> beside it,
        /// descending through every nested action list rather than assuming
        /// a shape. Returns whether it was placed; already-present counts as
        /// placed, which is what makes a second build a no-op.
        /// </summary>
        private static bool PlaceNalfeshneeHealCure39(ActionList actions,
            BlueprintBuff nativeBuff, BlueprintBuff clone)
        {
            if (actions == null || actions.Actions == null) return false;
            GameAction[] current = actions.Actions;
            if (current.OfType<ContextActionRemoveBuff>()
                .Any(value => ReferenceEquals(value.Buff, clone))) return true;
            int index = Array.FindIndex(current, value => {
                var remove = value as ContextActionRemoveBuff;
                return remove != null &&
                    ReferenceEquals(remove.Buff, nativeBuff); });
            if (index >= 0)
            {
                var native = (ContextActionRemoveBuff)current[index];
                var mirror = ScriptableObject.CreateInstance<
                    ContextActionRemoveBuff>();
                mirror.Buff = clone;
                // The same recipient the native cure uses, read from it
                // rather than assumed, so the two always agree.
                mirror.ToCaster = native.ToCaster;
                var updated = new GameAction[current.Length + 1];
                Array.Copy(current, updated, index + 1);
                updated[index + 1] = mirror;
                Array.Copy(current, index + 1, updated, index + 2,
                    current.Length - index - 1);
                actions.Actions = updated;
                return true;
            }
            bool placed = false;
            foreach (GameAction action in current)
            {
                if (action == null) continue;
                foreach (ActionList nested in NalfeshneeNestedActions39(action))
                    if (PlaceNalfeshneeHealCure39(nested, nativeBuff, clone))
                        placed = true;
            }
            return placed;
        }

        /// <summary>
        /// Every action list an action carries, found by reflection so that
        /// conditionals, saved-throw branches and any other container the
        /// installed game uses are all descended into without this code
        /// naming them.
        /// </summary>
        private static IEnumerable<ActionList> NalfeshneeNestedActions39(
            GameAction action)
        {
            return action.GetType()
                .GetFields(System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic)
                .Where(field => field.FieldType == typeof(ActionList))
                .Select(field => (ActionList)field.GetValue(action))
                .Where(value => value != null);
        }

        /// <summary>
        /// Greater Dispel Magic, targeted, capped at the printed one removal
        /// per four caster levels.
        ///
        /// <para>The native graph is cloned for its targeting, its school and
        /// its presentation, and then its single
        /// <c>ContextActionDispelMagic</c> is replaced by this creature's own
        /// action. Both reasons are measured rather than assumed. First, the
        /// shipped <c>DispelMagicGreaterTarget</c> sets
        /// <c>m_StopAfterFirstRemoved</c> false, which removes EVERY eligible
        /// effect, and the component has no success-count field, so the
        /// printed cap is unexpressible. Second, the printed targeted form
        /// applies ONE check result to the ordered effects, and
        /// <c>RuleDispelMagic.OnTrigger</c> unconditionally assigns
        /// <c>CheckRoll = Dice.D20</c> with no guard, so the native rule
        /// cannot be told to reuse a roll.</para>
        ///
        /// <para>The replacement rolls once, walks the same descending
        /// caster-level order with the same eligibility, and performs each
        /// removal through the native rule's own no-check branch. Only this
        /// creature's ability is affected; no released dispeller and no native
        /// blueprint is altered, and the area variant is untouched.</para>
        /// </summary>
        private static BlueprintAbility ConfigureNalfeshneeGreaterDispel39(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            BlueprintUnit unit)
        {
            string symbol = Nalfeshnee39Prefix + "GreaterDispelMagic";
            var ability = ConfigureNalfeshneeAtWillSpell39(library, symbols,
                symbol, NativeGreaterDispelTarget39, "Greater Dispel Magic",
                NalfeshneeRules39Policy.GreaterDispelMagicSpellLevel,
                "At will at caster level 12. One dispel check is made and applied to the target's ongoing spell effects from the highest caster level down, removing up to three of them. A failed effect is not retried, and the check is not rerolled per effect.");
            var run = ability.GetComponents<AbilityEffectRunAction>().Single();
            if (!run.Actions.Actions.OfType<ContextActionDispelMagic>().Any())
                throw new InvalidOperationException(
                    "Native greater dispel target contract changed.");
            var capped = ScriptableObject.CreateInstance<
                ContextActionNalfeshneeGreaterDispel39>();
            capped.SourceUnit = unit;
            capped.GreaterDispel = ability;
            run.Actions = new ActionList {
                Actions = run.Actions.Actions
                    .Where(value => !(value is ContextActionDispelMagic))
                    .Concat(new GameAction[] { capped }).ToArray() };
            ability.Range = AbilityRange.Custom;
            ability.CustomRange = new Feet(
                NalfeshneeRules39Policy.MediumRangeFeet(
                    NalfeshneeRules39Policy.SpellLikeCasterLevel));
            AlternateSummoningAdapterBuilder.ResetComponentCaches(ability);
            return ability;
        }
    }
}
