# Elemental Races deviation matrix

These adaptations are owner-approved. Local engine evidence may reject an
unsafe mechanism, but must not silently change the player-facing rule.

| Surface | Kingmaker implementation | Disposition |
| --- | --- | --- |
| Darkvision | Keen Senses and exactly +2 racial Perception | APPROVED ADAPTATION |
| Native outsider | Elemental outsider ancestry in player text, with the exact installed Aasimar/Tiefling creature-type precedent: neither donor base race nor its first heritage grants `OutsiderType`, and Hold/Charm/Enlarge/Reduce Person remain targetable. The four project races therefore do not add that fact. | NATIVE PERSON-SPELL AND PREREQUISITE MATRIX PASS; ENGINE LIMITATION DOCUMENTED |
| Race identity | Four distinct project `BlueprintRace` references using `RaceId.Aasimar`; no new enum member or broad race-check patch | IMPLEMENTED; DIALOGUE LIMITATION ACCEPTED |
| Elemental affinity | +1 racial spell DC only for Fire, Acid, Electricity, or Cold respectively; exactly once; no caster-level bonus. The component stores the authorized low-bit descriptor as a Unity-safe 32-bit mask because Kingmaker's `Int64`-backed enum cannot be serialized as a component field. | IMPLEMENTED; ACTUAL RULE-EVENT RUNTIME PASS |
| Ifrit SLA | Native Burning Hands, once daily, Charisma-based racial SLA DC, total character-level caster level | NATIVE COMMAND, CONE, SAVE, DAMAGE, RESOURCE, REST, TRANSITION, AND SAVE-BACKED MODULE-OFF RELOAD PASS |
| Oread movement | Base 20 feet plus the exact native Dwarf Slow and Steady feature for armor/encumbrance immunity | NATIVE MEDIUM/HEAVY ARMOR, HEAVY EQUIPPED LOAD, AND GENERIC MODIFIER MATRIX PASS |
| Oread SLA | Sanitized native Kingmaker Stone Fist clone, one use per rest | NATIVE COMMAND, BUFF, UNARMED REPLACEMENT, DURATION, EXPIRY, RESOURCE, REST, TRANSITION, AND SAVE-BACKED MODULE-OFF RELOAD PASS |
| Sylph SLA | Feather Step replaces Feather Fall and must be named honestly in player text | NATIVE COMMAND, BUFF, DURATION, EXPIRY, RESOURCE, REST, TRANSITION, AND SAVE-BACKED MODULE-OFF RELOAD PASS |
| Undine swimming | Swim speed and Swim-as-class-skill clauses are descriptive no-ops because Kingmaker has no ordinary player swimming system | APPROVED OMISSION |
| Hydraulic Push | Native `ContextActionCombatManeuver` Bull Rush with caster level as base attack and best mental stat; no save or unrelated attack roll. An idempotent action commits the resource immediately before the synchronous native maneuver while ordinary resource logic retains availability gating. No safe native water projectile was selected, so no projectile is claimed. | NATIVE COMMAND, RESOURCE, FORMULA, SUCCESS/FAILURE, IMMUNITY, FORCE-MOVEMENT, NO-SAVE/ATTACK/AOO, TRANSITION, AND SAVE-BACKED MODULE-OFF RELOAD PASS; WATER PROJECTILE OMITTED |
| Languages | Not implemented | OUT OF SCOPE |
| Favored-class bonuses | Not implemented | OUT OF SCOPE |
| Alternate traits/heritages | Base heritage only | OUT OF SCOPE |
| Race feats/archetypes/dialogue rewrites | Not implemented | OUT OF SCOPE |
| Visual geometry | Vanilla Kingmaker modular assets only; no new body meshes, fins, crystals, flame hair, or persistent VFX. Four body wrappers, twelve preset clones, and 28 stable equipment proxies use Human/Aasimar/Tiefling-compatible geometry plus audited native palettes. All 56 production race/sex/option cases, 224 expanded elemental Gunslinger equipment records, all 80 exact race/sex/class-clothing cases across ten classes, 216 native motion records, and 64 SLA/prone/death/resurrection/polymorph transition records render with complete materials. Save-backed module-OFF persistence also passes. Kingmaker exposes no race-level eye-color choice array. | APPROVED CONSTRAINT; ALL AUTOMATED VISUAL STRUCTURE MATRICES PASS; EYE COLOR/CLIPPING/AESTHETIC HUMAN REVIEW LIMITATION DOCUMENTED |
| Portraits | No automatic race-specific portraits | OUT OF SCOPE |
| Donor dialogue consequences | `RaceId.Aasimar` checks can classify an elemental race as Aasimar; do not globally rewrite dialogue or person-spell checks | ACCEPTED LIMITATION |
