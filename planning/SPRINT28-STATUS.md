# Sprint 28 current status

Status: **IN PROGRESS ? research checkpoint; no product candidate or native qualification yet**. Sprint 28 only. Current source branch `codex/expanded-summoning-sprint28-hill-stone-fire-giants` at `C:/Dev/KingmakerGunslingerLab/wt/s28`, forked from verified remote master `dc9b83c71ff61f14a43d9cdabff37822796f43f2`. Remote `v0.0.162` was absent at intake. The Sprint 27 v0.0.161 tag/package and all older branches/worktrees remain untouched.

## Current checkpoint

- The [adult PF1 contract](EXPANDED-SUMMONING-SPRINT28-CONTRACT.md) freezes the three unmodified species and eleven SNA routes. Its native donor, equipment, projectile, catching, feat and terrain consumers still need qualification before registration.
- Three original 1254 x 1254 painted giant icon sources are preserved with exact prompts and source hashes in `assets-source/original-icons/expanded-summoning/sprint28-generation.json`. They are not yet exported, bound or approved in native UI. Human visual review was not performed.
- The read-only `observe-expanded-summoning-sprint28-native-inputs` scenario is registered in the C# catalog/runner, PowerShell metadata/allowlists and runtime documentation. It reports native candidate and carrier metadata only; a PASS would not prove gameplay.
- The mission-authorized local push policy was backed up byte-for-byte to `C:/Users/howar/AppData/Local/Temp/KMG-S28-push-policy-20261019083655.bak` (SHA-256 `db46ee3853aea85d5d622a2eedfd3d9f1c58195e40411a8296a7d028b17aae4a`), then only the two exact Sprint 28 branch names were inserted. New policy SHA-256 `9111ce933098e613c368c9822ace56e4c77a83564be36acc7437bc19faf6d419`. A byte comparison proves that insertion is the only change.

Checks selected under `docs/VALIDATION-POLICY.md`:

| Command / scope | Duration | Result |
|---|---:|---|
| `scripts/build.ps1 -Configuration Release -CompileOnly -KingmakerInstallDir <actual Kingmaker install>`; production compile | 17.8 s | PASS |
| `scripts/Test-RuntimeScenarioPreflight.ps1`; no-game request/catalog/metadata preflight | ~46 s | PASS, 665 checks |
| `scripts/Test-ExpandedSummoningRuntimeOrchestration.ps1 -ScriptRoot scripts`; no-game restoration/dispatch policy | 2.4 s | PASS, 209 assertions |
| `scripts/Test-PushPolicyWrapper.ps1 -PermittedRoot C:/Dev/KingmakerGunslingerLab/wt/s28`; fake Git only | ~60 s | PASS, 29 cases including both exact new branches and mixed-case refusal |
| `git diff --check` | <1 s | PASS |

The first preflight attempt used an elevated shell and correctly refused with `Runtime tests must run without administrator elevation`; it was rerun unelevated and passed. The first compile command lacked this new worktree's local `GamePath.props`; the explicit installed game directory resolved it. Neither refused attempt launched the game or staged a package.

Next: commit/push this coherent research checkpoint; build one development-only package from that exact clean commit for the native input audit; snapshot actual installation/settings/sound bank/references/saves/processes/leases/staging before any guarded transaction. Then choose exact donors and implement the three creatures, equipment ownership, throws/catches, icons and focused native review. Release/publication, persistence and full Sprint qualification remain deferred until a complete frozen candidate exists. No Sprint 29 work.
