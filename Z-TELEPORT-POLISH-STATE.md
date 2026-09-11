# Z teleport polish + specialist mission — durable state

Companion to `Z-TELEPORT-POLISH-AND-SPECIALIST-MISSION.md` (the contract,
delivered as an attachment; eight requirements). This file records progress,
evidence, and exact resumption instructions only. Prefix mission records live
in `Z-TELEPORTATION-MISSION.md` / `Z-TELEPORTATION-STATE.md`.

## Repository / branch / artifact

- Repository: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`
- Branch: `codex/z-teleport-polish-specialist` (from master `258decb8`,
  which contains PR #12 and the merged 0.0.123 character-visibility repair)
- Implementation commit: `868db529` (pushed through the approved workflow)
- Active version: `0.0.124-teleport-polish-specialist`
- Qualified package: `artifacts/local-runtime/0.0.124/KingmakerGunslinger-0.0.124-local-runtime.zip`
  sha256 `e6cedaf54bd1e1cb22aa44b50f4d39b25dab689bb3600e6f57419fc1f838f28f`
- DLL sha256 `0ba5ed7659b2e44965851e4c1f6a966e3a091ba99b4fd0aa92ce9c1ad7ba4a7a`,
  MVID `bb14757a-1c89-425a-be81-2de366f46ba8`
- DEPLOYED (installed == package == build, exact-match guard passed):
  deployment manifest
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260911T1101585269392Z/deployment.json`;
  owner FeatureModules.json preserved
  (`featureModuleSettingsPreserved=true`); rollback backup
  `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260911T1101556659444Z`.
- Pre-mission installed artifact (owner reproduction baseline): 0.0.123,
  DLL sha256 `99FC60BA...` — the owner reproduced the specialist defect on a
  build that ALREADY contained the PR #12 Conjuration-list publication, so the
  report is not an old-build artifact.

## Root cause (R7/R8) — proven at IL + repository-evidence level

Favorite-slot eligibility is the per-book special-spell cache `m_SpecialSpells`,
serialized in saves and derived ONLY at (a) school-feature activation
(`AddSpecialList`, from then-known spells; for non-AllSpellsKnown books) and
(b) learn time (`AddKnown`, when the spell is in an attached special list at
that moment). `Spellbook.PostLoad` (called solely from
`UnitDescriptor.PostLoad`) rebuilds only `m_KnownSpellLevels`; fact
collections run `Fact.PostLoad` and never re-run component
`OnFactActivate` (repo-verified in `ElementalHeritageRuntime.cs`; the prior
mission's own fixture note also observed AddFact not running the component).
AllSpellsKnown books self-heal via `TryRestoreKnownSpells` inside PostLoad.

Therefore a Conjuration specialist whose Teleport/Greater-Teleport knowledge
predates the mod's publication into `WizardConjurationSpellList`
(`69a6eba1...`) keeps a stale cache forever: the favorite slot rejects the
spells (`GetMemorizeSlots` requires `GetSpecialSpells` membership) and the
spellbook shows no specialist styling, exactly as the owner reported — while
newly constructed books work (the prior mission's fixture attached
`AddSpecialList` AFTER knowledge + publication, masking the defect).

Call of the Wild compatibility was investigated: CotW's
`ClassToProgression+AddSpecialSpellList__OnFactActivate__Patch` only skips the
original when the book is missing (verified by IL dump); the native list
identity remains correct with CotW active.

## Repair

`TeleportSpecialistSpellCachePatches` (module-gated, fail-soft Harmony postfix
on `Kingmaker.UnitLogic.Spellbook.PostLoad`) restores, for exactly the
published Teleport/Greater Teleport spells, the native invariant
(known ∧ in an attached special list ⇒ special) via the private native
`AddSpecial(int, BlueprintAbility)` seam. Additive, idempotent, inert when
the module is off (list membership absent), never auto-learns, never grants
slots, never touches other schools/books. Pure decision logic lives in
`TeleportSpecialistSpellCachePolicy` (domain-tested).

## Requirement status

| Req | Change | Code | Domain tests | Runtime proof |
|---|---|---|---|---|
| 1 rows inside parchment | settled donor-extent width, flexibleWidth=0, post-settle containment verify (desktop+console) | done | 2 new layout tests | PENDING scenarios |
| 2 direct GT cast | `Begin` dispatcher, `OpenDirect` synchronous settle, shared `Settle`, `CanBegin` gate, `LastDirectCast` diagnostics | done | covered via presenter/scenarios | PENDING scenarios |
| 3 confirmation sections | `ConfirmationSections` + presenter-owned TMP-measured hairline rules + destination group separators | done | sections join keeps content (existing tests) | PENDING scenarios/screenshots |
| 4 GT success suppression | `SuppressSuccessAnnouncement` in `PublishResult`; evidence unchanged | done | new | PENDING scenarios |
| 5 somewhere-else copy | `ArrivalMessage` templates, new keys, `Result.UnnamedPoint`/`Result.Arrived` usage removed | done | new | PENDING scenarios |
| 6 target-location copy | same templates | done | new | PENDING scenarios |
| 7 Teleport specialist | PostLoad reconciliation (see above) | done | new policy test | PENDING `disposable-teleportation-specialist-cache` |
| 8 Greater Teleport specialist | same repair exercised independently at L7 | done | same | PENDING scenario (L7 covered in-scenario) |

## Validation completed

- Repository validation PASS (dispatches 0.0.124 to `validate_teleport_polish124.py`).
- Full domain suite: 1,572 tests PASS, 0 failures (five new).
- Clean exact-reference Release build + strict package validation PASS
  (Build-Local). Deployment identity above.

## Runtime scenario plan (guarded, installed 0.0.124)

1. `disposable-teleportation-specialist-cache` (NEW: stale rejection
   reproduced through native seams → production PostLoad repair → both levels
   independently → native UI prepare/rest/mixed counting → negative controls).
2. `disposable-teleportation-specialist` (prior fixture regression).
3. `disposable-teleportation-casting` (44 asserts; direct GT spec updated).
4. `disposable-teleportation-interaction` (direct GT + long-list).
5. `disposable-teleportation-coexistence` (rendered containment assertion).
6. `disposable-teleportation-arrows` (post-arrival first-arrow regression).
7. As time allows: scrolls / travelers / gamepad / destinations.

## Evidence log

- 2026-09-11: mission surveyed; installed artifact identified (0.0.123,
  contains PR #12 publication — owner reproduction is current); root cause
  proven (IL dumps + native Spellbook.cs/Spellbook.PostLoad chain + CotW patch
  disassembly in /tmp/ildump); branch created; all eight requirements
  implemented; 5 new domain tests; version 0.0.124 bump across identity
  files, validator lattice (validate_teleport_polish124.py + 7 historical
  validators), compatibility profiles, preflight/qualification script
  literals; 1,572 domain tests PASS; Build-Local PASS; deployed with backup;
  owner settings preserved; checkpoint commit 868db529 pushed.
- 2026-09-11: `disposable-teleportation-specialist-cache` launched (first
  guarded run against installed 0.0.124) — CHECK the newest
  `runtime-evidence/*Z-disposable-teleportation-specialist-cache` directory
  and this file's scenario table on resume.

## Next concrete actions on resume

1. Collect the specialist-cache scenario result; on PASS run items 2-6 of
   the scenario plan; on FAIL read its forensic JSON captures
   (stale-state / after-postload / cleanup) before changing anything.
2. Capture before/after screenshots of the destination popup and the
   confirmation sections at 1920x1200 as supporting visual evidence.
3. Write the eight-item acceptance report (below) and update
   `TELEPORTATION-COMPLETION-HANDOFF.md`-style summary; final checkpoint push.

## Acceptance report (to complete)

1. Rows inside parchment — PENDING runtime + screenshots.
2. Direct GT — PENDING runtime.
3. Confirmation sections — PENDING runtime + screenshots.
4. GT success suppression — PENDING runtime.
5. Somewhere-else copy — PENDING runtime.
6. Target-location copy — PENDING runtime.
7. Teleport specialist — PENDING runtime (root cause + repair described above).
8. Greater Teleport specialist — PENDING runtime.

NOT RUN / BLOCKED: owner's exact character/save was not exercised (no campaign
save in the bundle); a constructed fixture reproduces the reported stale-cache
state through native seams instead. Aspect ratios other than the fixture's
native geometry not separately exercised unless recorded above.
