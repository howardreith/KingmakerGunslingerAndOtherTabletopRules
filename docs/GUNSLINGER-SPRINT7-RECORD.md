# Sprint 7 record: presentation and audio remediation (S7 T1–T7)

Status: **development candidate for review (2026-11-24), review round 2; nothing is merged or released.** One draft PR
(#219), one agent-review enrollment (`kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`, foreground, single
plan), branch `codex/s7-visual-audio-remediation`. Mission: `docs/missions/sprint7/SPRINT7-AUTONOMOUS-MISSION.md`
(verbatim, sha256 `c232f135…f56f0`). Technical status, agent recommendations and owner acceptance are separate below;
every visual and listening judgment remains the owner's. Round 1 (reviewed head `0910995241`) returned 14 blocking
findings; §7 maps each to its correction.

## 1. Starting point

- Sprint 6 was merged (PRs #198–#216) and released: stable **v0.0.195**, release commit `ef0e1fe15d` (PR #217), records
  PR #218. Post-release `origin/master` is **`35763430ac`**, the base of this branch.
- Published package `456d504d…9444`, DLL `3ef052ea…6a6b` (`docs/BUILD-AND-RELEASE.md`). The local release copy
  `worktrees/publish-master/artifacts/release/0.0.195/…sprint6.zip` has the published hash.
- Environment: disposable loader-compat game copy with Unity Mod Manager owner profile 0.33.0 and no other mods;
  harness `KmgLoaderCompatHarness.dll` `04c9d1d4…cfa4` for round 1 and `9e90d22c…72bb` for round 2 (adds the
  `kmgTrumpetCue` probe, `tools/LoaderCompatHarness/TrumpetCueProbes.cs`); Blender 4.5.10 `8b15ef73…313c` with
  `PYTHONHASHSEED=0`.
- Carried forward unchanged and not part of this sprint's acceptance: Sprint 6 accepted scopes, REP-05/REP-06 BLOCKED,
  VER-02 open with G5 fixture-blocked, and the other retained limits in `docs/GUNSLINGER-SPRINT6-DISPOSITION.md`.

## 2. Outcome

| Story / record | Original problem | Result | Evidence (round 2) | Technical status | Remaining owner decision |
|---|---|---|---|---|---|
| S7 T1 / REP-04 | October review: "increasing" Ghaele dissolve and a scripted move that did not start | **Attributed; no product change.** The value read is the native looping, non-cutout dissolve animation (delay 4.75 s, lifetime 5 s) that the unsummoned native donor also runs; the body is never removed; ordinary movement and attacks work; expiry at the native 120 s | `f8cefba85d`, session `…025726Z` `gh` 12/12, `ghd` 7/7 | Disposition with native evidence (bright and dim) | Accept the attribution and the no-change disposition |
| S7 T2 / VIS-01 | 48 zero-area wing-root triangles (and inside-out welded membranes) | **Repaired at the generator.** 0 zero-area, 0 repeated edges, all parts outward | `ed88ad994e`; decoded regression; `…025726Z` `mod`/`modd` PASS | Implemented and verified | Visual acceptance of the wing roots |
| S7 T3 / VIS-02 | 19/12,924 normal corners at a waist pleat and left-hand fingers | **Repaired from the exact private input.** 16 → 0 opposing corners | `d864e7b01f`; decoded regression; `…025726Z` `mod`/`modd` PASS | Implemented and verified | Visual acceptance of waist and fingers |
| S7 T4 / VIS-03 | Owned Trample/Overrun windows: "Target point within 0 ft. feet." and school "None" | **Scoped presentation override.** "Target point within twice this creature's speed."; no school | `2614ea3bd6`; `…025726Z` `ui` 15/15 with native controls | Implemented and verified (desktop fact and HUD paths); controller mode blocked (§4.4) | Wording acceptance |
| S7 T5 / VIS-04 | No added impact sound on the Ankylosaurus tail strike | **One owned cue per resolved tail hit on the roll's own present target** (`Sweetener_Low_06_Quake`, provisional) | `90bc2ae7eb`, `d8ad322208`; `…035653Z` `fx` PASS | Implemented and verified | Listening choice among five installed candidates |
| S7 T6 / VIS-05 | Dust Mephit breath shows the ice cone | **Owned per-instance dust recolour bound to the executing owned Dust breath**, restored on release | `d136e7b560`, `d8ad322208`; `…035653Z` `fx` PASS | Implemented and verified (mechanics unchanged) | Visual acceptance of the dust look |
| S7 T7 / VIS-06 | Provisional `InspireCourage` cue on the Trumpet Blast | **Proposed disposition: keep `InspireCourage`** (no code change); six alternatives sampled; cue verified across an in-process save and load | `r6` PASS; `…022432Z`, `…034529Z` Trumpet reload PASS | Disposition with evidence | Listening decision (keep or choose an alternative) |

## 3. Candidates, sessions and identities

| Candidate | Package / DLL SHA-256 | Session | Result |
|---|---|---|---|
| Released v0.0.195 (baseline) | `456d504d…9444` / `3ef052ea…6a6b` | `20261123T233302Z-owner-0.33.0-s7-baseline` (r3, r6, r7 bright, r7 dim) | PASS 4/4, 7 min 47 s |
| `b438bb9d53` (round 1) | `177e6bf8…44c9` / `711f8ab7…791e` | `20261123T235421Z-owner-0.33.0-s7-cand` | FAIL, retained (round 1 record): observer admission, control GUID, Ghaele predicate. 15 min 35 s |
| `566de0d0bf` (round 1) | `179c2a44…787a` / `731d86a6…0669` | `20261124T002105Z-owner-0.33.0-s7-cand` | FAIL, retained: raw `_Dissolve` predicate; Dust check passed on a component that recoloured nothing. 19 min 33 s |
| `c0870c24de` (round 1) | `433a6bae…f05b` / `45793e82…783b` | `20261124T005329Z-owner-0.33.0-s7-cand` | ui, models, Ghaele, r6 PASS; fx FAIL (3 s wait). 18 min 46 s |
| `d136e7b560` (round 1 product) | `f7770dac…36f1` / `3b0a7c3a…250b` | `20261124T012247Z-owner-0.33.0-s7-fx` | PASS 2/2, 6 min 9 s |
| `d8ad322208` (round 2 product) | `83c7791d…fb9e` / `6104d2bf…4e26` | `20261124T022432Z-owner-0.33.0-s7-trumpet-reload` | **PASS** (Trumpet reload), 1 min 56 s |
| `d8ad322208` | same | `20261124T022637Z-owner-0.33.0-s7-cand` | FAIL, retained: ui, models bright, r6 PASS; models dim (scenery/other unit on two fixed bearings), Ghaele bright/dim (same-moment donor comparison invalid once the cast progresses naturally: loop phases differ), fx (one context counted per delivery target; reuse judged on one consumer; sample emitter). Observer causes only. 18 min 47 s |
| `f8cefba85d` | `fefea35f…c5af4` / `90c404ed…3fce` | `20261124T025726Z-owner-0.33.0-s7-cand` | **ui 15/15, mod 12/12, modd 7/7, gh 12/12, ghd 7/7, r6 17/17 PASS**; fx FAIL on one observer assumption (damage type reads `Physical`; allies roll the save, §4.6). 21 min 0 s |
| `ea45987df2` | `78fe7211…78f3` / `c39c92a4…7840` | `20261124T032725Z-owner-0.33.0-s7-fx` | FAIL, retained: every Dust check PASS; Ankylosaurus concurrent retirement FAIL (neither the retired nor the surviving cue received its native end-of-event callback within 5 s at a normal ~30 frames/s; the process then did not exit and was stopped after its launch timeout, 921 s); samples cell NRE (its summoned emitter expired). r6 PASS. 18 min 4 s. The callback loss did not reproduce (§4.5) |
| `ea45987df2` | same | `20261124T034529Z-owner-0.33.0-s7-trumpet-reload` | **PASS** (Trumpet reload), 1 min 57 s |
| **`09fbe70ce5`** (round 2 final) | `f4e481ba…1e9e` / `cc00fcf7…1f57` | `20261124T035653Z-owner-0.33.0-s7-fx` (fx, r6) | **PASS 2/2** (fx 25/25 rows, r6 17/17), 6 min 25 s |

Product code (everything outside `RuntimeTesting/`) is identical across `d8ad322208`, `f8cefba85d`, `ea45987df2` and
`09fbe70ce5`; those candidates differ only in the review observer. UI, model and Ghaele evidence is bound to
`f8cefba85d`, effects and R6 to `09fbe70ce5`, the Trumpet reload to `d8ad322208` and `ea45987df2`. Every session
completed its transaction, applied and restored game settings and verified owner files; the human installation was never
touched; game-copy windows were coordinated with the peer session sharing this machine. Builds: `Build-Local
-DevelopmentPurpose s7-visual-audio -DevelopmentTestFilter sprint7-visual` with the pinned reference bundle, 7 min 35 s to
8 min 11 s each, strict package validation passed, no deployment.

Expenditure (round 2): four builds (about 31 min) and seven loader-compat sessions (about 70 min of game-copy time:
1 min 56 s, 18 min 47 s, 21 min 0 s, 18 min 4 s, 1 min 57 s, 6 min 25 s, plus the harness build), all in one
coordinated window of 02:24–04:03 UTC. Round 1: five builds and five sessions (§3 rows above, 68 min of sessions).

Reviewable evidence (committed, sanitized): `docs/evidence/sprint7/r2/` holds one extract per launch of every round-2
session and the baseline (status, every check with its observed fields, the phase record, harness results and events;
machine-local paths reduced to file names; long arrays capped; superseded or failed sessions as check rows only) with
`MANIFEST.json` (SHA-256 of each raw source). `docs/evidence/sprint7/REVIEW-INDEX.md`/`.json` list the curated review
set (103 files with SHA-256). The images and audio themselves are renders and recordings of the proprietary game and stay
in the lab at `<lab>/runtime-evidence/s7/review-final/` (`INDEX.md` sha256 `81a14365…87eb`); the earlier sets
`review/` (round 1, `4409d72f…0ab6`) and `review-r2/` (an intermediate pick, superseded) are retained.

Listening: **no audio judgment in this record came from listening.** Cue evidence is native (playing IDs, posting
frames, end-of-event callbacks, stop requests) plus loopback level measurements of the whole device mix; suitability is
the owner's to judge from the excerpts.

## 4. Per story

### 4.1 T1 / REP-04 Ghaele

Observation (`f8cefba85d`, bright and dim): a Ghaele cast through its real published summon as a native
`UnitUseAbility` that progresses through the caster's own command queue (no forced start or tick; caster level 20 is
the only override), a Bralani sibling cast the same way, the native donor `AzataGhaelSummoned` spawned without any
summon, and a second Ghaele cast 36 s into the first one's life. Every cast finished `Success` with one creature.
- Each `SummonedUnitBuff` lasts 119.7 game seconds after appearance (120 s from the cast); each summon is removed
  121.5–121.9 s after it appeared (the native expiry fade).
- The native `StandardMaterialController` on the Ghaele runs one looping animation (delay 4.75 s, lifetime 5 s,
  `LoopAnimation` true, `DissolveCutout` false) driving three materials (body, sword, scabbard); the donor runs the same
  signature on its own three materials. In every in-life snapshot (10 bright, 11 dim) every animation on the Ghaele had
  a signature the donor also runs, none was a cutout dissolve, and each driven body material's `_Dissolve` equalled the
  animation's own current value (`s7-ghaele-*-dissolve-attribution`). Each unit's loop starts at its own spawn, so
  same-moment values differ in phase; the round-1 same-moment match (163/172) depended on forced near-simultaneous
  spawns and is superseded. The appearance cutout animations (0 → 1.8 s, then 1.8 s + 1.0 s) exist only before
  appearance completion.
- Surface-removing (cutout) dissolve on the body stays 0.00 from appearance completion to expiry for both Ghaeles, the
  sibling and the donor. Review frames at raw `_Dissolve` 1.00 and 0.00/0.02 show the same intact body.
- Ordinary move 3.91 m (bright); the dim interior move travelled 17.5 m to the nearest navigable node of its target.
  Ordinary attack (bright): three native weapon-attack rules by the Ghaele (`LongswordGhaeleAzata`, all hits) on the
  fixture hostile are retained in the record. The interior fixture has no hostile.

Disposition: the October reader sampled `_Dissolve` on the first skinned renderer, which is the body material this
native loop drives; its values (0.009, 0.04, 0.118, 0.156) are consistent with sampling that loop at arbitrary times and
did not measure a disappearance. The "move did not start" came from the old review path, which did not unpause or tick
movement (source inspection). No product change is warranted. Limits: the October run's machine-local evidence was not
re-read, so the attribution of that historical run rests on the identical reader, material and writer, not on its own
frames; the loop's authoring source is the native prefab controller, present without any KMG summon.

### 4.2 T2 / VIS-01 Nalfeshnee

See `assets-source/original-models/sprint39-nalfeshnee/SOURCE.md` (Sprint 7 section) for cause, repair, measurements
and hashes (`6ff85493…` mesh, `1e9f5303…` albedo). Native (`f8cefba85d`): the pinned R7 export `d34e6c19…` is shown on
the same live unit after the arrival and appearance settle. The fixture helper removes the arrival and appearance buffs
before the captures (disclosed in the phase limits); every body material reads `_Dissolve` 0. Front, rear, both
wing-root details and the rear wing roots, idle, a 3 m move, a turn of more than 45° and an attack (bright), after and
before. Every detail requires its named bone on the live body (no fallback to the unit origin), and every view records
the colliders between subject and camera: details clear of scenery and other units, whole-body views clear of scenery
by a ray from the bounds centre; each view keeps its nominal bearing or the nearest yaw offset up to 60° that is clear
(recorded). Dim: the rear whole-body view moved +45°; its centre ray is clear, but a gallery beam hides the lower body in
that frame (the centre-ray measure does not catch partial occlusion); the wing-root details, including the rear wing
roots, are unobstructed. Wings follow the chest; no scale, reach, collision or gameplay change.

### 4.3 T3 / VIS-02 Salamander

See `assets-source/original-models/sprint17-serpents/SOURCE.md` (Sprint 7 section): exact private capture
`eb0d5eaa…` recovered at its named lab location and reproduced byte for byte; export `0837e796…`. Native
(`f8cefba85d`): the released `b5524a69…` shown on the same live unit; waist front and rear, both hands with the spear
grip, idle, move, turn and spear attack (bright), after and before, with the same bone, occlusion and appearance rules as
§4.2. Dim: the right-hand detail moved −45° to clear a background unit. The material initialization fix, spear attacks,
owned tail slap and traveling tail gait are unchanged.

### 4.4 T4 / VIS-03 Trample/Overrun windows

Released baseline (`…233302Z` r3): both owned windows read "Target point within 0 ft. feet." and the header shows
"None". Candidate (`f8cefba85d` ui, PASS 15/15): the Brachiosaurus Overrun and Trample and the Mastodon Trample, each
through the description window (Ability fact) and the HUD path (AbilityData), read "Target point within twice this
creature's speed." with no 0 ft text and no "None". Controls: the native overrun (`1a3b471e…`) keeps its native target
text and native Magic Missile keeps "Evocation". The observer now opens only when no description window, hover or
unrelated modal is showing, owns a window only while it shows the exact `TooltipData` it built, removes only control
facts its own grant created (2 here), disposes its capture session and stops the phase on any ownership loss
(`sprint7-visual.observer-ownership` covers the refusals).

Controller mode — exact prerequisite blocker: the console tooltip (`TooltipSectionView`) renders through the same
`DescriptionBuilder.Build` → `AbilityHeader`/`GetTarget` the patch targets (decompile), but the only console route to a
summoned creature's abilities is the console Inspect view (`InGameUiStaticPartContext` → `InspectVm`), which requires
gamepad controller mode and a selected unit. No admitted Sprint 7 or earlier review observer switches
`Game.ControllerMode` or drives console input; summons are not directly controllable. Rendering it would need a new
input-bearing observer admission, which this mission does not authorize. Not rendered.

### 4.5 T5 / VIS-04 Ankylosaurus impact cue

Product change in round 2: the cue also requires the struck creature to be the attack roll's own target and still in the
game (a killing hit still qualifies; death resolves later) — `QualifiesForCue` now has six inputs, all 64 combinations
tested. Final candidate (`09fbe70ce5`, `…035653Z` fx): two resolved tail hits (`KMG_Summoning_Natural_Tail3d6`,
`15394605…`) each paired with exactly one `Sweetener_Low_06_Quake` posted in the hit's own frame (playing IDs 125 and
190, natural ends 2.15 s and 1.56 s); the attack rows carry owner, target, weapon GUID and roll identity. Production-path
replay: the qualifying roll passed again to the owner's own `AnkylosaurusTailControl` posted nothing; another
Ankylosaurus's native tail hit passed to it posted nothing; a later real hit posted its own cue. A foreign weapon and an
invalid target cannot be produced natively here (the Ankylosaurus has only its tail) and are covered by the
qualification test. Miss (armor raised) and pre-impact cancel: no cue. Concurrent retirement, judged by the R6 rule with
both cues live: retiring one Ankylosaurus requested a stop for its own playing ID 447 0.012 s after retirement with the
native end 0.049 s after; the other's cue 445 played on and ended naturally after 2.17 s. Retained failure
(`ea45987df2`): in one run neither cue of that step received an end-of-event callback within 5 s and the game process
then did not exit; the same step passed before and after it; cause unknown (no frame stall). Loopback levels (whole
device mix, round 1): strikes peak −1.6 / RMS −23.6 dBFS against a −42.4 dBFS floor. Recommendation: keep
`Sweetener_Low_06_Quake`; four alternatives are in the review set.

### 4.6 T6 / VIS-05 Dust Mephit breath

Product change in round 2: the recolour binds to the executing owned Dust breath, not merely the Dust unit. A
frame-stamped scope on the native `AbilityExecutionProcess.Tick` (nest-safe) names the ability whose delivery launches
the projectile; `CreateView` and the cast-effect spawn run synchronously inside it. A bounded association
(`DustCastFxAssociation`: one frame, adopted at most once, reset by the next creation) carries that projectile to the
cast effect spawned from its own prefab in that creation. The round-1 plan to supply a missing effect was dropped: the
native controller clears its previous launch point at the end of every `ProjectileController.Tick`, so suppression never
crosses executions (decompile), and every observed creation found it already cleared.

Final candidate (`09fbe70ce5`): three consecutive breaths from one unchanged unit position (launch points vary by
millimetres with the breath animation's source bone). Each was one execution context and one creation bound to the Dust
breath, with exactly one `ColdCone15Feet00_Aoe` cast effect: the same pooled instance −235974 each time, oriented with
the projectile (0.0°), lifetime 11 s, recoloured (11 materials, 16 particle colours). On release each instance read back
exactly its pre-tint signature: shared material instances and every particle start and over-lifetime colour, hashed.
The first cast was framed on an unobstructed bearing with the targets in view.

Mechanics, correlated to each execution's own context, with an ally inside the cone and a second enemy outside it:
- Natural 1 on the enemy's Reflex save (DC 13): full damage, 4 (1d4, physical, slashing form) and Sickened for 3
  rounds (17.2 s left at read).
- Natural 20: half damage (1, `HalfBecauseSavingThrow`) and no condition.
- The ally inside the cone and another summoned ally were selected and rolled Reflex saves (one failed) but took
  nothing and gained no condition. The enemy outside and the caster behind were never selected.
- Action type Standard, no resource; the breath stays available (the 4-round cooldown applies to AI casting).
- Released behaviour, not changed here: the save is rolled for every selected creature, so allies appear in the
  combat log with a save that has no effect.

Damage dice follow the seeded d20 state (disclosed).

Controls:
- The Dust Mephit executing the Ice breath (same launcher, same cold cone) got the untinted native cone.
- Two later non-Dust consumers (that Ice breath and the Ice Mephit's next breath) claimed the very instance −235974 the
  Dust breaths had used. Before any product code it read the native signature recorded before the first tint, and it
  stayed untouched.
- With both cones live for 379 frames, the Ice Mephit's separate instance kept its native signature while the Dust
  instance was recoloured and restored.
- A pre-execution cancel created nothing and dealt nothing.
- A Dust Mephit removed mid-effect left its instance reading native.

Observer rollback: the trace and the body swap now unhook or restore what they acquired if construction fails; the
trace held 4 owned hooks before disposal and 0 after in the final run.

Limits: the recolour is a luminance-preserving hue change of the native effect (subtractive layers take the
complementary tone); readability is the owner's. The interrupted-creation case (a `CreateView` that throws) is covered
by the association's domain test, not induced natively.

### 4.7 T7 / VIS-06 Trumpet Archon

Recorded on the released baseline and re-run on the final candidate (`r6` PASS both): one `InspireCourage` cue for a
blast reaching 7 creatures, natural completion about 3.3 s, none on a cancelled cast, early retirement stopped by
playing ID; the Vrock neighbour's cue completed naturally. Across an in-process reload (harness `kmgTrumpetCue` around
the admitted `saveGame`/`loadLastSave` in the disposable fixture save, `d8ad322208` and `ea45987df2`, PASS both): the
first blast's cue (ID 68) ended naturally after 3.90 s; the second blast's cue (ID 159) was playing when the save began
(02:26:02.68–04.75); the load began at 04.78; cue 159 reached its native end at 06.13 during the load, before any
retirement stop (so this reload ended it naturally; the retirement path is the R6 evidence); after the load nothing was
posted again through a 5 s quiet interval, and the loaded Trumpet's fresh blast posted exactly one new cue (ID 326) that
ended naturally after 3.90 s. Every non-Trumpet save was seeded to a natural 20 so the blast paralyzed nobody (disclosed).
Recommendation: **keep `InspireCourage`**, with `InspireGreatness` as the alternative; this is a proposed disposition.

## 5. Validation

Focused domain tests (`sprint7-visual.*`, 10 cases): VIS-01 and VIS-02 decoded-export regressions (each fails on its
baseline export), VIS-03 identities and text, VIS-04 qualification (64 combinations), VIS-05 identity (owned breath
execution only) and tint, the cast-effect association bounds, the six-phase admission boundaries (fresh, save-free,
exact scenario and name, one parameter, no input, no Wall, not host-restricted, outdoor/interior split, screen capture
only for the UI phase) and observer ownership. Neighbour selections: `summon-playtest-r3`, `-r4`, `-r6`, `-r7`,
`sprint22-`, `sprint5-`, `salamander`, `native-admission`, `loader-compat-probe`. The required controller gate receipts
are recorded by the controller on the reviewed candidate.

## 6. Owner decisions pending

1. T1: accept the REP-04 attribution and the no-change disposition.
2. T2, T3: visual acceptance of the repaired Nalfeshnee wing roots and Salamander waist and fingers.
3. T4: the target wording "within twice this creature's speed"; whether a controller-mode render (new input-bearing
   observer admission) is wanted.
4. T5: listening choice of the impact cue (recommended `Sweetener_Low_06_Quake`).
5. T6: visual acceptance of the dust recolour; whether allies rolling a no-effect save against mephit breaths (released
   behaviour) should be raised as a separate issue.
6. T7: keep `InspireCourage` (recommended) or choose an alternative.
7. Merge and release remain the owner's; this sprint authorizes neither.

## 7. Review round 1 findings and round 2 corrections

| Finding | Correction | Where |
|---|---|---|
| R1-F1 evidence reviewable | Sanitized per-launch extracts with source hashes, review index with hashes, identities, durations, expenditure, restoration, explicit "no listening" statement | §3, `docs/evidence/sprint7/` |
| R1-F2 REP-04 on a normal cast with a witnessed attack | Natural command-queue cast; native weapon-attack rules retained; phase-independent controller attribution; historical reader related | §4.1 |
| R1-F3 capture labels | Named bone required; occluders measured and recorded per view with bearing search; appearance-buff removal disclosed | §4.2, §4.3 |
| R1-F4 UI restoration | Exact `TooltipData` ownership, only self-added facts removed, capture session disposed, any foreign/lost ownership stops the phase; refusal tests | §4.4 |
| R1-F5 controller mode | Exact prerequisite blocker recorded | §4.4 |
| R1-F6 struck target and dedup | Roll-target and validity in the product predicate; production-path replay with same-roll and foreign-owner controls; full correlation | §4.5 |
| R1-F7 live overlap | Retirement only with both cues live; the R6 rule applied | §4.5 |
| R1-F8 repeated breaths | Same-position consecutive breaths; engine shows no cross-execution suppression; no supply needed | §4.6 |
| R1-F9 execution binding | Execution-scoped identity and a bounded association; foreign-ability and concurrent controls; association tests | §4.6 |
| R1-F10 Dust mechanics | Execution-correlated saves, dice, damage, condition, selection, action; ally inside / enemy outside; unobstructed framing; released ally-save behaviour recorded | §4.6 |
| R1-F11 reuse | Same instance ID reused by non-Dust consumers; hashed pre-tint and restored signatures | §4.6 |
| R1-F12 Trumpet across reload | Admitted same-process reload in the disposable fixture save; no replay; fresh blast | §4.7 |
| R1-F13 admission | Six-phase boundary test | §5 |
| R1-F14 observer rollback | Trace and body-swap constructors roll back; owned hook counts recorded | §4.6 |
