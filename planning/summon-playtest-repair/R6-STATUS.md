# Summoned Creature Manual Playtest Repair — R6 status

## R6 complete for review (2026-11-08, candidate `2f1c4eb9f90f`)

**All required R6 technical gaps are closed.** Owner viewing and listening remain
separate and are not implied by this result. PRES-08 stays deferred. The earlier
sections and their verdicts are kept and stay attached to their own candidates.

### Cue retirement repair (PRES-05, PRES-06)

- **Failure reproduced earlier:** on `c31c6292f7d3` a Trumpet cue (ID 861) ended
  4.62 s after its view retired, because the event-and-emitter stop found the
  emitter already unregistered.
- **Verified contract:** the installed integration exposes
  `AkSoundEngine.StopPlayingID(uint)` (with transition-duration and fade-curve
  overloads), returns `void`, and needs no game object. See
  `docs/FIREARM-WWISE-EXACT-RUNTIME-CONTRACT.md`.
- **Change:** `SummonCastCueLifecycle.Retire` stops each of this feature's own
  unfinished playbacks by playing ID, one attempt per playback, independently, and
  records "stop-requested" (never a success code). Only the native end-of-event
  callback marks completion. No global stop, duplicate registration or re-registered
  emitter.
- **Native result (Trumpet Archon only):** a real Trumpet Blast, command Success,
  playing ID 504 still active at retirement; stop requested 0.03 s after retirement;
  native end 0.07 s after retirement, 0.56 s into the cue. Natural-completion controls
  on the same candidate: Trumpet 3.26 s, Vrock 2.60 s, neither stopped. A cast
  cancelled before execution posted nothing. One cue per execution (7 targets each).
- **Limit:** early retirement was not separately executed for the Vrock. It shares
  the implementation. Native event lifetime is not audibility.

### Lantern ray: qualified re-reading of `c31c6292f7d3`

`r6-lantern-paired-views` accepted `ray.finished` alone. Its immutable record for
both passes shows `result` Success, `cancelled` false, `executionEnded` true,
`actedAtShot` 1, and act-state frames (12 and 15). That meets the corrected
predicate (`AbilityExecutionObserved`). The original verdict stands, this reading
stays with that candidate, and no Lantern cell ran on `2f1c4eb9f90f`.

### Native itinerary and result (`2f1c4eb9f90f`)

- Selected cells: `config`, `redcap-scythe`, `audio`. Hound Aid, Lantern and the
  mixed group were not replayed. Commands were observed passively.
- **Run** `summon-playtest-r6-presentation`: **PASS, all assertions**, wrapper exit 0.
- **Redcap clip** (optional): moved to open ground. I inspected extracted frames:
  no silhouettes; scythe swings (frames 675-728 span the real roll at 698) and the
  kick (roll at 792) are clear. `redcap-scythe.webm` under
  `runtime-evidence/summon-playtest-r6/final-2f1c4eb9f90f/clips`.
- Device audio was recorded but not used for the retirement claim. Earlier WAV
  excerpts document the unchanged cue choices only.

### Where each result lives

Product code for Hound sword, Aid, Redcap mapping, Lantern and sheen did not change
between `c31c6292f7d3` and `2f1c4eb9f90f` (only the cue playback, a policy file and
the observer did). Their native results stay attached to the candidates that ran
them: Hound sword on `0dd811be26b2`; Aid, Lantern and sheen on `c31c6292f7d3`.

### Identities

- **Source / candidate:** `2f1c4eb9f90fef5193d05b9dfeb888ae35cd1b23`, v0.0.179
  development `summon-playtest-r6`.
- **Package** `cae8e66ba18ef1496e73c94b6c38da9cd2f0d8497e36b817724c43e0c6d1e262`.
- **DLL** `b994faa751e64e28d365ba07488db33e1c7838e2389d112bdb66f6b28621ee4e`,
  MVID `0d73c16b-78a6-459a-9caf-f8751a0c0420`; the observer is in that DLL.
- **Focused:** `summon-playtest` 39 of 2,882 selected, passing, production compiled;
  new behavior cases for retirement, observer retirement and Lantern execution.
- **Incidental R5 state:** provider, Mist and Dance `active`. The 22/0 stays with
  `366fad0ac41b`.

### Restoration

Intake v0.0.140, DLL `00EF6757…66FD`, 16 of 16 mods, `Params.xml` `E44F4495…30BB`,
95 saves, 66 settings, no lock. After the run: live tree `5C3E4AD4…BFBB` (254 files),
95/95 saves identical, settings restored, `Params.xml` unchanged, no lock, no foreign
Kingmaker during the run.

### Ledgers

The earlier repair total of 688 s entered 16 s for the last gate; it took 19 s, so
it is **691 s**. The owner raised the repair ceiling from 1,200 s to 1,500 s for
this checkpoint.

| Repair step | Seconds |
| --- | ---: |
| Earlier repair checkpoint (reconciled) | 691 |
| Focused gates (one compile refusal, one pass) | 24 |
| Candidate build | 422 |
| Guarded native run | 161 |
| **Repair total** | **about 1,298 of 1,500** |

Original R6 ledger stays about 1,498 s. **Cumulative R6: about 2,796 s.** R5 stays
about 1,389 s.

### Owner viewing and listening

1. `hound-sword` (first R6 clips): does the sword read as striking?
2. `hound-aid-self`, `hound-aid-ally`: is the gesture acceptable?
3. `redcap-scythe` (final clips): does the scythe attack look right?
4. `lantern-donor-hidden` against `lantern-donor-shown`: is the protrusion gone, and
   are the small pale flecks acceptable?
5. `mixed-group`: is the summon sheen consistent?
6. The Trumpet and Vrock WAVs: do they fit? `InspireCourage` is provisional.

---

## R6 repair checkpoint (2026-11-08, candidate `c31c6292f7d3`): review checkpoint

One candidate and one guarded run under a separate repair allowance. **Native
result: FAIL by one assertion** (`r6-cue-early-retirement`), a real product
finding described under PRES-05/06. All other assertions passed. The original
R6 evidence for candidate `0dd811be26b2` is kept below and stays attached to
those bytes. After the run, one code comment was corrected to state the observed
limit (no behavior change, not rebuilt).

### Disposition by item (repair candidate)

- **PRES-01 Hound sword:** unchanged from `0dd811be26b2`, where it was
  established. Not rechecked; no shared path it depends on changed.
- **PRES-02 Hound Aid — technically established.** Native `Touch` is rewritten to
  `SelfTouch` for a self target and this rig has no such clip. The Hound's Aid now
  uses `Omni`, which the engine does not rewrite. Commanded on an ally and on
  itself: `Omni_casting` plays during the actual casting interval on each, one
  application each, command Success. Action stays Standard, matching the native
  Aid blueprint. Real-time cooldown read 0 before and after (out of combat), so
  native action consumption itself was not measured.
- **PRES-03 Redcap scythe — technically established.** The unit's real style is
  `AxeTwoHanded`, which its native main-hand action does not map. A private
  per-view action and set add that style with the existing scythe clips (Stalker
  ownership and restoration pattern). Binding is gated on the summoned-unit
  buff's real context: a `KMG_Summoning_` ability that actually spawns
  `RedcapSummoned`. A summon whose saved context lacks that keeps native behavior;
  other Redcaps and the shared scythe are untouched. Observed:
  `Attack_with_scythe_02` on the private action spans frames 1059-1115 and the
  `StandardScythe` roll is at 1077; `RedcapKick1d4` roll at 1176 with
  `LegAttack_01`; two attacks. Reload behavior was reasoned from the view-attach
  path, not exercised.
- **PRES-04 Lantern — supported by paired views.** Same live Lantern, donor body
  shown then hidden, idle/move/real ray, orb framed fully (0 out of frame). A dark
  blue mass protrudes from the orb in donor-shown frames and is absent in
  donor-hidden frames, so the mesh suppression is kept. Small pale flecks of light
  at the orb edge appear in both passes; they are not removed and their source
  was not isolated from nearby effects. Intrinsic light, orb and ray are intact.
- **PRES-07 Sheen — technically established.** Monster-family arrival rims were
  the persistent rim. They now end after 3 s on KMG summons, leaving the arrival
  cue and the buff. Seven-unit group (Ally and Monster families, native Redcap,
  fire Mephit, Shadow Demon, Invisible Stalker): 0 generic rims active; the rim was
  ended on each Monster-family unit and finished naturally on Ally-family units;
  `MephitFireVisualBuff` stays active and `NaturalInvisibilityBuff` remains. The
  Shadow Demon and Stalker keep their transparent look in the group frame.
- **PRES-05 Vrock / PRES-06 Trumpet — execution complete; retirement open.** Real
  executions post one accepted cue each (reaching 12 and 15 targets), with native
  end-of-event at 4.25 s and 4.77 s and no stop. A cast cancelled before execution
  posts nothing. **Early retirement fails:** a Trumpet whose view is destroyed
  while its cue plays (playing ID 861) ends 4.62 s later (5.15 s total, natural
  control 4.77 s). The stop reports `not-registered`: the native `AkGameObj` is
  destroyed before this view's handler, and its unregistration does not end the
  sound. `InspireCourage` remains a provisional substitute: the installed Wwise
  event and source-file names were searched for horn, trumpet, bugle, fanfare and
  brass, and the only match is the music file `PF_LK_HornedHunter`.
- **PRES-08:** deferred.

### Clips and what was inspected

Private, under `runtime-evidence/summon-playtest-r6/repair-c31c6292f7d3/clips`:
`hound-aid-ally`, `hound-aid-self`, `redcap-scythe`, `lantern-donor-shown`,
`lantern-donor-hidden`, `mixed-group` (WebM). I inspected extracted frames from
each, not continuous playback. Audio was recorded but only native cue records
were analysed for retirement; WASAPI UTC offsets are approximate and audio and
video are separate. The agent cannot judge audibility.

- **Capture limits:** in `redcap-scythe` a tree that has no collider puts both
  units in occlusion silhouette in most frames; the scythe swing is still visible.
  The `mixed-group` view is clear but wide, so units are small.

### Identities and results

- **Source / candidate:** `c31c6292f7d361c5be7eb460c808f0e55159184f`, v0.0.179
  development `summon-playtest-r6`. Package
  `e3d0bf672fefb3a144353b32f5352e794eab7dc06130314761f1424c5f3c242a`, DLL
  `33fd900a0e9ccb61940bc240f06a49073f5420be434d3400ce73e6a7e7d63ab4`, MVID
  `52d925bf-f908-4c5c-b393-c7d9a844489d`; the observer is in that DLL.
- **Native run** `20261108T1832463736030Z-7e98916caaa34b088e4bbe42f55e2062`. The
  observer no longer ticks abilities. Incidental R5 state: provider, Mist and Dance
  `active`; the 22/0 stays with `366fad0ac41b`.
- **Focused:** `summon-playtest` 36 of 2,879 selected, passing, production
  compiled; the candidate builder selected the same 36. UMM tests 85 assertions
  pass, and the real `Get-Process` cmdlet is back afterward.
- **Intake and restoration:** v0.0.140, DLL `00EF6757…66FD`; 16 of 16 mods;
  `Params.xml` `E44F4495…30BB`; 95 saves; 66 settings. Restored: live tree
  `5C3E4AD4…BFBB` (254 files), 95/95 saves identical, settings restored,
  `Params.xml` unchanged, no lock, no foreign Kingmaker during the run.

### Repair ledger (target 900 s, ceiling 1,200 s)

| Step | Seconds |
| --- | ---: |
| Focused gates (including one compile refusal, one test correction, comment check) | 72 + 19 |
| Candidate build | 398 |
| Guarded native run | 202 |
| **Repair total** | **about 691** (reconciled; first recorded as 688) |

Original R6 ledger stays about 1,498 s; combined R6 was then about 2,189 s. R5 stays about
1,389 s. A further build and run (about 600 s) did not fit the 1,200 s ceiling then in force.

### Owner viewing and listening

1. `hound-aid-self` and `hound-aid-ally`: is the cast gesture acceptable?
2. `redcap-scythe`: does the swing read as a scythe attack (tree-silhouetted)?
3. `lantern-donor-hidden` against `lantern-donor-shown`: is the protrusion gone,
   and are the small pale flecks acceptable?
4. `mixed-group`: is the summon sheen now consistent?
5. Earlier WAV excerpts: do the Trumpet and Vrock cues fit? Retirement remains a
   separate technical gap.

---

# R6 status as first reviewed (candidate `0dd811be26b2`)

**R6 checkpoint (2026-11-08): partly complete; review checkpoint.** One
candidate was built and observed once. PRES-01 is technically established,
PRES-05 and PRES-06 have working one-shot cues awaiting a listener, and
PRES-02, PRES-03, PRES-04 and PRES-07 have concrete open gaps. PRES-08 is
deferred. The R6 ledger cannot fit a second candidate, so the gaps are recorded
rather than rebuilt. No merge, release or permanent install. R7 has not started.

## Branch and base

`codex/summon-playtest-r6-presentation`, on R5 head `731b41933d` (neither R4 nor
R5 is in master). Targets `codex/summon-playtest-r5-spells`, so it depends on
draft #115, which depends on draft #114.

## Intake (DATA, recorded before any change)

- Installed: v0.0.140, DLL `00EF6757…66FD`, MVID `842d73db-a49f-459f-9b47-ca80cf0f273a`.
- `FeatureModules.json` `6E24B278…85D0`; UMM profile 16 of 16 mods enabled.
- UMM `Params.xml` `E44F4495…30BB`, 3,077 bytes; no pending UMM receipts.
- 95 save files (1 `KMG_AUTOMATION_BASELINE`); 66 settings values.
- Reference manifest `7F1EDE2B…1C04`; coordination lock absent.

## Capture method

- **Audio:** WASAPI shared-mode loopback of the default output device, through
  the new single-purpose `scripts/Capture-LoopbackAudioEvidence.ps1` (Windows and
  .NET only). It records the whole device mix, not one process.
- **Motion:** the observer renders the game camera to 960x540 JPEG frames under
  `Time.captureFramerate = 30`, with native animation state per frame. The
  Playwright-bundled ffmpeg (`ms-playwright/ffmpeg-1011`) turns them into VP8 WebM.
  That build has no screen grab or audio encoder, so audio and video are separate.
- **Proof before the build (no game):** a loopback recording of a stock Windows
  sound, and 30 test frames encoded and decoded back. Both worked.
- **Listening limit:** the agent cannot hear audio. It measured level and timing.
  Whether a cue is audible, fitting or pleasant needs the owner's ears. "Watched"
  below means inspecting extracted frames.

## Disposition by item

### PRES-01 Hound sword — technically established (commanded)

- **Cause:** this free-hands rig maps only `ThrownStraight` for main-hand attacks,
  so a greatsword hit with no swing.
- **Change:** `BindBarbazuNativeHand34(unit, sword)`, the existing Barbazu mapping,
  on the Hound's private sword only.
- **Evidence:** real `UnitAttack`; `Attack_01_3` plays with the native act event.
  Rolls: greatsword, greatsword, bite (same weapons as before; values unchanged).
- **Clip:** `hound-sword.webm`. Its first swing overlaps arrival effects and frames
  are blurred by depth of field; the second swing (shots 79-133) is clean.

### PRES-02 Hound Aid — open

- `BindHoundAidCastR6` keeps a playable style, and the native `Touch` was already
  playable, so **the build did not change Aid's style**.
- **Observed:** commanded Aid on another creature plays `Touching`, applies once,
  with the native effect (`hound-aid.webm`).
- **Gap:** native code rewrites `Touch` to `SelfTouch` when the target is the
  caster, and this rig has no `SelfTouch` entry (Touch, Directional, Point, Self,
  Omni, Potion, Special). Self-targeted Aid therefore has no gesture by code
  reading; this is the likely owner-visible defect. Not observed natively.

### PRES-03 Redcap scythe — defect confirmed, not fixed

- **Inspected:** the summon is native `RedcapSummoned`, `StandardScythe` plus
  `RedcapKick1d4`; prefab has `RedCap_Weapon` and `Attack_with_scythe_01/02`.
- **Observed (observer FAIL `r6-redcap-scythe`):** the scythe attack's
  `MainHandAttack` handle ends within two frames with no clip while its roll still
  lands; only the kick animates (`LegAttack_01`). The scythe is visibly held.
- **Next:** bind a compatible style on a private per-view animation set for this
  unit (the Stalker pattern), never on native `StandardScythe`.

### PRES-04 Lantern protrusion — change made, acceptance open

- **Change:** the donor `will_o_wisp1` skinned mesh is swapped, per view, for a
  private empty mesh. Native fading stays authoritative; Release restores it.
- **Observed:** donor body replaced at spawn and after the sequence; orb ready;
  light on; ray finishes. Non-owned renderers: only that mesh and `AttackLine`.
- **Gap:** the camera crops the orb (`lantern-donor-body-shown.webm`,
  `lantern-idle-move-cast.webm`), so the protrusion was not seen before or after.
  The diagnosis remains unproven, and blue light near the orb in some frames is
  unexplained.

### PRES-05 Vrock and PRES-06 Trumpet — cues work; listening pending

- **Change:** `SummonAbilityCastCue` uses the native once-per-cast hook (one cue
  per execution context) on those two abilities only, posting on the caster's view.
- **Events:** Vrock `EarPiercingScream`; Trumpet `InspireCourage`, both native
  one-shots in the VFX bank. A search of installed Wwise event and source-file names
  found no horn or trumpet effect; this is a provisional bardic substitute.
- **Observed:** Trumpet reached 14 targets and Vrock 13, one accepted cue each; a
  cast interrupted before execution posted no cue and applied nothing.
- **Measured:** about -16 to -14 dBFS against a -35.6 dBFS baseline, starting at
  the logged cue time (Trumpet about 1.5 s loud, Vrock about 1 s).
- **Inconclusive:** despawning the Vrock 0.43 s into a cue left the same level
  shape as an undisturbed cue, so early release of the sound is not shown.
- **Excerpts:** `trumpet-blast-cue-InspireCourage.wav`,
  `vrock-screech-cue-EarPiercingScream.wav`, four `sample-*.wav` alternatives,
  `baseline-no-cue.wav`, `vrock-cue-with-despawn.wav`.

### PRES-07 Summon sheen — lead found, not fixed

- The group clip is unusable: units render as occluded blue silhouettes.
- **Census (7 units):** the known `SummonedCreatureVisual` is suppressed wherever
  present (0 active). But `SummonedCreatureSpawnMonsterIV-VI` shows its **rim still
  active** on Redcap, Hound, Shadow Demon and Stalker, while the Ally-family buffs
  show 0 active. The Hound was minutes old. This fits the reported inconsistency:
  the rim persists by summon family. Not visually confirmed, no product change.
- Preserved in the census: `MephitFireVisualBuff` and `NaturalInvisibilityBuff`.

### PRES-08 Ankylosaurus sound — deferred

Optional; deferred to protect the required items.

## Shared paths changed

Hound sword and Aid blueprints, Vrock screech and Trumpet blast components,
`LanternArchonOrbView`, a new observer phase, and test and capture scripts. No
weapon values, targets, costs, frequency or AI. Commanded only; nothing observes
autonomous casting.

## Identities and native result

- **Source / candidate:** `0dd811be26b2c4659c4caff901406bbe1d691ffc`; v0.0.179
  development `summon-playtest-r6`.
- **Package** `3bcd5f2a7f3e56a08fe6865c79ca50247ac74494a32d354c03a21844dba6c2e5`.
- **DLL** `cccf608ba075f51a8d303edde873462e5015ac6833a170f697de6e33fcf5176e`,
  MVID `7ca130ae-be90-445e-aada-b12225c43706`; the observer is compiled into it.
- **Native phase** `summon-playtest-r6-presentation`, run
  `20261108T1740383398021Z-23cf05021f0a4c8fa24e998f715b4436`: overall **FAIL by one
  assertion** (`r6-redcap-scythe`). All other assertions passed. No retry.
- **Incidental R5 state on this candidate:** provider `active` (CotW 1.14.4c-2.1),
  Mist `active`, Dance `active`. This does not transfer the 22/0 from `366fad0ac41b`.
- **Focused:** `summon-playtest` filter 31 of 2,874 selected, passing, production
  compiled; candidate builder selected the same 31. UMM tests 85 assertions pass.

## Restoration (exact intake)

Live tree restored to `5C3E4AD4…BFBB`; installed version 0.0.140; 95 of 95 saves
byte-identical; settings restored; UMM `Params.xml` unchanged `E44F4495…30BB`
(receipt verified); no coordination lock remains. No other Kingmaker process
appeared during the run. Evidence is private under
`runtime-evidence/summon-playtest-r6`.

## Correction to R5 and concurrent-lab note

The UMM test added at `307cf89356` left its `Get-Process` stand-in behind in a
shared PowerShell session (`-LiteralPath function:global:...` removes nothing).
Its child fixture also saw real Kingmaker processes. That broke later checks in
`validate-repository.ps1` and made candidate builds fail. Both are fixed here
(`22a632a9e4`, `0dd811be26`). The R5 UMM results were standalone and remain valid
for what they tested.

`KingmakerSkipCinematicLab`, another session on DATA, runs its own game copies.
Guarded launches refuse while any Kingmaker runs, and that lab's game also
contaminated the first audio proof. It held launches for two agreed windows.

## R6 ledger (target 900 s, ceiling 1,800 s)

| Step | Seconds |
| --- | ---: |
| Capture proof | 5 |
| Focused gates (including two compile refusals) | 51 |
| UMM reruns and refusal probes | 20 |
| Failed candidate builds (39, 252, 247, 236; own test defects) | 774 |
| Successful candidate build | 428 |
| Guarded native run | 220 |
| **R6 total** | **about 1,498** |

About 302 s remain: not enough for another build and run (about 650 s). R5 stays
at about 1,389 s. Over the 900 s target; under the ceiling.

## Owner viewing and listening checklist

Files are in `runtime-evidence/summon-playtest-r6/clips`.

1. `hound-sword.webm`: does the sword visibly swing and connect (second swing)?
2. `hound-aid.webm`: is the cast gesture acceptable on another creature?
3. `trumpet-blast-cue-InspireCourage.wav` and the `sample-*` files: is any close
   enough to a horn, or should licensed audio be sourced?
4. `vrock-screech-cue-EarPiercingScream.wav`: does it fit a Vrock's screech?
5. In play: Lantern close-up for the blue protrusion; a Summon Monster group for
   lingering shine against a Summon Nature's Ally group.
