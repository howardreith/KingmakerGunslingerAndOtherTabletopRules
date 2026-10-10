#!/usr/bin/env python3
"""Offline review renders of the Girallon and Xill prototypes.

The camera rig, the shading modes, the synthetic poses and the self-checking
swing are the Sprint 18 reviewer's and are reused rather than copied: the two
sprints share a donor rig, a bind frame and an exporter, so a second reviewer
would be a second thing to keep right.

What this one adds is the thing Sprint 19 has to be able to see. Both arms on
a side ride one driver chain, so a pose that swings the donor's right arm has
to swing BOTH right arms together. The `four-arm-spread` pose exists to make
that visible rather than leaving it as a claim in a design note, and the
`party-camera` view decides whether four arms read at all in play.

These are synthetic offline poses. They prove geometry and skinning; they
prove nothing about runtime timing, contact or animation.
"""
import argparse
import json
from pathlib import Path
import sys

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "sprint14-insects"))
sys.path.insert(0, str(HERE.parents[1] / "sprint18-primates"))
sys.path.insert(0, str(HERE.parent))
import render_primate_review as shared
from render_sprint14_review import rotate_bone

VIEWS = shared.VIEWS
POSES = shared.POSES + ("four-arm-spread",)

# Captured before anything rebinds the module attribute, so delegating to the
# Sprint 18 poses reaches the Sprint 18 function rather than this one.
SHARED_APPLY_POSE = shared.apply_pose


def apply_pose(armature, creature, pose):
    """One synthetic pose.

    Everything but the Sprint 19 pose is the Sprint 18 reviewer's. The new
    one swings each shoulder out and each elbow in, which under
    FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS must carry four arms and not two: if a
    render of this pose shows two arms moving and two standing still, the
    lower pair has been weighted to something it should not have been.
    """
    if pose != "four-arm-spread":
        return SHARED_APPLY_POSE(armature, creature, pose)
    shared.swing(armature, creature,
                 [(side + "_Up_Arm_01", 54) for side in ("L", "R")] +
                 [(side + "_Forearm_01", 30) for side in ("L", "R")],
                 shared.HAND_GROUPS)
    rotate_bone(armature, "Jaw_01", (1, 0, 0), 20)
    return ("both driver chains swung; all four arms must move together "
            "under FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--blend", required=True)
    parser.add_argument("--out-dir", required=True)
    parser.add_argument("--shading", default="textured",
                        choices=("clay", "silhouette", "textured", "unlit"))
    parser.add_argument("--pose", default="rest", choices=POSES)
    parser.add_argument("--views",
                        default="three-quarter,side,front,party-camera")
    parser.add_argument("--resolution", type=int, default=640)
    parser.add_argument("--suite", action="store_true")
    args = parser.parse_args(
        sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else [])
    # The shared renderer dispatches poses through the module-level name, so
    # pointing that at this module's version is what adds the Sprint 19 pose
    # without copying the renderer.
    shared.apply_pose = apply_pose
    try:
        if not args.suite:
            shared.render(args, args.shading, args.pose)
            return
        for shading in ("clay", "silhouette", "textured", "unlit"):
            shared.render(args, shading, "rest")
        args.views = "three-quarter,side,front,top"
        # The four-arm pose is reviewed from four sides, because the one
        # failure it exists to catch - a lower arm that does not follow its
        # chain - is invisible from some of them.
        for pose in ("four-arm-spread", "slam-contact", "claw-rake", "crouch"):
            shared.render(args, "textured", pose)
        args.views = "head-closeup"
        shared.render(args, "textured", "jaw-open")
        shared.render(args, "unlit", "rest")
        args.views = "hand-closeup"
        shared.render(args, "textured", "fists-closed")
        shared.render(args, "unlit", "rest")
    finally:
        shared.apply_pose = SHARED_APPLY_POSE
    print("[fourarmed-review] " + json.dumps(dict(
        poses=list(POSES),
        warning="synthetic offline poses; no runtime timing or contact proof")))


if __name__ == "__main__":
    main()
