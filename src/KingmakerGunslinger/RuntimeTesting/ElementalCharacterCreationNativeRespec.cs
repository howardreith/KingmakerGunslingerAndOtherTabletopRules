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
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.Controllers.Rest;
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
        private JObject _nereidOriginalBeforeRespec;
        private readonly Dictionary<ElementalAlternateTraitId, int> _respecBloodSpent = new Dictionary<ElementalAlternateTraitId, int>();

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
            SpendNativeRespecBlood();
            if (_nereidQualification) _nereidOriginalBeforeRespec = DescribeNereidOriginal();
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

        private JObject DescribeNereidOriginal()
        {
            var owner = _respecOriginal.Descriptor;
            var remembered = owner.Get<UnitPartElementalHeritageState>()?.CopyResourceAmounts();
            return new JObject {
                ["unitId"] = _respecOriginal.UniqueId,
                ["race"] = owner.Progression.Race.AssetGuid,
                ["gender"] = owner.Gender.ToString(),
                ["level"] = owner.Progression.CharacterLevel,
                ["stats"] = new JArray(AbilityStats.Select(stat => new JObject {
                    ["base"] = owner.Stats.GetStat(stat).BaseValue,
                    ["modified"] = owner.Stats.GetStat(stat).ModifiedValue })),
                ["racialFacts"] = new JArray(RegressionRace.Heritages.Choices().SelectMany(value =>
                    new[] { value.Marker, value.Affinity, value.SlaFeature }).Concat(RegressionRace.AlternateTraits.Traits()
                    .SelectMany(value => new[] { value.Marker, value.Provider }))
                    .Select(value => new JObject { ["guid"] = value.AssetGuid,
                        ["rank"] = owner.Progression.Features.GetRank(value) })),
                ["resources"] = new JArray(OwnedRacialResources().Select(value => new JObject {
                    ["guid"] = value.AssetGuid, ["count"] = owner.Resources.PersistantResources.Count(entry =>
                        entry != null && ReferenceEquals(entry.Blueprint, value)),
                    ["amount"] = owner.Resources.GetResourceAmount(value) })),
                ["remembered"] = new JArray((remembered ?? new Dictionary<string, int>())
                    .OrderBy(value => value.Key, StringComparer.Ordinal).Select(value =>
                        new JObject { ["guid"] = value.Key, ["amount"] = value.Value })),
                ["remote"] = Game.Instance.Player.RemoteCompanions.Count(value => ReferenceEquals(value.Value, _respecOriginal)),
                ["crossScene"] = ReferenceEquals(_respecOriginal.HoldingState, Game.Instance.Player.CrossSceneState)
            };
        }

        private void QualifyCanceledNereidRespec()
        {
            if (!_nereidQualification || !_nativeRespec || _respecOriginal == null ||
                _nereidOriginalBeforeRespec == null || !_controller.State.IsComplete() ||
                _controller.State.RemainingSelections() != 0 || _respecCallbacks != 0)
                throw new InvalidOperationException("Exact native Nereid cancellation prerequisites are absent.");
            CloseOwnedCreatorController(); // invokes this actual Player-respec controller's native Cancel
            var after = DescribeNereidOriginal();
            bool exact = _respecCallbacks == 0 && !_successCallback &&
                JToken.DeepEquals(_nereidOriginalBeforeRespec, after);
            _character["nativeRespecCancellation"] = new JObject {
                ["before"] = _nereidOriginalBeforeRespec.DeepClone(), ["after"] = after,
                ["exact"] = exact, ["successCallbacks"] = _respecCallbacks };
            _character["canceledNativeRespec"] = exact;
            _character["nativeCommitPerformed"] = false;
            if (!exact) throw new InvalidOperationException("Canceled native Nereid respec changed the original character or its spent resource ledger.");
            if (_raceIndex == _races.Length - 1) QualifyOrdinaryRestAfterNativeRespec(_respecOriginal.Descriptor);
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


        private void SpendNativeRespecBlood()
        {
            UnitEntityData owner = _respecOriginal;
            var rows = new JArray();
            _character["bloodBeforeNativeRespec"] = rows;
            foreach (var trait in RegressionRace.AlternateTraits.Traits().Where(value =>
                value.Provider.ComponentsArray.OfType<ElementalBloodDamageTrigger>().Any()))
            {
                if (!owner.Descriptor.HasFact(trait.Provider)) continue;
                var trigger = trait.Provider.ComponentsArray.OfType<ElementalBloodDamageTrigger>().Single();
                var capacity = owner.Descriptor.Get<UnitPartElementalBloodCapacity>();
                if (capacity == null || owner.Descriptor.Progression.CharacterLevel != 1 || owner.Descriptor.State.IsDead)
                    throw new InvalidOperationException("The owned first-level blood fixture is not ready.");
                int before = capacity.Spent(trait.Definition.Id);
                int expected;
                if (!_respecBloodSpent.TryGetValue(trait.Definition.Id, out expected)) expected = 0;
                if (before != expected) throw new InvalidOperationException("Native respec lost the previously observed blood expenditure.");
                if (before == 2)
                {
                    rows.Add(new JObject { ["trait"] = trait.Definition.Id.ToString(), ["before"] = before, ["after"] = before });
                    continue;
                }
                if (before != 0 || owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, trigger.HealingBuff)))
                    throw new InvalidOperationException("The native blood spend has an ambiguous initial capacity or buff.");
                TimeSpan clock = Game.Instance.TimeController.GameTime;
                bool paused = Game.Instance.IsPaused;
                int wounds = owner.Damage;
                var random = UnityEngine.Random.state;
                try
                {
                    Game.Instance.IsPaused = true;
                    var damage = Rulebook.Trigger(new RuleDealDamage(owner, owner,
                        new DamageBundle(new EnergyDamage(new DiceFormula(0, DiceType.D6), trigger.Energy)
                            { PreRolledValue = 3 })) { IsFake = false });
                    Buff buff = owner.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, trigger.HealingBuff));
                    TimeSpan tick = (TimeSpan)typeof(Buff).GetProperty("NextTickTime", Members).GetValue(buff, null);
                    if (tick < clock || tick > buff.EndTime || damage.ResultDamage == null ||
                        !damage.ResultDamage.Any(value => value.ValueWithoutReduction == 3))
                        throw new InvalidOperationException("The real matching damage rule did not schedule the owned blood heal.");
                    owner.Damage = 2;
                    Game.Instance.Player.GameTime = tick;
                    owner.Buffs.Tick();
                    int after = capacity.Spent(trait.Definition.Id);
                    bool absent = !owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, trigger.HealingBuff));
                    rows.Add(new JObject { ["trait"] = trait.Definition.Id.ToString(), ["before"] = before,
                        ["after"] = after, ["remaining"] = capacity.Remaining(trait.Definition.Id),
                        ["woundsAfterHeal"] = owner.Damage, ["nativeBuffRemoved"] = absent,
                        ["damageBeforeResistance"] = damage.DamageWithoutReduction });
                    Write();
                    if (after != 2 || capacity.Remaining(trait.Definition.Id) != 0 || owner.Damage != 0 || !absent)
                        throw new InvalidOperationException("The native blood tick did not exhaust exactly two points and retire its buff.");
                    _respecBloodSpent[trait.Definition.Id] = after;
                }
                finally
                {
                    owner.Damage = wounds;
                    Game.Instance.Player.GameTime = clock;
                    Game.Instance.IsPaused = paused;
                    UnityEngine.Random.state = random;
                }
            }
        }

        private bool ObserveNativeRespecBlood(string checkpoint, UnitDescriptor owner, bool committed)
        {
            if (!_nativeRespec) return true;
            var rows = new JArray();
            bool exact = true;
            var capacity = owner.Get<UnitPartElementalBloodCapacity>();
            foreach (var id in new[] { ElementalAlternateTraitId.FireInTheBlood,
                ElementalAlternateTraitId.StoneInTheBlood, ElementalAlternateTraitId.StormInTheBlood })
            {
                int expected;
                if (!_respecBloodSpent.TryGetValue(id, out expected)) expected = 0;
                int spent = capacity == null ? 0 : capacity.Spent(id);
                exact &= spent == expected;
                rows.Add(new JObject { ["trait"] = id.ToString(), ["spent"] = spent, ["expectedSpent"] = expected });
            }
            if (_character["nativeBloodCapacity"] == null) _character["nativeBloodCapacity"] = new JArray();
            ((JArray)_character["nativeBloodCapacity"]).Add(new JObject { ["checkpoint"] = checkpoint,
                ["committed"] = committed, ["exact"] = exact, ["counters"] = rows });
            if (!exact && !committed)
                _respecPreviewMismatches.Add(new JObject { ["checkpoint"] = checkpoint, ["bloodCounters"] = rows.DeepClone() });
            return !committed || exact;
        }

        private void QualifyOrdinaryRestAfterNativeRespec(UnitDescriptor owner)
        {
            if (_raceIndex != _races.Length - 1) return;
            var resources = OwnedRacialResources().Where(resource => owner.Resources.PersistantResources.Any(value =>
                value != null && ReferenceEquals(value.Blueprint, resource))).ToArray();
            RestController.ApplyRest(owner);
            var capacity = owner.Get<UnitPartElementalBloodCapacity>();
            bool exact = resources.All(resource => owner.Resources.GetResourceAmount(resource) == 1) &&
                _respecBloodSpent.Keys.All(id => capacity != null && capacity.Spent(id) == 0);
            _character["ordinaryRestAfterNativeRespec"] = new JObject { ["exact"] = exact,
                ["restoredResources"] = new JArray(resources.Select(resource => resource.AssetGuid)),
                ["resetBloodCounters"] = new JArray(_respecBloodSpent.Keys.Select(id => id.ToString())) };
            if (!exact) throw new InvalidOperationException("Ordinary rest after native respec failed to restore the owned daily budgets.");
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
            QualifyOrdinaryRestAfterNativeRespec(owner);
            if (_respecOriginal == null) return;
            if (_respecCallbacks != 1 || _respecOriginal.UniqueId != _respecOriginalId ||
                _respecOriginal.Descriptor.Progression.CharacterLevel != 1 || _respecPreviewMismatches.Count != 0)
                throw new InvalidOperationException("The native respec callback or preview failed exact original/spent-state preservation.");
        }
    }
}
