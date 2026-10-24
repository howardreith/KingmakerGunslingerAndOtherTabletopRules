using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.UnitSettings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Blueprints;
using TMPro;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class ElementalNativeSource
    {
        internal MechanicActionBarSlot Mechanism;
        internal AbilityData Source, Child;
        internal BlueprintAbility PresentationParent;
        internal Spellbook Book;
        // Preparation is the native aggregate row's retained mechanism slot;
        // native SpendInternal may debit another equal preparation in this book.
        internal SpellSlot Preparation;
        internal SpellSlot DebitPreparation {get;private set;}
        internal int DebitPreparationOrdinal {get{return DebitPreparation==null?0:
            Book.GetMemorizedSpells(DebitPreparation.SpellLevel).ToList().FindIndex(value=>ReferenceEquals(value,DebitPreparation))+1;}}
        internal MetamagicData Metamagic;
        internal string CreatureKey;
        internal int Rank;
        private AbilityData _retainedChild,_retainedSource;
        private SpellSlot _retainedPreparation;
        private AbilityData _retainedDebitSpell;
        private ElementalInvocationContextSnapshot _childContext,_sourceContext;
        internal string Label { get { return Source.Name + (ReferenceEquals(Source.Blueprint,PresentationParent)?"":" via "+PresentationParent.Name) + " · " + Book.Blueprint.DisplayName +
            " · level " + Source.SpellLevel + (Child.MetamagicData==null?"":" · "+Child.MetamagicData.MetamagicMask) +
            (Preparation == null ? " spontaneous" : " matching preparation" +
                (DebitPreparationOrdinal==0?" unavailable":" (uses preparation "+DebitPreparationOrdinal+")")); } }
        internal string Cost { get { return "Available: " + Child.GetAvailableForCastCount() + " · " +
            Child.RuntimeActionType + (Child.RequireFullRoundAction ? " · full round" : "") +
            "\n" + Source.Description; } }

        internal string Validate()
        {
            UnitEntityData owner = Mechanism.Unit;
            if(!ReferenceEquals(Child,_retainedChild) || !ReferenceEquals(Source,_retainedSource) ||
                !ReferenceEquals(Preparation,_retainedPreparation) || _childContext==null || _sourceContext==null ||
                !_childContext.Current || !_sourceContext.Current || !ReferenceEquals(Child.ConvertedFrom,Source) ||
                !ReferenceEquals(Child.Caster,owner?.Descriptor) || !ReferenceEquals(Child.Spellbook,Book))
                return "Retained native invocation context changed; choose again.";
            if (owner == null || owner.Destroyed || !owner.Descriptor.Spellbooks.Any(value => ReferenceEquals(value,Book)) ||
                !ReferenceEquals(Mechanism.GetContentData(),Source) || !ReferenceEquals(Source.Spellbook,Book) ||
                !ReferenceEquals(Source.MetamagicData,Metamagic)) return "Source changed; choose again.";
            if (Preparation != null && (!ElementalSourceChooserPolicy.RetainedPreparationAvailable(Preparation,
                    (Mechanism as MechanicActionBarSlotMemorizedSpell)?.SpellSlot,true) ||
                !Book.GetMemorizedSpells(Preparation.SpellLevel).Any(value => ReferenceEquals(value,Preparation)) ||
                !ReferenceEquals(Preparation.Spell,Source) || Preparation.Type != SpellSlotType.Common)) return "Preparation changed; choose again.";
            if (Preparation != null && (!ElementalSourceChooserPolicy.RetainedPreparationAvailable(DebitPreparation,
                CurrentNativeDebitPreparation(),DebitPreparation!=null && DebitPreparation.Available) ||
                !ReferenceEquals(DebitPreparation.Spell,_retainedDebitSpell)))
                return "Native preparation debit changed or exhausted; open Elementals again.";
            if (Preparation == null && !ElementalSourceChooserRuntime.OrdinaryKnownSource(Book,Source))
                return "Known source changed; choose again.";
            var fresh = Mechanism.GetConvertedAbilityData();
            AbilityData[] exact = fresh == null ? new AbilityData[0] : fresh.Where(value => value != null &&
                ReferenceEquals(value.Blueprint,Child.Blueprint) && ReferenceEquals(value.ConvertedFrom,Source) &&
                ReferenceEquals(value.Spellbook,Book)).ToArray();
            if (exact.Length != 1 || !_childContext.Matches(ContextReferences(exact[0]),ContextValues(exact[0])) ||
                !ReferenceEquals(Child.Blueprint.Parent,PresentationParent) ||
                PresentationParent.Variants==null || PresentationParent.Variants.Count(value=>ReferenceEquals(value,Child.Blueprint))!=1)
                return "Native source variant changed; choose again.";
            if (!Child.IsAvailableForCast || Child.GetAvailableForCastCount() == 0) return "This source is unavailable or exhausted.";
            return null;
        }
        internal void CaptureContext()
        {
            _retainedChild=Child;_retainedSource=Source;_retainedPreparation=Preparation;
            _childContext=new ElementalInvocationContextSnapshot(()=>ContextReferences(Child),()=>ContextValues(Child));
            _sourceContext=new ElementalInvocationContextSnapshot(()=>ContextReferences(Source),()=>ContextValues(Source));
            CaptureDebitPreparation();
        }
        internal void CaptureDebitPreparation()
        {DebitPreparation=Preparation==null?null:CurrentNativeDebitPreparation();_retainedDebitSpell=DebitPreparation?.Spell;}
        private SpellSlot CurrentNativeDebitPreparation()
        {
            int level=Book.GetSpellLevel(Source);
            // Installed GetMemorizedSpells06001E78 retains raw order, filtering
            // only empty slots. SpendInternal06001E6E visits that order backwards
            // and uses AbilityData.Equals, not the mechanism's slot reference.
            return level<0 || level>9?null:ElementalSourceChooserPolicy.NativeDebitPreparation(
                Book.GetMemorizedSpells(level),value=>value.Available,value=>Source.Equals(value.Spell));
        }
        private static object[] ContextReferences(AbilityData data)
        {return new object[]{data.Blueprint,data.Caster,data.Spellbook,data.Fact,data.ConvertedFrom,data.MetamagicData,
            data.ParamSpellSlot,data.ParamSpellbook,data.SourceItem,data.Blueprint.ComponentsArray};}
        private static long[] ContextValues(AbilityData data)
        {var meta=data.MetamagicData;return new long[]{data.ParamSpellLevel??-1,data.OverrideDC??-1,data.OverrideSpellLevel??-1,
            data.IsSpellCopy?1:0,data.PotionForOther?1:0,meta==null?0:Convert.ToInt64(meta.MetamagicMask),
            meta==null?0:meta.SpellLevelCost,meta==null?0:meta.HeightenLevel};}
    }

    internal sealed class ElementalChooserSession
    {
        internal ActionBarGroupElement Owner;
        internal ActionBarSubGroupLevels Levels;
        internal UnitEntityData Unit;
        internal Canvas Canvas;
        internal Spellbook[] Books;
        internal TMP_FontAsset Font;
        internal bool Ready, LauncherAdded;
        internal readonly List<ElementalNativeSource> Sources = new List<ElementalNativeSource>();
        internal readonly List<string> Exceptions = new List<string>();
        internal readonly List<MechanicActionBarSlot> Consolidated = new List<MechanicActionBarSlot>();
        internal int MinimumLevel = 10;
        internal bool Current { get { return Ready && ElementalSourceChooserRuntime.Active && Owner != null &&
            ReferenceEquals(ElementalSourceChooserRuntime.SelectedField.GetValue(Owner),Unit) &&
            ReferenceEquals(ElementalSourceChooserRuntime.Session(Owner),this) &&
            Unit.Descriptor.Spellbooks.SequenceEqual(Books); } }
    }

    internal static class ElementalSourceChooserRuntime
    {
        internal const string GameMvid = "07fa1e4d-8618-41b3-9b8d-faa17d3b26f7";
        internal static readonly FieldInfo SelectedField = typeof(ActionBarGroupElement).GetField("m_Selected",BindingFlags.Instance|BindingFlags.NonPublic);
        private static readonly FieldInfo LevelsField = typeof(ActionBarGroupElement).GetField("m_Levels",BindingFlags.Instance|BindingFlags.NonPublic);
        private static readonly FieldInfo TypeField = typeof(ActionBarGroupElement).GetField("SlotType",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        private static readonly ConditionalWeakTable<ActionBarGroupElement,ElementalChooserSession> Sessions =
            new ConditionalWeakTable<ActionBarGroupElement,ElementalChooserSession>();
        internal static string Outcome = "not-opened";
        internal static AbilityData LastDispatched;
        internal static bool Active { get { return ExpandedSummoningBlueprints.Registered != null && ExpandedSummoningVisualVariantPatch.IsModuleActive; } }

        internal static ElementalChooserSession Session(ActionBarGroupElement owner)
        { ElementalChooserSession value; return owner != null && Sessions.TryGetValue(owner,out value) ? value : null; }

        internal static void Begin(ActionBarGroupElement owner)
        {
            ElementalSourceChooserView.CloseOwned("native-tab-rebuilt");
            Sessions.Remove(owner);
            if (!Active || Game.Instance.IsControllerGamepad || Game.Instance.CurrentMode!=GameModeType.Default ||
                (ActionBarSlotType)TypeField.GetValue(owner) != ActionBarSlotType.Spell) return;
            UnitEntityData unit = SelectedField.GetValue(owner) as UnitEntityData;
            Canvas canvas = owner.GetComponentInParent<Canvas>();
            TMP_FontAsset font = canvas == null ? null : canvas.GetComponentsInChildren<TextMeshProUGUI>(true)
                .Where(value => value != null && value.font != null).Select(value => value.font).FirstOrDefault();
            if (unit == null || !unit.IsDirectlyControllable || canvas == null || font == null) { Outcome = "preserve-native-tab:missing-rendered-native-surface"; return; }
            Sessions.Add(owner,new ElementalChooserSession { Owner=owner, Unit=unit, Canvas=canvas, Font=font,
                Levels=LevelsField.GetValue(owner) as ActionBarSubGroupLevels, Books=unit.Descriptor.Spellbooks.ToArray() });
            var lifetime=owner.GetComponent<ElementalChooserTabLifetime>() ?? owner.gameObject.AddComponent<ElementalChooserTabLifetime>();
            lifetime.Session=Session(owner);
        }

        // Replacement of the three exact native FillSlots AddSlot call sites,
        // never a global interception of spellbook/list/known/prepared data.
        internal static void AddDesktopSlot(ActionBarSubGroupLevels levels,int level,MechanicActionBarSlot mechanism,ActionBarGroupElement owner)
        {
            // Preserve native insertion first. Optional collection never
            // withholds a row before a complete group can commit atomically.
            levels.AddSlot(level,mechanism);
            ElementalChooserSession session = Session(owner);
            if (session == null || !ReferenceEquals(levels,session.Levels)) return;
            bool consolidate;
            try { consolidate=Collect(session,mechanism,level); }
            catch(Exception exception)
            { session.Sources.RemoveAll(value=>ReferenceEquals(value.Mechanism,mechanism));
                session.Exceptions.Add("preserve-source-inspection-failure:"+exception.GetType().Name);consolidate=false; }
            if(consolidate)session.Consolidated.Add(mechanism);
        }

        internal static void End(ActionBarGroupElement owner)
        {
            var session = Session(owner);
            if (session == null) return;
            var originals=new Dictionary<List<MechanicActionBarSlot>,MechanicActionBarSlot[]>();
            var staged=new Dictionary<List<MechanicActionBarSlot>,MechanicActionBarSlot[]>();
            var written=new List<List<MechanicActionBarSlot>>();
            try
            {
                if(session.Sources.Count==0){session.Ready=true;return;}
                if(!Active || !ReferenceEquals(SelectedField.GetValue(owner),session.Unit) ||
                    !session.Unit.Descriptor.Spellbooks.SequenceEqual(session.Books))throw new InvalidOperationException("Native tab source context changed.");
                var pairs=(System.Collections.IEnumerable)typeof(ActionBarSubGroupLevels).GetField("m_LevelButtonPairs",
                    BindingFlags.Instance|BindingFlags.NonPublic).GetValue(session.Levels);
                List<MechanicActionBarSlot> destination=null;
                foreach(object pair in pairs)
                {
                    var type=pair.GetType();
                    var slots=(List<MechanicActionBarSlot>)type.GetField("Slots").GetValue(pair);
                    originals.Add(slots,slots.ToArray());
                    if((int)type.GetField("Index").GetValue(pair)==session.MinimumLevel)destination=slots;
                }
                if(destination==null || session.Consolidated.Any(value=>originals.Values.Sum(rows=>rows.Count(row=>ReferenceEquals(row,value)))!=1))
                    throw new InvalidOperationException("Exact native source slots or destination level are missing.");
                var launch=new ElementalChooserLauncher(session);launch.UpdateResourceCount();
                foreach(var pair in originals)
                {
                    var rows=pair.Value.Where(value=>!session.Consolidated.Any(source=>ReferenceEquals(value,source))).ToList();
                    if(ReferenceEquals(pair.Key,destination))
                    {
                        int first=Array.FindIndex(pair.Value,value=>session.Consolidated.Any(source=>ReferenceEquals(value,source)));
                        rows.Insert(first<0?rows.Count:Math.Min(first,rows.Count),launch);
                    }
                    staged.Add(pair.Key,rows.ToArray());
                }
                if(originals.Any(pair=>!pair.Key.SequenceEqual(pair.Value)))throw new InvalidOperationException("Foreign native slot collection change.");
                foreach(var pair in staged)
                {written.Add(pair.Key);pair.Key.Clear();pair.Key.AddRange(pair.Value);}
                session.LauncherAdded=true;session.Ready=true;
                Outcome="desktop-tab-ready;sources="+session.Sources.Count+";exceptions="+session.Exceptions.Count;
            }
            catch(Exception error)
            {
                bool restored=true;
                foreach(var slots in written)
                {
                    if(slots.SequenceEqual(staged[slots].Take(slots.Count)))
                    {slots.Clear();slots.AddRange(originals[slots]);}
                    else restored=false;
                }
                session.Ready=false;Sessions.Remove(owner);
                Outcome="preserve-native-desktop-group-failure:"+error.GetType().Name+";originalRowsRestored="+restored;
            }
        }

        internal static bool Collect(ElementalChooserSession session,MechanicActionBarSlot mechanism,int level)
        {
            AbilityData source = mechanism == null ? null : mechanism.GetContentData() as AbilityData;
            if (source == null || source.Blueprint == null) return false;
            var memorized = mechanism as MechanicActionBarSlotMemorizedSpell;
            var spontaneous = mechanism as MechanicActionBarSlotSpontaneusSpell;
            Spellbook book = source.Spellbook;
            SpellSlot slot = memorized == null ? null : memorized.SpellSlot;
            if ((memorized == null && spontaneous == null) || book == null || source.SourceItem != null ||
                !ReferenceEquals(mechanism.Unit,session.Unit) || (slot != null && slot.Type != SpellSlotType.Common) ||
                book.Blueprint.SpellList == null || !StandaloneElementalPresentationPolicy.IsObservedOrdinaryList(book.Blueprint.SpellList.AssetGuid))
            { session.Exceptions.Add(source.Blueprint.AssetGuid + ":preserve-special-item-or-foreign-source"); return false; }
            if (spontaneous != null && !OrdinaryKnownSource(book,source))
            { session.Exceptions.Add(source.Blueprint.AssetGuid+":preserve-special-known-grant"); return false; }
            if(book.IsSpellSpecial(source) && !book.IsSpellCommon(source))
            { session.Exceptions.Add(source.Blueprint.AssetGuid+":preserve-nonordinary-access");return false; }
            List<AbilityData> children;
            try { children = mechanism.GetConvertedAbilityData(); }
            catch (Exception) { session.Exceptions.Add(source.Blueprint.AssetGuid+":preserve-native-variant-failure"); return false; }
            if (children == null) return false;
            int before = session.Sources.Count;
            foreach (AbilityData child in children)
            {
                if (child == null || !ReferenceEquals(child.ConvertedFrom,source) || !ReferenceEquals(child.Spellbook,book)) continue;
                BlueprintAbility parent=child.Blueprint.Parent;
                if(parent==null || !Candidate(parent.AssetGuid))continue;
                int rank; string key = Creature(parent,child.Blueprint,out rank);
                if(key==null || !book.Blueprint.SpellList.GetSpells(rank).Any(value=>ReferenceEquals(value,parent)))continue;
                if (key == null || session.Sources.Any(value => ReferenceEquals(value.Source,source) && value.CreatureKey == key)) continue;
                var record=new ElementalNativeSource { Mechanism=mechanism, Source=source, Child=child, Book=book,
                    PresentationParent=parent,Preparation=slot, Metamagic=source.MetamagicData, CreatureKey=key, Rank=rank };
                record.CaptureContext();session.Sources.Add(record);
                session.MinimumLevel=Math.Min(session.MinimumLevel,level);
            }
            var native = ElementalSourceChooserPolicy.Parent(source.Blueprint.AssetGuid);
            bool complete = native != null && children.Count == 4 && session.Sources.Count-before == 4 &&
                native.CreatureKeys.All(key => session.Sources.Any(value => ReferenceEquals(value.Source,source) && value.CreatureKey==key));
            if (native != null && !complete) session.Exceptions.Add(native.Guid+":preserve-incomplete-or-extra-native-conversions");
            return complete;
        }

        private static bool Candidate(string guid)
        { return StandaloneElementalPresentationPolicy.All.Any(value => value.IsParent &&
            (value.Guid==guid || value.MonsterParentGuid==guid || value.NaturesAllyParentGuid==guid)); }

        internal static bool OrdinaryKnownSource(Spellbook book,AbilityData source)
        { return Enumerable.Range(0,10).Any(level=>book.GetKnownSpells(level).Concat(book.GetCustomSpells(level))
            .Any(value=>ReferenceEquals(value,source))); }

        private static string Creature(BlueprintAbility parent,BlueprintAbility child,out int rank)
        {
            rank=0;
            string native=ElementalSourceChooserPolicy.NativeCreature(parent.AssetGuid,child.AssetGuid);
            if (native!=null) { rank=ElementalSourceChooserPolicy.Parent(parent.AssetGuid).CanonicalTier; return native; }
            if (!ExpandedSummoningPublisher.IsPublishedExpandedParent(parent)) return null;
            foreach(var spec in StandaloneElementalPresentationPolicy.All.Where(value=>value.IsParent))
                foreach(var family in new[]{SummonFamily.Monster,SummonFamily.NaturesAlly})
                {
                    if(parent.AssetGuid!=(family==SummonFamily.Monster?spec.MonsterParentGuid:spec.NaturesAllyParentGuid)) continue;
                    foreach(string key in spec.CreatureKeys)
                    {
                        var variant=ExpandedSummoningCatalog.GenerateVariants(family).Single(value=>value.ParentTier==spec.CanonicalTier &&
                            value.Multiplicity==SummonMultiplicity.One && value.Creature.Key==key);
                        Kingmaker.Blueprints.BlueprintScriptableObject actual;
                        if(SummonVisibilityCatalog.IsPublished(variant) && ExpandedSummoningBlueprints.Registered.BySymbol.TryGetValue(
                            ExpandedSummoningIdentityCatalog.AbilitySymbol(variant),out actual) && ReferenceEquals(actual,child))
                        {rank=spec.CanonicalTier;return key;}
                    }
                }
            return null;
        }

        internal static string Validate(ElementalChooserSession session,ElementalNativeSource source)
        {
            if(!session.Current || !session.Sources.Any(value=>ReferenceEquals(value,source))) return "The spell tab changed; open Elementals again.";
            if(Game.Instance.SelectedAbilityHandler==null || Game.Instance.SelectedAbilityHandler.Ability!=null)
                return "Finish or cancel the current targeting action first.";
            return source.Validate();
        }

        internal static void Dispatch(ElementalChooserSession session,ElementalNativeSource source)
        {
            // This is exactly the slot the native variant popup constructs.
            // Its native OnClick retains the child's own rules and source chain.
            var native=new MechanicActionBarSlotSpontaneusConvertedSpell { Unit=session.Unit,Spell=source.Child };
            native.UpdateResourceCount();
            if(!native.IsPossibleActive) throw new InvalidOperationException("Native selected source became unavailable.");
            if(!ElementalSourceChooserView.CloseOwned("cast-native-source"))
                throw new InvalidOperationException("Native keyboard ownership did not restore; casting refused.");
            string changed=Validate(session,source);
            if(changed!=null)throw new InvalidOperationException(changed);
            LastDispatched=source.Child; native.OnClick(); Outcome="native-source-handed-off";
        }
    }

    internal sealed class ElementalChooserTabLifetime : MonoBehaviour
    {
        internal ElementalChooserSession Session;
        private void Update()
        {
            var session=Session;
            if(session==null || ElementalSourceChooserRuntime.Active)return;
            Session=null;ElementalSourceChooserView.CloseOwned("module-off-no-spend");
            if(session.Owner!=null && ReferenceEquals(ElementalSourceChooserRuntime.SelectedField.GetValue(session.Owner),session.Unit) &&
                ReferenceEquals(ElementalSourceChooserRuntime.Session(session.Owner),session))session.Owner.Fill(session.Unit);
        }
    }

    internal sealed class ElementalChooserLauncher : MechanicActionBarSlot
    {
        private readonly ElementalChooserSession _session;
        internal ElementalChooserLauncher(ElementalChooserSession session){_session=session;Unit=session.Unit;}
        public override int GetResource(){return -1;}
        public override bool IsCasting(){return false;}
        public override object GetContentData(){return null;}
        public override Sprite GetIcon(){return _session.Sources.Count==0?null:_session.Sources[0].Child.Blueprint.Icon;}
        public override Sprite GetDecorationSprite(){return null;}
        public override Color GetDecorationColor(){return Color.white;}
        public override string GetTitle(){return "Elementals";}
        public override string GetDescription(){return "Choose an elemental, then Cast using an explicit original spell source. Each source keeps its own spellbook, preparation, metamagic and rules.";}
        public override void OnClick(){if(_session.Current)ElementalSourceChooserView.Open(_session);}
    }
}
