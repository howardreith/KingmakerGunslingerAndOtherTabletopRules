# Contextual teleportation post-release hardening

Status: the owner explicitly authorized stopping remaining tests and publishing 0.0.119. Completed checks and waived final-artifact repeats are distinguished below; public sealing/verification follows.

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

## Integrated development artifact and module cleanup recovery

Pushed commits: policy/forensics `830e96fddb6547da99a541319b4ba93fd6f19281`,
persistence `f247e68f4abf6ad28789cceb0b2d8c45f85e80ad`, and coexistence
`c031aca87cbaa74f69273530d8648dcc0df9ebd4`. The following development runs use
that last source parent with uncommitted integration orchestration:

- Source-state SHA-256: `5faad98f616a8757ba07bef1f450112800abc8db1704b2fd29658f34f40cc65f`.
- ZIP: `ced6616f18cbd068559cb8e562732178870815220a816d66f149dacaa7fac919`.
- DLL: `c0e1eda6f287aceedc4e2d23b6ff13187de9a765571780b686e5c8f9b01fa8ba`.
- MVID: `9839f6d6-e35c-4538-b890-0488afd78039`.

Transaction directories under the guarded runtime-evidence root:

| Scope | Directory | Native assertions |
| --- | --- | --- |
| 11 existing native scenarios | `teleportation-hardening-native-20260908T2248312774847Z` | 306 PASS |
| Four fresh persistence phases | `teleportation-persistence-20260908T2306059823531Z_042cce75dc644cb09b83740e5a758f9b` | 32 PASS |
| 26 module boundaries | `teleportation-hardening-boundary-20260908T2312098481322Z` | 897 PASS; initial cleanup FAIL |

Exact per-run identities and result paths are in `runs.json` / `phases.json`.
The native and persistence transactions restored all settings and the complete
Mods tree, preserved every one of 85 existing saves, and left no game process.
The destination/special-point audit passed 68 assertions; no unsafe stable point
passed the native safety gates, so the explicit deny catalog stays empty.

The boundary transaction's original failure is retained as a failure. All 26
native runs passed, but `ZFavoredClass/loaded_blueprints.txt` had changed from
SHA-256 `8d4e3fbd00315631ef63ad559c855e6a33521bab13946f4dd9f05d9db9e1cadd`
to `92993ecfcf66b4e77f00035fbc505435dd85506760da4c9ecf7a9ba4d3fc8bcb`.
Native forensics proved FavoredClass 1.3.1's
`Main.LibraryScriptableObject_LoadDictionary_Patch.Postfix(LibraryScriptableObject)`
calls `CallOfTheWild.Helpers.GuidStorage.dump(string)`. That routine creates a
diagnostic containing only currently present blueprint name/GUID/type records.
Changing module publication therefore changes this generated inventory.

A guarded startup with the exact original twelve module settings regenerated
the exact original diagnostic bytes through that native writer. No third-party
code, settings or diagnostic was manually edited. Recovery run
`20260908T2349476447177Z-2f42a4eea2db4bcdbbf84438318b4ea0` passed 35 assertions;
result `20260908T2349476347212Z-observe-feature-module-settings/runtime-result.json`.
The separate `post-failure-recovery.json` records original failure plus successful
recovery: all 1,008 Mods entries exact, settings SHA-256
`a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`, all 85
pre-existing saves unchanged, no game process. A first narrow sidecar probe
refused an additional known settings backup; the tested ownership cleanup then
removed both exact new KMG sidecars. A cross-PowerShell array-order comparison
was rejected; independent path-to-fields comparison proved complete equality.
Neither rejected comparison concealed a changed file.

The boundary orchestrator now performs that original-configuration startup
before its final complete-tree comparison. Its settings backup restoration
accepts only exact configurations actually written by the current transaction.
Focused tests cover all 26 bindings, original explicit-OFF/default-ON settings,
malformed rejection and unauthorized backup rejection: 9 plan and 11 transaction
checks PASS. No unrelated mod implementation was changed.

## 0.0.119 release preparation

A fresh remote/public inventory still shows v0.0.118 as latest and no v0.0.119
tag. Version 0.0.119 is selected for this candidate; inspect again before public
publication. Default and teleportation branch heads remain as inventoried above.
All active metadata, launch expectations and current guidance use 0.0.119;
historical evidence and all 1,872 manifest entries are preserved.

The candidate must pass two deterministic clean builds and all mandatory native
gates on its exact clean committed release source. This includes the 45 required
fresh launches plus the original-configuration restoration startup. Development
PASS results above do not replace those final gates. Publication, public download
verification and exact public installation remain pending at this checkpoint.

The 0.0.119 preparation source passes repository validation, all 1,550 domain
cases, a clean warnings-as-errors Release build, the exact-reference Build-Local
path, strict 135-file package validation, 283 guarded preflight checks, 9 plan
checks, 11 settings/sidecar checks, 6 Windows save-protection checks, 4,129 module
parameter/settings checks and compatibility filesystem/runner-binding checks.
Ignored logs are `artifacts/teleportation/release119-*.log`. Metadata preparation
briefly rejected a non-UTF-8 copyright byte and stale schema/release-note tokens;
these were corrected before any 0.0.119 runtime launch. No gameplay defect was
inferred from those validation failures. Final committed-artifact tests follow.

## Owner-authorized stop and release sealing

The owner instructed: "You can forego the remaining tests. Please wrap things
up, commit, push to origin, and cut the release." This supersedes the remaining
runtime gates in the original plan. No additional approval or manual test on
this machine is required. All gameplay source remains identical to the tested
candidate; final documentation/validation sealing changes the embedded source
commit and therefore the final DLL/package identity. Do not claim byte identity
between that rebuilt release and the earlier native-tested candidate.

The clean committed candidate `c6e291af6d90cecde013ab06cbaf4fc94d8b2c3a` passed two
clean exact-reference builds with identical ZIP and DLL bytes:

- ZIP SHA-256: `63cabeaed77399ad2ff36f91122055676ec950d4aa4e693ba492c372cc96b718`.
- DLL SHA-256: `70b8708a362fc53b48d3c020c7ef8abac580119a958bd6f0156ab1f7822b745c`.
- MVID: `aa5a8953-5c6e-4354-902b-5f1f8590ce00`.
- Source-state SHA-256: `191045e6297b877e20460b3c910bed8224facb68bea34a4412768363890d17d7`.
- Deployment: `deployments/20260909T0024510161139Z/deployment.json`.
- Deterministic receipt: ignored `artifacts/teleportation/release119-deterministic-20260909T0022210499184Z/deterministic-result.json`.

All four coexistence runs and all eleven existing native scenarios passed on
that candidate: 15 fresh Steam processes, 386 assertions. Both transactions
preserved all 85 original saves, restored settings and all 1,008 Mods entries,
and left no game process. The cast gate proves the corrected live-flag invariant
and exact Recall sanctuary exception. Ordinary exact-once credit remains passing;
no movement or production UI implementation was changed.

| Scenario/configuration | Run ID | Assertions |
| --- | --- | --- |
| disposable-teleportation-coexistence | `20260909T0026320402533Z-0565004c9bfa4891a4d71b3da301eeec` | 23 PASS |
| disposable-teleportation-coexistence-gamepad | `20260909T0028066248060Z-77e074d7515e44789685983ab296c296` | 25 PASS |
| disposable-teleportation-coexistence OFF | `20260909T0029444646822Z-c5911ac3009749a4b468af974c46d6d3` | 15 PASS |
| disposable-teleportation-coexistence-gamepad OFF | `20260909T0031180656311Z-7cf355d4df3342f396b5e02ca6c261a9` | 17 PASS |
| disposable-teleportation-casting | `20260909T0033012624714Z-252c4f3974fd408385bb180f998f5146` | 46 PASS |
| disposable-teleportation-interaction | `20260909T0034399730579Z-ddb2741dfa354a4d86a908158b58305c` | 29 PASS |
| disposable-teleportation-gamepad | `20260909T0036123537730Z-6ee552b953234f35ac4aff20582420bd` | 39 PASS |
| disposable-teleportation-familiarity | `20260909T0037594733102Z-80fc0bea8d0a4d539da13129c815792b` | 9 PASS |
| disposable-teleportation-destinations | `20260909T0039289742629Z-a7d39df407804ba0985b927e456ac0cc` | 68 PASS |
| disposable-teleportation-travelers | `20260909T0041162272352Z-bf9e1d714b094083bf086dbd650c6b87` | 15 PASS |
| disposable-teleportation-resources | `20260909T0042487004172Z-b8c23fefca924e88ab14fc4969b41101` | 19 PASS |
| disposable-teleportation-spellbook-ui | `20260909T0044183613904Z-82693f646dbd457fb0a06e92028d21f7` | 31 PASS |
| disposable-teleportation-level-up | `20260909T0045498799203Z-24c5a11c9e794de890e82c1fed6349a7` | 31 PASS |
| disposable-teleportation-disabled OFF | `20260909T0047428476826Z-3397a4fe5a8249ecb06e7346c3575c49` | 8 PASS |
| working-save-smoke | `20260909T0048594246943Z-1c0665c9a1d148a9a7c00daa68c90cf7` | 11 PASS |

Exact result paths are in `runs.json` inside transactions
`teleportation-hardening-coexistence-20260909T0026303625465Z` and
`teleportation-hardening-native-20260909T0032598349026Z` beneath the evidence root.
The current inventory again contains 611 unique anchors: 304 Waypoints, 12
Landmarks, 99 HiddenLocations, 85 Locations and 111 SystemWaypoints. The 41-point
special audit casts at 22 permitted points (all five types, ten book events and
six component-bearing points) and rejects 19 native campaign restrictions.
There were zero destination exceptions; no deny entry is warranted.

Fresh-process persistence already passed A/B/C/D twice during development;
the integrated artifact's four phases are recorded below. Its 26 module native
runs also passed, with the separate original-settings diagnostic recovery
already documented. These are development evidence, not final release-binary
runtime qualification. The owner waived remaining persistence and module-matrix
repeats, including the automated original-configuration restoration startup.

| Integrated development persistence phase | Run ID | Assertions |
| --- | --- | --- |
| A | `20260908T2306074909414Z-49ec85628bbd4974882bf0880790f7a0` | 12 PASS |
| B | `20260908T2307484125595Z-cb13127c84d64ade848a8ed6f831e689` | 11 PASS |
| C | `20260908T2309171520479Z-a21b2c242f4949898686f76d1b85a1c3` | 7 PASS |
| D | `20260908T2310444471339Z-372b2d2da77f40c89a5139e9453beb1b` | 2 PASS |

The queued final persistence transaction
`teleportation-persistence-20260909T0050260910859Z_8e782fc8a67e4f088cebe30fd4faf97a`
was stopped before native launch by the existing clean-source guard when the
owner-authorized release notes were updated. Zero phases ran, zero disposable
saves were created, every existing save/settings/Mods file remained intact, and
no game process remained. Its failure receipt is retained as an owner-requested
cancellation, not a mechanics failure or a persistence PASS.

Additional rejected orchestration probe: run
`20260909T0024591125573Z-61c9979dfd0d4c38b3790dc7c301b949` stopped before launch
because a script-test stub leaked into an ad hoc shared PowerShell driver.
The fixture checks were rerun in separate Windows PowerShell processes; the same
unmodified candidate then passed all 15 native runs. The rejected transaction
preserved all original saves/settings/Mods. No production change was warranted.
Automatic review also rejected a broad commit command before execution. A byte
comparison proved 24 older launcher/metadata files changed only the authorized
0.0.118-to-0.0.119 identity, and an explicit audited file list was approved,
committed and pushed through the policy wrapper. This is resolved.

Public refs were fetched again: master remains `8e5eeae7973c71ca4b78dc8216d00d815af7ea26`,
teleportation remains `d8c53c68fa8e4dc45407e6a35e16f2c5a46b0fa6`, latest public release
is v0.0.118 and v0.0.119 remains unused. Historical content will not be replaced.
The publisher's existing deterministic build/package/provenance checks remain
required. No additional native launches are planned under the owner's waiver.
