# Elemental Races deviation matrix

These adaptations are owner-approved. Local engine evidence may reject an
unsafe mechanism, but must not silently change the player-facing rule.

| Surface | Kingmaker implementation | Disposition |
| --- | --- | --- |
| Darkvision | Keen Senses and exactly +2 racial Perception | APPROVED ADAPTATION |
| Native outsider | Match installed Aasimar/Tiefling behavior through the narrowest safe facts/type mechanism | ENGINE EVIDENCE PENDING |
| Race identity | Four distinct `BlueprintRace` references; use a safe donor `RaceId`, not a new enum member without an established project pattern | ENGINE EVIDENCE PENDING |
| Elemental affinity | +1 racial spell DC only for Fire, Acid, Electricity, or Cold respectively; exactly once; no caster-level bonus | APPROVED ADAPTATION |
| Ifrit SLA | Native Burning Hands, once daily, Charisma-based racial SLA DC, total character-level caster level | APPROVED |
| Oread movement | Base 20 feet plus native Dwarf-style armor/encumbrance immunity | OWLCAT PRECEDENT; ENGINE EVIDENCE PENDING |
| Oread SLA | Stone Fist once daily; use a safe local donor or narrowly reconstruct its exact behavior | ENGINE EVIDENCE PENDING |
| Sylph SLA | Feather Step replaces Feather Fall and must be named honestly in player text | APPROVED ADAPTATION |
| Undine swimming | Swim speed and Swim-as-class-skill clauses are descriptive no-ops because Kingmaker has no ordinary player swimming system | APPROVED OMISSION |
| Hydraulic Push | One-creature Bull Rush; bonus is total character level plus highest Intelligence, Wisdom, or Charisma modifier; no save or unrelated attack roll | APPROVED RECONSTRUCTION; ENGINE EVIDENCE PENDING |
| Languages | Not implemented | OUT OF SCOPE |
| Favored-class bonuses | Not implemented | OUT OF SCOPE |
| Alternate traits/heritages | Base heritage only | OUT OF SCOPE |
| Race feats/archetypes/dialogue rewrites | Not implemented | OUT OF SCOPE |
| Visual geometry | Vanilla Kingmaker modular assets only; no new body meshes, fins, crystals, flame hair, or persistent VFX | APPROVED CONSTRAINT |
| Portraits | No automatic race-specific portraits | OUT OF SCOPE |
| Donor dialogue consequences | Document exact known donor-`RaceId` misclassification; do not globally rewrite dialogue or person-spell checks | ACCEPTED LIMITATION PENDING DONOR |
