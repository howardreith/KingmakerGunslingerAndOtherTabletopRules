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
using Kingmaker.ElementsSystem;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Vrock37Prefix = "KMG.Summoning.Demon.Vrock.";
        private const string NativeStunned37 =
            "09d39b38bb7c6014394b6daced9bacd3";
        // Named by the Sprint 37 installed-library audit rather than
        // guessed: run 20261102T0730259540236Z recorded both as the only
        // exact-name BlueprintAbility matches in the live library.
        private const string NativeHeroism37 =
            "5ab0d42fb68c9e34abae4921822b9d63";
        private const string NativeMirrorImage37 =
            "3e4ab69ada402d145a5e0ad3ad4b8564";
        // The native bless effect, named by the extended Sprint 37
        // library audit run 20261102T1419486445531Z.
        private const string NativeBlessBuff37 =
            "87b8c6270ea85c743afc734dfe99afee";

        /// <summary>
        /// Sprint 37. One adult, untemplated Vrock reusing the released
        /// Sprint 36 demon chassis, the released Trumpet Archon area
        /// save-or-lose shape and the released Gorgon recharge shape. The
        /// printed Vrock profile, its five primary natural attacks, its area
        /// stun and its spores are the scope; greater teleport, nested summon,
        /// telekinesis and Dance of Ruin are dispositioned in
        /// planning/SPRINT37-RULES.md.
        /// </summary>
        private static void ConfigureVrock37(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            VrockRules37Policy.Validate();
            string p = Vrock37Prefix;
            var unit = InfernalProfile36(library, symbols, "Vrock", "vrock",
                p, "Vrock", VrockRules37Policy.VrockHitDice,
                VrockRules37Policy.HitPoints,
                VrockRules37Policy.BaseFortitude,
                VrockRules37Policy.BaseReflex, VrockRules37Policy.BaseWill,
                VrockRules37Policy.SkillRanks, VrockRules37Policy.SkillRanks,
                0);
            // Kingmaker exposes one movement speed, so the effective flying
            // speed is the unit speed and the printed 30-foot land speed is
            // omitted rather than summed. See the Airborne fact below.
            ConfigureUnitCore(unit, "Vrock", "Vrock", Alignment.ChaoticEvil,
                Size.Large, 21, 15, 25, 14, 16, 16,
                VrockRules37Policy.EffectiveFlySpeedFeet);

            // Five primary natural attacks from three exact weapon identities,
            // every limb at the printed full-Strength primary role. The Large
            // 10-foot attack range matches the released Hezrou contract.
            var claw = Sprint29Weapon(library, symbols, p + "Claw",
                p + "ClawType", Claw2d4Guid, Size.Large, 2,
                DiceType.D6, VrockRules37Policy.ReachFeet);
            var bite = Sprint29Weapon(library, symbols, p + "Bite",
                p + "BiteType", NativeBite1d6Guid, Size.Large, 1,
                DiceType.D8, VrockRules37Policy.ReachFeet);
            var talon = Sprint29Weapon(library, symbols, p + "Talon",
                p + "TalonType", NativeClaw1d4Guid, Size.Large, 1,
                DiceType.D6, VrockRules37Policy.ReachFeet);
            unit.Body = NaturalBody(bite, new[] { claw, claw, talon, talon },
                Array.Empty<BlueprintItemWeapon>());

            List<BlueprintComponent> parts = InfernalDefenses36(unit,
                VrockRules37Policy.NaturalArmor,
                VrockRules37Policy.SpellResistance, false);
            var strength = ScriptableObject.CreateInstance<
                DeinonychusPrimaryTalonStrength>();
            strength.OwningBlueprint = unit;
            strength.Talon = bite;
            var ownership = ScriptableObject.CreateInstance<
                SummonGiantEquipmentOwnership>();
            ownership.OwningBlueprint = unit;
            ownership.GreatWeapon = bite;
            ownership.Slam = claw;
            ownership.Rock = talon;
            var perception = ScriptableObject.CreateInstance<AddStatBonus>();
            perception.Stat = StatType.SkillPerception;
            perception.Value = VrockRules37Policy.RacialPerceptionBonus;
            perception.Descriptor = ModifierDescriptor.Racial;
            parts.AddRange(new BlueprintComponent[] { strength, ownership,
                perception });

            BlueprintAbility screech = ConfigureVrockScreech37(library,
                symbols, unit);
            BlueprintAbility spores = ConfigureVrockSpores37(library, symbols,
                unit);
            BlueprintAbilityResource heroismResource =
                ConfigureVrockDailyResource37(symbols, p + "HeroismResource",
                    "Heroism", "One Heroism use for this summoned Vrock.");
            BlueprintAbility heroism = ConfigureVrockDailySpell37(library,
                symbols, p + "Heroism", NativeHeroism37, "Heroism", 3,
                heroismResource,
                "Once per summoning at caster level 12. The installed native spell owns its own targeting, duration and morale bonus; this vrock simply has one use of it.");
            BlueprintAbilityResource mirrorResource =
                ConfigureVrockDailyResource37(symbols,
                    p + "MirrorImageResource", "Mirror Image",
                    "One Mirror Image use for this summoned Vrock.");
            BlueprintAbility mirror = ConfigureVrockDailySpell37(library,
                symbols, p + "MirrorImage", NativeMirrorImage37,
                "Mirror Image", 2, mirrorResource,
                "Once per summoning at caster level 12. The installed native spell owns its own image count, duration and loss behaviour; this vrock simply has one use of it.");
            var parameters = ScriptableObject.CreateInstance<
                VrockRacialParameters37>();
            parameters.OwningBlueprint = unit;
            parameters.Abilities = new[] { screech, spores };
            parts.Add(parameters);
            // Two independent single-use resources: spending one never
            // touches the other.
            parts.Add(AddResource(heroismResource));
            parts.Add(AddResource(mirrorResource));

            var traits = Require<BlueprintFeature>(symbols, p + "Traits");
            BindInfernalTraits35(traits, p, "Vrock",
                "DR10/good; electricity and poison immunity; acid, cold and fire resistance 10; SR20. Five primary natural attacks — two claws, a bite and two talons — all at 10-foot reach. Stunning Screech is once per hour; spores are a free action every three rounds. Kingmaker exposes one movement speed, so the 50-foot flying speed is used with airborne navigation and the printed 30-foot land speed is omitted. Telekinesis and Dance of Ruin are not represented; greater teleport and nested summoning are excluded for every summon.",
                parts);
            unit.AddFacts = InfernalFacts36(library, traits).Concat(
                new BlueprintUnitFact[] {
                    Feature(library, AirborneGuid,
                        "established airborne navigation"),
                    Require<BlueprintFeature>(symbols, GiantCleaveFeature),
                    Feature(library, CombatReflexesGuid, "Combat Reflexes"),
                    Feature(library, ImprovedInitiativeGuid,
                        "Improved Initiative"),
                    Feature(library, LightningReflexesGuid,
                        "Lightning Reflexes"),
                    Feature(library, PowerAttackGuid, "Power Attack"),
                    screech, spores, heroism, mirror }).ToArray();

            var screechAi = Require<BlueprintAiCastSpell>(symbols,
                p + "ScreechAi");
            ConfigureMephitAiAction(screechAi, p + "ScreechAi", screech, 0);
            var screechSafety = Require<VrockScreechEligibility37>(symbols,
                p + "ScreechEligibility");
            screechSafety.SourceUnit = unit;
            screechSafety.Screech = screech;
            screechSafety.Demons = VrockDemons37(symbols);
            screechAi.ActorConsiderations = new Consideration[] {
                screechSafety };
            // The control bomb outranks the spore cloud, and both yield to the
            // native attack action when their considerations score zero.
            screechAi.BaseScore = 7;

            var sporeAi = Require<BlueprintAiCastSpell>(symbols,
                p + "SporeAi");
            ConfigureMephitAiAction(sporeAi, p + "SporeAi", spores, 0);
            var sporeSafety = Require<VrockSporeEligibility37>(symbols,
                p + "SporeEligibility");
            sporeSafety.SourceUnit = unit;
            sporeSafety.Spores = spores;
            sporeAi.ActorConsiderations = new Consideration[] { sporeSafety };
            sporeAi.BaseScore = 5;

            // Both self-buffs sit below the control bomb and the spore
            // cloud, and the released InfernalSpellAi35 refuses to cast
            // while its own effect is already standing, so a single use
            // is never spent on an already satisfied state.
            InfernalSpellAi35 heroismAi = ConfigureInfernalAi35(symbols,
                p + "HeroismAi", unit, heroism, true, false, 4);
            heroismAi.ActiveBuff = VrockAppliedBuff37(heroism);
            InfernalSpellAi35 mirrorAi = ConfigureInfernalAi35(symbols,
                p + "MirrorImageAi", unit, mirror, true, false, 4);
            mirrorAi.ActiveBuff = VrockAppliedBuff37(mirror);
            unit.Brain = InfernalBrain35(library, symbols, p, screechAi,
                sporeAi, heroismAi, mirrorAi);

            foreach (var pair in symbols.Where(pair =>
                pair.Value is BlueprintUnitFact &&
                pair.Key.StartsWith(Vrock37Prefix, StringComparison.Ordinal)))
            {
                var fact = (BlueprintUnitFact)pair.Value;
                if (fact.name != InternalName(pair.Key)) continue;
                NamePrivateSummonFactComponents(fact, pair.Key, "Sprint37");
            }
        }

        /// <summary>
        /// The buff a cloned native self-spell actually applies, read out
        /// of its own graph. The released spell AI refuses to recast while
        /// that buff stands, so reading it from the graph keeps the two
        /// in step without pinning a second asset GUID.
        /// </summary>
        private static BlueprintBuff VrockAppliedBuff37(
            BlueprintAbility ability)
        {
            BlueprintBuff[] applied = (ability.ComponentsArray ??
                    Array.Empty<BlueprintComponent>())
                .OfType<AbilityEffectRunAction>()
                .SelectMany(run => run.Actions == null ||
                    run.Actions.Actions == null ?
                    Array.Empty<GameAction>() : run.Actions.Actions)
                .OfType<ContextActionApplyBuff>()
                .Select(action => action.Buff)
                .Where(buff => buff != null).Distinct().ToArray();
            if (applied.Length != 1)
                throw new InvalidOperationException(
                    "A Vrock once-per-summoning self-spell needs exactly one applied native effect: " +
                    ability.name);
            return applied[0];
        }

        private static BlueprintAbilityResource ConfigureVrockDailyResource37(
            IDictionary<string, BlueprintScriptableObject> symbols,
            string symbol, string title, string description)
        {
            var resource = Require<BlueprintAbilityResource>(symbols, symbol);
            ConfigureNamedResource(resource, symbol, symbol, title,
                description, 1);
            return resource;
        }

        private static BlueprintAbility ConfigureVrockDailySpell37(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            string symbol, string nativeGuid, string title, int spellLevel,
            BlueprintAbilityResource resource, string description)
        {
            BlueprintAbility ability = ConfigureOwnedSpell35(library,
                symbols, symbol, nativeGuid, title, spellLevel,
                UnitCommand.CommandType.Standard,
                VrockRules37Policy.SpellLikeCasterLevel, description);
            var cost = ScriptableObject.CreateInstance<
                AbilityResourceLogic>();
            cost.RequiredResource = resource;
            cost.IsSpendResource = true;
            cost.Amount = 1;
            ability.ComponentsArray = ability.ComponentsArray.Concat(
                new BlueprintComponent[] { cost }).ToArray();
            return ability;
        }

        /// <summary>
        /// The exact registered demon identities the screech exempts, resolved
        /// from the one authoritative <see cref="SummonDemonIdentityCatalog"/>
        /// so the real effect and the AI consideration can never disagree.
        ///
        /// <para>The first pass listed four creatures by hand and silently
        /// omitted the released Succubus and Shadow Demon, which are both
        /// printed demons. Hand-maintained duplicates are what made that
        /// possible, so there is now one list and this is its only consumer.</para>
        /// </summary>
        private static BlueprintUnit[] VrockDemons37(
            IDictionary<string, BlueprintScriptableObject> symbols)
        {
            SummonDemonIdentityCatalog.Validate();
            return SummonDemonIdentityCatalog.UnitSymbols()
                .Select(symbol => Require<BlueprintUnit>(symbols, symbol))
                .ToArray();
        }

        private static BlueprintBuff ConfigureVrockMarker37(
            IDictionary<string, BlueprintScriptableObject> symbols,
            string symbol, string title, string description)
        {
            var marker = Require<BlueprintBuff>(symbols, symbol);
            marker.name = InternalName(symbol);
            marker.IsClassFeature = true;
            marker.Stacking = StackingType.Replace;
            marker.ComponentsArray = Array.Empty<BlueprintComponent>();
            SetBuffFlags(marker, false, true);
            BlueprintUnitFactAccess.Resolve().Configure(marker,
                LocalizationService.Create(symbol + ".Name", title),
                LocalizationService.Create(symbol + ".Description",
                    description), null);
            return marker;
        }

        private static BlueprintAbility ConfigureVrockScreech37(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            BlueprintUnit unit)
        {
            string p = Vrock37Prefix;
            string symbol = p + "Screech";
            var native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, NativeStunned37, "native Stunned condition");
            var condition = native.GetComponents<AddCondition>()
                .Single(value => value.Condition == UnitCondition.Stunned);
            var stunned = Require<BlueprintBuff>(symbols, p + "Stunned");
            CopyFields(native, stunned);
            stunned.name = InternalName(p + "Stunned");
            stunned.IsClassFeature = false;
            // Each failed save owns one finite victim effect. Another vrock's
            // screech neither replaces nor extends this one.
            stunned.Stacking = StackingType.Stack;
            SetBuffFlags(stunned, true);
            var descriptor = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.Stun;
            stunned.ComponentsArray = new BlueprintComponent[] {
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(condition),
                descriptor };
            stunned.ResourceAssetIds = Array.Empty<string>();

            BlueprintBuff recharge = ConfigureVrockMarker37(symbols,
                p + "ScreechRecharge", "Screech Recovering",
                "This vrock has screeched within the hour. The remaining wait is ordinary saved state: it survives a reload and belongs to this one summoned vrock.");

            var ability = Require<BlueprintAbility>(symbols, symbol);
            var around = ScriptableObject.CreateInstance<AbilityTargetsAround>();
            SetField(around, "m_Radius",
                new Feet(VrockRules37Policy.ScreechRadiusFeet));
            SetField(around, "m_TargetType",
                Kingmaker.UnitLogic.Abilities.Components.TargetType.Any);
            SetField(around, "m_IncludeDead", false);
            SetField(around, "m_Condition", new ConditionsChecker {
                Operation = Operation.And,
                Conditions = Array.Empty<Condition>() });
            SetField(around, "m_SpreadSpeed", new Feet(0));
            var resolve = ScriptableObject.CreateInstance<
                ContextActionVrockScreech37>();
            resolve.SourceUnit = unit;
            resolve.Screech = ability;
            resolve.Stunned = stunned;
            resolve.Demons = VrockDemons37(symbols);
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            run.SavingThrowType = SavingThrowType.Unknown;
            run.Actions = new ActionList {
                Actions = new GameAction[] { resolve } };
            var debit = ScriptableObject.CreateInstance<
                VrockRechargeOnCast37>();
            debit.OwningBlueprint = unit;
            debit.Ability = ability;
            debit.Marker = recharge;
            debit.Hours = VrockRules37Policy.ScreechRechargeHours;
            debit.Rounds = 0;
            var available = ScriptableObject.CreateInstance<
                AbilityCasterHasNoFacts>();
            available.Facts = new BlueprintUnitFact[] { recharge };

            ability.name = InternalName(symbol);
            ability.Parent = null;
            ability.Type = AbilityType.Supernatural;
            ability.Range = AbilityRange.Personal;
            ability.CanTargetPoint = false;
            ability.CanTargetSelf = true;
            ability.CanTargetFriends = false;
            ability.CanTargetEnemies = false;
            ability.SpellResistance = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.SetIsFullRoundAction(false);
            ability.Animation = UnitAnimationActionCastSpell
                .CastAnimationStyle.Omni;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.MaterialComponent =
                new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.ComponentsArray = new BlueprintComponent[] { around, run,
                debit, available,
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                    descriptor) };

            var icon = ExpandedSummoningProjectIcons.Require("vrock");
            var facts = BlueprintUnitFactAccess.Resolve();
            var title = LocalizationService.Create(symbol + ".Name",
                "Stunning Screech");
            facts.Configure(ability, title,
                LocalizationService.Create(symbol + ".Description",
                    "Once per hour as a standard action, emit a shrill screech. Every creature except demons within a 30-foot radius must make a Fortitude save or be stunned for 1 round. The live supernatural DC is 10 + half racial Hit Dice + Constitution modifier (DC 21 at the adult profile). Spell resistance does not apply and allies are eligible, so the AI only screeches when no ally stands in the radius. Kingmaker 2.1.7b has subtype facts for devils and daemons but none for demons, so the exemption is the exact set of this mod's own demons - Dretch, Shadow Demon, Succubus, Babau, Hezrou and Vrock. A demon belonging to the campaign or to another mod is not recognised and is not exempt."),
                icon);
            facts.Configure(stunned, title,
                LocalizationService.Create(p + "Stunned.Description",
                    "Stunned by one vrock screech for its own independently expiring round. Native stun immunity still applies, and another source's stun keeps its own ownership and duration."),
                native.Icon);
            return ability;
        }

        private static BlueprintAbility ConfigureVrockSpores37(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> symbols,
            BlueprintUnit unit)
        {
            string p = Vrock37Prefix;
            string symbol = p + "Spores";

            // The growing vines: one finite ten-round effect whose per-round
            // damage is the native new-round action, exactly as the released
            // Giant Flytrap engulf does. Extraordinary, so it carries no
            // poison descriptor and is not a spell effect.
            var vines = Require<BlueprintBuff>(symbols, p + "SporeVines");
            vines.name = InternalName(p + "SporeVines");
            vines.IsClassFeature = false;
            // Replace, not Stack. The printed entry describes one growth of
            // vines and never multiplies the per-round damage, so a second
            // exposure - from this vrock three rounds later or from another
            // one - refreshes the ten rounds instead of adding a second 1d4
            // tick. One creature therefore carries one vine effect, which is
            // also what makes the bless and disease cures unambiguous.
            vines.Stacking = StackingType.Replace;
            SetBuffFlags(vines, true);
            var growth = ScriptableObject.CreateInstance<
                ContextActionDealDamage>();
            growth.DamageType = new DamageTypeDescription {
                Type = DamageType.Direct };
            growth.Value = new ContextDiceValue {
                DiceType = DiceType.D4,
                DiceCountValue = Simple(
                    VrockRules37Policy.SporeVineDiceCount),
                BonusValue = Simple(0) };
            growth.Duration = new ContextDurationValue {
                Rate = DurationRate.Rounds, DiceType = DiceType.Zero,
                DiceCountValue = Simple(0), BonusValue = Simple(0) };
            var cadence = ScriptableObject.CreateInstance<
                AddFactContextActions>();
            cadence.Activated = new ActionList {
                Actions = Array.Empty<GameAction>() };
            cadence.Deactivated = new ActionList {
                Actions = Array.Empty<GameAction>() };
            // Growth is asked, once per round and immediately before the
            // damage, whether disease protection has arrived since exposure.
            // Neither native immunity component sweeps facts that already
            // exist, so this is the narrow hook that makes "halted" true
            // after the fact. The halted branch runs the native self-removal
            // action and deals nothing.
            var halted = ScriptableObject.CreateInstance<
                VrockSporeGrowthHalted37>();
            halted.Vines = vines;
            var stillGrowing = ScriptableObject.CreateInstance<Conditional>();
            stillGrowing.Comment =
                "KMG vrock vines: halt if disease protection arrived";
            stillGrowing.ConditionsChecker = new ConditionsChecker {
                Operation = Operation.And,
                Conditions = new Condition[] { halted } };
            stillGrowing.IfTrue = new ActionList {
                Actions = new GameAction[] {
                    ScriptableObject.CreateInstance<ContextActionRemoveSelf>() } };
            stillGrowing.IfFalse = new ActionList {
                Actions = new GameAction[] { growth } };
            cadence.NewRound = new ActionList {
                Actions = new GameAction[] { stillGrowing } };
            // The printed ability is halted by effects that remove or
            // provide immunity to disease, and the native Disease descriptor
            // is how that reaches consumers which already exist.
            //
            // Where the descriptor has to live was traced, not assumed. Every
            // native consumer reads MechanicsContext.SpellDescriptor:
            // BuffDescriptorImmunity.OnEventAboutToTrigger passes
            // RuleApplyBuff.Context, and ContextActionRemoveBuffsByDescriptor
            // - the action a disease cure runs - reads
            // buff.MaybeContext.SpellDescriptor. That value is fixed in the
            // MechanicsContext constructor as
            // (ParentContext?.SpellDescriptor) | AssociatedBlueprint
            // .GetComponent<SpellDescriptorComponent>().Descriptor.
            //
            // A Buff stores the context it was handed verbatim
            // (BuffCollection.CreateFact passes it straight to Buff..ctor,
            // which assigns m_Context; no child context is made), so the
            // vines' context is the *spores ability's* context. A descriptor
            // component on the vine blueprint alone is therefore never read
            // by any of those consumers, and native Remove Disease would not
            // strip the vines. The component is attached to the ability as
            // well, which is what the cure and the immunity actually see; it
            // stays on the vine blueprint too, so a context built for that
            // blueprint on its own still carries Disease.
            //
            // Poison is a different descriptor bit and is deliberately not
            // set, so poison immunity alone does nothing here.
            var disease = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            disease.Descriptor = SpellDescriptor.Disease;
            var abilityDisease = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            abilityDisease.Descriptor = SpellDescriptor.Disease;
            // The printed bless counter, wired to the exact native bless
            // effect. Nothing about bless itself changes.
            var blessed = ScriptableObject.CreateInstance<
                VrockSporeBlessRemoval37>();
            blessed.BlessEffect = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativeBlessBuff37,
                    "native bless effect for the printed spore counter");
            vines.ComponentsArray = new BlueprintComponent[] {
                cadence, disease, blessed };
            vines.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(vines,
                LocalizationService.Create(p + "SporeVines.Name",
                    "Vrock Spores"),
                LocalizationService.Create(p + "SporeVines.Description",
                    "Thick green vines grow from vrock spores, dealing 1d4 damage each round for 10 rounds. Three things end the growth early: casting bless on this creature, any effect that removes disease, and gaining immunity to disease, which stops the vines at the start of the next round. Poison immunity does none of those, and Dispel Magic does not touch an extraordinary growth. Ordinary damage resistance still applies to each 1d4 tick."),
                ExpandedSummoningProjectIcons.Require("vrock"));

            BlueprintBuff cooldown = ConfigureVrockMarker37(symbols,
                p + "SporeCooldown", "Spores Regrowing",
                "This vrock releases spores once every three rounds.");

            var ability = Require<BlueprintAbility>(symbols, symbol);
            var around = ScriptableObject.CreateInstance<AbilityTargetsAround>();
            SetField(around, "m_Radius",
                new Feet(VrockRules37Policy.SporeCollectionRadiusFeet));
            SetField(around, "m_TargetType",
                Kingmaker.UnitLogic.Abilities.Components.TargetType.Any);
            SetField(around, "m_IncludeDead", false);
            SetField(around, "m_Condition", new ConditionsChecker {
                Operation = Operation.And,
                Conditions = Array.Empty<Condition>() });
            SetField(around, "m_SpreadSpeed", new Feet(0));
            var impact = ScriptableObject.CreateInstance<
                ContextActionDealDamage>();
            impact.DamageType = new DamageTypeDescription {
                Type = DamageType.Direct };
            impact.Value = new ContextDiceValue {
                DiceType = DiceType.D8,
                DiceCountValue = Simple(
                    VrockRules37Policy.SporeImpactDiceCount),
                BonusValue = Simple(0) };
            impact.Duration = new ContextDurationValue {
                Rate = DurationRate.Rounds, DiceType = DiceType.Zero,
                DiceCountValue = Simple(0), BonusValue = Simple(0) };
            impact.IsAoE = true;
            impact.HalfIfSaved = false;
            var resolve = ScriptableObject.CreateInstance<
                ContextActionVrockSpores37>();
            resolve.SourceUnit = unit;
            resolve.Spores = ability;
            resolve.Vines = vines;
            // One gate in front of both effects. The native area collector
            // over-collects on purpose, so adjacency and the printed disease
            // counterplay are decided once, here, and the opening packet can
            // no longer land on a creature the vines would refuse.
            var exposed = ScriptableObject.CreateInstance<
                VrockSporeExposure37>();
            exposed.SourceUnit = unit;
            exposed.Spores = ability;
            exposed.Vines = vines;
            var gate = ScriptableObject.CreateInstance<Conditional>();
            gate.Comment =
                "KMG vrock spores: adjacent and not already disease-immune";
            gate.ConditionsChecker = new ConditionsChecker {
                Operation = Operation.And,
                Conditions = new Condition[] { exposed } };
            gate.IfTrue = new ActionList {
                Actions = new GameAction[] { impact, resolve } };
            gate.IfFalse = new ActionList {
                Actions = Array.Empty<GameAction>() };
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            run.SavingThrowType = SavingThrowType.Unknown;
            run.Actions = new ActionList {
                Actions = new GameAction[] { gate } };
            var debit = ScriptableObject.CreateInstance<
                VrockRechargeOnCast37>();
            debit.OwningBlueprint = unit;
            debit.Ability = ability;
            debit.Marker = cooldown;
            debit.Hours = 0;
            debit.Rounds = VrockRules37Policy.SporeCadenceRounds;
            var available = ScriptableObject.CreateInstance<
                AbilityCasterHasNoFacts>();
            available.Facts = new BlueprintUnitFact[] { cooldown };

            ability.name = InternalName(symbol);
            ability.Parent = null;
            ability.Type = AbilityType.Extraordinary;
            ability.Range = AbilityRange.Personal;
            ability.CanTargetPoint = false;
            ability.CanTargetSelf = true;
            ability.CanTargetFriends = false;
            ability.CanTargetEnemies = false;
            ability.SpellResistance = false;
            ability.NeedEquipWeapons = false;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            // The printed ability is a free action, which Kingmaker's command
            // type enumeration does support exactly.
            ability.ActionType = UnitCommand.CommandType.Free;
            ability.SetIsFullRoundAction(false);
            ability.Animation = UnitAnimationActionCastSpell
                .CastAnimationStyle.Immediate;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.MaterialComponent =
                new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.ComponentsArray = new BlueprintComponent[] { around, run,
                debit, available, abilityDisease };

            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", "Spores"),
                LocalizationService.Create(symbol + ".Description",
                    "A free action once every three rounds. Every adjacent creature takes 1d8 damage from the released spores, then 1d4 each round for 10 rounds as thick green vines grow. There is no save and no spell resistance, matching the printed extraordinary ability. Adjacency is measured from the actual footprints and is not the same as this vrock's 10-foot melee reach. A creature already immune to disease is not exposed at all and takes neither the 1d8 nor the vines. Once the vines are growing, casting bless destroys them, an effect that removes disease strips them, and immunity gained afterwards stops them at the start of the next round. Poison immunity does none of that, and Dispel Magic does not touch an extraordinary growth. Allies are eligible, so the AI only releases spores when no ally is adjacent."),
                ExpandedSummoningProjectIcons.Require("vrock"));
            return ability;
        }
    }
}
