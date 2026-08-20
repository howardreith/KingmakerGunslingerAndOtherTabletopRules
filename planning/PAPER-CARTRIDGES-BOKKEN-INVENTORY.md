# Paper Cartridges Bokken Acquisition Inventory

## Current authority - Issue 6 qualified on 2026-08-20

The historical defer below is preserved as evidence of the older observer's
boundary, but it is superseded. The old unit loop skipped `AddVendorItems`
whenever a unit had no `AddSharedVendor`. A materially distinct bounded
localized/dialog/unit traversal resolved exact installed authority:

- `BlueprintUnitLoot` `C11_BokkenVendorTable`
  (`4778ecb5df5d48742b9be5a204ed4657`).
- `OTP_Bokken` (`4f5acdb403f6ef642959f6bedc051ac7`) and
  `OTP_Bokken_ZeroState` (`57f84fdde3cc2994284fb3acc4a3cb97`), each with one
  direct `AddVendorItems` reference. Base `Bokken` is a prototype, not a third
  stock owner.
- Forensic run `20260820T0548397631886Z-31199f78288f43e2b7655b0433abba7b`
  on `3b29451f24cc163f48f03150cce0e7563165beaa`.
- Qualified publication run
  `20260820T0555507894600Z-0772504de3254a64986e6ea2da172a02` on
  `73e776a91167bc02024e7be794a822fa63fec48e`: one Black Powder row at 100,
  one Lead Ball row at 100, and the existing Paper Cartridge identity once at
  100, with all 21 native stock rows retained.

Static not-yet-materialized/new-campaign publication is qualified. Refresh of
already-materialized save-owned merchant inventory is not claimed.

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
