# Sprint 103 native vendor lifecycle contract

Extend the existing read-only vendor observer to emit the exact installed
`VendorLogic` constructors, methods, fields, and properties. This metadata-only
checkpoint selects the native sale/repurchase boundary for a reversible
request-created firearm transaction without invoking a vendor or mutating a
save.

The observed reversible boundary is `AddForBuy`/`RemoveFromBuy` before `Deal`.
Extend the working-save transaction to stage and return the exact request-created
pistol, require reference identity and origin retention, and invoke `ReturnItems`
in `finally`. This qualifies native vendor staging without money, store, or save
mutation; durable post-Deal restart remains separately tracked.
