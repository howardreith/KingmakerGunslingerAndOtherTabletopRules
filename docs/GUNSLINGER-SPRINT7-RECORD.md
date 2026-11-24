# Sprint 7 record: presentation and audio remediation (S7 T1–T7)

Status: **IN REVIEW (round 4 of 5, 2026-11-24).** The owner resumed the stopped enrollment through owner amendment 1
(`resume --amend-mission`; amendment revision 1, sha256 `59be8e3f…31e0`; message sha256 `f4d26be0…a7dd`, stored outside
the worktree at `<lab>/owner-guidance/S7-OWNER-AMENDMENT-1-2026-11-24.md`). The round-3 BLOCKED stop (no progress on R1-F1,
R1-F3, R1-F5, R1-F9) and every earlier failure remain recorded. **Nothing is merged or released, and no visual or listening
choice has been accepted.** Twelve findings were pending after round 3. Each now has a correction or a precise disposition
for independent review (§7).
- Draft PR #219; one agent-review enrollment (`kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`, foreground,
  single plan); branch `codex/s7-visual-audio-remediation`.
- Mission: `docs/missions/sprint7/SPRINT7-AUTONOMOUS-MISSION.md` (verbatim, sha256 `c232f135…f56f0`), plus owner amendment 1.
- Amended acceptance (owner amendment 1):
  - The Dust breath keeps its released Sickened behaviour; the mission's "blindness" was a mistake.
  - The Trample/Overrun wording "Target point within twice this creature's speed." is approved.
  - `Sweetener_Low_06_Quake` and `InspireCourage` are kept provisionally; neither is acoustically accepted.
  - Technical audio readiness means cue ownership, timing, retirement, provenance, and a playable private packet for the
    owner. The text-based reviewer is not required to hear audio.
  - The Ghaele conclusion stays bounded, and the October incident stays unattributed.
- Technical status, observed behaviour, unexecuted paths and owner taste are kept separate below. Every visual and
  listening judgment remains the owner's.
- Review history:
  - round 1 (head `0910995241`): 14 findings;
  - round 2 (head `0d976f4fd1`): 4 verified, 10 open plus 4 new;
  - round 3 (head `511160dc6e`): 8 resolved, 6 open plus 6 new, then the controller's no-progress stop.

## 1. Starting point

- Sprint 6 was merged (PRs #198–#216) and released: stable **v0.0.195**, release commit `ef0e1fe15d` (PR #217), records
  PR #218. Post-release `origin/master` is **`35763430ac`**, the base of this branch.
- Released package `456d504d…9444`, DLL `3ef052ea…6a6b`.
- Environment: disposable loader-compat game copy, Unity Mod Manager owner profile 0.33.0 (its `0Harmony12` is a shim over
  Harmony 2), no other mods.
- Harness `KmgLoaderCompatHarness.dll` from this branch, by round:
  - round 1 `04c9d1d4…`; round 2 `9e90d22c…`; round 3 `3d86bd3a…`;
  - round 4 `dcdf4574…` (first runs, retained) and **`71b0c6f03de48fce27d3e6d8ef384769b524e09b3c47f21b740b1bfdb7475701`**
    (final).
- Blender 4.5.10 `8b15ef73…313c` with `PYTHONHASHSEED=0`.
- Carried forward unchanged and outside this sprint: Sprint 6 accepted scopes, REP-05/REP-06 BLOCKED, VER-02 open with
  G5 fixture-blocked, and the other limits in `docs/GUNSLINGER-SPRINT6-DISPOSITION.md`.

## 2. Outcome

| Story / record | Original problem | Result | Evidence | Technical status | Retained limitation / owner decision |
|---|---|---|---|---|---|
| S7 T1 / REP-04 | October review: "increasing" Ghaele dissolve; a scripted move that did not start | **No product change.** On released v0.0.195 and on the candidate, an ordinary Ghaele keeps its body (cutout 0.00) to its native 120 s expiry, moves, and fights. Its raw `_Dissolve` is the native looping non-cutout animation the unsummoned donor also runs: 11/11 positive samples per package, with driven body-material identity recorded. | Harness `…162819Z` (released) and `…163235Z` (candidate); product `…050957Z` `gh`/`ghd` (round 3) | Current-build disposition, bounded to the observed packages and conditions | Released baseline observed only in the admitted fixture's interior throne room. The outdoor/bright released case has no admitted route (§4.1). **October incident unattributed.** Owner: accept the bounded disposition. |
| S7 T2 / VIS-01 | 48 zero-area wing-root triangles (and inside-out welded membranes) | **Repaired at the generator.** 0 zero-area triangles, 0 repeated edges, all parts outward | `ed88ad994e`; export receipt; models `…162110Z` (round 4, party views bound to the body) and `…050957Z` | Implemented and verified | Owner visual acceptance |
| S7 T3 / VIS-02 | 19/12,924 normal corners at a waist pleat and the left-hand fingers | **Repaired from the exact private input.** 16 → 0 opposing corners | `d864e7b01f`; export receipt; `…162110Z`, `…050957Z` | Implemented and verified | Owner visual acceptance |
| S7 T4 / VIS-03 | Owned Trample/Overrun windows: "Target point within 0 ft. feet." and school "None" | **Scoped presentation override:** "Target point within twice this creature's speed." (owner-approved wording), no school | `2614ea3bd6`; `…050957Z` `ui` | Implemented and verified for desktop fact and HUD | **Controller mode unexecuted** (§4.4): no admitted route creates or selects a summon in the console UI. Not a controller PASS. |
| S7 T5 / VIS-04 | No added impact sound on the Ankylosaurus tail strike | **One owned cue per resolved tail roll on the roll's own in-game target** (`Sweetener_Low_06_Quake`, provisional) | `90bc2ae7eb`; fx `…170720Z` (round 4) and `…061437Z` | Cue ownership, timing and retirement verified; private packet delivered | A survivor cue's native end callback was missing in one run (`…164749Z`; cause unknown, retained). **Listening and choice pending (owner).** |
| S7 T6 / VIS-05 | The Dust Mephit breath shows the ice cone | **Owned per-instance dust recolour** of the one cone CreateView spawns for the executing owned Dust breath, now bound to that native spawn call; restored on release; released mechanics (Sickened, damage, save, enemies only, action) unchanged | `9733a0c9b7`; fx `…170720Z` | Implemented and verified | Owner visual acceptance. Allies' harmless saves stay documented. |
| S7 T7 / VIS-06 | Provisional `InspireCourage` cue on the Trumpet Blast | **Kept provisionally** (no code change); six alternatives sampled; cue verified across an in-process save and load, with blast-owned seeding | `r6` in `…170720Z`; Trumpet reload `…162605Z` | Cue technically verified; private packet delivered | **Listening and choice pending (owner).** |

## 3. Candidates, sessions and identities

**Round 4.** Product code changed only in `9733a0c9b7` (Dust call-site binding). Every later commit changes only
`RuntimeTesting/`, the harness, the tests and the sessions; product code outside `RuntimeTesting/` is identical from
`1ac4324617` to `bc67438f3b` (`git diff` empty).

| Candidate | Package / DLL SHA-256 | Harness | Session | Result |
|---|---|---|---|---|
| `1ac4324617` | `fcc8ee81…1e0e` / `d826ae76…5335` | `dcdf4574…` | 5 launches refused before acquiring anything (harness `Info.json` missing from the build folder) | refused, nothing acquired |
| `1ac4324617` | as above | `dcdf4574…` | `20261124T154136Z-…-s7-fx` | FAIL, retained (observer): the first Dust and Ice Mephits reached their native 120 s expiry during the added steps, and the early-exit cast only refreshed an existing Sickened. Nested interruption PASS. 7 min 52 s |
| `1ac4324617` | as above | `dcdf4574…` | `20261124T154929Z-…-s7-models` | FAIL, retained (observer): the setup-failure trace rollback read a never-patched method through the Harmony12 shim. Party captures, failed-normal-disposal case and dim phase PASS. 4 min 53 s |
| `1ac4324617` | as above | `dcdf4574…` | `20261124T155421Z-…-s7-trumpet-reload` | PASS (superseded by `…162605Z`), 2 min 17 s |
| released v0.0.195 / `1ac4324617` | `456d504d…9444` / `fcc8ee81…` | `dcdf4574…` | `20261124T155639Z`, `20261124T155845Z` `-s7-ghaele` | FAIL, retained (observer): every ownership readback held; the bystander move had ended on its own before it was read. 2 min 7 s, 2 min 0 s |
| `a07716e3ab` | `4dc92312a19670b116fd2482b9083312261b01124f8c6d99bfb52096cebec41a` / `c26b7aa02b5650e6277c1ae208efbc58baa0cd9193356d9ed1ab3bb53369857f` | `71b0c6f0…` | `20261124T161344Z-…-s7-fx` | FAIL, retained (observer): the fresh Dust Mephit's Sickened refreshed an instance the cell's own Ice breath had introduced, which the ledger did not own. 7 min 25 s |
| `a07716e3ab` | as above | `71b0c6f0…` | **`20261124T162110Z-…-s7-models`** (mod, modd) | **PASS 2/2**, 4 min 56 s |
| `a07716e3ab` | as above | `71b0c6f0…` | **`20261124T162605Z-…-s7-trumpet-reload`** | **PASS**, 2 min 13 s |
| released v0.0.195 | `456d504d…9444` / `3ef052ea…6a6b` | `71b0c6f0…` | **`20261124T162819Z-…-s7-ghaele`** (lifecycle, timeline) | **PASS**, 4 min 16 s |
| `a07716e3ab` | as above | `71b0c6f0…` | **`20261124T163235Z-…-s7-ghaele`** | **PASS**, 4 min 19 s |
| `2e2737f8c6` | `620d04e9…8c53` / `e4672be5…7952` | `71b0c6f0…` | `20261124T164749Z-…-s7-fx` (diagnostics) | FAIL, retained: the same Sickened ownership gap, with diagnostics; and a lost native end callback for the surviving Ankylosaurus cue (cause unknown). 8 min 42 s |
| **`bc67438f3b`** (final) | `7adde59cce585da3543a849c010ba186dbbca3407c8a67bfb27df6c5f0d2d894` / `aeb633f7d82ecb7d81a188dd80da98a2ea279723d958b8b7ad41939c04829fd2` | `71b0c6f0…` | **`20261124T170720Z-…-s7-fx`** (fx, r6) with loopback audio | **PASS 2/2** (fx 31/31, r6 16/16), 7 min 16 s |

Identity binding:
- Effects and R6 bind to `bc67438f3b`.
- Models, Trumpet and candidate Ghaele bind to `a07716e3ab`. From `a07716e3ab` to `bc67438f3b`, only the effects observer
  changed.
- UI and the product Ghaele phases bind to round 3's `6f2f16ca88`. Their dependencies are unchanged, so they were not
  rerun.
- Every session completed its transaction, applied and restored game settings, and verified owner saves and the
  installation; the outer receipts are in `docs/evidence/sprint7/r4/*-session.json`. The five refused launches acquired
  nothing.
- The human installation was never touched. The game-copy window was agreed with the peer session sharing this machine,
  and released afterwards.

Round 3 (retained): `6f2f16ca88` (`8039a627…` / `d7088878…`) core `…050957Z` PASS 5/5; Trumpet reload `…052423Z` PASS;
Ghaele `…052619Z` (released) and `…052954Z` PASS; `f564cbb844` (`d4ded9a8…` / `192c9726…`) fx `…061437Z` PASS 2/2; failures
`…050257Z`, `…054209Z` and `…055920Z` retained. Rounds 1–2: `docs/evidence/sprint7/r2/MANIFEST.json`.

Builds: `Build-Local -DevelopmentPurpose s7-visual-audio -DevelopmentTestFilter sprint7-visual` with the pinned reference
bundle. Each took 7–8 min, passed strict package validation, and deployed nothing.

Expenditure. The round-4 cost proposed before any native work was 1 build and 6 sessions, about 42 minutes. The actual
cost was higher because four observer defects needed new candidates; every failed run is retained above.

| Round | Builds | Sessions | Game-copy time |
|---|---|---|---|
| 1 | 5 | 5 | 68 min |
| 2 | 4 | 6 | 68 min 9 s |
| 3 | 4, plus 1 refused at the clean-commit check | 8 | 51 min 57 s |
| 4 | 4 candidates and 2 harnesses | 12, plus 5 refused before acquisition | 58 min 16 s |

Reviewable evidence (committed):
- `evidence/sprint7-review/`: 94 curated stills, including the 12 added in round 4 (party views bound to the body, and the
  released/candidate Ghaele idle and combat views). `INDEX.json` gives each lab original's SHA-256, the committed file's
  SHA-256 and its source session. PNGs are re-encoded as JPEG quality 92.
- `docs/evidence/sprint7/r4/` (and `r3/`, `r2/`): sanitized extracts of every launch and of each session's outer receipt;
  `MANIFEST.json` gives each raw source's SHA-256.
- `docs/evidence/sprint7/exports.json`: generator, input and output identities.
- `docs/evidence/sprint7/audio-context.json`: round-3 excerpts, with the distance labels corrected in round 4.
- `docs/evidence/sprint7/audio-context-r4.json`: the round-4 packet manifest (§4.5).

The private listening packet is described in §4.5. Game audio is never committed.

## 4. Per story

### 4.1 T1 / REP-04 Ghaele

Harness probe `kmgGhaeleLifetime`, round 4, run identically on the released v0.0.195 package (`…162819Z`) and the
candidate (`…163235Z`), in the admitted fixture save KMCS_AFTER_NORMAL_012_MODONLY (Jamandi mansion throne room):

**Cast (R2-F4, R3-F5).**
- The published summon is cast as one owned native `UnitUseAbility` from the main character, at caster level 20 for that
  cast only.
- The ability loan (borrowed parameters, or the exact temporary grant) ends when the cast settles, before any lifetime
  observation. At the first post-cast observation the grant is gone.
- The command, and any Ghaele it produces, is owned from submission. Each release is held in the probe's resources.

**Lifecycle cases (both packages, all held).**
- A borrowed ability with non-default parameters (CL 7, SL 3) was cast at CL 20 and restored, same instance, same
  parameter object.
- A temporary grant replaced during the cast: the replacement survived the loan's release untouched.
- Abandoned with the command pending (not acted, no execution): the command was settled, the grant removed, and no late
  summon appeared.
- Abandoned after the Ghaele spawned but before registration: the early summon was destroyed (1), and no owned command or
  grant remained.
- In both abandonment cases, an unrelated unit's command started just before the abandonment was still held after it.

**Timeline.**
- Appearance ends at 1.72/1.73 s and is kept.
- The body's `_DissolveCutout` stays 0.00 in life; raw `_Dissolve` reaches 1.00.
- Ordinary move of 4.0 m.
- Native lifetime ends at 119.7 s; the unit is removed at 121.55/121.56 s.

**Combat and lighting (R3-F1).**
- Combat: the Ghaele made three commanded attacks, all hits, on a disposable hostile target. The target is a
  `DefaultPlayerCharacter` copy under the loaded faction `StagFort_AuchDovan` (the sixth faction tried; brain off).
- Lighting was recorded idle and in combat: ambient 1.0 (0.375, 0.329, 0.270); interpolated light-probe DC luminance
  0.241 idle and 0.207 in combat; 20 and 18 lights in range.
- The game camera's party view, bound to the enabled body renderer, was in frame, lit, with no fade:
  - released: luma 36.0 idle, 41.9 in combat;
  - candidate: luma 35.9 idle, 45.5 in combat.

**Attribution (R3-F6).** Only positive samples count. A sample is positive when an active, looping, non-cutout animation
that the donor also runs drives a readable body material to its own current value. Empty, inactive, delayed-only or
unreadable samples are inconclusive.
- Round 4: 11/11 in-life snapshots positive per package, with body-material identity recorded; 0 contradicting.
- Re-evaluation of the round-3 raw rows (`…052619Z` released, `…052954Z` candidate) through the corrected predicate, by
  the domain test `sprint7-visual.ghaele-attribution`: 11/11 positive each. Body-material identity was not recorded in
  those rows. Their original verdicts are kept.

Candidate, product observer (round 3, `…050957Z`, bright outdoor and dim interior): ordinary casts, body cutout 0.00 to
expiry, moves of 3.9–4.0 m, three native weapon-attack rules (bright), and removal 121.6–122.0 s after appearance.

**Released-baseline boundary (R3-F1).** The admitted fixtures that can run the released package were each inspected:
- KMCS_AFTER_NORMAL_012_MODONLY (harness ops): used above. This gives one interior lighting condition, plus combat.
- The KMG automation baseline/working saves admit only named product scenarios with the owner's mod stack. v0.0.195 has
  no Ghaele presentation scenario there.
- The product's fresh-world review fixtures (outdoor Default, static interior) admit only the phases compiled into the
  loaded package. In v0.0.195 the runtime code that references the Ghaele checks its owned grant and descriptions
  (`summon-playtest-r3`), cleric conversion spawns and roster spawns. None of it observes the Ghaele's body, lifetime or
  combat presentation.

The bright outdoor released case therefore has no admitted route. It stays a retained limitation; candidate outdoor
evidence does not replace it.

Disposition and open scope:
- No product material, lifetime or despawn change is warranted for the observed released or current packages under these
  conditions.
- The October incident is **not attributed**. Its run's writer, build and frames were not re-read, and no observation
  ties its reading to this loop.

### 4.2 T2 / VIS-01 Nalfeshnee

See `assets-source/original-models/sprint39-nalfeshnee/SOURCE.md` and `docs/evidence/sprint7/exports.json` (base
generator → R7 `d34e6c19…`; repaired → mesh `6ff85493…`, albedo `1e9f5303…`, twice each).

Native, round 4 (`…162110Z`, R1-F3):
- **Gameplay-distance party view.** For each state (after and before), the shared capture helper is given the enabled
  body renderer's exact name.
  - Its readback is judged in the verdict: image written, in frame, lit, fade cleared, renderer enabled, the renderer's
    current mesh, body intact.
  - After reads `KMG_nalfeshnee_Original`; before reads `KMG_nalfeshnee_S7BeforeEvidence`. Bright luma 56.5, dim 32.4.
  - A domain test covers absent, wrong-body, disabled, unlit, out-of-frame, faded and dissolving captures.
- **Natural appearance and body binding.** As in round 3: the published spell, a native appearance completing on its own,
  and every view's body readback naming the enabled renderer (11,593 vertices after; 12,402 for the pinned R7 export
  before).
- **Whole-body views.** These are context only, judged on nine sampled body rays. Dim rear (after and before): 4/9 rays
  blocked by the gallery at the best yaw (+45°), so it is identified as obstructed. All bright views and every wing-root
  detail are clear on nominal bearings. The anatomical claims rest on the details.
- **Motion.** Idle, move, a turn over 45°, and an attack (bright), after and before.

### 4.3 T3 / VIS-02 Salamander

See `assets-source/original-models/sprint17-serpents/SOURCE.md` and the export receipt (exact private capture
`eb0d5eaa…`; base generator → released `b5524a69…`; repaired → `0837e796…`, twice).

Native, round 4 (`…162110Z`):
- Party views bound to the enabled body: `KMG_SalamanderHuman_…_Mesh` after and `KMG_salamander_S7BeforeEvidence`
  before; luma 50.4 bright, 33.3 dim.
- Waist front and rear and both hands, after and before, all clear. Only the dim right-hand detail uses a yaw offset (−45°).
  No obstructed whole-body view.

### 4.4 T4 / VIS-03 Trample/Overrun windows

Released baseline (`…233302Z` r3): both owned windows read "Target point within 0 ft. feet." with the header "None".

Candidate (`…050957Z` ui, 14/14):
- The Brachiosaurus Overrun and Trample and the Mastodon Trample, through the description window and the HUD path, read
  "Target point within twice this creature's speed.", with no "0 ft" and no "None". The owner approved this wording.
- Controls: native Overrun keeps its native target text; native Magic Missile keeps "Evocation".
- Ownership: an existing control fact is used as it is. Only a fact the observer granted is released, by instance, while
  the actor still holds that instance.

Controller mode (R1-F5), traced through both existing console routes. This trace was read from source and native
decompilation; nothing was executed. It corrects the round-3 text, which wrongly said only the teleportation and firearm
console routes exist. **Controller mode is unexecuted. This is not a controller PASS.**

**Admission.** `IsCleanup2ConsoleReview` (`RuntimeTestRunner.SummonCleanup2ConsoleScene.cs:11-13`) also admits the
cleanup2 phases `elementals-console` and `alternate-console`:
- The scenario is `disposable-expanded-summoning-cleanup2-native-review` with exactly one parameter, `reviewPhase`
  (`RuntimeAutomation.Common.ps1` `Test-KmgCleanup2ReviewPhaseScope`, `Test-RuntimeScenarioPreflight.ps1`).
- The runtime refuses any other parameter count (`SummonCleanup2ElementalConsole.cs:195-198`,
  `SummonCleanup2AlternateMenus.cs:186-190`). No creature, ability or target can be named.
- Both phases set controller mode before world creation (`SummonCleanup2ConsoleScene.cs:23-67`). Both load the Oleg
  outdoor area with no save, through `ExpandedSummoningNativeProfile.cs:114-131,165-170`. In that fresh world the native
  console in-game UI is live.

**`elementals-console`**
- **Units and selection.** The only unit is the fixture caster, a level-20 Wizard clone of `DefaultPlayerCharacter`
  (`SummonCleanup2Review.cs:115-146`). The phase calls `selection.SelectUnit(fixture.Caster.View)`
  (`SummonCleanup2ElementalConsole.cs:281-284`).
- **Guards.** `uiOwned` (`:224-230`) requires `bar.Unit.Value` and `CurrentSelectUnitValue` to be `fixture.Caster`, and
  the selector guard (`:231-251`) requires the selector owner to be the caster.
- **Input.** Only the selector handlers and the converted-panel handler are driven, through
  `InvokeTeleportGamepadInput`. The fixed small-fire-elemental source is cast; Summon Monster II, Summon Nature's Ally II
  and Magic Missile appear only as selector rows.

**`alternate-console`**
- **Units and selection.** It selects only the alternate-source casters it creates (`SummonCleanup2AlternateMenus.cs:608-611`).
- **Input.** It opens the converted panel without casting. Its one description-like step is a hover tooltip on the
  converted Horse row (`:845-852`), not the description window.

Neither route calls `OpenDescriptionWindow`, `HandleOpenDescription` or `OnRightStickButton`, or presses the console
description button.

**Native console path (Assembly-CSharp)**
- **Bar binding.** `ActionBarVM.Unit` is `UnitSelectionManager.Instance.CurrentSelectUnit`, and its pages are built from
  that unit's `UISettings`. `ActionBarSelectorVm` reads the same unit's abilities. Every slot, selector row and
  converted row therefore belongs to the selected unit.
- **Opening the description.** Button 19 is bound to `ActionBarVM.OnRightStickButton` (gated by `TooltipEnabled`). That
  toggles the current slot's tooltip, and `ActionBarSlotView.ShowHideDescriptionWindow` calls
  `TooltipTrigger.OpenDescriptionWindow`, which reaches `HandleOpenDescription`.
- **Selection guard.** The console `UnitSelectionManager.SelectUnit` returns without selecting when
  `!selectedUnit.IsDirectlyControllable` (line 80, verified in the decompiled source). A summoned unit takes that flag
  from `UnitPartSummonedMonster`, which is set from the spawning `ContextActionSpawnMonster.IsDirectlyControllable`.
- **Bypass.** `ForceSelectUnit` skips the check, but `SelectionManagerBase.LateUpdate` deselects units that are not
  directly controllable, so using it would be a bypass.

**First unsupported operation.** In both routes, the first operation not admitted is creating a summoned creature
that owns Trample or Overrun (a Brachiosaurus or Mastodon). Neither phase casts such a summon or can be given one:
the single-parameter guard above.

The next operation, selecting that summon as the console action-bar unit, is refused twice:
- by the routes' own actor guards (`uiOwned`, `SummonCleanup2ElementalConsole.cs:229`;
  `SummonCleanup2AlternateMenus.cs:610`);
- natively, by `UnitSelectionManager.SelectUnit`'s controllability check. It is inferred, not checked per blueprint,
  that this product's summon spawns keep the native `false` flag; no repository code sets it.

The owned Trample and Overrun belong to the summon blueprints (`ExpandedSummoningSpecialBuilder.cs:925-935`), so no
caster's bar can legitimately hold them. Native Overrun is also absent from the console casters.

Magic Missile is on the elemental caster's selector, but no admitted step opens its description window. The desktop
fact/HUD evidence above is unaffected. Reaching the console description of an owned summon action would need a new
admission (a summon cast plus selection of a non-controllable unit, or a new owned-description dispatch). This
continuation does not create one.

### 4.5 T5 / VIS-04 Ankylosaurus impact cue

Product (round 3): the cue has its own once-per-roll claim. It is taken only after the exact owner, tail weapon, resolved
hit, in-game victim and the roll's own target all hold. The strike-rider stamp is unchanged.

Native, round 4 (`…170720Z`, `bc67438f3b`):
- **Hits.** Two resolved tail hits each posted exactly one cue in the hit's own frame (IDs 105 and 175), ending naturally
  after 1.56 s and 2.14 s.
- **Listener distance.** Measured in the resolving frame: 37.45 m camera to emitter.
- **Controls and replays.**
  - production-path controls: valid roll 1 cue; foreign weapon, wrong-target wrapper and out-of-game target 0; the same
    roll's valid wrapper 1;
  - replays 0; another owner 0; a later hit 1; miss and cancel 0.
- **Retirement with both cues live.** Retired ID 451 had its stop requested and its native end within 0.055 s. The
  survivor 449 ended naturally after 2.28 s.
- **End-of-phase dump.** All 8 cue records of the phase have their native end.

Retained, cause unknown: in `…164749Z` (same product code) the surviving cue 497 had no native end callback within 10 s
of the retirement. It joins round 2's lost callbacks. It did not recur in `…154136Z`, `…161344Z` or `…170720Z`.

**Private listening packet (R1-F1, owner amendment 1).**
- **Location:** `C:\Dev\KingmakerGunslingerLab\private\s7-listening-packet-r4\` and
  `C:\Dev\KingmakerGunslingerLab\private\s7-listening-packet-r4.zip` (sha256 `b2fddccd…17d0`).
- **Contents:** `index.html` (players per clip, grouped and labelled), `playlist.m3u8`, `MANIFEST.json`, `README.txt` and
  `clips/`.
- **Clips:** 18 excerpts from the `…170720Z` loopback recording (`f67b1d4f…e37f`):
  - the two in-play Ankylosaurus hits (actual);
  - the in-play Trumpet Blast (actual, ID 397, 37.09 m at its post);
  - the Vrock neighbour (40.05 m);
  - two quiet baselines;
  - isolated observer samples: the two current cues and eleven alternatives.
- **Per clip:** hash, session, package, UTC interval, event, playing ID, emitter, game volumes (all 100) and listener
  context.
- **Verification:** every clip matches its lab excerpt's hash, decodes as 16-bit PCM, and loads in the Windows WAV
  decoder; every link resolves; the zip tests clean. Browser playback itself was not exercised.
- **Kept separate:**
  - technical playback, timing and ownership: verified;
  - clip integrity and accessibility: verified;
  - actual listening: none, since no agent here has supported audio perception;
  - suitability and the final choice: the owner's. `Sweetener_Low_06_Quake` stays provisional.

Distance corrections: the round-3 tail-hit distance (37.45 m) was sampled after both strikes plus a 4 s wait, not at
posting. It is now labelled that way in `audio-context.json`, the round-3 event-time distance is "not measured", and the
round-3 R6 distance is "not measured". Nothing is estimated.

### 4.6 T6 / VIS-05 Dust Mephit breath

**Product change, round 4 (R1-F9).** The round-3 creation tracker is removed.
- A Harmony12 transpiler on `ProjectileController.CreateView` routes its one
  `FxHelper.SpawnFxOnPoint(GameObject, Vector3)` call through `DustMephitBreathPresentation.SpawnCastFx`, passing the
  creation's projectile.
- `SpawnCastFx` makes the same native call. It adopts the returned instance only when that creation is the owned Dust
  breath's (`CastEffectAdoption.Spawn`: executing ability, launcher, cold-cone projectile, own prefab, own launch point).
- Nothing is pending between calls, so:
  - an interrupted creation lends nothing;
  - a spawn by any other caller never reaches the adoption;
  - if the call is not found exactly once, CreateView is left native and no cone is recoloured (`Installed` readback).
- Reproduced before the repair: a domain case on the round-3 tracker gave the foreign spawn the tint
  (`foreignTints=1`; `<lab>/runtime-evidence/s7/r4-offline/r1f9-repro-before-repair.txt`).

Native, round 4 (`…170720Z`):

**Nested interruption (R1-F9)**
- While an owned Dust creation was in progress, an inner owned creation of the same breath was interrupted
  (NullReferenceException, after setting its launch point and before its spawn).
- Before the enclosing call returned, another caller spawned the same cast effect at that exact point. That instance
  stayed native: no tint, the native signature `fee8e469…`.
- The enclosing creation's own cast effect was then tinted exactly once and restored on release.

**Casts and mechanics** (unchanged released mechanics, by each breath's own context)
- Three consecutive breaths ran from one position on the same pooled instance −237330, with the installed call site:
  - natural 1: 4 damage, full, Sickened;
  - natural 20: 1 damage, half, no condition;
  - natural 1: 4 damage, full, Sickened.
- The ally rolls a harmless save; the enemy outside and the caster behind are never selected.

**Controls (unchanged and passing)**
- The Dust Mephit's own Ice breath and the Ice Mephit's breath reused the instance untouched.
- The Ice donor held its native signature through the Dust release.
- Of two live Dust instances, only the exact one was restored.
- A pre-execution cancel created nothing.
- Source loss read native.

**Conditions (R3-F3)**
- The cell's ledger owns every Sickened instance delivered by an ability execution of the cell's own units, captured at
  delivery. This matters because native Sickened does not stack: in `…161344Z`/`…164749Z`, later Dust applications only
  refreshed an instance the cell's own Ice breath had introduced.
- Final run: 13 owned executions, 4 instances acquired, 1 live at cleanup after the last cast and removed, 0 remaining.
- The foreign control (now on the caster behind the cone, a unit that outlives the cell) was still present at cleanup.
- Early exit: a fresh Dust Mephit's cast, owning its own ledger, was abandoned the moment its Sickened was applied, with
  the command still running. Its one instance was removed, its command interrupted, and the foreign control kept.

**Trace release (R3-F4)**
- Normal disposal feeds `Sprint7ObserverPolicy.Released` into the cell check and the outer restoration check.
- Final run: 4 hooks → 0, no failure.
- Models phase (`…162110Z`): a normal disposal with an injected CreateView unpatch failure gave a failed verdict. The native
  spawn and product-wrapper releases still ran, leaving 2 CreateView hooks and none elsewhere; a later disposal removed
  them.
- The setup-failure rollback still leaves 0 hooks, and the body-swap rollback is unchanged. Under the Harmony12 shim, hook
  counts and unpatches now act only on methods the trace patched.

Design questions resolved by owner amendment 1: the breath keeps Sickened. Allies' harmless saves stay documented, out
of scope.

### 4.7 T7 / VIS-06 Trumpet Archon

R6 (`…170720Z`):
- One `InspireCourage` cue per blast, ending naturally (ID 397; listener 37.09 m at its post).
- None on a cancelled cast.
- Early retirement stopped by playing ID.
- The Vrock neighbour completed naturally.

Reload (`…162605Z`, R2-F2, R3-F2):
- **Reload.** The saved cue (ID 159) ended natively and naturally. Nothing was posted after the load, and the fresh blast
  posted one new cue (ID 323).
- **Blast-owned seeding.**
  - A saving throw is seeded only when its rule's context is the observed blast's own execution and its saver is not the
    Trumpet. The generator is saved at that rule's start and put back at that rule's end (`OwnedRollScope`).
  - 25 Fortitude saves of the first blast were seeded.
  - While the blast command was pending (before its execution began), an unrelated Will save and an unrelated draw kept
    the generator's own control values (d20 9 and 9; draws equal), with no seeding.
  - After the command, the generator was not back at its blast-start state.
- **Abandonment.** A blast probe was abandoned by its 4 s step timeout (expected failure, isolation restored). The check
  step then found the subscription ended, no intervention open, the observed command cleared, nothing held, the Trumpet
  destroyed, and no release failure.
- **Cleanup propagation (R3-F2).** Both probes use `YieldedProbeLifecycle`: every held release's outcome, from whichever
  drain ran it, reaches `ProbeSlot`.
  - Reproduced before the repair with the round-3 pattern: isolationRestored stayed true after a failed unsubscribe
    (`<lab>/runtime-evidence/s7/r4-offline/r3f2-repro-before-repair.txt`).
  - The domain case `sprint7-visual.probe-cleanup-reaches-slot` now shows, through `ProbeSlot`: the named failure, every
    other release attempted once, the original error kept, IsolationRestored=false, and refusal to start another probe.

`InspireCourage` is kept provisionally; the listening comparisons are in the private packet (§4.5).

## 5. Validation

Focused domain tests, `sprint7-visual.*` (16 cases):
- VIS-01/VIS-02 decoded exports;
- VIS-03 identities and text;
- VIS-04 qualification;
- VIS-05 identity, tint, and call-site ownership through the reviewer's interval (`vis05-call-site-ownership`);
- Sprint 7 admission and screen-capture decision; observer ownership;
- Trumpet reload verdict and abandonment;
- cleanup reaching `ProbeSlot` (`probe-cleanup-reaches-slot`);
- blast-owned roll scope (`trumpet-owned-roll`);
- positive Ghaele attribution with negative cases and the retained-row re-evaluation (`ghaele-attribution`);
- the condition ledger, release verdict and party-capture verdict (`cell-ownership-and-capture`).

Neighbour selections: `summon-playtest-r3`, `-r4`, `-r6`, `-r7`, `sprint22-`, `sprint5-`, `salamander`,
`native-admission`, `native-profile` and `loader-compat-probe` (111 tests in total, all passing locally). The controller
records the required gate receipts on the reviewed head.

## 6. Owner decisions pending and open scopes

1. T1: accept the bounded current-build disposition. The released outdoor/bright case has no admitted route; the
   October incident remains unattributed.
2. T2, T3, T6: visual acceptance of the Nalfeshnee wing roots, the Salamander waist and fingers, and the dust cone.
3. T4: controller mode stays unexecuted (§4.4); it would need a new admission, which this continuation does not create.
4. T5, T7: listen with the private packet and choose; both cues stay provisional until then.
5. Merge and release remain the owner's; this sprint authorizes neither.

## 7. Review findings and corrections

Round-4 correction matrix (the twelve findings pending after round 3):

| Finding | Exact trigger | Owning code | Change or amended disposition | Decisive check | Evidence | Remaining limit |
|---|---|---|---|---|---|---|
| R1-F1 audio | Cue recommendations followed to recordings and distances | lab packet; `RuntimeTestRunner.Sprint7Presentation.cs` (`S7AttackLog`), `SummonPlaytestR6Presentation.cs` (`R6RunPassive`) | Private playable packet; event-time distances recorded; round-3 labels corrected; no listening claim (owner amendment 1) | Packet self-verification and Windows decoder load; distances in the resolving or post frame | `<lab>/private/s7-listening-packet-r4`, `audio-context-r4.json`, `…170720Z` | Owner listening and choice pending; browser playback not exercised |
| R1-F3 party view | Normal-distance capture of a disabled accessory or an empty image | `S7Stills`, `Sprint7ObserverPolicy.PartyCaptureValid` | Capture bound to the enabled body renderer and judged in the verdict; whole-body views sample 9 rays and are context only | Party valid on all 8 states; negative cases in the domain test | `…162110Z`, 8 stills | Dim Nalfeshnee rear whole-body view 4/9 rays obstructed (identified, context only) |
| R1-F5 controller | Claim that console routes need new admission | `SummonCleanup2*Console*.cs`, native console UI | Both cleanup2 console routes traced; wrong round-3 claim corrected; first unsupported operation and guards named | Source and decompiled citations (§4.4) | this record | Unexecuted; not a controller PASS |
| R1-F9 Dust | Interrupted inner owned creation, then a foreign same-point spawn before the outer returns | `DustMephitBreathPresentation.cs`, `MephitBreathPolicy.cs` (`CastEffectAdoption`) | Adoption bound to CreateView's own spawn call (transpiler) | Native nested-interruption check; domain interval case; pre-repair reproduction | `…170720Z`, `r1f9-repro-before-repair.txt` | — |
| R2-F2 Trumpet seeding | An unrelated save or draw while the blast yields | `TrumpetCueProbes.cs`, `TrumpetCueVerdict.cs` (`OwnedRollScope`) | Seeding only on the blast execution's own saves, generator put back at that rule's end | Native interleave and no rewind; abandonment check; domain roll-scope case | `…162605Z` | The interleave happened while the command was pending, before its execution began |
| R2-F4 Ghaele loan | Borrowed or granted ability held through the lifetime | `GhaeleLifetimeProbes.cs` (`CastOwned`) | Loan released when the cast settles | Lifecycle borrowed and replaced cases; post-cast readback | `…162819Z`, `…163235Z` | — |
| R3-F1 released combat/lighting | Attack and lighting attributed to the released package | `GhaeleLifetimeProbes.cs` (`Observe`) | Released-package combat and lighting observed in the admitted fixture; boundary for outdoor/bright recorded | 3 hits, idle and combat lighting and party views | `…162819Z` | Released outdoor/bright: no admitted route |
| R3-F2 cleanup propagation | Failed restore during iterator disposal | `TrumpetCueVerdict.cs` (`YieldedProbeLifecycle`, `OwnedProbeResources.Outcome`) | Every drain's outcome reaches the slot | Domain case through `ProbeSlot`; pre-repair reproduction | `r3f2-repro-before-repair.txt`; Trumpet abandonment `…162605Z` | — |
| R3-F3 later Sickened | A later cast's Sickened surviving cleanup | `S7DustConditions`, `AcquisitionLedger` | Cell-unit executions own their conditions (captured at delivery); early-exit case | Live later-cast instance removed; early exit; foreign kept | `…170720Z` | Sickened that expires natively before cleanup is gone, not counted as live |
| R3-F4 trace release | Failed normal unpatch with PASS | `S7DisposeTrace`, `Sprint7ObserverPolicy.Released`, `S7ProjectileTrace` | Release verdict in the cell and outer checks; injected failed normal disposal | 4→0 positive control; injected 2 remaining, failed verdict, other releases run | `…170720Z`, `…162110Z` | — |
| R3-F5 Ghaele command | Timeout or abandonment after `Commands.Run` and before registration | `GhaeleLifetimeProbes.cs` (`CastOwned`) | Command and late summon owned from submission | Lifecycle pending and after-spawn cases with an unrelated command | `…162819Z`, `…163235Z` | — |
| R3-F6 attribution | Empty, inactive or unreadable samples counted as consistent | `GhaeleAttribution.cs` | Positive-only predicate with body-material identity; inconclusive classes | Domain negatives; native 11/11 positive; re-evaluated retained rows | `…162819Z`, `…163235Z`, `ghaele-attribution` | Retained round-3 rows lack body identity (labelled) |

Earlier findings: R1-F2, F4, F6, F7, F8, F10, F11, F12, F13, F14, R2-F1 and R2-F3 were verified in rounds 2–3. Their
evidence is retained; the round-4 runs re-exercise R1-F7/F8/F11/F12/F14 and R2-F3 on the final candidates.
