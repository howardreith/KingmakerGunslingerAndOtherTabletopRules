# Build, package, and publish

## 0.0.164 Cleanup 2 publication authorization

On 2026-10-23, after reviewing the Summon Cleanup Iteration 2 gallery and a
Pro code review, the owner wrote "The artwork looks great" and authorized the
release once the review's checks are clear. That covers the v0.0.164 release
validator and Sprint-level candidate gate, merging PR #67 into master,
publishing through `Publish-Release.ps1`, and checking the public download. The
release is 0.0.164, one above the published 0.0.163; the publisher checks the
tag again.

`tools/validate_expanded_summoning_cleanup2_release164.py` admits, relative to
released v0.0.163 master `a35a3277a9`:
- no identity change;
- the 73 pinned Cleanup 2 production files and the version metadata;
- the 62 pinned project lines (compile items, the UniRx and Cloth module
  references and the Gorgon parameter content);
- the 33 pinned modified and 22 pinned added assets and source-art files, at
  exactly their reviewed bytes;
- its own static record.

The released Sprint 29 roster and 1,156-choice census still hold. The
historical sprint and release validators stay as they were.

The branch passes one Sprint-level release candidate gate and a guarded
creature review on that candidate. Its PR then merges normally, and the
ordinary exact-reference publisher runs from clean, pushed `master`:

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -Publish -ConfirmReleaseReady
```

Known limits are listed in `docs/RELEASE-NOTES-0.0.164.md`.

## Selected Sprint 25 exact-artifact publication

The owner explicitly authorized native qualification, public promotion, PR #49
merge and the next available stable patch, then public-download/restoration
verification and final records. Master v0.0.158 is integrated normally; the
current expectation is v0.0.159, checked again by the publisher, not reserved.
Build once in `wt/s25` with `Invoke-KmgGate.ps1 -Level Sprint`; retain the
original identities of scoped reused native evidence. After fresh public
inventory/icon/all-eleven-root checks, use `Publish-Release.ps1` with
`-ReuseQualifiedCandidate -Sprint25GiantBodies -ProductPrNumber 49`, the exact
package/root/ledger/notes arguments. First run `-PreflightOnly` with the fresh
`-ExpectedBaseCommit` while PR49 is draft. Actual publication requires the
verified `-MergedReleaseCommit` and explicit `-Publish -ConfirmReleaseReady`.
These are separate modes; preflight creates no tag, release or promotion copy.
No rebuild stamps a later merge or reporting commit. No Sprint 26 is selected.

## Historical 0.0.158 publication authorization

On 2026-10-17, after testing `v0.0.157-visual-refinement-alpha.2`, the owner
wrote: "Alright, I have some changes to make, but this is an EXCELLENT
revision. Please merge this into master and cut a new full release. The next
stage of revisions are minor and can go when we have more summons added to
revise." The release is 0.0.158, one above the published 0.0.157. Draft
Sprint 25 (PR #49) renumbers when it releases, as the owner chose for
v0.0.157.

It ships the Expanded Summoning visual refinement
(`codex/expanded-summoning-visual-refinement-pilot`), which already carries
`master` through PR #54, the Brown-Fur startup-order and CotW controller
save-loading repair. `tools/validate_expanded_summoning_visual_refinement_release158.py`
admits, relative to released v0.0.157 master `2eed3944f6`:

- no identity change;
- the seven visual-refinement and thirteen startup-repair production files;
- the version metadata;
- six compile items;
- 57 assets modified in place to their pinned reviewed bytes;
- its own static record.

The historical sprint and release validators stay as they were. The refined
Tyrannosaurus and Deinonychus supersede the Sprint 24 art hashes only through
this validator.

The branch passes one Sprint-level release candidate gate and a guarded
creature review on that candidate. Its PR then merges normally, and the
exact-reference publisher runs from clean, pushed `master`:

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir `
  'C:\Dev\KingmakerGunslingerLab\private\refresh-2026-10-15\extracted\KingmakerGunslinger-private-build-references' `
  -Publish -ConfirmReleaseReady
```

The owner's in-game visual review is accepted provisionally for this release
("Let's assume that your repairs are good. Move forward with them."). Known
limits are listed in `docs/RELEASE-NOTES-0.0.158.md`.

## Visual refinement alphas — 2026-10-17

- **Alpha 1.** On the owner's instruction ("Please cut an alpha release. No need
  to run all the tests for a normal release."), prerelease
  `v0.0.157-visual-refinement-alpha.1` was published by hand. Its tag is at
  `95a5c2e2f0`, and it carries that commit's exact native-reviewed development
  package (`a2f00e7b…3c5a`).
- **Alpha 2.** On the request "merge master into your current work and build a
  fresh alpha release", `v0.0.157-visual-refinement-alpha.2` followed. Its tag
  is at merge `c035a9ba9f`, with package `81e4f3a9…ed5f`, and a guarded load
  check passed 6/6 on it.

Both were prereleases, never Latest, with `releaseQualification=false`. They
authorize nothing else.

## Controller-load alpha 3 authorization — 2026-10-17

After the output_log2 correction/local ZIP handoff, the owner requested:
"Please put it up in github." This authorizes one public test prerelease of
exact product `39c60fb5e9167f1b2fe35a07b545fa2a2ebaac2e`, including required
review-branch/tag push. Use `Publish-CotwActionBarAlpha.ps1 -AlphaVersion 3`
with its exact receipt/hash/MVID/evidence profile. Tag
`v0.0.157-controller-load-alpha.3`, prerelease / never Latest; reuse the tested
ZIP without rebuilding, full-suite/native rerun or qualification promotion.
The alpha 2 profile/pins remain unchanged and cannot select the new product.
No default-branch merge, stable release, game installation or unrelated alpha
change is authorized. See [notes](RELEASE-NOTES-CONTROLLER-LOAD-ALPHA-3.md).

## Controller-load alpha 2 authorization — 2026-10-17

The owner subsequently asked to push another alpha to test and clarify whether
Gunslinger needs an accompanying change. This authorizes one public test alpha
of exact KMG product `92b442c06dd8d13f246ca006fdda6382b9eb6236`, including its
review-branch/tag push. `Publish-CotwActionBarAlpha.ps1` accepts only its pinned
development receipt, ZIP/DLL/MVID and 39-assertion evidence log. Publish tag
`v0.0.157-controller-load-alpha.2` as prerelease / never Latest; retain
releaseQualification=false. No full suite, rebuild, native run, merge or game
installation is selected. This is the Gunslinger update itself; no companion
Buff Planner or CotW update is part of the repair. See
[alpha notes](RELEASE-NOTES-CONTROLLER-LOAD-ALPHA-2.md). All stable release and
other development-candidate publication guards remain unchanged.

## Brown-Fur alpha 1 authorization — 2026-10-17

After the local repair handoff, the owner explicitly requested: "Could you
publish a new alpha build for me to test please?" This supersedes the earlier
no-publication / remote-write boundary for this one test prerelease only.
It does not authorize a stable release, default-branch merge or installation.

Publish the exact tested product `913dfd6e89386ff233849f06f902ca8ea2de239e`
as **v0.0.157-brown-fur-alpha.1**, following the repository's earlier convention
of a distinct alpha tag over unchanged UMM version metadata. UMM still displays
0.0.157. Do not rebuild, change GUIDs, relabel the binary's test provenance,
mark Latest, or replace any existing stable tag/asset.

This is a narrow owner-authorized **public unqualified preview** of a development
artifact, not a promotion to a qualified release. The stable publisher and its
development-proof refusals remain unchanged. The selected package keeps its
development receipt; only a sanitized alpha manifest is public. Other development
candidates remain private. See [alpha notes](RELEASE-NOTES-BROWN-FUR-ALPHA-1.md).

```powershell
# First commit and push the clean review branch through the project push guard.
./scripts/Test-BrownFurAlpha.ps1
./scripts/Publish-BrownFurAlpha.ps1 -Publish -ConfirmAlphaTestPublication -WhatIf
./scripts/Publish-BrownFurAlpha.ps1 -Publish -ConfirmAlphaTestPublication -Confirm:$false
```

The alpha-specific publisher revalidates the frozen source/receipt/package,
checks exact native evidence identity and remote source ancestry, copies the
ZIP byte-for-byte, tags the product commit, verifies all uploaded draft asset
digests, and only then publishes prerelease / not Latest. It verifies the public
download SHA-256 and unchanged stable Latest. It refuses existing release/tag
collisions and never clobbers an asset. No full build/runtime campaign is selected.

## Qualified Sprint 21 artifact promotion

The October 11 owner closure mission authorizes PR #36 merge and stable release
only after its complete closure ledger qualifies. The policy requires one frozen
candidate pipeline and exact artifact promotion. Use the tested package from its
unchanged candidate checkout; reporting and merge commits have separate provenance.
The explicit promotion path does not compile or replace the tested ZIP:

```powershell
.\scripts\Publish-Release.ps1 -ReuseQualifiedCandidate `
  -CandidateRepositoryRoot '<actual frozen candidate checkout>' `
  -PackagePath '<actual validated Build-Local ZIP>' `
  -QualificationLedgerPath 'planning/SPRINT21-CLOSURE-LEDGER.json' `
  -MergedReleaseCommit '<verified PR36 merge SHA>' `
  -Publish -ConfirmReleaseReady
```

Run from the clean, pushed Sprint 21 reporting branch. The publisher requires the
merged master lineage, unchanged product inputs, exact candidate/source/package
identity, immutable proof hashes, every mandatory coverage row, native fixture
cleanup and exact actual human-installation restoration. It refuses an existing
stable tag/release. Any narrowly authorized inherited failed exception remains
failed and must meet the owner mission's matched-baseline conditions and appear
in the notes/manifest. Earlier release procedures below retain their historical
authorization and original build cadence.

## Historical 0.0.157 publication authorization

On 2026-10-16 the owner reviewed the alternate summoning sources checkpoint
(PR #52, third pass with its evidence matrix) and wrote: "This look close to
ready to go. Pro has some recommendations for finalizations." followed by a
review's narrow correction pass, and then "After that, please merge tihs, push
it to remote, and cut a new release, incrementing the version number." Asked
which number to use because draft Sprint 25 (PR #49) already carries 0.0.157,
the owner chose "0.0.157 (Recommended)": one above the published 0.0.156, with
Sprint 25 renumbering when it releases. That accepts the checkpoint's stated
limits: the owner checklist cases (hotbar binding, saved-shortcut cold load,
a turn-based round, a scroll-required menu) are not run, and the
blocked-family shortcut load and grapple/swallow reset on reload need
maintainer fixtures (see `docs/ALTERNATE-SUMMONING-SOURCES.md`).

The release is 0.0.157. The branch passes one Sprint-level `Build-Local`
release candidate gate and the guarded alternate summoning native scenarios
on that candidate before its PR merges normally; the existing exact-reference
publisher then runs from clean, pushed `master` with the refreshed private
reference bundle:

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir `
  'C:\Dev\KingmakerGunslingerLab\private\refresh-2026-10-15\extracted\KingmakerGunslinger-private-build-references' `
  -Publish -ConfirmReleaseReady
```

## Historical 0.0.156 publication authorization

On 2026-10-15 the owner reviewed the firearm RTWP kill-continuation
checkpoint (second review revision, guarded native run 491/491) and wrote:
"I think this is satisfactory. If you feel the goals set out to be
accomplished in this mission are adequately resolved, I think we can finalize
this and merge it and cut a new release." and then "I explicitly allow it."
That accepts the checkpoint's stated limits: the original within-full-attack
symptom was not reproduced in the save-free fixture, the owner's live RTWP
check was not performed, and the recorded open design questions remain open
(see `docs/FIREARM-RTWP-KILL-CONTINUATION.md`).

The release is 0.0.156, one above the published 0.0.155. The fix branch
passes one Sprint-level `Build-Local` candidate gate and the guarded firearm
native scenario before its PR merges normally; the existing exact-reference
publisher then runs from clean, pushed `master` with the refreshed private
reference bundle (the older bundle lacks `UnityEngine.PhysicsModule.dll`):

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir `
  'C:\Dev\KingmakerGunslingerLab\private\refresh-2026-10-15\extracted\KingmakerGunslinger-private-build-references' `
  -Publish -ConfirmReleaseReady
```

## Historical 0.0.154 publication authorization

On 2026-10-15 the owner reviewed the animal-companion Gunslinger eligibility
checkpoint and wrote: "This is acceptable. Please merge and cut a release."
That accepts the checkpoint's stated limits: Gunslinger keeps Fighter's
selector presentation, no real companion was observed in game, and Call of the
Wild's Eidolon, Phantom and Dragon companions remain a separate follow-up.

The release is 0.0.154, one above the published 0.0.153. The fix branch passes
one Sprint-level `Build-Local` candidate gate before its PR merges normally;
the existing exact-reference publisher then runs from clean, pushed `master`:

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir `
  'C:\Dev\KingmakerGunslingerLab\private\extracted-references\KingmakerGunslinger-private-build-references' `
  -Publish -ConfirmReleaseReady
```

## Supported release baseline

Kingmaker Gunslinger `0.0.120-elemental-races-completion` targets:

- Pathfinder: Kingmaker Enhanced Plus Edition 2.1.7b;
- Unity Mod Manager 0.32.4 API contracts; guarded 0.0.119 runtime testing uses
  UMM 0.33.0.0, with 0.32.4 runtime acceptance deferred to owner testing;
- Harmony 1.2 through `0Harmony12.dll`;
- .NET Framework 4.7;
- C# 7.3;
- Windows Release/AnyCPU.

Game, Unity, Unity Mod Manager, Harmony, Newtonsoft, compiler, save, and local
configuration files are build inputs only. They must not be committed or bundled
as extra binaries in the UMM package.

## Machine prerequisites

Install Git, GitHub CLI, Python 3, Visual Studio 2022 Build Tools with MSBuild
and the .NET desktop build workload, and the .NET Framework 4.7 targeting pack.

Authenticate GitHub CLI once:

```powershell
gh auth login
gh auth status
```

Copy the ignored game-path example:

```powershell
Copy-Item .\GamePath.props.example .\GamePath.props
```

The checked-in example already points to the usual Steam installation:

```text
C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker
```

Unity Mod Manager must already be installed for Kingmaker, including:

```text
Kingmaker_Data\Managed\UnityModManager\UnityModManager.dll
Kingmaker_Data\Managed\UnityModManager\0Harmony12.dll
```

On a fresh Windows installation, remove Mark-of-the-Web from the locally
installed UMM files before .NET Framework contract tests:

```powershell
$umm = 'C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Kingmaker_Data\Managed\UnityModManager'
Get-ChildItem $umm -File -Recurse | Unblock-File
```

## Ordinary release build

From a clean checkout:

```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\build.ps1 `
  -Configuration Release `
  -Clean `
  -Package
```

The build runs version-aware repository validation, the complete
dependency-free test suite, production compilation, strict build-output
validation, SoundBank validation, deterministic ZIP creation, and strict UMM
package validation.

## Historical 0.0.138 publication authorization

On 2026-09-23 the owner reviewed the Better Vendors progression candidate
(PR #22), including its stated limits. The owner then wrote: "Run the final
tests. IF they have issues, please address them. If not, please proceeding
with committing, merging, pushing to remote, incrementing the version number
and cutting a new release."

The candidate already carries 0.0.138, one above the last published 0.0.137,
so the release version is 0.0.138. After the final gates, `master` is
fast-forwarded to the tested branch commit, so the released commit is the one
the guarded runtime check exercised. The existing exact-reference publisher
then runs from clean, pushed `master`.

The five merchant and persistence acceptance areas in
`docs/BETTER-VENDORS-COMPATIBILITY.md` were NOT RUN, because no kingdom-stage
test save was authorized. Asked to confirm, the owner replied: "Yes confirm
those are waived. I will test that in my main game and revisit this thread if
it's broken." The owner also permitted updating the 0.0.138 validator from its
candidate state to this release state. The five areas remain open and must not
be reported as qualified. Save/load compatibility is unverified.

## Historical 0.0.134 publication authorization

The owner accepted Magic Circle PR #19, including the shared lease follow-up,
and explicitly requested finalization, merge to master, push and a new release.
Integrate master 0.0.133 without dropping its Word of Recall Favored Class repair.
Use the existing exact-reference publisher from clean pushed master after the
0.0.134 candidate passes required gates and focused guarded native checks.
Retain exact artifact attribution, prior artwork approval and honest qualification
limits; verify downloaded public assets and restore the normal-play installation.
See `planning/MAGIC-CIRCLE-RELEASE-134.md` and `docs/RELEASE-NOTES-0.0.134.md`.

## Historical 0.0.132 publication authorization

The owner explicitly requested merge to master, push and a public release of
the icon art overhaul on 2026-09-18 ("Please merge to master, push to remote,
and cut a release. I will install that release on my main computer and test
the icons there."). Use version 0.0.132 from merged, clean, pushed master,
with the existing exact-reference publisher and current notes. Preserve the
qualification evidence, honest limits and owner review materials under
reports/icon-overhaul and planning/ICON-OVERHAUL-STATE.md. Focused guarded
runtime checks qualified the versioned artifact before publication; verify
the downloaded public ZIP/checksum/manifest and preserve installed settings
and other mods.

## Historical 0.0.129 publication authorization

The owner accepted the completed Recall/automatic-reader mission and explicitly
requested merge, push and a new full public release on 2026-09-13. Preserve the
accepted candidate's evidence and limitations in Z-RECALL-AND-SMART-SCROLL-STATE.md.

## Historical 0.0.119 publication authorization

The owner subsequently instructed: "You can forego the remaining tests. Please
wrap things up, commit, push to origin, and cut the release." This supersedes
the remaining native-repeat requirements in the original plan below. Preserve
completed results with their exact artifact identities; do not claim the sealed
release binary passed runtime tests that were waived. The existing publisher's
built-in deterministic build/package/provenance checks remain in use.

## Original 0.0.119 release gate plan

The owner authorized a normal public patch from the dedicated repair branch,
without merging or modifying master. Use the existing provenance-checked
`Publish-Release.ps1` with `-ReleaseBranch codex/teleportation-post-release-hardening`
and `-AllowNonDefaultReleaseBranch` only after the same clean committed artifact
passes all 45 required fresh-process launches and the original-configuration
restoration startup. Build twice with the exact reference bundle and compare
ZIP/DLL bytes; the publisher repeats both builds. Recheck public releases before
publication, preserving all historical tags/assets. Download the public ZIP,
checksums and manifest again, independently compare them with the runtime-tested
artifact, validate the strict package and install that exact public ZIP while
preserving existing settings. Curated final evidence belongs in the hardening
report; raw saves/packages/runtime artifacts remain ignored.

## Historical release authorizations

The public master 0.0.115 Share Transmutation fix is incorporated into this
branch. Its authorized publication and its NOT-RUN save-backed gameplay
record remain distinct from this mission's unpublished release checkpoints.

The owner explicitly authorized finalizing, merging, pushing, and publishing
0.0.116. The accepted content candidate passed the complete domain suite,
24-state module boundary matrix, and guarded shop, firearm, and purchase
save/load scenarios. Release sealing reuses that unchanged-source evidence and
checks the rebuilt release artifact through the guarded runtime workflow.
Exact fingerprints and results belong in the task report. Historical 0.0.114
compatibility evidence stays pinned to that package; 0.0.115's separate
save-backed API qualification is not inferred from these content scenarios.

The owner accepted the installed 0.0.117 stabilization candidate and explicitly
authorized finalization, integration with latest master, merge, push and public
release on 2026-09-07. This supersedes the earlier candidate-only boundary.
The prior acceptance handoff and incremental notes remain historical evidence;
the [public release report](ELEMENTAL-RACES-0.0.117-PUBLIC-RELEASE.md) records the
new authorization and final artifact verification.

Where installed UMM targets a newer framework, pass `-ReferenceBundleDir` to
the publisher to use the existing provenance-checked `Build-Local.ps1` path
for both clean deterministic builds. It runs the same source, full domain,
output, SoundBank, and strict package gates. No installed UMM/Harmony files
are changed. If the legacy external Unity output is absent, Build-Local uses
the tracked bundle only after the existing manifest hash check succeeds.

Release 0.0.117 preserves all existing elemental identities and publishes only
nineteen implemented alternate traits. Treacherous Earth and Nereid Fascination
remain registered but unavailable. The accepted artifact passed 1,458 tests,
259 harness preflight checks and 13,847 assertions across 28 guarded processes.
Release sealing preserves gameplay source and assets, compares the new payload
against that accepted artifact, and verifies the rebuilt DLL's final commit
through focused guarded character-creation, native respec and load checks.
Earlier matrix evidence retains its original artifact attribution. The owner's
acceptance does not fabricate individual full-screen checklist results.

The GitHub repository's release branch is `master`. Before publishing, make
`master` the repository's GitHub default branch. The publisher blocks a default
branch mismatch unless `-AllowNonDefaultReleaseBranch` is supplied deliberately.

After explicit owner authorization, create a draft release for inspection:

```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\Publish-Release.ps1
```

Publish only the owner-approved release:

```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\Publish-Release.ps1 `
  -Publish `
  -ConfirmReleaseReady
```

The publisher:

1. requires a clean, fully pushed `master`;
2. verifies GitHub CLI authentication, the remote repository, visibility, and
   version metadata;
3. runs the complete build and package pipeline twice;
4. requires byte-identical package and DLL hashes across both clean builds;
5. validates the final release copy;
6. creates `SHA256SUMS.txt` and `release-manifest.json`;
7. creates and pushes annotated tag `v<Info.json Version>`;
8. creates or refreshes a GitHub release, remaining draft unless `-Publish`
   is supplied; and
9. uploads the actual UMM ZIP, checksum, and manifest.

A published release is immutable project history. The script refuses to replace
one. Any later code change requires a new version, validator entry, changelog
section, and release asset.

## End-user download test

After publication, download the named file under **Assets**. Do not download
GitHub's automatic **Source code (zip)** archive.

```powershell
gh release download v0.0.117 `
  --repo howardreith/KingmakerGunslingerAndOtherTabletopRules `
  --pattern 'KingmakerGunslinger-0.0.117-elemental-char-gen-stabilization.zip' `
  --dir "artifacts\release-download\0.0.117"
```

Drag that downloaded ZIP directly into Unity Mod Manager's Mods tab. The
installed layout must contain one `KingmakerGunslinger` directory with
`Info.json`, `KingmakerGunslinger.dll`, the approved assets, blueprints, and
documentation. The package validator rejects foreign game, UMM, Harmony,
Newtonsoft, compiler, or symbol binaries.
