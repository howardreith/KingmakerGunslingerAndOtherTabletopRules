# Gunslinger Human-Review Defect Matrix

Status vocabulary: `reported`, `reproduced`, `root cause proven`, `implemented`,
`source-qualified`, `runtime-qualified`, `human-gated`, `blocked`, and
`accepted/frozen`.

| ID | Defect or acceptance | Intake status | Superseded evidence | Current hypothesis | Next action |
|---|---|---|---|---|---|
| P0 | New Tenebrous Depths save is unloadable | reproduced; root cause proven; implemented; source-qualified; blocked | `0.0.88` working-save and mod-load smoke did not cover the affected save or Focused Aim serialization matrix | null Focused Aim buff FX/resource links caused the first reconstruction exception and are repaired compatibly; both the repaired affected copy and a no-marker same-area control still stall before the native after-load callback | retain release block; use Tenebrous-specific lifecycle instrumentation or consolidated human copied-save observation; never mutate the original |
| ACADEMAE | Toggle-on prepared Summon Monster I still uses full-round; no save/fatigue | reported; reproduced; root cause proven; implemented; source-qualified; runtime qualification pending; human-gated | synthetic 14/14 and later 15/15 runs did not reproduce the detached canonical `AbilityData` used by ordinary selection; the exact installed `0.0.88` human run supersedes both conclusions | the outer KMG variant and detached native `SummonMonsterISingle` both had null `ParamSpellSlot`; eligibility inspected the outer node only and rejected `status=not-prepared` despite an available matching memorized slot | run the packaged detached-player-path scenario twice, inspect patch owners/order, deploy exact candidate, then request one OFF/ON human cast |
| ACQUISITION | 30 targets remain clustered/questionable | structurally accepted; runtime-qualified; materialization human-gated; regression-frozen | 25/25 graph run proved reference shape, not organic pacing or normal lootability | revised fixed map reduces maximum normalized named-area density from six to two and preserves one exact target per unique | do not redistribute without new evidence; retain ordinary accessibility/materialization checks |
| BTSL | Permanent equipment is on Xelliren instead of Honest Guy | accepted/frozen; runtime-qualified | prior publication tests asserted identical rows on all four tables and did not test owner responsibility or appended-block ordering | human accepted Honest Guy permanent equipment and Xelliren support-material responsibility | regression only; do not redesign |
| ICONS | Firearm monograms remain aesthetically wrong | reported; human-gated; parked | generated PNG approximations do not reproduce the accepted Nodachi selector, which is rendered by `CustomWeaponSelectorRuntime` / `FeatureUIData` | future work should route firearm parameters through the exact runtime monogram path instead of another approximate PNG family | do not implement before Acadamae closes |
| RAPID-RELOAD-ICON | Rapid Reload is very close | temporary candidate accepted; minor human-gated adjustment parked | broad redesign is no longer requested | only a restrained color adjustment remains | do not implement before Acadamae closes |
| LONG-GUN-YAW | Musket and Blunderbuss point farther left; Musket stays horizontal during attack | reported; human-gated; parked | prior structural rig run did not compare barrel/muzzle forward against target direction at release; `+3/+4` deltas moved in the wrong direction | first revert the deltas, then test the opposite sign; treat Musket attack aiming as a separate animation/attachment defect | do not implement before Acadamae closes |
| SPEAR-ORIENTATION | Point faces backward and back pose is horizontal | root cause proven; implemented; source-qualified; runtime-qualified; human-gated | geometry run proved length/axis magnitude, not forward direction or carry pose | the held wrapper mapped source `+Z` to installed `+Y`, but the installed Longspear forward contract is `-Y`; the null donor belt model also caused reuse of the held model for carry | preserve the accepted 2.28 m source geometry; use `+90 X` held mapping plus a distinct upper-left diagonal BeltModel; human-check equipped attack phases and carried pose on representative bodies |
| FOCUSED-AIM | Charisma damage, one Grit spend, kill recovery | accepted/frozen; runtime-qualified | none | stable marker compatibility repair does not alter the transaction | final run `20260820T1558117539372Z-38e92ddef9bb4892b87a21ad17f24384` passed 7/7; retain one consolidated human counter/damage check |
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
| Acadamae real prepared player path | yes | yes, exact human trace captured outer KMG variant plus detached native root, both with null `ParamSpellSlot`, `prepared=False`, `canSpend=False`, `status=not-prepared` | yes, prior resolver required an already-bound chain slot and object-reference identity instead of resolving the available memorized slot by canonical blueprint identity | yes, read-only canonical resolver for presentation, command-only slot binding, detached-node runtime fixture, bounded presentation trace, and startup Harmony audit | yes, repository validation, 1,162/1,162, clean Release/output/SoundBank/package/strict package | pending; former two 15/15 runs are superseded by the later exact human failure | yes, final ordinary OFF/ON cast | no |

## 2026-08-20 Acadamae priority continuation

- Human-run evidence: `C:/Dev/KingmakerGunslingerLab/runtime-evidence/acadamae-human-runs/20260820T1949234419079Z`.
- Captured log SHA-256: `5CF02C1F71DBD121855B3B2F0ADBFC9DA3301FA3DC57DB03E59098EAF61CB231`.
- Exact rejection: Leinna, feat rank 1, mode active, KMG Dog/Eagle/Poisonous Frog variants, native `SummonMonsterISingle` converted root, `prepared=False`, `canSpend=False`, both slot fields null, `preRequireFullRound=True`, `status=not-prepared`.
- Installed identity: package `98BA3475B5CD2068DF6152C49DEAF47CF9D8C1247F889E1F12FB0646079265C9`; DLL `E6E08804CD19C69DACA8A3BE77DC04220497BFC78E0CE31B07BE0B498953B76D`; Call of the Wild `1.14.4c-2.1`; Bag of Tricks `1.16.4`.
- Current state: source-qualified only. Runtime and human gates remain open.
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
