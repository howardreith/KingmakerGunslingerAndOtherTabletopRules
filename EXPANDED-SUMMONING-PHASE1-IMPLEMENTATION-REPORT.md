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
| Grapple link ownership and mouth ownership (2026-09-26) | every grabber | `UnitPartSummonGrappleLinks` on the holder | limb slot plus target id; mouth occupancy for held and engulfed victims; read-only reconciliation | the establishing limb is owned for the life of the hold and the maintain deals its damage, not the first grab limb's; one target per mouth in the attack roll and in the planned full attack; the cats' rake is made by the maintain, where the tabletop puts it, because the game's initiator part leaves a holder unable to act | complete in session; the post-reload maintain is BLOCKED by the engine (no active grapple survives a save; a unit part is written by type without its contents) with the safe post-load state proven |
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

The 2026-09-25 order on the reviewed head `2569a8eb` (runtime commit
`30a13445`) found the draft's cats, grab sizes, Flytrap, mephit roles,
Cyclops, Web, hooves and visual resources short of the charter and the
rules, and required exact implementation or a BLOCKED mark. Everything was
implemented; nothing is BLOCKED. The corrected mechanics are those in the
sprint tables above (marked "corrected 2026-09-25"); the correction record
in the state file carries the ten items, and the requalification below is
the same record's evidence.

### Requalification on the candidate commit

Candidate commit `145810a5` (`Build-Local.ps1` PASS: 1806 domain tests, 0
failures; package `KingmakerGunslinger-0.0.138-local-runtime.zip` SHA-256
`e0fed6b851898965eda8dc9f1e152d44376bb15a6bd1a735fbf6758b6c63aff1`, DLL SHA-256 `db9daf45b086fbfd9b36e1523edba712bd82141c76e2f98616240fb725d65bcf`). Every gate below ran through the guarded launcher
on that commit, each batch alone on the machine, and the live installation
was restored to 0.0.117 / 136 files / `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` after each batch (the restoration
records under `runtime-evidence/expanded-summoning-restoration`, one per batch of this candidate's set:
`20260926T0835365558996Z-observe-expanded-summoning-inventory.json`, `20260926T0841312237824Z-disposable-expanded-summoning-visual-contracts.json`, `20260926T0900053560660Z-disposable-expanded-summoning.json`, `20260926T0906541254695Z-disposable-expanded-summoning-rules.json`, `20260926T0912450437146Z-disposable-expanded-summoning-visual-lifecycle.json`, `20260926T0925560848494Z-working-save-expanded-summoning-prepare.json`, `20260926T0938214691864Z-working-save-expanded-summoning-creature-review.json`, `20260926T0948091648963Z-working-save-expanded-summoning-creature-review.json`).

| Evidence | Gate | Status | Commit | Assertions |
|---|---|---|---|---|
| `20260926T0830195163604Z-observe-expanded-summoning-inventory` | observe-expanded-summoning-inventory | PASS | 145810a5 | 48 |
| `20260926T0833495384375Z-observe-expanded-summoning-native-donors` | observe-expanded-summoning-native-donors | PASS | 145810a5 | 2 |
| `20260926T0838457786724Z-disposable-expanded-summoning-visual-contracts` | disposable-expanded-summoning-visual-contracts | PASS | 145810a5 | 15 |
| `20260926T0844373783638Z-disposable-expanded-summoning` | disposable-expanded-summoning | PASS | 145810a5 | 17 |
| `20260926T0847527151275Z-disposable-expanded-summoning-player-path` | disposable-expanded-summoning-player-path | PASS | 145810a5 | 10 |
| `20260926T0903112506775Z-disposable-expanded-summoning-rules` | disposable-expanded-summoning-rules | PASS | 145810a5 | 15 |
| `20260926T0910013397653Z-disposable-expanded-summoning-visual-lifecycle` | disposable-expanded-summoning-visual-lifecycle | PASS | 145810a5 | 7 |
| `20260926T0915501597831Z-working-save-expanded-summoning-prepare` | working-save-expanded-summoning-prepare | PASS | 145810a5 | 13 |
| `20260926T0919391993072Z-working-save-expanded-summoning-verify-cleanup` | working-save-expanded-summoning-verify-cleanup | PASS | 145810a5 | 13 |
| `20260926T0923268632578Z-working-save-expanded-summoning-verify-absent` | working-save-expanded-summoning-verify-absent | PASS | 145810a5 | 13 |
| `20260926T0929095873696Z-working-save-expanded-summoning-creature-review` | working-save-expanded-summoning-creature-review (half 1 of 2) | PASS | 145810a5 | 38 |
| `20260926T0941347798912Z-working-save-expanded-summoning-creature-review` | working-save-expanded-summoning-creature-review (half 2 of 2) | PASS | 145810a5 | 24 |
| `20260926T0948097208982Z/20260926T0951509453478Z-disposable-expanded-summoning` | compatibility-mechanical (gunslinger-only; record `20260926T0948097208982Z`, failures=0, liveTreeUnchanged=True) | PASS | 145810a5 | 17 |
| `20260926T0948097208982Z/20260926T0957291887929Z-disposable-expanded-summoning` | compatibility-mechanical (gunslinger-high-risk-combined; record `20260926T0948097208982Z`, failures=0, liveTreeUnchanged=True) | PASS | 145810a5 | 17 |

Live observations carried into the record (each the assertion's own observed
string, clipped):

- Visual resource lifecycle: `loadingGate:framesWaited=39;inProcess=False,screen=False,manual=False,paused=False,mode=Default||baseline:materials=0;textures=0;live=0;donor[mephit_grey_character_d (Instance)/PF/StandardDynamic/asset:mephit_grey_character_d#1441332,rim=RGBA(1, 1, 1, 1),tint=RGBA(1, 1, 1, 1),variantPatch=<none>];donorRim[RGBA(1.3, 1.2, 1.13, 1)]||cycle1:cast=7;dust-mephit=variant:applied,key=dust-mephit,materials=1,slot=_TintColor,rim=1,rimAnimations=0,controller=reinitialized,driven=KMG_SummonVisualVariant (Instance){renderers=1;materials=1;textures=0;released=False},steam-mephit=variant:applied,key=steam-mephit,materials=1,slot=_TintColor,rim=1,rimAnimations=0,controller=reinitialized,driven=KMG_SummonVisualVariant (Instance){renderers=1;materials=1;textures=0;released=False},steam-mephit=variant:applied,key=steam-mephit,materials=1,slot=_TintColor,rim=1,rimAnimations=0,controller=reinitialized,driven=KMG_SummonVisualVariant (Instance){renderers=1;materials=1;textures=0;released=False},steam-mephit=variant:applied,key=steam-mephit,materials=1,slot=_TintColor,rim=1,rimAnimations=0,controller=reinitialized,driven=KMG_SummonVisualVariant (Instance){renderers=1;materials=1;textures=0;released=False},tiger=variant:applied,key=tiger,materials=1,slot=_TintColor,rim=0,rimAnimations=0,coat=1,coat=Stripes,mode=mesh,triangles=3226,size=512,controller=reinitialized,driven=KMG_SummonVisualVariant (Ins...`; failed attach: `outcome=variant:exception:InvalidOperationException;materials=0;textures=0;live=0;native=True`; module sweep: `released=1;swept=0;materials=0;textures=0;live=0;variantOff=True`;
  donor and Pteranodon: `donorSame=True;before[mephit_grey_character_d (Instance)/PF/StandardDynamic/asset:mephit_grey_character_d#1441332,rim=RGBA(1, 1, 1, 1),tint=RGBA(1, 1, 1, 1),variantPatch=<none>];after[mephit_grey_character_d (Instance)/PF/StandardDynamic/asset:mephit_grey_character_d#1441332,rim=RGBA(1, 1, 1, 1),tint=RGBA(1, 1, 1, 1),variantPatch=<none>];rimBefore[RGBA(1.3, 1.2, 1.13, 1)];rimAfter[RGBA(2.2, 1.8...`.
- Cats: `leopard:classification=True;rakeRefused=True;clawGrabbed=False(expected False);biteGrabbed=True;singleAttackClean=True;sameTurnPlan=dropped(3/0);sequence[held=kept(5/2),other=dropped(2/0),charge=kept(5/2),ordinary=dropped(2/0)];released=True;lion:classification=True;rakeRefused=True;clawGrabbed=False(expected False);biteGrabbed=True;singleAttackClean=True;sameTurnPlan=dropped(3/0);sequence[held=kept(5/2),other=dropped(2/0),charge=kept(5/2),ordinary=dropped(2/0)];released=True;dire-lion:classification=True;rakeRefused=True;clawGrabbed=False(expected False);biteGrabbed=True;singleAttackClean=True;sameTurnPlan=dropped(3/0);sequence[held=kept(5/2),other=dropped(2/0),charge=kept(5/2),ordinary=dropped(2/0)];released=True;tiger:classification=True;rakeRefused=True;clawGrabbed=True(expected True);biteGrabbed=True;singleAttackClean=True;sameTurnPlan=dropped(3/0);sequence[held=kept(5/2),other=d...`.
- Grab sizes and the worm: `grizzly-bear:size=Large;Medium:grabbed=True;maintainCmb=115;tripCmb=106,Large:grabbed=True;maintainCmb=115;tripCmb=106,Huge:grabbed=False;freeGrappleCmb=110;freeTripCmb=106;grabBonus=True;maneuverImmuneRefused=True;takenOnceImmunityEnds=True;leopard:size=Medium;Small:grabbed=True;maintainCmb=112;tripCmb=103,Medium:grabbed=True;maintainCmb=112;tripCmb=103,Large:grabbed=False;freeGrappleCmb=107;freeTripCmb=103;grabBonus=True;maneuverImmuneRefused=True;takenOnceImmunityEnds=True;worm:gargantuanGrabbed=True;swallowSizeRefused=True;eligible=True;swallowed=False;stillHeld=True;damage=0->94;notSwallowed=True;colossalRefused=True;hugeSwallowAllowed=True`.
- Flytrap: `placement:hostile=dir1@2.5m,wolf0=dir2@2.5m,wolf1=dir3@2.5m,wolf2=dir4@2.5m,wolf3=dir5@2.5m;links:fourBites=True;link0=True;link1=True;busyBiteRefused=True;link2=True;link3=True;fifthRefused=True;held=4;hostile[holder=True,bite=0];wolf0[holder=True,bite=1];wolf1[holder=True,bite=2];wolf2[holder=True,bite=3];distinct=True;pathing[error=False;points=2;obstacles=PathClear;heldStanding=4;from=(13.13,-6.00,-16.82);to=(25.13,-6.00,-4.82);crowd=(19.13,-6.00,-10.82)];release:escaped=True;swept=2;sweptFree=True;reach:reach=True,los=True,dist=2.50,conscious=True;engulf:stillHeldAfterOwnRound=True;eligible=True;largeMaintained=True;heldAfterLarge=1;engulfed=True;engulfTickDamage=True;heldAfterEngulf=0;multiHoldAfterEngulf=False;hostile=cantMove=True;entangled=False;cantAct=True;ending:relinked=True;holdEnded=True;spatOut=True;regrabbed=True;disposalReleased=True`.
- Mephit roles and the ally-safe cloud: `chillMetal:noMetalUntargetable=True;armored[armor=ChainmailType;metalArmor=True;metalWeapon=False;tier=2];targetable=True;chilled=True;ticks=1,3,3,5,3,0;outcomes=round=2,tier=2,dice=1d4,minimal=1,damage=1/round=3,tier=2,dice=2d4,minimal=2,damage=3/round=4,tier=2,dice=2d4,minimal=2,damage=3/round=5,tier=2,dice=2d4,minimal=2,damage=5/round=6,tier=2,dice=1d4,minimal=1,damage=3/round=7,tier=2,dice=0d4,minimal=0,damage=0;fullTable=True;armed[armor=none;metalArmor=False;metalWeapon=True;tier=1];minimalTicks=1,2,2,2,1,0;minimalTable=True;noMetalAfter=True;execution=ability=KMG_Summoning_Special_IceMephit_SpellLikeTwo;targetable=True;approach=True;endedAfterTicks=True;detached=False;finished=True;result=Success;magma:blinded=True;alliesSighted=True;blindSeconds=30;pooled=True;speed=40->10->40;cannotAttack=True;attackInterrupted=True;abilitiesWork=True;slash18InForm=0;slash18OutOfForm=10(DR 5/magic and the game's difficulty scaling);attacksAgain=True;execution=ability=KMG_Summoning_Special_MagmaMephit_SpellLikeTwo;targetable=True;approach=True;endedAfterTicks=True;detached=False;finished=True;result=Success;glitterdust:blindBefore=False;hostileBlinded=True;partyCasterSighted=True;alliedS...`; wind wall: `windWall:area=True;inside=3;sheltered=True;bow[HuntersBlessingLongbowItem:type=Ranged,autoMiss=True,missChance=0,hit=False];thrown[FrozenCrescentItem:type=Ranged,autoMiss=False,missChance=30,hit=True];melee[LongswordPlus1:type=Melee,autoMiss=False,missChance=0,hit=True];ray:type=RangedTouch,autoMiss=False,missChance=0,hit=True;outcomes=56098bbd-8a40-4195-99d2-5f9452a7466f:Deflected:Longbow:missChance=0/56098bbd-8a40-4195-99d2-5f9452a7466f:MissChance:ThrowingAxe:missChance=30;ended=True;shelter[component=True;componentBuffIsState=True;areaCaster=KMG_Summoning_Unit_DustMephit;mephitPlayerFaction=False;caster[hasState=True,sourced=KMG_Summoning_Special_DustMephit_WindWallState,allyOfMephit=F...`; stinking cloud: `cloud:area=True;inside=4(KMG_Summoning_Unit_Wolf,KMG_Runtime_ExpandedSummoning_HostileTarget,KMG_Runtime_ExpandedSummoning_RulesCaster,KMG_Summoning_Unit_OozeMephit);hostileInside=True;alliesInside=True;hostileNauseated=True;partyCasterClean=True;alliedSummonClean=True;mephitClean=True;gameFramesInside=4;areaTick=ok;insideAfterTick=4;gates[view=True;areaPos=(27.90,-6.00,-12.33);viewPos=(27.90,-6.00,-12.33);shape=ScriptZoneCylinder(radius=6.10,height=100.00,centre=(0.00,0.00,0.00));onUnit=Fals...`.
- Cyclops: `ac:target=17;tabletop=19;difficulty=-2;modifiers=DexterityBonus:-1,Size:-1,NaturalArmor:7,Armor:4,Difficulty:-2;armorFact=True;natural7=True;noArmorItem=True;dex=8;size=Large;flash:uses=1->0;armed=True;untimedArming=True;saveWhileArmed=5;saveAfterSpentSameSeed=5;stillArmedAfterSave=True;armedNatural=20;hit=True;threat=True;confirmationRoll=1;confirmed=False;autoFlags=False;spent=True;nextNatural=1;nextMiss=True;secondUseAvailable=False`; across the save and the reload: `uses=0;available=False;armedStates=1;untimed=True;reloadedNatural=20;hit=True;spent=True;nextNatural=1;nextMiss=True`.
- Web: `web:spiderPos=(25.84,-6.00,-14.09);hostileSpot=dir0@3m;casterSpot=dir1@3m;web:spiderSize=Medium;oneLargerAllowed=True;twoLargerRefused=True;highTouchAc=114;usesBefore=2;webA:ended=True;frames=13;webB:ended=True;frames=8;rollA=type=RangedTouch,natural=10,bonus=5,targetAc=114,hit=False,weapon=RayItem,target=KMG_Runtime_ExpandedSummoning_HostileTarget,autoMiss=False,logged=False;missedHighTouch=True;lowTouchAc=5;rollB=type=RangedTouch,natural=10,bonus=5,targetAc=5,hit=True,weapon=RayItem,target=KMG_Runtime_ExpandedSummoning_HostileTarget,autoMiss=False,logged=False;webbedLowTouch=True;uses=2->1->0;thirdUseAvailable=False;stillWebbedHopeless=True;brokeFreeOverwhelming=True;immunityFact=True;d...`.
- Hooves: `pony:strengthBonus=1;hooves=2;flaggedAtSpawn=True;PrimaryHand:attack=-3(primary 2),damageBonus=0(primary 1);Additional[0]:attack=-3(primary 2),damageBonus=0(primary 1);fullAttack=2;horse:strengthBonus=3;hooves=2;flaggedAtSpawn=True;PrimaryHand:attack=-2(primary 3),damageBonus=1(primary 4, the game's own one-and-a-half);Additional[0]:attack=-2(primary 3),damageBonus=1(primary 3);fullAttack=2`.

Eleven guarded shake-out runs preceded the candidate (commits `815ddd2f`,
`1d247e1b`, `c9313ce9`, `7f199c48`, `927a3ae6`, `9631b4b8`, `0f13ca40`,
`11e1d7ca`, `ceacab65`, `c6eb242a`, `0ab74226`); each found a code,
fixture or record defect that the next commit closed, and the journal
keeps their findings: the area-effect shell factory, the live grab
component, the docile hooves at body initialization, the held state's
round component, the fixture's placement in the open and in the area's
spatial grid, the wall keyed on the enemy relation (a summon's own ally
test excludes the party it fights for), the scenarios waiting out the
game's loading screen, the inventory gate's exactness following the
corrected blueprints, the natural 1 and 20 on the summon grapple's checks
(the engine's maneuver rule decides by the sum alone), the Flash of
Insight arming that lasts until the attack (a one-round state lapsed
between the save and the reloaded attack), three creature records still
written as the reviewed draft had them, and the grab that would have
taken a foe immune to combat maneuvers (the engine's rule returns before
it computes CMB and CMD there, and the verdict it leaves behind reads as
a success).

Reviewer findings and closure, 2026-09-25 order: items 1-7 implemented
exactly and proven live above; item 8 (documents) audited; item 9 (this
requalification) on the one candidate commit; item 10 kept (draft PR #23,
unmerged).

Reviewer findings and closure, 2026-09-26 order: item 1 (grapple attack
identity) is owned per link with mouth occupancy and proven live for a
bite hold, a foreclaw hold and four flytrap mouths; its post-reload
maintain is BLOCKED by the engine, which carries no active grapple across
a save and writes a unit part by type without its contents, with the safe
post-load state proven instead. Item 2 (held-target rake) is made by the
maintain check the tabletop puts it in, because the game's own initiator
part leaves a holder unable to act: two genuine rake attacks against the
exact held foe, logged, never the turn the hold was taken and never
against another unit, with the charge rake proven by genuine execution and
absent from an ordinary full attack. Item 3 (mouth ownership and engulf) is
one target per mouth for held and engulfed victims, in the attack roll and
in the planned full attack, with the game refusing the attack outright
when every mouth is shut, exactly one mouth freed per release, and the
engulf dealing the stat block's 1d8+7 and 2d6 acid each round on an
audited cadence. Item 4 (Web projectile) pins one exact identity the donor
audit established, the library carrying 170 projectiles and no web among
them. Item 5 (evidence) is this gate list. Nothing was reclassified as an
accepted deviation. Internal acceptance on this evidence; HumanReview:
NOT_PERFORMED_NONBLOCKING.
OwnerDelegationGranted.
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
