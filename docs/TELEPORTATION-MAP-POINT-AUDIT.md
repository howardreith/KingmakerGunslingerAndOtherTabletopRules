# Teleportation map-point inventory and exclusion audit

This audit combines the exact base-game structural inventory with guarded
contextual arrival evidence at 22 permitted special/type-representative points.
It does not qualify every campaign restriction state or other map scenes.

The companion [CSV](TELEPORTATION-MAP-POINT-INVENTORY.csv) contains all 706 unique
stable point IDs discovered in the exact standalone base-game library. It retains
only the IDs, native types, internal asset identifiers, scene-anchor membership,
area/book-event presence, and native component types needed for this audit. Asset
names are diagnostic labels and must never drive eligibility or recall resolution.

Source: guarded standalone run
`20260907T2336260492141Z-78c2851fd60a41468e664a68683e50a8`, game MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`. Current main-map scene membership comes
from guarded run `20260908T0028468837443Z-700a1cbb6ead4159812161c32be81f08`.
Raw observation and proprietary IL are excluded from the repository.

| Native point type | Registered IDs | Current main-map anchors | Structural support |
|---|---:|---:|---|
| Location | 117 | 85 | Supported subject to every current-state gate |
| HiddenLocation | 102 | 99 | Supported only while actually revealed and allowed |
| Landmark | 23 | 12 | Supported subject to every current-state gate |
| Waypoint | 321 | 304 | Supported; absence of a local area is not disqualifying |
| SystemWaypoint | 143 | 111 | Supported only if persistent, selectable, and not synthetic |
| Total | 706 | 611 | Registration alone never establishes eligibility |

All 611 observed scene points have a native component-transform placement anchor
and tooltip anchor. The other 95 registered IDs are absent from this main-map
scene. They fail the current-map anchor check here; this is not a blanket ban on
points in another legitimate global-map context. None of these inventory facts
establishes that the player has visited or revealed a point.

The inventory contains 24 `LocationRestriction` components, two
`SummonPoolTrigger` components, and one each of `LocationRadiusBuff`,
`ActivateTrigger`, and `DeactivateTrigger`. Ten points expose persistent book
events. Native campaign restrictions must be evaluated at action composition and
again at confirmation; they cannot be replaced by this static inventory. Book
event presence and a name containing `Fake`, `Portal`, or `DLC` are not sufficient
reasons to include or exclude a point.

`LocationData.IsFake` is assigned from the native `UnlockLocation.FakeDescription`
action. It is campaign presentation state, not proof that an object is a transient
encounter marker. Its arrival restrictions require separate native inspection;
the adapter must not conflate the two concepts.

Automatic exclusions must cover missing/duplicate stable identity, absent or
inactive current scene anchors, unrevealed or unvisited state, native transient state, native campaign prohibition, and the party's occupied point.
Random encounter markers, temporary camp/event markers, and the moving token
have no qualifying persistent selectable point unless independently proven by
these same contracts. Unknown point types fail closed.

The versioned explicit forbidden catalog currently has no production entries:
no unconditional point-specific prohibition has been established. All 22 permitted
points in the special-point audit below passed real contextual arrival. Nineteen
other special points remain excluded by native campaign restrictions. This does
not permit an arbitrary registered point or bypass those conditions. Non-main-map
points and other campaign restriction states remain outside this arrival evidence;
any later unconditional exclusion requires an exact ID and proved reason.

Recall identity is fixed to Oleg's point
`758559f44d15fc844bf30a10a83154d5` before capital establishment and capital point
`f83de5c382e087b4ab6ce0b7397a2a13` afterward. Establishment and current anchor
validity still require native campaign-state qualification. An invalid capital
must not fall back to Oleg's.


Current-state qualification checkpoint: installed-profile run
`20260908T0421564890455Z-9fc529962db34a83958398ca5b84c1e9` read all 611 current
native points through the production adapter without adding/changing map records,
resources, time or familiarity. Native closed/hidden/unvisited/current-point
controls suppress all actions. Capital establishment follows exact native capital
region `caacbcf9f6d6561459f526e584ded703`, which constructs a prebuilt settlement
before it is claimed. The strict static-scene/unique-anchor/native-restriction
checks now have guarded composition evidence. This is not arrival or final forbidden
catalog qualification; special points still require the complete casting audit.


## Guarded special-point arrivals and deferred exploration

Run `20260908T1330037600506Z-cc0ea3d5a53a4123a156a651546ca8c3` passed **68 assertions**. The
[curated arrival CSV](TELEPORTATION-MAP-POINT-ARRIVAL-AUDIT.csv) records all 41
selected stable IDs, native component types, eligibility and mechanical result.
The fixture selected every current book-event/component point, Oleg and capital,
and one native permitted representative of each type. It provided visited/revealed
state only inside the disposable request; it never changed native restrictions.

All 22 permitted points received actual Greater Teleport actions and native
confirmations, spent one real seventh-level slot each and preserved their native
controls. This covers all five stable types, all ten current book events and six
component-bearing points. Complete protected map, time, fatigue, route, encounter,
party and familiarity snapshots remain identical through 12 deferred frames.
The 19 rejected points all report `CampaignProhibition`, with `IsRestricted=true`;
real spell resources do not promote them. Shelyn's cathedral point
`e3edf8b6987b34444972aae5bfa9d2fe`, including its two SummonPoolTriggers and
activation/deactivation components, remains in this natively excluded group.

All 611 scene anchors have at least one native graph edge: 165 have one, 31 have
two, 318 have three, 96 have four and one has five. No zero-edge anchor was observed.
This inventory fact does not establish that a route is currently revealed.

The first audit exposed native stationary exploration after arrival at
`07b9f4001ac3eed4da444c36653c584f`: the nearby hidden point
`312bf36ac8bc4c74cb0969908c876cce` changed its LastPerceptionRolled from 0 to 12.
The production saved-arrival guard now prevents that deferred reveal/perception
work until native ordinary travel resumes. The corrected audit proves both
suppression after every cast and resumed exploration during actual native Travel,
including recovery from malformed spell state. The native positive-control edge
is fixture setup after all cast assertions, never a teleport effect.

Full raw evidence remains under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260908T1330037500523Z-disposable-teleportation-destinations`.
No additional unconditional deny-catalog entry is justified by these results.
