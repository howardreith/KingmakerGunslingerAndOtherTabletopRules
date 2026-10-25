"""Original Lemure body, bound to the measured Lizardfolk Free Hands rig.
Only geometry-free bone interfaces are read from the private capture. Native
meshes, materials, textures and animations are neither copied nor exported.
"""
import argparse
import json
from pathlib import Path
import sys
import bpy
import numpy as np
from mathutils import Vector
sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parent / "continuous-bodies"))
sys.path.insert(0, str(HERE.parent / "sprint24-theropods"))
import continuous_body as cb
import generate_theropods as donor
AXES = (Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1)))
UP, FRONT = AXES[2], AXES[1]
HEAT = {"Torso_Lower", "Torso_Upper", "neck", "neck1", "Head"} | {
    side + name for side in ("L", "R")
    for name in ("_Arm_Upper", "_Arm_Lower", "_Palm")}


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    volume = cb.Volume(.022)
    # A legless, slumped mass. Its skirt follows the pelvis rather than the
    # donor's legs or tail; claw arms retain the actual native attack drivers.
    volume.ellipsoid(Vector((0, -.03, .37)), AXES, (.55, .47, .34))
    volume.ellipsoid(Vector((0, -.02, .83)), AXES, (.39, .34, .63))
    volume.ellipsoid(Vector((0, .01, 1.27)), AXES, (.34, .27, .34))
    volume.sweep([Vector((0, .02, 1.34)), at("neck1"), at("Head")],
                 [.25, .20, .20])
    volume.ellipsoid(at("Head") + FRONT * .05, AXES, (.23, .22, .25))
    # Uneven low lobes make the amorphous base readable at ordinary scale.
    for x, y, r in ((-.36, .17, .23), (.36, .09, .24), (-.17, -.30, .21),
                    (.19, -.31, .23), (0, .33, .25)):
        volume.ellipsoid(Vector((x, y, .21)), AXES, (r, r, .18))
    for side in ("L", "R"):
        shoulder, elbow, palm = [at(side + n) for n in
                                  ("_Arm_Upper", "_Arm_Lower", "_Palm")]
        volume.sweep([shoulder, elbow, palm], [.19, .135, .105])
        volume.ellipsoid(palm + Vector((0, 0, -.055)), AXES, (.11, .085, .15))
    body = volume.polygonise("LemureBody")
    cb.finish_surface(body, 4200, smooth_iterations=8)
    positions = [v.co.copy() for v in body.data.vertices]
    rows = cb.heat_weights(body, armature, HEAT)
    rows = cb.clean_weights(cb.smooth_weights(body, rows), positions, armature, HEAT)
    # No leg weights: the low mass cannot split into the donor's two legs.
    for index, point in enumerate(positions):
        if point.z < .62:
            rows[index] = [("Torso_Lower", 1.0)]
    parts = [(body, rows, 0)]

    def rigid(obj, bone, region):
        parts.append((obj, cb.rigid_rows(obj, [(bone, 1.0)]), region))
    head = at("Head")
    for sign in (-1, 1):
        # Deep, small eyes and a drooping mouth; no horns or reptile muzzle.
        rigid(cb.blob("LemureEye", head + Vector((sign * .105, .244, .055)),
                      AXES, (.035, .022, .025), 12, 8), "Head", 1)
    rigid(cb.blob("LemureMouth", head + Vector((0, .259, -.083)),
                  AXES, (.105, .018, .041), 16, 8), "Head", 1)
    for side in ("L", "R"):
        palm = at(side + "_Palm")
        for suffix in ("_finger1", "_finger2", "_Bfinger2"):
            bone = side + suffix
            tip = at(bone)
            root = palm.lerp(tip, .28)
            rigid(cb.strand("LemureFinger", [root, root.lerp(tip, .6), tip],
                            [.048, .037, .023], 8), bone, 0)
            direction = (tip - palm).normalized()
            rigid(cb.horn("LemureClaw", tip - direction * .02,
                          tip + direction * .105 + FRONT * .045,
                          .029, 10, 4, hint=FRONT), bone, 2)
    result, weights = cb.assemble("LemureOriginal", parts)
    cb.require_single_body(result)
    return result, weights


def colours(p, n, region):
    broad = cb.fbm(p, 5.0, 301)
    fine = cb.fbm(p, 34.0, 302)
    base = np.array([.67, .59, .51]) * (.82 + .25 * broad[:, None])
    base += np.array([.09, -.025, -.035]) * np.clip(fine - .5, 0, 1)[:, None]
    base = np.where((region == 1)[:, None], np.array([.075, .035, .025]), base)
    return np.where((region == 2)[:, None], np.array([.28, .20, .15]), base)


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out", "preview"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = donor.measured_rig(args.capture)
    armature = cb.review_armature(rig_data, "LemureReviewRig")
    body, weights = build(rig, armature)
    cb.unwrap(body)
    cb.bake_albedo(body, 1024, colours, args.albedo)
    cb.attach_albedo_preview(body, args.albedo)
    extra = {"creature": "lemure", "donorFamily": "lizardfolk-free-hands",
             "nativeDonor": donor.DONOR_GUID, "nativePrefab": donor.PREFAB_GUID,
             "bodyRenderer": donor.BODY_RENDERER,
             "accessoryRenderer": donor.ACCESSORY_RENDERER,
             "nativeRendererCount": 2, "visibleArms": 2, "visibleLegs": 0,
             "visibleTail": False, "amorphousLowerBody": True}
    summary = cb.write_mesh_data(args.mesh_data, body, weights, rig_hash,
                                 args.albedo,
                                 "donor renderer local; +X lateral, +Y forward, +Z up",
                                 extra)
    report = {"schemaVersion": 1, "builder": "build_lemure.py",
              "rigSha256": rig_hash, **extra, **summary,
              "albedo": cb.albedo_manifest(args.albedo)}
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    groups = {}
    for index, row in weights.items():
        for name, value in row:
            group = groups.get(name) or body.vertex_groups.new(name=name)
            groups[name] = group
            group.add([index], value, "REPLACE")
    modifier = body.modifiers.new("Armature", "ARMATURE")
    modifier.object = armature
    body.parent = armature
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.device = 'CPU'
    scene.cycles.samples = 16
    scene.world = scene.world or bpy.data.worlds.new("LemurePreviewWorld")
    scene.world.color = (.18, .18, .18)
    bpy.ops.object.camera_add(location=(3.1, 4.8, 3.0))
    camera = bpy.context.object
    camera.rotation_euler = (Vector((0, 0, 1.0)) - camera.location).to_track_quat('-Z', 'Y').to_euler()
    camera.data.type = 'ORTHO'
    camera.data.ortho_scale = 2.65
    scene.camera = camera
    bpy.ops.object.light_add(type='AREA', location=(2.5, 3.5, 5.0))
    light = bpy.context.object
    light.data.energy = 550
    light.data.shape = 'DISK'
    light.data.size = 4
    light.rotation_euler = (-light.location).to_track_quat('-Z', 'Y').to_euler()
    scene.render.resolution_x, scene.render.resolution_y = 640, 640
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    scene.render.filepath = args.preview
    bpy.ops.render.render(write_still=True)
    print('[lemure] ' + json.dumps(report, sort_keys=True))


if __name__ == '__main__':
    main()
