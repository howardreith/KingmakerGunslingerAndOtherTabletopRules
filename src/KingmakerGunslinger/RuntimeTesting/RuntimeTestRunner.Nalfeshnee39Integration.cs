using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
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
        /// <summary>
        /// The two places where this sprint reaches outside its own creature,
        /// observed on live units.
        ///
        /// <para>Everything else Sprint 39 added is private to the Nalfeshnee:
        /// its own blueprints, its own components, its own route. Exactly two
        /// things are shared, and both are therefore the sprint's highest
        /// integration risk.</para>
        ///
        /// <para><b>Native heal.</b> Teaching the installed <i>heal</i> spell
        /// to cure this project's feeblemind clone edits a NATIVE blueprint's
        /// action graph - one every consumer of heal in the game walks. The
        /// insertion is beside the native removal rather than instead of it,
        /// so what has to be shown is that heal still cures what it always
        /// cured.</para>
        ///
        /// <para><b>The demon set.</b> The authoritative demon identity
        /// catalog grew from seven members to eight, and the released Sprint
        /// 37 Vrock's Stunning Screech resolves its printed demon exemption
        /// from that same catalog. So a Nalfeshnee must now be spared by a
        /// Vrock's screech, and that is a behaviour change in a RELEASED
        /// creature caused by this sprint. Asserting it here is what makes the
        /// catalog growth visible rather than incidental.</para>
        /// </summary>
        private IEnumerable<int> ReviewNalfeshneeIntegration39(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint39NalfeshneeIntegrationPhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            int oldWill = target.Stats.SaveWill.BaseValue;
            int oldFort = target.Stats.SaveFortitude.BaseValue;
            int oldHp = target.Stats.HitPoints.BaseValue;
            int oldDamage = target.Damage;
            Vector3 oldPosition = target.Position;
            Vector3 oldCasterPosition = fixture.Caster.Position;
            BlueprintFaction oldFaction = target.Faction;
            var loan = new Nalfeshnee39CureLoan();
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint39-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint39 nalfeshnee integration cell failed: " + id);
                };
            try
            {
                Nalfeshnee39HealGraph(check);
                target.Stats.HitPoints.BaseValue = 100000;
                target.Damage = 0;
                foreach (int frame in Nalfeshnee39HealCuresBoth(fixture,
                    owned, target, loan, check)) yield return frame;
                foreach (int frame in Nalfeshnee39VrockExemption(fixture,
                    owned, target, check)) yield return frame;
            }
            finally
            {
                if (loan.PartyAdded)
                    Game.Instance.Player.Party.Remove(target);
                target.Descriptor.SwitchFactions(oldFaction, false);
                if (loan.Heal != null)
                    fixture.Caster.Descriptor.RemoveFact(loan.Heal);
                foreach (Buff buff in target.Buffs.Enumerable.ToArray())
                    if (buff.Blueprint != null && (buff.Blueprint.name
                        .StartsWith("KMG_", StringComparison.Ordinal) ||
                        buff.Blueprint.AssetGuid ==
                            ExpandedSummoningSpecialBuilder
                                .NativeFeeblemindBuff39))
                        buff.Remove();
                target.Stats.SaveWill.BaseValue = oldWill;
                target.Stats.SaveFortitude.BaseValue = oldFort;
                target.Stats.HitPoints.BaseValue = oldHp;
                target.Damage = oldDamage;
                PlaceExpandedSummoningUnit(target, oldPosition);
                PlaceExpandedSummoningUnit(fixture.Caster, oldCasterPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint39-nalfeshnee-integration.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        /// <summary>
        /// The native heal action graph, read as it now stands.
        ///
        /// <para>This is a structural reading rather than a behavioural one,
        /// and it is here because behaviour alone cannot see it: a cast that
        /// cures feeblemind says nothing about whether blindness, dazzled,
        /// stunned, daze and insanity are still cured beside it. The clone's
        /// removal must sit in the SAME action list as the native one - which
        /// is what makes it inherit that list's undead condition - and that
        /// list must still hold the other cures it always held.</para>
        /// </summary>
        private static void Nalfeshnee39HealGraph(
            Action<string, bool, JObject, string> check)
        {
            var heal = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                BlueprintBootstrap.Library,
                ExpandedSummoningSpecialBuilder.NativeHeal39, "native Heal");
            var clone = (BlueprintBuff)ExpandedSummoningBlueprints.Registered
                .BySymbol[ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix +
                    "FeeblemindCondition"];
            var native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                BlueprintBootstrap.Library,
                ExpandedSummoningSpecialBuilder.NativeFeeblemindBuff39,
                "native Feeblemind condition");

            ActionList host = null;
            foreach (AbilityEffectRunAction run in
                heal.GetComponents<AbilityEffectRunAction>())
            {
                ActionList found = Nalfeshnee39FindRemovalList(run.Actions,
                    native);
                if (found != null) host = found;
            }
            ContextActionRemoveBuff[] removals = host == null ?
                new ContextActionRemoveBuff[0] :
                host.Actions.OfType<ContextActionRemoveBuff>().ToArray();
            int cloneRemovals = removals.Count(value =>
                ReferenceEquals(value.Buff, clone));
            int otherRemovals = removals.Count(value =>
                !ReferenceEquals(value.Buff, clone) &&
                !ReferenceEquals(value.Buff, native));
            check("nalfeshnee-integration-heal-graph",
                host != null && cloneRemovals == 1 &&
                    removals.Any(value =>
                        ReferenceEquals(value.Buff, native)) &&
                    otherRemovals >= 4,
                new JObject {
                    ["nativeHeal"] =
                        ExpandedSummoningSpecialBuilder.NativeHeal39,
                    ["hostListFound"] = host != null,
                    ["removalsInHostList"] = removals.Length,
                    ["cloneRemovals"] = cloneRemovals,
                    ["nativeFeeblemindRemovalPresent"] = removals.Any(value =>
                        ReferenceEquals(value.Buff, native)),
                    ["otherNativeCuresInTheSameList"] = otherRemovals,
                    ["removedConditions"] = new JArray(removals
                        .Select(value => value.Buff == null ? "(null)" :
                            value.Buff.name)),
                    ["whyStructural"] = "A cast that cures feeblemind says nothing about whether heal's other cures survived the insertion; only the graph does."
                },
                "This project's feeblemind removal sits in the same native heal action list as the native one - which is what makes it inherit that list's own non-undead condition - and that list still carries the native feeblemind removal and at least four other native cures beside them.");
        }

        /// <summary>
        /// Descends heal's action graph for the list that removes the given
        /// condition, rather than assuming where it lives.
        /// </summary>
        private static ActionList Nalfeshnee39FindRemovalList(
            ActionList actions, BlueprintBuff wanted)
        {
            if (actions == null || actions.Actions == null) return null;
            if (actions.Actions.OfType<ContextActionRemoveBuff>()
                .Any(value => ReferenceEquals(value.Buff, wanted)))
                return actions;
            foreach (GameAction action in actions.Actions)
            {
                if (action == null) continue;
                foreach (var field in action.GetType().GetFields(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic))
                {
                    if (field.FieldType != typeof(ActionList)) continue;
                    ActionList nested = Nalfeshnee39FindRemovalList(
                        field.GetValue(action) as ActionList, wanted);
                    if (nested != null) return nested;
                }
            }
            return null;
        }

        /// <summary>
        /// One native heal cast, on a recipient carrying BOTH the native
        /// feeblemind condition and this project's clone.
        ///
        /// <para>Curing them together in one cast is the decisive form: it
        /// shows the clone's removal was added beside the native one rather
        /// than in place of it, which two separate casts could not
        /// distinguish. The native condition is attached by the fixture
        /// because it is the installed game's own blueprint and not this
        /// sprint's product - what is under observation is heal's graph. The
        /// clone arrives the way a player would meet it, from a real
        /// nalfeshnee's real cast.</para>
        /// </summary>
        private IEnumerable<int> Nalfeshnee39HealCuresBoth(
            ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> owned, UnitEntityData target,
            Nalfeshnee39CureLoan loan,
            Action<string, bool, JObject, string> check)
        {
            var clone = (BlueprintBuff)ExpandedSummoningBlueprints.Registered
                .BySymbol[ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix +
                    "FeeblemindCondition"];
            var nativeCondition =
                BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                    BlueprintBootstrap.Library,
                    ExpandedSummoningSpecialBuilder.NativeFeeblemindBuff39,
                    "native Feeblemind condition");
            BlueprintAbility feeblemind = Nalfeshnee39Spell(
                ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix +
                    "Feeblemind");

            UnitEntityData nalfeshnee = CastExpandedSummoningOwnTier(
                fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
            owned.Add(nalfeshnee);
            VrockQuiesce37(nalfeshnee);
            foreach (int frame in Outsider30NativeAppearance(nalfeshnee,
                nalfeshnee.Blueprint.name)) yield return frame;
            PlaceExpandedSummoningUnit(target, nalfeshnee.Position +
                Vector3.right * (nalfeshnee.Corpulence +
                    target.Corpulence + 1f));
            nalfeshnee.Memory.Add(target).Visible = true;
            target.Memory.Add(nalfeshnee).Visible = true;
            target.Stats.SaveWill.BaseValue = -100;

            // The clone, through the production cast, with the same bounded
            // natural-twenty retry the spells phase uses.
            Buff cloned = null;
            var rolls = new JArray();
            for (int attempt = 1; attempt <= 6 && cloned == null; attempt++)
            {
                var watcher = new Nalfeshnee39SpellWatcher(nalfeshnee,
                    feeblemind, target);
                EventBus.Subscribe(watcher);
                try
                {
                    UnitUseAbility ignored = null;
                    foreach (int frame in Sprint26Cast(nalfeshnee, feeblemind,
                        new TargetWrapper(target), value => ignored = value))
                        yield return frame;
                }
                finally { EventBus.Unsubscribe(watcher); }
                RuleSavingThrow save = watcher.Saves.Count == 1 ?
                    watcher.Saves[0] : null;
                cloned = target.Descriptor.Buffs.GetBuff(clone);
                rolls.Add(new JObject {
                    ["attempt"] = attempt,
                    ["saveD20"] = save == null ? null : (int?)save.D20.Value,
                    ["savePassed"] = save == null ? null :
                        (bool?)save.IsPassed,
                    ["cloneApplied"] = cloned != null });
                if (cloned == null && (save == null ||
                    save.D20.Value != 20)) break;
            }

            // The native condition, by its own identity, attached by the
            // fixture: it is the installed game's blueprint, and heal's graph
            // is what this cell observes.
            Buff nativeApplied = target.Descriptor.AddBuff(nativeCondition,
                nalfeshnee);
            bool bothStood = cloned != null && nativeApplied != null &&
                target.Descriptor.Buffs.GetBuff(clone) != null &&
                target.Descriptor.Buffs.GetBuff(nativeCondition) != null;

            var heal = fixture.Blueprints.OfType<BlueprintAbility>().Single(
                value => value.AssetGuid ==
                    ExpandedSummoningSpecialBuilder.NativeHeal39);
            Kingmaker.UI.SettingsUI.SettingsRoot.Instance.EnableTurnBasedMode
                .CurrentValue = false;
            Game.Instance.TurnBasedCombatController.Activate();
            target.Descriptor.SwitchFactions(fixture.Caster.Faction, false);
            Game.Instance.Player.Party.Add(target);
            loan.PartyAdded = true;
            loan.Heal = fixture.Caster.Descriptor.AddFact(heal) as Ability;
            PlaceExpandedSummoningUnit(fixture.Caster, target.Position +
                Vector3.right * (target.Corpulence +
                    fixture.Caster.Corpulence + 0.5f));
            foreach (int frame in Sprint26Cast(fixture.Caster, heal,
                new TargetWrapper(target), value => { })) yield return frame;
            for (int frame = 0; frame < 16; frame++) yield return 0;

            check("nalfeshnee-integration-heal-cures-both",
                bothStood &&
                    target.Descriptor.Buffs.GetBuff(clone) == null &&
                    target.Descriptor.Buffs.GetBuff(nativeCondition) == null &&
                    !(bool)target.Descriptor.State.SpellCastingForbidden,
                new JObject {
                    ["nativeHeal"] = heal.AssetGuid,
                    ["recipient"] = target.UniqueId,
                    ["projectCloneAttempts"] = rolls,
                    ["bothConditionsStoodBeforeTheCure"] = bothStood,
                    ["projectCloneAfterCure"] =
                        target.Descriptor.Buffs.GetBuff(clone) != null,
                    ["nativeConditionAfterCure"] = target.Descriptor.Buffs
                        .GetBuff(nativeCondition) != null,
                    ["spellCastingForbiddenAfterCure"] =
                        (bool)target.Descriptor.State.SpellCastingForbidden,
                    ["nativeConditionAttachedBy"] = "the fixture, by its own identity: it is the installed game's blueprint and heal's graph is what this cell observes",
                    ["removedByObserver"] = false
                },
                "ONE native heal cast removes this project's feeblemind clone and the installed game's own feeblemind condition together from the same recipient, so the clone's removal was added beside the native one rather than in place of it.");
        }

        /// <summary>
        /// The released Vrock's Stunning Screech, now that the authoritative
        /// demon set has grown to eight.
        ///
        /// <para>This sprint changed the behaviour of a RELEASED creature:
        /// the Vrock resolves its printed demon exemption from the same
        /// catalog the Nalfeshnee joined, so a Vrock's screech must now spare
        /// a Nalfeshnee. A non-demon control standing in the same radius is
        /// what keeps the cell from passing on a screech that simply did
        /// nothing, and the released exemption is re-measured on a succubus
        /// so the growth is shown to have added a member rather than replaced
        /// the set.</para>
        /// </summary>
        private IEnumerable<int> Nalfeshnee39VrockExemption(
            ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> owned, UnitEntityData target,
            Action<string, bool, JObject, string> check)
        {
            var screech = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                    .Vrock37Prefix + "Screech"];
            var stunned = (BlueprintBuff)ExpandedSummoningBlueprints
                .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                    .Vrock37Prefix + "Stunned"];

            UnitEntityData vrock = CastExpandedSummoningOwnTier(
                fixture, VrockVisualPolicy37.Vrock);
            UnitEntityData nalfeshnee = CastExpandedSummoningOwnTier(
                fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
            UnitEntityData succubus = CastExpandedSummoningOwnTier(
                fixture, "succubus");
            UnitEntityData devil = CastExpandedSummoningOwnTier(
                fixture, "bone-devil");
            foreach (UnitEntityData unit in
                new[] { vrock, nalfeshnee, succubus, devil })
            {
                owned.Add(unit);
                VrockQuiesce37(unit);
                unit.Stats.SaveFortitude.BaseValue = -100;
                foreach (int frame in WaitInfernalSupport34(unit))
                    yield return frame;
            }
            foreach (int frame in Outsider30NativeAppearance(vrock,
                vrock.Blueprint.name)) yield return frame;
            target.Stats.SaveFortitude.BaseValue = -100;

            // Everything inside the printed screech radius.
            UnitEntityData[] inside = { nalfeshnee, succubus, devil, target };
            VrockRing37(vrock, inside,
                VrockRules37Policy.ScreechRadiusFeet - 5f);

            var observed = new Astral31SmiteEvents(vrock, screech, stunned,
                null, inside);
            EventBus.Subscribe(observed);
            try
            {
                vrock.JoinCombat();
                target.JoinCombat();
                UnitUseAbility command = null;
                foreach (int frame in Sprint26Cast(vrock, screech,
                    new TargetWrapper(vrock), value => command = value))
                    yield return frame;
                for (int frame = 0; frame < 16; frame++) yield return 0;

                Func<UnitEntityData, bool> affected = unit =>
                    observed.Save(unit) != null ||
                    unit.Descriptor.Buffs.GetBuff(stunned) != null;
                check("nalfeshnee-integration-vrock-exemption",
                    command != null &&
                        command.Result == UnitCommand.ResultType.Success &&
                        !affected(nalfeshnee) && !affected(succubus) &&
                        affected(devil) && affected(target) &&
                        SummonDemonIdentityCatalog.IsDemonKey(
                            NalfeshneeVisualPolicy39.Nalfeshnee) &&
                        SummonDemonIdentityCatalog.UnitSymbols().Count() == 8,
                    new JObject {
                        ["vrock"] = vrock.UniqueId,
                        ["screech"] = screech.AssetGuid,
                        ["commandResult"] = command == null ? null :
                            command.Result.ToString(),
                        ["nalfeshneeAffected"] = affected(nalfeshnee),
                        ["succubusAffected"] = affected(succubus),
                        ["boneDevilAffected"] = affected(devil),
                        ["nonDemonHostileAffected"] = affected(target),
                        ["nalfeshneeIsNowADemonKey"] =
                            SummonDemonIdentityCatalog.IsDemonKey(
                                NalfeshneeVisualPolicy39.Nalfeshnee),
                        ["authoritativeDemonCount"] =
                            SummonDemonIdentityCatalog.UnitSymbols().Count(),
                        ["radiusFeet"] =
                            VrockRules37Policy.ScreechRadiusFeet,
                        ["whyThisIsIntegration"] = "Sprint 39 changed the behaviour of a released creature: the Vrock reads its printed demon exemption from the same catalog the Nalfeshnee joined, so this is the growth from seven members to eight arriving at its other consumer."
                    },
                    "The released Vrock's Stunning Screech now spares a Nalfeshnee as a demon, still spares the succubus it always spared, and still affects a registered devil and a non-demon enemy standing in the same radius - so the authoritative demon set gained a member rather than changing meaning.");
            }
            finally { EventBus.Unsubscribe(observed); }
        }
    }
}
