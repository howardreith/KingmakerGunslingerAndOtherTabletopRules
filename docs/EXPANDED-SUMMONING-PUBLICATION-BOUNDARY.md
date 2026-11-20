# The Expanded Summoning publication check: 38 choices it could count and not name (INT-01)

Status: **development change on a review branch, awaiting review. Not merged, not released.** Branch
`codex/int01-summoning-publication-attribution`, stacked on draft PR #173 at its accepted head `6f1590d31d` (which is
stacked on #172, #171, #170 and #169). The published v0.0.191 does not contain it. Nothing here is a release
qualification.

This is one bounded cut of the Gunslinger remediation epic of 2026-11-15: story S3 T1, inventory record INT-01.
Current status is kept in `planning/GUNSLINGER-REMEDIATION-STATUS.md`. It repairs a diagnostic. Publication, gameplay
and released identities are unchanged.

## Disposition

**Observer defect, corrected. Publication was right and was not changed.**

`feature-module-expanded-summoning-publication-gate` reported `unclassified=38` whenever Races Unleashed was
installed. The 38 are the project's own generated Spider Summoner rows: one for each of Races Unleashed's four spider
species at every tier above the species' base tier, in both spell families. Each is under its declared parent, once,
behind the Spider Summoner feat. The check sorted choices by the prefix of their internal name. It was written on
2026-09-23, and the spider rows were added on 2026-11-09 under a prefix it had no class for, so it counted them as
unclassified against an expectation of none.

All 38 of the originally reported entries are accounted for: every one is a required, correctly placed project
choice. None was unexpected, duplicated, missing or unreadable.

## The finding as first reported

| | |
|---|---|
| Where | `scripts/fixtures/feature-module-settings/README.md`, commits `c5047c292d`, `7582b4b819` and `9b144dcee7` (2026-11-12 and 13); `KNOWN-ISSUES.md`; `docs/BUILD-AND-RELEASE.md` |
| Scenario | `observe-feature-module-settings`, every module on, no staging |
| Route | The guarded launcher on the owner's installation |
| Other mods | "the same eleven installed mods", of which the record names Call of the Wild, Races Unleashed, Tweak or Treat and Craft Magic Items "and others" |
| Baseline | A development candidate built from released master `a3912f569ac5` (v0.0.185; `9d2938483d11` before the history rewrite of 2026-11-13) |
| Candidate | `468391fc06a3` (a commit of that campaign; the identifier no longer names an object in the repository) |
| Summoning result | `unclassified=38` against an expected 0, on both |
| Brown-Fur result | `contract:True/identities:25/published:True/selectorRef:1/selectorGuid:1/foreign:14/unique:True/index:6/firstCombined:7/knownCombined:5`, identical on both |

What could not be recovered: the raw result files of those two runs. They are not under the lab's
`runtime-evidence`, `compatibility-state` or `runtime-backups` folders, and nothing in the repository gives another
location. The record does not list the eleven mods, their versions, the package hashes or the other totals
(`placements`, `nativeOptions`, `nativeVariants`). So the original population is known only as a count.

## What was reproduced

Three configurations in the disposable game copy, with the test-only loader harness beside the product. The harness
gained one read-only step, `kmgVariantCensus`, which lists every entry under named variant parents from the loaded
library. It does not use the product's observers and changes nothing, so it describes a published package the same
way as a candidate, and its identity is recorded apart from the product's.

| Configuration | Product | Result of the summoning check | Entries under the 18 parents |
|---|---|---|---|
| Races Unleashed 1.0.11 only | published v0.0.191 | `unclassified=38`; FAIL | 1,254 |
| Twelve of the owner's mods (lab snapshot) | published v0.0.191 | `unclassified=38`; FAIL | 1,254, the same identities in the same order |
| Races Unleashed only, module off | published v0.0.191 | PASS | 52 |

The twelve-mod run also reproduced the Brown-Fur measurement character for character as far as the original record
quotes it. That is two of two recorded values matching, on a newer release and on the lab's snapshot of the owner's
mods. It is not the original installation: the original named eleven mods and this stack has twelve, and the
original's mod list was not recorded. It is a wider configuration that shows the same population, not the original
matched comparison.

Races Unleashed needs nothing else to load, and with it alone the count is already 38. No other mod of the twelve
adds or removes a choice under the eighteen parents.

## The 1,254 choices, by identity

With the module on and Races Unleashed installed:

| Class | Count | Whose | Contract that establishes it |
|---|---|---|---|
| Logical placements (`KMG.Summoning.Ability.*`) | 1,179 | project, required | `SummonVisibilityCatalog` over `ExpandedSummoningCatalog.GenerateVariants` (1,188 registered less the 9 withheld Stirge placements) |
| Native expansion wrappers (`KMG.Summoning.NativeOption.*`) | 29 | project, required | `SummonNativeExpansionCatalog` |
| Generated spider rows (`KMG.Summoning.Spider.*`) | 38 | project, required while Races Unleashed supplies the source row | `SpiderSummonerCatalog.Generated`, published by `SpiderSummonerIntegration` through `ExpandedSummoningPublisher.AddLateOwnedRow` |
| Races Unleashed's own rows | 8 | Races Unleashed, kept | `SpiderSummonerCatalog.RowIdentity`; the publisher keeps every choice it does not own |
| The game's own options | 0 | the game; none of its 46 remains in a published list | `SummonNativeOptionCatalog` and its replacement coverage |
| Tier-one preservation children | 0 | project; not offered | the same coverage rule |

With the module off the eighteen parents hold 52 choices: the game's 46 options at tiers II to IX and six Races
Unleashed rows (its two tier-I rows are not under a parent, because a tier-I summon has no choice list until the
module publishes one). Nothing of the project's is offered.

## The ledger of the 38

Every row below was read on the published v0.0.191 with Races Unleashed only, and again, identically, on the twelve-mod
stack and on the candidate. For each: requested and active module state enabled; expected class "generated Spider
Summoner row, required" from `SpiderSummonerCatalog.Generated`; found once, under the declared parent, linked to that
parent, shown only to a caster with the Spider Summoner feat. Old classification: unclassified (no name prefix
matched). Disposition: **required, correctly published**.

| Parent | Position | Identity | Symbol (`KMG.Summoning.Spider.` +) | Shown as |
|---|---|---|---|---|
| Summon Monster II | 19 | `277840a48dfa5d1f82e52e7b2f8d9c3a` | `SM.Tier2.CrabSpider.OneD3` | Spider, Giant Crab (1d3) |
| Summon Monster III | 32 | `5571183ec78e5fd9a9853a5c1aa83943` | `SM.Tier3.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Monster IV | 60 | `08873d633ac15f50bfa88b5dd46dd154` | `SM.Tier4.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Monster V | 41 | `6aa7c191c654573a906379201538ebe6` | `SM.Tier5.GiantBlackWidow.OneD3` | Spider, Giant Black Widow (1d3) |
| Summon Monster V | 74 | `6bdb9826b4055b3bbe308303e7f6c415` | `SM.Tier5.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Monster VI | 29 | `03159c2852ec52afad5d1ba602d614b3` | `SM.Tier6.OgreSpider.OneD3` | Spider, Ogre (1d3) |
| Summon Monster VI | 87 | `ce47bbab03d15bc1a7057fcb93249d87` | `SM.Tier6.GiantBlackWidow.OneD4PlusOne` | Spider, Giant Black Widow (1d4+1) |
| Summon Monster VI | 88 | `3b4ff1b7e081509aa675f50e1e236ade` | `SM.Tier6.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Monster VII | 98 | `efa9d8fe41b05ef3a8fda462b7b3f195` | `SM.Tier7.GiantBlackWidow.OneD4PlusOne` | Spider, Giant Black Widow (1d4+1) |
| Summon Monster VII | 99 | `2b7a7f00a36759a189cdfd9a9c51dc54` | `SM.Tier7.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Monster VII | 100 | `0b1bc3b604a45b45b9c14f83fc7f2f21` | `SM.Tier7.OgreSpider.OneD4PlusOne` | Spider, Ogre (1d4+1) |
| Summon Monster VIII | 20 | `96c74600a94d5bc89cabbbaca109b8f3` | `SM.Tier8.GiantTarantula.OneD3` | Spider, Giant Tarantula (1d3) |
| Summon Monster VIII | 104 | `110accab5e5f54519f38b551f967ea24` | `SM.Tier8.GiantBlackWidow.OneD4PlusOne` | Spider, Giant Black Widow (1d4+1) |
| Summon Monster VIII | 105 | `d8261c0c5ef553f6b9d0e5da3d392503` | `SM.Tier8.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Monster VIII | 106 | `743fa2f980195c82b8aa282d6400fbed` | `SM.Tier8.OgreSpider.OneD4PlusOne` | Spider, Ogre (1d4+1) |
| Summon Monster IX | 107 | `21d9f87663c653659ce89d140d6bf6d7` | `SM.Tier9.GiantBlackWidow.OneD4PlusOne` | Spider, Giant Black Widow (1d4+1) |
| Summon Monster IX | 108 | `3be4b03f39cd5977a9434a548c7f5f4d` | `SM.Tier9.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Monster IX | 109 | `0dc34b3055f95287a064e18ddc1928d8` | `SM.Tier9.GiantTarantula.OneD4PlusOne` | Spider, Giant Tarantula (1d4+1) |
| Summon Monster IX | 110 | `b35e80bff70555aca40046bdae1cdc79` | `SM.Tier9.OgreSpider.OneD4PlusOne` | Spider, Ogre (1d4+1) |
| Summon Nature's Ally II | 19 | `fb5b55408e005e2e9aefccfe41e70345` | `SNA.Tier2.CrabSpider.OneD3` | Spider, Giant Crab (1d3) |
| Summon Nature's Ally III | 31 | `70cf1491c5d45cf28758604c150372ec` | `SNA.Tier3.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Nature's Ally IV | 61 | `016c2ba1222750d69282fe92a9aabf52` | `SNA.Tier4.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Nature's Ally V | 40 | `d9145ca9798b519fa1333789c5c281ed` | `SNA.Tier5.GiantBlackWidow.OneD3` | Spider, Giant Black Widow (1d3) |
| Summon Nature's Ally V | 72 | `17c18984c66555e89ada63f5960892d9` | `SNA.Tier5.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Nature's Ally VI | 24 | `61db16d7c6975856844a48de75c388e5` | `SNA.Tier6.OgreSpider.OneD3` | Spider, Ogre (1d3) |
| Summon Nature's Ally VI | 81 | `031a859bb375588a94721fadc9fa4f47` | `SNA.Tier6.GiantBlackWidow.OneD4PlusOne` | Spider, Giant Black Widow (1d4+1) |
| Summon Nature's Ally VI | 82 | `6e5a494853075a199f6e4e9b6ca42619` | `SNA.Tier6.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Nature's Ally VII | 93 | `689521004da25ec6b6e319dfc23ea9a5` | `SNA.Tier7.GiantBlackWidow.OneD4PlusOne` | Spider, Giant Black Widow (1d4+1) |
| Summon Nature's Ally VII | 94 | `70ddc380a7145bf1a0862ed473c62afe` | `SNA.Tier7.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Nature's Ally VII | 95 | `0640f8b99b2b585887266771e0aaa443` | `SNA.Tier7.OgreSpider.OneD4PlusOne` | Spider, Ogre (1d4+1) |
| Summon Nature's Ally VIII | 18 | `822a7b05d93a5056ac077116fd658719` | `SNA.Tier8.GiantTarantula.OneD3` | Spider, Giant Tarantula (1d3) |
| Summon Nature's Ally VIII | 97 | `9c05b38161ee50c588c7cafd5addeea0` | `SNA.Tier8.GiantBlackWidow.OneD4PlusOne` | Spider, Giant Black Widow (1d4+1) |
| Summon Nature's Ally VIII | 98 | `b4dfab04f2d557a98d0e3844bab0b5c7` | `SNA.Tier8.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Nature's Ally VIII | 99 | `3945fac2d739579994a91c3527b7d03a` | `SNA.Tier8.OgreSpider.OneD4PlusOne` | Spider, Ogre (1d4+1) |
| Summon Nature's Ally IX | 98 | `a6374a687ee451dbb82cd9c0e6d4749e` | `SNA.Tier9.GiantBlackWidow.OneD4PlusOne` | Spider, Giant Black Widow (1d4+1) |
| Summon Nature's Ally IX | 99 | `a284ea4f672659aab2d52132821eea0d` | `SNA.Tier9.CrabSpider.OneD4PlusOne` | Spider, Giant Crab (1d4+1) |
| Summon Nature's Ally IX | 100 | `e0eb89183d9f5fc0b11f5b3d7db4fd5f` | `SNA.Tier9.GiantTarantula.OneD4PlusOne` | Spider, Giant Tarantula (1d4+1) |
| Summon Nature's Ally IX | 101 | `f86a753d5e5b56bd95a654764e7f9f3f` | `SNA.Tier9.OgreSpider.OneD4PlusOne` | Spider, Ogre (1d4+1) |

Spell family and tier are in the symbol (`SM` or `SNA`, then the tier). The eight Races Unleashed rows the old check
counted as "native" are attributed too: its Giant Crab Spider at tier I, Giant Black Widow at IV, Ogre Spider at V
and Giant Tarantula at VII, in each family, each at its own base tier.

## Root cause

The boundary reader (`RuntimeTestRunner.ExpandedSummoningBoundary.cs`, 2026-09-23, `9134c477b3`) classified each
choice by internal-name prefix: `KMG_Summoning_Ability_`, `KMG_Summoning_NativeOption_`, `KMG_Summoning_Native_`,
anything else beginning `KMG_Summoning_` as unclassified, and everything else as native. It kept seven totals and
nothing about any single choice.

The Spider Summoner integration (2026-11-09, `b423d8f692`, released in v0.0.182) added a fourth kind of project
choice, named `KMG_Summoning_Spider_*`, published late and only when Races Unleashed is installed. The reader was not
given a class for it. With Races Unleashed absent nothing changes, which is why the lab's single-mod runs of this
check always passed.

So the failure says nothing about publication. It also could not have said anything useful if publication had been
wrong: a wrong row and a right row with an unfamiliar prefix produced the same number.

## What changed

Publication code is untouched (`ExpandedSummoningPublication.cs`, `SpiderSummonerIntegration.cs` and the catalogs
are byte-identical to the base).

- **`Summoning/ExpandedSummoningPublicationAudit.cs`** (new, no engine types). The contract is built from the
  catalogs the publisher reads and from the identity manifest: placements, native expansion wrappers, the two
  preservation children, the generated spider rows, the game's 48 options with the publisher's own replacement
  coverage rule, and Races Unleashed's eight row identities. Each choice read is classified by identity and given a
  verdict and a reason. A name is used for one thing only: to notice a choice that carries the project's prefix
  without a declared identity, which fails.
- **What is required.** Placements the visibility catalog publishes, and native expansion wrappers, when the module
  is requested on. Spider rows when the module is requested on and the library holds the Spider Summoner feature and
  that species' Races Unleashed row in that family. That is read from the library's identities, not from the summon
  parents and not from what the integration reports. If Races Unleashed is present and a row is not published, the
  row is reported missing with the integration's own outcome beside it.
- **What fails.** A required choice that is absent; a project choice under another parent, in a disabled state,
  withheld by the visibility catalog, or without its source; a project identity that is not a publication choice; a
  project-named choice with no declared identity; a native option the publisher replaces, still offered; an identity
  twice in a published list; a position or parent that cannot be read.
- **What is reported and not constrained.** Choices that are not the project's: the game's options, Races Unleashed's
  rows, and anything declared by no catalog. The last kind is listed as *not attributed*, with the reason that the
  publisher keeps every choice it does not own and that its owner was not established. It is not called acceptable
  and it is not called the project's. This is the old `nativeVariants` rule, now itemized.
- **Results.** The totals keep their seven keys in the same order, with `spiderRows` and `unreadable` after them.
  Beside the totals are five assertions: required choices, declared boundary, duplicates, readability, and the
  ledger. The ledger file `expanded-summoning-publication-ledger.json` holds every choice read, each mismatch in full
  (parent identity and name, family, tier, position, occurrences, requested and active state, class, contract,
  verdict, reason) and every unattributed choice.
- **Reading.** Each parent is read position by position. A null or unreadable position is kept with its reason and
  the positions around it are still judged; a parent that cannot be read is reported and the other seventeen are
  still judged. Before, a null position was counted as a native choice, and a parent that could not be read, or one
  carrying two choice lists, ended the whole observation with an exception.
- **Both callers** use it: `observe-feature-module-settings` (the reported check) and
  `observe-expanded-summoning-module-boundary`. The projected-menu scenario reads the same totals.

The three claims stay separate. This check is about *choices offered under a parent*. It does not establish that a
blueprint is registered when it is not offered, and it does not establish that any choice can be cast.

## Focused tests

Gate filter
`summoning-publication-boundary;expanded-summoning.sprint9-module-boundary;summon-spider-summoner;summon-menu-order.spider;expanded-summoning.runtime-publication;feature-modules.publication-plans`:
production compiled, 23 of 3,057 selected, 0 failures. Eight tests are new.

They run the production contract, reader and audit. The lists they judge are not produced by that code: they are
what the game held under the eighteen parents with the published v0.0.191 and Races Unleashed, as the harness listed
it (`tests/fixtures/summoning-publication/races-unleashed-parent-census.json`). Each test then changes that list in
one stated way.

| Test (`summoning-publication-boundary.` +) | What it shows |
|---|---|
| `recorded-provider-lists-are-attributed-choice-by-choice` | The recorded module-on lists conform; the 38 are each a required spider row with its contract named; a placement is recognized as required; the recorded module-off lists conform with nothing published |
| `missing-required-choice-is-reported-with-its-place` | A removed placement and a removed spider row are each reported with identity, parent, class and reason; the other results do not move |
| `same-count-replacement-and-duplicate-stay-detectable` | A withheld Stirge placement in place of a required one, and a second copy of one choice in place of another, both leave the placement count unchanged and both fail |
| `native-and-provider-choices-keep-their-own-classification` | The game's option and Races Unleashed's row keep their own classes; a replaced native option left in a published list fails; a provider row under another parent is shown and not called the project's; without Races Unleashed no spider row is required and none may be offered |
| `unknown-and-wrongly-attributed-choices-stay-visible` | Another mod's choice is listed as not attributed; a project-named choice with no identity, a project identity that is not a publication choice, and a placement under the wrong parent each fail and are named |
| `project-choices-in-a-disabled-state-fail-the-boundary` | Everything published, judged against a request for the module off, fails; so does one row left behind; so does the module requested on with nothing published |
| `malformed-entries-do-not-suppress-readable-ones` | A null, an entry whose identity cannot be read, an entry with no identity and an unavailable parent are each reported; every readable choice beside them is still judged; nothing is called missing because something else could not be read |
| `totals-keep-their-established-keys-and-follow-the-catalogs` | The seven keys and their order; the spider expectation follows the library row by row |

Seven deliberate faults put into the audit were each caught by at least one of these tests: accepting a choice on
its name, never reporting a duplicate, requiring spider rows whatever the library holds, allowing a project choice
with the module off, dropping a null position, allowing a withheld placement, and not reporting a missing choice.

## Native results

Five sessions in the disposable game copy (Unity Mod Manager 0.33.0), all with the same harness. The baseline is
the unchanged published v0.0.191 package; nothing was rebuilt for it. The candidate is one fresh build of this
branch.

| Product | Configuration | Session | Summoning check | Session result |
|---|---|---|---|---|
| published v0.0.191 | Races Unleashed only | `20261116T181244Z` | module on: FAIL, `unclassified=38`; module off: PASS | FAIL (that check) |
| published v0.0.191 | twelve-mod stack | `20261116T181751Z` | FAIL, `unclassified=38` | FAIL (that check and Brown-Fur) |
| candidate `4856b7084b5c` | Races Unleashed only | `20261116T183715Z` | module on: PASS; module off: PASS | PASS, 52 of 52 in each observation |
| candidate `4856b7084b5c` | no other mod | `20261116T184138Z` | PASS | PASS, 52 of 52 |
| candidate `4856b7084b5c` | twelve-mod stack | `20261116T184404Z` | PASS | FAIL, 51 of 52 (Brown-Fur only; INT-02) |

The candidate's totals:

| Configuration | Totals |
|---|---|
| Races Unleashed, module on | `publishedParents=18;placements=1179;nativeOptions=29;preservation=0;unclassified=0;placementsExact=True;nativeVariants=8;spiderRows=38;unreadable=0` |
| Races Unleashed, module off | `publishedParents=0;placements=0;nativeOptions=0;preservation=0;unclassified=0;placementsExact=True;nativeVariants=52;spiderRows=0;unreadable=0` |
| no other mod, module on | `publishedParents=18;placements=1179;nativeOptions=29;preservation=0;unclassified=0;placementsExact=True;nativeVariants=0;spiderRows=0;unreadable=0` |
| twelve-mod stack, module on | the same as Races Unleashed, module on |

The candidate's individual results, in each observation: required choices present (1,246 required with Races
Unleashed, 1,208 without, 0 with the module off; none missing, none unchecked); declared boundary, 0 unexpected;
duplicates, 0; readability, 0 unreadable parents and positions; ledger written, with 0 choices not attributed. With
the module off the ledger itemizes the 52 as 46 of the game's options and 6 Races Unleashed rows. The integration's
own report was `integrated;repaired=8;generated=38;problems=` with Races Unleashed, `registered-module-off` with the
module off and `absent:spider-summoner-feature-not-installed` without it.

Three comparisons that do not depend on the corrected observer:

- **The candidate publishes what the baseline publishes.** The harness listing under the eighteen parents is
  identical between baseline and candidate, position by position: 1,254 choices with Races Unleashed and the module
  on, 52 with it off, 1,254 on the twelve-mod stack. The observer change did not alter publication.
- **The product's ledger agrees with the harness.** In each of the four candidate observations the ledger's
  positions, identities and names equal the harness listing taken in an adjacent launch of the same configuration.
- **The module switch is real.** The module was turned off through the loader's own panel and read in a fresh
  process; the harness then found none of the project's choices under any parent, and tier I with no choice list.

The disabled boundary was exercised with Races Unleashed present, which is the case the changed expectation bears
on. On the baseline the module-off check already passed, because the old reader did not constrain what it called
native; the candidate passes it with each of the 52 attributed.

All five sessions reported the owner's 95 save files and installation unchanged and the registry restored. No save
was loaded: the scenario runs from the main menu. No Kingmaker process was left running.

Validation took about 25 minutes: two baseline sessions (246 s and 133 s), the focused gate twice (18 s and 19 s),
the seven mutation runs (about 130 s), one candidate build (435 s) and three candidate sessions (544 s). That is
inside the 30-minute ceiling and over the 15-minute target; the second baseline, on the twelve-mod stack, is what
took it over, and it is the run that matched the original report's second value.

## Identities

| Thing | Identity |
|---|---|
| Branch and base | `codex/int01-summoning-publication-attribution` on `6f1590d31d31588b03726484c6ad6da4a2e88da1` (PR #173's accepted head) |
| Candidate commit | `4856b7084b5c08d804f507a00fb7b981c69248e4` |
| Candidate package | `KingmakerGunslinger-0.0.191-development-int01-summoning-publication-4856b7084b5c.zip`, SHA-256 `97ac7308e4cd98c59ca587acb96d3524c7889b6b0dcfb5dc960e630914422fa0` |
| Candidate DLL | SHA-256 `d5c0d98cf647715c5dc2ed3fbcaeee4f724d5828fad95f67f604f45b9a0faf88` |
| Baseline package | published v0.0.191, SHA-256 `c09ca6255feefe6c547d051ab3cf3ead8bc918402d7488f077efa0ac5d2e7fbb`; DLL `7c3b0324f40e32b0a476c24e9e66a63a2ecefd8908dc3902bed8ec81a5d87287` |
| Harness | `KmgLoaderCompatHarness.dll`, SHA-256 `56e8dd08844460582e9d2dde8646e582a437efd7703db887497aa349b593c5c0`, built from `tools/LoaderCompatHarness` at `2546491e91` (adds `kmgVariantCensus`); the same file in all five sessions |
| Races Unleashed | 1.0.11, DLL SHA-256 `6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0` |
| Twelve-mod stack | BagOfTricks 1.16.4, BetterVendors 2.0.8, CallOfTheWild 1.14.4c-2.1, CheatMenu 1.2.3, CraftMagicItems 2.1.0, EddicKingmakerRespec 1.0, KingmakerBugfixes 1.3.3, ProperFlanking2 1.2.3-2.1, RacesUnleashed 1.0.11, SkipIntro 1.3.1, TweakOrTreat 1.1.0, ZFavoredClass 1.3.1 |
| Module settings | every module on; for the disabled runs Expanded Summoning off and the rest on |
| Session `summoning-publication-boundary.json` | SHA-256 `81944f391a4a394858d1a43489e586b01c93d9168380f278a6b51f76e7c8bf54` |
| Session `summoning-publication-boundary-alone.json` | SHA-256 `5c1963090a854fbace88c80b7271daacc3083afc5641ee766ef5a5e2280b4995` |
| Session `summoning-publication-boundary-owner-stack.json` | SHA-256 `9ccc3927809d5722a654f6bd6a61772c74626e03ed04d63f9d487eacad4df7dc` |
| Recorded lists used by the tests | `tests/fixtures/summoning-publication/races-unleashed-parent-census.json`, SHA-256 `ef540df1868ba5be731178b2290d61bd538f131fd9b7f03c253b07ffde13156f`, from session `20261116T181244Z` |
| Game | Pathfinder: Kingmaker 2.1.7b, disposable copy `game-copies/loader-compat`, loader profile `owner-0.33.0` |

## Limits and what is not established

- The original report's raw results and mod list were not recovered. The 38 of that report are identified with the
  38 here because the count matches, the Brown-Fur value recorded beside it matches, the spider rows are the only
  project choices the old reader had no class for, and Races Unleashed was among the mods the report names. That is
  an identification by evidence, not a re-reading of the original files.
- The original baseline was v0.0.185; the baseline here is the published v0.0.191. v0.0.185 was not rebuilt.
- The owner's installation was not used. The twelve-mod stack is the lab's snapshot of twelve of the owner's mods.
- No failing boundary was produced in the game: no missing, misplaced, duplicated or unreadable choice exists there
  to observe. Those reports are covered by the tests.
- Races Unleashed 1.0.11 is the only version read. What the integration does with another version's rows was not
  observed; the check would report such rows missing with the integration's reason.
- No other mod that adds summon choices was available, so no choice was listed as not attributed in the game.
- A spider row was not cast, and the Spider Summoner feat was not taken, in any run here. This check is about what
  is offered under a parent.
- The complete domain suite, the compatibility matrix and the sixteen module states were not run.
- `observe-expanded-summoning-module-boundary` and the projected-menu scenario share the corrected code and were not
  run in the game; the reported check, in `observe-feature-module-settings`, was.
- In the ledger assertion's text the key `racesUnleashedRows` appears twice: first the source rows the library holds
  (8), then the rows found under the parents (8 with the module on, 6 with it off). The ledger file keeps the two
  apart. The text was left as built so that the evidence above belongs to one candidate.
- `feature-module-brown-fur-publication-gate` still fails on the twelve-mod stack. That is INT-02, a separate record
  with its own cause to find; nothing here explains it.
