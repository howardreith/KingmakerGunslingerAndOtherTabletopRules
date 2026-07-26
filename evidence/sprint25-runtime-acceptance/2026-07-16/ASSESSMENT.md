# Sprint 25 runtime acceptance assessment

The supplied 0.0.25 screenshots satisfy the bounded second-misfire exact-wielder gate.

- Fresh-process explosion counters began at zero.
- The first forced misfire changed the exact item from Normal to Broken, recorded `notRequired=1`, and scheduled no damage.
- The empty/Broken item reloaded while preserving Broken and consumed one powder/Lead Ball pair.
- The second forced misfire changed the exact item from Broken to Wrecked.
- Hedwig, the exact current wielder, made one native Reflex save against DC 12 and succeeded with total 23.
- Kingmaker reported successful-save half damage and applied 4 piercing damage from the exact runtime 1d10 weapon formula.
- The final diagnostic reported `scheduled=1`, `attempts=1`, `applied=1`, `notRequired=1`, `rejected=0`, `duplicates=0`, and `faults=0`.
- HP moved from 28 to 24 and the exact item ended empty/Wrecked.
- Misfire, attack, reload, AC, trace, and token-reconciliation diagnostics showed no relevant fault or duplicate.

Decision: Sprint 25 is runtime-accepted. Sprint 26 entry is approved for native burst targeting only.
