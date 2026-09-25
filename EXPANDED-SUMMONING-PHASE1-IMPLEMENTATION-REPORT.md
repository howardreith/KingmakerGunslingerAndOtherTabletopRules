# Expanded Summoning Phase 1 implementation report (charter Sprints 3-8)

Draft, never merged under the 2026-09-24 order. Per-sprint sections are
added as each sprint completes; run identities live in
`EXPANDED-SUMMONING-PHASE1-AUTONOMOUS-STATE.md`. The 2026-09-25 correction
order (section 4) corrected and requalified Sprints 3-8; the sprint tables
below describe the corrected mechanics.

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
| Pony | SM I (templated), SNA I | `PonySummoned` `3f95557fc806db741b500a5735990841`, dedicated summon | Animal 2 HD, Medium, 13/13/14/2/11/4, 40 ft, two 1d3 hooves (native `SmallHoof1d3`) | both hooves secondary (Docile: the game's own secondary flag, -5 to hit and half Strength to damage; corrected 2026-09-25); Endurance/Run omitted | complete; corrected and requalified (correction record) |
| Horse | SM II (templated), SNA II | `HorseSummoned` `5bb9579fdb2b26b48bb10d61c81cfdfb`, dedicated summon | Animal 2 HD, Large, 16/14/17/2/13/7, 50 ft, two 1d4 hooves (native `Hoof1d4`), reduced reach | both hooves secondary (Docile; corrected 2026-09-25); Endurance/Run omitted | complete; corrected and requalified (correction record) |
| Owlbear | SNA IV | `CR8_OwlbearStandard` `d6e0acbdbdb56114898922063ae2cba0`, sanitized body | Magical beast 5 HD, Large, 19/12/18/2/12/10, 30 ft, NA +5, bite 1d6, two 1d6 claws, Improved Initiative, Great Fortitude, Skill Focus (Perception), reduced reach | claw grab deferred to Sprint 4's shared grapple lifecycle | complete; internally accepted on the Sprints 3-8 evidence |
| Cyclops | SNA V | `CR5_CyclopStandard` `124f1c45ef24d654e9cd420fe84f7f36`, sanitized body | Humanoid 10 HD, Large, 21/8/15/10/13/8, 30 ft, NA +7 plus +4 hide armor as an armor-descriptor fact (AC 19), greataxe (Large 3d6), Ferocity, Power Attack, Cleave | Flash of Insight bounded (one swift use per summoning, armed until the next attack roll: that attack's own d20 is chosen as a natural 20, ordinary confirmation, no other roll touched, one use across save and reload; corrected 2026-09-25); crossbow, Alertness, Great Cleave, Improved Bull Rush omitted | complete; corrected and requalified (correction record) |
| Frost Giant | SNA VII / VIII (1d3) / IX (1d4+1) | retained native unit `590cd3d5e76fdc649a5f97bc984cd3c4`, no new identity | native wrappers carved from the Mastodon options (`6d8d59aa…`, `256739c1…`, `9bd8cb61…`), spawn unit replaced | identity reused (charter D-01); SM VIII wrapper unchanged | complete; internally accepted on the Sprints 3-8 evidence |

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
| Shambling Mound | SNA VI (1d3 at VII, 1d4+1 at VIII-IX) | `CR6_ShamblingMound` `b98ae409beb5e8543a75b82ecda082a7`, sanitized body | Plant 9 HD, Large, 21/10/17/7/10/9, 20 ft, NA +10, two native 2d6 slams, fire resistance 10, electricity immunity, Power Attack, Iron Will, Lightning Reflexes, Cleave, Weapon Focus (slam) | slam grab and constrict 2d6+7 on the shared lifecycle; Electric Fortitude's Constitution gain, swim and the native poison aura omitted | complete; internally accepted on the Sprints 3-8 evidence |
| Giant Flytrap | SNA VII (1d3 at VIII, 1d4+1 at IX) | `CR10_GiantFlytrapStandard` `fb824352b7968fb4d8103ac439644633`, sanitized body | Plant 13 HD, Huge, 25/18/25/1/12/6, 10 ft, NA +10, four native 1d8 bites, acid resistance 20, native 60-ft blindsight (tremorsense), trip immunity, Cleave, Great Fortitude, Improved Initiative, Power Attack, Skill Focus (Stealth), Weapon Focus (bite) | one grab link per bite (four at most) on the shared lifecycle; engulf of a Medium or smaller foe held since the round began (1d8+7 bludgeoning plus 1d8 acid each round inside); release on escape, the last link's end, disposal, the swallow lifecycle and the area-leave sweep (corrected 2026-09-25); Vital Strike omitted | complete; corrected and requalified (correction record) |
| Purple Worm | SNA VIII (1d3 at IX) | native `PurpleWormSummoned` `bf2216f48b3f4d24c9c502007649340d`, dedicated summon, rebuilt on the natural builder | Magical beast 16 HD, Gargantuan, 35/6/25/1/8/8, 20 ft, NA +22, native bite and sting, exact native sting poison, trip immunity, Critical Focus, Improved Critical (bite), Power Attack, Weapon Focus (bite) | bite grab holds; a later turn's successful maintain check swallows a foe up to one size smaller whole through the native part (swallowed state cloned from the native worm; corrected 2026-09-25); burrow, swim, the native brain, Awesome Blow, Improved Bull Rush, Staggering Critical, Weapon Focus (sting) omitted | complete; corrected and requalified (correction record) |
| Shared grapple lifecycle | Owlbear, Shambling Mound, Giant Flytrap, Purple Worm (Sprints 6-8 reuse) | native `UnitPartGrappleInitiator` / `UnitPartGrappleTarget` / `UnitPartSwallowWhole` | `SummonGrabComponent`, `SummonHoldComponent`, `SummonSwallowLifecycleComponent`, `SummonGrappleAreaSafeguard`; shared `Grapple.Hold` and `Grapple.Grappled` buffs | grab by limb identity against a foe of the holder's size or smaller, +4 through the game's check; maintain +5 more each round with the establishing limb's own weapon damage, or release; both checks apply the attack-roll natural 1 and 20 over the engine's sum-only maneuver rule; swallow whole on a later turn's successful check against a foe up to one size smaller; multi-link holds for the Flytrap; hold-buff end releases its own target(s); swallow spit-out on traits end; area leave/load safeguard (corrected 2026-09-25) | complete; corrected and requalified (correction record) |

Placements propagate to the 1d3 / 1d4+1 tiers by construction (9 new
logical placements). Ledger: 19 identities appended and active
(`blueprints/blueprints.json`, entries 2049-2067), pinned by
`tools/validate_expanded_summoning_phase1.py`. Icons: three Blender
procedural renders. Tests: four Sprint 4 domain regressions; suite
1783/1783. Runtime evidence: see the runtime qualification record in the state file.

### Sprint 5 - Mephit Family Expansion

| Item | Placement | Donor (audit-proven) | Chassis | Signature / deviations | Status |
|---|---|---|---|---|---|
| Dust Mephit | SM IV / SNA IV (1d3 at V, 1d4+1 at VI-IX) | native `MephitAirSummoned` `50782bc4eb36aac4287023e20ee00808`, dedicated summon | Outsider 3 HD, Small, 13/15/12/6/11/14, 40 ft, NA +3, two native 1d4 claws, DR 5/magic, fast healing 2, air subtype, Dodge, Improved Initiative | 15-ft enemies-only 1d4 slashing breath, sickened 3 rounds on a failed Reflex save; blur once per summoning; project Wind Wall once per summoning (15-ft shelter for every creature inside that is not the mephit's enemy, shelter for 6 rounds: arrows and bolts miss, other ranged weapons 30% miss chance; corrected 2026-09-25); warm sand rim glow (the mephit body's visible colour) | complete; corrected and requalified (correction record) |
| Ice Mephit | SM IV / SNA IV | native `MephitWaterSummoned` `4615328295cd7e84bb2ef09d3dba8403`, dedicated summon | as above; air subtype, cold immunity, fire vulnerability | 15-ft enemies-only 1d4 cold breath, sickened rider; magic missile once per summoning; project Chill Metal once per summoning (a metal-bearing foe within close range, Will negates, the seven-round cold table; corrected 2026-09-25); icy cyan-white rim glow | complete; corrected and requalified (correction record) |
| Magma Mephit | SM IV / SNA IV | native `MephitFireSummoned` `10a820de0a417f345866f794324205ad`, dedicated summon | as above; earth and fire subtypes (fire immunity, cold vulnerability) | 15-ft enemies-only 1d8 fire breath (no rider); project Pyrotechnics once per summoning (enemies within 20 ft blinded 1d4+1 rounds, Will negates) and project Magma Form once per summoning (5 rounds: DR 20/magic, speed 10, no attacks, abilities intact; corrected 2026-09-25); ember-red rim glow (no emission slot on the shader) | complete; corrected and requalified (correction record) |
| Ooze Mephit | SM IV / SNA IV | native `MephitWaterSummoned` `4615328295cd7e84bb2ef09d3dba8403`, dedicated summon | as above; water subtype | 15-ft enemies-only 1d4 acid breath, Reflex negates damage and sickening together; acid arrow once per summoning; stinking cloud once per summoning on a project ally-safe clone of the native cloud area (enemies of the caster only; corrected 2026-09-25); slime-green rim glow | complete; corrected and requalified (correction record) |
| Salt Mephit | SM IV / SNA IV | native `MephitAirSummoned` `50782bc4eb36aac4287023e20ee00808`, dedicated summon | as above; earth subtype | 15-ft enemies-only 1d4 slashing breath, sickened rider; glitterdust (enemies only; corrected 2026-09-25) once per summoning; dehydrate as a project 20-ft enemies-only burst (2d8, Fortitude half) once per summoning; crystalline white rim glow | complete; corrected and requalified (correction record) |
| Steam Mephit | SM IV / SNA IV | native `MephitWaterSummoned` `4615328295cd7e84bb2ef09d3dba8403`, dedicated summon | as above; fire and water subtypes (fire immunity, cold vulnerability) | 15-ft enemies-only 1d4 fire breath, sickened rider; blur once per summoning; boiling rain as a project 20-ft enemies-only burst (2d6 fire, Fortitude half) once per summoning; grey-white vapour rim glow (no emission slot on the shader) | complete; internally accepted on the Sprints 3-8 evidence |
| Shared visual variant | the six mephits (later sprints reuse) | native rig materials | `ExpandedSummoningVisualVariantPatch`: private material clone per view on attach carrying a tint and rim light colour (the mephits' element glow; the lion's tint) or a procedural coat on the main texture (the tiger, the cheetah), handed to the game's material controller | never writes the donor's shared material; every clone, coat texture and controller instance is owned per view and destroyed with the view, on a failed attach and on the module sweep (corrected 2026-09-25; the visual lifecycle scenario counts them back to baseline); attach outcome and capture-time materials recorded per view for the review; a tint cannot show on the mephit rig (the controller rewrites its tint slot) and a main texture barely does (translucent body) - the round-8 review found it and the round-11 probe proved it | complete; corrected and requalified (correction record)d on the Sprints 3-8 evidence |

Placements propagate to the 1d3 / 1d4+1 tiers by construction (72 new
logical placements across both families). Ledger: 126 identities appended
and active (`blueprints/blueprints.json`, entries 2068-2193), pinned by
`tools/validate_expanded_summoning_phase1.py`. Icons: six Blender
procedural renders. Tests: four Sprint 5 domain regressions; suite
1787/1787. Runtime evidence: see the runtime qualification record in the state file.

### Sprint 6 - Existing Signature Mechanics Repair

| Item | Placement | Donor (audit-proven) | Chassis | Signature / deviations | Status |
|---|---|---|---|---|---|
| Monitor Lizard | SM III / SNA III (unchanged) | unchanged | unchanged | bite grab on the shared summon grapple lifecycle (`c988aa874d11ff84d873508ddc9b928f`); no donor constrict | complete; internally accepted on the Sprints 3-8 evidence |
| Grizzly Bear | SM IV / SNA IV (unchanged) | unchanged | unchanged | claw grab on the shared lifecycle (`c76f72a862d168d44838206524366e1c`) | complete; internally accepted on the Sprints 3-8 evidence |
| Dire Bear | SM VI / SNA VI (unchanged) | unchanged | unchanged | claw grab on the shared lifecycle | complete; internally accepted on the Sprints 3-8 evidence |
| Giant Spider | SM II / SNA II (unchanged) | unchanged | native 60-ft blindsight (tremorsense), native web immunity added | ranged Web: a 50-ft ranged touch attack through the projectile delivery, one foe up to one size larger, no save, native web-grappled state up to ten rounds with its Constitution-based break-free, native immunity, two uses per summoning, own brain (corrected 2026-09-25); climb omitted (no save-safe seam) | complete; corrected and requalified (correction record) |
| Pixie | SNA IX (unchanged) | unchanged | unchanged | verified: sixteen sleep arrows and one irresistible dance per summoning on named resources, one cast action; live mechanical evidence each run | verified; internally accepted on the Sprints 3-8 evidence |

Ledger: 8 identities appended and active (`blueprints/blueprints.json`,
entries 2194-2201), pinned by `tools/validate_expanded_summoning_phase1.py`.
No new icons or package files. Tests: four Sprint 6 domain regressions;
suite 1791/1791. Runtime evidence: see the runtime qualification record in the state file.

### Sprint 7 - Big-Cat Combat System

| Item | Placement | Donor (audit-proven) | Chassis | Signature / deviations | Status |
|---|---|---|---|---|---|
| Leopard | SM III / SNA III (unchanged) | unchanged | unchanged (bite, two claws, two rake claws, Pounce) | bite grab by limb identity (a foe of its size or smaller); rake gate: a charge or the foe held since the round began, dropped from other full attacks (corrected 2026-09-25) | complete; corrected and requalified (correction record) |
| Lion | SM IV / SNA IV (unchanged) | unchanged (leopard rig) | unchanged | bite grab; rake gate; tawny visual tint on the leopard rig (no mane) | complete; corrected and requalified (correction record) |
| Dire Lion | SM V / SNA V (unchanged) | unchanged | unchanged (secondary rake pair) | bite grab; rake gate | complete; corrected and requalified (correction record) |
| Smilodon (Dire Tiger) | SM VI / SNA VI (unchanged) | unchanged | unchanged (secondary rake pair) | bite and foreclaw grab; rake gate | complete; corrected and requalified (correction record) |
| Shared rake gate | the four cats (Sprint 8 reuses) | native attack rules | `SummonRakeComponent` on each cat's combat-traits buff and `ExpandedSummoningRakeSequencePatch` on `UnitAttack.CreateFullAttack` | rake claws strike only on a charge or against the exact foe held since the cat's round began; the sequencing seam drops them from any other full attack; a single attack (an attack of opportunity) never carries one; a rake claw never grabs | complete; corrected and requalified (correction record) |

Ledger: 4 identities appended and active (`blueprints/blueprints.json`,
entries 2202-2205), pinned by `tools/validate_expanded_summoning_phase1.py`.
No new icons or package files. Tests: four Sprint 7 domain regressions;
suite 1795/1795. Runtime evidence: see the runtime qualification record in the state file.

### Sprint 8 - Big-Cat Roster Completion

| Item | Placement | Donor (audit-proven) | Chassis | Signature / deviations | Status |
|---|---|---|---|---|---|
| Tiger | SNA IV (1d3 at V, 1d4+1 at VI-IX); new | `LeopardSummoned` `768275c9885dd954fb3c84ba69ac4281` (the leopard rig), 1.25 view scale | Animal 6 HD, Large, 23/15/17/2/12/6, 40 ft, NA +3, native 2d6 bite, four project 1d8 claws, Pounce, Improved Initiative, Skill Focus (Perception), Weapon Focus (claw) | bite and foreclaw grab by limb identity on the shared lifecycle; rake gate (corrected 2026-09-25); procedural striped coat generated in the rig's texture space; Run and Skill Focus (Stealth) omitted | complete; corrected and requalified (correction record) |
| Cheetah | SM III / SNA III (unchanged) | unchanged (leopard rig), 0.92 view scale | unchanged (bite, two claws, trip bite) | procedural spotted coat; bounded sprint: swift, once per summoning, +30 ft enhancement speed for one round, own brain | complete; internally accepted on the Sprints 3-8 evidence |
| Procedural coats | Tiger, Cheetah (later sprints reuse) | the rig's own geometry | `SummonCoatRasterizer` on the shared visual variant patch | stripes / spots / pale belly from vertex positions; private 512x512 texture; no game pixels read | complete; internally accepted on the Sprints 3-8 evidence |

Placements propagate to the 1d3 / 1d4+1 tiers by construction (6 new
logical placements). Ledger: 15 identities appended and active
(`blueprints/blueprints.json`, entries 2206-2220), pinned by
`tools/validate_expanded_summoning_phase1.py`. Icons: one Blender procedural
render. Tests: four Sprint 8 domain regressions; suite 1799/1799. Runtime
evidence: recorded in the state file as the guarded batches complete.

## 4. Correction order (2026-09-25) - PR #23 correction and requalification

{{REPORT_CORRECTION}}

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
