# Sprint 65 firearm item lifecycle qualification

Sprint 65 separates exact-item token behavior from collection, merchant, and
save-backed paths. Source commit `1e206d1` adds a save-free observer using only
detached production Pistol items.

## Qualified Stage A behavior

- The installed runtime exposes exact `ItemsCollection.Remove(ItemEntity)`,
  `Extract(ItemEntity)`, blueprint-add, and `ItemsEntityFactory.CreateEntity`
  contracts.
- A loaded production firearm retains exactly one state token and decodes the
  same state after native `ItemEntity.ApplyEnchantments` reconstruction.
- A distinct item created from the same production blueprint starts canonical
  empty/Normal with zero state tokens.
- Removing the source repository state clears its token and does not transfer
  state to the distinct item.
- Both detached fixtures are cleaned up; no unit, collection, inventory,
  vendor, stash, or save is mutated.

## Evidence

Repository validation, 831/831 tests, 38 request checks, 84 preflight checks,
seven focused lifecycle checks, clean exact-reference Release build, and strict
package validation passed. Package/DLL SHA-256 are
`528d22fff0516528afdb9112c0775a837304761bc9055659bfcb72fadb68ffdd` /
`c6322627269e8503f57d3c2c2fcaf03ceab503eecbdd198cd7b3fdd056942fea`.

Exact mod load passed at
`20260802T1005415884560Z-mod-load-smoke`. Independent PASS runs:

- `20260802T1007011584387Z-observe-firearm-item-lifecycle-contracts`
- `20260802T1008259836323Z-observe-firearm-item-lifecycle-contracts`

Both observed native contracts true, source token counts `1->1->0`, created
token count zero, exact loaded-state reconstruction, canonical created state,
and successful source removal.

## Remaining boundary

This evidence does not prove live equip/transfer/stash/vendor ownership or
save/restart reconstruction. Those paths require the guarded named working-save
boundary, and durable sale/restart evidence additionally requires an explicitly
authorized disposable-save protocol. The coverage row remains
`SOURCE-QUALIFIED` rather than overstating Stage A.
