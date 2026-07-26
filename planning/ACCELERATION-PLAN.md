# Delivery acceleration plan

## Problem statement

The project spent many sprints proving one runtime behavior at a time. That protected save data and prevented compounding unknown engine behavior, but it also imposed a full compile/package/manual-test cycle on very small changes.

The risk profile has changed. The following foundations are now demonstrated in Kingmaker:

- exact firearm marker isolation;
- rule-attack hooks and touch-AC selection;
- exact per-item token state and save/restart persistence;
- atomic inventory consumption and rollback;
- full-round delivery-time mutation;
- natural-d20 capture and deterministic forcing;
- condition transitions;
- native save/damage events;
- native spatial target enumeration and deduplication; and
- exact-item lifecycle/recovery.

The next phase should optimize throughput without weakening release evidence.

## 1. Make sprints larger, not qualification weaker

Every completed sprint will still receive exact-reference compilation, warnings-as-errors, three full test runs, standalone UMM packaging, checksums, source archive, and milestone archive.

The acceleration comes from redefining a sprint as a coherent vertical slice containing several related low-risk changes. Intermediate implementation checkpoints are not separately packaged or called completed sprints.

## 2. Classify changes by runtime risk

### Class A — new engine boundary

Examples: a new Harmony target, a new save-owned storage mechanism, a new native item mutation path, or a new targeting subsystem.

Policy: isolate and live-test immediately before stacking dependent behavior.

### Class B — reuse of qualified contracts

Examples: another firearm definition using the same marker/state/reload pipeline, another ability using the same full-round delivery transaction, or another deed using an established resource component.

Policy: batch several related changes into one vertical slice and one runtime gate.

### Class C — pure domain/content/presentation

Examples: immutable definition data, localization, manifests, deterministic policy tests, documentation, and non-runtime assets.

Policy: develop in parallel and qualify with the surrounding vertical slice.

## 3. Add an in-game qualification harness

Sprint 29 should add one deterministic scenario runner that can:

- create two exact firearm items and the required consumables;
- normalize one item to requested states;
- queue deterministic attack rolls;
- run or guide reload, misfire, burst, overhaul, and repair scenarios;
- snapshot item identities and inventory counts;
- summarize every relevant fault/duplicate counter; and
- emit one copyable PASS/FAIL report.

This will replace long sequences of manual button presses for regression checks while preserving a small number of human visual checks for animation, action cancellation, targeting, and combat-log behavior.

## 4. Generalize proven code before multiplying content

Sprint 30 should remove Test-Musket-specific selection and action duplication. A shared exact-firearm action service should power Reload, Overhaul, and Repair from firearm definitions. New firearm content can then be added mostly as data plus targeted tests.

## 5. Use a release-train workflow inside each sprint

1. Implement pure policy and tests first.
2. Compile once against exact private references for API feedback.
3. Integrate runtime adapters and blueprints.
4. Run the automated in-game fixture for regression evidence.
5. Perform the full deterministic compile/test/package discipline only when the vertical slice is complete.

This avoids repeatedly sealing artifacts during active implementation while retaining the same standard for anything called complete.

## 6. Keep manual runtime gates focused

Manual testing should concentrate on facts that dependency-free tests cannot prove:

- actual Harmony binding;
- real ability timing and interruption;
- save/restart persistence;
- native targeting/LOS/corpulence behavior;
- combat-log and animation presentation; and
- compatibility with the installed runtime.

Pure state transitions, rollback branches, ordering, deduplication, and validation should remain automated.

## 7. Parallelize work by lane

Future vertical slices can be prepared in parallel lanes:

- engine/runtime adapter;
- pure domain and tests;
- blueprints/content data;
- class/progression design;
- documentation and packaging validation.

Only the engine lane needs to block the others when a genuinely new contract is unresolved.

## Expected effect

The early project pace was often one visible behavior per sprint. The target from Sprint 29 onward is one meaningful player-facing package per sprint, usually comprising multiple mechanics and content changes. The exact time saved will depend on runtime defects, but the manual-test and artifact overhead should fall substantially once the scenario runner and generic action layer are in place.
