# Elven Branched Spear visual calibration

Deterministic technical acceptance requires an identity root transform, a
`Visual` child rotated exactly -90 degrees around X, finite positive scales, a
+Y point beyond the support-hand anchor, a negative-Y butt, opaque Standard
materials, at least one enabled renderer, no camera or light, and overall
anchor length from 2.25 through 2.32 m. Runtime diagnostics report either
`custom:validated:<prefab>` or the exact native-fallback reason.

Human acceptance should use a disposable development character and check:

- inventory icon alpha, framing, and contrast;
- medium male and female models, then a small race;
- idle, walk, run, ordinary thrust, full attack, movement AoO, and critical;
- weapon switching and dropped-item rendering where the game exposes it;
- Enlarge Person and Reduce Person;
- light, medium, and bulky armor silhouettes;
- hand separation, backwards point, floor drag, and body penetration relative
  to the unmodified native Longspear donor;
- missing-purple material, broken trail origin, and severe branch collision.

The custom mesh does not change animation style, socket, two-hand semantics,
attack timing, trail, reach, or sound. Those remain native Longspear contracts.
A visual imperfection is not a mechanics failure: disable or remove only the
dedicated bundle and the native presentation resumes without changing any item
or weapon-category blueprint identity.
