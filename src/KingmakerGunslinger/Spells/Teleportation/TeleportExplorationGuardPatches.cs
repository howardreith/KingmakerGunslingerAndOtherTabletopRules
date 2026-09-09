using System;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Globalmap;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // No attribute-based patch: the OFF module attaches no exploration hook.
    internal static class TeleportExplorationGuardPatches
    {
        internal static bool Installed { get; private set; }
        internal static void Install(ModContext context)
        {
            if (!context.FeatureModules.Active.TeleportationSpells || Installed) return;
            MethodInfo tick = typeof(LocationRevealController).GetMethod("Tick", Type.EmptyTypes);
            try
            {
                if (tick == null || tick.IsStatic || tick.ReturnType != typeof(void) || tick.GetMethodBody() == null)
                    throw new InvalidOperationException("Native stationary location-reveal tick differs.");
                context.Harmony.Patch(tick, new HarmonyMethod(typeof(TeleportExplorationGuardPatches)
                    .GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static)), null, null);
                Installed = true;
                context.Logger.Info("teleportation", "exploration.hook-installed",
                    "native=LocationRevealController.Tick;scope=saved-magical-arrival;ordinaryWalkingReleases=true");
            }
            catch (Exception exception)
            {
                Installed = false;
                try { if (tick != null) context.Harmony.Unpatch(tick, HarmonyPatchType.Prefix, context.ModId); }
                catch (Exception cleanup) { context.Logger.Failure("teleportation", "exploration.hook-cleanup-failed", "Callback remains inert.", cleanup); }
                context.Logger.Failure("teleportation", "exploration.hook-unavailable",
                    "Contextual casting fails closed; unrelated modules continue.", exception);
            }
        }
        private static bool Prefix()
        {
            if (!Installed || !TeleportFamiliarityRuntime.Enabled) return true;
            var game = Game.Instance;
            var owner = game == null || game.Player == null ? null : game.Player.MainCharacter.Value;
            var ledger = owner == null ? null : owner.Descriptor.Get<UnitPartTeleportFamiliarity>();
            if (ledger == null) return true;
            try
            {
                var map = game.Player.GlobalMap;
                var point = map == null || map.PartyPosition == null ? null : map.PartyPosition.Location;
                string areaId = game.CurrentlyLoadedArea == null ? null : game.CurrentlyLoadedArea.AssetGuid;
                bool walking = map != null && map.TravelData != null && map.TravelData.Walking;
                if (map != null && ledger.SuppressExploration(areaId, point == null ? null : point.AssetGuid, map.MilesTravelled, walking))
                    return false;
                // A real native travel command, changed point/map or mileage also
                // releases a boundary retained while the module was disabled.
                ledger.ClearExplorationBoundary();
                return true;
            }
            catch (Exception exception)
            {
                if (!ledger.ExplorationDiagnosticEmitted)
                {
                    ledger.ExplorationDiagnosticEmitted = true;
                    ModContext context;
                    if (ModContext.TryGet(out context)) context.Logger.Failure("teleportation", "exploration.saved-boundary-invalid",
                        "Native exploration remains blocked rather than revealing map from an unqualified magical arrival.", exception);
                }
                return false;
            }
        }
        internal static void MarkArrival(TeleportationWorldMapContext context, string destinationId)
        {
            if (!Installed || context == null || !context.Usable || context.Player.MainCharacter.Value == null)
                throw new InvalidOperationException("A qualified exploration guard is required for relocation.");
            var ledger = context.Player.MainCharacter.Value.Descriptor.Get<UnitPartTeleportFamiliarity>();
            if (ledger == null) throw new InvalidOperationException("No save-owned arrival boundary owner.");
            ledger.MarkMagicalArrival(new TeleportExplorationBoundary(Game.Instance.CurrentlyLoadedArea.AssetGuid, destinationId, context.Map.MilesTravelled));
        }
    }
}
