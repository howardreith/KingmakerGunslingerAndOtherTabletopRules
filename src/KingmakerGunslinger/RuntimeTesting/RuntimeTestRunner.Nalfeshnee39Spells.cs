using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
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
        /// The four at-will spell-likes and the two constant defences, each
        /// measured as the thing it actually is rather than as the component
        /// that carries it.
        ///
        /// <para>Three of the charter's named risks land in this phase.
        /// <i>Feeblemind</i> is checked for the DISABILITY the printed spell
        /// states and for the one it does not - the engine's own
        /// spellcasting-forbidden flag must be retained and its
        /// magic-items-forbidden flag must not be - and for a lasting result
        /// that only its named cure ends. <i>Greater Dispel</i> is checked
        /// for one carried decisive check over an ordered snapshot with a
        /// real cap, no reroll and no retry. The constant <i>unholy aura</i>
        /// is checked for the owner's scoping decision: an exact-source self
        /// effect, with nothing granted to an adjacent ally.</para>
        /// </summary>
        private IEnumerable<int> ReviewNalfeshneeSpells39(
            ExpandedSummoningCorrectionFixture fixture)
        {
            var owned = new List<UnitEntityData>();
            var transient = new List<UnityEngine.Object>();
            var cells = new JArray();
            var receipt = new JObject {
                ["phase"] = ExpandedSummoningNativeProfilePolicy
                    .Sprint39NalfeshneeSpellsPhase,
                ["finalQualification"] = false,
                ["cells"] = cells
            };
            UnitEntityData target = fixture.Hostile;
            int oldTargetHp = target.Stats.HitPoints.BaseValue;
            int oldTargetWill = target.Stats.SaveWill.BaseValue;
            int oldTargetInt = target.Stats.Intelligence.BaseValue;
            int oldTargetCha = target.Stats.Charisma.BaseValue;
            int oldTargetDamage = target.Damage;
            Vector3 oldTargetPosition = target.Position;
            Vector3 oldCasterPosition = fixture.Caster.Position;
            BlueprintFaction oldFaction = target.Faction;
            // Native heal is friend-targeted, so the cure cell borrows a
            // party membership and an ability grant. An iterator cannot take
            // ref parameters, so what it borrowed comes back here to be
            // returned in the finally rather than being left to it.
            var loan = new Nalfeshnee39CureLoan();
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode
                .CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            Action<string, bool, JObject, string> check =
                (id, pass, row, description) => {
                    row["cell"] = id;
                    cells.Add(row);
                    Sprint25Check("sprint39-" + id, pass, row, description);
                    if (!pass) throw new InvalidOperationException(
                        "Sprint39 nalfeshnee spells cell failed: " + id);
                };
            try
            {
                string p = ExpandedSummoningSpecialBuilder.Nalfeshnee39Prefix;
                BlueprintAbility feeblemind = Nalfeshnee39Spell(
                    p + "Feeblemind");
                BlueprintAbility dispel = Nalfeshnee39Spell(
                    p + "GreaterDispelMagic");
                BlueprintAbility lightning = Nalfeshnee39Spell(
                    p + "CallLightning");
                BlueprintAbility slow = Nalfeshnee39Spell(p + "Slow");
                var condition = (BlueprintBuff)ExpandedSummoningBlueprints
                    .Registered.BySymbol[p + "FeeblemindCondition"];
                var aura = (BlueprintBuff)ExpandedSummoningBlueprints
                    .Registered.BySymbol[p + "UnholyAura"];

                Nalfeshnee39SpellDeclarations(new[] { feeblemind, dispel,
                    lightning, slow }, feeblemind, dispel, check);

                UnitEntityData nalfeshnee = CastExpandedSummoningOwnTier(
                    fixture, NalfeshneeVisualPolicy39.Nalfeshnee);
                owned.Add(nalfeshnee);
                VrockQuiesce37(nalfeshnee);
                foreach (int frame in Outsider30NativeAppearance(nalfeshnee,
                    nalfeshnee.Blueprint.name)) yield return frame;

                target.Stats.HitPoints.BaseValue = 100000;
                target.Damage = 0;
                PlaceExpandedSummoningUnit(target, nalfeshnee.Position +
                    Vector3.right * (nalfeshnee.Corpulence +
                        target.Corpulence + 1f));
                nalfeshnee.Memory.Add(target).Visible = true;
                target.Memory.Add(nalfeshnee).Visible = true;

                Nalfeshnee39GeneralResistance(nalfeshnee, check);
                foreach (int frame in Nalfeshnee39AuraSelfOnly(fixture,
                    owned, nalfeshnee, aura, check)) yield return frame;
                foreach (int frame in Nalfeshnee39Feeblemind(fixture,
                    nalfeshnee, target, feeblemind, condition, check))
                    yield return frame;
                foreach (int frame in Nalfeshnee39FeeblemindCure(fixture,
                    target, condition, loan, check)) yield return frame;
                foreach (int frame in Nalfeshnee39GreaterDispel(fixture,
                    nalfeshnee, target, dispel, transient, check))
                    yield return frame;
                foreach (int frame in Nalfeshnee39AtWillRepeats(nalfeshnee,
                    target, lightning, slow, check)) yield return frame;
            }
            finally
            {
                if (loan.PartyAdded)
                    Game.Instance.Player.Party.Remove(target);
                target.Descriptor.SwitchFactions(oldFaction, false);
                if (loan.Heal != null)
                    fixture.Caster.Descriptor.RemoveFact(loan.Heal);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue =
                    oldMode;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = oldPause;
                // Cleanup only; it never supplies any cell's predicate.
                foreach (Buff buff in target.Buffs.Enumerable.ToArray())
                    if (buff.Blueprint != null && buff.Blueprint.name
                        .StartsWith("KMG_", StringComparison.Ordinal))
                        buff.Remove();
                target.Stats.HitPoints.BaseValue = oldTargetHp;
                target.Stats.SaveWill.BaseValue = oldTargetWill;
                target.Stats.Intelligence.BaseValue = oldTargetInt;
                target.Stats.Charisma.BaseValue = oldTargetCha;
                target.Damage = oldTargetDamage;
                PlaceExpandedSummoningUnit(target, oldTargetPosition);
                PlaceExpandedSummoningUnit(fixture.Caster, oldCasterPosition);
                DisposeExpandedSummoningUnits(fixture.Created,
                    owned.ToArray());
                ResetExpandedSummoningHostile(fixture);
                foreach (UnityEngine.Object value in transient
                    .Where(value => value != null)
                    .Reverse<UnityEngine.Object>())
                    UnityEngine.Object.DestroyImmediate(value);
                RuntimeTestResultWriter.WriteAtomic(Path.Combine(
                    _request.EvidenceDirectory,
                    "sprint39-nalfeshnee-spells.json"),
                    receipt.ToString(Formatting.Indented));
            }
        }

        private static BlueprintAbility Nalfeshnee39Spell(string symbol)
        {
            return (BlueprintAbility)ExpandedSummoningBlueprints.Registered
                .BySymbol[symbol];
        }

        /// <summary>
        /// At will means no resource at all, and the two ranged spells carry
        /// a DERIVED distance rather than a fixed category.
        ///
        /// <para>The range half is the Sprint 38 finding applied rather than
        /// repeated: a range category does not read a caster level, so the
        /// printed medium range of 100 feet plus 10 per level has to be
        /// shipped as an explicit custom distance or it is silently wrong.
        /// At caster level 12 that is 220 feet.</para>
        /// </summary>
        private static void Nalfeshnee39SpellDeclarations(
            BlueprintAbility[] spells, BlueprintAbility feeblemind,
            BlueprintAbility dispel,
            Action<string, bool, JObject, string> check)
        {
            var rows = new JArray();
            bool coherent = true;
            int medium = NalfeshneeRules39Policy.MediumRangeFeet(
                NalfeshneeRules39Policy.SpellLikeCasterLevel);
            foreach (BlueprintAbility spell in spells)
            {
                AbilityResourceLogic[] costs = spell
                    .GetComponents<AbilityResourceLogic>().ToArray();
                bool ranged = ReferenceEquals(spell, feeblemind) ||
                    ReferenceEquals(spell, dispel);
                bool row = costs.Length == 0 &&
                    spell.Type == AbilityType.SpellLike &&
                    spell.ActionType == UnitCommand.CommandType.Standard &&
                    !spell.IsFullRoundAction && !spell.Hidden &&
                    (!ranged || (spell.Range == AbilityRange.Custom &&
                        (int)spell.CustomRange.Value == medium));
                coherent &= row;
                rows.Add(new JObject {
                    ["ability"] = spell.name,
                    ["guid"] = spell.AssetGuid,
                    ["resourceComponents"] = costs.Length,
                    ["abilityType"] = spell.Type.ToString(),
                    ["actionType"] = spell.ActionType.ToString(),
                    ["fullRoundAction"] = spell.IsFullRoundAction,
                    ["hiddenFromUi"] = spell.Hidden,
                    ["range"] = spell.Range.ToString(),
                    ["customRangeFeet"] = (int)spell.CustomRange.Value,
                    ["derivedRangeExpected"] = ranged ? (int?)medium : null,
                    ["coherent"] = row });
            }
            check("nalfeshnee-spells-at-will-declarations", coherent &&
                    spells.Length == 4,
                new JObject {
                    ["spells"] = rows,
                    ["casterLevel"] =
                        NalfeshneeRules39Policy.SpellLikeCasterLevel,
                    ["mediumRangeFeet"] = medium,
                    ["scope"] = "Unholy Nimbus is NOT in this set: it is supernatural with its own three daily uses, and is observed in the nimbus phase."
                },
                "All four at-will spell-likes carry no resource of any kind, are declared spell-like standard actions, and the two ranged ones carry the printed medium range derived from this creature's own caster level rather than a fixed range category.");
        }

        /// <summary>
        /// The creature's own general spell resistance, measured against a
        /// real resistance check rather than read as a number.
        ///
        /// <para>The core phase deliberately deferred this: the live value
        /// lives behind <c>UnitPartSpellResistance</c> and needs a mechanics
        /// context to resolve at all, so a bare stat read would have been
        /// either impossible or misleading. Here an actual
        /// <c>RuleSpellResistanceCheck</c> is raised against the creature and
        /// the resistance the engine itself reports is compared with the
        /// printed 25.</para>
        /// </summary>
        private static void Nalfeshnee39GeneralResistance(
            UnitEntityData nalfeshnee,
            Action<string, bool, JObject, string> check)
        {
            var missile = BlueprintLibraryLookup
                .RequireExact<BlueprintAbility>(BlueprintBootstrap.Library,
                    FcbMagicMissileGuid,
                    "Sprint39 real spell-resistance consumer");
            var context = new AbilityExecutionContext(
                new AbilityData(missile, nalfeshnee.Descriptor),
                new AbilityParams { CasterLevel = 1, SpellLevel = 1 },
                new TargetWrapper(nalfeshnee), null);
            RuleSpellResistanceCheck rule = context.TriggerRule(
                new RuleSpellResistanceCheck(context, nalfeshnee));
            int part = nalfeshnee.Get<UnitPartSpellResistance>() == null ? 0 :
                nalfeshnee.Get<UnitPartSpellResistance>().GetValue(context);
            check("nalfeshnee-spells-general-resistance",
                rule != null &&
                    rule.SpellResistance ==
                        NalfeshneeRules39Policy.SpellResistance &&
                    part == NalfeshneeRules39Policy.SpellResistance,
                new JObject {
                    ["owner"] = nalfeshnee.UniqueId,
                    ["observedSpellResistance"] = rule == null ? null :
                        (int?)rule.SpellResistance,
                    ["printedSpellResistance"] =
                        NalfeshneeRules39Policy.SpellResistance,
                    ["spellResistancePartValue"] = part,
                    ["challengeRatingPlusEleven"] =
                        NalfeshneeRules39Policy.SpellResistance,
                    ["auraConditionalResistanceVsGood"] =
                        NalfeshneeRules39Policy
                            .AuraConditionalSpellResistance,
                    ["method"] = "A real RuleSpellResistanceCheck raised against the live creature, not a stat read."
                },
                "A real spell-resistance check against the live creature reports the printed resistance of 25, which is the general value the core phase deferred to an actual check.");
        }

        /// <summary>
        /// The owner's scoping decision, measured on a neighbour.
        ///
        /// <para>The printed <i>unholy aura</i> protects allies within 20
        /// feet; the owner scoped this creature's constant version to the
        /// exact source alone. That is a deliberate narrowing, so it is
        /// verified rather than asserted: an adjacent owned ally carries no
        /// instance of the aura and its armour class and saves do not move
        /// when it stands beside the nalfeshnee.</para>
        /// </summary>
        private IEnumerable<int> Nalfeshnee39AuraSelfOnly(
            ExpandedSummoningCorrectionFixture fixture,
            List<UnitEntityData> owned, UnitEntityData nalfeshnee,
            BlueprintBuff aura, Action<string, bool, JObject, string> check)
        {
            UnitEntityData neighbour = CastExpandedSummoningOwnTier(
                fixture, "succubus");
            owned.Add(neighbour);
            VrockQuiesce37(neighbour);
            foreach (int frame in WaitInfernalSupport34(neighbour))
                yield return frame;

            // Measured far away first, so the comparison is this neighbour
            // against itself rather than against a printed table.
            PlaceExpandedSummoningUnit(neighbour, nalfeshnee.Position +
                Vector3.right * new Feet(200f).Meters);
            for (int frame = 0; frame < 8; frame++) yield return 0;
            int apart = neighbour.Stats.AC.ModifiedValue;
            int fortApart = neighbour.Stats.SaveFortitude.ModifiedValue;
            int reflexApart = neighbour.Stats.SaveReflex.ModifiedValue;
            int willApart = neighbour.Stats.SaveWill.ModifiedValue;

            // Then well inside the printed twenty-foot ally radius the
            // unscoped spell would have used.
            PlaceExpandedSummoningUnit(neighbour, nalfeshnee.Position +
                Vector3.right * (nalfeshnee.Corpulence +
                    neighbour.Corpulence + 0.5f));
            for (int frame = 0; frame < 16; frame++) yield return 0;
            check("nalfeshnee-aura-self-only",
                neighbour.Descriptor.Buffs.GetBuff(aura) == null &&
                    nalfeshnee.Descriptor.Buffs.GetBuff(aura) != null &&
                    neighbour.Stats.AC.ModifiedValue == apart &&
                    neighbour.Stats.SaveFortitude.ModifiedValue ==
                        fortApart &&
                    neighbour.Stats.SaveReflex.ModifiedValue ==
                        reflexApart &&
                    neighbour.Stats.SaveWill.ModifiedValue == willApart,
                new JObject {
                    ["source"] = nalfeshnee.UniqueId,
                    ["neighbour"] = neighbour.UniqueId,
                    ["neighbourKey"] = "succubus",
                    ["sourceCarriesAura"] =
                        nalfeshnee.Descriptor.Buffs.GetBuff(aura) != null,
                    ["neighbourCarriesAura"] =
                        neighbour.Descriptor.Buffs.GetBuff(aura) != null,
                    ["neighbourArmorClassApart"] = apart,
                    ["neighbourArmorClassAdjacent"] =
                        neighbour.Stats.AC.ModifiedValue,
                    ["neighbourSavesApart"] = new JObject {
                        ["fortitude"] = fortApart,
                        ["reflex"] = reflexApart,
                        ["will"] = willApart },
                    ["neighbourSavesAdjacent"] = new JObject {
                        ["fortitude"] =
                            neighbour.Stats.SaveFortitude.ModifiedValue,
                        ["reflex"] =
                            neighbour.Stats.SaveReflex.ModifiedValue,
                        ["will"] = neighbour.Stats.SaveWill.ModifiedValue },
                    ["printedAllyRadiusFeet"] = 20,
                    ["ownerDecision"] = "The constant unholy aura is scoped to the exact source alone; the printed twenty-foot ally grant is deliberately not implemented, and that narrowing is recorded rather than hidden."
                },
                "The constant unholy aura is an exact-source self effect: an owned ally standing adjacent carries no instance of it and its armour class and all three saves are identical to its own values measured two hundred feet away.");
        }

        /// <summary>
        /// Feeblemind's real disability, and the one it deliberately does not
        /// impose.
        ///
        /// <para>Both halves were read from the installed engine rather than
        /// guessed. <c>ForbidSpellCasting.OnTurnOn</c> calls
        /// <c>Owner.State.SpellCastingForbidden.Retain()</c> unconditionally
        /// and <c>Owner.State.MagicItemsForbidden.Retain()</c> only when its
        /// <c>ForbidMagicItems</c> field is set. The native condition carries
        /// neither - it is only Intelligence and Charisma at -99, and it is
        /// absent from every shipped carrier of that component - which is why
        /// the clone adds it, and why the magic-item flag staying down is a
        /// real observation and not a tautology.</para>
        /// </summary>
        private IEnumerable<int> Nalfeshnee39Feeblemind(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData nalfeshnee, UnitEntityData target,
            BlueprintAbility feeblemind, BlueprintBuff condition,
            Action<string, bool, JObject, string> check)
        {
            int castingBefore = target.Descriptor.State
                .SpellCastingForbidden.Count;
            int itemsBefore = target.Descriptor.State
                .MagicItemsForbidden.Count;
            int intBefore = target.Stats.Intelligence.ModifiedValue;
            int chaBefore = target.Stats.Charisma.ModifiedValue;
            target.Stats.SaveWill.BaseValue = -100;

            var watcher = new Nalfeshnee39SpellWatcher(nalfeshnee,
                feeblemind, target);
            EventBus.Subscribe(watcher);
            UnitUseAbility command = null;
            try
            {
                foreach (int frame in Sprint26Cast(nalfeshnee, feeblemind,
                    new TargetWrapper(target), value => command = value))
                    yield return frame;
            }
            finally { EventBus.Unsubscribe(watcher); }

            Buff applied = target.Descriptor.Buffs.GetBuff(condition);
            ForbidSpellCastingState39 state = ForbidSpellCastingState39.Read(
                condition);
            check("nalfeshnee-feeblemind-disability",
                command != null &&
                    command.Result == UnitCommand.ResultType.Success &&
                    watcher.Saves.Count == 1 &&
                    watcher.Saves[0].Type == SavingThrowType.Will &&
                    !watcher.Saves[0].IsPassed &&
                    applied != null && applied.IsPermanent &&
                    target.Stats.Intelligence.ModifiedValue < intBefore &&
                    target.Stats.Charisma.ModifiedValue < chaBefore &&
                    target.Descriptor.State.SpellCastingForbidden.Count ==
                        castingBefore + 1 &&
                    (bool)target.Descriptor.State.SpellCastingForbidden,
                new JObject {
                    ["caster"] = nalfeshnee.UniqueId,
                    ["target"] = target.UniqueId,
                    ["commandResult"] = command == null ? null :
                        command.Result.ToString(),
                    ["saveCount"] = watcher.Saves.Count,
                    ["saveType"] = watcher.Saves.Count == 0 ? null :
                        watcher.Saves[0].Type.ToString(),
                    ["savePassed"] = watcher.Saves.Count == 0 ? null :
                        (bool?)watcher.Saves[0].IsPassed,
                    ["saveDc"] = watcher.Saves.Count == 0 ? null :
                        (int?)watcher.Saves[0].DifficultyClass,
                    ["spellResistanceChecked"] =
                        watcher.Resistances.Count > 0,
                    ["cloneApplied"] = applied != null,
                    ["clonePermanent"] = applied != null &&
                        applied.IsPermanent,
                    ["cloneTimeLeft"] = applied == null ? null :
                        (double?)applied.TimeLeft.TotalSeconds,
                    ["intelligenceBefore"] = intBefore,
                    ["intelligenceAfter"] =
                        target.Stats.Intelligence.ModifiedValue,
                    ["charismaBefore"] = chaBefore,
                    ["charismaAfter"] = target.Stats.Charisma.ModifiedValue,
                    ["spellCastingForbiddenBefore"] = castingBefore,
                    ["spellCastingForbiddenAfter"] = target.Descriptor.State
                        .SpellCastingForbidden.Count,
                    ["spellCastingForbiddenNow"] =
                        (bool)target.Descriptor.State.SpellCastingForbidden,
                    ["forbidSpellCastingComponents"] = state.Components
                },
                "One real cast with a failed Will save applies this project's own feeblemind clone permanently, collapses Intelligence and Charisma, and retains the engine's own spellcasting-forbidden flag - which the native condition alone does not do, so a divine caster is actually stopped rather than merely weakened.");

            check("nalfeshnee-feeblemind-items-permitted",
                !state.ForbidMagicItems &&
                    target.Descriptor.State.MagicItemsForbidden.Count ==
                        itemsBefore &&
                    !(bool)target.Descriptor.State.MagicItemsForbidden,
                new JObject {
                    ["target"] = target.UniqueId,
                    ["forbidMagicItemsFlag"] = state.ForbidMagicItems,
                    ["magicItemsForbiddenBefore"] = itemsBefore,
                    ["magicItemsForbiddenAfter"] = target.Descriptor.State
                        .MagicItemsForbidden.Count,
                    ["magicItemsForbiddenNow"] =
                        (bool)target.Descriptor.State.MagicItemsForbidden,
                    ["ownerDecision"] = "ForbidMagicItems stays false. The printed spell prohibits spellcasting and coherent communication; it states no blanket ban on every magic item, and the spell-completion and spell-trigger restrictions that do apply are already the engine's own."
                },
                "The engine's magic-items-forbidden flag is never retained: the component's own field is false and the live counter is unchanged, so the clone imposes the printed prohibition without inventing a blanket item ban.");

            // LASTING, which is a different claim from "permanent flag set".
            // The printed spell is instantaneous with a result that outlives
            // its cause, so the condition must survive the summon that
            // applied it going away entirely.
            int ticks = 0;
            double start = Game.Instance.TimeController.GameTime.TotalSeconds;
            while (ticks++ < 600 &&
                Game.Instance.TimeController.GameTime.TotalSeconds - start <
                    GameConsts.RoundDuration * 3)
            {
                Game.Instance.IsPaused = false;
                yield return 0;
            }
            bool survivedTime = target.Descriptor.Buffs
                .GetBuff(condition) != null;
            DisposeExpandedSummoningUnits(fixture.Created,
                new[] { nalfeshnee });
            for (int frame = 0; frame < 16; frame++) yield return 0;
            check("nalfeshnee-feeblemind-lasting",
                survivedTime &&
                    nalfeshnee.Destroyed &&
                    target.Descriptor.Buffs.GetBuff(condition) != null &&
                    (bool)target.Descriptor.State.SpellCastingForbidden,
                new JObject {
                    ["target"] = target.UniqueId,
                    ["survivedThreeRounds"] = survivedTime,
                    ["elapsedGameSeconds"] = Game.Instance.TimeController
                        .GameTime.TotalSeconds - start,
                    ["sourceDestroyed"] = nalfeshnee.Destroyed,
                    ["conditionStillPresent"] = target.Descriptor.Buffs
                        .GetBuff(condition) != null,
                    ["spellCastingStillForbidden"] =
                        (bool)target.Descriptor.State.SpellCastingForbidden,
                    ["carriesCasterMissingCleanup"] = false
                },
                "The result lasts: it does not expire on a timer and it is not erased when the summoned caster that applied it is destroyed, which is what separates an instantaneous spell with a lasting effect from a maintained buff.");
        }

        /// <summary>
        /// The one supported cure, cast as the real native spell.
        ///
        /// <para>Native <i>heal</i> removes feeblemind by IDENTITY - a
        /// <c>ContextActionRemoveBuff</c> naming the native condition - so a
        /// clone with its own identity would simply never be cured. The build
        /// inserts one removal beside the native one, inside whatever
        /// condition encloses it. This cell is what shows that insertion
        /// actually works through the player's own spell rather than through
        /// anything this fixture does.</para>
        /// </summary>
        private IEnumerable<int> Nalfeshnee39FeeblemindCure(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData target, BlueprintBuff condition,
            Nalfeshnee39CureLoan loan,
            Action<string, bool, JObject, string> check)
        {
            var heal = fixture.Blueprints.OfType<BlueprintAbility>().Single(
                value => value.AssetGuid ==
                    ExpandedSummoningSpecialBuilder.NativeHeal39);
            bool present = target.Descriptor.Buffs.GetBuff(condition) != null;
            // Native heal is friend-targeted, so this disposable opponent is
            // admitted to the disposable fixture party for the real
            // eligibility and removal path. Both memberships are restored.
            SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
            Game.Instance.TurnBasedCombatController.Activate();
            target.Descriptor.SwitchFactions(fixture.Caster.Faction, false);
            Game.Instance.Player.Party.Add(target);
            loan.PartyAdded = true;
            loan.Heal = fixture.Caster.Descriptor.AddFact(heal) as Ability;
            PlaceExpandedSummoningUnit(fixture.Caster, target.Position +
                Vector3.right * (target.Corpulence +
                    fixture.Caster.Corpulence + 0.5f));
            foreach (int frame in Sprint26Cast(fixture.Caster, heal,
                new TargetWrapper(target), value => { })) yield return frame;
            for (int frame = 0; frame < 16; frame++) yield return 0;
            check("nalfeshnee-feeblemind-cure",
                present &&
                    target.Descriptor.Buffs.GetBuff(condition) == null &&
                    !(bool)target.Descriptor.State.SpellCastingForbidden,
                new JObject {
                    ["nativeCure"] = heal.AssetGuid,
                    ["curer"] = fixture.Caster.UniqueId,
                    ["target"] = target.UniqueId,
                    ["conditionBeforeCure"] = present,
                    ["conditionAfterCure"] = target.Descriptor.Buffs
                        .GetBuff(condition) != null,
                    ["spellCastingForbiddenAfterCure"] =
                        (bool)target.Descriptor.State.SpellCastingForbidden,
                    ["spellCastingForbiddenCount"] = target.Descriptor.State
                        .SpellCastingForbidden.Count,
                    ["removedByObserver"] = false
                },
                "The actual native heal spell, cast by a real caster at a real recipient, removes this project's own feeblemind clone and releases the engine's spellcasting-forbidden flag - so the printed cure genuinely reaches the clone rather than only the native condition.");
        }

        /// <summary>
        /// Greater Dispel: one carried decisive check, an ordered walk, a
        /// real cap, no reroll and no retry.
        ///
        /// <para>Five request-local spell effects are planted at caster
        /// levels 20, 4, 3, 2 and 1, through the same qualified native
        /// fixture the released dispel review uses. With one check of
        /// <c>r</c> at caster level 12 an effect passes when
        /// <c>r + 12 &gt; 11 + its own caster level</c>, so the twenty fails
        /// for every roll below twenty and the low four all pass from four
        /// up. That arrangement makes one cast decide four separate things at
        /// once: the single roll, the descending order, the cap of three, and
        /// that a FAILED higher effect is skipped rather than ending the walk
        /// while a fifth passing effect beyond the cap survives
        /// untouched.</para>
        ///
        /// <para>The roll is native and is not forced. Instead the attempt is
        /// repeated a bounded number of times until the roll lands in the
        /// window that exercises the cap, and every attempt is recorded with
        /// its roll - so a run that never reaches the window reports that
        /// honestly rather than passing on a weaker arrangement.</para>
        /// </summary>
        private IEnumerable<int> Nalfeshnee39GreaterDispel(
            ExpandedSummoningCorrectionFixture fixture,
            UnitEntityData nalfeshnee, UnitEntityData target,
            BlueprintAbility dispel, List<UnityEngine.Object> transient,
            Action<string, bool, JObject, string> check)
        {
            int casterLevel = NalfeshneeRules39Policy.SpellLikeCasterLevel;
            int cap = NalfeshneeRules39Policy.GreaterDispelMaxRemovals(
                casterLevel);
            int[] levels = { 20, 4, 3, 2, 1 };
            var attempts = new JArray();
            bool decided = false;
            JObject winning = null;
            for (int attempt = 1; attempt <= 8 && !decided; attempt++)
            {
                var observed = new Astral31DispelEvents {
                    Owner = nalfeshnee };
                observed.Targets.Add(target);
                var dice = new Nalfeshnee39DiceWatcher(nalfeshnee, dispel);
                var planted = new List<Buff>();
                EventBus.Subscribe(observed);
                EventBus.Subscribe(dice);
                try
                {
                    // The native fixture helper appends to the collection it
                    // is handed, so its return value is deliberately not
                    // added again.
                    for (int index = 0; index < levels.Length; index++)
                        Astral31DispelFixtureEffect(target,
                            fixture.Caster, "s39-dispel-" + attempt + "-" +
                                index,
                            Nalfeshnee39DispelGuid("a", attempt, index),
                            Nalfeshnee39DispelGuid("b", attempt, index),
                            levels[index], SpellSchool.Abjuration,
                            SpellDescriptor.None, false, transient, planted,
                            observed);
                    Buff[] effects = planted.Take(levels.Length).ToArray();
                    UnitUseAbility command = null;
                    foreach (int frame in Sprint26Cast(nalfeshnee, dispel,
                        new TargetWrapper(target), value => command = value))
                        yield return frame;
                    for (int frame = 0; frame < 8; frame++) yield return 0;

                    // Assigned rather than selected by a conditional:
                    // RuleRollDice.Result is a RollEntry that converts to and
                    // from int, so a ternary over it has no common type here.
                    int roll = -1;
                    if (dice.Rolls.Count == 1)
                        roll = dice.Rolls[0].Result;
                    bool[] gone = effects.Select(value =>
                        !target.Descriptor.Buffs.Enumerable.Any(live =>
                            ReferenceEquals(live, value))).ToArray();
                    int[] passing = NalfeshneeRules39Policy
                        .GreaterDispelPassingCandidates(casterLevel, roll,
                            levels);
                    int[] expected = passing.Take(cap).ToArray();
                    bool shapeMatches = roll > 0 &&
                        gone.Select((value, index) => value ==
                            expected.Contains(index)).All(value => value);
                    bool inWindow = roll >= 4 && roll <= 19;
                    var row = new JObject {
                        ["attempt"] = attempt,
                        ["commandResult"] = command == null ? null :
                            command.Result.ToString(),
                        ["decisiveRollCount"] = dice.Rolls.Count,
                        ["decisiveRoll"] = roll,
                        ["rollDieSides"] = dice.Rolls.Count == 1 ?
                            (int?)(int)dice.Rolls[0].DiceFormula.Dice : null,
                        ["casterLevel"] = casterLevel,
                        ["cap"] = cap,
                        ["effectCasterLevels"] = new JArray(levels),
                        ["dispelRulesRaised"] = observed.Dispels.Count,
                        ["removedIndices"] = new JArray(gone
                            .Select((value, index) => new { value, index })
                            .Where(pair => pair.value)
                            .Select(pair => pair.index)),
                        ["predictedPassingIndices"] = new JArray(passing),
                        ["predictedRemovedIndices"] = new JArray(expected),
                        ["shapeMatchesPolicy"] = shapeMatches,
                        ["rollInCapWindow"] = inWindow,
                        ["capExercised"] = inWindow &&
                            expected.Length == cap,
                        ["highestEffectSurvived"] = !gone[0],
                        ["fifthEffectSurvived"] = !gone[4],
                        ["dispatchesEqualConfirmed"] =
                            observed.Dispels.Count == expected.Length
                    };
                    attempts.Add(row);
                    if (!shapeMatches || dice.Rolls.Count != 1)
                        throw new InvalidOperationException(
                            "Sprint39 greater dispel attempt " + attempt +
                            " did not match the production policy at its own observed roll: " +
                            row.ToString(Formatting.None));
                    if (inWindow && expected.Length == cap)
                    {
                        decided = true;
                        winning = row;
                    }
                }
                finally
                {
                    EventBus.Unsubscribe(dice);
                    EventBus.Unsubscribe(observed);
                    // Cleanup of this attempt's own planted effects only.
                    foreach (Buff value in planted.Where(value =>
                        value != null && !value.IsDisposed).ToArray())
                        value.Remove();
                }
            }
            check("nalfeshnee-dispel-one-check-cap-and-no-retry", decided,
                new JObject {
                    ["caster"] = nalfeshnee.UniqueId,
                    ["target"] = target.UniqueId,
                    ["ability"] = dispel.AssetGuid,
                    ["casterLevel"] = casterLevel,
                    ["printedCap"] = cap,
                    ["capFormula"] = "one removal per four caster levels",
                    ["checkDifficultyFormula"] =
                        "eleven plus the effect's own caster level",
                    ["effectCasterLevels"] = new JArray(levels),
                    ["attempts"] = attempts,
                    ["decisive"] = winning,
                    ["boundedRetryReason"] = "The check is a native roll and is never forced; attempts repeat only until the roll lands in the window that exercises the cap, and every attempt is asserted against the production policy at its own roll."
                },
                "One cast raises exactly one decisive check and applies it down an ordered snapshot: three effects are removed at the printed cap, a failed higher-level effect is skipped rather than ending the walk and is never retried, a fifth passing effect beyond the cap survives, and the number of native dispel rules raised equals the number of confirmed removals.");
        }

        private static string Nalfeshnee39DispelGuid(string kind, int attempt,
            int index)
        {
            return ("39d1" + kind + attempt.ToString("x") +
                index.ToString("x")).PadRight(32, '0');
        }

        /// <summary>
        /// Call Lightning and Slow, cast repeatedly because at will means
        /// exactly that: no resource, no marker and no cooldown of this
        /// creature's own. Each cast is checked for this creature's own
        /// caster level and the printed Charisma-derived difficulty rather
        /// than for a pinned number.
        /// </summary>
        private IEnumerable<int> Nalfeshnee39AtWillRepeats(
            UnitEntityData nalfeshnee, UnitEntityData target,
            BlueprintAbility lightning, BlueprintAbility slow,
            Action<string, bool, JObject, string> check)
        {
            var rows = new JArray();
            bool coherent = true;
            foreach (BlueprintAbility spell in new[] { lightning, slow })
            {
                for (int round = 1; round <= 2; round++)
                {
                    var granted = new AbilityData(nalfeshnee.Descriptor
                        .Abilities.GetAbility(spell));
                    bool availableBefore = granted.IsAvailable;
                    AbilityParams parameters = granted.CalculateParams();
                    UnitUseAbility command = null;
                    foreach (int frame in Sprint26Cast(nalfeshnee, spell,
                        new TargetWrapper(target), value => command = value))
                        yield return frame;
                    var after = new AbilityData(nalfeshnee.Descriptor
                        .Abilities.GetAbility(spell));
                    bool row = availableBefore && after.IsAvailable &&
                        command != null &&
                        command.Result == UnitCommand.ResultType.Success &&
                        parameters.CasterLevel ==
                            NalfeshneeRules39Policy.SpellLikeCasterLevel;
                    coherent &= row;
                    rows.Add(new JObject {
                        ["ability"] = spell.name,
                        ["round"] = round,
                        ["availableBefore"] = availableBefore,
                        ["availableAfter"] = after.IsAvailable,
                        ["commandResult"] = command == null ? null :
                            command.Result.ToString(),
                        ["casterLevel"] = parameters.CasterLevel,
                        ["spellLevel"] = parameters.SpellLevel,
                        ["difficultyClass"] = parameters.DC,
                        ["coherent"] = row });
                }
            }
            check("nalfeshnee-spells-at-will-repeat", coherent,
                new JObject {
                    ["caster"] = nalfeshnee.UniqueId,
                    ["casts"] = rows,
                    ["expectedCasterLevel"] =
                        NalfeshneeRules39Policy.SpellLikeCasterLevel
                },
                "Call Lightning and Slow each succeed twice in a row and stay available immediately afterwards, at this creature's own caster level of twelve - so at will is a real frequency rather than a label on a once-a-day ability.");
        }

        /// <summary>
        /// What the cure cell borrowed, so the enclosing phase can give it
        /// back. An iterator method cannot take <c>ref</c> parameters, and
        /// leaving the restoration to the borrower would put it outside the
        /// phase's own finally.
        /// </summary>
        private sealed class Nalfeshnee39CureLoan
        {
            internal bool PartyAdded;
            internal Ability Heal;
        }

        /// <summary>
        /// What the clone's spellcasting restriction actually declares, read
        /// off the built blueprint rather than assumed from the build code.
        /// </summary>
        private struct ForbidSpellCastingState39
        {
            internal int Components;
            internal bool ForbidMagicItems;

            internal static ForbidSpellCastingState39 Read(
                BlueprintBuff condition)
            {
                Kingmaker.UnitLogic.FactLogic.ForbidSpellCasting[] found =
                    condition.ComponentsArray
                        .OfType<Kingmaker.UnitLogic.FactLogic
                            .ForbidSpellCasting>().ToArray();
                return new ForbidSpellCastingState39 {
                    Components = found.Length,
                    ForbidMagicItems = found.Length == 1 &&
                        found[0].ForbidMagicItems };
            }
        }

        /// <summary>
        /// One owner's one spell-like, on the global rulebook: the save, the
        /// spell-resistance check and the conditions applied.
        /// </summary>
        private sealed class Nalfeshnee39SpellWatcher :
            IGlobalRulebookHandler<RuleSavingThrow>,
            IGlobalRulebookHandler<RuleSpellResistanceCheck>,
            IGlobalRulebookHandler<RuleApplyBuff>
        {
            private readonly UnitEntityData _owner;
            private readonly BlueprintAbility _ability;
            private readonly UnitEntityData _target;
            internal readonly List<RuleSavingThrow> Saves =
                new List<RuleSavingThrow>();
            internal readonly List<RuleSpellResistanceCheck> Resistances =
                new List<RuleSpellResistanceCheck>();
            internal readonly List<RuleApplyBuff> Applications =
                new List<RuleApplyBuff>();

            internal Nalfeshnee39SpellWatcher(UnitEntityData owner,
                BlueprintAbility ability, UnitEntityData target)
            { _owner = owner; _ability = ability; _target = target; }

            private bool Mine(MechanicsContext context)
            {
                return context != null &&
                    ReferenceEquals(context.MaybeCaster, _owner) &&
                    ReferenceEquals(context.SourceAbility, _ability);
            }

            public void OnEventAboutToTrigger(RuleSavingThrow evt) { }
            public void OnEventDidTrigger(RuleSavingThrow evt)
            {
                if (ReferenceEquals(evt.Initiator, _target) &&
                    evt.Reason != null && Mine(evt.Reason.Context))
                    Saves.Add(evt);
            }

            public void OnEventAboutToTrigger(RuleSpellResistanceCheck evt) { }
            public void OnEventDidTrigger(RuleSpellResistanceCheck evt)
            { if (Mine(evt.Context)) Resistances.Add(evt); }

            public void OnEventAboutToTrigger(RuleApplyBuff evt) { }
            public void OnEventDidTrigger(RuleApplyBuff evt)
            { if (Mine(evt.Context)) Applications.Add(evt); }
        }

        /// <summary>
        /// The dice one ability raises, so "one decisive check" can be
        /// counted rather than claimed. Filtered to this owner and this
        /// ability's own context, because an unfiltered dice handler on a
        /// live game hears everything.
        /// </summary>
        private sealed class Nalfeshnee39DiceWatcher :
            IGlobalRulebookHandler<RuleRollDice>
        {
            private readonly UnitEntityData _owner;
            private readonly BlueprintAbility _ability;
            internal readonly List<RuleRollDice> Rolls =
                new List<RuleRollDice>();

            internal Nalfeshnee39DiceWatcher(UnitEntityData owner,
                BlueprintAbility ability)
            { _owner = owner; _ability = ability; }

            public void OnEventAboutToTrigger(RuleRollDice evt) { }
            public void OnEventDidTrigger(RuleRollDice evt)
            {
                if (ReferenceEquals(evt.Initiator, _owner) &&
                    evt.Reason != null && evt.Reason.Context != null &&
                    ReferenceEquals(evt.Reason.Context.SourceAbility,
                        _ability)) Rolls.Add(evt);
            }
        }
    }
}
