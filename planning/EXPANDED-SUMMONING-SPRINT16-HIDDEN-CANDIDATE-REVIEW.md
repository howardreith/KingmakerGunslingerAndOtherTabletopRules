# Sprint 16 hidden candidate review

Historical artifact evidence only. Current finalization authority and chosen
repairs are in `EXPANDED-SUMMONING-PHASE2B-FINALIZATION-RECONCILIATION.md`.
Earlier proposed fixture/reset strategies below are not all retained: preserve
the runtime-derived all-owned load reset, native destruction and sheet closure.
No save copying, parsing or replacement is permitted by the current mission.

## Historical local candidate 390f9a39 — FAIL; live install exactly restored

Source `390f9a39d9cce2a40e6528d3215a678d6cd15757`, version `0.0.141`.
Complete repository wrapper, **2018/2018** domain tests, clean exact-reference
Release and strict package validation PASS. Runtime remains NOT QUALIFIED.

| Artifact | SHA-256 / identity |
| --- | --- |
| Source state | `c0679e04ecdf32a077bf0035d97453bdd98951a7dbe28713df8cc45cbfa5bb50` |
| Runtime ZIP | `10ee8add096fce0f2db9aa283f0c90d681d041f2ed0fefbdeb159404636df765` |
| DLL | `6657d8e04ff280c012d088418b3649149a77b57f7ba6efebe45ee7eff94c365b` |
| DLL MVID | `97ac598a-0e94-4698-82d0-af386c28a572` |

All five runs used the same immutable artifact, guarded Steam App 640820,
and only `KMG_AUTOMATION_WORKING`. Relative evidence paths below are under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/`.

| Run directory | PASS / FAIL | Result SHA-256 |
| --- | --- | --- |
| `20261005T1613259240418Z-disposable-expanded-summoning-crocodilians` | 145 / 60 | `FCAC2474068C443A4B11DD4EC91E2287D70E754EDC69371874C336D8FF569DA9` |
| `20261005T1623589391793Z-working-save-expanded-summoning-creature-review` | 20 / 0 | `ADE00CE4EA6D310FB4A5FBFF94BEF1A99980A6D55B67809FD8B1977B9E08D7D2` |
| `20261005T1631322806965Z-working-save-expanded-summoning-prepare` | 9 / 0 | `57475921CD8D6B0E90F377601212DEC2A5BFD73D01580DE590E646D5BADD1AFA` |
| `20261005T1635049648442Z-working-save-expanded-summoning-verify-cleanup` | 6 / 3 | `8E4AC45C91EC9EA700D031C7FD6AC7E04CB9C14A2AB149A464B4CF8032229688` |
| `20261005T1638056052795Z-working-save-expanded-summoning-verify-absent` | 4 / 1 | `DF29E48C8CB44AF24FFAD6271723E4C30058BFA1D292B4DCD4D3EFBF5AB57D41` |

Passing partial evidence: repeated copies' exact ranks/HP/Strength bonus;
prone immunity; swallowed later-round cadence/native break-free; Sprint speed
interactions/timeline; nine actual native UI emblem bindings; all fourteen
published Crocodile and six private Dire roots; original movement/crowd review;
owned per-view destruction; original-party area preservation. Purple Worm and
Flytrap component graphs and native Monitor Lizard negative control remained
unchanged. This does not override failed contact, lifecycle or reload gates.

The fixture's Animal Growth PASS was a false positive: it compared changed dice
only with the static contract, not the actual pre-buff bite. Both wrong dice
lines stayed unchanged. Do not cite that assertion as size-modification proof.

### Diagnoses and containing correction checkpoint (NOT QUALIFIED)

- NPC `ItemEntityWeapon.Size` retains a fixed non-Medium blueprint size
  (`Assembly-CSharp` IL 1273490–1273523). Scaling relative to that item wrongly
  changed the printed bites to `1d6+4` / `2d6+13`. Resolve these exact owned
  natural weapons from live creature size plus the native rule's item-size
  shift, clamped to Fine–Colossal. Retain existing dice overrides. Add behavioral
  tests, size metadata and an actual before/after growth oracle (`2d6` / `4d6`).
- Guard a crocodilian maintain against dead/missing participants before any
  check or rider. Lethal evidence now separates before-life/after-life damage
  events and exercises a dead replay. The unexplained second damage event is
  not waived; it must disappear or be positively attributed.
- Native `BuffCollection.Tick` uses turn-start time and current unit/caster in
  TB (IL 437678–437707). Final UI/lifecycle cases now explicitly run in RTWP,
  restoring mode/pause/clock afterward. All failed expiry cells need rerun.
- Failed visual attachment lost a material-controller clone during rollback.
  Capture and destroy the crocodilian-only adopted material before reverting;
  donor/cache objects are excluded. Wait for actual native visible frames in
  fallback instead of sampling immediately disabled spawn renderers.
- Native UI restoration now records every predicate and allows window-close
  updates. Target-death lifecycle records each part, buff and immobilization
  predicate. No contact tolerance or lifecycle requirement is relaxed.
- Paused reload retained holder buffs and a Dire stored engulf link. Add a
  source-owned load reset for exactly Crocodile/Dire Crocodile, never link
  reconstruction. An idempotence drill preserves a real Purple Worm hold.
  Fresh-load proof is still mandatory. The persistence fixture also removes
  disposable appearance buffs before Sprint arming and distinguishes disposed
  cache references from attached/live units during cleanup.
- Combat remains open: a deliberately unavailable manual cast stayed queued;
  AI-fresh casts were interrupted before one success; TB cells often exhausted
  their budget before a full sequence; one manual-swallow cell recorded only
  an attack of opportunity. Tail contacts missed by about 0.65 m. Next combat
  work needs exact command timing and native pose-window evidence, not a looser
  contact threshold or a forced animation contact.

Closed `crocodilianReview=mechanics|combat|lifecycle` diagnostic requests now
permit narrower runs. Omission still executes the full candidate scenario.
They require the exact working save and automatic exit, reject unknown/extra
parameters, and mark their result as diagnostic, never a complete Sprint PASS.
Next: qualify corrected mechanics plus targeted persistence, then address the
remaining combat/contact and lifecycle cells. Suppression is unchanged.

### Exact machine and working-save disposition

Snapshot `runtime-backups/live-mod/20261005T1609558177944Z`; deployment
`deployments/20261005T1613258392220Z/deployment.json`; restoration
`expanded-summoning-restoration/20261005T1641070919903Z-disposable-expanded-summoning-crocodilians.json`,
SHA-256 `82CB90A03E8678EB5F3F01B0F5AF8C980A2D54E553AF761108133C59C31C89DC`.
Independent post-run verification: 136 files / Info 0.0.117 /
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
zero Kingmaker processes, no compatibility lock, all six leases Completed.

Prepare made **one authorized working-save write**. Cleanup refused its write;
the final absence correctly failed because the prepared five summons remain
in that disposable save. Working archive SHA-256:
`A0DB560EB96A6FB140D73BB91ECB1485DA83C6B959A2A3EC6381CCB6D0E3FE9D`.
Protected baseline remains untouched, SHA-256:
`CC7CBB0D08581873ED0AD2A6AC8EBD16A95333B5665CD74DCD0C538E16119C07`.
The mod installation is exactly restored; **do not claim save cleanup or save
byte restoration**. The next targeted prepare safely disposes stale test
summons before recreating them, and a successful cleanup/absence pair remains
required. Raw evidence and save files stay machine-local.

`OwnerAcceptedEngineLimitation: SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`
and `OwnerAcceptedAdaptation: SWALLOW_ELIGIBLE_TARGET_ELSE_DEATH_ROLL` remain
recorded. Neither excuses these failures. `HumanReview: NOT_PERFORMED_NONBLOCKING`.

## Candidate 0238bf03 — FAIL, restored, NOT QUALIFIED

Source `0238bf03166dfb3dd7bf0205c12f82a2d2757caa`, version `0.0.141`.
Full repository wrapper, 2018/2018 domain tests, clean exact-reference Release,
and strict standalone/current-runtime package validation PASS. Package counts
312 with sound / 310 without. These checks did not establish runtime correctness.

| Artifact | SHA-256 / identity |
| --- | --- |
| Source state | `b7fd84b9605dd00d47b1ced84d03b720a74ec7fd08c0d5e8ff858cfe9add3431` |
| Runtime ZIP | `52786637dabfa8d563f92595080227522ee4b2de6a9a580f814d164339e97182` |
| DLL | `e8c528592bbea7e07306fb88b25b40d62b2ed1dd2a56c443522402daaa714934` |
| DLL MVID | `24e630dc-e3ba-4961-b96e-15214c68954c` |

One immutable artifact, one baseline snapshot/deployment, five guarded fresh
Steam App 640820 launches, and one exact final restoration. All used only
`KMG_AUTOMATION_WORKING`. The prepare and cleanup stages each refused their save
write; neither saved the incomplete fixture. The final absence PASS does not
qualify persistence because prepare failed.

Evidence directories are under `C:/Dev/KingmakerGunslingerLab/runtime-evidence/`.

| Run directory | Assertions PASS / FAIL | Result SHA-256 |
| --- | --- | --- |
| `20261005T1535350208461Z-disposable-expanded-summoning-crocodilians` | 55 / 19 | `1D76EC35BF80204428036242BE8199E41D68E36A4D490D9F4A538B7A16DBF4C3` |
| `20261005T1538431720124Z-working-save-expanded-summoning-creature-review` | 16 / 4 | `79736EEBE1D158EF5B5005BE042386394A54D9489CD6CE4175CC4F2276EC7D99` |
| `20261005T1546159564262Z-working-save-expanded-summoning-prepare` | 6 / 3 | `A5669B7725D3D5B86A8402229B1CE6116ECE6829040D32672255F67EF69AC4E3` |
| `20261005T1549174154097Z-working-save-expanded-summoning-verify-cleanup` | 5 / 4 | `8B5B7785E32A204379D34BBD6C88A0E5496DE64D83B4AE8A4A68B34301D26449` |
| `20261005T1552180746230Z-working-save-expanded-summoning-verify-absent` | 5 / 0 | `C07F57A8CF0D306541F225D60C188EF7E0A26F403E447E665C40996CF2709AEA` |

### What the failed artifact actually proved

- The first copy of each creature had the printed skills. Later copies did not.
- Sprint's 20→40→20, Haste ordering, Slow, penalties, caps, six-second active
  state and sixty-second cooldown/deadline cases passed. Real-command/AI proof
  was not reached.
- Three later swallowed rounds each delivered one owned `3d6+13` bludgeoning
  bundle; activation/deactivation did not duplicate it. Native six-second
  break-free timing/checks passed. Initial Swallow bite damage was wrong.
- The native type/graph census confirmed the already activated
  `OwnerAcceptedEngineLimitation: SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`.
  This is not an open engine blocker and does not excuse the failed initial bite.
- Base-component identity survived an energy component moved ahead of it.
  Actual bite/Death Roll DR, material, enhancement and weapon attribution agreed;
  no second attack was introduced. The baseline positive Strength damage was
  wrong, so the combined bonus assertions still failed.
- Crowd native movement, original mesh attachment, expiry and exact project-owned
  resource cleanup passed for both creatures. Separate guided single-subject
  movement assertions failed. Native UI, combat contacts and the twenty route
  checks were not reached in the first scenario.

Two crowd images were inspected only as optional art support. Indoor walls and
crowding obscure some anatomy. They do not establish mechanical correctness or
owner visual approval: `HumanReview: NOT_PERFORMED_NONBLOCKING`.

### Evidence-backed corrections prepared after restoration

1. `IHandleEntityComponent.OnEntityCreated` runs on a shared blueprint component.
   Its single `m_Applied` flag incorrectly skipped every later crocodilian. Remove
   shared mutable creation state; allocate each new unit's exact ranks, reject
   preallocated ranks, and leave deserialization alone. Both the mechanics and
   two-copy persistence observations exposed this defect.
2. Native HP generation gave 26/150, not 22/138. Initialize only these creatures'
   racial-dice base HP to 13/54 after their class levels, retaining the native
   live Constitution dependency. No global class/HP rule is changed.
3. Native weapon stats give a lone primary-hand natural bite 1.5× Strength even
   with a secondary additional tail. Actual bites were `1d8+6` / `3d6+19`, and
   Death Rolls `1d8+8` / `3d6+25`. Use the native multiplier-override seam on the
   exact crocodilian bite to restore ordinary 1× Strength. Death Roll still uses
   that live bite and adds only the positive extra half.
4. Native NPC overridden weapon dice skip size scaling. An exact-owner/weapon
   component uses the native size table relative to Large/Gargantuan for these
   printed bite/tail dice; other existing dice overrides remain respected.
5. Prone immunity masked an added condition until removal. Do not add Prone when
   native `HasConditionImmunity(Prone)` is true; the fixture checks both during
   immunity and after its removal.
6. The secondary tail's `ItemEntityWeapon.IsSecondary` is authoritative;
   `RuleCalculateWeaponStats.SecondaryWeapon` is a separate override flag, not
   the live weapon role. Keep real attack/damage checks, fix the observation.
7. A lethal damage rule is followed by the native life-controller update. The
   synchronous fixture now ticks only its exact disposable victim's native life
   controller and the production hold path before checking cleanup. It records
   whether death had already settled on damage return; it never forces death.
8. The correction fixture read a nonexistent `State.AllUnits` reflection member.
   Its empty snapshots allowed it to dispose original area units and falsely
   pass cleanup, preventing the next fixture from finding the party's area.
   Use native `State.Units.All`, require a nonempty census containing the party,
   destroy only explicitly owned actors, and verify every original HoldingState
   reference and non-destroyed unit. This was in-memory fixture damage only;
   that scenario never wrote the working save. Historical single-fixture cleanup
   claims are not evidence of area preservation; no completed creature is
   redesigned on that basis.
9. The visual request included crocodilian ground assertions but omitted them
   from the existing guided movement measurement. Add exactly these two subjects
   to its awake/unpaused connected-floor measurement.

Focused 216/216 (2018 registered) and incremental exact-reference Release compile
PASS after these corrections. Corrected runtime proof remains pending. The next
candidate must rerun the affected gates; no failed assertion is waived.

### Restoration and unchanged publication

Snapshot: `runtime-backups/live-mod/20261005T1532081508820Z`.
Deployment: `deployments/20261005T1535349205786Z/deployment.json`.
Restoration record:
`expanded-summoning-restoration/20261005T1555173824556Z-disposable-expanded-summoning-crocodilians.json`.
Record SHA-256: `CF71CB42FDF6F082112A9B718937E7AE1BE8C487C1272C87C010ABBAE854B6F5`.

Actual post-batch tree: **136 files, Info 0.0.117**,
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
No Kingmaker process or compatibility lock remained; all six batch/restore
leases were Completed. No permanent deployment or publication occurred.
Surface stays 976 registered / 970 published / 6 Dire withheld / 29 wrappers /
999 visible choices. Sprints 14–15 remain accepted. Sprint 17 has not begun.
