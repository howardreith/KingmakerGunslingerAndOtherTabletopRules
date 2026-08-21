# Building the Elven Branched Spear asset

1. Run Blender 4.5 in background mode:

   `blender --background --python assets-source/original-models/elven-branched-spear/generate_elven_branched_spear.py`

2. Stage the result into the exact existing Unity project:

   `.\scripts\Prepare-ElvenBranchedSpearAssets.ps1`

3. Run the installed Unity 2018.4.10f1 editor:

   `"C:\Program Files\Unity\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Dev\KingmakerGunslingerLab\unity-asset-build\KingmakerGunslinger-2018.4.10f1" -executeMethod BuildElvenBranchedSpearBundle.BuildBatch`

4. Verify the emitted
   `Builds/Windows/kingmakergunslinger.elvenbranchedspear` SHA-256 is
   `A59DC61CE246A7F5931F22494C4C52CE39C6E96312F3448FB9138A0AC0D7DC9B`,
   then stage it as `assets/bundles/kingmakergunslinger.elvenbranchedspear`.

The dedicated builder rejects every Unity version except 2018.4.10f1 and emits
six uniquely named prefabs: one held and one back-carry prefab for each classic,
thorn, and crown mesh. It uses opaque Standard materials and removes cameras
and lights. Each FBX must contain exactly one source-authored `KMG_Grip`,
`KMG_Support`, `KMG_Tip`, `KMG_Butt`, `KMG_HeadUp`, and `KMG_Back` marker.

The builder solves the source +Z/+Y basis into the measured native Longspear
held and stored bases and solves translation from the appropriate source/target
anchor. It validates the unchanged 2.28 m semantic span, renderer-bound physical
ends, head plane normal, +0.593016 m support station, identity root, and
independent BackMount. Every final prefab exposes Grip, SupportHandTarget, Tip,
Butt, and HeadUp; back prefabs additionally expose BackMount. It does not touch
the firearm bundle.

At game startup, the dedicated runtime loads all three held/back pairs into a
candidate cache and validates every complete render/anchor/frame contract. Only
then does it atomically publish the set. Missing, corrupt, partial,
nonrenderable, or implausible bundles leave the native Longspear presentation
active. Saves contain blueprint identities, never prefab state. The prior
six-prefab Euler-calibrated bundle hash
`33EB89C74EC4AE7CDA5A8155224A449233904B74CB59FC453C24AE022EE3CB2A` is retained
here only as superseded build history.
