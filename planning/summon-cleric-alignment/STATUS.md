# Cleric alignment restrictions: status

**Draft for review. The single authorized native invocation failed before opening any menu
(2026-11-09). The menu filter, legal cast and forbidden attempt have not been observed in game.**
Separate from R1-R7 and the alphabetical-menu mission, whose results keep their own identities.
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
- **Cleric detection**: real class levels (`Progression.Classes`). A deity or a cleric spellbook is
  not membership. Multiclass clerics stay restricted in any spellbook. Items and abilities
  without a spellbook keep native rules. Class SLAs, including the alternate summon families,
  are out of scope.
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
census listed the first 60 differences. That is why the policy does not read them. No shared
or third-party blueprint was changed.

## Candidate and native result

Candidate `059854c046e1` (source `059854c046e18322aa457dd2dfe00ffdde89a9b6`, v0.0.180 development,
package `43af528aff6ea6117104bef219ed6cc9e2a794f7ebec029255d114f60e17d4bf`, DLL
`0dcf91f4f7416c79977b8aeb65eacd1f3eb2d8d941df000d429f423beb79011d`, MVID
`16e65126-1573-475e-ace5-869ba02d0dc5`). Observer: compiled into that DLL. Run
`20261109T1812332936554Z-97654466dd04405abc0791dcfab52a8c`.

**Result: FAIL, 4 PASS and 3 FAIL.** The passes are the route census and three state-restoration checks.

Established, structural only:
- `cleric-route-census` PASS: class, spellbook and list GUIDs exact; all 9 parents on the real
  cleric list; 621 children, 617 gated, 4 named RacesUnleashed rows, 0 unclassified.

Failures:
- `cleric-deity-census`: 5 of 26 options were unmapped. A real product gap, fixed in source below.
- `cleric-herald-caller-route`: my check assumed shared parents. A real routing finding (below).
- `execution`: a null reference. `GetSpellbook(cleric)` returned null after the native level-up,
  so setup stopped before a menu opened. The exact cause is not established.

**Not performed:** no menu was opened, nothing was selected or cast, no forbidden attempt ran, no
deity-derived or non-cleric control ran, and no screenshot was captured.

## Herald Caller: unsupported route

The installed conversion feature reaches the shared Summon Monster parent only at tier I. Its
other 23 conversions are native quantity rows, which carry no gate, so **the restriction does not
apply to a Herald Caller's spontaneous conversions above tier I**. Repairing this needs a
Druid-style projection or a change to native blueprints, so it is left for an owner decision.
Prepared Summon Monster spells on a Herald Caller use the shared parents. `docs/ALTERNATE-SUMMONING-SOURCES.md`
now states this route correctly.

## Source corrections after the run (`0d1cd93f8b`)

Focused checks only; **not built into a candidate and not run natively.** The native result stays
with `059854c046e1`.
- Five more deities mapped from the run's exact GUIDs: Groetus, Gyronna, Ragathiel, Tsukiyo, Gruhastha.
- The observer obtains the book through native `DemandSpellbook` and records the Herald route
  accurately. Whether that resolves the null is unverified.

Focused gate: 21 of 2,902 selected, all passing, production compiled.

## Restoration

Install v0.0.140, DLL `00EF6757...66FD`, 254 files, 16 mods, `Params.xml` `E44F4495...30BB`,
settings and 95 protected saves restored and verified. `KMG_AUTOMATION_BASELINE` untouched, no
campaign save written, no pending receipt. Raw evidence stays private under `runtime-evidence`.

## Validation ledger (900 s target, 1,800 s ceiling)

| Step | Seconds |
| --- | ---: |
| Focused gates (22, 3, 20) | 45 |
| Candidate build | 417 |
| Guarded native invocation | 128 |
| **Total** | **about 591** |

Memory: lowest 6.8 GB available in the build and 5.9 GB in the run; no warning.
