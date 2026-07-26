# Known issues

- Version 0.0.29 is a disposable-save complete-maintenance-loop smoke-test candidate, not a general release.
- The Test Musket still displays and animates as a Heavy Crossbow.
- Black Powder Charges, Lead Balls, and Firearm Repair Kits still use placeholder native-item artwork.
- The stable action blueprint names still say Test Musket for compatibility, but their runtime target selection is generic and marker-driven.
- Sprint 30 is compile-qualified but still requires its focused Kingmaker runtime gate before the early firearm catalog begins.
- The one-command maintenance qualification runner deliberately bypasses action economy. It is a regression diagnostic, not gameplay; full-round cancellation and delivery still require the separate action-bar test.
- The process-local maintenance baseline becomes invalid if the exact target or second fixture item is removed, replaced, unequipped into an unobserved location, or mutated outside the expected sequence. Clear and prepare a fresh fixture rather than interpreting a stale FAIL matrix.
- The full-round interruption guarantee requires live qualification in Kingmaker. Overhaul and Repair transactions mutate only during ability delivery.
- Ordinary Repair has no skill check, gold charge, vendor service, or time beyond its full-round action yet. The current cost is exactly one Firearm Repair Kit.
- The five-foot burst uses Kingmaker's native mechanics distance, unit corpulence, targetability, and line-of-sight query. A unit whose visual center appears slightly beyond five feet may still qualify because occupied space is part of the native distance rule.
- A failed per-target native save/damage delivery does not roll back the already-committed empty/Wrecked firearm state and is not broadly retried. Diagnostics record the partial failure.
- The destructive development cleanup can remove all unequipped Test Muskets, but it requires a separate arm and confirm action. Arming or cancelling is non-mutating.
- Natural-roll forcing is development-only and applies only to the next eligible exact firearm main attack roll. It never forces Reflex saves or native Heavy Crossbow rolls.
- Process-local counters, repository labels, runtime reference hashes, and the maintenance qualification baseline reset when Kingmaker exits. Item-owned token state is the durable persistence evidence across restart.
