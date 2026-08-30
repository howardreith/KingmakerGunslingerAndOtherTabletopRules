# Kingmaker Gunslinger 0.0.110

Release archive:
`KingmakerGunslinger-0.0.110-protection-from-alignment-control-immunity.zip`.

## Protection from Alignment control immunity

- Protection from Evil, Good, Law, and Chaos now prevent a new explicitly
  registered domination, charm, or comparable mental-control effect when the
  actual source has the alignment opposed by that protection.
- A lawful evil source is blocked by either Protection from Law or Protection
  from Evil. A neutral or good source is not blocked by Protection from Evil.
- The target-side Kingmaker `RuleApplyBuff` seam checks both the delivery
  ability and the terminal control buff. This covers direct terminal-buff
  delivery, monster abilities, the mod's Expanded Summoning Succubus, and
  wrappers that share a patched protection buff.
- Qualification uses a small explicit catalog. Fear, Confusion, sleep, daze,
  paralysis, Hold Person, ordinary morale effects, beneficial mind-affecting
  effects, and other unregistered effects are not blocked merely because they
  carry a broad descriptor.
- A missing or unclassifiable source fails open. Optional Call of the Wild
  registrations remain late-bound and cannot prevent the base mod from
  starting when that mod is absent or changes.

## Player-facing spell and buff descriptions

- The generic Protection from Alignment spell, its communal form, all four
  individual alignment spells, and all four communal alignment spells now
  explain the matching-alignment control immunity.
- The four shared terminal protection buffs and the Paladin protection-aura
  effect buff now carry matching tooltip text, so scroll, potion, item, class,
  and other delivery paths that share those buffs expose the active behavior.
- The descriptions retain the ordinary +2 deflection Armor Class and +2
  resistance saving-throw benefits and identify the control protection as the
  set of effects recognized by this mod.
- Description publication is transactional and idempotent. Repeated blueprint
  initialization does not duplicate records, and a failed publication restores
  the prior descriptions and components when safe.

## Independent feature setting

`Protection from Alignment: control immunity` is an independent, default-on
checkbox in the Unity Mod Manager panel. Like the neighboring feature modules,
its saved value applies after a complete game restart. When disabled at startup,
Kingmaker retains its vanilla control behavior and vanilla protection
descriptions; unrelated Gunslinger modules continue independently.

## Important Wrath-parity limitation

An already-active domination, charm, or other control effect remains active
when Protection from Alignment is applied. The mod does not remove, suppress,
pause, replace, or grant a new saving throw against an existing effect, and
removing protection does not alter that effect. This release intentionally does
not claim the complete Pathfinder tabletop paragraph.

## Compatibility and qualification

The implementation has no new third-party dependency and no global Harmony
patch. It retains the exact five protected terminal buffs, 13 registered
control abilities, eight registered terminal control buffs, and optional
reflection-free blueprint resolution for audited Call of the Wild identities.
Existing Craft Magic Items compatibility remains late-bound; the release does
not link or package `CraftMagicItems.dll`.
The unchanged production firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

The complete deterministic suite contains 1,359 tests. The release workflow
also validates the full project, clean Release build, deterministic package,
and live enabled/disabled blueprint inventory through the guarded Steam test
mechanism. A complete encounter-by-encounter domination matrix still requires
ordinary in-game play; the owner has authorized this release while continuing
campaign validation on another computer.

This release retains the 1,288-test 0.0.103 baseline, the 1,307-test 0.0.104
summon repair, the 1,315-test 0.0.105 presentation baseline, the 1,323-test
0.0.106 fatigue-authority baseline, the 1,325-test 0.0.107/0.0.108 art
baseline, and the 1,348-test 0.0.109 mechanics baseline.
