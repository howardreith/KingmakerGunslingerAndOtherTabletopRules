# Sprint 28 runtime acceptance — 0.0.28

Date: 2026-07-17

## Decision

**PASS. Sprint 28 is runtime-accepted. Sprint 29 entry is approved.**

## Evidence

The supplied Kingmaker 2.1.7b / UMM 0.32.4 run proves the player-facing same-item overhaul contract:

- Overhaul became available only when exactly one Wrecked Test Musket was equipped and at least one Firearm Repair Kit was present.
- Cancelling the full-round action by moving before delivery left the firearm Wrecked, consumed no repair kit, and left `attempts=0`, `completed=0`, `rejected=0`, `faults=0`.
- A completed delivery changed the exact item from empty/Wrecked to empty/Broken.
- The repair-kit stack changed from 3 to 2 exactly once.
- The exact runtime item and repository identity were preserved; the state revision advanced exactly once, from 11 to 12.
- The second blueprint-identical Test Musket remained independently Normal.
- Reload became available after overhaul, proving Wrecked-to-Broken recovery remains distinct from reload.
- A repeat Overhaul attempt was unavailable while the item was Broken.
- The empty/Broken state survived save and load.
- Visible attack, misfire, explosion, reload, AC, trace, overhaul, and state-token diagnostics remained free of relevant faults and duplicate applications.

## Blocking criteria

| Criterion | Result |
|---|---|
| Missing-kit rejection | Pass |
| Wrecked-only availability | Pass |
| Full-round interruption consumes nothing | Pass |
| Exact one-kit consumption | Pass |
| Same exact item Wrecked → Broken | Pass |
| No ammunition creation or consumption | Pass |
| Second-item isolation | Pass |
| Repeat-use rejection on Broken | Pass |
| Save/load persistence | Pass |
| Relevant faults and duplicates | Pass — zero observed |
