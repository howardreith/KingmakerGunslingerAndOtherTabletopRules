using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony12;
using Kingmaker;
using Kingmaker.Assets.UI.LevelUp;
using Kingmaker.UI.ServiceWindow;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingmakerGunslinger.Summoning
{
    // A fold owns only visibility of exact native rows during this one native
    // screen generation. Source objects, row callbacks, ToggleGroups, budgets,
    // preparation actions and native pagination remain the original objects.
    internal sealed class ElementalNativeSourceFold : MonoBehaviour
    {
        internal sealed class Row
        {
            internal Component Component;
            internal BlueprintAbility Blueprint;
            internal Func<BlueprintAbility> CurrentBlueprint;
            internal Func<bool> Selected;
            internal Transform Parent;
            internal bool Current { get { return Component!=null && ReferenceEquals(CurrentBlueprint(),Blueprint) &&
                ReferenceEquals(Component.transform.parent,Parent); } }
        }
        private Row[] _rows;
        private GameObject _header;
        private TextMeshProUGUI _caption;
        private Func<bool> _current;
        private Action _layout;
        private ElementalRowVisibilityLease<Row> _visibility;
        private bool _expanded,_releasing;
        internal static string Outcome="not-used";
        internal bool HasFold { get { return _header!=null; } }

        internal static void Release(Component owner)
        { var fold=owner==null?null:owner.GetComponent<ElementalNativeSourceFold>();if(fold!=null)fold.Release(); }

        internal static void Install(Component owner,IEnumerable<Row> rows,Func<bool> current,Action layout,string operation)
        {
            Release(owner);
            if(!ElementalSourceChooserRuntime.Active || Game.Instance.IsControllerGamepad)return;
            var exact=rows.Where(value=>value.Component!=null && value.Component.gameObject.activeSelf && value.Current).ToArray();
            if(exact.Length==0)return;
            Transform parent=exact[0].Parent;
            if(parent==null || exact.Any(value=>!ReferenceEquals(value.Parent,parent)) || parent.GetComponent<LayoutGroup>()==null)
            {Outcome=operation+":preserve-unverified-native-layout";return;}
            TMP_FontAsset font=exact.SelectMany(value=>value.Component.GetComponentsInChildren<TextMeshProUGUI>(true))
                .Where(value=>value.font!=null).Select(value=>value.font).FirstOrDefault();
            if(font==null){Outcome=operation+":preserve-missing-native-font";return;}
            var fold=owner.GetComponent<ElementalNativeSourceFold>() ?? owner.gameObject.AddComponent<ElementalNativeSourceFold>();
            fold._rows=exact;fold._current=current;fold._layout=layout;fold._expanded=false;
            try
            {
            fold._visibility=new ElementalRowVisibilityLease<Row>(exact.Select(row=>new ElementalRowVisibilityLease<Row>.Row {
                Native=row,ParentGuid=row.Blueprint.AssetGuid,Current=()=>row.Current,Selected=row.Selected,
                Visible=()=>row.Component.gameObject.activeSelf,SetVisible=value=>row.Component.gameObject.SetActive(value) }));
            var header=new GameObject("KMG_ElementalSourceDisclosure",typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));
            fold._header=header;var rect=(RectTransform)header.transform;rect.SetParent(parent,false);
            rect.SetSiblingIndex(exact.Min(value=>value.Component.transform.GetSiblingIndex()));
            var size=header.GetComponent<LayoutElement>();size.minHeight=48;size.preferredHeight=48;size.flexibleWidth=1;
            header.GetComponent<Image>().color=new Color(.20f,.17f,.105f,.96f);
            var label=new GameObject("Sources",typeof(RectTransform),typeof(TextMeshProUGUI));
            var textRect=(RectTransform)label.transform;textRect.SetParent(rect,false);textRect.anchorMin=Vector2.zero;textRect.anchorMax=Vector2.one;
            textRect.offsetMin=new Vector2(12,4);textRect.offsetMax=new Vector2(-12,-4);
            fold._caption=label.GetComponent<TextMeshProUGUI>();fold._caption.font=font;fold._caption.fontSize=19;
            fold._caption.color=new Color(.94f,.87f,.69f,1);fold._caption.raycastTarget=false;fold._caption.enableWordWrapping=true;
            header.GetComponent<Button>().onClick.AddListener(()=>fold.Toggle());
            fold.Apply();Outcome=operation+":native-source-fold;rows="+exact.Length;
            }
            catch
            {fold.Release();throw;}
        }
        private void Toggle()
        {if(_current==null || !_current() || _rows.Any(value=>!value.Current)){Release();return;}_expanded=!_expanded;Apply();}
        private void Apply()
        {
            int selected=0;
            foreach(var row in _rows)if(row.Selected())selected++;
            _visibility.Apply(_expanded);
            _caption.text="Elementals ("+_rows.Length+") — "+(_expanded?"hide original sources":"show original sources")+
                (selected==0?"":" · selected source remains visible");
            _layout?.Invoke();
        }
        internal void Release()
        {
            if(_releasing)return;_releasing=true;
            try
            {
                try{_visibility?.Release();}
                finally
                {
                if(_header!=null){_header.SetActive(false);Destroy(_header);}
                _header=null;_rows=null;_current=null;_caption=null;_layout=null;_visibility=null;
                }
            }
            finally{_releasing=false;}
        }
        private void Update()
        {
            if(_header==null)return;
            if(!ElementalSourceChooserRuntime.Active || _current==null || !_current() || _rows.Any(value=>!value.Current))
            {Release();return;}
            // A native allocation made after opening the fold stays visible and
            // undoable even if the current disclosure is collapsed.
            try{_visibility.Apply(_expanded);}
            catch(Exception error){Outcome="preserve-native-fold-update-failure:"+error.GetType().Name;Release();}
        }
        private void OnDisable(){Release();}
    }

    internal static class ElementalNativeRowsPresentation
    {
        private const BindingFlags Fields=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        private static readonly FieldInfo LearningSelection=typeof(CharBSelectorLayer).GetField("CurrentSpellSelectionData",Fields);
        internal static void Learning(CharBSelectorLayer layer)
        {
            ElementalNativeSourceFold.Release(layer);
            if(!ElementalSourceChooserRuntime.Active || Game.Instance.IsControllerGamepad || LearningSelection==null)return;
            var selection=LearningSelection.GetValue(layer) as SpellSelectionData;
            var controller=layer.CharBuildController?.LevelUpController;
            if(selection?.SpellList==null || selection.Spellbook==null || selection.ExtraByStat || controller?.Preview==null ||
                !ReferenceEquals(selection.Spellbook.SpellList,selection.SpellList) ||
                !StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(selection.SpellList.AssetGuid))return;
            var books=controller.Preview.Spellbooks.Where(value=>ReferenceEquals(value.Blueprint,selection.Spellbook)).ToArray();
            if(books.Length!=1)return;
            var rows=layer.GetComponentsInChildren<CharBuildSelectorItem>(true).Where(value=>value.gameObject.activeSelf &&
                ReferenceEquals(value.SelectorLayer,layer) && value.BlueprintAbility!=null &&
                ElementalSourceChooserPolicy.Parent(value.BlueprintAbility.AssetGuid)!=null &&
                selection.SpellList.GetSpells(value.SpellLevel).Any(spell=>ReferenceEquals(spell,value.BlueprintAbility))).Select(item=>
                new ElementalNativeSourceFold.Row {Component=item,Blueprint=item.BlueprintAbility,CurrentBlueprint=()=>item.BlueprintAbility,
                    Parent=item.transform.parent,Selected=()=>selection.LevelCount!=null && selection.LevelCount.Any(level=>
                        level!=null && level.SpellSelections!=null && level.SpellSelections.Any(value=>ReferenceEquals(value,item.BlueprintAbility))) }).ToArray();
            ElementalNativeSourceFold.Install(layer,rows,()=>ReferenceEquals(LearningSelection.GetValue(layer),selection) &&
                ReferenceEquals(layer.CharBuildController?.LevelUpController,controller),()=>layer.UpdateLayout(false),"learning");
        }
        internal static void Preparation(SpellBookView view)
        {
            ElementalNativeSourceFold.Release(view);
            var book=view.CurrentSpellbook;
            if(!ElementalSourceChooserRuntime.Active || book?.Blueprint.SpellList==null ||
                !StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(book.Blueprint.SpellList.AssetGuid))return;
            var rows=view.GetComponentsInChildren<SpellItem>(true).Where(value=>value.gameObject.activeSelf && value.SpellData!=null &&
                ReferenceEquals(value.SpellData.Spellbook,book) && value.SpellData.SourceItem==null && value.SlotType==SpellSlotType.Common &&
                (!book.IsSpellSpecial(value.SpellData) || book.IsSpellCommon(value.SpellData)) &&
                ElementalSourceChooserPolicy.Parent(value.SpellData.Blueprint.AssetGuid)!=null &&
                book.Blueprint.SpellList.GetSpells(ElementalSourceChooserPolicy.SourceRank(value.SpellData.Blueprint.AssetGuid))
                    .Any(spell=>ReferenceEquals(spell,value.SpellData.Blueprint))).Select(item=> {
                        AbilityData data=item.SpellData;
                        return new ElementalNativeSourceFold.Row {Component=item,Blueprint=data.Blueprint,
                            CurrentBlueprint=()=>ReferenceEquals(item.SpellData,data)?data.Blueprint:null,Parent=item.transform.parent,
                            Selected=()=>ReferenceEquals(view.SelectedSpell,data) || item.Toggle.isOn}; }).ToArray();
            ElementalNativeSourceFold.Install(view,rows,()=>ReferenceEquals(view.CurrentSpellbook,book),
                ()=>Canvas.ForceUpdateCanvases(),"preparation");
        }
        internal static bool Exact(Type owner,string method,int bytes)
        {var target=owner.GetMethod(method,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            return owner.Module.ModuleVersionId.ToString()==ElementalSourceChooserRuntime.GameMvid &&
                target!=null && target.GetMethodBody().GetILAsByteArray().Length==bytes;}
        internal static void Safe(Component owner,Action action)
        {
            try{action();}
            catch(Exception error)
            {
                string outcome="preserve-native-optional-presentation-failure:"+error.GetType().Name;
                try{ElementalNativeSourceFold.Release(owner);}
                catch(Exception restore){outcome+=";restore-unverified:"+restore.GetType().Name;}
                ElementalNativeSourceFold.Outcome=outcome;
            }
        }
    }
    [HarmonyPatch(typeof(CharBSelectorLayer),"SetupSpellSelectorView")]
    internal static class ElementalLearningRowsPatch
    {
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(CharBSelectorLayer),"SetupSpellSelectorView",377);}
        private static void Prefix(CharBSelectorLayer __instance){ElementalNativeRowsPresentation.Safe(__instance,()=>ElementalNativeSourceFold.Release(__instance));}
        private static void Postfix(CharBSelectorLayer __instance){ElementalNativeRowsPresentation.Safe(__instance,()=>ElementalNativeRowsPresentation.Learning(__instance));}
    }
    [HarmonyPatch(typeof(CharBSelectorLayer),"FillSpellLightUpdate")]
    internal static class ElementalLearningRefreshRowsPatch
    {
        private static bool Prepare(){return typeof(CharBSelectorLayer).Module.ModuleVersionId.ToString()==ElementalSourceChooserRuntime.GameMvid;}
        private static void Prefix(CharBSelectorLayer __instance){ElementalNativeRowsPresentation.Safe(__instance,()=>ElementalNativeSourceFold.Release(__instance));}
        private static void Postfix(CharBSelectorLayer __instance){ElementalNativeRowsPresentation.Safe(__instance,()=>ElementalNativeRowsPresentation.Learning(__instance));}
    }
    [HarmonyPatch(typeof(SpellBookView),"RefreshSpellsView")]
    internal static class ElementalPreparationRowsPatch
    {
        // Installed 2.1.7b RefreshSpellsView 06004838 is 731 IL bytes; the
        // former 730 pin silently left this preparation fold uninstalled.
        private static bool Prepare(){return ElementalNativeRowsPresentation.Exact(typeof(SpellBookView),"RefreshSpellsView",731);}
        private static void Prefix(SpellBookView __instance){ElementalNativeRowsPresentation.Safe(__instance,()=>ElementalNativeSourceFold.Release(__instance));}
        private static void Postfix(SpellBookView __instance){ElementalNativeRowsPresentation.Safe(__instance,()=>ElementalNativeRowsPresentation.Preparation(__instance));}
    }
}
