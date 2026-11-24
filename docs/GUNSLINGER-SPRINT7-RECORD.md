# Sprint 7 record: presentation and audio remediation (S7 T1–T7)

Status: **development candidate for review (2026-11-24), review round 3; nothing is merged or released.**
- Draft PR #219; one agent-review enrollment (`kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`, foreground,
  single plan); branch `codex/s7-visual-audio-remediation`.
- Mission: `docs/missions/sprint7/SPRINT7-AUTONOMOUS-MISSION.md` (verbatim, sha256 `c232f135…f56f0`).
- Technical status, agent recommendations and owner acceptance are kept separate below. Every visual and listening
  judgment remains the owner's.
- Review history: round 1 (head `0910995241`) returned 14 findings; round 2 (head `0d976f4fd1`) verified four of them
  and returned 10 open plus 4 new. §7 maps each to its correction.

## 1. Starting point

- Sprint 6 was merged (PRs #198–#216) and released: stable **v0.0.195**, release commit `ef0e1fe15d` (PR #217), records
  PR #218. Post-release `origin/master` is **`35763430ac`**, the base of this branch.
- Released package `456d504d…9444`, DLL `3ef052ea…6a6b`.
- Environment: disposable loader-compat game copy, Unity Mod Manager owner profile 0.33.0, no other mods.
- Harness `KmgLoaderCompatHarness.dll` from this branch: `04c9d1d4…` (round 1), `9e90d22c…` (round 2), and
  `3d86bd3a6b61209f5bbc900c67a48877afaabf3dbbc10310aecafd79612988e8` (round 3), which adds the `kmgTrumpetCue` and
  `kmgGhaeleLifetime` probes.
- Blender 4.5.10 `8b15ef73…313c` with `PYTHONHASHSEED=0`.
- Carried forward unchanged and outside this sprint: Sprint 6 accepted scopes, REP-05/REP-06 BLOCKED, VER-02 open with
  G5 fixture-blocked, and the other limits in `docs/GUNSLINGER-SPRINT6-DISPOSITION.md`.

## 2. Outcome

| Story / record | Original problem | Result | Evidence (round 3) | Technical status | Remaining owner decision or precise blocker |
|---|---|---|---|---|---|
| S7 T1 / REP-04 | October review: "increasing" Ghaele dissolve; a scripted move that did not start | **Current behaviour attributed; no product change.** On the released v0.0.195 package and on the candidate, an ordinarily summoned Ghaele keeps its body (cutout dissolve 0.00) until its native 120 s expiry. Its raw `_Dissolve` is the native looping non-cutout animation that the unsummoned donor also runs. Ordinary moves and attacks work. | Harness `kmgGhaeleLifetime` on v0.0.195 (`…052619Z`) and the candidate (`…052954Z`); `…050957Z` `gh`/`ghd` | Disposition for the current and released builds | **Open historical scope:** the October run's writer and frames are unavailable, so the October observation is not attributed. Owner: accept the current-build disposition. |
| S7 T2 / VIS-01 | 48 zero-area wing-root triangles (and inside-out welded membranes) | **Repaired at the generator.** 0 zero-area triangles, 0 repeated edges, all parts outward | `ed88ad994e`; export receipt; `…050957Z` `mod`/`modd` | Implemented and verified | Visual acceptance of the wing roots |
| S7 T3 / VIS-02 | 19/12,924 normal corners at a waist pleat and the left-hand fingers | **Repaired from the exact private input.** 16 → 0 opposing corners | `d864e7b01f`; export receipt; `…050957Z` `mod`/`modd` | Implemented and verified | Visual acceptance of waist and fingers |
| S7 T4 / VIS-03 | Owned Trample/Overrun windows: "Target point within 0 ft. feet." and school "None" | **Scoped presentation override.** "Target point within twice this creature's speed."; no school | `2614ea3bd6`; `…050957Z` `ui` | Implemented and verified for the desktop fact and HUD paths | **Controller-mode render blocked** (§4.4); wording acceptance |
| S7 T5 / VIS-04 | No added impact sound on the Ankylosaurus tail strike | **One owned cue per resolved tail roll on the roll's own in-game target** (`Sweetener_Low_06_Quake`, provisional) | `90bc2ae7eb`, `6f2f16ca88`; `…061437Z` `fx` | Implemented and verified | Listening choice |
| S7 T6 / VIS-05 | The Dust Mephit breath shows the ice cone | **Owned per-instance dust recolour** of the cone created by the executing owned Dust breath, restored on release | `d136e7b560`, `d8ad322208`, `6f2f16ca88`; `…061437Z` `fx` | Implemented and verified (mechanics unchanged) | Visual acceptance; blindness vs Sickened design question |
| S7 T7 / VIS-06 | Provisional `InspireCourage` cue on the Trumpet Blast | **Proposed disposition: keep `InspireCourage`** (no code change); six alternatives sampled; cue verified across an in-process save and load | `r6` PASS; Trumpet reload `…052423Z` | Disposition with evidence | Listening decision |

## 3. Candidates, sessions and identities

Round 3 (this candidate):

| Candidate | Package / DLL SHA-256 | Session | Result |
|---|---|---|---|
| `6f2f16ca88` | `8039a627fa835ee3c5768756210cb93d92f32be3b0ee4ba3b35947ec1bde55f0` / `d7088878c3deda8f9d3d37f5cf2bb97be79c48b4bea66c4aa66f0cd9e72d5976` | `20261124T050957Z-owner-0.33.0-s7-core` (ui, mod, modd, gh, ghd) | **PASS 5/5**, 14 min 26 s |
| `6f2f16ca88` | same | `20261124T052423Z-owner-0.33.0-s7-trumpet-reload` | **PASS**, 1 min 57 s |
| released v0.0.195 | `456d504d…9444` / `3ef052ea…6a6b` | `20261124T052619Z-owner-0.33.0-s7-ghaele` (harness timeline) | **PASS**, 3 min 34 s |
| `6f2f16ca88` | as above | `20261124T052954Z-owner-0.33.0-s7-ghaele` | **PASS**, 3 min 36 s |
| `6f2f16ca88` | as above | `20261124T050257Z-owner-0.33.0-s7-fx` | FAIL, retained (observer causes): crafted control roll built without its wrapper (native NRE); the donor ended before the Dust release was sampled. r6 PASS. 6 min 59 s |
| `12ba761474`/`dc46a92fbc` | `6a29ecc1…fb13` / `6e2f39b4…7b44` | `20261124T054209Z-owner-0.33.0-s7-fx` | FAIL, retained (observer): the "foreign" weapon was the Ankylosaurus's absent empty hand; the concurrent loop exited at the release frame. 7 min 18 s |
| `8ed5467fce` | `c0118b26…00eb` / `7cc9df4b…cd4c` | `20261124T055920Z-owner-0.33.0-s7-fx` | FAIL, retained (observer): every value was as expected; the error field read a null string as non-null. 7 min 7 s |
| **`f564cbb844`** (final) | `d4ded9a868753cd0766f7ebf8a0f710e21786740a5808785277c558202d46619` / `192c972645c5145f426cf2352b49f012696c31307289a5eb6aa08d3812c86068` | `20261124T061437Z-owner-0.33.0-s7-fx` (fx, r6) with loopback audio | **PASS 2/2** (fx 27/27 rows, r6 PASS), 7 min 0 s |

Identity binding:
- Product code outside `RuntimeTesting/` is identical from `6f2f16ca88` to `f564cbb844`, and the harness is unchanged.
  The UI, model, Ghaele, Trumpet and baseline evidence binds to `6f2f16ca88`; effects and R6 bind to `f564cbb844`.
- Rounds 1–2 sessions and failures are listed in `docs/evidence/sprint7/r2/MANIFEST.json`. Round 2's final product was
  `d8ad322208`; its product code equals `6f2f16ca88` except the two round-3 product changes (§4.5, §4.6).
- Every session completed its transaction, applied and restored game settings, and verified owner saves and
  installation. The outer receipts are in the `*-session.json` extracts.
- The human installation was never touched. Each game-copy window was agreed with the peer session sharing this machine.

Builds: `Build-Local -DevelopmentPurpose s7-visual-audio -DevelopmentTestFilter sprint7-visual` with the pinned
reference bundle. Each took 7–8 min, passed strict package validation, and deployed nothing.

Expenditure:

| Round | Builds | Sessions | Game-copy time |
|---|---|---|---|
| 1 | 5 | 5 | 68 min |
| 2 | 4 | 6 | 68 min 9 s |
| 3 | 4, plus 1 refused at the clean-commit check | 8 | 51 min 57 s |

Round 3 also ran 6 Blender export runs (about 1 min in total).

Reviewable evidence (committed):
- `evidence/sprint7-review/`: 82 curated stills with `INDEX.json`, which gives each lab original's SHA-256, the committed
  file's SHA-256 and its source session. PNGs are re-encoded as JPEG quality 92.
- `docs/evidence/sprint7/r3/`: sanitized extracts of every round-3 launch (every check with its observed fields, the
  phase records, harness results and events) and of each session's outer receipt (transaction, lease, registry
  readback, owner saves and installation, package/DLL/harness hashes). `MANIFEST.json` gives each raw source's SHA-256.
- `docs/evidence/sprint7/exports.json`: generator, input and output identities. The base generators reproduce the
  shipped baselines byte for byte, and two runs of each repaired generator are byte-identical.
- `docs/evidence/sprint7/audio-context.json`: per excerpt, the session, UTC interval, emitter, listener distance, game
  volumes and levels.

Audio and raw records stay in the lab: game audio is not committed, and the excerpts are at
`<lab>/runtime-evidence/s7/review-r3/`. **No judgment in this record came from listening.**

## 4. Per story

### 4.1 T1 / REP-04 Ghaele

Released baseline and candidate, observed by the harness probe:
- **Cast.** The probe casts the published Ghaele summon as a native `UnitUseAbility` from the main character in the
  fixture save, with caster level 20 as the only override. The ability is granted by instance and released afterwards.
- **Appearance.** It ends at 1.7 s and is kept, not removed.
- **Body in life.** The enabled body renderer's `_DissolveCutout` stays 0.0, while its raw `_Dissolve` sweeps up to 1.0.
- **Attribution.** In every in-life snapshot (11 of 11, both packages) the Ghaele's dissolve animations have signatures
  the unsummoned native donor also runs: delay 4.75 s, lifetime 5 s, looping, non-cutout. Each driven material's
  `_Dissolve` equals the animation's own current value.
- **Movement and expiry.** The ordinary move travels 3.6/3.7 m. The native lifetime ends at 119.7 s and the unit is
  removed at 121.5/121.6 s.
- **No attack here:** the fixture save has no hostile.

Candidate, product observer (`…050957Z`, bright and dim):
- **Casts.** Ghaele, Bralani sibling and a second Ghaele are cast through the caster's command queue. Each summon ability
  was granted by instance and released, with its overrides restored.
- **Attribution.** 9 of 9 (bright) and 10 of 10 (dim) in-life snapshots are consistent with the donor's loop.
- **Body, movement and combat.** Body cutout 0.00 to expiry. Ordinary move 3.93 m (bright) and 3.98 m (dim). Three
  native `LongswordGhaeleAzata` weapon-attack rules on the hostile (bright).
- **Expiry.** Each summon is removed 121.6–122.0 s after it appeared.

Disposition and open scope: no product material, lifetime or despawn change is warranted for the released or current
build. The October incident is **not attributed**: its run's writer, build and frames were not re-read, and no
observation from that time ties its reading to this loop. That historical scope stays open in the outcome table and the
remediation state.

### 4.2 T2 / VIS-01 Nalfeshnee

See `assets-source/original-models/sprint39-nalfeshnee/SOURCE.md` and `docs/evidence/sprint7/exports.json` (base
generator → R7 `d34e6c19…`; repaired → mesh `6ff85493…`, albedo `1e9f5303…`, twice each).

Native (`…050957Z`):
- **Cast and appearance.** The Nalfeshnee is cast through the published spell and its native appearance completes on its
  own after 1.75 s, with no buff removed. It is then moved to its mark.
- **Bound to the body.** Every view's readback names the enabled body renderer `KMG_nalfeshnee_Original` (11,593 vertices
  after; 12,402 for the pinned R7 export before) with `_Dissolve` 0.
- **Views.** Front, rear, both wing-root details and the rear wing roots are shown after and before. Each detail
  requires its named bone; each view records its occluders and keeps its nominal bearing or the nearest clear yaw.
  Bright: all nominal. Dim: the rear whole-body view moved +45°.
- **Motion.** Idle, move, a turn over 45° and an attack (bright), after and before.

Limit: the occluder measure casts a single ray from the bounds centre. In the dim rear whole-body still, a gallery beam
can hide part of the lower body. The wing-root details are unobstructed.

### 4.3 T3 / VIS-02 Salamander

See `assets-source/original-models/sprint17-serpents/SOURCE.md` and the export receipt (exact private capture
`eb0d5eaa…`; base generator → released `b5524a69…`; repaired → `0837e796…`, twice).

Native: the same natural appearance (1.75 s) and the same body-bound readbacks (`KMG_SalamanderHuman_…_Mesh`, 2,198
vertices). Waist front and rear, both hands with the spear grip, after and before, all on nominal bearings except the
dim right-hand detail (−45°).

### 4.4 T4 / VIS-03 Trample/Overrun windows

Released baseline (`…233302Z` r3): both owned windows read "Target point within 0 ft. feet." with the header "None".

Candidate (`…050957Z` ui, 14/14):
- The Brachiosaurus Overrun and Trample and the Mastodon Trample, through the description window and the HUD path, read
  "Target point within twice this creature's speed.", with no "0 ft" and no "None".
- Controls: the native overrun keeps its native target text; native Magic Missile keeps "Evocation".
- Ownership: an existing control fact is used as it is. Only a fact the observer granted (2 here) is released, by
  instance, while the actor still holds that instance. Window ownership includes the fact instance.

Controller mode — exact prerequisite blocker, traced against the existing console code:
- The console tooltip renders through `DescriptionBuilder.Build` (`TooltipSectionView`), which reaches the patched
  `AbilityHeader`/`GetTarget`.
- Console Inspect exists only inside the console in-game UI scene `UI_Ingame_Scene_Console`
  (`RootUiContext.InitializeUiScene` → `InGameUiContext` → `InGameUiStaticPartContext.OnActivateInspect`, using the
  action bar's `InspectVm` unit).
- The two admitted console routes do not load that scene. The teleportation gamepad fixture
  (`DisposableTeleportationGamepad*` only) swaps `UI_LoadingScreen_Scene_Console` for the global-map teleport dialog.
  The firearm controller scope builds a bare `UnitSelectionManager` and `InGameInputLayer` with no UI scene.
- The Sprint 32 review scenario admits no controller switch.
- Rendering the console Inspect window would require a new admission: loading `UI_Ingame_Scene_Console` and driving
  console input in a review fixture. This mission does not authorize it.

Controller verification therefore stays open.

### 4.5 T5 / VIS-04 Ankylosaurus impact cue

Round-3 product change: the cue has its own once-per-roll claim. It is taken only after the exact owner, tail weapon,
resolved hit, in-game victim (`!Destroyed && IsInGame`; a killing hit still qualifies) and the roll's own target all
hold. A stale, removed or mismatched wrapper therefore never consumes the roll's cue. The strike-rider stamp is
unchanged.

Native (`…061437Z`):
- **Hits.** Two resolved tail hits each produced exactly one cue in the hit's own frame (IDs 149 and 227), ending
  naturally after 2.15 s and 2.03 s. The camera listener was 37.5 m away.
- **Production-path controls.** Fresh resolved rolls with crafted wrappers went through the owner's own handler:
  - valid tail roll: 1 cue;
  - the caster's `WeaponEmptyHand` (foreign weapon): 0;
  - a wrapper naming another creature than its roll's target: 0, and the same roll's valid wrapper then posted its 1;
  - an out-of-game target: 0.
- **Replays.** The same native qualifying roll replayed: 0. Another owner's native hit: 0. A later real hit: 1.
- **Miss and cancel:** 0 each.
- **Retirement with both cues live.** The retired ID 468 had its stop requested 0.010 s after retirement and its native
  end 0.020 s after. The survivor 466 ended naturally after 1.60 s.

Retained: one round-2 run lost both end-of-event callbacks and its process did not exit (§3 of the round-2 extracts).
That did not recur in five later runs; its cause is unknown.

Audio context: `docs/evidence/sprint7/audio-context.json`. In play the strikes peak −1.2/−3.0 dBFS against a −13.0 dBFS
baseline interval.

### 4.6 T6 / VIS-05 Dust Mephit breath

Round-3 product change: creation ownership is scoped to the native execution tick.
- Every `CreateView` inside an `AbilityExecutionProcess.Tick` is tracked, owned or not, so a nested foreign creation
  shadows an enclosing owned one.
- A cast-effect spawn adopts only the innermost creation of the innermost scope of its frame, at most once, and only at
  that creation's own launch point.
- An interrupted creation is discarded by its enclosing creation's end or by the tick's end. The native tick catches
  delivery exceptions, so its postfix always runs, and nothing outside a scope can adopt.
- Domain tests cover the nested owned/foreign, interrupted, out-of-scope, other-frame and stale-scope cases.

Native (`f564cbb844`):

**Casts**
- Three consecutive breaths ran from one unchanged position. Each was one execution with one creation bound to the
  Dust breath and one cast effect: the same pooled instance −235646, oriented 0.0°, recoloured 11 materials and 16
  colours, and restored to its exact pre-tint signature on release.
- The first was framed on an unobstructed bearing.

**Mechanics, by each breath's own context**

| Natural d20 (enemy's Reflex) | Damage to the enemy inside | Condition |
|---|---|---|
| 1 | 4 (1d4 slashing, full) | Sickened |
| 20 | 1 (half) | none |
| 1 | 4 (full) | Sickened |

- The ally inside the cone rolled saves (one failed) but took nothing and gained nothing. The enemy outside and the
  caster behind were never selected.
- A Sickened condition from another context on the ally survived every reset and the cleanup.

**Controls**
- The Dust Mephit's own Ice breath and the Ice Mephit's next breath both reused instance −235646 untouched, reading the
  native signature.
- The Ice donor, started after the Dust tint, held its native signature at all 26 samples over 265 overlapping frames,
  including the frame the Dust instance was restored.
- Restoring one of two live Dust instances returned it to native while the other kept its tint until its own release.
- The native-look comparison restored only its own instance (−235646).
- A pre-execution cancel created nothing.
- A Dust Mephit removed mid-effect left its instance reading native.

**Observer rollback**
- In the bright models phase, a body swap failing after allocating its comparison mesh and putting it on the renderer
  surfaced the original failure with "rollback complete": the renderer was back on its own mesh and the comparison mesh
  was destroyed.
- A trace failing after its first patch (2 hooks) left 0 hooks.
- A normal trace installed 4 hooks and removed all 4.

Design questions (no change made):
- Mission wording says "blindness"; the released breath sickens.
- Mephit breaths roll a no-effect Reflex save for allies, which shows in the combat log.

### 4.7 T7 / VIS-06 Trumpet Archon

R6 (`f564cbb844`): one `InspireCourage` cue per blast, natural completion, none on a cancelled cast, and early
retirement stopped by playing ID. The Vrock neighbour completed naturally.

Reload (`…052423Z`):
- The saved cue (ID 159) ended natively and naturally (4.10 s), judged by the product's recorded stop-result contract.
- Nothing was posted after the load.
- The fresh blast posted one new cue (ID 327, 3.90 s).
- Seeding was scoped to each observed blast and the shared generator restored; pause was restored.
- The probe disposes its iterator and releases every held change on any exit. Domain tests cover abandonment while
  yielded and the natural, retired, failed-stop and missing-callback verdicts.

Recommendation: **keep `InspireCourage`**, with `InspireGreatness` as the alternative. Samples are in the review set
and audio context.

## 5. Validation

Focused domain tests (`sprint7-visual.*`, 12 cases):
- VIS-01/VIS-02 decoded-export regressions;
- VIS-03 identities and text;
- VIS-04 qualification with the production gate;
- VIS-05 identity, tint and creation ownership;
- Sprint 7 admission and the screen-capture decision `Sprint32ScreenCaptureAdmitted`, which
  `NativeIconScreenEvidence.Supports` consumes;
- observer ownership and rollback reporting;
- the Trumpet reload verdict and probe abandonment.

Neighbour selections: `summon-playtest-r3`, `-r4`, `-r6`, `-r7`, `sprint22-`, `sprint5-`, `salamander`,
`native-admission`, `native-profile`, `loader-compat-probe`. The controller records the required gate receipts on the
reviewed candidate.

## 6. Owner decisions pending and open scopes

1. T1: accept the current-build disposition. The October incident remains unattributed (historical scope open).
2. T2, T3: visual acceptance of the Nalfeshnee wing roots and the Salamander waist and fingers.
3. T4: the wording. Controller-mode verification is open and needs a new console in-game UI admission (§4.4).
4. T5: listening choice of the impact cue (recommended `Sweetener_Low_06_Quake`).
5. T6: visual acceptance. Design questions: should the breath blind (mission wording) or sicken (released)? Should
   allies' no-effect saves be raised separately?
6. T7: keep `InspireCourage` (recommended) or choose an alternative.
7. Merge and release remain the owner's; this sprint authorizes neither.

## 7. Review findings and corrections

| Finding | Round-3 correction | Where |
|---|---|---|
| R1-F1 evidence | Committed curated stills with original/committed hashes; session outer receipts; export receipt with baseline reproduction and repeat runs; audio context per excerpt; reconciled expenditure | §3, `evidence/sprint7-review/`, `docs/evidence/sprint7/` |
| R1-F2 REP-04 | Released-baseline timeline by the harness; historical attribution kept open | §4.1 |
| R1-F3 models | Natural appearance (no buff removed); body-renderer-bound readbacks | §4.2, §4.3 |
| R1-F4 UI facts | Existing controls used as is; grants released by instance only while held | §4.4 |
| R1-F5 controller | Existing console routes traced; exact unsupported prerequisite recorded; kept open | §4.4 |
| R1-F6 impact | In-game target, roll target and a separate cue claim taken last; production-path controls | §4.5 |
| R1-F9 Dust ownership | Creation tracking scoped to the execution tick; nested/interrupted tests | §4.6 |
| R1-F11 reuse and donor | Own-instance comparison; donor timeline through the Dust release; two-instance restore | §4.6 |
| R1-F13 screen capture | Production decision extracted and tested | §5 |
| R1-F14 rollback | Failures reported with the original; fault-injected native checks | §4.6 |
| R2-F1 reload verdict | Product stop-result contract, saved ID and ordering; verdict tests | §4.7 |
| R2-F2 probe cleanup | Iterator disposal, held restores on every exit, scoped seeding and generator restore; abandonment test | §4.7 |
| R2-F3 Sickened | Only breath-owned instances removed; foreign control survives | §4.6 |
| R2-F4 borrowed ability | Borrowed by instance, overrides restored, grants released by instance | §4.1 |
| R1-F7, F8, F10, F12 | Verified in round 2; evidence retained and re-run | §4.5–§4.7 |
