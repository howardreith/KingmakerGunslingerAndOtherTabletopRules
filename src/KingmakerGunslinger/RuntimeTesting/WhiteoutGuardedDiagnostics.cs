using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.ElementalRaces;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed class WhiteoutAttackObservation
    {
        public string Id, AttackType, WeaponGuid, AttackerId, TargetId, Error;
        public int StageCalls, NativeFailures, WhiteoutRollCalls, ForcedConsumed;
        public int? WhiteoutRoll, NativeConcealmentRoll;
        public string NativeConcealment;
        public bool NativeSucceeded, Result, Active, IgnoreConcealment, Seeking, DecisionFailedOpen;
    }
    // Exact request + current thread + exact registered attack, one consumption.
    // No ordinary-play caller, broad tuple matching or global RNG patch.
    internal sealed class WhiteoutGuardedDiagnostics : IDisposable
    {
        [ThreadStatic] private static WhiteoutGuardedDiagnostics _current;
        private readonly int _thread=Thread.CurrentThread.ManagedThreadId;
        private readonly Dictionary<RuleAttackRoll,WhiteoutAttackObservation> _owned=new Dictionary<RuleAttackRoll,WhiteoutAttackObservation>(new AttackReferenceComparer());
        private readonly List<WhiteoutAttackObservation> _records=new List<WhiteoutAttackObservation>();
        private RuleAttackRoll _pending;
        private int _roll;
        private bool _disposed;
        internal readonly string RunId;
        internal bool CleanupVerified { get; private set; }
        internal IEnumerable<WhiteoutAttackObservation> Records { get { return _records; } }
        internal WhiteoutGuardedDiagnostics(RuntimeTestRequest request,bool workingComplete,bool saveWriteObserved)
        {
            if(request==null || request.Scenario!=RuntimeTestScenarioCatalog.ObserveUnpublishedWhiteoutFoundation ||
                !request.ExitAfterCompletion || (string)request.Parameters?["saveName"]!="KMG_AUTOMATION_WORKING" ||
                string.IsNullOrWhiteSpace(request.RunId) || !workingComplete || saveWriteObserved || _current!=null)
                throw new InvalidOperationException("Whiteout deterministic rolls require exact guarded no-write foundation request.");
            RunId=request.RunId; _current=this;
        }
        private void Guard()
        {
            if(_disposed || !ReferenceEquals(_current,this) || Thread.CurrentThread.ManagedThreadId!=_thread)
                throw new InvalidOperationException("Whiteout diagnostic request/thread mismatch.");
        }
        internal WhiteoutAttackObservation Register(string id,RuleAttackRoll attack)
        {
            Guard();
            if(attack?.Initiator==null || attack.Target==null || attack.Weapon==null || _owned.ContainsKey(attack))
                throw new InvalidOperationException("Exact owned attack registration required.");
            var value=new WhiteoutAttackObservation { Id=id, AttackType=attack.AttackType.ToString(), WeaponGuid=attack.Weapon.Blueprint.AssetGuid,
                AttackerId=attack.Initiator.UniqueId, TargetId=attack.Target.UniqueId };
            _owned.Add(attack,value); _records.Add(value); return value;
        }
        internal void Queue(RuleAttackRoll attack,int roll)
        {
            Guard();
            if(roll<1 || roll>100) throw new ArgumentOutOfRangeException("roll");
            if(!_owned.ContainsKey(attack) || _pending!=null) throw new InvalidOperationException("One exact registered attack may own a forced roll.");
            _pending=attack; _roll=roll;
        }
        internal void CancelPending() { Guard(); _pending=null; _roll=0; }
        internal static int RollOrNative(RuleAttackRoll attack)
        {
            var scope=_current; WhiteoutAttackObservation record;
            if(scope!=null && !scope._disposed && Thread.CurrentThread.ManagedThreadId==scope._thread && scope._owned.TryGetValue(attack,out record))
            {
                record.WhiteoutRollCalls++;
                if(ReferenceEquals(scope._pending,attack))
                {
                    int result=scope._roll; scope._pending=null; scope._roll=0; record.ForcedConsumed++; return result;
                }
            }
            return RulebookEvent.Dice.D100.Value;
        }
        internal static void Record(RuleAttackRoll attack,bool native,bool result,bool active,bool ignore,bool seeking,WhiteoutNativeStageDecision decision,string error)
        {
            var scope=_current; WhiteoutAttackObservation value;
            if(scope==null || scope._disposed || Thread.CurrentThread.ManagedThreadId!=scope._thread || attack==null || !scope._owned.TryGetValue(attack,out value)) return;
            value.StageCalls++; if(!native) value.NativeFailures++;
            value.NativeSucceeded=native; value.Result=result; value.Active=active; value.IgnoreConcealment=ignore; value.Seeking=seeking;
            value.WhiteoutRoll=decision?.Roll; value.DecisionFailedOpen=decision!=null && decision.FailedOpen; value.Error=error;
            var check=attack.ConcealmentCheck;
            value.NativeConcealment=check==null ? "none" : check.Concealment.ToString();
            value.NativeConcealmentRoll=check==null ? (int?)null : check.Roll.Value;
        }
        internal static void RecordFailure(RuleAttackRoll attack,bool native,Exception error)
        { try { Record(attack,native,native,false,false,false,null,error.ToString()); } catch(Exception) { } }
        public void Dispose()
        {
            if(_disposed) return;
            Guard();
            try
            {
                _pending=null; _roll=0;
                foreach(var attack in _owned.Keys) WhiteoutAttackRuntime.Forget(attack);
                CleanupVerified=_owned.Keys.All(attack=>!WhiteoutAttackRuntime.HasDecision(attack));
                _owned.Clear();
            }
            finally { _disposed=true; _current=null; }
        }
        internal static bool HasCurrent { get { return _current!=null; } }
        private sealed class AttackReferenceComparer : IEqualityComparer<RuleAttackRoll>
        {
            public bool Equals(RuleAttackRoll a,RuleAttackRoll b) { return ReferenceEquals(a,b); }
            public int GetHashCode(RuleAttackRoll value) { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
