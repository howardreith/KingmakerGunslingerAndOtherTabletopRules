using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Cleanup2ElementalSourceChooserTests
    {
        private sealed class Source { internal string Book; internal bool Available = true; internal long Metamagic; }
        private sealed class Preparation {internal Source Spell;internal bool Available=true;}
        // The learning/spellbook disclosure projection and row-visibility
        // lease tests were retired with those helpers (owner withdrawal,
        // 2026-11-11 and 2026-11-12); see Cleanup2ElementalPresentationTests
        // for the replacement default-hidden presentation policy.

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

            NativeAggregateRowRetainsItsExactExpectedDebit();
        }

        private static void NativeAggregateRowRetainsItsExactExpectedDebit()
        {
            var sourceA=new Source {Book="prepared book A"};
            var preparationA=new Preparation {Spell=sourceA};
            var preparationB=new Preparation {Spell=new Source {Book=sourceA.Book}};
            var otherBook=new Preparation {Spell=new Source {Book="prepared book B"}};
            var metamagic=new Preparation {Spell=new Source {Book=sourceA.Book,Metamagic=1}};
            var nativeOrder=new[]{preparationA,preparationB,otherBook,metamagic};
            // The comparison is supplied by the native AbilityData.Equals
            // boundary in production. The policy must keep both book and
            // metamagic matches separate while following native reverse order.
            Func<Preparation> currentDebit=()=>ElementalSourceChooserPolicy.NativeDebitPreparation(nativeOrder,
                value=>value.Available,value=>value.Spell.Book==sourceA.Book && value.Spell.Metamagic==sourceA.Metamagic);
            Preparation mechanismSlot=preparationA;
            int debitA=0,debitB=0,dispatches=0;
            Func<ElementalSourceSelection<Source>> open=()=>{
                Preparation expected=currentDebit();Source expectedSpell=expected?.Spell;
                return new ElementalSourceSelection<Source>(new[]{sourceA},()=>true,
                    value=>ElementalSourceChooserPolicy.RetainedPreparationAvailable(preparationA,mechanismSlot,true) &&
                        ElementalSourceChooserPolicy.RetainedPreparationAvailable(expected,currentDebit(),expected!=null && expected.Available) &&
                        ReferenceEquals(expected.Spell,expectedSpell)?null:"native-preparation-debit-changed",
                    value=>{Assertions.True(ReferenceEquals(value,sourceA),"Dispatch retains native aggregate source A.");
                        dispatches++;expected.Available=false;if(ReferenceEquals(expected,preparationA))debitA++;else debitB++;});
            };
            Assertions.True(ReferenceEquals(currentDebit(),preparationB),"Two available equal preparations use native last-available B, not the displayed row A.");
            var positive=open();positive.Select(sourceA);
            Assertions.True(positive.Cast() && preparationA.Available && !preparationB.Available && debitB==1 && debitA==0,
                "An explicit source A handoff retains its captured native B debit and leaves A untouched.");
            preparationB.Available=true;var stale=open();stale.Select(sourceA);preparationB.Available=false;
            Assertions.True(ReferenceEquals(currentDebit(),preparationA),"A remains available after captured B is exhausted.");
            Assertions.False(stale.Cast(),"An old B-bound choice cannot fall back to available A.");
            Assertions.True(ReferenceEquals(stale.Selected,sourceA) && dispatches==1 && debitA==0 && debitB==1,
                "Stale expected B neither substitutes source A nor dispatches/debits another preparation.");
            preparationB.Available=true;mechanismSlot=preparationB;
            Assertions.False(stale.Cast(),"Rebinding the native aggregate mechanism away from retained slot A refuses even with B available.");
            mechanismSlot=preparationA;preparationB.Available=false;stale.Cancel();
            var reopened=open();reopened.Select(sourceA);
            Assertions.True(reopened.Cast() && debitA==1 && debitB==1 && dispatches==2,
                "Only a genuinely reopened choice can capture the newly current native debit A.");
            Assertions.True(otherBook.Available && metamagic.Available,
                "Another book and different metamagic remain outside this source's native debit match and untouched.");
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
