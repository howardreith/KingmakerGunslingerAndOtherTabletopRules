using System;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class SlingersLuckRequest
    {
        internal SlingersLuckRequest(int gunslingerLevel, int currentGrit,
            bool armed, SlingersLuckRollKind armedKind,
            SlingersLuckRollKind eventKind, bool firstEvaluation,
            int firstRoll, int secondRoll)
        {
            if (gunslingerLevel < 0 || currentGrit < 0 ||
                !Enum.IsDefined(typeof(SlingersLuckRollKind), armedKind) ||
                !Enum.IsDefined(typeof(SlingersLuckRollKind), eventKind) ||
                firstRoll < 1 || firstRoll > 20 || secondRoll < 1 ||
                secondRoll > 20)
                throw new ArgumentOutOfRangeException("Slinger's Luck request");
            GunslingerLevel = gunslingerLevel; CurrentGrit = currentGrit;
            Armed = armed; ArmedKind = armedKind; EventKind = eventKind;
            FirstEvaluation = firstEvaluation; FirstRoll = firstRoll;
            SecondRoll = secondRoll;
        }
        internal int GunslingerLevel { get; private set; }
        internal int CurrentGrit { get; private set; }
        internal bool Armed { get; private set; }
        internal SlingersLuckRollKind ArmedKind { get; private set; }
        internal SlingersLuckRollKind EventKind { get; private set; }
        internal bool FirstEvaluation { get; private set; }
        internal int FirstRoll { get; private set; }
        internal int SecondRoll { get; private set; }
    }
}
