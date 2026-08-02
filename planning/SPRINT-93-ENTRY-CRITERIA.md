# Sprint 93 item-owned battered origin repair

The first authorized `gunslinger-starting-items` run failed deterministically
because installed Kingmaker 2.1.7b exposes no stable `ItemEntityWeapon.UniqueId`.
The earlier GUID/UnitPart ownership design contradicted the already-qualified
item-owned firearm-state architecture and could never bind a native item.

The repair uses one mechanically inert item enchantment. Its serialized
`ItemEnchantment.ParentContext` carries the exact originating unit reference;
the item therefore owns both the battered marker and its origin across
inventory transfer and native serialization. Runtime resolution requires
exactly one marker and an exact non-null `MaybeCaster`; duplicates or missing
origin context fail closed. No item identity is invented.

Starting-item binding, effective-condition resolution, and fixed sale value now
read this common carrier. The prior UnitPart/ledger types remain only as
historical domain evidence and are no longer production authorities.

Source qualification requires the focused carrier contract, repository
validation, all 848 domain/reflection tests, clean Release build, strict
package validation, and exact staged safety audits. Runtime requires mod load
followed by the authorized guarded scenario; any second materially different
failure changes to narrower evidence rather than a speculative third repair.

## Qualification result

Exact commit `6b1e413ba5153d30163f9c11923129e4c7b515c7` passed mod load as
`20260802T1440278862789Z-mod-load-smoke`. Two independent fresh-process
`gunslinger-starting-items` runs passed as
`20260802T1441506873456Z-gunslinger-starting-items` and
`20260802T1445126858809Z-gunslinger-starting-items`. Each proved the native
1/1/1 firearm/ammunition grant, exact owner binding, 22 gp bound value versus
1000 gp for an ordinary pistol, exact in-memory rollback, no save-writing API,
and loaded version `0.0.60`.
