# Gunslinger Outfit Kitbash Qualification

Status: not qualified. This document is a live gate ledger, not an acceptance
claim.

## Authority

- Intake baseline: `5949165e2a6407ca480d46cd86d8944e4152e2fb`
- Feature branch: `codex/gunslinger-class-outfit-kitbash`
- Intake version: `0.0.110`
- Runtime target: Steam-preserving Pathfinder: Kingmaker 2.1.7

## Gate ledger

| Gate | State | Required evidence |
|---|---|---|
| Installed API contracts | Pass (audit stage) | Exact installed 2.1.7b assembly identity and public/reflected member findings recorded |
| Native class/resource catalog | Pass | Guarded run `20260830T2012181937219Z`, candidate set `dd81603f...03357` |
| Serious candidate renders | Pass (Human stage) | Accepted outer/in-game PASS for 48 cases/96 directly inspected images |
| Best-three scoring | Pass (selection stage) | Magus 88 after accepted coverage/static equipment, Rogue 75, Slayer 70; motion/persistence points remain withheld |
| Race/gender coverage | Pass (selection stage) | Accepted guarded 9-race x 2-gender matrix and direct review of all 72 PNGs |
| Color ramps | Pass (systematic sample) | Native 2/22 and one valid alternate applied to both entities in every 9x2 cell |
| Body/material integrity | Pass (no-weapon selection stage) | All 72 race/gender/palette preview/isometric PNGs directly accepted; equipment/motion gates remain separate |
| Animation/weapon fit | Partial | Static pistol/musket/blunderbuss fit passes; idle/walk/run/turn/fire/reload/melee evidence pending |
| Equipment overrides | Pass | Guarded 32-state/64-image light/heavy armor, headgear/hair, cloak, backpack, inactive-weapon and removal matrix |
| Preview/gameplay paths | Pass (selection stage) | Four-view preview-like and ordinary isometric evidence across the complete 9x2 grid |
| Save/load/rebuild | Partial | Commit-bound working save load and request-local repeated rebuilds pass; persisted outfit and respec-like reconstruction pending |
| Focused tests | Pass (static-runtime checkpoint; repeat final) | Exact catalog/validation/defensive-copy, atomic wiring, and guarded compatibility contracts |
| Repository validation | Pass (static-runtime checkpoint; repeat final) | Active 0.0.110 validator with 1369 current tests |
| Complete domain suite | Pass (static-runtime checkpoint; repeat final) | 1369/1369, clean Release run |
| Clean Release build | Pass (production checkpoint; repeat final) | Exact installed-reference Release construction |
| Installable package | Pass (commit-bound static checkpoint; repeat final) | Strict standalone validation, SHA-256 `e15546c5...394` |
| Compatibility profiles | Pending | Exact applicable command/result |
| Guarded runtime smoke | Pass (static checkpoint; repeat final) | Canonical load plus accepted production matrix `20260831T0344513197562Z` with exact commit/build/save correlation |
| Publication | Pass (static source checkpoint; repeat final) | `59eb7a9...566` published by helper; HEAD/local/origin refs identical |

## Guarded catalog evidence

Command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario gunslinger-outfit-audit `
  -ExpectedVersion 0.0.110 `
  -ExitAfterCompletion:$true `
  -AllowDirtyGit `
  -Confirm:$false
```

Passing run: `runtime-evidence/20260830T2012181937219Z-gunslinger-outfit-audit`.

- result: PASS, all nine assertions;
- loaded mod: `0.0.110`;
- loaded game contract: supported version `2.1.7b`, exact
  `Assembly-CSharp.dll` SHA-256
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`,
  MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`;
- dynamically discovered race IDs: Aasimar, Dwarf, Elf, Gnome, Half-Elf,
  Half-Orc, Halfling, Human, and Tiefling, each inventoried for male and female;
- sources: 49 class, 163 item-linked, 361 bounded raw;
- inventory: 1,206 unique loaded equipment entities, 3,816 matrix rows,
  4,878 resolved links, zero unresolved links, zero inspection errors;
- state: no save-owned state, inventory, progression, or avatar mutation;
- deterministic candidate-set SHA-256:
  `dd81603f583444f335381d72cc69b73f1c036c4625e8227cb1e1f9db18603357`;
- ignored catalog SHA-256:
  `73af097a4dd21fe905d2f9b4388f2ef6a68503f4b6723040e1dd00d3e3e2e294`.

Two preceding FAIL runs are retained as diagnostic evidence. Both failed
closed, exited automatically, had zero game exceptions, and led to narrower
instrumentation. They are not acceptance evidence.

The passing launch pipeline also recorded repository validation PASS, complete
domain suite PASS (`1362/1362`), compilation PASS, and strict standalone package
validation PASS. These are audit-checkpoint checks and must be repeated after
production implementation.

## Guarded renderer source qualification

The first serious batch fixes six native class presentations and 32 exact
gender-specific IDs: Bard, Alchemist, Magus, cap/cape-free Ranger,
cap/cape-free Rogue, and cap-free Slayer. Investigation code excludes the
structurally unsafe caps/capes, uses disposable Human actors, captures native
and alternate valid ramps with no weapon/pistol/musket, restores exact avatar
state, and verifies cleanup.

At 2026-08-30T21:05:28Z, .\scripts\Build-Local.ps1 passed repository
validation, all 1365/1365 tests, exact-reference Release construction,
deterministic packaging, and strict validation. Standalone and local-runtime
packages have SHA-256
693c09684256fab77b4835b78eff12ab974c2bc460a63824f877768cd9c16ce8;
the staged DLL SHA-256 is
17bfe03b52e85cab627be425c680b1ccf6db88275ba4e253081065685304e377.
Runtime preflight passes 160 checks.

The initial runtime invocation was rejected before deployment or game launch
because the harness requires a clean Git state. It is not visual evidence.
Candidate-render gates remain pending until a clean published checkpoint is
run and the images are directly inspected.

The first clean published attempt is retained at
runtime-evidence/20260830T2109519221444Z-gunslinger-outfit-candidate-render.
It loaded commit 189ae46fa19552fa3b906740d9f30372c588f7f5 through
Steam, then failed closed at request acceptance with
scenario-timeouts-not-allowed. No hook, UI action, save action, render, or
score occurred. The missing in-mod working-save predicate entry was repaired
and covered by the focused guard test. After repair, all 1365 tests,
Build-Local, strict package validation, and the quiescent 160-check runtime
preflight pass. A new clean-commit runtime attempt remains required.

The next clean run is retained at
runtime-evidence/20260830T2119065677129Z-gunslinger-outfit-candidate-render.
It accepted the request, identified and loaded exactly
KMG_AUTOMATION_WORKING, reached a stable fingerprint with no save-writing API,
then failed closed before the first render because the otherwise valid male
pregen rig exposed no optional Progression equipment class. Cleanup passed,
automatic exit completed, and no candidate was scored. The renderer now uses
the reported class when available and the exact audited native Fighter class
otherwise, while recording original/donor entities and their intersection.
All 1365 tests, game-facing compilation, Build-Local, and strict package
validation pass after that repair. Installed candidate images remain pending.

The next run is retained at
`runtime-evidence/20260830T2130124467293Z-gunslinger-outfit-candidate-render`.
It loaded exact commit `9de7c4ef40483150ffba40782deb71714d2a0307`,
mod version `0.0.110`, and DLL SHA-256
`7fb96cd42ed986241fa63f79a52e01633da7c8b7bc18e1ed68d0a1562e4d5aac`
through Steam App ID 640820. Exact working-save correlation and a stable
fingerprint passed with no save-writing API.

The outer collector timed out at 21:32:28Z under the generic 120-second
deadline, approximately five seconds before the post-load fingerprint
completed. It left Kingmaker running and did not force terminate. The guarded
scenario then completed safely at 21:34:30Z, passed all ten assertions,
removed hooks, initiated automatic exit, and left no game process.

The terminal index records candidate set
`ef38c5c841510df7f03bbf68a8ca9e7fbef3f3403369022505449cb038d347be`,
six candidates, two Human gender fixtures, four cases per candidate/gender,
48 exact held states, 48 preview images, 48 isometric images, 24 palette
applications, and 12/12 restorations. It records
`saveApiCalled=false` and `productionBlueprintMutated=false`. All 96 images
were directly inspected from ignored local evidence. Eleven isometric images
were flagged low pixel density; paired four-view previews remained usable,
but this cannot close the final presentation gate.

Human-stage weighted scores are Magus complete 81/100, Rogue capless/capeless
75/100, and Slayer capless 70/100. Magus advances as the provisional finalist
because its native fitted torso, split waist tails, belts, bracers, and boots
best satisfy the swashbuckler/privateer brief. Bard, Alchemist, and Ranger
were below the shortlist for traveler-pack, apron/tank, and bedroll-heavy
wilderness identities respectively. No enumerated hard rejection is inferred
for the best three before the exhaustive matrix.

The collector now grants only this scenario a bounded
`max(request timeout, 600) + 15` seconds. The focused test requires that exact
ceiling, and all 1365/1365 Release domain tests pass. A clean validated
published checkpoint and accepting rerun remain required before the Human
render gate can close.
The full `Build-Local.ps1` gate then passed at 2026-08-30T21:54:14Z.
Standalone and local-runtime packages are byte-identical at SHA-256
`2f515302e2d0263adccb837b4e4f079d1120fcb0074054fae9ba4093aef76849`;
the DLL SHA-256 is
`7fb96cd42ed986241fa63f79a52e01633da7c8b7bc18e1ed68d0a1562e4d5aac`.
Runtime preflight first observed the immediate post-build timestamp-invariance
guard, then passed all 160 checks once outputs were quiescent.

Accepting rerun
`runtime-evidence/20260830T2158516580621Z-gunslinger-outfit-candidate-render`
used a clean published tree, `-TimeoutSeconds 600`, exact save
`KMG_AUTOMATION_WORKING`, automatic exit, and no force-termination option.
Outer orchestration reached `final-result-received` with PASS, and all 10
in-game assertions passed without exception.

The run loaded commit `8f47f2db723fdfe6146ca30c352ea83ba7d3589f`,
package SHA-256
`2fdaa2813262e237e687ce277d1a67cc56a8e8ce72c778dcdbd01da995b5d7f4`,
DLL SHA-256
`c9ace6013911f041e5e824c340b04e06d2b09a5a1bbdee5e123396853e0900c0`,
and MVID `5a02e6db-4452-4a75-a2cf-f836f98a3407`. It records exact
request/save/build correlation, no save interaction/write, no production
mutation, 48 records, 96 images, 48 exact held states, 12/12 restorations,
and automatic exit with no remaining Kingmaker process.

All 96 accepted images were directly inspected from ignored local evidence.
The candidate-set identity reproduced exactly and the scored order remained
Magus, Rogue, Slayer. Zero preview images and eight ordinary-isometric images
were tagged low density; the latter remain an explicit final-matrix concern.

## Finalist race/gender source checkpoint

Guarded scenario `gunslinger-outfit-finalist-race-matrix` is now
source-qualified. It discovers supported races from the installed progression
root rather than an assumed list, derives two gender cells per discovered
race, and validates that native Magus `LoadClothes` produces the exact
audited ordered entity pair in every cell. Each request-local native body is
captured with native and alternate valid palettes, no visible weapon,
preview-like four-view framing, elevated isometric framing, and exact
entity/ramp/saved-link restoration. The bounded expected result is 18
fixtures, 36 records, 72 images, 180 views, and 18 restorations.

Repository validation, game-facing compilation, all 1365/1365 tests,
`Build-Local.ps1`, deterministic packaging, and strict standalone and local
package validation pass. Package SHA-256 is
`cdd85e981f9847b0259a965506db457af98818d25aaf7c87d619022eae9559dc`;
DLL SHA-256 is
`36cf201fca3040c3a7b9a35f4253207d87b5480b3f13b1df14897860fdb02b7b`.
The unchanged preflight passed 163 checks after the known immediate-post-build
timestamp guard became quiescent.

This checkpoint does not claim installed-game race visual acceptance.
Steam-backed execution, direct image inspection, and separate equipment,
animation, production, and persistence gates remain required.

## First finalist matrix diagnostic

The clean published run at
`runtime-evidence/20260830T2237589386140Z-gunslinger-outfit-finalist-race-matrix`
used Steam App ID 640820, exact save `KMG_AUTOMATION_WORKING`, and commit
`fe86bce4484d45ca8f6a6f7070bfd7942fd5a0fc`. It loaded MVID
`985fe6cf-03f4-4120-9a8e-e586315c1135` and DLL SHA-256
`09fcb5096344aac82da288b8306b212b8b0dc44c9c7654f4d6515ff293a735a9`.
The outer and in-game result correctly reported FAIL after 122,368 ms.

Male Aasimar completed native-default and alternate palettes, producing two
records, four PNGs, ten views, and one exact restoration. Before female
Aasimar finalist application, the first deterministic native donor failed the
original-avatar exactness requirement. The gate stopped rather than applying
or scoring against an unproven body. Guard, working-save boundary, installed
Assembly-CSharp identity, cleanup, no-save behavior, no-production mutation,
and automatic exit passed. No Kingmaker process remained.

The retained donor inventory also showed a Medium initial female Halfling
source. The repaired selector now requires canonical size (Small for Gnome and
Halfling; Medium for all other installed player races), retains all exact
race/gender/size sources in deterministic order, and probes each source before
acceptance. The probe removes and re-adds the original avatar entities and
requires exact order, primary/secondary ramps, and saved links. A rejected
disposable source is recorded with its attempt index, identity, reason, and
mismatch details, retired, and followed by the next source. This does not
weaken restoration or touch a campaign actor.

After repair, repository validation, game-reference compilation, all
1365/1365 tests, clean Release build, package construction, strict standalone
and local-runtime validation, and the quiescent 163-check runtime preflight
pass. Pre-publication repair package SHA-256 is
`255de7da0529767b089d65fbd9638fb4964020a562797f1c6048d3315014c624`;
DLL SHA-256 is
`c9840e31c00997b9c6d50b6f6b044175cbe34165d3f00414ce90fc7781040bef`.
Full installed-game rerun and direct inspection remain open; the partial images
make no aesthetic or compatibility acceptance claim.

The clean published retry at
`runtime-evidence/20260830T2257046480918Z-gunslinger-outfit-finalist-race-matrix`
loaded commit `a27c4a7ecb061bf972df799ee096dc1b31e5e62d`, MVID
`7b7c4298-4aa5-4689-b317-23f15ccbfbc5`, and DLL SHA-256
`a4ffb80afa5a9574e5138bdd72b196708c23de9f7218cdb43322158f5ddbe1c7`.
It safely exercised all six deterministic female Aasimar donors and recorded
the same detail for each: avatar present, original entity count zero. The run
then failed closed with donor exhaustion. Cleanup, no-save behavior,
no-production mutation, and automatic exit passed.

Zero entities is a legitimate exact ordered baseline. The corrected probe
removes/re-adds it, requires the resulting sequence to remain empty, verifies
saved links are unchanged, records `originalEmpty=true`, and still fails a
null avatar. The exceptional cleanup path performs the same verification.
Nonempty entity order and ramp requirements are unchanged. All 1365 tests and
the clean strict package gate pass; pre-publication repair package SHA-256 is
`3b7e2deb7b96dac8e62eba66d1628af2355e0ab2c4ff4259ab245e5710b3168a`
and DLL SHA-256 is
`8621f5402e652fbdc1b3eb7d0657d0450f3f5c00cfd861a02961c5563cb0e46f`.
This remains instrumentation qualification, not race-grid acceptance.

The next clean published run at
`runtime-evidence/20260830T2309022972406Z-gunslinger-outfit-finalist-race-matrix`
loaded commit `5116a127d92ca09ea13e5822439e6b833b47c7e7`, MVID
`79cd33b7-76d0-4240-bd20-6bc51d4a5729`, and DLL SHA-256
`4fb8ef472bf4ac602b1861a0d1d42094d0995cdd3090bc7e8fee457ce04f26bb`.
It passed every mechanical matrix assertion except cleanup: 18 dynamic
race/gender fixtures, 18 exact donor probes, exact native links, 36 palette
records, 72 PNGs, 180 views, and 18/18 restorations. No exception, save call,
production mutation, or party delta was recorded.

The global unit set alone remained nonidentical after 360 cleanup updates.
Because the evidence did not yet identify the differing reference, the run is
FAIL and its images are not accepted. A diagnostic-only checkpoint now emits
expected/current counts and described missing/unexpected unit and party
references while retaining the exact cleanup condition. Compilation, all
1365 tests, clean Release/package construction, and strict validation pass;
pre-publication package SHA-256 is
`368140973c5e42aacf420168159b30b4a48fe26c7476984a282f621b529721f2`
and DLL SHA-256 is
`93edda11b82111e8a76c1c2298e7260ae142e8d1c68ba127e004b6cef7ea24aa`.

The resulting published diagnostic evidence is
`runtime-evidence/20260830T2323563433313Z-gunslinger-outfit-finalist-race-matrix`.
All 18 fixtures, 36 records, 72 images/180 views, native links, palettes, and
18 restorations passed again. The exact cleanup difference was one unexpected
unit and no missing units: `Leopard`, blueprint
`AnimalCompanionUnitLeopard` (`54cf380dee486ff42b803174d1b9da1b`), unique ID
`e8019935-e26e-4be8-a799-c00d8fb7a26f`. The global count changed 265 to 266;
the party remained exact at 3 and the disposable actor was absent. The run is
still FAIL and no images from it are accepted.

The next checkpoint retains whole-snapshot equality and adds no world-delta
heuristic. A unit is eligible for request-owned cleanup only when reached by
exact reference through the active disposable actor's native
`UnitDescriptor.Pet` property and absent by reference from the initial unit
snapshot. That exact dependent is recorded and retired before the actor; the
gate still requires the original global-unit and party sets. Repository
validation, installed-reference compilation, 1365/1365 tests, clean Release
build, and strict installable-package validation pass. Pre-publication package
SHA-256 is
`ddb92778082adc354b1e574abad9a467a10246c17cefa75ab61281f410feab62`;
DLL SHA-256 is
`af8262f6593053ceadf56af84c26e56e61d38964b816ed39896ce7b5f7885b39`.

The published cleanup run at
`runtime-evidence/20260830T2341080018300Z-gunslinger-outfit-finalist-race-matrix`
loaded commit `8b8d0b17aa90318425404efac56f6977bb2ad11c`, MVID
`3595f627-40de-4b76-830b-99920d2838ac`, and reached terminal PASS. It
completed 9 installed races/18 gender cells, 36 records, 72 PNGs/180 views,
18/18 exact restorations, exact 265-unit and 3-party-member snapshots, and
recorded no save or production mutation. Its request-owned
`AnimalCompanionUnitLeopard` relationship belongs to male HalfElf and was
retired exactly; this corrects the earlier inferred female-Elf association.

Direct visual inspection of all 72 PNGs rejects that batch. Several donor
prefabs rendered non-avatar clothing or equipment, including stored shields,
bows, quivers, capes, and large weapons. A mechanically empty avatar was thus
not a neutral player body. These images do not qualify race compatibility or
change candidate scoring.

The replacement fixture follows Kingmaker's native character-creation path:

- resolve the exact `BlueprintRace.Presets` entry for race and gender;
- configure `DollState` with that preset and the exact native Magus class;
- create `DollData` and its native `CreateUnitView(false)` player view;
- spawn a disposable same-race/gender descriptor with that view;
- clear every `UnitBody.AllSlots` item and both weapon-model channels;
- require a nonempty exact doll entity set with zero unexpected avatar entity;
- retain exact avatar restoration, global cleanup, no-save, and no-production
  gates.

Focused source contracts enforce each step. Repository validation, installed-
reference compilation, 1365/1365 tests, clean Release construction, package
creation, and independent strict package validation pass. The pre-publication
package SHA-256 is
`04f13af8fd17a0d9e18611e13c3cc3d27d83f6c7cf1e7dca3b05e094e5f73d18`;
DLL SHA-256 is
`d3ec07a2238ff2c062686dfc4e570ee602afaa716a26ddfa01607cb2627653bc`.
Runtime and visual acceptance remain open until a published commit-bound run
replaces and directly reviews the complete image matrix.

The first published neutral-doll qualification at
`runtime-evidence/20260831T0013309100348Z-gunslinger-outfit-finalist-race-matrix`
loaded commit `b67ec5444d4b3ef8480007c10fb2d73bab3c031e`, MVID
`f557435b-3d2a-4b8b-be4f-97de26665088`, and failed before the first spawn.
The resolver reported no complete male-Aasimar preset because it incorrectly
equated progression and visual race IDs. Zero fixture/capture records exist.
The guard, exact working-save load, no-save/no-production boundaries, clean
265-unit/3-party snapshot, hooks removal, and automatic exit passed.

Read-only installed IL makes the corrective contract exact:

- `DollState.Validate` selects `BlueprintRace.Presets[0]` in serialized order;
- `DollData.CreateUnitView` loads the preset skin with
  `BlueprintRaceVisualPreset.RaceId`;
- no native equality predicate relates that visual identity to the progression
  race identity.

The corrected probe mirrors these operations, records
`racePresetVisualRaceId`, and retains the nonnull skin/skeleton, nonempty doll,
no-unexpected-entity, all-slot-clear, both-hand-empty, restoration, cleanup,
save, and production guards. The focused test forbids the invalid equality
predicate. Repository validation, 1365/1365 tests, clean Release build/package,
and explicit strict validation pass. Pre-publication package SHA-256 is
`e6af511660abba47fd22dae853f6875ed31c1bd68607cc60440fe640f62c9502`;
DLL SHA-256 is
`edbc636195bd0b1fe80e41df7bdf532236502135da819570e07980a99a645824`.
The complete commit-bound rerun remains required.

The next guarded attempt,
`runtime-evidence/20260831T0026335779530Z-gunslinger-outfit-finalist-race-matrix`,
loaded published commit `55c487cc460c4950305d47e3c679bf8e858c943d`, DLL
SHA-256
`1be20efa6c457eb8da426b54f67598c3529cfc76c5c62454eea7ce9654e1897c`,
and MVID `e496489f-2fbc-47cf-a4c1-da914eda915a` through Steam `640820`.
It failed closed at `spawn-male-aasimar` before creating a fixture or image.
The working-save guard, no-save/no-production boundaries, exact cleanup, and
automatic exit passed.

Installed IL distinguishes the two native lifecycle stages:

- `DollData.CreateUnitView` configures a root `Character` component on the
  returned unbound `UnitEntityView` template;
- `UnitEntityView.OnDataAttached` later initializes `CharacterAvatar` with
  `GetComponentInChildren<Character>()`.

The qualification probe now checks the root `Character` before spawn and
retains the runtime `CharacterAvatar` check after attachment. Its focused test
forbids a pre-spawn `dollView.CharacterAvatar` requirement. Repository
validation, all 1365 tests, clean installed-reference Release packaging, and
strict validation pass. Pre-publication package SHA-256 is
`024d0c2b89a6e561b4c8d6eecc67e6f30c6b85941b893db7f9dcc6d5d22b0f2e`;
DLL SHA-256 is
`3cf170e14b0dc96910b093ee0737e713fd7d0c432a20cd59971c36dfc7be7d42`.
A published commit-bound rerun and direct inspection of all replacement images
remain mandatory.

The published lifecycle rerun at
`runtime-evidence/20260831T0044105199782Z-gunslinger-outfit-finalist-race-matrix`
loaded commit `08bfed17843adf348b210883b6f929b1af7c5678`, exact DLL
SHA-256
`9ebe80a42b3711dcc874357792da4b5a2e797eb0db18cd7a8d7f7d9a5e374db8`,
and MVID `f8abfd0e-59da-48c0-a796-15f085984c32`. All five male-Aasimar
donors attached and passed body, rig, size, and no-weapon checks, but each
recorded `dollExact=false` and zero renderers after 360 updates. No image was
created. Save, production, cleanup, and automatic-exit gates passed.

Installed IL identifies an ownership mismatch:

- `DollData.CreateUnitView` returns an instantiated, runtime-configured view;
- `SpawnUnit(BlueprintUnit, UnitEntityView, ...)` instantiates its argument
  again before registration;
- public `SpawnEntityWithView` attaches and registers the supplied view without
  cloning it.

The corrected probe initializes the existing view's blueprint, unique ID, and
transform, registers it directly, and requires the actor's view to be the same
reference before transferring cleanup ownership. Focused tests forbid the
double-clone path. Repository validation, 1365/1365 tests, clean
installed-reference Release packaging, and strict package validation pass.
Pre-publication package SHA-256 is
`d1dfe7cf3697e5757ce0bc86d7f0e2af72a621e98e4021c5ff5101511885a0ec`;
DLL SHA-256 is
`5462960ebbfd8815523b2132e84d7b2377dfc52a051b2ccdb04a646bf33e7108`.
The complete published rerun and direct image review remain mandatory.

## 2026-08-30 replacement-matrix visual rejection

Evidence directory
`20260831T0058130079392Z-gunslinger-outfit-finalist-race-matrix`
is a mechanical `PASS` for published commit
`141c6a8e1fcdacdb61164113ac77a6191b16254e` and DLL SHA-256
`d5d28e5e974b655cfcd5411aa9ceb726b2de00588a935bad5ababbc520b7c3f4`.
It contains the expected 9 races, 18 gender/race fixtures, 36 records,
72 PNGs/180 views, and 18 exact restorations. Unit/party cleanup, no-save,
no-production-mutation, hook removal, and automatic exit also passed.

Every PNG was reviewed through labeled boards kept outside the repository.
All cells rendered the intended Magus outfit, but both female-Human palettes
also showed a large two-handed sword. Original-resolution front, side, rear,
three-quarter, and isometric views confirm it. Therefore:

- the batch is visually `FAIL` despite its structured `PASS`;
- `magus-complete` remains provisional at 81/100;
- no production outfit is selected or mutated;
- the other clean-looking cells cannot substitute for a complete all-race
  matrix.

The female-Human fixture used `AmiriLevel20_Companion` and recorded
`clearedSlotItemCount=14`, `rendererCount=2`, and
`noWeaponModels=true`. The last value covered current
`HandsEquipment` model references but not an orphaned renderer created
before item removal.

Installed API reflection verifies a public
`BlueprintUnit.UnitBody()` and explicit starting equipment fields.
The replacement harness now gives only the request-local cloned donor a
neutral body before entity creation, preserves `EmptyHandWeapon`,
empties starting inventory/limb/quick-slot arrays, and rejects any donor that
still creates a slot item. Tests forbid source-blueprint mutation and
donor-specific exceptions. Repository validation, 1365/1365 tests, clean
Release packaging, and strict package validation pass; pre-publication package
SHA-256 is
`be1b6048c299f1d996db1091372c8e6c43863f51bae7b287ee58ca76f3c92bbb`
and DLL SHA-256 is
`68489bd17dd3bb363bbf53464beda0f7011cc10a7725212b31ef60127c80e13d`.
A fresh guarded full matrix and direct inspection of all replacement images
are mandatory.

## 2026-08-30 accepted neutral-body matrix

Command:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario gunslinger-outfit-finalist-race-matrix `
  -ExpectedVersion 0.0.110 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true `
  -Confirm:$false
```

Accepted evidence directory:
`20260831T0125478276325Z-gunslinger-outfit-finalist-race-matrix`.

- published commit: `47d6c55f6742219dac07824b08e1daa1c23309a1`;
- loaded DLL SHA-256:
  `57f9d7dec390cae8f53a78fadb9bd8c5cadb30368c97b5eadd8e454806ce285c`;
- loaded MVID: `1bace4ca-657e-4d4b-bccf-d9ee4933876e`;
- result: terminal `PASS`, all 11 scenario assertions;
- scope: 9 dynamically discovered races, 18 gender/race fixtures, 36
  native/alternate palette records, 72 PNGs, and 180 total views;
- fixture boundary: 18/18 request-local neutral bodies, zero created slot
  items, no weapon models, and three unsafe donor attempts rejected before
  capture;
- state boundary: 18/18 exact entity/ramp/saved-link restorations, exact
  party/global-unit cleanup, no save API, no production mutation, hooks
  removed, and process exited;
- visual boundary: every ignored PNG was inspected through eight labeled
  boards outside the repository. Female Human is free of the previously
  inherited greatsword in both palettes and both camera paths. All cells retain
  intact geometry/materials and expected hair/ear/horn/tail features with no
  donor gear or color-ramp failure.

The race/gender/color/no-weapon selection gate is accepted. The chosen
`magus-complete` pair now scores 88/100 (26/23/15/15/9). Equipment,
animation, rebuild, persistence, and production integration remain open and
may still establish a hard rejection.

## 2026-08-30 production-binding local checkpoint

The focused production implementation now owns the exact selected IDs and
defaults independently of every native donor. It validates and defensively
copies the catalog, resolves every entity before blueprint mutation, constructs
new direct links/arrays plus a new empty shared array, and replaces only the
five Fighter-derived presentation assignments. Fighter remains the existing
starting-gold source; no native class is mutated.

Local qualification results:

- active repository validator: `PASS`, deterministic count 1367;
- complete Release domain/reflection suite: `1367/1367 PASS`;
- clean installed-reference Release build: `PASS`;
- production firearm and SoundBank validation: `PASS`;
- strict standalone package validation: `PASS`;
- package:
  `KingmakerGunslinger-0.0.110-protection-from-alignment-control-immunity.zip`;
- package SHA-256:
  `34d9a7005fd9f535c33e460d7b4e23dc94553dbbcd34ee45540aeff167476df0`;
- DLL SHA-256:
  `6f039e773910a314f6abf46e2bd0d87d737660abd898d1ea7bd58918d11893eb`.

This is a dirty-tree/pre-publication build identity. It proves compilation and
package contents, not in-game resource initialization or visual override
behavior. The next gate is publication, commit-bound rebuild/preflight, and a
guarded canonical working-save load.

## 2026-08-30 published production save-smoke checkpoint

Production commit `bf3e052cb3a91691e214ec9a87c025f25f380c2d` was published
through the approved helper and all three feature refs were identical. Its
clean commit-bound local-runtime build passed repository validation, 1367/1367
tests, exact-reference Release construction, and strict package validation:

- package SHA-256:
  `4a91c92b9f842b7744adf707a2149ae13a4cc1ec70733979ad453406548a6c61`;
- DLL SHA-256:
  `78c8a7e8d8c1372bea930e4a48b4211ef4941974a062c1dbb707b0a8b7a1b8f5`;
- MVID: `41fd1851-9dec-4adf-87eb-0e79763d5e02`;
- quiescent runtime preflight: `163/163 PASS`.

The exact canonical command was:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario working-save-smoke `
  -ExpectedVersion 0.0.110 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true `
  -Confirm:$false
```

Evidence directory
`runtime-evidence/20260831T0159136175513Z-working-save-smoke` records terminal
`PASS` through Steam App ID 640820. Exact observations are:

- 111 catalog descriptors, one exact working save, and one distinct protected
  baseline;
- one exact UI action and receiver-correlated slot/window/load invocation;
- strict action/callback/fingerprint sequence `22<24<26<29<32`;
- stable post-load game ID `dce769e0-229c-4bfd-b8ea-e2d572bf8472`, party
  count 3, and a nonnull main-character reference;
- no save-writing API, no warning, hooks removed, and automatic exit.

This is commit-bound load evidence. It does not close outfit rebuild,
persistence, equipment override, animation, or final visual qualification.

## 2026-08-30 production compatibility harness local checkpoint

The independently guarded scenario name is
`gunslinger-outfit-production-compatibility`. Its eventual commit-bound command
is:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario gunslinger-outfit-production-compatibility `
  -ExpectedVersion 0.0.110 `
  -SaveName KMG_AUTOMATION_WORKING `
  -ExitAfterCompletion:$true `
  -Confirm:$false
```

The source-qualified acceptance contract is:

- observe the actual production Gunslinger class and exact selected resources
  for every installed player race and both genders before rendering;
- build native request-local Human character-generation dolls and apply the
  production class with `DollState.SetClass`;
- capture 16 named states for each gender, yielding exactly 32 sidecars, 64
  ignored PNGs, and 160 views across preview-like and ordinary-isometric paths;
- include default/alternate ramps; empty hand; held pistol, held musket,
  inactive/stored musket, and held blunderbuss; light/heavy armor override and
  removal; tricorn override and hair restoration; cloak override/removal;
  backpack visibility/removal; and repeated final rebuild;
- assert exact link resources, body slots, ramps, saved links, production
  blueprint immutability, request-local actor cleanup, exact global state, no
  save-writing API, hook removal, and process exit;
- make no motion, fire/reload/melee, or save-persistence claim from this run.

The focused registration/reflection contract and all 1368 domain tests pass.
Repository validation, clean installed-reference Release construction,
production firearm/SoundBank validation, packaging, and strict standalone
validation also pass. Dirty-tree package SHA-256 is
`b6da46f4c1a7c61fab0625762b46f5f7c222f6d478811300fdfa041512f409d6`;
DLL SHA-256 is
`1ca246f477ed3ccbd6ef7a194fc90a5b5a14671d2334bbbaf0a08b76236b9d8`.
The command has not yet been run against this source checkpoint; no visual or
runtime compatibility gate is closed by the local results.

## 2026-08-30 first production compatibility run

Evidence directory:
`runtime-evidence/20260831T0304180367838Z-gunslinger-outfit-production-compatibility`.

- published commit:
  `82361d31d2b0d7d278046161c13ee503aff6d51a`;
- loaded DLL SHA-256:
  `5265ce9925c4c5b3dd4b2ef90bd0f14d5707edd9d871d9959e77bb060c943562`;
- loaded MVID: `3b3ab851-e16f-48e6-b900-43c4d78b2558`;
- result: terminal `FAIL`, not timeout or ambiguous exit;
- completed scope: one exact male-Human production doll, four records, eight
  PNGs, and all 18 installed race/gender production-link rows;
- passed boundaries: exact game/mod/class identity, working save, no save API,
  production-blueprint immutability, exact party/global-unit cleanup, hooks
  removed, and automatic exit;
- failure: `musket-stored-inactive` had exact outfit/hair/ramp/saved-link/body-
  slot state but failed the harness's `!Renderable` expectation.

The failed expectation was invalid. Existing guarded presentation evidence
defines the musket as a visibly stored long gun when out of combat; only
designated handgun profiles use intentionally hidden storage. The partial
images are diagnostic and are not accepted as the final visual batch.

The corrected contract calls the existing stored/held active-presentation
resolver, requires the stored musket to remain out of combat with an exact
slot item and renderable model, includes the model in framing, and records its
presentation role. Repository validation, all 1368/1368 tests, clean Release
packaging, and strict validation pass. Dirty-tree package SHA-256 is
`beed41bbe74601d8d0f499c2ff5dff340f3e90822e02d4fa2e0f25cd69ab6baa`;
DLL SHA-256 is
`397f4c6a5a9069ae07e0ee2cfd195aa88d2a8fa1d82edaed49b983f68efa3396`.
A published commit-bound rerun and direct inspection of all 64 replacement
PNGs remain required.

## 2026-08-30 second production compatibility run

Evidence directory:
`runtime-evidence/20260831T0319410552031Z-gunslinger-outfit-production-compatibility`.

- published commit:
  `453f54732c05be6141d3eec259e4c46325f047e0`;
- commit-bound package SHA-256:
  `bd3934c4acdfb42ca369753ce29d523f6a3391badfb39a224254ef265b6e1fda`;
- loaded DLL SHA-256:
  `d9be26094a0eb8fd6f86dcff5572e85756ff311f1db12d22699ca4311c2b1388`;
- loaded MVID: `0c09675f-81e2-44f2-b98d-f14dd0ee619e`;
- result: terminal `FAIL`, not timeout or ambiguous exit;
- completed scope: all 18 installed race/gender production-link rows, zero
  fixture records, zero PNGs, and zero visual captures;
- passed boundaries: exact game/mod/class identity, working save, no save API,
  production-blueprint immutability, exact party/global-unit cleanup, hooks
  removed, and automatic exit;
- failure: after the old production-first settle sequence, class entities,
  saved links, and empty weapon state were exact, but native hair
  `9edf6b60bbf4d834facd4789837a3e0b` was absent.

The same hair entity survived through four states in the preceding run. The
second run therefore exposes scheduler-sensitive fixture ordering, not a
visual candidate defect. Installed assembly inspection confirms that native
view attachment has continuing `UnitEntityView`/`Character` lifecycle work;
the old harness took its mutation snapshot before proving that all resolved
`DollData` entities had settled. No image exists to inspect from this run.

The corrected source now adopts the accepted race-matrix boundary: before any
production mutation, it waits at least 30 updates and requires exact descriptor
`DollData`, every resolved native doll entity, selected hair, humanoid rig,
active renderers, and no weapon presentation. A bounded timeout records active
entity names and fails closed. Focused source assertions require this gate to
precede the production snapshot.

Local corrected-source results:

- repository validation: `PASS`;
- complete Release domain/reflection suite: `1368/1368 PASS`;
- clean installed-reference Release build: `PASS`;
- firearm/SoundBank and strict package validation: `PASS`;
- package SHA-256:
  `f7e0b896470a4fc120e6d9f8d7166ca1d6bdfaf7a94c53b1545ba73b12ea073c`;
- DLL SHA-256:
  `79f5f5138ea94c37b202d21b9320513a1986c78975d9fd3dd78bd8eeb1e8dd76`;
- MVID: `1e6d17a7-bb7c-4e5a-b36f-19e64b59969c`.

These are dirty-tree local identities. Publication, commit-bound rebuild,
quiescent preflight, a full terminal-PASS rerun, and direct inspection of all
64 replacement PNGs remain required. The candidate stays at 88/100.

## 2026-08-31 accepted production compatibility run

Evidence directory:
`runtime-evidence/20260831T0344513197562Z-gunslinger-outfit-production-compatibility`.

- published commit:
  `59eb7a97d6c1278f1e4e0d351aa6d4557b2db566`;
- commit-bound package SHA-256:
  `e15546c561d244f5f29517bec79f71025713cbd79530238ff69232f38fb18394`;
- loaded DLL SHA-256:
  `10f1beaf90eb6f5578ab5c8c09f9d10b219d587bb2adb11b308a959a7a422b26`;
- loaded MVID: `780b053b-acb8-4716-a5b5-87b578e356e0`;
- installed game assembly SHA-256:
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`;
- installed game assembly MVID: `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`;
- result: terminal `PASS`, version `0.0.110`;
- scope: 9 dynamic races, 18 exact race/gender production links, 2 Human
  fixtures, 32 exact ordered states/sidecars, 64 PNGs, 160 labeled views, and
  2/2 exact restorations;
- safety: exact named working-save boundary, no save-writing API, unchanged
  production blueprint arrays/links/colors, exact actor/global cleanup, hooks
  removed, and automatic exit.

Direct inspection covered every one of the 64 images through 16 labeled
male/female preview/isometric review boards. Default and alternate colors,
pistol, held/stored musket, blunderbuss, light/heavy armor and removal,
tricorn and hair restoration, cloak, backpack, and final rebuild are visually
accepted for both genders. No missing geometry, broken material, baked weapon
duplication, unacceptable hair loss, severe clipping, or stale override was
seen. Eight female isometric files are conservatively flagged low-density;
their minimum is 11,278 meaningful pixels, all paired previews pass density,
and every flagged image was directly legible.

An independent structured reconciliation returned `PASS` with zero issues:
32/32 sidecars equal their index records, 64/64 byte counts and SHA-256 hashes
match, all 160 views are accounted for, production pairs and ramps are exact,
previous states clear, native hair/saved links persist, weapon roles and body
slots are correct, and save/blueprint/restoration safeguards remain exact.

This closes static equipment override and request-local rebuild qualification.
Motion/fire/reload/melee and actual outfit persistence/respec-like
reconstruction remain independent mandatory gates. The candidate remains
88/100 until those complete the five-point withheld compatibility block.

## Local evidence policy

Raw catalogs, extracted metadata, screenshots/contact sheets, runtime result
batches, assemblies, saves, and machine-local configuration stay ignored and
untracked. This file will contain only concise curated findings, reproducible
commands, hashes/fingerprints permitted by policy, and honest uncertainties.

## Acceptance threshold

The selected candidate must score at least 75/100 and have no missing geometry,
broken material, baked weapon duplication, unacceptable body-part hiding,
severe animation clipping, unsafe race/gender gap, broken armor transition,
optional dependency, or generic-Fighter identity. Until every applicable gate
above passes, the mission remains unqualified.

## 2026-08-31 production motion source qualification

Scenario: `gunslinger-outfit-production-motion`.

Guard boundary:

- autonomous guarded request only;
- exact permitted save `KMG_AUTOMATION_WORKING`;
- Steam App ID 640820 launch remains mandatory for the runtime run;
- no save-writing API, UI input, mouse, screenshots, or player-save mutation;
- distinct 1,800-second evidence collector and exact cleanup result.

Installed native contracts verified before implementation:

- `UnitMovementAgentBase.MaxSpeedOverride`, `Velocity`, `WantsToMove`,
  `IsReallyMoving`, and `TickMovement(float)`;
- `UnitAnimationManager.Speed`, `WalkSpeedType`, and
  `GetAction(UnitAnimationType)`;
- exact Slow/Normal enum in
  `Kingmaker.Visual.Animation.Kingmaker.Actions`;
- `UnitMoveTo`, same-area `ForcedPath`, and `ForceLookAt`;
- `UnitAttack.CreateAttackCommand`, `UnitCommands.Run`, and acted animation;
- production Reload Firearm through `AbilityData`, `TargetWrapper`, and
  `UnitUseAbility`, with `ReloadRuntimeDiagnostics` and exact ammunition
  deltas.

Expected evidence is exactly two production Human fixtures, eight actions per
fixture, 54 records/sidecars, 54 four-view PNGs, 216 labeled views, six attack
outcomes, four movement outcomes, two turn outcomes, two reload outcomes, and
two original-avatar restorations. Required states are unarmed idle; slow walk;
normal run; right turn; pistol and musket native attack; production musket
reload; and native Shortsword melee. Fixed and event-aligned schedules are
encoded in the source and focused test.

Local source gates:

- repository validation: `PASS`;
- complete Release domain/reflection suite: `1369/1369 PASS`;
- focused `outfit-render.production-motion`: `PASS`;
- clean installed-reference Release build: `PASS`;
- firearm manifest and SoundBank checks: `PASS`;
- strict standalone UMM package validation: `PASS`;
- runtime scenario preflight: `169 PASS`;
- local package SHA-256:
  `00c80de81ff7acc218c1bbf08e51623950281f90e74e5750fee685da48b6e9be`;
- local DLL SHA-256:
  `c60baee8be07590b39c30a8685bde51e277bb13d8f9d0b226fb9f3950a1e4abd`;
- local DLL MVID: `a9e50b0b-b2e1-42f4-aa91-c9cdf98d4c5c`.

These are pre-commit local identities. Motion remains `PENDING` until the
exact published commit is rebuilt/installed, the guarded Steam run reaches
terminal `PASS`, every sidecar/hash/invariant is reconciled, and all 54 images
are directly inspected. Persistence remains a separate subsequent gate, so
the selected candidate remains 88/100.

## 2026-08-31 production motion attempt 1

Published source commit:
`3071fe38a61b79131f96f965053e7bc058ce209f`.
Commit-bound local-runtime package SHA-256:
`5eb5da0e740b3d84801c256721f921b636db5471d676cd00de98e99f245d2db7`.
Evidence directory:
`20260831T0455599323551Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL`.

The run produced 28/54 records: all 27 male records and the female unarmed
idle. The male native walk/run/turn, three attacks, and production reload
executed through their required live boundaries. The female exact production
doll then failed before slow-walk execution because the clean-combat guard
observed residual cached player combat state after the male disposable
combatants had left and been retired. No partial image is accepted as final
evidence. Save/version/game identity, blueprint immutability, no-save, cleanup,
and automatic exit assertions passed.

Installed IL confirms registered `UnitCombatJoinController.Tick()` invokes
the engine's `Player.UpdateIsInCombat()` character-list/group recomputation
and raises the party-combat event on change. The pending narrow repair records
the exact player, party-combatant, and turn-based state before and after each
fixture boundary and fails if it differs from the clean pre-run snapshot. A
complete replacement PASS plus direct inspection remains mandatory; the
motion gate and score are unchanged.

Repair source gate: repository validation `PASS`; full Release suite
`1369/1369 PASS`; clean Release build/package and strict firearm/SoundBank
checks `PASS`; settled-tree runtime preflight `169 PASS`. The first preflight
immediately after each clean build observed a transient artifact-tree
fingerprint change only; it created no backup/evidence and called no CIM or
process launch. The identical settled rerun passed. Pre-commit package
SHA-256 is
`7de0fc0ce93a703907a10d5862368083765dae831cd74487073988128538889d`;
DLL SHA-256 is
`b378256b722350bc9128b491e7f0d8e8f3a2b630bdccefe4664fb5c80f84e18f`;
MVID is `b4bf5593-d05b-41d5-b92c-d6ad1eff1356`. These identities are not
runtime acceptance.

## 2026-08-31 production motion attempt 2

Published source commit:
`fe24655acd4516e334796524ab7a3f40fd633888`.
Commit-bound package SHA-256:
`5228a562f65fbb2b694ec617548e71d1b713c3fea35d93789834b36eccebd44e`.
Loaded DLL SHA-256:
`ba1638817210bfa9b2d163356465719cc0d22e941947286c3d399bf3f236a9dc`.
Evidence directory:
`20260831T0521459019080Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL`.

The run completed all 27 male records, including both native locomotion
commands, turn, three attacks, and production reload. At fixture restoration,
the baseline expected false/zero/false but the recorded native boundary was
`player=true->true;party=3->3;turnBased=true->true`. Combat from the
disposable hostile had propagated to all three baseline party units. The
join controller correctly preserved that live group state, so the harness
failed rather than reporting a false clean boundary. No record from this
partial batch is accepted.

Read-only installed IL establishes the replacement lifecycle:
`UnitCombatLeaveController.Tick()` evaluates registered groups and calls full
`UnitEntityData.LeaveCombat()` when the retired encounter no longer qualifies;
`UnitCombatJoinController.Tick()` then performs player recomputation and the
normal party event. The source now resolves both registered controllers and
requires that leave-then-join sequence per fixture and during cleanup. It
contains no manual combat-flag write or event spoof. Repository validation,
the focused invariant, complete `1369/1369` Release suite, clean
installed-reference build, strict package/firearm/audio checks, and the
settled 169-check preflight pass. The first post-build preflight reported only
the documented artifact-tree stabilization sentinel; the identical rerun
passed. Pre-commit package/DLL SHA-256 values are
`b3598b28366eb82161b66b1e65144430c9461380dc93dc3dd2bb15db9fd7fbb3`
and `9f717b6c8d08f39cd67635bfc5e635543e38d60a38ce215a0a5c4f590cadfa41`.
Motion remains `PENDING` until an exact-commit replacement terminal PASS,
structured reconciliation, and direct review of all replacement images.

## 2026-08-31 production motion attempt 3

Published source commit:
`df4f3f04f55bbbdfe56ef113f723f89af23fa62a`.
Commit-bound package SHA-256:
`fa29aab259ef800d0db3ab11ccf6bd3b82999760778733523ef2737dfec348dc`.
Loaded DLL SHA-256:
`876879b6ab7f1cd2a376e8f43ed74109722f4841eb335179c20dad463ad0b651`.
Evidence directory:
`20260831T0539205863874Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` after 27/54 records.

The complete male action schedule passed its live action boundaries, but the
final combat record remained `true/3/true`; no partial capture is accepted.
Identity, no-save, blueprint immutability, inventory/target restoration,
structural cleanup, and exit protections held.

Installed IL proves `UnitCombatState.LeaveCombat()` does not raise the event
needed by `CombatController.HandleUnitLeaveCombat`. Full
`UnitEntityData.LeaveCombat()` performs that event-bearing lifecycle;
registered `CombatController.Tick()` then refreshes `HasEnemyInCombat` before
group leave and player recomputation. The pending repair uses full unit leave
for every disposable participant, records exact enemy/history/sorted-unit
caches, requires turn-based, group-leave, then player-recompute order, and
forbids low-level actor/target exits in the focused test. Installed-reference
compilation, repository validation, and all `1369/1369` tests pass. Clean
pre-commit package SHA-256 is
`ae22f6d1804ef1d4b9677d0a55c57dd3371c0340b63284f76e94e7bd8b5120f3`;
DLL SHA-256 is
`d0ba5261d5cf26d0b57534f060fbcba7407b1c4f0c421230f99ea8de2dcdcd75`;
MVID is `40e11afc-987d-4755-a057-df54bbfd09bf`. Strict package and
firearm/audio validation pass. The first preflight reported only the known
artifact-tree stabilization sentinel; the unchanged rerun passed 169/169.
Motion remains `PENDING` until an exact-commit replacement terminal PASS,
structured reconciliation, and direct image review.

## 2026-08-31 production motion attempt 4

Published source commit:
`f127e1f25f0d6d562a27a56ce9fe23f9b1ab8044`.
Commit-bound package SHA-256:
`66d97da08b4615991210cf74e5f0784d1de3c8910dfcecac78d779ec96f6dbed`.
Loaded DLL SHA-256:
`ea7c0b4931fbd32587aa9451b2c3475613bb866cc3658ad9dc67b63abfe7229e`.
MVID: `1f1de511-e4f9-4f52-98e0-ec2127a56494`.
Evidence directory:
`20260831T0601202638447Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` after 9/54 records.

The complete male pistol schedule reached fixed and acted frames and recorded
one exact discharge, but transition to musket rejected a non-quiescent prior
action. No partial capture is accepted. Identity, save-name, no-save, game,
blueprint, inventory, target, structural cleanup, and exit guards held; player
and turn-based combat remained true and correctly failed the cleanup gate.

Installed IL proves the disposable actor inherited the real player faction and
therefore the directly-controllable group. It also proves native group memory
can rejoin a live conscious hostile between actions. The replacement creates
two non-player, player-neutral faction clones with only bilateral request-local
hostility and a fresh target for each attack. Sidecars and live guards require
no shared player group, no actor/target hostility to the real anchor, and exact
player/party/turn-based caches at every tick and capture. Target memory links,
targets, blueprints, and faction clones are all request-local and destroyed.
Installed-reference compile, repository validation, and all `1369/1369` tests
pass. Clean Release/package and strict firearm/audio validation pass. The first
preflight reported only the known artifact-tree stabilization sentinel; the
unchanged rerun passed all 169 checks. Pre-commit package SHA-256 is
`78e8a067544d097c158aa77ce014fa9ccc0caf9863a6d2d9691492c7821cfd9c`,
DLL SHA-256 is
`db27ce97885fbba43df32c5bc804fde1ef81d3e6ed45c521c1bfd7386616cd9d`,
and MVID is `f4bc8c6e-c148-4890-818c-34dba4f32f1a`. Motion remains
`PENDING` until an exact-commit terminal PASS, structured reconciliation, and
direct image review.

## 2026-08-31 production motion attempt 5

Published source commit:
`1d2b1f8865b5ec12e57ea7dcc1ad25a8762eb63c`.
Commit-bound package SHA-256:
`8102f48085bed0830f746c52042e5b05e6a603dc36de49c556b052ec30863e71`.
Loaded DLL SHA-256:
`65c530ec491759987d026d86cb4400197eccd209cdb2ba641e774940edd22925`.
MVID: `f420093c-fef2-4a76-ad47-21e79bbc5c2b`.
Evidence directory:
`20260831T0637014594621Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` after 4/54 records.

The four noncombat male records retained exact outfit, hair, links, rig,
weapon, palette, and boundary contracts. Pistol preparation then observed
`player=True/False;party=0/0;turnBased=True/False;units=2/0`; no record in the
partial batch is accepted. Game/build/save identity, no-save, blueprint,
inventory, target/faction, structural cleanup, and exit assertions passed.

Installed IL proves the party anchor's holding state is still a player-coupled
boundary even after faction isolation: cross-scene units enter
`m_ControllableCharacters` through `Player.AddCharacterToLists` without a
faction predicate, and `UpdateIsInCombat` counts their groups. The pending
repair uses the exact loaded `AreaPersistentState.MainState`, verifies it is
live and distinct from `Player.CrossSceneState`, and requires exact
controllable/cross-scene reference sets plus area-local actor/target identities
at every boundary. Installed-reference compile, repository validation, and
all `1369/1369` tests pass. Clean Release/package and strict firearm/audio
validation pass. The first preflight reported only the documented artifact-
tree stabilization sentinel; the unchanged rerun passed all 169 checks.
Pre-commit package SHA-256 is
`2c6bdf7ffe6901ef33ddf5ab908e195cb3ce0675d93fc974b8c2798de9a30077`,
DLL SHA-256 is
`81a315c486dae914ec04c63bd0079be1780c626d5031416c0f5c0c0d7ecf6651`,
and MVID is `6ed1466d-9131-4b83-84e6-5f86c156a20f`. Motion remains
`PENDING` until an exact-commit terminal PASS, structured reconciliation, and
direct review of every replacement image.

## 2026-08-31 production motion attempt 6

Published source commit:
`27bc24ae9ce5b84d3eb8760741833697ed52a911`.
Commit-bound package SHA-256:
`9c97279edf78fb4f7540667b3e983b2c5b5b0b5ec98604c3fdea3b0e4bec3413`.
Loaded DLL SHA-256:
`37c764f27e63f984fd09b9ec80d465372e997e693269716b8b61e66f07eb98a3`.
MVID: `38f7a207-baa3-4ee8-8774-c8d3de192b92`.
Evidence directory:
`20260831T1215532823796Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` before record 1.

The male-Human view attached and its weapon presentation remained empty, but
the bounded settle gate observed `doll=False;hair=False;active=`. The run
retained exact false player/turn-based combat, zero party combatants, exact
controllable and cross-scene lists, inventory restoration, blueprint
immutability, no-save behavior, the then-current global-unit cleanup gate, and
automatic exit. That cleanup gate did not inspect `MainState.AllEntityData`;
no save API ran and the process exited. No partial capture exists or is
accepted.

Installed IL shows that using the save-backed `MainState` is unnecessary for
live scene behavior. `SceneEntitiesState.AddEntityData` accepts an independent
container, and `IsSceneLoaded` resolves only its `SceneName` through Unity's
scene manager. It also shows `EntityDataBase.Dispose` does not remove state
membership. The pending repair creates a disposable state with the exact
loaded `MainState.SceneName`, marks it `SkipSerialize`, and proves it differs
from both `MainState` and `CrossSceneState`. Actor and target retain the live
Unity rendering/navigation context without joining player or persistence
graphs. Native `RemoveEntityData` must empty it between fixtures; terminal
cleanup must dispose it while still empty.

Installed-reference compile, repository validation, all `1369/1369` tests,
clean Release/package, and strict firearm/audio/package validation pass. The
first preflight reported only the known artifact-tree stabilization sentinel;
the unchanged rerun passed 169/169. Pre-commit package SHA-256 is
`64d07b6d3aa843aefb185cd2a07e4dce860ea46e522770e9eff7e9d16988981e`;
DLL SHA-256 is
`582e306bae50394eca161705b425c847bc08ba36e59ab23b36ac6fdfdd91a0d3`;
MVID is `43f248b1-be23-43f8-aaf9-78cb02a8f9cd`. Motion remains
`PENDING` until an exact-commit terminal PASS, structured reconciliation, and
direct review of every replacement image.

## 2026-08-31 production motion attempt 7

Published source commit:
`b27438c7fd38d4e588a47b05b5e2329fb3676932`.
Commit-bound package SHA-256:
`788dcf4d89fac23941f79d9cca54db5f673bb5405125ca5e8817ef24553056e8`.
Loaded DLL SHA-256:
`9a0d1a9d671697f9a5a46c366cb6fe29af83dc528530e805987c55791ff21456`.
MVID: `60f2fd26-9d78-401d-94d1-69a1c393afbe`.
Evidence directory:
`20260831T1253077289617Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` after 10/54 records.

Male locomotion, turn, and one complete pistol `UnitAttack` ran. The pistol
produced ready, fixed, and acted frames and exactly one discharge. Before the
real musket command, its readiness sidecar showed `loadedRounds=0`, fired count
two, and an active `UnitAttack`; the production empty-firearm patch therefore
returned no command. No partial capture is accepted. Guarded request, exact
game and loaded build, working-save/no-save boundary, blueprint immutability,
exact player lists and combat state, empty/disposed request-local scene,
cleanup, and automatic exit passed.

Installed IL proves the readiness probe itself was improperly live:
`UnitAttack.Init` performs attack planning and approach-radius initialization,
whereas `UnitCommands.Run` also registers the command for advancement. The
pending repair directly initializes the probe, proves it never enters actor
commands throughout target placement, and creates a separate live command
only after the ready capture. Every attack record and terminal attack outcome
now requires `readinessProbeDetached=true`.

Installed-reference compile, repository validation, all `1369/1369` tests,
clean Release/package, strict firearm/audio/package validation, and the
settled 169-check preflight pass. The first preflight reported only the known
artifact-tree stabilization sentinel. Pre-commit local-runtime package
SHA-256 is
`31498b7bed5b9532d0a208cda645b744cdfefa30b2a7246fab472696da7f0ce1`;
DLL SHA-256 is
`877b451e4a4a62751b3d1b75c217e24c2b66c857ec4d973e28bdfc5e23ef100d`;
MVID is `2ada3432-6aa8-4a77-81b1-934fe1a698f0`. Motion remains `PENDING`
until an exact-commit terminal PASS, structured reconciliation, and direct
review of all 54 replacement images.

## 2026-08-31 production motion attempt 11

Published source commit:
`4ef28f65577d09329536a905976b405cac4562ef`.
Commit-bound package SHA-256:
`6f849b89c4ffba745585d268c1a1ff12c83074b2e5f80d13853e91e3c6c77a34`.
Loaded DLL SHA-256:
`871a89190537624f150356e381b106cb162b70a215936c780913642096cb01c4`.
MVID: `10e8676b-e8d8-48f4-b4a1-210d0afe0d2f`.
Evidence directory:
`20260831T1438053243232Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` before record 1/54.

The male Human fixture again timed out before production application with no
native DollData entities or hair
(`doll=False;hair=False;noWeapon=True;active=.`). Guarded request, exact game
and loaded-build identity, disposable working-save/no-save boundary,
production-blueprint immutability, exact player and structural cleanup,
empty/disposed request-local scene, and automatic exit passed. No PNG was
created or accepted.

Because this is the second occurrence of attempt 9's pre-action boundary, no
unchanged retry is permitted. Installed IL shows the native DollData character
should receive synchronously resolved equipment entities before spawn, while
the subsequent queued attachment starts, updates, and rebuilds that view's
character. The pending exact-commit diagnostic records the original character
after creation, after spawn before the entity tick, after attachment, and at
timeout. It reports resource-preloading state, Unity instance identity,
raw/active/saved entity counts, expected ID count, active entity names, and
template/attached reference equality. These records distinguish creation
failure, transfer loss, and replacement during attachment without mutating
production assets or the save.

Installed-reference compile, repository validation, all `1369/1369` tests,
clean Release/package, strict firearm/audio/package validation, and the
settled 169-check preflight pass. The first preflight reported only the known
artifact-tree stabilization sentinel. Pre-commit local-runtime package
SHA-256 is
`aa512f88878ef88d7486176080552f6ff3ac237f540a3f042d49d75842227112`;
DLL SHA-256 is
`0c97e7c7a7c450fa93fef6fcc42a523809302c9dc01934352ab06530cdc0583b`;
MVID is `47392b4f-cbc0-450f-9b72-82b284e578c7`. Motion remains `PENDING`
until an exact-commit terminal PASS, structured reconciliation, and direct
review of all 54 replacement images.

## 2026-08-31 production motion attempt 8

Published source commit:
`5d520bbccaff98e09a9a94c3fa2c59811cd2f0ca`.
Commit-bound package SHA-256:
`a703f089ff28cc83c3d835df36de1180950d668b5230a6bcef9a7cc9fcf7eb6b`.
Loaded DLL SHA-256:
`7e8c1619acec69da73f10f6e5f3f6089a5d571163077fe9533193c4976763548`.
MVID: `8b7060eb-cefe-4ac9-8be1-62d61a0e1974`.
Evidence directory:
`20260831T1330408485246Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` after 10/54 records.

The detached-probe evidence is positive: both pistol and musket preparation
reported `probeDetached=True`. The pistol's acted frame consumed exactly one
round, but its update-36 sidecar still showed a running `UnitAttack`. At the
next musket-ready frame, `loadedRounds=0`, total fired count was two, and the
same command type remained active. Production correctly returned no command
for the empty musket. Guarded request, exact loaded package and game identity,
working-save/no-save boundary, blueprint immutability, request-local
scene/player/combat cleanup, and automatic exit passed. No partial capture is
accepted.

Installed IL establishes why ordinary cleanup was insufficient. An acted
`UnitCommand` is non-interruptible while its animation handle is unfinished,
and `UnitCommands.InterruptAll(true)` skips it. The prior evidence schedule
ended at update 36 and proceeded to equipment/target teardown without proving
native command retirement. The game log records an equipment-animation null
reference after pistol discharge and another round consumption after the
weapon switch, consistent with the retained pistol command firing the newly
equipped musket.

The pending exact-commit repair waits until all visual/discharge evidence is
complete and the command is either stopped or natively interruptible. It adds
structured `retirementReady`, running/interruptible, update-count, and
running-command-type evidence; terminal attack contracts require readiness.
After ordinary interruption, a hard gate forbids weapon, combat, or target
mutation while any command remains running, and the inter-action transient
contract independently requires zero running commands.

Installed-reference compile, repository validation, all `1369/1369` tests,
clean Release/package, strict firearm/audio/package validation, and the
settled 169-check preflight pass. The first preflight reported only the known
artifact-tree stabilization sentinel. Pre-commit local-runtime package
SHA-256 is
`17d46838be9b31b3fecda29ef582f2aae2cfc422e2f5c25be41f3d58811f2dbb`;
DLL SHA-256 is
`e1b154a9e2c35348d6b6d67cd9fa8274c4764ffa5604335a24e680ada14b5844`;
MVID is `bbd56913-905f-4d32-8546-cc3926bdaa2f`. Motion remains `PENDING`
until an exact-commit terminal PASS, structured reconciliation, and direct
review of all 54 replacement images.

## 2026-08-31 production motion attempts 9 and 10

Published source commit:
`0dbdaf2b283bbb6245939d4078c26f90d94d01ff`.
Commit-bound package SHA-256:
`a8a6ae85f171e1c5140f17794830b0d11b64b4154af21a755332dd784ee570ca`.
Loaded DLL SHA-256:
`585e4abf748225398f13c02afbd62313e2111fd46137cd57415af331925efd40`.
MVID: `a6768c5d-46e6-4fef-b45c-c2b958989d4e`.

Attempt 9 evidence directory:
`20260831T1401393847532Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` before record 1/54.

The male Human fixture did not populate native DollData or hair inside its
bounded settle window (`doll=False;hair=False;noWeapon=True;active=.`).
Guarded request, exact game/build identity, working-save/no-save boundary,
blueprint immutability, exact structural cleanup, empty/disposed request-local
scene, and automatic exit passed. Attempts 7 and 8 had passed the identical
pre-action fixture, so one controlled retry of the unchanged commit was made;
no visual record from this run exists or is accepted.

Attempt 10 evidence directory:
`20260831T1407213494923Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` after 10/54 records.

The pistol update-36 sidecar recorded `loadedRounds=0`, a running
non-interruptible `UnitAttack`, and a detached readiness probe. Its terminal
outcome later reached retirement ready. The musket-ready sidecar then recorded
no running command but retained a raw `UnitAttack`, had `loadedRounds=0`, and
showed total firearm count two. The separately constructed musket command was
correctly rejected as unloaded. Guard/build/save/no-save/blueprint/cleanup/exit
contracts passed; no partial image is accepted.

Installed `UnitCommands` IL proves the residual slot was not equivalent to a
running command: `InterruptAll(bool)` skips an `IsFinished` command without
clearing its raw slot, while public `RemoveFinishedAndUpdateQueue()` performs
that eviction. The pending exact-commit repair rejects queued work, interrupts,
calls native finished-slot cleanup, and requires `slotEvicted`,
`Commands.Empty`, and zero running, resident, and queued commands before any
weapon, target, combat, or outfit teardown. Sidecars and outcomes expose all
four facts, and transient cleanup independently repeats the gate.

Installed-reference compile, repository validation, all `1369/1369` tests,
clean Release/package, strict firearm/audio/package validation, and the
settled 169-check preflight pass. The first preflight reported only the known
artifact-tree stabilization sentinel. Pre-commit local-runtime package
SHA-256 is
`fca9cf06fb1fb6a3e967eb7414c3ffb4ac679d639695ab81faf146788921e274`;
DLL SHA-256 is
`1d2d17ffe350388308fab4aa62d81637d378ce44c2408ec8c2d34c365a3a418a`;
MVID is `d983a009-2e6c-41aa-ba32-56b9c20487f9`. Motion remains `PENDING`
until an exact-commit terminal PASS, structured reconciliation, and direct
review of all 54 replacement images.

## 2026-08-31 production motion attempts 11 and 12

Attempt 11 used published commit
`4ef28f65577d09329536a905976b405cac4562ef` and evidence directory
`20260831T1438053243232Z-gunslinger-outfit-production-motion`. It failed
before record one at the repeated empty-doll settle boundary. Guard, exact
game/build, working-save/no-save, immutable-blueprint, cleanup, request-local
scene, and exit passed. Because this was the second occurrence, the next
commit added four-stage native doll lifecycle instrumentation instead of
retrying unchanged.

Attempt 12 used published lifecycle commit
`2e73bf3035860ffc940c31f4e5c090b0f5d5df2e`, package SHA-256
`6d4e6b3aa27658e958f7010937a7d62e481988eb4eb5967fdc8d719dfbd94d5f`,
DLL SHA-256
`90c727bcbd90ac962e7dd406c6bbc0c8f16f55ac05ff4ba8f812a9ff0e1f205d`,
and MVID `6d5eafa3-919a-40fc-a39c-9206ab6ca58f`. Evidence directory:
`20260831T1509405239304Z-gunslinger-outfit-production-motion`. Terminal
status: `FAIL` after 10/54 records.

Lifecycle evidence showed the same avatar instance with five raw/active
entities, zero saved entities, four expected outfit IDs, and preloading false
before spawn, after spawn before tick, and after attachment. The doll was
healthy. At musket ready, however, the firearm had zero rounds and two total
discharges; no harness attack was installed and its readiness probe was
detached, while resident and running collections contained `UnitAttack`.
Because pistol cleanup had already proven an empty command container, a new
unowned attack appeared between actions. No partial image is accepted.

The request-local actor and target clones inherited the donor NPC brain, which
native combat engagement activated. The pending exact-commit repair clears
that brain only on the disposable clones, requires an empty command container
before accepting attack evidence, requires the evidence command's `AiAction`
to be null, and records clone-brain plus active-command ownership in sidecars
and terminal contracts. Installed-reference compilation and all `1369/1369`
tests pass, as do clean strict packaging, firearm/audio validation, and the
settled 169-check preflight; the first pass reported only the documented
artifact-tree stabilization sentinel. Pre-commit package SHA-256 is
`b3c73cf63e68fa3cb4aff086bd236accf3769e69f53a5afe7c259776139d76e2`,
DLL SHA-256 is
`3c95c8c5115135023023ff74d4c77cbc3aaf90ff7c0ca742c3e397e1741c839d`,
and MVID is `5a199838-48eb-49a2-8b92-7ca8d0dfabe2`. Motion remains `PENDING`
until the replacement run reaches terminal PASS and every one of its 54 PNGs
is reconciled and directly reviewed.

## 2026-08-31 production motion attempt 13

Published source commit:
`934785962bb4ef752993add5558d20cb751f1c7d`.
Commit-bound package SHA-256:
`a53c3314dd6aeb5d4ee13a8f0b5615d93325212062f1c5916ef0aa9460f88e5f`.
Loaded DLL SHA-256:
`af2af437dd06f55c1316305190e02aee86f95a1d0f0c2364b48b4eb032c7fff1`.
MVID: `0e441834-5f14-41d6-b1ad-15d46b4f976e`.
Evidence directory:
`20260831T1548069712324Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` before record 1/54.

The same avatar reported zero raw, active, and saved entities with four
expected DollData IDs before spawn, after spawn before tick, and after
attachment; `ResourcesLibrary.Preloading=True` at all three points. The flag
was false by timeout, but no entity had appeared. Guard, exact game/build,
working-save/no-save, immutable-blueprint, request-local scene, cleanup, and
automatic-exit contracts passed. No visual record exists or is accepted.

Installed `DollData.CreateUnitView(false)` IL synchronously calls
`TryGetResource<EquipmentEntity>(id, false)` for every ID. Installed
`TryGetResource` IL returns null when preloading is true and the ignore flag is
false. The avatar therefore cannot heal merely by waiting after creation.

The pending replacement waits up to 360 updates before creation, proceeds only
when preloading is false, and rechecks that fact at the construction line. It
records the wait count plus creation-time state and makes both facts terminal
fixture requirements in static compatibility and motion. Compilation and all
`1369/1369` tests pass, as do clean strict packaging, firearm/audio validation,
and the settled 169-check preflight; its first pass reported only the expected
artifact-tree stabilization sentinel. Pre-commit package SHA-256 is
`8aba976c9550a3c09b95539dee11d7825362169b0933b546837cb2e34d25c378`,
DLL SHA-256 is
`379f0bc2a1612065b3ae53539b391f11ac20161be18b4a0dfb0f47bba8803a89`,
and MVID is `5a3b66e8-97b3-4d55-b7e0-db500ca82c96`. Motion remains `PENDING`
until the exact-commit replacement reaches terminal PASS and all 54 PNGs are
reconciled and directly reviewed.

## 2026-08-31 production motion attempt 14

Published source commit:
`4447ebd679aaf55058958a52b69ba9ac4b00effb`.
Commit-bound package SHA-256:
`028fd526db9656a4952d3343e0f08453343ff6a5614d53acad475f0e23eff833`.
Loaded DLL SHA-256:
`348b19e22d598cb5a818ff7847a6e7a896966ab2380bad710318ab85c90585c2`.
MVID: `2cedc19a-8bac-4655-b0ed-03e02e98b3a4`.
Evidence directory:
`20260831T1608345329020Z-gunslinger-outfit-production-motion`.
Terminal status: `FAIL` after 54/54 records and 216/216 views.

All assertions except the aggregate fixture assertion passed. That includes
exact guard/game/build/save identity, no save call, both resource-ready exact
production dolls, native walk/run with distinct measured speeds, turn, all
six native attacks, both production reloads, exact restoration, combat-state
reconciliation, blueprint immutability, request-local cleanup, and automatic
exit. Both dolls were created while preloading was false.

The fixture failure was exclusively `locomotionClipCount=0` on both genders.
The successful live movement outcomes and frames prove that positive generic
clip-list population is not the engine's locomotion contract. Existing
exact-game repository code accepts a non-null `LocoMotion` action plus the
movement agent. The pending repair requires that exact action surface and
retains the zero count as diagnostic information; it does not remove any live
movement assertion.

Independent reconciliation result: `PASS`; 54 unique indexed PNGs, 54
sidecars, 27 records per gender, 216 views, and exact hash/byte/identity/
meaningful-pixel agreement. Index SHA-256:
`278cce94824eaae17a5886071221aa54eb541108900030346f086b661ad2fc66`.
Canonical filename-sorted PNG-set digest:
`043f3dd3d8cd2bbba09dc035067dd0e110b19ac6fab41c67ab1a4ef8813605cd`.
Every one of the 54 four-view sheets was directly inspected. No selected
outfit part disappeared or clipped through the body or another outfit part in
idle, walk, run, turn, pistol, musket, reload, or shortsword motion. Some live
combat frames contain a native outline; the outfit remains legible and
unchanged. This visual result is not promoted to qualification because the
batch's terminal status is `FAIL`.

Repair qualification: repository validation and installed-reference compile
pass; complete domain suite `1369/1369`; clean Release/package, firearm/audio,
and strict standalone-package validation pass; stabilized runtime preflight
`169/169`. Its first pass emitted only the documented
`unsupported-does-not-build-or-stage-package` stabilization sentinel.
Pre-commit package SHA-256:
`3c6cc236fc0e84b1da02616bcafe15eb82c427c6b8ea7e1f4ffc1ddbea285b49`.
DLL SHA-256:
`5d46a1faeb471014841af5732244ad64e22b3c15ae935fc28fb950119a68c2f1`.
MVID: `6b1c2eb8-6a9a-41d2-b15e-de3d1df503ef`.

Motion remains `PENDING` until the published repair's exact-commit attempt 15
reaches terminal PASS and its replacement 54-image batch is reconciled and
directly reviewed.
