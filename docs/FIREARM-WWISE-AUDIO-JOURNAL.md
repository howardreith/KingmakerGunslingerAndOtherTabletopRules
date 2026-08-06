# Firearm Wwise audio journal

## 2026-08-05 — mission start

The current authority is commit `871609a` at version `0.0.70`. The original
worktree is dirty only because of unrelated untracked `KingmakerGunslinger.zip`
and `.z01` through `.z08` files. They were not changed. Work continues in an
isolated `codex/firearm-wwise-audio` worktree.

The installed Kingmaker 2.1.7b `Assembly-CSharp.dll` exposes:

- `bool AkSoundEngine.IsInitialized()`
- `void AkBankManager.LoadBank(string, bool, bool)`
- `void AkBankManager.UnloadBank(string)`
- `void AkBankManager.DoUnloadBanks()`
- direct `AkSoundEngine.LoadBank(...)` overloads returning `AKRESULT`
- `string AkBankHandle.bankName`
- `string AkBankHandle.relativeBasePath`
- `string AkBasePathGetter.GetPlatformBasePath()`
- `string AkBasePathGetter.GetValidBasePath()`

Reflection-only inspection encountered dependency-resolution faults after
these signatures, so overload enumeration and call-site behavior remain an
active bounded inspection item; no runtime code will guess the missing pieces.

No Wwise executable was found in standard Audiokinetic Program Files,
ProgramData, per-user Audiokinetic, PATH, environment, or registry locations.
This does not yet block source work. Release bank generation remains gated on
an exact Wwise 2016.2.x authoring executable and its accepted license state.

Existing source confirms Unity clips are loaded from
`kingmakergunslinger.firearms`, played on a `KMG_FirearmAudio` child through
`AudioSource.PlayOneShot`, and asserted by a guarded runtime scenario. These
are invocation diagnostics only and will be removed after Wwise routing.

Next: strict catalog/manifest/staging implementation and focused tests.

## 2026-08-05 — initial contract checkpoint

Added the centralized five-event catalog plus strict manifest validation and a
pure-root staging service. No release manifest or bank exists yet. The stager
hardcodes the only permitted filename, canonicalizes both roots, verifies the
packaged SHA-256, skips an identical destination, replaces only the exact
destination through a same-directory temporary file, reverifies it, and cleans
only its own temporary file.

Evidence:

- `scripts/test-domain.ps1 -Configuration Release -Clean`: PASS, 893/893.
- `scripts/Build-Local.ps1 -ReferenceBundleDir C:/Dev/KingmakerGunslingerLab/private/extracted-references/KingmakerGunslinger-private-build-references`: PASS.
- Exact-reference compile candidate, build-output validation, deterministic
  package creation, and strict package validation: PASS.
- Local package SHA-256: `30c5776fddaaaf162f294eef9cf1627cd4874e527a8c55ba2270e15295305456`.
- DLL SHA-256: `5bfbced10513013f26d8ae034e987ce1009b2dc3721414a9f6a8f4ce50f522ce`.

The package intentionally contains no Wwise bank yet. Next: add exhaustive
manifest/staging tests and finish exact PostEvent/lifecycle inspection.

## 2026-08-06 — source-complete external-authoring gate

The authorized push script now parses but refused checkpoint `36efee6` because
its configured expected origin remains the placeholder
`git@github-kmg:OWNER/REPOSITORY.git`, while this worktree's origin is
`git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
No direct push or alternate network path was used.

Implemented native Wwise runtime, fail-soft diagnostics/retry, global and
selected-unit previews, exact post-commit routing for ordinary attacks,
Scatter, Dead Shot, Startling Shot, Menacing Shot, and Stop Bleeding, and
removed Unity firearm playback plus the AudioModule reference. Added the
save-free `disposable-firearm-wwise-audio` guarded scenario. Native sound-field
IL inspection proves local zero/empty values fall back through Prototype, so
crossbow combat sound suppression is intentionally pending authentic-bank
auditory evidence rather than risking qualified presentation.

Added deterministic Wwise source preparation, 2016.2 authoring handoff,
production bank validator, strict release/source-only package modes, package
hash validation, and forbidden `Init.bnk`/extra `.bnk`/`.wem` rejection.

Final source evidence:

- Runtime scenario preflight: PASS, 86 checks.
- Repository validator: PASS.
- Domain/reflection suite: PASS, 898/898.
- Exact-reference clean Release compile: PASS.
- Build-output validation: PASS.
- Source-only deterministic package and strict package validation: PASS.
- Package SHA-256: `6c3b74ed5ca1dc32d87c3677ac8232005013ae5243bf81312b98dd800fa25669`.
- DLL SHA-256: `451e5a7c89bd5193b72feaade29a4214814b69f4528041bb6d48648058940fab`.
- Deterministic five-WAV preparation: PASS.
- `git diff --check`: PASS (line-ending notices only).

No Wwise authoring/CLI executable is present in PATH or the standard
`C:\Program Files (x86)\Audiokinetic` and `C:\Program Files\Audiokinetic`
trees. No bank or production manifest was fabricated. Runtime execution is not
meaningful until a genuine 2016.2.x bank exists; the dedicated scenario will
otherwise fail closed with explicit diagnostics.

Next external action: make an actual Wwise 2016.2.x authoring/CLI installation
available, run `scripts/audio/Prepare-FirearmWwiseSources.ps1`, author and
generate the exact Windows `KMG_Firearms` bank per the handoff, curate the bank
and manifest, run `scripts/Validate-FirearmSoundBank.ps1`, then resume Codex for
release packaging and guarded repeated runtime qualification.

Source-complete checkpoint commit: `3cbfe4a`. The authorized push attempt
failed with the same policy remote mismatch (placeholder expected origin versus
the repository's actual origin). The commit is local and unpushed; no direct
push was attempted.

## 2026-08-06 — Owlcat authoring project checkpoint

Resumed from `e34c1a0`. The branch and worktree were clean, and
`origin/codex/firearm-wwise-audio` also pointed to `e34c1a0`.

Verified Wwise 2016.2.6.6153 under
`C:\Audiokinetic\Wwise_2016.2.6.6153`; both x64 `Wwise.exe` and `WwiseCLI.exe`
report file/product version `2016.2.6.6153`.

Curated only the `.wproj` and 23 `.wwu` files from the user-generated
Owlcat.Templates 1.14.4 `kmsoundvoicemod` seed. No generated mod binary or UMM
source was copied. All copied Work Units hash-identically to the seed. The
project declares schema 75, Wwise `v2016.2.6`, build `6153`, one Windows
platform, and native `WEAPONS` bus
`{90EB9CC7-BB9C-42E0-9B57-62AC34459906}`.

The project name was curated from `KMTemplate` to
`KingmakerGunslingerFirearms`; all supplied GUIDs were preserved. The new
validator checks version/platform, exact bus identity, canonical source-map
events, and absence of generated `.bnk`/`.wem`/`Init.bnk` artifacts. Its
optional `-RequireAuthoredObjects` gate intentionally fails until Wwise creates
the five events and `KMG_Firearms` bank.

`WwiseCLI.exe -help` did not return during a bounded 20-second noninteractive
probe and was terminated by timeout. No GUI automation was attempted. Next is
the minimal human Wwise GUI sequence in the authoring README.

Checkpoint validation:

- Authoring scaffold validator: PASS.
- Byte-for-byte comparison of all 23 copied Work Units to seed: PASS.
- Authored-object gate: expected FAIL, missing first canonical event; confirms
  the seed is not falsely represented as an authored bank.
- Repository validator: PASS.
- Initial sandboxed clean domain run: FAIL only at isolated temporary-bank
  `File.Replace` with `UnauthorizedAccessException`; rerun with repository test
  filesystem permission: PASS, 898/898.
- Exact-reference clean Release build and focused blueprint tests: PASS.
- Build-output validation: PASS.
- Source-only package creation and strict validation: PASS.
- Source-only package SHA-256:
  `0EC7343BAECE3A97F5426A2FFEB0B39630D83D264B98759F32C6C533D4E26B17`.
- Built DLL SHA-256:
  `ede0b47f79b428bacfa5cedb78ba5ff971283a62597baf2ad25133f4a0044ca5`.

Authoring checkpoint commit:
`4b8cf93afa8d2797815eb26df4325936229abe68`. The corrected authorized push
script completed successfully and verified
`origin/codex/firearm-wwise-audio` at that exact commit. No alternate push
mechanism was used.

## 2026-08-06 — authentic SoundBank and release-package checkpoint

The human-authored project validator passed with `-RequireAuthoredObjects`.
Wwise 2016.2.6.6153 generated the Windows banks with zero warnings/errors. The
generated `Init.bnk` remains only in the ignored authoring output and was never
curated, staged, or packaged.

Independent generated-metadata evidence:

- `KMG_Firearms.txt` lists exactly five canonical Events and five in-memory
  `KMG_Firearms_SFX` objects.
- `SoundbanksInfo.xml` identifies Windows, SoundBank format version 120,
  exactly one `KMG_Firearms.bnk` entry, all five canonical Events, and five
  `IncludedMemoryFiles` matching the approved source map.
- `StreamedFiles` and `MediaFilesNotInAnyBank` are empty.
- The generated Windows directory contains no `.wem` files.
- Each Wwise `Originals\SFX` WAV hashes exactly to its approved processed hash
  in `audio-manifest.json`.
- `KMG_Firearms_SFX` routes to the exact native `WEAPONS` bus
  `{90EB9CC7-BB9C-42E0-9B57-62AC34459906}` and contains five non-streaming
  Sound SFX objects.

Production artifacts:

- `KMG_Firearms.bnk`: 1,208,670 bytes.
- Bank SHA-256:
  `FF9245DDCEEAC12CF9759EE9BF34E79A817F1A07B2E82ED01C7516EF3666D9F4`.
- Manifest SHA-256:
  `DAEC8B174E3586ED20DD31C4146C651AEDFB79E76F74EB3FAEC4687F870935A9`.

Added deterministic metadata/source validation and curation. Real release data
exposed and fixed dormant uppercase-hash comparison and event-list parenthesis
bugs in the production validator, plus the deterministic packager's old fixed
40-file source-only count. Release packages now require the explicit 42-file
layout when bank+manifest are present and retain 40 only for an explicitly
acknowledged source-only build.

Qualification evidence:

- Authored-project validator: PASS.
- Production SoundBank validator: PASS.
- Repository validator: PASS.
- Domain/reflection suite: PASS, 898/898.
- Exact-reference clean Release build and build-output validation: PASS.
- Strict bank-present package creation and validation: PASS.
- Release package SHA-256:
  `CA1A41CD2A787B45D967C3464097DB78141208A75A9AE3BD121EA234C5A121D4`.
- DLL SHA-256:
  `B549CC7605123443F60DE8A131E6026244FD96F22F1561EFB81C43E6CFFEDED6`.
- Package inspection: one `assets\soundbanks\KMG_Firearms.bnk`, one manifest,
  zero `Init.bnk`, zero other `.bnk`, zero `.wem`, and no authoring files/cache.

Next: commit/push this authentic-bank checkpoint, then deploy only through the
guarded runtime tooling and execute `disposable-firearm-wwise-audio`.
