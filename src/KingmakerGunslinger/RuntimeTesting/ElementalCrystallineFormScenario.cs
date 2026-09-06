using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.ElementalRaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>Native commands and attack/effect resolution on named disposable
    /// units. Only asynchronous projectile travel is completed at its impact
    /// boundary, with a restored request-local clock for delayed rays: no
    /// direct invocation of the trait or damage effect.</summary>
    internal static class ElementalCrystallineFormScenario
    {
        private const string Frost = "9af2ab69df6538f4793b2f9c3cc85603";
        private const string Snowball = "9f10909f0be1f5141bf1c102041f93d9";

        internal static void Exercise(RuntimeTestRequest request,
            ICollection<RuntimeTestAssertion> assertions, ICollection<string> files)
        {
            var rows = new JArray();
            var diagnostics = new List<string>();
            UnitEntityData[] before = Game.Instance.State.Units.All.ToArray();
            UnityEngine.Random.State random = UnityEngine.Random.state;
            if (Game.Instance.ProjectileController.Projectiles.Any())
                throw new InvalidOperationException("Ray fixture refuses preexisting projectiles.");
            ElementalRaceBlueprints race = BlueprintBootstrap.ElementalRaces.Oread;
            try
            {
                foreach (ElementalHeritageBlueprints heritage in race.Heritages.Choices())
                {
                    var fixture = ElementalUndineFeatScenario.OpenSummonFixture(race.Race, diagnostics);
                    try
                    {
                        UnitEntityData defender = fixture.Caster;
                        // Native ranged-touch attacks against willing allies
                        // auto-hit before AC calculation. Give only this request's
                        // attacker a hostile, unregistered faction clone.
                        BlueprintFaction hostile = UnityEngine.Object.Instantiate(defender.Blueprint.Faction);
                        hostile.name = "KMG_Runtime_Crystalline_HostileFaction";
                        hostile.Peaceful = false;
                        hostile.AlwaysEnemy = false;
                        hostile.Neutral = false;
                        hostile.IsDirectlyControllable = false;
                        hostile.Dummy = null;
                        hostile.AttackFactions = new[] { defender.Blueprint.Faction };
                        UnitEntityData attacker = fixture.SpawnFixtureUnit(race.Race,
                            hostile, new Vector3(3, 0, 0), "CrystallineRayAttacker");
                        Check(assertions, rows, heritage.Definition.Id + "-native-hostility",
                            defender.IsEnemy(attacker) && attacker.IsEnemy(defender),
                            "native groups recognize hostility; no shared faction array mutated");
                        foreach (UnitEntityData unit in new[] { attacker, defender })
                        {
                            unit.CombatState.JoinCombat();
                            unit.CombatState.OnNewRound();
                        }
                        attacker.Memory.Add(defender);
                        defender.Memory.Add(attacker);
                        Run(attacker, defender, race, heritage, rows, assertions);
                    }
                    finally
                    {
                        // Native controller cleanup owns these request-local
                        // projectile views. No Destroy on registered objects.
                        foreach (Projectile projectile in Game.Instance.ProjectileController.Projectiles.ToArray())
                            projectile.Cleared = true;
                        Game.Instance.ProjectileController.Tick();
                        fixture.Dispose();
                        Check(assertions, rows, heritage.Definition.Id + "-native-lifetime",
                            fixture.NativeErrors == 0 && fixture.NativeExceptions == 0 &&
                            fixture.NativeObservationReleased && fixture.NativeTeardownObserved &&
                            fixture.AreaContextRestored && fixture.PlayerContextRestored,
                            "errors=" + fixture.NativeErrors + ";exceptions=" + fixture.NativeExceptions);
                    }
                }
            }
            finally
            {
                UnityEngine.Random.state = random;
                bool clean = Game.Instance.State.Units.All.Count == before.Length &&
                    before.All(value => Game.Instance.State.Units.All.Contains(value)) &&
                    !Game.Instance.ProjectileController.Projectiles.Any();
                Check(assertions, rows, "fixture-cleanup", clean, "exact units and empty original projectile catalog");
                string path = Path.Combine(request.EvidenceDirectory, "elemental-crystalline-form.json");
                File.WriteAllText(path, new JObject {
                    { "schemaVersion", 1 }, { "saveStateTouched", false }, { "cleanupExact", clean },
                    { "isolatedBoundary", "request-local asynchronous projectile arrival and restored clock scheduling only; native command, projectile creation/delay, attack roll, OnHit event and damage effects retained" },
                    { "diagnostics", new JArray(diagnostics) }, { "observations", rows }
                }.ToString(Formatting.Indented));
                files.Add(path);
            }
        }

        private static void Run(UnitEntityData attacker, UnitEntityData defender, ElementalRaceBlueprints race,
            ElementalHeritageBlueprints heritage, JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            UnitDescriptor owner = defender.Descriptor;
            owner.AddFact(heritage.Marker);
            ElementalAlternateTraitBlueprints trait = race.AlternateTraits.Require(ElementalAlternateTraitId.CrystallineForm);
            BlueprintAbilityResource resource = trait.Mechanics().OfType<BlueprintAbilityResource>().Single();
            BlueprintActivatableAbility modeBlueprint = trait.Mechanics().OfType<BlueprintActivatableAbility>().Single();
            string prefix = heritage.Definition.Id + "-";
            BlueprintAbility frost = Require(Frost);
            owner.AddFact(trait.Marker);
            ActivatableAbility mode = Mode(owner, modeBlueprint);
            Check(assertions, rows, prefix + "owned-graph",
                owner.HasFact(trait.Provider) && owner.HasFact(race.Resistance) && owner.HasFact(heritage.SlaFeature) &&
                !owner.HasFact(heritage.Affinity) && resource.GetMaxAmount(owner) == 1 && !mode.IsOn &&
                ElementalTraitDailyResourceRuntime.IsExact(owner, race.AlternateTraits),
                "affinity replaced, resistance/SLA retained; independent use and off-by-default mode");

            // Every cataloged native/project ray traverses actual RuleAttackRoll
            // and nested native touch AC, including racial stacking and cleanup.
            foreach (string guid in ElementalCrystallineFormPolicy.RayAbilityGuids.Concat(new[] {
                Snowball, "0c852a2405dd9f14a8bbcfaf245ff823", "9a46dfd390f943647ab4395fc997936d",
                "0a2f7c6aa81bc6548ac7780d8b70bcbc", "5e1db2ef80ff361448549beeb7785791" }))
            {
                BlueprintAbility ability = Require(guid);
                bool nativeDelivery = ability.ComponentsArray.OfType<AbilityDeliverProjectile>().Any();
                if (!nativeDelivery && guid != "e50e2db3d78b7ff4aa5c9699ba26febe")
                    throw new InvalidOperationException("Unexpected changed ray delivery: " + guid);
                owner.RemoveFact(trait.Marker);
                int baseline = ArmorClass(attacker, defender, ability);
                int raw = owner.Stats.AC.ModifiedValue;
                owner.AddFact(trait.Marker);
                int current = ArmorClass(attacker, defender, ability);
                int expected = nativeDelivery && ElementalCrystallineFormPolicy.RayAbilityGuids.Contains(guid) ? 2 : 0;
                Check(assertions, rows, prefix + "ray-ac-" + ability.name,
                    current - baseline == expected && owner.Stats.AC.ModifiedValue == raw,
                    "baseline=" + baseline + ";withTrait=" + current + ";expectedDelta=" + expected + ";rawRestored=" + (owner.Stats.AC.ModifiedValue == raw) +
                    (nativeDelivery ? "" : ";native delivery absent: paired ray weapon negative control only; optional replacement mechanic NOT-RUN;components=" +
                        string.Join(",", ability.ComponentsArray.Select(value => value.GetType().FullName))));
            }
            mode = Mode(owner, modeBlueprint);
            int withoutRacial = ArmorClass(attacker, defender, frost);
            var racial = owner.Stats.AC.AddModifier(4, owner.GetFact(heritage.Marker),
                "KMG_Runtime_Crystalline_RacialControl", ModifierDescriptor.Racial);
            owner.Stats.AC.UpdateValue();
            int strongerRacial = ArmorClass(attacker, defender, frost);
            racial.Remove();
            Check(assertions, rows, prefix + "independent-racial-stacking", strongerRacial - withoutRacial == 4,
                "PRD bonus-type exception and native Racial preserve both independent sources (2 + 4); delta=" +
                (strongerRacial - withoutRacial));

            float[] actionBudget = Cooldowns(defender);
            mode.IsOn = true;
            var consent = owner.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, modeBlueprint.Buff));
            try
            {
                owner.TurnOff(false);
                Check(assertions, rows, prefix + "unit-unload-retains-consent",
                    !owner.IsTurnedOn && ReferenceEquals(Mode(owner, modeBlueprint), mode) && mode.IsOn &&
                    owner.Buffs.Enumerable.Any(value => ReferenceEquals(value, consent)) &&
                    owner.Resources.GetResourceAmount(resource) == 1,
                    "native whole-unit TurnOff is not permanent race or trait removal");
            }
            finally { owner.TurnOn(); }
            Check(assertions, rows, prefix + "unit-reload-retains-consent",
                owner.IsTurnedOn && ElementalHeritageRuntime.Reconcile(owner, null, null) &&
                ReferenceEquals(Mode(owner, modeBlueprint), mode) && mode.IsOn &&
                owner.Buffs.Enumerable.Single(value => ReferenceEquals(value.Blueprint, modeBlueprint.Buff)) == consent &&
                consent.Active && owner.Resources.GetResourceAmount(resource) == 1,
                "same native mode and buff, exact resource and idempotent provider reconciliation after TurnOn");
            mode.IsOn = false;
            Check(assertions, rows, prefix + "toggle-cancel",
                owner.Resources.GetResourceAmount(resource) == 1 && !owner.HasFact(modeBlueprint.Buff) &&
                actionBudget.SequenceEqual(Cooldowns(defender)),
                "native opt-in then cancel spends no use or standard/move/swift action and removes consent buff");
            CastEvidence unarmedOff = Cast(attacker, defender, frost, true);
            Check(assertions, rows, prefix + "off-allows-ray",
                unarmedOff.Completed && unarmedOff.Damage > 0 && unarmedOff.Hits == 1 &&
                owner.Resources.GetResourceAmount(resource) == 1, unarmedOff.ToString());

            mode.IsOn = true;
            CastEvidence deflected = Cast(attacker, defender, frost, true);
            Check(assertions, rows, prefix + "native-hit-deflected",
                deflected.Completed && deflected.Projectiles == 1 && deflected.Parried == 1 && deflected.Damage == 0 &&
                owner.Resources.GetResourceAmount(resource) == 0 && !mode.IsOn && !mode.IsAvailable &&
                actionBudget.SequenceEqual(Cooldowns(defender)),
                deflected + ";uses=" + owner.Resources.GetResourceAmount(resource) + ";freeHand=" +
                ElementalCrystallineFormRuntime.HasFreeHand(owner) + ";canAct=" + defender.CombatState.CanActInCombat +
                ";flat=" + Rulebook.Trigger(new RuleCheckTargetFlatFooted(attacker, defender)).IsFlatFooted);

            CastEvidence spent = Cast(attacker, defender, frost, true);
            Check(assertions, rows, prefix + "spent-use-cannot-deflect",
                spent.Completed && spent.Projectiles == 1 && spent.Hits == 1 && spent.Parried == 0 &&
                spent.Damage > 0 && owner.Resources.GetResourceAmount(resource) == 0, spent.ToString());

            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            mode.IsOn = true;
            CastEvidence nonray = Cast(attacker, defender, Require(Snowball), true);
            Check(assertions, rows, prefix + "non-ray-retains-use",
                nonray.Completed && nonray.Damage > 0 && nonray.Parried == 0 &&
                owner.Resources.GetResourceAmount(resource) == 1 && mode.IsOn, nonray.ToString());
            CastEvidence miss = Cast(attacker, defender, frost, false);
            Check(assertions, rows, prefix + "miss-retains-use", miss.Completed && miss.Hits == 0 &&
                miss.Parried == 0 && miss.Damage == 0 && owner.Resources.GetResourceAmount(resource) == 1, miss.ToString());
            owner.Resources.Spend(resource, 1);
            owner.GetFact(trait.Provider).Deactivate();
            owner.GetFact(trait.Provider).Activate();
            Check(assertions, rows, prefix + "reactivation-retains-spent",
                ElementalHeritageRuntime.Reconcile(owner, null, null) && owner.Resources.GetResourceAmount(resource) == 0,
                "native deactivate/activate and provider reconciliation do not refill");
            owner.RemoveFact(trait.Marker);
            Check(assertions, rows, prefix + "remove-cleans-owned",
                owner.HasFact(heritage.Affinity) && !owner.HasFact(modeBlueprint.Buff) &&
                ElementalTraitDailyResourceRuntime.IsExact(owner, race.AlternateTraits),
                "native affinity restored; exact mode/buff/resource cleaned");
            owner.AddFact(trait.Marker);
            Check(assertions, rows, prefix + "readd-retains-spent", owner.Resources.GetResourceAmount(resource) == 0,
                "true marker removal/re-add retains daily expenditure");
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            Check(assertions, rows, prefix + "rest-one-use", owner.Resources.GetResourceAmount(resource) == 1,
                "ordinary native rest restores exactly one deflection");
            // The preceding real marker removal/re-add replaces AddFacts-owned
            // activatables. Never drive the detached pre-respec instance.
            EquipmentAndAwareness(attacker, defender, resource, Mode(owner, modeBlueprint), frost, prefix, rows, assertions);
            MultipleAndNonDamageRays(attacker, defender, resource, Mode(owner, modeBlueprint),
                prefix, rows, assertions);
        }

        private static void EquipmentAndAwareness(UnitEntityData attacker, UnitEntityData defender,
            BlueprintAbilityResource resource, ActivatableAbility mode, BlueprintAbility frost, string prefix,
            JArray rows, ICollection<RuntimeTestAssertion> assertions)
        {
            UnitDescriptor owner = defender.Descriptor;
            if (!ReferenceEquals(Mode(owner, mode.Blueprint), mode))
                throw new InvalidOperationException("Ray consent must use the current native owned activatable.");
            if (owner.Body.PrimaryHand.HasItem || owner.Body.SecondaryHand.HasItem)
                throw new InvalidOperationException("Ray equipment fixture requires initially empty hands.");
            BlueprintItemWeapon sword = BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(
                BlueprintBootstrap.Library, "57c8994d1f1becf49ac4f642e5d8ca9d", "native short sword");
            var primary = new ItemEntityWeapon(sword);
            var secondary = new ItemEntityWeapon(sword);
            try
            {
                owner.Body.PrimaryHand.InsertItem(primary);
                owner.Body.SecondaryHand.InsertItem(secondary);
                mode.IsOn = true;
                CastEvidence occupied = Cast(attacker, defender, frost, true);
                Check(assertions, rows, prefix + "both-hands-occupied",
                    ReferenceEquals(owner.Body.PrimaryHand.MaybeItem, primary) &&
                    ReferenceEquals(owner.Body.SecondaryHand.MaybeItem, secondary) &&
                    !ElementalCrystallineFormRuntime.HasFreeHand(owner) && occupied.Completed &&
                    occupied.Hits == 1 && occupied.Parried == 0 && occupied.Damage > 0 &&
                    owner.Resources.GetResourceAmount(resource) == 1 && mode.IsOn, occupied.ToString());
                owner.Body.SecondaryHand.RemoveItem(false);
                CastEvidence free = Cast(attacker, defender, frost, true);
                Check(assertions, rows, prefix + "free-hand-restores-deflection",
                    ElementalCrystallineFormRuntime.HasFreeHand(owner) && free.Completed &&
                    free.Parried == 1 && free.Damage == 0 && owner.Resources.GetResourceAmount(resource) == 0,
                    free + ";native equipment removal requires no rest or trait re-add");
            }
            finally
            {
                if (ReferenceEquals(owner.Body.PrimaryHand.MaybeItem, primary)) owner.Body.PrimaryHand.RemoveItem(false);
                if (ReferenceEquals(owner.Body.SecondaryHand.MaybeItem, secondary)) owner.Body.SecondaryHand.RemoveItem(false);
                primary.Dispose();
                secondary.Dispose();
            }
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            BlueprintItemWeapon greatsword = BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(
                BlueprintBootstrap.Library, "2fff2921851568a4d80ed52f76cccdb6", "audited native StandardGreatsword");
            if (greatsword.Type.AssetGuid != EasternWeaponBlueprints.NodachiVisualDonorGuid || greatsword.Enchantments.Any())
                throw new InvalidOperationException("The audited unenchanted greatsword contract changed.");
            var twoHanded = new ItemEntityWeapon(greatsword);
            try
            {
                owner.Body.PrimaryHand.InsertItem(twoHanded);
                mode.IsOn = true;
                CastEvidence blocked = Cast(attacker, defender, frost, true);
                Check(assertions, rows, prefix + "native-two-handed-weapon",
                    twoHanded.HoldInTwoHands && !ElementalCrystallineFormRuntime.HasFreeHand(owner) &&
                    blocked.Completed && blocked.Hits == 1 && blocked.Parried == 0 && blocked.Damage > 0 &&
                    owner.Resources.GetResourceAmount(resource) == 1, blocked + ";empty off-hand is not a free hand;item=" +
                    greatsword.AssetGuid + "/" + greatsword.name);
            }
            finally
            {
                if (ReferenceEquals(owner.Body.PrimaryHand.MaybeItem, twoHanded)) owner.Body.PrimaryHand.RemoveItem(false);
                twoHanded.Dispose();
            }
            owner.Body.PrimaryHand.RetainDeactivateFlag();
            owner.Body.SecondaryHand.RetainDeactivateFlag();
            try
            {
                mode.IsOn = true;
                CastEvidence blocked = Cast(attacker, defender, frost, true);
                Check(assertions, rows, prefix + "native-disabled-hands",
                    owner.Body.PrimaryHand.Disabled && owner.Body.SecondaryHand.Disabled &&
                    !ElementalCrystallineFormRuntime.HasFreeHand(owner) && blocked.Completed &&
                    blocked.Hits == 1 && blocked.Parried == 0 && blocked.Damage > 0 &&
                    owner.Resources.GetResourceAmount(resource) == 1, blocked.ToString());
            }
            finally
            {
                owner.Body.SecondaryHand.ReleaseDeactivateFlag();
                owner.Body.PrimaryHand.ReleaseDeactivateFlag();
            }
            mode.IsOn = true;
            CastEvidence handsRecovered = Cast(attacker, defender, frost, true);
            Check(assertions, rows, prefix + "native-hands-reenabled",
                ElementalCrystallineFormRuntime.HasFreeHand(owner) && handsRecovered.Completed &&
                handsRecovered.Parried == 1 && handsRecovered.Damage == 0 &&
                owner.Resources.GetResourceAmount(resource) == 0, handsRecovered.ToString());
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            foreach (UnitCondition condition in new[] { UnitCondition.Blindness, UnitCondition.Paralyzed })
            {
                mode.IsOn = true;
                owner.State.AddCondition(condition, null);
                try
                {
                    bool flat = Rulebook.Trigger(new RuleCheckTargetFlatFooted(attacker, defender)).IsFlatFooted;
                    CastEvidence blocked = Cast(attacker, defender, frost, true);
                    Check(assertions, rows, prefix + "native-condition-" + condition,
                        owner.State.HasCondition(condition) && (flat || owner.State.IsHelpless) &&
                        blocked.Completed && blocked.Parried == 0 && blocked.Damage > 0 &&
                        owner.Resources.GetResourceAmount(resource) == 1,
                        blocked + ";flatFooted=" + flat + ";helpless=" + owner.State.IsHelpless);
                }
                finally { owner.State.RemoveCondition(condition); }
            }
            mode.IsOn = true;
            CastEvidence recovered = Cast(attacker, defender, frost, true);
            Check(assertions, rows, prefix + "awareness-restored",
                recovered.Completed && recovered.Parried == 1 && recovered.Damage == 0 &&
                owner.Resources.GetResourceAmount(resource) == 0, recovered.ToString());
        }

        private static void MultipleAndNonDamageRays(UnitEntityData attacker, UnitEntityData defender,
            BlueprintAbilityResource resource, ActivatableAbility mode, string prefix, JArray rows,
            ICollection<RuntimeTestAssertion> assertions)
        {
            UnitDescriptor owner = defender.Descriptor;
            BlueprintCharacterClass wizard = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                BlueprintBootstrap.Library, "ba34257984f4c41408ce1dc2004e342e", "native ray spellbook");
            attacker.Stats.Intelligence.BaseValue = 20;
            ElementalSpellAffinityScenario.Advance(attacker.Descriptor, wizard,
                11 - attacker.Descriptor.Progression.GetClassLevel(wizard));
            Spellbook book = attacker.Descriptor.GetSpellbook(wizard);
            if (book == null) throw new InvalidOperationException("Native ray class spellbook is absent.");
            int originalBookLevel = book.CasterLevel;
            // The existing native fixture level-up helper applies class
            // mechanics, not spell-learning steps. As in the summon fixture,
            // initialize the real book via its native level API, never a
            // per-cast CL override or a replacement parameter component.
            for (int level = 0; book.CasterLevel < 11 && level < 11; level++) book.AddCasterLevel();
            Check(assertions, rows, prefix + "native-spellbook-fixture",
                attacker.Descriptor.Progression.GetClassLevel(wizard) == 11 && book.CasterLevel == 11,
                "WizardClass=" + attacker.Descriptor.Progression.GetClassLevel(wizard) +
                ";bookBefore=" + originalBookLevel + ";nativeBookAfter=" + book.CasterLevel);
            if (book.CasterLevel != 11)
                throw new InvalidOperationException("Native multi-ray witness requires CL11; observed " + book.CasterLevel);
            book.UpdateAllSlotsSize(false);
            owner.Stats.HitPoints.BaseValue = 5000;
            defender.Damage = 0;
            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            mode.IsOn = true;
            BlueprintAbility scorching = Require("cdb106d53c65bbc4086183d54c3b97c7");
            AbilityData prepared = Prepare(book, scorching);
            CastEvidence multi = Cast(attacker, defender, scorching, true, true, prepared);
            Check(assertions, rows, prefix + "three-native-rays-one-deflection",
                prepared.CalculateParams().CasterLevel == 11 && multi.Completed && multi.Projectiles == 3 &&
                multi.UniqueAttackRolls == 3 && multi.Parried == 1 && multi.Hits == 2 && multi.Damage > 0 &&
                multi.ImpactNotifications == 6 && owner.Resources.GetResourceAmount(resource) == 0 &&
                !mode.IsOn && !prepared.ParamSpellSlot.Available && multi.ClockAdvancedSeconds > 0 &&
                multi.ClockRestored,
                multi + ";real prepared Wizard spell; duplicate native impact notifications cannot spend twice");

            Kingmaker.Controllers.Rest.RestController.ApplyRest(owner);
            mode.IsOn = false;
            BlueprintAbility enfeeblement = Require("450af0402422b0b4980d9c2175869612");
            var before = owner.Buffs.Enumerable.ToArray();
            int strength = owner.Stats.Strength.ModifiedValue;
            CastEvidence ordinary = Cast(attacker, defender, enfeeblement, true, false, Prepare(book, enfeeblement));
            var applied = owner.Buffs.Enumerable.Except(before).ToArray();
            Check(assertions, rows, prefix + "non-damage-ray-positive-control",
                ordinary.Completed && ordinary.Hits == 1 && ordinary.Parried == 0 && applied.Length > 0 &&
                owner.Stats.Strength.ModifiedValue < strength && owner.Resources.GetResourceAmount(resource) == 1,
                ordinary + ";newBuffs=" + applied.Length + ";strength=" + owner.Stats.Strength.ModifiedValue);
            foreach (var buff in applied) owner.Buffs.RemoveFact(buff);
            before = owner.Buffs.Enumerable.ToArray();
            strength = owner.Stats.Strength.ModifiedValue;
            mode.IsOn = true;
            CastEvidence deflected = Cast(attacker, defender, enfeeblement, true, true, Prepare(book, enfeeblement));
            Check(assertions, rows, prefix + "non-damage-ray-effect-suppressed",
                deflected.Completed && deflected.Parried == 1 && deflected.Hits == 0 &&
                owner.Buffs.Enumerable.Except(before).Count() == 0 && owner.Stats.Strength.ModifiedValue == strength &&
                owner.Resources.GetResourceAmount(resource) == 0 && !mode.IsOn,
                deflected + ";native effect is skipped, not applied and later removed");
        }

        private static AbilityData Prepare(Spellbook book, BlueprintAbility ability)
        {
            if (!book.Blueprint.SpellList.Contains(ability))
                throw new InvalidOperationException("Exact ray is not on the native Wizard list.");
            int level = book.Blueprint.SpellList.GetLevel(ability);
            book.AddKnown(level, ability, true);
            SpellSlot slot = book.GetMemorizedSpellSlots(level).FirstOrDefault(value =>
                value.Spell != null && ReferenceEquals(value.Spell.Blueprint, ability));
            if (slot == null)
            {
                if (!book.Memorize(new AbilityData(ability, book), null))
                    throw new InvalidOperationException("Native ray memorization failed.");
                slot = book.GetMemorizedSpellSlots(level).Single(value =>
                    value.Spell != null && ReferenceEquals(value.Spell.Blueprint, ability));
            }
            book.Rest();
            slot.Spell.ParamSpellSlot = slot;
            if (!slot.Available || !slot.Spell.IsAvailable)
                throw new InvalidOperationException("Native prepared ray slot is unavailable after rest.");
            return slot.Spell;
        }

        private static int ArmorClass(UnitEntityData attacker, UnitEntityData target, BlueprintAbility ability)
        {
            AbilityData data = Data(attacker.Descriptor, ability);
            var context = new AbilityExecutionContext(data, data.CalculateParams(), new TargetWrapper(target), null);
            AbilityDeliverProjectile delivery = ability.ComponentsArray.OfType<AbilityDeliverProjectile>().SingleOrDefault();
            // A foreign rewrite can leave a cataloged identity without native
            // ray delivery. Deliberately pair that context with the known native
            // ray weapon as a negative control; never reconstruct its old spell.
            var weapon = new ItemEntityWeapon((delivery ?? Require(Frost).ComponentsArray
                .OfType<AbilityDeliverProjectile>().Single()).Weapon);
            var roll = new RuleAttackRoll(attacker, target, weapon, 100) { Reason = new RuleReason(context) };
            UnityEngine.Random.InitState(7419);
            Rulebook.Trigger(roll);
            if (roll.ACRule == null) throw new InvalidOperationException("Ray probe did not resolve native AC: " +
                ability.name + ";enemy=" + target.IsEnemy(attacker) + ";autoHit=" + roll.AutoHit +
                ";autoMiss=" + roll.AutoMiss + ";result=" + roll.Result);
            return roll.ACRule.TargetAC;
        }

        private sealed class CastEvidence
        {
            internal bool Completed;
            internal int Projectiles, Hits, Parried, Damage, UniqueAttackRolls, ImpactNotifications;
            internal double ClockAdvancedSeconds;
            internal bool ClockRestored;
            internal string Sources;
            public override string ToString() { return "completed=" + Completed + ";projectiles=" + Projectiles +
                ";hits=" + Hits + ";parried=" + Parried + ";damage=" + Damage +
                ";uniqueRolls=" + UniqueAttackRolls + ";impactNotifications=" + ImpactNotifications +
                ";clockAdvancedSeconds=" + ClockAdvancedSeconds + ";clockRestored=" + ClockRestored + ";sources=" + Sources; }
        }

        private static CastEvidence Cast(UnitEntityData caster, UnitEntityData target, BlueprintAbility ability, bool hit,
            bool duplicateImpact = false, AbilityData prepared = null)
        {
            var observed = new List<Projectile>();
            var previous = new HashSet<Projectile>(Game.Instance.ProjectileController.Projectiles);
            int damage = target.Damage;
            int bonus = caster.Stats.AdditionalAttackBonus.BaseValue;
            int notifications = 0;
            TimeSpan clock = Game.Instance.TimeController.GameTime;
            bool delayed = ability.ComponentsArray.OfType<AbilityDeliverProjectile>()
                .Any(value => value.DelayBetweenProjectiles > 0);
            var evidence = new CastEvidence();
            caster.Stats.AdditionalAttackBonus.BaseValue = hit ? 100 : -100;
            var command = ElementalUndineFeatScenario.CreateCommand(prepared ?? Data(caster.Descriptor, ability), new TargetWrapper(target), caster);
            command.ForceAlwaysHit = hit;
            UnityEngine.Random.InitState(7419);
            try
            {
                ElementalUndineFeatScenario.InvokeCommandAction(command);
                for (int tick = 0; command.ExecutionProcess != null && !command.ExecutionProcess.IsEnded && tick < 100; tick++)
                {
                    // The native delivery iterator compares GameTime with its
                    // unmodified DelayBetweenProjectiles. Drive only that
                    // scheduling boundary, as in the disposable blood fixture;
                    // restore the exact clock even when the command fails.
                    if (delayed) Game.Instance.Player.GameTime += TimeSpan.FromSeconds(0.1);
                    command.ExecutionProcess.Tick();
                    foreach (Projectile projectile in Game.Instance.ProjectileController.Projectiles.Where(value =>
                        !previous.Contains(value) && !value.Cleared && ReferenceEquals(value.Launcher, caster) &&
                        !observed.Contains(value)).ToArray())
                    {
                        observed.Add(projectile);
                        // Complete only the asynchronous transport boundary.
                        // Native OnHit and subsequent delivery/effects are untouched.
                        typeof(Projectile).GetProperty("IsHit").GetSetMethod(true).Invoke(projectile, new object[] { true });
                        projectile.OnHit();
                        notifications++;
                        if (duplicateImpact) { projectile.OnHit(); notifications++; }
                    }
                }
                bool complete = command.ExecutionProcess != null && command.ExecutionProcess.IsEnded;
                if (!complete && command.ExecutionProcess != null) command.ExecutionProcess.Detach();
                ElementalUndineFeatScenario.InvokeCommandEnded(command, !complete);
                evidence = new CastEvidence { Completed = complete, Projectiles = observed.Count,
                    ClockAdvancedSeconds = (Game.Instance.TimeController.GameTime - clock).TotalSeconds,
                    UniqueAttackRolls = observed.Where(value => value.AttackRoll != null).Select(value => value.AttackRoll).Distinct().Count(),
                    ImpactNotifications = notifications,
                    Hits = observed.Count(value => value.AttackRoll != null && value.AttackRoll.IsHit),
                    Parried = observed.Count(value => value.AttackRoll != null && value.AttackRoll.Result == AttackResult.Parried),
                    Damage = target.Damage - damage,
                    Sources = string.Join(",", observed.Select(value => value.AttackRoll == null ? "no-roll" :
                        "eligible=" + ElementalCrystallineFormRuntime.IsRay(value.AttackRoll) + "/reason=" + value.AttackRoll.Reason.Name)) };
                return evidence;
            }
            finally
            {
                Game.Instance.Player.GameTime = clock;
                evidence.ClockRestored = Game.Instance.TimeController.GameTime == clock;
                caster.Stats.AdditionalAttackBonus.BaseValue = bonus;
                foreach (Projectile projectile in observed) projectile.Cleared = true;
                Game.Instance.ProjectileController.Tick();
            }
        }

        private static AbilityData Data(UnitDescriptor owner, BlueprintAbility ability)
        {
            if (owner.Abilities.GetAbility(ability) == null) owner.AddFact(ability);
            return new AbilityData(owner.Abilities.GetAbility(ability));
        }
        private static float[] Cooldowns(UnitEntityData unit) { return new[] {
            unit.CombatState.Cooldown.StandardAction, unit.CombatState.Cooldown.MoveAction,
            unit.CombatState.Cooldown.SwiftAction }; }
        private static BlueprintAbility Require(string guid) { return BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
            BlueprintBootstrap.Library, guid, "exact Crystalline Form native/project ray witness"); }
        private static ActivatableAbility Mode(UnitDescriptor owner, BlueprintActivatableAbility blueprint) {
            return owner.ActivatableAbilities.Enumerable.Single(value => ReferenceEquals(value.Blueprint, blueprint)); }
        private static void Check(ICollection<RuntimeTestAssertion> assertions, JArray rows, string name, bool pass, string observed)
        {
            rows.Add(new JObject { { "name", name }, { "pass", pass }, { "observed", observed } });
            assertions.Add(new RuntimeTestAssertion { Name = "elemental-crystalline-" + name,
                Expected = "true", Observed = observed, Status = pass ? RuntimeTestStatuses.Pass : RuntimeTestStatuses.Fail,
                Evidence = "native disposable command/rule/impact scenario; projectile travel isolated; no save access" });
        }
    }
}
