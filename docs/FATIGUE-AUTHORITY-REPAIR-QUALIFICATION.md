# Fatigue authority repair qualification

## Scope and invariant

This record qualifies the 0.0.106 repair candidate. Its governing invariant is:

> Ordinary Kingmaker fatigue can never become Exhausted merely because KMG
> observed a refresh or reapplication of Fatigued. Only native Kingmaker
> applying Exhausted, or an explicitly scoped Acadamae Graduate failed-save
> escalation, may increase the condition to Exhausted.

No travel class is patched. Native `RuleApplyBuff` remains first and native
rejection or immunity remains authoritative. Cord coordination remains after a
successful native application and after the effective incoming condition is
resolved.

## Installed Kingmaker 2.1.7b contract evidence

The installed `Assembly-CSharp.dll` inspected for this repair has SHA-256:

`3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB`

Read-only metadata/IL inspection produced these exact contracts:

- `BuffCollection.TriggerRuleApplyBuff`, metadata token `0x060029F6`, has a
  93-byte body. It rejects disabled/dead targets, constructs `RuleApplyBuff`
  with `AddBuffInternal` as the application delegate, triggers the rulebook,
  and returns `AppliedBuff`. KMG's Harmony postfix therefore observes native
  success; it does not suppress or synthesize the native call.
- `BuffCollection.AddBuffInternal`, token `0x060029F7`, has a 647-byte body.
  For a non-Stack blueprint it finds the existing exact blueprint fact.
  `StackingType.Prolong` returns that same live `Buff`; a null duration makes it
  permanent and a finite request extends the end only when the requested
  absolute end is later. The canonical Fatigued and Exhausted blueprints are
  both verified at guarded runtime as `StackingType.Prolong`.
- `UnitPartWeariness.Tick`, token `0x06002186`, has a 142-byte body. At the next
  weariness threshold it increments `WearinessStacks`, records the stack time,
  and calls `ApplyBuff`. Independently, it calls `ApplyBuff` again when the last
  application is at least one in-game hour old.
- `UnitPartWeariness.ApplyBuff`, token `0x06002188`, has a 116-byte body. Stack
  0 returns. Stack 1 selects `BlueprintRoot.SystemMechanics.FatigueBuff`; every
  higher stack selects `ExhaustedBuff`. It applies the selected exact blueprint
  through the unit's buff collection and makes the accepted result permanent.
- `UnitWearinessController.Tick`, token `0x060091B4`, walks the current player
  party, ensures `UnitPartWeariness`, and ticks each member.

### Exact travel behavior

World-map travel advances game time; the weariness controller is the native
authority consuming that time. At the first threshold, `WearinessStacks`
becomes 1 and native code requests canonical Fatigued. That application updates
the last-apply time, so the same threshold transition does not issue a duplicate Fatigued request.
While the stack remains 1, native code reapplies Fatigued once
per in-game hour. Because the blueprint uses `StackingType.Prolong`, this is a
same-reference refresh/merge. At the later native threshold, the stack becomes
2 and native code explicitly requests canonical Exhausted.

Fast world-map time can cross the hourly refresh shortly after the first
threshold in wall-clock time. The 0.0.103-line KMG policy treated that successful
hourly Fatigued refresh as a second independent fatigue event and replaced it
with Exhausted. This is the verified source of the reported premature travel
exhaustion; there is no separate travel-specific KMG defect to patch.

## Historical comparison

Immutable Git inspection compared the active implementation with commit
`1fd5577c08c5d7e5cf9b65c9e346cd1eec6c836b` and merge commit
`cf1ca7aedf34ee76690f8864daedc9319a8e21a6`. Before the former, Acadamae's
failed-save consequence directly applied the canonical Fatigued blueprint and
made that accepted fact permanent. Commit `1fd5577...` introduced the global
canonical coordinator and a policy that escalated every successful repeated
Fatigued application. The merge retained it. The repair does not revert that
commit because it also contains accepted Overhaul Firearm and Expanded
Summoning work.

Prior structured 0.0.103 evidence also encoded the defect: the old
`disposable-fatigue-escalation` fixture expected a second same-frame canonical
Fatigued request to produce Exhausted. That expectation is retired, not carried
forward under a new name.

## Repair architecture

- Every arbitrary global Harmony observation defaults to
  `NativePassthrough`.
- `ApplyPermanentAcadamaeFatigue` enters
  `EscalateIfAlreadyFatigued` only around its exact native canonical Fatigued
  call.
- The intent is `[ThreadStatic]`, keyed by exact reference identity for the
  `BuffCollection` and expected blueprint, one-shot, parent-linked for nesting,
  and disposed by `using`/`finally` semantics.
- A nested unrelated application cannot inspect a parent request. A second
  unit, a later application, a second same-scope claim, another blueprint, an
  exception, or another managed thread receives native passthrough semantics.
- Native Fatigued passthrough leaves the successful native `Buff` reference and
  duration untouched. When Exhausted already exists, only a newly introduced
  exact weaker fact is removed; the existing exhaustion reference and duration
  are not extended or replaced.
- Explicit Acadamae escalation remains post-success, preserves the strongest
  applicable duration/permanence, and does not erase a successful fatigue fact
  if native exhaustion is rejected.
- Cord still receives the intent-resolved incoming kind after native success.

## Automated qualification

The version-aware repository validator, 1,323-case domain suite, clean Release
build, strict package validation, package/DLL hashes, and guarded runtime result
directories are recorded here after execution. All six runtime qualifications
completed with 86/86 assertions passing and no warnings.

| Gate | Result | Evidence |
|---|---|---|
| Repository validation | PASS | Version-aware 0.0.106 validator and all inherited gates |
| Complete clean Release domain suite | PASS | 1,323/1,323; failures=0 |
| Clean Release/local package | PASS | `scripts/Build-Local.ps1`; exact private-reference build |
| Strict package validation | PASS | Official and local-runtime archives both accepted |
| `disposable-native-fatigue-refresh` | PASS | 10/10; `20260828T1249048399379Z-disposable-native-fatigue-refresh` |
| `disposable-acadamae-fatigue-escalation` | PASS | 10/10; `20260828T1251253456101Z-disposable-acadamae-fatigue-escalation` |
| `disposable-acadamae-graduate` | PASS | 20/20; `20260828T1255307622173Z-disposable-acadamae-graduate` |
| `disposable-cord-of-stubborn-resolve` | PASS | 14/14; `20260828T1253292879175Z-disposable-cord-of-stubborn-resolve` |
| Three-launch working-save fatigue persistence | PASS | 7/7 each: `20260828T1257393331889Z-working-save-fatigue-prepare`; `20260828T1259486417089Z-working-save-fatigue-verify-cleanup`; `20260828T1301583102651Z-working-save-fatigue-verify-absent` |
| `working-save-smoke` | PASS | 11/11; `20260828T1304368014092Z-working-save-smoke` |

### Qualified immutable artifacts and deployment

- Official candidate archive:
  `artifacts/packages/KingmakerGunslinger-0.0.106-fatigue-authority-repair.zip`
- Guarded local-runtime archive:
  `artifacts/local-runtime/0.0.106/KingmakerGunslinger-0.0.106-local-runtime.zip`
- Both archive SHA-256 values:
  `6FBD4ECC0DC025036E0E15677BCE74A112BBC6E4EFC979F8FC1C783AA5CEE507`
- Release DLL SHA-256:
  `8EC54F3F5D8EDF55F04851AEA224807C08EF77F9F1870600B3EF989441E59393`
- Release DLL MVID: `b5fcdf91-4f2d-4775-b009-b21e9cc63e33`
- Candidate source commit:
  `9cc702f3ebe6a86e047fa4c1e3b89cd19044f123`
- Candidate branch: `codex/repair-global-fatigue-escalation`
- Source-state SHA-256:
  `b1c1db4c7331619657808f21bb5b9f59fa42c13568e98bd82c491ce5480466a3`
- Guarded runtime preflight: 157/157 PASS.
- Pre-deployment live-mod backup:
  `C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260828T1248349312809Z`
- Verified deployment manifest:
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260828T1248381692477Z\deployment.json`
- The deployed DLL hash exactly matched the build manifest. The repository
  workflow preserved `FeatureModules.json` with SHA-256
  `28B9589DB49EF977D2A033AA563052930A1D0E37E920689DB746BD0AF9108B59`.

Every runtime row used the guarded `-kmgRuntimeTestRequest` mechanism through
Steam App ID 640820 and reused the exact verified deployment above. The
three-launch persistence sequence wrote only `KMG_AUTOMATION_WORKING`; its
final launch proved the cleanup persisted. `working-save-smoke` separately
proved no save-writing API ran. `KMG_AUTOMATION_BASELINE` was never selected,
loaded, modified, overwritten, renamed, or deleted.

## Remaining supervised world-map acceptance

World-map pathing is intentionally not automated through mouse coordinates,
OCR, Computer Use, or inferred UI state. After every autonomous gate passes, a
human may perform this concise supervised world-map acceptance on an approved
disposable or working save:

1. Start with a fully rested party and begin ordinary world-map travel.
2. At the first native weariness threshold, confirm party members are
   Fatigued, not Exhausted.
3. Continue travel until Kingmaker's later native exhaustion threshold and
   confirm Exhausted appears only when native weariness applies it.
4. Separately fail an Acadamae Graduate save while already Fatigued and confirm
   one permanent/rest-removable Exhausted fact.
5. Separately confirm Cord receives ordinary Fatigued as Fatigue, native
   Exhausted as Exhaustion, and Acadamae escalation as Exhaustion, with one
   damage event and the 1-HP floor.

This manual observation is not a substitute for the structured autonomous
evidence and does not authorize save selection or input automation.
