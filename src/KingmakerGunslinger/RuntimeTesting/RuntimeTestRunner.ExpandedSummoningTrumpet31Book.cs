using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
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
        private static int Trumpet31HealSlots(Spellbook book, Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility heal)
        { return book.GetMemorizedSpellSlots(6).Count(slot => slot.Available && ReferenceEquals(slot.Spell?.Blueprint, heal)); }

        private IEnumerable<int> ReviewTrumpet31Book(ExpandedSummoningCorrectionFixture fixture)
        {
            CaptureCelestial31CasterInputs();
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            UnitEntityData owner = null, sibling = null;
            CreateExpandedSummoningCorrectionHostile(fixture);
            var target = fixture.Hostile;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                PlaceExpandedSummoningUnit(target, fixture.Caster.Position + Vector3.forward * 40f);
                owner = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                sibling = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                foreach (var unit in new[] { owner, sibling })
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                var creation = owner.Blueprint.GetComponent<TrumpetPreparedBook>();
                var book = owner.Descriptor.GetSpellbook(creation.Book);
                var siblingBook = sibling.Descriptor.GetSpellbook(creation.Book);
                var data = new AbilityData(creation.Heal, book);
                var parameters = data.CalculateParams();
                int[] capacities = Enumerable.Range(1, 7).Select(level => book.CalcSlotsLimit(level, SpellSlotType.Common)).ToArray();
                var preparationRows = new JArray(creation.Preparations.Select(row => {
                    var ownSlots = book.GetMemorizedSpellSlots(row.Level).Where(slot =>
                        ReferenceEquals(slot.Spell?.Blueprint, row.Ability)).ToArray();
                    var otherSlots = siblingBook.GetMemorizedSpellSlots(row.Level).Where(slot =>
                        ReferenceEquals(slot.Spell?.Blueprint, row.Ability)).ToArray();
                    var nativeParams = new AbilityData(row.Ability, book).CalculateParams();
                    return new JObject { ["spell"] = row.Ability.AssetGuid, ["name"] = row.Ability.name,
                        ["level"] = row.Level, ["preparedCopies"] = row.Copies,
                        ["ownerCopies"] = ownSlots.Length, ["siblingCopies"] = otherSlots.Length,
                        ["allAvailable"] = ownSlots.Concat(otherSlots).All(slot => slot.Available),
                        ["nativeCl"] = nativeParams.CasterLevel, ["nativeSpellLevel"] = nativeParams.SpellLevel,
                        ["nativeDc"] = nativeParams.DC };
                }));
                bool prepared = preparationRows.Count > 1 && preparationRows.All(row =>
                    (int)row["ownerCopies"] == (int)row["preparedCopies"] &&
                    (int)row["siblingCopies"] == (int)row["preparedCopies"] && (bool)row["allAvailable"] &&
                    (int)row["nativeCl"] == 14 && (int)row["nativeSpellLevel"] == (int)row["level"]);
                var stats = owner.Stats;
                bool nativeBook = prepared && stats.HitPoints.ModifiedValue == 175 && stats.BaseAttackBonus.ModifiedValue == 14 &&
                    stats.AC.ModifiedValue == 27 && stats.SaveFortitude.ModifiedValue == 16 &&
                    stats.SaveReflex.ModifiedValue == 9 && stats.SaveWill.ModifiedValue == 14 &&
                    owner.Descriptor.Progression.CharacterLevel == 14 && owner.Descriptor.Progression.GetClassLevel(creation.Book.CharacterClass) == 0 &&
                    owner.Descriptor.Spellbooks.Count() == 1 && !ReferenceEquals(book, siblingBook) &&
                    book.CasterLevel == 14 && siblingBook.CasterLevel == 14 && book.MaxSpellLevel == 7 &&
                    !book.Blueprint.Spontaneous && parameters.CasterLevel == 14 && parameters.SpellLevel == 6 && parameters.DC == 21 &&
                    capacities.SequenceEqual(new[] { 6, 5, 5, 5, 4, 3, 2 }) &&
                    Trumpet31HealSlots(book, creation.Heal) == 2 && Trumpet31HealSlots(siblingBook, creation.Heal) == 2;
                Sprint25Check("trumpet-book-native-profile", nativeBook, new JObject {
                    ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId, ["hp"] = stats.HitPoints.ModifiedValue,
                    ["bab"] = stats.BaseAttackBonus.ModifiedValue, ["ac"] = stats.AC.ModifiedValue,
                    ["fort"] = stats.SaveFortitude.ModifiedValue, ["reflex"] = stats.SaveReflex.ModifiedValue, ["will"] = stats.SaveWill.ModifiedValue,
                    ["racialHd"] = owner.Descriptor.Progression.CharacterLevel,
                    ["clericClassLevels"] = owner.Descriptor.Progression.GetClassLevel(creation.Book.CharacterClass),
                    ["book"] = book.Blueprint.AssetGuid, ["independentBooks"] = !ReferenceEquals(book, siblingBook),
                    ["nativeCapacities"] = new JArray(capacities), ["cl"] = parameters.CasterLevel, ["spellLevel"] = parameters.SpellLevel,
                    ["dc"] = parameters.DC, ["heal"] = creation.Heal.AssetGuid, ["ownerHealSlots"] = Trumpet31HealSlots(book, creation.Heal),
                    ["siblingHealSlots"] = Trumpet31HealSlots(siblingBook, creation.Heal), ["preparations"] = preparationRows, ["fullCasterQualification"] = false },
                    "Actual14 racial HD and genuine CL14 prepared book/capacities/Heal slots; no cleric class statistics, domains or SLA parameter override.");
                if (!nativeBook) throw new InvalidOperationException("Trumpet native prepared-book profile failed.");
                Vector3 far; string lane, direction;
                var origin = Sprint24AiCombatLane(fixture.Caster, owner, target, 8f, out far, out lane, out direction,
                    new Feet(20).Meters + 1f);
                PlaceExpandedSummoningUnit(owner, origin); PlaceExpandedSummoningUnit(sibling, origin + Vector3.right);
                PlaceExpandedSummoningUnit(target, far);
                owner.Damage = 100;
                for (int frame = 0; frame < 8; frame++) yield return 0;
                foreach (var unit in new[] { owner, target })
                { unit.Wake(0f); if (!Game.Instance.State.AwakeUnits.Contains(unit)) Game.Instance.State.AwakeUnits.Add(unit); }
                owner.Memory.Add(target); target.Memory.Add(owner); owner.SightCache.ClearIfNeeded(); target.SightCache.ClearIfNeeded();
                new Kingmaker.Controllers.Units.UnitMemoryController().Tick();
                owner.JoinCombat(); target.JoinCombat(); Game.Instance.Player.UpdateIsInCombat();
                new UnitCombatJoinController().Tick(); new UnitCombatPrepareController().Tick();
                Sprint28SetBrainActivePreservingActions(owner, true); owner.Brain.RestoreAvailableActions();
                var commands = new HashSet<UnitCommand>(); var ambient = new HashSet<string>(); var costs = new JArray();
                using (var economy = new FirearmReviewCosts(owner, costs))
                {
                    DateTime deadline = DateTime.UtcNow.AddSeconds(30); int frames = 0, ticks = 0;
                    while (DateTime.UtcNow < deadline && frames++ < 3000)
                    {
                        if (frames == 1 || frames % 60 == 0 && !owner.Commands.Raw.Any(value => value != null && !value.IsFinished))
                        { foreach (var candidate in Sprint24TickExactAi(owner)) if (candidate != owner.UniqueId) ambient.Add(candidate); ticks++; }
                        foreach (var command in owner.Commands.Raw.Where(value => value != null)) commands.Add(command);
                        if (commands.OfType<UnitUseAbility>().Any(value => value.AiAction != null && value.IsFinished &&
                            value.ExecutionProcess != null && value.ExecutionProcess.IsEnded)) break;
                        Game.Instance.IsPaused = false; yield return 0;
                    }
                    var cast = commands.OfType<UnitUseAbility>().FirstOrDefault(value =>
                        value.AiAction?.name == "KMG_Summoning_OutsiderCelestial_TrumpetArchon_HealAi" &&
                        value.IsFinished && value.Result == UnitCommand.ResultType.Success);
                    bool healed = cast != null && !cast.Cutscene && !cast.IsIgnoreCooldown && economy.Count(cast) == 1 &&
                        economy.Exact(cast, false) && owner.Damage == 0 && Trumpet31HealSlots(book, creation.Heal) == 1 &&
                        Trumpet31HealSlots(siblingBook, creation.Heal) == 2;
                    Sprint25Check("trumpet-book-native-heal-ai", healed, new JObject {
                        ["owner"] = owner.UniqueId, ["aiTicks"] = ticks, ["commands"] = new JArray(commands.Select(Sprint27AiCommand)),
                        ["costs"] = costs, ["fixtureInjury"] = 100, ["damageAfter"] = owner.Damage,
                        ["ownerHealSlotsAfter"] = Trumpet31HealSlots(book, creation.Heal), ["siblingHealSlotsAfter"] = Trumpet31HealSlots(siblingBook, creation.Heal),
                        ["ambientCandidatesNeverTicked"] = new JArray(ambient), ["fixtureCasts"] = 0, ["fixtureSlotDebits"] = 0, ["forcedRolls"] = 0 },
                        "Actual native AI selects prepared Heal, pays a standard action, heals its injured owner and spends exactly one own slot; sibling independent.");
                    if (!healed) throw new InvalidOperationException("Trumpet native prepared Heal AI/slot path failed.");
                }
                Sprint28SetBrainActivePreservingActions(owner, false);
                var eligibility = fixture.Blueprints.OfType<TrumpetPreparedHealingEligibility>().Single();
                float score = eligibility.Score(new Kingmaker.Controllers.Brain.DecisionContext {
                    Unit = owner, Target = new Kingmaker.Controllers.Brain.TargetInfo { Unit = owner } });
                owner.Descriptor.GetFact(owner.Blueprint.AddFacts.Single(fact => fact.name == "KMG_Summoning_OutsiderCelestial_TrumpetArchon_Traits")).Recalculate();
                bool retained = score == 0f && Trumpet31HealSlots(book, creation.Heal) == 1 && Trumpet31HealSlots(siblingBook, creation.Heal) == 2;
                Sprint25Check("trumpet-book-no-refresh", retained, new JObject { ["scoreOnHealthyTarget"] = score,
                    ["ownerHealSlots"] = Trumpet31HealSlots(book, creation.Heal), ["siblingHealSlots"] = Trumpet31HealSlots(siblingBook, creation.Heal) },
                    "Healthy target is not healed again; native trait recalculation never refreshes a spent prepared slot.");
                if (!retained) throw new InvalidOperationException("Trumpet slot refresh/healthy-target control failed.");
                var routes = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster).Where(value => value.Creature.Key == "trumpet-archon").ToArray();
                int publicReferences;
                bool hidden = routes.Length == 1 && routes[0].ParentTier == 9 && !SummonVisibilityCatalog.IsPublished(routes[0]) &&
                    ExpandedSummoningPublisher.RequiredBasePublicationIsExact(BlueprintBootstrap.Library, true, out publicReferences);
                Sprint25Check("trumpet-book-hidden-route", hidden, new JObject { ["routes"] = routes.Length, ["tier"] = routes.Single().ParentTier },
                    "Exactly one hidden SM IX Trumpet route; released public roots remain exact.");
                if (!hidden) throw new InvalidOperationException("Trumpet hidden route/public preservation failed.");
                foreach (int frame in ReviewTrumpet31Purge(fixture, owner, sibling)) yield return frame;
            }
            finally
            {
                foreach (var unit in new[] { owner, sibling }.Where(value => value != null && !value.Destroyed))
                { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); }
                DisposeExpandedSummoningUnits(fixture.Created, new[] { owner, sibling }.Where(value => value != null && !value.Destroyed).ToArray());
                ResetExpandedSummoningHostile(fixture);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }
    }
}
