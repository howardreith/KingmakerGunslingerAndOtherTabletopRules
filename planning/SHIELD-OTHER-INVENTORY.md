# Shield Other Pre-implementation Inventory

## Baseline and installed profile

- Frozen base and starting HEAD:
  `7ba84439caa1fc92b8c8148ce95ea79fd59bdc57`.
- Exact Kingmaker references: private extracted 2.1.7b `Assembly-CSharp.dll`
  selected by `GamePath.props`.
- Installed Call of the Wild: 1.14.4c-2.1; DLL SHA-256
  `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`.
- Static scans of CotW `blueprints.txt`, `loaded_blueprints.txt`, and complete
  IL contain no `Shield Other`, `ShieldOther`, or `shield_other` candidate.
  Final live scans before and after all `LoadDictionary` postfixes remain a
  mandatory runtime gate; static absence is not treated as final proof.

## Exact 2.1.7b damage contract

Read-only ILDASM inspection of
`Kingmaker.RuleSystem.Rules.Damage.RuleDealDamage` proves that this Kingmaker
build has no `RedirectionTarget` or `RedirectedPercent` field/property.
`OnTrigger` calculates per-entry mitigation, sets aggregate `Damage`, applies
difficulty, sets final `Damage`, then in this exact order sets
`UnitEntityData.LastHandledDamage`, consumes target temporary hit points through
`TemporaryHitPoints.HandleDamage`, increments `UnitEntityData.Damage`, raises
`IDamageHandler.HandleDamageDealt`, and publishes hit presentation.

Selected seam: a narrowly signature-validated Harmony transpiler inserts one
Shield Other callback after the final `set_Damage` following
`ApplyDifficultyModifiers`, and before `set_LastHandledDamage`. The callback may
replace only the private finalized aggregate `Damage` value and create one
guarded transfer event. A guarded transfer event is forced to its exact already
finalized share at the same seam, avoiding a second defense or difficulty pass,
then continues through native temporary-HP, HP, death/threshold, damage-handler,
and log pathways. Exact IL anchors and cardinality must fail closed.

Rejected alternatives:

- `RuleDealDamage` prefix: damage is not finalized and defenses have not run.
- `IDamageHandler`: HP and temporary-HP application already occurred.
- full damage, heal subject, damage caster: explicitly forbidden and exposes
  incorrect death, concentration, riders, and observers.
- editing `DamageBundle` before calculation: evaluates/redistributes defenses
  incorrectly and cannot conserve finalized HP damage.
- direct `UnitEntityData.Damage` mutation: bypasses native temporary HP and
  damage/death/log consumers.

## Donors still to validate

Native blueprint runtime inventory remains required for close-range harmless
ally targeting, hour/level duration, deflection AC, resistance saves,
dismissal, caster-linked buffs, range termination, and the five base spell
lists. Final GUIDs and component contracts will be recorded before blueprint
construction.
