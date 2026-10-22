using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Cleanup2ElementalSourceChooserTests
    {
        private sealed class Source { internal string Book; internal bool Available = true; }
        private sealed class NativeRow
        {internal bool Current=true,Selected,Visible=true,FailOnce;internal int Writes;}
        private static ElementalRowVisibilityLease<NativeRow>.Row Row(NativeRow native,string parent)
        {return new ElementalRowVisibilityLease<NativeRow>.Row {Native=native,ParentGuid=parent,
            Current=()=>native.Current,Selected=()=>native.Selected,Visible=()=>native.Visible,
            SetVisible=value=>{native.Visible=value;native.Writes++;if(native.FailOnce){native.FailOnce=false;throw new InvalidOperationException("native setter failure");}}};}

        internal static void RetainedInvocationDriftRefusesWithoutSubstitutingFreshChild()
        {
            var book=new object();var slot=new object();var source=new object();var child=new object();
            object convertedFrom=source;long metamagic=1;int dispatches=0;
            var context=new ElementalInvocationContextSnapshot(()=>new[]{book,slot,convertedFrom},()=>new[]{metamagic});
            var choice=new ElementalSourceSelection<object>(new[]{child},()=>true,
                value=>context.Current?null:"retained-native-context-drift",value=>dispatches++);
            choice.Select(child);
            convertedFrom=new object();
            Assertions.False(choice.Cast(),"A fresh valid factory child cannot excuse the retained child's changed source link.");
            Assertions.True(ReferenceEquals(choice.Selected,child),"The selected original child is never substituted.");
            convertedFrom=source;metamagic=2;
            Assertions.False(choice.Cast(),"In-place metamagic drift is detected despite stable object references.");
            Assertions.Equal(0,dispatches,"Neither source-context nor metamagic drift can hand off or spend.");
            metamagic=1;
            Assertions.True(choice.Cast() && dispatches==1,"Exact restored original invocation may dispatch once after explicit selection.");
        }

        internal static void NativeDisclosurePreservesMixedSourcesAndSelectedAllocations()
        {
            var native=new NativeRow();var selected=new NativeRow{Selected=true};var monster=new NativeRow();var ally=new NativeRow();
            var spec=ElementalSourceChooserPolicy.Rank(2);
            var lease=new ElementalRowVisibilityLease<NativeRow>(new[]{Row(native,spec.Guid),Row(selected,spec.Guid),
                Row(monster,spec.MonsterParentGuid),Row(ally,spec.NaturesAllyParentGuid)});
            Assertions.Equal(2,lease.Count,"Only exact standalone source rows belong to the disclosure.");
            lease.Apply(false);
            Assertions.False(native.Visible,"Unselected native standalone source folds.");
            Assertions.True(selected.Visible,"Actual selected legacy allocation stays visible and undoable.");
            Assertions.True(monster.Visible && ally.Visible && monster.Writes==0 && ally.Writes==0,
                "Mixed SM/SNA source rows must never be folded or written.");
            lease.Apply(true);Assertions.True(native.Visible,"Expansion restores the same original row.");
            lease.Release();Assertions.True(native.Visible && selected.Visible,"Release restores original native visibility.");
        }

        internal static void NativeDisclosureFailureRollsBackOnlyOwnedRowGeneration()
        {
            var first=new NativeRow();var second=new NativeRow{FailOnce=true};var spec=ElementalSourceChooserPolicy.Rank(4);
            var lease=new ElementalRowVisibilityLease<NativeRow>(new[]{Row(first,spec.Guid),Row(second,spec.Guid)});
            bool failed=false;try{lease.Apply(false);}catch(InvalidOperationException){failed=true;}
            Assertions.True(failed && first.Visible && second.Visible,"Partial native row initialization restores both exact original rows and retains failure.");
            var foreign=new NativeRow();var owned=new NativeRow();
            lease=new ElementalRowVisibilityLease<NativeRow>(new[]{Row(foreign,spec.Guid),Row(owned,spec.Guid)});
            lease.Apply(false);foreign.Current=false;int writes=foreign.Writes;lease.Release();
            Assertions.True(owned.Visible,"Owned unchanged row restores.");
            Assertions.True(!foreign.Visible && foreign.Writes==writes,"A recycled foreign row generation is never overwritten during release.");
        }

        internal static void ExplicitSourceSelectionNeverPoolsOrFallsBack()
        {
            var standalone = new Source { Book = "Wizard standalone" };
            var canonical = new Source { Book = "Wizard SM" };
            Source dispatched = null; int spends = 0;
            var session = new ElementalSourceSelection<Source>(new[] { standalone, canonical }, () => true,
                value => value.Available ? null : "native-source-exhausted", value => { dispatched = value; spends++; });
            Assertions.False(session.Cast(), "A common species cannot choose a source implicitly.");
            Assertions.True(session.Select(standalone), "Exact original source must be selectable.");
            Assertions.Equal(0, spends, "Selecting a source cannot dispatch or spend.");
            standalone.Available = false;
            Assertions.False(session.Cast(), "Exhausted selection cannot borrow canonical slots.");
            Assertions.True(dispatched == null, "No implicit fallback is allowed.");
            Assertions.True(session.Select(canonical) && session.Cast(), "Explicitly selecting the other source should use it.");
            Assertions.True(ReferenceEquals(dispatched, canonical), "Dispatch must retain the exact selected source.");
            Assertions.False(session.Cast(), "Repeated submit must not dispatch twice.");
            Assertions.Equal(1, spends, "Exactly one selected source was handed off.");
        }

        internal static void StaleForeignAndCancelledSelectionsSpendNothing()
        {
            bool current = true; int dispatches = 0;
            var source = new Source { Book = "prepared book A" };
            var otherBook = new Source { Book = "prepared book B" };
            var session = new ElementalSourceSelection<Source>(new[] { source }, () => current,
                value => null, value => dispatches++);
            Assertions.False(session.Select(otherBook), "A source from another book/session is not interchangeable.");
            session.Select(source); current = false;
            Assertions.False(session.Cast(), "Character/tab replacement must refuse the old selection.");
            current = true; session.Cancel();
            Assertions.False(session.Cast(), "Cancel must invalidate the selection.");
            Assertions.Equal(0, dispatches, "Stale, foreign and cancelled selections must never dispatch.");
        }

        internal static void AllSixRanksAndFourElementsHaveExactNativeIdentity()
        {
            foreach (int rank in ElementalSourceChooserPolicy.Ranks)
            {
                var parent = ElementalSourceChooserPolicy.Rank(rank);
                Assertions.Equal(4, parent.CreatureKeys.Count, "Each rank must preserve all four actual native children.");
                foreach (string key in parent.CreatureKeys)
                {
                    var child = StandaloneElementalPresentationPolicy.All.Single(value => value.ParentGuid == parent.Guid && value.CreatureKeys.Contains(key));
                    Assertions.Equal(key, ElementalSourceChooserPolicy.NativeCreature(parent.Guid, child.Guid), "Child must retain exact native parent identity.");
                    Assertions.True(ElementalSourceChooserPolicy.Title(key).StartsWith("Elemental, "), "Chooser uses family-first presentation.");
                    Assertions.True(ElementalSourceChooserPolicy.NativeCreature("foreign-parent", child.Guid) == null, "A child cannot be reparented by presentation.");
                }
            }
            Assertions.True(ElementalSourceChooserPolicy.NativeCreature("foreign", StandaloneElementalPresentationPolicy.WrigglingManAbilityGuid) == null,
                "The infested NPC exception must never be grouped as ordinary Elder Air.");
        }
    }
}
