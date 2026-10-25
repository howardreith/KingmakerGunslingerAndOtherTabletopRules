"""Original canine-headed Hound Archon body on the measured free-hands rig; no native art copied."""
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
    side + name for side in ("L", "R") for name in (
        "_Arm_Upper", "_Arm_Lower", "_Palm", "_Leg0_Upper", "_Leg0_Lower",
        "_Foot0", "_toe")}


def build(rig, armature):
    at = lambda name: Vector(rig[name]["head"])
    head = at("Head") + Vector((0, .07, .02))
    volume = cb.Volume(.018)
    # Functional upright humanoid frame on the measured native joints.
    volume.ellipsoid(at("Torso_Lower"), AXES, (.25, .20, .27))
    volume.sweep([at("Torso_Lower"), at("Torso_Upper"), at("neck") - UP * .08],
                 [.23, .29, .22])
    volume.ellipsoid(at("Torso_Upper") + UP * .12, AXES, (.31, .23, .23))
    volume.sweep([at("neck") - UP * .08, at("neck1"), head], [.19, .18, .19])
    # Broad short canine muzzle/jowls and a separate native-controlled jaw.
    volume.ellipsoid(head, AXES, (.23, .23, .25))
    volume.ellipsoid(head + FRONT * .20 - UP * .055, AXES, (.19, .20, .115))
    for sign in (-1, 1):
        volume.ellipsoid(head + Vector((sign * .11, .16, .075)), AXES, (.13, .11, .07))
    for side in ("L", "R"):
        shoulder, elbow, palm = [at(side + n) for n in ("_Arm_Upper", "_Arm_Lower", "_Palm")]
        volume.sweep([shoulder, elbow, palm], [.13, .10, .075])
        volume.ellipsoid(palm - UP * .04, AXES, (.085, .070, .105))
        hip, knee, ankle, toe = [at(side + n) for n in ("_Leg0_Upper", "_Leg0_Lower", "_Foot0", "_toe")]
        volume.sweep([hip, knee, ankle, toe + UP * .065], [.165, .12, .085, .085])
        volume.ellipsoid(toe + UP * .07, AXES, (.105, .15, .09))
    body = volume.polygonise("HoundArchonBody")
    cb.finish_surface(body, 4800, smooth_iterations=8)
    points = [v.co.copy() for v in body.data.vertices]
    rows = cb.heat_weights(body, armature, HEAT)
    rows = cb.clean_weights(cb.smooth_weights(body, rows), points, armature, HEAT)
    parts = [(body, rows, 0)]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), region))
    for sign in (-1, 1):
        # Plain floppy ears are rigid with the skull, without a new ear rig.
        rigid(cb.blob("HoundEar", head + Vector((sign * .22, -.025, -.025)),
                      AXES, (.105, .095, .19), 16, 10), [("Head", 1.0)], 1)
        eye = head + Vector((sign * .13, .225, .065))
        rigid(cb.blob("HoundEyeRim", eye, AXES, (.041, .027, .035), 12, 8), [("Head", 1.0)], 2)
        rigid(cb.blob("HoundEye", eye + FRONT * .024, AXES, (.025, .012, .024), 12, 8), [("Head", 1.0)], 3)
        rigid(cb.blob("HoundPupil", eye + FRONT * .034, AXES, (.011, .007, .015), 10, 8), [("Head", 1.0)], 2)
        rigid(cb.blob("HoundJowl", head + Vector((sign * .095, .26, -.10)),
                      AXES, (.105, .145, .095), 16, 10), [("Head", 1.0)], 1)
    rigid(cb.blob("HoundNose", head + Vector((0, .405, -.025)), AXES,
                  (.12, .055, .07), 18, 10), [("Head", 1.0)], 2)
    rigid(cb.blob("HoundMouth", head + Vector((0, .315, -.145)), AXES,
                  (.16, .09, .035), 18, 10), [("Head", 1.0)], 2)
    rigid(cb.blob("HoundJaw", head + Vector((0, .25, -.175)), AXES,
                  (.16, .16, .065), 18, 10), [("jaw", .4), ("jaw1", .6)], 1)
    for sign in (-1, 1):
        root = head + Vector((sign * .105, .34, -.11))
        rigid(cb.horn("HoundCanine", root, root - UP * .065,
                      .019, 10, 4, hint=FRONT), [("Head", 1.0)], 4)
    # Native palms/fingers retain their normal weapon-grip motion.
    for side in ("L", "R"):
        palm = at(side + "_Palm")
        for suffix in ("_finger1", "_finger2", "_Bfinger1", "_Bfinger2"):
            bone = side + suffix
            tip = at(bone)
            root = palm.lerp(tip, .18)
            rigid(cb.strand("HoundFinger", [root, root.lerp(tip, .6), tip],
                            [.033, .027, .020], 8), [(bone, 1.0)], 0)
    mesh, weights = cb.assemble("HoundArchonOriginal", parts)
    cb.require_single_body(mesh)
    return mesh, weights


def colours(p, n, region):
    broad = cb.fbm(p, 5.0, 321)
    fine = cb.fbm(p, 45.0, 322)
    base = np.array([.43, .25, .12]) * (.78 + .30 * broad[:, None])
    base *= .93 + .10 * fine[:, None]
    # Original pale cloth/tabard and plain boots/bracers are cosmetic only.
    cloth = (abs(p[:, 0]) < .28) & (p[:, 2] > .69) & (p[:, 2] < 1.48)
    base = np.where(cloth[:, None], np.array([.73, .69, .56]) * (.90 + .10 * broad[:, None]), base)
    trim = cloth & (abs(p[:, 0]) > .23)
    base = np.where(trim[:, None], np.array([.45, .33, .12]), base)
    leather = (p[:, 2] < .43) | ((abs(p[:, 0]) > .39) & (p[:, 2] > .97) & (p[:, 2] < 1.15))
    base = np.where(leather[:, None], np.array([.17, .12, .075]), base)
    base = np.where((region == 1)[:, None], np.array([.24, .14, .08]) * (.85 + .15 * broad[:, None]), base)
    base = np.where((region == 2)[:, None], np.array([.055, .045, .035]), base)
    base = np.where((region == 3)[:, None], np.array([.48, .27, .075]), base)
    return np.where((region == 4)[:, None], np.array([.76, .70, .55]), base)


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out", "preview"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = donor.measured_rig(args.capture)
    armature = cb.review_armature(rig_data, "HoundArchonReviewRig")
    body, weights = build(rig, armature)
    cb.unwrap(body)
    cb.bake_albedo(body, 1024, colours, args.albedo)
    cb.attach_albedo_preview(body, args.albedo)
    extra = {"creature": "hound-archon", "donorFamily": "lizardfolk-free-hands",
             "nativeDonor": donor.DONOR_GUID, "nativePrefab": donor.PREFAB_GUID,
             "bodyRenderer": donor.BODY_RENDERER, "accessoryRenderer": donor.ACCESSORY_RENDERER,
             "nativeRendererCount": 2, "visibleArms": 2, "visibleLegs": 2,
             "visibleTail": False, "canineHead": True, "humanoidBody": True, "jawJoints": 2}
    summary = cb.write_mesh_data(args.mesh_data, body, weights, rig_hash, args.albedo,
                                 "donor renderer local; +X lateral, +Y forward, +Z up", extra)
    report = {"schemaVersion": 1, "builder": Path(__file__).name, "rigSha256": rig_hash,
              **extra, **summary, "albedo": cb.albedo_manifest(args.albedo)}
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
    scene.world = scene.world or bpy.data.worlds.new("HoundArchonPreviewWorld")
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
    print('[hound-archon] ' + json.dumps(report, sort_keys=True))


if __name__ == '__main__':
    main()
