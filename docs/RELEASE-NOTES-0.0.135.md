# Release Notes 0.0.135 - Magic Circle polish

Version: `0.0.135-magic-circle-alignment-spells`.

Magic Circle is now learned as **Magic Circle against Alignment**. Choose Evil,
Good, Chaos or Law when casting. Oracle favored class selection exposes one
family, with ordinary known-spell accounting. Paladin retains only Evil/Chaos;
Antipaladin retains only Good/Law. Existing legacy spell identities and known
selections are preserved.

Spell, variant and buff icons now share the Protection family's visual identity.
Magic Circle scrolls use matching emblems on the native parchment scroll shell,
consistent with Word of Recall and Teleport. Scroll identities, finite vendor
availability, scribing and casting are unchanged.

The bearer carries a visible ten-foot horizontal boundary projected onto the
terrain by the game's native area decal machinery. It remains readable over
slopes and stairs, using blue for Evil, red for Chaos, gold for Good and violet
for Law. Ordinary occlusion and per-area lifecycle cleanup are preserved.

The grouped family's Extend route was qualified through the native metamagic
mixer, variant selection and casting during the accepted review. It spends one
adjusted slot and doubles duration without restarting when recipients refresh.
No production metamagic behavior was changed.

Shared Protection mechanics, Oracle/Word of Recall selector repairs, save and
runtime safety tooling, and class access remain intact. The existing setting
continues to govern protection against **new qualifying control**. Existing
control remains active. No summoned-creature barrier, inward circle, trap
diagram, blanket immunity, Buff Planner, Gamepad or uninstall work is included.

## Approval and qualification

The owner authorized this release on 2026-09-21 after approving the polish and
confirming its merge to master in PR #20. This approval supersedes the earlier
review-stage pending-artwork status; artwork bytes are unchanged for release.

The accepted review recorded 1,683 domain tests and 509 native assertions across
eight guarded runs, including uneven terrain, grouped Extend and disposable
`KMG_AUTOMATION_WORKING` persistence. Those results belong to the exact earlier
candidates recorded in the [review handoff](MAGIC-CIRCLE-TERRAIN-METAMAGIC-REVIEW.md)
and [evidence ledger](../reports/magic-circle/TERRAIN-METAMAGIC-REVIEW.json).

**No additional tests were run for 0.0.135, as explicitly instructed by the
owner.** The release DLL is rebuilt with the new version and commit identity;
it is not represented as a newly runtime-tested binary. No game launch,
installation or settings changes are part of this release preparation.

## Installation

Download **KingmakerGunslinger-0.0.135-magic-circle-alignment-spells.zip** from the
GitHub release assets and install through Unity Mod Manager, preserving existing
`FeatureModules.json` settings. The automatic Source code archives are not the
installable mod. `SHA256SUMS.txt` and `release-manifest.json` identify the payload.
