# Sprint 40 entry criteria: Utility Shot

Sprint 40 is a checkpoint in the autonomous completion mission, not a stopping
condition.

## Authoritative branches

- **Blast Lock** is `OMITTED-NO-MEANINGFUL-INTERACTION`. Installed Kingmaker
  represents locks through map interaction and Disable Device contracts, not as
  firearm-attack targets with lock AC, quality, jammed state, or destructible
  unlocking semantics. A combat ability would invent a parallel lock system.
- **Scoot Unattended Object** is `OMITTED-NO-MEANINGFUL-INTERACTION`.
  Kingmaker exposes no general Tiny-or-smaller unattended-object combat target
  whose position can be moved 15 feet while suppressing damage.
- **Stop Bleeding** remains `ADAPTED` and is the Sprint 40 implementation slice.

## Stop Bleeding acceptance contract

- A standard-action extraordinary ability is granted at Gunslinger level 3.
- The caster must have at least one current grit point; the deed does not spend
  grit.
- Exactly one equipped marked firearm is required. Wrecked and empty firearms
  reject atomically; Normal and Broken loaded firearms are allowed.
- The target is the caster or an adjacent creature within 5 feet and must have
  at least one active buff carrying native `SpellDescriptor.Bleed`.
- Delivery consumes exactly one loaded chamber through the immutable firearm
  discharge transition, deals no damage, and performs no attack roll. This is
  the tabletop shoot-into-the-air branch, which explicitly requires no attack
  roll but consumes ammunition normally; without a natural attack roll there is
  no misfire roll to interpret.
- Exactly one bleed fact is removed deterministically. Other buffs and other
  bleed facts remain.
- Any fault after the firearm transition restores the exact prior firearm state
  before surfacing the failure. Rejections change neither firearm nor buffs.
- Runtime qualification must prove self/adjacent delivery, one-round
  consumption, single-bleed removal, positive-grit/no-spend behavior,
  empty/zero-grit rejection, unrelated-buff preservation, and cleanup without a
  save load or save write.
