# Sprint 84 Gunsmithing maintenance authority

## Rule and authorized adaptation

The level-one Gunslinger gains Gunsmithing. Under the authorized persistent-
owner mapping, Gunsmithing is a real, visible, automatically granted feature.
Kingmaker has no compatible firearm-crafting or one-hour rest-work UI, so this
slice preserves the existing kit-backed Repair and Overhaul actions as the
maintenance adaptation. Origin ownership and battered value remain the next
separate persistence slice.

## Observable behavior

- Firearm Proficiency grants Reload and does not grant maintenance.
- Gunsmithing alone grants Repair and Overhaul, in that stable order.
- Gunsmithing is visible, localized, rank one, and appears once at Gunslinger
  level one.
- Removing or lacking Gunsmithing removes both maintenance actions through
  native `AddFacts` ownership without affecting Reload.

## Deterministic qualification

The focused source contract verifies manifest identity, exact grants,
progression placement, bootstrap ordering, and active/ledger counts. Repository
validation, the complete domain suite, clean Release build, and strict package
validation must also pass. A save-free guarded runtime scenario must prove the
native fact graph before runtime qualification.

## Non-goals and failure behavior

This slice does not implement crafting, rest selection, battered-item identity,
origin ownership, or sale value. Bootstrap fails closed on duplicate abilities,
missing references, changed component shape, or manifest/count drift.
