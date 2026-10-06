using System;
using System.IO;
namespace KingmakerGunslinger.DomainTests
{
    // Real installed-IL contract checks, not combat-mode gameplay qualification.
    internal static class LungeEngineContractTests
    {
        private static string _il;
        internal static string Native(string method, int occurrence = 0)
        {
            if (_il == null) _il = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "artifacts/inspection/bodyguard-native/Assembly-CSharp.il")).Replace("\r\n", "\n");
            int end = -1;
            for (int i = 0; i <= occurrence; ++i) end = _il.IndexOf("} // end of method " + method + "\n", end + 1, StringComparison.Ordinal);
            Assertions.True(end >= 0, "Exact installed method exists: " + method);
            int start = _il.LastIndexOf("  .method ", end, StringComparison.Ordinal);
            return _il.Substring(start, end - start);
        }
        internal static void AttackThreatSeparation()
        {
            string attack = Native("UnitAttack::GetApproachRadius"), threat = Native("UnitEngagementExtension::IsReach");
            Assertions.True(attack.Contains("AttackHandInfo::WeaponRange") && attack.Contains("get_Corpulence"), "Attack command starts from selected native final reach and live corpulence.");
            Assertions.True(threat.Contains("ItemEntityWeapon::get_AttackRange") && !threat.Contains("GetApproachRadius"), "Native threat is a distinct calculation.");
        }
        internal static void IndependentPeriodicClock()
        {
            string s = Native("UnitTicksController::TickOnUnit");
            Assertions.True(s.Contains("TimeToNextRoundTick") && s.Contains("ldc.r4     6."), "Native periodic six-second clock exists.");
            Assertions.False(s.Contains("UnitCommand") || s.Contains("StandardAction"), "Periodic fact rounds do not align to attack command or action cooldown.");
        }
        internal static void CooldownTransitionClock()
        {
            string s = Native("UnitCombatCooldownsController::TickOnUnit");
            Assertions.True(s.Contains("get_StandardAction") && s.Contains("cgt") && s.Contains("UnitCombatState::OnNewRound") && s.Contains("IUnitNewCombatRoundHandler"), "Native action round requires the inspected positive-to-zero Standard transition.");
        }
        internal static void TurnBasedBoundaries()
        {
            Assertions.True(Native("TurnController::Prepare").Contains("ITurnBasedModeHandler") && Native("TurnController::Prepare").Contains("OnNewRound"), "Turn-based start is native.");
            Assertions.True(Native("TurnController::End").Contains("InterruptCommands") && Native("TurnController::End").Contains("set_Status"), "Turn-based end is discrete.");
        }
        internal static void AcceptedAttemptDiffersFromRoll()
        {
            string run = Native("UnitCommands::Run", 1), start = Native("UnitCommand::Start");
            Assertions.True(run.Contains("IUnitRunCommandHandler") && run.Contains("TryMergeInto"), "Accepted and merged command paths differ.");
            Assertions.True(start.Contains("get_IsInState") && start.Contains("get_IsUnitEnoughClose") && start.Contains("IUnitCommandStartHandler"), "Start checks target/range before event delivery, so it cannot cover every canceled accepted attempt.");
        }
        internal static void BlockerHasNoProductionBinding()
        {
            string s = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,"docs/research/LUNGE-ENGINE-CONTRACT.md"));
            Assertions.True(s.Contains("BLOCKED-NO-SEPARABLE-SEAM") && s.Contains("two different clocks"), "Exact documented timing blocker retained.");
            string bootstrap = File.ReadAllText(Path.Combine(Environment.CurrentDirectory,"src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs"));
            Assertions.False(bootstrap.Contains("UnpublishedLunge"), "No blocked Lunge approximation is bootstrapped.");
        }
    }
}
