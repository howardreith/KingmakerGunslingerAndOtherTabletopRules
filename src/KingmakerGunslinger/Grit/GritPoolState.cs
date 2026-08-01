using System;

namespace KingmakerGunslinger.Grit
{
    internal sealed class GritPoolState : IEquatable<GritPoolState>
    {
        internal GritPoolState(int current, int maximum)
        {
            if (maximum < 1) throw new ArgumentOutOfRangeException(nameof(maximum));
            if (current < 0 || current > maximum)
                throw new ArgumentOutOfRangeException(nameof(current));
            Current = current;
            Maximum = maximum;
        }

        internal int Current { get; private set; }
        internal int Maximum { get; private set; }

        public bool Equals(GritPoolState other)
        {
            return other != null && Current == other.Current && Maximum == other.Maximum;
        }

        public override bool Equals(object obj) { return Equals(obj as GritPoolState); }

        public override int GetHashCode()
        {
            unchecked { return (Current * 397) ^ Maximum; }
        }

        public override string ToString()
        {
            return "grit=" + Current + "/" + Maximum;
        }
    }
}
