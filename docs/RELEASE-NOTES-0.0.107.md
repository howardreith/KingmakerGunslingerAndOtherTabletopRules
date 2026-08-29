# Kingmaker Gunslinger 0.0.107

Release archive:
`KingmakerGunslinger-0.0.107-icon-art-overhaul.zip`.

## Official firearm support

- Official firearm support is exactly Blunderbuss, Musket, and Pistol across
  Rapid Reload, native weapon feats, Gun Training, starting access, vendors,
  loot, and Craft Magic Items creation.
- Stable Rifle and Revolver identities remain registered for existing saves
  and deliberate Toy Box use, but are hidden from ordinary selection,
  acquisition, and crafting surfaces.
- Existing legacy-owned items retain their historical mechanics; no stable
  blueprint GUID was removed or reassigned.

## Native-style icon overhaul

- Rapid Reload uses a larger transparent muted-red circular arrow and tool
  motif without the former blue corner decorations.
- Blunderbuss, Musket, and Pistol use centralized full-square decorative B/M/P
  selector monograms in Rapid Reload, native weapon feats, and Gun Training.
- Supported firearm items use transparent lower-left-to-upper-right silhouettes
  without the former dark rectangular backgrounds.
- All 30 mod-added Eastern weapon items and all 12 Elven Branched Spear items
  resolve to corrected diagonal-fill textures.
- The repository includes original high-resolution sources, deterministic
  downsampling and model-render tools, alpha/dimension validation, five curated
  1920x1200 live-sprite frames, and a before/after contact sheet.

## Compatibility and scope

The release retains the 0.0.106 native fatigue-authority repair and all earlier
qualified gameplay behavior. It does not redesign unrelated class, grit, deed,
spell, feat, item, animation, audio, or save systems.

Optional Craft Magic Items compatibility remains reflection-only. The package
does not link or include `CraftMagicItems.dll`. From-scratch firearm bases are
exactly Pistol, Musket, and Blunderbuss; already-owned legacy firearms remain
recognizable for safe upgrades.

The unchanged production firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

## Qualification

The complete dependency-free deterministic suite contains 1,325 tests. The
icon asset gate validates the exact 14-file final set, three selector icons,
ten item textures, source/final dimensions, alpha bounds, transparent corners,
blueprint references, and retired monogram absence.

This release retains the 1,288-test 0.0.103 baseline, the 1,307-test 0.0.104
summon repair, the 1,315-test 0.0.105 presentation baseline, and the 1,323-test
0.0.106 fatigue-authority baseline.

The implementation passed version-aware repository validation, a clean Release
build, strict build-output and SoundBank validation, deterministic package
creation, strict standalone UMM package validation, guarded native level-up and
vendor assertions, guarded presentation observation, and the exact
`KMG_AUTOMATION_WORKING` save smoke. The release publisher performs two clean
builds and requires byte-identical package and DLL hashes before publication.

The five curated frames are in-game Unity renders built from the actual loaded
blueprint names and sprite objects. They are supporting perceptual evidence;
the separate guarded native level-up, vendor, presentation, and save-backed
scenarios provide mechanical evidence.
