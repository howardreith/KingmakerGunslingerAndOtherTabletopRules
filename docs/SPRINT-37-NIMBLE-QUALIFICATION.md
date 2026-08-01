# Sprint 37 Nimble qualification

Nimble is runtime-qualified on source commit
`b82768be890a63041c705bf547d81af93f89832c`.

## Implemented contract

- Five cumulative class facts grant +1 Dodge AC at levels 2, 6, 10, 14,
  and 18, for a maximum of +5.
- Each fact applies only while wearing light armor or no armor and refreshes
  on armor-slot and active-equipment-set changes.
- Native Dodge modifier semantics exclude the bonus whenever Dexterity AC is
  lost, including flat-footed AC.

## Deterministic qualification

- Repository validation passed through Sprint 37.
- The complete clean Release domain suite passed 737/737.
- Runtime-request and preflight suites passed 18 and 40 checks.
- Exact-reference Release build and strict standalone package validation passed.
- Package SHA-256: `e8244e972dfcd257f163246d1179a467e5a17ed6ab9caaa79c52dfeb18e7807f`.
- DLL SHA-256: `dafbb6bd85f01df1497815d21e3ed97c81ee95cce2797c63b7252798a026272a`.

## Guarded runtime evidence

- Mod load PASS: `20260801T2144324918751Z-mod-load-smoke`.
- Fresh-launch Nimble PASS:
  `20260801T2145560578604Z-disposable-gunslinger-nimble`.
- Independent fresh-launch Nimble PASS:
  `20260801T2147123020977Z-disposable-gunslinger-nimble`.

Both feature runs observed exactly
`base=10;noArmor=15;baseFlat=10;nimbleFlat=10;lightWith=23;lightWithout=18;mediumWith=20;mediumWithout=20`.
Both removed the armor and feature facts, disposed the detached unit, and
preserved party/global-unit reference snapshots. No save was selected, loaded,
or written.

## Disposition

Nimble is `RUNTIME-QUALIFIED`. Continue to Gunslinger Initiative; Sprint 37 is
a checkpoint, not a stopping condition.
