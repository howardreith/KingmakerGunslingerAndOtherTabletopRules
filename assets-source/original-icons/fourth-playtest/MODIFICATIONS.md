# Modifications

`tools/New-RapidReloadIcon.ps1` deterministically removes green-dominant chroma
pixels, despills antialiased edge pixels, finds the non-key square icon bounds,
and bicubic-resamples the result to the 64-by-64 ARGB production PNG.
