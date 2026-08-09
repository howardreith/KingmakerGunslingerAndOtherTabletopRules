# Paper Cartridges Qualification

Status: fully qualified. The rejected pre-repair evidence remains historical;
the repaired 0.0.74 candidate passed autonomous qualification and human recovery
acceptance on 2026-08-09.

The minimal unchanged-GUID repair passes 954 deterministic tests and every clean
exact-reference build/package/SoundBank gate. It is not yet runtime acceptance:
live party-view lifecycle, compatibility, comprehensive, and human failing-save
recovery checks remain pending.

## Persistent-mode repair qualification

- Deterministic: 954 PASS, 0 failures; repository/static, exact-reference Release,
  output, SoundBank, strict package, preflight, and compatibility restoration gates PASS.
- Attached-view lifecycle PASS run IDs:
  - standalone `20260809T0340349302370Z-7f894e1b28664af984306eef39612c6f`;
  - Call of the Wild `20260809T0346128762528Z-8c837fa06a0f41999496fddc7ef265a8`;
  - Arms & Armor + Toggle `20260809T0350165216971Z-649bd1f9aa7b4d09b8280f0de4b92501`;
  - all-three bounded profile `20260809T0354295434718Z-67bfad7adc7c4550b581734a079262ed`.
- Restored compatibility transactions: `compat-20260809T034315Z-dee461202246`,
  `compat-20260809T034810Z-8ebd1f999603`, and
  `compat-20260809T035130Z-fae78f2aaf1b`.
- Paper reload PASS `20260809T0359265378728Z-8f08b6388e42428f8d5c1b335a8b3c4d`.
- Independent comprehensive PASSes
  `20260809T0401569084331Z-7b195c2e2e744efba4f9bd9303ee7ed6` and
  `20260809T0404227443323Z-2901a80a736e4d009ba96743c244fda0`.
- Canonical working-save smoke PASS
  `20260809T0406443165230Z-06b59b103a254dab86098def126464e8`;
  this is general save compatibility evidence, not misreported as active-marker proof.
- Final DLL SHA-256 `54DFF704555E402905E56D41C2F7628939BD649913AB5083B33888AEC0D9BC3A`.
  Distinct repair package SHA-256
  `1DC1CB0E23C278253A9B5C0C77340A3527BED088C5B0EAFA4AE0DD9A1C4D3D60`.
- Autonomous qualification is complete. Release acceptance remains pending only
  the user's load/transition/reload sequence on a preserved previously failing save.

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

## Human-only recovery acceptance checklist

Preserve copies of the currently failing quicksaves, install the distinctly named
repair package without editing any save, then:

1. Load one previously failing quicksave and confirm the character and Paper mode
   state reconstruct.
2. Transition into the dungeon, then to its second level.
3. Save to a new disposable slot and reload that new slot.
4. Turn Paper mode off and transition; turn it on and transition again.
5. Confirm ordinary reload, Paper reload, and one full attack still work.

The decisive result is step 1: a previously failing save loads with the unchanged
marker GUID. Do not delete/edit saves, overwrite `KMG_AUTOMATION_BASELINE`, or
remove Call of the Wild from a save that depends on it.

Human acceptance completed 2026-08-09: the user successfully loaded a previously
affected game and traveled between dungeon floors without the modal exception.
This closes the final recovery and area-transition acceptance gate.
