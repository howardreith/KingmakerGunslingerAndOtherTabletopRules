# Kingmaker Gunslinger 0.0.113

Release archive:
`KingmakerGunslinger-0.0.113-save-load-hotfix.zip`.

The release retains the 1,288-test Overhaul, Summon, and Fatigue baseline; the
1,325-test icon baseline; the 1,348-test repair/notification baseline; the
1,359-test protection baseline; and the 1,370-test native outfit baseline.

## Save-load hotfix

- Makes every paper-mode read read-only. The native activatable ability's current
  `IsOn` state is the sole mechanical source of truth; no marker fact is
  reconciled or changed by a read.
- Removes KMG's `UnitEntityData.PostLoad` paper-mode patch. Loading a save no
  longer adds, removes, stops, or otherwise changes serialized unit facts while
  Kingmaker is hydrating them.
- Retains only lightweight cache invalidation in the activatable `set_IsOn`
  postfix. It does not alter facts, markers, abilities, or saved state.
- A stale Paper Cartridge marker is intentionally ignored mechanically. Toggle
  off selects loose ammunition and its normal action economy; toggle on selects
  paper cartridges with no loose-ammunition fallback.
- Restores concise local descriptions for generic Nodachi and every named
  eastern weapon. This prevents blank local descriptions from inheriting donor
  text containing `Brace`, which caused blueprint bootstrap validation to fail
  before a save could be reached in the guarded runtime environment.

The hotfix preserves all existing blueprint, item, toggle, marker, and reload
identities. It makes no save migration and does not modify a user's save files.
The 0.0.112 ammunition pricing, targeted Craft Magic Items bridge, item-copy,
and notification repairs remain in effect. No production reference to or package copy
of `CraftMagicItems.dll` is included.

## Verification

The release workflow runs source validation, all 1,373 DomainTests, clean
Release builds, build-output and SoundBank validation, deterministic package
creation, strict package validation, and guarded Steam save-load qualification
against a disposable working save. Runtime evidence is recorded with the
candidate rather than inferred from compilation.

The production firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`
without modification.