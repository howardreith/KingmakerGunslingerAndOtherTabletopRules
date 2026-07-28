# Kingmaker Gunslinger Agent Instructions

## Project scope

This repository contains the Pathfinder: Kingmaker Gunslinger mod.

Work only on explicitly assigned Gunslinger backlog items. Do not redesign
unrelated game systems or implement speculative features.

## Source baseline

The repository is reconstructed through Sprint 30.

Do not begin Sprint 31 or later work until the Sprint 30 in-game runtime
qualification passes.

Never treat a successful build or domain-test run as proof of correct in-game
behavior.

## Required workflow

For every source change:

1. Inspect the existing implementation and documentation.
2. Make one narrowly scoped change.
3. Add or update focused tests.
4. Run repository validation.
5. Run the complete domain-test suite.
6. Run a clean Release build.
7. Validate the installable package.
8. Perform the documented in-game scenario when runtime testing is authorized.
9. Record exact evidence and any uncertainty.
10. Commit only when all required checks pass.

## Runtime testing

On Windows 10, autonomous runs must use the guarded
`-kmgRuntimeTestRequest` mechanism documented in
`docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md`. Computer Use, screenshots, OCR,
mouse coordinates, and visual UI navigation are not valid sources of
mechanical runtime correctness. Screenshots may be retained only as optional
supporting evidence.

Every real Kingmaker runtime launch must go through Steam App ID 640820 so
Steam DLC entitlement detection is preserved. Direct `Kingmaker.exe` launch
is not a valid save-backed qualification environment. If known-good saves all
show `DLC Required`, stop and report a launch-environment failure; do not
modify the saves.

Use only named disposable test saves.

Never overwrite:

- KMG_AUTOMATION_BASELINE

Use this working save for automated testing:

- KMG_AUTOMATION_WORKING

The supervised, read-only `observe-manual-save-load` procedure is documented in
`docs/SAVE-LOAD-OBSERVATION.md`. It never authorizes automated save selection or
save loading; a human must initiate the working-save load through the normal UI.

Stop rather than guessing when:

- The wrong mod version is loaded.
- The save cannot be positively identified.
- Test prerequisites are missing.
- Steam shows credentials, purchases, cloud conflicts, updates, or unexpected dialogs.
- The observed result is ambiguous.
- Two materially different attempts at the same implementation or UI interaction fail.

An ambiguous runtime result counts as a failure.

## Git safety

- Never commit directly to master or main.
- Work on a dedicated feature or qualification branch.
- Never merge branches autonomously.
- Never force-push.
- Never rewrite history.
- Never use destructive reset or clean commands without explicit authorization.
- Do not commit generated packages, raw or machine-local runtime artifacts,
  saves, credentials, machine-local configuration, or proprietary
  Kingmaker/Unity assemblies. Commit curated evidence only when the assigned
  backlog item or qualification workflow explicitly requires it.

## Design authority

Do not make autonomous balance decisions or invent missing tabletop-to-Kingmaker
adaptations.

Record unresolved design questions for human review.

## Machine safety

Do not access browsers, email, password managers, personal documents, unrelated
repositories, or other games.

Do not install software or use network access unless the task explicitly
authorizes it.
