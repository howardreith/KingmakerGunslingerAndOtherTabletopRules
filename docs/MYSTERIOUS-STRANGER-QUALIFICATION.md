# Mysterious Stranger Qualification

Version 0.0.69 adds Mysterious Stranger as a subordinate Gunslinger archetype.

Replacement rows:

- Level 1: base Grit and Quick Clear -> Charisma Grit and Focused Aim.
- Levels 2, 6, 10, 14, 18: Nimble +1 through +5 -> Lucky +1 through +5.
- Level 5: first Gun Training selection -> Stranger's Fortune.
- Level 11: Bleeding Wound -> Clipping Shot.

The level 9, 13, and 17 Gun Training choices remain available. Focused Aim uses a
swift action and fixed one-grit activation, including its Dead Shot hit multiplier.
Stranger's Fortune arms as a free action and consumes one Charisma-based daily use
only when a misfire is actually ignored. Clipping Shot arms as a free action and
spends its non-reducible one-grit cost only after a qualifying miss.

Automated evidence: focused validation passed; all 893 deterministic domain tests
passed; exact installed-reference Release compilation and strict package validation
passed; guarded Steam-backed `working-save-smoke` passed for 0.0.69. This smoke test
proves load/bootstrap/save compatibility, not full player-facing mechanical acceptance.

Manual acceptance should create a new Mysterious Stranger, inspect every replacement
row through level 20, verify Charisma controls Grit and Fortune uses, exercise Focused
Aim with ordinary and Dead Shot firearm attacks, force a misfire with Fortune armed,
and verify Clipping Shot deals half rolled damage after a miss without affecting Dead
Shot. Confirm a base Gunslinger still receives the original replaced features.

## Overnight Focused Aim transaction repair

The current candidate establishes the Focused Aim marker before debiting the
shared live Grit resource, then verifies the exact decrement. A failed or
inconsistent debit removes the marker, preventing the damage bonus from
surviving without its cost. Ordinary Focused Aim costs one Grit; a legally
selected True Grit choice reduces it to zero but still requires positive Grit.
The same authoritative resource remains exposed through the existing action-bar
resource component. Source gates pass at 1,151 tests; guarded runtime evidence
is recorded in `docs/OVERNIGHT-GUNSLINGER-BUGFIX-QUALIFICATION.md`.

## Overnight Issue 2 corrective qualification

The authoritative diagnosis supersedes the provisional reconciliation hypothesis: native `AddBuff` can return null after placing the exact Focused Aim marker in `RawFacts`. Restoring the pre-activation Grit snapshot on that null return caused the observed bonus-without-spend split. The implementation at `24ffb78ac6821d4aec173213df1f046940be683b` resolves the exact materialized marker before the transactional resource commit.

Guarded run `20260820T0458550047640Z-b20727d24cc24d3297e0e9d23d385235` passed 7/7 assertions: ordinary shared-resource spends, repeated use, zero-Grit rejection, exact True Grit selection, firearm-only damage, wrong-owner/reconciliation controls, and cleanup. Final visible-counter and save/load checks remain human-gated.
