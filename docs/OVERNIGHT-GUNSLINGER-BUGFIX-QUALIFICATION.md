# Overnight Gunslinger Bug-Fix Qualification

Status: BASELINE ESTABLISHED; ISSUE QUALIFICATION PENDING

## Starting identity

- Branch: codex/gunslinger-overnight-bugfixes
- Baseline: d13268d3abe9ffe89c8195b213c1eee194328672
- Version: 0.0.87
- Domain/reflection tests: 1,150 passed, zero failed
- Repository validation: PASS
- Clean exact-reference Release: PASS
- Build-output validation: PASS
- Firearm SoundBank validation: PASS
- Deterministic package and strict validation: PASS

## Starting artifacts

- Local-runtime package:
  artifacts/local-runtime/0.0.87/KingmakerGunslinger-0.0.87-local-runtime.zip
- Package SHA-256:
  7572D7664C02BD07A1DD713971CFD9E9A46C02E32011E0F7A410F25F5D597CED
- DLL SHA-256:
  1E15AE66DEB0132D80F3774DA0A243AAE13388B91874898695926A966736F78F
- Firearm bundle SHA-256:
  CC9DA6B2FB43FD2932971E3CCE015610497E4C2DB657F62DBA675A31DE327B20
- Spear bundle SHA-256:
  6E9FE86E43072361EEC3357D9C73E17ADD71D22BAF257FB8C7ED6F52931CE777
- SoundBank SHA-256:
  0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18

No runtime launch or deployment was performed for the documentation-only
mission checkpoint. Issue-specific and final evidence will be appended without
rewriting this baseline.

## Issue 1 source qualification

- Parent SHA: `879ffe152a4ccfbfe42679055f7c392e5d0f1669`
- Version: 0.0.87
- Focused Acadamae tracker and source contracts: PASS
- Complete domain/reflection suite: 1,150 passed, zero failed
- Repository validation: PASS
- Clean exact-reference Release: PASS
- Build-output validation: PASS
- Firearm SoundBank validation: PASS
- Deterministic standalone/local-runtime packages: PASS
- Strict package validation: PASS
- Candidate package SHA-256:
  `538FE2A912B07990B12DC20C89D379ED8C3878C36FF41A5F46A5A7A3D8556B7B`
- Candidate DLL SHA-256:
  `E2CCBCE48C95C2183D37442F3D4E334F6EEF151192C6219CA15EFBE19ADB5116`

Guarded runtime is intentionally pending an immutable published commit. Source
and package gates do not establish live command ordering, fatigue persistence,
or visible diagnostics.

### First guarded attempt

- Run ID: `20260820T0358157167579Z-246801cb00f44e4a80f6e69e4dffa28c`
- Commit: `d691d508c43f3c048f28860389a6146186c11448`
- Result: `ERROR`; zero assertions evaluated
- Boundary: detached test command remained `Result=None` after one tick
- Disposition: rejected as evidence; fixture corrected to invoke exact native
  `UnitUseAbility.OnAction()` without manually constructing a cast rule

Corrected-fixture source gates pass 1,150/1,150 and the complete clean
Release/output/SoundBank/package/strict-package pipeline. Runtime retry remains
pending publication of the corrected immutable source.

### Second guarded attempt

- Run ID: `20260820T0404361106448Z-a262364c94774a8d9e444d73f7c32e10`
- Commit: `b2b8d6edcfa16d0fdb72ea7b1e8ecb2cfe50406c`
- Result: `ERROR`; zero assertions evaluated
- Boundary: installed composed `UnitUseAbility.OnAction_Patch3` threw a null
  dereference for the detached `ChargenUnit`
- Disposition: rejected as evidence; further detached action driving stopped

The third mode exercises exact native `RuleCastSpell` construction during the
command scope and deliberately triggers it after that scope. This is the
smallest live reproduction of the repaired delayed-callback boundary. Actual
loaded-area animation command execution remains human-gated rather than being
inferred from this narrower test.

### Third guarded attempt

- Run ID: `20260820T0410154111888Z-395ce15edb3b49a19d1f343d0604f369`
- Commit: `0708d9d7683bf8646a23efad06631cdddf2e473b`
- Result: `FAIL`; 11 passed, 2 failed, zero exception
- Mechanical result: delayed rules consumed exactly once; Standard/OFF,
  fatigue permanence, rest, cancellation, snapshot, Cord, and cleanup passed
- Failed boundary: diagnostic/test-control roll semantics only
- Installed IL: base total is implicit d20 + `StatValue`; final `RollResult`
  optionally adds `SuccessBonus`
- Repair: no completion-time `BaseRollResult` mutation; report d20, Fortitude
  modifier, conditional bonus, and final total separately

The repaired source again passes 1,150/1,150 and all clean Release/package
gates. A new immutable guarded run is required before Issue 1 disposition.

### Final guarded qualification

- Diagnostic run: `20260820T0417236469478Z-8e69f69546804934a84ea4d184d7a05d`
  on `1963a80eb3f49cffa6324df6d43450ef9e3fb05f`; 11/13 passed and
  native d20/modifier/total reporting was truthful.
- Rejected seed run: `20260820T0423312051777Z-775c65b925a240ec9590c0ab5d46913c`
  on `928fab624ab4cdd6e59a9b98d40d95783c708713`; Unity RNG did not
  control the installed save roll.
- Passing run: `20260820T0428503321600Z-d97a49371e1949c89f3de25aac1c6eff`
  on `f807eb1cc3dabf9dc66acaa2b773c029a72dc942`.
- Result: PASS, 14/14 assertions, zero exception.
- Gates: repository validation, 1,150/1,150 domain/reflection tests, clean
  exact-reference Release, output validation, SoundBank validation, package
  build, and strict package validation PASS.
- Runtime package SHA-256:
  `B6E6F409C5B78C16EDBD98A35E349DCD9F6412C08659A6D1300712DA838996CF`.
- Runtime DLL SHA-256:
  `788E98066FD614F78C3CE42B5ADC680BED45F4AF5C3B4A54AF956E257A1F6DDF`.
- Residual: loaded-area animation command execution, area transition, and
  visual log/Cord presentation require consolidated human observation.
