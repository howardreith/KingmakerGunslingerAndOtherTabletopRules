# Kingmaker Gunslinger 0.0.142: Elemental race traits and content polish

Candidate informational version: 0.0.142-elemental-race-traits-and-content.

This candidate starts at exact released v0.0.141 commit
97f0a966b3219ce0122626a492529b509e1db880. Its Sprints 9–11 checkpoint
is preserved unchanged, including the already hidden later groundwork. No
post-v0.0.141 Expanded Summoning or Phase 2B development is imported.

The four character race traits are intended for Favored Class racial_traits
when its supported contract, traits setting and Elemental Races module are enabled:

- Fiery Glare (Ifrit): an optional free Intimidate toggle, initially off. A result
  of 10 is used when it succeeds; otherwise the check rolls, including in combat.
- Stoic Dignity (Oread): while conscious, +1 Trait against new mind-affecting
  effects, and +1 Morale for other allies within 10 feet. An exact already
  present effect suppresses its bonus; unrelated effects do not.
- Aerial Observer (Sylph): +2 Trait Perception while the exact KMG Wings of Air
  effect is active. Generic flight and visual wings do not qualify.
- Whiteout (Undine): outdoors in actual Rain/Snow at Light or greater, an
  independent 10% miss check follows successful native concealment handling.
  Concealment-ignoring attacks, including authorized Seeking, bypass it.
  Magical fog, waterfall spray and nonattack damage do not qualify. Native
  20% concealment followed by Whiteout gives 28% combined misses.

Firearm descriptions use consistent ordinary prose, including shared Reliable
and Seeking descriptions and lead-ball-specific Blunderbuss penetration.
Model D stocks Oleg with one each of the three mundane firearms and 50 matched
shots; the capital smith with +1 firearms, 200 of each ammunition item and a
Gunsmith's Kit; and Bokken with 100 of each ammunition item. Regional, BTSL,
Better Vendors, named-loot and Skeletal Salesman paths are preserved.
Already serialized merchant inventories are not rewritten.

Nodachi was already eligible for Heirloom Weapon and is not new. Earthsense
and Lunge are not included. The two released catalog authority hashes were
corrected as metadata only; their referenced released production bytes did
not change.

Automated qualification is in progress. This file is not a shipping claim.
Owner icon/contact-sheet, tooltip and merchant-screen acceptance remain pending.
No merge, tag or GitHub release is authorized by this candidate.

Candidate archive: `KingmakerGunslinger-0.0.142-elemental-race-traits-and-content.zip`. The inherited firearm SoundBank remains byte-identical, SHA-256 `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

Existing `CraftMagicItems.dll` compatibility remains inherited from the released checkpoint; this candidate introduces no new crafting integration.

Current candidate validation registers 2,252 domain tests. Runtime and persistence qualification remain pending.

Historical inherited gates still preserve the earlier 1,288-test content checkpoint; that historical count does not describe this candidate.
Historical fatigue-authority evidence remains at its recorded 1,325 tests; no prior runtime artifact is requalified by this count.
