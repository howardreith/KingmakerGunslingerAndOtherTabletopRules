using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Globalmap;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Spells.Teleportation;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private JObject CaptureTeleportPersistence()
        {
            var game = Game.Instance; var main = game.Player.MainCharacter.Value;
            var part = main.Descriptor.Get<UnitPartTeleportFamiliarity>();
            var manager = typeof(UnitDescriptor).GetField("m_Parts", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(main.Descriptor);
            var parts = (IDictionary)manager.GetType().GetField("m_Parts", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager);
            object owner = manager.GetType().GetField("m_Owner", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(manager);
            var owners = game.State.Units.Where(unit => unit.Descriptor.Get<UnitPartTeleportFamiliarity>() != null).ToArray();
            if (part == null || !ReferenceEquals(owner, main.Descriptor) || !ReferenceEquals(part.Owner, main.Descriptor) ||
                parts.Values.Cast<object>().Count(value => value is UnitPartTeleportFamiliarity) != 1 ||
                owners.Length != 1 || !ReferenceEquals(owners[0], main))
                throw new InvalidOperationException("Canonical owner graph has a duplicate, detached, preview or missing familiarity part.");
            string payload = (string)typeof(UnitPartTeleportFamiliarity).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(part);
            string boundary = (string)typeof(UnitPartTeleportFamiliarity).GetField("_explorationBoundary", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(part);
            var grantsValue = typeof(UnitPartTeleportFamiliarity).GetField("_scrollVendorGrants", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(part);
            var grants = grantsValue as System.Collections.Generic.List<string>;
            var state = part.Read(); part.ReadExplorationBoundary();
            if (!state.LegacyMigrationComplete) throw new InvalidOperationException("Persisted legacy migration is incomplete.");
            return new JObject { ["ownerId"] = main.UniqueId, ["ownerCount"] = owners.Length,
                ["gameId"] = game.Player.GameId, ["areaId"] = game.CurrentlyLoadedArea.AssetGuid,
                ["areaName"] = game.CurrentlyLoadedArea.name, ["party"] = new JArray(game.Player.Party.Select(unit => unit.UniqueId)),
                ["pointId"] = GlobalMapRules.State.PartyLocation.AssetGuid, ["miles"] = GlobalMapRules.State.MilesTravelled.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
                ["payload"] = payload, ["boundary"] = boundary, ["migrationComplete"] = state.LegacyMigrationComplete,
                ["scrollVendorGrants"] = new JArray(grants ?? new System.Collections.Generic.List<string>()),
                ["canonicalState"] = state.Serialize(), ["chainIds"] = new JArray(_teleportPersistenceChain) };
        }
        private IEnumerable<int> SaveTeleportPersistence()
        {
            var game = Game.Instance; var plan = _teleportPersistencePlan;
            if (plan.Phase == "D" || !game.SaveManager.IsSaveAllowed() || game.SaveManager.CommitInProgress ||
                Kingmaker.UI.SettingsUI.SettingsRoot.Instance.OnlyOneSave.CurrentValue)
                throw new InvalidOperationException("The exact new native manual save is not permitted in this state.");
            var requested = game.SaveManager.CreateNewSave(plan.OutputName);
            var lease = new GuardedDisposableSaveLease(requested, plan.Transaction, plan.Phase, game.SaveManager.SavePath, save =>
            {
                WriteTeleportationForensicJson(Path.Combine(_request.EvidenceDirectory, "teleportation-persistence-owned-save.json"),
                    new { schemaVersion = 1, runId = _request.RunId, transactionId = plan.Transaction, phase = plan.Phase,
                        name = save.Name, file = save.FileName, path = save.FolderName, gameId = save.GameId,
                        existedBeforePreparation = false, lifecycle = "native-prepared-before-write" });
            });
            _workingSaveSmoke.ArmDisposableSave(lease);
            bool completed = false;
            game.SaveGame(requested, () => { completed = true; });
            var watch = Stopwatch.StartNew();
            while (!completed || lease.Saved == null || game.SaveManager.CommitInProgress || lease.Saved.OperationState != SaveInfo.StateType.None ||
                LoadingProcess.Instance.IsLoadingInProcess || !File.Exists(lease.Saved.FolderName))
            {
                if (watch.Elapsed.TotalSeconds > 90) throw new InvalidOperationException("Native disposable save did not complete its exact disk commit.");
                yield return 0;
            }
            var saved = lease.Saved;
            JObject header; JToken party;
            using (var reader = saved.Saver.Clone())
            { header = JObject.Parse(reader.ReadHeader()); party = JToken.Parse(reader.ReadJson("party")); }
            string identity = typeof(UnitPartTeleportFamiliarity).FullName;
            var serialized = ((JContainer)party).DescendantsAndSelf().OfType<JProperty>().Where(property => property.Name.StartsWith(identity, StringComparison.Ordinal))
                .Select(property => property.Value as JObject).Where(value => value != null).ToArray();
            if (serialized.Length == 0) serialized = ((JContainer)party).DescendantsAndSelf().OfType<JObject>().Where(value =>
                ((string)value["$type"] ?? "").StartsWith(identity, StringComparison.Ordinal)).ToArray();
            PersistenceAssert("native-disk-payload", "Native campaign ZIP contains exactly one unchanged familiarity payload and boundary after removing cast fixtures",
                serialized.Length == 1 && (string)serialized[0]["_state"] == (string)_teleportPersistenceFinal["payload"] &&
                (string)serialized[0]["_explorationBoundary"] == (string)_teleportPersistenceFinal["boundary"] &&
                JToken.DeepEquals(_teleportPersistenceFinal, CaptureTeleportPersistence()) && lease.RoutineCount == 1 && !_workingSaveSmoke.WriteObserved,
                new { lease.RoutineCount, lease.StashedAreaCount, parts = serialized, actual = CaptureTeleportPersistence() });
            PersistenceAssert("native-disk-header", "Completed new native manual save has the exact campaign, area, party and transaction identity",
                (string)header["Name"] == plan.OutputName && (string)header["GameId"] == game.Player.GameId &&
                saved.Area == game.CurrentlyLoadedArea && saved.GameName == game.Player.MainCharacter.Value.CharacterName &&
                saved.PartyPortraits.Count == game.Player.Party.Count && saved.LoadedTimes == 0,
                new { header, path = saved.FolderName, saveName = saved.Name });
            _teleportPersistenceSaved = new JObject { ["name"] = saved.Name, ["file"] = saved.FileName, ["path"] = saved.FolderName,
                ["sha256"] = TeleportPersistencePlan.Hash(saved.FolderName), ["gameName"] = saved.GameName,
                ["gameId"] = saved.GameId, ["areaName"] = saved.Area.name, ["partyCount"] = saved.PartyPortraits.Count };
        }
    }
}
