# Processing record

`tools/New-SupplyItemIcons.ps1` deterministically despills and removes the flat
green chroma background from both preserved high-resolution sources, then uses
.NET `System.Drawing` high-quality bicubic interpolation to export 128-by-128
ARGB PNG files under `assets/game/icons/`. The exporter requires a transparent
region and fails if any materially opaque, strongly green chroma fringe remains.

Source and export SHA-256 values are recorded in `SHA256SUMS.txt` beside this
record after export.
