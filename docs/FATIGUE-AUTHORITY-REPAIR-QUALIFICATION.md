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
directories are recorded here after execution.

| Gate | Result | Evidence |
|---|---|---|
| Repository validation | PASS | Version-aware 0.0.106 validator and all inherited gates |
| Complete clean Release domain suite | PASS | 1,323/1,323; failures=0 |
| Clean Release/local package | PASS | `scripts/Build-Local.ps1`; exact private-reference build |
| Strict package validation | PASS | Official and local-runtime archives both accepted |
| `disposable-native-fatigue-refresh` | Pending | Pending |
| `disposable-acadamae-fatigue-escalation` | Pending | Pending |
| `disposable-acadamae-graduate` | Pending | Pending |
| `disposable-cord-of-stubborn-resolve` | Pending | Pending |
| Three-launch working-save fatigue persistence | Pending | Pending |
| `working-save-smoke` | Pending | Pending |

### Qualified pre-runtime artifacts

- Official candidate archive:
  `artifacts/packages/KingmakerGunslinger-0.0.106-fatigue-authority-repair.zip`
- Guarded local-runtime archive:
  `artifacts/local-runtime/0.0.106/KingmakerGunslinger-0.0.106-local-runtime.zip`
- Both archive SHA-256 values:
  `92C6DCC5DEF7DADD36C7E8DC0810B81F37E5E3D73384E563FA23367E75366F35`
- Release DLL SHA-256:
  `2709023C7A224B4BD4830728C5B4D841EB450A3F77CB0205E3CDCB43AEA43D62`
- Release DLL MVID: `cda089c1-a398-45db-950e-65b448c8038c`
- Guarded runtime preflight: 157/157 PASS.

All autonomous runtime rows must use the guarded `-kmgRuntimeTestRequest`
mechanism through Steam App ID 640820. Only `KMG_AUTOMATION_WORKING` may be
written. `KMG_AUTOMATION_BASELINE` is outside the mutation boundary.

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
