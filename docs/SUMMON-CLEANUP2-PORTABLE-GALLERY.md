# Portable Cleanup 2 native gallery

The repository exporter creates a self-contained review ZIP from the existing
local gallery. It contains **188 original, byte-identical native PNG captures**,
a relative-link HTML index, README and bounded identity manifest. Extract the
whole ZIP and open `index.html`; each thumbnail links to the unmodified full
frame. No image service or account is needed.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Export-SummonCleanup2ReviewGallery.ps1 -OutputPath artifacts/cleanup2-portable-review-gallery-v2.zip
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Export-SummonCleanup2ReviewGallery.ps1 -Mode Verify -OutputPath artifacts/cleanup2-portable-review-gallery-v2.zip
```

Export refuses existing output, limits output to this worktree's ignored
`artifacts` directory, checks each gallery PNG against its original source and
identity SHA-256, and refuses unclassified PNGs. Four derived aids are explicitly
excluded: the Horse size/contact sheet and three menu crop sheets. The ZIP
includes no saves, DLLs, game meshes/textures, credentials, raw logs or complete
private observation graphs. No upload or transmission is performed.

The exporter reads these five existing identity lists:

- `native-image-identities.json`: 90 baseline/review 5 body frames;
- `review7-ettin-crab-identities.json`: 51 later lifecycle/attack frames;
- `review7-menu-identities.json`: 28 native menu pages;
- `review8-crab-identities.json`: nine ordinary Crab attack frames;
- `review9-tooltip-identities.json`: ten popup/hover frames for five targets.

For each exact raw run it extracts source commit, loaded DLL SHA-256/MVID,
version, phase, run ID, original verdict/counts/failed assertion names and
selected fixture observations. Hashes identify the original request, result
and loaded-identity JSONs, without redistributing those whole files. Local
absolute paths are replaced by evidence-directory and original PNG names.
Missing independent area attestation is stated rather than inferred.

## Retained results

| Group | Source prefix | Loaded DLL SHA-256 prefix | Original result |
| --- | --- | --- | --- |
| Baseline bodies | `489dde48816e` | `857b574ad26d` | FAIL, 60 PASS / 10 FAIL |
| Review 5 bodies | `acdb5d90ada7` | `8b8b2e1911f8` | FAIL, 60 PASS / 10 FAIL |
| Review 7 Ettin lifecycle | `a04e204bc01a` | `c574da54b7ef` | FAIL, 72 PASS / 2 FAIL |
| Review 7 Crab attack | `a04e204bc01a` | `c574da54b7ef` | FAIL, 14 PASS / 2 FAIL |
| Review 7 menus | `a04e204bc01a` | `c574da54b7ef` | PASS, 43 PASS / 0 FAIL |
| Review 8 Crab attack | `071142449b6b` | `eeaf6349df3c` | PASS, 26 PASS / 0 FAIL |
| Review 9 tooltips | `6e6f063486f5` | `62de58b91350` | PASS, 21 PASS / 0 FAIL |

The archive manifest contains the full identities. Newer evidence does not
rewrite old FAIL records. These runs used different frozen source/DLL inputs;
the gallery is not one combined candidate PASS.

## Review limits

Screenshots support presentation review. They do not grant owner art approval,
prove mechanics, calibrate giant height, demonstrate cold hydration, qualify a
release or prove the actual current installed mod stack. Earlier body/Ettin
findings remain unresolved where their structured run failed. Camera and
background can differ between baseline and later frames.

Menu evidence covers canonical SNA I–IX page coverage in a disposable caster
explicitly granted access. It does not prove natural learning or Druid/domain,
item, alternate-adapter or old saved-button access. The five tooltip targets
use native text-only HeaderItem cards: `tooltipArtDisplayed=false`. Their
source rows contain the approved icon; no card-header art is asserted.

Verification checks archive entry allowlisting, duplicate/path rejection,
image lengths/hashes, complete run identities, document hashes, relative links
and index reachability. It verifies consistency against the included manifest,
not a digital signature or independent native replay. Keep the ZIP SHA-256 in
the handoff; preserve the local identity lists and original raw runs.

Focused synthetic fixtures (no game/build/save access):

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SummonCleanup2ReviewGallery.ps1
```

They test positive export/verification, overwrite refusal, changed PNG,
unexpected DLL, changed index, duplicate path and source/gallery mismatch.
Actual ZIPs and fixture outputs stay ignored under `artifacts`.
