# 0.0.113 Save-Load Reconciliation Hotfix

## Scope

This hotfix repairs the 0.0.112 paper-cartridge reconciliation regression that
can mutate a unit's persistent fact graph during Kingmaker save hydration. It
also restores nonempty local copy for generic Nodachi and every named eastern
weapon so a donor `Brace` phrase cannot trip blueprint validation before save
loading begins.

## Safety contract

- `PaperCartridgeModeRuntime.IsActive` reads exactly one live native
  activatable ability and returns its `IsOn` state without reconciling facts.
- KMG installs no `UnitEntityData.PostLoad` paper-mode patch.
- The `set_IsOn` postfix is limited to nonpersistent presentation revision
  invalidation.
- The hidden marker is never evidence for ammunition selection, action economy,
  inventory consumption, or misfire behavior.
- No blueprint GUID, item identity, toggle identity, marker identity, or save
  schema changes.

## Runtime evidence

Candidate runtime evidence is added after guarded Steam qualification. The
procedure uses only `KMG_AUTOMATION_WORKING`; it never selects, loads, changes,
or deletes a user's real save.