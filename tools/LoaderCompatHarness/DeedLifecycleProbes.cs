using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Newtonsoft.Json.Linq;
using TurnBased.Controllers;
using UnityEngine;

namespace KmgLoaderCompatHarness
{
    /// <summary>
    /// What a buff blueprint finally holds once every builder and later publication step has run: its two effect links,
    /// its visibility, stacking, components and icon. A link is "absent" (null), "empty" (an object that loads nothing)
    /// or an asset id.
    /// </summary>
    internal static class BuffCensus
    {
        public static JObject Describe(BlueprintBuff blueprint)
        {
            return new JObject
            {
                ["guid"] = blueprint.AssetGuid, ["blueprint"] = blueprint.name, ["name"] = blueprint.Name,
                ["fxOnStart"] = Link(blueprint.FxOnStart), ["fxOnRemove"] = Link(blueprint.FxOnRemove),
                ["resourceAssetIds"] = blueprint.ResourceAssetIds == null ? null : (JToken)blueprint.ResourceAssetIds.Length,
                ["hiddenInUi"] = blueprint.IsHiddenInUI, ["stacking"] = blueprint.Stacking.ToString(), ["isClassFeature"] = blueprint.IsClassFeature,
                ["icon"] = blueprint.Icon == null ? null : blueprint.Icon.name,
                ["components"] = new JArray(blueprint.ComponentsArray.Select(component => (object)(component == null ? "<null>" : component.GetType().Name)).ToArray())
            };
        }

        public static string Link(Kingmaker.ResourceLinks.PrefabLink link)
        {
            if (link == null) return "absent";
            return string.IsNullOrEmpty(link.AssetId) ? "empty" : link.AssetId;
        }

        /// <summary>
        /// "required" must resolve; "optional" may be absent from this stack. With "linked", a resolved buff whose link
        /// is absent is a failure. Returns the failures.
        /// </summary>
        public static List<string> Run(IEnumerable<string> required, IEnumerable<string> optional, bool linked, JObject result)
        {
            var failures = new List<string>();
            var rows = new JArray();
            foreach (var entry in required.Select(guid => new { guid, must = true }).Concat(optional.Select(guid => new { guid, must = false })))
            {
                BlueprintBuff blueprint = ResourcesLibrary.TryGetBlueprint(entry.guid) as BlueprintBuff;
                if (blueprint == null)
                {
                    rows.Add(new JObject { ["guid"] = entry.guid, ["resolved"] = false });
                    if (entry.must) failures.Add("buff " + entry.guid + " does not resolve");
                    continue;
                }

                JObject row = Describe(blueprint);
                row["resolved"] = true;
                rows.Add(row);
                if (linked && (blueprint.FxOnStart == null || blueprint.FxOnRemove == null))
                    failures.Add(blueprint.name + " has an absent effect link (start=" + Link(blueprint.FxOnStart) + ", remove=" + Link(blueprint.FxOnRemove) + ")");
            }

            result["buffs"] = rows;
            return failures;
        }
    }

    /// <summary>
    /// Drives a deed or named-weapon effect through its real trigger in a loaded save with time running, and records the
    /// buff's whole life: the game's own "buff added" notice, the mechanical effect, consumption or cure, the end that
    /// comes by itself, and the stats afterwards. The actors are disposable units spawned beside the party and levelled
    /// through Kingmaker's level-up controller. Abilities are used with the game's own ability command; weapon attacks
    /// are the game's own RuleAttackWithWeapon with the equipped weapon. Nothing is saved.
    /// </summary>
    internal sealed class DeedLifecycleProbe : ScreenProbe, IOwnedProbe
    {
        private const string Bootstrap = "KingmakerGunslinger.Bootstrap.BlueprintBootstrap";
        private const string DodgeDiagnostics = "KingmakerGunslinger.Deeds.GunslingerDodgeRuntimeDiagnostics";
        private const string DodgeGuard = "KingmakerGunslinger.Deeds.GunslingerDodgeExpirationPatch";
        private const string DodgeBonus = "KingmakerGunslinger.Deeds.GunslingerDodgeArmorClassBonus";
        private const string EasternDiagnostics = "KingmakerGunslinger.EasternWeapons.EasternNamedWeaponEffectDiagnostics";

        private sealed class Observer : IUnitBuffHandler, IGlobalRulebookHandler<RuleAttackRoll>, IGlobalRulebookHandler<RuleDealDamage>,
            IGlobalRulebookHandler<RuleDealStatDamage>
        {
            public readonly HashSet<string> Buffs = new HashSet<string>();
            public readonly HashSet<UnitEntityData> Units = new HashSet<UnitEntityData>();
            public readonly JArray BuffEvents = new JArray();
            public readonly JArray AttackRolls = new JArray();
            public readonly JArray Damage = new JArray();

            private static double Now() => Math.Round(Game.Instance.TimeController.GameTime.TotalSeconds, 2);

            private void Buff(string kind, Buff buff)
            {
                if (buff == null || buff.Blueprint == null || !Buffs.Contains(buff.Blueprint.AssetGuid)) return;
                BuffEvents.Add(new JObject
                {
                    ["event"] = kind, ["blueprint"] = buff.Blueprint.name, ["guid"] = buff.Blueprint.AssetGuid,
                    ["unit"] = buff.Owner == null || buff.Owner.Unit == null ? null : buff.Owner.Unit.Blueprint.name, ["gameSeconds"] = Now()
                });
            }

            public void HandleBuffDidAdded(Buff buff) => Buff("added", buff);
            public void HandleBuffDidRemoved(Buff buff) => Buff("removed", buff);
            public void OnEventAboutToTrigger(RuleAttackRoll evt) { }

            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                if (evt.Target == null || !Units.Contains(evt.Target)) return;
                AttackRolls.Add(new JObject
                {
                    ["initiator"] = evt.Initiator == null ? null : evt.Initiator.Blueprint.name, ["target"] = evt.Target.Blueprint.name,
                    ["targetArmorClass"] = evt.TargetAC, ["targetFlatFooted"] = evt.IsTargetFlatFooted, ["hit"] = evt.IsHit, ["gameSeconds"] = Now()
                });
            }

            public void OnEventAboutToTrigger(RuleDealDamage evt) { }

            public void OnEventDidTrigger(RuleDealDamage evt)
            {
                if (evt.Target == null || !Units.Contains(evt.Target)) return;
                Damage.Add(new JObject
                {
                    ["kind"] = "hitPoints", ["source"] = evt.Initiator == null ? null : evt.Initiator.Blueprint.name, ["target"] = evt.Target.Blueprint.name,
                    ["amount"] = evt.Damage, ["gameSeconds"] = Now()
                });
            }

            public void OnEventAboutToTrigger(RuleDealStatDamage evt) { }

            public void OnEventDidTrigger(RuleDealStatDamage evt)
            {
                if (evt.Target == null || !Units.Contains(evt.Target)) return;
                Damage.Add(new JObject
                {
                    ["kind"] = evt.Stat == null ? "stat" : evt.Stat.Type.ToString(), ["source"] = evt.Initiator == null ? null : evt.Initiator.Blueprint.name,
                    ["target"] = evt.Target.Blueprint.name, ["gameSeconds"] = Now()
                });
            }
        }

        private readonly string script;
        private readonly Dictionary<string, string> ids;
        private readonly List<string> findings = new List<string>();
        private readonly List<UnitEntityData> spawned = new List<UnitEntityData>();
        private readonly List<BlueprintUnit> clones = new List<BlueprintUnit>();
        private readonly Observer observer = new Observer();
        private IEnumerator<object> routine;
        private Func<bool> pending;
        private string pendingName;
        private double pendingDeadline;
        private bool subscribed;
        private int pausesLifted;
        private int unitIndex;

        public DeedLifecycleProbe(string directory, string label, string script, Dictionary<string, string> ids) : base(directory, label)
        {
            this.script = script;
            this.ids = ids ?? new Dictionary<string, string>();
        }

        protected override string Step()
        {
            if (pending != null)
            {
                if (Game.Instance.IsPaused)
                {
                    Game.Instance.IsPaused = false;
                    pausesLifted++;
                }

                bool done;
                try { done = pending(); }
                catch (Exception exception) { return Verdict(exception.ToString()); }
                if (!done && Time.realtimeSinceStartup < pendingDeadline) return null;
                if (!done) findings.Add("timed out waiting for " + pendingName);
                pending = null;
            }

            if (routine == null)
            {
                if (Game.Instance.Player.MainCharacter.Value == null) return "no main character; this probe needs a loaded save";
                Record["script"] = script;
                Record["gameMode"] = Game.Instance.CurrentMode.ToString();
                EventBus.Subscribe(observer);
                subscribed = true;
                switch (script)
                {
                    case "dodge": routine = Dodge(); break;
                    case "armed": routine = Armed(); break;
                    case "bleeds": routine = Bleeds(); break;
                    case "condition": routine = Condition(); break;
                    case "fallingPetal": routine = FallingPetal(); break;
                    case "mountainSunder": routine = MountainSunder(); break;
                    default: return Verdict("unknown deed lifecycle script " + script);
                }
            }

            bool more;
            try { more = routine.MoveNext(); }
            catch (Exception exception) { return Verdict(exception.ToString()); }
            return more ? null : Verdict(null);
        }

        object IOwnedProbe.Result
        {
            get { return Record; }
        }

        /// <summary>The probe's own verdict: every finding so far, then the fault that stopped it, if one did. Releases nothing.</summary>
        private string Verdict(string fault)
        {
            var parts = new List<string>(findings);
            if (!string.IsNullOrEmpty(fault)) parts.Add(fault);
            return string.Join("; ", parts.ToArray());
        }

        /// <summary>
        /// Gives back everything this probe put into the game: its event subscription, a queued forced roll, the pause
        /// it lifted, each spawned unit and each blueprint clone. Each is attempted on its own and each failure is
        /// reported; the slot calls this once, whether the probe finished, timed out or threw.
        /// </summary>
        public CleanupReport Release(string testFailure)
        {
            // What was observed is recorded first, so that no release below can lose it.
            Record["buffEvents"] = observer.BuffEvents;
            Record["attackRollsOnWatchedUnits"] = observer.AttackRolls;
            Record["damageOnWatchedUnits"] = observer.Damage;
            Record["pausesLifted"] = pausesLifted;
            Record["findings"] = new JArray(findings.Cast<object>().ToArray());
            Record["endedBecause"] = testFailure;
            try { Record["turnBasedCombat"] = CombatController.IsInTurnBasedCombat(); }
            catch (Exception exception) { Record["turnBasedCombat"] = "not readable: " + exception.Message; }

            var releases = new List<KeyValuePair<string, Action>>
            {
                new KeyValuePair<string, Action>("event subscription", () =>
                {
                    if (!subscribed) return;
                    EventBus.Unsubscribe(observer);
                    subscribed = false;
                }),
                new KeyValuePair<string, Action>("forced natural roll", () =>
                {
                    Type misfire = Product("KingmakerGunslinger.Misfires.FirearmMisfireRuntime");
                    misfire.GetMethod("CancelForcedNaturalRoll", Any).Invoke(null, null);
                    object stillPending = ProductStatic("KingmakerGunslinger.Misfires.FirearmMisfireRuntime", "PendingForcedNaturalRoll");
                    if (stillPending != null) throw new InvalidOperationException("a forced natural roll of " + stillPending + " is still queued");
                }),
                new KeyValuePair<string, Action>("pause", () => Game.Instance.IsPaused = true)
            };
            for (int index = 0; index < spawned.Count; index++)
            {
                UnitEntityData unit = spawned[index];
                releases.Add(new KeyValuePair<string, Action>("unit " + (index + 1) + " of " + spawned.Count, () =>
                {
                    observer.Units.Remove(unit);
                    unit.Commands.InterruptAll(true);
                    unit.Dispose();
                }));
            }

            for (int index = 0; index < clones.Count; index++)
            {
                BlueprintUnit clone = clones[index];
                releases.Add(new KeyValuePair<string, Action>("blueprint clone " + (index + 1) + " of " + clones.Count, () =>
                {
                    UnityEngine.Object.DestroyImmediate(clone);
                    if (clone != null) throw new InvalidOperationException("the clone still exists after it was destroyed");
                }));
            }

            CleanupReport report = ProbeCleanup.RunAll(releases);
            spawned.Clear();
            clones.Clear();
            pending = null;
            return report;
        }

        // ---- waiting ----

        private static double Now() => Game.Instance.TimeController.GameTime.TotalSeconds;

        private object Until(string name, Func<bool> condition, double realSeconds)
        {
            pending = condition;
            pendingName = name;
            pendingDeadline = Time.realtimeSinceStartup + realSeconds;
            return null;
        }

        private object GameSeconds(double seconds)
        {
            double end = Now() + seconds;
            return Until(seconds.ToString(CultureInfo.InvariantCulture) + " s of game time", () => Now() >= end, seconds * 4 + 30);
        }

        private object Finished(UnitUseAbility command)
        {
            return Until("the ability command and its delivery", () => command.IsFinished && (command.ExecutionProcess == null || command.ExecutionProcess.IsEnded), 30);
        }

        // ---- identities ----

        private string Id(string key)
        {
            string value;
            if (!ids.TryGetValue(key, out value) || string.IsNullOrEmpty(value)) throw new InvalidOperationException("the step names no '" + key + "'");
            return value;
        }

        private T Blueprint<T>(string key) where T : BlueprintScriptableObject
        {
            T blueprint = ResourcesLibrary.TryGetBlueprint(Id(key)) as T;
            if (blueprint == null) throw new InvalidOperationException(key + " " + Id(key) + " does not resolve to a " + typeof(T).Name);
            return blueprint;
        }

        private T[] Blueprints<T>(string key) where T : BlueprintScriptableObject
        {
            return Id(key).Split(',').Select(guid =>
            {
                T blueprint = ResourcesLibrary.TryGetBlueprint(guid.Trim()) as T;
                if (blueprint == null) throw new InvalidOperationException(key + " " + guid + " does not resolve to a " + typeof(T).Name);
                return blueprint;
            }).ToArray();
        }

        private static Type Product(string fullName)
        {
            return ProductProbes.Entry().Assembly.GetType(fullName, true);
        }

        private static object ProductStatic(string type, string member)
        {
            Type owner = Product(type);
            FieldInfo field = owner.GetField(member, Any);
            if (field != null) return field.GetValue(null);
            PropertyInfo property = owner.GetProperty(member, Any);
            if (property != null) return property.GetValue(null, null);
            throw new MissingMemberException(type, member);
        }

        private static object Member(object instance, string member)
        {
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                PropertyInfo property = type.GetProperty(member, Any | BindingFlags.DeclaredOnly);
                if (property != null) return property.GetValue(instance, null);
                FieldInfo field = type.GetField(member, Any | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(instance);
            }

            throw new MissingMemberException(instance.GetType().FullName, member);
        }

        private static long Counter(string type, string member) => Convert.ToInt64(ProductStatic(type, member), CultureInfo.InvariantCulture);

        // ---- units ----

        private Vector3 Near(float x, float z)
        {
            return Game.Instance.Player.MainCharacter.Value.Position + new Vector3(x, 0f, z);
        }

        private UnitEntityData SpawnFriendly(string label, Vector3 position)
        {
            BlueprintUnit clone = UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
            clone.name = "KmgLoaderCompat_" + label + "_" + (++unitIndex);
            clone.IsCheater = false;
            clones.Add(clone);
            UnitEntityData unit = Game.Instance.EntityCreator.SpawnUnit(clone, position, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
            if (unit == null || unit.View == null) throw new InvalidOperationException("the game did not spawn a live " + label);
            spawned.Add(unit);
            unit.Descriptor.State.Immortality.Retain();
            if (!unit.Descriptor.IsTurnedOn) unit.Descriptor.TurnOn();
            observer.Units.Add(unit);
            return unit;
        }

        /// <summary>A copy of the default player character under the first faction the game treats as the unit's enemy.</summary>
        private UnitEntityData SpawnHostile(UnitEntityData enemyOf, Vector3 position)
        {
            BlueprintUnit source = BlueprintRoot.Instance.DefaultPlayerCharacter;
            var seen = new HashSet<BlueprintFaction>();
            foreach (BlueprintUnit candidate in ResourcesLibrary.GetBlueprints<BlueprintUnit>().OrderBy(value => value.AssetGuid, StringComparer.Ordinal))
            {
                BlueprintFaction faction = candidate.Faction;
                if (faction == null || ReferenceEquals(faction, source.Faction) || !seen.Add(faction)) continue;
                BlueprintUnit clone = UnityEngine.Object.Instantiate(source);
                clone.name = "KmgLoaderCompat_Hostile_" + (++unitIndex);
                clone.IsCheater = true;
                clone.Faction = faction;
                UnitEntityData unit = null;
                try
                {
                    unit = Game.Instance.EntityCreator.SpawnUnit(clone, position, Quaternion.identity, Game.Instance.State.LoadedAreaState.MainState);
                    if (unit != null && unit.IsEnemy(enemyOf))
                    {
                        clones.Add(clone);
                        spawned.Add(unit);
                        unit.Descriptor.State.Immortality.Retain();
                        observer.Units.Add(unit);
                        return unit;
                    }
                }
                catch (Exception)
                {
                    // The next faction is tried.
                }

                if (unit != null) unit.Dispose();
                UnityEngine.Object.DestroyImmediate(clone);
            }

            throw new InvalidOperationException("no loaded faction produced a hostile disposable target");
        }

        private static void Level(UnitEntityData unit, BlueprintCharacterClass characterClass, BlueprintArchetype archetype, int levels)
        {
            MethodInfo apply = typeof(LevelUpController).GetMethod("ApplyLevelup", Any);
            if (apply == null) throw new MissingMethodException("LevelUpController.ApplyLevelup");
            for (int level = 1; level <= levels; level++)
            {
                LevelUpController controller = LevelUpController.StartWithoutAssigningStaticInstance(unit.Descriptor, false, null, null,
                    level == 1 ? LevelUpState.CharBuildMode.CharGen : LevelUpState.CharBuildMode.LevelUp);
                try
                {
                    if (!controller.SelectClass(characterClass, false)) throw new InvalidOperationException("the level-up controller refused " + characterClass.name + " at level " + level);
                    if (archetype != null && level == 1 && !controller.AddArchetype(archetype))
                        throw new InvalidOperationException("the level-up controller refused the archetype " + archetype.name);
                    controller.ApplyClassMechanics();
                    apply.Invoke(controller, new object[] { unit.Descriptor });
                }
                finally
                {
                    controller.Cancel();
                }

                int reached = unit.Descriptor.Progression.GetClassLevel(characterClass);
                if (reached != level) throw new InvalidOperationException("levelling reached " + reached + " after requested level " + level);
            }
        }

        private UnitEntityData Gunslinger(string label, Vector3 position, int levels, int wisdom, BlueprintArchetype archetype = null)
        {
            UnitEntityData unit = SpawnFriendly(label, position);
            unit.Descriptor.Stats.Wisdom.BaseValue = wisdom;
            unit.Descriptor.Stats.Dexterity.BaseValue = 18;
            Level(unit, Blueprint<BlueprintCharacterClass>("class"), archetype, levels);
            BlueprintAbilityResource grit = Blueprint<BlueprintAbilityResource>("grit");
            unit.Descriptor.Resources.Restore(grit, 100);
            return unit;
        }

        private int Grit(UnitEntityData unit) => unit.Descriptor.Resources.GetResourceAmount(Blueprint<BlueprintAbilityResource>("grit"));

        private static Buff OnUnit(UnitEntityData unit, BlueprintBuff blueprint)
        {
            return unit.Descriptor.Buffs.RawFacts.OfType<Buff>().FirstOrDefault(candidate => ReferenceEquals(candidate.Blueprint, blueprint));
        }

        private static JToken TimeLeft(Buff buff)
        {
            if (buff == null) return null;
            return buff.IsPermanent ? (JToken)"permanent" : Math.Round(buff.TimeLeft.TotalSeconds, 2);
        }

        private int Events(string kind, BlueprintBuff blueprint, UnitEntityData unit, double since)
        {
            return observer.BuffEvents.Count(token => (string)token["event"] == kind && (string)token["guid"] == blueprint.AssetGuid &&
                (string)token["unit"] == unit.Blueprint.name && (double)token["gameSeconds"] >= Math.Round(since, 2) - 0.011);
        }

        /// <summary>Every notice of this kind the game has raised for this buff on this unit so far.</summary>
        private int Notices(string kind, BlueprintBuff blueprint, UnitEntityData unit)
        {
            return observer.BuffEvents.Count(token => (string)token["event"] == kind && (string)token["guid"] == blueprint.AssetGuid &&
                (string)token["unit"] == unit.Blueprint.name);
        }

        private double? EventAt(string kind, BlueprintBuff blueprint, UnitEntityData unit, double since)
        {
            JToken last = observer.BuffEvents.LastOrDefault(token => (string)token["event"] == kind && (string)token["guid"] == blueprint.AssetGuid &&
                (string)token["unit"] == unit.Blueprint.name && (double)token["gameSeconds"] >= Math.Round(since, 2) - 0.011);
            return last == null ? (double?)null : (double)last["gameSeconds"];
        }

        /// <summary>
        /// Game seconds the buff was on the unit, from the game's own added and removed notices when both were raised,
        /// otherwise from when this probe saw it gone. Null while it is still on the unit.
        /// </summary>
        private double? Lasted(BlueprintBuff blueprint, UnitEntityData unit, double since)
        {
            if (OnUnit(unit, blueprint) != null) return null;
            double? added = EventAt("added", blueprint, unit, since), removed = EventAt("removed", blueprint, unit, since);
            return Math.Round(added.HasValue && removed.HasValue && removed.Value >= added.Value ? removed.Value - added.Value : Now() - since, 2);
        }

        private static bool OneRound(double? lasted) => lasted.HasValue && Close(lasted.Value, 6.0, 0.35);

        private static JToken Seconds(double? value) => value.HasValue ? (JToken)value.Value : null;

        private object Ended(BlueprintBuff blueprint, UnitEntityData unit)
        {
            return Until(blueprint.name + " to end", () => OnUnit(unit, blueprint) == null, 16);
        }

        private void Watch(params BlueprintBuff[] blueprints)
        {
            foreach (BlueprintBuff blueprint in blueprints) observer.Buffs.Add(blueprint.AssetGuid);
        }

        private void Fight(UnitEntityData unit, UnitEntityData hostile)
        {
            unit.CombatState.JoinCombat();
            hostile.CombatState.JoinCombat();
            unit.CombatState.Cooldown.Initiative = 0f;
            hostile.CombatState.Cooldown.Initiative = 0f;
            if (!unit.Memory.Contains(hostile)) unit.Memory.Add(hostile);
            if (!hostile.Memory.Contains(unit)) hostile.Memory.Add(unit);
        }

        // ---- abilities and attacks ----

        private UnitUseAbility Use(UnitEntityData unit, BlueprintAbility blueprint, UnitEntityData target, JObject record)
        {
            Ability fact = unit.Descriptor.Abilities.GetAbility(blueprint);
            if (fact == null) throw new InvalidOperationException(unit.Blueprint.name + " was not granted " + blueprint.name);
            var data = new AbilityData(fact);
            record["availableBeforeUse"] = data.IsAvailable;
            unit.Commands.InterruptAll(true);
            var command = new UnitUseAbility(data, new TargetWrapper(target));
            command.IgnoreCooldown(null);
            unit.Commands.Run(command);
            return command;
        }

        private static JObject Used(UnitUseAbility command)
        {
            return new JObject
            {
                ["commandResult"] = command.Result.ToString(), ["commandFinished"] = command.IsFinished,
                ["deliveryEnded"] = command.ExecutionProcess != null && command.ExecutionProcess.IsEnded
            };
        }

        private bool Available(UnitEntityData unit, BlueprintAbility blueprint)
        {
            Ability fact = unit.Descriptor.Abilities.GetAbility(blueprint);
            return fact != null && new AbilityData(fact).IsAvailable;
        }

        private ItemEntityWeapon Equip(UnitEntityData unit, BlueprintItemWeapon blueprint)
        {
            if (unit.Body.SecondaryHand != null && unit.Body.SecondaryHand.HasItem) unit.Body.SecondaryHand.RemoveItem();
            if (unit.Body.PrimaryHand.HasItem) unit.Body.PrimaryHand.RemoveItem();
            var weapon = new ItemEntityWeapon(blueprint);
            unit.Body.PrimaryHand.InsertItem(weapon);
            if (!ReferenceEquals(unit.Body.PrimaryHand.MaybeItem, weapon)) throw new InvalidOperationException(blueprint.name + " did not stay in the primary hand");
            return weapon;
        }

        private static BlueprintItemWeapon Firearm(string kind)
        {
            object catalog = ProductStatic(Bootstrap, "ProductionFirearms");
            return (BlueprintItemWeapon)Member(Member(catalog, kind), "Item");
        }

        /// <summary>One loaded lead ball in a sound firearm, set through the product's own item-state service.</summary>
        private static void LoadRound(ItemEntityWeapon weapon)
        {
            Type stateType = Product("KingmakerGunslinger.Firearms.FirearmState");
            object lead = ProductStatic("KingmakerGunslinger.Firearms.FirearmStateTokenCatalog", "DiagnosticLeadBall");
            object normal = Enum.Parse(Product("KingmakerGunslinger.Firearms.FirearmCondition"), "Normal");
            object schema = stateType.GetField("CurrentSchemaVersion", Any).GetValue(null);
            object state = stateType.GetConstructors(Any).Single(constructor => constructor.GetParameters().Length == 4).Invoke(new[] { schema, 1, lead, normal });
            object service = ProductStatic("KingmakerGunslinger.Firearms.FirearmRuntimeState", "Service");
            MethodInfo set = service.GetType().GetMethods(Any).Single(method => method.Name == "Set" && method.GetParameters().Length == 2);
            set.Invoke(service, new[] { (object)weapon, state });
        }

        private static int Seed(int natural)
        {
            for (int seed = 1; seed <= 100000; seed++)
            {
                UnityEngine.Random.InitState(seed);
                if (RulebookEvent.Dice.D20.Value == natural) return seed;
            }

            throw new InvalidOperationException("no native d20 seed produced " + natural);
        }

        /// <summary>
        /// The game's own weapon attack rule with the given equipped weapon. A firearm gets one loaded round first and its
        /// natural roll through the product's forced-roll queue; any other weapon rolls from a seeded native generator.
        /// </summary>
        private RuleAttackWithWeapon Attack(UnitEntityData attacker, UnitEntityData target, ItemEntityWeapon weapon, int natural, bool firearm, JArray log)
        {
            // The forced value replaces the reported natural roll, but a native 1 still misses; the native generator
            // is seeded as well so that both agree.
            Type misfire = Product("KingmakerGunslinger.Misfires.FirearmMisfireRuntime");
            int seed = Seed(natural);
            if (firearm)
            {
                LoadRound(weapon);
                misfire.GetMethod("QueueForcedNaturalRoll", Any).Invoke(null, new object[] { natural });
            }

            UnityEngine.Random.InitState(seed);
            RuleAttackWithWeapon attack;
            try { attack = Rulebook.Trigger(new RuleAttackWithWeapon(attacker, target, weapon, 0)); }
            finally { if (firearm) misfire.GetMethod("CancelForcedNaturalRoll", Any).Invoke(null, null); }
            if (attack.AttackRoll == null) throw new InvalidOperationException("the attack produced no attack roll");
            if (log != null)
                log.Add(new JObject
                {
                    ["weapon"] = weapon.Blueprint.name, ["requestedNatural"] = natural, ["hit"] = attack.AttackRoll.IsHit,
                    ["result"] = attack.AttackRoll.Result.ToString(), ["criticalConfirmed"] = attack.AttackRoll.IsCriticalConfirmed,
                    ["targetArmorClass"] = attack.AttackRoll.TargetAC, ["targetFlatFooted"] = attack.AttackRoll.IsTargetFlatFooted,
                    ["gameSeconds"] = Math.Round(Now(), 2)
                });
            return attack;
        }

        private void Expect(bool condition, string finding)
        {
            if (!condition) findings.Add(finding);
        }

        private static bool Close(double value, double expected, double tolerance) => Math.Abs(value - expected) <= tolerance;

        // ---- Gunslinger's Dodge ----

        private IEnumerator<object> Dodge()
        {
            BlueprintAbility ability = Blueprint<BlueprintAbility>("ability");
            BlueprintBuff blueprint = Blueprint<BlueprintBuff>("buff");
            Watch(blueprint);
            Record["blueprint"] = BuffCensus.Describe(blueprint);
            Record["abilityComponents"] = new JArray(ability.ComponentsArray.Select(component => (object)(component == null ? "<null>" : component.GetType().Name)).ToArray());
            Game.Instance.IsPaused = true;
            UnitEntityData actor = Gunslinger("DodgeGunslinger", Near(3f, 3f), 1, 20);
            UnitEntityData hostile = SpawnHostile(actor, actor.Position + new Vector3(1.5f, 0f, 0f));
            ItemEntityWeapon fist = hostile.Body.PrimaryHand.MaybeWeapon ?? hostile.Body.EmptyHandWeapon;
            if (fist == null) throw new InvalidOperationException("the hostile unit has no weapon to attack with");
            hostile.Descriptor.Stats.BaseAttackBonus.BaseValue = 5;
            var attacks = new JArray();
            Record["incomingAttacks"] = attacks;
            Fight(actor, hostile);
            yield return GameSeconds(0.5);
            Fight(actor, hostile);

            int armorClassBase = actor.Descriptor.Stats.AC.ModifiedValue;
            RuleAttackWithWeapon control = Attack(hostile, actor, fist, 10, false, attacks);
            long guardBefore = Counter(DodgeGuard, "ExpiredRemovals");
            long onBefore = Counter(DodgeBonus, "TurnedOn"), offBefore = Counter(DodgeBonus, "TurnedOff");

            // 1. One real activation: the grit transaction, the buff, the bonus against an incoming attack, the end.
            var first = new JObject();
            Record["firstActivation"] = first;
            int gritBefore = Grit(actor);
            double castAt = Now();
            UnitUseAbility command = Use(actor, ability, actor, first);
            yield return Finished(command);
            first.Merge(Used(command));
            Buff buff = OnUnit(actor, blueprint);
            first["gritBefore"] = gritBefore;
            first["gritAfter"] = Grit(actor);
            first["buffOnUnit"] = buff != null;
            first["buffActive"] = buff != null && buff.Active;
            first["timeLeftSeconds"] = TimeLeft(buff);
            first["armorClassBefore"] = armorClassBase;
            first["armorClassAfter"] = actor.Descriptor.Stats.AC.ModifiedValue;
            // The registered ability applies the buff with the native apply-buff action, which marks the buff only when
            // AddBuff hands it back; AddBuff hands nothing back when activation is cut short. The product's custom
            // delivery class is recorded too: it is not one of the ability's components and is never entered.
            first["markedNotDispelableByNativeAction"] = buff != null && buff.IsNotDispelable;
            first["customDeliveryClassEvidence"] = Convert.ToString(ProductStatic(DodgeDiagnostics, "DeliveryEvidence"), CultureInfo.InvariantCulture);
            first["buffAddedNotices"] = Events("added", blueprint, actor, castAt);
            first["availableWhileActive"] = Available(actor, ability);
            Expect(buff != null, "the Dodge buff is not on the gunslinger after the ability was used");
            Expect(Grit(actor) == gritBefore - 1, "Dodge changed grit from " + gritBefore + " to " + Grit(actor) + ", expected one spent");
            Expect(actor.Descriptor.Stats.AC.ModifiedValue == armorClassBase + 2, "armor class went from " + armorClassBase + " to " + actor.Descriptor.Stats.AC.ModifiedValue + ", expected +2");
            Expect((bool)first["markedNotDispelableByNativeAction"], "the native apply-buff action did not receive the Dodge buff back (activation was cut short)");
            Expect((int)first["buffAddedNotices"] == 1, "the game raised " + first["buffAddedNotices"] + " buff-added notices for Dodge, expected 1");
            Expect(!(bool)first["availableWhileActive"], "Dodge can be activated again while it is active");
            Fight(actor, hostile);
            RuleAttackWithWeapon during = Attack(hostile, actor, fist, 10, false, attacks);
            first["incomingAttackArmorClass"] = new JObject
            {
                ["beforeDodge"] = control.AttackRoll.TargetAC, ["duringDodge"] = during.AttackRoll.TargetAC,
                ["flatFootedBefore"] = control.AttackRoll.IsTargetFlatFooted, ["flatFootedDuring"] = during.AttackRoll.IsTargetFlatFooted
            };
            if (control.AttackRoll.IsTargetFlatFooted || during.AttackRoll.IsTargetFlatFooted)
                findings.Add("an incoming attack treated the gunslinger as flat-footed, so the dodge bonus could not be read from it");
            else
                Expect(during.AttackRoll.TargetAC == control.AttackRoll.TargetAC + 2, "an incoming attack read armor class " + during.AttackRoll.TargetAC + " during Dodge, " + control.AttackRoll.TargetAC + " before it");

            yield return Ended(blueprint, actor);
            double? lasted = Lasted(blueprint, actor, castAt);
            first["endedAfterGameSeconds"] = Seconds(lasted);
            first["armorClassAfterEnd"] = actor.Descriptor.Stats.AC.ModifiedValue;
            first["expiryGuardRemovals"] = Counter(DodgeGuard, "ExpiredRemovals") - guardBefore;
            first["buffRemovedNotices"] = Events("removed", blueprint, actor, castAt);
            first["bonusTurnedOn"] = Counter(DodgeBonus, "TurnedOn") - onBefore;
            first["bonusTurnedOff"] = Counter(DodgeBonus, "TurnedOff") - offBefore;
            Expect(OneRound(lasted), lasted.HasValue ? "Dodge lasted " + lasted + " s of game time, expected one round" : "Dodge did not end by itself");
            Expect((int)first["buffRemovedNotices"] == 1, "the game raised " + first["buffRemovedNotices"] + " buff-removed notices for Dodge, expected 1");
            Expect(actor.Descriptor.Stats.AC.ModifiedValue == armorClassBase, "armor class is " + actor.Descriptor.Stats.AC.ModifiedValue + " after Dodge ended, " + armorClassBase + " before it");
            Expect((long)first["bonusTurnedOn"] == 1 && (long)first["bonusTurnedOff"] == 1, "the bonus was turned on " + first["bonusTurnedOn"] + " and off " + first["bonusTurnedOff"] + " times, expected once each");
            Fight(actor, hostile);
            RuleAttackWithWeapon after = Attack(hostile, actor, fist, 10, false, attacks);
            first["incomingAttackArmorClass"]["afterDodge"] = after.AttackRoll.TargetAC;

            // 2. An early end, then a new activation before the first one's deadline: nothing owed to the first
            //    activation may end the second. The check means something only if both activations really happened
            //    and the first one's own instance was really gone before the second; each is required first.
            var recast = new JObject();
            Record["recastBeforeEarlierDeadline"] = recast;
            guardBefore = Counter(DodgeGuard, "ExpiredRemovals");

            var earlierRow = new JObject();
            recast["earlierActivation"] = earlierRow;
            int gritBeforeEarlier = Grit(actor);
            int addedBeforeEarlier = Notices("added", blueprint, actor);
            double earlierCastAt = Now();
            command = Use(actor, ability, actor, earlierRow);
            yield return Finished(command);
            earlierRow.Merge(Used(command));
            Buff earlier = OnUnit(actor, blueprint);
            earlierRow["buffOnUnit"] = earlier != null;
            earlierRow["buffAddedNotices"] = Notices("added", blueprint, actor) - addedBeforeEarlier;
            earlierRow["gritSpent"] = gritBeforeEarlier - Grit(actor);
            bool earlierHappened = command.IsFinished && command.Result == Kingmaker.UnitLogic.Commands.Base.UnitCommand.ResultType.Success &&
                command.ExecutionProcess != null && command.ExecutionProcess.IsEnded && earlier != null &&
                (int)earlierRow["buffAddedNotices"] == 1 && gritBeforeEarlier - Grit(actor) == 1;
            earlierRow["completed"] = earlierHappened;
            Expect(earlierHappened, "the earlier activation of the recast check did not complete as required (command " + earlierRow["commandResult"] +
                ", delivery ended " + earlierRow["deliveryEnded"] + ", buff on unit " + earlierRow["buffOnUnit"] + ", notices " + earlierRow["buffAddedNotices"] +
                ", grit spent " + earlierRow["gritSpent"] + ")");

            yield return GameSeconds(2.0);
            var removalRow = new JObject();
            recast["earlyRemovalByFixture"] = removalRow;
            Buff beforeRemoval = OnUnit(actor, blueprint);
            bool sameAsEarlier = earlier != null && ReferenceEquals(beforeRemoval, earlier);
            int removedBeforeRemoval = Notices("removed", blueprint, actor);
            if (beforeRemoval != null) actor.Descriptor.Buffs.RemoveFact(beforeRemoval);
            bool earlierGone = earlier != null && !actor.Descriptor.Buffs.RawFacts.Contains(earlier) && OnUnit(actor, blueprint) == null;
            removalRow["afterGameSeconds"] = Math.Round(Now() - earlierCastAt, 2);
            removalRow["instanceOnUnitWasTheEarlierOne"] = sameAsEarlier;
            removalRow["earlierInstanceGone"] = earlierGone;
            removalRow["buffRemovedNotices"] = Notices("removed", blueprint, actor) - removedBeforeRemoval;
            removalRow["armorClassAfter"] = actor.Descriptor.Stats.AC.ModifiedValue;
            bool earlierRemoved = sameAsEarlier && earlierGone && (int)removalRow["buffRemovedNotices"] == 1 &&
                actor.Descriptor.Stats.AC.ModifiedValue == armorClassBase;
            removalRow["verified"] = earlierRemoved;
            Expect(earlierRemoved, "the earlier activation's own instance was not verifiably removed before the later one (same instance " + sameAsEarlier +
                ", gone " + earlierGone + ", removed notices " + removalRow["buffRemovedNotices"] + ", armor class " + removalRow["armorClassAfter"] + ")");

            yield return GameSeconds(0.5);
            var laterRow = new JObject();
            recast["laterActivation"] = laterRow;
            double laterCastAt = Now();
            int gritBeforeLater = Grit(actor);
            int addedBeforeLater = Notices("added", blueprint, actor);
            command = Use(actor, ability, actor, laterRow);
            yield return Finished(command);
            laterRow.Merge(Used(command));
            Buff later = OnUnit(actor, blueprint);
            laterRow["castAfterEarlierGameSeconds"] = Math.Round(laterCastAt - earlierCastAt, 2);
            laterRow["buffOnUnit"] = later != null;
            laterRow["isANewInstance"] = later != null && !ReferenceEquals(later, earlier);
            laterRow["timeLeftSeconds"] = TimeLeft(later);
            laterRow["buffAddedNotices"] = Notices("added", blueprint, actor) - addedBeforeLater;
            laterRow["gritSpent"] = gritBeforeLater - Grit(actor);
            bool laterHappened = command.IsFinished && command.Result == Kingmaker.UnitLogic.Commands.Base.UnitCommand.ResultType.Success &&
                command.ExecutionProcess != null && command.ExecutionProcess.IsEnded && later != null && !ReferenceEquals(later, earlier) &&
                (int)laterRow["buffAddedNotices"] == 1 && gritBeforeLater - Grit(actor) == 1;
            laterRow["completed"] = laterHappened;
            Expect(laterHappened, "the later activation of the recast check did not complete as required (command " + laterRow["commandResult"] +
                ", delivery ended " + laterRow["deliveryEnded"] + ", buff on unit " + laterRow["buffOnUnit"] + ", new instance " + laterRow["isANewInstance"] +
                ", notices " + laterRow["buffAddedNotices"] + ", grit spent " + laterRow["gritSpent"] + ")");

            bool prerequisites = earlierHappened && earlierRemoved && laterHappened;
            recast["prerequisitesMet"] = prerequisites;
            if (!prerequisites)
            {
                recast["isolation"] = "not judged";
                findings.Add("the recast isolation check was not judged: an activation or the early removal it depends on is missing");
                if (OnUnit(actor, blueprint) != null) yield return Ended(blueprint, actor);
            }
            else
            {
                yield return Until("the earlier activation's deadline", () => Now() >= earlierCastAt + 6.6, 60);
                Buff atEarlierDeadline = OnUnit(actor, blueprint);
                recast["atEarlierDeadline"] = new JObject
                {
                    ["gameSecondsAfterEarlierCast"] = Math.Round(Now() - earlierCastAt, 2), ["laterStillOnUnit"] = atEarlierDeadline != null,
                    ["sameInstance"] = atEarlierDeadline != null && ReferenceEquals(atEarlierDeadline, later),
                    ["timeLeftSeconds"] = TimeLeft(atEarlierDeadline), ["armorClass"] = actor.Descriptor.Stats.AC.ModifiedValue,
                    ["expiryGuardRemovals"] = Counter(DodgeGuard, "ExpiredRemovals") - guardBefore
                };
                Expect(atEarlierDeadline != null && ReferenceEquals(atEarlierDeadline, later), "the second activation was gone when the first one's deadline passed");
                Expect(actor.Descriptor.Stats.AC.ModifiedValue == armorClassBase + 2,
                    "armor class is " + actor.Descriptor.Stats.AC.ModifiedValue + " at the first deadline, expected the second activation's +2");
                yield return Ended(blueprint, actor);
                double? laterLasted = Lasted(blueprint, actor, laterCastAt);
                recast["laterEndedAfterGameSeconds"] = Seconds(laterLasted);
                recast["armorClassAfterEnd"] = actor.Descriptor.Stats.AC.ModifiedValue;
                recast["expiryGuardRemovals"] = Counter(DodgeGuard, "ExpiredRemovals") - guardBefore;
                recast["activeBonusModifiers"] = Counter(DodgeBonus, "ActiveModifiers");
                recast["isolation"] = "judged";
                Expect(OneRound(laterLasted), laterLasted.HasValue ? "the second activation lasted " + laterLasted + " s of game time, expected one round" : "the second activation did not end by itself");
                Expect(actor.Descriptor.Stats.AC.ModifiedValue == armorClassBase,
                    "armor class is " + actor.Descriptor.Stats.AC.ModifiedValue + " after the second activation ended, " + armorClassBase + " before any");
            }

            // 3. With no grit the ability is refused before anything is spent or applied.
            int remaining = Grit(actor);
            if (remaining > 0) actor.Descriptor.Resources.Spend(Blueprint<BlueprintAbilityResource>("grit"), remaining);
            Record["withoutGrit"] = new JObject { ["grit"] = Grit(actor), ["available"] = Available(actor, ability), ["buffOnUnit"] = OnUnit(actor, blueprint) != null };
            Expect(!Available(actor, ability), "Dodge is available with no grit");
        }

        // ---- armed states: Deadeye, Up Close and Deadly, Steady Aim ----

        private IEnumerator<object> Armed()
        {
            BlueprintAbility ability = Blueprint<BlueprintAbility>("ability");
            BlueprintBuff blueprint = Blueprint<BlueprintBuff>("buff");
            int gritAtArm = int.Parse(Id("gritAtArm"), CultureInfo.InvariantCulture);
            int gritAtShot = int.Parse(Id("gritAtShot"), CultureInfo.InvariantCulture);
            // "refusedWhileArmed" is "true" for an ability whose own logic refuses a second use while its state is up.
            bool refusedWhileArmed = Id("refusedWhileArmed") == "true";
            Watch(blueprint);
            Record["blueprint"] = BuffCensus.Describe(blueprint);
            Game.Instance.IsPaused = true;
            BlueprintArchetype archetype = ids.ContainsKey("archetype") ? Blueprint<BlueprintArchetype>("archetype") : null;
            UnitEntityData actor = Gunslinger("ArmedGunslinger", Near(3f, 3f), 1, 20, archetype);
            UnitEntityData hostile = SpawnHostile(actor, actor.Position + new Vector3(3f, 0f, 0f));
            ItemEntityWeapon firearm = Equip(actor, Firearm(Id("firearm")));
            actor.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            Record["firearm"] = firearm.Blueprint.name;
            Record["archetype"] = archetype == null ? null : archetype.name;
            var shots = new JArray();
            Record["shots"] = shots;
            yield return GameSeconds(0.5);

            // 1. Arm through the real ability, then fire one eligible shot.
            var consumed = new JObject();
            Record["armedThenShot"] = consumed;
            int gritBefore = Grit(actor);
            double armedAt = Now();
            UnitUseAbility command = Use(actor, ability, actor, consumed);
            yield return Finished(command);
            consumed.Merge(Used(command));
            Buff armed = OnUnit(actor, blueprint);
            consumed["gritBefore"] = gritBefore;
            consumed["gritAfterArming"] = Grit(actor);
            consumed["armedOnUnit"] = armed != null;
            consumed["timeLeftSeconds"] = TimeLeft(armed);
            consumed["buffAddedNotices"] = Events("added", blueprint, actor, armedAt);
            consumed["availableWhileArmed"] = Available(actor, ability);
            Expect(armed != null, blueprint.name + " is not on the gunslinger after the ability was used");
            Expect(Grit(actor) == gritBefore - gritAtArm, "arming changed grit from " + gritBefore + " to " + Grit(actor) + ", expected " + gritAtArm + " spent");
            Expect((int)consumed["buffAddedNotices"] == 1, "the game raised " + consumed["buffAddedNotices"] + " buff-added notices for " + blueprint.name + ", expected 1");
            if (refusedWhileArmed) Expect(!(bool)consumed["availableWhileArmed"], "the ability can be used again while armed");
            int gritBeforeShot = Grit(actor);
            RuleAttackWithWeapon shot = Attack(actor, hostile, firearm, 15, true, shots);
            consumed["shotHit"] = shot.AttackRoll.IsHit;
            consumed["armedAfterShot"] = OnUnit(actor, blueprint) != null;
            consumed["gritAfterShot"] = Grit(actor);
            consumed["buffRemovedNotices"] = Events("removed", blueprint, actor, armedAt);
            Expect(shot.AttackRoll.IsHit, "the eligible firearm shot did not hit");
            Expect(OnUnit(actor, blueprint) == null, blueprint.name + " was not consumed by the eligible firearm shot");
            Expect(Grit(actor) == gritBeforeShot - gritAtShot, "the shot changed grit from " + gritBeforeShot + " to " + Grit(actor) + ", expected " + gritAtShot + " spent");
            if (OnUnit(actor, blueprint) != null)
            {
                // Not consumed: let it run out so the next part starts from an unarmed gunslinger.
                yield return Ended(blueprint, actor);
                consumed["leftOverEndedAfterGameSeconds"] = Seconds(Lasted(blueprint, actor, armedAt));
            }

            yield return GameSeconds(1.0);

            // 2. Arm again and do not shoot: the state ends by itself, with no refund.
            var timedOut = new JObject();
            Record["armedThenLeft"] = timedOut;
            gritBefore = Grit(actor);
            armedAt = Now();
            command = Use(actor, ability, actor, timedOut);
            yield return Finished(command);
            timedOut.Merge(Used(command));
            timedOut["armedOnUnit"] = OnUnit(actor, blueprint) != null;
            timedOut["gritBefore"] = gritBefore;
            timedOut["gritAfterArming"] = Grit(actor);
            Expect(OnUnit(actor, blueprint) != null, blueprint.name + " is not on the gunslinger after the second use");
            yield return Ended(blueprint, actor);
            double? lasted = Lasted(blueprint, actor, armedAt);
            timedOut["endedAfterGameSeconds"] = Seconds(lasted);
            timedOut["gritAfterEnd"] = Grit(actor);
            timedOut["buffRemovedNotices"] = Events("removed", blueprint, actor, armedAt);
            Expect(OneRound(lasted), lasted.HasValue ? blueprint.name + " lasted " + lasted + " s of game time unused, expected one round" : blueprint.name + " did not end by itself");
            Expect(Grit(actor) == gritBefore - gritAtArm, "grit is " + Grit(actor) + " after the unused state ended, expected " + (gritBefore - gritAtArm) + " (no refund, no extra charge)");

            // 3. A shot after the state ended gets nothing from it and is charged nothing.
            int gritBeforeLate = Grit(actor);
            Attack(actor, hostile, firearm, 15, true, shots);
            Record["shotAfterEnd"] = new JObject { ["gritBefore"] = gritBeforeLate, ["gritAfter"] = Grit(actor), ["armedOnUnit"] = OnUnit(actor, blueprint) != null };
            Expect(Grit(actor) == gritBeforeLate, "a shot after the state ended changed grit from " + gritBeforeLate + " to " + Grit(actor));
        }

        // ---- Bleeding Wound: four target bleeds ----

        private IEnumerator<object> Bleeds()
        {
            BlueprintAbility[] abilities = Blueprints<BlueprintAbility>("abilities");
            BlueprintBuff[] markers = Blueprints<BlueprintBuff>("markers");
            BlueprintBuff[] bleeds = Blueprints<BlueprintBuff>("bleeds");
            BlueprintAbility stopBleeding = Blueprint<BlueprintAbility>("stopBleeding");
            string[] kinds = { "hitPoints", "Strength", "Dexterity", "Constitution" };
            int[] costs = { 1, 2, 2, 2 };
            if (abilities.Length != 4 || markers.Length != 4 || bleeds.Length != 4) throw new InvalidOperationException("four abilities, markers and bleeds are needed");
            Watch(markers);
            Watch(bleeds);
            Record["blueprints"] = new JArray(bleeds.Select(bleed => (object)BuffCensus.Describe(bleed)).ToArray());
            Game.Instance.IsPaused = true;
            UnitEntityData actor = Gunslinger("BleedGunslinger", Near(3f, 3f), 11, 26);
            ItemEntityWeapon firearm = Equip(actor, Firearm("Pistol"));
            actor.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            int dexterityBonus = actor.Descriptor.Stats.Dexterity.Bonus;
            Record["gunslinger"] = new JObject { ["level"] = 11, ["dexterityBonus"] = dexterityBonus, ["grit"] = Grit(actor) };

            // The targets stand within touch range of the gunslinger, so the cure needs no movement. They are friendly
            // disposable units; the attack rule does not ask about faction.
            var offsets = new[] { new Vector3(0.9f, 0f, 0f), new Vector3(-0.9f, 0f, 0f), new Vector3(0f, 0f, 0.9f), new Vector3(0f, 0f, -0.9f) };
            var victims = new UnitEntityData[4];
            for (int index = 0; index < 4; index++)
            {
                victims[index] = SpawnFriendly("BleedTarget" + kinds[index], actor.Position + offsets[index]);
                victims[index].Descriptor.Stats.Constitution.BaseValue = 20;
                victims[index].Descriptor.Stats.HitPoints.BaseValue = 200;
            }

            var shots = new JArray();
            Record["shots"] = shots;
            yield return GameSeconds(0.5);

            // 1. Each kind: arm through its ability, then one eligible firearm hit. One second later the shot's own
            //    damage has landed; everything after that is the bleed's.
            var applied = new JArray();
            Record["applied"] = applied;
            var appliedAt = new double[4];
            var afterShot = new int[4];
            var bleeding = new bool[4];
            for (int index = 0; index < 4; index++)
            {
                var row = new JObject { ["kind"] = kinds[index] };
                applied.Add(row);
                double armedAt = Now();
                UnitUseAbility arm = Use(actor, abilities[index], actor, row);
                yield return Finished(arm);
                row.Merge(Used(arm));
                row["markerOnGunslinger"] = OnUnit(actor, markers[index]) != null;
                row["markerAddedNotices"] = Events("added", markers[index], actor, armedAt);
                int gritBefore = Grit(actor);
                appliedAt[index] = Now();
                RuleAttackWithWeapon shot = Attack(actor, victims[index], firearm, 15, true, shots);
                Buff bleed = OnUnit(victims[index], bleeds[index]);
                bleeding[index] = bleed != null;
                row["shotHit"] = shot.AttackRoll.IsHit;
                row["markerAfterShot"] = OnUnit(actor, markers[index]) != null;
                row["bleedOnTarget"] = bleed != null;
                row["bleedActive"] = bleed != null && bleed.Active;
                row["bleedTimeLeft"] = TimeLeft(bleed);
                row["bleedAddedNotices"] = Events("added", bleeds[index], victims[index], appliedAt[index]);
                row["gritSpent"] = gritBefore - Grit(actor);
                Expect(shot.AttackRoll.IsHit, "the armed " + kinds[index] + " shot did not hit");
                Expect(bleed != null, kinds[index] + " bleed is not on the target after the armed firearm hit");
                Expect(OnUnit(actor, markers[index]) == null, kinds[index] + " armed marker was not consumed by the shot");
                Expect(gritBefore - Grit(actor) == costs[index], kinds[index] + " bleed spent " + (gritBefore - Grit(actor)) + " grit, expected " + costs[index]);
                Expect((int)row["bleedAddedNotices"] == 1, "the game raised " + row["bleedAddedNotices"] + " buff-added notices for the " + kinds[index] + " bleed, expected 1");
                yield return GameSeconds(1.0);
                afterShot[index] = Suffered(victims[index], kinds[index]);
                row["damageOneSecondAfterShot"] = afterShot[index];
            }

            // 2. Two full rounds after the last hit.
            yield return Until("two rounds after the last hit", () => Now() >= appliedAt[3] + 12.6, 80);
            var rounds = new JArray();
            Record["afterTwoRounds"] = rounds;
            for (int index = 0; index < 4; index++)
            {
                UnitEntityData victim = victims[index];
                int ticks = Ticks(victim, kinds[index], appliedAt[index] + 1.0, Now());
                int inflicted = Suffered(victim, kinds[index]) - afterShot[index];
                rounds.Add(new JObject
                {
                    ["kind"] = kinds[index], ["ticks"] = ticks, ["tickGameSecondsAfterHit"] = TickTimes(victim, kinds[index], appliedAt[index] + 1.0, appliedAt[index]),
                    ["inflictedByBleed"] = inflicted, ["bleedStillOnTarget"] = OnUnit(victim, bleeds[index]) != null,
                    ["gameSecondsSinceHit"] = Math.Round(Now() - appliedAt[index], 2)
                });
                if (!bleeding[index]) continue;
                Expect(ticks >= 2, "the " + kinds[index] + " bleed ticked " + ticks + " times in " + Math.Round(Now() - appliedAt[index], 1) + " s of game time, expected at least 2");
                int perTick = index == 0 ? dexterityBonus : 1;
                Expect(inflicted == ticks * perTick, "the " + kinds[index] + " bleed inflicted " + inflicted + " in " + ticks + " ticks, expected " + perTick + " each");
                Expect(OnUnit(victim, bleeds[index]) != null, "the " + kinds[index] + " bleed ended without being removed");
            }

            // 3. The supported cure: the Utility Shot deed's Stop Bleeding, one loaded shot per bleed, no grit.
            if (ids.ContainsKey("cure") && ids["cure"] == "false")
            {
                Record["cure"] = "not run in this step";
                yield break;
            }

            var cured = new JArray();
            Record["cured"] = cured;
            var curedAt = new double[4];
            var atCure = new int[4];
            for (int index = 0; index < 4; index++)
            {
                var row = new JObject { ["kind"] = kinds[index] };
                cured.Add(row);
                if (!bleeding[index])
                {
                    row["skipped"] = "no bleed to remove";
                    curedAt[index] = Now();
                    atCure[index] = Suffered(victims[index], kinds[index]);
                    continue;
                }

                // A standard action leaves the gunslinger on cooldown for the rest of its round. The fixture clears
                // that between cures; it is not part of what is being judged.
                actor.CombatState.Cooldown.StandardAction = 0f;
                actor.CombatState.Cooldown.MoveAction = 0f;
                actor.CombatState.Cooldown.SwiftAction = 0f;
                LoadRound(firearm);
                int gritBefore = Grit(actor);
                double startedAt = Now();
                UnitEntityData victim = victims[index];
                BlueprintBuff bleedBlueprint = bleeds[index];
                UnitUseAbility cure = Use(actor, stopBleeding, victim, row);
                // The whole command and its delivery, not just the bleed's disappearance: the next cure must not
                // interrupt a command that is still finishing.
                yield return Finished(cure);
                row.Merge(Used(cure));
                Expect(cure.IsFinished && cure.Result == Kingmaker.UnitLogic.Commands.Base.UnitCommand.ResultType.Success && cure.ExecutionProcess != null &&
                    cure.ExecutionProcess.IsEnded, "the Stop Bleeding command for the " + kinds[index] + " bleed did not complete (" + row["commandResult"] +
                    ", delivery ended " + row["deliveryEnded"] + ")");
                curedAt[index] = EventAt("removed", bleedBlueprint, victim, startedAt) ?? Now();
                atCure[index] = Suffered(victim, kinds[index]);
                row["bleedOnTargetAfter"] = OnUnit(victim, bleedBlueprint) != null;
                row["bleedRemovedNotices"] = Events("removed", bleedBlueprint, victim, startedAt);
                row["gameSecondsToRemove"] = Math.Round(curedAt[index] - startedAt, 2);
                row["gritSpent"] = gritBefore - Grit(actor);
                row["inflictedByBleedAtCure"] = atCure[index] - afterShot[index];
                Expect(OnUnit(victim, bleedBlueprint) == null, "Stop Bleeding left the " + kinds[index] + " bleed on its target");
                Expect(gritBefore == Grit(actor), "Stop Bleeding changed grit from " + gritBefore + " to " + Grit(actor));
                Expect((int)row["bleedRemovedNotices"] == 1, "the game raised " + row["bleedRemovedNotices"] + " buff-removed notices for the " + kinds[index] + " bleed, expected 1");
            }

            actor.Commands.InterruptAll(true);

            // 4. More than a round later: no further ticks, and what was already inflicted stays.
            yield return GameSeconds(7.5);
            var afterCure = new JArray();
            Record["afterCure"] = afterCure;
            for (int index = 0; index < 4; index++)
            {
                if (!bleeding[index]) continue;
                int later = Suffered(victims[index], kinds[index]);
                int lateTicks = Ticks(victims[index], kinds[index], curedAt[index] + 0.02, Now());
                afterCure.Add(new JObject
                {
                    ["kind"] = kinds[index], ["ticksAfterCure"] = lateTicks, ["damageAtCure"] = atCure[index], ["damageNow"] = later,
                    ["inflictedByBleed"] = later - afterShot[index], ["gameSecondsSinceCure"] = Math.Round(Now() - curedAt[index], 2)
                });
                Expect(OnUnit(victims[index], bleeds[index]) == null, "the " + kinds[index] + " bleed is back on its target");
                Expect(lateTicks == 0, "the " + kinds[index] + " bleed ticked " + lateTicks + " times after it was removed");
                Expect(later == atCure[index], "damage from the " + kinds[index] + " bleed went from " + atCure[index] + " to " + later + " after removal; removal must neither add nor refund");
                Expect(later - afterShot[index] > 0, "the " + kinds[index] + " bleed had inflicted nothing by the time it was removed");
            }
        }

        private static int Suffered(UnitEntityData victim, string kind)
        {
            switch (kind)
            {
                case "hitPoints": return victim.Descriptor.Damage;
                case "Strength": return victim.Descriptor.Stats.Strength.Damage;
                case "Dexterity": return victim.Descriptor.Stats.Dexterity.Damage;
                default: return victim.Descriptor.Stats.Constitution.Damage;
            }
        }

        private IEnumerable<JToken> TickEvents(UnitEntityData victim, string kind, double since)
        {
            return observer.Damage.Where(token => (string)token["target"] == victim.Blueprint.name && (string)token["kind"] == kind && (double)token["gameSeconds"] >= since - 0.011);
        }

        private int Ticks(UnitEntityData victim, string kind, double since, double until)
        {
            return TickEvents(victim, kind, since).Count(token => (double)token["gameSeconds"] <= until + 0.011);
        }

        private JArray TickTimes(UnitEntityData victim, string kind, double since, double relativeTo)
        {
            return new JArray(TickEvents(victim, kind, since).Select(token => (object)Math.Round((double)token["gameSeconds"] - relativeTo, 2)).ToArray());
        }

        // ---- Startling Shot and Targeting (Head): a condition on the target for one round ----

        private IEnumerator<object> Condition()
        {
            BlueprintAbility ability = Blueprint<BlueprintAbility>("ability");
            BlueprintBuff blueprint = Blueprint<BlueprintBuff>("buff");
            var condition = (UnitCondition)Enum.Parse(typeof(UnitCondition), Id("condition"));
            int gritCost = int.Parse(Id("gritCost"), CultureInfo.InvariantCulture);
            Watch(blueprint);
            Record["blueprint"] = BuffCensus.Describe(blueprint);
            Game.Instance.IsPaused = true;
            UnitEntityData actor = Gunslinger("ConditionGunslinger", Near(3f, 3f), 7, 20);
            UnitEntityData hostile = SpawnHostile(actor, actor.Position + new Vector3(3f, 0f, 0f));
            ItemEntityWeapon firearm = Equip(actor, Firearm("Pistol"));
            actor.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            yield return GameSeconds(0.5);

            var attempts = new JArray();
            Record["attempts"] = attempts;
            double appliedAt = 0;
            Buff buff = null;
            for (int attempt = 0; attempt < 4 && buff == null; attempt++)
            {
                var row = new JObject();
                attempts.Add(row);
                LoadRound(firearm);
                int gritBefore = Grit(actor);
                appliedAt = Now();
                UnitUseAbility command = Use(actor, ability, hostile, row);
                yield return Finished(command);
                row.Merge(Used(command));
                buff = OnUnit(hostile, blueprint);
                row["gritSpent"] = gritBefore - Grit(actor);
                row["buffOnTarget"] = buff != null;
                row["timeLeftSeconds"] = TimeLeft(buff);
                row["conditionOnTarget"] = hostile.Descriptor.State.HasCondition(condition);
                row["buffAddedNotices"] = Events("added", blueprint, hostile, appliedAt);
                if (buff != null)
                {
                    Expect(gritBefore - Grit(actor) == gritCost, ability.name + " spent " + (gritBefore - Grit(actor)) + " grit, expected " + gritCost);
                    Expect(hostile.Descriptor.State.HasCondition(condition), "the target does not have " + condition + " while " + blueprint.name + " is on it");
                    Expect((int)row["buffAddedNotices"] == 1, "the game raised " + row["buffAddedNotices"] + " buff-added notices for " + blueprint.name + ", expected 1");
                }
                else yield return GameSeconds(1.0);
            }

            if (buff == null)
            {
                findings.Add(ability.name + " did not put " + blueprint.name + " on the target in four attempts");
                yield break;
            }

            yield return Ended(blueprint, hostile);
            double? lasted = Lasted(blueprint, hostile, appliedAt);
            Record["end"] = new JObject
            {
                ["endedAfterGameSeconds"] = Seconds(lasted),
                ["conditionOnTargetAfter"] = hostile.Descriptor.State.HasCondition(condition), ["buffRemovedNotices"] = Events("removed", blueprint, hostile, appliedAt)
            };
            Expect(OneRound(lasted), lasted.HasValue ? blueprint.name + " lasted " + lasted + " s of game time, expected one round" : blueprint.name + " did not end by itself");
            Expect(!hostile.Descriptor.State.HasCondition(condition), "the target still has " + condition + " after " + blueprint.name + " ended");
        }

        // ---- Falling Petal ----

        private RuleAttackWithWeapon ConfirmedCritical(UnitEntityData actor, UnitEntityData hostile, ItemEntityWeapon weapon, JArray log)
        {
            for (int attempt = 0; attempt < 12; attempt++)
            {
                RuleAttackWithWeapon attack = Attack(actor, hostile, weapon, 20, false, log);
                if (attack.AttackRoll.IsHit && attack.AttackRoll.IsCriticalConfirmed) return attack;
                UnityEngine.Random.InitState(1000 + attempt);
            }

            throw new InvalidOperationException("no confirmed critical hit in twelve natural 20s");
        }

        private IEnumerator<object> FallingPetal()
        {
            BlueprintItemWeapon named = Blueprint<BlueprintItemWeapon>("weapon");
            BlueprintItemWeapon other = Blueprint<BlueprintItemWeapon>("otherWeapon");
            BlueprintBuff blueprint = Blueprint<BlueprintBuff>("buff");
            Watch(blueprint);
            Record["blueprint"] = BuffCensus.Describe(blueprint);
            Game.Instance.IsPaused = true;
            UnitEntityData actor = SpawnFriendly("FallingPetalWielder", Near(3f, 3f));
            UnitEntityData hostile = SpawnHostile(actor, actor.Position + new Vector3(1.5f, 0f, 0f));
            ItemEntityWeapon weapon = Equip(actor, named);
            actor.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            var attacks = new JArray();
            Record["attacks"] = attacks;
            yield return GameSeconds(0.5);
            int armorClassBase = actor.Descriptor.Stats.AC.ModifiedValue;

            // 1. An ordinary hit grants nothing; a confirmed critical hit grants the bonus, which ends by itself.
            RuleAttackWithWeapon ordinary = Attack(actor, hostile, weapon, 10, false, attacks);
            Record["ordinaryHit"] = new JObject { ["hit"] = ordinary.AttackRoll.IsHit, ["buffOnUnit"] = OnUnit(actor, blueprint) != null };
            Expect(OnUnit(actor, blueprint) == null, "an ordinary hit granted " + blueprint.name);
            double grantedAt = Now();
            ConfirmedCritical(actor, hostile, weapon, attacks);
            Buff buff = OnUnit(actor, blueprint);
            var critical = new JObject
            {
                ["buffOnUnit"] = buff != null, ["timeLeftSeconds"] = TimeLeft(buff), ["armorClassBefore"] = armorClassBase,
                ["armorClassAfter"] = actor.Descriptor.Stats.AC.ModifiedValue, ["buffAddedNotices"] = Events("added", blueprint, actor, grantedAt)
            };
            Record["confirmedCritical"] = critical;
            Expect(buff != null, "a confirmed critical hit did not grant " + blueprint.name);
            Expect(actor.Descriptor.Stats.AC.ModifiedValue == armorClassBase + 1, "armor class went from " + armorClassBase + " to " + actor.Descriptor.Stats.AC.ModifiedValue + ", expected +1");
            Expect((int)critical["buffAddedNotices"] == 1, "the game raised " + critical["buffAddedNotices"] + " buff-added notices for " + blueprint.name + ", expected 1");
            yield return Ended(blueprint, actor);
            double? lasted = Lasted(blueprint, actor, grantedAt);
            critical["endedAfterGameSeconds"] = Seconds(lasted);
            critical["armorClassAfterEnd"] = actor.Descriptor.Stats.AC.ModifiedValue;
            Expect(OneRound(lasted), lasted.HasValue ? blueprint.name + " lasted " + lasted + " s of game time, expected one round" : blueprint.name + " did not end by itself");
            Expect(actor.Descriptor.Stats.AC.ModifiedValue == armorClassBase, "armor class is " + actor.Descriptor.Stats.AC.ModifiedValue + " after " + blueprint.name + " ended, " + armorClassBase + " before it");

            // 2. The weapon boundary: the bonus holds only while Falling Petal is the active weapon.
            ConfirmedCritical(actor, hostile, weapon, attacks);
            int withNamed = actor.Descriptor.Stats.AC.ModifiedValue;
            ItemEntityWeapon plain = Equip(actor, other);
            int armorClassOtherBase = armorClassBase;
            var boundary = new JObject
            {
                ["armorClassWithFallingPetal"] = withNamed, ["otherWeapon"] = plain.Blueprint.name,
                ["armorClassWithOtherWeapon"] = actor.Descriptor.Stats.AC.ModifiedValue, ["buffStillOnUnit"] = OnUnit(actor, blueprint) != null
            };
            Record["weaponBoundary"] = boundary;
            Expect(withNamed == armorClassBase + 1, "armor class is " + withNamed + " after the second confirmed critical hit, expected +1");
            Expect(actor.Descriptor.Stats.AC.ModifiedValue == armorClassOtherBase, "armor class is " + actor.Descriptor.Stats.AC.ModifiedValue + " with another weapon in hand, expected the bonus to be withheld");
            yield return Ended(blueprint, actor);
            boundary["armorClassAfterEnd"] = actor.Descriptor.Stats.AC.ModifiedValue;
            Expect(actor.Descriptor.Stats.AC.ModifiedValue == armorClassOtherBase, "armor class is " + actor.Descriptor.Stats.AC.ModifiedValue + " after the swapped-out bonus ended");
        }

        // ---- Mountain-Sunder ----

        private IEnumerator<object> MountainSunder()
        {
            BlueprintItemWeapon named = Blueprint<BlueprintItemWeapon>("weapon");
            BlueprintBuff marker = Blueprint<BlueprintBuff>("buff");
            BlueprintFeature powerAttackFeature = Blueprint<BlueprintFeature>("powerAttackFeature");
            Watch(marker);
            Record["blueprint"] = BuffCensus.Describe(marker);
            Game.Instance.IsPaused = true;
            UnitEntityData actor = SpawnFriendly("MountainSunderWielder", Near(3f, 3f));
            UnitEntityData hostile = SpawnHostile(actor, actor.Position + new Vector3(1.5f, 0f, 0f));
            actor.Descriptor.Stats.BaseAttackBonus.BaseValue = 100;
            actor.Descriptor.AddFact(powerAttackFeature);
            ActivatableAbility powerAttack = actor.Descriptor.ActivatableAbilities.Enumerable.Single(value => value != null && value.Blueprint.AssetGuid == Id("powerAttackToggle"));
            ItemEntityWeapon weapon = Equip(actor, named);
            var attacks = new JArray();
            Record["attacks"] = attacks;
            powerAttack.IsOn = true;
            yield return Until("Power Attack to run", () => powerAttack.IsRunning, 15);
            Record["powerAttackRunning"] = powerAttack.IsRunning;
            Func<long> applications = () => Counter(EasternDiagnostics, "MountainSunderApplications");
            long start = applications();

            double markedAt = Now();
            Attack(actor, hostile, weapon, 10, false, attacks);
            long afterFirst = applications() - start;
            Buff present = OnUnit(actor, marker);
            Attack(actor, hostile, weapon, 10, false, attacks);
            long afterSecond = applications() - start;
            var round = new JObject
            {
                ["applicationsAfterFirstHit"] = afterFirst, ["applicationsAfterSecondHitSameRound"] = afterSecond, ["markerOnUnit"] = present != null,
                ["markerHiddenInUi"] = marker.IsHiddenInUI, ["markerTimeLeftSeconds"] = TimeLeft(present), ["markerAddedNotices"] = Events("added", marker, actor, markedAt)
            };
            Record["firstRound"] = round;
            Expect(afterFirst == 1, "the first Power Attack hit produced " + afterFirst + " force-damage applications, expected 1");
            Expect(afterSecond == 1, "a second hit in the same round produced " + (afterSecond - afterFirst) + " more applications, expected none");
            Expect(present != null, "the round marker is not on the wielder after the first application");
            Expect(marker.IsHiddenInUI, "the round marker is not hidden");
            yield return Ended(marker, actor);
            double? lasted = Lasted(marker, actor, markedAt);
            round["markerEndedAfterGameSeconds"] = Seconds(lasted);
            Expect(OneRound(lasted), lasted.HasValue ? "the round marker lasted " + lasted + " s of game time, expected one round" : "the round marker did not end by itself");
            Attack(actor, hostile, weapon, 10, false, attacks);
            long nextRound = applications() - start;
            Record["nextRound"] = new JObject { ["applicationsAfterHit"] = nextRound, ["gameSecondsAfterFirstApplication"] = Math.Round(Now() - markedAt, 2), ["markerOnUnit"] = OnUnit(actor, marker) != null };
            Expect(nextRound == 2, "a hit in the next round brought applications to " + nextRound + ", expected 2");
        }
    }
}
