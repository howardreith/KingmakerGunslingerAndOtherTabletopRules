# Expanded Summoning Phase 1 implementation report (charter Sprints 3-8)

Draft, never merged under the 2026-09-24 order. Per-sprint sections are
added as each sprint completes; run identities live in
`EXPANDED-SUMMONING-PHASE1-AUTONOMOUS-STATE.md`.

## 1. Baseline and identifiers

- Base `master` @ `b0641a58` - Phase 0 finalized and merged (PR #21 at
  `ba5e20a7`, 2026-09-24T16:55:50Z). Release 0.0.138; live installation
  0.0.117 / 136 files throughout, restored and verified after every run.
- Branch `codex/expanded-summoning-phase1-sprints3-8` in its own worktree.
- Integrated master verification on `b0641a58`: recorded in the state file
  (build, package and one guarded smoke scenario).

## 2. Method

Each creature work item goes through the same path: catalog entry, donor,
profile or builder, ledger identities (append-only, allocated by
`tools/expanded_summoning_manifest.py`), icon, count pins in the domain
tests, the manifest tool, the runner and the static validation; then the
guarded runtime scenarios - structural inventory, mechanical casting, player
path, visual contracts, the persistence trio, the compatibility profiles - and
an internal review of in-game images for anything visual. Nothing is accepted
on assertions alone when a render can contradict them; Phase 0's last finding
(a material outside the game's fades, invisible while every observer was
satisfied) is the standing reason.

## 3. Sprint sections

### Sprint 3 - Native Publication Pack I

| Item | Placement | Donor (audit-proven) | Chassis | Signature / deviations | Status |
|---|---|---|---|---|---|
| Pony | SM I (templated), SNA I | `PonySummoned` `3f95557fc806db741b500a5735990841`, dedicated summon | Animal 2 HD, Medium, 13/13/14/2/11/4, 40 ft, two 1d3 hooves (native `SmallHoof1d3`) | Endurance/Run omitted; hooves primary as on the native summon | implemented; runtime qualification in progress |
| Horse | SM II (templated), SNA II | `HorseSummoned` `5bb9579fdb2b26b48bb10d61c81cfdfb`, dedicated summon | Animal 2 HD, Large, 16/14/17/2/13/7, 50 ft, two 1d4 hooves (native `Hoof1d4`), reduced reach | Endurance/Run omitted | implemented; runtime qualification in progress |
| Owlbear | SNA IV | `CR8_OwlbearStandard` `d6e0acbdbdb56114898922063ae2cba0`, sanitized body | Magical beast 5 HD, Large, 19/12/18/2/12/10, 30 ft, NA +5, bite 1d6, two 1d6 claws, Improved Initiative, Great Fortitude, Skill Focus (Perception), reduced reach | claw grab deferred to Sprint 4's shared grapple lifecycle | implemented; runtime qualification in progress |
| Cyclops | SNA V | `CR5_CyclopStandard` `124f1c45ef24d654e9cd420fe84f7f36`, sanitized body | Humanoid 10 HD, Large, 21/8/15/10/13/8, 30 ft, NA +7, greataxe (Large 3d6), Ferocity, Power Attack, Cleave | Flash of Insight bounded (one swift use per summoning: next attack roll auto-hit and threat; confirmation ordinary); hide armor, crossbow, Alertness, Great Cleave, Improved Bull Rush omitted | implemented; runtime qualification in progress |
| Frost Giant | SNA VII / VIII (1d3) / IX (1d4+1) | retained native unit `590cd3d5e76fdc649a5f97bc984cd3c4`, no new identity | native wrappers carved from the Mastodon options (`6d8d59aa…`, `256739c1…`, `9bd8cb61…`), spawn unit replaced | identity reused (charter D-01); SM VIII wrapper unchanged | implemented; runtime qualification in progress |

Placements propagate to the 1d3 / 1d4+1 tiers by construction (45 new
logical placements, 34 template executions). Ledger: 92 identities appended
and active (`blueprints/blueprints.json`, entries 1957-2048), pinned by
`tools/validate_expanded_summoning_phase1.py`. Icons: four Blender
procedural renders (see the state file). Tests: five Sprint 3 domain
regressions; suite 1779/1779. Runtime evidence: recorded in the state file
as the guarded batches complete.

### Sprint 4 - Native Publication Pack II - Plants and Colossal Reuse

| Item | Placement | Donor (audit-proven) | Chassis | Signature / deviations | Status |
|---|---|---|---|---|---|
| Shambling Mound | SNA VI (1d3 at VII, 1d4+1 at VIII-IX) | `CR6_ShamblingMound` `b98ae409beb5e8543a75b82ecda082a7`, sanitized body | Plant 9 HD, Large, 21/10/17/7/10/9, 20 ft, NA +10, two native 2d6 slams, fire resistance 10, electricity immunity, Power Attack, Iron Will, Lightning Reflexes, Cleave, Weapon Focus (slam) | slam grab and constrict 2d6+7 on the shared lifecycle; Electric Fortitude's Constitution gain, swim and the native poison aura omitted | implemented; runtime qualification in progress |
| Giant Flytrap | SNA VII (1d3 at VIII, 1d4+1 at IX) | `CR10_GiantFlytrapStandard` `fb824352b7968fb4d8103ac439644633`, sanitized body | Plant 13 HD, Huge, 25/18/25/1/12/6, 10 ft, NA +10, four native 1d8 bites, acid resistance 20, native 60-ft blindsight (tremorsense), trip immunity, Cleave, Great Fortitude, Improved Initiative, Power Attack, Skill Focus (Stealth), Weapon Focus (bite) | bite grab on the shared lifecycle, one held target at a time; engulf and Vital Strike omitted | implemented; runtime qualification in progress |
| Purple Worm | SNA VIII (1d3 at IX) | native `PurpleWormSummoned` `bf2216f48b3f4d24c9c502007649340d`, dedicated summon, rebuilt on the natural builder | Magical beast 16 HD, Gargantuan, 35/6/25/1/8/8, 20 ft, NA +22, native bite and sting, exact native sting poison, trip immunity, Critical Focus, Improved Critical (bite), Power Attack, Weapon Focus (bite) | bite grab swallows whole through the native part (swallowed state cloned from the native worm); burrow, swim, the native brain, Awesome Blow, Improved Bull Rush, Staggering Critical, Weapon Focus (sting) omitted | implemented; runtime qualification in progress |
| Shared grapple lifecycle | Owlbear, Shambling Mound, Giant Flytrap, Purple Worm (Sprints 6-8 reuse) | native `UnitPartGrappleInitiator` / `UnitPartGrappleTarget` / `UnitPartSwallowWhole` | `SummonGrabComponent`, `SummonHoldComponent`, `SummonSwallowLifecycleComponent`, `SummonGrappleAreaSafeguard`; shared `Grapple.Hold` and `Grapple.Grappled` buffs | grab +4 through the game's check; maintain +5 each round or release; hold-buff end releases its own target; swallow spit-out on traits end; area leave/load safeguard | implemented; runtime qualification in progress |

Placements propagate to the 1d3 / 1d4+1 tiers by construction (9 new
logical placements). Ledger: 19 identities appended and active
(`blueprints/blueprints.json`, entries 2049-2067), pinned by
`tools/validate_expanded_summoning_phase1.py`. Icons: three Blender
procedural renders. Tests: four Sprint 4 domain regressions; suite
1783/1783. Runtime evidence: recorded in the state file as the guarded
batches complete.

### Sprint 5 - Mephit Family Expansion

Not started.

### Sprint 6 - Existing Signature Mechanics Repair

Not started.

### Sprint 7 - Big-Cat Combat System

Not started.

### Sprint 8 - Big-Cat Roster Completion

Not started.

## Appendix A - tabletop stat blocks (fetched 2026-09-24 from the public SRD)

Recorded here so the profiles can be checked against their source without a
network. Numbers are the Bestiary's; Kingmaker deviations are recorded per
profile in the catalog.

- Pony: N Medium animal; AC 11 (+1 Dex); hp 13 (2d8+4); Fort +5 Ref +4 Will +0; 40 ft; 2 hooves -3 (1d3, secondary/docile); Str 13 Dex 13 Con 14 Int 2 Wis 11 Cha 4; Endurance, Run.
- Horse: N Large animal; AC 11 (+2 Dex -1 size); hp 15 (2d8+6); Fort +6 Ref +5 Will +1; 50 ft; 2 hooves -2 (1d4+1, secondary/docile); Str 16 Dex 14 Con 17 Int 2 Wis 13 Cha 7; Endurance, Run.
- Owlbear: N Large magical beast; AC 15 (+1 Dex +5 natural -1 size); hp 47 (5d10+20); Fort +10 Ref +5 Will +2; 30 ft; 2 claws +8 (1d6+4 plus grab), bite +8 (1d6+4); Str 19 Dex 12 Con 18 Int 2 Wis 12 Cha 10; Improved Initiative, Great Fortitude, Skill Focus (Perception).
- Cyclops: NE Large humanoid (giant); AC 19 (+4 armor -1 Dex +7 natural -1 size); hp 65 (10d8+20); Fort +9 Ref +2 Will +4; ferocity; 30 ft; greataxe +11/+6 (3d6+7/x3); heavy crossbow +5 (2d8/19-20); reach 10; Str 21 Dex 8 Con 15 Int 10 Wis 13 Cha 8; Alertness, Cleave, Great Cleave, Improved Bull Rush, Power Attack; Flash of Insight (Su) 1/day immediate action: select the exact result of one of its own die rolls before rolling.
- Shambling Mound: N Large plant; AC 19 (+10 natural -1 size); hp 67 (9d8+27); Fort +9 Ref +5 Will +5; immune electricity (electric fortitude), resist fire 10, plant traits; 20 ft, swim 20; 2 slams +11 (2d6+5 plus grab); constrict 2d6+7; reach 10; Str 21 Dex 10 Con 17 Int 7 Wis 10 Cha 9; Cleave, Iron Will, Lightning Reflexes, Power Attack, Weapon Focus (slam).
- Giant Flytrap: N Huge plant; AC 22 (+4 Dex +10 natural -2 size); hp 149 (13d8+91); Fort +17 Ref +8 Will +5; plant immunities, resist acid 20; low-light, tremorsense 60; 10 ft; 4 bites +15 (1d8+7 plus grab); space/reach 15/15; engulf (1d8+7 plus 2d6 acid); Str 25 Dex 18 Con 25 Int 1 Wis 12 Cha 6; Cleave, Great Fortitude, Improved Initiative, Power Attack, Skill Focus (Stealth), Vital Strike, Weapon Focus (bite).
- Purple Worm: N Gargantuan magical beast; AC 26 (-2 Dex +22 natural -4 size); hp 200 (16d10+112); Fort +17 Ref +8 Will +4; darkvision, tremorsense 60; 20 ft, burrow 20, swim 10; bite +25 (4d8+12/19-20 plus grab), sting +25 (2d8+12 plus poison); space/reach 20/15; swallow whole (4d8+12 bludgeoning, AC 21, 20 hp); poison Fort DC 25, 1/round for 6 rounds, 1d4 Str, cure 3 saves; Str 35 Dex 6 Con 25 Int 1 Wis 8 Cha 8; Awesome Blow, Critical Focus, Improved Bull Rush, Improved Critical (bite), Power Attack, Staggering Critical, Weapon Focus (bite, sting).
- Mephits (all N Small outsider, 3d10+3 hp 19, Fort +2 Ref +5 Will +3, DR 5/magic, 2 claws +5 (1d3+1), Str 13 Dex 15 Con 12 Int 6 Wis 11 Cha 14, Dodge, Improved Initiative, fast healing 2 in a named environment, 15-ft cone breath every 4 rounds Reflex DC 13):
  Dust (air) AC 17, fly 50 perfect, breath 1d4 slashing plus sickened 3 rounds, blur 1/hour, wind wall 1/day.
  Ice (cold) AC 17, fly 40, immune cold, vulnerable fire, breath 1d4 cold plus sickened 3 rounds, chill metal 1/day DC 14, magic missile 1/hour.
  Magma (fire) AC 16, fly 40, immune fire, vulnerable cold, breath 1d8 fire, pyrotechnics 1/day, magma form 1/hour.
  Ooze (water) AC 17, swim 30, breath 1d4 acid plus sickened 3 rounds (Reflex negates both), acid arrow 1/hour, stinking cloud 1/day DC 15.
  Salt (earth) AC 17, fly 40, breath 1d4 slashing plus sickened 3 rounds, glitterdust 1/hour DC 14, dehydrate 1/day 20-ft radius 2d8 Fort DC 14 half.
  Steam (fire) AC 17, fly 40, immune fire, vulnerable cold, breath 1d4 fire plus sickened 3 rounds, blur 1/hour, boiling rain 1/day 20-ft square 2d6 fire Fort DC 14 half.
- Tiger: N Large animal; AC 14 (+2 Dex +3 natural -1 size); hp 45 (6d8+18); Fort +8 Ref +7 Will +3; 40 ft; 2 claws +10 (1d8+6 plus grab), bite +9 (2d6+6 plus grab); pounce, rake (2 claws +10, 1d8+6); Str 23 Dex 15 Con 17 Int 2 Wis 12 Cha 6; Improved Initiative, Skill Focus (Perception), Weapon Focus (claw).
- Cheetah: N Medium animal; AC 15 (+4 Dex +1 natural); hp 19 (3d8+6); Fort +5 Ref +7 Will +2; 50 ft, sprint (once per hour, ten times speed on a charge); bite +6 (1d6+3 plus trip), 2 claws +6 (1d3+3); Str 17 Dex 19 Con 15 Int 2 Wis 12 Cha 6; Improved Initiative, Weapon Finesse.
