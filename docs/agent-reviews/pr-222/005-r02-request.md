<!-- agent-review:v1 enrollment=kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 key=r02-request part=1/1 sha256=7a6e994620903e13b6cf717a77f7a853958cd86eb38aa4ee8992b3d04a22ecc7 -->
### Claude Opus — review request — round 2 — candidate `525a27751b`

| Field | Value |
|---|---|
| Request ID | `kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08-r02-a67b64` |
| Checkpoint | `final` (final) |
| Candidate H | `525a27751b95fd51f3080b2a7865719532fff298` |
| Target B (`codex/s7-recovery-c1-cue-evaluator`) | `2c67a049ee33e0b29621fd29a8b25608ed31e956` |
| Merge base M | `2c67a049ee33e0b29621fd29a8b25608ed31e956` |
| Mission | `s7-recovery-c2-mission` sha256 `f54c407bac` |
| Workflow | `6d613defe7` |
| Owner guidance | none |
| Owner amendments | none |
| Developer observed | foreground session `438fdcd7-910d-4c0e-8441-dde58c90713d`: models ['claude-opus-5-5'], effort ['high'], permission modes ['auto'] (CLI-recorded transcript; the headless permission profile does not apply to a foreground session) |
| Reviewer requested | Codex `gpt-6.2-astra`, effort `max`, read-only sandbox |
| Round budget | 2 of 2 |

**Scope:** The complete mission as written.

**Exit conditions:**
- Every requirement of the mission is implemented, with evidence appropriate to each claim; no requirement is weakened or silently dropped.
- Validation follows the repository's own instructions; commands, results and limits are reported honestly.
- Deviations, limitations and deferred optional items are stated explicitly, separate from required work.

**Developer summary:**

Sprint 7 recovery child C2, review 2 of 2, on H=525a27751b, PR #222 targeting C1's branch at its archive A 2c67a049ee. All eight round-1 findings are fixed in the external harness only: settlement is the engine's Destroyed mark (pending/detached units unresolved, never settled); unknown identities stay named failures even when registered for teardown; the witness caller releases subscription and reconciliation independently; no global destroyer drain; one terminal snapshot feeds row and outcome; the late verdict requires the reconciliation's own request and the foreign controls are read alive and unrequested after reconciliation and settlement; the production orchestration is what the domain test drives through ProbeSlot; the ledger is a single table. The read-only witness, the verified post-disposal delivery order and one destruction request per owned unit are preserved. Native: the short lifecycle session requalified PASS on the unchanged candidate-3 package with the rebuilt harness bf5d74a12487 (built from 44fd90a8d1), restoration confirmed. No product package (0 of 2). Recovery validation 9 min 40.5 s of 45. C3 retains the combined product qualification.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --gate focused -- -Filter sprint7-visual | yes | pass | 4.0s | 23 tests on H; receipt 2.1s. Includes the rewritten sprint7-visual.ghaele-late-units-reach-slot and the existing ProbeSlot cases. |
| python C:/Dev/agent-review/agent_review.py verify --enrollment kmg-codex-s7-recovery-c2-ghaele-late-units-20261125192639-5a08 --gate focused -- -Filter loader-compat-probe | yes | pass | 3.4s | 5 tests on H; receipt 1.6s. |
| Build-LoaderCompatHarness.ps1 (round-2 source 44fd90a8d1) | no | pass | 2.2s | Harness bf5d74a12487a92ac4d00a792a17a1ecaa41407308728ee7cc3d4d9f405163cd. tools/LoaderCompatHarness is unchanged from 44fd90a8d1 to H (the later commit is records only). |
| Invoke-KmgGate.ps1 -Filter sprint7-visual (local, round-2 source) | no | pass | 3.9s | 23 tests; test binary rebuilt after the test edit (timestamps checked). |
| Invoke-LoaderCompatSession.ps1 s7-ghaele-lifecycle.json on the existing candidate-3 package with harness bf5d74a1 | no | pass | 2m14s | Session 20261126T005206Z (window 00:52Z-00:54Z agreed with the other Kingmaker session, closed by message): package 49f8b88c052ca448b214552a6aab7a0e615697c10ecbea842ae013456faf6413 / DLL 5969f596811984eaaf69014bc3bc8ddb39d733d8837cce280a76655abd98e2c3 (candidate 3, unchanged), git 44fd90a8d1, staged harness bf5d74a12487. Lifecycle step only: pending verdict null with witness owned/unknown empty and foreignPreserved true read after reconciliation and settlement; post-disposal late control verdict late-owned-summon, lateUnitDestroyRequestedByReconciliation true, lateUnitDestroyed true (engine Destroyed mark), foreignPreserved and foreignUnrequested true; real-spawn case earlySummonDestroyed true, foreignPreserved and foreignUnrequested true; unsettledAfterWitness empty in all three cases, unsettledAtEnd and unsettledAtFinalRelease empty; problems empty; isolationRestored true; transaction completed, owner files verified, registry restored. Extracted to docs/evidence/sprint7/recovery-c2/. |
| Shared recovery ledger | no | pass | 9m40.5s | Cumulative recovery validation after these gates: 9 min 40.5 s of 45 (the committed ledger stops at 9 min 33.1 s, before these controller gates: 7.4s batch wall, receipts 3.7s). C2 round 2 spent 2 min 27.5 s. No failed attempt in C2. Product candidates 0 of 2. |

**Evidence references:**
- docs/missions/sprint7/recovery/S7-RECOVERY-C2-MISSION.md
- docs/missions/sprint7/recovery/S7-RECOVERY-MANIFEST.md
- docs/evidence/sprint7/recovery-c2/MANIFEST.json
- docs/evidence/sprint7/recovery-c2/20261126T005206Z-owner-0.33.0-s7-ghaele-lifecycle-01-ghaele.json
- tools/LoaderCompatHarness/TrumpetCueVerdict.cs
- tools/LoaderCompatHarness/GhaeleLifetimeProbes.cs
- tests/KingmakerGunslinger.DomainTests/Sprint7VisualRepairTests.cs

**Prior findings to assess:** `R1-F1`, `R1-F2`, `R1-F3`, `R1-F4`, `R1-F5`, `R1-F6`, `R1-F7`, `R1-F8`

_Posted by the agent-review controller through the owner's GitHub account on behalf of the Claude Opus developer. Automated request; not a human review._
