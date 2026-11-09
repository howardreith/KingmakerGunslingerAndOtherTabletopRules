# Spider Summoner integration: status

**Implemented with verification pending.** The full gameplay scope is implemented and the delivered
candidate passed its focused and native checks (139 PASS, 0 FAIL), but named acceptance observations
remain open (table below). This is not mission completion and not release qualification. No merge,
release or permanent installation has been performed.

Baseline and its original result, unchanged: [BASELINE.md](BASELINE.md), including the `FAIL, 70 PASS / 1 FAIL`
result on candidate `efc75b8cc0b0`.

## Candidate and run

| Item | Value |
| --- | --- |
| Source | `ae714f04965bd338f7d1da402566d4745b799ce5`, v0.0.181 development, branch `codex/races-unleashed-summoning` |
| Package SHA-256 | `278540097efdafef9647c083f327eff0fe2bddb12a85805eeb122ca5200bec9b` |
| DLL SHA-256 | `511c1d3a445f38e21b63067889fafb9dbc95698a2813bdd10557be5b44544e4d` |
| MVID | `dc2771c2-05ab-4afc-83ae-46fae8be9a4a` |
| Purpose | Development candidate `races-unleashed-summoning`; never a release |
| Run | `20261109T2245072861516Z-cacb7251beea41abb11f16fe8d298a4c`, phase `summon-cleric-alignment`, **PASS, 139 PASS / 0 FAIL** |
| Profile | The current DATA combined installation. Races Unleashed 1.0.11, DLL `6d18168c...4cb0` |
| Restoration | `verified`; live v0.0.140, DLL `00EF6757...66FD`, `Params.xml` `E44F4495...30BB` |

Documentation commits after `ae714f04965b` do not change the candidate. Raw evidence stays private under
`runtime-evidence\20261109T2245072625244Z-disposable-expanded-summoning-sprint32-native-review`.

An earlier candidate on this branch is retained, not replaced: `3e27c5809a9d`, package `461dc4f1...9953`, DLL
`a9aac03e...2f1f`, MVID `c52b6ac5-b65f-47ce-9205-37995f5be19b`, run directory `20261109T2231457729890Z-...`: **FAIL, 132 PASS / 3 FAIL.**
All 38 generated rows were withheld by this mission's own quantity check, which had assumed a fixed dice form,
so the two quantity casts were not offered. The rows' originals and every Summon Monster effect passed there too.

## Version reconciliation (RU-01)

The installed `Info.json` reads 1.0.11 and its DLL hashes to the recorded `6d18168c...4cb0`. Nexus advertises
1.1.1, which is not installed here. Nothing was replaced. Behavior was established from a read-only decompile
of the installed DLL and the loaded graphs, not from a version label.

## What the loaded game showed

- The Spider Summoner feature has only a Drow prerequisite. **Races Unleashed implements no DC benefit**, so this
  integration owns the +2 and there is no second implementation to stack with.
- Four species copy the native Giant Spider unit, 7 HD, so all use the Mid template band. They keep the donor's
  `HuntingSpiderPoison` and add their own copied poison. The donor has no web, so **a web DC is inapplicable to
  the four Races Unleashed species**; the ordinary Giant Spider's web is the only web in scope.
- The two tier I Crab rows inherit Expanded Summoning's own variants component because Races Unleashed copies
  the parent after publication. The repair removes it from the private row.
- Native quantity rows count as one die plus a rank bonus, the hook Superior Summoning uses, and from tier VII
  branch into two spawns. Generated rows clone that donor, so the bonus and branches are preserved.

## Implementation map

| Concern | Where |
| --- | --- |
| Species, 46-placement matrix, exact DC blueprint set | `Summoning/SpiderSummonerCatalog.cs` |
| Eight exact row contracts: known input and repaired result | `Summoning/SummonForeignRowContracts.cs` |
| In-place repair, generated rows, publication | `Blueprints/SpiderSummonerIntegration.cs` |
| Per-creature benefit action and DC owner | `Summoning/SpiderSummonerBenefit.cs` |
| Conversion admission by spell family | `Summoning/DruidSummonConversionVariantPatch.cs` |
| Late hook, before the existing reorder | `Summoning/AlternateSummoningCoordinator.cs` |

**Who owns the +2.** One hidden marker buff, granted once per eligible creature by the after-spawn action, adds
2 inside `RuleCalculateAbilityParams` for exact poison and web blueprints only. Native `ContextActionSpawnMonster`
runs after-spawn actions per creature, so each extra creature is covered. Each calculation is a new rule, so nothing
accumulates across saves, recalculation or reload. Existing summons from before an upgrade receive no marker.

Generated identities are 38 append-only manifest entries, plus one for the marker. The art registry receipt was
refreshed for that exact append, with no icon or catalog entry changed.

**Decisions recorded for review.** Original rows keep their Races Unleashed names (for example "Summon Monster V
(Ogre Spider)") while generated rows use family-first labels such as "Spider, Ogre (1d3)". Summon Nature's Ally
spiders follow the existing caster-alignment action and receive no Summon Monster template. The cleric rule stays
Summon Monster only.

## Acceptance status

Candidate for every native entry below: `ae714f04965b`. Focused evidence is the named domain tests; they are not
native observations.

| ID | Implementation | Focused evidence | Native evidence | Candidate | Remaining work |
| --- | --- | --- | --- | --- | --- |
| RU-01 | Done | Contract and matrix tests | All 8 rows repaired from their exact inputs, 0 problems. `repaired=8;generated=38` | `ae714f04965b` | None |
| RU-02 | Done | Input and shape negatives | Held row refused after feature loss, no spend or spawn. Rows gated in census. Ordinary Giant Spider cast without the feat | `ae714f04965b` | Natural Drow feat acquisition (the fixture grants the exact feature) |
| RU-03 | Done | Matrix tests | Four Summon Monster originals cast, each exactly the named unit. Repaired Ogre under evil and non-evil casters | `ae714f04965b` | **Four Summon Nature's Ally original casts** |
| RU-04 | Done | Every matrix cell; quantity expression | 46 of 46 placements in census. 1d4+1 and 1d3 Summon Monster casts | `ae714f04965b` | Representative Summon Nature's Ally quantity casts |
| RU-05 | Done (donor rank preserved) | Quantity expression rejects a fixed bonus | None | none | **Superior and Augment Summoning native cast** |
| RU-06 | Done | Selection and band policy | Celestial on all four species; fiendish on the Ogre and Tarantula; correct Mid band, no leftover opposite or native template | `ae714f04965b` | Native fiendish on Crab and Black Widow; LN and CN focused only |
| RU-07 | Done | None new | Neutral toggle read at execution, off and on. Census shows no Summon Monster template on Summon Nature's Ally rows | `ae714f04965b` | Toggle changed with a menu open; Summon Nature's Ally cast under both toggle positions |
| RU-08 | Done | Repaired Ogre judged as spider | LG/Erastil, LE/Asmodeus, TN/Pharasma casts. Deity-derived fixed-alignment control passed. Repaired Ogre offered to LG in both routes | `ae714f04965b` | None for the restriction itself |
| RU-09 | Done | Membership tests | Non-cleric provider control | `ae714f04965b` | **Real multiclass book; ordinary spontaneous SM and SNA pool casts** |
| RU-10 | Done | Family-route tests | Repaired Ogre and Tarantula in Herald tier V and VII menus, on the original source | `ae714f04965b` | **Herald spider casts, including a quantity tier, debit and metamagic**; Crab I and Widow IV admission |
| RU-11 | Done | Family-route tests | None beyond census | none | **Druid conversion and prepared SNA comparison and casts** |
| RU-12 | Done | Benefit scope | Each species' poison and the ordinary Giant Spider: DC reads base+2 in the resolving context (14 to 16; 12 to 14), stable on recalculation | `ae714f04965b` | **Actual first and continuing saves; Augment interaction; inherited Hunting poison** |
| RU-13 | Done, unverified | Benefit scope | None | none | **Native web hit and break-free DC.** The webbed state's break-free route must be shown to read this DC |
| RU-14 | Done | Exact identity negatives | Ordinary Giant Spider with and without the feat | `ae714f04965b` | **Reload**; native ineligible-source controls |
| RU-15 | Done | Display order tests | Real popups exact, sorted, no duplicates, including spider rows on the cleric's Summon Monster menus | `ae714f04965b` | SNA and Druid rendered menus; caster without the feature in those routes |
| RU-16 | Done | None | None | none | **Fresh process, saved shortcut, reload** of an original and a generated row |
| RU-17 | Done | Module boundary | None | none | **RU-absent and module-off profiles** |
| RU-18 | Done | Negative shapes for feature, unit, tier, count, condition, recipient, hidden spawn, effect | Zero unresolved rows | `ae714f04965b` | None |
| RU-19 | Done | Observer compiles | Old Bralani and 3/1 expectations replaced; per-row semantics passed; failure records retained | `ae714f04965b` | None |
| RU-20 | Done | n/a | Draft PR opened, restoration verified | `ae714f04965b` | **NPC-without-feat selection/AI control**: generated rows extend the published parents' variant arrays, as Races Unleashed's own rows do, and no NPC selection was observed. Owner review |

## Smallest next test bundle

One new guarded phase with a Drow Druid and a Drow spontaneous caster: the eight RU-03/RU-04/RU-11 Summon Nature's
Ally casts, conversion and spontaneous debits, Superior and Augment Summoning, one real poison save sequence and one
web break-free attempt, then a save/reload and valid RU-absent and module-off profiles.

The web break-free route (RU-13) is the item most likely to need a code change: if the native state uses the
target's CMD rather than these parameters, the +2 cannot reach it through this hook.

## Known review item

`AlternateSummoningCoordinator` calls `LoadManifest` before the integration's once-only guard, so while a family
is still waiting the manifest parses on each update. In practice this resolves on the first update; it is left as is
because a fix changes the candidate's bytes. Move the load behind the guard with the next product change.

## Validation ledger (15-minute target, 30-minute ceiling)

| Step | Seconds |
| --- | ---: |
| Focused gates, including the one failed compile | about 130 |
| Failed candidate build (art registry receipt) | about 100 (estimated) |
| Candidate build `3e27c5809a9d` | 431 |
| Guarded native run, `3e27c5809a9d`, FAIL | 264 |
| Candidate build `ae714f04965b` | 414 |
| Guarded native run, `ae714f04965b`, PASS | 268 |
| **Total** | **about 1,600** |

The 900 s target was exceeded; the 1,800 s ceiling was not. The remaining observations do not fit, so they are
reported rather than launched.

Focused results: `summon-cleric-alignment;summon-spider-summoner;expanded-summoning.display-ord;druid`, 30 of 2,917
selected, 0 failures, recorded by the candidate build. A wider pre-observer selection across expanded and alternate
summoning, Druid, manifest, favored-class leaf and better-vendors ran 823 tests, 0 failures. The complete
domain suite and release qualification are deferred.
