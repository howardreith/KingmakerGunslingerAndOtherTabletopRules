"""Original Dretch body on the measured free-hands rig; no native art copied."""
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
    head = at("Head") + Vector((0, .10, -.09))
    volume = cb.Volume(.019)
    volume.ellipsoid(at("Torso_Lower") + Vector((0, .09, .01)), AXES, (.40, .38, .43))
    volume.ellipsoid(at("Torso_Lower") + Vector((0, .20, .06)), AXES, (.38, .40, .41))
    volume.ellipsoid(at("Torso_Upper"), AXES, (.36, .29, .35))
    volume.sweep([at("Torso_Upper") + UP * .16, at("neck1"), head], [.29, .24, .26])
    volume.ellipsoid(head, AXES, (.28, .26, .28))
    volume.ellipsoid(head + FRONT * .20 - UP * .04, AXES, (.22, .16, .16))
    volume.ellipsoid(head + FRONT * .29, AXES, (.085, .09, .055))
    for sign in (-1, 1):
        volume.ellipsoid(head + Vector((sign * .13, .21, .10)), AXES, (.15, .09, .065))
    for side in ("L", "R"):
        shoulder, elbow, palm = [at(side + n) for n in ("_Arm_Upper", "_Arm_Lower", "_Palm")]
        volume.sweep([shoulder, elbow, palm], [.19, .14, .105])
        volume.ellipsoid(palm - UP * .05, AXES, (.105, .09, .13))
        hip, knee, ankle, toe = [at(side + n) for n in ("_Leg0_Upper", "_Leg0_Lower", "_Foot0", "_toe")]
        volume.sweep([hip, knee, ankle], [.22, .15, .095])
        foot = ankle.lerp(toe, .70) + UP * .035
        volume.ellipsoid(foot, AXES, (.13, .21, .085))
        volume.ellipsoid(toe + UP * .07, AXES, (.135, .15, .09))
    body = volume.polygonise("DretchBody")
    cb.finish_surface(body, 4900, smooth_iterations=8)
    points = [v.co.copy() for v in body.data.vertices]
    rows = cb.heat_weights(body, armature, HEAT)
    rows = cb.clean_weights(cb.smooth_weights(body, rows), points, armature, HEAT)
    parts = [(body, rows, 0)]

    def rigid(obj, bones, region):
        parts.append((obj, cb.rigid_rows(obj, bones), region))
    for sign in (-1, 1):
        ear = cb.blob("DretchEar", head + Vector((sign * .30, .01, .13)),
                      AXES, (.19, .045, .14), 16, 10)
        rigid(ear, [("Head", 1.0)], 0)
        tip = cb.horn("DretchEarTip", head + Vector((sign * .30, .01, .16)),
                      head + Vector((sign * .52, .01, .32)), .10, 12, 5, hint=FRONT)
        for vertex in tip.data.vertices:
            vertex.co.y = head.y + .01 + (vertex.co.y - head.y - .01) * .35
        rigid(tip, [("Head", 1.0)], 0)
        rigid(cb.blob("DretchInnerEar", head + Vector((sign * .33, .055, .15)),
                      AXES, (.095, .018, .07), 12, 8), [("Head", 1.0)], 1)
        eye = head + Vector((sign * .14, .276, .045))
        rigid(cb.blob("DretchEye", eye, AXES, (.044, .026, .033), 12, 8), [("Head", 1.0)], 2)
        rigid(cb.blob("DretchPupil", eye + FRONT * .023, AXES,
                      (.013, .009, .024), 10, 8), [("Head", 1.0)], 3)
        rigid(cb.blob("DretchNostril", head + Vector((sign * .043, .377, -.005)),
                      AXES, (.022, .012, .015), 10, 8), [("Head", 1.0)], 3)
    rigid(cb.blob("DretchMouth", head + Vector((0, .316, -.115)), AXES,
                  (.18, .033, .075), 20, 12), [("Head", 1.0)], 3)
    rigid(cb.blob("DretchJaw", head + Vector((0, .245, -.19)), AXES,
                  (.21, .155, .075), 20, 12), [("jaw", .4), ("jaw1", .6)], 0)
    for x in (-.14, -.07, .07, .14):
        root = head + Vector((x, .344, -.053))
        rigid(cb.horn("DretchUpperTooth", root, root - UP * .085, .020, 10, 4,
                      hint=FRONT), [("Head", 1.0)], 4)
    for sign in (-1, 1):
        root = head + Vector((sign * .14, .35, -.20))
        rigid(cb.horn("DretchTusk", root, root + UP * .17 + FRONT * .018,
                      .032, 12, 5, hint=FRONT), [("jaw1", 1.0)], 4)
    for side in ("L", "R"):
        palm = at(side + "_Palm")
        for suffix in ("_finger1", "_finger2", "_Bfinger2"):
            bone = side + suffix
            tip = at(bone)
            root = palm.lerp(tip, .22)
            rigid(cb.strand("DretchFinger", [root, root.lerp(tip, .6), tip],
                            [.045, .036, .024], 8), [(bone, 1.0)], 0)
            direction = (tip - palm).normalized()
            rigid(cb.horn("DretchHandClaw", tip - direction * .015,
                          tip + direction * .11 + FRONT * .035, .028, 10, 4,
                          hint=FRONT), [(bone, 1.0)], 5)
        toe = at(side + "_toe")
        for x in (-.075, 0, .075):
            root = toe + Vector((x, .015, .065))
            rigid(cb.horn("DretchFootClaw", root, root + FRONT * .13 - UP * .025,
                          .030, 10, 4, hint=UP),
                  [(side + "_toe", .5), (side + "_claw", .5)], 5)
    mesh, weights = cb.assemble("DretchOriginal", parts)
    cb.require_single_body(mesh)
    return mesh, weights


def colours(p, n, region):
    broad = cb.fbm(p, 4.0, 311)
    fine = cb.fbm(p, 35.0, 312)
    front = np.clip(n[:, 1] * .8, 0, 1) * np.clip(1 - abs(p[:, 2] - 1.0) * 2, 0, 1)
    base = np.array([.40, .42, .40]) + front[:, None] * np.array([.12, .035, -.025])
    base *= .82 + .28 * broad[:, None]
    base += np.clip(fine - .5, 0, 1)[:, None] * np.array([.05, -.025, -.035])
    base = np.where((region == 1)[:, None], np.array([.37, .23, .19]), base)
    base = np.where((region == 2)[:, None], np.array([.65, .39, .10]), base)
    base = np.where((region == 3)[:, None], np.array([.07, .035, .025]), base)
    base = np.where((region == 4)[:, None], np.array([.78, .70, .51]), base)
    return np.where((region == 5)[:, None], np.array([.15, .13, .11]), base)


def main():
    parser = argparse.ArgumentParser()
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out", "preview"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = donor.measured_rig(args.capture)
    armature = cb.review_armature(rig_data, "DretchReviewRig")
    body, weights = build(rig, armature)
    cb.unwrap(body)
    cb.bake_albedo(body, 1024, colours, args.albedo)
    cb.attach_albedo_preview(body, args.albedo)
    extra = {"creature": "dretch", "donorFamily": "lizardfolk-free-hands",
             "nativeDonor": donor.DONOR_GUID, "nativePrefab": donor.PREFAB_GUID,
             "bodyRenderer": donor.BODY_RENDERER, "accessoryRenderer": donor.ACCESSORY_RENDERER,
             "nativeRendererCount": 2, "visibleArms": 2, "visibleLegs": 2,
             "visibleTail": False, "potbelly": True, "largeEars": True, "jawJoints": 2}
    summary = cb.write_mesh_data(args.mesh_data, body, weights, rig_hash, args.albedo,
                                 "donor renderer local; +X lateral, +Y forward, +Z up", extra)
    report = {"schemaVersion": 1, "builder": "build_dretch.py", "rigSha256": rig_hash,
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
    scene.world = scene.world or bpy.data.worlds.new("DretchPreviewWorld")
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
    print('[dretch] ' + json.dumps(report, sort_keys=True))


if __name__ == '__main__':
    main()
