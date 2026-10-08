# Sprint 12 disease and allergy lifetime contract

Written 2026-10-01 under the Claude Phase 2 continuation order, which requires
the intended lifetime of the Sprint 12 illness effects to be reconciled against
the charter in a written contract before Sprint 12 can be accepted.

## 1. The two sentences being reconciled

The charter's Sprint 12 acceptance boundary says:

> No disease or allergy effect leaks beyond the summoned unit's intended
> targets or persists after cleanup.

The implemented behaviour applies the native Filth Fever disease (Dire Rat) and
a one-day allergic reaction (Goblin Dog) to victims, and both outlast the
summon, whose duration is measured in rounds or minutes.

Read naively, those appear to conflict. They do not, because "persists after
cleanup" and "the rules effect on a victim" are different subjects. This
contract separates four lifetimes that the earlier records ran together.

## 2. The four distinct lifetimes

| # | Lifetime | Owner | Ends when | Charter clause it answers to |
| --- | --- | --- | --- | --- |
| L1 | The summoned creature | The summon spell | Duration expiry, death, dismissal, area transition, module disable, reload | "Correct casting, slot use, duration, same-turn activation, dismissal, death, cleanup, area transition, and save/load" |
| L2 | A rules effect already inflicted on a victim | The victim | Its own printed duration or cure | Signature role (D-02); "Correct size, HD, ... attacks, ... and signature battlefield role" |
| L3 | Project-owned engine state created to deliver L2 | The mod | Immediately, once the effect is handed to the victim | "No disease or allergy effect leaks ... or persists after cleanup" |
| L4 | Disposable test fixtures and their victims | The guarded scenario | End of the guarded transaction | Guarded-testing restoration rules |

The charter's no-persistence line governs **L3 and L4**. It does not govern
**L2**, and it must not, because L2 is the printed rule.

## 3. Why L2 must outlive the summon

A disease contracted from a dire rat does not end when the rat dies. Neither
does damage the rat dealt. Summon durations in Kingmaker are rounds to minutes;
filth fever's printed onset alone is 1d3 days. An implementation that ended the
disease when the summon expired would therefore cancel the disease before its
onset had elapsed in every ordinary cast, which is functionally identical to
having no disease at all.

Appendix A assigns Dire Rat the battlefield role "Low-level flanker / disease
bite", and binding design decision D-02 says a creature "is not complete if the
mechanic that justifies its inclusion is absent or replaced by generic damage".
Tying L2 to L1 would delete the only mechanic that distinguishes the Dire Rat
from the Dog. The same argument applies to the Goblin Dog, whose printed
reaction explicitly lasts "1 day" and whose Appendix A row asks for a
"disease/allergic-reaction identity".

So: **L2 runs for its own printed duration, independent of the summon.** This
is a rules-fidelity requirement, not a deviation, and it is not an accepted
limitation.

## 4. What "leaks beyond the intended targets" forbids

The intended targets are exactly the creatures the printed rules expose:

- **Dire Rat** - the creature its bite hits for positive damage. One exposure
  per attack event; a replayed provider callback must not roll a second save.
- **Goblin Dog** - a non-goblinoid creature damaged by its bite; a creature
  that deals damage to it with a natural weapon or unarmed attack; a creature
  that attempts to grapple it. Each exposure is caused by that creature's own
  contact.

The charter's clause forbids widening past that set. Specifically forbidden,
and asserted against:

- No aura, area or proximity exposure. A creature standing next to a Goblin Dog
  without contacting it is never exposed.
- No contagion. An infected victim does not expose anyone else.
- No exposure of the summon's owner, the caster, the party, or any unit that did
  not itself make contact.
- No exposure of a creature the printed rule exempts: goblinoids for the Goblin
  Dog rash, and any creature with disease immunity for either effect, since
  both are disease effects.
- No self-exposure. A Goblin Dog does not expose itself, and is disease-immune
  in any case.
- No second exposure from one contact event.

## 5. What "persists after cleanup" requires (L3)

When the summon leaves - expiry, death, dismissal, area transition, module
disable or reload - the following must hold, and are the acceptance criteria
for this contract:

1. No project-owned component, buff, context or runtime table keeps a reference
   to the destroyed summon. The exposure tables are
   `ConditionalWeakTable`s keyed on the rule event, so they cannot outlive the
   event, let alone the creature.
2. An already-applied L2 effect on a surviving victim continues to tick, expire
   and cure correctly with its source unit gone. It must not throw, must not
   stall the victim's turn, and must not block a save being written or loaded.
3. Disabling the Expanded Summoning module does not break deserialization of a
   save that contains either effect.
4. No new exposure can occur after the summon is gone.
5. Repeated spawn, expose, despawn cycles return project-owned state to its
   named baseline.

Criterion 2 is the one the previous records never tested: "native disease
applied" was treated as equivalent to the whole lifecycle. It is not, and it is
the specific risk of a victim-side effect whose `MechanicsContext` names a
caster that no longer exists.

## 6. Cure and removal routes

| Effect | Printed removal | Implemented |
| --- | --- | --- |
| Dire Rat filth fever | 2 consecutive Fortitude saves; Remove Disease | The native Filth Fever blueprint's own cure lifecycle, unmodified |
| Goblin Dog rash | "Remove disease or any magical healing removes the rash instantly"; otherwise 1 day | Positive healing from an actual spell, spell-like or supernatural ability removes it; native Remove Disease removes it; ordinary and zero-value healing do not; otherwise 86,400 seconds |

No blanket cure is invented. Nothing shortens either effect on summon
departure.

## 7. Disclosed adaptations

These are intentional adaptations, recorded separately from the rules facts
above, and each is bounded:

1. **Filth Fever payload.** The Dire Rat uses the exact native Filth Fever
   blueprint rather than a project reimplementation of the printed
   onset/frequency/effect/cure line, so the disease behaves the way every other
   source of filth fever in the installed game behaves. The guarded audit's
   `FilthFever` graph in run
   `20261001T1658000459746Z-observe-expanded-summoning-native-donors` records
   what that blueprint actually is: a Disease-descriptor buff whose `NewRound`
   action takes a Fortitude save and, on failure, applies two
   `ContextActionDealDamage` ability packets while tracking consecutive
   successes through a shared value. That is the printed two-ability damage
   (1d3 Dexterity and 1d3 Constitution) and the printed two-consecutive-save
   cure. Two printed details the native blueprint does not model are the
   1d3-day onset and the 1/day frequency: Kingmaker ticks the disease per
   round instead. Both differences are the native game's own adaptation of
   filth fever, are used unmodified so the Dire Rat matches every other source
   of the disease in the installed game, and are recorded here rather than
   restated as the printed numbers. The practical effect is that the disease
   resolves faster and is cured sooner than tabletop, which makes it less
   punishing rather than more.
2. **Goblinoid exemption.** The printed rule exempts the goblinoid *subtype*.
   Kingmaker has no goblinoid subtype fact, so the exemption is the exact
   enumerated set of native goblinoid `BlueprintUnitType` asset ids the
   installed library carries, taken from the guarded unit-type census rather
   than from a name search. The census in run
   `20261001T1658000459746Z-observe-expanded-summoning-native-donors`
   enumerated all 106 unit types in the installed library and found exactly one
   goblinoid type - `Goblin`, `d524df24b2f38cf4590525b2e7c4f34e`, declared by
   69 units - with no Hobgoblin and no Bugbear type present. The enumerated set
   is therefore complete for this installation: nothing goblinoid is being left
   out. If a future installation adds such a type, the census will show it and
   the set must grow; that is why the set is a list rather than a single id.
3. **Riding contact omitted.** The printed rule's contact list includes
   "attempts to ... ride the creature". The charter's non-goals exclude
   mounted-combat functionality outright, so no ride contact exists to detect.
   This is a charter scope exclusion, not a weakening of the rule.
4. **Other combat maneuvers excluded.** Only grapple is implemented as a
   contact trigger, because the printed text names grapple and riding
   specifically. Treating trip, overrun, disarm or the dirty tricks as contact
   would broaden the rule beyond its text.

## 8. Irreducible decisions left to the owner

None. Every item above follows either the primary stat block or an explicit
charter clause. No new balance policy is introduced, so nothing in this
contract is BLOCKED.

## 9. Acceptance

Sprint 12 may be accepted on the disease and allergy axis only when sections 4
and 5 are each demonstrated by evidence that exercises the real command and
rule paths - not by a source-text check and not by observing that a native
disease buff appeared.
