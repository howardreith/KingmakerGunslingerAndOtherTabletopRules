# Gunslinger Class Outfit Kitbash Mission

## Authority and status

This is the durable operating contract for the Gunslinger class outfit kitbash
mission. The authoritative work order supplied by the human remains controlling.
This record is an operationally equivalent checklist intended to survive context
compaction. The mission is active and incomplete.

Intake baseline:

- integration baseline: `5949165e2a6407ca480d46cd86d8944e4152e2fb`;
- baseline release: `v0.0.110`;
- feature branch: `codex/gunslinger-class-outfit-kitbash`;
- installed-game target: Pathfinder: Kingmaker 2.1.7;
- production version must remain derived from repository conventions until the
  transactional release checkpoint.

Current checkpoint:

- the deterministic audit, accepted Human shortlist render, and accepted
  nine-race/two-gender neutral-body matrix are complete;
- `magus-complete` is selected at 88/100 and is independently bound to the
  Gunslinger class in published commit `bf3e052cb3a91691e214ec9a87c025f25f380c2d`;
- the commit-bound clean package and canonical guarded working-save load pass
  with exact build/save correlation and no save write;
- equipment overrides, weapon/motion states, rebuild, and outfit persistence
  remain open, so the mission remains active and incomplete.

## Objective

Replace the Gunslinger's generic Fighter-derived class clothing with a
distinctive, coherent outfit assembled only from native Kingmaker visual
resources. It must read as a Golarion black-powder swashbuckler, frontier
officer, or privateer in class-preview and ordinary isometric presentation,
including when no firearm is visible.

The preferred vocabulary is a fitted coat, long vest, waistcoat, split-tail
garment, or equivalent strong torso silhouette; dark or weathered leather;
charcoal, muted brown, cream/gray, and restrained burgundy or oxblood accents;
practical straps, pouches, gloves, bracers, belts, and sturdy boots; and at most
one controlled asymmetrical detail.

Do not ship a generic Fighter recolor, literal cowboy, literal pirate, heavy
knight, barbarian, wizard, brightly theatrical costume, mandatory hat, baked
weapon, broken body geometry, or unrelated male/female identities.

## Non-negotiable scope

- Use no custom mesh, texture, material, shader, animation, or asset bundle.
- Add no runtime dependency and never depend on another mod's assets.
- Never copy or redistribute game resources, assemblies, reference images, raw
  catalogs, screenshot batches, saves, or machine-local configuration.
- Never modify installed game files or native blueprints.
- Preserve all Gunslinger GUIDs, symbols, mechanics, progression, starting
  items, archetypes, localization, save compatibility, and user color choices.
- Prefer one coherent native base outfit and no more than two proven-compatible
  accents.
- Preserve race, gender, armor, cloak, helmet, backpack, hands/weapon,
  animation, rebuild, save/load, and color-ramp behavior.

## Required evidence sequence

### 1. Installed API and native-resource investigation

Before any production identifier is selected or hardcoded, inspect and record:

- `BlueprintCharacterClass` male, female, and shared equipment arrays;
- color defaults, `GetClothesLinks`, and `LoadClothes` or installed
  equivalents;
- `EquipmentEntityLink` construction and loading;
- `KingmakerEquipmentEntity.GetLinks` and `Load`;
- `EquipmentEntity` body/outfit parts, special part types, hiding flags,
  color profiles, ramp indices, and ordering/layering behavior;
- `CharacterAvatar` add/remove/rebuild/baking behavior;
- normal armor, cloak, helmet, backpack, hands, and race/gender fallback rules;
- native Barbarian and Paladin implementations as class-identity benchmarks;
- current repository registration, diagnostics, package, validation, and
  guarded-runtime extension points.

Investigate sources in this order:

1. native player-class clothing links;
2. native item-linked equipment entities;
3. NPC/raw/orphaned entities only after direct rig, shader, body-part, and
   animation proof.

Initial donor streams are Alchemist/Grenadier, Inquisitor, Ranger, Rogue,
Slayer, Bard, Magus, Fighter variants, Aldori/Duelist-like sources, followed by
resource-name hypotheses for officer/privateer/practical accents. Resource
names are discovery hints only, never aesthetic proof.

### 2. Deterministic guarded audit

Implement the smallest default-off, fail-closed audit reachable only through
the existing guarded runtime-test request mechanism. It must be deterministic,
must not mutate saves, progression, inventory, or normal gameplay, and must:

- emit a sorted, deduplicated ignored JSON/CSV catalog with source, blueprint
  identity, asset ID, resource name, source classification, gender/race
  coverage, load result, body/outfit metadata, hiding flags, color profiles,
  ramp evidence, structural risks, and deterministic candidate-set identity;
- apply a selected candidate set to a disposable avatar, rebuild it, cycle only
  valid colors, and restore original state;
- render serious candidates in-game with exact candidate IDs in sidecar data;
- capture male/female, preview-like and isometric, no-weapon, pistol, and
  long-gun presentations.

Raw audit output and captures remain ignored local evidence.

### 3. Evidence-based selection

Narrow to the best three candidate sets and score each out of 100:

| Criterion | Weight |
|---|---:|
| Distinctive silhouette and immediate Gunslinger readability | 30 |
| Black-powder swashbuckler/frontier-officer/privateer coherence | 25 |
| Body, clipping, animation, and equipment compatibility | 20 |
| Race and gender coverage | 15 |
| Color-ramp behavior and visual quality | 10 |

Production requires at least 75/100 and no hard rejection. Hard rejections are
missing body geometry, broken materials, baked weapon duplication,
unacceptable hair/ear/horn/tail/head hiding, severe animation clipping,
unsupported race/gender without a safe fallback, broken armor transitions,
optional entitlement/dependency, or a result still reading primarily as the
generic Fighter.

### 4. Production implementation

After runtime evidence selects a viable candidate, add a focused presentation
policy that owns exact selected shared/male/female identifiers and proven
default colors. It must construct independent arrays/links, validate malformed,
duplicate, null, or unresolved data with actionable diagnostics, and assign
only the new Gunslinger blueprint. Avoid reflection where a stable installed or
already-used API exists; isolate and validate any unavoidable reflection.

### 5. Tests and qualification

Focused tests must prove exact approved IDs and placement, independent arrays,
valid/nonduplicated links and colors, clean-environment resolution, supported
race/gender loading or documented fallback, non-mutation of Fighter,
Barbarian, Paladin, donors, and unrelated classes, stable Gunslinger gameplay
identity, no optional dependency, and clean package contents.

Then run and record:

1. focused validation;
2. repository validation;
3. complete domain tests;
4. clean Release build;
5. installable package validation;
6. required compatibility/profile validation;
7. guarded installed-game qualification.

Runtime qualification covers all dynamically discovered supported player
race/gender combinations, valid color sampling, idle/walk/run/turn/fire/reload/
melee, no weapon/pistol/musket/blunderbuss-equivalent, light/heavy armor
transitions, headgear/hair/cloak/backpack/inactive-weapon interactions,
preview-like and ordinary isometric views, and save/load plus rebuild/respec
where applicable. Mechanical claims require structured guarded evidence;
screenshots support aesthetic claims.

## Fallback and stopping contract

Use a compatible multi-entity native kitbash first, then a strong single native
non-Fighter outfit. If exhaustive rendered evidence proves every native option
scores below 75 or has a hard rejection, keep production unchanged, preserve
and publish the reusable guarded audit, document exact failures, and identify
the smallest separately authorized custom accessory fallback.

Stop only for a genuine hard stop: missing/unidentifiable required game
installation, inability to isolate the repository without risking unrelated
work, an unsafe Steam condition, approved push-helper rejection, required
proprietary mutation/redistribution, or exhaustive native-candidate failure.

## Publication and completion

Commit only coherent, validated checkpoints on the exact feature branch.
Publish every coherent commit solely with the approved external helper, then
verify `HEAD`, the local branch ref, and the matching origin branch are
identical. Never raw-push, merge, rebase published work, rewrite history, or
alter push policy.

Completion requires all audit, rendering, scoring, production, tests, clean
build, package, compatibility, guarded runtime, documentation, publication,
remote-SHA, clean-tree, and forbidden-artifact checks to pass, with exact
commands and uncertainties recorded.

## Execution checkpoint: first finalist matrix diagnostic

Published commit `fe86bce4484d45ca8f6a6f7070bfd7942fd5a0fc` ran through
Steam App ID 640820 against exact working save `KMG_AUTOMATION_WORKING` at
`runtime-evidence/20260830T2237589386140Z-gunslinger-outfit-finalist-race-matrix`.
The guard, save boundary, installed-game identity, cleanup, and automatic exit
passed. Male Aasimar completed both palettes (two records/four images), then
the first female Aasimar donor failed closed before finalist application
because its original avatar could not prove exact restoration. No save API or
production blueprint was touched, and no visual acceptance is inferred from
the partial batch.

The evidence also exposed a noncanonical Medium female Halfling source in the
unfiltered donor order. The guarded fixture selector now filters Gnome and
Halfling to Small and all other installed player races to Medium, retains every
matching donor in deterministic order, and accepts one only after exact
entity-order, ramp, and saved-link remove/re-add proof. Rejected disposable
donors are recorded with exact source and reason before the next is tried.
Repository validation, installed-game compilation, all 1365 tests, clean
Release/package validation, and the quiescent 163-check runtime preflight pass
for this repair. A clean published rerun remains required.

The published retry at
`runtime-evidence/20260830T2257046480918Z-gunslinger-outfit-finalist-race-matrix`
proved the donor retry path and canonical Halfling size, then safely exhausted
six female Aasimar donors for one identical instrumentation reason: each valid
avatar had an original entity count of zero. Empty is a legitimate ordered
avatar state, not a missing avatar. The next narrow repair therefore accepts
only an exact empty-to-empty round trip with unchanged saved links, continues
to reject a null avatar, and applies the same restoration during fallback
cleanup. All 1365 tests and clean package validation pass; a published rerun
remains required before race-grid acceptance.

The exact-empty retry at
`runtime-evidence/20260830T2309022972406Z-gunslinger-outfit-finalist-race-matrix`
advanced through all 18 fixtures. Dynamic race coverage, donor acceptance,
native links, 36 palette records, 72 PNGs/180 views, and 18/18 avatar
restorations passed. Only the final whole-world unit snapshot timed out after
360 updates; party/save/production boundaries remained intact. Because the
current evidence does not identify the missing or unexpected global reference,
the gate remains failed. Diagnostic-only instrumentation now emits exact
expected/actual counts and described missing/unexpected unit and party
references without changing the strict cleanup criterion.

The published diagnostic rerun at
`runtime-evidence/20260830T2323563433313Z-gunslinger-outfit-finalist-race-matrix`
identified the exact difference: no missing unit, one unexpected native
`AnimalCompanionUnitLeopard` (`54cf380dee486ff42b803174d1b9da1b`), exact
party, and no remaining disposable actor. The native female-Elf fixture was
`StartGamePregenRangerUnit`. Cleanup is now narrowly instrumented to capture
only the active disposable actor's exact `UnitDescriptor.Pet` reference when
that reference was absent from the initial snapshot, retire only that proven
request-owned dependent, and retain the original strict whole-unit/party
equality gate. The next published runtime run must prove that relationship and
exact cleanup before the 72-image matrix can be reviewed or accepted.

## Execution checkpoint: clean mechanics, rejected prefab visuals

Published commit `8b8d0b17aa90318425404efac56f6977bb2ad11c` completed the
guarded matrix with terminal PASS at
`runtime-evidence/20260830T2341080018300Z-gunslinger-outfit-finalist-race-matrix`.
It proved all 9 installed player races/18 gender cells, 36 palette records,
72 PNGs/180 views, 18/18 exact avatar restorations, exact party/global-unit
cleanup, no save call, and no production mutation. The one request-owned
Leopard was captured from and retired with the male-HalfElf fixture. The
earlier female-Elf attribution was an inference from the preceding terminal
delta, not the final relationship record, and is superseded by this evidence.

Direct inspection of every image rejects this batch for visual acceptance.
Several native NPC donor prefabs retained baked clothing, shields, bows,
quivers, capes, or large weapons despite an empty `CharacterAvatar`; examples
include male Elf, male HalfElf, male Tiefling, female Aasimar, female Gnome,
female Human, and female Tiefling. This is a fixture-visual contamination
failure, not evidence that the exact Magus clothing pair is incompatible, so
the provisional 81/100 finalist score and production state remain unchanged.

Installed-game API inspection identified the deterministic player-appearance
path: `BlueprintRace.Presets`, `DollState`, `DollData.CreateData`, and
`DollData.CreateUnitView(false)`. The fixture now supplies that exact native
character-creation view to `EntityCreationController.SpawnUnit`, requires a
nonempty exact preset/doll entity baseline with zero unexpected avatar
entities, clears every `UnitBody.AllSlots` item, and requires both hand weapon
models to be absent. Focused source tests enforce this contract. Repository
validation, all 1365 tests, installed-reference compilation, clean Release
construction, and explicit strict package validation pass. The
pre-publication package SHA-256 is
`04f13af8fd17a0d9e18611e13c3cc3d27d83f6c7cf1e7dca3b05e094e5f73d18`;
DLL SHA-256 is
`d3ec07a2238ff2c062686dfc4e570ee602afaa716a26ddfa01607cb2627653bc`.
The next accepted matrix must be rebuilt from the published commit and all 72
new images must be inspected; mechanical PASS alone remains insufficient.

### Native visual-race correction

The first published player-doll run at
`runtime-evidence/20260831T0013309100348Z-gunslinger-outfit-finalist-race-matrix`
failed closed during initialization on male Aasimar before any fixture spawn or
capture. Published commit `b67ec5444d4b3ef8480007c10fb2d73bab3c031e` had
incorrectly required a preset's visual `RaceId` to equal the progression
race. The working-save guard passed, no save or production mutation occurred,
the initial 265-unit/3-party snapshots remained exact, and automatic exit
completed.

Read-only IL inspection resolves the semantics exactly. Native
`DollState.Validate` selects serialized `BlueprintRace.Presets[0]` without a
progression-race equality predicate; native `DollData.CreateUnitView` loads
the skin using `RacePreset.RaceId`. The fixture now mirrors both operations and
records progression race and visual race separately. A focused regression test
forbids restoring the invalid equality assumption. Repository validation, all
1365 tests, clean installed-reference Release construction, and strict package
validation pass. Pre-publication package SHA-256 is
`e6af511660abba47fd22dae853f6875ed31c1bd68607cc60440fe640f62c9502`;
DLL SHA-256 is
`edbc636195bd0b1fe80e41df7bdf532236502135da819570e07980a99a645824`.

### Native view attachment correction

The corrected visual-race build at published commit
`55c487cc460c4950305d47e3c679bf8e858c943d` ran through Steam App ID
`640820` at
`runtime-evidence/20260831T0026335779530Z-gunslinger-outfit-finalist-race-matrix`.
It loaded exact DLL SHA-256
`1be20efa6c457eb8da426b54f67598c3529cfc76c5c62454eea7ce9654e1897c`
and MVID `e496489f-2fbc-47cf-a4c1-da914eda915a`, then failed closed at
`spawn-male-aasimar` before a fixture, asset mutation, or capture existed.
The working-save/no-save boundary, exact cleanup, production non-mutation, and
automatic exit passed.

Installed IL identifies a second probe-lifecycle error. Native
`DollData.CreateUnitView` configures the root
`Character` component and returns its `UnitEntityView` template.
`UnitEntityView.CharacterAvatar` is assigned later by
`UnitEntityView.OnDataAttached` using
`GetComponentInChildren<Character>()`. Requiring that property before
`EntityCreationController.SpawnUnit` was therefore earlier than the native
lifecycle. The probe now requires `dollView.GetComponent<Character>()` before
spawn and retains the stronger `_actor.View.CharacterAvatar` requirement after
data attachment. A focused test forbids the premature property check.

Repository validation, all 1365 domain tests, clean installed-reference Release
construction, and strict package validation pass for this narrow correction.
The current pre-publication local-runtime package SHA-256 is
`024d0c2b89a6e561b4c8d6eecc67e6f30c6b85941b893db7f9dcc6d5d22b0f2e`;
DLL SHA-256 is
`3cf170e14b0dc96910b093ee0737e713fd7d0c432a20cd59971c36dfc7be7d42`.
The correction still requires publication, a commit-bound rebuild, guarded
preflight, the complete runtime matrix, and direct review of every replacement
image.

### Native doll-view ownership correction

Published lifecycle commit `08bfed17843adf348b210883b6f929b1af7c5678`
passed quiescent preflight (163 checks) and ran through Steam `640820` at
`runtime-evidence/20260831T0044105199782Z-gunslinger-outfit-finalist-race-matrix`.
It loaded exact DLL SHA-256
`9ebe80a42b3711dcc874357792da4b5a2e797eb0db18cd7a8d7f7d9a5e374db8`
and MVID `f8abfd0e-59da-48c0-a796-15f085984c32`. The view attached
successfully, proving the previous lifecycle repair, but all five deterministic
male-Aasimar donors reached the 360-update ceiling with
`rigExact=true`, exact gender/race/size, no weapon models, and
`dollExact=false`/`rendererCount=0`. Zero images were produced. Exact
cleanup, no save call, no production mutation, and automatic exit passed.

Installed IL explains the shared failure. The
`SpawnUnit(BlueprintUnit, UnitEntityView, ...)` overload always instantiates
its supplied prefab again. `DollData.CreateUnitView` had already instantiated
and runtime-configured a `Character`, so the second clone retained the rig but
not its runtime equipment collection. The public native
`SpawnEntityWithView` path instead creates data for the supplied view, attaches
it, registers it, and does not clone it. The fixture now assigns the exact
blueprint, fresh unique ID, position, and rotation, registers that existing
view, requires reference-identical ownership transfer, and destroys the local
view only if transfer fails.

Focused tests require the direct registration/ownership contract and forbid the
old double-clone call shape. Repository validation, all 1365 tests, clean
installed-reference Release construction, and strict package validation pass.
The pre-publication package SHA-256 is
`d1dfe7cf3697e5757ce0bc86d7f0e2af72a621e98e4021c5ff5101511885a0ec`;
DLL SHA-256 is
`5462960ebbfd8815523b2132e84d7b2377dfc52a051b2ccdb04a646bf33e7108`.

## 2026-08-30 - Direct review rejects a no-weapon false positive

Published commit `141c6a8e1fcdacdb61164113ac77a6191b16254e` passed the
guarded Steam `640820` race matrix at
`20260831T0058130079392Z-gunslinger-outfit-finalist-race-matrix`.
Structured evidence reported all 9 player races, 18 gender/race fixtures, 36
palette records, 72 PNGs/180 views, 18/18 exact restorations, exact cleanup,
no save API, and no production mutation. All 72 images were then inspected.
Both female-Human palettes visibly retained the donor's oversized two-handed
sword, so the mechanically passing batch is visually rejected.

The false-positive record is exact: fixture source
`AmiriLevel20_Companion`, source GUID
`ca08eabf5f6a33e4ba366e889e4fecdc`,
`clearedSlotItemCount=14`, `rendererCount=2`, and
`noWeaponModels=true`. The existing check observed only the current
active/inactive `HandsEquipment` references after removing items; it did
not prove that a previously instantiated donor weapon renderer had been
destroyed. This changes no candidate score or production binding.

Installed reflection shows that `BlueprintUnit.UnitBody` has a public
constructor and explicit weapon, armor, accessory, limb, and quick-slot fields.
The corrected harness replaces only the disposable cloned donor's body before
entity creation with a request-local neutral body, preserves the native hidden
`EmptyHandWeapon`, empties starting inventory and limb/quick-slot arrays,
and rejects any donor that nevertheless creates a slot item. It does not mutate
the source blueprint and contains no donor-name or donor-GUID exception.

Repository validation, the complete 1365/1365 domain suite, clean
installed-reference Release construction, packaging, and strict standalone
package validation pass. The pre-publication package SHA-256 is
`be1b6048c299f1d996db1091372c8e6c43863f51bae7b287ee58ca76f3c92bbb`;
DLL SHA-256 is
`68489bd17dd3bb363bbf53464beda0f7011cc10a7725212b31ef60127c80e13d`.
The full replacement matrix and direct review of every new image remain
mandatory.

## 2026-08-30 - Neutral-body race matrix accepted

The published neutral-body commit
`47d6c55f6742219dac07824b08e1daa1c23309a1` passed the guarded Steam
`640820` replacement matrix at
`20260831T0125478276325Z-gunslinger-outfit-finalist-race-matrix`.
The game loaded exact DLL SHA-256
`57f9d7dec390cae8f53a78fadb9bd8c5cadb30368c97b5eadd8e454806ce285c`
and MVID `1bace4ca-657e-4d4b-bccf-d9ee4933876e`. The result contains all
9 dynamically discovered player races, 18 gender/race fixtures, 36 palette
records, 72 PNGs/180 views, and 18/18 exact restorations. Every accepted donor
used a request-local neutral body, created zero slot items, and exposed no
weapon model. Exact cleanup, no save API, no production mutation, and process
exit passed.

All 72 ignored images were inspected directly. The female-Human greatsword
contamination is absent from both palettes and both preview/isometric paths.
All other cells show intact geometry/materials, expected race features, no
donor gear, and coherent native versus alternate ramps. The replacement batch
is mechanically and visually accepted. `magus-complete` is now the selected
production candidate at 88/100; five compatibility points remain withheld for
equipment, animation, rebuild, and persistence qualification.

Exact next action: implement the focused, independently owned Gunslinger class
appearance policy with the accepted two-link male/female pairs and 2/22
defaults, then add observable blueprint-state tests before any final equipment
or motion runtime claim.

## 2026-08-30 - Focused production binding locally qualified

Production now owns the accepted identifiers in
`GunslingerClassAppearanceCatalog` and applies them through
`GunslingerClassAppearance`. The catalog returns validated defensive copies;
the adapter resolves every native `EquipmentEntity` before changing the new
Gunslinger blueprint, creates new male/female link arrays and link objects,
assigns a new empty shared array, and applies defaults 2/22. Fighter remains a
source only for starting gold. No Magus or Fighter blueprint is mutated.

Two focused tests prove exact link order/defaults, defensive copying,
null/malformed/duplicate/count rejection, resource-before-mutation wiring,
fresh arrays, and absence of the five former Fighter appearance aliases. The
active deterministic count is 1367; the active inherited validator now
forwards that count through the previously missing 0.0.106-to-0.0.105 edge.
Repository validation and all 1367/1367 Release domain tests pass.

The clean installed-reference Release build, package construction, and strict
standalone package validation pass. Pre-publication package SHA-256 is
`34d9a7005fd9f535c33e460d7b4e23dc94553dbbcd34ee45540aeff167476df0`;
DLL SHA-256 is
`6f039e773910a314f6abf46e2bd0d87d737660abd898d1ea7bd58918d11893eb`.
Version remains 0.0.110. No runtime claim is made from this local gate.

Exact next action: commit and publish this coherent production binding, verify
all three refs, rebuild the commit-bound local-runtime package, pass quiescent
preflight, and run the guarded canonical working-save smoke before extending
the final production equipment/motion harness.

## 2026-08-30 - Published production binding and save smoke pass

Commit `bf3e052cb3a91691e214ec9a87c025f25f380c2d` was published only through the
approved helper. `HEAD`, the local feature ref, and
`origin/codex/gunslinger-class-outfit-kitbash` were identical afterward. Its
clean `Build-Local.ps1` artifact passed repository validation, all 1367 domain
tests, exact-reference Release construction, and strict package validation.
The commit-bound package SHA-256 is
`4a91c92b9f842b7744adf707a2149ae13a4cc1ec70733979ad453406548a6c61`;
the DLL SHA-256 is
`78c8a7e8d8c1372bea930e4a48b4211ef4941974a062c1dbb707b0a8b7a1b8f5`,
with MVID `41fd1851-9dec-4adf-87eb-0e79763d5e02`. After the required artifact
quiescence interval, all 163 runtime preflight checks passed.

The canonical Steam App ID 640820 run
`20260831T0159136175513Z-working-save-smoke` reached terminal `PASS`. It loaded
the exact commit-bound DLL and version 0.0.110, found one exact
`KMG_AUTOMATION_WORKING` and one distinct protected baseline among 111 complete
catalog descriptors, invoked one receiver-correlated UI/load path, observed
load completion, and stabilized at game ID
`dce769e0-229c-4bfd-b8ea-e2d572bf8472` with three party members. The strict
sequence was `22<24<26<29<32`; no save-writing API was observed, hooks were
removed, and automatic exit completed. This proves the production build loads
the disposable save safely. It does not yet prove outfit rebuild, persistence,
equipment override, or motion compatibility.

Exact next action: implement and source-qualify a guarded production outfit
compatibility scenario that observes the exact bound blueprint and covers
equipment overrides, representative firearm/empty-hand states, required
motions, preview/isometric rendering, rebuild, and save/load persistence
without mutating the protected baseline.

## 2026-08-30 - Production compatibility harness locally qualified

The guarded `gunslinger-outfit-production-compatibility` scenario now observes
the actual production Gunslinger class rather than a donor approximation. It
first resolves and validates the exact production male/female entity links and
2/22 defaults for every installed player race and both genders. It then creates
two request-local Human character-generation dolls through native
`DollState`/`DollData`, calls `SetClass` with the production Gunslinger, and
captures 16 deterministic states per gender in paired character-creation-like
and ordinary-isometric presentations.

The state matrix covers default and alternate ramps; empty hand; held pistol,
held musket, stored inactive musket, and held blunderbuss; light- and
heavy-armor override plus removal/rebuild; tricorn override plus removal with
hair restoration; cloak override plus removal/rebuild; and backpack visible
plus removal/final rebuild. Every record carries exact link-backed entity
identity, body-slot state, color ramps, saved links, production-blueprint
immutability, and cleanup assertions. The scenario expects 32 structured
records, 64 ignored PNGs, and 160 views. It never calls a save-writing API and
does not attempt to prove motion, firing, reload, melee, or save persistence.

One focused executable/reflection case brings the deterministic suite to
1368. It proves catalog/request/runner/script registration, the production
class boundary, exact equipment identifiers and state labels, native doll
construction, isolated exact-signature reflection for installed private hair
and backpack state only, fail-closed accounting, no-save/no-production-
mutation assertions, and the separate motion boundary. Repository validation,
all 1368/1368 Release domain tests, clean installed-reference Release build,
package construction, production firearm/SoundBank validation, and strict
standalone package validation pass. The dirty-tree package SHA-256 is
`b6da46f4c1a7c61fab0625762b46f5f7c222f6d478811300fdfa041512f409d6`;
the staged DLL SHA-256 is
`1ca246f477ed3ccbd6ef7a194fc90a5b5a14671d2334bbbaf0a08b76236b9d8`.
No runtime or visual compatibility claim is made from these local gates.

Exact next action: commit and policy-publish the coherent compatibility
harness, verify the three refs, build and validate its commit-bound local
runtime artifact, pass quiescent preflight, run the exact guarded scenario
through Steam App ID 640820, and inspect every resulting image before awarding
or withholding any remaining compatibility points.
