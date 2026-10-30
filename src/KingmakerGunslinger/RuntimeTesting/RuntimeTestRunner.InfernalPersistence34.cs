using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string InfernalSavedScope34 = "KMG_Sprint34_OwnedEffects_v1";
        private Buff[] _infernalLoadedEffects34 = Array.Empty<Buff>();
        private static UnitEntityData[] InfernalSavedRecipients34(UnitEntityData caster) =>
            Sprint32SavedAuxiliaries(caster).Where(unit => unit.Get<UnitPartSprint32ListenerReceipt>().Scope == InfernalSavedScope34).ToArray();
        private static T InfernalSavedBlueprint34<T>(bool kyton, string suffix) where T : BlueprintScriptableObject =>
            (T)ExpandedSummoningBlueprints.Registered.BySymbol[(kyton ? ExpandedSummoningSpecialBuilder.Kyton34Prefix :
                ExpandedSummoningSpecialBuilder.Barbazu34Prefix) + suffix];
        private static BlueprintBuff[] InfernalSavedBuffs34() => new[] {
            InfernalSavedBlueprint34<BlueprintBuff>(false, "InfernalWound"),
            InfernalSavedBlueprint34<BlueprintBuff>(false, "DevilChills"),
            InfernalSavedBlueprint34<BlueprintBuff>(true, "Shaken"),
            InfernalSavedBlueprint34<BlueprintBuff>(true, "ControlledChains") };
        private static bool OwnsInfernalRecipient34(UnitEntityData target, UnitEntityData caster, UnitEntityData[] sources)
        {
            var receipt = target.Get<UnitPartSprint32ListenerReceipt>();
            if (receipt == null || receipt.Scope != InfernalSavedScope34 || receipt.UnitId != target.UniqueId ||
                receipt.CasterId != caster.UniqueId || receipt.Role != "infernal-effects" || string.IsNullOrEmpty(receipt.PrepareRunId) ||
                !ReferenceEquals(target.Blueprint, BlueprintRoot.Instance.DefaultPlayerCharacter) ||
                !ReferenceEquals(target.HoldingState, caster.HoldingState) || Game.Instance.Player.Party.Contains(target)) return false;
            return sources.Any(source => source.UniqueId == receipt.SourceId &&
                source.Get<UnitPartRelease143RosterReceipt>()?.Key == InfernalVisualPolicy34.Barbazu);
        }
        private static JObject InfernalSavedEffect34(Buff buff)
        {
            var disease = buff.SelectComponents<DevilChills34>().SingleOrDefault();
            return new JObject { ["buff"] = buff.Blueprint.AssetGuid, ["owner"] = buff.Owner.Unit.UniqueId,
                ["source"] = buff.Context?.MaybeCaster?.UniqueId, ["ability"] = buff.Context?.SourceAbility?.AssetGuid,
                ["dc"] = buff.Context?.Params?.DC, ["casterLevel"] = buff.Context?.Params?.CasterLevel,
                ["permanent"] = buff.IsPermanent, ["endTicks"] = buff.EndTime.Ticks,
                ["nextExposure"] = disease == null ? (JToken)JValue.CreateNull() : new JValue(disease.NextExposure.Ticks),
                ["consecutiveSaves"] = disease == null ? (JToken)JValue.CreateNull() : new JValue(disease.ConsecutiveSaves) };
        }
        private static JObject InfernalSavedRegeneration34(UnitEntityData kyton) => new JObject {
            ["owner"] = kyton.UniqueId, ["suppressionRound"] = InfernalRegeneration34(kyton)?.SuppressionRound,
            ["active"] = (bool)kyton.Descriptor.State.IsRegenerate, ["damage"] = kyton.Damage };
        private IEnumerable<int> PrepareInfernalSaved34(UnitEntityData caster, UnitEntityData[] sources)
        {
            if (!Game.Instance.IsPaused || InfernalSavedRecipients34(caster).Length != 0)
                throw new InvalidOperationException("Expected paused empty Sprint34 persistence recipient.");
            var barbazu = sources.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>()?.Key == InfernalVisualPolicy34.Barbazu);
            var kyton = sources.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>()?.Key == InfernalVisualPolicy34.Kyton);
            var party = Game.Instance.Player.Party.ToArray();
            var intake = Game.Instance.State.Units.All.Where(unit => unit != null && !sources.Contains(unit) && !party.Contains(unit)).ToArray();
            var faction = SelectSprint32ListenerFaction(caster, sources, party, intake);
            var target = Game.Instance.EntityCreator.SpawnUnit(BlueprintRoot.Instance.DefaultPlayerCharacter,
                caster.Position, Quaternion.identity, caster.HoldingState);
            Game.Instance.EntityCreator.Tick();
            var receipt = target.Ensure<UnitPartSprint32ListenerReceipt>();
            receipt.Scope = InfernalSavedScope34; receipt.UnitId = target.UniqueId; receipt.CasterId = caster.UniqueId;
            receipt.SourceId = barbazu.UniqueId; receipt.Role = "infernal-effects"; receipt.PrepareRunId = _request.RunId;
            receipt.ListenerFaction = faction.AssetGuid;
            SetExpandedSummoningBrainActive(target, false); target.GroupId = target.UniqueId;
            target.Descriptor.SwitchFactions(faction, true); target.AttackFactions.Match(Array.Empty<BlueprintFaction>());
            var group = target.Group;
            if (!OwnsInfernalRecipient34(target, caster, sources) || group.IsPlayerParty || group.Count != 1 || !ReferenceEquals(group[0], target))
                throw new InvalidOperationException("Sprint34 recipient is not privately owned.");
            group.ResetFactionSet(); group.UpdateAttackFactionsCache();
            if (intake.Any(unit => target.IsEnemy(unit) || unit.IsEnemy(target))) throw new InvalidOperationException("Sprint34 witness reaches protected intake.");
            var buffs = InfernalSavedBuffs34();
            for (int index = 0; index < buffs.Length; index++) {
                var owner = index == 3 ? kyton : target;
                MechanicsContext context;
                if (index < 2) context = AstralVictimEffectContext.Create(barbazu, owner, buffs[index],
                    new AbilityParams { DC = InfernalRules34.BeardDiseaseDc(barbazu.Stats.Constitution.Bonus), CasterLevel = 6, SpellLevel = 0 });
                else {
                    var ability = InfernalSavedBlueprint34<BlueprintAbility>(true, index == 2 ? "UnnervingGaze" : "DancingChains");
                    var data = new AbilityData(kyton.Descriptor.Abilities.GetAbility(ability));
                    context = new AbilityExecutionContext(data, data.CalculateParams(), new TargetWrapper(owner), null);
                }
                // Native effect-only saved preconditions. Attack, injury, gaze save
                // and action costs are proved separately, never fabricated here.
                var apply = new RuleApplyBuff(owner, buffs[index], context, index == 2 ? (TimeSpan?)TimeSpan.FromSeconds(18) : null,
                    (blueprint, parent, duration) => owner.Buffs.AddBuff(blueprint, parent, duration));
                Rulebook.Trigger(apply);
                if (apply.AppliedBuff == null || !apply.AppliedBuff.Active || apply.AppliedBuff.IsDisposed)
                    throw new InvalidOperationException("Sprint34 saved effect precondition refused.");
            }
            for (int frame = 0; frame < 3; frame++) yield return 0;
            var effects = target.Buffs.Enumerable.Concat(kyton.Buffs.Enumerable).Where(buff => buffs.Contains(buff.Blueprint)).ToArray();
            var injury = new PhysicalDamage(new DiceFormula(0, DiceType.Zero), PhysicalDamageForm.Slashing);
            injury.AddBonus(10); injury.AddMaterial(PhysicalDamageMaterial.Silver);
            var damage = Rulebook.Trigger(new RuleDealDamage(barbazu, kyton, new DamageBundle(injury)));
            if (damage.Damage <= 0 || InfernalRegeneration34(kyton)?.SuppressionRound != 1 || (bool)kyton.Descriptor.State.IsRegenerate)
                throw new InvalidOperationException("Native saved regeneration suppression precondition missing.");
            receipt.ControlState = new JObject {
                ["effects"] = new JArray(effects.OrderBy(buff => buff.Blueprint.AssetGuid, StringComparer.Ordinal).Select(InfernalSavedEffect34)),
                ["regeneration"] = InfernalSavedRegeneration34(kyton)
            }.ToString(Formatting.None);
            ObserveInfernalSaved34(caster, sources, "prepared");
        }
        private void ObserveInfernalSaved34(UnitEntityData caster, UnitEntityData[] sources, string phase)
        {
            var targets = InfernalSavedRecipients34(caster);
            if (targets.Length != 1 || !OwnsInfernalRecipient34(targets[0], caster, sources))
                throw new InvalidOperationException("Exact saved Sprint34 recipient absent.");
            var target = targets[0]; var receipt = target.Get<UnitPartSprint32ListenerReceipt>();
            var saved = JObject.Parse(receipt.ControlState); var blueprints = InfernalSavedBuffs34();
            var effects = sources.Concat(new[] { target }).SelectMany(unit => unit.Buffs.Enumerable)
                .Where(buff => blueprints.Contains(buff.Blueprint)).OrderBy(buff => buff.Blueprint.AssetGuid, StringComparer.Ordinal).ToArray();
            var kyton = sources.Single(unit => unit.Get<UnitPartRelease143RosterReceipt>().Key == InfernalVisualPolicy34.Kyton);
            var actual = new JObject { ["effects"] = new JArray(effects.Select(InfernalSavedEffect34)),
                ["regeneration"] = InfernalSavedRegeneration34(kyton) };
            bool enabled = _context.FeatureModules.Active.ExpandedSummoning;
            bool exact = enabled ? effects.Length == blueprints.Length && actual.ToString(Formatting.None) == saved.ToString(Formatting.None) &&
                effects.All(buff => buff.Active && !buff.IsDisposed && sources.Contains(buff.Context?.MaybeCaster) &&
                    (buff.IsPermanent || buff.TimeLeft > TimeSpan.Zero)) &&
                effects.Single(buff => buff.Blueprint == blueprints[1]).SelectComponents<DevilChills34>().Single().NextExposure > Game.Instance.Player.GameTime &&
                InfernalMartialProfile34.For(kyton).ControlsOwnChains(kyton) &&
                InfernalRegeneration34(kyton)?.SuppressionRound == 1 && !(bool)kyton.Descriptor.State.IsRegenerate :
                effects.Length == 0 && !(bool)kyton.Descriptor.State.IsRegenerate;
            _infernalLoadedEffects34 = effects;
            RosterPersistenceCheck("s34-effects-" + phase, exact, new JObject { ["recipient"] = target.UniqueId,
                ["moduleEnabled"] = enabled, ["saved"] = saved, ["actual"] = actual, ["effectOnlyFixture"] = true },
                "Cold native source/recipient contexts, finite gaze deadline, day-scale disease state, owned-chain control and native regeneration suppression are exact ON, safely disabled OFF.");
            if (!exact) throw new InvalidOperationException("Sprint34 saved effect ownership or state differs.");
        }
        private IEnumerable<int> CleanupInfernalSaved34(UnitEntityData caster)
        {
            var targets = InfernalSavedRecipients34(caster);
            if (targets.Length != 1) throw new InvalidOperationException("Sprint34 cleanup recipient ambiguous.");
            var target = targets[0]; var blueprints = InfernalSavedBuffs34();
            bool clean = _infernalLoadedEffects34.All(buff => buff.IsDisposed) &&
                !target.Buffs.Enumerable.Any(buff => blueprints.Contains(buff.Blueprint));
            RosterPersistenceCheck("s34-effects-source-cleanup", clean, new JObject { ["loadedCount"] = _infernalLoadedEffects34.Length,
                ["allDisposed"] = _infernalLoadedEffects34.All(buff => buff.IsDisposed), ["recipient"] = target.UniqueId },
                "Destroying saved sources retires every owned effect before the exact recipient is retired.");
            if (!clean) throw new InvalidOperationException("Sprint34 owned effect survived its source.");
            CleanupExpandedSummoningUnit(target);
            for (int frame = 0; frame < 3; frame++) { Game.Instance.EntityDestroyer.Tick(); yield return 0; }
            _infernalLoadedEffects34 = Array.Empty<Buff>(); InfernalSavedAbsence34(caster, "cleanup");
        }
        private void InfernalSavedAbsence34(UnitEntityData caster, string phase)
        {
            var blueprints = InfernalSavedBuffs34();
            int recipients = InfernalSavedRecipients34(caster).Length;
            int effects = Game.Instance.State.Units.All.Where(unit => unit != null).Sum(unit =>
                unit.Buffs.Enumerable.Count(buff => blueprints.Contains(buff.Blueprint)));
            RosterPersistenceCheck("s34-effects-" + phase + "-absence", recipients == 0 && effects == 0,
                new JObject { ["recipients"] = recipients, ["effects"] = effects }, "No saved S34 recipient or orphan effect remains; no unrelated effect is removed.");
            if (recipients != 0 || effects != 0) throw new InvalidOperationException("Sprint34 saved state remains.");
        }
        private static JObject InfernalSavedItems34(UnitEntityData unit) => new JObject {
            ["activeSet"] = unit.Body.CurrentHandEquipmentSetIndex,
            ["items"] = new JArray(unit.Descriptor.Inventory.Items.OrderBy(item => item.Blueprint.AssetGuid, StringComparer.Ordinal)
                .Select(item => new JObject { ["blueprint"] = item.Blueprint.AssetGuid, ["owner"] = item.Owner?.Unit?.UniqueId,
                    ["count"] = item.Count, ["nonRemovable"] = item.IsNonRemovable, ["lootable"] = item.IsLootable })) };
        private void ObserveInfernalItems34(UnitEntityData[] units, ItemEntity[] partyItems, int[] partyCounts)
        {
            var all = units.SelectMany(unit => unit.Descriptor.Inventory.Items).ToArray();
            bool exact = all.Length != 0 && all.Distinct().Count() == all.Length && Sprint28PartyInventoryExact(partyItems, partyCounts);
            foreach (var unit in units) {
                var body = unit.Blueprint.Body;
                var expected = new BlueprintItem[] { body.PrimaryHand, body.SecondaryHand, body.PrimaryHandAlternative1,
                    body.SecondaryHandAlternative1, body.PrimaryHandAlternative2, body.SecondaryHandAlternative2,
                    body.PrimaryHandAlternative3, body.SecondaryHandAlternative3 }.Where(value => value != null)
                    .Concat(body.AdditionalLimbs ?? Array.Empty<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>())
                    .Concat(body.AdditionalSecondaryLimbs ?? Array.Empty<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>()).Select(value => value.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                var items = unit.Descriptor.Inventory.Items.ToArray();
                exact &= items.Select(item => item.Blueprint.AssetGuid).OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(expected) &&
                    items.All(item => ReferenceEquals(item.Owner, unit.Descriptor) && item.IsNonRemovable && !item.IsLootable && item.Count == 1);
            }
            RosterPersistenceCheck("s34-items-owned", exact, new JObject { ["units"] = new JArray(units.Select(InfernalSavedItems34)),
                ["instances"] = new JArray(all.Select(RuntimeHelpers.GetHashCode)), ["partyInventoryExact"] = Sprint28PartyInventoryExact(partyItems, partyCounts) },
                "Both saved owners retain exactly their blueprint-declared separate nontransferable arms, including inactive slots; party inventory is unchanged.");
            if (!exact) throw new InvalidOperationException("Sprint34 saved item ownership differs.");
        }
    }
}
