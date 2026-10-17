"""Build original Triceratops and Stegosaurus skins from private rig frames."""
import argparse
import hashlib
import json
import math
from pathlib import Path
import sys

import bpy
import bmesh
from mathutils import Vector

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "pteranodon"))
sys.path.insert(0, str(ROOT / "sprint12-quadrupeds"))
sys.path.insert(0, str(ROOT / "ungulates"))
import generate_pteranodon as shared
from generate_sprint12_quadrupeds import ellipsoid, tube, tuft
from generate_ungulates import articulated_leg, hoof

KINDS = ("triceratops", "stegosaurus")
DONORS = {
    "triceratops": "028cc6f46e7998f46855a33ffde89567",
    "stegosaurus": "c3524f96954a1d94f8525b86e7626633",
}
TRICERATOPS_DRIVERS = (
    "Head", "L_Arm_Lower", "L_Arm_Upper", "L_Foot0", "L_Leg0_Lower",
    "L_Leg0_Upper", "L_Palm", "LowerTorso", "Neck", "R_Arm_Lower",
    "R_Arm_Upper", "R_Foot0", "R_Leg0_Lower", "R_Leg0_Upper", "R_Palm",
    "Spine", "Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M",
    "UpperTorso", "backKnee_L", "backKnee_R", "frontKnee_L", "frontKnee_R")
STEGOSAURUS_DRIVERS = (
    "LowerTorso", "Spine1_M", "UpperTorso", "Neck_M", "Head", "Jaw_M",
    "Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M", "Tail5_M") + tuple(
        side + part for side in ("Left", "Right")
        for part in ("UpperArm", "LowerArm", "Hand", "UpperLeg0",
                     "LowerLeg0", "Foot0"))


def _rig_from_rows(raw, rows, renderer, root, source, drivers):
    by_name = {row["name"]: row for row in rows}
    if len(by_name) != len(rows) or not set(drivers).issubset(by_name):
        raise ValueError("missing, duplicate or unreviewed donor driver")

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
    rig = {"source": source, "space": "renderer-local bind frame",
           "renderer": renderer, "rootBone": root,
           "bindPoseCount": len(rows), "bones": bones}
    return rig, {row["name"]: row for row in bones}, hashlib.sha256(raw).hexdigest()


def measured_rig(path, kind):
    raw = Path(path).read_bytes()
    capture = json.loads(raw)
    if kind == "triceratops":
        renderers = capture.get("renderers", [])
        if ("mastodon" not in capture.get("source", "").lower() or
                len(renderers) != 1):
            raise ValueError("exact geometry-free Mastodon capture required")
        skin = renderers[0]
        rows = skin.get("bones", [])
        if (skin.get("renderer") != "mastodont" or
                skin.get("rootBone") != "LowerTorso" or
                skin.get("boneCount") != skin.get("bindPoseCount") or
                len(rows) != skin.get("boneCount")):
            raise ValueError("Mastodon renderer/bind contract changed")
        return _rig_from_rows(raw, rows, "mastodont", "LowerTorso",
                              "Sprint 11 private geometry-free Mastodon capture",
                              TRICERATOPS_DRIVERS)
    donors = [row for row in capture.get("donors", [])
              if row.get("key") == "hodag" and
              row.get("nativeBlueprint") == DONORS["stegosaurus"]]
    if len(donors) != 1 or len(donors[0].get("skinnedRenderers", [])) != 1:
        raise ValueError("exact single-renderer Hodag capture required")
    skin = donors[0]["skinnedRenderers"][0]
    rows = skin.get("bones", [])
    if (skin.get("renderer") != "hodag" or skin.get("rootBone") != "LowerTorso" or
            skin.get("boneCount") != 36 or skin.get("bindPoseCount") != 36 or
            len(rows) != 36):
        raise ValueError("Hodag renderer/bind contract changed")
    actions = [clip.get("name") for manager in donors[0].get("animationManagers", [])
               for action in manager.get("actions", []) if action
               for clip in (action.get("clips") or []) if clip]
    if "Hodag_attack_tail" not in actions:
        raise ValueError("native tail action metadata missing")
    return _rig_from_rows(raw, rows, "hodag", "LowerTorso",
                          "Sprint 22 private geometry-free Hodag capture",
                          STEGOSAURUS_DRIVERS)


def frill(bm, weights, uvs, centre, bone):
    # Broad original oval shield behind the skull. It is intentionally much
    # wider than a rhinoceros head and carries a scalloped keratin rim.
    side, up, forward = Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, -1))
    ellipsoid(bm, weights, uvs, centre - forward * .25,
              side, up, forward, (1.06, .92, .16), bone, "membrane", 9, 20)
    for sign in (-1, 1):
        for index in range(3):
            base = centre + side * sign * (.42 + index * .24) + \
                up * (.52 - index * .18) - forward * .20
            tip = base + side * sign * (.12 + index * .04) + up * .28
            tuft(bm, weights, uvs, base, tip, .09, bone)


def triceratops_body(bm, weights, uvs, rig):
    at = lambda name: Vector(rig[name]["head"])
    side, up, forward = Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, -1))
    rear, spine, shoulder, neck, skull = (at(name) for name in
        ("LowerTorso", "Spine", "UpperTorso", "Neck", "Head"))
    tube(bm, weights, uvs,
         [rear - forward * .30, rear, spine, shoulder, neck],
         [.84, 1.12, 1.22, 1.15, .78],
         ["LowerTorso", "LowerTorso", "Spine", "UpperTorso", "Neck"],
         "body", 18)
    # Raised shoulder mass and broad pelvic tank distinguish this from the
    # narrow released rhinoceros silhouettes.
    ellipsoid(bm, weights, uvs, shoulder + up * .22, side, up, forward,
              (1.12, .82, .92), "UpperTorso", "body", 8, 18)
    ellipsoid(bm, weights, uvs, rear + up * .08, side, up, forward,
              (.98, .76, .93), "LowerTorso", "body", 8, 18)
    frill(bm, weights, uvs, skull + up * .17, "Head")
    tube(bm, weights, uvs,
         [neck, skull, skull + forward * .42 - up * .08,
          skull + forward * .78 - up * .17],
         [.62, .58, .43, .25], ["Neck", "Head", "Head", "Head"],
         "body", 16)
    tube(bm, weights, uvs,
         [skull + forward * .55 - up * .24,
          skull + forward * .88 - up * .30],
         [.31, .15], ["Head", "Head"], "beak", 12)
    # Two long brow horns and one shorter nasal horn: three, and only three.
    for sign in (-1, 1):
        base = skull + side * sign * .36 + up * .34 + forward * .12
        tube(bm, weights, uvs,
             [base, base + forward * .54 + up * .13,
              base + forward * 1.38 + up * .26],
             [.15, .095, .006], ["Head"] * 3, "crest", 11)
    nose = skull + forward * .57 + up * .05
    tube(bm, weights, uvs,
         [nose, nose + forward * .34 + up * .16,
          nose + forward * .84 + up * .42],
         [.12, .075, .005], ["Head"] * 3, "crest", 10)
    for prefix in ("L", "R"):
        front = [prefix + "_Arm_Upper", prefix + "_Arm_Lower",
                 "frontKnee_" + prefix, "frontAnkle_" + prefix,
                 prefix + "_Palm"]
        hind = [prefix + "_Leg0_Upper", prefix + "_Leg0_Lower",
                "backKnee_" + prefix, "backAnkle_" + prefix,
                prefix + "_Foot0"]
        for names in (front, hind):
            points = [at(name) for name in names]
            articulated_leg(bm, weights, uvs, points,
                            [.48, .44, .39, .34, .29], names, 12)
            hoof(bm, weights, uvs, points[-1], names[-1], forward,
                 .52, 3)
    tail_names = ["Tail0_M", "Tail1_M", "Tail2_M", "Tail3_M", "Tail4_M"]
    tube(bm, weights, uvs, [at(name) for name in tail_names],
         [.18, .14, .10, .065, .025], tail_names, "limbs", 10)


def plate(bm, weights, uvs, centre, half_width, height, depth, bone):
    # A thin, closed five-sided plate extruded across the sagittal plane.
    profile = [(-depth, 0.0), (-depth * .42, height * .76),
               (0.0, height), (depth * .48, height * .68), (depth, 0.0)]
    sides = []
    for x in (-half_width, half_width):
        row = []
        for z, y in profile:
            vertex = bm.verts.new(centre + Vector((x, y, z)))
            weights[vertex] = [(bone, 1.0)]
            uvs[vertex] = shared.region_uv("crest", (z / (2 * depth)) + .5,
                                           y / height)
            row.append(vertex)
        sides.append(row)
    bm.faces.new(tuple(sides[0]))
    bm.faces.new(tuple(reversed(sides[1])))
    for index in range(len(profile)):
        nxt = (index + 1) % len(profile)
        bm.faces.new((sides[0][index], sides[0][nxt],
                      sides[1][nxt], sides[1][index]))


def stegosaurus_body(bm, weights, uvs, rig):
    at = lambda name: Vector(rig[name]["head"])
    side, up, forward = Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1))
    lower, spine, upper, neck, skull = (at(name) for name in
        ("LowerTorso", "Spine1_M", "UpperTorso", "Neck_M", "Head"))
    tube(bm, weights, uvs,
         [at("Tail0_M"), lower, spine, upper, neck],
         [.35, .79, .86, .64, .30],
         ["Tail0_M", "LowerTorso", "Spine1_M", "UpperTorso", "Neck_M"],
         "body", 18)
    # High pelvis, descending shoulders and tiny head form the unmistakable
    # stegosaur silhouette rather than an Ankylosaurus barrel.
    ellipsoid(bm, weights, uvs, lower + up * .24, side, up, forward,
              (.82, .62, .78), "LowerTorso", "body", 8, 18)
    ellipsoid(bm, weights, uvs, upper + up * .09, side, up, forward,
              (.61, .46, .58), "UpperTorso", "body", 8, 18)
    tube(bm, weights, uvs, [neck, skull, skull + forward * .29 - up * .04],
         [.25, .19, .105], ["Neck_M", "Head", "Head"], "body", 12)
    tube(bm, weights, uvs,
         [at("Jaw_M") - up * .04, at("Jaw_M") + forward * .22 - up * .06],
         [.13, .07], ["Jaw_M", "Jaw_M"], "beak", 10)
    stations = [
        (at("Tail0_M") + up * .24, .045, .29, .18, "Tail0_M"),
        (lower - forward * .18 + up * .60, .055, .54, .21, "LowerTorso"),
        (lower + forward * .08 + up * .69, .065, .73, .24, "LowerTorso"),
        (spine - forward * .04 + up * .73, .07, .88, .26, "Spine1_M"),
        (spine + forward * .26 + up * .72, .07, .82, .25, "Spine1_M"),
        (upper - forward * .20 + up * .57, .06, .67, .23, "UpperTorso"),
        (upper + forward * .10 + up * .48, .055, .54, .20, "UpperTorso"),
        (neck - forward * .05 + up * .30, .045, .38, .17, "Neck_M"),
        (neck + forward * .22 + up * .22, .04, .26, .14, "Neck_M")]
    for centre, width, height, depth, bone in stations:
        plate(bm, weights, uvs, centre, width, height, depth, bone)
    for prefix in ("Left", "Right"):
        for names in ((prefix + "UpperArm", prefix + "LowerArm", prefix + "Hand"),
                      (prefix + "UpperLeg0", prefix + "LowerLeg0", prefix + "Foot0")):
            points = [at(name) for name in names]
            tube(bm, weights, uvs, points, [.18, .15, .11], list(names),
                 "limbs", 12)
            ellipsoid(bm, weights, uvs, points[-1] + forward * .06 - up * .04,
                      side, up, forward, (.17, .075, .22), names[-1],
                      "limbs", 5, 12)
    tail_names = ["Tail%d_M" % index for index in range(6)]
    tail_points = [at(name) for name in tail_names]
    tube(bm, weights, uvs, tail_points,
         [.22, .19, .15, .11, .075, .04], tail_names, "body", 14)
    # Four terminal thagomizer spikes; no terminal club geometry exists.
    for joint, spread, lift in (("Tail4_M", .42, .10), ("Tail5_M", .49, -.04)):
        base = at(joint)
        for sign in (-1, 1):
            tip = base + side * sign * spread + up * lift - forward * .10
            tuft(bm, weights, uvs, base + side * sign * .035, tip, .085, joint)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--kind", choices=KINDS, required=True)
    for name in ("capture", "albedo", "mesh-data", "report", "blend-out"):
        parser.add_argument("--" + name, required=True)
    args = parser.parse_args(sys.argv[sys.argv.index("--") + 1:])
    if args.kind == "triceratops":
        raise SystemExit("triceratops is built by build_triceratops.py since "
                         "the visual refinement (VIS-002); see SOURCE.md")
    bpy.ops.wm.read_factory_settings(use_empty=True)
    rig_data, rig, rig_hash = measured_rig(args.capture, args.kind)
    armature = shared.build_armature(rig_data)
    armature.name = args.kind.title() + "EditableRig"
    mesh = bpy.data.meshes.new(args.kind.title() + "OriginalMesh")
    obj = bpy.data.objects.new(args.kind.title(), mesh)
    bpy.context.collection.objects.link(obj)
    bm = bmesh.new()
    weights, uvs = {}, {}
    if args.kind == "triceratops":
        triceratops_body(bm, weights, uvs, rig)
        drivers = TRICERATOPS_DRIVERS
    else:
        stegosaurus_body(bm, weights, uvs, rig)
        drivers = STEGOSAURUS_DRIVERS
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
    if not set(groups).issubset(drivers):
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
    report = {
        "schemaVersion": 1, "creature": args.kind,
        "donorFamily": "mastodon" if args.kind == "triceratops" else "hodag",
        "nativeDonor": DONORS[args.kind], "rigSha256": rig_hash,
        "vertices": len(mesh.vertices), "polygons": len(mesh.polygons),
        "drivers": sorted(groups), "visibleLegs": 4,
        "horns": 3 if args.kind == "triceratops" else 0,
        "frill": args.kind == "triceratops",
        "dorsalPlates": 9 if args.kind == "stegosaurus" else 0,
        "tailJoints": 6 if args.kind == "stegosaurus" else 5,
        "tailSpikes": 4 if args.kind == "stegosaurus" else 0,
        "terminalClub": False,
        "extent": {"x": [min(xs), max(xs)], "y": [min(ys), max(ys)],
                   "z": [min(zs), max(zs)]},
        "albedo": shared.albedo_manifest(args.albedo)}
    shared.write_mesh_data(args.mesh_data, obj, mesh, indexed, uv_indexed,
                           report, args.albedo)
    payload = json.loads(Path(args.mesh_data).read_text(encoding="utf-8"))
    payload.update({
        "space": "donor renderer local; +X lateral, +Y up, " +
                 ("-Z forward" if args.kind == "triceratops" else "+Z forward"),
        "creature": args.kind, "nativeDonor": DONORS[args.kind],
        "visibleLegs": 4, "horns": report["horns"], "frill": report["frill"],
        "dorsalPlates": report["dorsalPlates"],
        "tailJoints": report["tailJoints"], "tailSpikes": report["tailSpikes"],
        "terminalClub": False})
    with Path(args.mesh_data).open("w", encoding="utf-8", newline="\n") as out:
        out.write(json.dumps(payload, indent=1) + "\n")
    Path(args.report).write_text(json.dumps(report, indent=2) + "\n",
                                 encoding="utf-8")
    bpy.ops.wm.save_as_mainfile(filepath=args.blend_out)
    print("[armored-dinosaur] " + json.dumps(report, sort_keys=True))


if __name__ == "__main__":
    main()
