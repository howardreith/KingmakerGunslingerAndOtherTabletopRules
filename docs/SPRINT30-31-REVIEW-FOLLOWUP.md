# v0.0.167 review follow-up: rules fixes and model refinement

Development checkpoint, 2026-10-25, branch `codex/s30-31-rules-and-model-refinement`
from master `48e6fb4abf`, lab machine DATA. It answers the owner's October 25
rules review and model refinement plan. **No release is authorized or
prepared.** The released v0.0.167 records stay as published.

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

## Open items

- **Owner aesthetic review** of the three bodies. Offline renders and native
  frames are supporting art evidence, not approval.
- **Hound sword grip.** The owned sword is parented at the `R_Palm` origin (the
  wrist joint) with identity rotation. In native idle stills the greatsword
  rests near horizontal at hip height and appears to pass behind or through
  the skirt line. This attachment is unchanged by this work. The smallest
  alternative is a measured local offset and rotation in
  `HoundArchonSwordView`, verified by rerunning `hound-armed`; it is proposed,
  not implemented.
- **Astral globe SR=no dispatch** (the review's qualification question) was not
  investigated; no defect is claimed.
- **Not run:** cold-load or persistence, public routes, standalone and combined
  final integration, the complete domain suite, and any release candidate.
  Releasing this work needs a new release validator, because the v0.0.167
  validator pins the released art bytes.
