<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r08-request part=1/1 sha256=c058a7dd2df8fb8ac1a79a6836cac0ef1e1c77216121b97b06ae7dcb01ac3eb6 -->
### Claude Opus — review request — round 8 — candidate `953dc8cc51`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r08-42cbce` |
| Checkpoint | `final` (final) |
| Candidate H | `953dc8cc51a594af84943261feacd7a4dd17b3af` |
| Target B (`master`) | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Merge base M | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Mission | `spear-bootstrap-tooltip-repair-mission` sha256 `8c636b1fca` |
| Workflow | `17b66fac32` |
| Developer observed | headless session `58f2af95-341a-4483-8cc1-0d467f2b2de6`: init `claude-opus-5-5`, API models ['claude-opus-5-5'], effort ['high'] |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 8 of 15 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Round 7 is complete. The focused gate passed on the pushed clean HEAD 953dc8cc51.

Changes:
- R7-F1 (ecf1d641bc): the loader-compat runner now:
  - holds the lab's shared compatibility.lock through Enter-KmgRuntimeLease, with its runtime-lease journal, for the whole session;
  - re-checks the host immediately before every launch (any Kingmaker process, sibling-lab locks, the lease itself), withholding that launch and every later one;
  - restores game settings and completes the lease only when no game remains, otherwise keeping the journal, both locks and a RecoveryRequired lease for a guarded -RecoverOnly.
- R7-F2 (6d6a295b64): item-damage failures are one bounded item-damage predicate diagnostic. It names the item, the expected and actual values, the donor item's values (an unavailable donor never hides the owned failure) and the BaseDamage getter's patch owners. New domain case.
- R7-F3: the reload probe now enforces the loaded spear's BlueprintItemWeapon.BaseDamage and its override flag.
- R7-F4 (4e0ac475aa and 953dc8cc51): record corrections and round-7 evidence.

The round-7 candidate is built from 4e0ac475aabc (ZIP 253fbe43…, DLL 6b2696c7…). Sessions I7 (item-damage diagnostics), P7 (persistence reload), X7 (round-6 controls) and R7 (reporter-shaped stack) all passed on it. Each was surfaced through -Report with ownership restored, a Completed shared lease, a free shared lock, and registry, saves and install restored or unchanged. The offline ownership fixtures passed (PASS=35).

Limits, stated in the record:
- the simulated and membership sessions were not repeated on the round-7 product (last S6 and C6);
- L and O were last run in round 3;
- contention is proven by fixtures, not by a real second game process.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/versions/34fb852974476cfbd48374fc676c822b4e2f97911cee6706a79161c7e9d9d43c/agent_review.py verify --enrollment kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 --gate focused -- -Filter elven-branched-spear | yes | pass | 2.1 s | Candidate 953dc8cc51a5: exit 0, worktree_unchanged true, log gate-focused-953dc8cc51a5-d88e49a1250-aca821ee7ae-20261121T231703357953Z.log. Production compile plus 30 elven-branched-spear domain tests, including item-damage-diagnostic. Domain tests are supporting evidence only. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/compatibility/loader-compat/Invoke-LoaderCompatSession.ps1 -SafetyTests | yes | pass | seconds | Test-LoaderCompatSafety.ps1 PASS=35 at the final commit. Offline: disposable state root, lock files and journal, with a substituted process query standing in for a peer game; no game, loader, registry or owner file touched. Proves the contention paths with fixtures, not with a real second Kingmaker process. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear -ReferenceBundleDir … -LoaderReferenceDir … | yes | pass | about 6 min | Candidate 4e0ac475aabc: ZIP 253fbe43188d433f57a771261a215ba794160ff98b06142fafcedca69d643518, DLL 6b2696c70988db4ea1ed42ce015b99226a4de1fdd1727c569bf35eefd5e7049f; strict UMM package validation passed; 30 tests. Product source is unchanged from it to HEAD. |
| Invoke-LoaderCompatSession.ps1 spear-bootstrap-item-damage-diagnostic (candidate 4e0ac475aabc) + -Report 20261121T230213Z-umm-0.31.1-spear-bootstrap-item-damage-diagnostic [-ReportLaunch 0] | yes | pass | 53, 53 s | Product alone, disposable copy, UMM 0.31.1; summary b2f68884…; ownership restored, lease Completed, shared lock free. |
| Invoke-LoaderCompatSession.ps1 spear-bootstrap-persistence (candidate 4e0ac475aabc) + -Report 20261121T230443Z-umm-0.31.1-spear-bootstrap-persistence [-ReportLaunch 03] | yes | pass | 72, 62, 70 s | The fixture save is created by launch 1 of the same session (authorized disposable profile), with Gunslinger disabled through the panel and reloaded in a fresh process; summary a8e792eb…. |
| Invoke-LoaderCompatSession.ps1 spear-bootstrap-item-enchantment-list and spear-bootstrap-reporter-stack (candidate 4e0ac475aabc) + -Report for each | yes | pass | 70, 53, 54 s and 102, 110 s | Summaries 35eb493f… and b8583629…. The reporter stack uses four of the reported mods, not the reporter's full installation. |
| pwsh -NoProfile -File scripts/Invoke-KmgGate.ps1 -Filter elven-branched-spear (pre-commit working tree) | no | pass | about 17 s | 30/30; a local focused run. |

**Evidence references:**
- gate-focused-953dc8cc51a5-d88e49a1250-aca821ee7ae-20261121T231703357953Z.log
- scripts/compatibility/loader-compat/Test-LoaderCompatSafety.ps1
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T230213Z-umm-0.31.1-spear-bootstrap-item-damage-diagnostic/session-summary.json (sha256 b2f68884afc9a06648958694205b920036f6fe57337823514927d4f13a5c0c1b)
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T230443Z-umm-0.31.1-spear-bootstrap-persistence/session-summary.json (sha256 a8e792ebd34ae94766f74734cdd6b9504fc35e665f1332c929ace7d9baa62118)
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T230845Z-umm-0.31.1-spear-bootstrap-item-enchantment-list/session-summary.json (sha256 35eb493f78cbac5339e375db4b1147bf360880e3ecacf9a2498b848d57976511)
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T231156Z-umm-0.31.1-spear-bootstrap-reporter-stack/session-summary.json (sha256 b8583629117c8164227530837337d3b2d6ddc892cd478ca4eab146be843f0189)
- docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md

**Prior findings to assess:** `R7-F1`, `R7-F2`, `R7-F3`, `R7-F4`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
