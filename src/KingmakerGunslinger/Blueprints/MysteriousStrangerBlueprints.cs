using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Archetypes;
using KingmakerGunslinger.Grit;
using Kingmaker.UnitLogic;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class MysteriousStrangerBlueprintSet
    {
        internal MysteriousStrangerBlueprintSet(BlueprintArchetype archetype,
            BlueprintFeature grit, BlueprintFeature focusedAim, BlueprintBuff focusedBuff,
            BlueprintFeature[] lucky, BlueprintFeature fortune, BlueprintBuff fortuneBuff,
            BlueprintFeature clipping, BlueprintBuff clippingBuff,
            BlueprintAbilityResource fortuneResource)
        { Archetype=archetype; Grit=grit; FocusedAim=focusedAim; FocusedAimBuff=focusedBuff;
          Lucky=lucky; StrangersFortune=fortune; FortuneBuff=fortuneBuff;
          ClippingShot=clipping; ClippingShotBuff=clippingBuff;
          FortuneResource=fortuneResource; }
        internal BlueprintArchetype Archetype { get; private set; }
        internal BlueprintFeature Grit { get; private set; }
        internal BlueprintFeature FocusedAim { get; private set; }
        internal BlueprintBuff FocusedAimBuff { get; private set; }
        internal BlueprintFeature[] Lucky { get; private set; }
        internal BlueprintFeature StrangersFortune { get; private set; }
        internal BlueprintBuff FortuneBuff { get; private set; }
        internal BlueprintFeature ClippingShot { get; private set; }
        internal BlueprintBuff ClippingShotBuff { get; private set; }
        internal BlueprintAbilityResource FortuneResource { get; private set; }
        internal int Count { get { return 17; } }
        internal bool TryIgnoreMisfire(UnitDescriptor owner)
        {
            if (owner == null || FortuneBuff == null) return false;
            var fact = owner.Buffs.RawFacts.FirstOrDefault(value =>
                ReferenceEquals(value.Blueprint, FortuneBuff));
            if (fact == null) return false;
            if (FortuneResource == null ||
                owner.Resources.GetResourceAmount(FortuneResource) < 1) return false;
            owner.Resources.Spend(FortuneResource, 1);
            owner.Buffs.RemoveFact(fact);
            return true;
        }
    }

    internal static class MysteriousStrangerBlueprints
    {
        internal const string ArchetypeSymbol="KMG.Archetypes.MysteriousStranger";
        internal static MysteriousStrangerBlueprintSet Register(BlueprintRegistry registry,
            BlueprintCharacterClass cls, GritBlueprintSet baseGrit,
            BlueprintFeature quickClear, BlueprintFeature[] nimble,
            BlueprintFeatureSelection gunTraining, BlueprintFeature bleedingWound)
        {
            var grit=registry.Register<BlueprintFeature>("KMG.Archetypes.MysteriousStrangerGrit",
                ()=>GritBlueprints.CreateAlternateFeature(baseGrit.Resource, cls,
                    baseGrit.InitializedMarker, StatType.Charisma, "Mysterious Stranger Grit"));
            BlueprintBuff focusedBuff=registry.Register<BlueprintBuff>("KMG.Archetypes.FocusedAimBuff",
                ()=>CreateBuff("FocusedAim", "Focused Aim", "Until the end of your turn, every firearm damage roll gains your Charisma modifier (minimum +1). This does not modify attack rolls. At 7th level, Dead Shot multiplies this bonus by its number of hits.", new FocusedAimDamage()));
            BlueprintAbility focusedAbility=registry.Register<BlueprintAbility>("KMG.Archetypes.FocusedAimAbility",
                ()=>CreateArmAbility("Focused Aim", focusedBuff, baseGrit.Resource, 1, true));
            BlueprintFeature focused=registry.Register<BlueprintFeature>("KMG.Archetypes.FocusedAimFeature",
                ()=>CreateGrant("Focused Aim", "Spend 1 grit as a swift action to add your Charisma modifier (minimum +1) to every firearm damage roll until the end of your turn.", focusedAbility));
            var lucky=new BlueprintFeature[5];
            for(int i=0;i<5;i++){int rank=i+1; lucky[i]=registry.Register<BlueprintFeature>(
                "KMG.Archetypes.Lucky"+rank, ()=>CreateLucky(rank));}
            BlueprintAbilityResource fortuneResource=registry.Register<BlueprintAbilityResource>(
                "KMG.Archetypes.StrangersFortuneResource", ()=>GritBlueprints.CreateAttributeResource("Stranger's Fortune", StatType.Charisma));
            BlueprintBuff fortuneBuff=registry.Register<BlueprintBuff>("KMG.Archetypes.StrangersFortuneArmed",
                ()=>CreateBuff("StrangersFortune", "Stranger's Fortune Armed", "Your next firearm misfire this round is ignored, consuming one daily use only when the misfire occurs.", null));
            BlueprintAbility fortuneAbility=registry.Register<BlueprintAbility>("KMG.Archetypes.StrangersFortuneAbility",
                ()=>CreateArmAbility("Stranger's Fortune", fortuneBuff, fortuneResource, 1, false));
            BlueprintFeature fortune=registry.Register<BlueprintFeature>("KMG.Archetypes.StrangersFortuneFeature",
                ()=>CreateResourceGrant("Stranger's Fortune", "A number of times per day equal to your Charisma bonus, arm this free action to ignore your next firearm misfire.", fortuneAbility, fortuneResource));
            BlueprintBuff clippingBuff=registry.Register<BlueprintBuff>("KMG.Archetypes.ClippingShotArmed",
                ()=>CreateBuff("ClippingShot", "Clipping Shot Armed", "If your next qualifying firearm attack this round misses, spend exactly 1 grit to deal half normal damage. Dead Shot is excluded.", new ClippingShotAttackHandler{Grit=baseGrit.Resource}));
            BlueprintAbility clippingAbility=registry.Register<BlueprintAbility>("KMG.Archetypes.ClippingShotAbility",
                ()=>CreateArmAbility("Clipping Shot", clippingBuff, baseGrit.Resource, 1, false));
            BlueprintFeature clipping=registry.Register<BlueprintFeature>("KMG.Archetypes.ClippingShotFeature",
                ()=>CreateGrant("Clipping Shot", "Arm this deed as a free action. If your next firearm attack this round misses, spend exactly 1 grit and deal half normal damage. Dead Shot is excluded.", clippingAbility));
            BlueprintArchetype archetype=registry.Register<BlueprintArchetype>(ArchetypeSymbol,
                ()=>CreateArchetype(cls, baseGrit.Feature, grit, quickClear, focused,
                    nimble, lucky, gunTraining, fortune, bleedingWound, clipping));
            cls.Archetypes=cls.Archetypes.Concat(new[]{archetype}).ToArray();
            return new MysteriousStrangerBlueprintSet(archetype,grit,focused,focusedBuff,lucky,
                fortune,fortuneBuff,clipping,clippingBuff,fortuneResource);
        }

        private static BlueprintArchetype CreateArchetype(BlueprintCharacterClass cls,
            BlueprintFeature baseGrit, BlueprintFeature grit, BlueprintFeature quickClear,
            BlueprintFeature focused, BlueprintFeature[] nimble, BlueprintFeature[] lucky,
            BlueprintFeatureSelection training, BlueprintFeature fortune,
            BlueprintFeature bleeding, BlueprintFeature clipping)
        {
            var a=ScriptableObject.CreateInstance<BlueprintArchetype>(); a.name="KMG_MysteriousStranger_Archetype";
            a.LocalizedName=LocalizationService.Create("KMG.MysteriousStranger.Name","Mysterious Stranger");
            a.LocalizedDescription=LocalizationService.Create("KMG.MysteriousStranger.Description","A force-of-personality gunslinger who relies on Charisma, luck, and an unwillingness to give up.");
            a.OverrideAttributeRecommendations=true;a.RecommendedAttributes=new[]{StatType.Dexterity,StatType.Charisma};a.NotRecommendedAttributes=Array.Empty<StatType>();
            typeof(BlueprintArchetype).GetField("m_ParentClass",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(a,cls);
            a.RemoveFeatures=new[]{Entry(1,baseGrit,quickClear),Entry(2,nimble[0]),Entry(5,training),Entry(6,nimble[1]),Entry(10,nimble[2]),Entry(11,bleeding),Entry(14,nimble[3]),Entry(18,nimble[4])};
            a.AddFeatures=new[]{Entry(1,grit,focused),Entry(2,lucky[0]),Entry(5,fortune),Entry(6,lucky[1]),Entry(10,lucky[2]),Entry(11,clipping),Entry(14,lucky[3]),Entry(18,lucky[4])};
            a.ComponentsArray=Array.Empty<BlueprintComponent>(); return a;
        }
        private static LevelEntry Entry(int level,params BlueprintFeatureBase[] f){return new LevelEntry{Level=level,Features=f.ToList()};}
        private static BlueprintFeature CreateLucky(int rank){var f=ScriptableObject.CreateInstance<BlueprintFeature>();f.name="KMG_Lucky_"+rank;f.Ranks=1;f.IsClassFeature=true;var b=ScriptableObject.CreateInstance<AddStatBonus>();b.Stat=StatType.SaveWill;b.Value=rank;b.Descriptor=ModifierDescriptor.Luck;f.ComponentsArray=new BlueprintComponent[]{b};BlueprintUnitFactAccess.Resolve().Configure(f,LocalizationService.Create("KMG.Lucky."+rank+".Name","Lucky +"+rank),LocalizationService.Create("KMG.Lucky."+rank+".Description","Gain a +"+rank+" luck bonus on Will saving throws."),null);return f;}
        private static BlueprintBuff CreateBuff(string name,string display,string description,BlueprintComponent component){var b=ScriptableObject.CreateInstance<BlueprintBuff>();b.name="KMG_"+name+"_Buff";b.Stacking=StackingType.Replace;b.IsClassFeature=true;b.FxOnStart=new Kingmaker.ResourceLinks.PrefabLink();b.FxOnRemove=new Kingmaker.ResourceLinks.PrefabLink();b.ResourceAssetIds=Array.Empty<string>();b.ComponentsArray=component==null?Array.Empty<BlueprintComponent>():new[]{component};BlueprintUnitFactAccess.Resolve().Configure(b,LocalizationService.Create("KMG."+name+".Buff.Name",display),LocalizationService.Create("KMG."+name+".Buff.Description",description),null);return b;}
        private static BlueprintAbility CreateArmAbility(string name,BlueprintBuff marker,BlueprintAbilityResource resource,int cost,bool spend){var a=ScriptableObject.CreateInstance<BlueprintAbility>();a.name="KMG_"+name.Replace(" ","").Replace("'","")+"_Ability";a.Type=AbilityType.Extraordinary;a.Range=AbilityRange.Personal;a.CanTargetSelf=true;a.CanTargetPoint=a.CanTargetEnemies=a.CanTargetFriends=false;a.SpellResistance=false;a.ActionType=name=="Focused Aim"?UnitCommand.CommandType.Swift:UnitCommand.CommandType.Free;a.Animation=UnitAnimationActionCastSpell.CastAnimationStyle.Self;a.ResourceAssetIds=Array.Empty<string>();a.LocalizedDuration=LocalizationService.Create("KMG."+name+".Ability.Duration","Until the end of your turn");a.LocalizedSavingThrow=LocalizationService.Create("KMG."+name+".Ability.SavingThrow","None");var logic=ScriptableObject.CreateInstance<ArmMysteriousStrangerDeed>();logic.Marker=marker;logic.Resource=resource;logic.Cost=cost;logic.SpendOnActivation=spend;logic.UsesFocusedAimTrueGrit=name=="Focused Aim";a.ComponentsArray=new BlueprintComponent[]{logic};string description=name=="Focused Aim"?"As a swift action, spend 1 grit. Until the end of your turn, add your Charisma modifier (minimum +1) to all firearm damage rolls, not attack rolls. At 7th level, Dead Shot multiplies this bonus by the number of hits.":"Activate "+name+" for your next qualifying firearm event this round.";BlueprintUnitFactAccess.Resolve().Configure(a,LocalizationService.Create("KMG."+name+".Ability.Name",name),LocalizationService.Create("KMG."+name+".Ability.Description",description),null);return a;}
        private static BlueprintFeature CreateGrant(string name,string description,BlueprintAbility ability){var f=ScriptableObject.CreateInstance<BlueprintFeature>();f.name="KMG_"+name.Replace(" ","").Replace("'","")+"_Feature";f.Ranks=1;f.IsClassFeature=true;var add=ScriptableObject.CreateInstance<AddFacts>();add.Facts=new BlueprintUnitFact[]{ability};f.ComponentsArray=new BlueprintComponent[]{add};BlueprintUnitFactAccess.Resolve().Configure(f,LocalizationService.Create("KMG."+name+".Feature.Name",name),LocalizationService.Create("KMG."+name+".Feature.Description",description),null);return f;}
        private static BlueprintFeature CreateResourceGrant(string name,string description,BlueprintAbility ability,BlueprintAbilityResource resource){var f=CreateGrant(name,description,ability);var add=ScriptableObject.CreateInstance<AddAbilityResources>();add.Resource=resource;add.RestoreAmount=true;add.Amount=0;f.ComponentsArray=f.ComponentsArray.Concat(new BlueprintComponent[]{add,new GritResourceAmountBonus{Resource=resource,Attribute=StatType.Charisma,Minimum=0}}).ToArray();return f;}
    }
}
