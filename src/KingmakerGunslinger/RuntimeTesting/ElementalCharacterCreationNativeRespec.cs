using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class ElementalCharacterCreationBaselineScenario
    {
        private readonly bool _nativeRespec;
        private bool _creatorPauseBefore;
        private UnitEntityData _respecOriginal;
        private string _respecOriginalId;
        private readonly List<UnitEntityData> _respecReplacements = new List<UnitEntityData>();
        private readonly Dictionary<UnitEntityData, UnitBody> _respecBodies = new Dictionary<UnitEntityData, UnitBody>();
        private readonly Dictionary<string, int> _respecSpent = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly JArray _respecPreviewMismatches = new JArray();
        private int _respecCallbacks;

        private UnitDescriptor CommittedCreatorOwner => _nativeRespec && _respecOriginal != null
            ? _respecOriginal.Descriptor : _unit.Descriptor;

        private void BeginNativeRespec()
        {
            if (_respecOriginal == null || ReferenceEquals(_respecOriginal, _mainBefore) ||
                !_respecOriginal.Descriptor.IsCustomCompanion() || _respecOriginal.UniqueId != _respecOriginalId ||
                !ReferenceEquals(_respecOriginal.HoldingState, Game.Instance.Player.CrossSceneState) ||
                Game.Instance.Player.RemoteCompanions.Count(value => ReferenceEquals(value.Value, _respecOriginal)) != 1)
                throw new InvalidOperationException("Native respec source is not the exact registered disposable companion.");
            _respecCallbacks = 0;
            _respecPreviewMismatches.Clear();
            var spent = new JArray();
            foreach (var resource in OwnedRacialResources())
            {
                int present = _respecOriginal.Descriptor.Resources.PersistantResources.Count(value =>
                    value != null && ReferenceEquals(value.Blueprint, resource));
                if (present == 0) continue;
                if (present != 1) throw new InvalidOperationException("Cannot spend a duplicated owned respec resource.");
                int before = _respecOriginal.Descriptor.Resources.GetResourceAmount(resource);
                if (before > 0) _respecOriginal.Descriptor.Resources.Spend(resource, before);
                int after = _respecOriginal.Descriptor.Resources.GetResourceAmount(resource);
                _respecSpent[resource.AssetGuid] = after;
                spent.Add(new JObject { ["guid"] = resource.AssetGuid, ["before"] = before, ["after"] = after });
                if (after != 0) throw new InvalidOperationException("Native owned-resource spend did not reach zero.");
            }
            _character["spentBeforeNativeRespec"] = spent;
            Game.Instance.Player.RespecCompanion(_respecOriginal, () => {
                ++_respecCallbacks; _successCallback = true;
                _character["nativeRespecCallback"] = new JObject {
                    ["count"] = _respecCallbacks, ["originalIdExact"] = _respecOriginal.UniqueId == _respecOriginalId,
                    ["originalDescriptorOwnerExact"] = ReferenceEquals(_respecOriginal.Descriptor.Unit, _respecOriginal),
                    ["originalLevel"] = _respecOriginal.Descriptor.Progression.CharacterLevel };
            });
            _controller = _build.LevelUpController;
            _unit = _controller?.Unit?.Unit;
            if (_unit == null || ReferenceEquals(_unit, _respecOriginal) || _unit.UniqueId != _respecOriginalId ||
                _controller.State.Mode != LevelUpState.CharBuildMode.Respec ||
                !ReferenceEquals(_controller, Game.Instance.UI.LevelUpController))
                throw new InvalidOperationException("Native Player respec did not create the exact distinct replacement controller.");
            _respecReplacements.Add(_unit);
            _respecBodies.Add(_unit, _unit.Descriptor.Body);
            _character["nativeRespecStarted"] = new JObject {
                ["originalId"] = _respecOriginalId, ["replacementId"] = _unit.UniqueId,
                ["distinctOriginalReplacement"] = !ReferenceEquals(_unit, _respecOriginal),
                ["distinctOriginalPreview"] = !ReferenceEquals(_controller.Preview, _respecOriginal.Descriptor),
                ["mode"] = _controller.State.Mode.ToString() };
        }

        private void EndNativeRespecVisit()
        {
            CloseOwnedCreatorController();
            CleanupCreatorItems();
            if (_respecOriginal == null)
            {
                _respecOriginal = _unit; _respecOriginalId = _unit.UniqueId;
                if (!ReferenceEquals(_unit.HoldingState, Game.Instance.Player.CrossSceneState))
                    throw new InvalidOperationException("The initial native respec companion was not registered.");
            }
            _unit = null;
        }

        private void CleanupNativeRespecActors()
        {
            CleanupCreatorItems();
            UnitEntityData pending = _unit;
            if (pending != null && !ReferenceEquals(pending, _respecOriginal) &&
                !_respecReplacements.Any(value => ReferenceEquals(value, pending)))
                _respecReplacements.Add(pending);
            if (_respecOriginal == null && pending != null && _committed &&
                ReferenceEquals(pending.HoldingState, Game.Instance.Player.CrossSceneState))
            {
                _respecOriginal = pending;
                _respecReplacements.RemoveAll(value => ReferenceEquals(value, pending));
            }
            if (_respecOriginal != null)
            {
                // This source was actually committed and registered in visit zero,
                // including when a later respec preview is canceled after a failure.
                _unit = _respecOriginal;
                bool visitCommitted = _committed; _committed = true;
                try { CleanupCreatorMembership(); }
                finally { _committed = visitCommitted; _unit = null; }
                _respecOriginal = null;
            }
            foreach (var replacement in _respecReplacements)
            {
                if (ReferenceEquals(replacement, _mainBefore) || _worldBefore.Any(value => ReferenceEquals(value, replacement)))
                    throw new InvalidOperationException("Respec cleanup cannot retire a preexisting actor.");
                // Native PrepareRespec deliberately nulls Body before copying into the
                // original. Restore only this discarded shell's exact captured,
                // now empty body so native/CotW Dispose can retire it normally.
                if (replacement.Descriptor.Body == null)
                {
                    UnitBody body;
                    if (!_respecBodies.TryGetValue(replacement, out body) || body == null ||
                        !ReferenceEquals(body.Owner, replacement.Descriptor) || body.Items.Any())
                        throw new InvalidOperationException("Cannot retire an ambiguous prepared respec body.");
                    typeof(UnitDescriptor).GetProperty("Body", Members).SetValue(replacement.Descriptor, body, null);
                }
                if (replacement.HoldingState != null && replacement.HoldingState.AllEntityData.Any(value => ReferenceEquals(value, replacement)))
                    replacement.HoldingState.RemoveEntityData(replacement);
                else replacement.Dispose();
            }
            _respecReplacements.Clear(); _respecBodies.Clear(); _unit = null;
            if (_crossSceneBefore != null) Game.Instance.IsPaused = _creatorPauseBefore;
        }

        private BlueprintAbilityResource[] OwnedRacialResources()
        {
            return RegressionRace.Heritages.Choices().Select(value => value.SlaResource)
                .Concat(RegressionRace.AlternateTraits.Traits().SelectMany(value => value.Mechanics())
                    .OfType<BlueprintAbilityResource>())
                .Concat(ElementalRaceIdentityCatalog.FeatSymbols().Select(symbol =>
                    BlueprintBootstrap.ElementalFeats.RequireSymbol<BlueprintScriptableObject>(symbol))
                    .OfType<BlueprintAbilityResource>()).Distinct().ToArray();
        }

        private bool ObserveRespecResourceAmount(string checkpoint, BlueprintAbilityResource blueprint, int count, int amount, bool committed)
        {
            int expected;
            if (!_respecSpent.TryGetValue(blueprint.AssetGuid, out expected)) expected = 1;
            bool exact = count == 0 || amount == expected;
            if (!exact && !committed)
            {
                _respecPreviewMismatches.Add(new JObject { ["checkpoint"] = checkpoint, ["guid"] = blueprint.AssetGuid,
                    ["expectedAmount"] = expected, ["observedAmount"] = amount });
                _character["nativeRespecPreviewMismatches"] = _respecPreviewMismatches.DeepClone();
            }
            // Preserve a mismatching preview as evidence, then exercise the full
            // native success callback before failing. No state is corrected here.
            return !committed || exact;
        }

        private void QualifyNativeRespecCallback()
        {
            if (!_nativeRespec) return;
            UnitDescriptor owner = CommittedCreatorOwner;
            var feature = BlueprintBootstrap.ElementalFeats.RequireFeature(ElementalFeatId.ElementalStrike);
            if (owner.Progression.Features.GetRank(feature) != 1)
                throw new InvalidOperationException("The existing elemental feat was not selected through the native creator.");
            var resources = new JArray();
            bool exact = true;
            foreach (var resource in OwnedRacialResources())
            {
                int count = owner.Resources.PersistantResources.Count(value => value != null && ReferenceEquals(value.Blueprint, resource));
                if (count == 0) continue;
                int amount = owner.Resources.GetResourceAmount(resource);
                int expected;
                if (!_respecSpent.TryGetValue(resource.AssetGuid, out expected)) expected = 1;
                resources.Add(new JObject { ["guid"] = resource.AssetGuid, ["count"] = count,
                    ["amount"] = amount, ["expectedAmount"] = expected });
                exact &= count == 1 && amount == expected;
            }
            _character["committedOwnedResources"] = resources;
            if (!exact) { Write(); throw new InvalidOperationException("Native respec refilled or duplicated an exact previously spent elemental resource."); }
            _character["nativeRespecPreviewMismatches"] = _respecPreviewMismatches.DeepClone();
            if (_respecPreviewMismatches.Count != 0)
                throw new InvalidOperationException("The native respec preview changed an exact spent elemental resource.");
            if (_respecOriginal == null) return;
            if (_respecCallbacks != 1 || _respecOriginal.UniqueId != _respecOriginalId ||
                _respecOriginal.Descriptor.Progression.CharacterLevel != 1 || _respecPreviewMismatches.Count != 0)
                throw new InvalidOperationException("The native respec callback or preview failed exact original/spent-state preservation.");
        }
    }
}
