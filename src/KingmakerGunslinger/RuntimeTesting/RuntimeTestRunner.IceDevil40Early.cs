using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Assets;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Focused development evidence only. Commands and native consumers
        // supply all effects; this observer never rolls or applies Slow.
        private IEnumerable<int> ReviewIceDevilEarly40(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var trace = new InfernalEvents34();
            var receipt = new JObject { ["phase"] =
                ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilEarlyPhase,
                ["finalQualification"] = false, ["cells"] = cells,
                ["events"] = trace.Rows };
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row);
                Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Sprint40 early: " + id);
            };
            var target = fixture.Hostile;
            int oldHp = target.Stats.HitPoints.BaseValue;
            int oldFort = target.Stats.SaveFortitude.BaseValue;
            bool fearImmunityAdded = false;
            try
            {
                // Fear has its own later consequence cell. Keep this one
                // disposable recipient stationary using the native immunity
                // counter, paired below; Slow/action conditions stay real.
                target.Descriptor.State.AddConditionImmunity(UnitCondition.Frightened);
                fearImmunityAdded = true;
                string p = ExpandedSummoningSpecialBuilder.IceDevil40Prefix;
                var planned = ExpandedSummoningIdentityCatalog.Build().Where(
                    x => x.Symbol.StartsWith(p, StringComparison.Ordinal)).ToArray();
                var missing = planned.Where(x => {
                    BlueprintScriptableObject value;
                    return !ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(
                        x.Symbol, out value) || value == null || value.GetType().Name != x.PlannedType;
                }).Select(x => x.Symbol).ToArray();
                var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Where(x => x.Creature.Key == IceDevilVisualPolicy40.Key).ToArray();
                check("dependencies", planned.Length == 26 && missing.Length == 0 &&
                    routes.Length == 1 && routes[0].ParentTier == 9 &&
                    routes[0].Multiplicity == SummonMultiplicity.One,
                    new JObject { ["privateIdentities"] = planned.Length,
                        ["missingOrWrongType"] = new JArray(missing),
                        ["routeCount"] = routes.Length,
                        ["published"] = routes.Length == 1 && SummonVisibilityCatalog.IsPublished(routes[0]) },
                    "Complete hidden dependency graph and exactly one SM IX quantity-one root.");
                var owner = CastExpandedSummoningOwnTier(fixture, IceDevilVisualPolicy40.Key);
                owned.Add(owner); Sprint28SetBrainActivePreservingActions(owner, false);
                foreach (int frame in Outsider30NativeAppearance(owner, owner.Blueprint.name)) yield return frame;
                var martial = IceDevilMartialProfile40.For(owner);
                Mesh mesh; Texture2D paint; string[] bones; string status;
                bool loaded = PteranodonAssetRuntime.TryGetIceDevilVisual40(
                    IceDevilVisualPolicy40.Key, out mesh, out bones, out paint, out status);
                string view = ExpandedSummoningPteranodonViewPatch.DescribeView(owner.View);
                check("profile-and-body", martial != null && owner.Stats.HitPoints.BaseValue ==
                    IceDevilRules40Policy.RacialBaseHitPoints &&
                    owner.Stats.Speed.ModifiedValue == 60 && loaded && mesh != null && paint != null &&
                    view.StartsWith("visual:attached;", StringComparison.Ordinal),
                    new JObject { ["unit"] = owner.UniqueId, ["hp"] = owner.Stats.HitPoints.BaseValue,
                        ["speed"] = owner.Stats.Speed.ModifiedValue, ["view"] = view,
                        ["loader"] = status, ["vertices"] = mesh?.vertexCount },
                    "Live profile and original attached geometry, not successful donor fallback.");
                check("profile-live-totals", owner.Stats.HitPoints.ModifiedValue == 161 &&
                    owner.Stats.AC.ModifiedValue == 32 && owner.Stats.SaveFortitude.ModifiedValue == 15 &&
                    owner.Stats.SaveReflex.ModifiedValue == 14 && owner.Stats.SaveWill.ModifiedValue == 12 &&
                    owner.Stats.GetStat(StatType.SkillPerception).ModifiedValue == 27 &&
                    owner.Stats.GetStat(StatType.SkillStealth).ModifiedValue == 18 &&
                    owner.Stats.GetStat(StatType.SkillMobility).ModifiedValue == 22,
                    new JObject { ["hpTotal"] = owner.Stats.HitPoints.ModifiedValue,
                        ["ac"] = owner.Stats.AC.ModifiedValue, ["fort"] = owner.Stats.SaveFortitude.ModifiedValue,
                        ["reflex"] = owner.Stats.SaveReflex.ModifiedValue, ["will"] = owner.Stats.SaveWill.ModifiedValue,
                        ["perception"] = owner.Stats.GetStat(StatType.SkillPerception).ModifiedValue,
                        ["stealth"] = owner.Stats.GetStat(StatType.SkillStealth).ModifiedValue,
                        ["mobility"] = owner.Stats.GetStat(StatType.SkillMobility).ModifiedValue },
                    "Actual adult totals include feats/class skills once, with no double Constitution HP or inherited Alertness contribution.");
                foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
                var weapons = new[] { owner.Body.PrimaryHand.MaybeWeapon }
                    .Concat(owner.Body.AdditionalLimbs.Select(x => x.MaybeWeapon))
                    .Where(x => x != null).Distinct().ToArray();
                var stats = new JArray(weapons.Select(x => Sprint29WeaponRule(owner, x)));
                Func<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon, ItemEntityWeapon> item =
                    bp => weapons.Single(x => ReferenceEquals(x.Blueprint, bp));
                var spearStats = Sprint29WeaponRule(owner, item(martial.Spear));
                var biteStats = Sprint29WeaponRule(owner, item(martial.Bite));
                var tailStats = Sprint29WeaponRule(owner, item(martial.Tail));
                check("native-weapon-contributions", (int)spearStats["damageBonus"] == 10 &&
                    (int)biteStats["damageBonus"] == 6 && (int)tailStats["damageBonus"] == 3,
                    new JObject { ["weapons"] = stats },
                    "Actual equipped native weapon-stat rules preserve two-handed spear and different printed bite/tail Strength.");

                target.Stats.HitPoints.BaseValue = 100000;
                target.Stats.SaveFortitude.BaseValue = -100;
                JObject hostility = Nalfeshnee39EstablishHostility(fixture, owner, target);
                check("recipient-hostility", owner.IsEnemy(target) && target.IsEnemy(owner) &&
                    !ReferenceEquals(owner.Group, target.Group), hostility,
                    "Exact disposable recipient is mutually hostile and in its own native group.");
                InfernalCombatLane34(fixture, owner, target, 3);
                int firstAttack = trace.Rows.Count;
                var command = new UnitAttack(target) { ForceFullAttack = true, IsSingleAttack = false };
                owner.Commands.Run(command);
                foreach (int frame in WaitInfernalCommand34(command, () =>
                    trace.Rows.Skip(firstAttack).Count(x => (string)x["event"] == "attack" &&
                        (string)x["owner"] == owner.UniqueId) >= 5, 30)) yield return frame;
                var attacks = trace.Rows.Skip(firstAttack).Where(x =>
                    (string)x["event"] == "attack" && (string)x["owner"] == owner.UniqueId).ToArray();
                check("native-full-routine", attacks.Length == 5 &&
                    attacks.Count(x => (string)x["weapon"] == martial.Spear.AssetGuid) == 3 &&
                    attacks.Count(x => (string)x["weapon"] == martial.Bite.AssetGuid) == 1 &&
                    attacks.Count(x => (string)x["weapon"] == martial.Tail.AssetGuid) == 1,
                    new JObject { ["attacks"] = new JArray(attacks),
                        ["plan"] = new JArray(command.AllAttacks.Select(x => new JObject {
                            ["weapon"] = x.Hand?.MaybeWeapon?.Blueprint?.AssetGuid,
                            ["penalty"] = x.AttackBonusPenalty })),
                        ["command"] = Sprint27AiCommand(command) },
                    "One actual full attack executes three native spear iteratives plus bite and tail, with native penalties.");
                var slow = ((Kingmaker.Blueprints.Classes.BlueprintFeature)
                    ExpandedSummoningBlueprints.Registered.BySymbol[p + "Traits"])
                    .GetComponent<IceDevilSlowOnHit40>().Slowed;
                // A natural 20 may save. Up to six real standard spear attacks
                // obtain a meaningful application without synthetic dice.
                for (int attempt = 0; attempt < 6 && target.Buffs.GetBuff(slow) == null; attempt++)
                {
                    var single = new UnitAttack(target) { IsSingleAttack = true, ForceFullAttack = false };
                    owner.Commands.Run(single);
                    foreach (int frame in WaitInfernalCommand34(single, () => true, 20)) yield return frame;
                }
                var applied = target.Buffs.GetBuff(slow);
                var saves = trace.Rows.Skip(firstAttack).Where(x => (string)x["event"] == "save" &&
                    (string)x["owner"] == owner.UniqueId && (string)x["target"] == target.UniqueId &&
                    (int?)x["dc"] == 23).ToArray();
                check("hit-slow-native-consequence", applied != null && saves.Any(x => !(bool)x["passed"]) &&
                    target.Descriptor.State.HasCondition(UnitCondition.Slowed) &&
                    target.Descriptor.State.HasCondition(UnitCondition.Staggered),
                    new JObject { ["saves"] = new JArray(saves), ["buff"] = applied?.Blueprint.AssetGuid,
                        ["source"] = applied?.Context?.MaybeCaster?.UniqueId,
                        ["deadline"] = applied?.EndTime.ToString(),
                        ["slowed"] = target.Descriptor.State.HasCondition(UnitCondition.Slowed),
                        ["staggered"] = target.Descriptor.State.HasCondition(UnitCondition.Staggered),
                        ["speed"] = target.Stats.Speed.ModifiedValue, ["forcedRolls"] = 0 },
                    "Production spear/tail hit emits Fortitude and native Slow imposes actual action/movement conditions.");
                foreach (int frame in IceDevilColdEarly40(fixture, owner, target, trace, check))
                    yield return frame;
                foreach (int frame in IceDevilWallEarly40(fixture, owner, owned, check))
                    yield return frame;
            }
            finally
            {
                trace.Dispose();
                DisposeExpandedSummoningUnits(fixture.Created, owned);
                target.Stats.HitPoints.BaseValue = oldHp;
                target.Stats.SaveFortitude.BaseValue = oldFort;
                if (fearImmunityAdded)
                    target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Frightened);
                ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "sprint40-ice-devil-early.json"), receipt.ToString(Formatting.Indented));
            }
        }

        private IEnumerable<int> IceDevilColdEarly40(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, UnitEntityData target, InfernalEvents34 trace,
            Action<string, bool, JObject, string> check)
        {
            string p = ExpandedSummoningSpecialBuilder.IceDevil40Prefix;
            foreach (string suffix in new[] { "ConeOfCold", "IceStorm" })
            {
                var bp = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[p + suffix];
                var grant = owner.Descriptor.Abilities.GetAbility(bp);
                var data = new AbilityData(grant);
                var preview = data.CalculateParams();
                Vector3 end; string lane, direction;
                Vector3 origin = Sprint24AiCombatLane(fixture.Caster, owner,
                    target, 9f, out end, out lane, out direction,
                    new Feet(20).Meters + 2f, null, 30);
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, end);
                owner.Memory.Add(target).Visible = true;
                target.Memory.Add(owner).Visible = true;
                var delivery = bp.GetComponent<Kingmaker.UnitLogic.Abilities.Components.
                    AbilityDeliverProjectile>();
                var footprint = Game.Instance.State.Units.All.Where(x => x != null &&
                    !x.Destroyed && x.IsInGame &&
                    (suffix == "ConeOfCold" ? delivery.WouldTargetUnit(data, target.Position, x) :
                        Vector3.Distance(x.Position, target.Position) <= new Feet(20).Meters + x.Corpulence))
                    .ToArray();
                bool ownedOnly = footprint.Contains(target) && footprint.All(x =>
                    ReferenceEquals(x, target));
                check("cold-owned-footprint-" + suffix, ownedOnly,
                    new JObject { ["lane"] = lane,
                        ["nativeConeDelivery"] = suffix == "ConeOfCold",
                        ["footprint"] = new JArray(footprint.Select(x => new JObject {
                            ["unit"] = x.UniqueId, ["blueprint"] = x.Blueprint.name,
                            ["exactDisposableRecipient"] = ReferenceEquals(x, target) })) },
                    "Before casting, native cone membership or conservative storm footprint contains only the exact disposable recipient; protected actors are excluded.");
                var aim = suffix == "IceStorm" ? new TargetWrapper(target.Position) :
                    new TargetWrapper(target);
                int start = trace.Rows.Count;
                bool legal = data.IsAvailable && data.CanTarget(aim);
                check("cold-target-" + suffix, legal, new JObject {
                    ["available"] = data.IsAvailable, ["canTarget"] = legal,
                    ["ability"] = bp.AssetGuid, ["preContextCasterLevel"] = preview.CasterLevel,
                    ["preContextSpellLevel"] = data.SpellLevel,
                    ["preContextDc"] = preview.DC },
                    "Real native availability and targeting for the owned at-will SLA.");
                var command = new UnitUseAbility(data, aim);
                owner.Commands.Run(command);
                foreach (int frame in WaitInfernalCommand34(command, () =>
                    trace.Rows.Skip(start).Any(x => (string)x["event"] == "damage" &&
                        (string)x["owner"] == owner.UniqueId &&
                        (string)x["target"] == target.UniqueId), 25)) yield return frame;
                var rows = trace.Rows.Skip(start).Select(x => x.DeepClone()).ToArray();
                check("cold-native-cast-" + suffix,
                    command.Result == UnitCommand.ResultType.Success && data.IsAvailable,
                    new JObject { ["command"] = Sprint27AiCommand(command),
                        ["events"] = new JArray(rows), ["availableAgain"] = data.IsAvailable },
                    "Actual native cast emits damage and remains at will; packet/save/SR observations retained for review.");
            }
        }

        private IEnumerable<int> IceDevilWallEarly40(ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData owner, List<UnitEntityData> owned,
            Action<string, bool, JObject, string> check)
        {
            Vector3 start, middle, end, casterPoint; JArray attempts; JObject survey;
            if (!InfernalEarlyWallLane35(fixture, owned, out start, out middle, out end,
                out casterPoint, out attempts, out survey))
                throw new InvalidOperationException("No native floor lane for Ice wall: " + attempts);
            owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
            PlaceExpandedSummoningUnit(fixture.Caster, start);
            PlaceExpandedSummoningUnit(owner, casterPoint);
            // The disposable hostile has no role in this obstruction cell.
            PlaceExpandedSummoningUnit(fixture.Hostile, casterPoint + Vector3.right * 5);
            string p = ExpandedSummoningSpecialBuilder.IceDevil40Prefix;
            var bp = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[p + "WallOfIce"];
            var sectionBp = (BlueprintUnit)ExpandedSummoningBlueprints.Registered.BySymbol[p + "WallSectionUnit"];
            var before = InfernalEarlyPath35(fixture.Caster, end);
            var prior = new HashSet<string>(Game.Instance.State.Units.Select(x => x.UniqueId));
            var data = new AbilityData(owner.Descriptor.Abilities.GetAbility(bp));
            var target = new TargetWrapper(middle);
            bool legal = data.IsAvailable && data.CanTarget(target);
            var command = new UnitUseAbility(data, target);
            owner.Commands.Run(command);
            UnitEntityData section = null;
            DateTime deadline = DateTime.UtcNow.AddSeconds(25);
            while (DateTime.UtcNow < deadline && (!command.IsFinished || section == null))
            {
                Game.Instance.IsPaused = false; yield return 0;
                section = Game.Instance.State.Units.FirstOrDefault(x => !prior.Contains(x.UniqueId) &&
                    ReferenceEquals(x.Blueprint, sectionBp));
            }
            if (section != null) owned.Add(section);
            bool obstructed = false; JObject blocked = null;
            deadline = DateTime.UtcNow.AddSeconds(15);
            while (section != null && DateTime.UtcNow < deadline && !obstructed)
            {
                yield return 0;
                blocked = InfernalEarlyPath35(fixture.Caster, end);
                obstructed = InfernalEarlyWallObstructs35(before, blocked);
            }
            var binding = section?.View?.GetComponent<InfernalWallNavmesh35>();
            check("wall-cast-and-obstruction", legal && section != null &&
                section.Stats.HitPoints.BaseValue == 39 && binding != null && obstructed,
                new JObject { ["legal"] = legal, ["command"] = Sprint27AiCommand(command),
                    ["section"] = section?.UniqueId, ["hitPoints"] = section?.Stats.HitPoints.BaseValue,
                    ["before"] = before, ["blocked"] = blocked,
                    ["visual"] = binding?.WallVisual != null, ["obstructed"] = obstructed },
                "Actual CL13 cast creates a 39 HP visible section and claims native movement nodes.");
            // Real source retirement must release the production wall and its
            // claims; the observer does not edit walkability or invoke the cut.
            DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
            JObject after = null; bool restored = false;
            deadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < deadline && !restored)
            {
                Game.Instance.EntityDestroyer.Tick(); yield return 0;
                after = InfernalEarlyPath35(fixture.Caster, end);
                restored = (section.Destroyed || !section.IsInGame) &&
                    InfernalEarlyWallRestored35(before, after);
            }
            check("wall-source-removal-restores", restored,
                new JObject { ["after"] = after, ["sourceDestroyed"] = owner.Destroyed,
                    ["sectionDestroyed"] = section.Destroyed },
                "Normal exact-source removal releases this wall and restores movement.");
        }
    }
}
