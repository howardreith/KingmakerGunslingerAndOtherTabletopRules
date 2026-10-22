using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Assets;
using UnityEngine;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private void ReviewHound30OriginalBody(UnitEntityData owner)
        {
            Mesh cache; string[] bones; Texture2D albedo; string status;
            bool loaded = PteranodonAssetRuntime.TryGetHoundArchonVisual(out cache, out bones, out albedo, out status);
            var renderers = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var body = renderers.Single(value => value.name == TheropodVisualPolicy.BodyRendererName);
            var resources = ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(owner.View);
            var pose = Sprint17OriginalWorldVertices(body);
            bool finite = pose.Length > 0 && pose.All(point => !float.IsNaN(point.x) && !float.IsInfinity(point.x) &&
                !float.IsNaN(point.y) && !float.IsInfinity(point.y) && !float.IsNaN(point.z) && !float.IsInfinity(point.z));
            var selection = Sprint22SelectionSurface(owner);
            string party = WriteExpandedSummoningPartyCameraCapture(owner, _request.EvidenceDirectory,
                "s30-hound-body-party.png", TheropodVisualPolicy.BodyRendererName);
            string supporting = CaptureSprint17OriginalBodyFrame(owner, body, "s30-hound-archon");
            var sword = owner.Body.PrimaryHand.MaybeWeapon;
            bool ownedSword = sword != null && sword.Wielder != null && ReferenceEquals(sword.Wielder.Unit, owner) &&
                sword.HoldingSlot != null && ReferenceEquals(sword.HoldingSlot.MaybeItem, sword) && !sword.IsLootable;
            bool exact = loaded && status == "visual:published" && body.enabled && EntityFadedIn(owner) && finite &&
                body.sharedMesh.name == "KMG_hound-archon_Original" && body.sharedMesh != cache &&
                body.sharedMesh.vertexCount == cache.vertexCount && HoundArchonVisualPolicy.IsPermitted("hound-archon", body.bones.Select(value => value.name)) &&
                ReferenceEquals(body.sharedMaterial.mainTexture, albedo) && resources.Length >= 3 &&
                renderers.Single(value => value.name == TheropodVisualPolicy.AccessoryRendererName).enabled == false &&
                (bool)selection["selectionSurface"] && ownedSword &&
                party.Contains("inFrame=true") && party.Contains("screenLit=true") && party.Contains("rendererEnabled=true") &&
                party.Contains("faderVisible=true") && supporting.Contains("supporting-art-only");
            Sprint25Check("archon-hound-original-body", exact, new JObject {
                ["owner"] = owner.UniqueId, ["blueprint"] = owner.Blueprint.AssetGuid, ["assetStatus"] = status,
                ["attachment"] = ExpandedSummoningPteranodonViewPatch.DescribeView(owner.View),
                ["partyCamera"] = party, ["supportingFrame"] = supporting, ["selection"] = selection,
                ["posedVertices"] = pose.Length, ["poseFinite"] = finite,
                ["privateResources"] = new JArray(resources.Select(value => value.GetInstanceID())),
                ["meshInstance"] = body.sharedMesh.GetInstanceID(), ["cacheInstance"] = cache == null ? 0 : cache.GetInstanceID(),
                ["bones"] = new JArray(body.bones.Select(value => value.name)),
                ["sword"] = sword?.Blueprint.AssetGuid, ["swordOwner"] = sword?.Wielder?.Unit?.UniqueId,
                ["swordLootable"] = sword == null || sword.IsLootable,
                ["scope"] = "Native canine-headed humanoid body/owned sword presentation; no resolved attack or full lifecycle claim" },
                "The exact Hound binds its original body to the measured native renderer, appears intact/selectable at native camera scale and owns its nonlootable sword; idle presentation does not prove the weapon is drawn.");
            if (!exact) throw new InvalidOperationException("Native original Hound body presentation failed.");
        }

        private IEnumerable<int> ReviewHound30Profile(ExpandedSummoningCorrectionFixture fixture, UnitEntityData owner)
        {
            foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
            foreach (int frame in Outsider30NativeAppearance(owner, "hound-archon")) yield return frame;
            ReviewHound30OriginalBody(owner);
            var power = owner.Descriptor.ActivatableAbilities.Enumerable.Single(value =>
                value.Blueprint.AssetGuid == "a7b339e4f6ff93a4697df5d7a87ff619");
            bool powerBuff = owner.Buffs.GetBuff(power.Blueprint.Buff) != null;
            var stats = owner.Stats;
            var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
            var areas = Sprint29OwnedAreas(owner, circle.Area);
            var carriers = owner.Buffs.Enumerable.Where(buff => ReferenceEquals(buff.Blueprint, circle.Carrier) &&
                ReferenceEquals(buff.Context?.MaybeCaster, owner)).ToArray();
            var missile = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                FcbMagicMissileGuid, "Hound real SR consumer");
            var context = new AbilityExecutionContext(new AbilityData(missile, fixture.Hostile.Descriptor),
                new AbilityParams { CasterLevel = 1, SpellLevel = 1 }, new TargetWrapper(owner), null);
            var check = context.TriggerRule(new RuleSpellResistanceCheck(context, owner));
            int sr = owner.Get<UnitPartSpellResistance>()?.GetValue(context) ?? 0;
            bool profile = stats.HitPoints.ModifiedValue == 39 && stats.BaseAttackBonus.ModifiedValue == 6 &&
                stats.AC.ModifiedValue == 19 && stats.SaveFortitude.ModifiedValue == 6 &&
                stats.SaveReflex.ModifiedValue == 5 && stats.SaveWill.ModifiedValue == 5 &&
                stats.SkillPerception.ModifiedValue == 10 && stats.SkillStealth.ModifiedValue == 13 &&
                stats.SkillMobility.ModifiedValue == 9 && !power.IsOn && !powerBuff && sr == 15 &&
                check.HasSpellResistance && !check.TargetIsImmune &&
                owner.Descriptor.State.HasConditionImmunity(UnitCondition.Petrified) &&
                !owner.Descriptor.Spellbooks.Any() && owner.Descriptor.State.CanAct;
            Sprint25Check("archon-hound-profile", profile, new JObject {
                ["owner"] = owner.UniqueId, ["hp"] = stats.HitPoints.ModifiedValue, ["bab"] = stats.BaseAttackBonus.ModifiedValue,
                ["ac"] = stats.AC.ModifiedValue, ["fort"] = stats.SaveFortitude.ModifiedValue,
                ["reflex"] = stats.SaveReflex.ModifiedValue, ["will"] = stats.SaveWill.ModifiedValue,
                ["perception"] = stats.SkillPerception.ModifiedValue, ["stealth"] = stats.SkillStealth.ModifiedValue,
                ["mobilityAcrobatics"] = stats.SkillMobility.ModifiedValue, ["sr"] = sr, ["nativeSrConsumer"] = check.HasSpellResistance,
                ["petrificationConditionImmune"] = owner.Descriptor.State.HasConditionImmunity(UnitCondition.Petrified),
                ["powerAttackToggleOn"] = power.IsOn, ["powerAttackBuffPresent"] = powerBuff,
                ["spellbooks"] = owner.Descriptor.Spellbooks.Count(), ["visual"] = ExpandedSummoningPteranodonViewPatch.DescribeView(owner.View) },
                "Actual adult Hound racial bases and native skill/SR/condition consumers; Power Attack is explicitly off for these rule comparisons, no cleric progression.");
            if (!profile) throw new InvalidOperationException("Hound adult native profile failed.");
            var grant = owner.Blueprint.AddFacts.SelectMany(fact => fact.ComponentsArray)
                .OfType<ArchonConstantMagicCircle>().Single();
            bool canonical = ReferenceEquals(grant.OwningBlueprint, owner.Blueprint) &&
                ReferenceEquals(grant.Carrier, circle.Carrier) && ReferenceEquals(grant.Spell, circle.Spell) && grant.CasterLevel == 6;
            bool shared = areas.Length == 1 && carriers.Length == 1 && canonical &&
                ReferenceEquals(areas[0].Blueprint, circle.Area) && areas[0].Context.Params.CasterLevel == 6 &&
                carriers[0].Context.Params.CasterLevel == 6 &&
                ReferenceEquals(carriers[0].Context.SourceAbility, circle.Spell) &&
                !ReferenceEquals(areas[0].Context.Params, AbilityParams.Empty);
            Sprint25Check("archon-hound-shared-circle", shared, new JObject {
                ["owner"] = owner.UniqueId, ["carrier"] = circle.Carrier.AssetGuid, ["area"] = circle.Area.AssetGuid,
                ["recipient"] = circle.Recipient.AssetGuid, ["ownerAreas"] = areas.Length, ["ownerCarriers"] = carriers.Length,
                ["cl"] = areas.FirstOrDefault()?.Context?.Params.CasterLevel,
                ["carrierCl"] = carriers.FirstOrDefault()?.Context?.Params.CasterLevel,
                ["sourceAbility"] = carriers.FirstOrDefault()?.Context?.SourceAbility?.AssetGuid,
                ["usesSharedEmptyParams"] = ReferenceEquals(areas.FirstOrDefault()?.Context?.Params, AbilityParams.Empty),
                ["sameCanonicalCarrier"] = canonical },
                "Hound uses the existing project's exact carrier/area/recipient, source owner and CL6, without a fork.");
            if (!shared) throw new InvalidOperationException("Hound shared Magic Circle source/CL failed.");
            int publicReferences;
            bool publication = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(BlueprintBootstrap.Library, true, out publicReferences);
            var roots = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Where(value => value.Creature.Key == "hound-archon").ToArray();
            int choices = ExpandedSummoningInventoryObserver.CanonicalParentGuids.Select(guid =>
                BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library, guid, "Hound hidden public surface"))
                .Sum(ability => ability.GetComponent<Kingmaker.UnitLogic.Abilities.Components.AbilityVariants>().Variants.Length);
            bool hidden = publication && roots.Length == 6 && roots.All(value => !SummonVisibilityCatalog.IsPublished(value)) &&
                publicReferences == SummonVisibilityCatalog.PublishedLogicalPlacementCount &&
                choices == publicReferences + ExpandedSummoningIdentityCatalog.NativeExpandedOptionIdentityCount;
            Sprint25Check("archon-hound-hidden-routes", hidden, new JObject { ["hiddenHoundRoots"] = roots.Length,
                ["publicReferences"] = publicReferences, ["choices"] = choices }, "All six new Hound roots stay suppressed and every released public choice remains exact.");
            if (!hidden) throw new InvalidOperationException("Hidden Hound changed public choices.");
            int originalStrength = stats.Strength.BaseValue;
            try
            {
                foreach (int mode in new[] { 0, 1 })
                {
                    if (owner.Body.CurrentHandEquipmentSetIndex != mode)
                    {
                        var command = new UnitSwitchHandEquipmentSet(mode); owner.Commands.Run(command);
                        DateTime deadline = DateTime.UtcNow.AddSeconds(20); int frames = 0;
                        while (!command.IsFinished && DateTime.UtcNow < deadline && frames++ < 1000) yield return 0;
                        if (!command.IsFinished || owner.Body.CurrentHandEquipmentSetIndex != mode)
                            throw new InvalidOperationException("Native Hound weapon-set command failed.");
                    }
                    var main = owner.Body.PrimaryHand.MaybeWeapon;
                    var bite = owner.Body.AdditionalLimbs.Select(slot => slot.MaybeWeapon).Single(weapon => weapon != null);
                    var attack = new UnitAttack(fixture.Hostile); attack.Init(owner);
                    var plan = attack.CreateFullAttack();
                    bool roles = main != null && !main.IsSecondary && bite.IsSecondary == (mode == 0) &&
                        plan.Count == (mode == 0 ? 3 : 2) && main.AttackRange.Value == 5 && bite.AttackRange.Value == 5;
                    var rows = new JArray();
                    foreach (int strength in new[] { 15, 21, 7 })
                    {
                        stats.Strength.BaseValue = strength;
                        int bonus = stats.Strength.Bonus;
                        var mainRule = Sprint29WeaponRule(owner, main); var biteRule = Sprint29WeaponRule(owner, bite);
                        int mainDamage = mode == 0 && bonus > 0 ? bonus * 3 / 2 : bonus;
                        int biteDamage = mode == 0 && bonus > 0 ? bonus / 2 : bonus;
                        bool exact = (int)mainRule["attack"] == 6 + bonus + (mode == 0 ? 1 : 0) &&
                            (int)biteRule["attack"] == 6 + bonus - (mode == 0 ? 5 : 0) &&
                            (int)mainRule["damageBonus"] == mainDamage && (int)biteRule["damageBonus"] == biteDamage &&
                            (int)mainRule["diceRolls"] == (mode == 0 ? 2 : 1) && (string)mainRule["dice"] == (mode == 0 ? "D6" : "D4") &&
                            (int)biteRule["diceRolls"] == 1 && (string)biteRule["dice"] == "D8";
                        rows.Add(new JObject { ["strength"] = strength, ["main"] = mainRule, ["bite"] = biteRule,
                            ["mainSecondary"] = main.IsSecondary, ["biteSecondary"] = bite.IsSecondary, ["exact"] = exact });
                        roles &= exact;
                    }
                    stats.Strength.BaseValue = originalStrength;
                    Sprint25Check("archon-hound-mode-" + mode, roles, new JObject { ["owner"] = owner.UniqueId,
                        ["mode"] = mode, ["rules"] = rows, ["planOnlyNoHitClaim"] = true,
                        ["secondaryWeapon"] = owner.Body.SecondaryHand.MaybeWeapon?.Blueprint?.AssetGuid,
                        ["nativePlan"] = new JArray(plan.Select(row => new JObject { ["weapon"] = row.Hand?.MaybeWeapon?.Blueprint?.AssetGuid,
                            ["penalty"] = row.AttackBonusPenalty, ["number"] = row.AttackNumber,
                            ["mainHand"] = ReferenceEquals(row.Hand, owner.Body.PrimaryHand),
                            ["secondaryHand"] = ReferenceEquals(row.Hand, owner.Body.SecondaryHand) })) },
                        "Actual native item/rule/plan consumers follow general PF1 signed Strength and primary/secondary roles; printed species damage discrepancies are disclosed.");
                    if (!roles) throw new InvalidOperationException("Hound native attack roles failed.");
                }
            }
            finally { stats.Strength.BaseValue = originalStrength; }
        }
    }
}
