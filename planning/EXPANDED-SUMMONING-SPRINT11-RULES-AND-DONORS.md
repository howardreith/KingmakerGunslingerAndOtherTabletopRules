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
assembly; this project must not depend on it. Neither native charge component
has yet been shown to yield exactly the rhinoceros stat-block damage in both
combat modes.

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
