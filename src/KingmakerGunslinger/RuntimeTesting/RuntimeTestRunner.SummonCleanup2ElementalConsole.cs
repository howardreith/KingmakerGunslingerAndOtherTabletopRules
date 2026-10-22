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
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private bool Cleanup2ConsoleNativeUiReady()
        {
            if(!IsSummonCleanup2Review || _request.Parameters==null ||
                (string)_request.Parameters["reviewPhase"]!="elementals-console" || !Game.Instance.IsControllerGamepad || GamePad.Instance==null)return false;
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
            var original=party.Select(value=>new Cleanup2TooltipInputs(value)).ToArray();
            var originalPins=party.Select(value=>new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            var originalLayers=GamePad.Instance.Layers.ToArray();bool added=false;
            var mode=Cleanup2EnterNativeDefault(fixture,"elementals-console");
            var screens=new NativeIconScreenEvidence(_request);var watch=Stopwatch.StartNew();
            var hooks=HarmonyInstance.Create("kmg.cleanup2.elementals-console."+_request.RunId);
            ActionBarSelectorVm ownedSelector=null;
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
                foreach(var parent in StandaloneElementalPresentationPolicy.All.Where(value=>value.IsParent))
                    PrepareExpandedSummoningPlayerPathSpell(book,Cleanup2ElementalAbility(fixture,parent.Guid),
                        Cleanup2ElementalAbility(fixture,parent.Guid),parent.CanonicalTier);
                selection.SelectUnit(fixture.Caster.View,true,true,false);
                for(int frame=0;frame<12;frame++)yield return 0;
                if(!ReferenceEquals(bar.Unit.Value,fixture.Caster) || !ReferenceEquals(selection.CurrentSelectUnitValue,fixture.Caster))
                    throw new InvalidOperationException("Actual console selection did not bind the owned caster.");
                var input=new Cleanup2TooltipInputs(fixture.Caster);var pins=new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
                var layers=GamePad.Instance.Layers.ToArray();
                typeof(ActionBarVM).GetMethod("ShowSpellMenu",Cleanup2Fields).Invoke(bar,null);ownedSelector=bar.SelectorVm.Value;
                for(int frame=0;frame<8;frame++)yield return 0;
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
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Rank==0 && value.Source==null,false))yield return frame;
                ownedSelector=bar.SelectorVm.Value;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Rank==2 && value.Creature==null,false))yield return frame;
                ownedSelector=bar.SelectorVm.Value;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Creature=="small-fire-elemental" && value.Source==null,false))yield return frame;
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

                typeof(ActionBarVM).GetMethod("ShowSpellMenu",Cleanup2Fields).Invoke(bar,null);ownedSelector=bar.SelectorVm.Value;for(int frame=0;frame<6;frame++)yield return 0;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Rank==0 && value.Source==null,false))yield return frame;
                ownedSelector=bar.SelectorVm.Value;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Rank==2 && value.Creature==null,false))yield return frame;
                ownedSelector=bar.SelectorVm.Value;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>value.Creature=="small-fire-elemental" && value.Source==null,false))yield return frame;
                ownedSelector=bar.SelectorVm.Value;context=ElementalSourceChooserConsole.Context(ownedSelector);
                var source=context.Sources.Single(value=>value.CreatureKey=="small-fire-elemental" && ReferenceEquals(value.Book,book));
                int before=CountAvailableExpandedSummoningSlots(book,2);string resources=Cleanup2ElementalResources(fixture.Caster);
                ExpandedSummoningRuleCapture.Clear();ExpandedSummoningPlayerPathEvents.Clear();
                _expandedSummoningPlayerPathRuleCastCount=0;_expandedSummoningPlayerPathSpawnActionCount=0;
                foreach(int frame in Cleanup2ConsoleChoose(bar,value=>ReferenceEquals(value.Source,source),true))yield return frame;
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
                try
                {
                    _expandedSummoningPlayerPathCaptureActive=false;hooks.UnpatchAll(hooks.Id);
                    if(game.SelectedAbilityHandler?.Ability!=null && ReferenceEquals(game.SelectedAbilityHandler.Ability,ElementalSourceChooserRuntime.LastDispatched))
                        game.SelectedAbilityHandler.DropAbility();
                    if(ownedSelector!=null && ReferenceEquals(bar.SelectorVm.Value,ownedSelector))
                        typeof(ActionBarVM).GetMethod("HideSelectorVm",Cleanup2Fields).Invoke(bar,null);
                    if(typeof(CharcaterServiceContext).GetField("m_SpellBookVM",Cleanup2Fields).GetValue(service) is SpellBookVM vm &&
                        ReferenceEquals(vm.UnitEntity.Value,fixture.Caster))vm.Back();
                    if(selection.SelectedUnits.All(value=>ReferenceEquals(value,fixture.Caster) || selected.Any(prior=>ReferenceEquals(prior,value))))
                    {
                        typeof(UnitSelectionManager).GetMethod("Clear",Cleanup2Fields).Invoke(selection,null);
                        foreach(var unit in selected)if(unit?.View!=null)selection.SelectUnit(unit.View,true,false,false);
                        if(current!=null && !ReferenceEquals(selection.CurrentSelectUnitValue,current))selection.ForceSelectUnit(current,false,false);
                    }
                    if(added){player.PartyCharacters.Remove(fixture.Caster);player.InvalidateCharacterLists();player.UpdateCharacterLists();}
                    bool restored=player.Party.SequenceEqual(party) && selection.SelectedUnits.SequenceEqual(selected) &&
                        ReferenceEquals(selection.CurrentSelectUnitValue,current) && original.All(value=>value.Unchanged()) && originalPins.All(value=>value.IsRestored()) &&
                        GamePad.Instance.Layers.SequenceEqual(originalLayers);
                    Cleanup2Check("elemental-console-native-ui-restored",restored,new JObject { ["partyExact"]=player.Party.SequenceEqual(party),
                        ["selectionExact"]=selection.SelectedUnits.SequenceEqual(selected),["currentUnitExact"]=ReferenceEquals(selection.CurrentSelectUnitValue,current),
                        ["originalInputsExact"]=original.All(value=>value.Unchanged()),["pinsExact"]=originalPins.All(value=>value.IsRestored()),
                        ["inputLayersExact"]=GamePad.Instance.Layers.SequenceEqual(originalLayers) },
                        "All original native party/selection/source budgets/pins and controller input layers restore before exiting the owned Default scope.");
                    if(!restored)throw new InvalidOperationException("Original console UI restoration is unverified.");
                }
                finally{try{screens.Dispose();}finally{mode.Dispose();}}
            }
        }

        private IEnumerable<int> Cleanup2ConsoleChoose(ActionBarVM owner,Func<ElementalConsoleNode,bool> predicate,bool use)
        {
            var model=owner.SelectorVm.Value;var context=ElementalSourceChooserConsole.Context(model);
            if(context==null || !context.Current)throw new InvalidOperationException("Exact native console context absent before input.");
            var item=model.ItemVms.Single(value=>value.MechanicActionBarSlot is ElementalConsoleNode node && predicate(node));
            var view=context.BoundView;
            var row=view.GetComponentsInChildren<ActionBarSelectorItemView>(false).Single(value=>ReferenceEquals(ElementalConsoleBookPresentation.Model(value),item));
            var navigation=(ConsoleNavigationBehavior)typeof(ActionBarSelectorView).GetField("m_NavigationBehavior",Cleanup2Fields).GetValue(view);
            if(!navigation.SelectEntityManually(row) || !ReferenceEquals(navigation.GetCurrentSelectedEntity(),row))
                throw new InvalidOperationException("Actual native console row did not accept navigation focus.");
            InvokeTeleportGamepadInput(view,use?"<GetInputLayer>b__11_2":"<GetInputLayer>b__11_0");
            for(int frame=0;frame<5;frame++)yield return 0;
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
