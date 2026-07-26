# Sprint 24 repair runtime acceptance — 0.0.24.1

Date: 2026-07-16
Candidate: `0.0.24.1-s24-broken-reload-repair`
Decision: **PASS — Sprint 25 entry approved**

The supplied Kingmaker 2.1.7b / UMM 0.32.4 screenshots establish the repaired end-to-end sequence on one exact Test Musket:

```text
loaded/Normal
  -> forced natural 1 misfire
empty/Broken
  -> full-round reload
loaded/Broken
  -> forced natural 1 misfire
empty/Wrecked
```

Observed blocking results:

- first misfire: `normalToBroken=1`, `rounds=0`, `condition=Broken`;
- Broken reload: `attempts=1`, `loaded=1`, `rejected=0`, `faults=0`, before and after condition both Broken;
- atomic inventory use: Black Powder Charge and Lead Ball each changed from 17 to 16;
- second misfire: `brokenToWrecked=1`, `rounds=0`, `condition=Wrecked`;
- Wrecked reload readiness: `available=False` with an explicit Wrecked rejection reason;
- Wrecked attack enforcement: `wreckedRejected=1` while `fired` remained 2;
- duplicate assignment, evaluation, and attack-event counters remained zero;
- attack, AC, reload, natural-roll, token-reconciliation, and combat-trace fault counters remained zero; and
- the tester reported that saving completed successfully.

The accepted item-owned inert `BlueprintWeaponEnchantment` token remains the authoritative state carrier. No evidence supports reviving the rejected `ItemEntityWeapon.UniqueId` vault.

Sprint 25 may therefore add only the bounded second-misfire explosion/damage consequence described in `planning/SPRINT-25-ENTRY-CRITERIA.md`.
