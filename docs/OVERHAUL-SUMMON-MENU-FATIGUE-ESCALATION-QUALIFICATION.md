# Overhaul, summon-menu, and fatigue escalation qualification

## Candidate identity

- Branch: `codex/overhaul-summon-menu-fatigue-escalation`
- Authoritative baseline:
  `970aeb7972dcb155d5789a636ba156e68d1c0d0a`
- Candidate commit: pending immutable source-qualified commit
- Source-qualified package SHA-256:
  `54C3C382DA502FFD1D0F3703D7B40551CCA476B381EFD191F8AC98BC9D80842C`
- Source-qualified DLL SHA-256:
  `A3D67D1FEBA183973D1B280A9E5A01F2A675A7B982171488C7509DE508B59AE6`

Generated packages, deployment files, saves, and raw runtime logs are excluded
from the source commit.

## Automated source and package gates

- Focused overhaul, summon-menu, fatigue, Acadamae, Cord, Expanded Summoning,
  and maintenance tests: PASS as part of the complete suite.
- Version-aware repository validation: PASS.
- Complete dependency-free domain/reflection suite: PASS, 1278/1278.
- Clean exact-reference Release, output, SoundBank, deterministic package, and
  strict package validation: PASS, with zero compile warnings/errors.
- Runtime-scenario preflight safety suite: PASS, 147 checks.
- Identical-input package reconstruction: PASS; both archives have the package
  hash recorded above.

## Guarded runtime

Every real launch uses the guarded request mechanism through Steam App ID
640820. Before the immutable commit, a dirty-source build of the exact source
tree passed the focused canonical matrix plus a real three-launch persistence
sequence on `KMG_AUTOMATION_WORKING`: prepare/save, reload/verify/cleanup/save,
and reload/verify-absent with no write. These runs are engineering evidence;
all final changed-surface runs and working-save smoke remain pending the clean
immutable candidate. The menu scenario
`observe-expanded-summoning-variant-menu` is deliberately supervised and
read-only: a human must load `KMG_AUTOMATION_WORKING`, place the largest list
near the top-left sidebar, and open it. The observer never sends input, selects
an option, casts, or writes the save.

## Evidence boundary

Pure geometry tests cover top, middle, bottom, narrow, ultrawide, scale,
oversize, short-native, repeat, third-party-height, and navigation policies.
They are not a substitute for the actual rendered-view measurement. Until the
supervised observer completes, final menu presentation and controller focus in
the real player UI remain human-gated. This is the only expected manual
mechanical/UI boundary; automatic guarded scenarios will qualify overhaul and
canonical condition behavior.

## Human acceptance

1. Equip one empty Wrecked firearm and possess one Firearm Repair Kit.
2. Out of combat, activate **Overhaul Firearm**.
3. Confirm it completes promptly without waiting one game minute.
4. Confirm the same firearm becomes Broken and exactly one kit is consumed.
5. Confirm no campaign or game-time minute was visibly advanced.
6. Enter combat and confirm Overhaul Firearm cannot be used.
7. Place a high-tier Summon Monster spell near the top of the left sidebar.
8. Open its largest variant list.
9. Confirm every option remains on-screen or reachable by scrolling.
10. Confirm the first and last options can both be selected.
11. Confirm a short native variant menu still looks and behaves normally.
12. With Acadamae Graduate active and the caster not Fatigued, fail the save
    and confirm Fatigued.
13. Fail another Acadamae save before resting and confirm Exhausted.
14. Confirm another fatigue application does not duplicate or downgrade
    Exhausted.
15. Repeat the relevant fatigue cases while wearing the Cord of Stubborn
    Resolve and confirm its established substitution occurs exactly once.
