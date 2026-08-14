# Elven Branched Spear Dexterity integration matrix

## Evidence boundary

This matrix is based on the guarded combined-profile blueprint inventory in
`runtime-evidence/20260813T2206508050890Z-observe-elven-branched-spear-contracts`,
the game assembly rule/component inventory, and the current local Call of the
Wild assembly. The foundation registration and module boundary passed guarded
Steam `mod-load-smoke` run
`20260813T2234240789658Z-e241b2c36ab34ae1afe5882ddc1615a9`.
Core combat rows were exercised by guarded Steam run
`20260814T0027460102758Z-disposable-elven-branched-spear-combat` with three
request-local live units, equipped item entities, native facts, and actual
attack/stat rule events. Save/load, respec, and optional-mod class-owner rows
remain separately identified below; registration evidence is not substituted
for those tests.

The spear is always two-handed and is never made light or one-handed. Its
custom category reports exactly `Melee`, `Finessable`, `TwoHanded`, `Exotic`,
and `Metal`.

## Audited sources

| Source | Owner | Engine surface and restrictions | Spear result | Integration | Runtime acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- |
| Strength baseline | Core combat | Native melee attack and damage default | Applies without a replacement | None | STR controls attack and damage without Weapon Finesse | Runtime PASS: Strength/Strength x1.5 |
| Weapon Finesse `90e54424d682d104ab36436bd527af09` | Native feat | `AttackStatReplacement(Dexterity, Finessable)`; changes attack only | Qualifies because the category is exactly `Finessable` | Category subcategory patch only | DEX attack, STR damage; compare Elven Curve Blade | Runtime PASS: Dexterity attack, Strength x1.5 damage |
| Rogue Finesse Training `b78d146cea711a84598f0acef69462ea` | Native rogue class feature | Static selection children use `WeaponTypeDamageStatReplacement`; native Elven Curve Blade child has `OnlyOneHanded=false`, `TwoHandedBonus=true` | Qualifies through the ordinary spear child; every item shares the category | Idempotently publish `KMG.ElvenBranchedSpear.FinesseTraining` | Exactly one option; DEX damage on all variants; respec/save/load; native two-handed multiplier comparison | Runtime PASS for exactly-one selector option and all 12 variants; respec/save-load pending |
| A class/archetype reusing native Finesse Training | Native or optional content | Same selection and same category replacement component | Qualifies if it grants/selects the native Finesse Training parent without adding another restriction | No extra child or bespoke feature | Construct each discovered owner and compare with its native qualifying weapon | Generic source complete; concrete runtime inventory pending |
| Agile `a36ad92c51789b44fa8a1c5c116a1328` | Native weapon enchantment | `WeaponDamageStatReplacement(Dexterity)` with `RequiresFinesse=true`; exact owning weapon; replaces only when DEX is better | Qualifies because the spear is finesse-compatible | Reference the native enchantment unchanged | DEX damage without Finesse Training; attack remains STR absent a valid attack-stat source | Runtime PASS: Dexterity x1, bonus 7 |
| Agile plus Finesse Training | Native enchantment plus class feature | Both call the native damage-stat replacement path | Qualifies, but native replacement semantics must select one stat source rather than add two modifiers | No stacking patch | One DEX modifier only; compare native Agile finesse weapon with Finesse Training | Runtime PASS: one Dexterity source, native Finesse Training x1.5, bonus 9 |
| Multiple category replacement facts | Native `WeaponTypeDamageStatReplacement` | Exact `WeaponCategory`, `OnlyOneHanded`, and native replacement arbitration | Qualifies only when the concrete fact names the spear category and allows two-handed use | Publish only the ordinary Finesse Training child; do not synthesize unrelated facts | Two equivalent facts do not double; switch/unequip clears selection | Agile plus category replacement PASS; two equivalent category facts and unequip persistence pending |
| Fighter's Finesse `c790786d2e2349ff9f6f20731a7c425a` | Call of the Wild advanced weapon training | `AttackStatReplacementIfWeaponTraining(Dexterity)`; requires Weapon Finesse and a qualifying weapon-training group | Qualifies through the native Spears fighter group | No hard dependency or selector copy; shared type has `WeaponFighterGroup.Spears` | DEX attack with Spears training; damage remains governed by its independent sources | Compatible optional route; runtime pending |
| Trained Grace `3bf81c936aac4e039eaa2ec032a34584` | Call of the Wild advanced weapon training | Adds trained-grace weapon-training value; it is not a generic DEX damage-stat replacement | May affect a spear only under its own weapon-training and attack/damage-stat rules | None | Compare to a native Spears-group finesse weapon; do not classify its bonus as DEX-to-damage | Compatible auxiliary route; runtime pending |
| Deadeye's Blessing `f0e3b832fd8d412b898810a8d3a14d8e` / Guided Hand `ad06b024a6cb4ea4919a65fcb4beaae2` | Call of the Wild feats | Wisdom attack replacement tied to deity favored-weapon parametrization | Not a Dexterity route; only applicable if some independent favored-weapon system legitimately names the spear | No special publication | Confirm no accidental DEX damage or bypass | Excluded from Dexterity matrix by stat/rules |
| Fencing Grace `47b352ea0f73c354aba777945760b441` | Native feat | `WeaponSubCategory.OneHandedPiercing`, free-hand/Grace semantics | Two-handed spear is ineligible | Do not add spear or weaken category | Not offered; cannot apply | Deliberately excluded |
| Slashing Grace `697d64669eb2c0543abb9c9b07998a38` | Native feat | `WeaponSubCategory.OneHandedSlashing`, Grace semantics | Piercing two-handed spear is ineligible | Do not add spear or weaken category | Not offered; cannot apply | Deliberately excluded |
| Dervish Dance `a18c439e0cca4232a067d13e6401d925` | Call of the Wild feat | Scimitar proficiency plus `DamageGraceForWeapon(Scimitar)` | Wrong named category and handedness contract | None | Spear absent and rejected | Deliberately excluded |
| Deft Strike `b63a316cb172c7b4e906a318a0621c2c` | Native Aldori feature | `WeaponTypeDamageStatReplacement(DuelingSword, OnlyOneHanded=true)` | Wrong category and one-handed restriction | None | Spear unaffected | Deliberately excluded |
| Feral Grace `9e8f5f85f4f84544ae14d8ba5dbb0ce2` | Call of the Wild parametrized feature | Natural-weapon selection plus animal/eidolon prerequisites | Manufactured spear is not natural | None | Spear absent and rejected | Deliberately excluded |
| Trained Throw `1a8c178b20644661be3b2770e7303d09` | Call of the Wild advanced weapon training | `WeaponCategoryGrace` for thrown-weapon training | Spear is explicitly not thrown | None | No effect | Deliberately excluded |
| Lesser Spirit Totem slam `97161df004c44010ba4e75b5af15c7bf` | Call of the Wild enchantment | CHA attack and damage replacement on its exact generated slam enchantment | Not a generic inventory-weapon enchantment | None | No effect | Deliberately excluded |
| Linnorm Style `7259551872374558a362b15dff1073b6` | Call of the Wild style | WIS damage replacement only against marked owner and only unarmed/feral-combat-training attacks | Manufactured spear is ineligible | None | No effect | Deliberately excluded |
| Sensei Insightful Strike `f4a3f9ede5a57c142b30a9dfbb8efa90` | Native monk feature | Wisdom attack replacement for Monk subcategory | Spear is not a monk weapon | None | No effect | Deliberately excluded |
| Zen Archery `d2baf00283974a659ed7bd23ea8c773c` | Call of the Wild/native-derived feature | Wisdom attack replacement for bows | Spear is melee and not a bow | None | No effect | Deliberately excluded |
| Unarmed Agile `90316f5801dbe4748a66816a7c00380c` | Call of the Wild equipment enchantment | Equipment replacement restricted to unarmed weapon type | Spear is ineligible | None | No effect | Deliberately excluded |
| Unearthly/Unholy Grace | Native creature features | Defensive Charisma-to-save/AC mechanics | Not weapon damage | None | No effect on spear attack/damage | Irrelevant |

## Acceptance matrix

The deterministic integrated scenario must retain the exact attack-stat,
damage-stat, stat multiplier, and final modifier source for each row:

| Fixture | Expected attack stat | Expected damage stat | Multiplicity |
| --- | --- | --- | --- |
| Spear, no finesse source | Strength | Strength | One native STR source |
| Spear, Weapon Finesse | Dexterity | Strength | One attack replacement only |
| Spear, Weapon Finesse plus Finesse Training | Dexterity | Dexterity | One damage replacement |
| Agile spear, no attack-stat source | Strength | Dexterity if native Agile's `RequiresFinesse` mechanics flag is satisfied by the weapon; Agile does not itself replace attack | One damage replacement |
| Agile spear plus Weapon Finesse | Dexterity | Dexterity | One of each |
| Agile spear plus Finesse Training | Dexterity when a valid attack source exists | Dexterity | Never two DEX damage modifiers |
| Call of the Wild Fighter's Finesse plus Spears training | Dexterity | Strength unless an independent valid damage source applies | One attack replacement |
| Any excluded Grace/named/light/natural route | Native baseline | Native baseline | Zero spear-specific replacements |

Every foundation and named spear uses the one stable category value
`0x004b4d47`; variant switching therefore cannot create a separate feat or
damage-stat family. No code changes native one-handed, light, free-hand,
scimitar, rapier, dueling-sword, bow, natural, or named-weapon restrictions.

## Guarded live observations

Run `20260814T0027460102758Z-disposable-elven-branched-spear-combat` passed all
assertions. With Strength 10, Dexterity 20, and BAB 12, the live observations
were:

- baseline: Strength attack, Strength x1.5 damage, stat bonus 0;
- Weapon Finesse: Dexterity attack, Strength x1.5 damage, stat bonus 0;
- Finesse Training: Dexterity x1.5 damage, stat bonus 7;
- native Agile alone: Dexterity x1 damage, stat bonus 7;
- Agile plus Finesse Training: Dexterity x1.5 damage, stat bonus 9, exactly one
  Dexterity damage source;
- every one of the six foundation and six named items: Dexterity x1.5 through
  the same selected Finesse Training category, with only its own enhancement
  added to the final bonus.

The same run observed the spear option exactly once in all seven parameterized
chosen-weapon selectors, Exotic Weapon Proficiency, and Rogue Finesse
Training. It also proved exact -4 nonproficiency for an untrained unit and a
blanket-martial-only unit, and no penalty with the exact exotic proficiency or
native Elven Weapon Familiarity. Save/load, respec, optional Call of the Wild
class construction, and deliberately excluded Grace menu execution remain in
the final compatibility workstream.

The subsequent three-phase working-save sequence proved that all 12 item
variants preserve their one category and exact registered blueprint identities
through a fresh save/load, including a load with the spear module disabled.
That closes variant ownership/category persistence; Finesse Training selection
respec and save/load remain a distinct pending player-build qualification.
