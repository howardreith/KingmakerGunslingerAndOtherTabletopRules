# PR #19 generic runtime lease follow-up

Baseline: `3794fb57bc4ba536d26fce1ca1bafb7bd6dbea23`, clean feature
worktree `codex/magic-circle-alignment-spells`. Preserve accepted R1, R2,
C1, approved art and deferred mechanics. This assignment changes shared
orchestration only; compiled product changes are not intended.

- [x] Reproduce generic leave-open success becoming an error with retained lock,
  using the actual launcher and disposable coordination/files.
- [x] Distinguish a deliberate handoff from recovery; retain parent ownership.
- [x] Guard generic completion/recovery by exact run, process and file identities;
  reject pending restoration, foreign/live owners and competing recovery.
- [x] Run production entry-point regressions, including interruption and WhatIf;
  retain Circle settings/preparation/selector coverage and required gates.
- [x] Record applicable native evidence and exact artifact attribution.
- [ ] Publish coherent commits through the guarded helper, without merging.

Integration inspection: GitHub reports draft PR #19 CONFLICTING/DIRTY against
master `d7fe028c8fa60f85b5f7c58a38207088f17c7546`. Read-only `merge-tree`
identifies conflicts in `scripts/RuntimeAutomation.Common.ps1`,
`src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs`,
`tools/validate_bodyguard90.py`, `tools/validate_icon_overhaul132.py`, and
`validation/static-validation.json`. No branch/index/worktree integration
performed; AGENTS.md forbids autonomous merges.

Qualification: 39 new production launcher assertions; retained Circle/shared
suites; repository validation; clean 1,660-domain-test suite; clean Release and
package PASS. Three stale broader test expectations remain separately recorded.
Both applicable native runs passed on the new provenance-bearing DLL. All 136
normal-play files/settings restored; no game or shared lock remains.
See [the evidence report](../reports/magic-circle/PR19-LEASE-FOLLOWUP.md).
