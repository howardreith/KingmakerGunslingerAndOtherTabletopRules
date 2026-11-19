# Inherited summon templates per owned placement (DEF-05 / S5 T1)

Status: **the owner-approved tier-one donor contract is implemented and built. Its census rerun FAILED on one condition
of that contract, which contradicts released publication behaviour. The cast session was not run. S5 T1 is stopped for
review and not ready for principal review** (see "Tier-one donor contract and census rerun"). Branch
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

## Review corrections (second checkpoint)

The review of the source checkpoint found four census false-pass paths. Each is repaired in `552a3d766f`, with focused
controls.

1. **Independently verified donor.** Before, the policy and the census both read `RecordedDonor`, so a wrong carrier
   could pass all three checks together. `InheritedTemplateDonorContract` (diagnostic only; the policy never reads it)
   states the retained native-template signature for each (family, parent tier, quantity). The signatures come from
   session `20261118T032921Z`. It also checks the family name (`SummonMonster…` and `SummonNaturesAlly…`) and the
   tier-one direct parents (`8fd74eddd9b6c224693d9ab241f25e84` for Summon Monster, `c6147854641924442a3bb736080cfeb6`
   for Nature's Ally). The census checks every recorded donor against it and reports any of:
   - `wrong-donor`;
   - one shape on two carriers;
   - two quantities of one tier sharing a carrier.

   The retained census did not keep donor asset identities or names. The census now records them per shape, and only
   the template signature, family name and tier-one parent are enforced. Shapes the retained census never saw (tier 1
   single, tier 2 d3 and tier 3 d4+1, used only by templated creatures) are reported as recorded now, not proved.

   **Controls:** single and d3 carriers swapped (tier 4), d3 and d4+1 swapped (tier 5), the wrong template pair, a
   Nature's Ally placement on a Summon Monster carrier, the wrong tier-one parent, one shape on two carriers, and two
   quantities on one carrier.

   **Limit:** a full swap of two quantities with identical signatures, such as tier 3 single and d3, is visible only
   through the per-shape identities the census now records.
2. **All seven alternate families.** The census enumerates `AlternateSummoningSourceCatalog.All`: Monster Tactician,
   Summoner, Master Summoner, Occultist, Naturalist, Feral Hunter and Preservationist.
   - Expected children come from the projection, and actual children from the production adapter.
   - A family that cannot be resolved or built is reported, and its children stay expected, so they show as skipped.
   - Totals are given by family and by spell family.
   - Every family must be built with observed children equal to expected.

   The failed first candidate, which broke Monster Tactician and Summoner, is the direct evidence that checking only
   Master Summoner was not enough.

   **Control:** a whole missing family yields exactly its child as skipped.
3. **Donor immutability receipts.** `DonorRecord` now keeps an immutable receipt taken before cloning:
   - donor identity and name;
   - the components array and component references;
   - spawn references;
   - each spawn's after-spawn array and its element references;
   - every traversed action, by reference and type, in order;
   - every `ApplyBuff` buff identity;
   - the native application sequence.

   Receipts are cached per carrier. The census and the cast scenario report every difference.

   **Controls:** one action replaced with the count unchanged, a reordering, an `ApplyBuff` buff changed in place, and an
   after-spawn array swapped for one of equal length.
4. **Branch-level pairing and redundancy.** Approach B, branch-aware. Each canonical clone branch is paired with a donor
   branch by after-spawn shape: the donor's action types must prefix the clone's, and the same traversal position is
   preferred. Findings:
   - `branch-count`, unpaired, duplicate pairing and skipped donor branch;
   - `application-lost`: a donor branch's application missing from its clone;
   - `application-not-in-donor`.

   A removal with nothing to remove is listed in `JustifiedRedundant` with its reason for any caller, canonical as well
   as alternate.

   **Controls:** an applying donor branch whose clone lost the application, a skipped donor branch, two generated
   branches on one donor branch, a removal only on the wrong branch, and an unpaired branch.

**Tests:**

| Command | Result | Time |
|---|---|---|
| `Invoke-KmgGate.ps1 -Filter 'expanded-summoning.inherited-template'` | 10 of 10 | 7 s |
| `Invoke-KmgGate.ps1 -Filter 'summon;alternate-summoning;expanded-summoning'` | 829 of 829 (before and after the cast observer) | 38 s, 16 s |
| `Invoke-KmgGate.ps1 -Filter 'expanded-summoning.inherited-template;alternate-summoning;runtime-scenario;scenario-catalog;native-profile'` | first: a compile error (missing `using`); then 55 of 55 | 17 s |
| `Test-RuntimeScenarioPreflight.ps1` | 1205 passed | 27 s |
| `tools/validate_development_candidate.py --purpose def05-template-cleanup` | PASS | 4 s |
| `tools/validate_icon_catalog.py` | PASS | under 5 s |
| `git diff --check` | clean | |

No expected result in these tests is derived from `InheritedTemplatePolicy` or from the production `RecordedDonor`.

## Representative native-cast observer

`disposable-inherited-template-casts` (`bab7e7dd43`) runs in the save-free native first area of the summoning native
profile, with Call of the Wild staged. Session file: `inherited-template-casts.json`.

**Cases.** A level-20 Sorcerer casts through its own spontaneous slots:
- the affected tier-IV single Air Mephit, as a good caster and as an evil caster;
- a tier-VI d4+1 Air Mephit;
- the caster-chosen Dire Wolf, as a good caster (celestial project template) and as an evil caster (fiendish);
- a tier-VII Greater Air Elemental, with no inherited template;
- a Nature's Ally II Wolf.

A Call of the Wild Master Summoner (level 7) casts its tier-IV Air Mephit adapter child through its resource. One cast
is cancelled before commit.

**What each completed cast must show:**
- the parent-to-child conversion;
- exactly one debit;
- the quantity contract (1, or 2–5 for d4+1);
- exact unit identities;
- none of the four native template buffs;
- the project template and smite by exact identity, and not the opposite template;
- positive durations;
- every innate unit fact;
- the native summon marker, as the unrelated-effect control;
- the child graph's own native applications;
- no duplicate capture and no lingering entity.

**Cancellation must show:** the command admitted, the result `Interrupt`, no debit and no summon.

**Global checks:** donor receipts before and after, two unrelated published rows (SM IX Lemure d4+1 and SNA IV Wolf
d4+1) unchanged, and the fixture's exact unit, party and area snapshots restored.

## Native status

**The candidate.**
- Source: `bab7e7dd43fc2e1b89ccf497cf45d6548a3fa51a`.
- Package `ce1be0a9303c76e3597422916f8e622872d272f542b29f180e034b93eedc7865`.
- DLL `b607b92ff68eae822fd02a091d7623bbd0b5904abb420371894d667a9f700d47`.
- MVID `5720c765-9132-4289-b6df-1a3af67a5785`, read offline from the packaged DLL; no game has loaded it yet.
- Harness `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`.
- Build: 04:38:43 to 04:45:52 UTC (7 min 9 s), development validation PASS, 829 of 829 selected.

**Memory preflight**, before the build, after the earlier stop:
- physical 6.2 of 31.7 GB free;
- commit charge 45.8 of 56.7 GB (about 81%);
- pagefile 3.4 GB used;
- C: 324 GB free;
- no Kingmaker process;
- this session's idle Roslyn and MSBuild servers (0.9 GB) shut down first, leaving 6.6 GB free and 45.4 GB committed.

The largest consumers were browser processes and the WSL VM, which were left alone. No critical-pressure report was
active, but no prior successful-build baseline was recorded to compare with. The build completed.

**Both native sessions are blocked.** The loader-compat preflight refused to launch the census session: "Steam Cloud sync
for Kingmaker is not proven disabled (cloud_log.txt)". Steam's cloud log for App ID 640820 shows, at 2026-11-17 23:02
local (04:02 UTC):
- "Sync conflict" for `Quick_436.zks`, `Auto_1120.zks`, `Auto_1121.zks` and `steam-saves-release.json`;
- the remote copies newer than the local ones;
- no "Sync Disabled" line among the last six entries.

These are protected owner saves, and AGENTS.md requires a stop on cloud conflicts. Nothing was staged or launched, no
save was touched, and the conflict was not resolved. The refusal happened before the session lock, so it left no
journal. The cast session was not attempted.

**Prerequisite to resume:** the owner resolves or confirms the Kingmaker Steam Cloud state, with sync disabled for App ID
640820 so that the log records it. Then run, on this same unchanged package:
`inherited-template-census.json`, followed by `inherited-template-casts.json`.

## Evidence states, kept separate

| State | Identity | Result |
|---|---|---|
| Retained v0.0.186 observation | PR #138, `RELEASE-NOTES-0.0.186.md` | 65 apply without removal, 36 removal without application |
| Accepted Sprint 4 baseline reproduction | product `16708bd5e237`, session `20261118T032921Z-owner-0.33.0-template-census-baseline-cotw` | PASS; the same 281 rows |
| Missing Call of the Wild prerequisite | session `20261118T032657Z-owner-0.33.0-template-census-baseline` | FAIL, fixture prerequisite |
| Failed first DEF-05 candidate | `f232c2df7ec3`, package `be98cd6c…6d46`, DLL `7afb74bd…b674`, session `20261118T035406Z` | FAIL, product defect (repaired in `0544169f86`) |
| Repaired source, never built | `0544169f86` (its build was stopped for low memory; no package) | none |
| Built-but-unexecuted candidate | `bab7e7dd43fc`, package `ce1be0a9…7865`, DLL `b607b92f…0d47`, MVID `5720c765-…5785` | built; never loaded by the game; its census launch was refused at the Steam Cloud preflight (a safety refusal, not a gameplay run); superseded by the second review's corrections |
| Built-but-unexecuted candidate (second review) | `0af2b30bd4c7`, package `336b88e0…c0f8`, DLL `6c44072b…390c`, MVID `63c5f429-…1daa0` | built; never loaded by the game; superseded by the third review's correction |
| Frozen candidate, first census | `0d6006ac03a2`, package `f1fdd873…9dbd`, DLL `0575b4fc…7a45`, MVID `5e5ff7e5-…b8f7` | census session `20261118T215938Z-owner-0.33.0-inherited-template-census` **FAIL**: 170 repeated donor-receipt findings, representing two unique tier-one parent transitions; cast session not run |
| Tier-one contract candidate | `67704e6459ce`, package `ccfbf1d4…b961`, DLL `e30b2bd7…9d8a`, MVID `5151ff94-…b595` | census session `20261118T225322Z-owner-0.33.0-inherited-template-census` **FAIL**: both tier-one parents failed only `preserved-child-in-menu:0`; cast session not run |
| Withhold-aware candidate | `540556bbf004`, package `59b24c5a…44c5`, DLL `14e32cb0…0d69`, MVID `caaba169-…987f` | census session `20261119T192806Z-owner-0.33.0-inherited-template-census` **PASS**; cast session `20261119T193054Z-owner-0.33.0-inherited-template-casts` **FAIL**, 3 of 15 assertions (observer rank reader, donor assertion); unresolved diagnostic |
| Observer-repair candidate | `8ceb08ccef29`, package `3ef1bce7…7ccb`, DLL `e0dadbc9…cc35`, MVID `3806eb49-…0659` | census session `20261119T201715Z-owner-0.33.0-inherited-template-census` **PASS**; cast session `20261119T201938Z-owner-0.33.0-inherited-template-casts` **PASS**, 15 of 15 |

## Second review corrections (third checkpoint)

The review accepted the four earlier corrections at source and test level. Five more were made in `0af2b30bd4`. Each is
judged by an engine-free contract in `Summoning/InheritedTemplateCastContracts.cs`, used by the native observers and
exercised by focused negative controls.

1. **Exact source debit.** Each cast window now records every native spend and restore on the caster's own sources:
   - observer postfixes on `Spellbook.SpendInternal` and `Spellbook.RestoreSpontaneousSlots`;
   - observer postfixes on `UnitAbilityResourceCollection.Spend` and its private `Restore`;
   - each postfix is installed for the scenario and removed afterwards by method.

   It also snapshots every spontaneous budget, raw slot, known and custom spell identity, and resource.
   `InheritedSourceLedger` then requires exactly one spend event on the expected source, the expected fall on that
   counter only, and no other change.

   `Cleanup2SourceDebit` is reused: `ExactlyOneSpontaneous(book, level)` for the Sorcerer and `Unspent` for the
   cancellation. It is extended narrowly with `ExactlyOneResource(resource, cost)` for the Master Summoner, whose
   `CalculateCost` must be positive and equal the observed debit.

   **Controls:** two uses spent and one restored; the wrong spell level; the expected resource plus an unrelated one; a
   zero calculated cost; a cancellation with collateral mutation; a cancellation with a spend and matching restore.
2. **Exact quantity.** The child graph is read with `SummonSignatureReader` before the cast:
   - a single row is `Zero` dice plus a Simple bonus of 1;
   - a quantity row is one `D3` or one `D4` (Simple count 1) plus the Rank bonus `ProjectilesCount`, evaluated
     natively for the caster (0 for d3, 1 for d4+1);
   - every branch must spawn the expected unit;
   - the expected branch count comes from the recorded donor (canonical) or the untouched host row (Master Summoner).

   The output must be distinct live units of that blueprint, within the expression's range. The exact engine roll is
   not captured, so it is not claimed.

   **Controls:** d4 instead of d4+1; d3+1 instead of d4+1; a fixed count that fell in range; the right expression on the
   wrong unit; an extra spawn branch; a duplicate spawn event; a foreign unit.
3. **Exact duration.** Each spawned unit must have exactly one duration observation:
   - the level-20 Sorcerer: base 120 s and bonus 0;
   - the Master Summoner: the untouched Call of the Wild host row's own duration (rate, Zero dice, and a Simple or
     Rank bonus evaluated for the caster), stated before the cast;
   - a cancellation: none.

   **Controls:** a one-second duration; the right total from the wrong base and bonus; a missing duration in a quantity
   summon; a duration on a different unit.
4. **Exact template and smite sets** per unit:
   - untemplated and Nature's Ally units: no native template, no project template and no project smite;
   - a celestial or fiendish control: exactly its one project template and one smite, and nothing from the other side.

   Project templates are every registered `KMG.Summoning.Template.*` identity; smites are `KMG.Summoning.Smite.Celestial*`
   and `KMG.Summoning.Smite.Fiendish*`. Innate facts and the native summon marker are still required.

   **Controls:** another template band; both smites; the opposite smite alone; a duplicate template; a missing smite; a
   stray smite.
5. **Exact alternate child sets.** The census builds each family's full output. It classifies actual creature children
   from that output: not a preserved root copy, spawning a registered project unit, and associated with a placement
   only when the unit is that placement's own. Native options and preserved roots are excluded by that classification.
   - `InheritedChildSetContract` compares the expected and actual sets in both directions and reports missing-child,
     extra-child, duplicate-placement (more children than the projection gives), null-placement and
     wrong-placement-family.
   - Every actual creature child is observed, or recorded as unclassifiable, which fails the census.
   - A family passes only when its observed and actual counts equal the projection's.

   **Controls:** every expected child plus one unexpected one (extra-child); missing; wrong family; null; duplicate.

**Tests:**

| Command | Result | Time |
|---|---|---|
| `Invoke-KmgGate.ps1 -Filter 'expanded-summoning.inherited-template'` | 15 of 15 (five new cases), twice | 30 s, under 30 s |
| `Invoke-KmgGate.ps1 -Filter 'expanded-summoning.inherited-template;alternate-summoning;runtime-scenario;scenario-catalog;native-profile;cleanup2;source-debit'` | 107 of 107 | 19 s |
| `Invoke-KmgGate.ps1 -Filter 'summon;alternate-summoning;expanded-summoning'` | 834 of 834 | 36 s |
| `Test-RuntimeScenarioPreflight.ps1` | 1205 passed | 28 s |
| `tools/validate_development_candidate.py --purpose def05-template-cleanup` | PASS | 7 s |
| `tools/validate_icon_catalog.py`, `git diff --check` | PASS, clean | |

One compile error during this work (a missing `using` for `BlueprintBuff`) was fixed before the runs above.

**Replacement candidate**, built from `0af2b30bd4c76817f81815c2e492be98ea4bfee2`:
- package `336b88e0fbd68aad561be95746d67f0a5327e143ae87110e43ed1545dea4c0f8`;
- DLL `6c44072b228258cd4920961c66fe9a2de7ccfe21a52d116231f41a09afc9390c`;
- MVID `63c5f429-cf60-4abd-b91a-9d11a6a1daa0`, read offline from the packaged DLL; no game has loaded it;
- harness `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`;
- build 13:33:37 to 13:43:18 UTC (9 min 41 s), development validation PASS, 834 of 834 selected.

**Memory preflight**, compared with the last successful build:

| | Now | Last successful build |
|---|---|---|
| Physical free | 4.9 of 31.7 GB | 6.6 GB |
| Commit charge | 47.1 of 54.7 GB (86%) | 45.4 of 56.7 GB (80%) |

The pagefile was 3.3 GB used, C: had 326 GB free, and no Kingmaker process was running. This session's idle compiler
servers were shut down first. Nothing reported critical pressure, and the build completed.

**Steam Cloud safety.** Before and after the build, Steam's cloud log for App ID 640820 still ended with the
2026-11-17 23:02 local "Sync conflict" entries for `Quick_436.zks`, `Auto_1120.zks`, `Auto_1121.zks` and
`steam-saves-release.json`, with no "Sync Disabled" among the last six.

No launch was attempted, and no save, cloud copy or log was touched. The earlier refusal of `bab7e7dd43fc` stays a
safety refusal, not a gameplay run.

## Third review correction (fourth checkpoint)

The review accepted the five second-review corrections at source and test level. One false-pass path remained, and
`0d6006ac03` closes it.

**The defect.** The cast observer took its expected project template and smite from the tested child's own
`ContextActionApplySummonTemplateByCaster`. A child wired to the wrong band could therefore certify itself.

**The fix.** For each deliberately templated case, before the cast:
- the expected unit is resolved from `item.UnitSymbol`, and its hit dice are read from its own `AddClassLevels`;
- the band comes from `InheritedDeliberateTemplateContract.Band`, which restates the established unit-HD contract
  (below 5 Low, 5–10 Mid, above 10 High). A domain test checks it agrees with `SummonTemplateBandPolicy` from HD 0
  to 30;
- the identities are resolved directly from the registered stable symbols
  `KMG.Summoning.Template.{Celestial,Fiendish}.<Band>` and `KMG.Summoning.Smite.{Celestial,Fiendish}.Available`;
- the child graph must hold exactly one dynamic-template action, whose celestial and fiendish templates and smites
  match those identities, with `ReplacedNativeTemplateBuffs` exactly the four native identities;
- the live `InheritedBuffSetContract` check receives only the independent expectation;
- both the independently expected identities and the identities wired into the child are recorded.

The graph check and the live check fail independently.

**Controls:**
- a celestial action wired to the wrong band;
- a celestial action wired to a fiendish template while keeping the celestial smite;
- the right template with the wrong smite;
- no dynamic-template action;
- two dynamic-template actions;
- incomplete or wrong replaced identities;
- a live unit carrying exactly the wrong identity its defective child wired, which still fails;
- the exact graph and the exact live set, which pass.

**Tests:**

| Command | Result | Time |
|---|---|---|
| `Invoke-KmgGate.ps1 -Filter 'expanded-summoning.inherited-template'` | 16 of 16 (one new case) | 26 s |
| `Invoke-KmgGate.ps1 -Filter 'expanded-summoning.inherited-template;alternate-summoning;runtime-scenario;scenario-catalog;native-profile;template-band'` | 61 of 61 | 2 s (incremental) |
| `Test-RuntimeScenarioPreflight.ps1` | 1205 passed | 28 s |
| `tools/validate_development_candidate.py --purpose def05-template-cleanup` | PASS | 5 s |
| `git diff --check` | clean | |

The summoning suite was not rerun on its own, because the changed code is used only by the cast observer. The
candidate build selected and passed it: 835 of 835.

**Memory preflight:**
- physical memory 4.9 of 31.7 GB free;
- commit charge 44.6 of 55.0 GB (81%), in line with the last two successful builds;
- pagefile 2.5 GB used;
- C: 326 GB free;
- no Kingmaker process;
- this session's idle compiler servers were shut down first.

**Final replacement candidate (frozen)**, built from `0d6006ac03a22a16b3ac6dec3197ac1eef68cfa6`:
- package `f1fdd87313218ef858976b81ea1577c6afd53d2b9f0e671460b9043db59d9dbd`;
- DLL `0575b4fc0103163560b9d85763bcbda3bbf711a8ec4a9f8aa21ea00976697a45`;
- MVID `5e5ff7e5-7ca8-4539-ba02-29334dfdb8f7`, read offline from the packaged DLL; no game has loaded it;
- harness `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089`;
- build 18:46:14 to 18:53:51 UTC (7 min 37 s), development validation PASS, 835 of 835 selected.

**Steam Cloud safety.** Before and after the build, Steam's cloud log for App ID 640820 still ended with the 2026-11-17
23:02 local "Sync conflict" entries, with no "Sync Disabled" among the last six. No launch was attempted, and no save,
cloud copy or log was touched.

## Native census on the frozen final candidate

**Precondition.** The owner disabled Steam Cloud for Kingmaker: Steam's setting `cloudenabled "0"` for App ID 640820 was
written at 2026-11-18 16:34:49. The owner then launched and quit Kingmaker from Steam. A read-only check at 16:59:23
local found `Starting sync (AC Exit,Sync Disabled,)` for App ID 640820 at 16:59:16 among the last six entries.

The owner chose how to handle the conflicted owner files; I did not choose, move or edit any of them. The frozen
package (`f1fdd873…9dbd`) and harness (`c47e7f14…b089`) hashes were verified before launch.

**Session `20261118T215938Z-owner-0.33.0-inherited-template-census`** (`inherited-template-census.json`, Call of the Wild
staged): FAIL.
- Run 21:59:37–22:02:13 UTC; 131 s in game, 156 s in total.
- Loaded build: DLL `0575b4fc…7a45`, MVID `5e5ff7e5-7ca8-4539-ba02-29334dfdb8f7`.
- Active mods: Call of the Wild 1.14.4c-2.1, KingmakerGunslinger 0.0.191, KmgLoaderCompatHarness 0.1.0. Loader UMM
  0.33.0, game 2.1.7b.
- Restoration: the 95 owner saves, the owner installation and the registry were unchanged or restored. On the game
  copy's exit Steam logged `AC Exit, Sync Disabled` (17:02:13) and no conflict.

**What passed.** Every other contract and composition assertion of the fixture passed, and the census itself found:
- **Observations:** 5,769 of 5,769 expected; none unresolved or skipped.
  - Canonical: 1,826 (1,238 Summon Monster, 588 Nature's Ally).
  - Alternate: 3,943.

  | Alternate family | Children (expected = actual = observed) | Non-creature records excluded |
  |---|---|---|
  | Monster Tactician | 600 | 41 |
  | Summoner | 600 | 18 |
  | Master Summoner | 600 | 18 |
  | Occultist | 600 | 18 |
  | Naturalist | 588 | 13 |
  | Feral Hunter | 588 | 13 |
  | Preservationist | 367 | 9 |

  No child-set finding.
- **Dispositions:**

  | Disposition | Canonical | Alternate |
  |---|---|---|
  | Remove inherited | 81 | 324 |
  | Preserve the deliberate template | 957 | 1,276 |
  | No inherited template | 200 | 800 |
  | Outside Summon Monster (Nature's Ally) | 588 | 1,543 |

  Templated abilities: 2,233. Quantity removals: 58.
- **Quantity rows:**

  | | One | 1d3 | 1d4+1 |
  |---|---|---|---|
  | Canonical | 315 | 307 | 1,204 |
  | Alternate | 732 | 701 | 2,510 |

- **Branches.** Canonical: 81 with a removal on an applying donor branch, 486 cloned applications on templated
  abilities (replaced by the project template), and 1,695 with neither. Alternate: 405 removals on host branches that
  apply nothing, and 5,318 with neither. 1,744 abilities have more than one branch.
- **Donors:** 48 shapes, each recorded by identity and name. Every donor matched the independent donor contract. Three
  shapes were never in the retained census (Monster tier 1 single, tier 2 d3 and tier 3 d4+1); their signatures,
  Celestial I and Fiendish I, are recorded now and not proved.
- **Justified redundancy:** 405, all `mirrored-canonical-removal;alternate-host-applies-none`. Master Summoner 162;
  Monster Tactician, Summoner and Occultist 81 each.
- **No other findings:** no wrong-donor, branch-pairing, omitted, extra, owner, identity, ordering or
  deliberate-template finding.

**The failure.** 170 findings, all `donor-mutated` on two donors only:
- `components-array` and `components` on the native tier-one direct parents, `SummonMonsterISingle`
  (`8fd74eddd9b6c224693d9ab241f25e84`) and `SummonNaturesAllyI` (`c6147854641924442a3bb736080cfeb6`);
- reported on all 85 abilities that clone them: 30 canonical and 55 alternate tier-one children;
- no action-reference, action-type or buff-identity difference.

**Classification: diagnostic.** It is neither a DEF-05 gameplay defect nor an environment fault.
- The receipt is taken when `Configure` clones the canonical spells, before publication.
- The established publisher (`ExpandedSummoningPublication.PublishParent`, `47c6a4ab04`, 2026-08-12, "preserve native
  tier-one casts") then turns these two direct parents into menus. It keeps the original casts as the separate
  `NativeMonsterTierOneSymbol` and `NativeNaturesAllyTierOneSymbol` copies.
- The receipt therefore correctly records a real, intended, pre-existing rewrite of those two native blueprints. The
  census's contract that donors are never mutated does not account for that publication.

**Possible correction, for review.** Compare the two tier-one donors against the publisher's recorded rewrite, with the
original components unchanged, or against the preserved native copies. That is a decision about what the contract
should allow, so nothing was changed.

**The cast session** (`inherited-template-casts.json`) was not run, as instructed after a census failure.

## Tier-one donor contract and census rerun

**Owner decision (after session `20261118T215938Z`).** The census allows one exact approved tier-one publication
transition: Summon Monster I `8fd74eddd9b6c224693d9ab241f25e84` and Summon Nature's Ally I
`c6147854641924442a3bb736080cfeb6`. Every other donor must match its receipt exactly.

**Implementation (`67704e6459`).** `InheritedTierOnePublicationContract.Judge` (engine-free) is called once per recorded
carrier by the native census. A difference counts as an approved transition only on those two parents, and only when
every condition holds:
1. the receipt's components are kept as the same references, in order;
2. exactly one added component, an `AbilityVariants` named `$KMG_ExpandedSummoning_Variants`, with no second variants
   component;
3. the stable preserved child (`KMG.Summoning.Native.SM.Tier1` or `KMG.Summoning.Native.SNA.Tier1`) appears exactly once
   in that menu, and is not the parent;
4. the preserved child is semantically equal to the pre-publication donor (the `SummonSignature` render, action types
   and buff identities) and shares no component or action instance with it;
5. with the menu excluded, the live parent still equals the receipt;
6. every expected owned tier-one child appears exactly once (foreign rows and order are free);
7. the publisher's record agrees with the receipt and the live parent.

Donor results are reported once per carrier. The census requires exactly 2 approved transitions and 0 mutations. To
support this, `DonorRecord` keeps the pre-publication signature, `RecordedCarriers` lists the carriers, `CaptureReceipt`
takes an explicit component list, and `ExpandedSummoningPublication` exposes its records and the last publication.

**Controls**, in `InheritedTierOnePublicationTests` (four cases): every control named in the decision, plus the parent
standing in for the preserved child, a preserved child with different native-template applications, and a publication
record that disagrees with the receipt.

**Tests:**

| Command | Result | Time |
|---|---|---|
| `Invoke-KmgGate.ps1 -Filter 'expanded-summoning.inherited-template'` | 20 of 20 (after one missing `using` was fixed) | 19 s |
| `Invoke-KmgGate.ps1 -Filter 'summon;alternate-summoning;expanded-summoning;publication'` | 891 of 891 | 19 s |
| `Test-RuntimeScenarioPreflight.ps1` | 1205 passed | 26 s |
| `tools/validate_development_candidate.py --purpose def05-template-cleanup` | PASS | 5 s |
| `tools/validate_icon_catalog.py`, `git diff --check` | PASS, clean | |

**Memory preflight:** 7.7 of 31.7 GB physical free; commit 40.3 of 54.7 GB (74%); pagefile 1.5 GB; C: 326 GB free. This
session's idle compiler server was shut down first.

**Candidate**, built from `67704e6459ce091a7c686f24ddd09a3207b2c10f`:
- package `ccfbf1d481ee8e08cfba6eabc182e23e8eecb324901e118e3f0fcb6634d1b961`;
- DLL `e30b2bd710b2a7faae3de2d70bdbe149e491e05ab2ccdbaab6ae008259999d8a`;
- MVID `5151ff94-3d2e-4e82-bbf6-b58f64edb595`, as loaded in game;
- harness `c47e7f14…b089`;
- build 22:45:22 to 22:53:08 UTC, 891 of 891 selected.

**Session `20261118T225322Z-owner-0.33.0-inherited-template-census`**: FAIL.
- Run 22:53:22–22:55:34 UTC; 115 s in game, 132 s in total. Call of the Wild 1.14.4c-2.1, UMM 0.33.0, game 2.1.7b.
- Steam Cloud stayed "Sync Disabled".
- Saves (95), installation and registry unchanged or restored.

**What passed:**
- 5,769 of 5,769 observations: no placement findings, no child-set findings, nothing unresolved or skipped.
- Donor carriers: 48, of which 46 were unchanged.

**What failed:** the two tier-one parents, each with exactly one failed condition: `preserved-child-in-menu:0`. Conditions
1, 2, 4, 5, 6 and 7 all passed for both parents. Approved transitions: 0, against the required 2.

**Why: condition 3 contradicts released publication behaviour.**
- `PublishParent` withholds any native original whose replacement coverage is complete
  (`SummonNativeOptionCatalog.ReplacementCoverageIsComplete`).
- `ExpandedSummoningPublicationAudit` catalogues the preserved tier-one child as a native option of class `Preservation`,
  with a `Replaced` flag.
- `docs/ALTERNATE-SUMMONING-SOURCES.md` documents it as "preserved tier-I cast | registered but withheld from the menu
  (its lineage is replaced)".

So the preserved child is registered but absent from the live menu by design.

**Classification:** diagnostic (an owner-specified contract condition), not a gameplay defect.

**Possible correction, for review.** Require the preserved child in the menu exactly once when the publication's own
replacement rule keeps it, and absent (but registered, and proved semantically) when that rule withholds it. The rule
would be evaluated independently, from the catalogs rather than from the publisher's record. That changes the owner's
condition 3, so it was not made. The cast session was not run.

## Withheld preserved child: census PASS, first cast session FAIL

**Owner decision (after session `20261118T225322Z`).** Condition 3 was revised: the preserved tier-one child is always
registered and proved semantically, and its live-menu exposure follows a replacement expectation derived independently
from the catalogs (`InheritedTierOneReplacementContract`):
- complete exact replacement coverage: withheld;
- incomplete or no coverage: exactly once;
- ambiguous or malformed coverage: fail.

Menu rows are classified as preserved, expected KMG-owned, unexpected KMG-owned or foreign, and any unexpected owned row
fails. Implemented in `540556bbf0`.

**Candidate**, built from `540556bbf004f539b2c67f0de46a576b8e550388`:
- package `59b24c5a7400a11f151100b1dceba53fa793c5188edc054bd197091c0b3744c5`;
- DLL `14e32cb0976cf6372c8d9223cc1aeb1d66efe28bfaaf2d18fe08adc80e5a0d69`;
- MVID `caaba169-80f8-4f0c-b0d7-32fc076a987f`, as loaded in game;
- harness `c47e7f14…b089`;
- build 16:03:03 to 16:11:04 UTC, 894 of 894 selected.

Both sessions below ran this unchanged package with UMM 0.33.0, Call of the Wild 1.14.4c-2.1 and game 2.1.7b.
Steam Cloud stayed "Sync Disabled".

The first census launch on this package (16:11:23 UTC) was refused by the session preflight: another lab held a
runtime lock on the shared installation. No session directory was created and nothing was staged. That lock was
released before the run below.

**Session `20261119T192806Z-owner-0.33.0-inherited-template-census`: PASS.**
- Run 19:28:06–19:30:12 UTC, 111 s.
- 5,769 of 5,769 observations (1,826 canonical, 3,943 alternate), none unresolved.
- Branches: 2,262 canonical, 5,723 alternate; 1,744 abilities with more than one branch.
- Dispositions:
  - RemoveInherited 405;
  - PreserveDeliberateTemplate 2,233;
  - NoInheritedTemplate 1,000;
  - OutsideSummonMonster 2,131.
- Quantity removals: 58.
- All seven alternate families exact (600, 600, 600, 600, 588, 588, 367), zero child-set findings.
- 405 justified redundancies listed; zero findings.
- Donor carriers: 48. Of these, 46 are unchanged and 2 are approved tier-one transitions, both
  `WithholdCompleteReplacement`. 0 mutated, 0 donor findings.
- `SummonMonsterISingle` (`8fd74edd…`):
  - preserved child `KMG.Summoning.Native.SM.Tier1` (`6c7915c9…`): 0 in the menu by GUID, 0 by reference;
  - required replacement `KMG.Summoning.Ability.SM.Tier1.Dog.One` (`d5915038…`): once by GUID and once as the exact
    registered object;
  - 7 menu rows, all expected KMG rows: 0 foreign, 0 unexpected owned;
  - 29 original components kept in order; semantic proof "equal-and-independent"; publication record consistent.
- `SummonNaturesAllyI` (`c6147854…`):
  - preserved child `b5b7cf07…`: 0 in the menu by GUID, 0 by reference;
  - required replacement `KMG.Summoning.NativeOption.SNA.Tier1.Mite.One` (`a274c1f9…`): once by GUID and once by
    reference;
  - 9 menu rows, all expected KMG rows;
  - 16 original components kept in order; same proofs as above.
- Saves (95), installation and registry unchanged or restored.

**Session `20261119T193054Z-owner-0.33.0-inherited-template-casts`: FAIL, 3 of 15 assertions.**
- Run 19:30:54–19:33:09 UTC, 121 s (109.8 s in game).
- Saves (95), installation and registry unchanged or restored.

**The seven cases that passed**, each with source, exactly one debit, quantity, unit identities, native and project
templates, smites, duration, innate facts, summon marker and no lingering unit:
- `good-tier4-single-mephit` and `evil-tier4-single-mephit`: SM IV Air Mephit, one unit; one level-4 spontaneous slot
  spent; 120 s; no native or project template; 12 innate facts.
- `celestial-project-template`: SM IV Dire Wolf for a good caster; the celestial template `c3c53de0…` and smite
  `83718bdd…` only; 120 s.
- `fiendish-project-template`: the same for an evil caster; the fiendish template `031bc1b9…` and smite `0e5197ca…` only.
- `tier7-untemplated-no-inherited`: SM VII Greater Air Elemental; one level-7 slot; 120 s; no template.
- `natures-ally-preservation`: SNA II Wolf; one level-2 slot; 120 s; no template.
- `cancel-before-commit`: interrupted before commit; no debit and no unit.

**Assertions that also passed:** all nine cases observed; the four hooks installed and removed; two unrelated rows
unchanged; cleanup complete; loaded version 0.0.191; native profile restored.

**The three failures:**
1. `quantity-tier6-d4plus1-mephit`: `expression:branch0:wrong-bonus-value:0/1`.
   - The observer read `ProjectilesCount` as 0 and expected 1.
   - Three Air Mephits were summoned and the debit and duration held. Three units fit both d4 and d4+1, so the count
     decides nothing.
2. `master-summoner-tier4-mephit`: the duration observation was 420 + 0 s against an expected 0 + 0.
   - The expectation is the untouched Call of the Wild host row (`Minutes × Rank(Default)`), and the observer read
     that rank as 0.
3. `inherited-casts-donors-unmodified` required zero pre-publication receipt differences before and after the casts.
   - It saw `components-array` and `components` on `8fd74edd…` and `c6147854…`, which are the two approved tier-one
     transitions the census accepts.
   - Before and after were identical.

**Classification:** unresolved diagnostic evidence. Findings 1 and 2 share the observer's rank reader, which builds a
bare `MechanicsContext` and reads it without recalculation. Finding 3 is an assertion that was not updated for the
approved transitions. Whether the d4+1 quantity is correct in play is not established. The cast session has not been
rerun.

## Cast observer repair: census PASS and cast PASS

The owner authorized a bounded observer repair, with no gameplay change, after session `20261119T193054Z`. It was
implemented in `8ceb08ccef`. The failed cast session above is retained as it was.

**Correction 1: the actual execution context.** The bare, unrecalculated `MechanicsContext` rank reader and its
duration evaluator are removed. For each cast the observer:
1. creates the exact `AbilityData` the command uses, and one `TargetWrapper`;
2. creates `data.CreateExecutionContext(target)` and calls `Recalculate()`;
3. reads ranks only from that context.

Each case records:
- the associated blueprint;
- the `ConvertedFrom` chain;
- the spellbook or resource fact;
- caster level and `Params.RankBonus`;
- the recalculated `Default` and `ProjectilesCount` ranks.

**Correction 2: a controlled d4+1** (`InheritedControlledQuantityContract`).
- The case asserts the caster lacks Superior Summoning (`0477936c…`) and records the child's `ProjectilesCount`
  `ContextRankConfig`.
- It forces the exact caster's first d4 roll to 2 and records every roll by that caster.
- The expected count is the forced die plus the expected rank (1), never the observed output.
- Verdicts:
  - pass;
  - product failure: a forced 2 and only two units;
  - observer failure: a missing, repeated, unforced or wrongly formed roll.

**Correction 3: Master Summoner duration** (`InheritedClassRankDurationContract`).
- The structural expectation comes from the untouched host row: `Minutes`, bonus `Rank(Default)`, and a `Default`
  rank based on Summoner class level with `AsIs` progression.
- The Summoner class level must be exactly 7, so the expectation is 7 × 60 = 420 s.
- The native value is the `Default` rank of the recalculated child execution context. Neither value comes from the
  spawned unit's duration.

**Correction 4: donors across the cast window** (`InheritedDonorWindowContract`).
- Before the first cast and after the last, the census donor validation runs: 48 carriers = 46 unchanged + 2 approved
  `WithholdCompleteReplacement`, 0 mutated.
- Every carrier's current post-publication receipt is then compared field by field: component array and references,
  spawns, after-spawn arrays and actions, action types, `ApplyBuff` identities and native sequence.
- Category names are never compared.

**Negative controls (engine-free):**
- rank 0 with a forced 2 and two units fails;
- rank 1, a forced 2 and three units passes;
- three units without a captured roll fails;
- two relevant rolls fail;
- foreign or duplicate units fail;
- an unrecalculated rank 0 fails, and class level 7 with context rank 0 fails;
- rank 7 with 0 s fails, and rank 7 with 420 s passes;
- a duration on the wrong unit fails;
- unchanged approved transitions pass;
- a new component replacement, an in-place buff identity mutation and an action reorder each fail;
- approved-to-mutated fails;
- equal category names with a different underlying mutation fail.

**Focused checks:**

| Check | Result | Time |
|---|---|---|
| `Invoke-KmgGate -Filter 'expanded-summoning.inherited-template'` | 25 of 25 (3,128 registered) | 32 s |
| `-Filter 'expanded-summoning.native-profile;expanded-summoning.scenario;loader-compat;runtime-scenario;scenario-catalog'` | 18 of 18 | 2 s |
| Scenario preflight | 1,205 passed | 36 s |
| Development-candidate validation | PASS | 9 s |
| Icon catalog | PASS | — |
| `git diff --check` | clean | — |

The broader summoning tests were not needed, because no shared production helper changed. The first gate attempt
failed to compile the test assembly: the new view's fields were never assigned there. They became properties, and the
gate was rerun.

**Replacement candidate**, built from `8ceb08ccef29f1029618ee86e4797f40480ec8d8`:
- package `3ef1bce7027b0c168dddd9f3042cfc65e7a107ad7382b508075c5d08b0427ccb`;
- DLL `e0dadbc9c854768bd83711324403fc9f5d61f82c8572b9b844249e5a0f14cc35`;
- MVID `3806eb49-4581-4784-a01d-99c7cfec0659`, as loaded in game in both sessions;
- harness `c47e7f14…b089`;
- built 20:08:49–20:16:50 UTC, 897 of 897 selected; memory preflight 33.9 of 63.2 GB physical free.

Both sessions ran with UMM 0.33.0, Call of the Wild 1.14.4c-2.1 and game 2.1.7b. Steam Cloud stayed "Sync Disabled"
and no other lab held a runtime lock.

**Session `20261119T201715Z-owner-0.33.0-inherited-template-census`: PASS.**
- Run 20:17:14–20:19:21 UTC; 112 s, of which 100.8 s in game.
- 5,769 of 5,769 observations (1,826 canonical, 3,943 alternate), none unresolved.
- Branches: 2,262 canonical, 5,723 alternate; 1,744 abilities with more than one branch.
- Dispositions:
  - RemoveInherited 405;
  - PreserveDeliberateTemplate 2,233;
  - NoInheritedTemplate 1,000;
  - OutsideSummonMonster 2,131.
- Quantity removals: 58.
- All seven families exact (600, 600, 600, 600, 588, 588, 367), zero child-set findings.
- 405 justified redundancies; zero findings.
- 48 donors = 46 unchanged + 2 approved `WithholdCompleteReplacement`, 0 mutated, 0 unexpected owned rows.
- Tier-one menus: both preserved children are absent (0 by GUID, 0 by reference). The Dog (`d5915038…`) and the Mite
  (`a274c1f9…`) each appear once as their exact registered objects.
- Original components kept in order; semantic proof "equal-and-independent"; publication record consistent.
- Saves (95), installation and registry unchanged or restored.

**Session `20261119T201938Z-owner-0.33.0-inherited-template-casts`: PASS, 15 of 15 assertions.**
- Run 20:19:38–20:21:41 UTC; 108 s, of which 97.0 s in game.
- Same package, DLL and MVID as the census.
- All nine cases passed. Each completed cast spent exactly one source and produced the expected units, with the
  expected templates, smites, durations, innate facts and summon marker, and left no lingering unit.
- The cancellation spent nothing and summoned nothing.
- Every context is an `AbilityExecutionContext`, recalculated for the caster, with the child as associated blueprint and
  the parent as its `ConvertedFrom`.
- Hooks were removed, the two unrelated rows were unchanged, cleanup completed and the native profile was restored.
- Saves (95), installation and registry unchanged or restored.

**Controlled quantity** (`quantity-tier6-d4plus1-mephit`, Summon Monster VI, level-20 Sorcerer):
- The caster does not hold Superior Summoning.
- The child's `ProjectilesCount` config: base `FeatureListRanks` over Superior Summoning, progression
  `OnePlusDivStep`, step 1, no min or max.
- Recalculated `ProjectilesCount` = 1.
- Exactly one relevant roll: 1d4 by the caster, forced to 2. The caster's only other rolls were seven zero-dice
  evaluations.
- Expected count 2 + 1 = 3. Actual: 3 distinct live Air Mephits, each 120 s, one level-6 slot spent. Verdict: Pass.

**Master Summoner** (`master-summoner-tier4-mephit`, child `1e125d75…` from `MasterSummonerSummon4Base`, resource fact
`5554be32…`):
- Structural: the host row `60da9f1b…` is `Minutes`, `Zero` dice, bonus `Rank(Default)`; its `Default` config is
  `ClassLevel` over the Summoner class `0f4c4ada…`, `AsIs`.
- Summoner class level 7, so the expected rank is 7 and the expected duration 420 s.
- Native: the child's own `Default` config is identical; the recalculated `Default` rank is 7 (caster level 7,
  `RankBonus` 0).
- Observed: one duration of 420 + 0 s bound to the one spawned unit; one resource use spent (9 to 8).

**Donor window:**
- Before and after the casts: 48 carriers, 46 unchanged, 2 approved `WithholdCompleteReplacement`, 0 mutated, no
  findings; 4,073 live adapter identities counted as KMG-owned.
- Receipt differences across the casts: none.
- Both approved transitions are identical before and after:
  - `SummonMonsterISingle`: preserved child 0 / 0 in the menu, Dog 1 / 1, 7 menu rows;
  - `SummonNaturesAllyI`: preserved child 0 / 0, Mite 1 / 1, 9 menu rows.
  - Each has pre-clone receipt differences `components-array`, `components` only, and none excluding the menu.

## Status

S5 T1 / DEF-05 is ready for principal review.

## Remaining

1. Release obligation: the next release validator must account for the removed creature list. The v0.0.191 historical
   validator is unchanged.

## Limits

- The quantity is proved for the controlled roll only: one d4 forced to 2 gave 2 + 1 = 3. An ordinary, unforced roll's
  distribution is not claimed beyond that.
- The retained Sprint 4 baseline lacks the old donor identities, so the donor window is compared from this product's
  own receipts.
- The census is graph evidence; it does not say what a cast unit carries.
- The donor table in the domain tests is the retained observation of one installation (game 2.1.7b as staged). A
  different native library needs the census, not the table.
- Units summoned before this change keep their buffs.
