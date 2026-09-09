# Teleportation destination UI coexistence

The guarded `disposable-teleportation-coexistence` and
`disposable-teleportation-coexistence-gamepad` scenarios add one request-owned
synthetic foreign action before KMG augmentation. They use the real native
point presenter, prepared spellbook resources, confirmation, navigation and
input handlers. No OS input, screen coordinates or visual inference is used.
The foreign owner is removed in `finally` and is unreachable in normal play.

The fixture verifies exact control, parent, sibling, label, enabled state and
callback references/content; one foreign callback invocation; native Travel
once without expenditure; repeated reopen without duplicates; live exhaustion;
Escape, Hide, ForceClose, replacement modal and Dispose ownership; zero UI
exceptions; and exact fixture cleanup without save writes. Gamepad assertions
also retain the pre-existing navigation entries/default and input layers.
With the module OFF, a real prepared source still constructs zero KMG controls,
attaches no production hooks and leaves familiarity untouched. The initial
no-source interaction constructs zero KMG controls in both module states.

Native `GlobalMapMessageBoxView.UpdateNavigation()` rebuilds native entries with
`ConsoleMultiNavigationCollection.SetEntities`. The independent synthetic owner
re-registers its same control after that native rebuild, before KMG's normal
postfix. A later probe verifies KMG preserves every entry. This does not claim
that vanilla retains arbitrary entries across its own collection rebuild.
Desktop `FillDialogInfoLocation(bool)` starts while the panel is inactive;
the fixture finds its donor by the exact native label ancestry without activating
or changing native controls. Harmony12's installed field is named `prioritiy`.
These are fixture details, not changes to production UI behavior.

Run from Windows PowerShell after deploying the pinned qualified artifact:

```powershell
.\scripts\Invoke-TeleportationHardeningQualification.ps1 `
  -Scope Coexistence -ExpectedVersion 0.0.119 `
  -DeploymentManifestPath <exact-deployment.json> -PackagePath <exact-package.zip> `
  -Confirm:$false
```

The launcher opens read-only leases on every pre-existing save, records the
complete Mods tree, changes only the Teleportation setting for the two OFF
processes, and restores the original settings/backup bytes. Shared tested
restoration functions remove only new, proven transaction-owned KMG sidecars.
Raw evidence belongs under the ignored runtime-evidence root; commit only
curated results. `-Scope Native` runs the ten existing teleportation scenarios
and working-save-smoke with the same preservation transaction.

Development checkpoint: all four fresh processes PASS, 80 assertions, on the
installed UMM 0.33.0.0 stack. No production UI defect was reproduced and no
production UI adapter was changed. Exact IDs and rejected probes are recorded
in `TELEPORTATION-HARDENING-REPORT.md`. Final release qualification must rerun
these scenarios against the exact clean committed release artifact.

`-Scope Boundary` covers all 26 current module configurations with exact runtime
parameters and reversible settings. Its final original-configuration startup
lets `ZFavoredClass.Main.LibraryScriptableObject_LoadDictionary_Patch.Postfix`
call `CallOfTheWild.Helpers.GuidStorage.dump` normally to regenerate its
`loaded_blueprints.txt` diagnostic. A matrix changes that diagnostic's contents
because it lists currently published blueprints. The final native startup was
proven to regenerate the original bytes; no third-party file is manually edited.
The original cleanup failure and native recovery remain separate evidence.
