using System;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.UnitLogic.Parts;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool _circlePreparedMutationAuthorized;

        private JObject CirclePreparationArtifact()
        {
            if (_loadedBuildIdentity == null) throw new InvalidOperationException("Loaded artifact identity is unavailable.");
            return new JObject { ["version"] = _loadedBuildIdentity.SemanticVersion,
                ["dllSha256"] = _loadedBuildIdentity.LoadedModuleSha256,
                ["mvid"] = _loadedBuildIdentity.ModuleVersionId, ["gitCommit"] = _loadedBuildIdentity.GitCommit };
        }

        private JObject CirclePreparationSave()
        {
            return new JObject(_workingSaveSmoke.WorkingIdentity.SafeFields.Select(pair => new JProperty(pair.Key, pair.Value)));
        }

        // Deliberately read-only: no area refresh, destruction tick, rule event,
        // merchant visit, grant reconciliation, Ensure<UnitPart>, or save call.
        private JObject ReadCirclePreparedFixture()
        {
            var game = Game.Instance; var actors = CircleSavedActors(); var circles = BlueprintBootstrap.MagicCircles;
            var carriers = circles.SelectMany(circle => CircleBuffs(actors[2], circle.Carrier))
                .OrderBy(buff => buff.Blueprint.AssetGuid, StringComparer.Ordinal)
                .ThenBy(buff => buff.Context.MaybeCaster?.Descriptor.CustomName, StringComparer.Ordinal).ToArray();
            var receipt = actors[1].Get<UnitPartMagicCirclePersistenceReceipt>();
            if (receipt == null) throw new InvalidOperationException("Prepared fixture receipt is absent.");
            var receiptJson = new JObject();
            foreach (var field in typeof(UnitPartMagicCirclePersistenceReceipt).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)) {
                object value = field.GetValue(receipt);
                receiptJson[field.Name] = value == null ? JValue.CreateNull() : JToken.FromObject(value, new JsonSerializer());
            }
            var control = actors[3].Buffs.Enumerable.Where(buff => buff.Blueprint.AssetGuid == "c0f4e1c24c9cd334ca988ed1bd9d201f");
            return new JObject {
                ["favoredOracle"] = CircleFavoredOracleState(actors[2]),
                ["area"] = game.CurrentlyLoadedArea.AssetGuid,
                ["actors"] = new JArray(actors.Select(unit => new JObject { ["role"] = unit.Descriptor.CustomName, ["id"] = unit.UniqueId,
                    ["blueprint"] = unit.Blueprint.AssetGuid, ["heldDelivery"] = unit.Get<UnitPartTouch>()?.Ability.Data.Blueprint.AssetGuid,
                    ["heldRoot"] = unit.Get<UnitPartTouch>()?.Ability.Data.StickyTouch?.Blueprint.AssetGuid,
                    ["books"] = new JArray(unit.Descriptor.Spellbooks.OrderBy(book => book.Blueprint.AssetGuid, StringComparer.Ordinal).Select(book => new JObject {
                        ["blueprint"] = book.Blueprint.AssetGuid, ["level"] = book.CasterLevel,
                        ["slots3"] = book.GetSpontaneousSlots(3), ["slots4"] = book.GetSpontaneousSlots(4),
                        ["knownCircles"] = new JArray(book.GetKnownSpells(3).Where(data => circles.Any(circle => ReferenceEquals(data.Blueprint, circle.Spell)))
                            .Select(data => data.Blueprint.AssetGuid).OrderBy(id => id, StringComparer.Ordinal)) })) })),
                ["carriers"] = CirclePersistedCarriers(actors[2], carriers),
                ["control"] = new JArray(control.Select(buff => new JObject { ["source"] = buff.Context.MaybeCaster?.UniqueId, ["endTimeTicks"] = buff.EndTime.Ticks })),
                ["marketReceipt"] = receiptJson, ["inventory"] = new JArray(CircleSavedItems(game.Player.Inventory)),
                ["gold"] = game.Player.Money
            };
        }

        private void AuthorizeCirclePreparedPhase(Action phase)
        {
            var binding = new MagicCirclePreparationBinding((string)_request.Parameters["preparationBinding"], _request.ExpectedModVersion);
            var observed = ReadCirclePreparedFixture();
            _circlePersistenceRecord["preparationAuthorization"] = new JObject {
                ["prepareRunId"] = binding.PrepareRunId, ["resultSha256"] = binding.ResultSha256,
                ["recordSha256"] = binding.RecordSha256, ["matched"] = false,
                ["producerIdentitySha256"] = binding.ProducerIdentitySha256,
                ["producerArtifact"] = binding.ProducerArtifact, ["consumerArtifact"] = CirclePreparationArtifact(),
                ["fixtureMutationStarted"] = false, ["observedBeforeMutation"] = observed.DeepClone() };
            try { binding.Execute(CirclePreparationSave(), CirclePreparationArtifact(), observed, () => {
                _circlePreparedMutationAuthorized = true;
                _circlePersistenceRecord["preparationAuthorization"]["matched"] = true;
                CirclePersistenceCheck("preparation-bound-before-mutation", true,
                    "immutable successful prepare result, artifact, exact working descriptor and complete prepared fixture match before any deliberate fixture mutation");
                _circlePersistenceRecord["preparationAuthorization"]["fixtureMutationStarted"] = true;
                phase();
            }); }
            catch {
                if (!_circlePreparedMutationAuthorized) {
                    var after = ReadCirclePreparedFixture();
                    _circlePersistenceRecord["preparationAuthorization"]["observedAfterRejection"] = after;
                    CirclePersistenceCheck("rejected-binding-preserves-fixture", JToken.DeepEquals(observed, after),
                        "rejected preparation leaves exact loaded actor, carrier, control, market receipt, inventory and gold identities unchanged");
                }
                throw;
            }
        }
    }
}
