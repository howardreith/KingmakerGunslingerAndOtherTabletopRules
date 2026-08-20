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
| SPEAR-ORIENTATION | Point faces backward and back pose is horizontal | root cause proven; implemented; source-qualified; runtime-qualified; human-gated | geometry run proved length/axis magnitude, not forward direction or carry pose | the held wrapper mapped source `+Z` to installed `+Y`, but the installed Longspear forward contract is `-Y`; the null donor belt model also caused reuse of the held model for carry | preserve the accepted 2.28 m source geometry; use `+90 X` held mapping plus a distinct upper-left diagonal BeltModel; human-check equipped attack phases and carried pose on representative bodies |
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

## 2026-08-20 firearm feat icon correction checkpoint

- Status: implemented; source qualification and guarded UI observation pending.
- Human rejection: the 0.0.88 dark circular firearm monograms and Rapid Reload medallion are superseded.
- Root boundary: the accepted Nodachi parameter appearance is produced by CustomWeaponSelectorRuntime through FeatureUIData with a null sprite plus the NO monogram; nodachi.png is item art, not the parameter template.
- Repair: retained every stable firearm choice blueprint and exact publication mapping, replaced only the six project-owned rendered assets with a deterministic reconstruction of the native selector grammar, and added a separate pale-field oxblood reload glyph.
- Source/provenance: JSON palette and monograms plus PowerShell vector/source generator; Segoe Script and Georgia system fonts are rendered but not packaged; no native pixels or proprietary fonts are included.
- Automated evidence: deterministic 64/32 contact sheet generated; focused/full/build/package/runtime gates pending.
- Human gate: compare P/M/B/Ri/Rv and Rapid Reload beside native choices at actual UI scale.
- Next action: run focused icon test, repository validator, complete suite, clean Release/package gates, then the packaged disposable firearm-dependent-feats observer.
## 2026-08-20 firearm feat icon automated qualification

- Status: automated-qualified; final aesthetic judgment remains human-gated.
- Determinism: a second tools/New-FirearmFeatIcons.ps1 run reproduced all six PNG and 64/32 contact-sheet SHA-256 values exactly.
- Repository/source: PASS.
- Complete dependency-free suite: 1,162/1,162 PASS, including firearm-feat-icons.semantic-publication.
- Clean Release/package: PASS; output validation PASS; firearm AssetBundle manifest/output validation PASS; SoundBank validation PASS; strict standalone package validation PASS.
- Guarded runtime scenario: disposable-firearm-dependent-feats, run 20260820T1505344745363Z-71ef2e5f35aa45ce9c929d0dc5369f47, 13/13 PASS.
- Runtime publication: distinct exact P/M/B/Ri/Rv sprites resolved under Weapon Focus, Greater Weapon Focus, Weapon Specialization, Greater Weapon Specialization, Improved Critical, and Rapid Reload children; the separate Rapid Reload top sprite resolved; all native top-level icons were preserved.
- Local-runtime package SHA-256: f256f59f65587d7475672eb415ed0e648cc60c7c85e4e388f60fa35021630b70.
- DLL SHA-256: c6060a14968fe0227b601fd0fe5c2c2f736241d4044b24036717576071900ecf.
- Firearm AssetBundle SHA-256: 1aa75fa1230abfb60cd5148ca90b99d604dbece7d80d98d85cb7d7c0a885a8ff.
- SoundBank SHA-256: 0e9f88c562f4f937a8941ace0f241bb31a7ed56b46fbca549c98f764392edf18.
- Runtime deployment backup: C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20260820T1505311020164Z; exact only-target restore verified.
- Human gate: inspect the five calligraphic parameter fields and Rapid Reload beside native feats at real 32/64 UI scale.
## Long-gun yaw correction disposition - 2026-08-20

| Defect | Reported | Reproduced | Root cause proven | Implemented | Source-qualified | Runtime-qualified | Human-gated | Blocked |
|---|---|---|---|---|---|---|---|---|
| Musket and Blunderbuss point too far left | yes | yes, zero held yaw retained after normalized rig work | yes, production held `Visual` and semantic anchors remained at identity while independent back frames were already correct | yes, Musket +3 degrees and Blunderbuss +4 degrees local Y only | yes, deterministic bundle plus 1,162/1,162 and full gates | yes, run `20260820T1526089673122Z-4727a84add664cbbbb4c93f1b3695c06`, 65/65 | yes, final in-character aesthetics | no |
