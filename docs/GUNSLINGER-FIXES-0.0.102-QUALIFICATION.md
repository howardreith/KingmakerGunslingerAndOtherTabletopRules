# Gunslinger fixes 0.0.102 qualification

## Candidate identity

- Branch: `codex/gunslinger-starter-bokken-feedback-acadamae-toggle`
- Authoritative base: `d14aa69fe2e95a5eecbee8e37e6aadf463303b48`
- Qualified dirty-source fingerprint:
  `abbd8bda0134d4ece43e5e4a344d7d098f7bfb1c9c1bc21d536eef732d33a7a1`
- Package SHA-256:
  `4f53c5dbea42d07d82168f95d753a177c0e022f7082ad9185adc5842063f7ac5`
- DLL SHA-256:
  `e981cc9be4b10136d7fb290fc51412c9d320a067f823e50f3894ed7d48793fe5`
- DLL MVID: `8bb47dcd-561a-48ce-ab13-c5bce327ed08`
- Deployment manifest:
  `runtime-evidence/deployments/20260825T2054507736757Z/deployment.json`

Generated packages, deployment files, saves, and raw runtime logs are not part
of the source commit.

## Automated gates

- Version-aware repository validation: PASS.
- Complete dependency-free domain/reflection suite: PASS, 1,251/1,251.
- Exact Kingmaker 2.1.7b reference Release compilation: PASS, zero warnings and
  zero errors.
- Build-output and focused supply-icon validation: PASS.
- Firearm SoundBank and manifest validation: PASS; bank SHA-256
  `0e9f88c562f4f937a8941ace0f241bb31a7ed56b46fbca549c98f764392edf18`.
- Deterministic package creation and strict standalone UMM package validation:
  PASS.

All final-candidate runtime launches used Steam App ID 640820 and the guarded
request mechanism. The request-local disposable scenarios made no save writes.

| Scenario | Assertions | Evidence directory |
| --- | ---: | --- |
| `disposable-firearm-penetration` | 5/5 PASS | `20260825T2055167194084Z-disposable-firearm-penetration` |
| `disposable-acadamae-graduate` | 17/17 PASS | `20260825T2057203805254Z-disposable-acadamae-graduate` |
| `disposable-overhaul-maintenance` | 8/8 PASS | `20260825T2059230590434Z-disposable-overhaul-maintenance` |
| `disposable-gunslinger-dead-shot` | 7/7 PASS | `20260825T2101214889246Z-disposable-gunslinger-dead-shot` |
| `disposable-archetype-reconciliation` | 40/40 PASS | `20260825T2103311489025Z-disposable-archetype-reconciliation` |
| `disposable-gunslinger-creation-commit` | 5/5 PASS | `20260825T2105421218014Z-disposable-gunslinger-creation-commit` |
| `disposable-gunslinger-levelup-preview` | 4/4 PASS | `20260825T2107458389992Z-disposable-gunslinger-levelup-preview` |
| `disposable-gunslinger-levelup-commit` | 4/4 PASS | `20260825T2109476495044Z-disposable-gunslinger-levelup-commit` |
| `disposable-gunslinger-multiclass-preview` | 4/4 PASS | `20260825T2111491097399Z-disposable-gunslinger-multiclass-preview` |
| `disposable-gunslinger-multiclass-commit` | 7/7 PASS | `20260825T2113502396880Z-disposable-gunslinger-multiclass-commit` |
| `disposable-gunslinger-respec-preview` | 4/4 PASS | `20260825T2115507395877Z-disposable-gunslinger-respec-preview` |
| `disposable-gunslinger-respec-commit` | 4/4 PASS | `20260825T2117520198049Z-disposable-gunslinger-respec-commit` |
| `observe-vendor-table-contracts` | 25/25 PASS | `20260825T2120033698670Z-observe-vendor-table-contracts` |

## Evidence boundaries

The save-free runtime reaches its disposable mechanics at the main menu, where
Kingmaker has not created `BattleLogManager.LogView`. The feedback scenarios
therefore prove the exact supported `BattleLogView.AddLogEntry` call is
attempted once per committed event, the message is concise, repeated
calculation does not duplicate it, detailed diagnostics remain in the mod log,
and an unavailable presentation sink cannot alter the mechanic. The injected
behavior test proves an available sink receives exactly one entry. Actual
visible placement in a loaded game's native combat log, and absence of the old
warning overlay there, remain human-gated.

Acadamae runtime qualification reproduced a marker lingering after the native
activatable was turned off. A newly constructed command was Full-Round and
unarmed, while a command genuinely constructed while on retained its one-save
snapshot. Cancellation/reselection and repeated state changes left zero
tracker records. This no-save fixture did not perform fresh-process ON/OFF
serialization. Native activatable persistence and the effective-state policy
are covered by source and behavior tests; actual save/load persistence remains
human-gated.

## Manual acceptance checklist

1. Bard 1 -> base Gunslinger 1 receives one Pistol.
2. Bard 1 -> Musket Master 1 receives one Musket and no Pistol.
3. Bokken sells all six required firearm-supply types.
4. Oleg sells none of the KMG firearm supplies.
5. Firearm AC information appears only in the combat log, with no warning overlay.
6. Acadamae Graduate ON accelerates and saves.
7. Acadamae Graduate OFF restores normal summoning.
8. ON -> OFF -> new summon remains Full-Round and produces no Acadamae save.
