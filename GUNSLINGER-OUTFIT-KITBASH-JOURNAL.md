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
