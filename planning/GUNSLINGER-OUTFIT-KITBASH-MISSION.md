# Gunslinger Class Outfit Kitbash Mission

## Authority and status

This is the durable operating contract for the Gunslinger class outfit kitbash
mission. The authoritative work order supplied by the human remains controlling.
This record is an operationally equivalent checklist intended to survive context
compaction. The mission is complete.

Intake baseline:

- integration baseline: `5949165e2a6407ca480d46cd86d8944e4152e2fb`;
- baseline release: `v0.0.110`;
- feature branch: `codex/gunslinger-class-outfit-kitbash`;
- installed-game target: Pathfinder: Kingmaker 2.1.7;
- production version must remain derived from repository conventions until the
  transactional release checkpoint.

Current checkpoint:

- the deterministic audit, accepted Human shortlist render, accepted
  nine-race/two-gender neutral-body matrix, static equipment matrix, and native
  motion matrix are complete;
- `magus-complete` is selected at 93/100 with no hard rejection and is
  independently bound to the Gunslinger class;
- the guarded three-launch persistence transaction proves exact native respec,
  save/load, class-outfit reconstruction, palette retention, cleanup, and a
  fresh-process marker-absence boundary;
- final repository/package revalidation and documentation closure pass;
- implementation/evidence checkpoint
  `92de86005d2db4f0731b296fc3d803033e15112a` was published by the approved
  helper with identical HEAD/local/origin refs and a clean tree.

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

## 2026-08-30 - Stored-musket diagnostic and corrected retry contract

Published harness commit
`82361d31d2b0d7d278046161c13ee503aff6d51a` loaded exact DLL SHA-256
`5265ce9925c4c5b3dd4b2ef90bd0f14d5707edd9d871d9959e77bb060c943562`
and MVID `3b3ab851-e16f-48e6-b900-43c4d78b2558` through Steam App ID
640820. Guarded evidence
`20260831T0304180367838Z-gunslinger-outfit-production-compatibility`
reached a clean terminal `FAIL` after four male-Human states. It passed exact
game/mod/class/race-link identity, production-blueprint immutability,
working-save/no-save boundaries, request-local cleanup, and process exit.

The failed state was `musket-stored-inactive`. The actor retained the exact
musket slot item and every outfit/hair/ramp/rebuild contract, but the harness
required the inactive long-gun model to be non-renderable. That requirement
contradicts the repository's already-qualified firearm presentation contract:
long guns have visible stored presentation, while only designated handgun
profiles may be intentionally hidden. This is an instrumentation defect, not
an outfit compatibility or aesthetic failure; the candidate remains 88/100.

The narrow correction exposes the established stored/held resolver only to
other runtime-test code, uses it for every equipped firearm, requires the
inactive musket to be out of combat and visibly renderable, includes that
model in capture bounds, and records its exact presentation role. The focused
test now prevents a return to the hidden-long-gun assumption. Repository
validation, all 1368/1368 tests, clean Release/package construction, and
strict standalone validation pass. Dirty-tree package SHA-256 is
`beed41bbe74601d8d0f499c2ff5dff340f3e90822e02d4fa2e0f25cd69ab6baa`;
DLL SHA-256 is
`397f4c6a5a9069ae07e0ee2cfd195aa88d2a8fa1d82edaed49b983f68efa3396`.

Exact next action: publish this focused harness repair, rebuild and validate
the exact commit, pass quiescent preflight, rerun the complete 32-state matrix,
and inspect all 64 replacement PNGs. The four partial images from the failed
batch are diagnostic only and cannot close any visual gate.

## 2026-08-30 - Native-doll settlement retry contract

Stored-musket repair commit
`453f54732c05be6141d3eec259e4c46325f047e0` loaded exact DLL SHA-256
`d9be26094a0eb8fd6f86dcff5572e85756ff311f1db12d22699ca4311c2b1388`
and MVID `0c09675f-81e2-44f2-b98d-f14dd0ee619e` in guarded evidence
`20260831T0319410552031Z-gunslinger-outfit-production-compatibility`.
The run failed before any capture because selected native hair was absent at
the end of the old production-first settle sequence. Exact production links,
class entities, saved links, save protection, blueprint immutability, cleanup,
and exit passed. Zero PNGs were produced, so this is not visual rejection
evidence and the candidate remains 88/100.

Read-only installed-method inspection plus comparison with the already-
accepted race harness identified the missing pre-mutation boundary. The
compatibility harness now waits the full native settle minimum and requires
exact descriptor `DollData`, all resolved doll entities, selected hair, rig,
renderers, and empty weapon state before snapshot or production mutation. The
change is harness-only. Repository validation, 1368/1368 tests, clean Release
build/package, firearm/SoundBank checks, and strict package validation pass;
local package SHA-256 is
`f7e0b896470a4fc120e6d9f8d7166ca1d6bdfaf7a94c53b1545ba73b12ea073c`.

Exact next action: publish the readiness correction, rebuild and preflight the
exact commit, rerun the full 32-state matrix through Steam 640820, and inspect
all 64 images. Do not advance to motion or persistence qualification until
this static compatibility gate passes and its complete images are accepted.

## 2026-08-31 - Static production compatibility accepted

Readiness commit `59eb7a97d6c1278f1e4e0d351aa6d4557b2db566` was already
policy-published with identical HEAD/local/origin refs and a clean tree. Its
commit-bound package SHA-256 is
`e15546c561d244f5f29517bec79f71025713cbd79530238ff69232f38fb18394`;
the installed DLL SHA-256 is
`10f1beaf90eb6f5578ab5c8c09f9d10b219d587bb2adb11b308a959a7a422b26`;
MVID is `780b053b-acb8-4716-a5b5-87b578e356e0`.

Guarded Steam App ID 640820 evidence
`20260831T0344513197562Z-gunslinger-outfit-production-compatibility`
reached terminal `PASS` on version `0.0.110`. It proves 18/18 exact installed
race/gender production links, two exact native Human dolls, 32/32 ordered
states, 32 sidecars, 64 PNGs, 160 labeled views, 2/2 original restorations,
unchanged production blueprint state, no save-writing API, exact request-local
cleanup, and automatic exit.

Every one of the 64 captures was directly inspected through 16 labeled review
boards outside the repository. Both genders retain coherent bodies, materials,
hair, and class clothing across both palettes; pistol, held/stored musket, and
blunderbuss presentation; light/heavy armor overrides and removals; tricorn
equip/removal with hair restoration; cloak; backpack; and final rebuild. No
hard rejection was observed. A separate sidecar/hash reconciliation passed
with zero issues over all 32 records and all 64 files. Eight female isometric
images retain low-density warnings but remain legible at 11,278-14,496
meaningful pixels and have non-low-density paired previews.

The static equipment/rebuild gate is closed. The candidate remains at a
conservative 88/100 because the final five compatibility points are withheld
as a block until motion/fire/reload/melee and outfit persistence/rebuild across
save/load and respec-like reconstruction also pass.

Exact next action: implement and source-qualify a separately guarded,
deterministic motion matrix for idle, walk, run, turn, fire, reload, and melee
on both production genders, then run and directly inspect it before beginning
the independent persistence gate.

## 2026-08-31 - Native motion source gate complete

Read-only installed-assembly inspection resolved the exact native contracts
before implementation. `UnitMovementAgentBase` exposes writable nullable
`MaxSpeedOverride`, live `Velocity`, `WantsToMove`, `IsReallyMoving`, and
`TickMovement(float)`. `UnitAnimationManager` exposes `Speed`,
`WalkSpeedType`, and `GetAction(UnitAnimationType)`; the installed enum is
`Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionLocoMotion.WalkSpeedType`
with exact `Slow` and `Normal` members. Existing qualified repository paths
establish native `UnitMoveTo` plus same-area `ForcedPath`, `ForceLookAt`,
`UnitAttack.CreateAttackCommand`, and production Reload Firearm through
`AbilityData`/`UnitUseAbility`. No animation-only substitute is used.

The new guarded `gunslinger-outfit-production-motion` scenario reuses the
accepted production `DollState`/`DollData` settlement boundary, but has its
own allowlist identity, 1,800-second collector window, evidence index, and
result contract. For each exact male/female Human production doll it captures
unarmed idle; musket slow walk, normal run, and body-relative turn; pistol and
musket native attacks; the actual production musket reload; and native
Shortsword melee. Attacks record ready, updates 1/12/36, and an event-aligned
acted frame. Reload records ready, updates 1/12/36/96/160/240, and an acted
frame. Expected output is 54 sidecars, 54 four-view PNGs, and 216 labeled
views. Every record repeats production-pair, ramp, hair, rig, saved-link,
blueprint, and no-save invariants.

Request-local cleanup restores nullable speed override, walk type, animation
speed, exact powder/ball counts, original avatar entities/ramps/saved links,
and exact global-unit/party snapshots; it removes the actor, target,
dependents, items, firearm state, and blueprint clones. Repository validation,
the complete `1369/1369` Release suite, clean installed-reference Release
build, SoundBank/firearm checks, strict standalone package validation, and all
169 runtime preflight checks pass. The local clean package SHA-256 is
`00c80de81ff7acc218c1bbf08e51623950281f90e74e5750fee685da48b6e9be`;
DLL SHA-256 is
`c60baee8be07590b39c30a8685bde51e277bb13d8f9d0b226fb9f3950a1e4abd`;
MVID is `a9e50b0b-b2e1-42f4-aa91-c9cdf98d4c5c`. These identities are
pre-commit and cannot support runtime acceptance.

The candidate remains 88/100. Exact next action: commit and policy-publish
this coherent harness checkpoint, rebuild/install the exact commit, pass
quiescent preflight, run the guarded scenario through Steam App ID 640820,
reconcile every sidecar/hash/invariant, and directly inspect all 54 PNGs.

## 2026-08-31 - Motion attempt 1 and native boundary repair

Commit `3071fe38a61b79131f96f965053e7bc058ce209f` was policy-published
with identical refs; commit-bound local-runtime package SHA-256 was
`5eb5da0e740b3d84801c256721f921b636db5471d676cd00de98e99f245d2db7`.
Guarded Steam run
`20260831T0455599323551Z-gunslinger-outfit-production-motion` returned
terminal `FAIL` after 28/54 records. The male fixture completed every native
action and the female exact production doll completed unarmed idle. Female
slow-walk then failed the unchanged clean-combat guard because player combat
state remained cached after the male request-local actor and target left
combat and were disposed.

The run retained exact save/version/game identity, blueprint immutability,
inventory/global-unit cleanup, no-save, and exit protections. Installed IL
shows registered `UnitCombatJoinController.Tick()` invokes
`Player.UpdateIsInCombat()` and raises the party-combat event when its
controllable-group recomputation changes the cache. The narrow repair captures
the clean player/party/turn-based baseline, invokes that full native lifecycle
after each fully retired fixture, records both sides of the boundary, and
requires exact equality. Diagnostic movement failures now expose navmesh,
actor, player, and turn-based predicates separately. No partial runtime image
is accepted and the candidate remains 88/100.

Exact next action: source-qualify, package, publish, and rerun the repaired
full matrix; accept motion only after a complete PASS, structured
reconciliation, and direct review of all replacement images.

The repaired source gate now passes repository validation, all `1369/1369`
Release tests, clean installed-reference build, strict package/firearm/audio
validation, and the stable 169-check runtime preflight. Pre-commit package
SHA-256 is
`7de0fc0ce93a703907a10d5862368083765dae831cd74487073988128538889d`;
DLL SHA-256 is
`b378256b722350bc9128b491e7f0d8e8f3a2b630bdccefe4664fb5c80f84e18f`;
MVID is `b4bf5593-d05b-41d5-b92c-d6ad1eff1356`. Commit-bound rebuild and
runtime replacement evidence remain mandatory.

## 2026-08-31 - Motion attempt 2 exposes group combat retirement

Published repair commit `fe24655acd4516e334796524ab7a3f40fd633888`
was rebuilt as package SHA-256
`5228a562f65fbb2b694ec617548e71d1b713c3fea35d93789834b36eccebd44e`,
DLL SHA-256
`ba1638817210bfa9b2d163356465719cc0d22e941947286c3d399bf3f236a9dc`,
and MVID `90286a4d-27e5-476d-82eb-1b1cbb3ac3a9`. Guarded Steam evidence
`20260831T0521459019080Z-gunslinger-outfit-production-motion` returned a
definite `FAIL` after all 27 male records. The clean initial boundary was
false/zero/false, but native attacks placed the three baseline party members
in combat. Retiring only the disposable actor and hostile left the observed
player/party/turn-based boundary `true/3/true`; the join controller correctly
preserved that live group state instead of fabricating a clean result.

The package/save/game identities, no-save guard, blueprint immutability,
inventory restoration, disposable target retirement, and structural cleanup
held. The batch remains diagnostic only. Read-only installed IL then resolved
the missing registered lifecycle: `UnitCombatLeaveController.Tick()` evaluates
groups and invokes full `UnitEntityData.LeaveCombat()` (unit event, equipment,
and AI lifecycle), after which `UnitCombatJoinController.Tick()` recomputes
the player cache and raises the party event. The harness now requires that
leave-then-join sequence at each retired fixture boundary and in cleanup.
Focused source validation, all `1369/1369` Release tests, the clean
installed-reference build, firearm/SoundBank checks, and strict package
validation pass. The settled runtime preflight passes all 169 checks; its
first post-build invocation reported only the already documented artifact-tree
stabilization sentinel. Pre-commit package SHA-256 is
`b3598b28366eb82161b66b1e65144430c9461380dc93dc3dd2bb15db9fd7fbb3`,
DLL SHA-256 is
`9f717b6c8d08f39cd67635bfc5e635543e38d60a38ce215a0a5c4f590cadfa41`,
and MVID is `6e2a6987-f89a-42c1-a3ad-e1635a47b796`. These dirty-state
identities are not runtime acceptance. The score remains 88/100.

Exact next action: commit and policy-publish the native leave/join repair,
rebuild the exact commit, and run a wholly replacement 54-record batch.
Accept no motion evidence until terminal PASS, invariant reconciliation, and
direct inspection of every replacement PNG.

## 2026-08-31 - Motion attempt 3 resolves the skipped unit event

Published commit `df4f3f04f55bbbdfe56ef113f723f89af23fa62a` rebuilt as
package SHA-256
`fa29aab259ef800d0db3ab11ccf6bd3b82999760778733523ef2737dfec348dc`,
DLL SHA-256
`876879b6ab7f1cd2a376e8f43ed74109722f4841eb335179c20dad463ad0b651`,
and MVID `c162b31d-1195-47ef-b8d7-685142f07801`. Guarded evidence
`20260831T0539205863874Z-gunslinger-outfit-production-motion` again failed
closed after all 27 male records with `true/3/true` still present. Exact
build/save/game identity, no-save, blueprint immutability, inventory/target
restoration, structural cleanup, and exit guards held; the batch is rejected.

Read-only installed IL identified the skipped boundary precisely. The harness
used low-level `UnitCombatState.LeaveCombat()`, which changes the unit state
but does not raise `IUnitCombatHandler`. Full
`UnitEntityData.LeaveCombat()` calls that state method, interrupts AI,
updates equipment/audio, and raises the unit event. The subscribed turn-based
controller handles that event with `RemoveUnit`; its registered `Tick()` then
recomputes cached `HasEnemyInCombat`. Only after that cache update can
`UnitCombatLeaveController.Tick()` retire the player group and
`UnitCombatJoinController.Tick()` recompute/announce the player boundary.

The harness now uses the full unit lifecycle for every request-local actor,
target, and dependent; executes the registered turn-based, group-leave, and
player-recompute ticks in that order; records enemy/history/unit-list caches;
and requires exact clean-baseline restoration. A focused test forbids the
low-level actor/target bypass. Installed-reference compilation, repository
validation, and all `1369/1369` tests pass. Candidate score remains 88/100.

Clean pre-commit packaging and all strict firearm/audio checks pass. Package
SHA-256 is
`ae22f6d1804ef1d4b9677d0a55c57dd3371c0340b63284f76e94e7bd8b5120f3`,
DLL SHA-256 is
`d0ba5261d5cf26d0b57534f060fbcba7407b1c4f0c421230f99ea8de2dcdcd75`,
and MVID is `40e11afc-987d-4755-a057-df54bbfd09bf`. The first
post-build preflight reported only the documented
`unsupported-does-not-build-or-stage-package` artifact-tree stabilization
sentinel; the identical settled-tree rerun passed all 169 checks. These
dirty-tree identities qualify the source checkpoint, not runtime behavior.

Exact next action: commit and policy-publish this full-event repair, rebuild
the exact commit, and rerun the entire replacement matrix. No partial image is
accepted.

## 2026-08-31 - Motion attempt 4 replaces party-coupled combat fixtures

Published commit `f127e1f25f0d6d562a27a56ce9fe23f9b1ab8044` ran as
package SHA-256
`66d97da08b4615991210cf74e5f0784d1de3c8910dfcecac78d779ec96f6dbed`,
DLL SHA-256
`ea7c0b4931fbd32587aa9451b2c3475613bb866cc3658ad9dc67b63abfe7229e`,
and MVID `1f1de511-e4f9-4f52-98e0-ec2127a56494`. Evidence
`20260831T0601202638447Z-gunslinger-outfit-production-motion` failed closed
after 9/54 male records when the next musket attack found the preceding
pistol action no longer quiescent. The complete pistol schedule, including
the acted frame and one exact discharge, ran, but no partial image is accepted.

Installed IL and the live boundary identify fixture coupling rather than an
outfit defect. The disposable actor inherited the real player faction, whose
group ID is the directly-controllable group; native combat therefore enlisted
the working-save group. Live enemy memory can also rejoin a conscious retained
target between actions. Exception cleanup restored units, target, inventory,
blueprint, and save guards, but correctly reported player and turn-based combat
still true.

The replacement fixture clones two factions request-locally, makes only those
clones mutually hostile, proves neither actor nor fresh per-attack target is a
player-faction/group member or hostile to the save anchor, and retires each
target before the next action. Every tick and capture now requires the original
player/party/turn-based caches exactly. All clones and their memory links are
removed during guarded cleanup. Installed-reference compilation, repository
validation, and all `1369/1369` tests pass. Candidate score remains 88/100.

Clean Release packaging, strict package/firearm/audio validation, and the
settled 169-check preflight pass. The first preflight reported only the known
`unsupported-does-not-build-or-stage-package` stabilization sentinel; the
identical rerun passed. Pre-commit package SHA-256 is
`78e8a067544d097c158aa77ce014fa9ccc0caf9863a6d2d9691492c7821cfd9c`,
DLL SHA-256 is
`db27ce97885fbba43df32c5bc804fde1ef81d3e6ed45c521c1bfd7386616cd9d`,
and MVID is `f4bc8c6e-c148-4890-818c-34dba4f32f1a`. These dirty-tree
identities qualify the source/package checkpoint only.

Exact next action: publish the coherent checkpoint, rebuild its exact commit,
and rerun all 54 records.

## 2026-08-31 - Motion attempt 5 removes cross-scene player-cache coupling

Published commit `1d2b1f8865b5ec12e57ea7dcc1ad25a8762eb63c` ran through
Steam 640820 as package SHA-256
`8102f48085bed0830f746c52042e5b05e6a603dc36de49c556b052ec30863e71`,
DLL SHA-256
`65c530ec491759987d026d86cb4400197eccd209cdb2ba641e774940edd22925`,
and MVID `f420093c-fef2-4a76-ad47-21e79bbc5c2b`. Evidence
`20260831T0637014594621Z-gunslinger-outfit-production-motion` failed closed
after four clean male records when pistol preparation changed the boundary to
`player=true;party=0;turnBased=true;units=2`. No partial image is accepted.

The faction pair was isolated, but a second installed-IL review disproved the
initial global-bookkeeping explanation. The actor was still created in the
main character's cross-scene holding state. `Player.UpdateCharacterLists`
enumerates that state, and `Player.AddCharacterToLists` adds every in-game,
non-detached, non-ex-companion unit to `m_ControllableCharacters` without a
faction predicate. `Player.UpdateIsInCombat` then reads those groups, and
turn-based state reads `Player.IsInCombat`.

The replacement uses `PersistentState.LoadedAreaState.MainState`, requires it
to be the loaded live scene and distinct from `Player.CrossSceneState`, and
recomputes and compares the exact controllable and cross-scene reference sets
at fixture, tick, capture, reconciliation, and cleanup boundaries. Actor and
target must both remain area-local and absent from controllable characters.
Installed-reference compilation and all `1369/1369` tests pass. Candidate
score remains 88/100.

Clean Release packaging and strict package/firearm/audio validation pass. The
first preflight reported only the known artifact-tree stabilization sentinel;
the unchanged rerun passed all 169 checks. Pre-commit package SHA-256 is
`2c6bdf7ffe6901ef33ddf5ab908e195cb3ce0675d93fc974b8c2798de9a30077`,
DLL SHA-256 is
`81a315c486dae914ec04c63bd0079be1780c626d5031416c0f5c0c0d7ecf6651`,
and MVID is `6ed1466d-9131-4b83-84e6-5f86c156a20f`. These dirty-tree
identities qualify the source/package checkpoint only.

Exact next action: commit and policy-publish this scene-state correction,
rebuild its exact commit, and rerun all 54 records.

## 2026-08-31 - Motion attempt 6 separates live scene identity from persistence ownership

Published commit `27bc24ae9ce5b84d3eb8760741833697ed52a911` ran through
Steam 640820 as package SHA-256
`9c97279edf78fb4f7540667b3e983b2c5b5b0b5ec98604c3fdea3b0e4bec3413`,
DLL SHA-256
`37c764f27e63f984fd09b9ec80d465372e997e693269716b8b61e66f07eb98a3`,
and MVID `38f7a207-baa3-4ee8-8774-c8d3de192b92`. Evidence
`20260831T1215532823796Z-gunslinger-outfit-production-motion` failed closed
before record 1: the male-Human view attached, remained weapon-empty, and
preserved the exact clean player lists, but its native DollData entities and
hair stayed absent through the bounded settle window. No image is accepted.
The then-current global-unit cleanup assertion passed, but did not inspect the
area container's own entity list; no save API ran and the process exited.

The player-boundary correction was effective, but using
`AreaPersistentState.MainState` as request-local ownership was not. Installed
IL shows `SceneEntitiesState.AddEntityData` needs no area registration,
`IsSceneLoaded` derives only from the container's scene-name string, and
`EntityDataBase.Dispose` does not remove holding-state membership. The prior
path therefore both failed to reproduce the proven doll lifecycle and left a
disposed entry in the save-backed area container unless separately removed.

The replacement creates a disposable `SceneEntitiesState` with the exact live
`MainState.SceneName` and `SkipSerialize=true`. Actor and target thus retain
the active Unity scene's rendering/navigation context while belonging to
neither area persistence nor `Player.CrossSceneState`. Native
`RemoveEntityData` now clears holding state and disposal ownership for every
fixture; the container must be empty between genders and empty plus disposed
at cleanup. Exact controllable/cross-scene snapshots remain mandatory.

Installed-reference compile, repository validation, all `1369/1369` tests,
clean Release packaging, strict package/firearm/audio validation, and the
settled 169-check preflight pass. The first preflight reported only the known
artifact-tree stabilization sentinel. Pre-commit package SHA-256 is
`64d07b6d3aa843aefb185cd2a07e4dce860ea46e522770e9eff7e9d16988981e`,
DLL SHA-256 is
`582e306bae50394eca161705b425c847bc08ba36e59ab23b36ac6fdfdd91a0d3`,
and MVID is `43f248b1-be23-43f8-aaf9-78cb02a8f9cd`. Candidate score remains
88/100.

Exact next action: commit and policy-publish the request-local loaded-scene
container, rebuild its exact commit, and rerun the complete 54-record matrix.

## 2026-08-31 - Motion attempt 7 detaches the readiness-only attack probe

Published commit `b27438c7fd38d4e588a47b05b5e2329fb3676932` ran through
Steam 640820 as package SHA-256
`788dcf4d89fac23941f79d9cca54db5f673bb5405125ca5e8817ef24553056e8`,
DLL SHA-256
`9a0d1a9d671697f9a5a46c366cb6fe29af83dc528530e805987c55791ff21456`,
and MVID `60f2fd26-9d78-401d-94d1-69a1c393afbe`. Evidence
`20260831T1253077289617Z-gunslinger-outfit-production-motion` failed closed
after 10/54 male records. Locomotion, turn, and the complete pistol attack
executed; the pistol acted and discharged exactly once. The following musket
ready sidecar then showed zero loaded rounds, total fired count two, and a
live `UnitAttack`; construction of the real musket evidence command was
correctly rejected as unloaded. No partial image is accepted.

The request-local scene repair is validated by this run: guard, exact game and
loaded-build identity, working-save/no-save boundary, production-blueprint
immutability, native cleanup, exact player lists, empty/disposed request-local
scene, and automatic exit all passed. Every partial sidecar retained the exact
outfit, hair, rig, player-neutral faction, non-controllable holding-state, and
unchanged player combat boundary.

Installed IL identifies the remaining harness error. `UnitAttack.Init` performs
native attack planning and computes approach radius; `UnitCommands.Run` calls
that initialization and then registers the command. The readiness probe had
been run before target positioning, so subsequent live ticks could advance it
through the acted frame and consume its round. The replacement calls native
`Init` directly, proves the probe is absent from actor commands throughout
target placement, and reserves `Run` for the separately constructed evidence
command. Every attack sidecar and terminal attack outcome must now report the
detached-probe boundary.

Installed-reference compile, repository validation, all `1369/1369` tests,
clean Release packaging, strict package/firearm/audio validation, and the
settled 169-check preflight pass. The first preflight reported only the known
artifact-tree stabilization sentinel. Pre-commit local-runtime package
SHA-256 is
`31498b7bed5b9532d0a208cda645b744cdfefa30b2a7246fab472696da7f0ce1`,
DLL SHA-256 is
`877b451e4a4a62751b3d1b75c217e24c2b66c857ec4d973e28bdfc5e23ef100d`,
and MVID is `2ada3432-6aa8-4a77-81b1-934fe1a698f0`. Candidate score remains
88/100.

Exact next action: commit and policy-publish the detached-probe repair, rebuild
its exact commit, and execute the complete replacement matrix.

## 2026-08-31 - Motion attempt 8 waits for native attack retirement

Published commit `5d520bbccaff98e09a9a94c3fa2c59811cd2f0ca` ran through
Steam 640820 as package SHA-256
`a703f089ff28cc83c3d835df36de1180950d668b5230a6bcef9a7cc9fcf7eb6b`,
DLL SHA-256
`7e8c1619acec69da73f10f6e5f3f6089a5d571163077fe9533193c4976763548`,
and MVID `8b7060eb-cefe-4ac9-8be1-62d61a0e1974`. Evidence
`20260831T1330408485246Z-gunslinger-outfit-production-motion` failed closed
after 10/54 male records. Both pistol and musket preparation reported a
detached readiness probe, positively validating attempt 7's repair. The
pistol acted and fired once, but its update-36 sidecar still contained a
running `UnitAttack`. The next musket-ready sidecar then reported zero loaded
rounds, total fired count two, and that same active command; production
correctly rejected a new unloaded musket attack. No partial image is accepted.

The installed log records an equipment-animation null reference immediately
after the pistol discharge and a second round consumption after the weapon
switch. Installed IL explains the sequence: base `UnitCommand.IsInterruptible`
is false while a started, acted animation has not finished, and
`UnitCommands.InterruptAll(true)` retains a non-interruptible command. The
prior harness declared evidence complete at update 36, attempted ordinary
interruption, then removed the weapon and target while that command remained
live. The surviving pistol command subsequently acted against the newly
equipped musket.

The replacement separates visual-evidence completion from native-command
retirement. It waits within the existing 360-update bound until the command is
finished or `IsInterruptible`, records the exact retirement state and update
count, and requires `retirementReady=true` in every terminal attack outcome.
After native interruption, teardown fails before changing combat, target, or
equipment state if any actor command remains running. Per-frame sidecars now
list running command types, and inter-action cleanup independently requires
zero running commands. Focused source tests enforce the order.

Installed-reference compile, repository validation, all `1369/1369` tests,
clean Release packaging, strict package/firearm/audio validation, and the
settled 169-check preflight pass. The first preflight reported only the known
artifact-tree stabilization sentinel. Pre-commit local-runtime package
SHA-256 is
`17d46838be9b31b3fecda29ef582f2aae2cfc422e2f5c25be41f3d58811f2dbb`,
DLL SHA-256 is
`e1b154a9e2c35348d6b6d67cd9fa8274c4764ffa5604335a24e680ada14b5844`,
and MVID is `bbd56913-905f-4d32-8546-cc3926bdaa2f`. Candidate score remains
88/100.

Exact next action: commit and policy-publish the retirement gate, rebuild its
exact commit, and execute the complete replacement matrix.

## 2026-08-31 - Motion attempts 9-10 expose a finished resident command

Published commit `0dbdaf2b283bbb6245939d4078c26f90d94d01ff` ran through
Steam 640820 as package SHA-256
`a8a6ae85f171e1c5140f17794830b0d11b64b4154af21a755332dd784ee570ca`,
DLL SHA-256
`585e4abf748225398f13c02afbd62313e2111fd46137cd57415af331925efd40`,
and MVID `a6768c5d-46e6-4fef-b45c-c2b958989d4e` in both runs. Evidence
`20260831T1401393847532Z-gunslinger-outfit-production-motion` failed before
record one when the male Human native doll intermittently did not populate
DollData or hair inside the bounded settle window. Exact guard, loaded build,
no-save, immutable-blueprint, request-local cleanup, and automatic-exit gates
passed. The same unchanged commit had passed this pre-action boundary in the
two preceding runs, so one controlled retry was evidence-supported; another
identical miss would have required narrower readiness instrumentation.

Evidence `20260831T1407213494923Z-gunslinger-outfit-production-motion` passed
that boundary and reached 10/54 male records. The pistol outcome reached the
new retirement-ready gate after acting and firing once. Its update-36 sidecar
had shown a running, non-interruptible `UnitAttack`; the subsequent
musket-ready sidecar showed that no command was running, yet still listed a
resident `UnitAttack`, zero loaded musket rounds, and total fired count two.
The fresh musket command was correctly rejected as unloaded. No partial image
from either failed batch is accepted.

Exact installed `UnitCommands` IL resolves the apparent contradiction.
`InterruptAll(bool)` skips an entry whose `IsFinished` is already true and
does not clear its raw command slot. The public
`RemoveFinishedAndUpdateQueue()` method clears finished raw slots and then
updates the queue. The retired pistol command therefore passed the prior
zero-running gate while remaining resident, later resumed during the weapon
transition, and consumed the musket round.

The replacement rejects a pre-existing queue, performs ordinary interruption,
calls native `RemoveFinishedAndUpdateQueue`, and proves the evidence command is
absent and the complete command container has zero running, resident, or
queued entries before weapon, combat, target, or outfit teardown. Sidecars and
terminal attack outcomes carry resident/queued command types and
`slotEvicted`; inter-action cleanup independently requires the same empty
container. Focused tests enforce the native eviction-before-teardown order.

Installed-reference compile, repository validation, all `1369/1369` tests,
clean Release/package construction, strict package/firearm/audio validation,
and the settled 169-check preflight pass. The first preflight reported only
the documented artifact-tree stabilization sentinel. Pre-commit local-runtime
package SHA-256 is
`fca9cf06fb1fb6a3e967eb7414c3ffb4ac679d639695ab81faf146788921e274`,
DLL SHA-256 is
`1d2d17ffe350388308fab4aa62d81637d378ce44c2408ec8c2d34c365a3a418a`,
and MVID is `d983a009-2e6c-41aa-ba32-56b9c20487f9`. Candidate score remains
88/100.

Exact next action: commit and policy-publish the finished-slot eviction
repair, rebuild its exact commit, and execute the complete 54-record matrix.

## 2026-08-31 - Motion attempt 11 requires doll-attachment lifecycle evidence

Published commit `4ef28f65577d09329536a905976b405cac4562ef` ran through
Steam 640820 as package SHA-256
`6f849b89c4ffba745585d268c1a1ff12c83074b2e5f80d13853e91e3c6c77a34`,
DLL SHA-256
`871a89190537624f150356e381b106cb162b70a215936c780913642096cb01c4`,
and MVID `10e8676b-e8d8-48f4-b4a1-210d0afe0d2f`. Evidence
`20260831T1438053243232Z-gunslinger-outfit-production-motion` failed closed
before record one because the male Human native doll did not populate its
DollData outfit or hair inside the bounded settle window
(`doll=False;hair=False;noWeapon=True;active=.`). No image exists or is
accepted. Guard, exact game and loaded-build identity, the disposable working
save with no save writes, production-blueprint immutability, exact structural
cleanup, empty/disposed request-local scene, and automatic exit all passed.

This is the second occurrence of the same pre-action empty-doll boundary
(attempts 9 and 11, with attempt 10 passing it), so an unchanged retry is not
permitted. Installed IL shows that `DollData.CreateUnitView(false)` creates the
native character and synchronously resolves and adds every equipment-entity
ID before ownership transfer; `SpawnEntityWithView` queues attachment, and
`UnitEntityView.OnDataAttached` later obtains and starts the view's
`Character`, then updates and rebuilds its outfit. The prior evidence could
not distinguish an empty template from state lost or replaced during that
attachment transition.

The replacement captures the native `Character` at four bounded points:
immediately after `CreateUnitView`, after `SpawnEntityWithView` but before the
next entity tick, after data attachment, and at a settle timeout. Each record
includes `ResourcesLibrary.Preloading`, avatar instance identity,
raw/active/saved entity counts, expected DollData ID count, active names, and
whether the attached avatar is the original template. Cleanup clears every
new reference even on fallback. Focused source tests require the four records
in lifecycle order.

Installed-reference compile, repository validation, all `1369/1369` tests,
clean Release/package construction, strict package/firearm/audio validation,
and the settled 169-check preflight pass. The first preflight reported only
the documented artifact-tree stabilization sentinel. Pre-commit local-runtime
package SHA-256 is
`aa512f88878ef88d7486176080552f6ff3ac237f540a3f042d49d75842227112`,
DLL SHA-256 is
`0c97e7c7a7c450fa93fef6fcc42a523809302c9dc01934352ab06530cdc0583b`,
and MVID is `47392b4f-cbc0-450f-9b72-82b284e578c7`. Candidate score remains
88/100.

Exact next action: commit and policy-publish the lifecycle instrumentation,
rebuild its exact commit, and execute attempt 12 to localize the first state
transition that loses the native DollData entities.

## 2026-08-31 - Motion attempt 12 excludes autonomous clone AI

Published lifecycle commit `2e73bf3035860ffc940c31f4e5c090b0f5d5df2e`
ran through Steam 640820 as package SHA-256
`6d4e6b3aa27658e958f7010937a7d62e481988eb4eb5967fdc8d719dfbd94d5f`,
DLL SHA-256
`90c727bcbd90ac962e7dd406c6bbc0c8f16f55ac05ff4ba8f812a9ff0e1f205d`,
and MVID `6d5eafa3-919a-40fc-a39c-9206ab6ca58f`. Evidence
`20260831T1509405239304Z-gunslinger-outfit-production-motion` reached 10/54
male records and then failed closed because the musket command could not be
constructed after an unowned native attack consumed its round. Guard, exact
game/build, disposable working-save and no-save, immutable-blueprint,
request-local scene, exact cleanup, and automatic-exit contracts passed. No
partial image is accepted.

The new lifecycle evidence clears the prior doll hypothesis. Before spawn,
after spawn before tick, and after attachment, the same avatar instance
`-654738` remained present with `ResourcesLibrary.Preloading=False`, five raw
and active entities, zero saved entities, and four expected outfit IDs. The
active set was the naked body, face, eyebrows, short hair, and empty facial-
hair entity. No entity disappeared during attachment.

The musket-ready sidecar instead proved `loadedRounds=0`, total firearm
discharges two, a detached readiness probe, and no installed harness command,
while both resident and running collections contained `UnitAttack`. The
preceding pistol teardown had already passed its synchronous empty-container
gate, so this was not its retired command. Source and installed-API inspection
show that both request-local clones retained their donor `BlueprintUnit.Brain`;
`JoinCombat` plus `Engage` activated that NPC AI and allowed it to issue the
second attack before the harness did.

The narrow replacement sets `Brain=null` only on the disposable actor and
target blueprint clones. Native factions, combat state, commands, weapons,
animation, outfit, and the working save remain untouched. Before any attack
frame can count, the harness now requires `Commands.Empty` and zero resident
or queued commands; the separately created evidence `UnitAttack` must have no
`AiAction`. Sidecars expose clone-brain state and command `AiAction` ownership,
and terminal contracts require every recorded command to be harness-owned.
Installed-reference compilation, repository validation, all `1369/1369`
domain tests, clean Release/package construction, strict package/firearm/audio
validation, and the settled 169-check preflight pass. The first preflight
reported only the documented artifact-tree stabilization sentinel. Pre-commit
package SHA-256 is
`b3c73cf63e68fa3cb4aff086bd236accf3769e69f53a5afe7c259776139d76e2`,
DLL SHA-256 is
`3c95c8c5115135023023ff74d4c77cbc3aaf90ff7c0ca742c3e397e1741c839d`,
and MVID is `5a199838-48eb-49a2-8b92-7ca8d0dfabe2`. Candidate score remains
88/100.

Exact next action: commit and policy-publish the AI-isolation repair, rebuild
that exact commit, and execute attempt 13.

## 2026-08-31 - Motion attempt 13 gates native doll creation on resources

Published AI-isolation commit `934785962bb4ef752993add5558d20cb751f1c7d`
ran through Steam 640820 as package SHA-256
`a53c3314dd6aeb5d4ee13a8f0b5615d93325212062f1c5916ef0aa9460f88e5f`,
DLL SHA-256
`af2af437dd06f55c1316305190e02aee86f95a1d0f0c2364b48b4eb032c7fff1`,
and MVID `0e441834-5f14-41d6-b1ad-15d46b4f976e`. Evidence
`20260831T1548069712324Z-gunslinger-outfit-production-motion` failed closed
before record one. Exact guard/game/build, disposable working-save/no-save,
blueprint immutability, request-local scene, structural cleanup, and automatic
exit contracts passed. No image exists or is accepted.

The lifecycle record now identifies the empty-doll cause rather than merely
its symptom. `ResourcesLibrary.Preloading` was true before spawn, after spawn,
and after attachment; the same avatar instance had zero raw, active, and saved
entities despite four expected DollData IDs. Preloading became false by the
settle timeout, but the already-created avatar remained empty.

Exact installed IL shows `DollData.CreateUnitView(false)` loops its entity IDs
and calls `ResourcesLibrary.TryGetResource<EquipmentEntity>(id, false)`.
`TryGetResource` immediately logs and returns null when global preloading is
true and the ignore flag is false. `CreateUnitView` adds those null results and
has no later retry. Extending the post-creation settle window therefore cannot
repair the object. Installed game code elsewhere waits until
`ResourcesLibrary.Preloading` is false before resource-dependent work.

The replacement waits before any clone or DollData view is created, with a
360-update bound and an explicit timeout. It hard-checks the flag again at the
creation line, records wait length and creation-time state in both production
fixture types, and makes `gatePassed=true` plus
`resourcePreloadingAtDollCreation=false` terminal requirements. This is a
sequencing guard only; it neither preloads assets nor weakens the existing
native-doll settle contract. Installed-reference compilation and all
`1369/1369` tests pass, as do clean Release/package construction, strict
package/firearm/audio validation, and the settled 169-check preflight. The
first pass reported only the documented artifact-tree stabilization sentinel.
Pre-commit package SHA-256 is
`8aba976c9550a3c09b95539dee11d7825362169b0933b546837cb2e34d25c378`,
DLL SHA-256 is
`379f0bc2a1612065b3ae53539b391f11ac20161be18b4a0dfb0f47bba8803a89`,
and MVID is `5a3b66e8-97b3-4d55-b7e0-db500ca82c96`. Candidate score remains
88/100.

Exact next action: commit and policy-publish the pre-creation resource gate,
rebuild its exact commit, and execute attempt 14.

## 2026-08-31 - Motion attempt 14 completes the matrix and exposes a stale predicate

Published resource-gate commit
`4447ebd679aaf55058958a52b69ba9ac4b00effb` ran through Steam 640820 as
package SHA-256
`028fd526db9656a4952d3343e0f08453343ff6a5614d53acad475f0e23eff833`,
DLL SHA-256
`348b19e22d598cb5a818ff7847a6e7a896966ab2380bad710318ab85c90585c2`,
and MVID `2cedc19a-8bac-4655-b0ed-03e02e98b3a4`. Evidence
`20260831T1608345329020Z-gunslinger-outfit-production-motion` completed both
fixtures and all 54 records/216 labelled views. Native locomotion, turn, six
attacks, two production reloads, restoration, combat reconciliation,
blueprint immutability, request-local cleanup, no-save, and automatic exit all
passed. Both creation gates reported `passed=true`, preloading false at the
creation line, and zero wait updates.

The terminal result remained `FAIL` only because the fixture aggregate
required `GetAction(LocoMotion).Clips.Count > 0`. Both exact male and female
runtime managers expose locomotion through the native action surface without
populating that generic clip list; the live `UnitMoveTo` outcomes nonetheless
proved accepted paths, velocity, displacement, and distinct walk/run speeds,
and the captured frames showed the poses. Existing repository exact-game code
already treats action presence, not a positive clip-list count, as the
locomotion surface contract. This is a stale harness predicate, not an outfit
or animation failure.

Independent reconciliation matched every index and sidecar hash, byte count,
fixture/action/state identity, and meaningful-pixel count: 54 unique PNGs, 54
sidecars, 27 records per gender, and 216 views. Index SHA-256 is
`278cce94824eaae17a5886071221aa54eb541108900030346f086b661ad2fc66`;
the filename-sorted `name:sha256` PNG-set digest is
`043f3dd3d8cd2bbba09dc035067dd0e110b19ac6fab41c67ab1a4ef8813605cd`.
All 54 four-view sheets were directly inspected outside the repository. The
selected outfit remained complete and coherent through every pose; there was
no missing geometry, outfit/body clipping, detached accent, or weapon-induced
garment break. A native live-combat outline appears in some frames but does
not alter or obscure the kitbash. Because the scenario status is not terminal
PASS, this review is recorded but the batch is not yet accepted as qualifying
motion evidence and the score remains 88/100.

The narrow repair records and requires `locomotionActionPresent`, retains
`locomotionClipCount` as informational evidence, and leaves all live movement
outcome predicates intact. Installed-reference compilation, repository
validation, all `1369/1369` tests, clean Release/package construction, strict
package/firearm/audio validation, and the settled 169-check preflight pass.
The first preflight reported only the documented artifact-tree stabilization
sentinel. Pre-commit package SHA-256 is
`3c6cc236fc0e84b1da02616bcafe15eb82c427c6b8ea7e1f4ffc1ddbea285b49`,
DLL SHA-256 is
`5d46a1faeb471014841af5732244ad64e22b3c15ae935fc28fb950119a68c2f1`,
and MVID is `6b1c2eb8-6a9a-41d2-b15e-de3d1df503ef`.

Exact next action: commit and policy-publish the corrected action-surface
contract, rebuild that exact commit, and execute attempt 15. Accept motion
only after its terminal PASS and a fresh reconciliation of the replacement
batch.

## 2026-08-31 - Motion attempt 15 accepted

Published repair commit `c22b103b4080b7ac88b1893d7940e1f1fda5ec71`
ran through Steam 640820 as package SHA-256
`0358326d581583563fcf83dab9b38c7a58d36447a387a1bd93737a035a8c61c3`,
DLL SHA-256
`4f04f64d1f7fdc96d70a7b355fb57bef28f0d8a6679cf8b0f98e4b3910d88c94`,
and MVID `8fc3629c-9fb3-41f6-961a-6b35e46d442b`. Evidence
`20260831T1635361787487Z-gunslinger-outfit-production-motion` reached terminal
`PASS`; every assertion passed with 54 records, 216 labelled views, and 109
scenario evidence files.

Both exact Human fixtures proved their pre-creation resource gate, production
outfit, humanoid rig, non-null locomotion action, 39 main-hand attack clips,
disabled disposable brain, request-local scene ownership, and exact player
lists. The generic locomotion clip count remained zero as expected. Male
walk/run maxima were 1.2191999/3.047531 with 0.764907241/0.781484246 metres of
displacement; female maxima were 1.62559927/2.8448 with
0.8196764/0.856158555 metres. Both turns reached exactly 90 degrees. All six
native attacks started, ran, animated, acted, retired, and evicted their
command slots with no `AiAction`; each firearm attack discharged exactly once
with zero faults. Both update-240 production reloads ran their execution
process to completion, consumed exactly one powder and ball, loaded one round,
and returned `Success` with zero faults. Avatar/movement/ammunition restoration,
combat reconciliation, blueprint immutability, request-local disposal,
working-save identity, no-save, and automatic exit all passed.

Independent reconciliation matched every PNG and sidecar to its index hash,
byte count, fixture/action/state identity, and positive meaningful-pixel
count. Counts are 54 unique PNGs, 54 sidecars, 27 records per gender, and 216
views. Index SHA-256 is
`0e3ae7eb6cd73642c7c77dc127ce55eb756356ad62aae111a86f956bf1bf01df`;
runtime-result SHA-256 is
`cac0f8b73031d19f6f9c2dd2879612ad796bf021f1ad5bf0dbd0bac7411435d0`;
runtime-evidence SHA-256 is
`cecb82b3ab2d946ab8340dfac359bcee9a0557e02b22a258810f9ce851b287ac`;
and the filename-sorted PNG-set digest is
`9d3e90e23dceebd164f96452009b7b154b0c8eb58331dc0bb8a2d52bc8bd7975`.

All 54 replacement four-view sheets were directly inspected. Male and female
base/accessory geometry remained complete and coherent through unarmed idle,
musket walk/run/turn, pistol and musket native attacks, production reload,
and shortsword melee. There was no body/outfit clipping, missing part,
detached accent, or pose-specific silhouette failure. A native combat outline
appears in some female hostile-action frames but neither changes nor obscures
the outfit. Motion/fire/reload/melee qualification is accepted.

The score remains the conservative 88/100 because the final five
compatibility points were defined as an indivisible motion-plus-persistence
block; motion has passed, but persisted outfit reconstruction and the required
respec-like path remain open. Exact next action: build the guarded persistence
qualification against the mission's save/load and rebuild boundaries, then
run and directly inspect its complete evidence matrix.

## 2026-08-31 - Persistence and native respec accepted

The guarded persistence implementation is a three-fresh-process transaction:
`gunslinger-outfit-production-persistence-prepare` creates one exact
marker-bound disposable actor through native Fighter-to-Gunslinger Respec,
captures it before any save call, and writes exactly
`KMG_AUTOMATION_WORKING` once;
`gunslinger-outfit-production-persistence` loads that exact marker, proves
the deserialized class outfit before activation, activates it only through
native view APIs, captures loaded and reconstructed states, runs new male and
female native Respec fixtures, removes the marker, restores all snapshots, and
writes the working save exactly once; and
`gunslinger-outfit-production-persistence-verify-absent` performs a final
fresh load with zero writes and requires the marker to be absent from global,
party, party-character, remote-companion, and cross-scene state.

Installed 2.1.7b IL established the required native path. `CharBuildMode`
uses `Respec=3`; a level-zero replacement is required for first-level class
replacement; the native commit writes `Doll.CreateData()`; and
`UnitEntityView.UpdateClassEquipment` is guarded by
`UseClassEquipment` plus its cached equipment class. The harness therefore
uses distinct real Fighter source/replacement actors, sets serialized
`ForcceUseClassEquipment`, invalidates only the stale view equipment-class
cache after commit, and calls the public native update/rebuild path. A hard
pre-save gate refuses to persist anything unless class, outfit pair, 2/22
palette, rig, level transition, starter-grant rollback, and marker identity
are exact.

The accepted transaction used deployment
`runtime-evidence/deployments/20260831T2042133344215Z/deployment.json`:

- package SHA-256
  `fd264483efa7eafd93520a9bd95431142f120e8b3f981b4fc17e48b6d12fbbb5`;
- DLL SHA-256
  `a935db30e597c482a155bbba3fe9e207db78bbbb6e823acb3c6eeca313078acf`;
- DLL MVID `6e07322d-0bc4-4f2c-96a8-f89f4b75c8eb`;
- source-state SHA-256
  `15448540eb44a5a52821e2d165061a82d0be345baca7e5650b1d4e1f8e3a8827`;
- exact installed Assembly-CSharp SHA-256
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`
  and MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.

Prepare evidence
`20260831T2042133984189Z-gunslinger-outfit-production-persistence-prepare`
passed 7/7 assertions, captured one exact male Human native Respec record
before the save API, and performed one exact save. Verify evidence
`20260831T2044530382564Z-gunslinger-outfit-production-persistence`
passed 10/10 assertions and four records: persisted-loaded,
persisted-native-class-reconstruction, native-respec-male-human, and
native-respec-female-human. Each record retained the exact gender-specific
production pair, no historical Fighter clothes, exact 2/22 palette, humanoid
rig, absent serialized class clothes, and an unchanged production blueprint.
It then removed the marker and performed one exact cleanup save. Final
evidence
`20260831T2047331339967Z-gunslinger-outfit-production-persistence-verify-absent`
passed 5/5 assertions with the original three-character fingerprint, zero
marker memberships, and zero writes.

Independent reconciliation matched all five sidecars to all ten PNG hashes,
byte lengths, labels, and exact appearance fields. All ten preview-like and
ordinary-isometric images were directly inspected. Prepared, loaded,
reconstructed, and fresh male states are visually identical; the female uses
the intended gender-specific pair. Both retain complete bodies, attached
components, the navy/red officer-privateer palette, and legible isometric
silhouettes with no Fighter remnant, palette drift, material failure, or rig
deformation.

The final compatibility criterion advances from 15/20 to 20/20. Final score:
`93/100 (26/23/20/15/9)`, above the 75-point threshold with no hard
rejection.

## 2026-08-31 - Final local qualification

The completed tree passed the required local gate sequence:

1. `.\scripts\validate-repository.ps1` - active 0.0.110 and all inherited
   validators pass;
2. `.\scripts\test-domain.ps1 -Configuration Release` - 1370/1370 pass;
3. `.\scripts\build.ps1 -Configuration Release -Clean -SkipDomainTests` -
   clean installed-reference Release build and output validation pass;
4. `.\scripts\Build-Local.ps1` - repository validation, a second clean
   1370/1370 run, exact private-reference Release compilation, icon checks,
   SoundBank/firearm validation, deterministic packaging, and strict
   standalone/local-runtime package validation pass;
5. `.\scripts\Test-RuntimeScenarioPreflight.ps1` - the documented first
   post-package run reported only
   `unsupported-does-not-build-or-stage-package`; the unchanged second run
   passed all 178 checks.

Standalone and local-runtime archives are byte-identical, contain 135 entries,
and have SHA-256
`4f3e7d97b0be0e3fb3636b4037642b657f2ade6f4d932a6b99f411911e0397d0`.
Archive inspection found zero game-assembly, raw-catalog, reference-image,
screenshot, runtime-evidence, or CSV matches. The DLL remains byte-identical
to the fully runtime-qualified artifact, SHA-256
`a935db30e597c482a155bbba3fe9e207db78bbbb6e823acb3c6eeca313078acf`;
only package-facing README/CHANGELOG text changed the archive hash.

Implementation/evidence commit
`92de86005d2db4f0731b296fc3d803033e15112a` was published only through the
approved helper. `HEAD`, the local branch, and the origin feature branch were
identical afterward and the tree was clean. This final documentation-only
closure is also helper-published before handoff, whose final SHA check is the
authoritative self-publication record. No engineering or qualification gate
remains.
