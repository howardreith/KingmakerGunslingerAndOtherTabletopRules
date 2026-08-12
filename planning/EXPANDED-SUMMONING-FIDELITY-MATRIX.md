# Expanded Summoning fidelity matrix

Status: incremental; template/alignment, seven custom special creatures, and
the complete tier I-VII natural/proxy group are final-live structure-qualified. Runtime
cast, visual, persistence, and compatibility columns remain open unless
explicitly marked.

Each catalog row will record family/tier, creature and template/alignment policy,
native reuse or donor GUID/name/view, frozen KMG unit and ability identities,
size/reach/speed/movement, ability scores and combat statistics, attacks,
defenses, senses, feats, special abilities, removed donor behavior, deviations,
and structural/runtime/visual/compatibility evidence.

No omitted or conservatively adapted mechanic will be described as implemented.

Primary rules references:

- [Lantern Archon](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lantern+Archon)
- [Mephit](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Mephit)
- [Elemental](https://legacy.aonprd.com/bestiary/elemental.html)
- [Boar](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Boar)
- [Leopard](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Leopard)
- [Monitor Lizard](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Monitor+Lizard)
- [Cheetah](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Cheetah)
- [Crocodile](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Crocodile)
- [Dire Bat](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Bat)
- [Wolverine](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Wolverine)
- [Dire Boar](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Boar)
- [Dire Wolf](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Wolf)
- [Grizzly Bear](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Grizzly+Bear)
- [Lion](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lion)
- [Pteranodon](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Pteranodon)
- [Dire Lion](https://legacy.aonprd.com/bestiary/lion.html)
- [Ankylosaurus](https://legacy.aonprd.com/bestiary/dinosaur.html)
- [Dire Bear](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Bear+%28Cave+Bear%29)
- [Dire Tiger / Smilodon](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Tiger+%28Smilodon%29)
- [Elephant](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Elephant)
- [Mastodon](https://legacy.aonprd.com/bestiary/elephant.html)
- [Roc](https://legacy.aonprd.com/bestiary/roc.html)

## Native dedicated summon reuse

| Entries | Families/tiers | Implementation | Sanitization | Evidence |
|---|---|---|---|---|
| Air, earth, fire, water elementals; Small through Elder (24 unique units) | SM II/IV/V/VI/VII/VIII and matching SNA tiers | Exact Owlcat dedicated summon donor for every element/size; KMG freezes one shared family-neutral unit identity per creature | XP/loot/inventory/campaign surfaces stripped; class spell arrays empty; no celestial/fiendish template applied | Exact 24-key immutable profile; donor graph PASS `20260811T2055502086857Z`; source tests PASS; actual casts/visual/profile runs pending |
| Air, earth, fire, water mephits (4 unique units) | SM IV and SNA IV | Exact Owlcat dedicated summon donor preserves Small outsider chassis, two claws, breath weapon, elemental facts, DR 5/magic, and native spell-like abilities | XP removed; no summon/conjure or planar-travel fact appears in selected donor facts; source component arrays independently cloned | Exact 4-key immutable profile; donor graph PASS `20260811T2055502086857Z`; source tests PASS; environmental fast-healing deviation and actual casts pending |

The native mephit units grant unconditional Fast Healing 2. Tabletop restricts
fast healing to element-specific environments. Kingmaker has no safe local
environment predicate established yet; retaining Owlcat's native practical
adaptation is explicitly recorded pending mechanical runtime qualification.

## Low-tier natural reconstruction

| Creature | Families/tiers | KMG unit | Delivered chassis/offense | Deviation | Qualification |
|---|---|---|---|---|---|
| Dog | SM I; SNA I | `e8c90cb29374455cb6301e4fa7d1f837` | Small animal 1; Str 13/Dex 13/Con 15/Int 2/Wis 12/Cha 6; speed 40; bite 1d4; Perception focus | None structurally identified | Structural PASS `20260812T0010300046437Z`; actual cast/visual pending |
| Eagle | SM I; SNA I | `7383db28c1d74dce98533ddc257a2e3c` | Small animal 1; 80-foot airborne movement; bite and two 1d4 talons; Weapon Finesse | Separate 10-foot ground speed omitted; Roc visual still requires scale/navigation proof | Structural PASS; actual cast/visual pending |
| Poisonous Frog | SM I; SNA I | `e1a8e5e206154dd48b6aca1d4262e8e7` | Tiny animal 1; bite 1; native six-tick 1d2 Constitution poison, Fortitude, one save cures | Swim movement omitted | Structural PASS; poison use/cast/visual pending |
| Giant Centipede | SM II; SNA I | `baf9e8f829e9410db8f3d200bb62a2c6` | Medium vermin 1; speed 40; bite 1d6-1; native six-tick 1d3 Dexterity poison; eight-leg trip defense | Int 1 represents absent Intelligence; climb omitted; native poison lacks the tabletop +2 racial DC bonus | Structural PASS; poison use/cast/visual pending |
| Giant Spider | SM II; SNA II | `4a3cd49e751448c8b8836485b262fdf1` | Medium vermin 3; bite 1d6; native four-tick 1d2 Strength poison; natural armor +1 | Int 1 represents absent Intelligence; web, climb, and tremorsense omitted pending bounded contracts | Structural PASS; poison use/cast/visual pending |
| Goblin Dog | SM II; SNA II | `f1584066792a436fa3a5ba0b3731b481` | Medium animal 1; speed 50; bite 1d6+3; Toughness; Worg view only | Disease immunity and allergic reaction omitted pending safe exact contracts | Structural PASS; actual cast/visual pending |
| Hyena | SM II; SNA II | `ec12fab8be5c412d8ee824d15a6621d0` | Medium animal 2; speed 50; bite 1d6+3 with native trip; Perception focus | Wolf view only | Structural PASS; actual cast/visual pending |

All 67 KMG summon units carry hidden marker
`KMG.Summoning.Subtype.Extraplanar` (`1812739855844dc4adf3c32a70f13512`)
exactly once. This provides deterministic standalone subtype metadata without
requiring CotW's later-loaded feature during bootstrap; native optional-marker
reconciliation remains open.

## Tier III-IV natural and proxy reconstruction

The donor in every row supplies the selected view/rig only. KMG replaces class
levels, stats, size, speed, body weapons, facts, inventory, brain, XP, loot,
tags, and campaign behavior from the immutable profile. All rows share the
frozen quantity abilities generated for their higher-tier placements.

| Creature | Families/tier | KMG unit; visual donor | Delivered chassis/offense | Conservative deviation | Qualification |
|---|---|---|---|---|---|
| Boar | SM III; SNA III | `d98699ea265441a09c5b9b51769ef7ce`; `5f968d63d756f994ebff0d774e88e4ab` | Medium animal 2; 17/10/17/2/13/4; speed 40; NA +4; 1d8 gore; Ferocity, Toughness | None structurally identified | Structural PASS `20260812T0031212209441Z`; cast/visual pending |
| Leopard | SM III; SNA III | `f36aad1d463444c69950f219457b894f`; `768275c9885dd954fb3c84ba69ac4281` | Medium animal 3; 16/19/15/2/13/6; speed 30; NA +1; bite 1d6, two claws plus native extra rake limbs; Pounce, Weapon Finesse | Native four-claw body is Owlcat's rake adaptation; grab omitted because the only proven generic graph includes unrelated constrict state | Structural PASS; charge/full-attack cadence, cast/visual pending |
| Monitor Lizard | SM III; SNA III | `10b148d216364b509cd2c665a47b2950`; `4109b40f6bbb49640840644cc84ada67` | Medium animal 3; 17/15/17/2/12/6; speed 30; NA +3; bite 1d8; exact native Constitution-scaled poison; Great Fortitude | Swim and grab omitted | Structural PASS; poison/cast/visual pending |
| Cheetah | SM III; SNA III | `6eb29e76792c41c5ab9e3812fe4b66e0`; Leopard view `768275c9885dd954fb3c84ba69ac4281` | Medium animal 3; 17/19/15/2/12/6; speed 50; NA +1; bite 1d6 with trip, two 1d3 claws; Weapon Finesse, Improved Initiative | Once-per-hour tenfold sprint omitted; no bounded native cooldown contract proven | Structural PASS; cast/visual pending |
| Crocodile | SM III; SNA III | `ed3fa562802b418ab062a7da622874da`; Monitor Lizard view `4109b40f6bbb49640840644cc84ada67` | Large animal 3; 19/12/17/1/12/2; speed 20; NA +4; bite 1d8 and secondary KMG 1d12 tail `d7ec01bae32a4d9086214f156ce52ecd` | Swim, grab, death roll, sprint, and hold breath omitted pending summon-safe target-state and movement contracts | Structural PASS; secondary cadence/cast/visual pending |
| Dire Bat | SM III; SNA III | `d867cb795b5640219a8362661f447697`; Roc-compatible Giant Eagle view `406c1e1af5400ac4881e330502ccbd9e` | Large animal 4; 17/15/13/2/14/6; 40-foot airborne movement; NA +3; bite 1d8; Stealthy | 20-foot ground mode, blindsense, and Alertness omitted | Structural PASS; scale/navigation/cast/visual pending |
| Wolverine | SM III; SNA III | `f640d3e77d7d4a0d8de3351129cd7148`; Worg view `313a17cbd273d1f40bd1654ee2ae186e` | Medium animal 3; 15/15/15/2/12/10; speed 30; NA +2; two 1d6 claws and secondary 1d4 bite; Toughness | Burrow/climb and after-damage rage omitted pending a save/load-safe summon-local rage state | Structural PASS; claw animation/cast/visual pending |
| Dire Boar | SM IV; SNA IV | `1710475cee544d9d858b05b24fb3ad4c`; `6ec9c63c41a1e754ea4dcd85557625b4` | Large animal 5; 23/10/17/2/13/8; speed 40; NA +6; 2d6 gore; Ferocity, Improved Initiative, Toughness | None structurally identified | Structural PASS; cast/visual pending |
| Dire Wolf | SM IV; SNA IV | `d2e7b46ea8994f7085063abac3775142`; `03dd28e92faf2e44eb9564a6ba01fdd0` | Large animal 5; 19/15/17/2/12/10; speed 50; NA +3; bite 1d8 with trip; Perception and bite focus | Run omitted because no exact final-live feature identity was proven | Structural PASS; trip/cast/visual pending |
| Grizzly Bear | SM IV; SNA IV | `f7370368039e41ba8f88e8218cfb39d0`; `0b214d8e81a563549ba0be37cd1c16d0` | Large animal 5; 21/13/19/2/12/6; speed 40; NA +6; bite and two 1d6 claws | Claw grab, Endurance, Run, and Survival focus omitted; generic grab carries unrelated constrict state | Structural PASS; full attack/cast/visual pending |
| Lion | SM IV; SNA IV | `14c7cbb0f32f4e30bbefaeaccba10269`; Leopard view `768275c9885dd954fb3c84ba69ac4281` | Large animal 5; 21/17/15/2/12/6; speed 40; NA +3; bite 1d8, two claws plus native extra rake limbs; Pounce | Native four-claw body is Owlcat's rake adaptation; grab and Run omitted | Structural PASS; charge/full-attack cadence, scale/cast/visual pending |
| Pteranodon | SM IV; SNA IV | `c9a94142c9164ab793f7a06ae3fdcf56`; Roc-compatible Giant Eagle view `406c1e1af5400ac4881e330502ccbd9e` | Large animal 5; 16/19/15/2/15/12; 50-foot airborne movement; NA +2; bite 2d6; Dodge, Improved Initiative | Separate 10-foot ground mode omitted | Structural PASS; reach/scale/navigation/cast/visual pending |

The fresh-process assertion compares every row's HD/class, size, six ability
scores, speed, primary/additional/secondary weapon references, natural armor,
feat/special-fact GUIDs, extraplanar marker, and empty inventory against the
checked-in catalog. It also verifies the KMG crocodile tail is exactly 1d12.

## Tier V-VII natural and proxy reconstruction

The final natural group uses the same immutable reconstruction contract: the
donor supplies only the view/rig, while KMG owns all HD, stats, body weapons,
facts, brain, inventory, and alignment state. The three KMG weapon identities
below preserve proven native animation categories while freezing tabletop dice.

| Creature | Families/tier | KMG unit; visual donor | Delivered chassis/offense | Conservative deviation | Qualification |
|---|---|---|---|---|---|
| Dire Lion | SM V; SNA V | `56c64aa6765a4c37a1b30c0c5b31427b`; Smilodon `beae4985629a6f64eb98081e3171e4c1` | Large animal 8; 25/15/17/2/12/10; speed 40; NA +4; bite 1d8, two 1d6 claws plus two secondary rake claws; Pounce, Improved Initiative, Perception/claw focus | Grab and Run omitted; secondary rake cadence requires actual-charge proof | Structural PASS `20260812T0045336396930Z`; cast/visual pending |
| Ankylosaurus | SM V; SNA V | `10c80d5cfa594332bd3e5127799e426f`; Hodag `c3524f96954a1d94f8525b86e7626633` | Huge animal 10; 27/10/17/2/13/8; speed 30; NA +14; KMG 3d6 tail `15394605e1664a51bce4b50f38a7603a`; Great Fortitude, Power Attack | Strength-based daze/stun rider omitted pending a proven bounded native Dazed contract; bull-rush/overrun/tail-focus identities unproven | Structural PASS; tail animation/cast/visual pending |
| Dire Bear | SM VI; SNA VI | `11b15a81cea1498babfdb57af1b53c41`; `260da5b557e3fb04bb4960a36a5d1dc4` | Large animal 10; 25/13/21/2/12/10; speed 40; NA +8; bite 1d8, two claws 1d6; Improved Initiative, Iron Will, Perception focus | Grab, Endurance, and Run omitted | Structural PASS; full attack/cast/visual pending |
| Dire Tiger / Smilodon | SM VI; SNA VI | `d15ee151c2274f9f86ab523f111bc3af`; `beae4985629a6f64eb98081e3171e4c1` | Large animal 14; 27/15/17/2/12/10; speed 40; NA +6; 2d6/19-20 bite, two 2d4 claws plus two secondary rake claws; Pounce and exact critical/focus feats | Grab and Run omitted; secondary rake cadence requires actual-charge proof | Structural PASS; charge/full-attack/cast/visual pending |
| Elephant | SM VI; SNA VI | `9dd8544097234f05bcd35d400d91b510`; Mastodon `028cc6f46e7998f46855a33ffde89567` | Huge animal 11; 30/10/19/2/13/7; speed 40; NA +9; native 2d8 gore and secondary 2d6 slam; Great Fortitude, Iron Will, Power Attack, Perception focus | Trample omitted pending a commandable path-safe movement contract; Endurance and Improved Bull Rush identities unproven | Structural PASS; gore/slam/cast/scale pending |
| Mastodon | SM VII; SNA VII | `e129ffc2768d47c5bee61bb99b0c8703`; dedicated summon `028cc6f46e7998f46855a33ffde89567` | Huge animal 14; 34/12/21/2/13/7; speed 40; NA +12; native 2d8 gore and secondary 2d6 slam; Iron Will, Power Attack, Perception focus | Trample omitted; Endurance, Improved Bull Rush/Will, and gore-focus concrete identities unproven | Structural PASS; gore/slam/cast/scale pending |
| Roc | SM VII; SNA VII | `439e955cb0fd41daafd0478d3641615a`; Giant Eagle/Roc rig `406c1e1af5400ac4881e330502ccbd9e` | Gargantuan animal 16; 28/15/17/2/12/11; 80-foot airborne movement; NA +14; KMG 2d8 bite `c19d1025fe2b47769c93a3b76d0c052c` and two 2d6 talons `8a3741a7598147baa08de552565635ad`; exact critical/initiative/save/focus feats | Separate ground speed, talon grab, and Flyby Attack omitted | Structural PASS; footprint/reach/navigation/camera/cast/visual pending |

Fresh Steam-backed run `20260812T0045336396930Z` passed all 29 assertions on
exact source `3c2c5fef82a7d9b032f7da906385013a5699cc8c`. It checked every
profile field and attack reference, exact 3d6/2d8/2d6 custom weapon dice, all
681 placements, the constant 1,403 registry, and zero donor component aliases,
forbidden references, inherited spells, inventory, or native-action
contamination. No save was accessed.

## Lantern Archon

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster III; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `02f8e9c6c91549deaded9ef667399449` (`KMG.Summoning.Unit.LanternArchon`) |
| Visual donor | `24719a49b84c5cd43b894268d22d9c89` (`CR6_WillOWispStandart`), prefab `8a8d7c448ff2c8749adc08eeb223333b`; visual only |
| Chassis | 2 outsider HD; Small; lawful good; Str 1, Dex 11, Con 12, Int 6, Wis 11, Cha 10; 60-foot movement using airborne native navigation |
| Offense | KMG ray `d4c2ce6c90094fdfb0fd908312372d72`; two native projectile/ranged-touch attack rolls, each 1d6 direct damage, custom 30-foot range |
| AI | KMG action `3579bfa7c4b040c4812286f4ade47146` and single-action brain `427b496a05db48aa94997415f1a74c39`; no Ghaele spell AI |
| Defenses | Natural armor +4; electricity immunity; DR 10/evil; +4 racial vs poison; +2 resistance saves and +2 deflection AC vs evil through KMG buff `4c55af41c90443c18267a806c740ce16` |
| Traits/aura | Good, lawful, extraplanar, airborne facts; native Aura of Menace area carrier `1ce4878b5e714f659d0854a12f4b3cf2` |
| Removed donor mechanics | Wisp HD/type mechanics, touch weapons, invisibility, spell immunity, ambush/tags/brain; all Ghaele weapons, spells, gaze, inventory, and azata facts |
| Conservative deviations | Greater teleport and gestalt omitted by summon safety contract. No distinct native Archon subtype fact was found; outsider HD plus explicit lawful/good/extraplanar traits implement the mechanical type surface. Low-light/darkvision and truespeech are not separately added because no safe bounded unit fact has yet been proven. |
| Qualification | Static/domain `1006/1006 PASS`; clean Release and strict package PASS. Exact final-live structure PASS `20260811T2238575798728Z`; actual cast, projectile/view, aura behavior, cleanup, persistence, and compatibility pending. |

## Invisible Stalker

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster VI; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `3cd7b4f2c65b4d35929dd4d969dfaa41` (`KMG.Summoning.Unit.InvisibleStalker`) |
| Visual donor | `2e24256e459468743b91fbb9aa85e1ab` (`SummonedAirElementalHuge`); prefab only, with the KMG chassis reset to Medium |
| Chassis | 7 outsider HD; Medium neutral; Str 18, Dex 19, Con 22, Int 14, Wis 15, Cha 11; 30-foot airborne movement |
| Offense | Two native `AirElementalSlam_Large` attacks, matching the intended 2d6 slam dice; Combat Reflexes and Weapon Focus (slam) |
| Defenses/traits | Natural armor +6; air, elemental, and extraplanar traits; Improved Initiative and Lightning Reflexes; native `NaturalInvisibilityBuff` whose exact graph preserves invisibility after offensive actions |
| Removed donor mechanics | Huge size/stats/HD, whirlwind, air mastery, elemental brain, enemy scaling, XP/loot, and donor class progression |
| Conservative deviations | Dedicated tracking behavior and scent are omitted because no bounded native tracking fact was proven. The air-elemental view must still pass scale, footprint, navigation, and animation qualification. |
| Qualification | Static/domain `1006/1006 PASS`; clean Release and strict package PASS. Exact final-live structure PASS `20260811T2207541420526Z`; actual cast/combat/visual/persistence/profile runs pending. |

## Bebelith

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster VII; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `9d7d36e71ea141258509b8a32557577e` (`KMG.Summoning.Unit.Bebelith`) |
| Visual donor | `51c66b0783a748c4b9538f0f0678c4d7` (Doomspider); view/rig only, enlarged through the KMG Huge chassis |
| Chassis | 12 outsider HD; Huge chaotic evil; Str 28, Dex 12, Con 24, Int 11, Wis 13, Cha 13; 40-foot ground movement; natural armor +13; DR 10/good |
| Offense | Two KMG 2d4 claws (`85971d6300dd41a0a62b0dd92a570045`) and a native-animation 2d6 Huge bite; +2 attack and damage against exact chaotic-evil outsiders |
| Dismantle | The second same-target claw hit in one round triggers Reflex DC 25; failure applies KMG state `736a349933f24cc2b11bc284b6e559cb` for one round and reduces AC by 2 without changing the equipped item |
| Removed donor mechanics | Doomspider poison, web and web immunity, donor HD/stats/type/brain, enemy scaling, XP/loot/inventory, and campaign behavior |
| Conservative deviations | Permanent armor destruction is replaced with a bounded one-round AC penalty to preserve inventory/save safety. Demon hunting keys from exact chaotic-evil outsider facts and grants +2 attack/damage. Rot and climb are omitted because no safe bounded native implementation has been proven. |
| Qualification | Static/domain `1006/1006 PASS`; clean Release and strict package PASS. Exact final-live structure PASS `20260811T2310424930290Z`; actual claw sequencing, save/effect, combat, scale/navigation, visual, persistence, and profile runs pending. |

## Pixie

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Nature's Ally IX |
| Unit identity | `396881ade24e4ddba188dc2e7ff481f9` (`KMG.Summoning.Unit.Pixie`) |
| Visual donor | `394610e32cfbc4f43a0efaab16faae49` (`CR1_Nixie`), prefab `6d6f3b81a5b50534399bb5cb778cd4e0`; view/rig only |
| Chassis | 4 fey HD; Small neutral good before SNA spawn-local caster alignment; Str 7, Dex 21, Con 12, Int 16, Wis 15, Cha 16; 60-foot airborne movement; natural armor +1; DR 10/cold iron; SR 15; attack-safe native natural invisibility |
| Sleep arrows | Body-mounted KMG longbow `0a1d8ac4be724595aa952471a9491975` uses the native arrow rig and deals zero weapon dice; 16 save-backed resource uses, Will DC 15, native Sleeping state for 50 rounds on failure; no ammunition item or inventory transfer |
| Irresistible dance | KMG spell-like ability `5cacf6a72c724b7fad8d7605cba1e790`, one resource-backed use, CL 8/spell level 6, native touch delivery and dance state; 1d4+1 rounds on failed Will save and one round on success |
| Removed donor mechanics | Donor HD/stats/class progression/spells/brain/inventory/campaign behavior; no teleportation, summon/conjuration, permanent ammunition, transferable loot, or persistent external effect |
| Conservative deviations | The no-damage sleep bow preserves projectile/animation behavior while keeping the special effect bounded. The exact fey view still requires scale, projectile socket, casting, hit, death, navigation, and selection-circle qualification. |
| Qualification | Static/domain `1006/1006 PASS`; clean Release and strict package PASS. Exact final-live structure PASS `20260811T2310424930290Z`; actual bow/dance use, resource persistence, animation/projectile, combat, cleanup, save/load, and profile runs pending. |

## Salamander

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster V; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `f8fb103168d74b4c93182437e5d2b4e4` (`KMG.Summoning.Unit.Salamander`) |
| Visual donor | Frozen Lizardfolk donor; view/rig only, with all donor equipment, progression, inventory, and campaign behavior removed |
| Chassis | 8 outsider HD; Medium chaotic evil; Str 16, Dex 13, Con 18, Int 14, Wis 15, Cha 13; 20-foot movement; natural armor +7; fire and extraplanar traits; DR 10/magic |
| Offense | Native standard spear in the primary hand; KMG tail weapon `93e097b8d3db42d3a37656502899e1a9` dealing 2d6; bounded combat fact `a47bc65d6b6b42b6a19610e22b13f171` supplies one hit-confirmed 1d6 fire rider and a cloned grab/constrict graph dealing 2d6+4 |
| Removed donor mechanics | Lizardfolk stats, HD, inventory, drops, class progression, donor brain, and campaign surfaces; no planar travel, summoning, or unrelated poison/web/spell behavior |
| Conservative deviations | The engine graph models spear and tail through native weapon slots and bounded hit triggers; iterative attack cadence and grab target cleanup require actual-cast/runtime combat proof. Cold vulnerability is not yet separately represented because no exact safe bounded fact has been proven. |
| Qualification | Static/domain `1006/1006 PASS`; clean Release and strict package PASS. Exact final-live structure PASS `20260811T2238575798728Z`; actual cast/combat/visual/persistence/profile runs pending. |

## Succubus

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster VI; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `0c908145873f4b67a188397ca5f46da1` (`KMG.Summoning.Unit.Succubus`) |
| Visual donor | Frozen Nymph-preferred donor; view/rig only, with all donor spells, gaze, class progression, inventory, and campaign behavior removed |
| Chassis | 8 outsider HD; Medium chaotic evil; Str 13, Dex 17, Con 14, Int 18, Wis 13, Cha 27; 30-foot movement; natural armor +7; chaotic, evil, extraplanar traits |
| Offense | Two native 1d6 claws; bounded Dominate Person spell-like ability `1662d63944d94cdeaa62562dc9ac9349` with Charisma parameters, humanoid-only target contract, Will save, and three-round duration; single-action AI/brain `8109da6090a64cbcb02326fb08e8ce1f` / `38e57062576e4c9e97c2982972c81328` |
| Defenses/drain | DR 10/cold iron or good; acid/cold resistance 10; fire/electricity/poison immunity; SR 18. Combat fact `cd51ad31f8764d2797b59eb43da7a9f8` applies one temporary negative level for one round on the first qualifying hit only. Domination buff `6e1f6eb3e773451dbda9e0ecd07486d9` removes itself if the summoned caster disappears. |
| Removed donor mechanics | Nymph spells/gaze/class facts and campaign surfaces; native one-day/permanent-capable energy drain was rejected; teleportation, summoning, permanent profane gift, and external persistent target state are absent |
| Conservative deviations | Charm and energy-drain identity are represented by bounded domination and a one-round temporary first-hit drain. Profane gift is omitted because it can outlive the summon. Exact humanoid targeting, save cadence, and cleanup still require actual-cast/save-load proof. |
| Qualification | Static/domain `1006/1006 PASS`; clean Release and strict package PASS. Exact final-live structure PASS `20260811T2238575798728Z`; actual cast/combat/visual/persistence/profile runs pending. |

## Shadow Demon

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster VI; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `627c1841a5eb4e32b1a94c0f43ec8a60` (`KMG.Summoning.Unit.ShadowDemon`) |
| Visual donor | `1832be68f9814254dbbdab6df7fd5d0b` (`SoulEaterSummoned`); view only |
| Chassis | 7 outsider HD; Medium chaotic evil; Str 17, Dex 20, Con 14, Int 14, Wis 13, Cha 17; 40-foot airborne movement |
| Offense | Claw/claw/bite using native 1d6 claw and 1d8 bite weapons; KMG combat-traits fact `f81993d391054678a138227b91141eae` adds 1d6 cold to hit-confirmed natural attacks |
| Defenses/traits | Native incorporeal damage handling and critical/precision immunity; DR 10/cold iron or good; acid/fire resistance 10; cold/electricity/poison immunity; SR 17; chaotic, evil, extraplanar facts |
| Removed donor mechanics | Soul Eater HD/stats, Wisdom-damage feature, all-around vision, DR/magic, campaign facts, and donor brain; teleportation and summon/conjuration are absent |
| Conservative deviations | Possession is omitted because no duration-bound, save/load-safe control transfer has been proven. Shadow blend and sprint are omitted pending safe light-state and cooldown primitives. Demon subtype is represented by outsider plus chaotic/evil/extraplanar facts because no exact standalone native Demon subtype fact was found. |
| Qualification | Static/domain `1006/1006 PASS`; clean Release and strict package PASS. Exact final-live structure PASS `20260811T2207541420526Z`; actual cast/combat/visual/persistence/profile runs pending. |
