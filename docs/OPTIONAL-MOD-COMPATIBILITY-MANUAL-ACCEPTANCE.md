# Optional-mod compatibility manual acceptance

Use only after an exact profile has completed automated staging, runtime
observation, and restoration. Record exact UMM IDs/versions/hashes and the
Gunslinger commit/version. A human observation supplements but does not replace
structured mechanical evidence.

- Confirm UMM lists exactly the expected isolated profile entries without a
  duplicate Gunslinger ID or red load failure.
- Create a new base Gunslinger and inspect the progression through level 20.
- Create a new Mysterious Stranger and inspect every documented replacement row
  while confirming the base class retains its original rows.
- Equip, switch, fire, reload, misfire, repair, and overhaul a disposable set of
  production firearms; confirm accepted native-rig behavior is unchanged.
- For an exact runtime-qualified Arms & Armor build, inspect ordinary equipment
  plus held firearm coexistence without reopening firearm transform tuning.
- For an exact runtime-qualified Toggle Custom Soundpacks build, confirm voice
  replacement coexists with exactly-once firearm discharge and the Gunslinger
  SoundBank.
- Test Craft Magic Items interaction only if a proven runtime-loadable local
  build was staged; record item identity/state before and after the interaction.
- Test respec only if an exact local respec mod was available and explicitly
  qualified.
- Exit, verify the transaction report says the original Mods and managed
  SoundBank states were restored exactly, and retain no staged third-party data.

Never use a valued save or `KMG_AUTOMATION_BASELINE`.

## Manual transaction recovery

If a run is interrupted, do not rename or delete any Mods-related directory by
hand. Ensure Kingmaker has exited, preserve the active transaction directory,
and run:

```powershell
.\scripts\compatibility\Restore-KingmakerCompatibilityProfile.ps1 `
  -RunId <recorded-run-id> -Confirm:$false
```

If restoration fails closed, preserve the live `Mods`, the
`Mods.kmg-compat-<runId>.original` or `.staged` directory, and
`C:\Dev\KingmakerGunslingerLab\compatibility-state\<runId>`. Do not attempt a
second speculative cleanup. The transaction record contains exact recovery
paths and the mismatch reason.
