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

## Fresh-process persistence checkpoint

Policy/loader commit `830e96fddb6547da99a541319b4ba93fd6f19281` is pushed.
The new persistence harness is documented in
[TELEPORTATION-PERSISTENCE-QUALIFICATION.md](docs/TELEPORTATION-PERSISTENCE-QUALIFICATION.md).

Development artifact: ZIP `1f3acce7b03f3e4fdf537e198fc09348b486789e36ea19dfa240c3400b0e99ea`,
DLL `9c0950bde194b1037269eee7d491b4c3437b3bf6f720ff42f977d9cc1df99300`,
MVID `2b347d2b-87fb-4bb7-885d-3da1e5b68bfd`, source-state
`b572bcc43143b81740db4511a3f3e037968b917c88846d383fddb7f6c08c6551`.
Deployment: `deployments/20260908T2150221586271Z/deployment.json`.
Transaction directory beneath the evidence root:
`teleportation-persistence-20260908T2150222561999Z_98ff6a1c8c1e4b6db9df61dc5ae0da7f`.

| Phase | Fresh process | Run ID | Assertions |
| --- | --- | --- | --- |
| A: establish/native save | 17716 | 20260908T2150242698997Z-106e2046fd4544438dc9ab88920c7f7b | 12 PASS |
| B: reload/travel/cast/save | 24944 | 20260908T2152055558836Z-faef0cad17af420ca8c83d14558882d7 | 11 PASS |
| C: module OFF/reload/save | 28008 | 20260908T2153375576905Z-52f5afc00a6c4bdaaa7963f8040a82c0 | 7 PASS |
| D: module ON/fresh reload | 19292 | 20260908T2154497622660Z-f5f61fa5cc5941e49c5a289dbbbb3ebf | 2 PASS |

All 32 assertions passed on the same artifact. Native campaign ZIP saves A, B,
and C passed header and exact UnitPart checks. Fresh processes restored the one
canonical owner, format-1 payload, migration flag, counts and exploration
boundary exactly. Both Teleport families used positive persisted visits. Real
contextual casts consumed one native prepared use and added no familiarity.
Native intermediate/final ordinary boundaries each incremented exactly once;
the alleged duplicate count remains unreproduced and movement code is unchanged.

Cleanup removed only transaction-owned Manual_303/304/305 A/B/C saves. All 85
pre-existing files, including Baseline and Working, retained hashes and metadata.
Settings bytes and the complete Mods tree matched the initial inventory; no game
process remained. New KMG settings backup and UMM cache cleanup is recorded in
`owned-mod-sidecar-cleanup.json`. No raw artifacts or saves are committed.

Current source checks: repository validation PASS; full domain suite 1,550 PASS;
clean Release build with warnings as errors PASS; strict UMM package PASS;
guarded preflight 283 PASS; settings/ownership/sidecar transaction 9 PASS;
Windows save-protection 6 PASS; exact module parameter/settings 4,129 PASS;
compatibility filesystem transactions and runner bindings PASS.

Rejected development probes retained truthfully:

- Three fixture compilation errors and an isolated OrderedDictionary copy error
  were fixed before campaign writes.
- A PowerShell 7 compatibility-fixture comparison rejected timestamp string
  precision (`...428504Z` versus `...4285040Z`) although bytes and timestamps were
  unchanged. The documented Windows PowerShell runner passed; production profile
  code was not changed for that formatting difference.
- Transaction `20260908T2126046632787Z_9fe1ffebea7048d79d56b86a70fe0223`
  stopped before launch on a PowerShell path-walk property error. All original
  saves/settings/Mods remained unchanged.
- Transaction `20260908T2129139088157Z_8878bdaa90574a2f9fa71ee28600d13f`
  passed A/B but failed C when a fixture snapshot assumed a point anchor during
  native Travel. No production defect was inferred. Its two owned saves were
  deleted and all existing saves preserved. The initial full-tree check found
  newly created KMG `.previous` and exact DLL cache files; precise subsequent
  cleanup restored the complete original tree, recorded in
  `post-failure-recovery.json`. This attempt is not a persistence PASS.
- Automatic approval review rejected an isolated test edit after interpreting
  its fixture DLL path as the live assembly. The test now explicitly confines
  its directory to repository artifacts/tests and uses a harmless `.bin`
  payload. All nine transaction checks pass; no installed DLL was overwritten
  with fixture content.

UI coexistence, final artifact native scenarios/module matrix, deterministic
release builds and public package verification remain required. These development
results are not substituted for qualification of the eventual release commit.


## UI coexistence development checkpoint

All four guarded fresh-process coexistence runs passed on source parent
`f247e68f4abf6ad28789cceb0b2d8c45f85e80ad`, source-state SHA-256
`5c6d879b70c8b056723e20c61a2955e2445014d8926df523abdbdaec42eaf832`.
ZIP `5b206553da8d80484d6de3144c236f0cb4b22825eda9bb785c39cc7846f7c14a`;
DLL `24156a39e4aea51faa27f33b0919b4d0bc444a002c81eb110add7dfb738b49a7`;
MVID `b6d8d1b2-41f8-41b7-9c8d-35bbb51a0de2`.
Deployment: `deployments/20260908T2232446583798Z/deployment.json`.

| Controller/module | Run ID | Assertions |
| --- | --- | --- |
| Desktop ON | `20260908T2232467118200Z-099298049ea84af59f035e31f4876961` | 23 PASS |
| Gamepad ON | `20260908T2234202574244Z-7e42ceada0904cdaa4334d23aee22ec4` | 25 PASS |
| Desktop OFF | `20260908T2235577630744Z-6fa6f1d5822f46989a00ae5effb3c2a0` | 15 PASS |
| Gamepad OFF | `20260908T2237297184946Z-f9e2ec2882cd4d8f9f524f6f68dedef3` | 17 PASS |

Transaction `teleportation-hardening-coexistence-20260908T2232447428920Z`
contains `runs.json` with exact runtime-result paths and
`transaction-result.json`: 80 assertions PASS, all 85 pre-existing saves
unchanged, settings and complete Mods tree restored, no game process remaining.
Control identity/content/callback, native Travel once, no-source/OFF zero UI
construction, exhaustion, dismissal/modal/disposal ownership, gamepad navigation
and input-layer retention all passed. No production UI change was warranted.
See `docs/TELEPORTATION-UI-COEXISTENCE.md` for exact fixture boundaries.

Rejected probes: a compile-time Harmony12 field spelling mismatch was corrected
before launch. Native run `20260908T2225577176895Z-22d81f4a4c604de885f4160d00c46eb3`
failed in the new foreign fixture because Unity's active-only ancestor lookup
could not find a button before native Fill activated the panel. Explicit inactive
ancestry inspection fixed the probe; no production defect was inferred. Its
transaction verified every save, setting and Mods file unchanged. A separate
profile-resolution dry run stopped before staging because its default example
reference root was absent; the mandatory isolated compatibility filesystem and
runner-binding tests passed under Windows PowerShell.

Repository validation, full 1,550-test domain suite, clean Release build, strict
package validation, 283 preflight checks, 9 settings/ownership checks, 6 save
protection checks and 4,129 module parameter/settings checks passed. The unchanged
persistence restoration functions now live in the shared guarded script library;
the same isolated tests execute those exact functions.

Final committed artifact regression/matrix, persistence/coexistence reruns,
deterministic release builds and public package verification remain required.
