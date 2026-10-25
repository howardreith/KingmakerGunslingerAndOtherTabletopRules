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

        private void PrepareSprints3031PersistenceState(UnitEntityData unit, string key)
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
                var preparations = Enumerable.Range(1, 7).SelectMany(level =>
                    book.GetMemorizedSpellSlots(level).Select((slot, index) =>
                        new { Level = level, Index = index, Slot = slot, Available = slot.Available })).ToArray();
                var heals = preparations.Where(value => value.Level == 6 &&
                    ReferenceEquals(value.Slot.Spell?.Blueprint, creation.Heal)).ToArray();
                if (heals.Length != 2 || heals.Any(value => !value.Available) ||
                    preparations.Any(value => value.Slot.Spell != null && !value.Available))
                    throw new InvalidOperationException("Pilot native preparations are not fresh.");
                var requested = heals[0];
                var data = requested.Slot.Spell;
                data.ParamSpellSlot = requested.Slot;
                data.Spend();
                // Installed2.1.7b SpendInternal scans matching preparations in
                // reverse and AbilityData.Equals ignores ParamSpellSlot. It
                // may debit the other identical Heal. Observe every native
                // slot; do not write availability or prescribe its index.
                var changed = preparations.Where(value => value.Available != value.Slot.Available).ToArray();
                bool exact = changed.Length == 1 && changed[0].Available && !changed[0].Slot.Available &&
                    heals.Contains(changed[0]) && heals.Count(value => value.Slot.Available) == 1;
                RosterPersistenceCheck("s3031-prepared-slot-setup", exact,
                    new JObject { ["owner"] = unit.UniqueId, ["book"] = book.Blueprint.AssetGuid,
                        ["casterLevel"] = book.CasterLevel, ["requestedLevel"] = requested.Level,
                        ["requestedIndex"] = requested.Index, ["nativeSpendCalls"] = 1,
                        ["fixtureAvailabilityWrites"] = 0, ["fixtureSetupNotCombatProof"] = true,
                        ["preparations"] = new JArray(preparations.Select(value => new JObject {
                            ["level"] = value.Level, ["index"] = value.Index,
                            ["spell"] = value.Slot.Spell?.Blueprint.AssetGuid,
                            ["before"] = value.Available, ["after"] = value.Slot.Available })) },
                    "One native Spend debits exactly one of two equivalent Heal preparations and preserves every other slot.");
                if (!exact) throw new InvalidOperationException("Native prepared Heal spend changed an unexpected slot set.");
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
            UnitEntityData[] units, ItemEntity[] partyItems, int[] partyCounts, bool prepare)
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
                prepare ? "Pre-save setup contains the actual item owners, spent resources/preparations and finite source-specific victim states; no cold-load claim." :
                    "Actual saved native item owners, independently spent resources/preparations and finite source-specific victim states survive fresh loading without reseeding.");
        }

        private static int Sprints3031PersistenceAttackAC(UnitEntityData attacker, UnitEntityData target, ItemEntityWeapon weapon)
        {
            // Same installed native consumer as CircleAttackAC, with an
            // explicitly selected real weapon and no damage restoration.
            var roll = Rulebook.Trigger(new RuleAttackWithWeapon(attacker, target, weapon, 0)).AttackRoll;
            if (roll == null || roll.ACRule == null)
                throw new InvalidOperationException("Saved aura native weapon attack produced no AC rule.");
            return roll.TargetAC;
        }

        private void ObserveSprints3031PersistentAuras(UnitEntityData[] units, bool prepare)
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
                            ["cl"] = area.Context?.Params.CasterLevel, ["ended"] = area.IsEnded,
                            ["destroyed"] = area.Destroyed, ["inGame"] = area.IsInGame, ["inState"] = area.IsInState,
                            ["parentCarrierMatches"] = unit.Buffs.Enumerable.Count(buff => ReferenceEquals(buff.Context, area.Context?.ParentContext)) })),
                        ["carriers"] = new JArray(unit.Buffs.Enumerable.Where(buff =>
                            buff.SelectComponents<Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect>().Any()).Select(buff =>
                            new JObject { ["blueprint"] = buff.Blueprint.AssetGuid, ["active"] = buff.Active,
                                ["components"] = new JArray(buff.SelectComponents<Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect>().Select(component =>
                                    new JObject { ["name"] = component.name, ["areaId"] =
                                        (typeof(Kingmaker.UnitLogic.Buffs.Components.AddAreaEffect).GetField("m_AreaEffectInstance",
                                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(component) as AreaEffectEntityData)?.UniqueId })) })),
                        ["recipient"] = dretch.UniqueId, ["moduleEnabled"] = _context.FeatureModules.Active.ExpandedSummoning },
                    prepare ? "Pre-save native aura owners and recipient grants have each source's own caster level; no cold-load claim." :
                        "Cold native aura owners and recipient grants reconstruct once with each source's own caster level; observation does not refresh or recreate them.");
            }
            var lemure = units.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "lemure");
            var hound = units.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "hound-archon");
            // ArmorClassBonusAgainstAlignment listens to RuleAttackWithWeapon,
            // not standalone RuleCalculateAC. Lemure and Hound deliberately
            // have no EmptyHandWeapon. Use their actual owned claw and bite,
            // without switching equipment or adding a fixture weapon. Astral
            // is durable and is not one of Lemure's saved Menace sources: a hit
            // must not legitimately clear the very penalty being persisted.
            // Dretch's exact grants remain checked above. Keep actual damage.
            var recipient = units.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == "astral-deva");
            var evilWeapon = lemure.Body.PrimaryHand.MaybeWeapon;
            var goodWeapon = hound.Body.AdditionalLimbs.Select(slot => slot.MaybeWeapon).Single(value => value != null);
            if (evilWeapon == null || goodWeapon == null ||
                !ReferenceEquals(evilWeapon.Wielder?.Unit, lemure) || !ReferenceEquals(goodWeapon.Wielder?.Unit, hound) ||
                !recipient.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, astralRecipient)) ||
                Sprints3031SavedEffects(lemure).Any(buff => ReferenceEquals(buff.Context?.MaybeCaster, recipient)))
                throw new InvalidOperationException("Saved aura attack consumer lacks its owned weapon/grant or would consume a saved Menace source.");
            int damageBefore = recipient.Damage;
            int evilAc = Sprints3031PersistenceAttackAC(lemure, recipient, evilWeapon);
            int damageAfterEvil = recipient.Damage;
            int goodAc = Sprints3031PersistenceAttackAC(hound, recipient, goodWeapon);
            bool recipientAlive = !recipient.Descriptor.State.IsDead && !recipient.Destroyed && recipient.IsInState;
            var spell = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                "d7cbd2004ce66a042aeab2e95a3c5c61", "pilot cold circle save consumer");
            int evilSave = CircleSave(lemure, recipient, spell), goodSave = CircleSave(hound, recipient, spell);
            RosterPersistenceCheck("s3031-" + (prepare ? "prepare" : "cold") + "-aura-consumers", recipientAlive && evilAc - goodAc == 4 && evilSave - goodSave == 4,
                new JObject { ["recipient"] = recipient.UniqueId, ["evilAc"] = evilAc, ["goodAc"] = goodAc,
                    ["evilSave"] = evilSave, ["goodSave"] = goodSave, ["forcedRolls"] = 0,
                    ["phase"] = prepare ? "pre-save-setup" : "fresh-load",
                    ["evilAttacker"] = lemure.UniqueId, ["goodAttacker"] = hound.UniqueId,
                    ["evilWeapon"] = evilWeapon.Blueprint.AssetGuid,
                    ["goodWeapon"] = goodWeapon.Blueprint.AssetGuid,
                    ["damageBefore"] = damageBefore, ["damageAfterEvil"] = damageAfterEvil,
                    ["damageAfterGood"] = recipient.Damage, ["recipientAlive"] = recipientAlive, ["observerHealing"] = false },
                prepare ? "Pre-save actual native weapon-attack AC and save consumers show the angel's nonstacking +4 over shared +2 circles; damage remains real." :
                    "After fresh loading, actual native weapon-attack AC and save consumers retain the angel's nonstacking +4 over shared +2 circles; damage remains real.");
        }
    }
}
