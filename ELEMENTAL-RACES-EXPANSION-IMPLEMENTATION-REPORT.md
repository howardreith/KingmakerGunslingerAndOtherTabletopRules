# Elemental Races expansion implementation report

## Current outcome

**IN PROGRESS - FOUNDATION AND RELEASE A MECHANICAL/COMPATIBILITY GATES PASS;
RELEASE B NATIVE CONTRACTS, FIXED IDENTITIES, BLUEPRINT GRAPH, SELECTOR
PUBLICATION, AND ALL REQUIRED FEAT MECHANICS ARE LOCALLY QUALIFIED;
PERSISTENCE AND RELEASE-WIDE COMPATIBILITY REMAIN IN PROGRESS. THE
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
| B | 0.0.116-elemental-feats | shared, Ifrit, Sylph, and Undine feat catalog | ALL REQUIRED MECHANICS PASS; PERSISTENCE/COMPATIBILITY PENDING |
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
identity/publication checkpoint and all required feat mechanics qualified,
but persistence and release-wide compatibility gates remain incomplete;
Release C is incomplete. Foundation spell
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

## Release B mechanics slice 2

Scorching Weapons is now an exact-race Swift command using native custom
ability delivery. On acceptance it snapshots at most the two distinct weapons
currently occupying the Ifrit's hand slots and admits only manufactured
weapons carrying native `WeaponSubCategory.Metal`. Each qualifying item owns
the stable project enchantment for one round with removal-on-unequip disabled;
the benefit therefore neither transfers to a replacement item nor disappears
from the selected item merely because it is unequipped. Partial application
rolls back exact item facts and the round marker.

The enchantment contributes exactly one fire packet on a successful native
weapon attack. Base Scorching Weapons contributes +1; Inner Flame replaces it
with 1d6 rather than adding another packet. The handler binds the exact
initiator, weapon, target, damage bundle, and item-owned context, and uses weak
damage-rule identity to suppress replay. It declines an attack that already
contains fire weapon damage or whose item has another live native
`WeaponEnergyDamageDice(Fire)` effect, leaving resistance and immunity to the
ordinary damage pipeline.

The passive save component applies one `ModifierDescriptor.Racial` modifier:
+2 for Scorching Weapons or +4 total with Inner Flame. Fire descriptors are
resolved across effective ability parents and direct fire-damage reasons are
recognized. Kingmaker 2.1.7b exposes no `SpellDescriptor.Light`, so an enriched
isolated KMG-only native audit produced an exact immutable seven-spell catalog:
Daylight, Flare, Flare Burst, Searing Light, Sunbeam root and delivery, and
Sunburst. Only native `AbilityType.Spell` entries enter that branch; exact
parent identity handles variants without admitting racial SLAs.

The enriched audit run
`20260904T2056551938471Z-3294e74f2e5a4a78a9baed9cb5f1cac3`
passed 10/10 with zero warnings. Result/audit/runtime-evidence hashes are
`633c00f21f4311858df73c14f43363b4a29c9dfcb53b95f6b321456e1b27c339`,
`34adf61f8bf6194b7504e7cf5a9dba04631236c40ac19d4f8f2563dc61091aef`,
and `2ab4ce76a9e33dbe228b999184f788c1ad5eb6e6e0815a167ba0ba40995d7cf`.
The exact isolated-profile transaction restored successfully.

Dedicated save-free scenario `disposable-elemental-ifrit-feats` exercises the
native command, item slots/enchantments, attack/damage/save rule pipelines,
fire resistance, native Flaming nonstacking control, and exact teardown.
Exact-artifact run
`20260904T2222242573484Z-6e4985f6214a4ffeba5512e353f884f3`
passed 12/12 with zero warnings. It proves canceled versus accepted commands,
two-weapon snapshots, one-round timing, unequip and swap behavior,
metal/nonmetal/natural/empty-hand classification, +1 and replacement 1d6
damage, one application under replay, resistance, nonstacking, +2 and
replacement +4 saves, fire/Light overlap deduplication, and exact cleanup.
Runtime-result, feature-evidence, and runtime-evidence SHA-256 values are
`4c98ea5a6d09c82576ee5e89166aaf98428ac5935da63d32d545d2e364a93b21`,
`0b6bffed286270909cfe296f1b504936f4a0e27c2ae151729fa5397cb8a3fca8`,
and `e57cc1f5f430b19b8ab111a64c9601f8b70edb3a34c0466a2e2f5e605e227b7b`.
Compatibility transaction `compat-20260904T222205Z-322a3ccf16c8` restored
the original staged mod/settings state exactly.

Repository validation, the complete 1,408/1,408 suite, exact-reference clean
Release build, and strict 135-entry package validation passed. The exact
23,088,220-byte package hashes to
`5fff026ebaee0cde153a7f9f5205b57d6e1d93c907214e7e4a7c920f4615db7b`;
its 5,775,872-byte DLL hashes to
`bd9c283ab4600e9bf7b53391a0f3f4a0f2aa5db533890d919328e5c747634884`
with MVID `387f41cc-054d-4982-8563-affb8fdbc5c6`.

This remains a partial Release B checkpoint. Blazing Aura, Firesight, Airy
Step, Cloud Gazer, Inner Breath, Hydraulic Maneuver, Triton Portal,
feat-bearing persistence, compatibility profiles, and final Release B gates
remain pending; no Release B PASS is claimed.

Mechanics-slice commit `768d8c94a4ec6658b71085fb0446243dae2d8d66`
was followed immediately by the exact mandated push wrapper. The wrapper
again refused the required branch because its external allowlist omits
`codex/elemental-races-expansion`; no bypass was attempted.

## Release B mechanics slice 3

Blazing Aura is implemented as an exact-Ifrit Free command available only
while the current Scorching Weapons marker is active and, in turn-based mode,
only on the owner's turn. It creates one six-second owned aura marker. A
narrow postfix on `TurnController.Prepare` delegates to a feature-specific
handler whose weak exact-controller identity claim prevents duplicate damage
within a creature turn. Edge-to-edge adjacency is five feet; the owner is
excluded but friendly creatures are intentionally included. Each qualifying
turn start emits one ordinary 1d6 Fire `RuleDealDamage`, retaining native fire
resistance and immunity.

Firesight adds native Dazzled condition immunity and narrowly adjusts only the
active parent attack's `RuleConcealmentCheck.Success`. Bypass requires the
exact Firesight fact, an ordinary sight-capable attacker, no target
invisibility, at least one exact qualifying fire/smoke source, and no unrelated
concealment source. Project effects use an explicit public semantic marker.
The isolated Kingmaker 2.1.7b inventory found eight native `AddConcealment`
providers and no semantic fire/smoke provider, so the immutable native
Firesight GUID catalog is empty. Blur, displacement, Obscuring Mist, project
fog, invisibility, blindness, darkness, concurrent Blur, and Mirror Image are
not suppressed.

Dedicated save-free exact-artifact run
`20260905T0137340360592Z-e5da1d69116a4fd1837b7ae385ed7bd9`
passed 14/14 in 59,227 ms with no warning, no save access, automatic exit, and
exact unit cleanup. It uses the native ability, attack, concealment, damage,
resistance, and condition-immunity pipelines. Expected concealment failures
are paired with an otherwise identical forced-success control that rolls 19
plus 120 against AC 6; the Mirror Image case reaches the native
`MirrorImage` result after smoke is bypassed. Aura timing is exercised through
the production handler using a unique controller-equivalent token; final
Release B qualification retains a live campaign `TurnController` dispatch
recheck rather than overstating this slice's scope.

Runtime-result, mechanics, runtime-evidence, and compatibility-attribution
SHA-256 values are
`18c16298da519e1558115eaf782090776ee0722256b0314a6baa659ed0cda6c7`,
`9f3c3c1cf60c7a0611747025a9627f4875cda02f42b6f706f262671c4c662abf`,
`03624f0345142a31e84e419f9edf080d4d445e320b6497b171c7e110bc792a69`,
and `2c3de4bc0c8ffdb1dd398e1d6f73a87553e677c2ad625e70a42ebeb906b24cad`.
The evidence collector announced an `evidence-manifest.json` destination, but
that file was absent after collection; no result is attributed to it.

Repository validation, all 1,408 domain/reflection tests, the exact-reference
Release build, and strict package validation passed. The exact 135-entry,
23,101,096-byte runtime package hashes to
`d9f994b53d32118c344f08d75bcc363a7cd1f65afbc5ecbb7be30d21ccadd4a5`.
Its 5,817,344-byte DLL hashes to
`ab72a132295ed802e5e8581e073b243af3610d996c43fceb70f6e4e0dc78c92b`
with MVID `44463f82-dd81-4db6-93e9-c089972efd49`. Deployment manifest
`20260905T0136373727562Z` hashes to
`6ae31813e063f45806f625309b7faea0b6376b8495a538cff837874ed4c45c78`.
Compatibility transaction `compat-20260905T013712Z-0ff1234a1add` restored
the original profile and exact feature settings; its transaction hashes to
`605fe28bfa01b0ec13d866c0bd95fd7848e9c02f9fccfac925efa4934e4e4b8c`.

Implementation commit `7aee2740f7c08f0f9eec1b3efee4eff8e526ce51`
was followed immediately by the exact mandated push wrapper. The wrapper
again refused the required branch because its external allowlist omits
`codex/elemental-races-expansion`; no bypass was attempted.

Release B remains incomplete. Airy Step, Cloud Gazer, Inner Breath, Hydraulic
Maneuver, Triton Portal, feat-bearing persistence, compatibility profiles, and
the complete 0.0.116 gate remain pending.

## Release B mechanics slice 4

Airy Step, Cloud Gazer, and Inner Breath are now mechanically active and
independently qualified. Airy Step owns a local `RuleSavingThrow` component.
It computes one +2 racial bonus for native Electricity descriptors, direct
electricity-damage reasons, or one of eleven exact isolated-runtime-audited Air
ability identities. It traverses effective parents and claims the exact save
event through a weak identity table, so parent/variant and Air/Electricity
overlap remain +2 total. An active Wings of Air fact changes the computation
to +4 total instead of contributing a second modifier.

Kingmaker 2.1.7b has no `SpellDescriptor.Air`. The exact supplement contains
the native Sirocco and Shadow Sirocco abilities, five Air Elemental Whirlwind
abilities, and four air-derived Cyclone forms. These identities came from the
83,023-blueprint KMG-only inventory, not an optional-mod profile. No name or
visual matching enters the predicate.

Cloud Gazer shares only the exact `RuleConcealmentCheck.Success` target with
Firesight and Seeking. Its feature-specific postfix inspects the active parent
attack and every active target concealment provider. It bypasses only exact
native Obscuring Mist or an explicit project Fog/Mist/Cloud marker, and fails
closed if the attacker cannot see, the target is invisible, or any unrelated
concealment source is concurrent. Actual attacks prove Smoke, Blur,
displacement, invisibility, blindness, darkness, concurrent Blur, and Mirror
Image remain effective.

Inner Breath uses a local `RuleApplyBuff` component rather than a global
poison rewrite. Kingmaker exposes no general breathing or inhaled-poison
classification, so the exact native catalog contains only the two audited
poisonous-swamp-gas processing buffs. Project content can opt in with an
explicit respiration-required component. Ordinary poison, Stinking Cloud,
Cloudkill, and unrelated `SwampGasDOT` remain applicable; underwater breathing
has no extra player-facing mechanic.

The enriched native audit run
`20260905T0221048360892Z-e1fec44f33434a60a12d7b2e9168dbcb`
passed 10/10 with no warning in an exactly restored KMG-only transaction.
Audit SHA-256 is
`5d8a0addb2c0bb7aa34ae7c2586c7e4237511d6b94553c0fe0507e78650f1122`.

Dedicated save-free exact-artifact run
`20260905T0258431754839Z-f395da4f5be54cbdad4e980f477f2791`
passed 18/18 in 63,157 ms with no warning, no save access, automatic exit, and
exact unit cleanup. Runtime-result, mechanic-evidence, runtime-evidence, and
compatibility-attribution SHA-256 values are
`ba407df98928b3b475b52e34c36f44b776603c17b8fd5da71c8a833e3afaadfd`,
`9780e37b9da8104912f98bdf9521c4168c575892ef94a3e1d46a778dbe1ac475`,
`acd7c924ad126a9254c16696a61739cd3598336e29abff7e6c2db028f37d3517`,
and `7f0c0767b408d4f77ca79412c949685317c5983fd08e79eff75121f6510e55f6`.
The announced `evidence-manifest.json` was absent after collection and carries
no claim.

Repository validation, all 1,408 domain/reflection tests, the exact-reference
clean Release build, and strict package validation passed. The exact
135-entry, 23,113,344-byte runtime package hashes to
`9db45f57693133b0cc80fbba89e32794236c97278a39d57115b37ca311343e5b`.
Its 5,855,744-byte DLL hashes to
`995d059605df2b500507e28bce565c4a90571a837012ae6d107cd106ba819cdd`
with MVID `0a5b2f43-acb4-4903-a921-cacfd1222cbc`. Deployment manifest
`20260905T0258430802149Z` hashes to
`581583c0947f8cc00da4b1116aa6c981490bcd826948630e7081353fa8d5bf0d`.
Compatibility transaction `compat-20260905T025703Z-fff4ca9f0bb5` restored
the original mod/settings state exactly and hashes to
`bf3f80e148c303f3efc7446739489f424e92deecea9425a7d6b47c1a91f92444`.

Implementation commit `f514c3dbb31c8f2f705a5e3dea1237e8d9eeebc5`
was followed immediately by the exact mandated push wrapper. The wrapper
again refused the required branch because its external allowlist omits
`codex/elemental-races-expansion`; no bypass was attempted.

Release B remains incomplete. Feat-bearing persistence, the complete
compatibility matrix, and final 0.0.116 gates remain pending.

## Release B mechanics slice 5

Hydraulic Maneuver and Triton Portal are mechanically active and independently
qualified. Hydraulic Maneuver is an ability-variant parent whose four
manifest-backed children invoke native `ContextActionCombatManeuver` for Bull
Rush, Disarm, Trip, or Dirty Trick (blind). Each variant substitutes current
total character level for base attack bonus and adds the current best
Intelligence, Wisdom, or Charisma modifier. The feat-local availability
component requires the exact project Undine race, active racial Hydraulic Push
provider and ability, and a positive amount of the existing resource. The
existing commit action spends that shared use exactly once after acceptance.

The isolated runtime inventory exposes a genuine `DirtyTrickBlind` maneuver
but no `DirtyTrickDazzle`. Blind is therefore implemented through the native
path and dazzle remains an explicit engine omission. No spell or unrelated
condition simulates it. The guarded scenario gives Disarm a held manufactured
weapon and proves each variant creates one native `RuleCombatManeuver`. A
temporary Wisdom increase changes Trip from level 7 + modifier 4 to level 7 +
modifier 7. Cancellation retains the resource at 1, acceptance changes 1 to
0, zero uses block both feat paths, and ordinary rest restores exactly one.

Native Trip checks `target.View.IsGetUp` before constructing its rule. A newly
spawned live-scene fixture reports `IsGetUp` while its spawn animation runs,
so the request-local test patch suppresses only that false-positive state for
its named disposable target; production behavior is not patched. Native
combat-maneuver immunity then early-returns with CMB and d20 both zero and no
prone effect. Kingmaker's result object retains its default `Success` value on
that early return, so qualification keys on the actual zero-valued native
event and unchanged target state rather than inventing another result.

Triton Portal clones only the exact native Small Water Elemental summon
component/action graph. Mutable components and actions are copied into the
project-owned ability while native blueprint references remain references.
The owned spawn count is 1d3. The full-round SpellLike point command uses the
same Hydraulic Push resource and retains ordinary native summon duration,
allied non-hostile faction, source linkage, lifecycle, death, and cleanup.
An owned checker rejects nonfinite or non-ground points through
`AbilityData.CanTarget`, Kingmaker's player-facing point-selection gate. A raw
synthetic `UnitUseAbility.CanStart` assumes that upstream validation has
already occurred and is retained as a separate diagnostic observation.
Expanded Summoning is neither queried nor referenced.

Final KMG-only run
`20260905T0550526363250Z-a4c7158ae8e74168b36082c6c6e6e3a0`
passed 13/13 in 61,003 ms with zero runtime-result warnings, no save access,
automatic exit, and exact request-local unit, object, area, item, faction, and
player-cache cleanup. Runtime-result, mechanic-evidence, runtime-evidence,
runtime-summary, and compatibility-attribution SHA-256 values are respectively
`0bcc4e6bf43472c1653f9fe98c24f825ec9f860b4bcd0af0df94b7d3e442045a`,
`6797701ef2d806136807661c70130075bb5eb06270dcaa29eb98815d861114c4`,
`af88dbb52734d7ba00f96b57b3cc03449708fb08a23e7fb5bbc66ea719229c27`,
`ba67b9af0022a2fb069f97c18ae28981d600fad834d1094d8598cb0ba6d5ac76`,
and `b8eea82306456e2cc41b23726df9a22d88fc309ff37650da239735fed1660a1a`.
The collector announced `evidence-manifest.json`, but it was absent after
collection and carries no claim.

Repository validation, all 1,408 domain/reflection tests, the exact-reference
clean Release build, and strict package validation passed. The 135-entry,
23,129,428-byte runtime package hashes to
`1de04caba357580b7f5dbffb422b65b8fde399fed8f90ddd568da83ba40e8804`.
Its 5,912,064-byte DLL hashes to
`89cd4fe6f96a339a54102441b0958ff970b7ce258e90884ed46eda3e8c02bf90`
with MVID `4eb29429-0ece-42e2-bf6a-cd9a8e9c967c`. Source-state SHA-256 is
`000f158a2e1235e7ad37d80974639ab9abfe4eb0b791ceee19fbe9c4816a3382`;
deployment manifest `20260905T0550525413468Z` hashes to
`8370bc4d68592829ca00bed9f6ad4404483d55fb86bfb1daac74198a7a280671`.

Compatibility transaction `compat-20260905T054922Z-c2398e3ecad3`
completed `Restored` with verification true and hashes to
`07702b939756bb5d0bc5c3fae4e07f8f89a1cb9df21f57b352293bb02270e6af`.
`FeatureModules.json` returned byte-for-byte to SHA-256
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
The six preceding diagnostic results and their exactly restored transactions
remain enumerated in the mission state; none is rewritten as a pass.

Implementation commit `b70ad97d25ff2c25a41dd544f2e6a7870c6bd12d`
was followed immediately by the exact mandated push wrapper. The wrapper
again refused the required branch because its external allowlist omits
`codex/elemental-races-expansion`; no bypass was attempted.

All required Release B feat mechanics now have isolated guarded proof, but
Release B remains incomplete. Feat-bearing save persistence, module-OFF
hydration, the full installed compatibility matrix, integrated runtime
regression, and final 0.0.116 gates remain pending.

## Release B feat persistence qualification

Release B's high-risk temporary state is now save-backed without introducing a
new blueprint identity. A schema-versioned project `UnitPart` stores absolute
game-time end ticks for Elemental Strike and Scorching Weapons plus at most two
direct exact `ItemEntityWeapon` references. Its pure decision policy clears
expired, corrupt, dead, foreign, or prerequisite-less state; waits for native
owned-item hydration; and restores only the original qualifying items. It does
not restore a spent daily use, extend a duration, or transfer an enchantment to
replacement equipment. An exact `UnitEntityData.PostLoad` patch only schedules
reconciliation when that project part exists.

The existing elemental-race persistence harness now overlays Release B state
onto all 24 race/sex/heritage fixtures. Ifrit female Sunsoul receives two native
shortswords and activates Scorching Weapons through the real command. Undine
female Rimesoul activates Elemental Strike through the real command. Sylph
fixtures retain Wings of Air. Module-OFF load preserves registered feat facts,
abilities, resource amounts, absolute timestamps, the original item references,
and the exact two enchantments. Level-up does not refill resources or extend
the transient effects. Explicit cleanup removes only the short effects; normal
rest/resource handling, module-ON restoration, heritage respec cleanup, fixture
deletion, and final fresh-process absence all pass.

The first module-OFF attempt failed rather than yielding an ambiguous pass. The
prepare evidence stored exactly six seconds of remaining game time, but
Kingmaker rejected `Game.Instance.IsPaused = true` during its after-load
callback. The older loader treated the attempted assignment as success and
continued through earlier fixtures until native time expired the command-
created effects. A focused regression failed first. The loader now retries at
its guarded update boundary and blocks all fingerprint and feature inspection
until the engine observably accepts the pause. The final run recorded pending
attempts followed by actual pause at 13:07:22.461Z and release only after
verification at 13:08:08.844Z.

Repository validation and 1,408/1,408 domain/reflection cases pass. The clean
Release build and strict 135-entry package validation pass. The
23,142,818-byte package hashes to
`5d9a6112a8f3c57d0b97c1bb414705a19ddf6dadbe02acdc279617223cf6ece9`;
the 5,948,928-byte DLL hashes to
`16b1af2f8db5bc3eafd92c96adfbb19b91004cb4cc6236fc31d64a2178d835cb`
with MVID `880bc467-c3a8-4504-9df1-33cd63b06703`. Deployment manifest
`20260905T1257423901840Z` hashes to
`4ff9b605172ed4871af8b3911a9357811ebc2d3789ece1871294d7924c747ac5`.

Prepare, module-OFF, module-restored, and final-absence runs are respectively
`20260905T1301245986745Z-elemental-race-persistence-prepare`,
`20260905T1305075446941Z-elemental-race-module-disabled-persistence`,
`20260905T1308241102853Z-elemental-race-module-restored-persistence`, and
`20260905T1313027193036Z-elemental-race-persistence-verify-absent`. Their
runtime-evidence SHA-256 values are respectively
`08dc53d0de0b4dc30ec6bdf2e35b2fb79e32e386702c30984d9fcfe11509c668`,
`eb408a67b66c1b780192a2b3ead8fccde73a7ef46fa7c44393681af83a5dfb53`,
`e0e0422a04fab508f5dfa37d1b81c6dba364d7bec1fbf63c3592875e94bba849`,
and `e4596d8d11f7a748e3be12ee5a2e3b8456ae010a7eba16515a9a0eb08ffc7511`.
The same artifact then re-passed Elemental Strike/Wings 16/16 and
Scorching/Inner 12/12. `FeatureModules.json` restored byte-for-byte to SHA-256
`a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
The named disposable save is absent after cleanup.

This is a persistence gate PASS, not a Release B PASS. The complete installed
compatibility matrix, integrated regression, final documentation, and final
0.0.116 artifact qualification remain pending.

Commit `a4a377cc73585776ed24d40d77d3cebbe20ba72b` records this
checkpoint. The exact mandated push wrapper was run immediately and refused
the required branch because its external allowlist still omits
`codex/elemental-races-expansion`; no alternate push path was used.
