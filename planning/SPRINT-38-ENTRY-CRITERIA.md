# Sprint 38 entry criteria: Gunslinger Initiative

## Authority

At Gunslinger level 3, a character with at least one current grit point gains
+2 on initiative checks. With Quick Draw, free and unrestrained hands, and a
visible firearm, the tabletop rule also permits drawing one firearm as part of
the check.

## Qualified adaptation boundary

- Add exactly +2 to each native `RuleInitiativeRoll` for the owning unit while
  its native Gunslinger grit resource is positive.
- Apply the bonus after Kingmaker snapshots the native Initiative stat and
  before the engine consumes the rule result through the exact installed
  `IUnitInitiativeHandler` boundary.
- Reject zero or negative grit without changing the roll.
- Apply at most once to the same rule object.
- Grant the feature at Gunslinger level 3 through one stable blueprint.

The initiative-time firearm draw is not part of this checkpoint. It requires
an exact native contract for Quick Draw identity, free and unrestrained hands,
visible firearm selection, and action-free weapon-set change. Do not infer
those predicates or mutate equipment merely to claim completion.

## Deterministic acceptance

- Domain cases prove positive-grit +2, zero-grit +0, and invalid input failure.
- Blueprint/reflection checks prove the stable feature, level-three grant,
  exact handler type, exact private modifier field, and duplicate guard.
- A guarded detached runtime scenario proves native roll deltas of +2 with
  grit and +0 without grit, duplicate stability, no grit spend, exact unit
  isolation, and cleanup.
- Full repository validation, domain tests, clean Release build, strict package
  validation, runtime-request tests, preflight tests, exact mod load, and two
  independent feature PASS runs are required before runtime qualification.

## Non-goals

- No initiative roll override or forced d20.
- No global Harmony patch.
- No automatic inventory, hand, or weapon-set mutation.
- No save selection, loading, or writing.
