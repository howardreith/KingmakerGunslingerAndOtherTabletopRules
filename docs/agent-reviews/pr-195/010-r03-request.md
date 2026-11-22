<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r03-request part=1/1 sha256=401a94b17ad7ab36600fe1d7de56a66be3b10c3eb148bf35d767aa85d23058ab -->
### Claude Opus — review request — round 3 — candidate `67059cdc47`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r03-f4cf52` |
| Checkpoint | `final` (final) |
| Candidate H | `67059cdc47e46e734ad141af35884fe895e47ead` |
| Target B (`master`) | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Merge base M | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Mission | `spear-bootstrap-tooltip-repair-mission` sha256 `8c636b1fca` |
| Workflow | `6b0bec9f60` |
| Developer observed | headless session `58f2af95-341a-4483-8cc1-0d467f2b2de6`: init `claude-opus-5-5`, API models ['claude-opus-5-5'], effort ['high'] |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `high`, read-only sandbox |
| Round budget | 3 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Round 2 is complete. The corrections are pushed at 67059cdc47. The worktree is clean, and the controller's focused gate passed: production compiled and 27 of 3160 selected tests passed.

- **Product source:** cc8da0e71f.
- **Native development candidate:** built at 8ee7eabaeda0 with identical product source. Package f8988fde…, DLL f0f17f7c….

**Product changes**
- **R2-F1:** Configure, inside the registry factory, checks only the owned serialized profile, because the clone still carries the donor's GUID there. The definitive check (serialized plus effective values) runs on the registered type, after it has its released GUID and before selector publication. Effective values not read in the early check are recorded as `<deferred>` and never judged.
- **R1-F3:** Register no longer reads the donor's Weight getter. The donor's serialized fields, m_Weight included, are covered by its before/after fingerprint.
- **R2-F2:** sub-categories are checked against the engine's patched `WeaponCategoryExtension.HasSubCategory` result for the spear's category, with that method's patch owners reported on failure.
- **R2-F3:** the effective AttackRange is compared at full `Feet` precision, so 6.25 ft no longer passes as 6 ft.

**Harness changes**
- **R1-F5:** the summon check compares the chosen row, the submitted command and the spawned unit with released identities named by the session, not derived from the ability under test.
- **R2-F4:** the summon probe refuses a UI that is not idle before changing anything. On release it undoes only the UI state it changed itself.

**Native evidence**
All runs used UMM 0.31.1 in the disposable loader-compat copy. The game's registry was restored and the owner's saves and installation were unchanged in every session.

| Session | Launches | Result |
|---|---|---|
| Simulated (on the candidate) | 12 | all passed |
| Baseline (published v0.0.192) | 1 | passed |
| Persistence (on the candidate) | 3 | all passed |
| Probe lifecycle (on the candidate) | 3 | all passed |
| Reporter-shaped stack (on the candidate) | 2 | all passed |

**Still unknown:** the reporter's exact failing predicate and its cause. ArmsArmor 1.0.10, about 38 other mods and the raw log are unavailable.

**Behaviour change for the owner:** a foreign mod that changes the registered spear's effective values now fails startup visibly, with a diagnostic naming the predicate and the patch owner. Patches keyed on the native Longspear no longer affect the spear.

**Residual risk:** Eastern Weapons still inherits values from its donors; this remains documented.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/versions/617060d8.../agent_review.py verify --enrollment kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 --gate focused -- -Filter elven-branched-spear | yes | pass | 1.9 s (incremental compile; tests 0.2 s) | Candidate 67059cdc47. Production compiled. 27 of 3160 selected tests passed (12 bootstrap-repair policy cases and 15 spear neighbours). Focused only. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear -ReferenceBundleDir ... -LoaderReferenceDir ... | no | pass | about 6 min | Development candidate at 8ee7eabaeda0, product source identical to cc8da0e71f (never a release). Strict package validation passed. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-simulated.json -LoaderProfile umm-0.31.1 -CandidateZip <8ee7eabaeda0> | yes | pass | 12 launches, 53-73 s each | Session 20261121T171410Z. Simulated, test-only conditions with the product alone. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-baseline.json -CandidateZip <published v0.0.192> | yes | pass | 74 s | Session 20261121T174507Z. Unrepaired package under the simulated donor mutation. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-probe-lifecycle.json -CandidateZip <8ee7eabaeda0> | yes | pass | 96 + 80 + 74 s | Session 20261121T174636Z. Forced timeout, injected release failure and non-idle-UI refusal, via test-only invariants. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-persistence.json -CandidateZip <8ee7eabaeda0> | yes | pass | 76 + 69 + 71 s | Session 20261121T175105Z. Fresh-process reload with Gunslinger off; product alone. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-reporter-stack.json -CandidateZip <8ee7eabaeda0> | yes | pass | 105 + 111 s | Session 20261121T175459Z. Four of the reported mods at the reported versions with the reporter's settings; not the 43-mod environment. |

**Evidence references:**
- docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md
- C:/Dev/KingmakerGunslingerLab/agent-review-state/kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704/evidence/gate-focused-67059cdc47e4-d88e49a1250-aca821ee7ae-20261121T180028383129Z.log
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T171410Z-umm-0.31.1-spear-bootstrap-simulated
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T174507Z-umm-0.31.1-spear-bootstrap-baseline
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T174636Z-umm-0.31.1-spear-bootstrap-probe-lifecycle
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T175105Z-umm-0.31.1-spear-bootstrap-persistence
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T175459Z-umm-0.31.1-spear-bootstrap-reporter-stack
- artifacts/development-candidates/spear-bootstrap-tooltip-repair/8ee7eabaeda0/KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-8ee7eabaeda0.zip (sha256 f8988fdee8411c59d7d2cd726b9e4adebf49d90801e8eaf883d2b52fd4bbbb00)
- product commit cc8da0e71f; candidate 67059cdc47

**Prior findings to assess:** `R1-F3`, `R1-F5`, `R2-F1`, `R2-F2`, `R2-F3`, `R2-F4`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
