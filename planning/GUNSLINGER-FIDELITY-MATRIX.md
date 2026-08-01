# Base Gunslinger fidelity matrix

The authoritative local class text is
`C:\Dev\KingmakerGunslingerLab\private\rules\GUNSLINGER_PFSRD.md`. Alternative
deeds and archetype replacements are excluded from the base-class matrix.
Classifications are provisional until the implementation checkpoint records
and qualifies its exact Kingmaker mapping.

| Level | Feature | Classification | Current state | Required disposition / adaptation question |
|---|---|---|---|---|
| 1 | Weapon/armor/firearm proficiencies | EXACT | Native level-one preview contains exactly one production aggregate plus exact simple, martial, light-armor, and firearm proficiency facts; source isolation and cleanup reproduced twice | Preserve through creation commit, level-up, multiclass, and respec |
| 1 | Gunsmith and battered starting firearm | ADAPTED | Native `LevelUpHelper.AddStartingItems` grants exactly one production Early Pistol, one black-powder charge, and one lead ball on the real receiver; exact rollback and no-save behavior reproduced twice on `cc2f77d`; normal creation commit and battered/Gunsmith behavior remain unqualified | Qualify the normal creation-commit path, then implement only the established battered-state/Gunsmith equivalent |
| 1 | Grit: Wisdom-based pool, daily reset, critical/killing-blow recovery | EXACT | Runtime-qualified pool/rest/persistence and recovery. Exact confirmed critical and weapon-damage zero crossing restore separately; helpless/unaware/below-half-level targets are excluded; attack/target references weakly dedupe | Preserve through deed integration |
| 1 | Deadeye | ADAPTED | Runtime-qualified personal free action arms a native persisted fact; next successfully discharged exact firearm spends one grit per increment beyond first and extends touch AC without changing native range penalties; insufficient grit rejects atomically | Preserve through save/respec and final integrated combat regression |
| 1 | Gunslinger's Dodge | ADAPTED | Runtime-qualified drop-prone reaction: personal free action arms a persisted fact; the next ranged weapon attack spends one grit in light/medium armor at light load, applies native prone, and adds +4 AC exactly once; insufficient grit fails atomically | Implement the 5-foot movement / +2 AC alternative only after safe deterministic destination selection is established; Kingmaker has no Immediate action type |
| 1 | Quick Clear | ADAPTED | Runtime-qualified exact single-equipped-firearm actions: standard action requires positive grit without spending; move action spends one grit; both atomically change the item-owned misfire-origin Broken state to Normal without a repair kit | Preserve through save/respec and final integrated combat regression |
| 2/6/10/14/18 | Nimble +1..+5 | EXACT | Runtime-qualified five cumulative native Dodge-descriptor facts at exact levels; two fresh runs proved +5 in light/no armor, zero in medium armor, and native flat-footed exclusion | Preserve through level-up/respec and final integrated regression |
| 3 | Gunslinger Initiative | ADAPTED | Runtime-qualified +2: level-three feature adds +2 to the owning unit's exact native initiative roll while current grit is positive, with no spend and rule-object duplicate protection. The conditional firearm draw is OMITTED-NO-MEANINGFUL-INTERACTION because installed Kingmaker 2.1.7b has no player-facing Quick Draw feat; forcing a weapon-set switch would also bypass native action accounting | Revisit the draw clause only if the separately scoped feat work introduces Quick Draw with an exact free-hands/visible-firearm contract |
| 3 | Pistol-Whip | ADAPTED | Runtime-qualified standard-action ability requires exactly one equipped non-Wrecked firearm and one grit, then uses an unowned transient native melee surrogate: 1d6 one-handed or 1d10 two-handed, bludgeoning, 20/x2, firearm enhancement copied to attack/damage, and a free native Trip event on hit. Grit is spent on delivery even on a miss; ammunition, misfire condition, equipment, and persistence are unchanged | Preserve through level-up/respec and final integrated regression |
| 3 | Utility Shot: blast lock | OMITTED-NO-MEANINGFUL-INTERACTION | Exact installed metadata exposes locks through map interaction, Pick Lock, and Disable Device contracts, not firearm attack targets with lock AC, quality, jammed state, or destructible unlocking semantics | Revisit only if a supported firearm-targetable lock system is introduced |
| 3 | Utility Shot: scoot unattended object | OMITTED-NO-MEANINGFUL-INTERACTION | Exact installed metadata exposes no general Tiny-or-smaller unattended-object combat target with suppress-damage and 15-foot movement semantics | Revisit only if supported movable unattended combat objects are introduced |
| 3 | Utility Shot: stop bleeding | ADAPTED | Runtime-qualified standard-action self/adjacent ability requires positive grit without spending it, consumes exactly one loaded chamber without an attack roll or damage, and removes exactly one native `SpellDescriptor.Bleed` fact with rollback on delivery faults | Preserve through level-up/respec and final integrated regression |
| 4/8/12/16/20 | Bonus feats | EXACT | Not implemented | Firearm/combat feat selections and prerequisites |
| 5/9/13/17 | Gun Training selections | ADAPTED | Not implemented | Firearm group selection and Dex-to-damage using available weapon categories |
| 7 | Dead Shot | ADAPTED | Not implemented | Single native firearm attack action with BAB-derived rolls/damage and exact misfire logic |
| 7 | Startling Shot | ADAPTED | Not implemented | Pre-shot ability that intentionally deals no damage and applies flat-footed |
| 7 | Targeting: arms | ADAPTED | Not implemented | Debuff replacing unavailable item-drop interaction where required |
| 7 | Targeting: head | EXACT | Not implemented | Confused for 1 round on hit, immunity rules preserved |
| 7 | Targeting: legs | ADAPTED | Not implemented | Prone mapping; flying/non-locomotion immunity |
| 7 | Targeting: torso | EXACT | Not implemented | Threat range 19-20 for the deed attack only |
| 7 | Targeting: wings | OMITTED-NO-MEANINGFUL-INTERACTION | Not implemented | Kingmaker has no general winged-flight maneuver interaction; verify creature support |
| 11 | Bleeding Wound | ADAPTED | Not implemented | Pre-shot selection for HP bleed; ability-score bleed support requires contract review |
| 11 | Expert Loading | ADAPTED | Not implemented | Pre-shot grit spend/toggle prevents broken-firearm explosion |
| 11 | Lightning Reload | ADAPTED | Not implemented | Swift once/round; free-action case limited by engine action economy |
| 15 | Evasive | EXACT | Not implemented | Evasion, uncanny dodge, improved uncanny dodge while grit-positive |
| 15 | Menacing Shot | ADAPTED | Not implemented | Self-centered 30-foot fear burst; firearm discharge/ammunition treatment must be explicit |
| 15 | Slinger's Luck | ADAPTED | Not implemented | Pre-roll reroll toggle/ability; fixed non-reducible grit costs |
| 19 | Cheat Death | EXACT | Not implemented | Spend all remaining grit, minimum 1, remain at 1 HP |
| 19 | Death's Shot | ADAPTED | Not implemented | Pre-shot critical rider; death immunity and no grit recovery preserved |
| 19 | Stunning Shot | ADAPTED | Not implemented | Pre-shot rider, 2 grit, Fortitude negates, critical-immunity rule |
| 20 | True Grit | ADAPTED | Not implemented | Select two eligible deeds; reduce each by 1 grit with minimum 0 and explicit exclusions |
