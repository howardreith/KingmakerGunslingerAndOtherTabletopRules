# Favored Class Integration — Mission

This file is the durable task and boundary contract for the Gunslinger Favored
Class integration mission. It is not a completion report. Status lives in
`FAVORED-CLASS-STATE.json`, `FAVORED-CLASS-RESUME.md`,
`FAVORED-CLASS-COVERAGE.md`, `FAVORED-CLASS-BLOCKERS.md` and
`FAVORED-CLASS-IMPLEMENTATION-REPORT.md`.

## Authority

| Item | Value |
| --- | --- |
| Charter (execution reference) | `C:\Dev\KingmakerGunslingerLab\Gunslinger_Favored_Class_Integration_Charter.md` |
| Charter SHA-256 | `35e5ed5776839e8b1f4007f2d413079e26dac51ba3ab0f77ce4174be39ec7959` |
| Implementation instruction | `C:\Dev\KingmakerGunslingerLab\Gunslinger_Favored_Class_Claude_Overnight_Prompt.md` (same text the owner pasted into the session) |
| Instruction SHA-256 | `43498ee3a9b1554ea5bed5cf24fb2beb38f2484eaf34604a3b812788f22b7b45` |
| Mission start | 2026-09-23 |
| Integration owner | Claude Code session (single owner; subagents are read-only auditors) |

The owner adopted the charter's recommended implementation scope and its
explicitly stated CRPG adaptations. The charter's own statement that the
research task did not authorize implementation is superseded by this separate
implementation instruction. Neither document is evidence that any code or
runtime test has passed.

The charter is preserved unedited. Source corrections and design questions are
recorded separately in `FAVORED-CLASS-BLOCKERS.md` and the coverage ledger;
acceptance criteria are never edited to match what was implemented.

## Adopted scope

- Faithful first-party rows (22): G01 G02 G04 G05 G06 G07 G08 G10 G11 G14,
  I01 I06 I08, O01 O05 O06 O07 O08, S04 S06, U02 U04.
- Explicitly labeled CRPG adaptations (3): I05 I07 O04.
- Optional third-party profile (5, OFF by default, Jon Brazer Enterprises
  attribution): G16 G17 G18 G20 G21.
- Alias: I04 = G11 (never a second reward). G04 = G07 grit counter.
- Not published (23): P = G09 G12 G13 G22; D = G03 G15 G19 S03 S08 U06 U07;
  X = I02 I03 O02 O03 S01 S02 S05 S07 U01 U03 U05 U08. Their specifications and
  reasons stay in the catalog; none may appear as a selectable placeholder.
- Required infrastructure: optional host adapter; exact compatibility
  manifest; native host progression; canonical fractional/target accounting;
  scoped ancestry bridge; the genuine four-race Mostly Human companion racial
  feature; configuration/lifecycle policy; diagnostics; migration guidance;
  all required evidence (H01-H10, E01-E16, M01-M28, L01-L10).
- Mandatory workstreams (not stretch goals): Oracle/Sorcerer selected-power
  adapters (I06 I08 S04 S06), Oread performance range (O01), Paladin auras
  (O06), and a Mostly Human feature that is real dual-identity racial
  behavior, not an FCB unlock label.

Excluded by the charter and not added to expand the task: new races or
classes, Magical Tail, Racial/Planar Heritage feat systems, underwater combat,
global skill systems, familiar skill actors, host rebalance.

## Operational boundaries

Authorized (no further confirmation needed):

- Read-only inspection of repository source, installed game/mod assemblies,
  project configuration, public upstream sources and project diagnostics.
- Isolated worktree/branch; project code, tests, narrowly necessary harness
  support and project documentation; scoped local commits under the
  repository validation policy.
- Existing build, validation, decompilation, packaging and test tools;
  ordinary existing dependency restoration.
- Reversible local candidate installation and guarded Steam App ID 640820
  runtime tests with exact pre-change backups and restoration records.
- `KMG_AUTOMATION_WORKING` and mission-created `KMG_FCB_*` disposable fixtures
  only through mechanisms the current runtime guard allows; back up an
  existing working fixture before any permitted test write; new fixtures only
  through a documented guarded native path.

Not authorized:

- Production-save access or alteration; loading or overwriting
  `KMG_AUTOMATION_BASELINE`.
- Destructive reset/clean, force-push, history rewriting, merging into a shared
  branch, tags, public releases, pushing, PR creation or any remote write.
- Modifying another session's checkout, terminating unrelated processes,
  installing/upgrading global software, changing Unity licensing,
  account-selection hooks, security/permission settings, purchases, or using a
  different account/billing source.
- Inspecting unrelated repositories, browsers, email, credentials or personal
  files.
- Modifying Favored Class or Call of the Wild binaries or writing production
  runtime files into their folders (including `ZFavoredClass/Custom/`).
- Testing a host-dependent campaign with its dependency removed.

## Reconciliation with repository instructions

- `AGENTS.md` "GitHub checkpoint publication" requires pushing `codex/*`
  branches with the guarded push script. This mission's branch is
  `claude/favored-class-integration`; the guarded script itself refuses
  non-`codex/*` branches, and the owner explicitly withheld every remote write
  for this mission. No push is performed. This is a boundary, not a skipped
  gate.
- `AGENTS.md` "Source baseline" (Sprint 30 gate) is historical: the repository
  carries published releases through 0.0.138 with qualified guarded runtime
  channels (`working-save-smoke` qualified on `4f28dcf`). It does not block this
  mission; the current runtime guard rules still apply to every launch.
- `AGENTS.md` "Autonomous Gunslinger completion" applies only when an active
  goal references `AUTONOMOUS-GUNSLINGER-MISSION.md`. This mission does not; its
  trackers (`AUTONOMOUS-*`, `planning/GUNSLINGER-*`) are not overwritten.
- `AGENTS.md` "Icon authoring" applies to every new visible feat/trait/ability
  choice this mission adds; the icon guide and catalog are read before any
  visible choice is published.
- Historical example commands (for example `-ExpectedVersion 0.0.30`) are not
  copied; the expected version is always the actual candidate `Info.json`
  version.

## Status vocabulary

Implementation/qualification ladder per row and infrastructure item:
`NOT STARTED` → `CODE COMPLETE` → `DOMAIN TESTED` → `NATIVE TESTED` →
`SAVE TESTED` → `QUALIFIED`.

Non-pass dispositions: `BLOCKED`, `EXPECTED PROVIDER ABSENCE`, `NOT RUN`,
`OUT OF SCOPE`, `DEFERRED (P/D)`, `EXCLUDED (X)`, `ALIAS`.

No absent provider, skipped test or computed expectation becomes a PASS.
