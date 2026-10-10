#!/usr/bin/env python3
"""Offline review renders of the Giant Scorpion prototype.

The camera rig, the shading modes and the self-checking poses are the Sprint
14 reviewer's, reused rather than copied: this creature shares that sprint's
donor rig, bind frame and exporter, so a second reviewer would be a second
thing to keep right.

What this one adds is the thing Sprint 20 has to be able to see. The whole
metasoma rides the abdomen chain, so a pose that sways the abdomen has to
carry the entire arched tail and the sting with it. The `tail-sway` pose
exists to make that visible rather than leaving it as a claim in a design
note: if a render of that pose shows the body turning and the tail standing
still, the weighting is wrong and the limitation is worse than recorded.

These are synthetic offline poses. They prove geometry and skinning; they
prove nothing about runtime timing, contact or animation.
"""
import argparse
from pathlib import Path
import sys

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "sprint14-insects"))
sys.path.insert(0, str(HERE.parent))
import render_sprint14_review as shared  # noqa: E402

VIEWS = shared.VIEWS
SHADINGS = shared.SHADINGS
POSES = shared.POSES + ("tail-sway", "claws-forward")

# Captured before anything rebinds the module attribute, so delegating to the
# Sprint 14 poses reaches the Sprint 14 function rather than this one. The
# same guard Sprint 19 needed when it extended the Sprint 18 reviewer.
SHARED_APPLY_POSE = shared.apply_pose
rotate_bone = shared.rotate_bone


def apply_pose(armature, pose):
    """One synthetic pose, for geometry review only.

    Everything but the two Sprint 20 poses is the Sprint 14 reviewer's.
    """
    if pose == "tail-sway":
        # The pose the limitation is named for. Swinging the abdomen chain
        # must carry the whole arched metasoma, the telson and the sting. A
        # render where the body turns and the tail stands still means the
        # weighting is wrong and the limitation is worse than recorded.
        moved = 0
        for name, degrees in (("Tail1_M", 9.0), ("UpperTorso", 13.0),
                              ("Tail3_M", 15.0)):
            moved += 1 if rotate_bone(armature, name, "Z", degrees) else 0
        return moved > 0
    if pose == "claws-forward":
        # The printed routine's opening: two claws seizing one target.
        moved = 0
        for side, sign in (("L", 1.0), ("R", -1.0)):
            for name, axis, degrees in (
                    ("pedipalp1_%s" % side, "Z", -17.0 * sign),
                    ("pedipalp3_%s" % side, "Z", 13.0 * sign),
                    ("pedipalp5_%s" % side, "X", -10.0)):
                moved += 1 if rotate_bone(armature, name, axis, degrees) else 0
        return moved > 0
    return SHARED_APPLY_POSE(armature, pose)


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--blend", required=True)
    parser.add_argument("--out-dir", required=True)
    parser.add_argument("--views", default=",".join(sorted(VIEWS)))
    parser.add_argument("--shading", default="textured")
    parser.add_argument("--pose", default="rest")
    parser.add_argument("--resolution", type=int, default=768)
    args = parser.parse_args(argv)
    if args.pose not in POSES:
        raise SystemExit("unknown Sprint 20 pose " + args.pose)
    # One reviewer for one rig family. The scorpion joins the Sprint 14
    # insects on the Giant Spider rig, so it is admitted to that reviewer
    # rather than given a second copy of it to keep right.
    shared.KINDS = tuple(shared.KINDS) + ("giant-scorpion",)
    shared.POSES = POSES
    shared.apply_pose = apply_pose
    sys.argv = [sys.argv[0], "--",
                "--blend", args.blend, "--out-dir", args.out_dir,
                "--views", args.views, "--shading", args.shading,
                "--pose", args.pose, "--resolution", str(args.resolution)]
    shared.main()
    print("rendered giant-scorpion", args.pose, args.shading)


if __name__ == "__main__":
    main()
