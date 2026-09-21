using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Group;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Bootstrap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private IEnumerable<int> CircleGroupedMetamagicUi(UnitEntityData caster, UnitEntityData bearer,
            Spellbook book, IList<UnitEntityData> actors)
        {
            var game = Game.Instance; var ui = game.UI; var controller = ui.SpellBookController;
            var family = MagicCircleBlueprints.Family;
            var circle = BlueprintBootstrap.MagicCircles.Single(value => value.Alignment == "Evil");
            // Identity observed on the native mixer path, not a guessed name.
            var extendBlueprint = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(BlueprintBootstrap.Library,
                "f180e72e4a9cbaa4da8be9bc958132ef", "native Extend Spell feat");
            if (extendBlueprint.GetComponent<AddMetamagicFeat>()?.Metamagic != Metamagic.Extend)
                throw new InvalidOperationException("The exact native Extend feat contract changed.");
            CircleUiCapture("grouped-extend-native-feat", new { extendBlueprint.AssetGuid, extendBlueprint.name });
            if (caster.Descriptor.HasFact(extendBlueprint)) throw new InvalidOperationException("Extend fixture must own its exact new feat.");
            var extend = (Feature)caster.Descriptor.AddFact(extendBlueprint);
            var originalCaster = caster.Position; var originalBearer = bearer.Position; var time = game.Player.GameTime;
            try {
                GroupController.Instance.SelectUnit(caster); ui.SelectionManagerPC.SelectUnit(caster.View, true, true, false);
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 40; frame++) yield return 0;
                // Select the actual learned family row, then use the native
                // mixer and Write button. No hand-authored MetamagicData or
                // individually constructed child is part of this test.
                foreach (int frame in CircleBookUiEntry(controller, new TeleportUiSpellEntry(book, family, 3))) yield return frame;
                var known = book.GetAllKnownSpells().ToArray();
                controller.OnSwitchMetamagic();
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var mixer = controller.GetComponentsInChildren<SpellBookMetamagicMixer>(true).Single(value => value.IsShowed);
                var selector = mixer.GetComponentsInChildren<MetamagicSelectorSlotItem>(true).Single(value =>
                    value.gameObject.activeInHierarchy && ReferenceEquals(value.MetamagicFeature, extend));
                selector.OnPointerClick(new PointerEventData(EventSystem.current) { clickCount = 2 });
                for (int frame = 0; frame < 12; frame++) yield return 0;
                var temporary = controller.CurrentTemporarySpell;
                CircleUiAssert("grouped-extend-mixer", "native Extend mixer produces a level-four family spell",
                    "level=" + temporary?.SpellLevel, temporary?.Blueprint == family && temporary.HasMetamagic(Metamagic.Extend) &&
                    temporary.SpellLevel == 4 && ReferenceEquals(temporary.Spellbook, book));
                var write = (Button)typeof(SpellBookMetamagicMixer).GetField("m_WriteButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(mixer);
                if (!write.gameObject.activeInHierarchy || !write.interactable) throw new InvalidOperationException("Native metamagic Write button is unavailable.");
                write.onClick.Invoke();
                for (int frame = 0; frame < 20; frame++) yield return 0;
                var authored = book.GetCustomSpells(4).Single(value => value.Blueprint == family && value.HasMetamagic(Metamagic.Extend));
                CircleUiAssert("grouped-extend-written", "Write records one custom family; ordinary known-spell choices unchanged",
                    "custom=" + book.GetCustomSpells(4).Count(), book.GetAllKnownSpells().SequenceEqual(known) &&
                    book.GetCustomSpells(4).Count() == 1 && authored.SpellLevel == 4);
                if (mixer.IsShowed) controller.OnSwitchMetamagic();
                foreach (int frame in CircleBookUiEntry(controller, new TeleportUiSpellEntry(book, family, 4))) yield return frame;
                ui.ServiceWindow.HandleOpenSpellbook();
                for (int frame = 0; frame < 30; frame++) yield return 0;
                var prepared = book.GetMemorizedSpells(4).Single(value => value.Available && value.Spell.Blueprint == family && value.Spell.HasMetamagic(Metamagic.Extend));
                AbilityData chosen = null;
                foreach (int frame in CircleChooseGroupedUi(prepared.Spell, circle.Spell, value => chosen = value)) yield return frame;
                game.ClickEventsController.ClearPointerMode(); game.SelectedAbilityHandler.DropAbility();
                CircleUiAssert("grouped-extend-selected", "native child retains Extend and exact adjusted parent resource",
                    "level=" + chosen.SpellLevel, chosen.HasMetamagic(Metamagic.Extend) && chosen.SpellLevel == 4 &&
                    ReferenceEquals(chosen.ConvertedFrom, prepared.Spell) && ReferenceEquals(chosen.Spellbook, book));
                var allSlots = Enumerable.Range(0, 10).SelectMany(level => book.GetMemorizedSpellSlots(level)).ToArray();
                var available = allSlots.Select(slot => slot.Available).ToArray();
                CircleCast(caster, bearer, chosen, _circleUiDiagnostics);
                var carrier = CircleBuffs(bearer, circle.Carrier).Single(); var area = CircleArea(carrier);
                CircleRefresh(area, actors);
                var deadline = carrier.EndTime; var context = carrier.Context; var areaContext = area.Context;
                CircleUiAssert("grouped-extend-cost-context-duration", "exactly the selected level-four slot spent; original caster, Extend and doubled duration retained",
                    "seconds=" + carrier.TimeLeft.TotalSeconds + ";casterLevel=" + book.CasterLevel,
                    !prepared.Available && allSlots.Where((slot, index) => slot.Available != available[index]).SequenceEqual(new[] { prepared }) &&
                    ReferenceEquals(context.MaybeCaster, caster) && ReferenceEquals(areaContext.MaybeCaster, caster) &&
                    ReferenceEquals(areaContext.MaybeOwner, bearer) && context.Params.CasterLevel == book.CasterLevel &&
                    areaContext.Params.CasterLevel == book.CasterLevel && context.HasMetamagic(Metamagic.Extend) && areaContext.HasMetamagic(Metamagic.Extend) &&
                    Math.Abs(carrier.TimeLeft.TotalSeconds - 1200 * book.CasterLevel) < 2);
                // Re-enter a real recipient after game-time advances. The same
                // cast must retain its original absolute end time and contexts.
                game.Player.GameTime = time + TimeSpan.FromSeconds(90);
                caster.Translocate(bearer.Position + new Vector3(8, 0, 0), null); CircleRefresh(area, actors);
                bool exited = CircleBuffs(caster, circle.Recipient).Length == 0;
                caster.Translocate(bearer.Position, null); CircleRefresh(area, actors); CircleRefresh(area, actors);
                CircleUiAssert("grouped-extend-recipient-refresh", "recipient re-entry never restarts the Extended cast or spends another slot",
                    "seconds=" + carrier.TimeLeft.TotalSeconds, exited && CircleBuffs(caster, circle.Recipient).Length == 1 &&
                    carrier.EndTime == deadline && ReferenceEquals(carrier.Context, context) && ReferenceEquals(area.Context, areaContext) &&
                    Math.Abs(carrier.TimeLeft.TotalSeconds - (1200 * book.CasterLevel - 90)) < 2 &&
                    allSlots.Where((slot, index) => slot.Available != available[index]).SequenceEqual(new[] { prepared }));
                CircleUiCapture("grouped-extend-result", new { parent = family.AssetGuid, child = chosen.Blueprint.AssetGuid,
                    feat = extendBlueprint.AssetGuid, level = chosen.SpellLevel, metamagic = chosen.MetamagicData.MetamagicMask.ToString(),
                    caster = caster.UniqueId, bearer = bearer.UniqueId, area = area.UniqueId, casterLevel = book.CasterLevel,
                    durationSeconds = 1200 * book.CasterLevel, remainingSeconds = carrier.TimeLeft.TotalSeconds, deadlineTicks = deadline.Ticks,
                    slots = allSlots.Select((slot, index) => new { before = available[index], after = slot.Available,
                        spell = slot.Spell?.Blueprint.AssetGuid, level = slot.Spell?.SpellLevel }).ToArray() });
                carrier.Remove(); CircleRefresh(area, actors);
            }
            finally {
                foreach (var carrier in CircleBuffs(bearer, circle.Carrier)) carrier.Remove();
                caster.Descriptor.RemoveFact(extend); game.Player.GameTime = time;
                caster.Translocate(originalCaster, null); bearer.Translocate(originalBearer, null); CircleSynchronize(actors);
                if (controller.GetComponentsInChildren<SpellBookMetamagicMixer>(true).Any(value => value.IsShowed)) controller.OnSwitchMetamagic();
                if (controller.IsShow && ui.ServiceWindow.WindowTabs.IsShow) ui.ServiceWindow.HandleOpenSpellbook();
            }
        }
    }
}
