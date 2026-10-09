# Recovery of missed relocated campaign weapons

Changing loot definitions does not promise to refill generated inventories in an
old campaign. Recovery is an explicit, selected action for 28 historically
relocated named weapons. It is separate from ordinary discovery and crafting.
World-Tree Severer, Cord, Roadwarden, Dead Reckoning and generic weapons are
excluded. Paper Lantern and The River King's Measure are included because the
installed scene checks exposed additional retired sources with no scene object.

## Supported player invocation

Open Unity Mod Manager's Gunslinger development controls in the loaded campaign.
In **Missed relocated campaign weapon recovery**, use **Previous recovery weapon**
or **Next recovery weapon** to select one named weapon. Press **Inspect selected
weapon before recovery**. Read its known ownership locations, destination visit,
current/cached container observations and eligibility/refusal. If eligible,
acknowledge **I accept the unknown historical ownership and request one recovery
of this selected weapon**, then press **Recover selected weapon once**.

The action repeats inspection immediately before granting. Changing the selection
or reloading requires inspection again. It grants only the selected canonical
weapon, count one, to the party's shared inventory. Save normally afterward to
persist both the weapon and the main-character native UnitPart recovery record.
These specific recovery controls are the supported old-campaign action. The
other development fixtures retain their disposable-campaign restriction.
Repeating recovery or loading that saved record refuses another grant, including
when the recovered weapon has subsequently been removed. As with other campaign
changes, loading an earlier save rolls back both the grant and its record.

The existing development bridge also exposes
`InspectCampaignWeaponRecovery(key)` and
`RecoverCampaignWeapon(key, inspectedGameId, acknowledgeHistoricalUncertainty)`.
They accept only registry keys for eligible named campaign weapons and perform
the same inspection/refusal. The UI is the supported player invocation; these
bridge functions support guarded disposable fixtures, not arbitrary item GUIDs.

## Gates and limitations

The destination area must have been visited in this campaign and the owning
module must be enabled. This uses native campaign visit state as the progression
gate, rather than granting a late weapon to an early character. It records the
observed chapter and target identities. It refuses during combat, trading or a
save commit. Fixtures which prepare visit state do not prove organic progression.

Inspection searches native party inventory, equipment (including unused weapon
sets), loaded/cross-scene characters, resolvable remote/ex-companions and shared
stash. A canonical copy or a supported Craft Magic Items upgrade rooted in that
canonical GUID refuses recovery and reports its location. Refusal leaves
inventory unchanged. Missing character/storage references fail closed; the
inspection cannot claim coverage of storage that the native API cannot resolve.
Other integrations with different item identities are not assumed supported.

An available copy in an exact loaded target or cached persistent container also
refuses recovery. Unloaded historical container state is reported as unknown.
The action does not load an area, remove a valid weapon/upgrade, change native
treasure, refill a container or run on save load. Its ledger reserves before
the native inventory addition; even a failed transfer closes that weapon's
recovery record, preventing repeated attempts from becoming repeated grants.

**Absence from known inventories does not establish historical non-ownership.**
An older copy may have been sold, dropped or left in an unloaded area. The ledger
prevents repeated recovery grants; it does not establish that no other historical
copy exists. The acknowledgement makes this uncertainty explicit.

## Qualification

Recovery is developed and tested only on named disposable fixtures under the
Steam-based guarded runtime workflow. The actual campaign and
`KMG_AUTOMATION_BASELINE` remain protected. `KMG_AUTOMATION_WORKING` is a read-only
seed. Current measured canonical/upgraded ownership refusal, missing-progress,
single-grant, repeated invocation and fresh-process reload results are in
[the qualification report](WEAPON-FINDABILITY-QUALIFICATION.md).

Native disposable preparation run
`20261008T1344396554927Z-weapon-findability-owned-save` passed 412 assertions.
It exercised all 28 eligible weapons: missing progress and acknowledgement,
canonical copies in inventory/equipment/stash/companion storage and loaded
historical containers, one selected grant, repeated invocation, and preserved
unrelated contents. An actual Craft Magic Items Winter Reed +3 clone refused
recovery from inventory, equipment and stash without inventory changes.

Fresh-process verification
`20261008T1347232654274Z-weapon-findability-owned-save` passed after native disk
save, process exit and reload. All 28 ledger entries survived; owned-copy and
recorded-attempt refusals still held. These are explicit native fixtures on a
leveled disposable seed, not evidence of organic campaign progression.

A real generated eastern-cave inventory separately retained its original Cloak
of Resistance +1 through revisiting and reload. Native Kingmaker reconciliation
could add the new weapon when its old item-history dictionary was known; it did
not refill an extracted weapon or add one when that history was unknown. Recovery
does not promise that native reconciliation will repair any particular old save.
