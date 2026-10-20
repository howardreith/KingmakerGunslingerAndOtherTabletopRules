# Sprint 28 adult giant contract (implementation freeze)

Scope: Hill Giant and Stone Giant at SNA VI; Fire Giant at SNA VII. Each is an untemplated Large humanoid (giant), 10-foot space and reach. Fire also has the fire subtype. Monster rows, templates, Cloud/Storm/Frost/Ettin implementation, elder/class/advanced donors, and collectible summon gear are excluded. This freezes printed adult mechanics before registration; native implementation and qualification status are tracked separately. The installed melee weapon type subtracts four feet and combines its result with the giant Reach stat; project-owned greatclub, greatsword and slam types use raw ten-foot range so each equipped attack resolves to ten feet in native play, without changing donor types.

Primary PF1 sources: [Hill](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Hill%20Giant), [Stone](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Stone%20Giant), [Fire](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Fire%20Giant), [legacy Bestiary](https://legacy.aonprd.com/bestiary/giant.html), and [rock rules](https://legacy.aonprd.com/bestiary/universalMonsterRules.html). The adult entries agree with the mission cross-check; no source conflict was found in these values.

| Adult | Hill | Stone | Fire |
|---|---:|---:|---:|
| HD; BAB; hp | 10d8; +7; 85 | 12d8; +9; 102 | 15d8; +11; 142 |
| Str/Dex/Con/Int/Wis/Cha | 25/8/19/6/10/7 | 27/15/19/10/12/10 | 31/9/21/10/14/10 |
| Fort/Ref/Will | +11/+2/+3 | +12/+6/+7 | +14/+4/+9 |
| AC/touch/flat | 21/8/21 | 22/11/20 | 24/8/24 |
| Armor/natural/size | +4 hide/+9/-1 | 0/+11/-1 | +8 half-plate/+8/-1 |
| Speed | 40 ft.; 30 in hide | 40 ft. | 40 ft.; 30 in half-plate |
| Great weapon | greatclub +14/+9, 2d8+10 | greatclub +16/+11, 2d8+12 | greatsword +21/+16/+11, 3d6+15 |
| Alternative unarmed | two slams +13, 1d8+7 | two slams +16, 1d8+8 | two slams +20, 1d8+10 |
| Rock | +6, 1d8+10 | +11/+6, 1d8+12 | +10, 1d8+15 plus 1d6 fire |
| Rock increment | 120 ft. | 180 ft. | 120 ft. |
| Perception | +6 | +12 | +14 |
| Other skills | Climb +10, Intimidate +12 | Climb +12, Intimidate +12, Stealth +4 (+12 rocky terrain) | Climb +14, Craft (one) +8, Intimidate +11 |
| Languages | Giant | Common, Giant | Common, Giant |
| Senses | low-light | darkvision 60 ft., low-light | low-light |

The printed skill totals use the adult racial rank budget: Hill has 3 Perception, 4 Intimidate and 3 Climb ranks (10 total); Stone has 8 Perception, 9 Intimidate, 6 Stealth and 1 Climb rank (24 total); Fire has 9 Perception, 8 Intimidate, 8 Climb and 5 Craft ranks (30 total). Giant-subtype Intimidate and Perception each receive the class-skill +3 once when ranked. Hill's Intimidating Prowess adds its Strength modifier only to Intimidate. Stone's ordinary Stealth is 6 ranks +2 Dexterity -4 Large size = +4, then +8 only in the selected rocky-area settings. Climb includes its class-skill bonus and Hill/Fire armor check penalties. Fire's chosen Craft is not a general Knowledge bonus. The owner-approved Sprint 28 disposition leaves named Climb unmodeled for Hill,
Stone and Fire and named Craft (one) unmodeled for Fire. The printed values and
rank budgets above remain reference data, separate from implemented statistics.
No pit-only Climb adapter, new stat, rank reallocation or substitute bonus is
included. Ordinary native pit behavior remains unchanged. This can affect
printed climbing performance and does not establish that a pit consumer is
impossible. A future focused pit/climbing review is backlog, outside Sprint 28.

Hill's feats are Cleave, Intimidating Prowess, Martial Weapon Proficiency (greatclub), Power Attack, and Weapon Focus (greatclub). Stone's are Iron Will, Martial Weapon Proficiency (greatclub), Point-Blank Shot, Power Attack, Precise Shot, and Quick Draw. Fire's are Cleave, Great Cleave, Improved Overrun, Improved Sunder, Iron Will, Martial Weapon Proficiency (greatsword), Power Attack, and Weapon Focus (greatsword). Printed hit/damage numbers already include Weapon Focus where named, size, Strength, armor and other embedded modifiers; Stone's Iron Will is included in its printed Will save. Point-Blank Shot is conditional on distance and is not baked into Stone's printed rock attack. Live buffs and conditions must still flow through native consumers.

Attack decomposition: Hill greatclub BAB 7 + Str 7 - size 1 + Weapon Focus 1 = +14; rock BAB 7 + Dex -1 - size 1 + racial rock 1 = +6. Stone greatclub BAB 9 + Str 8 - size 1 = +16; rock BAB 9 + Dex 2 - size 1 + racial rock 1 = +11. Fire greatsword BAB 11 + Str 10 - size 1 + Weapon Focus 1 = +21; rock BAB 11 + Dex -1 - size 1 + racial rock 1 = +10. Two-handed club/sword and thrown rock use floor(1.5 x live Strength modifier), including signed odd values; primary slam uses the live modifier once. A normal one-attack action spends one attack. Stone's two rocks depend on Quick Draw/preparation; Hill and Fire do not gain repeated throws just from BAB. The generic rock damage formula does not replace the three explicit 1d8 lines.

Rock throwing has +1 racial to hit once, maximum five increments, and native range/cover/concealment/line-of-effect constraints. The summoned attack uses no collectible ammunition. A fire giant's heated rock adds 1d6 fire on an actual hit only; fire immunity and cold vulnerability remain separate from melee weapon properties. Rock catching is once per round for an aware Large-or-larger would-be-hit target and an incoming Small/Medium/Large rock or similar projectile. Its Reflex DC is 15/20/25 by projectile size plus the projectile's magical attack enhancement; Stone gets +4 only on this catch attempt. Catch success prevents that attack's harm at the valid pre-damage seam and produces no inventory object.

The eleven SNA roots are Hill VI one, VII 1d3, VIII/IX 1d4+1; Stone the same; Fire VII one, VIII 1d3, IX 1d4+1. Source-derived totals must be recomputed after integration, with the existing 29 native wrappers left alone. Registration remains inert until icons, valid donor dependencies and owned equipment/rock behavior are ready.

Implementation status and native qualification are tracked in
[SPRINT28-STATUS.md](SPRINT28-STATUS.md). The owner authorized a project-owned
Hill/Fire Cleave correction; no global native Cleave, Overrun or durability
project is authorized. A spent Cleave Standard action applies one native-duration
−2 AC penalty even when its first attack misses; a miss produces no follow-up.
The printed Climb/Craft omission above is the only new
owner-approved gameplay omission for Sprint 28.
