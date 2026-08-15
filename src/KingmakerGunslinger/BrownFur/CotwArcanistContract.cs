using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class CotwArcanistContract
    {
        internal Assembly Assembly { get; set; }
        internal BlueprintCharacterClass ArcanistClass { get; set; }
        internal BlueprintProgression ArcanistProgression { get; set; }
        internal BlueprintSpellbook CastingSpellbook { get; set; }
        internal BlueprintSpellbook MemorizationSpellbook { get; set; }
        internal BlueprintAbilityResource Reservoir { get; set; }
        internal BlueprintFeatureSelection ExploitSelection { get; set; }
        internal BlueprintFeature MagicalSupremacy { get; set; }
        internal CotwSharedSpellsBridge SharedSpells { get; set; }
        internal CotwProgressionDecision ProgressionDecision { get; set; }
        internal CotwCompatibilityFingerprint Fingerprint { get; set; }
    }
}
