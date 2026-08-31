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

- the deterministic audit and accepted Human shortlist render are complete;
- `magus-complete` is the provisional 81/100 finalist;
- the guarded, production-free finalist race/gender matrix is source-qualified
  and package-qualified, with installed-game execution still pending;
- production class clothing remains unchanged until the race, overlay, motion,
  rebuild, and persistence gates pass.

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
