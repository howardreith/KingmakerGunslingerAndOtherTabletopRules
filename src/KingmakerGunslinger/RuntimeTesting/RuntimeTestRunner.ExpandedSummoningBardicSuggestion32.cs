using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Continuation of the existing calm, naturally established Fascinate.
        // Successful saves, high SR and one successful Suggestion share that
        // same performance. Fixture casts are used only for the labeled
        // ordinary-book negative at the end, never for bardic execution proof.
        private IEnumerable<int> ReviewLillendBardicSuggestion32(ExpandedSummoningCorrectionFixture fixture,
            Cleanup2NativeDefaultScope mode, UnitEntityData owner, UnitEntityData sibling,
            ActivatableAbility toggle, BlueprintAbilityResource resource, BlueprintBuff fascinated,
            Lillend32CommandStarts starts, Astral31DispelEvents events, FirearmReviewCosts economy,
            JArray costs, Lillend32PerformanceRounds rounds)
        {
            var target = fixture.Hostile;
            var dispatcher = LillendNoncombat32.Find(owner);
            var ability = dispatcher.BardicSuggestion;
            var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(ability));
            var bookType = owner.Blueprint.GetComponent<LillendBardBook32>().Book;
            var book = owner.Descriptor.GetSpellbook(bookType);
            var otherBook = sibling.Descriptor.GetSpellbook(bookType);
            var slots = Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots).ToArray();
            var otherSlots = Enumerable.Range(0, 4).Select(otherBook.GetSpontaneousSlots).ToArray();
            // The player anchor lawfully sees this hostile and shares group
            // memory with the summon. Preserve that initial visibility; only
            // reciprocal hostile acquisition must remain absent.
            bool ownerMemoryAtStart = owner.Memory.Contains(target);
            var anchor = Game.Instance.Player.MainCharacter.Value;
            var captured = target.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, fascinated) &&
                ReferenceEquals(value.Context?.MaybeCaster, owner));
            var source = owner.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, toggle.Blueprint.Buff))
                .SelectComponents<LillendFascinateState32>().Single();
            var oldComponents = target.Blueprint.ComponentsArray;
            BlueprintFeature srFact = null;
            UnitAggroFilter aggro = null;
            Buff invisible = null;
            bool seeAdded = false;
            var ownContext = new AbilityExecutionContext(data, data.CalculateParams(), new TargetWrapper(target), Rulebook.CurrentContext);
            var otherData = new AbilityData(sibling.Descriptor.Abilities.GetAbility(ability));
            var otherContext = new AbilityExecutionContext(otherData, otherData.CalculateParams(), new TargetWrapper(target), Rulebook.CurrentContext);
            Func<UnitUseAbility[]> casts = () => starts.Commands.OfType<UnitUseAbility>().Where(value =>
                ReferenceEquals(value.Spell?.Blueprint, ability)).ToArray();
            Func<bool> retained = () => mode.Current && toggle.IsRunning && source.Owns(target) &&
                captured.Active && !captured.IsDisposed && target.Buffs.Enumerable.Contains(captured) &&
                !owner.IsInCombat && !target.IsInCombat && !Game.Instance.Player.IsInCombat &&
                owner.Memory.Contains(target) == ownerMemoryAtStart && !target.Memory.Contains(owner) &&
                !target.Memory.Contains(anchor) && !target.Memory.Contains(fixture.Caster);
            Func<Buff> suggestion = () => target.Buffs.Enumerable.SingleOrDefault(value =>
                value.Blueprint.AssetGuid == Sprint32Suggestion.OwnedBuffGuid &&
                ReferenceEquals(value.Context?.MaybeCaster, owner));
            Func<bool> budgets = () => rounds.Exact() && sibling.Descriptor.Resources.GetResourceAmount(resource) == 20 &&
                Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots).SequenceEqual(slots) &&
                Enumerable.Range(0, 4).Select(otherBook.GetSpontaneousSlots).SequenceEqual(otherSlots);
            try
            {
                // A bounded availability probe on the idle sibling, not a
                // spell cast: the private Hold must refuse the exact excluded
                // discovery and recover when its temporary fact is removed.
                var holdData = otherBook.GetKnownSpells(2).Single(value =>
                    value.Blueprint.name == "KMG_Summoning_FeyCelestial_LillendAzata_HoldPerson");
                var discovery = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                    Sprint32Suggestion.ExcludedWizardDiscoveryGuid, "installed Infectious Charms discovery");
                bool holdBefore = holdData.IsAvailable, holdRefused = false;
                if (UnitHelper.HasFact(sibling.Descriptor, discovery))
                    throw new InvalidOperationException("Fresh sibling unexpectedly owns the excluded wizard discovery.");
                try
                {
                    sibling.Descriptor.AddFact(discovery);
                    holdRefused = UnitHelper.HasFact(sibling.Descriptor, discovery) && !holdData.IsAvailable;
                }
                finally { sibling.Descriptor.RemoveFact(discovery); }
                bool holdAfter = holdData.IsAvailable && !UnitHelper.HasFact(sibling.Descriptor, discovery);
                var registry = HarmonyInstance.Create("KMG.Sprint32.BardicPatchInspection");
                var patchRows = new JArray();
                foreach (var method in new[] {
                    AccessTools.Method(typeof(OffensiveActionsController), "RaiseEvents",
                        new[] { typeof(UnitEntityData), typeof(UnitEntityData) }),
                    AccessTools.Method(typeof(UnitMemoryController), "HandleUnitMakeOffensiveAction",
                        new[] { typeof(UnitEntityData), typeof(UnitEntityData) }) })
                {
                    if (method == null) throw new MissingMethodException("Bardic memory inventory target is missing.");
                    // The installed Harmony12 shim throws while converting a
                    // null Harmony2 record. Its GetPatchedMethods enumerates
                    // all owners, as in CotwSpontaneousSlotCompatibility.
                    bool patchedByAnyOwner = registry.GetPatchedMethods().Contains(method);
                    var patches = patchedByAnyOwner ? registry.GetPatchInfo(method) : null;
                    var all = patches == null ? Array.Empty<Patch>() :
                        patches.Prefixes.Concat(patches.Postfixes).Concat(patches.Transpilers).ToArray();
                    patchRows.Add(new JObject { ["method"] = method.DeclaringType.FullName + "." + method.Name,
                        ["patchedByAnyOwner"] = patchedByAnyOwner,
                        ["harmony12Owners"] = new JArray(all.Select(value => value.owner)) });
                    // Refuse even patch kinds not represented by Harmony12's
                    // owner lists (for example a Harmony2-only finalizer).
                    if (patchedByAnyOwner) throw new InvalidOperationException(
                        "Bardic memory qualification requires review of another Harmony12 patch: " + method);
                }
                bool ready = holdBefore && holdRefused && holdAfter && ReferenceEquals(target.Blueprint, fixture.HostileBlueprint) &&
                    target.Blueprint.GetComponent<UnitAggroFilter>() == null && retained() && budgets() &&
                    casts().Length == 0 && data.IsAvailable && data.CanTarget(new TargetWrapper(target)) &&
                    owner.DistanceTo(target) <= data.GetApproachDistance(target) &&
                    LillendBardicMemory32.Qualifies(ownContext, owner, target) &&
                    !LillendBardicMemory32.Qualifies(otherContext, sibling, target) &&
                    !new AbilityData(sibling.Descriptor.Abilities.GetAbility(ability)).CanTarget(new TargetWrapper(target));
                Sprint25Check("bardic-ready", ready, new JObject { ["provider"] = Sprint32Suggestion.Disposition,
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["distance"] = owner.DistanceTo(target),
                    ["approachDistance"] = data.GetApproachDistance(target), ["patches"] = patchRows,
                    ["ownerMemoryAtStart"] = ownerMemoryAtStart,
                    ["holdAvailableBefore"] = holdBefore, ["holdDiscoveryRefused"] = holdRefused,
                    ["holdAvailableAfterRemoval"] = holdAfter,
                    ["ownSlots"] = new JArray(slots), ["siblingSlots"] = new JArray(otherSlots),
                    ["wrongOwnerRefused"] = !LillendBardicMemory32.Qualifies(otherContext, sibling, target) },
                    "Exact active provider, actual own Fascinate, ordinary ability reach/resources and wrong-owner refusal. Constructed contexts check admission only, not casting.");
                if (!ready) throw new InvalidOperationException("Bardic Suggestion fixture capability or ownership failed.");

                // Only the already-owned disposable target gets this native
                // empty aggro script. The read-only probe observes its real
                // OnAggroAction call; no memory/roll/effect is supplied.
                aggro = ScriptableObject.CreateInstance<UnitAggroFilter>();
                aggro.ActionsOnAggro = new ActionList { Actions = Array.Empty<GameAction>() };
                aggro.FilterCondition = new ConditionsChecker();
                target.Blueprint.ComponentsArray = oldComponents.Concat(new BlueprintComponent[] { aggro }).ToArray();
                using (var offensive = new Bardic32OffensiveEvents(owner, target, aggro))
                {
                    foreach (string stage in new[] { "successful-save", "high-sr", "effect-invisibility" })
                    {
                        var before = new HashSet<UnitUseAbility>(casts());
                        int savesBefore = events.Saves.Count, srBefore = events.Resistances.Count;
                        int globalBefore = offensive.Global.Count, aggroBefore = offensive.Aggro.Count;
                        if (stage == "successful-save") target.Stats.SaveWill.BaseValue = 100;
                        else target.Stats.SaveWill.BaseValue = -100;
                        if (stage == "high-sr")
                        {
                            var grant = ScriptableObject.CreateInstance<AddSpellResistance>();
                            grant.Value = new ContextValue { ValueType = ContextValueType.Simple, Value = 100 };
                            grant.AddCR = false;
                            srFact = ScriptableObject.CreateInstance<BlueprintFeature>();
                            srFact.name = "KMG_Runtime_Bardic32_HighSR"; srFact.Ranks = 1;
                            srFact.ComponentsArray = new BlueprintComponent[] { grant };
                            target.Descriptor.AddFact(srFact);
                        }
                        if (stage == "effect-invisibility")
                        {
                            target.Descriptor.RemoveFact(srFact);
                            if (target.Descriptor.State.HasCondition(UnitCondition.SeeInvisibility))
                                throw new InvalidOperationException("Unexpected native SeeInvisibility fixture intake.");
                            target.Descriptor.State.AddCondition(UnitCondition.SeeInvisibility, null); seeAdded = true;
                            var blueprint = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                                "525f980cb29bc2240b93e953974cb325",
                                "native ordinary Invisibility for bardic reaction proof");
                            invisible = ApplySprint12Buff(owner, blueprint, owner, TimeSpan.FromMinutes(2), 17).AppliedBuff;
                            if (invisible == null || !invisible.Active ||
                                !owner.Descriptor.State.HasCondition(UnitCondition.Invisible) ||
                                !target.Descriptor.IsSeeInvisibility || !retained() ||
                                !data.CanTarget(new TargetWrapper(target)))
                                throw new InvalidOperationException("Ordinary invisibility/own visible performance fixture failed.");
                        }
                        Sprint28SetBrainActivePreservingActions(owner, true);
                        var deadline = DateTime.UtcNow.AddSeconds(stage == "effect-invisibility" ? 32 : 22);
                        while (DateTime.UtcNow < deadline)
                        {
                            if (!retained()) throw new InvalidOperationException("Bardic attempt ended its own Fascinate or created hostile memory.");
                            var finished = casts().Where(value => !before.Contains(value) && value.IsFinished &&
                                value.ExecutionProcess != null && value.ExecutionProcess.IsEnded).ToArray();
                            if (finished.Length > 0 && (stage != "effect-invisibility" || suggestion() != null || finished.Length >= 3)) break;
                            yield return 0;
                        }
                        Sprint28SetBrainActivePreservingActions(owner, false);
                        var attempts = casts().Where(value => !before.Contains(value)).ToArray();
                        var saved = events.Saves.Skip(savesBefore).Where(value =>
                            ReferenceEquals(value.Reason?.Context?.SourceAbility, ability)).ToArray();
                        var resisted = events.Resistances.Skip(srBefore).Where(value =>
                            ReferenceEquals(value.Context?.SourceAbility, ability)).ToArray();
                        bool normal = attempts.Length >= 1 && attempts.Length <= (stage == "effect-invisibility" ? 3 : 1) &&
                            attempts.All(value => value.IsActed && value.IsFinished && value.Result == UnitCommand.ResultType.Success &&
                                value.ExecutionProcess != null && value.ExecutionProcess.IsEnded &&
                                value.Type == UnitCommand.CommandType.Standard && !value.Cutscene && !value.IsIgnoreCooldown &&
                                economy.Count(value) == 1 && economy.Unchanged(value)) &&
                            offensive.Global.Count - globalBefore == attempts.Length &&
                            offensive.Aggro.Count - aggroBefore == attempts.Length && retained() && budgets();
                        bool outcome = stage == "successful-save" ?
                            saved.Length == 1 && saved[0].IsPassed && suggestion() == null :
                            stage == "high-sr" ? resisted.Length == 1 && resisted[0].SpellResistance == 100 &&
                                resisted[0].IsSpellResisted && saved.Length == 0 && suggestion() == null :
                            saved.Length == attempts.Length && saved.Any(value => !value.IsPassed) && suggestion() != null &&
                                invisible.IsDisposed && !owner.Descriptor.State.HasCondition(UnitCondition.Invisible) &&
                                suggestion().Context.Params.CasterLevel == 7 &&
                                suggestion().Context.Params.DC == 13 + owner.Stats.Charisma.Bonus;
                        bool pass = normal && outcome && saved.All(value => value.DifficultyClass == 13 + owner.Stats.Charisma.Bonus);
                        Sprint25Check("bardic-" + stage, pass, new JObject { ["commands"] = new JArray(attempts.Select(Sprint27AiCommand)),
                            ["costs"] = costs, ["saves"] = new JArray(saved.Select(value => new JObject {
                                ["dc"] = value.DifficultyClass, ["passed"] = value.IsPassed })),
                            ["sr"] = new JArray(resisted.Select(Astral31ResistanceRow)),
                            ["globalEvents"] = offensive.Global.Count - globalBefore, ["aggroCallbacks"] = offensive.Aggro.Count - aggroBefore,
                            ["performanceRetained"] = retained(), ["upkeep"] = rounds.Rows,
                            ["slots"] = new JArray(Enumerable.Range(0, 4).Select(book.GetSpontaneousSlots)),
                            ["siblingPool"] = sibling.Descriptor.Resources.GetResourceAmount(resource),
                            ["invisibilityRemoved"] = invisible?.IsDisposed, ["effectSeconds"] = suggestion()?.TimeLeft.TotalSeconds,
                            ["fixtureBardicCasts"] = 0, ["fixtureDispatcherRounds"] = 0, ["forcedRolls"] = 0 },
                            "Natural ordinary Standard bardic attempt, native Will/SR, same exact Fascinate and no added hostile memory. Pool debit is solely observed native upkeep; exploration cooldowns remain native. Aggro/global notifications and ordinary invisibility reaction are preserved.");
                        if (!pass) throw new InvalidOperationException("Bardic Suggestion native " + stage + " outcome failed.");
                    }
                    int completed = casts().Length;
                    var end = Game.Instance.Player.GameTime + TimeSpan.FromSeconds(7);
                    var noRepeatLimit = DateTime.UtcNow.AddSeconds(20);
                    Sprint28SetBrainActivePreservingActions(owner, true);
                    while (Game.Instance.Player.GameTime < end && DateTime.UtcNow < noRepeatLimit)
                    { if (!retained()) throw new InvalidOperationException("Bardic active-effect refusal lost the performance."); yield return 0; }
                    Sprint28SetBrainActivePreservingActions(owner, false);
                    bool refusal = Game.Instance.Player.GameTime >= end && casts().Length == completed &&
                        suggestion() != null && !data.CanTarget(new TargetWrapper(target)) && retained() && budgets();
                    Sprint25Check("bardic-active-effect-refusal", refusal, new JObject {
                        ["before"] = completed, ["after"] = casts().Length, ["upkeep"] = rounds.Rows,
                        ["retained"] = retained() }, "One further real native round refuses an already suggested subject; no extra cast, slot or exceptional round cost.");
                    if (!refusal) throw new InvalidOperationException("Bardic active-effect refusal failed.");

                    // Case boundary after all positive verdicts: remove only
                    // the captured optional effect, retain actual Fascinate,
                    // and reinstate the disclosed high-SR fixture. A normal
                    // ordinary book spell must keep its hostile memory path.
                    suggestion().Remove(); target.Descriptor.AddFact(srFact);
                    var ordinary = book.GetKnownSpells(2).Single(value =>
                        ReferenceEquals(value.Blueprint, owner.Blueprint.GetComponent<LillendBardBook32>().OptionalSuggestion));
                    var ordinaryContext = new AbilityExecutionContext(ordinary, ordinary.CalculateParams(),
                        new TargetWrapper(target), Rulebook.CurrentContext);
                    bool ordinaryReady = retained() && ordinary.IsAvailable && ordinary.CanTarget(new TargetWrapper(target)) &&
                        !LillendBardicMemory32.Qualifies(ordinaryContext, owner, target);
                    if (!ordinaryReady) throw new InvalidOperationException("Ordinary book negative cannot use normal native targeting.");
                    int globals = offensive.Global.Count, aggros = offensive.Aggro.Count, oldSlot = book.GetSpontaneousSlots(2);
                    var negative = new UnitUseAbility(ordinary, new TargetWrapper(target));
                    owner.Commands.Run(negative);
                    var negativeLimit = DateTime.UtcNow.AddSeconds(18);
                    while (DateTime.UtcNow < negativeLimit && (!negative.IsFinished ||
                        negative.ExecutionProcess == null || !negative.ExecutionProcess.IsEnded))
                    { if (!mode.Current) throw new InvalidOperationException("Bardic negative native lease changed."); yield return 0; }
                    bool ordinaryPass = negative.IsActed && negative.IsFinished && negative.Result == UnitCommand.ResultType.Success &&
                        negative.ExecutionProcess != null && negative.ExecutionProcess.IsEnded &&
                        negative.Type == UnitCommand.CommandType.Standard && !negative.Cutscene && !negative.IsIgnoreCooldown &&
                        economy.Count(negative) == 1 && (economy.InCombat(negative) ? economy.Exact(negative, false) : economy.Unchanged(negative)) &&
                        book.GetSpontaneousSlots(2) == oldSlot - 1 && otherBook.GetSpontaneousSlots(2) == otherSlots[2] &&
                        offensive.Global.Count == globals + 1 && offensive.Aggro.Count == aggros + 1 &&
                        owner.Memory.Contains(target) && target.Memory.Contains(owner) && !source.Owns(target) &&
                        captured.IsDisposed && !LillendBardicMemory32.Qualifies(ownContext, owner, target);
                    Sprint25Check("bardic-ordinary-book-negative", ordinaryPass, new JObject {
                        ["command"] = Sprint27AiCommand(negative), ["costs"] = costs,
                        ["slot"] = oldSlot + "->" + book.GetSpontaneousSlots(2),
                        ["reciprocalMemory"] = new JArray(owner.Memory.Contains(target), target.Memory.Contains(owner)),
                        ["fascinateDisposed"] = captured.IsDisposed, ["globalDelta"] = offensive.Global.Count - globals,
                        ["aggroDelta"] = offensive.Aggro.Count - aggros, ["fixtureOrdinaryCastCommands"] = 1 },
                        "The explicitly supplied normal book cast is a negative integration probe: real cost/slot, ordinary hostile memory and Fascinate interruption. It is not natural bardic AI evidence.");
                    if (!ordinaryPass) throw new InvalidOperationException("Ordinary book offensive behavior was not preserved.");
                }
            }
            finally
            {
                Sprint28SetBrainActivePreservingActions(owner, false);
                if (invisible != null && !invisible.IsDisposed) invisible.Remove();
                if (seeAdded) target.Descriptor.State.RemoveCondition(UnitCondition.SeeInvisibility);
                if (srFact != null) { target.Descriptor.RemoveFact(srFact);
                    foreach (var component in srFact.ComponentsArray) UnityEngine.Object.DestroyImmediate(component);
                    UnityEngine.Object.DestroyImmediate(srFact); }
                target.Blueprint.ComponentsArray = oldComponents;
                if (aggro != null) UnityEngine.Object.DestroyImmediate(aggro);
            }
        }

        private sealed class Bardic32OffensiveEvents : IDisposable, IUnitMakeOffensiveActionGlobalHandler
        {
            private static Bardic32OffensiveEvents active;
            private readonly UnitEntityData owner, target;
            private readonly UnitAggroFilter filter;
            private readonly HarmonyInstance hooks;
            private readonly MethodInfo method;
            private readonly string id = "KMG.Bardic32OffensiveEvents." + Guid.NewGuid().ToString("N");
            internal readonly JArray Global = new JArray(), Aggro = new JArray();
            internal Bardic32OffensiveEvents(UnitEntityData owner, UnitEntityData target, UnitAggroFilter filter)
            {
                if (active != null) throw new InvalidOperationException("Nested bardic offensive observation.");
                this.owner = owner; this.target = target; this.filter = filter;
                method = AccessTools.Method(typeof(UnitAggroFilter), "OnAggroAction",
                    new[] { typeof(UnitEntityData), typeof(UnitEntityData) });
                hooks = HarmonyInstance.Create(id);
                hooks.Patch(method, new HarmonyMethod(typeof(Bardic32OffensiveEvents), nameof(AggroNotification)));
                active = this; EventBus.Subscribe(this);
            }
            private static void AggroNotification(UnitAggroFilter __instance, UnitEntityData __0, UnitEntityData __1)
            {
                if (active != null && ReferenceEquals(__instance, active.filter) &&
                    ReferenceEquals(__0, active.target) && ReferenceEquals(__1, active.owner))
                    active.Aggro.Add(Game.Instance.Player.GameTime.TotalSeconds);
            }
            public void HandleUnitMakeOffensiveAction(UnitEntityData unit, UnitEntityData target)
            {
                if (ReferenceEquals(unit, owner) && ReferenceEquals(target, this.target))
                    Global.Add(Game.Instance.Player.GameTime.TotalSeconds);
            }
            public void Dispose()
            {
                EventBus.Unsubscribe(this); hooks.Unpatch(method, HarmonyPatchType.All, id);
                if (ReferenceEquals(active, this)) active = null;
            }
        }
    }
}
