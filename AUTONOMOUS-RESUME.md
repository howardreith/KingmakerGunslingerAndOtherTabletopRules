# Sprint 23 Powerful Charge rounding follow-up — IN PROGRESS

The owner selected a bounded correction and patch-release campaign for the
released Triceratops Powerful Charge live-Strength calculation. Work is on
`codex/expanded-summoning-sprint23-charge-rounding`, based directly on master
`15d6c0d82bf14fa4fd66c72c0f780f86abf44794`. Sprint 24 has not been started.

The released helper separately truncated the additional half-Strength term.
A fresh focused reproduction selected one test and failed at modifier +7:
ordinary +10 plus the released +3 correction produced +13 instead of the
documented +14 target. Installed Kingmaker 2.1.7b IL confirms that a positive
single-natural-attack contribution is truncated after multiplication by 1.5,
while a zero or negative modifier is applied once. The source correction now
computes the difference between that already-applied native term and the
Triceratops's existing positive 2x target; zero and penalties receive no extra
correction. The full Sprint 23 focused selection passes 12/12 after this edit.

The request-local charge observer now records engine weapon-stat rows and real
issued charge commands at Strength 24/26/28/30, plus signed, Power Attack,
critical, aborted/opportunity/later-attack and released-rhino controls. Offline
request/profile preflight and the 12-test Sprint 23 focused selection pass.
The first versioned candidate gate preserved a seven-failure domain result:
the added release-note package member correctly advanced inventory to 364/362,
but six historical delivery assertions and the deterministic ZIP builder still
pinned 363/361. Their exact expectations/allowlist were repaired; the affected
22-test selection and the package-focused selection pass. Next actions are to
commit that repair, rerun the Sprint gate on the changed candidate, and qualify
the affected RTWP, turn-based and high-risk-combined cases through guarded
Steam App ID 640820 transactions. Every transaction must
restore the actual intake:
968 Mods entries, KMG 0.0.117, 83 settings, 118 saves, the protected baseline
and authorized working-save hashes, and no compatibility lock or staging.

The authoritative active contract and evidence ledger are
[the rounding mission](docs/SPRINT23-TRICERATOPS-POWERFUL-CHARGE-ROUNDING-MISSION.md)
and [its machine-readable ledger](planning/SPRINT23-TRICERATOPS-POWERFUL-CHARGE-ROUNDING-LEDGER.json).
Do not alter dinosaur art, identities, routes, trample/trip behavior or old
release records. Do not begin Sprint 24.
