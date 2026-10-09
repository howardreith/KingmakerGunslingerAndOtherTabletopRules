# Sprint 17 snake crowd/persistence fixture repair

Historical prelaunch review: SOURCE PASS. Exactec9721f7 later passed crowd18
and prepare13 but failed cleanup10/12; full Sprint17 NOT QUALIFIED.
Current authority: [attempt2 review](EXPANDED-SUMMONING-SPRINT17-SNAKE-CROWD-PERSISTENCE-ATTEMPT2-REVIEW.md).
Source parent: 9de166145d71dbf5efafe33b88550d4539f76f2f.
Laptop PR26 only; DATA salvage-only / ZERO PORTS. No publication/version change.

## Evidence and bounded disposition

The [exact 4c95 failure](EXPANDED-SUMMONING-SPRINT17-SNAKE-CROWD-PERSISTENCE-ATTEMPT1-REVIEW.md)
is preserved: smoke 11/11, direct 14/14; crowd 17/18, prepare 10/13,
dependent cleanup 3/6; absence NOT RUN. Zero native writes; actual snapshot
20261007T0020146810604Z restored exactly at 2026-10-07T00:42:42.5922110Z.
At this prelaunch checkpoint, no runtime had occurred since that restoration.

The Viper crowd assertion reports only awakeRestored=false. Its actual
changed identities were not recorded. The old fixture removed entries it
added, but did not restore preexisting owned entries that native processing
could sleep or reorder. This repair does not assert which happened.

The new closed two-snake helper restores only the exact two-to-five owned,
live, same-area object references. It first compares the ordered non-owned
subsequence with the snapshot. Any foreign change rejects without mutation.
Otherwise only owned entries are removed/reinserted at their snapshot
positions. The full final SequenceEqual assertion and all movement thresholds
remain. Before, native-after, restored and owned identity arrays are recorded.
Other creatures retain their existing fixture behavior; no global scheduler
or gameplay change.

Prepare's missing venom remains unexplained. The same actual seeded bite now
records its roll/hit/critical, bonus/AC, final damage versus missing damage
event, target health, weapon, enemy relation, live Constitution and native
injury/buff saving throws and Constitution damage. Diagnostic labels never
alter outcomes. Temporary fixture BAB/Fortitude and observer subscription are
restored in finally. No poison production change, forced saving throw or
weakened arming assertion.

A separate fixture precondition was missing: visual dissolve completion did
not prove the native summon appearance lock had ended. The existing movement
fixture already checks that lock, and the Sprint16 second-batch evidence
records retained native CanAct/CanMove restrictions. Prepare now naturally
waits for intact fade, CanAct, CanMove and absence of that exact native buff
inside its unchanged 600-frame budget. It records all four states and native
elapsed time; on failure it stops before an attack or save is armed. No buff
removal, visibility forcing or world-time jump. This is not claimed as the
proven cause of the prior missing venom.

## Source checks

Five behavior tests cover owned identity restoration, nonmutating foreign
rejection, malformed/dead scope rejection, native arming outcome labels and
appearance/control readiness (including NaN/infinite dissolve rejection).
Registered development count 2,071; immutable historical counts unchanged.

PASS: 269 focused; complete unfiltered 2,071 tests in 82.3s; complete repository,
static/icon/manifest, clean 14-reference Release, strict 320-member package
in 178.6s. Standalone preflight 508; orchestration 168; provenance 17;
persistence 11 plus 6 round trips/3 defaults/56 rejects; crowd 8/11;
actual outer launcher WhatIf 13 valid/10 rejected. No runtime mutation.
Private logs: artifacts/sprint17-snake-crowd-persistence-repair-*.
Earlier source gates 1/2 rejected stale development test-count metadata;
gate3 passed 2,070 before the final readiness test; gate4 passed final 2,071.
Every log and old candidate remains. Dirty-source output is not an exact-head
runtime artifact and was never deployed.

## Next exact gate

Commit and policy-push this NOT QUALIFIED checkpoint. Repeat all prelaunch
gates on the immutable committed head, archive exact DLL/MVID/ZIP/source
fingerprints, then repeat smoke/direct/crowd/prepare/cleanup/absence under
one lease and actual snapshot with fresh Steam App ID640820 processes.
The exact 11/14/18/13/12/5 assertion and 0/0/0/1/1/0 write contracts remain.
Absence requires a successful native cleanup save. No manual save surgery
or protected-baseline load/write. Preserve failures and exact restoration.

Sprints14–16 complete.32 snake roots hidden. Native hit/death/fade/lifecycle,
UI/private routes, separate Salamander, full Sprint17 hidden/publication and
Phase2B closure remain mandatory. HumanReview: NOT_PERFORMED_NONBLOCKING.
Stop after Phase2B owner review; Phase2C authorized but deferred.
