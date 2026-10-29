using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects.SriptZones;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static int Trumpet31PreparedCopies(Spellbook book, BlueprintAbility spell, int level)
        { return book.GetMemorizedSpellSlots(level).Count(slot => slot.Available && ReferenceEquals(slot.Spell?.Blueprint, spell)); }

        private IEnumerable<int> Trumpet31PreparedCommand(UnitEntityData actor, Spellbook book, BlueprintAbility spell, string id)
        {
            var slot = book.GetMemorizedSpellSlots(3).Single(value => value.Available &&
                ReferenceEquals(value.Spell?.Blueprint, spell));
            slot.Spell.ParamSpellSlot = slot;
            var data = slot.Spell; var target = new TargetWrapper(actor); var parameters = data.CalculateParams();
            if (!data.IsAvailable || !data.CanTarget(target) || parameters.CasterLevel != 14 || parameters.SpellLevel != 3)
                throw new InvalidOperationException("Prepared Purge native command preconditions failed.");
            var command = new UnitUseAbility(data, target); var costs = new JArray();
            using (var economy = new FirearmReviewCosts(actor, costs))
            {
                actor.Commands.Run(command);
                var deadline = DateTime.UtcNow.AddSeconds(30);
                while (DateTime.UtcNow < deadline && (!command.IsFinished ||
                    command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded))
                { Game.Instance.IsPaused = false; yield return 0; }
                bool exact = command.IsFinished && command.Result == UnitCommand.ResultType.Success &&
                    command.ExecutionProcess != null && command.ExecutionProcess.IsEnded &&
                    !command.Cutscene && !command.IsIgnoreCooldown && command.AiAction == null &&
                    economy.Count(command) == 1 && economy.Exact(command, false) && !slot.Available;
                Sprint25Check(id, exact, new JObject { ["actor"] = actor.UniqueId, ["spell"] = spell.AssetGuid,
                    ["target"] = actor.UniqueId, ["cl"] = parameters.CasterLevel, ["spellLevel"] = parameters.SpellLevel,
                    ["command"] = Sprint27AiCommand(command), ["executionEnded"] = command.ExecutionProcess?.IsEnded,
                    ["costs"] = costs, ["ownSlotAvailableAfter"] = slot.Available, ["manualNativeCast"] = true,
                    ["fixtureSlotDebits"] = 0, ["fixtureEffectExecutions"] = 0 },
                    "One manual native prepared cast completes, pays one real-time standard action and spends its own slot; this row makes no AI claim.");
                if (!exact) throw new InvalidOperationException("Native prepared Purge command/action/slot failed.");
            }
        }

        private IEnumerable<int> ReviewTrumpet31Purge(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData sibling, UnitEntityData natural = null)
        {
            var spell = fixture.Blueprints.OfType<BlueprintAbility>().Single(value => value.name ==
                "KMG_Summoning_OutsiderCelestial_TrumpetArchon_InvisibilityPurge");
            var carrier = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name ==
                "KMG_Summoning_OutsiderCelestial_TrumpetArchon_PurgeCarrier");
            var recipient = fixture.Blueprints.OfType<BlueprintBuff>().Single(value => value.name ==
                "KMG_Summoning_OutsiderCelestial_TrumpetArchon_PurgeRecipient");
            var area = fixture.Blueprints.OfType<BlueprintAbilityAreaEffect>().Single(value => value.name ==
                "KMG_Summoning_OutsiderCelestial_TrumpetArchon_PurgeArea");
            var creation = owner.Blueprint.GetComponent<TrumpetPreparedBook>();
            var book = owner.Descriptor.GetSpellbook(creation.Book); var otherBook = sibling.Descriptor.GetSpellbook(creation.Book);
            var nativeInvisibility = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                "525f980cb29bc2240b93e953974cb325", "captured native InvisibilityBuff control");
            UnitEntityData ally = null; var original = new List<Buff>();
            try
            {
                ally = CastExpandedSummoningOwnTier(fixture, "lemure");
                Sprint28SetBrainActivePreservingActions(ally, false); ally.Commands.InterruptAll(true);
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, fixture.Hostile, 8f,
                    out far, out lane, out direction, new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(sibling, origin + Vector3.right);
                PlaceExpandedSummoningUnit(fixture.Hostile, far); PlaceExpandedSummoningUnit(ally, far + Vector3.right);
                if (natural != null) PlaceExpandedSummoningUnit(natural, far - Vector3.right);
                foreach (var unit in new[] { ally, fixture.Hostile })
                    original.Add(unit.Buffs.AddBuff(nativeInvisibility,
                        new MechanicsContext(fixture.Caster, unit.Descriptor, nativeInvisibility, null, new TargetWrapper(unit)),
                        TimeSpan.FromMinutes(10)));
                var deadlines = original.Select(buff => buff?.EndTime).ToArray();
                bool before = original.All(buff => buff != null) && new[] { ally, fixture.Hostile }
                    .All(unit => unit.Descriptor.State.HasCondition(UnitCondition.Invisible)) &&
                    !owner.IsEnemy(ally) && owner.IsEnemy(fixture.Hostile) &&
                    Trumpet31PreparedCopies(book, spell, 3) == 1 && Trumpet31PreparedCopies(otherBook, spell, 3) == 1 &&
                    !Game.Instance.State.AreaEffects.Any(value => ReferenceEquals(value.Blueprint, area) && !value.IsEnded);
                Sprint25Check("trumpet-purge-preconditions", before, new JObject { ["owner"] = owner.UniqueId,
                    ["sibling"] = sibling.UniqueId, ["ally"] = ally.UniqueId, ["enemy"] = fixture.Hostile.UniqueId,
                    ["nativeInvisibility"] = nativeInvisibility.AssetGuid, ["lane"] = lane,
                    ["fixtureNativeStatusGrants"] = original.Count, ["purgeSlots"] = Trumpet31PreparedCopies(book, spell, 3),
                    ["siblingPurgeSlots"] = Trumpet31PreparedCopies(otherBook, spell, 3) },
                    "Two disposable eligible units, one ally and one enemy, really start invisible; both native prepared Purge slots are independently available and no prior Purge area exists.");
                if (!before) throw new InvalidOperationException("Purge invisibility/ownership preconditions failed.");
                foreach (int frame in Trumpet31PreparedCommand(owner, book, spell, "trumpet-purge-first-native-cast")) yield return frame;
                foreach (int frame in Trumpet31PreparedCommand(sibling, otherBook, spell, "trumpet-purge-second-native-cast")) yield return frame;
                Func<UnitEntityData, int> grants = unit => unit.Buffs.Enumerable.Count(buff => ReferenceEquals(buff.Blueprint, recipient));
                var deadline = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < deadline && new[] { ally, fixture.Hostile }.Any(unit => grants(unit) != 2)) yield return 0;
                var areas = Game.Instance.State.AreaEffects.Where(value => ReferenceEquals(value.Blueprint, area) && !value.IsEnded).ToArray();
                bool covered = areas.Length == 2 && areas.All(value => value.View.Shape is ScriptZoneCylinder &&
                    Math.Abs(((ScriptZoneCylinder)value.View.Shape).Radius - new Feet(70).Meters) < 0.001f &&
                    value.Context.Params.CasterLevel == 14) && new[] { owner, sibling }.All(source =>
                        areas.Count(value => ReferenceEquals(value.Context.MaybeCaster, source)) == 1) &&
                    new[] { ally, fixture.Hostile }.All(unit => grants(unit) == 2 &&
                        !unit.Descriptor.State.HasCondition(UnitCondition.Invisible)) &&
                    original.Select((buff, index) => buff.Active && !buff.IsDisposed && buff.EndTime == deadlines[index]).All(value => value) &&
                    Trumpet31PreparedCopies(book, spell, 3) == 0 && Trumpet31PreparedCopies(otherBook, spell, 3) == 0 &&
                    Trumpet31HealSlots(book, creation.Heal) == 1 && Trumpet31HealSlots(otherBook, creation.Heal) == 2;
                Sprint25Check("trumpet-purge-mixed-sources", covered, new JObject { ["owner"] = owner.UniqueId,
                    ["sibling"] = sibling.UniqueId, ["allyGrants"] = grants(ally), ["enemyGrants"] = grants(fixture.Hostile),
                    ["areas"] = new JArray(areas.Select(value => new JObject { ["id"] = value.UniqueId,
                        ["caster"] = value.Context.MaybeCaster?.UniqueId, ["cl"] = value.Context.Params.CasterLevel,
                        ["radiusMeters"] = (value.View.Shape as ScriptZoneCylinder)?.Radius })),
                    ["originalInvisibilityPreserved"] = original.All(buff => buff.Active && !buff.IsDisposed),
                    ["ownerHealSlots"] = Trumpet31HealSlots(book, creation.Heal), ["siblingHealSlots"] = Trumpet31HealSlots(otherBook, creation.Heal) },
                    "Two independent native70ft emanations suppress ally and enemy invisibility while preserving the original buff instances/deadlines; each spends one own L3 slot and leaves Heal independent.");
                if (!covered) throw new InvalidOperationException("Purge source/radius/eligibility consumers failed.");
                if (natural != null)
                {
                    bool exempt = areas.All(value => value.UnitsInside.Contains(natural)) && grants(natural) == 0 &&
                        natural.Descriptor.State.HasCondition(UnitCondition.Invisible) && natural.Buffs.Enumerable.Any(value =>
                            value.Blueprint.AssetGuid == "94b2838e8a492c44ebf89e7fe7a75a62" && value.IsTurnedOn);
                    Sprint25Check("sprint33-natural-invisibility-purge-exemption", exempt, new JObject {
                        ["owner"] = natural.UniqueId, ["insideBothNativeAreas"] = areas.All(value => value.UnitsInside.Contains(natural)),
                        ["suppressionGrants"] = grants(natural), ["invisible"] = natural.Descriptor.State.HasCondition(UnitCondition.Invisible) },
                        "Natural invisibility survives both actual Purge areas while ordinary ally/enemy invisibility is suppressed.");
                    if (!exempt) throw new InvalidOperationException("Natural invisibility Purge exemption failed.");
                }
                PlaceExpandedSummoningUnit(fixture.Hostile, origin + Vector3.forward * new Feet(90).Meters);
                deadline = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < deadline && (grants(fixture.Hostile) != 0 ||
                    !fixture.Hostile.Descriptor.State.HasCondition(UnitCondition.Invisible))) yield return 0;
                bool left = grants(fixture.Hostile) == 0 && fixture.Hostile.Descriptor.State.HasCondition(UnitCondition.Invisible) &&
                    grants(ally) == 2 && !ally.Descriptor.State.HasCondition(UnitCondition.Invisible);
                PlaceExpandedSummoningUnit(fixture.Hostile, far);
                deadline = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < deadline && grants(fixture.Hostile) != 2) yield return 0;
                bool returned = grants(fixture.Hostile) == 2 && !fixture.Hostile.Descriptor.State.HasCondition(UnitCondition.Invisible);
                Sprint25Check("trumpet-purge-exit-reentry", left && returned, new JObject {
                    ["exitObserved"] = left, ["reentryObserved"] = returned, ["enemyGrantsAfterReentry"] = grants(fixture.Hostile),
                    ["allyGrants"] = grants(ally) },
                    "Leaving both emanations restores the original invisibility, with the covered ally unchanged; re-entry suppresses it again without another cast or slot.");
                if (!left || !returned) throw new InvalidOperationException("Purge exit/re-entry failed.");
                owner.Buffs.Enumerable.Single(buff => ReferenceEquals(buff.Blueprint, carrier)).Remove();
                deadline = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < deadline && new[] { ally, fixture.Hostile }.Any(unit => grants(unit) != 1)) yield return 0;
                bool one = new[] { ally, fixture.Hostile }.All(unit => grants(unit) == 1 &&
                    !unit.Descriptor.State.HasCondition(UnitCondition.Invisible));
                sibling.Buffs.Enumerable.Single(buff => ReferenceEquals(buff.Blueprint, carrier)).Remove();
                deadline = DateTime.UtcNow.AddSeconds(10);
                while (DateTime.UtcNow < deadline && new[] { ally, fixture.Hostile }.Any(unit => grants(unit) != 0)) yield return 0;
                bool clear = new[] { ally, fixture.Hostile }.All(unit => grants(unit) == 0 &&
                    unit.Descriptor.State.HasCondition(UnitCondition.Invisible)) &&
                    !Game.Instance.State.AreaEffects.Any(value => ReferenceEquals(value.Blueprint, area) && !value.IsEnded) &&
                    original.Select((buff, index) => buff.Active && !buff.IsDisposed && buff.EndTime == deadlines[index]).All(value => value);
                Sprint25Check("trumpet-purge-source-cleanup", one && clear, new JObject { ["firstRemovalLeavesOtherSource"] = one,
                    ["allOwnedAreasAndGrantsAbsent"] = clear, ["originalBuffsRetained"] = original.All(buff => buff.Active && !buff.IsDisposed),
                    ["ownerPurgeSlots"] = Trumpet31PreparedCopies(book, spell, 3), ["siblingPurgeSlots"] = Trumpet31PreparedCopies(otherBook, spell, 3) },
                    "Removing one exact emitter preserves the other's suppression; ending the second removes all owned grants/areas and restores still-live foreign invisibility without refreshing any slot.");
                if (!one || !clear) throw new InvalidOperationException("Purge exact-source cleanup failed.");
                if (natural != null && (!natural.Descriptor.State.HasCondition(UnitCondition.Invisible) || grants(natural) != 0))
                    throw new InvalidOperationException("Natural invisibility changed during Purge source cleanup.");
            }
            finally
            {
                foreach (var source in new[] { owner, sibling })
                    foreach (var buff in source.Buffs.Enumerable.Where(value => ReferenceEquals(value.Blueprint, carrier)).ToArray()) buff.Remove();
                foreach (var buff in original.Where(value => value != null && !value.IsDisposed)) buff.Remove();
                if (ally != null && !ally.Destroyed) DisposeExpandedSummoningUnits(fixture.Created, new[] { ally });
            }
        }
    }
}
