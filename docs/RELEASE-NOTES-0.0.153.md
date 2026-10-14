# v0.0.153 — Triceratops Powerful Charge rounding

`0.0.153-expanded-summoning-sprint23-charge-rounding`

This patch corrects one local arithmetic edge in the released Triceratops
Powerful Charge adapter. Kingmaker first applies the ordinary positive
single-natural-attack Strength contribution as truncated 1.5× Strength. The
adapter now adds the difference between that already-applied amount and Sprint
23's documented 2× Triceratops charge target. Positive odd modifiers therefore
no longer lose a point through separate rounding.

| Strength modifier | Ordinary contribution | v0.0.152 charge total | Corrected charge total |
| ---: | ---: | ---: | ---: |
| +7 | +10 | +13 | +14 |
| +8 | +12 | +16 | +16 |
| +9 | +13 | +17 | +18 |
| +10 | +15 | +20 | +20 |

Zero and Strength penalties follow Kingmaker's native signed convention: the
ordinary calculation applies them once, and this positive-bonus correction
adds nothing. The patch does not double the whole damage bundle or alter Power
Attack, critical multiplication, weapon attribution, physical damage forms,
enhancement, or damage reduction.

The ordinary 2d10+12 gore and printed baseline 4d10+16 charge remain unchanged.
Only the first eligible real charge gore receives the special 4d10 dice and
live Strength correction. Ordinary attacks, aborted charges, later attacks,
opportunity attacks, and replayed callbacks remain ineligible. Rhinoceros and
Woolly Rhinoceros retain their historical fixed charge increments.

This correction adds no identities, routes, assets, serialization state, or
summon-menu entries. The released v0.0.152 catalog remains 105 project
creatures, 1,075 generated placements, 29 retained native wrappers, and 1,104
visible choices. Sprint 23's twelve routes, trample and trip mechanics,
original visuals, module lifecycle, and persistence evidence are unchanged and
retain their v0.0.152 provenance. Sprint 24 is not part of this patch.

The source interpretation is intentionally narrow: the universal Powerful
Charge rule directs readers to each creature's damage entry; it does not impose
a generic 2× Strength formula on every monster. This patch makes the project's
already documented Triceratops scaling contract internally consistent.
