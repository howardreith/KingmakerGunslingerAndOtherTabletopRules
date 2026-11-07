using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.View;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Repair R1: one save-free observation in the static interior fixture.
        //  1. The live publication surface: the canonical inventory is exact at
        //     the derived count and no AbilityVariants menu anywhere in the
        //     library - canonical parent or alternate-source root - offers a
        //     Stirge canonical or adapter child, while those identities still
        //     resolve.
        //  2. Fresh summons cast through their ordinary own-tier children: the
        //     withdrawn grants, resources and brain actions are absent; the
        //     retained abilities, Wall source lifetime and Storm Giant control
        //     are present. With ProperFlanking2 active, the exact Improved
        //     Sunder chain is resolved (toggle and replacement buff) and the
        //     toggle's presence, on and running state and the buff's presence
        //     and activity are read separately; the Storm Giant's buff must be
        //     observed active within a bounded wait.
        //  3. The real SummonWithdrawnCapabilityFixer on an induced pre-R1 state.
        //     Adding the withdrawn facts/actions to a fresh summon is a disclosed
        //     fixture precondition, not a cold load of an older save, and does
        //     not exercise native once-per-unit fixer bookkeeping. The induced
        //     Fire Giant's replacement buff must be observed active before the
        //     fixer and absent after it; retained capabilities are re-read from
        //     the live units after reconciliation.
        // Brains are inactive throughout; nothing is cast by a summon, no input,
        // no save and no setting is changed.
        private static readonly string[] SummonPlaytestR1Keys =
        { "bone-devil", "ice-devil", "magma-mephit", "lillend-azata", "fire-giant", "storm-giant" };
        // The toggle installed ProperFlanking2 actually grants through Improved
        // Sunder's AddFacts (blueprints.txt: SunderActionToggleAbility) and the
        // attack-replacement buff that toggle applies while running
        // (SunderActionToggleBuff, AttackReplacementWithCombatManeuver). The
        // first R1 run used a0808193..., PF2's never-granted save-cleanup dummy
        // SunderArmorToggleAbility, for both the toggle and its buff; that
        // run's two FAILs are retained with a reader-only re-evaluation.
        private const string ProperFlankingSunderToggleGuid = "d9604eade0f54d88a899b5052f98d536";
        private const string ProperFlankingSunderToggleName = "SunderActionToggleAbility";
        private const string ProperFlankingSunderBuffGuid = "f9f7edc1148c453181460dccbbefa3dc";
        private const string ProperFlankingSunderBuffName = "SunderActionToggleBuff";
        private const string ProperFlankingReplacementComponent =
            "ProperFlanking20.ManeuverAsAttack.AttackReplacementWithCombatManeuver";
        // Bounded waits for a live native condition. A running toggle's buff is
        // applied by the native activatable controller, and Stop may remove it
        // after up to one round, so both directions wait for the observed state.
        private const int SummonPlaytestR1WaitFrames = 900;
        private const double SummonPlaytestR1WaitGameSeconds = 8.0;

        private IEnumerable<int> ReviewSummonPlaytestCutsR1(ExpandedSummoningCorrectionFixture fixture, string phase)
        {
            if (!IsSprint32Review || !_request.ExitAfterCompletion || !ReferenceEquals(fixture, _sprint25Fixture) ||
                fixture?.Scene == null || fixture.Caster == null || !_context.FeatureModules.Active.ExpandedSummoning ||
                phase != ExpandedSummoningNativeProfilePolicy.Sprint39PlaytestCutsPhase)
                throw new InvalidOperationException("Exact fresh R1 fixture with the summoning module enabled required.");
            const string prefix = "r1-";
            LibraryScriptableObject library = BlueprintBootstrap.Library;
            IDictionary<string, BlueprintScriptableObject> symbols = ExpandedSummoningBlueprints.Registered.BySymbol;
            Func<string, BlueprintScriptableObject> registered = symbol => symbols[symbol];
            var owned = new List<UnitEntityData>();
            var byKey = new Dictionary<string, UnitEntityData>(StringComparer.Ordinal);
            var record = new JObject { ["phase"] = phase, ["runId"] = _request.RunId,
                ["limits"] = "Fresh summons and the live library. The pre-R1 state is induced on a fresh summon; no older save is cold-loaded. No summon acts, so no attack, cast or AI decision is claimed." };
            Vector3 centre = fixture.Caster.Position;
            try
            {
                // 1. Publication surface.
                int references;
                bool exact = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(library, true, out references);
                var stirgeCanonical = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly)
                    .Where(value => value.Creature.Key == SummonPlaytestWithdrawalPolicy.StirgeKey)
                    .Select(value => registered(ExpandedSummoningIdentityCatalog.AbilitySymbol(value)) as BlueprintAbility)
                    .ToArray();
                BlueprintScriptableObject[] all = library.GetAllBlueprints().Where(value => value != null).ToArray();
                ILookup<string, BlueprintAbility> abilities = all.OfType<BlueprintAbility>()
                    .ToLookup(value => value.name, StringComparer.Ordinal);
                AlternateSummoningRoster roster = AlternateSummoningRoster.Production();
                var stirgeAdapterNames = new List<string>();
                int adapterResolved = 0;
                foreach (AlternateSummoningFamilyIdentity family in AlternateSummoningSourceCatalog.Identities)
                    foreach (AlternateSummoningChoice choice in AlternateSummoningProjection.Identities(family, roster)
                        .Where(value => value.Kind == AlternateSummoningChoiceKind.Creature &&
                            value.Placement.Creature.Key == SummonPlaytestWithdrawalPolicy.StirgeKey))
                    {
                        string name = AlternateSummoningAdapterBuilder.InternalName(choice.Symbol);
                        stirgeAdapterNames.Add(name);
                        if (abilities[name].Count() == 1) adapterResolved++;
                    }
                var stirgeChildren = new HashSet<BlueprintAbility>(stirgeCanonical.Where(value => value != null)
                    .Concat(stirgeAdapterNames.SelectMany(name => abilities[name])));
                BlueprintAbility[] menus = all.OfType<BlueprintAbility>().Where(value =>
                    (value.Variants ?? new BlueprintAbility[0]).Any(child => child != null)).ToArray();
                string[] offering = menus.Where(menu => menu.Variants.Any(child => child != null &&
                    stirgeChildren.Contains(child))).Select(menu => menu.name).ToArray();
                int menusWithAdapters = menus.Count(menu => menu.Variants.Any(child => child != null &&
                    child.name.StartsWith(AdapterNamePrefix, StringComparison.Ordinal)));
                var stirgeUnit = registered("KMG.Summoning.Unit.Stirge") as BlueprintUnit;
                var surface = new JObject { ["canonicalExact"] = exact, ["canonicalReferences"] = references,
                    ["expectedReferences"] = SummonVisibilityCatalog.PublishedLogicalPlacementCount,
                    ["stirgeCanonicalResolved"] = stirgeCanonical.Count(value => value != null),
                    ["stirgeAdapterIdentities"] = stirgeAdapterNames.Count, ["stirgeAdapterResolved"] = adapterResolved,
                    ["menusScanned"] = menus.Length, ["menusWithAdapterChildren"] = menusWithAdapters,
                    ["menusOfferingStirge"] = new JArray(offering), ["stirgeUnitResolved"] = stirgeUnit != null };
                record["surface"] = surface;
                Sprint25Check(prefix + "stirge-withheld-everywhere",
                    exact && references == SummonVisibilityCatalog.PublishedLogicalPlacementCount &&
                    stirgeCanonical.Length == 9 && stirgeCanonical.All(value => value != null) &&
                    stirgeAdapterNames.Count > 0 && adapterResolved == stirgeAdapterNames.Count &&
                    stirgeUnit != null && offering.Length == 0 && menusWithAdapters > 0,
                    (JObject)surface.DeepClone(),
                    "Canonical inventory exact at 1179; no canonical or alternate menu offers Stirge; its nine roots, adapters and unit still resolve.");

                // 2. Fresh summons.
                for (int index = 0; index < SummonPlaytestR1Keys.Length; index++)
                {
                    string key = SummonPlaytestR1Keys[index];
                    UnitEntityData owner = CastExpandedSummoningOwnTier(fixture, key);
                    owned.Add(owner); byKey[key] = owner;
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    Vector3 offset = Quaternion.Euler(0f, index * (360f / SummonPlaytestR1Keys.Length), 0f) *
                        Vector3.forward * 5f;
                    PlaceExpandedSummoningUnit(owner, ObstacleAnalyzer.GetNearestNode(centre + offset).clampedPosition);
                }
                // Let fact activation settle.
                for (int frame = 0; frame < 10; frame++) { Game.Instance.IsPaused = false; yield return 0; }

                // The installed Improved Sunder grant chain, resolved exactly.
                var sunder = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                    SummonPlaytestWithdrawalPolicy.NativeImprovedSunderGuid, "native Improved Sunder");
                BlueprintUnitFact[] sunderGrants = sunder.GetComponents<AddFacts>()
                    .SelectMany(value => value.Facts ?? new BlueprintUnitFact[0]).Where(value => value != null).ToArray();
                JObject properFlanking2 = InfernalProperFlanking2State35();
                bool pf2Active = properFlanking2.Value<bool?>("modActive") == true;
                var toggle = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>(ProperFlankingSunderToggleGuid);
                var replacementBuff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(ProperFlankingSunderBuffGuid);
                bool toggleGranted = toggle != null && sunderGrants.Any(value => ReferenceEquals(value, toggle));
                bool replacementComponent = replacementBuff != null && replacementBuff.ComponentsArray != null &&
                    replacementBuff.ComponentsArray.Any(value => value != null &&
                        value.GetType().FullName == ProperFlankingReplacementComponent);
                bool chainExact = toggle != null && replacementBuff != null &&
                    toggle.name == ProperFlankingSunderToggleName && replacementBuff.name == ProperFlankingSunderBuffName &&
                    ReferenceEquals(toggle.Buff, replacementBuff) && toggle.IsOnByDefault && toggleGranted && replacementComponent;
                string chainStatus = !pf2Active
                    ? (toggle == null && replacementBuff == null && !toggleGranted ? "provider-inactive" : "provider-inactive-identities-present")
                    : chainExact ? "provider-active-exact" : "provider-active-identity-mismatch";
                bool observeChain = chainStatus == "provider-active-exact";
                BlueprintUnitFact[] sunderActions = sunderGrants.Where(value => !ReferenceEquals(value, toggle)).ToArray();
                record["sunderChain"] = new JObject {
                    ["status"] = chainStatus, ["properFlanking2Active"] = pf2Active, ["properFlanking2"] = properFlanking2,
                    ["improvedSunder"] = new JObject { ["guid"] = sunder.AssetGuid, ["name"] = sunder.name },
                    ["grants"] = new JArray(sunderGrants.Select(value => new JObject { ["guid"] = value.AssetGuid, ["name"] = value.name })),
                    ["toggle"] = new JObject { ["guid"] = ProperFlankingSunderToggleGuid, ["resolved"] = toggle != null,
                        ["name"] = toggle?.name, ["isOnByDefault"] = toggle != null && toggle.IsOnByDefault,
                        ["grantedByImprovedSunder"] = toggleGranted, ["buffGuid"] = toggle?.Buff?.AssetGuid },
                    ["replacementBuff"] = new JObject { ["guid"] = ProperFlankingSunderBuffGuid, ["resolved"] = replacementBuff != null,
                        ["name"] = replacementBuff?.name, ["replacementComponent"] = replacementComponent },
                    ["dependentActions"] = new JArray(sunderActions.Select(value => value.name)) };
                Sprint25Check(prefix + "sunder-chain-identities", observeChain || chainStatus == "provider-inactive",
                    (JObject)record["sunderChain"].DeepClone(),
                    "With ProperFlanking2 active, Improved Sunder grants SunderActionToggleAbility, which is default-on and applies SunderActionToggleBuff carrying AttackReplacementWithCombatManeuver. An inactive provider is recorded as not observed; an active provider with a missing or different identity fails.");

                // Separate readings: toggle present, switched on, running; buff present, active.
                Func<UnitEntityData, JObject> sunderState = unit =>
                {
                    ActivatableAbility live = toggle == null ? null : unit.Descriptor.ActivatableAbilities.Enumerable
                        .FirstOrDefault(value => ReferenceEquals(value.Blueprint, toggle));
                    Kingmaker.UnitLogic.Buffs.Buff[] buffs = replacementBuff == null ? new Kingmaker.UnitLogic.Buffs.Buff[0] :
                        unit.Descriptor.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, replacementBuff)).ToArray();
                    return new JObject {
                        ["improvedSunder"] = unit.Descriptor.HasFact(sunder),
                        ["dependentActions"] = sunderActions.Count(value => unit.Descriptor.HasFact(value)),
                        ["togglePresent"] = live != null, ["toggleOn"] = live != null && live.IsOn,
                        ["toggleRunning"] = live != null && live.IsRunning, ["toggleAvailable"] = live != null && live.IsAvailable,
                        ["replacementBuffPresent"] = buffs.Length, ["replacementBuffActive"] = buffs.Count(value => value.Active && !value.IsSuppressed),
                        ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds };
                };
                Func<JObject, bool> chainLive = state => state.Value<bool>("improvedSunder") &&
                    state.Value<int>("dependentActions") == sunderActions.Length && state.Value<bool>("togglePresent") &&
                    state.Value<bool>("toggleOn") && state.Value<bool>("toggleRunning") && state.Value<int>("replacementBuffActive") >= 1;
                Func<JObject, bool> chainGone = state => !state.Value<bool>("improvedSunder") &&
                    state.Value<int>("dependentActions") == 0 && !state.Value<bool>("togglePresent") &&
                    state.Value<int>("replacementBuffPresent") == 0;

                // Storm Giant positive control: wait for the default-on toggle to run.
                UnitEntityData stormControl = byKey["storm-giant"];
                TimeSpan stormWaitStart = Game.Instance.TimeController.GameTime;
                int stormFrames = 0;
                while (observeChain && !chainLive(sunderState(stormControl)) && stormFrames < SummonPlaytestR1WaitFrames &&
                    (Game.Instance.TimeController.GameTime - stormWaitStart).TotalSeconds < SummonPlaytestR1WaitGameSeconds)
                { Game.Instance.IsPaused = false; stormFrames++; yield return 0; }
                record["stormControlWait"] = new JObject { ["frames"] = stormFrames,
                    ["gameSeconds"] = (Game.Instance.TimeController.GameTime - stormWaitStart).TotalSeconds };

                var bodies = new JObject();
                record["fresh"] = bodies;
                foreach (string key in SummonPlaytestR1Keys) bodies[key] = SummonPlaytestR1Snapshot(byKey[key], replacementBuff);
                record["improvedSunderGrants"] = new JArray(sunderGrants.Select(value => value.name));

                foreach (WithdrawnCapabilitySpec spec in SummonPlaytestWithdrawalPolicy.All)
                {
                    UnitEntityData owner = byKey[spec.CreatureKey];
                    BlueprintUnitFact[] facts = spec.FactSymbols.Select(symbol => (BlueprintUnitFact)registered(symbol))
                        .Concat(spec.NativeFeatGuids.Select(guid => (BlueprintUnitFact)BlueprintLibraryLookup
                            .RequireExact<BlueprintFeature>(library, guid, "withdrawn native feat"))).ToArray();
                    BlueprintAiAction[] actions = spec.AiActionSymbols.Select(symbol => (BlueprintAiAction)registered(symbol)).ToArray();
                    BlueprintAbilityResource[] resources = spec.ResourceSymbols.Select(symbol => (BlueprintAbilityResource)registered(symbol)).ToArray();
                    bool fixerAttached = owner.Blueprint.GetComponents<SummonWithdrawnCapabilityFixer>().Count() == 1;
                    bool absent = facts.All(value => !owner.Descriptor.HasFact(value)) &&
                        actions.All(value => owner.Brain == null || owner.Brain.Actions.All(action => !ReferenceEquals(action.Blueprint, value))) &&
                        resources.All(value => !owner.Descriptor.Resources.ContainsResource(value));
                    Sprint25Check(prefix + spec.CreatureKey + "-withdrawn-absent", absent && fixerAttached,
                        new JObject { ["facts"] = new JArray(facts.Select(value => value.name)),
                            ["aiActions"] = new JArray(actions.Select(value => value.name)),
                            ["resources"] = new JArray(resources.Select(value => value.name)),
                            ["fixerAttached"] = fixerAttached, ["snapshot"] = bodies[spec.CreatureKey].DeepClone() },
                        "A fresh " + spec.CreatureKey + " has no withdrawn grant, resource or brain action; its load fixer is attached.");
                }

                // Retained capabilities and controls.
                UnitEntityData bone = byKey["bone-devil"], ice = byKey["ice-devil"], magma = byKey["magma-mephit"],
                    lillend = byKey["lillend-azata"], fire = byKey["fire-giant"], storm = byKey["storm-giant"];
                Func<UnitEntityData, string, bool> has = (unit, symbol) => unit.Descriptor.HasFact((BlueprintUnitFact)registered(symbol));
                Func<UnitEntityData, string, bool> brainHas = (unit, symbol) => unit.Brain != null &&
                    unit.Brain.Actions.Any(action => ReferenceEquals(action.Blueprint, registered(symbol)));
                Func<UnitEntityData, BlueprintAbility, InfernalWallSourceLifetime35[]> lifetimes = (unit, wall) =>
                    unit.Descriptor.Progression.Features.Enumerable.SelectMany(feature =>
                        feature.SelectComponents<InfernalWallSourceLifetime35>()).Where(value =>
                            ReferenceEquals(value.WallAbility, wall)).ToArray();
                var boneWall = (BlueprintAbility)registered("KMG.Summoning.Infernal.BoneDevil.WallOfIce");
                var iceWall = (BlueprintAbility)registered("KMG.Summoning.Infernal.IceDevil.WallOfIce");
                Sprint25Check(prefix + "devils-retain-other-spells-and-wall-cleanup",
                    has(bone, "KMG.Summoning.Infernal.BoneDevil.Invisibility") &&
                    has(bone, "KMG.Summoning.Infernal.BoneDevil.QuickenedInvisibility") &&
                    brainHas(bone, "KMG.Summoning.Infernal.BoneDevil.InvisibilityAi") &&
                    has(ice, "KMG.Summoning.Infernal.IceDevil.ConeOfCold") && has(ice, "KMG.Summoning.Infernal.IceDevil.IceStorm") &&
                    brainHas(ice, "KMG.Summoning.Infernal.IceDevil.ConeOfColdAi") && brainHas(ice, "KMG.Summoning.Infernal.IceDevil.IceStormAi") &&
                    lifetimes(bone, boneWall).Length == 1 && lifetimes(ice, iceWall).Length == 1,
                    new JObject { ["boneLifetimes"] = lifetimes(bone, boneWall).Length, ["iceLifetimes"] = lifetimes(ice, iceWall).Length },
                    "Both devils keep their other spell-like abilities and AI, and each keeps exactly one Wall source lifetime bound to the retained Wall identity.");
                Sprint25Check(prefix + "magma-retains-breath-and-pyrotechnics",
                    has(magma, "KMG.Summoning.Special.MagmaMephit.Breath") && brainHas(magma, "KMG.Summoning.Special.MagmaMephit.BreathAi") &&
                    has(magma, "KMG.Summoning.Special.MagmaMephit.SpellLikeOne") && brainHas(magma, "KMG.Summoning.Special.MagmaMephit.SpellLikeOneAi") &&
                    magma.Descriptor.Resources.ContainsResource(registered("KMG.Summoning.Special.MagmaMephit.SpellLikeOneResource")),
                    (JObject)bodies["magma-mephit"].DeepClone(), "Magma Mephit keeps its breath and Pyrotechnics with resource and AI.");
                var lillendBook = (BlueprintSpellbook)registered("KMG.Summoning.FeyCelestial.LillendAzata.Spellbook");
                var lillendSuggestion = (BlueprintAbility)registered("KMG.Summoning.FeyCelestial.LillendAzata.Suggestion");
                bool suggestionProviderActive = Sprint32Suggestion.Active;
                // Read from the live unit each time it is called.
                Func<JObject> lillendState = () =>
                {
                    var book = lillendBook == null ? null : SummonPlaytestR1Book(lillend, lillendBook);
                    bool suggestionKnown = book != null && book.GetKnownSpells(2).Any(value => ReferenceEquals(value.Blueprint, lillendSuggestion));
                    bool suggestionListed = lillendBook != null && lillendBook.SpellList.SpellsByLevel.Any(level =>
                        level.SpellLevel == 2 && level.Spells.Contains(lillendSuggestion));
                    var dispatcher = LillendNoncombat32.Find(lillend);
                    var performanceRows = new JObject();
                    foreach (string name in new[] { "Courage", "Competence", "Fascinate" })
                        performanceRows[name] = lillend.Descriptor.ActivatableAbilities.Enumerable.Any(value =>
                            ReferenceEquals(value.Blueprint, registered("KMG.Summoning.FeyCelestial.LillendAzata." + name)));
                    bool performances = performanceRows.Properties().All(value => value.Value.Value<bool>());
                    bool bardicFact = lillend.Descriptor.HasFact((BlueprintUnitFact)registered("KMG.Summoning.FeyCelestial.LillendAzata.BardicSuggestion"));
                    bool retained = book != null && dispatcher != null && dispatcher.BardicSuggestion == null && performances &&
                        !bardicFact && (!suggestionProviderActive || (suggestionListed && suggestionKnown));
                    return new JObject { ["suggestionProviderActive"] = suggestionProviderActive,
                        ["suggestionObservation"] = suggestionProviderActive ? "observed" : "provider inactive / not observed",
                        ["spellbook"] = book != null, ["suggestionListed"] = suggestionListed, ["suggestionKnown"] = suggestionKnown,
                        ["dispatcher"] = dispatcher != null, ["dispatcherBardicRoute"] = dispatcher?.BardicSuggestion?.name,
                        ["bardicSuggestionFact"] = bardicFact, ["performances"] = performanceRows, ["retained"] = retained };
                };
                JObject lillendRow = lillendState();
                record["lillend"] = lillendRow;
                Sprint25Check(prefix + "lillend-retains-suggestion-spell-and-performances",
                    lillendRow.Value<bool>("retained"), (JObject)lillendRow.DeepClone(),
                    "Lillend keeps its spellbook, the provider-gated Suggestion spell when its provider is active, and its three performances; the noncombat route has no bardic target.");
                var cleave = (BlueprintUnitFact)registered("KMG.Summoning.Giant.CleaveAction");
                var greatCleave = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                    "cc9c862ef2e03af4f89be5088851ea35", "native Great Cleave");
                JObject fireFresh = sunderState(fire), stormFresh = sunderState(storm);
                var giants = new JObject {
                    ["fire"] = fireFresh, ["storm"] = stormFresh,
                    ["fireGreatCleave"] = fire.Descriptor.HasFact(greatCleave), ["fireCleaveAction"] = fire.Descriptor.HasFact(cleave),
                    ["firePrimaryWeapon"] = fire.Body.PrimaryHand.MaybeWeapon?.Blueprint?.name,
                    ["stormPrimaryWeapon"] = storm.Body.PrimaryHand.MaybeWeapon?.Blueprint?.name,
                    ["chainStatus"] = chainStatus };
                record["giants"] = giants;
                bool stormKeeps = stormFresh.Value<bool>("improvedSunder") &&
                    stormFresh.Value<int>("dependentActions") == sunderActions.Length &&
                    (observeChain ? chainLive(stormFresh) : chainStatus == "provider-inactive");
                Sprint25Check(prefix + "fire-giant-no-sunder-source-storm-keeps-it",
                    chainGone(fireFresh) && fire.Descriptor.HasFact(greatCleave) && fire.Body.PrimaryHand.MaybeWeapon != null &&
                    stormKeeps, (JObject)giants.DeepClone(),
                    "Fire Giant has no Improved Sunder, Sunder action, SunderActionToggleAbility or SunderActionToggleBuff, and keeps Great Cleave and its weapon; Storm Giant keeps the feat and action, and its default-on toggle runs with an active SunderActionToggleBuff.");

                // 3. The load fixer on an induced pre-R1 state.
                var induced = new JObject();
                record["inducedLegacy"] = induced;
                Func<UnitEntityData, WithdrawnCapabilitySpec, BlueprintUnitFact[]> withdrawnFacts = (unit, spec) =>
                    spec.FactSymbols.Select(symbol => (BlueprintUnitFact)registered(symbol))
                        .Concat(spec.NativeFeatGuids.Select(guid => (BlueprintUnitFact)BlueprintLibraryLookup
                            .RequireExact<BlueprintFeature>(library, guid, "withdrawn native feat"))).ToArray();
                foreach (WithdrawnCapabilitySpec spec in SummonPlaytestWithdrawalPolicy.All)
                {
                    UnitEntityData owner = byKey[spec.CreatureKey];
                    foreach (BlueprintUnitFact fact in withdrawnFacts(owner, spec)) owner.Descriptor.AddFact(fact);
                    foreach (string symbol in spec.AiActionSymbols)
                        owner.Brain.Actions.Add(new AiAction((BlueprintAiAction)registered(symbol)));
                    foreach (string symbol in spec.ResourceSymbols)
                        owner.Descriptor.Resources.Add(registered(symbol), true);
                }
                for (int frame = 0; frame < 5; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                // The induced feat must actually bring a running toggle and an
                // active replacement buff before the fixer is called.
                TimeSpan fireWaitStart = Game.Instance.TimeController.GameTime;
                int fireFrames = 0;
                while (observeChain && !chainLive(sunderState(fire)) && fireFrames < SummonPlaytestR1WaitFrames &&
                    (Game.Instance.TimeController.GameTime - fireWaitStart).TotalSeconds < SummonPlaytestR1WaitGameSeconds)
                { Game.Instance.IsPaused = false; fireFrames++; yield return 0; }
                JObject fireInduced = sunderState(fire);
                induced["fireBeforeFixer"] = fireInduced;
                induced["fireBeforeFixerWait"] = new JObject { ["frames"] = fireFrames,
                    ["gameSeconds"] = (Game.Instance.TimeController.GameTime - fireWaitStart).TotalSeconds };
                induced["fireBeforeFixerSnapshot"] = SummonPlaytestR1Snapshot(fire, replacementBuff);
                // Negative: another creature's fixer never touches this unit.
                var boneFixer = bone.Blueprint.GetComponents<SummonWithdrawnCapabilityFixer>().Single();
                boneFixer.OnPostLoad(ice.Descriptor);
                bool foreignUntouched = ice.Descriptor.HasFact(iceWall) &&
                    brainHas(ice, "KMG.Summoning.Infernal.IceDevil.WallOfIceAi");
                Sprint25Check(prefix + "fixer-exact-owner-only", foreignUntouched,
                    new JObject { ["iceWallKept"] = ice.Descriptor.HasFact(iceWall) },
                    "The Bone Devil fixer leaves an Ice Devil's induced Wall grant and AI action alone.");
                foreach (WithdrawnCapabilitySpec spec in SummonPlaytestWithdrawalPolicy.All)
                {
                    UnitEntityData owner = byKey[spec.CreatureKey];
                    owner.Blueprint.GetComponents<SummonWithdrawnCapabilityFixer>().Single().OnPostLoad(owner.Descriptor);
                }
                JObject fireAfterImmediate = sunderState(fire);
                for (int frame = 0; frame < 5; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                // Native Stop may remove a running toggle's buff after up to one
                // round; wait boundedly for the observed removal.
                TimeSpan removalWaitStart = Game.Instance.TimeController.GameTime;
                int removalFrames = 0;
                while (observeChain && !chainGone(sunderState(fire)) && removalFrames < SummonPlaytestR1WaitFrames &&
                    (Game.Instance.TimeController.GameTime - removalWaitStart).TotalSeconds < SummonPlaytestR1WaitGameSeconds)
                { Game.Instance.IsPaused = false; removalFrames++; yield return 0; }
                JObject fireAfter = sunderState(fire), stormAfter = sunderState(storm);
                induced["fireAfterFixerImmediate"] = fireAfterImmediate;
                induced["fireAfterFixer"] = fireAfter;
                induced["fireAfterFixerWait"] = new JObject { ["frames"] = removalFrames,
                    ["gameSeconds"] = (Game.Instance.TimeController.GameTime - removalWaitStart).TotalSeconds };
                induced["stormAfterFixer"] = stormAfter;
                foreach (WithdrawnCapabilitySpec spec in SummonPlaytestWithdrawalPolicy.All)
                {
                    UnitEntityData owner = byKey[spec.CreatureKey];
                    bool retired = withdrawnFacts(owner, spec).All(value => !owner.Descriptor.HasFact(value)) &&
                        spec.AiActionSymbols.All(symbol => !brainHas(owner, symbol)) &&
                        spec.ResourceSymbols.All(symbol => !owner.Descriptor.Resources.ContainsResource(registered(symbol)));
                    var row = SummonPlaytestR1Snapshot(owner, replacementBuff);
                    induced[spec.CreatureKey] = row;
                    Sprint25Check(prefix + spec.CreatureKey + "-legacy-retired", retired, (JObject)row.DeepClone(),
                        "The real fixer removes exactly the induced withdrawn grant, brain action and resource from " + spec.CreatureKey + ".");
                }
                bool fireBefore = observeChain ? chainLive(fireInduced)
                    : chainStatus == "provider-inactive" && fireInduced.Value<bool>("improvedSunder") &&
                        fireInduced.Value<int>("dependentActions") == sunderActions.Length;
                bool stormStill = stormAfter.Value<bool>("improvedSunder") &&
                    stormAfter.Value<int>("dependentActions") == sunderActions.Length &&
                    (observeChain ? chainLive(stormAfter) : chainStatus == "provider-inactive");
                Sprint25Check(prefix + "fire-giant-fixer-removes-feat-dependents", fireBefore && chainGone(fireAfter) && stormStill,
                    new JObject { ["chainStatus"] = chainStatus, ["before"] = fireInduced.DeepClone(),
                        ["beforeWait"] = induced["fireBeforeFixerWait"].DeepClone(),
                        ["afterImmediate"] = fireAfterImmediate.DeepClone(), ["after"] = fireAfter.DeepClone(),
                        ["afterWait"] = induced["fireAfterFixerWait"].DeepClone(), ["stormAfter"] = stormAfter.DeepClone() },
                    "Induced Improved Sunder brings its Sunder action and a running SunderActionToggleAbility with an active SunderActionToggleBuff; after the fixer the feat, action, toggle and buff are all gone, while the Storm Giant's chain is still live.");
                JObject lillendAfter = lillendState();
                induced["lillendAfterFixer"] = lillendAfter;
                var retainedRow = new JObject {
                    ["boneInvisibility"] = has(bone, "KMG.Summoning.Infernal.BoneDevil.Invisibility"),
                    ["iceConeOfCold"] = has(ice, "KMG.Summoning.Infernal.IceDevil.ConeOfCold"),
                    ["magmaPyrotechnics"] = has(magma, "KMG.Summoning.Special.MagmaMephit.SpellLikeOne"),
                    ["magmaBreath"] = has(magma, "KMG.Summoning.Special.MagmaMephit.Breath"),
                    ["lillend"] = lillendAfter.DeepClone(),
                    ["fireGreatCleave"] = fire.Descriptor.HasFact(greatCleave),
                    ["firePrimaryWeapon"] = fire.Body.PrimaryHand.MaybeWeapon?.Blueprint?.name,
                    ["stormImprovedSunder"] = storm.Descriptor.HasFact(sunder),
                    ["boneLifetimes"] = lifetimes(bone, boneWall).Length, ["iceLifetimes"] = lifetimes(ice, iceWall).Length };
                Sprint25Check(prefix + "retained-after-fixer",
                    retainedRow.Value<bool>("boneInvisibility") && retainedRow.Value<bool>("iceConeOfCold") &&
                    retainedRow.Value<bool>("magmaPyrotechnics") && retainedRow.Value<bool>("magmaBreath") &&
                    lillendAfter.Value<bool>("retained") && retainedRow.Value<bool>("fireGreatCleave") &&
                    fire.Body.PrimaryHand.MaybeWeapon != null && retainedRow.Value<bool>("stormImprovedSunder") &&
                    retainedRow.Value<int>("boneLifetimes") == 1 && retainedRow.Value<int>("iceLifetimes") == 1,
                    retainedRow, "Nothing outside the withdrawn list is removed, read from the live units after reconciliation: Lillend performances, spellbook and provider-gated Suggestion, devil spells and Wall source lifetimes, mephit breath and Pyrotechnics, Fire Giant Great Cleave and weapon, Storm Giant Improved Sunder.");
            }
            finally
            {
                foreach (UnitEntityData owner in owned.Where(value => !value.Destroyed))
                {
                    owner.Commands.InterruptAll(command => true); owner.Commands.RemoveFinishedAndUpdateQueue();
                    if (owner.IsInCombat) owner.LeaveCombat();
                }
                PlaceExpandedSummoningUnit(fixture.Caster, centre);
                DisposeExpandedSummoningUnits(fixture.Created, owned.Where(value => !value.Destroyed).ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory, phase + ".json"),
                    record.ToString(Formatting.Indented));
            }
        }

        private static Kingmaker.UnitLogic.Spellbook SummonPlaytestR1Book(UnitEntityData unit, BlueprintSpellbook blueprint)
        {
            return unit.Descriptor.Spellbooks.FirstOrDefault(value => ReferenceEquals(value.Blueprint, blueprint));
        }

        private static JObject SummonPlaytestR1Snapshot(UnitEntityData unit, BlueprintBuff replacementBuff)
        {
            // The native resource collection exposes GetEnumerator only.
            var resources = new List<string>();
            foreach (BlueprintScriptableObject resource in unit.Descriptor.Resources)
                if (resource != null) resources.Add(resource.name);
            resources.Sort(StringComparer.Ordinal);
            return new JObject {
                ["unit"] = unit.UniqueId, ["blueprint"] = unit.Blueprint.name,
                ["abilities"] = new JArray(unit.Descriptor.Abilities.Enumerable.Select(value => value.Blueprint.name).OrderBy(value => value, StringComparer.Ordinal)),
                ["activatables"] = new JArray(unit.Descriptor.ActivatableAbilities.Enumerable.Select(value => value.Blueprint.name + (value.IsOn ? ":on" : ":off")).OrderBy(value => value, StringComparer.Ordinal)),
                ["features"] = new JArray(unit.Descriptor.Progression.Features.Enumerable.Select(value => value.Blueprint.name).OrderBy(value => value, StringComparer.Ordinal)),
                ["resources"] = new JArray(resources),
                // Only the exact ProperFlanking2 replacement buff, by identity.
                ["replacementBuffs"] = new JArray(replacementBuff == null ? new string[0] : unit.Descriptor.Buffs.Enumerable
                    .Where(value => ReferenceEquals(value.Blueprint, replacementBuff))
                    .Select(value => value.Blueprint.AssetGuid + ":" + (value.Active ? "active" : "inactive") +
                        (value.IsSuppressed ? ":suppressed" : "")).ToArray()),
                ["brainActions"] = new JArray(unit.Brain == null ? new string[0] : unit.Brain.Actions.Select(value => value.Blueprint.name).ToArray()) };
        }
    }
}
