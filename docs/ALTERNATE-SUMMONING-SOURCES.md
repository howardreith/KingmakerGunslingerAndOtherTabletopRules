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
| Runtime observation | guarded save-free runs on development candidates, 2026-10-16 (see "Native coverage") |
| Graph discovery | `observe-alternate-summoning-sources` census, CotW profile (results under "Native coverage") |

Abbreviations: **IMPL** means implemented with focused source and domain
checks. **NATIVE** means one real granted cast was also observed in game (see
"Native coverage"); other tiers and positions were not each cast. **SHARED**
means the route reaches the canonical parents at use time, so it shows the
canonical menu; that is static evidence unless the row says "observed" (the
observer's canonical-reference census).

| Route | Provider / identity | Tier and quantity | Ability type, action, resource, duration, pool | Disposition |
|---|---|---|---|---|
| Master Summoner SM | CotW `MasterSummonerSummonMonster{1-9}Feature` -> `MasterSummonerSummonISingle` / `MasterSummonerSummon{2-9}Base` | SM I-IX at class levels 1,3,...,17; single, 1d3 from II, 1d4+1 from III | Monster Tactician clone. 5 + Cha shared daily uses. Standard action. Minutes per level. Free pool, or the summoner pool while the eidolon lives, with a conditional clear | IMPL; NATIVE (tiers I and III, no-eidolon branch) |
| Summoner SM (base) | CotW `SummonerSummonMonster{1-9}Feature` -> `SummonerSummonISingle` / `SummonerSummon{2-9}Base` | SM I-IX | 3 + Cha. Eidolon must be dead or unsummoned. One effect (pool clear) | IMPL; NATIVE (tier I) |
| Twinned Summoner SM | CotW; keeps the base Summoner features | SM I-IX | as base Summoner | IMPL (shared base Summoner roots, no separate adapter); NATIVE (tier I) |
| Monster Tactician SM | native features `c19f6f34...` ... `f63d23b4...` | SM I-IX; one ability per quantity | as native: 1 use, minutes per level (observed) | IMPL; the observer verified every child against its template; NATIVE (lowest granted tier at Inquisitor 6 is III) |
| Occultist Arcanist Conjurer's Focus | CotW `OccultistSummonMonster{1-9}Feature` -> `OccultistSummonISingle` / `OccultistSummon{2-9}Base` | SM I-IX at levels 1,3,...,17 | Arcane reservoir cost = tier, including lower-tier groups. Perfect Summoner: free use and one day. One effect | IMPL; NATIVE (tier III cost and duration, replacement, level-20 capstone) |
| Naturalist SNA | CotW `SummonerSummonNaturesAlly{1-9}Feature` -> `FeyCallerSummonNaturesAllyI` / `SummonerSummonNaturesAlly{2-9}Base` | SNA I-IX | Copies of the native SNA children, Spell type. Summoner resource and pool. Minutes per level | IMPL; NATIVE (tier I) |
| Fey Caller archetype | CotW `FeyCallerArchetype` | — | built but not offered in `SummonerClass.Archetypes` in 1.14.4c | ABSENT_IN_PROFILE; it would share the Naturalist roots |
| Feral Hunter SNA | CotW `FeralHunterSummonNaturesAlly{1-9}Feature` -> `FeralHunterSummonNaturesAllyI` / `...{2-9}Base` | SNA I-IX | 3 + Wis. Hunter level. Minutes. Own pool with clear. Usable with no pet or a dead pet. Spell type | IMPL; NATIVE (tier I) |
| Preservationist Bottled Ally | CotW `PreservationistBottledAlly{1-6}Feature` (AddKnownSpell, Alchemist) -> `PreservationistSummonNaturesAlly{I,II,IV,V,VII,IX}` | extract levels 1-6 -> SNA I, II, IV, V, VII, IX | Alchemist extract slot. Spell type. Standard action | IMPL; NATIVE (Bottled Ally I from a prepared slot) |
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

Structured engine observations, not screenshots. All save-free, all in a
fresh empty world, every created unit destroyed and the native census
restored, every profile restored exactly (see the state file for receipts).

**Observer and blocked family** (`observe-alternate-summoning-sources`,
`observe-alternate-summoning-blocked-family`):
- CotW profile: all seven families `Published` at the first update after
  `LoadDictionary`; registered identities 584 / 561 / 561 / 561 / 563 / 563
  / 353 (all of each family's); every adapter child's executed signature
  equals its authorized template's except the deliberate composed spawn and
  template/alignment action; host templates unchanged since resolution; the
  canonical publication is exact (1,093 references) and no adapter child sits
  under a canonical parent.
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
converted-ability path) and runs the native `UnitUseAbility` command.
Final run (`651dde310e63`): 20 / 20.

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

## Remaining (not checked in game)

Runtime is not checked for any of these; they are not claimed.

1. **Real UI selection.** No native action-bar popup was opened or clicked
   and no hotbar slot was used: the casts used the same converted-ability
   data the action bar builds, through `UnitUseAbility`. A popup-click
   driver exists for spellbook spells (Magic Circle) but not for class
   abilities, and the repository records that an earlier attempt to
   install a slot through `settings.SetSlot` hung the game. Needs a
   party-member caster selected in the UI and an ability-group popup
   driver.
2. **Hotbar reuse and saved references.** No saved shortcut to an adapter
   child was reloaded: the reduced CotW profile cannot hydrate the
   established working save, and no save-backed fixture exists for a
   shortcut. What was observed instead: a blocked family's identities stay
   registered as unavailable placeholders; with the module OFF every
   identity registers and nothing is exposed; and an unexposed registered
   child is refused through its root (the preserved Master Summoner tier-I
   cast). That is what the real loader resolves a saved reference to, but a
   reload of a real shortcut was not performed.
3. **Turn-based sample.** Every cast ran synchronously outside combat, which
   is the same command path in either mode; no turn-based combat round was
   exercised.
4. **Master Summoner with an eidolon.** The disposable Master Summoner took
   no lesser eidolon, so the one-summon-while-the-eidolon-lives branch and
   the eidolon's return were not exercised; only the no-eidolon coexistence
   branch was.
5. **Recent-creature cleanup** (for example Tyrannosaurus swallow/grapple
   reset on reload) through an adapter route.
6. **Higher tiers.** One tier per family was cast (tier I or III; Occultist
   also at level 20). Middle and last menu positions were not each cast.

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
