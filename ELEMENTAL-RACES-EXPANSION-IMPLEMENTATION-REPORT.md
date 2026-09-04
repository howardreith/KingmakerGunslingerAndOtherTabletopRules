# Elemental Races expansion implementation report

## Current outcome

**IN PROGRESS - FOUNDATION AND RELEASE A MECHANICAL/COMPATIBILITY GATES PASS;
RELEASE B NATIVE CONTRACTS, FIXED IDENTITIES, BLUEPRINT GRAPH, AND SELECTOR
PUBLICATION ARE LOCALLY QUALIFIED; GAMEPLAY MECHANICS REMAIN IN PROGRESS. THE
REQUIRED BRANCH PUSH REMAINS BLOCKED BY AN EXTERNAL ALLOWLIST.**

The mission began from clean authoritative `master` commit
`6874dc15a27ded132456dbdd480f47c794543a05` on dedicated branch
`codex/elemental-races-expansion`. The first work phase is the required
0.0.114 foundation audit and hardening is complete and independently
qualified. Release A source, identities, versioning, focused tests, live
blueprint graph, selection/reconciliation, exact SLA parameters, resource
lifecycle, alternate SLA command delivery, native respec, module-OFF hydration,
four-process persistence/cleanup, exact 0.0.114 migration, native
death/resurrection and polymorph/return, and all six required installed
compatibility profiles have live proof.
Historical Elemental Races evidence is
preserved as historical evidence only and does not qualify new release
behavior.

## Planned release inventory

| Release | Version | Scope | Status |
| --- | --- | --- | --- |
| Foundation | 0.0.114 baseline | affinity, SLA, movement/maneuver, ownership, runtime organization | PASS |
| A | 0.0.115-elemental-heritages | twelve heritage choices under four parent races | PASS LOCALLY; REQUIRED PUSH BLOCKED EXTERNALLY |
| B | 0.0.116-elemental-feats | shared, Ifrit, Sylph, and Undine feat catalog | IDENTITY/PUBLICATION CHECKPOINT PASS; MECHANICS IN PROGRESS |
| C | 0.0.117-elemental-traits | replacement slots and required alternate traits | NOT STARTED |

Favored-class bonuses are out of scope.

## Authoritative baseline

- Starting and fetched master SHA:
  `6874dc15a27ded132456dbdd480f47c794543a05`
- Intervening master commits: none
- Starting version: `0.0.114` / `0.0.114-elemental-races`
- Starting manifest: 1,706 total, 1,704 active, two reserved
- Starting Elemental Races manifest: 69 total, 68 active, one guarded reserved
- Feature module schema/count/boundary: 10 / 11 / 24
- Inherited race identity model: four exact `BlueprintRace` objects using
  `RaceId.Aasimar`, no `OutsiderType`, Keen Senses adaptation
- Inherited publication model: unconditional identity registration plus
  module-gated atomic additive selector publication

## Qualification status

The clean 0.0.114 branch baseline is qualified: repository validation passed;
the complete dependency-free domain/reflection suite passed 1,390/1,390; the
clean Release build and packaging pipeline passed; and an independent strict
package validation passed. The baseline ZIP contains 135 entries and is
22,977,592 bytes with SHA-256
`b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694`.
The 5,411,328-byte DLL has SHA-256
`09af96b95e2abfa39e45f30c8ccb4cb1e8772981dd3be17846f07cbbd2dd8262`
and MVID `dcd73856-39d4-40ce-9b05-77bf249103d7`.

Foundation behavior/runtime qualification is complete. Release A is now also
mechanically qualified. Release B is active at version 0.0.116 with its
identity/publication checkpoint qualified, but its mechanics and later gates
remain incomplete; Release C is incomplete. Foundation spell
affinity, exact SLA calculation and command behavior, native movement layering,
Hydraulic Push, visual ownership, blueprint publication, and the three-process
module-OFF persistence transaction have passing guarded evidence. The
foundation deterministic suite passed 1,399/1,399.

The affinity predicate now requires the effective ability or one of its
parents to be exact `AbilityType.Spell`, plus reference-identical non-null
spellbook context. It applies once across a variant chain and rejects
SpellLike, item, supernatural, kinetic/nonspell, and context-free calls.
Kingmaker exposes no modifier-descriptor overload on the DC event, so exact
policy nonduplication is the documented engine-compatible equivalent.

The ownership audit found and repaired one concrete issue: visual-cache
rollback could partially remove owned entries before encountering a foreign
replacement. A test-first pure removal plan now validates the complete batch
before any reverse-order cache mutation. Race selector publication, donor
arrays, project proxies, optional entries, and bootstrap rollback ordering
otherwise passed the audit; Elemental Races destroys no Unity object.

The clean foundation package passed strict validation with 135 entries,
22,986,873 bytes, SHA-256
`db18732406bc3facdbeecb3d6305016db49b3fbde74e8bb7987afda4f30ab431`;
its DLL SHA-256 is
`17c6fd96652888aa8ad5781e216b5dab21606c8221f871f17538a7eedb8b6ca9`
and MVID is `112ead36-b1ed-4f1d-9b06-73376d3bd541`. Exact guarded run IDs,
runtime artifact hashes, settings restoration hashes, persistence results, and
the bounded diagnostic failures are maintained in the state and journal.

Release A adds four obligatory three-choice selections and 53 stable manifest
identities without changing the four parent race or legacy provider GUIDs.
All 12 heritage definitions, exact stat overlays, affinity presentations, and
SLA graphs are implemented. The installed donor audit selected native
Firebelly, Flare Burst, Color Spray, Expeditious Retreat, Shocking Grasp, and
Blur; project-owned bounded implementations cover absent Unerring Weapon and
Chill Touch. The complete suite passes 1,407/1,407, and the post-runtime clean
Release build plus independent strict package validation pass.

Guarded Steam run
`20260904T0106348081056Z-7258c85fa8e14ca498201baac7f51ef4`
passed 19/19 live blueprint assertions against DLL SHA-256
`d04710ae349308a51fb7ce814420537b31eb524b7d0b1361212a98911584d5b3`
and MVID `45a12bec-2f12-49af-93cb-a0849d3d48aa`. It proved exact top-level
race counts, selection shape/order, General reference reuse, alternate
SpellLike provider separation, complete presentation, and 53/53 exact live
registrations without touching a save. Runtime-result SHA-256 is
`1acc4b3a2078a45086118330797ce67f463e281f1d3e3545a48cb2383fe53d6d`.
This was the initial blueprint proof; the later evidence below completes the
remaining Release A gates rather than retroactively treating this run alone as
release qualification.

The first dedicated heritage-mechanics run then passed 64/68 and exposed one
real activation-order defect in all four parent races: marker-first hydration
could leave both alternate and inherited General providers active. A narrow
owned controller on the existing trailing heritage-selection fact now performs
post-race reconciliation. Corrected guarded Steam run
`20260904T0152229922454Z-3991ff2bbbb44a2096ce6085328a6b39` passed
68/68 live assertions across all twelve choices and all four transition
matrices. It proves exact live stats, provider/resource uniqueness, exact
multiclass CL and current-Charisma DC calculation, affinity exclusion,
spend/no-level-refill/rest, add-before-remove, explicit and legacy General,
idempotence, and marker-first activation. Runtime-result SHA-256 is
`6ec91796fddfe146a5330505017212895b76a40096e175f767c973d73951bd16`
and companion SHA-256 is
`7a8ab109f8d8d4014f6557e0783ab20d33c47cb9bd93c1432c0976a04f9a2b87`.
The dedicated save-free alternate-SLA run
`20260904T0405120089434Z-cb642458ce4041d989b242982630fda0` then passed
20/20 with zero warnings. It proves native command cancellation, exact
one-use commitment, zero-use blocking, rest recovery, all six donor-backed
effects, exact-item Unerring Weapon duration/confirmation behavior, and both
living and undead Chill Touch delivery with 20 -> 19 persistent charges. It
also proves the explicit Harmony-before contract required alongside the
installed Call of the Wild sticky-touch prefix. Runtime-result SHA-256 is
`80cdc2dd846c5f1de49b3575b522145603f4b243dee3c0314d6dc33d33d5675c`;
companion SHA-256 is
`e34d40ed88e27daf02340359e8c55f1aae971c11706aa7fc9b3570becffb4c7c`.

The transactional persistence harness now covers 24 fixtures: all four parent
races, both sexes, all three heritages, and every production body preset. The
first full module-OFF load exposed an exact inactive-General SLA ability orphan
on alternate characters. The owned reconciler now removes only that exact
project ability through the native fact collection after its inactive provider
fact, without touching resource amounts or foreign/native abilities. The live
68-assertion mechanics scenario injects and removes this precise hydration
state. A later restored-module run exposed a contradictory harness-only actor
identity assertion; the pure policy now distinguishes disposable preparation
(different ID) from save-backed native respec (distinct object, preserved
stable ID).

The final four fresh Steam processes passed 40/40 aggregate assertions:
prepare `20260904T0844013659099Z-8682937a3298455b9eed12bbdc539a6e`,
module OFF `20260904T0847415312813Z-0fa36465554a47a5a78d66e3d2c90acb`,
module restored/respec/cleanup
`20260904T0850532325846Z-a6e7f664417f4669b4a9ebd08e35f02a`, and
fresh absence `20260904T0855068314112Z-8cfd708818ca4f53bca9487e977db573`.
They prove 24 exact native creation/respec records, exact stats/providers and
spent resources, module-OFF identity registration with selector rollback,
level-up without refill, ordinary-rest refill and exact re-spend, 72 sidecars,
144 retained PNGs/360 labelled views, module-ON same-race heritage transitions,
exact cleanup, and zero leaked fixture identities in a fourth process. Each of
the first three phases made exactly one guarded write to
`KMG_AUTOMATION_WORKING`; the absence phase made none. Original settings were
restored to SHA-256
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
The complete per-run hashes and the two diagnostic failures are recorded in
the state and journal. This independently qualifies new 0.0.115 persistence.

The commit-bound runtime qualification package contains 135 entries and is
23,043,017 bytes, SHA-256
`9f445409336829fed6ec31754b206b3f2f8944da5fe40f4eddec36fff6b224f6`.
Its 5,632,512-byte DLL has SHA-256
`192626200791f38cf76492a7b2b4c5dc1cba5f4e4da298585527a018b93141cf`
and MVID `c5997e3e-e0b2-4983-b70f-ea23d42c4c03`. It was built from
`1613cf8a766f680e28d201341327feb25b52dc5a`; repository validation, all
1,407 tests, clean compilation/package creation, and strict validation passed.

## Release A final runtime qualification

The exact legacy sequence begins with pinned 0.0.114 producer run
`20260904T1013490325299Z-0aa31a4a5af44e3d976e00fedef36a65`
(11/11), continues with current receiver run
`20260904T1052083826042Z-c9c9164de51c467caa8bab191c5bd68c`
(10/10), and finishes with fresh absence run
`20260904T1055286220098Z-7f788486bf0c4a68b4eaf4d4d2bf5d89`
(7/7). It proves eight markerless General race/sex fixtures retain exact stats,
race/provider GUIDs, spent resources, DollData, and appearance, then cleanly
disappear after one current-version cleanup save. The original and restored
feature settings both hash to
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
The producer's post-run cache/settings products were rejected by the old
collector but independently accepted by the repaired 35-case exact-overlay
verifier; no failed wrapper attempt is presented as PASS.

Motion/state-transition run
`20260904T1109218928176Z-483ad4f0c74b4b5aaed745970fb67985`
passed 18/18. Its 1,392,043-byte index (SHA-256
`9941832b6f8b898e9e30037b2bc1a4590e57300244f0286acca7ce8e6c2d2cbc`)
contains 216 motion records/864 views and 64 transition records/256 views for
both sexes of all four races. Native SLA execution, prone/restore,
death/resurrection, Beast Shape II/return, locomotion, turns, firearm reload and
attack, melee, materials, and cleanup passed. The two emitted warnings require
subjective review of optional contact sheets and do not assert visual taste.

Compatibility passed 31 guarded processes and 365 assertions with zero
runtime-result warnings. Each of six required installed profiles ran once with
the module ON and once OFF: standalone, Call of the Wild, Races Unleashed,
Call of the Wild + Favored Class, the minimum valid Tweak or Treat stack, and
the highest-risk combined stack. The ON runs proved exact expected mod and
foreign-catalog preservation, singular contiguous Elemental publication, and
all 53 heritage identities. OFF runs proved all identities remained registered
while no Elemental race remained published. All twelve transactions restored
the exact original 968-entry mod tree, whose SHA-256 is
`376f3a6ce9432789d00bb2c8e314d8dfdb4ca2d12d14a9f709aae16673263999`,
and the relevant settings. The exact 31 run/result/evidence hashes and twelve
transaction hashes are in `ELEMENTAL-RACES-EXPANSION-STATE.json`. Visual
Adjustments was absent and is NOT-RUN. Captured optional-renderer warnings in
the high-risk attribution logs match known non-KMG fingerprints; no new KMG
warning or error was introduced.

## Publication status

Foundation checkpoint
`9c0b7d7bdfe39dd54947c7a37d601cd91db98027` and subsequent Release A and B
implementation/qualification commits through
`ecc65faad142960de6f5b1ea523feaa9ed83dac7` exist locally. The exact mandated
push wrapper refused each checkpoint because
`codex/elemental-races-expansion` is absent from its external branch
allowlist; no bypass was attempted. No pull request has been created. Nothing
has been merged, tagged, or publicly released, and no generated release
package is tracked.

## Release B native-contract audit

Dedicated save-free scenario `observe-elemental-feat-native-contracts` runs
outside the central runner's feature logic and inventories the live enum and
blueprint contracts needed by Release B. Isolated `gunslinger-only` run
`20260904T1428561048826Z-652f2d0914124e21a23e666ceb0f846b` passed 9/9
with no warnings. Runtime-result, audit, and companion evidence SHA-256 values
are respectively
`ecbb01fcbf63c4f0501afcad50d4ddae0bbea0e5b8eee86ab2490a14d3126e71`,
`45744802127f3b227b3aec36fcad85e5ead5fa60d959910e900847cfef023344`,
and `0c92ae86cab445b3de0ab586cce286af992ceb0651f61b3a1760510054e20204`.
Compatibility transaction `compat-20260904T142724Z-1dafdf0d5614` restored the
original mod and settings state exactly.

The installed Kingmaker contract exposes `DirtyTrickBlind` but no Dazzle
variant, so Hydraulic Maneuver can use the genuine native Blind maneuver and
will omit Dazzle. Base Owlcat draconic Wings buff
`08ae1c01155a2184db869e9ebedc758d` grants exactly +3 Dodge AC against melee,
immunity to the `DifficultTerrain` condition, and immunity to
`Ground`-descriptor buffs. The native Airborne feature is a different
conditional attack/damage rule and is not an appropriate player flight fact.
Call of the Wild injects a broader `AddFlying` and maneuver-immunity package
into native wing facts; Release B will construct only the audited base-game
contract so optional-mod presence cannot change Wings of Air mechanics.

The audit also pins native Obscuring Mist buff
`61b312b8f91cc48418768b77cd6dcc02`, Flaming enchantment
`30f90becaaac51f41bf56641966c4121`, summon ability
`107788f47c4481f4db6da06498b28270`, and Small Water Elemental unit
`56372b0a2749c224392a5ee74105c534`. Kingmaker reduces concealment mechanics
to broad descriptors such as `Fog`; source-GUID catalogs are therefore
required for Firesight and Cloud Gazer. The native summon action is linked to
its caster, not directly controllable, and lasts rounds from caster level;
Triton Portal will reuse that native model with project-owned 1d3 count logic.

## Release B identity and publication checkpoint

The active candidate is now `0.0.116-elemental-feats`. Twenty-five stable
manifest identities were added: eleven `BlueprintFeature` feats, nine
`BlueprintAbility` commands/variants, four `BlueprintBuff` states, and one
project-owned `BlueprintWeaponEnchantment`. The complete manifest is 1,784
entries (1,782 active and two reserved); Elemental Races accounts for 147
entries (146 active and one reserved). All 25 identities register
unconditionally under the existing module and no GUID is generated
dynamically.

The blueprint graph binds every prerequisite to the exact project Ifrit,
Oread, Sylph, or Undine `BlueprintRace`, the published level and feat chain,
and the exact active project Hydraulic Push provider where required. The four
Combat feats are published to both universal and Fighter combat selectors;
all eleven are published to the universal selector. Publication is gated by
the existing `elemental-races` setting and uses the established transactional
array helper, including exact-GUID conflict refusal, stable ordering,
idempotence, partial-failure rollback, and reverse-order complete rollback.

Version-aware repository validation and the complete 1,408-case suite pass.
The clean Release build and strict standalone package validator pass. The
untracked 135-entry candidate ZIP is 23,070,990 bytes with SHA-256
`e7567183e40a0499b83d7a96a62e9f6c24aa9aec5b900b649c8f5bd7cb541dd9`.
Its 5,704,192-byte DLL has SHA-256
`c2824d22ca4c351c3edd1959382f3a66f082ed0f40c9365ac7edaa18c50b889b`
and MVID `eef6d06b-3b7d-4712-ae09-257932ea9d39`.

This checkpoint deliberately does not claim Release B PASS. Gameplay
components, command delivery, focused guarded runtime evidence, feat-bearing
save persistence, and Release B compatibility profiles remain pending. The
registered subsidiary blueprints are therefore a buildable identity shell,
not evidence that the feats' player-facing mechanics are complete.

Identity/publication checkpoint commit
`ecc65faad142960de6f5b1ea523feaa9ed83dac7` was followed immediately by the
exact mandated push wrapper. It again refused the user-required branch because
the external allowlist omits `codex/elemental-races-expansion`; no bypass was
attempted.

## Release B mechanics slice 1

Elemental Strike now runs from its one-round owned buff through the native
weapon-damage rule chain. It derives the flat +1 through +5 value from current
total character level, derives fire/acid/electricity/cold from the exact
project parent race, and rejects spell-source, nonweapon, mismatched-owner,
mismatched-target, and replayed damage events. Its Swift command uses the
native ability process and no daily resource.

Wings of Air now uses a project-owned copy of only the audited base Owlcat
draconic-flight mechanics: +3 Dodge AC against melee attacks, difficult-terrain
condition immunity, and Ground-descriptor buff immunity. An owned equipment
controller applies that buff in no/light armor and removes it in medium/heavy
armor. This deliberately excludes optional-mod additions, Angel Wings spell
immunity, prone/trip immunity, custom navigation, teleportation, meshes, and
persistent VFX. Read-only local IL inspection confirmed that Owlcat applies the
AC benefit as a temporary target-stat modifier around `RuleAttackWithWeapon`,
not as a direct `RuleCalculateAC.BonusSource`.

Dedicated save-free exact-source run
`20260904T2000378983332Z-b0699acd82da4d378c3abdded3983858` passed 16/16
with zero warnings and exact request-local cleanup. It proves all four energy
types and five damage breakpoints, canceled/accepted command behavior, one
packet under replay, native resistance, spell/nonweapon exclusions, +3 melee
and +0 ranged AC, exact same-light-armor comparison, medium suppression,
out-of-combat armor-removal restoration, terrain/Ground immunity, neutral
effect admission, and no prone immunity. Result/mechanics/runtime-evidence
SHA-256 values are
`50d215ce18078d93cb404a53198261a48e7b98a1b3347c351de6f9a04df8c9f2`,
`aef0cf2db12fb5a324f64beb9acabebec9fe09e879ea6dc3c29ae7b7a5482845`,
and `7184beafce5eda62e3e854ecb9ffe18c6ec4f3e7e984fd49eaabc02c3ca73d3c`.
The runtime package contains 135 entries and is 23,074,709 bytes, SHA-256
`003c5f0e5c146f75f3ba601f3628659eaa3c5cb6ec0e4ea68540c7073a13e78f`;
its 5,729,792-byte DLL has SHA-256
`3e5def35c361410bdd4583fd9936e54e639ec7e9366ea453ffc592129bd4b7de`
and MVID `60ebeb1c-fa23-497e-9d2f-c99601f363d3`.

This remains a partial Release B checkpoint. Scorching Weapons, Inner Flame,
Blazing Aura, Firesight, Airy Step, Cloud Gazer, Inner Breath, Hydraulic
Maneuver, Triton Portal, feat persistence, compatibility profiles, and final
Release B qualification remain pending; no Release B PASS is claimed.

Mechanics-slice commit `bacc7a0da6400fa4538db6092ee29f3ae28bd514`
was followed immediately by the exact mandated push wrapper. The wrapper
again refused the user-required branch because its external allowlist omits
`codex/elemental-races-expansion`; no bypass was attempted.
