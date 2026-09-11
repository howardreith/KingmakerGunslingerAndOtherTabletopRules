using System;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Specialist/favorite-slot eligibility is the per-book special-spell cache,
    // which is serialized in saves and derived only at school-feature activation
    // (AddSpecialList, from then-known spells) and at learn time (AddKnown, when
    // the spell is already in an attached special list). Spellbook.PostLoad
    // rebuilds only the known-level index; fact components do not re-run
    // OnFactActivate on load. Books whose Teleport knowledge predates the
    // Conjuration special-list publication therefore keep rejecting the spell in
    // the favorite slot even though the published list now contains it. This
    // load-time reconciliation restores, for exactly the published Teleport
    // spells, the membership native would have cached — additive, idempotent,
    // and inert unless the book knows the spell and its attached school list
    // contains it. No slots are granted, no spell is auto-learned, and other
    // schools', lists' and books' boundaries are untouched.
    internal static class TeleportSpecialistSpellCachePatches
    {
        internal static bool Installed { get; private set; }
        private static readonly MethodInfo AddSpecialMethod = typeof(Spellbook).GetMethod("AddSpecial",
            BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(int), typeof(BlueprintAbility) }, null);
        private static bool _reported;
        internal static void Install(ModContext context)
        {
            if (!context.FeatureModules.Active.TeleportationSpells || Installed) return;
            MethodInfo postLoad = typeof(Spellbook).GetMethod("PostLoad", Type.EmptyTypes);
            try
            {
                if (postLoad == null || postLoad.IsStatic || postLoad.ReturnType != typeof(void) || postLoad.GetMethodBody() == null)
                    throw new InvalidOperationException("Native spellbook load contract differs.");
                if (AddSpecialMethod == null || AddSpecialMethod.ReturnType != typeof(void))
                    throw new InvalidOperationException("Native spellbook special-spell seam differs.");
                context.Harmony.Patch(postLoad, null, new HarmonyMethod(typeof(TeleportSpecialistSpellCachePatches)
                    .GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static)), null);
                Installed = true;
                context.Logger.Info("teleportation", "specialist-cache.hook-installed",
                    "native=Spellbook.PostLoad;scope=publishedTeleportSpells;additiveOnly=true");
            }
            catch (Exception exception)
            {
                Installed = false;
                try { if (postLoad != null) context.Harmony.Unpatch(postLoad, HarmonyPatchType.Postfix, context.ModId); }
                catch (Exception cleanup) { context.Logger.Failure("teleportation", "specialist-cache.hook-cleanup-failed", "Callback remains inert.", cleanup); }
                context.Logger.Failure("teleportation", "specialist-cache.hook-unavailable",
                    "Existing specialist saves keep their cached special-spell membership; favorite-slot repair is inactive.", exception);
            }
        }
        private static void Postfix(Spellbook __instance)
        {
            try
            {
                if (!Installed || __instance == null || __instance.Blueprint == null || __instance.Blueprint.AllSpellsKnown) return;
                var spells = BlueprintBootstrap.Teleportation;
                if (spells == null) return;
                foreach (BlueprintAbility spell in new[] { spells.Teleport, spells.GreaterTeleport })
                {
                    if (spell == null) continue;
                    int level = __instance.GetSpellLevel(spell);
                    bool restore = TeleportSpecialistSpellCachePolicy.ShouldRestoreSpecialMembership(
                        __instance.Blueprint.AllSpellsKnown, level >= 0,
                        level >= 0 && __instance.IsSpellInSpecialList(spell, level),
                        level >= 0 && __instance.GetSpecialSpells(level).Any(value => value != null && value.Blueprint == spell));
                    if (!restore) continue;
                    AddSpecialMethod.Invoke(__instance, new object[] { level, spell });
                    ModContext context;
                    if (ModContext.TryGet(out context))
                        context.Logger.Info("teleportation", "specialist-cache.restored",
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
                    context.Logger.Failure("teleportation", "specialist-cache.reconcile-failed",
                        "The spellbook keeps its serialized special-spell cache; favorite-slot repair is inactive for this load.", exception);
            }
        }
    }
}
