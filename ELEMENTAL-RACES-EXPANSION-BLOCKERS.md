# Elemental Races expansion blockers

The six-trait active-buff save regression is resolved by exact native
trait-marker-loss cleanup. Two native processes (9,660 assertions) and four
save-backed processes (230 assertions, 46 retained visual/DollData warnings)
pass on one immutable candidate. The failed result remains historical in STATE;
this incremental correction is not a Release C PASS. The isolated
highest-risk profile still cannot currently load this working fixture; its
save-free compatibility evidence remains separate. The restored installed
configuration successfully loads the exact same working descriptor.

## Active hard blockers

None established.

## Active publication-process blocker

- Local checkpoints, including the first-eight-trait implementation commit
  `530eff1ebe6814fc17a5fc39c1ac50bb215bfbbf`, cannot currently be pushed.
  The mandatory external `Push-KingmakerGunslinger.ps1` wrapper refuses the
  exact required `codex/elemental-races-expansion` branch because it is absent
  from the wrapper's branch allowlist. The historical `codex/elemental-races`
  branch is allowlisted, but changing this mission's branch would violate the
  assignment. No bypass is attempted. Independent implementation continues.

Release B itself has no remaining engineering or qualification blocker. Its
local PASS is separate from this branch-publication blocker.

Release C has no established hard blocker. Its replacement framework and
first eight passive mechanics have incremental proof. Three typed summon
Insights and three reactive-healing traits now also pass focused native tests
in KMG-only and highest-risk combined profiles, and those six traits now pass
module-OFF/ON persistence in the restored installed configuration. Efreeti
Magic additionally passes native KMG-only/combined, actual native-selected
multi-trait OFF/ON saves, spent shared uses and active size effects, native
rest/level/respec/cleanup, fresh absence and renewed pinned 0.0.114 migration.
Six mechanics, the other fourteen traits' persistence, full lifecycle and final
qualification remain. Ray-deflection and
native difficult-terrain contracts still require focused engine evidence.

## Open engineering questions requiring evidence

- The missing legacy-input preflight blocker is resolved by byte-identical
  recovery of both files under `artifacts/release/0.0.114`. The historical
  deployment backup and existing deterministic packer reproduce the pinned
  ZIP `b5c881...`; the release generator's ordered JSON/encoding reproduces
  manifest `485054...`. Both exact hashes were independently recorded before
  recovery. No publisher, download or verifier bypass was used. The corrected
  three-process legacy transaction now also passes mechanically (28 assertions,
  all eight fixtures, exact spent uses and fresh absence). Its 13 warnings and
  subjective visual-review gate remain explicit in STATE; restored inputs alone
  were never treated as qualification.
  For history, the other retained `artifacts/packages` ZIP hashes to
  `0964f1832d1886f5c587a87c5895c67fe8cc8cb8233f000512625cfc6fa18f3a`,
  not the pinned `b5c881...` producer. A bounded search of lab package,
  archive, private, safety, worktree and runtime-evidence locations found no
  exact package/manifest pair. That earlier failed preflight opened or wrote
  no save. Recovery now restores the full original 135-entry package, not a
  substitute DLL-only artifact.
- The factory-only naming repair passes all 114 fact identity checks. Final
  run `20260906T0318087797451Z-d27ad3fc0773431abbd8ae274572b02c`
  additionally proves complete initialization/level-up/command/teardown error
  observation: 4,741 assertions, zero native errors/exceptions, exact cleanup.
  The older `20260906T0239499321459Z-b45648b2f8314fd5a091f2e478b3a558`
  observed only level-up and commands; its scope is not broadened retroactively.
  Current heritage/feat fresh-process persistence also passes mechanically,
  and subsequent pinned legacy migration now passes mechanically. Actual
  complete trait-bearing persistence and subjective visual review remain gates;
  the six Insights/blood traits now have incremental persistence proof.
  The following older observations are retained as investigation history.
- The native summon-pool fixture defect is repaired in run
  `20260906T0207040739900Z-7c5f9b57fa5844efb04541a1d3bf2f4f`: exact
  rolled counts, native pool membership and all 59-unit cleanup pass.
  Its runner PASS is not an overall qualification PASS. Exact project race
  and provider components have duplicate blank native save names, producing
  30 level-up hydration exceptions. The regression now audits every owned
  Elemental blueprint and counts errors across the entire fixture lifetime.
  Repair requires preserving AddFacts saved ownership and requalifying
  actual 0.0.114 migration; blueprint identities must not change.
- Current Insight qualification is FAIL, not a hard stop. Guarded run
  `20260906T0147257495923Z-70d3b26204674a60a96e1f8e2b3282f7` identifies a
  missing request-local native summon-pool service, which aborts group
  spawning after its first creature. The fixture must install and restore
  the real service rather than alter spell mechanics. Independent duration
  and boundary checks run for all three Insights, and all 51 units clean up.
- The same run newly observes native Fact.PostLoad errors during level-up
  fixture cloning due to ambiguous component names. Exact affected facts
  and save implications are not yet established. This must be attributed
  and resolved before current release qualification; no speculative rename
  of save-bearing component identities is authorized by this observation.
- Release B's native maneuver, flight, concealment, fire-enchantment, and
  Small Water Elemental summon surfaces were resolved by isolated guarded run
  `20260904T1428561048826Z-652f2d0914124e21a23e666ceb0f846b`.
  Wings of Air is now implemented and passes exact attack/armor/immunity runtime
  qualification in run
  `20260904T1940018479703Z-78dcc0b63a264ff281f62699090dfd4d`.
  Airy Step, Cloud Gazer, and Inner Breath now use narrow exact catalogs and
  passed behavioral qualification in isolated run
  `20260905T0258431754839Z-f395da4f5be54cbdad4e980f477f2791`.
  Hydraulic Maneuver and Triton Portal passed their exact native
  maneuver/summon qualification in isolated run
  `20260905T0550526363250Z-a4c7158ae8e74168b36082c6c6e6e3a0`.
  Release C audits remain open for ray deflection and difficult terrain.
- Native `DirtyTrickBlind` is present and player-facing; no
  `DirtyTrickDazzle` enum member exists. Hydraulic Maneuver implements the
  printed blind option and explicitly omits dazzle rather than simulating it.
- Base Owlcat Wings is exactly +3 Dodge AC against melee attacks,
  `DifficultTerrain` condition immunity, and `Ground`-descriptor buff
  immunity. Call of the Wild injects a broader `AddFlying`/maneuver package;
  the implemented project-owned buff copies only the base contract and the
  guarded attack pipeline passes without those optional components.
- Visual Adjustments was absent during Release A qualification and is recorded
  as NOT-RUN. Its absence is not a Release A blocker under repository policy.
- Local reflection confirms neither `BlueprintItemWeapon` nor
  `BlueprintWeaponType` exposes a separate material field. The native
  `WeaponSubCategory.Metal` contract is the exact available classification;
  Scorching Weapons now uses it and guarded representative metal, nonmetal,
  natural, replacement, and empty-hand tests pass.

These are investigation items, not hard stops. Features fail closed only under
the mission's hard-stop contract; independent work continues.

## Resolved Release B mechanics findings

- Kingmaker 2.1.7b exposes no `SpellDescriptor.Air`. A second isolated
  KMG-only inventory established eleven exact native Sirocco, elemental
  Whirlwind, and air-derived Cyclone ability identities. Airy Step uses that
  immutable catalog alongside native Electricity descriptor and direct
  electricity-damage predicates. Actual saving throws prove +2 exactly once
  for every catalog entry, a parent variant, and an Air/Electricity overlap;
  Wings of Air replaces that value with +4 total.
- Obscuring Mist is the only native fog/mist/cloud `AddConcealment` provider
  in the isolated inventory. Cloud Gazer bypasses only that exact identity or
  an explicit project Fog/Mist/Cloud marker. Actual attacks prove that Smoke,
  Blur, displacement, invisibility, blindness, darkness, concurrent unrelated
  concealment, and Mirror Image remain effective.
- Kingmaker exposes no general respiration or inhaled-poison rule. The exact
  native poisonous-swamp-gas pair is the only safely distinguishable
  respiration-required catalog. Inner Breath also accepts an explicit project
  semantic marker; actual `RuleApplyBuff` tests prove ordinary poison,
  Stinking Cloud, Cloudkill, and unrelated `SwampGasDOT` remain effective.

- The isolated Kingmaker 2.1.7b inventory exposes eight native
  `AddConcealment` sources but none that is semantically fire or smoke.
  Firesight therefore has no native GUID catalog entries and accepts only
  explicit project-owned Fire/Smoke semantic markers. Guarded native attacks
  prove that Blur, displacement, Obscuring Mist, invisibility, blindness,
  darkness, concurrent unrelated concealment, and Mirror Image remain
  effective. This is a resolved fail-closed design result, not missing content.
- Kingmaker 2.1.7b has no `SpellDescriptor.Light`. An isolated KMG-only audit
  established an immutable seven-GUID native Spell catalog and exact parent
  traversal for Scorching Weapons. Spell-like abilities do not enter the
  Light-spell branch; fire attacks remain covered through their fire
  descriptor or direct fire-damage rule reason.
- The first isolated Ifrit-feat run attached the feat to the Ifrit but made a
  target dummy initiate `RuleSavingThrow`, so the native initiator component
  correctly did not fire. Only the request-local scenario was corrected. The
  rerun proves +2 and replacement +4 modifiers on the feat owner through the
  actual save rule.
- Disk exhaustion and a removed reproducible validator executable caused two
  orchestration failures. Exact staged-profile restoration passed after each;
  no save was accessed. Narrow cleanup removed only current-version generated
  build outputs, the full suite recreated the validator, and the final exact
  artifact/runtime transaction passed.

## Resolved foundation limitations

- Kingmaker 2.1.7b exposes only `RuleCalculateAbilityParams.AddBonusDC(int)`;
  it cannot attach `ModifierDescriptor.Racial` to that event. Exact
  nonduplication is enforced by the one-result affinity policy across the
  effective parent/variant chain.
- Raw `UnitUseAbility.CanStart` does not include resource availability.
  Player-path availability is the native combined
  `AbilityData.IsAvailable && CanStart` boundary and is qualified as such.
- Viewless chargen fixtures cannot safely evaluate `CurrentSpeedMps`.
  Movement qualification uses real native buffs/conditions plus the installed
  `CalculateSpeedModifier` contract; final in-area heritage qualification
  remains required in Release A.

## Resolved Release A implementation findings

- Native Aasimar has no runtime race icon. Hydraulic Push now uses the exact
  native Feather Step icon only as a presentation fallback; all other racial
  SLAs retain their exact donor icon. Live blueprint evidence proves every
  selection, marker, and SLA icon is non-null.
- The first heritage probe was missing central runtime-catalog membership; the
  request was rejected before hooks or state access. A focused regression now
  covers the allowlist entry.
- The first accepted probe exposed the null-icon bootstrap failure and exact
  owned-registration rollback. The repair is narrow and the subsequent live
  bootstrap passes. Neither finding is an active blocker.
- The first live heritage-mechanics run exposed a real marker-first hydration
  defect: inherited General providers could activate after an alternate marker
  had reconciled, leaving both provider sets active. One owned controller on
  the existing trailing heritage-selection fact now performs a post-race
  reconciliation. The corrected guarded run passes this order for all four
  races while preserving spent-resource bookkeeping.
- Call of the Wild installs a broad sticky-touch prefix that removes native
  `UnitPartTouch` state when its own multi-charge part is absent. Chill Touch's
  exact project charge state therefore declares `HarmonyBefore("CallOfTheWild")`
  and returns before that foreign prefix. Guarded evidence proves 20 -> 19
  charges and exact retained touch state for living and undead targets. The
  live audit checks Harmony's declared `before` metadata; its raw registration
  collection is insertion ordered and is not an execution-order report.
- Native Blur's party-member target checker uses `IsPlayerFaction`. The
  save-free Mistsoul fixture now uses the existing player faction without
  entering the party or touching a save, matching that native predicate.
- Fresh module-OFF load exposed an inactive General SLA ability left in the
  native ability collection after the alternate provider fact hydrated. The
  owned reconciler now removes only the exact project ability through native
  fact-collection routing after removing its inactive feature; it does not
  touch resources or foreign/native abilities. A dedicated in-game orphan
  injection regression and the 24-fixture module-OFF process both pass.
- Native save-backed Respec replaces the unit object/descriptor while
  intentionally retaining the actor's stable `UniqueId`. The persistence
  harness previously combined the preparation-only different-ID expectation
  with its stable-identity assertion. A pure phase-aware identity policy now
  requires different IDs only for disposable preparation sources and the same
  ID for persisted sources; both paths still require distinct native objects.
- The pinned 0.0.114 runtime creates one PID-scoped byte-identical DLL cache
  and normalizes `FeatureModules.json` while retaining its byte-exact input as
  `.previous`. The historical verifier now recognizes only that exact overlay:
  cache SHA/MVID, backup bytes, and settings semantics are independently
  checked, while every other extra file or mutation remains a hard failure.

## Resolved Release B persistence findings

- One-round Elemental Strike and exact-item Scorching Weapons effects need
  state that outlives native buff/item teardown during serialization. A small
  schema-versioned project `UnitPart` now owns only absolute game-time end
  ticks and at most two direct `ItemEntityWeapon` references. It restores only
  exact unexpired eligible state, waits for native owned-item hydration, never
  retargets replacement gear, and clears corruption, expiry, death, or missing
  prerequisites. The post-load patch is exact and no-ops for every unit without
  that part.
- The first module-OFF verification reached the command-bearing fixture after
  its one-round state expired. Evidence showed that Kingmaker rejected
  `Game.Instance.IsPaused = true` inside the after-load callback even though
  the previous harness treated the assignment as successful. A focused test
  failed first. The guarded loader now retries from its update boundary and
  refuses all fingerprint or feature inspection until the engine observably
  accepts the pause. The complete three-process rerun and final absence pass.

## Resolved Release B compatibility finding

- A live compatibility replay initially rebuilt the universal and Fighter feat
  arrays after a later publisher inserted foreign entries around KMG entries.
  No foreign entry was lost, but rebuilding violated the required true no-op
  replay and order-preservation contract. A failing executable regression was
  added first. The shared transaction now retains each singular exact project
  reference in its current position, removes only duplicate exact references,
  rejects foreign same-GUID entries, and inserts only missing additions. The
  31-process matrix proves forward replay, reverse rollback, and all
  native/foreign references and order across six installed profiles.
