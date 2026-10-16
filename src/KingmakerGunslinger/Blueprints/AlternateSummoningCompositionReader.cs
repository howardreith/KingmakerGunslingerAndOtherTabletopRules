using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.Blueprints
{
    /// <summary>
    /// Reads every spawn branch of a summon ability, in graph order, as the
    /// ordered after-spawn actions <see cref="AlternateSummoningComposition"/>
    /// checks. Read-only.
    /// </summary>
    internal static class AlternateSummoningCompositionReader
    {
        internal static IReadOnlyList<IReadOnlyList<ComposedActionView>> Branches(
            BlueprintAbility ability)
        {
            return AlternateSummoningGraph.Objects<ContextActionSpawnMonster>(
                    ability == null ? null : ability.ComponentsArray)
                .Select(spawn => (IReadOnlyList<ComposedActionView>)(
                    spawn.AfterSpawn == null || spawn.AfterSpawn.Actions == null ?
                        new GameAction[0] : spawn.AfterSpawn.Actions)
                    .Select(Read).ToList())
                .ToList();
        }

        internal static ComposedActionView Read(GameAction action)
        {
            var template = action as ContextActionApplySummonTemplateByCaster;
            if (template != null)
                return new ComposedActionView {
                    Kind = ComposedActionKind.Template,
                    Type = action.GetType().FullName,
                    CelestialTemplate = Guid(template.CelestialTemplate),
                    FiendishTemplate = Guid(template.FiendishTemplate),
                    CelestialSmite = Guid(template.CelestialSmite),
                    FiendishSmite = Guid(template.FiendishSmite),
                    NeutralFiendishMode = Guid(template.NeutralFiendishMode),
                    ReplacedNativeTemplateBuffs = (template.ReplacedNativeTemplateBuffs ??
                        new Kingmaker.UnitLogic.Buffs.Blueprints.BlueprintBuff[0])
                        .Select(value => value == null ? null : value.AssetGuid).ToArray()
                };
            var alignment = action as ContextActionSetSummonAlignment;
            if (alignment != null)
                return new ComposedActionView {
                    Kind = ComposedActionKind.Alignment,
                    Type = action.GetType().FullName,
                    AlignmentMode = alignment.Mode.ToString()
                };
            return ComposedActionView.Other(action == null ? "null" : action.GetType().FullName);
        }

        /// <summary>Null when the child keeps the composition contract
        /// against its canonical child; for a child that should compose
        /// nothing, pass a null canonical.</summary>
        internal static string Verify(BlueprintAbility child, BlueprintAbility canonical)
        {
            ComposedActionView expected = null;
            if (canonical != null)
            {
                string inconsistency;
                expected = AlternateSummoningComposition.Expected(Branches(canonical),
                    out inconsistency);
                if (inconsistency != null) return inconsistency;
            }
            return AlternateSummoningComposition.Verify(Branches(child), expected);
        }

        private static string Guid(BlueprintScriptableObject value)
        {
            return value == null ? null : value.AssetGuid;
        }
    }
}
