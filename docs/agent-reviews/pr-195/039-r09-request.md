<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r09-request part=1/1 sha256=3fecade434d1b05e72056db30706ada49ab0fd5f0dce8ccb5dc4c8b13f907138 -->
### Claude Opus — review request — round 9 — candidate `db63a941a6`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r09-c9052b` |
| Checkpoint | `final` (final) |
| Candidate H | `db63a941a65b3ccf0a6ecbf6cfdab8c4b4461c04` |
| Target B (`master`) | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Merge base M | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Mission | `spear-bootstrap-tooltip-repair-mission` sha256 `8c636b1fca` |
| Workflow | `17b66fac32` |
| Developer observed | headless session `58f2af95-341a-4483-8cc1-0d467f2b2de6`: init `claude-opus-5-5`, API models ['claude-opus-5-5'], effort ['high'] |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 9 of 15 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Round 8 is complete. The focused gate passed on the pushed clean HEAD db63a941a6.

Round 8 changed only the loader-compat runner (8354822130) and the record (db63a941a6). The product and its round-7 candidate are unchanged (4e0ac475aabc: ZIP 253fbe43…, DLL 6b2696c7…).

The runner is now one journaled session transaction:
- the package and harness are checked before anything is acquired;
- journal-first acquisition of the shared compatibility.lock lease and the private lock, rolled back exactly on failure;
- every later step sits inside the try;
- owner-file intake digests are kept beside the lease;
- a shared Complete-LcTransaction acts only at a safe boundary (no game, no sibling or foreign lock). It restores settings once, verifies owner files while still owning the host (recording verified, changed or unresolved in the journal and lease), and then releases. Each step is journaled, so an interrupted teardown resumes without a second restore;
- -RecoverOnly establishes exclusive ownership (owner exited, host free, exact lease lock, exclusive recovery handle) before its first registry write. Refused callers change nothing, and only matching locks are removed;
- older journals get the same owner checks.

Evidence:
- the offline transaction fixtures passed (PASS=74);
- a real missing-package refusal acquired nothing;
- a real post-acquisition setup failure (F8) was completed and released, with integrity verified;
- I8 and R8 then acquired both locks and passed, each with a Completed lease, integrity verified and the shared lock free.

R8-F5 is corrected in the record. Contention is proven by fixtures, not by a real second game or a real sibling lab; the record states this limit.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/versions/34fb852974476cfbd48374fc676c822b4e2f97911cee6706a79161c7e9d9d43c/agent_review.py verify --enrollment kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 --gate focused -- -Filter elven-branched-spear | yes | pass | 2.3 s | Candidate db63a941a65b: exit 0, worktree_unchanged true, log gate-focused-db63a941a65b-d88e49a1250-aca821ee7ae-20261121T234445823151Z.log. Production compile plus 30 elven-branched-spear domain tests. Supporting evidence only; the product is unchanged since round 7. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -SafetyTests | yes | pass | seconds | Test-LoaderCompatSafety.ps1 PASS=74 at the final commit. Offline: disposable state root, locks, journal and owner-file manifests, with a substituted peer-process query; no game, loader, registry or owner file touched. Contention paths are fixture-proven, not exercised with a real second game or sibling lab. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -SessionFile …/spear-bootstrap-item-damage-diagnostic.json -LoaderProfile umm-0.31.1 -CandidateZip artifacts/development-candidates/spear-bootstrap-tooltip-repair/no-such-candidate.zip -HarnessDir artifacts/loader-compat-harness-spear | yes | pass | seconds | Expected refusal: 'Candidate ZIP not found: … Nothing was acquired.' before any lock was taken; the subsequent sessions acquired normally. |
| Invoke-LoaderCompatSession.ps1 -SessionFile artifacts/round8-setup-failure-session.json (disposable, untracked) … + -Report 20261121T233622Z-umm-0.31.1-spear-bootstrap-round8-setup-failure | yes | pass | under 1 min | Real post-acquisition setup failure: a fixture save that does not exist. The summary (669e5b17…) shows host-error by design, ownership restored, registryRestored true, ownerIntegrity verified (95 saves, install unchanged), lease Completed with integrity, shared lock free. |
| Invoke-LoaderCompatSession.ps1 spear-bootstrap-item-damage-diagnostic and spear-bootstrap-reporter-stack (candidate 4e0ac475aabc, round-8 runner) + -Report for each | yes | pass | 60, 54 s and 102, 111 s | Summaries 3308b3f8… and 385b1601…: PASS, ownership restored, ownerIntegrity verified, leases Completed, shared lock free. The reporter stack uses four of the reported mods, not the reporter's full installation. |

**Evidence references:**
- gate-focused-db63a941a65b-d88e49a1250-aca821ee7ae-20261121T234445823151Z.log
- scripts/compatibility/loader-compat/LoaderCompat.Common.ps1
- scripts/compatibility/loader-compat/Test-LoaderCompatSafety.ps1
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T233622Z-umm-0.31.1-spear-bootstrap-round8-setup-failure/session-summary.json (sha256 669e5b1718386b70bccf974edbf2a8be308561747653596a0e9e0f183bc21ed9)
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T233715Z-umm-0.31.1-spear-bootstrap-item-damage-diagnostic/session-summary.json (sha256 3308b3f8426f0c5a0d68874825c444aa7aebf350a7a74cf061d36c15929aee37)
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T233924Z-umm-0.31.1-spear-bootstrap-reporter-stack/session-summary.json (sha256 385b16016cf7a3e3324eae0ffbfaffcc8f0636d8ec4f4b1d245f81d33ea521c0)
- docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md

**Prior findings to assess:** `R7-F1`, `R8-F1`, `R8-F2`, `R8-F3`, `R8-F4`, `R8-F5`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
