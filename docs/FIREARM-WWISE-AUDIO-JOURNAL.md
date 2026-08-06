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
