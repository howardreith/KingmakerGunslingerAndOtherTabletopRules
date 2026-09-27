# Expanded Summoning Phase 2 evidence index

Structured runtime artifacts are kept under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/` and are not committed.
The guarded wrapper restores the installed mod tree after each run. The
original 136-file live tree has SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.

| Scope | Passing result directory | Restoration record | Limit |
| --- | --- | --- | --- |
| Sprint 9 final Eagle/Bat doorway and cleanup | `20260927T1536450257132Z-working-save-expanded-summoning-creature-review` | `20260927T1541351948729Z-working-save-expanded-summoning-creature-review.json` | One connected-room route; owner visual approval pending. |
| Sprint 10 Wasp hidden registration and menu | `20260927T1804369885102Z-observe-expanded-summoning-inventory` | `20260927T1808371924124Z-observe-expanded-summoning-inventory.json` | Wasp placements suppressed; no visible choice. |
| Sprint 10 Wasp poison and private view | `20260927T1912579609072Z-disposable-expanded-summoning` | `20260927T1916362556609Z-disposable-expanded-summoning.json` | 21/21 assertions; DC 18, sting/poison/cure and visual 2/2; full Wasp qualification pending. |
| Sprint 10 Wasp suppressed movement and camera review | `20260927T1955333075608Z-working-save-expanded-summoning-creature-review` | `20260927T1959253902969Z-working-save-expanded-summoning-creature-review.json` | Native 12.345 m travel through a connected doorway and cleanup passed; crowded camera frames do not qualify target contact or visual acceptance. |
| Sprint 10 Wasp private quantity casts | `20260927T2015561300256Z-disposable-expanded-summoning` | `20260927T2019330163598Z-disposable-expanded-summoning.json` | 22/22 assertions, 183 casts, four Wasp quantity commands across SM/SNA and 14/14 private visual attachments; placements remain hidden. |
| Sprint 10 Wasp native Vermin immunity | `20260927T2115391949167Z-disposable-expanded-summoning` | `20260927T2119178826656Z-disposable-expanded-summoning.json` | 23/23 assertions. Native RuleApplyBuff: Wasp immune/ineligible, human control eligible; the fixture installed no buff on either target. Species marker still reads EagleGiant. |
| Sprint 10 owned Wasp species type and immunity | `20260927T2152154541742Z-disposable-expanded-summoning` | `20260927T2155531149844Z-disposable-expanded-summoning.json` | 23/23 assertions. Owned Wasp type replaces EagleGiant; native VerminType grants VerminImmunities and paired RuleApplyBuff remains Wasp immune/human eligible. No control buff installation is claimed. |
| Sprint 10 Wasp native turn-based strike cadence | `20260927T2217134616132Z-summon-same-turn-activation` | `20260927T2223192246293Z-summon-same-turn-activation.json` | Two exact-target native sting rules; stinger surface 0.643–0.654 m from body bounds at impact, so visual contact remains open. |
| Sprint 10 Wasp native RTWP strike cadence | `20260927T2220168994981Z-summon-same-turn-rtwp-control` | `20260927T2223192246293Z-summon-same-turn-activation.json` | Two exact-target native sting rules after 145 wait frames; stinger surface 0.743 m and 2.04 m from body bounds at impact. Both runs shared one exact restoration. |
| Sprint 10 Wasp impact camera diagnosis, turn-based | `20260927T2251029714353Z-summon-same-turn-activation` | `20260927T2257092555149Z-summon-same-turn-activation.json` | Two valid overhead and two party-camera PNGs at native strikes; overhead shows stinger pointing away from target, and party-camera wall occlusion prevents visual acceptance. Mechanical assertions passed. |
| Sprint 10 Wasp impact camera diagnosis, RTWP | `20260927T2254096344270Z-summon-same-turn-rtwp-control` | `20260927T2257092555149Z-summon-same-turn-activation.json` | Two valid overhead and two party-camera PNGs; second RTWP frame shows abdomen/stinger extended away from target. First RTWP frame was partly dissolved. Both modes shared one exact restoration. |

Diagnostic failures `20260927T1841288896478Z`,
`20260927T1853108579217Z` and `20260927T1903352617466Z` are excluded
from qualification. They exposed and drove the Wasp DC, visual bone-count
and round-tick assertion corrections. The final run observed an enemy
damage scale of 0.8, which truncated a rolled 1 on the next poison round
to zero integer stat damage; no positive next-round damage claim is made
from that tick.

The preceding visual probe `20260927T1938520090580Z` also passed its
mechanical assertions. The later run added one attack-frame diagnostic with
the Wasp's sole auxiliary renderer temporarily hidden and restored. Cyan
silhouettes remained in that frame, including one at a distant wall. Native
occlusion display is the current interpretation, not a proven renderer
identity. Neither run establishes unobstructed wing or target contact.

Immunity diagnostics `20260927T2037593011761Z`,
`20260927T2051011933829Z` and `20260927T2104166748655Z` are excluded
from qualification. They established the native VerminType fact graph and
showed why direct buff insertion and control installation were unsuitable
assertions. The final paired RuleApplyBuff check reports only native
eligibility/immunity, not a successful control debuff application.

Species diagnostic `20260927T2140542227252Z` is excluded: the owned type
was present, but an added direct `SpellDescriptor.MindAffecting` mask
assertion failed. Native `VerminType` instead grants `VerminImmunities`;
the passing run tests the observed rule decision and records the mask as
diagnostic only. Its restoration also passed.

Request-local Wasp Tail-pose diagnostic: guarded turn-based
`20260927T2310527552766Z`, RTWP `20260927T2314172123373Z`, and exact
restoration `20260927T2317241914821Z-summon-same-turn-activation.json`.
Both combat modes passed two exact stings. Baked geometry improved but the
same-frame images did not establish visible contact; Wasp remains hidden.

Two-frame Wasp Tail diagnostic: turn-based `20260927T2331283868029Z`,
RTWP `20260927T2334367843649Z`, exact restoration
`20260927T2337441522077Z-summon-same-turn-activation.json`. Both combat
modes passed two exact stings. Delayed images change pose, but doorway
clipping and a 1.193 m second RTWP gap prevent visual qualification.

Stirge rules-policy checkpoint: repository validation, 1,939 domain tests,
clean Release and strict package PASS. Guarded working-save smoke
`20260927T2353348639066Z` PASS and exact original installation restoration
`20260927T2356206464811Z-working-save-smoke.json` PASS. This is only a
load/safety check; no Stirge runtime mechanics are installed yet.
