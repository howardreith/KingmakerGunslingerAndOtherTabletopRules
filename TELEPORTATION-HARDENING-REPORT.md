# Contextual teleportation post-release hardening

Status: implementation and automated qualification in progress. No new release is qualified or published by this repair pass yet.

## Source inventory before editing

- Clean base / previous evidence commit: `d8c53c68fa8e4dc45407e6a35e16f2c5a46b0fa6`.
- Repair branch: `codex/teleportation-post-release-hardening`.
- Fresh remote teleportation branch: same `d8c53c68fa8e4dc45407e6a35e16f2c5a46b0fa6`.
- Published v0.0.118 source: `b439f5df22e2260322453c069b23eaee734b8a33`.
- Fresh default/master: `8e5eeae7973c71ca4b78dc8216d00d815af7ea26`.
- Remote elemental branch: `505fdd4`; its tree equals current master exactly. No newer accepted source change needs reconciliation. The repair base retains published 0.0.117 default-branch content additively.
- Latest public release on inspection: v0.0.118, 2026-09-08T17:48:27Z. No later public release was present. Recheck immediately before selecting the next patch version; v0.0.119 is not reserved by this report.
- No pre-existing uncommitted files were present. No history was rewritten and master was not modified.

## Defect and repair

Live native exploration flags were accepted by the shared destination policy, while only ordinary Teleport required familiarity. Greater Teleport could therefore offer a destination with zero persisted ordinary/migrated arrivals. Native writer forensics proves these flags are not reliable live physical-arrival evidence. Both Teleport families now require the persisted positive count. Recall retains exact sanctuary eligibility through separate general safety checks.

The alleged duplicate-boundary count is not reproduced. The existing invocation-local observer, open/closed distance interval, and idempotent completion are retained unchanged.

## Development checks

Initial policy change: repository validation PASS; all 1,550 domain tests PASS; clean Release build PASS with warnings as errors; strict standalone UMM package PASS. Log: ignored `artifacts/teleportation/hardening-policy-build.log`. These checks are not native runtime qualification or release evidence.

The guarded loader now preserves the original ZIP, including LoadedTimes. An exact request-owned ISaver decorator suppresses only one proven native header increment and its commit. Other writes are rejected. Independent Windows read leases forbid writes/deletes/renames to every pre-existing save during qualification. Six filesystem protection assertions PASS; the expanded header protocol tests pass in the full domain suite. Guarded preflight: 279 PASS. Exact module parameter/settings checks: 4,129 PASS. Compatibility profile filesystem transaction/binding tests: PASS.

## First native repair checkpoint

These are development qualification results on the exact uncommitted source state below; they do not replace final committed-release qualification.

| Scenario | Run ID | Result |
| --- | --- | --- |
| working-save-smoke | 20260908T2026320766312Z-cff9000bf8c2416a8c449789102bb279 | PASS, 11 assertions |
| disposable-teleportation-casting | 20260908T2029129142944Z-2b35853b1ab44e4f9a0de9825a4d6153 | PASS, 46 assertions |
| disposable-teleportation-familiarity | 20260908T2031479112469Z-ab4b99cec3904372ae59f35bff4ee710 | PASS, 9 assertions |

Result directories beneath `C:/Dev/KingmakerGunslingerLab/runtime-evidence/`:
`20260908T2026320676301Z-working-save-smoke`,
`20260908T2029129033016Z-disposable-teleportation-casting`, and
`20260908T2031479012449Z-disposable-teleportation-familiarity`.
Deployment: `deployments/20260908T2026320330705Z/deployment.json`.

- Source-state SHA-256: `ec9bdb2388f9dbb0227412142589e662454d56f6b38d98cd18709468ea23b964`.
- Candidate ZIP: `28dd8719dda701e4d7f5583198bef3cef8bb496f88f8d0777f2c3ada335f8c80`.
- DLL: `33f190a2b0e1bf59986d2700f988f3d5a348a2f8297debd0b80bbc680bbe6751`.
- MVID: `a1dc4288-8e10-4b18-bcb7-c4e35cffe53a`.
- Module settings SHA-256 retained: `a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`.

The actual native MarkLocationExplored action, followed by reveal/open/seen flags, produced no Teleport or Greater Teleport rows with a migrated empty ledger. Exact pre-capital Recall remained available without a Teleport visit count. Three native ordinary two-edge trips each credited intermediate and final boundaries exactly once. The earlier duplicate-count concern was not reproduced; no movement patch was changed.

All 85 pre-existing save files retained their full SHA-256, length, creation/write times, and attributes in all three runs. The normal native load preserved the working header exactly; there were no new save files and no game process remained. Protection reports: `hardening-save-protection-20260908T2025396335308Z`, `hardening-save-protection-20260908T2029118188070Z`, and `hardening-save-protection-20260908T2031468113536Z` beneath the evidence root.

A fixture-local C# name collision and a PowerShell parameter parse error were corrected before runtime. Neither attempted a campaign load or write. Fresh-process disk persistence, UI coexistence, the final native suite, module matrix, deterministic release builds, and public package verification remain required.
