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
