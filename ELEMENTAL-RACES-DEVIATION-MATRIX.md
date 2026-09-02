# Elemental Races deviation matrix

These adaptations are owner-approved. Local engine evidence may reject an
unsafe mechanism, but must not silently change the player-facing rule.

| Surface | Kingmaker implementation | Disposition |
| --- | --- | --- |
| Darkvision | Keen Senses and exactly +2 racial Perception | APPROVED ADAPTATION |
| Native outsider | Exact native empty `OutsiderType` fact plus Aasimar donor identity, matching installed Aasimar/Tiefling person-spell precedent | IMPLEMENTED; ACTUAL PERSON-SPELL MATRIX PENDING |
| Race identity | Four distinct project `BlueprintRace` references using `RaceId.Aasimar`; no new enum member or broad race-check patch | IMPLEMENTED; DIALOGUE LIMITATION ACCEPTED |
| Elemental affinity | +1 racial spell DC only for Fire, Acid, Electricity, or Cold respectively; exactly once; no caster-level bonus | APPROVED ADAPTATION |
| Ifrit SLA | Native Burning Hands, once daily, Charisma-based racial SLA DC, total character-level caster level | APPROVED |
| Oread movement | Base 20 feet plus the exact native Dwarf Slow and Steady feature for armor/encumbrance immunity | IMPLEMENTED; ACTUAL MOVEMENT MATRIX PENDING |
| Oread SLA | Sanitized native Kingmaker Stone Fist clone, one use per rest | IMPLEMENTED; ACTUAL CAST/EXPIRY PENDING |
| Sylph SLA | Feather Step replaces Feather Fall and must be named honestly in player text | APPROVED ADAPTATION |
| Undine swimming | Swim speed and Swim-as-class-skill clauses are descriptive no-ops because Kingmaker has no ordinary player swimming system | APPROVED OMISSION |
| Hydraulic Push | Native `ContextActionCombatManeuver` Bull Rush with caster level as base attack and best mental stat; no save or unrelated attack roll | IMPLEMENTED; ACTUAL COMBAT/IMMUNITY RUNTIME PENDING |
| Languages | Not implemented | OUT OF SCOPE |
| Favored-class bonuses | Not implemented | OUT OF SCOPE |
| Alternate traits/heritages | Base heritage only | OUT OF SCOPE |
| Race feats/archetypes/dialogue rewrites | Not implemented | OUT OF SCOPE |
| Visual geometry | Vanilla Kingmaker modular assets only; no new body meshes, fins, crystals, flame hair, or persistent VFX | APPROVED CONSTRAINT |
| Portraits | No automatic race-specific portraits | OUT OF SCOPE |
| Donor dialogue consequences | `RaceId.Aasimar` checks can classify an elemental race as Aasimar; do not globally rewrite dialogue or person-spell checks | ACCEPTED LIMITATION |
