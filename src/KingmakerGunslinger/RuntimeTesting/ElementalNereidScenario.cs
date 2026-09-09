using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Intermediate mechanic evidence only. The request-local provider omits the
    // publication controller so an unpublished graph can be tested independently.
    // Native creator, replacement, respec and persistence remain separate gates.
    internal static partial class ElementalNereidScenario
    {
        // Pinned native negative controls, independent of the checker under test.
        private static readonly string[] NonHumanoidControls = {
            "3bec99efd9a363242a6c8d9957b75e91",
            "a95311b3dc996964cbaa30ff9965aaf6",
            "fd389783027d63343b4a5634bd81645f",
            "455ac88e22f55804ab87c2467deff1d6",
            "018af8005220ac94a9a4f47b3e9c2b4e",
            "625827490ea69d84d8e599a33929fdc6",
            "57614b50e8d86b24395931fffc5e409b",
            "9054d3988d491d944ac144e27b6bc318",
            "706e61781d692a042b35941f14bc41c5",
            "09478937695300944a179530664e42ec",
            "734a29b693e9ec346ba2951b27987e33",
        };
        private sealed class Saves : IGlobalRulebookHandler<RuleSavingThrow>
        {
            internal readonly List<RuleSavingThrow> Events = new List<RuleSavingThrow>();
            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt) { Events.Add(evt); }
        }

        internal static RuntimeTestResult Run(ModContext context, RuntimeTestRequest request)
        {
            DateTime started = DateTime.UtcNow;
            var assertions = new List<RuntimeTestAssertion>();
            var diagnostics = new List<string>();
            var files = new List<string>();
            string failure = string.Empty;
            try { Exercise(request, assertions, diagnostics, files); Actions(request, assertions, diagnostics, files); }
            catch (Exception exception) { failure = exception.ToString(); diagnostics.Add(failure); }
            Assembly assembly = context.Assembly;
            return new RuntimeTestResult {
                SchemaVersion = 1, RunId = request.RunId, Scenario = request.Scenario,
                Status = failure.Length == 0 && assertions.Count > 0 &&
                    assertions.All(value => value.Status == RuntimeTestStatuses.Pass)
                    ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                LoadedModVersion = context.ModEntry.Info.Version,
                RuntimeIdentity = assembly.FullName + ";mvid=" + assembly.ManifestModule.ModuleVersionId +
                    ";pid=" + Process.GetCurrentProcess().Id,
                GitCommit = assembly.GetCustomAttributes(typeof(AssemblyMetadataAttribute), false)
                    .OfType<AssemblyMetadataAttribute>().Single(value => value.Key == "GitCommit").Value,
                GameVersion = Application.version, StartUtc = started.ToString("o"), EndUtc = string.Empty,
                DurationMilliseconds = (long)(DateTime.UtcNow - started).TotalMilliseconds,
                Assertions = assertions, Diagnostics = diagnostics, Warnings = new List<string>(),
                ExceptionSummary = failure, EvidenceFiles = files, EvidenceDirectory = request.EvidenceDirectory,
                AutomaticExitRequested = request.ExitAfterCompletion
            };
        }

        private static void Exercise(RuntimeTestRequest request, ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics, ICollection<string> files)
        {
            var rows = new JArray();
            var world = Game.Instance.State.Units.All.ToArray();
            var clock = Game.Instance.Player.GameTime;
            var random = UnityEngine.Random.state;
            var race = BlueprintBootstrap.ElementalRaces.Undine;
            var trait = race.AlternateTraits.Require(ElementalAlternateTraitId.NereidFascination);
            var graph = trait.Mechanics().ToArray();
            var main = graph.OfType<BlueprintAbility>().Single(value => value.Type == AbilityType.Supernatural);
            var resource = graph.OfType<BlueprintAbilityResource>().Single();
            var aura = graph.OfType<BlueprintBuff>().Single(value => value.GetComponent<ElementalNereidAuraState>() != null);
            var targetBuff = graph.OfType<BlueprintBuff>().Single(value => value.GetComponent<ElementalNereidFascinated>() != null);
            var areaBlueprint = graph.OfType<BlueprintAbilityAreaEffect>().Single();
            var provider = CoreProvider(trait);
            var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
            var observer = new Saves();
            EventBus.Subscribe(observer);
            try
            {
                var caster = fixture.Caster;
                var owner = caster.Descriptor;
                var target = fixture.SpawnFixtureUnit(null, caster.Blueprint.Faction,
                    new Vector3(0, 0, 1), "NereidHumanoidAlly");
                var saved = fixture.SpawnFixtureUnit(null, caster.Blueprint.Faction,
                    new Vector3(0, 0, 2), "NereidSavedAlly");
                var excluded = fixture.SpawnFixtureUnit(null, caster.Blueprint.Faction,
                    new Vector3(0, 0, 3), "NereidNonPersonControl");
                var outside = fixture.SpawnFixtureUnit(null, caster.Blueprint.Faction,
                    new Vector3(0, 0, 12), "NereidOutside");
                var person = areaBlueprint.GetComponent<ElementalNereidArea>().PersonCheck;
                rows.Add(new JObject { { "name", "native-person-checker-catalog" },
                    { "nativePersonReferenceAbility", ElementalNereidFactory.PersonDonorGuid },
                    { "inverted", person.Inverted },
                    { "facts", new JArray(person.CheckedFacts.Select(value => new JObject {
                        { "guid", value.AssetGuid }, { "name", value.name } })) } });
                foreach (string guid in NonHumanoidControls)
                {
                    var fact = Exact<BlueprintFeature>(guid);
                    excluded.Descriptor.AddFact(fact);
                    Check(assertions, rows, "native-person-exclusion-" + fact.AssetGuid,
                        !person.CanTarget(caster, new TargetWrapper(excluded)),
                        "native excluded fact=" + fact.name + ";guid=" + fact.AssetGuid + ";inverted=" + person.Inverted);
                    excluded.Descriptor.RemoveFact(fact);
                }
                excluded.Descriptor.AddFact(person.CheckedFacts[0]);
                target.Stats.SaveWill.BaseValue = outside.Stats.SaveWill.BaseValue = -100;
                saved.Stats.SaveWill.BaseValue = 100;
                Check(assertions, rows, "exact-owned-graph", ElementalNereidFactory.IsExact(trait),
                    "seven registered mechanic identities; exact provider order and supernatural graph");
                ExactGraphNegativeControls(trait, rows, assertions);
                Check(assertions, rows, "native-person-controls",
                    person.CanTarget(caster, new TargetWrapper(target)) &&
                    !person.CanTarget(caster, new TargetWrapper(excluded)),
                    "owned native checker with audited creature types: human positive; excluded-type fact negative");

                var fighter = Exact<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
                var wizard = Exact<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e");
                ElementalSpellAffinityScenario.Advance(owner, fighter, 1);
                owner.AddFact(provider);
                foreach (int level in new[] { 1, 2, 5, 6, 9, 10, 19, 20 })
                {
                    if (level > owner.Progression.CharacterLevel)
                        ElementalSpellAffinityScenario.Advance(owner, wizard, level - owner.Progression.CharacterLevel);
                    // The hidden production trait is not selected. Level reconciliation
                    // removes its unselected graph, so restore only this test container.
                    if (owner.Abilities.GetAbility(main) == null) {
                        owner.RemoveFact(provider); owner.AddFact(provider);
                    }
                    foreach (int charisma in new[] { 6, 18 })
                    {
                        owner.Stats.Charisma.BaseValue = charisma;
                        var data = new AbilityData(owner.Abilities.GetAbility(main));
                        var parameters = data.CalculateParams();
                        Check(assertions, rows, "parameters-" + level + "-cha" + charisma,
                            data.Spellbook == null && !data.IsAffectedByArcaneSpellFailure &&
                            parameters.DC == ElementalNereidPolicy.DifficultyClass(level, owner.Stats.Charisma.Bonus) &&
                            parameters.CasterLevel == level,
                            "total multiclass level=" + level + ";current Cha=" + owner.Stats.Charisma.Bonus +
                            ";DC=" + parameters.DC + ";CL=" + parameters.CasterLevel);
                    }
                    var modifier = owner.Stats.Charisma.AddModifier(-8, owner.GetFact(provider),
                        "nereid-temporary-charisma", ModifierDescriptor.UntypedStackable);
                    try {
                        int dc = new AbilityData(owner.Abilities.GetAbility(main)).CalculateParams().DC;
                        Check(assertions, rows, "temporary-charisma-" + level,
                            dc == ElementalNereidPolicy.DifficultyClass(level, owner.Stats.Charisma.Bonus), "DC=" + dc);
                    } finally { owner.Stats.Charisma.RemoveModifier(modifier); }
                    owner.Resources.Restore(resource, 1);
                    observer.Events.Clear();
                    UnityEngine.Random.InitState(7419);
                    Cast(caster, main);
                    Buff source = caster.Buffs.GetBuff(aura);
                    if (source == null) throw new InvalidOperationException("Accepted native Nereid action produced no aura.");
                    var area = Area(source);
                    Game.Instance.EntityCreator.Tick();
                    Tick(area);
                    double duration = ElementalNereidPolicy.DurationRounds(level) * 6;
                    Check(assertions, rows, "native-activation-" + level,
                        owner.Resources.GetResourceAmount(resource) == 0 &&
                        Math.Abs(source.TimeLeft.TotalSeconds - duration) < .01 &&
                        target.Descriptor.HasFact(targetBuff) && !saved.Descriptor.HasFact(targetBuff) &&
                        !excluded.Descriptor.HasFact(targetBuff) && !outside.Descriptor.HasFact(targetBuff) &&
                        !owner.HasFact(targetBuff),
                        "uses=" + owner.Resources.GetResourceAmount(resource) + ";seconds=" + source.TimeLeft.TotalSeconds +
                        ";target=" + target.Descriptor.HasFact(targetBuff) + ";saved=" + saved.Descriptor.HasFact(targetBuff) +
                        ";excluded=" + excluded.Descriptor.HasFact(targetBuff) + ";outside=" + outside.Descriptor.HasFact(targetBuff) +
                        ";caster=" + owner.HasFact(targetBuff) + ";saves=" + observer.Events.Count);
                    var deadline = source.EndTime;
                    int saves = observer.Events.Count;
                    Tick(area); Tick(area);
                    Check(assertions, rows, "no-tick-reroll-" + level,
                        observer.Events.Count == saves && source.EndTime == deadline,
                        "native area updates retain save count=" + saves + " and original deadline");
                    target.Position = new Vector3(0, 0, 12); Tick(area);
                    bool left = !target.Descriptor.HasFact(targetBuff);
                    target.Position = new Vector3(0, 0, 1); Tick(area);
                    Check(assertions, rows, "exit-reentry-" + level,
                        left && target.Descriptor.HasFact(targetBuff) && observer.Events.Count == saves &&
                        target.Buffs.GetBuff(targetBuff).EndTime == deadline,
                        "exit removes owned condition; reentry keeps original save and deadline");
                    target.Buffs.GetBuff(targetBuff)?.Remove();
                    Tick(area);
                    target.Position = new Vector3(0, 0, 12); Tick(area);
                    target.Position = new Vector3(0, 0, 1); Tick(area);
                    Check(assertions, rows, "interruption-terminal-" + level,
                        !target.Descriptor.HasFact(targetBuff) && observer.Events.Count == saves,
                        "native buff removal cannot be undone by area updates or reentry");
                    Game.Instance.Player.GameTime = deadline - TimeSpan.FromSeconds(.1);
                    caster.Buffs.Tick(); Tick(area);
                    bool beforeExpiry = caster.Buffs.GetBuff(aura) != null && !area.IsEnded;
                    Game.Instance.Player.GameTime = deadline + TimeSpan.FromSeconds(.1);
                    caster.Buffs.Tick(); Tick(area);
                    Game.Instance.EntityCreator.Tick();
                    Check(assertions, rows, "native-expiration-" + level,
                        beforeExpiry && caster.Buffs.GetBuff(aura) == null && area.IsEnded &&
                        !target.Descriptor.HasFact(targetBuff),
                        "native timed caster buff owns area expiration; no duration refresh");
                    Game.Instance.Player.GameTime = clock;
                }
                EffectsAndThreats(fixture, caster, target, outside, main, resource, aura, targetBuff,
                    provider, observer, rows, assertions);
                owner.RemoveFact(provider);
            }
            finally
            {
                EventBus.Unsubscribe(observer);
                foreach (var unit in Game.Instance.State.Units.All.ToArray())
                    foreach (Buff buff in unit.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, aura)).ToArray()) {
                        var area = Area(buff); buff.Remove(); Tick(area);
                    }
                fixture.Dispose();
                UnityEngine.Object.Destroy(provider);
                Game.Instance.Player.GameTime = clock;
                UnityEngine.Random.state = random;
                bool clean = world.SequenceEqual(Game.Instance.State.Units.All) && fixture.NativeErrors == 0 &&
                    fixture.NativeExceptions == 0 && fixture.NativeObservationReleased && fixture.NativeTeardownObserved &&
                    fixture.AreaContextRestored && fixture.PlayerContextRestored;
                Check(assertions, rows, "cleanup", clean, "nativeErrors=" + fixture.NativeErrors +
                    ";nativeExceptions=" + fixture.NativeExceptions + ";worldExact=" + world.SequenceEqual(Game.Instance.State.Units.All));
                string path = Path.Combine(request.EvidenceDirectory, "elemental-nereid-core.json");
                File.WriteAllText(path, new JObject {
                    { "schemaVersion", 1 }, { "saveStateTouched", false },
                    { "qualificationBoundary", "registered mechanic graph with request-local provider container; no publication, native creator, respec, movement-path, or persistence acceptance" },
                    { "observations", rows }, { "diagnostics", new JArray(diagnostics) }
                }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }


        private static void EffectsAndThreats(ElementalUndineFeatScenario.PortalHarness fixture,
            UnitEntityData caster, UnitEntityData target, UnitEntityData outside,
            BlueprintAbility main, BlueprintAbilityResource resource, BlueprintBuff aura,
            BlueprintBuff fascinated, BlueprintFeature provider, Saves observer,
            JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            var owner = caster.Descriptor;
            var immunity = ScriptableObject.CreateInstance<BuffDescriptorImmunity>();
            immunity.Descriptor = SpellDescriptor.MindAffecting;
            var immune = Control("MindImmunity", immunity);
            var dazed = ScriptableObject.CreateInstance<Kingmaker.UnitLogic.FactLogic.AddCondition>();
            dazed.Condition = UnitCondition.Dazed;
            var unrelated = Control("UnrelatedDaze", dazed);
            var shake = BlueprintBootstrap.ElementalRaces.Undine.AlternateTraits
                .Require(ElementalAlternateTraitId.NereidFascination).Mechanics()
                .OfType<BlueprintAbility>().Single(value => value.Type == AbilityType.Extraordinary);
            UnitEntityData second = null;
            try
            {
                foreach (int charisma in new[] { 6, 18 })
                {
                    owner.Stats.Charisma.BaseValue = charisma;
                    var source = Activate(caster, main, resource, aura, observer);
                    Check(assertions, rows, "actual-save-dc-cha" + charisma,
                        observer.Events.Count > 0 && observer.Events.All(value =>
                            value.DifficultyClass == ElementalNereidPolicy.DifficultyClass(
                                owner.Progression.CharacterLevel, owner.Stats.Charisma.Bonus)),
                        "actual native Will DCs=" + string.Join(",", observer.Events.Select(value => value.DifficultyClass)));
                    End(source);
                }
                var adjustment = owner.Stats.Charisma.AddModifier(-8, owner.GetFact(provider),
                    "nereid-cast-temporary-charisma", ModifierDescriptor.UntypedStackable);
                try {
                    var source = Activate(caster, main, resource, aura, observer);
                    Check(assertions, rows, "actual-save-temporary-charisma",
                        observer.Events.Count > 0 && observer.Events.All(value =>
                            value.DifficultyClass == ElementalNereidPolicy.DifficultyClass(
                                owner.Progression.CharacterLevel, owner.Stats.Charisma.Bonus)),
                        "temporary current Cha=" + owner.Stats.Charisma.Bonus + ";DCs=" +
                            string.Join(",", observer.Events.Select(value => value.DifficultyClass)));
                    End(source);
                } finally { owner.Stats.Charisma.RemoveModifier(adjustment); }

                target.Descriptor.AddFact(immune);
                var immuneSource = Activate(caster, main, resource, aura, observer);
                Check(assertions, rows, "native-mind-immunity",
                    !target.Descriptor.HasFact(fascinated) && owner.Resources.GetResourceAmount(resource) == 0,
                    "native mind-affecting immunity blocks the condition; accepted activation stays spent");
                // Supply a distinct affected ally before asking whether the
                // immune target can assist; an all-immune aura needs no helper.
                outside.Position = new Vector3(0, 0, 1.2f);
                Tick(Area(immuneSource));
                Check(assertions, rows, "immune-ally-can-assist",
                    target.Descriptor.Abilities.GetAbility(shake) != null,
                    "non-mind-affecting assistance remains available to an immune nearby ally");
                End(immuneSource);
                outside.Position = new Vector3(0, 0, 12);
                target.Descriptor.RemoveFact(immune);

                var radiusSource = Activate(caster, main, resource, aura, observer);
                var radiusArea = Area(radiusSource);
                outside.Position = new Vector3(0, 0, 20.Feet().Meters + outside.View.Corpulence + .05f);
                Tick(radiusArea);
                int beforeEntry = observer.Events.Count;
                bool beyond = !outside.Descriptor.HasFact(fascinated);
                outside.Position = new Vector3(0, 0, 20.Feet().Meters - .05f);
                Tick(radiusArea);
                Check(assertions, rows, "native-radius-boundary",
                    beyond && outside.Descriptor.HasFact(fascinated) && observer.Events.Count == beforeEntry + 1,
                    "outside native radius plus corpulence excluded; inside 20 feet receives exactly one save");
                outside.Position = new Vector3(0, 0, 12);
                Tick(radiusArea);
                End(radiusSource);

                var targetPosition = target.Position;
                target.Position = new Vector3(0, 0, 6);
                outside.Position = new Vector3(0, 0, 7);
                var edgeSource = Activate(caster, main, resource, aura, observer);
                var edgeArea = Area(edgeSource);
                var edgeAbility = outside.Descriptor.Abilities.GetAbility(shake);
                Check(assertions, rows, "ally-outside-aura-can-assist",
                    !edgeArea.UnitsInside.Contains(outside) && target.Descriptor.HasFact(fascinated) &&
                    edgeAbility != null && new AbilityData(edgeAbility).CanTarget(new TargetWrapper(target)),
                    "ally at 7m is outside the harmful aura and within touch of its affected ally at 6m");
                Cast(outside, shake, target); Tick(edgeArea);
                Check(assertions, rows, "outside-ally-shake-commits",
                    !target.Descriptor.HasFact(fascinated) &&
                    edgeSource.SelectComponents<ElementalNereidAuraState>().Single().Response(target) ==
                        ElementalNereidResponse.Interrupted,
                    "native helper command breaks only Nereid and area tick cannot reapply it");
                End(edgeSource);
                Check(assertions, rows, "outside-helper-cleanup",
                    outside.Descriptor.Abilities.GetAbility(shake) == null,
                    "ending the source cleans its grant even though the helper was outside area membership");
                outside.Position = new Vector3(0, 0, 12); target.Position = targetPosition;

                var oldActivation = Activate(caster, main, resource, aura, observer);
                var oldContext = Area(oldActivation).Context;
                var oldArea = Area(oldActivation);
                var newActivation = Activate(caster, main, resource, aura, observer);
                Tick(oldArea); Tick(Area(newActivation));
                Check(assertions, rows, "same-caster-context-isolation",
                    ElementalNereidAuraState.Find(oldContext, aura) == null &&
                    ReferenceEquals(ElementalNereidAuraState.Find(Area(newActivation).Context, aura),
                        newActivation.SelectComponents<ElementalNereidAuraState>().Single()) &&
                    newActivation.SelectComponents<ElementalNereidAuraState>().Single().Response(target) ==
                        ElementalNereidResponse.Affected && target.Descriptor.HasFact(fascinated),
                    "retired activation cannot resolve to a newer aura on the same caster; old area cleanup preserves new effect");
                End(newActivation);

                target.Descriptor.AddFact(unrelated);
                var shaken = Activate(caster, main, resource, aura, observer);
                var assistance = caster.Descriptor.Abilities.GetAbility(shake);
                Check(assertions, rows, "shake-targeting",
                    assistance != null && new AbilityData(assistance).CanTarget(new TargetWrapper(target)) &&
                    !new AbilityData(assistance).CanTarget(new TargetWrapper(caster)),
                    "native assistance targets another fascinated ally at touch range");
                Cast(caster, shake, target);
                Tick(Area(shaken));
                Check(assertions, rows, "shake-owned-condition-only",
                    !target.Descriptor.HasFact(fascinated) && target.Descriptor.HasFact(unrelated) &&
                    target.Descriptor.State.HasCondition(UnitCondition.Dazed) &&
                    shaken.SelectComponents<ElementalNereidAuraState>().Single().Response(target) ==
                        ElementalNereidResponse.Interrupted,
                    "native Shake Free removes Nereid only; unrelated Dazed and interruption ledger remain");
                End(shaken);
                target.Descriptor.RemoveFact(unrelated);

                var damaged = Activate(caster, main, resource, aura, observer);
                int damageBefore = target.Damage;
                var damage = Rulebook.Trigger(new Kingmaker.RuleSystem.Rules.Damage.RuleDealDamage(caster, target,
                    new Kingmaker.RuleSystem.Rules.Damage.DamageBundle(
                        new Kingmaker.RuleSystem.Rules.Damage.DirectDamage(new DiceFormula(0, DiceType.Zero), 1))));
                Tick(Area(damaged));
                Check(assertions, rows, "native-damage-breaks",
                    damage.Damage == 1 && target.Damage == damageBefore + 1 &&
                    !target.Descriptor.HasFact(fascinated) &&
                    damaged.SelectComponents<ElementalNereidAuraState>().Single().Response(target) ==
                        ElementalNereidResponse.Interrupted,
                    "actual native damage=" + damage.Damage + ";native condition absent after following area update");
                End(damaged);

                second = fixture.SpawnFixtureUnit(null, caster.Blueprint.Faction,
                    new Vector3(5, 0, 0), "NereidSecondSource");
                second.Blueprint.IsCheater = false;
                ElementalSpellAffinityScenario.Advance(second.Descriptor,
                    Exact<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd"), 2);
                second.Descriptor.AddFact(provider);
                second.Descriptor.AddFact(immune);
                int perception = target.Stats.SkillPerception.ModifiedValue;
                var firstAura = Activate(caster, main, resource, aura, observer);
                var secondAura = Activate(second, main, resource, aura, observer);
                Check(assertions, rows, "benign-carrier-contexts",
                    firstAura.Context.SpellDescriptor == SpellDescriptor.None &&
                    secondAura.Context.SpellDescriptor == SpellDescriptor.None &&
                    ReferenceEquals(firstAura.Context.SourceAbility, main) &&
                    ReferenceEquals(secondAura.Context.SourceAbility, main) &&
                    target.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, fascinated))
                        .All(value => value.Context.SpellDescriptor == SpellDescriptor.MindAffecting &&
                            ReferenceEquals(value.Context.SourceAbility, main)),
                    "immune caster can create the aura; benign carriers retain the supernatural source; target effects remain mind-affecting");
                Check(assertions, rows, "two-source-overlap",
                    target.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, fascinated)) == 2 &&
                    target.Stats.SkillPerception.ModifiedValue == perception - 4,
                    "owned condition instances=" + target.Buffs.Enumerable.Count(value =>
                        ReferenceEquals(value.Blueprint, fascinated)) + ";Perception=" +
                        target.Stats.SkillPerception.ModifiedValue + ";before=" + perception);
                End(firstAura);
                Check(assertions, rows, "one-source-removal",
                    target.Buffs.Enumerable.Count(value => ReferenceEquals(value.Blueprint, fascinated)) == 1 &&
                    target.Descriptor.State.HasCondition(UnitCondition.Dazed) &&
                    target.Stats.SkillPerception.ModifiedValue == perception - 4,
                    "remaining source retains native condition and single -4 penalty");
                Check(assertions, rows, "surviving-source-assistance",
                    caster.Descriptor.Abilities.GetAbility(shake) != null,
                    "ending one overlapping aura leaves the surviving source's helper usable");
                End(secondAura);
                Check(assertions, rows, "last-source-helper-removed",
                    caster.Descriptor.Abilities.GetAbility(shake) == null,
                    "ending the final aura removes all temporary helper grants");
                Check(assertions, rows, "last-source-removal",
                    !target.Descriptor.HasFact(fascinated) && !target.Descriptor.State.HasCondition(UnitCondition.Dazed) &&
                    target.Stats.SkillPerception.ModifiedValue == perception,
                    "last source releases only its own native condition and penalty");
            }
            finally
            {
                target.Descriptor.RemoveFact(immune);
                target.Descriptor.RemoveFact(unrelated);
                if (second != null) { second.Descriptor.RemoveFact(immune); second.Descriptor.RemoveFact(provider); }
                UnityEngine.Object.Destroy(immune);
                UnityEngine.Object.Destroy(unrelated);
            }
        }

        private static BlueprintFeature Control(string suffix, BlueprintComponent component)
        {
            var value = ScriptableObject.CreateInstance<BlueprintFeature>();
            value.name = "KMG_Runtime_Nereid_" + suffix; value.Ranks = 1;
            value.ComponentsArray = new[] { component };
            return value;
        }
        private static Buff Activate(UnitEntityData caster, BlueprintAbility main,
            BlueprintAbilityResource resource, BlueprintBuff aura, Saves observer)
        {
            caster.Descriptor.Resources.Restore(resource, 1);
            observer.Events.Clear(); UnityEngine.Random.InitState(7419);
            Cast(caster, main);
            var source = caster.Buffs.GetBuff(aura);
            if (source == null) throw new InvalidOperationException("Accepted native aura absent.");
            Game.Instance.EntityCreator.Tick(); Tick(Area(source));
            return source;
        }
        private static void End(Buff source)
        {
            var area = Area(source);
            source.Remove(); Tick(area); Game.Instance.EntityCreator.Tick();
        }


        private static void ExactGraphNegativeControls(ElementalAlternateTraitBlueprints trait, JArray rows,
            ICollection<RuntimeTestAssertion> assertions)
        {
            var inventory = trait.Mechanics().ToArray();
            var main = inventory.OfType<BlueprintAbility>().Single(value => value.Type == AbilityType.Supernatural);
            var shake = inventory.OfType<BlueprintAbility>().Single(value => value.Type == AbilityType.Extraordinary);
            var provider = trait.Provider.ComponentsArray;
            var parameters = provider.OfType<ElementalNereidParameters>().Single();
            var resource = inventory.OfType<BlueprintAbilityResource>().Single();
            var commit = main.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction>()
                .Actions.Actions.OfType<ElementalHydraulicResourceCommit>().Single();
            var source = inventory.OfType<BlueprintBuff>().Single(value => value.GetComponent<ElementalNereidAuraState>() != null);
            var state = source.GetComponent<ElementalNereidAuraState>();
            var helperFacts = state.Assistance.GetComponent<Kingmaker.UnitLogic.FactLogic.AddFacts>();
            var originalFacts = helperFacts.Facts;
            try
            {
                parameters.Ability = shake;
                Check(assertions, rows, "validator-rejects-wrong-parameters", !ElementalNereidFactory.IsExact(trait), "wrong Su parameter binding rejected");
                parameters.Ability = main;
                commit.Resource = null;
                Check(assertions, rows, "validator-rejects-missing-commit", !ElementalNereidFactory.IsExact(trait), "missing daily commitment binding rejected");
                commit.Resource = resource;
                helperFacts.Facts = new Kingmaker.Blueprints.Facts.BlueprintUnitFact[] { main };
                Check(assertions, rows, "validator-rejects-wrong-helper", !ElementalNereidFactory.IsExact(trait), "wrong transient assistance grant rejected");
                helperFacts.Facts = originalFacts;
                var wrongOrder = provider.ToArray(); var ledger = wrongOrder[2]; wrongOrder[2] = wrongOrder[4]; wrongOrder[4] = ledger;
                trait.Provider.ComponentsArray = wrongOrder;
                Check(assertions, rows, "validator-rejects-resource-reconciliation-order", !ElementalNereidFactory.IsExact(trait), "reconciliation preceding remembered expenditure rejected");
            }
            finally
            {
                parameters.Ability = main; commit.Resource = resource; helperFacts.Facts = originalFacts;
                trait.Provider.ComponentsArray = provider;
                Check(assertions, rows, "validator-controls-restored", ReferenceEquals(trait.Provider.ComponentsArray, provider) &&
                    ReferenceEquals(helperFacts.Facts, originalFacts) && ElementalNereidFactory.IsExact(trait),
                    "exact registered component arrays and bindings restored before behavior tests");
            }
        }

        private static void Actions(RuntimeTestRequest request, ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> diagnostics, ICollection<string> files)
        {
            var rows = new JArray();
            var world = Game.Instance.State.Units.All.ToArray();
            var random = UnityEngine.Random.state;
            var hands = Game.Instance.HandsEquipmentController;
            var setter = typeof(Game).GetProperty("HandsEquipmentController").GetSetMethod(true);
            var queueField = typeof(Kingmaker.Controllers.Units.UnitHandEquipmentController).GetField(
                "m_UnitsToUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            var queue = hands == null ? null : (List<UnitEntityData>)queueField.GetValue(hands);
            if (world.Length != 0 || (queue != null && queue.Count != 0))
                throw new InvalidOperationException("Native Nereid actions require an empty fixture boundary.");
            var ownedHands = hands == null ? new Kingmaker.Controllers.Units.UnitHandEquipmentController() : null;
            if (ownedHands != null) setter.Invoke(Game.Instance, new object[] { ownedHands });
            var tutorial = Kingmaker.Blueprints.Root.BlueprintRoot.Instance.UITutorials.TBMStarted.Lock;
            if (tutorial == null) throw new InvalidOperationException("Native turn tutorial flag absent.");
            bool tutorialLocked = tutorial.IsLocked;
            var flagsBefore = Game.Instance.Player.UnlockableFlags.UnlockedFlags.ToArray();
            var race = BlueprintBootstrap.ElementalRaces.Undine;
            var trait = race.AlternateTraits.Require(ElementalAlternateTraitId.NereidFascination);
            var main = trait.Mechanics().OfType<BlueprintAbility>().Single(value => value.Type == AbilityType.Supernatural);
            var resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
            var aura = trait.Mechanics().OfType<BlueprintBuff>().Single(value => value.GetComponent<ElementalNereidAuraState>() != null);
            try
            {
                foreach (bool turnBased in new[] { false, true })
                {
                    string label = turnBased ? "turn-based-" : "rtwp-";
                    var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
                    var provider = CoreProvider(trait);
                    BlueprintFaction hostile = null;
                    ElementalNativeTurnScope turns = null;
                    var clock = Game.Instance.Player.GameTime;
                    try
                    {
                        var caster = fixture.Caster;
                        hostile = UnityEngine.Object.Instantiate(caster.Blueprint.Faction);
                        hostile.name = "KMG_Runtime_Nereid_Action_Hostile";
                        hostile.Peaceful = hostile.AlwaysEnemy = hostile.Neutral = hostile.IsDirectlyControllable = false;
                        hostile.Dummy = null; hostile.AttackFactions = new[] { caster.Blueprint.Faction };
                        var enemy = fixture.SpawnFixtureUnit(null, hostile, new Vector3(0, 0, .8f), "NereidActionEnemy");
                        enemy.Stats.SaveWill.BaseValue = 100;
                        caster.Memory.Add(enemy); enemy.Memory.Add(caster);
                        ElementalSpellAffinityScenario.Advance(caster.Descriptor,
                            Exact<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd"), 2);
                        caster.Descriptor.AddFact(provider);
                        caster.Descriptor.Resources.Restore(resource, 1);
                        // The fixture borrows the save-free main-menu Player.
                        // Temporarily mark this one tutorial as seen; the exact
                        // flag dictionary is verified after restoration below.
                        if (turnBased) {
                            tutorial.Unlock();
                            turns = new ElementalNativeTurnScope(caster, enemy, rows, label);
                        }
                        else { caster.CombatState.JoinCombat(); caster.CombatState.OnNewRound(); }
                        var controller = new Kingmaker.Controllers.Units.UnitActionController();
                        var data = new AbilityData(caster.Descriptor.Abilities.GetAbility(main));
                        var target = new TargetWrapper(caster);
                        var before = ActionCosts(caster);
                        var canceled = new Kingmaker.UnitLogic.Commands.UnitUseAbility(data, target);
                        caster.Commands.Run(canceled);
                        bool queued = caster.Commands.Contains(canceled);
                        caster.Commands.InterruptAll(true); caster.Commands.RemoveFinishedAndUpdateQueue();
                        Check(assertions, rows, label + "cancel-before-commit",
                            queued && !canceled.IsStarted && !canceled.IsActed && !canceled.Cutscene &&
                            caster.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                            ActionCosts(caster).SequenceEqual(before),
                            "native queued cancellation preserves all action channels and daily use");
                        UnityEngine.Random.InitState(7419);
                        var command = new Kingmaker.UnitLogic.Commands.UnitUseAbility(data, target);
                        caster.Commands.Run(command);
                        try
                        {
                            for (int tick = 0; !command.IsActed && !command.IsFinished && tick < 16; tick++)
                            {
                                if (turns != null) turns.Drive(command);
                                else {
                                    if (command.Animation != null) command.Animation.IsActed = true;
                                    ElementalBreathScenario.TickCommand(controller, command);
                                }
                            }
                            var committed = ActionCosts(caster);
                            float expected = before[0] + (turnBased ? 6 : Math.Max(0, 6 - command.TimeSinceStart));
                            Check(assertions, rows, label + "native-standard-commit",
                                command.IsStarted && command.IsActed && !command.Cutscene && !command.IsIgnoreCooldown &&
                                Math.Abs(committed[0] - expected) < .001f &&
                                committed[1] == before[1] && committed[2] == before[2] &&
                                caster.Descriptor.Resources.GetResourceAmount(resource) == 0,
                                "before=" + string.Join(",", before) + ";committed=" + string.Join(",", committed) +
                                ";expectedStandard=" + expected + ";started=" + command.IsStarted + ";acted=" + command.IsActed);
                            for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 120; tick++)
                            {
                                if (turns == null) command.ExecutionProcess.Tick();
                                else turns.Execute(command.ExecutionProcess.Tick);
                            }
                            var source = caster.Buffs.GetBuff(aura);
                            if (source == null) throw new InvalidOperationException("Native accepted command left no aura.");
                            Game.Instance.EntityCreator.Tick(); Tick(Area(source));
                            Check(assertions, rows, label + "accepted-saves-not-refunded",
                                command.ExecutionProcess != null && command.ExecutionProcess.IsEnded &&
                                ActionCosts(caster).SequenceEqual(committed) &&
                                caster.Descriptor.Resources.GetResourceAmount(resource) == 0 && !data.IsAvailable &&
                                source.SelectComponents<ElementalNereidAuraState>().Single().Response(enemy) == ElementalNereidResponse.Resisted,
                                "native completion leaves one spent use and the same charged action channels");
                            End(source);
                            if (!turnBased)
                                ThreatCommands(fixture, caster, enemy, main, resource, aura, controller, rows, assertions);
                        }
                        finally
                        {
                            if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Detach();
                            caster.Commands.InterruptAll(true); caster.Commands.RemoveFinishedAndUpdateQueue();
                        }
                    }
                    finally
                    {
                        if (turns != null) turns.Dispose();
                        Game.Instance.Player.GameTime = clock;
                        fixture.Dispose();
                        UnityEngine.Object.Destroy(provider);
                        if (hostile != null) UnityEngine.Object.Destroy(hostile);
                        Check(assertions, rows, label + "lifetime",
                            (turns == null || turns.Restored) && fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                            fixture.NativeObservationReleased && fixture.NativeTeardownObserved &&
                            fixture.AreaContextRestored && fixture.PlayerContextRestored,
                            "nativeErrors=" + fixture.NativeErrors + ";nativeExceptions=" + fixture.NativeExceptions);
                    }
                }
            }
            finally
            {
                UnityEngine.Random.state = random;
                if (tutorialLocked) tutorial.Lock();
                Check(assertions, rows, "tutorial-flag-restored",
                    flagsBefore.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal).SequenceEqual(
                        Game.Instance.Player.UnlockableFlags.UnlockedFlags.OrderBy(value => value.Key.AssetGuid, StringComparer.Ordinal)),
                    "exact native main-menu Player flag keys and values restored; no save was loaded");
                if (ownedHands != null) {
                    if (!ReferenceEquals(Game.Instance.HandsEquipmentController, ownedHands))
                        throw new InvalidOperationException("Native hand-controller ownership changed.");
                    setter.Invoke(Game.Instance, new object[] { hands });
                }
                if (queue != null && queue.Count != 0) {
                    if (queue.Any(unit => unit != null && !unit.ShouldBeDestroyed && unit.View != null))
                        throw new InvalidOperationException("Live foreign hand work entered the fixture.");
                    bool paused = Game.Instance.IsPaused;
                    try { Game.Instance.IsPaused = false; hands.Tick(); }
                    finally { Game.Instance.IsPaused = paused; }
                }
                Check(assertions, rows, "action-fixture-cleanup",
                    world.SequenceEqual(Game.Instance.State.Units.All) && (queue == null || queue.Count == 0) &&
                    ReferenceEquals(hands, Game.Instance.HandsEquipmentController),
                    "exact world and native controller references restored");
                string path = Path.Combine(request.EvidenceDirectory, "elemental-nereid-actions.json");
                File.WriteAllText(path, new JObject { { "saveStateTouched", false },
                    { "qualificationBoundary", "native action controllers on request-local actors; creator/respec and persistence excluded" },
                    { "observations", rows } }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }
        private static float[] ActionCosts(UnitEntityData unit)
        { return new[] { unit.CombatState.Cooldown.StandardAction, unit.CombatState.Cooldown.MoveAction,
            unit.CombatState.Cooldown.SwiftAction }; }

        private static BlueprintFeature CoreProvider(ElementalAlternateTraitBlueprints trait)
        {
            var provider = ScriptableObject.CreateInstance<BlueprintFeature>();
            provider.name = "KMG_Runtime_Nereid_Core_Provider"; provider.Ranks = 1;
            provider.ComponentsArray = trait.Provider.ComponentsArray.Where(value =>
                !(value is ElementalAlternateTraitProviderController)).Select(value =>
                    UnityEngine.Object.Instantiate(value)).ToArray();
            return provider;
        }

        private static AreaEffectEntityData Area(Buff source)
        {
            var component = source.SelectComponents<AddAreaEffect>().Single();
            var field = typeof(AddAreaEffect).GetField("m_AreaEffectInstance", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException(typeof(AddAreaEffect).FullName, "m_AreaEffectInstance");
            return (AreaEffectEntityData)field.GetValue(component);
        }
        private static void Tick(AreaEffectEntityData area)
        {
            if (area == null) throw new InvalidOperationException("Native attached area absent.");
            var update = typeof(AreaEffectEntityData).GetMethod("UpdateUnits", BindingFlags.Instance | BindingFlags.NonPublic);
            if (update == null) throw new MissingMethodException(typeof(AreaEffectEntityData).FullName, "UpdateUnits");
            update.Invoke(area, null);
            area.Tick();
        }
        private static void Cast(UnitEntityData caster, BlueprintAbility blueprint, UnitEntityData target = null)
        {
            var data = new AbilityData(caster.Descriptor.Abilities.GetAbility(blueprint));
            var command = ElementalUndineFeatScenario.CreateCommand(data, new TargetWrapper(target ?? caster), caster);
            try {
                ElementalUndineFeatScenario.InvokeCommandAction(command);
                for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 100; tick++)
                    command.ExecutionProcess.Tick();
                if (command.ExecutionProcess == null || !command.ExecutionProcess.IsEnded)
                    throw new InvalidOperationException("Native personal command did not complete.");
                ElementalUndineFeatScenario.InvokeCommandEnded(command, false);
            } finally {
                if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Detach();
            }
        }
        private static T Exact<T>(string guid) where T : BlueprintScriptableObject
        { return BlueprintLibraryLookup.RequireExact<T>(BlueprintBootstrap.Library, guid, "Nereid native core fixture"); }
        private static void Check(ICollection<RuntimeTestAssertion> assertions, JArray rows, string name, bool pass, string observed)
        {
            rows.Add(new JObject { { "name", name }, { "pass", pass }, { "observed", observed } });
            assertions.Add(new RuntimeTestAssertion { Name = "elemental-nereid-" + name,
                Expected = "exact source-owned supernatural fascination graph behavior", Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "native commands, area membership, rule events and disposable unit facts; no saves" });
        }
    }
}
