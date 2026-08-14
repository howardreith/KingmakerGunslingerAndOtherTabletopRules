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
