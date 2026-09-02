# Elemental Races deviation matrix

These adaptations are owner-approved. Local engine evidence may reject an
unsafe mechanism, but must not silently change the player-facing rule.

| Surface | Kingmaker implementation | Disposition |
| --- | --- | --- |
| Darkvision | Keen Senses and exactly +2 racial Perception | APPROVED ADAPTATION |
| Native outsider | Elemental outsider ancestry in player text, with the exact installed Aasimar/Tiefling creature-type precedent: neither donor base race nor its first heritage grants `OutsiderType`, and Hold/Charm/Enlarge/Reduce Person remain targetable. The four project races therefore do not add that fact. | NATIVE PERSON-SPELL AND PREREQUISITE MATRIX PASS; ENGINE LIMITATION DOCUMENTED |
| Race identity | Four distinct project `BlueprintRace` references using `RaceId.Aasimar`; no new enum member or broad race-check patch | IMPLEMENTED; DIALOGUE LIMITATION ACCEPTED |
| Elemental affinity | +1 racial spell DC only for Fire, Acid, Electricity, or Cold respectively; exactly once; no caster-level bonus. The component stores the authorized low-bit descriptor as a Unity-safe 32-bit mask because Kingmaker's `Int64`-backed enum cannot be serialized as a component field. | IMPLEMENTED; ACTUAL RULE-EVENT RUNTIME PASS |
| Ifrit SLA | Native Burning Hands, once daily, Charisma-based racial SLA DC, total character-level caster level | NATIVE COMMAND, CONE, SAVE, DAMAGE, RESOURCE, REST PASS; SAVE-BACKED RELOAD PENDING |
| Oread movement | Base 20 feet plus the exact native Dwarf Slow and Steady feature for armor/encumbrance immunity | NATIVE MEDIUM/HEAVY ARMOR, HEAVY EQUIPPED LOAD, AND GENERIC MODIFIER MATRIX PASS |
| Oread SLA | Sanitized native Kingmaker Stone Fist clone, one use per rest | NATIVE COMMAND, BUFF, UNARMED REPLACEMENT, DURATION, EXPIRY, RESOURCE, REST PASS; SAVE-BACKED RELOAD PENDING |
| Sylph SLA | Feather Step replaces Feather Fall and must be named honestly in player text | NATIVE COMMAND, BUFF, DURATION, EXPIRY, RESOURCE, REST PASS; SAVE-BACKED RELOAD PENDING |
| Undine swimming | Swim speed and Swim-as-class-skill clauses are descriptive no-ops because Kingmaker has no ordinary player swimming system | APPROVED OMISSION |
| Hydraulic Push | Native `ContextActionCombatManeuver` Bull Rush with caster level as base attack and best mental stat; no save or unrelated attack roll. An idempotent action commits the resource immediately before the synchronous native maneuver while ordinary resource logic retains availability gating. | NATIVE COMMAND, RESOURCE, FORMULA, SUCCESS/FAILURE, IMMUNITY, FORCE-MOVEMENT, NO-SAVE/ATTACK/AOO RUNTIME PASS; SAVE-BACKED RELOAD AND WATER PRESENTATION PENDING |
| Languages | Not implemented | OUT OF SCOPE |
| Favored-class bonuses | Not implemented | OUT OF SCOPE |
| Alternate traits/heritages | Base heritage only | OUT OF SCOPE |
| Race feats/archetypes/dialogue rewrites | Not implemented | OUT OF SCOPE |
| Visual geometry | Vanilla Kingmaker modular assets only; no new body meshes, fins, crystals, flame hair, or persistent VFX. Four body wrappers, twelve preset clones, and 28 stable equipment proxies use Human/Aasimar/Tiefling-compatible geometry plus audited native palettes. All 56 production race/sex/option cases, all 128 elemental Gunslinger outfit/equipment/rebuild states, all 80 exact race/sex/class-clothing cases across ten classes, and all 216 elemental idle/walk/run/turn/attack/reload/melee motion records render with complete materials. Remaining equipment, noncovered motion, persistence, and subjective acceptance remain pending. Kingmaker exposes no race-level eye-color choice array. | APPROVED CONSTRAINT; PRODUCTION ALL-OPTION, ELEMENTAL GUNSLINGER EQUIPMENT, TEN-CLASS CLOTHING, AND NATIVE-MOTION MATRICES PASS; EYE COLOR/HUMAN REVIEW LIMITATION DOCUMENTED |
| Portraits | No automatic race-specific portraits | OUT OF SCOPE |
| Donor dialogue consequences | `RaceId.Aasimar` checks can classify an elemental race as Aasimar; do not globally rewrite dialogue or person-spell checks | ACCEPTED LIMITATION |
