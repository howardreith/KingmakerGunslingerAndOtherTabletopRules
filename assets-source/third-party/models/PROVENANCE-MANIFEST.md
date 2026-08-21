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
| Killian Delias, *Winchester lever action rifle* | `killian-delias-winchester-lever-action-rifle/source/fusilALevier.fbx` | `74D60FCC6D9A6E89C20EBB8C2D35471417E9F6C23A840FA65967C20251540A1D` | CC-BY-4.0; cleared for Advanced Rifle only |

All accompanying texture hashes are recorded in
`C:\Dev\KingmakerGunslingerLab\ASSET-INTAKE-AUDIT.md`.

Issue 11 deterministic derivatives preserve those exact originals:

| Derivative | SHA-256 | Modifications |
|---|---|---|
| `firearm-long-gun-derivatives/musket-normalized.fbx` | `C5E2EA93E903782BF3110E50C1D6677C4E7C109248651495192D8B6063F73A0A` | metres, canonical +Z/+Y frame, trigger-wrist grip, renderer-bound ends, seven semantic markers, muted material |
| `firearm-long-gun-derivatives/blunderbuss-normalized.fbx` | `45DD00FD88D7CE1B66690E1A1B6FFE732A343F3C728D84B4FF8956F1F4F4197C` | metres, canonical +Z/+Y frame, trigger-wrist grip, renderer-bound ends, seven semantic markers, muted material |
| `firearm-long-gun-derivatives/rifle-normalized.fbx` | `9D9288D04DEED70A6CA7AA321A2107B0F482431A082A1E2EDF4B50CB14742072` | metres, canonical +Z/+Y frame, trigger/lever-wrist grip, renderer-bound ends, seven semantic markers, muted material; Advanced Rifle only |

The generator fixes FBX creation metadata and UUID derivation so two clean
Blender processes produce byte-identical outputs. Its source-hash mapping is in
`firearm-long-gun-derivatives/generation-report.json`.

The Advanced Rifle payload is cleared by its adjacent `CLEARANCE.md`, exact
hash manifest, embedded identity, and user attestation. Loose audio WAVs remain
outside this model provenance manifest.
