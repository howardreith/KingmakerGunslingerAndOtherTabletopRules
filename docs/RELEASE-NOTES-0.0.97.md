# Kingmaker Gunslinger 0.0.97

This patch release records the completed compatibility-attribution audit for
Pathfinder: Kingmaker 2.1.7b and retains narrowly guarded diagnostics used to
distinguish KMG-owned asset behavior from game and third-party log noise.

## Compatibility attribution audit

- Favored Class / Helpful: the reported
  `ComponentAppliedOnceOnLevelUp.OnFactActivate` exception was not reproduced
  in the controlled negative or positive configurations. KMG's Helpful
  publication remained structurally valid, transactional, optional-dependency
  safe, and exactly once. The four observed Favored Class JSON startup failures
  did not reference KMG-owned blueprint identities and are external to KMG.
- Polymorph / view teardown: apply, replace, restore, deactivation, disposal,
  and `UnitFxVisibilityManager.Update` exception fingerprints were all zero in
  repeated behavior-negative and behavior-positive fresh processes. No KMG
  production defect was established.
- Asset warnings: every reproduced unsupported-shader, particle mesh/read-write,
  missing-script, lightmap-mode, and missing-`_MainTex` fingerprint remained
  present with all KMG bundle families suppressed. KMG's complete firearm,
  Elven Branched Spear, and Eastern Weapons inventories contained none of those
  defects. The reported zero-surface-area family was not reproduced.

The retained asset-attribution path is available only through an accepted
guarded runtime-test request. It is process-local, does not patch third-party
behavior, does not write saves or `FeatureModules.json`, and is inactive during
ordinary gameplay. The full evidence and remaining uncertainty are recorded in
`docs/KMG-COMPATIBILITY-ATTRIBUTION-AUDIT.md`.

No blueprint identity, item mechanic, balance rule, production asset, or
third-party implementation changed. The qualified firearm SoundBank remains
byte-identical at SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

## Compatibility

- Pathfinder: Kingmaker Enhanced Plus Edition 2.1.7b
- Unity Mod Manager 0.32.4 / supported 0.32.x line
- Harmony 1.2 through `0Harmony12.dll`
- .NET Framework 4.7
- Windows Steam installation used for qualification

The mod remains standalone. Call of the Wild is required only for the optional
Brown-Fur Transmuter module. This release does not claim compatibility with
every version of every third-party mod.

## Updating

Close Kingmaker, back up affected saves and the installed mod, then install the
complete `KingmakerGunslinger-0.0.97-compatibility-attribution-audit.zip`
through Unity Mod Manager. Do not overlay individual files onto an older
installation, and do not download GitHub's automatically generated source
archives as the mod package.

## Save warning

Kingmaker Gunslinger publishes save-owned classes, archetypes, feats, spells,
buffs, items, weapon categories, summons, enchantments, resources, and firearm
state identities. Keep this version or a compatible newer version installed for
campaigns that use its content. Uninstalling the complete mod from such a
campaign is not generally safe.
