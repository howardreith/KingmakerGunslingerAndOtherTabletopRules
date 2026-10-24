using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private static BlueprintAbilityResource Sprints3031PersistenceResource(string key, string suffix)
        {
            string prefix = key == "dretch" ? "KMG.Summoning.OutsiderCelestial.Dretch." :
                key == "astral-deva" ? ExpandedSummoningSpecialBuilder.Astral31Prefix :
                throw new InvalidOperationException("No pilot SLA resource contract for this key.");
            return (BlueprintAbilityResource)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + suffix];
        }

        private static string[] Sprints3031PersistenceResourceNames(string key)
        {
            return key == "dretch" ? new[] { "CauseFearResource", "StinkingCloudResource" } :
                key == "astral-deva" ? new[] { "HealResource", "BladeBarrierResource", "CureLightWoundsResource", "SeeInvisibilityResource" } :
                Array.Empty<string>();
        }

        private static void PrepareSprints3031PersistenceState(UnitEntityData unit, string key)
        {
            // Explicit saved preconditions, matching existing S29 practice.
            // These spends are NOT evidence of casts, actions, AI or effects.
            foreach (string suffix in key == "dretch" ? new[] { "StinkingCloudResource" } :
                key == "astral-deva" ? new[] { "HealResource", "CureLightWoundsResource" } : Array.Empty<string>())
            {
                var resource = Sprints3031PersistenceResource(key, suffix);
                int expected = suffix == "CureLightWoundsResource" ? 7 : 1;
                if (unit.Descriptor.Resources.GetResourceAmount(resource) != expected)
                    throw new InvalidOperationException("Pilot saved resource precondition is not fresh: " + key + "/" + suffix);
                unit.Descriptor.Resources.Spend(resource, 1);
            }
            if (key == "trumpet-archon")
            {
                var creation = unit.Blueprint.GetComponent<TrumpetPreparedBook>();
                var book = unit.Descriptor.GetSpellbook(creation.Book);
                if (book == null || book.CasterLevel != 14) throw new InvalidOperationException("Pilot prepared book is not CL14.");
                var slot = book.GetMemorizedSpellSlots(6).First(value => value.Available &&
                    ReferenceEquals(value.Spell?.Blueprint, creation.Heal));
                slot.Spell.ParamSpellSlot = slot;
                slot.Spell.Spend();
                if (slot.Available) throw new InvalidOperationException("Native prepared Heal slot did not become spent.");
            }
        }

        private static JObject Sprints3031PersistenceState(UnitEntityData unit, string key)
        {
            var result = new JObject {
                ["owner"] = unit.UniqueId,
                ["activeSet"] = unit.Body.CurrentHandEquipmentSetIndex,
                // ItemEntity has no public stable instance ID in installed2.1.7b.
                // Preserve exact blueprint/owner/index/count/charges/loot state;
                // never invent a cross-process GetInstanceID identity.
                ["ownedItems"] = new JArray(unit.Descriptor.Inventory.Items
                    .OrderBy(item => item.InventorySlotIndex).ThenBy(item => item.Blueprint.AssetGuid, StringComparer.Ordinal)
                    .Select(item => new JObject {
                        ["blueprint"] = item.Blueprint.AssetGuid, ["owner"] = item.Owner?.Unit?.UniqueId,
                        ["inventoryIndex"] = item.InventorySlotIndex, ["count"] = item.Count,
                        ["charges"] = item.Charges, ["nonRemovable"] = item.IsNonRemovable,
                        ["lootable"] = item.IsLootable })),
                ["resources"] = new JObject(Sprints3031PersistenceResourceNames(key).Select(suffix =>
                    new JProperty(suffix, unit.Descriptor.Resources.GetResourceAmount(Sprints3031PersistenceResource(key, suffix)))))
            };
            if (key == "trumpet-archon")
            {
                var creation = unit.Blueprint.GetComponent<TrumpetPreparedBook>();
                var book = unit.Descriptor.GetSpellbook(creation.Book);
                if (book == null) throw new InvalidOperationException("Native saved Trumpet book absent.");
                result["book"] = new JObject {
                    ["blueprint"] = creation.Book.AssetGuid, ["casterLevel"] = book.CasterLevel,
                    ["preparations"] = new JArray(Enumerable.Range(1, 7).SelectMany(level =>
                        book.GetMemorizedSpellSlots(level).Select((slot, index) => new JObject {
                            ["level"] = level, ["index"] = index, ["spell"] = slot.Spell?.Blueprint.AssetGuid,
                            ["available"] = slot.Available, ["main"] = slot.IsMainSlot,
                            ["busy"] = slot.BusySlotsCount }))) };
            }
            result["savedEffects"] = new JArray(Sprints3031SavedEffects(unit).Select(buff => new JObject {
                ["blueprint"] = buff.Blueprint.AssetGuid, ["source"] = buff.Context?.MaybeCaster?.UniqueId,
                ["cl"] = buff.Context?.Params.CasterLevel, ["dc"] = buff.Context?.Params.DC,
                ["permanent"] = buff.IsPermanent, ["notDispelable"] = buff.IsNotDispelable,
                ["endTicks"] = buff.IsPermanent ? (JToken)JValue.CreateNull() : new JValue(buff.EndTime.Ticks) }));
            return result;
        }

        private static BlueprintBuff Sprints3031SavedMenace(bool immunity)
        {
            return BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintBuff>().Single(value =>
                value.name == "KMG_Summoning_OutsiderCelestial_Archon_Menace" + (immunity ? "Immunity" : "Penalty"));
        }

        private static Buff[] Sprints3031SavedEffects(UnitEntityData unit)
        {
            var penalty = Sprints3031SavedMenace(false); var immunity = Sprints3031SavedMenace(true);
            return unit.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, penalty) ||
                ReferenceEquals(buff.Blueprint, immunity) || buff.Blueprint.AssetGuid == "319b4679f25779e4e9d04360381254e1")
                .OrderBy(buff => buff.Blueprint.AssetGuid, StringComparer.Ordinal)
                .ThenBy(buff => buff.Context?.MaybeCaster?.UniqueId, StringComparer.Ordinal).ToArray();
        }

        private static BlueprintAbilityAreaEffect[] Sprints3031SavedAreaBlueprints()
        {
            return new[] {
                BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil").Area,
                (BlueprintAbilityAreaEffect)ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.Astral31Prefix + "ProtectiveArea"],
                BlueprintBootstrap.Library.GetAllBlueprints().OfType<BlueprintAbilityAreaEffect>()
                    .Single(value => value.name == "KMG_Summoning_OutsiderCelestial_Archon_MenaceArea") };
        }

        private void Sprints3031PersistenceAbsence(string stage)
        {
            var blueprints = Sprints3031SavedAreaBlueprints();
            var units = Game.Instance.State.Units.All.Where(unit => unit != null && !unit.Destroyed).ToArray();
            var keys = ExpandedSummoningRosterPersistencePolicy.OutsiderCelestialKeys;
            var scopedUnits = units.Where(unit => keys.Contains(unit.Get<UnitPartRelease143RosterReceipt>()?.Key)).ToArray();
            var areas = Game.Instance.State.AreaEffects.All.Where(area => blueprints.Contains(area.Blueprint)).ToArray();
            var menace = units.SelectMany(unit => Sprints3031SavedEffects(unit))
                .Where(buff => ReferenceEquals(buff.Blueprint, Sprints3031SavedMenace(false)) ||
                    ReferenceEquals(buff.Blueprint, Sprints3031SavedMenace(true))).ToArray();
            bool absent = scopedUnits.Length == 0 && areas.Length == 0 && menace.Length == 0;
            RosterPersistenceCheck("s3031-" + stage + "-absence", absent,
                new JObject { ["scopedUnits"] = scopedUnits.Length, ["areas"] = areas.Length,
                    ["menaceGrants"] = menace.Length },
                "This finite Working fixture requires no preexisting selected aura; no unrelated grant is removed to satisfy the boundary.");
            if (!absent) throw new InvalidOperationException("Pilot saved-state absence/precondition failed: " + stage);
        }

        private static void PrepareSprints3031SavedEffects(UnitEntityData[] units)
        {
            var victim = units.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "lemure");
            foreach (string key in new[] { "hound-archon", "lantern-archon", "trumpet-archon" })
            {
                var source = units.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == key);
                var blueprint = Sprints3031SavedMenace(key == "lantern-archon");
                var context = new MechanicsContext(source, victim.Descriptor, blueprint, null, new TargetWrapper(victim));
                context.Recalculate();
                var buff = victim.Buffs.AddBuff(blueprint, context, TimeSpan.FromHours(24));
                if (buff == null) throw new InvalidOperationException("Pilot saved Menace precondition rejected.");
                buff.IsNotDispelable = true;
            }
            var hound = units.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "hound-archon");
            var dretch = units.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "dretch");
            var aid = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol["KMG.Summoning.OutsiderCelestial.HoundArchon." + "Aid"];
            var granted = hound.Descriptor.Abilities.GetAbility(aid);
            if (granted == null) throw new InvalidOperationException("Pilot Hound has no native Aid access.");
            var data = new AbilityData(granted);
            var aidBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(BlueprintBootstrap.Library,
                "319b4679f25779e4e9d04360381254e1", "pilot saved native Aid effect");
            var aidContext = new AbilityExecutionContext(data, data.CalculateParams(), new TargetWrapper(dretch), null);
            if (dretch.Buffs.AddBuff(aidBuff, aidContext, TimeSpan.FromMinutes(6)) == null)
                throw new InvalidOperationException("Pilot saved Aid precondition rejected.");
            // Explicit state preparation, never a claim of native cast, save,
            // hit, AI decision, or Aid temporary-HP roll. Actual consumers have
            // independent combat proofs. All victims are among the six owners.
        }

        private void ObserveSprints3031PersistentUnit(UnitEntityData unit, string key, JObject profile,
            UnitEntityData[] units, ItemEntity[] partyItems, int[] partyCounts)
        {
            var state = (JObject)profile["sprints3031"];
            var resources = (JObject)state["resources"];
            bool resourcesExact = key == "dretch" ? (int)resources["CauseFearResource"] == 1 &&
                (int)resources["StinkingCloudResource"] == 0 : key == "astral-deva" ?
                (int)resources["HealResource"] == 0 && (int)resources["BladeBarrierResource"] == 1 &&
                (int)resources["CureLightWoundsResource"] == 6 && (int)resources["SeeInvisibilityResource"] == 7 : resources.Count == 0;
            bool slotsExact = true;
            if (key == "trumpet-archon")
            {
                var creation = unit.Blueprint.GetComponent<TrumpetPreparedBook>();
                var book = unit.Descriptor.GetSpellbook(creation.Book);
                var slots = Enumerable.Range(1, 7).SelectMany(level => book.GetMemorizedSpellSlots(level)).ToArray();
                slotsExact = book.CasterLevel == 14 && slots.Count(slot => slot.Spell != null) == 26 &&
                    slots.Count(slot => slot.Spell != null && !slot.Available) == 1 &&
                    slots.Count(slot => ReferenceEquals(slot.Spell?.Blueprint, creation.Heal) && slot.Available) == 1 &&
                    slots.Count(slot => ReferenceEquals(slot.Spell?.Blueprint, creation.Heal) && !slot.Available) == 1;
            }
            var items = unit.Descriptor.Inventory.Items.ToArray();
            bool itemsExact = items.Distinct().Count() == items.Length && items.All(item =>
                ReferenceEquals(item.Owner, unit.Descriptor) && item.IsNonRemovable && !item.IsLootable && item.Count == 1) &&
                Sprint28PartyInventoryExact(partyItems, partyCounts);
            var effects = Sprints3031SavedEffects(unit);
            bool effectsExact = key == "lemure" ? effects.Length == 3 && effects.All(buff =>
                units.Contains(buff.Context?.MaybeCaster) && !buff.IsPermanent && buff.IsNotDispelable &&
                buff.TimeLeft.TotalSeconds > 86000 && buff.TimeLeft.TotalSeconds <= 86400) &&
                effects.Count(buff => ReferenceEquals(buff.Blueprint, Sprints3031SavedMenace(false))) == 2 &&
                effects.Count(buff => ReferenceEquals(buff.Blueprint, Sprints3031SavedMenace(true))) == 1 :
                key == "dretch" ? effects.Length == 1 && effects[0].Context?.Params.CasterLevel == 6 &&
                    effects[0].Context?.MaybeCaster == units.Single(value => value.Get<UnitPartRelease143RosterReceipt>().Key == "hound-archon") &&
                    !effects[0].IsPermanent && effects[0].TimeLeft.TotalSeconds > 200 && effects[0].TimeLeft.TotalSeconds <= 360 : effects.Length == 0;
            RosterPersistenceCheck("s3031-items-resources-effects-" + key,
                resourcesExact && slotsExact && itemsExact && effectsExact,
                new JObject { ["key"] = key, ["state"] = state.DeepClone(), ["resourcesExact"] = resourcesExact,
                    ["slotsExact"] = slotsExact, ["itemsExact"] = itemsExact, ["effectsExact"] = effectsExact,
                    ["partyInventoryExact"] = Sprint28PartyInventoryExact(partyItems, partyCounts),
                    ["fixtureSetupNotCombatProof"] = true },
                "Actual saved native item owners, independently spent resources/preparations and finite source-specific victim states survive without reseeding.");
        }

        private void ObserveSprints3031PersistentAuras(UnitEntityData[] units)
        {
            var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
            var astralRecipient = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol[
                ExpandedSummoningSpecialBuilder.Astral31Prefix + "ProtectiveRecipient"];
            var dretch = units.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "dretch");
            foreach (var unit in units)
            {
                string key = unit.Get<UnitPartRelease143RosterReceipt>().Key;
                int expected = key == "hound-archon" || key == "trumpet-archon" || key == "astral-deva" ? 2 : key == "lantern-archon" ? 1 : 0;
                var areas = Game.Instance.State.AreaEffects.All.Where(area => ReferenceEquals(area.Context?.MaybeCaster, unit)).ToArray();
                bool exact = areas.Length == expected && areas.All(area => !area.Destroyed && area.IsInState);
                if (key == "hound-archon" || key == "trumpet-archon" || key == "astral-deva")
                {
                    int cl = key == "hound-archon" ? 6 : key == "trumpet-archon" ? 14 : 15;
                    var circles = areas.Where(area => ReferenceEquals(area.Blueprint, circle.Area)).ToArray();
                    var carriers = unit.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, circle.Carrier)).ToArray();
                    exact &= circles.Length == 1 && carriers.Length == 1 && carriers[0].Context?.Params.CasterLevel == cl &&
                        circles[0].Context?.Params.CasterLevel == cl && dretch.Buffs.Enumerable.Count(buff =>
                            ReferenceEquals(buff.Blueprint, circle.Recipient) && buff.SourceAreaEffectId == circles[0].UniqueId &&
                            ReferenceEquals(buff.Context?.MaybeCaster, unit)) == 1;
                    if (key == "astral-deva") exact &= dretch.Buffs.Enumerable.Count(buff =>
                        ReferenceEquals(buff.Blueprint, astralRecipient) && ReferenceEquals(buff.Context?.MaybeCaster, unit)) == 1;
                }
                RosterPersistenceCheck("s3031-aura-source-" + key, exact,
                    new JObject { ["key"] = key, ["owner"] = unit.UniqueId, ["expectedAreas"] = expected,
                        ["areas"] = new JArray(areas.Select(area => new JObject { ["id"] = area.UniqueId,
                            ["blueprint"] = area.Blueprint.AssetGuid, ["source"] = area.Context?.MaybeCaster?.UniqueId,
                            ["cl"] = area.Context?.Params.CasterLevel })),
                        ["recipient"] = dretch.UniqueId, ["moduleEnabled"] = _context.FeatureModules.Active.ExpandedSummoning },
                    "Cold native aura owners and recipient grants reconstruct once with each source's own caster level; observation does not refresh or recreate them.");
            }
            var lemure = units.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "lemure");
            var hound = units.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "hound-archon");
            int evilAc = Rulebook.Trigger(new RuleCalculateAC(lemure, dretch, AttackType.Melee)).TargetAC;
            int goodAc = Rulebook.Trigger(new RuleCalculateAC(hound, dretch, AttackType.Melee)).TargetAC;
            var spell = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                "d7cbd2004ce66a042aeab2e95a3c5c61", "pilot cold circle save consumer");
            int evilSave = CircleSave(lemure, dretch, spell), goodSave = CircleSave(hound, dretch, spell);
            RosterPersistenceCheck("s3031-cold-aura-consumers", evilAc - goodAc == 4 && evilSave - goodSave == 4,
                new JObject { ["recipient"] = dretch.UniqueId, ["evilAc"] = evilAc, ["goodAc"] = goodAc,
                    ["evilSave"] = evilSave, ["goodSave"] = goodSave, ["forcedRolls"] = 0 },
                "Actual native AC/save consumers retain the angel's nonstacking +4 against evil over the shared +2 circles after hydration.");
        }
    }
}
