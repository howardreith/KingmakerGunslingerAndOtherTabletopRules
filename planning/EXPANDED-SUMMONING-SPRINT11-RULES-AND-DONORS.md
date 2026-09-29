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

## Summon-scoped powerful charge seam

`UngulatePowerfulCharge` now applies configured extra gore dice and damage
only when the exact owning unit and gore weapon make the first native marked
charge attack, never on follow-up or opportunity attacks. The Rhino profiles
derive increments from their printed ordinary and powerful damage: two dice
and +3 for Rhinoceros, two dice and +5 for Woolly Rhinoceros. This component
is implemented but is not attached to a published or hidden new Rhino yet.

The first guarded disposable check `20260928T1628286600311Z` failed its
owned-charge assertion: an existing Dire Boar had ordinary `2d6+9`, but its
first marked charge with the new component became `6d6+12`. That donor has an
additional two charge dice through a still-unisolated existing game effect;
it is unsafe as the component control and is excluded from qualification.
All other 16 assertions passed. Its wrapper restored the original live mod
tree exactly at `20260928T1632448318962Z`. The narrowed control uses the
previously measured Mastodon and does not claim Rhino's baseline.

Guarded Steam `20260928T1638254604120Z-disposable-expanded-summoning-rules`
passed 17/17 assertions. The temporary project component changed Mastodon
gore from `2d8+24` to `4d8+27` on the first charge; later, opportunity and
post-marker attacks were all `2d8+24`. This proves the configured +2 dice
and +3 damage without native component's +18 Strength scaling. Repository
validation, all 1,945 domain tests, clean Release and strict package passed;
tested package SHA-256
`8e1d307aef89ea5910389392c53ff337e8029ce27411edb92bafb4acab683ad9`,
DLL SHA-256
`a0cd789b4ffaf790d5d5ce1e43eabc4c8889f784fe7cf760b5e5df9934fbb55a`.
Restoration `20260928T1642382480272Z-disposable-expanded-summoning-rules.json`
verified the original 136-file tree before and after SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Next: register unpublished Rhino and Woolly Rhino, confirm their own ordinary
and charged gore, and prove real attacks in both combat modes before release.

## Four-unit hidden registration and live inventory

Aurochs, Bison, Rhinoceros and Woolly Rhinoceros now have append-only unit,
ability and template identities: 100 new manifest entries. All 48 new logical
placements are registered but suppressed. The total is 87 project units,
882 registered logical placements and the unchanged 834 published placements.
Each unit has its own printed racial hit dice, size, abilities, natural armor,
speed and gore weapon; Horse/Mastodon views are temporary hidden donors.
Charge, trample, stampede and original 3D visuals remain uninstalled, so none
of these choices is ready for publication.

Repository validation, all 1,945 domain tests, a clean exact-reference
Release build and strict package checks passed. The candidate package SHA-256
is `c71315aa2af8e7dd3189d0bc87853667180fe54890d74bfc283409492709d628`;
DLL SHA-256 is
`a6534ffa0227eeb95010ac3015d879a8243b7c2ab2d9e927978dccbd63c20552`.
The first guarded inventory request `20260928T1715167922950Z` is excluded:
the game produced an all-PASS result after 235 seconds, but orchestration
timed out at 180 seconds and recorded `PASS-WITH-TEARDOWN-FAULT`. Its wrapper
restored the original installation exactly at `20260928T1719410127955Z`.
The safe retry increased the request window to 420 seconds. Guarded Steam
inventory `20260928T1724419577973Z` then passed cleanly: 87/87 units,
1,395/1,395 ability identities, 882/882 registered executable contracts,
834/834 published parent placements, 18 exact menu equations, zero
prohibited references, zero donor-component sharing, zero inherited spells
or starting inventory. The wrapper reported `launcherOutcome=Clean` and
`outcome=PASS`. Restoration
`20260928T1728545901562Z-observe-expanded-summoning-inventory.json`
verified the pre/post original 136-file tree SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`;
no Kingmaker process remained. This proves hidden registration and menu
isolation, not the new species' combat damage or visual fidelity.

## Registered Rhino powerful charge damage

Two append-only hidden feature identities attach `UngulatePowerfulCharge` to
the exact Rhinoceros and Woolly Rhinoceros units and primary gore weapons.
The initial guarded live check `20260928T1754133569009Z` failed one of 18
assertions: ordinary Rhino gore was `2d6+9`, but the first marked charge was
`6d6+12` rather than printed `4d6+12`. Woolly Rhino already passed
`2d8+13` to `4d8+18`; later/opportunity/post-marker cases and exact cleanup
passed. The original 136-file installation was restored at
`20260928T1758359171925Z`. A reversible Mastodon-gore clone diagnostic
`20260928T1820184232276Z` also failed: Rhino reached `8d6+12`; Bison was
`2d6+14` both ordinarily and when charging, while Woolly remained correct.
Its wrapper restored the original tree at `20260928T1824380823076Z`.
The clone was not retained, and its uncommitted identity was removed.

Installed `Assembly-CSharp` IL shows why: after a fact sets
`RuleCalculateWeaponStats.WeaponDamageDiceOverride`, the native calculation
scales that override a second time unless `DoNotScaleDamage` is set. The
summon-owned component now sets that flag only for its exact first gore
charge, using the printed final dice; ordinary, later and opportunity
attacks remain on native handling. This diagnosis replaced the earlier
working hypothesis that a native 2d6 weapon itself added charge dice.

Guarded Steam `20260928T1840088844180Z-disposable-expanded-summoning-rules`
passed 18/18 assertions on the final candidate. Rhinoceros ordinary/first/
later/opportunity/post-marker were `2d6+9`, `4d6+12`, `2d6+9`, `2d6+9`,
`2d6+9`. Woolly Rhinoceros measured `2d8+13`, `4d8+18`, `2d8+13`,
`2d8+13`, `2d8+13`. Bison with its granted native Power Attack feat measured
`2d6+14`; removing that feat only in the disposable fixture restored its
printed `2d6+12`, and a native charge marker added no dice. Repository
validation, all 1,947 domain tests, clean exact-reference Release, strict
package, request-local cleanup and version 0.0.140 passed. Package SHA-256:
`224796490e3200be7adb9039f7c1eb0f383c7cdc6a9a92ec24cbfd924414aa7b`;
DLL SHA-256:
`5157eb9b2d5beaa43af8461c97a84d9faf31a9bcc07298855bd4a59cdb08d526`.
Restoration `20260928T1844262522767Z-disposable-expanded-summoning-rules.json`
records `launcherOutcome=Clean`, `outcome=PASS`, no remaining Kingmaker
process, and the identical pre/post original 136-file mod tree SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
The rule-stat calculations and owner-unit scope are qualified. Real
turn-based/RTWP attack cadence, charge path/contact, trample, original views,
quantity summons, and publication remain pending.

## Hidden trample path and command-fixture boundary

The installed `AbilityCustomOverrun` provides a forced path and calls its
`Actions` once for each contacted unit. Its ordinary mode performs an Overrun
CMB check, so the hidden Aurochs, Bison and Woolly Rhinoceros abilities use
`AutoSuccess=true` solely to bypass that maneuver. A summon-owned contact
action filters exact caster blueprint, enemies, living targets and printed
size, claims a target once per combat round, rolls the printed Reflex DC and
applies the printed bludgeoning damage with the native half-damage flag on a
successful save. A separate path checker restores the twice-current-speed
range that the native Overrun checker otherwise bypasses in AutoSuccess mode.
The three abilities are full-round and carry append-only identities. Their
unit grants remain inside the 48 hidden Sprint 11 placements. The action uses
the installed Overrun icon provisionally; no new public menu consumer exists.

This was a hidden implementation checkpoint, not a trample qualification.
At this point the target response had not been proven against native movement,
and the code conservatively awarded no Stampede same-size/+2 benefit. The
later automatic-response section records the owner-authorized target-response
adaptation and its live qualification. Coordinated Stampede and the four
original 3D ungulate views remain publication gates.

A request-local Rhino `UnitAttack` charge probe in guarded run
`20260928T1906471936890Z-disposable-expanded-summoning-rules` recorded
`CanStart=True` but `started=False`, `finished=False`, no attack rolls and no
damage events after 600 frames. The pre-existing cat command in that same
fixture had the same queued-only outcome: its frame loop does not drive the
native action controller. Its runtime result was PASS only for recording and
restoring the diagnostic, not for charge cadence. The source probe was
reversed before this checkpoint. A real command must use a native turn/RTWP
controller fixture rather than treating that queued command as evidence.

The first guarded inventory with the three new abilities,
`20260928T1937339123190Z-observe-expanded-summoning-inventory`, failed its
prohibited-reference assertion on five expected direct grants: the two
earlier Rhino charge facts and the three trample abilities. Its wrapper
restored the original installation exactly. The inventory now permits only
those five exact unit/fact GUID pairs. Repository validation, all 1,948
domain tests, the clean exact-reference Release build and strict package
validation passed; package SHA-256 is
`890fbe2d10412b6759056d6385528e6bfc29bf427899c1676cbd57d3ca28c937`,
DLL SHA-256 is
`9cc10de013c3a51f04906418c78a1623dd87f7a205df0c79686b9784e661cc27`.
Guarded Steam inventory `20260928T1951390082664Z-observe-expanded-summoning-inventory`
then passed 50/50 assertions: 87 units, 1,398 registered ability identities,
882 executable summon contracts, 834 published parent placements, 18 exact
menu equations, zero prohibited references and no missing icons in the
published menu. Restoration
`20260928T1955506906568Z-observe-expanded-summoning-inventory.json`
records `launcherOutcome=Clean`, `outcome=PASS`, no remaining Kingmaker
process, and the original 136-file tree SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`
both before and after the run.

### Native Aurochs trample-path contact

Guarded loaded-area run `20260929T0033452745962Z` passed 21/21 assertions.
The hidden Aurochs used its exact granted ability through the native
`UnitCommands` queue and the installed `AbilityCustomOverrun`. On a valid
request-local RTWP route, the native movement agent traveled 8.92 m through
the smaller hostile, produced one DC 17 Reflex save and one 16-point
bludgeoning event, and left the allied caster unharmed. The fixture restored
the prior turn-mode setting, pause state and awake-unit snapshot; the wrapper
restored the original installation at `20260929T0037501346548Z`.

An ablation removed the proposed radius-reflection delivery subclass and
passed the same path/contact assertion. Native overrun is retained without
that correction. This measures one Aurochs RTWP contact; it does not establish
Bison/Woolly contacts, victim AoO choice, turn-based full-round cadence,
multiple-quantity navigation, or original 3D silhouettes. Those remain
publication gates.

The expanded three-species guarded run `20260929T0050555483840Z` passed
27/27 assertions: Aurochs 4.96 m/DC 17/16 damage, Bison 4.96 m/DC 20/19
damage, and Woolly Rhinoceros 4.99 m/DC 23/19 damage. Each crossed a smaller
hostile with one save and one damage event and no allied damage. Mode, pause,
game time and awake-unit state were restored after each case. It does not
close the remaining cadence, target-choice, quantity or visual gates.

Guarded run `20260929T0116041484925Z` also passed a separate successful
Reflex case: the native Aurochs path made one DC 17 successful save and one
half-on-save damage event, with 3 actual damage under the working save's
difficulty adjustment. The disposable hostile's prior Reflex value was
restored. This closes the basic half-save branch, not the victim's tabletop
AoO-versus-save choice or same-round replay contract.

Guarded run `20260929T0146260041731Z` then passed 31/31 assertions with two
native Aurochs overrun paths through the same hostile in the same game-time
round. The second path moved 4.63 m, while save count and damage-event count
stayed at one and the hostile's actual damage stayed at four. The first
contact's successful DC 17 save carried the half-on-save flag. This qualifies
the per-target, per-round replay guard for the tested RTWP path; victim AoO
choice and turn-based cadence remain separate gates.

### Prepared creature-choice icons

The four original source paintings were exported at 128 px through the
existing icon pipeline, and the delegated manifest now names each exact unit
and its 29/25/25/21 generated ability/template consumers. They are packaged
as prepared hidden concepts, with no ungulate placement published. Repository
validation, 1,948 domain tests, clean Release and strict 266-file package
passed. Guarded Steam inventory `20260929T0218259420008Z` passed 50/50
assertions with unchanged 834 published placements, no missing published
menu icons, and exact original-installation restoration at
`20260929T0222241478727Z`. Actual ungulate UI use and owner visual review
remain open.

The guarded `20260929T0236149627664Z` rules run also captured exact
renderer-local bind frames for original 3D authoring. Horse/Aurochs has one
2,110-vertex skinned renderer with 31 bones and 31 bind poses; Mastodon/
Woolly Rhinoceros has one 4,307-vertex renderer with 40 bones and 40 poses.
The captures stay in guarded local evidence, never source control or the mod
package. The run passed 31/31 assertions and restored the original live tree
at `20260929T0240261038414Z`. Distinct ungulate meshes and visual review
remain open.

Four original schema-2 ungulate meshes and 1024 px albedos are now exported
and packaged. The cattle bind to named Horse bones; both rhinoceroses bind
to named Mastodon bones. Captured donor transforms stay only in local evidence.
The existing per-view renderer swap leaves the native donor intact on asset
rejection. Repository validation, 1,950 tests, clean Release and strict
274-file package passed. Guarded Steam `20260929T0348308658037Z` passed
31/31 rules assertions and captured Aurochs/Woolly original mesh names on
live renderers; exact original-installation restoration passed. This does
not establish the other two live views, deformed geometry, impact points,
navigation or owner visual acceptance. All choices remain hidden.

Guarded creature review `20260929T0432458054535Z` passed 24/24 after a
failed resource/movement diagnostic exposed and prompted correction of the
per-view release path and ground-review ticking. Aurochs, Bison, Rhinoceros
and Woolly Rhinoceros each retained their exact original mesh and albedo,
crossed the surveyed connected doorway on native movement for roughly
12.3 m, and left zero private mesh/material instances after dismissal. The
two Rhinoceroses use view-only scales 0.55/0.60 to fit their Large identity
on the Huge Mastodon rig; collision and mechanical size remain native Large.
The sixteen party-camera frames were inspected; the route's walls/bookcases
cause native cyan occlusion highlights. Clear art review, exact attack contact,
charge-command travel, quantity pathing, victim AoO choice, turn-based
cadence, negative-control in-game checks and publication are still open.

The next guarded rules run `20260929T0455499342752Z` passed 31/31 after
1,951 tests, clean Release and strict 274-file package. In addition to the
first-charge stat calculations (`4d6+12` Rhino, `4d8+18` Woolly), the fixture
triggered a real gore attack with each hidden Rhino and the native charge
marker active. Both hit and dealt damage (47 and 77 in this run); it then
restored the disposable target. Exact original-installation restoration
`20260929T0500068625552Z` passed. This closes the direct attack-rule effect,
not the native queued charge command, its travel or visual contact.

The queued charge fixture then isolated the game's charge command seam.
`UnitAttack.IsCharge` by itself traveled and attacked without the native
charge marker; read-only installed-assembly inspection showed
`AbilityCustomCharge` supplies that marker and queues the attack after its
movement coroutine. The exact native `ChargeAbility`
(`c78506dd0e14f7c45a599990e4e65038`) is already granted to both hidden
Rhinos. The final guarded run `20260929T0639171416597Z` passed 33/33:
Rhinoceros moved 3.88 m, queued and hit with its first gore under the marker,
dealing 25 damage; Woolly Rhinoceros moved 3.42 m and did the same for 30.
Repository validation, 1,951 tests, clean Release and strict 274-file
package passed; wrapper restoration `20260929T0643316615128Z` verified the
original installation. The six earlier diagnostic runs are excluded from
qualification and restored exactly. This closes the one-at-a-time RTWP native
charge path, not turn-based charge, multiple-quantity navigation or visual
impact alignment.

## Clear-floor art inspection, 2026-09-29

Guarded Steam run `20260929T1022039564996Z` passed 32/32 after repository
validation, 1,952 domain tests, clean Release and strict package validation.
For each hidden ungulate, native doorway motion completed first; a request-local
visual survey then found a connected-floor point with eight clear radial
walk/sight rays and moved only the disposable subject. One overhead plus four
oblique live views per species were written, with every camera pose restored.
The Aurochs/Bison horn span, Bison mane, two Rhinoceros nasal-horn/coated-body
profiles and body bulk are distinguishable. The obliques also show thin,
splayed hoof/foot ends and awkward apparent leg poses, especially on both
Rhinoceroses. The art gate is not accepted on these frames; an idle side view
and attack impact contact remain to be checked. Screenshots support visual
inspection only. Zero save writes, zero owned-view resources and exact
original-installation restoration passed; restoration record
`20260929T1029221524485Z` verified the original 136-file tree SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Package SHA-256 `937CBCB24C2AFC766F43E367C971A86030239B703DE88120499AE3513DC96932`;
DLL SHA-256 `285298FEDE8CEEB05F1DB5B88C025A2A52D659ADF77708A1F484E3CE3663FA1D`.

## Owner-authorized automatic Trample response

Kingmaker has no native contact-time prompt for the tabletop target choice.
The owner therefore authorized a disclosed automatic adaptation. On a
trampler's first valid contact with a target in the round, the component uses
the native threat hand, reach test, action state and ordinary attack-of-
opportunity resource to decide the branch before any attack or save roll. If
the target can actually make a legal melee attack of opportunity, it makes
exactly one at the printed -4 penalty, spends one normal resource, receives
no Reflex save, and resolves the attack before contact damage. If that attack
kills, incapacitates, removes or otherwise stops the trampler, the contact
deals no Trample damage and the round ledger rejects all later contacts. If
there is no legal attack, or the prequalified attack cannot execute, the
target instead rolls the printed Reflex save for half damage. The same policy
applies to every faction and does not inspect a roll before selecting a branch.
The hidden ability tooltip discloses this automatic Kingmaker adaptation.

A focused native audit found that `UnitCombatState.Disengage` both removes the
engagement and invokes the ordinary movement attack-of-opportunity path. A
narrow Harmony prefix temporarily sets that combat state's existing
`PreventAttacksOfOpporunityNextFrame` flag only when the moving unit has an
exact active Aurochs, Bison or Woolly Rhinoceros Trample command and its
round ledger has already claimed that defender. The postfix immediately
restores the flag. Native disengagement and events still run; the scope only
prevents an ordinary movement attack from duplicating the already resolved
contact response. Lethal synchronous attacks are detected from current hit
points as well as native state flags because Kingmaker defers some death-state
updates until after rule resolution.

Guarded Steam run
`20260929T2200169193790Z-disposable-expanded-summoning-rules` passed the final
matrix on version `0.0.140` after repository validation, all 1,952 domain
tests, a clean Release build and strict package validation. Live cases proved:

- RTWP zero-resource, unable-to-act and nonthreatening defenders used the
  Reflex branch without spending a resource; forced failed and successful
  saves produced full and half damage respectively.
- A legal RTWP melee response hit at exactly -4, spent the sole resource,
  produced no save and allowed full contact damage because the trampler
  continued.
- A player-faction defender against a hostile Bison used the same policy.
- A turn-based quantity Aurochs received one -4 miss, spent one resource,
  produced no save and dealt full damage.
- A turn-based Combat Reflexes defender hit once and moved from four resources
  to three; it did not receive a second response or a save.
- A lethal turn-based response reduced the trampler from one hit point to
  negative two before damage; that contact and a later distinct contact both
  produced no save and no damage.
- A quantity-summoned Aurochs traversed a real RTWP native Overrun path through
  a Combat Reflexes defender. Exactly one -4 response occurred, the resource
  moved from four to three, no save occurred, full damage followed, and no
  ordinary-plus-special duplicate appeared.
- Direct and quantity summons, hostile and player-controlled targets, RTWP and
  turn-based mode, hit and miss attacks, full and half saves, exact DCs,
  once-per-target replay suppression, command interruption and disposable
  fixture restoration all passed.

The tested package SHA-256 was
`c9c3ddb9a738a99339dc154e6100dcce695909e724b576598995e6a95fd71506`;
the built, deployed and loaded DLL SHA-256 was
`df5f4ed0a3ef219d565398029925842b0876dc7dfe85b33dfe5a3da0ce5eb7e2`.
Restoration record
`20260929T2205130992683Z-disposable-expanded-summoning-rules.json` records a
clean launcher outcome, scenario PASS, no failures, and the exact original
136-file live tree before and after restoration at SHA-256
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
This closes the ordinary Trample target-response contract. Coordinated
Stampede and corrected hoof/leg art remain open, so all Sprint 11 placements
remain hidden.
