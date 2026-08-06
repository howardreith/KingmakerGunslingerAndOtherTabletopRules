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
