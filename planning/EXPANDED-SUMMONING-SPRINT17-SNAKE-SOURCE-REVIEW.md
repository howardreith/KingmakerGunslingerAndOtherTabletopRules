# Sprint 17 hidden snake source checkpoint

Status: **NOT QUALIFIED**. Source work only; no gameplay acceptance or publication.
Latest disposition: exact7ab77c32 prelaunch gate rejected on stale107-icon
packaging guards, after2048 tests (84.9s) and exact Release passed. Both guards
now require109; direct packaging and strict320-member validation PASS.
The corrected committed complete gate remains next; no runtime was launched.
Branch `codex/expanded-summoning-phase2b-sprints14-17`, draft PR26; parent
`aa5289cb980f064f682aee7c98e0da3f725c063f`. DATA contributes no source.

## Implemented and withheld

The frozen creature contract controls: Viper is Medium, not a Tiny/Small
replacement. Viper has2 animal HD,13 HP, Str8/Dex13/Con14/Int1/Wis13/Cha2,
20-foot land speed, +3 natural armor, one1d4 bite, Improved Initiative,
bonus Weapon Finesse and trip immunity. Constrictor Snake has3 animal HD,
19 HP, Str17/Dex17/Con12/Int1/Wis12/Cha2,20-foot land speed,+2 natural armor,
one1d4 bite, Skill Focus Perception, Toughness and trip immunity.
These are implemented contracts, not live observed totals.

| Land skill | Viper | Constrictor Snake |
|---|---:|---:|
| Mobility |0 ranks +0 class +1 Dex +8 racial =9|1 rank +3 class +3 Dex +8 racial =15|
| Perception |1 rank +3 class +1 Wis +4 racial =9|1 rank +3 class +1 Wis +4 racial +3 feat =12|
| Stealth |1 rank +3 class +1 Dex +4 racial =9|1 rank +3 class +3 Dex +4 racial =11|

Creation-only allocation rejects existing ranks. Racial HP bases9/13 leave
native Constitution and Toughness dependencies live; no reload reallocation.
The exact new bite/owner handler follows live size and legitimate native
weapon-size shifts, respecting an existing dice override. It does not change
the native single-natural-attack Strength multiplier. Baselines are1d4-1
and1d4+4; actual native stat breakdowns remain mandatory.

Viper owns a deep-cloned native saved poison graph: damaging-bite wound gate,
Fortitude10+half2HD+liveCon (13 baseline),1d2 Constitution,six exposures,
one cure save. Its state/type/text is not Purple Worm's. Native poison save,
stacking and cure cadence remain runtime gates, not inferred from a clone.

Constrictor opts into the shared single-link grab lifecycle on the bite only,
target own size or smaller. Its own traits use1d4 crushing with live size
and1.5x positive Strength (penalty once). The new owner-scoped path refuses
dead prey, waits for a later round before maintain, claims a round once and
releases on prey death. Existing grabbers retain their old parameters and
paths; relevant old grab/death-roll/swallow regressions remain part of the
future feature-boundary gate. No grapple re-establishment is added.

Two new inspectable types and hidden CombatProfile facts remove worm identity
and donor skill/poison/swallow facts. New original icons are documented in
[the source and review record](../assets-source/original-icons/expanded-summoning/PHASE2-SPRINT17-SOURCES.md).
Their painting, grayscale/small-size and export checks do not qualify UI.

## Preservation and counts

Exactly73 identities append after the unchanged2836-entry ledger:2 units,
32 logical placements,32 template children,2 types,2 hidden combat profiles,
Viper poison/venom and Constrictor combat traits. Normalized prior-entry digest:
`bf46e4e3d2d709935f2a27fa32f2bd8ad640098801174680c755c89287ea0570`.
Every old symbol, GUID, type, status and note is covered by the new prefix test.
The generator now activates only inactive entries, avoiding historical-note
rewrites. Its first local pass rewrote29 notes; those were restored exactly
before qualification, with no published commit containing that rewrite.

Viper:SM I/SNA I,18 hidden roots. Constrictor:SM III/SNA III,14 hidden roots.
Totals:97 units;1008 registered,976 published,32 withheld;29 native wrappers;
1005 visible choices. The ledger contains2909 entries,2907 active and2 reserved.
The planned package adds only the two128-pixel exports:320 members with audio,
318 without. No old painting or protected assignment changed. Salamander's
unit `f8fb103168d74b4c93182437e5d2b4e4`, placements, visuals and icon are untouched.

## Source checks and remaining gate

250 focused checks PASS (246 Expanded Summoning plus4 global-ledger checks).
Complete unfiltered2048 tests PASS in82.1s. Eight new behavior/manifest tests
cover profiles,ranks/rejection,live modifiers,size shifts,withholding,
append-only identities and exact artwork consumers. Repository/static/manifest/
icon validation PASS. Exact-reference Release compile PASS against the qualified
14-reference bundle, as a dirty-tree compile diagnostic only.
The committed exact build/deterministic package/strict-package gate follows
this checkpoint; no dirty-tree package may be deployed.

Earlier source failures were a missing namespace import and stale ledger/
package-count fixtures. Corrected without gameplay assertion waivers. Logs:
`artifacts/sprint17-snake-focused-pass.log`,
`artifacts/sprint17-snake-full-domain-final.log`,
`artifacts/sprint17-snake-repository-pass.log`,
`artifacts/sprint17-snake-dirty-compile-fixed.log`.
The native poison cadence, exact final stats, natural attacks and grab/constrict,
save/load cleanup, original-body production attachment, UI, crowd/lifecycle,
and every private/public route still require the exact guarded artifact.

No runtime launch, lease/deployment, save operation or installation observation
occurred during this source work. Last transaction remains the exactly restored
a904 bind census at14:14:02UTC, not a qualification of these new units.
Accepted passive-sense and clean grapple-reset limitations remain unchanged.
Swim/climb and aquatic skill uses are omitted under land-use scope.

Next: committed artifact source gate; production snake-body attachment and
closed mechanics fixtures; bounded Salamander native spear/tail solution.
No blind human-bind deduplication, slashing-to-piercing relabeling or contact
waiver. Then the complete Sprint17 candidate/publication and Phase2B closure.
Phase2C authorized but deferred until Phase2B owner acceptance.
HumanReview: NOT_PERFORMED_NONBLOCKING.

## Preserved prelaunch rejection and bounded packaging correction

Exact7ab source/binary was built, but no accepted runtime artifact was emitted.
The DLL and complete build/log are retained under
`artifacts/sprint17-snake-prelaunch-rejected-7ab77c32/`:
DLL `4D7FD1C0296701EF41314FE1AB7A8E8566B012DE947D346B00FBCA71E7ED661F`;
gate log `A34BE7A7B05907BB1EB4051A8E92B77D26ABDCC3C761479BFAC65D288748A2E3`.
Failure class: stale build/package-validation cardinality, not gameplay or
environment. No repeated runtime assertion, waiver, deployment or save work.
Only the two107-to109 guard values change. The current source manifest and
consumer tests already require109, and the exact package still checks every
member/hash, rejects extras, and requires320 members with the soundbank.
The direct corrected-validator diagnostic ZIP is retained alongside the
rejected build; it is not a qualified gameplay candidate. Focused source
and complete exact-head gates must run on the correcting commit.
