# Gunslinger Outfit Kitbash Journal

## 2026-08-30 - Intake and isolation

- Read the repository agent instructions and the complete mission package
  documents before modifying tracked files.
- Verified repository root and fetched origin metadata.
- Verified local `master`, `origin/master`, and the pre-branch `HEAD` were
  identical at `5949165e2a6407ca480d46cd86d8944e4152e2fb`
  (`v0.0.110`, `Release protection from alignment control immunity`).
- Created and switched to exactly
  `codex/gunslinger-class-outfit-kitbash` from `origin/master`.
- Verified active source pins are `0.0.110` with informational identity
  `0.0.110-protection-from-alignment-control-immunity`. No version change has
  been made.
- The human-supplied external reference-package path was absent. A pre-existing
  untracked, non-symlink package under `docs/reference` contained the three
  named documents and exactly eight images. Its document/image manifest entries
  matched the computed image SHA-256 values. The package remains untouched and
  is explicitly ignored to prevent accidental publication. This location
  discrepancy remains an intake uncertainty; findings below come from the
  manifest-matching local package.
- Positively inspected all eight images. The rejected male/female Gunslinger
  presentations both use the generic blue Fighter tunic, red sash, belt, dark
  trousers, and boots. Barbarian and Paladin benchmarks demonstrate that native
  outfits can retain clear class identity at preview scale. The four
  inspirations consistently support fitted/long garments, dark leather,
  restrained burgundy, practical belts/pouches/straps, gloves/bracers/boots,
  and controlled asymmetry; hats and visible weapons are not essential to the
  shared vocabulary.
- Confirmed the current class registration still resolves Fighter and the
  current class creation path copies Fighter presentation fields. Production
  selection remains prohibited until installed API and rendered-asset evidence
  exist.
- Created the five required durable mission records. No gameplay source,
  production asset ID, native blueprint, installed game file, or version pin
  has been changed.

## 2026-08-30 - Installed API and native-resource investigation

- Verified the live game through Steam App ID 640820. Unity Mod Manager reports
  Kingmaker `2.1.7b`; the loaded `Assembly-CSharp.dll` SHA-256 is
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`
  and its MVID is `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.
- Installed reflection/IL inspection established that
  `BlueprintCharacterClass.GetClothesLinks(gender, race)` yields shared-wrapper
  links before gender-specific direct links. Shared
  `KingmakerEquipmentEntity` wrappers select race/gender links and provide the
  installed fallback behavior. `EquipmentEntityLink.AssetId` is the stable
  loading boundary.
- Verified the public avatar operations needed by the next audit stage:
  add/remove equipment entities, rebuild the outfit, apply primary/secondary
  ramps, and restore saved equipment. Verified native equipment metadata for
  layer, hidden body parts, lower-material behavior, color profiles/ramps, body
  parts, outfit parts, and special cloak/backpack parts.
- Verified armor/item presentation is separately exposed through
  `BlueprintItemEquipment.EquipmentEntity` and alternatives. The production
  class currently copies Fighter shared, male, female, and color fields; this
  remains unchanged.
- Inspected native Fighter, Barbarian, and Paladin definitions as structural
  benchmarks, then inventoried class, item-linked, and bounded raw native
  streams. Public Visual Adjustments research was used only as architectural
  precedent; the installed assembly and live resource library are authoritative.

## 2026-08-30 - Guarded catalog checkpoint

- Added the default-off, save-free `gunslinger-outfit-audit` request to the
  existing guarded runtime harness. It dynamically discovers supported player
  races and class/item/raw resource sources; loads and describes exact links;
  emits sorted, deduplicated ignored JSON; hashes the deterministic candidate
  set; and records that it did not mutate a save, inventory, progression, or
  avatar.
- The first guarded run (`20260830T1956227219163Z`) failed closed without an
  exception. It exposed three audit defects: Unity returned
  `Application.version=UNKNOWN`, race blueprints included duplicate enum IDs,
  and already-known resources were excluded from raw-source classification.
  The harness was narrowed to the exact loaded assembly hash/MVID, distinct race
  enum IDs, and independent source classification.
- The second guarded run (`20260830T2005018122430Z`) passed eight of nine
  assertions and again had zero exceptions. Its sole failure showed that
  Kingmaker maps equipment entities as bare names such as
  `EE_Armor_LeatherRanger_M`, not only path-qualified `/EE_...` names. A focused
  test and the smallest exact `EE_` classifier correction were added.
- The third guarded run (`20260830T2012181937219Z`) passed all nine assertions:
  49 class sources, 163 item-linked sources, 361 bounded raw sources, 1,206
  unique loaded entities, 3,816 matrix rows, 4,878 resolved links, zero
  unresolved links, zero inspection errors, and nine dynamically discovered
  player-race enum IDs across both genders.
- Passing candidate-set ID:
  `dd81603f583444f335381d72cc69b73f1c036c4625e8227cb1e1f9db18603357`.
  Catalog SHA-256:
  `73af097a4dd21fe905d2f9b4388f2ef6a68503f4b6723040e1dd00d3e3e2e294`.
  Raw catalog and runtime batches remain ignored local evidence.
- During the passing launch pipeline, repository validation passed, all
  `1362/1362` domain/reflection tests passed, source compilation passed, and the
  generated installable and local-runtime packages passed strict standalone UMM
  validation. These results qualify the audit checkpoint, not outfit aesthetics.
- The explicit clean checkpoint command
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` then passed
  repository validation, all 1,362 tests, clean Release compilation, package
  creation, and strict package validation.
- The immediately following runtime-preflight invocation hit the repository's
  known post-build timestamp check
  `unsupported-does-not-build-or-stage-package`. The unchanged rerun after
  outputs became quiescent passed all 157 checks. Both outcomes are recorded;
  the passing quiescent result is the integration gate.

## 2026-08-30 - Guarded candidate-render checkpoint

- Added the default-off gunslinger-outfit-candidate-render request. It is
  restricted to the guarded autonomous harness and exactly
  KMG_AUTOMATION_WORKING.
- The renderer uses request-local disposable Human actors. For each gender it
  snapshots exact avatar entity order, primary/secondary ramps, and saved
  equipment links; removes the donor class clothing; applies only the audited
  candidate IDs; captures native-default and alternate valid ramps in
  no-weapon, production-pistol, and production-musket states; restores and
  verifies the snapshot; then disposes actors, items, cameras, textures, and
  request-local blueprints.
- The first serious batch is exactly six coherent native presentations:
  complete Bard, complete Alchemist, complete Magus, cap/cape-free Ranger,
  cap/cape-free Rogue, and cap-free Slayer. The fixed matrix has 32
  gender-specific audited links. Ranger/Rogue/Slayer caps and Ranger/Rogue
  capes remain structurally excluded.
- Focused guard/catalog/matrix tests and runtime preflight pass. The complete
  dependency-free suite passes 1365/1365; exact-reference Release compilation
  passes.
- .\scripts\Build-Local.ps1 passed repository validation, a clean complete
  domain run, exact-reference Release construction, deterministic packaging,
  and strict standalone UMM validation at 2026-08-30T21:05:28Z.
  The standalone and local-runtime packages are byte-identical at SHA-256
  693c09684256fab77b4835b78eff12ab974c2bc460a63824f877768cd9c16ce8;
  the staged DLL SHA-256 is
  17bfe03b52e85cab627be425c680b1ccf6db88275ba4e253081065685304e377.
- A launch attempt was rejected before build, deployment, or game start because
  the guarded harness requires a clean Git state. This was a source-state gate,
  not a runtime failure. The qualified renderer must therefore be committed and
  published before its first installed-game run.

Exact next action: commit and publish this reusable guarded-render checkpoint,
verify all three feature refs, then run the renderer from that exact clean
commit and directly inspect every generated candidate image.

## 2026-08-30 - First renderer request rejection and repair

- Published renderer checkpoint
  189ae46fa19552fa3b906740d9f30372c588f7f5 and verified local HEAD,
  the local feature ref, and the origin feature ref were identical.
- Guarded evidence directory
  20260830T2109519221444Z-gunslinger-outfit-candidate-render loaded that
  exact commit and DLL SHA-256
  49ca6f72b5ca5ad25611916bb5cb7b2eb7aa21b9dfc47f64cf4d1c7e217c2f30
  through Steam App ID 640820.
- The request failed closed at request acceptance with
  sanitizedReason=scenario-timeouts-not-allowed. The request run ID was not
  copied into the rejection result, so the outer orchestrator also rejected
  the envelope. Structured evidence records hookInstalled=false,
  uiActionOccurred=false, saveActionOccurred=false, and
  guardedRequestAccepted=false. No candidate was rendered or scored.
- Root cause: the renderer was correctly configured with the established
  working-save timeouts in the PowerShell scenario catalog but was missing
  from the in-mod workingSmoke predicate that permits those exact fields.
- Added only GunslingerOutfitCandidateRender to that predicate and extended
  the focused guard test to extract and verify the predicate itself.
- Repository validation and all 1365/1365 tests pass. Build-Local passed
  exact-reference Release construction and strict validation of byte-identical
  standalone/local packages at SHA-256
  fb40935682a9d74fb28d12e99d76c93f490848c2d29abec11079398bd52e5f72;
  the DLL SHA-256 is
  c32ca0ef557d09bbaf5f032001dddc843bc057dc346fa6185d6a0bed680d8eb8.
- Runtime preflight first observed the known immediate post-build timestamp
  guard, then passed all 160 checks unchanged once outputs were quiescent.

Exact next action: commit and publish this narrow request-contract repair,
verify the three feature refs, and rerun the renderer from the new clean
commit.

## 2026-08-30 - Disposable avatar class-source repair

- Published request-contract repair
  166d7d0756de2f7cc4ab1cbbfea515e9b5aad081 and verified the three
  required feature refs were identical. The earlier rejected process was
  responsive and had performed no UI or save action; it accepted a graceful
  main-window close and exited. Force termination was not used.
- Guarded run
  20260830T2119065677129Z-gunslinger-outfit-candidate-render loaded the
  exact published commit through Steam, accepted the request, positively
  identified and loaded KMG_AUTOMATION_WORKING, obtained a stable post-load
  fingerprint, observed no save-writing API, and exited automatically.
- The scenario then failed closed before the first candidate at
  settle-male-human because StartGamePregenFighterUnit produced a valid Male
  Human Medium rig but Progression.GetEquipmentClass() returned null.
  Cleanup passed and restored the exact party/global-unit snapshots; zero
  candidates, images, or scores were accepted.
- The prior guarded audit proves native Fighter class
  48ac8db94d5de7645906c7d0ad3bcfbd and its exact three Human links per
  gender. The renderer now preserves a reported equipment class when present
  and otherwise loads that exact Fighter donor class. It records the source,
  original entity names/layers, donor names/layers, and exact intersection
  count instead of treating the optional live field as mandatory.
- The focused matrix test now rejects the obsolete optional-field requirement
  and requires the exact audited Fighter fallback and added diagnostics.
- Repository validation, game-facing compilation, and all 1365/1365 tests
  pass. Build-Local passed exact-reference Release construction and strict
  standalone/local package validation at SHA-256
  55241abdbea95d5e273fc18372cf2f1b4636a406b7990a924146260476ed9af2;
  the DLL SHA-256 is
  74bc1c9209f561eeceea68456e6b697f5fe77caf7a37e3643e6aef05a829cb10.

Exact next action: pass quiescent runtime preflight, commit and publish the
audited donor fallback, verify the three refs, then rerun the serious render
batch from that clean commit.

## 2026-08-30 - Complete Human render batch and scored shortlist

- Published donor-fallback checkpoint
  `9de7c4ef40483150ffba40782deb71714d2a0307`; local HEAD, the local
  feature ref, and the origin feature ref were identical before launch.
- Guarded evidence directory
  `20260830T2130124467293Z-gunslinger-outfit-candidate-render` loaded that
  exact commit through Steam App ID 640820. The loaded DLL SHA-256 is
  `7fb96cd42ed986241fa63f79a52e01633da7c8b7bc18e1ed68d0a1562e4d5aac`.
- The outer orchestrator used the generic 120-second final-result deadline and
  timed out at 21:32:28Z, before the stable post-load fingerprint completed at
  21:32:33Z. It left the responsive game running and did not force terminate.
- The guarded in-game scenario continued safely, completed at 21:34:30Z,
  passed all ten assertions, removed its hooks, requested automatic exit, and
  exited. Its total request duration was 249,628 ms; the render phase after the
  stable fingerprint was approximately 117 seconds.
- Exact working save identification, descriptor/load correlation, completion
  callback, stable fingerprint, and no-save-writing evidence passed.
  Candidate set
  `ef38c5c841510df7f03bbf68a8ca9e7fbef3f3403369022505449cb038d347be`
  produced 48 records, 48 preview-like four-view images, 48 ordinary isometric
  images, 48 exact held states, 24 palette applications, and 12/12 exact
  restorations. `saveApiCalled=false` and
  `productionBlueprintMutated=false`.
- Directly inspected all 96 source images via temporary external contact
  boards; no capture or reference image entered the repository. Eleven
  ordinary-isometric records, primarily female Magus/Rogue, were tagged
  low-pixel-density, but their paired four-view previews were usable. Final
  qualification must produce stronger density or preserve this limitation.
- Weighted Human-stage ranking: Magus complete 81/100, Rogue capless/capeless
  75/100, Slayer capless 70/100. Magus is the provisional finalist. Race,
  animation, armor, overlay, rebuild, and persistence points remain withheld;
  production remains unchanged.
- Bard, Alchemist, and Ranger were omitted from the best three because their
  rendered silhouettes respectively read as packed traveler/commoner, bulky
  Alchemist, and bedroll-heavy wilderness class. No enumerated hard rejection
  was inferred merely from a below-threshold visual score.
- Added an exact scenario-only collector branch that grants this 96-image
  render a bounded `max(request timeout, 600) + 15` seconds without changing
  other scenarios or weakening request guards. The focused test requires that
  exact branch. Repository validation and all 1365/1365 Release domain tests
  pass after the repair.
- `Build-Local.ps1` passed at 2026-08-30T21:54:14Z: repository validation,
  1365/1365 tests, exact-reference Release construction, deterministic
  packaging, and strict standalone/local-runtime validation. Both packages
  are byte-identical at SHA-256
  `2f515302e2d0263adccb837b4e4f079d1120fcb0074054fae9ba4093aef76849`;
  the DLL SHA-256 is
  `7fb96cd42ed986241fa63f79a52e01633da7c8b7bc18e1ed68d0a1562e4d5aac`.
  Runtime preflight first encountered its known immediate post-build
  timestamp-invariance guard, then passed all 160 checks unchanged once the
  outputs were quiescent.

Exact next action: complete clean build/package validation, commit and publish
the collector/evidence checkpoint, verify all three refs, then rerun the same
Human batch so the outer orchestrator accepts the terminal PASS before adding
the finalist-only exhaustive qualification scenario.

## 2026-08-30 - Accepting Human render rerun

- Published collector checkpoint
  `8f47f2db723fdfe6146ca30c352ea83ba7d3589f`; HEAD, the local feature
  ref, and the origin feature ref were identical and the tree was clean.
- Canonical guarded command used `-TimeoutSeconds 600`, Steam App ID 640820,
  exact save `KMG_AUTOMATION_WORKING`, automatic exit, and no force
  termination. Evidence:
  `20260830T2158516580621Z-gunslinger-outfit-candidate-render`.
- The outer orchestration and in-game result both passed. All 10/10 assertions
  passed with no exception; `guardedRequestAccepted=true`,
  `saveInteractionOccurred=false`, `saveApiCalled=false`, and
  `productionBlueprintMutated=false`. Automatic exit initiated and no
  Kingmaker process remained.
- Loaded exact commit `8f47f2db723fdfe6146ca30c352ea83ba7d3589f`,
  package SHA-256
  `2fdaa2813262e237e687ce277d1a67cc56a8e8ce72c778dcdbd01da995b5d7f4`,
  DLL SHA-256
  `c9ace6013911f041e5e824c340b04e06d2b09a5a1bbdee5e123396853e0900c0`,
  and MVID `5a02e6db-4452-4a75-a2cf-f836f98a3407`.
- Candidate set
  `ef38c5c841510df7f03bbf68a8ca9e7fbef3f3403369022505449cb038d347be`
  reproduced exactly with 48 records, 96 images, 48 exact held states, and
  12/12 restorations. All 96 accepted images were directly inspected through
  temporary external boards. The Magus/Rogue/Slayer order is unchanged.
- All preview captures cleared the density floor. Eight ordinary-isometric
  captures were tagged low density, improved from eleven in the diagnostic
  batch; paired preview views remain usable. Pixel hashes differ between
  runs because live actors were captured at different animation phases, while
  catalog, IDs, cases, restoration, and structured evidence are deterministic.

Exact next action: implement the guarded finalist-only exhaustive matrix for
the Magus pair across dynamically discovered races, both genders, systematic
valid colors, required firearm/animation states, armor and overlay
interactions, rebuild, and persistence boundaries.

## 2026-08-30 - Finalist race/gender matrix source gate

- Confirmed from guarded audit evidence that native Magus class clothing
  resolves the same ordered two-entity pair for every one of the nine
  installed player races: male
  `6df8f61725a84294c8661bb9585eca97` /
  `4c59d2b9740930145a27a4c693217d22`, female
  `beba0e0c7dcd5c64d97d767be3e72995` /
  `a93ead19aae8afc4794c54f5bcf73168`. Native defaults remain 2/22.
- Added guarded scenario `gunslinger-outfit-finalist-race-matrix`. It
  discovers the installed player-race list dynamically, discovers native
  same-race/same-gender body donors deterministically, validates exact native
  Magus `LoadClothes` order in all 18 cells, and fails closed if any donor,
  rig, link, ramp, renderer, saved-link, or restoration contract is absent.
- The request-local matrix is deliberately bounded: both valid palettes,
  no-weapon readability, four-view preview-like plus elevated isometric
  capture, and exact avatar restoration for every race/gender cell. It makes
  no save call and does not mutate the Gunslinger or any production blueprint.
  Equipment overlays and engine-driven animation remain separate finalist
  gates so a sparse sample cannot masquerade as full Cartesian proof.
- Focused source invariants execute within the existing outfit-matrix test
  entry, preserving the fixed suite count. Repository validation, installed
  game-reference Release compilation, and all 1365/1365 tests pass.
- `Build-Local.ps1` passed clean Release construction, deterministic
  packaging, strict standalone validation, and strict local-runtime
  validation. Local-runtime package SHA-256 is
  `cdd85e981f9847b0259a965506db457af98818d25aaf7c87d619022eae9559dc`;
  DLL SHA-256 is
  `36cf201fca3040c3a7b9a35f4253207d87b5480b3f13b1df14897860fdb02b7b`.
- Runtime metadata preflight initially observed the known immediate-post-build
  artifact timestamp race, then passed unchanged after quiescence: 163 checks,
  with no process launch, deployment, backup, or evidence side effect from the
  negative unsupported-scenario fixture.

Exact next action: commit and publish this clean source-qualified matrix,
verify all three feature refs, then run it through Steam App ID 640820 against
exact save `KMG_AUTOMATION_WORKING` and directly inspect all ignored images.

## 2026-08-30 - First finalist matrix diagnostic and donor repair

- Published checkpoint `fe86bce4484d45ca8f6a6f7070bfd7942fd5a0fc`
  was identical at HEAD, the local feature ref, and the origin feature ref
  before launch. The canonical guarded command used Steam App ID 640820,
  exact save `KMG_AUTOMATION_WORKING`, `-TimeoutSeconds 1200`, automatic exit,
  and no force termination. Evidence:
  `20260830T2237589386140Z-gunslinger-outfit-finalist-race-matrix`.
- The run loaded mod `0.0.110`, MVID
  `985fe6cf-03f4-4120-9a8e-e586315c1135`, and DLL SHA-256
  `09fcb5096344aac82da288b8306b212b8b0dc44c9c7654f4d6515ff293a735a9`.
  Guard acceptance, exact working-save boundary, installed Assembly-CSharp
  identity, request-local cleanup, and automatic exit passed. No Kingmaker
  process remained; no save API or production blueprint mutation occurred.
- Male Aasimar completed both requested palettes: two records, four PNGs, ten
  views, and one exact post-outfit restoration. The first female Aasimar donor
  was `LibraryNPC02/967f70edf50093242949489c50c5fb65/Female/Aasimar/Medium`.
  Its original avatar state failed the pre-application exactness expectation,
  so the scenario stopped before applying the finalist to that body. The
  partial output is not used for aesthetic scoring or race-grid acceptance.
- Donor diagnostics also exposed
  `StartGamePregenClericUnit/a8c334ffb376dde44952ac805f85076e/Female/Halfling/Medium`
  as the initial female Halfling source. The repair filters Gnome/Halfling to
  Small and every other installed player race to Medium, retains all exact
  race/gender/size matches in deterministic priority order, and advances only
  after recording a rejected disposable donor.
- Acceptance now requires a full original-avatar remove/re-add probe before
  finalist application. Exact entity order, primary and secondary ramps, and
  saved links are recorded for every accepted donor; rejection records retain
  source identity, attempt index, reason, and detailed mismatch. The strict
  restoration contract was not weakened, and no campaign actor is involved.
- Repository validation and installed-game Release compilation pass. The
  complete domain suite passes 1365/1365. `Build-Local.ps1` then passed clean
  Release construction, deterministic package creation, and strict standalone
  plus local-runtime package validation. Pre-publication repair package
  SHA-256 is
  `255de7da0529767b089d65fbd9638fb4964020a562797f1c6048d3315014c624`;
  DLL SHA-256 is
  `c9840e31c00997b9c6d50b6f6b044175cbe34165d3f00414ce90fc7781040bef`.
  Runtime preflight first saw the known immediate post-build artifact
  quiescence check, then passed all 163 checks unchanged.

Exact next action: commit and publish the focused donor-retry repair, verify
all three feature refs and a clean tree, rebuild the commit-bound package, and
rerun the full guarded race matrix before inspecting any accepted images.

## 2026-08-30 - Exact-empty avatar baseline repair

- Published donor-retry checkpoint
  `a27c4a7ecb061bf972df799ee096dc1b31e5e62d` was identical at HEAD,
  local, and origin with a clean tree. Its commit-bound package SHA-256 was
  `945dbb864c94c9cf1be75ea455e44c321e501d7ba24f97f54e84fb476ca6525e`;
  DLL SHA-256 was
  `a4ffb80afa5a9574e5138bdd72b196708c23de9f7218cdb43322158f5ddbe1c7`.
- Guarded retry evidence is
  `20260830T2257046480918Z-gunslinger-outfit-finalist-race-matrix`. It
  used Steam App ID 640820 and exact working save
  `KMG_AUTOMATION_WORKING`, reached `final-result-received`, cleaned up,
  automatically exited, left no Kingmaker process, and recorded no save
  interaction or production mutation.
- Male Aasimar again completed two palettes/four images. Donor retry then
  exercised all six canonical Medium female Aasimar sources. Every source had
  a valid avatar but an empty original equipment-entity sequence; all six were
  rejected solely because the probe conflated zero entities with a missing
  avatar. This proves deterministic retry and rejection evidence while
  identifying a request-local instrumentation defect.
- An empty ordered sequence is exact when removal/re-add leaves it empty and
  saved links unchanged; its ramp comparison is vacuously exact. The repair
  rejects only a null avatar, records `originalEmpty`, and requires the same
  exact empty restoration in exceptional cleanup. It does not weaken any
  nonempty order, ramp, or saved-link comparison.
- Installed-game Release compilation and all 1365/1365 tests pass. The clean
  standalone/local-runtime package gate passes with pre-publication package
  SHA-256
  `3b7e2deb7b96dac8e62eba66d1628af2355e0ab2c4ff4259ab245e5710b3168a`
  and DLL SHA-256
  `8621f5402e652fbdc1b3eb7d0657d0450f3f5c00cfd861a02961c5563cb0e46f`.

Exact next action: publish this focused empty-baseline correction, verify the
three refs, rebuild its commit-bound package, pass quiescent preflight, and
rerun the complete matrix without changing the finalist score.

## 2026-08-30 - Complete matrix with unresolved cleanup delta

- Published empty-baseline commit
  `5116a127d92ca09ea13e5822439e6b833b47c7e7` was identical at HEAD,
  local, and origin with a clean tree. Commit-bound package SHA-256 was
  `d91a16126416a349e2fab9520eef1cd1febfc5a68361e4511c6d70b4eff06500`;
  DLL SHA-256 was
  `4fb8ef472bf4ac602b1861a0d1d42094d0995cdd3090bc7e8fee457ce04f26bb`.
- Guarded evidence:
  `20260830T2309022972406Z-gunslinger-outfit-finalist-race-matrix`.
  Loaded MVID `79cd33b7-76d0-4240-bd20-6bc51d4a5729`; Steam App ID,
  working-save identity, no-save behavior, and automatic exit passed.
- The substantive matrix completed without exception: 9 dynamic races, 18
  gender cells, 18 exact native link pairs, 18 accepted donors, zero donor
  rejections, 36 palette applications/records, 72 PNGs, 180 views, and 18/18
  exact avatar restorations. This closes no visual gate because terminal
  status remained FAIL.
- The sole failed assertion was final cleanup. Party references and actor-null
  state were intact, but the whole `State.Units.All` reference set did not
  equal its initial snapshot after 360 updates. Current evidence did not
  identify missing versus unexpected units. The game log showed third-party
  level-up errors and pet enumeration during disposable-body activity, but
  that is a clue rather than attribution.
- Added diagnostic-only cleanup evidence: initial/current unit and party
  counts, exactness booleans, and safe descriptions of every missing and
  unexpected reference. The strict whole-world cleanup condition is unchanged
  until installed-game evidence distinguishes a leaked scenario actor from
  ambient or third-party population churn.
- Repository validation, installed-game compilation, all 1365/1365 tests,
  clean Release construction, and strict standalone/local package validation
  pass. Pre-publication diagnostic package SHA-256 is
  `368140973c5e42aacf420168159b30b4a48fe26c7476984a282f621b529721f2`;
  DLL SHA-256 is
  `93edda11b82111e8a76c1c2298e7260ae142e8d1c68ba127e004b6cef7ea24aa`.

Exact next action: publish the diagnostic-only checkpoint, verify three refs,
rebuild, pass quiescent preflight, and rerun once to obtain the exact cleanup
delta before changing cleanup ownership semantics or accepting images.

## 2026-08-30 - Exact dependent-unit cleanup attribution

- Published diagnostic checkpoint
  `051cab77cf30941e86a94d40aeac2cb4d619c5c5` was identical at HEAD,
  local, and origin with a clean tree. Its commit-bound local-runtime package
  SHA-256 was
  `4bccb9e8ce5cf6f415eb97c117f01f6353e76dd5b74fa40989c70afeecb03ac9`;
  DLL SHA-256 was
  `38f89aaf7607ee39c899a4abcfd1e1d2732b4d0528163aa0d06fe9ae6c0d8179`.
- Guarded diagnostic evidence is
  `20260830T2323563433313Z-gunslinger-outfit-finalist-race-matrix`.
  Steam App ID 640820, exact working-save boundary, installed-game identity,
  no-save behavior, production non-mutation, final-result collection, and
  automatic exit all passed.
- All substantive matrix assertions again passed: 9 races/18 gender cells,
  18 accepted donors, exact native links, 36 palette records, 72 PNGs/180
  views, and 18/18 exact avatar restorations. Images remain unaccepted because
  terminal status is still FAIL.
- Cleanup evidence is now exact: initial `State.Units.All` count 265, final
  count 266, no missing references, party 3/3 exact, and the disposable actor
  cleared. The sole unexpected reference is runtime unit
  `e8019935-e26e-4be8-a799-c00d8fb7a26f`, name `Leopard`, blueprint
  `AnimalCompanionUnitLeopard`, GUID
  `54cf380dee486ff42b803174d1b9da1b`. The matrix used native female-Elf donor
  `StartGamePregenRangerUnit` (`29c3981ecfbfac6479cd3d0b2b2f3f4c`).
- The repair does not delete arbitrary world deltas. It captures a dependent
  only through the active disposable actor's installed-game
  `UnitDescriptor.Pet` reference and only if that exact reference was absent
  from the initial unit snapshot. It retires that exact request-owned unit,
  records its lifecycle, and still requires the original complete global-unit
  and party reference sets to be restored.
- Repository validation, installed-reference compilation, all 1365/1365
  tests, clean Release construction, and explicit strict package validation
  pass. Pre-publication package SHA-256 is
  `ddb92778082adc354b1e574abad9a467a10246c17cefa75ab61281f410feab62`;
  DLL SHA-256 is
  `af8262f6593053ceadf56af84c26e56e61d38964b816ed39896ce7b5f7885b39`.

Exact next action: publish the relationship-scoped cleanup checkpoint, rebuild
the commit-bound local-runtime package, pass quiescent preflight, and rerun the
complete matrix. Acceptance requires structured proof that the unexpected
Leopard is the exact disposable donor's `Descriptor.Pet`, that it is retired,
and that the original strict snapshots are restored before direct image review.

## 2026-08-30 - Mechanical PASS rejected after complete visual review

- Published cleanup commit
  `8b8d0b17aa90318425404efac56f6977bb2ad11c` was identical at HEAD,
  local, and origin with a clean tree. Its commit-bound package SHA-256 was
  `79f2c09c9114a17bfbed21751f764ea0c273b7e30816523c8952b311d441afc5`;
  DLL SHA-256 was
  `4fcd0e88529e3d474f4924a77322c334e9974159b8d160f36f038d13f96bd9e7`.
- Guarded evidence
  `20260830T2341080018300Z-gunslinger-outfit-finalist-race-matrix`
  reached terminal PASS with loaded MVID
  `3595f627-40de-4b76-830b-99920d2838ac`. It proved 9 races, 18
  gender cells, 36 records, 72 PNGs/180 views, 18/18 restorations, exact
  native links/palettes, no save or production mutation, and exact cleanup of
  265/265 global units and 3/3 party references.
- The relationship record captured one exact `UnitDescriptor.Pet` for the
  male-HalfElf fixture: `AnimalCompanionUnitLeopard`
  (`54cf380dee486ff42b803174d1b9da1b`). It was registered globally but not in
  party, then retired and absent. This supersedes the prior female-Elf source
  inference.
- Every one of the 72 PNGs was inspected in eight temporary review boards
  outside the repository. The batch is rejected. Several NPC prefabs retained
  arbitrary baked clothes or equipment even when their `CharacterAvatar`
  baseline was empty: shields/swords, bows/quivers, capes, a large greatsword,
  and race-inappropriate donor outfits remained visible. Male Elf, male
  HalfElf, male Tiefling, female Aasimar, female Gnome, female Human, and
  female Tiefling provide concrete examples. The temporary review boards and
  raw images remain ignored and outside the repository.
- This invalidates the fixture's visual neutrality, not the finalist's
  provisional 81/100 score. Production remains unchanged and no race-grid
  visual gate closes from this run.

Installed-game reflection/IL inspection established the native player-doll
path. `BlueprintRace.Presets` supplies exact gender skeleton and skin;
`DollState` deterministically owns race preset, appearance choices, class
clothes, and ramps; `CreateData()` creates `DollData`; and
`CreateUnitView(false)` builds a character-generation view. The repaired
scenario spawns that view using the same-race/gender descriptor blueprint,
requires all preset/doll entities and no unexpected avatar entity, clears
every `UnitBody.AllSlots` item, and checks both active and inactive hand models.
An empty baseline now fails closed because a real player doll must be nonempty.

Repository validation, installed-game compilation, all 1365/1365 tests, clean
Release construction, package creation, and explicit strict package validation
pass. Pre-publication package SHA-256 is
`04f13af8fd17a0d9e18611e13c3cc3d27d83f6c7cf1e7dca3b05e094e5f73d18`;
DLL SHA-256 is
`d3ec07a2238ff2c062686dfc4e570ee602afaa716a26ddfa01607cb2627653bc`.

Exact next action: publish this neutral-character-creation-fixture checkpoint,
verify all three refs, rebuild its commit-bound runtime package, pass guarded
preflight, rerun all 18 cells, and directly inspect all 72 replacement images.

## 2026-08-30 - Aasimar visual-race semantics fail closed

- Neutral-doll commit `b67ec5444d4b3ef8480007c10fb2d73bab3c031e` was
  published with exact HEAD/local/origin refs and a clean tree. Its
  commit-bound local-runtime package SHA-256 was
  `17ef9c3ffd53514099b15780a97430d361088fe27c3ed275ab66475abcb401ef`;
  DLL SHA-256 was
  `87330635b5e02107ec184b5b6ecce3ca2834c7cb3b776bc401c0ecb41363c339`.
- The standalone runtime preflight initially observed the known
  immediate-post-build artifact timestamp window, then passed all 163 checks
  after the artifact tree became quiescent. No launch occurred while it was
  red.
- Guarded evidence is
  `20260831T0013309100348Z-gunslinger-outfit-finalist-race-matrix`, loaded
  MVID `f557435b-3d2a-4b8b-be4f-97de26665088`. It failed during
  initialization with `No complete character-creation race preset exists for
  Male Aasimar`; zero fixtures and zero images were created. Working-save
  identity, no-save behavior, exact 265/265 global units and 3/3 party,
  production non-mutation, cleanup, hooks removal, and automatic exit passed.
- Root cause: the new resolver incorrectly required
  `BlueprintRaceVisualPreset.RaceId == BlueprintRace.RaceId`. The former is a
  visual-body identity and native playable races may share it; it is not the
  progression race identity.
- Installed `Assembly-CSharp.dll` IL establishes the exact engine contract:
  `DollState.Validate` uses `BlueprintRace.Presets` followed by native
  `FirstOrDefault` in serialized order, and `DollData.CreateUnitView` passes
  `RacePreset.RaceId` to `KingmakerEquipmentEntity.Load`. The correction uses
  `race.Presets[0]`, loads skin with `fixture.Preset.RaceId`, and records
  `racePresetVisualRaceId` separately. The focused test forbids the obsolete
  equality predicate.
- Repository validation, installed-reference compilation, all 1365/1365
  tests, clean Release/package construction, and independent strict package
  validation pass. Pre-publication package SHA-256 is
  `e6af511660abba47fd22dae853f6875ed31c1bd68607cc60440fe640f62c9502`;
  DLL SHA-256 is
  `edbc636195bd0b1fe80e41df7bdf532236502135da819570e07980a99a645824`.

Exact next action: publish the native visual-race correction, verify all refs,
rebuild its commit-bound package, pass quiescent preflight, and rerun the full
matrix. No visual or score gate changed.

## 2026-08-30 - Native doll view lifecycle fails closed

- Commit `55c487cc460c4950305d47e3c679bf8e858c943d` was published with
  exact HEAD/local/origin refs and a clean tree. Its commit-bound runtime
  package SHA-256 was
  `5ff29573950c002977c9f35e3d290bf3662a510f172c327489de6c6f9876a4b1`;
  DLL SHA-256 was
  `1be20efa6c457eb8da426b54f67598c3529cfc76c5c62454eea7ce9654e1897c`.
- Quiescent preflight passed all 163 checks. The guarded Steam `640820` run
  `20260831T0026335779530Z-gunslinger-outfit-finalist-race-matrix` loaded
  that exact commit/DLL and MVID `e496489f-2fbc-47cf-a4c1-da914eda915a`.
  It failed at `spawn-male-aasimar` because the unbound doll view's
  `CharacterAvatar` property was null. Zero fixtures and zero images were
  created. The named working-save guard, no-save/no-production boundaries,
  cleanup, hooks removal, and automatic exit passed.
- Installed `Assembly-CSharp.dll` IL proves `DollData.CreateUnitView` gets
  and configures the root `Character` component, whereas
  `UnitEntityView.OnDataAttached` later assigns `CharacterAvatar` using
  `GetComponentInChildren<Character>()`. The probe now checks
  `dollView.GetComponent<Character>()` before spawn and continues to require
  `_actor.View.CharacterAvatar` during post-attachment readiness.
- The focused test both requires the native root-component check and forbids
  `dollView.CharacterAvatar`. Repository validation, all 1365/1365 tests,
  clean installed-reference Release/package construction, and strict package
  validation pass. The pre-publication package SHA-256 is
  `024d0c2b89a6e561b4c8d6eecc67e6f30c6b85941b893db7f9dcc6d5d22b0f2e`;
  DLL SHA-256 is
  `3cf170e14b0dc96910b093ee0737e713fd7d0c432a20cd59971c36dfc7be7d42`.

Exact next action: publish this lifecycle correction, verify all three refs,
rebuild its commit-bound package, pass quiescent preflight, and rerun the
complete matrix. No visual, ranking, or production gate changed.

## 2026-08-30 - Native doll-view ownership fails closed

- Lifecycle commit `08bfed17843adf348b210883b6f929b1af7c5678` was
  published with exact refs and a clean tree. Its commit-bound package
  SHA-256 was
  `ad125fccd577368077ff6784ac6c94f102a22d5a95a7e34be75d63ceccee323c`;
  DLL SHA-256 was
  `9ebe80a42b3711dcc874357792da4b5a2e797eb0db18cd7a8d7f7d9a5e374db8`.
- After the known immediate-post-build timestamp window, unchanged preflight
  passed all 163 checks. Guarded evidence
  `20260831T0044105199782Z-gunslinger-outfit-finalist-race-matrix` loaded
  the exact commit/DLL and MVID
  `f8abfd0e-59da-48c0-a796-15f085984c32` through Steam `640820`.
- The view attached and every one of five male-Aasimar donors proved exact
  gender, progression race, size, humanoid rig, and empty active/inactive
  weapon models. Each failed after 360 updates with
  `dollEntityCount=5`, `dollExact=false`, and `rendererCount=0`. Zero
  captures resulted. Exact 265-unit/3-party cleanup, no save API, no
  production mutation, hooks removal, and automatic exit passed.
- Installed IL establishes that `SpawnUnit(BlueprintUnit, UnitEntityView, ...)`
  always `Instantiate`s the supplied view. The already-instantiated
  `DollData` view's runtime `Character` equipment collection did not survive
  that second clone. Public `SpawnEntityWithView` attaches and registers the
  supplied existing view without cloning it.
- The corrected fixture mirrors native identity initialization (blueprint,
  unique ID, transform), calls `SpawnEntityWithView`, requires
  `ReferenceEquals(_actor.View, dollView)`, and transfers ownership by clearing
  the local cleanup reference. The focused test forbids the prior
  `SpawnUnit` call shape. Repository validation, 1365/1365 tests, clean
  installed-reference Release/package construction, and strict validation
  pass. Pre-publication package SHA-256 is
  `d1dfe7cf3697e5757ce0bc86d7f0e2af72a621e98e4021c5ff5101511885a0ec`;
  DLL SHA-256 is
  `5462960ebbfd8815523b2132e84d7b2377dfc52a051b2ccdb04a646bf33e7108`.

Exact next action: publish the direct-view ownership correction, verify refs,
rebuild its commit-bound package, pass quiescent preflight, and rerun the
complete matrix. Candidate score and production remain unchanged.

## 2026-08-30 - Mechanical PASS rejected by direct image review

- Commit `141c6a8e1fcdacdb61164113ac77a6191b16254e` was published
  with exact HEAD/local/origin refs and a clean tree. Its commit-bound runtime
  package SHA-256 was
  `835e61db8f1a4b59e45cfec7421eab3b48322e49d355041449a679369b2d4a4b`;
  DLL SHA-256 was
  `d5d28e5e974b655cfcd5411aa9ceb726b2de00588a935bad5ababbc520b7c3f4`.
- Standalone preflight passed 163/163. Guarded Steam `640820` evidence
  `20260831T0058130079392Z-gunslinger-outfit-finalist-race-matrix`
  loaded that exact commit and DLL, then reached terminal `PASS`:
  9 races, 18 fixtures, 36 records, 72 PNGs, 180 views, 18/18
  restorations, exact unit/party cleanup, no save API, no production mutation,
  and process exit.
- Labeled review boards were created outside the repository and every one of
  the 72 ignored runtime PNGs was inspected. Male and female bodies otherwise
  showed the coherent Magus pair across all races and both palettes. The
  female-Human native and alternate records visibly retained an oversized
  two-handed sword; original-resolution review confirmed it in front, side,
  rear, three-quarter, and isometric presentation. The matrix is visually
  rejected.
- Structured evidence identifies the contaminated source as
  `AmiriLevel20_Companion` /
  `ca08eabf5f6a33e4ba366e889e4fecdc` with
  `clearedSlotItemCount=14`, `rendererCount=2`, and
  `noWeaponModels=true`. The old assertion checked current
  `HandsEquipment.GetWeaponModel` references only; removal after entity
  creation could leave the donor's already-instantiated renderer visible.
- Installed reflection proves `BlueprintUnit.UnitBody` is a
  public-constructible nested type with explicit active hand, hand-set, armor,
  accessory, limb, empty-hand, and quick-slot fields. The repair assigns a new
  empty body only to the cloned disposable blueprint before
  `SpawnEntityWithView`, preserves the source
  `EmptyHandWeapon`, clears starting inventory, and rejects any donor
  for which `ClearAllQualificationEquipment` still finds an item. It
  neither mutates a source blueprint nor hardcodes the failed donor.
- Focused contracts require the clone-before-neutralization order, exact zero
  created items, and the generic fail-closed rejection. Repository validation,
  all 1365/1365 tests, clean installed-reference Release/package construction,
  and strict standalone validation pass. Pre-publication package SHA-256 is
  `be1b6048c299f1d996db1091372c8e6c43863f51bae7b287ee58ca76f3c92bbb`;
  DLL SHA-256 is
  `68489bd17dd3bb363bbf53464beda0f7011cc10a7725212b31ef60127c80e13d`.

Exact next action: publish the neutral-body correction, verify all refs,
rebuild its commit-bound package, pass quiescent preflight, rerun the complete
matrix, and directly inspect every replacement image. The candidate remains
provisional at 81/100 and production remains unchanged.

## 2026-08-30 - Replacement race matrix accepted

- Neutral-body commit `47d6c55f6742219dac07824b08e1daa1c23309a1`
  was published through the approved helper; HEAD, the local branch, and
  origin were identical with a clean tree.
- Its commit-bound local-runtime package SHA-256 was
  `36b3a6e096df511b689ab91126c3dad6845398fd64a3559558cdc8da32870104`;
  DLL SHA-256 was
  `57f9d7dec390cae8f53a78fadb9bd8c5cadb30368c97b5eadd8e454806ce285c`.
  After the known timestamp-quiescence window, unchanged preflight passed all
  163 checks.
- Guarded Steam `640820` evidence
  `20260831T0125478276325Z-gunslinger-outfit-finalist-race-matrix`
  loaded that exact commit/DLL and MVID
  `1bace4ca-657e-4d4b-bccf-d9ee4933876e`, then reached terminal `PASS`:
  9 races, 18 fixtures, 36 records, 72 PNGs/180 views, 18/18 restorations,
  exact cleanup, no save API, no production mutation, and automatic exit.
- All 18 accepted fixtures recorded `requestLocalNeutralBody=true`,
  `clearedSlotItemCount=0`, and no weapon model. Three unsafe donor attempts
  were recorded and rejected before capture: one non-round-tripping male
  Aasimar donor and the two female Tiefling donors with Kineticist tattoos.
- Eight labeled boards outside the repository cover all 72 ignored PNGs and
  were directly inspected. The female-Human native and alternate preview and
  isometric cells no longer show Amiri's greatsword. No race/gender cell shows
  donor clothing, weapon contamination, missing geometry, broken material, or
  unacceptable hair/ear/horn/tail loss. Both palettes remain coherent.
- The race/gender/color/no-weapon gate is accepted. `magus-complete` gains the
  seven previously withheld coverage points and advances to 88/100
  (26/23/15/15/9). Production is still unchanged at this exact checkpoint.

Exact next action: implement an independently owned Gunslinger appearance
policy containing the selected male/female Magus link pairs and 2/22 defaults,
replace only the Fighter-derived appearance assignments, and add focused
observable blueprint-state/non-mutation tests.

## 2026-08-30 - Production appearance binding passes local gates

- Added a pure `GunslingerClassAppearanceCatalog` containing only the accepted
  ordered male/female native IDs and default colors 2/22. Every accessor
  validates and returns a fresh defensive copy.
- Added `GunslingerClassAppearance.Apply`. It resolves all four installed
  `EquipmentEntity` resources before mutation, constructs fresh
  `EquipmentEntityLink` objects/arrays plus a fresh empty shared array, and
  then assigns only the new Gunslinger class. The Magus donor class is never
  resolved or mutated; Fighter now supplies only `StartingGold`.
- Added two focused cases covering exact values, defensive copies,
  null/malformed/duplicate/count failures, atomic resource-first wiring,
  project/factory integration, and rejection of all former Fighter aliases.
- The first repository-validation attempt correctly failed on the old 1365
  count. The active count/static evidence moved to 1367 and the one missing
  inherited count propagation was added to the 0.0.106 validator. The first
  test compilation then exposed three malformed escaped literals; replacing
  them with quote-character composition fixed the test, without weakening its
  assertions.
- Repository validation and the complete 1367/1367 Release domain/reflection
  suite pass. A clean installed-reference Release build, package construction,
  production firearm/SoundBank validation, and strict standalone package
  validation also pass.
- Pre-publication package SHA-256 is
  `34d9a7005fd9f535c33e460d7b4e23dc94553dbbcd34ee45540aeff167476df0`;
  DLL SHA-256 is
  `6f039e773910a314f6abf46e2bd0d87d737660abd898d1ea7bd58918d11893eb`.
  No runtime, equipment, animation, rebuild, or persistence gate changes yet.

Exact next action: commit and publish the production binding, verify HEAD/local/
origin identity, rebuild its commit-bound local-runtime artifact, pass
quiescent preflight, and run guarded `working-save-smoke` before building the
production outfit equipment/motion scenario.

## 2026-08-30 - Published production build passes canonical save smoke

- Committed the focused production binding as
  `bf3e052cb3a91691e214ec9a87c025f25f380c2d` and published it only through the
  approved policy helper. `HEAD`, the local branch ref, and the matching origin
  feature ref were identical.
- The clean commit-bound local-runtime build passed repository validation,
  1367/1367 tests, installed-reference Release construction, package creation,
  and strict package validation. Package SHA-256 is
  `4a91c92b9f842b7744adf707a2149ae13a4cc1ec70733979ad453406548a6c61`;
  DLL SHA-256 is
  `78c8a7e8d8c1372bea930e4a48b4211ef4941974a062c1dbb707b0a8b7a1b8f5`;
  MVID is `41fd1851-9dec-4adf-87eb-0e79763d5e02`.
- The first immediate preflight attempt failed closed only because the new
  artifact had not satisfied the fingerprint-quiescence interval. No game or
  save interaction occurred. After waiting without modifying the artifact,
  all 163 preflight checks passed.
- Launched the canonical guarded `working-save-smoke` through Steam App ID
  640820. Evidence directory
  `20260831T0159136175513Z-working-save-smoke` reached terminal `PASS` with
  exact commit/DLL/version identity, a complete 111-entry catalog, one exact
  working save, one distinct protected baseline, exact receiver-correlated
  load order, completion callback, stable post-load fingerprint, no save API,
  hook removal, and automatic process exit.
- The stable fingerprint records game ID
  `dce769e0-229c-4bfd-b8ea-e2d572bf8472`, party count 3, and a nonnull main
  character reference. This is safe-load evidence, not yet outfit rebuild or
  persistence evidence. The candidate remains 88/100.

Exact next action: add a narrowly guarded production compatibility scenario
and focused tests, then repeat complete source qualification before rendering
and directly inspecting equipment, weapon, motion, rebuild, and persistence
states in game.

## 2026-08-30 - Production compatibility harness passes local gates

- Added guarded scenario `gunslinger-outfit-production-compatibility`, wired
  through the exact request catalog, parser, runner, script allowlist, and
  preflight surface.
- The harness resolves the actual production Gunslinger class and proves its
  exact selected link pairs/default colors across every installed race and
  both genders before it creates a fixture. It does not reuse the Magus class
  as a behavioral proxy.
- Male and female Human fixtures use native character-generation
  `DollState`/`DollData`, a request-local neutral body, `SetClass` on the
  production blueprint, and a real compatible hair entity. Exact-signature
  reflection is isolated to the installed private `DollState.GetHairEntities`
  and `Character.m_ShowBackpack` boundaries; production appearance code stays
  reflection-free.
- Each gender has 16 named states: default/no weapon, alternate color, held
  pistol, held musket, inactive/stored musket, held blunderbuss, light armor,
  light-armor removal/rebuild, heavy armor, heavy-armor removal/rebuild,
  tricorn, tricorn removal/hair restoration, cloak, cloak removal/rebuild,
  backpack visible, and backpack removal/final rebuild. Expected output is 32
  sidecars, 64 ignored PNGs, and 160 views.
- Each transition asserts link-backed resource identity, exact body slots,
  ramps, saved links, blueprint immutability, and paired preview/isometric
  capture. Cleanup requires exact actor/global state restoration; save-writing
  calls are forbidden. Motion/fire/reload/melee and persistence remain
  explicitly separate gates.
- Installed-reference compilation exposed two useful API facts before
  publication: `EquipmentEntity` has no public asset-ID property, so evidence
  uses the owning wrapper link plus resource-reference equality; hair
  enumeration and raw backpack state are private, so the harness uses exact
  installed signatures and public mutation paths.
- Repository validation and all 1368/1368 Release domain/reflection tests pass.
  The clean Release/package and strict standalone validator also pass.
  Dirty-tree package SHA-256 is
  `b6da46f4c1a7c61fab0625762b46f5f7c222f6d478811300fdfa041512f409d6`;
  DLL SHA-256 is
  `1ca246f477ed3ccbd6ef7a194fc90a5b5a14671d2334bbbaf0a08b76236b9d8`.

Exact next action: commit and publish this harness through the approved policy
helper, verify HEAD/local/origin identity, rebuild the exact commit, pass
strict package validation and quiescent preflight, then execute and directly
review the full production compatibility render matrix.

## 2026-08-30 - First production matrix fails on a false hidden-storage assumption

- Compatibility harness commit
  `82361d31d2b0d7d278046161c13ee503aff6d51a` was policy-published with
  identical HEAD/local/origin refs and a clean tree.
- Its commit-bound local-runtime package SHA-256 was
  `d8f026bec05a99ebc98eb61545e99c6af5d1662fd7446a3d628f28a26037bd44`;
  DLL SHA-256 was
  `5265ce9925c4c5b3dd4b2ef90bd0f14d5707edd9d871d9959e77bb060c943562`;
  MVID was `3b3ab851-e16f-48e6-b900-43c4d78b2558`. The first immediate
  preflight fingerprint check saw post-build timestamp movement; the unchanged
  quiescent rerun passed all 166 checks.
- Guarded Steam 640820 run
  `20260831T0304180367838Z-gunslinger-outfit-production-compatibility`
  reached terminal `FAIL` at male-Human `musket-stored-inactive` after four
  complete records/eight PNGs. Exact game/mod identity, all 18 production
  race/gender link rows, save guard, class immutability, global cleanup, and
  automatic exit passed.
- The failed assertion expected `!Renderable(heldModel)` for an inactive
  musket. The exact item slot, production entities, hair, saved links, ramps,
  overlay/base sets, and backpack state all remained correct. Existing
  qualified weapon-presentation logic proves that a long gun's stored model
  is supposed to remain visible; designated handgun profiles alone may be
  intentionally hidden.
- Repaired only the harness: the existing active-presentation resolver is now
  internal to runtime-test code, the matrix requires a visible out-of-combat
  stored musket, the model participates in framing, and each firearm record
  includes its resolved presentation role and renderability. The focused
  source contract covers this boundary.
- Repository validation, 1368/1368 tests, clean installed-reference Release
  build/package, and strict standalone validation pass. Dirty-tree package
  SHA-256 is
  `beed41bbe74601d8d0f499c2ff5dff340f3e90822e02d4fa2e0f25cd69ab6baa`;
  DLL SHA-256 is
  `397f4c6a5a9069ae07e0ee2cfd195aa88d2a8fa1d82edaed49b983f68efa3396`.
  No candidate score changes from the rejected partial batch.

Exact next action: commit and policy-publish the stored-musket correction,
verify all refs, rebuild the commit-bound package, pass quiescent preflight,
then rerun and directly review the complete compatibility matrix.

## 2026-08-30 - Second matrix exposes a premature native-doll mutation

- The stored-musket repair was committed and policy-published as
  `453f54732c05be6141d3eec259e4c46325f047e0`. Its commit-bound package
  SHA-256 was
  `bd3934c4acdfb42ca369753ce29d523f6a3391badfb39a224254ef265b6e1fda`;
  loaded DLL SHA-256 was
  `d9be26094a0eb8fd6f86dcff5572e85756ff311f1db12d22699ca4311c2b1388`;
  loaded MVID was `0c09675f-81e2-44f2-b98d-f14dd0ee619e`.
- Guarded Steam 640820 evidence
  `20260831T0319410552031Z-gunslinger-outfit-production-compatibility`
  reached terminal `FAIL` before its first capture. It produced zero records
  and zero PNGs. All 18 production race/gender links, exact game/mod/class
  identity, protected working-save/no-write boundary, class immutability,
  cleanup, and automatic exit passed.
- At the end of the old post-mutation settle window, class clothing, saved
  links, and empty-weapon state were exact, but selected native hair
  `9edf6b60bbf4d834facd4789837a3e0b` was absent. The preceding run used the
  same hair and retained it through four states. This is scheduler-sensitive
  fixture lifecycle evidence, not an aesthetic or compatibility rejection.
- Read-only inspection of the installed assembly with the game-bundled
  metadata library confirmed that `DollData.CreateUnitView` creates the
  initial avatar entities, while `UnitEntityView` and `Character` continue
  native start/class-equipment/rebuild work after attachment. `RebuildOutfit`
  rebuilds render objects but does not establish `DollData` membership. The
  already-accepted race-matrix harness waits for every resolved doll entity
  before mutation; this compatibility harness had not.
- The narrow correction now requires the exact descriptor `DollData`, every
  resolved native doll entity, the selected hair, the humanoid rig, active
  renderers, and an empty out-of-combat weapon presentation to survive the
  full 30-update native settle window before taking the snapshot or adding
  production entities. Failure records the active entity names. No production
  class, item, weapon, or appearance behavior changed.
- The first complete test invocation found one line-break-sensitive token in
  the new source-contract assertion; correcting that test expectation yielded
  1368/1368 PASS. Repository validation, clean installed-reference Release
  build, firearm/SoundBank validation, package construction, and strict
  standalone validation pass. Dirty-tree package SHA-256 is
  `f7e0b896470a4fc120e6d9f8d7166ca1d6bdfaf7a94c53b1545ba73b12ea073c`;
  DLL SHA-256 is
  `79f5f5138ea94c37b202d21b9320513a1986c78975d9fd3dd78bd8eeb1e8dd76`;
  MVID is `1e6d17a7-bb7c-4e5a-b36f-19e64b59969c`.

Exact next action: commit and policy-publish this readiness correction, verify
HEAD/local/origin identity, rebuild the exact commit, pass strict validation
and quiescent preflight, then rerun the complete matrix. Only a terminal PASS
followed by direct inspection of all 64 replacement PNGs can close this gate.

## 2026-08-31 - Complete production compatibility matrix accepted

- Readiness correction commit
  `59eb7a97d6c1278f1e4e0d351aa6d4557b2db566` was policy-published with
  identical local/remote refs and a clean tree. The commit-bound package
  SHA-256 was
  `e15546c561d244f5f29517bec79f71025713cbd79530238ff69232f38fb18394`;
  loaded DLL SHA-256 was
  `10f1beaf90eb6f5578ab5c8c09f9d10b219d587bb2adb11b308a959a7a422b26`;
  loaded MVID was `780b053b-acb8-4716-a5b5-87b578e356e0`.
- Guarded Steam 640820 run
  `20260831T0344513197562Z-gunslinger-outfit-production-compatibility`
  reached terminal `PASS`: 9 races, 18 exact production links, 2 exact Human
  fixtures, 32 ordered state records/sidecars, 64 PNGs, 160 views, 2/2 exact
  restorations, immutable production blueprint, no save API, exact cleanup,
  hook removal, and automatic exit.
- Independently parsed every record and sidecar and rehashed every capture.
  The reconciliation passed with zero issues: 32/32 sidecar/index matches,
  64/64 byte/hash matches, exact production pairs and 35x35 ramps, exact
  default 2/22 and alternate 13/4 applications, cleared prior states, native
  hair/saved-link preservation, correct held/stored weapon roles, and exact
  armor/head/shoulder/backpack transitions.
- Directly inspected all 64 images through 16 labeled boards: male and female,
  preview-like and isometric, four state groups each. No missing body part,
  material failure, severe clipping, baked weapon duplicate, lost hair, or
  stale equipment geometry was present. Light/heavy armor, tricorn, cloak,
  and backpack all override and clear cleanly; held and stored firearms leave
  the outfit intact.
- All previews passed density checks. Eight female isometric images were
  conservatively flagged low-density, with a minimum 11,278 meaningful
  pixels; every one was directly legible and paired with a clear four-view
  preview. This limitation is retained rather than hidden.
- Static equipment and rebuild compatibility is accepted. The candidate stays
  at 88/100 because the five reserved compatibility points remain bundled
  until motion/animation and persistence both pass.

Exact next action: build the bounded motion/fire/reload/melee runtime gate,
fully source-qualify and publish it, then run and directly inspect the complete
commit-bound installed-game result.

## 2026-08-31 - Production motion harness source-qualified

- Read-only reflection over the installed `Assembly-CSharp.dll` resolved the
  exact locomotion contracts. `UnitMovementAgentBase` supplies nullable
  `MaxSpeedOverride`, live velocity/movement flags, and `TickMovement`;
  `UnitAnimationManager` supplies `Speed`, `WalkSpeedType`, and actions. The
  exact Slow/Normal enum type is in
  `Kingmaker.Visual.Animation.Kingmaker.Actions`. No GUID or API was guessed.
- Reused the accepted production doll settlement lifecycle as a partial
  session, while giving `gunslinger-outfit-production-motion` its own guarded
  request identity, runner field, result/index, script metadata, preflight
  assertions, and 1,800-second collector window.
- Added eight exact actions on each production gender: unarmed idle; musket
  slow walk, normal run, and right turn; pistol and musket `UnitAttack`;
  production musket `UnitUseAbility` reload; and native Shortsword melee.
  Attack frames are ready + 1/12/36 + acted; reload frames are ready +
  1/12/36/96/160/240 + acted. Expected output is 54 PNGs, 54 sidecars, and
  216 views.
- Each capture records the exact class/pair/ramp/hair/saved-link/rig state,
  item/presentation role, movement or command state, relevant diagnostics,
  image hash/bytes/pixels, immutable blueprint, and `saveApiCalled=false`.
  Movement requires accepted native `UnitMoveTo`/`ForcedPath`, nonzero
  velocity/displacement, Slow/Normal modes, and distinct observed speed.
- Success and failure cleanup remove actor, target, dependent, items, firearm
  state, and clones; restore original avatar and movement settings; restore
  exact shared powder/ball counts; and require original unit/party snapshots.
- Initial validation correctly stopped on the deterministic suite-size guard.
  The one new focused test raised the current inherited count from 1368 to
  1369. The next compile exposed and corrected the exact installed
  `...Kingmaker.Actions` namespace plus two over-assumed capture-summary
  fields and missing known source namespaces. No behavior was weakened.
- Final local gates: repository validation `PASS`; focused contract `PASS`;
  full Release suite `1369/1369 PASS`; clean installed-reference Release build
  `PASS`; firearm/SoundBank checks `PASS`; strict standalone package
  validation `PASS`; runtime preflight `169 PASS`.
- Pre-commit clean package SHA-256:
  `00c80de81ff7acc218c1bbf08e51623950281f90e74e5750fee685da48b6e9be`;
  DLL SHA-256:
  `c60baee8be07590b39c30a8685bde51e277bb13d8f9d0b226fb9f3950a1e4abd`;
  MVID: `a9e50b0b-b2e1-42f4-aa91-c9cdf98d4c5c`.
- No runtime or visual claim is made from these source/build checks. The
  candidate remains 88/100.

Exact next action: commit and policy-publish this coherent harness, verify all
three refs, rebuild/install the exact commit, pass quiescent preflight, execute
the guarded Steam 640820 motion scenario, reconcile all records/files, and
directly inspect every generated image before persistence work begins.

## 2026-08-31 - First production motion run failed closed

- Harness commit `3071fe38a61b79131f96f965053e7bc058ce209f` was
  policy-published with identical HEAD/local/origin refs. Its commit-bound
  local-runtime package SHA-256 was
  `5eb5da0e740b3d84801c256721f921b636db5471d676cd00de98e99f245d2db7`.
- Guarded Steam 640820 run
  `20260831T0455599323551Z-gunslinger-outfit-production-motion` reached
  terminal `FAIL`, not ambiguity. Save name/version/game identity,
  production-blueprint immutability, no-save, exact inventory/unit cleanup,
  hook removal, and automatic exit all remained protected.
- The male Human fixture completed all eight actions and emitted 27 records:
  live slow walk/run/turn, pistol and musket attacks, production reload, and
  Shortsword melee all reached their native acted/counter boundaries. The
  female production doll and exact outfit then instantiated and emitted its
  unarmed-idle record, for 28/54 total records.
- The next female slow-walk stopped at the clean-combat guard. Both disposable
  male combatants had received `LeaveCombat` and were disposed, but the
  cached `Game.Player.IsInCombat` boundary had not yet been recomputed.
  This is a harness lifecycle defect, not visual acceptance and not a score
  change.
- Installed 2.1.7b reflection and IL inspection identified the registered
  `UnitCombatJoinController.Tick()` lifecycle. It invokes
  `Player.UpdateIsInCombat()` to recompute the cached flag from controllable
  groups and raises the matching party-combat event when the value changes.
  The narrow repair snapshots player/party/turn-based state, ticks that native
  controller only after both request-local combatants leave, records
  before/after facts per fixture, and requires exact restoration. The
  locomotion guard remains fail-closed and reports each predicate separately.

Exact next action: complete every required source/build/package gate, publish
the repair, rebuild/install that exact commit, and rerun the full 54-record
matrix. The 28 partial captures are diagnostic only and will not be mixed
with replacement qualification evidence.

### Repair source gate

- Repository validation and diff hygiene: `PASS`.
- Focused motion contract and complete Release suite: `1369/1369 PASS`.
- Clean installed-reference Release build, firearm/SoundBank validation, and
  strict standalone package validation: `PASS`.
- Stable guarded-runtime preflight: `169 PASS`. The first invocation directly
  after each clean package build detected only a transient artifact-tree
  timestamp/directory fingerprint change; backup, evidence, CIM, and process
  guards all remained unchanged. The identical settled-tree rerun passed.
- Pre-commit package SHA-256:
  `7de0fc0ce93a703907a10d5862368083765dae831cd74487073988128538889d`;
  DLL SHA-256:
  `b378256b722350bc9128b491e7f0d8e8f3a2b630bdccefe4664fb5c80f84e18f`;
  MVID: `b4bf5593-d05b-41d5-b92c-d6ad1eff1356`.

These are pre-commit identities and do not qualify runtime behavior. Exact
next action remains commit, policy publication, commit-bound rebuild, and a
complete replacement Steam run.

## 2026-08-31 - Second motion run failed closed at group retirement

- Repair commit `fe24655acd4516e334796524ab7a3f40fd633888` was
  policy-published with identical HEAD/local/origin refs. Its commit-bound
  package SHA-256 was
  `5228a562f65fbb2b694ec617548e71d1b713c3fea35d93789834b36eccebd44e`;
  DLL SHA-256 was
  `ba1638817210bfa9b2d163356465719cc0d22e941947286c3d399bf3f236a9dc`;
  MVID was `90286a4d-27e5-476d-82eb-1b1cbb3ac3a9`.
- Guarded Steam evidence
  `20260831T0521459019080Z-gunslinger-outfit-production-motion` reached
  terminal `FAIL` after all 27 male records. No female record was attempted.
  Every male locomotion, turn, firearm attack, production reload, and melee
  boundary completed, but none is accepted from this partial batch.
- Initialization proved a clean false/zero/false combat baseline. After the
  disposable pair retired, the boundary record showed
  `player=true->true;party=3->3;turnBased=true->true`. Native attacks had
  enlisted the three real party members; the join controller correctly
  recomputed that still-live group state.
- Exact build/save/game identity, blueprint immutability, no-save,
  ammunition restoration, target retirement, hook removal, and automatic
  exit remained protected. Structural cleanup was true, while the combat
  boundary and therefore total cleanup correctly failed.
- Read-only installed 2.1.7b IL inspection found the registered missing
  stage. `UnitCombatLeaveController.Tick()` evaluates group retirement and
  calls full `UnitEntityData.LeaveCombat()`; that method updates combat state,
  interrupts AI commands, updates hands/audio, and raises the unit event.
  `UnitCombatJoinController.Tick()` then recomputes the player cache and
  raises the party event. No inspected method was invoked by the metadata
  probe.
- The harness now resolves both registered controllers and requires
  leave-then-join order after target/actor retirement and in cleanup.
- Repository validation, diff hygiene, focused invariant, full `1369/1369`
  Release suite, clean installed-reference build, firearm/SoundBank checks,
  and strict package validation pass. The first preflight after the build
  reported only `unsupported-does-not-build-or-stage-package`; the identical
  settled-tree rerun passed all 169 checks.
- Pre-commit package SHA-256 is
  `b3598b28366eb82161b66b1e65144430c9461380dc93dc3dd2bb15db9fd7fbb3`;
  DLL SHA-256 is
  `9f717b6c8d08f39cd67635bfc5e635543e38d60a38ce215a0a5c4f590cadfa41`;
  MVID is `6e2a6987-f89a-42c1-a3ad-e1635a47b796`. These dirty-state
  identities do not qualify runtime behavior.

Exact next action: commit and policy-publish this coherent repair, rebuild the
exact commit, then execute and inspect a complete replacement batch. The
score remains 88/100.

## 2026-08-31 - Third motion run exposes low-level leave-event bypass

- Published commit `df4f3f04f55bbbdfe56ef113f723f89af23fa62a` rebuilt as
  package SHA-256
  `fa29aab259ef800d0db3ab11ccf6bd3b82999760778733523ef2737dfec348dc`,
  DLL SHA-256
  `876879b6ab7f1cd2a376e8f43ed74109722f4841eb335179c20dad463ad0b651`,
  and MVID `c162b31d-1195-47ef-b8d7-685142f07801`.
- Guarded Steam evidence
  `20260831T0539205863874Z-gunslinger-outfit-production-motion` returned
  terminal `FAIL` after the full 27-record male matrix. The native boundary
  remained `player=true->true;party=3->3;turnBased=true->true`. No record in
  this partial batch is accepted.
- Exact commit/DLL/game/save identity, no-save, production-blueprint
  immutability, ammunition and target restoration, structural cleanup,
  hook removal, and automatic exit remained protected. Combat cleanup alone
  correctly prevented acceptance.
- Installed IL proved the request-local actor/target calls were one layer too
  low. `UnitCombatState.LeaveCombat()` omits `IUnitCombatHandler`;
  `UnitEntityData.LeaveCombat()` performs state exit, AI interruption,
  equipment/audio updates, and that event. `CombatController.HandleUnitLeaveCombat`
  removes the participant, and `CombatController.Tick()` refreshes cached
  `HasEnemyInCombat` when the unit set changes.
- The repair uses full unit leave for every actor, target, and dependent, then
  orders the registered turn-based cache tick, group-leave tick, and player
  recomputation tick. It records and restores exact combat, enemy-history,
  and sorted-unit baselines. The focused test forbids low-level actor/target
  exits. Installed-reference compilation and all `1369/1369` tests pass.
- Repository validation, clean Release/package validation, firearm/audio
  validation, and the settled 169-check preflight pass. The first preflight
  reported only `unsupported-does-not-build-or-stage-package`; the identical
  settled-tree rerun passed.
- Pre-commit package SHA-256 is
  `ae22f6d1804ef1d4b9677d0a55c57dd3371c0340b63284f76e94e7bd8b5120f3`;
  DLL SHA-256 is
  `d0ba5261d5cf26d0b57534f060fbcba7407b1c4f0c421230f99ea8de2dcdcd75`;
  MVID is `40e11afc-987d-4755-a057-df54bbfd09bf`. These dirty-tree
  identities are source/package evidence only.

Exact next action: commit and policy-publish the repair, rebuild its exact
package, and execute a complete replacement batch. Candidate score remains
88/100.

## 2026-08-31 - Fourth motion run exposes player-faction fixture coupling

- Commit `f127e1f25f0d6d562a27a56ce9fe23f9b1ab8044`, package
  `66d97da08b4615991210cf74e5f0784d1de3c8910dfcecac78d779ec96f6dbed`,
  DLL `ea7c0b4931fbd32587aa9451b2c3475613bb866cc3658ad9dc67b63abfe7229e`,
  and MVID `1f1de511-e4f9-4f52-98e0-ec2127a56494` ran through Steam 640820.
- Evidence `20260831T0601202638447Z-gunslinger-outfit-production-motion`
  failed at 9/54 records before male musket attack. The prior pistol command
  reached all fixed frames, acted, and discharged exactly once. No partial
  image is accepted.
- Exact identity, game, save-name, no-save, blueprint, inventory, target,
  structural cleanup, and automatic-exit guards held. The player and
  turn-based combat caches remained true, so cleanup correctly failed.
- Installed IL proves player-faction units use the directly-controllable group.
  It also proves live group memory can rejoin a conscious target between
  actions. Repeated controller cleanup is therefore the wrong fixture design.
- The replacement clones two factions request-locally, makes only that pair
  mutually hostile, creates and retires one fresh target per attack, and
  requires zero player hostility/shared group plus exact player caches at every
  tick and capture. Cleanup destroys all faction, target, blueprint, and memory
  state. Compile, repository validation, and `1369/1369` tests pass.
- Clean Release/package, firearm/audio, and strict package validation pass.
  The first preflight reported only the known
  `unsupported-does-not-build-or-stage-package` stabilization sentinel; the
  unchanged rerun passed all 169 checks. Pre-commit package SHA-256 is
  `78e8a067544d097c158aa77ce014fa9ccc0caf9863a6d2d9691492c7821cfd9c`,
  DLL SHA-256 is
  `db27ce97885fbba43df32c5bc804fde1ef81d3e6ed45c521c1bfd7386616cd9d`,
  and MVID is `f4bc8c6e-c148-4890-818c-34dba4f32f1a`. These are
  dirty-tree source/package identities, not runtime qualification.

Exact next action: commit and policy-publish the isolated fixture, rebuild its
exact package, then execute a complete replacement batch. Score stays 88/100.

## 2026-08-31 - Fifth motion run identifies cross-scene cache membership

- Published commit `1d2b1f8865b5ec12e57ea7dcc1ad25a8762eb63c`, package
  `8102f48085bed0830f746c52042e5b05e6a603dc36de49c556b052ec30863e71`,
  DLL `65c530ec491759987d026d86cb4400197eccd209cdb2ba641e774940edd22925`,
  and MVID `f420093c-fef2-4a76-ad47-21e79bbc5c2b` ran through Steam 640820.
- Evidence `20260831T0637014594621Z-gunslinger-outfit-production-motion`
  failed after four noncombat male records. Pistol setup observed
  `player=True/False;party=0/0;turnBased=True/False;units=2/0`. No partial
  record is accepted. Exact identity, no-save, blueprint, inventory, target,
  faction, structural cleanup, and exit guards passed.
- Rechecking the installed IL showed the prior conclusion was incomplete.
  Despite non-player factions and distinct groups, the shared actor spawn used
  the main character's `CrossSceneState`. `Player.AddCharacterToLists` adds
  every qualifying in-game cross-scene unit to controllable characters with no
  faction check; `UpdateIsInCombat` therefore counted the disposable group.
- The repair spawns actor and target in the exact loaded area's live
  `MainState`, requires that scene to differ from `CrossSceneState`, refreshes
  canonical player lists, and proves the exact controllable and cross-scene
  reference sets never change. Every sidecar and final assertion includes the
  area-local and non-controllable contracts. Compile and all `1369/1369`
  tests pass.
- Clean Release/package, firearm/audio, and strict package validation pass.
  The first preflight reported only the known stabilization sentinel; the
  unchanged rerun passed all 169 checks. Pre-commit package SHA-256 is
  `2c6bdf7ffe6901ef33ddf5ab908e195cb3ce0675d93fc974b8c2798de9a30077`,
  DLL SHA-256 is
  `81a315c486dae914ec04c63bd0079be1780c626d5031416c0f5c0c0d7ecf6651`,
  and MVID is `6ed1466d-9131-4b83-84e6-5f86c156a20f`. These are
  dirty-tree source/package identities only.

Exact next action: commit and policy-publish the scene-state repair, rebuild
its exact package, then execute a complete replacement batch. Score remains
88/100.

## 2026-08-31 - Sixth motion run rejects save-backed area ownership

- Published commit `27bc24ae9ce5b84d3eb8760741833697ed52a911`, package
  `9c97279edf78fb4f7540667b3e983b2c5b5b0b5ec98604c3fdea3b0e4bec3413`,
  DLL `37c764f27e63f984fd09b9ec80d465372e997e693269716b8b61e66f07eb98a3`,
  and MVID `38f7a207-baa3-4ee8-8774-c8d3de192b92` ran through Steam 640820.
- Evidence `20260831T1215532823796Z-gunslinger-outfit-production-motion`
  failed before its first record. The male-Human view attached and stayed
  weapon-empty, but `doll=False;hair=False;active=` remained exact through
  the bounded settle window. The player/party/turn-based boundary stayed
  false/zero/false; identity, no-save, blueprint, inventory, player lists,
  then-current global-unit cleanup, and automatic exit passed. That cleanup
  assertion did not inspect `MainState.AllEntityData`; no save API ran and the
  process exited. No partial image exists.
- Exact installed IL shows `SceneEntitiesState.AddEntityData` only adds the
  entity, assigns holding state, and raises the global event. The container
  need not be registered in `AreaPersistentState`. `IsSceneLoaded` is solely
  `SceneManager.GetSceneByName(SceneName).isLoaded`. It also shows ordinary
  `EntityDataBase.Dispose()` detaches the view but does not remove the entity
  from its holding state's list.
- The repair creates one request-local state bearing the exact live
  `MainState.SceneName` and marks it `SkipSerialize`. Actor and target retain
  live Unity rendering/navigation while belonging to neither the save-backed
  area state nor player cross-scene state. Every unit is removed through
  native `RemoveEntityData`; exact emptiness is required between fixtures and
  emptiness plus state disposal at terminal cleanup.
- Installed-reference compilation, repository validation, all `1369/1369`
  tests, clean Release/package, firearm/audio, and strict package validation
  pass. The first preflight reported only the known stabilization sentinel;
  the unchanged rerun passed all 169 checks. Pre-commit package SHA-256 is
  `64d07b6d3aa843aefb185cd2a07e4dce860ea46e522770e9eff7e9d16988981e`,
  DLL SHA-256 is
  `582e306bae50394eca161705b425c847bc08ba36e59ab23b36ac6fdfdd91a0d3`,
  and MVID is `43f248b1-be23-43f8-aaf9-78cb02a8f9cd`.

Exact next action: commit and policy-publish this request-local loaded-scene
container, rebuild its exact package, and execute all 54 replacement records.
Candidate score remains 88/100.

## 2026-08-31 - Seventh motion run proves the attack probe was live

- Published commit `b27438c7fd38d4e588a47b05b5e2329fb3676932`, package
  `788dcf4d89fac23941f79d9cca54db5f673bb5405125ca5e8817ef24553056e8`,
  DLL `9a0d1a9d671697f9a5a46c366cb6fe29af83dc528530e805987c55791ff21456`,
  and MVID `60f2fd26-9d78-401d-94d1-69a1c393afbe` ran through Steam 640820.
- Evidence `20260831T1253077289617Z-gunslinger-outfit-production-motion`
  failed after 10/54 records. Male locomotion, turn, and the full pistol
  schedule ran; the pistol acted and discharged exactly once. The musket-ready
  record then exposed zero loaded rounds, total fired count two, and a live
  `UnitAttack`, so the real command was correctly rejected as unloaded. No
  partial image is accepted.
- Guard, game/build/save identity, no-save, blueprint immutability, exact
  player and combat state, request-local loaded-scene ownership, structural
  cleanup, and automatic exit passed. This positively validates the scene
  isolation repair and localizes the failure to command preparation.
- Installed IL shows `UnitAttack.Init` plans attacks and approach radius without
  registration. `UnitCommands.Run` performs that initialization and registers
  a live command. The repair now calls `Init` only for the readiness probe,
  proves it never appears in actor commands, and reserves `Run` for the fresh
  evidence command. Sidecars and terminal outcomes require that boundary.
- Installed-reference compile, repository validation, all `1369/1369` tests,
  clean Release/package, strict firearm/audio/package validation, and the
  settled 169-check preflight pass. The first preflight reported only the known
  stabilization sentinel. Pre-commit package SHA-256 is
  `31498b7bed5b9532d0a208cda645b744cdfefa30b2a7246fab472696da7f0ce1`,
  DLL SHA-256 is
  `877b451e4a4a62751b3d1b75c217e24c2b66c857ec4d973e28bdfc5e23ef100d`,
  and MVID is `2ada3432-6aa8-4a77-81b1-934fe1a698f0`.

Exact next action: commit and policy-publish the detached-probe repair, rebuild
its exact package, and execute all 54 replacement records. Candidate score
remains 88/100.

## 2026-08-31 - Eighth motion run exposes non-interruptible attack teardown

- Published commit `5d520bbccaff98e09a9a94c3fa2c59811cd2f0ca`, package
  `a703f089ff28cc83c3d835df36de1180950d668b5230a6bcef9a7cc9fcf7eb6b`,
  DLL `7e8c1619acec69da73f10f6e5f3f6089a5d571163077fe9533193c4976763548`,
  and MVID `8b7060eb-cefe-4ac9-8be1-62d61a0e1974` ran through Steam 640820.
- Evidence `20260831T1330408485246Z-gunslinger-outfit-production-motion`
  failed after 10/54 records. Pistol and musket preparation both reported
  `probeDetached=True`, so the preceding repair worked. The pistol acted and
  discharged once; at update 36 its sidecar still showed a running
  `UnitAttack`. Musket-ready then showed `loadedRounds=0`, fired count two,
  and an active `UnitAttack`; production rejected the new unloaded attack.
  No partial image is accepted.
- The game log places a `UnitViewHandsEquipment.get_IsDollRoom` /
  `AnimateEquipping` null reference immediately after the pistol discharge and
  a second round consumption after the weapon switch. Installed IL shows an
  acted command remains non-interruptible until its animation finishes, and
  `UnitCommands.InterruptAll(true)` skips it. The evidence-backed cause is the
  old update-36 completion gate allowing target and weapon teardown while the
  pistol command remained live.
- The repair waits for `!IsRunning || IsInterruptible` after all scheduled and
  acted/discharge evidence exists. Attack outcomes carry retirement readiness,
  running/interruptible state, and update count. Teardown interrupts normally,
  then fails before any mutation if a running command remains; transient-state
  cleanup also requires none. Every sidecar lists its running command types.
- Installed-reference compilation, repository validation, all `1369/1369`
  tests, clean Release/package, strict firearm/audio/package validation, and
  the settled 169-check preflight pass. The first preflight reported only the
  expected stabilization sentinel. Pre-commit package SHA-256 is
  `17d46838be9b31b3fecda29ef582f2aae2cfc422e2f5c25be41f3d58811f2dbb`,
  DLL SHA-256 is
  `e1b154a9e2c35348d6b6d67cd9fa8274c4764ffa5604335a24e680ada14b5844`,
  and MVID is `bbd56913-905f-4d32-8546-cc3926bdaa2f`.

Exact next action: commit and policy-publish the native retirement gate,
rebuild its exact package, and execute all 54 replacement records. Candidate
score remains 88/100.

## 2026-08-31 - Ninth and tenth motion runs expose a finished resident attack

- Published commit `0dbdaf2b283bbb6245939d4078c26f90d94d01ff`, package
  `a8a6ae85f171e1c5140f17794830b0d11b64b4154af21a755332dd784ee570ca`,
  DLL `585e4abf748225398f13c02afbd62313e2111fd46137cd57415af331925efd40`,
  and MVID `a6768c5d-46e6-4fef-b45c-c2b958989d4e` ran through Steam 640820.
- Evidence `20260831T1401393847532Z-gunslinger-outfit-production-motion`
  failed before record one when the male Human native doll did not populate
  DollData or hair during the bounded settle window. Exact request, loaded
  build, working-save/no-save boundary, blueprint immutability, request-local
  cleanup, and automatic exit passed. Because attempts 7 and 8 passed this
  unchanged pre-action fixture, one controlled same-commit retry was made;
  another identical miss would have triggered readiness instrumentation.
- Evidence `20260831T1407213494923Z-gunslinger-outfit-production-motion`
  reached 10/54 male records. The pistol's update-36 sidecar showed a running,
  non-interruptible `UnitAttack`; its outcome later reached retirement ready
  and advanced. Musket-ready then showed no running command but a resident
  `UnitAttack`, zero loaded rounds, and two total discharges. Production
  correctly rejected the new unloaded musket command. No partial image from
  either failed run is accepted.
- Exact installed IL explains this second lifecycle boundary:
  `UnitCommands.InterruptAll(bool)` skips `IsFinished` entries without nulling
  their raw slots. Public `RemoveFinishedAndUpdateQueue()` clears those slots.
  Thus the pistol was no longer running when the harness advanced, but remained
  resident and later consumed the newly equipped musket round.
- The repair rejects any queued command, interrupts normally, calls the native
  finished-slot cleanup, and proves the evidence command is evicted plus the
  entire command container is empty of running, resident, and queued work
  before removing a weapon or target. Sidecars and terminal outcomes record
  the exact collections and `slotEvicted`; transient cleanup repeats the gate.
- Installed-reference compilation, repository validation, all `1369/1369`
  tests, clean Release/package, strict firearm/audio/package validation, and
  the settled 169-check preflight pass. The first preflight reported only the
  expected artifact-tree stabilization sentinel. Pre-commit package SHA-256 is
  `fca9cf06fb1fb6a3e967eb7414c3ffb4ac679d639695ab81faf146788921e274`,
  DLL SHA-256 is
  `1d2d17ffe350388308fab4aa62d81637d378ce44c2408ec8c2d34c365a3a418a`,
  and MVID is `d983a009-2e6c-41aa-ba32-56b9c20487f9`.

Exact next action: commit and policy-publish the finished-slot eviction repair,
rebuild its exact package, and execute all 54 replacement records. Candidate
score remains 88/100.

## 2026-08-31 - Eleventh motion run repeats the empty native-doll boundary

- Published commit `4ef28f65577d09329536a905976b405cac4562ef`, package
  `6f849b89c4ffba745585d268c1a1ff12c83074b2e5f80d13853e91e3c6c77a34`,
  DLL `871a89190537624f150356e381b106cb162b70a215936c780913642096cb01c4`,
  and MVID `10e8676b-e8d8-48f4-b4a1-210d0afe0d2f` ran through Steam 640820.
- Evidence `20260831T1438053243232Z-gunslinger-outfit-production-motion`
  failed before record one: male Human reached the settle timeout with no
  DollData outfit and no hair (`doll=False;hair=False;noWeapon=True;active=.`).
  No screenshot exists or is accepted. Exact guard/game/build, disposable
  working save, no-save, immutable-blueprint, structural cleanup,
  empty/disposed request-local scene, and automatic-exit contracts passed.
- This is the second occurrence of the attempt-9 boundary, despite attempt 10
  passing it. Per the retry contract, no unchanged-commit retry is allowed.
- Installed IL confirms `DollData.CreateUnitView(false)` creates a char-gen
  character and resolves/adds its equipment IDs before spawn. Entity spawn
  then queues attachment, whose `OnDataAttached` obtains, starts, updates, and
  rebuilds the view character. Existing evidence did not identify which side
  of this boundary was empty.
- The new diagnostic retains the original template character and records it
  after creation, after spawn before the next entity tick, after attachment,
  and at timeout. Each line captures resource-preloading state, Unity instance
  identity, raw/active/saved entity counts, DollData expected-ID count, active
  names, and template/attached reference equality. Fallback cleanup clears all
  retained diagnostic references. Tests enforce lifecycle ordering.
- Installed-reference compilation, repository validation, all `1369/1369`
  tests, clean Release/package, strict firearm/audio/package validation, and
  the settled 169-check preflight pass. The first preflight reported only the
  expected stabilization sentinel. Pre-commit package SHA-256 is
  `aa512f88878ef88d7486176080552f6ff3ac237f540a3f042d49d75842227112`,
  DLL SHA-256 is
  `0c97e7c7a7c450fa93fef6fcc42a523809302c9dc01934352ab06530cdc0583b`,
  and MVID is `47392b4f-cbc0-450f-9b72-82b284e578c7`.

Exact next action: commit and policy-publish the lifecycle diagnostic, rebuild
its exact package, and run attempt 12. Candidate score remains 88/100.

## 2026-08-31 - Twelfth motion run identifies donor-brain command ownership

- Published commit `2e73bf3035860ffc940c31f4e5c090b0f5d5df2e`, package
  `6d4e6b3aa27658e958f7010937a7d62e481988eb4eb5967fdc8d719dfbd94d5f`,
  DLL `90c727bcbd90ac962e7dd406c6bbc0c8f16f55ac05ff4ba8f812a9ff0e1f205d`,
  and MVID `6d5eafa3-919a-40fc-a39c-9206ab6ca58f` ran through Steam 640820.
- Evidence `20260831T1509405239304Z-gunslinger-outfit-production-motion`
  reached 10/54 male records, then failed closed when native construction of
  the musket attack was rejected as unloaded. Guard, exact game/build,
  working-save/no-save, blueprint immutability, request-local scene, cleanup,
  and exit passed. No partial image is accepted.
- Lifecycle instrumentation positively clears attachment: before spawn, after
  spawn before tick, and after attachment, `Preloading=False`, avatar instance
  `-654738`, raw/active counts `5/5`, saved count zero, and expected outfit-ID
  count four were unchanged. The active names remained body, face, eyebrows,
  short hair, and the empty facial-hair entity.
- At musket ready, the firearm had zero rounds and total discharge count two.
  The readiness probe was detached and the harness command was not installed,
  yet resident and running collections both contained `UnitAttack`. The prior
  pistol action had already passed empty-container teardown, proving a new
  unowned command appeared between actions.
- Both request-local blueprint clones inherited the donor NPC brain. Native
  `JoinCombat`/`Engage` activated that brain and allowed it to attack before
  the harness. The replacement clears `Brain` only on those disposable clones,
  requires an empty command container before an attack frame, rejects an
  evidence command carrying `AiAction`, and records all ownership facts.
- Installed-reference compilation passed. The initial complete test run had
  one source-string assertion mismatch caused by a wrapped diagnostic literal;
  the assertion was narrowed to its two exact fragments and the unchanged
  implementation then passed all `1369/1369` tests.
- Clean Release/package construction, strict package/firearm/audio validation,
  and the settled 169-check preflight pass. Its first pass reported only the
  documented artifact-tree stabilization sentinel. Pre-commit package SHA-256
  is `b3c73cf63e68fa3cb4aff086bd236accf3769e69f53a5afe7c259776139d76e2`,
  DLL SHA-256 is
  `3c95c8c5115135023023ff74d4c77cbc3aaf90ff7c0ca742c3e397e1741c839d`,
  and MVID is `5a199838-48eb-49a2-8b92-7ca8d0dfabe2`.

Exact next action: commit and policy-publish the clone-AI isolation, rebuild
the exact commit, and run attempt 13. Candidate score remains 88/100.

## 2026-08-31 - Thirteenth motion run proves a pre-creation resource race

- Published commit `934785962bb4ef752993add5558d20cb751f1c7d`, package
  `a53c3314dd6aeb5d4ee13a8f0b5615d93325212062f1c5916ef0aa9460f88e5f`,
  DLL `af2af437dd06f55c1316305190e02aee86f95a1d0f0c2364b48b4eb032c7fff1`,
  and MVID `0e441834-5f14-41d6-b1ad-15d46b4f976e` ran through Steam 640820.
- Evidence `20260831T1548069712324Z-gunslinger-outfit-production-motion`
  failed before record one. Guard, exact game/build, disposable working save,
  no-save, immutable-blueprint, request-local scene, cleanup, and exit passed;
  no image exists or is accepted.
- Before spawn, after spawn, and after attachment, preloading was true and the
  same avatar instance had zero raw/active/saved entities for four expected
  IDs. At timeout preloading was false, but the avatar was still empty.
- Installed IL proves the cause: `DollData.CreateUnitView(false)` calls
  `TryGetResource(id, false)` once for each ID; during preloading that API
  returns null immediately, and the created avatar has no deferred retry.
  Native game code also demonstrates waiting for preloading to become false
  before resource-dependent work.
- The repair adds a 360-update pre-creation wait, hard-checks the flag again at
  `CreateUnitView`, records wait updates and creation-time preloading state,
  and requires that evidence in both static and motion terminal contracts.
  It does not load resources itself or relax native doll validation.
- Installed-reference compilation and all `1369/1369` tests pass.
- Clean Release/package construction, strict package/firearm/audio validation,
  and the settled 169-check preflight pass; its first pass reported only the
  expected artifact-tree stabilization sentinel. Pre-commit package SHA-256
  is `8aba976c9550a3c09b95539dee11d7825362169b0933b546837cb2e34d25c378`,
  DLL SHA-256 is
  `379f0bc2a1612065b3ae53539b391f11ac20161be18b4a0dfb0f47bba8803a89`,
  and MVID is `5a3b66e8-97b3-4d55-b7e0-db500ca82c96`.

Exact next action: commit and policy-publish the repair, rebuild the exact
commit, and run attempt 14. Candidate score remains 88/100.
