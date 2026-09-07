# Elemental Races character-creation stabilization

## Active mission and acceptance

The owner's P0 stabilization instruction supersedes the earlier manual-test
pause. Character creation is currently **FAIL** by direct owner observation:
elemental heritages appear during Abilities, and ordinary ZFavoredClass Traits
have no options and block completion. Existing blueprint and synthetic-state
passes do not qualify that route. Elemental feat selection is a working control.
Human UI acceptance is **NOT RUN** for any new candidate.

This mission permits routing, selector compatibility, existing-content lifecycle
repairs and focused instrumentation only. Treacherous Earth and Nereid
Fascination must retain their registered identities but become unpublished.
No new content, master integration, merge, tag or public release is authorized.

## Startup evidence - 2026-09-07 UTC

- Exact starting local and fetched remote feature head:
  `c7df5de21ea2860b99a25777b021d7f44a5858fa`.
- Last production-code parent: `efc9d54ec29dbdd84ffe328183139f517c2f3350`.
- Branch: `codex/elemental-races-expansion`; starting worktree clean.
- `git fetch origin` succeeded. No newer `origin/master` commit is missing
  from this branch. No checkout, stash, reset, clean or merge was needed.
- Installed version: `0.0.117-elemental-traits`; DLL 6,264,320 bytes,
  SHA-256 `b3839a63fb83a5894169fa7ea2cbc1ef6e15f01229081b51d0f74b0d88984f05`,
  MVID `0a84af89-794b-46b5-b55e-13bf9959bcd6`.
- Installed UMM assembly version: `0.32.4.0`, SHA-256
  `1387468bc3af41c50fe51859a3bb7af4922891aa8f13a6187e7a348ceaabfd88`.
- Owner-captured game version: Kingmaker 2.1.7b. Preserved log reports UMM's
  `2.1.7` detection and Unity `2018.4.10f1`. Installed Assembly-CSharp SHA-256:
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
- FeatureModules.json SHA-256:
  `a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`.
- All eleven installed mod versions, top-level DLL and settings hashes, and
  exact UMM Params.xml bytes are retained in ignored startup evidence under
  `artifacts/qualification/0.0.117/character-creation-stabilization/startup-20260907T0315045809699Z`.
- Before any new launch, copied the existing 639,499-byte game output_log.txt
  (last write `2026-09-07T02:56:55.8389878Z`) and independently verified
  SHA-256 `948d59a90bc88669e53a1c9ca4bfe2b700527403a4ed80f55a752047d795d5e8`.
  The original remains untouched. No Kingmaker process was running.

The preserved log corroborates heritage registration, `Enabling Traits`, four
ZFavoredClass KeyNotFound exceptions, and Helpful publication changing Combat
Traits Features 0 to 1 and AllFeatures 14 to 15. That mutation is a hypothesis to
test, not yet an attributed cause. No new runtime qualification has occurred.

## Work and evidence ledger

1. In progress: inspect native/optional selection contracts and implement the
   read-only `observe-elemental-character-creation-routing` scenario before
   production repairs. Capture pre-reconciliation, post-reconciliation and
   actual first-level phase state without modifying foreign blueprints.
2. Pending: controlled profiles A-F and narrow repairs in distinct commits.
3. Pending: creation/back-navigation/respec, migration/persistence, visible
   trait lifecycle, full compatibility and module boundaries.
4. Pending: exact deterministic candidate, backup-first acceptance installation,
   owner checklist and actual owner UI report. Prior historical evidence is
   retained unchanged and does not close these gates.

All runtime launches must use guarded requests through Steam App ID 640820.
Only named disposable fixtures are permitted; KMG_AUTOMATION_BASELINE is
protected. Every compatibility transaction must restore exact prior folders
and settings. Screenshots, coordinates, OCR and Computer Use are not mechanical
evidence. Every coherent commit must pass its required checks and be pushed
using the prescribed external PowerShell wrapper.

## Checkpoint 1 - guarded routing observation, 2026-09-07 UTC

The first production change adds instrumentation only. No heritage/trait group,
choice publication, provider, feat or existing GUID has changed. The observer
arms only for the allowlisted `observe-elemental-character-creation-routing`
request. Coordinator hooks capture before and after optional reconciliation;
the runner captures the final live catalog. It reads published races and exact
Races Unleashed callback-held selections as well as the library enumeration.
It never invokes those callbacks or operates the character creator.

Snapshots record reference identity separately from GUID equality, both arrays
and their ordered entries, routing/visibility flags, prerequisites, race and
progression ownership, and any existing active controller, phase collections,
preview availability, selector layers, completion state and next-button state.
Absent active character creation is explicitly NOT-RUN. Observation PASS is
not a character-creation acceptance result.

Qualification commands and results:

- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1`
  passed repository validation, all **1,435** domain/reflection tests, clean
  exact-reference Release compilation, output validation, deterministic package
  creation and strict standalone UMM package validation. The three added tests
  cover reference collisions, array identity/order/null distinctions and the
  observer's read-only boundary. The build's inherited compiler response-file
  warning is unrelated to character-creation behavior.
- Guarded `observe-elemental-character-creation-routing`, Steam App 640820,
  full eleven-mod Profile F transaction `chargen-baseline-f-03`: **9/9 observation
  checks PASS**. Three checkpoints each contain 164 selections; published race
  count is 20. Thirteen Races Unleashed callback owners expose 29 alternate-racial
  selectors. All 492 selector observations preserve their inspected arrays and
  component references. No character or save was created or loaded.
- Exact original mod tree, file timestamps, managed sound bank and UMM Params.xml
  restored after every baseline transaction. The original installed DLL hash
  remains the startup hash. UMM enabled-state and module settings are preserved.

Machine-local evidence (ignored):
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260907T0355506028689Z-observe-elemental-character-creation-routing`.
Snapshot SHA-256:
`bbdbedbafe21303324ecc0cf0546fde1355b6279432f5980235d621287cdadf2`.
The result's inherited `Application.version` field reports UNKNOWN; the exact
installed game assembly hash and preserved UMM 2.1.7 detection remain recorded
above. This field does not independently confirm the owner-reported 2.1.7b.

### Exact live reference contract

| Selection | Group | Group2 | Groups | Features / AllFeatures |
| --- | --- | --- | --- | --- |
| Native Aasimar Heritage | AasimarHeritage | None | Racial | 0 / 7 |
| Native Tiefling Heritage | TieflingHeritage | None | Racial | 0 / 11 |
| Races Unleashed Dhampir Heritage | AasimarHeritage | None | Racial | 0 / 6 |
| RU alternate racial selectors (29) | AasimarHeritage | None | AasimarHeritage | 0 / varies |
| KMG elemental heritages (4) | None | None | empty | 3 / 3 |
| KMG replacement-slot selectors (10) | None | None | empty | populated, equal ordered entries |
| ZFavoredClass top-level Traits (2) | None | None | None | 0 / 8, unchanged |
| ZFavoredClass Combat Trait before KMG | Trait | None | Trait | 0 / 14 |
| ZFavoredClass Combat Trait after KMG | Trait | None | Trait | 1 / 15 |

Native IL token 0x060039C3 (`CharacterBuildController.DefineAvailibleData`)
routes groups 42/45 to Determinators with the Heritage label. Group None, and
Racial (11) by itself, use the generic Abilities branch. RU's exact installed
constructor sets its Dhampir selection's Group to AasimarHeritage (42), and
uses 42 for its alternate selectors. This establishes the routing defect's
blueprint/dispatcher mechanism; actual first-level consumption remains pending.
A blanket None-to-Racial replacement would not satisfy the native dispatcher.

Combat Trait is the same object (`ref-46`) before/after reconciliation.
Features changes from empty `ref-3141` to `ref-10193` with one entry;
AllFeatures changes from `ref-3142` (14) to `ref-10194` (15), appending exact
KMG Helpful GUID `e4b29a7c8d5f4c1796ab03e1f72d8456` after the original ordered
entries. Both top-level Trait arrays retain their original references and eight
categories. All eight categories are DLC-available. The native selection Items
getter reads AllFeatures, so this evidence alone does **not** attribute the
reported empty screen to the Features mutation. An actual creator/control
comparison is still required. Eastern Weapons independently adds one existing
Heirloom choice to Equipment AllFeatures (49 to 50), leaving Features empty.

Four ZFavoredClass custom-data KeyNotFound exceptions recur for the same
captured stack; they remain unmodified and unattributed to KMG pending the
KMG-disabled control. Native shader, missing-script and lightmap warnings are
retained separately. This run introduces no KMG structured ERROR.

The first two diagnostic iterations are retained, not overwritten:
`chargen-baseline-f-01` captured 134 selectors through library enumeration;
`chargen-baseline-f-02` added the published race catalog (135 selectors).
Inspection showed RU alternate selectors live in registered callback closures,
so the final iteration observes those exact objects without invoking them.
Those earlier observation passes do not claim complete reference coverage.

Instrumented test artifact (not an acceptance installation): source HEAD
`c7df5de21ea2860b99a25777b021d7f44a5858fa` plus the pre-commit source fingerprint
`942fff838f0853ce0e61f95fbc74bf119f5ef5d85ba5a504843210619a2cbca6`;
informational version remains `0.0.117-elemental-traits` at this instrumentation
checkpoint. Manifest count 1,867; package entry count 135.
ZIP SHA-256 `c0ade5b65f82983d12df81392cb9a9a1ad9973c437fb81b86d74a0b758f6d77e`;
DLL SHA-256 `6f48fc0163d593b054d7660ebd5f717aa0112cd39c04a6521a15cd792e150516`;
MVID `346e1500-c823-4b91-a998-bb15a62cee71`.
An exact copy is retained under the ignored
`artifacts/qualification/0.0.117/character-creation-stabilization/instrumentation-qualified/`.

All P0 player-facing acceptance gates remain open. Profiles A-E, a true
KMG-disabled control, active first-level selection/phase inspection and full
character completion are the next work. No repair, merge, tag or release has
been performed at this checkpoint.

### Installed optional-mod DLL identities at mission start

| Mod | Version | SHA-256 |
| --- | --- | --- |
| BagOfTricks | 1.16.4 | `d03626594ece0f339aeef03ec7259684af7ddeb8b2edf41936f144848059a0e6` |
| CallOfTheWild | 1.14.4c-2.1 | `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915` |
| CheatMenu | 1.2.3 | `7d659eb092073ab9e059414f8bfbdfe46991cede56b394f58f7ea2bcc1ff0845` |
| CraftMagicItems | 2.1.0 | `4ae2da61470350b31beef162717a604c9ccd322f66193917944ea4a9596e392d` |
| KingmakerBuffPlanner | 0.0.16 | `4c7249ad7a953522ea755e8bb46c5b89b136b9479a7360440526f417fb171597` |
| KingmakerDiceRoller | 0.1.2 | `962d5968d5021db2868d39104b4cbfc3911fe1de00ebde85974a1a2b1b977acd` |
| KingmakerLastAzlantiPreserver | 0.1.0 | `89dc72c1b331a818a7e6112b5c1d6b5d8d51d8027d74e1bc3d3168fb653ada23` |
| RacesUnleashed | 1.0.11 | `6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0` |
| TweakOrTreat | 1.1.0 | `a518324e15632aba46d6c467b156a31e9afd282e9827dee3e79ad14673852b92` |
| ZFavoredClass | 1.3.1 | `dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c` |
