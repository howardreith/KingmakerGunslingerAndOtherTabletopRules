# Sprint 51 entry criteria: Expert Loading

## Authority and adaptation

The mission-authorized local Gunslinger rules grant Expert Loading at level 11:
after rolling a misfire with a Broken gun, the gunslinger may spend 1 grit to
prevent the explosion while the gun remains Broken.

Kingmaker cannot pause the native attack pipeline after the misfire roll for a
player choice. Sprint 51 adapts the reaction to a personal free-action pre-shot
choice that arms the next exact firearm attack. The marker is consumed by that
attack whether it hits, misses, or misfires. It spends no grit unless the attack
would perform the exact Broken-to-Wrecked explosion transition.

## Observable contract

- The feature and its personal extraordinary free-action ability appear exactly
  once at Gunslinger level 11.
- Arming replaces only this unit's previous Expert Loading marker and does not
  alter any firearm state or grit immediately.
- A non-firearm action does not consume the marker.
- The next exact firearm attack consumes the marker.
- On a Broken-firearm misfire with at least 1 grit, exactly 1 grit is spent,
  Broken-to-Wrecked mutation is suppressed, and no burst is scheduled; the
  discharged firearm remains empty/Broken and the native attack remains a
  misfire/miss.
- With no grit, the same event retains the ordinary Broken-to-Wrecked mutation
  and one native burst.
- A Normal-firearm attack, a non-misfire, an empty/Wrecked/incompatible weapon,
  or another unit's attack receives no suppression and spends no grit.
- Duplicate native success evaluation cannot spend twice or suppress an
  unrelated transition.
- Any failure while spending or verifying grit fails closed to the ordinary
  Broken-to-Wrecked explosion path.

## Deterministic qualification

- Pure tests cover suppression, insufficient grit, marker consumption, exact
  event gates, duplicates, and invalid input.
- Repository validation, the full domain suite, clean Release build, and strict
  package validation must pass.
- A guarded save-free scenario must prove level-11 progression, pre-shot arming,
  one-grit suppression with the exact firearm remaining Broken, ordinary
  no-grit Wrecked/burst behavior, chamber consumption, cleanup, and isolation.
- The exact assembly requires mod-load PASS and two independent fresh-process
  feature PASS runs.

## Non-goals

Sprint 51 does not change misfire thresholds, ordinary Normal-to-Broken damage,
burst radius/damage, Gun Training, Quick Clear, repair/overhaul, Lightning
Reload, True Grit, or the independently blocked Targeting Arms adaptation.
