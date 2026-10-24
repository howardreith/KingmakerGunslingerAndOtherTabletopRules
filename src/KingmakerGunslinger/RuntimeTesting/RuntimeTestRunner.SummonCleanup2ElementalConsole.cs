using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Assets.Console.GamepadInput;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI._ConsoleUI.ActionBar;
using Kingmaker.UI._ConsoleUI.Context;
using Kingmaker.UI._ConsoleUI.Context.InGame;
using Kingmaker.UI._ConsoleUI.Models;
using Kingmaker.UI._ConsoleUI.InputLayers.InGameLayer;
using Kingmaker.UI._ConsoleUI.Config.InGame;
using Kingmaker.UI._ConsoleUI.SpellBook;
using Kingmaker.UI._ConsoleUI.Utils.MultiNavigationTool;
using Kingmaker.UI._ConsoleUI.Utils.NavigationTool;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private sealed class Cleanup2ConsoleSelectionSnapshot
        {
            private readonly UnitSelectionManager _manager;
            private readonly ReactiveProperty<UnitEntityData> _currentProperty;
            private readonly UnitEntityData _current;
            private readonly UnitEntityData[] _selected,_grouped;
            private readonly List<UnitEntityData> _groupedList;
            private static readonly FieldInfo Grouped=typeof(UnitSelectionManager).GetField("m_GroupedUnits",Cleanup2Fields);
            internal bool Restored,GroupedReferenceExact,GroupedMembersExact;
            internal string Outcome="not-restored";
            internal Cleanup2ConsoleSelectionSnapshot(UnitSelectionManager manager)
            {
                if(Game.Instance.Player.IsInCombat)throw new InvalidOperationException("Idle native console selection required.");
                _manager=manager;_currentProperty=manager.CurrentSelectUnit;_current=manager.CurrentSelectUnitValue;
                _selected=manager.SelectedUnits.ToArray();_groupedList=(List<UnitEntityData>)Grouped.GetValue(manager);_grouped=_groupedList.ToArray();
            }
            internal bool Restore(ExpandedSummoningCorrectionFixture fixture)
            {
                Func<UnitEntityData,bool> original=value=>ReferenceEquals(value,_current) || _selected.Any(prior=>ReferenceEquals(prior,value)) ||
                    _grouped.Any(prior=>ReferenceEquals(prior,value));
                Func<UnitEntityData,bool> owned=value=>ReferenceEquals(value,fixture.Caster) || fixture.Created.Any(prior=>ReferenceEquals(prior,value));
                var current=_manager.CurrentSelectUnitValue;
                var grouped=(List<UnitEntityData>)Grouped.GetValue(_manager);
                if(!ReferenceEquals(UnitSelectionManager.Instance,_manager) || !ReferenceEquals(_manager.CurrentSelectUnit,_currentProperty) ||
                    Game.Instance.Player.IsInCombat || (current!=null && !original(current) && !owned(current)) ||
                    _manager.SelectedUnits.Any(value=>!original(value) && !owned(value)) || grouped.Any(value=>!original(value) && !owned(value)))
                {Outcome="foreign-console-selection-preserved-no-write";return false;}
                typeof(UnitSelectionManager).GetMethod("Clear",Cleanup2Fields).Invoke(_manager,null);
                // Native UpdateGroupedUnit rebuilds from actual live
                // IsDirectlyControllable && IsLink. Unlink only new, positively
                // fixture-owned units that were never in the captured group.
                foreach(var unit in fixture.Created.Concat(new[]{fixture.Caster}).Distinct().Where(value=>value!=null && !value.Destroyed &&
                    !_grouped.Any(prior=>ReferenceEquals(prior,value)) && value.IsLink))_manager.UnlinkUnit(unit);
                typeof(UnitSelectionManager).GetMethod("UpdateGroupedUnit",Cleanup2Fields).Invoke(_manager,null);
                foreach(var unit in _selected)if(unit?.View!=null)_manager.SelectUnit(unit.View,true,false,false);
                if(_current!=null)_manager.ForceSelectUnit(_current,false,false);
                else if(_manager.CurrentSelectUnitValue==null || owned(_manager.CurrentSelectUnitValue))
                    _currentProperty.Value=null; // Exact captured native property; Clear never clears it.
                grouped=(List<UnitEntityData>)Grouped.GetValue(_manager);
                GroupedReferenceExact=ReferenceEquals(grouped,_groupedList);GroupedMembersExact=grouped.SequenceEqual(_grouped);
                Restored=GroupedReferenceExact && GroupedMembersExact && _manager.SelectedUnits.SequenceEqual(_selected) &&
                    ReferenceEquals(_manager.CurrentSelectUnitValue,_current);
                Outcome=Restored?"native-console-selection-and-group-exact":"native-console-selection-restoration-unverified";
                return Restored;
            }
            internal JObject Describe(){return new JObject { ["outcome"]=Outcome,["restored"]=Restored,
                ["originalCurrentWasNull"]=_current==null,["currentExact"]=ReferenceEquals(_manager.CurrentSelectUnitValue,_current),
                ["groupedListReferenceExact"]=GroupedReferenceExact,["groupedMembersExact"]=GroupedMembersExact,
                ["selectedExact"]=_manager.SelectedUnits.SequenceEqual(_selected) };}
        }

        private JObject _cleanup2ConsoleReadiness;
        private bool Cleanup2ConsoleNativeUiReady()
        {
            if(!IsCleanup2ConsoleReview || _request.Parameters==null ||
                !Game.Instance.IsControllerGamepad || GamePad.Instance==null)return false;
            var context=Game.Instance.RootUiContext?.InGameUiContext;if(context==null || UnitSelectionManager.Instance==null)return false;
            var part=typeof(InGameUiContext).GetField("m_StaticPartContext",Cleanup2Fields).GetValue(context) as InGameUiStaticPartContext;
            if(part==null)return false;
            var model=typeof(InGameUiStaticPartContext).GetField("m_ActionBarVm",Cleanup2Fields).GetValue(part) as ActionBarVM;
            var config=typeof(InGameUiStaticPartContext).GetField("m_Config",Cleanup2Fields).GetValue(part) as InGameUiStaticPartConfig;
            var view=config?.ActionBarView;
            // The installed InGameInputLayer.inGameUiContext field is never
            // assigned. Bind06005D20 stores/pushes the view's exact layer;
            // VM06005D3B retains the actual context. Observe that ownership.
            var inputView=context.GetInGameInputLayerView();
            var inputVm=typeof(InGameUiContext).GetField("m_InGameInputLayerVm",Cleanup2Fields).GetValue(context) as InGameInputLayerVM;
            var inputLayer=inputView==null?null:typeof(InGameInputLayerView).GetField("m_InGameInputLayer",Cleanup2Fields).GetValue(inputView) as InGameInputLayer;
            var layers=GamePad.Instance.Layers.OfType<InGameInputLayer>().ToArray();
            var predicates=new JObject {
                ["actionBarModelLive"]=model!=null && !model.IsDisposed,
                ["actionBarViewLoaded"]=view!=null && view.gameObject.scene.IsValid() && view.gameObject.scene.isLoaded,
                ["actionBarViewActive"]=view!=null && view.gameObject.activeInHierarchy,
                ["actionBarBindingExact"]=view!=null && ReferenceEquals(ElementalConsoleBookPresentation.Model(view),model),
                ["inputViewLoadedActive"]=inputView!=null && inputView.gameObject.scene.IsValid() && inputView.gameObject.scene.isLoaded && inputView.gameObject.activeInHierarchy,
                ["inputVmLiveExactContext"]=inputVm!=null && !inputVm.IsDisposed && ReferenceEquals(inputVm.Context,context),
                ["inputViewBindingExact"]=inputView!=null && inputVm!=null && ReferenceEquals(ElementalConsoleBookPresentation.Model(inputView),inputVm),
                ["exactNativeInputLayer"]=inputLayer!=null && layers.Length==1 && ReferenceEquals(layers[0],inputLayer),
                ["nativeInputLayerBound"]=inputLayer!=null && inputLayer.LayerBinded.Value };
            bool ready=predicates.Properties().All(value=>(bool)value.Value);
            _cleanup2ConsoleReadiness=new JObject { ["predicates"]=predicates,["ready"]=ready,
                ["actualLayers"]=new JArray(GamePad.Instance.Layers.Select(value=>new JObject {
                    ["type"]=value.GetType().FullName,["bound"]=value.LayerBinded.Value })),
                ["contract"]="Actual bound InGame input view/VM/context and exact native pushed layer; unused layer.inGameUiContext is not ownership." };
            return ready;
        }

        private bool Cleanup2ConsoleNarrativeAbsent(JObject row)
        {
            var game=Game.Instance;var context=game?.RootUiContext?.InGameUiContext;
            var part=context==null?null:typeof(InGameUiContext).GetField("m_StaticPartContext",Cleanup2Fields).GetValue(context) as InGameUiStaticPartContext;
            var dialog=part==null?null:typeof(InGameUiStaticPartContext).GetField("m_DialogUiCtx",Cleanup2Fields).GetValue(part) as DialogUiContext;
            var config=part==null?null:typeof(InGameUiStaticPartContext).GetField("m_Config",Cleanup2Fields).GetValue(part) as InGameUiStaticPartConfig;
            var dialogConfig=dialog==null?null:typeof(DialogUiContext).GetField("m_Config",Cleanup2Fields).GetValue(dialog) as DialogUiConfig;
            var predicates=new JObject {
                ["exactConsoleReview"]=IsCleanup2ConsoleReview && game!=null && game.IsControllerGamepad,
                ["nativeDialogControllerPresent"]=game?.DialogController!=null,
                ["noLiveBlueprintDialog"]=game?.DialogController!=null && game.DialogController.Dialog==null,
                ["exactNativeContext"]=context!=null && part!=null && ReferenceEquals(part.InGameCtx,context),
                ["dialogContextLive"]=dialog!=null && !dialog.IsDisposed,
                ["nativeDialogDisabled"]=dialog!=null && !dialog.IsDialogEnabled,
                ["exactDialogConfig"]=config!=null && dialogConfig!=null && ReferenceEquals(config.DialogConfig,dialogConfig) };
            foreach(string name in new[]{"m_DialogVm","m_BookEventVm","m_InterchapterVM"})
            {
                var field=typeof(DialogUiContext).GetField(name,Cleanup2Fields);
                predicates[name+"Absent"]=dialog!=null && field!=null && field.GetValue(dialog)==null;
            }
            var views=new JArray();row["consoleViews"]=views;
            if(dialogConfig!=null)
            {
                foreach(string name in new[]{"DialogView","BookEventView","InterchapterView"})
                {
                    var field=typeof(DialogUiConfig).GetField(name,Cleanup2Fields);
                    var view=field?.GetValue(dialogConfig) as MonoBehaviour;
                    var observation=new JObject { ["configuredView"]=name };views.Add(observation);
                    predicates[name+"Closed"]=Cleanup2ConsoleNarrativeViewHidden(view,name=="DialogView",observation);
                }
            }
            else predicates["configuredViewsPresent"]=false;
            row["consolePredicates"]=predicates;
            row["contract"]="Native console DialogUiContext IsDialogEnabled0600626E and three VM refs; Stop06006276/75/73 dispose/null. Exact configured view/input ownership and native hidden rendering, no Stop/Show or layer mutation.";
            return predicates.Properties().All(value=>(bool)value.Value);
        }

        private static bool Cleanup2ConsoleNarrativeViewHidden(MonoBehaviour view,bool ordinary,JObject row)
        {
            Cleanup2NarrativeObject(view,row);
            if(view==null || GamePad.Instance==null)return false;
            var field=view.GetType().GetField("m_InputLayer",Cleanup2Fields);
            var layer=field?.GetValue(view) as InputLayer;
            bool unbound=ElementalConsoleBookPresentation.Model(view)==null && field!=null &&
                (layer==null || !layer.LayerBinded.Value && !GamePad.Instance.Layers.Any(value=>ReferenceEquals(value,layer)));
            bool loaded=view.gameObject.scene.IsValid() && view.gameObject.scene.isLoaded;
            var rendered=view.GetComponentsInChildren<CanvasRenderer>(true);
            bool zero=rendered.Length>0 && rendered.All(value=>!value.gameObject.activeInHierarchy || value.GetInheritedAlpha()==0f) &&
                !view.GetComponentsInChildren<Renderer>(true).Any(value=>value.enabled && value.gameObject.activeInHierarchy);
            bool nativeHiddenPosition=false;
            if(ordinary)
            {
                var initialized=view.GetType().GetField("m_IsInit",Cleanup2Fields);
                var hiddenY=view.GetType().GetField("m_HidePosY",Cleanup2Fields);
                // Native getter060060E5 honors the configured private target;
                // it need not be the component's own transform.
                var rect=view.GetType().GetProperty("RectTransform",Cleanup2Fields)?.GetValue(view,null) as RectTransform;
                nativeHiddenPosition=initialized!=null && initialized.FieldType==typeof(bool) && (bool)initialized.GetValue(view) &&
                    hiddenY!=null && hiddenY.FieldType==typeof(int) && rect!=null && rect.anchoredPosition.y==(int)hiddenY.GetValue(view);
                row["nativeInitializedHiddenPosition"]=nativeHiddenPosition;
                row["anchoredY"]=rect==null?JValue.CreateNull():new JValue(rect.anchoredPosition.y);
                row["nativeHideY"]=hiddenY==null?JValue.CreateNull():JToken.FromObject(hiddenY.GetValue(view));
            }
            row["loaded"]=loaded;row["modelAndInputUnbound"]=unbound;row["allRenderedAlphaZero"]=zero;
            row["activeCanvasRenderers"]=new JArray(rendered.Where(value=>value.gameObject.activeInHierarchy).Select(value=>new JObject {
                ["path"]=Cleanup2NarrativePath(view.transform,value.transform),["inheritedAlpha"]=value.GetInheritedAlpha() }));
            return loaded && unbound && (!view.gameObject.activeInHierarchy || zero || nativeHiddenPosition);
        }

        private IEnumerable<int> ReviewSummonCleanup2ElementalConsole(ExpandedSummoningCorrectionFixture fixture,Spellbook book)
        {
            if(!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters==null ||
                _request.Parameters.Count!=1 || (string)_request.Parameters["reviewPhase"]!="elementals-console" ||
                !Cleanup2LearningModuleState(Cleanup2OffState()))
                throw new InvalidOperationException("Exact stable ON or fresh-OFF automatic-exit elementals-console request required.");
            bool off=Cleanup2OffState();
            var game=Game.Instance;var player=game.Player;var selection=UnitSelectionManager.Instance;
            var inGame=game.RootUiContext?.InGameUiContext;
            if(!game.IsControllerGamepad || inGame==null || selection==null || GamePad.Instance==null)
                throw new InvalidOperationException("Actual native console local UI and selection manager required.");
            var staticPart=(InGameUiStaticPartContext)typeof(InGameUiContext).GetField("m_StaticPartContext",Cleanup2Fields).GetValue(inGame);
            var bar=(ActionBarVM)typeof(InGameUiStaticPartContext).GetField("m_ActionBarVm",Cleanup2Fields).GetValue(staticPart);
            var service=staticPart.CharcaterServiceContext;
            if(bar==null || service==null || service.CharGenIsOpened ||
                typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service)!=null)
                throw new InvalidOperationException("Idle actual console action bar and character service required.");
            var party=player.Party.ToArray();var selected=selection.SelectedUnits.ToArray();var current=selection.CurrentSelectUnitValue;
            var selectionState=new Cleanup2ConsoleSelectionSnapshot(selection);
            var original=party.Select(value=>new Cleanup2TooltipInputs(value)).ToArray();
            var originalPins=party.Select(value=>new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var originalLayers=GamePad.Instance.Layers.ToArray();bool added=false;
            var mode=Cleanup2EnterNativeDefault(fixture,"elementals-console");
            var screens=new NativeIconScreenEvidence(_request);var watch=Stopwatch.StartNew();
            var hooks=HarmonyInstance.Create("kmg.cleanup2.elementals-console."+_request.RunId);
            ActionBarSelectorVm ownedSelector=null;
            var ownedSelectors=new HashSet<ActionBarSelectorVm>();
            var ownedContexts=new HashSet<ElementalConsoleContext>();
            var ownedDispatched=new HashSet<Kingmaker.UnitLogic.Abilities.AbilityData>();
            InputLayer[] selectorBaseLayers=null;UnitEntityData[] ownedParty=null,ownedSelection=null;
            var gamepad=GamePad.Instance;
            Func<bool> uiOwned=()=>ownedParty!=null && player.Party.SequenceEqual(ownedParty) &&
                Cleanup2LearningModuleState(off) &&
                ReferenceEquals(Game.Instance,game) && ReferenceEquals(game.RootUiContext?.InGameUiContext,inGame) &&
                ReferenceEquals(UnitSelectionManager.Instance,selection) && ReferenceEquals(GamePad.Instance,gamepad) &&
                ReferenceEquals(typeof(InGameUiStaticPartContext).GetField("m_ActionBarVm",Cleanup2Fields).GetValue(staticPart),bar) &&
                ReferenceEquals(bar.Unit.Value,fixture.Caster) && ReferenceEquals(selection.CurrentSelectUnitValue,fixture.Caster) &&
                ownedSelection!=null && selection.SelectedUnits.SequenceEqual(ownedSelection);
            Action rememberSelector=()=>{
                var live=bar.SelectorVm.Value;var owner=ElementalSourceChooserConsole.Context(live);
                if(live==null || owner==null || !owner.Current || !uiOwned() || !ReferenceEquals(owner.Owner,bar) ||
                    !ReferenceEquals(owner.Unit,fixture.Caster))return;
                if(ownedSelectors.Contains(live) || ownedContexts.Contains(owner))
                {ownedSelectors.Add(live);ownedContexts.Add(owner);}
            };
            Func<bool> selectorInputOwned=()=>{
                var live=bar.SelectorVm.Value;var owner=ElementalSourceChooserConsole.Context(live);
                if(live==null || selectorBaseLayers==null || !uiOwned() ||
                    (!ownedSelectors.Contains(live) && (owner==null || !ownedContexts.Contains(owner))))return false;
                if(owner!=null && (!owner.Current || !ownedContexts.Contains(owner) || !ReferenceEquals(owner.Owner,bar) ||
                    !ReferenceEquals(owner.Unit,fixture.Caster)))return false;
                var views=Resources.FindObjectsOfTypeAll<ActionBarSelectorView>().Where(value=>value.gameObject.scene.IsValid() &&
                    value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy &&
                    ReferenceEquals(ElementalConsoleBookPresentation.Model(value),live)).ToArray();
                if(views.Length!=1)return false;
                var layer=typeof(ActionBarSelectorView).GetField("m_InputLayer",Cleanup2Fields).GetValue(views[0]) as InputLayer;
                var actual=gamepad.Layers.ToArray();
                return layer!=null && actual.Count(value=>ReferenceEquals(value,layer))==1 &&
                    actual.Length==selectorBaseLayers.Length+1 && actual.Where(value=>!ReferenceEquals(value,layer)).SequenceEqual(selectorBaseLayers);
            };
            try
            {
                const BindingFlags statics=BindingFlags.Static|BindingFlags.NonPublic;
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"),null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix",statics)),null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix",statics)),null,null);
                _expandedSummoningPlayerPathCaptureActive=true;
                if(!party.Contains(fixture.Caster))
                {player.PartyCharacters.Add(fixture.Caster);added=true;player.InvalidateCharacterLists();player.UpdateCharacterLists();}
                ownedParty=player.Party.ToArray();
                foreach(var parent in StandaloneElementalPresentationPolicy.All.Where(value=>value.IsParent))
                    PrepareExpandedSummoningPlayerPathSpell(book,Cleanup2ElementalAbility(fixture,parent.Guid),
                        Cleanup2ElementalAbility(fixture,parent.Guid),parent.CanonicalTier);
                selection.SelectUnit(fixture.Caster.View,true,true,false);
                for(int frame=0;frame<12;frame++)yield return 0;
                if(!ReferenceEquals(bar.Unit.Value,fixture.Caster) || !ReferenceEquals(selection.CurrentSelectUnitValue,fixture.Caster))
                    throw new InvalidOperationException("Actual console selection did not bind the owned caster.");
                // Installed SelectedUnits returns the native grouped cohort
                // when it contains CurrentSelectUnit, not always a singleton.
                // Retain that established cohort; do not unlink other units
                // or manufacture a one-element selection for this observer.
                ownedSelection=selection.SelectedUnits.ToArray();
                var input=new Cleanup2TooltipInputs(fixture.Caster);var pins=new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
                var layers=GamePad.Instance.Layers.ToArray();
                selectorBaseLayers=layers;
                if(!uiOwned() || bar.SelectorVm.Value!=null || game.SelectedAbilityHandler?.Ability!=null)
                    throw new InvalidOperationException("Foreign console selector/selection/targeting; source menu is not opened.");
                if(off)
                {
                    var destination=bar.CurrentPage.FirstOrDefault(value=>value.IsEmpty && value.Index>=0 &&
                        value.Index<fixture.Caster.UISettings.Slots.Length && fixture.Caster.UISettings.Slots[value.Index] is MechanicActionBarSlotEmpty);
                    if(destination==null)throw new InvalidOperationException("Fresh-OFF native console has no visible empty destination slot.");
                    typeof(ActionBarVM).GetMethod("SetSlotSelected",Cleanup2Fields).Invoke(bar,new object[]{destination});
                }
                typeof(ActionBarVM).GetMethod("ShowSpellMenu",Cleanup2Fields).Invoke(bar,null);ownedSelector=bar.SelectorVm.Value;
                if(ownedSelector!=null)ownedSelectors.Add(ownedSelector);rememberSelector();
                for(int frame=0;frame<8;frame++)yield return 0;
                rememberSelector();
                if(off)
                {
                    foreach(int frame in Cleanup2ConsoleOffNativeSource(fixture,book,bar,ownedSelector,mode,watch,screens,
                        uiOwned,selectorInputOwned,ownedDispatched,selectorBaseLayers))yield return frame;
                    ownedSelector=null;
                    yield break;
                }
                var context=ElementalSourceChooserConsole.Context(ownedSelector);
                if(context==null || !context.Current)throw new InvalidOperationException("Actual console spell selector did not publish an owned source context: "+ElementalSourceChooserConsole.Outcome);
                var coverage=new JArray();
                foreach(int rank in ElementalSourceChooserPolicy.Ranks)
                foreach(string key in ElementalSourceChooserPolicy.Rank(rank).CreatureKeys)
                {
                    var rows=context.Sources.Where(value=>value.Rank==rank && value.CreatureKey==key && ReferenceEquals(value.Book,book) &&
                        ElementalSourceChooserPolicy.Parent(value.Source.Blueprint.AssetGuid)!=null).ToArray();
                    coverage.Add(new JObject { ["rank"]=rank,["key"]=key,["count"]=rows.Length,["exactNativeChain"]=rows.Length==1 &&
                        ReferenceEquals(rows[0].Child.ConvertedFrom,rows[0].Source) && ReferenceEquals(rows[0].Child.Spellbook,book) && rows[0].Preparation!=null });
                }
                bool exact=coverage.Count==24 && coverage.Cast<JObject>().All(value=>(bool)value["exactNativeChain"]);
                Cleanup2Check("elemental-console-24-original-native-source-bindings",exact,new JObject {
                    ["rows"]=coverage,["nativeSelectorView"]=context.BoundView.GetInstanceID(),["nativeInputLayers"]=new JArray(GamePad.Instance.Layers.Select(value=>value.ContextName)),
                    ["exceptions"]=new JArray(context.Sources.Select(value=>value.Label)),["scope"]="Actual native prepared mechanisms/conversion factories and real bound console selector. This row is not a 24-cast claim." },
                    "All six ranks/four elements use original native source, child, spellbook and preparation objects in the real console selector.");
                if(!exact)throw new InvalidOperationException("Native console source binding coverage incomplete.");
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Rank==0 && value.Source==null,false,uiOwned,selectorInputOwned,rememberSelector,ownedDispatched))yield return frame;
                ownedSelector=bar.SelectorVm.Value;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Rank==2 && value.Creature==null,false,uiOwned,selectorInputOwned,rememberSelector,ownedDispatched))yield return frame;
                ownedSelector=bar.SelectorVm.Value;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Creature=="small-fire-elemental" && value.Source==null,false,uiOwned,selectorInputOwned,rememberSelector,ownedDispatched))yield return frame;
                ownedSelector=bar.SelectorVm.Value;context=ElementalSourceChooserConsole.Context(ownedSelector);
                foreach(int frame in screens.Capture("elementals-console-native-original-sources",new JObject {
                    ["nativeInputMode"]="console",["renderedText"]=new JArray(context.BoundView.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value=>value.text)),
                    ["originalSources"]=new JArray(context.Sources.Where(value=>value.Rank==2).Select(value=>new JObject {
                        ["source"]=value.Source.Blueprint.AssetGuid,["child"]=value.Child.Blueprint.AssetGuid,["book"]=value.Book.Blueprint.AssetGuid })),
                    ["scope"]="Actual native console selector row templates, native navigation and native input layer; no synthetic VM or substituted source." },
                    ()=>context.Current && input.Unchanged() && pins.IsRestored() && Cleanup2NoNarrativeOverlay()))yield return frame;
                InvokeTeleportGamepadInput(context.BoundView,"<GetInputLayer>b__11_1");ownedSelector=null;
                for(int frame=0;frame<5;frame++)yield return 0;
                bool cancelled=bar.SelectorVm.Value==null && GamePad.Instance.Layers.SequenceEqual(layers) && input.Unchanged() && pins.IsRestored();
                Cleanup2Check("elemental-console-native-cancel",cancelled,new JObject { ["selectorClosed"]=bar.SelectorVm.Value==null,
                    ["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(layers),["originalSourceBudgetsExact"]=input.Unchanged(),["pinsExact"]=pins.IsRestored() },
                    "Native console rank/species/source traversal and the real native decline input spend nothing and restore the original input-layer stack.");
                if(!cancelled)throw new InvalidOperationException("Native console cancellation restoration failed.");

                foreach(int frame in Cleanup2ConsolePinOriginalSource(fixture,book,bar,mode,watch,uiOwned,selectorInputOwned,
                    rememberSelector,ownedSelectors,ownedDispatched,selectorBaseLayers))yield return frame;

                if(!uiOwned() || bar.SelectorVm.Value!=null || game.SelectedAbilityHandler?.Ability!=null || !GamePad.Instance.Layers.SequenceEqual(selectorBaseLayers))
                    throw new InvalidOperationException("Foreign console selector/input owner; cast source menu is not reopened.");
                typeof(ActionBarVM).GetMethod("ShowSpellMenu",Cleanup2Fields).Invoke(bar,null);ownedSelector=bar.SelectorVm.Value;
                if(ownedSelector!=null)ownedSelectors.Add(ownedSelector);rememberSelector();
                for(int frame=0;frame<6;frame++)yield return 0;
                rememberSelector();
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Rank==0 && value.Source==null,false,uiOwned,selectorInputOwned,rememberSelector,ownedDispatched))yield return frame;
                ownedSelector=bar.SelectorVm.Value;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Rank==2 && value.Creature==null,false,uiOwned,selectorInputOwned,rememberSelector,ownedDispatched))yield return frame;
                ownedSelector=bar.SelectorVm.Value;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Creature=="small-fire-elemental" && value.Source==null,false,uiOwned,selectorInputOwned,rememberSelector,ownedDispatched))yield return frame;
                ownedSelector=bar.SelectorVm.Value;context=ElementalSourceChooserConsole.Context(ownedSelector);
                var source=context.Sources.Single(value=>value.CreatureKey=="small-fire-elemental" && ReferenceEquals(value.Book,book));
                int before=CountAvailableExpandedSummoningSlots(book,2);string resources=Cleanup2ElementalResources(fixture.Caster);
                ExpandedSummoningRuleCapture.Clear();ExpandedSummoningPlayerPathEvents.Clear();
                _expandedSummoningPlayerPathRuleCastCount=0;_expandedSummoningPlayerPathSpawnActionCount=0;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>ReferenceEquals(value.Source,source),true,uiOwned,selectorInputOwned,rememberSelector,ownedDispatched))yield return frame;
                ownedSelector=null;
                bool selectedChild=ReferenceEquals(game.SelectedAbilityHandler?.Ability,source.Child) &&
                    ReferenceEquals(ElementalSourceChooserRuntime.LastDispatched,source.Child);
                bool clicked=selectedChild && game.SelectedAbilityHandler.OnClick(null,fixture.Caster.Position+new Vector3(2,0,2),0);
                var command=fixture.Caster.Commands.GetCommand<UnitUseAbility>();
                bool nativeCommand=clicked && command!=null && ReferenceEquals(command.Spell,source.Child);
                var castWatch=Stopwatch.StartNew();
                while(nativeCommand && castWatch.Elapsed.TotalSeconds<25 && watch.Elapsed.TotalSeconds<360 &&
                    (_expandedSummoningPlayerPathRuleCastCount==0 || !fixture.Caster.Commands.Empty))
                {if(!mode.Current || !Cleanup2NoNarrativeOverlay())throw new InvalidOperationException("Console cast lost its native Default context.");game.IsPaused=false;yield return 0;}
                var units=ExpandedSummoningRuleCapture.Where(value=>value!=null && ExpandedSummoningPlayerPathUnitExact(value,fixture.Caster)).Distinct().ToArray();
                foreach(var unit in units)if(!fixture.Created.Contains(unit))fixture.Created.Add(unit);
                var expected=SummonSignatureReader.Read(source.Child.Blueprint).Spawns().ToArray();
                bool cast=nativeCommand && expected.Length==1 && units.Length==1 && units[0].Blueprint.AssetGuid==expected[0].Unit &&
                    _expandedSummoningPlayerPathRuleCastCount==1 && _expandedSummoningPlayerPathSpawnActionCount==1 &&
                    CountAvailableExpandedSummoningSlots(book,2)==before-1 && !source.Preparation.Available &&
                    Cleanup2ElementalResources(fixture.Caster)==resources && pins.IsRestored() && fixture.Caster.Commands.Empty;
                Cleanup2Check("elemental-console-native-source-use-cast",cast,new JObject { ["selectedOriginalChild"]=selectedChild,
                    ["actualNativeCommand"]=nativeCommand,["nativeSelectorInput"]="Actual ActionBarSelectorView OnFunc01 handler",
                    ["targetInput"]="Actual selected-ability native point handler; no OS/controller emulation or RuleCast shortcut",
                    ["sourceChain"]=DescribeExpandedSummoningAbilityData(source.Child),["availableBefore"]=before,
                    ["availableAfter"]=CountAvailableExpandedSummoningSlots(book,2),["spentOriginalSlot"]=!source.Preparation.Available,
                    ["ruleCasts"]=_expandedSummoningPlayerPathRuleCastCount,["spawns"]=_expandedSummoningPlayerPathSpawnActionCount,
                    ["expectedUnit"]=expected.FirstOrDefault()?.Unit,["actualUnit"]=units.FirstOrDefault()?.Blueprint.AssetGuid,
                    ["nativeMode"]=mode.Describe(),["manualCommandTicks"]=0,["forcedActs"]=0 },
                    "Native console Use hands off the exact original source child, one ordinary native cast spawns the expected actor and only its original preparation is debited.");
                if(!cast)throw new InvalidOperationException("Native console explicit-source cast/debit failed.");
                foreach(int frame in Cleanup2ConsolePreparation(fixture,book,service,screens))yield return frame;
                // Native learning/preview entry is added below independently of
                // the cast fixture; a real screen is required, never a headless VM.
            }
            finally
            {
                var errors=new List<string>();
                Action<string,Action> restore=(name,action)=>{try{action();}catch(Exception error){errors.Add(name+":"+error.GetType().Name+":"+error.Message);}};
                try
                {
                    _expandedSummoningPlayerPathCaptureActive=false;
                    restore("capture-hooks",()=>hooks.UnpatchAll(hooks.Id));
                    restore("owned-targeting",()=>{
                        var ability=game.SelectedAbilityHandler?.Ability;
                        if(ability==null)return;
                        if(!mode.Current || !uiOwned() || !ownedDispatched.Contains(ability))
                            throw new InvalidOperationException("Foreign targeting is not dropped.");
                        game.SelectedAbilityHandler.DropAbility();
                    });
                    restore("owned-selector",()=>{
                        rememberSelector();
                        var live=bar.SelectorVm.Value;if(live==null)return;
                        if(!mode.Current || !selectorInputOwned())
                            throw new InvalidOperationException("Foreign or unproved selector/input layer is not closed.");
                        // Native decline restores selector tooltip/active
                        // bookkeeping as well as disposing/unbinding its view.
                        // The current VM may be newer than the caller's last
                        // assignment after a helper yielded or threw.
                        live.OnDecline();
                        if(bar.SelectorVm.Value!=null || !GamePad.Instance.Layers.SequenceEqual(selectorBaseLayers))
                            throw new InvalidOperationException("Owned native selector did not restore the captured input-layer stack.");
                    });
                    restore("owned-spellbook",()=>{
                        var vm=typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service) as SpellBookVM;
                        if(vm==null)return;
                        if(!uiOwned() || !ReferenceEquals(vm.UnitEntity.Value,fixture.Caster))
                            throw new InvalidOperationException("Foreign character-service spellbook is not closed.");
                        Cleanup2CloseOwnedConsoleBook(service,vm,fixture.Caster);
                    });
                    bool untouched=player.Party.SequenceEqual(party) && selection.SelectedUnits.SequenceEqual(selected) &&
                        ReferenceEquals(selection.CurrentSelectUnitValue,current);
                    var authorityPredicates=new JObject {
                        ["sameGame"]=ReferenceEquals(Game.Instance,game), ["sameSelection"]=ReferenceEquals(UnitSelectionManager.Instance,selection),
                        ["sameGamepad"]=ReferenceEquals(GamePad.Instance,gamepad), ["selectorClosed"]=bar.SelectorVm.Value==null,
                        ["nativeBookFieldCleared"]=typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service)==null,
                        ["tooltipClosed"]=game.UI.TooltipsController.ContextTooltipData==null, ["targetingClosed"]=game.SelectedAbilityHandler?.Ability==null,
                        ["originalOrOwnedUi"]=untouched || uiOwned() };
                    bool authority=authorityPredicates.Properties().All(value=>(bool)value.Value);
                    bool selectionRestored=false;
                    if(!authority)errors.Add("Foreign native console owner; original party/selection is not overwritten.");
                    else
                    {
                        restore("owned-party-entry",()=>{if(added){player.PartyCharacters.Remove(fixture.Caster);player.InvalidateCharacterLists();player.UpdateCharacterLists();}});
                        restore("selection",()=>{selectionRestored=selectionState.Restore(fixture);if(!selectionRestored)throw new InvalidOperationException(selectionState.Outcome);});
                    }
                    bool restored=errors.Count==0 && player.Party.SequenceEqual(party) && selection.SelectedUnits.SequenceEqual(selected) &&
                        ReferenceEquals(selection.CurrentSelectUnitValue,current) && original.All(value=>value.Unchanged()) && originalPins.All(value=>value.IsRestored()) &&
                        GamePad.Instance.Layers.SequenceEqual(originalLayers) && selectionRestored && Cleanup2LearningModuleState(off);
                    Cleanup2Check("elemental-console-native-ui-restored",restored,new JObject { ["partyExact"]=player.Party.SequenceEqual(party),
                        ["selectionExact"]=selection.SelectedUnits.SequenceEqual(selected),["currentUnitExact"]=ReferenceEquals(selection.CurrentSelectUnitValue,current),
                        ["originalInputsExact"]=original.All(value=>value.Unchanged()),["pinsExact"]=originalPins.All(value=>value.IsRestored()),
                        ["nativeSelectionRestoration"]=selectionState.Describe(),
                        ["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(originalLayers),["restorationAuthority"]=authority,
                        ["restorationPredicates"]=authorityPredicates,
                        ["ownedSelectorContexts"]=ownedContexts.Count,["ownedNativeSelectorTokens"]=ownedSelectors.Count,
                        ["locallyDispatchedChildReferences"]=ownedDispatched.Count,["freshModuleOff"]=off,["errors"]=new JArray(errors) },
                        "All original native party/selection/source budgets/pins and controller input layers restore before exiting the owned Default scope.");
                    if(!restored)throw new InvalidOperationException("Original console UI restoration is unverified: "+string.Join("|",errors.ToArray()));
                }
                finally{try{screens.Dispose();}finally{mode.Dispose();}}
            }
        }

        private IEnumerable<int> Cleanup2ConsolePinOriginalSource(ExpandedSummoningCorrectionFixture fixture,Spellbook book,
            ActionBarVM bar,Cleanup2NativeDefaultScope mode,Stopwatch phase,Func<bool> uiOwned,Func<bool> selectorInputOwned,
            Action remember,ICollection<ActionBarSelectorVm> selectors,ICollection<AbilityData> dispatched,InputLayer[] layers)
        {
            if(!mode.Current || !uiOwned() || Cleanup2OffState() || phase.Elapsed.TotalSeconds>=360 || bar.SelectorVm.Value!=null ||
                Game.Instance.SelectedAbilityHandler?.Ability!=null || !GamePad.Instance.Layers.SequenceEqual(layers))
                throw new InvalidOperationException("Exact idle ON native selector owner required for source pin.");
            var caster=fixture.Caster;var snapshot=new TeleportationUiSettingsFixture(caster.UISettings);
            var slots=caster.UISettings.Slots;var contents=slots.ToArray();
            var destination=bar.CurrentPage.FirstOrDefault(value=>value.IsEmpty && value.Index>=0 && value.Index<slots.Length && slots[value.Index] is MechanicActionBarSlotEmpty);
            if(destination==null)throw new InvalidOperationException("Native ON source pin has no visible empty slot.");
            int index=destination.Index;var prior=slots[index];MechanicActionBarSlot owned=null;ElementalNativeSource source=null;
            var debit=new Cleanup2SourceDebit(caster);
            try
            {
                typeof(ActionBarVM).GetMethod("SetSlotSelected",Cleanup2Fields).Invoke(bar,new object[]{destination});
                typeof(ActionBarVM).GetMethod("ShowSpellMenu",Cleanup2Fields).Invoke(bar,null);
                var selector=bar.SelectorVm.Value;if(selector!=null)selectors.Add(selector);remember();
                for(int frame=0;frame<6;frame++)yield return 0;remember();
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Rank==0 && value.Source==null,false,uiOwned,selectorInputOwned,remember,dispatched))yield return frame;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Rank==2 && value.Creature==null,false,uiOwned,selectorInputOwned,remember,dispatched))yield return frame;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Creature=="small-fire-elemental" && value.Source==null,false,uiOwned,selectorInputOwned,remember,dispatched))yield return frame;
                selector=bar.SelectorVm.Value;var context=ElementalSourceChooserConsole.Context(selector);
                if(!mode.Current || !uiOwned() || !selectorInputOwned() || context==null || !context.Current || !debit.Unspent || !snapshot.IsRestored())
                    throw new InvalidOperationException("Exact unchanged native source context required before Confirm pin.");
                source=context.Sources.Single(value=>value.CreatureKey=="small-fire-elemental" && ReferenceEquals(value.Book,book));
                var item=selector.ItemVms.Single(value=>value.MechanicActionBarSlot is ElementalConsoleNode node && ReferenceEquals(node.Source,source));
                var row=context.BoundView.GetComponentsInChildren<ActionBarSelectorItemView>(false).Single(value=>ReferenceEquals(ElementalConsoleBookPresentation.Model(value),item));
                var navigation=(ConsoleNavigationBehavior)typeof(ActionBarSelectorView).GetField("m_NavigationBehavior",Cleanup2Fields).GetValue(context.BoundView);
                if(source.Validate()!=null || !navigation.SelectEntityManually(row) || !ReferenceEquals(navigation.GetCurrentSelectedEntity(),row))
                    throw new InvalidOperationException("Exact validated native source node did not receive Confirm focus.");
                try{InvokeTeleportGamepadInput(context.BoundView,"<GetInputLayer>b__11_0");}
                finally
                {
                    // Capture the synchronous native Result(false) receipt
                    // before yielding. Never adopt a refreshed/foreign slot.
                    if(ReferenceEquals(caster.UISettings.Slots,slots) && slots[index] is MechanicActionBarSlotSpontaneusConvertedSpell &&
                        ReferenceEquals(slots[index].Unit,caster) && ReferenceEquals(slots[index].GetContentData(),source.Child) &&
                        slots.Where((value,i)=>i!=index && !ReferenceEquals(value,contents[i])).Any()==false)owned=slots[index];
                }
                for(int frame=0;frame<6;frame++)yield return 0;
                bool exact=mode.Current && uiOwned() && bar.SelectorVm.Value==null && GamePad.Instance.Layers.SequenceEqual(layers) &&
                    owned!=null && ReferenceEquals(caster.UISettings.Slots,slots) && ReferenceEquals(slots[index],owned) &&
                    !(owned is ElementalConsoleNode) && !(owned is ElementalChooserLauncher) && ReferenceEquals(owned.GetContentData(),source.Child) &&
                    ReferenceEquals(source.Child.ConvertedFrom,source.Source) && ReferenceEquals(source.Child.Spellbook,book) &&
                    source.Validate()==null && debit.Unspent && Game.Instance.SelectedAbilityHandler?.Ability==null;
                Cleanup2Check("elemental-console-explicit-source-native-pin",exact,new JObject {
                    ["nativeConfirmInput"]="ActionBarSelectorView Confirm -> ActionBarVM.Result(execute=false)",
                    ["storedNativeMechanism"]=owned?.GetType().FullName,["transientNodeSerialized"]=slots[index] is ElementalConsoleNode || slots[index] is ElementalChooserLauncher,
                    ["exactRetainedChild"]=owned!=null && ReferenceEquals(owned.GetContentData(),source.Child),
                    ["nativeSourceChain"]=DescribeExpandedSummoningAbilityData(source.Child),["sourceBudgetsUnspent"]=debit.Unspent,
                    ["nativeInputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(layers),["pinIndex"]=index },
                    "Native Confirm on an explicit ON source pins its original native converted child mechanism, retaining the exact source/book and spending nothing.");
                if(!exact)throw new InvalidOperationException("Native ON explicit source pin did not retain its exact native child mechanism.");
            }
            finally
            {
                if(owned!=null)Cleanup2AlternateRestorePin(bar,caster,slots,contents,index,prior,owned,snapshot);
                bool restored=snapshot.IsRestored() && debit.Unspent;
                Cleanup2Check("elemental-console-explicit-source-pin-restored",restored,new JObject {
                    ["pinArrayAndMetadataExact"]=snapshot.IsRestored(),["sourceBudgetsUnspent"]=debit.Unspent },
                    "Only the exact observed native source pin is removed and the original pin array/metadata and all source budgets restore.");
                if(!restored)throw new InvalidOperationException("Native ON source pin restoration is unverified; foreign changes are preserved.");
            }
        }

        // Fresh OFF follows the installed native selector -> pin -> converted
        // panel callbacks. It never creates a chooser node or a source chain.
        private IEnumerable<int> Cleanup2ConsoleOffNativeSource(ExpandedSummoningCorrectionFixture fixture,Spellbook book,
            ActionBarVM bar,ActionBarSelectorVm selector,Cleanup2NativeDefaultScope mode,Stopwatch phase,NativeIconScreenEvidence screens,
            Func<bool> uiOwned,Func<bool> selectorInputOwned,ICollection<AbilityData> dispatched,InputLayer[] baseLayers)
        {
            var game=Game.Instance;var caster=fixture.Caster;
            var pins=new TeleportationUiSettingsFixture(caster.UISettings);
            var slots=caster.UISettings.Slots;var contents=slots.ToArray();
            var destination=bar.CurrentPage.FirstOrDefault(value=>value.IsEmpty && value.Index>=0 && value.Index<slots.Length &&
                slots[value.Index] is MechanicActionBarSlotEmpty);
            if(destination==null)throw new InvalidOperationException("Fresh-OFF native empty destination changed before selector input.");
            int index=destination.Index;var prior=slots[index];MechanicActionBarSlot ownedPin=null;
            ActionBarSlotVM pinned=null;ActionBarConvertedVm converted=null;ActionBarConvertedView convertedView=null;
            AbilityData child=null;UnitUseAbility command=null;
            Func<bool> boundary=()=>mode.Current && uiOwned() && Cleanup2OffState() && Cleanup2NoNarrativeOverlay() && phase.Elapsed.TotalSeconds<360;
            try
            {
                if(!boundary() || selector==null || !ReferenceEquals(bar.SelectorVm.Value,selector) || !selectorInputOwned() ||
                    ElementalSourceChooserView.IsOpen || ElementalSourceChooserConsole.Context(selector)!=null ||
                    selector.ItemVms.Any(value=>value.MechanicActionBarSlot is ElementalConsoleNode || value.MechanicActionBarSlot is ElementalChooserLauncher))
                    throw new InvalidOperationException("Fresh-OFF selector is not the original native owned surface.");
                var parent=Cleanup2ElementalAbility(fixture,ElementalSourceChooserPolicy.Rank(2).Guid);
                var item=selector.ItemVms.Single(value=>{
                    var data=value.MechanicActionBarSlot.GetContentData() as AbilityData;
                    return ReferenceEquals(data?.Blueprint,parent) && ReferenceEquals(data?.Spellbook,book) &&
                        value.MechanicActionBarSlot is MechanicActionBarSlotMemorizedSpell;
                });
                var source=(AbilityData)item.MechanicActionBarSlot.GetContentData();
                var preparation=((MechanicActionBarSlotMemorizedSpell)item.MechanicActionBarSlot).SpellSlot;
                var debit=new Cleanup2SourceDebit(caster);
                var view=Resources.FindObjectsOfTypeAll<ActionBarSelectorView>().Single(value=>value.gameObject.scene.IsValid() &&
                    value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy && ReferenceEquals(ElementalConsoleBookPresentation.Model(value),selector));
                var row=view.GetComponentsInChildren<ActionBarSelectorItemView>(false).Single(value=>ReferenceEquals(ElementalConsoleBookPresentation.Model(value),item));
                var navigation=(ConsoleNavigationBehavior)typeof(ActionBarSelectorView).GetField("m_NavigationBehavior",Cleanup2Fields).GetValue(view);
                if(!navigation.SelectEntityManually(row) || !ReferenceEquals(navigation.GetCurrentSelectedEntity(),row))
                    throw new InvalidOperationException("Native OFF source row did not receive real navigation focus.");
                var icon=(Image)typeof(ActionBarSelectorItemView).GetField("m_Icon",Cleanup2Fields).GetValue(row);
                var title=(TextMeshProUGUI)typeof(ActionBarSelectorItemView).GetField("m_Title",Cleanup2Fields).GetValue(row);
                var parents=StandaloneElementalPresentationPolicy.All.Where(value=>value.IsParent).ToArray();
                bool allParents=parents.All(parentSpec=>selector.ItemVms.Any(value=>{
                    var data=value.MechanicActionBarSlot.GetContentData() as AbilityData;
                    return data!=null && data.Blueprint.AssetGuid==parentSpec.Guid && ReferenceEquals(data.Spellbook,book);
                }));
                bool preparationExact=preparation!=null && preparation.Available && ReferenceEquals(preparation.Spell,source) &&
                    ReferenceEquals(source.Caster,caster.Descriptor) && source.ConvertedFrom==null;
                // Native41: native Bind b__10_1 sets m_Title.text to the VM title
                // unchanged and the controller label renders it upper-case; the
                // visible text must name the source, ignoring presentational case.
                bool titleExact=title!=null && string.Equals(title.GetParsedText(),source.Name,StringComparison.OrdinalIgnoreCase) && !title.isTextTruncated;
                bool iconExact=icon!=null && ReferenceEquals(icon.sprite,parent.Icon) && Cleanup2TooltipGraphicVisible(icon);
                bool nativeRows=allParents && preparationExact && debit.Unspent && titleExact && iconExact;
                Cleanup2Check("elementals-off-console-original-source-rows",nativeRows,new JObject {
                    ["freshModuleOff"]=true,["originalParentsPresent"]=parents.Length,["ownedChooserAbsent"]=ElementalSourceChooserConsole.Context(selector)==null,
                    ["ownedNodesAbsent"]=!selector.ItemVms.Any(value=>value.MechanicActionBarSlot is ElementalConsoleNode),
                    ["allNativeParentRows"]=allParents,["originalPreparationExact"]=preparationExact,["titleExact"]=titleExact,["iconExact"]=iconExact,
                    ["nativeTitleText"]=title?.text,["titleTruncated"]=title!=null && title.isTextTruncated,["sourceName"]=source.Name,
                    ["nativeTitle"]=title?.GetParsedText(),["source"]=source.Blueprint.AssetGuid,["book"]=book.Blueprint.AssetGuid,
                    ["nativeSourceMechanism"]=item.MechanicActionBarSlot.GetType().FullName,["sourceBudgetsUnspent"]=debit.Unspent },
                    "Fresh OFF retains the real six native parent rows and original visible source title/icon, without an owned chooser or any source debit.");
                if(!nativeRows)throw new InvalidOperationException("Fresh-OFF original controller source rows changed.");
                foreach(int frame in screens.Capture("elementals-off-console-native-source",new JObject {
                    ["source"]=source.Blueprint.AssetGuid,["freshModuleOff"]=true,["nativeSelectorView"]=view.GetInstanceID() },
                    ()=>boundary() && selectorInputOwned() && debit.Unspent && pins.IsRestored() && ReferenceEquals(bar.SelectorVm.Value,selector)))yield return frame;
                ownedPin=item.MechanicActionBarSlot;
                InvokeTeleportGamepadInput(view,"<GetInputLayer>b__11_0");
                for(int frame=0;frame<8;frame++)yield return 0;
                if(!boundary() || bar.SelectorVm.Value!=null || !ReferenceEquals(caster.UISettings.Slots,slots) || !ReferenceEquals(slots[index],ownedPin) || !debit.Unspent)
                    throw new InvalidOperationException("Native OFF confirm did not pin the exact original source mechanism without spending it.");
                pinned=bar.CurrentPage.Single(value=>value.Index==index && ReferenceEquals(value.MechanicActionBarSlot,ownedPin));
                var sourceView=Resources.FindObjectsOfTypeAll<ActionBarSlotView>().Single(value=>value.gameObject.scene.IsValid() &&
                    value.gameObject.scene.isLoaded && value.gameObject.activeInHierarchy && ReferenceEquals(value.GetViewModel(),pinned));
                pinned.OnShowConvertRequest();converted=pinned.ConvertedVm.Value;
                for(int frame=0;frame<8;frame++)yield return 0;
                convertedView=(ActionBarConvertedView)typeof(ActionBarSlotView).GetField("m_ConvertedView",Cleanup2Fields).GetValue(sourceView);
                if(!boundary() || converted==null || convertedView==null || !convertedView.gameObject.activeInHierarchy ||
                    !ReferenceEquals(ElementalConsoleBookPresentation.Model(convertedView),converted))
                    throw new InvalidOperationException("Original native OFF converted panel did not bind.");
                string fire=StandaloneElementalPresentationPolicy.All.Single(value=>value.ParentGuid==parent.AssetGuid &&
                    value.CreatureKeys.Contains("small-fire-elemental")).Guid;
                var nativeChildren=converted.Slots.Select(value=>value.MechanicActionBarSlot.GetContentData() as AbilityData).ToArray();
                var childVm=converted.Slots.Single(value=>(value.MechanicActionBarSlot.GetContentData() as AbilityData)?.Blueprint.AssetGuid==fire);
                child=(AbilityData)childVm.MechanicActionBarSlot.GetContentData();
                var childView=convertedView.GetComponentsInChildren<ActionBarSlotView>(false).Single(value=>ReferenceEquals(value.GetViewModel(),childVm));
                var childNavigation=(ConsoleMultiNavigationCollection)typeof(ActionBarConvertedView).GetField("m_NavigationCollection",Cleanup2Fields).GetValue(convertedView);
                childNavigation.SetCurrentEntityManual(childView);
                for(int frame=0;frame<4;frame++)yield return 0;
                var childIcon=(Image)typeof(ActionBarSlotView).GetField("m_Icon",Cleanup2Fields).GetValue(childView);
                var variants=parent.Variants?.ToArray();
                bool original=boundary() && variants!=null && nativeChildren.Length==variants.Length && nativeChildren.All(value=>value!=null &&
                    variants.Any(variant=>ReferenceEquals(variant,value.Blueprint)) && ReferenceEquals(value.ConvertedFrom,source) && ReferenceEquals(value.Spellbook,book)) &&
                    variants.All(variant=>nativeChildren.Count(value=>ReferenceEquals(value?.Blueprint,variant))==1) &&
                    !converted.Slots.Any(value=>value.MechanicActionBarSlot is ElementalConsoleNode) &&
                    ReferenceEquals(childNavigation.CurrentEntity,childView) && ReferenceEquals(childIcon?.sprite,child.Blueprint.Icon) &&
                    Cleanup2TooltipGraphicVisible(childIcon) && child.Name==child.Blueprint.Name && debit.Unspent;
                Cleanup2Check("elementals-off-console-original-converted-rows",original,new JObject {
                    ["source"]=source.Blueprint.AssetGuid,["child"]=child.Blueprint.AssetGuid,["sourceChain"]=DescribeExpandedSummoningAbilityData(child),
                    ["nativeChildRows"]=new JArray(nativeChildren.Select(value=>value?.Blueprint.AssetGuid)),["nativeOriginalTitle"]=child.Name,
                    ["originalIconVisible"]=ReferenceEquals(childIcon?.sprite,child.Blueprint.Icon) && Cleanup2TooltipGraphicVisible(childIcon),
                    ["originalSourceUnspent"]=debit.Unspent,["freshModuleOff"]=true },
                    "The actual native converted panel retains every original child, the exact native parent/book chain, original title/icon and unspent preparation.");
                if(!original)throw new InvalidOperationException("Fresh-OFF native original child panel changed.");
                foreach(int frame in screens.Capture("elementals-off-console-native-small-fire",new JObject {
                    ["child"]=child.Blueprint.AssetGuid,["nativeConvertedView"]=convertedView.GetInstanceID(),["freshModuleOff"]=true },
                    ()=>boundary() && ReferenceEquals(pinned.ConvertedVm.Value,converted) && debit.Unspent && ReferenceEquals(slots[index],ownedPin)))yield return frame;
                ExpandedSummoningRuleCapture.Clear();ExpandedSummoningPlayerPathEvents.Clear();
                _expandedSummoningPlayerPathRuleCastCount=0;_expandedSummoningPlayerPathSpawnActionCount=0;
                dispatched.Add(child);InvokeTeleportGamepadInput(convertedView,"<GetInputLayer>b__8_0");
                bool selected=ReferenceEquals(game.SelectedAbilityHandler?.Ability,child) && debit.Unspent;
                bool clicked=selected && game.SelectedAbilityHandler.OnClick(null,caster.Position+new Vector3(2,0,2),0);
                command=caster.Commands.GetCommand<UnitUseAbility>();
                bool issued=clicked && command!=null && ReferenceEquals(command.Spell,child);
                var castWatch=Stopwatch.StartNew();
                while(issued && castWatch.Elapsed.TotalSeconds<25 && phase.Elapsed.TotalSeconds<360 &&
                    (_expandedSummoningPlayerPathRuleCastCount==0 || !caster.Commands.Empty))
                {if(!boundary())throw new InvalidOperationException("Native OFF cast lost its exact Default/module/UI context.");game.IsPaused=false;yield return 0;}
                var units=ExpandedSummoningRuleCapture.Where(value=>value!=null && ExpandedSummoningPlayerPathUnitExact(value,caster)).Distinct().ToArray();
                foreach(var unit in units)if(!fixture.Created.Contains(unit))fixture.Created.Add(unit);
                var expected=SummonSignatureReader.Read(child.Blueprint).Spawns().ToArray();
                bool cast=boundary() && issued && command.IsFinished && command.Result.ToString()=="Success" && caster.Commands.Empty &&
                    expected.Length==1 && units.Length==1 && ExpandedSummoningRuleCapture.Distinct().Count()==1 && units[0].Blueprint.AssetGuid==expected[0].Unit &&
                    _expandedSummoningPlayerPathRuleCastCount==1 && _expandedSummoningPlayerPathSpawnActionCount==1 && debit.ExactlyOne(preparation) &&
                    ReferenceEquals(child.ConvertedFrom,source) && ReferenceEquals(child.Spellbook,book);
                Cleanup2Check("elementals-off-console-original-small-fire-cast",cast,new JObject {
                    ["nativeCommandExact"]=issued,["nativeResult"]=command?.Result.ToString(),["nativeFinished"]=command!=null && command.IsFinished,
                    ["nativeSourceChain"]=DescribeExpandedSummoningAbilityData(child),["debit"]=debit.Describe(preparation),
                    ["ruleCasts"]=_expandedSummoningPlayerPathRuleCastCount,["spawns"]=_expandedSummoningPlayerPathSpawnActionCount,
                    ["expectedUnit"]=expected.FirstOrDefault()?.Unit,["actualUnit"]=units.FirstOrDefault()?.Blueprint.AssetGuid,
                    ["nativePathEvents"]=new JArray(ExpandedSummoningPlayerPathEvents),["freshModuleOff"]=true,["manualCommandTicks"]=0,["forcedActs"]=0 },
                    "Actual native console Use targets and casts the original child through its retained source; one real spawn spends exactly its original preparation across all books/resources.");
                if(!cast)throw new InvalidOperationException("Fresh-OFF actual controller cast/debit failed.");
            }
            finally
            {
                foreach(var unit in ExpandedSummoningRuleCapture.Where(value=>value!=null && ExpandedSummoningPlayerPathUnitExact(value,caster)).Distinct())
                    if(!fixture.Created.Contains(unit))fixture.Created.Add(unit);
                var errors=new List<string>();
                Action<string,Action> restore=(name,action)=>{try{action();}catch(Exception error){errors.Add(name+":"+error.Message);}};
                restore("command",()=>{if(command!=null && !command.IsFinished){
                    if(!ReferenceEquals(caster.Commands.GetCommand<UnitUseAbility>(),command) || !ReferenceEquals(command.Spell,child))
                        throw new InvalidOperationException("Foreign command preserved.");command.Interrupt(true);}});
                restore("target",()=>{if(game.SelectedAbilityHandler?.Ability==null)return;
                    if(!mode.Current || !uiOwned() || !ReferenceEquals(game.SelectedAbilityHandler.Ability,child))throw new InvalidOperationException("Foreign targeting preserved.");
                    game.SelectedAbilityHandler.DropAbility();});
                restore("converted-panel",()=>{if(pinned==null || pinned.ConvertedVm.Value==null)return;
                    if(!mode.Current || !uiOwned() || !ReferenceEquals(pinned.ConvertedVm.Value,converted) || convertedView==null ||
                        !ReferenceEquals(ElementalConsoleBookPresentation.Model(convertedView),converted))throw new InvalidOperationException("Foreign converted panel preserved.");
                    converted.Decline();});
                restore("temporary-pin",()=>{if(ownedPin!=null)Cleanup2AlternateRestorePin(bar,caster,slots,contents,index,prior,ownedPin,pins);});
                bool restored=errors.Count==0 && pins.IsRestored() && (pinned==null || pinned.ConvertedVm.Value==null) &&
                    (bar.SelectorVm.Value!=null || GamePad.Instance.Layers.SequenceEqual(baseLayers)) && Cleanup2OffState();
                Cleanup2Check("elementals-off-console-native-pin-restored",restored,new JObject {
                    ["pinsExact"]=pins.IsRestored(),["convertedPanelClosed"]=pinned==null || pinned.ConvertedVm.Value==null,
                    ["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(baseLayers),["errors"]=new JArray(errors),
                    ["scope"]="Only the exact temporary native mechanism/panel/command is retired. The outer owner closes any still-owned source selector; foreign replacements are preserved." },
                    "The temporary native source pin and converted panel restore without adopting or overwriting foreign UI.");
                if(!restored)throw new InvalidOperationException("Fresh-OFF native temporary source restoration failed: "+string.Join("|",errors.ToArray()));
            }
        }

        private IEnumerable<int> Cleanup2ConsoleChoose(ActionBarVM owner,Func<ElementalConsoleNode,bool> predicate,bool use,
            Func<bool> uiOwned,Func<bool> selectorInputOwned,Action rememberSelector,
            ICollection<Kingmaker.UnitLogic.Abilities.AbilityData> ownedDispatched)
        {
            var model=owner.SelectorVm.Value;var context=ElementalSourceChooserConsole.Context(model);
            rememberSelector();
            if(context==null || !context.Current || !uiOwned() || !selectorInputOwned() || Game.Instance.SelectedAbilityHandler?.Ability!=null)
                throw new InvalidOperationException("Exact owned native console context/input layer absent before input.");
            var item=model.ItemVms.Single(value=>value.MechanicActionBarSlot is ElementalConsoleNode node && predicate(node));
            var view=context.BoundView;
            var row=view.GetComponentsInChildren<ActionBarSelectorItemView>(false).Single(value=>ReferenceEquals(ElementalConsoleBookPresentation.Model(value),item));
            var navigation=(ConsoleNavigationBehavior)typeof(ActionBarSelectorView).GetField("m_NavigationBehavior",Cleanup2Fields).GetValue(view);
            if(!navigation.SelectEntityManually(row) || !ReferenceEquals(navigation.GetCurrentSelectedEntity(),row))
                throw new InvalidOperationException("Actual native console row did not accept navigation focus.");
            try
            {
                if(use && item.MechanicActionBarSlot is ElementalConsoleNode dispatchNode && dispatchNode.Source!=null)
                    ownedDispatched.Add(dispatchNode.Source.Child);
                InvokeTeleportGamepadInput(view,use?"<GetInputLayer>b__11_2":"<GetInputLayer>b__11_0");
                // Capture the new positive context synchronously, before the
                // caller can fail during this helper's yielded native frames.
                rememberSelector();
                for(int frame=0;frame<5;frame++)
                {
                    if(!uiOwned() || !Cleanup2NoNarrativeOverlay() || (!use && !selectorInputOwned()))
                        throw new InvalidOperationException("Native console navigation lost its owned context/input layer.");
                    yield return 0;
                    rememberSelector();
                }
            }
            finally{rememberSelector();}
        }

        private IEnumerable<int> Cleanup2ConsolePreparation(ExpandedSummoningCorrectionFixture fixture,Spellbook book,
            CharcaterServiceContext service,NativeIconScreenEvidence screens)
        {
            var before=new Cleanup2TooltipInputs(fixture.Caster);var pins=new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
            var layers=GamePad.Instance.Layers.ToArray();SpellBookVM owned=null;
            try
            {
                typeof(CharcaterServiceContext).GetMethod("OnSpellBookHandler",Cleanup2Fields).Invoke(service,null);
                for(int frame=0;frame<8;frame++)yield return 0;
                owned=typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service) as SpellBookVM;
                if(owned==null || !ReferenceEquals(owned.UnitEntity.Value,fixture.Caster))throw new InvalidOperationException("Native console character service did not open the owned caster spellbook.");
                owned.ChooseSpellBook(book.Blueprint);owned.ChooseLevel(2);for(int frame=0;frame<8;frame++)yield return 0;
                var view=Resources.FindObjectsOfTypeAll<SpellBookView>().Single(value=>value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded &&
                    value.gameObject.activeInHierarchy && ReferenceEquals(ElementalConsoleBookPresentation.Model(value),owned));
                var scroll=view.GetComponentsInChildren<SpellbookKnownSpellsScrollView>(true).Single(value=>ReferenceEquals(ElementalConsoleBookPresentation.Model(value),owned.SpellBookKnownSpells));
                var context=ElementalConsoleBookPresentation.PreparationFor(scroll);
                if(context==null || !context.Current)throw new InvalidOperationException("Actual console preparation source disclosure absent: "+ElementalConsoleBookPresentation.Outcome);
                var authority=owned.SpellBookKnownSpells.KnownSpells.ToArray();
                var header=scroll.GetComponentsInChildren<SpellBookKnownSpellView>(false).Single(value=>ReferenceEquals(value.GetViewModel(),context.Header));
                bool visible=header.IsValid() && header.GetComponentsInChildren<TextMeshProUGUI>(false).Any(value=>value.text=="Elementals" && !value.isTextTruncated);
                var navigation=(ConsoleMultiNavigationCollection)typeof(SpellBookView).GetField("m_ConsoleMultiNavigationCollection",Cleanup2Fields).GetValue(view);
                navigation.SetCurrentEntityManual(header);
                if(!ReferenceEquals(ElementalConsoleBookPresentation.FocusedPreparation(view),context.Header))
                    throw new InvalidOperationException("Native console preparation header did not receive actual focus.");
                InvokeTeleportGamepadInput(view,"WantConfirm");for(int frame=0;frame<5;frame++)yield return 0;
                bool expanded=context.Expanded && context.Current && context.Projection.Any(value=>ReferenceEquals(value,context.Sources[0])) &&
                    owned.SpellBookKnownSpells.KnownSpells.SequenceEqual(authority) && before.Unchanged() && pins.IsRestored();
                Cleanup2Check("elemental-console-preparation-disclosure",visible && expanded,new JObject { ["headerVisibleFocusable"]=visible,
                    ["expandedThroughNativeConfirm"]=expanded,["authoritativeKnownVMReferencesExact"]=owned.SpellBookKnownSpells.KnownSpells.SequenceEqual(authority),
                    ["sourceCount"]=context.Sources.Length,["nativeSources"]=new JArray(context.Sources.Select(value=>value.SpellData.Blueprint.AssetGuid)),
                    ["budgetExact"]=before.Unchanged(),["pinsExact"]=pins.IsRestored(),["outcome"]=ElementalConsoleBookPresentation.Outcome },
                    "The real native console preparation list shows one focusable disclosure; native confirm expands original known VM/source references without changing any budget or pin.");
                if(!visible || !expanded)throw new InvalidOperationException("Actual console preparation disclosure contract failed.");
                foreach(int frame in screens.Capture("elementals-console-native-preparation",new JObject {
                    ["nativeText"]=new JArray(scroll.GetComponentsInChildren<TextMeshProUGUI>(false).Select(value=>value.text)),
                    ["sourceObjectsExact"]=owned.SpellBookKnownSpells.KnownSpells.SequenceEqual(authority),
                    ["scope"]="Real scene-bound console SpellBookView with native virtual rows and original known spell VM objects." },
                    ()=>context.Current && before.Unchanged() && pins.IsRestored() && Cleanup2NoNarrativeOverlay()))yield return frame;
            }
            finally
            {
                if(owned!=null && ReferenceEquals(typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service),owned))
                    Cleanup2CloseOwnedConsoleBook(service,owned,fixture.Caster);
            }
            for(int frame=0;frame<8;frame++)yield return 0;
            bool nativeBookCleared=typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service)==null;
            bool restored=nativeBookCleared && before.Unchanged() && pins.IsRestored() && GamePad.Instance.Layers.SequenceEqual(layers);
            Cleanup2Check("elemental-console-preparation-closed",restored,new JObject { ["budgetExact"]=before.Unchanged(),
                ["pinsExact"]=pins.IsRestored(),["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(layers),["nativeBookFieldCleared"]=nativeBookCleared },
                "Closing the exact native owned book removes its input layers and leaves every original source budget and pin unchanged.");
            if(!restored)throw new InvalidOperationException("Native console preparation closure unverified.");
        }

        private static void Cleanup2CloseOwnedConsoleBook(CharcaterServiceContext service,SpellBookVM owned,UnitEntityData caster)
        {
            var field=typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields);
            if(service==null || owned==null || field==null || !ReferenceEquals(field.GetValue(service),owned) ||
                !ReferenceEquals(owned.UnitEntity.Value,caster))
                throw new InvalidOperationException("Only the exact owned native spellbook may be closed.");
            // Installed Back0600512A only disposes; it leaves the service's
            // field populated. HideSpellbook06006255 disposes AND clears that
            // exact field. HandleHideSpellbook skips an already-disposed VM.
            var hide=typeof(CharcaterServiceContext).GetMethod("HideSpellbook",Cleanup2Fields,null,Type.EmptyTypes,null);
            if(hide==null || hide.ReturnType!=typeof(void))throw new InvalidOperationException("Native spellbook close contract unavailable.");
            hide.Invoke(service,null);
            if(field.GetValue(service)!=null)throw new InvalidOperationException("Native spellbook field remains after exact owned close.");
        }
    }
}
