<!-- agent-review:v1 enrollment=kmg-codex-umm-0311-pinned-build-20261124-20261123223003-1af6 key=r05-dispositions-c46e656c5a part=1/1 sha256=c46e656c5af99926b9370fa60884fbc4750a3017214b8a1fb9c7bb1af9681632 -->
### Claude Opus — finding dispositions — round 5

Candidate after corrections: `b16ce7a47ced01a3c4db5388d89b467b08334ee8`

- **R1-F4** — `fixed` — commits: `42e9da86d6`, `1ada444c03`, `b16ce7a47c`
  Test-CandidateAdmission.ps1 on a temporary clone of 42e9da86d6 (62/63; run 14:02-14:20 UTC, report kept in the mission's records): shipped-input-changed-before-packaging-gets-no-receipt-and-is-not-admitted (clean starting fingerprint; README.md changed before the real package.ps1 copied it; the ZIP carries the change; Assert-KmgConsumedSourceState refuses; the receiptless ZIP is not admitted after the checkout is restored; clean control passes).
- **R5-F1** — `fixed` — commits: `42e9da86d6`, `1ada444c03`, `b16ce7a47c`
  Test-CandidateAdmission.ps1 on a temporary clone of 42e9da86d6 (62/63; run 14:02-14:20 UTC, report kept in the mission's records): loader-staging-failed-destination-check-restores-the-previous-mods (an injected destination verification failure after the swap: refused, sentinel back at Mods, no leftovers); loader-staging-stages-the-genuine-tree-completely is the successful-swap control.
- **R5-F2** — `fixed` — commits: `42e9da86d6`, `1ada444c03`, `b16ce7a47c`
  Test-DevelopmentCandidate 98/0: complete-duplicate-deferral-replacing-a-pass-refused, complete-conflicting-outcomes-for-one-case-refused, complete-unbound-skip-as-deferral-refused, complete-valid-deferral-accepted, receipt-deferral-list-mismatch-refused.
- **R5-F3** — `fixed` — commits: `42e9da86d6`, `1ada444c03`, `b16ce7a47c`
  Test-CandidateAdmission.ps1 on a temporary clone of 42e9da86d6 (62/63; run 14:02-14:20 UTC, report kept in the mission's records): package-measurement-call-returns-exactly-one-value (MEASUREMENT-COUNT 1, DEFAULT-COUNT 1 with the whole output captured) and package-measurement-return-leaves-the-zip-equal-to-its-checksum.
- **R5-F4** — `unresolved_blocker` — commits: `42e9da86d6`, `1ada444c03`, `b16ce7a47c`
  Code corrected (outer save/restore of all five variables); the requested in-process evidence (pre-existing values restored after success and after a representative failure) was not run within the validation allowance.
- **R4-F3** — `unresolved_blocker` — commits: `42e9da86d6`, `1ada444c03`, `b16ce7a47c`
  The offline ordinary-admission case exists; its only run failed in the test's JSON extraction (the child itself exited 0), corrected but not re-run within the validation allowance.

**Validation (developer-reported):**

| Command | Required | Result | Duration | Scope and limits |
|---|---|---|---|---|
| scripts/Test-CandidateAdmission.ps1 (temporary clone of 42e9da86d6) | no | fail | 1098s | Test-CandidateAdmission.ps1 on a temporary clone of 42e9da86d6 (62/63; run 14:02-14:20 UTC, report kept in the mission's records): every round-6 case passed except ordinary-admission-child-returns-build-local-and-the-report-projects-it, whose child exited successfully but whose wrapped JSON line the test could not parse. |
| scripts/Test-DevelopmentCandidate.ps1 (round-6 batch) | no | pass | 4.2s | 98/0, including the R5-F2 cases. |
| R4-F3 focused reproduction (temporary clone of b16ce7a47c: development Build-Local, ordinary fixture, admission child, report) | no | fail | 467s | Build-Local exited without a candidate; its output was not retained and the cause is not established; no game was running. |
| Required controller gate | yes | not_run | 0 | Not run: no review is requested this round. |
| Native checks | no | not_run | 0 | Not authorized. |

_Model-generated developer response (Claude opus / high), posted by the agent-review controller through the owner's GitHub account; not a human review._
