# Gunslinger Outfit Kitbash Qualification

Status: not qualified. This document is a live gate ledger, not an acceptance
claim.

## Authority

- Intake baseline: `5949165e2a6407ca480d46cd86d8944e4152e2fb`
- Feature branch: `codex/gunslinger-class-outfit-kitbash`
- Intake version: `0.0.110`
- Runtime target: Steam-preserving Pathfinder: Kingmaker 2.1.7

## Gate ledger

| Gate | State | Required evidence |
|---|---|---|
| Installed API contracts | Pass (audit stage) | Exact installed 2.1.7b assembly identity and public/reflected member findings recorded |
| Native class/resource catalog | Pass | Guarded run `20260830T2012181937219Z`, candidate set `dd81603f...03357` |
| Serious candidate renders | Pending | M/F preview-like, isometric, no-weapon, pistol, long-gun |
| Best-three scoring | Pending | Weighted rubric and hard-rejection audit |
| Race/gender coverage | Pending | Dynamically discovered supported matrix |
| Color ramps | Pending | Valid defaults and systematic valid sampling |
| Body/material integrity | Pending | Structured load data plus direct renders |
| Animation/weapon fit | Pending | Idle/walk/run/turn/fire/reload/melee evidence |
| Equipment overrides | Pending | Light/heavy armor, headgear/hair, cloak, backpack, inactive weapon |
| Preview/gameplay paths | Pending | Class-preview/API-equivalent and isometric evidence |
| Save/load/rebuild | Pending | Guarded structured evidence |
| Focused tests | Pass (render checkpoint; repeat final) | Renderer guard/catalog/matrix plus 160 runtime preflight checks |
| Repository validation | Pass (render checkpoint; repeat final) | Build-Local.ps1, 2026-08-30 |
| Complete domain suite | Pass (render checkpoint; repeat final) | 1365/1365, Release clean run |
| Clean Release build | Pass (render checkpoint; repeat final) | Exact-reference Release construction |
| Installable package | Pass (render checkpoint; repeat final) | Strict standalone UMM validation, SHA-256 693c0968...16ce8 |
| Compatibility profiles | Pending | Exact applicable command/result |
| Guarded runtime smoke | Pending | Exact request, result, build fingerprint |
| Publication | Pending | Commit(s), helper output, identical local/remote SHAs |

## Guarded catalog evidence

Command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario gunslinger-outfit-audit `
  -ExpectedVersion 0.0.110 `
  -ExitAfterCompletion:$true `
  -AllowDirtyGit `
  -Confirm:$false
```

Passing run: `runtime-evidence/20260830T2012181937219Z-gunslinger-outfit-audit`.

- result: PASS, all nine assertions;
- loaded mod: `0.0.110`;
- loaded game contract: supported version `2.1.7b`, exact
  `Assembly-CSharp.dll` SHA-256
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`,
  MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`;
- dynamically discovered race IDs: Aasimar, Dwarf, Elf, Gnome, Half-Elf,
  Half-Orc, Halfling, Human, and Tiefling, each inventoried for male and female;
- sources: 49 class, 163 item-linked, 361 bounded raw;
- inventory: 1,206 unique loaded equipment entities, 3,816 matrix rows,
  4,878 resolved links, zero unresolved links, zero inspection errors;
- state: no save-owned state, inventory, progression, or avatar mutation;
- deterministic candidate-set SHA-256:
  `dd81603f583444f335381d72cc69b73f1c036c4625e8227cb1e1f9db18603357`;
- ignored catalog SHA-256:
  `73af097a4dd21fe905d2f9b4388f2ef6a68503f4b6723040e1dd00d3e3e2e294`.

Two preceding FAIL runs are retained as diagnostic evidence. Both failed
closed, exited automatically, had zero game exceptions, and led to narrower
instrumentation. They are not acceptance evidence.

The passing launch pipeline also recorded repository validation PASS, complete
domain suite PASS (`1362/1362`), compilation PASS, and strict standalone package
validation PASS. These are audit-checkpoint checks and must be repeated after
production implementation.

## Guarded renderer source qualification

The first serious batch fixes six native class presentations and 32 exact
gender-specific IDs: Bard, Alchemist, Magus, cap/cape-free Ranger,
cap/cape-free Rogue, and cap-free Slayer. Investigation code excludes the
structurally unsafe caps/capes, uses disposable Human actors, captures native
and alternate valid ramps with no weapon/pistol/musket, restores exact avatar
state, and verifies cleanup.

At 2026-08-30T21:05:28Z, .\scripts\Build-Local.ps1 passed repository
validation, all 1365/1365 tests, exact-reference Release construction,
deterministic packaging, and strict validation. Standalone and local-runtime
packages have SHA-256
693c09684256fab77b4835b78eff12ab974c2bc460a63824f877768cd9c16ce8;
the staged DLL SHA-256 is
17bfe03b52e85cab627be425c680b1ccf6db88275ba4e253081065685304e377.
Runtime preflight passes 160 checks.

The initial runtime invocation was rejected before deployment or game launch
because the harness requires a clean Git state. It is not visual evidence.
Candidate-render gates remain pending until a clean published checkpoint is
run and the images are directly inspected.

The first clean published attempt is retained at
runtime-evidence/20260830T2109519221444Z-gunslinger-outfit-candidate-render.
It loaded commit 189ae46fa19552fa3b906740d9f30372c588f7f5 through
Steam, then failed closed at request acceptance with
scenario-timeouts-not-allowed. No hook, UI action, save action, render, or
score occurred. The missing in-mod working-save predicate entry was repaired
and covered by the focused guard test. After repair, all 1365 tests,
Build-Local, strict package validation, and the quiescent 160-check runtime
preflight pass. A new clean-commit runtime attempt remains required.

## Local evidence policy

Raw catalogs, extracted metadata, screenshots/contact sheets, runtime result
batches, assemblies, saves, and machine-local configuration stay ignored and
untracked. This file will contain only concise curated findings, reproducible
commands, hashes/fingerprints permitted by policy, and honest uncertainties.

## Acceptance threshold

The selected candidate must score at least 75/100 and have no missing geometry,
broken material, baked weapon duplication, unacceptable body-part hiding,
severe animation clipping, unsafe race/gender gap, broken armor transition,
optional dependency, or generic-Fighter identity. Until every applicable gate
above passes, the mission remains unqualified.
