# Better Vendors progression compatibility (0.0.138 candidate)

Status: **candidate, not released**. Static contract verification and the
complete domain suite pass. In-game qualification of the stock behaviour has
**not** been observed (see [Verification status](#verification-status)).

When the optional Better Vendors mod is installed and enabled, and its vendor
progression is switched on, this mod's generic magic weapons now progress at
the capital blacksmith alongside Better Vendors' own +1 to +5 weapons.
Better Vendors stays optional. Without it, nothing in this document happens and
the rest of the mod is unchanged.

## Why the weapons were missing

Better Vendors' Military stock uses one library-wide query,
`ProgressionLogic.GetFilterWeapons`. It keeps only weapons whose `FlavorText`
and `Description` are both empty. `BlueprintItemWeapon.Description` falls back
to the weapon type's magic description when the item has none. Every Gunslinger,
Eastern Weapons and Elven Branched Spear weapon type sets that description,
and Eastern items also carry flavour text. So the query never returned any of
this mod's weapons. The Reliable firearms would have failed anyway, because the
query wants exactly one enchantment. Also, +2 to +5 variants of these families
did not exist before this candidate.

The integration keeps those items out of Better Vendors' query. Its results are
never changed. The integration adds this mod's authorized entries itself, in
step with Better Vendors' own stock events.

## Verified Better Vendors identity

| Property | Value |
| --- | --- |
| UMM id / display name | `BetterVendors` / Better Vendors (author hambeard) |
| UMM version | 2.0.8 (`ManagerVersion` 0.22.9) |
| Assembly | `BetterVendors` 1.0.0.39014, references HarmonyLib 2.0.2 |
| MVID | `04fc03cf-853f-46c8-b6d5-1404180451fb` |
| File SHA-256 | `8843509852964d9016d2996a3050bfbe6f068360c4f2f6b8ff7f6440ca712009` |
| Military destination | `Verdel` → `SmithVendorTable` `7de959347266092448d8a72089ef9778` |

The contract was taken from the installed 2.0.8 binary through read-only
reflection and decompilation. The Nexus Mods page (Pathfinder: Kingmaker mod
173) returned HTTP 403 and could not be inspected. Whether its current
download is byte-identical to this binary is therefore unverified. The public
GitHub source (`thehambeard/BetterVendors`, tag 1.0.7, commit `3cb12966`) is
**older than the binary** and differs from it:

- the source uses Harmony12, while the binary uses HarmonyLib 2.0.2;
- the source stocks arcane items at Arsinoe, the binary at Zarcie;
- the source's `AddStock` has no swallow-all `try/catch`, the binary's does.

The Military weapon schedule is identical in both. Support is claimed only for
the verified 2.0.8 binary, not for anything the source alone suggests.

### Enablement and lifecycle (as implemented by 2.0.8)

- `ProgressionLogic.AddStock()` returns at once when there is no kingdom, when
  Better Vendors' `Main.Mod.Enabled` is false, or when
  `SettingsWrapper.ToggleVendorProgression` is off.
- **First pass.** Better Vendors keeps a flag in the main character's
  `FreeformData["stockUpToDate"]`. While it is `0`, `AddStock` calls
  `AddMilitaryStock(k)` for every rank `k` below the current Military rank,
  then sets the flag to `1`.
- **Every pass.** `AddStock` then calls `AddMilitaryStock(currentRank)` once.
- **Triggers.** `AddStock` runs from a `Player.PostLoad` postfix, only while the
  flag is still `0`. It also runs from a `KingdomActionImproveStat.RunAction`
  postfix whenever Arcane, Divine, Military or Stability improves.
- **Additive stock.** `AddItemsToVendorStock` builds fixed loot rows and runs
  the native `AddItemsToVendor` action, which calls
  `SharedVendorTables.GetTable(table).Add(item, count)`. Stock is only ever
  added; nothing is set, removed or reset.
- **Military schedule.** Ranks 1, 3, 5, 7 and 9 add every matching +1, +2, +3,
  +4 and +5 generic weapon, at 5, 5, 5, 2 and 2 copies each. Ranks 6 and 8
  add corrosive and bow elemental variants. Ranks 2, 4 and 10 add no weapons.
- **Restocking.** Better Vendors' only restocking is that every later stat
  improvement re-adds the *current* Military rank's stock. Nothing restocks on
  opening trade, changing area or reloading.

This integration never reads, writes, resets or depends on `stockUpToDate`.
It never triggers a Better Vendors pass itself.

## Dependency contract and failure behaviour

There is no compile-time reference, no UMM `Requirements` entry and no
redistributed Better Vendors code. The adapter:

1. finds the live UMM entry with id `BetterVendors`. It retries at package
   load, on the first UMM update and lazily when trading starts, so load order
   does not matter;
2. reflects the required types and members, the `Verdel` destination, the
   five native enhancement GUIDs and both Harmony patch targets;
3. hashes the IL bodies of `AddStock`, `AddMilitaryStock`, `GetFilterWeapons`
   and both lifecycle postfixes with SHA-256. All five must match the verified
   2.0.8 bodies. The version label, MVID and file hash are logged for
   diagnosis, but the fingerprints decide;
4. only then installs its hooks under Harmony owner
   `KingmakerGunslinger.better-vendors-progression`.

If Better Vendors is absent, only this integration stays inactive. An
installed build that does not match, including a rebuilt 2.0.8, disables only
this integration with one actionable warning naming the failed check. An
unexpected exception fails closed with one error. Better Vendors, its other
features and the rest of this mod keep working in every case.

Every stock event reads Better Vendors' enabled state and progression toggle
live. Turning either off stops future additions at once.

### Hooks

| Hook | Kind | Purpose |
| --- | --- | --- |
| `ProgressionLogic.AddStock` | postfix | closes the per-pass scope; the binary's `AddStock` swallows inner exceptions, so this always runs |
| `ProgressionLogic.AddMilitaryStock(int)` | prefix + postfix | classifies the call and applies this mod's matching tier after Better Vendors' own call completes |
| `ProgressionLogic.GetFilterWeapons` | postfix, read-only | records whether Better Vendors itself selected a catalog entry; never modifies the result |
| `VendorLogic.BeginTrading(UnitEntityData)` | postfix | one-time existing-save catch-up, only for a vendor whose shared inventory is `SmithVendorTable` |

The per-pass scope is thread-local and bound to the loaded campaign's `Player`.
It also starts lazily, so a failed or foreign pass cannot leak into a later
one. Every callback catches, logs once per distinct failure and returns.
Queries other than the ordinary single-enhancement tier query are never
observed: corrosive, elemental, bow, category and composite/thrown/oversized
queries are all excluded.

## Catalog

The explicit catalog in `ProgressionWeaponCatalog` holds exactly 50 entries,
with no name-based or scan-based inference. The machine-readable manifest is
[`better-vendors-progression-catalog.json`](better-vendors-progression-catalog.json).
It is generated by the domain-test utility `--write-better-vendors-catalog`,
and a domain test checks it against the code.

| Family | Module | Ordinary | Reliable | Reused +1 | Mundane base |
| --- | --- | --- | --- | --- | --- |
| Pistol | Gunslinger | +1 to +5 | +1 to +5 | Pistol +1 | 1000 gp |
| Musket | Gunslinger | +1 to +5 | +1 to +5 | Musket +1 | 1500 gp |
| Blunderbuss | Gunslinger | +1 to +5 | +1 to +5 | Blunderbuss +1 | 2000 gp |
| Elven Branched Spear | Elven Branched Spears | +1 to +5 | none | +1 Elven Branched Spear | 20 gp |
| Wakizashi | Eastern Weapons | +1 to +5 | none | +1 Wakizashi | 35 gp |
| Katana | Eastern Weapons | +1 to +5 | none | +1 Katana | 50 gp |
| Nodachi | Eastern Weapons | +1 to +5 | none | +1 Nodachi | 60 gp |

That is 30 firearm and 20 melee entries. Seven canonical +1 items are reused
with their GUIDs, symbols and presentation unchanged. The other 43 blueprints
are new and appended to `blueprints/blueprints.json` under milestone
"Better Vendors progression", for 1956 stable identities in total. The existing
1913 are untouched.

Each entry carries exactly one native enhancement enchantment, `Enhancement1`
to `Enhancement5`, with no masterwork enchantment and no special material. It
uses its family's canonical weapon type, model, icon and weight. New entries
share the family's approved +1 icon and visual variant; the icon catalog
records this as an intentional family share. Names follow the canonical
pattern: `Pistol +2`, `Reliable Pistol +3`, `+4 Katana`,
`+5 Elven Branched Spear`.

Price is the complete package: mundane base + 300 (masterwork) + 2000 ×
(equivalent bonus)². For an ordinary item the equivalent bonus is N. A
Reliable item's is N + 1. Examples: Pistol +2 is 9,300 gp, Reliable Pistol +1
is also 9,300 gp, Reliable Blunderbuss +5 is 74,300 gp, and +3 Katana is
18,350 gp.

### Reliable semantics

Reliable variants reuse the canonical Reliable enchantment and its runtime
unchanged. Reliable reduces the firearm's misfire value by 1 after other
increases, to a minimum of 0. A natural 1 still misses. A Reliable entry
unlocks by its **actual** enhancement: Reliable Pistol +1 arrives at Military I
with the other +1 weapons, not with the +2 tier its price equivalent would
suggest.

### Deliberate exclusions

The catalog has no Rifle or Revolver variants, and no legacy, NPC, battered or
diagnostic items. It adds no named or unique items, no elemental, corrosive or
corrosive/Reliable combinations, no special materials such as cold iron, no
Greater Reliable and no new weapon families. Named-item placement and fixed
loot are unchanged. Nothing in the catalog is placed in Better Vendors' rank 6
and rank 8 corrosive or elemental queries. Elemental and material variants are
deferred until a design decision exists.

## Stock semantics

This integration only adds items to `SmithVendorTable`, using the same native
shared-table operation Better Vendors uses. Better Vendors' throne-room
blacksmith clone reads that table too. The integration never removes, clears
or normalizes stock. It never uses current shop contents to decide anything, so
buying every copy never makes an entry look ungranted. The quantities in this
table are *additions*:

| Military rank | Tier | Entries | Copies added per stock event |
| --- | --- | --- | --- |
| I | +1 | 10 (7 ordinary + 3 Reliable) | 5 |
| III | +2 | 10 | 5 |
| V | +3 | 10 | 5 |
| VII | +4 | 10 | 2 |
| IX | +5 | 10 | 2 |

**Bookkeeping.** A save-local ledger (`UnitPartBetterVendorsProgressionGrants`
on the main character, schema 1) records which entries have received their
one-time *initial grant*. The part is created only on the first recorded grant,
so campaigns that never receive one carry no new data. The ledger is sorted,
has no duplicates, accepts only authorized catalog GUIDs and keeps unknown
entries. An entry is recorded only after its stock change is observed. An
addition that changed nothing stays unrecorded and is retried later. A partial
addition is recorded and never topped up. The ledger and the stock live in the
same save, so they persist or are discarded together.

**Mirroring Better Vendors' events.** Each `AddMilitaryStock` call is
classified:

- **Current tier.** The first call of a pass, for the current Military rank.
  Better Vendors repeats this call on every later stat improvement. The
  integration adds the current tier's entries at that tier's quantity. An
  entry receives its initial grant, or a **replenish**ment if its initial grant
  was already recorded. This mirrors Better Vendors' own restocking, no more
  and no less.
- **Catch-up.** Any lower-rank or later call inside Better Vendors' first-time
  pass. Only entries without a recorded initial grant are added. Better
  Vendors' own first pass therefore never duplicates this mod's grants.
- **Unsupported.** A call above the current rank or out of range. Nothing is
  added.

**Existing saves (catch-up).** A save can reach this candidate with Better
Vendors' flag already `1`. The first time the capital blacksmith (or its
throne-room clone) opens trade, every reached milestone's missing initial
grants are added once. Military V gives the +1, +2 and +3 tiers. Military VII
also gives +4. Future tiers are never granted. A save without a kingdom gets
nothing. If the vendor or catalog cannot be resolved, nothing is recorded, so
a later trade retries. Opening trade again, changing area or reloading adds
nothing more; none of them is a restocking trigger.

The same rules cover the other cases:

- enabling Better Vendors mid-campaign;
- re-enabling a content module;
- a later catalog expansion;
- loading saves out of order;
- switching campaigns.

In each case the ledger decides what is missing, and the pass scope belongs to
the loaded campaign.

## Content modules

The Gunslinger, Eastern Weapons and Elven Branched Spear module settings apply
independently, and each is read at every stock event. When a module is off,
its entries are not added, and they are not recorded either. The entries
become eligible at the next stock event or catch-up after the module is turned
back on. Their blueprints stay registered whatever the settings, so bought or
stocked items still resolve in existing saves.

## Other acquisition paths

The progression entries are a separate catalog. They are never published to
the capital vendor rows, the BTSL vendor support, the campaign loot and vendor
publications or the retired-stock cleanup. None of those paths change. The
existing +1 firearm vendor stock stays as it was. Better Vendors' own stock
additions are persistent table entries, not `LootItemsPackFixed` rows. The
native fixed-row reconciliation in `SharedVendorTables.GetTable` therefore
neither removes nor increments them.

Craft Magic Items is not required. The new entries are not added to this mod's
Craft Magic Items registration catalog and gain no crafting privileges. Craft
Magic Items' own library indexing is its own behaviour and was not tested with
these entries.

Better Vendors' manual, player-driven item-injection search lists every
library item. It already listed this mod's items and is unchanged.

## Disabling and uninstall

Turning off vendor progression, disabling or removing Better Vendors, or
turning off a content module only stops future additions. No item, ledger
entry or stock is deleted, and every blueprint stays registered.

**Uninstall** of *this* mod is not made safe by this candidate. As before,
removing the mod removes every blueprint it owns. That includes weapons
already stocked in `SmithVendorTable` or carried by the party. Uninstall only
from a save you are willing to lose, or roll back to 0.0.137.

## Logs

UMM log lines use phase `better-vendors`:

- `compatibility.ready`, `.notinstalled`, `.installedinactive`, `.incompatible`
  or `.faulted`, each logged once per state change;
- `progression-catalog.ready` at bootstrap;
- `stock-call.applied` and `catch-up.applied`, with counts: granted,
  replenished, copies, partial, failed, suppressed by module, and ledger size;
- `*.failed`, logged once per distinct failure, then `*.repeated` warnings.

## Verification status

| Layer | Status |
| --- | --- |
| Contract identity and IL fingerprints vs installed 2.0.8 binary | PASS (static, read-only) |
| Domain suite (35 `better-vendors.*` cases: catalog, schedule, classification, planning, ledger, lifecycle, modules, fail-closed contract, hooks, acquisition isolation) | PASS |
| Repository validation, clean Release build, package validation | recorded in the release notes |
| Adapter resolution and hook installation in game | NOT RUN |
| Merchant stock in a kingdom-stage campaign | NOT RUN: the only authorized disposable fixture predates kingdom creation, and no kingdom-stage fixture is authorized |
| Save/load of the ledger | NOT RUN |

Unverified combinations:

- any Better Vendors build other than the exact 2.0.8 binary;
- other mods that patch the same Better Vendors methods or `SmithVendorTable`;
- Call of the Wild or Tweak or Treat weapon additions to the same queries;
- external save editing of the ledger or of `SmithVendorTable`.
