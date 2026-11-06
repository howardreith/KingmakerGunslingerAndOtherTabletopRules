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
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
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
            var receipt = new JObject { ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint4SummonReachPhase,
                ["cells"] = cells, ["scope"] = "Read-only summon weapon reach census and native threat probes." };
            string path = Path.Combine(_request.EvidenceDirectory, "s4-summon-reach.json");
            Action persist = () => RuntimeTestResultWriter.WriteAtomic(path, receipt.ToString(Formatting.Indented));
            Action<string, bool, JObject, string> check = (id, pass, row, description) => {
                row["cell"] = id; cells.Add(row); persist();
                Sprint25Check("sprint4-" + id, pass, row, description);
                if (!pass) throw new InvalidOperationException("Sprint 4 reach cell failed: " + id);
            };
            JObject census = SummonReachCensus4();
            receipt["census"] = census;
            receipt["properFlanking2"] = InfernalProperFlanking2State35();
            persist();
            check("reach-census", (int)census["creatureCount"] == ExpandedSummoningCatalog.All.Count &&
                ((JArray)census["errors"]).Count == 0, new JObject {
                    ["creatureCount"] = census["creatureCount"], ["weaponRows"] = ((JArray)census["weapons"]).Count,
                    ["errors"] = census["errors"] },
                "Every catalog creature's installed unit blueprint and weapon references are read.");
            var probes = new JArray();
            receipt["probes"] = probes;
            foreach (string key in SummonReachProbePolicy4.ProbeKeys)
            {
                var probe = new JObject { ["creature"] = key };
                probes.Add(probe);
                UnitEntityData unit = null;
                try { unit = CastExpandedSummoningOwnTier(fixture, key); }
                catch (Exception error) { probe["castError"] = error.GetType().Name + ": " + error.Message; }
                if (unit == null) { persist(); continue; }
                InfernalWallPrepareActor35(unit);
                for (int frame = 0; frame < 3; frame++) { Game.Instance.IsPaused = false; yield return 0; }
                InfernalWallPrepareActor35(unit);
                try { SummonReachProbe4(fixture, unit, probe); }
                catch (Exception error) { probe["probeError"] = error.GetType().Name + ": " + error.Message; }
                persist();
                InfernalWallPrepareActor35(fixture.Hostile);
                CleanupExpandedSummoningUnit(unit);
                fixture.Created.Remove(unit);
                ResetExpandedSummoningHostile(fixture);
                for (int frame = 0; frame < 2; frame++) { Game.Instance.IsPaused = false; yield return 0; }
            }
            check("reach-probes-recorded", probes.OfType<JObject>().All(value => value["castError"] == null &&
                    value["probeError"] == null && value["slots"] is JArray && ((JArray)value["slots"]).Count > 0),
                new JObject { ["probes"] = probes.Count,
                    ["failed"] = new JArray(probes.OfType<JObject>().Where(value => value["castError"] != null ||
                        value["probeError"] != null).Select(value => value["creature"])) },
                "Every representative is summoned and every wielded weapon's native threat is observed at the probe distances.");
            persist();
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
            Vector3 origin = unit.Position;
            float farthest = distances.Max(value => value.Value);
            Vector3? direction = null;
            for (int step = 0; step < 16 && direction == null; step++)
            {
                Vector3 candidate = Quaternion.Euler(0f, step * 22.5f, 0f) * Vector3.forward;
                bool clear = distances.All(value => {
                    var near = AstarPath.active.GetNearest(origin + candidate * value.Value);
                    return near.node != null && near.node.Walkable && Vector3.Distance(near.clampedPosition, origin + candidate * value.Value) < .1f;
                }) && AstarPath.active.GetNearest(origin + candidate * farthest).node != null;
                if (clear) direction = candidate;
            }
            if (direction == null) throw new InvalidOperationException("No clear probe direction around the summon.");
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
                Vector3 point = AstarPath.active.GetNearest(origin + direction.Value * distance.Value).clampedPosition;
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
    }
}
