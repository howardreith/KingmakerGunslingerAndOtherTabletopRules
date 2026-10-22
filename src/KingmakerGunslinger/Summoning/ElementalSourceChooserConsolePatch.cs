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
            "Choose a rank, element and original spell source. Confirm selects a source to pin; Use casts the explicitly selected source. Original rules and budgets remain separate.":Source.Cost;}
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
            var retained=new List<ActionBarSelectorItemVm>();int first=-1;
            foreach(var item in original)
            {
                bool consolidate=false;
                try{consolidate=ElementalSourceChooserRuntime.Collect(temporary,item.MechanicActionBarSlot,0);}
                catch(Exception error){temporary.Sources.RemoveAll(value=>ReferenceEquals(value.Mechanism,item.MechanicActionBarSlot));
                    Outcome="preserve-console-source-inspection:"+error.GetType().Name;}
                if(consolidate){if(first<0)first=retained.Count;removed.Add(item);}
                else retained.Add(item);
            }
            if(temporary.Sources.Count==0)return;
            var context=new ElementalConsoleContext {Owner=owner,CurrentVm=vm,Unit=unit,Books=unit.Descriptor.Spellbooks.ToArray()};
            context.Sources.AddRange(temporary.Sources);
            var launch=Node(context,"Elementals",0,null,null);
            var itemVm=new ActionBarSelectorItemVm(launch,vm.IsPossibleActive);
            retained.Insert(first<0?retained.Count:first,itemVm);
            if(!vm.ItemVms.SequenceEqual(original) || !ReferenceEquals(owner.Unit.Value,unit))
            {itemVm.Dispose();Outcome="preserve-foreign-console-collection";return;}
            Contexts.Add(vm,context);
            try{vm.ItemVms.Clear();vm.ItemVms.AddRange(retained);}
            catch
            {
                if(vm.ItemVms.SequenceEqual(retained.Take(vm.ItemVms.Count)))
                {vm.ItemVms.Clear();vm.ItemVms.AddRange(original);}
                Contexts.Remove(vm);itemVm.Dispose();throw;
            }
            // Native source VM disposal happens only after the complete group
            // and every new native VM have committed successfully.
            foreach(var item in removed)item.Dispose();
            Outcome="native-console-elementals-ready;sources="+context.Sources.Count;
        }
        private static ElementalConsoleNode Node(ElementalConsoleContext context,string title,int rank,string creature,ElementalNativeSource source)
        {var node=new ElementalConsoleNode {Context=context,Unit=context.Unit,Title=title,Rank=rank,Creature=creature,Source=source};
            node.UpdateResourceCount();return node;}
        internal static bool Result(ActionBarVM owner,ref MechanicActionBarSlot mechanism,bool execute)
        {
            var node=mechanism as ElementalConsoleNode;
            if(node==null)return true;
            var context=node.Context;
            if(!ReferenceEquals(owner,context.Owner) || !context.Current)
            {Outcome="refuse-stale-console-node-no-spend";return false;}
            if(!ElementalSourceChooserRuntime.Active)
            {Restore(context);return false;}
            if(node.Source!=null)
            {
                string error=node.Source.Validate();
                if(error!=null || (execute && (Game.Instance.SelectedAbilityHandler==null || Game.Instance.SelectedAbilityHandler.Ability!=null)))
                {Outcome="refuse-console-source-no-spend:"+(error??"foreign-targeting-owner");return false;}
                // The original Result now receives exactly the same native
                // converted slot as the native popup. Its execute=false branch
                // pins that native slot; execute=true calls its native OnClick.
                var native=new MechanicActionBarSlotSpontaneusConvertedSpell {Unit=context.Unit,Spell=node.Source.Child};
                native.UpdateResourceCount();mechanism=native;
                if(execute)ElementalSourceChooserRuntime.LastDispatched=node.Source.Child;
                Outcome=execute?"native-console-explicit-source-use":"native-console-explicit-source-pin";return true;
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
                    Node(context,"Cast using: "+source.Label,node.Rank,node.Creature,source));
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
