# Gunslinger Human-Review Defect Matrix

Status vocabulary: `reported`, `reproduced`, `root cause proven`, `implemented`,
`source-qualified`, `runtime-qualified`, `human-gated`, `blocked`, and
`accepted/frozen`.

| ID | Defect or acceptance | Intake status | Superseded evidence | Current hypothesis | Next action |
|---|---|---|---|---|---|
| P0 | New Tenebrous Depths save is unloadable | reported; release-blocking | `0.0.88` working-save and mod-load smoke did not cover the affected save or Focused Aim serialization matrix | Focused Aim timing is correlation only; exact exception and serialized boundary are unknown | Identify save unambiguously, hash/copy it, collect logs/catalog evidence, then audit exact transient/save surfaces |
| ACADEMAE | Toggle-on prepared Summon Monster I still uses full-round; no save/fatigue | root cause proven; implemented; source-qualified; runtime-qualified | synthetic 14/14 run manually paired command/rule and copied the prepared slot onto the selected variant | native selected variants retain the exact prepared slot only on their `ConvertedFrom` root; the outer `ParamSpellSlot` was null, so eligibility rejected `not-prepared` and native spending missed the slot | preserve for final regression and consolidated human command/UI confirmation |
| ACQUISITION | 30 targets remain clustered/questionable | reported; prior pacing conclusion rejected | 25/25 graph run proved reference shape, not organic pacing or normal lootability | several targets are concentrated, generic, puzzle/quest coupled, or power/theme mismatched | resolve richer target evidence and reduce named-area density |
| BTSL | Permanent equipment is on Xelliren instead of Honest Guy | root cause proven; implemented; source-qualified; runtime-qualified; human-gated | prior publication tests asserted identical rows on all four tables and did not test owner responsibility or appended-block ordering | central, Eastern, and spear publishers independently mirrored project rows into both merchant roles; the shared transaction only appended | preserve for final regression; human-check actual new/not-yet-materialized shops and visual ordering |
| ICONS | Firearm monograms and Rapid Reload remain aesthetically wrong | reported; human-gated | semantic identity/contact sheets did not match accepted Nodachi/native feat grammar | firearm art used a separate dark badge template | reuse actual Nodachi generator/template; create pale native-style Rapid Reload art |
| LONG-GUN-YAW | Musket and Blunderbuss point too far left | reported; human-gated | structural 63/63 rig run did not measure apparent yaw on dolls | small wrapper/import/equipment-offset yaw mismatch | compare recorded local forward vectors and apply smallest per-gun correction |
| SPEAR-ORIENTATION | Point faces backward and back pose is horizontal | reported; human-gated | geometry run proved length/axis magnitude, not forward direction or carry pose | active mesh frame and holstered frame need independent correction | establish native spear active/back frames and repair separately |
| FOCUSED-AIM | Charisma damage, one Grit spend, kill recovery | accepted/frozen | none | preserve unless exact P0 evidence implicates a representation | run save-safety and gameplay regressions |
| PISTOL-AUDIO | Pistol shot is audible | accepted/frozen | none | preserve Wwise route | run routing/package regression; do not claim other families audible |
| SPEAR-LENGTH | Revised spear length is improved | accepted/frozen | none | preserve geometry length | change only orientation/carry frames |
| LONG-GUN-RIG | Model/texture/scale/support-hand work improved | accepted/frozen | none | preserve rig; yaw-only refinement | constrain asset diff and runtime assertions |

## Intake checkpoint

- Branch, HEAD, local ref, and refreshed remote ref:
  `e2e3d9ec941549a889a1e03a590e24241b745b7f`.
- Expected ancestor check: PASS.
- Worktree and Git-lock check: clean.
- Kingmaker process: absent.
- Live Mods compatibility sentinel and transaction sibling directories: absent.
- Compatibility state root active-status search: no matches; only fixture-test data exists.
- Repository validation: PASS.
- Complete dependency-free domain/reflection suite: 1,160/1,160 PASS.
# Current checkpoint override - 2026-08-20

| Defect | Reported | Reproduced | Root cause proven | Implemented | Source-qualified | Runtime-qualified | Human-gated | Blocked |
|---|---|---|---|---|---|---|---|---|
| P0 unloadable Tenebrous save | yes | yes, original stack and affected copy | yes, null Focused Aim FX links at view reconstruction | yes, stable-ID empty-link compatibility repair and guarded copy runner | yes, 1161/1161 plus full build/package gates | no, old exception removed but Tenebrous after-load callback stalls for affected and no-marker control | yes | release qualification only; independent corrections continue |
| Acadamae real prepared player path | yes | yes, selected Summon Monster I variant with null outer slot | yes, exact prepared slot remained on `ConvertedFrom` root and was never rebound | yes, exact-chain slot resolution/binding plus bounded rejection trace and native-command fixture | yes, 1161/1161 plus full build/package gates | yes, two fresh 15/15 PASS runs | yes, final visible command/UI confirmation only | no |
| Thirty-item acquisition pacing | yes | yes, prior normalized named-area maximum was 6 and several targets were generic/quest-coupled | yes, publication optimized target count rather than campaign pacing or ordinary accessibility | yes, all 30 exact routes rebalanced with retired-row cleanup and transactional rollback | yes, 1161/1161 plus full build/package gates | yes, run `20260820T1425182231173Z-observe-rare-firearm-acquisition` proved 30 items/30 targets, maximum density 2, one loot row each, and zero vendor rows | yes, ordinary interaction, theme, power, and discoverability | no |
| BTSL Honest Guy/Xelliren responsibility | yes | yes, all four tables received the same central/Eastern/spear rows | yes, three independent publishers had symmetric desired sets and the shared transaction concatenated additions | yes, Honest Guy gets permanent equipment; Xelliren gets six support stacks; exact wrong-owner cleanup and stable-key integration preserve retained-row order | yes, 1162/1162 plus clean build/package gates | yes, run `20260820T1444126864934Z-observe-rare-firearm-acquisition` | yes, actual new/not-yet-materialized shop visibility and list aesthetics | no |
