# Inherited summon templates per owned placement (DEF-05 / S5 T1)

Status: **source and graph-census checkpoint on a review branch. Not complete and not ready for principal review:** the
native census and the representative native casts are still owed (see "Remaining"). Branch
`codex/def05-inherited-template-cleanup`, draft PR #186, stacked on the accepted Sprint 4 closure PR #185
(`5c662d50b0`). Not merged, not released.

## Contract

Epic S5 T1, inventory DEF-05 / GS-REP-0002. The v0.0.186 graph census found 65 untemplated Summon Monster placements
that apply a native template with no cleanup and 36 that carry cleanup with nothing to remove. These are graph
observations, not 101 demonstrated cast defects.

The work:
- Replace the creature-name list with a per-placement policy over the actual cloned spawn graph.
- Remove only the exact inherited celestial and fiendish identities, after the donor's applications.
- Preserve deliberate templates, innate subtypes, foreign effects and donor blueprints.
- Use one composition rule for canonical and alternate callers.
- Keep PR #138's comparator and its withdrawn alternate-caller allegation.
- Document harmless redundancy explicitly.

## Retained observations (original identities)

- **v0.0.186 graph census** (PR #138 record, `RELEASE-NOTES-0.0.186.md`, KNOWN-ISSUES): 281 untemplated placements across
  63 creatures; 81 apply, 52 carry cleanup, 16 both, 65 apply without cleanup, 36 cleanup without application.
- **Reproduced on the accepted Sprint 4 product** `16708bd5e237` (package `e017a5f3d8ec…`), unchanged, with Call of the
  Wild staged: session `20261118T032921Z-owner-0.33.0-template-census-baseline-cotw`, PASS. The same 281 rows gave
  164 neither, 65 apply only, 36 cleanup only and 16 both. Owner saves (95), installation and registry were unchanged
  and restored.
- **A first attempt without Call of the Wild** failed before the census, because the contract fixture needs CotW's Master
  Summoner (`feature-missing:abcff90e1fdc4729a26f886c12aa0b9a`): session
  `20261118T032657Z-owner-0.33.0-template-census-baseline`, FAIL. It is a fixture prerequisite, kept as a failure.

Native identities: Summon Monster Celestial I `83a8a909e7ad19b4fa57e306884cc8bd`, Fiendish I
`c707fdbe58d0c614a89a872b76777f4b`, Celestial II `ec906031bb8766f42a37a52cf1d89836`, Fiendish II
`4b808ccd7e0b7bd43b10b2d47733b1a4`.

## Placement-level classification

**The deciding input is the donor.** Each canonical placement clones the native Summon Monster carrier for its parent
tier and quantity: the tier parent, or its "single", "d3" or "d4" child. Every placement of the same shape has the same
donor applications, whatever the creature. Every applying donor pairs a celestial and a fiendish buff, chosen at run
time by alignment. So the same creature needs removal on some placements and not on others. Lemure, for example,
removes at tiers 2–6 and has nothing to remove at tiers 7–9.

**Untemplated Summon Monster placements**, all 281 from the retained census. The "Old cleanup" column lists the
creatures on which the removed v0.0.186 name list put a cleanup.

| Parent tier | Quantity | Donor applies (by caster alignment) | Required disposition | Placements | Old cleanup present on | Old verdict |
|---|---|---|---|---|---|---|
| 2 | One | Celestial I + Fiendish I | remove inherited celestial and fiendish | 5 | lemure | 4 omitted, 1 correct |
| 3 | One | Celestial I + Fiendish I | remove inherited celestial and fiendish | 2 | dretch, lantern-archon | 0 omitted, 2 correct |
| 3 | OneD3 | Celestial I + Fiendish I | remove inherited celestial and fiendish | 5 | lemure | 4 omitted, 1 correct |
| 4 | One | Fiendish I + Celestial II | remove inherited celestial and fiendish | 16 | hound-archon | 15 omitted, 1 correct |
| 4 | OneD3 | Celestial I + Fiendish I | remove inherited celestial and fiendish | 2 | dretch, lantern-archon | 0 omitted, 2 correct |
| 4 | OneD4PlusOne | Celestial I + Fiendish I | remove inherited celestial and fiendish | 5 | lemure | 4 omitted, 1 correct |
| 5 | One | none | no inherited template | 10 | babau, bearded-devil, kyton-chain-devil | 3 redundant |
| 5 | OneD3 | Fiendish II + Celestial II | remove inherited celestial and fiendish | 16 | hound-archon | 15 omitted, 1 correct |
| 5 | OneD4PlusOne | Celestial I + Fiendish I | remove inherited celestial and fiendish | 7 | dretch, lantern-archon, lemure | 4 omitted, 3 correct |
| 6 | One | none | no inherited template | 10 | - | correct |
| 6 | OneD3 | none | no inherited template | 10 | babau, bearded-devil, kyton-chain-devil | 3 redundant |
| 6 | OneD4PlusOne | Fiendish II + Celestial II | remove inherited celestial and fiendish | 23 | dretch, hound-archon, lantern-archon, lemure | 19 omitted, 4 correct |
| 7 | One | none | no inherited template | 7 | bone-devil | 1 redundant |
| 7 | OneD3 | none | no inherited template | 10 | - | correct |
| 7 | OneD4PlusOne | none | no inherited template | 33 | babau, bearded-devil, dretch, hound-archon, kyton-chain-devil, lantern-archon, lemure | 7 redundant |
| 8 | One | none | no inherited template | 7 | barbed-devil, hezrou | 2 redundant |
| 8 | OneD3 | none | no inherited template | 7 | bone-devil | 1 redundant |
| 8 | OneD4PlusOne | none | no inherited template | 43 | babau, bearded-devil, dretch, hound-archon, kyton-chain-devil, lantern-archon, lemure | 7 redundant |
| 9 | One | none | no inherited template | 6 | astral-deva, trumpet-archon | 2 redundant |
| 9 | OneD3 | none | no inherited template | 7 | barbed-devil, hezrou | 2 redundant |
| 9 | OneD4PlusOne | none | no inherited template | 50 | babau, bearded-devil, bone-devil, dretch, hound-archon, kyton-chain-devil, lantern-archon, lemure | 8 redundant |

- **Tier 2 One** (Celestial I + Fiendish I): lemure, small-air-elemental, small-earth-elemental, small-fire-elemental, small-water-elemental
- **Tier 3 One** (Celestial I + Fiendish I): dretch, lantern-archon
- **Tier 3 OneD3** (Celestial I + Fiendish I): lemure, small-air-elemental, small-earth-elemental, small-fire-elemental, small-water-elemental
- **Tier 4 One** (Fiendish I + Celestial II): air-mephit, dust-mephit, earth-mephit, fire-mephit, hell-hound, hound-archon, ice-mephit, magma-mephit, medium-air-elemental, medium-earth-elemental, medium-fire-elemental, medium-water-elemental, ooze-mephit, salt-mephit, steam-mephit, water-mephit
- **Tier 4 OneD3** (Celestial I + Fiendish I): dretch, lantern-archon
- **Tier 4 OneD4PlusOne** (Celestial I + Fiendish I): lemure, small-air-elemental, small-earth-elemental, small-fire-elemental, small-water-elemental
- **Tier 5 One** (none): babau, bearded-devil, bralani-azata, kyton-chain-devil, large-air-elemental, large-earth-elemental, large-fire-elemental, large-water-elemental, salamander, xill
- **Tier 5 OneD3** (Fiendish II + Celestial II): air-mephit, dust-mephit, earth-mephit, fire-mephit, hell-hound, hound-archon, ice-mephit, magma-mephit, medium-air-elemental, medium-earth-elemental, medium-fire-elemental, medium-water-elemental, ooze-mephit, salt-mephit, steam-mephit, water-mephit
- **Tier 5 OneD4PlusOne** (Celestial I + Fiendish I): dretch, lantern-archon, lemure, small-air-elemental, small-earth-elemental, small-fire-elemental, small-water-elemental
- **Tier 6 One** (none): erinyes-devil, huge-air-elemental, huge-earth-elemental, huge-fire-elemental, huge-water-elemental, invisible-stalker, lillend-azata, shadow-demon, shadow-mastiff, succubus
- **Tier 6 OneD3** (none): babau, bearded-devil, bralani-azata, kyton-chain-devil, large-air-elemental, large-earth-elemental, large-fire-elemental, large-water-elemental, salamander, xill
- **Tier 6 OneD4PlusOne** (Fiendish II + Celestial II): air-mephit, dretch, dust-mephit, earth-mephit, fire-mephit, hell-hound, hound-archon, ice-mephit, lantern-archon, lemure, magma-mephit, medium-air-elemental, medium-earth-elemental, medium-fire-elemental, medium-water-elemental, ooze-mephit, salt-mephit, small-air-elemental, small-earth-elemental, small-fire-elemental, small-water-elemental, steam-mephit, water-mephit
- **Tier 7 One** (none): bebelith, bone-devil, greater-air-elemental, greater-earth-elemental, greater-fire-elemental, greater-water-elemental, vrock
- **Tier 7 OneD3** (none): erinyes-devil, huge-air-elemental, huge-earth-elemental, huge-fire-elemental, huge-water-elemental, invisible-stalker, lillend-azata, shadow-demon, shadow-mastiff, succubus
- **Tier 7 OneD4PlusOne** (none): air-mephit, babau, bearded-devil, bralani-azata, dretch, dust-mephit, earth-mephit, fire-mephit, hell-hound, hound-archon, ice-mephit, kyton-chain-devil, lantern-archon, large-air-elemental, large-earth-elemental, large-fire-elemental, large-water-elemental, lemure, magma-mephit, medium-air-elemental, medium-earth-elemental, medium-fire-elemental, medium-water-elemental, ooze-mephit, salamander, salt-mephit, small-air-elemental, small-earth-elemental, small-fire-elemental, small-water-elemental, steam-mephit, water-mephit, xill
- **Tier 8 One** (none): barbed-devil, elder-air-elemental, elder-earth-elemental, elder-fire-elemental, elder-water-elemental, gorgon, hezrou
- **Tier 8 OneD3** (none): bebelith, bone-devil, greater-air-elemental, greater-earth-elemental, greater-fire-elemental, greater-water-elemental, vrock
- **Tier 8 OneD4PlusOne** (none): air-mephit, babau, bearded-devil, bralani-azata, dretch, dust-mephit, earth-mephit, erinyes-devil, fire-mephit, hell-hound, hound-archon, huge-air-elemental, huge-earth-elemental, huge-fire-elemental, huge-water-elemental, ice-mephit, invisible-stalker, kyton-chain-devil, lantern-archon, large-air-elemental, large-earth-elemental, large-fire-elemental, large-water-elemental, lemure, lillend-azata, magma-mephit, medium-air-elemental, medium-earth-elemental, medium-fire-elemental, medium-water-elemental, ooze-mephit, salamander, salt-mephit, shadow-demon, shadow-mastiff, small-air-elemental, small-earth-elemental, small-fire-elemental, small-water-elemental, steam-mephit, succubus, water-mephit, xill
- **Tier 9 One** (none): astral-deva, ghaele-azata, glabrezu, ice-devil, nalfeshnee, trumpet-archon
- **Tier 9 OneD3** (none): barbed-devil, elder-air-elemental, elder-earth-elemental, elder-fire-elemental, elder-water-elemental, gorgon, hezrou
- **Tier 9 OneD4PlusOne** (none): air-mephit, babau, bearded-devil, bebelith, bone-devil, bralani-azata, dretch, dust-mephit, earth-mephit, erinyes-devil, fire-mephit, greater-air-elemental, greater-earth-elemental, greater-fire-elemental, greater-water-elemental, hell-hound, hound-archon, huge-air-elemental, huge-earth-elemental, huge-fire-elemental, huge-water-elemental, ice-mephit, invisible-stalker, kyton-chain-devil, lantern-archon, large-air-elemental, large-earth-elemental, large-fire-elemental, large-water-elemental, lemure, lillend-azata, magma-mephit, medium-air-elemental, medium-earth-elemental, medium-fire-elemental, medium-water-elemental, ooze-mephit, salamander, salt-mephit, shadow-demon, shadow-mastiff, small-air-elemental, small-earth-elemental, small-fire-elemental, small-water-elemental, steam-mephit, succubus, vrock, water-mephit, xill

The old verdicts total exactly 65 omitted and 36 redundant.

**Other placements.** The census covers them; their counts come from the native run that is still owed:
- Deliberately templated Summon Monster placements: caster-chosen, celestial and fiendish abilities. Disposition:
  preserve the deliberate template. The project template action names the four native identities it replaces, after
  the donor's applications.
- Nature's Ally placements: outside the Summon Monster contract. Their donors must apply no native template, and
  nothing is removed.

**Callers.**
- The canonical ordinary Summon Monster spells publish every placement.
- The Call of the Wild alternate callers build children for the same placements: Monster Tactician, Summoner, Master
  Summoner and Occultist for Summon Monster; Naturalist, Feral Hunter and Preservationist for Nature's Ally.

  These children come from the caller's own host graph. PR #138 measured that it applies no native template, so a
  removal on them mirrors the canonical decision. That mirror is a deliberate, harmless redundancy: the run-time action
  removes only those of the four identities actually present. The census lists each one.

## What changed

**Gameplay:** inherited-template removal per placement.
- `Summoning/InheritedTemplatePolicy.cs`, engine-free. It removes inherited templates for an untemplated Summon Monster
  placement whose donor applies a native template. A templated placement keeps its deliberate template. With no donor
  application, nothing is removed, and Nature's Ally is outside the contract.
- `ExpandedSummoningAbilityBuilder`:
  - The twelve-name `InheritedTemplateRemovalCreatures` list is removed.
  - `RequiresInheritedTemplateRemoval(placement, donor)` applies the policy to `DonorAppliedTemplateIds(donor)`.
  - `Configure` records the native carrier each placement clones (`RecordedDonor`), before any publication rewrites
    the native parents.
  - The removal is still `ContextActionRemoveInheritedSummonTemplates` with the exact four identities, appended to each
    spawn branch's after-spawn list after the donor's actions. Donor blueprints are not mutated.
- `AlternateSummoningAdapters` decides from the placement's recorded donor, so both callers apply one rule.

**Effect on the canonical graphs.** Removal is added to the 65 omitted placements and dropped from the 36 redundant
ones. Blueprint identities and keys are unchanged. Units already summoned in a save keep whatever they had.

**Diagnostics:**
- `Summoning/InheritedTemplateCensus.cs`, engine-free, judges observations with its own statement of the contract
  (not the policy). Its findings: skipped, duplicate, no-spawn-branch, omitted, duplicate-removal, wrong-owner,
  identities, before-donor, extra, deliberate-template-missing, unremoved-application and ally-native-template.
  Justified redundancy is listed separately.
- The native census in `disposable-alternate-summoning-contracts` replaces the creature-level wiring check. It covers:
  - every canonical placement of both families, with templated abilities and every spawn branch, including nested and
    quantity ones;
  - every Master Summoner and Naturalist child built through the production adapter.

  Donors are read from the recorded carrier, which must still have its original component array and action count.

**Tests:** `expanded-summoning.inherited-template.*` (six cases).

**Fixtures:** the session `scripts/compatibility/loader-compat/sessions/inherited-template-census.json`, with Call of the
Wild staged.

## False-pass controls

The domain tests take placement intent from `ExpandedSummoningCatalog`. Donor applications come from the retained
census table above, not from the policy.

- **Policy agreement:** the policy agrees with the census's independent contract on every placement of both families,
  and it removes on exactly the 81 placements whose donor applied a template.
- **Placement, not creature:**
  - lemure needs removal and no removal on different placements;
  - a creature published by both families is removed on Summon Monster and left alone on Nature's Ally;
  - a templated placement keeps its template even with an applying donor.
- **Correct graph:** a correct graph passes, including quantity placements, templated and Nature's Ally controls, and
  alternate children. Exactly the mirrored alternate removals are listed as justified redundancy.
- **Stale creature list:** it fails with exactly 65 omissions and 36 extras, matching the independent v0.0.186
  observation.
- **One mutation per named false pass,** each producing its own finding:

| Mutation | Finding |
|---|---|
| Omitted cleanup | omitted |
| Extra cleanup | extra |
| Cleanup moved to another placement | omitted and extra |
| Cleanup bound to another unit | wrong-owner |
| Cleanup before the donor application | before-donor |
| Duplicated cleanup | duplicate-removal |
| Removal naming a non-native look-alike (the project template or a foreign buff) | identities |
| Deliberate template lost | deliberate-template-missing |
| Wrong donor | unremoved-application |
| Reporter skipping a placement | skipped |
| Reporter reporting a placement twice | duplicate |
| Nature's Ally donor applying a native template | ally-native-template |
| No spawn branch | no-spawn-branch |

- **Non-native buffs:** a donor's non-native buff is not treated as an inherited template.
- **Native-only controls:** donor reference and content stability is checked natively, by the census, through each
  recorded donor's component array and action count.

## Commands and results

| Command | Result | Time |
|---|---|---|
| `Invoke-KmgGate.ps1 -Filter 'expanded-summoning.catalog'` | first: compile errors at the old census call sites; then production compiled, 1 of 1 | 26 s each |
| `Invoke-KmgGate.ps1 -Filter 'expanded-summoning.inherited-template'` | first: 2 of 6 (the test donor table had no shape for templated placements, a test-fixture defect); then 6 of 6 | 26 s, 4 s |
| `Invoke-KmgGate.ps1 -Filter 'summon;alternate-summoning;expanded-summoning'` | 825 of 825, before and after the repair | 25 s, 33 s |
| `tools/validate_icon_catalog.py` | PASS | under 5 s |
| `tools/validate_development_candidate.py --purpose def05-template-cleanup`, plus its fixture suites | PASS | 10 s |
| `Build-Local.ps1 -DevelopmentPurpose def05-template-cleanup` at `f232c2df7ec3` | PASS, 825 of 825 selected | 10 min 39 s |
| Loader-compat `inherited-template-census` on `f232c2df7ec3` | **FAIL, product defect** (below) | 115 s |
| `Build-Local.ps1` at `0544169f86ea` | **stopped by Claude Code because the system was critically low on memory**; no package; worktree clean; not restarted | stopped about 1 min after starting, during its repository-validation phase |

**The `f232c2df7ec3` candidate:**
- package `be98cd6ca4556d75a2ade240237c6af54f7290790ee24694fe5eb310f7376d46`;
- DLL `7afb74bdfab40b6a8a7b0008ff6e75b6b5a7c9fe309b86142ddab80e96e3b674`;
- harness `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`;
- session `20261118T035406Z-owner-0.33.0-inherited-template-census`, FAIL;
- owner saves, installation and registry unchanged and restored.

The failure was a product defect in this change. The alternate adapter re-resolved each donor from the native parent
at publication time, after the parent's menu had been replaced by the project's choices. "Native summon template missing
for Monster.2.small-air-elemental.One" followed. Monster Tactician and Summoner failed closed, keeping their original
host behaviour, and the contract fixture could not resolve Master Summoner tier 2, so no census verdict was produced.

It was repaired in `0544169f86` by recording the donor when the canonical spell is built, with 825 of 825 passing. That
repair has no native evidence yet.

The v0.0.191 release validator's "inherited-template membership unchanged" check names the removed list. It is a
release disclosure check, not a development gate, and it already fails on the accepted stack for unrelated reasons. The
next release validator must account for DEF-05 replacing the list.

## Remaining (S5 T1 is not complete)

1. **Build one unchanged candidate at the current source head** (`0544169f86`, or a later records-only head) and run
   `inherited-template-census`. It must report no findings and no unresolved or mutated donors, every expected
   observation present, quantity removals and deliberate templates present, and the justified redundancy listed.
2. **Representative native casts,** chosen from the classification above. Each must observe the source ability and
   parent choice, one source debit, the spawned quantity and live units, the native template facts by exact identity,
   duration, retained innate and foreign effects, cancellation, and no donor or unrelated-row change. Cases:
   - an untemplated placement whose donor applies, cast by a good caster (inherited celestial removed), for example tier
     4 single (Fiendish I + Celestial II) on a mephit or an elemental;
   - the same with an evil caster (inherited fiendish removed);
   - a quantity wrapper (tier 3 d3, or tier 6 d4+1, which uses the second-tier identities);
   - a deliberate celestial and fiendish control on a templated creature;
   - an alternate-caller child (Master Summoner, with Call of the Wild);
   - a no-template control (tier 7+ untemplated);
   - Summon Monster and Nature's Ally preservation controls (the preserved native tier I and an SNA row).

   No existing fixture casts canonical placements with real spell slots across alignments, so this needs a new native
   fixture. It did not fit this checkpoint.
3. **Review records,** once both are done. S5 T1 must not be called complete before then.

## Limits

- The census is graph evidence; it does not say what a cast unit carries.
- The donor table in the domain tests is the retained observation of one installation (game 2.1.7b as staged). A
  different native library needs the census, not the table.
- Units summoned before this change keep their buffs.
