using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.MagicCircle
{
    // Native AddKnown already handles new learning. PostLoad only restores
    // special-slot membership for already-known circles on an attached special
    // list, preserving the native school and spellbook ownership boundaries.
    internal static class MagicCircleSpecialistSpellCachePatches
    {
        internal static bool Installed { get; private set; }
        private static readonly MethodInfo AddSpecialMethod = typeof(Spellbook).GetMethod("AddSpecial",
            BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(int), typeof(BlueprintAbility) }, null);
        private static bool _reported;
        internal static void Install(ModContext context)
        {
            if (!context.FeatureModules.Active.MagicCircleSpells || Installed) return;
            MethodInfo postLoad = typeof(Spellbook).GetMethod("PostLoad", Type.EmptyTypes);
            string owner = context.ModId + ".MagicCircle.Specialist";
            var harmony = HarmonyInstance.Create(owner);
            try
            {
                if (postLoad == null || postLoad.IsStatic || postLoad.ReturnType != typeof(void) || postLoad.GetMethodBody() == null)
                    throw new InvalidOperationException("Native spellbook load contract differs.");
                if (AddSpecialMethod == null || AddSpecialMethod.ReturnType != typeof(void))
                    throw new InvalidOperationException("Native spellbook special-spell seam differs.");
                harmony.Patch(postLoad, null, new HarmonyMethod(typeof(MagicCircleSpecialistSpellCachePatches)
                    .GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static)), null);
                Installed = true;
                context.Logger.Info("magic-circle", "specialist-cache.hook-installed",
                    "native=Spellbook.PostLoad;scope=publishedMagicCircleSpells;additiveOnly=true");
            }
            catch (Exception exception)
            {
                Installed = false;
                try { harmony.UnpatchAll(owner); }
                catch (Exception cleanup) { context.Logger.Failure("magic-circle", "specialist-cache.hook-cleanup-failed", "Callback remains inert.", cleanup); }
                context.Logger.Failure("magic-circle", "specialist-cache.hook-unavailable",
                    "Existing specialist saves keep their cached special-spell membership; favorite-slot repair is inactive.", exception);
            }
        }
        private static void Postfix(Spellbook __instance)
        {
            try
            {
                if (!Installed || __instance == null || __instance.Blueprint == null || __instance.Blueprint.AllSpellsKnown) return;
                var spells = BlueprintBootstrap.MagicCircles;
                if (spells == null || BlueprintBootstrap.MagicCirclePublication == null) return;
                foreach (BlueprintAbility spell in spells.Select(circle => circle.Spell)
                    .Concat(KingmakerGunslinger.Blueprints.MagicCircleBlueprints.Families))
                {
                    if (spell == null) continue;
                    int level = __instance.GetSpellLevel(spell);
                    bool restore = KingmakerGunslinger.Spells.Teleportation.TeleportSpecialistSpellCachePolicy.ShouldRestoreSpecialMembership(
                        __instance.Blueprint.AllSpellsKnown, level >= 0,
                        level >= 0 && __instance.IsSpellInSpecialList(spell, level),
                        level >= 0 && __instance.GetSpecialSpells(level).Any(value => value != null && value.Blueprint == spell));
                    if (!restore) continue;
                    AddSpecialMethod.Invoke(__instance, new object[] { level, spell });
                    ModContext context;
                    if (ModContext.TryGet(out context))
                        context.Logger.Info("magic-circle", "specialist-cache.restored",
                            "book=" + __instance.Blueprint.name + ";owner=" + (__instance.Owner == null || __instance.Owner.Unit == null ?
                                "unknown" : __instance.Owner.Unit.UniqueId) + ";spell=" + spell.name + ";level=" + level.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            catch (Exception exception)
            {
                // A failed reconciliation must never break save loading; the
                // book simply keeps its serialized cache until the next load.
                if (_reported) return;
                _reported = true;
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("magic-circle", "specialist-cache.reconcile-failed",
                        "The spellbook keeps its serialized special-spell cache; favorite-slot repair is inactive for this load.", exception);
            }
        }
    }
}
