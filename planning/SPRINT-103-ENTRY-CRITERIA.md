# Sprint 103 native vendor lifecycle contract

Extend the existing read-only vendor observer to emit the exact installed
`VendorLogic` constructors, methods, fields, and properties. This metadata-only
checkpoint selects the native sale/repurchase boundary for a reversible
request-created firearm transaction without invoking a vendor or mutating a
save.

The observed reversible boundary is `BeginTrading` followed by
`AddForBuy`/`RemoveFromBuy` before `Deal`. Resolve the unique native unit that
owns the exact capital vendor table, construct that receiver as a detached
request-local unit, and extend the working-save transaction to stage and return
the exact request-created pistol. Require reference identity and origin
retention, then invoke `ReturnItems`, `EndTraiding`, and dispose the receiver in
`finally`. This qualifies native vendor staging without money, store, or save
mutation; durable post-Deal restart remains separately tracked.
