using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
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
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // First prepared Dispel Evil path. Full dismissal, expiry and turn
        // coverage remain separate; this proves the book/variant/touch risk.
        private IEnumerable<int> ReviewTrumpet31Dispel(
            ExpandedSummoningCorrectionFixture fixture)
        {
            bool oldMode = SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue;
            bool oldPause = Game.Instance.IsPaused;
            var areaBaseline = Game.Instance.State.AreaEffects.All.ToArray();
            UnitEntityData owner = null, sibling = null, target = null;
            var transient = new List<UnityEngine.Object>();
            var fixtureBuffs = new List<Buff>();
            Astral31DispelEvents observed = null;
            UnitPartSpellResistance resistance = null;
            int resistanceId = -1;
            AbilityExecutionContext resistanceContext = null;
            JObject resistanceBefore = null;
            int sharedCl = AbilityParams.Empty.CasterLevel;
            int sharedLevel = AbilityParams.Empty.SpellLevel;
            int sharedDc = AbilityParams.Empty.DC;
            try
            {
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = false;
                Game.Instance.TurnBasedCombatController.Activate();
                Game.Instance.IsPaused = false;
                for (int frame = 0; frame < 16; frame++) yield return 0;
                CreateExpandedSummoningCorrectionHostile(fixture);
                target = fixture.Hostile;
                owner = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                sibling = CastExpandedSummoningOwnTier(fixture, "trumpet-archon");
                foreach (var unit in new[] { owner, sibling })
                {
                    Sprint28SetBrainActivePreservingActions(unit, false);
                    unit.Commands.InterruptAll(true);
                    unit.Commands.RemoveFinishedAndUpdateQueue();
                }
                Vector3 origin = fixture.Caster.Position;
                PlaceExpandedSummoningUnit(owner, origin);
                PlaceExpandedSummoningUnit(target, origin + Vector3.forward * 1.5f);
                PlaceExpandedSummoningUnit(sibling, origin + Vector3.right * 2f);
                PlaceExpandedSummoningUnit(fixture.Caster, origin + Vector3.back * 14f);
                foreach (var unit in new[] { owner, sibling, target })
                {
                    unit.Wake(0f);
                    if (!Game.Instance.State.AwakeUnits.Contains(unit))
                        Game.Instance.State.AwakeUnits.Add(unit);
                    unit.SightCache.ClearIfNeeded();
                    unit.JoinCombat();
                }
                owner.Memory.Add(target); target.Memory.Add(owner);
                Game.Instance.Player.UpdateIsInCombat();
                new Kingmaker.Controllers.Combat.UnitCombatJoinController().Tick();
                new Kingmaker.Controllers.Combat.UnitCombatPrepareController().Tick();
                for (int frame = 0; frame < 4; frame++) yield return 0;

                string[] newKeys = { "lemure", "dretch", "hound-archon",
                    "astral-deva", "trumpet-archon" };
                var hidden = ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Where(value => newKeys.Contains(value.Creature.Key)).ToArray();
                int publicReferences;
                bool publication = ExpandedSummoningPublisher.RequiredBasePublicationIsExact(
                    KingmakerGunslinger.Bootstrap.BlueprintBootstrap.Library, true,
                    out publicReferences) && hidden.Length == 23 &&
                    hidden.All(value => !SummonVisibilityCatalog.IsPublished(value));
                Sprint25Check("trumpet-dispel-hidden-publication", publication,
                    new JObject { ["publicReferences"] = publicReferences,
                        ["registeredRoots"] = SummonVisibilityCatalog.RegisteredLogicalPlacementCount,
                        ["suppressedRoots"] = SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                        ["hiddenRoutes"] = new JArray(hidden.Select(value => value.StableKey)) },
                    "The real hidden spell dependencies bootstrap while all 23 new roots remain suppressed and released choices stay exact.");
                if (!publication) throw new InvalidOperationException("Hidden Trumpet publication changed.");

                string prefix = ExpandedSummoningSpecialBuilder.Trumpet31Prefix;
                var family = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "DispelEvil"];
                var heldMenu = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "DispelEvilHeld"];
                var dismissalTouch = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "DispelEvilDismissalTouch"];
                var dismissal = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "DispelEvilDismissal"];
                var dispel = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "DispelEvilDispel"];
                var touchBlueprint = (BlueprintAbility)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "DispelEvilDispelTouch"];
                var ward = (BlueprintBuff)ExpandedSummoningBlueprints.Registered.BySymbol[prefix + "DispelEvilWard"];
                var creation = owner.Blueprint.GetComponent<TrumpetPreparedBook>();
                Spellbook book = owner.Descriptor.GetSpellbook(creation.Book);
                Spellbook siblingBook = sibling.Descriptor.GetSpellbook(creation.Book);
                var slot = book.GetMemorizedSpellSlots(5).Single(value => value.Available &&
                    ReferenceEquals(value.Spell?.Blueprint, family));
                slot.Spell.ParamSpellSlot = slot;
                AbilityData root = slot.Spell;
                var data = root;
                var parameters = data.CalculateParams();
                var dismissalParameters = data.CalculateParams();
                JArray siblingBefore = Trumpet31DispelSlotRows(siblingBook);
                JArray otherSlotsBefore = Trumpet31DispelSlotRows(book, family);
                var preparation = creation.Preparations.Single(value => ReferenceEquals(value.Ability, family));
                bool mapping = family.Type == AbilityType.Spell && family.Icon != null &&
                    family.GetComponent<AbilityVariants>() == null &&
                    family.GetComponent<AbilityEffectArmAstralDispelEvilTouch>()?.TouchDeliveryAbility == heldMenu &&
                    heldMenu.Variants.SequenceEqual(new[] { dismissalTouch, touchBlueprint }) &&
                    ReferenceEquals(dismissalTouch.Parent, heldMenu) && ReferenceEquals(touchBlueprint.Parent, heldMenu) &&
                    family.GetComponent<SpellComponent>()?.School == SpellSchool.Abjuration &&
                    family.GetComponent<SpellDescriptorComponent>() != null &&
                    (family.GetComponent<SpellDescriptorComponent>().Descriptor & SpellDescriptor.Good) != 0 &&
                    dismissal.Type == AbilityType.Spell && dispel.Type == AbilityType.Spell &&
                    touchBlueprint.Type == AbilityType.Spell && !touchBlueprint.SpellResistance &&
                    preparation.Level == 5 && preparation.Copies == 1 &&
                    Trumpet31PreparedCopies(book, family, 5) == 1 &&
                    Trumpet31PreparedCopies(siblingBook, family, 5) == 1 &&
                    !ReferenceEquals(book, siblingBook) && book.CasterLevel == 14 &&
                    book.Blueprint.CastingAttribute == StatType.Wisdom &&
                    owner.Descriptor.Progression.CharacterLevel == 14 &&
                    owner.Descriptor.Progression.GetClassLevel(creation.Book.CharacterClass) == 0 &&
                    ReferenceEquals(data.Spellbook, book) && data.IsAvailable &&
                    parameters.CasterLevel == 14 && parameters.SpellLevel == 5 && parameters.DC == 20 &&
                    dismissalParameters.CasterLevel == 14 && dismissalParameters.SpellLevel == 5 &&
                    dismissalParameters.DC == 20 &&
                    family.GetComponent<AbilityResourceLogic>() == null &&
                    dispel.GetComponent<AbilityResourceLogic>() == null;
                Sprint25Check("trumpet-dispel-native-mapping", mapping,
                    new JObject { ["owner"] = owner.UniqueId, ["sibling"] = sibling.UniqueId,
                        ["book"] = book.Blueprint.AssetGuid, ["family"] = family.AssetGuid,
                        ["castRoot"] = data.Blueprint.AssetGuid,
                        ["heldMenu"] = heldMenu.AssetGuid,
                        ["dischargeChoices"] = new JArray(heldMenu.Variants.Select(value => value.AssetGuid)), ["touch"] = touchBlueprint.AssetGuid,
                        ["cl"] = parameters.CasterLevel, ["spellLevel"] = parameters.SpellLevel,
                        ["dc"] = parameters.DC, ["castingStat"] = book.Blueprint.CastingAttribute.ToString(),
                        ["racialHd"] = owner.Descriptor.Progression.CharacterLevel,
                        ["clericLevels"] = owner.Descriptor.Progression.GetClassLevel(creation.Book.CharacterClass),
                        ["type"] = family.Type.ToString(), ["icon"] = family.Icon?.name,
                        ["preparationCopies"] = preparation.Copies,
                        ["ownerSlots"] = Trumpet31DispelSlotRows(book), ["siblingSlots"] = siblingBefore },
                    "Dispel Evil is a real CL14/Wisdom level5 preparation with both native discharge choices retained after casting and independent owned books, without a resource/SLA or cleric-stat progression.");
                if (!mapping) throw new InvalidOperationException("Trumpet Dispel Evil book mapping failed.");

                observed = new Astral31DispelEvents { Owner = owner };
                observed.Targets.Add(target); EventBus.Subscribe(observed);
                Buff selected = Astral31DispelFixtureEffect(target, fixture.Caster,
                    "TrumpetEvilSpell", Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"),
                    9, SpellSchool.Necromancy, SpellDescriptor.Evil, false,
                    transient, fixtureBuffs, observed);
                resistanceContext = new AbilityExecutionContext(data, parameters,
                    new TargetWrapper(target), null);
                resistanceBefore = Lemure30SrState(target, resistanceContext);
                resistance = target.Ensure<UnitPartSpellResistance>();
                resistanceId = resistance.AddResistance(1000, null, null);

                UnitUseAbility parent = null;
                var parentCosts = new JArray();
                using (var economy = new FirearmReviewCosts(owner, parentCosts))
                {
                    foreach (int frame in Astral31DispelCast(owner, data,
                        new TargetWrapper(owner), value => parent = value)) yield return frame;
                    Buff activeWard = owner.Buffs.Enumerable.SingleOrDefault(value => ReferenceEquals(value.Blueprint, ward));
                    var held = owner.Get<UnitPartTouch>();
                    var heldData = held?.Ability?.Data;
                    var heldParameters = heldData?.CalculateParams();
                    bool cast = parent != null && parent.IsFinished &&
                        parent.Result == UnitCommand.ResultType.Success && parent.ExecutionProcess != null &&
                        parent.ExecutionProcess.IsEnded && parent.AiAction == null && !parent.Cutscene &&
                        !parent.IsIgnoreCooldown && economy.Count(parent) == 1 && economy.Exact(parent, false) &&
                        !slot.Available && Trumpet31PreparedCopies(book, family, 5) == 0 &&
                        activeWard != null && activeWard.IsFromSpell &&
                        ReferenceEquals(activeWard.Context?.MaybeCaster, owner) &&
                        activeWard.Context.Params.CasterLevel == 14 &&
                        activeWard.TimeLeft.TotalSeconds > 75d && activeWard.TimeLeft.TotalSeconds <= 84d &&
                        ReferenceEquals(heldData?.Blueprint, heldMenu) &&
                        heldMenu.Variants.SequenceEqual(new[] { dismissalTouch, touchBlueprint }) &&
                        ReferenceEquals(heldData.StickyTouch, data) &&
                        ReferenceEquals(heldData.Spellbook, book) &&
                        heldParameters != null && heldParameters.CasterLevel == 14 &&
                        heldParameters.SpellLevel == 5 && heldParameters.DC == 20;
                    Sprint25Check("trumpet-dispel-prepared-parent", cast,
                        new JObject { ["owner"] = owner.UniqueId,
                            ["command"] = parent == null ? null : Sprint27AiCommand(parent),
                            ["costs"] = parentCosts, ["nativeSlotAvailable"] = slot.Available,
                            ["ownCopiesAfter"] = Trumpet31PreparedCopies(book, family, 5),
                            ["wardSeconds"] = activeWard?.TimeLeft.TotalSeconds,
                            ["wardCaster"] = activeWard?.Context?.MaybeCaster?.UniqueId,
                            ["held"] = heldData?.Blueprint.AssetGuid,
                            ["heldBook"] = heldData?.Spellbook?.Blueprint.AssetGuid,
                            ["heldCl"] = heldParameters?.CasterLevel,
                            ["heldLevel"] = heldParameters?.SpellLevel, ["heldDc"] = heldParameters?.DC,
                            ["fixtureSlotDebits"] = 0 },
                        "One real standard cast spends the owner's one level5 preparation, applies its14-round ward and retains CL14/level5/DC20 and the exact native book on the held delivery.");
                    if (!cast) throw new InvalidOperationException("Prepared Dispel Evil parent/slot failed.");
                }

                UnitUseAbility delivery = null;
                var deliveryCosts = new JArray();
                using (var economy = new FirearmReviewCosts(owner, deliveryCosts))
                {
                    foreach (int frame in Astral31DispelHeld(owner, target, touchBlueprint,
                        value => delivery = value)) yield return frame;
                    var rule = observed.Dispels.LastOrDefault(value => ReferenceEquals(value.Buff, selected));
                    var sr = observed.Resistances.Where(value => ReferenceEquals(value.Ability, touchBlueprint)).ToArray();
                    var saves = observed.Saves.Where(value => ReferenceEquals(value.Reason?.Context?.SourceAbility, touchBlueprint)).ToArray();
                    bool removed = delivery != null && delivery.IsFinished &&
                        delivery.Result == UnitCommand.ResultType.Success && delivery.ExecutionProcess != null &&
                        delivery.ExecutionProcess.IsEnded && delivery.AiAction == null &&
                        economy.Count(delivery) == 1 && economy.Unchanged(delivery) &&
                        rule != null && rule.Success && rule.Check == RuleDispelMagic.CheckType.None &&
                        rule.CheckRoll == 0 && selected.IsDisposed && !target.Buffs.Enumerable.Contains(selected) &&
                        sr.Length == 1 && sr[0].SpellResistance == 1000 &&
                        !sr[0].HasResistanceRoll && !sr[0].TargetIsImmune && !sr[0].IsSpellResisted &&
                        saves.Length == 0 && !slot.Available &&
                        Trumpet31PreparedCopies(book, family, 5) == 0 && owner.Get<UnitPartTouch>() == null &&
                        !owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, ward));
                    Sprint25Check("trumpet-dispel-held-native-removal", removed,
                        new JObject { ["owner"] = owner.UniqueId, ["target"] = target.UniqueId,
                            ["command"] = delivery == null ? null : Sprint27AiCommand(delivery),
                            ["costs"] = deliveryCosts, ["nativeDispel"] = rule?.Success,
                            ["checkType"] = rule?.Check.ToString(), ["checkRoll"] = rule?.CheckRoll,
                            ["removedBuff"] = selected.Blueprint.AssetGuid,
                            ["removedCl"] = selected.Context.Params.CasterLevel,
                            ["resistanceEvents"] = sr.Length,
                            ["resistanceChecks"] = sr.Count(value => value.HasResistanceRoll),
                            ["saves"] = saves.Length, ["ownCopiesAfterTouch"] = Trumpet31PreparedCopies(book, family, 5),
                            ["wardAbsent"] = !owner.Buffs.Enumerable.Any(value => ReferenceEquals(value.Blueprint, ward)),
                            ["heldAbsent"] = owner.Get<UnitPartTouch>() == null,
                            ["nativeSeededManualControl"] = true, ["aiCommands"] = 0 },
                        "The native held melee touch removes one actual eligible effect through no-check RuleDispelMagic, bypasses SR1000 without a save, closes its own ward/touch and spends no second slot or action.");
                    if (!removed) throw new InvalidOperationException("Prepared Dispel Evil held removal failed.");
                }

                bool independent = JToken.DeepEquals(otherSlotsBefore, Trumpet31DispelSlotRows(book, family)) &&
                    JToken.DeepEquals(siblingBefore, Trumpet31DispelSlotRows(siblingBook)) &&
                    !root.IsAvailable && !data.IsAvailable && !slot.Available &&
                    Trumpet31PreparedCopies(book, family, 5) == 0 &&
                    Trumpet31PreparedCopies(siblingBook, family, 5) == 1 &&
                    AbilityParams.Empty.CasterLevel == sharedCl &&
                    AbilityParams.Empty.SpellLevel == sharedLevel && AbilityParams.Empty.DC == sharedDc;
                Sprint25Check("trumpet-dispel-independent-slots", independent,
                    new JObject { ["ownerOtherSlotsBefore"] = otherSlotsBefore,
                        ["ownerOtherSlotsAfter"] = Trumpet31DispelSlotRows(book, family),
                        ["siblingBefore"] = siblingBefore, ["siblingAfter"] = Trumpet31DispelSlotRows(siblingBook),
                        ["spentRootAvailable"] = root.IsAvailable, ["spentVariantAvailable"] = data.IsAvailable,
                        ["sharedEmptyBefore"] = new JArray(sharedCl, sharedLevel, sharedDc),
                        ["sharedEmptyAfter"] = new JArray(AbilityParams.Empty.CasterLevel,
                            AbilityParams.Empty.SpellLevel, AbilityParams.Empty.DC) },
                    "The spent family and variant cannot refresh themselves; every other preparation and the sibling book remain exact and global default parameters are unchanged.");
                if (!independent) throw new InvalidOperationException("Prepared Dispel Evil slot isolation failed.");

                // A distinct owned emitter observes native holding-charge
                // cancellation. Preserve the later spell's actual effect.
                var siblingSlot = siblingBook.GetMemorizedSpellSlots(5).Single(value =>
                    value.Available && ReferenceEquals(value.Spell?.Blueprint, family));
                siblingSlot.Spell.ParamSpellSlot = siblingSlot;
                var siblingCosts = new JArray();
                UnitUseAbility siblingParent = null;
                bool siblingCast;
                using (var economy = new FirearmReviewCosts(sibling, siblingCosts))
                {
                    foreach (int frame in Astral31DispelCast(sibling, siblingSlot.Spell,
                        new TargetWrapper(sibling), value => siblingParent = value)) yield return frame;
                    siblingCast = siblingParent != null && siblingParent.IsFinished &&
                        siblingParent.Result == UnitCommand.ResultType.Success &&
                        economy.Count(siblingParent) == 1 && economy.Exact(siblingParent, false);
                }
                Buff siblingWard = sibling.Buffs.Enumerable.SingleOrDefault(value =>
                    ReferenceEquals(value.Blueprint, ward));
                bool siblingArmed = siblingCast && siblingWard != null &&
                    ReferenceEquals(sibling.Get<UnitPartTouch>()?.Ability.Data.Blueprint, heldMenu) &&
                    !siblingSlot.Available;
                if (!siblingArmed) throw new InvalidOperationException(
                    "Intervening-cast fixture did not arm its own prepared Dispel Evil.");
                var virtue = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    KingmakerGunslinger.Bootstrap.BlueprintBootstrap.Library,
                    "d3a852385ba4cd740992d1970170301a", "native Virtue orison");
                var virtueBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                    KingmakerGunslinger.Bootstrap.BlueprintBootstrap.Library,
                    "a13ad2502d9e4904082868eb71efb0c5", "native Virtue temporary HP");
                JArray beforeVirtue = Trumpet31DispelSlotRows(siblingBook);
                foreach (int frame in Trumpet31ManualCommand(sibling,
                    new AbilityData(sibling.Descriptor.Abilities.GetAbility(virtue)),
                    sibling, "trumpet-dispel-intervening-command")) yield return frame;
                Buff actualVirtue = sibling.Buffs.Enumerable.SingleOrDefault(value =>
                    ReferenceEquals(value.Blueprint, virtueBuff));
                bool canceled = siblingWard.IsDisposed &&
                    !sibling.Buffs.Enumerable.Contains(siblingWard) &&
                    sibling.Get<UnitPartTouch>() == null && actualVirtue != null &&
                    ReferenceEquals(actualVirtue.Context?.MaybeCaster, sibling) &&
                    actualVirtue.Context.Params.CasterLevel == 14 &&
                    sibling.Descriptor.TemporaryHP == 1 &&
                    JToken.DeepEquals(beforeVirtue, Trumpet31DispelSlotRows(siblingBook));
                Sprint25Check("trumpet-dispel-intervening-cast", canceled,
                    new JObject { ["owner"] = sibling.UniqueId,
                        ["priorWard"] = ward.AssetGuid, ["wardDisposed"] = siblingWard.IsDisposed,
                        ["wardPresent"] = sibling.Buffs.Enumerable.Contains(siblingWard),
                        ["heldAfter"] = sibling.Get<UnitPartTouch>()?.Ability.Data.Blueprint.AssetGuid,
                        ["siblingParent"] = Sprint27AiCommand(siblingParent), ["siblingCosts"] = siblingCosts,
                        ["nextSpell"] = virtue.AssetGuid, ["nextBuff"] = actualVirtue?.Blueprint.AssetGuid,
                        ["nextCaster"] = actualVirtue?.Context?.MaybeCaster?.UniqueId,
                        ["nextCl"] = actualVirtue?.Context?.Params.CasterLevel,
                        ["temporaryHp"] = sibling.Descriptor.TemporaryHP,
                        ["beforeSlots"] = beforeVirtue, ["afterSlots"] = Trumpet31DispelSlotRows(siblingBook) },
                    "A later real native spell closes only the held Dispel Evil ward/charge and preserves its own Virtue effect and every leveled preparation.");
                if (!canceled) throw new InvalidOperationException(
                    "Native intervening spell did not close the prior Dispel Evil charge/ward.");

                EventBus.Unsubscribe(observed); observed = null;
                resistance.Remove(resistanceId); resistanceId = -1;
                JObject resistanceAfter = Lemure30SrState(target, resistanceContext);
                bool srRestored = JToken.DeepEquals(resistanceBefore["entries"], resistanceAfter["entries"]) &&
                    (int)resistanceBefore["nativeValue"] == (int)resistanceAfter["nativeValue"];
                var units = new[] { owner, sibling };
                var grants = target.Buffs.Enumerable.Where(value => units.Contains(value.Context?.MaybeCaster)).ToArray();
                var weapons = units.Select(value => value.Body.PrimaryHand.MaybeWeapon).ToArray();
                var items = units.SelectMany(value => value.Descriptor.Inventory.Items)
                    .Concat(weapons.Where(value => value != null)).Distinct().ToArray();
                foreach (var unit in units)
                { unit.Commands.InterruptAll(true); if (unit.IsInCombat) unit.LeaveCombat(); }
                DisposeExpandedSummoningUnits(fixture.Created, units);
                string reset = ResetExpandedSummoningHostile(fixture);
                int cleanupFrames = 0; var deadline = DateTime.UtcNow.AddSeconds(8);
                while (DateTime.UtcNow < deadline && cleanupFrames++ < 1000 &&
                    (!Game.Instance.State.AreaEffects.All.SequenceEqual(areaBaseline) ||
                        grants.Any(value => !value.IsDisposed && target.Buffs.Enumerable.Contains(value)))) yield return 0;
                foreach (var unit in units)
                { unit.Commands.InterruptAll(true); unit.Commands.RemoveFinishedAndUpdateQueue(); }
                foreach (UnityEngine.Object value in transient.Where(value => value != null).Reverse<UnityEngine.Object>())
                    UnityEngine.Object.DestroyImmediate(value);
                bool cleanup = srRestored && weapons.All(value => value != null) &&
                    !ReferenceEquals(weapons[0], weapons[1]) && items.Length >= 2 && units.All(value => value.Destroyed && !value.IsInState &&
                    !value.Commands.Raw.Any(command => command != null && !command.IsFinished)) &&
                    Game.Instance.State.AreaEffects.All.SequenceEqual(areaBaseline) &&
                    grants.All(value => value.IsDisposed || !target.Buffs.Enumerable.Contains(value)) &&
                    fixtureBuffs.All(value => value.IsDisposed || !target.Buffs.Enumerable.Contains(value)) &&
                    transient.All(value => value == null) &&
                    items.All(value => !value.IsLootable && value.Collection == null) &&
                    target.Damage == fixture.HostileDamage;
                Sprint25Check("trumpet-dispel-owned-cleanup", cleanup,
                    new JObject { ["ownersAbsent"] = units.All(value => value.Destroyed && !value.IsInState),
                        ["areasExact"] = Game.Instance.State.AreaEffects.All.SequenceEqual(areaBaseline),
                        ["capturedAuraGrantsAbsent"] = grants.All(value => value.IsDisposed || !target.Buffs.Enumerable.Contains(value)),
                        ["fixtureResistanceRestored"] = srRestored, ["fixtureResistanceAfter"] = resistanceAfter,
                        ["privateFixtureObjectsGone"] = transient.All(value => value == null),
                        ["ownedItemsObserved"] = items.Length,
                        ["ownedItemsNonlootableAbsent"] = items.All(value => !value.IsLootable && value.Collection == null),
                        ["hostileReset"] = reset, ["cleanupFrames"] = cleanupFrames,
                        ["freshProcessBoundaryRequired"] = true, ["fullDispelQualification"] = false },
                    "The focused save-free process retires both owned casters, items, aura grants, held state and private fixture objects, restores native SR/hostile state and ends before another case.");
                if (!cleanup) throw new InvalidOperationException("Prepared Dispel Evil owned cleanup failed.");
            }
            finally
            {
                if (observed != null) EventBus.Unsubscribe(observed);
                if (resistance != null && resistanceId >= 0) resistance.Remove(resistanceId);
                if (target != null && !target.Destroyed)
                    foreach (Buff value in fixtureBuffs.Where(value => value != null && !value.IsDisposed).ToArray()) value.Remove();
                var units = new[] { owner, sibling }.Where(value => value != null && !value.Destroyed).ToArray();
                foreach (var unit in units) { Sprint28SetBrainActivePreservingActions(unit, false); unit.Commands.InterruptAll(true); }
                DisposeExpandedSummoningUnits(fixture.Created, units);
                ResetExpandedSummoningHostile(fixture);
                foreach (UnityEngine.Object value in transient.Where(value => value != null).Reverse<UnityEngine.Object>())
                    UnityEngine.Object.DestroyImmediate(value);
                SettingsRoot.Instance.EnableTurnBasedMode.CurrentValue = oldMode;
                Game.Instance.TurnBasedCombatController.Activate(); Game.Instance.IsPaused = oldPause;
            }
        }

        private static JArray Trumpet31DispelSlotRows(Spellbook book, BlueprintAbility exclude = null)
        {
            return new JArray(Enumerable.Range(1, 7).SelectMany(level =>
                book.GetMemorizedSpellSlots(level).Select((slot, index) => new { level, index, slot })
                    .Where(value => exclude == null || !ReferenceEquals(value.slot.Spell?.Blueprint, exclude))
                    .Select(value => new JObject { ["level"] = value.level, ["index"] = value.index,
                        ["spell"] = value.slot.Spell?.Blueprint.AssetGuid, ["available"] = value.slot.Available })));
        }
    }
}
