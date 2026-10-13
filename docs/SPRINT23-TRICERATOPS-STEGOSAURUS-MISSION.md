# Expanded Summoning Sprint 23: Triceratops and Stegosaurus

Owner mission supplied in chat on October 13, 2026; this is its local repository
copy. Work is restricted to HOWARD-LAPTOP and the assigned Gunslinger/Tabletop
Added Rules lab. The endpoint is one qualified two-creature product PR, one next
stable release, independently downloaded artifact verification, exact restoration,
and a final released-state record on master. Sprint 24 must not start.

The current [validation policy](VALIDATION-POLICY.md) governs the mission. The
October 11 focused workflow remains authoritative, but this is an explicitly
selected implementation-to-release sprint rather than a request to stop at the
first development checkpoint. Coherent intermediate checkpoints are committed
and pushed, and then the mission continues.

## Baseline, branch, and publication boundary

- Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.
- Starting master: `bda157c6c402b329f47fb0531d96b52e01514114`.
- Stable baseline: v0.0.151; product merge
  `30f41dfb76a8264fdfd3238c9cba05a4c131f3bf`.
- Sprint 22 tested source: `d42e33ccc5b282daad594836b1defd4f680990e5`.
- Frozen v0.0.151 ZIP SHA-256:
  `5d4efe07770fba24fecb8d7dda6933e483ccef31f1ef0d0e35849b6d5c27bf67`.
- Product branch:
  `codex/expanded-summoning-sprint23-triceratops-stegosaurus`.
- Narrow post-release record branch, only if needed:
  `codex/expanded-summoning-sprint23-release-records`.
- One draft product PR goes directly to current master. It is made ready and
  merged normally only after qualification. Expected next patch is v0.0.152 if
  no intervening stable release exists.

The push policy may gain only the two exact branch names above. Preserve its
before/after hashes and exact diff. Use the repository's approved push wrapper;
no wildcard, bypass, force push, reset, clean, history rewrite, merge without
qualification, or release overwrite is authorized.

## Scope contract

Sprint 23 is the charter's single **Armored Dinosaurs** grouping. It adds only
Triceratops and Stegosaurus: a Huge trample/charge tank and a distinct
tail-control defender. Shared armored-quadruped infrastructure is allowed where
genuinely safe, but the creatures need different silhouettes, attack anchors,
rules, and summon surfaces. No Sprint 24, Deinonychus, Tyrannosaurus, sauropods,
animal-companion work, mounted combat, theropod framework, or unrelated product
scope is included.

The adult Pathfinder First Edition monster entries are authoritative, not animal
companion tables, another edition, donor profiles, or remembered fixtures:

- [Triceratops](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Triceratops)
- [Stegosaurus](https://legacy.aonprd.com/bestiary/dinosaur.html#dinosaur-stegosaurus)
- [Summon Monster VI](https://aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Monster%206)
- [Summon Nature's Ally VI](https://aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Nature%27s%20Ally%206)
- [Universal monster rules](https://legacy.aonprd.com/bestiary/universalMonsterRules.html)
- [Improved Critical](https://www.aonprd.com/FeatDisplay.aspx?ItemName=Improved%20Critical)

### Frozen Triceratops facts

Neutral Huge animal; 14d8+56, 119 hp; AC 21, touch 7, flat-footed 21;
Fortitude +15, Reflex +8, Will +5; Str 26, Dex 9, Con 19, Int 2, Wis 12,
Cha 7; BAB +10, CMB +20, CMD 29 (33 versus trip); speed 30 feet, space and
reach 15 feet; Perception +24; low-light vision and scent. It has one primary
gore +17 for 2d10+12, powerful charge (gore) 4d10+16, and trample 1d8+12,
Reflex DC 25. Feats are Great Fortitude, Improved Bull Rush, Improved Critical,
Power Attack, Run, Skill Focus (Perception), and Weapon Focus (gore).

The compact attack line does not print an expanded threat range and the feat list
does not name Improved Critical's weapon. The feat rule requires one weapon and
the animal has only the printed gore attack. Subject to primary errata review,
the implementation therefore chooses gore and doubles its normal threat range
to 19–20; it invents neither multiplier nor damage. This inference and the source
presentation discrepancy remain explicit.

Powerful charge applies exactly once to the eligible real charge strike. The
printed total is not a second attack and not twice the entire ordinary line.
Current weapon dice/size and live Strength contributions must remain native; the
unmodified derivation is twice the gore dice plus 2×Str rather than copying a
fixed result. Power Attack is independently proved at BAB +10.

Trample reuses the released automatic response adaptation: one legal native
opportunity attack at -4 when executable, otherwise Reflex for half, never both,
with response chosen before its result. It uses native movement/path contacts,
enemy-aware targeting, legal target size, one resolution per source/target/round,
and interrupted-command cleanup. Its d8 damage, live Strength and live HD DC must
work without changing released d6 callers. It is not a radius pulse or CMB
overrun, and it grants no free gore/charge or second full-round action.

### Frozen Stegosaurus facts

Neutral Huge animal; 12d8+36, 90 hp; AC 22, touch 10, flat-footed 20;
Fortitude +13, Reflex +10, Will +5; Str 27, Dex 14, Con 17, Int 2, Wis 13,
Cha 10; BAB +9, CMB +19, CMD 31 (35 versus trip); speed 30 feet, space and
reach 15 feet; Perception +16; low-light vision and scent. It has one primary
spiked tail +16 for 4d6+12 plus trip. Feats are Cleave, Great Fortitude,
Improved Initiative, Improved Overrun, Power Attack, and Weapon Focus (tail).

Each valid resolved tail hit can attempt native trip against the exact target,
at most once. Misses do not; distinct legal strikes may. Trip is not a save,
automatic knockdown, damage gate, daze, stun, poison, or extra weapon attack.
Native CMD, size, prone state and maneuver immunity govern it, and failure never
trips the Stegosaurus back. Power Attack is independently proved at BAB +9.
Cleave remains an active native feat, not a passive sweep. Improved Overrun may
retain Sprint 22's precise engine limitation only after the same absent consumer
is verified. Stegosaurus must acquire no Ankylosaurus rider or club behavior.

## Identities and publication surface

Allocate append-only identities once after confirming no newer registration.
Keep both new creature keys suppressed through hidden-candidate qualification.
Triceratops has four Summon Monster roots and four Summon Nature's Ally roots at
tiers VI–IX. Monster roots follow the existing celestial/fiendish execution-child
policy; Nature's Ally roots do not. Stegosaurus has four Nature's Ally roots only
and no Monster roots or template children. Quantity is one at VI, 1d3 at VII,
and 1d4+1 at VIII and IX.

With unchanged intake, the derived surface moves from 103 to 105 unique project
creatures; Monster 92/530 to 93/534 creatures/placements; Nature's Ally 91/533
to 93/541; generated placements 1,063 to 1,075; retained native wrappers remain
29; visible choices move 1,092 to 1,104. Re-derive rather than force these values
if legitimate independent work lands.

## Mechanics, profiles, and compatibility

Derive every field independently through real engine carriers: racial HD/hp,
class saves, feats, natural armor, size, reach, skills, attacks, and four-legged
trip defense. Report raw live values separately from normalized printed
comparisons. Do not cancel the user's difficulty modifiers. Perception uses exact
ranks with no generic Mobility or Stealth. Passive low-light vision/scent retain
the existing explicit limitation rather than substitute blindsight.

Reuse Sprint 22's final event-identity, victim-state, body lifecycle, controller,
movement, toggle, expiry, and save/load observations. Ankylosaurus remains a
negative control, not a Stegosaurus behavior template. Extend exact trample
dispatch for Triceratops and d8 narrowly. Parameterize live charge/trample math
without silently changing released profiles. Use relevant released rhinoceros
and d6-trampler controls when their shared paths change.

Required focused/native witnesses cover complete profiles and all twelve roots;
ordinary gore, powerful charge, trample responses/ledger/interruption and live
math; tail hit/miss and trip success/failure/immunity/prone/event uniqueness;
Power Attack and critical interaction; RTWP and turn based; qualified AI/action
spending; module OFF/ON, teardown, save/load, cleanup and fresh absence; standalone
and highest-risk combined compatibility plus targeted cold load in the compatible
fixture stack. Existing roots and wrappers remain structurally present.

## Original art and native presentation

Use project-owned deterministic editable bodies, albedos and icons. Redistribute
no donor meshes, textures or animations. Triceratops needs a forward horn contact,
full frill and a silhouette that is not a rhino with extra horns. Stegosaurus
needs a small head, high hindquarters, dorsal plates and a long four-spiked tail,
not an Ankylosaurus shell or club. Different measured donor rigs are acceptable.
Review clay, silhouette, textured, unlit, party-camera, walk/turn extremes and
attack-contact poses before deployment.

Each creature needs a genuinely Huge footprint and measured 15-foot reach; no
blind reuse of the Ankylosaurus +3-foot adjustment or stored range, target/root
sliding, or gameplay reach inflation for cosmetic contact. Qualify native idle,
movement, turns, attacks, death/fade/despawn, quantities, failed attach rollback,
module OFF/ON, fallback and donor isolation. Resource receipts identify exact
private owners versus borrowed immutable runtime assets. Owner aesthetic approval
remains `NOT_PERFORMED_NONBLOCKING` unless supplied.

## Safety, validation, release, and stop

Before every native transaction, snapshot the actual installation, settings,
foreign mods, authorized fixture metadata and protected saves. The immutable
`KMG_AUTOMATION_BASELINE`, human saves and human installation are never test
inputs. Use guarded Steam App ID 640820 requests, current package validation,
transactional deployment/restoration, fresh receipts, and only compatible
disposable fixtures. Do not directly launch the game, manually alter saves,
blanket-remove buffs, clear caches, or load an eleven-mod fixture under a reduced
stack. Recover the exact failed receipt and preserve failure evidence.

Editing uses named focused filters with nonzero matches and production compilation.
After review, freeze one coherent hidden candidate and run one Sprint gate. Do
not duplicate identical broad checks. Native launches require offline wiring and
serialization validation first. Observer repairs are evidence-driven and rerun
affected cases when binary identity changes. Reuse v0.0.150/v0.0.151 evidence only
for named unchanged dependencies.

After hidden qualification, remove only the two suppression keys, regenerate
controlled artifacts, and qualify the exact public artifact and twelve roots.
Merge the ready product PR normally, publish the next unused stable patch, then
download and verify the ZIP, SHA256SUMS, manifest, strict package validity,
DLL/MVID, peeled tag and stable Latest state. Restore the exact machine intake,
clean the authorized fixture, verify no owned process/lease/staging remains, and
synchronize the final RELEASED/STOP record through a narrow documentation PR if
needed. Then stop. Sprint 24 is expressly not started.
