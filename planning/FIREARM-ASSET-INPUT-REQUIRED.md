# Firearm Model Asset Input Required

KingmakerGunslinger 0.0.61 has no legally usable firearm mesh in the authorized
local workspace. The audit found no FBX, OBJ, DAE, glTF/GLB, Blender, Unity
asset, or asset-bundle source. No asset was extracted from Cowboys and Demons
or from Kingmaker/Wrath compiled content.

## Accepted source input

- An original or lawfully reusable flintlock-style pistol in FBX, OBJ, or DAE
  form. A `.blend`, `.gltf`, or `.glb` source is acceptable only if it can be
  exported losslessly to one of those Unity 2018 import formats.
- A license allowing redistribution, modification, and compiled game-mod use.
  CC0, CC BY 4.0, or a project-owned original is preferred. CC BY-SA or a
  custom license requires a compatibility review before import.
- The original source URL, author, license text/link, acquisition date, and
  required attribution. “Free download” without an explicit license is not
  sufficient.
- One mesh at sensible real-world scale, with the grip near the origin and a
  documented forward/up axis. Separate trigger/hammer pieces are optional.
- PNG or TGA base-color textures. Normal and metallic/specular maps are
  optional. Textures must be redistributable under the same compatible terms.
  A practical maximum is 2048x2048; 1024x1024 is preferred for this isometric
  game.

## Kingmaker import contract

1. Use Unity `2018.4.10f1`, the exact engine version reported by the qualified
   Kingmaker installation.
2. Import mesh and textures without embedding an undeclared external path.
3. Use a Kingmaker-compatible non-Wrath shader/material; validate that no
   material renders magenta.
4. Orient and scale the pistol so its grip follows the native weapon hand
   anchor. Reusing the crossbow animation controller is allowed; the crossbow
   mesh, limbs, string, stock, and bolt are not.
5. Record the muzzle transform/position for future effects, even if 0.0.61 does
   not attach a muzzle effect.
6. Build a Kingmaker-compatible asset bundle, declare it in the mod package,
   load it without Wrath-only patches, and assign the Early Pistol blueprint's
   model reference to the bundled firearm object.
7. Verify weapon-set changes remove the model cleanly, uninstall leaves no
   files outside the mod directory, and no absolute machine path is serialized.
8. At ordinary isometric zoom, visually confirm a credible one-handed grip,
   no floating model, no crossbow geometry, correct icon, and no missing
   material.

Candidate sources may be sought from reputable original repositories such as
OpenGameArt or Sketchfab only by filtering for explicit CC0/CC BY assets and
then verifying the license on the individual original asset page. This is a
search direction, not a claim that any particular candidate is licensed or
technically suitable.
