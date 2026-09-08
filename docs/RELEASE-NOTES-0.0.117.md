# Kingmaker Gunslinger 0.0.117

Elemental Races gains twelve heritage choices, eleven racial feats and
nineteen implemented alternate racial traits. Heritage and alternate racial
choices appear in the racial Heritage section before ability allocation and
skills. Ordinary ZFavoredClass background Traits retain their separate,
populated selections.

Informational version: `0.0.117-elemental-char-gen-stabilization`.
UMM displays version **0.0.117**.

## Heritage and alternate racial choices

| Race | Heritage choices | Alternate racial traits |
| --- | --- | --- |
| Ifrit | General, Lavasoul, Sunsoul | Wildfire Heart; Brazen Flame; Fire in the Blood; Efreeti Magic; Forge-Hardened; Fire Insight |
| Oread | General, Gemsoul, Ironsoul | Crystalline Form; Earth Insight; Granite Skin; Stone in the Blood |
| Sylph | General, Smokesoul, Stormsoul | Air Insight; Breeze-Kissed; Like the Wind; Secretive; Storm in the Blood; Thunderous Resilience; Whispering Wind |
| Undine | General, Mistsoul, Rimesoul | Acid Breath; Ooze Breath |

Alternate traits replace Energy Resistance, Elemental Affinity or the racial
spell-like ability as described. Every required slot retains a legal keep-base
choice. Overlapping replacements cannot be combined. Heritage reselection,
back-navigation and native respec reconstruct racial modifiers and providers
without duplicating benefits or restoring spent daily uses.

Treacherous Earth and Nereid Fascination remain deferred and unavailable in
character creation or respec. Their registered identities remain resolvable.
No favored-class bonuses or additional top-level races are introduced.

## Elemental feats

The feat catalog includes Elemental Strike; Scorching Weapons, Inner Flame,
Blazing Aura and Firesight; Airy Step, Wings of Air, Cloud Gazer and Inner
Breath; Hydraulic Maneuver and Triton Portal. Existing race, level and feat
prerequisites apply. Feats remain available through their normal selectors.

## Compatibility and persistence

- Both ordinary ZFavoredClass Trait selections retain their categories and
  choices. Helpful appears once in Combat when Bodyguard Feats is enabled;
  disabling Bodyguard removes Helpful while preserving the foreign catalog.
- Point-buy and Dice Roller ownership survive preview reconstruction and
  back-navigation. Races Unleashed heritage and alternate-trait selectors remain
  separate and intact.
- Save/load, module OFF/ON, rest, level-up, native respec and physical lifecycle
  checks cover every visible implemented trait. Existing blueprint GUIDs,
  legacy General heritage behavior and spent racial resources are preserved.
- Breeze-Kissed and both breath abilities have verified native turn-based action
  costs. Crystalline Form uses the reviewed semantic ray catalog.
- The previous release's Roadwarden, Dead Reckoning, Skeletal Salesman stock and
  Protection from Alignment wording remain included.

Known ZFavoredClass missing custom-data exceptions remain unchanged and were
also observed with production KMG disabled. This release does not perform a
general ZFavoredClass repair. The original empty Trait screen's exact trigger
was not conclusively attributed; the repaired foreign selector contract and
completed native character-creation routes are independently verified.

## Acceptance and verification

The owner accepted the installed candidate and explicitly authorized
finalization, merge to master, push and this public release. The accepted
candidate passed all **1,458** domain/reflection tests, **259** runtime preflight
checks and **13,847** assertions across **28** fresh guarded Steam processes.
Release sealing preserves the gameplay source, rebuilds deterministically and
checks the final artifact separately. Exact release fingerprints, runtime
results and public-download verification are recorded in the
[release report](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/master/docs/ELEMENTAL-RACES-0.0.117-PUBLIC-RELEASE.md).

Installable asset: **KingmakerGunslinger-0.0.117-elemental-char-gen-stabilization.zip**.
The package contains no proprietary game or Unity reference assemblies.

The firearm SoundBank retains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Optional Craft Magic Items integration remains reflection-only;
`CraftMagicItems.dll` is neither linked nor packaged. The inherited 1,288-test
overhaul and 1,325-test fatigue-authority checkpoints remain historical evidence.
