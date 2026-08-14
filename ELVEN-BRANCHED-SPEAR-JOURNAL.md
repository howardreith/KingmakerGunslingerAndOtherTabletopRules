# Elven Branched Spear journal

## 2026-08-14 - First-playtest repair

- Resumed `codex/elven-branched-spear` from accepted checkpoint `f5136b12` and
  continued its clean published descendant. Draft PR #3 remains the only PR.
- Identified the stale release cause: both package entry points and all active
  version pins still selected the Expanded Summoning `0.0.78` identity.
  Advanced transactionally to assembly `0.0.79`, informational version
  `0.0.79-elven-branched-spear`, and an explicit spear archive name.
- Repaired category presentation centrally at `StatsStrings.GetText` for only
  the owned stable category. Native categories and saved numeric identity are
  untouched.
- Matched native selector presentation: shared EWP icon, spear art for Finesse
  Training, exact parenthesized Rogue name, and native `EB` glyph metadata for
  parameterized weapon feats. Guarded combat run passed 18/18.
- Added all six generic spear tiers to four exact installed BTSL merchant
  tables. Guarded vendor observation passed with 24 singular rows and existing
  native/firearm rows intact; focused ON/OFF module observations passed.
- Assessed named visual variants. The current family shares one weapon type and
  one fit-proven prefab. Differentiating it safely would require an invasive
  type split or renderer mutation, so optional polish is deferred to preserve
  the accepted no-clipping rig.

Final source, compatibility, save-smoke, package, installed-binary, and artifact
identity evidence is recorded in the qualification report after sealing.

## 2026-08-14 - Final seal

- Final artifact source `224e536f24ed548a8df8746b8b3d8a9a6a38defe`
  passed repository validation, 1,032 tests, clean Release, build-output checks,
  package creation, and strict standalone validation.
- Final Call of the Wild combat/presentation passed 18/18 and restored the Mods
  tree exactly. Canonical `KMG_AUTOMATION_WORKING` smoke passed without a save
  write.
- The uninterrupted five-module runtime matrix passed all 32 unique masks from
  `20260814T0512562299220Z` through `20260814T0623367488297Z`, all on one commit
  and loaded version, then restored the original settings SHA-256 exactly.
- Built and installed DLL hashes match. Package, DLL, MVID, and bundle identity
  are recorded in the qualification report and `BUILD-INFO.txt`.

## 2026-08-14 - Final Exotic Weapon Proficiency presentation repair

- The reported live installation was not stale: its DLL matched the sealed
  candidate byte-for-byte. The remaining bare title and top-of-list placement
  were therefore treated as a real defect.
- Changed the static child title to exactly **Proficiency (Elven Branched
  Spear)**. With Call of the Wild installed, the native Elven Curve Blade
  anchor is contributed to the merged `AllFeatures` catalog rather than the
  serialized `Features` catalog. Publication now appends singularly to the
  latter and inserts immediately after Elven Curve Blade in the former. The
  native reversed list presentation consequently places the spear immediately
  above Elven Curve Blade without changing either blueprint identity.
- Source commit `9a24147b717b2502442d48d5f2026becdaba4e8d` passed
  repository validation, 1,033 tests, clean Release/package validation, and
  guarded Call of the Wild runtime run
  `20260814T1009047959759Z-disposable-elven-branched-spear-combat` (18/18).
  The exact candidate remains installed in Unity Mod Manager.

## 2026-08-14 - Native EWP wording and list-source correction

- Human review established the exact native prefix as **Weapon Proficiency**
  and confirmed that the spear still rendered at the top. The installed DLL
  matched the qualified candidate, so both findings were genuine presentation
  defects rather than stale deployment.
- Renamed the child to **Weapon Proficiency (Elven Branched Spear)**. Removed
  it from the EWP `Features` array, which the UI prioritizes as a separate top
  block, and retained exactly one entry immediately after Elven Curve Blade in
  `AllFeatures`; native reversed rendering places it immediately above the
  curve blade entry.
- Commit `9e710754e50c09e95c7790d70af8a334757b940e` passed 1,033
  tests, clean Release/package validation, and guarded Call of the Wild run
  `20260814T1025471192636Z-disposable-elven-branched-spear-combat` (18/18).
  The runtime observed `Features=-1/-1`, `AllFeatures=5/6`, the exact title,
  one option, and all prior mechanics. Its exact DLL remains installed.
