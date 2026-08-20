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

## Human-review orientation correction

- Held position: `(0, 0, 0)`.
- Held rotation: `Quaternion.Euler(90, 0, 0)`, mapping source point `+Z` to the
  installed Longspear forward `-Y` direction.
- Back position: `(0, -0.18, 0.06)`.
- Back rotation: `AngleAxis(35, forward) * Euler(-90, 0, 0)`, producing a
  distinct upper-left diagonal along the back rather than reusing the held
  model across the shoulder.
- Mesh scale and source length remain unchanged at 2.28 m.

Guarded run `20260820T1542457366433Z-eb6ee44b6d434229bfc2b1f671afc544`
passed 25/25 assertions. It measured held tip `(0,-1.14,0)` and butt
`(0,1.14,0)`, and back tip `(-0.654,0.754,0.06)` and butt
`(0.654,-1.114,0.06)`. The remaining acceptance boundary is visual inspection
of actual materialized world/inventory dolls, attacks, movement, switching,
body sizes, and the carried silhouette.
A visual imperfection is not a mechanics failure: disable or remove only the
dedicated bundle and the native presentation resumes without changing any item
or weapon-category blueprint identity.
