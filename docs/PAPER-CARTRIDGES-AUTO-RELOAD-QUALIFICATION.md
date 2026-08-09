# Paper Cartridges Qualification

Status: autonomously qualified on `codex/paper-cartridges-auto-reload`; not merged.

## Evidence policy

Mechanical runtime claims require the repository's guarded Steam App ID 640820
request mechanism and structured evidence. A build, deterministic suite, visual
observation, screenshot, OCR, direct executable launch, or unguarded UI input is
not runtime proof. Runtime fixtures are request-local and save-free unless an
explicit documented working-save smoke names `KMG_AUTOMATION_WORKING`; the
baseline save is never selected or modified.

## Required final evidence

- Complete deterministic/static/repository/Release/output/SoundBank/package gates.
- Scenario preflight and exact-build mod-load smoke before feature sequences.
- Disposable reload, native full-attack, misfire, Scatter, Lightning Reload,
  crafting/vendor, and comprehensive scenario PASS evidence.
- Two consecutive comprehensive PASSes from independent fresh processes.
- Standalone, Arms & Armor, Toggle Custom Soundpacks, combined, and one bounded
  Call of the Wild sequence with exact compatibility restoration.
- Exact run IDs, process freshness, package/DLL hashes, version, blueprint counts,
  Bokken outcome, inherited blocker status, clean-tree audit, and local/remote SHA.

## Final evidence

- Version: `0.0.74` / `0.0.74-paper-cartridges-auto-reload`.
- Complete deterministic suite: 954 PASS. Repository, clean Release,
  build-output, SoundBank, strict package, preflight, and request gates PASS.
- Final independent comprehensive runs:
  `20260809T0223531574928Z-disposable-paper-cartridge-comprehensive` and
  `20260809T0228043712566Z-disposable-paper-cartridge-comprehensive`.
- Working-save smoke runs:
  `20260809T0230156578884Z-working-save-smoke` and
  `20260809T0232431480084Z-working-save-smoke`.
- Compatibility restoration-verified transactions: standalone
  `compat-20260809T013637Z-4660ecf4446e`; Arms & Armor
  `compat-20260809T015702Z-08d5c0a31965`; Toggle Custom Soundpacks
  `compat-20260809T020420Z-0100b5e97026`; qualified combined
  `compat-20260809T021134Z-a8ec10bef81e`; one bounded Call of the Wild smoke
  `compat-20260809T021852Z-bf16e6df813d`.
- Registry/ledger: 248 active / 249 stable identities including one reserved.
  Six active identities were appended; no old identity changed.
- Bokken: evidence-backed defer only. Smith and all four installed BTSL tables
  passed exact 200-count publication; renewable crafting passed.
- Final DLL SHA-256:
  `24C06ABAADB0F6CD9BD9BDE1153766C5F343933D93D1CE3F5FD6B94750A1B928`.
  Local-runtime package SHA-256:
  `19AE04841664CF5C54C02D70140D932ED30315DA5026184874F1E20D8B16CE94`.

## Human-only visual/player checklist

One human may optionally use a disposable copy of `KMG_AUTOMATION_WORKING` to:

1. Load a Paper Cartridge, save under a new disposable name, exit, and reload it.
2. Confirm the chamber still reports Paper Cartridge, then fire it and confirm the
   next loose reload no longer reports the Paper +1 modifier.
3. Visually confirm the Paper item/mode icons, right-click Reload auto-use hint,
   Lightning action label, Smith stock, and crafting text at normal UI scale.

This is the only unperformed human-only item. It is not represented as mechanical
runtime proof and must never overwrite `KMG_AUTOMATION_BASELINE`.
