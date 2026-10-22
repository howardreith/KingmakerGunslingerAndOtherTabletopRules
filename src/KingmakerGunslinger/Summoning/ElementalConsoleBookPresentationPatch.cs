using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.UI._ConsoleUI;
using Kingmaker.UI._ConsoleUI.CharGen.Phases.Spells;
using Kingmaker.UI._ConsoleUI.Common;
using Kingmaker.UI._ConsoleUI.SpellBook;
using Kingmaker.UI._ConsoleUI.Utils.HintTool;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using TMPro;
using UniRx;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    // Only native virtual-list presentation is projected. Neither authoritative
    // VM collection, known/prepared object, selection budget nor spell list is
    // replaced. A header is an owned transient VM borrowing one original source
    // for native row sizing/icon; it can never select/memorize/cast that source.
    internal static class ElementalConsoleBookPresentation
    {
        internal const BindingFlags Fields=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        internal static readonly Type LearningView=typeof(SelectorEntitiesGroupView<CharGenSpellSelectorItemVM,CharGenSpellSelectorItemView>);
        private static readonly FieldInfo Selection=typeof(CharGenSpellsPhaseVM).GetField("m_SelectionData",Fields);
        private static readonly FieldInfo PreviewBook=typeof(CharGenSpellsPhaseVM).GetField("PreviewSpellbook",Fields);
        private static readonly ConditionalWeakTable<List<CharGenSpellSelectorItemVM>,CharGenSpellsPhaseVM> LearningOwners=
            new ConditionalWeakTable<List<CharGenSpellSelectorItemVM>,CharGenSpellsPhaseVM>();
        private static readonly ConditionalWeakTable<CharGenSpellsSelectorView,LearningContext> LearningContexts=
            new ConditionalWeakTable<CharGenSpellsSelectorView,LearningContext>();
        private static readonly ConditionalWeakTable<SpellBookKnownSpellsVM,BookIntake> BookOwners=
            new ConditionalWeakTable<SpellBookKnownSpellsVM,BookIntake>();
        private static readonly ConditionalWeakTable<SpellbookKnownSpellsScrollView,PreparationContext> PreparationContexts=
            new ConditionalWeakTable<SpellbookKnownSpellsScrollView,PreparationContext>();
        internal static string Outcome="not-used";
        internal static int LearningHeaders,PreparationHeaders,HeaderConfirms;
        internal static LearningContext LearningFor(CharGenSpellsSelectorView view)
        {LearningContext value;return view!=null && LearningContexts.TryGetValue(view,out value)?value:null;}
        internal static PreparationContext PreparationFor(SpellbookKnownSpellsScrollView view)
        {PreparationContext value;return view!=null && PreparationContexts.TryGetValue(view,out value)?value:null;}

        internal static object Model(Component view)
        {return view==null?null:view.GetType().GetProperty("ViewModel",Fields)?.GetValue(view,null);}
        internal static bool Exact(MethodBase method,int bytes)
        {return method!=null && method.Module.ModuleVersionId.ToString()==ElementalSourceChooserRuntime.GameMvid &&
            method.GetMethodBody()?.GetILAsByteArray().Length==bytes;}
        internal static void Safe(Action action)
        {try{action();}catch(Exception error){Outcome="preserve-native-console-book:"+error.GetType().Name+":"+error.Message;}}
        internal static void RegisterLearning(CharGenSpellsPhaseVM owner,List<CharGenSpellSelectorItemVM> rows)
        {if(rows==null)return;LearningOwners.Remove(rows);LearningOwners.Add(rows,owner);}
        internal static void RegisterBook(SpellBookKnownSpellsVM vm,Spellbook book)
        {BookOwners.Remove(vm);BookOwners.Add(vm,new BookIntake {Book=book});}
        private sealed class BookIntake {internal Spellbook Book;}

        internal sealed class LearningHeader : CharGenSpellSelectorItemVM
        {
            internal readonly LearningContext Context;
            internal LearningHeader(LearningContext context,CharGenSpellSelectorItemVM source)
                :base(source.Spell,source.Level,source.RecommendationPriority,source.HasInSpellbook)
            {Context=context;CanChoose.Value=false;}
        }
        internal sealed class PreparationHeader : AbilityDataVM
        {
            internal readonly PreparationContext Context;
            internal PreparationHeader(PreparationContext context,AbilityDataVM source,Spellbook book):base(source.SpellData,book)
            {Context=context;DisplayName="Elementals";Description="Confirm to show or hide original spell sources. Choose an original source to prepare, cast or edit metamagic.";
                SchoolName="Show original spell sources";LevelIsValid=false;IsDomain=false;IsSpecial=false;}
        }

        internal sealed class LearningContext
        {
            internal CharGenSpellsSelectorView View;
            internal SelectorEntitiesGroupVM<CharGenSpellSelectorItemVM> Group;
            internal CharGenSpellsPhaseVM Owner;
            internal SpellSelectionData SelectionData;
            internal Spellbook Book;
            internal CharGenSpellSelectorItemVM[] Originals,Sources;
            internal LearningHeader Header;
            internal List<CharGenSpellSelectorItemVM> Projection;
            internal bool Expanded,Released,Retiring,Disposed;
            internal bool Current {get{return !Released && View!=null && !Owner.IsDisposed &&
                ReferenceEquals(Model(View),Group) && ReferenceEquals(Owner.SpellsSelector.Value,Group) &&
                ReferenceEquals(Selection.GetValue(Owner),SelectionData) && ReferenceEquals(PreviewBook.GetValue(Owner),Book) &&
                Group.VisibleCollection.SequenceEqual(Originals) &&
                ElementalSourceChooserPolicy.SourcesRemainEligible(Sources,value=>!value.IsHidden);}}
            internal bool Selected(CharGenSpellSelectorItemVM source)
            {return source.IsChoosen.Value || ReferenceEquals(Group.LastSelectedEntity.Value,source) ||
                (Owner.MechanicSelectedSpells!=null && Owner.MechanicSelectedSpells.Any(value=>ReferenceEquals(value,source.Spell)));}
            internal void Build()
            {
                Header.CanChoose.Value=false;
                Projection=ElementalSourceChooserPolicy.ProjectOriginalSources(Originals,Sources,Header,Expanded,Selected,value=>!value.IsHidden);
            }
            internal void Toggle()
            {
                if(!Current || !ElementalSourceChooserRuntime.Active){Release(true);return;}
                try
                {Expanded=!Expanded;Build();View.UpdateViews();LearningView.GetMethod("TrySelectChosenEntity",Fields)
                        .Invoke(View,new object[]{Header});
                    HeaderConfirms++;Outcome="console-learning-original-source-disclosure;expanded="+Expanded;}
                catch(Exception error)
                {try{Release(true);}catch(Exception restore){throw new InvalidOperationException("Native learning refresh failed; original restoration unverified: "+restore.Message,error);}throw;}
            }
            internal void Release(bool refresh)
            {
                if(Disposed || Retiring)return;bool first=!Released;Released=true;Retiring=true;
                try
                {
                    if(Header==null){Disposed=true;return;}
                    if(first && refresh && View!=null && ReferenceEquals(Model(View),Group))
                    {View.UpdateViews();View.SelectCurrentOrFirst();}
                    var data=View==null?null:LearningView.GetField("m_VirtualViews",Fields).GetValue(View) as IList<IVirtualItemData>;
                    bool bound=View!=null && View.GetComponentsInChildren<CharGenSpellSelectorItemView>(true)
                        .Any(row=>ReferenceEquals(row.GetViewModelClass(),Header));
                    if((data==null || !data.Any(value=>ReferenceEquals(value,Header))) && !bound)
                    {Header?.Dispose();Projection=null;Disposed=true;}
                    else Outcome="console-learning-header-retired-await-native-unbind";
                }
                finally{Retiring=false;}
            }
        }
        internal sealed class PreparationContext
        {
            internal SpellbookKnownSpellsScrollView View;
            internal SpellBookKnownSpellsVM Group;
            internal SpellBookVM BookOwner;
            internal Spellbook Book;
            internal AbilityDataVM[] Originals,Sources;
            internal PreparationHeader Header;
            internal List<AbilityDataVM> Projection;
            internal IReactiveProperty<AbilityDataVM> SelectionProperty;
            internal AbilityDataVM PreviousSelected;
            internal bool Expanded,Released,Retiring,Disposed;
            internal bool Current {get{return !Released && OwnsLiveSelection &&
                ReferenceEquals(Model(View),Group) && Group.KnownSpells.SequenceEqual(Originals) &&
                Originals.All(value=>ReferenceEquals(value.SpellData?.Spellbook,Book));}}
            private bool OwnsLiveSelection {get{return View!=null && Group!=null && !Group.IsDisposed &&
                BookOwner!=null && !BookOwner.IsDisposed && SelectionProperty!=null &&
                ReferenceEquals(Group.CurrentSelectedSpell,SelectionProperty) &&
                ReferenceEquals(Model(View),Group) && ReferenceEquals(BookOwner.CurrentSpellbook.Value,Book) &&
                ReferenceEquals(BookOwner.SpellBookKnownSpells,Group);}}
            internal void Build()
            {Projection=ElementalSourceChooserPolicy.ProjectOriginalSources(Originals,Sources,Header,Expanded,
                source=>ReferenceEquals(Group.CurrentSelectedSpell.Value,source));
                Header.SchoolName=Expanded?"Hide original spell sources":"Show original spell sources";}
            internal void Refresh()
            {
                typeof(SpellbookKnownSpellsScrollView).GetField("m_VirtualViews",Fields).SetValue(View,null);
                var list=(VirtualListController)typeof(SpellbookKnownSpellsScrollView).GetField("m_VirtualListController",Fields).GetValue(View);
                list.UpdateData(VirtualListController.ScrollBasePosition.None);
                typeof(SpellbookKnownSpellsScrollView).GetMethod("OnPageChanged",Fields).Invoke(View,null);
            }
            internal void Toggle()
            {
                if(!Current || !ElementalSourceChooserRuntime.Active){Release(true);return;}
                try
                {Expanded=!Expanded;Build();Refresh();HeaderConfirms++;
                    Outcome="console-preparation-original-source-disclosure;expanded="+Expanded;}
                catch(Exception error)
                {try{Release(true);}catch(Exception restore){throw new InvalidOperationException("Native preparation refresh failed; original restoration unverified: "+restore.Message,error);}throw;}
            }
            internal void Remember(AbilityDataVM previous)
            {if(!(previous is PreparationHeader) && (previous==null || Originals.Any(value=>ReferenceEquals(value,previous))))PreviousSelected=previous;}
            internal void Release(bool refresh)
            {
                if(Disposed || Retiring)return;bool first=!Released;Released=true;Retiring=true;
                try
                {
                    if(Header==null){Disposed=true;return;}
                    // Native DisposeImplementation clears CurrentSelectedSpell.
                    // An old header may outlive that VM until its pooled rows
                    // unbind; retire it without calling back into disposed or
                    // replaced native selection/book authority.
                    bool live=OwnsLiveSelection;
                    if(live && ReferenceEquals(SelectionProperty.Value,Header))
                        SelectionProperty.Value=PreviousSelected!=null &&
                            Group.KnownSpells.Any(value=>ReferenceEquals(value,PreviousSelected))?PreviousSelected:null;
                    if(first && refresh && live)Refresh();
                    var data=View==null?null:typeof(SpellbookKnownSpellsScrollView).GetField("m_VirtualViews",Fields).GetValue(View) as IList<IVirtualItemData>;
                    bool bound=View!=null && View.GetComponentsInChildren<SpellBookKnownSpellView>(true)
                        .Any(row=>ReferenceEquals(row.GetViewModel(),Header));
                    if((data==null || !data.Any(value=>ReferenceEquals(value,Header))) && !bound)
                    {Header?.Dispose();Projection=null;Disposed=true;}
                    else Outcome="console-preparation-header-retired-await-native-unbind";
                }
                finally{Retiring=false;}
            }
        }

        internal static List<CharGenSpellSelectorItemVM> LearningRows(SelectorEntitiesGroupVM<CharGenSpellSelectorItemVM> group,
            SelectorEntitiesGroupView<CharGenSpellSelectorItemVM,CharGenSpellSelectorItemView> nativeView)
        {
            var view=nativeView as CharGenSpellsSelectorView;
            if(view==null)return group.VisibleCollection;
            LearningContext context;
            return LearningContexts.TryGetValue(view,out context) && context.Current && ElementalSourceChooserRuntime.Active?
                context.Projection:group.VisibleCollection;
        }
        internal static void BeginLearning(SelectorEntitiesGroupView<CharGenSpellSelectorItemVM,CharGenSpellSelectorItemView> nativeView)
        {
            var view=nativeView as CharGenSpellsSelectorView;if(view==null)return;
            var group=Model(view) as SelectorEntitiesGroupVM<CharGenSpellSelectorItemVM>;
            LearningContext previous;
            if(LearningContexts.TryGetValue(view,out previous))
            {
                if(previous.Retiring)return;
                if(previous.Current && ElementalSourceChooserRuntime.Active){previous.Build();return;}
                previous.Release(false);LearningContexts.Remove(view);
            }
            CharGenSpellsPhaseVM owner;
            if(!ElementalSourceChooserRuntime.Active || !Game.Instance.IsControllerGamepad || group?.VisibleCollection==null ||
                !LearningOwners.TryGetValue(group.VisibleCollection,out owner) || owner.IsDisposed)return;
            var selection=Selection.GetValue(owner) as SpellSelectionData;var book=PreviewBook.GetValue(owner) as Spellbook;
            if(selection?.SpellList==null || selection.Spellbook==null || selection.ExtraByStat || book==null ||
                !ReferenceEquals(selection.SpellList,selection.Spellbook.SpellList) || !ReferenceEquals(book.Blueprint,selection.Spellbook) ||
                !StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(selection.SpellList.AssetGuid))return;
            var sources=group.VisibleCollection.Where(value=>!value.IsHidden && value.Spell!=null &&
                ElementalSourceChooserPolicy.Parent(value.Spell.AssetGuid)!=null &&
                selection.SpellList.GetSpells(value.Level).Any(spell=>ReferenceEquals(spell,value.Spell))).ToArray();
            if(sources.Length==0)return;
            var context=new LearningContext {View=view,Owner=owner,Group=group,SelectionData=selection,Book=book,
                Originals=group.VisibleCollection.ToArray(),Sources=sources};
            try
            {
                context.Header=new LearningHeader(context,sources[0]);context.Build();LearningContexts.Add(view,context);
                var lifetime=view.GetComponent<ElementalConsoleBookLifetime>() ?? view.gameObject.AddComponent<ElementalConsoleBookLifetime>();
                lifetime.Learning.Add(context);LearningHeaders++;Outcome="console-learning-native-projection-ready";
            }
            catch{context.Release(false);throw;}
        }
        internal static void BeginPreparation(SpellbookKnownSpellsScrollView view)
        {
            var group=Model(view) as SpellBookKnownSpellsVM;PreparationContext previous;
            if(PreparationContexts.TryGetValue(view,out previous))
            {
                if(previous.Retiring)return;
                if(previous.Current && ElementalSourceChooserRuntime.Active){previous.Build();return;}
                previous.Release(false);PreparationContexts.Remove(view);
            }
            BookIntake intake;
            if(!ElementalSourceChooserRuntime.Active || !Game.Instance.IsControllerGamepad || group==null || group.IsDisposed ||
                !BookOwners.TryGetValue(group,out intake) || intake.Book?.Blueprint.SpellList==null ||
                !StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(intake.Book.Blueprint.SpellList.AssetGuid))return;
            Spellbook book=intake.Book;
            var bookView=view.GetComponentInParent<SpellBookView>();var bookOwner=Model(bookView) as SpellBookVM;
            if(bookOwner==null || !ReferenceEquals(bookOwner.CurrentSpellbook.Value,book) ||
                !ReferenceEquals(bookOwner.SpellBookKnownSpells,group))return;
            var sources=group.KnownSpells.Where(value=>value.SpellData!=null && value.Type==SpellSlotType.Common &&
                ReferenceEquals(value.SpellData.Spellbook,book) && value.SpellData.SourceItem==null &&
                (!book.IsSpellSpecial(value.SpellData) || book.IsSpellCommon(value.SpellData)) &&
                ElementalSourceChooserPolicy.Parent(value.SpellData.Blueprint.AssetGuid)!=null &&
                book.Blueprint.SpellList.GetSpells(ElementalSourceChooserPolicy.SourceRank(value.SpellData.Blueprint.AssetGuid))
                    .Any(spell=>ReferenceEquals(spell,value.SpellData.Blueprint))).ToArray();
            if(sources.Length==0)return;
            var context=new PreparationContext {View=view,Group=group,BookOwner=bookOwner,Book=book,Originals=group.KnownSpells.ToArray(),Sources=sources,
                SelectionProperty=group.CurrentSelectedSpell,PreviousSelected=group.CurrentSelectedSpell.Value};
            try
            {
                context.Header=new PreparationHeader(context,sources[0],book);context.Build();PreparationContexts.Add(view,context);
                var lifetime=view.GetComponent<ElementalConsoleBookLifetime>() ?? view.gameObject.AddComponent<ElementalConsoleBookLifetime>();
                lifetime.Preparation.Add(context);PreparationHeaders++;Outcome="console-preparation-native-projection-ready";
            }
            catch{context.Release(false);throw;}
        }
        internal static List<AbilityDataVM> PreparationRows(SpellBookKnownSpellsVM group,SpellbookKnownSpellsScrollView view)
        {PreparationContext context;return PreparationContexts.TryGetValue(view,out context) && context.Current && ElementalSourceChooserRuntime.Active?
            context.Projection:group.KnownSpells;}

        internal static LearningHeader FocusedLearning(CharGenSpellsPhaseDetailedView owner)
        {
            var embeddedBook=(ReactiveProperty<bool>)typeof(CharGenSpellsPhaseDetailedView)
                .GetField("m_IsSpellbookActive",Fields).GetValue(owner);
            if(embeddedBook.Value)return null;
            var view=typeof(CharGenSpellsPhaseDetailedView).GetField("m_SpellsSelector",Fields).GetValue(owner) as CharGenSpellsSelectorView;
            var row=view?.CurrentSelected as CharGenSpellSelectorItemView;
            return row?.GetViewModelClass() as LearningHeader;
        }
        internal static PreparationHeader FocusedPreparation(SpellBookView owner)
        {
            var row=typeof(SpellBookView).GetField("m_CurrentSelected",Fields).GetValue(owner) as SpellBookKnownSpellView;
            return row?.GetViewModel() as PreparationHeader;
        }
        internal static void HeaderLearningText(CharGenSpellSelectorItemView view)
        {
            var header=view.GetViewModelClass() as LearningHeader;if(header==null)return;
            var title=typeof(CharGenSpellSelectorItemView).GetField("m_SpellName",Fields).GetValue(view) as TextMeshProUGUI;
            var school=typeof(CharGenSpellSelectorItemView).GetField("m_SchoolName",Fields).GetValue(view) as TextMeshProUGUI;
            title.text="Elementals";school.text=header.Context.Expanded?"Hide original spell sources":"Confirm to show original spell sources";
        }
        internal static void PreparationHints(SpellBookView view)
        {
            var header=FocusedPreparation(view);if(header==null)return;
            ((ReactiveProperty<bool>)typeof(SpellBookView).GetField("m_ConfirmHintActive",Fields).GetValue(view)).Value=header.Context.Current;
            ((ReactiveProperty<bool>)typeof(SpellBookView).GetField("m_ContextHintActive",Fields).GetValue(view)).Value=false;
            ((ReactiveProperty<bool>)typeof(SpellBookView).GetField("m_MetaMagicHintActive",Fields).GetValue(view)).Value=false;
            ((GenericHintView)typeof(SpellBookView).GetField("m_ConfirmHint",Fields).GetValue(view)).SetLabel(
                header.Context.Expanded?"Hide original sources":"Show original sources");
        }
        internal static IEnumerable<CodeInstruction> Project(IEnumerable<CodeInstruction> instructions,FieldInfo source,MethodInfo helper,int expected)
        {
            var codes=instructions.ToList();int count=0;
            foreach(var code in codes)
            {
                if(code.opcode==OpCodes.Ldfld && Equals(code.operand,source))
                {
                    if(code.blocks.Count!=0)throw new InvalidOperationException("Unexpected native virtual-list exception boundary.");
                    var self=new CodeInstruction(OpCodes.Ldarg_0);self.labels.AddRange(code.labels);yield return self;
                    yield return new CodeInstruction(OpCodes.Call,helper);count++;
                }
                else yield return code;
            }
            if(count!=expected)throw new InvalidOperationException("Native elemental virtual-list reads changed: "+count);
        }
    }
    internal sealed class ElementalConsoleBookLifetime : MonoBehaviour
    {
        internal readonly List<ElementalConsoleBookPresentation.LearningContext> Learning=new List<ElementalConsoleBookPresentation.LearningContext>();
        internal readonly List<ElementalConsoleBookPresentation.PreparationContext> Preparation=new List<ElementalConsoleBookPresentation.PreparationContext>();
        private void Update()
        {
            foreach(var value in Learning.ToArray())
                if(value.Released || !ElementalSourceChooserRuntime.Active || !value.Current)
                    ElementalConsoleBookPresentation.Safe(()=>value.Release(!value.Released));
            foreach(var value in Preparation.ToArray())
                if(value.Released || !ElementalSourceChooserRuntime.Active || !value.Current)
                    ElementalConsoleBookPresentation.Safe(()=>value.Release(!value.Released));
            Learning.RemoveAll(value=>value.Disposed);Preparation.RemoveAll(value=>value.Disposed);
        }
        private void OnDestroy()
        {foreach(var value in Learning)ElementalConsoleBookPresentation.Safe(()=>value.Release(false));
            foreach(var value in Preparation)ElementalConsoleBookPresentation.Safe(()=>value.Release(false));}
    }
    [HarmonyPatch(typeof(CharGenSpellsPhaseVM),"GetSpellsCollection")]
    internal static class ElementalConsoleLearningIntakePatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(CharGenSpellsPhaseVM),"GetSpellsCollection",298);}
        private static void Postfix(CharGenSpellsPhaseVM __instance,List<CharGenSpellSelectorItemVM> __result)
        {ElementalConsoleBookPresentation.Safe(()=>ElementalConsoleBookPresentation.RegisterLearning(__instance,__result));}
    }
    [HarmonyPatch]
    internal static class ElementalConsoleLearningProjectionPatch
    {
        private static MethodBase TargetMethod(){return ElementalConsoleBookPresentation.LearningView.GetMethod("GenerateVirtualViews",ElementalConsoleBookPresentation.Fields);}
        private static bool Prepare(){return ElementalConsoleBookPresentation.Exact(TargetMethod(),339);}
        private static void Prefix(SelectorEntitiesGroupView<CharGenSpellSelectorItemVM,CharGenSpellSelectorItemView> __instance)
        {ElementalConsoleBookPresentation.Safe(()=>ElementalConsoleBookPresentation.BeginLearning(__instance));}
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {return ElementalConsoleBookPresentation.Project(instructions,
            typeof(SelectorEntitiesGroupVM<CharGenSpellSelectorItemVM>).GetField("VisibleCollection"),
            typeof(ElementalConsoleBookPresentation).GetMethod("LearningRows",BindingFlags.Static|BindingFlags.NonPublic),3);}
    }
    [HarmonyPatch(typeof(CharGenSpellSelectorItemView),"RefreshView")]
    internal static class ElementalConsoleLearningHeaderTextPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(CharGenSpellSelectorItemView),"RefreshView",165);}
        private static void Postfix(CharGenSpellSelectorItemView __instance)
        {ElementalConsoleBookPresentation.Safe(()=>ElementalConsoleBookPresentation.HeaderLearningText(__instance));}
    }
    [HarmonyPatch(typeof(CharGenSpellsPhaseDetailedView),"PressConfirmOnPhase")]
    internal static class ElementalConsoleLearningConfirmPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(CharGenSpellsPhaseDetailedView),"PressConfirmOnPhase",261);}
        private static bool Prefix(CharGenSpellsPhaseDetailedView __instance,ref bool __result)
        {
            var header=ElementalConsoleBookPresentation.FocusedLearning(__instance);if(header==null)return true;
            ElementalConsoleBookPresentation.Safe(()=>header.Context.Toggle());__result=false;return false;
        }
    }
    [HarmonyPatch]
    internal static class ElementalConsolePreparationIntakePatch
    {
        private static MethodBase TargetMethod()
        {return typeof(SpellBookKnownSpellsVM).GetConstructor(ElementalConsoleBookPresentation.Fields,null,
            new[]{typeof(Spellbook),typeof(SpellBookLevelVM),typeof(IReactiveProperty<AbilityDataVM>)},null);}
        private static bool Prepare()
        {var target=TargetMethod();return ElementalConsoleBookPresentation.Exact(target,206) && target.MetadataToken==0x060050df;}
        private static void Postfix(SpellBookKnownSpellsVM __instance,Spellbook __0)
        {ElementalConsoleBookPresentation.Safe(()=>ElementalConsoleBookPresentation.RegisterBook(__instance,__0));}
    }
    [HarmonyPatch(typeof(SpellbookKnownSpellsScrollView),"GenerateVirtualViews")]
    internal static class ElementalConsolePreparationProjectionPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(SpellbookKnownSpellsScrollView),"GenerateVirtualViews",228);}
        private static void Prefix(SpellbookKnownSpellsScrollView __instance)
        {ElementalConsoleBookPresentation.Safe(()=>ElementalConsoleBookPresentation.BeginPreparation(__instance));}
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {return ElementalConsoleBookPresentation.Project(instructions,typeof(SpellBookKnownSpellsVM).GetField("KnownSpells"),
            typeof(ElementalConsoleBookPresentation).GetMethod("PreparationRows",BindingFlags.Static|BindingFlags.NonPublic),2);}
    }
    [HarmonyPatch(typeof(SpellBookView),"WantConfirm")]
    internal static class ElementalConsolePreparationConfirmPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(SpellBookView),"WantConfirm",232);}
        private static bool Prefix(SpellBookView __instance)
        {var header=ElementalConsoleBookPresentation.FocusedPreparation(__instance);if(header==null)return true;
            ElementalConsoleBookPresentation.Safe(()=>header.Context.Toggle());return false;}
    }
    [HarmonyPatch(typeof(SpellBookView),"SetupHintsLabels")]
    internal static class ElementalConsolePreparationHintPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(SpellBookView),"SetupHintsLabels",156);}
        private static void Postfix(SpellBookView __instance)
        {ElementalConsoleBookPresentation.Safe(()=>ElementalConsoleBookPresentation.PreparationHints(__instance));}
    }
    [HarmonyPatch(typeof(SpellBookVM),"SelectKnownSpell")]
    internal static class ElementalConsolePreparationSelectedPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(SpellBookVM),"SelectKnownSpell",13);}
        private static void Prefix(SpellBookVM __instance,AbilityDataVM __0)
        {var header=__0 as ElementalConsoleBookPresentation.PreparationHeader;
            if(header!=null)header.Context.Remember(__instance.CurrentSelectedSpell.Value);}
    }
    [HarmonyPatch(typeof(SpellBookVM),"TryMemorize")]
    internal static class ElementalConsolePreparationBudgetGuardPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(SpellBookVM),"TryMemorize",79);}
        private static bool Prefix(AbilityDataVM __0,ref bool __result)
        {if(!(__0 is ElementalConsoleBookPresentation.PreparationHeader))return true;__result=false;return false;}
    }
    [HarmonyPatch(typeof(SpellBookVM),"TryContext")]
    internal static class ElementalConsolePreparationContextGuardPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(SpellBookVM),"TryContext",357);}
        private static bool Prefix(AbilityDataVM __1){return !(__1 is ElementalConsoleBookPresentation.PreparationHeader);}
    }
    [HarmonyPatch(typeof(SpellBookVM),"SwitchMetaMagicBuilderMode")]
    internal static class ElementalConsolePreparationMetaGuardPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(SpellBookVM),"SwitchMetaMagicBuilderMode",40);}
        private static bool Prefix(SpellBookVM __instance)
        {return !(__instance.CurrentSelectedSpell.Value is ElementalConsoleBookPresentation.PreparationHeader);}
    }
    [HarmonyPatch(typeof(CharGenSpellSelectorItemView),"TooltipObject")]
    internal static class ElementalConsoleLearningHeaderTooltipPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(CharGenSpellSelectorItemView),"TooltipObject",12);}
        private static void Postfix(CharGenSpellSelectorItemView __instance,ref object __result)
        {if(__instance.GetViewModelClass() is ElementalConsoleBookPresentation.LearningHeader)__result=null;}
    }
    [HarmonyPatch(typeof(AbilityDataVM),"TooltipObject")]
    internal static class ElementalConsolePreparationHeaderTooltipPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(AbilityDataVM),"TooltipObject",7);}
        private static void Postfix(AbilityDataVM __instance,ref object __result)
        {if(__instance is ElementalConsoleBookPresentation.PreparationHeader)__result=null;}
    }
}
