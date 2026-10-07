# Transaction-owned elemental character trait persistence

Owner authority: final release gate, 2026-10-06. This supersedes only the previous
working-save overwrite/restoration blocker. Historical failed routes remain
recorded; no preexisting save is backed up, replaced or overwritten.

The closed scenario `elemental-character-traits-owned-save` accepts only
`prepare`, `verify-remove` and `verify-absent` with automatic exit. Working is
load-only. Baseline is never a game-load candidate. External inventory hashing
of every preexisting save is explicitly authorized; no save ZIP/header/payload
is inspected outside the native game loader.

The orchestration inventories every existing file and holds the established
generic deny-write catalog handles across all three fresh processes. A unique
`KMG_TRAITS_0142_<UTC>_<random>` descriptor belongs to a durable save lease
bound to its owner PID/start time, phase and expiration. The shared runtime lease
also excludes competing launch/deployment transactions. Neither lease is stolen.

P0 loads the qualified seed, grants four canonical published visible features
to the existing player-owned main character, checks exact owned providers and
Fiery enabled state, then saves through `Game.SaveGame` to a new manual
descriptor. The native `PrepareSave` callback records its exact absent path
before the saver can write. The manual number is chosen by the game; external
code never guesses it. The descriptor/path-pattern is recorded before launch,
and the exact path is admitted only from this native pre-write receipt.

P1 loads only that receipt-backed save in a fresh process, checks all eleven
stable identities and reconstructed owned state, exercises the actual module
OFF/ON notification, and restores the original in-memory setting. It removes
each visible feature via `Owner.RemoveFact(feature)`, letting the production
owned-grant/component lifecycle clean providers, toggle, buffs and area state.
It then overwrites only the same leased manual descriptor. P3 freshly loads
the removed-feature save and verifies absence without writing. P4 deletes
only the positively owned file and proves exact original inventory equality.

The native contract is SaveManager from Assembly-CSharp MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`:
`CreateNewSave(string): SaveInfo`,
`SaveRoutine(SaveInfo, bool): IEnumerator<object>`,
`PrepareSave(SaveInfo): void`, and
`SaveStashedArea(SaveInfo, AreaPersistentState): void`.
For a new descriptor, PrepareSave chooses the unused manual number, path and
ZipSaver. For an already saved descriptor it retains its exact native path.
The new overwrite admission additionally requires the exact loaded descriptor,
leased filename, native ZipSaver and preceding SHA-256. Existing new-save
callers retain their original admission rules. Unarmed or foreign write APIs
are blocked only inside this guarded request; ordinary play is unchanged.

No request-local blueprint clone is serialized. Cross-process witnesses compare
stable production identities, party character IDs, ordered inventory entries
with slot/blueprint/count/identification, money, game time and foreign fact
blueprint multiplicities. Native ItemEntity has no general UniqueId property;
the test does not invent one or claim cross-process object-reference equality.
Same-process cleanup uses exact recorded fact references. Native save turns
units/facts OFF temporarily before serialization. The owned-grant component,
like native AddFacts, retains its exact serialized grant across this temporary
OFF/ON cycle; real feature deactivation and module OFF remove it. The fixture
checks reference preservation and Fiery ON both immediately after save and in
the next fresh process.

Every stage retains structured evidence and performs save-inventory checks.
Finally removes only a native receipt-backed owned file, restores the acquired
live-mod snapshot and closes both leases. A foreign save/autosave/quicksave
delta, ownership ambiguity or failed exact restoration stops qualification.

Run, unelevated and on a clean exact artifact:

```powershell
.\scripts\Test-ElementalCharacterTraitPersistence.ps1
.\scripts\Invoke-ElementalCharacterTraitPersistenceQualification.ps1 -ExpectedVersion 0.0.142 -PackagePath <exact-build-local-zip> -Confirm:$false
```

Native qualification is pending until the final integration handoff records
fresh-process PASS evidence. Respec UI is not automated; this gate qualifies
saved visible-feature removal through the exact native deactivation lifecycle.

Native overwrite preserves the logical descriptor and final owned path. Exact
SaveRoutine creates one fresh temporary SaveInfo, and SerializeAndSaveThread
renames its ZipSaver to the original path after commit. Before the first write,
the receipt binds this initially-absent native preparation to the same lease.
Success requires that temporary path to be absent and the final path unchanged.
Failure cleanup may delete only those two receipt-proven run-owned paths; every
preexisting file remains protected. No external copy, rename or replacement is
used. Native MVID 07fa1e4d-8618-41b3-9b8d-faa17d3b26f7, SaveRoutine MoveNext
1800-byte and SerializeAndSaveThread 1190-byte contracts are required.
