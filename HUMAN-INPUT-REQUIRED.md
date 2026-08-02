# Human input required: Gunsmith and battered starting firearm

## Exact blocker

The authoritative rule requires an origin-bound battered firearm and a
Gunsmithing feat, but the project has no accepted Kingmaker mapping for
origin ownership, nonowner condition, scrap value, crafting/rest restoration,
or maintenance gating.

## Evidence

`planning/SPRINT-83-GUNSMITH-BATTERED-CONTRACT-AUDIT.md` records the exact local
rules, current item-owned condition and maintenance architecture, and three
implementable outcomes. The coverage and fidelity matrices still require this
mandatory level-one feature.

## Why autonomous resolution is prohibited

The alternatives materially change companion transfer, equipment eligibility,
economy, save compatibility, and feat value. No authoritative local decision
selects one, so choosing would be an autonomous balance/compatibility decision.

## Smallest precise question

Which battered-firearm/Gunsmithing mapping is authorized?

1. **Persistent originating owner (recommended):** exact owner-bound effective
   condition, fixed 22 gp scrap value, and a real automatically granted
   Gunsmithing feat gating Repair/Overhaul.
2. **Class-bound adaptation:** any Gunslinger uses a distinct Battered Pistol,
   non-Gunslingers cannot equip it, fixed 22 gp value, Gunsmithing gates
   Repair/Overhaul.
3. **Maintenance-only adaptation:** keep the normal starting pistol,
   Gunsmithing grants/gates existing maintenance, and explicitly omit
   ownership and scrap behavior.

## Exact continuation command

Resume the autonomous Gunslinger mission and implement the authorized Sprint
83 option; do not invoke broad first-level creation Commit.
