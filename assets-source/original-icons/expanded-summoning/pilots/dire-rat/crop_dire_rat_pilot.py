"""Dire Rat icon pilot (VIS-004): the original painting cropped closer on the
rat, with the bronze medallion ring faded into a dark painted vignette.

blender -b --factory-startup --python crop_dire_rat_pilot.py -- \
    ../../sources/dire-rat.png dire-rat-pilot.png 170 170 920

Arguments: source, output, crop left, crop top (source pixels, origin top
left) and crop size. The original source is read only; nothing is painted
in: the ring and the black corners beyond it fade to a colour taken from the
painting's own shadows.
"""
import sys

import bpy
import numpy as np

src, out, x0, y0, size = sys.argv[sys.argv.index("--") + 1:][:5]
x0, y0, size = int(x0), int(y0), int(size)
img = bpy.data.images.load(src)
w, h = img.size
px = np.array(img.pixels[:], dtype=np.float32).reshape(h, w, 4)[::-1]
crop = px[y0:y0 + size, x0:x0 + size].copy()
# The painting is a disc centred in the source; the ring's inner edge sits
# at about 573 px. Outside 470 px the pixels fade to the dark backdrop, fully
# by 556 px, so no bronze band or black corner remains.
cy, cx = h / 2.0, w / 2.0
ys = (np.arange(size) + y0 + 0.5)[:, None]
xs = (np.arange(size) + x0 + 0.5)[None, :]
fade = np.clip((np.hypot(ys - cy, xs - cx) - 470.0) / (556.0 - 470.0), 0, 1)
fade = fade * fade * (3 - 2 * fade)
dark = np.array([0.035, 0.028, 0.022, 1.0], dtype=np.float32)
crop = crop * (1 - fade[..., None]) + dark * fade[..., None]
crop[..., 3] = 1.0
result = bpy.data.images.new("pilot", size, size, alpha=True)
result.pixels.foreach_set(np.ascontiguousarray(crop[::-1], dtype=np.float32).ravel())
result.filepath_raw = out
result.file_format = "PNG"
result.save()
