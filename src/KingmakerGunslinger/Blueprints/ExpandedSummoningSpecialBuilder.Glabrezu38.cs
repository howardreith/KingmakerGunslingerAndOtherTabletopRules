using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Blueprints.Classes.Spells;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Glabrezu38Prefix =
            "KMG.Summoning.Demon.Glabrezu.";

        // Named by the Sprint 37 installed-library audit rather than guessed.
        private const string NativeStunned38 =
            "09d39b38bb7c6014394b6daced9bacd3";
        private const string NativeMirrorImage38 =
            "3e4ab69ada402d145a5e0ad3ad4b8564";
        // Already consumed by released builders at these exact identities.
        private const string NativeGreatCleave38 =
            "cc9c862ef2e03af4f89be5088851ea35";
        private const string NativePersuasive38 =
            "86d93a5891d299d4983bdc6ef3987afd";
        private const string NativeDispelMagicTarget38 =
            "143775c49ae6b7446b805d3b2e702298";
        // Named by the Sprint 38 installed-input probe
        // (run 20261103T1359412870584Z) rather than guessed.
        private const string NativeConfusion38 =
            "cf6c901fb7acc904e85c63b342e9c949";
        // Internal rather than private: the Sprint 38 persistence observer
        // reads the SAME identity to assert the printed constant sense comes
        // back after a cold load, and a second copy of the GUID in the
        // observer would be a second thing to keep in step.
        internal const string NativeTrueSeeingBuff38 =
            "09b4b69169304474296484c74aa12027";
        private const string NativeVitalStrikeFeature38 =
            "14a1fc1356df9f146900e1e42142fc9d";

        /// <summary>
        /// Sprint 38. One adult, untemplated Glabrezu at Summon Monster IX,
        /// reusing the released Sprint 36 demon chassis, the released
        /// alignment-burst builder, the released dispel selector and the
        /// released rend gate.
        ///
        /// <para>Scope is the printed profile, its five primary natural
        /// attacks, the pincer-only rend, the single-use Power Word Stun,
        /// five at-will spell-likes, printed plain Vital Strike and printed
        /// constant true seeing. Greater teleport, nested summoning and
        /// <i>wish</i> are excluded by the existing summon sanitization
        /// policy; Reverse Gravity is omitted by the owner decision of
        /// 2026-11-03; and printed self-only <i>veil</i> has no installed
        /// consumer, which the Sprint 38 input probe established rather than
        /// assumed - the library's only "Veil" abilities are Veil of Heaven
        /// and Veil of Positive Energy, entirely different spells. All of
        /// that is recorded in planning/SPRINT38-RULES.md.</para>
        /// </summary>
        private static void ConfigureGlabrezu38(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            GlabrezuRules38Policy.Validate();
            string p = Glabrezu38Prefix;
            var unit = InfernalProfile36(library, symbols, "Glabrezu",
                "glabrezu", p, "Glabrezu",
                GlabrezuRules38Policy.GlabrezuHitDice,
                GlabrezuRules38Policy.HitPoints,
                GlabrezuRules38Policy.BaseFortitude,
                GlabrezuRules38Policy.BaseReflex,
                GlabrezuRules38Policy.BaseWill,
                GlabrezuRules38Policy.SkillRanks,
                GlabrezuRules38Policy.SkillRanks, 0);
            // Huge, and one printed land speed. This creature has no printed
            // fly speed, so unlike the Vrock there is no dual-mode movement
            // omission to disclose and no Airborne fact.
            ConfigureUnitCore(unit, "Glabrezu", "Glabrezu",
                Alignment.ChaoticEvil, Size.Huge,
                GlabrezuRules38Policy.Strength,
                GlabrezuRules38Policy.Dexterity,
                GlabrezuRules38Policy.Constitution,
                GlabrezuRules38Policy.Intelligence,
                GlabrezuRules38Policy.Wisdom,
                GlabrezuRules38Policy.Charisma,
                GlabrezuRules38Policy.SpeedFeet);

            // Five primary natural attacks from three exact weapon identities
            // at the printed Huge 15-foot reach. The pincer carries the
            // printed 19-20 threat range as its own critical roll edge, which
            // is how printed Improved Critical (pincer) is delivered.
            var pincer = Sprint29Weapon(library, symbols, p + "Pincer",
                p + "PincerType", Claw2d4Guid, Size.Huge,
                GlabrezuRules38Policy.PincerDiceCount, DiceType.D8,
                SummonReachRepresentation4.StoredRawFeet,
                GlabrezuRules38Policy.PincerCriticalRollEdge);
            var claw = Sprint29Weapon(library, symbols, p + "Claw",
                p + "ClawType", NativeClaw1d4Guid, Size.Huge,
                GlabrezuRules38Policy.ClawDiceCount, DiceType.D6,
                SummonReachRepresentation4.StoredRawFeet);
            var bite = Sprint29Weapon(library, symbols, p + "Bite",
                p + "BiteType", NativeBite1d6Guid, Size.Huge,
                GlabrezuRules38Policy.BiteDiceCount, DiceType.D8,
                SummonReachRepresentation4.StoredRawFeet);
            // The limb ORDER is load-bearing, not cosmetic. The released rend
            // gate asks SummonLimbs.Classify for an Additional limb whose
            // index is below its claw count, so the two pincers have to be
            // additional limbs 0 and 1 for a two-pincer rend to be reachable
            // at all. The claws then sit at 2 and 3 and carry a different
            // weapon blueprint, so neither a claw nor the primary-hand bite
            // can stand in for a pincer on either of the gate's two tests.
            unit.Body = NaturalBody(bite,
                new[] { pincer, pincer, claw, claw },
                Array.Empty<BlueprintItemWeapon>());

            List<BlueprintComponent> parts = InfernalDefenses36(unit,
                GlabrezuRules38Policy.NaturalArmor,
                GlabrezuRules38Policy.SpellResistance, false);
            // Every printed limb carries the plain Strength bonus (+10), not
            // one and a half times it, because this creature has more than one
            // natural attack. The released correction takes the engine's
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
            ownership.Slam = pincer;
            ownership.Rock = claw;
            // Printed racial +8 Perception. The printed racial +8 Bluff is
            // deliberately NOT applied: Kingmaker folds Bluff, Diplomacy and
            // Intimidate into one Persuasion skill, so a Bluff-only racial
            // bonus cannot be expressed without also inflating the other two,
            // and the released racial profile carries no Persuasion ranks at
            // all. It is recorded as unrepresented instead.
            var perception = ScriptableObject.CreateInstance<AddStatBonus>();
            perception.Stat = StatType.SkillPerception;
            perception.Value = GlabrezuRules38Policy.RacialPerceptionBonus;
            perception.Descriptor = ModifierDescriptor.Racial;
            parts.AddRange(new BlueprintComponent[] { strength, ownership,
                perception });

            BlueprintFeature rend = ConfigureGlabrezuRend38(symbols, unit,
                pincer);
            BlueprintAbilityResource stunResource =
                ConfigureGlabrezuDailyResource38(symbols,
                    p + "PowerWordStunResource", "Power Word Stun",
                    "One Power Word Stun use for this summoned Glabrezu.");
            BlueprintAbility stun = ConfigureGlabrezuPowerWordStun38(library,
                symbols, unit, stunResource);
            BlueprintAbility chaos = ConfigureAlignmentBurst36(library,
                symbols, unit, true, p,
                GlabrezuRules38Policy.SpellLikeCasterLevel);
            BlueprintAbility unholy = ConfigureAlignmentBurst36(library,
                symbols, unit, false, p,
                GlabrezuRules38Policy.SpellLikeCasterLevel);
            // Three at-will owned spells with NO resource at all. The printed
            // entry lists them as at will, and the Sprint 37 Vrock's
            // single-use Mirror Image resource is deliberately not inherited:
            // a previous creature's frequency is not this creature's.
            BlueprintAbility confusion = ConfigureGlabrezuAtWillSpell38(
                library, symbols, p + "Confusion", NativeConfusion38,
                "Confusion", GlabrezuRules38Policy.ConfusionSpellLevel,
                "At will at caster level 14. The installed native spell owns its own targeting, duration, confused behaviour and Will save.");
            BlueprintAbility dispel = ConfigureGlabrezuAtWillSpell38(library,
                symbols, p + "DispelMagic", NativeDispelMagicTarget38,
                "Dispel Magic", GlabrezuRules38Policy.DispelMagicSpellLevel,
                "At will at caster level 14. The installed native spell owns its own dispel check and effect selection.");
            BlueprintAbility mirror = ConfigureGlabrezuAtWillSpell38(library,
                symbols, p + "MirrorImage", NativeMirrorImage38,
                "Mirror Image", GlabrezuRules38Policy.MirrorImageSpellLevel,
                "At will at caster level 14. The installed native spell owns its own image count, duration and loss behaviour.");
            parts.Add(AddResource(stunResource));

            var traits = Require<BlueprintFeature>(symbols, p + "Traits");
            BindInfernalTraits35(traits, p, "Glabrezu",
                "DR10/good; electricity and poison immunity; acid, cold and fire resistance 10; SR24. Five primary natural attacks - two pincers, two claws and a bite - all at 15-foot reach, with the pincers threatening a critical on 19-20. Two pincer hits on one creature in a round rend it; claws and the bite never do, and no round rends twice. Power Word Stun is once per summoning and has no saving throw: it stuns for longer the fewer hit points its target has, and does nothing at all to a creature with 151 or more. Chaos hammer, unholy blight, confusion, dispel magic and mirror image are at will, and true seeing is constant. Reverse gravity and veil are not represented; greater teleport, nested summoning and wish are excluded for every summon. Bluff, Diplomacy, Intimidate, Knowledge, Sense Motive, Use Magic Device, darkvision and telepathy have no supported summon consumer and are not modelled.",
                parts);
            unit.AddFacts = InfernalFacts36(library, traits).Concat(
                new BlueprintUnitFact[] {
                    // The released private ordinary Cleave delivery, plus the
                    // printed native Great Cleave feature beside it - the same
                    // pairing Sprints 28, 29 and 36 already ship.
                    Require<BlueprintFeature>(symbols, GiantCleaveFeature),
                    Feature(library, NativeGreatCleave38, "Great Cleave"),
                    Feature(library, PowerAttackGuid, "Power Attack"),
                    Feature(library, NativePersuasive38, "Persuasive"),
                    rend, stun, chaos, unholy, confusion, dispel,
                    mirror,
                    // Printed plain Vital Strike. The installed feature
                    // carries a base-attack prerequisite of 6 and grants
                    // VitalStrikeAbility; this creature's printed base attack
                    // is 12, so the prerequisite is met rather than bypassed.
                    Feature(library, NativeVitalStrikeFeature38,
                        "Vital Strike"),
                    // Printed CONSTANT true seeing. A spell-like ability has
                    // no material component, so the charter's costly-component
                    // boundary - which the summon sanitization policy enforces
                    // as ExpensiveMaterialComponent - does not reach it; the
                    // 250 gp ointment belongs to the cast spell, not to this.
                    // The native effect buff is carried directly on the unit,
                    // which is how the installed game expresses a permanent
                    // creature sense, so the real native consumer owns the
                    // behaviour and nothing here claims an unimplemented one.
                    BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                        library, NativeTrueSeeingBuff38,
                        "native constant true seeing effect")
                    }).ToArray();

            // Power Word Stun outranks the at-will bursts: it is the single
            // daily control effect and it has no saving throw. Its own
            // consideration refuses it whenever no reachable enemy is
            // affectable, so the brain falls through to the bursts and then to
            // the native attack action rather than wasting the one use.
            var stunAi = Require<BlueprintAiCastSpell>(symbols,
                p + "PowerWordStunAi");
            ConfigureMephitAiAction(stunAi, p + "PowerWordStunAi", stun, 0);
            var stunEligibility = Require<GlabrezuStunEligibility38>(symbols,
                p + "PowerWordStunEligibility");
            stunEligibility.SourceUnit = unit;
            stunEligibility.PowerWordStun = stun;
            stunAi.ActorConsiderations = new Consideration[] {
                stunEligibility };
            // The actor consideration above can only say that SOME enemy is
            // usable - the engine's own tooltip for ActorConsiderations is
            // "Considerations that only depend on deciding unit". Suitability
            // also has to sit on the candidate path, or the brain can be
            // admitted because enemy A is eligible and then select enemy B,
            // spending the one daily use on a creature the printed spell
            // cannot touch. AiAction.ScoreTarget multiplies every target
            // consideration into that candidate's own score, so a zero here
            // removes the candidate rather than merely discouraging it.
            var stunTargetEligibility =
                Require<GlabrezuStunTargetEligibility38>(symbols,
                    p + "PowerWordStunTargetEligibility");
            stunTargetEligibility.SourceUnit = unit;
            stunTargetEligibility.PowerWordStun = stun;
            stunAi.TargetConsiderations = new Consideration[] {
                stunTargetEligibility };
            stunAi.BaseScore = 8;

            InfernalSpellAi35 chaosAi = ConfigureInfernalAi35(symbols,
                p + "ChaosAi", unit, chaos, false, true, 5);
            chaosAi.AvoidFriendlyArea = true;
            chaosAi.SharedSpellCooldownRounds = 2;
            chaosAi.AreaRadiusFeet = 20;
            InfernalSpellAi35 unholyAi = ConfigureInfernalAi35(symbols,
                p + "UnholyAi", unit, unholy, false, true, 5);
            unholyAi.AvoidFriendlyArea = true;
            unholyAi.SharedSpellCooldownRounds = 2;
            unholyAi.AreaRadiusFeet = 20;
            // At will does not mean recasting every tick. The released spell
            // AI refuses while its own effect already stands on the target, so
            // Mirror Image is not re-queued over itself. This is an AI
            // selection policy only: it adds no resource and changes nothing
            // the player can command.
            InfernalSpellAi35 confusionAi = ConfigureInfernalAi35(symbols,
                p + "ConfusionAi", unit, confusion, false, true, 4);
            confusionAi.SharedSpellCooldownRounds = 2;
            InfernalSpellAi35 mirrorAi = ConfigureInfernalAi35(symbols,
                p + "MirrorImageAi", unit, mirror, true, false, 3);
            mirrorAi.ActiveBuff = VrockAppliedBuff37(mirror);
            // The released Sprint 36 dispel selector, given this creature as
            // its owner rather than reimplemented. Its correction - that an
            // effect counts as harmful to its owner from the blueprint's own
            // Harmful flag or from the caster being an enemy of the owner - is
            // reused unchanged and is not reopened here.
            var dispelAi = Require<BabauDispelAi36>(symbols, p + "DispelAi");
            ConfigureMephitAiAction(dispelAi, p + "DispelAi", dispel, 0);
            dispelAi.OwningBlueprint = unit;
            dispelAi.BaseScore = 3;

            unit.Brain = InfernalBrain35(library, symbols, p, stunAi, chaosAi,
                unholyAi, confusionAi, dispelAi, mirrorAi);

            foreach (var pair in symbols.Where(pair =>
                pair.Value is BlueprintUnitFact &&
                pair.Key.StartsWith(Glabrezu38Prefix,
                    StringComparison.Ordinal)))
            {
                var fact = (BlueprintUnitFact)pair.Value;
                if (fact.name != InternalName(pair.Key)) continue;
                NamePrivateSummonFactComponents(fact, pair.Key, "Sprint38");
            }
        }

        /// <summary>
        /// The Glabrezu's rend. The engine owns the damage exactly as it does
        /// for the released Dire Ape and Girallon: its own <c>RendFeature</c>
        /// deals the dice plus one and a half times the live Strength modifier
        /// as one event, which is the printed 2d8+15 at Strength 31 and which
        /// follows a buffed, enlarged or weakened creature.
        ///
        /// <para>Only the decision is this project's, and it is a
        /// Glabrezu-local one. The released <c>DireApeRendGate</c> could not
        /// be reused here: its shared tracker resets - emitted flag included -
        /// whenever the attack command changes, so its limit is per command
        /// and a second qualifying pair in the same round would rend again.
        /// <see cref="GlabrezuRendGate38"/> keeps the round allowance as its
        /// own state, scopes the victim bookkeeping to the round as the
        /// printed rule does, and leaves the released Dire Ape and Girallon
        /// gates untouched.</para>
        /// </summary>
        private static BlueprintFeature ConfigureGlabrezuRend38(
            IDictionary<string, BlueprintScriptableObject> symbols,
            BlueprintUnit unit, BlueprintItemWeapon pincer)
        {
            string symbol = Glabrezu38Prefix + "Rend";
            var feature = Require<BlueprintFeature>(symbols, symbol);
            var native = ScriptableObject.CreateInstance<
                Kingmaker.Designers.Mechanics.Facts.RendFeature>();
            native.RendDamage = new DiceFormula(
                GlabrezuRules38Policy.RendDiceCount, DiceType.D8);
            native.RendType = pincer.Type.DamageType.Copy();
            var gate = ScriptableObject.CreateInstance<GlabrezuRendGate38>();
            gate.OwningBlueprint = unit;
            gate.Pincer = pincer;
            gate.RendPincerCount = GlabrezuRules38Policy.RendPincerCount;
            feature.name = InternalName(symbol);
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.ComponentsArray = new BlueprintComponent[] { native, gate };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(symbol + ".Name", "Rend"),
                LocalizationService.Create(symbol + ".Description",
                    "When both of this glabrezu's pincers hit the same creature in the same round, it tears the wound open for 2d8 plus one and a half times its Strength bonus - 2d8+15 for an unmodified glabrezu. One pincer, two pincers on different creatures, and a claw or the bite standing in for a pincer do not rend. It happens at most once a round, however many attacks the round contains, and the two hits may come from different attacks in that round."),
                null);
            return feature;
        }

        private static BlueprintAbilityResource
            ConfigureGlabrezuDailyResource38(
                IDictionary<string, BlueprintScriptableObject> symbols,
                string symbol, string title, string description)
        {
            var resource = Require<BlueprintAbilityResource>(symbols, symbol);
            ConfigureNamedResource(resource, symbol, symbol, title,
                description, GlabrezuRules38Policy.PowerWordStunUses);
            return resource;
        }

        /// <summary>
        /// One at-will owned spell-like: no resource and no cooldown buff, so
        /// the only limit on it is the standard action it costs and whatever
        /// the AI's own admission decides.
        /// </summary>
        private static BlueprintAbility ConfigureGlabrezuAtWillSpell38(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            string symbol, string nativeGuid, string title, int spellLevel,
            string description)
        {
            return ConfigureOwnedSpell35(library, symbols, symbol, nativeGuid,
                title, spellLevel, UnitCommand.CommandType.Standard,
                GlabrezuRules38Policy.SpellLikeCasterLevel, description);
        }

        /// <summary>
        /// Power Word Stun: a standard action at close range against one
        /// creature, with <b>no saving throw</b>, spell resistance applying
        /// through the engine's own check, and a duration set by the
        /// recipient's current hit points.
        ///
        /// <para>The Vrock's area screech is explicitly not cloned. That is a
        /// supernatural all-creature radius spread with a Fortitude save, a
        /// one-round stun and an hour-long recharge; this is a single-target
        /// spell-like with no save, a variable duration and one daily use.
        /// They share only the native stun condition carrier.</para>
        ///
        /// <para>The mind-affecting and compulsion descriptors sit on the
        /// <b>ability</b> as well as the carrier buff. That placement is the
        /// Sprint 37 finding, not a guess: every native immunity and removal
        /// consumer reads <c>MechanicsContext.SpellDescriptor</c>, which is
        /// fixed from the associated blueprint's own descriptor component, so
        /// a descriptor on the carrier alone would be invisible to them.</para>
        /// </summary>
        private static BlueprintAbility ConfigureGlabrezuPowerWordStun38(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            BlueprintUnit unit, BlueprintAbilityResource resource)
        {
            string p = Glabrezu38Prefix;
            string symbol = p + "PowerWordStun";
            var nativeStun = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, NativeStunned38, "native Stunned condition");
            var condition = nativeStun.GetComponents<AddCondition>()
                .Single(value => value.Condition == UnitCondition.Stunned);
            var carrier = Require<BlueprintBuff>(symbols,
                p + "PowerWordStunCarrier");
            CopyFields(nativeStun, carrier);
            carrier.name = InternalName(p + "PowerWordStunCarrier");
            carrier.IsClassFeature = false;
            // One finite effect per cast. Another glabrezu's word neither
            // replaces nor extends this one.
            carrier.Stacking = StackingType.Stack;
            SetBuffFlags(carrier, true);
            var carrierDescriptor = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            carrierDescriptor.Descriptor = SpellDescriptor.Stun |
                SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion;
            carrier.ComponentsArray = new BlueprintComponent[] {
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(condition),
                carrierDescriptor };
            carrier.ResourceAssetIds = Array.Empty<string>();

            var ability = Require<BlueprintAbility>(symbols, symbol);
            var resolve = ScriptableObject.CreateInstance<
                ContextActionGlabrezuPowerWordStun38>();
            resolve.SourceUnit = unit;
            resolve.PowerWordStun = ability;
            resolve.Stunned = carrier;
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            // No saving throw at any threshold. SavingThrowType.Unknown is how
            // the released builders say "this effect asks for none", and the
            // resolution action asks for none either.
            run.SavingThrowType = SavingThrowType.Unknown;
            run.Actions = new ActionList {
                Actions = new GameAction[] { resolve } };
            var school = ScriptableObject.CreateInstance<SpellComponent>();
            school.School = SpellSchool.Enchantment;
            var descriptor = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.Stun |
                SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion;
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(
                GlabrezuRules38Policy.SpellLikeCasterLevel);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(
                GlabrezuRules38Policy.PowerWordStunSpellLevel);
            var cost = ScriptableObject.CreateInstance<AbilityResourceLogic>();
            cost.RequiredResource = resource;
            cost.IsSpendResource = true;
            cost.Amount = 1;

            ability.name = InternalName(symbol);
            ability.Parent = null;
            ability.Type = AbilityType.SpellLike;
            // The printed close range at caster level 14 is 60 feet. The
            // installed Close CATEGORY is a flat 30 that reads no caster
            // level, so using it would silently halve this creature's reach;
            // an engine limitation in one category is not authority to
            // shorten the ability. AbilityRange.Custom with an explicit
            // CustomRange is the mechanism the released scatter, web, ray,
            // Infernal34 and Breeze-Kissed abilities already use, and
            // BlueprintAbility.GetRange returns CustomRange verbatim, so one
            // distance is shared by player targeting, approach, the cast and
            // the AI rather than being restated anywhere. The number is
            // derived from this ability's own caster level, not written down.
            ability.Range = AbilityRange.Custom;
            ability.CustomRange = new Feet(
                GlabrezuRules38Policy.CloseRangeFeet(
                    GlabrezuRules38Policy.SpellLikeCasterLevel));
            ability.CanTargetEnemies = true;
            ability.CanTargetFriends = true;
            ability.CanTargetSelf = false;
            ability.CanTargetPoint = false;
            // Printed "spell resistance: yes". The flag is what the native
            // SpellImmunity path reads; the resistance ROLL is applied by
            // AbilityExecutionProcess for every unit-targeted ability either
            // way, so this is a presentation and immunity contract rather than
            // the mechanism.
            ability.SpellResistance = true;
            ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.SetIsFullRoundAction(false);
            ability.Animation = UnitAnimationActionCastSpell
                .CastAnimationStyle.Omni;
            // Reachable from the real action bar. The AI's own refusal is not
            // the player's.
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.MaterialComponent =
                new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.ComponentsArray = new BlueprintComponent[] { run, school,
                descriptor, parameters, cost };
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name",
                    "Power Word Stun"),
                LocalizationService.Create(symbol + ".Description",
                    "Once per summoning, at caster level 14: speak a word of power that stuns one creature within 60 feet, whether or not it can hear. There is no saving throw. The stun lasts 4d4 rounds against a creature at 50 or fewer current hit points, 2d4 rounds from 51 to 100, and 1d4 rounds from 101 to 150; a creature with 151 or more current hit points is unaffected entirely. Spell resistance applies, and it is a mind-affecting compulsion, so a creature immune to either is unaffected."),
                nativeStun.Icon);
            return ability;
        }
    }
}
