# Character Visibility Repair — Durable Mission Record

## Assignment

Repair the reported regression in
`howardreith/KingmakerGunslingerAndOtherTabletopRules`: newly created
characters have invisible bodies while clothes and weapons remain visible,
during both new-game character creation and mercenary recruitment, and the
character remains invisible after creation.

Source mission document: `Kingmaker_Character_Visibility_Repair_Z_Mission.md`
(repo root, untracked assignment).

Delivery gates (all required for completion):

1. Supported causal explanation with the first actual failing resource or
   model-assembly operation, demonstrated behaviorally (unfixed vs fixed).
2. Narrowly scoped repair following repository conventions; no new
   dependencies; stable blueprint/appearance identities; save-safe.
3. Focused regression tests plus repository validation, complete domain-test
   suite, clean Release build, installable-package validation.
4. Native qualification of the final artifact through BOTH reported creation
   paths (new-game creation, mercenary recruitment) to commitment and world
   appearance, plus persistence/compatibility coverage per mission section 6.
5. Reviewable pushed branch (`codex/z-character-visibility-repair`); no merge,
   tag, release, or PR.
6. Verified backup-first local installation for Howie's playtest with exact
   identity checks and rollback procedure.

## Constraints reconfirmed from AGENTS.md and the mission document

- Guarded `-kmgRuntimeTestRequest` mechanism only; Steam App ID 640820;
  no direct Kingmaker.exe launch; no OCR/coordinate navigation.
- `KMG_AUTOMATION_BASELINE` never touched; `KMG_AUTOMATION_WORKING` only
  through its identity-verified contract; mission-owned disposable saves only
  through proven guarded leases.
- Preserve unrelated work, foreign mods, settings, and saves.
- No autonomous merges, force-pushes, history rewrites, destructive resets.
- Publication only through the approved wrapper
  `C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1`.
- Human visual acceptance remains pending until Howie reports it; do not
  claim it.

## Durable records

- Mission/state: `CHARACTER-VISIBILITY-REPAIR-STATE.md` (updated every
  checkpoint; includes exact resume instructions).
- Final report: `CHARACTER-VISIBILITY-REPAIR-REPORT.md` (written at
  completion; incomplete gates reported as FAIL/BLOCKED/NOT RUN).
- Machine-local evidence root:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/character-visibility-repair/`.

## Baseline snapshot (2026-09-10, mission start)

- Host: Windows 10 (win32 10.0.19045 x64); lab at
  `C:\Dev\KingmakerGunslingerLab`.
- Repository root: `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`;
  remote `origin = howardreith/KingmakerGunslingerAndOtherTabletopRules`.
- Starting branch: `master` at `221f6080` ("Advance active version to
  0.0.122-teleportation-completion"), clean worktree except the untracked
  mission assignment document.
- Mission branch: `codex/z-character-visibility-repair` (created from master;
  no other branches checked out; no stash/reset/clean used).
- Previous review reference commit: `ab9eb762823fe706abb68df05c9f42d93aea8eb1`
  (0.0.121 unified-firearm-repair merge). Its resource-lifetime /
  global-creator-prefix explanation is treated as a hypothesis to verify.
- Installed game: Steam
  `C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker`,
  Kingmaker 2.1.7b, Unity 2018.4.10f1, UMM present with a full owner stack
  (BagOfTricks, BetterVendors, CallOfTheWild 1.14.4c-2.1, CheatMenu,
  CraftMagicItems 2.1.0, EddicKingmakerRespec, KingmakerBuffPlanner,
  KingmakerDiceRoller, KingmakerBugfixes, KingmakerLastAzlantiPreserver,
  ProperFlanking2, RacesUnleashed 1.0.11, SkipIntro, TweakOrTreat 1.1.0,
  ZFavoredClass 1.3.1).
- Installed mod at mission start: `Mods/KingmakerGunslinger` version 0.0.122
  (installed 2026-09-10 20:55 by the just-completed teleportation cycle),
  DLL SHA-256
  `6965AAB9326BBEEF6DFF845B46DD6912C8081BBC87E6EEF88F875A0368E9CEC1`.
  MVID to be recorded at first qualification build.
- Log preservation: the owner's original failing-session output_log.txt was
  already overwritten by the 0.0.122 teleportation qualification runs before
  this mission began. The surviving log (a 0.0.121 automated run, last write
  2026-09-10 19:53) was preserved to
  `runtime-evidence/character-visibility-repair/baseline-20260910/output_log_root_copy.txt`.
  It confirms 28 elemental visual proxies register at bootstrap under the
  owner's real stack (0.0.121), but contains no character-creation session;
  no one of the four retention-failure messages appears in it.
- Source SHA at record creation: `221f6080` (branch carries mission docs on
  top).

## Investigative baseline (to be extended as evidence lands)

- The hypothesized chain exists at this baseline:
  `ElementalCharGenVisualRetentionPatch` prefixes
  `CharGenDollRoom.DollStateUpdated` for every race and delegates to
  `ElementalRaceVisualResourceRegistry.RetainCharacterCreatorResources`,
  which throws `InvalidOperationException` on four lost-resource conditions
  ("Owned character creator visual resource was lost:",
  "Owned character creator inner asset was destroyed:",
  "Native visual donor was unloaded:",
  "Native visual dependency inner asset was destroyed:").
- Elemental visual registration is unconditional at bootstrap
  (`BlueprintBootstrap.InitializeCore`), so the prefix applies to every
  character creation regardless of module state.
- History: the retention patch itself was the 0.0.117-era repair for native
  inner-asset destruction (see
  `ELEMENTAL-RACES-CHARACTER-CREATION-STABILIZATION.md`); Visuals sources
  have not changed since `df6b28c3` (0.0.120 work), so any regression is
  expected to come from interaction with newer changes, a different lifecycle
  boundary (mercenary recruitment, scene/save-load, fresh process), or an
  unqualified path — to be determined by evidence.
