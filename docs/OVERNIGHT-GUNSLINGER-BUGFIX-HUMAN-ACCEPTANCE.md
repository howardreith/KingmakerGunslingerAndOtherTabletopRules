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

## Issue 9 - firearm feat icon remaining human check

1. Open Weapon Focus, Greater Weapon Focus, Weapon Specialization, Greater Weapon Specialization, and Improved Critical in the real feat-selection UI.
2. Confirm Pistol `P`, Musket `M`, Blunderbuss `B`, Rifle `Ri`, and Revolver `Rv` are distinct, legible, and visually coherent beside native rows at both ordinary and compact UI scale.
3. Confirm each native top-level feat keeps its original icon and all non-firearm choices remain unchanged.
4. Open Rapid Reload; confirm its top-level muted salmon reload/ramrod symbol fits neighboring feats and its five children use the same exact monograms.
5. Reject the candidate aesthetically if lettering, border weight, contrast, saturation, or 32-pixel readability does not match the native visual language; automated sprite identity is not aesthetic acceptance.

Automated boundary already proven: guarded run `20260820T0659308177934Z-65bd0924b3dc455ebb04097f00172d90` passed 13/13 with the exact live native menu and Rapid Reload icon map. The before/after hashes and deterministic 64/32 map are in `docs/FIREARM-FEAT-ICON-MAP.md`.

## Issue 10 - Elven Branched Spear remaining human check

1. Equip an Elven Branched Spear on medium male and female units and inspect both world and inventory dolls. Confirm the model is approximately ordinary native spear length and the primary hand remains at the centered haft grip.
2. Observe idle, walk/run, ordinary thrust, full attack, movement attack of opportunity, weapon switch, and sheathe/unsheathe. Reject any backwards point, floor drag, torso penetration, detached hand, or return of the old 2.925 m silhouette.
3. Repeat on one Small body and with Enlarge Person and Reduce Person where safely available. Record any repeatable body-size residual rather than accepting broad race-specific offsets.
4. Confirm damage, reach, critical behavior, proficiency, feat interaction, named effects, and icon orientation remain unchanged.

Automated boundary already proven: guarded run `20260820T0733252707402Z-1a9897121438417f95edefbf51d348e5` passed 22/22. The packaged custom model measured 2.27855754 m on the native +Y axis versus 2.28250313 m for installed `TH_LongspearKnight1`, and every non-model native visual field remained equivalent. The disposable unit view did not synchronously materialize the equipped renderer, so this checklist is the required visual acceptance boundary.

## Issue 11 - Musket and Blunderbuss remaining human check

1. Equip the Musket first on medium male and female units. Inspect world and inventory dolls for the actual Musket mesh, muted wood/metal treatment, firing-hand grip at the trigger/stock, support hand on the fore-end, and no torso or upper-arm clipping.
2. Observe Musket idle, walk/run, ordinary firing, miss, reload, weapon switch, sheathe/unsheathe, and back state. Confirm the muzzle flash/projectile origin is the barrel opening and the back pose is independently aligned rather than reusing the held frame.
3. Repeat the same matrix for Blunderbuss and confirm its shorter flared-barrel silhouette and material remain visually distinct from the Musket.
4. Repeat both weapons on one Small body and with Enlarge Person and Reduce Person where safely available. Record only repeatable residual size defects; do not request broad race-specific offsets from one ambiguous frame.
5. Confirm Crossbow-derived delivery, firearm state, reload, audio, one projectile, damage, critical behavior, feats, and save identities remain unchanged. Reject any crossbow placeholder, inherited bolt/release visual/audio, detached model, support-hand miss, floor drag, or back-state clipping.

Automated boundary already proven: guarded run `20260820T0819129064284Z-4b9313e5c2784b099756b97fc139b68e` passed 63/63. Packaged held frames are scale one at exact `1.33999968 m` Musket and `0.8599998 m` Blunderbuss semantic lengths, both native support IK targets are assigned, both back prefabs are distinct with exact `BackMount`, both effective items retain one projectile, and inherited sheath presentation is absent. These structural facts do not establish visual acceptance on animated bodies.

## Issue 12 - Project magic-item distribution

Use a new campaign or disposable save whose relevant containers have not materialized. First use the read-only Development Tools action `Print all project magic-item location audits`; it gives every exact GUID/current count without granting or moving items.

1. Stag Lord Fort: Paper Lantern, Wayfarer's Oath, and Border Sentinel in three distinct chests.
2. Capital: Quiet Current in the tavern; Winter Reed and Cord in two distinct Capital Square Village chests.
3. Narlmarches/Lonely Barrow: Boughkeeper, Thornstep, Cloud-Cleaver, and Moonlit Fork in four distinct sources.
4. Act III: Falling Petal at Goblin King Fort and Drawn Horizon at Silverstep Grotto.
5. Vordakai's Tomb: four named Eastern weapons in four Level 1 caches; Duelist's Rebuttal and Viper's Reach in two Level 2 caches.
6. Pitax: Empty Sleeve, Moonlit Crossing, Unfixed Form, River King's Measure, Irovetti's Ovation, and Briar-Crowned Spear in six distinct sources.
7. Late game: Night Without Moon at Castle of Knives; Heaven's Measure, The Last Word, and Watch at World's End in three distinct House at the Edge of Time sources; World-Tree Severer and First Branch in distinct Final Dungeon sources.

Confirm one named project item per target, none of the 30 in ordinary recurring merchants, and preserved native contents. Do not use an already opened container as evidence against static blueprint publication.

## Install the final candidate

Use the manifest-backed exact-reference package:

```powershell
.\scripts\Deploy-Local.ps1 `
  -PackagePath .\artifacts\local-runtime\0.0.88\KingmakerGunslinger-0.0.88-local-runtime.zip `
  -Confirm:$true
```

The deploy script validates the strict package and build-local manifest, backs up the current live mod without deleting it, replaces only the `KingmakerGunslinger` mod directory, and writes a hash-bound deployment manifest. Launch runtime qualification only through `Invoke-KingmakerRuntimeTest.ps1`/Steam App ID 640820. Do not launch `Kingmaker.exe` directly.
## Final candidate identity

- Install package: `artifacts/local-runtime/0.0.88/KingmakerGunslinger-0.0.88-local-runtime.zip`
- Package SHA-256: `FAFBAE86F4D890A958435C2D3D87ED6BFABC5504988E709B0960A90BF161F8CA`
- DLL SHA-256: `E54E35145EABD51461E9277C1B1CCD8CF7EEA29BA48CFB156D40ADDC9FA4E1EB`
- Runtime artifact smoke: `20260820T0930236838361Z-f2cdf3b1f77d499f9a1c1ba419556627`, PASS
