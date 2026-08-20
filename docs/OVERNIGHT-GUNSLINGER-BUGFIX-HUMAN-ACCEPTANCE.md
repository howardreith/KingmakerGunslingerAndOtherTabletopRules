# Overnight Gunslinger Bug-Fix Human Acceptance

## Issue 7 - Border Sentinel

1. Use a new campaign or an unopened/not-yet-instantiated Stag Lord Fort state; do not use a previously opened chest as refresh evidence.
2. At Oleg, confirm Border Sentinel is absent while mundane/masterwork eastern weapons and the Issue 5 maintenance kits remain intact.
3. In Stag Lord Fort, open the separate native treasure chest whose original fixed contents are Rusty Horseshoe x1 and Gold x12; confirm Border Sentinel appears exactly once and the native contents remain.
4. In the development panel under Eastern Weapons Acceptance, run `Print Border Sentinel location audit`. In Stag Lord Fort it must report item GUID `c1c7a6746916504ebfdcb2b650a7145b`, target GUID `c8b8159fb695be64883b609a7e77e75d`, `countOneMatches=1`, and `currentAreaMatch=True`.
5. Do not interpret an already-owned/sold/dropped/stashed copy, an already-materialized Oleg inventory, or an already-opened chest as a static-publication failure.

Status: DO NOT RUN YET

Human verification is requested only after all safely actionable issues and
final automated qualification are complete. The final immutable candidate and
exact installation steps will be recorded here.

The consolidated sequence must cover:

1. Acadamae OFF/ON, displayed/executed Standard action, save success/failure,
   fatigue persistence/rest removal, and Cord behavior.
2. Focused Aim damage with exactly one visible Grit spend, zero-Grit, and True
   Grit.
3. Pistol/Musket/direct Blunderbuss inside/outside penetration range with
   Touch/Normal feedback.
4. Acadamae selection with one prerequisite presentation.
5. Oleg with Repair Kits x5 and Overhaul Kits x2.
6. Bokken with Black Powder x100, Lead Balls/Bullets x100, and Paper Cartridges
   x100.
7. Border Sentinel absent from Oleg and present at its later fixed target.
8. Audible Pistol, Musket, Blunderbuss, Revolver, and Rifle; no event on empty
   rejection and no inherited crossbow release/bolt.
9. Distinct firearm monograms and native-style Rapid Reload at real UI scale.
10. Elven Branched Spear length/grip through world/inventory idle, move, attack.
11. Musket/Blunderbuss texture, grip, support hand, clipping, fire, reload,
    back state, and muzzle origin.
12. Every redistributed project unique at its distinct fixed campaign source.

## Issue 1 final human matrix (deferred until consolidated handoff)

Automated boundary: published commit
`f807eb1cc3dabf9dc66acaa2b773c029a72dc942` passed guarded run
`20260820T0428503321600Z-d97a49371e1949c89f3de25aac1c6eff` 14/14.
The checks below are limited to loaded-area animation, transition persistence,
and player-visible presentation not established by the disposable fixture.

Use a feat owner with one qualifying prepared arcane Summoning spell and no
Cord of Stubborn Resolve, then repeat the specified Cord control. Confirm OFF
shows and executes native full-round/one-round casting with no Acadamae log or
fatigue. Confirm ON shows and executes Standard, emits one log entry containing
d20, Fortitude modifier/total, DC, result, and fatigue disposition, and never
grants Swift/Move/Free. Observe one success with no fatigue and one failure with
canonical Fatigued surviving summon expiry and an area transition, then native
rest removal. Confirm cancel/interruption and ineligible/item/spontaneous casts
emit no Acadamae save. With the Cord equipped, confirm the save still appears
and only the accepted Cord consequence substitution changes.

## Issue 2 - Focused Aim remaining human check

1. On a Mysterious Stranger with at least two visible Grit, activate Focused Aim normally and confirm the same visible Grit counter decreases by exactly one while the firearm damage bonus is active.
2. Remove the marker or let its native duration expire, activate it again, and confirm exactly one additional point is spent.
3. At zero Grit, confirm the activation is unavailable or safely rejected and no damage marker appears.
4. Select Focused Aim through True Grit, retain one positive Grit, activate it, and confirm the established zero-effective-cost behavior without allowing activation at zero.
5. Save and reload a disposable test state and repeat one ordinary activation to confirm the same authoritative counter and feature facts reconcile.

Automated boundary already proven: guarded run `20260820T0458550047640Z-b20727d24cc24d3297e0e9d23d385235` passed live resource, damage, repeat, zero-Grit, True Grit, owner lifecycle, and cleanup assertions.

## Issue 3 - Firearm penetration remaining human check

1. Inspect Pistol, Musket, Blunderbuss, Advanced Rifle, and Advanced Revolver descriptions and Qualities. Confirm the base penetration distances read 20, 40, 10, 400, and 100 feet respectively, and that Blunderbuss limits the rule to ordinary direct fire.
2. Fire a Pistol, Musket, and ordinary direct-fire Blunderbuss just inside and just outside one effective range increment. Confirm the battle log says `Touch AC` inside and `Normal AC` outside, with actual distance and effective range but no enemy AC number.
3. Repeat with Advanced Rifle and Advanced Revolver just inside and outside five effective range increments.
4. Use Steady Aim or another legal effective-range modifier and confirm the reported penetration range changes for that attack.
5. Confirm Scatter Shot retains cone behavior and does not gain the direct-fire Touch AC shortcut. Confirm concealment, Mirror Image, cover, line of sight/effect, natural 1, critical confirmation, and normal damage processing remain native.

Automated boundary already proven: guarded run `20260820T0513443721972Z-cceff2c263254181ad15fd7af638ed3f` passed 15 production boundary events, the effective-range case, one exact feedback line per event, and cleanup. Pre-attack hover was not implemented because no qualified narrow seam was found.

## Issue 4 - Acadamae prerequisite text remaining human check

1. Open feat selection on an eligible specialist Wizard and inspect Acadamae Graduate at normal UI scale.
2. Confirm Kingmaker shows one native prerequisite presentation for Wizard level 1 and Conjuration not being forbidden.
3. Confirm the rules description begins with the activation/effect text and does not repeat a separate `Prerequisite:` paragraph.
4. Repeat on an ineligible Wizard configuration and confirm the native prerequisite failure presentation remains truthful.

Automated boundary already proven: guarded run `20260820T0524095518410Z-ea7adae339fc4fa4a98bfe7bd52b4222` passed the live registered-blueprint presentation assertion and all existing Acadamae action/save/fatigue assertions.

## Issue 5 - Oleg maintenance-stock remaining human check

1. In a disposable new campaign, or before Oleg's shop inventory has materialized, open Oleg's normal trading UI.
2. Confirm one Firearm Repair Kit stock entry with quantity 5 and one Overhaul Kit stock entry with quantity 2.
3. Confirm Oleg's native and foreign-mod stock remains present and ordered normally, with no added firearm, named magic firearm, or unrelated capital-only stock attributable to this issue.
4. Treat an already-materialized old-save shop as informational only: the repair publishes the static shared table and does not claim or force retroactive refresh of save-owned inventory.

Automated boundary already proven: guarded run `20260820T0535269245782Z-7edf2d2c158a4085893931e91b14db1d` passed 20/20, including exact live table rows/counts and exact two-owner identity.

## Issue 6 - Bokken ammunition-stock remaining human check

1. In a disposable new campaign, or before Bokken's shop inventory has materialized, open Bokken's normal trading UI at Oleg's Trading Post.
2. Confirm one Black Powder Charges entry at quantity 100, one Lead Balls entry at quantity 100, and one Paper Cartridges entry at quantity 100.
3. Confirm Bokken's normal potion, scroll, wand, alchemical, and material stock remains present and no Jhod/capital/BTSL-only stock was copied into the shop.
4. Treat an already-materialized old-save shop as informational only: the repair publishes exact static `BlueprintUnitLoot` and does not claim or force retroactive refresh of save-owned inventory.

Automated boundary already proven: guarded run `20260820T0555507894600Z-0772504de3254a64986e6ea2da172a02` passed 23/23, including all three exact `1/100` rows, both exact owners, and retained native stock.

## Issue 8 - firearm audio remaining human check

Use the final immutable candidate and a fresh Steam launch after final package deployment. A nonzero playing ID is not an audible pass.

1. Fire one ordinary committed shot each from Pistol, Musket, Blunderbuss, Revolver, and Rifle; confirm one audible, correctly mapped firearm report.
2. Confirm an ordinary miss after a committed shot still produces one report.
3. Confirm empty, Wrecked, rejected/canceled, and true-misfire paths produce no normal firearm report.
4. Confirm Scatter produces one Blunderbuss report per volley, not per target.
5. Exercise Dead Shot, Startling Shot, Menacing Shot, and Stop Bleeding where available; confirm one normal report per committed physical discharge.
6. Listen specifically for inherited crossbow release/twang/bolt layers and record any weapon, action, and timing where one remains.

Automated boundary already proven: run `20260820T0635323959656Z-88cfa04a0deb4595bfbc2ee8d4284e31` passed 6/6 with the exact bank loaded once, all five Event families accepted, live-unit and ordinary committed Blunderbuss posts accepted, and forced misfire suppressed. That evidence deliberately makes no audibility claim.
