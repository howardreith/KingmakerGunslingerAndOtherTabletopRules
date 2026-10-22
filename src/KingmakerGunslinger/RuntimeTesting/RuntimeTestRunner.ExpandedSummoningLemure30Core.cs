using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private const string Lemure30Claw = "780c5819371c4b5b88d72d12fc43a9f2";

        private IEnumerable<int> ReviewLemure30Core(ExpandedSummoningCorrectionFixture fixture, string phase)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            Game.Instance.IsPaused = true;
            CreateExpandedSummoningCorrectionHostile(fixture);
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                var completed = new List<UnitEntityData>();
                var targetBuffs = fixture.Hostile.Descriptor.Buffs.RawFacts.Select(value => value.Blueprint.AssetGuid)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                var areaBaseline = Game.Instance.State.AreaEffects.All.ToArray();
                ResetExpandedSummoningHostile(fixture);
                if (phase == "lemure-full" || phase == "lemure-ai")
                {
                    // The representative shared attempt detected retained
                    // combat state after a command. These cases therefore
                    // enter through separate fresh guarded processes.
                    Lemure30RequireReset(fixture, completed, targetBuffs, areaBaseline, "fresh-" + phase);
                    foreach (int frame in ReviewLemure30Combat(fixture, phase == "lemure-ai",
                        phase == "lemure-full", completed)) yield return frame;
                    yield break;
                }
                if (phase != "lemure-core") throw new InvalidOperationException("Unsupported isolated Lemure phase.");
                fixture.Hostile.Descriptor.Damage = fixture.HostileDamage + 1;
                var dirty = Lemure30ResetState(fixture, completed, targetBuffs, areaBaseline);
                Sprint25Check("lemure-core-residue-detected", !(bool)dirty["clean"] &&
                    !(bool)dirty["damageReset"], dirty,
                    "An intentionally introduced owned-target injury is noticed before the next case can start.");
                ResetExpandedSummoningHostile(fixture);
                Lemure30RequireReset(fixture, completed, targetBuffs, areaBaseline, "before-rules");
                UnityEngine.Object[] bodyResources = null;
                var owner = CastExpandedSummoningOwnTier(fixture, "lemure");
                Sprint28SetBrainActivePreservingActions(owner, false);
                try
                {
                    for (int frame = 0; frame < 8; frame++) yield return 0;
                    foreach (int frame in ReviewLemure30OriginalBody(fixture, owner)) yield return frame;
                    bodyResources = ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(owner.View);
                    ReviewLemure30Rules(owner, fixture.Hostile);
                    // Let the exact disposable actors' native damage effects settle.
                    for (int frame = 0; frame < 3; frame++) yield return 0;
                }
                finally
                {
                    owner.Commands.InterruptAll(true); fixture.Hostile.Commands.InterruptAll(true);
                    if (owner.IsInCombat) owner.LeaveCombat();
                    if (fixture.Hostile.IsInCombat) fixture.Hostile.LeaveCombat();
                    if (fixture.Hostile.Memory.Contains(owner)) fixture.Hostile.Memory.Remove(owner);
                    if (owner.Memory.Contains(fixture.Hostile)) owner.Memory.Remove(fixture.Hostile);
                    Game.Instance.Player.UpdateIsInCombat();
                    DisposeExpandedSummoningUnits(fixture.Created, new[] { owner }); completed.Add(owner);
                    ResetExpandedSummoningHostile(fixture);
                }
                for (int frame = 0; frame < 3; frame++) yield return 0;
                bool releasedBody = bodyResources != null && bodyResources.Length >= 3 &&
                    bodyResources.All(value => value == null);
                Sprint25Check("lemure-original-body-resources-released", releasedBody,
                    new JObject { ["captured"] = bodyResources == null ? 0 : bodyResources.Length,
                        ["remaining"] = bodyResources == null ? -1 : bodyResources.Count(value => value != null) },
                    "Actual disposal releases the exact per-view original mesh, materials and accessory component.");
                if (!releasedBody) throw new InvalidOperationException("Lemure body resource cleanup failed.");
                Lemure30RequireReset(fixture, completed, targetBuffs, areaBaseline, "before-single");
                foreach (int frame in ReviewLemure30Combat(fixture, false, false, completed)) yield return frame;
                // Native world/clock teardown closes this transaction. Do not
                // erase or tick protected party state to force another case.
            }
            finally
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
            }
        }

        private static JObject Lemure30ResetState(ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> completed, string[] targetBuffs,
            Kingmaker.EntitySystem.Entities.AreaEffectEntityData[] areaBaseline)
        {
            var target = fixture.Hostile;
            bool noUnits = !Game.Instance.State.Units.All.Any(value => value != null && !value.Destroyed &&
                value.Blueprint.AssetGuid == "80fd27e965b943b5b307a8c0a45cfb96") &&
                completed.All(value => value.Destroyed && !fixture.Created.Contains(value));
            bool noCommands = !target.Commands.Raw.Any(value => value != null && !value.IsFinished);
            bool noMemory = completed.All(value => !target.Memory.Contains(value));
            bool buffs = targetBuffs.SequenceEqual(target.Descriptor.Buffs.RawFacts.Select(value =>
                value.Blueprint.AssetGuid).OrderBy(value => value, StringComparer.Ordinal));
            var areas = Game.Instance.State.AreaEffects.All.ToArray();
            bool areasExact = areas.Length == areaBaseline.Length && areas.All(value => areaBaseline.Contains(value));
            bool damage = target.Descriptor.Damage == fixture.HostileDamage;
            bool settings = !SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            return new JObject { ["clean"] = noUnits && noCommands && noMemory && buffs && areasExact && damage &&
                    !target.IsInCombat && settings,
                ["noPriorSummons"] = noUnits, ["noQueuedTargetCommands"] = noCommands,
                ["noPriorOwnerMemory"] = noMemory, ["targetBuffsExact"] = buffs, ["areasExact"] = areasExact,
                ["damageReset"] = damage, ["targetOutOfCombat"] = !target.IsInCombat,
                ["rtwpSetting"] = settings, ["completedOwners"] = new JArray(completed.Select(value => value.UniqueId)) };
        }

        private void Lemure30RequireReset(ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> completed, string[] targetBuffs,
            Kingmaker.EntitySystem.Entities.AreaEffectEntityData[] areaBaseline, string stage)
        {
            var row = Lemure30ResetState(fixture, completed, targetBuffs, areaBaseline);
            bool clean = (bool)row["clean"];
            Sprint25Check("lemure-core-reset-" + stage, clean, row,
                "Exact disposable owners, commands, target memory, buffs, areas, injury and combat state meet the next case's preconditions.");
            if (!clean) throw new InvalidOperationException("Lemure shared fixture contamination: " + stage);
        }
        private IEnumerable<int> ReviewLemure30OriginalBody(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner)
        {
            Mesh source; string[] sourceBones; Texture2D albedo; string status;
            bool loaded = PteranodonAssetRuntime.TryGetLemureVisual(out source, out sourceBones, out albedo, out status);
            var missile = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                FcbMagicMissileGuid, "Lemure body SR isolation observation");
            var srContext = new AbilityExecutionContext(new AbilityData(missile, fixture.Hostile.Descriptor),
                new AbilityParams { CasterLevel = 1, SpellLevel = 1 }, new TargetWrapper(owner), null);
            var srBeforeSibling = Lemure30SrState(owner, srContext);
            var control = CastExpandedSummoningOwnTier(fixture, "lemure");
            Sprint28SetBrainActivePreservingActions(control, false);
            UnityEngine.Object[] controlResources = null;
            try
            {
                var ownerRenderers = owner.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var controlRenderers = control.View.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var body = ownerRenderers.Single(value => value.name == TheropodVisualPolicy.BodyRendererName);
                var otherBody = controlRenderers.Single(value => value.name == TheropodVisualPolicy.BodyRendererName);
                string attachment = ExpandedSummoningPteranodonViewPatch.DescribeView(owner.View);
                string otherAttachment = ExpandedSummoningPteranodonViewPatch.DescribeView(control.View);
                bool valid = loaded && status == "visual:published" &&
                    attachment.StartsWith("visual:attached;", StringComparison.Ordinal) &&
                    otherAttachment.StartsWith("visual:attached;", StringComparison.Ordinal) &&
                    LemureVisualPolicy.IsPermitted("lemure", body.bones.Select(value => value.name)) &&
                    body.sharedMesh.name == "KMG_lemure_Original" &&
                    body.sharedMesh != source && otherBody.sharedMesh != source &&
                    body.sharedMesh != otherBody.sharedMesh &&
                    body.sharedMaterial != otherBody.sharedMaterial &&
                    body.sharedMaterial.mainTexture == albedo && otherBody.sharedMaterial.mainTexture == albedo &&
                    body.sharedMesh.vertexCount == source.vertexCount &&
                    body.rootBone != null && body.rootBone.name == "Torso_Lower" &&
                    ownerRenderers.Single(value => value.name == TheropodVisualPolicy.AccessoryRendererName).enabled == false &&
                    controlRenderers.Single(value => value.name == TheropodVisualPolicy.AccessoryRendererName).enabled == false;
                controlResources = ExpandedSummoningPteranodonViewPatch.CaptureTheropodOwnedResources(control.View);
                Sprint25Check("lemure-original-body-attachment-isolation", valid,
                    new JObject { ["owner"] = owner.UniqueId, ["control"] = control.UniqueId,
                        ["cacheStatus"] = status, ["attachment"] = attachment,
                        ["controlAttachment"] = otherAttachment,
                        ["mesh"] = body.sharedMesh.name, ["vertices"] = body.sharedMesh.vertexCount,
                        ["bones"] = new JArray(body.bones.Select(value => value.name)),
                        ["meshInstance"] = body.sharedMesh.GetInstanceID(),
                        ["otherMeshInstance"] = otherBody.sharedMesh.GetInstanceID(),
                        ["sourceMeshInstance"] = source == null ? 0 : source.GetInstanceID(),
                        ["materialInstance"] = body.sharedMaterial.GetInstanceID(),
                        ["otherMaterialInstance"] = otherBody.sharedMaterial.GetInstanceID(),
                        ["albedoInstance"] = albedo == null ? 0 : albedo.GetInstanceID(),
                        ["controlOwnedResources"] = controlResources.Length,
                        ["srBeforeSibling"] = srBeforeSibling,
                        ["srWithSibling"] = Lemure30SrState(owner, srContext) },
                    "Two real summons bind original geometry to their own native rigs with private meshes/materials and one immutable original albedo cache.");
                if (!valid) throw new InvalidOperationException("Lemure original body attachment/isolation failed.");
            }
            finally { DisposeExpandedSummoningUnits(fixture.Created, new[] { control }); }
            for (int frame = 0; frame < 3; frame++) yield return 0;
            bool independent = controlResources != null && controlResources.Length >= 3 &&
                controlResources.All(value => value == null) && owner.IsInGame &&
                ExpandedSummoningPteranodonViewPatch.DescribeView(owner.View).StartsWith("visual:attached;", StringComparison.Ordinal) &&
                source != null && albedo != null;
            Sprint25Check("lemure-original-body-sibling-disposal", independent,
                new JObject { ["disposed"] = control.UniqueId, ["remainingOwner"] = owner.UniqueId,
                    ["captured"] = controlResources == null ? 0 : controlResources.Length,
                    ["remainingPrivateResources"] = controlResources == null ? -1 : controlResources.Count(value => value != null),
                    ["sourceAlive"] = source != null, ["albedoAlive"] = albedo != null,
                    ["srAfterSibling"] = Lemure30SrState(owner, srContext) },
                "Disposing one exact summon releases its private resources while the sibling and immutable cache remain live.");
            if (!independent) throw new InvalidOperationException("Lemure sibling body disposal isolation failed.");
        }

        private void ReviewLemure30ExtraplanarSrSources(UnitEntityData owner,
            UnitEntityData source, BlueprintAbility missile)
        {
            var marker = owner.Blueprint.ComponentsArray.OfType<OutsiderExtraplanarIdentity>().Single();
            var nativeMarker = marker.NativeMarker;
            var ownerContext = new AbilityExecutionContext(new AbilityData(missile, source.Descriptor),
                new AbilityParams { CasterLevel = 1, SpellLevel = 1 }, new TargetWrapper(owner), null);
            var sourceContext = new AbilityExecutionContext(new AbilityData(missile, owner.Descriptor),
                new AbilityParams { CasterLevel = 1, SpellLevel = 1 }, new TargetWrapper(source), null);
            bool sourceHadMarker = source.Descriptor.HasFact(nativeMarker);
            int sourceBefore = source.Get<UnitPartSpellResistance>()?.GetValue(sourceContext) ?? 0;
            var sourceFacts = source.Descriptor.Progression.Features.Enumerable.ToArray();
            if (sourceHadMarker) throw new InvalidOperationException("Native SR control already has extraplanar fact.");
            var added = source.Descriptor.AddFact(nativeMarker);
            try
            {
                int native = source.Get<UnitPartSpellResistance>()?.GetValue(sourceContext) ?? 0;
                var rule = sourceContext.TriggerRule(new RuleSpellResistanceCheck(sourceContext, source));
                bool retained = native > sourceBefore && rule.SpellResistance == native &&
                    source.Descriptor.HasFact(nativeMarker) && owner.Descriptor.HasFact(nativeMarker);
                Sprint25Check("lemure-extraplanar-native-control", retained,
                    new JObject { ["owner"] = owner.UniqueId, ["control"] = source.UniqueId,
                        ["marker"] = nativeMarker.AssetGuid, ["before"] = sourceBefore,
                        ["nativePart"] = native, ["nativeRule"] = rule.SpellResistance,
                        ["ownerKeepsNativeMarker"] = owner.Descriptor.HasFact(nativeMarker),
                        ["nativeGrantComponents"] = nativeMarker.ComponentsArray.OfType<AddSpellResistance>().Count() },
                    "An unselected exact disposable actor still receives the original native marker's real SR; native blueprint/components remain unchanged.");
                if (!retained) throw new InvalidOperationException("Native extraplanar SR positive control failed.");
            }
            finally { source.Descriptor.RemoveFact(added); }
            int sourceAfter = source.Get<UnitPartSpellResistance>()?.GetValue(sourceContext) ?? 0;
            bool sourceReset = sourceAfter == sourceBefore && !source.Descriptor.HasFact(nativeMarker) &&
                sourceFacts.SequenceEqual(source.Descriptor.Progression.Features.Enumerable);
            Sprint25Check("lemure-extraplanar-native-control-cleanup", sourceReset,
                new JObject { ["control"] = source.UniqueId, ["before"] = sourceBefore,
                    ["after"] = sourceAfter, ["factsExact"] = sourceFacts.SequenceEqual(source.Descriptor.Progression.Features.Enumerable) },
                "Remove only the exact added native fact and verify the control's original SR and feature references.");
            if (!sourceReset) throw new InvalidOperationException("Native extraplanar SR control cleanup failed.");
            var transient = new List<UnityEngine.Object>();
            var buff = TraitCondition("Lemure30-foreign-SR", SpellDescriptor.None, transient);
            var grant = ScriptableObject.CreateInstance<AddSpellResistance>();
            grant.Value = new ContextValue { ValueType = ContextValueType.Simple, Value = 17 };
            grant.AddCR = false;
            transient.Add(grant);
            buff.ComponentsArray = buff.ComponentsArray.Concat(new BlueprintComponent[] { grant }).ToArray();
            try
            {
                var applied = ApplySprint12Buff(owner, buff, source, TimeSpan.FromSeconds(6), 1);
                var rule = ownerContext.TriggerRule(new RuleSpellResistanceCheck(ownerContext, owner));
                int native = owner.Get<UnitPartSpellResistance>()?.GetValue(ownerContext) ?? 0;
                bool retained = applied.AppliedBuff != null && !applied.Immunity &&
                    native == 17 && rule.SpellResistance == 17 && owner.Descriptor.HasFact(nativeMarker);
                Sprint25Check("lemure-extraplanar-foreign-sr-retained", retained,
                    new JObject { ["owner"] = owner.UniqueId, ["caster"] = source.UniqueId,
                        ["applied"] = applied.AppliedBuff != null, ["nativePart"] = native,
                        ["nativeRule"] = rule.SpellResistance,
                        ["nativeMarkerStillPresent"] = owner.Descriptor.HasFact(nativeMarker) },
                    "A different actual native SR component on the selected creature is retained; suppression is exact source-specific, not a zero-SR override.");
                if (!retained) throw new InvalidOperationException("Foreign SR source preservation failed.");
            }
            finally
            {
                RemoveSprint12Buff(owner, buff);
                foreach (var value in transient) if (value != null) UnityEngine.Object.DestroyImmediate(value);
            }
            var afterRule = ownerContext.TriggerRule(new RuleSpellResistanceCheck(ownerContext, owner));
            int ownerAfter = owner.Get<UnitPartSpellResistance>()?.GetValue(ownerContext) ?? 0;
            bool reset = ownerAfter == 0 && afterRule.SpellResistance == 0 &&
                !afterRule.HasSpellResistance && owner.Descriptor.HasFact(nativeMarker);
            Sprint25Check("lemure-extraplanar-foreign-sr-cleanup", reset,
                new JObject { ["owner"] = owner.UniqueId, ["nativePart"] = ownerAfter,
                    ["nativeRule"] = afterRule.SpellResistance,
                    ["nativeMarkerStillPresent"] = owner.Descriptor.HasFact(nativeMarker) },
                "Only the controlled foreign buff grant ends, leaving zero adult SR and the native dismissal/banishment identity intact.");
            if (!reset) throw new InvalidOperationException("Foreign SR source cleanup failed.");
        }

        private static JObject Lemure30SrState(UnitEntityData owner, MechanicsContext context)
        {
            var part = owner.Get<UnitPartSpellResistance>();
            var entries = ReadExpandedSummoningOptionalMember(part, "SRs") as System.Collections.IEnumerable;
            var values = new JArray();
            if (entries != null) foreach (object entry in entries)
                values.Add(new JObject {
                    ["id"] = Convert.ToInt32(ReadExpandedSummoningOptionalMember(entry, "Id")),
                    ["value"] = Convert.ToInt32(ReadExpandedSummoningOptionalMember(entry, "Value")),
                    ["alignment"] = Convert.ToString(ReadExpandedSummoningOptionalMember(entry, "Alignment")),
                    ["descriptor"] = Convert.ToString(ReadExpandedSummoningOptionalMember(entry, "SpellDescriptor")) });
            return new JObject {
                ["partPresent"] = part != null,
                ["nativeValue"] = part == null ? 0 : part.GetValue(context),
                ["entries"] = values,
                ["features"] = new JArray(owner.Descriptor.Progression.Features.Enumerable.Select(fact => new JObject {
                    ["name"] = fact.Blueprint.name, ["guid"] = fact.Blueprint.AssetGuid,
                    ["srComponents"] = new JArray(fact.Blueprint.ComponentsArray.Where(component =>
                        component != null && component.GetType().Name.IndexOf("SpellResistance", StringComparison.Ordinal) >= 0)
                        .Select(component => component.GetType().FullName)) })),
                ["buffs"] = new JArray(owner.Buffs.Enumerable.Select(fact => new JObject {
                    ["name"] = fact.Blueprint.name, ["guid"] = fact.Blueprint.AssetGuid,
                    ["caster"] = fact.Context?.MaybeCaster?.UniqueId,
                    ["srComponents"] = new JArray(fact.Blueprint.ComponentsArray.Where(component =>
                        component != null && component.GetType().Name.IndexOf("SpellResistance", StringComparison.Ordinal) >= 0)
                        .Select(component => component.GetType().FullName)) })) };
        }

        private void ReviewLemure30Rules(UnitEntityData owner, UnitEntityData source)
        {
            var stats = owner.Descriptor.Stats;
            bool profile = stats.HitPoints.ModifiedValue == 13 &&
                stats.BaseAttackBonus.ModifiedValue == 2 && stats.AC.ModifiedValue == 14 &&
                stats.SaveFortitude.ModifiedValue == 4 && stats.SaveReflex.ModifiedValue == 3 &&
                stats.SaveWill.ModifiedValue == 0 && stats.Speed.ModifiedValue == 20 &&
                stats.Intelligence.BaseValue == 0 && owner.Descriptor.State.IsConscious &&
                owner.Descriptor.Alignment.Value == Alignment.LawfulEvil && !owner.Descriptor.Spellbooks.Any();
            Sprint25Check("lemure-core-profile", profile, new JObject {
                ["owner"] = owner.UniqueId, ["hp"] = stats.HitPoints.ModifiedValue,
                ["bab"] = stats.BaseAttackBonus.ModifiedValue, ["ac"] = stats.AC.ModifiedValue,
                ["fort"] = stats.SaveFortitude.ModifiedValue, ["reflex"] = stats.SaveReflex.ModifiedValue,
                ["will"] = stats.SaveWill.ModifiedValue, ["speed"] = stats.Speed.ModifiedValue,
                ["intelligence"] = stats.Intelligence.BaseValue,
                ["conscious"] = owner.Descriptor.State.IsConscious,
                ["alignment"] = owner.Descriptor.Alignment.Value.ToString(),
                ["spellbooks"] = new JArray(owner.Descriptor.Spellbooks.Select(value => value.Blueprint.AssetGuid)),
                ["activeAbilities"] = new JArray(owner.Descriptor.Abilities.RawFacts.Select(value => new JObject {
                    ["name"] = value.Blueprint.name, ["guid"] = value.Blueprint.AssetGuid })) },
                "Native adult bases include live Constitution and no intellect or generic undead substitution.");
            if (!profile) throw new InvalidOperationException("Lemure core profile failed.");
            Sprint25Check("lemure-core-weapon-reach", owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Value == 5,
                new JObject { ["owner"] = owner.UniqueId,
                    ["rangeFeet"] = owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Value,
                    ["typeRangeFeet"] = owner.Body.PrimaryHand.MaybeWeapon.Blueprint.Type.AttackRange.Value,
                    ["rawTypeRangeFeet"] = ((Feet)ReadExpandedSummoningOptionalMember(
                        owner.Body.PrimaryHand.MaybeWeapon.Blueprint.Type, "m_AttackRange")).Value,
                    ["weaponGuid"] = owner.Body.PrimaryHand.MaybeWeapon.Blueprint.AssetGuid },
                "The actual native claw retains the adult five-foot reach.");
            var buffs = owner.Descriptor.Buffs.RawFacts.Select(value => new JObject {
                ["name"] = value.Blueprint.name, ["guid"] = value.Blueprint.AssetGuid }).ToArray();
            bool noTemplate = !buffs.Any(value => ExpandedSummoningAbilityBuilder.NativeTemplateIds.Contains((string)value["guid"]) ||
                ((string)value["name"]).StartsWith("KMG_Summoning_Template_", StringComparison.Ordinal));
            Sprint25Check("lemure-core-no-inherited-template", noTemplate,
                new JObject { ["owner"] = owner.UniqueId, ["buffs"] = new JArray(buffs) },
                "An untemplated outsider retains the native summon marker without inherited celestial/fiendish power.");
            bool summonMarker = owner.Descriptor.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) != null;
            Sprint25Check("lemure-core-native-summon-marker", summonMarker,
                new JObject { ["owner"] = owner.UniqueId, ["summonMarker"] = summonMarker },
                "The native summon-owned lifecycle marker remains after scoped inherited-template removal.");
            if (!summonMarker) throw new InvalidOperationException("Lemure summon ownership marker is absent.");
            int strength = stats.Strength.BaseValue;
            try
            {
                foreach (var spec in new[] { new { Str=11, Attack=2, Bonus=0 },
                    new { Str=15, Attack=4, Bonus=2 }, new { Str=7, Attack=0, Bonus=-2 } })
                {
                    stats.Strength.BaseValue = spec.Str;
                    JObject rule = Sprint29WeaponRule(owner, owner.Body.PrimaryHand.MaybeWeapon);
                    JObject other = Sprint29WeaponRule(owner, owner.Body.AdditionalLimbs.Single().MaybeWeapon);
                    bool exact = (int)other["attack"] == spec.Attack && (int)other["damageBonus"] == spec.Bonus &&
                        (int)other["diceRolls"] == 1 && (string)other["dice"] == "D4" &&
                        (bool)other["ownerExact"] && (int)rule["attack"] == spec.Attack &&
                        (int)rule["damageBonus"] == spec.Bonus && (int)rule["diceRolls"] == 1 &&
                        (string)rule["dice"] == "D4" && (bool)rule["ownerExact"];
                    rule["owner"] = owner.UniqueId; rule["fixtureStrength"] = spec.Str;
                    rule["nativeStrengthModifier"] = stats.Strength.Bonus;
                    rule["additionalClaw"] = other;
                    Sprint25Check("lemure-core-strength-" + spec.Str, exact, rule,
                        "Actual native attack and weapon-damage calculations retain signed live Strength scaling.");
                    if (!exact) throw new InvalidOperationException("Lemure live Strength rule failed.");
                }
            }
            finally { stats.Strength.BaseValue = strength; }
            int hp = stats.HitPoints.BaseValue, injury = owner.Descriptor.Damage;
            try
            {
                // Capacity only: protect this owned 13-HP fixture from difficulty-scaled probe damage.
                stats.HitPoints.BaseValue = 10000;
                foreach (string bypass in new[] { "mundane", "good", "silver", "good-and-silver", "cold-iron", "evil" })
                {
                    var packet = new PhysicalDamage(new DiceFormula(0, DiceType.Zero), PhysicalDamageForm.Slashing);
                    packet.AddBonus(12);
                    if (bypass == "good" || bypass == "good-and-silver") packet.AddAlignment(DamageAlignment.Good);
                    if (bypass == "evil") packet.AddAlignment(DamageAlignment.Evil);
                    if (bypass == "silver" || bypass == "good-and-silver") packet.AddMaterial(PhysicalDamageMaterial.Silver);
                    if (bypass == "cold-iron") packet.AddMaterial(PhysicalDamageMaterial.ColdIron);
                    int expected = bypass == "good" || bypass == "silver" || bypass == "good-and-silver" ? 12 : 7;
                    Lemure30DamageProbe(owner, source, packet, "physical-" + bypass, expected, false);
                }
                foreach (DamageEnergyType energy in new[] { DamageEnergyType.Fire, DamageEnergyType.Acid,
                    DamageEnergyType.Cold, DamageEnergyType.Electricity, DamageEnergyType.Sonic })
                {
                    var packet = new EnergyDamage(new DiceFormula(0, DiceType.Zero), energy);
                    packet.AddBonus(12);
                    int expected = energy == DamageEnergyType.Acid || energy == DamageEnergyType.Cold ? 2 : 12;
                    Lemure30DamageProbe(owner, source, packet, "energy-" + energy, expected,
                        energy == DamageEnergyType.Fire);
                }
            }
            finally { owner.Descriptor.Damage = injury; stats.HitPoints.BaseValue = hp; }
            var transient = new List<UnityEngine.Object>();
            try
            {
                foreach (var spec in new[] {
                    new { Name="mind-affecting", Descriptor=SpellDescriptor.MindAffecting | SpellDescriptor.Fear, Immune=true },
                    new { Name="poison", Descriptor=SpellDescriptor.Poison, Immune=true },
                    new { Name="unrelated", Descriptor=SpellDescriptor.None, Immune=false } })
                {
                    var buff = TraitCondition("Lemure30-" + spec.Name, spec.Descriptor, transient);
                    try
                    {
                        var control = ApplySprint12Buff(source, buff, source, TimeSpan.FromSeconds(6), 10);
                        var rule = ApplySprint12Buff(owner, buff, source, TimeSpan.FromSeconds(6), 10);
                        bool exact = control.AppliedBuff != null && !control.Immunity &&
                            rule.Immunity == spec.Immune && (rule.AppliedBuff == null) == spec.Immune;
                        Sprint25Check("lemure-core-descriptor-" + spec.Name, exact,
                            new JObject { ["owner"] = owner.UniqueId, ["source"] = source.UniqueId,
                                ["descriptor"] = spec.Descriptor.ToString(), ["nativeImmune"] = rule.Immunity,
                                ["applied"] = rule.AppliedBuff != null, ["controlApplied"] = control.AppliedBuff != null,
                                ["controlledDescriptorProbe"] = true },
                            "Native RuleApplyBuff tests immunity with a positive susceptible control; this is not a Cause Fear or cloud cast.");
                        if (!exact) throw new InvalidOperationException("Lemure descriptor immunity failed.");
                    }
                    finally { RemoveSprint12Buff(owner, buff); RemoveSprint12Buff(source, buff); }
                }
            }
            finally { foreach (var value in transient) if (value != null) UnityEngine.Object.DestroyImmediate(value); }
            var missile = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                FcbMagicMissileGuid, "Lemure no-SR native rule probe");
            var context = new AbilityExecutionContext(new AbilityData(missile, source.Descriptor),
                new AbilityParams { CasterLevel = 1, SpellLevel = 1 }, new TargetWrapper(owner), null);
            var resistance = context.TriggerRule(new RuleSpellResistanceCheck(context, owner));
            int nativeSr = owner.Get<UnitPartSpellResistance>()?.GetValue(context) ?? 0;
            bool noSr = missile.SpellResistance && nativeSr == 0 && resistance.SpellResistance == 0 &&
                !resistance.HasSpellResistance && !resistance.IsSpellResisted && !resistance.TargetIsImmune;
            Sprint25Check("lemure-core-no-sr", noSr, new JObject {
                ["owner"] = owner.UniqueId, ["source"] = source.UniqueId,
                ["spell"] = missile.AssetGuid, ["hasResistance"] = resistance.HasSpellResistance,
                ["resisted"] = resistance.IsSpellResisted, ["immune"] = resistance.TargetIsImmune,
                ["nativePartValue"] = nativeSr, ["nativeRuleValue"] = resistance.SpellResistance,
                ["initiator"] = resistance.Initiator.UniqueId, ["target"] = resistance.Target.UniqueId,
                ["contextCaster"] = context.MaybeCaster.UniqueId,
                ["contextCl"] = context.Params.CasterLevel, ["contextSpellLevel"] = context.Params.SpellLevel,
                ["srState"] = Lemure30SrState(owner, context),
                ["ruleOnlyNoCast"] = true }, "An actual SR-allowing native spell resistance check and actual owner part both find zero Lemure SR, with exact source attribution.");
            if (!noSr) throw new InvalidOperationException("Lemure no-SR consumer failed.");
            ReviewLemure30ExtraplanarSrSources(owner, source, missile);
            foreach (var spec in new[] {
                new { Name="mind-affecting-ability", Guid="bd81a3931aa285a4f9844585b5d97e51", Cl=2 },
                new { Name="poison-ability", Guid="68a9e6d7256f1354289a39003a46d826", Cl=2 } })
            {
                var ability = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    spec.Guid, "Lemure native descriptor ability consumer");
                var immunityContext = new AbilityExecutionContext(new AbilityData(ability, source.Descriptor),
                    new AbilityParams { CasterLevel = spec.Cl }, new TargetWrapper(owner), null);
                var check = immunityContext.TriggerRule(new RuleSpellResistanceCheck(immunityContext, owner));
                var controlContext = new AbilityExecutionContext(new AbilityData(ability, source.Descriptor),
                    new AbilityParams { CasterLevel = spec.Cl }, new TargetWrapper(source), null);
                var control = controlContext.TriggerRule(new RuleSpellResistanceCheck(controlContext, source));
                Sprint25Check("lemure-core-" + spec.Name, check.TargetIsImmune && !control.TargetIsImmune,
                    new JObject { ["owner"] = owner.UniqueId, ["source"] = source.UniqueId,
                        ["ability"] = ability.AssetGuid, ["nativeSpellDescriptor"] = immunityContext.SpellDescriptor.ToString(),
                        ["targetImmune"] = check.TargetIsImmune, ["controlImmune"] = control.TargetIsImmune,
                        ["ruleOnlyNoCast"] = true },
                    "Actual native ability-immunity eligibility is distinct from buff immunity and finite spell casts.");
            }
        }

        private void Lemure30DamageProbe(UnitEntityData owner, UnitEntityData source,
            BaseDamage packet, string id, int expected, bool immune)
        {
            owner.Descriptor.Damage = 0;
            if (owner.Descriptor.State.IsDead || owner.Descriptor.Damage != 0 || owner.Descriptor.Stats.HitPoints.ModifiedValue < 10000)
                throw new InvalidOperationException("Owned damage fixture cannot establish its next probe preconditions.");
            var part = owner.Get<UnitPartDamageReduction>();
            var sources = part == null ? new JArray() : new JArray(part.AllSources.Select(value => new JObject {
                ["component"] = value.GetType().FullName, ["value"] = value.GetCurrentValue(),
                ["bypassedForPacket"] = value.Bypassed(packet, null) }));
            var rule = Rulebook.Trigger(new RuleDealDamage(source, owner, new DamageBundle(packet)));
            var result = rule.ResultDamage?.SingleOrDefault(value => ReferenceEquals(value.Source, packet));
            bool exact = result.HasValue && result.Value.FinalValue == expected && packet.Immune == immune &&
                (immune ? rule.Damage == 0 : rule.Damage > 0) && owner.Descriptor.Damage == rule.Damage;
            Sprint25Check("lemure-core-" + id, exact, new JObject {
                ["owner"] = owner.UniqueId, ["source"] = source.UniqueId, ["fixedInput"] = 12,
                ["controlledDamageProbeNoAttack"] = true, ["fixtureHpCapacity"] = 10000,
                ["finalBeforeDifficulty"] = result?.FinalValue, ["nativeImmune"] = packet.Immune,
                ["nativeDamage"] = rule.Damage, ["hpDelta"] = owner.Descriptor.Damage,
                ["packetType"] = packet.GetType().FullName, ["resistanceSources"] = sources },
                "Actual native damage reduction, resistance or immunity consumes this attributed packet independently.");
            // A clean product mismatch remains a failed row. Other independent
            // probes may continue only while this owned fixture is still safe;
            // the next probe separately resets and verifies its injury state.
            if (owner.Descriptor.State.IsDead || owner.Descriptor.Damage != rule.Damage)
                throw new InvalidOperationException("Owned damage fixture lost injury/actor integrity: " + id);
        }

        private IEnumerable<int> ReviewLemure30Combat(ExpandedSummoningCorrectionFixture fixture, bool ai, bool full, List<UnitEntityData> completed)
        {
            ResetExpandedSummoningHostile(fixture);
            var owner = CastExpandedSummoningOwnTier(fixture, "lemure");
            var target = fixture.Hostile;
            Sprint28SetBrainActivePreservingActions(owner, false);
            string id = ai ? "ai" : full ? "full" : "single";
            int targetAc = target.Descriptor.Stats.AC.BaseValue;
            try
            {
                // Match the final successful native command fixture: allow
                // fresh spawn/view state to settle before checking readiness.
                for (int frame = 0; frame < 8; frame++) yield return 0;
                owner.Commands.InterruptAll(true); target.Commands.InterruptAll(true);
                owner.Commands.RemoveFinishedAndUpdateQueue(); target.Commands.RemoveFinishedAndUpdateQueue();
                Vector3 far; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 5f, out far, out lane, out direction);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, ai ? far : origin + (far - origin).normalized *
                    (owner.Corpulence + target.Corpulence + .8f));
                // Only the owned target's AC is lowered; rolls and hit decisions remain native.
                target.Descriptor.Stats.AC.BaseValue = 0;
                foreach (var unit in new[] { owner, target })
                {
                    unit.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit);
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                if (ai) { Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions(); }
                var probe = new UnitAttack(target); probe.Init(owner);
                float beforeRangePlacement = owner.DistanceTo(target);
                bool inRangeBefore = probe.IsUnitEnoughClose;
                if (!ai)
                {
                    // Derive contact from this actual native command instead
                    // of borrowing a giant weapon's fixed separation margin.
                    float contact = Math.Max(owner.Corpulence + target.Corpulence + .01f,
                        probe.ApproachRadius - .05f);
                    PlaceExpandedSummoningUnit(target, origin + (far - origin).normalized * contact);
                    owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                }
                bool powerAttackAbsent = !owner.Descriptor.HasFact(BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                    BlueprintBootstrap.Library, "9972f33f977fc724c838e59641b2fca5", "native Power Attack control")) &&
                    !owner.Descriptor.ActivatableAbilities.RawFacts.Any(value => value.Blueprint.AssetGuid ==
                        "a7b339e4f6ff93a4697df5d7a87ff619");
                bool ready = owner.IsInGame && owner.View != null && owner.View.IsInGame &&
                    target.IsInGame && target.View != null && target.View.IsInGame &&
                    owner.Descriptor.State.CanAct && owner.IsEnemy(target) && target.IsEnemy(owner) &&
                    owner.HasLOS(target) && owner.Memory.ContainsVisible(target) && powerAttackAbsent &&
                    !target.Descriptor.State.IsDead && (ai ? owner.IsAIEnabled && owner.Brain.Actions.Count > 0 &&
                        owner.Brain.AvailableActions.Count > 0 : probe.IsUnitEnoughClose);
                Sprint25Check("lemure-core-" + id + "-ready", ready, new JObject {
                    ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["lane"] = lane,
                    ["direction"] = direction, ["los"] = owner.HasLOS(target),
                    ["canAct"] = owner.Descriptor.State.CanAct,
                    ["conscious"] = owner.Descriptor.State.IsConscious,
                    ["ownerEnemy"] = owner.IsEnemy(target), ["targetEnemy"] = target.IsEnemy(owner),
                    ["ownerInCombat"] = owner.IsInCombat, ["targetAlive"] = !target.Descriptor.State.IsDead,
                    ["aiEnabled"] = owner.IsAIEnabled, ["ownerInGame"] = owner.IsInGame,
                    ["viewInGame"] = owner.View != null && owner.View.IsInGame,
                    ["targetInGame"] = target.IsInGame,
                    ["targetViewInGame"] = target.View != null && target.View.IsInGame,
                    ["nativeInRange"] = probe.IsUnitEnoughClose, ["approachRadius"] = probe.ApproachRadius,
                    ["weaponReachFeet"] = owner.Body.PrimaryHand.MaybeWeapon.AttackRange.Value,
                    ["ownerRadius"] = owner.Corpulence, ["targetRadius"] = target.Corpulence,
                    ["distanceBeforeContact"] = beforeRangePlacement, ["inRangeBeforeContact"] = inRangeBefore,
                    ["spawnSettlingFrames"] = 8,
                    ["visible"] = owner.Memory.ContainsVisible(target), ["distance"] = owner.DistanceTo(target),
                    ["fixtureTargetAcBase"] = 0, ["nativeTargetAc"] = target.Descriptor.Stats.AC.ModifiedValue,
                    ["powerAttackAbsent"] = powerAttackAbsent, ["actions"] = owner.Brain.Actions.Count,
                    ["available"] = owner.Brain.AvailableActions.Count },
                    "An awake visible disposable hostile and the native owner/action preconditions precede combat.");
                if (!ready) throw new InvalidOperationException("Lemure combat fixture not ready: " + id);
                var observer = new Sprint24AttackObserver { Owner = owner, Target = target };
                var costs = new JArray(); var endings = new JArray();
                var commands = new HashSet<UnitCommand>(); var ambient = new HashSet<string>();
                UnitAttack issued = ai ? null : new UnitAttack(target) { ForceFullAttack = full, IsSingleAttack = !full };
                int ticks = 0, frames = 0, impactFrames = 0;
                EventBus.Subscribe(observer);
                try
                {
                    using (var economy = new FirearmReviewCosts(owner, costs))
                    using (var trace = new Sprint26CommandTrace(owner, endings))
                    {
                        if (issued != null) { commands.Add(issued); owner.Commands.Run(issued); }
                        DateTime deadline = DateTime.UtcNow.AddSeconds(45);
                        while (frames++ < 4000 && DateTime.UtcNow < deadline)
                        {
                            if (ai && (frames == 1 || frames % 60 == 0 &&
                                !owner.Commands.Raw.Any(value => value != null && !value.IsFinished)))
                            {
                                foreach (string candidate in Sprint24TickExactAi(owner))
                                    if (candidate != owner.UniqueId) ambient.Add(candidate);
                                ticks++;
                            }
                            foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                            bool terminal = ai ? commands.OfType<UnitAttack>().Any(value => value.AiAction != null &&
                                value.IsFinished && (value.Result == UnitCommand.ResultType.Success || trace.Exhausted(value))) :
                                issued.IsFinished && observer.WeaponAttacks.Count >= (full ? 2 : 1);
                            if (terminal && (!ai || observer.Damage.Any(value => value.Damage > 0))) break;
                            Game.Instance.IsPaused = false; yield return 0;
                        }
                        int hits = observer.WeaponAttacks.Count(value => value.AttackRoll != null && value.AttackRoll.IsHit);
                        DateTime impactDeadline = DateTime.UtcNow.AddSeconds(10);
                        while (observer.Damage.Count < hits && impactFrames++ < 600 && DateTime.UtcNow < impactDeadline) yield return 0;
                        bool packets = observer.WeaponAttacks.Count > 0 && observer.WeaponAttacks.All(value =>
                            value.Weapon?.Blueprint?.AssetGuid == Lemure30Claw) &&
                            observer.Damage.Count == hits && observer.Rolls.Count == observer.WeaponAttacks.Count;
                        bool exact = packets && !target.Descriptor.State.IsDead && (ai ? ticks > 0 &&
                            commands.OfType<UnitAttack>().Any(value => value.AiAction != null && value.IsFinished &&
                                (value.Result == UnitCommand.ResultType.Success || trace.Exhausted(value))) &&
                            observer.Damage.Any(value => value.Damage > 0) : issued.IsFinished &&
                            (issued.Result == UnitCommand.ResultType.Success || trace.Exhausted(issued)) &&
                            issued.AllAttacks.Count == (full ? 2 : 1) && observer.WeaponAttacks.Count == (full ? 2 : 1) &&
                            observer.OwnerWeaponAttacks.Count == (full ? 2 : 1) &&
                            economy.Count(issued) == 1 && economy.Exact(issued, false) && !issued.Cutscene && !issued.IsIgnoreCooldown);
                        Sprint25Check("lemure-core-" + id + "-resolved", exact, new JObject {
                            ["owner"] = owner.UniqueId, ["target"] = target.UniqueId, ["frames"] = frames,
                            ["issuedFinished"] = issued?.IsFinished, ["issuedResult"] = issued?.Result.ToString(),
                            ["planned"] = issued?.AllAttacks.Count, ["weaponAttacks"] = observer.WeaponAttacks.Count,
                            ["rolls"] = new JArray(observer.WeaponAttacks.Select(value => new JObject {
                                ["weapon"] = value.Weapon?.Blueprint?.AssetGuid, ["hit"] = value.AttackRoll?.IsHit,
                                ["natural"] = value.AttackRoll?.Roll.Value, ["attackBonus"] = value.AttackRoll?.AttackBonus })),
                            ["damage"] = new JArray(observer.Damage.Select(Sprint16DamageEvent)), ["hits"] = hits,
                            ["physicalAlignments"] = new JArray(observer.Damage.SelectMany(value => value.DamageBundle
                                .OfType<PhysicalDamage>()).Select(value => value.AlignmentsMask.ToString())),
                            ["impactWaitFrames"] = impactFrames, ["costs"] = costs, ["nativeEnds"] = endings,
                            ["commands"] = new JArray(commands.Select(Sprint27AiCommand)), ["aiTicks"] = ticks,
                            ["ambientCandidatesNeverTicked"] = new JArray(ambient),
                            ["fixtureAttackCommands"] = ai ? 0 : 1, ["forcedRolls"] = 0,
                            ["powerAttackAbsent"] = powerAttackAbsent }, ai ?
                            "The native brain chooses and finishes its own claw attack with resolved damage and no fixture attack command." :
                            "One fixture-issued native attack spends its action and resolves exactly the selected claw routine without forced rolls.");
                        if (!exact) throw new InvalidOperationException("Lemure combat consumer failed: " + id);
                    }
                }
                finally { EventBus.Unsubscribe(observer); }
            }
            finally
            {
                Sprint28SetBrainActivePreservingActions(owner, false);
                owner.Commands.InterruptAll(true); target.Commands.InterruptAll(true);
                if (owner.IsInCombat) owner.LeaveCombat(); if (target.IsInCombat) target.LeaveCombat();
                Game.Instance.Player.UpdateIsInCombat();
                if (target.Memory.Contains(owner)) target.Memory.Remove(owner);
                if (owner.Memory.Contains(target)) owner.Memory.Remove(target);
                target.Descriptor.Stats.AC.BaseValue = targetAc;
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
                completed.Add(owner);
                ResetExpandedSummoningHostile(fixture);
            }
        }
    }
}
