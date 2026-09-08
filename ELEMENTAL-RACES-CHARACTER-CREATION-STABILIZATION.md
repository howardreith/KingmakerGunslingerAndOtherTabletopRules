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


P0 D contract-preservation qualification uses the same exact donor-retention
artifact above. Bodyguard OFF (C): three reconciliation callbacks preserve the
original empty Features reference, original 14-choice AllFeatures reference and
zero Helpful entries; all four creators complete their two legal global Trait
choices. Bodyguard ON (D): Combat object `ref-7`, Features `ref-5151` stays empty;
AllFeatures changes `ref-5152`/14 to `ref-9243`/15. Full stack (F): Combat object
`ref-46`, Features `ref-3141` stays empty; AllFeatures changes `ref-3142`/14 to
`ref-10185`/15. All 14 original ordered (reference, GUID) pairs are exact in each
published prefix. The sole final entry is Helpful
`e4b29a7c8d5f4c1796ab03e1f72d8456`. Three repeated callbacks, actual first-level
Trait consumption and runtime-ready snapshots retain those exact resulting
references. Both global Trait roots expose 8 categories (8 legal first, 7 legal
second); D Combat has 15 extracted/13 legal choices. No foreign trait is lost.

These results qualify the narrow foreign-array repair, but do not establish it
as the cause of the owner's empty lists: B and the earlier KMG-enabled native
control routes also completed both Traits. The owner-specific reproduction and
human UI acceptance remain open. An offline field-reader scan found no Features
read in ZFavoredClass or native selection consumption; CotW reads found were in
construction/balance methods. Three unrelated optional methods could not be
inspected because their requested assembly versions were absent from the offline
reflection context; that limitation is recorded, not treated as a runtime error.
The preexisting ZFavoredClass custom-data KeyNotFound warnings remain separate
and unsuppressed.

The first actual-commit request, full-stack
`20260907T0756184646606Z-working-save-elemental-character-creation`, loaded the
exact working save successfully, then failed the fixture's occupied-creator
precondition before creating any unit. The read-only snapshot found no visible
active controller. Native Warmup leaves a global automatic controller after
clearing the visible controller, which is a candidate for this readiness
mismatch; exact dormant-state fields still need runtime correlation. The run
restored exact Mods/UMM bytes and passed the save audit (one load, no content
changes). It is not a character-creation completion PASS. The fixture will be
narrowed to preserve a proven idle native warmup controller without operating
on any existing character or claiming an interactive creator.


### Native creator fixture ownership and allocation (in qualification)

The working-save fixture now records dormant creator fields. It accepts an
existing global controller only when the exact inspected native warmup contract
holds: AutoCommit, Preview equals Unit, no Doll, LevelUp mode, the hidden native
build surface still references that same Unit, no visible controller, and warmup
has ended. It preserves/restores that exact controller reference without
canceling, committing, or operating on its unit. Interactive/ambiguous ownership
still fails closed. A failure before fixture entry no longer incorrectly claims
that an uncaptured world snapshot failed restoration.

The request-local allocator now refunds a negative skill budget through native
CharacterBuildController.SpendSkillPoint(stat,false). It uses only a skill
recorded by this request, requires exactly one matching native action, and
verifies that exactly that action was removed. It never writes skill budgets or
stats directly. This addresses the observed Gunslinger/Roll fixture convergence
failure without altering production Gunslinger, Dice Roller or allocation rules.
Focused source safety checks cover both ownership and refund boundaries.


`20260907T0812592512664Z-working-save-elemental-character-creation` again
stopped before creating a unit and restored exact Mods/UMM bytes; the save audit
passed with one load and no content changes. Its newly captured fields narrow
the mismatch: the native window is hidden, warmup false, visible controller
absent, global AutoCommit true, Preview is the same Unit, Doll absent, actions
empty, and State.Mode=CharGen. The current build surface does not reference that
global Unit. Thus the first proposed warmup signature was too specific; the
runtime facts establish an idle automatic global alias, not the exact Warmup
call provenance. The fixture now preserves only this action-free automatic
contract (CharGen/LevelUp), records its blueprint identity, and still refuses
an interactive, action-bearing, preview-owning, visible or ambiguous controller.
It never operates on the preserved automatic controller or its unit.

### Native committed creators and rolled Gunslinger checkpoint

- `scripts/Build-Local.ps1` (`creator-ownership-build-02.log`) passed repository
  validation, all 1,443 domain/reflection tests, clean exact-reference Release
  compilation, deterministic packaging, and strict standalone package validation.
  Source was `977e0a123bf16758ea4c0b0061c85611f31c8f8a` plus source fingerprint
  `0927ac28ce511f0f6196eed5a4b99962b0899d18da729ddee7a1da43ef914651`.
  ZIP SHA-256 `0ffbc3a2f99c33ccfb0cd77d2ee09b9c22e94f3446a2bb0905897906a204cc5c`;
  DLL SHA-256 `0a5458fba172c069686a103a570ab0dd493f469910059fc4a0690baf00793370`;
  MVID `d74be144-1c1f-4fb5-b2cd-0b199352baa6`.
- Full-stack guarded `working-save-elemental-character-creation` run
  `20260907T0827266891992Z-working-save-elemental-character-creation` PASS 11/11.
  All four independently owned level-0 disposable characters reached native
  final review, `CharacterBuildController.Commit`, its success callback, final
  level 1 and the exact chosen race, with zero unresolved selections. All four
  per-character acceptance results PASS. These were General/retain-base builds;
  alternate-choice and back-navigation completion remain pending.
- Every heritage exposed 3/3 choices in native Determinator/Heritage. The ten
  replacement slots exposed retain plus published choices there (Oread SLA 1/1).
  Both ordinary global Trait roots were later consumed in Abilities with eight
  categories, 8 then 7 legal; the chosen nested Combat and Faith routes completed.
  No racial selection reappeared in generic Abilities. No owned/shared asset
  unload occurred; all 220 retained shared inner assets remained alive.
- The observed initial global controller was automatic, action-free, mode CharGen,
  Preview identical to Unit, Doll absent, with no visible creator/controller.
  Its blueprint was `4391e8b9afbb0cf43aeba700c089f56d`. The fixture preserved and
  restored its exact reference and the independent original build.Unit reference;
  it neither reused nor cancelled/committed that controller or its unit.
- Full-stack `disposable-elemental-character-creation-case` (Ifrit, Gunslinger,
  roll) run `20260907T0833492894852Z-disposable-elemental-character-creation-case`
  reached native final review, both global Trait roots and zero unresolved
  selections; save-free cancel and exact cleanup PASS. Dice Roller 0.1.2 verified
  generation 11 with assigned/base scores `[15,15,12,12,12,13]`, exact preview
  ownership, and no contaminated baseline. This roll needed no skill refund:
  the negative-budget branch is not yet runtime-qualified and will be exercised
  by a native back-navigation heritage change that reduces Intelligence.
- Both profile transactions restored the complete original mod tree/settings and
  UMM bytes exactly. Save audit PASS: baseline and all non-header working data
  exact; working load deltas respectively 1 and 0. No save-writing API was
  reached. Existing ZFavoredClass custom JSON KeyNotFound errors, shader,
  missing-script, lightmap and Buff Planner HUD-overlay warnings remain separate
  from KMG acceptance; no new KMG visual-retention exception occurred.
- Human full-screen UI acceptance remains **NOT-RUN**. This checkpoint is real
  automated creator completion evidence, not owner-reported acceptance and not
  final stabilization qualification.

### Native racial round-trip regression work in progress

A new strict guarded working-save regression request covers three disposable
native creators per named elemental race. Route plans are General -> alternate A
-> General, alternate A -> alternate B, and alternate B -> General -> alternate A.
Every visit must reach native final review; final commits cover all three markers.
Representative plans include Brazen Flame and Secretive multi-slot choices,
independent combinations, Crystalline Form/Breeze-Kissed resources and both
Undine breaths. All replacement slots are first reset through native retain-base
choices before another combination; no facts or stat/skill budgets are written.
The fixture compares exact selected marker/provider/resource/ability graphs and
racial overlays after native choices, unchanged point-buy/rolled allocation
baselines at every review, exact rendered choices and actual native commit
completion. Publication of either deferred exact GUID now fails the actual
choice-list check rather than being filtered out by the fixture. Runtime results
and any failed hypotheses will be appended after the full source qualification.

#### First native round-trip runs and fixture corrections

- `native-revision-build-03.log`: validation, 1,444 domain/reflection tests,
  clean Release, deterministic package and strict package validation PASS.
  The first two build attempts stopped on the newly incremented test-count pins;
  the current validator and static source contracts now consistently specify 1,444.
  Request-preflight PASS 245. Its first run observed artifact-directory metadata
  changes during its no-staging check; repeating without a log inside the observed
  artifact tree passed. No guard was relaxed.
- Artifact source `d05383b3d2d06888f51f47a064c8a070cf66d041` plus fingerprint
  `bc618cd3be4cdf28f8c6aa4044384f6b6ab3fbcf40de751fa876521e2b01d3f5`;
  ZIP `e88bf97a2fc1a4e8466a314b49c7d3e036c12b88f8915e4e7a573184032bf8f9`;
  DLL `ba79a03a4de0375f5fc80b1f61171da66049b43e466cfc044b582628b07b50f8`;
  MVID `44303cd7-9e18-4d4d-b129-eb2b6e833793`.
- Ifrit run `20260907T0856319936696Z-working-save-elemental-character-creation-regression`
  FAIL: the second creator's atomic evidence-file replacement met a Windows
  sharing violation while the external observer was reading the large JSON.
  First creator had completed General -> Lavasoul -> General with exact graphs,
  unchanged point-buy bases and native skill refund -1 -> 0. The run remains FAIL.
  External reads now explicitly permit FILE_SHARE_READ|WRITE|DELETE and close the
  handle before JSON parsing; no game/writer exception is suppressed.
- Same-artifact repeat `20260907T0904331433506Z-working-save-elemental-character-creation-regression`
  PASS: three native Ifrit commits (General, Sunsoul, Lavasoul), eight complete
  final-review visits, exact graphs/baselines, and two native one-point skill
  refunds. Brazen Flame plus Fire in the Blood and the independent Wildfire Heart,
  Fire Insight, Forge-Hardened combination both complete and replace cleanly.
- Oread `20260907T0911127592196Z-working-save-elemental-character-creation-regression`
  PASS: three native commits (General, Ironsoul, Gemsoul), eight complete review
  visits and 48 exact graph observations. Granite Skin, Crystalline Form and
  Earth Insight switch through native retain routes. Crystalline resource count
  is exactly one while present and zero when replaced. All allocation baselines
  remain exact; Oread SLA's retain-only selection completes normally.
- Sylph/Gunslinger/roll run
  `20260907T0918238178513Z-working-save-elemental-character-creation-regression`
  FAIL before allocation: the fixture dereferenced an absent Dice Roller session.
  Dice Roller explicitly rejected a controller-owned non-mercenary candidate
  while Player.MainCharacter resolved to the different existing campaign actor;
  `mercenaryStateEmployee=false`, `mercenaryStableOwnerCustom=false`. This fixture
  had used DefaultPlayerCharacter in a loaded campaign. It is not a reproduced
  elemental mechanic defect. Rolled loaded-game fixtures now use the exact native
  CustomCompanion blueprint, with explicit remote/cross-scene/party/inventory/money
  restoration checks and final committed base-score comparison. No Dice Roller
  production code, eligibility rule, main-character identity or existing actor is
  changed. This correction is not yet runtime-qualified.
- Every listed profile restored the exact original Mods/UMM state; every save
  audit PASS with baseline/non-header working data unchanged and load delta 1.
  Human UI acceptance remains NOT-RUN; no failed run is relabelled PASS.

- Rolled Sylph retry `20260907T0930227612534Z-working-save-elemental-character-creation-regression`
  FAIL: the first real CustomCompanion completed and retained its exact Dice Roller
  assignment through all three native reviews. The second creator reached its
  final review, then native starting-item publication found the first disposed
  fixture's orphaned battered-firearm origin token in party inventory. The probe
  had removed the companion without rolling back its native starting items.
  Cleanup now captures the synchronous Commit's exact item references and stack
  deltas (also on failure), removes only those additions before disposing their
  owner, and checks original ordered references/counts. Production firearm
  ownership and Dice Roller behavior are unchanged. Profile restoration and save
  audit passed; this failed run remains failed.

- Sylph retry `20260907T0942544671888Z-working-save-elemental-character-creation-regression`
  FAIL after two exact rolled commits and seven complete reviews. Item rollback
  restored original references and counts for both commits, including two existing
  ammunition stacks. Third character's final heritage revision failed the combined
  legal/rendered guard; the old evidence did not distinguish those predicates.
  Revision readiness now records each predicate and actual rendered item state;
  legal choices may await bounded native rendering, matching initial selection.
  No missing or illegal choice is tolerated. Cleanup also exposed that native
  AddEntity queues registration for EntityCreator.Tick: synchronous disposal had
  preceded actual cross-scene ownership. The probe now waits for exact native
  registration before retirement and reports each restoration field separately.
  This is fixture timing instrumentation, not a production routing change. Exact
  external mod restoration/save audit passed; failed acceptance is retained.

- Sylph retry `20260907T0953569835351Z-working-save-elemental-character-creation-regression`
  FAIL with exact cross-scene, remote, party and inventory restoration; money alone
  differed. Native registration timing is now proved. The failed racial boundary
  had all three exact heritage choices active, interactable and mechanically legal.
  Their cached FeatureSelectionState objects differed from reconstructed state
  references. Native SelectFeature.GetSelectionState resolves Selection object plus
  Index, rather than state-object reference; FeatureSelectionState equality also
  compares value fields. The probe now requires a unique exact native identity,
  matching source/level, and a legal active choice, then invokes that rendered
  item's actual native Action. It records reference equality separately. No game
  or mod UI state is refreshed or repaired by the probe. Money is now captured
  across the synchronous owned Commit and only that exact delta is reversed;
  changes outside the boundary fail. External restoration/save audit passed.

## Qualified native creator round trips: candidate 08

All four fresh-process owner-stack profiles PASS (12/12 guarded assertions each):

| Race / class / allocation | Run ID | Real commits | Final reviews | Exact graphs | Native skill refunds |
| --- | --- | ---: | ---: | ---: | ---: |
| Sylph / Gunslinger / Dice Roller | `20260907T1010547202824Z-working-save-elemental-character-creation-regression` | 3 | 8 | 50 | 2 |
| Undine / Fighter / Dice Roller | `20260907T1016551190289Z-working-save-elemental-character-creation-regression` | 3 | 8 | 36 | 1 |
| Ifrit / Fighter / point-buy | `20260907T1022352171465Z-working-save-elemental-character-creation-regression` | 3 | 8 | 50 | 2 |
| Oread / Fighter / point-buy | `20260907T1028514852316Z-working-save-elemental-character-creation-regression` | 3 | 8 | 48 | 0 |

The twelve commits cover all twelve existing heritage markers. All 32 final
reviews have a complete native state, zero remaining selections and an enabled
Complete button. All 184 observed racial graphs have exact markers, providers,
ability/resource identities and racial overlays. Each character reaches both
ordinary global Trait roots, their populated Combat/Faith catalogs, and the normal
feat selector. Elemental choices remain in Determinator before the native Skills
phase that owns allocation and skills. All retain/alternate and multi-slot/independent
round trips complete through the real native creator. This checkpoint does not
qualify full Player.RespecCompanion, fresh-process visible-trait persistence or
human UI acceptance.

All rolled reviews retain the exact Dice Roller controller/state/preview, applied
assignment and uncontaminated baseline; committed base values equal the verified
assignment. All point-buy reviews retain their exact allocation. Five native skill
refunds remove exactly one request-owned allocation action and resolve -1 to 0
remaining skill points. No base-stat or budget field is repaired by the probe.

All twelve native commit cleanups restore exact ordered world/cross-scene/remote/
party membership, inventory references and stack counts, controller ownership,
main actor and money. Native mercenaries finish queued cross-scene registration
before retirement. Sylph/Gunslinger commits each changed money 4 -> 415; the exact
synchronous delta was reversed to 4. One Sylph revision used a distinct cached
view-state object with native value equality and exact selection/index/source/level;
its actual rendered Action completed successfully. The probe records this object
reference distinction rather than replacing native UI state.

All four full Mods/UMM transactions restore exactly. Each save audit PASS: protected
baseline exact, personal-save metadata exact, working non-header data exact, working
header unchanged except the native load counter (delta 1). No save write occurred.
All 220 retained visual inner assets remain alive through the creator operations.

Build-Local candidate 08 passed repository validation, all 1,444 domain/reflection
cases, clean Release compilation, deterministic packaging and strict validation.
Candidate 07 failed compilation because the inspected native view Action is private;
candidate 08 obtains that exact field through reflection. No native or foreign
production class was modified for this access.

- Source commit: `d05383b3d2d06888f51f47a064c8a070cf66d041` plus qualified source
  fingerprint `f035aa133e37f0f16b09180a6e4e2b571bcd16c407c544c5adaa9ec49f193551`.
- Numeric version `0.0.117`; informational version still `0.0.117-elemental-traits`
  for this instrumentation checkpoint, not the final acceptance installation.
- Preserved ignored ZIP: `artifacts/qualification/0.0.117/character-creation-stabilization/native-creator-roundtrip-candidate-08.zip`.
- ZIP SHA-256 `84f6abadfc01a19fdc316652a56ed65094e05840e498974975bcc08224dc7ad1`.
- DLL SHA-256 `371bd1411b1e4e1b73567f0423e6e8339e923e40e111d870152547d2e06eb401`;
  MVID `65024647-d522-4beb-8094-862325f3e862`.
- Manifest 1,867 identities (1,865 active, two reserved); package 135 entries.

Curated result/creator evidence hashes (raw artifacts remain uncommitted):

| Race | runtime-result.json SHA-256 | Creator evidence SHA-256 |
| --- | --- | --- |
| Sylph | `a40264bbdc993fad262c434cb94473c426f253bacb55be47b53a320d09d53c9e` | `10b69e3bc5be61eb661c2ab665852b1e55b2b2d93dea61f5b9428f03024dbf6e` |
| Undine | `c2064be9880feb1a994b2777cb0d5fd4f7db3785ff474792cdd2a1f99a454b3a` | `f004c131ae12597481b4bae4a0ba7479b652408499449d0d476035a191032d9c` |
| Ifrit | `640661b883abd7d47efa401485c989bf6ec525d2968bcc4a48286fcc3d5afaa6` | `010fb58572634e7d327a2f865860d180a54b51e9ae91661d4124d2d2cc7969cc` |
| Oread | `0a8e4081797a4f6b5b9c1fee981f5b4b4b1c9c5e118c846b9cb1c1c7b2eb5bf9` | `a6c1e14626a27f7726d906c975c6709862e453a6bca55e296f3f5fbdac5a08ac` |

Logs contain zero KMG ERROR lines. Each preserves the four preexisting
ZFavoredClass custom JSON KeyNotFound exceptions and a native
`Kingmaker.UI.BugReportCanvas.OnEnable` NullReferenceException during startup.
The same exact native BugReportCanvas stack is present in the preserved owner
log from before this mission (SHA-256
`948d59a90bc88669e53a1c9ca4bfe2b700527403a4ed80f55a752047d795d5e8`).
These startup failures are separate from KMG creator acceptance and remain
unsuppressed. Native shader/missing-script/lightmap warnings also remain. No new
KMG mechanical or creator exception was observed in these four qualified runs.

Next: full native Player respec callback qualification, then all currently visible
Release C lifecycle/TB and migration/compatibility gates. Human UI acceptance is
NOT-RUN. The prior installation was restored after every profile; the final
acceptance candidate is not installed yet. No merge, tag or release occurred.


## Full native Player respec: observation work in progress

The next guarded fixture invokes Player.RespecCompanion on a real, committed,
request-owned native mercenary, rather than only committing a temporary Respec
controller. Eight visits cover unchanged General and both directions among all
three heritage/alternate-trait plans. It selects existing Elemental Strike in the
normal feat catalog and deliberately spends active owned resource GUIDs before
respec. Original, replacement and preview references are tracked separately;
preview and post-callback resource mismatches fail without fixture reconciliation.
This source adds instrumentation only. Actual full-respec qualification remains
pending; no production resource fix is justified by the earlier IL inference alone.

- Full-native-respec source candidate 01: repository validation, 1,445 complete
  domain/reflection cases, clean Release, deterministic/strict package PASS.
  Request preflight initially failed its no-staging metadata check when only the
  exact-build/bin and artifacts/packages directory timestamps changed; no compiler
  or game process remained during inspection. An isolated repeat passed all 255
  checks. The guard was not relaxed. Before runtime, cleanup gained an explicit
  prerequisite that pause state must have been captured before it can be restored.
  No native respec runtime result is claimed yet.


### Full native respec reproduction and narrow resource repair, 2026-09-07

The first full-stack native Player.RespecCompanion run,
`20260907T1111253889642Z-working-save-elemental-native-respec`, is **FAIL**.
Candidate DLL SHA-256 `77b897f29f9289a1b4e78169525069785c28475cecde7fa366d4b9b3b51f0d05`,
MVID `ca01f9a2-3df7-4727-99da-1f9def4a0e1e`, ZIP SHA-256
`e63531d706aa0b7c4d55e3eb0b00f1c730570e7f66003578f2c83838e93f8e64`;
source 68ca24d3751fb67d9451781e6e90f8749ce7b7d7 plus the recorded dirty-build fingerprint.
All 1,445 tests, repository validation, clean Release and deterministic/strict package checks passed before this run.

The original General Ifrit was really committed, then its exact SLA resource
`04e5fa42bffd4ab4b305e56dec7ccb0d` was spent from 1 to 0. The next unchanged
General/retain respec reached a complete native final review and one successful
native callback, but every measured preview and the original after copy-back had
amount 1. All compared racial stats, markers, providers and ability/resource
identities/counts were exact. This is a reproduced resource refill, not a
synthetic-state inference. The existing elemental feat remained rank 1.

Inspected native IL shows a fresh respec UnitDescriptor/UnitParts graph, followed
by SetupNewCharacher calling ApplyRest before the success callback copies the
replacement into the original. The focused repair captures only exact owned
present amounts and previously suppressed owned amounts, seeds the correlated
native replacement, preserves those amounts across native preview/action replay,
and removes native setup refills before copy-back and at the original success
boundary. Unseen identities keep their native budget. No GUID, selection, stat,
provider, maximum, foreign resource or ordinary rest implementation changes.
This repair is pending fresh runtime qualification.

The failed run also found a fixture cleanup error: native PrepareRespec nulls
the discarded replacement Body, while both native UnitDescriptor.Dispose and
Call of the Wild's replacement prefix call Body.Dispose unconditionally. The
fixture now retains that exact request-owned empty body for final shell disposal,
after all native callbacks and after retiring the original. It does not patch or
suppress the foreign Dispose method. Recorded world/party/inventory/money/pause
restoration was exact; the external profile restored the exact starting Mods
and UMM bytes. The failed attempt remains FAIL, including its cleanup exception.
Human full-screen acceptance remains NOT-RUN.


### Full native respec qualification checkpoint, 2026-09-07

Candidate 05 passes four fresh-process full eleven-mod-stack profiles. Each
performs one real first-level mercenary creation and seven actual
Player.RespecCompanion callbacks on that original. The visit sequence is
General, General, alternate A, alternate B, General, alternate B, alternate A,
General, including retain/alternate and multi-slot/independent changes.
All reviews have zero unresolved selections and a native enabled Complete
button. The two global Trait roots and existing Elemental Strike remain usable.

| Race | Guarded runtime run | Commits / respec callbacks / racial graphs | runtime-result.json SHA-256 |
| --- | --- | --- | --- |
| Ifrit | `20260907T1147385430995Z-working-save-elemental-native-respec` | 8 / 7 / 64 | `5799387a4d6bb88cbb3387f3559dd8553259cc047c8c554cb63f4dd5c090900a` |
| Oread | `20260907T1159336521752Z-working-save-elemental-native-respec` | 8 / 7 / 64 | `9ad6e65594471b147c031e65a57b90f03af909659d233d1f4e51be4850d4f1f3` |
| Sylph | `20260907T1211368234223Z-working-save-elemental-native-respec` | 8 / 7 / 64 | `57d4d7d3fb0c9e7dd9b508cd07857cb9d1f8cfeb76ef2448306f53f5603ed940` |
| Undine | `20260907T1223437010470Z-working-save-elemental-native-respec` | 8 / 7 / 48 | `49acc40152055bceb23c401a0b7ac05b258191165215fffcc78e9d20deed5936` |

Totals: 32 native commits, 28 native respec callbacks and 240 exact racial
provider/stat/resource graph observations. All 48 final runtime assertions pass.
No preview resource mismatch occurred. Each active owned resource was spent
through native Resources.Spend before respec; removed identities retained their
spent amounts when selected again. For example Sylph Breeze-Kissed gust resource
`e117e1e0a17a4acec001000000000077` was spent 1 -> 0, removed, then returned as
exactly one resource entry at 0. No fixture reconciliation, provider grant,
base-stat repair or remembered-resource write supplied these results.

All four final fixture cleanups restore exact world/cross-scene/remote/party
membership, inventory references and counts, money, controller and pause state.
Every external Mods/UMM transaction restores exactly. Every save audit passes:
protected baseline exact, other-save metadata exact, working non-header exact,
working header exact except its native load counter, delta 1. These runs do not
write a save. Each log has zero KMG ERROR lines, the four known ZFavoredClass
custom JSON KeyNotFound errors, and the already captured native BugReportCanvas
startup exception. Those third-party/native startup errors remain unsuppressed.

Qualification commands: `scripts/Build-Local.ps1` (repository validation, complete
1,447-case domain/reflection suite, clean exact-reference Release compilation,
deterministic package creation and strict package validation),
`scripts/Test-RuntimeScenarioPreflight.ps1` (255 PASS), and guarded
`scripts/Invoke-KingmakerRuntimeTest.ps1 -Scenario working-save-elemental-native-respec`
with ExpectedVersion 0.0.117, explicit KMG_AUTOMATION_WORKING, race, Fighter,
point-buy, TimeoutSeconds 1200, ExitAfterCompletion true, inside each reversible
full-stack transaction. The source and artifact are unchanged across these runs.

- Source commit `68ca24d3751fb67d9451781e6e90f8749ce7b7d7` plus source fingerprint
  `a6030aebf276e7cc75b63550b63213827755660b0f578cc6b1ca60cfa8677340`.
- Numeric version 0.0.117; informational version 0.0.117-elemental-traits.
- Preserved ignored package:
  `artifacts/qualification/0.0.117/character-creation-stabilization/native-respec-candidate-05.zip`.
- ZIP SHA-256 `d698f9fcaea98fe54e4c575c225cd2bd989990e433e1532ab85e47ec130c23b8`.
- DLL SHA-256 `2a181f769aa36574b88ab44e95fc9cb9b7ba08811705273e0d5727cab519b589`;
  MVID `4a7dd798-0f1b-4364-93bb-d54cf5b5a9de`.
- Manifest 1,867 identities (1,865 active, two reserved); package 135 entries.

Earlier candidate-05 attempt
`20260907T1131315408084Z-working-save-elemental-native-respec` remains a guarded
**FAIL**: the host's 600-second deadline elapsed before the fixture finished.
The runtime later completed all assertions in 651.253 seconds and the game
exited automatically, but that late internal PASS does not override the host
failure. After positively verifying process exit, the exact Mods transaction
and saved UMM bytes were restored and all save audits passed. The original UMM
timestamp was not durably captured by that older helper; only its exact bytes
are claimed restored for this recovery. The profile helper now durably records
its initial UMM timestamp and uses 1,200 seconds for this eight-visit scenario.
The later four runs above all finish within the guarded deadline and restore
normally. No forced game exit, personal save edit or UI input was used.

Build candidate 03 exposed an accidental local text-decoding change to one
preexisting em dash in the test runner; the exact original UTF-8 text was
restored. Candidate 04 passed all tests but failed compilation because the
native UnitBody collection is Items, not EquipmentItems; candidate 05 uses the
inspected native property. Neither failed build was installed for qualification.

This checkpoint qualifies owned AbilityResource preservation through native
respec. Blood-healing UnitPart expenditure, ordinary rest after respec, native
turn-based action costs and the expanded visible-trait persistence matrix are
still pending. Human full-screen acceptance is NOT-RUN. The prior installation
is restored; the final acceptance candidate is not installed. Nothing was merged,
tagged or released.


### Native respec blood-capacity observation, pending

The next source candidate extends the existing guarded eight-visit native respec
fixture to spend the three existing blood-healing counters through real damage
and native buff healing. Its request-local Oread/Sylph alternate-B combinations
use Stone/Storm in the Blood. Creator regression plans and production content
remain unchanged. Every native preview and original after callback is compared
against independently observed expenditure, including while providers are absent.
An ordinary rest after the final successful callback must reset the counters and
restore the currently active owned daily resources. No blood production repair
has been made; a runtime failure is required before diagnosing that lifecycle.


### Native blood-capacity respec reproduction and repair, 2026-09-07

Full-stack run `20260907T1246020293930Z-working-save-elemental-native-respec`
is FAIL. The first three real commits passed. Before visit 3, native matching
Fire damage armed the existing buff; its real tick healed two wounds, changed
Fire in the Blood Spent 0 -> 2, left Remaining 0, and removed the exhausted buff.
Every subsequent preview and the original after the real native callback showed
Spent 0 instead of 2, including with the provider absent. All recorded racial
stats, facts, provider/resource/ability identities and counts still matched.

The existing exact-respec bridge now snapshots the three known blood counters
when their original UnitPart exists, and preserves maximum(current,captured)
at the already qualified replacement/preview/setup/copy-back boundaries. It
never clamps expenditure down to a replacement's lower level. Existing UnitPart,
JSON schema/fields, rest handler and all GUIDs remain unchanged. No general rest
patch or foreign state mutation is added. Fresh-process qualification is pending.

Failed candidate: source baca3de8d4be464c03617f73d5cf7b690bf9d5f1 plus fingerprint
b3936239eb4a02588a4e05f590f831891e6f8b99d95eee329f11f172f476f3e8;
ZIP c0adb2728dd6447a43f5d90dbeb0ecea1e9f523d58a71d787e0a7727de94620b;
DLL 8b8312bcf4b1848a3a7c637461ec1ef2af83a5f619193b72c12159dfebdadf07;
MVID 8f813664-0384-4b8a-b925-7c0f6422fdac. All 1,447 domain/reflection cases,
repository validation, clean Release and deterministic/strict packaging passed
before this reproduction. Runtime-result SHA-256
330c247a88d389a220abbbf5c60f42d0f69a591bef3919ca0bad915ddd01cbe7;
creator-evidence SHA-256
786389d53b7b4cdfc3704b259aced8257470ee1694100c9094d08e5af046ca34.
The combined creator completion/cleanup assertion is FAIL because the planned
route did not complete. Its recorded membership/inventory/money/pause cleanup
fields are all exact, and external Mods/UMM restoration and all save audits PASS.
The failed run is preserved without overriding its outcome.


### Native blood-capacity respec qualification checkpoint, 2026-09-07

The focused repair passes all three fresh-process full-stack runs below. Each
performs eight real native commits and seven Player.RespecCompanion callbacks,
with 64 racial graph and blood-expenditure observations. Totals: 24 commits,
21 native callbacks, 192 exact graph observations, 192 exact blood observations,
zero resource or blood mismatches, and all 36 final runtime assertions PASS.

| Race | Guarded runtime run | runtime-result.json SHA-256 |
| --- | --- | --- |
| Ifrit | `20260907T1258375682479Z-working-save-elemental-native-respec` | `26e48e8c78f9569194948b383d0c5ba81e140b7b76bf4f22a54bbbece46aefc4` |
| Oread | `20260907T1310434810059Z-working-save-elemental-native-respec` | `f4f6cdf881905e254bd7c31fa348fbbffcdd0425c93c10ef4e0a121330946573` |
| Sylph | `20260907T1322554158268Z-working-save-elemental-native-respec` | `c6c8588b3cabe415261d7eedebe3a1d9e61a912286c8eddd7e8e4fae112d4432` |

Each corresponding Fire/Stone/Storm in the Blood provider was spent 0 -> 2 by
real matching damage followed by the native buff healing tick. It retained
Spent 2 through removal, preview reconstruction, reselection and final native
copy-back. The fixture never writes remembered blood expenditure. After the
last successful respec, native ordinary rest resets the observed counter to 0
and restores the currently present racial SLA resource to 1. Existing daily
AbilityResource preservation remains exact throughout all visits.

All final fixture cleanup fields, external Mods/UMM restoration, and save audits
pass. The protected baseline is unchanged; the working save differs only by its
native load counter, delta 1; other save metadata is unchanged. Each run records
zero KMG ERROR lines and the same four ZFavoredClass custom JSON KeyNotFound
errors and previously captured native BugReportCanvas startup exception. These
unrelated warnings remain unsuppressed.

Qualification used scripts/Build-Local.ps1: repository validation, the complete
1,448-case domain/reflection suite, clean exact-reference Release compilation,
deterministic package creation and strict 135-entry validation all PASS. Guarded
working-save-elemental-native-respec requests used Steam 640820, expected version
0.0.117, explicit KMG_AUTOMATION_WORKING, Fighter, point-buy, 1,200-second deadline,
and automatic exit inside independently restored full-stack transactions.

Candidate 03 identity:

- Source baca3de8d4be464c03617f73d5cf7b690bf9d5f1 plus source fingerprint
  91c6780e9164109feeb82ea796bf80f2be7b570d327b7a846abc7d654e01b05d.
- Informational version 0.0.117-elemental-traits; numeric version 0.0.117.
- Preserved ignored ZIP: artifacts/qualification/0.0.117/character-creation-stabilization/native-blood-respec-candidate-03.zip.
- ZIP SHA-256 3e5a33a1300a2327b986ce9ca5a373258cd0d8d3026cbcd1e5ee96548b1dca86.
- DLL SHA-256 575a41a0cf88779f8a40c4ab52614feb3c04734974a6e73dffc5ba000099336b.
- DLL MVID 9e2e063e-d7a5-45cf-b404-a6e7319adefa.
- Manifest 1,867 identities: 1,865 active and two reserved; 135 package entries.

Candidate 02 failed local validation because a new test registration used Run
instead of the existing Case initializer. It was corrected before candidate 03;
no failed-build artifact was installed. The earlier reproduced runtime failure
remains FAIL and is not superseded in the historical record.

Blood expenditure and ordinary rest after native respec are now qualified for
these three counters. Native turn-based action costs, expanded visible-trait
fresh-process lifecycle coverage and final compatibility/migration remain open.
Human full-screen acceptance is NOT-RUN. The prior installation is restored;
the final acceptance candidate is not yet installed. Nothing was merged, tagged
or released.


### Native turn-cost instrumentation, pending

The separate guarded disposable-elemental-trait-turn-costs scenario now tests
Breeze-Kissed Calm/Renew Swift costs, both Gust Standard costs, and Acid/Ooze
Breath Standard costs in the actual native turn-based controller. It covers all
three heritages per race, ordinary queued cancellation, blocked same-turn Swift
reuse, execution completion, daily resources and exact cleanup. No production
mechanic changes are included. Earlier real-time casts remain distinct evidence;
this new scenario is pending build and guarded runtime qualification.


Native-turn candidate 02 run 20260907T1355390881259Z-disposable-elemental-trait-turn-costs
is FAIL before any command: native CombatController.Clear invokes
UpdateNavigationGridTags, which dereferences the absent save-free Astar graph.
Inspected native IL shows only FlushGraphUpdates and GridGraph.ErodeWalkableArea.
The fixture now uses the existing guarded summoning precedent: skip only that
method for the synchronous lifetime of these exact two disposable actors.
Normal play always executes the native method. Native turns, enrollment,
commands, cooldowns and resources remain authoritative and unmodified.
Fixture cleanup and external Mods/UMM restoration were exact on the failed run;
no save was loaded or written. This is a fixture initialization failure, not an
ability-cost result. Candidate 02 source fingerprint 118153469eba799991be52fea82044600a933c0dee1305b59d40c53ee22bba18;
ZIP 11adbfbabb36417d7b7473e1def26a91b0737dae4ee4f3ac8ed3d1c3f4457a10;
DLL a2486b5e977ee534f1180c8dc55e0c1670c28a4d41268b88a747441ec4835043;
MVID 12ca3493-93c5-4eff-9c41-39b71ddac1f3. Full 1,449 tests and build/package checks
passed. Preflight passes 255 checks when run without concurrent artifact writes;
two redirected attempts retained artifact-tree fingerprint failures. The first
ran alongside packaging and the second wrote its own output inside that tree.
No preflight production check was weakened or removed.


Candidate 04 run 20260907T1405490013369Z-disposable-elemental-trait-turn-costs
remains FAIL before an ability command. Native StartTurn offset 0x142 calls
AstarPath.active.UpdateGraphs for another actor's obstacle bounds. This is
another absent-grid fixture boundary, not a UI exception. The existing summoning
fixture sets AvoidanceDisabled on each exact disposable actor; the turn fixture
now follows that precedent and restores both previous values before disposal.
Native cooldown/buff TickOnUnit calls also follow that established fixture.
Both failed runs restore all external folders/settings and save audits exactly.
Build 03 failed compilation from an incorrect Harmony namespace; build 04 uses
the repository's Harmony12 reference and passed all 1,449 tests and package gates.
Candidate 04 fingerprint a84a0b86c720bed100078de5f880aa5e73ce1f3d278aa908a0bada1d1676ff54;
ZIP 8ed63113f311bb0616cc2feb98dae43f45b64102a457cfd3fd0a65653a43b717;
DLL 8780d6159cdba6aae1d15d385b80b326a938fcc8246d0c19d4fda4ba81da9210;
MVID 6c398fb1-53f5-41a8-9e0e-7e9843d4824f. Qualification remains pending.


Candidate 05 run 20260907T1412550777926Z-disposable-elemental-trait-turn-costs
remains FAIL: the native caster turn and cancellation passed, but Calm did not
start. Native TickCommandTurnBased requires Acting/Ending. Native TurnController.Tick
returns before advancing Preparing when both selection services are absent.
The fixture now creates and destroys a native SelectionManager component during
its exact synchronous lifetime and ticks the native turn after queueing. It does
not assign TurnStatus, bypass native command gates, or manually apply cooldowns.
Native Awake/SelectUnit and the empty service boundary were inspected before the
change. The previous exact static instance is restored and asserted on cleanup.
Candidate 05 source fingerprint afa19e623cde0733ae1518c2ea911f29e70b9314f56dc3948785d0f6935e362d;
ZIP 3ca7fcc515eb8ea9879d763beb07bf5571a0044ef80916662e49d82ed9106173;
DLL 7787704ecca7d380506b9df662b55431411d9cf60fcbfbe5525efee9eab88894;
MVID 82e17e16-2422-4a44-b92a-bd9e1456ee75. All 1,449 tests/build/package gates
passed; external restoration and save audits were exact. No ability-cost PASS
is claimed from that failed attempt.


Native-turn candidate 07 run 20260907T1426043590480Z-disposable-elemental-trait-turn-costs
is FAIL at bounded caster-turn readiness; candidate 08 run
20260907T1435309557520Z-disposable-elemental-trait-turn-costs reaches a real Acting
caster turn with valid target, combat readiness, no UI wait and zero cooldown,
but Calm has not started. Both preserve exact cleanup and external restoration.
Candidate 08 adds retained constructor/turn traces, including failures before a
scope constructor returns. Native CanStart for these non-Kineticist abilities
unconditionally returns true; inspected ShouldStartCommand also requires an
unscheduled, nonbusy native hand-equipment state. The next instrumentation ticks
the native hand controller and records each remaining gate instead of bypassing
startup. No mechanical cost PASS is inferred from either failed run.
Build 06 exposed two private native methods; build 07 uses their exact inspected
reflection signatures. Builds 07/08 each pass all 1,449 tests and strict packaging.
Their exact artifact identities and raw evidence remain in ignored candidate
ledgers; final curated qualification will retain failed and passing outcomes.

### Native turn fixture: ordinary combat entry (candidate 10)

Candidate 09 remained FAIL in guarded A run `20260907T1446065390171Z-disposable-elemental-trait-turn-costs`: no ability executed. The next enemy retained six seconds of initiative while native combat time advanced. All fixture, mod-tree, UMM and save audits restored exactly. Native 2.1.7b IL identifies the main-menu zero clock / zero LastSurpriseActionTime opening-surprise inference; ChooseNextUnit could select the final skipped, surprised actor when neither actor had an opening offensive command. This is a fixture prerequisite failure, not a qualified action-cost result.

The narrowed fixture now enrolls the two owned actors in combat before invoking the native turn-based setting activation, whose Enable callback calls Reset(true, false). It requires native round 1 and neither actor surprised. Initiative, turn state and all ability action/resource costs remain native; no cooldown assignment or command-gate bypass is introduced. The prior candidate hash identities remain in the ignored build and profile ledgers.

Candidate 10 / A run `20260907T1506514500088Z-disposable-elemental-trait-turn-costs` reached native round 1 and Calm Winds: normal queued command started/acted successfully, costs `[0,0,0] -> [0,0,6]`, exact Calm buff present, completed execution and daily resource still 1. Overall FAIL remains: the second swift command was rejected before start with no execution process, but the old assertion mistook its terminal IsActed flag for execution. Two native SoundState.UnitIsCloseToCamera exceptions arose from raising a new-party-combat notification in the camera-less menu fixture. Candidate 11 restores the prior fixture's explicit native Player.UpdateIsInCombat-before-join-tick enrollment ordering and asserts rejection using command start/process, unchanged Calm fact, daily resource and cooldowns. No sound exception is suppressed. It also guards and exactly retires any borrowed empty hand-update queue. Mod/UMM/save restoration remained exact.

### Native turn-based action costs qualified (candidate 11)

`scripts/Build-Local.ps1` passed repository validation, all **1,449** domain/reflection tests, clean Release compilation, deterministic packaging, exact-reference icon checks and strict standalone validation. The standalone runtime-scenario preflight passed **255** checks; it ran alone without writing its output into the fingerprinted artifact tree. No preflight or runtime acceptance condition was disabled.

Qualified source parent `5408cea6e6f38aec1dd642af869f5f794d783682`, exact pre-curation source fingerprint `b827a04ae432216d6e1990776b9b605f3a0e54ac54b3fa7ac8598808fc4bdb32`; numeric version `0.0.117`, informational `0.0.117-elemental-traits`; 1,867 manifest entries (1,865 active + 2 reserved), 135 package entries. Temporary candidate ZIP SHA-256 `9bbea04ddd89fc7016023139859d6cf75df6682686391ecb838219a28826e80d`; DLL SHA-256 `d8aee780a0c0efcc0aa257ccca79e37daa19cbf3cd096f8dea70012f80d0fd0f`; MVID `5d2e233b-4980-45fd-bdbc-8fc5635d97a0`. This is an instrumentation checkpoint, not the final installed acceptance candidate.

| Profile | Guarded Steam run | Assertions | Native accepted commands | Same-turn swift rejection |
| --- | --- | ---: | ---: | ---: |
| A | `20260907T1514233615749Z-disposable-elemental-trait-turn-costs` | 103 PASS | 24 | 6 |
| F | `20260907T1517582352085Z-disposable-elemental-trait-turn-costs` | 103 PASS | 24 | 6 |

Each profile covered all three Sylph heritages with both gust variants and all three Undine heritages with both breaths: twelve owned two-actor combat fixtures. Calm and Renew each charged exactly +6 on native SwiftAction only. Bull Rush gust, Trip gust, Acid Breath and Ooze Breath each charged exactly +6 on StandardAction only. Queued cancellation consumed nothing; second swift commands had no start or execution process and could not remove Calm or spend a resource. Native turn end and initiative progression restored the next turn, permitting Renew plus a standard-action gust. Completed execution and repeated command ticks preserved exact once-only resource use: gust/breath daily 1 -> 0; Calm/Renew left it at 1.

All 24 native commands per profile traversed ordinary UnitCommands.Run / UnitActionController and the native execution process; no cutscene, ignore-cooldown, direct cost assignment, direct resource spending, forced turn status or command-start bypass. Explicit test boundaries: native turn mode was toggled on after owned combat enrollment; only missing save-free navigation-grid flush/erosion was skipped within the exact two-actor scope, and owned animation/projectile completion cues were supplied. These are mechanical turn-cost tests, not human UI acceptance.

Both profiles restored exact world, combat, selection service, hands controller/queue, camera, clock, random state, turn setting, original mod tree and UMM bytes/timestamp. Baseline archive, working-save non-header content and other-save metadata remained exact; working load delta 0. Zero KMG ERROR lines and zero fixture native exceptions/errors. Full F retained the four preexisting ZFavoredClass custom-data KeyNotFound exceptions; both logs retained only the captured native BugReportCanvas.OnEnable startup NullReferenceException at offset 0x17.

Evidence A: `runtime-result.json` SHA-256 `fed32e245e29944611d3a7ca102bd391819750d9895392b63ad9391bf2ccb60f`; `elemental-trait-native-turn-costs.json` SHA-256 `2ed381b822e263452e1a6cf8cfbdfe4bc2bb9ef7a1589c86629d790d8773cc2d`.

Evidence F: `runtime-result.json` SHA-256 `41d93cd317b382397a90947a2d9591c501f0bc1663a67c80cd4cb6cc39e41ae6`; `elemental-trait-native-turn-costs.json` SHA-256 `528b9b6a5034ed9ecb699dfef0e5d2797ba4781871a7e68beaec9ae6e59f9c17`.

Remaining: all nineteen visible-trait fresh-process persistence, semantic ray catalog and ranged-attack boundary completion, physical lifecycle, legacy migration reruns, final compatibility, final development identity/build/install and owner handoff. Human full-screen UI acceptance remains NOT-RUN.

### Visible-trait persistence matrix v5 (qualification pending)

The guarded persistence fixture now targets all nineteen published traits across the same 24 fixed race/sex/heritage identities. Historical v1-v4 policy functions and evidence remain unchanged. V5 has seventeen combined replacement rows, seven blood fixtures, two Breeze-Kissed fixtures, and ten passive SLA-replacement rows. Three pure tests require complete published coverage, legal provider reconstruction independent of marker order, unchanged heritage modifiers, explicit passive SLA absence and independent fail-closed fixture requests.

The existing fresh-process preparation/OFF/ON/cleanup suite retains its real selection, native ability, save, level-up, ordinary-rest, respec and fresh-absence checks. Its evidence now explicitly separates successful SLA spending from intentional SLA absence; absent casting is never recorded as a successful cast. Live checks include provider-owned passive modifiers, Forge-Hardened/Secretive saving-throw contexts, Thunderous Resilience sonic damage, Brazen Flame native unarmed damage packets, Breeze spent-use/voluntary-Calm persistence and exact inactive cleanup. The trait-specific observation occurs before the existing reconciliation idempotence probe. Blood setup uses actual matching damage followed by an owned one-point wound and native buff ticking; replaced resistance is accounted for. Breeze's spent state is seeded through the native resource API for save reconstruction, independently of the already qualified native turn-cost scenario. No production trait, GUID, provider or mechanic is added or changed by this matrix extension.

These expanded checks remain NOT-RUN until their guarded fresh-process sequence completes. The older synchronous persistence respec fixture remains distinct from the already qualified actual Player.RespecCompanion callback suite.


Candidate 04 passed all 1,452 domain/reflection tests, repository validation,
clean Release build and deterministic/strict package checks. The first v5 full-stack
transaction (`visible-trait-persistence-f-01`) passed initial fresh absence (8),
preparation (66), and fresh module-OFF load/level/rest/respend/save (173 assertions).
Module-ON run `20260907T1544500346461Z-elemental-race-module-restored-persistence`
remains FAIL at the new pre-respec conditional-mechanics pause guard. The existing
runner acquired a load-completion pause only for module-OFF. The instrumentation
now acquires that same native, exactly restored pause for module-ON, protecting
saved transient states throughout loaded observation and respec. No mechanic or
acceptance check was disabled. The failure cleanup saved the exact three-character
baseline; independent fresh absence is still required. Original mod-tree and UMM
restoration passed. Candidate 04: source parent 90994196f37d45689a5f251be9040189da33e02b,
fingerprint ad23a759ae975d843383ef301cb90158ebce2108ac9537b4b4e5bded6d65c66d,
ZIP 79836ba610213e873590ce550914071df6c206ea38d0e87f173068b8ca6ef1a4,
DLL baa818057bd47a36c8176de0bff1057a67db094cd4204f3a4eb5f317942e9790,
MVID d904e9a8-6cab-4d45-b62e-dc3ee1c760bd. Earlier build-only failures were strict
current test-count metadata and two source-guard locations moved into the shared
SLA snapshot helper; their original safety conditions remain asserted.


### All nineteen visible traits: fresh-process persistence qualified (candidate 05)

The full eleven-mod Profile F transaction `visible-trait-persistence-f-02` passed
all five guarded Steam launches and **340 assertions**. Every source trait remains
registered and nineteen remain published. No new trait, mechanic, provider, save
identity or production GUID was introduced by this coverage change.

| Phase | Guarded run | Assertions |
| --- | --- | ---: |
| elemental-race-persistence-verify-absent | `20260907T1602074073761Z-elemental-race-persistence-verify-absent` | 8 PASS |
| elemental-race-persistence-prepare | `20260907T1604361516786Z-elemental-race-persistence-prepare` | 66 PASS |
| elemental-race-module-disabled-persistence | `20260907T1608297702214Z-elemental-race-module-disabled-persistence` | 173 PASS |
| elemental-race-module-restored-persistence | `20260907T1611593595463Z-elemental-race-module-restored-persistence` | 85 PASS |
| elemental-race-persistence-verify-absent | `20260907T1616049663690Z-elemental-race-persistence-verify-absent` | 8 PASS |

The same 24 fixed race/sex/heritage fixtures cover all nineteen traits, seventeen
combined replacement rows, seven partially spent blood ledgers, spent and
voluntarily calmed Breeze states, and ten passive SLA replacements with explicitly
absent abilities/resources. There are 168 exact trait observations: 24 immediately
before preparation save, 96 across OFF load/level-before-rest/rest/respend, and 48
across ON source observation/respec cleanup. All eleven Release B feats and their
25 persistence identities remain included. Both preparation and restoration
performed 24 native fixed-race shell respec commits, with 72 native heritage/slot
selection records in total. These remain distinct from the earlier actual
Player.RespecCompanion copyback qualification.

Source observations run before the reconciler idempotence probe. Native evidence
includes the exact provider-owned Initiative/AC/Speed/Stealth modifiers; conditional
Forge-Hardened and Secretive saves with no leaked modifiers; Thunderous Resilience
sonic damage; Brazen Flame actual unarmed attacks with once-only fire packets;
Crystalline consent/use state; and Breeze ability/resource/Calm identities. Loaded
resources remain spent through level-up before ordinary rest. Rest replenishes
only according to the existing contract, followed by exact re-expenditure and a
new save. Respec removes the old trait/provider/mechanic graph. The final fresh
process proves all fixture facts, units, owned equipment and saved breath effects
absent and the original three-character working-party shape restored.

Candidate 05 passed `scripts/Build-Local.ps1`: repository validation, **1,452**
domain/reflection tests, clean Release compilation, deterministic packaging and
strict standalone package validation. Source parent
`90994196f37d45689a5f251be9040189da33e02b`; pre-curation source fingerprint
`ec9bc8fecf55400fbfc4f3e2640a53b83eccaad5148bb8ea440d76d512db50c9`.
Numeric `0.0.117`, informational `0.0.117-elemental-traits`; 1,867 manifest entries
(1,865 active + 2 reserved), 135 package entries. ZIP SHA-256
`e61a01099bae39b37395509b4d2e623b2d4a19ab5854afa7b1943c3e57d61670`;
DLL SHA-256 `e7b7a9f103711bf4d08c2ae31f9bbb373f370a96b8bcb999efc32c1ad638b096`;
MVID `52b8b41e-4c39-4e65-ac95-0cadddd16e14`. The package and complete machine-local
ledgers remain ignored. This is a temporary qualification candidate, not the final
owner acceptance installation.

The transaction restored exact starting Mods contents and UMM bytes/timestamp.
UMM SHA-256 before/after:
`516869f3cb0822d11dfe4aa84431620e0eaf59af56cd8489ea537c818af7257f`.
Protected baseline archive and every other save's metadata remained exact. Only
KMG_AUTOMATION_WORKING writes were authorized. Its final archive naturally differs
after native saves; functional cleanup is proven by the final guarded fresh load,
not by treating archive differences as evidence of cleanliness. Settings hashes
before/after each launch are retained in the per-phase ledger, including native
JSON formatting normalization and the intentional temporary OFF state.

Zero KMG ERROR lines and no new exception signature. Every process retained the
four preexisting ZFavoredClass custom-data KeyNotFound reports and native
BugReportCanvas.OnEnable startup exception. Preparation and ON also reported a
native exit-time ObstaclesHelper.TryConnectUnits [0x00051] -> RemoveFromGroup
[0x000a0] -> UnitMovementAgent.OnDisable [0x0000c] exception after scenario.complete.
This exact stack predates the mission: run
`20260906T2202268447372Z-elemental-race-module-restored-persistence`, source
`cf2426ac092b6bed33ff721fca722be9486f5e89` (verified ancestor of mission start),
retained log SHA-256 `4cd4cd9ba730f2513a55b572faea1021c032bfe4df90d6b9800691982e57cd21`.
The same stack also occurs in the earlier 20260906T1840506762664Z run. It is
recorded separately and neither suppressed nor attributed to this stabilization.

| Phase evidence | Persistence index SHA-256 |
| --- | --- |
| `20260907T1602074073761Z-elemental-race-persistence-verify-absent` | `4474514fdce7cb5dba6a7b53943a86a8b7b8091c9e30285edf2dec08880896bb` |
| `20260907T1604361516786Z-elemental-race-persistence-prepare` | `a5c63690b27e570a420c2d1009ee04130c5786017d2f55271797b6a1a0ca2c3a` |
| `20260907T1608297702214Z-elemental-race-module-disabled-persistence` | `ae5115945bb41ded052bf10bf347fb51783fcbedbd17ee70589e9b1072e87b98` |
| `20260907T1611593595463Z-elemental-race-module-restored-persistence` | `6ef3c48bd351fc8660dd7c6dd45d92ff88b96a7491e279941360b8bbc8654b72` |
| `20260907T1616049663690Z-elemental-race-persistence-verify-absent` | `22a1a6865fd6fca4f1d1a1e5d69e66bdbf4edae07006aa2eb4e0330be67473a8` |

Remaining qualification: Crystalline's optional semantic ray catalog, Breeze's
ability-sourced/nonphysical ranged-attack boundary, physical transitions,
pinned 0.0.114 migration rerun, final compatibility and development-version
build/install. Human full-screen character-creator acceptance remains NOT-RUN.


### Crystalline semantic catalog continuation (qualification pending)

The preserved combined live projectile audit identifies 56 additional exact
optional ray/spell-copy GUIDs whose descriptions explicitly describe rays and
whose live delivery is a simple attack-roll Ray weapon projectile. The production
catalog now preserves all original 34 native/project GUIDs and adds those 56
optional identities without loading an optional assembly or creating a blueprint.
Names and descriptions are review evidence, never runtime eligibility predicates.
The inherited exact source/parent, projectile geometry, attack-roll, weapon and
Hand of the Apprentice gates remain. Every present optional identity receives the
same live native attack/AC comparison as the original catalog; absent optional
mods are recorded explicitly. Full-stack presence will be checked against all 56
reviewed identities. Existing non-ray controls and opaque story/beam effects
remain excluded rather than guessed.

The native read-only audit also records ranged weapon damage metadata and actual
ability weapon-attack/Vital Strike components for the remaining Breeze boundary
qualification. It never casts an ability, changes a blueprint, or substitutes
names for exact production identities. No Breeze production change has been made.


Candidate 01 passed all 1,452 tests/build/package gates and KMG-only run
`20260907T1629590649279Z-observe-elemental-alternate-trait-framework`, with exact
profile/save restoration. Review of the abbreviated native story entries also
establishes three explicit game ray identities: Artifact_StarGauntletRay / Laser
Beam, NyrissaRay / Ray of Annihilation, and RaySpellLanternKingStar / Immolation
Curse. Their exact GUIDs, simple projectile attack-roll delivery and native Ray
weapon contract are captured in the preserved audit. They are now appended after
the original 34, bringing the proposed catalog to 37 native/project + 56 optional
identities. This is fixed-identity curation, not a runtime name heuristic or a new
story mechanic. The unlabelled Prismatic Surge and described blast/orb/arrow
controls remain excluded. The expanded 93-identity candidate requires new runtime
qualification before commit.


### Semantic ray catalog qualified (candidate 02)

All **1,452** domain/reflection tests, repository validation, clean Release build,
deterministic package creation and strict standalone validation passed. The exact
same candidate passed the complete alternate-trait framework/mechanics scenario
in isolated A and full eleven-mod F, with no failed assertion.

| Profile | Guarded run | Assertions | Native ray/non-ray AC pairs | Optional rays present |
| --- | --- | ---: | ---: | ---: |
| A | `20260907T1644230237202Z-observe-elemental-alternate-trait-framework` | 6211 PASS | 126 | 0 |
| F | `20260907T1637150575830Z-observe-elemental-alternate-trait-framework` | 6379 PASS | 294 | 56 |

All three Oread heritages exercised every present catalog identity through native
RuleAttackRoll and nested touch AC with exact modifier cleanup. F contains all 56
reviewed optional GUIDs; A explicitly records their absence. The catalog is now
37 native/project + 56 optional identities. The original 34 remain in their prior
order; three native story rays are appended. No optional assembly reference or
new blueprint identity is created. Existing parent recognition, projectile/roll/
weapon boundaries, once-only deflection, awareness, free hands, opt-in, native
suspend/reactivate, ordinary rest and transient cleanup checks remain passing.
Multiple native rays spend only one deflection use; a deflected non-damage ray
does not apply its effect. Shared projectile art does not classify a blast, orb,
arrow, cone or area as a ray. The foreign rewrite of Shadow Elemental Assessor
that lacks native delivery remains an explicit negative transport control; its
removed native spell is never reconstructed.

Source parent `31cdc018850c64c50aba691eecc540cf33162d51`; pre-curation fingerprint
`5c11ef34bd355440d4cfb7a7b8353b7c97f4618eb5b49111274ccd14f57d4605`.
ZIP SHA-256 `dad996a3cd84f92e5902b0a19022f5ae2e2dd664c1df7c030de0a8f9b4ea235c`;
DLL SHA-256 `0da5d87b7e5c57ebeae9ee02d0bca97e2fb2824ef9e3a82b634403cbc3f52229`;
MVID `ccd1f582-de6a-4c5c-9701-86fe6131fc31`. Temporary informational version remains
`0.0.117-elemental-traits`; manifest/package counts remain 1,867 / 135. Final
acceptance version and installation are pending.

Both profiles restored exact starting Mods and UMM bytes/timestamp. Protected
baseline, working-save content and other-save metadata remained exact; working
load delta 0. Zero KMG ERROR lines. Both logs contain one known native
BugReportCanvas startup exception and eighteen native missing-shader
ArgumentNullException reports from _2dxFX_Pixel8bitsBW.OnEnable [0x00076]. Those
same eighteen reports occur in pre-mission full-framework run
`20260906T1755326591359Z-observe-elemental-alternate-trait-framework`, retained log
SHA-256 `f5bd892e8a564980cdfae084944afc71976b17207f392048aceef91c071cd8e1`, and in
both the earlier routing-framework A and 90-ray candidate A. No new signature or
count increase occurred; these shader failures are neither suppressed nor
attributed to the catalog repair. F retains its four known ZFavoredClass custom
data exceptions. No shutdown obstacle exception occurred in these save-free runs.
Crystalline physical death/resurrection and polymorph qualification remains part
of the pending visible-trait physical lifecycle and is not inferred from catalog
or native suspend/reactivate checks.

| Evidence | Crystalline result SHA-256 |
| --- | --- |
| `20260907T1644230237202Z-observe-elemental-alternate-trait-framework` | `e7b27824a1a80955d2bd7ebd9fc3bad3fb81bc1a5ae09c3e6c561bed2e80614b` |
| `20260907T1637150575830Z-observe-elemental-alternate-trait-framework` | `8b56780b3bd38e686afb7dabcbb1345f9d9cb1e27b18f64e5554e625fece4707` |

The new read-only ranged-attack catalog records native/foreign ranged weapon base
damage types and real weapon-attack ability components without casting or changing
them. Native Vital Strike is Special; F also provides Special PinpointTargetingAbility
(`a6210acb28054f568ead7366bda31fee`) with foreign ContextActionAttack preserving its
ability reason. Cold Moon is the native energy-damage longbow; F adds the
Ray-category Produce Flame weapon. These are concrete inputs for the still-pending
Breeze boundary regression. No production Breeze change or native boundary PASS
is claimed from the catalog alone.


### Breeze native ranged-boundary regression (qualification pending)

The runtime suite now compares real Pinpoint Targeting commands with and without
Breeze-Kissed, recording native attack/roll ability reasons and weapon-stat
identity. It also compares actual Cold Moon attacks while enchanted and after
removing enchantments from only the disposable item instance, preserving its
native energy damage and registered blueprint. The regression expects the racial
bonus for the mundane ability and a native unenchanted weapon, and none for the
magical energy weapon. Production Breeze behavior is unchanged until a guarded
run proves whether those expectations expose a real mismatch.


Breeze boundary build 01 failed at compilation because the new runtime assertion
qualified DamageType in the energy-enum namespace instead of the native damage-rule
namespace. The reference is corrected; no failed-build package was installed and
no runtime result is inferred from it.


Breeze regression F `20260907T1710472981872Z` failed six assertions out of
6,388, with exact cleanup/restoration and no runtime exception summary. The
three actual Pinpoint commands retained the exact ability in both native attack
and roll contexts, nonmagical crossbow enhancement 0, and AC delta 0 instead of
2. This directly proves the all-ability-source rejection is too broad. The
other three failures exposed an invalid test precondition: native Cold Moon
fixed enchantments survive RemoveEnchantment and enhancement stays 4. Those
rows now assert the observed magical negative contract; no production energy
classification change is justified by them. The initial failure is preserved.

The narrow repair recognizes four exact registered Special feat actions
(Pinpoint and native Vital Strike ranks), retaining all other magic/unknown
source, geometry, identity and enhancement guards. The runtime regression
adds all three actual ranged Vital Strike commands in the CotW profile and
paired temporary +1 weapon controls. Bow Spirit is explicitly excluded even
though its type is Special. Qualification is pending.

Build 03 stopped at the exact test inventory guard after one focused test was
added. The inventory is now 1,453; no test was removed or skipped.

Build 04 identified the same current-suite count in 13 inherited static validation
records; those current inventory fields are updated to 1,453 without changing
version, GUID or historical qualification evidence.


### Breeze native weapon-source boundary qualified

Clean candidate 05 passes repository validation, all **1,453** domain/reflection
tests, clean Release compilation, deterministic packaging and strict 135-entry
package validation. Manifest remains 1,867 identities (1,865 active, two reserved).
No GUID or selector changes occur in this slice.

| Exact candidate identity | Value |
| --- | --- |
| Source parent | `018ad6b556b2c19e7d9454c6a1e9960f3b9527b9` |
| Qualified source state | `775c8cb173e369b7785190f0222e8773e30990f7337896136136a0d6ca154e90` |
| ZIP SHA-256 | `26017b7b3ce72507b377763fbc2def97045656bde37918a71007fba179a75237` |
| DLL SHA-256 | `9be96e6471bf9047141bd4972406c052fca1362e212f70998c13fdfa7efacdc6` |
| DLL MVID | `ff965e1e-7c82-4de3-9503-0c5627a00667` |

| Guarded Steam profile | Result | Assertions | Breeze observations |
| --- | --- | ---: | ---: |
| A `20260907T1727003973176Z-observe-elemental-alternate-trait-framework` | PASS | 6,217 | 199 |
| F `20260907T1722526350540Z-observe-elemental-alternate-trait-framework` | PASS | 6,409 | 220 |

F proves 24 paired native-command comparisons: four exact mundane feat abilities
across three Sylph heritages with ordinary and temporarily enchanted crossbows.
The real attack/roll reasons, exact weapon/stat objects, actual target AC and
completed native command are recorded. Ordinary attacks receive +2; enchanted
attacks receive zero. Pinpoint's pre-repair +0 failure remains preserved. Native
Vital Strike ranks use CotW's actual ranged delivery. A accurately records that
the optional Pinpoint surface is absent and does not claim those optional command
comparisons ran there.

Cold Moon's native Energy damage remains enhancement 4 in both observed item
states, receives no bonus, and retains the registered component-array reference.
The installed ranged catalog contains no demonstrated ordinary nonmagical energy
weapon: the other full-stack energy entry is Produce Flame's Ray weapon. Unknown
or nonphysical metadata still fails closed; no artificial unenchanted energy
weapon or blanket Special-ability exemption is introduced. The four catalogued
feat sources require exact registered object identity and Special type; all other
ability/context sources remain excluded, including Special Bow Spirit. Existing
crossbow/firearm/melee/masterwork/temporary-enhancement controls still pass.

Both profiles restore the exact mod tree and UMM bytes/timestamp, preserve all
save audit checks, and perform zero save loads/writes. No KMG ERROR or new exception
signature appears. Each retains the separately documented startup exception and
18 historical Pixel8bits shader exceptions; F retains four ZFavoredClass custom
JSON exceptions. The immutable candidate and detailed ledger are retained under
ignored `artifacts/qualification/0.0.117/character-creation-stabilization/` as
`breeze-boundary-candidate-05.*`; no raw evidence is committed. The original
installed candidate remains restored. This is a qualified mechanic slice, not
final human UI acceptance or the stable acceptance installation.


### Nineteen-trait native physical lifecycle instrumentation (pending)

The restored-ON phase now captures each of the 24 exact source persistence states
once before real native lethal damage, death, ResurrectAndFullRestore, Beast
Shape II, form removal, native heavy armor equip/remove and doll rebuild. It
compares owned marker/provider/ability/resource inventories and blood expenditure
at every transition, then exact post-resurrection stats and original item/slot/
inventory references after return. Native removal of transient buffs is recorded;
those effects are not recreated. Only the exact disposable actor is ticked by the
native life controller while the loaded world remains paused. Shared blueprints
and life-state flags are not assigned. Existing fixed-shell respec and cleanup
follow the completed physical sequence. Qualification is pending.

Physical build 01 stopped in the domain source guard before clean Release
compilation: the pre-respec feat check moved into the new lifecycle helper. The
guard now inspects that helper too, preserves the old check, and rejects direct
life-state or shared-blueprint assignment and direct KMG reconciliation. Normal
cleanup also avoids touching an already retired, completed source actor.

Physical build 02 exposed a test matcher that mistook `IsDead == expected` for
an assignment. The guard now distinguishes assignment from equality; the
prohibition is unchanged. No failed-build artifact was installed.

Physical build 03 passes all 1,453 tests but fails exact Release compilation
because the new helper was absent from the explicit project source inventory.
The helper is now included in that inventory; qualification remains pending.

Physical build 04 identified the missing native stats namespace import in the
new helper. That import is added; no production mechanic changed.


Physical candidate 05 F `visible-physical-f-01` preserves a failed acceptance
result. Fresh absence `20260907T1745063735770Z` passes 8, prepare
`20260907T1747400956148Z` passes 66, and module OFF
`20260907T1751244004876Z` passes 173. Restored ON
`20260907T1754451806056Z` fails while waiting after the first native resurrection;
its before-death and native-dead observations preserve every exact marker,
provider, ability, resource amount and blood expenditure. No physical PASS is
inferred. Existing failure cleanup saves a fixture-free working state; a fresh
absence check is still required. The complete original Mods tree and UMM bytes/
timestamp restore exactly; baseline and other-save audits pass.

Inspected native Resurrect clears wounds and death conditions and raises its
native event; UnitLifeController owns the ensuing life-state change. The paused
fixture ticked that controller during lethal death but omitted it while waiting
for resurrection. The new helper now ticks the same exact owned actor through
resurrection as well, with no life-state assignment, KMG reconciliation or
production mechanic change. Detailed native HP/death/view diagnostics and the
complete returned appearance observation are retained for any further mismatch.
Requalification is pending.


Physical candidate 06 F `visible-physical-f-02` also remains an overall FAIL.
Its fresh absence passes 8 and independently confirms candidate 05 cleanup;
prepare `20260907T1805004834696Z` passes 66 and OFF
`20260907T1808434006194Z` passes 173. ON
`20260907T1811593182858Z` passes all 168 physical observations (24 actors, all
19 visible traits), native respec, and exact cleanup. One of 253 final assertions
fails: the unchanged capture inventory expects 73 files, but the newly required
physical JSON report makes 74. Counts remain 24 sidecars, 48 PNGs and 120 views.
No exception summary is reported; normalPathComplete and structuralCleaned are
true, feat cleanup is exact, and the one authorized cleanup save completes.
The full original mod tree and UMM bytes/timestamp restore exactly, with baseline
and other-save audits passing. Fresh-process absence still must be repeated.

The capture guard now permits exactly one additional file only in restored ON,
and additionally requires that exact physical report, seven ordered successful
transitions per fixture, and the exact complete published trait inventory. Other
phase file counts remain unchanged. This strengthens the new evidence contract;
it does not waive the failed run. Native IsDead IL also confirms it reads the
controller-owned LifeState (2 for dead), supporting the earlier paused-controller
correction. Clean build and full fresh-process requalification are pending.


### Nineteen-trait physical lifecycle qualified

Candidate 07 passes repository validation, all **1,453** domain/reflection tests,
clean exact-reference Release compilation, deterministic packaging and strict
135-entry package validation. Manifest remains 1,867 identities (1,865 active,
two reserved); this slice changes guarded instrumentation only.

| Exact candidate identity | Value |
| --- | --- |
| Source parent | `cf5c2aa4733a1688cc2283949a0018d33898dbee` |
| Qualified source state | `a232699d5b073c36fd9c73b5396fb14452ea185af1afb95d064fffa22d2f42a9` |
| ZIP SHA-256 | `73f529e5991ccff802df6f94d3ba643754a1fae47375aaeadcc4ff6b9d589f9a` |
| DLL SHA-256 | `396009c961f5c03095b07c9f3b52f6fd1c53b4e61fa3776432f55bff5fb5897f` |
| DLL MVID | `81b77040-3e41-4869-a5d1-d17548b45170` |

Full-stack transaction `visible-physical-f-03` passes **508 assertions**:

| Fresh guarded Steam process | Assertions | Result |
| --- | ---: | --- |
| `20260907T1829125170896Z` initial absence | 8 | PASS |
| `20260907T1831397285381Z` prepare | 66 | PASS |
| `20260907T1835255385971Z` module OFF | 173 | PASS |
| `20260907T1838490780489Z` restored ON | 253 | PASS |
| `20260907T1847164220376Z` final absence | 8 | PASS |

All 24 loaded actors and all 19 published traits pass seven ordered native
physical states, **168 observations**: before death, dead, resurrected,
polymorphed, form return, armor equipped, original equipment/doll restored.
Every transition preserves the exact owned feature ranks, ability/resource
counts, spent resource amounts and blood expenditure. The 72 returned states
also verify native racial/passive mechanics and appearance. Native transient
buff removal is recorded and never reversed by instrumentation. Native life
controller ticks own the life-state transitions; the world clock stays paused.

The same cycle retains 168 trait-persistence records, 48 fixed-shell respecs,
72 native racial-selection records, all 11 Release B feats and their 25
identities, module-OFF level-up before rest, rest and re-spending, module-ON
restoration, exact cleanup and an independent fresh-process absence check.
Actual Player.RespecCompanion copyback is separately qualified above; these
fixed-shell records do not replace that evidence. The initial absence also
independently confirms candidate 06's cleanup after its metadata assertion
failure. Both earlier failed physical runs remain FAIL in the historical record.

The complete original Mods tree and UMM bytes/timestamp restore exactly;
protected baseline and other-save metadata audits pass. Only the named working
save receives authorized persistence/cleanup writes. Its archive intentionally
changes; final functional absence is proved by the native guarded load.

The standalone ledger initially assumed equal settings bytes before and after
each phase. Inspection proves the profile's PowerShell JSON is normalized by
KMG's existing settings serializer, with identical values: ON hashes
`0ca3c98bb0d791169a262f2b53649eff72e58acd1a37f0954172e66b4c2960d2`
to `a06601c52f1b98ac54eed309f7415677a3c55fe4c51daa2556dde5206c687f17`;
OFF hashes `b9a0f19d70085ef4fbda6ee4aeff87de89ee0e8efc0760edb7ca6989d5835fa8`
to `d07a06e1b67d35107ffd84da0e02453bfa0adcfaac59bcb68a4353444c7ec52e`.
The audit now derives and verifies both exact serialization forms and all
12 settings fields, as well as exact original restoration.

No KMG ERROR or exception summary occurs. Each process retains the native
BugReportCanvas startup exception and four known ZFavoredClass custom JSON
exceptions. Prepare retains the previously documented native shutdown
ObstaclesHelper offset `0x51`; restored ON records offset `0x6d` after
`scenario.complete` and manager destruction. The latter offset first appears
in candidate 06 and is not claimed to have an exact pre-mission stack match.
Read-only inspection of native method token `0x060017DD` proves `0x51` and
`0x6d` are the same unguarded EntityData.Descriptor dereference for the first
and second movement-group argument, respectively. Both have the unchanged
RemoveFromGroup `0xa0` / OnDisable `0x0c` shutdown stack. This is recorded as
a symmetric branch of the existing native teardown condition; no native code,
exception handler, foreign mod or production mechanic is patched or suppressed.
The ledger rejects other exception locations and any gameplay occurrence.

Immutable `visible-physical-candidate-07.*`, its full ledger, exact settings
serialization proofs and `native-obstacles-connect-units-il.txt` remain in the
ignored stabilization evidence directory. The original installation is restored;
final candidate identity, final profile attestation and owner UI acceptance
remain pending.


### Final candidate identity preparation

The numerical version remains 0.0.117; the informational identity is now
`0.0.117-elemental-char-gen-stabilization`. Version properties, assembly metadata,
the existing exact-version test and current validation milestone are updated
together. README, installation guidance, changelog and release notes identify
the stabilization candidate while preserving prior checkpoint evidence under
explicit historical labels. No production mechanic, GUID, module schema or
selection inventory changes in this slice. Final clean build, guarded identity
smoke, exact final profiles, pinned migration and acceptance installation remain
pending. Human full-screen acceptance remains NOT-RUN.

The first version validation correctly rejected the unchanged standalone package
suffix. The existing package script now uses the same stabilization suffix;
its deterministic build/package workflow and 135-entry contract are unchanged.

The inherited spear/bodyguard/audio validators also map numeric version 117 to
an exact package suffix. Their 117 branches and the existing package identity
test now require the stabilization suffix; all earlier version branches and
historical records are unchanged.


The informational identity slice passes all **1,453** domain/reflection tests,
repository validation, clean Release, deterministic packaging and strict 135
entries. Manifest remains 1,867. Preliminary candidate source parent is
`60f3d11743120c5c1a60de75148f5d7d8f6a4af4`, source fingerprint
`150272a4e2db7616292a278a7d1d133f99c2cfa5be9cf641293e88c111a032e7`,
ZIP `c9525fff8c3f239e98dc15fb053a50664bee44eb9c29a75b897bb61c365f2d3b`,
DLL `4b1d4087c1440705a1b3059c6034e103bcc86d268cd29519d41f58b8b84c7029`,
MVID `f62d9a55-416f-4bd0-a2e0-1409b36f5fc4`.
Full-stack `20260907T1906049378459Z-working-save-smoke` passes **11 assertions**,
with exact loaded informational version/source/DLL/MVID, protected-save audit
and original Mods/UMM restoration. No KMG ERROR appears; the native startup
and four known ZFavoredClass custom-data warnings remain separate.

The external audit reads the dedicated `runtime-loaded-build-identity.json` for
MVID and SHA rather than assuming every scenario's descriptive runtimeIdentity
string includes an MVID; the guarded exact identity contract is unchanged.
Package documentation now points to the handoff for final installation status,
so the build snapshot does not claim owner acceptance. The next build will be
from the committed source and will receive the final profile/migration matrix.


## Owner-authorized released-master integration - 2026-09-07

After the owner explicitly requested pulling and merging their new copy and
Skeletal Salesman firearms release, fetched origin and merged
`58d9511082af30f1a4ec88c1238ae7ae2b3651c2` into the clean feature head
`314438f44b50e85d7f9b7936ab062facf59d796d`. This authorization permits
master into the feature branch; this mission still does not publish, tag,
or merge the elemental candidate back into master.

The incoming public release is `0.0.116-midgame-firearms-and-protection`.
Its Roadwarden and Dead Reckoning source, two fixed item identities, native
Skeletal Salesman stock publication, and Protection descriptions are retained.
All entries from both parents preserve their complete original metadata.
The combined ledger contains **1869 entries: 1867 active and 2 reserved**.
The candidate remains `0.0.117-elemental-char-gen-stabilization`. Both public
116 notes and historical unpublished elemental-feat 116 notes are preserved;
the new release does not rename or reuse any elemental GUID.

Nineteen textual merge conflicts were resolved around version metadata,
append-only manifests, cumulative tests, guarded scenario dispatch, and
historical documentation. The inherited validator now includes the two exact
released firearm entries and checks the owner's revised Protection wording.
The released content validator is retained alongside the elemental and
Brown-Fur provider contracts. Existing release evidence remains attributed to
its original DATA artifact and environment.

`powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1`
passes repository validation, **1458/1458 domain/reflection tests**, clean
exact-reference Release compilation, deterministic packaging, and strict
135-entry package validation (integration build08). Qualified dirty merge
artifact before this evidence-only journal update:

- Parent commit: `314438f44b50e85d7f9b7936ab062facf59d796d`
- Source fingerprint: `5d0d14b51c4fa0d803c471cf42eac06ea480ad6ca55803c947022083879d9116`
- ZIP SHA-256: `0676cea2775b9dc93ded1406f623d3ecf10662ff9bcbef9b6d37a6659f18a336`
- DLL SHA-256: `26a07cdcf32c563829e5aee6e919a98eafd965bdb601a473d51855783adff947`
- DLL MVID: `1a9b60a4-e330-4862-934c-609420ac0d34`

Guarded full eleven-mod profile F runs used the backup-first installer and
Steam App ID 640820 with exact artifact attestation:

| Run | Result | Scope |
|---|---|---|
| `20260907T2231077396456Z-disposable-midgame-firearms` | PASS, 92 assertions | Native merchant stock, prices, purchases, fixed properties, Protection text; all 12 attempted native combat messages captured exactly |
| `20260907T2235392028913Z-disposable-elemental-character-creation-baseline` | PASS, 11 assertions | Four actual native first-level creator routes through complete final review, racial Determinator before Skills/Abilities, both global Traits populated; no character committed in this review-only profile |

Both profiles restore the original Mods tree and UMM/settings exactly. Save
scope audits pass with zero loads/writes. Independent native routing and
foreign-selector audits pass; no new KMG ERROR occurs. Native startup and
ZFavoredClass custom-data warnings remain attributed separately.

The first imported firearm fixture run
`20260907T2225162076031Z-disposable-midgame-firearms` passed 91 mechanics
assertions but failed this mission's zero-KMG-error gate because the unloaded
main menu has no BattleLogView. The fixture now reuses the existing bounded
final-UI-sink capture and asserts messages equal native publication attempts,
with no new publication fault. Native message validation, attack rules,
production logging and personal saves are unchanged. The failed log gate is
retained as evidence, not relabeled a clean pass.

The earlier exact314 artifact's final Sylph/Oread native respec checks passed
16 commits, 14 real Player respec callbacks and 128 exact racial graphs;
Elemental OFF/full-stack also passed. However, the subsequent KMG-alone
framework run `20260907T2159441652088Z` failed Stormsoul's Cold Moon paired
attack control (AC delta -1), then the Calm native command did not act and no
Calm buff appeared. It restored exactly. The cause remains under investigation;
this integration checkpoint does not waive that failure, install a final
candidate, or claim stabilization/human UI acceptance complete.


## Native Cold Moon control contamination resolved - 2026-09-07

The failed exact314 framework attempt remains FAIL. A narrowly instrumented
reproduction, `20260907T2243476180515Z-observe-elemental-alternate-trait-framework`,
failed 3 of 662 assertions and captured the precise cause: native Longbow of
Cold Moon (`65d29ca8c81c124418417bff73f8eaae`) landed a real critical hit. Its
registered enchantment (`9007910ac92ac624b9e67827104b9d77`) applied native
Paralysis (`af1e2d232ebbb334aaf25e2a46a92591`) for one round. CanAct changed
from true to false; all six attributes, including Dexterity, were unchanged.
The following AC lost its Dexterity contribution and the next Calm command
could not act. This is contamination between native fixture controls, not a
reason to change Breeze-Kissed's gameplay command or defense rules. The
instrumented failure restored the exact original profile and touched no save.

The disposable fixture now records the exact donor/enchantment contract and
before/after/native-restored unit state. It requires a buff-free active owned
target and the exact Cold Moon item. Any critical condition must be a single
finite native Paralyzed/Staggered effect from the actual attacker with the
recorded one-round deadline. It must remain immediately before EndTime and
expire immediately after EndTime through native Buffs.Tick. The clock then
returns exactly, and all buffs, conditions and six attributes must match the
original baseline before the next comparison. No direct buff removal,
condition rewrite, EndTime override, fabricated hit/save or weapon mutation is
used. An actual native critical witness is required for each Sylph heritage;
seeded ordinary dice are bounded and the prior random state is restored.

This also records every subsequent native Breeze command's pre-state; all
60 A/F command pre-states are active with no inhibiting native condition.
Calm/Renew, gust success/failure/immunity, spent use/rest, attack-source and
semantic-ray framework controls all pass. No Breeze production mechanic or
GUID changed. The focused current source gate requires the expiry witnesses
and exact-restoration assertions.

`Build-Local.ps1` passes repository validation, **1458/1458 full tests**, clean
exact-reference Release, deterministic 135-file package and strict validation
(`calm-native-expiry-build-01.log`). Artifact before this journal-only update:

- Parent commit: `a3d7288b5930af1ba476e6ccef1d7abe9b680e4a`
- Source fingerprint: `386fd86a2c943930363a21108e4bc8d6ecaa0b4df8123b4c6037f6c631e8da86`
- ZIP SHA-256: `27e54044d13ec2657a93a474c0076778f2dfea519cb00887685abe4e6e5c123f`
- DLL SHA-256: `3682c8dbfd5885d35b5062d1355d9f3f3230f990d1ab63696a3a5ff8070381d0`
- DLL MVID: `b6132974-4d7d-4159-b216-f834c650f2fe`

| Profile | Guarded native run | Assertions | Exact attack restorations | Actual critical conditions expired | Later command pre-states |
|---|---|---:|---:|---:|---:|
| A | `20260907T2252366139270Z-observe-elemental-alternate-trait-framework` | 6325 | 105 | 5 | 30 |
| F | `20260907T2255142065106Z-observe-elemental-alternate-trait-framework` | 6517 | 105 | 3 | 30 |

All **12842** assertions pass. Both profiles use guarded Steam 640820 launches,
attest exact commit/fingerprint/ZIP/DLL/MVID, have zero KMG ERROR, preserve
baseline/other saves, and restore Mods, settings and UMM exactly. Native
startup/Pixel8bits and four F ZFavoredClass custom-JSON warnings remain separate.
The failed reproduction is retained, not reclassified as PASS. The latest
master was fetched again and remains `58d9511082af30f1a4ec88c1238ae7ae2b3651c2`.

Final combined-artifact compatibility, migration, persistence, creator,
Player respec, turn-cost, installation and owner handoff gates follow this
checkpoint. Human full-screen acceptance remains NOT-RUN. No feature merge
back to master, new tag or public release is performed.


## Final automated qualification and installed owner-acceptance candidate

## Character-creation stabilization candidate (2026-09-07)

The exact `0.0.117-elemental-char-gen-stabilization` candidate is installed for
owner acceptance. Source commit: `132f0650e997579c589d19874a022aa5ee2213f2`.
All 1,458 domain/reflection tests, clean Release, deterministic/strict package,
final compatibility, migration and the 19 visible traits' persistence/lifecycle
checks pass. See the [stabilization acceptance handoff](docs/ELEMENTAL-RACES-CHARACTER-CREATION-STABILIZATION-HANDOFF.md) for exact
artifact hashes, run IDs, inventories, limitations, backup and owner checklist.

Heritages and alternate racial selections use the native Heritage route before
allocation and skills. Both ordinary Trait roots complete with Bodyguard OFF
and ON. Helpful preserves the foreign empty Features contract. Treacherous Earth
and Nereid Fascination remain registered but unpublished; the only added content is the owner-authorized released master integration.
The original empty-Trait trigger remains unconfirmed. Human full-screen UI
acceptance is **NOT-RUN**, so the owner-reported acceptance gate is not marked PASS.
Released master was merged into the feature branch at the owner's request.
No feature-to-master merge, tag or public release was performed; stop here for owner acceptance.


# Elemental Races character-creation stabilization acceptance handoff

## Candidate and acceptance status

The disposable candidate is `0.0.117-elemental-char-gen-stabilization` (numeric
0.0.117), built from `132f0650e997579c589d19874a022aa5ee2213f2` on
`codex/elemental-races-expansion`. Mission start was
`c7df5de21ea2860b99a25777b021d7f44a5858fa`; its last production-code parent was
`efc9d54ec29dbdd84ffe328183139f517c2f3350`.

Automated readiness and backup-first installation **PASS** for this exact artifact.
The final matrix has 13,847 passing assertions across 28 fresh
guarded Steam processes (20 final profiles, three pinned migration processes
and five visible-trait persistence processes). No owner UI PASS is claimed.

Owner full-screen UI acceptance is **NOT-RUN**. Native creator/controller tests
are structured functional evidence, and include completed real disposable
characters, but do not relabel the owner's reported failure or substitute for
the owner's ordinary full-screen entry point. The owner-authorized released master was merged into this feature branch.
No feature-to-master merge, tag or public release is performed by this mission. The two deferred mechanics and favored-class bonuses remain out of scope.

## Owner-authorized upstream integration

The owner requested integrating their new public release during final qualification.
Master `58d9511082af30f1a4ec88c1238ae7ae2b3651c2` was merged as
`a3d7288b5930af1ba476e6ccef1d7abe9b680e4a`. Roadwarden, Dead Reckoning,
their native Skeletal Salesman stock integration and the released Protection
wording are retained. The complete GUID ledgers from both parents remain exact.
The final combined artifact was rebuilt and qualified again; earlier evidence
retains its original artifact attribution.

## Diagnosis and exact changes

Kingmaker 2.1.7b's actual CharacterBuildController dispatcher routes Group None
(0), and Racial (11) by itself, to generic Abilities. The original four KMG
heritages and ten replacement-slot selections used None. Native Aasimar and
installed Races Unleashed use AasimarHeritage (42), which creates the native
Determinator/Heritage consumer before allocation/skills. Native Tiefling uses
its corresponding heritage group (45). The repair uses the inspected 42
contract, confirmed in real first-level native phase state.

- Heritage factory: Group 42, Group2 None, Groups Racial; three fixed choices,
  stable GUIDs, existing ordered owned Features/AllFeatures arrays.
- Alternate-trait factory/policy: Group 42, Group2 None, Groups 42, obligatory
  non-class selections, legal unconditional retain-base routes; preserved
  replacement-slot reconciler and trait/provider identities.
- Treacherous Earth and Nereid Fascination: registered hidden markers/providers,
  omitted from every player selection array, excluded before overlap/provider
  resolution. Runtime assertions reject these exact published marker GUIDs and
  any published alternate without its specific implemented mechanic.
- Optional Helpful transaction/resolver/coordinator: retain foreign Features
  exactly and append Helpful once to authoritative AllFeatures. The inspected
  empty-Features contract is required; GUID-aware ordered additions, rollback,
  idempotence and repeated live callbacks preserve foreign entries/references.
- Visual registry/retention: retain exact owned proxies/inner assets and native
  donor/palette assets across creator preview unloading; no global unload rewrite.
- Native respec bridge: preserve owned daily resource amounts and blood healing
  expenditure across replacement creation, preview, native rest and Player
  copyback; no changes to foreign resources, maxima, choices, stats or facts.
- Breeze boundary: recognize four exact registered mundane Special feat attacks
  (Vital Strike ranks and CotW Pinpoint), preserving ordinary +2 ranged defense;
  magical, ray, unknown and nonphysical attacks remain excluded.
- Crystalline policy: fixed reviewed 93-identity semantic ray catalog with native
  delivery/parent/geometry guards; no optional-mod compile-time dependency.
- Guarded scenarios/tests: read-only routing snapshots; real native creators,
  back-navigation and Player respec; native turn costs; all 19 visible traits'
  fresh-process persistence and physical lifecycle matrix.

Primary changed source locations (the branch diff and journal retain the full change history):

| Change | Source |
| --- | --- |
| Heritage phase | [ElementalHeritageBlueprintFactory.cs](src/KingmakerGunslinger/ElementalRaces/ElementalHeritageBlueprintFactory.cs) |
| Racial slots and deferred publication | [ElementalAlternateTraitBlueprintFactory.cs](src/KingmakerGunslinger/ElementalRaces/ElementalAlternateTraitBlueprintFactory.cs) |
| Slot compatibility and deferred normalization | [ElementalAlternateTraitPolicy.cs](src/KingmakerGunslinger/ElementalRaces/ElementalAlternateTraitPolicy.cs) |
| Foreign Combat selector transaction | [HelpfulPublicationTransaction.cs](src/KingmakerGunslinger/AidAnotherCompatibility/HelpfulPublicationTransaction.cs) |
| Foreign object/contract resolution | [FavoredClassTraitResolver.cs](src/KingmakerGunslinger/AidAnotherCompatibility/FavoredClassTraitResolver.cs) |
| Repeated optional lifecycle reconciliation | [AidAnotherOptionalExtensionCoordinator.cs](src/KingmakerGunslinger/AidAnotherCompatibility/AidAnotherOptionalExtensionCoordinator.cs) |
| Native respec resource ownership | [ElementalNativeRespecResourceRuntime.cs](src/KingmakerGunslinger/ElementalRaces/ElementalNativeRespecResourceRuntime.cs) |
| Spent-resource policy | [ElementalRespecResourcePolicy.cs](src/KingmakerGunslinger/ElementalRaces/ElementalRespecResourcePolicy.cs) |
| Blood expenditure persistence | [ElementalBloodRuntime.cs](src/KingmakerGunslinger/ElementalRaces/ElementalBloodRuntime.cs) |
| Creator visual asset retention | [ElementalCharGenVisualRetentionPatch.cs](src/KingmakerGunslinger/ElementalRaces/Visuals/ElementalCharGenVisualRetentionPatch.cs) |
| Breeze attack-source boundary | [ElementalBreezeKissedMechanics.cs](src/KingmakerGunslinger/ElementalRaces/ElementalBreezeKissedMechanics.cs) |
| Semantic ray catalog | [ElementalCrystallineFormPolicy.cs](src/KingmakerGunslinger/ElementalRaces/ElementalCrystallineFormPolicy.cs) |
| Real native phase observation | [ElementalCharacterCreationRoutingObserver.cs](src/KingmakerGunslinger/RuntimeTesting/ElementalCharacterCreationRoutingObserver.cs) |
| Native back-navigation and completion | [ElementalCharacterCreationRegression.cs](src/KingmakerGunslinger/RuntimeTesting/ElementalCharacterCreationRegression.cs) |
| Real Player respec regression | [ElementalCharacterCreationNativeRespec.cs](src/KingmakerGunslinger/RuntimeTesting/ElementalCharacterCreationNativeRespec.cs) |
| Native critical-effect test isolation | [ElementalBreezeKissedScenario.cs](src/KingmakerGunslinger/RuntimeTesting/ElementalBreezeKissedScenario.cs) |

The original empty ordinary Trait screen is **not conclusively attributed**.
Profile B with production KMG disabled works, but the original KMG-enabled
General-route controls also exposed legal global choices. Inspection proves
AllFeatures is authoritative and the old Features mutation violates the foreign
contract; it does not prove that mutation caused the owner's empty screen.
Current native first-level tests demonstrate two populated/completable global
Trait roots with Bodyguard OFF and ON. The original UI trigger remains an owner
acceptance check; unrelated ZFavoredClass custom-data exceptions are unchanged.

| Foreign Combat Trait surface | Original 117 | Stabilized Bodyguard ON | Bodyguard OFF |
| --- | --- | --- | --- |
| Features | 0 -> 1, replaces empty reference | 0 -> 0, exact empty reference retained | 0, unchanged |
| AllFeatures | 14 -> 15 | 14 original ordered exact objects + Helpful once | 14 originals, Helpful absent |
| Top-level global Trait roots | two roots, each eight categories | both retained and completable | both retained and completable |

Helpful remains GUID `e4b29a7c8d5f4c1796ab03e1f72d8456`. The separate existing
Eastern Heirloom Equipment addition remains additive (49 -> 50 in applicable
profiles) and preserves empty Features. The earlier exact314-artifact observer
`20260907T1936273690252Z` captures the same Combat object `ref-46` throughout:
Features stays the empty `ref-3141`; AllFeatures changes once from `ref-3142`
(14) to `ref-10185` (15), then retains that exact reference at runtime readiness.
All 14 original choice records, including object identities and metadata, are
identical and ordered. Those choices are Anatomist, Armor Expert, Berserker of
the Society, Blade of the Society, Defender of the Society, Deft Dodger, Dirty
Fighter, Reactionary, Resilient, Slippery, Dragon Armor, Fencer, Honored Fist of
the Society and Threatening Defender. Reference labels apply only within this
process; the GUIDs carry cross-process identity. Elemental feat factory/publication
source is byte-identical to mission start.

## Visible inventory

| Race | Heritage choices | Implemented alternate racial traits |
| --- | --- | --- |
| Ifrit | General, Lavasoul, Sunsoul | Wildfire Heart; Brazen Flame; Fire in the Blood; Efreeti Magic; Forge-Hardened; Fire Insight |
| Oread | General, Gemsoul, Ironsoul | Crystalline Form; Earth Insight; Granite Skin; Stone in the Blood |
| Sylph | General, Smokesoul, Stormsoul | Air Insight; Breeze-Kissed; Like the Wind; Secretive; Storm in the Blood; Thunderous Resilience; Whispering Wind |
| Undine | General, Mistsoul, Rimesoul | Acid Breath; Ooze Breath |

Ten existing replacement-slot selections retain their retain-base choices.
Oread's SLA slot currently offers retain base only. Multi-slot exclusions leave
that completion route available. All four races, twelve heritage markers and
all previous provider, affinity, SLA, resource, visual and feat GUIDs remain
stable. The original ordered 1,867-entry manifest identity/type/status inventory
matches mission start. Four descriptive notes, for the two deferred markers and
their providers, explicitly record their unpublished/inert status; identity,
type and registration status are unchanged. There are 21 registered traits, 19
published choices.

Deferred marker identities remain resolvable and are never selectable:
Treacherous Earth `e117e1e0a17a4acec001000000000031`;
Nereid Fascination `e117e1e0a17a4acec001000000000040`.
Their providers/other reserved identities are retained; deferred marker-only
states reconcile conservatively without consuming an SLA replacement slot.

## Qualification evidence

Repository validation, all **1,458** domain/reflection tests, clean exact-reference
Release compilation, deterministic package creation and strict **135-entry**
validation pass. The manifest has **1,869 identities: 1,867 active, two reserved**.

Commands include:

- `python tools/validate_repository.py`
- `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Build-Local.ps1`
  (includes clean full domain suite, Release, deterministic/strict package gates).
- Guarded `scripts/Invoke-KingmakerRuntimeTest.ps1`, Steam App 640820, through
  exact reversible profile transactions and the named `KMG_AUTOMATION_WORKING`
  save. The immutable local matrix command and per-process run IDs are below.

| Final profile / scenario | Guarded run | Assertions | Creator evidence | Restoration |
| --- | --- | ---: | --- | --- |
| mechanics-a | `20260907T2304369475236Z-observe-elemental-alternate-trait-framework` | 6325 | mechanical / compatibility / smoke | exact |
| mechanics-f | `20260907T2307155108037Z-observe-elemental-alternate-trait-framework` | 6517 | mechanical / compatibility / smoke | exact |
| turn-costs-a | `20260907T2310214408383Z-disposable-elemental-trait-turn-costs` | 103 | mechanical / compatibility / smoke | exact |
| turn-costs-f | `20260907T2312287709697Z-disposable-elemental-trait-turn-costs` | 103 | mechanical / compatibility / smoke | exact |
| firearms-f | `20260907T2315194825862Z-disposable-midgame-firearms` | 92 | mechanical / compatibility / smoke | exact |
| creator-a | `20260907T2348135161663Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| creator-g | `20260907T2350594465597Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| creator-h | `20260907T2354359368905Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| control-b | `20260907T2357227910606Z-disposable-global-traits-kmg-disabled-control` | 8 | characters: 1, commits: 0 | exact |
| creator-c | `20260908T0000055911468Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| creator-d | `20260908T0004438265554Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| creator-e | `20260908T0009222820644Z-disposable-elemental-character-creation-baseline` | 11 | characters: 4, commits: 0 | exact |
| creator-f-ifrit | `20260908T0014115124448Z-working-save-elemental-character-creation-regression` | 12 | characters: 3, commits: 3 | exact |
| creator-f-oread | `20260908T0020317226892Z-working-save-elemental-character-creation-regression` | 12 | characters: 3, commits: 3 | exact |
| creator-f-sylph | `20260908T0026382486968Z-working-save-elemental-character-creation-regression` | 12 | characters: 3, commits: 3 | exact |
| creator-f-undine | `20260908T0032393745170Z-working-save-elemental-character-creation-regression` | 12 | characters: 3, commits: 3 | exact |
| respec-f-sylph | `20260908T0038274374916Z-working-save-elemental-native-respec` | 12 | characters: 8, commits: 8 | exact |
| respec-f-oread | `20260908T0050248888147Z-working-save-elemental-native-respec` | 12 | characters: 8, commits: 8 | exact |
| elemental-off-f | `20260908T0102208611442Z-elemental-races-races-unleashed-compatibility` | 13 | mechanical / compatibility / smoke | exact |
| smoke-f | `20260908T0105054634241Z-working-save-smoke` | 11 | mechanical / compatibility / smoke | exact |

| Final migration / persistence process | Assertions | Result |
| --- | ---: | --- |
| `20260907T2318128194308Z-elemental-race-persistence-prepare` | 11 | PASS |
| `20260907T2321060370196Z-elemental-race-legacy-migration` | 10 | PASS |
| `20260907T2323466381836Z-elemental-race-persistence-verify-absent` | 8 | PASS |
| `20260907T2327205130928Z-elemental-race-persistence-verify-absent` | 8 | PASS |
| `20260907T2329536201419Z-elemental-race-persistence-prepare` | 66 | PASS |
| `20260907T2333420072446Z-elemental-race-module-disabled-persistence` | 173 | PASS |
| `20260907T2337019456951Z-elemental-race-module-restored-persistence` | 253 | PASS |
| `20260907T2345095531645Z-elemental-race-persistence-verify-absent` | 8 | PASS |

The independent native-state audit covers 53 character records and 28 native
commits. Every observed final review has an enabled completion button and zero
unresolved selections. The reduced profiles have no commits; F contains the
actual creator and Player-respec completions. All exact phase consumers,
three-choice heritage lists and two completed global roots are verified.

Final matrix command (machine-local ignored wrapper, using committed guards):

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File artifacts/qualification/0.0.117/character-creation-stabilization/Invoke-MergedFinalQualificationMatrix.ps1 -RunPrefix merged-final-01 -IdentityName final-acceptance-candidate-02
```

Pinned 114 producer package SHA-256 is
`b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694`,
from `6874dc15a27ded132456dbdd480f47c794543a05`, without an overlay. Eight
legacy characters lacking heritage markers load as General with spent state
preserved; cleanup passes in a fresh final-artifact process. Release A and B
persistence remains covered in the five-process final matrix (11 feats / 25
feat identities, 24 race/sex/heritage actors).

Earlier focused qualifications remain preserved in the mission journal:
12 real native creator commits / 32 final reviews / 184 exact racial graphs
with all three heritages per race and point-buy/Dice Roller back-navigation;
32 commits / 28 real Player respec callbacks / 240 exact graphs for daily
resources; 24 blood-resource commits / 21 callbacks / 192 blood observations;
103 native turn assertions in each of A and F, covering all Sylph/Undine
heritages, actual swift/standard spending, same-turn rejection and cancellation;
93 semantic ray identities with 294 full-stack paired AC controls; 24 paired
Breeze mundane/magical native-command comparisons. Failed precursor runs are
retained as FAIL and do not count toward readiness.

All 19 traits are covered by the 24-actor save matrix, including the nine
previously unqualified visible traits. The matrix exercises OFF loading,
level-up before rest, rest/re-spend, ON restoration, respec and exact cleanup.
Physical coverage includes seven ordered states per actor: death/resurrection,
polymorph/return, equipment and doll rebuild, with exact fact/resource/provider
state and 72 returned-state mechanic checks. Native removal of transient buffs
is recorded and not reversed by the fixture. Fresh-process absence is required.
Actual Player respec and fixed-shell persistence respec are reported separately.
Reduced-stack A/G/H/C/D/E creator checks reach the native final review without
committing a character. The actual completed characters and Player respec
callbacks come from the full-stack F runs; these evidence scopes are distinct.

All final profiles retain exact settings hashes and mod manifests, and restore
their original trees and UMM bytes/timestamps. In per-phase OFF/ON transactions,
PowerShell and KMG serialization hashes differ while the exact 12 setting values
match; both forms and final byte-exact restoration are verified. The baseline
and personal saves are protected. Only the named disposable working save receives
authorized persistence/cleanup writes; native fresh-load absence, not archive
byte equality, proves functional cleanup.

## Final harness preflight correction

The final preflight initially failed its fixed inventory list and a directory
metadata comparison. The two live scenario catalogs already agreed: 222 entries;
the test's 218-entry list omitted the four newly released midgame firearm
scenarios. The directory comparison reported only three old directory timestamps,
with no file entry differences. The test now refreshes filesystem metadata before
both snapshots, retaining all path, length and timestamp comparisons, and adds
four explicit firearm scenario safety assertions.

The corrected test passes **259 checks**, including unsupported-request rejection,
no build/staging, no backup/evidence creation, and no Steam/game process launch.
It was qualified in an isolated copy against the unchanged candidate before
installation; the identical test correction accompanies this handoff. The
original failed run remains recorded. The canonical command subsequently passed
all 259 checks from the final working tree. Repository validation and a fresh
Release rebuild/run of the full domain/reflection suite also pass: 1,458 tests,
zero failures. The game code and installed DLL are unchanged by this test
correction. Run the canonical test with:

`powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-RuntimeScenarioPreflight.ps1`

## Compatibility and warnings

Profile definitions and optional DLL/settings hashes are retained in the journal
and local startup identity record. A=KMG; G=KMG+CotW; H=KMG+Races Unleashed;
B=CotW+ZFavoredClass with production KMG disabled; C=CotW+ZFavoredClass+KMG,
Bodyguard OFF; D=same, Bodyguard ON; E=KMG+CotW+ZFavoredClass+Races Unleashed;
F=the owner's complete eleven-mod stack. ZFavoredClass requires CotW, so its
requested combinations are tested with that dependency, not reported as an
unsupported standalone ZFavoredClass launch. F includes Bag of Tricks 1.16.4,
Dice Roller 0.1.2 and Tweak or Treat 1.1.0 with the exact original settings.

An earlier exact314-artifact matrix stopped after Ifrit's successful native
run because its external auditor incorrectly required a Dice-specific flag on
point-buy reviews. The source writes that field only for rolled routes; the
request explicitly selected point-buy. Corrected auditing verified the unchanged
Ifrit evidence (three commits, eight reviews, 50 graphs). That outer audit
failure remains recorded. Matrix06 then completed the other three races, two
Player respec routes and module OFF, before its mechanics failure below.
These earlier matrices are historical, not the final merged-artifact readiness
ledger. The new merged matrix repeats all required profiles on one exact
combined artifact, with mechanics and turn costs first.

The failed framework run `20260907T2159441652088Z` and instrumented reproduction
`20260907T2243476180515Z` remain FAIL. Structured before/after state proves the
native Longbow of Cold Moon (`65d29ca8c81c124418417bff73f8eaae`) inflicted its
critical-hit Paralysis buff (`af1e2d232ebbb334aaf25e2a46a92591`), changing
CanAct from true to false and removing Dexterity from the next AC calculation.
The next Calm command therefore could not act. This was contamination between
native test controls; Breeze gameplay mechanics were unchanged. The fixture
now requires each finite, correctly sourced native critical condition to remain
until its recorded EndTime and expire through Buffs.Tick, then verifies exact
original actor state and clock before the next comparison. A seeded native
critical witness is required for every Sylph heritage; no hit/save result,
condition, duration or registered weapon is overridden. All later Calm commands
must start from an active, unconditioned native state. Full A/F framework checks
and the final combined artifact requalify this correction.

The first imported firearm test passed 91 functional assertions but had KMG
logging errors because its main-menu fixture lacked BattleLogView. Its scoped
final-UI-sink capture now verifies every native message is delivered without
new publication faults; gameplay logging is unchanged. The qualified merged
run `20260907T2231077396456Z` passes 92 assertions with 12/12 messages and zero
KMG errors. The final exact artifact repeats that check.

Two final-matrix launches failed before the guarded request was accepted because
Unity could not load the unchanged signed native steam_api64.dll (error 126).
Neither touched saves; both restored exactly and remain FAIL. A separate native
Steam readiness run and the subsequent five fresh-process persistence runs pass.
The library, Steam configuration and launch contract were not modified.

An attempted A saved-world run then failed in native Player.PostLoad at the
main-character UniqueId lookup, before creator initialization. The cause of that
reduced-stack save incompatibility is unresolved; the same native failure was
already recorded in early run A05 (20260907T0444519014086Z), with a successful
full-stack working-save control immediately afterward. It is not called a KMG
routing defect or a successful load. No save contents were altered. The reduced-stack
creator tests use the registered native disposable first-level review fixture
with no saved-world load. FullF creation, migration and persistence still require
actual working-save load and native completion.

| Final profile | KMG ERROR | Known ZFavoredClass custom JSON exceptions | Native exceptions |
| --- | ---: | ---: | ---: |
| mechanics-a | 0 | 0 | 19 |
| mechanics-f | 0 | 4 | 19 |
| turn-costs-a | 0 | 0 | 1 |
| turn-costs-f | 0 | 4 | 1 |
| firearms-f | 0 | 4 | 1 |
| creator-a | 0 | 0 | 1 |
| creator-g | 0 | 0 | 1 |
| creator-h | 0 | 0 | 1 |
| control-b | 0 | 4 | 1 |
| creator-c | 0 | 4 | 1 |
| creator-d | 0 | 4 | 1 |
| creator-e | 0 | 4 | 1 |
| creator-f-ifrit | 0 | 4 | 1 |
| creator-f-oread | 0 | 4 | 1 |
| creator-f-sylph | 0 | 4 | 1 |
| creator-f-undine | 0 | 4 | 1 |
| respec-f-sylph | 0 | 4 | 1 |
| respec-f-oread | 0 | 4 | 1 |
| elemental-off-f | 0 | 4 | 1 |
| smoke-f | 0 | 4 | 1 |
| migration: `20260907T2318128194308Z-elemental-race-persistence-prepare` | 0 | 4 | 1 |
| migration: `20260907T2321060370196Z-elemental-race-legacy-migration` | 0 | 4 | 1 |
| migration: `20260907T2323466381836Z-elemental-race-persistence-verify-absent` | 0 | 4 | 1 |
| persistence: `20260907T2327205130928Z-elemental-race-persistence-verify-absent` | 0 | 4 | 1 |
| persistence: `20260907T2329536201419Z-elemental-race-persistence-prepare` | 0 | 4 | 2 |
| persistence: `20260907T2333420072446Z-elemental-race-module-disabled-persistence` | 0 | 4 | 1 |
| persistence: `20260907T2337019456951Z-elemental-race-module-restored-persistence` | 0 | 4 | 2 |
| persistence: `20260907T2345095531645Z-elemental-race-persistence-verify-absent` | 0 | 4 | 1 |

Historical native startup BugReportCanvas, Pixel8bits shader diagnostics, and
native game-exit ObstaclesHelper teardown are recorded separately from KMG
failures. The teardown's `0x51` and `0x6d` offsets dereference the first/second
argument's missing EntityData in the same native method; the `0x6d` variant was
first observed in this mission's physical fixture and is explicitly disclosed.
No exception handler, third-party implementation or native teardown is patched
or suppressed. ZFavoredClass's four missing custom JSON entries
(`bonus_charmed_life.json`, `bonus_panache.json`, `arcane_archer.json`,
`deadeye_devotee.json`) persist in the KMG-disabled control; no general
ZFavoredClass repair is attempted.

Native tests use real engine creator phases and completed disposable characters.
Owner full-screen acceptance remains separate. The observed native ordering is
Race -> ClassInChargen -> Determinator (Choose heritage) -> Skills (allocation
and skills) -> Abilities (feats and ordinary Trait selections) -> Character
-> Total. The native enum's broad Abilities phase contains the later ordinary
Trait selectors; elemental heritages and alternate racial selections have only
the Determinator consumer. The installed catalog contains
no demonstrated nonmagical ordinary energy ranged weapon; Breeze's nonphysical
policy is checked structurally/purely and against the available native magical
energy weapon. Unknown weapon/ability metadata fails closed. Unclassified opaque
spell wrappers, including the installed Prismatic Surge wrapper, are not treated
as semantic rays merely for using shared transport. These limits do not claim coverage of arbitrary future optional mods.

## Package and installation

| Identity | Exact value |
| --- | --- |
| Artifact source commit | `132f0650e997579c589d19874a022aa5ee2213f2` |
| Source fingerprint | `86e70e45065704e67d0bc161c336c99d902d1cae48a2b40c327436ed6db29d8f` |
| Informational version | `0.0.117-elemental-char-gen-stabilization` |
| ZIP SHA-256 | `7b930f3f084796f81e9d7cb0a6babea248404c87eb72381509019c8843c9ec14` |
| DLL SHA-256 | `fcfe67df81077f9add11ad3770168161c65f3906805fe1c483ae096130892fef` |
| DLL MVID | `35dd73ec-4168-457b-8c03-5135e0e9e1ee` |
| Manifest / package entries | 1,869 / 135 |

The standalone archive is
`artifacts/packages/KingmakerGunslinger-0.0.117-elemental-char-gen-stabilization.zip`.
The guarded installation uses the byte-identical
`artifacts/local-runtime/0.0.117/KingmakerGunslinger-0.0.117-local-runtime.zip`
and its immutable `.build-local.json` sidecar. An immutable copy is retained in
the ignored stabilization qualification directory. Generated artifacts are not
committed. Final documentation and the preflight test correction follow the artifact source commit; no
unqualified source change is folded into the installed DLL.

Installed mod:
`C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Mods\KingmakerGunslinger`.

Recoverable previous installation:
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260908T0120399445825Z`.

Deployment record:
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260908T0120432427216Z\deployment.json`.
All 135 package files independently match the ZIP byte for byte; the only extra
file is the preserved FeatureModules.json. Its bytes/timestamp and the original
UMM bytes/timestamp remain exact. All unrelated mod folders and protected saves
remain exact, and no game is left running. The old DLL in the backup retains
SHA-256 `b3839a63fb83a5894169fa7ea2cbc1ef6e15f01229081b51d0f74b0d88984f05`.
The owner can recover the previous installation from that backup.

## Owner full-screen acceptance checklist

1. Restart with the complete original eleven-mod stack and verify the final
   candidate informational identity in KMG's log. Start character creation through
   your ordinary entry point (the entry point used for the original failure is
   especially useful).
2. For each Ifrit, Oread, Sylph and Undine, select a class and confirm the racial
   Heritage section appears before ability allocation and skills. Each heritage
   list must show exactly General and its two named alternatives: Lavasoul/Sunsoul,
   Gemsoul/Ironsoul, Smokesoul/Stormsoul, or Mistsoul/Rimesoul, respectively.
3. Confirm alternate racial choices appear in the racial route. Every applicable
   slot has a legal retain-base choice and the implemented compatible choices.
   Treacherous Earth and Nereid Fascination must be absent. Suggested independent
   examples already used in native tests: Ifrit Wildfire Heart + Fire in the Blood
   + Efreeti Magic; Oread Granite Skin + Earth Insight; Sylph Thunderous Resilience
   + Breeze-Kissed + Whispering Wind; Undine Acid Breath (then Ooze Breath on a
   second choice pass). Retain any remaining required slots.
4. Go back, change heritage, and go forward. Check the displayed racial modifiers
   and descriptions change once and your point-buy/rolled baseline remains yours.
   Check a multi-slot choice and then retain base or another legal combination;
   every required step must remain completable.
5. Complete abilities, skills and feats. Elemental Strike and other legal existing
   elemental feats must remain available. Heritage/alternate-racial choices must
   not recur as generic Abilities or ordinary global Traits.
6. In the later global Trait phase, confirm both selections contain categories and
   real choices. Choose two legal traits (different categories make this easy).
   With Bodyguard enabled, Helpful should occur once in Combat. A separate restart
   with Bodyguard disabled should remove Helpful while keeping ordinary Traits
   functional; return the setting to the original state afterward.
7. Complete normal details and finish one disposable character per elemental race.
   Include at least one alternate heritage and an alternate racial-trait combination.
   Repeat one route using Dice Roller with your ordinary Bag of Tricks settings.
8. On a disposable character, spend a racial daily resource, save/reload, and respec
   heritage/trait choices. Confirm the spent use stays spent until ordinary rest,
   then returns once; confirm original and replacement facts/stats do not duplicate.

Report which exact race, class, heritage, trait combination and entry point failed
if any phase is empty or cannot advance. Screenshots can support the visual review;
they do not replace the structured mechanical or completed-character evidence.


## Exact starting environment and optional mods

Windows 10 build 19045; Kingmaker 2.1.7b; UMM 0.32.4. The original output log
was preserved before any launch (SHA-256
`948d59a90bc88669e53a1c9ca4bfe2b700527403a4ed80f55a752047d795d5e8`).
Game assembly SHA-256:
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`;
UMM SHA-256:
`1387468bc3af41c50fe51859a3bb7af4922891aa8f13a6187e7a348ceaabfd88`.

| Installed mod | Version | Original DLL SHA-256 |
| --- | --- | --- |
| BagOfTricks | 1.16.4 | `d03626594ece0f339aeef03ec7259684af7ddeb8b2edf41936f144848059a0e6` |
| CallOfTheWild | 1.14.4c-2.1 | `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915` |
| CheatMenu | 1.2.3 | `7d659eb092073ab9e059414f8bfbdfe46991cede56b394f58f7ea2bcc1ff0845` |
| CraftMagicItems | 2.1.0 | `4ae2da61470350b31beef162717a604c9ccd322f66193917944ea4a9596e392d` |
| KingmakerBuffPlanner | 0.0.16 | `4c7249ad7a953522ea755e8bb46c5b89b136b9479a7360440526f417fb171597` |
| KingmakerDiceRoller | 0.1.2 | `962d5968d5021db2868d39104b4cbfc3911fe1de00ebde85974a1a2b1b977acd` |
| KingmakerGunslinger | 0.0.117 | `b3839a63fb83a5894169fa7ea2cbc1ef6e15f01229081b51d0f74b0d88984f05` |
| KingmakerLastAzlantiPreserver | 0.1.0 | `89dc72c1b331a818a7e6112b5c1d6b5d8d51d8027d74e1bc3d3168fb653ada23` |
| RacesUnleashed | 1.0.11 | `6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0` |
| TweakOrTreat | 1.1.0 | `a518324e15632aba46d6c467b156a31e9afd282e9827dee3e79ad14673852b92` |
| ZFavoredClass | 1.3.1 | `dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c` |

The original KMG row identifies the failed starting installation. The replacement
KMG identity is the exact stabilized artifact table above; optional DLLs are
unchanged. Per-profile full manifests, settings hashes, active selector snapshots
and exact restoration records remain in the ignored stabilization evidence
directory. No raw logs, generated ZIPs, saves or proprietary assemblies are
committed. All earlier journal evidence remains intact.
