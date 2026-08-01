# Sprint 39 entry criteria: Pistol-Whip

## Rule and adaptation

At level 3, Pistol-Whip spends one grit to make a standard-action melee attack
with the single equipped firearm. The attack is proficient, bludgeoning,
20/x2, uses 1d6 for a one-handed firearm or 1d10 for a two-handed firearm
before native size scaling, and receives the firearm's enhancement bonus on
attack and damage. A hit immediately triggers a native Trip combat maneuver.

Kingmaker's stock melee context action selects the threat hand and cannot use a
ranged firearm as its melee rules source. The narrow adaptation uses a hidden,
unowned transient melee surrogate selected by firearm handedness. It copies
only `GameHelper.GetItemEnhancementBonus` into the native weapon-stat rule and
then uses native `RuleAttackWithWeapon` and `RuleCombatManeuver.Trip` events.
It never equips the surrogate or copies firearm state enchantments.

## Observable behavior

- The ability is a targeted enemy standard action granted at Gunslinger 3.
- Exactly one equipped marked firearm and at least one current grit are
  required. Wrecked firearms are rejected; Normal and Broken firearms qualify.
- Delivery spends exactly one grit even if the native melee attack misses.
- One-handed and two-handed firearms select 1d6 and 1d10 base damage
  respectively; native creature-size scaling remains active.
- Firearm enhancement contributes equally to native attack and damage.
- A native Trip rule is raised only after a hit.
- No ammunition, loaded state, misfire, firearm condition, equipment, or save
  state is changed.
- Any exception before completed delivery restores the spent grit and leaves
  the exact firearm untouched.

## Deterministic evidence

- Domain cases cover handedness/dice, Normal/Broken eligibility, Wrecked and
  ambiguous rejection, insufficient-grit atomicity, and invalid input.
- Blueprint validation proves level-three grant, standard action, enemy touch
  range, hidden surrogate isolation, bludgeoning 20/x2 mechanics, and stable
  identities.
- A guarded disposable runtime scenario proves exact weapon identity,
  enhancement propagation, native hit/miss and Trip boundaries, grit deltas,
  handedness selection, and cleanup without save APIs.

## Non-goals

- No ranged shot, ammunition consumption, misfire roll, post-hit optional UI,
  equipment mutation, forced hit in production, or broad combat-rule patch.
