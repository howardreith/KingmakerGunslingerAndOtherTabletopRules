namespace KingmakerGunslinger.BrownFur
{
    internal enum BrownFurStartupState
    {
        WaitingForCotw, WaitingForLibrary, Reconciling, Complete, Rejected
    }

    // Readiness is not compatibility. Lifecycle notifications, not a timer,
    // retry pending work; the existing structural resolver is authoritative
    // once construction and the package's committed library are available.
    internal sealed class BrownFurStartupGate
    {
        private bool _creationCompleted;
        internal BrownFurStartupState State { get; private set; }

        internal bool TryBegin(bool arcanistConstructed, bool creationCompleted, bool libraryReady)
        {
            _creationCompleted |= creationCompleted;
            if (State == BrownFurStartupState.Complete || State == BrownFurStartupState.Rejected ||
                State == BrownFurStartupState.Reconciling) return false;
            if (!arcanistConstructed && !_creationCompleted)
            {
                State = BrownFurStartupState.WaitingForCotw;
                return false;
            }
            if (!libraryReady)
            {
                State = BrownFurStartupState.WaitingForLibrary;
                return false;
            }
            State = BrownFurStartupState.Reconciling;
            return true;
        }

        internal void Complete() { State = BrownFurStartupState.Complete; }
        internal void Reject() { State = BrownFurStartupState.Rejected; }
        internal void AwaitLibrary() { State = BrownFurStartupState.WaitingForLibrary; }
    }
}
