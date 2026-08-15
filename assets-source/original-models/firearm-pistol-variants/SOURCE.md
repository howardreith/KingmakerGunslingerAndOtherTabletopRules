# Firearm Pistol variant source

`PistolDuelist` and `PistolLastWord` are project-owned clean-room models created
entirely by `generate_firearm_pistol_variants.py`. They do not contain geometry,
textures, or topology copied from the licensed Cyril43 service Pistol.

The service Pistol remains the preserved Cyril43 CC-BY-4.0 asset documented in
`assets-source/third-party/models/cyril43-flintlock-pistol/`. These two variants
supplement it; they do not overwrite the original archive or extracted source.

Generation requires Blender 4.5.10 LTS and `PYTHONHASHSEED=0`. The script emits
the `.blend`, two FBXs, normalized source renders, and the JSON build report.
