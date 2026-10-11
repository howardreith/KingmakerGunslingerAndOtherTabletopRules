# Sprint 21 selected inventory boundary - October 11, 2026

Status: REVIEWABLE CHECKPOINT. The selected candidate preparation and
gunslinger-only inventory observation passed and restoration completed.
Compatibility closure and release readiness remain unqualified. Stop here.

## Scope and source

HOWARD-LAPTOP; worktree
`C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`; branch
`codex/expanded-summoning-sprint21-bebelith-giant-crab`; draft PR #36.
Local and fetched remote HEAD matched the reviewed
`f2fe16ca4fc2c8a8068b07b159f1c97efeb92108`, with a clean tree. That source remained
unchanged through candidate preparation, artifact reuse and restoration.
Source-state fingerprint:
`85be55e7335314b881d9256fce3db3a2df117ebccb77314dcff776665668e98f`.
Current AGENTS.md, validation policy, resume and the
[reviewed focused checkpoint](SPRINT21-FOCUSED-CHECKPOINT-2026-10-11.md) were read.

The original focused 28/28 and repaired reproduction 5/5 retain their identities.
The earlier complete 2554/2559 remains FAIL; counts were not combined. The new
complete-suite PASS below is an independently executed unfiltered run.
The alpha source/tag/assets remain historical and unchanged.

Intake verified no Kingmaker or competing compiler/test process, no active
compatibility transaction and no coordination lock. Fifteen older generic
journals retain Active/RecoveryRequired labels, but repository process-identity
checks found no recorded live owner/game or settings/profile restoration debt.
Those non-owning historical records were preserved, not completed or erased.

Raw logs/receipts remain outside source:
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/sprint21-inventory-boundary-20261011T172958Z`.

## Candidate and binary evidence

Selected one complete pipeline:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-KmgGate.ps1 -Level Sprint -TimingPath '<boundary-log-root>/gate-timings.jsonl'
```

One repository pass, clean complete domain suite, exact-reference Release build,
package assembly and strict standalone validation completed. No separate
Build-Local, Tranche or complete-suite invocation was added. The private-reference
guard matched the actual installed runtime before validation. No validator was
weakened or domain suite skipped.

Complete runner: **2559/2559 PASS**, failures 0, 93.2s. Build used
`C:/Program Files (x86)/Microsoft Visual Studio/18/BuildTools/MSBuild/Current/Bin/MSBuild.exe`
with the domain csproj, `/t:Rebuild /p:Configuration=Release /p:Platform=AnyCPU`.
The script verifies compiler and runner exit zero before continuing.
Executable:
`C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/artifacts/tests/Release/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.exe`.
Mtime `2026-10-11T17:37:35.5912261Z`; SHA-256
`872e2b112310750680cef3693dde5e4e2816bfc968ccded598dccefdca67a2b6`.
The hash equals the previous focused executable because its inputs are unchanged;
the clean Rebuild log, source identity and fresh mtime establish this execution.

Validated package:
`artifacts/local-runtime/0.0.150/KingmakerGunslinger-0.0.150-local-runtime.zip`.
Its `.build-local.json` manifest was generated at `2026-10-11T17:40:06.5191493Z`,
for f2fe16ca and the source fingerprint above, with `validated=true`.

| Identity | Value |
| --- | --- |
| ZIP SHA-256 | c43ca89596bad1d837e34886ef0c7d8193c6532d2426480273bbf50182fd142d |
| Production / deployed / loaded DLL SHA-256 | 60019b72dca27956c418f7195e6809b4b1e59179831094c5a886978a1e575bf7 |
| Production / loaded MVID | 43a5bf9e-92f1-4ca4-b4c8-2eda1a186456 |
| Build-Local manifest SHA-256 | c59d5cbfb949b1172e1644ed2c581693b8183abbd5a3321f5fdfac2098210bcc |

Two supervision limitations are retained in the raw record. The first capture
attempt aborted after 6.59s on ordinary unittest stderr, before completing
repository validation or starting any domain suite, build or package. A bounded
stderr-capture probe passed; separate stdout/stderr capture then completed the
single expensive pipeline. Its Process object did not retain the final child
ExitCode, so the null value remains recorded. Completion is supported by the
guarded full-suite/build/package output, the nonfailed gate timing receipt,
validated matching manifest, independent hashes/MVID and subsequent native
identity. No successful candidate operation was repeated to repair the reader.

## Selected native result and restoration

The profile parameter contract was inspected. Its only invocation was:

```powershell
& .\scripts\compatibility\Invoke-KingmakerCompatibilityProfile.ps1 `
    -ProfileId 'gunslinger-only' `
    -Scenario @('observe-expanded-summoning-inventory') `
    -ReuseInstalledArtifact `
    -PackagePath $validatedPackagePath `
    -Confirm:$false
```

The actual package above was reused without another build. The profile created
fresh deployment receipt
`runtime-evidence/deployments/20261011T1745016666357Z/deployment.json`, SHA-256
`fbbaf67cb610070468fa9a2a642e883ea5ed4349379a7e6ab97c437675767dc2`, under its own
transaction/lease. No external deployment receipt or AllowDirtyGit was passed.
Restoration invalidates that deployment authority for future runs.

Transaction RunId: **compat-20261011T174333Z-e7d434e8ec85**.
Runtime RunId: `20261011T1745029050066Z-ee45a4533f8343248902211198015889`.
Exact runtime directory:
`runtime-evidence/20261011T1745028731776Z-observe-expanded-summoning-inventory`.
Runtime-result SHA-256:
`ffbd3ced662c18c435d8f4910398049b9883478ca183242975dd9f84ee5de3b2`.
Steam App ID 640820, game 2.1.7b, mod 0.0.150, embedded source f2fe16ca;
loaded-build identity independently matches the candidate DLL hash and MVID.

Structured inventory: **50/50 PASS**, no failed assertions. Bebelith's twelve
reported subchecks are all `ok`, specifically:

- `bitePrimary2d6=ok`: compiled predicate checks the primary native weapon GUID
  `d2f99947db522e24293a7ec4eded453f` and actual Damage.Rolls=2 / Dice=D6.
- `twoClaws2d4=ok`: the two additional limbs are the owned claw identity, 2d4.
- `noSecondaryLimbs=ok`: the additional secondary-limb array is empty.

This native run qualifies the 20efcd71 census correction and subsequent dice
observer on these bytes. It is structural inventory evidence, not combat,
persistence or all-profile qualification. No production change was made.

The receipt reports **Restored / restorationVerified=true** at
`2026-10-11T17:47:33.9314130Z`; profile driver exit 0. The final independent check
used `Test-KmgCompatibilityManifestEqual` directly on the intake/current arrays:
all **968 entries match exactly**. FeatureModules.json, absent Gunslinger
settings.json, Favored Class and CotW settings match actual intake. The guarded
transaction also restored the managed SoundBank. Kingmaker and the selected
profile owner exited, no active compatibility transaction or lock remains,
and the source fingerprint still matches f2fe16ca. No recovery was required.
The newly observed installation was 0.0.117; it was restored from actual intake,
not from a remembered version/fingerprint. No gameplay save load/write or
protected-baseline access occurred; request parameters are empty and
orchestration records `saveInteractionOccurred=false`.

Measured pipeline: 278.70s. Profile including staging, launch and restoration:
241.72s. Combined **8m40s**, within the 6-10 minute estimate. Native result duration
124.10s. Logged intake at 17:29:58Z through final restoration verification at
17:49:43.831488Z: **19m46s**, including supervision repair and evidence checks,
within the 30-minute aggregate ceiling. Result documentation follows restoration
and has separate Git provenance; the tested package was not rebuilt for it.

## Deferred boundary

Stop after this record. Brown-Fur provider absence remains diagnosed, not fixed;
the three teleportation causes and three vendor discrepancies remain unresolved
as classified in the reviewed checkpoint. Matched released149 comparison, other
profiles including high-risk-combined, whole-roster persistence, full matrix and
final release qualification remain pending. No release waiver, master merge,
tag, publication, permanent deployment, DATA work or Sprint 22 occurred.
