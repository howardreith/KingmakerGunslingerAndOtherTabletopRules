using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Lillend32Prefix = "KMG.Summoning.FeyCelestial.LillendAzata.";

        // Private adult profile and native bard support. Public qualification
        // is recorded separately from these source mappings.
        private static void ConfigureLillend32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            var unit = Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit.LillendAzata");
            var type = Require<BlueprintUnitType>(bySymbol, Lillend32Prefix + "UnitType");
            type.name = InternalName(Lillend32Prefix + "UnitType");
            type.Name = LocalizationService.Create(Lillend32Prefix + "Type.Name", "Lillend Azata");
            type.Image = null; type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library,
                OutsiderClassGuid, "Lillend outsider racial HD");
            levels.Levels = 7; levels.LevelsStat = StatType.Unknown; levels.RaceStat = StatType.Constitution;
            levels.Skills = Array.Empty<StatType>(); levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>(); levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            var racial = ScriptableObject.CreateInstance<SummonGiantRacialProfile>();
            racial.OwningBlueprint = unit; racial.BaseRacialHitPoints = 38;
            racial.BaseFortitude = 2; racial.BaseReflex = 5; racial.BaseWill = 5;
            racial.PerceptionRanks = 7; racial.PersuasionRanks = 7;
            var nativeBook = BlueprintLibraryLookup.RequireExact<BlueprintSpellbook>(library,
                "bc04fc157a8801d41b877ad0d9af03dd", "captured native spontaneous bard book");
            if (!nativeBook.Spontaneous) throw new InvalidOperationException("Native bard book is not spontaneous.");
            var light = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                "5590652e1c2225c4ca30c4a699ab3649", "native Cure Light Wounds spell");
            var serious = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(library,
                "3361c5df793b4c8448756146a88026ad", "native Cure Serious Wounds spell");
            if (!nativeBook.SpellList.SpellsByLevel.Single(row => row.SpellLevel == 1).Spells.Contains(light) ||
                !nativeBook.SpellList.SpellsByLevel.Single(row => row.SpellLevel == 3).Spells.Contains(serious))
                throw new InvalidOperationException("Captured Lillend healing spells lost their native bard tiers.");
            var list = Require<BlueprintSpellList>(bySymbol, Lillend32Prefix + "SpellList");
            list.name = InternalName(Lillend32Prefix + "SpellList");
            var knownSpells = LillendKnownSpells32(library, bySymbol);
            list.SpellsByLevel = Enumerable.Range(0, 10).Select(level => new SpellLevelList {
                SpellLevel = level, Spells = (level <= 3 ? knownSpells[level] : Array.Empty<BlueprintAbility>()).ToList() }).ToArray();
            var book = Require<BlueprintSpellbook>(bySymbol, Lillend32Prefix + "Spellbook");
            CopyFields(nativeBook, book); book.name = InternalName(Lillend32Prefix + "Spellbook");
            book.ComponentsArray = Array.Empty<BlueprintComponent>(); book.SpellList = list;
            book.Spontaneous = true; book.AllSpellsKnown = false; book.CanCopyScrolls = false;
            book.CastingAttribute = StatType.Charisma; book.CasterLevelModifier = 0;
            book.Name = LocalizationService.Create(Lillend32Prefix + "Spellbook.Name", "Lillend Bard Spells");
            var caster = ScriptableObject.CreateInstance<LillendBardBook32>();
            caster.OwningBlueprint = unit; caster.Book = book;
            var nature = ScriptableObject.CreateInstance<FeyCelestialNatureKnowledge32>();
            // Printed +9 less Int+2 leaves seven trained points. Native class
            // skill status determines four or seven ranks; no unique tabletop
            // allocation is inferred from the printed total alone.
            nature.OwningBlueprint = unit; nature.TrainedContribution = 7;
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial, caster, nature };
            var load = unit.GetComponent<AddPostLoadActions>();
            if (load == null)
            {
                load = ScriptableObject.CreateInstance<AddPostLoadActions>();
                unit.ComponentsArray = unit.ComponentsArray.Concat(new BlueprintComponent[] { load }).ToArray();
            }
            load.Actions = new Kingmaker.ElementsSystem.ActionList {
                Actions = (load.Actions?.Actions ?? Array.Empty<Kingmaker.ElementsSystem.GameAction>())
                    .Concat(new Kingmaker.ElementsSystem.GameAction[] {
                        ScriptableObject.CreateInstance<LillendBookPostLoad32>() }).ToArray() };
            unit.Skills = new BlueprintUnit.UnitSkills(); unit.Race = null; unit.Type = type;
            ConfigureUnitCore(unit, "LillendAzata", "Lillend Azata", Alignment.ChaoticGood,
                Size.Large, 20, 17, 21, 14, 16, 19, 30);
            var sword = Sprint29Weapon(library, bySymbol, Lillend32Prefix + "Longsword",
                Lillend32Prefix + "LongswordType", "03d706655c07d804cb9d5a5583f9aec5",
                Size.Large, 2, DiceType.D6, 10);
            var nativeSword = BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(library,
                "03d706655c07d804cb9d5a5583f9aec5", "captured native +1 longsword");
            SetField(sword, "m_Enchantments", nativeSword.Enchantments.Cast<BlueprintWeaponEnchantment>().ToArray());
            var tail = Sprint29Weapon(library, bySymbol, Lillend32Prefix + "Tail",
                Lillend32Prefix + "TailType", LargeTailGuid, Size.Large, 2, DiceType.D6, 10);
            // Same accepted mapping as the Large giants: the native type
            // getter subtracts four from stored melee values <=10. Six plus
            // this Large owner's observed four-foot ReachRange gives ten.
            // Keep live native size/reach changes; no item or command patch.
            if (sword.AttackRange.Value != 6 || tail.AttackRange.Value != 6)
                throw new InvalidOperationException("Lillend native weapon reach contribution changed.");
            unit.Body = new BlueprintUnit.UnitBody { PrimaryHand = sword,
                AdditionalLimbs = Array.Empty<BlueprintItemWeapon>(), AdditionalSecondaryLimbs = new[] { tail },
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>() };
            var resource = Require<BlueprintAbilityResource>(bySymbol, Lillend32Prefix + "PerformanceResource");
            ConfigureNamedResource(resource, Lillend32Prefix + "PerformanceResource", Lillend32Prefix + "PerformanceResource",
                "Bardic Performance", "Twenty rounds of bardic performance for this summoning.", 20);
            var courage = ConfigureLillendCourage32(library, bySymbol, resource);
            var competence = ConfigureLillendCompetence32(library, bySymbol, courage);
            var start = ScriptableObject.CreateInstance<LillendPerformanceCommand32>();
            start.OwningBlueprint = unit; start.Courage = courage; start.Competence = competence;
            var noncombat = ScriptableObject.CreateInstance<LillendNoncombat32>();
            noncombat.OwningBlueprint = unit; noncombat.Competence = competence;
            noncombat.Recipient = Require<BlueprintBuff>(bySymbol, Lillend32Prefix + "CompetenceRecipient");
            // Buff.TickMechanics dispatches ITickEachRound at this frequency,
            // including noncombat. AddFacts owns/removes the exact buff; an
            // arbitrary component on BlueprintUnit is never a bound fact.
            var dispatcher = Require<BlueprintBuff>(bySymbol, Lillend32Prefix + "NoncombatDispatcher");
            dispatcher.name = InternalName(Lillend32Prefix + "NoncombatDispatcher");
            dispatcher.Frequency = Kingmaker.UnitLogic.Mechanics.DurationRate.Rounds;
            dispatcher.Stacking = StackingType.Replace;
            SetBuffFlags(dispatcher, harmful: false, hidden: true);
            dispatcher.ComponentsArray = new BlueprintComponent[] { noncombat };
            BlueprintUnitFactAccess.Resolve().Configure(dispatcher,
                LocalizationService.Create(Lillend32Prefix + "NoncombatDispatcher.Name", "Lillend Noncombat Performances"),
                LocalizationService.Create(Lillend32Prefix + "NoncombatDispatcher.Description",
                    "Choose a native performance through this summon’s normal actions."), null);
            var dispatchGrant = ScriptableObject.CreateInstance<AddFacts>();
            dispatchGrant.Facts = new BlueprintUnitFact[] { dispatcher };
            dispatchGrant.DoNotRestoreMissingFacts = false;
            var armor = ScriptableObject.CreateInstance<AddStatBonus>();
            armor.Stat = StatType.AC; armor.Descriptor = ModifierDescriptor.NaturalArmor; armor.Value = 8;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit; equipment.GreatWeapon = sword; equipment.Slam = tail;
            var proficiency = ScriptableObject.CreateInstance<AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { sword.Category };
            proficiency.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            var traits = Require<BlueprintFeature>(bySymbol, Lillend32Prefix + "Traits");
            traits.name = InternalName(Lillend32Prefix + "Traits"); traits.HideInUI = true;
            traits.ComponentsArray = new BlueprintComponent[] { armor, equipment, proficiency, start, dispatchGrant, AddResource(resource),
                Energy(DamageEnergyType.Cold, 10), Energy(DamageEnergyType.Fire, 10) }
                .Concat(ArchonPetrificationImmunities()).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(Lillend32Prefix + "Traits.Name", "Lillend Traits"),
                LocalizationService.Create(Lillend32Prefix + "Traits.Description",
                    "Electricity, petrification and poison immunity; cold and fire resistance10. Bard spells at caster level7 and20 owned performance rounds. Ordinary-map airborne adaptation; ground speed30 feet."), null);
            unit.AddFacts = new BlueprintUnitFact[] { Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, GoodSubtypeGuid, "good subtype"), Feature(library, ChaoticSubtypeGuid, "chaotic subtype"),
                Feature(library, ElectricityImmunityGuid, "electricity immunity"), Feature(library, PoisonImmunityGuid, "poison immunity"),
                Feature(library, ExtraplanarSubtypeGuid, "native extraplanar spell consumer"),
                Feature(library, "c1b26f97b974aec469613f968439e7bb", "accepted native legless trip immunity"),
                Feature(library, "06964d468fde1dc4aa71a92ea04d930d", "captured native Combat Casting"),
                Feature(library, IronWillGuid, "Iron Will"), Feature(library, LightningReflexesGuid, "Lightning Reflexes"),
                Feature(library, AirborneGuid, "accepted ordinary-map flight adaptation"), traits, courage, competence };
            ConfigureGrabber(library, bySymbol, "KMG.Summoning.Unit.LillendAzata",
                Lillend32Prefix + "GrabTraits", "LillendAzata", "Lillend Grab and Constrict",
                "Only this lillend's tail grabs a foe no larger than itself. A successful grab and " +
                "each later successful maintain deal 2d6+5 constrict at baseline, using live size " +
                "and Strength. The accepted summon grapple lifecycle resets active holds on reload.",
                new GrabSpec { Additional = 1,
                    Hold = Require<BlueprintBuff>(bySymbol, GrappleHoldSymbol),
                    Grappled = Require<BlueprintBuff>(bySymbol, GrappleGrappledSymbol),
                    ConstrictDice = 2, ConstrictDie = DiceType.D6, ConstrictBonus = 5,
                    LiveLillendConstrict = true });
            var healAi = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol, Lillend32Prefix + "HealAi"),
                Lillend32Prefix + "HealAi", serious, 0);
            var eligibility = Require<LillendHealingEligibility32>(bySymbol, Lillend32Prefix + "HealEligibility");
            eligibility.OwningBlueprint = unit; eligibility.Book = book; eligibility.Spell = serious;
            eligibility.Level = 3; eligibility.MinimumDamage = 18;
            healAi.TargetConsiderations = new Consideration[] { eligibility }; healAi.BaseScore = 6;
            var perform = Require<LillendCourageAi32>(bySymbol, Lillend32Prefix + "CourageAi");
            perform.name = InternalName(Lillend32Prefix + "CourageAi"); perform.OwningBlueprint = unit;
            perform.Courage = courage; perform.BaseScore = 5;
            perform.ActorConsiderations = Array.Empty<Consideration>(); perform.TargetConsiderations = Array.Empty<Consideration>();
            var nativeBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library, DumbBrainGuid, "Lillend ordinary attack fallback");
            var brain = Require<BlueprintBrain>(bySymbol, Lillend32Prefix + "Brain");
            CopyFields(nativeBrain, brain); brain.name = InternalName(Lillend32Prefix + "Brain");
            brain.ComponentsArray = (nativeBrain.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            brain.Actions = (nativeBrain.Actions ?? Array.Empty<BlueprintAiAction>())
                .Concat(new BlueprintAiAction[] { healAi, perform }).ToArray(); unit.Brain = brain;
            AddLillendSpellsAndAi32(library, bySymbol, unit, book, knownSpells);
        }

        private static BlueprintActivatableAbility ConfigureLillendCourage32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintAbilityResource resource)
        {
            var nativeToggle = BlueprintLibraryLookup.RequireExact<BlueprintActivatableAbility>(library,
                "5250fe10c377fdb49be449dfe050ba70", "captured bard Inspire Courage toggle");
            var nativeCarrier = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                "b4027a834204042409248889cc8abf67", "captured Inspire Courage carrier");
            var nativeArea = BlueprintLibraryLookup.RequireExact<BlueprintAbilityAreaEffect>(library,
                "5d4308fa344af0243b2dd3b1e500b2cc", "captured Inspire Courage ally area");
            var nativeRecipient = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                "6d6d9e06b76f5204a8b7856c78607d5d", "captured Inspire Courage effect");
            var recipient = Require<BlueprintBuff>(bySymbol, Lillend32Prefix + "CourageRecipient");
            CopyFields(nativeRecipient, recipient); recipient.name = InternalName(Lillend32Prefix + "CourageRecipient");
            ClearLillendPerformanceDeathPersistence32(recipient);
            recipient.ComponentsArray = nativeRecipient.ComponentsArray.Where(value => !(value is ContextRankConfig))
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            foreach (var bonus in recipient.GetComponents<AddContextStatBonus>()) bonus.Value = Simple(2);
            foreach (var bonus in recipient.GetComponents<SavingThrowContextBonusAgainstDescriptor>()) bonus.Value = Simple(2);
            var area = Require<BlueprintAbilityAreaEffect>(bySymbol, Lillend32Prefix + "CourageArea");
            CopyFields(nativeArea, area); area.name = InternalName(Lillend32Prefix + "CourageArea");
            area.ComponentsArray = nativeArea.ComponentsArray.Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            var delivery = area.GetComponents<AbilityAreaEffectBuff>().Single();
            delivery.Buff = recipient;
            // Same native mapping as the accepted mephit shelter: Summoned
            // faction IsAlly excludes player-faction units, even its summoner.
            // Preserve native area/effect/upkeep; only this private recipient
            // condition admits the party it supports and still excludes foes.
            var nonenemy = ScriptableObject.CreateInstance<ContextConditionIsEnemy>();
            nonenemy.Not = true;
            delivery.Condition = new Kingmaker.ElementsSystem.ConditionsChecker {
                Operation = Kingmaker.ElementsSystem.Operation.And,
                Conditions = new Kingmaker.ElementsSystem.Condition[] { nonenemy } };
            var carrier = Require<BlueprintBuff>(bySymbol, Lillend32Prefix + "CourageCarrier");
            CopyFields(nativeCarrier, carrier); carrier.name = InternalName(Lillend32Prefix + "CourageCarrier");
            ClearLillendPerformanceDeathPersistence32(carrier);
            var emanation = ScriptableObject.CreateInstance<AddAreaEffect>(); emanation.AreaEffect = area;
            carrier.ComponentsArray = new BlueprintComponent[] { emanation };
            if (carrier.StayOnDeath) throw new InvalidOperationException("Lillend performance cannot outlive its owner.");
            var toggle = Require<BlueprintActivatableAbility>(bySymbol, Lillend32Prefix + "Courage");
            CopyFields(nativeToggle, toggle); toggle.name = InternalName(Lillend32Prefix + "Courage");
            toggle.Buff = carrier; toggle.IsOnByDefault = false; toggle.ActivationType = AbilityActivationType.WithUnitCommand;
            SetField(toggle, "m_ActivateWithUnitCommand", UnitCommand.CommandType.Move);
            toggle.DeactivateIfCombatEnded = true; toggle.DeactivateIfOwnerDisabled = true;
            toggle.DeactivateIfOwnerUnconscious = true;
            var cost = (ActivatableAbilityResourceLogic)ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                nativeToggle.GetComponents<ActivatableAbilityResourceLogic>().Single());
            cost.RequiredResource = resource;
            toggle.ComponentsArray = new BlueprintComponent[] { cost };
            BlueprintUnitFactAccess.Resolve().Configure(toggle,
                LocalizationService.Create(Lillend32Prefix + "Courage.Name", "Inspire Courage"),
                LocalizationService.Create(Lillend32Prefix + "Courage.Description",
                    "Start as a move action. Allies gain+2 competence to attacks and weapon damage and+2 morale against charm and fear. Maintaining the native performance spends one owned round each round."), nativeToggle.Icon);
            return toggle;
        }

        private static void ClearLillendPerformanceDeathPersistence32(BlueprintBuff owned)
        {
            // The native bard carrier retains StayOnDeath. Clear only that
            // flag on these private summon clones; retain every other flag.
            var field = typeof(BlueprintBuff).GetField("m_Flags",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null || !field.FieldType.IsEnum)
                throw new MissingFieldException(typeof(BlueprintBuff).FullName, "m_Flags");
            int before = Convert.ToInt32(field.GetValue(owned));
            int death = Convert.ToInt32(Enum.Parse(field.FieldType, "StayOnDeath"));
            field.SetValue(owned, Enum.ToObject(field.FieldType, before & ~death));
            if (owned.StayOnDeath) throw new InvalidOperationException("Owned Lillend performance death flag did not clear.");
        }
    }
}
