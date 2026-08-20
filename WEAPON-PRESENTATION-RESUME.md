# Weapon Presentation Calibration Resume

## Exact state

- Repository: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`.
- Starting master/origin SHA:
  `7af4375238b2492857a131eefdf909b38a000a05`.
- Branch: `codex/weapon-presentation-calibration`, based on that exact origin
  commit.
- Published baseline evidence commit:
  `baa426f491ad7a63a9a2dc52c7236e5f4c4b5afd`
  (`test(presentation): add guarded baseline visual evidence`).
- Published baseline documentation commit:
  `1d2a2c40fdaa911e7fe4d4b2f6c28852f8d8379f`
  (`docs(presentation): record baseline visual qualification`).
- Published native-donor diagnostic commit:
  `07c11236d2047af63fc6aeccfb51be99b06fe708`
  (`test(presentation): capture native donor frames`).
- Version remains `0.0.88`; do not bump until the complete cosmetic package is
  qualified.
- The published source checkpoint adds only guarded presentation evidence,
  preflight/allowlist integration, the repaired firearm readiness expectation,
  focused tests, and durable mission records. No asset transform has changed.

## Qualified unchanged baseline

- Repository validation: PASS.
- Complete Release domain suite: PASS, 1,163 tests / 0 failures.
- Release compilation: PASS.
- Clean Release package build and explicit package validation: PASS.
- `Build-Local.ps1`: PASS; no deployment performed.
- Runtime preflight: PASS, 112 checks.
- Native firearm donor/capability observer: PASS at
  `20260820T2220444335411Z-observe-native-firearm-rig-contracts`.
- Exact stored/held visual matrix: PASS at
  `20260820T2307109303617Z-weapon-presentation-evidence`:
  44/44 exact native models, 44 PNG/JSON pairs, 176 views, no blank or
  low-density sheets, exact request cleanup.
- Fixture runtime DLL SHA-256:
  `36B28263DA564418C1421F02847891BAD0C2C7A8B50F17A1CE9BB63E8C95CADA`.
- Fixture local-runtime package SHA-256:
  `28BC3B5298510D4B4E72A050194E24AADF858DF451A945CE13B0B39DC274C35E`.
- Clean release/local-runtime package SHA-256:
  `28BC3B5298510D4B4E72A050194E24AADF858DF451A945CE13B0B39DC274C35E`.

The baseline visibly reproduces pelvis-clipping/misdirected handguns,
low-diagonal torso/leg-crossing long guns, horizontal shoulder storage,
one-hand low spear idle/back mounting, and inconsistent Eastern blade planes.
See `WEAPON-PRESENTATION-JOURNAL.md` and matrix evidence key `V1`.

## Qualified native-donor diagnostic

The published diagnostic commit changes exactly two source/test files:

- `src/KingmakerGunslinger/RuntimeTesting/WeaponPresentationEvidenceScenario.cs`
- `tests/KingmakerGunslinger.DomainTests/WeaponPresentationMissionTests.cs`

They add six exact native controls and model-local bounds/locator reporting to
the existing guarded scenario. Runtime preflight passes 112 checks, repository
validation passes, the clean Release domain suite passes 1,163 tests / 0
failures, clean Release packaging and explicit package validation pass, and
`Build-Local.ps1` passes without deployment. The guarded Steam run passed at:

`20260820T2345261164438Z-weapon-presentation-evidence`

That run contains 56 exact stored/held pairs and 224 views. It proves native
crossbow forward `+Z` / up `+Y`; native Longspear forward `+Y`; native sword
forward `+Y` with blade plane `YZ` and blade normal `+X`; and distinct native
held/stored attachment transforms. It also passes a 6/6 held/stored local
geometry invariant using actual mesh-local bounds at a documented `0.00001`
tolerance. Runtime DLL SHA-256 is
`EA0774E877274437ED63CA94E55FB8FA50CE183C0DBBE566BD2E13EFE8A2617E`.
The local-runtime package SHA-256 is
`24DF7A916343948A8515FE699B14B367914D97FD8D062D1007B9D34212AB098A`.

## Next concrete actions

1. Complete semantic-frame validation before changing transforms: add
   secondary-axis markers/contracts (`WeaponUp`, `HeadUp`, `BladeNormal`),
   reject degenerate/reflected/reversed frames, prove renderer endpoints and
   support intervals, and retain identity equipment roots.
2. Continue directly through handgun, long-gun, branched-spear, Eastern held,
   and Eastern stored calibration. Expand the fixture to combat/attack and the
   representative character-size/sex matrix as each family is repaired.

## Supported hypotheses requiring donor confirmation

- Branched-spear physical head is source `+Z`, source head normal is `+Y`, and
  current held maps the physical head to `-Y` while the measured donor head is
  `+Y`. This polarity defect is now proven; renderer-bound validation remains.
- Eastern physical tip is source `+Z`, cutting edge is `-X`, blade normal is
  `+Y`; the measured donor target is forward `+Y`, blade normal `+X`, and edge
  side `-Z`. Identity presentation is therefore not a valid basis conversion.
- Firearm service Pistol, Revolver, and Rifle still rely on unexplained legacy
  transforms. Musket/Blunderbuss `3°`/`4°` yaw is not a full basis conversion.

## Safety/publication

Recheck status, stage only explicit mission paths, and commit only on the
feature branch after all required checks. Never merge, rebase published
commits, amend a pushed commit, force-push, create a PR, or create a release.
After every coherent commit and before any handoff run exactly:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```
