# Expanded Summoning Phase 2 blocker

## Sprint 10: exact Stirge and Giant Wasp rules source, 2026-09-27

Status: awaiting explicit authorization for a narrow public Pathfinder 1e
rules lookup, or owner-supplied exact stat blocks. This is a source-access
gate, not an engine-infeasibility claim. Sprint 9 is internally technically
qualified; Sprint 10 and later sprints are not complete. Draft PR #25 is open
and not ready for owner review. No merge, release or permanent deployment
occurred.

The blocked requirements are the Stirge flying touch-attack, attach and
blood-drain rules and Giant Wasp's stat profile and poison (including exact
save/DC, frequency, effect and cure), needed to build rules-faithful creature
profiles and focused runtime assertions. The supplied charter and roster
establish spell tiers and tactical roles but omit numeric Bestiary stat
blocks. The guide contains no Stirge/Wasp passage. The charter cites public
Summon Monster and Summon Nature's Ally list URLs, but those are not the
creature stat blocks. `AGENTS.md` explicitly prohibits network access unless
the task authorizes it. No such specific authorization was in the packet.

Alternatives examined: the current creature and donor catalogs have neither
unit; the guarded in-game metadata audit `20260927T1601519083199Z` found no
named Stirge/Wasp/mosquito unit or attach/blood-drain fact or ability in its
selected blueprints. Generic ConstitutionDrain is the sole matching buff,
without an attach lifecycle. The existing vermin builder and poison graphs
are implementation seams but do not establish these creatures' numbers;
the summon grapple/hold path is not Stirge attach. Guessing the values or
reusing another creature's poison would violate the mission's no-silent-
approximation and owner-balance rules. This audit does not rule out a safe
dedicated implementation once the exact rules are available.

The audit passed repository validation, all 1,929 domain tests, clean
Release, strict package validation and guarded runtime. Its wrapper restored
the original 136-file live mod tree; restoration record
`20260927T1603474656325Z` verifies SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
No save was selected, loaded or written by this metadata audit. The feature
worktree is clean after the blocker record is committed and pushed.

Smallest owner action: authorize network access solely to consult public
Pathfinder 1e Stirge and Giant Wasp stat/rules pages (prefer the official
Paizo PRD or an authorized public rules reference), or provide the exact
creature stat blocks and poison/attach text. This does not request a balance
decision, art approval, test-operation assistance, or permission to advance
between the already-authorized sprints.

Ready-to-paste continuation prompt:

> Resume the Expanded Summoning Phase 2 Sprints 9-21 mission from the pushed
> `codex/expanded-summoning-phase2a-sprints9-13` branch and draft PR #25.
> Sprint 9 is internally technically qualified; Sprint 10 has a guarded
> native-donor audit and exact restoration. [I authorize network access only
> for public Pathfinder 1e Stirge and Giant Wasp rules/stat pages / I provide
> the exact stat blocks here: ...]. Use that source to implement and qualify
> Stirge and Giant Wasp, then continue the authorized Sprints 11-21 and three
> stacked draft PRs under the mission. Do not merge, release, permanently
> deploy, or begin Sprint 22.
