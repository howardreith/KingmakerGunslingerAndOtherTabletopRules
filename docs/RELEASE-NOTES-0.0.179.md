# Tabletop Added Rules v0.0.179

This stable release collects the summoning work merged since v0.0.178:
- the Wall of Ice navigation hardening;
- the Bone Devil Wall correction;
- the summon reach correction under ProperFlanking2.

It adds no creature, route, spell or icon, and changes no creature model,
texture or display scale. All 134 creatures and their 1,217 public summon
choices keep the routes and identities they had in v0.0.178.

## Wall of Ice

The bounded Wall of Ice sections cast by Bone Devils and Ice Devils now hold
their navigation obstruction durably:
- **Both navigation layers.** Each section keeps both of the game's
  navigation layers blocked while it stands, including during turn-based
  movement.
- **Overlapping sections.** When two sections overlap and one is removed, the
  survivor stays blocked, in either removal order.
- **Removal.** Breaking, expiry, source removal and area exit restore the
  ground exactly. A section that is mid-update when it retires finishes
  retiring safely.
- **Steam and tail.** The steam effect is pooled and reused correctly, and the
  Bone Devil's tail presentation is kept.
- **Saves.** Wall sections are not saved into formation data.

Bone Devil Wall sections no longer receive Infernal Wound when struck.

## Reach under ProperFlanking2

ProperFlanking2 treats any melee weapon whose type range is above 2 feet as a
reach weapon, which refuses adjacent targets. In v0.0.178, 45 natural or
ordinary weapon rows on 26 published summons carried creature reach in that
field, so the mod misread them as reach weapons. They now store the ordinary
5 feet, and the creature's own reach is added on the owning creature only.
Live reach distances are unchanged.

The affected creatures include:
- Lemure and Dretch;
- the Hound and Trumpet Archons, and the Astral Deva;
- the Bearded and Barbed Devil claws, the Babau, the Salamander tail and the
  Kyton chain;
- the Hill, Stone, Fire, Cloud and Storm Giants, the Ettin and the Lillend;
- the Hezrou, Vrock, Ice Devil, Glabrezu and Nalfeshnee;
- the Ankylosaurus, Stegosaurus and Brachiosaurus tails.

The Bearded Devil's glaive and the Babau's longspear are genuine reach weapons.
They are unchanged and keep their adjacent dead zone under ProperFlanking2.

## Test and record changes

This release also includes observation-only test code, and fixes to it, that
the game never runs in normal play:
- Sprint 41 inventory-observation repairs;
- the summoned-creature visual-closeout observer.

It also carries the Sprint 40 release-record correction, the Sprint 41 audit
records and the visual-closeout records. None of these changes gameplay.

## Disclosed open items

- **Wall review phase reliability (Sprint 41 A-15).** The guarded Wall of Ice
  review phase passed 1 of 5 runs at the audit source, with four different
  failure modes. Separate development runs on later sources have passed it
  in full. Whether the instability lies in the product or in the review
  fixture's measurement is undetermined. No player-facing symptom has been
  demonstrated.
- **Other open audit findings.**
  - A-5: the Brachiosaurus has no Summon Monster choice.
  - A-6: the roster report counts family coverage from identity, not from
    real choices.
  - A-7: wrapper ladders stop at tier+2.
  - A-9: the compatibility matrix cannot run on a development candidate.
  - A-13: the deep module-OFF sweep is reserved to one named purpose.
  - A-16: roster-scale visual acceptance has not been performed.
- **Not run for this release.**
  - Ice Devil visual acceptance remains pending. Its in-game review frames
    exist only on the designated test machine.
  - The roster-scale persistence matrix and performance measurement were not
    run.
- **Not claimed.** The owner's aesthetic approval of the creature visuals.

Native results for this work remain credited to the development candidates
that produced them. None was re-executed on this version-stamped build.
