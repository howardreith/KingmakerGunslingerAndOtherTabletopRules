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

Exact source commit `123f623` passed guarded Steam mod load as
`20260802T1418228589307Z-mod-load-smoke`. The rebuilt package/DLL hashes were
`be26f742d89771cf8e6cafe8a6443ddda664b04ed527c70c0bfa18a6dd2cb747` and
`db60a26b73cb09970daad42bf4c8bd35a61cfb0dd8c7f76b52a50d43aa31000f`.
Feature-specific runtime value/isolation evidence remains required.

The existing guarded `gunslinger-starting-items` scenario is extended rather
than creating a duplicate save path. After the exact native grant and origin
binding, it invokes the patched native vendor method on the bound pistol and a
fresh unbound production-pistol control, requires `22` only for the bound item,
then performs its existing exact inventory/class/gold rollback without saving.
