# Favored Class Integration - Blockers, Defects and Open Questions

This file keeps only what the charter, the installed providers and the
qualification runs do not settle: environment risks, pre-existing KMG defects
outside the adopted rows, and suspected host defects. Every former owner
question that the charter or the observable provider behavior answers is
resolved below with the rule that decides it; required tests are recorded as
runs, not as decisions.

## Environment

### B1 - Shared Kingmaker installation (mitigated)

Another lab (KingmakerBuffPlanner) uses the same Kingmaker installation. The
two sessions follow an agreed protocol: before a launch or any change to
`Mods`, check that the other lab's `runtime-state\deployment.lock` and
`kbp-gate.active` markers are absent, hold this lab's
`compatibility-state\compatibility.lock` for the whole batch, message the other
session at batch start and end, and never touch the other lab's files, saves
or processes. Every KMG launch still goes through the guarded Steam App ID
640820 launcher, which fails closed on a running game, a foreign sentinel, or
a loaded identity that differs from the deployment manifest. The other lab
stages and restores its own `Mods` copy byte-exactly (including this lab's
installed KMG build) before releasing its lock. On 2026-09-24 a KMG batch
started while the other lab's lock-free WhatIf gate was running; that gate
failed closed on the live Kingmaker process and ran nothing. The other lab
now marks its gate with `kbp-gate.active`, and KMG batches wait for it.

### B2 - CotW-only and CotW + Favored Class profiles (resolved)

The profiles were staged from the 2026-09-08 capture of this install's
optional mods (`C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger\artifacts\teleportation\compatibility-references`),
verified byte-identical first (CallOfTheWild 266 files, ZFavoredClass 24
files, no difference; re-verified before each round). On the final candidate
`12651613c`:

- `gunslinger-only` / `observe-favored-class-host-state`: `20260926T0528022419214Z-observe-favored-class-host-state` PASS (transaction `compat-20260926T052602Z-a75f8c60d59d`, restoration verified).
- `gunslinger-call-of-the-wild` / `observe-favored-class-host-state`: `20260926T0531183716191Z-observe-favored-class-host-state` PASS (transaction `compat-20260926T052931Z-3aacec891548`, restoration verified).
- `gunslinger-call-of-the-wild-favored-class` / `observe-favored-class-host-state`: `20260926T0534505046553Z-observe-favored-class-host-state` PASS (transaction `compat-20260926T053303Z-0acfe3e6e607`, restoration verified).

### B3 - Native character-build lanes inflate the game's committed memory (open: environment risk for qualification batches)

Only the guarded lanes that drive the native character-build screens (level-up
and character creation) raise the game's committed (private) memory far above
its resident set; every other lane peaks under 5 GB. Sampled every 5 s through
both final cycles (`5ae4eafc7`, then the final candidate `12651613c`):

| Lanes | Native screenshots | Peak private | Peak system commit |
| --- | --- | --- | --- |
| Visual census (47 native level-up visits, 4 creator visits) | none | 41.9 / 43.0 GB | 65.1 / 67.7 GB |
| Four elemental creator lanes | 91-110 per run, about 30 KB each | 57-72 GB | 83-97 GB |
| Two native respec lanes | none | 46-47 GB | 70-73 GB |
| Every other lane, transaction and profile run | none | 2.3-4.9 GB | 25-30 GB |

In the census the private bytes grow by about 0.75 GB per native visit; in a
creator lane they grow in bursts (often about 3.5 GB within 10 s) that do not
follow the screenshot times. The memory is released only when the game exits
(each guarded run is a fresh process, so nothing accumulates across runs); the
resident set stays at 6-15 GB and the page file absorbs the rest. The earlier
attribution to the screenshot poll was wrong: the census and the respec lanes
save no screenshot at all, and `e7748fff4` (the poll now reads a completed PNG
once, which is kept) did not lower the peaks (the creator lanes peaked at
52-65 GB before it and at 57-72 GB after it).

The favored-class integration does not cause it. With the integration
disabled by the integration-off settings profile (the game log reports
`IntegrationDisabled` with 0 leaves published), the Ifrit creator lane on the
final deployment peaked at 70.1 GB private and 96.5 GB system commit
(`20260926T0536579357364Z-working-save-elemental-character-creation-regression`,
PASS, 110 screenshots), against 69.2 and 72.0 GB with it enabled.

Consequence: on this machine (31.6 GB of RAM, system-managed page file) the
commit limit grows as needed (to 109 GB during the final cycle) and every run
passes. On 2026-09-25, with about 66 GB already committed by other processes
before a reboot, the Ifrit creator lane hit the 127.6 GB ceiling three times
and a Sylph run reported Out of memory. Qualification batches therefore start
only on a machine with a low idle commit charge (about 23 GB before these
cycles). Follow-up, outside the favored-class rows: isolate the allocation the
native character-build screens retain (for example by measuring a single
screen open per fresh process, or unloading unused assets between visits).

## Pre-existing KMG defects (outside the adopted rows)

### D1 - Base Gunslinger cannot complete level 17 (fixed by the tabletop rule, owner decision)

`GunslingerClassBlueprints` grants the obligatory `KMG_GunTraining_Selection`
at levels 5, 9, 13 and 17, but the selection offers only the three official
rank-one types (Pistol, Musket, Blunderbuss). A base Gunslinger who took all
three can never complete level 17: `FeatureSelectionState.CanSelectAnything`
is true for the obligatory selection, so `LevelUpState.IsComplete()` stays
false. Native evidence: `20260924T0054089932945Z-disposable-favored-class-grit`
(`openSelections=[KMG_GunTraining_Selection]` at level 17, every other
completion term satisfied, for the test and the control unit). Mysterious
Stranger keeps exactly three picks and is not stuck; Pistolero and Musket
Master replace the picks. The favored-class twenty-level proof therefore uses
the Pistolero. Owner decision (2026-09-25): fix by the tabletop rules. Fixed in
`bb4dfd674`: the Gun Training selection is no longer obligatory; the native
gate still requires a pick while an official type is untrained, and a base
Gunslinger's 17th level (all three types trained) completes with the empty
pick. Native PASS: the favored-class grit lane levels a base Gunslinger to 17
(domain test `gun-training.empty-pick-completes`).

### D2 - Dead Shot critical confirmation is unreachable (fixed by the tabletop rule, owner decision)

`DeadShotRuntime` probes set `RuleAttackRoll.ImmuneToCriticalHit = true`, and
the native `RuleAttackRoll.OnTrigger` computes `IsCriticalRoll = hit &&
!ImmuneToCriticalHit && ...`, so no probe records a threat and
`DeadShotOutcomeService` never produces a confirmation. If that path were
reachable, `DeadShotRuntime.ConfigureDelivery` assigns (rather than adds)
`CriticalConfirmationBonus`, discarding Critical Focus and every other
confirmation bonus. Evidence: source and the decompiled native rule (static;
not reproduced natively). Consequence for G02/G08/G16: the firearm
confirmation counter applies to every reachable firearm confirmation roll;
Dead Shot had none. Owner decision (2026-09-25): fix by the tabletop rules.
Fixed in `bb4dfd674`: probes threaten from their natural roll against the
weapon's critical edge, and the auto-hit delivery makes the shot's single
confirmation with the native rules (attack bonus, every confirmation bonus
plus -5 and +1 per extra threat up to 0, and the critical AC computed after
the firearm AC frame so the touch-AC rule applies); immunity and the party
critical setting block it as they block a native threat, and a failed
confirmation is contained. Native PASS: Dead Shot confirmed, unconfirmed,
immune-target and touch-AC checks (`dead-shot-critical-touch-ac`).

### D3 - Gunslinger Initiative timing (fixed in this mission)

The native `UnitCombatPrepareController` stores `RuleInitiativeRoll.Result`
before it raises `IUnitInitiativeHandler`, so the Sprint 38 deed's +2 never
reached the stored initiative (native reproduction:
`20260924T0137282472516Z-disposable-favored-class-initiative-timing`, stored
11 against 17). Fixed in `732cddae3`: the deed adds its bonus inside the rule
before the result is stored. Native PASS in real-time and turn-based entry.

### D4 - Same-level target picks dropped by the native replay (fixed in this mission)

The native level-up replays its picks in priority order, and the Favored
Class host's reward selection has an earlier priority (its feature group) than
a bloodline, a revelation or a power chosen in the same level-up. A
selected-power or selected-revelation counter picked after its target in the
same level-up was checked before the target existed and silently dropped (an
Ifrit Sorcerer 1 could not invest the level-1 bonus in the Fire Ray it had
just chosen; an Oracle could not invest in the revelation of that level).
Found by the L03 respec lane's native build of that Sorcerer. Fixed in
`b1b93acad`: a native replay hook (`LevelUpController.ApplyLevelup`) marks
the level-up being replayed, and the owned-target prerequisite also counts a
later pick of that same level-up; the usable-power check uses the same
prerequisite. Native PASS: the respec lane builds the Sorcerer through that
exact level-1 sequence.

### D5 - Stored level plans bypassed the same-level scope (fixed in this mission)

A stored level plan (auto-level for companions, a pregen, an imported
companion) is applied by the level-up controller's own constructor, one
`AddAction(action, ignoreOrder: true)` per planned pick, outside the scoped
replay of D4. In priority order the plan lists the host's reward before the
bloodline, revelation or power chosen at that level, so a counter targeting
that choice was rejected and silently dropped (the level committed without
it). Found by the E15 research and reproduced natively. Fixed in `60c93b319`:
a transpiler on `LevelUpController.ApplyLevelUpPlan` makes that one call
inside a scope of its controller and plan (closed in a finally block), and
the owned-target prerequisite also counts the plan's own picks. Native PASS:
the auto-level lane applies a recorded Ifrit Sorcerer level (reward listed
before its ray) whole through the constructor, while the same native
`AddAction` calls without the scope reject exactly the reward; a host-only
plan and Linzi's own stored plan are accepted identically either way.

### D6 - Pistol-Whip ignores the firearm's enhancement bonus (owner decision)

The tabletop Pistol-Whip adds the firearm's enhancement bonus to the attack
and damage rolls. KMG's deed (`PistolWhipRuntime`) attacks with its surrogate
weapon and copies the enhancement only into the damage weapon stats; the
attack roll gains nothing. Native evidence: the favored-class Gunslinger
mechanics lane records `enhancedPistolAttackDelta = 0` for a +1 pistol
against a plain one. The favored-class counter (G05/G20) adds exactly its
steps either way. This deed behavior predates the mission and lies outside
the favored-class rows; it is recorded for the owner and not changed.
Decision needed: apply the enhancement to the Pistol-Whip attack (and verify
its damage) by the tabletop rule, or keep the current behavior.

## Suspected host defects (Favored Class 1.3.1; recorded, never patched)

- H-1: the host's own Eidolon natural-armor reward likely grants +0 (not
  verified natively). KMG's O08 counter reads the summoner's own leaf rank and
  gives +2 at two steps natively.
- H-2: the host's Drow Disarm reward is capped at 1 (host data; unrelated to
  KMG rows).

## Former owner questions, resolved by the charter or provider behavior

| Former item | Resolution | Rule or evidence |
| --- | --- | --- |
| OD-1 Mostly Human scope | A genuine dual identity: human (humanoid) for race-related rules and prerequisites (the host's human favored-class leaves and human race traits, humanoid targeting such as Hold/Charm/Enlarge/Reduce Person), native outsider identity, race, RaceId, scores and geniekin favored-class routes retained; meaningful with the integration off; never inferred from appearance; other providers fail closed. | Charter 5.2/6.5, E05/E07/E08/E10; `disposable-favored-class-mostly-human` |
| OD-2 Other "counts as human" providers | Not recognized: only the verified Mostly Human identity and the host's explicit human routes grant human access (Races Unleashed's Suli Mostly Human stays closed). | Charter: unknown providers never acquire unrestricted eligibility (E10, section on provider scope) |
| OD-3 Host race traits | The host's human race traits open to a Mostly Human geniekin through the scoped bridge, and stay closed to the standard control. | Mostly Human host-bridge assertion |
| OD-4 Two paladins | Native descriptor and strongest-source behavior: one Replace instance at a time carrying its own paladin's Morale bonus (+6 invested, +4 native), never a sum, no stacking with Remove Fear; self immunities unchanged. | M17; lifecycle `fcb-lifecycle-aura-overlap` |
| OD-5 Bloodline eligibility and thresholds | Eligible bloodlines are exactly Elemental (Fire) and (Air) plus proven aliases. The chosen power's own level-dependent effects follow the effective level, including Blast's extra uses at 17th and 20th level (at most two steps); no other bloodline power, feature, spell, BAB or save is gained early (review finding 2). | Charter 8.10 and I08/S06; target manifest; advanced lane |
| OD-6 Revelation thresholds | The chosen revelation's own thresholds and level gates follow the effective level, so it gains its own later abilities and forms earlier; no other revelation, choice, class feature, spell, BAB, save or capstone is granted. Spirit of the Warrior is excluded because its possession sets BAB from oracle level (review finding 2); a held rank of an excluded or withheld target is inert (second review finding 2). | Charter 8.10 and I06/S04; oracle lane (level gates) |
| OD-7 Performance scope | Owner-local actual range, ring and displayed text agree; Storm Call and Mockery are excluded with recorded reasons; instantaneous, personal, masterpiece and inert entries are not targets. | Charter 8.7; target manifest O01; `disposable-favored-class-performance-range` |
| OD-8 Icons | Every published choice has a deterministic appropriate existing donor icon; no published choice is blank. Original art is optional later polish. | Icon catalog; domain census; native visual census |
| OD-9 Elemental re-qualification | Required test: the elemental creator and respec lanes were re-run. | Run table in the report |
| OD-10 Persistence breadth | Required test: a fresh-process reload of one subject per state/mechanic family (partial and full investment; the selected firearm, performance, revelation and bloodline targets; spent grit with its raised maximum; Nimble; the Undine Monk; owner-local performance and aura areas; companion armor with replacement; revelation and bloodline scaling; Mostly Human identity and human access), and the own persistence case of every remaining scheduled row (G05, G06 Dodge, G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04, S06): all SAVE TESTED. | L01 families and rows transactions in the report |
| OD-11 Host and dependency states | Owner-authorized: a real disabled host through a byte-exact Params.xml stage (H02), simulated host defects on cloned live observations (H02 unsupported and partial, H04, H05), and a missing-dependency fixture save read in the disabled-host profile (L06). | Host-state, host-defects and missing-dependency lanes in the report |
| OD-12 Turn-based Gunslinger's Dodge window | The adaptation's text promises "+2 dodge bonus to AC for one round"; a round-duration buff ends at the next round's start, so in turn-based mode it covers every actor after the Gunslinger in that round, not one whose turn opens the next round. Native round semantics, consistent with the text; checked at every observation inside the round (each time step and the enemy's turn when it falls there) and gone at the next round's start. | Turn-modes lane |
