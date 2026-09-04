# Elemental Races expansion journal

## 2026-09-03 - Mission start and authoritative baseline

- Repository checkout:
  `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`; its `origin` is
  `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
- Read the repository `AGENTS.md`, all current Elemental Races mission, state,
  journal, blockers, acceptance, deviation, implementation, release-note,
  architecture, blueprint-manifest, installation/compatibility, optional-mod
  compatibility, build/release, guarded runtime, save-observation, and testing
  records before changing source.
- Starting branch and SHA: clean `master` at
  `6874dc15a27ded132456dbdd480f47c794543a05` (`Merge Elemental Races
  0.0.114`).
- Ran `git fetch origin master`. Local `master` and `origin/master` remained
  exactly `6874dc15a27ded132456dbdd480f47c794543a05`; there were no intervening
  master commits.
- Created `codex/elemental-races-expansion` from the authoritative master.
  `origin/master` is an ancestor, left/right divergence is `0/0`, and the
  worktree contains no unrelated changes.
- Inherited baseline: 1,706 manifest entries, 1,704 active, two reserved;
  Elemental Races owns 69 entries, 68 active plus one guarded diagnostic.
  Version is 0.0.114 / `0.0.114-elemental-races`; feature schema remains 10
  with eleven modules and a 24-state runtime boundary.
- No milestone PASS is inferred from inherited 0.0.114 evidence. Foundation,
  0.0.115, 0.0.116, and 0.0.117 gates begin pending on this branch.

## 2026-09-03 - Clean 0.0.114 baseline qualification

- `./scripts/validate-repository.ps1`: PASS. Version-aware validation selected
  the 0.0.114 Elemental Races validator; manifest validation reported 1,184
  foundation, 1,704 active, two reserved, and 1,706 total identities.
- `./scripts/test-domain.ps1 -Configuration Release`: PASS, 1,390/1,390 tests
  with zero failures.
- `./scripts/build.ps1 -Configuration Release -Clean -Package`: PASS. The
  command repeated repository validation and all 1,390 tests, built the
  Release DLL, validated build output and the production firearm/SoundBank
  manifest, and created the standalone 0.0.114 package.
- `./scripts/validate-package.ps1 -PackagePath
  ./artifacts/packages/KingmakerGunslinger-0.0.114-elemental-races.zip`: PASS,
  including strict standalone UMM validation.
- Baseline package: 22,977,592 bytes; 135 entries; SHA-256
  `b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694`.
- Baseline DLL: 5,411,328 bytes; SHA-256
  `09af96b95e2abfa39e45f30c8ccb4cb1e8772981dd3be17846f07cbbd2dd8262`;
  MVID `dcd73856-39d4-40ce-9b05-77bf249103d7`.
- The generated package and checksum remain ignored build artifacts and will
  not be committed.

## 2026-09-03 - Foundation affinity and exact SLA hardening

- Local Kingmaker 2.1.7b metadata established that
  `RuleCalculateAbilityParams` exposes only `AddBonusDC(int)`; it has no
  modifier-descriptor overload. `BlueprintAbility.IsSpell` also includes
  `SpellLike`, while item ability construction bypasses ordinary spellbook
  context. The production predicate therefore fails closed unless the event
  has the same non-null spellbook as its `AbilityData`, and an effective
  variant/parent-chain blueprint has exact `AbilityType.Spell`.
- Added a pure affinity policy and four behavior tests. The pre-fix
  descriptor-only shape failed two new cases; the corrected policy accepts a
  matching ordinary spell or parent/variant chain once and rejects racial
  SLAs, item uses, supernatural abilities, and other nonspells.
- Added the dedicated save-free
  `disposable-elemental-spell-affinity` scenario; the central runtime runner
  contains dispatch only. Guarded Steam run
  `20260903T2023503562442Z-72c23cf0af684694b9717b27a2ee19d1` passed native
  matching/nonmatching spells, variant and parent overlap, project Ifrit
  Burning Hands, provisional Stormsoul Shocking Grasp, item, and
  supernatural cases. Feature evidence SHA-256:
  `eb6ef1058039572b98f9ca1d97fbbbe704e13519fb3c3da0dddb930c4eefc2c3`.
- Added a pure racial-SLA parameter policy. Caster level is current total
  character level; spell level is the configured racial SLA level; DC is
  exactly `10 + spell level + current ability modifier`.
- Strengthened live mechanics and command scenarios for exact current
  Charisma, temporary bonuses and penalties, multiclass caster level,
  cancellation, one committed debit, zero-use blocking, one-use rest
  restoration, and no level-up refill.

## 2026-09-03 - Foundation movement and Hydraulic Push layering

- Expanded the native-identity scenario with actual Barbarian Fast Movement,
  Haste, Slow, and `UnitCondition.DifficultTerrain`. A viewless chargen fixture
  cannot safely invoke `CurrentSpeedMps`; the scenario instead proves real
  condition installation/removal and audits the installed native
  `CalculateSpeedModifier` path. Haste changes the raw speed statistic while
  retaining its native two-times cap; Slow and difficult terrain retain raw
  speed and use their native one-half calculation path.
- Two bounded diagnostic native-identity runs failed before the fixture was
  corrected: one exposed the expected view dependency; one captured the
  exact Haste/raw-speed model. Final guarded run
  `20260903T2102263692728Z-aa6860361e204cc3a3d730686629f6fc` passed 17/17
  assertions. Feature evidence SHA-256:
  `76c462f30fa3d98d6e774a724b4704b4bff740db87e93b01ae859f108926612b`.
- Added a live temporary-Wisdom Hydraulic Push case. Guarded run
  `20260903T2104452897170Z-1fd16412780a4368a3eceeb0b0a0d119` passed 13/13:
  total level, all best-mental choices, negative/tie cases, a Fighter/Wizard
  multiclass, temporary-stat reselection, ordinary failure, maneuver
  immunity, native force movement, no save/attack/AOO, cancellation, debit,
  zero-use gating, and rest. Feature evidence SHA-256:
  `d4aaaa03e50f087bc9c18969837ce8121a8d7b490672f9c32f6473f75f159827`.
- The all-race mechanics run
  `20260903T2107584796377Z-a049a6fb69b34818b7081a1068c3a26b` passed 27/27
  on all four racial stat/resistance/affinity/SLA/resource contracts. Feature
  evidence SHA-256:
  `a90d20c271103068e31e21ec507a35d849c49d29ed8e16e2fc3c4a7fd66b9ee6`.
- Initial command-level SLA run
  `20260903T2110165345311Z-fbf92c0bf63e4cc696f6675235537a9f` failed three
  assertions because the new test treated raw `UnitUseAbility.CanStart` as
  resource-aware. Evidence showed the native player boundary is
  `AbilityData.IsAvailable && CanStart`: at zero uses, `IsAvailable` was false,
  the raw command property remained true, and no resource changed. The
  evidence model was corrected without weakening the player-path gate.
- Corrected guarded SLA run
  `20260903T2116413545673Z-e8c59c8e14f846bdb43b3aa2bad78051` passed 13/13
  on DLL SHA-256
  `9088c8f97d902165a1f559f55b41d8a90b4a3653e223cb81aadeb1468b907432`
  and MVID `e1fd70f4-8284-49b3-972d-ee4373ba0a40`. Feature evidence SHA-256:
  `858c6e7bd8ef9bd6752dfcecdb2e87b1fd9e92a6853d13e385990bd2f12649d0`.

## 2026-09-03 - Shared-catalog and visual ownership audit

- `ElementalRacePublication` remains additive, exact-GUID-aware,
  reference-preserving, deterministic, idempotent, foreign-conflict refusing,
  and single-reference reversible. No shared race array defect was found.
- Visual donors, palettes, body wrappers, presets, links, and customization
  arrays are cloned or newly allocated; no native or third-party donor is
  mutated in place. Elemental Races contains no Unity `Destroy` call.
- Found one concrete defect: visual resource rollback removed later-owned
  cache entries before discovering a foreign replacement at an earlier
  registration, producing a partial rollback on refusal.
- Added an intentionally red pure regression first. The initial run completed
  1,399 tests with exactly the two new rollback cases failing. Implemented a
  two-phase reverse-order ownership plan; a conflict is now detected before
  any cache removal. The successful plan skips absent entries and retains
  exact reverse registration order. The complete suite then passed
  1,399/1,399.
- Bootstrap failure ordering remains selector rollback, blueprint-registry
  rollback, then visual-cache rollback; each failure is isolated and logged.
  No additional ownership defect was found.

## 2026-09-03 - Foundation qualification gate

- Repository validation: PASS. Complete Release domain/reflection suite: PASS,
  1,399/1,399. Clean Release package build: PASS. Independent strict package
  validation: PASS.
- The clean-build package contains 135 entries and is 22,986,873 bytes with
  SHA-256
  `db18732406bc3facdbeecb3d6305016db49b3fbde74e8bb7987afda4f30ab431`.
  Its 5,447,168-byte DLL has SHA-256
  `17c6fd96652888aa8ad5781e216b5dab21606c8221f871f17538a7eedb8b6ca9`
  and MVID `112ead36-b1ed-4f1d-9b06-73376d3bd541`.
- Guarded runtime used the pinned local-runtime artifact with source-state
  SHA-256
  `f1e19f02d70ff8828d4c27c4f6b08a32e67e144126de853c6c0221c82e1f6141`.
  The package contains 135 entries, is 22,980,131 bytes, and has SHA-256
  `373242f111db67cc7cda31c8dbd22071439af4da30906272aa6ae75548fcc811`.
  Its 5,434,880-byte DLL has SHA-256
  `7aceb6c7e32ff6e8b373cc490f31bf09288f3322df99274dafe7022945f1a202`
  and MVID `0d555e10-babd-498e-a000-9a5c6b82851e`. Deployment record:
  `20260903T2133255121232Z`.
- Exact-artifact guarded Steam runs all passed: affinity
  `20260903T2133255988842Z-1c1eb1995b514e639d633bda04626da3`
  (12 assertions; result SHA-256
  `f56f09b864ebf99843e80a7ab1cdb759be936c2c03722fd5142933f94aaf47b1`),
  all-race mechanics
  `20260903T2135544051996Z-c855b8500de740058e9e283d5855d623`
  (27; `763ec220c8dc70eebc1bfbb31fb755ea79668db434dd90915675726bf0057207`),
  racial SLA commands
  `20260903T2137599295331Z-58636ac8f2c8435da12b05de86abb9dd`
  (13; `b749e8e28e0400a2e419dcf9d0cdde0a00dff443ed17b521d1f2caa82e787760`),
  Hydraulic Push
  `20260903T2140036293025Z-87b84a4364f044d7ac75be83523a4285`
  (13; `ad86f66495f0ac7c94aff75f8c8fff10c8c608d8753d9e02323ee64fa1bb580c`),
  native identity/movement
  `20260903T2142120311785Z-ee47d3f88fc3475bb877b4259fa5bb8f`
  (17; `46ae2a2ec716c36ef44304b054c62c0c62cb371ba2e3359bb98c932e8be66e63`),
  visual audit
  `20260903T2144172975324Z-4851ddb035d5488988c65a928d3a4413`
  (5; `4c53f03b7c11e84563a344f15b4299a8490c9d0333236abc9c6de3753958e511`),
  and blueprint/publication observation
  `20260903T2146278436103Z-eabf2189545440cba6e40cff2980baa4`
  (12; `09591c0568eaf1f76478b9bd30cbdef3be82edd4d5bc0be437c7cbc9267a5be8`).
- The three-process transaction used only `KMG_AUTOMATION_WORKING`. Prepare
  `20260903T2148395938477Z-82b7301d2ec34d46ad43f6d9dc6c0ffc`
  passed 11 assertions and saved eight exact race/sex fixtures after spending
  each racial SLA from one use to zero. Module-OFF verification/cleanup
  `20260903T2151259292540Z-0b6b930d96e14626b8a47a4b17ea062c`
  passed 10 assertions: all 68 active identities remained registered, race,
  facts, appearance, equipment, and zero resource amounts survived; level-up
  advanced caster level without refilling; ordinary rest restored exactly one
  use; cleanup removed all fixtures. Fresh-process absence
  `20260903T2154167870215Z-7c987f8ec8614f0681613ec2fc8a2d64`
  passed six assertions with zero residual fixtures or writes.
- Result SHA-256 values for those persistence stages are respectively
  `d723d78c60c94f9a6e912ede7bcbf074d5811aa5aaefac5d70b9540c17141f72`,
  `6004ddd32b6beb8b8f3dd7f0f72c32999f1378380c55d41f44bb466d9bccc8f0`,
  and `35cd4c44ada55a757fd0a313b56c029f804cfefcafad07196f2d9e7dee6f9362`.
  The protected baseline was never selected or modified.
- The persistence runs reported only explicitly non-mechanical presentation
  warnings (low foreground density for four female proxies, DollData/class
  clothing inspection notes). Mechanical assertions were unambiguous;
  screenshots were not used as proof. Release A retains its full visual gate.
- FeatureModules settings were restored byte-for-byte: before/after SHA-256
  `a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
  Temporary module-ON and module-OFF hashes were
  `333e5a3cdb7196ac6c58c9959ad24c81b2b0c98a06804b94e601743685d0fa8e`
  and `fd8420daa53de98aeb7b81538ab4eff4ae8a0eaeadc54342ee53581913de0195`.
- Foundation hardening is PASS. This does not qualify Release A, B, or C.

## 2026-09-03 - Foundation checkpoint and push refusal

- Created local foundation checkpoint
  `9c0b7d7bdfe39dd54947c7a37d601cd91db98027` (`Harden Elemental Races
  foundation mechanics`) on `codex/elemental-races-expansion`.
- Immediately ran the exact mandated
  `Push-KingmakerGunslinger.ps1` command. The external wrapper refused before
  contacting the remote because `codex/elemental-races-expansion` is absent
  from its branch allowlist; it currently allows the historical
  `codex/elemental-races` branch but not this mission's required branch.
- The user-required branch was retained. No policy file, branch, remote, or
  history was changed to bypass the refusal. This is a publication-process
  blocker, not a mechanic blocker, so independent Release A work continues.

## 2026-09-03 - Release A implementation

- Promoted the candidate to assembly/package version `0.0.115` and
  informational version `0.0.115-elemental-heritages` without changing the
  feature-module schema or adding a second module toggle.
- Added four obligatory heritage selections beneath the exact existing Ifrit,
  Oread, Sylph, and Undine race blueprints. Each selection has exactly three
  ordered choices. General markers reuse the 0.0.114 affinity, SLA feature,
  resource, and ability objects by reference; a legacy character with no
  marker continues to resolve as General.
- Added a deterministic overlay reconciler. Alternate markers carry only net
  racial stat deltas and swap project-owned affinity/SLA providers
  add-before-remove. Resource amounts are captured and restored when an
  unchanged resource remains desired, preventing reconciliation from
  refilling a spent use.
- Registered 53 new stable save-bearing identities in exact manifest range
  `e115e1e0a17a4aceb001000000000001` through
  `e115e1e0a17a4aceb001000000000053`. Current manifest counts are 1,759 total,
  1,757 active, two reserved; Elemental Races owns 122 total, 121 active, one
  reserved.
- Native donor audit run
  `20260903T2234504536010Z-48c8648287f54fe99369bfdfe3b30132`
  established exact installed Kingmaker donors for Firebelly, Flare Burst,
  Color Spray, Expeditious Retreat, Shocking Grasp, and Blur. Unerring Weapon
  and Chill Touch were absent. The first is a project-owned exact held-weapon
  confirmation-roll effect; the second is a project-owned multi-touch
  implementation using narrow touch-controller hooks. No donor blueprint,
  array, asset, or spell list is mutated.
- Added six focused policy/architecture cases. Repository validation and the
  complete Release suite pass 1,405/1,405; clean Release compilation,
  packaging, and strict package validation pass.

## 2026-09-03 - Release A live blueprint correction and proof

- First probe orchestration
  `20260904T0040206589395Z-56c3ac31e8344139a197f91a8ff273cb`
  was rejected as `scenario-not-allowed` before any hook, UI action, or save
  action. Result SHA-256:
  `13cf8b5fff9ffc78526df44adb134c7608626edd19bd64b02cb2f395879d4f7c`.
  The missing central catalog membership was fixed and covered by a focused
  wiring regression.
- The accepted retry
  `20260904T0049431335777Z-ab0db286997b479fb71a978c9aa27538`
  timed out after blueprint initialization failed closed and rolled back 18
  owned registrations. Structured log evidence identified a null Aasimar race
  icon passed as the Hydraulic Push fallback. Result SHA-256:
  `f4cc43face4b8ee97763994da885758081442c4e0330d176377c7facc239c744`.
- The narrow repair resolves the exact native Feather Step spell as a
  non-null presentation donor only when the parent-race icon is null and now
  validates every cloned racial SLA icon. No native asset is modified.
- Run `20260904T0101034386491Z-99a7d238acb8426b873fe3969887e4f0`
  passed all 19 live assertions, but its companion DTO inherited Kingmaker's
  global reference serializer and collapsed to an empty reference object. The
  mechanical PASS was retained as diagnostic evidence but superseded.
- Final run `20260904T0106348081056Z-7258c85fa8e14ca498201baac7f51ef4`
  passed 19/19 with a complete isolated 7,300-byte companion record. It proved
  four exact parent races once each at top level, four obligatory selections,
  12 complete choices, exact legacy provider reuse, independent alternate
  SpellLike graphs, 53/53 unique exact-reference registrations, and no save
  access. Runtime-result SHA-256:
  `1acc4b3a2078a45086118330797ce67f463e281f1d3e3545a48cb2383fe53d6d`;
  feature-evidence SHA-256:
  `cfb29951b5194306771158371ad1dec197d18c671515a50d3334d80b728e019b`;
  evidence-manifest SHA-256:
  `ae50327bfe9279cdd9c97513ad2544c1fb278e207d59c70af78c226146a29fb4`.
- The exact runtime artifact contains 135 entries and is 23,007,771 bytes,
  SHA-256
  `af60fcaa4d458ef4ebe43aea5717e29eef65d92fcf0a1674d381877e39506709`.
  Its 5,514,240-byte DLL has SHA-256
  `d04710ae349308a51fb7ce814420537b31eb524b7d0b1361212a98911584d5b3`
  and MVID `45a12bec-2f12-49af-93cb-a0849d3d48aa`.
- Release A remains IN PROGRESS. Mechanics, respec, migration, visuals,
  three-process persistence, and compatibility profiles have not yet passed.

## Next action

Add dedicated Release A mechanics and transition scenarios for every heritage,
then extend the transactional persistence harness and run the remaining visual
and compatibility gates. Retry the exact guarded push after each coherent
checkpoint even while the external branch allowlist remains unresolved.
