using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> ReviewIceDevilCombat40(ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var trace = new InfernalEvents34();
            var cells = new JArray();
            var receipt = new JObject { ["cells"] = cells, ["events"] = trace.Rows,
                ["phase"] = ExpandedSummoningNativeProfilePolicy.Sprint40IceDevilCombatPhase };
            Action<string, bool, JObject, string> check = (id, pass, row, why) => {
                row["cell"] = id; cells.Add(row); Sprint25Check("sprint40-" + id, pass, row, why);
                if (!pass) throw new InvalidOperationException("Sprint40 combat: " + id);
            };
            var target = fixture.Hostile;
            int bab = target.Stats.BaseAttackBonus.BaseValue;
            bool fearImmunityAdded = false;
            try
            {
                target.Descriptor.State.AddConditionImmunity(UnitCondition.Frightened);
                fearImmunityAdded = true;
                var owner = CastExpandedSummoningOwnTier(fixture, IceDevilVisualPolicy40.Key);
                owned.Add(owner); Sprint28SetBrainActivePreservingActions(owner, false);
                foreach (int frame in Outsider30NativeAppearance(owner, owner.Blueprint.name)) yield return frame;
                var profile = IceDevilMartialProfile40.For(owner);
                JObject hostility = Nalfeshnee39EstablishHostility(fixture, owner, target);
                check("combat-hostility", owner.IsEnemy(target) && target.IsEnemy(owner) &&
                    !ReferenceEquals(owner.Group, target.Group), hostility,
                    "Exact mutually hostile disposable actors, using the released recipient group repair.");
                foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
                foreach (int frame in Sprint22SetPowerAttack(owner, true)) yield return frame;
                var pa = Sprint29WeaponRule(owner, owner.Body.PrimaryHand.MaybeWeapon);
                check("spear-power-attack", (int)pa["attack"] == 17 &&
                    (int)pa["damageBonus"] == 22, pa,
                    "Native BAB14 Power Attack pays -4 attack for +12 two-handed spear damage; no observer damage bonus.");
                foreach (int frame in Sprint22SetPowerAttack(owner, false)) yield return frame;
                foreach (int frame in InfernalSingleAndOpportunity34(fixture, owner, trace,
                    check, "ice-devil-spear", 3)) yield return frame;
                foreach (int frame in InfernalCharge34(fixture, owner, check, "ice-devil-spear"))
                    yield return frame;

                owner.Commands.InterruptAll(true); owner.Commands.RemoveFinishedAndUpdateQueue();
                var spear = owner.Body.PrimaryHand.MaybeWeapon;
                var partyItems = Game.Instance.Player.Inventory.Items.ToArray();
                var partyCounts = partyItems.Select(x => x.Count).ToArray();
                target.Stats.BaseAttackBonus.BaseValue = 50;
                var attempts = new JArray(); bool disarmed = false;
                for (int attempt = 0; attempt < 8 && !disarmed; attempt++)
                {
                    var rule = Rulebook.Trigger(new RuleCombatManeuver(target, owner,
                        CombatManeuver.Disarm));
                    attempts.Add(new JObject { ["success"] = rule.Success,
                        ["partial"] = rule.IsPartialSuccess });
                    disarmed = rule.Success && owner.Buffs.GetBuff(
                        BlueprintRoot.Instance.SystemMechanics.DisarmMainHandBuff) != null;
                }
                bool retained = spear != null && ReferenceEquals(spear.Owner, owner.Descriptor) &&
                    !spear.IsLootable && spear.IsNonRemovable &&
                    Sprint28PartyInventoryExact(partyItems, partyCounts);
                check("native-disarm-owned-gear", disarmed && retained &&
                    ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon?.Blueprint, profile.Bite),
                    new JObject { ["attempts"] = attempts, ["gearRetained"] = retained,
                        ["fallback"] = owner.Body.PrimaryHand.MaybeWeapon?.Blueprint?.AssetGuid,
                        ["directNativeManeuverConsumer"] = true, ["forcedRolls"] = 0 },
                    "Real native Disarm disables the spear without transferring the exact owned item; native empty hand selects the bite.");
                InfernalCombatLane34(fixture, owner, target, 3);
                int first = trace.Rows.Count;
                UnitAttack bite = null; bool biteHit = false;
                for (int attempt = 0; attempt < 6 && !biteHit; attempt++)
                {
                    bite = new UnitAttack(target) { IsSingleAttack = true, ForceFullAttack = false };
                    owner.Commands.Run(bite);
                    foreach (int frame in WaitInfernalCommand34(bite, () => true, 25)) yield return frame;
                    biteHit = trace.Rows.Skip(first).Any(x => (string)x["event"] == "attack" &&
                        (string)x["owner"] == owner.UniqueId &&
                        (string)x["weapon"] == profile.Bite.AssetGuid && (bool?)x["hit"] == true);
                }
                var biteEvents = trace.Rows.Skip(first).ToArray();
                var attacks = biteEvents.Where(x => (string)x["event"] == "attack" &&
                    (string)x["owner"] == owner.UniqueId).ToArray();
                bool noAttempt = !biteEvents.Any(x => (string)x["event"] == "save" &&
                    (string)x["owner"] == owner.UniqueId && (int?)x["dc"] == 23);
                check("bite-has-no-slow", bite.Result == UnitCommand.ResultType.Success &&
                    biteHit && attacks.Length > 0 && attacks.All(x =>
                        (string)x["weapon"] == profile.Bite.AssetGuid) && noAttempt,
                    new JObject { ["events"] = new JArray(biteEvents), ["noSlowSave"] = noAttempt },
                    "Real standard bites include an observed hit and emit no supernatural Slow attempt; no missed-only implication can pass.");
                first = trace.Rows.Count;
                var full = new UnitAttack(target) { IsSingleAttack = false, ForceFullAttack = true };
                owner.Commands.Run(full);
                foreach (int frame in WaitInfernalCommand34(full, () => true, 25)) yield return frame;
                var fallbackEvents = trace.Rows.Skip(first).Where(x => (string)x["event"] == "attack" &&
                    (string)x["owner"] == owner.UniqueId).ToArray();
                check("disarmed-native-full-routine", fallbackEvents.Length == 2 &&
                    fallbackEvents.Count(x => (string)x["weapon"] == profile.Bite.AssetGuid) == 1 &&
                    fallbackEvents.Count(x => (string)x["weapon"] == profile.Tail.AssetGuid) == 1,
                    new JObject { ["attacks"] = new JArray(fallbackEvents),
                        ["command"] = Sprint27AiCommand(full) },
                    "Disarmed native full attack retains one bite and one tail, without spear iteratives or a duplicate fallback bite.");
                // Native Haste supplies its own extra attack. This is effect
                // setup, not a claimed Haste casting/action-cost observation.
                // The preceding real full attack consumes nearly all of the
                // one-round native Disarm duration in RTWP. Isolate that case,
                // then establish a fresh Disarm through the same real native
                // maneuver so the hasted command plans from the fallback hand.
                owner.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.DisarmMainHandBuff)?.Remove();
                var hasteDisarmAttempts = new JArray(); bool hasteDisarmed = false;
                for (int attempt = 0; attempt < 8 && !hasteDisarmed; attempt++)
                {
                    var rule = Rulebook.Trigger(new RuleCombatManeuver(target, owner,
                        CombatManeuver.Disarm));
                    hasteDisarmAttempts.Add(new JObject { ["success"] = rule.Success,
                        ["partial"] = rule.IsPartialSuccess });
                    hasteDisarmed = rule.Success && owner.Buffs.GetBuff(
                        BlueprintRoot.Instance.SystemMechanics.DisarmMainHandBuff) != null;
                }
                var haste = IceDevilNativeHaste40();
                if (!hasteDisarmed ||
                    !ReferenceEquals(owner.Body.PrimaryHand.MaybeWeapon?.Blueprint, profile.Bite))
                    throw new InvalidOperationException("Unobserved precondition: fresh native Disarm failed before Haste fallback case.");
                var context = new MechanicsContext(owner, owner.Descriptor, haste, null, new TargetWrapper(owner));
                context.Params.CasterLevel = 13; context.Params.SpellLevel = 3;
                var hasted = owner.Buffs.AddBuff(haste, context, TimeSpan.FromSeconds(30));
                try
                {
                    first = trace.Rows.Count;
                    var hastedFull = new UnitAttack(target) { IsSingleAttack = false, ForceFullAttack = true };
                    owner.Commands.Run(hastedFull);
                    foreach (int frame in WaitInfernalCommand34(hastedFull, () => true, 25)) yield return frame;
                    var hasteEvents = trace.Rows.Skip(first).Where(x => (string)x["event"] == "attack" &&
                        (string)x["owner"] == owner.UniqueId).ToArray();
                    check("native-haste-disarmed-fallback", hasted?.IsTurnedOn == true && hasteEvents.Length == 3 &&
                        hasteEvents.Count(x => (string)x["weapon"] == profile.Bite.AssetGuid) == 2 &&
                        hasteEvents.Count(x => (string)x["weapon"] == profile.Tail.AssetGuid) == 1,
                        new JObject { ["attacks"] = new JArray(hasteEvents),
                            ["nativeHasteBuff"] = haste.AssetGuid, ["observerExtraAttacks"] = 0,
                            ["freshNativeDisarmAttempts"] = hasteDisarmAttempts,
                            ["nativeBuffSetupNotCastProof"] = true },
                        "Native Haste legitimately repeats the primary bite; deduplication removes only the separate duplicate limb and preserves the real extra attack.");
                }
                finally { hasted?.Remove(); }
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner });
            }
            finally
            {
                trace.Dispose(); DisposeExpandedSummoningUnits(fixture.Created, owned);
                target.Stats.BaseAttackBonus.BaseValue = bab;
                if (fearImmunityAdded) target.Descriptor.State.RemoveConditionImmunity(UnitCondition.Frightened);
                ResetExpandedSummoningHostile(fixture);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(_request.EvidenceDirectory,
                    "sprint40-ice-devil-combat.json"), receipt.ToString(Formatting.Indented));
            }
            foreach (int frame in ReviewSprint28Cleave(fixture, reuseHostile: true,
                selectedKeys: new[] { IceDevilVisualPolicy40.Key },
                selectedPrefix: "s40-", selectedOrdinary: true)) yield return frame;
        }
    }
}
