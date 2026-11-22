<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r06-request part=1/1 sha256=1fde2ccc1828e483c5794a630396eb1e2707e998e43dec7089c7f679ef4b5a9f -->
### Claude Opus — review request — round 6 — candidate `0b3a9533cd`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r06-1376f6` |
| Checkpoint | `final` (final) |
| Candidate H | `0b3a9533cda103cf64923d81319e2c3ef14b5008` |
| Target B (`master`) | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Merge base M | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Mission | `spear-bootstrap-tooltip-repair-mission` sha256 `8c636b1fca` |
| Workflow | `17b66fac32` |
| Developer observed | headless session `58f2af95-341a-4483-8cc1-0d467f2b2de6`: init `claude-opus-5-5`, API models ['claude-opus-5-5'], effort ['high'] |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 6 of 15 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Round 5 is complete. STOP was removed and the focused gate passed on the pushed clean HEAD 0b3a9533cd with the 34fb8529 verify helper. (R5-F1) In 195d17ce19, the product's effective sub-category check now covers every WeaponSubCategory value and the None wildcard, through ElvenBranchedSpearProfilePolicy.SubCategoryText. An extra classification such as Simple, a missing one, or a denied wildcard now fails validation; the predicate, values, donor context and patch owners are still reported. A new domain case covers it. (R5-F2) In 18361607ed and 0b3a9533cd, the read-only report keeps empty and single-element arrays as arrays and projects list elements individually. Every report first prints a synthetic shape check. The round-5 candidate is 18361607eda8 (ZIP c96e1b3c…, DLL f0fb25cb…). Native sessions on it passed with registry restored and owner saves and install unchanged, each surfaced through -Report: S5 (simulated, 12/12), C (healthy, Finessable removed, Simple added) and R5 (reporter-shaped stack). The investigation record documents all of this. Persistence, probe-lifecycle and observer-selection were not repeated on the round-5 candidate; the record states this limit.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/versions/34fb852974476cfbd48374fc676c822b4e2f97911cee6706a79161c7e9d9d43c/agent_review.py verify --enrollment kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 --gate focused -- -Filter elven-branched-spear | yes | pass | 1.9 s | Candidate 0b3a9533cda1: exit 0, worktree_unchanged true, log gate-focused-0b3a9533cda1-d88e49a1250-aca821ee7ae-20261121T192419305790Z.log. Production compile plus the elven-branched-spear domain filter (28 tests, including subcategory-text-all-values). Domain tests are supporting evidence only. |
| pwsh -NoProfile -File scripts/Invoke-KmgGate.ps1 -Filter "custom-weapon;selector;martial-performance;eastern-weapons;battered;summoning-publication-boundary" | no | pass | 1.7 s | 56/56 neighbour tests on the R5-F1 tree; a local run. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear -ReferenceBundleDir … -LoaderReferenceDir … | no | pass | about 6 min | Candidate 18361607eda8: ZIP c96e1b3c94cf13707cb8f73368681e61150e23f0526542d050979bdc3707f20e, DLL f0fb25cb6027a83ed4265b880122dbd0cc573ca1f04f4eb4f74552c3bc4c8330. Strict UMM package validation passed. src/ and tests/ are unchanged since then. |
| Invoke-LoaderCompatSession.ps1 (spear-bootstrap-subcategory, candidate 18361607eda8) + -Report 20261121T185458Z-umm-0.31.1-spear-bootstrap-subcategory [-ReportLaunch 03] | yes | pass | 71, 52, 53 s | Product alone, disposable copy, UMM 0.31.1. Healthy startup with selectors published. Finessable removed: startup failed once. Simple added: startup failed once with the extra classification and patch owners named, subCategoryAdditions 1, selectors restored. |
| Invoke-LoaderCompatSession.ps1 (spear-bootstrap-simulated, candidate 18361607eda8) + -Report 20261121T190216Z-umm-0.31.1-spear-bootstrap-simulated [-ReportLaunch 02] | yes | pass | about 12 min | 12/12 PASS; summary sha256 5edbd352…; S5-2 has four ordinary weapons x 25 queries with 0 price or tooltip throws, prices 1/4/5/13, and the unavailable warning once. Restoration fields true. |
| Invoke-LoaderCompatSession.ps1 (spear-bootstrap-reporter-stack, candidate 18361607eda8) + -Report 20261121T185820Z-umm-0.31.1-spear-bootstrap-reporter-stack | yes | pass | 101, 109 s | Four of the reported mods, not the reporter's full installation. Startup complete with the stricter check; no profile-invalid, missing-token or unavailable-state line. |

**Evidence references:**
- gate-focused-0b3a9533cda1-d88e49a1250-aca821ee7ae-20261121T192419305790Z.log
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T185458Z-umm-0.31.1-spear-bootstrap-subcategory/session-summary.json (sha256 158a1ecb279b9f241b174eba7b33407014ad5afba74fd514579d35f0de409e21)
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T185458Z-umm-0.31.1-spear-bootstrap-subcategory/03-spear-simple-added/harness/harness-result.json (sha256 8415b9ee249cee6fea28b88015e93fe5904c2cdb8537392b3ae9b77a6b47766d)
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T190216Z-umm-0.31.1-spear-bootstrap-simulated/session-summary.json (sha256 5edbd352206de1825148797192f5edd47475bc191d04471625d149177133ec53)
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T185820Z-umm-0.31.1-spear-bootstrap-reporter-stack/session-summary.json (sha256 5d5afd69b51a12ba437623a3f7177e28dedfb49e09cda320ec8ddea0148972a5)
- docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md

**Prior findings to assess:** `R5-F1`, `R5-F2`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
