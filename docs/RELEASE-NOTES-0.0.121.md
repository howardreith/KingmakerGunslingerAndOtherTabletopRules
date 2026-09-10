# Kingmaker Gunslinger 0.0.121

Informational version: **0.0.121-unified-firearm-maintenance**.
The owner reviewed the change on PR #11, accepted it, and explicitly authorized
merge, push, and public release.

## Unified firearm maintenance

There is now exactly one maintenance action: **Repair Firearm**. One full-round
use restores a **Broken or Wrecked** firearm directly to **Normal** — no
intermediate step, no separate Overhaul action, and no second player action.

- Repair requires one **reusable Gunsmith's Kit**
  (`KMG.Gunsmithing.GunsmithKit`) in the shared party inventory. The kit is
  never consumed, loses no charges, and does not need to be equipped.
- Repair spends **nothing**: no gold, grit, ammunition, crafting entitlement,
  or rest allowance, and it is not connected to ammunition crafting's
  once-per-rest limit.
- The **Firearm Repair Kit** and **Firearm Overhaul Kit** are no longer
  required, consumed, or sold. Existing player-owned copies remain as inert
  obsolete items; their blueprint identities stay registered for save
  compatibility. Obsolete kits never substitute for a missing Gunsmith's Kit.
- **Ammunition is preserved:** surviving loaded rounds keep their exact count
  and ammunition identity through repair. A Wrecked firearm, which is always
  empty, stays empty. Nothing is regenerated or refunded.
- The exact runtime item, enchantments, and origin metadata are preserved; a
  magical firearm is never replaced by a basic one, and the battered
  starting-weapon origin is untouched.
- The Repair Firearm blueprint identity, display name, effective icon, and
  full-round action economy are unchanged.
- Ammunition crafting and its gold costs are unchanged.

## Old-save compatibility

The legacy `KMG.Test.OverhaulAbility` identity remains registered as a
**hidden, autofill-ignored delegate** of the unified repair, so an existing
character's saved Overhaul fact can never restore a second, kit-consuming
recovery action. The guarded `working-save-unified-repair-alias` scenario
loaded the genuine two-fact-era `KMG_AUTOMATION_WORKING` save (which persists
both the Overhaul fact and a saved action-bar slot) through the authorized
receiver-bound workflow: **PASS 16/16** (run `20260910T0344086090801Z`).
Exactly one visible maintenance action remained across all 106 deserialized
units, the persisted fact resolved to the hidden delegate, and the materialized
42-slot action bar contained zero slots wrapping the Overhaul blueprint — the
engine's own slot cleanup drops the obsolete button at load.

## Merchant stock

Both consumable kits were removed from every shop publication (capital
blacksmith, Bokken, campaign and standalone Beneath the Stolen Lands tables),
including idempotent cleanup of already-injected blueprint rows. Inventories an
earlier version already materialized into a saved merchant are swept as well:
`RetiredKitVendorStockCleanup` removes the two exact retired item identities
from a vendor's own inventory every time trading begins — never regenerating
stock, replenishing purchased merchandise, or touching player-owned legacy
kits. The guarded maintenance scenario qualified the real integration on a
genuine OTP_Bokken merchant receiver: **PASS 32/32**
(run `20260910T0350091184050Z`).

## Qualification status

- Repository source validation (0.0.121 chain): PASS.
- Domain test suite: **1,554 tests, 0 failures** (Release, clean).
- Two deterministic qualified Release builds and strict standalone UMM package
  validation (performed by the release script).
- Guarded in-game scenarios at the release source state: unified maintenance
  **32/32 PASS** and old-save alias **16/16 PASS** through Steam App ID 640820
  with disposable fixtures and the authorized working save only.

## Disclosed open item

Whether **Wrecked**-firearm repair should be available during active combat is
an explicit owner decision recorded in `KNOWN-ISSUES.md`. Historical Broken
repair never had a combat gate and historical Wrecked Overhaul rejected
combat; the current unified Repair has no gate, so only the Wrecked-in-combat
case changed behavior. No design was invented for this release; the code
matches the reviewed PR and the decision remains open.

## Retained audio and asset baseline

The installable archive is
`KingmakerGunslinger-0.0.121-unified-firearm-maintenance.zip`. The qualified
firearm SoundBank is retained unchanged: `KMG_Firearms.bnk` SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18` (see
`assets/soundbanks/firearm-soundbank-manifest.json`).

## Retained compatibility posture

Optional-mod compatibility is unchanged from 0.0.120: Craft Magic Items
coexistence keeps the runtime-loadable profile boundary and this package
still ships no foreign assemblies — `CraftMagicItems.dll` remains an
externally installed dependency of that mod, never a bundled file. The
inherited 1,288-test overhaul and 1,325-test fatigue-authority checkpoints
remain historical evidence.

Current development-artifact evidence includes 1,554 domain/reflection tests,
the guarded unified-maintenance run (32 assertions), and the old-save alias
run (16 assertions). These belong to their exact recorded development DLLs.
The manifest contains 1,883 identities: 1,881 active and two reserved.
