# Eastern Weapons implementation report

## Status and release identity

Eastern Weapons implementation and automated qualification are complete on
`codex/eastern-weapons`. The accepted base is
`4ffd15b09992bd9cee9d330eee0a650ad2c94661`; the final functional and artifact
source is `9966edfa160ed4d898482f754a6b8abf1f9ebc11`. Release identity is assembly
version `0.0.80`, informational version `0.0.80-eastern-weapons`, and package
`KingmakerGunslinger-0.0.80-eastern-weapons.zip`.

The branch is published through the required guarded non-force helper. Draft
PR #4, **Add complete Eastern Weapons feature**, targets `master` and remains
open, draft, and unmerged:
https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/4

Subjective visual acceptance remains deliberately pending human review. It is
the only acceptance surface not represented as an automated PASS.

## Delivered catalog

The independent, default-enabled `eastern-weapons` module adds stable
Wakizashi (`0x004B4D48`), Katana (`0x004B4D49`), and Nodachi (`0x004B4D4A`)
categories. Forty-six Eastern blueprint identities register in every module
state. Module state gates only new selection, commerce, loot, and custom
presentation.

Each family contains mundane, masterwork, cold iron, and +1 forms. The eighteen
named weapons form three independent progressions:

- Wakizashi: Paper Lantern, Quiet Current, Falling Petal, Foxfire Whisper,
  Empty Sleeve, Night Without Moon.
- Katana: Wayfarer's Oath, Winter Reed, Drawn Horizon, Thunder at the Gate,
  Moonlit Crossing, Heaven's Measure.
- Nodachi: Border Sentinel, Cloud-Cleaver, Storm Over Stone, Mountain-Sunder,
  Unfixed Form, World-Tree Severer.

All named native enchantments are exact installed blueprint references.
Heaven's Measure uses the preferred native Brilliant Energy configuration
after passing living and undead target controls. Every capstone has effective
bonus +10 or lower.

## Mechanics

The merged Exotic Weapon Proficiency catalog presents **Weapon Proficiency
(Elven Branched Spear)**, **Weapon Proficiency (Katana)**, and **Weapon
Proficiency (Wakizashi)** in the accepted native-consistent relative order.
Wakizashi and Katana entries are singular, idempotent static choices; Nodachi
is martial and has no exotic choice.

Katana uses the same live grip authority for proficiency and Moonlit Crossing.
Exact exotic proficiency permits both grips. Broad martial proficiency removes
the ordinary -4 penalty only while the Katana is actually wielded two-handed.
Nodachi is covered by broad and exact legitimate martial grants. It receives
Heavy Blades or Polearms training through native ranks without doubled benefit,
reach, Brace, or polearm animation. Wakizashi uses native light/Finesse rules;
Weapon Finesse, Finesse Training, and Agile routes do not duplicate Dexterity
damage.

Live positive and negative controls passed for:

- Wayfarer's Oath active-set +2 competence Initiative.
- Falling Petal confirmed-critical +1 Dodge AC, including miss, unconfirmed
  threat, refresh, and weapon-switch controls.
- Moonlit Crossing mutually exclusive one-hand +1 Dodge AC and two-hand +2
  flat weapon damage.
- Mountain-Sunder active-Power-Attack, first-hit-per-round 1d6 force damage,
  including miss, inactive, switching, repeated-hit, and critical controls.
- Unfixed Form's one native damage-size step for polymorph or current/natural
  size difference, without compounding simultaneous predicates.
- Native Speed in either hand, repeated full attacks, Haste arbitration, and
  switching; native Brilliant Energy living/undead exclusion.

Tabletop Deadly is omitted without approximation:

`DEFERRED  ENGINE HAS NO RELIABLE COUP-DE-GRACE DC HOOK`

## Acquisition and assets

Four base-campaign vendor tables receive 49 count-one rows: 42 generic and
seven named. Four fixed containers receive eleven named rows. Together the
seven vendor-named rows and eleven fixed-loot rows place all eighteen named
weapons exactly once. Each of four installed Beneath the Stolen Lands weapon
tables receives all twelve generic items, for 48 singular rows; no named
Eastern weapon enters ordinary BTSL stock.

The release contains original Wakizashi, Katana, and Nodachi equipped models
in `kingmakergunslinger.easternweapons`, plus canonical category icons and
distinct Night Without Moon, Heaven's Measure, and World-Tree Severer capstone
icons. The Unity bundle contains exactly three prefabs and falls back to the
validated native donor when rejected or unavailable.

## Qualification

- Repository validation: PASS.
- Complete dependency-free domain/reflection suite: PASS, 1,047/1,047.
- Clean exact-reference Release build and output validation: PASS.
- Strict deterministic standalone package validation: PASS.
- Expanded save-free combat: PASS, all 18 assertions.
- Vendor/loot observer with module ON and OFF: PASS.
- Three-phase working-save persistence: PASS; exactly two correlated writes.
- Call of the Wild, Arms and Armor, Toggle Custom Soundpacks, and maximum
  combined profiles: PASS; every Mods tree restored exactly.
- Complete six-module matrix: PASS, 64/64 unique fresh-launch states on one
  exact functional commit and DLL identity; settings restored byte-for-byte.
- Canonical non-mutating working-save smoke: PASS; no save-writing API call.
- Built, packaged, installed, and runtime-loaded DLL identity: PASS.

Final artifact identities:

- DLL SHA-256:
  `B8586B620413F0C0442B60FC0911395550C6B049AD8FF01F78B93A10B962B37D`
- DLL MVID: `13ba0899-9970-4403-ae00-e6ac32ffe473`
- Package SHA-256:
  `0FEAE10BA8DC5941C2C536C2AA3AF7C2BFECA2C2B1B5EBFF1B0300A3DF0DEF0C`
- Eastern bundle SHA-256:
  `39884FF681EE553DE957E36E01B350AB926A452F994C4E8D33015D57D4EAD1EC`
- Restored settings SHA-256:
  `2E53FA0A09C56662434F6EA548FF5EBCF91F5AAF293D668248221239A1308655`

The protected baseline was never accessed. Only `KMG_AUTOMATION_WORKING` was
used for authorized save-backed qualification. No autonomous merge was or will
be performed.

## First human playtest repair addendum

The accepted catalog, identities, mechanics, acquisition, saves, and version
remain frozen. The repair adds exact Call of the Wild Focused Weapon children,
measured diagonal icons, revised asymmetric single-edge family geometry,
Scimitar/Bastard Sword/Greatsword presentation donors, exact recursive binding
of the inherited equipment-hand visual field, and an all-30 instantiated
family-prefab audit.
Source/package gates pass with 1,048 tests and Eastern bundle SHA-256
`F58801B7B34514B06577EA9CE36F2F3FC0A79A6F157113EA227251BFE2A15B43`.
Functional source `5e99d4d7555d9d96efd7bd79714161003e314013` passed final
standalone, compatibility, persistence, 64-state, package,
installed/runtime-identity, and working-save-smoke gates. Subjective visual
acceptance remains pending.
