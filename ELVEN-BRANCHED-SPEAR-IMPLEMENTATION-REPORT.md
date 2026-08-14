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
