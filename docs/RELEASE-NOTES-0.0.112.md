# Kingmaker Gunslinger 0.0.112

Release archive:
`KingmakerGunslinger-0.0.112-ammunition-cmi-copy-notifications.zip`.

## Ammunition crafting

- Every project-owned 20-unit firearm-ammunition batch now uses one 10%-of-
  retail policy, rounded up with a 1 gp minimum: Black Powder Charges cost
  20 gp, Lead Balls cost 2 gp, the combined loose-ammunition batch costs
  22 gp, and Paper Cartridges cost 24 gp.
- Native Gunsmithing verifies money immediately before commitment, charges
  exactly once, and restores money, inventory, and the shared rest marker when
  its owned transaction fails. Its Paper Cartridge card and runtime evidence
  now display the same 24 gp policy.
- The scoped Craft Magic Items path applies the exact KMG price only to KMG
  ammunition, restores CMI settings in every success and failure path, and
  never charges a completed timed project a second time.

## Reload and optional-mod correctness

- Use Paper Cartridges is now derived from the current native activatable
  ability state. Its hidden marker is reconciled immediately, on load, and at
  relevant presentation boundaries; it is never evidence by itself that the
  toggle is enabled.
- Reload preview, queued command, action economy, and inventory commitment use
  one coherent plan. Turning the mode off selects loose ammunition and its
  normal action economy; it never consumes a paper cartridge as fallback.
- The Craft Magic Items bridge uses targeted, idempotent augmentation and
  finalization of KMG-owned state. It never drives CMI through a false/true
  toggle, mutates its UMM lifecycle state, or replays `Main.Load`; no synthetic CMI toggle remains. No production`n  reference to or package copy of `CraftMagicItems.dll` is included.

## Player-facing copy

- Project-authored item descriptions are concise and retain only useful
  nonstandard behavior. The item-by-item review is recorded in
  `docs/ITEM-DESCRIPTION-AUDIT.md`; Moonlit Fork now describes its opportunity
  effect without restating its normal card properties.
- Reload and crafting failures are mapped to short actionable messages while
  technical diagnostics remain in the structured mod log. The deliberate
  broken and wrecked firearm notifications are unchanged.

The production firearm SoundBank remains SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`
without modification.

## Verification

The release retains the 1,288-test Overhaul, Summon, and Fatigue baseline; the 1,325-test icon baseline; the 1,348-test repair/notification baseline; the 1,359-test Protection from Alignment baseline; and the 1,370-test native outfit baseline alongside all subsequent validated subsystems.`n`nThe release workflow runs version-aware repository validation, all 1,372
DomainTests, two clean deterministic Release builds, strict build-output
validation, SoundBank validation, deterministic package creation, and strict
standalone UMM package validation.

Two guarded Steam launch observations loaded the pre-release candidate but did
not receive the runtime request callback before timeout. No save was loaded or
modified. The targeted in-game reload, CMI, and presentation scenarios remain
listed for human verification; this release records that limitation rather than
claiming those observations passed.