#!/usr/bin/env python3
"""Offline review renders of the Sprint 21 prototypes.

The camera rig and the shading modes are the Sprint 14 reviewer's, reused
rather than copied: both creatures share that sprint's donor rig, bind frame
and exporter, so a second reviewer would be a second thing to keep right.

What this one adds is a pose for every printed thing the two bodies have to
be able to show, because the sprint's art packet has to answer "does the
creature do what its stat block says" and a rest render cannot.

For the Bebelith: its six-leg stride, which is the pose that proves the
fourth chain is empty and the silhouette is not the Doomspider's; its
foreclaws raised, which is the posture the printed creature holds; the
armour-dismantle sequence, both claws converging on one point, which is the
printed trigger; the bite, which is the attack that carries rot; and an
abdomen sway, which is the pose the recorded limitation is named for - the
spinnerets ride a two-bone chain and a render where the body turns and the
abdomen stands still means the weighting is worse than recorded.

For the Giant Crab: its eight-leg stride; both pincers forward, which is
grab; and a pincer drawn back in, which is constrict - the printed effect
that only happens while a claw is holding something.

These are synthetic offline poses. They prove geometry and skinning; they
prove nothing about runtime timing, contact or animation.
"""
import argparse
import math
from pathlib import Path
import sys

sys.dont_write_bytecode = True
HERE = Path(__file__).resolve()
sys.path.insert(0, str(HERE.parents[1] / "sprint14-insects"))
sys.path.insert(0, str(HERE.parent))
import render_sprint14_review as shared  # noqa: E402

from mathutils import Vector  # noqa: E402

VIEWS = shared.VIEWS
SHADINGS = shared.SHADINGS
UP = Vector((0.0, 0.0, 1.0))
SIDE = Vector((1.0, 0.0, 0.0))
FORWARD = Vector((0.0, -1.0, 0.0))

# Every printed thing the two bodies have to be able to show.
BEBELITH_POSES = ("rest", "six-leg-stride", "foreclaws-raised",
                  "armour-dismantle", "bite", "web-cast", "abdomen-sway",
                  "turn")
CRAB_POSES = ("rest", "eight-leg-stride", "pincers-forward", "constrict",
              "turn")
POSES = tuple(sorted(set(BEBELITH_POSES) | set(CRAB_POSES)))

# Captured before anything rebinds the module attribute, so delegating to
# the Sprint 14 poses reaches the Sprint 14 function rather than this one.
# The same guard Sprints 19 and 20 needed when they extended a reviewer.
SHARED_APPLY_POSE = shared.apply_pose
rotate_bone = shared.rotate_bone

# The Bebelith walks on three pairs; the alternating tripod is therefore
# left-front, right-middle, left-rear forward and the rest back - the same
# gait the Sprint 14 six-legged insects use, on the same three chains.
TRIPOD_FORWARD = (("L", 0), ("R", 1), ("L", 2))


def _swing_legs(armature, chains, forward):
    """Swing a set of leg chains fore and aft about the up axis."""
    moved = 0
    for side in ("L", "R"):
        for index in chains:
            degrees = 15.0 if (side, index) in forward else -15.0
            if rotate_bone(armature, "%s_Leg%d_Upper" % (side, index),
                           UP, degrees):
                moved += 1
    return moved


def _foreclaws(armature, lift, converge, reach):
    """Pose both pedipalp chains: how high, how far in, how far out."""
    moved = 0
    for side, sign in (("L", 1.0), ("R", -1.0)):
        for name, axis, degrees in (
                ("pedipalp1_%s" % side, SIDE, lift),
                ("pedipalp3_%s" % side, UP, converge * sign),
                ("pedipalp5_%s" % side, SIDE, reach)):
            if rotate_bone(armature, name, axis, degrees):
                moved += 1
    return moved


def apply_pose(armature, pose):
    """One synthetic pose, for geometry review only."""
    if pose == "six-leg-stride":
        # Proves the fourth chain is empty: three pairs swing and nothing
        # moves behind them. A render where a fourth pair of legs strides
        # means this body is still the Doomspider's.
        return _swing_legs(armature, (0, 1, 2), TRIPOD_FORWARD) > 0

    if pose == "eight-leg-stride":
        # All four pairs, which is the crab's whole printed anatomy and the
        # reason it carries a +12 against trip.
        forward = (("L", 0), ("R", 1), ("L", 2), ("R", 3))
        return _swing_legs(armature, (0, 1, 2, 3), forward) > 0

    if pose == "foreclaws-raised":
        # The posture the printed creature holds: both barbed foreclaws
        # well above the walking line and angled in.
        return _foreclaws(armature, lift=-34.0, converge=16.0,
                          reach=-14.0) > 0

    if pose == "armour-dismantle":
        # The printed trigger, as a pose: both claws converging on one
        # point in front of the creature. If the two blades cannot be made
        # to meet, the mechanic has no gesture a player can read.
        return _foreclaws(armature, lift=-20.0, converge=34.0,
                          reach=24.0) > 0

    if pose == "bite":
        # The attack that carries rot. Both mandibles open and the forebody
        # pitches down, which is the only way a jaw on this rig can show.
        moved = 0
        for side, sign in (("L", 1.0), ("R", -1.0)):
            if rotate_bone(armature, "chelicera_%s" % side, UP, 26.0 * sign):
                moved += 1
        if rotate_bone(armature, "LowerTorso", SIDE, 11.0):
            moved += 1
        return moved > 0

    if pose == "web-cast":
        # The printed web comes out of the abdomen, so casting it is the
        # abdomen swinging down and forward under the body while the
        # forebody lifts. There is no turret here and the recorded
        # limitation says so; what this pose has to show is that the
        # spinnerets travel with the mass and end up pointing at something.
        moved = 0
        for name, degrees in (("Tail1_M", -14.0), ("UpperTorso", -19.0),
                              ("Tail3_M", -24.0)):
            if rotate_bone(armature, name, SIDE, degrees):
                moved += 1
        if rotate_bone(armature, "LowerTorso", SIDE, -9.0):
            moved += 1
        return moved > 0

    if pose == "abdomen-sway":
        # The pose the recorded limitation is named for. Swinging the
        # abdomen chain must carry the whole abdomen and both spinneret
        # pairs with it; a render where the body turns and the abdomen
        # stands still means the weighting is worse than recorded.
        moved = 0
        for name, degrees in (("Tail1_M", 10.0), ("UpperTorso", 14.0),
                              ("Tail3_M", 17.0)):
            if rotate_bone(armature, name, UP, degrees):
                moved += 1
        return moved > 0

    if pose == "pincers-forward":
        # Grab: both pincers out and open, which is the crab's only attack
        # and the gesture its printed hold starts from.
        return _foreclaws(armature, lift=-8.0, converge=22.0,
                          reach=16.0) > 0

    if pose == "constrict":
        # Constrict happens only while a claw is already holding, so the
        # pose is a pincer drawn back toward the body rather than reaching:
        # the printed effect is the claw crushing what it has.
        return _foreclaws(armature, lift=4.0, converge=-26.0,
                          reach=-20.0) > 0

    if pose == "turn":
        # The extreme a walk cycle reaches. Both bodies are wide, so this
        # is where a silhouette either stays readable or folds into itself.
        moved = 0
        if rotate_bone(armature, "LowerTorso", UP, 26.0):
            moved += 1
        if rotate_bone(armature, "Tail1_M", UP, -14.0):
            moved += 1
        return moved > 0

    return SHARED_APPLY_POSE(armature, pose)


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--blend", required=True)
    parser.add_argument("--out-dir", required=True)
    parser.add_argument("--kind", required=True,
                        choices=("bebelith", "giant-crab"))
    parser.add_argument("--views", default=",".join(sorted(VIEWS)))
    parser.add_argument("--shading", default="textured")
    parser.add_argument("--pose", default="rest")
    parser.add_argument("--resolution", type=int, default=768)
    args = parser.parse_args(argv)
    allowed = BEBELITH_POSES if args.kind == "bebelith" else CRAB_POSES
    if args.pose not in allowed:
        raise SystemExit("%s has no pose %s" % (args.kind, args.pose))
    # One reviewer for one rig family. Both creatures join the Sprint 14
    # insects on the Giant Spider rig rather than being given a second copy
    # of the reviewer to keep right.
    shared.KINDS = tuple(shared.KINDS) + ("bebelith", "giant-crab")
    shared.POSES = POSES
    shared.apply_pose = apply_pose
    sys.argv = [sys.argv[0], "--",
                "--blend", args.blend, "--out-dir", args.out_dir,
                "--views", args.views, "--shading", args.shading,
                "--pose", args.pose, "--resolution", str(args.resolution)]
    shared.main()
    print("rendered", args.kind, args.pose, args.shading)


if __name__ == "__main__":
    main()
