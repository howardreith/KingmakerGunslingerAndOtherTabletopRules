using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
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
        /// Power Word Stun, observed on live units: the printed close range at
        /// this creature's caster level, the single use and its refusal, the
        /// hit-point buckets at every printed boundary, the absence of any
        /// saving throw, the refusal against a stun-immune recipient, and the
        /// recorded caster level reaching both the resolving effect and the
        /// spell-resistance roll.
        ///
        /// <para>Each bucket gets its own summoned Glabrezu, because the
        /// printed ability is once per summoning and this phase refuses to
        /// restore a spent use in order to cast again - that would be
        /// observing a fixture, not the creature.</para>
        /// </summary>
        private IEnumerable<int> ReviewGlabrezuStun38(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint38GlabrezuStunPhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetDamage = target.Damage;
            Vector3 oldTargetPosition = target.Position;
            bool immunityAdded = false;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint38-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint38 glabrezu stun cell failed: " + id);
                };

            try
            {
                GlabrezuStunRange38(check);

                foreach (int frame in GlabrezuStunBuckets38(fixture, target,
                    owned, check)) yield return frame;
                foreach (int frame in GlabrezuStunImmunity38(fixture, target,
                    owned, value => immunityAdded = value, check))
                    yield return frame;
                foreach (int frame in GlabrezuStunResistance38(fixture, owned,
                    check)) yield return frame;
            }
            finally
            {
                if (immunityAdded) target.Descriptor.State
                    .RemoveConditionImmunity(UnitCondition.Stunned);
                foreach (Kingmaker.UnitLogic.Buffs.Buff buff in
                    target.Buffs.Enumerable.ToArray())
                    if (buff.Blueprint != null && buff.Blueprint.name
                        .StartsWith("KMG_", StringComparison.Ordinal))
                        buff.Remove();
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                target.Damage = oldTargetDamage;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory, "sprint38-glabrezu-stun.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        private static BlueprintAbility GlabrezuStunAbility38()
        {
            return (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                .BySymbol[ExpandedSummoningSpecialBuilder.Glabrezu38Prefix +
                    "PowerWordStun"];
        }

        private static BlueprintAbilityResource GlabrezuStunResource38()
        {
            return (BlueprintAbilityResource)ExpandedSummoningBlueprints
                .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                    .Glabrezu38Prefix + "PowerWordStunResource"];
        }

        private static BlueprintBuff GlabrezuStunCarrier38()
        {
            return (BlueprintBuff)ExpandedSummoningBlueprints.Registered
                .BySymbol[ExpandedSummoningSpecialBuilder.Glabrezu38Prefix +
                    "PowerWordStunCarrier"];
        }

        /// <summary>
        /// The printed close range at caster level 14 is 60 feet, and the
        /// installed Close CATEGORY is a flat 30 that reads no caster level.
        /// This reads the number the engine will actually use for targeting,
        /// approach, the cast and the AI - <c>BlueprintAbility.GetRange</c> -
        /// so one distance is shared by all of them rather than restated.
        /// </summary>
        private static void GlabrezuStunRange38(
            Action<string, bool, JObject, string> check)
        {
            BlueprintAbility stun = GlabrezuStunAbility38();
            int range = stun.GetRange().Value;
            check("stun-range-is-printed-close",
                stun.Range == AbilityRange.Custom &&
                range == GlabrezuRules38Policy.SelectedCloseRangeFeet &&
                range == GlabrezuRules38Policy.CloseRangeFeet(
                    GlabrezuRules38Policy.SpellLikeCasterLevel) &&
                range != GlabrezuRules38Policy.InstalledCloseCategoryFeet &&
                stun.SpellResistance && !stun.CanTargetSelf &&
                stun.CanTargetEnemies,
                new JObject {
                    ["ability"] = stun.name,
                    ["abilityGuid"] = stun.AssetGuid,
                    ["rangeCategory"] = stun.Range.ToString(),
                    ["effectiveRangeFeet"] = range,
                    ["printedCloseRangeFeet"] =
                        GlabrezuRules38Policy.SelectedCloseRangeFeet,
                    ["derivedCloseRangeFeet"] =
                        GlabrezuRules38Policy.CloseRangeFeet(
                            GlabrezuRules38Policy.SpellLikeCasterLevel),
                    ["installedCloseCategoryFeet"] =
                        GlabrezuRules38Policy.InstalledCloseCategoryFeet,
                    ["rangeDerivation"] =
                        "PF1 close range is 25 + 5 x floor(CL/2); at this creature's caster level 14 that is 60 feet. BlueprintAbility.GetRange returns CustomRange verbatim, so player targeting, approach, the cast and the AI all read the same distance.",
                    ["spellResistanceApplies"] = stun.SpellResistance,
                    ["canTargetSelf"] = stun.CanTargetSelf,
                    ["canTargetEnemies"] = stun.CanTargetEnemies
                }, "Power Word Stun carries an explicit 60-foot custom range derived from its own caster level, not the installed flat 30-foot Close category, and is spell-resistible and enemy-only.");
        }

        private sealed class GlabrezuStunWatcher38 :
            IGlobalRulebookHandler<RuleApplyBuff>,
            IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleSpellResistanceCheck>
        {
            private readonly UnitEntityData _owner;
            internal readonly List<RuleApplyBuff> Applied =
                new List<RuleApplyBuff>();
            internal readonly List<RuleSavingThrow> Saves =
                new List<RuleSavingThrow>();
            internal readonly List<RuleSpellResistanceCheck> Resistance =
                new List<RuleSpellResistanceCheck>();

            internal GlabrezuStunWatcher38(UnitEntityData owner)
            { _owner = owner; }

            private bool Mine(MechanicsContext context)
            {
                return context != null &&
                    ReferenceEquals(context.MaybeCaster, _owner);
            }

            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            { if (Mine(evt.Context)) Applied.Add(evt); }

            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            // A saving throw carries no mechanics context of its own, so the
            // owner is identified from the reason's own source fact context.
            // Any save raised by this owner at all is a failure here: the
            // printed word allows none.
            public void OnEventDidTrigger(RuleSavingThrow evt)
            { if (Mine(evt.Reason.Context)) Saves.Add(evt); }

            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt) { }
            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            { if (Mine(evt.Context)) Resistance.Add(evt); }
        }

        /// <summary>
        /// The printed hit-point buckets at and either side of every boundary,
        /// each on its own freshly summoned Glabrezu with its own single use.
        /// The last affected value is 150 and the first unaffected one is 151,
        /// and that pair is the whole point of the table: above the ceiling the
        /// printed word does nothing at all, and the use must not be spent
        /// pretending otherwise.
        /// </summary>
        private IEnumerable<int> GlabrezuStunBuckets38(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData target,
            List<UnitEntityData> owned,
            Action<string, bool, JObject, string> check)
        {
            BlueprintAbility stun = GlabrezuStunAbility38();
            BlueprintAbilityResource resource = GlabrezuStunResource38();
            BlueprintBuff carrier = GlabrezuStunCarrier38();
            var rows = new JArray();
            bool coherent = true;
            int saveCount = 0;
            int[] hitPoints = {
                GlabrezuRules38Policy.PowerWordStunLowThreshold,
                GlabrezuRules38Policy.PowerWordStunMidThreshold,
                GlabrezuRules38Policy.PowerWordStunHitPointCeiling,
                GlabrezuRules38Policy.PowerWordStunHitPointCeiling + 1
            };
            foreach (int remaining in hitPoints)
            {
                int expectedDice = GlabrezuRules38Policy
                    .PowerWordStunDiceCount(remaining);
                UnitEntityData glabrezu = CastExpandedSummoningOwnTier(
                    fixture, GlabrezuVisualPolicy38.Glabrezu);
                owned.Add(glabrezu);
                VrockQuiesce37(glabrezu);
                // Inside the printed 60-foot range, visible, and hostile:
                // every gate the real suitability reads is real here.
                PlaceExpandedSummoningUnit(target, glabrezu.Position +
                    Vector3.right *
                    (glabrezu.Corpulence + target.Corpulence + 1f));
                target.Stats.HitPoints.BaseValue = remaining;
                target.Damage = 0;
                glabrezu.Memory.Add(target).Visible = true;
                target.Memory.Add(glabrezu).Visible = true;

                int held = glabrezu.Descriptor.Resources
                    .GetResourceAmount(resource);
                bool usable = GlabrezuStunSuitability38.IsUsableRecipient(
                    glabrezu, target, stun);
                JObject readiness = VrockCastReadiness37(glabrezu, stun,
                    new TargetWrapper(target));
                var watcher = new GlabrezuStunWatcher38(glabrezu);
                EventBus.Subscribe(watcher);
                UnitUseAbility command = null;
                JObject refusal = null;
                try
                {
                    if (expectedDice > 0)
                    {
                        foreach (int frame in Sprint26Cast(glabrezu, stun,
                            new TargetWrapper(target),
                            value => command = value)) yield return frame;
                        refusal = VrockCastReadiness37(glabrezu, stun,
                            new TargetWrapper(target));
                    }
                }
                finally { EventBus.Unsubscribe(watcher); }

                RuleApplyBuff[] stuns = watcher.Applied
                    .Where(value => ReferenceEquals(value.AppliedBuff
                        ?.Blueprint, carrier)).ToArray();
                double[] roundsLeft = stuns
                    .Where(value => value.Duration.HasValue)
                    .Select(value => value.Duration.Value.TotalSeconds /
                        GameConsts.RoundDuration).ToArray();
                int after = glabrezu.Descriptor.Resources
                    .GetResourceAmount(resource);
                int[] casterLevels = stuns
                    .Select(value => value.Context == null ? -1 :
                        value.Context.Params.CasterLevel).ToArray();
                saveCount += watcher.Saves.Count;
                bool row;
                if (expectedDice > 0)
                {
                    // One finite stun whose rounds lie inside the bucket's own
                    // dice: at least the dice count and at most four times it.
                    row = usable && command != null && command.IsFinished &&
                        command.Result == UnitCommand.ResultType.Success &&
                        held == GlabrezuRules38Policy.PowerWordStunUses &&
                        after == 0 && stuns.Length == 1 &&
                        roundsLeft.Length == 1 &&
                        roundsLeft[0] >= expectedDice - 0.01 &&
                        roundsLeft[0] <= expectedDice *
                            GlabrezuRules38Policy.PowerWordStunDieSides +
                            0.01 &&
                        casterLevels.All(value => value ==
                            GlabrezuRules38Policy.SpellLikeCasterLevel) &&
                        target.Descriptor.State.HasCondition(
                            UnitCondition.Stunned) &&
                        watcher.Saves.Count == 0 &&
                        refusal != null && !(bool)refusal["isAvailable"];
                }
                else
                {
                    // Above the printed ceiling the ability is not even a
                    // usable choice, so the single use stays unspent. No cast
                    // is attempted: the shared cast helper throws on an
                    // unavailable ability, and that would say nothing.
                    row = !usable && stuns.Length == 0 &&
                        held == GlabrezuRules38Policy.PowerWordStunUses &&
                        after == GlabrezuRules38Policy.PowerWordStunUses &&
                        !target.Descriptor.State.HasCondition(
                            UnitCondition.Stunned) &&
                        watcher.Saves.Count == 0;
                }
                coherent &= row;
                rows.Add(new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["recipient"] = target.UniqueId,
                    ["recipientHitPoints"] = remaining,
                    ["expectedStunDice"] = expectedDice,
                    ["dieSides"] =
                        GlabrezuRules38Policy.PowerWordStunDieSides,
                    ["usableRecipient"] = usable,
                    ["readinessBefore"] = readiness,
                    ["command"] = command == null ? null :
                        Sprint27AiCommand(command),
                    ["appliedStuns"] = stuns.Length,
                    ["stunRoundsLeft"] = new JArray(roundsLeft),
                    ["effectCasterLevels"] = new JArray(casterLevels),
                    ["expectedCasterLevel"] =
                        GlabrezuRules38Policy.SpellLikeCasterLevel,
                    ["resourceBefore"] = held,
                    ["resourceAfter"] = after,
                    ["savingThrowsRolled"] = watcher.Saves.Count,
                    ["recipientStunned"] = target.Descriptor.State
                        .HasCondition(UnitCondition.Stunned),
                    ["refusalAfterUse"] = refusal,
                    ["coherent"] = row
                });
                foreach (Kingmaker.UnitLogic.Buffs.Buff buff in
                    target.Buffs.Enumerable.ToArray())
                    if (ReferenceEquals(buff.Blueprint, carrier))
                        buff.Remove();
            }
            check("stun-buckets-follow-current-hit-points", coherent,
                new JObject {
                    ["observations"] = rows,
                    ["lowThreshold"] =
                        GlabrezuRules38Policy.PowerWordStunLowThreshold,
                    ["midThreshold"] =
                        GlabrezuRules38Policy.PowerWordStunMidThreshold,
                    ["ceiling"] =
                        GlabrezuRules38Policy.PowerWordStunHitPointCeiling,
                    ["totalSavingThrowsRolled"] = saveCount
                }, "At 50, 100 and 150 remaining hit points the word stuns for a duration inside that bucket's own dice at caster level 14, spending its single use and refusing a second; at 151 it is not a usable choice at all and the use stays unspent. No saving throw is rolled at any threshold.");
        }

        /// <summary>
        /// A recipient the printed word cannot touch. The condition immunity is
        /// added to the hostile and removed again, which is the same
        /// reversible mutation the released fatigue fixture makes, and it is
        /// the exact gate the real suitability reads.
        /// </summary>
        private IEnumerable<int> GlabrezuStunImmunity38(
            ExpandedSummoningCorrectionFixture fixture, UnitEntityData target,
            List<UnitEntityData> owned, Action<bool> immunityAdded,
            Action<string, bool, JObject, string> check)
        {
            BlueprintAbility stun = GlabrezuStunAbility38();
            BlueprintAbilityResource resource = GlabrezuStunResource38();
            UnitEntityData glabrezu = CastExpandedSummoningOwnTier(fixture,
                GlabrezuVisualPolicy38.Glabrezu);
            owned.Add(glabrezu);
            VrockQuiesce37(glabrezu);
            PlaceExpandedSummoningUnit(target, glabrezu.Position +
                Vector3.right *
                (glabrezu.Corpulence + target.Corpulence + 1f));
            target.Stats.HitPoints.BaseValue =
                GlabrezuRules38Policy.PowerWordStunLowThreshold;
            target.Damage = 0;
            glabrezu.Memory.Add(target).Visible = true;
            target.Memory.Add(glabrezu).Visible = true;
            bool usableBefore = GlabrezuStunSuitability38.IsUsableRecipient(
                glabrezu, target, stun);
            target.Descriptor.State.AddConditionImmunity(
                UnitCondition.Stunned);
            immunityAdded(true);
            bool usableAfter = GlabrezuStunSuitability38.IsUsableRecipient(
                glabrezu, target, stun);
            bool canAttempt = GlabrezuStunSuitability38.CanAttempt(glabrezu,
                glabrezu.Blueprint, stun);
            int held = glabrezu.Descriptor.Resources
                .GetResourceAmount(resource);
            target.Descriptor.State.RemoveConditionImmunity(
                UnitCondition.Stunned);
            immunityAdded(false);
            bool usableRestored = GlabrezuStunSuitability38.IsUsableRecipient(
                glabrezu, target, stun);
            check("stun-refuses-an-immune-recipient",
                usableBefore && !usableAfter && usableRestored &&
                canAttempt &&
                held == GlabrezuRules38Policy.PowerWordStunUses,
                new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["recipient"] = target.UniqueId,
                    ["recipientHitPoints"] = target.HPLeft,
                    ["usableBeforeImmunity"] = usableBefore,
                    ["usableWithImmunity"] = usableAfter,
                    ["usableAfterImmunityRemoved"] = usableRestored,
                    ["ownerCouldStillAttempt"] = canAttempt,
                    ["resourceHeld"] = held,
                    ["whyThisMatters"] =
                        "The owner is willing and holds its one use; only the candidate is unaffectable. A suitability bound to the actor alone would admit the cast and spend the single daily use on a creature the printed spell cannot touch."
                }, "A recipient immune to the stun condition is refused by the real suitability while the owner remains able to attempt the ability, and the single use is not spent.");
            yield break;
        }

        /// <summary>
        /// Spell resistance, on two fronts.
        ///
        /// <para>Power Word Stun is spell-resistible, and its penetration is
        /// its own caster level: <c>RuleSpellResistanceCheck.SpellPenetration</c>
        /// is <c>Context.Params.CasterLevel</c>, which for this ability is the
        /// printed 14 rather than the 12 hit dice a pre-cast preview
        /// reports.</para>
        ///
        /// <para>The second front is the narrow authorized check of the
        /// reported Sprint 37 concern, against real high-resistance
        /// non-demon recipients. It is a verification, not a change: the
        /// Vrock's screech is recorded exactly as the released bytes behave,
        /// and if numerical resistance is already handled correctly nothing is
        /// altered. Power Word Stun stands beside it as the
        /// resistance-applies neighbour, so the two are measured the same
        /// way in one transaction.</para>
        /// </summary>
        private IEnumerable<int> GlabrezuStunResistance38(
            ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> owned,
            Action<string, bool, JObject, string> check)
        {
            BlueprintAbility stun = GlabrezuStunAbility38();
            var screech = (BlueprintAbility)ExpandedSummoningBlueprints
                .Registered.BySymbol[ExpandedSummoningSpecialBuilder
                    .Vrock37Prefix + "Screech"];
            UnitEntityData glabrezu = CastExpandedSummoningOwnTier(fixture,
                GlabrezuVisualPolicy38.Glabrezu);
            owned.Add(glabrezu);
            VrockQuiesce37(glabrezu);
            foreach (int frame in Outsider30NativeAppearance(glabrezu,
                glabrezu.Blueprint.name)) yield return frame;

            // Real registered high-resistance non-demon recipients: the
            // Trumpet Archon at SR 25 and the Astral Deva at SR 24, both
            // released and both above this creature's caster level, so a
            // correct penetration roll can fail and be seen to.
            var resisters = new List<UnitEntityData>();
            foreach (string key in new[] { "trumpet-archon", "astral-deva" })
            {
                UnitEntityData unit = CastExpandedSummoningOwnTier(fixture,
                    key);
                owned.Add(unit);
                VrockQuiesce37(unit);
                resisters.Add(unit);
            }

            var rows = new JArray();
            bool coherent = true;
            foreach (UnitEntityData resister in resisters)
            {
                var stunContext = new AbilityExecutionContext(
                    new AbilityData(stun, glabrezu.Descriptor),
                    new AbilityParams {
                        CasterLevel =
                            GlabrezuRules38Policy.SpellLikeCasterLevel,
                        SpellLevel =
                            GlabrezuRules38Policy.PowerWordStunSpellLevel },
                    new TargetWrapper(resister), null);
                RuleSpellResistanceCheck stunRule = stunContext.TriggerRule(
                    new RuleSpellResistanceCheck(stunContext, resister));
                int resistance = resister.Get<UnitPartSpellResistance>()
                    ?.GetValue(stunContext) ?? 0;
                bool row = stun.SpellResistance &&
                    stunRule.HasSpellResistance &&
                    stunRule.SpellPenetration ==
                        GlabrezuRules38Policy.SpellLikeCasterLevel &&
                    resistance > GlabrezuRules38Policy.SpellLikeCasterLevel;
                coherent &= row;
                rows.Add(new JObject {
                    ["recipient"] = resister.UniqueId,
                    ["recipientBlueprint"] = resister.Blueprint.name,
                    ["recipientSpellResistance"] = resistance,
                    ["isDemon"] = SummonDemonIdentityCatalog.IsDemonKey(
                        resister.Get<UnitPartRelease143RosterReceipt>()?.Key),
                    ["stunResistanceApplies"] = stun.SpellResistance,
                    ["ruleSawResistance"] = stunRule.HasSpellResistance,
                    ["spellPenetration"] = stunRule.SpellPenetration,
                    ["expectedPenetration"] =
                        GlabrezuRules38Policy.SpellLikeCasterLevel,
                    ["preCastEstimate"] =
                        GlabrezuRules38Policy.PreCastEstimatedCasterLevel,
                    ["penetrationMeaning"] =
                        "RuleSpellResistanceCheck.SpellPenetration is Context.Params.CasterLevel. A pre-cast preview reports the 12 hit dice because RuleCalculateAbilityParams honours only ContextSetAbilityParams; the resolving context carries the printed 14, and that is the number the roll uses.",
                    ["coherent"] = row
                });
            }
            check("stun-resistance-uses-printed-caster-level", coherent,
                new JObject {
                    ["owner"] = glabrezu.UniqueId,
                    ["observations"] = rows
                }, "Power Word Stun is spell-resistible and its penetration is the printed caster level 14, measured against real registered recipients whose resistance is above it.");

            // The narrow authorized verification. Nothing is changed by it.
            var screechRows = new JArray();
            foreach (UnitEntityData resister in resisters)
            {
                var screechContext = new AbilityExecutionContext(
                    new AbilityData(screech, glabrezu.Descriptor),
                    new AbilityParams {
                        CasterLevel =
                            VrockRules37Policy.SpellLikeCasterLevel,
                        SpellLevel = 1 },
                    new TargetWrapper(resister), null);
                RuleSpellResistanceCheck screechRule =
                    screechContext.TriggerRule(new RuleSpellResistanceCheck(
                        screechContext, resister));
                screechRows.Add(new JObject {
                    ["recipient"] = resister.UniqueId,
                    ["recipientBlueprint"] = resister.Blueprint.name,
                    ["recipientSpellResistance"] = resister
                        .Get<UnitPartSpellResistance>()
                        ?.GetValue(screechContext) ?? 0,
                    ["screechBlueprintDeclaresResistance"] =
                        screech.SpellResistance,
                    ["ruleSawResistance"] = screechRule.HasSpellResistance,
                    ["spellPenetration"] = screechRule.SpellPenetration,
                    ["ruleResult"] = screechRule.IsSpellResisted
                });
            }
            // The released screech declares no spell resistance, which is
            // what a sonic area effect should declare, so the engine never
            // triggers the check for it during execution:
            // AbilityExecutionProcess.DoApplyEffect raises
            // RuleSpellResistanceCheck only for an ability that declares it.
            // That is the whole of the reported concern, and it is already
            // correct, so this cell records it and changes nothing.
            check("vrock-screech-numerical-resistance-unchanged",
                !screech.SpellResistance,
                new JObject {
                    ["observations"] = screechRows,
                    ["conclusion"] = screech.SpellResistance
                        ? "The released screech declares spell resistance; this is a finding to report, not something this sprint may change."
                        : "The released screech declares no spell resistance, so the engine never raises a resistance check for it. The reported concern does not reproduce and nothing is changed.",
                    ["scope"] =
                        "One narrow authorized verification of the reported Sprint 37 concern, with Power Word Stun measured the same way as the resistance-applies neighbour. No global resistance bypass and no Sprint 37 replay.",
                    ["authority"] =
                        "Sprint 38 correction mission: verify, and if the screech already handles numerical resistance correctly, change nothing."
                }, "The narrow authorized check: the released Vrock screech declares no spell resistance, so no resistance check is raised for it, and nothing is changed.");
        }
    }
}
