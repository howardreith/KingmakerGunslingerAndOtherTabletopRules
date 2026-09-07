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

## Native creator fixture development ? 2026-09-07 (in progress)

Instrumentation commit `aa63b22e6df9256f07157fd2ab845edc44849d19` is
pushed. The following native creator fixture work is a continuation of that
checkpoint. No production routing, trait publication, or GUID change has yet
been made at this checkpoint. These diagnostic results do not qualify the
installed candidate or override the owner's failed acceptance observations.

The fixture opens the actual full-screen `CharacterBuildController` with a
request-created `ChargenUnit`, `CharGen` mode and `NextLevel=1`. It uses native
portrait, race, class, racial-bonus, point-buy, skill, feature, and phase actions.
Choice selection requires both native legality and an active, interactable
rendered row holding the exact selection-state and feature references. It
records allocation state, phase order, unresolved selections, visible rows,
choice counts, and the actual phase collections consuming racial selections.
It never changes prerequisites, obligatory flags, selection arrays or game
rules to make a fixture pass.

Two scopes are now explicit:

- `disposable-elemental-character-creation-baseline` reaches native final review
  and cancels each disposable preview. It does not commit a main-menu character.
- `working-save-elemental-character-creation` requires the exact
  `KMG_AUTOMATION_WORKING` guarded load and its complete receiver/fingerprint
  evidence before allowing native final commit. This commit scenario has not
  yet been runtime qualified at this checkpoint.

A separate `disposable-global-traits-kmg-disabled-control` entry point runs
under a temporary `KMGCharacterCreationProbe` UMM manifest. It verifies that
KMG's actual UMM entry is unloaded/inactive, `Main` remains `NotStarted`, no
production context is published, no KMG blueprints are present, and no Harmony
patch has the production KMG owner. The DLL supplies diagnostics without
calling KMG's production entry point. The control uses native Human through
final review and cancels, so it cannot start a campaign. Its temporary folder
and UMM state belong to the compatibility transaction and are restored.

A request-scoped Harmony guard blocks the five exact native save mutation
entry points while the creator fixture exists. Unexpected write attempts fail
the diagnostic. Cleanup accepts native `OnHide` clearing its own controller
following commit, while rejecting every non-null foreign controller. The
original world-unit references, main-character reference and area reference
must be restored. Each profile restores the original mod directory tree and
exact original UMM Params bytes/hash. From run A05 onward a separate save audit
also requires the protected baseline to remain byte-identical, the working
save's non-header entries and all other header bytes to remain identical, and
at most one native `LoadedTimes` increment. Other save-file metadata must stay
unchanged. Audit evidence remains machine-local and ignored.

### Retained diagnostic iterations

All run evidence is under the external `runtime-evidence` root; all build and
transaction records are under the ignored
`artifacts/qualification/0.0.117/character-creation-stabilization/` directory.
Every completed profile below restored the exact original mod tree and UMM
Params hash `516869f3cb0822d11dfe4aa84431620e0eaf59af56cd8489ea537c818af7257f`.

| Run | Evidence directory stem | Exact finding / limitation |
| --- | --- | --- |
| A01 | `20260907T0414178705057Z-disposable-elemental-character-creation-baseline` | Actual Ifrit and Oread class selection put heritage plus three replacement slots in Abilities. The fixture initially used lower-level allocator actions without the UI preview refresh; it did not converge. Sylph also threw in the native color selector. Diagnostic FAIL. |
| A02 | `20260907T0429025819857Z-disposable-elemental-character-creation-baseline` | A new allocation snapshot used JObject for array-shaped native data and failed before character selection. Corrected to JToken. Diagnostic FAIL. |
| A03 | `20260907T0432205973635Z-disposable-elemental-character-creation-baseline` | Ifrit reached native final commit with the wrong racial route. Cleanup incorrectly rejected the controller that native OnHide had cleared. Diagnostic FAIL; this is not qualified completion evidence. |
| A04 | `20260907T0437013930971Z-disposable-elemental-character-creation-baseline` | Main-menu final commit triggered a native new-campaign transition and an unexpected SaveRoutine attempt, which the guard blocked. Diagnostic FAIL. Save-free profiles now cancel at final review. |
| A05 | `20260907T0444519014086Z-disposable-elemental-character-creation-baseline` | KMG-only profile could identify/load the working descriptor but native Player.PostLoad could not find its main character in CrossSceneState. Load-completion TIMEOUT before a creator opened. The save audit passed with exactly one LoadedTimes increment. No save repair was attempted. |
| Full-stack working-save control | `20260907T0451385377639Z-working-save-smoke` | PASS, 11/11 assertions, exact profile restoration. Same working save loaded under the full eleven-mod stack. Save audit passed, exactly one LoadedTimes increment. This separates the A05 profile prerequisite from a generally unreadable save. |
| A06 | `20260907T0500086180463Z-disposable-elemental-character-creation-baseline` | Ifrit and Oread reached native final review and were canceled cleanly; both still routed all four racial selections to Abilities. Sylph again threw in CharBColorSelector.SetData while reading a texture width. Diagnostic FAIL, save audit unchanged with zero load increments. The next fixture revision records ramp liveness and continues to the remaining disposable race after recording this exact native operation as failed race acceptance. |
| B01 | `20260907T0502469448408Z-disposable-global-traits-kmg-disabled-control` | KMG was actually skipped as disabled; production entry/blueprint/patch absence and cleanup were proven. Observation PASS is **not** Trait acceptance: the fixture omitted Human's racial ability bonus and stalled in Skills. Global Trait inspection was NOT-RUN. Native SetRacialBonus was added for the next control run. |

The actual first-level Ifrit/Oread states confirm the heritage root cause:
`FeatureGroup.None` is consumed by the native generic Abilities collection,
after the native allocation/skills phase. Native Aasimar/Tiefling and RU
contracts remain the comparison authority recorded above; plain Racial alone
would not fix the dispatcher. The global empty-Traits root cause remains open
until the corrected B control and KMG-enabled creator profiles reach the
actual Trait selectors. The Sylph color-selector exception is an additional
failed player-facing creation gate and must be diagnosed, not bypassed.

`native-creator-build-08.log` records repository validation, all **1,437**
domain/reflection tests, clean Release compilation, deterministic package
creation and strict 135-entry package validation. The only compiler warning
remains the inherited response-file `/noconfig` warning. The build is still a
diagnostic development artifact with informational version
`0.0.117-elemental-traits`, not an owner acceptance installation.

Build 08 ZIP SHA-256:
`052b6ce15c2701901e414cedda941c9e1e80f367347e9357e27acc92a2138e43`.
DLL SHA-256:
`4433a1a0e83b4a34b3a0a413b569aef7142298143bf5e9988e0ed58abe071e3e`.
Source commit is `aa63b22e6df9256f07157fd2ab845edc44849d19` plus the recorded
uncommitted source fingerprint; the guarded launch record binds the exact
source tree, package, installed DLL and MVID for each run. Earlier unsuccessful
iterations and their evidence have not been overwritten or erased.

Heritage repair, alternate-trait routing, no-op unpublication, Helpful contract
repair, real final completion, back-navigation, respec, full persistence and
lifecycle qualification, compatibility acceptance, and the final acceptance
installation are still outstanding. Human UI acceptance remains **NOT-RUN**.
Nothing has been merged, tagged or publicly released.


## Native global-Trait control and comparisons - 2026-09-07

The corrected Profile B control reached native Total/final review with
`State.IsComplete=true`, `RemainingSelections=0`, and the native completion
button enabled. Both ordinary Trait selections were consumed in Abilities:
first selection 8 extracted / 8 legal rendered categories; second selection
8 extracted / 7 remaining legal rendered categories. Combat had 14 extracted /
12 legal choices; Faith had 11 extracted / 10 legal choices. Native Human's
racial bonus and the deity's legal alignment were selected through native UI
actions. The final alignment was Chaotic Neutral. The preview was canceled;
no final commit or campaign start occurred. Human acceptance remains NOT-RUN.

B02 previously reached both Traits but failed the fixture's 240-operation
bound in Character details because its fixed True Neutral alignment was
incompatible with the selected deity. It did not fail at an empty Trait list.
B03 uses the native SelectAlignment.Check contract, records the three native
character-detail completion predicates, and reaches complete final review.

| Profile / run | Runtime evidence directory stem | Diagnostic result |
| --- | --- | --- |
| B / `chargen-native-b-03` | `20260907T0524314773304Z-disposable-global-traits-kmg-disabled-control` | PASS |
| C / `chargen-native-c-01` | `20260907T0527249164787Z-disposable-elemental-character-creation-baseline` | FAIL |
| D / `chargen-native-d-01` | `20260907T0531561519452Z-disposable-elemental-character-creation-baseline` | FAIL |
| E / `chargen-native-e-01` | `20260907T0536050809336Z-disposable-elemental-character-creation-baseline` | FAIL |
| F / `chargen-native-f-01` | `20260907T0539578489543Z-disposable-elemental-character-creation-baseline` | FAIL |

C (CotW + ZFavoredClass + KMG, Bodyguard OFF) and D (same, Bodyguard ON)
used the same exact Build 10 DLL and native creator fixture. In **both**,
Ifrit and Oread reached native final review with zero unresolved selections.
Both ordinary Trait choices worked: 8/8 first categories, 8/7 second categories,
and Faith 11/10. Combat changed from 14/12 in C to 15/13 in D. D's preexisting
KMG publication still changed Combat.Features from 0 to 1 and AllFeatures from
14 to 15. The isolated comparison therefore does **not** prove that this
mutation caused the owner's empty-Trait failure.

E added Races Unleashed; F restored the exact full eleven-mod stack, including
Bag of Tricks, Dice Roller, and TweakOrTreat. Both produced the same successful
Ifrit/Oread global-Trait and final-review observations. The racial selections
remained in generic Abilities, so every elemental acceptance remains FAIL.
No synthetic/blueprint PASS overrides that failure. The owner's exact race,
class and creator entry point have been requested to narrow the still-open
empty-global-Trait reproduction; no general foreign-mod repair is authorized
or implemented.

All five completed transactions restored the exact original mod tree and
UMM Params bytes/hash. Their independent save audits passed with zero working
load increments and byte-identical protected baseline. Each raw transaction
contains exact staged mod manifests/versions, settings hashes, deployment
identity, before/after selection snapshots, and preserved attributed logs.
Third-party custom-data exceptions remain in those logs without suppression.

C/D/E/F all reproduced a separate native Sylph appearance failure after Oread:
Oread skin ramp 0 (`CR_Skin_GrayDead_U_EL`) was alive, but the identical shared
texture at Sylph skin ramp 4 had become Unity-destroyed. Native
CharBColorSelector.SetData then threw while reading its width. The failed
native operation left the creator unable to initialize the next Undine phase;
those overall diagnostics are FAIL, and Undine remained NOT-RUN. This is not
character-creation acceptance for either race.

Native IL inspection identifies CharGenDollRoom.UpdateDollCoroutine as a caller
of EquipmentEntity.UnloadInnerAssetsExceptGiven; it constructs an exclusion
set from current and initially loaded inner assets, unloads other inner assets,
then calls ResourcesLibrary.TryUnloadResource. The next read-only probe records
exact KMG proxy membership in those initial sets and before/after inner-asset
unloads, without changing the exclusion set or native return values. It observes
Sylph last so that the known native failure cannot contaminate another race's
baseline. This is a narrower diagnostic strategy, not a workaround in production.

Build 10 passed repository validation, all **1,437** domain/reflection tests,
clean Release compilation, deterministic packaging and strict 135-entry package
validation. ZIP SHA-256:
`61e4b60c21ca135cb4b907550b4573742dea07b8cccb3ddf92d6ef777aadd40c`;
DLL SHA-256:
`aa364fd45182bcd4261e6d7a9eb41f85f927785519455391c65f68abdb7da9a7`;
MVID `24bded57-247b-415f-be54-94f657dfe772`.
Source: `aa63b22e6df9256f07157fd2ab845edc44849d19` plus source fingerprint
`748617bd56a3ccc520dbb0107947eef3cb9c24c55014fb507cd4288ac7dd84cc`.
No production behavior or identity has changed at this checkpoint.


### Narrower fixture and native visual-unload evidence

A07 (`20260907T0547342727864Z-disposable-elemental-character-creation-baseline`)
used Build 11 and remained FAIL, with exact transaction restoration and save
audit PASS. Moving Sylph last exposed the same failure in Undine instead;
Ifrit and Oread still reached final review, while Undine failed at the native
color selector and Sylph was NOT-RUN. The fixture's attempt to continue after
the failed native operation again found no active phase. No race was accepted.

The read-only before/after probe captured Oread's exact body proxy GUID
`1d661d42bdc24e8cb79a16f27e8e2a9e` and head proxy GUID
`d44896914e9e459385313890fcad7b56` during native inner-asset unloading. All seven
body skin ramps were alive beforehand, absent from the native exclusion set,
and Unity-destroyed immediately afterward. The head subsequently held the
same destroyed textures. The creator's initial loaded-resource set had zero
entries and protected none of the 28 registered KMG proxies; by the next
previews two, then four, registered proxies were themselves destroyed. Raw
caller stacks and reference evidence are retained in `assetUnloads` and
`dollRamps.dollRooms`. The probe never modified the native exclusion set or
return value. This proves a missing integration with native creator asset
retention; changing palette colors or suppressing the exception would not
repair the lifetime contract.

Build 11 passed all 1,437 domain/reflection tests, clean Release build,
deterministic package creation and strict package validation. ZIP SHA-256
`9eace45f56903d3bc60c62b43f1315d63921249da30dadea94bbe954dd40716c`;
DLL SHA-256
`8b2f6742387b87b7099575bc237c60a520edaad718c6916ad2c32d9afb659ae3`;
MVID `05e04dd3-1857-4cd1-b5d4-d7780c494899`.

A new bounded `disposable-elemental-character-creation-case` request selects
exactly one elemental race and either Fighter or Gunslinger, with point buy
or the installed Dice Roller 0.1.2 native Roll command. Both C# activation and
PowerShell preflight require exactly the three allowlisted string parameters
`race`, `class`, and `allocation`. It cannot load or save a campaign and always
cancels at final review. This supports independent fresh-process fixtures
without the known previous-preview asset failure contaminating another race.
For example, after the normal build/deployment identity checks:

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario disposable-elemental-character-creation-case `
  -ExpectedVersion 0.0.117 `
  -Parameters @{race='Ifrit';class='Fighter';allocation='roll'} `
  -ExitAfterCompletion:$true -Confirm:$false
```

The preserved owner log records `creationKind=NewMainCharacter`, mode Roll,
and a verified rolled array `15,6,13,12,16,11` through preview generations 5-9.
Previous controlled full-stack comparisons used point buy. That difference
requires an explicit rolled-stat reproduction; it does not attribute fault
to Dice Roller. The bounded rolled case verifies the exact native controller,
state and preview before invoking its command, then compares the verified
assignment with all six live preview BaseValues. It never recalls/stores
personal arrays or writes Dice Roller settings.

The first compilation of this additional case (Build 12) caught a missing
LINQ import in request validation; no runtime artifact was deployed from that
failed build. The import was corrected before complete validation was rerun.


### Strict request serialization recovery

Build 13 passed 1,437 domain/reflection checks, clean Release compilation,
deterministic packaging and strict package validation. The first bounded
rolled case (`20260907T0559454833069Z-disposable-elemental-character-creation-case`)
was rejected with `character-creation-case-not-allowed` before test activation:
PowerShell preflight accepted the three parameters, but its request serializer
omitted them. This is an instrumentation failure, not a character-creation result.
The C# guard correctly rejected the empty parameter object and was not loosened.

The exact owned Steam-launched process was identified by PID, start time and
executable path, then closed with CloseMainWindow/WaitForExit; no force termination
was used. After exit, the attributed output was preserved and the compatibility
transaction restored the exact original mod tree. UMM had changed only its native
LastUpdateCheck timestamp; the original bytes were restored and SHA-256 again
matched `516869f3cb0822d11dfe4aa84431620e0eaf59af56cd8489ea537c818af7257f`.
The independent save audit passed with zero load increments and exact protected
baseline. The ignored recovery record preserves the failed activation and recovery.

The serializer now retains the three allowlisted strings. Focused tests round-trip
all 16 race/class/allocation combinations through the actual JSON request writer;
the final failure check now runs after these added assertions. The profile helper
also preserves its starting UMM bytes before staging to make exceptional recovery
independent of a previous checkpoint. No production behavior changed.


### Rolled full-stack controls and instrumentation checkpoint

Build 15 passed repository validation, all 1,437 domain/reflection tests, clean
Release compilation, deterministic packaging, and strict 135-entry package
validation. The focused PowerShell request suite passed 223 checks. Two earlier
focused runs reported a changed artifact-tree fingerprint; the isolated rerun
passed and now reports changed paths on any future mismatch. Build 15's DLL and
ZIP are byte-identical to Build 13 (the serializer fix is PowerShell-only): ZIP
`f13a9425b70790aee3b1e92658dd60e74cd7a4098fd6dd64bd759915df7ab38f`, DLL
`f818063605fcb3b79fc25472b8112bf5735ab5e0d55bdddf95a7198825ca6d33`,
MVID `c9b906cb-1c82-491d-aedc-2bbcdeda6e8f`. Runtime source attestation was
`aa63b22e6df9256f07157fd2ab845edc44849d19` plus fingerprint
`1be56b2f07876b9a5a8f72bdd552bb38ad23e7fa95a6cc0c35d9aeb76dbc0d5e`.

F rolled Ifrit/Fighter (`20260907T0618157770950Z-disposable-elemental-character-creation-case`)
reached final review with zero unresolved selections, an enabled native button,
and complete character details. Dice Roller mode Roll, generation/verified
 generation 7/7, assignment `13,14,8,11,14,13` exactly matched preview BaseValues.
Both global Trait roots exposed 8 categories (8 then 7 legal); Combat exposed
15 entries/13 legal and Faith 11/10. The save-free fixture canceled without a
native commit. Diagnostic instrumentation PASS; character acceptance FAIL because
all four racial selections still routed through Abilities after Skills.

F rolled Ifrit/Gunslinger (`20260907T0622318421796Z-disposable-elemental-character-creation-case`)
also exposed both global Traits, with 8/8 and 8/7 categories, Combat 15/12 and
Faith 11/10. Its verified roll was `9,12,9,9,13,11`. It selected a legal feat and
reached Character with no remaining selections, but native NextPhase stayed
Character until the bounded operation limit. Character acceptance FAIL; no final
review or commit is claimed. Instrumentation and exact cleanup completed without
exceptions. The narrower completion-predicate observation remains outstanding.
Neither rolled control reproduced an empty global Trait list. Helpful's foreign
Features mutation remains a contract concern, not an established root cause of
the owner's empty-list report.

Both profile transactions restored the exact original mod tree and UMM bytes;
independent save audits passed with zero load increments and exact baseline.
Known ZFavoredClass custom-data errors remain attributed separately. The next
production checkpoint changes only heritage routing using the observed native
AasimarHeritage group (42); native Racial (11) also routes to Abilities and is
therefore not a valid repair. No production factory or Helpful publication has
changed in this instrumentation checkpoint.


### P0 A: native heritage routing repair (qualification in progress)

The heritage factory now uses Group=AasimarHeritage (42), Group2=None and
Groups=[Racial], matching the native Aasimar and installed Races Unleashed
heritage contract. This changes presentation only: all four race memberships,
twelve marker identities, three ordered choices, obligatory status, provider
reconciler, stat-overlay components, and resource identities are preserved.
The factory validates the complete presentation contract; the live heritage
scenario compares the constructed object with the exact native Aasimar object.
A focused regression pins the inspected route and rejects both generic routes
(None and Racial as primary Group). Alternate-trait routing and no-op publication
remain pending the next separate production checkpoint.


P0 A checkpoint evidence: `heritage-routing-build-02.log` passed repository
validation, all **1,438** domain/reflection tests, clean Release compilation,
deterministic packaging and strict 135-entry validation. The first build attempt
stopped at the static expected-test-count mismatch (1437 versus the new 1438),
which was corrected before compilation or deployment. ZIP SHA-256:
`eb77c3104203f9f7f6162c41d446d5e53680acde1c07af338384373213b3456d`;
DLL SHA-256:
`be963f06be80a74bae3d28bac85e386cc7c4cfa284c696aa4ec5b10f93766246`;
MVID `40c6b671-df8c-46e2-873d-c783e344d6f9`. Source attestation:
`3b3c80d477b496929612641f1965ba1658487047` plus fingerprint
`6967a922d3acac621c06bfd68a70ab87fea79ce52f82e2f2e00c5dc6831bb8b5`.

Profile A live blueprint comparison
`20260907T0638185669774Z-observe-elemental-heritage-blueprints`: **19/19 PASS**,
including all four exact native racial routing contracts and three choices each.
Profile F actual rolled Ifrit/Fighter
`20260907T0635008810797Z-disposable-elemental-character-creation-case`: diagnostic
**11/11 PASS**. Heritage marker selection occurred on Determinator/Choose heritage
before Skills, exactly once, with 3 extracted/3 legal rendered choices. It was
absent from the Abilities route. Dice Roller assignment `14,16,11,10,16,4` matched
live BaseValues at verified generation 8. Both global Traits and feats remained
populated, and the native creator reached Total with zero unresolved selections.
The three alternate-trait selectors still in Abilities correctly keep overall
character acceptance FAIL. This is the narrowly qualified heritage checkpoint;
all-race completion, back-navigation and owner acceptance remain pending.
Both transactions restored exact original Mods and UMM bytes, with independent
save audits PASS and zero load increments.

Correction to the preceding Gunslinger control summary: its last snapshot was
Skills, not Character. After completing character details, native preview
reconstruction returned to Skills with SkillPointsRemaining=-1. The fixture's
allocator only handled positive remaining points, so it repeatedly attempted to
advance rather than refunding the excess through the native control. Native
SpendSkillPoint(stat,false) calls UnspendSkillPoint, refreshes the preview, and
rebuilds the UI. A request-owned refund path is needed in regression coverage;
this evidence does not establish a production Gunslinger or global-Trait defect.


### P0 B/C: racial alternate-trait route and deferred choices (in qualification)

All ten replacement-slot selections now follow installed Races Unleashed's
alternate-racial-trait contract: Group=AasimarHeritage, Group2=None,
Groups=[AasimarHeritage]. Ordered retain-base choices remain unconditional and
obligatory. Their existing feature-array contract and stable identities remain.
Publication is now explicit for the 19 implemented candidates. Oread's existing
SLA selector retains its identity and offers only retain-base until a later
mission implements another legal choice.

Treacherous Earth marker `e117e1e0a17a4acec001000000000031` and Nereid Fascination
marker `e117e1e0a17a4acec001000000000040` remain registered, with their providers
and reservations unchanged. They are hidden and omitted from all player-facing
selection arrays. Retained development markers are validated as known identities,
then ignored before overlap/provider resolution: they cannot consume the SLA slot,
evict an implemented choice, or attach an inert provider. This is a narrow
conservative reconciliation of the two development identities, without save edits.

The factory verifies a concrete trait-specific provider for each published choice.
The live framework scenario asserts those mechanics and scans every constructed
Features/AllFeatures array for the two forbidden marker GUIDs. Its native matrix
now covers 60 published legal combinations across 180 heritage rows and both
activation orders (360 rows), while keeping 21 marker-first rows to exercise both
retained deferred identities. Every combination explicitly checks native retain-base
legality. Deferred-marker rows also spend the active heritage SLA and require
reconciliation/removal to preserve zero uses. These source changes are awaiting
complete source, build, package and guarded runtime qualification.


P0 B/C checkpoint validation: `alternate-routing-build-02.log` passed repository
validation, all **1,440** domain/reflection tests, clean Release compilation,
deterministic packaging and strict 135-entry package validation. The first
compilation caught a missing namespace import in the new provider validator;
that failed artifact was never deployed. A direct ordered manifest comparison
against mission-start `c7df5de2` verified all 1,867 symbols/GUIDs/types/statuses
unchanged. Only four deferred-identity notes changed in the manifest.

Qualified artifact: ZIP SHA-256
`1ca887bde2a64e0b8f1264d74f67a55c54f2f66644f4a2f6adef55be8e1cf501`;
DLL SHA-256
`6312f7995ef0582bec920f279e39a77190b5a812f87512c19e8e5ec3037c2343`;
MVID `13ed52b2-a108-4752-9a55-afcb914050b9`. Source attestation:
`23dffdc58fad63f6a674cda8513d44b8a683a1c0` plus fingerprint
`dd237ff3b56273f4baade9cbab60dd04ea7af4e08be322499f23c327ebadedfe`.

Profile A `20260907T0653109609068Z-observe-elemental-alternate-trait-framework`
passed **6,202/6,202** native assertions without exceptions. This includes all
19 exact trait-specific mechanic assertions, both exact deferred marker GUID
publication checks, 60 legal sets/180 heritage rows/360 activation-order rows,
21 marker-first rows including both inert retained identities, and native
retain-base legality throughout the combination matrix. Existing passive,
Insight, blood, daily-ability, breath, Crystalline and Breeze core probes also
passed in this process. This does not substitute for pending fresh-process
persistence, turn-based action costs or complete creator/respec qualification.

Profile F `20260907T0656438275743Z-disposable-elemental-character-creation-case`
completed Oread/Fighter's full native selection contract through Total with
zero unresolved selections and no per-character acceptance failures. Heritage
had 3 extracted/3 legal choices. Energy, Affinity and SLA replacement slots had
2/2, 4/4 and 1/1 respectively; every racial choice was selected in Determinator
before Skills, with no repeated racial choice in Abilities. Global Trait roots
had 8/8 and 8/7 categories; feats remained populated. This save-free fixture
canceled at final review (`completed=false`); actual commit and owner UI
acceptance remain pending. Both transactions restored exact original Mods and
UMM bytes, and independent save audits passed with zero load increments.

Current visible alternate-trait inventory (19 candidates, lifecycle qualification
still in progress): Ifrit -- Wildfire Heart, Brazen Flame, Fire in the Blood,
Efreeti Magic, Forge-Hardened, Fire Insight; Oread -- Crystalline Form, Earth
Insight, Granite Skin, Stone in the Blood; Sylph -- Air Insight, Breeze-Kissed,
Like the Wind, Secretive, Storm in the Blood, Thunderous Resilience, Whispering
Wind; Undine -- Acid Breath, Ooze Breath. Treacherous Earth and Nereid Fascination
are registered, hidden, unpublished and inert. No new trait or mechanic was added.


### Native preview visual retention repair (in qualification)

The demonstrated A07 inner-asset destruction is repaired at native
CharGenDollRoom.DollStateUpdated, before its unload plan is constructed. KMG
extends the creator's existing initial-retention collections with only the 28
exact registered proxy IDs and their live inner-asset references. It verifies
ownership against the existing native cache before any mutation, preserves all
native/foreign entries and ordering, and deduplicates shared assets by reference.
Repeated doll updates are idempotent. The hook also applies while the module is
OFF because the registered visual identities remain necessary for legacy loads.
No model, color, mesh, GUID or native unload algorithm changes.

Focused tests cover foreign-entry preservation, shared and equality-colliding
asset identities, repeated callbacks, and all-or-nothing invalid-plan rejection.
The existing read-only native unload probe and four consecutive actual creators
will validate that every registered proxy and skin ramp stays alive and retained.
Source, package and runtime qualification are pending for this checkpoint.


Visual-retention checkpoint: `visual-retention-build-01.log` passed repository
validation, all **1,442** domain/reflection tests, clean Release compilation,
deterministic packaging and strict 135-entry validation. ZIP SHA-256
`23db25807efb466a2227a62e5ce8dc0d72be18dd3523b9f476cfda1a6169579d`;
DLL SHA-256 `a0f6a8976ad4660ecd443b2d92beee26dbe03b6f0ef2d2d8b663ca5199d75d69`;
MVID `b9b8cf78-e82a-4b1f-b328-484ba9404c1f`. Source attestation:
`b83c7cdfb3d2beaedfc176f427b75ac4aac2b805` plus fingerprint
`1f2a3976b55316c75d71a46653b0dc920230e90bd405ad21c395692cb6831a54`.

Profile A `20260907T0707199636019Z-disposable-elemental-character-creation-baseline`
and full-stack Profile F
`20260907T0711441896542Z-disposable-elemental-character-creation-baseline`
both passed all four consecutive full-screen creator selection contracts
(Ifrit, Oread, Undine, Sylph). All four reached native final review with zero
unresolved selections, racial choices confined to Determinator before Skills,
and no per-character acceptance failures. Every snapshot retained all 28 live
registered visual proxies; neither process observed an owned inner-asset unload
or instrumentation failure. Both global Trait selections were observed for all
four full-stack characters. Both transactions restored the exact Mods tree and
UMM bytes; independent save audits passed with zero save-load increments.
These eight creators canceled after final review: real committed disposable
characters, changed choices, respec and owner UI acceptance remain pending.


### P0 D: preserve the observed ZFavoredClass array contract (in qualification)

Controlled B/C/D/E/F creators reached both global Trait roots. The owner's
empty-list failure remains an open acceptance defect, not a proven Helpful
causation. Native selection IL reads AllFeatures for extraction/legality; the
installed ZFavoredClass catalog intentionally has an empty Features array.
Observed lifecycle snapshots showed no later replacement of either Combat array.

Helpful now appends only to the authoritative AllFeatures array through the
existing reversible transaction. The helper accepts no Features writer, and
resolution rejects an uninspected nonempty/null Features contract. Exact-GUID
conflict detection, original ordering, repeat idempotence and exact rollback
remain. Bodyguard OFF leaves both Combat arrays unchanged. The source tests
exercise empty-array identity retention, repeated publication, rollback and
rejection before mutation for changed contracts. No unrelated ZFavoredClass
exceptions or data are changed. Qualification remains pending.


Profile C `20260907T0728150548273Z-disposable-elemental-character-creation-case`
reached Ifrit final review with both global Traits and three exact repeated
compatibility callbacks: Combat Features remained the same empty reference,
AllFeatures remained the same 14-choice reference, and Helpful stayed absent.
The profile restored exact Mods/UMM bytes and the save audit passed. However,
its game log contains repeated KMG visual-retention InvalidOperationExceptions:
an inner asset of Ifrit male body proxy `2b436bad4d4f480db61e5c16bc4f7e50`
was destroyed after the initial retention snapshot. The runner did not yet
monitor every shared inner asset, so its diagnostic PASS is overridden by this
log failure. This candidate is NOT qualified. Narrow instrumentation now checks
all initially live inner assets and observes foreign equipment unloads that
share any of them, to identify the missing lifecycle boundary.


The narrower Profile C probe
`20260907T0738386678192Z-disposable-elemental-character-creation-case` failed
on the exact missing boundary: eleven shared materials/textures from native
Tiefling male body/head/horns donors died after their inner-asset unload calls.
Every observed shared asset was excepted and alive immediately after
UnloadInnerAssetsExceptGiven. Native LoadedResource.Unload then calls
AssetBundle.Unload(true), destroying donor-bundle materials irrespective of the
inner-asset exception set. Preserving proxy IDs alone cannot protect that bundle.
The retention plan now also includes the exact native donor and palette-source
IDs resolved during construction (including actual fallback sources). It
verifies their original live cache references before extending the native ID
collection. It does not retain unrelated loaded equipment or alter the unload
algorithm. Every initial inner asset is now monitored throughout creator runs.

`shared-visual-c-01` never launched: an offline optional-assembly reader held
Mods open while the transaction tried to rename it. After the reader exited,
the complete original Mods manifest, SoundBank and original UMM hash were
verified exact; no backup/quarantine/sentinel had been created. The failed-entry
state was preserved and closed as an explicitly verified no-op restoration.
`shared-visual-c-02` then ran and restored exact Mods/UMM bytes and passed the
independent save audit. Failed runtime evidence is retained above.


Donor-retention validation: `donor-retention-build-01.log` passed repository
validation, all **1,443** domain/reflection tests, clean Release compilation,
deterministic packaging and strict 135-entry validation. The qualified artifact
attests source `79ca6301c12d980f08a626d307625798fd71cf2f` plus fingerprint
`1e1b2282a8ef19a55c0cdf938d144e6080a1839fe37de2181f895fb9d1abfb74`.
ZIP SHA-256 `c74752cab22316ddef0265a662b8ff586658ee641a92baecb4b7cb233855a8da`;
DLL SHA-256 `53b62944750c34923f54f782dc7c669f691fa3691d1e7f2bf0ca7ddbf3ee6d2c`;
MVID `aa7eea2b-9b0a-4fd3-89f2-9ea89339a08d`.

Profile C `20260907T0746464105055Z-disposable-elemental-character-creation-baseline`
and Profile F `20260907T0801288315432Z-disposable-elemental-character-creation-baseline`
each passed all four full-screen creators through final review, with no
per-character acceptance failures, no instrumentation failures and zero owned
or shared equipment unload observations. Every initial inner asset (220) stayed
alive. All 28 proxy IDs plus 29 exact donor/palette IDs were retained in the
native creator's existing collection (57 IDs total). There were no KMG ERROR
headers or visual-retention exceptions in these qualified logs. Profile D
`20260907T0752220713262Z-disposable-elemental-character-creation-case` also passed
Ifrit through final review. C/D/F restored exact Mods and UMM bytes and passed
save audits with zero load increments. These are save-free review/cancel routes;
actual committed characters and respec remain pending.
