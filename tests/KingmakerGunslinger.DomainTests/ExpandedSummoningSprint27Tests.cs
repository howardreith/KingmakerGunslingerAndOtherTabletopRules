using System;
using System.Linq;
using KingmakerGunslinger.Summoning;
namespace KingmakerGunslinger.DomainTests
{
    internal static class ExpandedSummoningSprint27Tests
    {
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
            Assertions.True(routes.All(v=>!SummonVisibilityCatalog.IsPublished(v)),"Unqualified Griffon publication is suppressed");
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
        internal static void ExactOriginalBodyBinding()
        {
            Assertions.True(GriffonVisualPolicy.IsPermitted("griffon",GriffonVisualPolicy.Bones),"Measured original weights bind");
            Assertions.True(!GriffonVisualPolicy.IsPermitted("hippogriff",GriffonVisualPolicy.Bones),"No second species");
            Assertions.True(!GriffonVisualPolicy.IsPermitted("griffon",GriffonVisualPolicy.Bones.Concat(new[]{"inventedWing"})),"Unknown weights refuse");
            Assertions.True(!GriffonVisualPolicy.IsPermitted("griffon",GriffonVisualPolicy.Bones.Skip(1)),"Incomplete binding refuses");
        }
    }
}
