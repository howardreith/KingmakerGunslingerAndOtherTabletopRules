# Elven Branched Spear implementation report

## Outcome

The complete spear feature remains intact, and the required first-playtest
repairs are implemented on `codex/elven-branched-spear` in draft PR #3. The
release candidate is now uniquely identified as
`0.0.79-elven-branched-spear`.

## Required UI repair

The stable category is still `0x004b4d47`, but Kingmaker's central category
display resolver now returns **Elven Branched Spear** for that value. EWP uses
the native proficiency icon; Rogue Finesse Training uses the spear icon and
exact parenthesized name; parameterized weapon feats use native `EB` glyph
metadata. No mechanic or save identity changed.

The final follow-up also gives the EWP child the exact title **Proficiency
(Elven Branched Spear)** and anchors it in the merged selector immediately
after Elven Curve Blade. Kingmaker's reversed list rendering places it directly
above Elven Curve Blade. Call of the Wild legitimately supplies the anchor only
to the merged catalog, so the serialized catalog remains an idempotent append.

## BTSL distribution repair

The six generic tiers are appended once to two standalone and two campaign
Beneath the Stolen Lands weapon tables. Publication remains module-gated,
additive, reversible, and tolerant of an uninstalled DLC. Named campaign items
are not copied into ordinary roguelike stock.

## Optional visual differentiation

The human accepted the model and tested fit. All variants share one weapon type
and serialized prefab; item-level variation would require a risky type split or
runtime material mutation. This optional polish is therefore deferred while the
accepted mesh, grip, scale, animation, fallback, and clipping result remain
unchanged.

## Evidence

Focused presentation/combat run
`20260814T0444110998835Z-disposable-elven-branched-spear-combat` passed 18/18.
BTSL run `20260814T0454174378820Z-observe-vendor-table-contracts` passed with
four exact tables and 24 singular rows. Focused module ON/OFF runs passed.
Final clean source, package, compatibility, smoke, hashes, MVID, and installed
identity are sealed in `docs/ELVEN-BRANCHED-SPEAR-QUALIFICATION.md`.

The final artifact source is `9a24147b717b2502442d48d5f2026becdaba4e8d`.
Repository validation, 1,033/1,033 tests, clean Release, strict 125-file package,
final Call of the Wild 18/18, canonical working-save smoke, and all 32
feature-module masks passed. The package is
`KingmakerGunslinger-0.0.79-elven-branched-spear.zip`, SHA-256
`ECD656AF16C507192D2638F374743B46BE606B349EE365C15B07E0EBFC2FB3CE`.

The final EWP-specific Call of the Wild rerun
`20260814T1009047959759Z-disposable-elven-branched-spear-combat` passed 18/18.
It observed the exact title, readable prerequisite, one static option, merged
indexes `Elven Curve Blade=5` and `Elven Branched Spear=6`, native EWP icon,
and all prior finesse/combat regressions. The built, packaged, installed, and
runtime-loaded DLL SHA-256 is
`70279B4C764E0E9FCCDF0CB56B886B5250421772EBFA6C46C79B90E41FC46FC4`.
