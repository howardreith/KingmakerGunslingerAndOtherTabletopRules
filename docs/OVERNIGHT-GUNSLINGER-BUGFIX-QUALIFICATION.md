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

## Issue 2 source qualification

- Candidate: marker-first, verified shared-Grit transaction with marker removal
  and resource snapshot restoration on an inconsistent debit.
- Focused cases: ordinary positive Grit, exactly one Grit, repeated activation,
  armed duplicate, zero Grit, selected/unselected True Grit, and wrong owner.
- Full gates: repository validation PASS; 1,151/1,151 domain/reflection tests
  PASS; exact-reference Release PASS; output, SoundBank, package, and strict
  package validation PASS.
- Candidate package SHA-256:
  `13BAFA9437E887BDDAE512D63C997542605DA4B5A9C60D033DD5D50812D88B40`.
- Candidate DLL SHA-256:
  `206606B2996DDD16D7F2F5FBF3F5FF00B142307F6B722CD85257B43C904133FD`.
- Guarded scenario: `disposable-focused-aim`, pending immutable publication.

## Issue 2 qualification - Focused Aim

- Qualified code SHA: `24ffb78ac6821d4aec173213df1f046940be683b`.
- Focused domain test: `mysterious-stranger.focused-aim-transactions` PASS.
- Complete domain/reflection suite: 1,151/1,151 PASS.
- Repository validation, exact-reference clean Release build, output validation, package build, strict package validation, SoundBank validation, and `git diff --check`: PASS.
- Guarded scenario: `disposable-focused-aim`.
- Passing run ID: `20260820T0458550047640Z-b20727d24cc24d3297e0e9d23d385235`.
- Runtime assertions: 7/7 PASS; ordinary transactions `2 -> 1 -> 0`; selected True Grit retained positive Grit at `1`; pistol damage delta `+4`; crossbow damage delta `0`; zero-Grit, wrong-owner, and duplicate controls rejected; request-local cleanup PASS.
- Deployed runtime package SHA-256: `A0D1B53AD3086339167BBBD23BB4B58D596014D989E1754BB4A1DAA30B63DEA0`.
- Deployed DLL SHA-256: `08FCE67E6947CA22E57BF987D7AD01F3EB78B6BFE9C422D1C3560EB8855B4857`.
- Deployed firearm AssetBundle SHA-256: `CC9DA6B2FB43FD2932971E3CCE015610497E4C2DB657F62DBA675A31DE327B20`.
- Automated status: qualified. Final action-bar counter presentation and ordinary save/load remain human-gated.

## Issue 3 qualification - firearm penetration

- Qualified code SHA: `1c671acc3196a3f416bdcf4177b7426c0e14ea01`.
- Focused tests: `ac.production-boundaries`, `ac.penetration-presentation` PASS.
- Complete domain/reflection suite: 1,152/1,152 PASS.
- Repository validation, clean exact-reference Release, output validation, SoundBank validation, package build, strict package validation, `git diff --check`: PASS.
- Runtime scenario preflight: 109/109 PASS.
- Guarded scenario: `disposable-firearm-penetration`.
- Passing run ID: `20260820T0513443721972Z-cceff2c263254181ad15fd7af638ed3f`.
- Runtime assertions: 5/5 PASS. All 15 Pistol/Musket/direct-Blunderbuss/Advanced-Rifle/Advanced-Revolver just-inside/exact/just-outside native `RuleCalculateAC` boundaries selected Touch/Touch/Normal; a +10 ft. Musket boundary selected Touch at 50 ft.; 16 native feedback events and cleanup passed.
- Deployed runtime package SHA-256: `A05A0389C3C5D4ABC19891EA1B0F57D40AD9A865DA8DF0C3D2F6A6C62F81BF27`.
- Deployed DLL SHA-256: `44208BB5D6351877C71A73E7A3979B732BD8654E89EABDDB46B493F882197C04`.
- Deployed firearm AssetBundle SHA-256: `CC9DA6B2FB43FD2932971E3CCE015610497E4C2DB657F62DBA675A31DE327B20`.
- Automated status: qualified. Real-scale tooltip/battle-log readability and ordinary player-issued attacks remain human-gated. No safe pre-attack hover seam was established.

## Issue 4 qualification - Acadamae prerequisite presentation

- Qualified code SHA: `78bc46d21b71dbfb35d430d00755228348afb751`.
- Focused test: `acadamae.prerequisite-presentation` PASS.
- Complete domain/reflection suite: 1,153/1,153 PASS.
- Repository validation, clean exact-reference Release, output validation, SoundBank validation, package build, strict package validation, and `git diff --check`: PASS.
- Runtime scenario preflight: 109/109 PASS after one unchanged cleanup retry of the known post-build staging-state transient.
- Guarded scenario: `disposable-acadamae-graduate` run `20260820T0524095518410Z-ea7adae339fc4fa4a98bfe7bd52b4222`, 15/15 PASS.
- Live presentation assertion: exactly one `PrerequisiteAcadamaeGraduate` component, native eligibility text retained, feat description contains no manual prerequisite block.
- Deployed runtime package SHA-256: `60C0B8AFA7F3E7E9EB98710D6300C9B7D3D4F4AB3BE3C761CED8D5FA5FFCCB34`.
- Deployed DLL SHA-256: `0ECE4157AA0512A5887B7FD6BB5B82BCE963758EE796873FE2AC20113E482E14`.
- Automated status: qualified. Real feat-selection layout remains human-gated because blueprint/localization assertions cannot establish final UI composition at display scale.
## Issue 5 qualification - Oleg maintenance stock

- Qualified code SHA: `1586c5e7abd9c8d1b18bac483df88e86700677b0`.
- Focused test: `vendors.oleg-maintenance-stock` PASS.
- Complete domain/reflection suite: 1,154/1,154 PASS.
- Repository validation, exact-reference clean Release, output validation, SoundBank validation, package build, strict package validation, runtime preflight 109/109, and `git diff --check`: PASS.
- Guarded scenario: `observe-vendor-table-contracts` run `20260820T0535269245782Z-7edf2d2c158a4085893931e91b14db1d`, 20/20 PASS.
- Live assertions: exact table `C11_OlegVendorTable:f720440559fc00949900bfa1575196ac`; Repair Kit rows/count `1/5`; Overhaul Kit rows/count `1/2`; exact owners `OTP_Oleg:5db389e0409ef534d81358555e6ab99d*1` and `OTP_Oleg_FirstVisit:67db4b8bacc69e643880f0a4ed6dff6f*1`.
- Deployed runtime package SHA-256: `00D8405393B74827915C09B67834B9B2D06BC4FE06E5123808AEC64F59868F69`.
- Deployed DLL SHA-256: `8E7FFC30C596F94BA2EF5D06B6C99EFE6A3CB29AEE25E09C29F2624D6C16A003`.
- Automated status: qualified for exact static publication. Actual merchant materialization in a disposable/new campaign is human-gated; no refresh claim is made for already-materialized old-save stock.
