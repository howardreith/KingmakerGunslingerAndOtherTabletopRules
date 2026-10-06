# Sprint 17 closed snake profile/body slice

Status: source checked; runtime **NOT RUN / NOT QUALIFIED**.
Laptop PR26, parent `a554858f41283e3ae60fcdfa2eb81de0f27f3eb3`.
Sprints14–16 remain complete. Neither snake is published; Salamander unchanged.

The new exact request `disposable-expanded-summoning-snake-profiles` runs
only Viper and Constrictor Snake. It uses the existing working-save loading,
fingerprint, save-write sentinels, bounded iterator and native owned-unit/
environment cleanup. It permits only `saveName=KMG_AUTOMATION_WORKING`;
unknown names, other saves, prefab inputs and creature selectors are rejected.
It has no save-write authority and does not run Salamander/donor metadata.

The request first executes the17 production view assertions from
[the view checkpoint](EXPANDED-SUMMONING-SPRINT17-SNAKE-VIEW-INTEGRATION.md).
Then18 profile assertions measure the actual untemplated private summons:
scores/HD/Medium/owned type; HP/AC/touch/flat-footed/saves/speed/initiative;
full land skill breakdowns and exact ranks; one primary bite, attack bonus,
and correctly scoped grab fact. RuleCalculateWeaponStats supplies the damage,
not the static profile. Temporary native Strength+4 and Strength-to7 modifiers,
native AnimalGrowthBuff and final restoration expose positive multiplier,
negative penalty and live size/dice behavior. All temporary effects are removed
in finally blocks before native unit destruction. Native appearance buffs stay.
The fixture does not force hits, deal profile-test damage or patch rule results.

Expected minimum closed completion is38 assertions:17 views,18 profiles,
environment restoration, fixture cleanup and loaded version. An early exception
is a failure, not reduced accepted coverage.

This is deliberately NOT the full Sprint17 hidden gate. It does not qualify
poison/constrict cadence, real RTWP/turn-based commands or AI, contacts in those
commands, persistence, crowds, routes or publication. Those remain mandatory.
It isolates source arithmetic and automatic-body ownership before extending the
real-command matrix, without repeating the closed Salamander donor census.

Source checks:248 focused tests PASS; complete unfiltered2050 tests PASS83.0s;
repository/static/icon/manifest, clean14-reference Release and deterministic/
strict320-member package PASS179.5s as dirty-tree diagnostics. Runtime request
preflight passed496 checks alone, with no concurrent source/artifact writes.
Initial compile missed two namespace imports in the fixture; corrected locally.
Logs `artifacts/sprint17-snake-profile-focused.log`,
`artifacts/sprint17-snake-profile-dirty-compile.log`,
`artifacts/sprint17-snake-profile-dirty-full-gate.log` are retained.

Next freeze/push, run the exact committed complete source gate and remaining
prelaunch checks, then fresh Steam working-save-smoke plus this closed slice
under one lease-first snapshot/deployment/restoration transaction.
Do not deploy any dirty diagnostic package. Record actual results honestly;
a profile/body PASS cannot close Sprint17.

Surface1008 registered/976 published/32 withheld+29 wrappers=1005 visible.
No new runtime, installation observation, deployment or save operation yet.
Last closed game transaction remains a904 at14:14UTC. Unity2018 authoring probe
confirmed invalid license and self-exited; do not reactivate or repeat it.
Phase2C authorized but deferred until Phase2B owner acceptance.
HumanReview: NOT_PERFORMED_NONBLOCKING.
