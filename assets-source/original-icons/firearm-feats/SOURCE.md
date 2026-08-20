# Firearm feat icon source

These six PNG assets are original project-owned deterministic renders produced by tools/New-FirearmFeatIcons.ps1 from icon-spec.json.

The design authority is the accepted Nodachi presentation path, not its item PNG. CustomWeaponSelectorRuntime constructs native FeatureUIData with a null sprite and the NO monogram, allowing Kingmaker to render its category-glyph treatment. Firearm feat choices are stable explicit blueprint features rather than parametrized rows, so the generator reconstructs that grammar without changing their identities: aged pale field, original calligraphic monogram, restrained border, wear, and compact margins. It copies no native pixels.

Rapid Reload uses the same project-owned field with an original oxblood circular-return/ramrod motif and restrained blue corner accents. It no longer uses the rejected dark medallion.

Segoe Script and Georgia are Windows system fonts used only while rendering; no font file is copied or packaged. The JSON specification and PowerShell vector drawing code are editable source art. firearm-feat-icon-map.png contains deterministic 64-pixel and 32-pixel inspection rows. SHA256SUMS.txt records all six runtime assets and the contact sheet.