# Building the Eastern Weapons assets

1. Generate the project-owned source meshes with installed Blender 4.5.10:

   `$env:PYTHONHASHSEED='0'; & "C:\Program Files\Blender Foundation\Blender 4.5\blender.exe" --background --python assets-source/original-models/eastern-weapons/generate_eastern_weapons.py`

2. Stage the 12 FBXs and dedicated builder into the authorized Unity project:

   `.\scripts\Prepare-EasternWeaponAssets.ps1`

3. Build with only the exact installed Unity editor:

   `"C:\Program Files\Unity\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1" -executeMethod BuildEasternWeaponsBundle.BuildBatch`

4. Verify `Builds/Windows/kingmakergunslinger.easternweapons` is 365,592
   bytes with SHA-256
   `AE311993F683295D3DD996285D28385A20F593DF16903D909818EB4F25A0096B`,
   then stage it at `assets/bundles/kingmakergunslinger.easternweapons`.

The builder rejects every Unity version except 2018.4.10f1 and force-rebuilds
the bundle. It creates exactly 24 prefabs: one held and one independently
calibrated `Stored` prefab for each of the 12 production variants. Every
equipment root is identity transformed. The `Visual` child maps the actual FBX
source basis (`+Z` grip-to-tip, `+Y` blade normal, `-X` cutting edge) into the
measured held or stored basis of Scimitar, Bastard Sword, or Greatsword. Held
translation lands `Grip` on the measured donor grip; stored translation lands
`StoredMount` on the measured donor renderer-center anchor.

Every prefab has renderer-grounded `Grip`, `Tip`, `Butt`, `WeaponForward`,
`BladeNormal`, and `CuttingEdge` markers. Held Nodachi prefabs additionally
carry a butt-side `SupportHandTarget` at the native Greatsword station; stored
prefabs carry `StoredMount` and cannot drive IK. The builder rejects missing,
collinear, reflected, reversed, renderer-disconnected, nonidentity-root, or
wrong-cardinality output. It removes cameras/lights and does not touch the
firearm or spear bundles. Two consecutive forced Unity builds produced the
exact hash above; logs are `eastern-presentation-build-3.log` and
`eastern-presentation-build-4.log` in the authorized Unity project's `Logs`
directory.

At runtime, all 24 prefabs must validate transactionally before publication.
Only `m_WeaponModel` and `m_WeaponBeltModel` are replaced on a clone of the
native donor visual parameters. Animation style, trails, sounds, attachment
slots, sheath, timing, and every other donor field remain unchanged. A runtime-
added Nodachi `EquipmentOffsets` component initializes an empty slot-offset
array so native sheath recreation is safe without adding a root correction.
Missing or rejected bundle data preserves the exact native donor presentation.
