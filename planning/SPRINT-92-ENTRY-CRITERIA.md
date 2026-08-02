# Sprint 92 battered firearm fixed scrap value

Installed Kingmaker 2.1.7b exposes the exact player-to-vendor instance-price
boundary as `VendorLogic.GetItemBuyPrice(ItemEntity)`. The battered overlay
patches only that one-argument instance method and returns 22 gp only when the
exact item GUID exists in the persisted battered ownership carrier.

The production firearm blueprint cost, vendor-to-player price, ordinary item
prices, and unbound firearms remain native. Missing item identity or ownership
records preserve the native result. The fixed value is the already-authorized
expected value of 4d10, not a new balance decision.

Qualification requires exact patch-target reflection, focused source/domain
contracts, repository validation, all domain/reflection tests, clean Release
build, strict package validation, and post-commit guarded mod load.
