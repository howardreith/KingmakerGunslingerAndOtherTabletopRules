# Expanded Summoning Phase 1 — pass-off to Codex

Written 2026-09-26 by the Claude session that finished Phase 1, merged it and
cut the release. Codex takes over when its quota returns at 17:30 today. Read
this before touching anything.

## State in one paragraph

Expanded Summoning Phase 1, the charter's Sprints 3 through 8, is merged to
`master` and published as release **v0.0.139**. Nothing is in flight. There is
no draft pull request left open for this work, no unpushed commit, no running
background job, and no guarded runtime lease held. The live game installation
is untouched at its restored baseline of 136 files.

## Exact identifiers

| Thing | Value |
|---|---|
| `master` head | `8eda10f741be9926682075bf96f2944e3244f84f` |
| Merge commit for pull request 23 | `f17f8752` |
| Release tag | `v0.0.139`, pointing at `8eda10f7`, published and marked Latest |
| Release version | `0.0.139-expanded-summoning-phase1` |
| Release asset | `KingmakerGunslinger-0.0.139-expanded-summoning-phase1.zip` |
| Asset SHA-256 | `7f1bf7ab2a7b1e170d61029a628304eb661af1ee9bab326bffd0dbbec7f8b5f2` |
| Runtime-qualified candidate | `145810a5` (the twelve-scenario matrix) |
| Domain suite | 1806 cases, 0 failures |

Release notes are `docs/RELEASE-NOTES-0.0.139.md`. The active release validator
is `tools/validate_expanded_summoning139.py`, dispatched from
`tools/validate_repository.py` for version `0.0.139`.

## The one owner decision you must not contradict

On 2026-09-26 the owner accepted a named engine limitation, recorded as
**OwnerAcceptedEngineLimitation: ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD**.

An active summon grab, hold, swallow or engulf, the mouth occupancy that goes
with it and the held-target rake are session-scoped. Kingmaker carries no
active grapple across a save and writes this kind of unit part by type without
its contents, so a reload has neither the hold nor the record of which limb
took it. What a reload must do, and does, is come back clean: no lingering
condition, no occupied mouth, no delayed damage, no dangling link, no unusable
unit, no deserialization fault with the module disabled.

Three standing instructions come with that acceptance.

- Do not implement project-owned re-establishment of holds on load. The owner
  set it aside explicitly; it would be a separately chartered persistence
  subsystem if it is ever shown to be worth the risk.
- Never describe the behaviour as grapple persistence. The new validator
  forbids that phrase in the release notes.
- Do not re-mark the item BLOCKED. The decision closed it.

## What is left, and who owns it

**The owner's own gate, not yours.** The engineering review approved the work
and deferred the subjective checks to him in play: whether the Tiger and
Cheetah coats read in motion, whether the Lion tint and the six mephit
treatments are distinct at normal camera distance, whether the new creature
icons are acceptable, whether cat rake and flytrap multi-mouth control and Web
and Flash of Insight feel sensible, and whether loading a save after a grapple
looks clean in play. Do not pre-empt these with more autonomous iteration. If
he reports a problem, that is new work with its own scope.

**Sprint 9 and Phase 2 are not authorized.** Every order through 2026-09-26
forbade starting Sprint 9. Nothing since has authorized it. Wait for a
charter.

**No follow-up defect is known.** The review's four nits were text only and
are fixed in `eeaffdae`: the state file no longer says the pull request stays
a draft, the report's Sprint 8 visual row lost a mangled clause, the Cyclops
comment says its armed state lasts until the next attack roll rather than one
round, and the link store's comment no longer implies the game re-links
grapple parts after a reload.

## Things that will bite you

**The version bump touched a wide surface.** Going from 0.0.138 to 0.0.139
required more than `Info.json`: `Directory.Build.props`, `AssemblyInfo.cs`,
four scripts, `compatibility/profiles.json` and its schema, the development
banner, the icon authoring catalog's registry hash, two domain tests that
assert the active release identity (`ElvenBranchedSpearCatalogTests` and
`EasternFavoredCompatibilityTests`), and roughly a dozen validator files whose
version sets and release-key ternaries needed the new version. If you bump
again, run `python tools/validate_repository.py` and the domain suite and
follow the failures; they name the exact file each time.

**The icon catalog's registry hash goes stale on any blueprint append.** The
Phase 1 merge changed `blueprints/blueprints.json` and the expanded-summoning
icon manifest, and `tools/validate_icon_catalog.py` pins both by hash. That
validator runs from `scripts/validate-repository.ps1`, which the Phase 1 gates
never ran, so the mismatch only surfaced during publication. Run the
PowerShell wrapper, not just the Python validator, before a release.

**The guarded runtime harness demands a clean tree.** Never edit a tracked
file while `scripts/Invoke-ExpandedSummoningRuntimeScenario.ps1` or the batch
runner is going; the scenario aborts and you lose the batch. The full set is
about ninety minutes.

**Quota.** The session that wrote this had under two percent of the weekly
allowance left, which is why no live runtime matrix was rerun for the release.
That was correct here: the closeout changed no executable behaviour. Do not
treat it as precedent for skipping runtime qualification when behaviour
changes.

## Worktrees as they stand

`.worktrees/expanded-summoning-phase1` still holds the merged branch
`codex/expanded-summoning-phase1-sprints3-8` at `eeaffdae`. It is clean and
safe to delete once the owner is satisfied with the release.

`.worktrees/magic-circle-release-master` is the checkout that holds `master`.
The release was cut from it, and it is clean at `8eda10f7`. It is the only
worktree that can hold `master`, which matters because
`scripts/Publish-Release.ps1` refuses to run from any other branch. Its name
is a leftover from an older release; do not assume it belongs to the Magic
Circle work.

Other sessions own the remaining worktrees. Leave them alone.
