# Sprint 58 entry criteria: Stunning Shot

## Authority

At Gunslinger level 19, after hitting a creature, the Gunslinger may spend 2
grit. The target makes a Fortitude save at DC
`10 + floor(Gunslinger level / 2) + Wisdom modifier`; failure stuns the target
for 1 round. Creatures immune to critical hits are immune to this deed.

## Kingmaker adaptation

Kingmaker cannot pause a completed hit for a player decision, so a personal
free action arms the next exact firearm attack. Its unit-owned marker is
consumed by that attack on miss or hit. A native hit that is not critical-hit
immune spends exactly 2 grit and requests one native Fortitude save. Failure
applies the exact installed Stunned condition for one round; success applies
nothing. The ordinary shot retains native chamber, misfire, attack, critical,
damage, concealment, cover, and line-of-sight behavior.

Installed `RuleAttackRoll` exposes the resolved `ImmuneToCriticalHit` flag.
That exact per-attack result is authoritative for the immunity clause; blueprint
names, creature types, anatomy, and hand-maintained immunity lists are forbidden.

## Exact-contract gate

A save-free guarded observer must identify exactly one installed native Stunned
buff, record its stable GUID and components, and prove it uses the native
Stunned condition contract. Production registration must require that exact GUID
and clone its mechanics rather than inventing a parallel penalty package.

## Acceptance criteria

1. Grant one stable Stunning Shot feature, arming ability, and unit-owned armed
   marker exactly once at Gunslinger level 19.
2. Arming requires level 19, at least 2 grit, and one equipped loaded
   non-Wrecked exact firearm, but spends neither grit nor ammunition.
3. The next owned exact firearm attack consumes the marker exactly once.
   Misses, other weapons, other units, and duplicate callbacks cannot apply it.
4. A miss consumes the marker without grit or rider. A hit whose exact native
   attack result has `ImmuneToCriticalHit == true` also spends no grit and
   applies no save or condition.
5. An eligible hit spends exactly 2 grit and requests exactly one native
   Fortitude save at the deed DC.
6. Save success preserves ordinary firearm damage and applies no condition.
   Save failure applies only the exact native Stunned mechanics for 1 round.
7. Grit, marker, and condition delivery are atomic with rollback on faults;
   duplicate rule callbacks cannot spend or apply twice.
8. Stunning Shot remains eligible for the later True Grit selection; Sprint 58
   preserves its ordinary 2-grit cost until that capstone is implemented.

## Required evidence

- Focused tests cover DC, resource and level gates, miss, native immunity, save
  success/failure, marker consumption, isolation, duplicates, and invalid data.
- Repository validation, complete domain suite, clean exact-reference Release
  build, and strict package validation pass.
- The native observer passes before production implementation.
- One exact-version mod-load PASS and two independent guarded feature PASS runs
  prove hit/damage preservation, exact grit, Fortitude branches, one-round
  native Stunned behavior, native critical immunity, isolation, and cleanup.
