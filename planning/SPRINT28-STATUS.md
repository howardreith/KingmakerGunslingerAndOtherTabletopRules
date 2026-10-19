# Sprint 28 current status

Status: **IN PROGRESS - native input checkpoint; no implemented giant or release candidate yet**. Sprint 28 only. Current source branch `codex/expanded-summoning-sprint28-hill-stone-fire-giants` at `C:/Dev/KingmakerGunslingerLab/wt/s28`, forked from verified remote master `dc9b83c71ff61f14a43d9cdabff37822796f43f2`. Remote `v0.0.162` was absent at intake. The Sprint 27 v0.0.161 tag/package and all older branches/worktrees remain untouched.

## Current checkpoint

- The [adult PF1 contract](EXPANDED-SUMMONING-SPRINT28-CONTRACT.md) freezes the three unmodified species and eleven SNA routes. Its native donor, equipment, projectile, catching, feat and terrain consumers still need qualification before registration.
- Three original 1254 x 1254 painted giant icon sources are preserved with exact prompts and source hashes in `assets-source/original-icons/expanded-summoning/sprint28-generation.json`. They are not yet exported, bound or approved in native UI. Human visual review was not performed.
- The read-only `observe-expanded-summoning-sprint28-native-inputs` scenario is registered in the C# catalog/runner, PowerShell metadata/allowlists and runtime documentation. Its first guarded run passed as a research observer, not gameplay qualification. The exact adult Hill Giant exists (`CR7_HillGiantStandart`, `61dd71e874b16f742a0f783314105553`, native prefab `635d6dee1c7831b41aa987bdbeff14bc`); all 14 matching unit names were Hill variants, with zero Stone and zero Fire. A bounded follow-up observer now inspects the retained native Frost control and rock/equipment seams before selecting adapters. No native donor is mutated.
- The mission-authorized local push policy was backed up byte-for-byte to `C:/Users/howar/AppData/Local/Temp/KMG-S28-push-policy-20261019083655.bak` (SHA-256 `db46ee3853aea85d5d622a2eedfd3d9f1c58195e40411a8296a7d028b17aae4a`), then only the two exact Sprint 28 branch names were inserted. New policy SHA-256 `9111ce933098e613c368c9822ace56e4c77a83564be36acc7437bc19faf6d419`. A byte comparison proves that insertion is the only change.

Checks selected under `docs/VALIDATION-POLICY.md`:

| Command / scope | Duration | Result |
|---|---:|---|
| `scripts/build.ps1 -Configuration Release -CompileOnly -KingmakerInstallDir <actual Kingmaker install>`; production compile | 17.8 s | PASS |
| `scripts/Test-RuntimeScenarioPreflight.ps1`; no-game request/catalog/metadata preflight | ~46 s | PASS, 665 checks |
| `scripts/Test-ExpandedSummoningRuntimeOrchestration.ps1 -ScriptRoot scripts`; no-game restoration/dispatch policy | 2.4 s | PASS, 209 assertions |
| `scripts/Test-PushPolicyWrapper.ps1 -PermittedRoot C:/Dev/KingmakerGunslingerLab/wt/s28`; fake Git only | ~60 s | PASS, 29 cases including both exact new branches and mixed-case refusal |
| `git diff --check` | <1 s | PASS |

The first preflight attempt used an elevated shell and correctly refused with `Runtime tests must run without administrator elevation`; it was rerun unelevated and passed. The first compile command lacked this new worktree's local `GamePath.props`; the explicit installed game directory resolved it. The first development package attempt also stopped at the domain-test assembly reference for that ignored local configuration. Copying the current checkout's ignored `GamePath.props` resolved it, without changing Git state. None of these refused attempts launched the game or staged a package.

## Fresh native input and restoration evidence

The independent HOWARD-LAPTOP intake record is `C:/Users/howar/AppData/Local/Temp/KMG-S28-intake-20261019T130020Z.json` (SHA-256 `1043164661214f447f80f87aa4fbe384c9e224badc39a21a8d72f39a2770d84b`): 900 installed mod files, 118 save files, one authorized Working and one protected baseline, 16 private reference files, 83 native settings values, no Kingmaker process, lease, active transaction, staged Mods directory or sentinel. It is local evidence, never a source artifact.

Development package from committed source `0d201872c4720215d5b555e00fe93bfe0910f935`: ZIP SHA-256 `18938bdd1e7e31752668cd739c1e4c14759af48da2eb17f3a38e6637557cc5d4`, DLL SHA-256 `0095ae9b0b50d90dbd549394d88f69c3ded559f2886cf8fdf8d2b206c390868c`. It passed the development validator and six of 2,693 named Sprint 27 domain tests; it is not release qualification. The profile `-WhatIf` refused to enter as expected, and the longest audit collector basename was preflighted at 80 characters.

One guarded `gunslinger-only` native execution used the existing Steam App ID 640820 request, limited Howard token, no save, and exact development-package reuse. Native RunId `20261019T1307485137347Z-c8342dcaed024bfa9f859c19d57a4e08` returned `PASS` for bounded metadata capture. Raw result SHA-256 `0c4811614e867d324d90ca59a0dc1af2d364b7d7aec27ccaa53335f683de6c20`; native input JSON SHA-256 `3289e75ef7006c7088ba19d4c3a25c83946b5f407df18a9c970f71ed10da8145`. The profile transaction `compat-20261019T130553Z-b47d8c46ca88` returned `Restored` with `restorationVerified=true` (transaction SHA-256 `224dfdc5166c268844586ad84fc98374d57e89a47318bb2774abd309b93ca427`), and the limited task returned exit code 0 and was removed. Independent post-run comparison matched the intake's full Mods, save and private-reference manifests, sound bank, 83 native settings values and game identity hashes exactly; no owned process, lock, sentinel or staging remained. These findings are donor inputs only.

The follow-up observer extension compiled under the Focused policy in 18.3 seconds with six selected tests PASS. One earlier compile failed on a missing `BlueprintLibraryLookup` namespace; the import was added and the successful compile replaced it. No game was launched for this source revision yet.

## Planned final campaign

At the coherent product candidate boundary, budget roughly 10-20 minutes per isolated native phase and 10-15 minutes for focused diagnosis between phases: composed melee/throw/armor, rock/catch/Fire and feat boundaries, RTWP and turn-based commands/AI, body/icon and ownership lifecycle, Working-save ON/OFF persistence, and all eleven public roots on the exact release bytes. Run each as a fresh restored transaction where the fixture requires it. Then complete full Sprint build, publisher preflight/dry-run, product merge, exact-byte promotion/public downloads, restoration audit and final records. These are planning estimates, not timeout or PASS criteria.

Next: commit/push this native-input checkpoint, package the follow-up observer from a clean source, run it under a fresh guarded profile, then implement the three species, ownership, rock throwing/catching, icons and compact native review. Release and publication remain deferred until the mandatory mechanics, presentation and restoration evidence pass. No Sprint 29 work.
