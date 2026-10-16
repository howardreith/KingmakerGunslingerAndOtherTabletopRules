# Expanded Summoning: alternate summoning sources

Status: development checkpoint. Source, identities and focused domain coverage
are in place. **No native (in-game) evidence exists yet.** The guarded
candidate path is blocked by the release-bound repository validator (see
"Deferred native gate"). Nothing here claims that any caller works in game.

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
- **Module OFF.** Identities still register, so saved references resolve. No
  host menu changes.

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
| Runtime observation | none |
| Graph discovery | `observe-alternate-summoning-sources` will add a census of every spawn graph and every component that references a canonical parent; it has not run |

Abbreviations: **IMPL** means implemented with focused source and domain
checks only; the native check is blocked (see below). **SHARED** means the
route reaches the canonical parents at use time, so it shows the canonical
menu; that is static evidence only.

| Route | Provider / identity | Tier and quantity | Ability type, action, resource, duration, pool | Disposition |
|---|---|---|---|---|
| Master Summoner SM | CotW `MasterSummonerSummonMonster{1-9}Feature` -> `MasterSummonerSummonISingle` / `MasterSummonerSummon{2-9}Base` | SM I-IX at class levels 1,3,...,17; single, 1d3 from II, 1d4+1 from III | Monster Tactician clone. 5 + Cha shared daily uses. Standard action. Minutes per level. Free pool, or the summoner pool while the eidolon lives, with a conditional clear | IMPL |
| Summoner SM (base) | CotW `SummonerSummonMonster{1-9}Feature` -> `SummonerSummonISingle` / `SummonerSummon{2-9}Base` | SM I-IX | 3 + Cha. Eidolon must be dead or unsummoned. One effect (pool clear) | IMPL |
| Twinned Summoner SM | CotW; keeps the base Summoner features | SM I-IX | as base Summoner | IMPL (shared base Summoner roots, no separate adapter) |
| Monster Tactician SM | native features `c19f6f34...` ... `f63d23b4...` | SM I-IX; one ability per quantity | as native (resource, pool and duration were not inspected statically) | IMPL; per-quantity lineage resolution unverified until the observer runs |
| Occultist Arcanist Conjurer's Focus | CotW `OccultistSummonMonster{1-9}Feature` -> `OccultistSummonISingle` / `OccultistSummon{2-9}Base` | SM I-IX at levels 1,3,...,17 | Arcane reservoir cost = tier, including lower-tier groups. Perfect Summoner: free use and one day. One effect | IMPL |
| Naturalist SNA | CotW `SummonerSummonNaturesAlly{1-9}Feature` -> `FeyCallerSummonNaturesAllyI` / `SummonerSummonNaturesAlly{2-9}Base` | SNA I-IX | Copies of the native SNA children, Spell type. Summoner resource and pool. Minutes per level | IMPL |
| Fey Caller archetype | CotW `FeyCallerArchetype` | — | built but not offered in `SummonerClass.Archetypes` in 1.14.4c | ABSENT_IN_PROFILE; it would share the Naturalist roots |
| Feral Hunter SNA | CotW `FeralHunterSummonNaturesAlly{1-9}Feature` -> `FeralHunterSummonNaturesAllyI` / `...{2-9}Base` | SNA I-IX | 3 + Wis. Hunter level. Minutes. Own pool with clear. Usable with no pet or a dead pet. Spell type | IMPL |
| Preservationist Bottled Ally | CotW `PreservationistBottledAlly{1-6}Feature` (AddKnownSpell, Alchemist) -> `PreservationistSummonNaturesAlly{I,II,IV,V,VII,IX}` | extract levels 1-6 -> SNA I, II, IV, V, VII, IX | Alchemist extract slot. Spell type. Standard action | IMPL |
| Spirit Summoner, Soulbound Summoner, Devil Binder SM | CotW | archetype removes the summon features | — | INTENTIONALLY_RESTRICTED (not restored) |
| Devil Binder Infernal Arcana | CotW, SM VI single (named) | one named creature | — | INTENTIONALLY_RESTRICTED |
| Hunter bonus SNA I-VI | CotW `AddKnownSpell` of the canonical parents | SNA I-VI | spellbook | SHARED |
| Druid spontaneous SNA | native conversion arrays | SNA I-IX | spellbook | SHARED (inferred: the native arrays hold the canonical parents) |
| Shaman Friend to Animals hex | CotW reuses the native druid conversion arrays | SNA I-IX | spellbook | SHARED (inferred) |
| Summoner, Witch, Shaman, Psychic and other class spell lists; Witch Animal patron; Psychic grants | CotW `AddToSpellList` / `AddKnownSpell` on the canonical parents | — | spellbook | SHARED |
| Herald Caller spontaneous SM | native, if present; CotW has no Herald Caller code | — | — | UNVERIFIED: the native identity was not located statically; the observer's canonical-reference census will list it if present |
| Occultist / Relic Hunter Servitor | CotW, fixed native single variant per tier (SM I-VII), rank switch | one creature per tier | SpellLike, mental focus | DEFERRED_SPECIALIZED_ADAPTER (no menu; a design question, below) |
| Occultist / Relic Hunter Shadow Beasts | CotW, load-time snapshot of the canonical SM children and elemental roots, ShadowSpell 60 | SM I-IX | shadow | DEFERRED_SPECIALIZED_ADAPTER (needs a shadow-reality adapter) |
| Warpriest / Divine Tracker Battle Companion (6 blessings) | CotW, rank switch over fixed native single variants | one creature per rank | Supernatural, blessing resource | DEFERRED_SPECIALIZED_ADAPTER (no menu; a design question, below) |
| Shadow Conjuration / Greater / Shades | CotW, load-time snapshot clones (MergeIds GUIDs) | SM I-III / I-VI / I-VIII | shadow 20/60/80 | DEFERRED_SPECIALIZED_ADAPTER |
| Skald Spell Kenning | CotW, load-time duplicates of the list variants | SM I-VI | kenning resource | DEFERRED_SPECIALIZED_ADAPTER |
| Shaman / Spirit Whisperer Lore Wish | CotW, load-time duplicates from every spellbook | SM I-VIII, SNA I-VII | manifestation resource | DEFERRED_SPECIALIZED_ADAPTER |
| Twinned Eidolon spell-like | CotW, load-time snapshot of the Summoner-list SM children | SM I-V | eidolon | DEFERRED_SPECIALIZED_ADAPTER |
| Pact Wizard Animal patron conversion | CotW, by-reference arrays of native variant objects | SNA IV, VIII, IX | conversion | DEFERRED_SPECIALIZED_ADAPTER (a conversion adapter, not a menu) |
| Rogue Major Magic | CotW, copy of the then-direct SM I | SM I | SpellLike | DEFERRED_SPECIALIZED_ADAPTER |
| Scrolls, wands, crafting | native items; CotW `AddScroll` covers only CotW spells | — | — | UNVERIFIED: items were not inspected in the installed profile |
| Call Celestial Ally, Spark of Life, element-restricted summons | — | named or element only | — | INTENTIONALLY_RESTRICTED (no general menu added) |

**Coverage limits.**
- The CotW findings come from reading the decompiled installed DLL; the
  Kingmaker findings come from decompiled engine types. Native blueprint data
  (asset bundles) was not read statically.
- Monster Tactician's spawn sets, the Druid conversion arrays, Herald Caller
  and native items therefore await the observer.
- A generic CotW compatibility PASS is not evidence that any archetype above
  was exercised.

## Verification performed

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
- One unintended complete-suite run happened while building the test binary.
  It failed before the ledger tests were updated; its output was not
  retained, and it is not evidence either way.
- Fifteen existing ledger-shape domain tests were updated as every appending
  sprint has done. Totals gain the computed adapter block, and
  canonical-allocation scans exclude the adapter namespace, which has its own
  both-ways test. No Python release validator was changed.

## Deferred native gate

**Why it is blocked.** `Build-Local.ps1` is the only candidate path and runs
the repository wrapper. The wrapper dispatches by `Info.json` version to the
v0.0.156 release validator, which fails with "The firearm patch must not
change blueprint identities". Any branch that appends identities is therefore
unbuildable as a candidate until a later release prepares its own validator.
This checkpoint does not bump the version, weaken a validator or bypass
packaging.

**Required at integration**, on a candidate that a release-preparation step
can build, using guarded launches only:

1. Save-free:
   `scripts/Invoke-KingmakerRuntimeTest.ps1 -Scenario observe-alternate-summoning-sources`,
   in the current CotW profile. It also runs through
   `scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1` for the
   `gunslinger-call-of-the-wild` and `gunslinger-only` profiles, which check
   PASS and `AbsentInProfile` respectively. Record every family's status, the
   template comparisons, roster parity and both censuses. Turn its
   discoveries into final dispositions above.
2. Player path, on the disposable working save, for Master Summoner and
   Occultist first:
   - actual granted feature -> action bar -> menu, with first / middle /
     last choices
   - the tier-I direct-to-menu conversion on an existing hotbar slot
   - exact resource spend (5 + Cha; reservoir = tier, with a lower-tier group)
   - Master Summoner multiple effects, then eidolon return/summon
   - Occultist one-effect replacement and the level-20 capstone
   - cancellation and unavailability
   - a representative recent creature with cleanup (for example
     Tyrannosaurus swallow/grapple reset)
   - turn-based and RTWP
3. Then each remaining execution contract once:
   - base Summoner (eidolon restriction, replacement)
   - Monster Tactician (per-quantity menus, Summon Tactics)
   - Naturalist
   - Feral Hunter
   - Preservationist (extract consumption, level mapping)
4. Module OFF / cold load: a saved shortcut to an adapter child must be
   unavailable. Restoration of the human install (KMG 0.0.140 at intake) must
   be exact.

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

## Open design questions (owner)

1. Servitor and Battle Companion are fixed one-creature-per-tier graphs in
   CotW. Should they ever offer a creature menu?
2. Should Elemental Insight apply to Bottled Ally or other caller summons?
3. Should Monster Tactician show three per-quantity menus per tier, as
   implemented, or one combined menu? A combined menu would be a new root
   rather than the host's own abilities.
