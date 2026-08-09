# Paper Cartridges Bokken Acquisition Inventory

## Scope and decision

Bokken publication is deferred. No production mutation targets Bokken in this
mission because the exact installed blueprint graph does not establish one unique
Bokken shared-vendor table and lifecycle transaction boundary.

This defer applies only to Bokken. The exact capital `SmithVendorTable`, every
installed project-managed Beneath the Stolen Lands table, and renewable crafting
remain mandatory acquisition paths.

## Exact evidence

- Candidate commit: `66445edeb25c8278be7b971e73aef7a959008f0c`.
- Prerequisite guarded smoke:
  `20260809T0021341618639Z-mod-load-smoke` (PASS).
- Read-only installed graph run:
  `20260809T0023419883620Z-05148294115f464c8e6deb578ace0270`
  (`observe-vendor-table-contracts`, PASS).
- The observer enumerated every installed `BlueprintSharedVendorTable`, every
  `AddSharedVendor` component owner, every vendor-unit association, supplemental
  `AddVendorItems`, and direct blueprint references without mutation.
- No unit internal name, display name, component owner, table name, or direct
  reference record contained `Bokken`.
- The observed exact associations were limited to named installed owners such as
  Jhod variants, Honest Guy, Xelliren, Ignash, the Pitax trader, the pilgrim
  trader, Dumra, Svelid, and Kjerdi. None is safe evidence for Bokken.
- The rejected Jhod table remains exact GUID
  `afa2c7f292b8e1c4d9c835f0e8047dd3` and is not a Bokken fallback.

## Safety conclusion

No unique GUID/name/owner/lifecycle tuple is proven. A name heuristic, inferred
shared herbalist table, or mutation of an unrelated vendor would violate the
bounded-publication contract. The safely evidenced action is therefore to defer
Bokken alone and make no repeated attempt with the unchanged theory.
