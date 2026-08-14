# Building the Eastern Weapons assets

1. Generate the original sources and icons with installed Blender 4.5:

   `"C:\Program Files\Blender Foundation\Blender 4.5\blender.exe" --background --python assets-source/original-models/eastern-weapons/generate_eastern_weapons.py`

2. Stage the three FBXs and dedicated builder into the authorized project:

   `.\scripts\Prepare-EasternWeaponAssets.ps1`

3. Build with only the exact installed Unity editor:

   `"C:\Program Files\Unity\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1" -executeMethod BuildEasternWeaponsBundle.BuildBatch`

4. Verify `Builds/Windows/kingmakergunslinger.easternweapons` is 181,093
   bytes with SHA-256
   `39884FF681EE553DE957E36E01B350AB926A452F994C4E8D33015D57D4EAD1EC`,
   then stage it at `assets/bundles/kingmakergunslinger.easternweapons`.

The builder rejects every Unity version except 2018.4.10f1. It creates exactly
three uniquely named prefabs, uses opaque Standard materials, removes cameras
and lights, validates finite family-specific bounds, creates `Grip`,
`SupportHandTarget`, `Tip`, and `Butt` anchors, force-rebuilds the bundle, and
does not touch the firearm or spear bundles. Two consecutive builds from the
recorded FBXs produced the exact hash above.

At runtime, all three prefabs load into a candidate dictionary and must pass
the full renderer/material/root/anchor/cardinality contract before publication.
Missing or rejected bundle data preserves the native Kukri, Bastard Sword, or
Falchion family donor without changing blueprint identity, category,
proficiency, mechanics, animation, sockets, trails, or sound.
