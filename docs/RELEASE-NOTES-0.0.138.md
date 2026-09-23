# Kingmaker Gunslinger 0.0.138

Release: `0.0.138-better-vendors-progression`
Package: `KingmakerGunslinger-0.0.138-better-vendors-progression.zip`
Build label: Kingmaker Gunslinger 0.0.138.
Publication status: **candidate, not released.** It has no owner
authorization, no tag and no public package. Merge and release are blocked
until the merchant and persistence acceptance areas below have evidence from
an owner-authorized disposable kingdom-stage save.

The qualified firearm SoundBank is unchanged, SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

## What changed

Optional **Better Vendors** compatibility for magic-weapon progression.

- When Better Vendors 2.0.8 is installed and enabled, and its vendor progression
  is on, the capital blacksmith gains this mod's generic magic weapons as the
  kingdom's Military rank rises. They arrive at the same ranks and quantities as
  Better Vendors' own +1 to +5 weapons: +1 at Military I, +2 at III, +3 at V,
  +4 at VII and +5 at IX. The first three tiers add 5 copies of each entry and
  the last two add 2.
- The 50 catalog entries are:
  - Pistol, Musket and Blunderbuss +1 to +5;
  - Reliable Pistol, Musket and Blunderbuss +1 to +5;
  - Elven Branched Spear, Wakizashi, Katana and Nodachi +1 to +5.

  The seven existing +1 items are reused unchanged. The other 43 are new
  blueprints with stable identities.
- Each weapon carries one native enhancement enchantment and its family's
  canonical weapon type, model, icon and weight. Its price covers the complete
  package: base + 300 + 2000 × (equivalent bonus)².
- **Reliable** variants use the existing Reliable enchantment, which reduces
  misfire by 1 to a minimum of 0; a natural 1 still misses. They unlock by their
  actual enhancement, so Reliable Pistol +1 arrives with the other +1 weapons.
  They cost as a +(N+1) weapon.
- Better Vendors re-adds the current Military tier's stock whenever a kingdom
  stat improves. These weapons follow the same rule, and there is no other
  restocking.
- Existing campaigns get a one-time **catch-up** the first time the capital
  blacksmith opens trade. A kingdom at Military V receives the +1, +2 and +3
  tiers, and one at Military VII also +4.
- A save-local ledger claims each entry's first grant *before* adding its
  stock. The claim is released only when the addition is positively confirmed
  to have changed nothing. A stock change whose bookkeeping failed or is
  uncertain is therefore never repeated automatically. Better Vendors' own save
  flag is never read or changed.
- The integration works only with the **exact approved Better Vendors
  binary**: its whole-file SHA-256 and module identity must match. Any other
  build, including a rebuilt 2.0.8, leaves only this integration inactive.
- The Gunslinger, Eastern Weapons and Elven Branched Spear module settings
  apply independently. A disabled module's weapons are not added.
- The new weapons are always registered, so saved items resolve. If a check
  specific to this integration fails, only merchant progression is disabled
  and one `progression-catalog.degraded` warning is logged; the rest of the mod
  is unaffected. For example, another mod may have changed a native
  enhancement enchantment.

## What deliberately did not change

- Better Vendors is **optional**. Without it, or with any other build, only
  this integration stays inactive and the log says why.
- Better Vendors' own stock, queries and other features are not modified. None
  of these weapons join its corrosive or elemental queries.
- No Rifle or Revolver, named, elemental, corrosive, special-material or
  Greater Reliable variants were added.
- Ordinary vendor publication, BTSL support, campaign loot, named-item placement
  and Craft Magic Items registration are unchanged, including the existing +1
  firearm stock.
- Existing blueprint identities are unchanged. The new identities are appended
  to the manifest (1956 in total).

## Verification status

| Gate | Result |
| --- | --- |
| Version-aware repository validation | PASS |
| Complete domain suite | PASS, 1,742 of 1,742 |
| Clean Release build and build-output validation | PASS, no warnings |
| Strict standalone UMM package validation | PASS |
| Exact Better Vendors 2.0.8 binary identity and contract vs the installed binary | PASS (static) |
| Guarded `working-save-smoke` on the final code commit `c9427faa` | PASS, 11 of 11 assertions (see below) |
| Exact-binary gate accepted the live Better Vendors 2.0.8 assembly | PASS (`compatibility.ready`) |
| All 50 progression entries registered and every contract check passed in game | PASS (`progression-catalog.ready`, not `degraded`) |
| Actual stocking and purchase at a known Military rank | NOT RUN, blocks release |
| Persistence and bought-out stock across a full restart | NOT RUN, blocks release |
| Native event coordination (real stat improvement, both orderings) | NOT RUN, blocks release |
| Progression and module settings, shared outdoor/throne-room stock | NOT RUN, blocks release |
| Purchased Reliable firearm mechanics after save and load | NOT RUN, blocks release |
| Craft Magic Items handling of the new variants | NOT TESTED |

Historical domain checkpoints of 1,251, 1,288 and 1,325 cases remain archived
under their original releases. The live suite for this candidate has 1,742
cases.

The guarded run `20260923T2036170490182Z-e8ae7b5631174645a40db7ccb7b89a38`
on `c9427faa` loaded `KMG_AUTOMATION_WORKING` through Steam App ID 640820,
with Better Vendors 2.0.8 among the 16 loaded mods, and made no save-writing
call. The exact-binary gate accepted the live assembly. The deployed artifact
was:

- package `20132261467f0d93ec501fda9ca0d495917ac8ce60ca99744935f59d5e72820f`;
- DLL `461183e8f4f193dbdba96068ab97a502ea7846dc2191f88b33ceb8861b81adca`;
- MVID `2d98ffc8-6c2b-4c06-bd3e-31d81f820211`, with commit `c9427faa`
  embedded.

Earlier runs on `060df1d4` and `cca27056` also passed. After each run, the
prior installation was restored byte-for-byte.

The domain suite models scheduling, planning and bookkeeping with a
dictionary shop and an in-memory ledger. It does not serialize saves, drive the
real trading hook or buy anything. The only authorized disposable save
predates kingdom creation, so Better Vendors' Military progression cannot occur
in it. No kingdom-stage save is authorized, and none was fabricated. The
acceptance requirements and the exact verified identity are in
[BETTER-VENDORS-COMPATIBILITY.md](BETTER-VENDORS-COMPATIBILITY.md).

## Compatibility

Better Vendors support is limited to, and enforced for, the exact installed
2.0.8 binary: MVID `04fc03cf-853f-46c8-b6d5-1404180451fb`, file SHA-256
`8843509852964d9016d2996a3050bfbe6f068360c4f2f6b8ff7f6440ca712009`.
Other optional-mod compatibility is unchanged and was not re-tested. Craft
Magic Items compatibility is unchanged. There is no static
`CraftMagicItems.dll` dependency, and its compatibility profile keeps its
existing NOT-TESTED disposition. Whether Craft Magic Items handles the new
variants correctly was not tested.

## Existing characters and saves

No existing item, feat or identity changes. The ledger is created only just
before a campaign's first initial grant. Turning off vendor progression,
disabling Better Vendors or turning off a module stops this integration's
future additions. The integration itself never removes merchandise, items or
ledger entries. Separately, when a content module is turned off, the existing
vendor-row reconciliation can still reduce a reused +1 weapon's merchant stock
by one.

## Install and uninstall

Install with Unity Mod Manager as usual: select the package zip in UMM and let
it deploy, then launch through Steam.

To uninstall, remove the mod through Unity Mod Manager. That removes every
blueprint the mod owns, including the new weapons, whether already stocked at
the blacksmith or carried by the party. Uninstall only from a save you are
willing to keep testing.

To roll back to 0.0.137, use a save made before 0.0.138 was installed. A save
written under 0.0.138 can contain the ledger or the new item identities, which
0.0.137 does not have. Whether it loads cleanly under 0.0.137 was not tested.
