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
