# Shield Other Qualification

Status: COMPLETE

This document will contain exact deterministic, build, package, runtime,
compatibility-profile, restoration, protected-save, and hash evidence for
release 0.0.77. No source/build result will be represented as in-game proof.

## Blueprint and publication checkpoint

- Source `823195c683d633fabbe4916119e847e65bb61013`.
- Complete deterministic suite: 978/978 PASS; exact-reference Release build and
  strict package validation PASS.
- Guarded save-free runtime PASS:
  `20260810T1243101482251Z-observe-shield-other-inventory`.
- Zero foreign Shield Other candidates. Exactly one level-2 membership in
  Cleric, Paladin, Inquisitor, Community, Protection, CotW Oracle, CotW
  Warpriest, and CotW Psychic.
- No save name, load, input, or save mutation was used.

## Standalone feature-module matrix checkpoint

- Exact source: `c90c0e43710e4801694b770de5e63121e00d2fd3`.
- All eight Gunslinger / Acadamae Graduate / Shield Other Boolean combinations
  passed guarded `observe-feature-module-settings` runs.
- Every combination retained exactly 254 registered identities. Each enabled
  module published only its own surfaces and each disabled module published none;
  Shield Other was exactly once in all five discovered base lists when enabled
  and absent from all five when disabled.
- Evidence spans `20260810T1409406936012Z-observe-feature-module-settings`
  through `20260810T1424139686495Z-observe-feature-module-settings`; the exact
  per-combination mapping is recorded in `SHIELD-OTHER-IMPLEMENTATION-JOURNAL.md`.
- Each isolated compatibility transaction reported exact restoration `True`.
- The profile-internal deterministic suite remained 981/981 PASS and strict
  package validation passed throughout.

## Typed damage checkpoint

- `20260810T1430187656299Z-disposable-shield-other` passed on exact source
  `ee0b4fa5f4d30b94dcb10c141eabac139fe6594b`.
- A native piercing packet with finalized damage 4 split `2/2`; a native fire
  packet with finalized damage 4 split `2/2`.
- All prior disposable assertions remained PASS, including temporary HP,
  reciprocal recursion prevention, lethal caster transfer, and request-local
  cleanup. Compatibility restoration verified `True`.

## Mitigation and immunity checkpoint

- `20260810T1436273826468Z-disposable-shield-other` passed on exact source
  `1c152496abb5af17f016d2e1a6c7d03532d64666`.
- Target DR and energy resistance were each evaluated once before splitting the
  finalized value. Caster DR 100 and fire immunity did not reduce the direct
  transferred share. Target fire immunity produced no subject or caster HP loss.
- All regression assertions and request-local cleanup passed; compatibility
  restoration verified `True`.

## Area and caster-death lifecycle checkpoint

- `20260810T1446127542431Z-disposable-shield-other` passed on exact source
  `79552bd9974907bf001ca645d0974cb05e72dfd8`.
- Unloading the caster from the active area removed the link before later damage;
  a caster reaching the lethal boundary through transferred damage also caused
  link removal on the next native round tick.
- All regression assertions passed and compatibility restoration verified `True`.

## Expanded disposable recovery checkpoint

- `20260810T1517266807175Z-disposable-shield-other` passed on exact source
  `0c6a3c7d6ebd9110c78a88bb00beaa625f1444f8` after invalid partial JSON save
  approximations were retired.
- The complete damage and lifecycle matrix remained green; exact compatibility
  restoration verified `True`. Genuine persistence remains assigned only to the
  guarded `KMG_AUTOMATION_WORKING` save path.

## Compatibility profile repetition checkpoint

- Exact source `76e5ce35a807388053ed4f911ea4f1c892611870` passed two consecutive
  `gunslinger-call-of-the-wild` runs and two consecutive
  `gunslinger-high-risk-combined` runs of the complete expanded disposable
  scenario.
- CotW evidence: `20260810T1520404782090Z-disposable-shield-other` and
  `20260810T1523377379140Z-disposable-shield-other`.
- Highest-risk evidence: `20260810T1526369993443Z-disposable-shield-other` and
  `20260810T1529104271088Z-disposable-shield-other`.
- All four isolated transactions restored the prior Mods directory exactly.
# Guarded save/load persistence

The authoritative persistence qualification is two fresh guarded launches. The
prepare scenario writes one fixed link only to `KMG_AUTOMATION_WORKING`. The
verify-cleanup scenario proves that Kingmaker reconstructed its originating
caster, subject, and caster-level context, exercises the odd damage split, then
removes the link and restores the party HP state before saving the working save
clean. The request-scoped sentinel accepts exactly one native `SaveRoutine` on
the same captured `SaveInfo` object and rejects every other save boundary.
Kingmaker 2.1.7b performs subordinate stashed-area serialization on a worker
thread; that exact method is accepted only after the authorized save routine has
started and only for the exact immutable working-save identity.
The subordinate method receives Kingmaker's serialization clone and omits
catalog-only presentation fields. Its guard therefore requires the exact
descriptor type, working name, folder leaf, and filename leaf plus explicit
baseline exclusion, and is reachable only after the top-level captured-reference
save is authorized. Catalog load and top-level save continue to require the
captured object reference.
Runtime evidence established that Kingmaker uses the next numbered manual-save
filename for the subordinate serialization clone before committing back to the
working slot. The accepted shape is therefore exactly
`Manual_<digits>_KMG_AUTOMATION_WORKING.zks`, with identical folder/file leaves
and exact working display name, and only within the authorized save lifecycle.

## Guarded save/load persistence result

- Prepare PASS: `20260810T1610288094375Z-working-save-shield-other-prepare`.
- Fresh verify/cleanup PASS:
  `20260810T1613120391758Z-working-save-shield-other-verify-cleanup`.
- Fresh load reconstructed party member 0 as caster, party member 1 as subject,
  caster level 5, and exactly one link. Finalized damage 3 split 1 subject / 2
  caster. It restored both HP totals, removed the link, and saved clean.
- Protected baseline retained timestamp `2026-07-28T19:26:46Z` and SHA-256
  `cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`.
- Final working-save SHA-256 after cleanup:
  `0349c94c48ad92e1b82d3df0b32209c5988711985ffa53d93f681ef30a467e64`.

## 0.0.77 release-source gates

- Complete deterministic suite: 981/981 PASS.
- Version-aware validation: PASS via `validate_shield77.py`.
- Clean exact-reference Release build and strict UMM package: PASS.
- Final package SHA-256:
  `b1b79a8c57b44e13fc2ed7343c2ad75bf32df575a7ede1a89dfca0bd76b6e646`.
- Final Release DLL SHA-256:
  `602a4f689b7fa41684dbc402bc4f223ce16e8e50691f0a1369e162cb5f3d8b06`.

## Final exact-commit runtime qualification

Release-source SHA `80d468ee22835d6c258041cd63f2cd288ef0401e`:

- All eight module combinations PASS from
  `20260810T1627445514271Z-observe-feature-module-settings` through
  `20260810T1643115666580Z-observe-feature-module-settings`; settings restored
  exactly to SHA-256
  `a08c80d5e877d4f6b5deea1247a9764bb48c08031b90da91a6e24d953e465ba6`.
- Standalone x2 PASS: `20260810T1645354769017Z-disposable-shield-other` and
  `20260810T1647523323239Z-disposable-shield-other`.
- CotW x2 PASS/restored: `compat-20260810T164943Z-046b85f47e64` and
  `compat-20260810T165221Z-e48789eb06ec`.
- Highest-risk combined x2 PASS/restored:
  `compat-20260810T165454Z-9ceec19dd586` and
  `compat-20260810T165726Z-dbba50e40dcd`.
- Every launch used the guarded Steam App 640820 path. No protected save was
  written, no Kingmaker process remained, and every profile reported
  `restorationVerified: true`.
