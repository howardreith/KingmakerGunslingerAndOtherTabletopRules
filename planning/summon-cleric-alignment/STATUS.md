# Cleric alignment restrictions: status

**Draft for review. Native result on the current candidate: FAIL, 49 PASS and 1 FAIL (2026-11-09).**
The menu filter, legal casts, forbidden refusals, Herald Caller coverage and the deity-derived
case were observed in game and passed. The one failure is an unresolved RacesUnleashed row: see
"Open item". The failure stands. Separate from R1-R7 and the alphabetical-menu mission.
Nothing here is merged, released or permanently installed.

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

## Current candidate and native result

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

## Open item: the RacesUnleashed Ogre Spider row

The census is right to refuse this row. It reports two spawned units. RacesUnleashed builds it by
copying native `0964bf88b582bed41b74e79596c4f6d9`, the Summon Monster V row with Evil and NonEvil
caster branches, and replaces only top-level spawn actions. That row's spawns are nested in its
branches, so by source reading the copy still spawns the native Redcap or Bralani rather than an
ogre spider. The two unit identities were not recorded and the row was not cast, so this reading
is unconfirmed in game.

Effect: the row carries no alignment gate, because it is a third-party blueprint. It needs the
RacesUnleashed Spider Summoner feature. If the reading is right, a non-evil cleric with that
feature who is barred from Chaos or Good, such as a lawful cleric or a follower of Asmodeus, can
still reach the Bralani (Good, Chaos) through it. An evil caster's branch is the Redcap, which has
no alignment descriptors.

**Owner decision needed:** judge this row by what it actually spawns, on the existing exact-identity
branch pattern, or by its label, or leave it unsupported. Not implemented, because it would add
unbuilt source to the tested candidate and the build and native run for this mission are used.

## Earlier candidate, preserved

Candidate `059854c046e1` (source `059854c046e18322aa457dd2dfe00ffdde89a9b6`, package
`43af528aff6ea6117104bef219ed6cc9e2a794f7ebec029255d114f60e17d4bf`, DLL
`0dcf91f4f7416c79977b8aeb65eacd1f3eb2d8d941df000d429f423beb79011d`, MVID
`16e65126-1573-475e-ace5-869ba02d0dc5`), run `20261109T1812332936554Z-97654466dd04405abc0791dcfab52a8c`:
**FAIL, 4 PASS and 3 FAIL**, before any menu opened. Its verdict is unchanged by the later run.
Its failures were 5 unmapped deities, a Herald check that assumed shared parents, and the missing
spellbook above. `0d1cd93f8b`'s `DemandSpellbook` change was superseded and is not the fix.

## Focused checks

`scripts/Invoke-KmgGate.ps1 -Level Focused`: 31 of 2,910 selected, all passing, production compiled.
Not full qualification. Cases include `summon-cleric-alignment.herald-route-exact`,
`.herald-projection`, `.native-branch-exact`, `.druid-unchanged`, `.third-party-semantic`,
`.fixture-prerequisites`, `.deity-derived-case` and `.herald-coverage-fails-uncovered`.

## Restoration

Install v0.0.140, DLL `00EF6757...66FD`, 254 files, 16 mods, `Params.xml` `E44F4495...30BB`,
settings and 95 protected saves restored and verified, on both runs. `KMG_AUTOMATION_BASELINE`
untouched, no campaign save written, no pending receipt. Restoration record `verified`. Raw
evidence stays private under `runtime-evidence`.

## Validation ledger (900 s target, 1,800 s ceiling)

| Step | Seconds |
| --- | ---: |
| First part: focused gates, candidate build, native run | about 591 |
| Focused gates (28, 4, 17.7; failed attempts counted) | about 50 |
| Candidate build | 544 |
| Guarded native invocation | 173 |
| **Total** | **about 1,358** |

This continuation spent about 767 s. The 900 s target was exceeded; the ceiling was not. Lowest
memory: 4.09 GB available in the build and 4.48 GB in the run, no warning.

No further build or native invocation is authorized.
