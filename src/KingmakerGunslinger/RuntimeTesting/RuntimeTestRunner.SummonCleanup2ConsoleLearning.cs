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
                _request.Parameters.Count!=1 || (string)_request.Parameters["reviewPhase"]!="elementals-console" || !Cleanup2ConsoleNativeUiReady() ||
                !Cleanup2LearningModuleState(Cleanup2OffState()))
                throw new InvalidOperationException("Exact native-console cleanup2 learning/preparation request required.");
            bool off=Cleanup2OffState();
            var game=Game.Instance;var player=game.Player;var selection=UnitSelectionManager.Instance;
            var rootUi=game.RootUiContext;var inGame=rootUi.InGameUiContext;
            var staticPart=(InGameUiStaticPartContext)typeof(InGameUiContext).GetField("m_StaticPartContext",Cleanup2Fields).GetValue(game.RootUiContext.InGameUiContext);
            var service=staticPart.CharcaterServiceContext;
            var charGenField=typeof(CharcaterServiceContext).GetField("m_CharGenVM",Cleanup2Fields);
            var spellBookField=typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields);
            var backendField=typeof(CharcaterServiceContext).GetField("LevelUpController",Cleanup2Fields);
            Func<bool> serviceExact=()=>ReferenceEquals(Game.Instance,game) && ReferenceEquals(game.RootUiContext,rootUi) &&
                ReferenceEquals(rootUi.InGameUiContext,inGame) && ReferenceEquals(typeof(InGameUiContext).GetField("m_StaticPartContext",Cleanup2Fields).GetValue(inGame),staticPart) &&
                ReferenceEquals(staticPart.CharcaterServiceContext,service) && service!=null && !service.IsDisposed;
            Func<JObject> closedConsoleOwner=()=>{
                var predicates=new JObject { ["exactLiveNativeServiceContext"]=serviceExact(),
                    ["nativeConsoleInputReady"]=Cleanup2ConsoleNativeUiReady(),
                    ["exactNativeServiceFieldContracts"]=charGenField!=null && spellBookField!=null && backendField!=null,
                    ["nativeCreatorVmAbsent"]=service!=null && charGenField!=null && charGenField.GetValue(service)==null,
                    ["nativeCreatorClosed"]=service!=null && !service.CharGenIsOpened,
                    ["nativeServiceBackendAbsent"]=service!=null && backendField!=null && backendField.GetValue(service)==null,
                    ["nativeBookVmAbsent"]=service!=null && spellBookField!=null && spellBookField.GetValue(service)==null };
                return new JObject { ["passed"]=predicates.Properties().All(value=>(bool)value.Value),["predicates"]=predicates,
                    ["originalGlobalControllerPresent"]=game.UI.LevelUpController!=null,
                    ["nativeContextIdentity"]=service==null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(service),
                    ["contract"]="Exact live console Root/InGame/static/service ownership and actual input binding; no creator/book/backend. An absent PC builder is admitted only through this positive console proof." };};
            var consoleIntake=closedConsoleOwner();
            Cleanup2Check("elemental-console-learning-service-intake",(bool)consoleIntake["passed"],consoleIntake,
                "Every actual console service/input/creator/book/backend intake predicate is recorded before accepting a retained completed global controller.");
            if(!(bool)consoleIntake["passed"])throw new InvalidOperationException("Foreign or active native console service at learning intake; inspect exact predicates.");
            var levelUpIntake=new Cleanup2SourcesLevelUpIntake(game,closedConsoleOwner);
            var party=player.Party.ToArray();var selected=selection.SelectedUnits.ToArray();var current=selection.CurrentSelectUnitValue;
            var selectionState=new Cleanup2ConsoleSelectionSnapshot(selection);
            var protectedInputs=party.Select(value=>new Cleanup2TooltipInputs(value)).ToArray();
            var protectedPins=party.Select(value=>new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var layers=GamePad.Instance.Layers.ToArray();bool added=false;
            var mode=Cleanup2EnterNativeDefault(fixture,"elementals-console-learning");
            var screens=new NativeIconScreenEvidence(_request);
            try
            {
                JObject levelUpReceipt;bool completedIntake=levelUpIntake.Current(out levelUpReceipt);
                Cleanup2Check("elemental-console-native-levelup-intake",completedIntake,levelUpReceipt,
                    "Only the exact completed global controller and idle native producer are retained under positively closed native console ownership.");
                if(!completedIntake)throw new InvalidOperationException("Native console global controller is not demonstrably completed/idle; inspect exact predicates.");
                if(!party.Contains(fixture.Caster))
                {player.PartyCharacters.Add(fixture.Caster);added=true;player.InvalidateCharacterLists();player.UpdateCharacterLists();}
                selection.SelectUnit(fixture.Caster.View,true,true,false);for(int frame=0;frame<8;frame++)yield return 0;
                foreach(int frame in Cleanup2ConsoleNativePrepareAndUndo(fixture,book,service,screens))yield return frame;
                foreach(int frame in Cleanup2ConsoleNativeLearnAndCancel(fixture,book,service,screens,levelUpIntake,serviceExact))yield return frame;
            }
            finally
            {
                try
                {
                    if(added){player.PartyCharacters.Remove(fixture.Caster);player.InvalidateCharacterLists();player.UpdateCharacterLists();}
                    bool selectionRestored=selectionState.Restore(fixture);
                    JObject levelUpReceipt;bool levelUpRestored=levelUpIntake.Current(out levelUpReceipt);
                    bool restored=player.Party.SequenceEqual(party) && selection.SelectedUnits.SequenceEqual(selected) &&
                        ReferenceEquals(selection.CurrentSelectUnitValue,current) && protectedInputs.All(value=>value.Unchanged()) &&
                        protectedPins.All(value=>value.IsRestored()) && GamePad.Instance.Layers.SequenceEqual(layers) && levelUpRestored && selectionRestored && Cleanup2LearningModuleState(off);
                    Cleanup2Check("elemental-console-learning-native-restoration",restored,new JObject {
                        ["partyExact"]=player.Party.SequenceEqual(party),["selectionExact"]=selection.SelectedUnits.SequenceEqual(selected),
                        ["protectedInputsExact"]=protectedInputs.All(value=>value.Unchanged()),["protectedPinsExact"]=protectedPins.All(value=>value.IsRestored()),
                        ["nativeSelectionRestoration"]=selectionState.Describe(),
                        ["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(layers),["nativeLevelUpClosed"]=levelUpRestored,
                        ["originalCompletedNativeControllerExact"]=levelUpReceipt,["freshModuleOff"]=off },
                        "The isolated native preparation/learning screens close without changing original party sources, pins, selection or input layers.");
                    if(!restored)throw new InvalidOperationException("Console learning/preparation restoration is unverified.");
                }
                finally{try{screens.Dispose();}finally{mode.Dispose();}}
            }
        }

        private IEnumerable<int> Cleanup2ConsoleNativePrepareAndUndo(ExpandedSummoningCorrectionFixture fixture,Spellbook book,
            CharcaterServiceContext service,NativeIconScreenEvidence screens)
        {
            bool off=Cleanup2OffState();
            var game=Game.Instance;SpellBookVM owned=null;
            var originalKnown=book.GetAllKnownSpells().ToArray();string resources=Cleanup2ElementalResources(fixture.Caster);
            var originalSlots=book.GetMemorizedSpellSlots(2).ToArray();var originalSpells=originalSlots.Select(value=>value.Spell).ToArray();
            var availability=originalSlots.Select(value=>value.Available).ToArray();
            var pins=new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
            SpellSlot written=null;AbilityData originalSource=null;
            ElementalConsoleBookPresentation.PreparationContext cleanupContext=null;
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
                if((!off && (context==null || !context.Current)) || (off && (context!=null || !Cleanup2LearningModuleState(true) ||
                    owned.SpellBookKnownSpells.KnownSpells.Any(value=>value is ElementalConsoleBookPresentation.PreparationHeader) ||
                    scroll.GetComponentsInChildren<SpellBookKnownSpellView>(true).Any(value=>value.GetViewModel() is ElementalConsoleBookPresentation.PreparationHeader))))
                    throw new InvalidOperationException("Native preparation disclosure/module-OFF original-row contract failed.");
                cleanupContext=context;
                if(!off)
                {
                    var header=scroll.GetComponentsInChildren<SpellBookKnownSpellView>(false).Single(value=>ReferenceEquals(value.GetViewModel(),context.Header));
                    Cleanup2ConsolePreparationFocus(view,owned,header,false,"disclosure");InvokeTeleportGamepadInput(view,"WantConfirm");
                    for(int frame=0;frame<5;frame++)yield return 0;
                }
                var sourceVm=owned.SpellBookKnownSpells.KnownSpells.Single(value=>value.SpellData!=null &&
                    value.SpellData.Blueprint.AssetGuid==ElementalSourceChooserPolicy.Rank(2).Guid && ReferenceEquals(value.SpellData.Spellbook,book));
                if(!off && !context.Sources.Any(value=>ReferenceEquals(value,sourceVm)))
                    throw new InvalidOperationException("Native original source is absent from the owned preparation disclosure.");
                originalSource=sourceVm.SpellData;
                var row=scroll.GetComponentsInChildren<SpellBookKnownSpellView>(false).Single(value=>ReferenceEquals(value.GetViewModel(),sourceVm));
                Cleanup2ConsolePreparationFocus(view,owned,row,false,"original-source");
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
                    ["resourcesExact"]=Cleanup2ElementalResources(fixture.Caster)==resources,["freshModuleOff"]=off,
                    ["ownedProjectionAbsent"]=off && context==null,["nativeInput"]="SpellBookView.WantConfirm on exact original native source row" },
                    "Choosing the original source through real console preparation assigns its exact known AbilityData to one native common slot, preserving every other slot and resource.");
                if(!exact)throw new InvalidOperationException("Native console original-source preparation failed.");
                var prepared=view.GetComponentsInChildren<SpellBookMemorizeSlotView>(false).Single(value=>ReferenceEquals(value.GetViewModel().SpellSlot,written));
                Cleanup2ConsolePreparationFocus(view,owned,prepared,true,"prepared-slot");
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
                if(owned!=null && ReferenceEquals(typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service),owned))
                    Cleanup2CloseOwnedConsoleBook(service,owned,fixture.Caster);
                // Only this disposable caster's original exact source was
                // written by this case; native rollback never touches others.
                if(written!=null && ReferenceEquals(written.Spell,originalSource))book.ForgetMemorized(written);
                pins.Restore();
            }
            for(int frame=0;frame<8;frame++)yield return 0;
            bool nativeUnbound=cleanupContext!=null && (cleanupContext.View==null ||
                (!ReferenceEquals(ElementalConsoleBookPresentation.Model(cleanupContext.View),cleanupContext.Group) &&
                !cleanupContext.View.GetComponentsInChildren<SpellBookKnownSpellView>(true)
                    .Any(value=>ReferenceEquals(value.GetViewModel(),cleanupContext.Header))));
            bool released=off ? owned!=null && owned.IsDisposed && cleanupContext==null && Cleanup2LearningModuleState(true) &&
                typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service)==null :
                owned!=null && owned.IsDisposed && cleanupContext!=null && cleanupContext.Group.IsDisposed &&
                cleanupContext.Group.CurrentSelectedSpell==null && nativeUnbound && cleanupContext.Released && cleanupContext.Disposed &&
                cleanupContext.Header.IsDisposed && cleanupContext.Projection==null &&
                typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service)==null;
            Cleanup2Check("elemental-console-native-disposed-preparation-release",released,new JObject {
                ["nativeBookDisposed"]=owned!=null && owned.IsDisposed,["nativeGroupDisposed"]=cleanupContext!=null && cleanupContext.Group.IsDisposed,
                ["nativeSelectionFieldCleared"]=cleanupContext!=null && cleanupContext.Group.CurrentSelectedSpell==null,
                ["nativeRowsUnbound"]=nativeUnbound,["ownedContextReleased"]=cleanupContext!=null && cleanupContext.Released,
                ["ownedHeaderDisposed"]=cleanupContext!=null && cleanupContext.Header.IsDisposed,
                ["ownedProjectionReleased"]=cleanupContext!=null && cleanupContext.Projection==null,
                ["presentationOutcome"]=ElementalConsoleBookPresentation.Outcome,
                ["freshModuleOff"]=off,["ownedProjectionExpected"]=!off,
                ["scope"]="Actual native book closure and scene-row unbind; fresh OFF has no owned header/context to release. No observer call to presentation Release or native VM Dispose." },
                "After native book disposal clears its selection field and native rows unbind, the owned preparation header and projection retire without dereferencing or modifying the disposed VM.");
            if(!released)throw new InvalidOperationException("Native disposed preparation presentation did not release after actual screen unbind.");
        }

        private void Cleanup2ConsolePreparationFocus(SpellBookView view,SpellBookVM owner,
            IConsoleMultiNavigationEntity row,bool memorized,string stage)
        {
            // Native UpdateNavigation06005045 puts only Left/RightNavUnit in
            // the top collection. SetNewCurrentEntity06004CD6 silently refuses
            // a row outside that collection's direct EntitiesList. Follow the
            // existing native branch/child collection/row; never assign native
            // selection properties or synthesize navigation membership.
            var root=typeof(SpellBookView).GetField("m_ConsoleMultiNavigationCollection",Cleanup2Fields).GetValue(view) as ConsoleMultiNavigationCollection;
            var branch=typeof(SpellBookView).GetField(memorized?"LeftNavUnit":"RightNavUnit",Cleanup2Fields).GetValue(view) as ConsoleMultiNavigationUnit;
            ConsoleMultiNavigationCollection leaf=null;IConsoleMultiNavigationEntity middle=null;
            if(memorized)
            {
                var panel=typeof(SpellBookView).GetField("m_MemorizingPanelView",Cleanup2Fields).GetValue(view) as SpellBookMemorizingPanelView;
                middle=panel?.NavigationUnit;leaf=panel?.NavigationUnit?.Collection;
            }
            else
            {
                var scroll=typeof(SpellBookView).GetField("m_KnownSpellsView",Cleanup2Fields).GetValue(view) as SpellbookKnownSpellsScrollView;
                middle=scroll?.Navigation;leaf=scroll?.Navigation;
            }
            var predicates=new JObject {
                ["exactLiveNativeBookBinding"]=view!=null && owner!=null && !owner.IsDisposed && view.gameObject.activeInHierarchy &&
                    ReferenceEquals(ElementalConsoleBookPresentation.Model(view),owner),
                ["nativeRootContainsExactBranch"]=root!=null && branch!=null && root.EntitiesList.Any(value=>ReferenceEquals(value,branch)),
                ["nativeBranchContainsExactCollection"]=branch?.Collection!=null && middle!=null && branch.Collection.EntitiesList.Any(value=>ReferenceEquals(value,middle)),
                ["nativeLeafContainsExactBoundRow"]=leaf!=null && row!=null && leaf.EntitiesList.Any(value=>ReferenceEquals(value,row)),
                ["nativeRowIsValid"]=row!=null && row.IsValid() };
            bool admitted=predicates.Properties().All(value=>(bool)value.Value);
            if(admitted)
            {
                root.SetCurrentEntityManual(branch);branch.Collection.SetCurrentEntityManual(middle);leaf.SetCurrentEntityManual(row);
            }
            var known=row as SpellBookKnownSpellView;var slot=row as SpellBookMemorizeSlotView;
            predicates["exactNativeRootBranchFocus"]=root!=null && ReferenceEquals(root.CurrentEntity,branch);
            predicates["exactNativeBranchCollectionFocus"]=branch?.Collection!=null && ReferenceEquals(branch.Collection.CurrentEntity,middle);
            predicates["exactNativeLeafRowFocus"]=leaf!=null && ReferenceEquals(leaf.CurrentEntity,row);
            predicates["nativeViewCallbackSelectedExactRow"]=ReferenceEquals(typeof(SpellBookView).GetField("m_CurrentSelected",Cleanup2Fields).GetValue(view),row);
            predicates["nativeSelectedAbilityVmExact"]=owner!=null && (memorized ? slot!=null && owner.CurrentSelectedSpell.Value==null :
                known!=null && ReferenceEquals(owner.CurrentSelectedSpell.Value,known.GetViewModel()));
            bool exact=admitted && predicates.Properties().All(value=>(bool)value.Value);
            Cleanup2Check("elemental-console-native-preparation-focus-"+stage,exact,new JObject {
                ["predicates"]=predicates,["nativeBranch"]=memorized?"LeftNavUnit/memorizing panel":"RightNavUnit/known spells",
                ["rowType"]=row?.GetType().FullName,["source"]=known?.GetViewModel()?.SpellData?.Blueprint.AssetGuid,
                ["preparedSource"]=slot?.GetViewModel()?.SpellSlot?.Spell?.Blueprint.AssetGuid,
                ["rootDirectEntityCount"]=root?.EntitiesList.Count,["leafDirectEntityCount"]=leaf?.EntitiesList.Count,
                ["nativeInputContract"]="Direct member navigation only, SetCurrentEntityManual06004CD3 -> EntitySelected06005030 -> SelectKnownSpell06005132; no VM/selection writes." },
                "The original source or preparation receives real focus through its exact native navigation hierarchy and callback before Confirm.");
            if(!exact)throw new InvalidOperationException("Actual native preparation row focus failed: "+stage+"; inspect exact navigation membership/callback predicates.");
        }

        private IEnumerable<int> Cleanup2ConsoleNativeLearnAndCancel(ExpandedSummoningCorrectionFixture fixture,Spellbook fixtureBook,
            CharcaterServiceContext service,NativeIconScreenEvidence screens,Cleanup2SourcesLevelUpIntake levelUpIntake,Func<bool> serviceExact)
        {
            bool off=Cleanup2OffState();
            var game=Game.Instance;var player=game.Player;
            var originalGlobal=game.UI.LevelUpController;
            var backendField=typeof(CharcaterServiceContext).GetField("LevelUpController",Cleanup2Fields);
            var charGenField=typeof(CharcaterServiceContext).GetField("m_CharGenVM",Cleanup2Fields);
            var vmBackend=typeof(CharGenVM).GetField("m_LevelUpController",Cleanup2Fields);
            BlueprintUnit blueprint=null;UnitEntityData unit=null;LevelUpController backend=null,ownedGlobal=null;CharGenVM owned=null;
            bool added=false;int successes=0;var layers=GamePad.Instance.Layers.ToArray();
            try
            {
                JObject intakeReceipt;
                if(!levelUpIntake.Current(out intakeReceipt))throw new InvalidOperationException("Original console controller/service changed before creating its independent learning actor: "+intakeReceipt);
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
                try{service.HandleLevelUpStart(unit.Descriptor,null,()=>successes++,LevelUpState.CharBuildMode.LevelUp);}
                finally
                {
                    // HandleLevelUpStart publishes the backend before native
                    // ShowChargen/BindChargen. Retain that exact owned result
                    // even if later binding throws; never adopt a replacement.
                    var candidate=backendField.GetValue(service) as LevelUpController;
                    var candidateVm=charGenField.GetValue(service) as CharGenVM;
                    if(serviceExact() && candidate!=null && !ReferenceEquals(candidate,originalGlobal) &&
                        ReferenceEquals(candidate.Unit,unit.Descriptor) && ReferenceEquals(game.UI.LevelUpController,candidate) &&
                        (candidateVm==null || ReferenceEquals(vmBackend.GetValue(candidateVm),candidate)))
                    {backend=candidate;ownedGlobal=candidate;owned=candidateVm;}
                }
                for(int frame=0;frame<10;frame++)yield return 0;
                if(backend==null || owned==null || ReferenceEquals(backend.Preview,unit.Descriptor) || !ReferenceEquals(backend.Unit,unit.Descriptor) ||
                    !ReferenceEquals(game.UI.LevelUpController,backend) || !serviceExact() ||
                    !ReferenceEquals(backendField.GetValue(service),backend) || !ReferenceEquals(charGenField.GetValue(service),owned))
                    throw new InvalidOperationException("Actual console context did not create an independent native learning preview.");
                Func<bool> ownsLearning=()=>serviceExact() && owned!=null && !owned.IsDisposed && backend!=null &&
                    ReferenceEquals(backendField.GetValue(service),backend) && ReferenceEquals(charGenField.GetValue(service),owned) &&
                    ReferenceEquals(vmBackend.GetValue(owned),backend) && ReferenceEquals(game.UI.LevelUpController,ownedGlobal) &&
                    ReferenceEquals(backend,ownedGlobal) && ReferenceEquals(backend.Unit,unit.Descriptor) && backend.Preview?.Unit!=null && !backend.Preview.Unit.Destroyed && Cleanup2LearningModuleState(off);
                if(!ownsLearning())throw new InvalidOperationException("Exact native console learning owner changed before class selection.");
                if(!backend.SelectClass(wizard))throw new InvalidOperationException("Native pending Wizard3 class selection failed.");
                Cleanup2ConsoleLearningPrerequisites(backend);
                for(int frame=0;frame<10;frame++)yield return 0;
                var phase=owned.Phases.OfType<CharGenSpellsPhaseVM>().Single(value=>value.SelectorLevel==2 &&
                    ReferenceEquals(value.SpellList,wizard.Spellbook.SpellList));
                var root=Resources.FindObjectsOfTypeAll<CharGenView>().Single(value=>value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded &&
                    value.gameObject.activeInHierarchy && ReferenceEquals(ElementalConsoleBookPresentation.Model(value),owned));
                var roadmap=root.GetComponentsInChildren<MonoBehaviour>(true).OfType<ICharGenPhaseRoadmapView>().Single(value=>ReferenceEquals(value.GetViewModel(),phase));
                var navigation=(ConsoleNavigationBehavior)typeof(CharGenView).GetField("m_RoadmapPhasesNavigationBehavior",Cleanup2Fields).GetValue(root);
                if(!ownsLearning() || !navigation.SelectEntityManually((IConsoleNavigationEntity)roadmap))throw new InvalidOperationException("Native learning roadmap refused its exact owned spell phase.");
                for(int frame=0;frame<8;frame++)yield return 0;
                var detail=root.GetComponentsInChildren<CharGenSpellsPhaseDetailedView>(true).Single(value=>value.gameObject.activeInHierarchy && ReferenceEquals(ElementalConsoleBookPresentation.Model(value),phase));
                var selector=detail.GetComponentsInChildren<CharGenSpellsSelectorView>(true).Single(value=>ReferenceEquals(ElementalConsoleBookPresentation.Model(value),phase.SpellsSelector.Value));
                var context=ElementalConsoleBookPresentation.LearningFor(selector);
                if((!off && (context==null || !context.Current)) || (off && (context!=null ||
                    selector.GetComponentsInChildren<CharGenSpellSelectorItemView>(true).Any(value=>value.GetViewModelClass() is ElementalConsoleBookPresentation.LearningHeader))))
                    throw new InvalidOperationException("Actual native console learning source/module-OFF row contract failed: "+ElementalConsoleBookPresentation.Outcome);
                var selection=off?(SpellSelectionData)typeof(CharGenSpellsPhaseVM).GetField("m_SelectionData",Cleanup2Fields).GetValue(phase):context.SelectionData;
                if(selection==null || !ReferenceEquals(selection.Spellbook,wizard.Spellbook) || !ReferenceEquals(selection.SpellList,wizard.Spellbook.SpellList))
                    throw new InvalidOperationException("Exact native console learning allocation unavailable.");
                var authoritative=phase.SpellsSelector.Value.VisibleCollection.ToArray();
                var nativeSelected=phase.UIChosenSpells.ToArray();var pending=selection.LevelCount[2].SpellSelections.ToArray();
                var detailNavigation=(ConsoleMultiNavigationCollection)typeof(CharGenSpellsPhaseDetailedView).GetField("m_ConsoleMultiNavigationCollection",Cleanup2Fields).GetValue(detail);
                if(!ownsLearning())throw new InvalidOperationException("Native console learning owner changed before disclosure.");
                CharGenSpellSelectorItemView nativeHeader=null;
                if(!off)
                {
                    nativeHeader=selector.GetComponentsInChildren<CharGenSpellSelectorItemView>(false).Single(value=>ReferenceEquals(value.GetViewModelClass(),context.Header));
                    detailNavigation.SetCurrentEntityManual(nativeHeader);typeof(CharGenView).GetMethod("ConfirmPressed",Cleanup2Fields).Invoke(root,null);
                    for(int frame=0;frame<5;frame++)yield return 0;
                }
                bool expanded=(off ? context==null && !authoritative.Any(value=>value is ElementalConsoleBookPresentation.LearningHeader) :
                    context.Expanded && context.Current && context.Header.CanChoose.Value==false && !authoritative.Contains(context.Header)) &&
                    phase.UIChosenSpells.SequenceEqual(nativeSelected) && selection.LevelCount[2].SpellSelections.SequenceEqual(pending) &&
                    phase.SpellsSelector.Value.VisibleCollection.SequenceEqual(authoritative);
                Cleanup2Check("elemental-console-real-learning-disclosure-no-allocation",expanded,new JObject { ["expanded"]=!off && context.Expanded,
                    ["authoritativeVMReferencesExact"]=phase.SpellsSelector.Value.VisibleCollection.SequenceEqual(authoritative),
                    ["headerNotAllocatable"]=!off && !context.Header.CanChoose.Value,["allocationObjectsExact"]=phase.UIChosenSpells.SequenceEqual(nativeSelected),
                    ["nativePendingBudgetExact"]=selection.LevelCount[2].SpellSelections.SequenceEqual(pending),
                    ["nativeHeaderValid"]=nativeHeader!=null && nativeHeader.IsValid(),["freshModuleOff"]=off,["ownedProjectionAbsent"]=off && context==null,
                    ["nativeInput"]=off?"Original native console learning row, no disclosure":"Actual CharGenView.ConfirmPressed/phase disclosure" },
                    "The real native console Wizard learning disclosure is focusable and expands original source VM references without allocating or consuming a learning choice.");
                if(!expanded)throw new InvalidOperationException("Native console learning disclosure changed allocation authority.");
                var source=authoritative.Single(value=>ReferenceEquals(value.Spell,legacy));
                var sourceRow=selector.GetComponentsInChildren<CharGenSpellSelectorItemView>(false).Single(value=>ReferenceEquals(value.GetViewModelClass(),source));
                if(!ownsLearning())throw new InvalidOperationException("Native console learning owner changed before source allocation.");
                detailNavigation.SetCurrentEntityManual(sourceRow);typeof(CharGenView).GetMethod("ConfirmPressed",Cleanup2Fields).Invoke(root,null);
                for(int frame=0;frame<8;frame++)yield return 0;
                bool learned=backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy) && !book.IsKnown(legacy) &&
                    selection.LevelCount[2].SpellSelections.Count(value=>value!=null)==pending.Count(value=>value!=null)+1 &&
                    phase.UIChosenSpells.Any(value=>ReferenceEquals(value.Spell,legacy)) && source.IsChoosen.Value && successes==0;
                Cleanup2Check("elemental-console-real-learning-original-source",learned,new JObject { ["spell"]=legacy.AssetGuid,
                    ["originalNativeVMChosen"]=source.IsChoosen.Value,["previewKnowsOriginal"]=backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy),
                    ["originalActorDoesNotKnow"]=!book.IsKnown(legacy),["allocatedBefore"]=pending.Count(value=>value!=null),
                    ["allocatedAfter"]=selection.LevelCount[2].SpellSelections.Count(value=>value!=null),["commits"]=successes,["freshModuleOff"]=off },
                    "Choosing the original source through actual console learning allocates exactly one native pending choice in the independent preview and does not modify the original actor.");
                if(!learned)throw new InvalidOperationException("Native console original-source learning allocation failed.");
                foreach(int frame in screens.Capture("elementals-console-native-learning-preview",new JObject { ["spell"]=legacy.AssetGuid,
                    ["nativeText"]=new JArray(selector.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value=>value.text)),
                    ["previewKnows"]=backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy),["originalKnows"]=book.IsKnown(legacy),
                    ["scope"]="Real console CharGenView/roadmap/detail from native Wizard2-to3 level-up; original source allocation resides only in native preview." },
                    ()=>ownsLearning() && ReferenceEquals(ElementalConsoleBookPresentation.Model(root),owned) && backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy) && !book.IsKnown(legacy) && Cleanup2NoNarrativeOverlay()))yield return frame;
                // Undo through the same actual native row/confirm path.
                if(!ownsLearning())throw new InvalidOperationException("Native console learning owner changed before source undo.");
                detailNavigation.SetCurrentEntityManual(sourceRow);typeof(CharGenView).GetMethod("ConfirmPressed",Cleanup2Fields).Invoke(root,null);
                for(int frame=0;frame<8;frame++)yield return 0;
                bool undo=!backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy) &&
                    selection.LevelCount[2].SpellSelections.SequenceEqual(pending) && phase.UIChosenSpells.SequenceEqual(nativeSelected) &&
                    !book.IsKnown(legacy) && book.GetAllKnownSpells().SequenceEqual(originalKnown) && Cleanup2ElementalResources(unit)==originalBudget;
                Cleanup2Check("elemental-console-real-learning-native-undo",undo,new JObject { ["pendingChoiceExact"]=selection.LevelCount[2].SpellSelections.SequenceEqual(pending),
                    ["originalKnownReferencesExact"]=book.GetAllKnownSpells().SequenceEqual(originalKnown),["originalResourcesExact"]=Cleanup2ElementalResources(unit)==originalBudget,
                    ["previewNoLongerKnows"]=!backend.Preview.GetSpellbook(wizard.Spellbook).IsKnown(legacy),["commits"]=successes },
                    "Confirming the chosen original native row again restores the original pending allocation and preserves original known objects/resources.");
                if(!undo)throw new InvalidOperationException("Native console source learning undo failed.");
                if(!ownsLearning())throw new InvalidOperationException("Native console learning owner changed before cancellation.");
                backend.Cancel();
                Cleanup2ConsoleCloseOwnedLearning(service,backend,owned,serviceExact);
                if(!ReferenceEquals(game.UI.LevelUpController,ownedGlobal))throw new InvalidOperationException("Console global backend changed during native close; original field is not overwritten.");
                game.UI.LevelUpController=originalGlobal;backend=null;owned=null;
                for(int frame=0;frame<10;frame++)yield return 0;
                JObject restoredLevelUpReceipt;bool restoredLevelUp=levelUpIntake.Current(out restoredLevelUpReceipt);
                bool cancelled=successes==0 && unit.Descriptor.Progression.GetClassLevel(wizard)==2 && !book.IsKnown(legacy) &&
                    book.GetAllKnownSpells().SequenceEqual(originalKnown) && GamePad.Instance.Layers.SequenceEqual(layers) &&
                    ReferenceEquals(game.UI.LevelUpController,originalGlobal) && restoredLevelUp;
                Cleanup2Check("elemental-console-native-levelup-cancel",cancelled,new JObject { ["commits"]=successes,
                    ["classLevel"]=unit.Descriptor.Progression.GetClassLevel(wizard),["originalKnownReferencesExact"]=book.GetAllKnownSpells().SequenceEqual(originalKnown),
                    ["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(layers),["originalCompletedNativeControllerExact"]=restoredLevelUpReceipt },
                    "Cancelling the owned native learning preview commits nothing, retains Wizard2 and the exact original known objects, and removes its native input layers.");
                if(!cancelled)throw new InvalidOperationException("Native console level-up cancellation did not restore state.");
            }
            finally
            {
                bool foreign=!serviceExact() ||
                    (!ReferenceEquals(game.UI.LevelUpController,originalGlobal) && !(ownedGlobal!=null && ReferenceEquals(game.UI.LevelUpController,ownedGlobal))) ||
                    (backendField.GetValue(service)!=null && !ReferenceEquals(backendField.GetValue(service),ownedGlobal)) ||
                    (charGenField.GetValue(service)!=null && !ReferenceEquals(charGenField.GetValue(service),owned));
                if(backend!=null && (unit==null || !ReferenceEquals(backend.Unit,unit.Descriptor) || ReferenceEquals(backend,originalGlobal) ||
                    backend.AutoCommit || ReferenceEquals(backend.Preview,unit.Descriptor)))foreign=true;
                if(foreign)throw new InvalidOperationException("Foreign console service/controller replacement; it is not cancelled, cleared or disposed.");
                if(backend?.Preview?.Unit!=null && !backend.Preview.Unit.Destroyed)backend.Cancel();
                if(ownedGlobal!=null && ReferenceEquals(backendField.GetValue(service),ownedGlobal))
                    Cleanup2ConsoleCloseOwnedLearning(service,ownedGlobal,owned,serviceExact);
                if(ownedGlobal!=null && ReferenceEquals(game.UI.LevelUpController,ownedGlobal))game.UI.LevelUpController=originalGlobal;
                if(!ReferenceEquals(game.UI.LevelUpController,originalGlobal) || backendField.GetValue(service)!=null || charGenField.GetValue(service)!=null)
                    throw new InvalidOperationException("Original console controller/service state not restored; referenced actor is not destroyed.");
                if(unit!=null)
                {
                    if(added){player.PartyCharacters.Remove(unit);player.InvalidateCharacterLists();player.UpdateCharacterLists();}
                    CleanupExpandedSummoningUnit(unit);fixture.Created.Remove(unit);game.EntityDestroyer.Tick();game.EntityDestroyer.Tick();
                }
                if(blueprint!=null)UnityEngine.Object.Destroy(blueprint);
            }
        }

        private static void Cleanup2ConsoleCloseOwnedLearning(CharcaterServiceContext service,LevelUpController backend,CharGenVM owned,Func<bool> serviceExact)
        {
            var backendField=typeof(CharcaterServiceContext).GetField("LevelUpController",Cleanup2Fields);
            var vmField=typeof(CharcaterServiceContext).GetField("m_CharGenVM",Cleanup2Fields);
            var close=typeof(CharcaterServiceContext).GetMethod("CloseCharGen",Cleanup2Fields,null,Type.EmptyTypes,null);
            if(serviceExact==null || !serviceExact() || backend==null || !ReferenceEquals(backendField.GetValue(service),backend) ||
                !ReferenceEquals(Game.Instance.UI.LevelUpController,backend) || !ReferenceEquals(vmField.GetValue(service),owned) ||
                (owned!=null && (owned.IsDisposed || !ReferenceEquals(typeof(CharGenVM).GetField("m_LevelUpController",Cleanup2Fields).GetValue(owned),backend))) ||
                close==null || close.MetadataToken!=0x0600624e || close.Module.ModuleVersionId.ToString()!=ElementalSourceChooserRuntime.GameMvid)
                throw new InvalidOperationException("Exact owned live console creator/backend required for native close; disposed/foreign state is not repaired.");
            // Native CloseCharGen clears its service backend, disposes the live
            // VM and clears that field, but leaves UIAccess's retained global.
            close.Invoke(service,null);
            if(backendField.GetValue(service)!=null || vmField.GetValue(service)!=null || (owned!=null && !owned.IsDisposed))
                throw new InvalidOperationException("Native owned console creator did not dispose/clear exactly.");
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
