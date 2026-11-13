using System;
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
        private readonly string _point;
        private bool _disposed;
        private bool _tripped;

        private FirearmCraftingFaultAdmission(UnitDescriptor caster, string point)
        { _caster = caster; _point = point; }

        internal bool Tripped { get { return _tripped; } }

        /// <summary>The fault points the service recognises. Named rather than
        /// positional so a moved call site cannot silently change which
        /// boundary is being exercised.</summary>
        internal const string AfterDebitBeforeOutput = "after-debit-before-output";
        internal const string AfterFirstOutput = "after-first-output";
        internal const string DuringCompensation = "during-compensation";

        internal static FirearmCraftingFaultAdmission Admit(UnitDescriptor caster, string point)
        {
            if (caster == null) throw new ArgumentNullException("caster");
            if (point != AfterDebitBeforeOutput && point != AfterFirstOutput &&
                point != DuringCompensation)
                throw new ArgumentOutOfRangeException("point",
                    "Unrecognised crafting fault point: " + point);
            if (_current != null) throw new InvalidOperationException(
                "A crafting fault admission is already live; they must not stack.");
            _current = new FirearmCraftingFaultAdmission(caster, point);
            return _current;
        }

        /// <summary>
        /// Throws only for the exact admitted caster at the exact admitted
        /// point. Every other caller - including any real player unit - passes
        /// through untouched.
        /// </summary>
        internal static void Trip(string point, UnitDescriptor caster)
        {
            FirearmCraftingFaultAdmission live = _current;
            if (live == null || live._disposed) return;
            if (!ReferenceEquals(live._caster, caster)) return;
            if (!string.Equals(live._point, point, StringComparison.Ordinal)) return;
            live._tripped = true;
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
