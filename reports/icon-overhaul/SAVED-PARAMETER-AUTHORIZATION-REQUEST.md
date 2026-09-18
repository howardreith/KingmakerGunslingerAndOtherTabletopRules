# Saved firearm parameter verification — authorization request

Status: **SAVED_PARAMETER_ROUND_TRIP = NOT RUN — AUTHORIZATION/INPUT REQUIRED.**

Read-only inspection of the permitted `KMG_AUTOMATION_WORKING` archive (Codex,
2026-09-13/14) found no supported firearm `FeatureParam` GUIDs in any of its
eight JSON members. Catalog identity tests, in-memory selection checks and the
same-artifact `working-save-smoke` do not establish a fresh-process disk round
trip, and the current handoff explicitly forbids new save creation, overwriting,
copying campaign saves for testing, or any additional save-writing route. This
document is the concrete bounded request required before that gap can close.

## What is being requested

One explicit owner authorization (reply in chat or by editing this file) to
create and use **one new uniquely named disposable test save** through the
guarded Steam route. Nothing below may run before that authorization exists.

## Proposed fixture

| Item | Value |
|---|---|
| New save name | `KMG_AUTOMATION_FIREARM_PARAMS` (unique; never previously used) |
| Origin | Fresh level-1 Gunslinger created inside the guarded request through the same native `CustomCompanion`/creator route already qualified by the creator-regression fixture, with Weapon Focus (Pistol), Weapon Focus (Musket), Weapon Focus (Blunderbuss) and Rapid Reload (Pistol/Musket/Blunderbuss) committed, then saved **once** through the ordinary native save UI path from the guarded scenario |
| Location | The same save folder the game already uses; one new file plus its native header sidecar only |
| Protected files | `KMG_AUTOMATION_BASELINE` (never selected/loaded/written/renamed/deleted), `KMG_AUTOMATION_WORKING` (not overwritten; may be loaded read-only only through the already-qualified working-save guard if needed for staging), all campaign saves (never read or copied) |
| Process/load path | Steam App ID 640820 with the validated `-kmgRuntimeTestRequest` mechanism only; a new narrowly allowlisted scenario (or an explicitly extended `working-save-smoke` parameter set) whose parser accepts exactly this save name; no direct `Kingmaker.exe`, no unguarded entry point, no coordinate/OCR automation |
| Exact operations | (1) guarded launch, create the disposable character, verify the five committed firearm facts and their `FeatureParam` blueprint identities in memory, save once; (2) fresh guarded launch of the same immutable artifact, load exactly `KMG_AUTOMATION_FIREARM_PARAMS`, re-verify the same five facts/parameters survive the disk round trip, verify native P/M/B monogram presentation on the loaded sheet/Total list; (3) delete the test save through the ordinary native delete path only after both phases PASS or the attempt is abandoned |
| Limits | No campaign save reads; no ability activation or resource spend beyond the level-up commits; no art, prerequisite, economics or gameplay change; request-local fixtures only; automatic exit each run |
| Before/after verification | Save-folder inventory + SHA-256 of `KMG_AUTOMATION_BASELINE` and `KMG_AUTOMATION_WORKING` captured before phase 1 and re-verified after every phase; both must be byte-identical; the working save remains loadable per the standard smoke afterward |
| Cleanup/retention | The disposable save is deleted via the native UI path in the final guarded run; its pre-deletion inventory/hash and the runtime JSON evidence are retained under `runtime-evidence/<unique-run>/`; if deletion fails, the save is left in place and reported (never bulk-deleted) |

## What the fixture would and would not prove

- Would prove: fresh-process load of a save whose units carry the five supported
  firearm `FeatureParam` identities preserves those parameters and renders the
  native P/M/B presentation on the loaded facts (same-version round trip).
- Would not prove: migration of older real player saves, cross-version upgrade
  behavior, or anything about saves written by other mod versions. Existing-load
  migration remains a separate claim requiring its own fixture.

## Preferred alternative

If the owner prefers not to authorize automated save creation, an
owner-created/supervised equivalent is equally acceptable: the owner manually
creates a dedicated disposable save containing a Gunslinger with the five
firearm feats (name it `KMG_AUTOMATION_FIREARM_PARAMS` or tell us the exact
name). The guarded load/verify/delete-free phases above then run unchanged,
with no automated save write at all. This is the safer practical route and
removes the only save-writing step.
