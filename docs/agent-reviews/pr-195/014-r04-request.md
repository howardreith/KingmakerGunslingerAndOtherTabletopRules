<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r04-request part=1/1 sha256=110513fec88b112a200019c1726a0f88662b3608795622650e5beecfdf099126 -->
### Claude Opus — review request — round 4 — candidate `9b8a6b4435`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r04-61b534` |
| Checkpoint | `final` (final) |
| Candidate H | `9b8a6b44352fd5babed673d354414be04fd619b1` |
| Target B (`master`) | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Merge base M | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Mission | `spear-bootstrap-tooltip-repair-mission` sha256 `8c636b1fca` |
| Workflow | `6b0bec9f60` |
| Developer observed | headless session `58f2af95-341a-4483-8cc1-0d467f2b2de6`: init `claude-opus-5-5`, API models ['claude-opus-5-5'], effort ['high'] |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `high`, read-only sandbox |
| Round budget | 4 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Round 3 is fixed, and the product is unchanged: `src/` and `tests/` are identical to 8ee7eabaeda0. Commit 9153164dcf changes only the test-only harness. (1) The native profile observer now records an effective Feet value at full precision through FieldText instead of the truncating Feet.Value. kmgWeaponTypeProfile gains longspearGetter:/ownedGetter: text invariants. (2) SorcererSummonProbe captures the incoming SelectionManager.SelectedUnits (any size, empty included) and GroupController's current character before changing anything. It refuses a selection it cannot restore natively and restores exactly that set before the caster is destroyed; the main-character fallback is removed. It verifies selection and current character on every release path, and a mismatch fails the release. New kmgSetSelection/kmgSelectionState ops and a new session, spear-bootstrap-observer-selection, cover this. Commit 9b8a6b4435 updates the investigation record: the machine-recorded summary of simulated session 20261121T171410Z, the corrected S-8/round-1 attribution, and the round-3 sessions. New native sessions on the 8ee7eabaeda0 candidate (zip f8988fde…, product DLL f0f17f7c…, harness DLL 22ab82fa…, git 9153164dcf) under UMM 0.31.1 both passed, with registryRestored, ownerSavesUnchanged and ownerInstallUnchanged all true: observer-selection 20261121T181329Z (4/4) and probe-lifecycle 20261121T181849Z (3/3). The focused gate passed on the pushed clean HEAD 9b8a6b4435.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/versions/617060d8f8782b811b1dd7bdca1f07abcbc488e338433e0d0cb4511287696954/agent_review.py verify --enrollment kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 --gate focused -- -Filter elven-branched-spear | yes | pass | 1.9 s | Candidate 9b8a6b44352f: exit 0, worktree_unchanged true, log gate-focused-9b8a6b44352f-d88e49a1250-aca821ee7ae-20261121T182532695935Z.log. Production compile plus the elven-branched-spear domain filter (27 tests in round 2; product and test sources unchanged since). Domain tests are supporting evidence only. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Build-LoaderCompatHarness.ps1 -SourceDir tools/LoaderCompatHarness -OutputDir artifacts/loader-compat-harness-spear -LoaderReferenceDir C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/artifacts/references/loader | no | pass | under 1 min | The test-only harness compiled with no errors against the UMM 0.31.1 references. This confirms the Kingmaker selection APIs used (SelectedUnits, MultiSelect, UnselectUnit, GroupController.GetCurrentCharacter/SelectUnit). |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -SessionFile scripts/compatibility/loader-compat/sessions/spear-bootstrap-observer-selection.json -LoaderProfile umm-0.31.1 -CandidateZip artifacts/development-candidates/spear-bootstrap-tooltip-repair/8ee7eabaeda0/KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-8ee7eabaeda0.zip -HarnessDir artifacts/loader-compat-harness-spear | yes | pass | about 5 min (70, 79, 80, 72 s) | Session 20261121T181329Z at git 9153164dcf, in the disposable game copy, product alone. O-1: with the Longspear getter at 6.25 ft, startup completed; the observer recorded Longspear AttackRange "6.25" and the owned spear "6" (invariants enforced; rangeOverrides 3). O-2: empty incoming selection preserved after a full Fire Beetle summon (Success, 714b0410…). O-3: main-only selection preserved. O-4: refusal with an empty selection plus pre-existing targeting left the selection empty and the targeting intact. A multi-unit selection was not exercised natively, because the fixture party has one member. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -SessionFile scripts/compatibility/loader-compat/sessions/spear-bootstrap-probe-lifecycle.json -LoaderProfile umm-0.31.1 -CandidateZip artifacts/development-candidates/spear-bootstrap-tooltip-repair/8ee7eabaeda0/KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-8ee7eabaeda0.zip -HarnessDir artifacts/loader-compat-harness-spear | yes | pass | about 4 min (98, 80, 71 s) | Session 20261121T181849Z at git 9153164dcf; supersedes 174636Z. In the timeout release, the injected release failure (harness verdict failed and isolation unrestored, as expected) and the non-idle refusal, outgoingSelection equalled incomingSelection (main character alone). |
| Read/Grep of C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T171410Z-umm-0.31.1-spear-bootstrap-simulated/session-summary.json and 02-invalid-owned-profile/harness/harness-result.json | yes | pass | n/a | A recorded read of the immutable round-2 simulated session (R3-F1): status PASS, 12/12 launch verdicts PASS with harness passed, git 8ee7eabaeda0, zip f8988fde…, productSha256 f0f17f7c… in every launch identity, registryRestored/ownerSavesUnchanged/ownerInstallUnchanged true. S-2: 4 weapons x 25 queries, priceThrows 0, tooltipThrows 0, unavailable warning exactly once. The shell (cat/Get-Content) was denied for paths outside the worktree, so the Read and Grep tools were used. |

**Evidence references:**
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T171410Z-umm-0.31.1-spear-bootstrap-simulated/session-summary.json
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T171410Z-umm-0.31.1-spear-bootstrap-simulated/02-invalid-owned-profile/harness/harness-result.json
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T181329Z-umm-0.31.1-spear-bootstrap-observer-selection/session-summary.json
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T181329Z-umm-0.31.1-spear-bootstrap-observer-selection/01-fractional-donor-observer/harness/harness-result.json
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T181329Z-umm-0.31.1-spear-bootstrap-observer-selection/02-empty-selection-preserved/harness/harness-result.json
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T181849Z-umm-0.31.1-spear-bootstrap-probe-lifecycle/session-summary.json
- gate-focused-9b8a6b44352f-d88e49a1250-aca821ee7ae-20261121T182532695935Z.log
- docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md
- scripts/compatibility/loader-compat/sessions/spear-bootstrap-observer-selection.json

**Prior findings to assess:** `R2-F1`, `R2-F3`, `R2-F4`, `R3-F1`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
