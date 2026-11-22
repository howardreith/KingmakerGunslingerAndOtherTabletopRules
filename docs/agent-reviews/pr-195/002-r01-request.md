<!-- agent-review:v1 enrollment=kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 key=r01-request part=1/1 sha256=9664515b5ed23acd5eb8dda671d89db41bafe72f59fe9a6bc069d31bf29877ef -->
### Claude Opus — review request — round 1 — candidate `287bd73ebb`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704-r01-ed1561` |
| Checkpoint | `final` (final) |
| Candidate H | `287bd73ebbe28723b135bdba0ed4ddbc407c5b5f` |
| Target B (`master`) | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Merge base M | `2e61e78097623d984630eadab873d8d87417f2c3` |
| Mission | `spear-bootstrap-tooltip-repair-mission` sha256 `8c636b1fca` |
| Workflow | `6b0bec9f60` |
| Developer observed | headless session `58f2af95-341a-4483-8cc1-0d467f2b2de6`: init `claude-opus-5-5`, API models ['claude-opus-5-5'], effort ['high'] |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `high`, read-only sandbox |
| Round budget | 1 of 5 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

The final candidate is 287bd73ebb. It is pushed through the guarded wrapper, the worktree is clean, and the controller's focused gate passed on it.

- **Product commit:** 288ce4dd07.
- **Later commits:** they change only the test-only loader-compat harness, its sessions, the session runner and docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md.
- **Native candidate:** built at 56e00163e9d1 with the product source unchanged. Package SHA-256 b7b4d619…, DLL SHA-256 031dc659….

**Spear repair**
- The owned clone now gets its whole mechanical profile written explicitly, so no mechanical value is inherited from the live native Longspear donor.
- The donor is never written. A fingerprint of all its mechanical fields is checked before and after construction.
- Validation is kept and is stricter: 15 predicates, each checking the owned serialized field and the live getter against the 2.1.7b engine contract.
  - An owned-field mismatch, or a getter mismatch with no outside patch, is fatal.
  - A getter changed by another mod's patch is logged once and tolerated.
- A failure now produces one bounded message naming each failed predicate, with expected, actual and donor values and the getter's patch owners.

**Pricing repair**
- The battered sale-price hook uses a narrow inspection path. When initialization is pending or failed and the token was never registered, the weapon keeps its native price and the warning is logged once per process.
- The creation, transfer and ownership checks are unchanged and still strict.

**Native evidence**
Kingmaker 2.1.7b, UMM 0.31.1, disposable loader-compat copy, harness commit 4ba6cab678. Registry was restored and owner saves and install were unchanged in every session.

- **Unrepaired v0.0.192 under a simulated donor mutation:** reproduces the reported cascade.
  - Generic spear failure; every registration rolled back.
  - Every price and tooltip query throws the missing battered-origin exception.
  - Summon Monster I stays native (no variants, does not expand) and still summons a Summoned Dog, matching the reporter's log.
- **Candidate, same mutation:** initializes. The spear has its exact intended profile, and the donor and an unrelated weapon type are unchanged.
- **Candidate, invalid owned profile:** startup fails with the new diagnostic, logged once. Ordinary prices and tooltips stay native over 25 queries per item with no throws, and the unavailable-state warning appears exactly once.
- **Candidate, outside getter patch:** startup completes with one warning.
- **Reporter-shaped stack** (the four reported mods held at their reported versions, plus the reporter's settings applied through real panel clicks, gunslinger off):
  - Candidate: a fresh level-1 sorcerer, created through native level-up, opens a 7-entry Summon Monster I creature menu with names and icons. Casting Fire Beetle from it succeeds.
  - Candidate: battered pistol sells for the fixed 22 gp; ordinary weapons sell at native prices.
  - Unrepaired v0.0.192 also passes on this stack, so the partial stack does not reproduce the report.

**Still not established**
- Which predicate and which mod caused the reporter's failure.
- The empty-menu wording is consistent with an unexpanded native spell. No menu that opens empty was observed in any run.

**Open for owner review**
- Design point: an externally patched getter is tolerated. The warning text assumes the patch applies to every weapon, but the test stand-in was limited to the Longspear family.
- Eastern Weapons still inherits range, attack type and natural-weapon status from its donors. This is a residual risk; it was not reported as failing and was not changed.

No reviewer findings were pending. Earlier blockers (missing GamePath.props, denied native commands) are resolved by the operator's changes.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/versions/617060d8.../agent_review.py verify --enrollment kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704 --gate focused -- -Filter elven-branched-spear | yes | pass | 1.9 s (compile 0.5 s incremental, tests 0.2 s) | Candidate 287bd73ebb. Production compiled. 25 of 3158 selected tests passed (10 new bootstrap-repair policy cases and 15 spear neighbours). Focused only: not a release or broad qualification. |
| .\scripts\Invoke-KmgGate.ps1 -Filter 'battered;craft-magic-items;sale;vendor;player-facing' and -Filter 'summoning-publication-boundary;summon.publication;alternate-summoning;summon-variant-menu' (with -KingmakerInstallDir) | no | pass | under 1 s of tests each | 98 and 43 neighbour tests passed. Run on the working tree identical to product commit 288ce4dd07. |
| powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1 -DevelopmentPurpose spear-bootstrap-tooltip-repair -DevelopmentTestFilter elven-branched-spear -ReferenceBundleDir <repo>\artifacts\summon-overhaul-baseline\reference-input\KingmakerGunslinger-private-build-references -LoaderReferenceDir <repo>\artifacts\references\loader | no | pass | about 6 min | Development candidate (never a release) at 56e00163e9d1, with product source identical to 288ce4dd07. Strict standalone UMM package validation passed. Package b7b4d619…, DLL 031dc659…. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-simulated.json -LoaderProfile umm-0.31.1 -CandidateZip <candidate> -HarnessDir artifacts/loader-compat-harness-spear | yes | pass | 70 s, 73 s, 61 s | Session 20261121T154637Z. Simulated, test-only conditions with the product alone: donor mutation, invalid owned profile, and an outside getter patch limited to the Longspear family. These prove the mechanisms only, not the reporter's interaction. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-baseline.json -LoaderProfile umm-0.31.1 -CandidateZip <published v0.0.192 zip> | yes | pass | 72 s | Session 20261121T155017Z. The unrepaired bytes reproduce the reported cascade under the simulated donor mutation. |
| Invoke-LoaderCompatSession.ps1 -SessionFile sessions/spear-bootstrap-reporter-stack.json -LoaderProfile umm-0.31.1 -CandidateZip <candidate> and <published v0.0.192 zip> | yes | pass | 107 s + 115 s (candidate); 101 s + 109 s (v0.0.192) | Sessions 20261121T154235Z (candidate) and 20261121T153842Z (v0.0.192). Call of the Wild 1.14.4c-2.1, CraftMagicItems 2.1.0, KingmakerBugfixes 1.3.3 and RacesUnleashed 1.0.11, with the reporter's module settings. ArmsArmor 1.0.10 and about 38 other reported mods are absent. The partial stack does not reproduce on v0.0.192. Earlier harness iterations are retained and listed in the record. |

**Evidence references:**
- docs/SPEAR-BOOTSTRAP-TOOLTIP-REPAIR.md
- C:/Dev/KingmakerGunslingerLab/agent-review-state/kmg-codex-spear-bootstrap-tooltip-repair-20261121062854-a704/evidence/gate-focused-287bd73ebbe2-d88e49a1250-aca821ee7ae-20261121T155400456375Z.log
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T154637Z-umm-0.31.1-spear-bootstrap-simulated
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T155017Z-umm-0.31.1-spear-bootstrap-baseline
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T154235Z-umm-0.31.1-spear-bootstrap-reporter-stack
- C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/20261121T153842Z-umm-0.31.1-spear-bootstrap-reporter-stack
- artifacts/development-candidates/spear-bootstrap-tooltip-repair/56e00163e9d1/KingmakerGunslinger-0.0.192-development-spear-bootstrap-tooltip-repair-56e00163e9d1.zip (sha256 b7b4d619d5e020bf259d4576f52820ebea890a66727992a2a21c99ab25b7083e)
- product commit 288ce4dd07; harness commit 4ba6cab678; candidate 287bd73ebb

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
