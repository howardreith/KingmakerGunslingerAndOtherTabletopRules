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
| Focused tests | Pending | Exact command/result |
| Repository validation | Pending | Exact command/result |
| Complete domain suite | Pending | Exact command/result |
| Clean Release build | Pending | Exact command/result |
| Installable package | Pending | Exact validation and forbidden-artifact scan |
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
