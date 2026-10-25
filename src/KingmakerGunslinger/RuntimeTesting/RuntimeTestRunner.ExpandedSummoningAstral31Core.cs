using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewAstral31Core(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            Alignment casterAlignment = fixture.Caster.Descriptor.Alignment.Value;
            UnitEntityData owner = null, sibling = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            UnitEntityData target = fixture.Hostile;
            var areaBaseline = Game.Instance.State.AreaEffects.All.ToArray();
            var targetBuffBaseline = target.Buffs.Enumerable.ToArray();
            var protectedPartyCombatBefore = fixture.PartyBefore
                .OfType<UnitEntityData>().ToDictionary(unit => unit,
                    unit => unit.IsInCombat);
            bool playerCombatAtStart = Game.Instance.Player.IsInCombat;
            var nativeTurnController = Game.Instance.TurnBasedCombatController;
            bool turnBasedCombatAtStart = TurnBased.Controllers.CombatController
                .IsInTurnBasedCombat();
            bool turnHasEnemyAtStart = nativeTurnController.HasEnemyInCombat;
            bool turnHadEnemyAtStart = nativeTurnController.HadEnemyAtSomePoint;
            UnitEntityData[] turnUnitsAtStart = nativeTurnController.SortedUnits
                .ToArray();
            if (playerCombatAtStart || turnBasedCombatAtStart ||
                turnHasEnemyAtStart || protectedPartyCombatBefore.Any(pair =>
                    pair.Value))
                throw new InvalidOperationException(
                    "Astral core requires a clean native combat boundary.");
            var ownedItems = new List<ItemEntity>();
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;

                owner = CastExpandedSummoningOwnTier(fixture, "astral-deva");
                sibling = CastExpandedSummoningOwnTier(fixture, "astral-deva");
                foreach (UnitEntityData unit in new[] { owner, sibling })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                Vector3 origin = fixture.Caster.Position;
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, origin + Vector3.forward * 5f);
                PlaceExpandedSummoningUnit(sibling, origin + Vector3.forward * 14f);
                PlaceExpandedSummoningUnit(fixture.Caster, origin + Vector3.back * 12f);
                DateTime appearanceDeadline = DateTime.UtcNow.AddSeconds(10);
                int appearanceFrames = 0;
                while (DateTime.UtcNow < appearanceDeadline && appearanceFrames++ < 1800 &&
                    (owner.View == null || !EntityFadedIn(owner) ||
                     !owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                        .Any(value => value.enabled && value.sharedMesh != null)))
                    yield return 0;

                var auraAreaBlueprint = ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.Astral31Prefix + "ProtectiveArea"] as
                        BlueprintAbilityAreaEffect;
                var auraCarrierBlueprint = ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.Astral31Prefix + "ProtectiveCarrier"] as
                        BlueprintBuff;
                var auraRecipientBlueprint = ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.Astral31Prefix + "ProtectiveRecipient"] as
                        BlueprintBuff;
                if (auraAreaBlueprint == null || auraCarrierBlueprint == null ||
                    auraRecipientBlueprint == null)
                    throw new InvalidOperationException("Astral protective aura identities are unavailable.");
                var ownerAura = Sprint29OwnedAreas(owner, auraAreaBlueprint).Single();
                var siblingAura = Sprint29OwnedAreas(sibling, auraAreaBlueprint).Single();
                var coveredActors = new List<UnitEntityData> {
                    owner, sibling, target, fixture.Caster };
                Astral31RefreshAreas(new[] { ownerAura, siblingAura }, coveredActors);

                var auraCarrier = owner.Buffs.Enumerable.Single(buff =>
                    ReferenceEquals(buff.Blueprint, auraCarrierBlueprint) &&
                    ReferenceEquals(buff.Context?.MaybeCaster, owner));
                var auraRecipient = target.Buffs.Enumerable.Single(buff =>
                    ReferenceEquals(buff.Blueprint, auraRecipientBlueprint) &&
                    buff.SourceAreaEffectId == ownerAura.UniqueId);
                var circle = BlueprintBootstrap.MagicCircles.Single(value =>
                    value.Alignment == "Evil");
                var circleAreas = Sprint29OwnedAreas(owner, circle.Area);
                var circleCarriers = owner.Buffs.Enumerable.Where(buff =>
                    ReferenceEquals(buff.Blueprint, circle.Carrier) &&
                    ReferenceEquals(buff.Context?.MaybeCaster, owner)).ToArray();
                Astral31RefreshAreas(circleAreas, coveredActors);
                bool targetOutsideCircle = !target.Buffs.Enumerable.Any(buff =>
                    ReferenceEquals(buff.Blueprint, circle.Recipient) &&
                    ReferenceEquals(buff.Context?.MaybeCaster, owner));

                var stats = owner.Stats;
                var missile = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    BlueprintBootstrap.Library, FcbMagicMissileGuid,
                    "Astral real SR and globe Spell consumer");
                var srContext = new AbilityExecutionContext(
                    new AbilityData(missile, target.Descriptor),
                    new AbilityParams { CasterLevel = 1, SpellLevel = 1 },
                    new TargetWrapper(owner), null);
                RuleSpellResistanceCheck srRule = srContext.TriggerRule(
                    new RuleSpellResistanceCheck(srContext, owner));
                int sr = owner.Get<UnitPartSpellResistance>()?.GetValue(srContext) ?? 0;
                bool profile = stats.HitPoints.ModifiedValue == 172 &&
                    stats.BaseAttackBonus.ModifiedValue == 15 &&
                    stats.AC.ModifiedValue == 29 &&
                    stats.SaveFortitude.ModifiedValue == 16 &&
                    stats.SaveReflex.ModifiedValue == 13 &&
                    stats.SaveWill.ModifiedValue == 11 &&
                    stats.Speed.ModifiedValue == 50 &&
                    stats.SkillPerception.ModifiedValue == 26 &&
                    stats.SkillStealth.ModifiedValue == 22 &&
                    owner.Descriptor.Progression.CharacterLevel == 15 &&
                    owner.Descriptor.Alignment.Value == Alignment.NeutralGood &&
                    owner.Descriptor.State.HasConditionImmunity(UnitCondition.Petrified) &&
                    owner.Descriptor.State.CanAct && !owner.Descriptor.Spellbooks.Any() &&
                    sr == 25 && srRule.SpellResistance == 25 &&
                    srRule.HasSpellResistance && !srRule.TargetIsImmune;
                Sprint25Check("astral-core-native-profile", profile, new JObject {
                    ["owner"] = owner.UniqueId, ["hp"] = stats.HitPoints.ModifiedValue,
                    ["bab"] = stats.BaseAttackBonus.ModifiedValue,
                    ["ac"] = stats.AC.ModifiedValue,
                    ["fort"] = stats.SaveFortitude.ModifiedValue,
                    ["reflex"] = stats.SaveReflex.ModifiedValue,
                    ["will"] = stats.SaveWill.ModifiedValue,
                    ["speed"] = stats.Speed.ModifiedValue,
                    ["perception"] = stats.SkillPerception.ModifiedValue,
                    ["stealth"] = stats.SkillStealth.ModifiedValue,
                    ["racialHd"] = owner.Descriptor.Progression.CharacterLevel,
                    ["alignment"] = owner.Descriptor.Alignment.Value.ToString(),
                    ["petrificationConditionImmune"] = owner.Descriptor.State
                        .HasConditionImmunity(UnitCondition.Petrified),
                    ["sr"] = sr, ["ruleSr"] = srRule.SpellResistance,
                    ["srImmune"] = srRule.TargetIsImmune,
                    ["spellbooks"] = owner.Descriptor.Spellbooks.Count() },
                    "Actual adult racial bases, skills, saves, native SR/condition consumers and neutral-good identity remain distinct from casting level.");
                if (!profile) throw new InvalidOperationException(
                    "Astral native adult profile failed.");

                var grants = owner.Blueprint.AddFacts.SelectMany(fact =>
                    fact.ComponentsArray).OfType<ArchonConstantMagicCircle>().ToArray();
                var circleGrant = grants.Single(value => ReferenceEquals(value.Carrier, circle.Carrier));
                var protectiveGrant = grants.Single(value =>
                    ReferenceEquals(value.Carrier, auraCarrierBlueprint));
                bool auras = auraCarrier.Context != null &&
                    ReferenceEquals(auraCarrier.Context.MaybeCaster, owner) &&
                    auraCarrier.Context.Params.CasterLevel == 15 &&
                    ReferenceEquals(ownerAura.Context.MaybeCaster, owner) &&
                    ownerAura.Context.Params.CasterLevel == 15 &&
                    ReferenceEquals(auraRecipient.Context?.MaybeCaster, owner) &&
                    circleAreas.Length == 1 && circleCarriers.Length == 1 &&
                    grants.Length == 2 &&
                    circleGrant.CasterLevel == 15 &&
                    ReferenceEquals(circleGrant.OwningBlueprint, owner.Blueprint) &&
                    ReferenceEquals(circleGrant.Spell, circle.Spell) &&
                    protectiveGrant.CasterLevel == 15 &&
                    ReferenceEquals(protectiveGrant.OwningBlueprint, owner.Blueprint) &&
                    ReferenceEquals(protectiveGrant.Spell, circle.Spell) &&
                    targetOutsideCircle;
                Sprint25Check("astral-core-owned-auras", auras, new JObject {
                    ["owner"] = owner.UniqueId,
                    ["protectiveCarrier"] = auraCarrierBlueprint.AssetGuid,
                    ["protectiveArea"] = ownerAura.Blueprint.AssetGuid,
                    ["protectiveRecipient"] = auraRecipient.Blueprint.AssetGuid,
                    ["protectiveCarrierCl"] = auraCarrier.Context?.Params?.CasterLevel,
                    ["protectiveAreaCl"] = ownerAura.Context?.Params?.CasterLevel,
                    ["protectiveGrantCl"] = protectiveGrant.CasterLevel,
                    ["recipientSourceArea"] = auraRecipient.SourceAreaEffectId,
                    ["circleCarrier"] = circle.Carrier.AssetGuid,
                    ["circleArea"] = circle.Area.AssetGuid,
                    ["circleAreas"] = circleAreas.Length,
                    ["circleCl"] = circleAreas.FirstOrDefault()?.Context?.Params?.CasterLevel,
                    ["circleGrantCl"] = circleGrant.CasterLevel,
                    ["targetDistanceMeters"] = Vector3.Distance(owner.Position, target.Position),
                    ["targetOutsideTenFootCircle"] = targetOutsideCircle },
                    "The exact owner emits its separate20-foot CL15 angel aura while consuming the published canonical CL15 Magic Circle; the5m target receives only the larger aura.");
                if (!auras) throw new InvalidOperationException(
                    "Astral aura ownership/CL boundary failed.");

                var renderers = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .Where(value => value.sharedMesh != null).ToArray();
                bool finiteBody = renderers.Length > 0 && renderers.All(value =>
                    Astral31Finite(value.bounds.center) && Astral31Finite(value.bounds.size)) &&
                    renderers.Any(value => value.enabled && value.bounds.size.sqrMagnitude > .01f);
                JObject selection = Sprint22SelectionSurface(owner);
                string partyFrame = WriteExpandedSummoningPartyCameraCapture(owner,
                    _request.EvidenceDirectory, "s31-astral-deva-native-body-party.png");
                var donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(
                    BlueprintBootstrap.Library, "afe56099ff6046b40a359b7562c0424e",
                    "Astral native deva visual donor");
                bool presentation = owner.View != null && sibling.View != null &&
                    !ReferenceEquals(owner.Blueprint, donor) &&
                    !ReferenceEquals(owner.View, sibling.View) && EntityFadedIn(owner) &&
                    finiteBody && (bool)selection["selectionSurface"] &&
                    partyFrame.Contains("inFrame=true") &&
                    partyFrame.Contains("screenLit=true") &&
                    partyFrame.Contains("rendererEnabled=true") &&
                    partyFrame.Contains("faderVisible=true");
                Sprint25Check("astral-core-native-body", presentation, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                    ["blueprint"] = owner.Blueprint.AssetGuid,
                    ["donor"] = donor.AssetGuid,
                    ["separateUnitBlueprint"] = !ReferenceEquals(owner.Blueprint, donor),
                    ["separateViews"] = !ReferenceEquals(owner.View, sibling.View),
                    ["appearanceWaitFrames"] = appearanceFrames,
                    ["fadedIn"] = EntityFadedIn(owner), ["selection"] = selection,
                    ["partyCamera"] = partyFrame,
                    ["renderers"] = new JArray(renderers.Select(value => new JObject {
                        ["name"] = value.name, ["enabled"] = value.enabled,
                        ["mesh"] = value.sharedMesh.name,
                        ["center"] = new JArray(value.bounds.center.x, value.bounds.center.y,
                            value.bounds.center.z),
                        ["size"] = new JArray(value.bounds.size.x, value.bounds.size.y,
                            value.bounds.size.z) })) },
                    "The hidden Astral uses a separate live unit/view with an intact selectable native deva body at the party camera; no donor blueprint or raw asset is changed.");
                if (!presentation) throw new InvalidOperationException(
                    "Astral native body presentation failed.");

                var route = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Single(value => value.Creature.Key == "astral-deva");
                var routeAbility = (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                    .BySymbol[ExpandedSummoningIdentityCatalog.AbilitySymbol(route)];
                int publicReferences;
                bool publicExact = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(
                    BlueprintBootstrap.Library, true, out publicReferences);
                int choices = ExpandedSummoningInventoryObserver.CanonicalParentGuids.Select(guid =>
                    BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                        guid, "Astral hidden public surface").GetComponent<
                            Kingmaker.UnitLogic.Abilities.Components.AbilityVariants>()
                        .Variants.Length).Sum();
                bool hidden = route.ParentTier == 9 &&
                    route.Multiplicity == SummonMultiplicity.One &&
                    SummonVisibilityCatalog.IsPublished(route) && publicExact &&
                    publicReferences == SummonVisibilityCatalog.PublishedLogicalPlacementCount &&
                    choices == publicReferences +
                        ExpandedSummoningIdentityCatalog.NativeExpandedOptionIdentityCount &&
                    routeAbility.Icon != null && routeAbility.Icon.texture != null &&
                    routeAbility.Icon.texture.width == 128 && routeAbility.Icon.texture.height == 128;
                Sprint25Check("astral-core-hidden-route-and-icon", hidden, new JObject {
                    ["route"] = routeAbility.AssetGuid, ["tier"] = route.ParentTier,
                    ["multiplicity"] = route.Multiplicity.ToString(),
                    ["published"] = SummonVisibilityCatalog.IsPublished(route),
                    ["publicReferences"] = publicReferences, ["choices"] = choices,
                    ["iconWidth"] = routeAbility.Icon?.texture?.width,
                    ["iconHeight"] = routeAbility.Icon?.texture?.height },
                    "Since v0.0.167 exactly one Astral SM IX root is published, all released choices stay exact and the actual route resolves its 128px icon.");
                if (!hidden) throw new InvalidOperationException(
                    "Astral released route/public/icon contract failed.");

                ItemEntityWeapon hammer = owner.Body.PrimaryHand.MaybeWeapon;
                ownedItems.AddRange(owner.Descriptor.Inventory.Items);
                ownedItems.AddRange(sibling.Descriptor.Inventory.Items);
                var powerAttack = owner.Descriptor.ActivatableAbilities.Enumerable.Single(value =>
                    value.Blueprint.AssetGuid == "a7b339e4f6ff93a4697df5d7a87ff619");
                JObject initialPowerAttack = Sprint22PowerAttackState(owner, powerAttack);
                foreach (int frame in Sprint22SetPowerAttack(owner, false))
                    yield return frame;
                bool attacks = hammer != null && !hammer.IsLootable &&
                    hammer.Wielder != null && ReferenceEquals(hammer.Wielder.Unit, owner) &&
                    hammer.Enchantments.Count(value => value.Blueprint.AssetGuid ==
                        MagicFirearmBlueprints.Enhancement2Guid) == 1 &&
                    hammer.Enchantments.Count(value => value.Blueprint.AssetGuid ==
                        "0f20d79b7049c0f4ca54ca3d1ea44baa") == 1;
                var attackRows = new JArray();
                foreach (int mode in new[] { 0, 1 })
                {
                    if (owner.Body.CurrentHandEquipmentSetIndex != mode)
                    {
                        var change = new UnitSwitchHandEquipmentSet(mode);
                        owner.Commands.Run(change);
                        DateTime deadline = DateTime.UtcNow.AddSeconds(20);
                        int frames = 0;
                        while (!change.IsFinished && DateTime.UtcNow < deadline &&
                            frames++ < 1000) yield return 0;
                        if (!change.IsFinished ||
                            owner.Body.CurrentHandEquipmentSetIndex != mode)
                            throw new InvalidOperationException(
                                "Astral native weapon-set command failed.");
                    }
                    ItemEntityWeapon weapon = owner.Body.PrimaryHand.MaybeWeapon;
                    if (weapon != null && !ownedItems.Contains(weapon)) ownedItems.Add(weapon);
                    JObject offPowerAttack = Sprint22PowerAttackState(owner, powerAttack);
                    JObject offRule = Sprint29WeaponRule(owner, weapon);
                    foreach (int frame in Sprint22SetPowerAttack(owner, true))
                        yield return frame;
                    var onAttackRule = Rulebook.Trigger(
                        new RuleCalculateAttackBonusWithoutTarget(owner, weapon, 0));
                    JObject onRule = Sprint29WeaponRule(owner, weapon);
                    JObject onPowerAttack = Sprint22PowerAttackState(owner, powerAttack);
                    foreach (int frame in Sprint22SetPowerAttack(owner, false))
                        yield return frame;
                    JObject resetPowerAttack = Sprint22PowerAttackState(owner, powerAttack);
                    var attack = new UnitAttack(target);
                    attack.Init(owner);
                    var plan = attack.CreateFullAttack();
                    int expectedAttack = mode == 0 ? 26 : 23;
                    int expectedDamage = mode == 0 ? 14 : 12;
                    int expectedPowerDamage = mode == 0 ? 12 : 8;
                    int expectedCount = mode == 0 ? 3 : 1;
                    bool exact = weapon != null && !weapon.IsSecondary &&
                        weapon.AttackRange.Value == 5 && !weapon.IsLootable &&
                        weapon.Wielder != null && ReferenceEquals(weapon.Wielder.Unit, owner) &&
                        (int)offRule["attack"] == expectedAttack &&
                        (int)offRule["damageBonus"] == expectedDamage &&
                        (int)offRule["diceRolls"] == 1 && (string)offRule["dice"] == "D8" &&
                        (bool)offRule["ownerExact"] &&
                        (int)onRule["attack"] == expectedAttack - 4 &&
                        (int)onRule["damageBonus"] == expectedDamage + expectedPowerDamage &&
                        onAttackRule.BonusSources.Any(value => value.Bonus == -4 &&
                            ReferenceEquals(value.Source?.Blueprint, powerAttack.Blueprint.Buff)) &&
                        powerAttack.Blueprint.Buff != null &&
                        (bool)onPowerAttack["isOn"] && (bool)onPowerAttack["isRunning"] &&
                        (int)onPowerAttack["activeBuffCount"] == 1 &&
                        !(bool)offPowerAttack["isOn"] && !(bool)offPowerAttack["isRunning"] &&
                        (int)offPowerAttack["activeBuffCount"] == 0 &&
                        !(bool)resetPowerAttack["isOn"] && !(bool)resetPowerAttack["isRunning"] &&
                        (int)resetPowerAttack["activeBuffCount"] == 0 &&
                        plan.Count == expectedCount &&
                        plan.All(value => value.Hand != null &&
                            ReferenceEquals(value.Hand.MaybeWeapon, weapon));
                    attacks &= exact;
                    attackRows.Add(new JObject {
                        ["mode"] = mode, ["weapon"] = weapon?.Blueprint?.AssetGuid,
                        ["secondary"] = weapon?.IsSecondary,
                        ["lootable"] = weapon == null || weapon.IsLootable,
                        ["offRule"] = offRule, ["onRule"] = onRule,
                        ["offPowerAttack"] = offPowerAttack,
                        ["onPowerAttack"] = onPowerAttack,
                        ["resetPowerAttack"] = resetPowerAttack,
                        ["onAttackSources"] = new JArray(onAttackRule.BonusSources.Select(value =>
                            new JObject { ["bonus"] = value.Bonus,
                                ["source"] = value.Source?.Blueprint?.AssetGuid })),
                        ["planCount"] = plan.Count,
                        ["plan"] = new JArray(plan.Select(value => new JObject {
                            ["weapon"] = value.Hand?.MaybeWeapon?.Blueprint?.AssetGuid,
                            ["penalty"] = value.AttackBonusPenalty,
                            ["number"] = value.AttackNumber })) });
                }
                Sprint25Check("astral-core-owned-attack-modes", attacks,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["hammer"] = hammer?.Blueprint?.AssetGuid,
                        ["hammerEnchantments"] = new JArray(hammer == null ?
                            Enumerable.Empty<string>() : hammer.Enchantments.Select(value =>
                                value.Blueprint.AssetGuid)),
                        ["initialPowerAttack"] = initialPowerAttack,
                        ["finalPowerAttack"] = Sprint22PowerAttackState(owner, powerAttack),
                        ["modes"] = attackRows, ["resolvedAttacks"] = 0 },
                    "With native Power Attack explicitly controlled and source-attributed, item/rule/plan consumers expose the owned +2 disrupting two-handed hammer and alternative primary slam with the adult signed bonuses; resolved hits and disruption remain separate proof.");
                if (!attacks) throw new InvalidOperationException(
                    "Astral native attack-mode plan failed.");

                foreach (int frame in Astral31Defenses(owner, target, missile))
                    yield return frame;

                var reason = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    BlueprintBootstrap.Library, "d7cbd2004ce66a042aeab2e95a3c5c61",
                    "Astral alignment save reason");
                fixture.Caster.Descriptor.Alignment.Set(Alignment.LawfulGood);
                int goodAc = CircleAttackAC(fixture.Caster, target);
                int goodSave = CircleSave(fixture.Caster, target, reason);
                fixture.Caster.Descriptor.Alignment.Set(Alignment.LawfulEvil);
                int evilAc = CircleAttackAC(fixture.Caster, target);
                int evilSave = CircleSave(fixture.Caster, target, reason);
                bool protection = evilAc == goodAc + 4 && evilSave == goodSave + 4 &&
                    targetOutsideCircle && ReferenceEquals(auraRecipient.Context?.MaybeCaster,
                        owner);
                Sprint25Check("astral-core-protective-bonuses", protection,
                    new JObject { ["owner"] = owner.UniqueId,
                        ["target"] = target.UniqueId, ["goodSourceAc"] = goodAc,
                        ["evilSourceAc"] = evilAc, ["goodSourceWill"] = goodSave,
                        ["evilSourceWill"] = evilSave,
                        ["targetOutsideCircle"] = targetOutsideCircle,
                        ["recipientSource"] = auraRecipient.Context?.MaybeCaster?.UniqueId },
                    "Actual native AC and saving-throw rules gain exactly the Astral aura's +4 deflection/resistance against an evil source at5m, outside the shared10-foot circle.");
                if (!protection) throw new InvalidOperationException(
                    "Astral protective alignment consumers failed.");

                fixture.Caster.Descriptor.Alignment.Set(Alignment.NeutralGood);
                var aid = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                    "KMG.Summoning.OutsiderCelestial.HoundArchon.Aid"];
                var heal = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.Astral31Prefix + "Heal"];
                foreach (int frame in ReviewAstral31Globe(fixture, owner, sibling,
                    target, origin, aid, ownerAura, siblingAura,
                    auraRecipientBlueprint, coveredActors))
                    yield return frame;

                // The globe probes deliberately move the fixture caster to
                // distant rule origins. Restore the original walkable party
                // anchor before the established AI lane surveys the graph.
                PlaceExpandedSummoningUnit(fixture.Caster, origin);
                foreach (int frame in ReviewAstral31Heal(fixture, owner, sibling,
                    target, heal)) yield return frame;

                foreach (UnitEntityData unit in new[] { owner, sibling })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                    if (unit.IsInCombat) unit.LeaveCombat();
                    if (target.Memory.Contains(unit)) target.Memory.Remove(unit);
                    if (unit.Memory.Contains(target)) unit.Memory.Remove(target);
                    if (fixture.Caster.Memory.Contains(unit)) fixture.Caster.Memory.Remove(unit);
                    if (unit.Memory.Contains(fixture.Caster)) unit.Memory.Remove(fixture.Caster);
                }
                bool targetCombatBeforeCleanup = target.IsInCombat;
                bool casterCombatBeforeCleanup = fixture.Caster.IsInCombat;
                bool playerCombatBeforeCleanup = Game.Instance.Player.IsInCombat;
                bool casterRememberedTargetBeforeCleanup =
                    fixture.Caster.Memory.Contains(target);
                bool targetRememberedCasterBeforeCleanup =
                    target.Memory.Contains(fixture.Caster);
                if (casterRememberedTargetBeforeCleanup)
                    fixture.Caster.Memory.Remove(target);
                if (targetRememberedCasterBeforeCleanup)
                    target.Memory.Remove(fixture.Caster);
                int protectedPartyCombatBeforeCleanup =
                    protectedPartyCombatBefore.Keys.Count(unit => unit.IsInCombat);
                bool turnBasedCombatBeforeCleanup =
                    TurnBased.Controllers.CombatController.IsInTurnBasedCombat();
                bool turnHasEnemyBeforeCleanup =
                    nativeTurnController.HasEnemyInCombat;
                bool turnHadEnemyBeforeCleanup =
                    nativeTurnController.HadEnemyAtSomePoint;
                int turnUnitsBeforeCleanup = nativeTurnController.SortedUnits.Count();
                bool targetBuffsExactBeforeRetirement = !target.Buffs.Enumerable
                    .Except(targetBuffBaseline).Any();
                bool targetCommandsClearBeforeRetirement = !target.Commands.Raw
                    .Any(command => command != null && !command.IsFinished);
                bool targetMemoryExactBeforeRetirement =
                    !target.Memory.Contains(fixture.Caster);
                if (fixture.UnitsBefore.Any(value => ReferenceEquals(value, target)) ||
                    fixture.PartyBefore.Any(value => ReferenceEquals(value, target)))
                    throw new InvalidOperationException(
                        "Astral cleanup refused to retire a protected hostile.");
                if (target.IsInCombat) target.LeaveCombat();
                if (fixture.Caster.IsInCombat) fixture.Caster.LeaveCombat();
                fixture.Caster.Descriptor.Alignment.Set(casterAlignment);
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling });
                ResetExpandedSummoningHostile(fixture);
                if (!target.Destroyed) target.Destroy();
                Game.Instance.EntityDestroyer.Tick();
                int ownedAreasBeforeSettle = Game.Instance.State.AreaEffects.All
                    .Except(areaBaseline).Count();
                int cleanupFrames = 0;
                DateTime cleanupDeadline = DateTime.UtcNow.AddSeconds(5);
                while (DateTime.UtcNow < cleanupDeadline && cleanupFrames < 120 &&
                    Game.Instance.State.AreaEffects.All.Except(areaBaseline).Any())
                {
                    cleanupFrames++;
                    yield return 0;
                }
                Game.Instance.EntityDestroyer.Tick();
                Game.Instance.Player.UpdateIsInCombat();
                bool targetRetired = target.Destroyed && !target.IsInState;
                bool areasExact = !Game.Instance.State.AreaEffects.All
                    .Except(areaBaseline).Any();
                bool protectedPartyCombatExact = protectedPartyCombatBefore
                    .All(pair => pair.Key.IsInCombat == pair.Value);
                bool nativeCombatBoundaryExact =
                    Game.Instance.Player.IsInCombat == playerCombatAtStart &&
                    TurnBased.Controllers.CombatController.IsInTurnBasedCombat() ==
                        turnBasedCombatAtStart &&
                    nativeTurnController.HasEnemyInCombat == turnHasEnemyAtStart &&
                    nativeTurnController.HadEnemyAtSomePoint == turnHadEnemyAtStart &&
                    turnUnitsAtStart.SequenceEqual(nativeTurnController.SortedUnits);
                bool sharedNextCaseAllowed = !fixture.Caster.IsInCombat &&
                    protectedPartyCombatExact && nativeCombatBoundaryExact;
                bool freshProcessBoundaryRequired = !sharedNextCaseAllowed;
                bool ownedCleanup = owner.Destroyed && sibling.Destroyed &&
                    !owner.IsInState && !sibling.IsInState &&
                    areasExact &&
                    targetBuffsExactBeforeRetirement &&
                    targetCommandsClearBeforeRetirement &&
                    targetMemoryExactBeforeRetirement && targetRetired &&
                    !fixture.Caster.Commands.Raw.Any(command => command != null && !command.IsFinished) &&
                    !fixture.Caster.Memory.Contains(target) &&
                    !target.Memory.Contains(fixture.Caster) &&
                    ownedItems.Count >= 4 && ownedItems.All(item =>
                        !item.IsLootable && item.Collection == null);
                Sprint25Check("astral-core-cleanup", ownedCleanup, new JObject {
                    ["ownerAbsent"] = !owner.IsInState,
                    ["siblingAbsent"] = !sibling.IsInState,
                    ["areasExact"] = areasExact,
                    ["ownedAreasBeforeSettle"] = ownedAreasBeforeSettle,
                    ["ownedAreasAfterSettle"] = Game.Instance.State.AreaEffects.All
                        .Except(areaBaseline).Count(),
                    ["cleanupFrames"] = cleanupFrames,
                    ["targetBuffsExact"] = targetBuffsExactBeforeRetirement,
                    ["targetCommandsClearBeforeRetirement"] =
                        targetCommandsClearBeforeRetirement,
                    ["targetMemoryExactBeforeRetirement"] =
                        targetMemoryExactBeforeRetirement,
                    ["targetRetired"] = targetRetired,
                    ["items"] = new JArray(ownedItems.Select(item => new JObject {
                        ["blueprint"] = item.Blueprint.AssetGuid,
                        ["lootable"] = item.IsLootable,
                        ["collectionAbsent"] = item.Collection == null })),
                    ["casterCommandsClear"] = !fixture.Caster.Commands.Raw.Any(command =>
                        command != null && !command.IsFinished),
                    ["targetCombatBeforeCleanup"] = targetCombatBeforeCleanup,
                    ["casterCombatBeforeCleanup"] = casterCombatBeforeCleanup,
                    ["playerCombatBeforeCleanup"] = playerCombatBeforeCleanup,
                    ["casterRememberedTargetBeforeCleanup"] =
                        casterRememberedTargetBeforeCleanup,
                    ["targetRememberedCasterBeforeCleanup"] =
                        targetRememberedCasterBeforeCleanup,
                    ["targetCombatAfterCleanup"] = target.IsInCombat,
                    ["casterCombatAfterCleanup"] = fixture.Caster.IsInCombat,
                    ["playerCombatAfterCleanup"] = Game.Instance.Player.IsInCombat,
                    ["protectedPartyCombatBeforeCleanup"] =
                        protectedPartyCombatBeforeCleanup,
                    ["protectedPartyCombatAfterCleanup"] =
                        protectedPartyCombatBefore.Keys.Count(unit => unit.IsInCombat),
                    ["protectedPartyCombatExact"] = protectedPartyCombatExact,
                    ["turnBasedCombatBeforeCleanup"] = turnBasedCombatBeforeCleanup,
                    ["turnBasedCombatAfterCleanup"] =
                        TurnBased.Controllers.CombatController.IsInTurnBasedCombat(),
                    ["turnHasEnemyBeforeCleanup"] = turnHasEnemyBeforeCleanup,
                    ["turnHasEnemyAfterCleanup"] =
                        nativeTurnController.HasEnemyInCombat,
                    ["turnHadEnemyBeforeCleanup"] = turnHadEnemyBeforeCleanup,
                    ["turnHadEnemyAfterCleanup"] =
                        nativeTurnController.HadEnemyAtSomePoint,
                    ["turnUnitsBeforeCleanup"] = turnUnitsBeforeCleanup,
                    ["turnUnitsAfterCleanup"] =
                        nativeTurnController.SortedUnits.Count(),
                    ["nativeCombatBoundaryExact"] = nativeCombatBoundaryExact,
                    ["sharedNextCaseAllowed"] = sharedNextCaseAllowed,
                    ["freshProcessBoundaryRequired"] = freshProcessBoundaryRequired,
                    ["phaseEndsAfterCleanup"] = true,
                    ["casterRemembersTargetAfterCleanup"] =
                        fixture.Caster.Memory.Contains(target),
                    ["targetRemembersCasterAfterCleanup"] =
                        target.Memory.Contains(fixture.Caster) },
                    "Both exact Astral owners and the disposable hostile are retired with owned areas, grants, commands, memory and items clean; any protected combat residue requires immediate fixture and fresh-process restoration before another case.");
                if (!ownedCleanup) throw new InvalidOperationException(
                    "Astral owned cleanup failed.");
            }
            finally
            {
                fixture.Caster.Descriptor.Alignment.Set(casterAlignment);
                foreach (UnitEntityData unit in new[] { owner, sibling }.Where(value =>
                    value != null && !value.Destroyed))
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    if (unit.IsInCombat) unit.LeaveCombat();
                }
                DisposeExpandedSummoningUnits(fixture.Created,
                    new[] { owner, sibling }.Where(value => value != null &&
                        !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private IEnumerable<int> Astral31Defenses(UnitEntityData owner,
            UnitEntityData source, BlueprintAbility neutralAbility)
        {
            int hitPoints = owner.Stats.HitPoints.BaseValue;
            int injury = owner.Damage;
            Alignment alignment = source.Descriptor.Alignment.Value;
            var rows = new JArray();
            bool exact = true;
            try
            {
                owner.Stats.HitPoints.BaseValue = 10000;
                foreach (string bypass in new[] { "mundane", "evil", "good" })
                {
                    var damage = new PhysicalDamage(new DiceFormula(0, DiceType.Zero),
                        PhysicalDamageForm.Slashing);
                    damage.AddBonus(20);
                    if (bypass == "evil") damage.AddAlignment(DamageAlignment.Evil);
                    if (bypass == "good") damage.AddAlignment(DamageAlignment.Good);
                    int expected = bypass == "evil" ? 20 : 10;
                    JObject row = Astral31Damage(owner, source, damage, expected, false,
                        "physical-" + bypass);
                    rows.Add(row); exact &= (bool)row["exact"];
                }
                foreach (DamageEnergyType energy in new[] { DamageEnergyType.Acid,
                    DamageEnergyType.Cold, DamageEnergyType.Electricity,
                    DamageEnergyType.Fire, DamageEnergyType.Sonic })
                {
                    var damage = new EnergyDamage(new DiceFormula(0, DiceType.Zero),
                        energy);
                    damage.AddBonus(20);
                    bool immune = energy == DamageEnergyType.Acid ||
                        energy == DamageEnergyType.Cold;
                    int expected = energy == DamageEnergyType.Electricity ||
                        energy == DamageEnergyType.Fire ? 10 : 20;
                    JObject row = Astral31Damage(owner, source, damage, expected, immune,
                        "energy-" + energy);
                    rows.Add(row); exact &= (bool)row["exact"];
                }
                source.Descriptor.Alignment.Set(Alignment.NeutralGood);
                var poison = (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                    .BySymbol["KMG.Summoning.OutsiderCelestial.Dretch.StinkingCloud"];
                int ordinary = Astral31SaveValue(source, owner, neutralAbility);
                int poisonSave = Astral31SaveValue(source, owner, poison);
                bool poisonExact = poisonSave == ordinary + 4;
                exact &= poisonExact;
                Sprint25Check("astral-core-defenses", exact, new JObject {
                    ["owner"] = owner.UniqueId, ["source"] = source.UniqueId,
                    ["damage"] = rows, ["ordinaryFortitude"] = ordinary,
                    ["poisonFortitude"] = poisonSave,
                    ["poisonDescriptor"] = poison.GetComponent<
                        SpellDescriptorComponent>()?.Descriptor.ToString(),
                    ["petrificationConditionImmune"] = owner.Descriptor.State
                        .HasConditionImmunity(UnitCondition.Petrified) },
                    "Actual attributed native damage and save rules independently consume DR10/evil, acid/cold immunity, electricity/fire resistance10, sonic control and the +4 poison save.");
                if (!exact) throw new InvalidOperationException(
                    "Astral native defenses failed.");
            }
            finally
            {
                source.Descriptor.Alignment.Set(alignment);
                owner.Damage = injury;
                owner.Stats.HitPoints.BaseValue = hitPoints;
            }
            for (int frame = 0; frame < 3; frame++) yield return 0;
        }

        private static JObject Astral31Damage(UnitEntityData owner,
            UnitEntityData source, BaseDamage packet, int expectedBeforeDifficulty,
            bool expectedImmune, string id)
        {
            owner.Damage = 0;
            var rule = Rulebook.Trigger(new RuleDealDamage(source, owner,
                new DamageBundle(packet)));
            var result = rule.ResultDamage?.SingleOrDefault(value =>
                ReferenceEquals(value.Source, packet));
            bool exact = result.HasValue &&
                result.Value.FinalValue == expectedBeforeDifficulty &&
                packet.Immune == expectedImmune && rule.Damage == owner.Damage &&
                (expectedImmune ? rule.Damage == 0 : rule.Damage > 0) &&
                !owner.Descriptor.State.IsDead;
            return new JObject { ["id"] = id, ["input"] = 20,
                ["expectedBeforeDifficulty"] = expectedBeforeDifficulty,
                ["expectedImmune"] = expectedImmune,
                ["finalBeforeDifficulty"] = result?.FinalValue,
                ["nativeDamage"] = rule.Damage, ["hpDelta"] = owner.Damage,
                ["nativeImmune"] = packet.Immune,
                ["packet"] = packet.GetType().FullName, ["exact"] = exact };
        }

        private static int Astral31SaveValue(UnitEntityData source,
            UnitEntityData target, BlueprintAbility ability)
        {
            var context = new MechanicsContext(source, source.Descriptor, ability,
                null, new TargetWrapper(target));
            var rule = new RuleSavingThrow(target, SavingThrowType.Fortitude, 100) {
                Reason = context };
            context.TriggerRule(rule);
            return rule.StatValue;
        }

        private static RuleSpellResistanceCheck Astral31Resistance(
            UnitEntityData source, UnitEntityData target, BlueprintAbility ability,
            int casterLevel, int spellLevel)
        {
            var granted = source.Descriptor.Abilities.GetAbility(ability);
            var data = granted == null ? new AbilityData(ability, source.Descriptor) :
                new AbilityData(granted);
            var context = new AbilityExecutionContext(data, new AbilityParams {
                CasterLevel = casterLevel, SpellLevel = spellLevel },
                new TargetWrapper(target), null);
            return context.TriggerRule(new RuleSpellResistanceCheck(context, target));
        }

        private static JObject Astral31ResistanceRow(
            RuleSpellResistanceCheck rule)
        {
            return new JObject { ["source"] = rule.Context?.MaybeCaster?.UniqueId,
                ["target"] = rule.Target?.UniqueId,
                ["ability"] = rule.Context?.SourceAbility?.AssetGuid,
                ["type"] = rule.Context?.SourceAbility?.Type.ToString(),
                ["cl"] = rule.Context?.Params?.CasterLevel,
                ["spellLevel"] = rule.Context?.Params?.SpellLevel,
                ["immune"] = rule.TargetIsImmune,
                ["sr"] = rule.SpellResistance,
                ["resisted"] = rule.IsSpellResisted };
        }

        // The lesser globe through its actual native consumers, never a spell
        // resistance rule. Each direct case runs a real spell or SLA effect
        // component through the private AbilityExecutionProcess.DoApplyEffect;
        // one real persistent spell area runs the native membership update;
        // a second Astral's constant aura shows that supernatural circles,
        // which carry a Magic Circle spell context, are not mistaken for casts.
        private IEnumerable<int> ReviewAstral31Globe(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            UnitEntityData sibling, UnitEntityData target, Vector3 origin,
            BlueprintAbility aidSla, AreaEffectEntityData ownerAura,
            AreaEffectEntityData siblingAura, BlueprintBuff recipient,
            IList<UnitEntityData> actors)
        {
            BlueprintAbility[] mageArmor = BlueprintBootstrap.Library
                .GetAllBlueprints().OfType<BlueprintAbility>()
                .Where(value => value != null && value.Type == AbilityType.Spell &&
                    value.Parent == null && string.Equals(value.name, "MageArmor",
                        StringComparison.Ordinal)).ToArray();
            if (mageArmor.Length != 1)
                throw new InvalidOperationException(
                    "The exact native Mage Armor globe probe is ambiguous: " +
                    mageArmor.Length + ".");
            // Native buff spells carry SpellResistance false. Magic Missile
            // carries it true; at CL50 it always overcomes the Astral's SR 25,
            // so against the covered Astral only the globe can stop it.
            BlueprintAbility noSr = mageArmor[0];
            BlueprintAbility withSr = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                BlueprintBootstrap.Library, FcbMagicMissileGuid,
                "native Magic Missile globe probe");
            var mist = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                BlueprintBootstrap.Library, "15ba44b3fcf48ce418dc72fadac6408a",
                "native Obscuring Mist globe area probe");
            var mistSpawn = ExpandedSummoningSpecialBuilder.FindSpawnAreaEffect(mist);
            if (noSr.SpellResistance || !withSr.SpellResistance || mist.SpellResistance ||
                mistSpawn == null || mistSpawn.AreaEffect == null ||
                mistSpawn.AreaEffect.SpellResistance ||
                mistSpawn.AreaEffect.Size.Meters < 3.6f)
                throw new InvalidOperationException(
                    "Astral globe native spell inputs are unavailable: " +
                        noSr.name + ":" + noSr.SpellResistance + ";" +
                        withSr.name + ":" + withSr.SpellResistance +
                        ";mist=" + mist.SpellResistance + "/" +
                    (mistSpawn?.AreaEffect == null ? "none" :
                        mistSpawn.AreaEffect.SpellResistance + "/" +
                        mistSpawn.AreaEffect.Size.Meters));
            var details = new JObject { ["owner"] = owner.UniqueId,
                ["target"] = target.UniqueId, ["directRuleChecksNoFiniteUses"] = true,
                ["spellResistanceRulesConsulted"] = false };
            bool exact = true;
            Action<string, JObject> direct = (id, row) => {
                details[id] = row; exact &= (bool)row["exact"]; };

            PlaceExpandedSummoningUnit(fixture.Caster, origin + Vector3.back * 12f);
            direct("outsideSpellNoSr", Astral31Dispatch(fixture.Caster, target, noSr, 1, 1, false));
            direct("outsideSpellWithSr", Astral31Dispatch(fixture.Caster, owner, withSr, 50, 1, false));
            direct("outsideSla", Astral31Dispatch(fixture.Caster, target, aidSla, 6, 2, false));
            direct("outsideHeightenedLevel4", Astral31Dispatch(fixture.Caster, target, noSr, 7, 4, true));
            PlaceExpandedSummoningUnit(fixture.Caster, origin + Vector3.right);
            direct("insideSpellNoSr", Astral31Dispatch(fixture.Caster, target, noSr, 1, 1, true));
            direct("insideSpellWithSr", Astral31Dispatch(fixture.Caster, owner, withSr, 50, 1, true));
            PlaceExpandedSummoningUnit(fixture.Caster, origin + Vector3.back * 20f);
            direct("uncoveredTarget", Astral31Dispatch(sibling, fixture.Caster, noSr, 1, 1, true));

            // One real level-1 persistent area without spell resistance, cast
            // from outside the aura and centred to reach the covered target.
            Vector3 targetHome = target.Position;
            Vector3 siblingHome = sibling.Position;
            Vector3 coveredSpot = origin + Vector3.back * 3f;
            Vector3 centre = origin + Vector3.back * 6.25f;
            PlaceExpandedSummoningUnit(target, coveredSpot);
            PlaceExpandedSummoningUnit(fixture.Caster, origin + Vector3.back * 9.5f);
            for (int frame = 0; frame < 3; frame++) yield return 0;
            Astral31RefreshAreas(new[] { ownerAura, siblingAura }, actors);
            var areasBefore = Game.Instance.State.AreaEffects.All.ToArray();
            var point = new TargetWrapper(centre);
            Astral31InvokeDispatch(Astral31Context(fixture.Caster, mist, 1, 1, point),
                point, mist.GetComponent<AbilityApplyEffect>());
            Game.Instance.EntityCreator.Tick();
            AreaEffectEntityData[] spawned = Game.Instance.State.AreaEffects.All
                .Except(areasBefore).ToArray();
            AreaEffectEntityData mistArea = spawned.SingleOrDefault(value =>
                ReferenceEquals(value.Blueprint, mistSpawn.AreaEffect));
            bool areaExact = false;
            var areaRow = new JObject { ["ability"] = mist.AssetGuid,
                ["area"] = mistSpawn.AreaEffect.AssetGuid,
                ["areaSpellResistance"] = mistSpawn.AreaEffect.SpellResistance,
                ["radiusMeters"] = mistSpawn.AreaEffect.Size.Meters,
                ["spawned"] = spawned.Length, ["caster"] = fixture.Caster.UniqueId };
            try
            {
                if (mistArea == null || spawned.Length != 1)
                    throw new InvalidOperationException(
                        "The native Obscuring Mist probe area did not spawn exactly once.");
                Astral31RefreshAreas(new[] { ownerAura, mistArea }, actors);
                bool covered = Astral31Covered(target, ownerAura, recipient);
                bool excluded = covered && !mistArea.UnitsInside.Contains(target) &&
                    mistArea.UnitsInside.Contains(fixture.Caster);
                areaRow["coveredExcluded"] = Astral31AreaRow(mistArea, target,
                    fixture.Caster, covered);
                PlaceExpandedSummoningUnit(target, origin + Vector3.back * 8f);
                for (int frame = 0; frame < 3; frame++) yield return 0;
                Astral31RefreshAreas(new[] { ownerAura, mistArea }, actors);
                bool leftCovered = Astral31Covered(target, ownerAura, recipient);
                bool returned = !leftCovered && mistArea.UnitsInside.Contains(target);
                areaRow["leftGlobeIncluded"] = Astral31AreaRow(mistArea, target,
                    fixture.Caster, leftCovered);
                PlaceExpandedSummoningUnit(target, coveredSpot);
                for (int frame = 0; frame < 3; frame++) yield return 0;
                Astral31RefreshAreas(new[] { ownerAura, mistArea }, actors);
                bool coveredAgain = Astral31Covered(target, ownerAura, recipient);
                bool suppressedAgain = coveredAgain && !mistArea.UnitsInside.Contains(target);
                areaRow["reenteredExcluded"] = Astral31AreaRow(mistArea, target,
                    fixture.Caster, coveredAgain);
                areaExact = excluded && returned && suppressedAgain;
            }
            finally
            {
                if (mistArea != null && !mistArea.Destroyed) mistArea.ForceEnd();
            }
            int endFrames = 0;
            while (mistArea != null && endFrames++ < 240 &&
                Game.Instance.State.AreaEffects.All.Contains(mistArea))
                yield return 0;
            bool mistGone = mistArea == null ||
                !Game.Instance.State.AreaEffects.All.Contains(mistArea);
            var lingering = actors.Where(unit => unit != null && unit.Buffs.Enumerable.Any(buff =>
                ReferenceEquals(buff.Context?.SourceAbility, mist))).ToArray();
            areaRow["endFrames"] = endFrames; areaRow["areaEnded"] = mistGone;
            areaRow["lingeringMistBuffs"] = new JArray(lingering.Select(unit => unit.UniqueId));
            areaRow["exact"] = areaExact && mistGone && lingering.Length == 0;
            details["persistentAreaNoSr"] = areaRow;
            exact &= areaExact && mistGone && lingering.Length == 0;

            // A second Astral outside this aura still extends its own constant
            // supernatural aura over the shared creature.
            PlaceExpandedSummoningUnit(sibling, origin + Vector3.back * 11f);
            PlaceExpandedSummoningUnit(target, origin + Vector3.back * 5.5f);
            PlaceExpandedSummoningUnit(fixture.Caster, origin + Vector3.right * 12f);
            for (int frame = 0; frame < 3; frame++) yield return 0;
            Astral31RefreshAreas(new[] { ownerAura, siblingAura }, actors);
            bool ownerCovers = Astral31Covered(target, ownerAura, recipient);
            bool siblingCovers = Astral31Covered(target, siblingAura, recipient);
            details["constantAuraNotACast"] = new JObject {
                ["sibling"] = sibling.UniqueId,
                ["siblingToOwnerMeters"] = Vector3.Distance(sibling.Position, owner.Position),
                ["ownerCovers"] = ownerCovers, ["siblingCovers"] = siblingCovers,
                ["exact"] = ownerCovers && siblingCovers };
            exact &= ownerCovers && siblingCovers;

            PlaceExpandedSummoningUnit(sibling, siblingHome);
            PlaceExpandedSummoningUnit(target, targetHome);
            for (int frame = 0; frame < 3; frame++) yield return 0;
            Astral31RefreshAreas(new[] { ownerAura, siblingAura }, actors);
            Sprint25Check("astral-core-lesser-globe", exact, details,
                "Through the actual native effect dispatch, level 1 spells with and without spell resistance (Magic Missile at CL50, which always overcomes the Astral's SR 25) and a level 2 SLA cast from outside the aura never reach the covered creature, while the same spells cast from inside, the buff heightened to level 4, or a cast at an uncovered creature apply. A real level 1 persistent area without spell resistance excludes the covered creature, includes it once it leaves the globe and excludes it again on return, then ends without residue. A second Astral's constant aura still covers the shared creature.");
            if (!exact) throw new InvalidOperationException(
                "Astral lesser-globe native consumers failed.");
        }

        private static readonly System.Reflection.MethodInfo Astral31DoApplyEffect =
            typeof(Kingmaker.Controllers.AbilityExecutionProcess).GetMethod("DoApplyEffect",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        private static AbilityExecutionContext Astral31Context(UnitEntityData source,
            BlueprintAbility ability, int casterLevel, int spellLevel, TargetWrapper target)
        {
            var granted = source.Descriptor.Abilities.GetAbility(ability);
            var data = granted == null ? new AbilityData(ability, source.Descriptor) :
                new AbilityData(granted);
            return new AbilityExecutionContext(data, new AbilityParams {
                CasterLevel = casterLevel, SpellLevel = spellLevel }, target, null);
        }

        private static void Astral31InvokeDispatch(AbilityExecutionContext context,
            TargetWrapper target, AbilityApplyEffect effect)
        {
            if (Astral31DoApplyEffect == null)
                throw new MissingMethodException(
                    "Kingmaker.Controllers.AbilityExecutionProcess", "DoApplyEffect");
            if (effect == null || context.AbilityBlueprint.StickyTouch != null)
                throw new InvalidOperationException(
                    "Astral globe probe needs one directly applied native effect: " +
                    context.AbilityBlueprint.name);
            Astral31DoApplyEffect.Invoke(null, new object[] { context, target, effect });
        }

        private static JObject Astral31Dispatch(UnitEntityData source,
            UnitEntityData target, BlueprintAbility ability, int casterLevel,
            int spellLevel, bool expectApplied)
        {
            var wrapper = new TargetWrapper(target);
            var context = Astral31Context(source, ability, casterLevel, spellLevel, wrapper);
            var before = target.Buffs.Enumerable.ToArray();
            int damageBefore = target.Damage;
            bool blocks = AstralProtectiveGlobe.Blocks(target, context);
            Astral31InvokeDispatch(context, wrapper, ability.GetComponent<AbilityApplyEffect>());
            var added = target.Buffs.Enumerable.Except(before).ToArray();
            var addedGuids = new JArray(added.Select(buff => buff.Blueprint.AssetGuid));
            int damageDelta = target.Damage - damageBefore;
            foreach (var buff in added) buff.Remove();
            target.Damage = damageBefore;
            bool removed = !target.Buffs.Enumerable.Intersect(added).Any() &&
                target.Damage == damageBefore;
            bool applied = added.Length > 0 || damageDelta > 0;
            return new JObject { ["source"] = source.UniqueId, ["target"] = target.UniqueId,
                ["ability"] = ability.AssetGuid, ["name"] = ability.name,
                ["type"] = ability.Type.ToString(), ["spellResistance"] = ability.SpellResistance,
                ["casterLevel"] = casterLevel, ["spellLevel"] = spellLevel,
                ["sourcePosition"] = new JArray(source.Position.x, source.Position.y, source.Position.z),
                ["globeBlocks"] = blocks, ["applied"] = applied, ["addedBuffs"] = addedGuids,
                ["damageDelta"] = damageDelta,
                ["probeBuffsRemoved"] = removed, ["expectedApplied"] = expectApplied,
                ["exact"] = applied == expectApplied && blocks == !expectApplied && removed };
        }

        private static bool Astral31Covered(UnitEntityData unit,
            AreaEffectEntityData aura, BlueprintBuff recipient)
        {
            return unit.Buffs.Enumerable.Any(buff => ReferenceEquals(buff.Blueprint, recipient) &&
                buff.SourceAreaEffectId == aura.UniqueId);
        }

        private static JObject Astral31AreaRow(AreaEffectEntityData area,
            UnitEntityData target, UnitEntityData uncovered, bool targetCovered)
        {
            return new JObject { ["targetCovered"] = targetCovered,
                ["targetInside"] = area.UnitsInside.Contains(target),
                ["uncoveredCasterInside"] = area.UnitsInside.Contains(uncovered),
                ["targetToCentreMeters"] = Vector3.Distance(target.Position, area.Position),
                ["inside"] = new JArray(area.UnitsInside.Select(value => value.UniqueId)) };
        }

        private IEnumerable<int> ReviewAstral31Heal(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner,
            UnitEntityData sibling, UnitEntityData target, BlueprintAbility heal)
        {
            var resource = (BlueprintAbilityResource)ExpandedSummoningBlueprints
                .Registered.BySymbol[ExpandedSummoningSpecialBuilder.Astral31Prefix +
                    "HealResource"];
            var granted = owner.Descriptor.Abilities.GetAbility(heal);
            var parameters = new AbilityData(granted).CalculateParams();
            int ownerBefore = owner.Descriptor.Resources.GetResourceAmount(resource);
            int siblingBefore = sibling.Descriptor.Resources.GetResourceAmount(resource);
            Vector3 far; string lane, direction;
            Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f,
                out far, out lane, out direction, new Feet(20).Meters + 1f);
            PlaceExpandedSummoningUnit(owner, origin);
            PlaceExpandedSummoningUnit(sibling, origin + Vector3.right * 2f);
            PlaceExpandedSummoningUnit(target, far);
            owner.Damage = 100;
            for (int frame = 0; frame < 8; frame++) yield return 0;
            foreach (UnitEntityData unit in new[] { owner, target })
            {
                unit.Wake(0f);
                if (!Game.Instance.State.AwakeUnits.Contains(unit))
                    Game.Instance.State.AwakeUnits.Add(unit);
            }
            owner.Memory.Add(target); target.Memory.Add(owner);
            owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
            owner.JoinCombat(); target.JoinCombat();
            Game.Instance.Player.UpdateIsInCombat();
            Sprint28SetBrainActivePreservingActions(owner, true);
            owner.Brain.RestoreAvailableActions();
            var commands = new HashSet<UnitCommand>();
            var ambient = new HashSet<string>();
            var costs = new JArray();
            using (var economy = new FirearmReviewCosts(owner, costs))
            {
                DateTime deadline = DateTime.UtcNow.AddSeconds(30);
                int frames = 0, ticks = 0;
                while (DateTime.UtcNow < deadline && frames++ < 3000)
                {
                    if (frames == 1 || frames % 60 == 0 &&
                        !owner.Commands.Raw.Any(value => value != null &&
                            !value.IsFinished))
                    {
                        foreach (string candidate in Sprint24TickExactAi(owner))
                            if (candidate != owner.UniqueId) ambient.Add(candidate);
                        ticks++;
                    }
                    foreach (UnitCommand command in owner.Commands.Raw.Where(value =>
                        value != null)) commands.Add(command);
                    if (commands.OfType<UnitUseAbility>().Any(value =>
                        value.AiAction?.name ==
                            "KMG_Summoning_OutsiderCelestial_AstralDeva_HealAi" &&
                        value.IsFinished && value.ExecutionProcess != null &&
                        value.ExecutionProcess.IsEnded)) break;
                    Game.Instance.IsPaused = false;
                    yield return 0;
                }
                var cast = commands.OfType<UnitUseAbility>().FirstOrDefault(value =>
                    value.AiAction?.name ==
                        "KMG_Summoning_OutsiderCelestial_AstralDeva_HealAi" &&
                    value.IsFinished && value.Result == UnitCommand.ResultType.Success &&
                    value.ExecutionProcess != null && value.ExecutionProcess.IsEnded);
                int ownerAfter = owner.Descriptor.Resources.GetResourceAmount(resource);
                int siblingAfter = sibling.Descriptor.Resources.GetResourceAmount(resource);
                bool healed = granted != null && parameters.CasterLevel == 13 &&
                    parameters.SpellLevel == 6 && ownerBefore == 1 && siblingBefore == 1 &&
                    cast != null && !cast.Cutscene && !cast.IsIgnoreCooldown &&
                    economy.Count(cast) == 1 && economy.Exact(cast, false) &&
                    owner.Damage == 0 && ownerAfter == 0 && siblingAfter == 1;
                Sprint25Check("astral-core-heal-ai", healed, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                    ["hostile"] = target.UniqueId, ["lane"] = lane,
                    ["cl"] = parameters.CasterLevel,
                    ["spellLevel"] = parameters.SpellLevel,
                    ["dc"] = parameters.DC, ["resource"] = resource.AssetGuid,
                    ["ownerResource"] = ownerBefore + "->" + ownerAfter,
                    ["siblingResource"] = siblingBefore + "->" + siblingAfter,
                    ["fixtureInjury"] = 100, ["damageAfter"] = owner.Damage,
                    ["aiTicks"] = ticks,
                    ["commands"] = new JArray(commands.Select(Sprint27AiCommand)),
                    ["costs"] = costs,
                    ["ambientCandidatesNeverTicked"] = new JArray(ambient),
                    ["fixtureCastCommands"] = 0, ["forcedRolls"] = 0 },
                    "Actual native AI selects the one-use CL13 Heal on its injured living owner, spends one standard action and only its own resource; sibling remains independent.");
                if (!healed) throw new InvalidOperationException(
                    "Astral native Heal AI/resource path failed.");
            }
            Sprint28SetBrainActivePreservingActions(owner, false);
            var eligibility = fixture.Blueprints.OfType<AstralHealingEligibility>()
                .Single(value => ReferenceEquals(value.Heal, heal));
            float score = eligibility.Score(new Kingmaker.Controllers.Brain.DecisionContext {
                Unit = owner,
                Target = new Kingmaker.Controllers.Brain.TargetInfo { Unit = owner } });
            var traits = owner.Blueprint.AddFacts.Single(fact => fact.name ==
                "KMG_Summoning_OutsiderCelestial_AstralDeva_Traits");
            owner.Descriptor.GetFact(traits).Recalculate();
            bool retained = score == 0f &&
                owner.Descriptor.Resources.GetResourceAmount(resource) == 0 &&
                sibling.Descriptor.Resources.GetResourceAmount(resource) == 1;
            Sprint25Check("astral-core-heal-no-refresh", retained, new JObject {
                ["scoreOnHealthyTarget"] = score,
                ["ownerResource"] = owner.Descriptor.Resources.GetResourceAmount(resource),
                ["siblingResource"] = sibling.Descriptor.Resources.GetResourceAmount(resource) },
                "Healthy target selection is refused and trait recalculation does not refresh the spent finite Heal resource.");
            if (!retained) throw new InvalidOperationException(
                "Astral Heal healthy-target/resource retention failed.");
        }

        private static void Astral31RefreshAreas(IEnumerable<AreaEffectEntityData> areas,
            IList<UnitEntityData> actors)
        {
            var update = typeof(AreaEffectEntityData).GetMethod("UpdateUnits",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            if (update == null) throw new MissingMethodException(
                "Astral native area-membership update surface");
            // PlaceExpandedSummoningUnit already translocated each actor and
            // updated the native spatial grid. Keep this AI-bearing phase
            // unpaused and refresh only the real area membership surface.
            bool paused = Game.Instance.IsPaused;
            Game.Instance.EntityCreator.Tick();
            foreach (AreaEffectEntityData area in areas)
            {
                if (area == null || area.Destroyed || !area.IsInState)
                    throw new InvalidOperationException(
                        "Astral aura area is not active in the native scene.");
                for (int pass = 0; pass < 2; pass++)
                {
                    Astral31RequireAreaActors(actors);
                    update.Invoke(area, null);
                    area.Tick();
                    Game.Instance.EntityDestroyer.Tick();
                }
            }
            Astral31RequireAreaActors(actors);
            if (Game.Instance.IsPaused != paused)
                throw new InvalidOperationException(
                    "Astral area refresh changed the native pause state.");
        }

        private static void Astral31RequireAreaActors(
            IList<UnitEntityData> actors)
        {
            if (actors == null || actors.Any(unit => unit == null))
                throw new InvalidOperationException(
                    "Astral area fixture actor list is incomplete.");
            UnitEntityData invalid = actors.FirstOrDefault(unit =>
                !unit.IsInGame || unit.View?.MovementAgent == null ||
                unit.View.MovementAgent.IsReallyMoving ||
                Vector3.Distance(unit.Position, unit.View.transform.position) > .08f ||
                !DescribeExpandedSummoningGridEntry(unit).StartsWith(
                    "grid=row", StringComparison.Ordinal));
            if (invalid != null) throw new InvalidOperationException(
                "Astral area fixture requires active standing grid-synchronized " +
                "native actors: unit=" + (invalid.UniqueId ?? "<none>") +
                ";inGame=" + invalid.IsInGame + ";agent=" +
                (invalid.View?.MovementAgent != null) + ";moving=" +
                invalid.View?.MovementAgent?.IsReallyMoving + ";position=" +
                invalid.Position + ";view=" + invalid.View?.transform.position +
                ";" + DescribeExpandedSummoningGridEntry(invalid));
        }

        private static bool Astral31Finite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }
    }
}
