# Building the Elven Branched Spear asset

1. Run Blender 4.5 in background mode:

   `blender --background --python assets-source/original-models/elven-branched-spear/generate_elven_branched_spear.py`

2. Stage the result into the exact existing Unity project:

   `.\scripts\Prepare-ElvenBranchedSpearAssets.ps1`

3. Run the installed Unity 2018.4.10f1 editor:

   `"C:\Program Files\Unity\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1" -executeMethod BuildElvenBranchedSpearBundle.BuildBatch`

4. Verify the emitted
   `Builds/Windows/kingmakergunslinger.elvenbranchedspear` SHA-256 is
   `6E9FE86E43072361EEC3357D9C73E17ADD71D22BAF257FB8C7ED6F52931CE777`,
   then stage it as `assets/bundles/kingmakergunslinger.elvenbranchedspear`.

The dedicated builder rejects every Unity version except 2018.4.10f1, creates
three uniquely named prefabs, uses opaque Standard materials, removes cameras and
lights, validates finite plausible bounds, and emits Grip, SupportHandTarget,
Tip, and Butt anchors. It does not touch the firearm bundle.

At game startup, the dedicated runtime loads into a candidate cache, requires
the exact classic, thorn, and crown prefabs and validates every complete
render/anchor contract. Only then does it atomically publish the set. Missing,
corrupt, partial, nonrenderable,
or implausible bundles leave the native Longspear model active. Saves contain
blueprint identities, never prefab state.
