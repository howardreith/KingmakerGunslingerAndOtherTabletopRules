using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.ElementalRaces;
using KingmakerGunslinger.Firearms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal static class ElementalBreezeKissedScenario
    {
        // The main menu has no BattleLogView. Replace only the final UI sink
        // during one exact disposable firearm attack; keep message validation,
        // AC calculation, ammunition commitment and all rule handlers native.
        private sealed class FirearmLogCapture : IPlayerCombatLogSink, IDisposable
        {
            private readonly object _service;
            private readonly FieldInfo _field;
            private readonly IPlayerCombatLogSink _prior;
            internal int Messages;

            internal FirearmLogCapture()
            {
                FieldInfo service = typeof(NativeCombatLog).GetField("Service", BindingFlags.Static | BindingFlags.NonPublic);
                _field = typeof(PlayerCombatLogPublicationService).GetField("_sink", BindingFlags.Instance | BindingFlags.NonPublic);
                if (service == null || _field == null) throw new InvalidOperationException("Exact native log sink boundary missing.");
                _service = service.GetValue(null);
                _prior = _field.GetValue(_service) as IPlayerCombatLogSink;
                if (_prior == null || _prior is FirearmLogCapture) throw new InvalidOperationException("Ambiguous native log sink.");
                _field.SetValue(_service, this);
            }
            public void Add(string message)
            {
                // This synchronous scope includes genuine misfire feedback as
                // well as AC. The publication service has already validated
                // every message; only its unavailable main-menu UI is isolated.
                Messages++;
            }
            public void Dispose()
            {
                _field.SetValue(_service, _prior);
                if (!ReferenceEquals(_field.GetValue(_service), _prior))
                    throw new InvalidOperationException("Exact native log sink was not restored.");
            }
        }

        private sealed class Probe : IGlobalRulebookHandler<RuleCombatManeuver>
        {
            internal UnitEntityData Caster, Target;
            internal BlueprintAbilityResource Resource;
            internal readonly List<RuleCombatManeuver> Rules = new List<RuleCombatManeuver>();
            internal int AmountAtManeuver = -1;
            public void OnEventAboutToTrigger(RuleCombatManeuver evt)
            {
                if (ReferenceEquals(evt.Initiator, Caster) && ReferenceEquals(evt.Target, Target))
                    AmountAtManeuver = Caster.Descriptor.Resources.GetResourceAmount(Resource);
            }
            public void OnEventDidTrigger(RuleCombatManeuver evt)
            { if (ReferenceEquals(evt.Initiator, Caster) && ReferenceEquals(evt.Target, Target)) Rules.Add(evt); }
        }

        internal static void Exercise(RuntimeTestRequest request, ICollection<RuntimeTestAssertion> assertions,
            ICollection<string> files)
        {
            var rows = new JArray();
            var diagnostics = new List<string>();
            UnitEntityData[] before = Game.Instance.State.Units.All.ToArray();
            UnityEngine.Random.State random = UnityEngine.Random.state;
            TimeSpan clock = Game.Instance.TimeController.GameTime;
            UnitHandEquipmentController priorHands = Game.Instance.HandsEquipmentController;
            UnitHandEquipmentController ownedHands = priorHands == null ? new UnitHandEquipmentController() : null;
            MethodInfo handSetter = typeof(Game).GetProperty("HandsEquipmentController").GetSetMethod(true);
            if (Game.Instance.ProjectileController.Projectiles.Any() ||
                TurnBased.Controllers.CombatController.IsInTurnBasedCombat() || handSetter == null)
                throw new InvalidOperationException("Breeze RTWP fixture requires idle native controllers.");
            if (ownedHands != null) handSetter.Invoke(Game.Instance, new object[] { ownedHands });
            try
            {
                ElementalRaceBlueprints race = BlueprintBootstrap.ElementalRaces.Sylph;
                foreach (ElementalHeritageBlueprints heritage in race.Heritages.Choices())
                {
                    var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
                    try
                    {
                        UnitEntityData defender = fixture.Caster;
                        BlueprintFaction hostile = UnityEngine.Object.Instantiate(defender.Blueprint.Faction);
                        hostile.name = "KMG_Runtime_Breeze_Hostile";
                        hostile.Peaceful = hostile.AlwaysEnemy = hostile.Neutral = hostile.IsDirectlyControllable = false;
                        hostile.Dummy = null;
                        hostile.AttackFactions = new[] { defender.Blueprint.Faction };
                        UnitEntityData attacker = fixture.SpawnFixtureUnit(race.Race, hostile, new Vector3(3, 0, 0), "BreezeAttacker");
                        foreach (UnitEntityData unit in new[] { defender, attacker })
                        {
                            unit.CombatState.JoinCombat();
                            unit.CombatState.OnNewRound();
                            unit.Stats.HitPoints.BaseValue = 10000;
                            unit.Damage = 0;
                        }
                        defender.Memory.Add(attacker);
                        attacker.Memory.Add(defender);
                        Check(assertions, rows, heritage.Definition.Id + "-hostility",
                            defender.IsEnemy(attacker) && attacker.IsEnemy(defender), "native request-local hostile factions");
                        Run(defender, attacker, race, heritage, rows, assertions);
                    }
                    finally
                    {
                        ClearProjectiles();
                        fixture.Dispose();
                        Game.Instance.Player.GameTime = clock;
                        Check(assertions, rows, heritage.Definition.Id + "-native-fixture-lifetime",
                            fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                            fixture.NativeObservationReleased && fixture.NativeTeardownObserved &&
                            fixture.AreaContextRestored && fixture.PlayerContextRestored,
                            "errors=" + fixture.NativeErrors + ";exceptions=" + fixture.NativeExceptions);
                    }
                }
            }
            finally
            {
                if (ownedHands != null) handSetter.Invoke(Game.Instance, new object[] { priorHands });
                UnityEngine.Random.state = random;
                Game.Instance.Player.GameTime = clock;
                bool clean = before.Length == Game.Instance.State.Units.All.Count &&
                    before.All(value => Game.Instance.State.Units.All.Contains(value)) &&
                    !Game.Instance.ProjectileController.Projectiles.Any() &&
                    ReferenceEquals(Game.Instance.HandsEquipmentController, priorHands);
                Check(assertions, rows, "fixture-cleanup", clean, "exact original units, clock, random, projectiles and controller");
                string path = Path.Combine(request.EvidenceDirectory, "elemental-breeze-kissed.json");
                File.WriteAllText(path, new JObject { { "schemaVersion", 1 }, { "saveStateTouched", false },
                    { "cleanupExact", clean }, { "isolatedBoundary", "request-local animation timing, absent hand controller and final firearm UI log sink; native publication service, command, cooldown, CMB and attack/AC rules" },
                    { "observations", rows }, { "diagnostics", new JArray(diagnostics) } }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }

        private static void Run(UnitEntityData defender, UnitEntityData attacker, ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage, JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            string prefix = heritage.Definition.Id + "-";
            UnitDescriptor owner = defender.Descriptor;
            var trait = race.AlternateTraits.Require(ElementalAlternateTraitId.BreezeKissed);
            var mechanics = trait.Mechanics();
            BlueprintAbilityResource resource = mechanics.OfType<BlueprintAbilityResource>().Single();
            BlueprintBuff calmBuff = mechanics.OfType<BlueprintBuff>().Single();
            BlueprintAbility gust = Ability(mechanics, ".Gust"), calm = Ability(mechanics, ".CalmWinds"),
                renew = Ability(mechanics, ".RenewWinds");
            BlueprintAbility[] variants = gust.GetComponent<AbilityVariants>().Variants;
            owner.AddFact(heritage.Marker);
            ElementalSpellAffinityScenario.Advance(owner, Exact<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd"), 5);
            ElementalSpellAffinityScenario.Advance(owner, Exact<BlueprintCharacterClass>("ba34257984f4c41408ce1dc2004e342e"), 4);
            owner.Stats.Strength.BaseValue = 18;
            owner.Stats.Charisma.BaseValue = 30;
            owner.Stats.HitPoints.BaseValue = 10000;
            owner.Damage = 0;
            var crossbow = new ItemEntityWeapon(Exact<BlueprintItemWeapon>("19a5092244dcf99478dcd73c974828b1"));
            var sword = new ItemEntityWeapon(Exact<BlueprintItemWeapon>("57c8994d1f1becf49ac4f642e5d8ca9d"));
            var firearm = new ItemEntityWeapon(Exact<BlueprintItemWeapon>("a303d71d244640959827e9464df5a867"));
            try
            {
                // Use the same native standing state for controls and later
                // attacks. Get-up itself adds +4 ranged AC at fixture time zero.
                Game.Instance.Player.GameTime += TimeSpan.FromSeconds(1);
                if (attacker.View.IsGetUp || defender.View.IsGetUp)
                    throw new InvalidOperationException("Native standing baseline is not ready.");
                int rangedBase = Attack(attacker, defender, crossbow).TargetAC;
                int meleeBase = Attack(attacker, defender, sword).TargetAC;
                int firearmBase = Attack(attacker, defender, firearm).TargetAC;
                owner.AddFact(trait.Marker);
                Check(assertions, rows, prefix + "owned-graph", owner.HasFact(trait.Provider) &&
                    !owner.HasFact(heritage.Affinity) && owner.HasFact(race.Resistance) &&
                    owner.Resources.GetResourceAmount(resource) == 1 &&
                    owner.Abilities.GetAbility(heritage.SlaAbility) != null &&
                    variants.Length == 2 && variants.All(value => ReferenceEquals(value.Parent, gust) &&
                        value.Type == AbilityType.Supernatural && value.ActionType == UnitCommand.CommandType.Standard &&
                        value.Range == AbilityRange.Custom && value.CustomRange == 30.Feet() &&
                        !value.SpellResistance && !value.CanTargetSelf && !value.CanTargetPoint) &&
                    ElementalTraitDailyResourceRuntime.IsExact(owner, race.AlternateTraits),
                    "exact affinity replacement; own daily gust plus two independent swift controls; native range/action");
                Armor(attacker, defender, crossbow, rangedBase, 2, prefix + "plain-crossbow", assertions, rows);
                Armor(attacker, defender, sword, meleeBase, 0, prefix + "melee-excluded", assertions, rows);
                Armor(attacker, defender, firearm, firearmBase, 2, prefix + "nonmagical-firearm", assertions, rows);
                var context = new MechanicsContext(attacker, attacker.Descriptor, crossbow.Blueprint, null, new TargetWrapper(defender));
                ItemEnchantment masterwork = crossbow.AddEnchantment(
                    Exact<BlueprintWeaponEnchantment>(EasternWeaponBlueprints.NativeMasterworkGuid), context, new Rounds(10));
                try { Armor(attacker, defender, crossbow, rangedBase, 2, prefix + "masterwork", assertions, rows); }
                finally { crossbow.RemoveEnchantment(masterwork); }
                ItemEnchantment magic = crossbow.AddEnchantment(
                    Exact<BlueprintWeaponEnchantment>(EasternWeaponBlueprints.NativeEnhancementOneGuid), context, new Rounds(10));
                try { Armor(attacker, defender, crossbow, rangedBase, 0, prefix + "temporary-plus-one", assertions, rows); }
                finally { crossbow.RemoveEnchantment(magic); }
                Armor(attacker, defender, crossbow, rangedBase, 2, prefix + "temporary-magic-removed", assertions, rows);

                Cast(defender, Data(owner, calm), new TargetWrapper(defender), resource, 1, assertions, rows, prefix + "calm");
                Buff originalCalm = owner.Buffs.GetBuff(calmBuff);
                Check(assertions, rows, prefix + "calm-state", originalCalm != null &&
                    !Data(owner, calm).IsAvailable && Data(owner, renew).IsAvailable,
                    "native permanent calm=" + (originalCalm != null) + ";calmAvailable=" + Data(owner, calm).IsAvailable +
                    ";renewAvailable=" + Data(owner, renew).IsAvailable);
                if (originalCalm == null) throw new InvalidOperationException("Native Calm command did not apply its actual owned buff.");
                Armor(attacker, defender, crossbow, rangedBase, 0, prefix + "calmed-defense", assertions, rows);
                try
                {
                    owner.TurnOff(false);
                    Check(assertions, rows, prefix + "native-save-suspension", ReferenceEquals(originalCalm, owner.Buffs.GetBuff(calmBuff)),
                        "native suspension preserves the same owned voluntary state");
                }
                finally { owner.TurnOn(); }
                Check(assertions, rows, prefix + "native-save-reactivation", ReferenceEquals(originalCalm, owner.Buffs.GetBuff(calmBuff)) &&
                    owner.Resources.GetResourceAmount(resource) == 1, "no reconstructed calm or resource drift");
                Cast(defender, Data(owner, renew), new TargetWrapper(defender), resource, 1, assertions, rows, prefix + "renew");
                Armor(attacker, defender, crossbow, rangedBase, 2, prefix + "renewed-defense", assertions, rows);
                var racial = owner.Stats.AC.AddModifier(4, owner.GetFact(heritage.Marker), "breeze-racial-control", ModifierDescriptor.Racial);
                try
                {
                    Check(assertions, rows, prefix + "native-racial-stacking-rule", racial.Stacks,
                        "native DefaultStackingDescriptors includes separate racial modifiers");
                    Armor(attacker, defender, crossbow, rangedBase, 6, prefix + "native-racial-layering", assertions, rows);
                }
                finally { owner.Stats.AC.RemoveModifier(racial); }

                foreach (BlueprintAbility variant in variants)
                {
                    foreach (string outcome in new[] { "success", "failure", "immune" })
                    {
                        Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
                        attacker.Stats.AdditionalCMD.BaseValue = outcome == "failure" ? 100 : -100;
                        attacker.Descriptor.State.Prone.Active = false;
                        attacker.Descriptor.State.Prone.ShouldBeActive = false;
                        // Native IsGetUp includes GameTime - m_StartGetUpTime <
                        // 0.5 seconds. A new view at fixture time zero is still
                        // getting up; move this disposable clock, never patch
                        // the eligibility predicate or force a maneuver result.
                        Game.Instance.Player.GameTime += TimeSpan.FromSeconds(1);
                        if (attacker.View == null || attacker.View.IsGetUp)
                            throw new InvalidOperationException("Native target has not finished its get-up timing boundary.");
                        attacker.Damage = 0;
                        attacker.Position = defender.Position + new Vector3(3, 0, 0);
                        if (outcome == "immune") attacker.Descriptor.State.AddCondition(UnitCondition.ImmuneToCombatManeuvers, null);
                        try
                        {
                            CombatManeuver type = variant.GetComponent<AbilityEffectRunAction>().Actions.Actions.OfType<ContextActionCombatManeuver>().Single().Type;
                            int expected = Rulebook.Trigger(new RuleCalculateCMB(defender, attacker, type)).Result;
                            // Native immunity returns before CMB/CMD/dice. Its Success
                            // getter alone is not an outcome witness (default 0 >= 0).
                            RuleCombatManeuver immuneControl = outcome == "immune" ?
                                Rulebook.Trigger(new RuleCombatManeuver(defender, attacker, type)) : null;
                            Vector3 beforeManeuver = attacker.Position;
                            bool beforeProne = attacker.Descriptor.State.Prone.ShouldBeActive;
                            var probe = new Probe { Caster = defender, Target = attacker, Resource = resource };
                            EventBus.Subscribe(probe);
                            try { Cast(defender, Data(owner, variant), new TargetWrapper(attacker), resource, 0, assertions, rows,
                                prefix + type + "-" + outcome); }
                            finally { EventBus.Unsubscribe(probe); }
                            RuleCombatManeuver rule = probe.Rules.SingleOrDefault();
                            bool nativeOutcome = rule != null && (immuneControl == null ?
                                rule.InitiatorCMB == expected && rule.Success == (outcome == "success") :
                                rule.InitiatorCMB == immuneControl.InitiatorCMB &&
                                rule.TargetCMD == immuneControl.TargetCMD &&
                                rule.InitiatorRoll.Value == immuneControl.InitiatorRoll.Value &&
                                rule.AutoFailure == immuneControl.AutoFailure &&
                                rule.ConcealmentCheck == null && immuneControl.ConcealmentCheck == null &&
                                attacker.Position == beforeManeuver &&
                                attacker.Descriptor.State.Prone.ShouldBeActive == beforeProne);
                            bool exact = nativeOutcome && rule.Type == type &&
                                !rule.ReplaceAttackBonus.HasValue && !rule.ReplaceBaseStat.HasValue &&
                                probe.AmountAtManeuver == 0;
                            Check(assertions, rows, prefix + type + "-" + outcome + "-native-rule", exact,
                                "events=" + probe.Rules.Count + ";expectedNativeCMB=" + expected + ";observed=" +
                                (rule == null ? "absent" : rule.InitiatorCMB + ";roll=" + rule.InitiatorRoll.Value +
                                    ";successGetter=" + rule.Success + ";autoFailure=" + rule.AutoFailure) +
                                ";nativeImmuneControl=" + (immuneControl != null) + ";resourceAtRule=" + probe.AmountAtManeuver);
                            Check(assertions, rows, prefix + type + "-" + outcome + "-exhausted",
                                owner.Resources.GetResourceAmount(resource) == 0 && variants.All(value => !Data(owner, value).IsAvailable) &&
                                !Data(owner, calm).IsAvailable && !Data(owner, renew).IsAvailable, "zero use blocks all gusts and renewal");
                            if (type == CombatManeuver.Trip)
                                Check(assertions, rows, prefix + type + "-" + outcome + "-native-prone",
                                    attacker.Descriptor.State.Prone.ShouldBeActive == (outcome == "success"),
                                    "native pending prone=" + attacker.Descriptor.State.Prone.ShouldBeActive);
                            Armor(attacker, defender, crossbow, rangedBase, 0, prefix + type + "-" + outcome + "-no-defense", assertions, rows);
                        }
                        finally { attacker.Descriptor.State.RemoveCondition(UnitCondition.ImmuneToCombatManeuvers); }
                    }
                }

                // Ordinary CMB reacts to current Strength, not the current best mental stat.
                int baseCmb = Rulebook.Trigger(new RuleCalculateCMB(defender, attacker, CombatManeuver.BullRush)).Result;
                var strength = owner.Stats.Strength.AddModifier(4, owner.GetFact(trait.Provider), "breeze-current-strength", ModifierDescriptor.UntypedStackable);
                try
                {
                    int changed = Rulebook.Trigger(new RuleCalculateCMB(defender, attacker, CombatManeuver.BullRush)).Result;
                    Check(assertions, rows, prefix + "current-strength-native-formula", changed == baseCmb + 2,
                        "native CMB before=" + baseCmb + ";temporary Strength +4=" + changed);
                    Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
                    attacker.Stats.AdditionalCMD.BaseValue = -100;
                    Cast(defender, Data(owner, variants[0]), new TargetWrapper(attacker), resource, 0, assertions, rows, prefix + "temporary-strength-gust");
                }
                finally { owner.Stats.Strength.RemoveModifier(strength); }
                var charisma = owner.Stats.Charisma.AddModifier(20, owner.GetFact(trait.Provider), "breeze-mental-exclusion", ModifierDescriptor.UntypedStackable);
                try { Check(assertions, rows, prefix + "mental-not-substituted",
                    Rulebook.Trigger(new RuleCalculateCMB(defender, attacker, CombatManeuver.BullRush)).Result == baseCmb,
                    "temporary Charisma +20 does not replace ordinary native CMB"); }
                finally { owner.Stats.Charisma.RemoveModifier(charisma); }

                ElementalSpellAffinityScenario.Advance(owner, Exact<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd"), 1);
                Check(assertions, rows, prefix + "spent-level-up", owner.Resources.GetResourceAmount(resource) == 0,
                    "native class level-up does not refill a spent gust");
                owner.RemoveFact(trait.Marker);
                owner.AddFact(trait.Marker);
                Check(assertions, rows, prefix + "spent-provider-readd", owner.Resources.GetResourceAmount(resource) == 0,
                    "exact resource memory survives same-day provider re-add");
                Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
                Armor(attacker, defender, crossbow, rangedBase, 2, prefix + "rest-restores-defense", assertions, rows);
                Cast(defender, Data(owner, calm), new TargetWrapper(defender), resource, 1, assertions, rows, prefix + "calm-before-removal");
                owner.RemoveFact(trait.Marker);
                Check(assertions, rows, prefix + "exact-owned-cleanup", !owner.HasFact(calmBuff) &&
                    owner.Resources.PersistantResources.All(value => !ReferenceEquals(value.Blueprint, resource)) &&
                    !owner.HasFact(trait.Provider) && owner.HasFact(heritage.Affinity) &&
                    ElementalTraitDailyResourceRuntime.IsExact(owner, race.AlternateTraits),
                    "marker removal removes only own calm/controls/resource and restores the correct affinity");
            }
            finally
            {
                attacker.Body.PrimaryHand.RemoveItem(false);
                crossbow.Dispose(); sword.Dispose(); firearm.Dispose();
            }
        }

        private static BlueprintAbility Ability(IEnumerable<BlueprintScriptableObject> mechanics, string suffix)
        { return mechanics.OfType<BlueprintAbility>().Single(value => value.name == (ElementalBreezeKissedFactory.Prefix + suffix).Replace('.', '_')); }

        private static AbilityData Data(UnitDescriptor owner, BlueprintAbility ability)
        {
            Ability root = owner.Abilities.GetAbility(ability.Parent ?? ability);
            if (root == null) throw new InvalidOperationException("Exact Breeze ability fact missing.");
            return ability.Parent == null ? new AbilityData(root) : new AbilityData(new AbilityData(root), ability);
        }

        private static void Cast(UnitEntityData caster, AbilityData data, TargetWrapper target, BlueprintAbilityResource resource,
            int expectedResource, ICollection<RuntimeTestAssertion> assertions, JArray rows, string label)
        {
            caster.Commands.InterruptAll(true);
            caster.Commands.RemoveFinishedAndUpdateQueue();
            caster.CombatState.Cooldown.StandardAction = 0;
            caster.CombatState.Cooldown.MoveAction = 0;
            caster.CombatState.Cooldown.SwiftAction = 0;
            if (!data.IsAvailable || !data.CanTarget(target)) throw new InvalidOperationException("Native Breeze cast unavailable: " + label);
            var canceled = new UnitUseAbility(data, target);
            caster.Commands.Run(canceled);
            caster.Commands.InterruptAll(true);
            caster.Commands.RemoveFinishedAndUpdateQueue();
            Check(assertions, rows, label + "-cancel", !canceled.IsStarted && !canceled.IsActed &&
                caster.Descriptor.Resources.GetResourceAmount(resource) == 1 &&
                caster.CombatState.Cooldown.StandardAction == 0 && caster.CombatState.Cooldown.SwiftAction == 0,
                "native queued cancellation spends no action or daily use");
            var command = new UnitUseAbility(data, target);
            caster.Commands.Run(command);
            var controller = new UnitActionController();
            try
            {
                for (int tick = 0; !command.IsActed && !command.IsFinished && tick < 10; tick++)
                {
                    if (command.Animation != null) command.Animation.IsActed = true;
                    ElementalBreathScenario.TickCommand(controller, command);
                }
                for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 100; tick++)
                    command.ExecutionProcess.Tick();
                bool swift = data.Blueprint.ActionType == UnitCommand.CommandType.Swift;
                float cost = Math.Max(0, 6 - command.TimeSinceStart);
                float actual = swift ? caster.CombatState.Cooldown.SwiftAction : caster.CombatState.Cooldown.StandardAction;
                Check(assertions, rows, label + "-native-commit", command.IsStarted && command.IsActed &&
                    !command.Cutscene && !command.IsIgnoreCooldown && command.ExecutionProcess != null &&
                    command.ExecutionProcess.IsEnded && caster.Descriptor.Resources.GetResourceAmount(resource) == expectedResource &&
                    cost > 0 && Math.Abs(actual - cost) < 0.001 &&
                    (swift ? caster.CombatState.Cooldown.StandardAction : caster.CombatState.Cooldown.SwiftAction) == 0 &&
                    caster.CombatState.Cooldown.MoveAction == 0,
                    "acted=" + command.IsActed + ";result=" + command.Result + ";resource=" +
                    caster.Descriptor.Resources.GetResourceAmount(resource) + ";cooldown=" + actual + ";expected=" + cost);
            }
            finally
            {
                if (command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded) command.ExecutionProcess.Detach();
                caster.Commands.InterruptAll(true);
                caster.Commands.RemoveFinishedAndUpdateQueue();
            }
        }

        private static RuleAttackRoll Attack(UnitEntityData attacker, UnitEntityData target, ItemEntityWeapon weapon)
        {
            attacker.Body.PrimaryHand.RemoveItem(false);
            attacker.Body.PrimaryHand.InsertItem(weapon);
            bool firearm = weapon.Blueprint.AssetGuid == "a303d71d244640959827e9464df5a867";
            if (firearm)
                // Seed only a disposable item's legitimate loaded pre-state.
                // The ordinary discharge/misfire pipeline is not bypassed.
                FirearmRuntimeState.Service.Set(weapon, new FirearmState(FirearmState.CurrentSchemaVersion, 1,
                    FirearmStateTokenCatalog.DiagnosticLeadBall, FirearmCondition.Normal));
            int wounds = target.Damage;
            FirearmLogCapture log = firearm ? new FirearmLogCapture() : null;
            long logAttempts = NativeCombatLog.Attempts, logFaults = NativeCombatLog.Faults;
            try
            {
                RuleAttackRoll roll = Rulebook.Trigger(new RuleAttackWithWeapon(attacker, target, weapon, 0)).AttackRoll;
                if (roll == null || roll.ACRule == null) throw new InvalidOperationException("Native attack did not calculate AC: weapon=" +
                    weapon.Blueprint.AssetGuid + ";enemy=" + target.IsEnemy(attacker) + ";roll=" +
                    (roll == null ? "absent" : roll.Result + ";autoHit=" + roll.AutoHit + ";autoMiss=" + roll.AutoMiss +
                        ";missChance=" + roll.MissChance + ";missChanceRoll=" + roll.MissChanceRoll));
                if (firearm)
                {
                    FirearmItemStateSnapshot state;
                    string rejection;
                    bool persisted = FirearmRuntimeState.Service.TryGetOrCreate(weapon, out state, out rejection);
                    long attempts = NativeCombatLog.Attempts - logAttempts;
                    if (!persisted || state.Repository.State.LoadedRounds != 0 || log.Messages != attempts ||
                        NativeCombatLog.Faults != logFaults)
                        throw new InvalidOperationException("Native firearm control: state=" + persisted + ";rounds=" +
                            (persisted ? state.Repository.State.LoadedRounds.ToString() : rejection) +
                            ";annotations=" + log.Messages + ";nativeAttempts=" + attempts +
                            ";newFaults=" + (NativeCombatLog.Faults - logFaults));
                }
                return roll;
            }
            finally
            {
                try { target.Damage = wounds; ClearProjectiles(); }
                finally { if (log != null) log.Dispose(); }
            }
        }

        private static void Armor(UnitEntityData attacker, UnitEntityData target, ItemEntityWeapon weapon, int baseline, int expected,
            string name, ICollection<RuntimeTestAssertion> assertions, JArray rows)
        {
            RuleAttackRoll roll = Attack(attacker, target, weapon);
            int enhancement = roll.WeaponStats.DamageDescription[0].TypeDescription.Physical.EnhancementTotal;
            Check(assertions, rows, name, roll.TargetAC == baseline + expected,
                "weapon=" + weapon.Blueprint.AssetGuid + ";attack=" + roll.AttackType + ";nativeEnhancementTotal=" + enhancement +
                ";nativeAC=" + roll.TargetAC + ";base=" + baseline + ";expectedBonus=" + expected);
        }

        private static void ClearProjectiles()
        {
            foreach (Projectile projectile in Game.Instance.ProjectileController.Projectiles.ToArray()) projectile.Cleared = true;
            Game.Instance.ProjectileController.Tick();
            Game.Instance.ProjectileController.Tick();
        }
        private static T Exact<T>(string guid) where T : BlueprintScriptableObject
        { return BlueprintLibraryLookup.RequireExact<T>(BlueprintBootstrap.Library, guid, "Breeze native regression fixture"); }
        private static void Check(ICollection<RuntimeTestAssertion> assertions, JArray rows, string name, bool pass, string observed)
        {
            rows.Add(new JObject { { "name", name }, { "pass", pass }, { "observed", observed } });
            assertions.Add(new RuntimeTestAssertion { Name = "elemental-breeze-" + name, Expected = "exact native Breeze-Kissed behavior",
                Observed = observed, Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "native disposable commands, cooldown, combat maneuvers and actual attack/AC rules; no save access" });
        }
    }
}
