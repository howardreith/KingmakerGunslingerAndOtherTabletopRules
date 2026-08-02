# Sprint 83 Gunsmith and battered-firearm contract audit

## Authority

The local base-class rule grants a level-one choice of pistol, musket, or
blunderbuss. The chosen item is battered: only its originating Gunslinger uses
it normally; another creature treats it as Broken, or unusable when its actual
condition is already Broken. It can be sold only as scrap for 4d10 gp. The
class also gains Gunsmithing.

The local Gunsmithing feat permits firearm/ammunition crafting with a kit,
restores one Broken firearm during an hour of work in a rest period, and lets a
Gunslinger upgrade the initial battered firearm for 300 gp and one day.

## Existing Kingmaker implementation

- Class starting items grant one production Early Pistol, one powder charge,
  and one lead ball through native `LevelUpHelper.AddStartingItems`.
- Firearm Proficiency currently grants generic Reload, Repair, and Overhaul.
- Repair and Overhaul consume vendor-available Firearm Repair Kits through
  exact-item atomic transactions.
- Item-owned condition tokens persist firearm state, but no accepted contract
  binds an item to its originating unit or changes one item's sale price.
- Kingmaker has no project-owned firearm/ammunition crafting UI or one-hour
  rest-work selection.

## Unresolved player-facing decision

At least three materially different adaptations are implementable:

1. Persist the exact originating unit identity on the battered item, apply
   Normal/Broken/Wrecked effective condition by viewer/wielder, use a fixed
   expected 4d10 scrap value of 22 gp, and make Gunsmithing a real feat that
   gates Repair/Overhaul while the class gains it automatically.
2. Use a distinct class-bound Battered Pistol: any Gunslinger uses it normally,
   non-Gunslingers cannot equip it, sale value is fixed at 22 gp, and the
   Gunsmithing feat gates Repair/Overhaul.
3. Keep the normal starting pistol and adapt only Gunsmithing to the existing
   kit-based Repair/Overhaul actions, explicitly omitting origin ownership and
   scrap-only sale.

These alternatives change companion transfer, equipment eligibility, economy,
save schema, and feat value. No local ADR or roadmap decision selects one.
Autonomous choice would therefore be a prohibited balance/compatibility
decision. Option 1 best preserves the authoritative rule and is recommended,
despite requiring a new persistent ownership schema and lifecycle coverage.

## Preserved boundaries

Do not invoke the broad first-level creation `Commit`; its global entity,
companion, view, and rest mutations still lack complete rollback proof. Do not
modify saves or infer an originating owner from current wielder. Existing
starting-item and maintenance evidence remains valid while this decision is
pending.
