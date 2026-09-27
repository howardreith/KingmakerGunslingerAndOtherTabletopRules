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
