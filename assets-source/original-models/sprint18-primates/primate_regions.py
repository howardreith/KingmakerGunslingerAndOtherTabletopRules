#!/usr/bin/env python3
"""The Sprint 18 albedo layout, shared by the generator and the painter.

It lives on its own so the painter does not have to import Blender and the
generator does not have to import numpy. Both must agree on it exactly:
the generator writes texture coordinates inside these regions and the
painter fills the same boxes.
"""

KINDS = ("ape", "dire-ape")

# Eight regions of one sheet, as (u0, v0, u1, v1) with v upward as Unity
# samples it. The torso and limbs carry the coat and take half the sheet
# between them; the face carries skin, muzzle and brow; the mane carries
# the crest and collar; the mouth carries gum and canine; the hands carry
# palm and sole leather; and the eye and nail regions exist because an
# eyeball and a claw cannot share a skin texture and still read as an
# eyeball and a claw.
PRIMATE_ATLAS = {
    "torso": (0.0, 0.5, 0.5, 1.0),
    "limbs": (0.5, 0.5, 1.0, 1.0),
    "face": (0.0, 0.25, 0.375, 0.5),
    "mane": (0.375, 0.25, 0.75, 0.5),
    "eye": (0.75, 0.25, 1.0, 0.5),
    "mouth": (0.0, 0.0, 0.375, 0.25),
    "hands": (0.375, 0.0, 0.75, 0.25),
    "nail": (0.75, 0.0, 1.0, 0.25),
}
