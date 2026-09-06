using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Real native preview/cancel/commit around command-created feat
    /// effects. Observes the native disabled preview, never enables its buffs
    /// or writes a serialized transient ledger.</summary>
    internal static class ElementalFeatTransientPreviewScenario
    {
        private sealed class LogObservation
        {
            internal readonly List<string> Errors = new List<string>();
            internal int Witnesses;
        }
        private static LogObservation _activeLog;

        // Observe only the project's exact log output boundary. UMM Log does
        // not traverse Unity's log callback in this installed environment.
        // Never suppress/replace the log or affect non-test requests.
        [HarmonyPatch(typeof(ModLogger), "Write",
            new[] { typeof(string), typeof(string), typeof(string), typeof(string) })]
        private static class PreviewLogObservationPatch
        {
            private static void Prefix(string __0, string __1, string __2, string __3)
            {
                LogObservation active = _activeLog;
                if (active == null) return;
                lock (active)
                {
                    if (__0 == "INFO" && __1 == "runtime-tests" && __2 == "feat-preview-observer-witness")
                        active.Witnesses++;
                    if (__0 == "ERROR" && __1 == "elemental-races" && __2 == "feat-transient.reconcile.failed")
                        active.Errors.Add(__3);
                }
            }
        }
        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            var rows = new JArray();
            var diagnostics = new List<string>();
            if (_activeLog != null) throw new InvalidOperationException("Transient preview log observation already active.");
            var observedLog = new LogObservation();
            UnitEntityData[] before = Game.Instance.State.Units.All.ToArray();
            UnityEngine.Random.State random = UnityEngine.Random.state;
            _activeLog = observedLog;
            try
            {
                ModContext context;
                if (!ModContext.TryGet(out context)) throw new InvalidOperationException("Runtime mod context absent.");
                context.Logger.Info("runtime-tests", "feat-preview-observer-witness",
                    "request-local read-only logger boundary active");
                foreach (bool scorching in new[] { true, false })
                {
                    ElementalRaceBlueprints race = scorching ? BlueprintBootstrap.ElementalRaces.Ifrit :
                        BlueprintBootstrap.ElementalRaces.Undine;
                    var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
                    TimeSpan clock = Game.Instance.TimeController.GameTime;
                    try { Run(fixture.Caster, scorching, rows, assertions); }
                    finally
                    {
                        Game.Instance.Player.GameTime = clock;
                        fixture.Dispose();
                        Check(assertions, rows, (scorching ? "scorching" : "strike") + "-native-lifetime",
                            fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                            fixture.NativeObservationReleased && fixture.NativeTeardownObserved &&
                            fixture.AreaContextRestored && fixture.PlayerContextRestored,
                            "errors=" + fixture.NativeErrors + ";exceptions=" + fixture.NativeExceptions);
                    }
                }
            }
            finally
            {
                _activeLog = null;
                UnityEngine.Random.state = random;
                bool clean = before.Length == Game.Instance.State.Units.All.Count &&
                    before.All(value => Game.Instance.State.Units.All.Contains(value));
                Check(assertions, rows, "fixture-cleanup", clean, "exact original unit membership restored");
                string[] errors;
                int witnesses;
                lock (observedLog) { errors = observedLog.Errors.ToArray(); witnesses = observedLog.Witnesses; }
                Check(assertions, rows, "log-boundary-witness", witnesses == 1 && _activeLog == null,
                    "actual project logger witness=" + witnesses + ";request-local observation released");
                Check(assertions, rows, "no-transient-restoration-errors", errors.Length == 0,
                    "actual native preview/cancel/commit; errors=" + errors.Length);
                string path = Path.Combine(request.EvidenceDirectory, "elemental-feat-transient-preview.json");
                File.WriteAllText(path, new JObject {
                    { "schemaVersion", 1 }, { "saveStateTouched", false }, { "cleanupExact", clean },
                    { "logObserverReleased", _activeLog == null }, { "logBoundaryWitnesses", witnesses },
                    { "transientErrors", new JArray(errors) },
                    { "diagnostics", new JArray(diagnostics) }, { "observations", rows }
                }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }

        private static void Run(UnitEntityData unit, bool scorching, JArray rows,
            ICollection<RuntimeTestAssertion> assertions)
        {
            UnitDescriptor owner = unit.Descriptor;
            string prefix = scorching ? "scorching-" : "strike-";
            BlueprintCharacterClass fighter = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                BlueprintBootstrap.Library, "48ac8db94d5de7645906c7d0ad3bcfbd", "native preview fighter");
            ElementalSpellAffinityScenario.Advance(owner, fighter, 2);
            int level = owner.Progression.CharacterLevel;
            var set = BlueprintBootstrap.ElementalFeats;
            owner.AddFact(set.RequireFeature(scorching ? ElementalFeatId.ScorchingWeapons : ElementalFeatId.ElementalStrike));
            BlueprintAbility ability = set.RequireSymbol<BlueprintAbility>(scorching ?
                ElementalRaceIdentityCatalog.ScorchingWeaponsAbility : ElementalRaceIdentityCatalog.ElementalStrikeAbility);
            BlueprintBuff buffBlueprint = set.RequireSymbol<BlueprintBuff>(scorching ?
                ElementalRaceIdentityCatalog.ScorchingWeaponsBuff : ElementalRaceIdentityCatalog.ElementalStrikeBuff);
            BlueprintWeaponEnchantment enchantment = set.RequireSymbol<BlueprintWeaponEnchantment>(
                ElementalRaceIdentityCatalog.ScorchingWeaponsEnchantment);
            var weapons = new List<ItemEntityWeapon>();
            try
            {
                if (scorching)
                {
                    if (owner.Body.PrimaryHand.HasItem || owner.Body.SecondaryHand.HasItem)
                        throw new InvalidOperationException("Transient preview fixture requires empty native hands.");
                    BlueprintItemWeapon sword = BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(
                        BlueprintBootstrap.Library, "57c8994d1f1becf49ac4f642e5d8ca9d", "native preview short sword");
                    weapons.Add(new ItemEntityWeapon(sword));
                    weapons.Add(new ItemEntityWeapon(sword));
                    owner.Body.PrimaryHand.InsertItem(weapons[0]);
                    owner.Body.SecondaryHand.InsertItem(weapons[1]);
                }
                Ability fact = owner.Abilities.GetAbility(ability);
                if (fact == null) throw new InvalidOperationException("Transient activation ability absent.");
                var data = new AbilityData(fact);
                if (!data.IsAvailable) throw new InvalidOperationException("Transient activation unavailable.");
                var command = ElementalUndineFeatScenario.CreateCommand(data, new TargetWrapper(unit), unit);
                object result = ElementalUndineFeatScenario.InvokeCommandAction(command);
                for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 100; tick++)
                    command.ExecutionProcess.Tick();
                bool ended = command.ExecutionProcess != null && command.ExecutionProcess.IsEnded;
                if (!ended && command.ExecutionProcess != null) command.ExecutionProcess.Detach();
                ElementalUndineFeatScenario.InvokeCommandEnded(command, !ended);
                Buff buff = owner.Buffs.GetBuff(buffBlueprint);
                UnitPartElementalFeatTransientState ledger = owner.Get<UnitPartElementalFeatTransientState>();
                if (!ended || buff == null || ledger == null)
                    throw new InvalidOperationException("Native command did not create the transient effect: " + result);
                long end = End(ledger, scorching);
                ItemEnchantment[] effects = weapons.SelectMany(value => value.Enchantments.Where(
                    effect => ReferenceEquals(effect.Blueprint, enchantment))).ToArray();
                Check(assertions, rows, prefix + "native-command-created",
                    buff.Active && end == buff.EndTime.Ticks && end > Game.Instance.TimeController.GameTime.Ticks &&
                    effects.Length == (scorching ? 2 : 0), "command=" + result + ";exactItems=" + effects.Length);

                foreach (bool commit in new[] { false, true })
                {
                    LevelUpController controller = null;
                    try
                    {
                        controller = LevelUpController.StartWithoutAssigningStaticInstance(
                            owner, false, null, null, LevelUpState.CharBuildMode.LevelUp);
                        UnitDescriptor preview = controller.Preview;
                        FieldInfo disabled = typeof(BuffCollection).GetField("m_Disabled",
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        if (disabled == null || disabled.FieldType != typeof(bool))
                            throw new MissingFieldException(typeof(BuffCollection).FullName, "m_Disabled");
                        Check(assertions, rows, prefix + (commit ? "commit" : "cancel") + "-native-preview",
                            preview != null && !ReferenceEquals(preview, owner) &&
                            (bool)disabled.GetValue(preview.Buffs) && !(bool)disabled.GetValue(owner.Buffs),
                            "actual native preview disables only its own BuffCollection");
                        UnitPartElementalFeatTransientState copy = preview.Get<UnitPartElementalFeatTransientState>();
                        long copyBefore = End(copy, scorching);
                        // Retain integration coverage through the actual project
                        // service in addition to native constructor/fact callbacks.
                        bool first = ElementalFeatTransientRuntime.ReconcileAfterUnitLoad(preview);
                        bool second = ElementalFeatTransientRuntime.ReconcileAfterUnitLoad(preview);
                        Check(assertions, rows, prefix + (commit ? "commit" : "cancel") + "-preview-no-op",
                            first && second && copy != null && !ReferenceEquals(copy, ledger) &&
                            copyBefore == end && End(copy, scorching) == copyBefore,
                            "reconcile=" + first + "/" + second + ";savedEnd=" + end + ";previewEnd=" + End(copy, scorching));
                        if (commit)
                        {
                            if (!controller.SelectClass(fighter, false))
                                throw new InvalidOperationException("Native preview class selection failed.");
                            controller.ApplyClassMechanics();
                            controller.Commit();
                        }
                        else controller.Cancel();
                        controller = null;
                    }
                    finally { if (controller != null) controller.Cancel(); }

                    bool exactItems = effects.Length == weapons.Count && effects.All(effect =>
                        effect.Owner != null && weapons.Any(weapon => ReferenceEquals(effect.Owner, weapon)) &&
                        effect.Owner.Enchantments.Contains(effect) && effect.EndTime.Ticks == end);
                    Check(assertions, rows, prefix + (commit ? "committed" : "canceled") + "-original-unchanged",
                        owner.Progression.CharacterLevel == level + (commit ? 1 : 0) &&
                        ReferenceEquals(owner.Get<UnitPartElementalFeatTransientState>(), ledger) &&
                        ReferenceEquals(owner.Buffs.GetBuff(buffBlueprint), buff) && buff.Active &&
                        buff.EndTime.Ticks == end && End(ledger, scorching) == end && exactItems &&
                        ElementalFeatTransientRuntime.ReconcileAfterUnitLoad(owner) &&
                        (scorching ? weapons.All(weapon => ElementalFeatTransientRuntime.IsScorchingWeaponsActive(owner, weapon)) :
                            ElementalFeatTransientRuntime.IsElementalStrikeActive(owner)),
                        "level=" + owner.Progression.CharacterLevel + ";sameBuff=" +
                        ReferenceEquals(owner.Buffs.GetBuff(buffBlueprint), buff) + ";end=" + End(ledger, scorching) +
                        ";sameEnchantmentItems=" + exactItems);
                }
                Game.Instance.Player.GameTime = TimeSpan.FromTicks(end) + TimeSpan.FromSeconds(0.1);
                owner.Buffs.Tick();
                // Native buff and item schedulers are separate. Invoke only
                // TickItem for these disposable items, not the global inventory
                // controller and not manual enchantment removal.
                MethodInfo tickItem = typeof(Kingmaker.Controllers.ItemsEnchantmentController).GetMethod(
                    "TickItem", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new[] { typeof(ItemEntity) }, null);
                if (tickItem == null) throw new MissingMethodException("ItemsEnchantmentController.TickItem");
                foreach (ItemEntityWeapon weapon in weapons) tickItem.Invoke(null, new object[] { weapon });
                Check(assertions, rows, prefix + "native-expiry",
                    !owner.HasFact(buffBlueprint) && End(ledger, scorching) == 0 &&
                    weapons.All(weapon => !weapon.Enchantments.Any(effect => ReferenceEquals(effect.Blueprint, enchantment))),
                    "native buff/item schedulers;buffPresent=" + owner.HasFact(buffBlueprint) +
                    ";remainingEnd=" + End(ledger, scorching) + ";exactEnchantments=" +
                    weapons.Sum(weapon => weapon.Enchantments.Count(effect => ReferenceEquals(effect.Blueprint, enchantment))));
            }
            finally
            {
                ElementalFeatTransientRuntime.RemoveScorchingWeapons(owner);
                ElementalFeatTransientRuntime.RemoveElementalStrike(owner);
                foreach (ItemEntityWeapon weapon in weapons)
                {
                    if (ReferenceEquals(owner.Body.PrimaryHand.MaybeItem, weapon)) owner.Body.PrimaryHand.RemoveItem(false);
                    if (ReferenceEquals(owner.Body.SecondaryHand.MaybeItem, weapon)) owner.Body.SecondaryHand.RemoveItem(false);
                    weapon.Dispose();
                }
            }
        }

        private static long End(UnitPartElementalFeatTransientState state, bool scorching)
        {
            return state == null ? 0L : scorching ? state.ScorchingWeaponsEndTimeTicks : state.ElementalStrikeEndTimeTicks;
        }

        private static void Check(ICollection<RuntimeTestAssertion> assertions, JArray rows,
            string name, bool pass, string observed)
        {
            rows.Add(new JObject { { "name", name }, { "pass", pass }, { "observed", observed } });
            assertions.Add(new RuntimeTestAssertion { Name = "elemental-feat-preview-" + name,
                Expected = "exact native preview/active-owner transient boundary", Observed = observed,
                Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "actual native commands, LevelUpController preview/cancel/commit, item facts and native expiry; no save access" });
        }
    }
}
