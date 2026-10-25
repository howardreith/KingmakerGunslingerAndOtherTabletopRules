using System;
using System.Linq;
using Kingmaker.Blueprints;
using KingmakerGunslinger.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    // Explicit after-spawn boundary for a scoped untemplated outsider clone.
    // Retains the native summon marker and every unrelated buff/action.
    [Serializable]
    public sealed class ContextActionRemoveInheritedSummonTemplates : ContextAction
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff[] NativeTemplates = Array.Empty<BlueprintBuff>();
        public override string GetCaption() { return "Remove inherited native templates from this untemplated summon"; }

        public override void RunAction()
        {
            var unit = Target == null ? null : Target.Unit;
            if (unit == null || OwningBlueprint == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint) ||
                NativeTemplates == null || NativeTemplates.Length != ExpandedSummoningAbilityBuilder.NativeTemplateIds.Count ||
                NativeTemplates.Any(value => value == null) ||
                !NativeTemplates.Select(value => value.AssetGuid).OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(ExpandedSummoningAbilityBuilder.NativeTemplateIds.OrderBy(value => value, StringComparer.Ordinal)))
                throw new InvalidOperationException("Inherited-template removal requires its exact owner and four native identities.");
            foreach (Buff buff in unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Where(value =>
                NativeTemplates.Any(native => ReferenceEquals(native, value.Blueprint))).ToArray())
                unit.Descriptor.Buffs.RemoveFact(buff);
        }
    }
}
