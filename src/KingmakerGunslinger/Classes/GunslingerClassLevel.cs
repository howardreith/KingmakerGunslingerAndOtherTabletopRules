using System;
using System.Globalization;

namespace KingmakerGunslinger.Classes
{
    internal sealed class GunslingerClassLevel : IEquatable<GunslingerClassLevel>
    {
        internal GunslingerClassLevel(int level, int baseAttackBonus,
            int fortitude, int reflex, int will)
        {
            if (level < 1 || level > GunslingerClassChassis.MaximumLevel)
                throw new ArgumentOutOfRangeException("level");
            if (baseAttackBonus < 0 || fortitude < 0 || reflex < 0 || will < 0)
                throw new ArgumentOutOfRangeException("baseAttackBonus");
            Level = level;
            BaseAttackBonus = baseAttackBonus;
            Fortitude = fortitude;
            Reflex = reflex;
            Will = will;
        }

        internal int Level { get; private set; }
        internal int BaseAttackBonus { get; private set; }
        internal int Fortitude { get; private set; }
        internal int Reflex { get; private set; }
        internal int Will { get; private set; }

        public bool Equals(GunslingerClassLevel other)
        {
            return other != null && Level == other.Level &&
                BaseAttackBonus == other.BaseAttackBonus &&
                Fortitude == other.Fortitude && Reflex == other.Reflex &&
                Will == other.Will;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GunslingerClassLevel);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Level;
                hash = (hash * 397) ^ BaseAttackBonus;
                hash = (hash * 397) ^ Fortitude;
                hash = (hash * 397) ^ Reflex;
                return (hash * 397) ^ Will;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "level={0};bab={1};fort={2};ref={3};will={4}",
                Level, BaseAttackBonus, Fortitude, Reflex, Will);
        }
    }
}
