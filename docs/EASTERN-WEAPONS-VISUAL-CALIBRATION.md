# Eastern Weapons visual calibration

## First-playtest repair candidate (2026-08-14)

The repair generator replaces the broad/symmetric blade surfaces with a narrow
asymmetric wedge: local `-X` is the distinct cutting edge and local `+X` the
blunt spine. Curvature is `0.055`, `0.085`, and `0.140` meters for Wakizashi,
Katana, and Nodachi respectively. Equipped roots remain identity transforms,
the primary grip remains the origin, and the tip remains along `+Z`.

All seven affected icons are measured at 42 degrees above horizontal with the
tip upper-right and butt lower-left. Camera and light objects are created only
after FBX export and therefore cannot alter equipped transforms.

The selected presentation donors are Scimitar for Wakizashi, Bastard Sword for
Katana, and Greatsword for Nodachi. These donors contribute only animation,
sockets, trails, and sound; the stable KMG categories and all rules fields are
unchanged. Human review of stance, clipping, and perceived attack-edge direction
remains pending.

## Structural calibration

| Family | Original length | Native presentation donor | Intended grip |
|---|---:|---|---|
| Wakizashi | 0.76 m | Scimitar forward one-handed contract | compact one hand; valid main or offhand |
| Katana | 1.05 m | Bastard Sword versatile contract | one hand or native two-hand support |
| Nodachi | 1.58 m | Greatsword two-handed sword contract | two-handed sword, never polearm animation |

Every prefab has an identity root, `Visual`, grip at zero, a finite positive
support target, tip, and negative butt. Opaque materials use Unity's Standard
shader. No prefab contains a light or camera. Runtime rejects an incomplete,
nonrenderable, nonfinite, implausible, or wrong-cardinality bundle and retains
the exact native family fallback.

The category icons share each family's equipped silhouette. Night Without
Moon, Heaven's Measure, and World-Tree Severer have distinct capstone palettes
and distinct file hashes. All six production icons are exact 128x128
transparent RGBA PNGs.

## Automated versus human evidence

Automated validation proves bundle identity, three-prefab cardinality,
family mapping, root/anchor relationships, renderer and material completeness,
finite plausible lengths, absence of cameras/lights, module-disabled fallback,
and instantiated-object cleanup. It cannot establish taste, clipping quality,
or animation appearance. Those claims remain open until the human checklist is
performed in game.
