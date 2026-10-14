# Sprint 23 follow-up: Triceratops Powerful Charge rounding

Status: **IN PROGRESS**. This file preserves the owner-supplied follow-up as the
active local contract. It revises released Sprint 23 only; it is not a repeat
of the art campaign and does not authorize Sprint 24.

## Endpoint and boundaries

Correct the mismatch between the Triceratops Powerful Charge helper's stated
live-Strength target and its separately rounded increment. Qualify the affected
source and native behavior, publish the next available stable patch when a
product change is required, download and verify the exact public artifacts,
restore HOWARD-LAPTOP, synchronize final RELEASED/STOP records through a narrow
documentation-only PR, and stop.

Work only in the Kingmaker Gunslinger/Tabletop Added Rules lab. Preserve all
existing releases, GUIDs, routes, art, saves and coherent work. Do not touch
DATA, Buff Planner, Mounted Combat, dinosaur bodies/icons, other creature
mechanics or global combat architecture. Do not reopen PR #41/#42, retag or
replace v0.0.152, or rewrite its historical qualification.

Product branch:
`codex/expanded-summoning-sprint23-charge-rounding`.
Final records branch:
`codex/expanded-summoning-sprint23-charge-rounding-release-records`.
Pushes use only the repository-authorized wrapper. Normal product and final
documentation PR merges and a stable patch release are explicitly authorized
after qualification.

## Frozen finding and correction contract

At released tested source `e0d7cb863b23925cfbf1721e3965c05e811a0bd7`,
`ArmoredDinosaurRulesPolicy.PowerfulChargeLiveBonus` returns
`floor(strengthModifier / 2)`. The caller adds that integer to ordinary weapon
damage. Positive odd modifiers therefore miss the stated final 2x contribution:

| Modifier | Native ordinary 1.5x | Released increment | Released total | Required total |
| ---: | ---: | ---: | ---: | ---: |
| +7 | +10 | +3 | +13 | +14 |
| +8 | +12 | +4 | +16 | +16 |
| +9 | +13 | +4 | +17 | +18 |
| +10 | +15 | +5 | +20 | +20 |

The first change must be a focused failing reproduction of the combined
helper/caller contract, not an assertion of the helper's old return value. For
positive `m`, after confirming the native ordinary contribution, the correction
is `2*m - floor(1.5*m)`. Do not double the complete damage bundle, hard-code
the printed baseline, add a synthetic attack, or alter critical, Power Attack,
enhancement, physical-form or damage-reduction ownership.

Installed Kingmaker 2.1.7b IL is the engine authority for signed handling. It
multiplies a nonnegative Strength modifier by the weapon multiplier and
truncates to an integer, but substitutes multiplier 1.0 for a negative
modifier. Consequently, the Triceratops-specific correction reaches 2x only
for positive modifiers; zero and penalties retain the native once-applied
contribution and receive no additional charge correction.

The Pathfinder source describes the individual Triceratops line as gore
2d10+12 and Powerful Charge gore 4d10+16. The universal rule directs readers
to each creature's entry; it does not impose this project's 2x live-Strength
interpretation on every monster. Source rules, source implementation findings
and native observations remain separately labeled.

## Finite qualification plan

1. Source reproduction and correction (30-minute ordinary slice): preserve the
   +7 failure; test +7/+8/+9/+10, a wider bounded positive range, zero and
   signed penalties, exact eligibility, unchanged fixed Rhino/Woolly Rhino
   increments and unrelated additive-damage separation. Run a named focused
   Sprint 23 gate with fresh production compilation.
2. Observer preparation (30-minute ordinary slice): extend only the existing
   Sprint 23 review to record native ordinary/charged weapon-stat and damage
   breakdowns at Strength 24/26/28/30, a valid penalty case, Power Attack and
   critical controls, markerless/opportunity/later controls, request-local IDs,
   action spending and exact restoration. Validate request wiring offline.
3. Candidate (30-minute gate target): after internal review, update current
   release metadata to the next unused patch, expected 0.0.153, freeze one
   coherent source, and run exactly one `Invoke-KmgGate.ps1 -Level Sprint`.
4. Native qualification (three independently restored transactions, each with
   a 15-minute case budget and a 900-second safety ceiling): standalone RTWP,
   standalone turn-based, and highest-risk-combined affected charge. Require a
   real odd-modifier charge hit in both combat modes, ordinary/aborted/AoO,
   bounded Power Attack, critical and released-rhino controls. Safe restoration
   always completes even if a budget expires.
5. Publication and verification (30-minute publication plus 15-minute public
   verification targets): merge the product PR normally, promote the exact
   tested ZIP without rebuilding, publish stable Latest, download ZIP,
   checksums and manifest, verify strict package identity/DLL/MVID/tag lineage,
   then restore and audit the machine.
6. Final records (15-minute documentation target): synchronize RELEASED/STOP
   text/link/JSON records on a separate branch and PR. Do not compile, package,
   bump version or launch the game solely for this reporting commit. Stop.

Reused v0.0.152 evidence is limited to unchanged bodies/icons, all twelve
routes, trample/trip, module lifecycle, persistence and broad historical
coverage. Do not rerun those unaffected campaigns. A changed observer is a
compiled candidate change and requires its affected native cases again.

## Safety

All launches use repository-owned guarded Steam App ID 640820 requests. The
protected baseline is immutable; only request-local disposable units are
needed for this correction. Snapshot actual installation, settings and saves
before every transaction and restore by its exact receipt/RunId. Never launch
Kingmaker.exe directly, load an incompatible save stack, clear shared caches,
edit saves, mutate human party equipment, or leave a candidate deployed.

Final disposition must be **RELEASED**, **RESOLVED WITHOUT PRODUCT CHANGE**, or
a genuinely evidenced **BLOCKED**. The report includes before/after arithmetic,
signed-input disposition, source/full/native provenance, old-consumer controls,
PR/tag/artifact identity, exact restoration, and confirmation that Sprint 24
was not started.
