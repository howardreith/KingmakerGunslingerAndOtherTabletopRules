# Sprint 79 entry criteria: production identical-firearm switching

Use one detached native unit and two distinct `ItemEntityWeapon` instances of
the production Early Pistol blueprint. Give the first loaded/Normal state and
the second empty/Broken state through the production item-owned repository.

Equip and resolve the first pistol, switch the native primary-hand slot to the
second pistol and resolve it, then require both item states to remain exact and
independent. Finally equip both distinct pistols across the native primary and
secondary hand slots and require the exact resolver to fail closed on ambiguity.

No inventory, collection, save, vendor, or campaign mutation is permitted.
Require focused source checks, inherited validation, the complete domain suite,
runner/preflight checks, clean Release build, strict package validation, exact
mod load, and two fresh save-free PASS runs.
