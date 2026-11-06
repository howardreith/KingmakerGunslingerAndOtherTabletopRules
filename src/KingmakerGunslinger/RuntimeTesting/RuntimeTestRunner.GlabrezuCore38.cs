using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        /// <summary>
        /// The Glabrezu's own body, observed on a live unit: five primary
        /// natural attacks from three exact weapon identities at one printed
        /// reach, the pincers' widened critical threat, the printed defensive
        /// profile, the printed feats, the printed constant sense, this
        /// project's own attached four-armed mesh, and the explicit absence of
        /// the routines this creature does not have.
        ///
        /// <para>The released Sprint 37 brain, readiness and watcher helpers
        /// are reused as they stand rather than reimplemented; only this
        /// creature's own facts are new.</para>
        /// </summary>
        private IEnumerable<int> ReviewGlabrezuCore38(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint38GlabrezuCorePhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            int oldTargetAc = target.Stats.AC.BaseValue;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetDamage = target.Damage;
            Vector3 oldTargetPosition = target.Position;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint38-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint38 glabrezu core cell failed: " + id);
                };

            try
            {
                GlabrezuCoreBootstrap38(check);

                UnitEntityData glabrezu = CastExpandedSummoningOwnTier(
                    fixture, GlabrezuVisualPolicy38.Glabrezu);
                owned.Add(glabrezu);
                VrockQuiesce37(glabrezu);
                foreach (int frame in Outsider30NativeAppearance(glabrezu,
                    glabrezu.Blueprint.name)) yield return frame;

                GlabrezuCoreProfile38(glabrezu, check);
                GlabrezuCoreDefenses38(glabrezu, check);
                GlabrezuCoreFeats38(glabrezu, check);
                foreach (int frame in GlabrezuAttackFallback38(glabrezu,
                    target, check)) yield return frame;
            }
            finally
            {
                target.Stats.AC.BaseValue = oldTargetAc;
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                target.Damage = oldTargetDamage;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory, "sprint38-glabrezu-core.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        /// <summary>
        /// Every private Sprint 38 dependency resolves at its planned type,
        /// and the creature has exactly one route. Publication state is
        /// recorded rather than demanded: this phase runs against a hidden
        /// route during qualification and against the published one at the
        /// final candidate, and asserting concealment would make it unrunnable
        /// at the bytes that are actually released.
        /// </summary>
        private static void GlabrezuCoreBootstrap38(
            Action<string, bool, JObject, string> check)
        {
            string prefix = ExpandedSummoningSpecialBuilder.Glabrezu38Prefix;
            var routes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.Monster)
                .Where(value => value.Creature.Key ==
                    GlabrezuVisualPolicy38.Glabrezu)
                .OrderBy(value => value.ParentTier).ToArray();
            var allyRoutes = ExpandedSummoningCatalog
                .GenerateVariants(SummonFamily.NaturesAlly)
                .Where(value => value.Creature.Key ==
                    GlabrezuVisualPolicy38.Glabrezu).ToArray();
            var expected = ExpandedSummoningIdentityCatalog.Build()
                .Where(value => value.Symbol.StartsWith(prefix,
                    StringComparison.Ordinal))
                .OrderBy(value => value.Symbol, StringComparer.Ordinal)
                .ToArray();
            var actual = ExpandedSummoningBlueprints.Registered.BySymbol
                .Where(pair => pair.Key.StartsWith(prefix,
                    StringComparison.Ordinal))
                .ToDictionary(pair => pair.Key, pair => pair.Value,
                    StringComparer.Ordinal);
            string[] missing = expected
                .Where(value => !actual.ContainsKey(value.Symbol))
                .Select(value => value.Symbol).ToArray();
            var wrongTypes = expected.Where(value =>
                actual.ContainsKey(value.Symbol) &&
                (actual[value.Symbol] == null || !string.Equals(
                    actual[value.Symbol].GetType().Name, value.PlannedType,
                    StringComparison.Ordinal)))
                .Select(value => new JObject {
                    ["symbol"] = value.Symbol,
                    ["expected"] = value.PlannedType,
                    ["actual"] = actual[value.Symbol] == null ? null :
                        actual[value.Symbol].GetType().Name
                }).ToArray();
            check("core-bootstrap",
                routes.Length == 1 && allyRoutes.Length == 0 &&
                routes[0].ParentTier == 9 &&
                routes[0].Multiplicity == SummonMultiplicity.One &&
                !routes[0].Creature.MonsterTemplated &&
                actual.Count == expected.Length &&
                missing.Length == 0 && wrongTypes.Length == 0,
                new JObject {
                    ["monsterRoutes"] = new JArray(routes.Select(value =>
                        new JObject {
                            ["tier"] = value.ParentTier,
                            ["quantity"] = value.Multiplicity.ToString(),
                            ["templated"] = value.Creature.MonsterTemplated,
                            ["published"] =
                                SummonVisibilityCatalog.IsPublished(value)
                        })),
                    ["naturesAllyRoutes"] = allyRoutes.Length,
                    ["expectedRegistrationCount"] = expected.Length,
                    ["actualRegistrationCount"] = actual.Count,
                    ["missing"] = new JArray(missing),
                    ["wrongTypes"] = new JArray(wrongTypes)
                }, "Exactly one untemplated quantity-one Summon Monster IX route exists, no Nature's Ally route does, and every Sprint 38 private dependency resolves at its planned type; publication state is recorded.");
        }

        /// <summary>
        /// The live profile: five primary limbs from three weapon identities at
        /// one reach, the pincers' widened critical threat, this project's own
        /// attached body, and the explicit absence of Pounce and Grab.
        /// </summary>
        private static void GlabrezuCoreProfile38(UnitEntityData glabrezu,
            Action<string, bool, JObject, string> check)
        {
            ItemEntityWeapon primary = glabrezu.Body.PrimaryHand.MaybeWeapon;
            ItemEntityWeapon[] limbs = glabrezu.Body.AdditionalLimbs
                .Select(value => value.MaybeWeapon)
                .Where(value => value != null).ToArray();
            ItemEntityWeapon[] all = new[] { primary }.Concat(limbs)
                .Where(value => value != null).ToArray();
            string[] identities = all
                .Select(value => value.Blueprint.AssetGuid)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            // The pincers are the printed 19-20 threat and the rend carriers,
            // so they are identified by their own damage dice rather than by
            // position in the limb list.
            ItemEntityWeapon[] pincers = all.Where(value =>
                value.Blueprint.Type != null &&
                value.Blueprint.Type.CriticalRollEdge ==
                    GlabrezuRules38Policy.PincerCriticalRollEdge).ToArray();
            ItemEntityWeapon[] others = all.Except(pincers).ToArray();
            Mesh mesh;
            string[] bones;
            Texture2D albedo;
            string status;
            bool visual = PteranodonAssetRuntime.TryGetGlabrezuVisual38(
                GlabrezuVisualPolicy38.Glabrezu, out mesh, out bones,
                out albedo, out status);
            string view = ExpandedSummoningPteranodonViewPatch.DescribeView(
                glabrezu.View);
            var grab = SummonGrabComponent.Find(glabrezu);
            string[] absentRoutines = glabrezu.Descriptor.Progression.Features
                .Enumerable.Select(value => value.Blueprint.name)
                .Concat(glabrezu.Descriptor.Buffs.Enumerable.Select(value =>
                    value.Blueprint.name))
                .Where(value => value.IndexOf("Pounce",
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    value.IndexOf("Grab",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            // The printed reach is the weapon TYPE's configured attack range.
            // The live EQUIPPED value is not the same number on a large body -
            // the released Sprint 29 Huge giants record exactly this
            // distinction, asserting the configured range and the approach
            // radius rather than the equipped figure - so the assertion is on
            // the configured range plus the fact that all five limbs share one
            // live reach, and the equipped value and the body's own reach stat
            // are recorded as evidence rather than pinned to a derived number.
            int[] equipped = all.Select(value => value.AttackRange.Value)
                .ToArray();
            bool exact = all.Length == 5 && identities.Length == 3 &&
                pincers.Length == GlabrezuRules38Policy.RendPincerCount &&
                others.Length == 3 &&
                // Sprint 4: every limb type is the native minimum (not a
                // ProperFlanking2 reach weapon), and SummonReachRange4 keeps
                // the one shared live reach the Sprint 38 record observed.
                all.All(value => value.Blueprint.Type != null &&
                    value.Blueprint.Type.AttackRange.Value ==
                        SummonReachRepresentation4.NativeMinWeaponRangeFeet) &&
                equipped.Distinct().Count() == 1 &&
                all.All(value => SummonReachRepresentation4.Find(glabrezu.Blueprint.AssetGuid,
                    value.Blueprint.AssetGuid)?.LiveRangeAfterFeet == value.AttackRange.Value) &&
                others.All(value => value.Blueprint.Type == null ||
                    value.Blueprint.Type.CriticalRollEdge == 20) &&
                grab == null && absentRoutines.Length == 0 &&
                visual && mesh != null && albedo != null &&
                glabrezu.Descriptor.State.Size == Size.Huge &&
                view.StartsWith("visual:attached;",
                    StringComparison.Ordinal);
            check("core-five-primary-attacks", exact, new JObject {
                ["owner"] = glabrezu.UniqueId,
                ["blueprint"] = glabrezu.Blueprint.AssetGuid,
                ["size"] = glabrezu.Descriptor.State.Size.ToString(),
                ["limbCount"] = all.Length,
                ["distinctWeaponIdentities"] = new JArray(identities),
                ["configuredReachFeet"] = new JArray(all.Select(value =>
                    (int?)value.Blueprint.Type?.AttackRange.Value)),
                ["equippedReachFeet"] = new JArray(equipped),
                ["bodyReachFeet"] = glabrezu.Stats.Reach.ModifiedValue,
                ["printedReachFeet"] = GlabrezuRules38Policy.ReachFeet,
                ["reachMeaning"] =
                    "Sprint 38 recorded a live equipped 22 feet against the printed 15 feet. Sprint 4 keeps that live value exactly (an open owner question, not adjusted here) while every limb type is the native minimum, so ProperFlanking2 no longer gives these natural attacks a reach-weapon dead zone. All five limbs share one live reach, so no limb is short.",
                ["pincerCount"] = pincers.Length,
                ["printedPincerCount"] =
                    GlabrezuRules38Policy.RendPincerCount,
                ["criticalRollEdges"] = new JArray(all.Select(value =>
                    (int?)value.Blueprint.Type?.CriticalRollEdge)),
                ["printedPincerCriticalRollEdge"] =
                    GlabrezuRules38Policy.PincerCriticalRollEdge,
                ["criticalEdgeMeaning"] =
                    "BlueprintWeaponType.CriticalRollEdge is the lowest roll that threatens; the sheet renders 21 minus the edge, so 19 reads as 19-20.",
                ["grabProfile"] = grab == null ? null : "present",
                ["pounceOrGrabFacts"] = new JArray(absentRoutines),
                ["view"] = view,
                ["visualStatus"] = status,
                ["visualPublished"] = status == "visual:published",
                ["visualFailureMeaning"] =
                    status == "donor-visual:missing-mesh-file"
                        ? "The asset files are not in the installed mod; packaging, not the mesh."
                        : status == "donor-visual:invalid-mesh-data"
                            ? "The asset files are installed but failed the anatomy, rig or albedo contract."
                            : null,
                ["meshVertices"] = mesh?.vertexCount,
                ["albedoWidth"] = albedo?.width,
                ["visibleArms"] = GlabrezuVisualPolicy38.VisibleArms,
                ["articulatedArmChains"] =
                    GlabrezuVisualPolicy38.ArticulatedArmChains,
                ["projectAbilities"] = new JArray(glabrezu.Descriptor
                    .Abilities.Enumerable.Select(value => value.Blueprint.name)
                    .Where(value => value.StartsWith("KMG_",
                        StringComparison.Ordinal))
                    .OrderBy(value => value, StringComparer.Ordinal))
            }, "The live Glabrezu fields five primary natural attacks from three weapon identities at the one printed configured reach on its own attached four-armed body, the two pincers threaten on 19-20 while the claws and bite do not, and it carries no Pounce or Grab routine.");
        }

        /// <summary>
        /// The printed defensive profile on the live unit. Each value is
        /// compared against the printed one, and the two-good-saves derivation
        /// is recorded beside the saves so a reader can see why Reflex is low:
        /// a PF1 outsider has TWO good saves, and this creature's are Fortitude
        /// and Will.
        /// </summary>
        private static void GlabrezuCoreDefenses38(UnitEntityData glabrezu,
            Action<string, bool, JObject, string> check)
        {
            int fortitude = glabrezu.Stats.SaveFortitude.ModifiedValue;
            int reflex = glabrezu.Stats.SaveReflex.ModifiedValue;
            int will = glabrezu.Stats.SaveWill.ModifiedValue;
            // Spell resistance is not a character stat: it is a unit part the
            // engine asks for a value in the context of one incoming spell.
            // The released hound profile already reads it that way, through a
            // real installed SR consumer, and that is reused here rather than
            // inventing a second way to ask.
            var missile = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                BlueprintBootstrap.Library, FcbMagicMissileGuid,
                "Sprint 38 real SR consumer");
            var context = new AbilityExecutionContext(
                new AbilityData(missile, glabrezu.Descriptor),
                new AbilityParams { CasterLevel = 1, SpellLevel = 1 },
                new TargetWrapper(glabrezu), null);
            var resistanceCheck = context.TriggerRule(
                new RuleSpellResistanceCheck(context, glabrezu));
            int resistance = glabrezu.Get<UnitPartSpellResistance>()
                ?.GetValue(context) ?? 0;
            int speed = glabrezu.Stats.Speed.ModifiedValue;
            int perception = glabrezu.Stats.SkillPerception.ModifiedValue;
            // The live Perception reads two above the printed 26, and the
            // modifier stack says exactly why: the installed native Persuasive
            // feat - which this creature's printed feat list includes - adds
            // +2 to PERCEPTION in this game as well as to Persuasion. The
            // printed line was computed without that implementation. Nothing
            // is subtracted to match the printed number; the whole stack is
            // recorded, the racial contribution is asserted at the printed +8,
            // and the excess is asserted to come from that exact feat and
            // nothing else.
            var perceptionModifiers = new JArray();
            int racialPerception = 0;
            int persuasivePerception = 0;
            int unattributedPerception = 0;
            foreach (ModifiableValue.Modifier modifier in
                glabrezu.Stats.SkillPerception.Modifiers)
            {
                if (modifier == null) continue;
                string source = modifier.Source?.Blueprint?.AssetGuid;
                if (modifier.ModDescriptor == ModifierDescriptor.Racial)
                    racialPerception += modifier.ModValue;
                else if (source ==
                        GlabrezuRules38Policy.NativePersuasiveFeatGuid)
                    persuasivePerception += modifier.ModValue;
                else unattributedPerception += modifier.ModValue;
                perceptionModifiers.Add(new JObject {
                    ["value"] = modifier.ModValue,
                    ["descriptor"] = modifier.ModDescriptor.ToString(),
                    ["sourceFact"] = modifier.Source?.Blueprint?.name,
                    ["sourceFactGuid"] = source,
                    ["sourceComponent"] = modifier.SourceComponent
                });
            }
            bool exact = fortitude == GlabrezuRules38Policy.PrintedFortitude &&
                reflex == GlabrezuRules38Policy.PrintedReflex &&
                will == GlabrezuRules38Policy.PrintedWill &&
                resistance == GlabrezuRules38Policy.SpellResistance &&
                resistanceCheck.HasSpellResistance &&
                speed == GlabrezuRules38Policy.SpeedFeet &&
                racialPerception ==
                    GlabrezuRules38Policy.RacialPerceptionBonus &&
                persuasivePerception == GlabrezuRules38Policy
                    .NativePersuasivePerceptionBonus &&
                unattributedPerception == 0 &&
                perception == GlabrezuRules38Policy.PrintedPerception +
                    GlabrezuRules38Policy.NativePersuasivePerceptionBonus;
            check("core-defenses", exact, new JObject {
                ["owner"] = glabrezu.UniqueId,
                ["fortitude"] = fortitude,
                ["printedFortitude"] = GlabrezuRules38Policy.PrintedFortitude,
                ["reflex"] = reflex,
                ["printedReflex"] = GlabrezuRules38Policy.PrintedReflex,
                ["will"] = will,
                ["printedWill"] = GlabrezuRules38Policy.PrintedWill,
                ["baseFortitude"] = GlabrezuRules38Policy.BaseFortitude,
                ["baseReflex"] = GlabrezuRules38Policy.BaseReflex,
                ["baseWill"] = GlabrezuRules38Policy.BaseWill,
                ["goodSaveBase"] = GlabrezuRules38Policy.GoodSaveBase,
                ["poorSaveBase"] = GlabrezuRules38Policy.PoorSaveBase,
                ["saveDerivation"] =
                    "A PF1 outsider has TWO good saves, not three. For 12 hit dice a good save bases at 2 + HD/2 = 8 and a poor one at HD/3 = 4; this creature's good saves are Fortitude and Will, so Reflex is the poor one and is low by design.",
                ["spellResistance"] = resistance,
                ["printedSpellResistance"] =
                    GlabrezuRules38Policy.SpellResistance,
                ["nativeSrConsumerSeesResistance"] =
                    resistanceCheck.HasSpellResistance,
                ["speed"] = speed,
                ["printedSpeedFeet"] = GlabrezuRules38Policy.SpeedFeet,
                ["perception"] = perception,
                ["printedPerception"] =
                    GlabrezuRules38Policy.PrintedPerception,
                ["perceptionAbovePrinted"] = perception -
                    GlabrezuRules38Policy.PrintedPerception,
                ["racialPerceptionModifier"] = racialPerception,
                ["printedRacialPerceptionBonus"] =
                    GlabrezuRules38Policy.RacialPerceptionBonus,
                ["nativePersuasivePerceptionModifier"] = persuasivePerception,
                ["expectedNativePersuasivePerceptionBonus"] =
                    GlabrezuRules38Policy.NativePersuasivePerceptionBonus,
                ["unattributedPerceptionModifier"] = unattributedPerception,
                ["expectedLivePerception"] =
                    GlabrezuRules38Policy.PrintedPerception +
                    GlabrezuRules38Policy.NativePersuasivePerceptionBonus,
                ["perceptionDisclosure"] =
                    "The installed native Persuasive feat adds +2 Perception in this game as well as its Persuasion bonus, and this creature's printed feat list includes Persuasive. The printed +26 was computed without that implementation, so the live value is 28. Nothing is subtracted to match the printed number; the difference is disclosed.",
                ["perceptionModifiers"] = perceptionModifiers,
                ["perceptionRanks"] = GlabrezuRules38Policy.SkillRanks,
                ["hitPoints"] = glabrezu.Stats.HitPoints.ModifiedValue,
                ["armourClass"] = glabrezu.Stats.AC.ModifiedValue
            }, "The live Glabrezu carries the printed saves, spell resistance, speed and a Perception of exactly the printed 26 plus the installed Persuasive feat's own measured +2 and nothing else, with the two-good-saves derivation and the whole modifier stack recorded beside them.");
        }

        /// <summary>
        /// The printed feats and the printed constant sense, each present on
        /// the live unit by its own identity. Vital Strike is the one worth
        /// naming: the installed feature carries a base-attack prerequisite of
        /// six, and this creature's printed base attack is twelve, so the
        /// prerequisite is met rather than bypassed.
        /// </summary>
        private static void GlabrezuCoreFeats38(UnitEntityData glabrezu,
            Action<string, bool, JObject, string> check)
        {
            var seeing = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                BlueprintBootstrap.Library,
                ExpandedSummoningSpecialBuilder.NativeTrueSeeingBuff38,
                "native constant true seeing effect");
            string[] features = glabrezu.Descriptor.Progression.Features
                .Enumerable.Select(value => value.Blueprint.name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            bool vitalStrike = features.Any(value => value.IndexOf(
                "VitalStrike", StringComparison.OrdinalIgnoreCase) >= 0);
            bool greatCleave = features.Any(value => value.IndexOf(
                "GreatCleave", StringComparison.OrdinalIgnoreCase) >= 0);
            bool powerAttack = features.Any(value => value.IndexOf(
                "PowerAttack", StringComparison.OrdinalIgnoreCase) >= 0);
            bool constantSense = glabrezu.Buffs.GetBuff(seeing) != null;
            int baseAttack = glabrezu.Stats.BaseAttackBonus.ModifiedValue;
            bool exact = vitalStrike && greatCleave && powerAttack &&
                constantSense &&
                baseAttack >= GlabrezuRules38Policy.GlabrezuHitDice;
            check("core-feats-and-constant-sense", exact, new JObject {
                ["owner"] = glabrezu.UniqueId,
                ["vitalStrike"] = vitalStrike,
                ["greatCleave"] = greatCleave,
                ["powerAttack"] = powerAttack,
                ["baseAttackBonus"] = baseAttack,
                ["printedBaseAttack"] = GlabrezuRules38Policy.GlabrezuHitDice,
                ["vitalStrikePrerequisite"] =
                    "The installed Vital Strike feature requires a base attack bonus of 6; this creature's printed base attack is 12, so the prerequisite is met rather than bypassed.",
                ["constantTrueSeeing"] = constantSense,
                ["trueSeeingBlueprint"] = seeing.name,
                ["trueSeeingGuid"] = seeing.AssetGuid,
                ["constantSenseMeaning"] =
                    "A spell-like ability has no material component, so the costly-component boundary does not reach it: the 250 gp ointment belongs to the cast spell, not to this constant sense. The native effect buff is carried on the unit, which is how the installed game expresses a permanent creature sense.",
                ["features"] = new JArray(features)
            }, "The live Glabrezu carries plain Vital Strike with its prerequisite met, Great Cleave, Power Attack, and the printed constant true seeing as the installed native effect.");
        }

        /// <summary>
        /// The brain acts with real weapon attacks. This creature's one daily
        /// control effect and its at-will bursts can all legitimately decline,
        /// and what must never happen is a stall: with an unkillable adjacent
        /// enemy the fallback is the native attack action.
        ///
        /// <para>The released Sprint 37 brain preparation and frame driver are
        /// reused unchanged. Their comments record, in order, the seven
        /// observations it took to learn that group memory, mutual combat
        /// enrolment and an exact-owner AI tick are all required - none of that
        /// is re-derived here.</para>
        /// </summary>
        private IEnumerable<int> GlabrezuAttackFallback38(
            UnitEntityData glabrezu, UnitEntityData target,
            Action<string, bool, JObject, string> check)
        {
            var stunResource = (BlueprintAbilityResource)
                ExpandedSummoningBlueprints.Registered.BySymbol[
                    ExpandedSummoningSpecialBuilder.Glabrezu38Prefix +
                    "PowerWordStunResource"];
            target.Stats.AC.BaseValue = -100;
            target.Stats.HitPoints.BaseValue = 100000;
            PlaceExpandedSummoningUnit(target, glabrezu.Position +
                Vector3.right *
                (glabrezu.Corpulence + target.Corpulence + 0.5f));
            JObject brain = VrockPrepareBrain37(glabrezu, target);
            var attacks = new List<RuleAttackWithWeapon>();
            var watcher = new VrockAttackWatcher37(glabrezu, attacks);
            EventBus.Subscribe(watcher);
            var commands = new JArray();
            try
            {
                foreach (int frame in VrockDriveBrain37(glabrezu,
                    () => attacks.Count > 0, 40, brain)) yield return frame;
                foreach (UnitCommand command in glabrezu.Commands.Raw
                    .Where(value => value != null).ToArray())
                    commands.Add(Sprint27AiCommand(command));
            }
            finally
            {
                EventBus.Unsubscribe(watcher);
                Sprint28SetBrainActivePreservingActions(glabrezu, false);
                glabrezu.Commands.InterruptAll(true);
                glabrezu.Commands.RemoveFinishedAndUpdateQueue();
            }
            check("core-attack-fallback", attacks.Count > 0, new JObject {
                ["owner"] = glabrezu.UniqueId,
                ["target"] = target.UniqueId,
                ["brain"] = brain,
                ["attackRules"] = attacks.Count,
                ["attackWeapons"] = new JArray(attacks.Select(value =>
                    value.Weapon?.Blueprint?.AssetGuid).Distinct()),
                ["commands"] = commands,
                ["powerWordStunRemaining"] = glabrezu.Descriptor.Resources
                    .GetResourceAmount(stunResource)
            }, "The brain reaches real weapon attacks rather than stalling on a spell it has declined.");
        }
    }
}
