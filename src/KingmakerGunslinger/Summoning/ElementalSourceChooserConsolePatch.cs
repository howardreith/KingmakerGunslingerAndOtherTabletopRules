using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI._ConsoleUI.ActionBar;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using UniRx;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class ElementalConsoleContext
    {
        internal ActionBarVM Owner;
        internal ActionBarSelectorVm CurrentVm;
        internal ActionBarSelectorView BoundView;
        internal ActionBarSelectorVm BoundVm;
        internal UnitEntityData Unit;
        internal Spellbook[] Books;
        internal readonly List<ElementalNativeSource> Sources=new List<ElementalNativeSource>();
        internal bool Current {get{return Owner!=null && ReferenceEquals(Owner.SelectorVm.Value,CurrentVm) &&
            ReferenceEquals(Owner.Unit.Value,Unit) && Unit!=null && !Unit.Destroyed && Unit.Descriptor.Spellbooks.SequenceEqual(Books) &&
            ReferenceEquals(BoundVm,CurrentVm) && ElementalSourceChooserConsole.IsBound(BoundView,CurrentVm);}}
    }
    internal sealed class ElementalConsoleNode : MechanicActionBarSlot
    {
        internal ElementalConsoleContext Context;
        internal int Rank;
        internal string Creature,Title;
        internal ElementalNativeSource Source;
        public override int GetResource(){return Source==null?-1:Source.Child.GetAvailableForCastCount();}
        public override bool IsCasting(){return false;}
        public override object GetContentData(){return Source==null?null:Source.Child;}
        public override Sprite GetIcon(){return Source==null?Context.Sources[0].Child.Blueprint.Icon:Source.Child.Blueprint.Icon;}
        public override Sprite GetDecorationSprite(){return null;}
        public override Color GetDecorationColor(){return Color.white;}
        public override string GetTitle(){return Title;}
        public override string GetDescription(){return Source==null?
            "Choose a rank, element and original spell source. Confirm selects a source to pin; Use casts the explicitly selected source. Original rules and budgets remain separate.":
            ElementalSourceChooserConsole.SourceDescription(Source);}
        // Console Result owns navigation/leaf delegation. A transient node must
        // never become a serialized action-bar mechanism or dispatch itself.
        public override void OnClick(){}
    }
    internal static class ElementalSourceChooserConsole
    {
        private const BindingFlags Instance=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        private static readonly FieldInfo Holder=typeof(ActionBarSelectorVm).GetField("m_Holder",Instance);
        private static readonly FieldInfo Selector=typeof(ActionBarVM).GetField("m_SelectorVm",Instance);
        private static readonly FieldInfo Title=typeof(ActionBarSelectorVm).GetField("m_Title",Instance);
        private static readonly FieldInfo InputLayer=typeof(ActionBarSelectorView).GetField("m_InputLayer",Instance);
        private static readonly PropertyInfo BoundModel=typeof(ActionBarSelectorView).GetProperty("ViewModel",Instance);
        private static readonly ConditionalWeakTable<ActionBarSelectorVm,ElementalConsoleContext> Contexts=
            new ConditionalWeakTable<ActionBarSelectorVm,ElementalConsoleContext>();
        internal static string Outcome="not-used";
        /// <summary>The last controller spell-selector collection decision (admission refusal or commit outcome).</summary>
        internal static string LastCollection="not-collected";
        internal static bool IsBound(ActionBarSelectorView view,ActionBarSelectorVm vm)
        {return view!=null && view.gameObject.activeInHierarchy && BoundModel!=null && InputLayer!=null &&
            ReferenceEquals(BoundModel.GetValue(view,null),vm) && InputLayer.GetValue(view)!=null;}
        internal static ElementalConsoleContext Context(ActionBarSelectorVm vm)
        {ElementalConsoleContext context;return vm!=null && Contexts.TryGetValue(vm,out context)?context:null;}
        internal static void Collect(ActionBarSelectorVm vm)
        {
            if(!ElementalSourceChooserRuntime.Active || !Game.Instance.IsControllerGamepad || Holder==null)return;
            var owner=Holder.GetValue(vm) as ActionBarVM;var unit=owner?.Unit.Value;
            if(unit==null || !unit.IsDirectlyControllable)return;
            var temporary=new ElementalChooserSession {Unit=unit};
            var original=vm.ItemVms.ToArray();var removed=new List<ActionBarSelectorItemVm>();
            // The one shared source policy: Collect records every elemental source
            // (standalone rows and Summon Monster/Nature's Ally rows alike) and
            // returns true only for a complete original standalone elemental row.
            foreach(var item in original)
            {
                bool consolidate=false;
                try{consolidate=ElementalSourceChooserRuntime.Collect(temporary,item.MechanicActionBarSlot,0);}
                catch(Exception error){temporary.Sources.RemoveAll(value=>ReferenceEquals(value.Mechanism,item.MechanicActionBarSlot));
                    temporary.Exceptions.Add("preserve-console-source-inspection:"+error.GetType().Name);}
                if(consolidate)removed.Add(item);
            }
            // S5 T2 / DEF-06: a launcher only for an actual consolidation of an
            // exact original row of this selector, at its first removed position.
            // Otherwise the native rows, their order and their VMs stay exactly
            // as CollectSpells built them: no launcher, no context, no disposal.
            var books=unit.Descriptor.Spellbooks.ToArray();
            ElementalConsoleContext context=null;
            var staging=ElementalConsoleLauncherPolicy.Stage<ActionBarSelectorItemVm>(original,removed,temporary.Sources.Count,()=>{
                context=new ElementalConsoleContext {Owner=owner,CurrentVm=vm,Unit=unit,Books=books};
                context.Sources.AddRange(temporary.Sources);
                return new ActionBarSelectorItemVm(Node(context,"Elementals",0,null,null),vm.IsPossibleActive);});
            if(!staging.Admitted)
            {
                LastCollection="preserve-native-console-rows:"+staging.Reason+";sources="+temporary.Sources.Count+";rows="+original.Length;
                Outcome=LastCollection;return;
            }
            // Revalidated immediately before the write. CollectSpells runs in the
            // selector's constructor, so no view is bound yet: the selector, its
            // holder, the unit and its spellbooks are the exact staged ones.
            LastCollection=ElementalConsoleLauncherPolicy.Commit<ActionBarSelectorItemVm>(staging,vm.ItemVms,()=>{
                    if(!ElementalSourceChooserRuntime.Active || !Game.Instance.IsControllerGamepad)return "module-or-controller-changed";
                    if(context==null || !ReferenceEquals(context.CurrentVm,vm))return "selector-changed";
                    if(!ReferenceEquals(Holder.GetValue(vm),owner))return "owner-changed";
                    if(!ReferenceEquals(owner.Unit.Value,unit) || unit.Destroyed || !unit.IsDirectlyControllable)return "unit-changed";
                    if(!unit.Descriptor.Spellbooks.SequenceEqual(books))return "spellbooks-changed";
                    return null;},
                ElementalConsoleLauncherPolicy.Write,()=>Contexts.Add(vm,context),()=>Contexts.Remove(vm),
                item=>item.Dispose(),item=>item.Dispose());
            Outcome=LastCollection+";sources="+temporary.Sources.Count;
        }
        private static ElementalConsoleNode Node(ElementalConsoleContext context,string title,int rank,string creature,ElementalNativeSource source)
        {var node=new ElementalConsoleNode {Context=context,Unit=context.Unit,Title=title,Rank=rank,Creature=creature,Source=source};
            node.UpdateResourceCount();return node;}
        private static string SourceTitle(ElementalNativeSource source)
        {
            // The native row is narrow and the header already names the
            // chosen elemental. Put the distinguishing book/preparation first.
            string preparation="spontaneous";
            if(source.Preparation!=null)
            {
                int ordinal=source.DebitPreparationOrdinal;
                preparation=ordinal==0?"preparation unavailable":"uses preparation "+ordinal;
            }
            return source.Book.Blueprint.DisplayName+" · level "+source.Source.SpellLevel+" · "+preparation;
        }
        internal static string SourceDescription(ElementalNativeSource source)
        {
            var details=new List<string>();
            if(source.Source.Resource!=null)
                details.Add(source.Source.Resource.LocalizedName+" × "+source.Source.ResourceCost);
            if(source.Child.MetamagicData!=null && Convert.ToInt64(source.Child.MetamagicData.MetamagicMask)!=0)
                details.Add(source.Child.MetamagicData.MetamagicMask.ToString());
            details.Add(source.Source.Name+(ReferenceEquals(source.Source.Blueprint,source.PresentationParent)?
                "":" via "+source.PresentationParent.Name));
            // Preserve the full native cost/prose string. The native detail
            // tooltip still receives the untouched original Child AbilityData.
            return string.Join(" · ",details.ToArray())+"\n"+source.Cost;
        }
        internal static bool Result(ActionBarVM owner,ref MechanicActionBarSlot mechanism,bool execute)
        {
            var node=mechanism as ElementalConsoleNode;
            var context=node==null?null:node.Context;
            string outcome;
            var route=ElementalConsoleResultPolicy.Route(node!=null,node!=null && ReferenceEquals(owner,context.Owner),
                node!=null && context.Current,ElementalSourceChooserRuntime.Active,node!=null && node.Source!=null,
                ()=>node.Source.Validate(),execute,
                ()=>Game.Instance.SelectedAbilityHandler!=null && Game.Instance.SelectedAbilityHandler.Ability==null,out outcome);
            if(route==ElementalConsoleResultRoute.NativePassThrough)return true;
            if(route==ElementalConsoleResultRoute.RefuseStale || route==ElementalConsoleResultRoute.RefuseSource)
            {Outcome=outcome;return false;}
            if(route==ElementalConsoleResultRoute.RestoreModuleOff)
            {Restore(context);return false;}
            if(route==ElementalConsoleResultRoute.PinSource || route==ElementalConsoleResultRoute.UseSource)
            {
                // The original Result now receives exactly the same native
                // converted slot as the native popup. Its execute=false branch
                // pins that native slot; execute=true calls its native OnClick.
                var native=new MechanicActionBarSlotSpontaneusConvertedSpell {Unit=context.Unit,Spell=node.Source.Child};
                native.UpdateResourceCount();mechanism=native;
                if(execute)ElementalSourceChooserRuntime.LastDispatched=node.Source.Child;
                Outcome=outcome;return true;
            }
            IEnumerable<ElementalConsoleNode> children;
            string caption;
            if(node.Rank==0)
            {
                caption="Elementals — choose rank";
                children=context.Sources.Select(value=>value.Rank).Distinct().OrderBy(value=>value).Select(rank=>
                    Node(context,ElementalSourceChooserPolicy.RankTitle(rank),rank,null,null));
            }
            else if(node.Creature==null)
            {
                caption="Elementals — choose element";
                children=ElementalSourceChooserPolicy.Rank(node.Rank).CreatureKeys.Where(key=>context.Sources.Any(value=>
                    value.Rank==node.Rank && value.CreatureKey==key)).Select(key=>Node(context,ElementalSourceChooserPolicy.Title(key),node.Rank,key,null));
            }
            else
            {
                caption=ElementalSourceChooserPolicy.Title(node.Creature)+" — choose Cast using / source to pin";
                children=context.Sources.Where(value=>value.Rank==node.Rank && value.CreatureKey==node.Creature).Select(source=>
                    Node(context,SourceTitle(source),node.Rank,node.Creature,source));
            }
            Replace(context,children.ToArray(),caption);return false;
        }
        private static void Replace(ElementalConsoleContext context,ElementalConsoleNode[] nodes,string title)
        {
            if(!context.Current || nodes.Length==0)throw new InvalidOperationException("Exact current native console chooser and nonempty children required.");
            var next=new ActionBarSelectorVm(SelectorType.None,context.Owner);
            var previous=context.CurrentVm;
            var property=(ReactiveProperty<ActionBarSelectorVm>)Selector.GetValue(context.Owner);
            try
            {
                ((ReactiveProperty<string>)Title.GetValue(next)).Value=title;
                foreach(var node in nodes)next.ItemVms.Add(new ActionBarSelectorItemVm(node,next.IsPossibleActive));
                if(!ReferenceEquals(property.Value,context.CurrentVm))throw new InvalidOperationException("Foreign console selector; not replaced.");
                // Native ViewBase.Bind first Unbinds (popping its input layer)
                // independently of VM.Dispose. Retain the previous live VM
                // until the new native Bind postfix proves success, permitting
                // exact reference rollback after partial view initialization.
                context.CurrentVm=next;Contexts.Add(next,context);property.Value=next;
                if(!context.Current || !ReferenceEquals(context.BoundVm,next))throw new InvalidOperationException("New native selector view did not bind successfully.");
                previous.Dispose();
                Outcome="native-console-source-navigation-no-spend";
            }
            catch(Exception error)
            {
                bool mayRestore=ReferenceEquals(property.Value,next) || ReferenceEquals(property.Value,previous);
                if(mayRestore)
                {context.CurrentVm=previous;if(!ReferenceEquals(property.Value,previous))property.Value=previous;}
                bool restored=mayRestore && context.Current && ReferenceEquals(context.BoundVm,previous);
                if(!ReferenceEquals(property.Value,next)){Contexts.Remove(next);next.Dispose();}
                Outcome="console-navigation-failed:"+error.GetType().Name+";originalRestored="+restored;
                if(!restored)Outcome+=";foreign-or-partial-view-not-overwritten";
            }
        }
        internal static void Restore(ElementalConsoleContext context)
        {
            if(!context.Current)return;
            var property=(ReactiveProperty<ActionBarSelectorVm>)Selector.GetValue(context.Owner);
            var previous=context.CurrentVm;
            var next=new ActionBarSelectorVm(SelectorType.Spell,context.Owner);
            try
            {
                if(!context.Current)throw new InvalidOperationException("Foreign selector before module-off restore.");
                property.Value=next;
                if(!ReferenceEquals(property.Value,next) || !IsBound(context.BoundView,next))
                    throw new InvalidOperationException("Native module-off selector did not bind.");
                previous.Dispose();Outcome="module-off-native-console-selector-restored";
            }
            catch(Exception error)
            {
                if(ReferenceEquals(property.Value,next))property.Value=previous;
                if(!ReferenceEquals(property.Value,next))next.Dispose();
                Outcome="module-off-console-restore-unverified:"+error.GetType().Name;
            }
        }
        internal static bool Exact(Type type,string method,int size)
        {var native=type.GetMethod(method,Instance);return type.Module.ModuleVersionId.ToString()==ElementalSourceChooserRuntime.GameMvid &&
            native!=null && native.GetMethodBody().GetILAsByteArray().Length==size && Holder!=null && Selector!=null && Title!=null && InputLayer!=null && BoundModel!=null;}
    }
    [HarmonyPatch(typeof(ActionBarSelectorVm),"CollectSpells")]
    internal static class ElementalConsoleSpellCollectionPatch
    {
        private static bool Prepare(){return ElementalSourceChooserConsole.Exact(typeof(ActionBarSelectorVm),"CollectSpells",508);}
        private static void Postfix(ActionBarSelectorVm __instance)
        {try{ElementalSourceChooserConsole.Collect(__instance);}
            catch(Exception error){ElementalSourceChooserConsole.Outcome="preserve-native-console-collection-failure:"+error.GetType().Name;}}
    }
    [HarmonyPatch(typeof(ActionBarVM),"Result")]
    internal static class ElementalConsoleSourceResultPatch
    {
        private static bool Prepare(){return ElementalSourceChooserConsole.Exact(typeof(ActionBarVM),"Result",67);}
        private static bool Prefix(ActionBarVM __instance,ref MechanicActionBarSlot mechanicActionBarSlot,bool execute)
        {return ElementalSourceChooserConsole.Result(__instance,ref mechanicActionBarSlot,execute);}
    }
    [HarmonyPatch(typeof(ActionBarSelectorView),"Bind")]
    internal static class ElementalConsoleSourceViewPatch
    {
        private static bool Prepare(){return typeof(ActionBarSelectorView).Module.ModuleVersionId.ToString()==ElementalSourceChooserRuntime.GameMvid;}
        private static void Postfix(ActionBarSelectorView __instance,ActionBarSelectorVm viewModel)
        {var context=ElementalSourceChooserConsole.Context(viewModel);
            if(context!=null && ElementalSourceChooserConsole.IsBound(__instance,viewModel)){context.BoundView=__instance;context.BoundVm=viewModel;}
            var lifetime=__instance.GetComponent<ElementalConsoleSourceLifetime>() ?? __instance.gameObject.AddComponent<ElementalConsoleSourceLifetime>();
            lifetime.Context=context;}
    }
    internal sealed class ElementalConsoleSourceLifetime : MonoBehaviour
    {
        internal ElementalConsoleContext Context;
        private void Update()
        {if(Context!=null && !ElementalSourceChooserRuntime.Active){var context=Context;Context=null;ElementalSourceChooserConsole.Restore(context);}}
    }
}
