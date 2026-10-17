# Brown-Fur startup repair — alpha 1

Owner-requested test prerelease `v0.0.157-brown-fur-alpha.1`. Stable v0.0.157
remains Latest. This is not stable-release qualification and does not merge
the repair into the default branch.

## Download and test

Download **KingmakerGunslinger-0.0.157-brown-fur-alpha.1.zip** from Assets, not
the automatically generated Source code archives. Install on your test computer
using the normal Unity Mod Manager ZIP workflow. Keep a backup of your existing
mod installation and saves; do not save over the previously failing save while
checking this repair. No save editing, unrelated mod removal or loader change
is part of the fix.

**UMM still displays 0.0.157.** This alpha deliberately publishes the exact
already-tested binary without rebuilding or modifying version metadata. Use
this alpha's download name and product commit to distinguish it from stable:
`913dfd6e89386ff233849f06f902ca8ea2de239e`.

## Repair

Brown-Fur registration remains pending while CotW Arcanist construction or
KMG's blueprint library is unavailable. The real committed-library lifecycle
now retries synchronously, before returning to the loading pipeline. All 25
original stable identities are checked against the actual library and expected
types, including Strength activatable
`16c06d016437be9e9e6dac6211ff30a5`. Compatibility checks, transactional rollback
and registration-versus-selector-publication rules remain intact. No GUIDs,
placeholder blueprints or PostLoad exception suppression are introduced.

## Evidence and remaining checks

- Exact packaged product: 83/83 focused tests, provenance-checked .NET 4.7 /
  C# 7.3 build and strict standalone package validation.
- One fresh guarded DATA run: 10/10 native assertions; all 25 actual types
  registered at library-ready, native **Load Game** of KMG_AUTOMATION_WORKING,
  actual disposable Brown-Fur effects/resource debits, interruption and cleanup.
- Actual input mode was **mouse/keyboard**. All sixteen installed mods were
  retained; original installation/settings restored; all 95 protected save
  files unchanged.
- **Not yet verified:** cold reconstruction of saved Brown-Fur owners, the
  affected character's save, controller-mode loading or physical controller
  input. The lab save did not contain the missing Strength GUID. Therefore the
  original PostLoad failure is not claimed reproduced or proven repaired.
- This is focused evidence, not a new full-suite/compatibility campaign.

Please try loading the affected save **without saving over it**. If it fails,
send the fresh output_log and say whether you used **Continue or Load Game**,
and **controller or mouse/keyboard mode**. If all 25 identities resolve but
PostLoad still fails, the next investigation is the actual PostLoad patch
owners and exception provenance—not assuming the missing GUID was the only cause.

ZIP SHA-256: `d7cf7aa3cbae1a3ff76a73d01ad302448e907be6da92fbe5b0a94569ca91f33e`.
DLL SHA-256: `f878c8c3e36264b9f34d0441443a26c50da5ea939490d03e264f30164731afbb`.
DLL MVID: `88974741-ddbe-4fcb-8f1c-145305534eaa`.
The attached alpha manifest and checksums preserve the exact artifact identity;
private game assemblies, saves and raw logs are not published.
