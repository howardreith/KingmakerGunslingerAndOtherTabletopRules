# v0.0.167 review follow-up: rules fixes and model refinement

Branch `codex/s30-31-rules-and-model-refinement` from master `48e6fb4abf`, lab
machine DATA, 2026-10-25. It answers the owner's October 25 rules review and
model refinement plan, then the owner's second review the same day, which
asked for a true lesser globe and a fixed sword grip and authorized
finalization, merge and release without another approval. The work ships as
v0.0.168 ([release notes](RELEASE-NOTES-0.0.168.md)). The released v0.0.167
records stay as published.

## R1: Trumpet Archon Airborne (fixed)

`ConfigureTrumpet31` rebuilt the unit's facts without the native Airborne fact
(`70cffb448c132fa409e49156d013b175`) that Astral and Lantern carry. Trumpet now
grants the same fact. Its 40-foot ground speed and the excluded vertical-flight
system are unchanged. A Trumpet saved before this change keeps its old facts
until it expires.

The native fact differs by installed stack, which the observer now records:

- Vanilla (KMG-only profile): two conditional terms, -1 attack and -1 damage
  against a target with *Air Mastery* (`be52ced7…`).
- With Call of the Wild: its own AddFlying, difficult-terrain and Ground buff
  and ability immunities, and a maneuver immunity, in place of those terms.

## R2: Astral circle protection reaches 20 feet (fixed)

The Astral's configured control protection came only from the canonical
10-foot Magic Circle, while PF1 Protective Aura functions as Magic Circle
against Evil with a 20-foot radius. The angel's own 20-foot aura recipient now
carries a clone of the canonical recipient's configured
`ProtectionFromAlignmentControlImmunityComponent`, so the startup choice and
fixes to that shared component carry through. The +2 typed defenses are not
copied (the aura's +4 supersedes them). Normal, Hound and Trumpet circles keep
10 feet; the canonical circle stays bound to the Astral as before. The
recipient text is registered once, after the circle configuration is known,
and states the control protection when it is enabled. The carrier text's
missing space ("A20-foot") is also fixed.

The aura recipient gains one named, stateless component. Native
`Fact.PostLoad` matches saved component data by name and starts new
components fresh, so saved recipients load without conflict. Serialized aura
ownership (carrier, area, link) is unchanged; no cold-load case was run.

## R3: Astral protective aura is a lesser globe (fixed)

The owner's second review: the protective aura acts as a lesser globe of
invulnerability, not spell resistance, against all spells of 3rd level or
lower regardless of how the spell interacts with spell resistance, without
depending on Call of the Wild.

`AstralProtectiveGlobe` used to be a `RuleTargetLogicComponent` that set
`RuleSpellResistanceCheck.TargetIsImmune`. That blocked most casts, but a
caster's `IgnoreSpellResistance` or Primal Magic Essence bypassed it, and a
persistent area without spell resistance (clouds, mists, webs) never asked.
It is now a plain owned component on the aura recipient, read by two native
consumers:

- `AbilityExecutionProcess.DoApplyEffect` prefix: the single native dispatch of
  an ability effect to one target (single target, burst, line, projectile,
  delivered touch). A level 0-3 Spell or SLA whose caster stands outside the
  emitter's 20-foot aura is skipped for the covered creature before any spell
  resistance check. A held touch charge is judged on delivery, as native spell
  resistance does.
- `AreaEffectEntityData.ShouldUnitBeInside` postfix: such a persistent area
  excludes the covered creature, which returns to it once it leaves the globe
  or the aura ends (suppressed, not dispelled).

The level, Spell/SLA and outside-origin rules are the unchanged
`AstralDevaRulesPolicy.BlocksLesserGlobe`. Spells of 4th level or higher
(including heightened), casts from inside and the caster's own effects pass.
The constant archon and angel circles from `ArchonConstantMagicCircle` carry a
Magic Circle spell context but are supernatural: a context with no ability
fact, spellbook or item whose caster owns that grant is never treated as a
cast. Call of the Wild's own globe (`UnitPartSpellInvulnerability`, patching
`UnitPartSpellResistance.IsImmune` and `AbilityEffectRunAction.Apply`) was
read for reference only; nothing depends on it. The recipient text now says
"Like a lesser globe of invulnerability ... whether or not they allow spell
resistance, and their areas exclude it."

The aura recipient keeps its component name and position; the component is
stateless, so saved recipients load as before.

### What the Movanic Deva uses

The owner suggested reusing the Movanic Deva's implementation. The native
inputs capture (`20261025T1715544517250Z`, gunslinger-only) shows the summoned
Movanic Deva gets its aura from `SubtypeAngel` (`65d9b688…`), which adds
`ProtectiveAuraFeature` (`bd3c71c5…`) -> `ProtectiveToggleAbility` ->
`ProtectiveAuraSourceBuff` -> `ProtectiveAuraAreaEffect` ->
`ProtectiveAuraEffectBuff` (`c30fd512…`). That effect buff has only
`SavingThrowBonusAgainstAlignment` and `ArmorClassBonusAgainstAlignment`
(+4 resistance and +4 deflection against evil), the same native components
the Astral aura already uses. Its in-game text (`b0388551…`) promises a magic
circle and a lesser globe, but no native component implements either, and no
installed mod changes the Movanic Deva. There was no native globe to reuse; the
native Movanic Deva is unchanged.

## Hound sword grip (fixed)

The owned greatsword was parented at the `R_Palm` origin (the wrist) with
identity rotation and passed through the body. A grip observation in
`hound-armed` measured the palm frame in game (fingers along -X curling to +Y,
thumb on +Z) and the native `WP_GreatswordBarbarian` clone (blade along model
+Y, hilt below the origin, not CPU-readable). `HoundArchonSwordView` now places
the hilt across the fist, leaving on the thumb side with the flat facing the
palm, the origin 7 cm past the fist centre so the guard clears it. A first
15-degree tip toward the fingers reached the floor at idle; the shipped grip
is tipped 5 degrees toward the wrist: level in the measured combat stance and
angled down without touching the floor at idle. The armed check asserts the
exact transform.

## Model refinement

Lemure, Dretch and Hound Archon visible surfaces are rebuilt by a new
signed-distance sculpt module feeding the existing exporter. See
[the asset source notes](../assets-source/original-models/outsider-celestial/SOURCE.md).
Identities, bone sets (17/29/29), donor and renderer contracts, loader flags,
attachment, accessory suppression, expiry visuals and teardown are unchanged.

| Creature | Vertices / triangles (was) | Components | Albedo |
|---|---|---|---|
| Lemure | 6,009 / 8,592 (4,107 / 5,528) | 9 | 1024px |
| Dretch | 7,769 / 10,000 (7,497 / 9,636) | 23 | 1024px |
| Hound Archon | 8,192 / 10,904 (6,401 / 8,420) | 8 | 1024px |

Two consecutive builds of each produce identical mesh and albedo bytes. The
October 14 rig capture is not on DATA; the released builders rebuilt their
v0.0.167 exports byte for byte from the October 17 recapture of the same
donor except the `rigSha256` field, so the three visual policies now pin
`6443b580…`. Restoring the October 14 file would reproduce the same geometry
under the old pin.

## Validation

Focused gate: production compilation and 16/16 selected domain tests
(`expanded-summoning.sprints30-31`, `magic-circle`,
`expanded-summoning.refinement`), including the new
`outsider-body-contracts` test (loader contract, closed outward lit
components, source-record hashes). It failed on the drifted source record
before the regeneration that closed it.

Development candidates (none is a release):

| Candidate | Commit | Package | DLL | MVID |
|---|---|---|---|---|
| rules 1 | `4e1aead408` | `da6eb6f0…` | `99232c5f…` | `06afc28c…` |
| rules 2 | `5da33572e6` | `7c067588…` | `ee072900…` | `01b6b420…` |
| art 1 | `773d990671` | `372a2b8d…` | `5b5669e6…` | `16e9947b…` |
| art 2 | `a25dda8b49` | `5739a252…` | `e4d468e2…` | `e741cc49…` |
| baseline | master `48e6fb4abf` | `542cd4ce…` | `9643e2aa…` | `e437c943…` |

Native observations (evidence directories under `runtime-evidence/`):

| Case | Candidate, stack | Result | Evidence |
|---|---|---|---|
| defense-consumers incl. new Airborne probe | rules 2, KMG-only | PASS 51/51 | `20261025T1427199771051Z` |
| shared-circles incl. new 10/20-foot reach | rules 2, KMG-only | PASS 20/20 | `20261025T1430068285888Z` |
| defense-consumers | rules 1, full stack | FAIL at Lemure reset | `20261025T1402327590067Z` |
| defense-consumers | baseline, full stack | same FAIL | `20261025T1437284938511Z` |
| defense-consumers (first probe) | rules 1, KMG-only | FAIL: no vanilla consumer modelled | `20261025T1410044457540Z` |
| Airborne contract audit | rules 1, KMG-only | recorded | `20261025T1418224034823Z` |
| outsider-bodies (Lemure, Dretch) | art 2, KMG-only | PASS 12/12 | `20261025T1544024634091Z` |
| outsider-bodies | art 1, KMG-only | FAIL: Dretch flags missing | `20261025T1525009903511Z` |
| hound-armed | art 1, KMG-only | PASS 9/9 | `20261025T1528152822149Z` |
| hound-disarm | art 1, KMG-only | PASS 12/12 | `20261025T1531261687289Z` |
| creature review, Working save | art 2, full stack | packet PASS x3, cleanup PASS x3, motion FAIL x3 | `20261025T1545587739077Z` |
| creature review, Working save | baseline, full stack | motion FAIL x3 (same cause) | `20261025T1551018071587Z` |

Reach probes, with the shared module enabled and an evil controller applying
the native Dominate buff: Hound and Trumpet protect at 1.5 m only; the Astral
protects at 1.5 m and 4.6 m; nothing at 9 m; a good controller is never
blocked; with Astral and Hound overlapping, removing the Astral leaves only
the Hound's 10-foot protection. Trumpet's Airborne fact is single and active,
and its conditional terms equal the Lantern and Astral flyers'.

Hound mesh and albedo bytes are identical in art 1 and art 2.

Second-review candidates (development, none is a release):

| Candidate | Commit | Package | DLL | MVID |
|---|---|---|---|---|
| globe 1 | `5b755a119d` | `d5ba53f9…` | `c1663e9c…` | `97f208d8…` |
| grip | `53bb76e7ad` | `9ca1ef0e…` | `0349fd5e…` | `7e786479…` |
| globe probe | `1452b22251` | `8d63f3f7…` | `750918b3…` | `2dde3bfa…` |

Product source is identical from globe 1 to globe probe except the grip
transform; later commits changed only the astral-core observer.

| Case | Candidate, stack | Result | Evidence |
|---|---|---|---|
| astral-core incl. lesser globe | globe probe, KMG-only | PASS 13/13 | `20261025T1806320876453Z` |
| hound-armed (measured grip asserted) | grip, KMG-only | PASS 9/9 | `20261025T1738243118908Z` |
| hound-disarm | grip, KMG-only | PASS 12/12 | `20261025T1741019771262Z` |
| creature review, Hound, Working save | grip, full stack | packet + cleanup PASS, motion FAIL (classified) | `20261025T1742587364973Z` |
| shared-circles | globe 1, KMG-only | PASS 20/20 | `20261025T1705195684488Z` |
| defense-consumers | globe 1, KMG-only | PASS 51/51 | `20261025T1708024286684Z` |
| astral-support | globe 1, KMG-only | PASS 9/9 | `20261025T1710381372850Z` |
| astral-atwill | globe 1, KMG-only | PASS 6/6 | `20261025T1713192874959Z` |
| native inputs (Movanic, angel aura) | globe 1, KMG-only | research | `20261025T1715544517250Z` |
| grip measurement | `dafcbc730d`, KMG-only | research | `20261025T1634235189829Z` |
| astral-core | globe 1 | FAIL: pilot-era hidden-route check | `20261025T1656356528526Z` |
| astral-core | `fb1b8f4b17` | FAIL: native buffs carry SR false | `20261025T1722323227092Z` |
| astral-core | grip | FAIL: probe context lacked FxSpawners | `20261025T1736025919270Z` |
| astral-core | `2b15141c38` | FAIL: Magic Missile control dealt no damage | `20261025T1750456166568Z` |

The globe probe dispatches real native effect components through the private
`DoApplyEffect` with its Harmony prefix, and observes the native spell
resistance rule. From outside the aura, Mage Armor (spell resistance no), Magic
Missile at CL50 (spell resistance yes) and the Hound's Aid SLA never reach the
covered creature and never reach a spell resistance check. From inside, Mage
Armor applies and Magic Missile reaches exactly one check, which CL50
overcomes against the Astral's SR 25. Mage Armor heightened to level 4 and
Mage Armor on an uncovered creature apply. A native Obscuring Mist (level 1,
area spell resistance no) cast from outside excludes the covered creature,
includes it after it leaves the aura, excludes it again on return, then ends
with no lingering buff. A second Astral 11 m away still covers the shared
creature with its own constant aura. Magic Missile's damage does not arrive
through a direct one-target dispatch (no damage packet even from inside), so
its pair proves the spell resistance boundary, not damage.

Every run restored the live mod exactly (254 files, `5C3E4AD4…BFBB`). The
Working-save reviews left the baseline and Working saves byte-identical (95
saves before and after).

### Classified failures (retained)

- **Full-stack defense reset.** With Call of the Wild enabled, its
  `VindicativeSmiteAllowedBuff` (`046b4936…`) marks the hostile once per damage
  packet the existing DR/energy probes deal a summoned creature (11 for the
  Lemure). The same failure reproduces on unchanged master. The KMG-only
  profile runs the phase cleanly.
- **Creature-review motion assertion.** The generic idle/move/attack check
  samples the first skinned renderer. On the two-renderer Lizardfolk donor
  that is the deliberately suppressed accessory (`_ammunition04`), so it
  reports `rendererEnabled=false`. The same failure reproduces on unchanged
  master. The refinement packet, which samples the first enabled renderer,
  and `outsider-bodies`, which names the body renderer, both pass.
- **Dirty-tree refusal.** One `shared-circles` launch was refused because a
  draft file was created in the worktree during the run (operator error).

## Release candidate gate

Sprint-level gate on release commit `eccc0d6130` (v0.0.168, 2026-10-25,
4.5 minutes): repository wrapper with the v0.0.168 release validator and its 22
corruption fixtures, the complete domain suite (2,748 of 2,748), exact-reference
Release build and strict package validation. Candidate package `b762a304…`,
DLL `669ff4a1…`. A first attempt on `44e28c522a` failed 24 tests: 23 read a
machine-local native IL extract absent from this worktree
(`artifacts/inspection/bodyguard-native`, copied from the main checkout), and
one required the active resume to state the public equation, which the
v0.0.167 release record had dropped (fixed in `eccc0d6130`).

## Open items

- **Owner aesthetic review.** The owner accepted the changes and authorized
  release on October 25; no separate personal approval of the bodies or grip
  is claimed.
- **Stale pilot-era observers.** `dretch-cloud`, `archon-hound`, `bootstrap`,
  `trumpet-book` and `trumpet-dispel` still require routes hidden that v0.0.167
  published, as astral-core did. They were not run here and are not repaired.
- **Not run:** cold-load or persistence, public routes, standalone and combined
  final integration, and the full-stack (Call of the Wild) native profile for
  these phases.
