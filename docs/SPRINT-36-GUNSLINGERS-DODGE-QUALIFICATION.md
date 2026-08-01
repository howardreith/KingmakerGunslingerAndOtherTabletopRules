# Sprint 36 Gunslinger's Dodge qualification

The drop-prone branch of Gunslinger's Dodge is runtime-qualified on source
commit `f79b4a2bf70e850d416c2943c7120a5a63e88e30`.

## Implemented contract

- A level-one deed feature grants a personal extraordinary free action that
  arms a hidden native persisted fact.
- The next incoming native ranged weapon attack consumes that fact.
- In light or medium armor and at light load, one grit is spent atomically,
  native prone is applied, and the triggering attack receives +4 AC.
- Duplicate callbacks cannot spend again or stack the AC bonus.
- Insufficient grit consumes the reaction marker but leaves grit, prone state,
  and AC unchanged.
- The tabletop 5-foot movement / +2 AC alternative is not implemented. It
  remains pending a safe deterministic destination-selection adaptation;
  Kingmaker exposes no Immediate action type.

## Deterministic qualification

- Repository validation passed through Sprint 36.
- The complete clean Release domain suite passed 727/727.
- Runtime-request, preflight, and dispatch suites passed 18, 40, and 13 checks.
- Exact-reference Release build and strict standalone package validation passed.
- Package SHA-256: `40d738a160929a4c611aaa0263a53fe0d48ccca6fab2ecc93d8fc400b7dd9b4a`.
- DLL SHA-256: `798e1fe7f96cc083de8493e164e5e640ccad2592481ea97251a4fe6cd5815677`.

## Guarded runtime evidence

- Mod load PASS: `20260801T2059139120474Z-mod-load-smoke`.
- Fresh-launch Dodge PASS:
  `20260801T2102104686115Z-disposable-gunslinger-dodge`.
- Independent fresh-launch Dodge PASS:
  `20260801T2103264904832Z-disposable-gunslinger-dodge`.

Both feature runs observed exactly
`initial=2;armedBefore=True;afterApplied=1;armedAfter=False;proneAfter=True;acAfter=24;acDuplicate=24;afterRejected=0;rejectedProne=False;rejectedConsumed=True;rejectedAc=20;applied=1;rejected=1;duplicates=1;faults=0`.
Both removed the native light armor, disposed detached entities, and preserved
party and global-unit reference snapshots. No save was selected, loaded, or
written.

## Disposition

The drop-prone branch is `RUNTIME-QUALIFIED`; the overall adapted deed remains
runtime-partial until the movement alternative has a safe adaptation. Continue
Sprint 36 with Quick Clear.
