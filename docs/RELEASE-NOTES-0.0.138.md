# Kingmaker Gunslinger 0.0.138

Release: `0.0.138-better-vendors-progression`
Package: `KingmakerGunslinger-0.0.138-better-vendors-progression.zip`
Build label: Kingmaker Gunslinger 0.0.138.
Publication status: **candidate, not released.** It has no owner
authorization, no tag and no public package.

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
  tiers, and one at Military VII also +4. A save-local ledger records each
  entry's first grant, so nothing is granted twice. Better Vendors' own save
  flag is never read or changed.
- The Gunslinger, Eastern Weapons and Elven Branched Spear module settings
  apply independently. A disabled module's weapons are not added.
- The new weapons are always registered, so saved items resolve. If a check
  specific to this integration fails, only merchant progression is disabled
  and one `progression-catalog.degraded` warning is logged; the rest of the mod
  is unaffected. For example, another mod may have changed a native
  enhancement enchantment.

## What deliberately did not change

- Better Vendors is **optional**. Without it, or with a different build, only
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
| Complete domain suite | PASS, 1,740 of 1,740 |
| Clean Release build and build-output validation | PASS, no warnings |
| Strict standalone UMM package validation | PASS |
| Better Vendors contract fingerprints vs installed 2.0.8 binary | PASS (static) |
| Guarded `working-save-smoke` on commit `060df1d4` | PASS, 11 of 11 assertions |
| Adapter resolution against the live Better Vendors 2.0.8 assembly | PASS (`compatibility.ready`) |
| All 50 progression entries registered and validated in game | PASS (`progression-catalog.ready`) |
| Merchant stock in a kingdom-stage campaign | NOT RUN |
| Save/load compatibility workflow | NOT RUN |

Historical domain checkpoints of 1,251, 1,288 and 1,325 cases remain archived
under their original releases. The live suite for this candidate has 1,740
cases.

The guarded run `20260923T1810242389403Z-1bf61c4530d14e879a16919b182a9602`
loaded `KMG_AUTOMATION_WORKING` through Steam App ID 640820, with Better
Vendors 2.0.8 among the 16 loaded mods. It made no save-writing call. The
deployed artifact was DLL `285cc5bb…1fdd`, MVID
`d49f0f7d-c1be-43dd-970b-64a2b5fb58bf`, with commit `060df1d4` embedded. The
prior installation was restored byte-for-byte afterwards.

The only authorized disposable save predates kingdom creation, so Better
Vendors' Military progression cannot occur in it. No kingdom-stage fixture is
authorized. The details and the exact verified identity are in
[BETTER-VENDORS-COMPATIBILITY.md](BETTER-VENDORS-COMPATIBILITY.md).

## Compatibility

Better Vendors support is claimed only for the exact installed 2.0.8 binary:
MVID `04fc03cf-853f-46c8-b6d5-1404180451fb`, file SHA-256
`8843509852964d9016d2996a3050bfbe6f068360c4f2f6b8ff7f6440ca712009`.
Other optional-mod compatibility is unchanged and was not re-tested. Craft
Magic Items compatibility is unchanged. There is no static
`CraftMagicItems.dll` dependency, and its compatibility profile keeps its
existing NOT-TESTED disposition.

## Existing characters and saves

No existing item, feat or identity changes. The ledger is created only just
before a campaign's first recorded grant. Turning off vendor progression,
disabling Better Vendors or turning off a module stops future additions
without deleting stock, items or the ledger.

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
