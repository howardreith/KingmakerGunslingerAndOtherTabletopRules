# Release notes 0.0.127 — firearm maintenance, full-rest recovery, and misfire interruption

**Status: engineering qualification IN PROGRESS on the 0.0.127 candidate.**
This record is updated as gates complete; unrun gates stay explicitly
marked. No public release is authorized by this document.

- Informational version: `0.0.127-firearm-maintenance`
- Mission: `Z-FIREARM-MAINTENANCE` (assignment and approved contract in
  `Z-FIREARM-MAINTENANCE-MISSION.md`; developer contract in
  `docs/FIREARM-MAINTENANCE-CONTRACT.md`).

## Behavior changes

1. Repair Firearm is an out-of-combat, Broken-only full-round maintenance
   action: native party-level combat authority, at-least-one reusable
   Gunsmith's Kit in shared inventory, exact-target binding at command
   start rechecked at delivery, same-item restoration with preserved
   enchantments/origin/rounds, nothing consumed. The hidden legacy Overhaul
   alias delegates to the same checks.
2. A genuine completed full rest automatically restores the party's carried
   Broken and Wrecked firearms to Normal (Wrecked stays unloaded) once per
   rest, before the native post-rest autosave, with one concise summary or
   one clear cannot-restore explanation.
3. A newly committed misfire break during an attack sequence stops the
   remainder of that sequence and its automatic continuations (remaining
   full-attack iterations, free auto-reload, reload-resume, immediate
   real-time auto-attack re-issue). A later deliberate player attack with
   the Broken gun remains possible.
4. Wrecked firearms remain unable to fire or reload; Quick Clear,
   ammunition crafting/economics, misfire chances, Broken penalties, burst
   balance, identities, and vendor behavior are unchanged.

## Qualification record

| Gate | Result |
| --- | --- |
| Repository validation | PASS (dispatched 0.0.127 validator) |
| Deterministic domain tests | PASS 1,611/1,611 (30 new mission cases) |
| Clean Release build (exact references) | PENDING |
| Installable package validation | PENDING |
| Guarded native lanes (misfire interruption, field-repair rejection, completed-rest restoration, cancelled-rest no-op), 2 runs each | PENDING |
| Persistence round trip (fresh-process load) | PENDING |
| Compatibility/module matrix | PENDING |
| Owner visual/play acceptance | PENDING (separately tracked) |

Evidence index: `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`;
run artifacts under `C:\Dev\KingmakerGunslingerLab\runtime-evidence\`.

## Retained baselines

This Kingmaker Gunslinger 0.0.127 release carries the retained qualification
counts forward: the inherited Gunslinger-fixes baseline of 1,288 tests, the
fatigue-authority baseline of 1,325 tests, and the current deterministic suite
of 1,611 tests all pass.

The installable archive is
`KingmakerGunslinger-0.0.127-firearm-maintenance.zip`. The qualified
firearm SoundBank is retained unchanged: `KMG_Firearms.bnk` SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Foreign assemblies — `CraftMagicItems.dll` remains an externally installed
optional mod and is never bundled.

## Owner smoke test

See `SMOKE-TEST-GUIDE.md` (0.0.127 section): fresh break, deliberate later
Broken shot, combat/field repair rejection, full-rest restoration, and
Quick Clear.
