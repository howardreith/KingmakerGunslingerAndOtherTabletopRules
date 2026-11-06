using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Sprint 4 summon reach census and representative threat probes.
        // Read-only: the census reads installed blueprints; a probe places
        // the stilled fixture hostile at fixed distances and asks the native
        // IsReach (with every installed postfix, ProperFlanking2 included)
        // for each wielded weapon. No command, roll or eligibility is
        // forced.
        private IEnumerable<int> ReviewSummonReach4(ExpandedSummoningCorrectionFixture fixture)
        {
            var cells = new JArray();
            var receipt = new JObject { ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint39OverhaulReachPhase,
                ["cells"] = cells, ["scope"] = "Summon weapon reach census, native threat probes and adjacent routines. Native commands only; no roll, position, eligibility or plan is written while a command runs." };
            string path = Path.Combine(_request.EvidenceDirectory, "s4-summon-reach.json");
            Action persist = () => RuntimeTestResultWriter.WriteAtomic(path, receipt.ToString(Formatting.Indented));
            var failed = new List<string>();
            // Cells record PASS/FAIL without stopping the phase, so one run
            // reports every representative; the closing cell fails the
            // phase if any cell failed.
            Action<string, bool, JObject, string> check = (id, pass, row, description) => {
                row["cell"] = id; cells.Add(row); persist();
                Sprint25Check("sprint4-" + id, pass, row, description);
                if (!pass) failed.Add(id);
            };
            JObject census = SummonReachCensus4();
            receipt["census"] = census;
            JObject flanking = InfernalProperFlanking2State35();
            receipt["properFlanking2"] = flanking;
            persist();
            check("reach-census", (int)census["creatureCount"] == ExpandedSummoningCatalog.All.Count &&
                ((JArray)census["errors"]).Count == 0, new JObject {
                    ["creatureCount"] = census["creatureCount"], ["weaponRows"] = ((JArray)census["weapons"]).Count,
                    ["errors"] = census["errors"] },
                "Every catalog creature's installed unit blueprint and weapon references are read.");
            check("properflanking2-dead-zone-active", (bool?)flanking["assemblyLoaded"] == true &&
                (bool?)flanking["modActive"] == true && (bool?)flanking["reachWeaponsDeadZone"] == true &&
                (bool?)flanking["getTargetPointPatched"] == true && (bool?)flanking["isReachPatched"] == true,
                (JObject)flanking.DeepClone(), "ProperFlanking2 is loaded, active, patched and its reach_weapons_dead_zone is enabled; nothing about it is changed.");
            check("no-false-reach-weapons", SummonReachCensusVerdict4(census),
                SummonReachCensusSummary4(census),
                "After Sprint 4 every converted row's type is the native minimum, and the only melee weapons ProperFlanking2 still treats as reach weapons are the genuine glaive and longspear.");
            var probes = new JArray();
            receipt["probes"] = probes;
            foreach (string key in SummonReachProbePolicy4.ProbeKeys)
            {
                var probe = new JObject { ["creature"] = key };
                probes.Add(probe);
                UnitEntityData unit = null;
                try { unit = CastExpandedSummoningOwnTier(fixture, key); }
                catch (Exception error) { probe["castError"] = error.GetType().Name + ": " + error.Message; }
                if (unit != null)
                {
                    InfernalWallPrepareActor35(unit);
                    for (int frame = 0; frame < 3; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                    InfernalWallPrepareActor35(unit);
                    try { SummonReachProbe4(fixture, unit, probe); }
                    catch (Exception error) { probe["probeError"] = error.GetType().Name + ": " + error.Message; }
                    InfernalWallPrepareActor35(fixture.Hostile);
                    CleanupExpandedSummoningUnit(unit);
                    fixture.Created.Remove(unit);
                    ResetExpandedSummoningHostile(fixture);
                    for (int frame = 0; frame < 2; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                }
                JObject verdict = SummonReachProbeVerdict4(probe);
                check("threat-" + key, (string)verdict["failure"] == null, verdict,
                    "Each wielded weapon threatens exactly as the installed contract predicts at the adjacent distance, just inside and just beyond its own range; converted rows keep their pre-Sprint-4 live range and are legal adjacent; genuine reach weapons keep their dead zone.");
            }
            var routines = new JArray();
            receipt["routines"] = routines;
            using (var trace = new InfernalEvents34())
            {
                foreach (string key in SummonReachProbePolicy4.RoutineKeys)
                    foreach (int frame in SummonAdjacentRoutine4(fixture, key, trace, check, routines, flanking)) yield return frame;
            }
            persist();
            check("summon-reach-acceptance", failed.Count == 0, new JObject { ["failedCells"] = new JArray(failed) },
                "Every Sprint 4 reach cell passed.");
            if (failed.Count != 0) throw new InvalidOperationException("Sprint 4 reach cells failed: " + string.Join(", ", failed.ToArray()));
        }

        // Every catalog creature: its registered unit blueprint, size, reach
        // bonuses, and every weapon its body or facts reference.
        private static JObject SummonReachCensus4()
        {
            var creatures = new JArray();
            var weapons = new JArray();
            var errors = new JArray();
            var consumers = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            Action<string, string> consume = (guid, key) => {
                if (guid == null) return;
                HashSet<string> set;
                if (!consumers.TryGetValue(guid, out set)) consumers[guid] = set = new HashSet<string>(StringComparer.Ordinal);
                set.Add(key);
            };
            var monster = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster);
            var ally = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.NaturesAlly);
            foreach (SummonCreatureSpec creature in ExpandedSummoningCatalog.All)
            {
                try
                {
                    string symbol = ExpandedSummoningIdentityCatalog.UnitSymbol(creature);
                    BlueprintScriptableObject registered;
                    ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(symbol, out registered);
                    var unit = registered as BlueprintUnit;
                    bool published = monster.Concat(ally).Any(variant => ReferenceEquals(variant.Creature, creature) &&
                        SummonVisibilityCatalog.IsPublished(variant));
                    var row = new JObject { ["key"] = creature.Key, ["name"] = creature.DisplayName, ["symbol"] = symbol,
                        ["published"] = published, ["monsterTier"] = creature.MonsterTier, ["allyTier"] = creature.NaturesAllyTier };
                    creatures.Add(row);
                    if (unit == null) { errors.Add(new JObject { ["key"] = creature.Key, ["error"] = "unit blueprint not registered" }); continue; }
                    int sizeReach = unit.Size.GetModifiers().Reach;
                    var bonuses = new JArray();
                    var references = new List<KeyValuePair<string, BlueprintItemWeapon>>();
                    var visited = new HashSet<BlueprintScriptableObject>();
                    SummonReachFacts4(unit, unit.ComponentsArray, "unit", bonuses, references, visited, 0);
                    foreach (BlueprintUnitFact fact in unit.AddFacts ?? new BlueprintUnitFact[0])
                        SummonReachFact4(fact, "fact", bonuses, references, visited, 0);
                    var body = unit.Body;
                    if (body != null)
                    {
                        if (!body.DisableHands && body.EmptyHandWeapon != null)
                            references.Add(new KeyValuePair<string, BlueprintItemWeapon>("body.EmptyHandWeapon", body.EmptyHandWeapon));
                        var hands = new[] { new KeyValuePair<string, BlueprintItemEquipmentHand>("body.PrimaryHand", body.PrimaryHand),
                            new KeyValuePair<string, BlueprintItemEquipmentHand>("body.SecondaryHand", body.SecondaryHand),
                            new KeyValuePair<string, BlueprintItemEquipmentHand>("body.PrimaryHandAlternative1", body.PrimaryHandAlternative1),
                            new KeyValuePair<string, BlueprintItemEquipmentHand>("body.SecondaryHandAlternative1", body.SecondaryHandAlternative1),
                            new KeyValuePair<string, BlueprintItemEquipmentHand>("body.PrimaryHandAlternative2", body.PrimaryHandAlternative2),
                            new KeyValuePair<string, BlueprintItemEquipmentHand>("body.SecondaryHandAlternative2", body.SecondaryHandAlternative2),
                            new KeyValuePair<string, BlueprintItemEquipmentHand>("body.PrimaryHandAlternative3", body.PrimaryHandAlternative3),
                            new KeyValuePair<string, BlueprintItemEquipmentHand>("body.SecondaryHandAlternative3", body.SecondaryHandAlternative3) };
                        foreach (var hand in hands)
                            if (hand.Value is BlueprintItemWeapon)
                                references.Add(new KeyValuePair<string, BlueprintItemWeapon>(hand.Key, (BlueprintItemWeapon)hand.Value));
                        foreach (var limb in body.AdditionalLimbs ?? new BlueprintItemWeapon[0])
                            references.Add(new KeyValuePair<string, BlueprintItemWeapon>("body.AdditionalLimbs", limb));
                        foreach (var limb in body.AdditionalSecondaryLimbs ?? new BlueprintItemWeapon[0])
                            references.Add(new KeyValuePair<string, BlueprintItemWeapon>("body.AdditionalSecondaryLimbs", limb));
                    }
                    int reachBonus = bonuses.OfType<JObject>().Sum(value => (int?)value["value"] ?? 0);
                    int reachRange = Math.Max(sizeReach + reachBonus - 5, 0);
                    row["unit"] = unit.AssetGuid; row["unitName"] = unit.name; row["size"] = unit.Size.ToString();
                    row["sizeReachFeet"] = sizeReach; row["reachBonuses"] = bonuses;
                    row["reachStatFeet"] = sizeReach + reachBonus; row["reachRangeFeet"] = reachRange;
                    row["disableHands"] = body != null && body.DisableHands;
                    var slots = new JArray();
                    row["weapons"] = slots;
                    foreach (var group in references.Where(value => value.Value != null).GroupBy(value => value.Value))
                    {
                        BlueprintItemWeapon weapon = group.Key;
                        BlueprintWeaponType type = weapon.Type;
                        float raw = SummonReachRawFeet4(type);
                        float typeRange = type == null ? 0f : type.AttackRange.Value;
                        bool melee = weapon.IsMelee;
                        var weaponRow = new JObject { ["owner"] = creature.Key, ["item"] = weapon.AssetGuid,
                            ["itemName"] = weapon.name, ["type"] = type?.AssetGuid, ["typeName"] = type?.name,
                            ["sources"] = new JArray(group.Select(value => value.Key).Distinct()),
                            ["natural"] = weapon.IsNatural, ["melee"] = melee,
                            ["attackType"] = type?.AttackType.ToString(), ["category"] = type?.Category.ToString(),
                            ["twoHanded"] = type != null && type.IsTwoHanded, ["itemSize"] = weapon.Size.ToString(),
                            ["rawTypeRangeFeet"] = raw, ["typeRangeFeet"] = typeRange,
                            ["computedUnitRangeFeet"] = typeRange + reachRange,
                            ["properFlanking2ReachWeapon"] = melee && typeRange > GameConsts.MinWeaponRange.Value,
                            ["kmgOwnedItem"] = weapon.name != null && weapon.name.StartsWith("KMG_", StringComparison.Ordinal),
                            ["kmgOwnedType"] = type?.name != null && type.name.StartsWith("KMG_", StringComparison.Ordinal) };
                        slots.Add(weaponRow); weapons.Add(weaponRow);
                        consume(weapon.AssetGuid, creature.Key); consume(type?.AssetGuid, creature.Key);
                    }
                }
                catch (Exception error)
                {
                    errors.Add(new JObject { ["key"] = creature.Key, ["error"] = error.GetType().Name + ": " + error.Message });
                }
            }
            foreach (JObject weapon in weapons)
            {
                HashSet<string> set;
                weapon["itemConsumers"] = consumers.TryGetValue((string)weapon["item"] ?? "", out set) ? new JArray(set.OrderBy(value => value)) : new JArray();
                weapon["typeConsumers"] = consumers.TryGetValue((string)weapon["type"] ?? "", out set) ? new JArray(set.OrderBy(value => value)) : new JArray();
            }
            return new JObject { ["creatureCount"] = creatures.Count, ["creatures"] = creatures, ["weapons"] = weapons,
                ["errors"] = errors, ["source"] = "installed blueprints read in-process; unit range computed as type range + max(size reach + reach bonuses - 5, 0), before item-level postfixes" };
        }

        private static void SummonReachFact4(BlueprintUnitFact fact, string path, JArray bonuses,
            List<KeyValuePair<string, BlueprintItemWeapon>> references, HashSet<BlueprintScriptableObject> visited, int depth)
        {
            if (fact == null || depth > 4 || !visited.Add(fact)) return;
            SummonReachFacts4(fact, fact.ComponentsArray, path + "/" + fact.name, bonuses, references, visited, depth);
        }

        private static void SummonReachFacts4(BlueprintScriptableObject owner, BlueprintComponent[] components, string path,
            JArray bonuses, List<KeyValuePair<string, BlueprintItemWeapon>> references,
            HashSet<BlueprintScriptableObject> visited, int depth)
        {
            foreach (BlueprintComponent component in components ?? new BlueprintComponent[0])
            {
                if (component == null) continue;
                var stat = component as AddStatBonus;
                if (stat != null && stat.Stat == StatType.Reach)
                    bonuses.Add(new JObject { ["fact"] = path, ["value"] = stat.Value, ["descriptor"] = stat.Descriptor.ToString() });
                var facts = component as AddFacts;
                if (facts != null)
                    foreach (BlueprintUnitFact child in facts.Facts ?? new BlueprintUnitFact[0])
                        SummonReachFact4(child, path, bonuses, references, visited, depth + 1);
                foreach (FieldInfo field in component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    object value = field.GetValue(component);
                    string where = path + ":" + component.GetType().Name + "." + field.Name;
                    var single = value as BlueprintItemWeapon;
                    if (single != null) references.Add(new KeyValuePair<string, BlueprintItemWeapon>(where, single));
                    var many = value as IEnumerable;
                    if (many != null && !(value is string))
                        foreach (object item in many)
                        {
                            var weapon = item as BlueprintItemWeapon;
                            if (weapon != null) references.Add(new KeyValuePair<string, BlueprintItemWeapon>(where, weapon));
                        }
                }
            }
        }

        private static float SummonReachRawFeet4(BlueprintWeaponType type)
        {
            if (type == null) return 0f;
            FieldInfo field = typeof(BlueprintWeaponType).GetField("m_AttackRange", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            object value = field?.GetValue(type);
            return value is Feet ? ((Feet)value).Value : -1f;
        }

        // Each wielded weapon slot (both hand sets, every limb): its live
        // item range, and native threat at the adjacent distance, just inside
        // its own range and just beyond it.
        private void SummonReachProbe4(ExpandedSummoningCorrectionFixture fixture, UnitEntityData unit, JObject probe)
        {
            UnitEntityData target = fixture.Hostile;
            ResetExpandedSummoningHostile(fixture);
            InfernalWallPrepareActor35(target);
            var slots = new List<KeyValuePair<string, WeaponSlot>>();
            for (int set = 0; set < unit.Body.HandsEquipmentSets.Count; set++)
            {
                slots.Add(new KeyValuePair<string, WeaponSlot>("set" + set + ".primary", unit.Body.HandsEquipmentSets[set].PrimaryHand));
                slots.Add(new KeyValuePair<string, WeaponSlot>("set" + set + ".secondary", unit.Body.HandsEquipmentSets[set].SecondaryHand));
            }
            for (int limb = 0; limb < unit.Body.AdditionalLimbs.Count; limb++)
                slots.Add(new KeyValuePair<string, WeaponSlot>("limb" + limb, unit.Body.AdditionalLimbs[limb]));
            int sizeReach = unit.Descriptor.State.Size.GetModifiers().Reach;
            float corpulence = unit.View.Corpulence + target.View.Corpulence;
            float deadRange = (sizeReach + 1) * .3048f * .5f;
            probe["unit"] = unit.UniqueId; probe["blueprint"] = unit.Blueprint.AssetGuid; probe["unitName"] = unit.Blueprint.name;
            probe["size"] = unit.Descriptor.State.Size.ToString(); probe["corpulence"] = unit.View.Corpulence;
            probe["targetCorpulence"] = target.View.Corpulence; probe["sizeReachFeet"] = sizeReach;
            probe["reachStat"] = unit.Stats.Reach.ModifiedValue; probe["reachRangeFeet"] = unit.Stats.ReachRange.Value;
            probe["properFlanking2DeadRangeMeters"] = deadRange;
            probe["currentHandSet"] = unit.Body.HandsEquipmentSets.IndexOf(unit.Body.CurrentHandsEquipmentSet);
            var wielded = slots.Where(value => value.Value != null && value.Value.HasWeapon).ToArray();
            var ranges = wielded.Select(value => value.Value.Weapon.AttackRange.Meters).Distinct().OrderBy(value => value).ToArray();
            var distances = new List<KeyValuePair<string, float>> { new KeyValuePair<string, float>("adjacent", corpulence + .15f) };
            foreach (float range in ranges)
            {
                distances.Add(new KeyValuePair<string, float>("inside-" + Math.Round(range / .3048f, 1) + "ft", corpulence + range - .10f));
                distances.Add(new KeyValuePair<string, float>("beyond-" + Math.Round(range / .3048f, 1) + "ft", corpulence + range + .10f));
            }
            float farthest = distances.Max(value => value.Value);
            JObject line = SummonReachClearLine4(fixture, unit, farthest);
            probe["line"] = line;
            if ((bool)line["found"] != true) throw new InvalidOperationException("No clear probe line near the summon or caster.");
            Vector3 origin = unit.Position;
            Vector3? direction = new Vector3((float)line["direction"][0], 0f, (float)line["direction"][2]);
            probe["direction"] = SurveyVector(direction.Value);
            var rows = new JArray();
            probe["slots"] = rows;
            foreach (var slot in wielded)
            {
                ItemEntityWeapon weapon = slot.Value.Weapon;
                rows.Add(new JObject { ["slot"] = slot.Key, ["item"] = weapon.Blueprint.AssetGuid,
                    ["itemName"] = weapon.Blueprint.name, ["type"] = weapon.Blueprint.Type?.AssetGuid,
                    ["natural"] = weapon.Blueprint.IsNatural, ["melee"] = weapon.Blueprint.IsMelee,
                    ["rawTypeRangeFeet"] = SummonReachRawFeet4(weapon.Blueprint.Type),
                    ["typeRangeFeet"] = weapon.Blueprint.Type.AttackRange.Value,
                    ["liveItemRangeFeet"] = weapon.AttackRange.Value,
                    ["nativeUnitRangeFeet"] = unit.Descriptor.GetWeaponRange(weapon.Blueprint).Value,
                    ["properFlanking2ReachWeapon"] = weapon.Blueprint.IsMelee &&
                        weapon.Blueprint.Type.AttackRange > GameConsts.MinWeaponRange,
                    ["threat"] = new JArray() });
            }
            foreach (var distance in distances)
            {
                Vector3 wanted = origin + direction.Value * distance.Value;
                Vector3 point = AstarPath.active.GetNearest(wanted).clampedPosition;
                point = new Vector3(wanted.x, point.y, wanted.z);
                PlaceExpandedSummoningUnit(target, point);
                float centre = Vector3.Distance(unit.Position, target.Position);
                float mechanics = unit.DistanceTo(target);
                bool los = unit.HasLOS(target);
                for (int index = 0; index < wielded.Length; index++)
                {
                    ItemEntityWeapon weapon = wielded[index].Value.Weapon;
                    bool threat = false; string error = null;
                    try { threat = unit.IsReach(target, wielded[index].Value); }
                    catch (Exception failure) { error = failure.GetType().Name; }
                    float reach = weapon.AttackRange.Meters;
                    bool nativeExpected = mechanics < corpulence + reach && los;
                    bool deadZone = weapon.Blueprint.IsMelee && weapon.Blueprint.Type.AttackRange > GameConsts.MinWeaponRange &&
                        mechanics < corpulence + deadRange;
                    ((JArray)rows[index]["threat"]).Add(new JObject { ["distance"] = distance.Key,
                        ["centreDistance"] = centre, ["mechanicsDistance"] = mechanics,
                        ["edgeGap"] = mechanics - corpulence, ["boundMeters"] = corpulence + reach,
                        ["lineOfSight"] = los, ["nativeExpected"] = nativeExpected,
                        ["insideProperFlanking2DeadZone"] = deadZone, ["isReach"] = threat, ["error"] = error });
                }
            }
            PlaceExpandedSummoningUnit(target, origin + direction.Value * (farthest + 3f));
        }
        // Read-only diagnostics for one attacker's weapon attacks during a
        // measured command: the outer weapon rule before and after it
        // triggers, and its attack roll's automatic and miss-chance outcome.
        // Every handler is guarded; it never changes a rule.
        private sealed class SummonReachRuleProbe4 : IDisposable,
            IGlobalRulebookHandler<RuleAttackWithWeapon>, IGlobalRulebookHandler<RuleAttackRoll>
        {
            internal readonly JArray Rows = new JArray();
            private readonly string _owner;
            internal SummonReachRuleProbe4(UnitEntityData owner) { _owner = owner.UniqueId; EventBus.Subscribe(this); }
            public void Dispose() { EventBus.Unsubscribe(this); }
            private void Add(string kind, RulebookEvent evt, Action<JObject> fill)
            {
                try
                {
                    if (evt?.Initiator?.UniqueId != _owner) return;
                    var row = new JObject { ["event"] = kind, ["gameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds,
                        ["rule"] = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(evt),
                        ["target"] = evt.GetRuleTarget()?.UniqueId };
                    fill(row);
                    Rows.Add(row);
                }
                catch (Exception error)
                {
                    Rows.Add(new JObject { ["event"] = kind, ["observerError"] = error.GetType().Name + ": " + error.Message });
                }
            }
            public void OnEventAboutToTrigger(RuleAttackWithWeapon evt)
            {
                Add("weapon-about", evt, row => { row["weapon"] = evt.Weapon?.Blueprint?.AssetGuid;
                    row["attackNumber"] = evt.AttackNumber; row["fullAttack"] = evt.IsFullAttack; row["autoHit"] = evt.AutoHit; });
            }
            public void OnEventDidTrigger(RuleAttackWithWeapon evt)
            {
                Add("weapon-did", evt, row => { row["weapon"] = evt.Weapon?.Blueprint?.AssetGuid;
                    row["hit"] = evt.AttackRoll?.IsHit; row["roll"] = evt.AttackRoll == null ? (int?)null : evt.AttackRoll.Roll.Value; });
            }
            public void OnEventAboutToTrigger(RuleAttackRoll evt)
            {
                Add("roll-about", evt, row => { row["autoHit"] = evt.AutoHit; row["autoMiss"] = evt.AutoMiss;
                    row["outerWeaponRule"] = evt.RuleAttackWithWeapon == null ? (int?)null :
                        System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(evt.RuleAttackWithWeapon); });
            }
            public void OnEventDidTrigger(RuleAttackRoll evt)
            {
                Add("roll-did", evt, row => { row["weapon"] = evt.Weapon?.Blueprint?.AssetGuid;
                    row["attackType"] = evt.AttackType.ToString(); row["roll"] = evt.Roll.Value; row["result"] = evt.Result.ToString();
                    row["hit"] = evt.IsHit; row["autoHit"] = evt.AutoHit; row["autoMiss"] = evt.AutoMiss;
                    row["missChance"] = evt.MissChance; row["missChanceRoll"] = evt.MissChanceRoll;
                    row["outerWeaponRule"] = evt.RuleAttackWithWeapon == null ? (int?)null :
                        System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(evt.RuleAttackWithWeapon); });
            }
        }

        // The attacker's active buffs carrying ProperFlanking2's
        // AttackReplacementWithCombatManeuver (read by type name; the mod is
        // optional). Read-only.
        private static JArray SummonReachAttackReplacements4(UnitEntityData unit)
        {
            var result = new JArray();
            foreach (var buff in unit.Buffs.Enumerable)
                foreach (var component in buff.Blueprint.ComponentsArray ?? new BlueprintComponent[0])
                {
                    if (component == null || component.GetType().Name != "AttackReplacementWithCombatManeuver") continue;
                    Func<string, object> field = name => component.GetType().GetField(name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(component);
                    result.Add(new JObject { ["buff"] = buff.Blueprint.name, ["active"] = buff.IsTurnedOn,
                        ["maneuver"] = field("maneuver")?.ToString(), ["onlyFirstAttack"] = field("only_first_attack") as bool?,
                        ["onlyFullAttack"] = field("only_full_attack") as bool?, ["onlyCharge"] = field("only_charge") as bool? });
                }
            return result;
        }

        // A clear straight line for probes: the summon's own spot first, then
        // a ring around the summoner (within summon-follow range). Clear
        // means every point along the line has a walkable node within 0.3 m
        // horizontally. The summon is moved to the chosen origin.
        private static JObject SummonReachClearLine4(ExpandedSummoningCorrectionFixture fixture, UnitEntityData unit, float length)
        {
            var origins = new List<Vector3> { unit.Position };
            foreach (float radius in new[] { 4f, 7f, 10f })
                for (int step = 0; step < 12; step++)
                    origins.Add(fixture.Caster.Position + Quaternion.Euler(0f, step * 30f, 0f) * Vector3.forward * radius);
            Func<Vector3, bool> clear = point => {
                var near = AstarPath.active.GetNearest(point);
                if (near.node == null || !near.node.Walkable) return false;
                Vector3 delta = near.clampedPosition - point; delta.y = 0f;
                return delta.magnitude < .3f;
            };
            int rejected = 0;
            foreach (Vector3 origin in origins)
            {
                if (!clear(origin)) { rejected++; continue; }
                for (int step = 0; step < 16; step++)
                {
                    Vector3 direction = Quaternion.Euler(0f, step * 22.5f, 0f) * Vector3.forward;
                    bool ok = true;
                    for (float along = .5f; along <= length + .6f && ok; along += .5f)
                        ok = clear(origin + direction * along);
                    if (!ok) { rejected++; continue; }
                    if ((origin - unit.Position).sqrMagnitude > .01f)
                        PlaceExpandedSummoningUnit(unit, AstarPath.active.GetNearest(origin).clampedPosition);
                    InfernalWallPrepareActor35(unit);
                    return new JObject { ["found"] = true, ["origin"] = SurveyVector(unit.Position),
                        ["direction"] = SurveyVector(direction), ["movedFromSpawn"] = (origin - origins[0]).sqrMagnitude > .01f,
                        ["rejectedCandidates"] = rejected, ["lengthMeters"] = length };
                }
            }
            return new JObject { ["found"] = false, ["rejectedCandidates"] = rejected, ["lengthMeters"] = length };
        }

        private static bool SummonReachCensusVerdict4(JObject census)
        {
            var weapons = ((JArray)census["weapons"]).OfType<JObject>().ToList();
            foreach (var row in SummonReachRepresentation4.Rows)
            {
                var match = weapons.FirstOrDefault(value => (string)value["owner"] == row.OwnerKey && (string)value["item"] == row.Item);
                if (match == null || (float)match["rawTypeRangeFeet"] != SummonReachRepresentation4.StoredRawFeet ||
                    (bool)match["properFlanking2ReachWeapon"]) return false;
            }
            var remaining = weapons.Where(value => (bool)value["melee"] && (bool)value["properFlanking2ReachWeapon"])
                .Select(value => (string)value["item"]).Distinct().OrderBy(value => value).ToArray();
            return remaining.SequenceEqual(SummonReachProbePolicy4.GenuineReachWeaponItems.OrderBy(value => value));
        }

        private static JObject SummonReachCensusSummary4(JObject census)
        {
            var weapons = ((JArray)census["weapons"]).OfType<JObject>().ToList();
            return new JObject {
                ["convertedRows"] = new JArray(SummonReachRepresentation4.Rows.Select(row => {
                    var match = weapons.FirstOrDefault(value => (string)value["owner"] == row.OwnerKey && (string)value["item"] == row.Item);
                    return new JObject { ["owner"] = row.OwnerKey, ["item"] = row.Item, ["itemName"] = row.ItemName,
                        ["found"] = match != null, ["rawTypeRangeFeet"] = match?["rawTypeRangeFeet"],
                        ["typeRangeFeet"] = match?["typeRangeFeet"], ["properFlanking2ReachWeapon"] = match?["properFlanking2ReachWeapon"],
                        ["expectedLiveRangeFeet"] = row.LiveRangeAfterFeet, ["correctionFeet"] = row.CorrectionFeet };
                })),
                ["remainingReachWeapons"] = new JArray(weapons.Where(value => (bool)value["melee"] &&
                    (bool)value["properFlanking2ReachWeapon"]).Select(value => new JObject { ["owner"] = value["owner"],
                        ["item"] = value["item"], ["itemName"] = value["itemName"], ["typeRangeFeet"] = value["typeRangeFeet"] })) };
        }

        // Per slot and distance: the observed native IsReach must equal the
        // installed contract (edge gap < live range, LOS, and ProperFlanking2's
        // dead zone for a type range above 2 ft). Converted rows must also
        // keep their recorded live range and be legal adjacent; genuine reach
        // weapons must be refused adjacent and legal just inside their range.
        private static JObject SummonReachProbeVerdict4(JObject probe)
        {
            var result = new JObject { ["creature"] = probe["creature"], ["unit"] = probe["unit"],
                ["line"] = probe["line"], ["slots"] = new JArray() };
            string failure = (string)probe["castError"] ?? (string)probe["probeError"];
            var slots = probe["slots"] as JArray;
            if (failure == null && (slots == null || slots.Count == 0)) failure = "no-wielded-weapon";
            string owner = (string)probe["blueprint"];
            foreach (JObject slot in (slots ?? new JArray()).OfType<JObject>())
            {
                var row = SummonReachRepresentation4.Find(owner, (string)slot["item"]);
                bool genuine = SummonReachProbePolicy4.GenuineReachWeaponItems.Contains((string)slot["item"]);
                var threat = ((JArray)slot["threat"]).OfType<JObject>().ToList();
                var summary = new JObject { ["slot"] = slot["slot"], ["itemName"] = slot["itemName"],
                    ["typeRangeFeet"] = slot["typeRangeFeet"], ["liveItemRangeFeet"] = slot["liveItemRangeFeet"],
                    ["properFlanking2ReachWeapon"] = slot["properFlanking2ReachWeapon"],
                    ["converted"] = row != null, ["genuineReachWeapon"] = genuine,
                    ["expectedLiveRangeFeet"] = row == null ? null : (JToken)row.LiveRangeAfterFeet };
                ((JArray)result["slots"]).Add(summary);
                if (!(bool)slot["melee"]) continue;
                foreach (JObject sample in threat)
                {
                    bool modelled = (bool)sample["nativeExpected"] && !(bool)sample["insideProperFlanking2DeadZone"];
                    if ((string)sample["error"] != null || (bool)sample["lineOfSight"] != true)
                        failure = failure ?? ("probe-unusable-" + slot["itemName"] + "-" + sample["distance"]);
                    else if ((bool)sample["isReach"] != modelled)
                        failure = failure ?? ("contract-mismatch-" + slot["itemName"] + "-" + sample["distance"]);
                }
                string live = Math.Round((double)slot["liveItemRangeFeet"], 1) + "ft";
                Func<string, bool?> at = name => (bool?)threat.FirstOrDefault(value => (string)value["distance"] == name)?["isReach"];
                if (row != null)
                {
                    if (Math.Abs((double)slot["liveItemRangeFeet"] - row.LiveRangeAfterFeet) > .01d)
                        failure = failure ?? ("live-range-changed-" + slot["itemName"]);
                    else if ((bool)slot["properFlanking2ReachWeapon"])
                        failure = failure ?? ("still-reach-weapon-" + slot["itemName"]);
                    else if (at("adjacent") != true || at("inside-" + live) != true || at("beyond-" + live) != false)
                        failure = failure ?? ("adjacent-inside-beyond-" + slot["itemName"]);
                }
                if (genuine && (at("adjacent") != false || at("inside-" + live) != true || at("beyond-" + live) != false))
                    failure = failure ?? ("genuine-reach-control-" + slot["itemName"]);
            }
            result["failure"] = failure;
            return result;
        }

        // Independently expected full-attack routine from the body and the
        // Pathfinder full-attack rules: a manufactured primary weapon makes
        // its base-attack iteratives (one more at +6, +11 and +16), a
        // natural primary one attack, and each additional limb one attack.
        // An empty-hand weapon is not a wielded weapon.
        private static List<string> SummonReachExpectedRoutine4(UnitEntityData unit, JObject row)
        {
            var expected = new List<string>();
            int bab = unit.Stats.BaseAttackBonus.ModifiedValue;
            int iteratives = Math.Max(1, Math.Min(4, 1 + (bab - 1) / 5));
            row["baseAttackBonus"] = bab; row["expectedIteratives"] = iteratives;
            ItemEntityWeapon primary = unit.Body.PrimaryHand.MaybeWeapon;
            if (primary != null && !ReferenceEquals(primary, unit.Body.EmptyHandWeapon))
                for (int count = primary.Blueprint.IsNatural ? 1 : iteratives; count > 0; count--)
                    expected.Add(primary.Blueprint.AssetGuid);
            ItemEntityWeapon secondary = unit.Body.SecondaryHand.MaybeWeapon;
            if (secondary != null && !ReferenceEquals(secondary, unit.Body.EmptyHandWeapon) &&
                !(primary != null && primary.Blueprint.IsTwoHanded))
                expected.Add(secondary.Blueprint.AssetGuid);
            foreach (WeaponSlot limb in unit.Body.AdditionalLimbs)
                if (limb != null && limb.HasWeapon) expected.Add(limb.Weapon.Blueprint.AssetGuid);
            return expected;
        }

        // A native full attack by a fresh representative on the adjacent
        // durable hostile. Verdict: SummonAdjacentRoutineVerdict4.
        private IEnumerable<int> SummonAdjacentRoutine4(ExpandedSummoningCorrectionFixture fixture, string key,
            InfernalEvents34 trace, Action<string, bool, JObject, string> check, JArray routines, JObject flanking)
        {
            var row = new JObject { ["creature"] = key };
            routines.Add(row);
            UnitEntityData unit = null;
            try { unit = CastExpandedSummoningOwnTier(fixture, key); }
            catch (Exception error) { row["setupFailure"] = "cast: " + error.GetType().Name + ": " + error.Message; }
            if (unit == null) { check("routine-" + key, false, row, "The representative must be summoned."); yield break; }
            UnitEntityData target = fixture.Hostile;
            InfernalWallPrepareActor35(unit);
            for (int frame = 0; frame < 3; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            row["witnessReset"] = ResetExpandedSummoningHostile(fixture);
            InfernalWallPrepareActor35(unit); InfernalWallPrepareActor35(target);
            float corpulence = unit.View.Corpulence + target.View.Corpulence;
            JObject line = SummonReachClearLine4(fixture, unit, corpulence + 1.5f);
            row["line"] = line;
            if ((bool)line["found"] != true)
            {
                row["setupFailure"] = "no clear line";
                check("routine-" + key, false, row, "A clear adjacent placement is required.");
                CleanupExpandedSummoningUnit(unit); fixture.Created.Remove(unit);
                yield break;
            }
            Vector3 direction = new Vector3((float)line["direction"][0], 0f, (float)line["direction"][2]);
            Vector3 wanted = unit.Position + direction * (corpulence + .15f);
            Vector3 spot = AstarPath.active.GetNearest(wanted).clampedPosition;
            PlaceExpandedSummoningUnit(target, new Vector3(wanted.x, spot.y, wanted.z));
            InfernalWallPrepareActor35(unit); InfernalWallPrepareActor35(target);
            // Settle before measuring; readiness is established before the
            // observed command starts.
            for (int frame = 0; frame < 6; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            InfernalWallPrepareActor35(unit); InfernalWallPrepareActor35(target);
            var expected = SummonReachExpectedRoutine4(unit, row);
            row["expected"] = new JArray(expected);
            row["expectedNames"] = new JArray(expected.Select(guid => unit.Body.AllSlots.OfType<WeaponSlot>()
                .Where(slot => slot.HasWeapon && slot.Weapon.Blueprint.AssetGuid == guid).Select(slot => slot.Weapon.Blueprint.name).FirstOrDefault()));
            row["owner"] = unit.UniqueId; row["target"] = target.UniqueId;
            row["properFlanking2"] = flanking.DeepClone();
            row["prerequisites"] = InfernalAdjacentActors35(unit, target);
            row["edgeGap"] = unit.DistanceTo(target) - corpulence;
            row["centreDistance"] = Vector3.Distance(unit.Position, target.Position);
            bool threatens = false;
            try { threatens = unit.IsReach(target, unit.Body.PrimaryHand); }
            catch (Exception error) { row["isReachError"] = error.GetType().Name; }
            row["threatens"] = threatens;
            Buff lifetimeBefore = unit.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            double remaining = lifetimeBefore == null ? 0d :
                lifetimeBefore.EndTime.TotalSeconds - Game.Instance.TimeController.GameTime.TotalSeconds;
            row["ownerLifetimeRemainingBeforeSeconds"] = remaining;
            if (remaining < 30d)
            {
                row["setupFailure"] = "summon lifetime below 30 s before the command";
                check("routine-" + key, false, row, "Sufficient summon lifetime is required before the command.");
                CleanupExpandedSummoningUnit(unit); fixture.Created.Remove(unit);
                yield break;
            }
            row["attackReplacements"] = SummonReachAttackReplacements4(unit);
            Vector3 unitStart = unit.Position, targetStart = target.Position;
            var command = new UnitAttack(target) { IsSingleAttack = false, ForceFullAttack = true };
            int reference = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(command);
            row["commandReference"] = reference;
            row["commandIssuedGameSeconds"] = Game.Instance.TimeController.GameTime.TotalSeconds;
            float unitTravel = 0f, targetTravel = 0f;
            var rules = new SummonReachRuleProbe4(unit);
            try
            {
                unit.Commands.Run(command);
                DateTime deadline = DateTime.UtcNow.AddSeconds(25);
                int settled = 0;
                while (DateTime.UtcNow < deadline && settled < 30)
                {
                    Game.Instance.IsPaused = false; yield return 0;
                    unitTravel = Math.Max(unitTravel, Vector3.Distance(unitStart, unit.Position));
                    targetTravel = Math.Max(targetTravel, Vector3.Distance(targetStart, target.Position));
                    settled = command.IsFinished ? settled + 1 : 0;
                }
            }
            finally { rules.Dispose(); }
            row["ruleRows"] = rules.Rows;
            var commandRows = trace.Rows.OfType<JObject>().Where(value => (string)value["owner"] == unit.UniqueId &&
                (int?)value["commandReference"] == reference).ToArray();
            var ends = commandRows.Where(value => (string)value["event"] == "command-end").ToArray();
            JObject planRow = ends.LastOrDefault() ?? commandRows.LastOrDefault(value => value["plan"] != null);
            var attackRows = trace.Rows.OfType<JObject>().Where(value => (string)value["event"] == "attack" &&
                (string)value["owner"] == unit.UniqueId && (int?)value["attackCommand"] == reference).ToArray();
            row["command"] = Sprint27AiCommand(command);
            row["commandFinished"] = command.IsFinished;
            row["commandResult"] = command.Result.ToString();
            row["nativeExhaustedInterrupt"] = Sprint40NativeExhaustedAttack(command, ends, expected.Count);
            row["plan"] = planRow?["plan"] is JArray ? new JArray(((JArray)planRow["plan"]).Select(slot => (string)slot["weapon"])) : new JArray();
            row["commandEnds"] = new JArray(ends.Select(value => new JObject { ["owner"] = value["owner"],
                ["target"] = value["target"], ["commandReference"] = value["commandReference"],
                ["gameSeconds"] = value["gameSeconds"], ["attackIndex"] = value["attackIndex"],
                ["plannedAttackPresent"] = value["plannedAttackPresent"], ["interruptionCaller"] = value["interruptionCaller"] }));
            row["commandEvents"] = new JArray(commandRows.Select(value => new JObject { ["event"] = value["event"],
                ["gameSeconds"] = value["gameSeconds"], ["attackIndex"] = value["attackIndex"],
                ["plannedAttackPresent"] = value["plannedAttackPresent"] }));
            row["attacks"] = new JArray(attackRows.Select(value => new JObject { ["owner"] = value["owner"],
                ["target"] = value["target"], ["attackCommand"] = value["attackCommand"],
                ["gameSeconds"] = value["gameSeconds"], ["weapon"] = value["weapon"], ["hit"] = value["hit"],
                ["hpDamage"] = value["hpDamage"], ["rangeFeet"] = value["rangeFeet"] }));
            row["attackCount"] = attackRows.Length;
            row["hits"] = attackRows.Count(value => (bool?)value["hit"] == true);
            row["misses"] = attackRows.Count(value => (bool?)value["hit"] != true);
            row["ownerTravel"] = unitTravel; row["targetTravel"] = targetTravel;
            row["ownerLifetimePresentAfter"] = unit.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) != null;
            row["ownerAliveAfter"] = !unit.Destroyed && unit.IsInGame && !unit.Descriptor.State.IsDead;
            row["after"] = InfernalAdjacentActors35(unit, target);
            // Every row the attacker produced from the command to its end,
            // whatever command it names, so an unresolved slot is explained.
            double issued = (double)row["commandIssuedGameSeconds"];
            double ended = ends.Length == 0 ? double.MaxValue : ends.Max(value => (double?)value["gameSeconds"] ?? 0d) + .5d;
            row["windowRows"] = new JArray(trace.Rows.OfType<JObject>().Where(value =>
                    (string)value["owner"] == unit.UniqueId && ((double?)value["gameSeconds"] ?? 0d) >= issued &&
                    ((double?)value["gameSeconds"] ?? 0d) <= ended)
                .Select(value => new JObject { ["event"] = value["event"], ["gameSeconds"] = value["gameSeconds"],
                    ["target"] = value["target"], ["weapon"] = value["weapon"], ["hit"] = value["hit"],
                    ["d20"] = value["d20"], ["hpDamage"] = value["hpDamage"], ["attackCommand"] = value["attackCommand"],
                    ["commandReference"] = value["commandReference"], ["attackIndex"] = value["attackIndex"],
                    ["nativeCommandType"] = value["nativeCommandType"], ["autoHit"] = value["autoHit"],
                    ["attackType"] = value["attackType"], ["bonus"] = value["bonus"], ["ac"] = value["ac"] }));
            // A grab rider on the final slot (the Salamander tail) grapples at
            // once; the native grapple then makes the attacker unable to act,
            // and UnitActionController interrupts the exhausted command.
            Func<JObject, string, string, bool> has = (actors, who, buff) =>
                ((JArray)actors[who]["buffs"]).Any(value => (string)value == buff);
            var acts = commandRows.Where(value => (string)value["event"] == "command-act").ToArray();
            JObject finalAttack = attackRows.OrderBy(value => (double?)value["gameSeconds"] ?? 0d).LastOrDefault();
            row["terminalGrab"] = new JObject {
                ["attackerHoldBefore"] = has((JObject)row["prerequisites"], "attacker", "KMG_Summoning_Special_Grapple_Hold"),
                ["attackerHoldAfter"] = has((JObject)row["after"], "attacker", "KMG_Summoning_Special_Grapple_Hold"),
                ["targetGrappledBefore"] = has((JObject)row["prerequisites"], "target", "KMG_Summoning_Special_Grapple_Grappled"),
                ["targetGrappledAfter"] = has((JObject)row["after"], "target", "KMG_Summoning_Special_Grapple_Grappled"),
                ["endAtFinalAct"] = ends.Length > 0 && acts.Length > 0 &&
                    Math.Abs(((double?)ends.Last()["gameSeconds"] ?? -1d) - ((double?)acts.Last()["gameSeconds"] ?? -2d)) < .001d,
                ["finalAttackHit"] = finalAttack != null && (bool?)finalAttack["hit"] == true,
                ["finalAttackWeapon"] = finalAttack?["weapon"] };
            // A planned slot the installed ProperFlanking2 attack replacement
            // resolved as a combat maneuver: an act of this command with no
            // weapon attack, and a standalone attack roll at that instant
            // against this target with a planned weapon (s4f Storm Giant:
            // Improved Sunder replaced the first iterative).
            var replaced = new JArray();
            foreach (JObject act in acts)
            {
                double at = (double?)act["gameSeconds"] ?? -1d;
                if (attackRows.Any(value => Math.Abs(((double?)value["gameSeconds"] ?? -2d) - at) < .001d)) continue;
                JObject roll = ((JArray)row["ruleRows"]).OfType<JObject>().FirstOrDefault(value =>
                    (string)value["event"] == "roll-did" && value["outerWeaponRule"]?.Type == JTokenType.Null &&
                    Math.Abs(((double?)value["gameSeconds"] ?? -2d) - at) < .001d);
                if (roll == null) continue;
                replaced.Add(new JObject { ["owner"] = unit.UniqueId, ["target"] = roll["target"],
                    ["attackCommand"] = reference, ["attackIndex"] = act["attackIndex"], ["gameSeconds"] = at,
                    ["weapon"] = roll["weapon"], ["standaloneRoll"] = true, ["rollResult"] = roll["result"] });
            }
            row["replacedSlots"] = replaced;
            var before = ((JArray)((JObject)row["prerequisites"])["target"]["buffs"]).Select(value => (string)value).ToList();
            row["targetEffectsGained"] = new JArray(((JArray)((JObject)row["after"])["target"]["buffs"])
                .Select(value => (string)value).Where(value => !before.Contains(value)).Distinct());
            row["forcedRolls"] = 0; row["observerWritesDuringCommand"] = 0;
            string failure = SummonAdjacentRoutineVerdict4.Failure(row);
            row["verdictFailure"] = failure;
            check("routine-" + key, failure == null, row,
                "A native full attack on the adjacent durable hostile plans the independently expected routine, resolves every slot against that target under that command, ends at a native completion boundary, keeps the summon's lifetime, and neither unit moves; ProperFlanking2's dead zone is active.");
            InfernalWallPrepareActor35(unit); InfernalWallPrepareActor35(target);
            CleanupExpandedSummoningUnit(unit); fixture.Created.Remove(unit);
            ResetExpandedSummoningHostile(fixture);
            for (int frame = 0; frame < 2; frame++) { Game.Instance.IsPaused = false; yield return 0; }
        }
    }
}
