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

## 2026-09-03 - Release A live mechanics and activation-order repair

- Added the dedicated save-free
  `disposable-elemental-heritage-mechanics` scenario. It uses request-local
  native units and the production selection/reconciliation graph rather than
  adding feature logic to the central runner.
- For all 12 heritage choices it exercises the native selection API, exact six
  ability-score deltas, exact active marker/affinity/SLA/ability/resource,
  2 Fighter + 3 Wizard caster-level scaling, exact
  `10 + spell level + current Charisma modifier` DCs, temporary Charisma
  bonuses and penalties, affinity exclusion from SLAs, spend, level-up without
  refill, and ordinary-rest refill. Four race transition exercises cover
  legacy no-marker General, add-before-remove, alternate-to-alternate,
  alternate-to-General, explicit General, idempotence, and remembered spent
  amounts.
- Run `20260904T0146321972357Z-063ed42c6a2c418db694f49a9812cb3d`
  failed exactly four of 68 assertions. Every heritage choice and ordinary
  transition passed, while all four marker-first hydration exercises found
  two affinities, two SLA features, two abilities, and two resource records.
  Runtime-result SHA-256:
  `736fd2ee6f3ea4f0fe650e881bd0bc01da326f921a326955c0f5089323a4fba6`;
  feature-evidence SHA-256:
  `8a9012830f3f20cbb8407ada6e95036efd470c1b08644012af768a0f4fbdf70c`.
- The defect was activation order: a hydrated alternate marker could reconcile
  before the parent race subsequently activated its inherited General
  providers. The narrow repair gives each existing heritage selection one
  owned activation component. Because the selection is the final parent-race
  feature, it performs one post-race reconciliation after those inherited
  providers activate. No identity, resource, donor, or publication surface
  changed.
- Corrected guarded Steam run
  `20260904T0152229922454Z-3991ff2bbbb44a2096ce6085328a6b39`
  passed 68/68. Runtime-result SHA-256:
  `6ec91796fddfe146a5330505017212895b76a40096e175f767c973d73951bd16`;
  feature-evidence SHA-256:
  `7a8ab109f8d8d4014f6557e0783ab20d33c47cb9bd93c1432c0976a04f9a2b87`;
  runtime-evidence SHA-256:
  `ecf7739ef8a63eec5be189a0eafa26fb6c1f93b37c8a5893f35770e75ee02b3a`.
  No save was opened or mutated.
- The corrected runtime package has 135 entries and is 23,015,151 bytes,
  SHA-256
  `151fb255bbd12f066f078ffa5c177599b14e5dfb92c63257939aa13d0fc6e002`.
  Its 5,543,936-byte DLL has SHA-256
  `ed766bc5a9bca7ecabd74b85968574f8557846ce53f7318d9663789ed54831db`
  and MVID `36359990-4f40-4865-a697-d05ea387e07c`.
- The post-evidence repository gate passed again, including all inherited
  invariants and the version-aware 0.0.115 validator. The complete Release
  suite passed 1,405/1,405. A clean Release build and independent strict
  validation passed for the 135-entry, 23,021,050-byte
  `KingmakerGunslinger-0.0.115-elemental-heritages.zip`, SHA-256
  `7b0bff0a54d0853fdfccc1ee845dc8c692e9800d6a4f3e8abbff9906536dce6a`.
  Its 5,556,736-byte DLL has SHA-256
  `f9ff6d245a4ce5866b90c1360868dd1a0f172b9e1aa39680bbed4551cc93985c`
  and MVID `feef5713-2a78-4c6b-871f-220c50b9f936`.
- This proves the selection, provider, calculation, resource, and hydration
  contracts. It does not yet prove each alternate SLA's player-command
  delivery, a real respec transaction, legacy save migration, visuals,
  three-process persistence, or compatibility.

## 2026-09-04 - Alternate heritage SLA player-command qualification

- Reconciliation qualification was committed locally as
  `aca9aece0933d4713d5eae5cd98e1097fca52325` (`Qualify Elemental Heritage
  reconciliation`). The exact mandated push wrapper was invoked and again
  refused `codex/elemental-races-expansion` because the branch is absent from
  its external allowlist. No alternate push path was used.
- Authoritative Pathfinder rules were rechecked before changing the two
  project-owned SLA implementations. Archives of Nethys confirms that Chill
  Touch grants one touch per caster level, deals 1d6 negative energy plus one
  Strength damage on a failed Fortitude save to living targets, and instead
  panics undead for `1d4 + 1 round/caster level` on a failed Will save, without
  a caster-level cap. The Pathfinder legacy SRD confirms Unerring Weapon's
  one-round-per-level duration and `+2 + floor(CL/4)`, maximum +7,
  critical-confirmation bonus. Sources:
  `https://www.aonprd.com/SpellDisplay.aspx?ItemName=Chill+Touch` and
  `https://pathfinder.d20srd.org/ultimateCombat/spells/unerringWeapon.html`.
- Added a pure `ElementalHeritageSlaPolicy` for Unerring Weapon scaling, Chill
  Touch charge/duration calculations, and exact effective-ability matching.
  Removed the incorrect Chill Touch CL 10 cap and changed Unerring Weapon from
  minutes to rounds. Focused tests cover every breakpoint, living/undead
  duration, and exact GUID matching without expanding the registered test
  count beyond 1,407.
- Added the dedicated, save-free
  `disposable-elemental-heritage-slas` scenario and only catalog/dispatch/
  script registration in central orchestration. It uses native
  `UnitUseAbility`, `AbilityExecutionProcess`, resource availability/rest,
  action graphs, item enchantments, attack rolls, touch delivery, saving
  throws, damage, ability damage, conditions, and exact cleanup. It covers the
  six donor-backed alternates plus Unerring Weapon and both living/undead
  Chill Touch branches.
- The first guarded run
  `20260904T0309099510875Z-a7bd7c561f3844778bac89dd0f224ae4`
  passed 9/19. It exposed two fixture/identity issues: request-local units had
  inherited `BlueprintUnit.IsCheater=true`, causing native resource spend to
  be skipped, and transient `AbilityData` reference comparison could not
  retain multi-touch state. Runtime-result SHA-256:
  `d9e7258eade8214897ea6f5821e230fd544899683175fdb31c334fb7a5e89542`;
  companion SHA-256:
  `9a259349329408385b9f398c53151c134e3640fb30176607401daa6b9fd7c9e4`.
- The second guarded run
  `20260904T0324085340809Z-54707a8a8b174333941185d3e4c8b7d8`
  passed 9/19 after exact spending was restored. It proved that raw
  `UnitUseAbility.CanStart` does not itself enforce resource availability and
  that player admission is the combined `AbilityData.IsAvailable &&
  CanStart` boundary used by the existing racial SLA scenario. It also left
  Chill Touch removal to diagnose. Runtime-result SHA-256:
  `416cb68b381ca8c230ab63085f00662f50912b7d1dd941cf1551b5a893090b87`;
  companion SHA-256:
  `07746aa59da47d93870fb4e72d1b5c312a14fa616fb36ebba70e97ac26d0b385`.
- Local IL inspection established the remaining native boundaries. Blur's
  `AbilityTargetIsPartyMember` checks `IsPlayerFaction`, so only the detached
  Mistsoul caster uses the existing player faction; it never enters the party
  or a save. The installed Call of the Wild sticky-touch prefix removes
  `UnitPartTouch` when its own multi-charge part is absent and returns false,
  suppressing later prefixes. The project prefix now declares
  `HarmonyBefore("CallOfTheWild")`, matches the exact held/executing blueprint
  GUID, retains the project charge part, and returns before that broader
  foreign prefix. This adds no compile-time optional-mod dependency.
- Diagnostic run
  `20260904T0345307589818Z-9014d08085a84b22ab1719270d1876fd`
  passed 17/20 and isolated exactly Mistsoul's faction predicate plus the two
  Call of the Wild touch branches. Runtime-result SHA-256:
  `7c8e17059fd0983600d074e8752ae833e4ceb46564dab6029285653a75b13e0a`.
  The next run
  `20260904T0357431568127Z-f71325f8ba924b6bb3cb1c6ae8141b9d`
  passed all 19 mechanical assertions; its sole failure was a diagnostic that
  incorrectly treated Harmony's raw registration collection as execution
  order. Runtime-result SHA-256:
  `7a8107c16d73868f150bb49de0d2f09a206e8759ef6257355433dc20f6b03660`.
  The audit now reads each live patch's `before` metadata instead.
- Final guarded Steam run
  `20260904T0405120089434Z-cb642458ce4041d989b242982630fda0`
  passed 20/20 with zero warnings and exact global-unit cleanup. All commands
  preserve one use on cancellation, spend exactly one on acceptance, block
  the zero-use player path, and restore exactly one on ordinary rest. Mistsoul
  is natively targetable; Unerring Weapon applies +7 at CL 20 to only the
  selected weapon for 20 rounds and survives unequip; both Chill Touch paths
  retain 19 charges. Living delivery dealt negative-energy damage plus one
  Strength damage; undead delivery dealt no damage and applied 24 rounds of
  frightened in this seeded run. Runtime-result SHA-256:
  `80cdc2dd846c5f1de49b3575b522145603f4b243dee3c0314d6dc33d33d5675c`;
  companion SHA-256:
  `e34d40ed88e27daf02340359e8c55f1aae971c11706aa7fc9b3570becffb4c7c`;
  runtime-evidence SHA-256:
  `c25a31bfec4cec3a435d512b2809065341f06ab3051c2e0faf21b73370974387`.
- That run used deployment `20260904T0405119242095Z`, manifest SHA-256
  `e5a21a649116e9af439c9099bed67c044d7b55132e00cadbe53bad38ebd595fe`,
  preserved settings SHA-256
  `a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`,
  and loaded the exact 5,590,528-byte DLL with SHA-256
  `e7e89a7a3b57679a933b27c49db06e7182443453974feaa76bceda6683abe1c8`
  and MVID `d05fb702-af1f-43b2-83ed-bdadece4c69b`.
- Before the first game launch, package preflight failed because C: was full.
  Exactly 162 reproducible generated package artifacts older than 0.0.114
  were removed, freeing 1,044,481,431 bytes; 0.0.114 and 0.0.115 artifacts,
  evidence, saves, source, and user data were preserved. These generated files
  are recoverable only by rebuilding.
- Post-runtime repository validation passed, the complete Release suite passed
  1,407/1,407, and a clean Release build plus independent strict package
  validation passed. The final 135-entry, 23,038,804-byte
  `KingmakerGunslinger-0.0.115-elemental-heritages.zip` has SHA-256
  `23014b77c1e43fa85773eee5d09299a65364d057dfa8355ab70504b6c8a9e20b`.
  Its 5,603,328-byte DLL has SHA-256
  `af9ae270441a898216301e9f612199b85b8d10ac7fc4bd1f2200f684feba5a16`
  and MVID `f2980361-84e5-4034-aca7-1e4a4e7a241d`. The ZIP remains untracked.

## Next action

Run the remaining exact 0.0.114 legacy migration, death/resurrection,
polymorph/return, and optional-mod compatibility gates. Retry the exact guarded
push after each coherent checkpoint even while the external branch allowlist
remains unresolved.

## 2026-09-04 - Full heritage persistence and native-respec qualification

- Expanded the transactional harness from eight General fixtures to 24 exact
  race/sex/heritage fixtures. The deterministic matrix covers all four parent
  races, both sexes, all three production body presets, and both required
  respec transition families. Character creation and respec use native
  `LevelUpController` paths; each fixture records exact race/facts/stats,
  provider/ability/resource uniqueness, caster level, DollData, live rig,
  materials, and Gunslinger equipment reconstruction.
- Added a constant anchor-local navigable staging point and 3,799 aggregate
  fail-closed combat-boundary checks across the final four processes. The
  harness never clears combat to manufacture a pass. Native `SaveManager`
  readiness is checked before each of the three exact writes to
  `KMG_AUTOMATION_WORKING`; the fourth process performs zero writes.
- Initial complete preparation run
  `20260904T0758229562675Z-050512dd92504b588d957e2aceecf2ae`
  passed 12/12, but module-OFF run
  `20260904T0803563958943Z-bb9529ea22c34ffe9797e576fe555af2`
  failed closed on the first alternate Ifrit. Its race, marker, stats,
  affinity, selected SLA feature/resource/DC/CL, and DollData were exact, but
  the native ability collection contained both the selected alternate and an
  orphaned inactive General SLA ability. Result hashes are respectively
  `3a0e1fa0c2aa00ec392007c5fe99eceb901ba6171cd463f10f2dd48b7f80cd84`
  and
  `b384f06cd27ddf2ece4b5730f6e035b5c74c2605157f739fbbb51a5a3b9d67fa`.
  The failed phase removed all fixtures and performed one exact cleanup save.
- Local IL evidence established that `UnitDescriptor.Abilities` is the native
  `AbilityCollection`/`FactCollection` and that `UnitHelper.RemoveFact`
  dispatches removal through the blueprint's target collection. The owned
  heritage reconciler now removes only exact inactive project SLA abilities
  after their provider facts, verifies their absence, and never edits resource
  amounts. The dedicated mechanics scenario injects precisely the orphaned
  General ability and proves exact removal; final run
  `20260904T0840514424415Z-d08cc9cda91d4dbca260c3f3049501e5`
  passed 68/68. Runtime-result SHA-256 is
  `660bfb1578ec21ec392b778382cfdb712e117a496e3c2b266552d53688c2d3f9`;
  runtime-evidence SHA-256 is
  `1c22e223884288c614616890d21bb2e21bb4f829884a16a289b581516c765d04`.
- A second transaction passed preparation and module-OFF hydration, then
  module-restored run
  `20260904T0828235023424Z-f0bdcd0cc2d04665b8f479d708344db0`
  failed on an internally contradictory test predicate: it proved a distinct
  replacement unit/descriptor and stable saved identity, but also required
  the replacement ID to differ. Runtime-result SHA-256 is
  `9e20634dcba57d6a353aa0066441575659a053c8508be20f0b76df4077fe93df`.
  A pure `RespecActorIdentityExact` policy now requires a new ID for disposable
  preparation and the preserved ID for save-backed respec; both require
  distinct native objects. The failed phase again cleaned every fixture and
  saved the exact baseline once.
- The final four fresh Steam processes all PASS against Kingmaker 2.1.7b:
  - prepare `20260904T0844013659099Z-8682937a3298455b9eed12bbdc539a6e`,
    12/12; result/evidence/index SHA-256
    `430238bf02a4f6529c22f40af8bcd08d11ed05e7a3a3d2e0d24a5682495d73b9`,
    `92e529e2a88ff5198197f3fa5e17696f2ec73a5bf230015dfe0325b338029a38`,
    `2b2769e2ff8e9caac5cae11451d561be577f25608d9aa01ca56221cbda33fb82`;
  - module OFF `20260904T0847415312813Z-0fa36465554a47a5a78d66e3d2c90acb`,
    11/11; hashes
    `062ec3782e018fc2da95ad3432f232c690c0917c9a6f105b5b83c47eed72f259`,
    `ddb2519d48307c84504fc4da14c415c0161fe615fbb598d3f1f482af56c64e41`,
    `073d7372731840a83101c2e955a7be75b5fc0fdff0b7db3e2e84be9159c443cc`;
  - module restored/respec/cleanup
    `20260904T0850532325846Z-a6e7f664417f4669b4a9ebd08e35f02a`,
    10/10; hashes
    `258933836bbab46449aadf4518e9adf6da850494b1648846cfba38feecde79c8`,
    `64f3e275a1447b65f1808e0b3b9cccdc3c30eb9908beddb8c6e623bdeaff31f8`,
    `56171c01efeb13c14bc4374ce408aef07b06fedd836db59fd74f0d49799e12b0`;
  - fresh absence
    `20260904T0855068314112Z-8cfd708818ca4f53bca9487e977db573`,
    7/7; hashes
    `cfd3eb102d068764334de9103c8ced33da2a0bb2e7abc223ff71758692a8724b`,
    `e663de2492a6e4e0922a4229529a3cb8779e83e18aa599299931a2c0e2523a3c`,
    `b629a42b32e9ebb26e345f1be9e3851659de76f97f569182c4ae3dfdf5d955ee`.
- Each of the first three final processes captured 24 sidecars, 48 PNGs, and
  120 labelled views; the retained final transaction therefore contains 72
  sidecars, 144 PNGs, and 360 views. It proves exact ON creation/respec/spend,
  OFF identity hydration/publication rollback/level-up/rest/re-spend, ON
  same-race native respec/cleanup, and fresh-process absence. The final save is
  `Manual_299_KMG_AUTOMATION_WORKING.zks`, 2,898,422 bytes, SHA-256
  `846fa8357b6b323da0149d4d66e0b0d480f12eea9cc5891ed9cb131015d444dc`.
  The protected baseline was never written.
- Original settings were restored exactly to SHA-256
  `a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`;
  temporary module-ON/OFF hashes were
  `333e5a3cdb7196ac6c58c9959ad24c81b2b0c98a06804b94e601743685d0fa8e`
  and
  `fd8420daa53de98aeb7b81538ab4eff4ae8a0eaeadc54342ee53581913de0195`.
  Optional PNGs from superseded/failed disposable runs and reproducible build
  staging were removed only after structured evidence was retained.
- Final repository validation, all 1,407 domain/reflection tests, exact-
  reference compilation, clean Release packaging, and strict validation pass.
  The 135-entry, 23,040,636-byte package has SHA-256
  `43a77f8a51472867ab913586ad68ba5996a0345fa1041ea5ff0bd74f860a461a`.
  Its 5,623,808-byte DLL has SHA-256
  `b5c0b6ff706b842a5b934ffa1f3b4910983b0321a68b0a033a703459bcae44b3`
  and MVID `0fd178ac-9024-4717-9fdd-c84a4ad09775`. Deployment manifest
  `20260904T0840326483079Z` has SHA-256
  `ebc7f6e6fefe2a8b5bc9ce1823585a5a04cb695b7b3c55d368b09a04e6a550f1`.
  The ZIP remains untracked. Legacy 0.0.114 migration,
  death/resurrection/polymorph, and compatibility are still open; Release A is
  not yet declared PASS.

## 2026-09-04 - Exact 0.0.114 migration producer and transaction harness

- Added a dedicated historical deployer that accepts only the repository's
  exact 0.0.114 release artifact and independently pins authoritative commit
  `6874dc15a27ded132456dbdd480f47c794543a05`, the 135-entry ZIP SHA-256
  `b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694`,
  DLL SHA-256
  `09af96b95e2abfa39e45f30c8ccb4cb1e8772981dd3be17846f07cbbd2dd8262`,
  and MVID `dcd73856-39d4-40ce-9b05-77bf249103d7`. It validates archive paths,
  extracted and installed file catalogs, backs up the exact live directory,
  preserves feature-setting bytes, and emits a distinct qualified historical
  deployment authority. Its `-WhatIf` execution validated the package and
  left zero temporary directories and no deployment.
- Added an isolated launcher reuse authority that permits this historical
  deployment only for `elemental-race-persistence-prepare`, expected version
  0.0.114, and exact save `KMG_AUTOMATION_WORKING`. Current-source reuse and
  historical reuse are mutually exclusive, and evidence collection rechecks
  the historical package, deployment, installed DLL, file catalog, MVID, and
  settings hash rather than presenting the artifact as a current build.
- Added a guarded transaction that enables Elemental Races, deploys 0.0.114,
  creates and spends the eight legacy General race/sex fixtures, deploys the
  current 0.0.115 build, verifies markerless-General hydration and an explicit
  idempotent reconciliation without resource refill or stat drift, cleans and
  saves the fixtures, verifies fresh-process absence, and finally restores the
  original settings bytes and current artifact even after a phase failure.
  The transaction hashes each runtime result and evidence manifest and never
  names the protected baseline.
- The current scenario uses the first eight stable fixture identities retained
  from the original General-only ordering, requires the exact 11-member loaded
  party, verifies no heritage marker or selection fact was retroactively
  inserted, proves the legacy General affinity/SLA/resource/ability references
  are unchanged, captures both sexes for all four parent races, and performs
  exactly one cleanup save. A new pure policy test fixes the legacy count at
  eight and rejects an invalid empty race catalog.
- The read-only historical-artifact qualification test passes with all pinned
  identities and four PowerShell scripts parsing cleanly. Runtime deployment
  safety passes 28/28. Repository validation, the complete Release domain and
  reflection suite (1,407/1,407), clean Release compilation/package creation,
  and independent strict package validation pass. The untracked 135-entry,
  23,049,189-byte candidate ZIP has SHA-256
  `98221bc9b8481104688907174ca48036972ec8a2005648d14d1fedf48b06c345`;
  its 5,644,800-byte DLL has SHA-256
  `546fb63bdfe097a9dd16060147274f24b7789005fa59f7739650ee8c80e1e3cb`
  and MVID `aabd0627-b8a1-4402-b289-a31029e80e72`.
- Runtime migration remains pending a clean checkpoint commit and immutable
  Build-Local deployment. No migration PASS is claimed from the harness or
  build alone.
- The first transaction attempt, ID
  `20260904T0959300044235Z-elemental-race-legacy-migration-transaction`,
  failed before any game launch because the shared request preflight still
  required the active source version 0.0.115. It created zero runtime phases;
  no save was loaded or written. Transaction SHA-256 is
  `5251867e5e80ef84bc0145f6726fe3aabb17a133eee212537137cb4c0b9faa11`.
  The pinned historical deployment manifest SHA-256 is
  `54392c8b118bf6715915829c9ca4659bdf9c8e83534246bf7b2764cbb4914e64`.
  The `finally` path restored settings exactly to SHA-256
  `a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`
  and reinstalled the current DLL; restored deployment-manifest SHA-256 is
  `b91bfe0fb5f83e42625b1b0a99a75202fc197ef93d92b9ded9401c62212185dc`.
- Repaired only that boundary: both preflight passes now accept 0.0.114 solely
  when an explicit `PermitQualifiedElementalRaces114` authority accompanies
  `elemental-race-persistence-prepare`; the same authority rejects every other
  scenario/version combination, and 0.0.114 without it remains rejected.
  Runtime preflight passes 202 checks, the historical and deployment suites
  pass, repository validation passes, all 1,407 domain/reflection tests pass,
  and the subsequent clean Release build and strict package validation pass.

## 2026-09-04 - Historical runtime overlay ownership correction

- The next legacy transaction reached the pinned 0.0.114 game process and its
  prepare scenario passed all 11 mechanical assertions. Run
  `20260904T1013490325299Z-0aa31a4a5af44e3d976e00fedef36a65`
  created and spent the eight legacy General race/sex fixtures and wrote only
  `KMG_AUTOMATION_WORKING`. Its runtime-result and persistence-index SHA-256
  values are
  `c37d1886dafdc2fc40f29b73b5d6268152c386af3701b7dfe8c9529b4c8bade2`
  and
  `2d6cda5bf53eed1ab056d12d674c0617e8e3347022063541121275b9d54582d8`.
- Evidence collection then failed closed before crediting the phase because
  the historical verifier compared the post-run live tree to its pristine
  deployment catalog. Direct inspection proved exactly two known runtime
  products: `FeatureModules.json.previous`, whose SHA-256 equals the exact
  deployed settings bytes, and `KingmakerGunslinger.dll.12046.cache`, whose
  SHA-256 and MVID equal the pinned 0.0.114 DLL. The live settings file differed
  only in JSON formatting and was semantically identical to its byte-exact
  backup. No arbitrary package file changed.
- Transaction
  `20260904T1013438943928Z-elemental-race-legacy-migration-transaction`
  therefore records FAIL with zero credited phases; transaction and
  orchestration SHA-256 values are
  `ddc6d869cdfad5c3a8fe67f2fa130109da5bc108c2f45aa3eb7b4c370aa921c0`
  and
  `ed3244ccc9d908c653242f6f87019527ac0f66c01c5701ea77a15b588a45b865`.
  Its `finally` path restored settings exactly to
  `a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`
  and restored the current 0.0.115 artifact; restored deployment-manifest
  SHA-256 is
  `4e35aa75e0eb47ddf9ed4343a6024c6f7f5afd376cdf589d68775a9dd0eee87e`.
- Added a narrow post-run overlay verifier. It admits at most one root-level,
  positive-PID DLL cache and only when both SHA-256 and MVID equal the pinned
  DLL. It admits the settings backup only when its bytes equal the deployed
  settings, and admits current-settings normalization only when the flat JSON
  values are exactly equivalent. Missing package files, arbitrary extras,
  multiple caches, cache tampering, backup tampering, and semantic settings
  drift all fail closed. Focused deployment safety now passes 35/35, the
  historical-artifact suite passes, and the verifier accepts the captured
  post-run tree in `normalized-with-exact-backup` mode with exactly the two
  observed runtime files. The complete migration transaction remains pending
  a clean commit/build; no migration PASS is claimed yet.
- Repository validation, all 1,407 Release domain/reflection tests, the clean
  Release build/package, independent strict package validation, PowerShell
  parsing, and all 35 focused deployment-safety checks pass for this repair.

## 2026-09-04 - Exact legacy migration and visual state-transition qualification

- The correlated three-process legacy sequence now passes against the pinned
  0.0.114 release and current 0.0.115 build. Historical producer
  `20260904T1013490325299Z-0aa31a4a5af44e3d976e00fedef36a65`
  passed 11/11 (runtime-result SHA-256
  `c37d1886dafdc2fc40f29b73b5d6268152c386af3701b7dfe8c9529b4c8bade2`,
  persistence-index SHA-256
  `2d6cda5bf53eed1ab056d12d674c0617e8e3347022063541121275b9d54582d8`).
  The current receiver
  `20260904T1052083826042Z-c9c9164de51c467caa8bab191c5bd68c`
  passed 10/10; its result/evidence/index hashes are
  `68ddabffb4ab34e6d821a3ed9091c10e3edd7d60dc2634665187aa1103e5cf88`,
  `13dab9d38c3f6d8191fb05d3ae8b0501d3c5727f77f50e9ac29f1cf036854d07`,
  and
  `964cad4346ecd9791938b0f3419111510a8d9911a0bcb4c24da0715408cf8606`.
  Fresh absence
  `20260904T1055286220098Z-7f788486bf0c4a68b4eaf4d4d2bf5d89`
  passed 7/7 with hashes
  `5d75aec84ce7ddd91630b5afe8d5a3c0e17870913184579a4e082750e9f6c1e1`,
  `f4acab2af84eb09f9332650146bf13e43aa569a52dafc13213d4f51a999bca4a`,
  and
  `dca4174bd43108169f1bc65e859db7ef1cf4e2d49109c3cd739c3f165ad48eed`.
- The sequence proves all eight legacy General race/sex fixtures retain exact
  parent race and legacy affinity/SLA/resource identities, marker absence,
  final stats, spent daily amount, DollData, and appearance. Current
  reconciliation is idempotent and does not restore the use; exact cleanup and
  absence hold in the next fresh process. The protected baseline was excluded.
  Original settings were restored byte-for-byte to SHA-256
  `a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
- A wrapper retry after the already-successful producer timed out only on a
  missing stable fingerprint after its load callback. Run
  `20260904T1038336403820Z-e052f20dc40b4b888f2adb9e041f84d4`
  is retained as FAIL diagnostic evidence and is not part of the PASS chain.
  The earlier producer's runtime products are accepted only by the repaired
  exact-overlay verifier; no failed wrapper transaction is relabelled PASS.
- The first comprehensive motion attempt failed on Win32 disk-full error 112
  while writing an optional PNG. The failed result is retained as diagnostic
  evidence; only its disposable PNG bulk was removed after structured evidence
  was preserved. Final fresh-process run
  `20260904T1109218928176Z-483ad4f0c74b4b5aaed745970fb67985`
  passed 18/18. Result/evidence/index SHA-256 values are
  `a457765bce7a788b150513be5ebfb834d1b1743356b2aa01fda9dea8f90311dd`,
  `a053be62963c9fcdb58f5d8e7d83c8f08a66c353c5fe879b9a741bc06ba1453d`,
  and
  `9941832b6f8b898e9e30037b2bc1a4590e57300244f0286acca7ce8e6c2d2cbc`.
  It covers both sexes of every parent race, 216 motion records/864 views and
  64 transition records/256 views: native SLA act/restore, prone/restore,
  death/resurrection, Beast Shape II/return, locomotion, turn, firearm reload
  and attack, melee, material completeness, and cleanup. Two warnings reserve
  subjective contact-sheet review and do not weaken the mechanical result.

## 2026-09-04 - Release A compatibility matrix and local qualification PASS

- Commit-bound runtime artifact from
  `1613cf8a766f680e28d201341327feb25b52dc5a` contains 135 entries and is
  23,043,017 bytes, SHA-256
  `9f445409336829fed6ec31754b206b3f2f8944da5fe40f4eddec36fff6b224f6`.
  Its 5,632,512-byte DLL hashes to
  `192626200791f38cf76492a7b2b4c5dc1cba5f4e4da298585527a018b93141cf`
  with MVID `c5997e3e-e0b2-4983-b70f-ea23d42c4c03`. Build-Local passed all
  1,407 tests, exact-reference compilation, repository validation, clean
  packaging, and strict validation.
- Six required installed profiles passed in both module states: standalone,
  Call of the Wild, Races Unleashed, Call of the Wild + Favored Class, the
  minimum valid Tweak or Treat stack, and the highest-risk combined stack.
  Their ON/OFF transaction pairs are respectively
  `compat-20260904T114414Z-a66a779679d0` / `compat-20260904T114948Z-7f2951acc45e`,
  `compat-20260904T115156Z-2a6b5b973d1f` / `compat-20260904T115946Z-7ae5bdca8d5b`,
  `compat-20260904T120220Z-ba652f683300` / `compat-20260904T120742Z-6faadb82cbdc`,
  `compat-20260904T120939Z-9bcc926da461` / `compat-20260904T121732Z-f8682ba4d712`,
  `compat-20260904T122005Z-572beaaf1679` / `compat-20260904T122804Z-f34a35be8f31`,
  and `compat-20260904T123051Z-7acb11877f1d` /
  `compat-20260904T124044Z-56f3ee53fb73`.
- All 31 guarded processes and 365 runtime assertions passed with zero
  runtime-result warnings. ON proved expected mod identity, foreign catalog
  preservation, singular contiguous Elemental publication, and the complete
  heritage graph. OFF proved complete identity registration and zero published
  Elemental top-level races. Every transaction observed staged mutation and
  exact restoration. The original/restored 968-entry mod tree hashes to
  `376f3a6ce9432789d00bb2c8e314d8dfdb4ca2d12d14a9f709aae16673263999`;
  feature settings hash to
  `a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
  The exact 31 result/evidence hashes and twelve transaction hashes are in the
  durable state JSON.
- Highest-risk attribution retained known optional-renderer shader, GPU,
  missing-script, and lightmap fingerprints; no new KMG warning/error was
  introduced. Favored Class remained compatibility-only with no new FCB
  behavior. Visual Adjustments was absent and is NOT-RUN, as permitted.
- Release A's required source, validation, build/package, migration,
  persistence, state-transition, and installed compatibility gates now PASS
  locally. The qualification validator now requires the mission's dedicated
  Tweak or Treat profile instead of the older redundant CotW + Races Unleashed
  profile. The latter remains explicitly NOT-TESTED rather than being inferred.
- The mandated push wrapper still refuses the exact user-required branch due
  its external allowlist. This blocks publication only; independent Release B
  engineering proceeds under the mission's hard-stop policy.

## Next action

Implement Release B policy services, stable identities, blueprint publication,
and the first focused mechanical group from the now-qualified native contracts.

## 2026-09-04 - Release B native-contract audit PASS

- Added dedicated save-free scenario
  `observe-elemental-feat-native-contracts`, its central dispatch/PowerShell
  allowlists, compatibility-profile support, and focused domain test. The
  scenario inventories live combat-maneuver values, exact concealment sources,
  Fire weapon enchantments, Small Water Elemental spawn actions, elemental
  unit fact graphs, and six field-level blueprint contracts. Feature logic was
  not added to `RuntimeTestRunner`.
- Complete local validation passed 1,408/1,408 tests, exact-reference Release
  compilation, clean package creation, and strict 135-entry package
  validation. The audited pre-commit package is 23,049,920 bytes, SHA-256
  `7771e740bd7011359a3348ba74822dafa09d6751102c56845331dcbb95332541`;
  its DLL SHA-256 is
  `ae62fd0500a99b6e163f2e85c25b7b714197111cb866bceebf088e2b3df4b286`
  and MVID is `04c61b77-1a0b-4bfb-a1fd-e7bfbb844344`.
- Initial combined-profile run
  `20260904T1411055050795Z-4211d818beb34719a186ca3e8feb9a31`
  passed 8/8 and revealed optional modification of native wing facts. The
  audit was therefore rerun transactionally with only KMG enabled rather than
  treating the combined graph as Owlcat authority.
- Final isolated run
  `20260904T1428561048826Z-652f2d0914124e21a23e666ceb0f846b`
  passed 9/9 with zero warnings. Result/audit/companion SHA-256 values are
  `ecbb01fcbf63c4f0501afcad50d4ddae0bbea0e5b8eee86ab2490a14d3126e71`,
  `45744802127f3b227b3aec36fcad85e5ead5fa60d959910e900847cfef023344`,
  and `0c92ae86cab445b3de0ab586cce286af992ceb0651f61b3a1760510054e20204`.
  Transaction `compat-20260904T142724Z-1dafdf0d5614` restored exact original
  state; its SHA-256 is
  `bc684a5848a9573ae04aa0e382225c16746d161cd82937ae2812659b1cdabe95`.
- Native `CombatManeuver` provides `DirtyTrickBlind` but no Dazzle variant.
  The printed blind choice is implementable through the actual maneuver path;
  Dazzle is an explicit engine omission.
- Native draconic Wings buff `08ae1c01155a2184db869e9ebedc758d`
  provides +3 Dodge AC against melee, `DifficultTerrain` condition immunity,
  and `Ground` buff-descriptor immunity. Angel Wings adds Ground spell
  immunity. Native Airborne is instead two conditional -1 attack/damage rules.
  Wings of Air will use the narrower draconic contract and armor gating. It
  will neither clone CotW's injected `AddFlying` nor grant trip/prone immunity.
- Base Kingmaker exposes eight `AddConcealment` components; the only native
  `Fog` entry in the isolated graph is Obscuring Mist buff
  `61b312b8f91cc48418768b77cd6dcc02`. Exact source identities—not blanket Fog
  descriptor suppression—will drive vision feats.
- Eleven native fire `WeaponEnergyDamageDice` enchantments were inventoried.
  Exact Flaming identity `30f90becaaac51f41bf56641966c4121` is 1d6 fire.
  Scorching Weapons will use project-owned item enchantments and exact
  fire-enchantment nonstacking checks rather than mutating a donor.
- Native summon ability `107788f47c4481f4db6da06498b28270` spawns exact
  Small Water Elemental `56372b0a2749c224392a5ee74105c534`, linked to the
  caster, not directly controllable, for rounds based on caster level. Triton
  Portal can reuse this native action model with a project-owned 1d3 count and
  no dependency on the expanded-summoning publication toggle.

## 2026-09-04 - Release B authoritative rules and pure policy checkpoint

- Re-read the exact Archives of Nethys entries for Elemental Strike,
  Scorching Weapons, Inner Flame, Blazing Aura, Firesight, Airy Step, Wings of
  Air, Cloud Gazer, Inner Breath, Hydraulic Maneuver, and Triton Portal. The
  catalog records the four Combat feats, exact 7/9/11/13 level gates, feat
  chains, active-Hydraulic-Push prerequisites, action economy, and durations.
- Added a dependency-free Release B behavior policy. Focused executable tests
  cover every Elemental Strike breakpoint and parent-race energy, exact race
  rejection, spell/nonweapon exclusion, event deduplication, two-item held
  weapon snapshots, Scorching Weapons nonstacking and Inner Flame replacement,
  overlapping save predicates exactly once, Wings armor gating, exact vision
  domains, respiration-only immunity, friendly Blazing Aura turn starts,
  native Hydraulic Maneuver choices/formula, and Triton Portal 1d3/duration.
- Kingmaker exposes genuine `DirtyTrickBlind` but no dazzle maneuver. The pure
  maneuver roster therefore contains Bull Rush, Disarm, Trip, and Dirty Trick
  (blind); dazzle remains the recorded engine omission. Triton Portal remains
  exactly 1d3 Small Water Elementals for one round per total character level.
- Read-only reflection of local 2.1.7b assemblies confirms
  `BlueprintItemWeapon` and `BlueprintWeaponType` have no weapon-composition
  field. The upcoming runtime boundary must use an immutable audited category
  or exact-type catalog and must qualify representative metal/nonmetal items.
- Repository validation and the complete Release suite passed 1,408/1,408
  cases. The five new focused policy methods execute from the existing
  `elemental-feats.native-audit` case, preserving the frozen 0.0.115 test-count
  record instead of rewriting older release evidence. A direct intermediate
  run with five temporary runner rows also passed 1,413/1,413 before the rows
  were consolidated; that intermediate count is diagnostic, not a release
  gate.
- The subsequent clean Release build and strict standalone package validation
  passed. The mechanically inert 0.0.115 checkpoint package contains 135
  entries and is 23,053,358 bytes, SHA-256
  `b1f9f3fac02be684242ca190b10991d25eb82b9cee76e98dd71c450dcb6e9fc5`.
  Its 5,667,328-byte DLL hashes to
  `dd0b1c830d2b87bf6e1e46aa05abddd747d68f01654d806f066531a9752f7366`
  with MVID `5a89d25c-4106-40eb-bd80-328485463a9b`. No deployment occurred.
