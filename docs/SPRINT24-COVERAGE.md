# Sprint 24 Deinonychus and Tyrannosaurus coverage

Status: **IN PROGRESS — DEINONYCHUS AND RTWP TYRANNOSAURUS PASS; TB CONTINUATION OBSERVER CORRECTION FOCUSED PASS** on
October 14, 2026.
Both creature keys remain unpublished until the required mechanics, art,
runtime, persistence, route, and cleanup evidence passes. Sprint 25 was not
started.

## Frozen authority and interpretations

The original expanded-summoning charter pages 21–22 define only two Sprint 24
creatures: Deinonychus at Summon Monster/Nature's Ally IV and Tyrannosaurus at
VII. The adult Pathfinder First Edition monster entries and universal monster
rules provide exact mechanics. No Pathfinder 2e, D&D 3.5, young/advanced,
companion, polymorph, mount, or flavor-prose mechanics are in scope.

Primary sources:

- [Deinonychus](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Deinonychus)
- [Tyrannosaurus](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Tyrannosaurus)
- [Bestiary dinosaurs](https://legacy.aonprd.com/bestiary/dinosaur.html)
- [Universal monster rules](https://legacy.aonprd.com/bestiary/universalMonsterRules.html)
- [Summon Monster IV](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Monster%204)
- [Summon Nature's Ally VII](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Nature%27s%20Ally%207)
- [Bleeding Critical](https://www.aonprd.com/FeatDisplay.aspx?ItemName=Bleeding%20Critical)

Deinonychus derivation is exact: 4d8 averages 18, plus 4 × Constitution +4
gives 34 hp; BAB +3 and Strength +2 give +5 primary attacks and +2 primary
damage. Its one foreclaws attack is secondary, so the attack is +0 after the
-5 secondary penalty and deals half Strength, +1. Four purchased ranks split
2/2 between Perception and Stealth; each ranked class skill gains +3, producing
Perception 2 ranks +3 class +1 Wisdom +8 racial = +14 and Stealth 2 ranks +3
class +2 Dexterity +8 racial = +15. Acrobatics/Mobility is +10 from Dexterity
+2 and racial +8; the separate 60-foot-speed jump adjustment yields printed
+22 only for jumping. CMD is 10 +3 BAB +2 Strength +2 Dexterity = 17, with no
borrowed quadruped trip defense.

Tyrannosaurus derivation is exact: 18d8 averages 81, plus 18 × Constitution +4
gives 153 hp; BAB +13, Strength +11, and Gargantuan attack-size -4 give bite
+20. Powerful Bite requires a signed final Strength contribution of `2m`, so
Strength 32 (`m = +11`) contributes +22. Installed Kingmaker 2.1.7b was already
shown by the Sprint 23 IL record to contribute `floor(1.5m)` for a positive
sole natural weapon and `m` for zero or a penalty. The scoped correction is
therefore `2m - native(m)`: +5, +5, +6, +6 for modifiers +9, +10, +11, +12;
zero stays zero; penalties receive the additional signed penalty needed to
reach exactly twice the modifier. It never doubles dice, enhancement, added
damage, critical riders, or any other damage bundle. Perception is 18 ranks +3
class +2 Wisdom +8 racial +6 Skill Focus = +37.

Grab and Swallow Whole have distinct gates. A Gargantuan Tyrannosaurus may grab
a Gargantuan or smaller target after an exact bite hit. It may swallow only a
Huge or smaller target that was held in its mouth at the start of the later
qualifying turn/round, after a new successful grapple check. The successful
swallow bite remains distinct from periodic 2d8+11 interior damage. There is
no immediate/Fast Swallow path.

## Publication contract

| Family | Creature | Base tier | Roots | Quantities through IX | Candidate state |
| --- | --- | ---: | ---: | --- | --- |
| Monster | Deinonychus | IV | 6 | one; 1d3; then 1d4+1 ×4 | Suppressed |
| Nature's Ally | Deinonychus | IV | 6 | one; 1d3; then 1d4+1 ×4 | Suppressed |
| Monster | Tyrannosaurus | VII | 3 | one; 1d3; 1d4+1 | Suppressed |
| Nature's Ally | Tyrannosaurus | VII | 3 | one; 1d3; 1d4+1 | Suppressed |

The candidate adds 18 logical roots, excluding Monster template execution
children. With no unrelated roster change the public totals derive to 107
project creatures; Monster 95 creatures/543 placements; Nature's Ally 95/550;
1,093 generated placements; 29 retained native wrappers; and 1,122 visible
choices. The hidden candidate keeps the current 1,075 published generated
placements and 1,104 visible choices. Monster routes use only the existing
celestial/fiendish policy; Nature's Ally routes remain untemplated.

## Requirement → carrier → proof map

| Requirement | Intended carrier/seam | Required proof | State |
| --- | --- | --- | --- |
| Stable, save-compatible identities | Append-only identity catalog, blueprint ledger, normal registration | GUID/type/collision tests; module OFF load/refusal; ON attach once | 48 exact identities appended; offline validators PASS; native load proof pending |
| Deinonychus four attacks | Primary talon plus talon and bite as additional primary limbs; one foreclaws secondary limb | Live ordinary full attack has exactly four rolls and +5/+5/+5/+0 with +2/+2/+2/+1 Strength | Implemented and domain-pinned; live proof pending |
| Pounce only on a legal charge | Exact native Pounce fact; ordinary body routine; no project replay | RTWP/TB legal, illegal, aborted, AoO, ordinary single/full, death/interruption, next-command controls | Closed native observer implemented; live execution pending |
| Deinonychus skills/CMD | Exact one-time 0/2/2 land ranks plus narrow racial modifiers; no four-leg fact | Live ranks, abilities, class bonuses, racial bonuses, ordinary Mobility versus jump-only distinction, CMD 17 | Implemented and derived offline; live proof pending |
| Tyrannosaurus one bite | Owned 4d6 bite weapon with live signed Powerful Bite delta | Live +9/+10/+11/+12, zero/penalty, size, buff/debuff, enhancement, added damage, critical multiplication | Implemented; signed arithmetic tests PASS; native damage proof pending |
| Critical suite | Exact native Improved Critical (bite) and Critical Focus facts; project-owned bite-only confirmed-critical trigger applying native `Bleed2d6Buff` | 19–20 threat, ordinary confirmation, confirmed piercing/slashing bleed, heal/end controls, negative attacks | Exact event gate and closed confirmation/cadence/healing observer implemented; live execution pending |
| Bite-only Grab | Existing summon grapple lifecycle, exact primary-limb identity, +4 maneuver | Hit/miss, same-size legal, oversized illegal, immunity/natural 1/20, duplicate callback, independent owners | Implemented; released shared-domain controls PASS; native proof pending |
| Later Swallow Whole | Existing held-since-round-start and maintain seam; separate -1 size delta | Same-size grab cannot swallow; later Huge target check succeeds; immediate/failed/wrong-limb controls | Implemented; distinct size-gate tests PASS; native proof pending |
| Periodic stomach damage | Tyrannosaurus-owned 2d8+11 payload, not Powerful Bite or donor stomach | One application per intended round in RTWP/TB; attribution, escape, source loss, simultaneous predators/victims | Owned payload and independent-pair observer implemented; live cadence/attribution proof pending |
| Hold/swallow lifecycle | Existing ownership parts with narrow reload distinction | Mouth holds reset safely; already-swallowed native state is not indiscriminately erased; dismissal/death/expiry/escape cleanup | Targeted prepare/load/cleanup/absence observer owns mouth-hold, swallowed and bleed receipts; live persistence proof pending |
| Original theropod presentation | Existing original-asset/export/attach infrastructure on the qualified Free Hands Lizardfolk skeleton | Offline clay/silhouette/textured/unlit/camera/extreme poses; live contacts, speed, reach, navigation, crowding, fade/fallback/module lifecycle | Original icons, procedural bodies, paint, exact renderer selection, rollback and package owners implemented; 20-frame final offline review and focused checks PASS; live proof pending |
| All 18 roots | Existing catalog generator and real parent spell paths | Every root, quantity, slot, duration, Monster templates, Nature's Ally untemplated behavior | Registered and suppressed; exact route/quantity/template tests PASS; native parent-path proof pending |
| Affected old consumers | Existing pouncer, Dire Crocodile/Purple Worm, visual owner, Triceratops if shared Strength seam changes | Narrow fresh native regressions, with historical broad evidence reused only after dependency audit | Observer includes Leopard, released swallowers, donor isolation and Triceratops +7/+9; live execution pending |

## Bounded native census decision

The single closed, save-free census passed on Kingmaker 2.1.7b. The selected
Free Hands Lizardfolk rig has separate upper/lower legs, feet and toes; four
serial tail joints; a two-joint jaw; both upper/lower arms and palms; locomotion;
and separate claw and bite actions whose command-contact events occur at 0.435/
0.530 and 0.597 seconds. That is a credible shared skeleton for original
Deinonychus and Tyrannosaurus geometry. The Troll negative control has only two
tail joints and is not used as an authoring skeleton. Size-specific cadence,
contact, reach, collision, navigation and crowding remain runtime proof items;
the donor decision does not pre-approve either final presentation.

The exact released Pounce, Critical Focus, Improved Critical (bite), Improved
Initiative, Iron Will, Skill Focus (Perception), Diehard, Endurance and bleed
facts resolved. No native blueprint named Bleeding Critical exists. Native
`Bleed2d6Buff` (`16249b8075ab8684ca105a78a047a5ef`) already owns the standard
2d6 bleed payload, healing removal and combat-end cleanup. The implementation
therefore owns only the Tyrannosaurus/bite/confirmed-critical predicate and
applies that native buff; it does not duplicate bleed timing or cleanup.

Local-only evidence is
`runtime-evidence/20261014T0546181325834Z-observe-expanded-summoning-sprint24-theropod-census`.
The runtime-result SHA-256 is
`893d72987c0154877571378d72c7319f692f5131c77f788d3b2ac68daa761b35`
and the census SHA-256 is
`727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4`.

## Explicit exclusions and limits

Deinonychus has no Rake, Rend, Grab, Swallow Whole, poison, fear, or Powerful
Charge. Tyrannosaurus has no claws, tail attack, Pounce, trample, roar/fear,
Power Attack, or automatic critical confirmation. Animation cannot generate
gameplay events or move logical position/collision.

The existing accepted limitations are bounded, not expanded:

- `ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD` applies to active mouth holds.
- `SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED` leaves printed interior AC 17/hp 15
  in the fidelity record without claiming an attackable stomach implementation.
- `PASSIVE_CREATURE_SENSES_UNMODELED` and `RUN_NATIVE_SUMMON_AI_DISPOSITION`
  may be reused only after their exact existing boundaries are reconfirmed.

`HumanReview` remains `NOT_PERFORMED_NONBLOCKING` unless the owner actually
reviews the finished art.

## Hidden mechanics checkpoint

The hidden candidate Release production assembly compiles. The Sprint 24 source
validator passes at the hidden stage with 3,149 ledger rows, the manifest
generator reports foundation 2,149 / active 3,146 / reserved 3, and its ten
corruption fixtures pass. After the native-review and persistence observers
were added, the named Sprint 24 filter passes 13/13 tests with production
compilation. The guarded request preflight passes 588 checks; targeted
persistence parsing passes 18 exact round trips and 168 fail-closed cases;
launcher routing passes 30 valid and 17 rejected cases; and the installed-stack
fixture passes 30 restoration predicates. A
second affected-seam filter passes 14/14, including the released shared grapple
lifecycle, universal grab-size rules, and Dire Crocodile swallow-number control.
These are development checks, not native qualification: both suppression keys
remain present. The first broad Sprint candidate attempt passed repository
validation but stopped in the complete domain suite with 40 stale historical
count/tail assertions out of 2,607 tests. It created no package, deployment,
lease, game process, or save access; every Sprint 24 behavior assertion passed.
The correction centralized the 48-identity/18-route append contract and kept
the historical tests scoped to their own released creatures. A complete-domain
correction run reduced the retained failures to four missed append assertions;
the final focused production build passes all four. The generated weapon audit
now covers 134/134 active custom weapons, including both theropod weapons, with
31 summoning-only exclusions. A fresh complete Sprint gate remains required.

That fresh gate subsequently passed repository validation and all 2,607 domain
tests in 94.4 seconds, then produced the exact-reference compile candidate. It
stopped before the standalone ZIP because the deterministic packager still
pinned the released 364-file total while the reviewed Sprint 24 append stages
370 files: four original theropod body/paint files plus two original icons.
No standalone candidate was accepted or deployed. The correction raises only
the strict package inventory to 368 without the optional soundbank pair and 370
with it; the current Sprint 24 test owns that total while historical tests now
prove their assets enter the strict inventory without copying a future global
count. Package-specific validation is next, followed by a complete Sprint gate
on the committed correction.

The exact seven affected package/source assertions now pass with production
compilation. A disposable local package then assembled and passed the strict
allowlist with exactly 370 entries: four theropod body/paint files and two
theropod icons are present once each. Its SHA-256 is
`697e6bcd5ba57f8dfaa7c8adeafe5849ebde34cf050f32aadfdbefd9298d1aa6`;
it is correction evidence only, not the frozen native candidate and was not
deployed.

The committed hidden candidate at
`ed25e0b56a4e4fc21f6bb39c05f43a5cff7fef4b` subsequently passed one complete
Sprint gate: repository validation, 2,607/2,607 domain tests in 93.5 seconds,
exact-reference compilation, and a strict 370-entry package. Total gate time was
189.4 seconds. The frozen ZIP SHA-256 was
`a917ba3562ff39c44a0497271243bca12ba1e4b680c67b4a7f804112f58d7e13`;
the DLL SHA-256 was
`2b085cd688b584ac7e5fa9a652060d6ac19d4b2a2b17f5d178e1fd26d8d55fdf`,
MVID `e0300b83-d05a-4b3a-a872-cdbdea4a37f8`, with source fingerprint
`22b4483a0367f63d87efac9135739ac10c9b0c5d18dface93713b9b6806328c6`.
That candidate was valid for native execution but is now superseded by the
bounded correction below.

The first guarded `rtwp-foundation` phase preserved 22 PASS and four FAIL
assertions over 103.583 seconds. All 18 hidden roots executed through the real
parent spell path with legal quantities and existing Monster/Ally template
policy. The four failures were precise: the visual-only Lizardfolk donor's
Human race supplied +2 Constitution and Medium combat-size arithmetic; the
primary-hand talon received native sole-natural-weapon 1.5x Strength; the
standalone weapon-stat probe's `SecondaryWeapon` diagnostic stayed false even
though the foreclaws occupied the secondary limb and produced the correct +0/
1d4+1 result; and native fade updates re-enabled `_ammunition04` after initial
suppression. Both original bodies, paint, selection surfaces and size states
attached; the failures do not claim mechanical or visual qualification.

The correction removes only the playable donor race from these two reconstructed
monster blueprints, removes only the excess lead-talon Strength term on the exact
Deinonychus/weapon, records but no longer elevates the misleading calculation
flag over equipped-limb and outcome evidence, and owns accessory suppression
through a per-view LateUpdate component included in exact resource receipts and
released before donor restoration. Production compilation and 13/13 Sprint 24,
6/6 shared resource-authority, and 1/1 instance-owned-resource tests pass.

The exact corrected commit
`905c1f7e926924e5ce0b636df2875fc20a2603ba` then passed one fresh complete
Sprint gate: 2,607/2,607 tests in 91.7 seconds and a strict 370-entry package in
188.9 seconds total. ZIP SHA-256 is
`fbab7c4126d785acef8885e0d0b2d17517a87af0b7dfe5d0614c637d8d7f3807`;
DLL SHA-256 is
`4de957a8bad495ff94c8e04d70b285102871578b59de2013269fda6a7e3b9422`,
MVID `2534e2fd-4c1d-4758-9708-4cc14e5493ba`, and source fingerprint
`feaa8f8ca0eb4d97331725c89ea41cc368a50ec0b808f2cdb7eabb1ff047ea75`.
Its changed `rtwp-foundation` reproduction passed 26/26 assertions in 95.127
seconds: both adult profiles, original bodies, selection/accessory ownership and
all 18 hidden real-parent routes are now exact.

The following `rtwp-deinonychus` phase retained 5 PASS and 5 FAIL assertions.
It proved the exact four-roll ordinary routine and single-roll AoO, while
preserving five observer/setup defects: the quiet-unit helper had inflated the
profile HP before readback; body/selection were sampled before the proven
12-frame settle; the nominal single command omitted native `IsSingleAttack`;
Pounce's exact four charge rolls and 11.638 m movement were failed merely because
one legitimate roll was a natural 1; and the illegal-charge check elevated a
generic `UnitUseAbility.CanStart` flag over the exact false target predicate and
zero emitted attacks/state. The batched correction uses the normal own-tier
profile path, waits for the same settle, explicitly selects native single attack,
controls and records d20 preconditions, and asserts Pounce routine shape rather
than all-hit luck. The named Sprint 24 focused gate passes 13/13. Because this
observer is compiled into the DLL, one fresh complete Sprint gate is required
before rerunning only the changed native phase.

The exact observer-correction commit
`aa048f1db9d279ea6185ec7ab1b3534eac93ff2a` passed that complete Sprint gate:
2,607/2,607 tests in 91.5 seconds and a strict 370-entry package in 186.1
seconds total. ZIP SHA-256 is
`68ce7f2d0699cc07f61903ca460b78ffd34539784f2543eba7ab6f810988fbd1`;
DLL SHA-256 is
`e69ae58067b5f69a6b6b501a2cb8fbba3c7125f688c566b49520f419ebbcb3f8`,
MVID `9228e5b3-1edd-44c7-9591-14520fe31c6f`, and source fingerprint
`78ad09ba48adfaf2b902c8fbd12bd8f85aa793bbb2e37bfdf8cee3f03f98eda5`.
The corrected `rtwp-deinonychus` reproduction then passed all 10 assertions:
adult profile/body, exact ordinary full and single routines, 11.703 m native
Pounce with four charge rolls, illegal/aborted and AoO controls, and teardown.

The first `turn-based-deinonychus` phase preserved 5 PASS and 3 FAIL assertions
in 194.520 seconds. The adult profile and body passed, but both ordinary attack
commands remained unstarted with zero rolls because the owner had not been
admitted to native turn order. The later charge route probe then reached a
native `AbilityCustomCharge.CanTarget` null path because TB mode lacked a valid
current turn. The qualified Sprint 22/23 sequence already defines the bounded
fix: begin in RTWP, enroll only request-owned actors through the supported mode
toggle, wait for the owner turn, and perform route survey in RTWP before
re-enrolling for the actual TB charge. That correction is now in the closed
observer and its carrier name is domain-pinned; production compilation and all
13 Sprint 24 focused tests pass.

The exact turn-controller commit
`eea076088c695ebb7cf48932f452f59c819e61e0` then passed its fresh complete
Sprint gate: 2,607/2,607 tests in 90.4 seconds and a strict 370-entry package in
186.2 seconds total. ZIP SHA-256 is
`4156261f122646fd6ab952916c8358dbbecb150981b17be958ec3a6e3af6e4ab`;
DLL SHA-256 is
`e41019388e5af11bb11020bb4d681b082443b0d5e55eb4f371870fd93ff80549`,
MVID `f70e80a8-1e81-4d0c-be9e-651422b156de`, and source fingerprint
`5b5c3ae05e8f9e11a9b804231af4def80bf4e7723b1472d95895328a61795c20`.

Its changed TB reproduction passed 9/11 assertions in 100.638 seconds. Native
turn admission, adult profile/body, both ordinary routines and the 11.097 m
four-roll Pounce passed. The rejected too-close command emitted zero attacks;
the AoO remained one non-charge talon. Both negative rows failed solely because
the preceding legal charge marker was still present. The shared observer helper
can return while the native controller is transiently null between turns, so
those checks had not actually reached the owner's next turn. The correction
kept the native expiry requirement, waited for the actual fresh owner turn, and
recorded marker state immediately after Pounce and before/after the illegal
command. It never removed the marker.

The exact fresh-turn commit
`3ed00e74c5602b6a28609a063481e6cb81b100db` passed the next complete Sprint
gate: 2,607/2,607 tests in 108.8 seconds and a strict 370-entry package in 214.2
seconds total. ZIP SHA-256 is
`923c2df33004b9fedcd2b23dcf514b7192b4ac407718d9ae54d5fae7f932b712`;
DLL SHA-256 is
`4d478541a126908e349121c6451f3a19fea9dc4468de143a63e51c7a0c8d7854`,
MVID `e0ce5b26-0e79-4a59-a71b-f0726d45196e`, and source fingerprint
`ba154235e334df4df69bbf1a2f65a062f3ab88bd186cd7d4f2e9c1706cccdcd6`.
The corrected TB reproduction then passed 11/11 in 111.715 seconds: the marker
was present immediately after Pounce, expired at the owner's actual fresh turn
after 57 frames, remained absent through the rejected command, and the AoO was
one non-charge talon.

The first `rtwp-tyrannosaurus` phase retained 8 PASS and 2 FAIL assertions in
91.386 seconds. The live adult profile, exact one-bite routine, every signed
Powerful Bite row (+9/+10/+11/+12/zero/penalties), independent simultaneous
predators, three 2d8+11 stomach rounds, escape and cleanup passed. The critical
row also proved the 19–20 threat carrier, +4 Critical Focus, false native
auto-confirm flag, confirmed ×2 physical damage, one native 2d6 bleed tick and
healing removal. Its unconfirmed control used +100 fixture attack, however, so
a natural-1 confirmation still beat target AC numerically. The same real bite
probes were permitted to establish an unrelated Grab, occupying the mouth
before the same-size test; the later independent Huge-target Grab/Swallow path
passed. The narrow correction set target AC between the natural-19 threat and
natural-1 confirmation totals, suppressed only unrelated maneuver side effects
during the critical fixture, and asserted clean mouth state before Grab.
Product critical, Grab and Swallow rules did not change.

The exact correction commit
`c95a8c03ec644c81f5c0bfe2a0faaff3f7215ded` passed the next complete Sprint
gate: 2,607/2,607 tests in 94.4 seconds and a strict 370-entry package in 190.4
seconds total. ZIP SHA-256 is
`e3c07a543796035e84f4d65a5783db72a42348683c931c442ba77742e104fc89`;
DLL SHA-256 is
`d325dbae5437080e8efd3661e8ff1385b424dac70da66c1ba87a4765f1676b30`,
MVID `5054599d-6a6d-442b-b323-38ca37dd7a26`, and source fingerprint
`665b32f6805cd8132d3bfccba307c47cd38903cb851703e418b4c66756b6bdab`.
The changed RTWP reproduction then passed 10/10 in 91.283 seconds, including
the natural-19/natural-1 non-confirm control at AC 30 and clean Grab isolation.

The first `turn-based-tyrannosaurus` phase retained 10 PASS and 1 FAIL
assertion in 91.969 seconds. Turn admission, one exact bite, all signed
Powerful Bite cases, the critical suite, same-size Grab without swallow,
later Huge-target swallow, three periodic rounds, escape, and one independent
2d8+11 packet for each of two simultaneous predator/victim pairs passed.
Releasing the first victim preserved the second victim's swallowed part and
one-victim owner list, but the continuation probe emitted no second packet.
That probe reused its pre-release Buff reference and due-time snapshot. The
narrow correction reacquires the live second-victim Buff after release, uses
the current native due boundary, and separately asserts that release neither
ticks nor relinks the surviving pair. Product swallow mechanics remain
unchanged. Production compilation and all 13 named Sprint 24 focused tests
pass; a new exact Sprint candidate is required before rerunning only the failed
TB Tyrannosaurus phase.

Earlier failed development checks, the candidate/correction-suite failures,
the guarded native foundation failure, both first Deinonychus attempts, the
inter-turn reproduction and both first Tyrannosaurus phases remain recorded in
the machine-readable ledger:
the first stage allowlist omitted the new production seams, the first provenance
check found the expected blueprint-ledger hash change, and the first production
compile lacked the `ContextDiceValue` namespace import. The first observer
focused build omitted one test-project namespace import, and its next run found
that the closed reflection coverage expected an explicit Bleeding Critical
carrier name. Each failed before any deployment and passed after a narrow
correction; none was discarded or rerolled unchanged.

## Original art and attachment checkpoint

The two icon paintings were generated with the repository's approved image
generation workflow, then exported through the established deterministic icon
pipeline. Their runtime SHA-256 values are
`d7c15ef813f4920efac88a5b1bcd194e1f872af98c5d619dd60758e96aa1dd36`
(Deinonychus) and
`e80aeaa47e293e74fd6bd589670ff864f4c3375d01f1a8c6c7ca3eb25eb50698`
(Tyrannosaurus). Exact prompts and generated-source hashes are recorded in
`assets-source/original-icons/expanded-summoning/SPRINT24-SOURCES.md`.

The deterministic Blender/Python source creates original biped meshes and
paintings from the geometry-free census frame. The current shipped body hashes
are `eb1a06fb8933d603bfb89655df82d6e240bbf861bbe98d4134c9e25df4ed16ab`
and `308a9a9950227b538ce9e7c068e090bf9e81a95fb6a573e8e8a0a4ea62a9ae87`;
the albedo hashes are
`4e0414763bc70122a4645402724c27c6dadd72fd9341ff1b25fad4bd57c236a7`
and `3a4f09eabf7095c6f79fc4a7f0d230c458bdc2b7838fec6faaac86884b5ef7e8`.
The loader admits only `_lizardman001` as the body renderer and the exact
`_ammunition04` accessory beside it. It disables only that per-instance
accessory after all validation, restores the prior enabled state during fault
rollback and teardown, and never changes shared donor assets. Module OFF reads
no theropod files and allocates no Unity art resources. Tyrannosaurus alone has
the bounded 2.0 view-only scale step; mechanical size, reach, footprint and
movement remain native-rule properties pending live measurement.

Offline review retained two useful predeployment failures. The first wide-bite
pose revealed that the original lower jaw was too far from the measured native
pivot. The second turn pass exposed a neck seam and tail-root ring triangles.
The authoring source now uses the actual jaw rest frame, a neck overlap and an
uncapped body-embedded tail transition. No package was deployed for either
failure. The complete regenerated clay, silhouette, textured, unlit,
party-camera, movement, turn and contact/extreme set passes internal review:
20 PNGs, 181.63 seconds of Blender render time, with the sorted hash-manifest
SHA-256 `504ea9cfaed5723e74b3b67b81ef43155baf71877510de80bbc9de3b950331ea`.
This proves only the offline authored shapes and pose envelope; native contact,
cadence and lifecycle remain required. Human aesthetic review remains
`NOT_PERFORMED_NONBLOCKING`.

## Validation order

Focused development uses a named nonempty Sprint 24 filter and production
compilation. Before deployment: source/profile/identity tests, offline asset
review, JSON-shape checks, internal coverage review, and a coherent pushed
candidate. The frozen candidate then receives exactly one fresh
`Invoke-KmgGate.ps1 -Level Sprint` run. Guarded native work uses that exact ZIP
across fresh transactions. Suppression removal creates the public artifact and
requires only affected native/publication reruns plus justified unchanged
evidence. A documentation-only final-records head receives text/JSON/link/
consistency checks only, never another product build.
