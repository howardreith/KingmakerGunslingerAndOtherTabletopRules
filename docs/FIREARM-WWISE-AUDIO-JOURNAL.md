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

## 2026-08-06 — guarded native-Wwise runtime qualification

Fixed isolated-worktree reference discovery after the first guarded invocation
safely stopped before deployment. Commit `9315f41` passed the build gates and
was published with the policy script. Commit `147c412` strengthened the
save-free scenario with a live disposable attacker/target, selected-unit
preview, ordinary committed attack, and forced misfire; it passed 898/898
tests, repository validation, exact-reference Release build, build-output
validation, and the authorized push.

Two consecutive fresh-launch `disposable-firearm-wwise-audio` runs passed:

- `20260806T2234249930950Z-9d5cad71fc464a3eb642c7524e5b7871`, result SHA-256
  `7C7D26AC009B3A9C355AC47D8744D5505AA7647A5A88B14E9A0F483857DD2063`.
- `20260806T2235554614335Z-d28d626bce5449f398c1adc4145168a4`, result SHA-256
  `AE112B7524E48153EB292D1ACABFF715A7B8E68CB13A1B634A7213AB724A47E4`.

Both observed `Ready`, matching source/staged bank hashes, one bank-load
attempt, and exact pistol Event posts: global `Canvas` playing ID 2, live unit
`Human_Fighter_Baron(Clone)` playing ID 3, and ordinary committed discharge
playing ID 4. The forced natural-1 misfire did not increment post attempts or
accepted posts. Fixture cleanup and collection isolation passed.

Audio-enabled deed scenarios also passed on `147c412`: Scatter Shot run
`f81b9c9ce8494a07a9a275523be298e0`, Dead Shot
`57e656e309194c6da903a94c3b76dbc6`, Startling Shot
`cd849e255d2143e1b1d0c09ee7fa80ac`, Menacing Shot
`6b819a9beb4b43778ac30ce4936086e5`, and Stop Bleeding
`22d88393020546c684fe12dd6dd4e14f`. Production fallback run
`a51a8b54958c4e21b0aa895c4db60d6b` passed all eight assertions and preserved
all five firearm visual/projectile/presentation fallbacks.

Current runtime package SHA-256:
`24E2AC4CBF468B19143C361C220AD1EE90F343DE4E85AA2D2A4BB593E5973AC1`;
DLL SHA-256:
`8BC7C6264629715050D1D57B87E9EEB5E2AB73D22C3BB1981A35C8DE1F6158F4`;
strict release package SHA-256:
`CA93B06093DDCA57A4811562CB3AB6E2FC23E69854500E647A20A6D816C534E3`.

Automated work is complete. A nonzero playing ID proves Wwise accepted the
Event, not audible speaker output. Audibility, mapping, mix/spatial behavior,
blunderbuss tail, and inherited crossbow overlap remain human judgments.

## 2026-08-06 — focused auditory-polish implementation

Human listening passed Pistol and Musket and confirmed the Blunderbuss/Scatter
custom report. It failed Blunderbuss timing, heard a crossbow release on a
misfire with zero custom posts, and heard borrowed Burning Hands audio on
Scatter. Waveform evidence located the Blunderbuss transient around 2.20 s.

Implemented a standard-library RIFF/PCM trim of exactly 2.180 s (104,640 frames
at 48 kHz), retaining 20 ms before the measured rise and the remaining tail.
Derived WAV: 174,764 bytes, SHA-256
`F3F1E94701C86D946679DAD5F1AE4577553D0DED23404D356E9ADC71ED9488E3`.
The other four sources remain byte-identical. Wwise 2016.2.6.6153 CLI generation
succeeded. New bank: 999,390 bytes, SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`;
manifest SHA-256
`20908FBB97AE465075B53491D5C7103E5C5520B5A481CDCC0CB2B8399A61F517`.

Materialized protected resolved presentation/equip/inventory/non-release sound
values before severing firearm Prototype fallback, then cleared only
`m_WhooshSound`. Scatter preserves native 15-foot cone geometry but uses the
firearm projectile. The audio scenario now exercises a live Blunderbuss
preview, ordinary success, and forced misfire.

Focused test PASS; authoring/bank validators PASS; repository validator PASS;
domain/reflection 898/898 PASS; exact-reference Release build, build-output,
strict packaging and package validation PASS; `git diff --check` PASS. Next:
checkpoint/push and guarded Wwise, Scatter, and presentation scenarios.

### Auditory-polish runtime evidence

Commit `896ec38b1af5142967348f11935cca86bd36f2f7` was published before runtime.
Two consecutive fresh-launch `disposable-firearm-wwise-audio` runs passed. The
first was `20260807T0204544970074Z-542cfd2fa60a4868bb8fe9accdbcbd39`
(result SHA-256
`E5609A66F04801F85839710CF1C9DDD13BAE688C7B1905B749DD481542A11AFD`);
the second was `20260807T0210021249437Z-259c1bd4abac47fbb5aa138846c1a0c6`
(result SHA-256
`81DE5F3DE21F47B98C98BB315A322AAC23EC353E338DFFD5AD85F1501FB47ED9`).

Both reached `Ready` with one bank-load attempt and exact new staged/source
hashes. They accepted global Pistol on `Canvas` as ID 2, live Blunderbuss on
`Human_Fighter_Baron(Clone)` as ID 3, and ordinary committed Blunderbuss as ID
4. Forced Blunderbuss misfire left attempts/accepted posts at three.

Scatter run `20260807T0206511814303Z-08ba9a38fe1e4ddaae60202b49ca32d3`
passed the two-target mixed/all-misfire transaction and cleanup. Presentation
run `20260807T0207599351364Z-b317e082a8bf4ad5b5a20c132efb38e0`
passed all eight assertions for five firearm models, projectiles, icons,
animation/presentation, descriptions, and isolation after prototype severing.

Final runtime package SHA-256:
`66FCA06C862A41FC0E3E42A8ECC3C9DBBE605EBEC1CAC4E92B72180FE9D7FBE5`;
DLL SHA-256:
`649E5E7DFA739E610E28D0BA2B2124BB8EA831B30FC451CCEBF391EF8944B9BA`;
last strict package SHA-256:
`94AFF3D386BDE6111EA06E8340C751DE3C7CA9EFDDB617D9F10398C982F0B1B6`.
Only fresh human auditory/visual acceptance remains.
