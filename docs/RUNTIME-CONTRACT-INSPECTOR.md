# Runtime-contract inspector loader safety

The deterministic runtime-contract inspector uses reflection-only loading
against the explicitly supplied Kingmaker installation. Required Kingmaker,
Unity Mod Manager, Harmony, persistence, combat, and item contracts remain
blocking gates.

## 2026-07-28 loader repair

`Assembly-CSharp.dll` contains a transitive
`System.Collections.Generic.HashSet<T>` type. The UnitPart extension-method
search previously enumerated every loadable type and called `GetParameters()`
on every generic static method before checking its name. Windows PowerShell
therefore attempted to decode the unrelated explicit
`ICollection<T>.Add(!0)` signature and raised a security-transparency
`TypeLoadException`.

The failing `HashSet<T>` method is not a declared runtime contract. The repair
first filters methods using stable member names (`Get` and `Ensure`) and
deterministic metadata ordering. Parameter metadata is loaded only for those
explicit UnitPart contract candidates. Failure to read a candidate's
parameters remains fatal and reports the contract, assembly, declaring type,
member, and original exception.

The JSON evidence records the inspection policy, the count of unrelated static
methods excluded before parameter inspection, and the ordered list of
tolerated loader failures. The current repair tolerates no thrown loader
failure: the unrelated `HashSet<T>.Add` member is excluded before the unsafe
reflection call. All pre-existing required contract gates remain unchanged.

Required field/property lookup likewise walks the requested type hierarchy one
declaring type at a time. This avoids `Type.GetProperty` ambiguity when a game
type hides an inherited property, while still failing if the same declaring
type exposes more than one property with the required name.

## Disputed gate reconciliation

The reconstructed inspector retained historical assumptions for
`UnitDescriptor.GetFeature(BlueprintFeature)` and a direct
`UnitDescriptor.AddFact`/`AddFeature` grant. Neither is the current production
call path. The reachable equipment restriction uses
`UnitDescriptor.Progression.Features.GetRank(BlueprintFeature)`. The reachable
development command grants through
`UnitDescriptor.Progression.Features.AddFeature(BlueprintFeature,
MechanicsContext)` and verifies with `GetRank`.

The inspector now gates those exact typed operations, including return and
parameter types. The private build bundle and installed Steam runtime contain
byte-identical copies of all ten compiler references. `Build-Local.ps1` now
fails before compilation if any private reference differs from its installed
counterpart, preventing a stale bundle from silently qualifying.

The deterministic candidate workflow now requires the current Sprint 30 total
of 611 tests rather than the inherited Sprint 29 total of 599.
Its two byte-comparison builds now use `Build-Local.ps1`, the same explicit
Roslyn/private-reference path that creates the runtime package, after proving
that all ten private references are byte-identical to the installed Steam
runtime. This removes the stale MSBuild resolver path that attempted to resolve
UMM's newer Harmony dependency while targeting .NET Framework 4.7.

The same provenance pass corrected the retained token-carrier type name from
the obsolete `Kingmaker.Items.ItemEnchantment` assumption to the production
type used by `KingmakerFirearmStateTokenStore`:
`Kingmaker.Blueprints.Items.Ecnchantments.ItemEnchantment`. Add/remove
signatures remain strict gates.
