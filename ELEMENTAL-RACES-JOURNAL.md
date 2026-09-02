# Elemental Races journal

## 2026-09-01 - Mission start and authoritative baseline

- The requested adjacent directory
  `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslingerAndOtherTabletopRules`
  did not exist. The provided checkout at
  `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger` has origin
  `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`, so
  it is the requested repository under its older local folder name.
- Starting branch: `master`.
- Starting commit: `06c4d998f160df75ad3be7bfcf3de7e415c631d4`.
- Exact starting status: `## master...origin/master`, with no tracked or
  untracked changes.
- Ran `git fetch origin`. `origin/master` remained exactly
  `06c4d998f160df75ad3be7bfcf3de7e415c631d4`; there were no intervening
  commits to reconcile.
- Created the required branch `codex/elemental-races` without rewriting or
  discarding any history.
- Read `AGENTS.md`, every runtime/build instruction it references,
  `docs/BUILD-AND-RELEASE.md`, the feature-module implementation, blueprint
  registry and complete bootstrap transaction, accepted Gunslinger appearance
  catalog/application, and the outfit scenario/test architecture.
- Current release identity is version `0.0.113`, informational version
  `0.0.113-save-load-hotfix`; current feature settings schema is 9; current
  module count is 10; the baseline runtime boundary count is 22.
- Ran `.\scripts\test-domain.ps1 -Configuration Release` before source work.
  Repository validation passed and the complete dependency-free suite completed
  **1,373/1,373 PASS**.

## Local runtime fingerprints

- `Assembly-CSharp.dll`: 7,262,208 bytes; SHA-256
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`;
  assembly version `0.0.0.0`; last-write UTC
  `2025-04-06T00:59:19.4022320Z`.
- `UnityModManager.dll`: version `0.32.4.0`; SHA-256
  `1387468bc3af41c50fe51859a3bb7af4922891aa8f13a6187e7a348ceaabfd88`.
- `0Harmony12.dll`: version `1.2.0.1`; SHA-256
  `aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292`.
- Call of the Wild `1.14.4c-2.1`: DLL SHA-256
  `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915`.
- Races Unleashed `1.0.11`: DLL SHA-256
  `6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0`.
- Tweak or Treat `1.1.0`: DLL SHA-256
  `a518324e15632aba46d6c467b156a31e9afd282e9827dee3e79ad14673852b92`.
- Favored Class (`ZFavoredClass`) `1.3.1`: DLL SHA-256
  `dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c`.
- Visual Adjustments was not found by the initial relevant-directory filter;
  an exact installed `Info.json` identity audit remains pending.

## 2026-09-02 - Engine, interoperability, and visual reconnaissance

- Exact installed-mod scan confirmed Visual Adjustments is not installed.
  Its final compatibility observation is therefore `NOT-RUN`, not PASS.
- Inspected the installed Races Unleashed assembly read-only. Its UMM identity
  is `RacesUnleashed`; it publishes during `LoadDictionary` by appending each
  owned race to the current `CharacterRaces` array. Elemental publication must
  consequently preserve the live third-party prefix, identify entries by
  blueprint identity rather than display name, and remain idempotent.
- Inspected Ebons Content Mod read-only at public commit
  `adc05da1b8962b1a9ac6d7f902e22e7756fa5c65`. It proves that Human-compatible
  race cloning, request-owned visual proxies, and a Bull Rush delivery path are
  possible. No source, GUID, asset, BlueprintCore pattern, or dependency was
  copied. Its caster-level affinity behavior is explicitly rejected because
  this mission authorizes DC only.
- Local engine metadata proves `BlueprintRace.RaceId` is a closed enum with no
  elemental members. A development clone using donor `RaceId.Human` therefore
  tests the approved donor-identity strategy without inventing an enum value.
- Guarded `gunslinger-outfit-audit` transaction
  `20260902T0300398535825Z-gunslinger-outfit-audit` passed with 49 classes, 163
  wrappers, 361 raw candidates, 1,206 entities, 3,816 matrix rows, 4,860 links,
  and zero unresolved links. Accepted male/female Magus-derived IDs and colors
  remain unchanged.
- Added one manifest-reserved, request-only diagnostic identity:
  `KMG.ElementalRaces.Diagnostics.ProbeRace` /
  `57005fca40ab4775ae2fea5613214054`. It is never production-published and is
  removed from both blueprint indexes after each request.
- Added `observe-elemental-race-blueprints`, a save-free guarded scenario that
  clones Human, performs native `LevelUpController.SelectRace`, renders male
  and female dolls, verifies accepted Gunslinger outfit links, serializes a
  hidden `BlueprintRace` reference with native JSON settings, and requires
  exact array/index rollback.

## Development-probe strategy changes

1. `20260902T0342307483942Z-observe-elemental-race-blueprints` - **FAIL**.
   Same-frame doll readiness was too strict (`avatarEntities=0`,
   `renderers=0`). Registration, collision scan, hiding, cleanup, and save-free
   invariants passed. Strategy changed to bounded multi-frame settling.
2. `20260902T0352481058858Z-observe-elemental-race-blueprints` - **FAIL**.
   Both roots acquired three renderers, but unattached diagnostic views
   correctly retained zero `CharacterAvatar.EquipmentEntities`. Strategy
   changed to audit the actual root renderers/materials/shaders and keep avatar
   attachment as a separate production-persistence surface.
3. `20260902T0358322341784Z-observe-elemental-race-blueprints` - **FAIL**.
   Rendering and outfit checks passed, but direct `SetRace` did not apply the
   clone-only feature, and a combined custom DollData persistence envelope hit
   a native dictionary-converter ambiguity. Strategy changed to native
   character-generation selection plus isolated blueprint-reference
   serialization; existing accepted doll persistence remains authoritative.
4. `20260902T0409422132157Z-observe-elemental-race-blueprints` - **PASS**.
   Seven exact native races resolved; no same-race collision was present;
   hidden registration resolved in both indexes; male/female views each had
   three renderable renderers, three valid materials, and no null shader;
   native race selection retained the exact race and applied the clone-only
   fact at rank 1; hidden identity serialization round-tripped; cleanup restored
   the exact `CharacterRaces` contents/reference and both library counts; no
   save, input, selector publication, or persistent unit was used. Evidence
   SHA-256: `edfdc13e786fe800851573e4339176b1fdac2116ed43141fd658627ccda40e04`.

## Current decisions and next action

- Preserve the accepted Gunslinger class-clothing IDs and colors unchanged.
- Use four distinct project-owned `BlueprintRace` references and the safest
  donor `RaceId`; the Human donor is mechanically viable but final
  Aasimar/Tiefling outsider semantics remain to be qualified before selection.
- Keep the diagnostic race permanently request-gated. Its successful probe is
  evidence for construction and persistence architecture, not production-race
  completion.
- Complete domain suite after the probe revision:
  `.\scripts\test-domain.ps1 -Configuration Release` - **1,376/1,376 PASS**.
- Phase B checkpoint command:
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` - **PASS**.
  Repository validation, the complete 1,376-test suite, production compile,
  build-output validation, SoundBank validation, deterministic packaging, and
  strict standalone UMM validation all passed. Checkpoint package SHA-256 is
  `160b21230624d3ebc66f2a6c7f3da4e33b3abb0a2605bff250cded143ff6c8c9`;
  DLL SHA-256 is
  `a0887d6061a35b213f7e9ad8df6e65543de66c2fbd39250c13f27cfa3b209320`;
  DLL MVID is `c900ae62-326b-4ec8-a36a-b672122b4266`.
- Next: checkpoint Phase A/B, then implement schema-10 `elemental-races`
  feature-module integration and its 24-state runtime boundary catalog.

## 2026-09-02 - Phase C feature-module integration

- Added one module ID, `elemental-races`, one UMM checkbox, and bit 1024. The
  established ten modules retain default ON; Elemental Races defaults OFF.
- Advanced `FeatureModules.json` to schema 10. Schemas 0 through 9 preserve all
  explicit existing values, migrate an absent Elemental Races key to OFF, and
  preserve an explicit Elemental Races true or false value. Schema 11 is
  rejected as future input.
- Malformed recovery now reports the actual mixed defaults instead of claiming
  every module defaults ON. The malformed source bytes remain untouched and a
  diagnostic quarantine copy is retained.
- Extended active/pending snapshots, equality, hash code, `ToString`, ordered
  serialization, publication planning, UMM presentation, guarded request
  validation/writing, live settings observation, compatibility-profile
  settings transactions, and focused persistence fixtures.
- Ran `.\scripts\build.ps1 -Configuration Release -SkipDomainTests`.
  Repository validation and production compilation passed.
- Ran `.\scripts\test-domain.ps1 -Configuration Release`.
  **1,377/1,377 PASS**, including all 2,048 settings combinations and the new
  every-schema migration case.
- Executed `Get-KmgFeatureModuleCatalog` and
  `Get-KmgFeatureModuleConfigurations` directly. Observed 11 modules, 2,048
  exhaustive configurations, 24 unique boundary profiles, 1,024 exhaustive
  Elemental-ON states, and `elemental-races` as the final deterministic key.
- Ran `.\scripts\build.ps1 -Configuration Release -Clean -Package` on the
  exact Phase C tree. Repository validation, 1,377 tests, production compile,
  output/SoundBank checks, deterministic packaging, and strict UMM validation
  passed. Package SHA-256:
  `a3fa4c26704f59ce3bc8eed61325a4443a04b3d504a6f3d3518d3f26461b5d5a`;
  DLL SHA-256:
  `5875845dd31e1b4c6a5ea4f764df08d4e325df88b58069c933b174661e204eaf`;
  MVID: `1dbe88b3-acc6-45e0-b6c4-f981d9a135f4`.
- Ran focused guarded configuration
  `on-on-on-on-on-on-on-on-on-on-off` through
  `Invoke-FeatureModuleRuntimeMatrix.ps1`. Transaction
  `20260902T0440201720486Z-observe-feature-module-settings` passed every
  existing publication assertion and the new
  `feature-module-elemental-races-restart-snapshot` assertion with observed
  `disabled`. The controller restored the exact original settings bytes;
  original SHA-256:
  `9c95e56da5713b0c9d040a918a270c117a8006b9fb8124b068a6a613d925f11e`.
- The complete 24-launch boundary matrix remains pending until production
  identities and selector publication exist.
