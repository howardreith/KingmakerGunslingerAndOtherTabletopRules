using System;
using System.Collections.Generic;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Gunsmithing
{
    /// <summary>
    /// GS-AUD-0036. A deliberately tiny fault seam for the disposable crafting
    /// scenario, and nothing else.
    ///
    /// Why it has to exist. The service's compensation can only be observed if
    /// a transaction fails after it has already changed money or inventory.
    /// Those are real native mutations, and there is no existing way to make
    /// one fail: a fault injected after the service's own bookkeeping would
    /// prove nothing about the native boundary, which is the thing in question.
    ///
    /// Why it cannot fire in play. An admission names the EXACT caster
    /// descriptor it applies to, and <see cref="Trip"/> throws only when the
    /// live admission's caster is that same object. A player's descriptor is
    /// never that object: the scenario admits only the disposable ChargenUnit
    /// descriptor it created moments earlier. There is no blueprint, setting, saved field or
    /// console path that constructs an admission - it is internal, it is only
    /// ever created by the runtime scenario, and it is disarmed in a finally.
    /// Nothing is serialized, so no save can carry one.
    ///
    /// It also refuses to stack: arming a second admission while one is live
    /// throws, so a leaked admission cannot silently extend over a later
    /// transaction.
    /// </summary>
    internal sealed class FirearmCraftingFaultAdmission : IDisposable
    {
        private static FirearmCraftingFaultAdmission _current;
        private readonly UnitDescriptor _caster;
        private readonly string _trigger;
        private readonly bool _faultCompensation;
        private readonly List<string> _tripped = new List<string>();
        private bool _disposed;

        private FirearmCraftingFaultAdmission(UnitDescriptor caster, string trigger,
            bool faultCompensation)
        {
            _caster = caster; _trigger = trigger; _faultCompensation = faultCompensation;
        }

        internal bool Tripped { get { return _tripped.Count > 0; } }
        internal bool TrippedAt(string point) { return _tripped.Contains(point); }
        internal string TrippedPoints
        { get { return string.Join("+", _tripped.ToArray()); } }

        /// <summary>The fault points the service recognises. Named rather than
        /// positional so a moved call site cannot silently change which
        /// boundary is being exercised.</summary>
        internal const string AfterDebitBeforeOutput = "after-debit-before-output";
        internal const string AfterFirstOutput = "after-first-output";
        internal const string DuringCompensation = "during-compensation";

        internal static FirearmCraftingFaultAdmission Admit(UnitDescriptor caster,
            string trigger)
        { return Admit(caster, trigger, false); }

        /// <summary>
        /// Admits one TRIGGER fault - a point on the forward path, which is the
        /// only kind that can make a transaction fail - and optionally a second
        /// fault inside the service's compensation.
        ///
        /// faultCompensation exists because <see cref="DuringCompensation"/> is
        /// NOT a trigger: that trip sits inside the service's catch block, so it
        /// is unreachable unless the transaction has already failed for some
        /// other reason. A run that admitted it alone observed exactly that -
        /// the transaction simply succeeded and the admission never fired - so
        /// admitting it as a trigger is refused here rather than left to produce
        /// a vacuous observation.
        /// </summary>
        internal static FirearmCraftingFaultAdmission Admit(UnitDescriptor caster,
            string trigger, bool faultCompensation)
        {
            if (caster == null) throw new ArgumentNullException("caster");
            if (string.Equals(trigger, DuringCompensation, StringComparison.Ordinal))
                throw new ArgumentOutOfRangeException("trigger",
                    DuringCompensation + " is not a trigger: the compensation trip is inside " +
                    "the service catch block and cannot be reached unless the transaction has " +
                    "already failed. Admit a forward-path trigger with faultCompensation:true.");
            if (trigger != AfterDebitBeforeOutput && trigger != AfterFirstOutput)
                throw new ArgumentOutOfRangeException("trigger",
                    "Unrecognised crafting fault trigger: " + trigger);
            if (_current != null) throw new InvalidOperationException(
                "A crafting fault admission is already live; they must not stack.");
            _current = new FirearmCraftingFaultAdmission(caster, trigger, faultCompensation);
            return _current;
        }

        /// <summary>
        /// Throws only for the exact admitted caster, and only at the admitted
        /// trigger or - when a compensation fault was admitted as well - inside
        /// compensation. Every other caller, including any real player unit,
        /// passes through untouched.
        /// </summary>
        internal static void Trip(string point, UnitDescriptor caster)
        {
            FirearmCraftingFaultAdmission live = _current;
            if (live == null || live._disposed) return;
            if (!ReferenceEquals(live._caster, caster)) return;
            if (string.Equals(point, DuringCompensation, StringComparison.Ordinal))
            {
                if (!live._faultCompensation) return;
            }
            else if (!string.Equals(live._trigger, point, StringComparison.Ordinal)) return;
            live._tripped.Add(point);
            throw new InvalidOperationException(
                "Admitted crafting fault at " + point + " for the disposable fixture.");
        }

        public void Dispose()
        {
            _disposed = true;
            if (ReferenceEquals(_current, this)) _current = null;
        }
    }
}
