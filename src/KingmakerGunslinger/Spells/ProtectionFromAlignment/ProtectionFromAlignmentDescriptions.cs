using System;

namespace KingmakerGunslinger.Spells.ProtectionFromAlignment
{
    internal static class ProtectionFromAlignmentDescriptions
    {
        internal const string ExistingControlLimitation =
            "It does not remove or suppress a control effect that was already active when the protection was applied.";

        internal static string SpecificSpell(ProtectionAlignment alignment,
            bool communal)
        {
            string alignmentName = AlignmentName(alignment);
            string title = "Protection from " + ProtectionName(alignment) +
                (communal ? ", Communal" : string.Empty);
            string recipient = communal ? "Each affected ally" : "The target";
            return title + " wards " +
                (communal ? "allies" : "the target") + " against " +
                alignmentName + " creatures. " + recipient +
                " gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by " +
                alignmentName + " creatures. While this protection is active, it also prevents a new domination, charm, or comparable mental-control effect recognized by this mod from taking hold when its source is " +
                alignmentName + ". " + ExistingControlLimitation;
        }

        internal static string GenericSpell(bool communal)
        {
            string title = communal ?
                "Protection from Alignment, Communal" :
                "Protection from Alignment";
            string recipient = communal ? "Each affected ally" : "The target";
            return title +
                " lets the caster choose evil, good, law, or chaos. " +
                recipient +
                " gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by creatures of the selected alignment. While the resulting protection is active, it also prevents a new domination, charm, or comparable mental-control effect recognized by this mod from taking hold when its source has that alignment. " +
                ExistingControlLimitation;
        }

        internal static string Buff(ProtectionAlignment alignment)
        {
            string alignmentName = AlignmentName(alignment);
            return "This creature is warded against " + alignmentName +
                " creatures. It gains a +2 deflection bonus to Armor Class and a +2 resistance bonus on saving throws against attacks and effects created by " +
                alignmentName +
                " creatures. A new domination, charm, or comparable mental-control effect recognized by this mod cannot take hold when its source is " +
                alignmentName + ". " + ExistingControlLimitation;
        }

        private static string AlignmentName(ProtectionAlignment alignment)
        {
            if (alignment == ProtectionAlignment.Evil) return "evil";
            if (alignment == ProtectionAlignment.Good) return "good";
            if (alignment == ProtectionAlignment.Law) return "lawful";
            if (alignment == ProtectionAlignment.Chaos) return "chaotic";
            throw new ArgumentOutOfRangeException("alignment", alignment,
                "A single protection alignment is required.");
        }

        private static string ProtectionName(ProtectionAlignment alignment)
        {
            if (alignment == ProtectionAlignment.Evil) return "Evil";
            if (alignment == ProtectionAlignment.Good) return "Good";
            if (alignment == ProtectionAlignment.Law) return "Law";
            if (alignment == ProtectionAlignment.Chaos) return "Chaos";
            throw new ArgumentOutOfRangeException("alignment", alignment,
                "A single protection alignment is required.");
        }
    }
}
