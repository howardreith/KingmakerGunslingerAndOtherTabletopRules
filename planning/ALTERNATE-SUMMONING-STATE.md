# Expanded Summoning alternate sources - mission state

Status: IN PROGRESS (development checkpoint not yet reached).

This file is the resumable state for the alternate-summoning-sources mission
only. It does not replace the Sprint 25 worker's resume narrative or any older
handoff; older handoffs remain history.

## Intake (2026-10-16)

| Item | Observed value |
|---|---|
| Machine / lab root | Windows 10 Pro 10.0.19045; `C:/Dev/KingmakerGunslingerLab` |
| Main checkout | `repo/KingmakerGunslinger`, branch `master` at `607da683b1`, clean, not touched |
| Mission worktree | `worktrees/expanded-summoning-alternate-sources` |
| Mission branch | `codex/expanded-summoning-alternate-sources` (upstream unset; pushes go only through the approved wrapper) |
| Base | `origin/master` `968b35495c11d87a7e00b96f3d33bea3ce05929b` (Merge PR #51, v0.0.156 release record) |
| Latest stable release | `v0.0.156` tag -> `607da683b1f6fd545dab90fd6f4121d155573142` (contained in base) |
| Unreleased on base beyond v0.0.156 content | `93a3bf1d17` release record + merge `968b35495c` (documentation only) |
| Active summoning PR | #49 Sprint 25, draft, head `5833dfb87e2a99992fd732c1b9b7bce03a147811`, base `master`; not imported, not touched |
| Runtime lease at intake | none active; newest lease `runtime-20261016T041518Z-...` status `Completed`; no Kingmaker process |
| Installed game | `C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker` (Kingmaker 2.1.7b Assembly-CSharp SHA-256 `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`) |
| Installed KMG (human install) | `Mods/KingmakerGunslinger/Info.json` version `0.0.140` |
| Installed CotW | `Mods/CallOfTheWild` Info.json `1.14.4c-2.1`; DLL SHA-256 `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915`; `blueprints.txt` SHA-256 `f227b1c302dc8db9773de483369407ecc4a154b4082d83c97fcfe0c65912a4f4` |
| CotW last-load registry | `loaded_blueprints.txt` (written 2026-10-16 00:11 local) contains no KMG-derived names: in that load CotW cloned native SM/SNA children before KMG publication |

The installed CotW DLL was decompiled (ILSpy 9.1, single-file, scratch only,
not committed) for static inspection. Static findings below are marked as
such; they are not runtime observations.

## Decisions so far

- Adapter reconciliation runs after all `LibraryScriptableObject.LoadDictionary`
  postfixes (first UMM update, the established Aid Another pattern), so CotW
  clones only native children and never sees adapter names.
- Adapter children are new, manifest-backed identities in the disjoint
  `KMG.Summoning.Adapter.*` namespace, one per caller root, tier, creature and
  quantity. Ordinary spell children are never reparented.
- Execution is cloned from the caller's own exact template child for that tier
  and quantity; only spawn units and the canonical template/alignment actions
  are composed in.

## Next actions

See the implementation section once written.
