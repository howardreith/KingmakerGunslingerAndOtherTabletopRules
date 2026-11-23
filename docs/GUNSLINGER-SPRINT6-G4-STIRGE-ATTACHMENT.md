# Sprint 6 checkpoint G4 (VER-02): retained Stirge attachment lifecycle and the shared Remove Stirge action

Status: **qualified natively on the accepted C3a product with no product change; returned for manual review**
(2026-11-23). VER-02 stays open. Nothing is merged, released or republished.

- Branch `codex/s6-ver02-g4-stirge-attachment` is stacked on the C3c/G3 acceptance-records head `631a66e5cf`
  (`codex/s6-ver02-c3c-flytrap-multi-engulf`, draft PR #209). The stack is not rebased.
- **Product (unchanged, reused):** accepted C3a `9e902dfa80`. Package `704cb0b4…79c9`, DLL `431f13da…5676`.
- **Harness:** `tools/LoaderCompatHarness/StirgeProbes.cs` (step `kmgStirge`, modes `lifecycle`, `save` and `reload`),
  plus read-only Stirge observation added to `HoldLifecycleObserver`. Builds: `af6a2d27…55ca1` (first session) and
  `545865cf…d018` (lifecycle rerun, oracle fix only).
- **Sessions:** `ver02-g4-stirge-attachment.json` (lifecycle, save, reload) and `ver02-g4-stirge-lifecycle.json`
  (lifecycle only).
- **Fixture:** `KMCS_AFTER_NORMAL_012_MODONLY.zks` (`0be64b3f…a249`); loader profile `owner-0.33.0`; product alone;
  RTWP.
- **Kept:** all earlier acceptance (A1/A2/B, C1–C3c, G3 for its selected scope); RISK-03's limits; the REP-05/06
  blocks; G5 (fixture-blocked); the outstanding G1/G2 configurations.

## 1. Publication boundary and ownership map

**Publication (unchanged).** The Stirge stays withdrawn from every public summon route (CUT-01,
`SummonPlaytestWithdrawalPolicy`, `SummonVisibilityCatalog`). Its unit, touch weapon, combat traits, hold buff and
Remove Stirge ability stay registered and configured so older references resolve. The fixture invoked the product's
retained (unpublished) Stirge execution ability directly. It changed no publication or suppression. This is a
**constructed legacy-equivalent fixture**, not a recovered historical save.

**Owner map** (`ExpandedSummoningSpecialCombatComponents.cs`, `ExpandedSummoningSpecialBuilder.ConfigureStirgeAttachment`,
`StirgeAttachPolicy.cs`):

| Element | Owner | Behavior |
|---|---|---|
| Attach | `StirgeAttachComponent` on the combat-traits buff | A touch hit with the exact touch weapon (`KMG_Summoning_Natural_StirgeTouch`, primary hand) on a living target while not attached adds the hold buff and calls `StirgeHoldComponent.Attach` (with a native `RuleAttackWithWeapon.OnTrigger` fallback for the same weapon) |
| Hold buff | `KMG_Summoning_Special_Stirge_Hold` (`13c3b690…`) on the **Stirge** | `AddCondition` LoseDexterityToAC, CantMove and CantAct on the Stirge, plus `StirgeHoldComponent`. The prey is **not** a grapple target and gets no constraints |
| Target reference | `StirgeHoldComponent.m_Target` | `[JsonIgnore]`, session-only; cleared in `OnTurnOff` |
| Constitution drain | `StirgeHoldComponent.OnNewRound` | 1 per round, cumulative `m_CumulativeDamage` (`[JsonProperty]`), detach at 4; disease exposure once per victim per Stirge (`m_DiseaseCheckedVictims`, serialized) |
| Shared action | `KMG_Summoning_Special_Stirge_Remove` (`d5e6506d…`) on the **prey** | One fact however many Stirges attach. Touch, standard action; `RemoveStirgeTargetChecker` admits only a Stirge attached to the caster. `ContextActionRemoveStirge` picks the better of CMB and Mobility; success calls `Detach` for that Stirge |
| Action reconciliation | `StirgeHoldComponent.ReleaseRemoveAction` (from `OnTurnOff`) | Removes the prey's action only when no Stirge still names it |
| Native follow/tick cleanup | `FollowAttached` (from `StirgeVisualTouch.LateUpdate`) and `OnNewRound` | Detach when the owner or target is dead, destroyed or out of game, or the target jumped more than 8 m; `OnNewRound` also detaches on a missing summon marker or a null/dead target |
| Translocation | `StirgePreyTranslocationPatch` | Detaches Stirges attached to a translocated unit (not exercised here, and not a G5 scene transition) |
| Load/leave sweep | `SummonGrappleAreaSafeguard.SweepStirge` (from `OnAreaLoadingComplete` and `HandlePartyLeaveArea`) | Detaches every Stirge hold; removes any Remove Stirge fact no Stirge names |

**Historical evidence reused (attached to its original candidates):** the attack statistics, drain balance, the
four-point meal, the disease mechanics and one failed-then-successful removal are covered by the product's own runtime
assertions (`expanded-summoning-stirge-*` in `RuntimeTestRunner`), recorded with releases 0.0.141 and 0.0.146. They are
not repeated here.

## 2. Method and disclosed setup

- Stirges were summoned through the retained execution ability. Every attachment came from a real touch attack
  (`RuleAttackWithWeapon` with the exact touch weapon) reaching the product's attach path. Each attached at its first
  attack (captured). Nothing called `Attach`, `Detach` or a sweep to manufacture an outcome.
- **Disclosed:**
  - Stirge BAB 100 (touch hit);
  - prey HP raised;
  - appear buffs removed;
  - brains off, so nothing re-attacks during the quiet intervals;
  - units woken;
  - navmesh placement;
  - pauses for observation;
  - for the removal action only, the prey's Mobility 0 → 300, so the genuine check succeeds;
  - summon expiry by a shortened native `SummonedUnitBuff` deadline, then native `UpdateNextEvent`/`Tick`;
  - in the save launch, one **synthetic orphan** Remove Stirge fact added (native `AddFact`) to a disposable unit that
    no Stirge targets.
- **Controls:** an independent Stirge/prey pair with an unrelated Bull's Strength on its prey. A fresh control was used
  for each terminal, so no control ran out its four-point meal while it served as a witness. (`control1` later
  finished its meal, after its last check: captured.)
- **Instrumentation:** the read-only observer records `TryAttach`, `OnNewRound` (cumulative before/after), `OnTurnOff`
  (owner on/off), `Detach`, `ReleaseRemoveAction` (fact before/after), the Remove Stirge action, and `SweepStirge`
  (count).

## 3. Results

### 3.1 Two Stirges on one prey (`20261123T033626Z-owner-0.33.0-ver02-g4-stirge-lifecycle`, PASS, 138 s)

**Setup, asserted:**

- A and B alive, each naming P;
- each with its hold buff and its owned constraints (CantMove, CantAct, LoseDexterityToAC on the Stirge);
- P alive, free (no CantMove, CantAct or grapple part), with **exactly one** Remove Stirge fact;
- the native `AbilityData.CanTarget` and the product checker admit A and B from P;
- the checker refuses the control's Stirge from P, and refuses A from the control's prey;
- the control attached.

**First-owner termination (A killed with `GameHelper.KillUnit`).** Release after 0.025 s of game time.

- Asserted:
  - A names no target and has no hold buff (its constraints went with it);
  - B still names P, with its hold and constraints;
  - P keeps exactly one action, which still admits B and no longer admits A;
  - the control intact.
- Asserted, B continuing: in the next round B's meal went 0 → 1 and P's Constitution damage rose by exactly **1** (B's
  point), with no `OnNewRound` from A after its death. A contributes no attachment damage.
- Captured path: `Detach(A)` → `OnTurnOff` (owner on) → `ReleaseRemoveAction(P)`, which kept the action
  (`hasAction: true`, because B still names P).
- Source inference: the caller was `FollowAttached` from the view's `LateUpdate`. It is the only caller that detaches
  on a dead owner without a preceding round tick. Without the Stirge visual, `OnNewRound` would detach within a round.

**Last-owner termination (P's Remove Stirge against B).** P's own ability ran through the native command
(`UnitUseAbility.CreateCastCommand` → `UnitCommands.Run`) and ability execution. Release after 1.82 s of game time
(command start to detach).

- Asserted:
  - the action ran for prey P against Stirge B, and B named no target when the action returned;
  - B released: no target, no hold, no CantMove, CantAct or LoseDexterityToAC (B alive);
  - P has no Remove Stirge fact;
  - P's unrelated effect kept;
  - the control intact;
  - over **13.0 s** of game time after it: no Constitution damage, no action recreated, no reattachment;
  - P's already-inflicted Constitution damage (1) was **not healed**.
- Captured path: `RemoveAction.begin(P, B)` → `Detach(B)` → `OnTurnOff` → `ReleaseRemoveAction(P)` (action removed) →
  `RemoveAction.end`.
- **Diagnostic only:** a direct repeated `Detach(B)` afterwards returned `false` (idempotent). It qualifies nothing.

### 3.2 Active summon expiry (same session)

- **Precondition, asserted:** D alive and attached to R, with hold and constraints; R holding the action; a fresh
  control attached.
- **Terminal:** native expiry through the shortened deadline.
- **Release:** **6.01 s** of game time after the expiry tick.
- **Asserted:** D named no target and had no hold buff; R had no action; the control intact; then over 13.0 s: no
  Constitution damage, no action recreated.
- **Captured path:** at D's next `OnNewRound`, the hold's missing-marker guard ran `Detach(D)` → `OnTurnOff` →
  `ReleaseRemoveAction(R)` (removed). D was destroyed by the time the release condition was met.
- **Captured:** during the gap D still named R, but R took no drain (Constitution damage 0 → 0), because the guard
  detaches before draining. The expiry did not release at once; the designed one-round guard released it.

### 3.3 Prey death while attached (same session)

- **Precondition, asserted:** E alive and attached to S; a fresh control attached.
- **Terminal:** `GameHelper.KillUnit(S)`.
- **Release:** **0.013 s** of game time.
- **Asserted:** E released (no target, no hold, no constraints; E alive); the dead S has no action; over 13.0 s: no
  drain on S, no relationship or action recreated; the control intact.
- **Captured path:** `Detach(E)` → `OnTurnOff` → `ReleaseRemoveAction(S)` (removed). Source inference: the caller was
  `FollowAttached`.

### 3.4 Save under the existing session policy (`20261123T033015Z-owner-0.33.0-ver02-g4-stirge-attachment`, launch 2 PASS, 108 s)

- **Before the save, asserted:**
  - F alive and attached to T after one drain round (meal 1; T Constitution damage 1);
  - the synthetic orphan unit holds one Remove Stirge fact;
  - native admission allowed.
- **Save, asserted:** full completion (callback received, no commit, no loading in progress) and an unchanged paused
  clock.
- **Captured during the save:** `OnTurnOff` of F's hold with **owner off** cleared the target, and
  `ReleaseRemoveAction(T)` removed T's action, both **before serialization**. This is the legacy save-time detachment.
- **Running process after the save (captured):**
  - F no longer named a target, but kept its hold buff with CantMove, CantAct and LoseDexterityToAC;
  - T had no action;
  - the synthetic orphan still had its fact (no sweep runs in the running process).
- **Running process after resume, asserted:**
  - F's hold was removed at its next `OnNewRound` (null-target guard), **6.0 s** of game time after resume;
  - F has no constraints;
  - T has no action;
  - over 13.0 s: no drain, nothing recreated;
  - T's Constitution damage kept, and its unrelated effect kept.
- **Disposition:** the ≤ 1-round targetless hold on the Stirge after a save is existing legacy behavior. It is
  self-clearing and is not a stuck state, so it was not repaired.

**Written file** (`Manual_1_ver02g4.zks`, SHA-256
`ca43efbb9e98327e4ad516dee9149898ddf8ca55bed7e33ea780eda902a9f5eb`; manual read-only inspection):

- `KMG_G4_Stirge`: the hold buff `13c3b690…` serialized, with `m_CumulativeDamage: 1` in its component data and no
  serialized condition counters (the `AddCondition` components re-apply on activation). The combat traits' disease
  record (`m_DiseaseCheckedVictims`) names T. There is **no target reference**, by design.
- `KMG_G4_Prey`: **no Remove Stirge fact**; its Bull's Strength and the native Constitution-damage buff are present.
- `KMG_G4_SyntheticOrphan`: the synthetic Remove Stirge fact (`d5e6506d…`).

This file is **not** a complete persisted attachment: the target is deliberately omitted, and the ordinary saved prey
holds **no orphan action**. The only orphan action in the input is the labelled synthetic one.

### 3.5 Fresh-process load (same session, launch 3 PASS, 125 s)

- **Asserted:**
  - the three named participants found;
  - at load and after 13.0 s: F names no target and has no hold buff or constraints;
  - T has no action; the **synthetic orphan fact is gone**;
  - no drain, nothing recreated;
  - T's unrelated effect and Constitution damage kept (1 at load, the saved value).
- **Captured native load-hook path** (all with loading in progress): `Game.OnAreaLoaded` → `ResetLoadedGrapples` →
  `Detach(F)` (hold removed) → `ReleaseRemoveAction` over the units, removing the synthetic fact (`hadAction: true` →
  `false`) → `SweepStirge` returning 1.
- So the sweep's hold branch ran on the ordinary saved input, and its orphan-action branch ran **only** on the synthetic
  fact.
- **Captured:** the hold's in-memory counter read 0 at the load-time detach, against 1 in the file. Nothing depends on
  it, because the hold is removed at load.

### 3.6 Failed run (retained)

| Session | Harness | Launch | Cause |
|---|---|---|---|
| `20261123T033015Z` | `af6a2d27…55ca1` | 1 FAIL (97 s) | **Oracle defect.** The removal happened (`RemoveAction` → `Detach(B)` → action removed). The check compared a null string field with `JTokenType.Null`, but Newtonsoft stores a null string as a String-typed token (the C2 lesson). Fixed to `(string)token == null`; launch 1 was rerun alone (`033626Z`). Launches 2–3 PASS. |

## 4. Persistent consequences versus attachment residue

- **Constitution damage** already inflicted (P: 1; T: 1) stayed after each release and after the load. Cleanup did not
  heal it (asserted at removal, after quiet, and at reload).
- **Filth fever:** no victim contracted it in these runs (a 10% exposure per victim per Stirge). Its preservation
  through cleanup was therefore not exercised. The once-per-victim disease record on the Stirge is serialized rule
  state, not attachment residue.
- **Attachment residue** (target reference, hold buff, owned constraints, Remove Stirge fact) was absent after every
  terminal and at load.

## 5. Evidence classes, defects and limits

- **Asserted:** as listed in 3.1–3.5.
- **Captured:** callback paths, release timings, the save-time detachment, the targetless hold after a save, attack
  counts, the in-memory counter at load.
- **Manual saved-file inspection:** section 3.4.
- **Source inferences:** the `FollowAttached` caller; release by `OnNewRound` when no Stirge visual is present.
- **Historical:** the attack, drain, meal and disease mechanics (released runtime assertions).
- **Defects:** none demonstrated. No product change, build or broad suite.
- **Limits:**
  - turn-based mode is unexecuted;
  - filth fever preservation is unexercised;
  - translocation is unexercised;
  - the `HandlePartyLeaveArea` sweep remains with G5 (fixture-blocked);
  - release by `OnNewRound` with no Stirge visual is source-inferred;
  - removal by the CMB branch (as opposed to Mobility) is historical only.

## 6. Recommendation

**Accept G4 for its demonstrated development scope.**

- On the accepted product, the retained Stirge owner releases exactly the terminated Stirge's attachment.
- The shared Remove Stirge action is reconciled correctly with two owners: kept while another Stirge names the prey,
  removed after the last.
- Native expiry, prey death and the prey's own removal action clean up without later drain or recreation.
- A save detaches under the existing legacy policy, and the native load sweep removes the serialized hold and an
  orphan action.

**VER-02 stays open:** G5 (real party exit, fixture-blocked), the G1 variant owners, and the Xill, Giant Scorpion and
Giant Crab configurations.

**Restoration:** both sessions' transactions completed with game settings applied and owner files verified. No peer
session or Kingmaker process was present when checked before the runs.
