# v0.0.152 — Armored Dinosaurs

`0.0.152-expanded-summoning-sprint23`

This patch adds Triceratops and Stegosaurus to Expanded Summoning while
preserving every previously released creature, route, template child, GUID and
save identity. Triceratops appears on Summon Monster VI and Summon Nature's
Ally VI; Stegosaurus appears on Summon Nature's Ally VI only. The twelve new
roots use the existing quantity policy: one at VI, 1d3 at VII and 1d4+1 at
VIII and IX. Triceratops's four Monster roots receive the established
celestial/fiendish execution children; Nature's Ally roots do not.

Triceratops has one primary gore, live-size 2d10 damage, Improved Critical
(gore), a real-charge-only 4d10 powerful charge, and the released enemy-aware
trample response policy with d8 damage and live Strength/HD arithmetic.
Stegosaurus has one primary 4d6 spiked tail and one native trip maneuver on an
exact successful tail strike. The trip is neither an automatic knockdown nor
the Ankylosaurus daze/stun rider. Both creatures derive Power Attack at their
own BAB and retain ordinary attacks when their special actions are unavailable.

Both creatures ship distinct, deterministic, editable project-owned bodies,
paintings and icons. Triceratops uses the captured Mastodon rig while retaining
a three-horned frill silhouette and forward contact. Stegosaurus uses the
captured Hodag rig with a small head, raised hindquarters, dorsal plates and a
long four-spiked tail; it retains no club. Native donor meshes, textures and
animations are not redistributed or mutated.

The registered catalog contains 105 project creatures and 1,075 generated
placements: 534 on Summon Monster and 541 on Summon Nature's Ally. With 29
retained native wrappers, publication exposes 1,104 visible choices.

Accepted engine limitations are `PASSIVE_CREATURE_SENSES_UNMODELED` for
low-light vision and scent, `IMPROVED_OVERRUN_NATIVE_CONSUMER_UNAVAILABLE` for
Stegosaurus, and `RUN_NATIVE_SUMMON_AI_DISPOSITION` for Triceratops. No
substitute senses, general overrun framework or invented Run action is added.
The inherited trample targeting policy is enemy-aware and automatically uses
one legal native opportunity attack at the established penalty, otherwise a
Reflex save for half; it never applies both responses to one contact.

Owner aesthetic approval remains `NOT_PERFORMED_NONBLOCKING`. Sprint 24 is not
part of this release.
