# Kingmaker Gunslinger 0.0.108

> Historical release. Version 0.0.109 supersedes this package with custom
> Martial Performance choices, loaded-firearm repair, and prominent Broken /
> Wrecked feedback; this file retains the exact 0.0.108 archive identity for
> provenance.

Release archive:
`KingmakerGunslinger-0.0.108-icon-art-polish-round-2.zip`.

## Icon art polish Round 2

- Blunderbuss, Musket, and Pistol keep their decorative B/M/P category
  identities, but the letters now occupy about half of the 64-pixel tile and
  no longer dominate neighboring native Weapon Focus glyphs.
- The generated selector backgrounds are full bleed. The generator no longer
  bakes the former dark/gold inset rectangles or corner ornaments into the
  texture, leaving only the normal game-provided row or cell framing.
- The same corrected sprite per official firearm kind remains shared by
  Weapon Focus, Rapid Reload, Greater Weapon Focus, Weapon Specialization,
  Greater Weapon Specialization, Improved Critical, and Gun Training surfaces.
- Cord of Stubborn Resolve uses a new original oblique braided-cord source. Its
  runtime silhouette is 116 by 64 pixels inside the 128-pixel canvas, with a
  visible knot, short hanging ends, front/rear depth, and transparent corners.

## Compatibility and protected scope

The official firearm set remains exactly Blunderbuss, Musket, and Pistol.
Rifle and Revolver remain stable legacy identities for old saves and deliberate
Toy Box use but do not return to ordinary supported selections.

Optional Craft Magic Items compatibility remains reflection-only; the release
does not link or package `CraftMagicItems.dll`.

Rapid Reload, all accepted firearm item art, all accepted Eastern weapon art,
and the Elven Branched Spear art retain their pre-polish SHA-256 hashes.
Blueprint GUIDs, item mechanics, slots, prices, localization, acquisition,
vendor placement, and save behavior are unchanged.

The unchanged production firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

## Qualification

The complete dependency-free deterministic suite contains 1,325 tests. The
version-aware repository gate also runs a focused Round 2 validator covering 30
protected files, three no-frame selector textures, exact source/final hashes,
Cord dimensions and alpha geometry, all ten supplied visual inputs, six curated
full-resolution runtime frames, and Rifle/Revolver retirement.

This release retains the 1,288-test 0.0.103 baseline, the 1,307-test 0.0.104
summon repair, the 1,315-test 0.0.105 presentation baseline, and the 1,323-test
0.0.106 fatigue-authority baseline.

The release passed repository validation, the complete domain suite, a clean
Release build, build-output and SoundBank checks, deterministic packaging,
strict standalone UMM package validation, guarded visual evidence, dependent
firearm-feat mechanics, Cord mechanics, and the exact
`KMG_AUTOMATION_WORKING` smoke.

The six 1920x1200 after frames are guarded in-game Unity renders of actual
loaded blueprint sprites in deterministic UI facsimiles. They are supporting
perceptual evidence, not screenshots of automated native-menu navigation;
structured guarded scenarios provide the mechanical evidence.
