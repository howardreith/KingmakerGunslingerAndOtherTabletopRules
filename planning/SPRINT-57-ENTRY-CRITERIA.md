# Sprint 57 entry criteria: Death's Shot

## Authority

The authoritative local Gunslinger text grants Death's Shot at level 19. When
the Gunslinger scores a critical hit, they may spend 1 grit; the shot deals its
normal damage and the target makes a Fortitude save at DC
`10 + floor(Gunslinger level / 2) + Dexterity modifier`. Failure kills the
target as a death attack. The deed cannot restore grit for either its confirmed
critical or its killing blow.

## Kingmaker adaptation

Kingmaker cannot pause a completed attack and ask whether to activate a deed.
Death's Shot is therefore armed before firing by a personal free action. The
marker applies only to the next exact firearm attack and is consumed by that
attack whether it misses, hits normally, or confirms a critical. This is the
same bounded pre-shot adaptation used by other conditional deeds and prevents
an indefinite passive critical rider.

The armed attack remains an ordinary native firearm attack. It consumes its
ordinary chamber, retains native attack, confirmation, misfire, damage,
concealment, cover, and immunity behavior, and never substitutes synthetic
damage. Only a confirmed critical may spend grit and request the Fortitude
save. A successful save leaves normal critical damage intact. A failed save
uses an exact installed native death-effect action and native Death descriptor
contract; direct HP assignment is not an acceptable substitute.

## Exact-contract gate

Before production implementation, a save-free guarded observer must identify
exactly one installed native ability suitable as the death-effect authority
and record its stable GUID, descriptors, saving-throw action, kill action, and
nested action graph. If the installed contract is ambiguous or does not expose
native death immunity semantics, implementation stops rather than guessing.

## Acceptance criteria

1. Grant one stable Death's Shot feature and its arming ability exactly once at
   Gunslinger level 19.
2. The ability requires level 19, one equipped loaded non-Wrecked exact
   firearm, and enough grit for the deed, but arming spends neither grit nor a
   chamber.
3. The marker is unit-owned and applies only to its owner's next exact firearm
   attack. Other weapons, units, and unrelated rule objects are isolated.
4. Miss, non-critical hit, and unconfirmed threat consume the marker without
   spending grit or applying a save/death rider.
5. A confirmed critical spends exactly 1 grit, preserves normal native
   critical damage, and requests one native Fortitude save at the exact deed DC.
6. Save success preserves the already-delivered normal critical damage and
   adds no further effect. Save failure invokes the exact native death action.
7. Native Death descriptor immunity fails closed and is never bypassed by HP,
   damage, state, or reflection writes.
8. The deed's attack is excluded from both confirmed-critical and killing-blow
   grit recovery, including when normal critical damage or the death effect
   kills the target.
9. State, grit, marker, and exclusion bookkeeping roll back atomically on a
   delivery exception; duplicate callbacks cannot spend or kill twice.
10. Death's Shot remains eligible for the later True Grit selection. Sprint 57
    does not anticipate that cost reduction before the capstone exists.

## Required evidence

- Focused policy tests cover exact DC, critical/save branches, resource gates,
  marker consumption, recovery suppression, isolation, duplicate callbacks,
  native-death requirement, and invalid inputs.
- Repository validation, the complete domain suite, clean exact-reference
  Release compilation, and strict package validation must pass.
- After the native observer passes and source is qualified, one exact-version
  mod-load PASS and two independent guarded feature PASS runs must prove normal
  critical damage, exact grit, save success/failure, native death immunity,
  recovery suppression, isolation, and cleanup.
