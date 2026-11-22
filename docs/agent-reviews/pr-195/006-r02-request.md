<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r02-request part=1/1 sha256=aff0b4bcb30cce02d10e710477dd150db1a7bcc16cb6a2ff0f2e1075c0e6493b -->
### Claude Opus — review request — round 2 — candidate `b12776636b`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r02-7edcf8` |
| Checkpoint | `final` (final) |
| Candidate H | `b12776636b659baf4904c930ff420f51403d8a9b` |
| Target B (`master`) | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Merge base M | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Mission | `spear-bootstrap-tooltip-repair-mission` sha256 `8c636b1fca` |
| Workflow | `6b0bec9f60` |
| Developer observed | headless session `58f2af95-341a-4483-8cc1-0d467f2b2de6`: init `claude-opus-5-5`, API models ['claude-opus-5-5'], effort ['high'] |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `high`, read-only sandbox |
| Round budget | 2 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

The round-1 corrections are pushed at b12776636b. The worktree is clean and the controller's focused gate passed on that commit: production compiled and 26 of 3159 tests passed.

- **Product source:** commit 88f1e86b5e.
- **Native development candidate:** built at 0f08f76f644d, whose product source is identical to 88f1e86b5e. Package 53f58154…, DLL ae1250cc….

**Product corrections**
- **R1-F1:** a mismatch between the spear's effective getter value and the engine contract is now fatal whoever patches the getter. Patch owners remain in the message as context only. The round-0 tolerance for foreign patches is gone.
- **R1-F2:** all fallible foundation checks now run before any native selector, familiarity or selector-runtime state is touched. Selector publication is deferred until named-spear registration has also succeeded. BlueprintBootstrap.cs is unchanged.
- **R1-F3:**
  - Each getter read is captured on its own. A throwing owned getter is a reported failure; a throwing donor getter only marks that comparison unavailable.
  - The enchantment and sub-category checks are now part of the same single bounded report (17 checks).

**Native evidence**
UMM 0.31.1 disposable loader-compat copy. Registry was restored and owner saves and install were unchanged in every session.

- **Simulated session (7 launches):**
  - A mutated donor still produces the correct spear profile.
  - The invalid owned profile gives the full diagnostic. Ordinary prices and tooltips stay native over 25 queries each, with exactly one warning.
  - Owned-spear-only getter patch: startup fails and names the predicate and patch owner.
  - Donor-only throwing getter: startup still succeeds.
  - Owned throwing getter plus a second invalid value: one report covers both.
  - Late post-registration validation failure and named-registration failure: native selectors and selector runtime are restored exactly.
- **Baseline v0.0.192:** 5 of 5 price queries and 5 of 5 tooltip fills throw per item, each with the missing-token cause.
- **Fresh-process persistence with Gunslinger off:** the saved spear, the battered pistol (with its owner) and the control items all resolve and price correctly.
- **Reporter-shaped stack:** a new sorcerer's creature row click puts exactly that ability into native targeting. The native ground click casts it successfully and summons the creature that ability spawns.
- **Probe lifecycle:** a forced timeout releases everything the probe owned. An injected release failure stops the launch and fails the harness verdict.

**Still unknown:** the reporter's exact failing predicate and cause. ArmsArmor 1.0.10, about 38 other mods and the raw log are unavailable.

**Owner-relevant behaviour change:** a foreign mod that changes the owned spear's effective getters now fails startup visibly, with diagnostics naming the predicate and the patch owner. Eastern Weapons' donor inheritance remains as a documented residual risk.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/versions/617060d8.../agent_review.py verify --enrollment kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 --gate focused -- -Filter elven-branched-spear | yes | pass | 1.7 s (incremental compile; tests 0.2 s) | Candidate b12776636b. Production compiled. 26 of 3159 selected tests passed (11 bootstrap-repair policy cases and 15 spear neighbours). Focused only. |
| .\scripts\Invoke-KmgGate.ps1 -Filter 'custom-weapon;selector;martial-performance;eastern-weapons;battered;summoning-publication-boundary' | no | pass | 1.3 s of tests | 56 neighbour tests covering selector, publication and pricing. Run on a working tree whose product source equals 88f1e86b5e. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear -ReferenceBundleDir ... -LoaderReferenceDir ... | no | pass | about 6 min | Development candidate at 0f08f76f644d (never a release). Strict standalone package validation passed. Package 53f58154…, DLL ae1250cc…. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-simulated.json -LoaderProfile umm-0.31.1 -CandidateZip <candidate 0f08f76f644d> | yes | pass | 7 launches, 53-74 s each | Session 20261121T162812Z, harness d7dd0ff01d. Simulated, test-only conditions with the product alone. These prove the mechanisms, not the reporter's interaction. An earlier run (20261121T162032Z) exposed three harness defects and is retained. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-baseline.json -CandidateZip <published v0.0.192> | yes | pass | 73 s | Session 20261121T163535Z. Strict throws judging. Unrepaired bytes under the simulated donor mutation. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-persistence.json -CandidateZip <candidate> | yes | pass | 75 + 65 + 70 s | Session 20261121T163705Z. Fresh-process load with Gunslinger off of items saved with Gunslinger on. Product alone; fixture save only. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-probe-lifecycle.json -CandidateZip <candidate> | yes | pass | 96 + 79 s | Session 20261121T164419Z, harness/runner b348c3f815. Forced timeout and injected release failure via test-only probe invariants. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-reporter-stack.json -CandidateZip <candidate> | yes | pass | 103 + 113 s | Session 20261121T164728Z. Four of the reported mods at the reported versions, with the reporter's module settings. Not the 43-mod environment. |

**Evidence references:**
- docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md
- C:/Dev/KingmakerGunslingerLab/agent-review-state/kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704/evidence/gate-focused-b12776636b65-d88e49a1250-aca821ee7ae-20261121T165327481699Z.log
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T162812Z-umm-0.31.1-spear-bootstrap-simulated
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T163535Z-umm-0.31.1-spear-bootstrap-baseline
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T163705Z-umm-0.31.1-spear-bootstrap-persistence
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T164419Z-umm-0.31.1-spear-bootstrap-probe-lifecycle
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T164728Z-umm-0.31.1-spear-bootstrap-reporter-stack
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T153842Z-umm-0.31.1-spear-bootstrap-reporter-stack (round 0, v0.0.192 partial stack)
- artifacts/development-candidates/spear-bootstrap-tooltip-repair/0f08f76f644d/KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-0f08f76f644d.zip (sha256 53f58154067c35c60b7f06aeaac4041df7bfc63aa37eecdcb038598a421e44ca)
- product commit 88f1e86b5e; candidate b12776636b

**Prior findings to assess:** `R1-F1`, `R1-F2`, `R1-F3`, `R1-F4`, `R1-F5`, `R1-F6`, `R1-F7`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
