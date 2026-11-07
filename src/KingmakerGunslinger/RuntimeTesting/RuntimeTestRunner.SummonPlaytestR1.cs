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
        //     are present, including ProperFlanking2's toggle when installed.
        //  3. The real SummonWithdrawnCapabilityFixer on an induced pre-R1 state.
        //     Adding the withdrawn facts/actions to a fresh summon is a disclosed
        //     fixture precondition, not a cold load of an older save.
        // Brains are inactive throughout; nothing is cast by a summon, no input,
        // no save and no setting is changed.
        private static readonly string[] SummonPlaytestR1Keys =
        { "bone-devil", "ice-devil", "magma-mephit", "lillend-azata", "fire-giant", "storm-giant" };
        // The toggle installed ProperFlanking2 actually grants through Improved
        // Sunder's AddFacts (blueprints.txt: SunderActionToggleAbility). The
        // first R1 run used a0808193..., PF2's never-granted save-cleanup dummy
        // SunderArmorToggleAbility; that run's two FAILs are retained with a
        // reader-only re-evaluation from its directly recorded grants.
        private const string ProperFlankingSunderToggleGuid = "d9604eade0f54d88a899b5052f98d536";

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
                // Let default-on toggles (ProperFlanking2) and fact activation settle.
                for (int frame = 0; frame < 10; frame++) { Game.Instance.IsPaused = false; yield return 0; }

                var bodies = new JObject();
                record["fresh"] = bodies;
                foreach (string key in SummonPlaytestR1Keys) bodies[key] = SummonPlaytestR1Snapshot(byKey[key]);

                var sunder = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                    SummonPlaytestWithdrawalPolicy.NativeImprovedSunderGuid, "native Improved Sunder");
                BlueprintUnitFact[] sunderGrants = sunder.GetComponents<AddFacts>()
                    .SelectMany(value => value.Facts ?? new BlueprintUnitFact[0]).Where(value => value != null).ToArray();
                var toggle = ResourcesLibrary.TryGetBlueprint<BlueprintActivatableAbility>(ProperFlankingSunderToggleGuid);
                record["properFlanking2SunderToggleInstalled"] = toggle != null;
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
                var book = lillendBook == null ? null : SummonPlaytestR1Book(lillend, lillendBook);
                bool providerActive = Sprint32Suggestion.Active;
                bool suggestionKnown = book != null && book.GetKnownSpells(2).Any(value => ReferenceEquals(value.Blueprint, lillendSuggestion));
                bool suggestionListed = lillendBook != null && lillendBook.SpellList.SpellsByLevel.Any(level =>
                    level.SpellLevel == 2 && level.Spells.Contains(lillendSuggestion));
                var dispatcher = LillendNoncombat32.Find(lillend);
                bool performances = new[] { "Courage", "Competence", "Fascinate" }.All(name =>
                    lillend.Descriptor.ActivatableAbilities.Enumerable.Any(value =>
                        ReferenceEquals(value.Blueprint, registered("KMG.Summoning.FeyCelestial.LillendAzata." + name))));
                var lillendRow = new JObject { ["providerActive"] = providerActive, ["spellbook"] = book != null,
                    ["suggestionListed"] = suggestionListed, ["suggestionKnown"] = suggestionKnown,
                    ["dispatcher"] = dispatcher != null, ["dispatcherBardicRoute"] = dispatcher?.BardicSuggestion?.name,
                    ["performances"] = performances };
                record["lillend"] = lillendRow;
                Sprint25Check(prefix + "lillend-retains-suggestion-spell-and-performances",
                    book != null && dispatcher != null && dispatcher.BardicSuggestion == null && performances &&
                    (!providerActive || (suggestionListed && suggestionKnown)), (JObject)lillendRow.DeepClone(),
                    "Lillend keeps its spellbook, the provider-gated Suggestion spell when its provider is active, and its three performances; the noncombat route has no bardic target.");
                var cleave = (BlueprintUnitFact)registered("KMG.Summoning.Giant.CleaveAction");
                var greatCleave = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                    "cc9c862ef2e03af4f89be5088851ea35", "native Great Cleave");
                Func<UnitEntityData, bool> toggleOn = unit => toggle != null &&
                    unit.Descriptor.ActivatableAbilities.Enumerable.Any(value => ReferenceEquals(value.Blueprint, toggle));
                Func<UnitEntityData, bool> toggleBuff = unit => toggle != null && toggle.Buff != null &&
                    unit.Descriptor.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, toggle.Buff));
                var giants = new JObject {
                    ["fireImprovedSunder"] = fire.Descriptor.HasFact(sunder), ["fireSunderGrants"] = sunderGrants.Count(value => fire.Descriptor.HasFact(value)),
                    ["fireToggle"] = toggleOn(fire), ["fireToggleBuff"] = toggleBuff(fire),
                    ["fireGreatCleave"] = fire.Descriptor.HasFact(greatCleave), ["fireCleaveAction"] = fire.Descriptor.HasFact(cleave),
                    ["firePrimaryWeapon"] = fire.Body.PrimaryHand.MaybeWeapon?.Blueprint?.name,
                    ["stormImprovedSunder"] = storm.Descriptor.HasFact(sunder), ["stormSunderGrants"] = sunderGrants.Count(value => storm.Descriptor.HasFact(value)),
                    ["stormToggle"] = toggleOn(storm), ["stormToggleBuff"] = toggleBuff(storm) };
                record["giants"] = giants;
                Sprint25Check(prefix + "fire-giant-no-sunder-source-storm-keeps-it",
                    !fire.Descriptor.HasFact(sunder) && sunderGrants.All(value => !fire.Descriptor.HasFact(value)) &&
                    !toggleOn(fire) && !toggleBuff(fire) && fire.Descriptor.HasFact(greatCleave) &&
                    fire.Body.PrimaryHand.MaybeWeapon != null &&
                    storm.Descriptor.HasFact(sunder) && sunderGrants.All(value => storm.Descriptor.HasFact(value)) &&
                    (toggle == null || toggleOn(storm)), (JObject)giants.DeepClone(),
                    "Fire Giant has no Improved Sunder, no Sunder Armor action and no ProperFlanking2 replacement toggle, and keeps Great Cleave and its weapon; Storm Giant keeps all of them.");

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
                var fireInduced = new JObject { ["improvedSunder"] = fire.Descriptor.HasFact(sunder),
                    ["sunderGrants"] = sunderGrants.Count(value => fire.Descriptor.HasFact(value)), ["toggle"] = toggleOn(fire) };
                induced["fireBeforeFixer"] = fireInduced;
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
                for (int frame = 0; frame < 5; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                foreach (WithdrawnCapabilitySpec spec in SummonPlaytestWithdrawalPolicy.All)
                {
                    UnitEntityData owner = byKey[spec.CreatureKey];
                    bool retired = withdrawnFacts(owner, spec).All(value => !owner.Descriptor.HasFact(value)) &&
                        spec.AiActionSymbols.All(symbol => !brainHas(owner, symbol)) &&
                        spec.ResourceSymbols.All(symbol => !owner.Descriptor.Resources.ContainsResource(registered(symbol)));
                    var row = SummonPlaytestR1Snapshot(owner);
                    induced[spec.CreatureKey] = row;
                    Sprint25Check(prefix + spec.CreatureKey + "-legacy-retired", retired, (JObject)row.DeepClone(),
                        "The real fixer removes exactly the induced withdrawn grant, brain action and resource from " + spec.CreatureKey + ".");
                }
                bool fireDependents = fireInduced.Value<bool>("improvedSunder") &&
                    fireInduced.Value<int>("sunderGrants") == sunderGrants.Length &&
                    (toggle == null || fireInduced.Value<bool>("toggle")) &&
                    sunderGrants.All(value => !fire.Descriptor.HasFact(value)) && !toggleOn(fire) && !toggleBuff(fire);
                Sprint25Check(prefix + "fire-giant-fixer-removes-feat-dependents", fireDependents,
                    new JObject { ["before"] = fireInduced.DeepClone(), ["afterGrants"] = sunderGrants.Count(value => fire.Descriptor.HasFact(value)),
                        ["afterToggle"] = toggleOn(fire) },
                    "Induced Improved Sunder brings its Sunder Armor action and installed toggle; removing the feat takes both with it.");
                Sprint25Check(prefix + "retained-after-fixer",
                    has(bone, "KMG.Summoning.Infernal.BoneDevil.Invisibility") && has(ice, "KMG.Summoning.Infernal.IceDevil.ConeOfCold") &&
                    has(magma, "KMG.Summoning.Special.MagmaMephit.SpellLikeOne") && has(magma, "KMG.Summoning.Special.MagmaMephit.Breath") &&
                    performances && fire.Descriptor.HasFact(greatCleave) && storm.Descriptor.HasFact(sunder) &&
                    lifetimes(bone, boneWall).Length == 1 && lifetimes(ice, iceWall).Length == 1,
                    new JObject(), "Nothing outside the withdrawn list is removed, including the Wall source lifetimes.");
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

        private static JObject SummonPlaytestR1Snapshot(UnitEntityData unit)
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
                ["brainActions"] = new JArray(unit.Brain == null ? new string[0] : unit.Brain.Actions.Select(value => value.Blueprint.name).ToArray()) };
        }
    }
}
