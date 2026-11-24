# Sprint 7 record: presentation and audio remediation (S7 T1–T7)

Status: **development candidate for review (2026-11-24); nothing is merged or released.** One draft PR (#219), one
agent-review enrollment (`kmg-codex-s7-visual-audio-remediation-20261123180106-d1e1`, foreground, single plan), branch
`codex/s7-visual-audio-remediation`. Mission: `docs/missions/sprint7/SPRINT7-AUTONOMOUS-MISSION.md` (verbatim,
sha256 `c232f135…f56f0`). Technical status, agent recommendations and owner acceptance are separate below; every visual
and listening judgment remains the owner's.

## 1. Starting point

- Sprint 6 was merged (PRs #198–#216) and released: stable **v0.0.195**, release commit `ef0e1fe15d` (PR #217), records
  PR #218. Post-release `origin/master` is **`35763430ac`**, the base of this branch.
- Published package `456d504d…9444`, DLL `3ef052ea…6a6b` (`docs/BUILD-AND-RELEASE.md`). The local release copy
  `worktrees/publish-master/artifacts/release/0.0.195/…sprint6.zip` has the published hash.
- Environment: disposable loader-compat game copy with Unity Mod Manager owner profile 0.33.0 and no other mods;
  harness `KmgLoaderCompatHarness.dll` `04c9d1d4…cfa4` (built from this branch, identical to the one used for the
  v0.0.195 published-bytes check); Blender 4.5.10 `8b15ef73…313c` with `PYTHONHASHSEED=0`.
- Carried forward unchanged and not part of this sprint's acceptance: Sprint 6 accepted scopes, REP-05/REP-06 BLOCKED,
  VER-02 open with G5 fixture-blocked, and the other retained limits in `docs/GUNSLINGER-SPRINT6-DISPOSITION.md`.

## 2. Outcome

| Story / record | Original problem | Result | Evidence | Technical status | Remaining owner decision |
|---|---|---|---|---|---|
| S7 T1 / REP-04 | October review: "increasing" Ghaele dissolve and a scripted move that did not start | **Attributed; no product change.** The value read (`_Dissolve` on the first skinned renderer, the Ghaele body) is driven by a native looping, non-cutout dissolve animation (delay 4.75 s, lifetime 5 s, `DissolveCutout` false) that every azata view runs, including a native donor spawned without any summon. It draws the coloured edge band and never removes the body. Ordinary movement starts and travels. | Candidate `c0870c24de`, sessions `…005329Z` `gh`/`ghd` PASS; §4.1 | Disposition with native evidence (bright and dim) | Accept the attribution and the no-change disposition |
| S7 T2 / VIS-01 | 48 zero-area wing-root triangles (and, found here, inside-out welded membranes) | **Repaired at the generator.** 0 zero-area, 0 repeated edges, all parts outward | `ed88ad994e`; decoded regression; native before/after `02-mod`/`03-modd` PASS | Implemented and verified | Visual acceptance of the wing roots |
| S7 T3 / VIS-02 | 19/12,924 normal corners at a waist pleat and left-hand fingers | **Repaired from the exact private input.** 16 → 0 opposing corners (welded audit 19 → 3, the 3 being two touching head parts) | `d864e7b01f`; decoded regression; native before/after | Implemented and verified | Visual acceptance of waist and fingers |
| S7 T4 / VIS-03 | Owned Trample/Overrun windows: "Target point within 0 ft. feet." and school "None" | **Scoped presentation override.** "Target point within twice this creature's speed."; no school | `2614ea3bd6`; released baseline vs candidate windows with native controls | Implemented and verified (desktop fact and HUD paths) | Wording acceptance; controller mode not rendered |
| S7 T5 / VIS-04 | No added impact sound on the Ankylosaurus tail strike | **One owned cue per resolved tail hit** (`Sweetener_Low_06_Quake`, provisional) | `90bc2ae7eb`; `…012247Z` `fx` PASS | Implemented and verified | Listening choice among five installed candidates |
| S7 T6 / VIS-05 | Dust Mephit breath shows the ice cone | **Owned per-instance dust recolour** of the Dust Mephit's own cone instance, restored on release | `c0870c24de`, `d136e7b560`; `…012247Z` `fx` PASS | Implemented and verified (mechanics unchanged) | Visual acceptance of the dust look (framing is distant) |
| S7 T7 / VIS-06 | Provisional `InspireCourage` cue on the Trumpet Blast | **Proposed disposition: keep `InspireCourage`** (no code change); six alternatives sampled | Baseline `r6` and candidate `r6` PASS; samples in `fx` | Disposition with evidence | Listening decision (keep or choose an alternative) |

## 3. Candidates, sessions and identities

| Candidate | Package / DLL SHA-256 | Session | Result |
|---|---|---|---|
| Released v0.0.195 (baseline) | `456d504d…9444` / `3ef052ea…6a6b` | `20261123T233302Z-owner-0.33.0-s7-baseline` (r3, r6, r7 bright, r7 dim) | PASS 4/4, 7 min 47 s |
| `b438bb9d53` | `177e6bf8…44c9` / `711f8ab7…791e` | `20261123T235421Z-owner-0.33.0-s7-cand` | FAIL, retained: outdoor phases lacked the grouped quiet anchor (engagement paused the summon); wrong native spell GUID in the UI control; Ghaele predicate read the native loop. 15 min 35 s |
| `566de0d0bf` | `179c2a44…787a` / `731d86a6…0669` | `20261124T002105Z-owner-0.33.0-s7-cand` | FAIL, retained: Ghaele body predicate still raw `_Dissolve`; Dust check passed on a component that recoloured nothing (weak predicate; the visible cone is the cast effect). 19 min 33 s |
| `c0870c24de` | `433a6bae…f05b` / `45793e82…783b` | `20261124T005329Z-owner-0.33.0-s7-cand` | ui, models bright, models dim, Ghaele bright, Ghaele dim, r6 PASS; fx FAIL (observer waited 3 s; the cast effect lives about 10 s). 18 min 46 s |
| **`d136e7b560`** (final product) | `f7770dac…36f1` / `3b0a7c3a…250b` | `20261124T012247Z-owner-0.33.0-s7-fx` (fx, r6) | **PASS 2/2**, 6 min 9 s |

Every session completed its transaction, applied and restored game settings and verified owner files; the human
installation was never touched. Product code differs between `c0870c24de` and `d136e7b560` only in
`DustMephitBreathPresentation.cs` and `MephitBreathPolicy.cs` (the subtractive-layer tone); the observer change is the
Dust release wait. UI, model and Ghaele evidence is therefore bound to `c0870c24de`, effects and R6 evidence to
`d136e7b560`. Builds: `Build-Local -DevelopmentPurpose s7-visual-audio -DevelopmentTestFilter sprint7-visual` with the
pinned reference bundle, 7 min 12 s to 7 min 55 s each, strict package validation passed, no deployment.

Raw evidence: `C:/Dev/KingmakerGunslingerLab/runtime-evidence/loader-compat/<session>/` and
`…/runtime-evidence/s7/` (build logs, loopback recordings and sidecars). Curated review set:
`…/runtime-evidence/s7/review/` (`INDEX.md` sha256 `4409d72f…0ab6`, 103 files: T1 frames, T2/T3 before/after stills,
details and motion, T4 before/after windows with controls, T6 dust and native-look frames, T5/T7 audio excerpts).

## 4. Per story

### 4.1 T1 / REP-04 Ghaele

Observation (candidate `c0870c24de`, both scenes): a Ghaele cast through its real published summon (native
`UnitUseAbility`, caster level 20, appearance buffs kept, unpaused native time), a Bralani sibling summon, the native
donor `AzataGhaelSummoned` spawned without any summon, and a second Ghaele 36 s into the first one's life.
- Each summon's `SummonedUnitBuff` lasts 120.0 game seconds; each is removed 122.0–122.3 s after it appeared (the
  native expiry fade).
- The body renderer `GhaeleAzata_Renderer_Character_Diffuse_Cutout` (also the first skinned renderer, which the
  October observer sampled) carries `DISSOLVE_ON` and a `_Dissolve` that sweeps 0 → 1 about every 5 s throughout life.
  The native `StandardMaterialController` holds one looping animation (delay 4.75 s, lifetime 5 s, `LoopAnimation`
  true, `DissolveCutout` false); native code writes `_DissolveCutout` from that flag. With the cutout off the shader
  draws the dissolve colour band and removes nothing.
- 163 of 172 samples of every rising renderer match the native donor at the same moment (the rest fall in the first
  appearance second); the sibling Bralani shows the same loop.
- Surface-removing (cutout) dissolve on the body stays 0.00 from appearance completion to expiry for both Ghaeles, the
  sibling and the donor. Frames at raw values 1.00 and 0.02–0.06 show the same intact body.
- Ordinary move: 3.98 m (bright), 3.97 m (dim); an ordinary attack on the fixture hostile acted (bright). The two
  Ghaeles and the donor hold distinct material instances.

Disposition: the historical "increasing dissolve" values (0.009, 0.04, 0.118, 0.156) are consistent with sampling this
native loop at arbitrary times; they did not measure a body disappearance. The "move did not start" came from the old
review path, which did not unpause or tick movement for the Ghaele (source inspection); ordinary movement works. No
product material, lifetime or despawn change is warranted. Limits: the October run's machine-local evidence was not
re-read; the native loop's authoring source (prefab or buff) is not identified beyond the controller and is present
without any KMG summon; the October "gold dissolve colour" matches the native band.

### 4.2 T2 / VIS-01 Nalfeshnee

See `assets-source/original-models/sprint39-nalfeshnee/SOURCE.md` (Sprint 7 section) for cause, repair, measurements
and hashes (`6ff85493…` mesh, `1e9f5303…` albedo). Native: the pinned R7 export `d34e6c19…` (the VIS-01 baseline, not
the older `1caa91e1`) is shown on the same live unit after the appearance settles (settled dissolve 0.0). Bright: front,
rear, both wing-root details and the rear wing roots, idle, a 3 m move, a turn of more than 45° and an attack, after and
before; dim: the same without the attack. All framed and completed (`s7-nalfeshnee-bright`, `-dim` PASS). Wings follow
the chest (`articulatedWings` false); no scale, reach, collision or gameplay change.

### 4.3 T3 / VIS-02 Salamander

See `assets-source/original-models/sprint17-serpents/SOURCE.md` (Sprint 7 section): exact private capture
`eb0d5eaa…` recovered at its named lab location and reproduced byte for byte; export `0837e796…`. Native: the
released `b5524a69…` shown on the same live unit; waist front and rear, both hands with the spear grip, idle, move,
turn and spear attack (bright), after and before (`s7-salamander-bright`, `-dim` PASS). The material initialization
fix, spear attacks, owned tail slap and traveling tail gait are unchanged (tail drivers, weights and the slap/rest
payloads are byte-identical in the export).

### 4.4 T4 / VIS-03 Trample/Overrun windows

Released baseline (`…233302Z` r3): both owned windows read "Target point within 0 ft. feet." and the header shows
"None". Candidate (`c0870c24de` ui, PASS 25/25): the Brachiosaurus Overrun and Trample and the Mastodon Trample, each
through the description window (Ability fact) and the HUD path (AbilityData), read "Target point within twice this
creature's speed." with no 0 ft text and no "None"; duration and saving-throw rows are as before. Controls rendered in
the same run: the native overrun (`1a3b471e…`, same DoubleMove point template) still reads "within 0 ft. feet" with
"None", and native Magic Missile keeps "Evocation" and its native target. Limit: the controller (gamepad) window was
not rendered; its tooltip section calls the same `DescriptionBuilder`, `AbilityHeader` and `GetTarget` (decompile).

### 4.5 T5 / VIS-04 Ankylosaurus impact cue

Final candidate (`…012247Z` fx): two resolved tail hits (`KMG_Summoning_Natural_Tail3d6`) produced exactly two
`Sweetener_Low_06_Quake` cues (playing IDs 100 and 163) that completed naturally in 1.56 s and 2.16 s; raised armor made
the strike miss with no cue; an attack interrupted before it resolved posted none; with two Ankylosaurs striking
together, retiring one stopped only its own playing cue (ID 301, `view-destroyed`, stop requested, native end 0.10 s
later) while the other's cue (ID 299) completed naturally in 1.70 s. Duplicate observation of one strike is not
natively induced here; the existing `ReferenceEventGate` stamp is covered by `expanded-summoning.sprint22-event-identity`
and the native count equality. Loopback levels (whole device mix): strikes in play peak −1.6 / RMS −23.6 dBFS against a
−42.4 dBFS floor. Recommendation: keep `Sweetener_Low_06_Quake`; samples of `Physical_Hit_03_Punch`, `Earthblast_Hit`,
`DustCritHeavy` and `MediumBluntNatural_Flesh` are in the review set.

### 4.6 T6 / VIS-05 Dust Mephit breath

Final candidate (`…012247Z` fx): three committed breaths at normal speed, each from a slightly shifted launch point.
Each spawned one `ColdCone15Feet00_Aoe` cast effect that the product recoloured (11 materials, 16 particle colours);
each dealt 4 slashing (1d4) to the hostile inside the forward 15-ft cone, which became Sickened on its failed save; the
caster standing behind as an ally was not affected; each recolour was restored when the native effect ended about 10 s
later. A breath interrupted before execution launched nothing; cast together with an Ice Mephit, only the Dust cone was
recoloured and the Ice cone kept native materials; a Dust Mephit removed while its cone was active left no recolour
behind; a later Ice cone, including on pooled instances, was native. Earlier failures retained (§3): recolouring the
projectile view did nothing, because the visible cone is the projectile's cast effect. Observation (not changed): native
`CreateView` spawns the cast effect only when the launch point differs from the previous one, so repeated breaths from
one exact point can show no cone for any mephit (source inspection; not induced here). Limits: the recolour is a
luminance-preserving hue change of the native effect (subtractive layers take the complementary tone); frames are
distant and the summoned creature partly occludes the cone, so readability is for the owner to judge.

### 4.7 T7 / VIS-06 Trumpet Archon

Recorded on the released baseline and re-run on the final candidate (`r6` PASS both, the shared cue helper having
changed): one `InspireCourage` cue (VFX bank, Wwise ID 1240241623) for a blast reaching 7 creatures, natural completion
3.27–3.30 s, none on a cancelled cast, early retirement stopped by playing ID; the Vrock neighbour's `EarPiercingScream`
completed naturally (2.46–2.60 s). Installed names were rechecked: no event matches horn, trumpet, bugle, fanfare, brass,
tuba, cornet or clarion. Samples for comparison: `InspireGreatness`, `CacophonousCall`, `AoE_Sonic_Large`,
`Sweetener_High_04_Celestial`, `HolyBomb`, `Heroism`. Recommendation: **keep `InspireCourage`** (the only musical,
instrument-like option; the others are sonic or holy bursts), with `InspireGreatness` as the alternative if a fuller
version is preferred. This is a proposed disposition, not a repair or an acceptance. Not executed: a save/load across a
playing cue; the cue posts only from the native on-cast hook and a destroyed view retires its cues (source inference).

## 5. Validation

Focused domain tests (`sprint7-visual.*`, 7 cases): VIS-01 and VIS-02 decoded-export regressions (each fails on its
baseline export: 48 zero-area triangles; 16 opposing corners), VIS-03 identities and text, VIS-04 qualification,
VIS-05 identity and tint. Neighbour selections run during development: `summon-playtest-r3`, `-r4`, `-r6`, `-r7`,
`sprint22-`, `sprint5-`, `salamander`, `native-admission`, `native-profile`. The ten existing Salamander authoring tests
pass on the exact capture. The required controller gate receipts are recorded by the controller on the reviewed
candidate.

## 6. Owner decisions pending

1. T1: accept the REP-04 attribution (native looping non-cutout dissolve; old review path) and the no-change disposition.
2. T2, T3: visual acceptance of the repaired Nalfeshnee wing roots and Salamander waist and fingers.
3. T4: the target wording "within twice this creature's speed".
4. T5: listening choice of the impact cue (recommended `Sweetener_Low_06_Quake`).
5. T6: visual acceptance of the dust recolour (or a request for a different look).
6. T7: keep `InspireCourage` (recommended) or choose an alternative.
7. Merge and release remain the owner's; this sprint authorizes neither.
