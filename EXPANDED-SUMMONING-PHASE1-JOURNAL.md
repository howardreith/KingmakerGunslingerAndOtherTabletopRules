# Expanded Summoning Phase 1 journal (charter Sprints 3-8)

Mission: implement charter Sprints 3-8 under the owner's order of
2026-09-24, on one branch and one draft PR that is never merged under the
order, with separately reviewable sprint checkpoints. Sprint 9 is out of
scope; so are release publication and permanent deployment.

## Intake

Phase 0 was finalized and merged as `master` @ `b0641a58` (PR #21 at
`ba5e20a7`) a few minutes before this branch was created from that commit in
its own worktree, so the accepted baseline and the mission's starting point
are the same tree. The Phase 0 closeout's last two findings shape this
mission's habits: a visual is not accepted until an in-game image has been
looked at, because the one defect no assertion could see (a material clone
outside the game's fades) was found only by a render; and a native seam that
has never run live (the menu's scrolling installer) is assumed broken until a
measurement says otherwise.

The game's blueprints cannot be read offline and the third-party sources on
this machine name no unit the sprints need, so Sprint 3 opens with a mod-load
audit scenario that records, as metadata only, which units, classes, facts,
buffs and abilities the installed library offers for the creatures ahead, and
what the native Grab feature's action graph wires to. Tabletop stat blocks for
every Sprint 3-8 creature were fetched from the public SRD on 2026-09-24 and
sit in the Phase 1 report's appendix.

The guarded push script's branch allowlist gained this mission's branch, as
every earlier mission's branch was added; the script itself, its origin check
and its refusal of any history rewrite are unchanged.
