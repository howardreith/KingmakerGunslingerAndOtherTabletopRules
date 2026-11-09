# Cleric alignment restrictions: status

**Draft for review. Native result on the current candidate `efc75b8cc0b0`: FAIL, 70 PASS and 1 FAIL
(2026-11-09).** The RacesUnleashed Ogre Spider row is now judged by the branch that executes, and that
was observed in game: refusal for a lawful good cleric and a permitted cast for a chaotic good one.
The one failure is the semantic census: two other RacesUnleashed rows carry a native template step my
contract did not list, so they are unresolved. See "Remaining item". The failure stands.
Separate from R1-R7 and the alphabetical-menu mission. Nothing here is merged, released or
permanently installed.

## Current candidate and native result

Candidate `efc75b8cc0b0` (source `efc75b8cc0b08da3220d9d1151860899d94adb99`, v0.0.180 development,
package `76eb276125b02dc3c6a5357982c727e619e13019ec342905ef824bbf360c309b`, DLL
`c49e34832e0c6ab755de11b74bed62693c9451e2e42bd228678e131cbabcfbae`, MVID
`61ea68d2-380d-4c99-b658-b0c210056752`). Run
`20261109T2033140995495Z-99cc4bd7d946443e95754fd706afc0fa`, phase `summon-cleric-alignment`.

**Result: FAIL, 70 PASS and 1 FAIL.** The only failure is `cleric-route-census`. This narrow result
belongs to this candidate; it does not change either earlier result.

### Structural contracts, read from the loaded game

| Row | Tier | Possible spawn references | Post-spawn | Outcome |
| --- | ---: | --- | --- | --- |
| Summon Monster V (Ogre Spider) | 5 | Redcap `af8bcff0...` if the caster is Evil, otherwise Bralani `58574e8d...` | Summon buff only | Conditionally governed |
| Summon Monster VII (Giant Tarantula) | 7 | Tarantula `2ea0fc9b...` | Summon buff only | Verified inherently legal |
| Summon Monster I (Giant Crab Spider) | 1 | Crab Spider `b7e12522...` | Summon buff, then a caster-Evil template step | **Unresolved** |
| Summon Monster IV (Giant Black Widow) | 4 | Black Widow `9b1fda47...` | Summon buff, then a caster-Evil template step | **Unresolved** |

All four: exact identity `130211b7fcb443e9b67d5561d75cf6a0` plus the internal name, required and
visible feature Spider Summoner `bd03ca46...`, quantity one, no alignment descriptors on the row.
Census of 621 children: 617 gated, 1 verified inherently legal, 1 conditionally governed, 2 unresolved.
These are possible references, not spawned instances.

### Third-party defect, recorded and not changed

**"Summon Monster V (Ogre Spider)" does not summon an ogre spider.** RacesUnleashed copies native
`0964bf88...` and replaces only top-level spawns, but that row's spawns are nested in its Evil
branch. Its Ogre Spider unit `4a205de6...` is not referenced by the row. The row's name, payload,
prerequisite and blueprint were left unchanged, and the current effect is what is restricted.

### Observed in game, each PASS

Rendered menus (real popups; 11 images inspected, every tile count matches its structured row count,
no blank tile; images support presentation only):

| Menu | Rows | Result |
| --- | ---: | --- |
| Spider feature, LG/Erastil, prepared Summon Monster V | 66 of 76 | Ogre row absent |
| Spider feature, LG/Erastil, Herald conversion from an ordinary tier V spell | 66 of 76 | Ogre row absent, order exact, no duplicates |
| Spider feature, LG/Erastil, Herald conversion from an ordinary tier VII spell | 86 of 104 | Tarantula survives at index 78, on its original prepared source |
| Spider feature, CG/Desna, Herald conversion from an ordinary tier V spell | 66 of 76 | Ogre row present at index 60, on its original prepared source |
| Spider feature, CG/Desna, prepared Summon Monster V | 66 of 76 | Ogre row present at index 60 |

The seven menus of the earlier candidate were repeated with the same row counts, 92, 31, 66, 92, 104 and 99 (the spider menu was replaced by those above).

Executed:

| Case | Result |
| --- | --- |
| Forbidden held Ogre row, LG/Erastil, native command | Visible with its feature, unavailable, command Fail, no slot spent, 0 spawn actions, 0 summons |
| Permitted Ogre row, CG/Desna, selected from the popup | Command Success, 1 cast, 1 spawn action, 1 spawned instance: **`AzataBralaniSummoned` `58574e8d...`, not an ogre spider** |
| Source debit for that cast | Exactly one tier V slot of the prepared Summon Monster V, all other books and budgets unchanged |
| Lantern Archon and Redcap legal casts; Ice Devil and retained native refusals | Repeated, PASS |

Controls: the row is hidden without its feature and visible with it on a real cleric. A real
non-cleric keeps it (decision `NotCleric`; provider level, no popup).

**Branch not cast:** the Evil branch (Redcap). It is covered by the loaded graph and focused tests only.

**Not observed:** a popup or cast of the Crab Spider or Black Widow row under this mission, console
navigation, saved-hotbar reload, cold load, autonomous AI, a multiclass book popup, a naturally
levelled Herald archetype, evil or true neutral cleric menus, and a module-disabled native profile.
The module boundary is covered by the focused hook tests only, as authorized.

## Remaining item

Crab Spider and Black Widow are unresolved because each adds a post-spawn step I did not list in the
contract: if the caster is Evil, apply native `SummonMonsterFiendishBuffI` (`c707fdbe...`), otherwise
`SummonMonsterCelestialBuffI` (`83a8a909...`) for the Crab Spider or `SummonMonsterCelestialBuffII`
(`ec906031...`) for the Black Widow. These are the repository's `NativeMonsterTemplateBuffs`, so they
look like the starred, caster-matched rule, but that mapping was not built or run.

Effect today: both rows stay ungoverned and keep their native availability, which is legal for every
valid cleric and deity pairing. Herald Caller conversions do not project them: the Black Widow is
missing from a Herald tier IV conversion. The Crab Spider uses the shared tier I parent.

## Rule

`forbidden = opposites(cleric alignment) UNION opposites(deity alignment)`, and a choice is
allowed when `forbidden` does not overlap its effective alignment descriptors. Good/Evil and
Law/Chaos are the opposites; neutral adds nothing.

Sources (aonprd.com): Cleric, "Chaotic, Evil, Good, and Lawful Spells"; Summon Monster I, the
type rule, table and starred-creature paragraph; deity pages; bestiary type lines.

## Implementation

- **Policy** `Summoning/SummonClericAlignmentPolicy.cs`, a pure single decision with exact catalogs.
- **Enforcement** `Summoning/SummonClericAlignmentGate.cs`:
  - A component on each owned Summon Monster child is the availability and visibility provider.
    Native `UnitUseAbility.OnAction` checks `IsAvailable` immediately before `Spend()`, so a
    held selection or shortcut is judged then, on the caster's current alignment and deity.
  - A postfix on `ActionBarSpellsGroup.Fill` removes forbidden rows from that popup's own widgets.
    The native popup does not filter by visibility; RacesUnleashed's own patch does that for
    its rows. The shared published array is never changed, so order and other casters are unaffected.
  - A postfix on `AbilityData.IsAvailableForCast` governs retained native Summon Monster quantity
    rows above tier I, by exact identity and the creature branch this caster would summon. A
    native row is a shared blueprint, so no descriptor union or blanket disable is applied to it.
- **Third-party rows** `Summoning/SummonForeignRowContracts.cs`: the four RacesUnleashed rows are
  classified from the loaded action graph by exact identity, tier, required and visible feature,
  quantity, nested condition, spawn references and post-spawn actions. A name never classifies a
  row, and any difference leaves it unresolved. Popup filtering and execution use one decision
  (`SummonClericAlignmentRuntime.DecideGovernedRow`). No third-party blueprint is changed.
- **Module boundary**: every hook first reads `BlueprintBootstrap.IsInitialized` and
  `ExpandedSummoningVisualVariantPatch.IsModuleActive`. Inactive leaves the native result, and the
  rule can only remove availability.
- **Cleric detection**: real class levels (`Progression.Classes`). A deity or a cleric spellbook is
  not membership. Multiclass clerics stay restricted in any spellbook. Items and abilities
  without a spellbook keep native rules. Class SLAs, including the alternate summon families,
  are out of scope.
- **`Spellbook == null` exemption, checked against the native contract**: `AbilityData.Spellbook`
  comes from `m_SpellbookBlueprint` or `Fact.Spellbook`. Every spell route constructs
  `new AbilityData(blueprint, spellbook)`: popup variants, conversions and metamagic copies. Only
  ability-fact variants have no book, and those are the out-of-scope class abilities. No route
  reaches a spell row with only conversion provenance, so the immediate check stands.
- **Deities**: 25 deities plus Atheism by exact installed feature GUID, read from the game's
  assets and Call of the Wild, with printed alignments. An unmapped deity is never treated as
  neutral: fixed-alignment rows are refused for that cleric, and starred rows still follow the caster.

## Descriptor coverage

The policy reads a catalog, not blueprint descriptors, and classifies every current Summon
Monster choice. A key with no classification throws, so nothing can count as neutral silently.

| Class | Rows | Basis |
| --- | --- | --- |
| Starred | Animals, dinosaurs and similar | Caster-dependent, so a neutral caster's template does not make a Good or Evil spell |
| Good | Astral Deva | Table (Good only) |
| Good, Law | Lantern, Hound, Trumpet Archons | Table |
| Good, Chaos | Bralani, Lillend, Ghaele | Table |
| Evil | Salamander | Table (Evil only) |
| Evil, Law | Lemure, Hell Hound, Bearded, Kyton, Xill, Erinyes, Bone, Barbed, Ice Devil | Table |
| Evil, Chaos | Dretch, Babau, Shadow Demon, Succubus, Bebelith, Vrock, Hezrou, Glabrezu, Nalfeshnee | Table |
| None | Elementals, mephits, Invisible Stalker | Table subtype is Elemental or Air |
| Additions | Gorgon, Redcap, Bogeyman, Frost Giant: none. Axiomite: Law. Shadow Mastiff, Soul Eater, Thanadaemon: Evil. Movanic Deva: Good | Bestiary type lines, recorded per row |

Live blueprints carry fewer descriptors than the rules (for example Hell Hound has none); the
census records 30 differences. That is why the policy does not read them. No shared or
third-party blueprint was changed.

## Herald Caller

Finished in this mission on the Druid pattern. No owner decision is outstanding for this route.
Feature `f6c112a579a81c0439c597f9ee222243` has 24 conversions: tier I uses the shared parent, and
the other 23 are native quantity rows. Those project onto the gated children, filtered for this
caster, and the original native row is governed at execution. Route identification
(`cleric-herald-route-identified`) and enforcement coverage (`cleric-herald-enforcement-coverage`)
are separate checks. Native result: 1 shared parent and 23 projected and governed, 0 uncovered.
The fixture granted the exact native feature to a levelled cleric, as the Druid route was
established; it is not a naturally levelled archetype.

## Earlier candidate `03a87b348912`, preserved

Candidate `03a87b348912` (source `03a87b34891203309488b42aa5618208c7801a83`, v0.0.180 development,
package `e5f32d7cb469d00cec974f7f67ecb3bd238c5839004341f1dd9ada966bc4f367`, DLL
`70b54d0051f38734f936d202152efddd7c3d34d998c820d04b17a846bc4638ea`, MVID
`aaaa72b0-3a64-4340-85d6-3e581df4899e`). Run
`20261109T1908013713796Z-42b7ada3857f4e37b2c0065eeaa4e2b5`, scenario
`disposable-expanded-summoning-sprint32-native-review`, phase `summon-cleric-alignment`.

**Result: FAIL, 49 PASS and 1 FAIL.** The only failure is `cleric-route-census`.

Semantic census (real cleric list, 9 parents, 621 children, identities exact):

| Classification | Count | Rows |
| --- | ---: | --- |
| Carries the gate | 617 | Owned Summon Monster children |
| Verified equivalent contract | 3 | RacesUnleashed Giant Crab Spider, Giant Black Widow, Giant Tarantula |
| Unresolved | 1 | RacesUnleashed Summon Monster V (Ogre Spider) |

A third-party row counts as verified only with an exact name, no live alignment descriptors,
exactly one spawned unit and its feature gate. The census did not compare that one unit's
identity with the named spider; RacesUnleashed's source shows a top-level replacement for them.

Fixture: the actual-cleric failure on `059854c046e1` is explained. `AdvanceDisposableSpellcaster`
never ran the native `ApplySpellbook` level-up action. Earlier classes received books only as a
side effect of features that demanded them, and a cleric's conversions wait on an unmade
selection. The fixture now runs the native action. `cleric-fixture-prerequisites` PASS: level 20,
class book `4673d19a...`, spell list `8443ce80...`, valid slots; dependent cells are gated on it.

Observed natively, each PASS:

| Evidence | Result |
| --- | --- |
| Menu, LG cleric of Erastil, tier IX | 116 published, 92 shown, 24 hidden |
| Menu, LG cleric of Erastil, tier III | 33 published, 31 shown, 2 hidden |
| Menu, Herald conversion, tier V | 76 published, 66 shown, 10 hidden, 3 quantities |
| Menu, Herald prepared parent, tier IX | 116 published, 92 shown, 24 hidden, conversions removed so no duplicates |
| Menu, LN cleric of Abadar, tier IX | 116 published, 104 shown, 12 hidden, Astral Deva shown |
| Menu, LN cleric of Asmodeus, tier IX | 116 published, 99 shown, 17 hidden, Astral Deva absent |
| Menu, spider feature, tier V | 76 published, 71 shown; Ogre Spider at index 64 of 71 |
| Legal cast, prepared parent | Lantern Archon; exact unit; one exact tier III slot spent |
| Legal cast, Herald conversion | Redcap; exact unit; one exact tier V slot spent; native conversion retained in chain |
| Forbidden attempt, selected child | Ice Devil; command Fail; no slot, no spawn, no summon |
| Forbidden attempt, retained native row | Ghaele or Thanadaemon row; command Fail; no slot, no spawn, no summon |
| Deity change on a held choice | Astral Deva available under Abadar; refused and hidden under Asmodeus (induced fixture state) |
| Non-cleric control | Real non-cleric with a deity fact and a Sorcerer book keeps the row. Provider-level; no popup opened |

The legal casts and forbidden commands ran with no manual tick or forced act. All seven captured
images were inspected: each popup's tile count matches its structured count, and no tile is
blank. Images support presentation only; row identity comes from the structured readings.

The retained native row read `visible: true` and `availableForCast: false`. It is withheld from
the menu by projection and refused at execution; it is not visibility-hidden.

**Not observed:** console navigation, saved-hotbar reload, cold load, autonomous AI, a multiclass
book popup, a naturally levelled Herald archetype, evil or chaotic cleric menus, a true neutral
cleric menu, and any cast of the Ogre Spider row.

The Ogre Spider row reported two spawn references and was left open for an owner decision. The
owner has since decided: judge a choice by the verified effect and caster branch that will execute.

## Earliest candidate `059854c046e1`, preserved

Candidate `059854c046e1` (source `059854c046e18322aa457dd2dfe00ffdde89a9b6`, package
`43af528aff6ea6117104bef219ed6cc9e2a794f7ebec029255d114f60e17d4bf`, DLL
`0dcf91f4f7416c79977b8aeb65eacd1f3eb2d8d941df000d429f423beb79011d`, MVID
`16e65126-1573-475e-ace5-869ba02d0dc5`), run `20261109T1812332936554Z-97654466dd04405abc0791dcfab52a8c`:
**FAIL, 4 PASS and 3 FAIL**, before any menu opened. Its verdict is unchanged by the later run.
Its failures were 5 unmapped deities, a Herald check that assumed shared parents, and the missing
spellbook above. `0d1cd93f8b`'s `DemandSpellbook` change was superseded and is not the fix.

## Focused checks

`scripts/Invoke-KmgGate.ps1 -Level Focused`: `summon-cleric-alignment` 19 of 2,913 selected, then with
`alternate-summoning`, `expanded-summoning.display-ord` and `druid` 59 of 2,913, all passing, production
compiled. Not full qualification. New cases: `.foreign-executing-branch`, `.foreign-semantic-negatives`,
`.foreign-herald-projection`, `.module-boundary`.

## Restoration

Install v0.0.140, DLL `00EF6757...66FD`, 254 files, 16 mods, `Params.xml` `E44F4495...30BB`,
settings and 95 protected saves restored and verified, on all three runs. `KMG_AUTOMATION_BASELINE`
untouched, no campaign save written, no pending receipt. Restoration record `verified`. Raw
evidence stays private under `runtime-evidence`.

## Validation ledger (900 s target exceeded; ceiling raised from 1,800 to 2,400 s)

| Step | Seconds |
| --- | ---: |
| Earlier work, through the `03a87b348912` run | about 1,358 |
| Focused gates (one failed compile about 20, then 22, 2, 2) | about 46 |
| Candidate build `efc75b8cc0b0` | 425 |
| Guarded native invocation | 197 |
| **Total** | **about 2,026** |

This continuation spent about 668 s, leaving about 374 s of the ceiling. Free memory immediately
before the build was 6.25 GB physical and 10.49 GB commit, and 7.7 and 12.19 GB before the run.

No further build or native invocation is authorized.
