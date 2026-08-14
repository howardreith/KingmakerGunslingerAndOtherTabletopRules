# Elven Branched Spear named-item balance

## Pricing method

The catalog uses Kingmaker's native enhancement enchantments and the ordinary
magic-weapon formula, `2,000 gp x effective bonus squared`, added to the
320 gp masterwork weapon price. Cold iron adds the locked 20 gp surcharge.
Bespoke effects use a disclosed fixed premium rather than a hidden enhancement
bonus, so their engine enchantments have zero automatic enchantment cost and the
item blueprint owns the final price.

| Item | Native properties | Ordinary price | Bespoke premium | Final price | Rationale |
| --- | --- | ---: | ---: | ---: | --- |
| Boughkeeper | +1 | 2,320 gp | 3,000 gp | 5,320 gp | A conditional, nonstacking +1 dodge AC effect that requires an AoO hit and ends after one round is materially below a continuous +1 enhancement-equivalent defense. |
| Thornstep | +1 Keen (+2 equivalent) | 8,320 gp | 6,000 gp | 14,320 gp | The once-per-round 10-foot penalty requires the narrow movement-AoO trigger, lasts one round, and refreshes rather than stacks. |
| Moonlit Fork | +2 Agile (+3 equivalent), cold iron | 18,340 gp | 0 gp | 18,340 gp | It is entirely native Agile, enhancement, and cold-iron behavior; no bespoke premium is warranted. |
| Viper's Reach | +3 Agile Corrosive (+5 equivalent) | 50,320 gp | 20,000 gp | 70,320 gp | The -2 Reflex rider requires damage actually tagged and applied as sneak attack, is once per round, and lasts one round. |
| Briar-Crowned Spear | +4 Agile (+5 equivalent) | 50,320 gp | 22,000 gp | 72,320 gp | The generated -5 attack requires a preceding AoO hit, consumes a real remaining AoO, is once per round, and cannot recurse. |
| Spear of the First Branch | +5 Agile Speed (+9 equivalent), cold iron | 162,340 gp | 40,000 gp | 202,340 gp | The native properties reach but do not exceed the ordinary +10 weapon ceiling. Reprisal is once per round across both triggers, permits a Fortitude save, and gives only a one-round speed penalty on success. |

## Power and stacking boundaries

- Agile remains the native `WeaponDamageStatReplacement`; it is not copied or
  added as flat Dexterity damage. Rogue Finesse Training and Agile therefore
  participate in native replacement arbitration instead of stacking damage.
- The three penalty buffs use `StackingType.Replace`; reapplication refreshes
  duration and does not create another modifier.
- Boughkeeper's modifier is `Dodge`, but the buff itself cannot stack with
  another Boughkeeper application and becomes inactive when the exact item is
  not equipped.
- Fortuitous consumes the native AoO economy and marks the generated command so
  its -5 applies exactly once and cannot generate another attack.
- First Branch's Reprisal uses `10 + floor(character level / 2) + Dexterity
  modifier`, matching the approved scaling. Its failed-save condition reuses
  native Dirty Trick Entangled buff `3a6c5d8520c3b404883276590b086702`.

These are catalog prices, not evidence of campaign availability. Placement and
chapter timing are qualified separately in the placement manifest.

## Runtime behavior qualification

Guarded Steam run
`20260814T0057261040998Z-disposable-elven-branched-spear-combat` passed the
complete named-effect matrix on live request-local units. The observed values
matched the power boundaries used above: Boughkeeper contributed exactly +1
Dodge AC; Thornstep contributed exactly -10 Speed; Viper's Reach contributed
exactly -2 Reflex after a 15-point native sneak packet; Briar-Crowned's single
generated attack was exactly -5 and consumed the native AoO resource; First
Branch used DC 15, applied native Entangled on failure and -10 Speed on success,
and accepted a separate 13-point native sneak packet. Same-round repetitions
were suppressed, refreshable effects remained count one, and generated attacks
did not recurse.
