# Z teleport polish + specialist mission — durable state

Companion to `Z-TELEPORT-POLISH-AND-SPECIALIST-MISSION.md` (the contract,
delivered as an attachment; eight requirements). This file records progress,
evidence, and exact resumption instructions only. Prefix mission records live
in `Z-TELEPORTATION-MISSION.md` / `Z-TELEPORTATION-STATE.md`.

## Repository / branch / artifact

- Repository: `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`
- Branch: `codex/z-teleport-polish-specialist` (from master `258decb8`,
  which contains PR #12 and the merged 0.0.123 character-visibility repair)
- Active version: `0.0.124-teleport-polish-specialist`
- FINAL artifact (all runtime results below attribute to this build):
  package `artifacts/local-runtime/0.0.124/KingmakerGunslinger-0.0.124-local-runtime.zip`
  DLL sha256 `007921e837c1b24a2664d7d3c6aed80c334c212fb71af8771027dadb43ed5c16`
  deployment manifest
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20260911T1608184364456Z/deployment.json`
  (installed == package == build exact-match guard PASS; owner
  FeatureModules.json preserved every deployment; rollback backups under
  `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/`).
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

## Requirement status (all eight implemented and runtime-verified)

| Req | Change | Runtime proof |
|---|---|---|
| 1 rows inside parchment | rows sized to the settled region of the dialog's ACTIVE native action buttons (Button or ConsoleButton), inset 8 units, flexibleWidth=0, post-settle containment verify; donor fallback when nothing active; viewport height from the complete laid-out content incl. group separators (`ViewportHeight` policy) | coexistence compact-rows-fit (rendered extents within the native region); casting native-layout; gamepad/coexistence-gamepad; interaction long-list scroll behavior |
| 2 direct GT cast | `Begin` dispatcher; `OpenDirect` synchronous settle through the same transaction/execution machinery; shared `Settle`; per-action offer policy (`TeleportBeginPolicy`: an unrelated active modal blocks EVERY action incl. direct casts — `OpenDirect` re-checks it and the rows withdraw beneath a modal — while a merely unavailable confirmation presenter removes only confirmed spells from the list, never the Greater Teleport action); one Pending guard; `LastDirectCast` diagnostics | casting greater-exact; interaction cast-after-frames + cast-duplicate-activation (same-frame and stale re-invokes of the retained event) + direct-blocked-by-unrelated-modal; arrows audit; destinations special-point casts; gamepad greater-direct-settlement |
| 3 confirmation sections | `ConfirmationSections` + presenter-owned hairline rules measured from TMP textInfo; destination row-group separators (desktop+console); missing native label contract reported, not silently skipped | interaction confirmation-section-rules / confirmation-rule-placement (independent between-groups placement check) / confirmation-rules-cleaned-up (no leftover rules in a later unrelated dialog); console kept plain |
| 4 GT success suppression | `SuppressSuccessAnnouncement` in `PublishResult`; diagnostics unchanged | domain policy test + casting/interaction commit paths (failures still announce; see limitations) |
| 5 somewhere-else copy | `ArrivalMessage` outcome templates, new localization keys; `Result.UnnamedPoint`/`Result.Arrived` usage removed | domain tests (exact six sentences) |
| 6 target-location copy | same | domain tests |
| 7 Teleport specialist | PostLoad reconciliation (root cause above) | **specialist-cache scenario PASS** (now incl. world-map spend of the repaired favorite-only preparation at L5 and a genuine Conjuration-specialist-without-knowledge negative control) (incl. twice consecutively on the qualifying build and once on the final build): stale-state reproduction (favorite slots REFUSE both spells while the book knows them and the list contains them) → production PostLoad seam restores membership → native UI prepare/mixed counting/rest → negative controls (evoker untouched, ConeOfCold refused, blank book auto-learns nothing) → idempotence → exact cleanup |
| 8 Greater Teleport specialist | same repair, exercised independently at L7 | same scenario: independent L7 assertions + world-map direct spend of the repaired favorite-only preparation at L7 |

## Validation completed

- Repository validation PASS (dispatches 0.0.124 to `validate_teleport_polish124.py`).
- Full domain suite: 1,574 tests PASS, 0 failures (seven new).
- Clean exact-reference Release build + strict package validation PASS
  (Build-Local) at every commit; runtime scenario preflight 471 PASS.

## Runtime evidence (guarded, Steam App 640820, KMG_AUTOMATION_WORKING)

REVIEW-CORRECTION SWEEP — every teleportation scenario PASS on the exact
final artifact (DLL 007921e8…): specialist-cache PASS (12/12 incl. the
repaired-favorite world-map casts at both levels and the
Conjuration-specialist-without-knowledge negative), interaction PASS (incl.
confirmation-section-rules, confirmation-rule-placement with per-rule
coordinates, confirmation-rules-cleaned-up, cast-duplicate-activation, and
direct-blocked-by-unrelated-modal), casting PASS, specialist PASS,
coexistence PASS, arrows PASS, scrolls PASS, gamepad PASS, travelers PASS,
destinations PASS, coexistence-gamepad PASS (20260911T16xx evidence dirs).
Zero save writes in every runn (write sentinels asserted). Development fixes made during qualification:
scenario allowlisting (request validator + RuntimeAutomation metadata +
version gate), inactive-donor extent regression (rows measured from ACTIVE
native buttons — the donor Accept control is inactive at world-map points),
single-button vs full-region width (the two-line rows need the Travel+Cancel
span), console extent from ConsoleButton controls, and destinations/gamepad/
arrows fixtures routed through the direct settlement.

## Evidence log

- 2026-09-11 (session 1): mission surveyed; installed artifact identified
  (0.0.123 contains PR #12 publication — owner reproduction is current); root
  cause proven (IL dumps + native Spellbook.cs/PostLoad chain + CotW patch
  disassembly); branch created; eight requirements implemented; 5 new domain
  tests; version 0.0.124 bump across identity files, validator lattice,
  compatibility profiles, preflight/qualification literals; 1,574 domain tests
  PASS; Build-Local PASS; deployed with backup; checkpoint pushed.
- 2026-09-11 (session 2): guarded runtime qualification as above; all
  teleportation scenarios PASS on the final artifact; state/acceptance
  documentation updated; final checkpoint pushed.

## NOT RUN / limitations

- The owner's exact character/campaign save was not exercised (the bundle
  contains no save), so the diagnosis is a credible mechanism matching the
  reported symptoms, not a confirmed inspection of that particular save. The
  constructed specialist-cache fixture reproduces the reported stale-cache
  state through native seams (list rollback → AddSpecialList → AddKnown →
  publication restored) — the state an existing specialist save presents at
  load — but no disposable-save deserialization round-trip through the
  authorized persistence workflow was run this mission; that lifecycle step
  remains explicitly unqualified for requirements 7/8.
- VISUAL ACCEPTANCE REMAINS PENDING OWNER REVIEW: the divider rules now have
  structural runtime assertions (count, between-group placement verified
  independently from the rendered text mesh, ownership, cleanup after close
  and across a later unrelated dialog), but no screenshots were captured and
  the owner's 1920×1200 geometry and other supported resolutions/UI scales
  were not exercised by automation. The machine's display settings were not
  modified autonomously. Before/after owner screenshots 01-09 document the
  prior state; manual visual confirmation at the owner's resolution is the
  remaining acceptance step for the visual polish (requirements 1 and 3).
- `disposable-teleportation-disabled` and the four-phase persistence suite
  were NOT rerun this mission (their dedicated orchestrators exist:
  Invoke-TeleportationHardeningQualification /
  Invoke-TeleportationPersistenceQualification); the disabled-scenario gate
  was updated to include the new patch's Installed flag.

- 2026-09-11 (RELEASED): owner approved after merging PR #13 (master
  398d2b7c). scripts/Publish-Release.ps1 -Publish -ConfirmReleaseReady ran
  from clean master == origin/master: two deterministic builds, strict
  package validation, tag v0.0.124 at 398d2b7c, GitHub release published
  (not draft, not prerelease):
  https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.124
  Asset KingmakerGunslinger-0.0.124-teleport-polish-specialist.zip
  sha256 0fd6915eae93150c819c0272564fb709af21fcb82d222672572cd1f5c5b85a52
  (release DLL sha256 6b091de6857465b3eb569c949311d899b45370d1d60fcb68acb4346b839c3f7c;
  differs from the runtime-qualified 007921e8 only by the embedded commit —
  branch commit vs merge commit, identical source). The released package was
  deployed locally (installed DLL == release DLL, owner settings preserved,
  backup 20260911T1706587689181Z). Record branch
  codex/z-teleport-polish-release-record flips the static release
  authorization flags and syncs the committed release notes to the published
  body; owner merges it.
- 2026-09-11 (review corrections, final): the definitive battery passed on
  DLL 007921e8 and the candidate was redeployed. Engineering fixes found by
  the new scenario code itself during qualification: the native dialog
  renders the message through GetSaberBookFormat (first character pulled
  inside font/color/size tags), so ownership detection and section boundary
  scans operate on the rendered label text with the trailing unsplit section
  as the gate; the placement check is orientation-agnostic for TMP's y-down
  text space; the destination viewport height is computed deterministically
  from row and separator extents.

## Acceptance report (eight items)

1. Rows inside parchment — DONE (settled active-native-region sizing +
   rendered containment verification; scenarios PASS as above).
2. Direct Greater Teleport — DONE (no second confirmation, one in-flight
   guard, transaction/execution reuse, desktop + gamepad, scrolls and
   prepared/spontaneous sources through the same Begin dispatcher).
3. Confirmation sections — DONE (hairline rules measured from rendered
   text; console keeps plain text; group separators in destination rows).
4. GT success suppression — DONE (policy + presenter; diagnostics intact).
5. Somewhere-else copy — DONE (complete-sentence templates).
6. Target-location copy — DONE (legacy fallback removed).
7. Teleport specialist — repair qualified through the native load seam,
   native UI prepare/rest, and an actual world-map cast from the repaired
   favorite-only preparation. A disposable-save deserialization round-trip
   through the persistence workflow remains NOT RUN: the fixture invokes the
   production-patched public Spellbook.PostLoad on a live book rather than
   reloading a serialized stale save. Requirement 7 is runtime-qualified
   EXCEPT that save-round-trip lifecycle, which stays explicitly unqualified.
8. Greater Teleport specialist — same qualification scope as 7, exercised
   independently at level 7 including its own world-map direct cast; the same
   save-round-trip limitation applies.

No merge, no public release; owner review required.
