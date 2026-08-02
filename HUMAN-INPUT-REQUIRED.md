# Human input required: Scatter Shot runtime fixture

The guarded Scatter Shot runtime gate has reached the documented two-attempt
limit. Both failures occurred before firearm mutation or save interaction:

1. `20260802T1558563329150Z` proved the native cone predicate is a private
   instance method; commit `ac5c520` repairs that exact contract.
2. `20260802T1604090773661Z` then proved detached `ChargenUnit` fixtures are not
   candidates in the live-area `GameHelper.GetTargetsAround` query.

Recommended authorization: permit one additional save-free guarded attempt
after implementing an exact, reversible registration of the disposable target
units in the live area-state collection, with reference snapshots and cleanup
verified before completion. No save load, save write, input automation, or
production unlock would occur before that attempt passes.

Alternative: authorize a supervised read-only scene fixture in which a human
positions ordinary disposable enemies; this is slower and less deterministic.

Until renewed authority is received, do not run
`disposable-gunslinger-scatter-shot`, remove the Blunderbuss restriction, or
publish it to a vendor.
