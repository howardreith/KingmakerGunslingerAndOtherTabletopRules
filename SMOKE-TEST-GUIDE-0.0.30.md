# Kingmaker Gunslinger 0.0.30 focused smoke test

Use a disposable campaign and the exact 0.0.30 package.

1. Confirm UMM loads the mod without bootstrap or Harmony faults.
2. Equip a genuine native Heavy Crossbow and print Reload, Overhaul, and Repair
   readiness. All must reject it, create no firearm state, and leave an ordinary
   crossbow attack untouched.
3. Equip one Test Musket with a second in inventory. Run the immediate complete
   maintenance qualification once. Require `overall=PASS` and
   `stage=MaintenanceLoopPassed`.
4. Prepare the fixture. Complete Overhaul, interrupt Repair before delivery,
   then complete Repair and Reload. Require exactly two kits, one powder, and
   one Lead Ball consumed; target revision `+3`; same target identity; unchanged
   second item; and no new faults or duplicates.
5. Quicksave, make a full save, exit to desktop, restart, and reload. Require the
   target to remain loaded/Normal, the second item to retain its independent
   state, and token conflicts/faults to remain zero.

This focused gate passed twice from independent fresh processes on commit
`0052dad0dae299eeefd302e511a0ae4b57dcdbac`. The guarded evidence is recorded
in `SPRINT-30-REPORT.md`; Sprint 31 content may begin from that qualified
baseline.
