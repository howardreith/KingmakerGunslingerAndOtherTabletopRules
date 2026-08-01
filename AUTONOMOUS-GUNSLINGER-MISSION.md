# Autonomous Gunslinger Completion Mission

## 1. Durable objective

Continue development of Kingmaker Gunslinger from the fully qualified
autonomous-runtime baseline until the mod constitutes a release-quality
implementation of the base Pathfinder Gunslinger class and the supporting
firearm system required to play it in Pathfinder: Kingmaker.

Sprint boundaries are implementation checkpoints, not stopping conditions.

Do not stop merely because:

- one sprint has completed;
- a sprint report has been written;
- one feature branch has been qualified;
- tests fail;
- a build fails;
- a runtime scenario fails;
- the first attempted implementation is blocked;
- a runtime API is initially unclear;
- the roadmap currently ends;
- an observation scenario returns ERROR, TIMEOUT, FAIL, or AMBIGUOUS.

After each completed checkpoint, select the next incomplete dependency or
coverage item and continue automatically.

Stop only when the complete definition of done in this document is satisfied,
the active Codex usage allowance is exhausted, the execution host becomes
unavailable, or one of the genuine human-input hard stops below is reached.

## 2. Qualified starting baseline

The autonomous runtime foundation was qualified at:

- Runtime implementation commit: 4f28dcf
- Qualified documentation HEAD:
  5c92012701873421adff1fc0e127b0b3597c352c
- Qualification report:
  C:\Dev\KingmakerGunslingerLab\
  AUTONOMOUS-WORKING-SAVE-QUALIFICATION-REPORT.md

Before continuing, verify that the current HEAD contains that qualified
history or is a descendant of it. Require a clean working tree.

Create or continue work on:

codex/complete-gunslinger

Do not work directly on main or master. Do not rewrite Git history. Do not
force-push, rebase published work, or discard qualified commits.

Commit each source-qualified checkpoint. Do not wait for human approval after
ordinary successful checkpoint commits.

## 3. Source-of-truth priority

When rules, implementation plans, or prior decisions appear to disagree, use
this precedence:

1. This mission and repository AGENTS.md safety requirements.
2. Explicit current project decisions, architecture documents, ADRs, fidelity
   matrices, and qualified runtime reports.
3. ROADMAP-NEXT.md and current planning/entry-criteria documents.
4. Legally obtained local tabletop rules material under:
   C:\Dev\KingmakerGunslingerLab\private\rules
5. Current repository tests and already qualified observable behavior.
6. Reference-mod documentation, used as precedent rather than proof.
7. The narrowest faithful Kingmaker adaptation supported by runtime evidence.

Existing explicit architectural decisions override a literal tabletop
implementation when Kingmaker cannot represent the tabletop interaction
directly.

Never silently change an existing project rule. Record any adaptation in the
fidelity matrix.

Do not commit proprietary game assemblies, private reference bundles,
copyrighted rulebooks, raw save files, credentials, machine-local paths, or
private source material.

## 4. Scope tier

Scope tier: SHIPPABLE BASE GUNSLINGER

Mandatory scope includes the following.

### 4.1 Class integration

- A functional base Gunslinger class across levels 1 through 20.
- Correct class chassis, progression, proficiencies, skills, hit die, attack
  progression, saves, and presentation.
- Character creation integration.
- Level-up integration.
- Multiclass progression behavior.
- Respec behavior wherever Kingmaker normally supports respec.
- Stable blueprint identifiers and save compatibility.
- Player-facing names, descriptions, icons or approved fallback icons,
  tooltips, localization, and progression display.

### 4.2 Class features

Account explicitly for every base-class feature, including:

- firearm proficiency;
- Gunsmith or the established Kingmaker equivalent;
- grit pool calculation, expenditure, refresh, and recovery;
- every base-class deed at its appropriate progression point;
- bonus feats;
- Nimble;
- Gun Training;
- True Grit;
- capstone behavior;
- any other base-class progression feature found in the authoritative rules
  sources.

Each deed or feature must be classified in:

planning/GUNSLINGER-FIDELITY-MATRIX.md

Allowed classifications:

- EXACT — tabletop behavior maps directly and is implemented faithfully.
- ADAPTED — tabletop behavior has no direct CRPG equivalent and has a
  documented Kingmaker adaptation preserving its intent and balance.
- OMITTED-NO-MEANINGFUL-INTERACTION — permitted only when the tabletop feature
  has no meaningful Kingmaker interaction, with a specific explanation and
  compensating scope decision where appropriate.
- BLOCKED — temporary only; no mandatory row may remain BLOCKED at completion.

Do not silently omit a class feature.

### 4.3 Grit and deeds

The complete system must account for:

- maximum grit;
- current grit;
- minimum and maximum bounds;
- qualifying refresh/recovery events;
- firearm critical and killing-blow recovery where applicable;
- per-rest or daily behavior where applicable;
- deed availability by class level;
- deed prerequisites;
- action economy;
- targeting;
- cost;
- toggles versus activated abilities;
- pre-shot adaptations where post-hit tabletop decisions are not representable;
- duplicate-event protection;
- save/load and respec behavior;
- interaction with multiple firearms;
- interaction with broken, wrecked, empty, or incompatible firearms.

Grit and deed state must not be incorrectly shared between unrelated units or
items.

### 4.4 Firearm rules

Maintain and complete the existing item-owned firearm architecture.

The coverage matrix must explicitly account for:

- early versus advanced firearms;
- one-handed versus two-handed firearms;
- range increments and maximum legal range;
- distance-aware touch-AC penetration;
- ordinary AC outside penetration range;
- preservation of concealment, mirror images, cover, line of sight, range
  penalties, critical rules, and normal weapon damage;
- firearm proficiency and nonproficiency behavior;
- loaded state;
- ammunition identity;
- capacity greater than one;
- partial loading where supported;
- compatible and incompatible ammunition;
- reload action economy;
- interrupted or rejected reloads consuming nothing;
- inventory-backed ammunition transactions;
- exactly one loaded chamber consumed per valid projectile;
- empty-firearm behavior;
- natural-roll misfire behavior;
- normal-to-broken and broken-to-wrecked transitions;
- burst or explosion behavior;
- wielder and nearby-target handling;
- repair and overhaul behavior;
- repair and overhaul resource consumption;
- scatter weapons;
- critical ranges and multipliers;
- special ammunition where already included by the roadmap;
- equipment switching;
- two otherwise identical firearms retaining independent state;
- inventory, stash, companion-transfer, loot, sale, copy, and reconstruction
  behavior where those paths are supported;
- save/load persistence;
- schema migration;
- removal or corruption diagnostics;
- conservative recovery from impossible state.

A firearm remains an ordinary weapon in the native attack pipeline. Do not
convert firearm attacks into spells merely to obtain touch AC.

### 4.5 Supporting playable content

Include the minimum complete content necessary for a player to create and play
a Gunslinger:

- usable firearm blueprints;
- ammunition and required crafting/resource items;
- repair and overhaul kits where required;
- player-facing reload, repair, overhaul, deed, and diagnostic actions;
- reliable access to initial Gunslinger equipment;
- a documented acquisition path for later equipment;
- sensible fallback visuals, animations, sounds, and projectiles where custom
  assets are unavailable;
- no mandatory dependency on another gameplay mod.

### 4.6 Explicitly excluded unless the current roadmap already requires them

- Gunslinger archetypes;
- third-party Gunslinger classes or deeds;
- every firearm, ammunition type, or firearm feat ever published;
- unrelated classes;
- custom 3D firearm models;
- a custom animation controller;
- a Wrath asset bundle;
- broad compatibility with every Kingmaker version;
- compatibility with every other gameplay mod;
- controller-specific UI;
- macOS or Linux support.

Do not expand into excluded work merely because it is adjacent.

## 5. Initial audit and planning artifacts

Before implementing the next feature, create or reconcile:

- planning/GUNSLINGER-COVERAGE-MATRIX.md
- planning/GUNSLINGER-FIDELITY-MATRIX.md
- planning/AUTONOMOUS-IMPLEMENTATION-PLAN.md
- AUTONOMOUS-GUNSLINGER-JOURNAL.md
- AUTONOMOUS-BLOCKERS.md
- AUTONOMOUS-RESUME.md

The coverage matrix must list every mandatory feature and rule area with:

- authoritative source;
- current status;
- implementation location;
- deterministic tests;
- runtime scenario;
- most recent evidence;
- remaining work;
- final disposition.

Allowed status values:

- NOT-STARTED
- INVESTIGATING
- SOURCE-IMPLEMENTED
- SOURCE-QUALIFIED
- RUNTIME-QUALIFIED
- ADAPTED-ACCEPTED
- BLOCKED
- COMPLETE

Do not infer completion merely because a related lower-level subsystem exists.

Read all current roadmap, planning, architecture, decision, report, and
qualification documents. Reconstruct the actual implementation state from
source and tests rather than trusting obsolete milestone labels.

If the existing roadmap ends before the coverage matrix is complete, create
additional internal milestones derived from the remaining matrix. Continue
without requesting permission merely to create the next sprint.

## 6. Autonomous checkpoint loop

Repeat this loop until the definition of done is satisfied.

### Step 1 — Select work

Choose the highest-dependency incomplete vertical slice that can be implemented
and tested meaningfully.

Prefer a narrow end-to-end slice over a large horizontal rewrite.

Do not repeatedly polish the autonomous harness. It is qualified infrastructure.
Modify it only when a concrete feature acceptance test requires a capability it
does not yet possess.

### Step 2 — Establish acceptance criteria

Before implementation, record:

- tabletop or project rule;
- Kingmaker adaptation if needed;
- observable behavior;
- deterministic tests;
- required runtime evidence;
- explicit non-goals;
- rollback or failure behavior.

### Step 3 — Implement

Match existing repository style and architecture.

Keep domain rules independent of reflection and Unity when practical.

Use narrowly scoped services and adapters.

Prefer exact runtime contracts over broad reflection enumeration.

Never broaden a Harmony patch or reflection sweep merely to make a test pass.

Preserve native game behavior unless the feature explicitly requires a change.

### Step 4 — Test

Add realistic deterministic tests around observable behavior.

Run focused tests after each meaningful change.

Before checkpoint completion run every applicable regression suite, including:

- PowerShell parser validation;
- focused feature tests;
- existing firearm-state tests;
- ammunition and maintenance tests;
- persistence and identity tests;
- combat-rule tests;
- runtime request/result/orchestrator tests;
- Steam/process/deployment safety tests;
- top-level repository validation;
- the complete dependency-free domain suite;
- clean Release build;
- strict standalone package validation;
- deterministic qualification;
- scenario WhatIf;
- git diff --check;
- tracked, staged, generated, binary, save, credential, and private-material
  audits.

Do not add network-dependent tests as mandatory local gates.

### Step 5 — Runtime qualify

Before feature-specific runtime acceptance, require mod-load-smoke PASS for the
current exact assembly.

Use the qualified autonomous working-save command:

.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario working-save-smoke `
  -ExpectedVersion 0.0.30 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true `
  -Confirm:$false

Update ExpectedVersion according to repository versioning when the project
version advances.

For each feature requiring live validation, implement a guarded deterministic
runtime scenario.

A feature runtime scenario may perform only the narrowly required in-memory
actions on KMG_AUTOMATION_WORKING, such as:

- granting test-only class levels or features;
- granting disposable test items;
- equipping a test firearm;
- reloading;
- firing at an approved disposable test target;
- forcing an allowlisted deterministic roll;
- applying or observing firearm condition;
- spending or restoring grit;
- invoking the feature being qualified.

These actions must not be persisted.

Never:

- load KMG_AUTOMATION_BASELINE;
- save;
- quicksave;
- autosave deliberately;
- overwrite;
- rename;
- delete;
- copy;
- migrate;
- alter raw save files;
- parse raw save archives;
- launch Kingmaker.exe directly;
- send mouse or keyboard input from PowerShell;
- use UI coordinates;
- rely on Continue or newest-save assumptions.

Launch only through Steam App ID 640820.

Create exactly one deployment backup per live run.

Preserve loaded-build identity checks, request guards, atomic evidence writing,
save-write sentinels, baseline rejection, exact descriptor correlation, final
result flushing, and structured non-PASS evidence.

A risky runtime checkpoint requires two consecutive PASS runs from independent
fresh Kingmaker processes before it is marked RUNTIME-QUALIFIED.

### Step 6 — Diagnose failures autonomously

An ordinary failure is not a reason to stop.

On ERROR, FAIL, TIMEOUT, or AMBIGUOUS:

1. Read the exact structured evidence.
2. Identify the first failed stage or invariant.
3. Compare against the last known PASS.
4. Determine whether the defect is source, orchestration, deployment, loaded
   identity, timing, runtime contract, fixture, or acceptance logic.
5. Implement the narrowest evidence-supported repair.
6. Rerun applicable source qualification.
7. Commit the repair.
8. Retry autonomously.

After two materially different failed repairs, do not blindly attempt a third
variation of the same theory. Change mode:

- gather narrower evidence;
- implement a non-initiating observation;
- inspect exact installed contracts;
- reduce the scenario;
- reassess the architecture.

Continue if a safe evidence-acquisition path remains.

Do not manufacture proof. A previous observation, call stack, visible label, or
reflection candidate is not by itself proof of one safe callable receiver.

### Step 7 — Commit and continue

After all applicable checkpoint gates pass:

- update the coverage matrix;
- update the fidelity matrix;
- update the journal;
- update AUTONOMOUS-RESUME.md;
- write the checkpoint report;
- commit source and documentation;
- require a clean tree;
- immediately begin the next incomplete checkpoint.

Do not stop merely to announce that the checkpoint passed.

## 7. Human-input policy

Do not ask the human for:

- routine implementation choices;
- naming choices that follow repository conventions;
- test design;
- branch naming;
- whether to fix a normal test failure;
- whether to retry after an ordinary runtime failure;
- whether to proceed to the next sprint;
- whether to write a report;
- whether to commit a qualified checkpoint;
- information already available in repository files, local rules material,
  runtime evidence, installed contracts, or prior reports.

Use the source hierarchy and choose the smallest faithful, reversible,
well-tested implementation.

Human input is absolutely necessary only when one of these is true:

1. A player-facing rules decision has multiple materially different outcomes,
   no authoritative local source or existing project decision resolves it, and
   choosing incorrectly would alter class balance or compatibility.
2. A legal or licensing decision is required.
3. A required credential, proprietary asset, or external file does not exist
   locally and cannot be generated or replaced lawfully.
4. A destructive or irreversible action outside the authorized repository,
   lab, live-mod, and disposable working-save boundaries is genuinely required.
5. A necessary test requires physical human perception or interaction and no
   safe deterministic or in-process observation can establish the result.
6. The execution platform or sandbox prevents required authorized operations
   and no safer local alternative exists.
7. Continuing would risk KMG_AUTOMATION_BASELINE, a non-disposable save, user
   data, credentials, or unrelated files.
8. The active Codex usage allowance is exhausted.

Before stopping for human input:

- exhaust local documentation and evidence;
- attempt a safe narrow observation where applicable;
- rule out a reversible adaptation;
- preserve a clean repository;
- write HUMAN-INPUT-REQUIRED.md containing:
  - exact blocker;
  - evidence;
  - why it cannot be resolved autonomously;
  - the smallest precise question;
  - available choices;
  - recommended choice;
  - exact continuation command.

Ask only that one precise question.

## 8. Context and token durability

Do not rely on chat memory as the only record.

After each meaningful checkpoint, update AUTONOMOUS-GUNSLINGER-JOURNAL.md with:

- branch and HEAD;
- completed work;
- tests and hashes;
- runtime evidence paths;
- current coverage percentage;
- unresolved failures;
- next exact action.

Keep AUTONOMOUS-RESUME.md continuously usable. It must contain:

- durable objective;
- branch and HEAD;
- current checkpoint;
- last qualified baseline;
- last runtime run IDs;
- current first failing invariant, if any;
- exact commands already run;
- next command;
- applicable safety boundaries.

Before context compaction, a model switch, or anticipated usage exhaustion,
flush both files and commit any qualified work.

Use subagents sparingly:

- appropriate for read-only rules audits, repository mapping, independent test
  review, and log analysis;
- one primary writer owns source changes;
- do not allow multiple agents to edit overlapping code simultaneously;
- summarize subagent results into durable files rather than flooding the main
  thread with raw logs.

## 9. Definition of done

The mission is complete only when all of the following are true:

1. Every mandatory row in GUNSLINGER-COVERAGE-MATRIX.md is COMPLETE,
   RUNTIME-QUALIFIED, or ADAPTED-ACCEPTED.
2. No mandatory row is NOT-STARTED, INVESTIGATING, BLOCKED, or UNKNOWN.
3. Every base-class feature is represented in the fidelity matrix.
4. The base Gunslinger is usable from character creation through level 20.
5. Grit, deeds, progression, firearm attacks, touch AC, ammunition, reload,
   misfire, damage states, explosion, repair, overhaul, persistence, and
   equipment transfer work together.
6. Multiple firearms retain independent item-owned state.
7. No known critical or major defect remains.
8. All applicable source tests pass.
9. The complete dependency-free domain suite passes.
10. Clean deterministic Release builds agree.
11. Strict install package validation passes.
12. Mod-load-smoke passes for the final assembly.
13. The final comprehensive autonomous Gunslinger acceptance scenario passes
    twice consecutively from fresh independent processes.
14. No unexpected save-writing API is observed.
15. KMG_AUTOMATION_BASELINE remains untouched.
16. Installation, removal warnings, compatibility claims, adaptations, and
    known limitations are documented.
17. The release package, checksums, changelog, coverage matrix, fidelity matrix,
    test report, and runtime qualification report are complete.
18. The final repository is clean on the dedicated completion branch.

At completion, create:

C:\Dev\KingmakerGunslingerLab\
COMPLETE-GUNSLINGER-QUALIFICATION-REPORT.md

The report must include:

- starting and final commits;
- implemented scope;
- excluded scope;
- coverage and fidelity summaries;
- test totals;
- build and package hashes;
- runtime PASS run IDs and evidence paths;
- save-write and baseline-protection evidence;
- known limitations;
- installation artifact path;
- exact recommended merge or release command.

End with:

COMPLETE BASE GUNSLINGER QUALIFIED