# Tabletop Added Rules v0.0.168 — Sprints 30–31 follow-up

This follow-up to v0.0.167 corrects three rules found in the owner's review,
rebuilds three summoned bodies and fixes the Hound Archon's sword grip. It adds
no creature, route or identity; the public roster and its 1,179 choices are
unchanged.

## Rules

- **Astral Deva protective aura as a lesser globe of invulnerability.** Spells
  and spell-like abilities of level 0–3 cast from outside the deva's 20-foot
  aura cannot affect a creature inside it, whether or not they allow spell
  resistance. Persistent spell areas, such as clouds and mists, exclude a
  covered creature until it leaves the aura. Spells of 4th level or higher
  (including heightened spells), spells cast from inside the aura, and the
  constant archon and angel circles are unaffected. The globe no longer works
  through spell resistance, and it needs no other mod.
- **Astral Deva circle protection reaches 20 feet.** The aura functions as
  Magic Circle against Evil over its full 20-foot radius: covered creatures
  gain the circle's configured protection against new control by evil
  creatures. Hound and Trumpet circles keep their 10-foot reach.
- **Trumpet Archon Airborne.** The Trumpet Archon now carries the game's
  ordinary Airborne fact, like the other winged celestials.

The game's own Movanic Deva describes the same protective aura, but its native
aura grants only the +4 deflection and +4 resistance bonuses against evil; this
release does not change it.

## Bodies

- **Lemure, Dretch and Hound Archon** have rebuilt original bodies with
  continuous sculpted surfaces: the Lemure's sagging mound and anguished face,
  the Dretch's swollen belly, bat ears and tusked underbite, and the Hound
  Archon's mastiff head, connected hands and cloth tunic, belt and mantle.
  Their identities, bones, sizes, attachments and teardown are unchanged.
- **Hound Archon sword grip.** The greatsword now sits in the Hound's fist and
  points forward, level in combat and angled down at rest, instead of hanging
  from the wrist through the body.

A creature summoned before updating keeps its old facts until it expires.

## Validation

Guarded native runs on development candidates of this source (Kingmaker
2.1.7b, the Tabletop Added Rules-only profile unless noted), each restoring
the installed mod exactly:

- Astral core, including the lesser globe through the game's own effect
  dispatch and a real persistent spell area: 13 of 13.
- Hound armed and disarmed attacks with the new grip asserted: 9 of 9 and
  12 of 12; Working-save close stills and cleanup of the Hound with all
  installed mods.
- Shared circles 20 of 20, defense consumers 51 of 51, Astral support 9 of 9
  and Astral at-will abilities 6 of 6 with the globe active.
- Earlier on the same branch: Trumpet Airborne and the 20-foot circle reach,
  and the Lemure and Dretch bodies (12 of 12).

The release build passes the complete source, domain and package gate and the
v0.0.168 release validator, which admits no identity change, the unchanged
project, the pinned production files and the pinned body exports.

Known limits: cold-load and persistence, the public route census in game, and
standalone and combined final integration were not rerun for this follow-up.
With Call of the Wild installed, one existing defense test reset fails on
unchanged v0.0.167 too (its Vindicative Smite marker); the generic creature
motion check samples a hidden accessory renderer and fails on unchanged
v0.0.167 as well. Visual review of the bodies is functional; no separate
aesthetic sign-off is claimed.
