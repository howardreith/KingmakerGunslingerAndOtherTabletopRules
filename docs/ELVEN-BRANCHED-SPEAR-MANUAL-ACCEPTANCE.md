# Elven Branched Spear manual acceptance

## Accepted first-playtest contracts

The first human playtest confirmed all of the following on the published
feature branch:

- Elven Branched Spear appears in Exotic Weapon Proficiency, Rogue Finesse
  Training, Weapon Focus, and Improved Critical;
- Weapon Finesse selects Dexterity for attack;
- Rogue Finesse Training provides the intended Dexterity-to-damage behavior;
- Piranha Strike functions with the weapon;
- reach and ordinary combat attacks function;
- the custom 3D model looks good on the tested character;
- no material clipping was observed; and
- hand and body alignment were acceptable.

The repair preserves these behaviors and the stable category value
`0x004b4d47` (decimal `4934983`).

## Findings that required continuation

The same playtest found crossed-firearm art on spear selector entries, native
parameterized feat tiles replaced by crossed-weapon art, an inconsistent bare
Rogue option name, raw `4934983` in an Exotic Weapon Proficiency prerequisite,
missing Beneath the Stolen Lands merchant availability, and limited named-item
visual differentiation.

## Repair evidence

Guarded run
`runtime-evidence/20260814T0444110998835Z-disposable-elven-branched-spear-combat`
passed 18/18 structured assertions. It observed the prerequisite text **Doesn't
have the following proficiencies: Elven Branched Spear**, native shared Exotic
Weapon Proficiency icon `ExoticWeaponProficiency`, exact Rogue name **Finesse
Training (Elven Branched Spear)** with spear icon
`KMG_Icon_elven-branched-spear`, and seven parameterized selector rows with the
native-glyph contract and acronym `EB`. No raw decimal or hexadecimal category
value and no firearm icon survived these surfaces.

That run also re-proved the existing Dexterity paths and real combat contracts,
including the exact movement-AoO delta. Guarded vendor run
`20260814T0454174378820Z-observe-vendor-table-contracts` proved the four actual
BTSL tables and 24 singular generic spear rows without losing native or firearm
stock.

## Remaining human review

Structured runtime evidence is authoritative for mechanics. A human should
still visually recheck the repaired list row, selected header, progression row,
character sheet, and respec presentation in the final package. Named equipped
materials remain intentionally shared to protect the accepted rig; broader
body-type, armor, size-changing, and animation visual review remains optional.

The final 0.0.79 Call of the Wild presentation/combat rerun passed 18/18 at
`20260814T0507290190244Z-disposable-elven-branched-spear-combat`. Automated
evidence therefore confirms metadata and mechanics in the final candidate;
subjective appearance of the repaired tiles and headers remains the stated
human recheck.

## Final Exotic Weapon Proficiency repair

The subsequent human report was verified against the live Unity Mod Manager
files: the prior candidate really was installed, so the bare title and top-row
placement were not stale deployment. The final follow-up now names the child
exactly **Weapon Proficiency (Elven Branched Spear)**, removes it from the
prioritized top-block array, and anchors the merged native selection immediately
after Elven Curve Blade, which renders immediately above it.

Guarded Call of the Wild run
`20260814T1025471192636Z-disposable-elven-branched-spear-combat` passed 18/18
on commit `9e710754e50c09e95c7790d70af8a334757b940e`. The structured observation was
`ewpName=Weapon Proficiency (Elven Branched Spear)`, readable
category/prerequisite, native `ExoticWeaponProficiency` icon, no spear in
`Features`, and merged indexes `5/6`. The exact
runtime-qualified DLL is installed; the remaining step is the requested human
visual confirmation in the level-up list.

## 2026-08-20 visual repair supersession

The overnight playtest finding supersedes the earlier acceptance statement for
length and hand alignment. Live run
`20260820T0712595741030Z-828d00615d54498297ec90f5dfaa4352`
measured native `TH_LongspearKnight1` at 2.2825 m along local +Y and the rejected
custom model at 2.9235 m along local +Z. The repair reauthors the custom length
to 2.28 m and maps its prefab `Visual` onto the native +Y frame while retaining
all non-model native `WeaponVisualParameters` fields.

Final human acceptance must separately inspect world and inventory dolls for
idle, walk/run, ordinary thrust, full attack, movement AoO, weapon switch,
sheathe/unsheathe, medium male/female, one small body, Enlarge Person, and
Reduce Person. Confirm the primary hand remains on the centered haft and reject
any backwards point, floor drag, torso penetration, or old 2.925 m silhouette.
