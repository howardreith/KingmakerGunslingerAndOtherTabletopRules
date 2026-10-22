using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Assets.Console.GamepadInput;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UI._ConsoleUI.CharGen;
using Kingmaker.UI._ConsoleUI.CharGen.Phases.Base;
using Kingmaker.UI._ConsoleUI.CharGen.Phases.Spells;
using Kingmaker.UI._ConsoleUI.Context;
using Kingmaker.UI._ConsoleUI.Context.InGame;
using Kingmaker.UI._ConsoleUI.Models;
using Kingmaker.UI._ConsoleUI.SpellBook;
using Kingmaker.UI._ConsoleUI.Utils.MultiNavigationTool;
using Kingmaker.UI._ConsoleUI.Utils.NavigationTool;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        // Called after the first console observer, in the same exact request.
        // Every allocation is made by the real scene's native input callback.
        // Native progression seeding is fixture setup, not a character-build UI
        // claim; the pending learning allocation itself uses the live UI.
        private IEnumerable<int> ReviewSummonCleanup2ConsoleLearning(ExpandedSummoningCorrectionFixture fixture,Spellbook book)
        {
            if(!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters==null ||
                _request.Parameters.Count!=1 || (string)_request.Parameters["reviewPhase"]!="elementals-console" || !Cleanup2ConsoleNativeUiReady())
                throw new InvalidOperationException("Exact native-console cleanup2 learning/preparation request required.");
            var game=Game.Instance;var player=game.Player;var selection=UnitSelectionManager.Instance;
            var staticPart=(InGameUiStaticPartContext)typeof(InGameUiContext).GetField("m_StaticPartContext",Cleanup2Fields).GetValue(game.RootUiContext.InGameUiContext);
            var service=staticPart.CharcaterServiceContext;
            if(service.CharGenIsOpened || typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service)!=null ||
                game.UI.LevelUpController!=null)throw new InvalidOperationException("Foreign character-service state at learning intake.");
            var party=player.Party.ToArray();var selected=selection.SelectedUnits.ToArray();var current=selection.CurrentSelectUnitValue;
            var selectionState=new Cleanup2ConsoleSelectionSnapshot(selection);
            var protectedInputs=party.Select(value=>new Cleanup2TooltipInputs(value)).ToArray();
            var protectedPins=party.Select(value=>new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var layers=GamePad.Instance.Layers.ToArray();bool added=false;
            var mode=Cleanup2EnterNativeDefault(fixture,"elementals-console-learning");
            var screens=new NativeIconScreenEvidence(_request);
            try
            {
                if(!party.Contains(fixture.Caster))
                {player.PartyCharacters.Add(fixture.Caster);added=true;player.InvalidateCharacterLists();player.UpdateCharacterLists();}
                selection.SelectUnit(fixture.Caster.View,true,true,false);for(int frame=0;frame<8;frame++)yield return 0;
                foreach(int frame in Cleanup2ConsoleNativePrepareAndUndo(fixture,book,service,screens))yield return frame;
                foreach(int frame in Cleanup2ConsoleNativeLearnAndCancel(fixture,book,service,screens))yield return frame;
            }
            finally
            {
                try
                {
                    if(added){player.PartyCharacters.Remove(fixture.Caster);player.InvalidateCharacterLists();player.UpdateCharacterLists();}
                    bool selectionRestored=selectionState.Restore(fixture);
                    bool restored=player.Party.SequenceEqual(party) && selection.SelectedUnits.SequenceEqual(selected) &&
                        ReferenceEquals(selection.CurrentSelectUnitValue,current) && protectedInputs.All(value=>value.Unchanged()) &&
                        protectedPins.All(value=>value.IsRestored()) && GamePad.Instance.Layers.SequenceEqual(layers) && game.UI.LevelUpController==null && selectionRestored;
                    Cleanup2Check("elemental-console-learning-native-restoration",restored,new JObject {
                        ["partyExact"]=player.Party.SequenceEqual(party),["selectionExact"]=selection.SelectedUnits.SequenceEqual(selected),
                        ["protectedInputsExact"]=protectedInputs.All(value=>value.Unchanged()),["protectedPinsExact"]=protectedPins.All(value=>value.IsRestored()),
                        ["nativeSelectionRestoration"]=selectionState.Describe(),
                        ["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(layers),["nativeLevelUpClosed"]=game.UI.LevelUpController==null },
                        "The isolated native preparation/learning screens close without changing original party sources, pins, selection or input layers.");
                    if(!restored)throw new InvalidOperationException("Console learning/preparation restoration is unverified.");
                }
                finally{try{screens.Dispose();}finally{mode.Dispose();}}
            }
        }

        private IEnumerable<int> Cleanup2ConsoleNativePrepareAndUndo(ExpandedSummoningCorrectionFixture fixture,Spellbook book,
            CharcaterServiceContext service,NativeIconScreenEvidence screens)
        {
            var game=Game.Instance;SpellBookVM owned=null;
            var originalKnown=book.GetAllKnownSpells().ToArray();string resources=Cleanup2ElementalResources(fixture.Caster);
            var originalSlots=book.GetMemorizedSpellSlots(2).ToArray();var originalSpells=originalSlots.Select(value=>value.Spell).ToArray();
            var availability=originalSlots.Select(value=>value.Available).ToArray();
            var pins=new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
            SpellSlot written=null;AbilityData originalSource=null;
            try
            {
                if(!originalSlots.Any(value=>value.Spell==null && value.Type==SpellSlotType.Common))
                    throw new InvalidOperationException("Native common empty level2 slot required for a real preparation choice.");
                typeof(CharcaterServiceContext).GetMethod("OnSpellBookHandler",Cleanup2Fields).Invoke(service,null);
                for(int frame=0;frame<8;frame++)yield return 0;
                owned=typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service) as SpellBookVM;
                if(owned==null || !ReferenceEquals(owned.UnitEntity.Value,fixture.Caster))throw new InvalidOperationException("Real native console book did not bind the fixture caster.");
                owned.ChooseSpellBook(book.Blueprint);owned.ChooseLevel(2);for(int frame=0;frame<8;frame++)yield return 0;
                var view=Resources.FindObjectsOfTypeAll<SpellBookView>().Single(value=>value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded &&
                    value.gameObject.activeInHierarchy && ReferenceEquals(ElementalConsoleBookPresentation.Model(value),owned));
                var scroll=view.GetComponentsInChildren<SpellbookKnownSpellsScrollView>(true).Single(value=>ReferenceEquals(ElementalConsoleBookPresentation.Model(value),owned.SpellBookKnownSpells));
                var context=ElementalConsoleBookPresentation.PreparationFor(scroll);
                if(context==null || !context.Current)throw new InvalidOperationException("Live native preparation disclosure required.");
                var navigation=(ConsoleMultiNavigationCollection)typeof(SpellBookView).GetField("m_ConsoleMultiNavigationCollection",Cleanup2Fields).GetValue(view);
                var header=scroll.GetComponentsInChildren<SpellBookKnownSpellView>(false).Single(value=>ReferenceEquals(value.GetViewModel(),context.Header));
                navigation.SetCurrentEntityManual(header);InvokeTeleportGamepadInput(view,"WantConfirm");
                for(int frame=0;frame<5;frame++)yield return 0;
                var sourceVm=context.Sources.Single(value=>value.SpellData.Blueprint.AssetGuid==ElementalSourceChooserPolicy.Rank(2).Guid);
                originalSource=sourceVm.SpellData;
                var row=scroll.GetComponentsInChildren<SpellBookKnownSpellView>(false).Single(value=>ReferenceEquals(value.GetViewModel(),sourceVm));
                navigation.SetCurrentEntityManual(row);
                if(!ReferenceEquals(owned.CurrentSelectedSpell.Value,sourceVm))throw new InvalidOperationException("Actual native original source row did not receive focus.");
                InvokeTeleportGamepadInput(view,"WantConfirm");for(int frame=0;frame<8;frame++)yield return 0;
                var changed=originalSlots.Where((value,index)=>!ReferenceEquals(value.Spell,originalSpells[index])).ToArray();
                written=changed.Length==1?changed[0]:null;
                bool exact=written!=null && written.Type==SpellSlotType.Common && written.SpellLevel==2 &&
                    ReferenceEquals(written.Spell,originalSource) && !written.Available &&
                    originalSlots.Where(value=>!ReferenceEquals(value,written)).All(value=>{
                        int index=Array.IndexOf(originalSlots,value);return ReferenceEquals(value.Spell,originalSpells[index]) && value.Available==availability[index];}) &&
                    book.GetAllKnownSpells().SequenceEqual(originalKnown) && Cleanup2ElementalResources(fixture.Caster)==resources;
                Cleanup2Check("elemental-console-real-native-preparation-choice",exact,new JObject { ["changedSlotCount"]=changed.Length,
                    ["source"]=originalSource.Blueprint.AssetGuid,["sourceBook"]=originalSource.Spellbook.Blueprint.AssetGuid,
                    ["exactKnownAbilityDataReference"]=written!=null && ReferenceEquals(written.Spell,originalSource),
                    ["slotType"]=written?.Type.ToString(),["slotIndex"]=written?.Index,["slotLevel"]=written?.SpellLevel,
                    ["availableAfterNativeMemorize"]=written?.Available,["knownObjectsExact"]=book.GetAllKnownSpells().SequenceEqual(originalKnown),
                    ["resourcesExact"]=Cleanup2ElementalResources(fixture.Caster)==resources,["nativeInput"]="SpellBookView.WantConfirm on exact original native source row" },
                    "Choosing the original source through real console preparation assigns its exact known AbilityData to one native common slot, preserving every other slot and resource.");
                if(!exact)throw new InvalidOperationException("Native console original-source preparation failed.");
                var prepared=view.GetComponentsInChildren<SpellBookMemorizeSlotView>(false).Single(value=>ReferenceEquals(value.GetViewModel().SpellSlot,written));
                navigation=(ConsoleMultiNavigationCollection)typeof(SpellBookView).GetField("m_ConsoleMultiNavigationCollection",Cleanup2Fields).GetValue(view);
                navigation.SetCurrentEntityManual(prepared);
                foreach(int frame in screens.Capture("elementals-console-native-prepared-slot",new JObject { ["source"]=originalSource.Blueprint.AssetGuid,
                    ["slotIndex"]=written.Index,["slotType"]=written.Type.ToString(),["slotReferenceExact"]=ReferenceEquals(prepared.GetViewModel().SpellSlot,written),
                    ["scope"]="Real native preparation slot after original source confirmation; no direct book.Memorize call in the observed action." },
                    ()=>ReferenceEquals(ElementalConsoleBookPresentation.Model(view),owned) && ReferenceEquals(written.Spell,originalSource) && Cleanup2NoNarrativeOverlay()))yield return frame;
                InvokeTeleportGamepadInput(view,"WantConfirm");for(int frame=0;frame<8;frame++)yield return 0;
                bool undone=written.Spell==null && book.GetAllKnownSpells().SequenceEqual(originalKnown) &&
                    originalSlots.Where(value=>!ReferenceEquals(value,written)).All(value=>{
                        int index=Array.IndexOf(originalSlots,value);return ReferenceEquals(value.Spell,originalSpells[index]) && value.Available==availability[index];}) &&
                    Cleanup2ElementalResources(fixture.Caster)==resources;
                Cleanup2Check("elemental-console-native-unprepare-original-source",undone,new JObject { ["slotEmpty"]=written.Spell==null,
                    ["knownOriginalRetained"]=book.IsKnown(originalSource.Blueprint),["nativeInput"]="SpellBookView.WantConfirm on its exact native prepared-slot row",
                    ["emptySlotAvailableBefore"]=availability[Array.IndexOf(originalSlots,written)],["emptySlotAvailableAfter"]=written.Available,
                    ["scope"]="Native Forget may reset empty-slot metadata and native automatic hotbar bookkeeping; these fixture-owned effects are reported, not counted as a presentation debit." },
                    "Native console unprepare clears only the chosen preparation while retaining the original known source and all other source slots/resources.");
                if(!undone)throw new InvalidOperationException("Native console unprepare did not retain original source ownership.");
            }
            finally
            {
                if(owned!=null && ReferenceEquals(typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service),owned))owned.Back();
                // Only this disposable caster's original exact source was
                // written by this case; native rollback never touches others.
                if(written!=null && ReferenceEquals(written.Spell,originalSource))book.ForgetMemorized(written);
                pins.Restore();
            }
            for(int frame=0;frame<8;frame++)yield return 0;
        }

        private IEnumerable<int> Cleanup2ConsoleNativeLearnAndCancel(ExpandedSummoningCorrectionFixture fixture,Spellbook fixtureBook,
            CharcaterServiceContext service,NativeIconScreenEvidence screens)
        {
            var game=Game.Instance;var player=game.Player;
            BlueprintUnit blueprint=null;UnitEntityData unit=null;LevelUpController backend=null,ownedGlobal=null;CharGenVM owned=null;
            bool added=false;int successes=0;var layers=GamePad.Instance.Layers.ToArray();
            try
            {
                var wizard=fixture.Blueprints.OfType<BlueprintCharacterClass>().Single(value=>ReferenceEquals(value.Spellbook,fixtureBook.Blueprint));
                var human=BlueprintRoot.Instance.Progression.CharacterRaces.Single(value=>value.AssetGuid=="0a5d473ead98b0646b94495af250fdc4");
                blueprint=UnityEngine.Object.Instantiate(BlueprintRoot.Instance.DefaultPlayerCharacter);
                blueprint.name="KMG_Cleanup2_ConsoleLearningWizard";blueprint.IsCheater=true;blueprint.Race=human;
                unit=game.EntityCreator.SpawnUnit(blueprint,fixture.Caster.Position+new Vector3(-2,0,-2),Quaternion.identity,fixture.Scene);
                if(unit?.View==null)throw new InvalidOperationException("Native isolated learning actor creation failed.");
                fixture.Created.Add(unit);game.EntityCreator.Tick();SetExpandedSummoningBrainActive(unit,false);
                unit.Stats.Intelligence.BaseValue=18;unit.Stats.Wisdom.BaseValue=12;unit.Stats.Charisma.BaseValue=14;
                for(int level=0;level<2;level++)
                {
                    backend=LevelUpController.StartWithoutAssigningStaticInstance(unit.Descriptor,false,null,null,
                        level==0?LevelUpState.CharBuildMode.CharGen:LevelUpState.CharBuildMode.LevelUp);
                    if(level==0){backend.SelectRace(human);backend.SelectGender(fixture.Caster.Descriptor.Gender);backend.SelectAlignment(fixture.Caster.Descriptor.Alignment.Value);}
                    if(!backend.SelectClass(wizard))throw new InvalidOperationException("Native Wizard seed class selection refused.");
                    FillOracleNativeChoices(backend);
                    typeof(LevelUpController).GetMethod("ApplyLevelup",Cleanup2Fields).Invoke(backend,new object[]{unit.Descriptor});
                    backend.Cancel();backend=null;
                    if(unit.Descriptor.Progression.GetClassLevel(wizard)!=level+1)throw new InvalidOperationException("Native Wizard seed level did not advance exactly once.");
                }
                var book=unit.Descriptor.GetSpellbook(wizard.Spellbook);
                var legacy=Cleanup2ElementalAbility(fixture,ElementalSourceChooserPolicy.Rank(2).Guid);
                if(book==null || book.IsKnown(legacy) || unit.Descriptor.Progression.CharacterLevel!=2)
                    throw new InvalidOperationException("Native Wizard2 original learning state differs.");
                typeof(UnitProgressionData).GetProperty("Experience").SetValue(unit.Descriptor.Progression,
                    game.BlueprintRoot.Progression.XPTable.GetBonus(3),null);
                if(!LevelUpController.CanLevelUp(unit.Descriptor))throw new InvalidOperationException("Native Wizard3 XP prerequisite not satisfied.");
                player.PartyCharacters.Add(unit);added=true;player.InvalidateCharacterLists();player.UpdateCharacterLists();
                var originalKnown=book.GetAllKnownSpells().ToArray();var originalBudget=Cleanup2ElementalResources(unit);
                service.HandleLevelUpStart(unit.Descriptor,null,()=>successes++,LevelUpState.CharBuildMode.LevelUp);
                for(int frame=0;frame<10;frame++)yield return 0;
                backend=typeof(CharcaterServiceContext).GetField("LevelUpController",Cleanup2Fields).GetValue(service) as LevelUpController;
                ownedGlobal=backend;
                owned=typeof(CharcaterServiceContext).GetField("m_CharGenVM",Cleanup2Fields).GetValue(service) as CharGenVM;
                if(backend==null || owned==null || ReferenceEquals(backend.Preview,unit.Descriptor) || !ReferenceEquals(backend.Unit,unit.Descriptor) ||
                    !ReferenceEquals(game.UI.LevelUpController,backend))
                    throw new InvalidOperationException("Actual console context did not create an independent native learning preview.");
                if(!backend.SelectClass(wizard))throw new InvalidOperationException("Native pending Wizard3 class selection failed.");
                Cleanup2ConsoleLearningPrerequisites(backend);
                for(int frame=0;frame<10;frame++)yield return 0;
                var phase=owned.Phases.OfType<CharGenSpellsPhaseVM>().Single(value=>value.SelectorLevel==2 &&
                    ReferenceEquals(value.SpellList,wizard.Spellbook.SpellList));
                var root=Resources.FindObjectsOfTypeAll<CharGenView>().Single(value=>value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded &&
                    value.gameObject.activeInHierarchy && ReferenceEquals(ElementalConsoleBookPresentation.Model(value),owned));
                var roadmap=root.GetComponentsInChildren<MonoBehaviour>(true).OfType<ICharGenPhaseRoadmapView>().Single(value=>ReferenceEquals(value.GetViewModel(),phase));
                var navigation=(ConsoleNavigationBehavior)typeof(CharGenView).GetField("m_RoadmapPhasesNavigationBehavior",Cleanup2Fields).GetValue(root);
                if(!navigation.SelectEntityManually((IConsoleNavigationEntity)roadmap))throw new InvalidOperationException("Native learning roadmap refused its actual spell phase.");
                for(int frame=0;frame<8;frame++)yield return 0;
                var detail=root.GetComponentsInChildren<CharGenSpellsPhaseDetailedView>(true).Single(value=>value.gameObject.activeInHierarchy && ReferenceEquals(ElementalConsoleBookPresentation.Model(value),phase));
                var selector=detail.GetComponentsInChildren<CharGenSpellsSelectorView>(true).Single(value=>ReferenceEquals(ElementalConsoleBookPresentation.Model(value),phase.SpellsSelector.Value));
                var context=ElementalConsoleBookPresentation.LearningFor(selector);
                if(context==null || !context.Current)throw new InvalidOperationException("Actual native console learning source disclosure absent: "+ElementalConsoleBookPresentation.Outcome);
                var authoritative=phase.SpellsSelector.Value.VisibleCollection.ToArray();
                var nativeSelected=phase.UIChosenSpells.ToArray();var pending=context.SelectionData.LevelCount[2].SpellSelections.ToArray();
                var nativeHeader=selector.GetComponentsInChildren<CharGenSpellSelectorItemView>(false).Single(value=>ReferenceEquals(value.GetViewModelClass(),context.Header));
                var detailNavigation=(ConsoleMultiNavigationCollection)typeof(CharGenSpellsPhaseDetailedView).GetField("m_ConsoleMultiNavigationCollection",Cleanup2Fields).GetValue(detail);
                detailNavigation.SetCurrentEntityManual(nativeHeader);
                typeof(CharGenView).GetMethod("ConfirmPressed",Cleanup2Fields).Invoke(root,null);
                for(int frame=0;frame<5;frame++)yield return 0;
                bool expanded=context.Expanded && context.Current && context.Header.CanChoose.Value==false &&
                    phase.UIChosenSpells.SequenceEqual(nativeSelected) && context.SelectionData.LevelCount[2].SpellSelections.SequenceEqual(pending) &&
                    phase.SpellsSelector.Value.VisibleCollection.SequenceEqual(authoritative) && !authoritative.Contains(context.Header);
                Cleanup2Check("elemental-console-real-learning-disclosure-no-allocation",expanded,new JObject { ["expanded"]=context.Expanded,
                    ["authoritativeVMReferencesExact"]=phase.SpellsSelector.Value.VisibleCollection.SequenceEqual(authoritative),
                    ["headerNotAllocatable"]=!context.Header.CanChoose.Value,["allocationObjectsExact"]=phase.UIChosenSpells.SequenceEqual(nativeSelected),
                    ["nativePendingBudgetExact"]=context.SelectionData.LevelCount[2].SpellSelections.SequenceEqual(pending),
                    ["nativeHeaderValid"]=nativeHeader.IsValid(),["sourceCount"]=context.Sources.Length,["nativeInput"]="Actual CharGenView.ConfirmPressed/phase disclosure" },
                    "The real native console Wizard learning disclosure is focusable and expands original source VM references without allocating or consuming a learning choice.");
                if(!expanded)throw new InvalidOperationException("Native console learning disclosure changed allocation authority.");
                var source=authoritative.Single(value=>ReferenceEquals(value.Spell,legacy));
                var sourceRow=selector.GetComponentsInChildren<CharGenSpellSelectorItemView>(false).Single(value=>ReferenceEquals(value.GetViewModelClass(),source));
                detailNavigation.SetCurrentEntityManual(sourceRow);typeof(CharGenView).GetMethod("ConfirmPressed",Cleanup2Fields).Invoke(root,null);
                for(int frame=0;frame<8;frame++)yield return 0;
                bool learned=backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy) && !book.IsKnown(legacy) &&
                    context.SelectionData.LevelCount[2].SpellSelections.Count(value=>value!=null)==pending.Count(value=>value!=null)+1 &&
                    phase.UIChosenSpells.Any(value=>ReferenceEquals(value.Spell,legacy)) && source.IsChoosen.Value && successes==0;
                Cleanup2Check("elemental-console-real-learning-original-source",learned,new JObject { ["spell"]=legacy.AssetGuid,
                    ["originalNativeVMChosen"]=source.IsChoosen.Value,["previewKnowsOriginal"]=backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy),
                    ["originalActorDoesNotKnow"]=!book.IsKnown(legacy),["allocatedBefore"]=pending.Count(value=>value!=null),
                    ["allocatedAfter"]=context.SelectionData.LevelCount[2].SpellSelections.Count(value=>value!=null),["commits"]=successes },
                    "Choosing the original source through actual console learning allocates exactly one native pending choice in the independent preview and does not modify the original actor.");
                if(!learned)throw new InvalidOperationException("Native console original-source learning allocation failed.");
                foreach(int frame in screens.Capture("elementals-console-native-learning-preview",new JObject { ["spell"]=legacy.AssetGuid,
                    ["nativeText"]=new JArray(selector.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value=>value.text)),
                    ["previewKnows"]=backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy),["originalKnows"]=book.IsKnown(legacy),
                    ["scope"]="Real console CharGenView/roadmap/detail from native Wizard2-to3 level-up; original source allocation resides only in native preview." },
                    ()=>ReferenceEquals(ElementalConsoleBookPresentation.Model(root),owned) && backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy) && !book.IsKnown(legacy) && Cleanup2NoNarrativeOverlay()))yield return frame;
                // Undo through the same actual native row/confirm path.
                detailNavigation.SetCurrentEntityManual(sourceRow);typeof(CharGenView).GetMethod("ConfirmPressed",Cleanup2Fields).Invoke(root,null);
                for(int frame=0;frame<8;frame++)yield return 0;
                bool undo=!backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy) &&
                    context.SelectionData.LevelCount[2].SpellSelections.SequenceEqual(pending) && phase.UIChosenSpells.SequenceEqual(nativeSelected) &&
                    !book.IsKnown(legacy) && book.GetAllKnownSpells().SequenceEqual(originalKnown) && Cleanup2ElementalResources(unit)==originalBudget;
                Cleanup2Check("elemental-console-real-learning-native-undo",undo,new JObject { ["pendingChoiceExact"]=context.SelectionData.LevelCount[2].SpellSelections.SequenceEqual(pending),
                    ["originalKnownReferencesExact"]=book.GetAllKnownSpells().SequenceEqual(originalKnown),["originalResourcesExact"]=Cleanup2ElementalResources(unit)==originalBudget,
                    ["previewNoLongerKnows"]=!backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy),["commits"]=successes },
                    "Confirming the chosen original native row again restores the original pending allocation and preserves original known objects/resources.");
                if(!undo)throw new InvalidOperationException("Native console source learning undo failed.");
                backend.Cancel();backend=null;owned.Close();owned=null;
                if(ReferenceEquals(game.UI.LevelUpController,ownedGlobal))game.UI.LevelUpController=null;
                for(int frame=0;frame<10;frame++)yield return 0;
                bool cancelled=successes==0 && unit.Descriptor.Progression.GetClassLevel(wizard)==2 && !book.IsKnown(legacy) &&
                    book.GetAllKnownSpells().SequenceEqual(originalKnown) && GamePad.Instance.Layers.SequenceEqual(layers);
                Cleanup2Check("elemental-console-native-levelup-cancel",cancelled,new JObject { ["commits"]=successes,
                    ["classLevel"]=unit.Descriptor.Progression.GetClassLevel(wizard),["originalKnownReferencesExact"]=book.GetAllKnownSpells().SequenceEqual(originalKnown),
                    ["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(layers) },
                    "Cancelling the owned native learning preview commits nothing, retains Wizard2 and the exact original known objects, and removes its native input layers.");
                if(!cancelled)throw new InvalidOperationException("Native console level-up cancellation did not restore state.");
            }
            finally
            {
                if(backend!=null && unit!=null && ReferenceEquals(backend.Unit,unit.Descriptor))backend.Cancel();
                if(owned!=null && ReferenceEquals(typeof(CharcaterServiceContext).GetField("m_CharGenVM",Cleanup2Fields).GetValue(service),owned))owned.Close();
                if(ownedGlobal!=null && ReferenceEquals(game.UI.LevelUpController,ownedGlobal))game.UI.LevelUpController=null;
                if(unit!=null)
                {
                    if(added){player.PartyCharacters.Remove(unit);player.InvalidateCharacterLists();player.UpdateCharacterLists();}
                    CleanupExpandedSummoningUnit(unit);fixture.Created.Remove(unit);game.EntityDestroyer.Tick();game.EntityDestroyer.Tick();
                }
                if(blueprint!=null)UnityEngine.Object.Destroy(blueprint);
            }
        }

        private static void Cleanup2ConsoleLearningPrerequisites(LevelUpController controller)
        {
            // Bounded native prerequisite setup only; do not fill any pending
            // spell choice before exercising the real original-source UI row.
            for(int step=0;step<128;step++)
            {
                if(controller.State.AttributePoints>0 && controller.SpendAttributePoint(StatType.Intelligence))continue;
                var selection=controller.State.Selections.FirstOrDefault(value=>!value.Selected && value.CanSelectAnything(controller.State,controller.Preview));
                if(selection!=null)
                {
                    var item=selection.Selection.ExtractSelectionItems(controller.Unit,controller.Preview)
                        .Where(value=>selection.Selection.CanSelect(controller.Preview,controller.State,selection,value))
                        .OrderBy(value=>value.Feature.AssetGuid,StringComparer.Ordinal).FirstOrDefault();
                    if(item!=null && controller.SelectFeature(selection,item))continue;
                }
                bool spent=false;
                foreach(StatType skill in Enum.GetValues(typeof(StatType)))
                    if(skill.ToString().StartsWith("Skill",StringComparison.Ordinal) && controller.State.SkillPointsRemaining>0 && controller.SpendSkillPoint(skill))
                    {spent=true;break;}
                if(!spent)return;
            }
            throw new InvalidOperationException("Bounded native console-learning prerequisites did not converge.");
        }
    }
}
