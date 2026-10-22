using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.GameModes;
using Kingmaker.UI;
using Kingmaker.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingmakerGunslinger.Summoning
{
    // Transient UI only. No Blueprint, action-bar settings or saved object is
    // created. All source rows hold the real native mechanism and AbilityData.
    internal sealed class ElementalSourceChooserView : MonoBehaviour, ICancelHandler
    {
        private static ElementalSourceChooserView _owned;
        private ElementalChooserSession _session;
        private ElementalSourceSelection<ElementalNativeSource> _selection;
        private RectTransform _panel, _species, _sources;
        private TextMeshProUGUI _status;
        private Button _cast;
        private int _rank;
        private string _creature;
        private GameObject _previousSelection;
        private bool _closing;
        private KeyboardAccess _keyboard;
        private CountingGuard _keyboardGuard;
        private bool _keyboardGuardOwned;
        internal static bool LastKeyboardRestored = true;
        internal static string KeyboardOutcome = "not-acquired";
        internal static bool KeyboardGuardHeld {get{return _owned!=null&&_owned._keyboardGuardOwned;}}
        private readonly Dictionary<int,Button> _rankButtons=new Dictionary<int,Button>();
        private readonly Dictionary<string,Button> _elementButtons=new Dictionary<string,Button>();
        private readonly Dictionary<ElementalNativeSource,Button> _sourceButtons=new Dictionary<ElementalNativeSource,Button>();

        internal static bool IsOpen { get { return _owned != null; } }
        internal static RectTransform RenderedPanel {get{return _owned==null?null:_owned._panel;}}
        internal static string[] RenderedText {get{return _owned==null?new string[0]:_owned.GetComponentsInChildren<TextMeshProUGUI>(false)
            .Where(value=>value.gameObject.activeInHierarchy).Select(value=>value.text).ToArray();}}
        internal static ElementalSourceSelection<ElementalNativeSource> Selection
        { get { return _owned == null ? null : _owned._selection; } }

        internal static void Open(ElementalChooserSession session)
        {
            if (session==null || !session.Current || session.Sources.Count == 0 || Game.Instance.IsControllerGamepad ||
                Game.Instance.CurrentMode != GameModeType.Default || !session.Owner.gameObject.activeInHierarchy ||
                EventSystem.current == null || EventSystem.current.currentInputModule == null)
            { ElementalSourceChooserRuntime.Outcome="preserve-native-input:chooser-surface-not-ready"; return; }
            if(!CloseOwned("reopen"))return;
            GameObject root=null;
            try
            {
                root=new GameObject("KMG_ElementalSourceChooser",typeof(RectTransform),typeof(Image));
                var rect=(RectTransform)root.transform;
                rect.SetParent(session.Canvas.rootCanvas.transform,false);
                rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
                root.GetComponent<Image>().color=new Color(0,0,0,.78f);
                var view=root.AddComponent<ElementalSourceChooserView>();_owned=view;
                view._session=session;
                view._selection=new ElementalSourceSelection<ElementalNativeSource>(session.Sources,
                    ()=>session.Current,source=>ElementalSourceChooserRuntime.Validate(session,source),
                    source=>ElementalSourceChooserRuntime.Dispatch(session,source));
                view._previousSelection=EventSystem.current.currentSelectedGameObject;
                view.AcquireKeyboard();
                view.Build(); ElementalSourceChooserRuntime.Outcome="elementals-chooser-open-no-spend";
            }
            catch(Exception error)
            {
                bool restored=CloseOwned("chooser-build-refused-no-spend");
                if(root!=null)Destroy(root);
                ElementalSourceChooserRuntime.Outcome="chooser-build-refused-no-spend:"+error.Message+"; keyboardRestored="+restored;
            }
        }

        private void AcquireKeyboard()
        {
            _keyboard=Game.Instance.Keyboard;
            _keyboardGuard=_keyboard==null?null:_keyboard.Disabled;
            if(_keyboardGuard==null || _keyboardGuard.GuardCount!=0)
                throw new InvalidOperationException("Native keyboard is absent or already owned by another UI.");
            // CountingGuard.Value acquires/releases one count, not a boolean
            // assignment. Never Reset or overwrite another UI's count.
            _keyboardGuard.Value=true;_keyboardGuardOwned=true;
            LastKeyboardRestored=false;KeyboardOutcome="owned-native-keyboard-count-acquired";
            if(_keyboardGuard.GuardCount!=1)throw new InvalidOperationException("Native keyboard acquisition did not add exactly one count.");
        }

        private bool ReleaseKeyboard()
        {
            if(!_keyboardGuardOwned)return true;
            if(Game.Instance==null || !ReferenceEquals(Game.Instance.Keyboard,_keyboard) ||
                !ReferenceEquals(_keyboard.Disabled,_keyboardGuard) || _keyboardGuard.GuardCount<1)
            {KeyboardOutcome="keyboard-context-changed-no-foreign-write";LastKeyboardRestored=false;return false;}
            int count=_keyboardGuard.GuardCount;
            _keyboardGuard.Value=false;_keyboardGuardOwned=false;
            LastKeyboardRestored=_keyboardGuard.GuardCount==count-1;
            KeyboardOutcome=LastKeyboardRestored?"owned-native-keyboard-count-released":"native-keyboard-release-unverified";
            return LastKeyboardRestored;
        }

        private void Build()
        {
            var canvasRect=_session.Canvas.rootCanvas.transform as RectTransform;
            float width=Mathf.Min(1000,canvasRect.rect.width*.92f),height=Mathf.Min(820,canvasRect.rect.height*.92f);
            _panel=Box("Panel",transform,new Vector2(width,height));
            _panel.gameObject.AddComponent<Image>().color=new Color(.08f,.075f,.065f,.99f);
            Label(_panel,"Elementals",new Vector2(24,-14),new Vector2(width-48,42),30);
            Label(_panel,"Choose a rank and element, then Cast using an original spell source.",new Vector2(24,-56),new Vector2(width-48,46),19);
            var rankBar=TopBox("Ranks",_panel,new Vector2(24,-108),new Vector2(width-48,48));
            int[] ranks=_session.Sources.Select(value=>value.Rank).Distinct().OrderBy(value=>value).ToArray();
            Button first=null;
            for(int i=0;i<ranks.Length;i++)
            {
                int rank=ranks[i];
                var button=Button(rankBar,ElementalSourceChooserPolicy.RankTitle(rank),
                    new Vector2(i*(width-48)/ranks.Length,0),new Vector2((width-48)/ranks.Length-6,46),()=>ChooseRank(rank));
                _rankButtons.Add(rank,button);
                if(first==null)first=button;
            }
            _species=TopBox("Elements",_panel,new Vector2(24,-168),new Vector2(width-48,76));
            var viewport=TopBox("SourcesViewport",_panel,new Vector2(24,-256),new Vector2(width-48,height-374));
            viewport.gameObject.AddComponent<Image>().color=new Color(.12f,.115f,.10f,1);
            viewport.gameObject.AddComponent<RectMask2D>();
            _sources=TopBox("Sources",viewport,Vector2.zero,new Vector2(width-48,0));
            var layout=_sources.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;
            layout.spacing=8;layout.padding=new RectOffset(8,8,8,8);
            var fit=_sources.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=_sources;
            scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity=32;
            _status=Label(_panel,"Select an element.",new Vector2(24,-height+104),new Vector2(width-380,82),18);
            _cast=Button(_panel,"Cast",new Vector2(width-340,-height+82),new Vector2(144,54),Cast);
            Button(_panel,"Cancel",new Vector2(width-184,-height+82),new Vector2(144,54),()=>CloseOwned("cancelled-no-spend"));
            _cast.interactable=false;
            ChooseRank(ranks[0]);
            if(first!=null)EventSystem.current.SetSelectedGameObject(first.gameObject);
        }

        private void ChooseRank(int rank)
        {
            _selection.ClearSelection();
            _elementButtons.Clear();_sourceButtons.Clear();
            _rank=rank;_creature=null;Clear(_species);Clear(_sources);_cast.interactable=false;
            var keys=ElementalSourceChooserPolicy.Rank(rank).CreatureKeys;
            float width=_species.rect.width/4;
            for(int i=0;i<keys.Count;i++)
            {
                string key=keys[i]; bool present=_session.Sources.Any(value=>value.Rank==rank&&value.CreatureKey==key);
                var button=Button(_species,ElementalSourceChooserPolicy.Title(key),new Vector2(i*width,0),new Vector2(width-6,72),()=>ChooseCreature(key));
                _elementButtons.Add(key,button);
                button.interactable=present;
                var icon=_session.Sources.Where(value=>value.Rank==rank && value.CreatureKey==key)
                    .OrderBy(value=>ElementalSourceChooserPolicy.Parent(value.Source.Blueprint.AssetGuid)==null?0:1)
                    .Select(value=>value.Child.Blueprint.Icon).FirstOrDefault(value=>value!=null);
                if(icon!=null)
                {
                    var imageRect=TopBox("ElementIcon",button.transform,new Vector2(6,-8),new Vector2(54,54));
                    var image=imageRect.gameObject.AddComponent<Image>();image.sprite=icon;image.preserveAspect=true;image.raycastTarget=false;
                    var text=button.GetComponentInChildren<TextMeshProUGUI>();var textRect=(RectTransform)text.transform;
                    textRect.anchoredPosition=new Vector2(66,-5);textRect.sizeDelta=new Vector2(width-78,62);
                }
            }
            _status.text="Choose an element. Sources retain their own spellbooks and rules.";
        }

        private void ChooseCreature(string key)
        {
            _selection.ClearSelection();
            _sourceButtons.Clear();
            _creature=key;Clear(_sources);
            var choices=_session.Sources.Where(value=>value.Rank==_rank&&value.CreatureKey==key).ToArray();
            foreach(var source in choices)
            {
                var button=Button(_sources,"Cast using: "+source.Label,Vector2.zero,new Vector2(_sources.rect.width-16,64),()=>ChooseSource(source));
                _sourceButtons.Add(source,button);
                var size=button.gameObject.AddComponent<LayoutElement>();size.minHeight=64;size.preferredHeight=64;
            }
            var info=Label(_sources,"Each source keeps its native descriptors, effects, metamagic, costs and matching preparations.",Vector2.zero,new Vector2(_sources.rect.width-16,72),18);
            info.gameObject.AddComponent<LayoutElement>().minHeight=72;
            _cast.interactable=false;_status.text=ElementalSourceChooserPolicy.Title(key)+" · choose Cast using.";
            if(choices.Length==1)ChooseSource(choices[0]);
        }

        private void ChooseSource(ElementalNativeSource source)
        {
            if(!_selection.Select(source)) {_status.text=_selection.Outcome;return;}
            _status.text=source.Label;
            var old=_sources.Find("NativeSourceRules");if(old!=null)Destroy(old.gameObject);
            var text=Label(_sources,source.Cost,Vector2.zero,new Vector2(_sources.rect.width-16,240),18);
            text.name="NativeSourceRules";text.gameObject.AddComponent<LayoutElement>().preferredHeight=
                Mathf.Max(120,text.GetPreferredValues(source.Cost,_sources.rect.width-16,10000).y+16);
            _cast.interactable=ElementalSourceChooserRuntime.Validate(_session,source)==null;
        }

        private void Cast()
        {
            try {if(!_selection.Cast()&&_status!=null)_status.text=_selection.Outcome;}
            catch(Exception error){if(_status!=null)_status.text="Native handoff refused: "+error.Message;}
        }

        // Request-owned observer drives these exact rendered Button events.
        // It does not call the selection policy/dispatch seam in their place.
        internal static void ReviewChoose(int rank,string creature,ElementalNativeSource source,bool cast)
        {
            var view=_owned;
            if(view==null || !view._session.Current)throw new InvalidOperationException("No current actual chooser view.");
            view._rankButtons[rank].onClick.Invoke();
            var element=view._elementButtons[creature];
            if(!element.interactable)throw new InvalidOperationException("Actual elemental button unavailable.");
            element.onClick.Invoke();
            if(source!=null)
            {
                var button=view._sourceButtons[source];
                if(!button.interactable)throw new InvalidOperationException("Actual source button unavailable.");
                button.onClick.Invoke();
            }
            if(cast)
            {
                if(!view._cast.interactable)throw new InvalidOperationException("Actual Cast button unavailable: "+view._status.text);
                view._cast.onClick.Invoke();
            }
        }
        internal static bool ReviewTryCast()
        {
            var view=_owned;
            if(view==null || !view._session.Current || !view._cast.interactable)return false;
            view._cast.onClick.Invoke();return true;
        }

        private void Update()
        {
            if(_closing)return;
            if(!_session.Current || Game.Instance.CurrentMode!=GameModeType.Default || Game.Instance.IsControllerGamepad ||
                !_session.Owner.gameObject.activeInHierarchy)
            {CloseOwned("source-context-changed-no-spend");return;}
            if(Input.GetKeyDown(KeyCode.Escape)){CloseOwned("cancelled-no-spend");return;}
            if(_selection.Selected!=null)_cast.interactable=ElementalSourceChooserRuntime.Validate(_session,_selection.Selected)==null;
        }

        public void OnCancel(BaseEventData data){CloseOwned("cancelled-no-spend");}
        internal static bool CloseOwned(string reason)
        {
            var view=_owned;if(view==null||view._closing)return LastKeyboardRestored;
            return view.Close(reason,true);
        }
        private bool Close(string reason,bool destroy)
        {
            var view=this;
            view._closing=true;_owned=null;
            bool restored=false;
            try
            {
                if(view._selection!=null&&!view._selection.Closed)view._selection.Cancel();
                if(EventSystem.current!=null&&EventSystem.current.currentSelectedGameObject!=null&&
                    EventSystem.current.currentSelectedGameObject.transform.IsChildOf(view.transform))
                    EventSystem.current.SetSelectedGameObject(view._previousSelection!=null&&view._previousSelection.activeInHierarchy?view._previousSelection:null);
            }
            finally
            {
                restored=view.ReleaseKeyboard();
                ElementalSourceChooserRuntime.Outcome=reason+"; "+KeyboardOutcome;
                if(destroy)Destroy(view.gameObject);
            }
            return restored;
        }
        private void OnDestroy(){if(!_closing)Close("chooser-destroyed-no-spend",false);}

        private TextMeshProUGUI Label(Transform parent,string text,Vector2 position,Vector2 size,int fontSize)
        {
            var rect=TopBox("Label",parent,position,size);var label=rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font=_session.Font;label.fontSize=fontSize;label.color=new Color(.91f,.86f,.72f,1);
            label.text=text;label.enableWordWrapping=true;label.raycastTarget=false;return label;
        }
        private Button Button(Transform parent,string text,Vector2 position,Vector2 size,Action action)
        {
            var rect=TopBox("Choice",parent,position,size);var image=rect.gameObject.AddComponent<Image>();image.color=new Color(.24f,.21f,.15f,1);
            var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;button.onClick.AddListener(()=>action());
            rect.gameObject.AddComponent<ElementalChooserCancel>();
            Label(rect,text,new Vector2(8,-5),new Vector2(size.x-16,size.y-10),18);
            return button;
        }
        private static RectTransform Box(string name,Transform parent,Vector2 size)
        {var obj=new GameObject(name,typeof(RectTransform));var rect=(RectTransform)obj.transform;rect.SetParent(parent,false);rect.sizeDelta=size;return rect;}
        private static RectTransform TopBox(string name,Transform parent,Vector2 position,Vector2 size)
        {var rect=Box(name,parent,size);rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);rect.anchoredPosition=position;return rect;}
        private static void Clear(Transform root){foreach(Transform child in root){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
    }

    internal sealed class ElementalChooserCancel : MonoBehaviour,ICancelHandler
    {public void OnCancel(BaseEventData data){ElementalSourceChooserView.CloseOwned("cancelled-no-spend");}}
}
