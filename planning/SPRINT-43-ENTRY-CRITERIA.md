# Sprint 43 entry criteria: Dead Shot

## Authoritative rule

- At Gunslinger level 7, spend 1 grit and use a full-round action to fire one
  loaded non-scatter firearm at one target.
- Roll the attacks available from base attack bonus, highest to lowest. The
  deed hits if any roll hits; every hit after the first adds one copy of the
  firearm's base damage dice, but does not multiply modifiers, precision
  damage, or weapon-property damage.
- Confirm at most one critical, using the highest attack bonus minus 5 and
  reducing that penalty by 1 for each critical threat after the first, to a
  minimum penalty of 0.
- The single discharge misfires only when every attack roll is a misfire.

## Kingmaker adaptation boundary

- Expose one extraordinary full-round targeted ability granted by the level-7
  deed feature. It must use the equipped exact firearm and native attack,
  damage, critical-confirmation, concealment, mirror-image, cover, line-of-
  sight, range, and target-validity contracts wherever they remain applicable.
- Base attack bonus determines roll count and iterative penalties using the
  installed native full-attack contract. Haste, two-weapon fighting, natural
  attacks, and unrelated bonus attacks do not add Dead Shot rolls.
- One loaded chamber and 1 grit are consumed once per accepted delivery,
  regardless of hit or miss. Empty, Wrecked, ambiguous, nonfirearm, or scatter
  contexts reject atomically.
- Additional hits add only exact base weapon dice through the native damage
  rule. They must not repeat Dexterity, enhancement, precision, elemental,
  critical, or other modifiers.
- Do not approximate the all-rolls-misfire or one-confirmation rule. Establish
  exact roll-control and critical-confirmation seams before production wiring;
  fail closed if the installed contracts cannot represent them safely.

## Observable acceptance

- The deed appears exactly at Gunslinger level 7 and is a full-round hostile
  single-target action costing 1 grit.
- BAB 1-5, 6-10, 11-15, and 16-20 produce exactly 1, 2, 3, and 4 ordered attack
  rolls with native iterative penalties.
- Zero hits deals no damage; one hit deals ordinary single-shot damage; each
  additional hit adds exactly one base-dice packet.
- Any non-misfire roll suppresses the discharge misfire. All rolls misfiring
  apply exactly one ordinary firearm condition transition.
- Multiple threats produce no more than one critical confirmation and use the
  specified confirmation penalty. State, grit, ammunition, and diagnostics
  remain isolated per unit and firearm.

## Deterministic tests

- BAB roll-count and penalty boundaries, hit aggregation, and invalid BAB.
- Base-dice multiplication separated from modifiers and critical damage.
- Mixed versus all-misfire roll sets and exactly one condition transition.
- Threat aggregation and confirmation-penalty bounds.
- Atomic rejection for zero grit, empty/Wrecked/scatter/nonfirearm/ambiguous
  equipment, invalid target, duplicate delivery, and post-acceptance faults.

## Runtime evidence

- Exact mod-load PASS for the source commit.
- A save-free guarded scenario must inspect the production feature/ability and
  deterministically deliver representative multi-roll hit, mixed-misfire, and
  all-misfire cases against detached units without saving.
- Require two independent fresh-process PASS runs before runtime qualification.

## Non-goals

- No cone scatter support, bonus-attack stacking, archetype replacement deed,
  new firearm kind, global full-attack rewrite, or change to ordinary firearm
  attacks, Gun Training, Deadeye, grit recovery, or native critical immunity.

## Failure and rollback

- Precondition or target failure changes no grit, ammunition, firearm state, or
  target state. An accepted delivery uses one atomic operation identity; a
  downstream fault must follow existing rollback and diagnostic contracts.
- Ambiguous native roll, damage, or confirmation behavior blocks production
  wiring until narrowed by exact installed-contract evidence; tests must not
  manufacture proof by replacing the production pipeline.
