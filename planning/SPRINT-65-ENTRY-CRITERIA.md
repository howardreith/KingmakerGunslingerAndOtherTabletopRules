# Sprint 65 entry criteria: firearm item lifecycle

## Carrier and path model

Production state is owned by an inert enchantment token on the exact firearm
item. It is not keyed by wielder, collection, slot, display name, blueprint, or
the rejected identity vault. The final lifecycle qualification separates paths
that preserve one item from paths that create a new item:

| Path | Required state result |
|---|---|
| equip/unequip and weapon-set switch | exact item and token unchanged |
| party inventory, companion transfer, and shared stash | exact item and token follow the firearm |
| sale and repurchase of the same merchant item | token remains on that exact item |
| ordinary loot acquisition | newly created firearm begins empty/Normal |
| blueprint creation or duplication into a new item | new item begins empty/Normal and cannot inherit another item's token |
| save/load reconstruction of the same serialized item | token decodes to the same state |
| removal or destruction | no surviving item receives the removed token |

## Qualification stages

Stage A is save-free and may inspect exact installed collection/create/copy
contracts and exercise detached native items where constructors are proven
safe. It must not open UI, select a save, mutate a live inventory, vendor, or
stash, or claim persistence.

Stage B uses only a guarded, explicitly allowlisted scenario and the named
disposable working save. It must be request-scoped, restore every reversible
in-memory mutation, invoke no save API, and fail closed on ambiguous item
identity, collection ownership, token counts, or cleanup. Paths that inherently
require durable sale/restart evidence remain pending until a separately
authorized disposable-save protocol exists.

## Source and failure contract

Focused tests must prove same-item preservation, distinct-item isolation,
tokenless new-item creation, unknown/duplicate-token rejection, and no state
transfer after removal. Existing migration and reconstruction tests remain
mandatory. Any path whose native semantics cannot be established is recorded
as pending rather than inferred from another path.

## Non-goals

Sprint 65 does not parse, edit, copy, or overwrite saves; automate UI; prune
sold-item state speculatively; invent an item identity; or treat a detached
fixture as proof of save/restart or merchant persistence.
