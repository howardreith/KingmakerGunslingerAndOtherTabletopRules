# Expanded Summoning Sprint 22: Ankylosaurus Completion

Owner mission issued October 12, 2026; HOWARD-LAPTOP only. Authority is the
September 3, 2026 original implementation charter, page 21, supplied in the
mission. Its core roster continues through Sprint 41, with optional Sprints
42–45. Sprint 21 completed the natural-creature rig tranche and its release
mission; its historical STOP did not cancel the later charter. This mission
authorizes Sprint 22 alone. Sprint 23 (Triceratops/Stegosaurus) is not started.

The endpoint is one qualified Ankylosaurus overhaul, one normally merged PR
and stable release, exact human-machine restoration and STOP. Current
[validation policy](VALIDATION-POLICY.md) governs development and the explicitly
selected Sprint candidate boundary. Intermediate checkpoints remain continuable.

Sprint 21 reporting commit `d3c442ae11862ad61650206959d41c4343890337` was
documentation/state/evidence only. Its exact contents were synchronized by
[PR #38](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/38),
merged at `aa09788e6ba109c3db7bd0ec28f0138506f678a8`. No build, launch, version,
tag, asset or release changed. Sprint 22 starts at that master. Stable baseline
is v0.0.150, original product merge `3d8e51ee653d598c1ac7e9a6fd452c4d99767ffb`.

## Frozen rules interpretation

Use the adult PF1e animal in [Archives of Nethys](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Ankylosaurus)
and the [legacy Bestiary](https://legacy.aonprd.com/bestiary/dinosaur.html),
verified October 12. Huge, neutral, 10 animal HD; abilities 27/10/17/2/13/8;
75 hp; AC 22/8/22; saves 12/7/4; speed 30 feet; one tail +14, 3d6+12;
space/reach 15 feet; BAB 7, CMB 17, CMD 27 (31 against trip); Perception 14.
Great Fortitude, Improved Bull Rush, Improved Overrun, Power Attack and Weapon
Focus (tail) are the printed feats. Audit supported native consumers before
classifying a non-signature omission. Passive low-light vision/scent retain the
accepted limitation without substitute senses.

Stun (Ex): each qualifying tail strike makes one Fortitude save against
10 + half racial HD + current Strength modifier (23 unbuffed). Fortitude is
the mission's explicit interpretation: the primary paragraphs specify a save
without naming its type. A failed ordinary strike dazes for one round; a failed
effective confirmed critical stuns for 1d4 rounds instead. The outcomes are
alternatives. A valid absorbed-damage hit still qualifies: the rule requires a
strike, not injury damage. No poison precondition, mind-affecting descriptor,
size restriction, cooldown, daily limit, additional attack or damage event.

Observe effective native hit/confirmation/critical immunity and separate daze
and stun immunity. Duplicate callbacks cannot reroll a save or extend control;
distinct legal hits may reapply under native finite-duration rules. Native
victim-side conditions must expire and restore actions in RTWP and turn-based
play. Multiple sources and unrelated effects retain their own ownership.
Summon removal, cold load and module transitions must neither strand control
nor restart its full duration. No broad removal of dazed/stunned buffs.

## Identity and art boundary

Preserve unit `10c80d5cfa594332bd3e5127799e426f`, existing Tail3d6 weapon
`15394605e1664a51bce4b50f38a7603a`, all ten logical roots and ten SM template
children. Both families remain single at V, 1d3 at VI, 1d4+1 at VII–IX.
The authoritative catalog currently derives 530 SM + 533 SNA generated roots,
plus 29 native wrappers, 1,092 total choices. No suppression, tier movement,
new roster entries or global Hodag changes. Append only necessary private IDs.

Inspect existing Hodag/crocodilian/large-quadruped captures first; capture only
missing rig/action facts in one bounded guarded donor run. Author deterministic,
editable original geometry/albedo: low broad armored quadruped, small head,
plated/spiked back, substantial terminal club and readable contact motion.
Review clay, silhouette, unlit texture, party camera and tail extremes offline.
Reuse per-view attachment, native material/fader integration, fallback and
owned teardown. Capture bounds/bones and deterministic/package checks before
native qualification. Native absent Hit animation remains a donor constraint.
The existing original Ankylosaurus icon was inspected and already depicts
armor and a terminal club; retain it unless changed art reveals a concrete mismatch.
Owner aesthetic approval remains NOT_PERFORMED_NONBLOCKING.

## Requirement → carrier → witness map at intake

| Requirement | Existing or planned production carrier | Focused/native witness |
| --- | --- | --- |
| Stable ten routes/templates/quantities | Catalog, identity ledger, existing ability builder | Exact identity sets, authoritative global invariants; ten real route casts |
| Printed unit/HP/ranks/trip/feats | Natural profiles/builder, native animal class/facts; existing creation-only exact-rank carrier where required | Live spawned carrier/derived-stat readback and native weapon/maneuver rolls |
| Single tail 3d6+1.5×Str | Existing private Tail3d6 and native weapon stats | Huge dice scaling, native damage stats, separate Power Attack |
| Stun (Ex) | Native condition facts and narrow Ankylosaurus-owned attack handler if needed | Hit/save/crit/miss/immunity/absorbed damage/live Str, event uniqueness, two sources, expiration/actions |
| Original body/club | Existing per-view mesh/attachment and original deterministic asset pair | Offline anatomy/contact; native movement/selection/crowding/death/fade/fallback/resource ownership |
| Persistence/module lifecycle | Existing registered identity/module/save/visual mechanisms | Targeted fresh cold loads, remaining finite control, cleanup/absence, OFF/ON idempotence |

These are obligations, not qualification claims. The published profile currently
omits its signature rider, three feats and four-legged trip fact, uses generic
skill allocation and inherits Hodag type/art. Audit actual engine carriers; a
policy arithmetic PASS is insufficient.

## Validation and finite native plan

Editing: register nonzero `expanded-summoning.sprint22` tests and select real
dependency filters with Invoke-KmgGate.ps1 (Focused), including production
compilation when changed. Plan 15 minutes/checkpoint, ceiling 30 minutes, safe
completion/restoration excluded. Documentation-only checkpoints use text/link/JSON
checks. Review rules, code/lifecycle/module safety, art and diagnostic validity
before freezing one coherent candidate and running one `-Level Sprint` pipeline.

Selected later native/release budget: 300 minutes, including up to 15 for bounded
donor facts, 120 for targeted mechanics/art/routes in both modes and standalone/
highest-risk combined profiles, 75 for targeted fresh-load/lifetime/module/clean
absence, 30 for justified dependency comparisons and 60 for release verification
and restoration. Revise from measured timings before expanding. Safe restoration
is never interrupted for a budget. No unchanged replay after failures; after two
failures of one requirement classify and change strategy.

Every run uses repository guards, Steam 640820, exact transaction/run IDs,
fresh deployment receipts and its own actual intake. Use compatible disposable
fixtures; never load the human eleven-mod save under a reduced stack. Preserve
protected saves and prior failures, record native rolls/setup preconditions and
complete reset before bounded setup retries. Raw evidence stays outside source.
Reuse adequate v0.0.150 unaffected census/roster/module/profile evidence only with
named unchanged-dependency comparisons; no whole-series replay by default.

Only after acceptance: refetch master/releases, integrate legitimate movement,
select next unused patch (expected 0.0.151), exact-candidate promotion, normal
merge/publication and download/hash/tag/Latest verification. Reporting commits
remain separate from tested source/DLL/MVID/ZIP. Synchronize postpublication
RELEASED/STOP documentation through a small authorized follow-up PR, restore
today's actual installation/settings and clean owned fixtures/processes/leases.
No permanent deployment, no Sprint 23, no unrelated repository work.
