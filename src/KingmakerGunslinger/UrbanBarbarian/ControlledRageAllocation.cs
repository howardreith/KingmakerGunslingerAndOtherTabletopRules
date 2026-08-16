using System;
using System.Globalization;

namespace KingmakerGunslinger.UrbanBarbarian
{
    internal sealed class ControlledRageAllocation :
        IEquatable<ControlledRageAllocation>
    {
        internal ControlledRageAllocation(int strength, int dexterity,
            int constitution)
        {
            Strength = strength;
            Dexterity = dexterity;
            Constitution = constitution;
            ValidateValue(strength, "strength");
            ValidateValue(dexterity, "dexterity");
            ValidateValue(constitution, "constitution");
            if (Total != 4 && Total != 6 && Total != 8)
                throw new ArgumentOutOfRangeException("strength",
                    "A Controlled Rage allocation must total +4, +6, or +8.");
        }

        internal int Strength { get; private set; }
        internal int Dexterity { get; private set; }
        internal int Constitution { get; private set; }
        internal int Total { get { return Strength + Dexterity + Constitution; } }
        internal string Symbol
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "KMG.UrbanBarbarian.Allocation.T{0}.S{1}.D{2}.C{3}",
                    Total, Strength, Dexterity, Constitution);
            }
        }
        internal string Name
        {
            get
            {
                string value = string.Empty;
                Append(ref value, "STR", Strength);
                Append(ref value, "DEX", Dexterity);
                Append(ref value, "CON", Constitution);
                return value;
            }
        }
        internal string Description
        {
            get
            {
                return "While Controlled Rage is active, gain the selected " +
                    "morale bonuses: " + Name + ".";
            }
        }

        public bool Equals(ControlledRageAllocation other)
        {
            return other != null && Strength == other.Strength &&
                Dexterity == other.Dexterity && Constitution == other.Constitution;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ControlledRageAllocation);
        }

        public override int GetHashCode()
        {
            return (Strength * 31 + Dexterity) * 31 + Constitution;
        }

        public override string ToString() { return Name; }

        private static void ValidateValue(int value, string name)
        {
            if (value < 0 || value % 2 != 0)
                throw new ArgumentOutOfRangeException(name,
                    "Controlled Rage values must be nonnegative +2 increments.");
        }

        private static void Append(ref string destination, string label, int value)
        {
            if (value == 0) return;
            if (destination.Length > 0) destination += " / ";
            destination += label + " +" + value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
