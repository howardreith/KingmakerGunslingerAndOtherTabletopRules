# Kingmaker Gunslinger 0.0.111

Release archive:
`KingmakerGunslinger-0.0.111-gunslinger-class-outfit-kitbash.zip`.

## Gunslinger class outfit kitbash

- Gives Gunslinger a coherent native black-powder swashbuckler, frontier-officer,
  and privateer presentation instead of the inherited generic Fighter clothing.
- Uses the installed game's Magus class-clothing base plus one compatible native
  belt/bracer accent for each gender. No proprietary game asset, copied catalog,
  custom mesh, texture, or third-party dependency is added to the package.
- Preserves normal armor, headgear, cloak, backpack, weapon, animation,
  supported-race/gender fallback, and native color-ramp behavior. The default
  ramps are 2/22 and remain player-adjustable.
- The qualifying evidence covers nine supported player races, male and female
  characters, preview-like and ordinary isometric presentation, unarmed,
  pistol, and long-gun states, equipment overrides, native respec/rebuild,
  and a three-launch persisted-outfit transaction.

## Retained behavior

This release retains 0.0.110's Protection from Alignment control-immunity and
player-description work, including its independent UMM setting and documented
scope. It also retains the existing firearm, feature-module, optional-mod, and
save compatibility contracts. It retains the 1,288-test 0.0.103 Overhaul, Summon, and Fatigue controls and the 1,325-test icon-art baseline. No CraftMagicItems.dll is linked or bundled.

The unchanged production firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

## Verification

The complete deterministic suite contains 1,370 tests. The release workflow
performs version-aware source validation, complete domain tests, two clean
deterministic Release builds, strict build-output and UMM-package validation,
and guarded Steam-backed runtime qualification before publication.
