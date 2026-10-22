# Outsider/Celestial original bodies

Lemure is project-authored geometry and a procedurally baked original 1024px
albedo. `build_lemure.py` uses the established continuous-body exporter and the
strict Lizardfolk Free Hands census reader. It reads only geometry-free native
bone interfaces; no game mesh, texture, material, animation or raw private rig
capture is included. The source record pins the actual available October 14
capture, not the later October 17 capture used by the refined theropods.

The continuous legless lower body follows the pelvis. Two arms use native
shoulder/elbow/palm controls, with original fingers/claws. Seventeen measured
drivers are used; no donor tail, leg surface or accessory remains visible.
Closed outward component checks and one continuous main body are required by
the exporter. Native attachment requires exact released unit GUID/name/prefab,
two measured renderers and original albedo identity, and uses existing per-view
material-controller adoption, accessory suppression, rollback and teardown.
Only the owned nested visual record receives the previously qualified expiry
effect link; the campaign donor is unchanged.

Regenerate with installed Blender (private outputs stay under ignored artifacts):

```
blender -b --factory-startup --python-exit-code 1 --python build_lemure.py -- --capture <private census.json> --albedo <lemure-albedo.png> --mesh-data <lemure-mesh.json> --report <private generation.json> --blend-out <private source.blend> --preview <private preview.png>
```

The offline preview shows the intended amorphous lower mass and two clawed arms.
It is not native camera-scale or owner aesthetic acceptance. Native lifecycle,
body presentation and final-package checks remain separately recorded in the
batch map. Seams, coarse anatomy and texture refinement may remain visual backlog.

Dretch uses the same measured donor interfaces and exporter, with an original
stocky/potbellied body, two legs/arms, large ears, fangs, claws and a two-control
jaw. The existing project-owned original Dretch painting is its visual reference;
its pixels are not copied into the procedural albedo. The final export uses29
drivers,7497 vertices/9636 triangles and a1024px original albedo. The initial
private preview showed a foot-claw connection gap; one toe-pad correction closed
it. Both private previews/generation records remain. Native presentation and
attack/lifecycle checks are separate requirements, not granted by this preview.

`build_dretch.py` uses the same six CLI parameters as the Lemure builder. Each
body has its own exact unit/bone/anatomy loader contract. The existing common
attachment/material controller, accessory suppression and teardown remain the
production path; only the repeated nested expiry-visual copy is shared. Source
records pin actual generated hashes and the geometry-free private capture.

Hound Archon uses the same established original-asset pipeline and measured
Free Hands rig: an original canine head with floppy ears, humanoid torso/limbs,
cloth, and native palm/finger controls for its owned sword. Its mesh has6401
vertices/8420 triangles,24 closed components and29 measured drivers. The source
record pins its original painting reference, exporter and actual asset hashes.
Simple anatomy, seams and cloth detail remain backlog. Native body/held-weapon
presentation and resolved combat remain distinct observations.
