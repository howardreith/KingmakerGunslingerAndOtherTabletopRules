# Icon overhaul v2 journal

## Native selected-fact diagnostics and narrower scrolling

The first native compile failed because `CharBPhaseTotal` is a phase object, not a
Component. The corrected source obtains its existing `CharBNewAbilities` component.
Build 2 passed repository validation, eleven capture fixtures, all 1,629 domain
tests, clean exact-reference Release and strict 224-file packaging. Its diagnostic
MVID was `aed9dfbc-efe3-41cd-8b97-0ef34c4497f4`.

The no-save Ifrit/Gunslinger run in `20260913T1526405323582Z-disposable-elemental-character-creation-case`
found the selected Weapon Focus/Pistol fact, but failed viewport qualification:
row Y `-796.9735:-732.9718` against `-401:0`. It is not a visual PASS. The capture
had searched only Unity `ScrollRect`; local native inspection also established
Kingmaker's separate `ScrollRectExtended` and its normal `ScrollToRectCenter` API.
The next candidate recognizes the nearest eligible implementation and retains
diagnostic bounds/images even on failure. A twelfth corruption fixture covers
that exact API/component and rejects offscreen rows. The original installation
was independently restored to all 136 paths/hashes before source work resumed.

The same bounded extension adds actual sheet capture before the existing rolled
Gunslinger mercenary regression's cleanup. Normal legal choices cover P/M/B across
its three actors, with Undine covering Rapid Reload. The actual service window,
native facts and prior UI/world context are checked; no active-party enrollment
or save write is introduced. This new candidate still requires full build/runtime
qualification. Raw diagnostic artifacts and the first candidate are local only.

Build 3 caught a missing explicit project Compile item for the new sheet partial.
Inspection also found that the standalone fact-slot adapter was absent from the
project in build 2; that diagnostic DLL therefore never contained the new adapter.
Neither build is claimed as fact-slot qualification. Both new files are now
explicitly included. A focused catalog corruption test rejects a cataloged icon
authority omitted from the compiled project (23 catalog tests pass), and a direct
exact-reference compile passes before the next complete Build-Local gate.

Build 4 passed all full gates with 23 catalog, twelve screenshot, six paired and
eleven request cases, 1,629 domain tests and a 224-file package. MVID
`0f2c464e-4186-45e2-abc2-10641f059fb6`, package
`2ec80c6a825b01abc0bbd1c466419b044518aba4b54c780c85c9fc0ac3527787`.
Both `20260913T1544156567190Z-disposable-elemental-character-creation-case`
and `20260913T1549264498462Z-working-save-elemental-character-creation-regression`
failed the same Total viewport condition. The actual selected fact reports native
P, Saber_Dist32, no overflow/truncation, exact background/fact/parameter/fallback,
and nineteen unchanged control facts. The actual component is Unity ScrollRect,
not ScrollRectExtended; content height is 862.030151 at scroll Y zero while the
target remains `-796.9735:-732.9718` below a `-401:0` viewport. The second route
stopped before its first native commit/sheet, so neither is claimed as qualified.
Both images retain the real scrolled list. All temporary package/backup bytes
were audited and the original 136-file installation was restored exactly.
The next bounded observation uses Unity's existing immediate layout rebuild and
records actual parent geometry; no custom dimensions or row relocation are added.

Build 5 passed all full gates (MVID `398d03c0-7017-4f69-9481-051e12a22c93`).
Its `20260913T1559574501116Z-disposable-elemental-character-creation-case` run
captured a visible native Pistol selector, but failed the normalized scroll
restoration check before Total. Original normalized X was `3.0420253e-07`, capture
X was `1`, while content height differed by only about 0.00025 native units.
The next candidate limits the explicit layout rebuild to selected facts, restores
only enabled axes and verifies actual content position as well as active-axis
normalization and velocity. It records original/restored geometry; a thirteenth
corruption fixture rejects real displacement/non-finite values. This does not
retroactively qualify the failed run. All 224 package files, three known runtime
additions and 136 backup files were audited; the original installation was again
restored and independently verified before editing.

## 2026-09-13 first integrated native screens and static-menu correction

The owner-requested master integration built successfully with 1,629/1,629
domain cases, repository/asset validation, 22 catalog, six paired-control,
seven screenshot-corruption and eleven request-format checks, clean Release
and a validated 224-file package. The upstream compiled input-wrapper check
passed all ten synthetic cases. Hotfix production files match the merged master.
Package `665b4d7e16e4349282897ca61a82009fcec904ad3a52347f89897564171988fe`;
DLL `15b2f40c092f3905fd3ab358ef19c1e3d7a2e8f53d92aa2b57c76b6812679065`;
MVID `c827e94d-dd34-42dd-9688-9bd37c95dad2`; source fingerprint
`f06cc32b0bafbb04559516e742039df24614c2e51632e8ebaee34721b347592e`.

Four guarded disposable Gunslinger creator runs passed all eleven assertions
each, producing 28 Ifrit, 28 Oread, 28 Sylph and 24 Undine native 1280x720 frames.
Run IDs: `20260913T1300483227258Z-ff8b1c91c41c4d5f974a98f9ed1993dd`,
`20260913T1304592794269Z-3fd9b526a15d4fa69232792a665523fa`,
Sylph evidence directory `20260913T1307305696043Z-disposable-elemental-character-creation-case`,
and `20260913T1310045107346Z-f89160e501c7433992f511eb83436929`.
UMM's normal close/restoration succeeded in every run. Inspected the four
heritage menus and Ifrit Weapon Focus: paintings are legible, with the native
B beside unchanged native weapon-category lettering. P/M were outside that
Weapon Focus viewport. The save-free Ifrit final-review frame has an empty
central doll panel; these cancellation-fixture screenshots do not establish
character/model visibility qualification. Appearance assets and code are unchanged.

Undine's actual Rapid Reload submenu exposed old thin bitmap letters despite
the constructor data checks passing. Installed IL shows the static selection
Items getter returns raw BlueprintFeature objects; the parametrized/sheet data
constructor is not that menu route. Added an exact Rapid Reload Items adapter
that wraps only existing official entries with null-icon P/M/B data, preserving
order/count, names/descriptions, feature/parameter references, native filtering
and all blueprint fallback sprites. Extended the live data check to enumerate
the actual Items property, and the creator check to require actual active TMP
glyphs. No global icon getter/font change or new choice publication was made.

Spellbook run `20260913T1313187975453Z-14874036b1194029aa1b9494e9836017`
passed 31 assertions, ten native captures and the screenshot provenance check.
Inspected Teleport and Word of Recall in their native description windows;
art is correct. An incidental hover tip obscured unrelated book content.
Installed IL verifies `TooltipsController.SetTemporaryCooldown()` clears only
the hover constructor and starts the native 0.3-second delay. Capture now holds
that temporary cooldown while allowing real frames to settle; explicit
description windows stay open and no mouse/input or preference changes occur.

The creator's result uses the exact assembly identity plus MVID/PID, whereas
other scenarios use the bare identity. Corrected the screenshot validator to
accept only those two exact formats derived from the loaded-build record,
with a negative test for the wrong PID. Earlier first-artifact capture records
passed every other check after an exact checked in-memory normalization; no
raw evidence was altered.

Final working smoke passed all eleven checks in evidence directory
`20260913T1317415123047Z-working-save-smoke`. Audited all 224 package files,
three explained temporary additions and the exact backup before restoring;
independently verified all 136 original paths/hashes, including schema-10
settings. Preserved this first integrated artifact and raw evidence locally.
The static-menu and hover correction now awaits the next full build and live
qualification; the merge commit remains deferred. No final-art approval claim.

## 2026-09-13 requested master integration and native capture repair

The owner explicitly requested the newly merged Gunslinger update and allowed
rebase or equivalent integration. Fetched master
`9dc2b6301d97bc83540845240544d34b6fad4b48` (PR #18, version `0.0.128`).
Used a non-rewriting merge into `codex/icon-art-overhaul-v2`; no force-push,
master mutation or release. Preserved all 32 local changed/untracked files in
an ignored hashed archive and retained a named stash before applying them to
the merge. Git reported no textual conflicts. The merge commit is deferred
until combined qualification; the hotfix's production files remain unchanged.
The combined suite contains 1,629 cases (1,622 upstream plus seven icon cases).
The current validator count follows that suite; upstream's archived 0.0.127
static snapshots retain their original 1,612 counts.

Before integration, the screenshot-instrumented `0.0.127` candidate had package
`e964f280d7e7b7d6b39679099165077f42f7d0114ce102a738554dc1a8a5ff64`,
DLL `9f210b8ad29bbe974feeddaeec49b650f1824a79b953e528726738aa0e7f6a5b`,
MVID `0c8753f1-4306-4e8d-ba66-62f8d0f74b87` and source fingerprint
`eda0a522eb930c15d07799592ec0ff5bdc2050fcb3fab886ebe1718b48a1fdb8`.
The corrected Eastern-OFF observer passed all 42 assertions in
`20260913T0723397334492Z-888c4e1978724da49c86f835cec7e394`.
The real Ifrit creator passed 11 in
`20260913T0719277730303Z-c05aaa03283a4736b319b4db3352f224` and generated
28 completed native screenshots. Inspection found UMM's startup window covering
the creator; these frames are diagnostic only and do not qualify appearance.
The following working smoke passed all 11 checks (evidence directory
`20260913T0730584837400Z-working-save-smoke`). Audited all 224 temporary package
files, three explained additions and the exact backup before restoration;
independently verified all 136 original installation paths/hashes afterward.
The original schema-10 settings are restored; the game is closed.

Inspected installed UMM IL: its public `UI.ToggleWindow(false)` releases the
blocking Canvas and restores cursor state without a settings-save operation.
The capture helper now requires both settings indexes to be `-1` and the game
callback list empty before using that API, verifies closure across capture
frames, and restores the original open state during fixture cleanup. Unexpected
UI state fails closed. New negative evidence checks reject the earlier
unobserved overlay, wrong run/artifact, incomplete/modified PNGs, unsafe names,
unfinished frames and missing restoration. Native appearance and owner approval
remain separate from those checks. This implementation awaits the combined
build and guarded live qualification.

The first integrated validation attempt rejected the focused icon request
fixture's stale literal `0.0.127` before compilation or deployment. The fixture
now reads the active version from repository metadata while retaining every
invalid-request test and the strict production preflight. This prevents future
baseline updates from making the focused request suite test an obsolete version.

## 2026-09-13 first firearm artifact and native capture preparation

Installed the approved Rapid Reload export (SHA-256
`5c3280145815f7b161602da161264ac1f43fb3ac3c4bf47ff907b0919a2aa9d3`),
archived the rejected old bytes, retired the extra chroma generator and updated
only its historical validation delegation. Rewrote the stale firearm icon map.
The first domain attempt stopped on a historical source token for the replaced
facsimile fallback call; corrected that focused test. Build-Local then passed
all 1,619 domain cases, repository/catalog checks, clean Release and 224-file
packaging. Source state `66fa87af6a3d1212951efbad17d889f9c408e2a191beab346e72764c68aec483`;
package `77712c80249376424e8ed38e00f31b236fbe9522b090270fc09d80ee32438fa3`;
DLL `5d8b95a292751214bc617d0157d6fc8cb841f951da2be354b295ad77025a2c7f`;
MVID `9f8e380e-6be7-40b6-b408-924a1b919a38`.

Candidate `20260913T0651329184269Z-d4ca17e5fc214129bf6a04dbca78699f`
and control `20260913T0653551492970Z-edbb11e706824333a12a79d5903ff0fb`
passed 27 assertions each; the paired comparison passed all 90 exports and
protected transitions/graphs. All 15 native parameter entries, three Rapid
children, ten hidden legacy controls and 395 other native category entries
retained the exact required data. Dependent firearm feat run
`20260913T0656221324919Z-c808bc789da84a808caef91621a4c7e3` passed 12 assertions;
Gunslinger-OFF `20260913T0659045021475Z-b4656459da9e4133b378ef070aa963cc`
passed 42. None of these results is native rendered-screen acceptance.

Eastern-OFF `20260913T0701467723675Z-7cdcec22175246479589996805548315`
failed one existing vendor-count assertion; every monogram assertion passed.
The observer still expected 12 capital rows and six rows in every DLC table.
Inspected production explicitly publishes ten capital rows, six Honest Guy
equipment rows and four Xelliren supply rows, excluding retired maintenance
kits. Corrected only the observer and added an explicit retired-row absence
check. This failure remains excluded from qualification pending a new build/run.

Working smoke `20260913T0705478323884Z-fe58d070450745c4ac2a9a531f10f2f0`
passed all 11 assertions. Every temporary package file,
settings/previous-settings file, loader cache and backup was audited before
restoration; all 136 original installed paths/hashes then matched exactly.
The intermediate restoration audit correctly rejected the OFF-profile previous
settings sidecar; the final all-ON native startup rotated it to the expected
all-ON bytes before restoration. First artifact and raw audit copies are retained
under ignored `artifacts/icon-overhaul-v2/firearm-first-*`.

Read only the exactly identified working-save archive: its header and SHA-256
`20fefb4195729c369c1d3b2624aaf4a61b43bd112a417c55a66f16e37323b46b`
matched, and all eight JSON members contain zero P/M/B parameter GUIDs. This is
a missing saved-fact fixture, not a parameter round-trip PASS. No save changed;
new save creation/writes remain a separate permission gate.

Added request-only native screenshot holds to the existing creator and spellbook
fixtures, with stable ownership, actual rendered rows/TMP data, completed PNG
hashes and explicit native evidence labels. These latest additions and the
observer correction are not yet built or runtime-qualified. Inventory/merchant,
strategic, action/buff screens and final owner approval remain outstanding.

## 2026-09-13 firearm presentation in progress

Published painted checkpoint `84c2ed9ed25e244e9cdb3e9e689ae72db3619a9f`
through the unchanged required wrapper; the worktree was clean afterward.
Inspected the installed FeatureUIData and UIFeature constructor chains. Added
a two-argument constructor postfix restricted to exact integrated native feat
roots with official saved blueprint parameters and the three registered Rapid
Reload children. It changes only the UI object's Icon and NameForAcronim.
Blueprint sprites, names, descriptions, parameters and publication flags remain
outside this setter. No Gunslinger-ON gate is used for already saved facts.

Added a pure scope test (expected domain total 1,619), retained explicitly labeled
blueprint-sprite facsimiles through a direct fallback reader, and prepared live
native data-construction checks for five feat families, Rapid Reload, hidden
legacy controls and native/eastern categories. Real rendered UI is still a
separate gate. These new source changes are not yet built or runtime-qualified.
Next: install the approved Rapid Reload export while preserving the rejected
legacy reference, retire its extra generator, update narrow legacy validation,
then qualify the complete firearm presentation change and capture native UI.

## 2026-09-13 painted integration qualified

The clean rebuild after the request serializer fix passed repository validation,
22 catalog tests, six paired-evidence corruption tests, eleven preflight/JSON
checks, all 1,618 domain tests, clean Release and strict 224-file packaging.
Package, DLL and MVID were byte-identical to the completed candidate artifact.

Candidate `20260913T0603143949131Z-84ec6416ad0542c9994853f5f3836310` and true
control `20260913T0610239313301Z-34c03d7fd6cd4bc0b8a01381e74a2ddd` passed 19
assertions each. The independent paired comparison passed all 284 identities,
137 mappings/89 exports, 28 appearance resources, reserved absence, 21 native
reuse consumers, 35,756 protected transitions and 255 owned graphs/components.
Late foreign icon fills and strategic spell-list additions match the no-mapping
control exactly. The earlier false-control launch is excluded from this pair.

Working-save smoke `20260913T0613155395530Z-86c16f682ba344d3be1865ff1dc6e484`
passed eleven assertions, including exact working/baseline distinction, load
correlation, stable three-member party fingerprint and no save-writing API.
All processes exited normally. Audited the exact package, runtime additions and
backup, then restored and verified all 136 original installation files.

`reports/icon-overhaul/PAINTED-INTEGRATION-QUALIFICATION.json` records exact hashes,
raw-evidence hashes, run IDs, source fingerprint limits and remaining gates.
The catalog now identifies the current owned painting source and retains each
consumer's original donor in `baselineArt`. Native screen evidence, complete
firearm typography/Rapid Reload, saved parameter observation and final owner
approval remain. Continue after this coherent checkpoint; no merge/release.

## 2026-09-13 painted integration qualification in progress

Paired-run preparation found a request-writer defect: although preflight accepted
`iconCensusControl=true`, the serializer emitted empty parameters. Run
`20260913T0600449731517Z-b00e4c4abc2c432dae47c7f7970c7906` therefore reports
`controlRun=false`; its 19/19 assertions are a normal-mapping result, not a control.
The mandatory independent gate caught the mode mismatch. Normal candidate run
`20260913T0603143949131Z-84ec6416ad0542c9994853f5f3836310` also passed 19/19.
Neither alone qualifies late transitions. Fixed the serializer and added actual
request JSON round trips to the focused test (eleven cases). No C# source or
runtime pixels changed for this fix. A rebuild must establish artifact equality
before either prior candidate may be paired with the corrected control.

The pre-fix package SHA-256 is
`952462ca1eb766404e8b21d493b5e4d31964963c921157c2041bef810cc9b9d5`;
DLL SHA-256 `a5f518be3a77ddc9df1431e9d7ac5f411fba873ecf9809884bf7e952ea062e33`;
MVID `31c95e82-1b7e-4b3d-85eb-10f362e59cfb`. Both processes exited normally.
All 224 temporary package files, three additions and the backup passed audit;
all 136 original installation files were restored exactly before the writer fix.

Fifth run `20260913T0544231563345Z-22ea2b0f811c4c51b7c0d3c1816d9458`
completed the full census: 18/20 assertions passed. All 137 mapped consumers,
89 installed exports, 28 appearance resources, 21 native-reuse entries and the
35,756 other observed icon references passed the immediate mapping comparison.
Late comparison flagged three initially null Duergar icons and appended
SpellListComponents on the three strategic spells. Nothing was accepted merely
because a foreign module appears responsible. Added a same-artifact, no-save
control request that suppresses only owned mapping, plus a mandatory independent
paired comparison of all protected transitions and owned graphs/components.
No per-mod/identity exception or late graph equality claim was introduced.

Fifth package SHA-256:
`346205101e9e22b532debdad9c5cc92e67d8ef2a73139224e33db90e69a43aa2`;
DLL SHA-256 `ec82e7a74602b53837f3e7781dda841d856575bdfa9ef36c7bb77796bd7073dc`;
MVID `82e13a71-0779-499f-ba53-6e58e21d089a`. Full build passed 1,617 domain
tests. All temporary files and the backup were audited, then all 136 original
files were restored exactly. The complete raw failed census stays local.

Six paired-evidence corruption tests and nine current-version control preflight
cases pass. The broad historical Test-RuntimeScenarioPreflight script stops at
its stale ExpectedVersion fixture before these new cases; it remains unchanged.
The focused current-version script is part of repository validation. The new
domain request-scope test brings the required suite to 1,618; replacement build
and paired guarded runs are next.

Fourth run `20260913T0529122126617Z-e425b92ef7584b808a0c12e806f2baba`
reported ERROR because the observer's eight-level action depth limit was reached.
Production initialization was preserved. Replaced recursive scanning with an
iterative, reference-aware reader that records cycles/shared edges and keeps a
4,096-node budget with exact failure paths. Added five executable domain cases
for nested actions, topology/identity changes and traversal boundaries.

Fourth package SHA-256:
`0e22e20ddafa377bab83849d9014eedb3a78584803e5c26e36cfef69567d7362`;
DLL SHA-256 `0b6222bf8eda19259d6282593c2dd16c3b1547334683369467aec9f12d27b890`;
MVID `68d8576f-6aa0-4fc4-9685-7a17d485beea`. Build passed 22 focused and 1,612
domain tests before this live failure. Exact package, three runtime additions
and backup passed the restoration audit; all 136 original files were restored.
The existing window helper successfully captured the native loading screen from
the exact foreground game process. This establishes capture capability only;
it qualifies no icon UI or gameplay behavior. Raw capture/logs stay outside Git.

Added 89 byte-exact painted runtime exports and an explicit table for 137 owned
consumers. The guarded icon scenario now compares the 284-consumer census,
direct graphs/components, native reuse and protected sprite references before
and after mapping. No painted production candidate gained owner approval.

The first complete workflow exposed two obsolete package-count expectations in
existing domain tests. Updated those expectations and the deterministic package
allowlist for the additional 89 files. The next workflow passed all 19 focused
catalog tests, 1,612 domain tests, clean Release and strict 224-file packaging.

The first guarded Steam run `20260913T0449323791107Z-ff00393bb10f404e911488bff1dc4522`
returned ERROR: the census had revalidated an already claimed request and was
correctly rejected by replay protection. No assignment qualification is claimed.
Failed candidate package SHA-256:
`49267b20e275261dbf05cddcd49ff789c0e3c056633cd5d754672e2c73578585`;
DLL SHA-256 `f5e976d8beee26a1739d872af60498f4870ebb666f0b120662dea90ce14287d2`;
MVID `5d56dda5-b481-41b2-861d-70b88cb7f311`.

Changed only census setup to receive the runner's accepted, exclusively claimed
request, following the existing observer lifecycle. Replay checks remain intact.
The runtime regression and full workflow must pass on the replacement artifact.

Before restoration, verified all 224 installed package files, hashed and checked
the two schema-11 all-ON settings files and exact DLL loader cache, and matched
the backup against all 136 original files. Native UMM shutdown rotated the
already migrated settings into `.previous`; its exact hash was recorded this
time. Restored original 0.0.117 and verified 136/136 original paths and hashes.
Raw evidence and audits remain local and uncommitted.

The replacement artifact passed the same full workflow, then run
`20260913T0501342793009Z-68981fd299b747458082bcf8daa19de3` timed out after a
bootstrap census error. The manifest contains 283 active consumers and one
reserved `KMG.ElementalRaces.Diagnostics.ProbeRace`; the census incorrectly
required that separate request-local probe to exist. Its source and manifest
explicitly require ordinary absence. Corrected the catalog's presence contract,
retained its inventory row as an asserted absence, and added two corruption
tests so active consumers cannot use that exception. Observation errors now
reach the scenario result without rolling back production bootstrap.

Second failed package SHA-256:
`2059c190750b4c8403126a5343482c4d604d69749d865507b3d6217e306ab85e`;
DLL SHA-256 `6d54233a03187e34e00216a9fd3583af1b4cb33e14bcc79b13c247c3e3ed8017`;
MVID `c56b558c-1603-4ed9-8423-a18c6b753f52`. Preserved the root-cause log locally.
The resulting bootstrap rollback also logged a refused protection-description
rollback; this failed diagnostic run qualifies no gameplay or art behavior.
The process exited normally, all temporary files and backup hashes passed the
audit, and restoration again matched all 136 original paths and hashes.

The third run `20260913T0515089552316Z-b6429b5917b04d1f87abddf9407947f8`
reported ERROR directly while preserving production initialization: the first
appearance `EquipmentEntity` was absent from the blueprint library because all
28 such proxies are registered in the separate native resource cache. Audited
every planned type and that registry together. The corrected census covers 255
blueprints, 28 read-only cached appearance references and one reserved absence;
missing/type-invalid entries are collected together for diagnosis. No resource
loader, retention mutation, appearance change or invented fixture was added.

Third failed package SHA-256:
`51d6c580bb1cf3dca3e98f6113a689bdd8f4dce674c5cca058d70e06beba1737`;
DLL SHA-256 `7f162503ab1dc3dad51c5f25a5c98f645fdae8de10aff8abe63491703f4a71c7`;
MVID `716550b3-b1be-4704-a429-30dc05968c96`. Normal exit and restoration verified
all 136 original files again. An optional window-capability check reached the
already exited PID and captured nothing; native UI remains unqualified.

All 22 focused catalog tests pass. The in-memory corruption fixtures now reuse
the actual immutable disk inspection results within their own process, reducing
repeated PNG decoding/path scans; every altered-document comparison still runs,
and ordinary repository validation remains uncached. Full source/runtime
qualification must pass on the next artifact.

## 2026-09-13 complete art candidates (UTC)

Completed the remaining 55 paintings and exports, bringing the collection to 89
painted identities plus the approved Rapid Reload emblem. All 80 production
briefs include implemented behavior, actual UI surfaces, subject/silhouette,
confusion comparisons, forbidden interpretations and exact tool provenance.
Revised Inner Flame to retain its complete weapon and Breeze-Kissed to preserve a
straight arrow redirected by air; retained both earlier originals and prompts.
The Acid Breath brief records an initial rejected tool request with no output
and the successful benign symbolic retry. All approved pilot pixels are unchanged.

Inspected twelve family sheets at 32/48/64px and in grayscale. Built one searchable
90-image review with original/export/brief links and separate approval states.
Repeat export preserved 187 PNG/manifest files exactly; production manifest SHA-256
is dfe4e008edb1770cc21ad2f14769202a3b5f99620ea95f39394dfb0370d349e6.
Gallery links, unique records, approval count and JavaScript syntax passed.

Build-Local exited 0 after repository validation, 16 focused catalog tests, all
1,612 domain tests, clean Release and strict output/package checks. Exact artifact
identity is in reports/icon-overhaul/PRODUCTION-QUALIFICATION.json. Package SHA-256:
f452387b66753e6d04dd31b6ab5e9f3ac9a120bea523f858242c753a550560b4.
No installed files, runtime icons or game sources changed in this art checkpoint;
no new runtime/native-UI claim. Existing installation remains outside this work.

Next: publish through the required wrapper, integrate the 137 exact painted
consumers with live before/after donor/protection evidence, then finish native
firearm presentation and final game-screen qualification. Final owner approval
of production art is still required; the mission is not complete.

## 2026-09-12 pilot approved; first production families

The owner's response, "All looks beautiful, please continue", approved the ten
reviewed pilot images and family direction. Bound the decision to their unchanged
export hashes in `reports/icon-overhaul/PILOT-APPROVAL.md` and both catalog/manifest.
No final approval of unseen art or native UI was inferred.

Created 25 individual production originals: eleven remaining heritages, three
resistances and eleven affinities. Their exact prompts and implementation
references are preserved beside the source/export manifest. Reviewed three
labeled contact sheets at 32/48/64px and in grayscale. Corrected Metal Affinity's
misleading orange fire effects through a separate image edit; retained the first
draft and its complete prompt. All approved pilot bytes remain unchanged.

Extended the canonical validator to production sources/briefs/revisions and
separate approval records. All 13 focused corruption tests passed. Repeated GDI+
export reproduced the 25-record manifest hash
`3fe9b5b0cf4841da7120e6a23fb904c601ca1f65157b302b22617b6fcc56bfb6`.
No game source, runtime export or installed file changed in this artwork phase.
Native consumer census and remaining families continue after this checkpoint.

Build-Local completed with exit 0 after directing ordinary Python unittest
progress to stdout. The initial outer PowerShell log redirection reported
NativeCommandError for passing stderr progress despite completed inner gates;
the corrected run passed repository validation, 13 focused tests, all 1,612
domain tests, clean Release, output and strict package validation. Package SHA-256:
`670b08bcb3d54a81f8d5e405cf347de1a8171ee098843108641dd22514aeaf08`;
DLL SHA-256: `b6309c346be3719c021932bf77345f440716df484befb99ae182d1ba45589358`.
Raw logs remain ignored in `artifacts/icon-overhaul-v2/production-foundations-build-verified.log`.
No runtime binding/PNG changed, so no new runtime claim is made. The owner added
the branch allowlist entry; the required wrapper published pilot commit
`4f141921a830042442b617f2788f1cd0b34a7cb6` successfully.

## 2026-09-12 intake and pilot

Read the complete supplied mission, plan, screenshot index and guide seed, root instructions, current icon factories/loaders/manifests and build/runtime entry points. Verified the supplied ZIP and every member checksum. Preserved the previous branch and started from clean current master on `codex/icon-art-overhaul-v2`.

Inspected actual protected project art and supplied native screenshots. Executed the built-in image tool; preserved individual originals rather than a generated grid. Began a source census and exact installed-assembly inspection without running game code. The native selector uses TMP text when its explicit sprite is null; reconstructed selected feats read the parameter blueprint sprite separately.

At intake: no owner reviews for new pixels or native UI qualification. Publication needs the new branch allowlisted by the owner. No installation changes had yet been made.

## 2026-09-12 qualified pilot checkpoint

Completed ten isolated candidates and the review page/contact sheet. Revised Fire Affinity for hand readability and Greater Teleport to remove UI-like brackets. Preserved source paintings and edit inputs; repeat export matched the exact manifest hash. Added the permanent guide, actual reference index, 284-consumer catalog, fifteen UI entries, 117 protected-file hashes and nine focused corruption tests. The source-only constructor delta is checked against the pre-change whole file; no mechanics or identity change is permitted by that exception.

Build-Local passed repository validation, all 1,612 domain tests, clean Release compilation and installable-package checks. Earlier validation attempts exposed a Python import cache and an obsolete whole-file lock; both were addressed without dropping their protection intent. The final executing C# sources remained unchanged through qualification.

Four serialized guarded Steam launches passed 66 assertions: live icon references/facsimiles, actual firearm menus/selection/effects, Gunslinger OFF with other modules ON, and exact KMG_AUTOMATION_WORKING load. No new pixels were installed and no save writes were authorized. Runtime entry/parameter evidence does not establish native typography, character-sheet appearance or a parameter-specific disk round trip.

Restored original 0.0.117 installation and verified 136/136 paths and hashes, including original settings. The report records the extra-file precheck caveat involving the runtime-created FeatureModules.json.previous file; its temporary contents were not independently hashed before restoration. All final original files were verified afterward.

Ran the exact push wrapper; it rejected codex/icon-art-overhaul-v2 because the branch is not allowlisted. No bypass or policy edit. State is AWAITING_PILOT_APPROVAL. Next work requires the owner's actual-image decision, bounded supervised native UI inspection, and then approved family production. Full integration and final visual acceptance remain outstanding. Exact commands, artifact hashes, run IDs, limits and restoration evidence are in reports/icon-overhaul/IMPLEMENTATION-REPORT.md and PILOT-QUALIFICATION.json.

## 2026-09-13 — corrected native presentation qualified on current master

Integrated owner-requested master `9dc2b6301d97bc83540845240544d34b6fad4b48`
(0.0.128) with published icon parent `84c2ed9` using an ordinary feature-branch
merge. No history rewrite, original-branch overwrite, feature-to-master merge or
release. All hotfix production Actions/Firing/Recovery/Gunsmithing files match
that upstream commit. The pre-integration stash and independent worktree archive
remain preserved. The current-version icon request test derives Info.json's version;
archived validation counters retain upstream 1,612 and active 0.0.128 uses 1,629.

The corrected artifact (source `06070cfcaafa950c427cc04aeea5f2d4fb1a643053c7ef25421d5a8964017ccc`,
DLL `037ffbd4314ad2f07f0acb916c73476ecbd62b6c8f8bdf238b7b6020b388998c`,
MVID `16ab23f5-129d-415f-9e57-e597fe7a46ae`) passed all build/package gates.
Nine guarded runs passed: Undine 11; mercenary 12; candidate/control 27 each;
dependent feats 12; Gunslinger OFF 42; Eastern OFF 43; spellbook 31; smoke 11.
Exact run IDs and hashes are in `reports/icon-overhaul/NATIVE-128-QUALIFICATION.json`.
The paired census preserves 35,756 protected transitions and 255 owned graphs.
426 all-ON/Gunslinger-OFF and 411 Eastern-OFF firearm data/control records are exact.

Current native screenshots total 116 and pass the independent provenance validator.
Undine021 shows all three genuine native P/M/B Rapid Reload glyphs; this corrects
the first candidate's raw static Items bypass. Mercenary002 has distinct heritage
art in its clear left pane; an unrelated right hover panel remains. Mercenary026
shows protected actions but an empty central doll, so no appearance PASS is claimed.
Spellbook000/002/006 show Teleport/Greater Teleport/Word of Recall correctly framed,
without hover occlusion. The local native gallery links originals; it is not a
substitute for remaining UI coverage or owner approval.

After final smoke, all 224 installed package files, three known runtime additions
and 136 backup files passed the restoration audit. The exact original 136-file
0.0.117 installation and schema-10 settings were independently restored at
2026-09-13 14:00 UTC. No save writes were observed. Candidate package/manifest/audit
are preserved under ignored `native-qualified-128-*`. Catalog curation now labels
six active firearm parameter/child consumers native-monogram and twelve retained
legacy wrappers protected-existing; it does not silently publish or repaint them.

The next native work is bounded viewport inspection of existing weapon/eastern
rows and capture of the existing cancel-only strategic spell learning fixture.
Racial action/buff, character-sheet, scroll and strategic control views remain.
The working-save firearm-parameter prerequisite and final visual approval are
explicit pending gates; no new save/write authorization is inferred.

## 2026-09-13 — native viewport and learning evidence

Published the current-master integration as `75017b3e039a16dbfd44b9212e4983de53be8249`
through the required wrapper. A subsequent bounded capture extension preserves
native ScrollRect position/velocity while holding one exact row in its viewport.
It also captures the existing cancel-only Wizard/Sorcerer learning fixture.

The first viewport artifact passed repository validation, 1,629 domain tests,
clean Release and 224-file package validation. Source hash:
`f7d8ab0069b4c8b7f0c6d66b00e8515858777690df1ea971d4691f6416c0e85c`;
DLL `170899e4358521fcff79d938e83b4d966f72b8039fb561028da20a5e3fad6c09`;
MVID `39c076c1-95a4-4c7d-ae5d-2565b86560f2`;
package `83889890c46e5688b19244c4d205b95481060e9f3a37dd257955e21a1a930415`.
Its Ifrit/Gunslinger creator failed with `Sequence contains no matching element`
while assembling capture targets: P/M/B and Nodachi were present, but untrained
exotic categories were correctly absent. The 26 diagnostic frames do not qualify
that run. The correction uses the existing Fighter case's two native feat choices
to learn an exotic proficiency normally before choosing Weapon Focus. No fact
grant, prerequisite change or new runtime request parameter is introduced.

The same first artifact passed all 31 native learning checks, run
`20260913T1418382766746Z-6b9086d6167d4c0181efe771084b036b`, with four completed
1600x900 captures for Wizard/Sorcerer Teleport/Greater Teleport. Provenance validation
passed; inspected Wizard Teleport is clear and centered in the real learning list.
Every viewport was restored. This is an isolated preview/cancel test, not a level
commit or saved spell-learning claim. All installed package/backup files were
audited and all 136 original live files independently restored afterward. The
first package, build manifest and restoration audit are retained as ignored
`viewport-first-*` artifacts. Neither diagnostic nor partial PASS replaces the
preceding nine-run qualification record.

The first revised Fighter build passed all 1,629 domain cases but failed exact
native compilation because this partial file had no `UnitDescriptor.HasFact`
extension import. It was not packaged or deployed. The check now uses the native
`Progression.Features.HasFact` collection method verified in the installed
selection implementation; the complete build gates are repeated after that fix.

The corrected viewport candidate passed all build gates and six guarded Steam
runs: four creator cases (11 each), learning (31) and working smoke (11), totaling
86 assertions. Source `600da30a7d2636ffd427e3f7df6efab527383bf0da1ecc4532c653b4fd6a5696`,
DLL `b5ebd760f91f8e8657705b3403fbdf107ab5e3ad539f65ce1c106c9ba39b8df7`,
MVID `d6adc4c1-20ed-4435-baab-e863f39d1fa6`, package
`f10fbb0db787bbf23d3ef23f825e5d98ca661db95155ff3fcb646413fcd500e6`.
The exact runs, hashes, ten weapon targets and four learning targets are in
`reports/icon-overhaul/NATIVE-VIEWPORT-QUALIFICATION.json`. All five native capture
manifests passed provenance validation; there are 138 unmodified PNGs. Inspected
P/M/B, NO, WK, KA and EB are clear; WK's native 32-point Saber_Dist32 text reports
no overflow/truncation at 1280x720. No eastern/spear lettering code was changed.
All four current Wizard/Sorcerer learning targets are clear and unobscured.

The immediate smoke preflight correctly stopped while the successful learning
process was still shutting down. It exited normally; smoke was launched separately
after read-only process verification. No force-kill, concurrent launch or guard
bypass occurred. All 224 temporary package files, three fully explained additions
and 136 backup files were audited. At 14:59:42 UTC, independent verification matched
all 136 original 0.0.117 installation paths/hashes and original settings. The game
is closed and the candidate package/manifest/audit are archived locally.

Native IL inspection found that the real selected-fact/character-sheet slot reads
Fact.Icon directly, bypassing the previously qualified FeatureUIData constructor.
The visible Total capture does not expose the below-fold firearm feat. Next work
will qualify this exact native component and its P/M/B text route; the original
fact/parameter/sprite and all other rows must remain intact. Full racial feat,
action/variant/buff, strategic-control and scroll views remain pending. No final
owner approval or save-writing authority is inferred from these technical passes.

## 2026-09-13 — selected-fact consumer extension started

Committed and pushed the viewport qualification as `32be894c920a44390dd820c04be4f55a5c6877a2`.
Read the guide, reference index, catalog, current native implementation and installed
IL before extending presentation. `CharSComponentAbilitySlot.SetFeature(Feature)`
reads Fact.Icon. Separately, CharBNewAbilities receives actual Feature objects from
LevelUpTotalStats.BuildDifference and calls SetData(IUIDataProvider). Neither path
constructs FeatureUIData itself. The working adapter targets only these exact
overloads, matches the existing five integrated roots/three official parameters
or three Rapid Reload children, and uses native SetIcon/TMP. It preserves the
original border/mask call sequence and never changes facts, names, parameters,
ranks, blueprint sprites, fonts or unrelated rows.

The working Total capture correlates the actual Feature owner/reference and
parameter, reveals its row, checks the real glyph/background and requires preserved
non-firearm control icons. Incorrect presentation keeps a diagnostic screenshot
before failing. Eleven focused provenance/corruption tests pass, including rejection
of a constructor-only-looking or unrelated selected-fact record. Full build/runtime
qualification of this extension is still pending. No game is running or candidate
installed at this preparation checkpoint; broader sheet/menu evidence remains open.

Build 6 passed all build gates (23 catalog, 13 screenshot, 1,629 domain and 224 package files), but run `20260913T1611037093503Z-disposable-elemental-character-creation-case` remains FAIL. All four native weapon rows restored their actual content position correctly. The final Total row had correct P/native font/background and 19 unchanged control icons, with exact scroll restoration; it remained outside the viewport. Its Content height was 862.03 despite a native preferred height of 1,461, while the Feats grid began at -1,130. Rebuilding layout alone did not change this. The diagnostic package/DLL/MVID are retained in `native-fact-slot-fourth-*`; all 136 original live files were independently verified restored at 16:22:34 UTC.

The next bounded candidate uses native PreferredSize only for a firearm-containing Total list whose enabled fitter is actually MinSize, and restores the prior mode on hide/disable/refill. Runtime metadata records the actual before/current mode; a different mode causes no production override. The sheet probe checks Total mode restoration after native commit. No icon pixels, fact identity, saved parameter, prerequisite, font or row position changes. This is unqualified work until its build and guarded native checks pass.

Build 7 passed 23 catalog, 14 screenshot, 1,629 domain and 224-file package checks. Guarded no-save run `20260913T1628481531961Z-disposable-elemental-character-creation-case` still FAIL: the exact native Total fitter was observed as PreferredSize but disabled, so the conditional MinSize adapter correctly applied no change. All four weapon viewports, Total scroll restoration, actual P glyph and 19 other fact icons remained correct. The fifth diagnostic package is `4985d471d439e8bb3a423f7c235eb84c28df6912a20b1553e2ec924a0adfc260`, DLL `2ae1e48a56746fddcc5a0d337a0992608837240ff62b71edf306e6077c169070`, MVID `549fcf90-d68f-4025-b3d7-9f7501f22571`. All 136 original files were independently restored at 16:33:27 UTC.

Build 8 replaces the unobserved MinSize hypothesis: only an exact firearm-containing Total list with a disabled PreferredSize fitter activates the existing native fitter, with horizontal fitting temporarily Unconstrained. It restores both original horizontal mode and enabled state. This avoids a fixed height and leaves width under its existing parent layout. Native metadata/negative tests require that exact before/after contract; the committed-sheet probe requires exact cleanup. Still unqualified pending runtime.

Build 8 passed all local gates. Its no-save run `20260913T1637327955790Z-disposable-elemental-character-creation-case` PASS (11 assertions, 33 native PNGs), and capture provenance validation PASS. The actual Total content became 1,461 units; native P was visible, unclipped and inspected clear. The expected disabled PreferredSize fitter was enabled with horizontal Unconstrained, and original scroll coordinates restored within 0.001 native units. The exact fact/parameter/fallback and 19 other icons were retained. Package `fc083bc35f7066d09cb8582869d35b5aa623c010ddd4c973385cbfd0d979e4ff`, DLL `f796336f751f5a357b42aeb47434b630ae3413ac68b6bc5e9ba4b318d985c87d`, MVID `59e4e738-f93f-404a-8d03-403248775b70`.

Working run `20260913T1641298358705Z-working-save-elemental-character-creation-regression` remains FAIL. Both initial and post-round-trip Total captures were visible/correct (heights 1,461 and 1,333), but the first sheet capture stopped before opening: native OnHide cleared the visible LevelUpController while retaining its same owned global reference. All recorded UI, original cross-scene membership, remote/active party, inventory references/counts and money restored exactly; no save write was observed. The fixed batch stopped before Undine. All 136 original installation files were independently restored after auditing this sixth diagnostic artifact.

The next capture-only correction invokes the existing CloseOwnedCreatorController ownership-checked cleanup after native commit/hide and before the sheet. It retains the registered actor for display while restoring the original global controller and presenter unit. The production glyph/layout adapter is unchanged from the passing no-save artifact. The overall selected-fact extension remains unqualified until working sheet and boundary checks pass.


## 2026-09-13 — selected-fact and character-sheet qualification complete

Build 9 passed repository validation, 23 catalog, 14 native screenshot, six paired
evidence and eleven request cases, 1,629 domain tests, clean Release and the strict
224-file package. Source `601a789472926949042b429a5c0b5d48c34d141e01ceebd94dcfaf9ddaf60c3d`,
package `1d2700bb0fcf069a05857071579456366aa43a3129643e4dbed0d115a163061b`,
DLL `1fe901a2d2fae111ba68116db319c0e625a399f29d5dd933536a4f8643c602d5`,
MVID `2276caa3-7c42-4cb0-98a1-d683706e0540`, based on published `32be894c`.

Its first working run `20260913T1651528053788Z-working-save-elemental-character-creation-regression`
remains FAIL: two complete sheets passed, but the third character exceeded the
creator's 300-second request deadline. CompletionTimeout600 does not control that
deadline. The guard cleaned all seven final invariants and the game exited normally;
no process was killed. Same-DLL reruns at the allowed Timeout900 completed all three
Ifrit Weapon Focus and all three Undine Rapid Reload characters: PASS24, 184 captures,
all ten sheet restoration flags for every actor, and both seven-check creator cleanups.
All six actual P/M/B sheets were inspected clear; names/remote inactive abilities
are recorded as fixture limits, not appearance/action-bar qualification.

The first seven-case boundary batch stopped before launching any game because a
local collector import generated an untracked Python bytecode file. That exact owned
file was moved into ignored artifacts; collectors now disable bytecode and run with
`python -B`. Source fingerprint returned exactly to 601a... and the full reusable
deployment guard passed. No manifest, source or policy bypass occurred.

Fixed boundary batch2 then passed ordinary Gunslinger/Fighter creator cases (11 each),
dependent feats (12), Gunslinger OFF (42), Eastern OFF (43), learning (31) and working
smoke (11). Together with both sheet regressions this is nine PASS runs, 185 assertions
and 255 native PNGs. All five capture manifests pass provenance validation. Current
Total P, Wakizashi and all four Wizard/Sorcerer learning rows were inspected clear.
The earlier timeout's 80 diagnostic images are excluded from qualified capture totals.

All 224 temporary package files, three known runtime additions and 136 backup files
passed the audit. Restore-Live-Mod completed successfully; the caller's subsequent
LASTEXITCODE check incorrectly treated a pure PowerShell script as a native program.
No restore was repeated. Independent verification at `2026-09-13T17:40:52.842003+00:00` proves all 136
original installation paths/hashes and settings are exact. The game exited normally;
no save write was observed. Package/manifest/audit/restoration copies are archived as
`native-fact-slot-qualified-128-*`; curated record is
`reports/icon-overhaul/NATIVE-FACT-SLOT-QUALIFICATION.json`.

Production images are unchanged. The owner has been asked to review the 80 new
paintings in the concrete production packet while independent native UI work continues.
No approval is inferred before an answer. Full racial/action/buff, higher dependent
rendering and scroll inventory/tooltip/merchant evidence remain. Strategic controls
use text-only native buttons in source; confirm live rather than adding icons.
No new save/write authority or master merge/release is inferred.

## 2026-09-13 — native scroll inventory extension started

Selected-fact checkpoint `197f9f6198e4f2d706cf0ac64cc491c7e6ef3bca` was committed
and published through the mandated wrapper. The original installation is restored.
Read native Inventory, ItemSlot, SlotsGroup, FilterController, ItemEntity and
TooltipTrigger contracts before adding the bounded capture phase to the existing
spellbook fixture. It creates three separate identified scroll entities only when
no matching stack preexists, uses native virtual-slot geometry and the existing
description trigger, and restores all original item/UI state without use or trade.
No production pixels, mappings, mechanics or request parameters change. The new
partial is explicitly compiled; two focused provenance corruption cases cover
exact scroll/spell/art identity, preserved controls and real description identity.
Full build and runtime qualification remain pending.


The first full build passed (1,629 domain cases, 16 native-capture corruption
cases, 224 package files). Run
`20260913T1757436387594Z-disposable-teleportation-spellbook-ui` remains ERROR:
Teleport's actual slot and nine unchanged controls passed, but the fixture
checked the tooltip object before native OpenDescriptionWindow initialized it.
The cleanup assertion failed during exception unwinding and obscured the initial
failure; the outer fixture restored successfully and observed no UI exceptions
or save writes. Eleven PNGs are diagnostic, not qualification.
Native IL confirms OpenDescriptionWindow calls SetupTooltipData, which collects
the real slot object. The revision checks both the collected object and
TooltipData.Item after normal opening, records each cleanup predicate separately,
and retains the original exception while still failing any cleanup assertion.
The first artifact is archived under `native-scroll-inventory-first-*`:
DLL `c8e41b2e2a28b7cbe1c22ccea72ce8e918be0356f3c065d59c4f36910502538f`.
The guarded restore succeeded and an independent hash comparison verified all
136 original installation files. No production pixels or mappings changed.


## 2026-09-13 — native scroll inventory qualified

Build2 passed all 1,629 domain tests, 23 catalog, 16 native-capture, six paired
and eleven request cases, clean Release and strict 224-file package validation.
DLL `337140eba471f730d753307f785aedf07c95049b44829c04538bb8853a9b94c4`,
MVID `4a268866-874b-425c-ba4c-40dfcf6a715b`.
Run `20260913T1809335245829Z-disposable-teleportation-spellbook-ui` passed all
38 assertions and produced 16 provenance-validated original PNGs. Each exact
scroll slot and native description passed; all 15 inventory cleanup predicates
passed for the 21 original items and three request-owned additions. Native
tooltip initialization is invoked before checking its collected item and
TooltipData.Item. All six target images were inspected clear at 1280x720.
Final same-artifact working-save smoke also passed 11 assertions.
Both game processes exited normally with no save writes. Audit verified all
224 package files, three expected runtime additions and 136 original backup
files. Guarded restore and independent path/hash comparison succeeded at
`2026-09-13T18:17:26.642244+00:00`. Curated report:
`reports/icon-overhaul/NATIVE-SCROLL-INVENTORY-QUALIFICATION.json`; local gallery:
`artifacts/icon-overhaul-v2/NATIVE-SCROLL-INVENTORY-REVIEW.html`.

No production pixels or mappings changed. Merchant/native racial/action/buff
coverage remains; native ShowAll filter and isolated VendorUI APIs were inspected
as narrower reversible routes. Production80 and final native UI owner approvals
remain pending. No new save/write, feature-to-master merge or release is inferred.


## 2026-09-13 ? native merchant capture started

Scroll inventory checkpoint `6dd055c4d7cc12bae5b1894e9293a5394c755875` passed
post-curation repository validation, was committed and published through the
mandated wrapper. All 136 original files/settings were restored.
The next bounded extension uses the same spellbook/inventory fixture and actual
VendorUI. Native contracts and the existing MidgameWorkingSave shop route were
inspected; no purchase/persistence scenario is invoked. A detached unregistered
ChargenUnit clone owns a private UnitPartVendor with three scrolls and one ordinary
item control. No shared stock/table or party identification is used. It opens and
closes through native trade APIs, never selects an item for sale/purchase, and
checks real Game.Vendor/Store identity, exact player inventory, empty baskets,
unchanged money, filters, group, registry and canonical icon references. Only the
private stock and detached actor/blueprint are disposed. The outer inventory and
spellbook guards still enforce complete original context and zero writes.
Seventeen capture corruption cases now include false merchant ownership, stock,
trade activity and mislabeled inventory surfaces. Build/runtime remain pending.

Merchant build1 passed repository/domain gates but stopped in private-reference
compilation: GroupController.SelectCharacterAction is not publicly accessible.
No deployment occurred. The revised ownership check reads its exact private
backing field, already confirmed in the native contract inspection.

Build2 passed all gates, but pre-launch IL review found that native SlotsGroup
Cleanup clears its virtual rows while retaining Collection. Before deployment,
the fixture now restores the four exact original collection references through
the native property's private scalar setter, only after normal trade closure,
empty virtual lists and exact owned collection checks. It releases the temporary
stock reference before disposing owned items. The superseded build2 was never
deployed; its manifest is archived locally. Build3 will qualify this final cleanup.


## 2026-09-13 — native merchant scroll rows qualified

Build3 passed repository, 23 catalog, 17 native-capture, six paired, eleven
request and 1,629 domain cases, clean Release and strict 224-file package gates.
Exact artifact/source hashes are in
`reports/icon-overhaul/NATIVE-SCROLL-MERCHANT-QUALIFICATION.json`.
The guarded spellbook/inventory/merchant run passed all 42 assertions, including
three actual merchant item rows and twelve merchant cleanup predicates; the
fifteen inventory cleanup predicates and outer world/resource/UI restoration
also passed. Nineteen original captures passed provenance validation. All three
merchant targets were inspected clear at 1280x720 with native frames and an
unchanged ordinary item control. No purchase, sale or save operation occurred.
Final same-artifact working-save smoke passed eleven assertions. Both processes
exited normally. Audit and guarded restore succeeded; all 136 original paths/
hashes and settings were independently verified at `2026-09-13T18:39:36.556148+00:00`.
Immutable archive prefix: `native-scroll-merchant-qualified-128-*`; local gallery:
`artifacts/icon-overhaul-v2/NATIVE-SCROLL-MERCHANT-REVIEW.html`.

Next use the normal native ShowAll feat filter for remaining racial menus. The
source uses fresh visible feat blueprints without HideNotAvailibleInUI. Preserve
eligibility and native disabled states; do not manufacture legal selections.
Strategic text-only controls and active action/variant/buff views remain separate.
All 80 later paintings and final native UI still await owner approval.


### 2026-09-13 — Native racial feat menu implementation (not yet qualified)

Continued from merchant checkpoint `3d46cad39ab74ab4f68dd228c02e57b1af40de5c`.
The exact disposable Gunslinger creator uses its real general-feat ShowAll toggle
and native rows. Native eligibility, markers, preview facts and selection are
retained; original filter and both supported scroll types are restored. Missing
targets or restoration fail a required result assertion. Eleven consumer
dispositions now reference this pending native evidence. Two negative evidence
fixtures cover art/identity/state/control corruption and wrong filter/race/set.
No icon pixels, gameplay prerequisites, publication or approved family changed.
Build and runtime qualification are next; original live installation is restored.

Build 1 passed repository validation, 23 catalog, 19 native capture, six paired,
eleven request and 1,629 domain tests, then failed exact Release compilation on
a local/lambda variable-name collision (CS0136). Renamed the lambda parameter;
no artifact was deployed. Full qualification is rerun on the corrected source.


### 2026-09-13 — All eleven native racial feat rows qualified

Corrected build 2 passed all required gates: 23 catalog, 19 native capture, six
paired, eleven request and 1,629 domain tests, clean Release and the strict
224-file package. Artifact source `1be9713bfda492491794b14cff5df70519ecda191f0602df82b7c7e46acff5f7`;
package `c625fb133c823b18b184cb807c50f029953b9f10e4bfdefabda5d9ebb6241747`; DLL `1d06760cc82b28a483f387d549725d8b1a10aeb072b6f7c5fb73ae4f0bc7e24a`;
MVID `954660fc-44bf-487e-9caa-8664d0ac8343`. The preliminary deployment count check mistook a
PowerShell JSON array wrapper for the list. Before any deployment mutation,
independent comparison proved all 136 original paths/hashes unchanged; corrected
check passed. This was neither installation drift nor an approval rejection.

Four exact disposable point-buy Gunslinger creator cases (Ifrit/Oread/Sylph/Undine)
and same-artifact working-save smoke passed: **59 assertions, 138 native captures,
fourteen focused targets and eleven distinct feats**. Every target was inspected
clear at 1280x720; actual icons, titles, native eligibility/markers and ordinary
controls were exact. Native ShowAll can include wrong-race unavailable entries;
the target set uses the race policy. No prerequisites were bypassed. Original
filter, scroll, selection and facts restored in all cases. Native final review
was reached and canceled without a committed character or new save. Exact runs:
- `20260913T1901332484934Z-disposable-elemental-character-creation-case` — 12 PASS assertions.
- `20260913T1905050196959Z-disposable-elemental-character-creation-case` — 12 PASS assertions.
- `20260913T1909031122668Z-disposable-elemental-character-creation-case` — 12 PASS assertions.
- `20260913T1912467856504Z-disposable-elemental-character-creation-case` — 12 PASS assertions.
- `20260913T1916076110117Z-working-save-smoke` — 11 PASS assertions.

All game processes exited normally. The audit verified all 224 packaged files,
three known runtime additions and 136 backup files. Original installation and
settings independently verified restored at `2026-09-13T19:19:47.653542+00:00`.
Curated record: `reports/icon-overhaul/NATIVE-RACIAL-FEAT-QUALIFICATION.json`.
Local originals/gallery: `artifacts/icon-overhaul-v2/NATIVE-RACIAL-FEAT-REVIEW.html`.
Eleven consumer dispositions updated. Raw native images/IL/packages stay local.
Only technical qualification is asserted; eighty production images and final UI
approval remain pending. Next: strategic native text controls and remaining
actions/variants/buffs, dependent rendered feats and the saved-parameter prerequisite.

### 2026-09-13 — Native strategic text-control qualification

Continued from published racial checkpoint `42fbd3e53a26d445c7f8cd2ce3d8b5c6367b6b34`.
Source inspection confirms TeleportDestinationRows.Add uses a native two-line
CompactRow text button with no spell/scroll image assignment. Six consumer
dispositions record that source contract. The narrow capture extension holds
the existing interaction scenario's six prepared/spontaneous Teleport/Greater
Teleport controls, preserving actual Buttons, stable fresh action keys/text,
native background/control references, source resources, familiarity and travel
state. It observes exceptions and uses the existing viewport/overlay restoration.
One corruption case rejects wrong sources, clipped text, invented spell images,
missing controls and altered resources. No icon, mechanic or scroll fixture is
changed; Word of Recall/scroll-specific screenshots are not claimed from this
test. Build 1 passed repository validation, 1,629 domain tests, clean Release
and the 224-file package gate. Runtime
`20260913T1936054333538Z-disposable-teleportation-interaction` failed the existing
reopen-height assertion (449.22, then seven 454 measurements). All six capture
checks, native-context retention and exception checks passed; the overall
41/42 result remains FAIL. Full outer cleanup passed. The package and diagnostic
evidence were preserved; all 136 original installed files/settings were verified
restored at `2026-09-13T19:41:02.915867+00:00`.

The existing reopen loop measured after one frame despite native fade/layout
work. It now uses the existing native panel readiness wait and canvas update
before each of the eight measurements. The exact stability assertion is
unchanged. Build 2 supplied the successful fresh qualification below. The journal's
earlier UTF-8 text was restored exactly after a local script encoding error;
the historical entries, art and source behavior were unaffected.


Build 2 passed all source gates: 23 catalog, 20 native-capture, six paired,
eleven request and 1,629 domain tests, clean Release and the strict 224-file
package. Artifact based on `42fbd3e5`: source `e7c6c8947475119f16147ab5ee1c18099842f1d619d967e2ca16a0cc110a2e00`;
package `d2038e25959549e2b904d9d390f1e9519e1996421749c1cfd2eb1154bafe8e35`; DLL `a3c3313b3b4e92e56b5343a072beb2010ce1cc1647849ea590ee4e7d64cacb49`;
MVID `e191952f-d7e3-4526-8da0-e1d1f385799a`. Two guarded Steam PASS runs:

- `20260913T1951218865405Z-disposable-teleportation-interaction`: 42 assertions;
  all eight reopen heights exactly 454, six captures, exact native context and
  full outer restoration. Three native movement starts/stops balance. No UI
  exceptions or save writes. Each 1280x720 capture was inspected with its full
  target label and all six buttons plus native Travel/Cancel visible.
- `20260913T1954466168350Z-working-save-smoke`: 11 assertions on the same artifact.

Game exited normally. Audit passed all 224 installed package files, three known
runtime additions and 136 backup files. All 136 original files/settings verified
restored at `2026-09-13T19:58:53.537420+00:00`. Immutable local archive prefix:
`native-strategic-controls-qualified-128-*`; native gallery:
`artifacts/icon-overhaul-v2/NATIVE-STRATEGIC-CONTROL-REVIEW.html`.
Curated evidence: `reports/icon-overhaul/NATIVE-STRATEGIC-CONTROL-QUALIFICATION.json`.
Six explicit non-icon strategic surface dispositions are recorded; actual
captures cover Teleport/Greater Teleport prepared/spontaneous controls. Recall
and scroll-specific rows have source evidence for the shared renderer, without
invented separate screenshot coverage. Art and gameplay are unchanged.
Eighty production images and final UI approval remain pending. Continue native
racial action/variant/buff review and higher dependent rendered feats; no new
saves or writes are authorized for the missing saved-parameter prerequisite.

### 2026-09-13 — Native dormant buff-sheet extension (not yet qualified)

Continued from published strategic checkpoint `2969801d8fa17fa9d8ca09cc4fe2cc1c500fbbbc`.
Native source inspection confirms the real buffs-and-conditions sheet includes
visible inactive buffs, using native dimming and inactive descriptions. Native
FactCollection.AddFact skips activation when its existing ActiveByDefault flag
is false. The bounded extension requires that state on the existing dormant
remote mercenary, adds only thirteen painted buff identities across four race
cases and a native Bless control, and changes no lifecycle flag. It captures
actual Buff objects in their native rows and verifies original actor facts,
other units, area effects and world time. Cleanup removes exact owned facts;
native Clear does not clear a pooled row's Buff reference, so that public field
is cleared only when it holds an exact removed fixture fact.

One new corruption test covers wrong sources/art, clipped titles, unexpected
activation, missing native control and altered context. All 21 capture tests
pass. Full source gates and native runtime are next. This checks dormant buff
presentation; no active ability/effect or final owner approval is inferred.
The original installation is restored and no game is running.

Buff build 1 passed repository validation, 21 capture tests and all 1,629 domain
tests, then exact Release compilation failed on CS1061 because the new partial
file lacked the existing Kingmaker.UnitLogic extension-method namespace. Added
that import; full build 2 is next. No buff candidate was deployed or launched.

Build 2 passed all gates. First Ifrit run
`20260913T2018595100359Z-working-save-elemental-character-creation-regression`
failed before any buff capture: 9/13 assertions passed, 32 native frames.
The native sheet's error-path immediate-close assertion masked the original
setup exception. Original creator membership, inventory, money and pause flags
all restored; the sheet had not finished its native close at that immediate
sample, so no UI restoration PASS is claimed for the run. The game exited
normally. All 136 original installed paths/hashes were independently restored
at `2026-09-13T20:25:01.520453+00:00`; failed package/evidence are preserved.

The next correction records exact native prerequisites and each blueprint/control
resolution stage before mutation. Sheet cleanup still fails the overall request
when incomplete, but records its failure without replacing the original exception.
No lifecycle condition or assertion is relaxed. Full build 3 and a fresh bounded
Ifrit run are next; other races and smoke wait for this fixture to qualify.

### Native buff diagnostic 3 and lifecycle correction

Build 3 passed all source/package gates. Guarded Ifrit run `20260913T2034161943142Z-working-save-elemental-character-creation-regression` failed before buff insertion: the exact registered remote owner was outside the world with an empty inactive buff collection, but its descriptor remained turned on. All other setup checks passed. The prior descriptor-off assumption was incorrect. Native `FactCollection.AddFact` gates activation on `ActiveByDefault`; `BuffCollection.OnFactCreated/OnFactAdded` do not independently activate the buff. The correction preserves the original descriptor state and retains the native inactive-collection gate; it changes neither lifecycle flag. Capture validation now independently requires world exclusion, unchanged descriptor state and inactive collection. Original creator membership/items/money/pause restored, normal process exit completed, and all 136 original installation paths/hashes were independently restored at `2026-09-13T20:39:26.586868+00:00`. Failed candidate/evidence archived locally under `native-racial-buffs-diagnostic-3-*`. Build 4 and a fresh Ifrit run remain pending.

### Native buff build 4: section-cache boundary

Build 4 passed all gates. Guarded Ifrit `20260913T2044196339364Z-working-save-elemental-character-creation-regression` passed lifecycle/binding prerequisites and added five exact inactive buffs without activating effects. Capture failed at the native control lookup; all owned buffs and retained row references were removed, with original facts/world unchanged and no instrumentation exceptions inside the buff scope. The native sheet `Refresh` invalidates only scores/attack/defense; `UISection.UpdateData` skips `FillData` for its cached same-owner reference. The correction calls the normal buff-section `SetDirty` before refresh, both after addition and after removal, chooses an actual section-group membership and checks native `IsShowed`. Diagnostic row bindings are recorded before capture. Labels must preserve exact native source text while allowing the font's own casing. All 136 original installation files were independently restored at `2026-09-13T20:50:19.776973+00:00`; failed artifact archive prefix `native-racial-buffs-cached-4-*`. Build 5 and a fresh Ifrit run remain pending.

### Native buff build 5: Ifrit PASS and Oread viewport failure

Build 5 passed all source/package gates. Ifrit run `20260913T2056263867119Z-working-save-elemental-character-creation-regression` passed all 13 assertions with 102 native captures and three complete mercenary round trips. Four target records were visually inspected in one independently verified byte-identical frame. Oread run `20260913T2106286755913Z-working-save-elemental-character-creation-regression` failed on restoration after its third target capture: all three target icons/labels and the native control were clear, but the normalized scroll restoration check failed. Its 150-unit content already fit the 426.22-unit viewport; recorded content Y changed only from -0.0000610351563 to -0.0000305175781. All owned buffs, original facts/world and creator membership/items/money/pause restored; no effect activation or save writes. Remaining races and smoke did not launch. All 136 original installation files were independently restored at `2026-09-13T21:17:03.274627+00:00`; archive prefix `native-racial-buffs-scroll-5-*`.

The next correction adds an explicit read-only mode for already visible buff rows. It calls no scrolling/restoration setters, keeps the existing real content-position/velocity/axis checks and visible bounds checks, and records normalized values for diagnosis without treating a fitting-content ratio as meaningful scroll state. Existing reveal-and-restore callers retain their behavior. Focused validation requires the read-only API and rejects requested scroll mutation. Build 6, Oread first, all remaining races and same-artifact smoke are required before qualification. Earlier Ifrit PASS remains a separately labeled artifact.

Build 6 Oread and Ifrit passed all 13 assertions each. Sylph stopped on the native
TMP overflow flag for Breeze-Kissed: Winds Calmed (all glyphs appeared visible
in its captured native frame); exact sprite, inactive buffs, world state and
owned buff cleanup passed. The incomplete creator case is FAIL, not qualified.
Undine/smoke did not run. All 136 original installation files/settings restored
at `2026-09-13T21:52:42.598471+00:00`. Immutable local archive:
`native-racial-buffs-label-6-*`. Investigate native TMP metrics before changing
labels/layout. Master advanced to `e5f1426a6347793e1e978237be4b0a88d4c9d662`
(v0.0.129); preserve this unfinished buff change, integrate and qualify that
requested upstream baseline, then resume the remaining buff/action/feat work.

## 2026-09-13 ? v0.0.129 upstream integration started


Current work integrates owner-requested master
`e5f1426a6347793e1e978237be4b0a88d4c9d662` (v0.0.129) into the existing published
icon branch, based at `2969801d8fa17fa9d8ca09cc4fe2cc1c500fbbbc`. The normal
merge is uncommitted until qualification. Keep upstream edge-scroll/camera
stabilization and the icon native panel readiness wait together. Combined domain
suite has 1,639 cases (1,632 upstream plus seven icon cases); the public upstream
release record remains historical and does not authorize releasing these icons.

Unfinished buff work is preserved locally in
`artifacts/icon-overhaul-v2/pre-129-native-buffs-worktree.zip`, SHA-256
`e434a4d023e01871ed5170f9aa90ea680bdad7a270aebe11b58285b8a4bc5a89`,
and exact own-work stash `9bc2ebed135d638d11b7118a06da2dd756261a9a`.
Build 6 Ifrit/Oread passed; Sylph failed its native TMP label overflow flag.
All added buffs and original world state were restored; the incomplete case is
FAIL. Undine/smoke did not run. Native frame is visually complete, so inspect
actual TMP geometry before changing the label/layout. Original 136 installation
files/settings were independently verified restored at
`2026-09-13T21:52:42.598471+00:00`. No game is running.

Next: full 0.0.129 build/package validation, guarded strategic interaction and
spellbook/item UI regression plus working-save smoke; inspect exact native
captures, restore the installation, curate and publish the integration checkpoint.
Then reapply the named buff WIP and complete buff/action/variant/higher-feat UI.
The following records qualify their stated earlier artifacts only.

## 2026-09-13 ? v0.0.129 integration qualified

The icon branch integrates owner-requested master `e5f1426a6347793e1e978237be4b0a88d4c9d662`
(v0.0.129), preserving published history. The integration combines upstream
camera stabilization with the native icon panel readiness wait. The exact
[qualification](../reports/icon-overhaul/NATIVE-129-INTEGRATION-QUALIFICATION.json) records **five guarded
Steam PASS runs, 208 assertions and 30 inspected native captures**: strategic
controls (43), spellbook/item/merchant UI (42), native scroll behavior (65),
working-save smoke (11), and native learning including Oracle Recall (47).

Repository validation, 23 catalog, 21 capture, six paired, eleven request and
all 1,639 domain tests passed, with a clean Release and strict 224-file package.
The post-capture validator update recognizes only the exact Oracle/class/Recall/
level-six combination and preserves the four arcane learning combinations.
Its fresh full build reproduced the tested package and DLL byte-for-byte.
Runtime artifact source `28341346def10fa61f08533094a7448f9193c3165076572dba7ff0f493c0a3b3` was built from published
parent `2969801d8fa17fa9d8ca09cc4fe2cc1c500fbbbc` with the pending merge; the report separately records
that tested source state and the validator follow-up build.
Package `ceb355c28891c71827a1116b5b39e49b5d65534b6e3ba788b4aaeaefb7ecf9a9`; DLL `5acabee74149901cc0476bf384631855f7936a36e04b6e3e98dffb09cd54473c`;
MVID `3e431017-1e9b-41b2-9b7f-4264ccf2a2bc`. All 136 original installation files/settings were
verified restored at `2026-09-13T22:18:56.409124+00:00`.
No save writes were observed; all game processes exited normally.

The six strategic text targets, ten spellbook/preparation/description captures,
nine scroll inventory/description/merchant targets, and five learning rows were
inspected at 1280x720. Two preparation frames contain an unrelated native hover
panel away from the target. The Wizard seventh-level learning fixture retains
a central header of 5 while its target badge/list show 7; target art/name/badge
are clear. These observations qualify the stated targets, not unrelated headers.
Native scroll behavior has structured evidence; separate pictures of every
Recall/grouped-reader action are not claimed. Earlier UI families keep their
separately labeled exact-artifact reports.

Ten exact pilots and family direction are approved. Eighty production images
and final native UI acceptance remain pending. The permanent guide, references
and catalog retain their approved-family contract; protected art and saved
identities are preserved. Runtime uses 90 exports and 137 painted assignments;
284 catalog identities retain their dispositions. The six strategic ability/item
records now link this v0.0.129 evidence.

The unfinished buff checkpoint is preserved in own-work stash
`9bc2ebed135d638d11b7118a06da2dd756261a9a` and local archive
`artifacts/icon-overhaul-v2/pre-129-native-buffs-worktree.zip` (SHA-256
`e434a4d023e01871ed5170f9aa90ea680bdad7a270aebe11b58285b8a4bc5a89`).
Build 6 Ifrit/Oread passed; Sylph failed a native TMP overflow flag although its
full title appeared visible. Undine/smoke did not run on that buff artifact.
Its original installation was restored, and it is not a qualified buff checkpoint.
Next: reapply that exact WIP while retaining this integration; validate actual
native glyph geometry and complete four racial buff cases, then supported
racial actions/variants and higher dependent firearm rendered feats. Existing
working-save P/M/B parameters remain absent; new saves/writes require separate
authorization. This is a technical checkpoint; owner approvals and final mission
acceptance remain open. No feature-to-master merge or icon release is authorized.

Original intake: sixteen members / ten references staged in the lab; ZIP SHA-256
`78a44c77861969fee778a574bdee9c3761cef323553a20951c5e9fbbb7765a8f`.
Local native gallery: `artifacts/icon-overhaul-v2/NATIVE-129-INTEGRATION-REVIEW.html`.
Raw native frames, packages, proprietary references and machine state remain local.

### Native buff work resumed on qualified v0.0.129


Current buff build 7 adds observational TMP final-mesh geometry to the native
inactive buff fixture. A raw Overflow flag is retained as diagnostic; every
expected nonspace glyph must exist in the actual mesh, fit its real row/clipping
masks and stay clear of the native icon/timer. No title, font, UI geometry,
scroll or lifecycle change is used to make it fit. Missing/clipped/overlapping
meshes fail. Original native controls/facts/world and complete cleanup remain
required. Run Sylph first on 0.0.129, then all other race cases and smoke on the
same artifact. Current source is unqualified; original installation remains
restored. Preserved earlier stash/archive remain intact.

### Native buff build 7 diagnostics and build 8 qualification

Build 7 on `a20b0d236a1259f4c92521ab3a7780956095631e` passed Sylph,
Oread and Ifrit (13 assertions each). Undine run
`20260913T2318326669713Z-working-save-elemental-character-creation-regression`
failed on the Nereid Fascination Aura mesh extending to X=120.214111 beyond
the unmasked row's nominal X=120. Every glyph was generated inside the real
mask (X maximum 150.270081), clear of its icon/timer. Native inspection shows
the complete title. This is a measurement-contract error, not permission to
resize text or relax actual clipping. Build 8 retains nominal row containment
as diagnostic and independently checks actual masks and every neighboring
visible row. Focused tests reject missing masks/rows, overlapping neighbors,
nonfinite bounds and false containment evidence. All 25 capture tests pass.

The failed Undine case removed all added buffs and restored original world/facts
and all seven outer creator collections. Its early exception left service-window
animation cleanup incomplete at observation time; the run remains FAIL. The
game exited normally and no save write occurred. The exact 136 original files
were restored and independently hashed before source changes. Build 7 package,
manifest, audit, restoration and partial PASS results remain local under
`native-racial-buffs-row-7-*`. Smoke did not run on that artifact. Build 8 must
complete all four racial cases and smoke before this extension is qualified.

### Native inactive buff-sheet checkpoint qualified

Five exact-artifact Steam runs passed 63 assertions with 393 native captures and thirteen inspected buff identities. All twelve disposable mercenaries completed their original regressions; owned buffs, original facts/world, native UI and creator state restored. Same-artifact smoke passed. Source `7e15cd13cbcf0917c9eaefab78a90d52e89467708c1b6fa93c3eac1572a0eb62`, package `30030682eda80a3a41f65dc0a0c42993e0eb3f5dfd45ec034088eeaeebcdd994`, DLL `804f0b30ab8e2dd417e1c6fd267c9bb23c8efa675889e87b3bff198eff2906f3`, MVID `ff0b4a9c-7daa-4a94-8f82-f9ca3bb47693`. Full 23/25/6/11/1639 test gates, clean Release and strict 224-file package passed. All 136 original installation files/settings restored at `2026-09-14T00:17:14.376986+00:00`. See the curated buff report for exact runs and evidence limits. Thirteen catalog consumers updated; post-run curation changes no compiled code or art. Owner approval of eighty production images and final native UI remains pending. Continue supported action/variant and higher dependent firearm surfaces.

### Native racial action fixture begins on the qualified buff checkpoint

Buff checkpoint `a42ba694cc4cb5910390341566f691a76352e435` passed the required
wrapper push. Integrated the independently researched native group/variant/
held-touch fixture for 39 exact consumers. Added seven focused corruption tests
and one complete capture-dispatch test. Original native availability and
collection activation are preserved; no cast or save write is requested.
Exact ordinary-control names and all native runtime assumptions must still be
verified by the guarded run. No runtime success or visual approval is claimed.

### Action build 1: native group has no original selected owner

All 23/33/6/11/1639 checks, clean Release and package validation passed.
Ifrit run `20260914T0023073077401Z-working-save-elemental-character-creation-regression`
failed before action fact insertion: all native manager/group/element owners
were null and the group was inactive after creator completion. Its buff sheet
and seven creator cleanup checks passed; overall FAIL with no save writes and
normal exit. Archived source/package/DLL identity and verified all 136 original
installation files restored. Remaining races and smoke did not launch.

Build 2 uses the native MultiSelect callback for the existing party leader only
when the original selection is empty and every owner is null. Installed-assembly
IL confirms native selection removal calls Setup(null), Group.Set(null) and the
normal hide animation. No owner field, availability flag or visible state is
forced. Original character facts/resources, inventory/time, selection and UI
settings are checked across the whole scope; native hiding must finish after
restoration. The wrapper retains a capture failure while cleanup animations
settle. Original inactive caches can warm through normal native selection;
request-owned pooled references must still be released.

### Action build 2: nonempty selection is not a bound native action owner

Ifrit run `20260914T0037265979380Z-working-save-elemental-character-creation-regression`
failed the same original-owner guard because the selection wrapper treated any
nonempty selection as already bound. Evidence confirms all three action owner
fields null and the group hidden; no action facts were inserted. Buff and seven
creator cleanup checks passed, no save writes, normal exit. Full build gates
passed before deployment; the exact original 136-file installation is restored.

Build 3 keys the wrapper on the actual native manager owner, records the exact
original selected list and native IsSingleSelected value, and uses the ordinary
single-leader selection for a fully unbound hidden group. Installed native Set
requires IsSingleSelected; an ordinary multiple selection also leaves it null.
Original ordered selection is restored. An existing bound manager still uses
the original no-selection-change path. No eligibility or owner flag is forced.

### Action build 3: native selection succeeds; exact control identity observed

Ifrit run `20260914T0049131933649Z-working-save-elemental-character-creation-regression`
proved the normal-selection scope and every cleanup check. The original native
selection contains three party members and IsSingleSelected=false. Single-leader
selection binds all expected owners and displays the group; the ordered original
selection, null owners, hidden group, UI settings and world restore after exit.
Both native ability collections are already inactive. The next guard failed on
the draft control name before any action additions. The existing native fact is
`FightDefensivelyToggleAbility`, GUID `09d742e8b50b0214fb71acfc99cc00b3`, inactive
and off. Build 4 pins that observed GUID/name and validates the same exact
control in capture evidence; a coordinated wrong control GUID is rejected by
the focused test. Full 23/33/6/11/1639 gates passed for build 3; its overall run
remains FAIL, with buff/creator cleanup, no save writes and normal exit. Original
136-file installation is independently restored. Remaining races did not launch.

### Action build 4: native action-type cache ownership

Ifrit run `20260914T0101496601247Z-working-save-elemental-character-creation-regression`
reached native menu setup and failed its strict world/part guard before any
qualified action capture. Native `AbilityData.ActionType` calls
`Ensure<UnitPartAbilityModifiers>`; the remote fixture acquired that empty part
during menu rendering. Native IL confirms the empty FreeActionList query. All
original abilities, buffs, features, activation, toggles, resources, commands,
positions, damage, party, selection, time and inventory were retained; the extra
part caused restoration FAIL. UI/group/pool/settings cleanup passed, no native
exceptions or save writes, and normal exit. The full build gates passed before
launch (23/33/6/11/1639). Artifact source
`13096744ad61d1f79f447d92717bc9903ed84bd1195598707e6388456b195354`, package
`80d1b4026634ccfcf6d20ef1b1f03923f00e983f931f958112b68480deb19512`, DLL
`a17d19946ca5e64223d23c9bdebd463383b97f4d56515c32061649e1212685d1`, MVID
`14e22f8e-7da6-4f6a-ae5a-fad638615194`. The original 136 paths/hashes/settings
were independently restored; archive prefix `native-racial-actions-build-4-*`.

Build 5 explicitly owns the initially absent empty native modifier cache on the
remote fixture before rendering and removes only that exact empty part after
releasing native UI references. An existing empty part is preserved. No entry or
free-action modifier is added. Original part equality remains required after
cleanup; capture validation rejects absent, foreign or populated caches. The
remaining race runs did not launch on build 4. This failure is not qualification.

### Owner allocation pause and production approval

The owner requested a stopping point and push with 3% weekly allocation remaining,
then explicitly wrote ?Approve all 80 images? in response to the production review.
All 80 source/export/runtime hashes matched; exact approval is recorded in
PRODUCTION-APPROVAL.md/.json and the canonical manifest/catalog. All 90 art exports
now have owner approval; native UI acceptance remains separate.

Build 5 finished with repository validation, 23 catalog / 34 capture / six paired /
eleven request / 1,639 domain tests, clean Release and strict package PASS. It was
never deployed or runtime-tested. Archive prefix:
`native-racial-actions-build-5-UNDEPLOYED-*`. The eleven unfinished action
source/project/test files are preserved in exact local stash
`2bf5ec3ded6b81f29c2058ec872cf5088d521bf7`; older stashes are untouched. Seven
higher-feat drafts compile in isolation but remain unintegrated. No unqualified
runtime source is committed at this pause. All 136 original installation files
and settings are restored; no game, test process or save write remains active.
See reports/icon-overhaul/PAUSE-REPORT.md for exact hashes and resume commands.

Approval/pause repository validation passed (23 catalog, 25 native capture, six
paired, eleven request cases and source/export/protected checks). The completed
coverage guard now accepts the owner-approved state while still rejecting a
missing required image. The negative approval fixture explicitly removes catalog
approval so it remains meaningful after actual production acceptance.

## 2026-09-17 — Z executor takeover (owner-authorized resumption)

The owner resumed the paused icon-overhaul qualification and handed it from Codex
to Z. Base: published pause checkpoint `7c50e0911c3d15e3e65ea4a296aa07f820cb5d3c`
(remote `origin/codex/icon-art-overhaul-v2` matched exactly; no local worktree
owned the branch). Z created `worktrees/icon-art-overhaul-v2` on that branch and
confirmed no locks, no other executor processes, and a clean tree. Main checkout
remains untouched on master.

### Local recovery material is absent

The pause report's local-only material could not be found on this machine:

- Exact stash `2bf5ec3ded6b81f29c2058ec872cf5088d521bf7` (eleven action
  source/project/test files): absent. `git stash list` is empty in the shared
  object store; `git cat-file` on the exact ID fails locally and the blobless
  promisor remote answers "not our ref". Twelve unreachable commits exist in the
  object store, but all are older stashes from Sep 4–5 based on `a9491b25` /
  `dc3367b5`, none on the reported `a42ba694` base. They were left untouched.
- `artifacts/icon-overhaul-v2/` (seven `NativeHigherFeat*.pending.cs` drafts,
  `Deploy-NativeRacialActions.ps1`, `Run-NativeRacialActionMatrix.ps1`,
  failed-run evidence, native API research, compiler helper, build-5 archive,
  local buff gallery): absent from every permitted lab path, worktree and
  backup; no bundle exists. The original worktree was evidently removed.

Per the handoff instruction this is reported rather than fabricated: nothing
was "recovered" and no GitHub claim is made. Because the action fixture is the
assigned remaining engineering, Z reimplemented it from the published evidence:
the qualified buff-fixture pattern (`ElementalCharacterCreationNativeBuffIcons`),
the build 1–5 diagnoses recorded above in this journal (owner-null binding,
single-leader selection, pinned `FightDefensivelyToggleAbility` control
`09d42e8b50b0214fb71acfc99cc00b3`, and the build-5 ownership rule for the
initially absent empty `UnitPartAbilityModifiers`), the committed native
action-bar APIs, and the canonical catalog's exact 39 action consumers.

### Reimplementation (source only at this point)

- `src/KingmakerGunslinger/RuntimeTesting/NativeRacialActionIconRules.cs`: pure
  fixed 39-symbol plan (per-race partition: Ifrit 15 incl. the cross-race
  elemental feats, Oread 7 incl. the activatable, Sylph 9 incl. a held-touch
  delivery, Undine 8 incl. the other delivery), row classification
  (parent/variant/activatable) and the modifier-cache ownership rule.
- `ElementalCharacterCreationNativeActionIcons.cs`: guarded fixture extending the
  qualified creator regression. New exact request parameter
  `nativeActionCase=racial-actions` (parser count 5, Fighter + point-buy +
  automatic exit only, exact scenario only; PS orchestrator preflight equally
  narrowed). It runs once for the first committed dormant mercenary, binds the
  native action bar through ordinary single-leader selection, renders every
  catalog consumer of the race through the real `ActionBarSpellsGroup` toggle
  (parents as inserted facts, variants/deliveries as detached native
  `AbilityData`, the activatable and the pinned Fight Defensively control through
  the native widget + `MechanicActionBarSlotActivableAbility` + native
  `InitSlot` icon path), captures native rows, and removes every request-owned
  fact/widget/selection/cache under exact before/after comparison. Variants are
  derived from the live blueprint graph (`Parent != null`), not hand-listed; the
  machine-derived parent/variant counts are recorded as evidence rather than
  copying the pause report's 26/10 bookkeeping.
- Six focused domain tests (exact set, corruption, cache ownership,
  classification, request gating, dispatch/restoration wiring) replace the lost
  seven; deterministic suite pin moved 1639 → 1645 with the matching
  static-validation record update (same pattern as `a20b0d23`).
- Three new orchestrator preflight rejection cases cover the wrong class, wrong
  case value and missing automatic exit for the new parameter.

### Runtime pause (shared installation)

While first attempting the preflight test, Kingmaker PID 9796 (plain owner
launch, no guarded request in its command line, started 2026-09-17 16:31 local)
was found running. Per the exclusive-ownership rule, deployment and all guarded
runtime launches are paused until the owner's game exits; local build gates
only. Build 6 numbering is retained for the first rebuilt candidate.

### Build 6 runtime qualification (2026-09-17/18 UTC)

The reimplemented fixture was qualified through the guarded Steam route with
iterative, evidence-driven corrections on this machine only. Artifact lineage
(all packages passed full Build-Local gates before deployment; deployment used
the backup-first Deploy-Local guard each time):

1. `21eca39a…`/`07c5ead4…` — request parameter was dropped by the PS request
   builder (ordinary regression PASS, fixture inert). Fixed by extending the
   creator parameter serialization.
2. `b74a9b00…`/`54374d3e…`-line — unit-creation mismatch: the dormant-mercenary
   path requires the roll-allocation CustomCompanion creator, but the installed
   Dice Roller is now 0.1.6 (the fixture contract pins 0.1.2 and the mission
   forbids touching another mod). Resolved by accepting the Fighter/point-buy
   committed request-owned unit instead; the action rendering path is
   independent of the unit's holding state.
3. `c8ff74fe…`-line — native-semantic-reuse consumers (no painted binding)
   resolved through the registered manifest directly.
4. Pause ownership: the working session loads unpaused, so game time advances
   during real-time captures; the fixture now owns and restores the pause for
   the observation window and compares every world dimension individually.
5. Popup-lifecycle finding (machine-traced with a request-armed Harmony prefix
   on `ActionBarSpellsGroup.Hide`, removed again after diagnosis):
   `ActionBarManager.Update → ActionBarSlots.Set → ActionBarIndexSlot.Set →
   ActionBarGroupSlot.SetSpontaneousControls → Hide` reconciles every group
   slot's conversion popup each frame and closes any popup whose list is not
   the slot's own spellbook conversion — a bare `Toggle` or a native
   `OnToggleGroupClick` with a borrowed conversion list is closed on the very
   next Update. The popup menu therefore cannot persistently present an
   arbitrary racial action list; this is a native design constraint, not a
   fixture bug.
6. Final design: rows render through the popup's own FillSlots binding — the
   exact native `ActionBarSpontaneousConvertedSlot` widget from the group's own
   prefab, native `MechanicActionBarSlotSpontaneusConvertedSpell` /
   `MechanicActionBarSlotActivableAbility` mechanic slots, native
   `Initialize`/`Set`/`InitSlot` icon binding (Icon.sprite = native GetIcon()) —
   parented to the always-live native portrait strip (GroupController). The
   party selection and the action bar's own binding are never displaced; the
   party stays untouched (verified), the pause is owned and restored, and the
   initially-absent empty `UnitPartAbilityModifiers` caches are owned and
   removed. Evidence labels the presentation as "native action-bar group rows
   through the FillSlots widget/mechanic/icon path; popup-menu lifecycle not
   exercised (closed by design for foreign lists, machine-traced)".

Ifrit PASS on package `426e94a6440562fbe5fe87fc740322f228d844a63e22c871816ed530d7cc2c19`,
DLL `5f7f5d5a88889e4371dfc4a184566bcfe0a1281fdaaeaace8b29675d984b6f85`
(run `20260918T0634424748160Z…`): 13/13 assertions including
`actual-native-racial-action-group` with all 15 Ifrit consumers
(9 parents + 6 variants; painted and native-reuse) captured in catalog order,
pinned `FightDefensivelyToggleAbility` control exact, selection untouched,
every world dimension exact and full request-owned cleanup. Oread/Sylph/Undine
runs follow on the same immutable artifact; their results are recorded below.

### Build 6 final matrix result

All four race runs and the same-artifact smoke PASS on the single immutable
artifact package `f0c234df044367dfc174336487687e3e29198b3599f7a0b1d9baddd18ed8c669`,
DLL `7d5191df338915e6ac08b2cacacdddff53a7c9fdc265f6b4d31be54211a7ffe7`,
MVID `abb01c54-1543-4616-ba2a-34d6d5769fd7`, source state
`e45612b732d6023993a3608e4b045bedb42322116b4824469b76c233803cad02` (uncommitted
working tree over `7c50e091`; committed below byte-identically):

- Ifrit `20260918T0817462342287Z` PASS — 15/15 consumers, 103 captures.
- Oread `20260918T0719294558189Z` PASS — 7/7 (incl. the CrystallineForm.Mode
  activatable), 95 captures.
- Sylph `20260918T0756109207621Z` PASS — 9/9 (incl. ShockingGraspDelivery),
  97 captures.
- Undine `20260918T0806485537845Z` PASS — 8/8 (incl. ChillTouchDelivery and
  the ShakeFree variant), 84 captures.
- Same-artifact `working-save-smoke` `20260918T0836413995623Z` PASS (11
  assertions).

39/39 catalog action consumers qualified through native widgets with exact
sprite references, catalog-order capture sequences, the pinned
FightDefensively control exact in every run, the party selection untouched,
every world dimension exact, owned pause and empty modifier caches removed,
and no save-writing API observed anywhere. 379 native captures total.
Post-run restoration: the owner installation was restored from the pre-test
snapshot `runtime-backups/live-mod/20260917T2038524766460Z`; the post-restore
inventory is identical (138 files) and the original DLL bytes are back.
Curated record: [NATIVE-RACIAL-ACTION-QUALIFICATION.json](../reports/icon-overhaul/NATIVE-RACIAL-ACTION-QUALIFICATION.json).

Machine-derived classification note: the pause report's 26 parents / 10
variants bookkeeping is superseded by the runtime-derived 25 parents /
11 variants (Gust is the parent of BullRush+Trip; ShakeFree is a parent-owned
variant), with the two held-touch deliveries and one activatable unchanged.

## 2026-09-18 — review-driven continuation: A-fixes, Task B, Task C revision

Continuation mission `Z-ICON-OVERHAUL-CONTINUATION-REVIEW-FIXES.md`. Takeover
verified at `c7a41559` (= remote, clean); local-only helpers backed up to
`C:\Dev\KingmakerGunslingerLab\icon-recovery\20260918T…-icon-worktree-local`
(18 files, SHA256SUMS + restore README). No new search for the missing Codex
stash was performed.

### Review findings A1–A4 fixed and re-qualified

- **A1** The pinned Fight Defensively control now has a dedicated capture and
  its own `controlRow` evidence (identity, rendered sprite, active row,
  preserved initial on/off state, capture record), evaluated by the shared
  `EvaluateNativeRacialActionEvidence`; it can neither inflate nor replace the
  39-consumer coverage. Negative tests prove missing/wrong control identity,
  missing capture, wrong sprite, changed state and control-substitution each
  fail the evaluation.
- **A2** `Reject` now throws before any fixture-owned mutation; an accepted
  existing cache must retain its exact instance and ordered entry identities
  (ability GUID + source fact reference), not just its reference; the owned
  (initially absent) cache may be removed only when it is the exact instance
  first observed under exclusive paused observation and still empty; populated
  or replaced parts fail without destructive cleanup
  (`DecideOwnedModifierCacheCleanup` + dispatch tests).
- **A3** Final `restored` is computed after every cleanup step including pause
  release (`pauseRestored` equality); pause-restoration failures are recorded
  before serialization and fold into the result; every owned widget is tracked
  at acquisition (`ownedWidgets`) and checked released
  (`ownedWidgetsDisposed`); the meaningless placeholder `slotsAfterHide` was
  removed; the bar-not-borrowed claim is now an actual before/after comparison
  of the bar owner and the chosen group's active/slot state.
- **A4** `NATIVE-RACIAL-ACTION-CORRECTION.md` records the superseded scope with
  separated statuses (exact bindings/widget rendering; rendered control +
  restoration; ordinary native action flow — pending, with the machine-traced
  popup constraint documented as evidence about the attempted route only; owner
  final UI acceptance — pending). Evidence `presentation`/`evidenceClass` and
  the assertion text state exactly what is and is not claimed. A supervised
  owner checklist (`SUPERVISED-NATIVE-FLOW-CHECKLIST.md`) covers parent menu,
  parent→variant navigation, activatable surface and held-touch delivery.

Deterministic pin 1645 → 1651 (six new native-racial-action negative/policy
groups + three higher-feat groups), validated by real evidence-evaluation
logic rather than source-text only.

### Task B qualified

New scenario `disposable-firearm-higher-feat-roots` (save-free, wired through
catalog/parser/runner/PS metadata/preflight). Run
`20260918T1350433937364Z` **PASS 7/7** on package
`2fca98bc2b1abecc69985f605b11d4e8d63c707869bd55cef3a6277e9edc77cd`, DLL
`de1e755e3d14e6f9…` (MVID `b84ee48b-d052-41d8-b688-ce5b7acd4149`): 12/12
root/weapon combinations committed through real Fighter 1–19 controller
ladders (Weapon Focus chain first; GWF refused at fighter 2–7 = ineligible
control; committed at 8+; WS at 4+; GWS at 12+; IC at BAB 8+), native
child-parameter selection flow, one cancellation visit committing nothing,
FeatureUIData null-Icon + exact P/M/B monogram for every committed fact, and
zero unintended KMG facts. Key native fact discovered: the appended entries of
all five integrated roots carry the registered Weapon Focus choice of the kind
as FeatureParam (shared published parameter set).
Record: [FIREARM-HIGHER-FEAT-ROOTS-QUALIFICATION.json](../reports/icon-overhaul/FIREARM-HIGHER-FEAT-ROOTS-QUALIFICATION.json).

### Task C revised

`SAVED-PARAMETER-AUTHORIZATION-REQUEST.md` rev. 2 fixes the six-vs-five count
inconsistency (now: three Weapon Focus parameters + one Greater Weapon Focus
parameter + one static Rapid Reload child = five, internally consistent),
distinguishes parametrized native-root facts from static children, bases the
character on the Task B legal progression, commits to read-only verification
of the real save path/format before any write, and keeps
SAVED_PARAMETER_ROUND_TRIP = NOT RUN — AUTHORIZATION/INPUT REQUIRED.

### Final-artifact matrix and restoration (2026-09-18 UTC)

The documentation-stabilized rebuild reproduced the candidate byte-identically
(package `2fca98bc2b1abecc69985f605b11d4e8d63c707869bd55cef3a6277e9edc77cd`,
DLL `de1e755e3d14e6f9e18c98282683fd332dc877f6e728278ca365c25ba48d7aaf`,
MVID `b84ee48b-d052-41d8-b688-ce5b7acd4149`; deployment manifest
`20260918T1413202579998Z`). Complete guarded matrix on that single artifact:

- `disposable-firearm-higher-feat-roots` `20260918T1422395155233Z` PASS 7/7.
- Racial action races (corrected fixture with control-row capture and
  post-cleanup evaluation): Ifrit `20260918T1423403537703Z`, Oread
  `20260918T1429428647441Z`, Sylph `20260918T1435258083180Z`, Undine
  `20260918T1634214906135Z` — all PASS. The first Undine attempt
  `20260918T1441274674433Z` timed out with no result written (transient;
  superseded by the clean PASS, not counted as evidence).
- Same-artifact `working-save-smoke` `20260918T1638315844558Z` PASS (an
  earlier smoke attempt raced the timed-out process and produced no result).

No save writes in any run. After the matrix the owner installation was
restored from the session's pre-test backup
(`runtime-backups/live-mod/20260918T1154036412512Z`): 138/138 file inventory
identical, original DLL bytes (`7e62290c…`) back, no game process left
running. All 90 approved images re-verified no-write on the final tree; the
Rapid Reload runtime copy matches its approval hash exactly. Records updated:
action qualification JSON (corrected-matrix section), higher-feat JSON
(confirmation run), evidence ledger (final run IDs + artifact identity).

## 2026-09-18 (later) — finalization fixes after the 1e433da1 review

Continuation mission `Z-ICON-OVERHAUL-FINALIZATION-FIXES.md`. Takeover
verified at `1e433da1` (= remote, clean, game idle; no newer local work).

### F1 Controller lifecycle and cancellation proof

`RunFirearmHigherFeatRoots` now gives every controller visit an explicit
native cleanup boundary: each level visit is a try/finally that closes its
own controller; a live reference is never overwritten; an unsuccessful
cancellation probe or ineligible probe closes its visit before a fresh one
opens; cleanup failures are recorded (never masking a body exception) and
counted in the PASS predicate (`higher-feat-cancellation-and-cleanup`
requires cancellationProved && cancellationAttempted && zero cleanup
failures). A failed child-parameter step now unselects the dangling bare
root selection (`UnselectFeature`) instead of leaving a parameter-less
commit in the visit. The cancellation proof is a real before/after
comparison: `SnapshotProgressionState` captures class/character levels plus
every tracked root/parameter fact with rank; the visit must actually hold
the exact GWS(Pistol) root/parameter pair in the preview
(`previewHeld` from the slot's SelectedItem); Cancel without apply; then
`FirearmHigherFeatRootsRules.EvaluateCancellationEvidence` compares the
snapshots (appeared/vanished/rank-changed facts named exactly, cancelled
target must be absent, levels unchanged). New focused negative tests drive
the same evaluator: target-appears, preview-not-held, level-change,
rank-gain and missing-snapshot all fail with named reasons.

### F2 Parameter diagnostics and save request rev. 3

The final review rows no longer write the per-root wrapper name into a
`paramBlueprint` field. Each row emits the actually observed
root/parameter GUIDs and names (from the committed fact), the independently
expected pair (root GUID + the shared Weapon Focus choice of the kind), the
wrapper GUID recorded separately as `notTheWrapperGuid`, rank, and explicit
`paramFailures` from `FirearmHigherFeatRootsRules.EvaluateCommittedParameter`,
which rejects wrapper-substitution, root mismatch, parameter mismatch and
non-unit rank. The saved-parameter request rev. 3 carries the consistent
five-fact table (three WF parameters + GWF(Pistol) **with the shared Weapon
Focus Pistol parameter, not the GWF wrapper** + the static Rapid Reload
child), read-only save-format verification before any write, and the exact
legal progression from the qualified scenario. Gate unchanged: NOT RUN —
authorization/input required.

### F3 Checklist rev. 2 (catalog-verified)

Burning Hands and Shocking Grasp (+delivery) are now correctly presented as
deliberate native-reuse controls (native spell identities), Chill Touch
(+delivery) as the original-art case; casting/toggling are explicitly marked
STATE-CHANGING; a higher-feat screen section (selection/preview per root
with justified 4-menu representative sampling across the identical weapon
machinery, cancellation/back-navigation with the layout-adapter cleanup
observation, Total/sheet after one real selection) was added; Wakizashi is
checked inside the actual parametrized weapon selector beside Katana/Nodachi
at the owner's verified settings. Separate statuses
(HIGHER_FEAT_SELECTION_AND_DATA / _CANCELLATION_AND_CLEANUP /
_NATIVE_SCREEN_RENDERING, RACIAL_WIDGET_BINDING, ORDINARY_NATIVE_ACTION_FLOW,
SAVED_PARAMETER_ROUND_TRIP, OWNER_FINAL_NATIVE_UI_ACCEPTANCE) are maintained
in the checklist and the higher-feat record.

### F4/F5 Packet and timeout record

A local curated gallery
(`icon-recovery/native-review-gallery-20260918/NATIVE-REVIEW-GALLERY.html`,
12 images, every link verified and SHA-256-matched to its source run via
`manifest.json`) replaces the directory-search packet; the packet names the
exact local package path, hashes, both deployment manifests of the
byte-identical package (reconciled in the ledger) and the restoration
procedure. The two result-less attempts of the previous matrix are recorded
from their actual files: the Undine timeout (orchestration ERROR, PID 17192
left running per the no-force-kill policy, **no in-game stage file ever
written** — nothing can be claimed about its internal behavior or save-write
state either way) and the failed smoke (orchestration ERROR with
`preLaunchKingmakerProcesses = []` — proving no overlapping game existed at
its launch; its own process exited before committing a result). Guards held;
no overlap ever occurred; the later clean PASSes are separate runs and are
not used to infer anything about these attempts.

Deterministic pin 1651 → 1653 (two real evaluation-test cases).

### Finalization re-qualification and restoration (2026-09-18 late UTC)

The corrected higher-feat fixture was qualified through three guarded
iterations on this machine, each with full Build-Local gates before
deployment (a transient BOM introduced into static-validation.json by an edit
was caught by repository validation and stripped before any build proceeded):
first run exposed a retry-condition regression (the probe stopped after its
first legally-too-early attempt at fighter 8, while GWS becomes legal only at
fighter 12); the second exposed that the standalone probe's child-parameter
`SelectFeature` is natively refused even when the ladder's identical call
succeeds in the same visit. Rather than fight that opaque native check, the
cancellation proof was restructured to ride on the ladder's own successful
GWS(Pistol) selection: the visit-start snapshot is taken before any pick;
when the exact legal choice is first held (previewHeld verified through the
controller's selected items) the entire visit is cancelled without apply,
the before/after progression snapshots are compared by the shared evaluator,
and the visit is then reopened and the picks redone for real.

Final artifact package `ebd4921f46aed7710c3f6ec52df9a3c67686a27b0f3ceef71487412e06552ba9`
(DLL `dd45f577c83b84297b520fec1f09f4876c1eb34c66a45490dd6c98469f8ed993` per
deployment manifest `20260918T2158074984660Z`): runs
`20260918T2206391210670Z` and `20260918T2212086644523Z` — **two consecutive
PASS runs, 7/7 assertions each**, including `higher-feat-cancellation-and-
cleanup` with previewHeld=true, identical before/after snapshots and zero
controller-cleanup failures. The action/race matrix on package `2fca98bc…`
remains valid for its scope: `git diff 1e433da1` over the racial-action
fixture sources is empty (byte-identical between the two packages), recorded
in the ledger. After the runs the owner installation was restored from the
session's pre-test snapshot and verified (138/138 inventory identical,
original DLL bytes, no game process). All 90 approved images re-verified
no-write on the final tree; the Rapid Reload runtime copy matches exactly.
