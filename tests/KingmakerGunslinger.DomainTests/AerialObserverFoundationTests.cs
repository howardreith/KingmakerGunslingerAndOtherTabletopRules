using System;
using System.IO;
using KingmakerGunslinger.ElementalRaces;
namespace KingmakerGunslinger.DomainTests
{
    internal static class AerialObserverFoundationTests
    {
        private static readonly object Carrier = new object();
        private static string Read(string path) { return File.ReadAllText(Path.Combine(Environment.CurrentDirectory,path.Replace('/',Path.DirectorySeparatorChar))); }
        private static string Mechanics { get { return Read("src/KingmakerGunslinger/ElementalRaces/AerialObserverMechanics.cs"); } }
        private static string Factory { get { return Read("src/KingmakerGunslinger/ElementalRaces/UnpublishedAerialObserverFoundationFactory.cs"); } }
        private static bool Flight(object actual,bool active=true,bool suppressed=false,bool contract=true) { return AerialObserverPolicy.ExactFlight(Carrier,actual,active,suppressed,contract); }
        private static void Runtime(string id) { Assertions.True(Read("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedAerialObserverFoundation.cs").Contains('"'+id+'"'),"Real native runtime obligation: "+id); }
        internal static void Grounded() { Assertions.False(Flight(null),"No exact carrier means grounded."); Runtime("aerial-grounded"); }
        internal static void ExactFlight() { Assertions.True(Flight(Carrier),"Canonical active flight qualifies."); Assertions.Equal(2,AerialObserverPolicy.Bonus(true,Flight(Carrier),true,true),"Exactly +2."); Runtime("aerial-active-perception"); }
        internal static void FlightRemoved() { Assertions.False(Flight(Carrier,active:false),"Inactive carrier does not qualify."); Assertions.False(Flight(null),"Removed carrier does not qualify."); Runtime("aerial-flight-removed"); }
        internal static void SuppressedFlight() { Assertions.False(Flight(Carrier,suppressed:true),"Suppression denies mechanical flight."); }
        internal static void VisualWings() { Assertions.False(Flight(new object()),"Unrelated wings identity is not flight."); }
        internal static void HoverOnly() { Assertions.Equal(0,AerialObserverPolicy.Bonus(true,Flight(null),true,true),"Hover animation without the carrier gives no bonus."); Runtime("aerial-visual-height-only"); }
        internal static void ElevatedTerrain() { Assertions.False(Flight(null),"Elevation has no carrier identity."); Assertions.False(Mechanics.Contains("Position.y"),"No altitude approximation."); }
        internal static void JumpFallOnly() { Assertions.False(Mechanics.Contains("FlyHeight"),"Jump/fall height is not a production predicate."); Assertions.False(Flight(new object()),"Jump-only identity cannot qualify."); }
        internal static void PerceptionOnly() { Assertions.Equal(0,AerialObserverPolicy.Bonus(true,true,false,true),"Another check cannot get a bonus."); Assertions.True(Mechanics.Contains("evt.StatType != StatType.SkillPerception"),"Exact native stat filter."); }
        internal static void OtherSkill() { Runtime("aerial-other-skill"); }
        internal static void DuplicateNativeTrait() { Assertions.True(Mechanics.Contains("ModifierDescriptor.Trait"),"Real native descriptor."); Runtime("aerial-duplicate-native-trait"); }
        internal static void ForeignTrait() { Runtime("aerial-foreign-lesser-trait"); Runtime("aerial-foreign-greater-trait"); Runtime("aerial-foreign-distinct-descriptor"); }
        internal static void UnitsIndependent() { Runtime("aerial-independent-units"); Assertions.False(Mechanics.Contains("static ConditionalWeakTable"),"No shared owner ledger."); }
        internal static void RepeatedInitialization() { Runtime("aerial-unregistered-idempotent-builders"); Assertions.False(Factory.Contains("registry.Register"),"Repeated builders register nothing."); }
        internal static void ExactContractMismatch() { Assertions.False(Flight(Carrier,contract:false),"Missing flight contract fails closed."); Assertions.Equal(0,AerialObserverPolicy.Bonus(true,true,true,false),"Missing rule contract gives no bonus."); Runtime("aerial-contract-fails-closed"); }
        internal static void CanonicalIdentityOnly() { Assertions.False(Flight(new object()),"A copied name/GUID is not reference identity."); Assertions.True(Mechanics.Contains("ReferenceEquals(carrier, ResourcesLibrary.TryGetBlueprint<BlueprintBuff>"),"Canonical lookup required."); Runtime("aerial-cloned-carrier-rejected"); }
        internal static void NoPublication()
        {
            foreach(string path in new[]{"blueprints/blueprints.json","src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs","src/KingmakerGunslinger/ElementalRaces/ElementalRaceBlueprintFactory.cs","src/KingmakerGunslinger/ElementalRaces/ElementalAlternateTraitBlueprintFactory.cs","src/KingmakerGunslinger/AidAnotherCompatibility/FavoredClassTraitResolver.cs","src/KingmakerGunslinger/FeatureModules/FeatureModuleConfiguration.cs","src/KingmakerGunslinger/Blueprints/OwnedIconAssignments.cs"})
                Assertions.False(Read(path).Contains("AerialObserver"),"No ordinary publication/selection/race/setting/icon reference: "+path);
            Assertions.False(Factory.Contains("LocalizationService") || Factory.Contains("m_Icon") || Factory.Contains("registry.Register"),"No visible graph.");
        }
        internal static void NoOrdinarySaveIdentity() { Assertions.False(Read("blueprints/blueprints.json").Contains("AerialObserver"),"No ordinary save-visible trait identity."); Runtime("aerial-provider-removal"); }
        internal static void FoundationAbsent() { Assertions.Equal(0,AerialObserverPolicy.Bonus(false,true,true,true),"Flight without the foundation has no bonus."); Runtime("aerial-flight-without-foundation"); }
        internal static void ActiveAndPassivePaths()
        {
            Assertions.True(LungeEngineContractTests.Native("RuleSkillCheck::OnTrigger").Contains("get_ModifiedValue"),"Stat is captured at native resolution.");
            Assertions.True(LungeEngineContractTests.Native("PartyPerceptionController::RollPerception").Contains("RuleSkillCheck::.ctor"),"Native passive map detection uses this rule.");
            Assertions.True(LungeEngineContractTests.Native("RuleCachedPerceptionCheck::OnTrigger").Contains("RuleSkillCheck::OnTrigger"),"Cached path shares base resolution."); Runtime("aerial-cached-perception");
        }
        internal static void TemporaryCleanup() { Assertions.True(Mechanics.Contains("_modifier?.Remove()") && Mechanics.Contains("public override void OnFactDeactivate()"),"Native flight/provider lifecycle removes only the owned modifier."); Runtime("aerial-rule-cleanup"); Runtime("aerial-fixture-exact-cleanup"); }
        internal static void ReplayOnce() { Assertions.True(Mechanics.Contains("ConditionalWeakTable<RuleSkillCheck, object> _seen"),"Exact rule object per component."); Runtime("aerial-replay-once-per-provider"); }
        internal static void NoPollingOrGlobalPatch() { foreach(string token in new[]{"Harmony","OnUpdate","Game.Instance.State.Units","Time.frameCount","FlyHeight"}) Assertions.False(Mechanics.Contains(token),"No broad surface: "+token); }
        internal static void PassiveRawConsumers() { Runtime("aerial-passive-stat-consumers"); Assertions.True(LungeEngineContractTests.Native("LocationRevealController::Tick").Contains("Enumerable::Max"),"Native world-map discovery uses the raw Perception stat before a rule."); }
        internal static void NativeFlightTransitions() { Assertions.True(Mechanics.Contains("carrier.Components.Add(listener)") && Mechanics.Contains("listener.Fact = carrier") && Mechanics.Contains("public override void OnTurnOff()"),"Public per-fact component lifecycle, no blueprint mutation."); Runtime("aerial-flight-inactive"); Runtime("aerial-flight-reactivated"); }
        internal static void NativeSuppressionLifecycle() { Runtime("aerial-native-flight-suppression"); Runtime("aerial-native-flight-suppression-release"); Assertions.True(LungeEngineContractTests.Native("UnitPartBuffSuppress::Update").Contains("Fact::Deactivate"),"Native suppression deactivates before marking suppression."); }
        internal static void OwnedListenerCleanup() { Runtime("aerial-flight-listener-detached"); Assertions.True(Mechanics.Contains("ReferenceEquals(c, listener)") && Mechanics.Contains("carrier.Components.RemoveAt(index)"),"Exact owned instance cleanup, registered graph unchanged."); }
        internal static void CompleteMechanicalCarrier() { Assertions.True(Mechanics.Contains("components.Length != 3") && Mechanics.Contains("SpellDescriptor.Ground") && Mechanics.Contains("UnitCondition.DifficultTerrain") && Mechanics.Contains("ac.ArmorClassBonus != 3"),"All exact mechanical flight components are required."); Runtime("aerial-native-flight-state"); }
    }
}
