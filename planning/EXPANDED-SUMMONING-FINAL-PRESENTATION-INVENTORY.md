# Expanded Summoning Final Presentation Inventory

## Provenance

- Source SHA: `0bf5cb3b4d882ca07be91aa71820268b01cce02f`
- Guarded scenario: `observe-expanded-summoning-inventory`
- Run ID: `20260813T1249008703958Z-a226bd2de4574984b5d06627b4acc61e`
- Result: PASS, 37/37 assertions
- Profile: installed combined profile (`BagOfTricks`, `CallOfTheWild`, KMG)
- Save access: none

The observer inspected the final-live `AbilityVariants` arrays by exact parent
and child references. It did not infer identity from localized text.

## Live SNA menu inventory

| Tier | Visible children before repair |
|---:|---:|
| I | 5 |
| II | 14 |
| III | 20 |
| IV | 32 |
| V | 39 |
| VI | 46 |
| VII | 52 |
| VIII | 56 |
| IX | 58 |

There are 322 visible SNA children. The KMG generated choices use non-null
unit-portrait/category sprites, generally 92 x 120, whose atlas textures are
not CPU-readable. The nine preserved native choices all use the same 64 x 64
`ChangeShapeBeast` generic sprite. Across the SNA menus, only 31 sprite objects
represent 65 currently observed semantic identifiers, with 11 proven groups
where unrelated creature concepts share one sprite.

The white squares are not caused by a null child icon or a late publication
overwrite: the final-live generated abilities hold non-null inherited portrait
sprites. They are caused by the unsupported production contract itself--unit
portrait/category atlas sprites are being assigned as ability-choice icons.
They are non-project, non-readable atlas resources and do not render reliably
in the SNA variant surface. The repair replaces that entire path with persistent
project-owned textures and sprites loaded from packaged PNGs.

## Tier-I duplicate cause

SNA I is originally a direct ability. Expanded Summoning converts it to a
variant parent and publishes the frozen preservation clone
`b5b7cf07ffeb4533a1320fbd06072cc5` (`KMG_Summoning_Native_SNA_Tier1`). The
clone retains the generic display name `Summon Nature's Ally I` and generic
`ChangeShapeBeast` icon. The outer spell remains the same generic SNA I spell,
so the player sees the parent and a generic child with indistinguishable
presentation. The preservation child is mechanically unique: it summons the
native Mite and must become a named KMG-owned `Mite` wrapper, not be discarded.

## Unique native SNA spawn map

The following exact source abilities are retained as registered identities but
will be replaced in the enabled visible graph by named KMG wrappers. Disabled
publication restores the exact original references.

| Tier | Source child GUID | Multiplicity | Creature | Spawn unit GUID |
|---:|---|---|---|---|
| I | `b5b7cf07ffeb4533a1320fbd06072cc5` | one | Mite | `0c433dcdefcaaeb4db78b07c3ebf4c94` |
| II | `b8ac9c653789b2a46ad85a075734c0e2` | 1d3 | Mite | `0c433dcdefcaaeb4db78b07c3ebf4c94` |
| III | `bb1bac85be6b1e44eafdc54a3b757c3e` | 1d4+1 | Mite | `0c433dcdefcaaeb4db78b07c3ebf4c94` |
| V | `28ea1b2e0c4a9094da208b4c186f5e4f` | one | Manticore | `7b7701ffc8f335a47a9ed97516531b71` |
| VI | `2aab2a0c280ed3e408a09967ec6bb281` | 1d3 | Manticore | `7b7701ffc8f335a47a9ed97516531b71` |
| VII | `b81bb947975c4e34395ab4e09a036a16` | 1d4+1 | Manticore | `7b7701ffc8f335a47a9ed97516531b71` |
| VIII | `8d3d5b62878d5b24391c1d7834d0d706` | one | Nereid | `1618961b217a446459c6a91481065d2c` |
| IX | `780cbc629e74c1049b041b2a2f979863` | 1d3 | Nereid | `1618961b217a446459c6a91481065d2c` |
| IX | `f6751c3b22dbd884093e350a37420368` | one | Hamadryad | `32a7776fb5bb9fa408b97757c04d4247` |

No optional-mod SNA child was present outside the frozen native catalog in the
observed combined profile. The final icon set therefore requires 77 distinct
concepts: 66 published KMG catalog creatures (Dire Bat remains suppressed),
seven split native SM creatures, and Mite, Manticore, Nereid, and Hamadryad.

## Final publication target

- SNA I remains five choices, but all five are creature-named: Dog, Eagle,
  Giant Centipede, Poisonous Frog, and Mite.
- All nine native SNA placements above become KMG-owned, creature-named,
  project-icon wrappers in the enabled graph.
- The original source abilities and preservation identity remain registered
  and unchanged for compatibility.
- All 322 SNA and 371 SM visible placements resolve to one of 77 project-owned
  icon concepts; same-creature placements share one cached sprite.
