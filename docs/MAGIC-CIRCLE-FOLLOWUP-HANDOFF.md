# Magic Circle initial follow-up handoff (historical)

The later [terrain and grouped-metamagic review](MAGIC-CIRCLE-TERRAIN-METAMAGIC-REVIEW.md)
replaces the flat boundary described below and adds native grouped Extend
coverage. Its exact artifacts and qualification supersede this initial
polish checkpoint for those changes. Artwork approval remains pending.

Branch: `codex/magic-circle-followup-polish`, based on accepted Magic Circle
`c2446fc5cb1cfd568b3f3bcaa4a5375396a0a4ca`. Version remains 0.0.134. No merge,
history rewrite, master conflict resolution, or public release was performed.

## Learning and casting

New learning and selection surfaces publish **Magic Circle against Alignment**
once per eligible spellbook. Its native variant popup chooses the alignment;
the chosen child retains the accepted effects, descriptor, held-touch delivery,
duration, slot cost and aura behavior. Oracle Favored Class gets one parent
parameter through the existing native Items/CanSelect repair and commits one
additional known spell without increasing the ordinary allowance.

| Source | Choices exposed by its parent |
| --- | --- |
| Existing general class, inherited and implement lists | Evil, Good, Chaos, Law |
| Paladin | Evil, Chaos |
| Antipaladin | Good, Law |

The three parent identities are new; the original 24 identities are unchanged.
Restricted parents have their own variant arrays. Native converted casts spend
the selected parent's book/slot. The narrow list-membership postfix admits only
an exact owned child exposed by an exact owned parent actually published in that
list. Native true results and unrelated spells remain untouched. General and
Paladin parents use the Evil emblem; Antipaladin uses Good.

The four existing scroll items retain their IDs, CL5/level3/375gp contract,
finite vendor stock and exact alignment-specific cast abilities. Native scribing
follows the child's Parent to the general family: the first successful copy
learns one family; subsequent alignment scrolls cannot add duplicate known
choices. CopyScroll.CustomSpell stays unset so scroll-use eligibility checks
the actual restricted child. Legacy independently learned child spells remain
valid; this pass does not respec, refund or rewrite old known-spell selections.

## Icons and moving boundary

The paintings now derive from the actual native Protection and Communal
Protection identities: blue curled sigil for Evil, gold sunburst for Good,
red fractured disc for Chaos, and geometric gold star/violet perimeter for Law.
Two enclosing rings distinguish Magic Circle. The scroll composites reuse the
existing Word of Recall/Teleport parchment shell and replace its measured emblem
window through the documented export pipeline. Spellbook, parents, variants,
held touch, carrier/recipient buffs, scroll inventory, vendor and tooltip
consumers have explicit catalog dispositions and consistent assignments.

The superseded approved paintings, briefs and approval records are archived.
**New pixels await owner visual approval.** Review the
[size/grayscale sheet](../reports/icon-overhaul/production-magic-circle-followup.png).
Native sprite/prefab reference identities are recorded in
[the reference audit](../reports/magic-circle/FOLLOWUP-ART-REFERENCES.json).
No screenshot attachments were available in the received chat; the actual
installed sibling sprites supplied the visual anchors.

After inspecting Arcane Concordance's area prefab, this pass uses a thin
LineRenderer boundary attached to the native moving area view. Its 96 vertices
use the area's actual Size.Meters (10 feet / 3.048m), with a 0.07m line width.
There is one material/renderer per area and no per-frame allocation or respawn
loop. Evil is blue, Chaos red, Good gold and Law violet. Native view attachment
creates/reuses the ring, ForceEnd and unload hide it, and view destruction frees
its material. Accepted aura lifetime/control mechanics are unchanged.

## Qualification

All seven final scenarios passed on candidate 11: **428 native assertions**.

| Final native scenario | Assertions | Evidence directory suffix (UTC) |
| --- | ---: | --- |
| Mechanics, acquisition and class access | 278 | `20260921T0210429232142Z-disposable-magic-circle-evil` |
| Native learning/casting and icon UI | 72 | `20260921T0213509009846Z-disposable-magic-circle-ui` |
| Oracle selection and fixture preparation | 21 | `20260921T0219091794245Z-working-save-magic-circle-prepare` |
| Fresh-load verification | 13 | `20260921T0222204181754Z-working-save-magic-circle-verify` |
| Reload and world-map round trip | 26 | `20260921T0225127319904Z-working-save-magic-circle-scene` |
| Exact fixture cleanup/save | 15 | `20260921T0229393113049Z-working-save-magic-circle-cleanup` |
| Fresh-load absence | 3 | `20260921T0232449588777Z-working-save-magic-circle-absent` |

Oracle known third-level entries increased from two to three; its ordinary
allowance remained two. Fresh loads retained exactly one family and no separately
learned children. Both real casting popups selected the chosen alignment.
Native scroll purchases, scribing, duplicate rejection, exact child casts,
Paladin progression/casting and all present class-list restrictions passed.

Native renderer checks covered actual radius/vertices, color, movement without
replacement, overlapping circles, expiry, dispel, death and reconstruction.
Eight saved areas reconstructed one correctly colored ring each. Cross-launch
comparisons matched original actors, contexts/deadlines, control, slots/known
spells and market state. Native UI checks passed with zero Circle exceptions.
Eight final framebuffer captures were also visually inspected; this is supporting
visual evidence, not the mechanical proof or owner art approval.

Every phase restored its settings. Both disposable fixture cycles were cleaned
through their exact preparation bindings. Final fresh absence passed. The complete
original normal-play installation (0.0.117), including exact settings bytes, was
restored from backup `20260921T0210300793116Z`; the game exited. No baseline save
was written. Final source changes after this tested build contain only evidence,
planning and qualification-status documentation.

The full domain suite contains 1,683 tests. Repository validation, clean Release
build, strict 235-file package validation, protected icon/source validation,
R1 request tests and all 12 R2 settings-ownership cases passed. The native
scenarios use Steam 640820 and only KMG_AUTOMATION_WORKING. Structured native
state supplies mechanical proof; framebuffer captures supply visual evidence.

The diagnostic persistence failure was an incorrect test oracle: after loading,
IsKnown recognizes usable child variants while the actual learned-spell array
contains just the parent. The corrected test counts actual learned entries and
still requires availability of every child. No casting or learning behavior was
changed to satisfy that test. The first scene run passed its 26 native assertions
after the outer launcher deadline; the final local wrapper increases only that
supported deadline to 600 seconds, retaining all native/R1/R2 guards.

The installed stack's cold native Protection popup reproduces four Races
Unleashed slot-initialization exceptions before Circle checks. The probe records
that exact matched control separately. No exception allowance applies to Magic
Circle, and no unrelated UI patch was added. Antipaladin's native list and two
choices are checked; full Antipaladin character progression/casting UI is not
claimed. Reduced/off profiles and gamepad presentation were not requalified.

## Exact candidate

Candidate 11, produced by Build-Local before the evidence-only documentation
update. Embedded commit is the accepted baseline above; the dirty source state
and binary identities distinguish this candidate from PR19 and prior attempts.

| Identity | Value |
| --- | --- |
| Source state SHA-256 | `2a2c52867c3fae720efa95352ea01026b47f4b588a17179cca06684979e06a57` |
| ZIP SHA-256 | `dd00912a5b93f3bfe888d0c59aae2c7156a6eb884ef3acfcfce70c0c9fb5384a` |
| DLL SHA-256 | `48850e819cf3e2d3e3a7a92f31ebdbe4cdde0b578dd19c8ac17c35f02c4d92eb` |
| DLL MVID | `f74654c0-ea2f-4c5b-8d25-9ee07dbd31be` |

Local package: `artifacts/local-runtime/0.0.134/KingmakerGunslinger-0.0.134-local-runtime.zip`.
The adjacent `.build-local.json` pins provenance. Prior installable and scratch
compile artifact hashes, exact native directories/results, failed attempts and
visual evidence hashes are curated in
[FOLLOWUP-QUALIFICATION.json](../reports/magic-circle/FOLLOWUP-QUALIFICATION.json).
Packages, proprietary assemblies, saves and raw runtime artifacts are not committed.

## Preserved boundaries

No new save against existing control, suppression/resumption, summoned-creature
barrier/breach, inward circle, planar binding, trap diagram or blanket descriptor
immunity. No Buff Planner changes, unrelated UI/observer repairs, reduced-profile
save qualification, uninstall work or owner integration. Existing shared
Protection logic, class entitlements, finite acquisition, R1/R2/C1 contracts and
normal-play restoration boundaries remain in place.
