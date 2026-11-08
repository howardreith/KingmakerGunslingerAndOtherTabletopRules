# Summoned Creature Manual Playtest Repair — R6 status

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
  one-shots in the VFX bank. **Kingmaker has no horn or trumpet effect**; this is
  a bardic substitute whose fit is the owner's decision.
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
