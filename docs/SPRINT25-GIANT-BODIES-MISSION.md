# Sprint 25 — Giant Bodies: Brachiosaurus and Elephant

Charter v1.0 page 22, Sprint 25, roster weight 5. Gargantuan/Huge quadruped
navigation, trample, long-neck framing, and completing Elephant so it is no
longer served by a Mastodon proxy. No mounts, companions, polymorph changes,
aquatic system, later creatures, or Sprint 26.

Branch `codex/expanded-summoning-sprint25-brachiosaurus-elephant`, created from
the verified released Sprint 24 master `97c881ea` (v0.0.155 published, records
and release tooling merged). One sprint, one branch, one product PR, one stable
release. The expected version is **v0.0.156**, confirmed free against the actual
remote: `git ls-remote --tags` shows v0.0.150 through v0.0.155 and no v0.0.156,
and the GitHub release list ends at v0.0.155 (Latest).

The final candidate repository root is chosen **now, before** any build or native
qualification, and is retained until promotion:
`C:/Dev/KingmakerGunslingerLab/wt/s25`. Sprint 24 produced two different artifact
builds from one identical source in two different roots, and the cause was never
established; picking the root up front is how this sprint avoids repeating that.

## Frozen source contract

Pathfinder First Edition adult entries and summon tables, read and frozen on
2026-10-15:

- <https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Brachiosaurus>
- <https://legacy.aonprd.com/bestiary/dinosaur.html>
- <https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Elephant>
- <https://legacy.aonprd.com/bestiary/elephant.html>
- <https://www.aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Nature%27s%20Ally%207>
- <https://www.aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Monster%206>
- <https://www.aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Monster%207> (read
  to settle the list-placement question below)

Printed numbers, live derivation, native implementation and owner-accepted
limitations are kept separate. No D&D 3.5, no Pathfinder 2e, no companion or
advanced forms.

### Brachiosaurus (new creature)

Both sources agree on everything except the tail attack bonus:

| Field | AoN current | Legacy PRD |
| --- | --- | --- |
| CR / XP | 10 / 9,600 | 10 / 9,600 |
| Type | N Gargantuan animal | N Gargantuan animal |
| Init / Senses | +0; low-light vision, scent; Perception +28 | same |
| AC | 18, touch 6, flat-footed 18 (+12 natural, −4 size) | same |
| hp | 171 (18d8+90) | same |
| Saves | Fort +18, Ref +11, Will +9 | same |
| Speed | 30 ft. | same |
| **Melee** | **tail +22 (4d6+19)** | **tail +23 (4d6+19)** |
| Space / Reach | 20 ft. / 20 ft. | same |
| Special Attacks | trample (2d6+19, DC 32) | same |
| Abilities | Str 37, Dex 10, Con 21, Int 2, Wis 13, Cha 10 | same |
| Base Atk / CMB / CMD | +13 / +30 / 40 (44 vs. trip) | same |
| Feats | Cleave, Great Cleave, Great Fortitude, Greater Overrun, Improved Bull Rush, Iron Will, Power Attack, Skill Focus (Perception), Weapon Focus (tail) | same |
| Skills | Perception +28 | same |

**Resolution of the tail bonus, recorded before implementation.** The selected
contract is the legacy **+23**, because it is the value the shared inputs
derive: BAB +13, Str 37 (+13), Gargantuan size −4, Weapon Focus (tail) +1 gives
+23. The same inputs independently derive the CMB and CMD both sources print
(CMB +30 = 13 + 13 + 4 Gargantuan; CMD 40 = 10 + 13 + 13 + 0 Dex + 4), so the
ability score and BAB are not in doubt. AoN's +22 would require an unexplained
−1 that no printed input supplies. No hidden modifier is manufactured, Weapon
Focus is not removed, and the two sources are not claimed to agree. Both are
recorded. This follows the owner's instruction of 2026-10-15 and stands unless
an authoritative erratum establishes a different intended input.

Derivations to implement live rather than hard-code: the tail is the creature's
**only** natural weapon, so it takes the native 1.5× Strength multiplier on a
positive modifier (+13 × 1.5 = +19, matching the printed 4d6+19) while retaining
correct ×1 behaviour at zero or a penalty. Trample is 1.5× Strength as well
(2d6+19) and its DC derives as 10 + ½ HD (9) + Str modifier (13) = 32. Trample
is a separate effect from the ordinary tail attack.

Explicitly not Brachiosaurus: Ankylosaurus daze/stun, Stegosaurus trip,
Tyrannosaurus's bite multiplier or Grab/Swallow, and any invented bite,
stampede, tail-sweep area attack or frightening roar.

### Elephant (existing released creature)

Both sources agree exactly, with no discrepancy to resolve:

CR 7, XP 3,200, N Huge animal. Init +0; low-light vision, scent; Perception +21.
AC 17, touch 8, flat-footed 17 (+9 natural, −2 size). hp 93 (11d8+44).
Fort +13, Ref +7, Will +6. Speed 40 ft. **Melee gore +16 (2d8+10), slam +16
(2d6+10).** Space 15 ft.; **Reach 10 ft.** Special Attacks trample (2d8+15;
DC 25). Str 30, Dex 10, Con 19, Int 2, Wis 13, Cha 7. Base Atk +8; CMB +20;
CMD 30 (34 vs. trip). Feats Endurance, Great Fortitude, Improved Bull Rush,
Iron Will, Power Attack, Skill Focus (Perception). Skills Perception +21.

There is **no** stamp routine and **no** grab: the adult Pathfinder Elephant has
a gore and a slam, and nothing else. Both are primary natural attacks, so each
takes the ×1 multi-natural Strength routine (+10 from Str 30), not the lone-tail
1.5× handling Brachiosaurus uses. Space is 15 ft. but printed reach is 10 ft.,
which must not be generalised into 15-ft. reach for every Huge creature.
Trample 2d8+15 derives as 1.5 × 10, and DC 25 as 10 + ½ HD (5) + 10.

This is a visual and mechanical completion of the existing creature. Its
identity, its eight existing roots, their children and their order are
preserved; it is not a second Elephant and not a Mastodon replacement. Actual
Mastodon units and assets stay unchanged, and no already-released Elephant route
is suppressed while a private development package exists.

## Settled: Brachiosaurus is Nature's Ally VII only

The order states that Brachiosaurus is "SNA VII only, three roots (VII one,
VIII 1d3, IX 1d4+1), no Monster row and no celestial/fiendish execution
children", and its expected surface — Monster unchanged at 95/543, Nature's Ally
96/553, 1,096 placements, 1,125 choices — is exactly self-consistent with that.

The frozen source does not agree. The Summon Monster VII 7th-level list prints
**"Brachiosaurus (dinosaur)\*"** alongside Dire crocodile, Dire shark, Giant
squid, Mastodon (elephant), Roc, Tyrannosaurus (dinosaur) and the outsiders. The
project's own frozen roster catalog says the same thing:
`ExpandedSummoningIdealRosterCatalog.cs` records
`R("brachiosaurus","Brachiosaurus",7,7,25,…)` — monster tier 7 **and** nature's
ally tier 7 — using the same two-tier shape that gives Summon-Monster-only
outsiders a null nature's ally tier. Sprint 24's Tyrannosaurus, also tier 7/7,
was published on both lists for this reason.

The owner settled this on 2026-10-15: **Nature's Ally VII only, three roots** —
`NaturesAlly.7.brachiosaurus.One`, `NaturesAlly.8.brachiosaurus.OneD3`,
`NaturesAlly.9.brachiosaurus.OneD4PlusOne` — with no Summon Monster row and no
celestial/fiendish execution children. The target surface is therefore 108
project creatures, Monster unchanged at 95/543, Nature's Ally 96/553, 1,096
generated placements plus 29 retained native wrappers for 1,125 visible choices,
with suppressed logical placements back to 0 after publication. These totals are
derived from source at build time, not hard-coded.

Because identities are allocated once and append-only, this is the reversible
direction: adding the Monster VII rows in a later sprint would be purely
additive, whereas publishing Monster rows that were not wanted could not be
cleanly withdrawn. The source discrepancy is kept in the sprint ledger so a
later sprint can act on it without re-deriving it, and it is **not** treated as
authorization to implement those rows now.

## Shared trample contract

The already-accepted automatic enemy-aware response policy is reused unchanged:
one legal native opportunity attack at its configured penalty, otherwise Reflex
for half, never both. Target-size legality, one contact per target per round,
exact command ownership, interrupt and death behaviour, action spending and the
ordinary-attack fallback are all preserved, and both new command registrations
must participate in every required interruption and duplicate-response guard.
Sprint 23's exact d8 trample support is reused for Elephant without granting it
powerful charge and without changing released d6 tramplers. Live tests cover
positive odd and even Strength changes and a zero-or-penalty case that does not
double non-Strength damage, plus the Triceratops, rhinoceros and d6-trampler
controls wherever shared code changes.
