# Expanded Summoning Phase 1 (charter Sprints 3-8) - autonomous state

Durable state for the Sprints 3-8 mission under the owner's order of
2026-09-24 ("Finalize Expanded Summoning Phase 0, Then Complete Sprints 3-8").
Read this first; it is the record the journal and the report summarise.

## Authority

- Order activated 2026-09-24 after the owner's manual review of the Phase 0
  Pteranodon candidate passed (revision unspecified). OwnerDelegationGranted
  for intermediate technical, menu/usability and visual acceptance; internal
  engineering, evidence and visual review between sprints; no routine
  confirmations, keyboard assistance, art choices or permission requests.
- Labels: OwnerDelegationGranted; InternalAcceptance per item; HumanReview:
  NOT_PERFORMED_NONBLOCKING for every piece of new work here.
- Hard limits: never write valued campaigns or protected test baselines
  (`KMG_AUTOMATION_BASELINE` is never selected or modified); no force-push,
  reset/clean of foreign work, credential or account changes, OS/game/Unity
  upgrades, purchases, or new paid fallbacks; proprietary captures stay in
  permitted local evidence locations; no release, deployment or Sprint 9.
  This PR stays a draft and is never merged under this order.

## Branch and baseline

- Phase 0 merged into `master` as `b0641a58` (PR #21 at reviewed head
  `ba5e20a7`, 2026-09-24T16:55:50Z). This branch,
  `codex/expanded-summoning-phase1-sprints3-8`, was created from that commit in
  its own worktree (`.worktrees/expanded-summoning-phase1`); the only draft PR
  for Sprints 3-8 is opened from it.
- Live installation must end at 0.0.117 / 136 files /
  `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` after
  every guarded run; the restoration records verify it.
- Integrated master verification on `b0641a58`, from this fresh worktree:
  `Build-Local.ps1` PASS (1774 domain tests, 0 failures; package
  `KingmakerGunslinger-0.0.138-local-runtime.zip` SHA-256
  `056d19ea274f478d07d3c152e017db5054432c80fd60b210ebbfb54606ad7e9e`, DLL
  `05ab10e50806f518fd2ad9e6535c211ae485530b4b09e61865ea9367c0ecd1e4`);
  guarded smoke `20260924T1710306893099Z-observe-expanded-summoning-inventory`
  PASS 38/38 on `b0641a58`, live tree restored and verified
  (`20260924T1714020506347Z`). Two local, git-ignored prerequisites had to be
  carried into the fresh worktree before the tree built: `GamePath.props`
  (the private extracted-reference paths) and `artifacts/inspection/`
  (the IL inspection the bodyguard domain tests read). Neither is part of
  the tree; the first two verification attempts failed on their absence, not
  on the merge.

## Scope (25 creature work items)

| Sprint | Creatures (SM / SNA) | Signature |
|---|---|---|
| 3 | Pony I/I, Horse II/II, Owlbear -/IV, Cyclops -/V, Frost Giant VIII(retained wrapper)/VII (reuse unit) | native publication; Flash of Insight bounded |
| 4 | Shambling Mound -/VI, Giant Flytrap -/VII, Purple Worm -/VIII | grab/constrict/swallow on shared grapple infrastructure |
| 5 | Dust, Ice, Magma, Ooze, Salt, Steam Mephits IV/IV | distinct visuals, icons, breath, SLAs; no Lightning Mephit |
| 6 | Monitor Lizard, Grizzly Bear, Dire Bear (grab); Giant Spider (ranged Web, tremorsense, bounded climb only if feasible); Pixie (sleep arrow, dance) | signature mechanics repair |
| 7 | Leopard, Lion (+visual), Dire Lion, Dire Tiger/Smilodon | pounce / grab / charge-only rake |
| 8 | Tiger -/IV new; Cheetah authentic visual + bounded per-summon sprint | cat family completion |

Placements propagate to the 1d3 / 1d4+1 tiers. No feats, aquatic, mounts,
familiars, variant elementals or later-sprint creatures; Dire Bat stays
hidden; Eagle and Roc are not redesigned. Identities are reused, the ledger is
append-only, stable identities register even when publication is disabled,
and no future roster data is published.

## Sprint ledger

| Sprint | State | Evidence |
|---|---|---|
| 3 | IMPLEMENTED; RUNTIME QUALIFICATION IN PROGRESS | see "Sprint 3 record" |
| 4 | IMPLEMENTED; RUNTIME QUALIFICATION IN PROGRESS | see "Sprint 4 record" |
| 5 | IMPLEMENTED; RUNTIME QUALIFICATION IN PROGRESS | see "Sprint 5 record" |
| 6 | IMPLEMENTED; RUNTIME QUALIFICATION IN PROGRESS | see "Sprint 6 record" |
| 7 | IMPLEMENTED; RUNTIME QUALIFICATION IN PROGRESS | see "Sprint 7 record" |
| 8 | IMPLEMENTED; RUNTIME QUALIFICATION IN PROGRESS | see "Sprint 8 record" |

## Sprint 3 record - Native Publication Pack I

Donor audit: `observe-expanded-summoning-native-donors`, evidence
`20260924T1724064386340Z-observe-expanded-summoning-native-donors` (PASS;
292 units, 76 classes, 603 facts, 255 abilities, 65 buffs, the native Grab
graph), restoration `20260924T1725555974441Z`. Its `native-donor-audit.json`
chose every donor and GUID below; nothing was guessed.

Decisions (recorded here rather than asked):

- Pony (SM I templated, SNA I) and Horse (SM II templated, SNA II) reuse
  Kingmaker's own dedicated summoned units `PonySummoned`
  `3f95557fc806db741b500a5735990841` and `HorseSummoned`
  `5bb9579fdb2b26b48bb10d61c81cfdfb` as donors; the profiles are the tabletop
  stat blocks (Appendix A) on the animal class, both hooves as primary limbs
  exactly as those native summons carry them. Endurance and Run are omitted
  (no proven final-live feature identity).
- Owlbear (SNA IV) is the first magical-beast chassis: `CR8_OwlbearStandard`
  `d6e0acbdbdb56114898922063ae2cba0` as a sanitized body donor, 5 HD on
  `MagicalBeastClass` `b9e97f47cb86f2d45a0784a096ff8037`, natural armor +5
  (`7661741dbb9604842a642457456fd0e4`), bite 1d6 and two 1d6 claws, Improved
  Initiative, Great Fortitude, Skill Focus (Perception), reduced reach. Claw
  grab is deferred to the shared grapple lifecycle Sprint 4 introduces and is
  recorded as a deviation until then.
- Cyclops (SNA V) is the first humanoid chassis: `CR5_CyclopStandard`
  `124f1c45ef24d654e9cd420fe84f7f36` as a sanitized body donor, 10 HD on
  `HumanoidClass` `6ab4526f94d2e3e439af0599a29b6675`, natural armor +7
  (`e73864391ccf0894997928443a29d755`), the native `StandardGreataxe`
  `6efea466862f014469cec6c3f2b85cb7` in hand (the game scales it to the
  Large 3d6), Ferocity, Power Attack and Cleave
  (`d809b6c4ff2aaff4fa70d712a70f7d7b`). Flash of Insight is bounded: once
  per summoning, as a swift action, the next attack roll in the round is an
  automatic hit and critical threat (`CyclopsFlashOfInsightComponent` on a
  one-round state that the native `RemoveBuffOnAttack` ends after one
  attack); the confirmation roll stays ordinary; a small brain spends it
  when the cyclops fights and the player may spend it from the action bar.
  Hide armor and the heavy crossbow are omitted (no equipment beyond the
  weapon); Alertness, Great Cleave and Improved Bull Rush are omitted (no
  proven identity).
- Frost Giant under Summon Nature's Ally VII, VIII and IX reuses the retained
  native unit `590cd3d5e76fdc649a5f97bc984cd3c4` through three creature-named
  wrappers carved from the native Mastodon options
  (`6d8d59aa38713be4fa3be76c19107cc0`, `256739c1e61e3f64eaf71734d271f4be`,
  `9bd8cb6180842f44e9302c58e47b91f0`, which the publisher already reconciles
  to the KMG Mastodon). The native option builder gained one bounded
  device for this: when a wrapper's spec says so, the cloned umbrella's single
  direct spawn is pointed at the retained unit, keeping the umbrella's count,
  duration, pool and post-spawn actions. No second Frost Giant identity
  exists; coverage now reports it Published in both families.
- Icons: this environment has no image-generation capability, so the four
  new concepts are project-owned procedural renders from Blender 4.5
  (`assets-source/original-icons/expanded-summoning/tools/render_creature_icon.py`;
  metaballs and primitives, procedural materials, warm key/cool rim/low fill,
  radial backdrop, dark bronze ring, 1254 px sources exported to 128 px by
  the existing tool). They were reviewed at 128 px in this session: each
  reads as its creature; they are plainer than the 77 painterly concepts and
  are recorded as such. No game pixels, downloaded model or texture is an
  input.
- Pins moved with the roster: 71 creatures, SM 68/378, SNA 61/348, 726
  logical placements (14 suppressed, 712 published), 199 templated, 30
  natural profiles, 81 icons, 29 wrappers (24 distinct sources), foundation
  identities 1276; the ledger gained exactly 92 appended, active identities
  (4 units, 45 placements, 34 template executions, 3 wrappers, 6 Cyclops
  specials) allocated by `tools/expanded_summoning_manifest.py --allocate`
  and activated; visible choices 693 -> 741 (SM 388, SNA 353); package
  237 -> 241 files. The static validation chain
  (`tools/validate_expanded_summoning_phase1.py`, wired from the 0.0.138
  validator) pins the append and the Sprint 3 figures; the runtime runner
  derives every roster pin from the catalogs instead of literals.
- New scenario `working-save-expanded-summoning-creature-review`
  (`creatures=<keys>`): casts the named creatures one at a time through the
  real parent chain into the working save and renders each from the party
  camera idle, moving and attacking, using the Phase 0 motion review
  generalised away from the Pteranodon. The save is read, never written.
  It is the standing internal-review instrument for every visual of
  Sprints 3-8.
- Domain suite: 1779 cases (five Sprint 3 regressions in
  `ExpandedSummoningSprint3Tests`), all passing; repository validation PASS.

Runtime qualification (guarded, live installation restored after each
batch): recorded below as it completes.

## Sprint 4 record - Native Publication Pack II (plants and the worm)

Deep donor audit: `observe-expanded-summoning-native-donors` re-run with the
full component graphs of the grab, constrict, swallow, poison, web, breath,
sleep and pounce facts ten levels deep (evidence
`20260924T1942171737899Z-observe-expanded-summoning-native-donors`, PASS;
296 units, 76 classes, 603 facts, 256 abilities, 66 buffs, 50 deep graphs).
The game's grapple and swallow runtime was read from its own assembly
(IL listing, no decompiled source committed) before anything was designed:

- `ContextActionGrapple` initialises `UnitPartGrappleInitiator` (adds the
  caster buff plus the CantAct and CantMove conditions) and
  `UnitPartGrappleTarget` (adds the target buff plus CantMove);
  `UnitGrappleController` releases the target when the initiator is gone,
  no longer names it or is unconscious, or when the target's once-per-round
  `UnitHelper.TryBreakFree` succeeds, and releases the initiator when the
  target is no longer grappled, unconscious or out of reach. Removing a part
  removes its buff and conditions; disposing a unit only unsubscribes its
  parts.
- `UnitPartSwallowWhole.Swallow` initialises `UnitPartSwallowed` on the
  target (untargetable, CantAct, CantMove, the target buff, a break-free
  attempt every six seconds); the part spits everyone out on the swallower's
  death and, because `EntityDestructionController` raises unit destruction
  before disposal, on its destruction too.
- `RuleCombatManeuver` rolls the caster's CMB (with `ManeuverBonus`
  components) against the target's CMD; `RuleAttackRoll`'s automatic-hit
  path never rolls the d20 (the Flash of Insight correction below).
- Native Shambling Mound grab: slam hit -> grapple maneuver -> caster buff
  `ShamblingMoundGrappledCantAttack` (CanNotAttack), target buff
  `ShamblingMoundGrappledBuff` (Entangled, CantMove, 4d6+Strength per
  round, per-round break-free) plus 2d6+5 on the grab. Native Purple Worm:
  bite hit -> grapple maneuver -> `ContextActionSwallowWhole` with
  `PurpleWormSwallowed` (4d8+12 per round, CMB -2, Dex -4).

Decisions (recorded here rather than asked):

- The shared summon grapple lifecycle is project-owned code over the native
  parts, never a copy of the mound's graph (whose constrict and target
  state are mound-specific): `SummonGrabComponent` on each grabber's
  combat-traits buff attempts the game's own grapple check after a hit with
  a listed grab weapon (tabletop +4 through the native `ManeuverBonus`),
  refuses while the summon holds or has swallowed anyone, while the target
  is held or swallowed, and against itself; success initialises the native
  parts with the project's shared `Grapple.Hold` (holder) and
  `Grapple.Grappled` (target, Entangled) buffs and deals any constrict.
  `SummonHoldComponent` on the hold buff maintains each new round with a
  grapple check at the tabletop +5, dealing the grab weapon's damage plus
  Strength (plus constrict) on success and releasing on failure, and - the
  link ownership the charter asks for - releases exactly the target the
  summon's own initiator part names whenever the hold buff turns off, for
  any reason (escape, the holder falling, dispel, the summon's disposal),
  never touching the initiator part itself (the game's controller drops it,
  and removing it from inside its own buff's removal would re-enter). The
  worm swallows through the native part on a successful grab, as
  Kingmaker's own worm does; `SummonSwallowLifecycleComponent` on its
  traits spits everyone out when the traits turn off. The tabletop
  grab-then-maintain-then-swallow sequence has no native hold-and-swallow
  path (a holding initiator cannot act) and is recorded as a deviation.
- `SummonGrappleAreaSafeguard` (subscribed once at load beside the
  alignment-mode runtime) releases party members held or swallowed by a
  KMG summon when the party leaves an area and repairs any party member
  whose native hold or swallow points at a unit that no longer exists when
  an area finishes loading. Without it a summoned worm's swallow could
  follow a companion into the next area with the worm left behind.
- Shambling Mound (SNA VI): `CR6_ShamblingMound`
  `b98ae409beb5e8543a75b82ecda082a7` as a sanitized body donor, 9 HD on
  `PlantClass` `9393cc36ea29d084bab7433e3a28d40b` (the plant traits ride
  the class progression), natural armor +10 (`4179c5c08d606a6439a62bf178b738e1`),
  two native `SlamGargantuan2d6` slams (`27eee74857c42db499b3a6b20cfa6211`),
  fire resistance 10, electricity immunity, Power Attack, Iron Will,
  Lightning Reflexes, Cleave, Weapon Focus (slam); slam grab and constrict
  2d6+7 (1.5 x Strength) on the shared lifecycle. Electric Fortitude's
  Constitution gain, swim and the native poison aura are omitted.
- Giant Flytrap (SNA VII): `CR10_GiantFlytrapStandard`
  `fb824352b7968fb4d8103ac439644633` as a sanitized body donor, 13 HD plant,
  Huge, tabletop 25/18/25/1/12/6 (the native unit's 29 Strength is not
  carried), speed 10, natural armor +10, four native `BiteLarge1d8`
  (`ec35ef997ed5a984280e1a6d87ae80a8`), acid resistance 20, the native
  60-foot `Blindsight` (`236ec7f226d3d784884f066aa4be1570`) standing in for
  tremorsense, trip immunity, Cleave, Great Fortitude, Improved Initiative,
  Power Attack, Skill Focus (Stealth), Weapon Focus (bite); bite grab on the
  shared lifecycle, one held target at a time (the native initiator part
  holds one). Engulf and Vital Strike are omitted.
- Purple Worm (SNA VIII): the native dedicated `PurpleWormSummoned`
  `bf2216f48b3f4d24c9c502007649340d` as donor, rebuilt on the natural
  builder as a 16 HD magical beast, Gargantuan, 35/6/25/1/8/8, speed 20,
  natural armor +22 (`eee672c8f6555b445a89dbbb91361d64`), the native
  `PurpleWormBite` (`7e4b9b41a9358264d9e3c69c183ca0a2`) and
  `PurpleWormSting` (`287cd06241fdaf8408410b226f744093`), the exact native
  `PurpleWormPoisonFeature` (`728446b9d0bf47144a1b621169299c2a`,
  Constitution-scaled sting poison), trip immunity, Critical Focus,
  Improved Critical (bite), Power Attack, Weapon Focus (bite); bite grab
  swallows whole, with the swallowed state cloned component-for-component
  from the native `PurpleWormSwallowed` (`368d1df7c1d0267459a584bf23ccadc8`).
  The native summoned worm's burrowing kit and brain are not carried
  (bounded combat adaptation, charter Sprint 4); Awesome Blow, Improved
  Bull Rush, Staggering Critical and Weapon Focus (sting) are omitted.
- Owlbear: the claw grab deferred in Sprint 3 now rides the shared
  lifecycle (`Owlbear.CombatTraits` with the native `ClawLarge1d6`).
- Flash of Insight correction (round-2 mechanical evidence
  `20260924T2023157894340Z-disposable-expanded-summoning`): with AutoHit
  set, `RuleAttackRoll` takes its automatic-hit path, never rolls the d20 and
  decides the critical only from AutoCriticalThreat and
  AutoCriticalConfirmation together, so the armed natural 1 hit with no
  threat. A threat with an ordinary confirmation cannot be expressed on that
  path; the bounded Flash of Insight is now an automatic critical hit (hit,
  threat and confirmation together), still once per summoning and for one
  attack. Every description, the profile deviation, the manifest note, the
  static record, the runner's check and the Sprint 3 test say so.
- Creature review instrument correction: its first run
  (`20260924T2013288609426Z`) failed inside the shared spawn helper with
  "observed 0". The request writer had dropped the `creatures` parameter
  (fixed: `New-KmgRuntimeRequest` carries it); the helper now also counts
  the units that appeared in the caster's area as a second witness, reports
  the executor's record when neither witness agrees, removes only its own
  postfix from the summon rule (it had been stripping the mod's same-turn
  activation postfix), and the review casts in the same guarded update as
  its setup, as the persistence prepare does.
- Icons: three more Blender procedural renders (mound, flytrap, worm),
  reviewed at 128 px in this session and recorded as plainer than the
  painted set; the render script carries their builders.
- Pins moved with the roster: 74 creatures, SM 68/378, SNA 64/357, 735
  logical placements (14 suppressed, 721 published), 199 templated, 33
  natural profiles (plant added), 84 icons, 29 wrappers, foundation
  identities 1295; the ledger gained exactly 19 appended, active identities
  (3 units, 9 placements, 7 grapple specials; entries 2049-2067) allocated
  by `tools/expanded_summoning_manifest.py --allocate` and activated;
  visible choices 741 -> 750 (SNA 362); package 241 -> 244 files. The Phase
  1 validator pins the whole 111-identity append and the current figures.
- Domain suite: 1783 cases (four Sprint 4 regressions in
  `ExpandedSummoningSprint4Tests`), all passing; repository validation PASS.
- Persistence fixture: the Shambling Mound and the Purple Worm join it (nine
  units), so the Large plant and the Gargantuan worm footprints ride the
  save/load and module-disabled legs.

Runtime qualification (guarded, live installation restored after each
batch): recorded below as it completes.

## Sprint 5 record - Mephit Family Expansion

Inputs: the deep native-donor audits (`20260924T2148497908566Z` and
`20260924T2214377611148Z`, PASS, 79 named graphs) gave the four native
summoned mephits' exact facts, breaths, spell-like abilities and brains, the
sickened condition, the energy vulnerabilities and immunities, the subtype
features and the three native breath cone projectiles; the game's own
component fields were read from its assembly (IL listing, no decompiled
source committed). The tabletop mephit family was checked against the public
SRD stat blocks recorded in the implementation report's appendix.

Decisions (recorded here rather than asked):

- Donors: each variant rides the nearest native summoned mephit as its
  body - Dust and Salt on the air mephit (`50782bc4eb36aac4287023e20ee00808`),
  Ice, Ooze and Steam on the water mephit
  (`4615328295cd7e84bb2ef09d3dba8403`), Magma on the fire mephit
  (`10a820de0a417f345866f794324205ad`). The pale air and water rigs take
  the light tints (a tint can only darken or shift a material); the earth
  and fire subtypes the Salt and Magma mephits need are restored as facts.
  No third-party Lightning Mephit exists anywhere in the catalogs (charter
  out-of-scope line), and the domain suite and the Phase 1 validator both
  refuse one.
- Element facts: the donor's subtype, immunities, breath and spell-like
  abilities are dropped (`MephitDonorElementFactGuids`); the variant's own
  are added: Dust air; Ice air, cold immunity, fire vulnerability; Magma
  earth and fire (the native fire subtype itself carries fire immunity and
  cold vulnerability); Ooze water; Salt earth; Steam fire and water. Damage
  reduction 5/magic, fast healing 2, natural armor, Dodge, Improved
  Initiative, the two native claws and the 3 HD outsider chassis stay.
- Breath: the donor's 15-foot cone (delivery, Reflex save, descriptor,
  Constitution-based DC 10 + 2 + Con) rebuilt with the tabletop energy and
  dice - Dust and Salt 1d4 slashing, Ice 1d4 cold, Magma 1d8 fire, Ooze 1d4
  acid, Steam 1d4 fire - with the native cold, acid or fire cone visual, the
  tabletop sickening rider (3 rounds on a failed save; Ooze: a Reflex save
  negates damage and sickening together; Magma has no rider), and its whole
  effect inside an enemies-of-the-caster conditional. That last clause is
  the charter's "no mephit uses harmful area effects without ally-safe
  targeting" applied to the breath: allies standing in the cone take
  nothing.
- Spell-like abilities, one use per summoning each (a named resource; the
  tabletop once-per-hour and once-per-day both exceed a summoning): Dust
  blur (the native mephit blur); Ice magic missile; Ooze acid arrow and
  stinking cloud (the native mephit cloud); Salt glitterdust and dehydrate;
  Steam blur and boiling rain. Dehydrate and boiling rain are project
  bursts centred on the mephit (20 feet, enemies only by targeting,
  Fortitude half, 2d8 and 2d6 fire). Wind wall, chill metal, pyrotechnics
  and magma form have no native spell or form and are omitted. Spell-list
  memberships are not carried on the clones.
- Brains: a project brain per variant with one cast action per ability (the
  breath on a four-round cooldown so the mephit also claws; the one-use
  abilities without), the shape the Cyclops and Pixie actions already have.
- Visuals: `ExpandedSummoningVisualVariantPatch` (a postfix on the view's
  data attach) clones the view's renderer materials, tints the colour slot
  by the variant's profile and, for Magma and Steam, sets an emission
  glow; the clone is private to the view, the donor's shared material is
  never written, and a view with no renderer or no colour slot is left
  native with the reason recorded. Tint profiles are plain numbers in the
  special profiles so the domain suite pins them; the runtime fixture and
  the creature review read the applied outcome per view.
- Icons: six Blender procedural renders from one parametrized mephit builder
  (a small horned, bat-winged bust breathing) in six elemental dressings.
- Pins moved with the roster: 80 creatures, SM 74/414, SNA 70/393, 807
  logical placements (14 suppressed, 793 published), 199 templated, 90
  icons, foundation identities 1421; the ledger gained exactly 126
  appended, active identities (6 units, 72 placements, 48 specials; entries
  2068-2193) allocated by `tools/expanded_summoning_manifest.py --allocate`
  and activated; visible choices 750 -> 822; package 244 -> 250 files. The
  Phase 1 validator pins the whole 237-identity append and the current
  figures.
- Domain suite: 1787 cases (four Sprint 5 regressions in
  `ExpandedSummoningSprint5Tests`), all passing; repository validation PASS.
- Runtime fixture: the inventory checks every variant part for part
  (`expanded-summoning-sprint-five-mephits`); the mechanical scenario
  breathes with the Steam Mephit (damage and the sickening rider on the
  hostile, the party caster untouched) and casts the Salt Mephit's dehydrate
  (the hostile damaged, the caster and the summons untouched, the one use
  spent); the persistence fixture gains the Steam Mephit (ten units); the
  creature review records the applied visual variant per reviewed view.
- Grapple fixture correction (mechanical evidence
  `20260924T2220326799940Z-disposable-expanded-summoning`): every link step
  of the Sprint 4 live case passed, but the holder-free and safeguard checks
  looked at CantAct/CantMove while the native `SummonedUnitAppearBuff` (which
  holds a fresh summon still while it materialises) was still on the mound.
  The fixture now removes that appearance buff before the case and records
  the holder's condition baseline; the lifecycle code did not change.

Runtime qualification (guarded, live installation restored after each
batch): recorded below as it completes.

## Sprint 6 record - Existing Signature Mechanics Repair

Inputs: the deep native-donor audit `20260924T1942171737899Z` (the native
`Web` spell, `WebGrappled`, `WebBuffSlowMovement`, `SpiderWebImmunity`,
the native `Pounce`, `TrippingBite` and `MonitorLizardPoisonFeature`
graphs) and the round-4 audit's named-graph resolution, which showed the
names that do not exist in the game (there is no native tremorsense
feature a summon can carry, no native cat rake feature, no native cheetah
sprint); the game's ability blueprint fields (custom range) and the attack
rule's charge flag were read from its assembly (IL listing, nothing
decompiled committed).

Decisions (recorded here rather than asked):

- Grab: the Monitor Lizard (bite `c988aa874d11ff84d873508ddc9b928f`),
  Grizzly Bear and Dire Bear (claws `c76f72a862d168d44838206524366e1c`)
  gain `SummonGrabComponent` carriers on the shared summon grapple
  lifecycle exactly as the Owlbear did - the native generic grab graph is
  the Shambling Mound's (constrict, mound target state) and stays unused,
  which is the charter's "without importing unrelated donor constrict
  logic". Only the mound passes constrict dice to the grabber; the domain
  suite pins that.
- Giant Spider: tremorsense is the native 60-foot blindsight
  (`236ec7f226d3d784884f066aa4be1570`, the flytrap's precedent; the game's
  own `Tremorsense` feature is a kineticist talent with class
  prerequisites), plus the native `SpiderWebImmunity`
  (`3051e7002c803fc47a11bcfa381b9fbd`) so the spider ignores webs. Web is
  a bounded ranged extraordinary ability on the special builder: 50 feet
  (custom range), one foe, Reflex DC 10 + half hit dice + Constitution
  (the native spider poison's scaling), the native `WebGrappled` state
  (`a719abac0ea0ce346b401060754cc1c0`: entangled, cannot move, per-round
  break-free) for at most ten rounds, two uses per summoning on a named
  resource (the tabletop four per day exceeds a summoning), one cast action
  on its own brain, the spider's summon icon. Climb is omitted: no
  save-safe seam exists for a summon's climb movement.
- Pixie: verified, not changed. Sixteen sleep arrows and one irresistible
  dance per summoning on named resources, one cast action on its brain (no
  cooldown loop to spin on); the mechanical scenario proves the dance and a
  sleep arrow live every run (`pixie[...danceApplied=True;...sleepApplied=
  True]`, evidence `20260924T2220326799940Z-disposable-expanded-summoning`),
  and the Sprint 6 regression pins the resources and the AI shape.
- Runtime fixture: the three carriers join the exact grapple check; the
  web pack is checked part for part
  (`expanded-summoning-sprint-six-spider-web`); the mechanical scenario
  grabs with the Monitor Lizard (bite, hold, release, holder free) and webs
  the hostile with the Giant Spider (Reflex -100, forced natural 1: webbed,
  one use spent, the spider immune); the persistence fixture gains the
  Grizzly Bear and the Giant Spider (twelve units).
- Ledger: 8 appended, active identities (entries 2194-2201); foundation
  identities 1429; no new units, placements, icons or package files.
- Domain suite: 1791 cases (four Sprint 6 regressions in
  `ExpandedSummoningSprint6Tests`), all passing; repository validation
  PASS.

Runtime qualification (guarded, live installation restored after each
batch): recorded below as it completes.

## Sprint 7 record - Big-Cat Combat System

Inputs: the game's attack rules read from its assembly (IL listing, nothing
decompiled committed): `RuleAttackWithWeapon.IsCharge`,
`RuleAttackRoll.AutoMiss` and `RuleAttackRoll.SuspendCombatLog` (settable),
`RuleAttackRoll.Weapon`, `ItemEntity.HoldingSlot`, and
`Kingmaker.Items.UnitBody.Initialize`, which inserts the blueprint's
additional limbs first and its additional secondary limbs after them into
one weapon-slot list; `RuleCalculateAttacksCount` carries only the primary
and secondary hand counts, so natural extra limbs cannot be removed there.
The native `Pounce` feature (`1a8149c09e0bdfc48a305ee6ac3729a8`,
AddMechanicsFeature) already sits on the cats' profiles.

Decisions (recorded here rather than asked):

- One truthful pounce/grab/rake system: Pounce stays the native feature (a
  charge becomes a full attack). Grab is the shared summon grapple
  lifecycle with each cat's own claw (leopard `800092a2...`, lion
  `118fdd03...`, dire lion `c76f72a8...`, smilodon `8afc4774...`). Rake is
  `SummonRakeComponent` on the same combat-traits buff: the cat's rake
  claws are the last two slots of its body's additional limbs (the two
  extra claws for the leopard and lion, the secondary pair for the dire
  lion and smilodon), and an attack roll with a rake claw that is neither a
  charge nor made while the cat holds a grappled foe becomes a silent
  automatic miss - no d20, no damage, no combat-log line. Ordinary full
  attacks, attacks of opportunity and replayed commands therefore never
  show a rake; a charge and a held foe do. The decision function
  `ShouldRakeApply(isRakeWeapon, isCharge, isHolding)` is pinned by the
  domain suite; the placements, sizes, reach, templates and limb sets are
  unchanged.
- Lion visual: a tawny tint on the leopard rig through the Sprint 5 visual
  variant (no mane geometry: the charter marks mane polish optional for the
  dire lion and asks only for a lion visual; a texture-free tint is the
  bounded answer on a shared rig).
- Runtime fixture: the four cats join the exact grapple check as carriers
  that also carry the rake gate; the mechanical scenario proves the cadence
  live on the Leopard (a primary claw strikes on an ordinary attack, a rake
  claw misses silently, the same rake claw strikes on a charge and against
  a held foe after a real grab); the inventory checks the Lion's registered
  variant; the persistence fixture gains the Lion (thirteen units).
- Ledger: 4 appended, active identities (entries 2202-2205); foundation
  identities 1433; no new units, placements, icons or package files.
- Domain suite: 1795 cases (four Sprint 7 regressions in
  `ExpandedSummoningSprint7Tests`), all passing; repository validation
  PASS.

Runtime qualification (guarded, live installation restored after each
batch): recorded below as it completes.

## Sprint 8 record - Big-Cat Roster Completion

Inputs: the game has no native tiger unit (the round-4 named-graph
resolution: only the leopard and smilodon bodies exist); the leopard rig
`LeopardSummoned` (`768275c9885dd954fb3c84ba69ac4281`) already carries the
Leopard, Lion and Cheetah; the Pteranodon precedent (Phase 0) proved that a
project texture on a private material clone survives the fader, the hit
flash and the death dissolve; `Kingmaker.Designers.Mechanics.Buffs
.BuffMovementSpeed` (an enhancement-typed speed bonus under the game's own
cap) was read from the assembly.

Decisions (recorded here rather than asked):

- Tiger (SNA IV, 1d3 at V, 1d4+1 at VI-IX): a new unit on the leopard rig,
  Large 6 HD animal 23/15/17/2/12/6, speed 40, natural armor +3, the native
  large 2d6 bite and four project 1d8 claws (`KMG.Summoning.Natural.Claw1d8`,
  the native 1d6 claw animation with 1d8 dice), Pounce, Improved
  Initiative, Skill Focus (Perception), Weapon Focus (claw); claw grab on
  the shared lifecycle and the Sprint 7 charge-only rake gate; a 1.25
  view-only scale so it reads Large. It is a pounce/rake premium option, not
  a relabeled Lion: different tier, size, dice, chassis and coat.
- Tiger and Cheetah visuals: a procedural coat generated at view attach in
  the rig's own texture space. `SummonCoatRasterizer` reads only geometry -
  the skinned mesh's vertices, texture coordinates and triangles - and
  rasterizes each triangle into a private 512x512 texture whose colour comes
  from where the vertices sit on the body: stripes across the long axis for
  the tiger, a cellular spot field for the cheetah, a paler belly for both.
  The texture replaces the albedo on the private material clone with the
  colour slot reset to white (the Pteranodon's treatment); the game's own
  texture pixels are never read or stored. A rig without texture
  coordinates leaves the native look with the reason recorded.
- Cheetah sprint: a swift extraordinary ability, one use per summoning on a
  named resource, applying a one-round state with a +30-foot enhancement
  speed bonus (the game caps speed itself); a cast action on the cheetah's
  brain spends it. The tabletop tenfold once-per-hour sprint cannot repeat
  within one summoning in either reading; the bound is the resource. The
  cheetah's trip bite and chassis are unchanged; its view scale is 0.92.
- Icon: one Blender procedural render (a striped cat bust) for the Tiger;
  the Cheetah keeps its icon.
- Runtime fixture: the tiger joins the natural exactness checks (the
  project 1d8 claw in the weapon map) and the grapple carrier check with its
  rake gate; the cheetah's sprint pack is checked part for part; the
  mechanical scenario sprints with the cheetah (state applied, speed up,
  one use spent, a second use refused) and rakes with the tiger (silent
  miss on an ordinary attack, a hit on a charge); the persistence fixture
  gains the tiger (fourteen units); the creature review requires the coats
  applied.
- Pins moved with the roster: 81 creatures, SNA 71/399, 813 logical
  placements (799 published), 34 natural profiles, 91 icons, foundation
  identities 1448; the ledger gained exactly 15 appended, active identities
  (entries 2206-2220); visible choices 822 -> 828; package 250 -> 251 files.
- Domain suite: 1799 cases (four Sprint 8 regressions in
  `ExpandedSummoningSprint8Tests`), all passing; repository validation
  PASS.

Runtime qualification (guarded, live installation restored after each
batch): recorded below as it completes.

## Verified facts carried from Phase 0 (do not re-derive)

- Catalog pins at the start of Phase 1: 67 creatures, SM 66 / 361, SNA 57 /
  320, 681 logical placements (14 suppressed, 667 published), 182 templated
  placements, 26 natural profiles, 77 project icons, 8 view-scale entries,
  26 native wrappers, foundation identities 1184. Every one moves with the
  roster and is pinned in the domain tests, the manifest tool, the runner and
  the static validation.
- The native grapple lives in `ContextActionGrapple` (caster and target
  buffs), `UnitPartGrappleInitiator` / `UnitPartGrappleTarget` and
  `UnitGrappleController`; swallow whole in `ContextActionSwallowWhole`,
  `UnitPartSwallowWhole` / `UnitPartSwallowed` and `SwallowWholeSettings` on
  the view. The native Grab feature `efc1e80fb41e06544be46604983806d6`
  carries Shambling Mound constrict and target-state behaviour and is not
  reused as-is.
- Attack rules expose `RuleAttackWithWeapon.IsCharge`, `IsFullAttack`,
  `IsAttackOfOpportunity`, `AttackNumber`; `RuleAttackRoll.AutoHit`,
  `AutoCriticalThreat`, `AutoCriticalConfirmation` are settable; `UnitAttack`
  builds `AttackHandInfo` lists in `InitAttacks` / `CreateFullAttack`.
- A swapped material must be reset intact and adopted by the view's
  `StandardMaterialController` (`ReinitMaterials`) or the game's fades never
  reach it (Phase 0 closeout finding).
- The Frost Giant's native summoned unit is `590cd3d5e76fdc649a5f97bc984cd3c4`.
- The game's blueprints are not readable offline; native donors are found by
  the mod-load audit scenario `observe-expanded-summoning-native-donors`,
  which writes metadata only.

## Next executable action

Sprints 3-8 guarded runtime qualification on one build: structural
inventory and deep donor audit, visual contracts (80 creatures), mechanical
casting (Flash of Insight as an automatic critical, the grapple lifecycle
live case with the corrected holder checks, the mephit breath and burst
case, the lizard grab, the spider web, the leopard's rake cadence, the
cheetah's sprint and the tiger's rake), the player-path matrix, the
persistence trio (fourteen-unit fixture), the creature review of the
fourteen new creatures beside the four native mephits, the four repaired
creatures and the five cats, and the two compatibility transactions, each
batch alone on the machine; internal review of the review renders (tint
legibility per element, the lion's coat, the tiger's stripes, the cheetah's
spots); then record the evidence, update the PR body and close Phase 1.
