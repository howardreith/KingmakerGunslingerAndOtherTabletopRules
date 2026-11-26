<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 key=r01-request part=1/1 sha256=0982d1e24305fb6aa3a5bcde3d4d1e0e3e88ea97d20397a209f65e2605e1a187 -->
### Claude Opus — review request — round 1 — candidate `192166b9ba`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08-r01-e7efe0` |
| Checkpoint | `final` (final) |
| Candidate H | `192166b9ba79161aaa2b05dc29012650ba68ee1e` |
| Target B (`codex/s7-recovery-c1-cue-evaluator`) | `2c67a049ee33e0b29621fd29a8b25608ed31e956` |
| Merge base M | `2c67a049ee33e0b29621fd29a8b25608ed31e956` |
| Mission | `s7-recovery-c2-mission` sha256 `f54c407bac` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | none |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 1 of 2 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 recovery child C2 on H=192166b9ba, PR #222 targeting C1's branch at its archive A 2c67a049ee (C1 handed off: H 5a56d480a2, A 2c67a049ee). Inherited parent finding R5-F1 (round-7 review https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/219#issuecomment-7147252673): a late Ghaele that survives its destruction wait, or a further correlated unit, reached only the probe's problems, not the cleanup outcome ProbeSlot reads. Correction (external harness only): OwnedDisposables gains a settlement reader; a returned or already-attempted request is not its result, and a requested unit still present or unreadable is a named failure in the probe's one cumulative outcome (once per unit); positively correlated units are adopted by the destruction owner; same-caster units of unknown ownership are named failures and left in place, never guessed. LateResultReconciliation is what the read-only witness's caller does on every release path (early ReleaseNow, iterator unwind, final release): adopt and request once each correlated unit and record unknown ownership. GhaeleLifetimeProbe observes settlement after each witness release and at the end of the run, and its final release ticks the destroyer and records anything unsettled. The verified post-disposal delivery and visibility fixes are preserved. Evidence: domain sprint7-visual.ghaele-late-units-reach-slot through the real ProbeSlot / Poll / Release / unwind path (late unit still present after its returned request; an additional correlated unit adopted, requested once and settled, and the same unit still present; unknown ownership; unreadable settlement; abandonment before the caller's own settlement; a settled control; each owned unit one request; independent release continues; foreign control untouched; next probe refused when isolation fails); native short lifecycle session PASS on the candidate-3 package with harness 0a038e40c6c1. No product package (0 of 2 used). Native scope is the lifecycle step only.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --gate focused -- -Filter sprint7-visual | yes | pass | 3.8s | 23 tests on H; receipt 2.1s. Includes sprint7-visual.ghaele-late-units-reach-slot (new) and the existing ProbeSlot cases (probe-slot-destroys-each-unit-once, ghaele-early-release-failures-reach-slot, probe-cleanup-reaches-slot, probe-final-releases-per-item), the witness correlation and pending-verdict cases. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --gate focused -- -Filter loader-compat-probe | yes | pass | 3.3s | 5 tests on H; receipt 1.7s. Harness probe lifecycle (ProbeSlot) contracts. |
| Build-LoaderCompatHarness.ps1 (C2 source, identical to c5fe26d146) | no | pass | 2.4s | Harness 0a038e40c6c1da95e59bd943e5972df8940e3b74158eacebe88dcab7ed571ca9. tools/LoaderCompatHarness is unchanged from c5fe26d146 to H (later commits are records only). |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local, C2 source) | no | pass | 18.3s | 23 tests. |
| Invoke-LoaderCompatSession.ps1 s7-ghaele-lifecycle.json on the existing candidate-3 package with harness 0a038e40 | no | pass | 2m20s | Session 20261126T002852Z: package 49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413 / DLL 5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3 (candidate 3, unchanged product contract), git c5fe26d146. Lifecycle only: positive pending verdict null (witness owned/unknown empty); post-disposal late control verdict late-owned-summon, late unit requested once by the witness reconciliation (lateUnitDestroyRequestedByReconciliation true) and observed destroyed; real-spawn witness owned the cast's spawn by execution, its foreign Ghaele known (unknown empty); unsettledAfterWitness and unsettledAtEnd empty; isolationRestored true; transaction completed, owner files and saves verified, registry restored. No timeline, expiry, combat or visibility step (accepted at their own identities). |
| Shared recovery ledger | no | pass | 7m13.0s | Cumulative recovery validation 7 min 13.0 s of 45 (C1 total 4 min 24.6 s; C2 2 min 48.4 s including these gates, 7.7s batch wall, receipts 3.8s). Product candidates 0 of 2. |

**Evidence references:**
- docs/missions/sprint7/recovery/S7-RECOVERY-C2-MISSION.md
- docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md
- docs/evidence/sprint7/recovery-c2/MANIFEST.json
- tools/LoaderCompatHarness/TrumpetCueVerdict.cs
- tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs
- scripts/compatibility/loader-compat/sessions/s7-ghaele-lifecycle.json

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
