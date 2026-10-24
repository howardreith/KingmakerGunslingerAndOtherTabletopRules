using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewTrumpet31Blast(ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            var units = new List<UnitEntityData>();
            TrumpetBlastObservation observed = null;
            Buff freedom = null;
            UnitPartSpellResistance resistance = null;
            int resistanceId = -1;
            var instrumentBaseline = Trumpet31InstrumentOwnedCounts();
            var instrumentResources = new List<UnityEngine.Object>();
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                CreateExpandedSummoningCorrectionHostile(fixture);
                var owner = CastExpandedSummoningOwnTier(fixture, "trumpet-archon"); units.Add(owner);
                var sibling = CastExpandedSummoningOwnTier(fixture, "trumpet-archon"); units.Add(sibling);
                var victim = CastExpandedSummoningOwnTier(fixture, "lemure", fixture.Hostile); units.Add(victim);
                var ally = CastExpandedSummoningOwnTier(fixture, "lemure"); units.Add(ally);
                var protectedAlly = CastExpandedSummoningOwnTier(fixture, "lemure"); units.Add(protectedAlly);
                var far = CastExpandedSummoningOwnTier(fixture, "lemure", fixture.Hostile); units.Add(far);
                var hound = CastExpandedSummoningOwnTier(fixture, "hound-archon"); units.Add(hound);
                var hostileHound = CastExpandedSummoningOwnTier(fixture, "hound-archon", fixture.Hostile); units.Add(hostileHound);
                foreach (var unit in units)
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                Vector3 farPoint; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, fixture.Hostile, 8f,
                    out farPoint, out lane, out direction, new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(sibling, origin + Vector3.right * 2f);
                PlaceExpandedSummoningUnit(victim, origin + Vector3.forward * 3f);
                PlaceExpandedSummoningUnit(ally, origin - Vector3.right * 3f);
                PlaceExpandedSummoningUnit(protectedAlly, origin - Vector3.right * 5f);
                PlaceExpandedSummoningUnit(hound, origin + Vector3.forward * 5f);
                PlaceExpandedSummoningUnit(hostileHound, origin + Vector3.forward * 7f);
                PlaceExpandedSummoningUnit(far, origin + Vector3.forward * 35f);
                PlaceExpandedSummoningUnit(fixture.Hostile, farPoint);
                // The fixture caster is disposable, but must not be a victim
                // of this deliberately indiscriminate effect.
                PlaceExpandedSummoningUnit(fixture.Caster, origin + Vector3.back * 40f);
                foreach (var unit in units.Concat(new[] { fixture.Hostile }))
                {
                    unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                    unit.SightCache.ClearIfNeeded(); unit.JoinCombat();
                }
                owner.Memory.Add(victim); victim.Memory.Add(owner);
                sibling.Memory.Add(victim); fixture.Hostile.Memory.Add(owner);
                Game.Instance.Player.UpdateIsInCombat(); new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                for (int frame = 0; frame < 4; frame++) yield return 0;
                var blast = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[ExpandedSummoningSpecialBuilder.Trumpet31Prefix + "TrumpetBlast"];
                var paralysis = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol[ExpandedSummoningSpecialBuilder.Trumpet31Prefix + "TrumpetParalyzed"];
                var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(blast));
                var otherData = new AbilityData(sibling.Descriptor.Abilities.GetAbility(blast));
                var bookSpec = owner.Blueprint.GetComponent<TrumpetPreparedBook>();
                var book = owner.Descriptor.GetSpellbook(bookSpec.Book);
                var otherBook = sibling.Descriptor.GetSpellbook(bookSpec.Book);
                Func<int[]> slots = () => Enumerable.Range(1, 7).Select(level => book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null))
                    .Concat(Enumerable.Range(1, 7).Select(level => otherBook.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null))).ToArray();
                int[] slotsBefore = slots();
                var normal = data.CalculateParams();
                owner.Stats.Charisma.BaseValue = 21;
                var raised = data.CalculateParams();
                owner.Stats.Charisma.BaseValue = 17;
                bool ready = owner.IsInCombat && sibling.IsInCombat && data.IsAvailable && otherData.IsAvailable &&
                    normal.DC == 20 && raised.DC == 22 && normal.CasterLevel == 14 && normal.SpellLevel == 0 &&
                    data.Spellbook == null && blast.Type == AbilityType.Supernatural && !blast.SpellResistance &&
                    blast.Icon != null && ReferenceEquals(blast.Icon, paralysis.Icon);
                Sprint25Check("trumpet-blast-access", ready, new JObject { ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                    ["ability"] = blast.AssetGuid, ["buff"] = paralysis.AssetGuid, ["adultDc"] = normal.DC, ["cha21Dc"] = raised.DC,
                    ["cl"] = normal.CasterLevel, ["book"] = data.Spellbook?.Blueprint.AssetGuid,
                    ["ownerCombat"] = owner.IsInCombat, ["siblingCombat"] = sibling.IsInCombat, ["slots"] = new JArray(slotsBefore),
                    ["lane"] = lane, ["icon"] = blast.Icon?.name },
                    "Real own-instrument supernatural access uses live Charisma/racial HD, a real original shared effect icon and no prepared book; both disposable sources have native combat enrollment.");
                if (!ready) throw new InvalidOperationException("Trumpet blast access fixture failed.");
                var freedomBlueprint = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                    ExpandedSummoningSpecialBuilder.NativeFreedomBuff31Guid, "native freedom immunity fixture");
                freedom = protectedAlly.Descriptor.AddBuff(freedomBlueprint, owner, TimeSpan.FromMinutes(5));
                resistance = victim.Ensure<UnitPartSpellResistance>(); resistanceId = resistance.AddResistance(1000, null, null);
                bool immunityReady = freedom != null && protectedAlly.Descriptor.State.HasConditionImmunity(UnitCondition.Paralyzed);
                Sprint25Check("trumpet-blast-native-immunity-fixture", immunityReady, new JObject { ["target"] = protectedAlly.UniqueId,
                    ["nativeBuff"] = freedomBlueprint.AssetGuid, ["paralysisImmunity"] = protectedAlly.Descriptor.State.HasConditionImmunity(UnitCondition.Paralyzed),
                    ["srTarget"] = victim.UniqueId, ["fixtureSr"] = 1000, ["fixtureBuffNotCastProof"] = true },
                    "The controlled native Freedom of Movement fixture supplies actual paralysis immunity; SR1000 is confined to the owned victim. Neither fixture is an action/AI proof.");
                if (!immunityReady) throw new InvalidOperationException("Trumpet blast immunity fixture failed.");
                var appearanceDeadline = DateTime.UtcNow.AddSeconds(8);
                while (DateTime.UtcNow < appearanceDeadline &&
                    (!(bool)Trumpet31InstrumentState(owner, "waiting")["swordVisible"] ||
                     !(bool)Trumpet31InstrumentState(sibling, "waiting")["swordVisible"]))
                { Game.Instance.IsPaused = false; yield return 0; }
                var donorBefore = Trumpet31InstrumentDonorState(owner);
                var firstSword = Trumpet31InstrumentState(owner, "before-real-blast");
                var siblingSword = Trumpet31InstrumentState(sibling, "sibling-before-real-blast");
                var ownerResources = Trumpet31InstrumentOwnedResources(owner);
                var siblingResources = Trumpet31InstrumentOwnedResources(sibling);
                instrumentResources.AddRange(ownerResources); instrumentResources.AddRange(siblingResources);
                bool ownedForms = (bool)firstSword["swordVisible"] && !(bool)firstSword["hornVisible"] &&
                    (bool)siblingSword["swordVisible"] && (bool)firstSword["weaponOwnerExact"] &&
                    (bool)firstSword["weaponWielderExact"] && !(bool)firstSword["weaponLootable"] &&
                    (bool)firstSword["nativeModelUnderOwner"] && (bool)firstSword["hornUnderNativeModel"] &&
                    ownerResources.Length >= 6 && siblingResources.Length >= 6 && !ownerResources.Intersect(siblingResources).Any();
                Sprint25Check("trumpet-instrument-owned-forms", ownedForms, new JObject {
                    ["owner"] = firstSword, ["sibling"] = siblingSword, ["donor"] = donorBefore,
                    ["ownerResources"] = new JArray(ownerResources.Select(value => value.GetInstanceID())),
                    ["siblingResources"] = new JArray(siblingResources.Select(value => value.GetInstanceID())),
                    ["createdItems"] = 0, ["observerViewMutations"] = 0 },
                    "Two real player summons bind independent original horn meshes/materials beneath their own native weapon models. The same nonlootable owned +4 swords remain authoritative; observations create no item or renderer state.");
                if (!ownedForms) throw new InvalidOperationException("Owned trumpet/sword presentation is not coherent.");
                observed = new TrumpetBlastObservation(blast, paralysis, owner, sibling); EventBus.Subscribe(observed);
                var formSamples = new JArray(); string hornCamera = null; bool sawHorn = false, overlap = false;
                foreach (int frame in Trumpet31ManualCommand(owner, data, owner, "trumpet-blast-first-command"))
                {
                    var form = Trumpet31InstrumentState(owner, "during-real-native-command");
                    formSamples.Add(form); overlap |= (bool)form["bothFormsVisible"];
                    if ((bool)form["hornVisible"])
                    {
                        sawHorn = true;
                        if (hornCamera == null) hornCamera = WriteExpandedSummoningPartyCameraCapture(owner,
                            _request.EvidenceDirectory, "s31-trumpet-horn-party.png");
                    }
                    yield return frame;
                }
                // Observe the subsequent real Unity LateUpdate rather than
                // forcing Sync or a view state from the observer.
                for (int frame = 0; frame < 2; frame++) yield return 0;
                var returnedSword = Trumpet31InstrumentState(owner, "after-real-blast");
                var siblingAfter = Trumpet31InstrumentState(sibling, "sibling-after-real-blast");
                string swordCamera = WriteExpandedSummoningPartyCameraCapture(owner,
                    _request.EvidenceDirectory, "s31-trumpet-sword-party.png");
                bool formResolved = sawHorn && !overlap && (bool)returnedSword["swordVisible"] &&
                    !(bool)returnedSword["hornVisible"] && (bool)siblingAfter["swordVisible"] &&
                    !(bool)siblingAfter["hornVisible"] && (bool)returnedSword["weaponOwnerExact"] &&
                    (bool)returnedSword["weaponWielderExact"] && !(bool)returnedSword["weaponLootable"] &&
                    hornCamera != null && hornCamera.Contains("inFrame=true") && hornCamera.Contains("screenLit=true") &&
                    swordCamera.Contains("inFrame=true") && swordCamera.Contains("screenLit=true") &&
                    JToken.DeepEquals(donorBefore, Trumpet31InstrumentDonorState(owner));
                Sprint25Check("trumpet-instrument-native-transition", formResolved, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId, ["samples"] = formSamples,
                    ["hornCamera"] = hornCamera, ["swordCamera"] = swordCamera,
                    ["returnedSword"] = returnedSword, ["siblingAfter"] = siblingAfter,
                    ["donorBefore"] = donorBefore, ["donorAfter"] = Trumpet31InstrumentDonorState(owner),
                    ["observerViewMutations"] = 0, ["fixtureFormCommands"] = 0, ["createdItems"] = 0 },
                    "The actual native blast command presents the original horn and then the owned sword, with real party-camera frames, no overlapping forms, no sibling/donor mutation and no replacement item or action. This is usable presentation, not owner aesthetic approval.");
                if (!formResolved) throw new InvalidOperationException("Actual native trumpet/sword transition was not established.");
                Game.Instance.IsPaused = true;
                var excluded = new[] { owner, sibling, hound, hostileHound, far };
                var eligible = new[] { victim, ally, protectedAlly };
                bool targeting = eligible.All(unit => observed.Saves.Any(row => (string)row["target"] == unit.UniqueId)) &&
                    excluded.All(unit => !observed.Saves.Any(row => (string)row["target"] == unit.UniqueId)) &&
                    observed.Saves.All(row => (int)row["dc"] == 20 && (string)row["type"] == "Fortitude") &&
                    !protectedAlly.Descriptor.State.HasCondition(UnitCondition.Paralyzed);
                Sprint25Check("trumpet-blast-all-creatures", targeting, new JObject { ["caster"] = owner.UniqueId,
                    ["eligible"] = new JArray(eligible.Select(unit => unit.UniqueId)), ["excluded"] = new JArray(excluded.Select(unit => unit.UniqueId)),
                    ["alliedVictim"] = ally.UniqueId, ["mindlessVictim"] = victim.UniqueId, ["hostileArchon"] = hostileHound.UniqueId,
                    ["distances"] = new JArray(units.Select(unit => new JObject { ["unit"] = unit.UniqueId,
                        ["meters"] = Vector3.Distance(owner.Position, unit.Position), ["enemy"] = owner.IsEnemy(unit) })),
                    ["saves"] = observed.Saves, ["effects"] = observed.Effects, ["effectEvents"] = observed.EffectEvents, ["dice"] = observed.Dice, ["srChecks"] = observed.Resistance },
                    "A real blast tests friendly and hostile nonarchons, including mindless Lemures; native Freedom immunity holds. Both factions' archons and the outside-radius unit are exempt. No save is fabricated.");
                if (!targeting) throw new InvalidOperationException("Trumpet blast native target consumer failed.");
                bool effects = observed.Saves.Any(row => !(bool)row["passed"] && (string)row["target"] != protectedAlly.UniqueId) &&
                    observed.Effects.Any(row => (bool)row["applied"] && (bool)row["nativeParalyzed"] && (string)row["target"] == victim.UniqueId) &&
                    observed.Effects.Where(row => (bool)row["applied"]).All(row => (int)row["dc"] == 20 && (int)row["cl"] == 14 &&
                        (double)row["durationSeconds"] >= 6 && (double)row["durationSeconds"] <= 24 && (bool?)row["notDispelable"] == true) &&
                    observed.Dice.Count > 0 && observed.Dice.All(row => (int)row["nativeResult"] >= 1 && (int)row["nativeResult"] <= 4) &&
                    observed.Resistance.Any(row => (string)row["target"] == victim.UniqueId && (int)row["resistance"] == 1000) &&
                    observed.Resistance.All(row => !(bool)row["hasRoll"] && !(bool)row["resisted"]) &&
                    observed.Effects.Count == observed.Dice.Count &&
                    observed.Effects.All(effect => observed.Dice.Any(roll =>
                        (string)roll["caster"] == (string)effect["caster"] &&
                        (string)roll["target"] == (string)effect["target"] &&
                        (int)roll["nativeResult"] * 6d == (double)effect["durationSeconds"])) &&
                    slotsBefore.SequenceEqual(slots());
                Sprint25Check("trumpet-blast-live-effect", effects, new JObject { ["saves"] = observed.Saves.DeepClone(),
                    ["effects"] = observed.Effects.DeepClone(), ["effectEvents"] = observed.EffectEvents.DeepClone(), ["dice"] = observed.Dice.DeepClone(),
                    ["srChecks"] = observed.Resistance.DeepClone(), ["slotsBefore"] = new JArray(slotsBefore), ["slotsAfter"] = new JArray(slots()) },
                    "Real failed Fortitude saves and native d4 rolls produce independent finite nondispellable paralysis through SR1000 without using either book's slots; native immunity remains decisive.");
                if (!effects) throw new InvalidOperationException("Trumpet blast effect preconditions/outcome not established; inspect actual saves.");
                var live = units.SelectMany(unit => unit.Buffs.Enumerable).Where(buff => ReferenceEquals(buff.Blueprint, paralysis)).ToArray();
                var ownedVictimEffects = live.Where(buff => ReferenceEquals(buff.Context?.MaybeCaster, owner)).ToArray();
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                // Match the proven owned-orb teardown observation: native
                // entity disposal queues Unity view/component destruction.
                // Retain the paused victim effects while observing real frames.
                for (int frame = 0; frame < 12; frame++) yield return 0;
                bool instrumentReleased = ownerResources.All(value => value == null) &&
                    siblingResources.All(value => value != null) && JToken.DeepEquals(donorBefore, Trumpet31InstrumentDonorState(sibling));
                Sprint25Check("trumpet-instrument-owner-isolation", instrumentReleased, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                    ["ownerPrivateResourcesAbsent"] = ownerResources.All(value => value == null),
                    ["siblingPrivateResourcesAlive"] = siblingResources.All(value => value != null),
                    ["donorUnchanged"] = JToken.DeepEquals(donorBefore, Trumpet31InstrumentDonorState(sibling)) },
                    "After real source disposal and twelve Unity destruction frames, its private horn resources are absent while the sibling and native donor remain intact.");
                if (!instrumentReleased) throw new InvalidOperationException("Trumpet private instrument teardown/isolation failed.");
                bool retained = owner.Destroyed && !blast.GetComponent<TrumpetBlastAccess>().CorrectCaster(owner) &&
                    ownedVictimEffects.Length > 0 && ownedVictimEffects.All(buff => !buff.IsDisposed);
                Sprint25Check("trumpet-blast-source-loss", retained, new JObject { ["source"] = owner.UniqueId, ["sourceDestroyed"] = owner.Destroyed,
                    ["victimEffects"] = new JArray(ownedVictimEffects.Select(buff => new JObject { ["buff"] = buff.Blueprint.AssetGuid,
                        ["disposed"] = buff.IsDisposed, ["end"] = buff.EndTime.ToString(), ["caster"] = buff.Context?.MaybeCaster?.UniqueId })),
                    ["sourceAccessAfterLoss"] = blast.GetComponent<TrumpetBlastAccess>().CorrectCaster(owner) },
                    "Removing the owned emitter closes future instrument access while already applied finite victim effects retain their own native lifetime.");
                if (!retained) throw new InvalidOperationException("Trumpet blast victim effect was erased with its source.");
                var expiryDeadline = DateTime.UtcNow.AddSeconds(35);
                while (DateTime.UtcNow < expiryDeadline && ownedVictimEffects.Any(buff => !buff.IsDisposed))
                { Game.Instance.IsPaused = false; yield return 0; }
                int[] siblingSlotsAfter = Enumerable.Range(1, 7).Select(level => otherBook.GetMemorizedSpellSlots(level).Count(slot => slot.Available && slot.Spell != null)).ToArray();
                bool expired = ownedVictimEffects.All(buff => buff.IsDisposed) && siblingSlotsAfter.SequenceEqual(slotsBefore.Skip(7));
                Sprint25Check("trumpet-blast-native-expiry", expired, new JObject { ["effects"] = ownedVictimEffects.Length,
                    ["allDisposed"] = ownedVictimEffects.All(buff => buff.IsDisposed), ["siblingSlotsAfter"] = new JArray(siblingSlotsAfter) },
                    "The real victim effects expire naturally after their rolled durations; no blanket source-loss deletion or fixture removal supplies the expiry verdict.");
                if (!expired) throw new InvalidOperationException("Trumpet blast finite native expiry failed.");
            }
            finally
            {
                if (observed != null) EventBus.Unsubscribe(observed);
                if (resistance != null && resistanceId >= 0) resistance.Remove(resistanceId);
                if (freedom != null && !freedom.IsDisposed) freedom.Remove();
                foreach (var unit in units.Where(value => value != null && !value.Destroyed))
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); }
                DisposeExpandedSummoningUnits(fixture.Created, units.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture); SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
            for (int frame = 0; frame < 12; frame++) yield return 0;
            var instrumentAfter = Trumpet31InstrumentOwnedCounts();
            Sprint25Check("trumpet-instrument-private-cleanup", instrumentResources.All(value => value == null) &&
                JToken.DeepEquals(instrumentBaseline, instrumentAfter), new JObject {
                    ["before"] = instrumentBaseline, ["after"] = instrumentAfter,
                    ["capturedPrivateResources"] = instrumentResources.Count,
                    ["allCapturedResourcesAbsent"] = instrumentResources.All(value => value == null) },
                "All request-owned instrument components, roots, meshes and materials return to the observed baseline after real actor disposal; no donor asset is deleted.");
        }
    }
}
