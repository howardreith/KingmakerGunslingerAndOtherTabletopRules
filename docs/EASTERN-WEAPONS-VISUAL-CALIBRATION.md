# Eastern Weapons visual calibration

## Structural calibration

| Family | Original length | Native presentation donor | Intended grip |
|---|---:|---|---|
| Wakizashi | 0.76 m | Kukri/light-blade contract | compact one hand; valid main or offhand |
| Katana | 1.05 m | Bastard Sword versatile contract | one hand or native two-hand support |
| Nodachi | 1.58 m | Falchion two-handed sword contract | two-handed sword, never polearm animation |

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
