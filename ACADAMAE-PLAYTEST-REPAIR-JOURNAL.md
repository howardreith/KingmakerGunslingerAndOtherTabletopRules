# Acadamae Playtest Repair Journal

## 2026-08-09 — mission intake

- Branch/HEAD: `codex/feature-modules-acadamae-graduate` / `c80615e917d1994daad679e8a78af11ae2c7e115`; local equals remote; clean worktree; base/merge-base `7a99ce5ac6d6976212310f997bd39ddfe4a57935`; version 0.0.75.
- Human findings accepted as authoritative: Cord donor icon is not acceptable; Acadamae acceleration must be a native default-OFF per-character toggle; failed-cast fatigue currently inherits the summon `MechanicsContext` and disappears with it instead of persisting to native removal/rest.
- Accepted module architecture, prerequisites, Cord mechanics/nonlethal-equivalent fallback, vendor placement, and all existing identities are frozen.
- Selected target 0.0.76 / `0.0.76-acadamae-mode-fatigue-icon-repair`, subject to exact pin inventory near release.
- Commands: branch/status/local-remote/merge-base verification; exact version metadata inspection; full AGENTS and previous implementation/qualification/mission intake.
- Tests: not yet run; this checkpoint contains no substantive implementation.
- Current uncertainty: exact AddFacts restoration flags, activatable lifecycle fields, independent native fatigue creation contract, and rest removal path require installed-contract proof.
- Next concrete action: inspect all required source plus exact installed activation, buff, rest, and optional-patch contracts.

## 2026-08-09 - source-qualified 0.0.76 repair

- Branch/HEAD before commit: `codex/feature-modules-acadamae-graduate` / `c80615e917d1994daad679e8a78af11ae2c7e115`; active version 0.0.76 / `0.0.76-acadamae-mode-fatigue-icon-repair`.
- Implemented append-only mode identities `b5fc52ec666640318f8921d5fa60ec39` and `a780ab99b76849ed825729808e2bbf29`, a default-OFF persistent native activatable, restoring feat `AddFacts`, policy/runtime marker gating, and command snapshot coverage. Existing Acadamae/Cord GUIDs are unchanged; registered count is 252 and ledger count 253.
- Exact installed IL proves `BuffCollection.AddBuff(BlueprintBuff, UnitEntityData, TimeSpan?, AbilityParams)` creates a new `MechanicsContext(caster, owner, blueprint, null, null)` before the existing `RuleApplyBuff` Cord boundary. `RestController.ApplyRest` removes buffs whose blueprint has `RemoveOnRest`. Production Acadamae fatigue now uses this caster overload with null duration and never passes `RuleCastSpell.Context`.
- Generated original Cord art through the built-in image generation workflow, then deterministically chroma-removed/despilled/resized it. Source SHA-256 `d7e5dfa7228419df65e3bfa88aafa7b94caa1e5cfadfb1a159686805042655c8`; production SHA-256 `cf3f040eb22691b1e526eb32cc31d1151eafef7113cb0ebe55d0c2637d5d9928`. No donor or third-party pixels were used.
- Commands/tests: `python tools/validate_repository.py` PASS; `scripts/Build-Local.ps1` PASS after one rejected 43-file package expectation was corrected to 44; complete deterministic suite PASS 970/970; clean exact-reference Release, asset/SoundBank audit, deterministic packaging, and strict validation PASS.
- Qualified package SHA-256 `d330504831fb24ffb76386b2cfddd4affc5ec243ef76ae6c6081e23db4f01249`; DLL SHA-256 `2cec07fefff2665dd304a88ace98e267b4e393f7697adf1dee2c376a27864713`.
- Rejected theory: carrying the spell `MechanicsContext` with an arbitrary duration. Chosen path is the exact native independent caster overload and native rest contract.
- Current uncertainty: real runtime must prove grant/toggle behavior, command timing, context expiration survival, rest cleanup, Cord interception, and attached-view no-FX lifecycle.
- Next concrete action: commit/publish this coherent source-qualified repair and execute the standalone guarded Acadamae scenario.

## 2026-08-09 - first guarded Acadamae repair run

- Branch/HEAD: `codex/feature-modules-acadamae-graduate` / `e5319f63ab32c6ef13efa095e8ab06dbc78660bf`; version 0.0.76; commit published through the approved helper.
- Guarded run `20260810T0008229348262Z-disposable-acadamae-graduate` / runtime ID `20260810T0008229686745Z-e325f6296bfe4732a4460db145df8e36`: FAIL 12/13 behavioral assertions.
- PASS: native mode grant/default OFF, mode-OFF native Full-Round and zero save, mode-ON Standard action, successful and failed saves, cancellation, command snapshot semantics, fatigue surviving forced spell-context cleanup, native rest removal, attached-view no-FX lifecycle, and one-roll Cord interception.
- FAIL: the composite structural `fatigueIndependent` observation. The decisive lifecycle observations independently passed, so the next strategy is narrower instrumentation of each duration/context/rest predicate rather than changing mechanics without knowing which diagnostic assumption was false.
- State: guarded runner exited Kingmaker and retained structured evidence; deployment backup `C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260810T0008211856274Z` exists.
- Next concrete action: publish predicate-level fatigue-context instrumentation and rerun the exact standalone scenario.

## 2026-08-09 - fatigue diagnostic refinement

- Branch/HEAD: `a7915f6c31f787bf6e9d3c4ec53957b1ae8792e9`; run `20260810T0012262447308Z-disposable-acadamae-graduate` / runtime ID `20260810T0012262738740Z-ed341c7e113d46c18cb12eb530bbe0e3`: FAIL 12/13.
- Predicate evidence: canonical fatigue context exists, has no parent, is reference-distinct from the spell context, and uses a `RemoveOnRest` blueprint; it survived forced spell-context collection and was removed by actual native rest. Only the private `Buff.m_EndTime == null` assumption was false: Kingmaker normalizes internal end-time storage even though production passes a null duration.
- Decision: retain the exact no-duration production call and deterministic source guard; define runtime independence by the root/distinct context plus observed context-cleanup survival and native rest behavior. Record the normalized private value diagnostically without treating its storage representation as the public duration contract.
- Next concrete action: rebuild, publish, and rerun the exact scenario with the corrected engine-aware assertion.

## 2026-08-09 - native permanent flag and isolated fixtures

- Run `20260810T0016185839512Z-disposable-acadamae-graduate` / runtime ID `20260810T0016185995756Z-5bbdba6de6654a928bc24bc88c0728ad` proved the corrected independent-context assertion and reported private end-time storage `00:00:18`; its cancellation and Cord assertions were contaminated by a pre-existing fatigue fact in the shared multi-case unit.
- Exact runtime `Buff.IsPermanent` is now the authoritative no-duration observation. The fatigue assertion requires permanent, root/distinct context, `RemoveOnRest`, actual spell-context-cleanup survival, and actual rest removal. The private end-time value remains diagnostic only.
- Cancellation and Cord subfixtures now remove any preceding canonical fatigue first; the preceding snapshot assertion independently requires that it created none, so isolation does not conceal a snapshot failure. Cancellation correlation no longer depends on synthetic post-cancel `RuleCastSpell.Success`, only on zero new completion/save/fatigue.
- Full repository/build/package gate PASS: 970/970; strict package SHA-256 `f477354fcac8c741c62496a62d6d3b307e66ec3676c8b1ffeb06f2ebfce55744`; DLL `ec5a6088f55147de7716cb54aaea71a0ef4d008016fa94a91ab5f1220dda0738`.
- Next concrete action: commit/publish the strengthened runtime fixture and rerun standalone Acadamae qualification.

## 2026-08-09 - exact permanent-fatigue repair

- Run `20260810T0020189878001Z-disposable-acadamae-graduate` / runtime ID `20260810T0020190170558Z-4d6f52d2693c424e84b7b8483747e73f` isolated the real remaining defect: root/distinct fatigue survived immediate context collection and native rest removed it, but `Buff.IsPermanent` was false and the nullable end time held current game time `00:00:18`.
- Read-only installed IL inspection proved public `Buff.MakePermanent()` clears `m_EndTime` and asks the owning `BuffCollection` to update its next event. This is the exact native indefinite transition; no arbitrary duration, clone, reflection, or manual rest handler is used.
- Production now applies canonical Fatigued through the independent caster overload, then calls `MakePermanent()` only when the Cord has not intercepted and suppressed the buff. Deterministic guards require both operations and reject the old spell-context route.
- Next concrete action: run the complete build/package gate, commit/publish, and rerun the real standalone lifecycle.

## 2026-08-10 - runtime and compatibility qualification checkpoint

- Source commit `097b2fcc28b28f1f62051bdf0a700c7b1ec02dec`; version 0.0.76.
- Standalone Acadamae PASS 13/13 at `20260810T0025495339458Z-disposable-acadamae-graduate` and `20260810T0057193245457Z-disposable-acadamae-graduate`; exact permanent/root/distinct fatigue, context-cleanup survival, real rest removal, native default-OFF toggle, command snapshots, view lifecycle, and integrated Cord path passed. Cord scenario `20260810T0028061943603Z-disposable-cord-of-stubborn-resolve` PASS 8/8 including distinct project sprite.
- Four module configurations PASS with registered=252: runtime IDs `20260810T0030378293295Z-92d7638006ad407591e69653ac1de6ed`, `20260810T0032442695172Z-b3aba843a05c4110a1ee80124c931edf`, `20260810T0034494700421Z-e5234d444a854e3d9382d24a97f87755`, and `20260810T0036544950657Z-453c089bf845435facf86f18be3bbcb0`. Settings restored to SHA-256 `8aa8233b19e69af001d28dc9db51748baf3abb9ffff37ce96754c4addfac7470`.
- Exact restored compatibility transactions PASS: CotW `compat-20260810T003846Z-8ef116748a84`; Arms & Armor `compat-20260810T004325Z-48b05aca6317`; Toggle `compat-20260810T004510Z-57bdf436e6d3`; qualified combined `compat-20260810T004700Z-01cf993fd775`; consecutive high-risk `compat-20260810T004845Z-b479c645228a` and `compat-20260810T005115Z-8d43f465f597`.
- Two guarded `KMG_AUTOMATION_WORKING` smokes PASS 11/11: `20260810T0059425523070Z-8d4ce89f4e124fd8a5528427ff6b6075` and `20260810T0102171809010Z-518fa9cbfd2d4b49ab8c4b4903ec40ae`. No protected baseline save was selected or modified.
- One exact-byte standalone run `20260810T0054114452133Z-8e432ad927b048769b772723b9616986` observed a forced-save fixture variance only in the Cord subcase (no fatigue attempt); permanent-fatigue mechanics passed and the same bytes passed in the immediately following fresh run. Evidence is retained, not rewritten.
- Current uncertainty: final documentation commit changes the release source SHA and package bytes; repeat exact-final-commit gates before deployment.
- Next concrete action: validate/commit/publish docs, build exact final package, repeat primary and maximum-risk gates, deploy, verify installed state.

## 2026-08-10 - forced-save fixture composition repair

- Final-documentation source `3442d8ac17c4ed62f575ed4ef7f4642010e84f44` passed two consecutive fresh standalone integrated runs. Highest-risk run `compat-20260810T011257Z-34483c1fa1d4` passed and restored; the next run `compat-20260810T011528Z-37f8e1162d08` restored but its final forced-failure subcase unexpectedly passed the save, while every mode/action/Cord and preceding lifecycle assertion remained correct.
- Root cause: the guarded fixture forced the next global `RuleRollD20`; foreign handlers can consume that thread-local roll during `RuleSavingThrow.OnTrigger`. Exact installed `RuleSavingThrow` exposes public `BaseRollResult` get/set.
- Repair: retain pre-roll forcing, and additionally apply the same request-local roll in an exact `RuleSavingThrow.OnTrigger` postfix ordered after Call of the Wild. The control is thread-static, active only around Acadamae's guarded native saving throw, and is cleared in `finally`; production play is inert.
- Next concrete action: full build/package, commit/publish the deterministic fixture repair, then rerun final consecutive standalone and high-risk gates before deployment.

## 2026-08-10 - exact saving-event correlation

- First run on `26ea15a9d3febb4ec824d9c6946d062639f59183`, `20260810T0120099779554Z-6b91b1119f9b45169c6afd95d84538bb`, showed a nested/foreign saving event could consume the new post-roll token: failure/Cord/permanent lifetime passed, while the forced-success and snapshot-success assertions failed.
- Repair: `Begin` now binds the exact `RuleSavingThrow` instance constructed by Acadamae, and the postfix consumes only for reference equality with that instance. Nested or foreign saving events cannot steal the request-local result.
- Next concrete action: rebuild, publish, and repeat final-release gates.

## 2026-08-10 - deterministic natural-roll boundary

- Run `20260810T0124155352746Z-799b32734c694579827369109880642d` proved exact-event correlation for both forced failures and all permanent-fatigue/Cord mechanics, but the fixture's requested success roll 10 did not guarantee success against the installed computed save value.
- Repair: forced-success cases now use the native automatic-success boundary 20; forced failures continue to use native automatic-failure boundary 1. This tests native saving behavior without assuming a derived modifier.
- Next concrete action: rebuild/publish and run final release gates.

## 2026-08-10 - explicit guarded success result

- Run `20260810T0128274766671Z-94a730025d8346dc86fffb8548040548` showed Kingmaker's installed `RuleSavingThrow.IsPassed` does not treat D20=20 as an unconditional success in this synthetic fixture; exact failure and all production mechanics remained PASS.
- The exact request-local postfix now also sets public native `AutoPass=true` for the guarded success case only. Failure cases retain `AutoPass=false` and D20=1. This affects no production save because the thread-static target exists only inside guarded runtime requests.
- Next concrete action: build, publish, obtain final PASS, then deploy.
