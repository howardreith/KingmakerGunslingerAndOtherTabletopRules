# Expanded Summoning alternate sources - mission state

Status: **development checkpoint reached; native evidence blocked.**

- This file is the resumable state for the alternate-summoning-sources mission
  only.
- It does not replace the Sprint 25 worker's resume narrative or any older
  handoff; older handoffs remain history.
- The design, the route audit and the maintainers' note are in
  [docs/ALTERNATE-SUMMONING-SOURCES.md](../docs/ALTERNATE-SUMMONING-SOURCES.md).

## Intake (2026-10-16)

| Item | Observed value |
|---|---|
| Machine / lab root | Windows 10 Pro 10.0.19045; `C:/Dev/KingmakerGunslingerLab` |
| Main checkout | `repo/KingmakerGunslinger`, branch `master` at `607da683b1`, clean, not touched |
| Mission worktree | `worktrees/expanded-summoning-alternate-sources` |
| Mission branch | `codex/expanded-summoning-alternate-sources` (upstream unset; pushed only through the approved wrapper) |
| Base | `origin/master` `968b35495c11d87a7e00b96f3d33bea3ce05929b` (Merge PR #51, the v0.0.156 release record) |
| Latest stable release | `v0.0.156` -> `607da683b1f6fd545dab90fd6f4121d155573142`, contained in the base |
| Base beyond the v0.0.156 content | `93a3bf1d17` release record plus merge `968b35495c`; documentation only |
| Active summoning PR | #49 Sprint 25, draft, head `5833dfb87e2a99992fd732c1b9b7bce03a147811`; not imported, not touched |
| Runtime lease at intake | none active; newest `runtime-20261016T041518Z-...` is `Completed`; no Kingmaker process |
| Installed game | `C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker`; Kingmaker 2.1.7b, Assembly-CSharp SHA-256 `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb` |
| Installed KMG (human install) | `Mods/KingmakerGunslinger/Info.json` `0.0.140`; untouched by this mission |
| Installed CotW | `1.14.4c-2.1`; DLL SHA-256 `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915`; `blueprints.txt` SHA-256 `f227b1c302dc8db9773de483369407ecc4a154b4082d83c97fcfe0c65912a4f4` |
| CotW last-load registry | `loaded_blueprints.txt` (2026-10-16 00:11 local) has no KMG-derived names |

## Delivered

- Seven adapter families with exact host identities (see the doc):
  - native Monster Tactician
  - with CotW: Summoner (Twinned shares it), Master Summoner, Occultist
    Arcanist, Naturalist, Feral Hunter, Preservationist
- 3,746 adapter identities (UUIDv5, append-only) and the manifest-tool
  extension.
- An availability gate on every adapter child.
- Menu-layout recognition for exact adapter roots.
- Per-family transactional publication with refuse-on-foreign-change
  rollback.
- First-update lifecycle.
- 12 domain tests, 6 tool tests, and the read-only observer
  `observe-alternate-summoning-sources`.
- 15 existing ledger-shape tests updated for the appended block.

## Runtime transactions

None were started. No deployment, lease, game launch or save access happened
in this mission; there is nothing to restore.

## Blockers and next actions

1. **Native evidence is blocked by the release-bound repository validator.**
   - `Build-Local.ps1` runs `validate-repository.ps1`, which dispatches
     version 0.0.156 to `validate_firearm_continuation_release156.py`.
   - That validator fails with "The firearm patch must not change blueprint
     identities". It was run once to classify the block.
   - No candidate can be built on this branch until a release-preparation
     step owns a new version and validator. This mission must not do that.
2. **At integration**, run the deferred native gate listed in the doc, in
   order:
   1. the observer in the CotW and no-CotW profiles
   2. the Master Summoner and Occultist player paths
   3. each remaining contract once
   4. module OFF / cold load with a saved shortcut
3. **Integration with Sprint 25 (PR #49).**
   - Both branches append to `blueprints/blueprints.json` and update the same
     ledger-offset tests.
   - Whichever merges second regenerates with
     `python tools/expanded_summoning_manifest.py --allocate --activate --emit-roster`.
     It must keep the earlier branch's rows in place; adapter GUIDs are
     deterministic, so re-appending them yields the same values.
   - Then add the other block's size to the ledger-offset sums.
   - Brachiosaurus being SNA-only, registered and withheld, will gain SNA
     adapter identities only, and will stay unexposed while withheld.
4. If `origin/master` advances, record the new head here. Do not merge
   automatically.
