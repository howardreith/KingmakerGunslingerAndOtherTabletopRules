using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Group;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics.Actions;
using KingmakerGunslinger.Summoning;
using KingmakerGunslinger.Blueprints;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KingmakerGunslinger.RuntimeTesting
{
    internal sealed partial class RuntimeTestRunner
    {
        private Cleanup2NativeDefaultScope _cleanup2NativeDefaultOwner;
        private bool Cleanup2NativeDefaultPending { get { return _cleanup2NativeDefaultOwner!=null; } }
        // This lease calls the actual GameModesFactory through StartMode. It
        // never installs an empty GameMode or assigns a synthetic controller.
        // The manual fixture clock stays suspended until the owned native mode
        // has been positively removed. An ownership failure leaves it suspended.
        private sealed class Cleanup2NativeDefaultScope : IDisposable
        {
            private readonly RuntimeTestRunner _runner;
            private readonly Game _game;
            private readonly GameMode[] _before;
            private readonly int[] _counts;
            private readonly object[] _controllers;
            private object[] _activeControllers;
            private int[] _activeCounts;
            private readonly bool _waiting, _paused;
            private readonly string _purpose;
            private GameMode _owned;
            private IDisposable _nativeTicks;
            private readonly int _manualPulses;
            private readonly JObject _gateBefore;
            private bool _closed;
            private static readonly FieldInfo Modes = typeof(Game).GetField("m_GameModes",Cleanup2Fields);
            private static readonly FieldInfo Counts = typeof(Game).GetField("m_ModesCount",Cleanup2Fields);
            private static readonly string[] PublicControllers = { "ClickEventsController", "SelectedAbilityHandler",
                "PlaceRestMarkerHandler", "InteractionHighlightController", "CombatEngagementController",
                "HandsEquipmentController", "CameraController" };
            private static object[] Controllers(Game game)
            { return PublicControllers.Select(name => typeof(Game).GetProperty(name,Cleanup2Fields).GetValue(game,null)).ToArray(); }
            private static bool ImmediateBoundary(Game game)
            {
                return !(bool)typeof(Game).GetField("m_GameModeTicking",Cleanup2Fields).GetValue(game) &&
                    !(bool)typeof(Game).GetField("m_SwitchingModes",Cleanup2Fields).GetValue(game) &&
                    ((System.Collections.ICollection)typeof(Game).GetField("m_BeforeTickActions",Cleanup2Fields).GetValue(game)).Count==0;
            }
            private Stack<GameMode> Stack { get { return (Stack<GameMode>)Modes.GetValue(_game); } }
            internal bool Current { get { return !_closed && _game.CurrentMode==GameModeType.Default &&
                (_owned==null ? Stack.SequenceEqual(_before) : Stack.Count==_before.Length+1 &&
                    ReferenceEquals(Stack.Peek(),_owned) && Stack.Skip(1).SequenceEqual(_before)) &&
                _activeCounts!=null && ((int[])Counts.GetValue(_game)).SequenceEqual(_activeCounts) &&
                _activeControllers!=null && Controllers(_game).Select((value,index)=>
                    ReferenceEquals(value,_activeControllers[index])).All(value=>value) &&
                _game.SelectedAbilityHandler!=null && _game.ClickEventsController!=null &&
                ReferenceEquals(_game.ClickEventsController,_game.DefaultPointerController) &&
                Stack.Count!=0 && Stack.Peek().Type==GameModeType.Default &&
                ReferenceEquals(Stack.Peek().GetController<Kingmaker.Controllers.Clicks.PointerController>(),_game.DefaultPointerController); } }
            internal JObject Describe()
            {
                object[] actualControllers=Controllers(_game);
                int[] actualCounts=Counts.GetValue(_game) as int[] ?? new int[0];
                bool stackExact=_owned==null ? Stack.SequenceEqual(_before) : Stack.Count==_before.Length+1 &&
                    ReferenceEquals(Stack.Peek(),_owned) && Stack.Skip(1).SequenceEqual(_before);
                return new JObject { ["purpose"]=_purpose,["nativeFactoryModeOwned"]=_owned!=null,
                ["existingNativeDefaultBorrowed"]=_owned==null,
                ["current"] = Current,["closed"]=_closed,["mode"]=_game.CurrentMode.ToString(),
                ["stackCount"]=Stack.Count,["originalStackCount"]=_before.Length,
                ["manualClockSuspended"]=_runner._sprint25WaitingForUnityAppearance,
                ["manualPulsesBefore"]=_manualPulses,["manualPulsesCurrent"]=_runner._sprint25Clock.Pulses,
                ["tickGateBefore"]=_gateBefore,["tickGateCurrent"]=_runner._sprint25Clock.NativeDefaultGateSnapshot(),
                ["selectedAbilityHandler"]=_game.SelectedAbilityHandler?.GetType().FullName,
                ["pointerController"]=_game.ClickEventsController?.GetType().FullName,
                ["ownershipPredicates"]=new JObject {
                    ["notClosed"]=!_closed,["modeIsNativeDefault"]=_game.CurrentMode==GameModeType.Default,
                    ["exactModeStackReferences"]=stackExact,
                    ["exactActiveModeCounts"]=_activeCounts!=null && actualCounts.SequenceEqual(_activeCounts),
                    ["exactControllerReferences"]=_activeControllers!=null && actualControllers.Select((value,index)=>
                        ReferenceEquals(value,_activeControllers[index])).All(value=>value),
                    ["selectedAbilityAbsent"]=_game.SelectedAbilityHandler!=null && _game.SelectedAbilityHandler.Ability==null,
                    ["immediateBoundary"]=ImmediateBoundary(_game),
                    ["factoryDefaultPointerExact"]=_game.DefaultPointerController!=null &&
                        ReferenceEquals(_game.ClickEventsController,_game.DefaultPointerController) && Stack.Count!=0 &&
                        ReferenceEquals(Stack.Peek().GetController<Kingmaker.Controllers.Clicks.PointerController>(),_game.DefaultPointerController),
                    ["gameModeTicking"]=(bool)typeof(Game).GetField("m_GameModeTicking",Cleanup2Fields).GetValue(_game),
                    ["switchingModes"]=(bool)typeof(Game).GetField("m_SwitchingModes",Cleanup2Fields).GetValue(_game),
                    ["pendingBeforeTickActions"]=((System.Collections.ICollection)typeof(Game).GetField("m_BeforeTickActions",Cleanup2Fields).GetValue(_game)).Count,
                    ["selectedAbility"]=_game.SelectedAbilityHandler?.Ability?.Blueprint.AssetGuid },
                ["activeModeCounts"]=new JArray(actualCounts),
                ["expectedActiveModeCounts"]=_activeCounts==null ? (JToken)JValue.CreateNull() : new JArray(_activeCounts),
                ["controllerReferences"]=new JArray(PublicControllers.Select((name,index)=>new JObject {
                    ["name"]=name,["expectedInstance"]=_activeControllers==null || _activeControllers[index]==null ? 0 :
                        System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(_activeControllers[index]),
                    ["actualInstance"]=actualControllers[index]==null ? 0 : System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(actualControllers[index]),
                    ["same"]=_activeControllers!=null && ReferenceEquals(actualControllers[index],_activeControllers[index]) })) };
            }
            internal Cleanup2NativeDefaultScope(RuntimeTestRunner runner,ExpandedSummoningCorrectionFixture fixture,string purpose)
            {
                _runner=runner;_game=Game.Instance;_purpose=purpose;
                var admission=new JObject {
                    ["exactReview"]=runner.IsSummonCleanup2Review && runner._request.ExitAfterCompletion,
                    ["ownedFixture"]=fixture?.Caster!=null && fixture.Caster.Blueprint==fixture.CasterBlueprint &&
                        !fixture.UnitsBefore.Any(value=>ReferenceEquals(value,fixture.Caster)),
                    ["saveFree"]=runner._workingSaveSmoke==null || !runner._workingSaveSmoke.WriteObserved,
                    ["narrativeAbsent"]=runner.Cleanup2NoNarrativeOverlay(),
                    ["inputModeAdmitted"]=!_game.IsControllerGamepad || runner.Cleanup2ConsoleNativeUiReady(),
                    ["nativeModeAdmitted"]=_game.CurrentMode==GameModeType.Dialog || _game.CurrentMode==GameModeType.Default,
                    ["noSelectedAbility"]=_game.SelectedAbilityHandler?.Ability==null,
                    ["immediateBoundary"]=ImmediateBoundary(_game) };
                bool admitted=admission.Properties().All(value=>(bool)value.Value);
                admission["mode"]=_game.CurrentMode.ToString();
                admission["controller"]=_game.ControllerMode.ToString();
                if (_game.IsControllerGamepad) admission["consoleInputEvidence"]=runner._cleanup2ConsoleReadiness;
                runner.Cleanup2Check("native-default-admission-"+purpose,admitted,admission,
                    "Only the exact save-free fixture may borrow existing native Default or push native factory Default from Dialog at an idle boundary.");
                if(!admitted)
                    throw new InvalidOperationException("Exact idle save-free owned fixture and immediate native mode boundary required.");
                _before=Stack.ToArray();_counts=((int[])Counts.GetValue(_game)).ToArray();_controllers=Controllers(_game);
                _waiting=runner._sprint25WaitingForUnityAppearance;_paused=_game.IsPaused;
                _manualPulses=runner._sprint25Clock.Pulses;_gateBefore=runner._sprint25Clock.NativeDefaultGateSnapshot();
                if(runner._cleanup2NativeDefaultOwner!=null)throw new InvalidOperationException("An owned Default scope already exists.");
                runner._cleanup2NativeDefaultOwner=this;
                runner._sprint25WaitingForUnityAppearance=true;
                try
                {
                    if(_game.CurrentMode!=GameModeType.Default)
                    {
                        _game.StartMode(GameModeType.Default);
                        if(Stack.Count==_before.Length+1 && Stack.Skip(1).SequenceEqual(_before)) _owned=Stack.Peek();
                        int[] actual=(int[])Counts.GetValue(_game);
                        if(_owned==null || actual.Length!=_counts.Length || !actual.Select((count,index)=>
                            count==_counts[index]+(index==(int)GameModeType.Default?1:0)).All(value=>value))
                            throw new InvalidOperationException("Native Default factory push did not preserve the exact prior stack/counts.");
                    }
                    _activeCounts=((int[])Counts.GetValue(_game)).ToArray();
                    _activeControllers=Controllers(_game);
                    if(!Current) throw new InvalidOperationException("Native Default pointer/selected-ability controllers are absent.");
                    _nativeTicks=runner._sprint25Clock.AllowOwnedNativeDefault(()=>Current);
                    _game.IsPaused=false;
                    runner.Cleanup2Check("native-default-enter-"+purpose,true,Describe(),
                        "The exact existing or factory-created native Default mode and real input controllers own progression; the manual fixture clock is suspended.");
                }
                catch (Exception setupError)
                {
                    try { Dispose(); }
                    catch (Exception cleanupError) { throw new AggregateException("Native Default setup and exact restoration both failed.",setupError,cleanupError); }
                    throw;
                }
            }
            public void Dispose()
            {
                if(_closed)return;
                if(!Current || _game.SelectedAbilityHandler.Ability!=null || !ImmediateBoundary(_game))
                {
                    _runner.Cleanup2Check("native-default-disposal-refused-"+_purpose,false,Describe(),
                        "Every exact stack/count/controller/target/boundary predicate must still hold before releasing the owned native Default lease; refusal preserves it.");
                    throw new InvalidOperationException("Owned native Default scope changed or still targets an ability; foreign mode/input is not overwritten and manual clock remains suspended.");
                }
                if(_owned!=null)_game.StopMode(GameModeType.Default);
                bool restored=Stack.SequenceEqual(_before) && ((int[])Counts.GetValue(_game)).SequenceEqual(_counts) &&
                    Controllers(_game).Select((value,index)=>ReferenceEquals(value,_controllers[index])).All(value=>value);
                if(!restored)throw new InvalidOperationException("Native mode stack/count/controller restoration is unverified; manual clock remains suspended.");
                if(_runner._sprint25Clock.Pulses!=_manualPulses)
                    throw new InvalidOperationException("Manual fixture clock advanced during native Default ownership.");
                _nativeTicks?.Dispose();_nativeTicks=null;
                _game.IsPaused=_paused;_closed=true;_runner._cleanup2NativeDefaultOwner=null;_runner._sprint25WaitingForUnityAppearance=_waiting;
                _runner.Cleanup2Check("native-default-restored-"+_purpose,true,Describe(),
                    "The existing native mode was retained, or StopMode removed only the exact owned push; every prior mode/count/public controller matches before resuming the fixture clock.");
            }
        }

        private Cleanup2NativeDefaultScope Cleanup2EnterNativeDefault(ExpandedSummoningCorrectionFixture fixture,string purpose)
        { return new Cleanup2NativeDefaultScope(this,fixture,purpose); }

        private IEnumerable<int> ReviewSummonCleanup2ElementalPresentation(ExpandedSummoningCorrectionFixture fixture,Spellbook book)
        {
            if(!IsSummonCleanup2Review || !_request.ExitAfterCompletion || _request.Parameters==null ||
                _request.Parameters.Count!=1 || (string)_request.Parameters["reviewPhase"]!="elementals-presentation")
                throw new InvalidOperationException("Exact automatic-exit elementals-presentation request required.");
            var game=Game.Instance;var player=game.Player;var ui=game.UI;
            var bar=ActionBarManager.Instance;var group=GroupController.Instance;
            if(bar?.Group==null || group==null || ui?.SelectionManagerPC==null || game.IsControllerGamepad)
                throw new InvalidOperationException("Actual desktop native action bar/group/selection required.");
            var party=player.Party.ToArray();var selected=ui.SelectionManagerPC.SelectedUnits.ToArray();
            var selectedGroup=group.GetCurrentCharacter();var groups=bar.Group.GroupElements.ToArray();
            bool[] toggles=groups.Select(value=>value.ToggleState).ToArray();
            if(toggles.Any(value=>value))throw new InvalidOperationException("Closed native groups required at intake.");
            var originalInputs=party.Select(value=>new Cleanup2TooltipInputs(value)).ToArray();
            var bars=party.Select(value=>new TeleportationUiSettingsFixture(value.UISettings)).ToArray();
            bool added=false;var watch=Stopwatch.StartNew();
            var hooks=HarmonyInstance.Create("kmg.cleanup2.elemental-presentation."+_request.RunId);
            var nativeMode=Cleanup2EnterNativeDefault(fixture,"elemental-presentation");
            var screens=new NativeIconScreenEvidence(_request);
            UnitEntityData[] ownedParty=null;
            var ownedSessions=new HashSet<ElementalChooserSession>();
            var ownedDispatched=new HashSet<AbilityData>();
            Func<bool> fixtureUiOwned=()=>ownedParty!=null && player.Party.SequenceEqual(ownedParty) &&
                ReferenceEquals(Game.Instance,game) && ReferenceEquals(game.UI,ui) && ReferenceEquals(ActionBarManager.Instance,bar) &&
                ReferenceEquals(GroupController.Instance,group) && bar.Group.GroupElements.SequenceEqual(groups) &&
                ReferenceEquals(group.GetCurrentCharacter(),fixture.Caster) && ui.SelectionManagerPC.SelectedUnits.Count==1 &&
                ReferenceEquals(ui.SelectionManagerPC.SelectedUnits.First(),fixture.Caster) &&
                ui.TooltipsController.ContextTooltipData==null && game.SelectedAbilityHandler?.Ability==null;
            try
            {
                const BindingFlags statics=BindingFlags.Static|BindingFlags.NonPublic;
                hooks.Patch(typeof(RuleCastSpell).GetMethod("OnTrigger"),null,
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathRuleCastPostfix",statics)),null);
                hooks.Patch(typeof(ContextActionSpawnMonster).GetMethod("RunAction"),
                    new HarmonyMethod(typeof(RuntimeTestRunner).GetMethod("ExpandedSummoningPlayerPathSpawnPrefix",statics)),null,null);
                _expandedSummoningPlayerPathCaptureActive=true;
                if(!party.Contains(fixture.Caster))
                { player.PartyCharacters.Add(fixture.Caster);added=true;player.InvalidateCharacterLists();player.UpdateCharacterLists(); }
                // Fixture grants are explicit. No natural class-learning claim.
                foreach(var parent in StandaloneElementalPresentationPolicy.All.Where(value=>value.IsParent))
                    PrepareExpandedSummoningPlayerPathSpell(book,Cleanup2ElementalAbility(fixture,parent.Guid),
                        Cleanup2ElementalAbility(fixture,parent.Guid),parent.CanonicalTier);
                ui.SelectionManagerPC.SelectUnit(fixture.Caster.View,true,true,false);group.SelectUnit(fixture.Caster);
                ownedParty=player.Party.ToArray();
                for(int frame=0;frame<12;frame++)yield return 0;
                var spellGroup=groups.Single(value=>(ActionBarSlotType)typeof(ActionBarGroupElement)
                    .GetField("SlotType",Cleanup2Fields).GetValue(value)==ActionBarSlotType.Spell);
                if(!spellGroup.ToggleState)spellGroup.OnClick();
                for(int frame=0;frame<12;frame++)yield return 0;
                var session=ElementalSourceChooserRuntime.Session(spellGroup);
                if(session==null || !session.Current)throw new InvalidOperationException("Actual native FillSlots did not publish a current chooser session: "+ElementalSourceChooserRuntime.Outcome);
                ownedSessions.Add(session);
                var coverage=new JArray();
                foreach(int rank in ElementalSourceChooserPolicy.Ranks)
                foreach(string key in ElementalSourceChooserPolicy.Rank(rank).CreatureKeys)
                {
                    var exact=session.Sources.Where(value=>value.Rank==rank && value.CreatureKey==key && ReferenceEquals(value.Book,book) &&
                        ElementalSourceChooserPolicy.Parent(value.Source.Blueprint.AssetGuid)!=null).ToArray();
                    bool linked=exact.Length==1 && ReferenceEquals(exact[0].Child.ConvertedFrom,exact[0].Source) &&
                        ReferenceEquals(exact[0].Child.Spellbook,book) && exact[0].Preparation!=null;
                    coverage.Add(new JObject { ["rank"]=rank,["key"]=key,["count"]=exact.Length,["linked"]=linked,
                        ["child"]=exact.FirstOrDefault()?.Child.Blueprint.AssetGuid,["parent"]=exact.FirstOrDefault()?.Source.Blueprint.AssetGuid });
                }
                bool covered=coverage.Count==24 && coverage.Cast<JObject>().All(value=>(bool)value["linked"]);
                Cleanup2Check("elemental-chooser-native-24-source-bindings",covered,new JObject { ["rows"]=coverage,
                    ["outcome"]=ElementalSourceChooserRuntime.Outcome,["exceptions"]=new JArray(session.Exceptions),
                    ["scope"]="Actual native prepared source mechanisms and native variant factory children; fixture-granted Wizard access, not natural learning or 24 cast claims." },
                    "All six native ranks and four elements retain exact original source/book/preparation/child references in the actual native tab session.");
                if(!covered)throw new InvalidOperationException("Native elemental binding coverage incomplete.");
                var levels=session.Levels;levels.OnClick(2);
                for(int frame=0;frame<8;frame++)yield return 0;
                var launcher=spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value=>
                    value.gameObject.activeInHierarchy && value.MechanicSlot is ElementalChooserLauncher);
                var settings=fixture.Caster.UISettings;
                var savedSlots=settings.Slots;
                if(savedSlots==null || savedSlots.Length<2)throw new InvalidOperationException("Actual native action-bar slot array is unavailable for persistence boundary proof.");
                var savedContents=savedSlots.ToArray();
                var saveBoundary=typeof(UnitUISettings).GetMethod("SetSlot",new[]{typeof(MechanicActionBarSlot),typeof(int)});
                var pinPatches=_context.Harmony.GetPatchInfo(saveBoundary);
                bool pinGuard=pinPatches!=null && pinPatches.Prefixes.Count(value=>value.owner==_context.ModId &&
                    value.patch.DeclaringType==typeof(ElementalChooserPersistencePatch))==1;
                int refusals=ElementalChooserPersistencePatch.RefusedPins;
                MechanicActionBarSlot ownedNativePin=null;
                try
                {
                    // This is the exact native persistence call reached by
                    // ActionBarGroupSlot.EndDrag -> ActionBarIndexSlot.TryToSet.
                    // It uses the actual rendered launcher's mechanism.
                    settings.SetSlot(launcher.MechanicSlot,0);
                    bool rejected=pinGuard && ElementalChooserPersistencePatch.RefusedPins==refusals+1 &&
                        ReferenceEquals(settings.Slots,savedSlots) && settings.Slots.SequenceEqual(savedContents);
                    Cleanup2Check("elemental-transient-launcher-native-pin-refused",rejected,new JObject {
                        ["actualRenderedMechanism"]=launcher.MechanicSlot.GetType().FullName,
                        ["nativeBoundaryToken"]="0x06002fff",["nativeBoundaryBytes"]=saveBoundary.GetMethodBody().GetILAsByteArray().Length,
                        ["ownedPrefixInstalled"]=pinGuard,["refusalCountDelta"]=ElementalChooserPersistencePatch.RefusedPins-refusals,
                        ["slotArrayReferenceExact"]=ReferenceEquals(settings.Slots,savedSlots),
                        ["allSerializedContentsExact"]=ReferenceEquals(settings.Slots,savedSlots) && settings.Slots.SequenceEqual(savedContents),
                        ["scope"]="Actual rendered transient launcher submitted to the actual native save boundary. This is a persistence-boundary proof, not a simulated drag screenshot." },
                        "The real transient launcher cannot be stored by native SetSlot; the complete original serialized slot array and every content reference remain exact.");
                    if(!rejected)throw new InvalidOperationException("Transient launcher native persistence refusal failed.");
                    var nativeChild=session.Sources.First(value=>value.CreatureKey=="small-fire-elemental" && ReferenceEquals(value.Book,book));
                    var nativePin=new MechanicActionBarSlotSpontaneusConvertedSpell {Unit=fixture.Caster,Spell=nativeChild.Child};
                    ownedNativePin=nativePin;
                    settings.SetSlot(nativePin,1);
                    bool nativeAllowed=ReferenceEquals(settings.Slots,savedSlots) && ReferenceEquals(savedSlots[1],nativePin) &&
                        ReferenceEquals(((MechanicActionBarSlotSpontaneusConvertedSpell)savedSlots[1]).Spell,nativeChild.Child) &&
                        savedSlots.Where((value,index)=>index!=1).SequenceEqual(savedContents.Where((value,index)=>index!=1));
                    Cleanup2Check("elemental-original-native-source-pin-preserved",nativeAllowed,new JObject {
                        ["nativeConvertedMechanismStored"]=ReferenceEquals(savedSlots[1],nativePin),
                        ["exactOriginalChildReference"]=nativeAllowed,["child"]=nativeChild.Child.Blueprint.AssetGuid,
                        ["scope"]="Disposable fixture only. Original native converted-source SetSlot behavior is preserved; this exact temporary native pin is restored immediately." },
                        "The transient-only guard still allows a real native converted source and its exact original AbilityData to be pinned.");
                    if(!nativeAllowed)throw new InvalidOperationException("Original native source pin was altered.");
                }
                finally
                {
                    if(ReferenceEquals(settings.Slots,savedSlots))
                        for(int index=0;index<2;index++)
                        {
                            var current=savedSlots[index];
                            if(ReferenceEquals(current,launcher.MechanicSlot) || ownedNativePin!=null && ReferenceEquals(current,ownedNativePin))
                                settings.SetSlot(savedContents[index],index);
                        }
                }
                var beforeCancel=new Cleanup2TooltipInputs(fixture.Caster);var pins=new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
                var keyboard=game.Keyboard;int keyboardCount=keyboard.Disabled.GuardCount;
                launcher.OnClick();for(int frame=0;frame<4;frame++)yield return 0;
                if(!ElementalSourceChooserView.IsOpen)throw new InvalidOperationException("Actual launcher click did not open chooser.");
                bool keyboardOwned=ReferenceEquals(game.Keyboard,keyboard) && ElementalSourceChooserView.KeyboardGuardHeld &&
                    keyboard.Disabled.GuardCount==keyboardCount+1;
                ElementalSourceChooserView.ReviewChoose(2,"small-fire-elemental",null,false);
                foreach(int frame in screens.Capture("elementals-common-chooser-standalone-small-fire",new JObject {
                    ["nativeInputMode"]=nativeMode.Describe(),["renderedText"]=new JArray(ElementalSourceChooserView.RenderedText),
                    ["source"]=session.Sources.Single(value=>value.CreatureKey=="small-fire-elemental" && ReferenceEquals(value.Book,book)).Source.Blueprint.AssetGuid,
                    ["panelInstance"]=ElementalSourceChooserView.RenderedPanel?.GetInstanceID(),
                    ["scope"]="Actual production chooser on the native Canvas with original native source; this capture alone does not prove debit." },
                    ()=>nativeMode.Current && session.Current && ElementalSourceChooserView.IsOpen && Cleanup2NoNarrativeOverlay() && beforeCancel.Unchanged()))yield return frame;
                ElementalSourceChooserView.CloseOwned("review-cancel-no-spend");
                Cleanup2Check("elemental-chooser-native-cancel",beforeCancel.Unchanged() && pins.IsRestored() && keyboardOwned &&
                    ReferenceEquals(game.Keyboard,keyboard) && keyboard.Disabled.GuardCount==keyboardCount && ElementalSourceChooserView.LastKeyboardRestored,
                    new JObject { ["inputs"]=beforeCancel.Describe(),["pinsExact"]=pins.IsRestored(),
                        ["ownedKeyboardCountWhileOpen"]=keyboardOwned,["keyboardCountBefore"]=keyboardCount,
                        ["keyboardCountAfter"]=keyboard.Disabled.GuardCount,["keyboardOutcome"]=ElementalSourceChooserView.KeyboardOutcome },
                    "Opening and selecting the actual chooser then cancelling changes no slots/resources/commands/pins and releases exactly its native keyboard guard count.");
                var source=session.Sources.Single(value=>value.CreatureKey=="small-fire-elemental" && ReferenceEquals(value.Book,book) &&
                    ElementalSourceChooserPolicy.Parent(value.Source.Blueprint.AssetGuid)!=null);
                int slots=CountAvailableExpandedSummoningSlots(book,2);
                ExpandedSummoningRuleCapture.Clear();ExpandedSummoningPlayerPathEvents.Clear();
                _expandedSummoningPlayerPathRuleCastCount=0;_expandedSummoningPlayerPathSpawnActionCount=0;
                launcher.OnClick();for(int frame=0;frame<3;frame++)yield return 0;
                if(!fixtureUiOwned() || !session.Current || !ReferenceEquals(ElementalSourceChooserView.CurrentSession,session))
                    throw new InvalidOperationException("Original source chooser ownership changed before dispatch.");
                ownedDispatched.Add(source.Child);
                ElementalSourceChooserView.ReviewChoose(2,source.CreatureKey,source,true);
                bool handoff=ReferenceEquals(game.SelectedAbilityHandler.Ability,source.Child) &&
                    ReferenceEquals(ElementalSourceChooserRuntime.LastDispatched,source.Child) && !ElementalSourceChooserView.IsOpen;
                Cleanup2Check("elemental-chooser-native-target-handoff",handoff,new JObject {
                    ["source"]=source.Source.Blueprint.AssetGuid,["child"]=source.Child.Blueprint.AssetGuid,
                    ["selectedExactChild"]=handoff,["dataLinks"]=DescribeExpandedSummoningAbilityData(source.Child),
                    ["sourceAvailable"]=source.Child.GetAvailableForCastCount() },
                    "The actual owned Cast button dispatches the native converted slot and the real targeting handler holds the exact original native child.");
                if(!handoff)throw new InvalidOperationException("Native targeting handoff failed: "+ElementalSourceChooserRuntime.Outcome);
                game.SelectedAbilityHandler.OnClick(null,fixture.Caster.Position+new Vector3(2,0,2),0);
                var castWatch=Stopwatch.StartNew();
                while(castWatch.Elapsed.TotalSeconds<25 && watch.Elapsed.TotalSeconds<360 &&
                    (_expandedSummoningPlayerPathRuleCastCount==0 || !fixture.Caster.Commands.Empty))
                {
                    if(!nativeMode.Current || !Cleanup2NoNarrativeOverlay())throw new InvalidOperationException("Native input/mode/narrative owner changed during cast.");
                    game.IsPaused=false;yield return 0;
                }
                var summoned=ExpandedSummoningRuleCapture.Where(value=>value!=null && ExpandedSummoningPlayerPathUnitExact(value,fixture.Caster)).Distinct().ToArray();
                foreach(var unit in summoned)if(!fixture.Created.Contains(unit))fixture.Created.Add(unit);
                var expectedSmall=SummonSignatureReader.Read(source.Child.Blueprint).Spawns().ToArray();
                bool cast=_expandedSummoningPlayerPathRuleCastCount==1 && _expandedSummoningPlayerPathSpawnActionCount==1 &&
                    expectedSmall.Length==1 && summoned.Length==1 && summoned[0].Blueprint.AssetGuid==expectedSmall[0].Unit &&
                    CountAvailableExpandedSummoningSlots(book,2)==slots-1 && fixture.Caster.Commands.Empty &&
                    ReferenceEquals(source.Child.ConvertedFrom,source.Source) && ReferenceEquals(source.Child.Spellbook,book) && pins.IsRestored();
                Cleanup2Check("elemental-chooser-native-small-fire-cast",cast,new JObject {
                    ["ruleCasts"]=_expandedSummoningPlayerPathRuleCastCount,["spawnActions"]=_expandedSummoningPlayerPathSpawnActionCount,
                    ["slotBefore"]=slots,["slotAfter"]=CountAvailableExpandedSummoningSlots(book,2),
                    ["expectedUnit"]=expectedSmall.FirstOrDefault()?.Unit,["source"]=source.Source.Blueprint.AssetGuid,
                    ["child"]=source.Child.Blueprint.AssetGuid,
                    ["summons"]=new JArray(summoned.Select(value=>new JObject{["unit"]=value.UniqueId,["blueprint"]=value.Blueprint.AssetGuid})),
                    ["naturalDefault"]=nativeMode.Describe(),["forcedActs"]=0,["manualCommandTicks"]=0,
                    ["scope"]="One standalone Small Fire native prepared source through actual launcher/buttons/target input, ordinary native Default progression and exact debit. Additional source/budget/input cases remain separate requirements." },
                    "One real explicit-source Cast and native target input create one provenance-owned summon and debit one original preparation without forced animation, command ticks or rule calls.");
                if(!cast)throw new InvalidOperationException("Native ordinary elemental cast/debit did not complete within bounds.");

                // A second actual parent/native AbilityData is now introduced;
                // the exhausted first preparation is deliberately not rested.
                var smallSpec=ElementalSourceChooserPolicy.Rank(2);
                var monsterParent=Cleanup2ElementalAbility(fixture,smallSpec.MonsterParentGuid);
                PrepareExpandedSummoningPlayerPathSpell(book,monsterParent,monsterParent,2);
                if(!fixtureUiOwned() || ElementalSourceChooserView.IsOpen || !nativeMode.Current)
                    throw new InvalidOperationException("Native postcast fixture UI ownership changed; tab rebuild refused.");
                bool tabWasOpen=spellGroup.ToggleState;
                foreach(int frame in Cleanup2SettleChooserNativeRefill(fixture,book,spellGroup,nativeMode,fixtureUiOwned,
                    ownedSessions,session,"postcast-two-sources",value=>session=value))yield return frame;
                Cleanup2Check("elemental-chooser-postcast-tab-reopened",spellGroup.ToggleState && session.Current && session.LauncherAdded,
                    new JObject { ["tabOpenBefore"]=tabWasOpen,["tabOpenAfter"]=spellGroup.ToggleState,
                        ["minimumLevel"]=session.MinimumLevel,["launcherAdded"]=session.LauncherAdded,
                        ["fixtureUiOwned"]=fixtureUiOwned(),["selectedCharacter"]=group.GetCurrentCharacter()?.UniqueId,
                        ["selectedUnits"]=new JArray(ui.SelectionManagerPC.SelectedUnits.Select(value=>value.UniqueId)),
                        ["outcome"]=ElementalSourceChooserRuntime.Outcome,["exceptions"]=new JArray(session.Exceptions),
                        ["sources"]=new JArray(session.Sources.Select(value=>new JObject { ["parent"]=value.Source.Blueprint.AssetGuid,
                            ["child"]=value.Child.Blueprint.AssetGuid,["key"]=value.CreatureKey,["available"]=value.Preparation?.Available })),
                        ["renderedSlots"]=new JArray(spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Select(value=>new JObject {
                            ["active"]=value.gameObject.activeInHierarchy,["mechanism"]=value.MechanicSlot?.GetType().FullName })) },
                    "Native Update consumes the real pending spellbook refill before this observer retains the fresh exact session and selects its launcher level through native OnClick.");
                launcher=spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value=>
                    value.gameObject.activeInHierarchy && value.MechanicSlot is ElementalChooserLauncher);
                var nativeSmall=session.Sources.Single(value=>value.CreatureKey=="small-fire-elemental" &&
                    ReferenceEquals(value.Book,book) && value.Source.Blueprint.AssetGuid==smallSpec.Guid);
                var monsterSmall=session.Sources.Single(value=>value.CreatureKey=="small-fire-elemental" &&
                    ReferenceEquals(value.Book,book) && ReferenceEquals(value.Source.Blueprint,monsterParent));
                var twoSourceSnapshot=new Cleanup2TooltipInputs(fixture.Caster);
                var twoSourcePins=new TeleportationUiSettingsFixture(fixture.Caster.UISettings);
                launcher.OnClick();for(int frame=0;frame<3;frame++)yield return 0;
                ElementalSourceChooserView.ReviewChoose(2,nativeSmall.CreatureKey,nativeSmall,false);
                bool refused=!ElementalSourceChooserView.ReviewTryCast();
                foreach(int frame in screens.Capture("elementals-common-chooser-two-real-sources",new JObject {
                    ["renderedText"]=new JArray(ElementalSourceChooserView.RenderedText),
                    ["nativeExhausted"]=!nativeSmall.Preparation.Available,["monsterAvailable"]=monsterSmall.Preparation.Available,
                    ["sourceReferencesDistinct"]=!ReferenceEquals(nativeSmall.Source,monsterSmall.Source) },
                    ()=>nativeMode.Current && session.Current && ElementalSourceChooserView.IsOpen && Cleanup2NoNarrativeOverlay() && twoSourceSnapshot.Unchanged()))yield return frame;
                bool separated=!ReferenceEquals(nativeSmall.Source,monsterSmall.Source) &&
                    !ReferenceEquals(nativeSmall.Child,monsterSmall.Child) &&
                    !ReferenceEquals(nativeSmall.Preparation,monsterSmall.Preparation) &&
                    ReferenceEquals(nativeSmall.Child.ConvertedFrom,nativeSmall.Source) &&
                    ReferenceEquals(monsterSmall.Child.ConvertedFrom,monsterSmall.Source) &&
                    nativeSmall.Preparation.Available==false && monsterSmall.Preparation.Available &&
                    refused && twoSourceSnapshot.Unchanged() && twoSourcePins.IsRestored() && game.SelectedAbilityHandler.Ability==null;
                Cleanup2Check("elemental-chooser-two-real-sources-exhaustion",separated,new JObject {
                    ["nativeSource"]=DescribeExpandedSummoningAbilityData(nativeSmall.Child),
                    ["monsterSource"]=DescribeExpandedSummoningAbilityData(monsterSmall.Child),
                    ["differentSourceAbilityReferences"]=!ReferenceEquals(nativeSmall.Source,monsterSmall.Source),
                    ["differentExecutionAbilityReferences"]=!ReferenceEquals(nativeSmall.Child,monsterSmall.Child),
                    ["differentPreparationReferences"]=!ReferenceEquals(nativeSmall.Preparation,monsterSmall.Preparation),
                    ["originalExhausted"]=!nativeSmall.Preparation.Available,["otherSourceAvailable"]=monsterSmall.Preparation.Available,
                    ["actualDisabledCastRefused"]=refused,["inputs"]=twoSourceSnapshot.Describe() },
                    "Two different real native source/child/preparation objects coexist; selecting the exhausted source cannot borrow the other source's preparation or hand off targeting.");
                ElementalSourceChooserView.CloseOwned("exhausted-source-review-cancel");
                if(!separated)throw new InvalidOperationException("Real two-source exhaustion separation failed.");
                foreach(int frame in Cleanup2ChooserCastSource(fixture,book,spellGroup,monsterSmall,nativeMode,watch,"monster-with-native-exhausted",fixtureUiOwned,ownedSessions,ownedDispatched))yield return frame;
                // Real native unpreparation makes the canonical source alone.
                // These are disposable fixture allocations, never saved rows.
                book.ForgetMemorized(nativeSmall.Preparation);
                PrepareExpandedSummoningPlayerPathSpell(book,monsterParent,monsterParent,2);
                if(!fixtureUiOwned() || ElementalSourceChooserView.IsOpen || !nativeMode.Current)
                    throw new InvalidOperationException("Canonical-only fixture UI ownership changed; tab rebuild refused.");
                foreach(int frame in Cleanup2SettleChooserNativeRefill(fixture,book,spellGroup,nativeMode,fixtureUiOwned,
                    ownedSessions,session,"canonical-only",value=>session=value))yield return frame;
                var alone=session.Sources.Where(value=>value.CreatureKey=="small-fire-elemental" && ReferenceEquals(value.Book,book)).ToArray();
                Cleanup2Check("elemental-chooser-canonical-source-alone",alone.Length==1 && ReferenceEquals(alone[0].Source.Blueprint,monsterParent),
                    new JObject { ["sourceCount"]=alone.Length,["standaloneStillKnown"]=book.IsKnown(Cleanup2ElementalAbility(fixture,smallSpec.Guid)),
                        ["standalonePreparationRemovedNatively"]=!book.GetMemorizedSpells(2).Any(value=>ReferenceEquals(value,nativeSmall.Preparation)) },
                    "Known legacy ownership remains while native unpreparation leaves the canonical cast source alone in the actual tab.");
                if(alone.Length!=1)throw new InvalidOperationException("Canonical-only native source setup is ambiguous.");
                foreach(int frame in Cleanup2ChooserCastSource(fixture,book,spellGroup,alone[0],nativeMode,watch,"monster-alone",fixtureUiOwned,ownedSessions,ownedDispatched))yield return frame;
            }
            finally
            {
                try
                {
                // Drop only our exact dispatched child; never clear foreign input.
                if(nativeMode.Current && game.SelectedAbilityHandler?.Ability!=null && ownedDispatched.Contains(game.SelectedAbilityHandler.Ability))
                    game.SelectedAbilityHandler.DropAbility();
                if(ownedSessions.Contains(ElementalSourceChooserView.CurrentSession))ElementalSourceChooserView.CloseOwned("review-owned-cleanup");
                _expandedSummoningPlayerPathCaptureActive=false;hooks.UnpatchAll(hooks.Id);
                bool untouched=player.Party.SequenceEqual(party) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected) &&
                    ReferenceEquals(group.GetCurrentCharacter(),selectedGroup);
                bool authority=ReferenceEquals(Game.Instance,game) && ReferenceEquals(game.UI,ui) &&
                    ReferenceEquals(ActionBarManager.Instance,bar) && ReferenceEquals(GroupController.Instance,group) &&
                    bar.Group.GroupElements.SequenceEqual(groups) && ui.TooltipsController.ContextTooltipData==null &&
                    game.SelectedAbilityHandler?.Ability==null && !ElementalSourceChooserView.IsOpen && (untouched || fixtureUiOwned());
                if(authority)
                {
                    for(int index=0;index<groups.Length;index++)groups[index].Toggle(toggles[index],true);
                    group.SelectUnit(selectedGroup);
                    ui.SelectionManagerPC.MultiSelect(selected.Where(value=>value?.View!=null).Select(value=>value.View).ToArray(),false);
                    if(added){player.PartyCharacters.Remove(fixture.Caster);player.InvalidateCharacterLists();player.UpdateCharacterLists();}
                }
                bool restored=player.Party.SequenceEqual(party) && ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected) &&
                    ReferenceEquals(group.GetCurrentCharacter(),selectedGroup) && originalInputs.All(value=>value.Unchanged()) && bars.All(value=>value.IsRestored());
                Cleanup2Check("elemental-chooser-native-ui-restored",restored,new JObject {
                    ["restorationAuthority"]=authority,
                    ["partyExact"]=player.Party.SequenceEqual(party),["selectionExact"]=ui.SelectionManagerPC.SelectedUnits.SequenceEqual(selected),
                    ["originalInputsExact"]=originalInputs.All(value=>value.Unchanged()),["originalPinsExact"]=bars.All(value=>value.IsRestored()) },
                    "Original party, native selection/group, original slots/resources and all original pins restore before the owned native Default mode exits.");
                if(!restored)throw new InvalidOperationException("Original UI/source state restoration is unverified.");
                }
                finally
                {
                    // A UI or hook restoration error must not strand the owned
                    // Default-mode/clock lease. Each owner still verifies its
                    // exact state; this does not clear a foreign mode or input.
                    try{screens.Dispose();}finally{nativeMode.Dispose();}
                }
            }
        }

        private IEnumerable<int> Cleanup2SettleChooserNativeRefill(ExpandedSummoningCorrectionFixture fixture,Spellbook book,
            ActionBarGroupElement spellGroup,Cleanup2NativeDefaultScope mode,Func<bool> uiOwned,
            ICollection<ElementalChooserSession> ownedSessions,ElementalChooserSession captured,string label,
            Action<ElementalChooserSession> completed)
        {
            // Installed HandleMemorizedSpell/HandleForgetSpell/AddSpellHandler
            // queue m_NeedReSet. ActionBarGroup.Update clears it then calls
            // Set(m_Selected) -> Fill on every group. FillSlots rebuilds the
            // production session; SetType -> SetLevelSlotsSet(false) selects
            // the highest nonempty level. A synchronous observer Fill races
            // that queued native update and cannot establish a retained row.
            var bar=ActionBarManager.Instance;var nativeGroup=bar.Group;
            var reset=typeof(ActionBarGroup).GetField("m_NeedReSet",Cleanup2Fields);
            var selected=typeof(ActionBarGroup).GetField("m_Selected",Cleanup2Fields);
            var levelIndex=typeof(ActionBarSubGroupLevels).GetField("Index",Cleanup2Fields);
            var levelsField=typeof(ActionBarGroupElement).GetField("m_Levels",Cleanup2Fields);
            var launcherSession=typeof(ElementalChooserLauncher).GetField("_session",Cleanup2Fields);
            if(reset==null || selected==null || levelIndex==null || levelsField==null || launcherSession==null)
                throw new MissingFieldException("Exact installed native action-bar refill/level fields are unavailable.");
            var books=fixture.Caster.Descriptor.Spellbooks.ToArray();
            var watch=Stopwatch.StartNew();var samples=new JArray();
            ElementalChooserSession candidate=null,requested=null,current=null;
            int quiet=0,frames=0,replacements=0,levelRequests=0;bool reopened=false,settled=false;
            Func<object,int> identity=value=>value==null?0:System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
            Func<JObject> describe=()=>{
                current=ElementalSourceChooserRuntime.Session(spellGroup);
                var levels=levelsField.GetValue(spellGroup) as ActionBarSubGroupLevels;
                var rows=spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Where(value=>value.gameObject.activeInHierarchy).ToArray();
                return new JObject { ["capturedSessionIdentity"]=identity(captured),["currentSessionIdentity"]=identity(current),
                    ["currentIsCapturedSession"]=ReferenceEquals(current,captured),["currentSessionCurrent"]=current!=null && current.Current,
                    ["currentSessionUnitExact"]=ReferenceEquals(current?.Unit,fixture.Caster),
                    ["nativeGroupSelectedExact"]=ReferenceEquals(selected.GetValue(nativeGroup),fixture.Caster),
                    ["nativeElementSelectedExact"]=ReferenceEquals(ElementalSourceChooserRuntime.SelectedField.GetValue(spellGroup),fixture.Caster),
                    ["nativeResetPending"]=(bool)reset.GetValue(nativeGroup),["nativeLevelIndex"]=levels==null?(int?)null:(int)levelIndex.GetValue(levels),
                    ["minimumLevel"]=current?.MinimumLevel,["tabOpen"]=spellGroup.ToggleState,
                    ["currentSourceCount"]=current?.Sources.Count,["publicationOutcome"]=ElementalSourceChooserRuntime.Outcome,
                    ["currentBooksMatchFixture"]=current!=null && current.Books.SequenceEqual(books),
                    ["fixtureBookReferencesExact"]=fixture.Caster.Descriptor.Spellbooks.SequenceEqual(books),
                    ["currentBooks"]=current==null?new JArray():new JArray(current.Books.Select(value=>new JObject {
                        ["blueprint"]=value.Blueprint.AssetGuid,["identity"]=identity(value),["isObservedBook"]=ReferenceEquals(value,book) })),
                    ["renderedSlots"]=new JArray(rows.Take(40).Select(value=>new JObject {
                        ["mechanism"]=value.MechanicSlot?.GetType().FullName,
                        ["launcherSessionExact"]=value.MechanicSlot is ElementalChooserLauncher &&
                            ReferenceEquals(launcherSession.GetValue(value.MechanicSlot),current) })) };
            };
            string refusal=null;string previousState=null;
            while(frames++<600 && watch.Elapsed.TotalSeconds<8)
            {
                if(!uiOwned() || !mode.Current || !Cleanup2NoNarrativeOverlay() || ElementalSourceChooserView.IsOpen ||
                    !ReferenceEquals(ActionBarManager.Instance,bar) || !ReferenceEquals(bar.Group,nativeGroup) ||
                    !nativeGroup.isActiveAndEnabled || !ReferenceEquals(selected.GetValue(nativeGroup),fixture.Caster) ||
                    !fixture.Caster.Descriptor.Spellbooks.SequenceEqual(books) || !books.Any(value=>ReferenceEquals(value,book)))
                {refusal="Foreign fixture UI/book/input owner; further native tab/level writes refused.";break;}
                current=ElementalSourceChooserRuntime.Session(spellGroup);
                bool pending=(bool)reset.GetValue(nativeGroup);
                bool valid=current!=null && current.Current && ReferenceEquals(current.Unit,fixture.Caster) &&
                    ReferenceEquals(current.Owner,spellGroup) && ReferenceEquals(current.Levels,levelsField.GetValue(spellGroup)) &&
                    current.Books.SequenceEqual(books) && current.LauncherAdded;
                if(!ReferenceEquals(candidate,current))
                {if(candidate!=null)replacements++;candidate=current;requested=null;quiet=0;}
                quiet=valid && !pending?quiet+1:0;
                if(valid)ownedSessions.Add(current);
                int index=valid?(int)levelIndex.GetValue(current.Levels):-1;
                var launchers=spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Where(value=>value.gameObject.activeInHierarchy &&
                    value.MechanicSlot is ElementalChooserLauncher).ToArray();
                bool exactLauncher=valid && launchers.Length==1 && ReferenceEquals(launchers[0].MechanicSlot.Unit,fixture.Caster) &&
                    ReferenceEquals(launcherSession.GetValue(launchers[0].MechanicSlot),current);
                string state=identity(current)+":"+pending+":"+index+":"+exactLauncher+":"+spellGroup.ToggleState;
                if(samples.Count<16 && (state!=previousState || quiet==3))
                {var sample=describe();sample["frame"]=Time.frameCount;sample["quietFrames"]=quiet;samples.Add(sample);previousState=state;}
                if(quiet>=3)
                {
                    if(!spellGroup.ToggleState)
                    {spellGroup.OnClick();reopened=true;quiet=0;requested=null;}
                    else if(!ReferenceEquals(requested,current))
                    {current.Levels.OnClick(current.MinimumLevel);requested=current;levelRequests++;quiet=0;}
                    else if(index!=current.MinimumLevel)
                    {refusal="Native level changed after selection without a session refill; stale presentation is not overwritten.";break;}
                    else if(exactLauncher)
                    {settled=true;break;}
                }
                yield return 0;
            }
            var evidence=describe();evidence["samples"]=samples;evidence["refusal"]=refusal;
            evidence["wallSeconds"]=watch.Elapsed.TotalSeconds;evidence["frames"]=frames;evidence["stableNativeFrames"]=quiet;
            evidence["observedSessionReplacements"]=replacements;evidence["nativeOnClickReopenedTab"]=reopened;
            evidence["nativeLevelSelections"]=levelRequests;evidence["observerFillCalls"]=0;
            Cleanup2Check("elemental-chooser-settled-native-refill-"+label,settled,evidence,
                "Normal native Update consumes pending preparation events; the fresh exact fixture session/books, chosen native level and its actual launcher remain stable across three real frames. No reset flag, level field, source chain or native row is fabricated.");
            if(!settled)throw new InvalidOperationException("Native chooser refill did not settle: "+label+":"+(refusal??"bounded native Update/level/launcher wait expired"));
            completed(current);
        }

        private IEnumerable<int> Cleanup2ChooserCastSource(ExpandedSummoningCorrectionFixture fixture,Spellbook book,
            ActionBarGroupElement spellGroup,ElementalNativeSource source,Cleanup2NativeDefaultScope mode,Stopwatch phase,string label,
            Func<bool> uiOwned,ICollection<ElementalChooserSession> ownedSessions,ICollection<AbilityData> ownedDispatched)
        {
            var session=ElementalSourceChooserRuntime.Session(spellGroup);
            if(session==null || !session.Current || !spellGroup.ToggleState || !session.Sources.Contains(source))
                throw new InvalidOperationException("The actual open tab must still own the explicitly selected source before casting: "+label);
            session.Levels.OnClick(session.MinimumLevel);
            for(int frame=0;frame<6;frame++)yield return 0;
            if(!uiOwned() || !mode.Current || !session.Current || !spellGroup.ToggleState || ElementalSourceChooserView.IsOpen)
                throw new InvalidOperationException("Source launcher UI ownership changed after frame progression: "+label);
            ownedSessions.Add(session);
            var launcher=spellGroup.GetComponentsInChildren<ActionBarGroupSlot>(true).Single(value=>value.gameObject.activeInHierarchy &&
                value.MechanicSlot is ElementalChooserLauncher);
            int before=CountAvailableExpandedSummoningSlots(book,source.Source.SpellLevel);
            var resources=Cleanup2ElementalResources(fixture.Caster);
            ExpandedSummoningRuleCapture.Clear();ExpandedSummoningPlayerPathEvents.Clear();
            _expandedSummoningPlayerPathRuleCastCount=0;_expandedSummoningPlayerPathSpawnActionCount=0;
            launcher.OnClick();for(int frame=0;frame<3;frame++)yield return 0;
            if(!uiOwned() || !mode.Current || !session.Current || !ReferenceEquals(ElementalSourceChooserView.CurrentSession,session))
                throw new InvalidOperationException("Source chooser ownership changed before explicit dispatch: "+label);
            ownedDispatched.Add(source.Child);
            ElementalSourceChooserView.ReviewChoose(source.Rank,source.CreatureKey,source,true);
            bool selected=ReferenceEquals(Game.Instance.SelectedAbilityHandler.Ability,source.Child);
            bool clicked=selected && Game.Instance.SelectedAbilityHandler.OnClick(null,fixture.Caster.Position+new Vector3(2,0,2),0);
            UnitUseAbility command=fixture.Caster.Commands.GetCommand<UnitUseAbility>();
            bool issued=clicked && command!=null && ReferenceEquals(command.Spell,source.Child);
            var watch=Stopwatch.StartNew();
            while(issued && watch.Elapsed.TotalSeconds<25 && phase.Elapsed.TotalSeconds<360 &&
                (_expandedSummoningPlayerPathRuleCastCount==0 || !fixture.Caster.Commands.Empty))
            {
                if(!mode.Current || !Cleanup2NoNarrativeOverlay())throw new InvalidOperationException("Native source cast lost its Default/input context.");
                Game.Instance.IsPaused=false;yield return 0;
            }
            var units=ExpandedSummoningRuleCapture.Where(value=>value!=null && ExpandedSummoningPlayerPathUnitExact(value,fixture.Caster)).Distinct().ToArray();
            foreach(var unit in units)if(!fixture.Created.Contains(unit))fixture.Created.Add(unit);
            var expected=SummonSignatureReader.Read(source.Child.Blueprint).Spawns().ToArray();
            bool passed=issued && expected.Length==1 && units.Length==1 && units[0].Blueprint.AssetGuid==expected[0].Unit &&
                _expandedSummoningPlayerPathRuleCastCount==1 && _expandedSummoningPlayerPathSpawnActionCount==1 &&
                CountAvailableExpandedSummoningSlots(book,source.Source.SpellLevel)==before-1 && !source.Preparation.Available &&
                Cleanup2ElementalResources(fixture.Caster)==resources && fixture.Caster.Commands.Empty &&
                ReferenceEquals(source.Child.ConvertedFrom,source.Source) && ReferenceEquals(source.Child.Spellbook,book);
            Cleanup2Check("elemental-chooser-native-cast-"+label,passed,new JObject {
                ["source"]=source.Source.Blueprint.AssetGuid,["child"]=source.Child.Blueprint.AssetGuid,
                ["selectedChildExact"]=selected,["targetClicked"]=clicked,["nativeCommandExactAbility"]=issued,
                ["slotBefore"]=before,["slotAfter"]=CountAvailableExpandedSummoningSlots(book,source.Source.SpellLevel),
                ["slotReferenceSpent"]=!source.Preparation.Available,["sourceLinks"]=DescribeExpandedSummoningAbilityData(source.Child),
                ["ruleCasts"]=_expandedSummoningPlayerPathRuleCastCount,["spawnActions"]=_expandedSummoningPlayerPathSpawnActionCount,
                ["actualUnit"]=units.FirstOrDefault()?.Blueprint.AssetGuid,["expectedUnit"]=expected.FirstOrDefault()?.Unit,
                ["resourcesExact"]=Cleanup2ElementalResources(fixture.Caster)==resources,["wallSeconds"]=watch.Elapsed.TotalSeconds,
                ["forcedActs"]=0,["manualCommandTicks"]=0 },
                "The explicit original source reaches the exact native command, one cast/spawn action and expected actor while spending only that original preparation.");
            if(!passed)throw new InvalidOperationException("Native source cast/debit failed: "+label);
        }
    }
}
