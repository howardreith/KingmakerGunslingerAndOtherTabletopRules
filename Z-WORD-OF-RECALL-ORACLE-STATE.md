# Word of Recall Oracle Scroll Eligibility — Mission State

Branch: `codex/z-word-of-recall-oracle-scroll`
Milestone: `0.0.126-word-of-recall-oracle`

## Owner report

Sayan, the owner's Call of the Wild Oracle, could not use a Word of Recall
scroll. Word of Recall belongs on the cleric/oracle list at spell level 6.
The teleportation system otherwise works; ordinary inventory activation of
strategic scrolls is intentionally blocked, so the defect had to be
exercised through the world-map destination action.

## Demonstrated cause

- `TeleportationSpellListPublication` publishes the canonical Word of Recall
  ability to the native Cleric 6 / Druid 8 lists only
  (`src/KingmakerGunslinger/Blueprints/TeleportationSpellListPublication.cs`).
- The installed Call of the Wild 1.14.4c-2.1 exports its own separate
  `OracleSpellList` blueprint (`f305174b73f64783a8379238a14c3283`) for the
  Oracle class (`32c02466b2364c8a906e6e4761175099`); the base publication
  never reaches it.
- The native reader predicate `BlueprintAbility.IsInSpellListOfUnit` walks
  `Progression.Classes -> ClassData.Spellbook.SpellList` with
  reference-equality membership in `SpellsByLevel[level].Spells` (verified
  from the native IL dump), so a zero-UMD Oracle fails the class-list check
  and `TeleportationScrollAdapter.IsReader` refuses the row.

## Repair

`src/KingmakerGunslinger/Spells/Teleportation/TeleportationFinalLiveReconciler.cs`,
attached once from `Main.Load` after Shield Other's reconciler, mirroring the
qualified Shield Other final-live pattern:

- At the first idle update after every optional mod has loaded, resolve the
  optional Oracle class through validated identity and structure (known CotW
  Oracle GUID, self-owned spontaneous divine Charisma spellbook reaching
  level 6; ambiguity throws and is logged, absence is safe).
- Merge the canonical Word of Recall exactly once at level 6 of that class's
  actual final spell list through `ShieldOtherSpellListMergePolicy`, clear
  the native `m_SpellsFiltered` cache, validate singularity, roll back
  exactly on failure, and run the whole pass twice idempotently.
- Fail closed without touching other modules when the TeleportationSpells
  module is off, the base publication is absent/failed, or a foreign
  duplicate Word of Recall ability exists.
- No spell, scroll, or vendor GUID changes; native Cleric 6 / Druid 8
  registration and rollback behavior untouched; Call of the Wild remains an
  optional load-time presence, never a dependency.

## Source gates (commit 1e4f4be2)

- Repository validation PASS at 0.0.126 (new
  `tools/validate_word_of_recall_oracle126.py` chain).
- Complete deterministic suite: 1,581/1,581 PASS (five new Oracle regression
  tests in `TeleportationOracleRecallTests.cs`).
- Clean exact-reference Release build and strict package validation PASS:
  package `KingmakerGunslinger-0.0.126-local-runtime.zip`
  SHA-256 `7225c2df424be6690ee875f068d189505d2de698a18338b062ac2cee0fdcf3a7`,
  release DLL SHA-256
  `a62f8a99469b50066471d7c347a7af70bc57ed453acea1f32c4548d4af45fb5d`.

## Runtime before/after (guarded working-save scenario, CotW installed and enabled)

- BEFORE control (reconciler compiled but inert via a temporary
  explicitly-permitted one-line dirty state; run
  `20260911T2107590264877Z-disposable-teleportation-scrolls`, Steam App
  640820, `KMG_AUTOMATION_WORKING`, loaded 0.0.126):
  - `oracle-final-list-recall-exactly-once` FAIL observed
    `levels=1;refs=0;guids=0` — the genuine Oracle level-6 list exists and
    contains no Word of Recall.
  - `oracle-native-classlist-eligibility` FAIL observed `eligible=False;umd=0`
    — the owner's exact failure.
  - `oracle-reader-row-offered` FAIL observed `uses=absent`; activation
    skipped (`oracle-activation-skipped {reason: row-absent}`).
  - 38/41 assertions PASS: Cleric/Druid/Wizard readers, UMD behavior,
    cancellation, variant, market-chain, and vendor-migration controls all
    intact.
- AFTER (committed repair, clean HEAD; run recorded below): see the next
  section.

## AFTER result

Run `20260911T2111133920070Z-disposable-teleportation-scrolls` (clean HEAD
`1e4f4be2`, Steam App 640820, `KMG_AUTOMATION_WORKING`, loaded 0.0.126,
CotW installed and enabled): **44/44 assertions PASS, no error**.

- `oracle-final-list-recall-exactly-once` observed `levels=1;refs=1;guids=1`
  — the production final-live reconciliation alone placed the canonical
  Word of Recall exactly once at Oracle level 6 (the fixture never touched
  the list).
- `oracle-native-classlist-eligibility` observed `eligible=True;umd=0` —
  the native class-list predicate passes for a zero-UMD Oracle.
- `oracle-reader-row-offered` observed `uses=2` (shared stock).
- `oracle-reader-not-know-spell` observed `knows=False` before and after.
- `oracle-scroll-teaching-intact` — the standard scroll still teaches its
  canonical ability.
- `oracle-sanctuary-destination-known` observed
  `known=True;established=False;destination=758559f4...` (Oleg's Trading
  Post, pre-capital).
- `oracle-sanctuary-row-composed` observed `row=2`.
- `oracle-sanctuary-cast-exactly-one` observed
  `committed=True;stock=2->1;booksUntouched=True;stillUnknown=True;party=758559f4...`
  — the confirmed world-map scroll action relocated the party to the
  sanctuary, consumed exactly one scroll, spent no spell-slot resource, and
  left the Oracle not knowing the spell.
- All 36 pre-existing assertions (Cleric/Druid/Wizard readers, UMD-only and
  per-spell negative controls, cancellation, UMD-failure refusal, crafted
  variants, ordinary-use refusal, vendor migration, market chain) PASS.

## Canonical committed-source artifacts

The qualified AFTER deployment rebuilt from committed HEAD (deployment
manifest `20260911T2111133564896Z`): package
`KingmakerGunslinger-0.0.126-local-runtime.zip` SHA-256
`7d96dc85dff6d3813955a7464a491e8437604ce1beea783fea2be02f11b1e781`; release
DLL SHA-256
`99a8ab3e454c5cb4b8b74fc13bee09e6f00893c79d0cc065da27ce3b857681fc`
(`dllSha256 == deployedDllSha256`). The commit message's earlier
`7225c2df.../a62f8a99...` pair was the pre-commit dirty-tree build; the
committed-source pair above is the one the guarded run qualified. Owner
`FeatureModules.json` preserved exactly across deployment (backup ==
installed == manifest
`a3fb0a2136547c5467d65469a782570b7e61ff9e3a83314197789b4095ea4749`); live-mod
backups retained under `runtime-backups/live-mod/`. No Kingmaker process
remained.

## Determinism

The complete domain suite ran three times on the committed source with
byte-identical results: 1,581/1,581 PASS each run.

## Honest boundaries

- The full compatibility-profile matrix (standalone, races-unleashed,
  highest-risk combined, etc.) was NOT rerun for this release; the focused
  qualification ran on the live installed owner profile, which has Call of
  the Wild enabled — the exact environment class of the owner report.
  `compatibilityRuntimeQualificationPending` remains true in
  `validation/static-validation.json` for that reason.
- Sayan's actual save was not touched; the owner's live campaign and the
  protected `KMG_AUTOMATION_BASELINE` save were never loaded or written.
  The oracle control reader was a fixture Oracle class structure on a
  party member of the disposable `KMG_AUTOMATION_WORKING` save, with
  membership supplied solely by the production reconciler.
- Level-6 Oracle spell-selection visibility is covered structurally (the
  reconciliation clears the native `m_SpellsFiltered` cache the level-up
  selection reads, mirroring the qualified Shield Other pattern) but was
  not exercised through a level-up UI scenario in this run.
- No public release was published; the candidate remains on the mission
  branch pending owner approval.

## Public release record

Owner approved publication on 2026-09-11. PR #16 merged the mission branch
to master (73b16a95). Tag `v0.0.126` published
`KingmakerGunslinger-0.0.126-word-of-recall-oracle.zip`:
https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.126
Asset SHA-256
`ee9c00eef064b2b93727051bd55311b622a5c7f8a0d4c695ce725bb370682edf`; release
DLL SHA-256
`1ed5dec089b9d6973342d6e9b425f5fa09afcd57bf8641b3c688dfd4e923756e`
(Publish-Release default MSBuild pipeline; two deterministic builds matched,
strict package validation PASS, `--latest`). The published source is the
merge of the runtime-qualified branch; the exact-reference DLL qualified in
game was `99a8ab3e...` from the same source.
