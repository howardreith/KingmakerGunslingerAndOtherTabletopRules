"""Build original Deinonychus and Tyrannosaurus bodies from a private rig census."""
import argparse
import hashlib
import json
from pathlib import Path
import sys

import bpy
import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "pteranodon"))
sys.path.insert(0, str(ROOT / "sprint12-quadrupeds"))
import generate_pteranodon as shared
from generate_sprint12_quadrupeds import ellipsoid, tube, tuft

KINDS = ("deinonychus", "tyrannosaurus")
DONOR_GUID = "a8cbd4cdb0feaf64eab1452257c3c971"
PREFAB_GUID = "d6db6ba3b2c459a48ba0730beef0bac8"
BODY_RENDERER = "_lizardman001"
ACCESSORY_RENDERER = "_ammunition04"
DRIVERS = tuple(sorted({
    "Head", "jaw", "jaw1", "neck", "neck1", "Torso_Lower", "Torso_Upper",
    "tail", "tail1", "tail2", "tail3",
    *[side + name for side in ("L", "R") for name in (
        "_Leg0_Upper", "_Leg0_Lower", "_Foot0", "_toe", "_claw",
        "_Arm_Upper", "_Arm_Lower", "_Palm", "_finger1", "_finger2",
        "_Bfinger1", "_Bfinger2")]
}))
REQUIRED_CLIPS = {
    "Slow_Moving", "Walking_01", "Running_cycle_01", "Idle_01",
    "Dying_e_(standing_up)", "Attack_01_3", "Attack_01_1", "Attack_01_2",
    "Gore_Attack", "Bite_Attack", "Attack_01_4"
}


def _rig_from_rows(raw, rows):
    by_name = {row["name"]: row for row in rows}
    if len(by_name) != len(rows) or not set(DRIVERS).issubset(by_name):
        raise ValueError("missing, duplicate or unreviewed theropod driver")

    def depth(name):
        parent = by_name[name].get("parent", "")
        return depth(parent) + 1 if parent in by_name else 0

    bones = []
    for row in rows:
        head = Vector(row["bindPosition"])
        children = [Vector(value["bindPosition"]) for value in rows
                    if value.get("parent") == row["name"]]
        tail = sum(children, Vector()) / len(children) if children else \
            head + Vector((0, .04, 0))
        if (tail - head).length < .001:
            tail = head + Vector((0, .04, 0))
        bones.append({"name": row["name"], "parent": row.get("parent", ""),
                      "index": row["index"], "depth": depth(row["name"]),
                      "head": list(head), "tail": list(tail)})
    return ({"source": "Sprint 24 private geometry-free theropod census",
             "space": "renderer-local bind frame", "renderer": BODY_RENDERER,
             "rootBone": "Torso_Lower", "bindPoseCount": len(rows),
             "bones": bones},
            {row["name"]: row for row in bones},
            hashlib.sha256(raw).hexdigest())


def measured_rig(path):
    raw = Path(path).read_bytes()
    capture = json.loads(raw)
    donors = [row for row in capture.get("donors", [])
              if row.get("key") == "lizardfolk-free-hands" and
              row.get("nativeBlueprint") == DONOR_GUID and
              row.get("prefab") == PREFAB_GUID]
    if len(donors) != 1:
        raise ValueError("exact Lizardfolk Free Hands donor census required")
    donor = donors[0]
    renderers = donor.get("skinnedRenderers", [])
    if sorted(row.get("renderer") for row in renderers) != \
            [ACCESSORY_RENDERER, BODY_RENDERER]:
        raise ValueError("reviewed two-renderer donor contract changed")
    body = next(row for row in renderers if row.get("renderer") == BODY_RENDERER)
    accessory = next(row for row in renderers
                     if row.get("renderer") == ACCESSORY_RENDERER)
    rows = body.get("bones", [])
    if (body.get("rootBone") != "Torso_Lower" or body.get("boneCount") != 39 or
            body.get("bindPoseCount") != 39 or len(rows) != 39 or
            accessory.get("rootBone") != "Torso_Lower" or
            accessory.get("boneCount") != 19 or
            accessory.get("bindPoseCount") != 19):
        raise ValueError("reviewed body/accessory bind contract changed")
    clips = {clip.get("name") for manager in donor.get("animationManagers", [])
             for action in manager.get("actions", []) if action and not action.get("null")
             for clip in (action.get("clips") or []) if clip}
    if not REQUIRED_CLIPS.issubset(clips):
        raise ValueError("reviewed locomotion/contact/death actions missing")
    return _rig_from_rows(raw, rows)


def tooth_row(bm, weights, uvs, start, forward, side, up, count, spread,
              jaw_bone, upper=True, scale=1.0):
    for index in range(count):
        along = (index + .5) / count
        centre = start + forward * (along * spread) + side * (
            (.16 + .06 * along) * (-1 if index % 2 else 1))
        direction = -up if upper else up
        tuft(bm, weights, uvs, centre, centre + direction * (.07 * scale),
             .024 * scale, jaw_bone)


def forelimbs(bm, weights, uvs, kind):
    side, up, forward = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
    tyr = kind == "tyrannosaurus"
    for prefix, sign in (("L", 1), ("R", -1)):
        if tyr:
            points = [Vector((.18 * sign, .34, 1.28)),
                      Vector((.22 * sign, .47, 1.16)),
                      Vector((.24 * sign, .58, 1.12))]
            radii = [.075, .052, .038]
        else:
            points = [Vector((.20 * sign, .30, 1.25)),
                      Vector((.28 * sign, .52, 1.04)),
                      Vector((.30 * sign, .72, .98))]
            radii = [.09, .065, .045]
        names = [prefix + "_Arm_Upper", prefix + "_Arm_Lower", prefix + "_Palm"]
        tube(bm, weights, uvs, points, radii, names, "limbs", 9)
        for offset, chain in ((-.035, ("_finger1", "_finger2")),
                              (.035, ("_Bfinger1", "_Bfinger2"))):
            base = points[-1] + side * sign * offset
            tip = base + forward * (.15 if tyr else .20) - up * .035
            tube(bm, weights, uvs, [base, base.lerp(tip, .62), tip],
                 [radii[-1] * .45, radii[-1] * .28, .006],
                 [prefix + chain[0], prefix + chain[1], prefix + chain[1]],
                 "crest", 6)


def hindlimbs(bm, weights, uvs, kind, rig):
    at = lambda name: Vector(rig[name]["head"])
    side, up, forward = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
    tyr = kind == "tyrannosaurus"
    radii = [.20, .16, .12, .075, .035] if tyr else [.145, .105, .075, .055, .028]
    for prefix, sign in (("L", 1), ("R", -1)):
        names = [prefix + value for value in
                 ("_Leg0_Upper", "_Leg0_Lower", "_Foot0", "_toe", "_claw")]
        points = [at(name) for name in names]
        tube(bm, weights, uvs, points, radii, names, "limbs", 11)
        foot = points[-2]
        ellipsoid(bm, weights, uvs, foot + forward * .07, side, up, forward,
                  ((.16 if tyr else .11), .065, (.25 if tyr else .19)),
                  names[-2], "limbs", 6, 12)
        # Three visible toes share the native foot/toe motion. The medial toe
        # becomes the distinctive raised sickle claw only on Deinonychus.
        for toe_index, lateral in enumerate((-.07, 0, .07)):
            base = foot + side * sign * lateral + forward * .13 - up * .035
            if not tyr and toe_index == 1:
                mid = base + forward * .08 + up * .08
                tip = base + forward * .16 + up * .18
                radius = .040
                bone = names[-1]
            else:
                mid = base + forward * (.12 if tyr else .09)
                tip = base + forward * (.23 if tyr else .17) - up * .015
                radius = .032 if tyr else .023
                bone = names[-2]
            tube(bm, weights, uvs, [base, mid, tip],
                 [radius, radius * .58, .004], [bone, bone, bone],
                 "crest", 7)


def theropod_body(bm, weights, uvs, kind, rig):
    at = lambda name: Vector(rig[name]["head"])
    side, up, forward = Vector((1, 0, 0)), Vector((0, 0, 1)), Vector((0, 1, 0))
    tyr = kind == "tyrannosaurus"
    # The body stays horizontal, but the head and lower jaw sit on the donor's
    # measured Head/jaw pivots. An early offline wide-bite review placed the
    # skull too low; the lower jaw then orbited visibly away from the head.
    # These coordinates make the jaw articulation, not a compensating visual
    # follow-through component, responsible for opening the mouth.
    pelvis = Vector((0, -.08, 1.08))
    chest = Vector((0, .10, 1.28))
    neck0 = Vector((0, .12, 1.46))
    neck1 = Vector((0, .15, 1.65))
    skull = Vector((0, .19, 1.77))
    snout = Vector((0, .88 if tyr else .72, 1.72))
    body_radii = ([.37, .43, .39, .28] if tyr else [.28, .30, .24, .17])
    tube(bm, weights, uvs, [pelvis, chest, neck0, neck1], body_radii,
         ["Torso_Lower", "Torso_Upper", "neck", "neck1"], "body", 16)
    ellipsoid(bm, weights, uvs, pelvis, side, up, forward,
              ((.39 if tyr else .29), (.34 if tyr else .26),
               (.43 if tyr else .32)), "Torso_Lower", "body", 8, 16)
    # The tail's first blended ring stays inside this project-owned rump mass,
    # so the transition cannot expose radial ring triangles at an oblique
    # camera angle.
    ellipsoid(bm, weights, uvs, pelvis - forward * .18 - up * .03,
              side, up, forward,
              ((.39 if tyr else .28), (.33 if tyr else .24),
               (.62 if tyr else .50)), "Torso_Lower", "body", 8, 16)
    ellipsoid(bm, weights, uvs, chest, side, up, forward,
              ((.35 if tyr else .25), (.32 if tyr else .23),
               (.40 if tyr else .30)), "Torso_Upper", "body", 8, 16)
    # An overlapping neck collar keeps the Head/neck1 interface closed during
    # the donor's turn and bite poses. It is original geometry on the measured
    # neck driver, not a runtime follow component.
    ellipsoid(bm, weights, uvs, Vector((0, .13, 1.64)), side, up, forward,
              ((.25 if tyr else .18), (.25 if tyr else .18),
               (.24 if tyr else .17)), "neck1", "body", 7, 14)
    # Upper skull and snout are Head-driven. The separate jaw/jaw1 chain
    # carries the lower jaw so the native Bite action visibly opens it.
    tube(bm, weights, uvs, [neck1, skull, snout],
         ([.27, .35, .22] if tyr else [.16, .22, .12]),
         ["neck1", "Head", "Head"], "body", 15)
    jaw_root = Vector((0, .20, 1.70))
    jaw_tip = snout + forward * .01 - up * (.15 if tyr else .11)
    tube(bm, weights, uvs, [jaw_root, jaw_root.lerp(jaw_tip, .58), jaw_tip],
         ([.16, .12, .045] if tyr else [.10, .07, .028]),
         ["jaw", "jaw1", "jaw1"], "membrane", 12)
    tooth_row(bm, weights, uvs, skull + forward * .08 - up * .13,
              forward, side, up, 6 if tyr else 5, .38 if tyr else .25,
              "Head", True, 1.15 if tyr else .8)
    tooth_row(bm, weights, uvs, jaw_root + forward * .03 + up * .01,
              forward, side, up, 5 if tyr else 4, .34 if tyr else .23,
              "jaw1", False, 1.05 if tyr else .75)
    for sign in (-1, 1):
        eye = skull + side * sign * (.20 if tyr else .13) + \
            forward * (.08 if tyr else .05) + up * (.10 if tyr else .07)
        ellipsoid(bm, weights, uvs, eye, side, up, forward,
                  ((.035 if tyr else .026),) * 3, "Head", "crest", 5, 9)

    tail_names = ["tail", "tail1", "tail2", "tail3"]
    # Original horizontal countertail geometry, weighted to the four captured
    # joints. Native animation still supplies every transform; no curve ships.
    tail_points = [pelvis, Vector((0, -.30, 1.04)),
                   Vector((0, -.66, .98)), Vector((0, -1.05, .87)),
                   Vector((0, -1.43, .72))]
    direction = (tail_points[-1] - tail_points[-2]).normalized()
    tail_points += [tail_points[-1] + direction * (.44 if tyr else .36),
                    tail_points[-1] + direction * (.88 if tyr else .72)]
    shared.add_tube(bm, weights, uvs, tail_points,
        ([.34, .30, .24, .17, .10, .05, .006] if tyr else
         [.25, .22, .17, .12, .07, .035, .004]),
        ["Torso_Lower"] + tail_names + ["tail3", "tail3"], "body",
        segments=14, cap_start=False)
    forelimbs(bm, weights, uvs, kind)
    hindlimbs(bm, weights, uvs, kind, rig)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=KINDS, required=True)
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    if args.kind == "tyrannosaurus":
        raise SystemExit("tyrannosaurus is built by build_tyrannosaurus.py "
                         "since the visual refinement (VIS-001); see SOURCE.md")
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = measured_rig(args.capture)
    armature = shared.build_armature(rig_data)
    title = "Deinonychus" if args.kind == "deinonychus" else "Tyrannosaurus"
    armature.name = title + "EditableRig"
    mesh = bpy.data.meshes.new(title + "OriginalMesh")
    obj = bpy.data.objects.new(title, mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    theropod_body(bm, weights, uvs, args.kind, rig)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(mesh)
    indexed = {vertex.index: values for vertex, values in weights.items()}
    uv_indexed = {vertex.index: uv for vertex, uv in uvs.items()}
    bm.free()
    if len(indexed) != len(mesh.vertices) or len(uv_indexed) != len(mesh.vertices):
        raise ValueError("unweighted or unpainted original vertex")
    layer = mesh.uv_layers.new(name="OriginalAlbedo")
    for loop in mesh.loops:
        layer.data[loop.index].uv = uv_indexed[loop.vertex_index]
    groups = {name: obj.vertex_groups.new(name=name)
              for name in sorted({name for row in indexed.values()
                                  for name, _ in row})}
    if not set(groups).issubset(DRIVERS):
        raise ValueError("unreviewed driver")
    for index, entries in indexed.items():
        total = sum(value for _, value in entries)
        for name, value in entries:
            groups[name].add([index], value / total, "REPLACE")
    modifier = obj.modifiers.new(name="NativeFramePreview", type="ARMATURE")
    modifier.object = armature
    obj.parent = armature
    shared.attach_preview_material(obj, mesh, args.albedo)
    xs, ys, zs = ([getattr(vertex.co, axis) for vertex in mesh.vertices]
                  for axis in ("x", "y", "z"))
    tyr = args.kind == "tyrannosaurus"
    report = {
        "schemaVersion": 1, "creature": args.kind,
        "donorFamily": "lizardfolk-free-hands", "nativeDonor": DONOR_GUID,
        "nativePrefab": PREFAB_GUID, "bodyRenderer": BODY_RENDERER,
        "accessoryRenderer": ACCESSORY_RENDERER, "nativeRendererCount": 2,
        "rigSha256": rig_hash, "vertices": len(mesh.vertices),
        "polygons": len(mesh.polygons), "drivers": sorted(groups),
        "visibleLegs": 2, "visibleForelimbs": 2, "biped": True,
        "jawJoints": 2, "tailJoints": 4,
        "sickleFootClaws": 0 if tyr else 2,
        "massiveSkull": tyr, "tinyForearms": tyr,
        "balancedCountertail": True,
        "extent": {"x": [min(xs), max(xs)], "y": [min(ys), max(ys)],
                   "z": [min(zs), max(zs)]},
        "albedo": shared.albedo_manifest(args.albedo)}
    shared.write_mesh_data(args.mesh_data, obj, mesh, indexed, uv_indexed,
                           report, args.albedo)
    payload = json.loads(Path(args.mesh_data).read_text(encoding="utf-8"))
    payload.update({
        "space": "donor renderer local; +X lateral, +Y forward, +Z up",
        "creature": args.kind, "donorFamily": report["donorFamily"],
        "nativeDonor": DONOR_GUID, "nativePrefab": PREFAB_GUID,
        "bodyRenderer": BODY_RENDERER, "accessoryRenderer": ACCESSORY_RENDERER,
        "nativeRendererCount": 2, "visibleLegs": 2, "visibleForelimbs": 2,
        "biped": True, "jawJoints": 2, "tailJoints": 4,
        "sickleFootClaws": report["sickleFootClaws"],
        "massiveSkull": tyr, "tinyForearms": tyr,
        "balancedCountertail": True})
    with Path(args.mesh_data).open("w", encoding="utf-8", newline="\n") as out:
        out.write(json.dumps(payload, indent=1) + "\n")
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[theropod] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
