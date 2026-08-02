# Sprint 103 native vendor lifecycle contract

Extend the existing read-only vendor observer to emit the exact installed
`VendorLogic` constructors, methods, fields, and properties. This metadata-only
checkpoint selects the native sale/repurchase boundary for a reversible
request-created firearm transaction without invoking a vendor or mutating a
save.

The observed reversible boundary is `BeginTrading` followed by
`AddForSell`/`RemoveFromSell` before `Deal` for an item owned by the player.
(`AddForBuy` is the opposite, player-purchase direction.) Resolve the native unit that
owns the exact capital vendor table. The read-only catalog proves that table is
shared by fifteen variants, so select exact native `Capital_Jhod`
(`c8d4913edee594749b706de35924617e`) and validate its unique association before
constructing that receiver as a detached request-local unit. Extend the
working-save transaction to stage and return
the exact request-created pistol. Resolve the exact reference from
`ItemsForSell` before passing it to `RemoveFromSell`. Require reference identity and origin
retention, then invoke `ReturnItems`, `EndTraiding`, and dispose the receiver in
`finally`. This qualifies native vendor staging without money, store, or save
mutation; durable post-Deal restart remains separately tracked.

After the reversible boundary is qualified, cross `Deal`: stage and sell the
same request-created pistol and require the exact 22 gp credit. Native `Deal`
reconstructs the sold stock instance, so require the exact pistol blueprint and
persisted battered-owner carrier on the unique reconstructed store item, stage
that item for player purchase, and commit the repurchase. Temporary request-local funding is allowed only when required and
must be reversed through the native money API. The final invariant requires the
same pistol, battered owner, store references, player inventory, money, class,
starting gold, and save-write sentinel to match the pre-request state exactly.
