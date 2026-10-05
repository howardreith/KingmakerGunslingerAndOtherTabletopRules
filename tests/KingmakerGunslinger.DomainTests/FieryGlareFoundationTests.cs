using System;
using System.IO;
namespace KingmakerGunslinger.DomainTests
{
    internal static class FieryGlareFoundationTests
    {
        private static string _il;
        private static string Read(string path) { return File.ReadAllText(Path.Combine(Environment.CurrentDirectory, path.Replace('/', Path.DirectorySeparatorChar))); }
        private static string Factory { get { return Read("src/KingmakerGunslinger/ElementalRaces/UnpublishedRaceTraitFoundationFactory.cs"); } }
        private static string Native(string method)
        {
            if (_il == null) _il = Read("artifacts/inspection/bodyguard-native/Assembly-CSharp.il");
            int end = _il.IndexOf("} // end of method " + method, StringComparison.Ordinal);
            Assertions.True(end >= 0, "Exact native method exists: " + method);
            int start = _il.LastIndexOf("  .method ", end, StringComparison.Ordinal);
            return _il.Substring(start, end - start);
        }
        private static void Runtime(string id) { Assertions.True(Read("src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedRaceTraitFoundations.cs").Contains('"' + id + '"'), "Guarded real-event obligation exists: " + id); }
        internal static void NoPublication(string name)
        {
            foreach (string path in new[] { "blueprints/blueprints.json", "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs", "src/KingmakerGunslinger/ElementalRaces/ElementalRaceBlueprintFactory.cs", "src/KingmakerGunslinger/ElementalRaces/ElementalAlternateTraitBlueprintFactory.cs", "src/KingmakerGunslinger/AidAnotherCompatibility/FavoredClassTraitResolver.cs", "src/KingmakerGunslinger/FeatureModules/FeatureModuleCatalog.cs" })
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, path))) Assertions.False(Read(path).Contains(name), "No ordinary acquisition/publication reference: " + path);
            Assertions.False(Factory.Contains("LocalizationService"), "No visible localization.");
            Assertions.False(Factory.Contains("registry.Register"), "No registered graph.");
            Assertions.False(Factory.Contains("m_Icon"), "No icon assignment.");
        }
        internal static void NativeStatContract() { Assertions.True(Native("Take10ForSuccess::OnEventAboutToTrigger").Contains("Take10ForSuccess::Skill") && Native("Take10ForSuccess::OnEventAboutToTrigger").Contains("RuleSkillCheck::get_StatType"), "Native handler matches only its configured stat."); }
        internal static void AbsentControl() { Runtime("fiery-absent"); }
        internal static void OffControl() { Runtime("fiery-off-default"); }
        internal static void SuccessExactlyTen() { Assertions.True(Native("RuleSkillCheck::OnTrigger").Contains("ldc.i4.s   10") && Native("RuleSkillCheck::OnTrigger").Contains("set_D20"), "Native branch selects a d20 of ten."); Runtime("fiery-take10-success"); }
        internal static void FailureRollsNormally() { Assertions.True(Native("RuleSkillCheck::OnTrigger").Contains("RuleSkillCheck::RollD20()"), "Native failure branch rolls normally."); Runtime("fiery-roll-on-take10-failure"); }
        internal static void CombatCompatible() { Assertions.False(Native("Take10ForSuccess::OnEventAboutToTrigger").Contains("IsInCombat"), "No native combat restriction."); Runtime("fiery-combat"); }
        internal static void DialogueStatCompatible() { Assertions.True(Factory.Contains("take10.Skill = StatType.CheckIntimidate"), "Exact dialogue CheckIntimidate stat, never broad Persuasion."); }
        internal static void BluffUnaffected() { Runtime("fiery-unaffected-"); Assertions.False(Factory.Contains("take10.Skill = StatType.CheckBluff"), "No Bluff binding."); }
        internal static void DiplomacyUnaffected() { Assertions.False(Factory.Contains("take10.Skill = StatType.CheckDiplomacy"), "No Diplomacy binding."); }
        internal static void OtherSkillsUnaffected() { Assertions.False(Factory.Contains("take10.Skill = StatType.SkillPersuasion"), "No general Persuasion binding."); }
        internal static void FreeActivation() { Assertions.True(Factory.Contains("AbilityActivationType.Immediately") && Factory.Contains("UnitCommand.CommandType.Free"), "Native immediate/free toggle contract."); Runtime("fiery-free-activation"); }
        internal static void StartsOff() { Assertions.True(Factory.Contains("toggle.IsOnByDefault = false"), "Off by default."); }
        internal static void DeactivationAndReplay() { Assertions.True(Factory.Contains("toggle.DeactivateImmediately = true"), "Native stop removes the buff on toggle-off before another rule."); Runtime("fiery-deactivation"); Runtime("fiery-repeat-one-component"); }
        internal static void IndependentUnits() { Runtime("fiery-independent-units"); Assertions.False(Factory.Contains("Dictionary<Unit"), "No shared unit state."); }
        internal static void NoPublication() { NoPublication("FieryGlare"); }
        internal static void NoSaveAcquisition() { Assertions.False(Read("blueprints/blueprints.json").Contains("FieryGlare"), "No serialized manifest identity."); }
        internal static void RepeatedInitializationIsDormant() { Assertions.False(System.Text.RegularExpressions.Regex.IsMatch(Factory, @"static\s+FieryGlareFoundation\s+\w+\s*;"), "No retained static graph field."); Assertions.False(Read("src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs").Contains("CreateFieryGlare"), "No initialization call."); }
        internal static void ExactMismatchFailsClosed() { Assertions.True(Factory.Contains("nativeComponent != typeof(Take10ForSuccess)"), "Exact type mismatch is rejected."); Runtime("fiery-contract-fails-closed"); }
        internal static void HonestAdaptationDescription() { Assertions.True(Factory.Contains("whenever that would succeed; otherwise, they are rolled normally"), "Future prose states adapted behavior honestly."); }
        internal static void NoGlobalPatchOrResources() { Assertions.False(Factory.Contains("Harmony"), "No patch."); Assertions.False(Factory.Contains("CreateInstance<ActivatableAbilityResourceLogic>"), "No consumed resource."); }
    }
}
