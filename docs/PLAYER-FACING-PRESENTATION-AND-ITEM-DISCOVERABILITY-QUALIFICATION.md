# Player-Facing Presentation and Item Discoverability Qualification

## Mission identity

- Baseline local `master`: `0fe38002fc022ad5a04d65430eb461046cd9cc3c`.
- Baseline `origin/master`: `0fe38002fc022ad5a04d65430eb461046cd9cc3c`.
- Baseline version: `0.0.104`.
- Candidate branch: `codex/player-facing-presentation-and-item-discoverability`.
- Candidate version: `0.0.105-player-facing-presentation-item-discoverability`.
- Baseline worktree: clean before the feature branch was created.

The expected drafting-time SHA matched the actual current local and remote
baseline, so there were no intervening master changes to reconcile.

## Installed qualification environment

- Pathfinder: Kingmaker `2.1.7b`, Steam build `6757524`, launched only through
  Steam App ID `640820` for real runtime tests.
- `Kingmaker.exe` file version `2018.4.10.10503941`, SHA-256
  `94A77969B1F770007E4E46F919FF2356AE33C213E5CA5F5907967653B97A6E14`.
- `Assembly-CSharp.dll` SHA-256
  `3B6450336AC660108F2DFF6A531C45B606DC01038A3905C9917540339769E215`.
- Unity Mod Manager `0.32.4`.
- Relevant installed optional mods: Call of the Wild `1.14.4c-2.1`
  (`4EBF22082EFB29F41C3614E3A967D76D42E41434E17E4841798707C2DB23E94A`),
  Favored Class `1.3.1`
  (`DCD377A286D2084C29D1D357927929E93031120390293E079764175397366518`),
  Craft Magic Items `2.1.0`, Bag of Tricks `1.16.4`, Cheat Menu `1.2.3`,
  Races Unleashed `1.0.11`, and Tweak or Treat `1.1.0`.
- Previously installed Gunslinger before staging: `0.0.104`, DLL SHA-256
  `4003C284C116D8BF1E2019692D035BE563E87F1021B6C26C6470246905B916CC`.

## Implemented contracts

### Brown-Fur Transmuter ordering

Brown-Fur Transmuter is inserted before the exact five installed Favored Class
combined-Arcanist identities. If none of those identities is present, it is
appended normally. The publication transaction records the insertion boundary,
preserves foreign archetype order, and can roll back without deleting a later
foreign append. Runtime evidence records the Brown-Fur index, the first combined
index, and whether the ordering is exact.

### Player-facing text

Weapon type, item, enchantment, and class-feature presentation was rewritten as
player-facing rules text. The hidden Eastern policy enchantment has blank name
and description instead of an implementation label. A shared presentation
policy rejects implementation vocabulary, internal symbols, null placeholders,
and common encoding artifacts. The live observer inventories 55 project weapons,
56 player-visible project items including the Cord, and 12 project enchantments.

### Discoverable items

The complete inventory now has 30 distinct fixed `BlueprintLoot` targets across
29 exact areas. It has no recurring-vendor rows, random-table placements,
temporary area variants, or target names suggesting hidden caches, secrets,
puzzles, quest coupling, corpses, or trash. The only same-area pair is River
King's Measure and Irovetti's Ovation in two separate ordinary palace chests.
The late capstones remain distributed across Castle of Knives, the House at the
Edge of Time, and three distinct Final Dungeon floors.

Cord of Stubborn Resolve is published once to
`9572baf3952095f41abda1fb25055cce`, `RichHuman_treasure_chest_04 (1)`, in
`CapitalTavern_Indoor`. The transaction also removes its retired
Capital Square Village row and restores both exact snapshots on rollback.

## Deterministic qualification

- Focused presentation, ordering, placement, cleanup, and rollback tests: PASS.
- Complete dependency-free domain/reflection suite: PASS, 1,315/1,315.
- Final repository validation: PASS.
- Clean Release build and installable-package creation: PASS.
- Build-output, firearm manifest and SoundBank, and strict standalone UMM
  package validation: PASS.
- Release compilation and deterministic tests do not establish in-game
  correctness.

## Guarded runtime qualification

Every launch below used the documented `-kmgRuntimeTestRequest` path through
Steam App ID `640820`. No direct executable launch, visual UI interpretation,
or unguarded save access was used.

### Preserved failed candidate

The first runtime candidate was not treated as qualified:

- source-state SHA-256
  `b26446e9bfe42bed1df351aa8450c0cfbfb0a6b9c09741aff753f0e1cb998043`;
- package SHA-256
  `1487b672f8c972170256c6696bfffb18138312bb500c1061fa8e94c8c5710cc2`;
- DLL SHA-256
  `eaf2dd1e5136229dc3f8d7c56e132d27ebe83ae0342c6acb5342211dc26c3f3c`;
- MVID `b64e99c7-4066-4bf9-aae8-19c45c593c12`;
- evidence directory
  `20260828T0218061203722Z-observe-gunslinger-presentation`, run ID
  `20260828T0218061359981Z-e807ed6314ac43b0ae1072c0acac43ba`:
  `TIMEOUT`, with `timeoutStage=request-accepted`.

The real bootstrap exposed `Eastern campaign publication cardinality mismatch`.
The publication check still expected the former 29 target mutations after the
active-plus-cleanup table grew; bootstrap rolled back all 1,604 registrations.
The fix derives the expected cardinality from `Loot.Length + CleanupLoot.Length`.
The artifact was rebuilt, redeployed, and every final scenario below was rerun.

### Immutable final runtime artifact

- Build-local source-state SHA-256
  `250ed285247113c33b39855609f6125c68652c7c744f06b967dd0ec7cd0981e7`.
- Package SHA-256
  `6b6a85bd7642715841a4820b6db9a443a69c4d9eb578e3c56a6fbc5912bce8ce`.
- DLL SHA-256
  `2a06d93880e3716e29b30153a7e5b48fc53f6c363d1c1d53a0cb29819fbb457c`.
- MVID `61589b34-b11d-43a2-9d06-f9fac46fcdf3`.
- Deployment manifest
  `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260828T0235278458389Z\deployment.json`.
- Preserved feature-module settings SHA-256
  `28b9589db49ef977d2a033aa563052930a1d0e37e920689db746bd0af9108b59`.

### Final structured results

- Presentation: evidence directory
  `20260828T0235417947135Z-observe-gunslinger-presentation`, run ID
  `20260828T0235418259237Z-fa2b55f6a75b4a99ad746f320a281214`, PASS,
  9/9 assertions. The live census found 55 project weapons, 56 project items,
  and 12 custom enchantments with `issues=<none>`.
- Complete acquisition audit: evidence directory
  `20260828T0237409495792Z-observe-rare-firearm-acquisition`, run ID
  `20260828T0237409652002Z-da91ee23bd11461cb090f401c03dcde3`, PASS,
  25/25 assertions. It observed 30 items, 30 distinct targets, 29 exact areas,
  23 normalized campaign-area families, maximum permitted density 3, zero
  vendor rows, one active row per item, and zero retired-target copies.
- Focused Cord audit: evidence directory
  `20260828T0240171423403Z-observe-capital-cord-vendor`, run ID
  `20260828T0240171580108Z-c3515522e0db41bfbb6dfe7a482d8271`, PASS,
  4/4 assertions. It observed exactly one count-one Cord row in
  `RichHuman_treasure_chest_04 (1)` in `CapitalTavern_Indoor`, zero vendor
  rows, and zero rows in the retired Capital Square Village target.
- Feature-module boundary matrix: the 20 evidence directories from
  `20260828T0242373072691Z-observe-feature-module-settings` through
  `20260828T0314308411386Z-observe-feature-module-settings` are PASS,
  260/260 assertions. All 20 Brown-Fur publication-gate assertions passed.
  In all 10 enabled states Brown-Fur was index 6 and the first of five known
  combined archetypes was index 7; all 10 disabled states had no Brown-Fur
  selector reference. The matrix restored the original settings bytes exactly.
- Canonical named-save smoke: evidence directory
  `20260828T0316455326107Z-working-save-smoke`, run ID
  `20260828T0316455639356Z-16f627acc4a2401195a09c48331cb4e3`, PASS,
  11/11 assertions. Only `KMG_AUTOMATION_WORKING` was requested. The catalog,
  descriptor, load callback, and fingerprint correlated exactly, and the
  observer invoked no save-writing API.

After qualification, with Kingmaker stopped, the exact pre-mission live mod
backup `20260828T0218025110282Z` was restored. The restored installation is
version `0.0.104`, DLL SHA-256
`4003C284C116D8BF1E2019692D035BE563E87F1021B6C26C6470246905B916CC`,
and retains the original feature-module settings hash above.

## Remaining uncertainty

Static target identity, transaction integrity, deterministic tests, and guarded
runtime graph observation can establish exact publication. They cannot prove a
human player's organic route, subjective visibility, or pacing experience, and
they do not rematerialize already opened containers in old saves. Human organic
pacing acceptance therefore remains explicitly pending.
