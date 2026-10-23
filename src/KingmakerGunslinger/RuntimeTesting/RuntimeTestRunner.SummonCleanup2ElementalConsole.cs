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
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Blueprints;
using KingmakerGunslinger.Summoning;
using Newtonsoft.Json.Linq;
using TMPro;
using UniRx;
using UnityEngine;

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
            var layers=GamePad.Instance.Layers.OfType<InGameInputLayer>().Where(value=>ReferenceEquals(
                typeof(InGameInputLayer).GetField("inGameUiContext",Cleanup2Fields).GetValue(value),context)).ToArray();
            return model!=null && !model.IsDisposed && view!=null && view.gameObject.scene.IsValid() && view.gameObject.scene.isLoaded &&
                view.gameObject.activeInHierarchy && ReferenceEquals(ElementalConsoleBookPresentation.Model(view),model) &&
                layers.Length==1 && layers[0].LayerBinded.Value;
        }

        private IEnumerable<int> ReviewSummonCleanup2ElementalConsole(ExpandedSummoningCorrectionFixture fixture,Spellbook book)
        {
            if(!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters==null ||
                _request.Parameters.Count!=1 || (string)_request.Parameters["reviewPhase"]!="elementals-console")
                throw new InvalidOperationException("Exact automatic-exit elementals-console request required.");
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
                typeof(ActionBarVM).GetMethod("ShowSpellMenu",Cleanup2Fields).Invoke(bar,null);ownedSelector=bar.SelectorVm.Value;
                if(ownedSelector!=null)ownedSelectors.Add(ownedSelector);rememberSelector();
                for(int frame=0;frame<8;frame++)yield return 0;
                rememberSelector();
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
                        vm.Back();
                    });
                    bool untouched=player.Party.SequenceEqual(party) && selection.SelectedUnits.SequenceEqual(selected) &&
                        ReferenceEquals(selection.CurrentSelectUnitValue,current);
                    bool authority=ReferenceEquals(Game.Instance,game) && ReferenceEquals(UnitSelectionManager.Instance,selection) &&
                        ReferenceEquals(GamePad.Instance,gamepad) && bar.SelectorVm.Value==null &&
                        typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service)==null &&
                        game.UI.TooltipsController.ContextTooltipData==null && game.SelectedAbilityHandler?.Ability==null && (untouched || uiOwned());
                    bool selectionRestored=false;
                    if(!authority)errors.Add("Foreign native console owner; original party/selection is not overwritten.");
                    else
                    {
                        restore("owned-party-entry",()=>{if(added){player.PartyCharacters.Remove(fixture.Caster);player.InvalidateCharacterLists();player.UpdateCharacterLists();}});
                        restore("selection",()=>{selectionRestored=selectionState.Restore(fixture);if(!selectionRestored)throw new InvalidOperationException(selectionState.Outcome);});
                    }
                    bool restored=errors.Count==0 && player.Party.SequenceEqual(party) && selection.SelectedUnits.SequenceEqual(selected) &&
                        ReferenceEquals(selection.CurrentSelectUnitValue,current) && original.All(value=>value.Unchanged()) && originalPins.All(value=>value.IsRestored()) &&
                        GamePad.Instance.Layers.SequenceEqual(originalLayers) && selectionRestored;
                    Cleanup2Check("elemental-console-native-ui-restored",restored,new JObject { ["partyExact"]=player.Party.SequenceEqual(party),
                        ["selectionExact"]=selection.SelectedUnits.SequenceEqual(selected),["currentUnitExact"]=ReferenceEquals(selection.CurrentSelectUnitValue,current),
                        ["originalInputsExact"]=original.All(value=>value.Unchanged()),["pinsExact"]=originalPins.All(value=>value.IsRestored()),
                        ["nativeSelectionRestoration"]=selectionState.Describe(),
                        ["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(originalLayers),["restorationAuthority"]=authority,
                        ["ownedSelectorContexts"]=ownedContexts.Count,["ownedNativeSelectorTokens"]=ownedSelectors.Count,
                        ["locallyDispatchedChildReferences"]=ownedDispatched.Count,["errors"]=new JArray(errors) },
                        "All original native party/selection/source budgets/pins and controller input layers restore before exiting the owned Default scope.");
                    if(!restored)throw new InvalidOperationException("Original console UI restoration is unverified: "+string.Join("|",errors.ToArray()));
                }
                finally{try{screens.Dispose();}finally{mode.Dispose();}}
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
                if(owned!=null && ReferenceEquals(typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service),owned))owned.Back();
            }
            for(int frame=0;frame<8;frame++)yield return 0;
            bool restored=before.Unchanged() && pins.IsRestored() && GamePad.Instance.Layers.SequenceEqual(layers);
            Cleanup2Check("elemental-console-preparation-closed",restored,new JObject { ["budgetExact"]=before.Unchanged(),
                ["pinsExact"]=pins.IsRestored(),["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(layers) },
                "Closing the exact native owned book removes its input layers and leaves every original source budget and pin unchanged.");
            if(!restored)throw new InvalidOperationException("Native console preparation closure unverified.");
        }
    }
}
