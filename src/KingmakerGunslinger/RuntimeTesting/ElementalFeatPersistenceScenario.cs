using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// Release B extension of the existing guarded race/heritage persistence
    /// transaction. It gives every fixture the exact race-applicable feat set,
    /// persists representative command-created buffs and exact-item temporary
    /// enchantments, observes them with publication disabled, and proves that
    /// native Respec removes the old fixture-owned feat state.
    /// </summary>
    internal static partial class GunslingerOutfitRenderScenario
    {
        internal sealed partial class ElementalRacePersistenceSession
        {
            private const string FeatPersistenceShortSwordGuid =
                "57c8994d1f1becf49ac4f642e5d8ca9d";
            private const int ScorchingPersistenceFixtureIndex = 17;
            private const int StrikePersistenceFixtureIndex = 23;

            private ElementalFeatBlueprintSet _featBlueprintSet;
            private BlueprintItemWeapon _featPersistenceShortSword;
            private readonly List<ItemEntityWeapon>
                _featPersistenceOwnedWeapons =
                    new List<ItemEntityWeapon>();
            private bool _featRegisteredExact;
            private bool _featSelectorExact;
            private JObject _preparedFeatTransientState = new JObject();
            private JObject _restoredSourceFeatObservation;
            private bool _prepareFeatPauseBefore;
            private bool _prepareFeatPauseApplied;

            private void InitializeFeatPersistence()
            {
                _featBlueprintSet = BlueprintBootstrap.ElementalFeats;
                if (_featBlueprintSet == null ||
                    _featBlueprintSet.RegisteredCount !=
                        ElementalRaceIdentityCatalog.FeatIdentityCount)
                    throw new InvalidOperationException(
                        "The complete registered elemental feat blueprint set is unavailable.");

                _featRegisteredExact = FeatIdentitiesRegisteredExact();
                _featSelectorExact = FeatSelectorStateExact();
                if (!_legacyMigration && !_verifyAbsent)
                    _featPersistenceShortSword = BlueprintLibraryLookup
                        .RequireExact<BlueprintItemWeapon>(
                            BlueprintBootstrap.Library,
                            FeatPersistenceShortSwordGuid,
                            "elemental feat persistence native shortsword");
            }

            private bool FeatIdentitiesRegisteredExact()
            {
                BlueprintScriptableObject[] blueprints =
                    ElementalRaceIdentityCatalog.FeatSymbols().Select(symbol =>
                        _featBlueprintSet.RequireSymbol<
                            BlueprintScriptableObject>(symbol)).ToArray();
                return blueprints.Length ==
                        ElementalRaceIdentityCatalog.FeatIdentityCount &&
                    blueprints.Distinct().Count() == blueprints.Length &&
                    blueprints.Select(value => value.AssetGuid).Distinct(
                        StringComparer.Ordinal).Count() == blueprints.Length &&
                    blueprints.All(value =>
                    {
                        BlueprintScriptableObject registered;
                        return value != null &&
                            BlueprintBootstrap.Library.BlueprintsByAssetId
                                .TryGetValue(value.AssetGuid,
                                    out registered) &&
                            ReferenceEquals(value, registered) &&
                            ReferenceEquals(ResourcesLibrary.TryGetBlueprint<
                                BlueprintScriptableObject>(value.AssetGuid),
                                value);
                    });
            }

            private bool FeatSelectorStateExact()
            {
                BlueprintFeatureSelection basic = BlueprintLibraryLookup
                    .RequireExact<BlueprintFeatureSelection>(
                        BlueprintBootstrap.Library,
                        ElementalFeatPublication.BasicFeatSelectionGuid,
                        "elemental persistence basic feat selection");
                BlueprintFeatureSelection fighter = BlueprintLibraryLookup
                    .RequireExact<BlueprintFeatureSelection>(
                        BlueprintBootstrap.Library,
                        ElementalFeatPublication
                            .FighterCombatFeatSelectionGuid,
                        "elemental persistence Fighter feat selection");
                BlueprintFeature[] all = _featBlueprintSet.AllFeats();
                BlueprintFeature[] combat = _featBlueprintSet.CombatFeats();
                bool enabled = _context.FeatureModules.Active.ElementalRaces;
                return FeatSurfaceExact(basic.Features, all, all, enabled) &&
                    FeatSurfaceExact(basic.AllFeatures, all, all, enabled) &&
                    FeatSurfaceExact(fighter.Features, combat, all,
                        enabled) &&
                    FeatSurfaceExact(fighter.AllFeatures, combat, all,
                        enabled);
            }

            private static bool FeatSurfaceExact(
                IEnumerable<BlueprintFeature> values,
                IEnumerable<BlueprintFeature> expected,
                IEnumerable<BlueprintFeature> allProjectFeats,
                bool enabled)
            {
                BlueprintFeature[] surface = (values ??
                    Enumerable.Empty<BlueprintFeature>()).ToArray();
                BlueprintFeature[] desired = enabled ? expected.ToArray() :
                    new BlueprintFeature[0];
                BlueprintFeature[] project = allProjectFeats.ToArray();
                int projectOccurrences = surface.Count(value => value !=
                    null && project.Any(candidate =>
                        ReferenceEquals(value, candidate) || string.Equals(
                            value.AssetGuid, candidate.AssetGuid,
                            StringComparison.Ordinal)));
                return projectOccurrences == desired.Length &&
                    desired.All(candidate => surface.Count(value => value !=
                        null && (ReferenceEquals(value, candidate) ||
                        string.Equals(value.AssetGuid, candidate.AssetGuid,
                            StringComparison.Ordinal))) == 1) &&
                    (!enabled || desired.All(candidate => surface.Any(value =>
                        ReferenceEquals(value, candidate))));
            }

            private BlueprintFeature[] ExpectedFeatFacts(
                ElementalPersistenceFixture fixture)
            {
                var ids = new List<ElementalFeatId>
                {
                    ElementalFeatId.ElementalStrike
                };
                switch (fixture.Blueprints.Definition.Kind)
                {
                    case ElementalRaceKind.Ifrit:
                        ids.Add(ElementalFeatId.ScorchingWeapons);
                        ids.Add(ElementalFeatId.InnerFlame);
                        ids.Add(ElementalFeatId.BlazingAura);
                        ids.Add(ElementalFeatId.Firesight);
                        break;
                    case ElementalRaceKind.Oread:
                        break;
                    case ElementalRaceKind.Sylph:
                        ids.Add(ElementalFeatId.AiryStep);
                        ids.Add(ElementalFeatId.WingsOfAir);
                        ids.Add(ElementalFeatId.CloudGazer);
                        ids.Add(ElementalFeatId.InnerBreath);
                        break;
                    case ElementalRaceKind.Undine:
                        ids.Add(ElementalFeatId.HydraulicManeuver);
                        ids.Add(ElementalFeatId.TritonPortal);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("fixture");
                }
                return ids.Select(_featBlueprintSet.RequireFeature).ToArray();
            }

            private JObject PrepareFeatPersistenceFixture(
                ElementalPersistenceFixture fixture, UnitEntityData unit)
            {
                foreach (BlueprintFeature feature in ExpectedFeatFacts(
                    fixture))
                    EnsureFeatPersistenceFact(unit.Descriptor, feature);
                JObject observation = ObserveFeatPersistence(fixture, unit,
                    true, false, "prepare-fixture-facts");
                return observation;
            }

            private JObject PrepareFeatPersistenceTransientState()
            {
                ElementalPersistenceFixture scorchingFixture = _fixtures
                    .Single(value => value.Index ==
                        ScorchingPersistenceFixtureIndex);
                ElementalPersistenceFixture strikeFixture = _fixtures
                    .Single(value => value.Index ==
                        StrikePersistenceFixtureIndex);
                UnitEntityData scorchingOwner = _createdUnits.Single(value =>
                    IsFixtureUnit(value, scorchingFixture));
                UnitEntityData strikeOwner = _createdUnits.Single(value =>
                    IsFixtureUnit(value, strikeFixture));

                EquipScorchingPersistenceWeapons(scorchingOwner);
                var activations = new JArray
                {
                    ExecuteFeatPersistenceAbility(scorchingOwner,
                        _featBlueprintSet.RequireSymbol<BlueprintAbility>(
                            ElementalRaceIdentityCatalog
                                .ScorchingWeaponsAbility)),
                    ExecuteFeatPersistenceAbility(strikeOwner,
                        _featBlueprintSet.RequireSymbol<BlueprintAbility>(
                            ElementalRaceIdentityCatalog
                                .ElementalStrikeAbility))
                };
                EnsurePreparePersistencePause();
                TimeSpan gameTimeAtPause = Kingmaker.Game.Instance
                    .TimeController.GameTime;
                JObject scorching = ObserveFeatPersistence(
                    scorchingFixture, scorchingOwner, true, true,
                    "prepare-immediately-before-save");
                JObject strike = ObserveFeatPersistence(strikeFixture,
                    strikeOwner, true, true,
                    "prepare-immediately-before-save");
                bool activationsExact = activations.All(value =>
                    value.Type == JTokenType.Object &&
                    ((JObject)value).Value<bool>("exact"));
                return new JObject
                {
                    { "activations", activations },
                    { "activationsExact", activationsExact },
                    { "pauseBefore", _prepareFeatPauseBefore },
                    { "pauseApplied", _prepareFeatPauseApplied },
                    { "gameTimeAtPause", gameTimeAtPause.ToString() },
                    { "scorching", scorching },
                    { "strike", strike },
                    { "exact", activationsExact &&
                        _prepareFeatPauseApplied &&
                        scorching.Value<bool>("exact") &&
                        strike.Value<bool>("exact") }
                };
            }

            private void EnsurePreparePersistencePause()
            {
                if (_prepareFeatPauseApplied) return;
                _prepareFeatPauseBefore = Kingmaker.Game.Instance.IsPaused;
                Kingmaker.Game.Instance.IsPaused = true;
                _prepareFeatPauseApplied = Kingmaker.Game.Instance.IsPaused;
                if (!_prepareFeatPauseApplied)
                    throw new InvalidOperationException("Guarded transient-state preparation did not pause the campaign clock.");
            }

            private bool RestorePrepareFeatPersistencePause()
            {
                if (!_prepareFeatPauseApplied) return true;
                Kingmaker.Game.Instance.IsPaused = _prepareFeatPauseBefore;
                bool exact = Kingmaker.Game.Instance.IsPaused ==
                    _prepareFeatPauseBefore;
                if (exact) _prepareFeatPauseApplied = false;
                if (_preparedFeatTransientState != null)
                {
                    _preparedFeatTransientState["pauseRestored"] = exact;
                    _preparedFeatTransientState["pauseRestoredValue"] =
                        Kingmaker.Game.Instance.IsPaused;
                }
                return exact;
            }

            private bool ReleaseLoadedFeatPersistencePause()
            {
                return _workingSaveSmoke.ReleaseLoadCompletionPause();
            }

            private void EnsureFeatPersistenceFact(UnitDescriptor owner,
                BlueprintFeature feature)
            {
                if (owner.HasFact(feature)) return;
                if (owner.AddFact(feature) == null ||
                    owner.Progression.Features.GetRank(feature) != 1)
                    throw new InvalidOperationException(
                        "The feat persistence fixture rejected " +
                        feature.name + ".");
            }

            private void EquipScorchingPersistenceWeapons(UnitEntityData unit)
            {
                if (unit.Body == null || unit.Body.PrimaryHand == null ||
                    unit.Body.SecondaryHand == null ||
                    unit.Body.PrimaryHand.MaybeItem != null ||
                    unit.Body.SecondaryHand.MaybeItem != null)
                    throw new InvalidOperationException(
                        "The Scorching Weapons persistence fixture requires two empty native hand slots.");
                var primary = new ItemEntityWeapon(
                    _featPersistenceShortSword);
                var secondary = new ItemEntityWeapon(
                    _featPersistenceShortSword);
                unit.Body.PrimaryHand.InsertItem(primary);
                unit.Body.SecondaryHand.InsertItem(secondary);
                if (!ReferenceEquals(unit.Body.PrimaryHand.MaybeWeapon,
                        primary) ||
                    !ReferenceEquals(unit.Body.SecondaryHand.MaybeWeapon,
                        secondary))
                    throw new InvalidOperationException(
                        "The Scorching Weapons persistence fixture rejected its exact native weapons.");
            }

            private JObject ExecuteFeatPersistenceAbility(
                UnitEntityData unit, BlueprintAbility blueprint)
            {
                Ability fact = unit.Descriptor.Abilities.GetAbility(blueprint);
                if (fact == null)
                    throw new InvalidOperationException(
                        "The feat persistence activation ability is absent: " +
                        blueprint.name + ".");
                var data = new AbilityData(fact);
                if (!data.IsAvailable)
                    throw new InvalidOperationException(
                        "The feat persistence activation ability is unavailable: " +
                        blueprint.name + ".");
                UnitUseAbility command;
                var cutscene = new Kingmaker.AreaLogic.Cutscenes
                    .CutsceneParametersContext();
                using (cutscene.Data)
                    command = new UnitUseAbility(data,
                        new TargetWrapper(unit));
                PropertyInfo executor = typeof(UnitCommand).GetProperty(
                    "Executor", BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic);
                MethodInfo setExecutor = executor == null ? null :
                    executor.GetSetMethod(true);
                if (setExecutor == null)
                    throw new MissingMethodException(
                        typeof(UnitCommand).FullName,
                        "set_Executor(UnitEntityData)");
                setExecutor.Invoke(command, new object[] { unit });
                command.IgnoreCooldown(TimeSpan.Zero);
                object canStart = command.CanStart;
                MethodInfo onAction = typeof(UnitUseAbility).GetMethod(
                    "OnAction", BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (onAction == null)
                    throw new MissingMethodException(
                        typeof(UnitUseAbility).FullName, "OnAction()");
                object commandResult = onAction.Invoke(command, null);
                AbilityExecutionProcess process = command.ExecutionProcess;
                if (process != null)
                {
                    for (int tick = 0; tick < 5000 && !process.IsEnded;
                        tick++) process.Tick();
                    if (!process.IsEnded)
                    {
                        process.InstantDeliver();
                        for (int tick = 0; tick < 5000 && !process.IsEnded;
                            tick++) process.Tick();
                    }
                    if (!process.IsEnded) process.Detach();
                }
                MethodInfo onEnded = typeof(UnitUseAbility).GetMethod(
                    "OnEnded", BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic, null,
                    new[] { typeof(bool) }, null);
                if (onEnded != null)
                    onEnded.Invoke(command, new object[] { false });
                bool exact = process != null && process.IsEnded;
                return new JObject
                {
                    { "abilityGuid", blueprint.AssetGuid },
                    { "canStart", canStart == null ? "<null>" :
                        canStart.ToString() },
                    { "commandResult", commandResult == null ? "<null>" :
                        commandResult.ToString() },
                    { "processPresent", process != null },
                    { "processEnded", process != null && process.IsEnded },
                    { "exact", exact }
                };
            }

            private JObject ObserveFeatPersistence(
                ElementalPersistenceFixture fixture, UnitEntityData unit,
                bool expectFeatFacts, bool expectShortEffects, string phase)
            {
                BlueprintFeature[] allFeats = _featBlueprintSet.AllFeats();
                BlueprintFeature[] expected = expectFeatFacts ?
                    ExpectedFeatFacts(fixture) : new BlueprintFeature[0];
                bool factsExact = allFeats.All(value =>
                    unit.Descriptor.Progression.Features.GetRank(value) ==
                        (expected.Contains(value) ? 1 : 0));

                BlueprintAbility[] allGranted = allFeats.SelectMany(value =>
                        (value.ComponentsArray ??
                            Array.Empty<BlueprintComponent>())
                        .OfType<AddFacts>()).SelectMany(value => value.Facts ??
                            Array.Empty<BlueprintUnitFact>())
                    .OfType<BlueprintAbility>().Distinct().ToArray();
                BlueprintAbility[] expectedGranted = expected.SelectMany(
                        value => (value.ComponentsArray ??
                            Array.Empty<BlueprintComponent>())
                        .OfType<AddFacts>()).SelectMany(value => value.Facts ??
                            Array.Empty<BlueprintUnitFact>())
                    .OfType<BlueprintAbility>().Distinct().ToArray();
                bool abilitiesExact = allGranted.All(value =>
                    (unit.Descriptor.Abilities.GetAbility(value) != null) ==
                        expectedGranted.Contains(value));

                BlueprintBuff strike = _featBlueprintSet.RequireSymbol<
                    BlueprintBuff>(ElementalRaceIdentityCatalog
                        .ElementalStrikeBuff);
                BlueprintBuff scorching = _featBlueprintSet.RequireSymbol<
                    BlueprintBuff>(ElementalRaceIdentityCatalog
                        .ScorchingWeaponsBuff);
                BlueprintBuff aura = _featBlueprintSet.RequireSymbol<
                    BlueprintBuff>(ElementalRaceIdentityCatalog
                        .BlazingAuraBuff);
                BlueprintBuff wings = _featBlueprintSet.RequireSymbol<
                    BlueprintBuff>(ElementalRaceIdentityCatalog
                        .WingsOfAirBuff);
                int strikeCount = CountFeatPersistenceBuff(unit, strike);
                int scorchingCount = CountFeatPersistenceBuff(unit,
                    scorching);
                int auraCount = CountFeatPersistenceBuff(unit, aura);
                int wingsCount = CountFeatPersistenceBuff(unit, wings);
                int expectedStrike = expectShortEffects && fixture.Index ==
                    StrikePersistenceFixtureIndex ? 1 : 0;
                int expectedScorching = expectShortEffects && fixture.Index ==
                    ScorchingPersistenceFixtureIndex ? 1 : 0;
                int expectedWings = expectFeatFacts &&
                    fixture.Blueprints.Definition.Kind ==
                        ElementalRaceKind.Sylph ? 1 : 0;
                bool buffsExact = strikeCount == expectedStrike &&
                    scorchingCount == expectedScorching && auraCount == 0 &&
                    wingsCount == expectedWings;

                ItemEntityWeapon primary = unit.Body == null ||
                        unit.Body.PrimaryHand == null ? null :
                    unit.Body.PrimaryHand.MaybeItem as ItemEntityWeapon;
                ItemEntityWeapon secondary = unit.Body == null ||
                        unit.Body.SecondaryHand == null ? null :
                    unit.Body.SecondaryHand.MaybeItem as ItemEntityWeapon;
                BlueprintWeaponEnchantment enchantment = _featBlueprintSet
                    .RequireSymbol<BlueprintWeaponEnchantment>(
                        ElementalRaceIdentityCatalog
                            .ScorchingWeaponsEnchantment);
                bool weaponsExpected = expectShortEffects && fixture.Index ==
                    ScorchingPersistenceFixtureIndex;
                ItemEnchantment primaryEffect = ExactFeatEnchantment(primary,
                    enchantment);
                ItemEnchantment secondaryEffect = ExactFeatEnchantment(
                    secondary, enchantment);
                if (weaponsExpected)
                {
                    RememberFeatPersistenceWeapon(primary);
                    RememberFeatPersistenceWeapon(secondary);
                }
                string primaryIdentityRejection;
                string secondaryIdentityRejection;
                string primaryId = OptionalFeatPersistenceItemId(primary,
                    out primaryIdentityRejection);
                string secondaryId = OptionalFeatPersistenceItemId(secondary,
                    out secondaryIdentityRejection);
                bool weaponsExact = weaponsExpected
                    ? primary != null && secondary != null &&
                        !ReferenceEquals(primary, secondary) &&
                        ReferenceEquals(primary.Blueprint,
                            _featPersistenceShortSword) &&
                        ReferenceEquals(secondary.Blueprint,
                            _featPersistenceShortSword) &&
                        primaryEffect != null && secondaryEffect != null &&
                        primaryEffect.IsTemporary &&
                        secondaryEffect.IsTemporary &&
                        !primaryEffect.RemoveOnUnequipItem &&
                        !secondaryEffect.RemoveOnUnequipItem &&
                        primaryEffect.EndTime >
                            Kingmaker.Game.Instance.TimeController.GameTime &&
                        secondaryEffect.EndTime >
                            Kingmaker.Game.Instance.TimeController.GameTime
                    : primary == null && secondary == null;

                UnitPartElementalFeatTransientState carrier = unit.Descriptor
                    .Get<UnitPartElementalFeatTransientState>();
                long nowTicks = Kingmaker.Game.Instance.TimeController
                    .GameTime.Ticks;
                bool strikeCarrierExact = expectedStrike == 1
                    ? carrier != null &&
                        carrier.ElementalStrikeEndTimeTicks > nowTicks
                    : carrier == null ||
                        carrier.ElementalStrikeEndTimeTicks == 0L;
                ItemEntityWeapon[] carrierWeapons = carrier == null ?
                    new ItemEntityWeapon[0] : carrier.ScorchingWeapons();
                bool scorchingCarrierExact = expectedScorching == 1
                    ? carrier != null &&
                        carrier.ScorchingWeaponsEndTimeTicks > nowTicks &&
                        carrier.ScorchingWeaponCount == 2 &&
                        carrierWeapons.Length == 2 &&
                        ReferenceEquals(carrierWeapons[0], primary) &&
                        ReferenceEquals(carrierWeapons[1], secondary)
                    : carrier == null ||
                        carrier.ScorchingWeaponsEndTimeTicks == 0L &&
                        carrier.ScorchingWeaponCount == 0 &&
                        carrierWeapons.Length == 0;
                bool carrierExact = strikeCarrierExact &&
                    scorchingCarrierExact;

                bool exact = factsExact && abilitiesExact && buffsExact &&
                    weaponsExact && carrierExact;
                return new JObject
                {
                    { "phase", phase },
                    { "expectedFeatCount", expected.Length },
                    { "actualFeatCount", allFeats.Count(value =>
                        unit.Descriptor.Progression.Features.GetRank(value) ==
                            1) },
                    { "featGuids", new JArray(expected.Select(value =>
                        value.AssetGuid)) },
                    { "factsExact", factsExact },
                    { "expectedGrantedAbilityCount",
                        expectedGranted.Length },
                    { "actualGrantedAbilityCount", allGranted.Count(value =>
                        unit.Descriptor.Abilities.GetAbility(value) != null) },
                    { "abilitiesExact", abilitiesExact },
                    { "strikeBuffCount", strikeCount },
                    { "scorchingBuffCount", scorchingCount },
                    { "blazingAuraBuffCount", auraCount },
                    { "wingsBuffCount", wingsCount },
                    { "buffsExact", buffsExact },
                    { "weaponsExpected", weaponsExpected },
                    { "primaryWeaponGuid", primary == null ? "" :
                        primary.Blueprint.AssetGuid },
                    { "secondaryWeaponGuid", secondary == null ? "" :
                        secondary.Blueprint.AssetGuid },
                    { "primaryItemId", primaryId },
                    { "secondaryItemId", secondaryId },
                    { "nativeItemIdentityAvailable",
                        !string.IsNullOrEmpty(primaryId) &&
                        !string.IsNullOrEmpty(secondaryId) },
                    { "primaryItemIdentityRejection",
                        primaryIdentityRejection },
                    { "secondaryItemIdentityRejection",
                        secondaryIdentityRejection },
                    { "primaryEnchantmentCount",
                        CountFeatEnchantment(primary, enchantment) },
                    { "secondaryEnchantmentCount",
                        CountFeatEnchantment(secondary, enchantment) },
                    { "weaponsExact", weaponsExact },
                    { "carrierPresent", carrier != null },
                    { "carrierStrikeEndTimeTicks", carrier == null ? 0L :
                        carrier.ElementalStrikeEndTimeTicks },
                    { "carrierScorchingEndTimeTicks", carrier == null ? 0L :
                        carrier.ScorchingWeaponsEndTimeTicks },
                    { "carrierScorchingWeaponCount", carrier == null ? 0 :
                        carrier.ScorchingWeaponCount },
                    { "carrierResolvedWeaponCount", carrierWeapons.Count(
                        value => value != null) },
                    { "strikeCarrierExact", strikeCarrierExact },
                    { "scorchingCarrierExact", scorchingCarrierExact },
                    { "carrierExact", carrierExact },
                    { "exact", exact }
                };
            }

            private JObject RemoveFeatPersistenceShortEffects(
                ElementalPersistenceFixture fixture, UnitEntityData unit)
            {
                ElementalFeatTransientRuntime.RemoveElementalStrike(
                    unit.Descriptor);
                ElementalFeatTransientRuntime.RemoveScorchingWeapons(
                    unit.Descriptor);
                BlueprintBuff strike = _featBlueprintSet.RequireSymbol<
                    BlueprintBuff>(ElementalRaceIdentityCatalog
                        .ElementalStrikeBuff);
                BlueprintBuff scorching = _featBlueprintSet.RequireSymbol<
                    BlueprintBuff>(ElementalRaceIdentityCatalog
                        .ScorchingWeaponsBuff);
                foreach (BlueprintBuff blueprint in new[]
                    { strike, scorching })
                {
                    Buff buff = unit.Descriptor.Buffs.GetBuff(blueprint);
                    if (buff != null) unit.Descriptor.Buffs.RemoveFact(buff);
                }

                BlueprintWeaponEnchantment enchantment = _featBlueprintSet
                    .RequireSymbol<BlueprintWeaponEnchantment>(
                        ElementalRaceIdentityCatalog
                            .ScorchingWeaponsEnchantment);
                ItemEntityWeapon[] weapons = new[]
                {
                    unit.Body.PrimaryHand.MaybeItem as ItemEntityWeapon,
                    unit.Body.SecondaryHand.MaybeItem as ItemEntityWeapon
                }.Where(value => value != null).Distinct().ToArray();
                foreach (ItemEntityWeapon weapon in weapons)
                    foreach (ItemEnchantment effect in weapon.Enchantments
                        .Where(value => value != null && !value.IsEnded &&
                            ReferenceEquals(value.Blueprint, enchantment))
                        .ToArray())
                        weapon.RemoveEnchantment(effect);
                if (fixture.Index == ScorchingPersistenceFixtureIndex)
                {
                    if (unit.Body.PrimaryHand.MaybeItem != null)
                        unit.Body.PrimaryHand.RemoveItem(false);
                    if (unit.Body.SecondaryHand.MaybeItem != null)
                        unit.Body.SecondaryHand.RemoveItem(false);
                    foreach (ItemEntityWeapon weapon in weapons)
                    {
                        _player.Inventory.Remove(weapon);
                        weapon.Dispose();
                    }
                }
                return ObserveFeatPersistence(fixture, unit, true, false,
                    "module-off-short-effect-cleanup");
            }

            private void CleanupFeatPersistenceEquipment(
                UnitEntityData unit)
            {
                if (unit == null || unit.Body == null ||
                    _featPersistenceShortSword == null)
                    return;
                ElementalPersistenceFixture fixture = _fixtures
                    .FirstOrDefault(value => string.Equals(unit.UniqueId,
                        value.UniqueId, StringComparison.Ordinal));
                if (fixture == null || fixture.Index !=
                    ScorchingPersistenceFixtureIndex)
                    return;

                var slots = new[]
                {
                    unit.Body.PrimaryHand,
                    unit.Body.SecondaryHand
                };
                foreach (var slot in slots)
                {
                    ItemEntityWeapon weapon = slot == null ? null :
                        slot.MaybeItem as ItemEntityWeapon;
                    if (weapon == null || !ReferenceEquals(weapon.Blueprint,
                            _featPersistenceShortSword))
                        continue;
                    RememberFeatPersistenceWeapon(weapon);
                    if (_prepare && _saveStarted)
                        continue;
                    if (_featBlueprintSet != null)
                    {
                        BlueprintWeaponEnchantment enchantment =
                            _featBlueprintSet.RequireSymbol<
                                BlueprintWeaponEnchantment>(
                                ElementalRaceIdentityCatalog
                                    .ScorchingWeaponsEnchantment);
                        foreach (ItemEnchantment effect in weapon.Enchantments
                            .Where(value => value != null && !value.IsEnded &&
                                ReferenceEquals(value.Blueprint,
                                    enchantment)).ToArray())
                            weapon.RemoveEnchantment(effect);
                    }
                    slot.RemoveItem(false);
                    _player.Inventory.Remove(weapon);
                    weapon.Dispose();
                }
            }

            private void RememberFeatPersistenceWeapon(
                ItemEntityWeapon weapon)
            {
                if (weapon != null && !_featPersistenceOwnedWeapons.Any(
                    value => ReferenceEquals(value, weapon)))
                    _featPersistenceOwnedWeapons.Add(weapon);
            }

            private bool FeatPersistenceCleanupInventoryExact()
            {
                object[] current = Snapshot(_inventory);
                object[] expectedBaseline = _inventoryBefore.Where(value =>
                    !_featPersistenceOwnedWeapons.Any(weapon =>
                        ReferenceEquals(value, weapon))).ToArray();
                object[] currentBaseline = current.Where(value =>
                    !_featPersistenceOwnedWeapons.Any(weapon =>
                        ReferenceEquals(value, weapon))).ToArray();
                object[] retained = current.Where(value =>
                    _featPersistenceOwnedWeapons.Any(weapon =>
                        ReferenceEquals(value, weapon))).ToArray();
                bool retainUntilExit = _prepare && _saveStarted &&
                    _saveCompleted;
                return SameReferences(expectedBaseline, currentBaseline) &&
                    (retainUntilExit
                        ? retained.Length <= 2 && retained.Distinct().Count() ==
                            retained.Length
                        : retained.Length == 0);
            }

            private int RetainedFeatPersistenceInventoryCount()
            {
                return Snapshot(_inventory).Count(value =>
                    _featPersistenceOwnedWeapons.Any(weapon =>
                        ReferenceEquals(value, weapon)));
            }

            private bool PreparedFeatPersistenceInventoryExact()
            {
                if (_createdUnits == null || _createdUnits.Count !=
                        ElementalPersistenceFixtureCount)
                    return false;
                ElementalPersistenceFixture fixture = _fixtures.Single(value =>
                    value.Index == ScorchingPersistenceFixtureIndex);
                UnitEntityData owner = _createdUnits.SingleOrDefault(value =>
                    IsFixtureUnit(value, fixture));
                ItemEntityWeapon primary = owner == null ||
                        owner.Body == null ? null :
                    owner.Body.PrimaryHand.MaybeItem as ItemEntityWeapon;
                ItemEntityWeapon secondary = owner == null ||
                        owner.Body == null ? null :
                    owner.Body.SecondaryHand.MaybeItem as ItemEntityWeapon;
                if (primary == null || secondary == null ||
                    ReferenceEquals(primary, secondary) ||
                    !ReferenceEquals(primary.Blueprint,
                        _featPersistenceShortSword) ||
                    !ReferenceEquals(secondary.Blueprint,
                        _featPersistenceShortSword))
                    return false;

                ItemEntityWeapon[] expected = { primary, secondary };
                object[] current = Snapshot(_inventory);
                object[] baseline = current.Where(value => !expected.Any(
                    weapon => ReferenceEquals(value, weapon))).ToArray();
                return current.Length == _inventoryBefore.Length +
                        expected.Length &&
                    expected.All(weapon => current.Count(value =>
                        ReferenceEquals(value, weapon)) == 1) &&
                    SameReferences(_inventoryBefore, baseline);
            }

            private void CaptureRestoredSourceFeatPersistence(
                ElementalPersistenceFixture fixture, UnitEntityData source)
            {
                _restoredSourceFeatObservation = ObserveFeatPersistence(
                    fixture, source, true, false,
                    "module-restored-source-before-respec");
                if (!_restoredSourceFeatObservation.Value<bool>("exact"))
                    throw new InvalidOperationException(fixture.Label +
                        " did not retain its exact Release B feat facts and persistent Wings state before native Respec: " +
                        _restoredSourceFeatObservation.ToString(
                            Newtonsoft.Json.Formatting.None) + ".");
            }

            private static int CountFeatPersistenceBuff(UnitEntityData unit,
                BlueprintBuff blueprint)
            {
                return unit.Descriptor.Buffs.RawFacts.OfType<Buff>().Count(
                    value => ReferenceEquals(value.Blueprint, blueprint));
            }

            private static ItemEnchantment ExactFeatEnchantment(
                ItemEntityWeapon weapon,
                BlueprintWeaponEnchantment blueprint)
            {
                return weapon == null ? null : weapon.Enchantments
                    .SingleOrDefault(value => value != null &&
                        !value.IsEnded && ReferenceEquals(value.Blueprint,
                            blueprint));
            }

            private static int CountFeatEnchantment(ItemEntityWeapon weapon,
                BlueprintWeaponEnchantment blueprint)
            {
                return weapon == null ? 0 : weapon.Enchantments.Count(value =>
                    value != null && !value.IsEnded &&
                    ReferenceEquals(value.Blueprint, blueprint));
            }

            private static string OptionalFeatPersistenceItemId(
                ItemEntityWeapon weapon, out string rejection)
            {
                rejection = string.Empty;
                if (weapon == null) return string.Empty;
                FirearmItemId identity;
                if (!new KingmakerFirearmItemIdentityProvider()
                        .TryGetIdentity(weapon, out identity, out rejection))
                    return string.Empty;
                return identity.Value;
            }
        }
    }
}
