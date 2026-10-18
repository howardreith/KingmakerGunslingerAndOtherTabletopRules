using System;
using System.Linq;
using KingmakerGunslinger.Summoning;
namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint27Tests
    {
        internal const int AppendedCatalogIdentityCount=13; // one unit, six roots, six private carriers; adapters are a separate manifest.
        internal static void AdultProfileAndOrdinaryRoles()
        {
            // Exercise the same pure catalog guards as blueprint bootstrap;
            // the native run must still prove final registration/consumers.
            ExpandedSummoningCatalog.Validate();
            ExpandedSummoningNaturalProfiles.Validate();
            SummonVisibilityCatalog.Validate();
            SummonIconCatalog.Validate();
            ExpandedSummoningDonorCatalog.Validate();
            ExpandedSummoningSpecialProfiles.Validate();
            SummonNativeOptionCatalog.Validate();
            SummonNativeExpansionCatalog.Validate();
            var p=ExpandedSummoningNaturalProfiles.For("griffon");
            Assertions.Equal("MagicalBeast",p.HitDieClass,"Printed racial class");
            Assertions.Equal(5,p.HitDice,"Printed racial dice");
            Assertions.Equal("Large",p.Size,"Printed footprint");
            Assertions.True(new[]{p.Strength,p.Dexterity,p.Constitution,p.Intelligence,p.Wisdom,p.Charisma}.SequenceEqual(new[]{16,15,16,5,13,8}),"Adult abilities");
            Assertions.Equal(6,p.NaturalArmor,"Printed natural armor");
            Assertions.Equal(80,p.SpeedFeet,"One effective aerial speed, never110");
            Assertions.Equal("GriffonBite1d6",p.PrimaryWeapon,"Bite leads");
            Assertions.True(p.AdditionalWeapons.SequenceEqual(new[]{"GriffonTalon1d6","GriffonTalon1d6","GriffonRake1d4","GriffonRake1d4"}),"Two ordinary talons then two gated rake slots");
            Assertions.Equal(0,p.AdditionalSecondaryWeapons.Count,"All printed attacks are primary");
            Assertions.True(p.Facts.Contains("WeaponFocusBite") && !p.Facts.Contains("WeaponFocusClaw") && p.Facts.Contains("IronWill") && p.Facts.Contains("SkillFocusPerception"),"Exactly the printed feat identities");
            Assertions.True(p.Skills.SequenceEqual(new[]{"Mobility","Perception"}),"Fly ranks not redistributed");
        }
        internal static void SixNatureRootsRemainHidden()
        {
            var creature=ExpandedSummoningCatalog.All.Single(v=>v.Key=="griffon");
            Assertions.True(!creature.MonsterTier.HasValue && !creature.MonsterTemplated && creature.NaturesAllyTier==4,"SNAIV only, no template");
            var routes=ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly).Where(v=>v.Creature.Key=="griffon").OrderBy(v=>v.ParentTier).ToArray();
            Assertions.True(routes.Select(v=>v.ParentTier).SequenceEqual(new[]{4,5,6,7,8,9}),"Six public-root identities");
            Assertions.Equal(SummonMultiplicity.One,routes[0].Multiplicity,"IV one");
            Assertions.Equal(SummonMultiplicity.OneD3,routes[1].Multiplicity,"V1d3");
            Assertions.True(routes.Skip(2).All(v=>v.Multiplicity==SummonMultiplicity.OneD4PlusOne),"VI-IX1d4+1");
            Assertions.True(routes.All(v=>SummonVisibilityCatalog.IsPublished(v)==!SummoningCurrentSurface.IsWithheld("griffon")),"Exactly the current Griffon publication stage");
            Assertions.Equal(AppendedCatalogIdentityCount,ExpandedSummoningIdentityCatalog.Build().Count(v=>v.Symbol=="KMG.Summoning.Unit.Griffon" || v.Symbol.Contains(".Griffon.")),"Exact scoped foundation allocation, separate from adapters");
        }
        internal static void RakeIsConditionalWithoutGrab()
        {
            Assertions.True(!ExpandedSummoningSpecialProfiles.ShouldRakeApply(true,false,false),"Ordinary routine excludes rake");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldRakeApply(true,true,false),"Pounce needs no successful Grab or talon hit");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldRakeApply(true,false,true),"Genuine already-held exception retained; no new consumer implied");
            Assertions.True(ExpandedSummoningSpecialProfiles.ShouldRakeApply(false,false,false),"Ordinary bite/talons remain available");
            var ids=ExpandedSummoningIdentityCatalog.Build();
            Assertions.True(!ids.Any(v=>v.Symbol.Contains("Griffon") && (v.Symbol.Contains("Grab")||v.Symbol.Contains("Swallow")||v.Symbol.Contains("Leap"))),"No invented special attack graph");
        }
        internal static void ReachPreservesNativeBonusesAndOtherConsumers()
        {
            string owner=GriffonVisualPolicy.BlueprintGuid;
            foreach(string weapon in new[]{"67549c5f756c495a8592ab27a019fec8","130a92b8f9d64a46908cf05258fbb2b0","4807a234c116416988cd388e5a923776"}) {
                Assertions.Equal(5f,GriffonRules.Reach(owner,weapon,2),"Printed five-foot reach");
                Assertions.Equal(10f,GriffonRules.Reach(owner,weapon,7),"Live five-foot bonus preserved");
                Assertions.Equal(2f,GriffonRules.Reach(GriffonVisualPolicy.DonorGuid,weapon,2),"Native donor untouched");
                Assertions.Equal(2f,GriffonRules.Reach(null,weapon,2),"Unowned weapon untouched");
            }
            Assertions.Equal(2f,GriffonRules.Reach(owner,"released-other-weapon",2),"Unrelated weapon untouched");
        }
        internal static void PersistenceScopeIsExact()
        {
            const string scope=KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.GriffonScope;
            var policy=KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.SelectCreatureVariant(scope,"griffon");
            Assertions.True(policy.Family==SummonFamily.NaturesAlly&&policy.ParentTier==4&&policy.Multiplicity==SummonMultiplicity.One,"One named Griffon fixture, never an entire roster");
            Assertions.True(KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(scope,new[]{"griffon"}),"Exact one-creature membership");
            foreach(var keys in new[]{new[]{"griffon","griffon"},new[]{"griffon","tiger"},new[]{"tiger"},new string[0]})
                Assertions.True(!KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.ExactSelectedKeys(scope,keys),"No duplicate or unrelated fixture member");
            bool refused=false;try{KingmakerGunslinger.RuntimeTesting.ExpandedSummoningRosterPersistencePolicy.SelectCreatureVariant(scope,"tiger");}catch(InvalidOperationException){refused=true;}
            Assertions.True(refused,"The named scope cannot prepare another creature");
        }
        internal static void ExactOriginalBodyBinding()
        {
            Assertions.True(GriffonVisualPolicy.IsPermitted("griffon",GriffonVisualPolicy.Bones),"Measured original weights bind");
            Assertions.True(!GriffonVisualPolicy.IsPermitted("hippogriff",GriffonVisualPolicy.Bones),"No second species");
            Assertions.True(!GriffonVisualPolicy.IsPermitted("griffon",GriffonVisualPolicy.Bones.Concat(new[]{"inventedWing"})),"Unknown weights refuse");
            Assertions.True(!GriffonVisualPolicy.IsPermitted("griffon",GriffonVisualPolicy.Bones.Skip(1)),"Incomplete binding refuses");
        }
    }
}
