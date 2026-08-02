# Sprint 76 entry criteria: Startling Shot applied-fact reconciliation

Two guarded attempts established that native `BuffCollection.AddBuff` may return
null for the detached target even when Kingmaker installs the exact requested
timed fact. Independent Targeting Head evidence confirmed the same native
contract and qualified exact-blueprint reconciliation through
`BuffCollection.RawFacts`.

Apply only that established reconciliation to production Startling Shot. Accept
exactly one installed `Buff` whose blueprint is the requested flat-footed buff;
if none or more than one exists, fail closed and retain the existing atomic
discharge rollback. Do not change action economy, grit, duration, condition,
targeting, or firearm-state policy.

The two-attempt limit prohibits a third guarded Startling Shot launch. Require
focused policy tests, inherited source validation, the complete domain suite,
clean Release build, and strict package validation. Record the repair as
source-qualified pending a newly authorized runtime attempt, then continue to
an independent coverage item.
