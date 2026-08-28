# Kingmaker Gunslinger 0.0.105

Candidate archive:
`KingmakerGunslinger-0.0.105-player-facing-presentation-item-discoverability.zip`.

## Player-facing presentation

- Brown-Fur Transmuter now appears after Call of the Wild's standalone
  Arcanist archetypes and before the five installed combined archetypes.
- Project weapon, enchantment, Finesse Training, and Cord tooltips use direct
  player rules instead of implementation terminology. The Eastern proficiency
  policy enchantment remains mechanically active but has no visible text.
- The guarded presentation observer audits 55 project weapons, the Cord, and
  12 project enchantments for complete, clean localized text.

## Item discoverability

- Cord of Stubborn Resolve is now in
  `RichHuman_treasure_chest_04 (1)` inside `CapitalTavern_Indoor`; its
  former `CapitalSquareVillage` row is explicitly removed.
- All 30 project-added magic items remain fixed loot, distributed across 30
  exact containers in 29 persistent campaign areas. No active target depends
  on a temporary area variant or an implementation-named hidden/cache target.
- Retired targets are cleanup-owned, count-one/zero validated, and included in
  foreign-mutation-safe rollback.

Build and deterministic tests do not by themselves prove organic in-game
discoverability or presentation. Exact guarded runtime results are recorded in
`docs/PLAYER-FACING-PRESENTATION-AND-ITEM-DISCOVERABILITY-QUALIFICATION.md`.

Optional Craft Magic Items compatibility remains reflection-only; the package
does not link or include `CraftMagicItems.dll`.

The unchanged production firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

The candidate retains the 1,288-test 0.0.103 baseline and the 1,307-test
0.0.104 summon repair; its complete deterministic suite contains 1,315 tests.
