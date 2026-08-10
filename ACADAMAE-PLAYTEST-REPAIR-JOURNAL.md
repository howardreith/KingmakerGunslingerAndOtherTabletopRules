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
