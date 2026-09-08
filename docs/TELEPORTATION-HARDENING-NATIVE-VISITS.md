# Native visit evidence audit

Installed Kingmaker 2.1.7b Assembly-CSharp.dll SHA-256:
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`;
MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`. The full managed IL field-store
and call-site inventory was examined locally. Proprietary decompilations and
IL remain ignored; this document records factual conclusions only.

## Writers

The four LocationData fields are serialized state, not arrival counters.
The complete direct-store inventory in the installed assembly is:

| Field | Writer and IL offset | Meaning |
| --- | --- | --- |
| EdgesOpened | GlobalMapLocation.OpenOutgoingEdges(GlobalMapLocation), IL_0007 | Sets true at entry without checking party position or ordinary travel. |
| EdgesOpened | CheatsCommon.RevealAllLocations, IL_0033; RevealReachableLocations, IL_0172 | Native reveal commands set multiple points without a journey. Forensic evidence only; not feature cast sources. |
| IsExplored | MarkLocationExplored.RunAction(), IL_0020 | Writes the configured Explored value for the configured blueprint without any party-position or arrival check. |
| IsExplored | GlobalMapRules.EnterLocation(), IL_00ed | Sets true for ExploreOnEnter after local-area/book-event entry handling. |
| IsSeen | GlobalMapRules.RevealLocation(GlobalMapLocation, Boolean), IL_01b1 | First reveal/visual notification; no physical arrival requirement. |
| LastVisited | GlobalMapRules.EnterLocation(), IL_00d9 | Records time of area/book/multiple-entry handling; does not cover ordinary crossroads visits. |
| LastVisited | GlobalMapRules.OnAreaDidLoad(), IL_0372 | Seeds current time for the current start location if zero, during reconstruction. |
| LastVisited | Both LocationData constructors, IL_0006 | Initializes to zero. |

Type namespaces: LocationData is Kingmaker.Globalmap.State; GlobalMapLocation
and GlobalMapRules are Kingmaker.Globalmap; MarkLocationExplored and
UnlockLocation are Kingmaker.Designers.EventConditionActionSystem.Actions;
CheatsCommon is Kingmaker.Cheats. No native ordinary-arrival counter exists.

LocationData.Reveal/Hide change the private revealed property; constructors
and JSON loading also establish/restore serialized flags. UnlockLocation.RunAction
calls GlobalMapRules.RevealLocation or LocationData.Reveal (and hide counterparts),
depending on whether the map exists, without testing party position.
PlayerUpgradeAction.Apply and LocationRevealController.Tick also call native
reveal paths. Quest unlock, proximity, exploration completion, and load-time
reconstruction are therefore distinct from ordinary arrival.

The native callers of OpenOutgoingEdges are GlobalMapRules.TeleportParty,
MapMovementController.Activate, and the intermediate/final paths of
MapMovementController.MoveAlongEdge. Activation opens the current point on
reconstruction; circle teleportation opens its arrival point. Neither is
ordinary travel. OpenOutgoingEdges itself has no arrival prerequisite. The
native reveal-command field writers additionally prove EdgesOpened can become
true without the party reaching that point at all. The campaign action writer
proves the same for IsExplored independently of cheats.

Kingmaker.Controllers.GlobalMap.MapMovementController.MoveAlongEdge reads its
current WalkedDistance into a local each invocation, then advances the native
path. The existing KMG observer captures this invocation-local baseline and
credits only before < boundary <= after. Completion is idempotent. A prior
exact boundary is excluded by the next invocation. The previous duplicate-count
concern is not reproduced by inspection/domain tests. The movement transpiler
and counting implementation are unchanged; native exact-once assertions remain.

## Production rule

Both Teleport and Greater Teleport, including failure alternatives, require a
positive persisted count. Live reveal/exploration/unlock flags cannot supply
that count after migration. Ordinary native movement remains the only ongoing
arrival writer. New points visited with the module OFF can require a normal
revisit after enabling.

One-time legacy migration retains v0.0.118 inference: EdgesOpened OR IsExplored
seeds each missing historical entry to exactly one. Native history cannot
distinguish every physical arrival from scripted unlock. This bounded legacy
inference does not continue as a live bypass. Existing positive counts,
migration flags, format-1 payloads, and exploration boundaries are unchanged.
Malformed payloads fail closed without replacement.

Word of Recall shares general destination safety, but separately proves the
exact current sanctuary: Oleg before capital establishment, exact capital
afterward. Ambiguous kingdom state never guesses a fallback. This exception
does not grant either Teleport spell a visit bypass.

## Guarded campaign load safety

SaveManager.LoadRoutine(SaveInfo, Boolean) increments LoadedTimes, calls
ISaver.SaveJson for the header, then ISaver.Save during loading. The prior
working-save sentinel watched gameplay saves and did not prevent that rewrite.
The repair mission forbids even this change to any pre-existing save.

ZipSaver.SaveJson updates an in-memory entry; Save commits the archive;
Dispose releases it without committing. The guarded loader uses a request-owned
read-through ISaver for the exact captured descriptor. Only the proven single
header-counter update is suppressed; other writes are rejected. Native reads,
stash extraction, deserialization, owner reconstruction, area loading, and
completion remain native. Independent OS read leases and complete catalog
hashes protect pre-existing saves. This infrastructure is unreachable during
normal player operation.
