# Better Vendors progression compatibility (0.0.138 candidate)

Status: **candidate, not released; merge and release are blocked.** Static
contract verification, the complete domain suite and a guarded load smoke run
pass. The real merchant and persistence paths are **not** qualified. They
need an owner-authorized disposable kingdom-stage save; see
[Acceptance requirements](#acceptance-requirements-before-merge-or-release).

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

The Military weapon schedule is identical in both. Support is limited to the
verified 2.0.8 binary, not anything the source alone suggests, and the
adapter enforces this (see below).

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

1. finds the live UMM entry with id `BetterVendors`. It tries at package load
   and again on the first UMM update, so load order does not matter;
2. as soon as that entry exists, installs its `VendorLogic.BeginTrading`
   postfix, under its own Harmony owner
   `KingmakerGunslinger.better-vendors-progression.trading`. This postfix does
   nothing until step 6 has succeeded. Its only other job is to retry
   resolution lazily, and only when the capital blacksmith (or its throne-room
   clone) opens trade. That covers a Better Vendors assembly that loaded late;
3. requires the **exact approved binary**. The whole-file SHA-256 of the
   loaded assembly's file and the loaded module's MVID must both equal the
   values above. An unknown, rebuilt or unreadable binary leaves only this
   integration inactive. The UMM version label is only logged;
4. reflects the required types and members, the `Verdel` destination, the
   five native enhancement GUIDs and both Harmony patch targets, which
   resolves the members the hooks need;
5. hashes the IL bodies of `AddStock`, `AddMilitaryStock`, `GetFilterWeapons`
   and both lifecycle postfixes with SHA-256, as a consistency check. These
   fingerprints are not relied on to prove behavioural equivalence:
   instruction bytes cover neither the helper methods they call nor their
   exception-handling clauses. That is why the binary identity is the gate;
6. only then installs its three Better Vendors hooks under Harmony owner
   `KingmakerGunslinger.better-vendors-progression`.

If Better Vendors is absent, only this integration stays inactive. Any other
binary, including a rebuilt 2.0.8, disables only this integration with one
actionable warning naming the failed check. An unexpected exception fails
closed with one error. Better Vendors, its other features and the rest of this
mod keep working in every case.

Every stock event reads Better Vendors' enabled state and progression toggle
live. Turning either off stops future additions at once.

**Registration never stops the mod.** The 43 new blueprints are registered
on every load, with or without Better Vendors, so saved items always resolve.
Checks specific to this integration run after registration and cannot throw.
They cover:

- the native `Enhancement1`–`Enhancement5` names, costs and single
  non-stacking bonus;
- Reliable's +1 cost;
- each entry's enchantments, price, weapon type, weight, text, icon and model.

If any check fails, for example because another mod changed a native
enchantment, every item stays registered and only merchant progression is
disabled. One `progression-catalog.degraded` warning lists the failed checks.
Players who never use Better Vendors are unaffected.

### Hooks

| Hook | Kind | Purpose |
| --- | --- | --- |
| `ProgressionLogic.AddStock` | postfix | closes the per-pass scope; the binary's `AddStock` swallows inner exceptions, so this always runs |
| `ProgressionLogic.AddMilitaryStock(int)` | prefix + postfix | classifies the call and applies this mod's matching tier after Better Vendors' own call completes |
| `ProgressionLogic.GetFilterWeapons` | postfix, read-only | records whether Better Vendors itself selected a catalog entry; never modifies the result |
| `VendorLogic.BeginTrading(UnitEntityData)` | postfix (separate `.trading` owner, installed before verification, inert until then) | one-time existing-save catch-up and lazy resolution retry, only for a vendor whose shared inventory is `SmithVendorTable` |

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
one-time *initial grant*. The part is created just before the first batch that
claims an initial grant. If it cannot be created, nothing is added. Campaigns
that never receive a grant carry no new data. The ledger is sorted, has no
duplicates, accepts only authorized catalog GUIDs and keeps unknown entries.

Initial grants are **claimed before stock changes** (write-ahead). For each
entry, the integration first reads the stack's current count, then writes the
entry's claim to the ledger, then adds the copies, then reads the count again:

- if either the first count or the claim fails, nothing was changed, so the
  entry stays eligible and a later trigger grants it;
- if the second count shows that nothing was added, the claim is released
  and a later trigger retries the entry;
- if copies were added, fully or partly, the claim stays. A partial addition
  is never topped up;
- if the result cannot be established, the claim stays. That happens when the
  second count fails or goes down, or when a claim that should be released
  cannot be.

So a stock change with failed or uncertain bookkeeping is never repeated
automatically. The cost of an unknowable outcome is at most one missed
grant, never a duplicate. Each outcome is counted in the log as `failed`,
`uncertain` or `skipped`, and the rest of the batch still applies. The ledger
and the stock live in the same save, so they persist or are discarded
together.

One exception is defensive. If Better Vendors' *own* query ever selects a
catalog entry, that entry is recorded as delivered by Better Vendors, without
this integration observing a stock change, whatever its module setting. This
path cannot occur with the verified 2.0.8 binary: every catalog entry fails
that query's empty-description filter.

**Mirroring Better Vendors' events.** Each `AddMilitaryStock` call is
classified:

- **Current tier.** The first call of a pass, for the current Military rank.
  Better Vendors repeats this call on every later stat improvement. The
  integration adds the current tier's entries at that tier's quantity. An
  entry receives its initial grant, or a **replenish**ment if its initial grant
  was already recorded. In a campaign whose ranks rise through normal kingdom
  actions, this matches Better Vendors' own restocking event for event. The
  exceptions are described under
  [Known limitations and intentional differences](#known-limitations-and-intentional-differences).
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
existing +1 vendor stock stays as it was.

This integration itself never removes merchandise. Its additions and Better
Vendors' are ordinary table entries, not `LootItemsPackFixed` rows. The 43 new
variants never appear in any fixed row, so the native reconciliation in
`SharedVendorTables.GetTable` never touches them.

The seven reused +1 items are different. This mod already publishes fixed
`SmithVendorTable` rows for them, one copy each while their module is on: the
Pistol, Musket and Blunderbuss +1 through the capital publication, the
Eastern +1 items through the Eastern campaign publication, and the +1 Elven
Branched Spear through the spear campaign publication. The native
reconciliation adjusts an item's combined stack by the change in its fixed
rows. So when such a module is turned off and the game reloaded, that item's
merchant stack drops by exactly one copy. If the fixed copy had already been
sold, that copy is a progression copy. Turning the module back on adds it back.
This is existing baseline vendor-row behaviour, not an action of this
integration. The `better-vendors.reused-fixed-row-reconciliation` domain case
pins it.

Craft Magic Items is not required. The new entries are not added to this mod's
Craft Magic Items registration catalog and gain no crafting privileges.
Whether Craft Magic Items handles the new variants correctly is **not
tested**. This stays an open verification item until a representative
purchased variant has gone through Craft Magic Items' crafting and upgrade
path.

Better Vendors' manual, player-driven item-injection search lists every
library item. It already listed this mod's items and is unchanged.

## Disabling and uninstall

Turning off vendor progression, disabling or removing Better Vendors, or
turning off a content module stops this integration's future additions. The
integration itself never removes merchandise, ledger entries or items, and
every blueprint stays registered. One baseline effect is separate: when a
content module is turned off, the existing vendor-row reconciliation can still
reduce a reused +1 weapon's merchant stock by one (see
[Other acquisition paths](#other-acquisition-paths)).

**Uninstall** of *this* mod is not made safe by this candidate. As before,
removing the mod removes every blueprint it owns. That includes weapons
already stocked in `SmithVendorTable` or carried by the party. Uninstall only
from a save you are willing to lose.

**Rolling back** to 0.0.137 means using a save made *before* 0.0.138 was
installed. A save written under 0.0.138 can contain the ledger part or any of
the 43 new item identities, which 0.0.137 does not have. Such a save may not
load cleanly under 0.0.137; that was not tested.

## Logs

UMM log lines use phase `better-vendors`:

- `compatibility.ready`, `.notinstalled`, `.installedinactive`, `.incompatible`
  or `.faulted`, each logged once per state change;
- `progression-catalog.ready` at bootstrap, or `progression-catalog.degraded`
  with the failed checks when merchant progression is disabled;
- `stock-call.applied` and `catch-up.applied`, with counts: granted,
  replenished, copies, partial, failed, uncertain, skipped, suppressed by
  module, and ledger size;
- `*.failed`, logged once per distinct failure, then `*.repeated` warnings.

## Known limitations and intentional differences

1. **Limitation: Military rank changed outside a kingdom action.** Suppose the
   rank rises without `KingdomActionImproveStat`, for example through a console
   or cheat-menu command. Better Vendors does not react. This integration's
   catch-up grants the tier once when the blacksmith opens trade. At the next
   stat improvement, Better Vendors adds its own copies for the first time and
   this integration replenishes. This mod's entries then have twice Better
   Vendors' quantity for that tier. No compensation is attempted for
   out-of-band rank edits. Normal kingdom progression and existing-save
   migration are the supported baseline.
2. **Intentional difference: progression off while ranks rise.** If vendor
   progression is off while ranks rise and is then switched back on, Better
   Vendors never back-fills its own skipped tiers, because its flag is already
   `1`. This integration's catch-up does back-fill its entries, as requested:
   weapons that are already unlocked become available once the integration is
   active.
3. **Baseline effect: module off.** Turning a content module off can reduce a
   reused +1 weapon's merchant stock by one through the existing vendor-row
   reconciliation (see [Other acquisition paths](#other-acquisition-paths)).
4. **Not tested: Craft Magic Items** handling of the new variants.

## Acceptance requirements before merge or release

The domain suite models the scheduling, planning and bookkeeping rules with a
dictionary shop and an in-memory ledger. It does not serialize a save,
reconstruct the ledger part, drive the real trading hook or buy anything. The
guarded smoke run proves loading and contract resolution only. So these
acceptance areas remain **NOT RUN** and block merge and release. They need an
explicitly owner-authorized, disposable kingdom-stage save. That save must
not be fabricated, and a real campaign must not be used:

| Acceptance area | Evidence needed |
| --- | --- |
| Actual stocking and purchase | At a known Military rank, the correct ordinary and Reliable variants appear with the correct added quantities, and future tiers are absent. Representative firearm and melee variants can be bought. The existing fixed +1 stock is accounted for separately. |
| Persistence and bought-out stock | After grants and purchases: save, exit completely, restart and reload. The ledger survives, and opening trade does not restore purchased copies. |
| Native event coordination | A genuine kingdom stat-improvement event, and both orderings: Better Vendors' own stocking before catch-up, and catch-up before later native stocking. Unrelated merchandise is compared against a baseline. |
| Settings and shared inventory | Vendor progression and each content module disabled and re-enabled. The outdoor and throne-room blacksmiths show the same stock where both are reachable. |
| Purchased-item behaviour | A purchased Reliable firearm uses the real Reliable mechanics and keeps its properties after save and load, not merely the expected name. |

## Verification status

| Layer | Status |
| --- | --- |
| Contract identity and IL fingerprints vs installed 2.0.8 binary | PASS (static, read-only) |
| Domain suite (38 `better-vendors.*` cases: catalog, schedule, classification, planning, write-ahead ledger and uncertain outcomes, lifecycle, modules, exact-binary gate, degrade-closed registration, fixed-row reconciliation, hooks, acquisition isolation) | PASS |
| Repository validation, clean Release build, package validation | recorded in the release notes |
| Guarded `working-save-smoke`, commit `cca27056` (before the exact-binary gate and write-ahead ledger) | PASS, 11 of 11 assertions (see below) |
| Adapter resolution and hook installation in game | PASS: `compatibility.ready` at package load against the live 2.0.8 assembly |
| Registration and all progression contract checks in game | PASS: `progression-catalog.ready`, not `degraded` |
| All five [acceptance areas](#acceptance-requirements-before-merge-or-release): stocking and purchase, persistence, native event coordination, settings, purchased-item behaviour | NOT RUN, **blocks merge and release**. The only authorized disposable save predates kingdom creation, and no kingdom-stage save is authorized. |
| Craft Magic Items handling of the new variants | NOT TESTED |

The guarded run `20260923T1845318979451Z-503bdaa28b564297b19a1c2a9763c8d1`
launched through Steam App ID 640820. It loaded `KMG_AUTOMATION_WORKING`
without any save-writing call; that save and `KMG_AUTOMATION_BASELINE` kept
their prior timestamps. The deployed artifact was:

- package `d9ba82339c52490406273f56b05824a63b9013a60b3e8a21bedaa5a973fa84e7`;
- DLL `34fe16ad05527cba5caad37a4d7ce7727d0b1553ec38511675017c6d2776d4fe`;
- MVID `c7ccdfcf-42a8-4b82-a241-bcc3a0310ac8`, with commit `cca27056`
  embedded.

UMM loaded all 16 installed mods, including Better Vendors 2.0.8. The same
session's UMM log recorded exactly these two `better-vendors` lines:

```text
[better-vendors][compatibility.ready] checkpoint=package-load;status=Ready;Verified Better Vendors mod=2.0.8;assembly=BetterVendors 1.0.0.39014;mvid=04fc03cf-853f-46c8-b6d5-1404180451fb;sha256=8843509852964d9016d2996a3050bfbe6f068360c4f2f6b8ff7f6440ca712009; hooks=AddStock,AddMilitaryStock,GetFilterWeapons,BeginTrading;progressionNow=active
[better-vendors][progression-catalog.ready] entries=50;reused=7;registered=43;firearms=30;melee=20;publishedToVendors=false
```

There were no failure, degraded or stock lines, and no Gunslinger error lines,
as expected for a save without a kingdom. These lines show that the live
fingerprints matched and that every progression contract check passed. They
are not evidence of merchant stock behaviour.

An earlier guarded run on the first candidate commit `060df1d4`
(`20260923T1810242389403Z-1bf61c4530d14e879a16919b182a9602`) also passed all
11 assertions with the same two log lines. After each run, the prior live
installation was restored from its pre-deployment backup. The final tree is
byte-identical to the state before the first run: 238 files, 0.0.136-labelled
DLL `c6cccdac…465c`.

Any Better Vendors build other than the exact 2.0.8 binary is rejected by the
gate, leaving only this integration inactive. Unverified combinations:

- other mods that patch the same Better Vendors methods or `SmithVendorTable`;
- Call of the Wild or Tweak or Treat weapon additions to the same queries;
- external save editing of the ledger or of `SmithVendorTable`.
