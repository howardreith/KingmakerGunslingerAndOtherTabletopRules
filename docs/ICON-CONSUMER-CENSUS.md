# Guarded icon consumer census

The `icon-overhaul-visual-evidence` scenario includes a read-only census of the
current icon mission. It requires the ordinary validated request through Steam
App ID 640820. No campaign or save is loaded. All normal request, identity,
deployment, timeout, evidence-root and clean-exit guards apply.

The runner arms the census only after validating and exclusively claiming the
request. Initialization consumes that accepted request identity; it does not
revalidate the claimed run ID or bypass the parser's replay protection.

After existing blueprint registration/publication, the exact validated scenario
captures the live objects selected by the installed blueprint manifest's four
elemental/strategic prefixes and 37 explicitly listed firearm identities. The
source catalog validates those census rules. It retains all 284 catalog records:
255 blueprint-library consumers, 28 appearance resources in the existing native
resource cache, plus the reserved diagnostic race whose absence must be verified. The
separate race-probe scenario owns that reservation; this census never creates it.

The snapshot also preserves every currently registered icon-bearing unit fact,
item and character class outside the 137 painted targets. It reads direct
blueprint references, variant arrays, granted facts, held-touch delivery links,
scroll abilities/CopyScroll targets, and nested action lists. An iterative reader
records shared/cyclic action edges without recursive depth limits. It fails with
an exact path if a root exceeds 4,096 distinct action containers; terminal
blueprint/resource links are never enqueued. Focused executable tests cover deep
actions, cycles, sharing, null slots, changed targets and the foreign boundary. A blueprint
reference ends traversal; the observer never descends into a foreign blueprint
or changes an icon, texture, component, UI entry, unit or save.

The same request compares assignments immediately after
`OwnedIconAssignments.Apply` and again after initialization/compatibility when
the runtime scenario executes. Required checks include:

- All 137 exact consumers use the intended cached 128px art, with 89 distinct keys.
- All other observed icon assignments retain their original Sprite references
  immediately across the mapping operation.
- The 255 owned graphs/components are unchanged across that operation; every
  original component reference retains its order through late initialization.
- All 28 appearance resources retain their exact cached object references.
- The reserved diagnostic race remains absent at both observation points.
- All 21 native-equivalent feature/ability/delivery consumers retain the exact
  native donor's Sprite reference.
- Installed export hashes and executing DLL/MVID are recorded for independent
  comparison with the authoring catalog and immutable Build-Local artifact.

Late compatibility initialization may fill previously empty foreign icons or
append spell-list components. A live scenario result alone cannot accept these
transitions. Run a second fresh launch of the **same immutable artifact/profile**
with `-Parameters @{iconCensusControl=$true}`. This exact, automatically exiting
no-save request suppresses only `OwnedIconAssignments.Apply`. Ordinary runs never
use this branch. Both runs retain the complete before/after evidence.

The mandatory offline comparison requires matching protected Sprite content and
reference-preservation results, every owned graph and component signature, and
the same executing DLL/MVID/profile. Only process-local instance IDs are removed
for cross-process comparison; they remain checked within each run. No foreign
identity or component type is exempted. A mismatch fails qualification.

Use the inspected runtime orchestrator, exact current version, explicit package
and deployment manifest. Reuse that immutable deployment for focused profile
checks. Module OFF still registers the saved identities; it does not authorize
publishing disabled selectors. The existing independent Teleportation
registration failure boundary is unchanged in normal gameplay; incomplete
identities cannot pass this diagnostic census.

Observation failures are retained for the scenario's structured ERROR result.
They do not cause production bootstrap rollback; required painted assignments
still use their ordinary strict identity/type checks.

The scenario writes `icon-consumer-census.json` beneath its unique guarded
evidence directory. Validate it afterward with:

```powershell
python -B tools/validate_icon_runtime_evidence.py `
  --census <exact-run-directory>/icon-consumer-census.json `
  --control <matching-control-directory>/icon-consumer-census.json `
  --build-manifest <exact-package>.build-local.json
```

Keep that raw machine-local census outside Git. Record curated counts, artifact
hashes, run IDs and failures in the mission report. Native screen placement,
clipping and aesthetic approval are separate gates; neither this JSON nor the
scenario's eleven live-sprite facsimiles supplies native game-screen evidence.

Painted integration passed the candidate/control comparison and canonical
working-save smoke. Exact runs, artifact hashes and restoration are in
[the curated qualification](../reports/icon-overhaul/PAINTED-INTEGRATION-QUALIFICATION.json).
Native icon screen review and firearm completion remain separate work.
