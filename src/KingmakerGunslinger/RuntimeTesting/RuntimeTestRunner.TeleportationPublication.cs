using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Harmony12;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void ObserveTeleportationSpellPublication(List<RuntimeTestAssertion> assertions)
        {
            if ((_request.Scenario != RuntimeTestScenarioCatalog.ObserveTeleportationNativeContracts &&
                 _request.Scenario != RuntimeTestScenarioCatalog.ObserveFeatureModuleSettings) ||
                Game.Instance.CurrentlyLoadedArea != null)
                throw new InvalidOperationException("Spell publication probe requires its guarded unloaded main-menu request.");
            var spells = BlueprintBootstrap.Teleportation;
            if (spells == null) throw new InvalidOperationException("The three strategic spell identities failed bootstrap.");
            var abilities = new[] { spells.Teleport, spells.GreaterTeleport, spells.WordOfRecall };
            foreach (var ability in abilities) TeleportationSpellBlueprints.Validate(ability);
            bool enabled = _context.FeatureModules.Active.TeleportationSpells;
            MethodInfo[] familiarityTargets = { typeof(MapMovementController).GetMethod("MoveAlongEdge", BindingFlags.Static | BindingFlags.NonPublic),
                typeof(Player).GetMethod("OnAreaLoaded", Type.EmptyTypes) };
            // The installed Harmony12-to-Harmony2 bridge throws for an unpatched method.
            // Its registry is authoritative for absence; only inspect registered targets.
            var patchedMethods = new HashSet<MethodBase>(_context.Harmony.GetPatchedMethods());
            int familiarityHookCount = familiarityTargets.Sum(target => {
                if (target == null) throw new MissingMethodException("A native familiarity target is absent.");
                if (!patchedMethods.Contains(target)) return 0;
                Patches patches = _context.Harmony.GetPatchInfo(target);
                return patches == null ? 0 : patches.Prefixes.Concat(patches.Postfixes).Concat(patches.Transpilers)
                    .Count(value => value.owner == _context.ModId && value.patch != null &&
                        value.patch.DeclaringType == typeof(Spells.Teleportation.TeleportFamiliarityPatches));
            });
            int destinationHookCount = new[] { "OnLocationSelect", "FillDialogInfoLocation", "Hide", "Dispose" }.Sum(name => {
                MethodInfo target = typeof(Kingmaker.UI.GlobalMap.GlobalMapMessageBox).GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (target == null) throw new MissingMethodException("Native destination target " + name);
                if (!patchedMethods.Contains(target)) return 0;
                Patches patches = _context.Harmony.GetPatchInfo(target);
                return patches == null ? 0 : patches.Prefixes.Concat(patches.Postfixes).Concat(patches.Transpilers)
                    .Count(value => value.owner == _context.ModId && value.patch != null &&
                        value.patch.DeclaringType == typeof(Spells.Teleportation.WorldMapPointSpellActionPatches));
            });
            bool travelExists = BlueprintBootstrap.Library.BlueprintsByAssetId.ContainsKey(TeleportationSpellListPublication.TravelListId);
            var targets = new List<TeleportationPublicationProbeTarget> {
                TeleportationProbeTarget(TeleportationSpellListPublication.WizardListId, 5, spells.Teleport),
                TeleportationProbeTarget(TeleportationSpellListPublication.WizardListId, 7, spells.GreaterTeleport),
                TeleportationProbeTarget(TeleportationSpellListPublication.ClericListId, 6, spells.WordOfRecall),
                TeleportationProbeTarget(TeleportationSpellListPublication.DruidListId, 8, spells.WordOfRecall)
            };
            if (travelExists) {
                targets.Add(TeleportationProbeTarget(TeleportationSpellListPublication.TravelListId, 5, spells.Teleport));
                targets.Add(TeleportationProbeTarget(TeleportationSpellListPublication.TravelListId, 7, spells.GreaterTeleport));
            }
            var cache = typeof(SpellLevelList).GetField("m_SpellsFiltered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (cache == null) throw new MissingFieldException("SpellLevelList.m_SpellsFiltered");
            string path = Path.Combine(_request.EvidenceDirectory, "teleportation-spell-publication.json");
            bool originalPublication = targets.All(value => value.Level.Spells.Count(spell => spell != null &&
                spell.AssetGuid == value.Spell.AssetGuid) == (enabled ? 1 : 0));
            bool duplicateSafe = !enabled;
            bool exactRollback = !enabled;
            bool restored = false;
            var physical = targets.Select(value => value.Level).Distinct().ToArray();
            var originalLists = physical.Select(value => value.Spells).ToArray();
            var originalCaches = physical.Select(value => cache.GetValue(value)).ToArray();
            try {
                if (enabled) {
                    var again = TeleportationSpellListPublication.Publish(BlueprintBootstrap.Library, spells);
                    duplicateSafe = again.ChangedLevelCount == 0 && physical.Select((value, index) =>
                        ReferenceEquals(value.Spells, originalLists[index]) && ReferenceEquals(cache.GetValue(value), originalCaches[index])).All(value => value);
                    // Disposable in-memory list fixture, never a character grant or save mutation.
                    for (int index = 0; index < physical.Length; index++) {
                        physical[index].Spells = originalLists[index].Where(value => !abilities.Any(spell => spell.AssetGuid == value.AssetGuid)).ToList();
                        cache.SetValue(physical[index], null);
                    }
                    var fixtureLists = physical.Select(value => value.Spells).ToArray();
                    var fixtureCaches = physical.Select(value => cache.GetValue(value)).ToArray();
                    var fixture = TeleportationSpellListPublication.Publish(BlueprintBootstrap.Library, spells);
                    bool addedExactly = targets.All(value => value.Level.Spells.Count(spell => ReferenceEquals(spell, value.Spell)) == 1);
                    fixture.Rollback();
                    exactRollback = addedExactly && physical.Select((value, index) =>
                        ReferenceEquals(value.Spells, fixtureLists[index]) && ReferenceEquals(cache.GetValue(value), fixtureCaches[index])).All(value => value);
                }
            }
            finally {
                for (int index = 0; index < physical.Length; index++) {
                    physical[index].Spells = originalLists[index];
                    cache.SetValue(physical[index], originalCaches[index]);
                }
                restored = physical.Select((value, index) => ReferenceEquals(value.Spells, originalLists[index]) &&
                    ReferenceEquals(cache.GetValue(value), originalCaches[index])).All(value => value);
                if (!restored) throw new InvalidOperationException("Strategic spell publication probe could not restore original native list/cache instances.");
            }
            WriteTeleportationForensicJson(path, new { runId = _request.RunId, enabled, travelExists, destinationHookCount,
                claims = "Native blueprint and spell-list publication/rollback only; no spell was cast, learned or prepared.",
                spells = abilities.Select(value => new { id = value.AssetGuid, name = value.Name,
                    type = value.Type.ToString(), action = value.ActionType.ToString(), icon = value.Icon != null,
                    materialData = value.MaterialComponent != null, parentAbsent = value.Parent == null,
                    value.ActionBarAutoFillIgnored, metamagic = value.AvailableMetamagic.ToString(),
                    localTargets = value.CanTargetPoint || value.CanTargetFriends || value.CanTargetEnemies || value.CanTargetSelf,
                    components = value.ComponentsArray.Select(component => component.GetType().FullName).ToArray() }).ToArray(),
                targets = targets.Select(value => new { listId = value.ListId, level = value.Level.SpellLevel,
                    spell = value.Spell.AssetGuid, exactReferences = value.Level.Spells.Count(spell => ReferenceEquals(spell, value.Spell)) }).ToArray(),
                rollbackExercised = enabled, duplicateSafe, exactRollback, originalInstancesRestored = restored, familiarityHookCount });
            assertions.Add(Assertion("teleportation-three-real-spell-blueprints", "three exact Conjuration/standard strategic blueprints", "count=" + abilities.Length,
                abilities.Select(value => value.AssetGuid).Distinct().Count() == 3, path));
            assertions.Add(Assertion("teleportation-module-spell-publication", "exact base/domain levels match saved module intent", "enabled=" + enabled + ";travelExists=" + travelExists,
                originalPublication && (BlueprintBootstrap.TeleportationPublication != null) == enabled, path));
            assertions.Add(Assertion("teleportation-familiarity-module-hooks", "ordinary-arrival hooks match module intent",
                "enabled=" + enabled + ";installed=" + Spells.Teleportation.TeleportFamiliarityPatches.Installed + ";actualHooks=" + familiarityHookCount,
                enabled == Spells.Teleportation.TeleportFamiliarityPatches.Installed && familiarityHookCount == (enabled ? 2 : 0), path));
            assertions.Add(Assertion("teleportation-destination-module-hooks", "native destination hooks match module intent; no hooks OFF",
                "enabled=" + enabled + ";installed=" + Spells.Teleportation.WorldMapPointSpellActionPatches.Installed + ";actualHooks=" + destinationHookCount,
                enabled == Spells.Teleportation.WorldMapPointSpellActionPatches.Installed && destinationHookCount == (enabled ? 4 : 0), path));
            assertions.Add(Assertion("teleportation-publication-duplicate-safe", enabled ? "native list/cache identity unchanged by repeated publication" : "not exercised while module OFF",
                enabled ? "passed=" + duplicateSafe : "module OFF; no publication invoked", duplicateSafe, path));
            assertions.Add(Assertion("teleportation-publication-exact-rollback", enabled ? "exact prior native list/cache references restored" : "not exercised while module OFF",
                enabled ? "passed=" + exactRollback + ";cleanup=" + restored : "module OFF; original lists retained",
                exactRollback && restored, path));
        }
        private static TeleportationPublicationProbeTarget TeleportationProbeTarget(string id, int level, BlueprintAbility spell)
        {
            var list = BlueprintLibraryLookup.RequireExact<BlueprintSpellList>(BlueprintBootstrap.Library, id, "native strategic spell list observation");
            var entry = list.SpellsByLevel.Single(value => value != null && value.SpellLevel == level);
            return new TeleportationPublicationProbeTarget { ListId = id, Level = entry, Spell = spell };
        }
        private sealed class TeleportationPublicationProbeTarget
        {
            internal string ListId;
            internal SpellLevelList Level;
            internal BlueprintAbility Spell;
        }
    }
}
