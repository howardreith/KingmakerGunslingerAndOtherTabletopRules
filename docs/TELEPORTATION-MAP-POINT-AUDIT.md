# Teleportation map-point inventory and exclusion audit

This is a structural audit for the contextual teleportation feature in progress.
Safe contextual casting at these points has not yet been qualified.

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
no unconditional point-specific prohibition has yet been established by this
forensic checkpoint. This is an unresolved qualification item, not a claim that
every registered point is permitted. Before enabling live casting, review the
native restrictions and arrival behavior of persistent book events, special
portal points, capital/Oleg state changes, and non-main-map points. Record any
additional unconditional exclusion by its exact ID and proved reason.

Recall identity is fixed to Oleg's point
`758559f44d15fc844bf30a10a83154d5` before capital establishment and capital point
`f83de5c382e087b4ab6ce0b7397a2a13` afterward. Establishment and current anchor
validity still require native campaign-state qualification. An invalid capital
must not fall back to Oleg's.
