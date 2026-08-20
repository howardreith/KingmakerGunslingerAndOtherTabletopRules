# Approved model provenance manifest

Re-audited 2026-08-03. Originals below are byte-identical copies of the
incoming files. Adjacent `SOURCE.md`, `LICENSE.txt`, `ATTRIBUTION.txt`, and
`MODIFICATIONS.md` files control attribution and change notices.

| Model source | Original payload | SHA-256 | License |
|---|---|---|---|
| Cyril43, *Flintlock pistol* | `cyril43-flintlock-pistol/source/pistol.zip` | `31A4CA244EA1F2756D546DF87031CA810EE6314D2409FA4D8F47B5AC6BBA2C89` | CC-BY-4.0 |
| ccotwist, *Blunderbuss Low Poly* | `ccotwist-blunderbuss/source/Blunderbuss_Low_Poly.fbx` | `107783B89D7A72FBCC4D7E657E35D78ECEADE320E0AA9BBFE40AC0C6AA56D52C` | CC-BY-4.0 |
| Steven Jurriaans, *1851 Colt Navy Revolver* | `1851-navy-colt-revolver/source/Final2 Sketchfab.fbx` | `0B2D0549E37FE244FC64F9BC57B917D65E25F979AC01CA048929D130F55FF142` | CC-BY-4.0 |
| Mesh Masters, *Flintlock Rifle* | `mesh-masters-rifle-musket/source/Musket 01.fbx` | `BD3AFC3372453FAFF4742220B5E49FC7E021F10D9596E5C7000D2555FE486E18` | CC-BY-4.0 |

All accompanying texture hashes are recorded in
`C:\Dev\KingmakerGunslingerLab\ASSET-INTAKE-AUDIT.md`.

Issue 11 deterministic derivatives preserve those exact originals:

| Derivative | SHA-256 | Modifications |
|---|---|---|
| `firearm-long-gun-derivatives/musket-normalized.fbx` | `AF8B08BF8153E23A6B8329A79634A9016347E43DDDF90B53028285B1C25ED397` | meters, +Z barrel, grip/support/butt/muzzle/back markers, muted material |
| `firearm-long-gun-derivatives/blunderbuss-normalized.fbx` | `559F8D8B434729DC8D81881F69C0EB9D39FA33B49402A58A977D004E1DEBF6F3` | meters, +Z barrel, grip/support/butt/muzzle/back markers, muted material |

The generator fixes FBX creation metadata and UUID derivation so two clean
Blender processes produce byte-identical outputs. Its source-hash mapping is in
`firearm-long-gun-derivatives/generation-report.json`.

The quarantined advanced-rifle folder and loose audio WAVs are deliberately not
present here.
