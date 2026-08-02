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
