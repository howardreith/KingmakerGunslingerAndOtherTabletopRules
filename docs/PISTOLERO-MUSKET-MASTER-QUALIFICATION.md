# Pistolero and Musket Master Qualification

Status: implementation and feature qualification in progress.

## Baseline

- Source: `10b792735db5d685b46749dc08ea819f31fa8052`, version 0.0.72.
- Repository validation: PASS.
- Complete deterministic suite: 911/911 PASS when run with required authorized
  temp-directory access. Restricted temp access reproducibly denied the audio
  staging test's atomic replace and is recorded as an environment boundary.
- Clean exact-reference Release/build-output/SoundBank/strict package: PASS.
- Package SHA-256:
  `C9EC17E87805D3E1C93DC1879FBAC300E3BE0493AB422CE93B2445556D0BC4FE`.
- DLL SHA-256:
  `895D0EA7F1D4CB7658CA9C81B3F478D75C29A5FCEA8839E44908BE6E13F525FF`.
- Guarded Steam PASS evidence:
  - `20260808T0332364552630Z-mod-load-smoke`;
  - `20260808T0334429458961Z-observe-class-blueprint-contracts`;
  - `20260808T0336494740671Z-observe-gunslinger-presentation`.

The inherited detached Dodge failure and the two distinct Call of the Wild
evidence classifications are recorded in the journal. No feature evidence or
runtime qualification is claimed yet.

## Current feature evidence

- Source: `49a545d638b930dbb6decd2119e00f6c3585fd58`, version 0.0.72.
- Guarded Steam class-blueprint PASS:
  `20260808T0517582472885Z-observe-class-blueprint-contracts`, runtime run ID
  `20260808T0517582785424Z-83c886dcc18c400f9fcac681db3fbf55`.
- The structured result proves one exact Musket Master on the Gunslinger parent,
  exact six replacement/addition rows including the existing Rapid Reload
  (Musket), `ReplaceStartingEquipment`, the exact production Musket/powder/ball/
  kit references, and exact starter-resolver configuration.
- Runtime DLL SHA-256:
  `63394DFC43F15FBBEEE82B14581EB3EAD0B1B9A3AC2015C570F76AD3CF11BA63`.
- No save interaction occurred. This is blueprint/catalog evidence only; live
  character-creation inventory, battered ownership, and deed mechanics remain
  unqualified.

The earlier directory
`20260808T0514160336987Z-observe-class-blueprint-contracts` is not Musket Master
evidence: its PASS contained only the inherited class assertions because the
new assertion call was registered in the wrong scenario. The correction and a
method-local source test preceded the passing run above.

Steady Aim's live blueprint contract passed under source
`a0fe0e8018216f8e73d57e5c925e34edd98d04d0` in guarded directory
`20260808T1204211142376Z-observe-class-blueprint-contracts`, runtime run ID
`20260808T1204211365604Z-b1036159eeab40a3a958d461fa91b107`. The result proves
the move action, exact feature/ability/marker linkage, and shared Grit identity;
it does not yet qualify behavioral attack consumption or range delivery.

Pistolero's live progression/catalog contract passed under exact source
`90cc34d82934a9abd639e57706b65598ea88881f` in guarded directory
`20260808T1213268469463Z-observe-class-blueprint-contracts`, runtime run ID
`20260808T1213268625740Z-654d9af3b1d9489b82c858f8d5f4d37a`. The result proves
the exact parent, replacement rows, inherited production-Pistol resolver, and
one-copy deterministic project archetype ordering. It does not qualify either
Pistolero deed's mechanics or live character-creation inventory.

Up Close and Deadly's live blueprint contract passed under exact source
`fe50f6b53e28b6f7e77276f1eda4b8078b66f1de` in guarded directory
`20260808T1227026428237Z-observe-class-blueprint-contracts`, runtime run ID
`20260808T1227026740842Z-0d8a298531cc4bda99418f0490b71ea8`. The result proves
the visible free action and exact feature/ability/marker/class/shared-Grit
references. Behavioral attack and damage qualification remains pending.

Twin Shot Knockdown's live blueprint contract passed under exact source
`5a3b65fa8a38014c1f562aae8df25ab2b0867c68` in guarded directory
`20260808T1235416856236Z-observe-class-blueprint-contracts`, runtime run ID
`20260808T1235417118457Z-5b58767f21c244c09765fbd04e829081`.
The result proves the targeted free action, feature-owned tracker, AddFacts
link, and shared-Grit reference. Behavioral qualification remains pending.

The first clean combined Pistolero mechanics slice passed under exact source
`ebafc8ba8b87d6976448797a9f9e65fddb48f2a7` in guarded directory
`20260808T1309347685793Z-disposable-pistolero-deeds`, runtime run ID
`20260808T1309347962805Z-5e29dca207904e6fafa2aed622f074c8`.
It proves Up Close hit/miss/immunity grit and marker effects, Twin Shot distinct
event and target isolation, native Prone delivery, and cleanup. The remaining
critical/misfire/scatter/Dead Shot/expiry/fault/True Grit branches are not yet
claimed and remain required in the expanded final scenario.

## Required final evidence

The final report will separate deterministic, exact-reference, package,
standalone runtime, save/reconciliation, optional-mod profile, bounded Call of
the Wild, inherited-blocker, and human-only evidence. An ambiguous result is a
failure. Exact run IDs, source SHA, version, package/DLL hashes, structured
result paths, and transaction restoration records are mandatory.
