# Sprint 11 ungulate rules and installed-engine audit

Status: implementation in progress. This is a read-only rules/engine inventory,
not acceptance of creature mechanics or visuals.

## Tabletop contract

The [Paizo herd-animal entries](https://legacy.aonprd.com/bestiary/herdAnimal.html)
specify Large Aurochs (3 animal HD, Str 23, speed 40, +4 natural armor,
1d8+9 gore, 2d6+9 trample DC 17) and Large Bison (5 HD, Str 27, speed 40,
+8 natural armor, 2d6+12 gore, 2d6+12 trample DC 20). Their stampede
requires at least three adjacent trampling herd animals; it can then affect
same-size foes and raises the save DC by two.

The [Paizo rhinoceros entries](https://legacy.aonprd.com/bestiary/rhinoceros.html)
specify Large Rhinoceros (5 HD, Str 22, speed 40, +7 natural armor,
2d6+9 gore, 4d6+12 powerful charge) and Large Woolly Rhinoceros (8 HD,
Str 28, speed 30, +10 natural armor, 2d8+13 gore, 4d8+18 powerful charge,
2d6+13 trample DC 23).

The [Paizo universal rules](https://legacy.aonprd.com/bestiary/universalMonsterRules.html)
require a full-round trample movement over a creature at least one size smaller.
An affected creature chooses an attack of opportunity at -4 or a Reflex save
for half damage; the save DC is 10 + half the trampler's HD + Strength modifier.
The same trampler damages a given target at most once per round. Powerful
charge adds the stat-block's specified damage to an ordinary charge attack;
the normal charge rules still apply.

## Installed-engine evidence

Guarded Steam native-donor surveys `20260928T1436375418005Z` and
`20260928T1448501815730Z` both PASS. The latter audit file is
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\20260928T1448501815730Z-observe-expanded-summoning-native-donors\native-donor-audit.json`,
SHA-256 `AAE845C0A2B30116AEA4C12CC4CDDEF0E116BA3FE23186454B87FB84FE7005C4`.
The corresponding restoration record is
`20260928T1450570230422Z-observe-expanded-summoning-native-donors.json`:
the original mod tree was restored to SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
The tested package SHA-256 was
`75ed87ab09a24f6cae6200fae35eb5321acbf8a2d0e11b8c99240151d0068351`;
DLL SHA-256 `0a0d1390bae236903077a9ca528e1be5f5ac4613d7d0f7bb48c8175ce9ff06e3`.
Source validation, all 1,943 domain tests, clean Release and strict package
validation preceded that launch.

`Assembly-CSharp.dll` defines `AbilityCustomOverrun`,
`AbilityIsFullRoundInTurnBased`, `ManeuverTrigger`, and `PowerfulCharge`.
The installed `FlyTrampleTest` and `OverrunAbility` blueprints use
`AbilityCustomOverrun` plus the turn-based full-round component. The engine
delivery has navmesh-traced target validity and a six-second combat-speed
maximum. Its `AutoSuccess`, `FirstTargetOnly`, `StopOnCorpulence`, and
`Actions` fields are explicit. The installed `MammothTrample` feature uses
`ManeuverTrigger` on a successful overrun and a Strength-based damage action.
These are candidate seams, not proof of Paizo trample behavior: the audited
abilities have `AutoSuccess=false`, and the graph alone does not establish
target-size filtering, AoO/Reflex choice, once-per-round damage, or save-safe
cleanup. The installed library also contains third-party-mod additions;
asset-name presence alone is not evidence of base-game provenance.

The game's own `PowerfulCharge` component changes charge weapon dice and
Strength scaling for a first attack. A separate `PowerfulChargeDouble`
component is present in the installed library and in the Call of the Wild
assembly; this project must not depend on it. The native component is now
measured and is unsuitable for either printed Rhinoceros charge profile;
no Rhinoceros charge is yet implemented or qualified in either combat mode.

Read-only dnlib inspection of the installed `AbilityCustomOverrun.Deliver`
state machine confirms it forces a navmesh path, iterates nearby living units,
and runs its `Actions` in each contact's target scope. `AutoSuccess=false`
triggers `RuleCombatManeuver(Overrun)` and only successful contacts reach
`Actions`; `AutoSuccess=true` skips the maneuver. The delivery temporarily
adds the native charge buff and changes `UnitState.IsCharging`, resets the
movement agent in `Cleanup`, and provides no built-in enemy/size filtering or
Reflex/AoO decision. The callable seam is real, but using it unmodified would
give a trample the wrong combat rules and could expose unrelated charge
modifiers. A request-local adapter must filter exact targets, enforce round
claims, use the correct action economy, and prove charge-state cleanup in
both modes. The installed library's dnlib metadata was read without loading
or executing game code; the private IL transcription is uncommitted evidence.

The same read-only IL check narrows the charge gap: the game's `PowerfulCharge`
component requires the native charge buff, first attack and no opportunity
attack, then adds its configured dice and **1.5 more Strength multipliers**.
`RuleCalculateWeaponStats.OnTrigger` adds the component multiplier to the
weapon's existing multiplier. The existing multiplier is donor-dependent,
so a live new-Rhinoceros baseline must be measured before choosing the
summon-local increment. No global change to the native component is justified.

The first pure rules-policy source checkpoint passed repository validation,
1,945 domain tests, clean Release, strict package, guarded Steam working-save
smoke `20260928T1517172841252Z`, and exact original-install restoration
`20260928T1520212559754Z`. This is startup safety only. The tested package
SHA-256 is
`9e38c53fbc9918a9017c88be570690d33f68816143e99873d225b491d2a183cc`;
DLL SHA-256
`96b353a407475e55f1b6fbc163629f849ebd30874b8bf8780a41ca9788a12390`.

Next: register a request-local, unpublished four-creature fixture using the
existing natural builder and donor/visual safety contracts. Measure native
charge and overrun delivery in turn-based and RTWP, then add narrowly scoped
rules components for the gaps. Publish choices only after mechanics, art,
quantity, player path, persistence, and negative controls pass.

## Native charge live measurement

The guarded Steam `disposable-expanded-summoning-rules` run
`20260928T1602214306773Z` passed all 16 assertions on mod `0.0.140`.
A request-local Mastodon with a temporary native `PowerfulCharge` feature
had Strength modifier 12, ordinary gore `2d8+24`, first native charge
`4d8+42`, later attack `2d8+24`, and opportunity attack `2d8+24`.
The native component therefore adds two dice and +18, precisely 1.5 times
that donor's Strength modifier. It is too strong for the printed Rhino
increment (+2 dice, +3 damage) and Woolly Rhino increment (+2 dice, +5
damage). The Mastodon already had a 2x Strength baseline; do not extrapolate
its base damage to either new species. The earlier diagnostic
`20260928T1548555422577Z` assumed an incorrect +18 Mastodon baseline and
is excluded; its 120-second orchestration timeout was also below the
observed four-minute fixture duration. The passing rerun used a 360-second
timeout. The temporary feature, charge marker and unit were removed.
Repository validation, all 1,945 domain tests, clean Release and strict
package validation passed. Tested package SHA-256
`6625a1159e4b29e698e87783cf948e66be6d63dc2757300a37b466c3e06cf741`;
DLL SHA-256
`95ee11ca240a540cd57f11ab8768a9f08c4d670d0d990071667421dcadd17819`.
Restoration `20260928T1606565388180Z-disposable-expanded-summoning-rules.json`
verified the original mod tree before/after SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
This qualifies only the native donor behavior, not new ungulate mechanics.
