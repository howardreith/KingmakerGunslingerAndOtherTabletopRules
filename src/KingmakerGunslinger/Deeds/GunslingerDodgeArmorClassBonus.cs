using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using System.Threading;

namespace KingmakerGunslinger.Deeds
{
    /// <summary>
    /// Owns the exact +2 Dodge modifier granted by Gunslinger's Dodge.
    ///
    /// A dedicated owned component is intentionally used instead of the generic
    /// AddStatBonus component.  The live game repeatedly accepted the buff fact
    /// while failing to expose any AC change.  This mirrors the already-qualified
    /// Nimble AC implementation and makes modifier creation/removal explicit at
    /// the buff lifecycle boundary without introducing an attack-time patch.
    /// </summary>
    public sealed class GunslingerDodgeArmorClassBonus :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        internal const int Bonus = 2;

        private ModifiableValue.Modifier _modifier;
        private static long _turnedOn;
        private static long _turnedOff;
        private static long _activeModifiers;

        internal static long TurnedOn { get { return Interlocked.Read(ref _turnedOn); } }
        internal static long TurnedOff { get { return Interlocked.Read(ref _turnedOff); } }
        internal static long ActiveModifiers
        { get { return Interlocked.Read(ref _activeModifiers); } }

        public override void OnTurnOn()
        {
            Interlocked.Increment(ref _turnedOn);
            Remove();
            if (Owner == null || Owner.Stats == null) return;
            _modifier = Owner.Stats.AC.AddModifier(
                Bonus,
                Fact,
                GetType().FullName,
                ModifierDescriptor.Dodge);
            if (_modifier != null) Interlocked.Increment(ref _activeModifiers);
        }

        public override void OnTurnOff()
        {
            Interlocked.Increment(ref _turnedOff);
            Remove();
        }

        internal static void ResetDiagnostics()
        {
            Interlocked.Exchange(ref _turnedOn, 0);
            Interlocked.Exchange(ref _turnedOff, 0);
            Interlocked.Exchange(ref _activeModifiers, 0);
        }

        private void Remove()
        {
            if (_modifier != null)
            {
                if (Owner != null && Owner.Stats != null)
                    Owner.Stats.AC.RemoveModifier(_modifier);
                Interlocked.Decrement(ref _activeModifiers);
            }
            _modifier = null;
        }
    }
}
