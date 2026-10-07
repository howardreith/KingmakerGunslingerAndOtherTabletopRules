using System;
using System.IO;
using KingmakerGunslinger.ElementalRaces;
namespace KingmakerGunslinger.DomainTests
{
    internal static class StoicDignityFoundationTests
    {
        private static readonly object AbilityA=new object(), AbilityB=new object(), BuffA=new object(), BuffB=new object(), ChildA=new object(), ChildB=new object();
        private sealed class Token { public override string ToString() { return "same name"; } }
        private static StoicEffectIdentity A() { return new StoicEffectIdentity(null,AbilityA); }
        private static StoicEffectIdentity B() { return new StoicEffectIdentity(null,AbilityB); }
        private static string Read(string path) { return File.ReadAllText(Path.Combine(Environment.CurrentDirectory,path.Replace('/',Path.DirectorySeparatorChar))); }
        private static void Match(bool expected,StoicEffectIdentity a,StoicEffectIdentity b) { Assertions.Equal(expected,StoicDignityPolicy.SameEffect(a,b),"Exact identity correlation."); }
        private static void Yes(StoicEffectIdentity existing,StoicEffectIdentity incoming) { Assertions.False(StoicDignityPolicy.SameEffect(incoming,existing),"Unproven identity leaves bonus enabled."); }
        private static void Eligible(bool expected,bool conscious=true,bool dead=false,bool mind=true,bool self=false,bool ally=true,double distance=1,bool present=true,bool contract=true)
        { Assertions.Equal(expected,StoicDignityPolicy.Eligible(present,conscious,dead,mind,true,self,ally,distance,false,contract),"Frozen aura/holder policy."); }
        private static void Runtime(string id) { Assertions.True(Read("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedRaceTraitFoundations.cs").Contains('"'+id+'"'),"Guarded real-event obligation exists: "+id); }
        internal static void NoExistingEffect() { Yes(null, A()); }
        internal static void ExactAbilityMatch() { Match(true, A(), A()); }
        internal static void ExactBuffMatch() { Match(true, new StoicEffectIdentity(BuffA, AbilityA), new StoicEffectIdentity(BuffA, AbilityB)); }
        internal static void ParentSourceLineage() { Match(true, A(), new StoicEffectIdentity(null, ChildA, new[] { AbilityA })); }
        internal static void CharmDoesNotSuppressFear() { Match(false, A(), B()); }
        internal static void FearDoesNotSuppressCharm() { Match(false, B(), A()); }
        internal static void DifferentAbilitySameCaster() { Match(false, A(), B()); }
        internal static void SameAbilityDifferentCaster() { Match(true, A(), A()); }
        internal static void MissingIdentityGrants() { Yes(new StoicEffectIdentity(null, null), A()); }
        internal static void AmbiguousIdentityGrants() { Yes(new StoicEffectIdentity(BuffA, AbilityA, ambiguous:true), new StoicEffectIdentity(BuffA, AbilityA)); }
        internal static void ExistingAmbiguityGrants() { Match(false, A(), new StoicEffectIdentity(null, AbilityA, ambiguous:true)); }
        internal static void KnownDifferentBuffsOverrideAbility() { Match(false, new StoicEffectIdentity(BuffA, AbilityA), new StoicEffectIdentity(BuffB, AbilityA)); }
        internal static void SiblingAbilitiesAreUnrelated() { Match(false, new StoicEffectIdentity(null, ChildA, new[] { AbilityA }), new StoicEffectIdentity(null, ChildB, new[] { AbilityA })); }
        internal static void NameEqualityIsNotIdentity() { Match(false, new StoicEffectIdentity(null, new Token()), new StoicEffectIdentity(null, new Token())); }
        internal static void HolderUnconscious() { Eligible(false, conscious:false); }
        internal static void HolderDead() { Eligible(false, dead:true); }
        internal static void NonMindAffecting() { Eligible(false, mind:false); }
        internal static void AllyNineFeet() { Eligible(true, distance:2.7432); }
        internal static void ExactlyTenFeet() { Eligible(true, distance:3.048); Eligible(true, distance:(double)3.048f); }
        internal static void AllyBeyondTenFeet() { Eligible(false, distance:3.3528); }
        internal static void EnemyWithinTenFeet() { Eligible(false, ally:false); }
        internal static void HolderTraitOnly() { Assertions.True(StoicDignityPolicy.Eligible(true,true,false,true,false,true,true,0,false,true), "Self holder gets trait."); Eligible(false,self:true); }
        internal static void ConsciousnessRecovered() { Eligible(false,conscious:false); Eligible(true,conscious:true); }
        internal static void AbsentFoundation() { Eligible(false,present:false); }
        internal static void ContractMismatch() { Eligible(false,contract:false); Runtime("stoic-contract-fails-closed"); Assertions.True(Read("src/KingmakerGunslinger/ElementalRaces/StoicDignityMechanics.cs").Contains("contextType != typeof(MechanicsContext)"), "Wrong native context type is rejected."); }
        internal static void InvalidDistances() { Eligible(false,distance:double.NaN); Eligible(false,distance:double.PositiveInfinity); Eligible(false,distance:-1); }
        internal static void NativeStackingAndReplay() { Runtime("stoic-two-morale-providers"); Runtime("stoic-foreign-morale"); Runtime("stoic-replay-once"); }
        internal static void NativeAreaLifecycle() { Runtime("stoic-recipient-exit"); Runtime("stoic-recipient-reentry"); Runtime("stoic-area-deactivation"); }
        internal static void NoEffectMutation() { Runtime("stoic-no-cleanse-or-immunity"); string s=Read("src/KingmakerGunslinger/ElementalRaces/StoicDignityMechanics.cs"); foreach(string token in new[]{".Remove(",".EndTime =",".IsSuppressed =","BuffImmunity","SpellImmunity"}) Assertions.False(s.Contains(token),"No effect mutation: "+token); }
        internal static void NoPublicationOrSaveIdentity() { FieryGlareFoundationTests.NoPublication("StoicDignity"); }
        internal static void InstanceReplayNotUnitGlobal() { string s=Read("src/KingmakerGunslinger/ElementalRaces/StoicDignityMechanics.cs"); Assertions.True(s.Contains("ConditionalWeakTable<RuleSavingThrow, object> _applied"),"Exact rule object replay key."); Assertions.False(s.Contains("static ConditionalWeakTable"),"Per-fact replay ledger."); }
        internal static void ExactDescriptorAndContextWalk() { string s=Read("src/KingmakerGunslinger/ElementalRaces/StoicDignityMechanics.cs"); Assertions.True(s.Contains("current.ParentContext") && s.Contains("SpellDescriptor.MindAffecting") && s.Contains("reason.Ability?.Blueprint"),"Typed current/parent/source descriptor contracts."); }
        internal static void NoPollingOrPatch() { string s=Read("src/KingmakerGunslinger/ElementalRaces/StoicDignityMechanics.cs"); foreach(string token in new[]{"HarmonyPatch","OnUpdate","Game.Instance.State.Units","GetMethods("}) Assertions.False(s.Contains(token),"No broad runtime surface: "+token); }
    }
}
