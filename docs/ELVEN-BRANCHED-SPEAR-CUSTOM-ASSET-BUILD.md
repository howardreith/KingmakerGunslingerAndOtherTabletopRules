# Building the Elven Branched Spear asset

1. Run Blender 4.5 in background mode:

   `blender --background --python assets-source/original-models/elven-branched-spear/generate_elven_branched_spear.py`

2. Stage the result into the exact existing Unity project:

   `.\scripts\Prepare-ElvenBranchedSpearAssets.ps1`

3. Run the installed Unity 2018.4.10f1 editor:

   `"C:\Program Files\Unity\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1" -executeMethod BuildElvenBranchedSpearBundle.BuildBatch`

4. Verify the emitted
   `Builds/Windows/kingmakergunslinger.elvenbranchedspear` SHA-256 is
   `33EB89C74EC4AE7CDA5A8155224A449233904B74CB59FC453C24AE022EE3CB2A`,
   then stage it as `assets/bundles/kingmakergunslinger.elvenbranchedspear`.

The dedicated builder rejects every Unity version except 2018.4.10f1 and emits
six uniquely named prefabs: one held and one back-carry prefab for each classic,
thorn, and crown mesh. It uses opaque Standard materials and removes cameras
and lights. Held `Visual` maps source `+Z` through `+90 X` to installed native
Longspear forward `-Y`; each separately referenced BeltModel uses an upper-left
diagonal frame. The builder validates the unchanged 2.28 m geometry and emits
Grip, SupportHandTarget, Tip, and Butt anchors for held models plus BackMount,
Tip, and Butt for back models. It does not touch the firearm bundle.

At game startup, the dedicated runtime loads all three held/back pairs into a
candidate cache and validates every complete render/anchor/frame contract. Only
then does it atomically publish the set. Missing, corrupt, partial,
nonrenderable, or implausible bundles leave the native Longspear presentation
active. Saves contain blueprint identities, never prefab state. The prior
three-prefab bundle hash
`F671904DDB492EA194C259889D18BC4916E161E107C5E9F179A375DDF87B5B85` is retained
here only as superseded build history.
