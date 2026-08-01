using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Classes
{
    internal sealed class GunslingerClassChassis
    {
        internal const int MaximumLevel = 20;
        internal const int HitDie = 10;
        internal const int SkillRanksPerLevel = 4;

        private readonly GunslingerClassLevel[] _levels;

        internal GunslingerClassChassis()
        {
            _levels = new GunslingerClassLevel[MaximumLevel];
            for (int level = 1; level <= MaximumLevel; level++)
            {
                _levels[level - 1] = new GunslingerClassLevel(
                    level, level, GoodSave(level), GoodSave(level), PoorSave(level));
            }
        }

        internal IReadOnlyList<GunslingerClassLevel> Levels
        {
            get { return Array.AsReadOnly(_levels); }
        }

        internal GunslingerClassLevel RequireLevel(int level)
        {
            if (level < 1 || level > MaximumLevel)
                throw new ArgumentOutOfRangeException("level");
            return _levels[level - 1];
        }

        internal static int GoodSave(int level)
        {
            if (level < 1 || level > MaximumLevel)
                throw new ArgumentOutOfRangeException("level");
            return 2 + (level / 2);
        }

        internal static int PoorSave(int level)
        {
            if (level < 1 || level > MaximumLevel)
                throw new ArgumentOutOfRangeException("level");
            return level / 3;
        }
    }
}
