# Expanded Summoning: alternate summoning sources

Status: development checkpoint, not a release. Source, identities, explicit
template contracts and focused domain coverage are in place, and a focused
guarded native campaign ran on development candidates (see "Native
coverage"). Only what that section lists was exercised in game; "Remaining"
lists what was not. Nothing here claims that all summoning is fixed.

## What this adds

The published Expanded Summoning roster is projected onto the direct Summon
Monster (SM) and Summon Nature's Ally (SNA) class callers that do not use the
eighteen ordinary spell parents.

| Family key | Caller | Provider | Summon family / tiers | Host shape | Identities |
|---|---|---|---|---|---:|
| `MonsterTactician` | Inquisitor Monster Tactician | native | SM I-IX | one direct ability per tier and quantity, granted by 9 exact features | 584 |
| `Summoner` | Summoner (also Twinned Summoner, which keeps the same features) | Call of the Wild | SM I-IX | tier I direct; II-IX CotW variant wrappers | 561 |
| `MasterSummoner` | Master Summoner | Call of the Wild | SM I-IX | as above | 561 |
| `Occultist` | Arcanist Occultist, Conjurer's Focus | Call of the Wild | SM I-IX | as above | 561 |
| `Naturalist` | Summoner Naturalist (CotW's shared SNA implementation, identifiers say `FeyCaller`) | Call of the Wild | SNA I-IX | as above | 563 |
| `FeralHunter` | Hunter Feral Hunter | Call of the Wild | SNA I-IX | as above | 563 |
| `Preservationist` | Alchemist Preservationist, Bottled Ally I-VI | Call of the Wild | extract levels 1-6 cast SNA I, II, IV, V, VII, IX | tier I extract direct; others extract parents with variants | 353 |

Identity counts are each family's projected creature placements (registered
or withheld), plus retained native wrappers, plus frozen copies of direct
roots. That makes 3,746 identities in all.

## How it works

Source:
- `src/KingmakerGunslinger/Summoning/AlternateSummoning*.cs`
- `src/KingmakerGunslinger/Blueprints/AlternateSummoningAdapters.cs`

1. **One roster.** `AlternateSummoningProjection` reads the existing
   `ExpandedSummoningCatalog`, `SummonVisibilityCatalog` and
   `SummonNativeExpansionCatalog`. A caller at tier T offers exactly the
   ordinary spell's tier-T placements with the ordinary quantity rule. No
   caller keeps a creature list.
2. **Exact host identity.**
   - `AlternateSummoningSourceCatalog` pins every CotW root, granting feature
     and quantity template by the GUIDs that CotW assigns to those names. CotW
     reads its embedded name table, which is identical to the shipped
     `blueprints.txt` (SHA-256 `f227b1c3...`).
   - All 231 pairs were cross-checked against the installed registry.
   - Native Monster Tactician roots are reached only through their nine exact
     granting features. The quantity of each granted ability is read from its
     lineage (next item), never from its name.
3. **Lineage.** A host child descends from a canonical native option only
   when its exact set of spawned unit GUIDs equals that option's set, within
   the same tier and quantity. No match, or more than one, means *unknown*,
   and an unknown option is always preserved.
4. **Caller mechanics from the caller.** Each adapter child is a deep clone of
   the caller's own template for its tier and quantity. That carries over:
   - ability type and action type
   - range
   - resource and exact spend, including Occultist's tier cost and Perfect
     Summoner reduction
   - class rank scaling and duration
   - summon pools and their clearing, including Master Summoner's
     free-pool/eidolon conditional
   - caster checkers and eidolon conditions

   Before anything is cloned, the template must match its family's explicit
   contract (`AlternateSummoningContracts`), derived from the installed
   provider and engine: quantity expressions, spawn and conditional structure,
   resource identity and spend, custom cost, class and rank, duration, pool
   and clear routing, caster checkers and prerequisites. A template that does
   not match leaves the whole family unpublished with a precise reason; there
   is no general graph interpreter.

   Only two things are composed in, both read from the canonical child of the
   same placement: the spawned unit, and the canonical template or alignment
   action. Those are the dynamic celestial/fiendish template for templated SM
   creatures and caster alignment for SNA. Name, description and icon are the
   canonical child's own references; no new text or art.
5. **Menu composition** follows the canonical publisher's rules
   (`AlternateSummoningMenuPolicy`):
   - An umbrella that the native expansion catalog splits is replaced by its
     split options.
   - An established semantic duplicate collapses only when exactly one
     published project creature replaces it at that quantity.
   - Every other host option, including other mods' options, stays by
     reference and order.
   - A direct root becomes a menu parent whose children include a frozen copy
     of its original cast. There is no parent/child cycle and no duplicate
     cast action.
   - A direct root that would gain nothing is left untouched.
6. **Gate on the executed choice.** Every adapter child carries
   `AlternateSummoningAvailability`. That gate makes a child castable only
   while its family's publication holds it in a host menu. So withholding a
   creature, turning the module OFF, a blocked family, or a saved action-bar
   shortcut to an unexposed child cannot reach it. Adapter clones clear
   Kingmaker's cached availability-provider and checker arrays, so the gate
   cannot be bypassed by a cache copied from an already-read host ability.
7. **Menu layout.** The existing scrolling variant-menu layout extends to
   exact adapter-published roots (`AlternateSummoningExposure.IsPublishedRoot`:
   an exact GUID that still carries the adapter variants component). Nothing
   is matched by name.

## Lifecycle and load order

- **When it runs.** `AlternateSummoningCoordinator` reconciles on the first
  Unity Mod Manager update after `LibraryScriptableObject.LoadDictionary`.
  This is the established Aid Another pattern. By then every
  `LoadDictionary` postfix, including CotW's class construction, has
  finished.
- **Why that matters.** The static audit found four CotW routes (Fey Caller /
  Naturalist, Feral Hunter, Shadow Beasts, Bottled Ally) that copy the
  canonical parents' `Variants` at CotW load with table-assigned GUIDs. Any
  unknown child name throws `Missing AssetId` and aborts the rest of CotW's
  initialization. Adapters therefore never publish a name before CotW has
  copied, independent of mod load order.
- **Readiness.** Reconciliation waits for this package's own blueprint
  lifecycle and roster. If that never becomes ready within 600 s, every
  unsettled family is reported as blocked.
- **Dependency detection.** CotW is detected through its live UMM entry; no
  CotW type is loaded. The outcomes are:
  - Absent: its families are `AbsentInProfile`. The native Monster Tactician
    still reconciles.
  - Inactive or ambiguous: its families are `Blocked`.
- **One transaction per family.** Each family is resolved, registered (its
  own `BlueprintRegistry`), built and published as one transaction:
  - Any failure rolls back that family's host menus. The rollback refuses to
    overwrite a root another mutation has changed since.
  - It also rolls back that family's identities, records a precise `Blocked`
    reason, and leaves other families, the ordinary spells and other modules
    untouched.
  - Repeating reconciliation never republishes.
- **Module OFF.** Every family's identities still register and are built,
  so saved references resolve, but nothing is published: no host menu
  changes and no child is exposed, so the availability gate keeps every
  adapter child uncastable.
- **Blocked family.** Identity availability is separate from graph
  adaptation. When a family cannot be adapted (an unsupported template, a
  fault drill, an exception during build or publication), its host menus
  are rolled back exactly and every owned identity is registered as an
  *unavailable placeholder*: a hidden ability with no parent and a single
  `AlternateSummoningUnavailable` component that refuses every cast. No
  partially configured executable child is ever left available. A family
  that re-adapts in a later process keeps the same GUIDs. A GUID collision
  with a foreign blueprint still rolls the whole pass back. This is not a
  promise that uninstalling CotW is safe: with CotW absent, its families are
  `AbsentInProfile` and register nothing.

## Identities and roster maintenance (maintainers' note)

- **Namespace.** Adapter identities live in the disjoint
  `KMG.Summoning.Adapter.<Family>.*` namespace:
  - creature: `...Tier<T>.<CreatureToken>.<Quantity>`
  - retained native wrapper: `...NativeOption.Tier<T>.<Key>.<Quantity>`
  - frozen direct-root copy: `...Preserved.Tier<T>.<Quantity>`
- **Allocation.** `tools/expanded_summoning_manifest.py` allocates them from
  the catalog and the `I(...)` identity rows of
  `AlternateSummoningSourceCatalog.cs`:
  - GUIDs are RFC 4122 version-5 hashes of the symbol under a fixed namespace,
    so allocation is reproducible on any checkout.
  - They are still written once into `blueprints/blueprints.json`; the runtime
    never derives an ID.
  - Existing symbols and GUIDs never move.

**Adding a creature** in a future sprint needs no adapter edits:

1. Add it to `ExpandedSummoningCatalog`. Withhold it in
   `SummonVisibilityCatalog` as usual.
2. Run `python tools/expanded_summoning_manifest.py --allocate` (and
   `--activate` / `--emit-roster` as the sprint already does). The tool
   appends the canonical identities and every eligible caller's adapter
   identities. An SNA-only creature never gains an SM route.
3. Add the new block size to the ledger-offset tests, as every sprint does.
   The adapter block's own size is computed, so it updates itself.
4. Publication is still the removal of the suppression key. Withheld
   placements are registered for every caller but not exposed or castable
   through any caller; publishing exposes them everywhere at once.

`tools/test_expanded_summoning_alternate_sources.py` proves the following
with an in-memory test-only creature:
- propagation reaches only eligible callers
- allocation is append-only and reproducible
- a hand-edited or stray adapter identity fails validation

The domain test `alternate-summoning.identities-deterministic` recomputes
every symbol and its UUIDv5 GUID in C# and checks the shipped ledger both
ways.

## Route audit

| | |
|---|---|
| Static source | installed CotW 1.14.4c-2.1 (`CallOfTheWild.dll` SHA-256 `4ebf8e1e...6915`), decompiled locally (not committed), plus Kingmaker 2.1.7b `Assembly-CSharp` (SHA-256 `3b6450ff...afb`) |
| Runtime observation | guarded runs on development candidates, 2026-10-16: save-free compatibility-profile scenarios, plus read-only use of the disposable working save `KMG_AUTOMATION_WORKING` for the natural scenario (see "Native coverage" and the third pass); no save file was written |
| Graph discovery | `observe-alternate-summoning-sources` census, CotW profile (results under "Native coverage") |

Abbreviations: **IMPL** means implemented with focused source and domain
checks. **NATIVE** means a real granted cast was also observed in game (see
"Native coverage"); "assisted" marks the synchronous command fixture and
"natural" the working-save run with real popup clicks and native RTWP time.
Other tiers and positions were not each cast. **SHARED**
means the route reaches the canonical parents at use time, so it shows the
canonical menu; that is static evidence unless the row says "observed" (the
observer's canonical-reference census).

| Route | Provider / identity | Tier and quantity | Ability type, action, resource, duration, pool | Disposition |
|---|---|---|---|---|
| Master Summoner SM | CotW `MasterSummonerSummonMonster{1-9}Feature` -> `MasterSummonerSummonISingle` / `MasterSummonerSummon{2-9}Base` | SM I-IX at class levels 1,3,...,17; single, 1d3 from II, 1d4+1 from III | Monster Tactician clone. 5 + Cha shared daily uses. Standard action. Minutes per level. Free pool, or the summoner pool while the eidolon lives, with a conditional clear | IMPL; NATIVE (assisted: tiers I and III, no-eidolon and real-eidolon pool branches; natural RTWP through the real popup: tiers I, III and VII) |
| Summoner SM (base) | CotW `SummonerSummonMonster{1-9}Feature` -> `SummonerSummonISingle` / `SummonerSummon{2-9}Base` | SM I-IX | 3 + Cha. Eidolon must be dead or unsummoned. One effect (pool clear) | IMPL; NATIVE (tier I) |
| Twinned Summoner SM | CotW; keeps the base Summoner features | SM I-IX | as base Summoner | IMPL (shared base Summoner roots, no separate adapter); NATIVE (tier I) |
| Monster Tactician SM | native features `c19f6f34...` ... `f63d23b4...` | SM I-IX; one ability per quantity | as native: 1 use, minutes per level (observed) | IMPL; the observer verified every child against its template; NATIVE (lowest granted tier at Inquisitor 6 is III) |
| Occultist Arcanist Conjurer's Focus | CotW `OccultistSummonMonster{1-9}Feature` -> `OccultistSummonISingle` / `OccultistSummon{2-9}Base` | SM I-IX at levels 1,3,...,17 | Arcane reservoir cost = tier, including lower-tier groups. Perfect Summoner: free use and one day. One effect | IMPL; NATIVE (assisted: tier III cost and duration, lower-tier 1d3 group at the tier III cost, replacement, level-20 capstone) |
| Naturalist SNA | CotW `SummonerSummonNaturesAlly{1-9}Feature` -> `FeyCallerSummonNaturesAllyI` / `SummonerSummonNaturesAlly{2-9}Base` | SNA I-IX | Copies of the native SNA children, Spell type. Summoner resource and pool. Minutes per level | IMPL; NATIVE (tier I) |
| Fey Caller archetype | CotW `FeyCallerArchetype` | — | built but not offered in `SummonerClass.Archetypes` in 1.14.4c | ABSENT_IN_PROFILE; it would share the Naturalist roots |
| Feral Hunter SNA | CotW `FeralHunterSummonNaturesAlly{1-9}Feature` -> `FeralHunterSummonNaturesAllyI` / `...{2-9}Base` | SNA I-IX | 3 + Wis. Hunter level. Minutes. Own pool with clear. Usable with no pet or a dead pet. Spell type | IMPL; NATIVE (tier I) |
| Preservationist Bottled Ally | CotW `PreservationistBottledAlly{1-6}Feature` (AddKnownSpell, Alchemist) -> `PreservationistSummonNaturesAlly{I,II,IV,V,VII,IX}` | extract levels 1-6 -> SNA I, II, IV, V, VII, IX | Alchemist extract slot. Spell type. Standard action | IMPL; NATIVE (assisted: Bottled Ally I from a prepared slot; Alchemist 8 extract III -> SNA IV) |
| Spirit Summoner, Soulbound Summoner, Devil Binder SM | CotW | archetype removes the summon features | — | INTENTIONALLY_RESTRICTED (not restored) |
| Devil Binder Infernal Arcana | CotW, SM VI single (named) | one named creature | — | INTENTIONALLY_RESTRICTED |
| Hunter bonus SNA I-VI | CotW `AddKnownSpell` of the canonical parents | SNA I-VI | spellbook | SHARED |
| Druid spontaneous SNA | native conversion arrays | SNA I-IX | spellbook | SHARED (observed: `DruidSpontaneousSummon` converts to the canonical parents) |
| Shaman Friend to Animals hex | CotW reuses the native druid conversion arrays | SNA I-IX | spellbook | SHARED (inferred) |
| Summoner, Witch, Shaman, Psychic and other class spell lists; Witch Animal patron; Psychic grants | CotW `AddToSpellList` / `AddKnownSpell` on the canonical parents | — | spellbook | SHARED |
| Herald Caller spontaneous SM | native, if present; CotW has no Herald Caller code | — | — | SHARED (observed: `HeraldCallerSpontaneousSummon` `f6c112a5...` converts to the canonical parent) |
| Occultist / Relic Hunter Servitor | CotW, fixed native single variant per tier (SM I-VII), rank switch | one creature per tier | SpellLike, mental focus | DEFERRED_SPECIALIZED_ADAPTER (no menu; a design question, below) |
| Occultist / Relic Hunter Shadow Beasts | CotW, load-time snapshot of the canonical SM children and elemental roots, ShadowSpell 60 | SM I-IX | shadow | DEFERRED_SPECIALIZED_ADAPTER (needs a shadow-reality adapter) |
| Warpriest / Divine Tracker Battle Companion (6 blessings) | CotW, rank switch over fixed native single variants | one creature per rank | Supernatural, blessing resource | DEFERRED_SPECIALIZED_ADAPTER (no menu; a design question, below) |
| Shadow Conjuration / Greater / Shades | CotW, load-time snapshot clones (MergeIds GUIDs) | SM I-III / I-VI / I-VIII | shadow 20/60/80 | DEFERRED_SPECIALIZED_ADAPTER |
| Skald Spell Kenning | CotW, load-time duplicates of the list variants | SM I-VI | kenning resource | DEFERRED_SPECIALIZED_ADAPTER |
| Shaman / Spirit Whisperer Lore Wish | CotW, load-time duplicates from every spellbook | SM I-VIII, SNA I-VII | manifestation resource | DEFERRED_SPECIALIZED_ADAPTER |
| Twinned Eidolon spell-like | CotW, load-time snapshot of the Summoner-list SM children | SM I-V | eidolon | DEFERRED_SPECIALIZED_ADAPTER |
| Pact Wizard Animal patron conversion | CotW, by-reference arrays of native variant objects | SNA IV, VIII, IX | conversion | DEFERRED_SPECIALIZED_ADAPTER (a conversion adapter, not a menu) |
| Rogue Major Magic | CotW, copy of the then-direct SM I | SM I | SpellLike | DEFERRED_SPECIALIZED_ADAPTER |
| Scrolls, wands, crafting | native items; CotW `AddScroll` covers only CotW spells | — | — | SHARED for copying (observed: 46 summon scrolls `CopyScroll` the canonical parents); a scroll used directly casts its fixed native option, unchanged |
| Call Celestial Ally, Spark of Life, element-restricted summons | — | named or element only | — | INTENTIONALLY_RESTRICTED (no general menu added) |

**Coverage limits.**
- The CotW findings come from reading the decompiled installed DLL; the
  Kingmaker findings come from decompiled engine types. Native blueprint data
  (asset bundles) was not read statically.
- The observer has since resolved Monster Tactician's spawn sets, the Druid
  and Herald Caller conversions and the scroll references (see "Native
  coverage").
- A generic CotW compatibility PASS is not evidence that any archetype above
  was exercised.

## Verification performed (first checkpoint)

All runs were on this worktree, on 2026-10-16.

| Check | Command | Result |
|---|---|---|
| Registry cross-check | Scratch script comparing the catalog with the installed CotW `blueprints.txt` | 231 / 231 name-GUID pairs exact |
| Manifest | `python tools/expanded_summoning_manifest.py` | PASS: foundation 2149, adapters 3746, total 6895 entries, ledger 2,436,342 bytes (bound 4 MiB, headroom test passes) |
| Tool tests | `python tools/test_expanded_summoning_alternate_sources.py` | 6 / 6 |
| Focused gate (first) | `scripts/Invoke-KmgGate.ps1 -Level Focused -Filter 'alternate-summoning;expanded-summoning.menu-layout;...'` | production compilation PASS; 45 / 45, about 9 s |
| Focused gate (final, at `9fa26051`) | `scripts/Invoke-KmgGate.ps1 -Level Focused -Filter 'alternate-summoning;expanded-summoning.;blueprints.ledger;favored-class.leaf-manifest-tail;better-vendors.catalog-manifest-append;elemental-races.identities;bodyguard-blueprints.identities'` | production compilation PASS; 475 / 475, 4.7 s |
| Ledger dependency | Every domain test whose source reads the ledger or a modified production, script or project file (837 cases) | 837 / 837 with the environment satisfied (see note) |
| Script preflight | `scripts/Test-RuntimeScenarioPreflight.ps1` | 588 WhatIf checks PASS; no deployment or launch |

Notes:
- Two of the 837 cases (`bodyguard-runtime.*`) first failed because a fresh
  worktree lacks a gitignored local IL dump
  (`artifacts/inspection/bodyguard-native`). They passed once with that
  artifact copied, so they are classified as an environment precondition.
- One unintended complete-suite run happened while building the test binary;
  see "The earlier failed complete-suite execution" below.
- Fifteen existing ledger-shape domain tests were updated as every appending
  sprint has done. Totals gain the computed adapter block, and
  canonical-allocation scans exclude the adapter namespace, which has its own
  both-ways test. No Python release validator was changed.

## Verification performed (corrections, 2026-10-16)

All on this worktree. Durations are wall-clock.

| Check | Command | Result |
|---|---|---|
| Focused gate, template contracts | `scripts/Invoke-KmgGate.ps1 -Level Focused -Filter 'alternate-summoning;expanded-summoning.;blueprints.ledger;favored-class.leaf-manifest-tail;better-vendors.catalog-manifest-append;elemental-races.identities;bodyguard-blueprints.identities'` at `c77f971529` | compile PASS; 480 / 480; about 13 s |
| Focused gate, with icon-catalog readers | same filter plus `icon;elemental-character-trait.` | compile PASS; 576 / 576 of 2,632; 6 s |
| Focused gate, player paths | `-Filter 'alternate-summoning'` at `651dde310e` | compile PASS; 18 / 18; 8 s |
| Development validator fixtures | `python tools/test_development_candidate_validator.py` | 13 / 13 |
| Development validator, live | `python tools/validate_development_candidate.py --purpose alternate-summoning-native` | PASS: 6,895 identities, 3,746 appended to the released 3,149 |
| Release validator, unchanged | `python tools/validate_repository.py --root .` | still FAIL: "The firearm patch must not change blueprint identities" (expected; this branch is not a release) |
| Development-candidate contract | `scripts/Test-DevelopmentCandidate.ps1` (Windows PowerShell 5.1 and pwsh 7) | 63 / 63 each; about 2 s |
| Repository wrapper, development mode | `scripts/validate-repository.ps1 -DevelopmentPurpose alternate-summoning-native` | PASS; 28 s |
| Repository wrapper, release mode | `scripts/validate-repository.ps1` | FAIL at the v0.0.156 release validator, as before; 1 s |
| Gate and orchestration contracts | `Test-ExplicitCandidateReuse.ps1`, `Test-KmgGate.ps1` (includes `Test-DevelopmentGate.ps1`) | 7 / 7; PASS (development gate 8 / 8) |
| Runtime preflight | `scripts/Test-RuntimeScenarioPreflight.ps1` | 588 checks PASS; 14 s |
| Pre-existing failures | `Test-RuntimeDeployment.ps1`, `Test-CompatibilityCandidateReuse.ps1` | both FAIL identically on unchanged `master` `607da683b1` ("legacy-deployment-pins-complete-artifact-identity"; "Nested build/deployment/scenario count mismatch"). Baseline, not caused here; neither runs in the wrapper |

### The earlier failed complete-suite execution (unclassified)

- **What happened.** On 2026-10-16, between about 06:41:15Z and 06:45Z, the
  domain test executable was run once without a filter, on a binary built
  from the uncommitted working tree before the fifteen ledger-shape tests
  were updated (base `968b35495c`). It failed.
- **Output.** Not retained. `test-domain.ps1` writes no result file, that
  invocation's standard output was discarded, and the runner writes no file
  of its own. `artifacts/gate-timings.jsonl` holds only Focused-level entries
  for that morning; a direct executable run is not recorded there. The
  session transcript holds no completion line for a run of all 2,627 cases
  registered then. No other copy was found in the worktree, the scratch area
  or the temporary directory.
- **What is known.** A filtered run of 513 cases on the same binary showed
  fifteen ledger-shape failures (`favored-class.leaf-manifest-tail`,
  `better-vendors.catalog-manifest-append`, `elemental-races.identities`,
  `expanded-summoning.sprint3/4/5/8-ledger-icons`,
  `expanded-summoning.sprint16-ledger-identities`,
  `expanded-summoning.sprint17-snake-append-only-ledger`,
  `expanded-summoning.sprint20-templated-monster-only`,
  `expanded-summoning.sprint20-ledger-append`,
  `expanded-summoning.sprint23-ledger-append`,
  `expanded-summoning.sprint14-identities`,
  `expanded-summoning.sprint14-catalog-matches-ledger`,
  `bodyguard-blueprints.identities`), all fixed in `9fa2605184`, plus two
  `bodyguard-runtime.*` environment-precondition failures (a gitignored IL
  dump missing from a fresh worktree; they pass with it present).
- **What is not known.** Its total count, pass/fail counts and any other
  failing case. It is recorded as an unclassified failed execution, not as
  evidence either way, and it was not repeated: the complete suite remains
  a Sprint-boundary gate under the validation policy.

## Native coverage (what was actually exercised in game)

Structured engine observations, not screenshots. The scenarios in this
section ran through compatibility-profile transactions: save-free, in a fresh
empty world, every created unit destroyed and the native census restored,
every profile restored to its recorded per-file SHA-256 manifest (see the
state file for receipts). The third pass's natural scenario is different: it
loads the disposable working save read-only through the live development
route (below).

**Observer and blocked family** (`observe-alternate-summoning-sources`,
`observe-alternate-summoning-blocked-family`):
- CotW profile: all seven families `Published` at the first update after
  `LoadDictionary`; registered identities 584 / 561 / 561 / 561 / 563 / 563
  / 353 (all of each family's); every adapter child matched its authorized
  template under the supported signature contract, apart from the deliberate
  creature and composed action (at `4c5b721bdcdf`/`651dde310e63` that
  contract recorded the composed template/alignment action by type only; the
  composed-action contract was added in the third pass, below); host
  templates unchanged since resolution; the canonical publication is exact
  (1,093 references) and no adapter child sits under a canonical parent.
- Standalone profile (no CotW): Monster Tactician `Published` (584); the six
  CotW families `AbsentInProfile`, registering nothing.
- Blocked-family drill (Master Summoner, by request): Master Summoner
  `Blocked`, all 561 identities registered as unavailable placeholders, its
  host menus unchanged; the six other families published.

**Contract fixture** (`disposable-alternate-summoning-contracts`, the
production resolver and builder on transient clones of the real installed
Master Summoner tier III 1d3 template): each of an unsupported spawn lineage,
an additional spawn, an altered quantity (D3 to D6) and a changed caller
condition (the Master Summoner free-pool checker pointed at the Summoner
pool) blocks the family at that template with a precise reason; an unknown
extra option is preserved; a built child shares no action object and no
provider cache with its template; a repeated reconciliation does no work
(3,716 exposed, 75 roots, library 111,247 unchanged).

**Player paths** (`disposable-alternate-summoning-player-paths`): disposable
in-area casters gain class and archetype through native `LevelUpController`
visits, so their summon abilities are the real granted host roots; each cast
builds the child's `AbilityData` from the root's own (the action bar's
converted-ability path) and runs the native `UnitUseAbility` command as an
assisted native command fixture (cooldown ignored, `IsActed` set, ticked
synchronously; see the third pass). Final run (`651dde310e63`): 20 / 20.

| Case | Observed |
|---|---|
| Master Summoner (Summoner 5 + archetype, Cha 18) | tier I-III features and published roots granted, IV-IX not; tier-I root menu of 7, II 19, III 31 |
| tier-I project creature | Dire Rat, 1 use (9 -> 8), 1 summoned, 300 s |
| preserved tier-I cast | registered but withheld from the menu (its lineage is replaced); through the root it is unavailable, no command, nothing spent |
| tier III 1d3 option | 2 Giant Centipedes for 1 use (8 -> 7), 300 s each |
| no uses left | unavailable, no command, nothing spent or summoned |
| cancelled command | started, interrupted before its act: nothing spent or summoned |
| no eidolon | two casts, both summons alive together |
| Occultist (Arcanist 5 + archetype) | tier I-III Conjurer's Focus features and published roots |
| tier III single | 3 reservoir points (8 -> 5), 300 s (one minute per level) |
| second cast | the first summon ends; only the new one remains (5 -> 2) |
| level 20, Perfect Summoner | free (13 -> 13), 86,400 s (1,440 minutes) |
| Monster Tactician (Inquisitor 6, archetype found by its exact feature) | lowest granted tier III; project creature, 1 use (7 -> 6), 360 s |
| base Summoner 3 | tier I; 1 use (7 -> 6), 180 s |
| Twinned Summoner 3 | shares the base roots; 1 use, 180 s |
| Naturalist 3 | tier I SNA; 1 use, 180 s |
| Feral Hunter 3 | tier I SNA; 1 use, 180 s |
| Preservationist (Alchemist 2) | Bottled Ally I known in the Alchemist formula book; a prepared slot opens a project creature through the root's variant data, consuming exactly that slot (1 -> 0) |

**Module OFF** (`observe-alternate-summoning-sources` with Expanded
Summoning OFF through the profile's own feature-module parameters, every
other module ON, `651dde310e63`): all seven families `RegisteredModuleOff`
with every identity registered (584 / 561 / 561 / 561 / 563 / 563 / 353),
no adapter child exposed and no adapter root published; the canonical
parents untouched. The companion `observe-feature-module-settings` (47 / 47)
confirms the staged settings were the ones loaded and that canonical
Expanded Summoning publication was gated off. This is what a saved
reference to an adapter child resolves to with the module OFF: a registered,
unexposed identity whose gate refuses the cast.

**Census results** (`observe-alternate-summoning-sources`, CotW profile,
`4c5b721bdcdf`):
- 237 distinct spawn-graph roots: 59 published adapter roots, 18 canonical
  parents, 34 KMG-owned abilities and 126 others; 6,268 spawning abilities.
- 209 components reference a canonical parent. They include the native
  `HeraldCallerSpontaneousSummon` (`f6c112a579a81c0439c597f9ee222243`),
  `DruidSpontaneousSummon` (`b296531ffe013c8499ad712f8ae97f6b`) and
  `EnemyOfTheFey` through `SpontaneousSpellConversion`, sorcerer bloodline
  `AddKnownSpell` grants and 46 summon scrolls through `CopyScroll`.
- The 126 other roots are the routes this audit already disposes of
  (Servitor, Shadow Beasts, Battle Companion blessings, Shadow Conjuration /
  Greater / Shades, Skald Spell Kenning, Lore Wish, Twinned spell-likes,
  Rogue Major Magic, Infernal Arcana, Call Celestial Ally, Spark of Life,
  elemental summons), fixed single-creature abilities, and cutscene,
  encounter, item and scroll summons. No other general class caller was
  found. Three native spell-likes, `SummonNaturesAllyAasimar{Single,d3,
  d4plus1}` (`b1326a7a...`, `37f4f8bd...`, `21748672...`), have no caller
  identified by this audit; they are not adapted.

## Targeted verification pass (2026-10-16, third pass)

Corrections implemented; native command-fixture coverage passed; real-UI and
natural-RTWP coverage passed on the working save for the cases listed, under
the strengthened natural verdict; player-facing validation partially complete
(turn-based, hotbar, saved shortcuts and a scroll-required menu remain manual;
a blocked-family load and grapple reset on reload need maintainer fixtures).

### What the observer verifies (the supported contracts)

The observer does not compare an unrestricted "full executed signature". An
adapter child must satisfy two named contracts.

**Supported signature contract** (`SummonSignatureReader`, checked against
`AlternateSummoningContracts.ExpectedAdapterChild` of the authorized
template): ability type, action type, full-round flag, range, school,
descriptor, spell-list and run-action counts, the host's own variants;
each resource (identity, spend, amount, custom cost, cost-increasing facts);
cost calculators; caster checkers with their fields (pools, flags); every
rank configuration; and the run action in order: each spawn's unit, pool,
count dice/count/bonus and duration rate/dice/count/bonus, each clear's pool,
each conditional's operation, condition descriptions and branches, every
other action by type, and a count of spawns hidden behind unmodelled actions.
After-spawn actions are recorded by type only.

**Composed-action contract** (`AlternateSummoningComposition`, from
`3e2bf5caf729`): the values the signature records only by type, checked
against the canonical child of the same placement.
- Placement: every spawn branch ends with exactly one composed action of the
  canonical kind, and none carries the other kind; a retained native option
  or preserved cast carries none.
- Template action: celestial and fiendish template buffs, both smite buffs and
  the neutral-mode buff equal the canonical ones.
- Replaced native template buffs: the canonical set. The runtime removes each
  listed buff that is present, so order and repeats do nothing and are not
  compared; a missing, extra or null entry is a difference.
- Alignment action: the canonical `SummonAlignmentMode`.

Anything outside both contracts is not compared, apart from the "template
unchanged since resolution" rendering check on each authorized host template.
Earlier observer PASS records (`4c5b721bdcdf`, `651dde310e63`) predate the
composed-action contract; they established the signature contract only.

Negative controls, against the actual verifier on disposable clones of real
built children (contract fixture, `c7216602b103`, 17/17):

| Mutation (clone only) | Verifier result | Summon signature |
|---|---|---|
| alignment mode changed, same action type (Naturalist tier I) | `branch0:alignment-mode:expected=Caster;actual=Celestial` | identical |
| one replaced native template buff removed (Master Summoner tier I, templated) | `branch0:replaced-native-template-buffs:missing=[83a8a909...];extra=[]` | identical |
| celestial template reference changed | `branch0:template-buffs:celestial-template:expected=e03163ef...;actual=6bf44252...` | identical |
| template action removed from the last spawn branch | `placement:branch1:expected-one-final-template;actual=[...ContextActionApplyBuff]` | differs |
| none (built children and an unmodified clone) | accepted; 2 templated branches | — |

The built child itself is unchanged before and after every mutation.

### Assisted native command fixture (`disposable-alternate-summoning-player-paths`)

Every cast in this fixture, earlier and now, is an **assisted native command
fixture**: the native `UnitUseAbility` built from the granted ability's own
data, under a request-local cutscene command context, with the cooldown
ignored, `Animation.IsActed` set, and the command and its execution process
ticked synchronously. It is not natural animation, action-bar interaction,
scheduler or turn timing, or a saved hotbar. The scenario output now says so
in a top-level `execution` field, in an `execution` field on every cast
record, and in the evidence text of every assertion that rests on a cast.
The earlier 20/20 record (`651dde310e63`) is the same kind of evidence and
keeps its result.

New cases at `c7216602b103` (28/28), expectations taken from the installed
CotW construction (constants from its `blueprints.txt`), not from this
module's code:

| Case | Observed |
|---|---|
| Master Summoner 5 takes a real lesser eidolon through the native level-1 `LesserEidolonFeatureSelection` | `LesserFey Eidolon`; `FeyEidolonUnit` live in the area; Summon and Unsummon Eidolon granted |
| eidolon present: first effect | Dire Rat for 1 use (9 -> 8), alone in the free pool `ae450ab8...` |
| eidolon present: second attempt | refused by `AbilityCasterCompanionDeadOrSummonPoolEmpty`: unavailable, no command, nothing spent or summoned |
| group is one effect | after the first effect ends the free pool is empty; a 1d3 group gives 3 Giant Centipedes for 1 use, all in the free pool; the next attempt is refused with nothing spent |
| eidolon absent | the real Unsummon Eidolon marks the eidolon for death; the engine's own life tick (`UnitLifeController.TickOnUnit`, request-local) makes it dead; two more effects, 1 use each, land in the summoner pool `e71b38e5...` beside the free-pool group |
| eidolon returns | the real Summon Eidolon removes `EidolonUnsummonedBuff`; after the life tick the eidolon is alive; the two summoner-pool effects are dismissed and leave the pool when destroyed; the free-pool group stays; the next attempt is refused with nothing spent |
| cleanup | every remaining effect's summoned-unit buff names this summoner; each leaves both pools when ended; one effect is then permitted again in the free pool (1 use) |
| Occultist lower-tier group | tier III Conjurer's Focus, Giant Centipede (roster source tier 2) 1d3: 3 reservoir points (8 -> 5), the selected tier's cost from CotW's `OccultistSummon3Base` (the implementation's own calculated cost is recorded separately); 2 units, 300 s each |
| Preservationist, Alchemist 8 | Summon Nature's Ally IV known as a 3rd-level extract (CotW Bottled Ally III); one prepared 3rd-level slot (1 -> 0; no 4th-level slot, 0 -> 0) opens a tier IV roster creature (Dire Boar) through the root; 48 s (8 rounds at caster level 8) |

Ending a summon in this fixture removes its summoned-unit buff (what expiry
and `ContextActionClearSummonPool` do); the engine then keeps the unit, and
its pool membership, through a timed dismemberment, which this synchronous
fixture completes by destroying the unit. The pool releases it on the
engine's own destroy event.

The first run of the eidolon cases (`3e2bf5caf729`, 27/28) failed
`player-ms-eidolon-return`: `Resurrect` lowers damage and clears the death
flags, and the life controller's next tick makes the eidolon conscious; the
fixture had not applied that tick after Summon Eidolon (it did after
Unsummon), so the eidolon still read dead and the next attempt was allowed.
Fixture defect, corrected in `c7216602b1`.

### Natural working-save run (`disposable-alternate-summoning-natural`)

On the guarded working save `KMG_AUTOMATION_WORKING`, loaded read-only (no
save file is written) with the full installed mod set, through the live
development route: `Deploy-Local.ps1 -DevelopmentPurpose` (backup first),
`Invoke-KingmakerRuntimeTest.ps1 -ReuseInstalledArtifact -SaveName
KMG_AUTOMATION_WORKING`, then `Restore-Live-Mod.ps1`. A disposable Master
Summoner 13 joins the UI party and is selected. Each choice opens the native
abilities group, the granted root's own variant popup, and clicks the row
(`ActionBarSlot.OnClick`); the cast is confirmed through the native
selected-ability click handler (`ClickWithSelectedAbilityHandler.OnClick` on a
point beside the caster) and left to the unpaused native RTWP scheduler and
animation. No forced act, no cooldown override, no direct command tick, no
hotbar slot.

**Verdict** (`AlternateSummoningNaturalVerdict`, from `a320442acd`). A cast
passes only when:
- the wait ended because the command finished and the standard-action
  cooldown cleared, not at the 30 s deadline (`waitOutcome` records
  `completed`, `command-stalled`, `cooldown-stalled` or `deadline`);
- it acted natively with no override, forced act or direct tick;
- exactly one use was spent;
- every creature is the chosen unit and is owned by this summoner;
- the count lies inside the option's tabletop bounds: one, 1d3 = 1-3,
  1d4+1 = 2-5, widened only by an active native Maximize or Empower (read
  from the cast; none was active).

Domain tests cover an ordinary completion and the failures: an unfinished
command with a creature already spawned, an uncleared cooldown, a deadline,
out-of-range groups, assistance, wrong spend or unit, and the count modifiers.

Run at `a320442acdb2`, strengthened verdict, 11/11 (evidence
`20261016T1740160951203Z-disposable-alternate-summoning-natural`):

| Case | Observed |
|---|---|
| tier I popup, early row | 7 rows (all exposed), navigation verified; row 0 Dire Rat clicked; selection is that child, converted from the granted root fact |
| tier I cast | wait `completed` (command finished, cooldown cleared), native act, no override, 2.4 s; 1 use (9 -> 8); 1 Dire Rat, legal [1,1] |
| tier III popup, last row | 31 rows; last row (Viper 1d4+1) in view and clicked |
| tier III cast | `completed`, 2.7 s; 1 use (8 -> 7); 3 Vipers, legal [2,5] |
| tier VII popup, recent creature | 94 rows; the single Tyrannosaurus (Sprint 24) row in view and clicked |
| tier VII cast | `completed`, 2.2 s; 1 use (7 -> 6); 1 Tyrannosaurus, legal [1,1] |
| Tyrannosaurus cleanup | explicit lifecycle-buff removal followed by native dismemberment and pool cleanup: the fixture removed its summoned-unit buff, native time (2.1 s) ran the dismemberment and destroyed it, and its pool released it. Not the summon's full duration elapsing, not a dismissal click, not grapple/swallow reset on reload |
| cleanup | the summoner and every summon gone; original units, area effects and party unchanged; no save write |

The cleanup target must be the Tyrannosaurus cast in the same run; a missing
target is a BLOCKED failure, never a substitute unit.

Earlier runs of this scenario, kept with their original verdict, which did
not yet require the command-finished, cooldown-cleared or wait-condition
state and accepted any positive group count:
- `b9c56e3da9a9`, 11/11 (`20261016T1550339254110Z-disposable-alternate-summoning-natural`);
- `c7216602b103`, 8/8 (Master Summoner 5; tiers I and III; a Dire Rat cleaned up;
  `20261016T1544477038010Z-disposable-alternate-summoning-natural`).
Their raw evidence lacks the terminal-state fields, so they could not be
re-read under the new verdict; the scenario was rerun instead.

At the runs' screen resolution no menu needed scrolling (7, 31 and 94 rows
all fit), so the scroll-required choice is not exercised.

### Commands, durations and results (third pass)

All on this worktree; durations are wall-clock.

| Check | Result |
|---|---|
| `Invoke-KmgGate.ps1 -Level Focused -Filter 'alternate-summoning'` (before `3e2bf5caf7`) | compile PASS; 24 / 24 of 2,639; 9 s |
| `Invoke-KmgGate.ps1 -Level Focused -Filter 'alternate-summoning;runtime-scenario;brown-fur.native-cast'` (each later commit) | compile PASS; 25 / 25; about 10 s |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | 590 PASS (two new checks for the natural scenario); 14 s |
| `Test-ExplicitCandidateReuse.ps1`, `Test-KmgGate.ps1` | 7 / 7; PASS (development gate 8 / 8) |
| `Invoke-KmgGate.ps1 -Level Focused -Filter 'alternate-summoning;runtime-scenario'` at `a320442acd` | compile PASS; 32 / 32 (7 new verdict cases); about 3 s |
| Candidate builds (`Build-Local.ps1 -ReferenceBundleDir <refresh-2026-10-15> -DevelopmentPurpose alternate-summoning-native -DevelopmentTestFilter <focused + icon readers>`) | `3e2bf5caf729` 68 s, `c7216602b103` 63 s, `b9c56e3da9a9` 63 s (583 / 2,639 selected); `a320442acdb2` 68 s (590 / 2,646); 0 failures each |
| Guarded profile transactions | see the state file; 2-7 min each, all `Restored` and verified |
| Guarded live working-save runs (`Deploy-Local -DevelopmentPurpose`, `Invoke-KingmakerRuntimeTest -ReuseInstalledArtifact -SaveName KMG_AUTOMATION_WORKING`, `Restore-Live-Mod`) | 3, about 1.5 min each; restoration evidence in the state file |
| Pre-existing failures | `Test-RuntimeDeployment.ps1`, `Test-CompatibilityCandidateReuse.ps1` (recorded before) and `Test-MagicCircleRuntimeRequest.ps1` ("'Get-KmgRepositoryRoot' is not recognized") fail identically on unchanged `master` `607da683b1`; not caused here |

Repository wrapper. Each development `Build-Local` above ran
`validate-repository.ps1 -DevelopmentPurpose alternate-summoning-native` as a
nested step (its log: "Development-candidate repository validation passed
... This is not a release qualification."). That is the development wrapper:
it runs the development-candidate validator in place of the active release
validator and the named test selection in place of the complete suite; every
other wrapper fixture runs unchanged. No independent release-mode wrapper run
qualified this work (the one recorded earlier fails, as intended, at the
v0.0.156 release validator).

Not run: the complete domain suite, a release-mode wrapper, the historical
full runtime matrix.

### Evidence matrix

Each row stands alone; no row's PASS covers another row's category.

| Category | Status | Candidate | Covers | Evidence (under `C:\Dev\KingmakerGunslingerLab\runtime-evidence`) |
|---|---|---|---|---|
| Source and domain checks | PASS | per commit | composition model 6 cases; 25 related cases; 583 selected in each candidate build; runtime preflight 590 | gate output; candidate receipts |
| Structural native observer, CotW | PASS 9/9 | `c7216602b103` | seven families published; both contracts on every adapter child; templates unchanged | `20261016T1537090363960Z-observe-alternate-summoning-sources` |
| Blocked-family drill | PASS 9/9 | `c7216602b103` | Master Summoner blocked with placeholders; others published under both contracts | `20261016T1538502973891Z-observe-alternate-summoning-blocked-family` |
| Standalone observer (no CotW) | PASS 9/9 | `b9c56e3da9a9` | Monster Tactician published under both contracts; the six CotW families `AbsentInProfile` | `20261016T1553238390102Z-observe-alternate-summoning-sources` (transaction `compat-20261016T155231Z-ebe9626d778a`) |
| Module OFF observer + settings | PASS 9/9; 47/47 | `b9c56e3da9a9` | every identity registered, nothing exposed or published; the staged OFF setting was the one loaded | `20261016T1555338499418Z-observe-alternate-summoning-sources`, `20261016T1557081100540Z-observe-feature-module-settings` (transaction `compat-20261016T155434Z-ea5fc4a94142`) |
| Composed-action negative controls | PASS 17/17 | `c7216602b103` | table above | `20261016T1540259762240Z-disposable-alternate-summoning-contracts` |
| Assisted native command fixture | PASS 28/28 | `c7216602b103` | every family's granted root; eidolon pools; Occultist lower-tier group; Preservationist III -> SNA IV. Timing assisted (see label) | `20261016T1541548916348Z-disposable-alternate-summoning-player-paths` |
| Eidolon and pool behavior | PASS (assisted) | `c7216602b103` | real Master Summoner + real lesser eidolon (Fey); pools, units and uses read from the engine | same |
| Real UI popup selection | PASS (natural run 11/11) | `a320442acdb2` | real action-bar popup rows for tiers I, III (last of 31) and VII (Tyrannosaurus of 94) clicked; the selected child is converted from the granted root fact. Earlier: `b9c56e3da9a9` 11/11, `c7216602b103` 8/8 | `20261016T1740160951203Z-disposable-alternate-summoning-natural` |
| Scroll-required menu choice (automated) | NOT EXERCISED | — | at the runs' resolution no menu (7, 31, 94 rows) needed scrolling; the last row was in view | — |
| Natural RTWP timing | PASS (strengthened verdict) | `a320442acdb2` | target click through the native handler; wait ended on command finished + cooldown cleared, never the deadline; natural act; no override, forced act or direct tick; one use each; counts inside tabletop bounds. The earlier `b9c56e3da9a9`/`c7216602b103` passes used the weaker verdict | same |
| Turn-based timing | NOT RUN | — | no genuine turn-based round | owner checklist step 5 |
| Hotbar binding and reuse | NOT RUN (manual) | — | binding goes through the `settings.SetSlot` path that hung a guarded run before; not automated | owner checklist steps 1-2 |
| Saved-reference cold load (normal / OFF / restored) | NOT RUN (manual) | — | needs a save-writing shortcut orchestrator; structural substitutes only: module-OFF observer, unexposed-child refusal | owner checklist steps 2-4 |
| Saved-reference load with a blocked family | BLOCKED / UNVERIFIED (maintainer only) | — | no safe, identified drill-capable load procedure exists; the save-free placeholder drill is not shortcut deserialization | maintainer case M1 |
| Recent-creature cleanup | PASS | `a320442acdb2` | the Tyrannosaurus (Sprint 24): explicit lifecycle-buff removal followed by native dismemberment and pool cleanup; not natural duration expiry, not a dismissal click | natural run |
| Scroll-required menu choice (manual) | NOT RUN | — | see above | owner checklist step 6 |
| Grapple / swallow reset on reload | BLOCKED (maintainer fixture needed) | — | the engine refuses to save in combat, and a grab or swallow exists only in combat; grapple persistence is not implemented | maintainer case M2 |

### Owner checklist (manual cases not automated)

None of these is automated or claimed. A checklist step that passes in your
hands is owner evidence for that step only.

**Artifact.** Use the published v0.0.157 package once it is released.
Before that, the development candidate is
`artifacts/development-candidates/alternate-summoning-native/a320442acdb2/KingmakerGunslinger-0.0.156-development-alternate-summoning-native-a320442acdb2.zip`
in the mission worktree, with its receipt next to it
(`...zip.development-receipt.json`, SHA-256 `8f09e234...5a07`). Its limits: it
is local and gitignored, never a release, still reports version 0.0.156, and
only `Deploy-Local.ps1 -PackagePath <zip> -DevelopmentPurpose
alternate-summoning-native` installs it (that backs up the live mod first;
`Restore-Live-Mod.ps1 -BackupDirectory <backup>` puts it back).

**Saves.** Work only in a disposable save: a new test character, or a copy
of a non-campaign save in its own slot. Before starting, copy the whole
`%USERPROFILE%\AppData\LocalLow\Owlcat Games\Pathfinder Kingmaker\Saved Games`
folder somewhere safe, and copy it back when finished. Never use a campaign
save, and never load, overwrite or rename `KMG_AUTOMATION_BASELINE`. Keep Call
of the Wild 1.14.4c-2.1 installed throughout; do not uninstall it from a save
that depends on it.

**Character.** A Summoner with the CotW Master Summoner archetype, level 5 or
higher (level 13 for tier VII), Charisma 14+, with a lesser eidolon. Rest
first so at least 4 Master Summoner uses remain. Before step 1 cast **Unsummon
Eidolon** and keep the eidolon unsummoned through step 4: with the eidolon
present and a summon already alive, the host correctly refuses another cast,
which would make the success expectations below misleading. Stay out of
combat, idle, in a safe area that will not change (no area transition, so no
autosave).

1. **Hotbar binding.** Open the abilities group on the action bar, open the
   Summon Monster I menu, and drag a KMG creature (for example Dire Rat) to an
   empty hotbar slot. Click the slot and place the summon.
   Expect: the tooltip names that creature under the Master Summoner's Summon
   Monster I; the Dire Rat appears; the use count drops by exactly one; the
   hotbar holds exactly one such slot.
2. **Save and cold reload (normal).** Save once to a new disposable slot `S1`.
   Exit the game to the desktop so the Kingmaker process ends, start it again
   from Steam, and load `S1`. Click the slot.
   Expect: the slot is still there once, with the same creature and icon; the
   cast works, summons a Dire Rat and spends one use. (Quitting to the main
   menu and loading is a separate warm-reload check, not this cold load.)
3. **Expanded Summoning OFF, cold.** Exit the game. Set `"expanded-summoning"`
   to `false` in `Mods\KingmakerGunslinger\FeatureModules.json`. Start the game
   and load `S1`. Do not save in this step. Click the slot.
   Expect: the slot is unavailable or the cast is refused; no use is spent; no
   creature appears; no error dialog; the slot is neither lost nor duplicated.
   Exit the game without saving; `S1`'s modified time is unchanged.
4. **Restored, cold.** Set `"expanded-summoning"` back to `true`, start the
   game, load the same `S1`, click the same slot.
   Expect: the same slot casts the same Dire Rat and spends one use, with no
   respec and no duplicated ability on the character sheet.
5. **Turn-based round.** Turn on turn-based mode in the game options, start a
   fight with a hostile you can handle, and on the Master Summoner's turn open
   the Summon Monster menu and choose a KMG creature.
   Expect, under the host rule (CotW Master Summoner summons are standard
   actions): the cast uses that turn's standard action; exactly one use is
   spent; the creature appears and is listed in the initiative order. When it
   first acts (its first eligible turn, or later) is a separate observation:
   record it rather than assuming it. With the eidolon summoned and a summon
   alive, a further attempt is refused with nothing spent.
6. **Scroll-required menu.** Note the current resolution and UI settings.
   Lower the resolution in the game's graphics options (a supported,
   reversible setting) until a large menu shows a scrollbar: tier VII (94
   entries, level 13) or tier III (31 entries). Scroll to the end, choose an
   entry that was out of view at the start, and cast it.
   Expect: the tooltip and the summoned creature match the entry chosen; the
   Master Summoner use count drops by one. Then restore the original
   resolution and UI settings.

Maintainer-only cases (BLOCKED / UNVERIFIED; not owner steps):
- **M1. Saved shortcut with a blocked family.** A family is blocked only when
  the installed provider's contract no longer matches. There is no safe,
  identified drill-capable load procedure yet, and the save-free placeholder
  drill (`observe-alternate-summoning-blocked-family`) is not proof of shortcut
  deserialization. Do not edit the provider DLL or alter a live install to
  provoke it.
- **M2. Grapple or swallow reset on reload.** The Tyrannosaurus (tier VII,
  Master Summoner 13) has grab and swallow. The engine refuses to save while
  the player is in combat (`SaveManager.IsSaveAllowed`), and a grab or swallow
  exists only in combat, so ordinary play cannot create the needed save. It
  needs a maintainer fixture. Expected when tested: a clean reset, meaning the
  grapple link and condition and the swallow (mouth) occupancy cleared, no
  stale interior damage and no dangling relationship. Unrelated combat
  conditions need not clear. Grapple persistence is not implemented.

## Release v0.0.157

Prepared for release as `0.0.157-alternate-summoning-sources` on the
owner's authorization (`docs/BUILD-AND-RELEASE.md`). The release candidate
`3dc23e9c7a2e` passed the Sprint gate (release-mode repository wrapper with
the v0.0.157 validator, complete suite 2,646 / 2,646, exact build, strict
373-file package) and, on exactly those bytes, the CotW, standalone and
module-OFF profile scenarios (including the canonical inventory, 50 / 50) and
the natural working-save scenario (11 / 11). Receipts are in the state file.
The owner checklist and maintainer cases above remain open.

## Remaining (not checked in game)

Runtime is not checked for any of these; they are not claimed. Items the
third pass closed (eidolon pools, real popup selection, natural RTWP casts, a
recent creature's cleanup by explicit lifecycle-buff removal followed by
native dismemberment and pool cleanup) are no longer listed; see the evidence
matrix.

1. **Hotbar binding and reuse.** Not automated: binding goes through the
   `settings.SetSlot` path that hung an earlier guarded run, and it was not
   repeated or forced. Owner checklist steps 1-2.
2. **Saved-reference cold load** (normal, Expanded Summoning OFF, restored).
   Needs a save-writing shortcut orchestrator on a disposable save.
   Structural substitutes observed: with the module OFF every identity
   registers and nothing is exposed; an unexposed registered child is refused
   through its root. Owner checklist steps 2-4.
3. **Saved-reference load with a blocked family.** BLOCKED / UNVERIFIED,
   maintainer only (case M1); the save-free placeholder drill is not shortcut
   deserialization.
4. **Turn-based round.** No genuine turn-based combat round. Natural RTWP is
   covered. Owner checklist step 5.
5. **Scroll-required choice.** No menu needed scrolling at the natural runs'
   resolution (7, 31 and 94 rows); the last and a deep row were clicked in
   view. Owner checklist step 6.
6. **Grapple or swallow reset on reload** through an adapter summon. BLOCKED:
   needs a maintainer fixture (case M2), because the engine refuses to save in
   combat. Grapple persistence is not implemented.
7. **Tiers.** Not every tier and position of every family was cast; the cases
   are the ones listed in each table.

## Findings for review

- **Existing canonical load-order dependency (not changed here).**
  - Canonical publication happens in this package's `LoadDictionary` postfix.
  - If it ever ran before CotW's postfix, the four Group A routes above would
    abort CotW's entire initialization.
  - It works today because CotW's postfix registers first; the last
    `loaded_blueprints.txt` contains no KMG names.
  - Both postfixes use default priority, so this is load-order luck for the
    canonical path. The adapters here do not depend on it.
- **Elemental Insight** follows `BlueprintAbility.Parent` to the eighteen
  canonical parents. Adapter children are parented to their caller roots, so
  they behave exactly as the hosts' own children: no bonus, not broadened.
  Whether Bottled Ally should count is a design question.
- **Same-turn activation** accepts only spellbook `Spell` casts with the
  Summoning descriptor:
  - Preservationist extracts, host and adapter alike, already match; the
    adapters do not change that.
  - Class spell-like callers have no spellbook and are not accepted, as
    before.
  - Not broadened.

- **The shared Expanded Summoning correction fixture marks its disposable
  caster `IsCheater`.** Kingmaker's `AbilityResourceLogic.Spend` skips
  cheaters, so casts from that caster never spend a class resource. The
  player-path fixture clears the flag on its own caster blueprints; any other
  resource-spend claim built on that fixture must do the same.
- **Two offline script tests fail on unchanged `master`** (`607da683b1`):
  `Test-RuntimeDeployment.ps1` ("legacy-deployment-pins-complete-artifact-identity")
  and `Test-CompatibilityCandidateReuse.ps1` ("Nested build/deployment/scenario
  count mismatch"). Neither runs in the repository wrapper; not changed here.
- **Machine-local build inputs.** On this machine the default private
  reference bundle lacks `UnityEngine.PhysicsModule.dll`, so candidates need
  the documented `private/refresh-2026-10-15` bundle, and the compatibility
  profiles' default reference root (`C:\Dev\KingmakerGunslingerLab\examples`)
  had to be recreated from the installed CotW.

## Open design questions (owner)

1. Servitor and Battle Companion are fixed one-creature-per-tier graphs in
   CotW. Should they ever offer a creature menu?
2. Should Elemental Insight apply to Bottled Ally or other caller summons?
3. Should Monster Tactician show three per-quantity menus per tier, as
   implemented, or one combined menu? A combined menu would be a new root
   rather than the host's own abilities.
