# Sprint 24 theropod dinosaurs — IN PROGRESS; NOT RELEASED

## Controlling state (2026-10-15) — read this section first

Everything below this section is dated history. Where it disagrees with this
section, this section wins.

**Reload contract.** `ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD` applies to
every KMG summon with no exemption. A save and reload reset active grabs, holds,
swallowed and engulfed relationships, the mouth occupancy that goes with them and
session-owned links — for Tyrannosaurus exactly as for the released Dire
Crocodile, Purple Worm and Giant Flytrap. This is a clean reset, **not** grapple
persistence. No creature opt-in, no swallowed-state preservation exception and no
project-owned re-establishment is authorized. The native Bleeding Critical effect
keeps its own separate lifetime, save behaviour and healing-removal rules. The
owner settled this on 2026-10-15; the October 14 handoff's swallowed-state
survival requirement is superseded, and any earlier text proposing a Tyrannosaurus
opt-in is historical.

The four accepted limitations, at their existing scope and with no new waiver:
`ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD`,
`SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`,
`PASSIVE_CREATURE_SENSES_UNMODELED`, `RUN_NATIVE_SUMMON_AI_DISPOSITION`.
`HumanReview` remains `NOT_PERFORMED_NONBLOCKING`.

**Latest executable source** is `bf9388d3cd9ced7adef6f91bc6a21520f1eb67b2`; the
reporting head is ahead of it in records only. **The qualified hidden candidate**
is that commit: source state
`fdb51df4f0f6efb4c353dfd9c8f2943c5457291f3ccd23ef39671b158cab0015`, package
`7746d002b382b58b129b7809a2907ed01b21f31521ae0c353a3aad59b364fd36`, DLL
`b8bffe1833bb7d577df93634b990e5501cfe311bf966cd6037de214457b0f4ee`, MVID
`c7f59041-9373-42e0-8a0f-9578d49f53d0`, version 0.0.153, sidecar validated. Its
Sprint gate passed repository validation and 2,608/2,608 domain tests with exact
compilation, deterministic packaging and strict validation, and deployed nothing.

**Stage route.** The Sprint 24 domain contracts assert that the eighteen roots
stay hidden and that targeted qualification never selects a published root, so
hidden qualification must complete while both keys are suppressed. The hidden
closeout therefore reuses the already-validated package above rather than
rebuilding, and exactly one public candidate follows it.

**Hidden closure is COMPLETE.** All seven closeout scenarios passed on the one
validated candidate `bf9388d3` / DLL `b8bffe18` / MVID `c7f59041`, run from a
temporary detached checkout at the tested commit so the sidecar identity matched.
No rebuild was made to stamp the reporting head.

- `shared-controls` replay, run `20261015T1956415320638Z-4adf192e492b429b85a05001af14859d`,
  85,815 ms, all seven rows, with one exact-owner brain tick, a native
  `UnitAttack`, real weapon rolls, 3.50 m approach and no fixture-issued attack.
- Cycle A — prepare (natural 19 confirmed; grab, grappled target, hold residue,
  engulfed-shut mouth, swallowed, bleeding; one save written), cold-load cleanup
  (`activeMouthHoldReset`, `alreadySwallowedReset`, `engulfedMouthReleased`,
  `bleedingCriticalPreserved`, `noOrphanInteriorPacket`, one 2d6 tick removed by
  healing, resource cleanup with zero survivors), fresh absence `ownedVictims` 0.
- Cycle B — prepare, module-OFF cold-load cleanup (same reset assertions true
  with `moduleEnabled` false, `donor-visual:module-disabled` recorded for both
  creatures, `theropodBodyExact` true, zero private and zero project resources),
  then fresh absence `ownedVictims` 0 after re-enabling.

**Released-behaviour dependency proof.** The `ResetLoadedGrapples` executable
loop is identical to released master `322ac7fe` — 33 lines each, zero differences
with comments stripped — so the loop was restored to its released behaviour
rather than keeping the unapproved exception. Released Sprint 16/17 old-consumer
cold-load evidence therefore applies to unchanged code, which is how the separate
actively-held-victim requirement is satisfied; this fixture proves only the
owner's hold residue and does not claim to prove both states. The separately
changed `SummonSwallowLifecycleComponent` victim-death configuration is covered
by the released-consumer controls inside `shared-controls`.

Machine audited and exactly restored: KMG 0.0.117/136 files, eleven mod
directories, 967 Mods entries, 118 saves, baseline save byte-identical, working
fixture left verified empty, sound bank unchanged, no leftover staging, lock or
process, and every transaction restored and verified.

**Remaining for Sprint 24:** the public candidate — unsuppress only the two keys
together, re-derive totals to 1,093 generated placements / 29 wrappers / 1,122
visible choices, move the version surface to the next free patch, extend the
existing qualified-candidate publisher with a Sprint 24 contract plus rejection
tests — then one Sprint gate, the publication-affecting native checks, merge,
stable release, independent asset verification, exact restoration and the
documentation-only records PR.

Both creature keys remain suppressed. No suppression, version, tag, merge or
release has been touched.

---

Claude takeover instructions are in
`CLAUDE-SPRINT24-HANDOFF-2026-10-14.md`. Treat its ordered remaining-work map as
the concise resume path and this file/coverage/ledger as the evidence record.

The owner selected the complete Deinonychus and Tyrannosaurus engineering and
release campaign on HOWARD-LAPTOP. Work is on
`codex/expanded-summoning-sprint24-deinonychus-tyrannosaurus`, based on current
released master `e825411a8f920fdb9447f7ad48d3f7d0267e6e32`. Stable v0.0.153
is the verified baseline; v0.0.154 was free at intake but must be rechecked at
publication. Sprint 25 has not been started.

## 2026-10-15 Claude takeover checkpoint

Intake on HOWARD-LAPTOP passed with two deviations from the handoff, both
resolved without discarding work.

**Upstream rewrote the repository lineage.** `origin/master` moved
`e825411a` to `322ac7fe` and the Sprint 24 product branch moved `886e6ad0` to
`5c5964da`, both as forced updates; the `v0.0.153` tag object also changed.
Every commit subject is preserved and the trees are identical — the branch tips
share tree `e0215299279e726b6a6fede1328b38d92ed63057`, released master shares
tree `4ce5e45c260c4cd2c36ee8da8df95c1dc0643e6d`, the tested source `fb7e471a`
shares tree `9314f862e1e8cfd2934f1af88a155eab34dae6f1` with remote `0784d0ef`,
and both `v0.0.153` commits share tree `57f0a0eb1368c51262365653b3f804fba63c9011`.
This is a SHA rewrite, not new work. The local lineage was preserved at
`codex/local-safety/sprint24-prerewrite-886e6ad0-20261015` and the remote
lineage merged in as an empty two-parent merge, mirroring the owner's own
precedent merge `94aebf3c`, which used the same pre-rewrite common ancestor
`91972f4e`. No reset, rebase, force-push or discarded commit. The local branch
is now a fast-forward descendant of PR #45's head.

Intake otherwise matched: product inputs unchanged between `fb7e471a` and the
reporting head (only `AUTONOMOUS-RESUME.md`, the Claude handoff, the coverage
record and the ledger differ); the hidden ZIP, sidecar, source-state, DLL and
MVID identities all verified; no Kingmaker process, transaction, lock, staging
or publisher checkout; `v0.0.154` still free.

**The handoff's limitations list was incomplete and the shared reload change
was out of scope.** Sprint 24's carriers declare four released limitations, not
two; the coverage record now reconciles them against
`ExpandedSummoningNaturalProfiles.cs`. The hidden candidate had also stopped
clearing swallowed state on load for every KMG summon, retracting the published
`ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD` promise for released Dire
Crocodile, Purple Worm and Giant Flytrap. That change is narrowed to a single
policy opt-in for the Tyrannosaurus, so the released load paths are unchanged
and needed no requalification. The focused Sprint 24 filter passes 14/14 and the
affected-seam selection passes 38/38 with production compilation.

Because this corrected shared production source, the `fb7e471a` hidden
candidate is stale. One new coherent hidden candidate was built before the
remaining native work; the old ZIP is not reused for it and its earlier offline
gate is not relabelled as execution on the new bytes. Its bytes are preserved
outside the repository at
`runtime-evidence/sprint24-superseded-candidate-fb7e471a/`.

**Current candidate** `70499a18b6b7437fd686b4e5eff446b27ac54be6`: source state
`05649b7f3ca7ba2cbaa7605432ff093575fce992f74f8a2b558aab52ebc0068b`,
local-runtime ZIP
`a45591472d65d6364230315c812205e9950be1368ca46756aa14111b4d591f16`,
DLL `78dce65d835bae16800dd3d4c48becf78c8e9eb5cc873da1e3532a54f4b492fb`,
MVID `39be1579-7fb7-4a13-a400-ef81d56bf345`. Fresh Sprint gate: repository
validation plus 2,608/2,608 domain tests in 100.5 seconds, 200.5 overall, no
deployment.

**shared-controls now PASSES**, closing the one unfinished hidden native phase.
Run `20261015T1656339936830Z-d7dbc7a93d8347918a382b1c0080a63e`, 96,330 ms, all
seven rows including `deinonychus-genuine-ai-fallback` with one exact-owner
brain tick against nine ambient candidates, a native `UnitAttack` carrying a
real `AiAction`, actual weapon rolls and 3.50 m of approach. Transaction
`compat-20261015T165501Z-e5ed30500b38` restored and verified; no save touched.

**The targeted persistence prepare stage failed as a fixture defect.** Run
`20261015T1705030393659Z-c88efbc2afa6415ab809c2170bce5cda` passed eleven rows
but failed `sprint24-state-fixture-prepared` and wrote no save. One cause:
`SwallowHeld` marks the swallowing mouth engulfed, and the Tyrannosaurus has one
bite limb, so after a swallow the hold is refused and a further bite auto-misses
with the log suspended. A single Tyrannosaurus cannot hold and have swallowed at
the same time. No save was written, both protected saves are byte-identical to
intake, and the installation restored to KMG 0.0.117 with its eleven mod
directories.

**The owner resolved the underlying scope question on 2026-10-15: clean reset
for all.** Sprint 24's requirement that the Tyrannosaurus keep its swallowed
victim across a cold load had no traceable owner charter — it appears only in
codex's handoff and coverage record — and it contradicted his decision of
2026-09-26, published in releases 0.0.139 and 0.0.146, that every KMG grab,
hold, swallow, engulf and mouth occupancy is session-scoped in exchange for a
clean reset, with project-owned re-establishment on load explicitly declined.
The opt-in was therefore removed rather than narrowed: `ResetLoadedGrapples`
again clears the swallower list and swallowed buffs for every KMG summon,
`SummonGrabComponent` carries no persistence field, and
`PersistsSwallowedAcrossLoad` is a pinned invariant returning false for every
token, with the profile self-check and
`expanded-summoning.sprint24-swallow-reload-scope` failing closed on any future
exemption. No waiver was added and no declined workaround was implemented.

That decision also removes the fixture's impossible premise. The prepare now
takes the confirmed-critical bite first while the mouth is free, then grabs and
swallows, and the active hold it resets is the owner's own residue from that
grab. The cold load requires `activeMouthHoldReset`, `alreadySwallowedReset`,
`engulfedMouthReleased`, `bleedingCriticalPreserved` and
`noOrphanInteriorPacket` — no 2d8+11 packet may fire — plus exactly one native
2d6 bleed tick that healing removes. Offline: production compilation and 136/136
across Sprint 24, the shared grapple lifecycle, grab carriers, universal grab
sizes, Dire Crocodile controls, the Sprint 17 persistence/load-reset observers
and the published-equation record test.

Two environment facts for the next session: the guarded harness refuses to run
under administrator elevation, so launch it de-elevated (`runas
/trustlevel:0x20000` works and keeps the needed write access); and Steam must
already be running, because its cold start exceeds the harness's 60-second
game-startup window. Also `Invoke-Sprint22InstalledStack.ps1` accepts
`TimeoutSeconds` up to 900 but forwards it to a child capped at 600, so the
handoff's prescribed 900 cannot bind; 600 is the usable maximum.

**The targeted persistence behaviour is now proven, under both module states.**
Running the cycles surfaced five defects, all observer or fixture gaps around the
new family rather than product faults in released gameplay, and each fix was
proven natively: the impossible prepare premise; the cold-load observer still
expecting a surviving swallowed victim; `RosterImmutableCaches` having no
theropod entry, so the albedo textures classified `UNRESOLVED` and failed
cleanup closed; the theropod identity gate returning before the attachment
record so module OFF reported `not-attempted` instead of the loader's
`donor-visual:module-disabled` refusal; and the wrapper's 900-second timeout
being unbindable against a child capped at 600.

Cycle A passed coherently on candidate `fa55ef68` (DLL `c11f944c`): prepare
proved the grab, grappled target, active mouth-hold residue, engulfed-shut
mouth, swallowed state and a natural-19 confirmed bleed, then wrote one working
save; the cold load proved `activeMouthHoldReset`, `alreadySwallowedReset`,
`engulfedMouthReleased`, `bleedingCriticalPreserved` and
`noOrphanInteriorPacket` with one 2d6 tick removed by healing; fresh absence
reported `ownedVictims` 0. Module-OFF cold load passed on candidate `bf9388d3`
(DLL `b8bffe18`) with `donor-visual:module-disabled` recorded and
`theropodBodyExact` true for both creatures.

Machine audited and exactly restored: KMG 0.0.117 with 136 files, eleven mod
directories, 118 saves, the baseline save byte-identical to intake, the
authorized working fixture left verified empty, the sound bank unchanged, and no
leftover staging, lock or process.

Next concrete action: the five fixes each changed the DLL, so the evidence is
spread across candidates. Build one final hidden candidate and run one coherent
set on it — Cycle A, Cycle B and the `shared-controls` replay, the last of which
must be rerun because the shared grapple/swallow seam it covers changed after
its PASS on `70499a18`. Then publication: remove only the two suppression keys,
regenerate totals to 1,093/29/1,122, move the version surface to the next free
patch, extend the qualified-candidate publisher with a Sprint 24 selector, run
one fresh Sprint gate on the frozen public source, then the public high-risk
inventory and all 18 public routes, then merge, release and the records PR.

## 2026-10-14 quota stopping checkpoint

The owner requested a stopping point before the final native replay. Product
source checkpoint `fb7e471a9877fb68cc0cb26feadc04615882a1b2` is pushed and
draft product PR #45 remains open. Its fresh full Sprint gate passed the
repository checks and all 2,607 domain tests: 94.3 seconds for the complete
test run and 192.1 seconds overall. The exact local-runtime ZIP SHA-256 is
`333a8c8772f089fd5b1e2d47bcfcc92c53f375360c120944fdf7b3cff0c82051`;
the compile-candidate ZIP SHA-256 is
`750089752b4b37b5e7e5b0c5fb2761eb705d8b67463321d2c0fc3c7d94f9cad5`.
The DLL SHA-256 is
`24af1a3fb3b88410b26656ad2d457edf245b24012858c3ac26e6d316f206c867`
with MVID `cd939d5b-6678-4c1f-8e26-ad5c761992d7`. This gate did not deploy or
launch the game.

The remaining shared-controls native check is not green. The last guarded run
used the preceding AI-controller strategy and stopped fail-closed after four
control assertions because the controller candidate set also contained nine
protected party actors. Its evidence is
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261014T1338497545977Z-disposable-expanded-summoning-sprint24-native-review`
(run `20261014T1338497858128Z-db9c9e1da2754e1398037313c3b5a76b`). The current
source replaces that broad controller tick with an exact native per-unit
`DecisionContext.InitGroup -> AiBrainController.TickBrain -> ReleaseGroup`
sequence and requires a native AI-owned `UnitAttack`; it passed focused checks
and the full Sprint gate but has deliberately not been replayed after the stop
request. Preserve the failed evidence rather than treating the offline pass as
native proof.

Resume with one guarded shared-controls replay against the exact current
package. If it passes, the remaining campaign work is the targeted
persistence/module-OFF batch, removal of only the two suppression entries,
final public-artifact qualification, normal product merge/publication and the
documentation-only final-records PR. No suppression was removed, version/tag
was created, PR merged, or release published at this checkpoint.

The last runtime transaction
`compat-20261014T133710Z-3cf6f796211f` restored successfully at
`2026-10-14T13:40:41.1768330Z`; restoration verification passed. The human
installation remains KMG 0.0.117 with its intake installation/settings/save
state, and no runtime launch occurred after the owner requested the stop.

The primary Pathfinder First Edition rules, original charter pages 21–22,
current repository policy, Sprint 22/23 reuse seams, and actual machine state
have been read. The compact source contract and requirement-to-proof map are in
[the Sprint 24 coverage record](docs/SPRINT24-COVERAGE.md); exact intake and
policy identities are in
[the machine-readable ledger](planning/SPRINT24-THEROPOD-DINOSAURS-LEDGER.json).
Both new catalog keys must remain unpublished until qualification.
Draft product PR #45 targets master directly.
The released v0.0.153 surface remains 1,075 generated placements plus 29
retained native wrappers, for 1,104 visible choices. The hidden Sprint 24
candidate must keep that public equation until its 18 new roots qualify.

Fresh intake evidence is local-only at
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/sprint24-deinonychus-tyrannosaurus-intake-20261014T050745Z/intake.json`
(SHA-256 `c54cfea1d5ee6577760ec710d37167951d2367a3e12088b8f823244c5f54a450`).
It records 968 Mods entries, installed KMG 0.0.117 with 136 files, 83 settings,
118 saves, the protected baseline SHA-256
`cc7cbb0d08581873ed0ad2a6ac8ebd16a95333b5665cd74dcd0c538e16119c07`,
and authorized working-save SHA-256
`795d59ff448173c6631518676b655d3bd03b31dc31e418f578f9e9488cb10ada`.
There was no Kingmaker process, runtime transaction, lock, game staging, or
publisher checkout.

The external push wrapper changed only by adding the two owner-authorized
Sprint 24 branches. Its SHA-256 moved from
`688b12798eac1b46b63a28205acbb647825e371cd3e9ae5e4db0ee13610d2fb2`
to `9354658937f2e679d3b77493a8bc56eed2395a53b8da3614b4e2cd5a05246542`;
removing those exact two lines reproduces the original hash.

The one bounded theropod census completed PASS on closed inputs: the Free Hands
Lizardfolk candidate, Troll negative control, six released feat carriers,
Diehard, Endurance, and bounded native bleed/critical graphs. The fresh complete
prebuild passed 2,596/2,596 domain tests in 88.5 seconds. The guarded native run
passed all four assertions in 115.399 seconds under Kingmaker 2.1.7b. Its local
result and census SHA-256 values are respectively
`893d72987c0154877571378d72c7319f692f5131c77f788d3b2ac68daa761b35`
and `727645bba689fc6623b8a690d524d751c96d9072809d4c8ce22e8ed6191ad7a4`.
The exact research ZIP SHA-256 is
`5c03793a412e535b214baaef3790eba539a363fc85406a39e7a397c84ce42169`;
its DLL SHA-256 is
`00e43cc30f132981b4201998bcd2f315d0d588e2fab7bc75955fb1bc73f80d11`
with MVID `fd089740-039a-4aa6-9f68-361413932273`.

The first guarded census request stopped before deployment or launch because
the frozen Sprint 23 source validator did not yet admit the two later-sprint
research files. Its preserved failure is classified as an offline harness
admission defect, not native evidence. The correction adds only those exact
research files to the historical allowed set; it still rejects every Sprint 24
gameplay builder/review/policy file. All eight corruption fixtures and the
Sprint 23 source validator pass after the correction.

The next guarded request also stopped before deployment or launch: its complete
domain pass found the active resume had omitted the exact released public-count
phrases required by the source-derived record test. The line above restores
those facts without changing product or package inputs; the failure remains in
the machine-readable ledger.

The census freezes the Free Hands Lizardfolk rig as the shared authoring base:
it has independent legs/feet/toes, four tail joints, jaw, both forelimbs, and
separate claw and bite action contacts. The two-joint-tail Troll remains a
negative control. Native `Bleed2d6Buff` is the exact standard 2d6 bleed carrier;
Kingmaker has no native Bleeding Critical feat, so the candidate needs a narrow
confirmed-critical trigger but must not clone the native bleed implementation.

After the census transaction, the sanctioned restore returned the actual KMG
installation to 0.0.117 with the exact intake 136-file canonical hash. All 83
settings, 118 saves, both named saves, Mods inventory, and sound-bank state match
intake exactly. There is no game process, active transaction, compatibility
lock, or game staging. Two initial registry restore attempts exposed JSON
binary-value type hydration in the restoration helper; both failed closed, then
the sanctioned settings restore succeeded with the exact intake hash.

The hidden mechanics checkpoint, original-art pipeline, closed native observer,
and targeted persistence observer are now implemented on draft PR #45. Release
production compilation and the named Sprint 24 filter pass 13/13. Guarded
preflight passes 588 checks; source-only persistence, launcher-routing and
installed-stack restoration fixtures also pass. Both keys remain suppressed.
Original icons and deterministic
biped bodies are packaged through the existing asset owners; the exact
two-renderer Lizardfolk view is handled per instance with reversible accessory
suppression. Offline review found and corrected a jaw-pivot defect and then a
neck/tail transition defect; the failed local frames remain preserved. The
complete corrected 20-frame offline review passes, with 181.63 seconds of
Blender rendering and sorted hash-manifest SHA-256
`504ea9cfaed5723e74b3b67b81ef43155baf71877510de80bbc9de3b950331ea`.

The corrected hidden candidate at
`905c1f7e926924e5ce0b636df2875fc20a2603ba` passed the complete Sprint gate:
repository validation, 2,607/2,607 domain tests in 91.7 seconds,
exact-reference compilation and a 370-entry strict ZIP. The 188.9-second gate
froze package SHA-256
`fbab7c4126d785acef8885e0d0b2d17517a87af0b7dfe5d0614c637d8d7f3807`,
DLL SHA-256
`4de957a8bad495ff94c8e04d70b285102871578b59de2013269fda6a7e3b9422`,
MVID `2534e2fd-4c1d-4758-9708-4cc14e5493ba`, and source fingerprint
`feaa8f8ca0eb4d97331725c89ea41cc368a50ec0b808f2cdb7eabb1ff047ea75`.

The corrected `rtwp-foundation` reproduction passed 26/26 assertions in 95.127
seconds under transaction `compat-20261014T085021Z-254c16326dc0`. Both exact
adult profiles, body/accessory ownership and all 18 hidden real-parent routes
passed. Runtime-result SHA-256 is
`ad9c4b2c7e609ace23515213818d8a9241ebd8f39634e6dc7b000c43c5b4eb4e`;
the evidence is
`runtime-evidence/20261014T0851597922754Z-disposable-expanded-summoning-sprint24-native-review`.

The next `rtwp-deinonychus` phase retained 5 PASS and 5 FAIL assertions in
93.356 seconds. The actual native run already proved one exact four-limb
ordinary full routine, a one-strike AoO, and clean teardown. The failed profile
was an observer setup defect: the shared quiet-unit helper had deliberately
inflated the summon to 100,000 base hp before the adult readback. The body was
sampled before its fade/selection settle. The ordinary single command omitted
the engine's explicit `IsSingleAttack` carrier. The legal Pounce moved 11.638 m,
spent native charge economy and emitted exactly four charge rolls, but a natural
1 was incorrectly treated as a routine failure. The too-close target predicate
correctly refused the charge and emitted zero attacks/state, while the assertion
incorrectly required the generic command readiness flag to be false. Transaction
`compat-20261014T085422Z-eb67bbd411f8` restored exactly; no process, lease, lock,
staging or permanent deployment remains. Runtime-result SHA-256 is
`72fa99628e0ddae8a3d9457e3f03509a16be6395dff8a9265b891ba46382ddb7`;
the retained evidence is
`runtime-evidence/20261014T0856001045658Z-disposable-expanded-summoning-sprint24-native-review`.

The batched observer correction now samples a normal own-tier summon after the
same 12-frame foundation settle, then applies fixture survivability; uses the
native `IsSingleAttack` contract; records controlled actual d20s without making
all hits a Pounce legality requirement; and treats native target refusal plus
zero attacks/stale state as the illegal/aborted result. The exact correction
commit `aa048f1db9d279ea6185ec7ab1b3534eac93ff2a` passed a new complete
Sprint gate: 2,607/2,607 in 91.5 seconds and strict package validation in 186.1
seconds total. The 370-entry ZIP SHA-256 is
`68ce7f2d0699cc07f61903ca460b78ffd34539784f2543eba7ab6f810988fbd1`;
DLL SHA-256 is
`e69ae58067b5f69a6b6b501a2cb8fbba3c7125f688c566b49520f419ebbcb3f8`,
MVID `9228e5b3-1edd-44c7-9591-14520fe31c6f`, and source fingerprint
`78ad09ba48adfaf2b902c8fbd12bd8f85aa793bbb2e37bfdf8cee3f03f98eda5`.

The corrected `rtwp-deinonychus` reproduction passed 10/10 assertions in
92.724 seconds. It proves the exact adult profile and body, an ordinary native
four-roll routine, one native primary-talon single attack, a 11.703 m legal
Pounce with exactly four charge rolls and ordinary economy, a rejected
too-close charge with no follow-up/stale state, a one-strike AoO and exact
teardown. Transaction `compat-20261014T090839Z-de4595196cfc` restored exactly;
runtime-result SHA-256 is
`e3f8c65e72faceabedc11041b1b48432f813e8d3408e99930e479fa2ee05a791`.

The first `turn-based-deinonychus` phase retained 5 PASS and 3 FAIL assertions
in 194.520 seconds. Profile, body and teardown passed, but the ordinary full and
single commands emitted zero rolls because the summon had never entered native
turn order. The subsequent route survey raised a native
`AbilityCustomCharge.CanTarget` null dereference because it ran while TB mode
had no valid current-turn path. This is the same harness ordering already solved
by the qualified Sprint 22/23 controller, not a creature-mechanics result.
Transaction `compat-20261014T091235Z-920f582e4222` restored exactly;
runtime-result SHA-256 is
`a4758447f28aea0df639bf0cadb73e5e68b00dfb99f63b601a2f584e1e1857cf`.

The exact turn-controller correction commit
`eea076088c695ebb7cf48932f452f59c819e61e0` passed a fresh complete Sprint
gate: 2,607/2,607 tests in 90.4 seconds and a strict 370-entry package in 186.2
seconds total. ZIP SHA-256 is
`4156261f122646fd6ab952916c8358dbbecb150981b17be958ec3a6e3af6e4ab`;
DLL SHA-256 is
`e41019388e5af11bb11020bb4d681b082443b0d5e55eb4f371870fd93ff80549`,
MVID `f70e80a8-1e81-4d0c-be9e-651422b156de`, and source fingerprint
`5b5c3ae05e8f9e11a9b804231af4def80bf4e7723b1472d95895328a61795c20`.

The changed `turn-based-deinonychus` reproduction then passed 9/11 assertions
in 100.638 seconds. Native turn admission, the exact full and single routines,
and an 11.097 m legal four-roll Pounce all passed. The two negative rows emitted
zero illegal-charge attacks and exactly one non-charge AoO, but both found the
legal charge marker still present. Raw evidence showed the local observer's
shared round helper had returned during an inter-turn null-controller window,
before the owner's actual fresh turn; it did not show a duplicate Pounce or
creature-mechanics failure. Transaction
`compat-20261014T092558Z-43136354a7f7` restored installation and settings
exactly, with no process, lock or staging left. Runtime-result SHA-256 is
`3995732f5bda934a030ac83ed778e8d0fbd25384a13eade57628213b78a1cf6e`.

The exact fresh-turn correction commit
`3ed00e74c5602b6a28609a063481e6cb81b100db` passed a complete Sprint gate:
2,607/2,607 tests in 108.8 seconds and a strict 370-entry package in 214.2
seconds total. ZIP SHA-256 is
`923c2df33004b9fedcd2b23dcf514b7192b4ac407718d9ae54d5fae7f932b712`;
DLL SHA-256 is
`4d478541a126908e349121c6451f3a19fea9dc4468de143a63e51c7a0c8d7854`,
MVID `e0ce5b26-0e79-4a59-a71b-f0726d45196e`, and source fingerprint
`ba154235e334df4df69bbf1a2f65a062f3ab88bd186cd7d4f2e9c1706cccdcd6`.

The corrected `turn-based-deinonychus` phase then passed 11/11 assertions in
111.715 seconds. Its native charge marker was present immediately after the
four-roll Pounce, expired at the owner's actual fresh turn after 57 frames,
stayed absent through the rejected charge, and the AoO remained exactly one
non-charge talon. Transaction `compat-20261014T094217Z-8e09a731bba0`
restored exactly; runtime-result SHA-256 is
`da9c62855c9d2854e5c2bbc607f6bde642a4ae3bcde6ed6aa2964143026ed609`.

The first `rtwp-tyrannosaurus` phase retained 8 PASS and 2 FAIL assertions in
91.386 seconds. The adult profile, one 4d6+22 bite, all signed Powerful Bite
cases (+9/+10/+11/+12/zero/penalties), independent simultaneous swallowers,
three exact 2d8+11 periodic packets, native escape and cleanup passed. The
critical row proved 19–20 threat, +4 Critical Focus, no auto-confirm carrier,
confirmed critical multiplication, one native 2d6 bleed tick and healing
removal, but its fixture +100 attack bonus let a natural-1 confirmation beat
AC. Those real bite probes also established an unrelated mouth hold, so the
following same-size Grab attempt correctly refused an already occupied mouth;
the independent Huge-target Grab/later-Swallow path still passed. Transaction
`compat-20261014T094705Z-c7e1b4e3f5b9` restored exactly; runtime-result SHA-256
is `b3cfe78650c28cf814ed0c15e93b7dc6504bc81d6a4a0e041da1d956c4527291`.

The exact critical/hold correction commit
`c95a8c03ec644c81f5c0bfe2a0faaff3f7215ded` passed a complete Sprint gate:
2,607/2,607 tests in 94.4 seconds and a strict 370-entry package in 190.4
seconds total. ZIP SHA-256 is
`e3c07a543796035e84f4d65a5783db72a42348683c931c442ba77742e104fc89`;
DLL SHA-256 is
`d325dbae5437080e8efd3661e8ff1385b424dac70da66c1ba87a4765f1676b30`,
MVID `5054599d-6a6d-442b-b323-38ca37dd7a26`, and source fingerprint
`665b32f6805cd8132d3bfccba307c47cd38903cb851703e418b4c66756b6bdab`.

The corrected `rtwp-tyrannosaurus` reproduction passed 10/10 assertions in
91.283 seconds. Its natural-19 threat confirmed on a natural 20 and did not
confirm on a natural 1 at AC 30; native auto-confirm remained false. The exact
one-bite routine, every signed Powerful Bite case, same-size Grab without
swallow, later Huge-target swallow, three separate 2d8+11 packets, escape,
independent predators and cleanup all passed. Transaction
`compat-20261014T100058Z-cf1d7b003d74` restored exactly; runtime-result
SHA-256 is
`03a5c6ce33c163bd0838f21f101758b86b4a45396babf88111914db48e6dc11c`.

The first `turn-based-tyrannosaurus` phase retained 10 PASS and 1 FAIL
assertion in 91.969 seconds. Its profile, turn admission, one-bite command,
signed Powerful Bite matrix, critical suite, same-size/Huge Grab and later
Swallow, three-round cadence, escape, and the first independent 2d8+11 packet
for each of two predators all passed. Releasing the first victim preserved the
second victim's native swallowed part and one-victim owner list, but the
continuation probe emitted no second packet. The probe had reused the
pre-release Buff reference and pre-release due time. Transaction
`compat-20261014T100454Z-b4ff58a2e19b` restored exactly; runtime-result
SHA-256 is
`dcf214490735a54645f580caccd8bb801fbd457e85a264b7d51dd1d2bd5b54db`.

The first continuation correction commit
`ca80f133e538c9607f6795297e0b2f6bd67c6ad7` passed a complete Sprint gate:
2,607/2,607 tests in 93.1 seconds and a strict 370-entry package in 188.9
seconds total. ZIP SHA-256 is
`4fe19f8d4b1129c4e8e88de9c1dd366436d68225983fec1b6e618c9151986215`;
DLL SHA-256 is
`333fcc2a81d7c8bbc0ec53128bdc4bc3cc68a314a7e2a17391f4befcb71baa4f`,
MVID `d53c7878-0ff6-4839-8d26-3bcb0bdaa4da`, and source fingerprint
`33e13b3989ee99b6768bffdd591f25c613c79bca14facc71067607dbf32b5d96`.

Its changed TB reproduction again passed 10/11 assertions in 93.564 seconds
and preserved the same single continuation failure. The new diagnostics proved
the second Buff reference was stable, its caster/victim ownership remained
exact, release caused no tick, and the first victim stayed released. The
surviving Buff was still at the already-processed current turn boundary before
and after the manual collection call (`0.0s` delay), so that call was not a
later native round. Transaction `compat-20261014T101641Z-a38d3a138c4f`
restored exactly; runtime-result SHA-256 is
`97f760a9f13d7504a405a7373f5cd48ba8dce67abf98c88c3379b45780e3bb71`.

The native-clock correction commit
`8aec80714cf8ca77f92d6ae72d065addf33161e1` passed a complete Sprint gate:
2,607/2,607 tests in 95.3 seconds and strict package validation in 191.4
seconds. ZIP SHA-256 is
`738b15aca71d51d00c691f8b9c9da83cdb84a64b4dccfa34472c0159d3728d8e`;
DLL SHA-256 is
`4029dccf88ce60b9ae57a8aee11f21afb641bea29ac1a11cee8a43ba1391ce7e`,
MVID `37420198-db58-4cbc-ba73-51685e56d840`, and source fingerprint
`7160ad8324995115dbeefc64d2b4014bc0cdada21cc095025bc1464567665fb5`.

Its changed TB reproduction again passed 10/11 assertions in 95.006 seconds.
All Buff identity, release-isolation and cleanup predicates remained exact, but
32 passive controller pulses advanced only 1.666 native seconds while the
current TB turn remained open, so the naturally scheduled second packet was
not due. Transaction `compat-20261014T103110Z-be8cc748cdf9` restored exactly;
runtime-result SHA-256 is
`d392f8f3c3ca892fff698bdbf42b088d9e050e33d937614f2c8ee15913a63e18`.

The native-turn correction commit
`8558144395bd65522a26ad59788d1dcbcacb30f4` passed a complete Sprint gate:
2,607/2,607 tests in 93.0 seconds and strict package validation in 189.8
seconds. ZIP SHA-256 is
`c596cd82b8c75c07b3eed8a2a5e5722db696087cd0d3f0542d9ace7f741cc437`;
DLL SHA-256 is
`62056224b8744a138a401900eaa41ae8d2feacb373cdc462d4dcb4ff320e1166`,
MVID `24c973aa-73f6-49bd-aa2a-8136298b84b0`, and source fingerprint
`747c8723b63c71fec814dcd0651293cea0af9cdab2b6aa3fbd372e43966c5a43`.

Its changed TB reproduction again passed 10/11 assertions in 121.703 seconds.
It advanced 900 native pulses, ended 24 turns, crossed rounds 1 through 7 and
advanced 35.333 native seconds, but the second Buff did not tick. The frozen
Kingmaker contract explains the result: in TB a Buff ticks only while the
current unit is its owner or caster, and the disposable second pair had never
been admitted to turn order. Transaction
`compat-20261014T104352Z-2fb0bc1662a8` restored exactly; runtime-result
SHA-256 is
`931256a5b1fb7b5d960d1113f8dc8f4c774bf13b1bc2a5e4b887ffb994f04428`.

The second-pair enrollment correction commit
`55695b0913a35b9a3a5521c780814b35b55ed3dc` passed a fresh complete Sprint
gate: 2,607/2,607 tests in 90.6 seconds and strict package validation in 185.2
seconds. ZIP SHA-256 is
`44102a5c8833774a00ccc2b5a9f9a623561d35e1940dd7e5ce39ed7e5ca8865d`;
DLL SHA-256 is
`80e1dd837ed3ff7a02b491a705975288bd15b195c56d60e4167d38e930eda75e`,
MVID `01329835-0f78-4d37-88e6-04c27c8d589c`, and source fingerprint
`2e0cb8a932d2cd1edf0a3d7bad48a954cc3dd39d48e48fd631bf0fc23ee50ba3`.

The changed `turn-based-tyrannosaurus` reproduction then passed 11/11 in
102.756 seconds. Pair two entered native turn order; pair one delivered one
exact 2d8+11 packet and was released; after 200 native pulses, five ended turns
and controller round 1 -> 2, pair two delivered exactly one second-owner
2d8+11 packet while pair one stayed released. Transaction
`compat-20261014T105815Z-6db7ea49dd7c` restored exactly. Runtime-result SHA-256
is `508a70436a939677f6052fbf65c14aaaf83f87d057463383521b2ac0915a8f2d`.

The first `visual-lifecycle` run retained 6 PASS and 5 FAIL assertions in
98.290 seconds. Two recovered Deinonychus views bound the original body and
suppressed only the accessory; carrier inventory, donor rollback, fixture
cleanup and guarded machine restoration passed. The resource receipt counted
one native Lizardfolk material-controller instance that the exact release path
does not own, causing the injected-fault row and all three teardown rows to
fail. The shared transform formatter then assumed an unrelated serpentine
attachment and aborted on the theropod death sample. Evidence is
`runtime-evidence/20261014T1103586702473Z-disposable-expanded-summoning-sprint24-native-review`;
runtime-result SHA-256 is
`5cf24f7b0b3e217dc29345131052fa5571da7944bbcb38c84d141499c532212e`.

The first correction commit
`b3c92b1c49a1a03f2e7d3b367985442f62a3c3f3` was rejected by the Sprint 24
source boundary before compilation or packaging because it modified the older
Sprint 17 formatter. The narrowed correction restores that file exactly and
keeps the theropod transform formatter in the Sprint 24 observer. Its first
focused compile exposed one missing existing `Kingmaker.View` import; after
that import, production compilation and all 13 named Sprint 24 tests pass.
Neither failed check deployed, launched the game or accessed a save.

The current batched correction filters theropod receipts to the exact
`KMG_<key>_Original` material family that the release path owns, recognizes the
qualified theropod driver set in a Sprint 24-only full-transform formatter,
contains a formatting exception to its diagnostic row, and records any
remaining teardown resource explicitly. Product mechanics remain unchanged
and both keys remain suppressed.

Next action: commit and push this visual receipt/formatter correction through
the required wrapper, run one fresh complete Sprint gate on that exact commit,
then rerun only `visual-lifecycle` with `-ReuseInstalledArtifact`. Continue
`shared-controls` only after the changed visual reproduction passes. Do not
begin Brachiosaurus, Elephant changes, companions, polymorph, mounts, feats, or
any Sprint 25 work.
