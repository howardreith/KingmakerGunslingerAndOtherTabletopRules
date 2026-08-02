# Sprint 61 later equipment acquisition and economy entry criteria

## Authority and scope

Mission section 4.5 requires a documented acquisition path for later firearms,
ammunition, and required repair resources. The roadmap assigns vendor or loot
integration to the playable-content checkpoint. ADR-0035 defers the repair kit
specifically to vendor, loot, or crafting distribution. Existing blueprint item
costs remain authoritative; this sprint makes no new price or balance decision.

## Evidence-first boundary

The local candidate ledger names Oleg and a capital merchant only as narrative
candidates and records their exact vendor-table IDs as unresolved. Before any
production catalog mutation, a guarded save-free and non-initiating observer
must establish from the installed Kingmaker 2.1.7b blueprint graph:

- the exact vendor-table blueprint type and content-member contract;
- exact stable IDs and readable identities for suitable early-game and capital
  merchant tables;
- whether entries are direct item references or weighted/quantity records;
- duplicate behavior and the native refresh/restock boundary;
- a narrow append and rollback contract that leaves native entries unchanged.

The observer may inspect metadata and construct detached value objects. It must
not open a shop, invoke a merchant, buy, sell, refresh inventory, load a save,
or mutate a live vendor table.

## Observable production contract

After the observer proves one unambiguous native contract:

- a normal campaign merchant route exposes player-fireable production firearms
  appropriate to that route, basic black powder, lead balls, and repair kits;
- the intentionally unavailable Blunderbuss remains excluded until scatter
  delivery is runtime-qualified;
- existing item costs, weights, stackability, firearm identity, ammunition
  identity, and repair-kit behavior remain unchanged;
- native entries retain reference identity and order; project entries are
  appended exactly once and initialization remains idempotent;
- a missing, ambiguous, duplicate, or unsupported vendor contract fails closed
  before publication;
- the acquisition path and its placeholder-visual limits are documented for
  players without requiring another gameplay mod.

## Qualification

- Focused source/domain checks cover exact entry construction, duplicate
  rejection, native-entry preservation, idempotence, and rollback.
- Repository validation, the complete domain suite, clean exact-reference
  Release build, strict package validation, request/preflight checks, and
  staged safety audits must pass.
- Exact mod-load PASS is required for each committed runtime observer or
  production candidate.
- The non-initiating observer must pass before production mutation is selected.
- Final guarded runtime qualification inspects the exact registered vendor
  tables from two independent fresh Kingmaker processes without opening a shop
  or loading/writing a save.

## Non-goals

No crafting subsystem, random loot injection, UI automation, purchase, sale,
save mutation, new prices, magical firearms, custom vendor, custom art, enemy
firearm access, or scatter enablement is authorized by this checkpoint.
