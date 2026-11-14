# Build, package, and publish

## 0.0.188 publication authorization

**Published (completed).** Stable
[v0.0.188](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.188)
is Latest and not a prerelease. The annotated tag `6bf87d0004` peels to the release commit `097e522f9f`,
which is `origin/master`.

- **Gate.** The complete suite passed 2,971/2,971 with 0 failures.
- **Builds.** Two deterministic exact-reference builds against the pinned UMM 0.31.1 loader references
  produced identical archives:
  - package SHA-256 `64d83955478c3824c5fee485f4189b7d0be1141938a89e70e4cdab037461d412`;
  - DLL SHA-256 `0eb2f0027f1b8639cb8988f24fffda263cf92bfdadd180f40b1aebe79019b151`.
- **Reference bundle.** `artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references`,
  as for v0.0.187.
- **Public download test.** A fresh download of all three assets matched `SHA256SUMS.txt`, and the manifest
  names the release commit `097e522f9fa0a8fdc8a773fb51fc3cfbf91d115c`. The embedded `Info.json` reads 0.0.188 with
  `ManagerVersion` 0.31.1 and `Id` `KingmakerGunslinger`, alongside the DLL hash above (458 files). The published DLL
  references `UnityModManager 0.31.1.0`. Its bound loader members are identical to the tested candidate and all
  present in UMM 0.31.1, 0.32.4 and 0.33.0.
- **Native check of the published bytes.** On UMM 0.31.1 (component-assembled, Harmony 2.3.1.1) in the disposable
  game copy, both sessions ran the published ZIP and passed with in-process DLL `0eb2f002…`. Owner saves, installation
  and settings were unchanged or restored.
  - `alone`, 11 launches (`20261114T031843Z`);
  - `cmi-bugfixes`, 4 launches (`20261114T033120Z`).
  0.32.4, 0.33.0 and Call of the Wild/Favored Class were not repeated on these bytes; their results are on the
  development candidate (same product source).
- **Timing.** The publisher took 1,158 s, after 431 s of release-branch preparation; the post-publication native
  check took 1,055 s.
- **Installation.** The installed mod is untouched and still reads 0.0.140; the publisher performed no
  deployment.
- **Merges.** PR #153 merged as `99fd8fa04a`; release PR #154 merged as `097e522f9f`.

Originally authorized as follows.

On 2026-11-13 Howie wrote: "Thank you. Please merge and cut a new release." PR #153 (UMM 0.31.1 minimum,
pinned loader references and the loader-compatibility matrix) merged as `99fd8fa04a`. No channel was named, so
this follows v0.0.187 as the next stable patch, v0.0.188, from master `99fd8fa04a`.

- **Reviewed product.** Master `99fd8fa04a`. The only product change from v0.0.187 is `Info.json`
  `ManagerVersion` 0.32.4 -> 0.31.1. The DLL is compiled against the pinned UMM 0.31.1 loader references
  (`scripts/loader-reference-pins.json`) instead of the installed loader. `tools/validate_umm_minimum_release188.py`
  pins it:
  - the reviewed product differs from the v0.0.187 tag target only in the Info `ManagerVersion`, with no
    change under `src/KingmakerGunslinger`, `assets`, `assets-source`, `blueprints` or `Directory.Build.props`;
  - the reviewed pins name the verified UMM 0.31.1 `UnityModManager.dll` and `0Harmony12.dll` hashes, and the
    pins and build inputs (`Build-Local.ps1`, `ReferenceProvenance.Common.ps1`, `validate-package.ps1`,
    `DevelopmentCandidate.Common.ps1`, the exact-reference compiler) equal the reviewed master;
  - only the three AssemblyInfo version attributes and the Info and props version values may change, and
    `ManagerVersion` stays 0.31.1;
  - all 7,933 released identities stay exact and in order, and none is appended;
  - the creature catalog is unchanged, with only the owner-withdrawn Stirge withheld (1,208 public choices);
  - the disclosed inherited-template membership still equals v0.0.187's twelve keys;
  - one static release record is added.
  Four executable controls each FAIL: a stray product edit, `ManagerVersion` back at 0.32.4, a changed
  release record and a changed loader pin. The intended tree PASSes.
- **Native evidence.** The development candidate `umm-0311-compat` (`b9400333e5`, ZIP `c82c73ab…`, DLL `7fa421d7…`)
  has the same product source apart from version attributes. Eight sessions passed on UMM 0.31.1, 0.32.4 and
  0.33.0 in a disposable game copy, with exact in-process loader identities (`docs/UMM-LOADER-COMPATIBILITY.md`).
  The released DLL differs from it in its version attributes and commit stamp; any native run on the published
  bytes is recorded with the publication.
- **Publication path.** Use the ordinary exact-reference publisher from clean, pushed master with
  `docs/RELEASE-NOTES-0.0.188.md` and the explicit summon-overhaul-baseline reference bundle. It runs the complete
  gate and two deterministic builds, then publishes.

## 0.0.187 publication authorization

**Published (completed).** Stable
[v0.0.187](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.187)
is Latest and not a prerelease. The annotated tag `191004ddef` peels to the release commit `bfaac01f52`,
which is `origin/master`.

- **Gate.** The complete suite passed 2,971/2,971 with 0 failures.
- **Builds.** Two deterministic exact-reference builds produced identical archives:
  - package SHA-256 `230e9298afdc534d6017fd2914101649827d7d53ffaed10056361cbade1fa48c`;
  - DLL SHA-256 `7b23241c3a219153e688041c905f8485ea70a7f36abb6f6a94e8beb7bbafc368`.
- **Reference bundle.** `artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references`,
  as for v0.0.185. A first publisher attempt with the `private/refresh-2026-10-15` extracted bundle stopped
  after 5 s on the provenance check (`Managed/UniRx.dll` missing), before any build, tag or upload.
- **Public download test.** A fresh download of all three assets matched `SHA256SUMS.txt`, the manifest
  names the release commit `bfaac01f522080aa43a8f89b9c119de1e1a744ae`, and the embedded `Info.json` reads
  0.0.187 with `DisplayName` "Mr. Bard's Tabletop Rules Expansion", `Id` `KingmakerGunslinger`, and the
  DLL hash above (458 files in the package).
- **Timing.** The publisher took 1,102 s; with the 780 s release-branch preparation the release spent about
  1,882 s, about 80 s over its 30-minute planning budget.
- **Installation.** The installed mod is untouched and still reads 0.0.140; the publisher performed no
  deployment.
- **Merges.** PRs #149, #148 and #150 merged as `d949d83c9b`, `f7a9143a95` and `ca6f4c0543`. The owner
  merged release PR #151 as `bfaac01f52`.

Originally authorized as follows.

On 2026-11-13 Howie wrote: "Please merge all 3 PRs and then cut a new release." PRs #149 (project-level
Claude Code attribution settings), #148 (Unity Mod Manager display name) and #150 (commit-id map after
the 2026-11-13 history rewrite) merged in that order. No channel was named, so this follows v0.0.186 as
the next stable patch, v0.0.187, from master `ca6f4c0543`.

- **Reviewed product.** Master `ca6f4c0543`. The only product change from v0.0.186 is the `Info.json`
  `DisplayName`, from "Kingmaker Gunslinger" to "Mr. Bard's Tabletop Rules Expansion". The mod `Id`,
  assembly, entry point, settings keys and identities are unchanged.
  `tools/validate_umm_display_name_release187.py` pins it:
  - the reviewed product differs from the v0.0.186 tag target only in the Info `DisplayName`, with no
    change under `src/KingmakerGunslinger`, `assets`, `assets-source`, `blueprints` or `Directory.Build.props`;
  - only the three AssemblyInfo version attributes and the Info and props version values may change;
  - all 7,933 released identities stay exact and in order, and none is appended;
  - the creature catalog is unchanged, with only the owner-withdrawn Stirge withheld (1,208 public choices);
  - the disclosed inherited-template membership still equals v0.0.186's twelve keys;
  - one static release record is added.
  Three executable controls each FAIL: a stray product edit, a different display name and a changed
  release record. The intended tree PASSes.
- **Native evidence.** None. The compiled source changes only in its version attributes. The new name is
  not observed in the Unity Mod Manager window; this is disclosed in the notes.
- **Commit ids.** v0.0.186 and earlier tags were moved by the 2026-11-13 rewrite. The ids in this section
  and in the release187 validator are post-rewrite ids. Older sections keep their original ids; translate
  them with `docs/history/commit-id-map-2026-11-13.tsv`.
- **Preparation.** On the release branch, the repository wrapper (dispatched to the release187 validator,
  PASS) plus the complete suite (2,971/2,971, 0 failures) took 780 s.
- **Publication path.** Use the ordinary exact-reference publisher from clean, pushed master with
  `docs/RELEASE-NOTES-0.0.187.md`. It runs the complete gate and two deterministic builds, then publishes.
  Planning budget: 30 minutes, excluding necessary safe completion.

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -ReleaseNotesPath 'docs\RELEASE-NOTES-0.0.187.md' -Publish -ConfirmReleaseReady
```

## 0.0.186 publication authorization

**Published (completed).** Stable
[v0.0.186](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.186)
is Latest and not a prerelease. The tag peels to the release commit `0d5f95e3a5`, which is
`origin/master`.

- **Builds.** Two deterministic exact-reference builds produced identical archives:
  - package SHA-256 `6c1aad978cd31b47518b1fe9b59317fd9ea169fd4e0c3b88b8538d3c125373b4`;
  - DLL SHA-256 `4450f6fa6ed9ebcb0bdcb02d996a0dd0c62bcd4e39a1be81b1b16ad1f3596bec`.
- **Reference bundle.** The lab's private extracted-references bundle, as for v0.0.185. Its path is
  machine-local and deliberately not recorded here.
- **Public download test.** A fresh download of all three assets matched `SHA256SUMS.txt`, the manifest
  names the release commit `0d5f95e3a5cb11eff25ceb216681125356bef3de`, and the embedded `Info.json`
  reads 0.0.186 with the DLL hash above (458 files in the package).
- **Installation.** The installed mod is untouched and still reads 0.0.117, 136 files, DLL
  `FD2FC61C…`, settings `A06601C5…` at `Archive`, beside the same eleven mod folders.
- **Merges.** The seven repair PRs merged as above; release PR #146 merged as `0d5f95e3a5`.
- **One prerequisite, recorded because it changed local state.** The publisher's
  `git fetch --prune --tags origin master` failed: eighteen local tags (v0.0.136-v0.0.153) still held
  the pre-lineage-rewrite SHAs and would have been clobbered. They were aligned to the remote with
  `git fetch --tags --force origin`, which only updates local refs and touches no remote. The two
  local-only tags (`safety/sprint21-checkpoint-b1ca8539`, `v0.0.30-runtime-qualified`) were deliberately
  preserved; `--prune-tags` was NOT used.

Originally authorized as the next stable patch after v0.0.185, from master `2c7ff93be9` after the
seven v0.0.182-audit repair PRs.

- **Reviewed product.** Master `2c7ff93be9eb`. The seven PRs were reviewed at their own heads and merged
  in the order #134 -> #136 -> #137 -> #139 -> #135 -> #138 -> #140, each pinned with
  `--match-head-commit`. `tools/validate_gunslinger_audit_repairs_release186.py` pins the release:
  - only the three AssemblyInfo version attributes and the Info and props version values may change;
  - all 7,933 released identities stay exact and in order, and none is appended;
  - the creature catalog is unchanged, with only the owner-withdrawn Stirge withheld (1,208 public choices);
  - the DISCLOSED inherited-template membership must still equal v0.0.185's twelve keys, so the release
    cannot quietly change what it discloses as pre-existing;
  - one static release record is added.
- **Gate.** Complete suite 2,971/2,971 with 0 failures on `1de8b5c7305d`, and 2,954/2,954 on post-merge
  master `ffc40bd2b` before #140. Focused gate 33/33 on
  `selection-boundary;crafting-compensation;gunsmithing;ammunition`.
- **Native evidence.** Guarded save-free runs with the installation snapshot-restored and verified after
  each: `disposable-gunslinger-scatter-dead-shot` 50/0 on `6c866f83a1dd`;
  `observe-feature-module-settings` staged writable and read-only on `468391fc06a3`;
  `disposable-firearm-dependent-feats` 17/17 and `disposable-paper-cartridge-crafting-vendors` 13/13 on
  `1de8b5c7305d`. The crafting budget cap was also demonstrated against negative control
  `a6fcd6e51d3b`, where the observation FAILS. No save was named in any run.
- **Release triage.** GS-REP-0002 (inherited-template set) is **pre-existing and unchanged** — membership
  byte-identical to v0.0.185 — so it does not block. Disclosed in `KNOWN-ISSUES.md` and the notes. The two
  failing feature-module publication gates reproduce identically on released master `a3912f569ac5`, so
  they are pre-existing; their mechanism is untraced and they are disclosed.
- **Disclosed limits.** In `KNOWN-ISSUES.md` and the notes: the Cord clamp is not natively observed, the
  composition repair's neutrality is a source inference for the measured route, and the selection-boundary
  evidence uses a SYNTHETIC foreign row because no installed mod supplies one.
- **Publication path.** Ordinary exact-reference publisher from clean, pushed master.

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -ReleaseNotesPath 'docs\RELEASE-NOTES-0.0.186.md' -Publish -ConfirmReleaseReady
```

## 0.0.185 publication authorization

**Published (completed).** Stable
[v0.0.185](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.185)
is Latest and not a prerelease. The annotated tag `49bd4a6bc8` peels to the
release commit `c136527a99`.

- **Gate.** The complete suite passed 2,950/2,950 with 0 failures.
- **Builds.** Two deterministic exact-reference builds produced identical
  archives:
  - package SHA-256
    `c48be70e6aef546e23c8414abec3090af77225248e150eef3bad97259eec5ccf`;
  - DLL SHA-256
    `5dc21d40a1a3474d3a0f6fc4cbb8012c664fd0848d280ca62a3109fab4dfa694`;
  - MVID `8c11f422-481b-4a0b-8573-b75bb786fa8f`.
- **Reference bundle.** `artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references`.
- **Public download.** A fresh download matched `SHA256SUMS.txt`, the manifest names the release commit, and the
  embedded `Info.json` reads 0.0.185 with the matching DLL.
- **Timing.** The publisher took 1,363 s; with the 421 s release-branch preparation the release spent about
  1,784 s, within its 30-minute planning budget.
- **Installation.** The installed mod is untouched and still reads 0.0.140.
- **Merges.** PR #143 was merged as `70eeed101a`. The owner merged release PR #144 as `c136527a99`.

On 2026-11-12 Howie wrote: "Repair that one issue then merge and cut a release please." The review P3 (stale
Eastern manifest paragraph) was repaired as `4455a418c6`. PR #143 was marked ready and merged as `70eeed101a`. No
channel was named, so this follows v0.0.184 as the next stable patch, v0.0.185, from master after PR #143.

- **Reviewed product.** Master `70eeed101a`; its tree equals the reviewed branch head `4455a418c6`.
  `tools/validate_regional_vendor_withdrawal_release185.py` pins it:
  - only the three AssemblyInfo version attributes and the Info and props version values may change;
  - all 7,933 released identities stay exact and in order, and none is appended;
  - the creature catalog is unchanged, with only the owner-withdrawn Stirge withheld (1,208 public choices);
  - one static release record is added.
- **Native evidence.** None. Focused gate 128 of 2,950 with 0 failures, production compilation and the decompiled
  2.1.7b `SharedVendorTables` contract only. The native rows in `planning/regional-vendor-melee-withdrawal/STATUS.md`
  stay NOT RUN.
- **Disclosed migration limits.** The sold-back decrement and the missing-history case are in `KNOWN-ISSUES.md`
  and the notes.
- **Preparation.** On the release branch, the repository wrapper (dispatched to the release185 validator, PASS)
  plus the complete suite (2,950/2,950, 0 failures) took 421 s.
- **Publication path.** Use the ordinary exact-reference publisher from clean, pushed master with
  `docs/RELEASE-NOTES-0.0.185.md`. It runs the complete gate and two deterministic builds, then publishes.
  Planning budget: 30 minutes, excluding necessary safe completion.

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -ReleaseNotesPath 'docs\RELEASE-NOTES-0.0.185.md' -Publish -ConfirmReleaseReady
```

## 0.0.184 publication authorization

**Published (completed).** Stable
[v0.0.184](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.184)
is Latest and not a prerelease. The annotated tag `1b2eaa3bee` peels to the
release commit `6d033bc4d9`.

- **Gate.** The complete suite passed 2,949/2,949 with 0 failures.
- **Builds.** Two deterministic exact-reference builds produced identical
  archives:
  - package SHA-256
    `1496ead5dbfd8227943fd0fc068aca881f830b52d498ab8357ecd5d764332c76`;
  - DLL SHA-256
    `9251118ef487e6393628b70a8198f20a238d533321a6dc10724d86f27e185fee`;
  - MVID `7b0d2325-988c-4dd2-8aef-c053b1899904`.
- **Reference bundle.** `artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references`
  (the verified bundle with `UniRx.dll`, used for every development candidate of this work).
- **Public download.** A fresh download matched `SHA256SUMS.txt`, the manifest names the release commit, and the
  embedded `Info.json` reads 0.0.184 with the matching DLL.
- **Timing.**
  - The publisher took 1,579 s; a peer session's short package build overlapped part of it.
  - Preparation: the release-branch repository wrapper plus complete suite took 603 s.
  - Total about 2,182 s, about 380 s over the 30-minute planning budget.
- **Installation.** The installed mod is untouched and still reads 0.0.140.
- **Merges.** PR #133 was merged as `9469105336`. The owner merged release PR #141 as `6d033bc4d9`.

On 2026-11-12 Howie wrote: "Approved. Please merge and cut a release." PR #133 was marked ready and merged as
`9469105336`. No channel was named, so this follows v0.0.183 as the next stable patch, v0.0.184, from master after
PR #133.

- **Reviewed product.** Master `9469105336`; its tree equals the reviewed branch head `75621038a7`.
  `tools/validate_hide_original_summon_elemental_release184.py` pins it:
  - only the three AssemblyInfo version attributes and the Info and props version values may change;
  - all 7,933 released identities stay exact and in order, and none is appended;
  - the creature catalog is unchanged, with only the owner-withdrawn Stirge withheld (1,208 public choices);
  - one static release record is added.
- **Native evidence.** Desktop development candidate `60d2d1b618` passed one guarded run, 22/22: mixed spellbook
  and PC metamagic tab hidden and revealed, preparation/undo, revealed learning with native replacement and normal
  cancellation. Candidate `0d944a262f` observed the default-hidden learning list and the reveal after reopening.
  Earlier candidates keep their FAIL results. Nothing was re-executed on this version-stamped build. Controller
  coverage is compile, Harmony target and shared-policy tests only. Details:
  `planning/hide-original-summon-elemental/STATUS.md`.
- **Publication path.** Use the ordinary exact-reference publisher from clean, pushed master with
  `docs/RELEASE-NOTES-0.0.184.md`. It runs the complete gate and two deterministic builds, then publishes.
  Planning budget: 30 minutes, excluding necessary safe completion.

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -ReleaseNotesPath 'docs\RELEASE-NOTES-0.0.184.md' -Publish -ConfirmReleaseReady
```

## 0.0.183 publication authorization

**Published (completed).** Stable
[v0.0.183](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.183)
is Latest and not a prerelease. The annotated tag `017930dd1d` peels to the
release commit `6365ddb261`.

- **Gate.** The complete suite passed 2,948/2,948 with 0 failures.
- **Builds.** Two deterministic exact-reference builds produced identical
  archives:
  - package SHA-256
    `018b41f12b83091e603ef59b3816c687abaa4ec0371a78d157babaf64d8c6288`;
  - DLL SHA-256
    `e8b03b81b62f75e69f731122eff0a21ac37afc40ebca7c4c9077dabce5a40e81`;
  - MVID `0d07e114-b0a6-4991-8c86-a7294bcafc32`.
- **Public download.** The asset matched `SHA256SUMS.txt`, the manifest names the release
  commit, and the embedded `Info.json` reads 0.0.183 with the matching DLL.
- **Preparation.** The first complete-suite run on the release branch had 1 failure of 2,948:
  `feature-modules.runtime-matrix` still pinned the observer's spear loot-row count at 6, which #129
  had changed to 5. The correction is test-only and shipped in release PR #130.
- **Timing.** The publisher took 1,386 s. With the preparatory complete suites (55 and 61 s) and
  the repository wrapper (388 s), the release spent about 1,890 s, about 90 s over its 30-minute
  planning budget.
- **Installation.** The installed mod is untouched and still reads 0.0.140.
- **Merges.** PR #129 was merged as `57d8640323`. The owner merged release PR #130 as `6365ddb261`.

On 2026-11-11 Howie wrote: "This is acceptable. Please merge this and cut a new release." PR #129 was marked
ready and merged as `57d8640323`. No channel was named, so this follows v0.0.182 as the next stable patch,
v0.0.183, from master after PR #129.

- **Reviewed product.** Master `57d8640323`; its tree equals the reviewed branch head `121623b5be`.
  `tools/validate_gunslinger_playtest_release183.py` pins it:
  - only the three AssemblyInfo version attributes and the Info and props version values may change;
  - all 7,933 released identities stay exact and in order, and none is appended;
  - the creature catalog is unchanged, with only the owner-withdrawn Stirge withheld (1,208 public choices);
  - one static release record is added.
- **Native evidence.** None. The six repairs have focused-test (153 of 2,948, 0 failures), compilation,
  read-only Harmony target (spellbook 40/40, retired-loot 1/1) and engine-contract evidence only. The owner
  accepted the source review; the native rows in `planning/gunslinger-playtest-repairs/STATUS.md` stay NOT RUN.
- **Disclosed migration limits.** The Oleg sold-copy decrement and the engine-added zero-time Thornstep are in
  `KNOWN-ISSUES.md` and the notes.
- **Publication path.** Use the ordinary exact-reference publisher from clean, pushed master with
  `docs/RELEASE-NOTES-0.0.183.md`. It runs the complete gate and two deterministic builds, then publishes.
  Planning budget: 30 minutes, excluding necessary safe completion.

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -ReleaseNotesPath 'docs\RELEASE-NOTES-0.0.183.md' -Publish -ConfirmReleaseReady
```

## 0.0.182 publication authorization

**Published (completed).** Stable
[v0.0.182](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.182)
is Latest and not a prerelease. The annotated tag `6f44d76823` peels to the
release commit `c19d12e095`.

- **Gate.** The complete suite passed 2,942/2,942 with 0 failures.
- **Builds.** Two deterministic exact-reference builds produced identical
  archives:
  - package SHA-256
    `84424d74dc9cd4e39bd1ba25b0a018d1056eeb200e5ad187100f424ac9be771b`;
  - DLL SHA-256
    `2ef86f154dd27d663f77841a104f4bda8dbf3d4cefb802a1415894d8fc5fe858`;
  - MVID `cceb476e-138c-4837-add8-107363c77250`.
- **Public download.** The asset matched `SHA256SUMS.txt`, the manifest names the release
  commit, and the embedded `Info.json` reads 0.0.182 with the matching DLL.
- **Interrupted first attempt.** The first publisher run (about 996 s) was stopped by the
  operator session for low system memory during the reproducibility build, after its primary
  build and complete suite passed. It published nothing and created no tag. Its outputs were
  moved, unchanged, to the git-ignored `artifacts/interrupted-release-attempts/`; its primary
  package hash equals the published one. The publisher refused to overwrite them, which it
  should, and the rerun repeated the whole sequence.
- **Timing.** The successful publisher run took 1,122 s. With the interrupted attempt, the
  refused restart (5 s), the repository wrapper (378 s) and two preparatory complete suites
  (143 s), the release spent about 2,644 s, over its 30-minute planning budget because of the
  repeated run.
- **Installation.** The installed mod is untouched and still reads 0.0.140.
- **Merges.** PR #125 was merged as `3fd5bf5d0b` and PR #126 as `22eea359b6`. Release PR #127
  was merged as `c19d12e095`.

On 2026-11-11 Howie wrote: "Accepted. Please merge, push, and cut a new release with a new version
number." PR #126 was merged as `22eea359b6`. No channel was named, so this follows v0.0.181 as the
next stable patch, v0.0.182, from master after PRs #125 and #126.

- **Reviewed product.** Master `22eea359b6`. `tools/validate_summon_menu_order_release182.py` pins it:
  - only the three AssemblyInfo version attributes and the Info and props version values may change;
  - all 7,894 released identities stay exact and in order, and exactly 39 `KMG.Summoning.Spider.*`
    identities are appended;
  - the creature catalog is unchanged, with only the owner-withdrawn Stirge withheld (1,208 public
    choices);
  - one static release record is added.
- **Native evidence.** Development-candidate results stay with their own artifacts; none was
  re-executed on the version-stamped build:
  - Spider Summoner: `0908e3415fa4`, every RU row closed;
  - menu order and Sunder: `3f170d4a9cea`, 44 PASS / 1 FAIL (census observer defect; reader-only
    re-evaluation PASS);
  - `c6e861a98a93`: initialization FAIL.

  The Bebelith Dismantle Armor withdrawal and the corrected census observer have focused and compile
  evidence only.
- **Publication path.** Use the ordinary exact-reference publisher from clean, pushed master with
  `docs/RELEASE-NOTES-0.0.182.md`. It runs the complete gate and two deterministic builds, then
  publishes. Planning budget: 30 minutes, excluding necessary safe completion.

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -ReleaseNotesPath 'docs\RELEASE-NOTES-0.0.182.md' -Publish -ConfirmReleaseReady
```

## 0.0.181 publication authorization

**Published (completed).** Stable
[v0.0.181](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.181)
is Latest and not a prerelease. The annotated tag `e29f4a7261` peels to the
release commit `daf87b5456`.

- **Gate.** The complete suite passed 2,913/2,913 with 0 failures.
- **Builds.** Two deterministic exact-reference builds produced identical
  archives:
  - package SHA-256
    `6733aea0c131c80a28482ca91ab11a8e1ce23381c7942ed94317929a4dfc8532`;
  - DLL SHA-256
    `c34783de6a05e09841896b765ec298634ff9b85ac24ba69613f171e9b3109678`;
  - MVID `6f7f70db-5d5a-4802-88ea-463aefbf8086`.
- **Public download.** The asset matched `SHA256SUMS.txt`, the manifest names the release
  commit, and the embedded `Info.json` reads 0.0.181 with the matching DLL.
- **Timing.** The publisher took 1,220 s. With the preparatory complete suite (81 s) and the
  repository wrapper (374 s), the release spent about 1,675 s against its 30-minute planning
  budget.
- **Installation.** The installed mod is untouched and still reads 0.0.140.
- **Merges.** PR #122 was merged as `2f5703f7c9`. The owner merged release PR #123 as `daf87b5456`.

On 2026-11-09 Howie wrote: "Merge PR #122 and cut a release". No channel was named, so this
follows v0.0.180 as the next stable patch, v0.0.181, from master after PR #122.

- **Reviewed product.** Master `2f5703f7c9`, the merge of PR #122.
  `tools/validate_cleric_alignment_release181.py` pins it:
  - Only the three AssemblyInfo version attributes and the Info and props version values
    may change.
  - All 7,894 released identities stay exact and in order; none is appended.
  - The creature catalog is unchanged; only the owner-withdrawn Stirge is withheld, leaving
    1,208 public choices.
  - One static release record is added.
- **Owner-accepted exception, carried forward.** The latest native result for this feature
  remains FAIL, 70 PASS / 1 FAIL, on development candidate `efc75b8cc0b0`. Its unresolved
  Races Unleashed census findings are accepted and appear in `KNOWN-ISSUES.md` and the notes.
- **Publication path.** Use the ordinary exact-reference publisher from clean, pushed master
  with `docs/RELEASE-NOTES-0.0.181.md`. It runs the complete gate and two deterministic
  builds, then publishes. Planning budget: 30 minutes, excluding necessary safe completion.
- **Native evidence.** Development-candidate results stay with their own artifacts. None was
  re-executed on the version-stamped build.

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -ReleaseNotesPath 'docs\RELEASE-NOTES-0.0.181.md' -Publish -ConfirmReleaseReady
```

## 0.0.180 publication authorization

**Published (completed).** Stable
[v0.0.180](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.180)
is Latest and not a prerelease. The annotated tag `68bdb3b37d` peels to the
release commit `282666c94b`.

- **Gate.** The complete suite passed 2,894/2,894 with 0 failures.
- **Builds.** Two deterministic exact-reference builds produced identical
  archives:
  - package SHA-256
    `d8b29d1170267f3edba10a5549e1e2832bed2a32683ced8a4035598e05d9c696`;
  - DLL SHA-256
    `671b81246c4cab86afc992e5e665d23e8ede67a3aaa79b59857b15d8e3011315`;
  - MVID `361e7af5-eec8-4de0-ae31-d063caa8819e`.
- **Public download.** The asset matched `SHA256SUMS.txt`, and the embedded
  `Info.json` reads 0.0.180.
- **Timing.** The publisher took 1,406 s. With the preparatory complete-suite runs (80 and
  69 s) and the repository wrapper (32 s for the attempt under the wrong PowerShell, 382 s
  under 5.1), the release spent about 1,969 s against its 30-minute planning budget, about
  170 s over.
- **Installation.** The installed mod is untouched and still reads 0.0.140.

On 2026-11-09 Howie wrote: "Go ahead and cut a release." No channel was named, so this
follows v0.0.179 as the next stable patch, v0.0.180, from master after every merged PR.

- **Reviewed product.** Master `325e10c4a1`, which includes #111 to #119.
  `tools/validate_summon_playtest_release180.py` pins it:
  - Only the three AssemblyInfo version attributes and the Info and props version values
    may change.
  - All 7,868 released identities stay exact and in order; 26 are appended.
  - The creature catalog is unchanged; only the owner-withdrawn Stirge is withheld, leaving
    1,208 public choices.
  - One static release record is added.
- **Stale tests corrected before publication.** The first complete-suite run on master had 2
  failures of 2,894, both stale test expectations, and the correction is test-only.
  `expanded-summoning.correction-mephit-roles` named the call signature from before R4.
  `expanded-summoning.overhaul-atlas-seams` used the Nalfeshnee for its negative control, but
  R7 re-baked that model to a single tile, so the control moved to the Vrock. Every shipped
  mesh still passes the zero-crossing check.
- **Publication path.** Use the ordinary exact-reference publisher from clean, pushed master
  with `docs/RELEASE-NOTES-0.0.180.md`. It runs the complete gate and two deterministic
  builds, then publishes. Planning budget: 30 minutes, excluding necessary safe completion.
- **Disclosed, not repaired.** The notes list the alphabetical-menu observation limits, owner
  viewing and listening, and the open audit findings. Development-candidate native results
  stay with their own artifacts.

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -ReleaseNotesPath 'docs\RELEASE-NOTES-0.0.180.md' -Publish -ConfirmReleaseReady
```

## 0.0.179 publication authorization

**Published (completed).** Stable
[v0.0.179](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.179)
is Latest and not a prerelease. The annotated tag `60dca547d3` peels to the
release commit `b337a73743`.

- **Gate.** The complete suite passed 2,843/2,843 with 0 failures.
- **Builds.** Two deterministic exact-reference builds produced identical
  archives:
  - package SHA-256
    `42daa4169b949109fb015457a514a2ee4b5914eba7c058657a480554ad2c64c4`;
  - DLL SHA-256
    `96140fd89d30d0a9a1125e5b7603d6a46c36b3dda2fdbe830fc8859effdb4290`;
  - MVID `22f933b9-6cab-4746-b3e3-49edf4afaa06`.
- **Public download.** The asset matched `SHA256SUMS.txt`, and the embedded
  `Info.json` reads 0.0.179.
- **Timing.** The publisher took 1,097 s.
- **Installation.** The installed mod is untouched and still reads 0.0.140.

On 2026-11-07 Howie wrote: "Alright. Please cut a release that I will test
with. Thank you." Asked to choose between a test prerelease and a stable
release, he answered: "Stable 0.0.179. I also just merged the other PRs that
were in the repo so make sure everything is incorporated." This authorizes the
next stable patch, v0.0.179, from master after every merged PR.

- **Reviewed product.** Master `9e67c45f46`, which includes #98, #101, #102 to
  #108. `tools/validate_expanded_summoning_closeout_release179.py` pins it:
  - Only the three AssemblyInfo version attributes and the Info and props
    version values may change.
  - All 7,868 released identities stay unchanged, with no additions.
  - The released routes and the 1,217 public choices stay unchanged.
  - One static release record is added.
- **Sprint 41 A-8.** Opening this version and registering its validator is the
  repair A-8 named, so the released 0.0.178 pin stops being the active one.
- **Publication path.** Use the ordinary exact-reference publisher from clean,
  pushed master with `docs/RELEASE-NOTES-0.0.179.md`. It runs the complete
  gate and two deterministic builds, then publishes. Planning budget: 30
  minutes, excluding necessary safe completion.
- **Disclosed, not repaired.** Sprint 41 A-15 and the other open findings,
  Ice Devil visual acceptance, and the not-run persistence matrix are all
  disclosed in the notes. Development-candidate native results stay with
  their own artifacts.

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -ReleaseNotesPath 'docs\RELEASE-NOTES-0.0.179.md' -Publish -ConfirmReleaseReady
```

## 0.0.170 Satyr/Lillend repair publication (completed)

[Stable v0.0.170](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.170)
is published from `43abeedfd223e95dec33a3556cc5606a029e4cd3`, after PR80 and PR81.
The complete2,759-test gate and two deterministic exact-reference builds passed;
public download/checksum/manifest, embedded Info/DLL/MVID, Latest and remote tag
were verified. The first attempt's strict file-count failure remains preserved;
PR81 corrected the declared count for the new release notes without changing
product code. See [exact release evidence](../planning/SATYR-LILLEND-RELEASE170-EVIDENCE.json).
The procedure below is retained as provenance; no additional release run is selected.

On 2026-10-29 the owner reviewed the repair gallery and wrote: "Alright, this
looks good. Please merge, push to remote, and cut a new release." This authorizes
normal integration and the next stable patch, v0.0.170; it selects no Sprint33.
The previously disclosed affected cold save/reload remains NOT_RUN, as do older
release gaps. The v0.0.169 reproducibility waiver is historical and does not
apply to this new release.

Use the ordinary exact-reference publisher from clean, pushed master, with
`docs/RELEASE-NOTES-0.0.170.md`. Its full gate and two deterministic builds are
the selected release campaign (planning budget 30 minutes, excluding necessary
safe completion). Do not add a redundant pre-merge product rebuild or relabel
the 53-assertion development native run as execution on release bytes.
`validate_satyr_lillend_repair_release170.py` pins the owner-reviewed e965cfd885
product, project, source art and exports, admits only version metadata changes,
requires the released 7,511-entry ledger unchanged, and preserves historical
static records. Native-tested product source is 4428109744; e965cfd885 adds
records only. Verify the public ZIP, checksum, manifest, embedded DLL/Info and tag.

## 0.0.169 Sprint 32 publication (completed)

On 2026-10-27 Howie wrote "Merge and release it thank you" after the qualified
PR74 handoff. This authorizes Sprint32's merge and next stable release, not
Sprint33. Stable [v0.0.169](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/releases/tag/v0.0.169) is published from
`d4e1f656971bd2010110c331fa51f65cdf919f7b`. The first full build/package passed. Howie
then explicitly waived further release procedures: two-build reproducibility is
NOT_RUN_OWNER_WAIVED. Public assets and exact protected intake are verified.
PR77 is accepted on master; no new artifact was built after the waiver.
Earlier approved omissions/adaptations remain. The default procedure below is
retained for provenance, not an instruction to resume waived testing.

The release validator pins all production code (including observers), project,
assets/source art and ledger to qualified87 sourceefd237d1. Only the three
AssemblyInfo version attributes and corresponding Info/props version values may
change. The released168 ledger remains an exact prefix:7372+139=7511 identities.
The qualified public census is1189 choices; no historical evidence is rewritten.

Use the existing ordinary exact-reference publisher from clean pushed master,
with explicit169 notes, after review and focused positive/refusal checks of this
metadata-only adapter. The product already passed the complete qualified87 gate;
the publisher runs the complete release gate and two deterministic builds,
skipping domain tests only on the second reproducibility build. No extra
pre-merge product rebuild or native campaign is implied by metadata/build stamps.
Native evidence stays attributed to its original artifacts; release DLL/MVID
will differ. Public download, checksum, embeddedDLL/Info and tag/commit are verified.

The exact-reference publisher uses a primary immutable candidate and a separate
`-reproducibility` sibling. Its second Build-Local call explicitly pairs
`-SkipDomainTests` with `-ReproducibilityBuild`, requiring the first validated
full-suite receipt, exact source and references. Both compile through one bounded
scratch path because the DLL embeds its PDB path; each archives its own exact
output. Existing candidate outputs and incomplete primary scratch still refuse
overwrite. The publisher verifies that the first package, receipt and complete
archive remain unchanged after the second build. Run under Limited Interactive
Howard, as the offline runtime-safety fixtures correctly refuse elevation.

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -ReleaseNotesPath 'docs\RELEASE-NOTES-0.0.169.md' -Publish -ConfirmReleaseReady
```

This grants no waiver to prior167/168 NOT_RUN gaps. Keep their disclosures and
the finite Sprint32 evidence limits in the release notes and active record.

## 0.0.168 Sprints 30-31 follow-up publication authorization

On 2026-10-25 the owner reviewed the follow-up checkpoint (branch
`codex/s30-31-rules-and-model-refinement`), asked for a true lesser globe and a
fixed Hound sword grip, and wrote "When you're done here please proceed with
finalization, merging, pushing, and releasing a new version. You do not need
another approval from me." That covers the v0.0.168 release validator and
Sprint-level candidate gate, merging the follow-up PR into master, publishing
through `Publish-Release.ps1`, and checking the public download. The release is
0.0.168, one above the published 0.0.167; the publisher checks the tag again.

`tools/validate_expanded_summoning_sprints3031_followup_release168.py` admits,
relative to released v0.0.167 master `48e6fb4abf`:
- no identity change;
- the pinned production files and the version metadata;
- the released project file, unchanged;
- the rebuilt Lemure, Dretch and Hound Archon exports and source art, and the
  added sculpt module, each pinned to its reviewed bytes;
- its own static record.

The released roster and 1,179-choice census still hold. The historical sprint
and release validators stay as they were, including the v0.0.167 validator
that pins the released art. The release follows the v0.0.166 route: one
Sprint-level release candidate gate, a normal merge, then the ordinary
exact-reference publisher from clean, pushed `master`. Native evidence comes
from development candidates of the same product source, listed in
`docs/SPRINT30-31-REVIEW-FOLLOWUP.md`; the published DLL differs from them in
version metadata and build stamps. Known limits are listed in
`docs/RELEASE-NOTES-0.0.168.md`.

## 0.0.166 Druid rows and giant sizes publication authorization

On 2026-10-24 the owner reviewed the Druid duplicate and giant size fixes
(PR #71) and wrote "Could you go ahead and push, merge, and do a new release of
this?" That covers the v0.0.166 release validator and Sprint-level candidate
gate, merging PR #71 into master, publishing through `Publish-Release.ps1`, and
checking the public download. The release is 0.0.166, one above the published
0.0.165; the publisher checks the tag again.

`tools/validate_expanded_summoning_druid_giant_sizes_release166.py` admits,
relative to released v0.0.165 master `00ef53dd82`:
- no identity change;
- the eight pinned production files and the version metadata;
- the released project file, unchanged;
- no asset or source-art change;
- its own static record.

The released roster and 1,156-choice census still hold. The historical sprint
and release validators stay as they were. The release follows the v0.0.165
route: one Sprint-level release candidate gate and guarded reviews on that
candidate, a normal merge, then the ordinary exact-reference publisher from
clean, pushed `master`. The published DLL differs from the gated candidate's
only in its build stamps. Known limits are listed in
`docs/RELEASE-NOTES-0.0.166.md`.

## 0.0.165 cleanup follow-up publication authorization

On 2026-10-24 the owner reviewed the follow-up corrections (PR #69) and wrote
"Go ahead and merge and release please." That covers the v0.0.165 release
validator and Sprint-level candidate gate, merging PR #69 into master,
publishing through `Publish-Release.ps1`, and checking the public download.
The release is 0.0.165, one above the published 0.0.164; the publisher checks
the tag again.

`tools/validate_expanded_summoning_cleanup_followup_release165.py` admits,
relative to released v0.0.164 master `114564ac71`:
- no identity change;
- the six pinned follow-up production files and the version metadata;
- the released project file, unchanged;
- the ten pinned modified and four pinned added assets and source-art files,
  at exactly their reviewed bytes;
- its own static record.

The released roster and 1,156-choice census still hold. The historical sprint
and release validators stay as they were.

The branch passes one Sprint-level release candidate gate and guarded reviews
on that candidate. Its PR then merges normally, and the ordinary
exact-reference publisher runs from clean, pushed `master`:

```powershell
.\scripts\Publish-Release.ps1 -ReferenceBundleDir '<private build references>' `
  -Publish -ConfirmReleaseReady
```

The publisher's build stamps the merge commit and its own PDB path, so the
published DLL differs from the gated candidate's in those stamps only; v0.0.158
and v0.0.164 shipped the same way. Known limits are listed in
`docs/RELEASE-NOTES-0.0.165.md`.

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
- the 74 pinned Cleanup 2 production files and the version metadata;
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
