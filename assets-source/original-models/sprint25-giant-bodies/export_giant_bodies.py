"""Export the Sprint 25 bodies as shipped mesh data.

A Blender prototype is not an installed body. This writes the two creatures
through the released exporter the runtime already reads - the same
`write_mesh_data` schema, the same base64 geometry blob, the same albedo
manifest - so the loader path is the proven one rather than a new format.

Two things are specific to this sprint and are done here rather than assumed:

AUTHORING SPACE. The released schema ships vertices in the DONOR's renderer
local frame (`+X left, +Y up, -Z forward`). The graybox authors in Blender
Z-up, through `(x, y, z) -> (x, -z, y)`. Every vertex and normal is converted
back by the exact inverse, `(X, Y, Z) -> (X, Z, -Y)`, whose determinant is +1,
so handedness is unchanged and the exporter's own winding reversal is still
the only handedness correction in the pipeline.

WHICH BODY SHIPS. Brachiosaurus takes the authored upright neck with the
forward-chain influence moved onto `Head` - the owner-authorized interim
design, stiff by construction and recorded as such. Elephant takes the
measured native trunk chain. Both carry the repairs already made: graded limb
sockets with a pinned neck anchor, rings on every joint the donor bends, the
shoulder mass on the donor's own `Neck`, soles flattened onto the donor's sole
plane, and radii tapering by arc length.

Run:
  blender --background --python-exit-code 1 --python export_giant_bodies.py \
      -- --census <sprint25-donor-census.json> --albedo-dir <dir> \
         --out <assets/sprint25-giant-bodies> --report <dir>
"""
import argparse
import hashlib
import json
from pathlib import Path
import sys

import bpy
from mathutils import Matrix

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve().parent
ROOT = HERE.parent
for folder in (ROOT / "pteranodon", ROOT / "sprint12-quadrupeds", HERE):
    sys.path.insert(0, str(folder))
import generate_pteranodon as shared            # noqa: E402
import graybox_giant_bodies as graybox          # noqa: E402
import prototype_giant_bodies as proto          # noqa: E402

# Blender Z-up back to the donor's renderer-local frame. Determinant +1.
TO_RENDERER = Matrix(((1.0, 0.0, 0.0), (0.0, 0.0, 1.0), (0.0, -1.0, 0.0)))

NATIVE_DONOR = "028cc6f46e7998f46855a33ffde89567"
NATIVE_PREFAB = "fe9f25fe62e0f3b49a13910a996470b2"
BODY_RENDERER = "mastodont"

# name, authored neck state, weight filter, the anatomy this body claims
BODIES = (
    ("brachiosaurus", "upright", ("damp", 0.0, "Head"), {
        "donorFamily": "mastodon",
        "quadruped": True,
        "visibleLegs": 4,
        "forwardChainJoints": 7,
        "rearTailJoints": 5,
        "neckDriver": "Head",
        "neckIsChainDriven": False,
        "tusks": 0,
        "ears": 0,
        "interimStiffNeck": True,
    }),
    ("elephant", "rest", None, {
        "donorFamily": "mastodon",
        "quadruped": True,
        "visibleLegs": 4,
        "forwardChainJoints": 7,
        "rearTailJoints": 5,
        "neckDriver": "chain",
        "neckIsChainDriven": True,
        "tusks": 2,
        "ears": 2,
        "interimStiffNeck": False,
    }),
)


def export(kind, state, filter_spec, anatomy, census, rig, rig_names,
           rig_hash, albedo_dir, out, report_dir):
    proto.setup_scene()
    rest = graybox.load_rest(census)
    armature = shared.build_armature(rig)
    armature.name = "GiantBodyDonorRig"
    built = graybox.build(kind, rest, state, seam="mixed")
    obj = built.obj
    obj.name = "KMG_" + kind
    weight_filter = (proto.damp_forward_chain(filter_spec[1], filter_spec[2])
                     if filter_spec is not None else None)
    indexed, groups = proto.bind(built, armature, rig_names, weight_filter)

    mesh = obj.data
    # Into the frame the runtime binds in. Done on the mesh rather than the
    # object so the exported coordinates are the shipped ones.
    mesh.transform(TO_RENDERER.to_4x4())

    albedo = Path(albedo_dir) / (kind + "-albedo.png")
    if not albedo.is_file():
        raise SystemExit("missing albedo " + str(albedo))

    report = {"rigSha256": rig_hash}
    path = out / (kind + "-mesh.json")
    shared.write_mesh_data(str(path), obj, mesh, indexed, built.uvs, report,
                           str(albedo))

    # The contract the loader checks before it will use this mesh, added to
    # the released payload the same way Sprint 12 added its space note.
    payload = json.loads(path.read_text(encoding="utf-8"))
    payload["space"] = "donor renderer local; +X left, +Y up, -Z forward"
    payload["creature"] = kind
    payload["nativeDonor"] = NATIVE_DONOR
    payload["nativePrefab"] = NATIVE_PREFAB
    payload["bodyRenderer"] = BODY_RENDERER
    payload["nativeRendererCount"] = 1
    payload.update(anatomy)
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(payload, handle, indent=1)
        handle.write("\n")

    blob = path.read_bytes()
    record = {
        "creature": kind,
        "file": path.name,
        "sha256": hashlib.sha256(blob).hexdigest(),
        "bytes": len(blob),
        "vertices": payload["vertexCount"],
        "triangles": payload["triangleCount"],
        "bones": len(payload["bones"]),
        "boneNames": payload["bones"],
        "albedo": payload["albedo"],
        "authoredState": state,
        "weighting": ("forward chain moved onto Head - the interim stiff neck"
                      if filter_spec is not None else
                      "measured native trunk chain"),
        "anatomy": anatomy,
    }
    if report_dir is not None:
        blend = Path(report_dir) / (kind + "-source.blend")
        bpy.ops.wm.save_as_mainfile(filepath=str(blend))
        data = blend.read_bytes()
        record["blend"] = {"file": blend.name, "bytes": len(data),
                           "sha256": hashlib.sha256(data).hexdigest()}
    return record


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--census", required=True)
    parser.add_argument("--albedo-dir", required=True)
    parser.add_argument("--out", required=True)
    parser.add_argument("--report", default=None)
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    args = parser.parse_args(argv)

    out = Path(args.out)
    out.mkdir(parents=True, exist_ok=True)
    report_dir = Path(args.report) if args.report else None
    if report_dir is not None:
        report_dir.mkdir(parents=True, exist_ok=True)

    rig, rig_names, rig_hash = proto.donor_rig(args.census)
    records = []
    for kind, state, filter_spec, anatomy in BODIES:
        records.append(export(kind, state, filter_spec, anatomy, args.census,
                              rig, rig_names, rig_hash, args.albedo_dir, out,
                              report_dir))

    summary = {
        "schemaVersion": 1,
        "donor": {"blueprint": NATIVE_DONOR, "prefab": NATIVE_PREFAB,
                  "renderer": BODY_RENDERER, "joints": len(rig["bones"]),
                  "censusSha256": rig_hash},
        "exporter": Path(__file__).name,
        "bodies": records,
    }
    if report_dir is not None:
        (report_dir / "export-report.json").write_text(
            json.dumps(summary, indent=2) + "\n", encoding="utf-8")
    print("EXPORTED: " + json.dumps(
        [{"creature": r["creature"], "sha256": r["sha256"][:16],
          "vertices": r["vertices"], "triangles": r["triangles"],
          "bones": r["bones"]} for r in records]))


if __name__ == "__main__":
    main()
