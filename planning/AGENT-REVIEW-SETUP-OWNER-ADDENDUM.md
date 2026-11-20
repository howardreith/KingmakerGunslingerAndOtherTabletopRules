# Agent review setup — owner addendum (2026-11-20)

Owner instruction given during the setup pilot, after round 1 of the
controller-core review. It extends the setup brief
(`planning/CLAUDE-GITHUB-REVIEW-SETUP.md`), which remains the hash-pinned
mission contract:

> I want this to be set up so it's a general development setup, not only for
> the tabletop repo, and certainly not only for black tentacles. I'll want to
> use this development setup for all repos in the Dev directory. Maybe not
> nima, but maybe nima too. So when doing that setup please keep that in mind.
> This shouldn't be only for tabletop mod. This should be a higher level
> setting option.

Implementation response:

- The controller is installed once at `C:\Dev\agent-review\` and keeps its
  machine-level home (locks, registry, default state, user-local project
  configs and plans) in `~\.agent-review\`.
- Repositories are onboarded with `init-project`; this repository is one
  shipped project (`kingmaker-gunslinger`), Black Tentacles one shipped plan,
  and `single` a generic plan for any mission.
- Repository specifics (push wrapper, branch prefix, validation commands,
  protected paths, runtime locks) are project configuration; the developer
  permission base, built-in guarded push and owner model policy are generic.
- Repositories under `C:\Dev\nima` default to the `~/.claude` (work) Claude
  configuration, per the existing account shim; onboarding nima remains the
  owner's choice.
