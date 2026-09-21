using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.MagicCircle
{
    internal static class MagicCircleFamilySpellListPatch
    {
        internal static void Install(ModContext context)
        {
            if (!context.FeatureModules.Active.MagicCircleSpells) return;
            var method = typeof(BlueprintAbility).GetMethod("IsInSpellList", new[] { typeof(BlueprintSpellList) });
            if (method == null || method.IsStatic || method.ReturnType != typeof(bool) || method.GetMethodBody() == null)
                throw new InvalidOperationException("Native spell-list membership contract changed.");
            HarmonyInstance.Create(context.ModId + ".MagicCircle.Family").Patch(method, null,
                new HarmonyMethod(typeof(MagicCircleFamilySpellListPatch).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic)), null);
        }

        private static void Postfix(BlueprintAbility __instance, BlueprintSpellList __0, ref bool __result)
        {
            if (__result || __0 == null || BlueprintBootstrap.MagicCirclePublication == null || BlueprintBootstrap.MagicCircles == null) return;
            __result = MagicCircleFamilyAccess.ContainsVariant(__instance,
                BlueprintBootstrap.MagicCircles.Select(circle => circle.Spell), MagicCircleBlueprints.Families,
                (__0.SpellsByLevel ?? Array.Empty<SpellLevelList>()).Where(level => level != null && level.SpellLevel == 3)
                    .SelectMany(level => level.Spells ?? Enumerable.Empty<BlueprintAbility>()),
                family => family.Variants);
        }
    }
}
