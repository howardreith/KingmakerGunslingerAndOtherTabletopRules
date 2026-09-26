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
| 7 | Leopard, Lion (+visual), Dire Lion, Dire Tiger/Smilodon | pounce / grab by limb and target identity / rake gate (charge, or the foe held since the round began) |
| 8 | Tiger -/IV new; Cheetah authentic visual + bounded per-summon sprint | cat family completion |

Placements propagate to the 1d3 / 1d4+1 tiers. No feats, aquatic, mounts,
familiars, variant elementals or later-sprint creatures; Dire Bat stays
hidden; Eagle and Roc are not redesigned. Identities are reused, the ledger is
append-only, stable identities register even when publication is disabled,
and no future roster data is published.

## Sprint ledger

| Sprint | State | Evidence |
|---|---|---|
| 3 | COMPLETE - CORRECTED AND REQUALIFIED (2026-09-25 correction record; HumanReview NOT_PERFORMED_NONBLOCKING) | see "Sprint 3 record" and the correction order record |
| 4 | COMPLETE - CORRECTED AND REQUALIFIED (2026-09-25 correction record; HumanReview NOT_PERFORMED_NONBLOCKING) | see "Sprint 4 record" and the correction order record |
| 5 | COMPLETE - CORRECTED AND REQUALIFIED (2026-09-25 correction record; HumanReview NOT_PERFORMED_NONBLOCKING) | see "Sprint 5 record" and the correction order record |
| 6 | COMPLETE - CORRECTED AND REQUALIFIED (2026-09-25 correction record; HumanReview NOT_PERFORMED_NONBLOCKING) | see "Sprint 6 record" and the correction order record |
| 7 | COMPLETE - CORRECTED AND REQUALIFIED (2026-09-25 correction record; HumanReview NOT_PERFORMED_NONBLOCKING) | see "Sprint 7 record" and the correction order record |
| 8 | COMPLETE - CORRECTED AND REQUALIFIED (2026-09-25 correction record; HumanReview NOT_PERFORMED_NONBLOCKING) | see "Sprint 8 record" and the correction order record |

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
  stat blocks (Appendix A) on the animal class. Both hooves are secondary
  attacks (Docile; corrected under the 2026-09-25 order): the native summons
  carry them as primary limbs, so a docile-hoof carrier on each unit sets the
  game's own secondary flag on both hoof entities at spawn and after a load,
  and the game itself applies -5 to hit and half the Strength modifier to
  damage. Endurance and Run are omitted (no proven final-live feature
  identity).
- Owlbear (SNA IV) is the first magical-beast chassis: `CR8_OwlbearStandard`
  `d6e0acbdbdb56114898922063ae2cba0` as a sanitized body donor, 5 HD on
  `MagicalBeastClass` `b9e97f47cb86f2d45a0784a096ff8037`, natural armor +5
  (`7661741dbb9604842a642457456fd0e4`), bite 1d6 and two 1d6 claws, Improved
  Initiative, Great Fortitude, Skill Focus (Perception), reduced reach. Claw
  grab is deferred to the shared grapple lifecycle Sprint 4 introduces and is
  recorded as a deviation until then; Sprint 4 delivered it, and the Owlbear
  entry below records the claw grab riding that lifecycle.
- Cyclops (SNA V) is the first humanoid chassis: `CR5_CyclopStandard`
  `124f1c45ef24d654e9cd420fe84f7f36` as a sanitized body donor, 10 HD on
  `HumanoidClass` `6ab4526f94d2e3e439af0599a29b6675`, natural armor +7
  (`e73864391ccf0894997928443a29d755`), the native `StandardGreataxe`
  `6efea466862f014469cec6c3f2b85cb7` in hand (the game scales it to the
  Large 3d6), Ferocity, Power Attack and Cleave
  (`d809b6c4ff2aaff4fa70d712a70f7d7b`). Flash of Insight is bounded to one
  use per summoning and to the next attack roll (corrected under the
  2026-09-25 order): a swift action arms a state with no duration of its
  own that the native `RemoveBuffOnAttack` ends after one attack, so a use
  never lapses unspent and a save and a reload find it exactly once (the
  first requalification run found a one-round state lapsed before the
  reloaded attack); that attack's own d20 result
  is chosen as a natural 20 through the game's pre-rolled-result seam
  (`RuleRollD20.m_PreRolledResult`, `CyclopsFlashOfInsightComponent`), so
  the hit and the threat follow from the roll, the critical confirmation is
  rolled normally, no automatic-hit flag is set and no other roll is
  touched; a small brain spends it when the cyclops fights and the player
  may spend it from the action bar. The +4 hide armor is carried as an
  exact armor-descriptor fact (`Cyclops.HideArmor`; no item, loot or
  inventory) over the stat block's +7 natural armor, so the armor class is
  the tabletop 19 (10 + 4 armor - 1 Dexterity + 7 natural - 1 size); the
  heavy crossbow is omitted (the summon carries no equipment beyond the
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
  path never rolls the d20, which is why the corrected Flash of Insight uses
  the pre-rolled-result seam instead (the Sprint 3 record).
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
  Strength (plus constrict) on success and releasing on failure - both the
  grab and the maintain apply the attack-roll natural 1 (always fails) and
  natural 20 (always succeeds) over the engine's combat-maneuver rule,
  which decides by the sum alone (`IsSummonManeuverSuccess`, pinned), while
  a failed concealment check, an auto-failure flag or a target immune to
  combat maneuvers still denies the maneuver whatever the die showed
  (`SummonManeuverChecks.Succeeded`; the engine's rule returns before it
  computes CMB and CMD against an immune target, so the verdict it leaves
  behind reads 0 + 0 >= 0 as a success, and a summon must not take hold on
  that - the rules scenario refuses the grab while the foe is immune and
  takes it once the immunity ends). The
  same component - this is the link ownership the charter asks for -
  releases exactly the target the summon's own initiator part names
  whenever the hold buff turns off, for
  any reason (escape, the holder falling, dispel, the summon's disposal),
  never touching the initiator part itself (the game's controller drops it,
  and removing it from inside its own buff's removal would re-enter). The
  worm holds on a successful grab and swallows through the native part on
  a later turn's successful maintain check, used as though attempting to
  pin, against a foe up to one size smaller (corrected under the
  2026-09-25 order); `SummonSwallowLifecycleComponent` on its traits spits
  everyone out when the traits turn off. The game has no native
  hold-and-swallow path of its own - a holding initiator cannot act - so
  the project's own components drive the tabletop
  grab-then-maintain-then-swallow sequence over the native parts.
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
  shared lifecycle with one link per bite, four at most (corrected under the
  2026-09-25 order: the links are held-state buffs on the targets that name
  the flytrap and their establishing bite; a bite that holds cannot take a
  second foe), and engulf as the swallow-whole sequence for a Medium or
  smaller foe held since the round began (the project `GiantFlytrap.Engulfed`
  state: 1d8+7 bludgeoning and 1d8 acid each round inside). Vital Strike is
  omitted.
- Purple Worm (SNA VIII): the native dedicated `PurpleWormSummoned`
  `bf2216f48b3f4d24c9c502007649340d` as donor, rebuilt on the natural
  builder as a 16 HD magical beast, Gargantuan, 35/6/25/1/8/8, speed 20,
  natural armor +22 (`eee672c8f6555b445a89dbbb91361d64`), the native
  `PurpleWormBite` (`7e4b9b41a9358264d9e3c69c183ca0a2`) and
  `PurpleWormSting` (`287cd06241fdaf8408410b226f744093`), the exact native
  `PurpleWormPoisonFeature` (`728446b9d0bf47144a1b621169299c2a`,
  Constitution-scaled sting poison), trip immunity, Critical Focus,
  Improved Critical (bite), Power Attack, Weapon Focus (bite); the bite grab
  holds (corrected under the 2026-09-25 order), and on a later turn a
  successful maintain check - used as though attempting to pin - swallows a
  foe up to one size smaller (Gargantuan swallows Huge) whole and deals the
  bite's damage, with the swallowed state cloned component-for-component
  from the native `PurpleWormSwallowed` (`368d1df7c1d0267459a584bf23ccadc8`);
  a failed check releases and a same-size foe is held but never swallowed.
  The native summoned worm's burrowing kit and brain are not carried
  (bounded combat adaptation, charter Sprint 4); Awesome Blow, Improved
  Bull Rush, Staggering Critical and Weapon Focus (sting) are omitted.
- Owlbear: the claw grab deferred in Sprint 3 now rides the shared
  lifecycle (`Owlbear.CombatTraits` with the native `ClawLarge1d6`).
- Flash of Insight history: round-2 mechanical evidence
  (`20260924T2023157894340Z-disposable-expanded-summoning`) showed that with
  AutoHit set `RuleAttackRoll` takes its automatic-hit path, never rolls the
  d20 and decides the critical only from AutoCriticalThreat and
  AutoCriticalConfirmation together, and the sprint answered with an
  automatic critical hit. The 2026-09-25 correction order rejected that
  adaptation; the corrected Flash of Insight chooses the attack's own d20
  as a natural 20 on the pre-rolled-result seam (the Sprint 3 record above
  and the correction record below), and every description, profile
  deviation, manifest note, static record, runner check and test says so.
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
  and magma form have no native spell or form; under the 2026-09-25
  correction order they are project-owned bounded abilities (the correction
  record below), the Ooze Mephit's stinking cloud spawns a project ally-safe
  clone of the native cloud area, and the Salt Mephit's glitterdust selects
  enemies of the caster only. Spell-list memberships are not carried on the
  clones.
- Brains: a project brain per variant with one cast action per ability (the
  breath on a four-round cooldown so the mephit also claws; the one-use
  abilities without), the shape the Cyclops and Pixie actions already have.
- Visuals: `ExpandedSummoningVisualVariantPatch` (a postfix on the view's
  data attach) clones the view's renderer materials and sets each
  variant's rim light colour - the shader's HDR glow through which the
  translucent mephit body is seen, and exactly what tells the game's own
  air mephit (1.3/1.2/1.13) from its fire mephit (4.16/1.51/0.32): warm
  sand for Dust, icy cyan-white for Ice, ember red for Magma, slime green
  for Ooze, crystalline white for Salt, grey-white vapour for Steam - on
  the clone, then has the game's material controller re-read the renderer
  so its fades and tints drive the clone (the Pteranodon's treatment), and
  recolours the view's own looping rim-light animation as it reaches the
  view's material controller with the unit's spawned effects (a prefix on
  the controller's update; the settings object belongs to this view's
  effects, and the controller evaluates it every frame to paint the rim
  slot): its colour gradient takes the variant colour and its intensity
  scale is set so the pulse peaks at the variant's brightness. The clone
  is private to the view, the donor's shared material and prefab are never
  written, and a view with no renderer or no colour slot is left native
  with the reason recorded. The profiles are plain numbers in the special
  profiles so the domain suite pins them; the creature review records the
  attach outcome and the materials on the view at capture.
  Rounds 8-12 correction: the first design tinted the rig's tint slot (with
  an emission glow declared for Magma and Steam), and the round-8 review
  renders (`20260925T0209555654561Z`) showed the native look on every
  variant although each view had reported the tint applied. The capture-
  time material record added in round 8 showed the clone retained and
  driven by the controller with `_TintColor` at the controller's own value
  on every mephit, native or variant (`20260925T0237410511666Z`): the game
  rewrites that slot every frame. A procedural coat on the main texture
  (`20260925T0312103104469Z`) was applied, retained - and invisible: the
  probe run (`20260925T0333329932642Z`) showed the body material
  (`PF/StandardDynamic`, premultiplied alpha, no emission keyword or slot)
  differing between the native elements only in `_RimColor`; and setting
  that slot on the clone (`20260925T0350332482297Z`) was undone every
  frame by the controller's looping rim animation, which only reaches the
  controller after the view attaches (`20260925T0408513561430Z`), so the
  animation's own per-view settings are recoloured as they arrive
  (`20260925T0425508244596Z`: all six legible; `20260925T0441444891847Z`:
  at the designed brightness once the target is shared across the
  animations; `20260925T0456499760098Z`: ice moved to cyan-white). The
  leopard rig has rim lighting off, so the lion's tint and the cats' coats
  show there. No emission glow is claimed anywhere.
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
  a bounded ranged extraordinary ability on the special builder (corrected
  under the 2026-09-25 order): a 50-foot (custom range) ranged touch attack
  through the game's own projectile delivery with the ray weapon its rays
  use (`f6ef95b1f7bb52b408a5b345a330ffe8`), against one foe up to one size
  larger than the spider (a target checker), no saving throw; a hit applies
  the native `WebGrappled` state (`a719abac0ea0ce346b401060754cc1c0`:
  entangled, cannot move, its own per-round break-free check against the
  Constitution-based DC 10 + half hit dice + Constitution) for at most ten
  rounds; two uses per summoning on a named resource (the tabletop four per
  day exceeds a summoning), one cast action on its own brain, the spider's
  summon icon. Climb is omitted: no save-safe seam exists for a summon's
  climb movement.
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
  the hostile with the Giant Spider (the cast targets it, one use spent,
  the spider immune; the ranged touch attack itself, its miss against a high
  touch AC and its hit against a low one, is proven live in the correction
  rules scenario); the persistence fixture gains the
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

- One truthful pounce/grab/rake system (as corrected under the 2026-09-25
  order): Pounce stays the native feature (a charge becomes a full attack).
  Grab is the shared summon grapple lifecycle keyed on attack identity - the
  slot the attacking weapon entity sits in, never the weapon blueprint the
  foreclaws and rake claws share: the leopard, the lion and the dire lion
  grab with the bite only; the tiger and the smilodon with the bite and the
  first two additional limbs (the foreclaws); a rake slot never grabs; and
  every grab respects the universal size rule (a foe of the holder's size
  or smaller). Rake is `SummonRakeComponent` on the same combat-traits buff
  plus the attack-sequencing seam (`ExpandedSummoningRakeSequencePatch` on
  `UnitAttack.CreateFullAttack`): the rake claws are the last two slots of
  the body's additional limbs, they strike only on a charge (pounce) or
  against the exact foe the cat has held since its round began (the held
  state has ticked at least once: the grappled state carries a no-op round
  component so the game advances its round number every six seconds of the
  hold, and the reading counts a tick due in the same frame so the holder's
  own check sees the round the foe is entering), the sequencing seam drops
  them from any other full attack so no hidden extra swing is made, and the roll-level
  gate makes any stray rake roll a silent automatic miss. A single attack
  (an attack of opportunity) never carries a rake slot. The decision
  functions `IsGrabLimb`, `IsRakeSlot`, `IsHeldSinceRoundStart` and
  `ShouldRakeApply(isRakeWeapon, isCharge, heldTargetSinceRoundStart)` are
  pinned by the domain suite; the placements, sizes, reach, templates and
  limb sets are unchanged.
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
  Initiative, Skill Focus (Perception), Weapon Focus (claw); grab with the
  bite and both foreclaws by limb identity on the shared lifecycle and the
  rake gate (a charge, or the foe held since the round began); a 1.25
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

## Runtime qualification record - Sprints 3-8 on the reviewed build (superseded)

This record is the reviewed head's (`2569a8eb`, runtime commit `30a13445`).
The 2026-09-25 correction order found that build's cats, grab sizes,
Flytrap, mephit roles, Cyclops, Web, hooves and visual resources short of
the charter; it is kept as history and superseded by the correction record
below, which is the qualification of the corrected build.

Every scenario below ran through the guarded launcher on commit `30a13445`
(`Build-Local.ps1` PASS: 1799 domain tests, 0 failures; package
`KingmakerGunslinger-0.0.138-local-runtime.zip` SHA-256 `729c1ffa2f77b5755b819c5f916614e96d2b94be8bb9fda435bd4f2074a951d1`, DLL SHA-256
`ef951f4dcc2961506513de9a82f5b1b04d98be49903a5426547e69048b225388`), each batch alone on the machine, and the live installation was
restored to 0.0.117 / 136 files /
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`
after each batch (restoration records under
`runtime-evidence/expanded-summoning-restoration`).

| Evidence | Scenario | Status | Commit | Assertions |
|---|---|---|---|---|
| `20260925T0508280717241Z-observe-expanded-summoning-inventory` | observe-expanded-summoning-inventory | PASS | 30a13445 | 48 |
| `20260925T0512074898947Z-observe-expanded-summoning-native-donors` | observe-expanded-summoning-native-donors | PASS | 30a13445 | 2 |
| `20260925T0517164499486Z-disposable-expanded-summoning-visual-contracts` | disposable-expanded-summoning-visual-contracts | PASS | 30a13445 | 15 |
| `20260925T0523222495194Z-disposable-expanded-summoning` | disposable-expanded-summoning | PASS | 30a13445 | 17 |
| `20260925T0526417220346Z-disposable-expanded-summoning-player-path` | disposable-expanded-summoning-player-path | PASS | 30a13445 | 10 |
| `20260925T0541546952818Z-working-save-expanded-summoning-prepare` | working-save-expanded-summoning-prepare | PASS | 30a13445 | 11 |
| `20260925T0545454137057Z-working-save-expanded-summoning-verify-cleanup` | working-save-expanded-summoning-verify-cleanup | PASS | 30a13445 | 11 |
| `20260925T0549365901069Z-working-save-expanded-summoning-verify-absent` | working-save-expanded-summoning-verify-absent | PASS | 30a13445 | 11 |
| `20260925T0555240446760Z-working-save-expanded-summoning-creature-review` | working-save-expanded-summoning-creature-review | PASS | 30a13445 | 38 |
| `20260925T0607591341645Z-working-save-expanded-summoning-creature-review` | working-save-expanded-summoning-creature-review | PASS | 30a13445 | 24 |
| `20260925T0616156497187Z/20260925T0620346667524Z-disposable-expanded-summoning` | compatibility-mechanical (gunslinger-only; record `20260925T0616156497187Z`, failures=0, liveTreeUnchanged=True) | PASS | 30a13445 | 17 |
| `20260925T0616156497187Z/20260925T0626348826393Z-disposable-expanded-summoning` | compatibility-mechanical (gunslinger-high-risk-combined; record `20260925T0616156497187Z`, failures=0, liveTreeUnchanged=True) | PASS | 30a13445 | 17 |

Internal acceptance: every Sprint 3-8 item is INTERNAL-ACCEPTED on this
evidence (structural inventory and deep donor audit exact, visual contracts
for every creature, the mechanical cases live, the player-path matrix, the
persistence trio, the party-camera creature review of every new or changed
creature, and the two compatibility transactions). HumanReview:
NOT_PERFORMED_NONBLOCKING. OwnerDelegationGranted.

## Correction order record - 2026-09-25 (PR #23 correction and requalification)

Order: "Expanded Summoning Phase 1 - PR #23 correction and requalification
order" on the reviewed head `2569a8eb`. The prior internal-acceptance labels
waived nothing; every finding below is implemented exactly or would have
been marked BLOCKED. Nothing was restarted, reset, force-pushed, merged,
released, deployed permanently, or begun on Sprint 9.

Findings and what was done:

1. Visual resource ownership: `SummonVisualOwnership` per view records the
   replaced renderers with their original shared materials, every private
   material clone and every coat texture; `ExpandedSummoningVisualTeardownPatch`
   (Harmony prefix on `UnitEntityView.OnDestroy`) releases it with the view;
   an attach that fails part way, or ends short of applied, is rolled back
   in the same frame (renderers put back, clones, textures and the material
   controller's instances of the clones destroyed at once); `ReleaseAll`
   is the module-wide sweep. The visual lifecycle scenario measures the
   variant-prefixed material and texture counts back to baseline after each
   of three cast-and-dispose cycles (dust mephit, a 1d3 steam mephit cast,
   tiger, cheetah, lion), after a fault-injected attach, after the sweep on
   a live lion, and at the end, with the native air mephit's materials and
   rim colour and the Pteranodon's visual unchanged.
2. Cats: grab by limb identity (bite only for the leopard, lion and dire
   lion; bite and both foreclaws for the tiger and smilodon; a rake slot
   never grabs) and by target identity; rake only on a charge or against
   the exact foe held since the cat's round began; the sequencing seam drops
   rake slots from any other full attack; a single attack never carries one.
3. Grab audit: the universal size rule (holder's size or smaller unless the
   stat block says otherwise), +4 on grapple checks from grab and +5 more to
   maintain (the grab bonus alone with nothing held), maintain damage as the
   establishing limb's own weapon damage; the Purple Worm holds on the grab
   and swallows a foe up to one size smaller on a later turn's successful
   check (natural 1 releases, natural 20 swallows, a same-size foe is never
   swallowed, a Colossal foe is refused); the Giant Flytrap holds one foe
   per bite on distinct held-state links, engulfs Medium or smaller, and
   releases on escape, the last link's end, disposal, the swallow lifecycle
   and the area-leave sweep; the game's pathfinder still routes a free unit
   past four held units.
4. Mephit roles: project Wind Wall (Dust; a 15-foot shelter for six rounds for
   every creature inside that is not the mephit's enemy: arrows and bolts
   aimed at a sheltered creature miss, other
   ranged weapons roll the tabletop 30% miss chance, melee and rays pass),
   Chill Metal (Ice; close range, only a foe wearing or carrying metal, Will
   negates, the seven-round table none/1d4/2d4/2d4/2d4/1d4/none in full
   against metal armor and minimal 1 or 2 against a metal weapon only),
   Pyrotechnics (Magma; every enemy within 20 feet blinded 1d4+1 rounds,
   Will negates) and Magma Form (Magma; five rounds of DR 20/magic, speed 10
   and no attacks with breath and abilities intact; the brain waits three
   rounds before pooling). Every DC is Charisma-based at caster level 6.
   The stinking cloud runs on a project clone of the native cloud area whose
   every action list and area buff is gated on enemy-of-caster; glitterdust
   selects enemies only. The live cases put the hostile, the party caster,
   an allied summon and the mephit inside the one cloud placement: only the
   hostile is nauseated.
5. Cyclops: +4 hide armor as an armor-descriptor fact (no item, loot or
   inventory) over +7 natural armor at armor class 19, with the live
   breakdown; Flash of Insight chooses the attack's own d20 as a natural 20
   with an ordinary confirmation, one use, a save rolled while armed is
   untouched, and the persistence trio proves the single armed use across
   the save and the reload (still spent after the reload, armed once, the
   next attack a 20, the one after it a 1 that misses).
6. Web: a ranged touch attack through the projectile delivery with the ray
   weapon, 50 feet, a foe up to one size larger, no save, the native
   web-grappled state with its Constitution-based break-free, the native
   immunity, two uses; live against a high-touch-AC/hopeless-Reflex foe
   (missed) and a low-touch-AC/superb-Reflex foe (webbed).
7. Pony and Horse: both hooves secondary through the game's own secondary
   flag (-5 to hit, half Strength to damage, both listed in the full
   attack), with the live breakdown against the same hooves as primary.
8. Documents and claims: every "omitted", "charge-only", "automatic
   critical", "Reflex-save web", "one held target" and "primary hooves"
   claim was replaced by the corrected mechanics in the state file, the
   report, the fidelity matrix, the roster notes, the static record, the
   changelog, the program state, the profiles and the tests; adaptations
   are recorded as adaptations, never as rules facts.
9. Requalification: the full gate list on one candidate commit (below).
10. PR #23 stays a draft and unmerged.

### Requalification on the candidate commit

Candidate commit `2f04baf4` (`Build-Local.ps1` PASS: 1805 domain tests, 0
failures; package `KingmakerGunslinger-0.0.138-local-runtime.zip` SHA-256
`bb12daef63a78c9ebcd1af9b3f0f51d7ea5d193c3f46028d425dbf443009de12`, DLL SHA-256 `1753c235d42b9f23569a5608837e290b0ab1b77ea20cb006d9a7e1c7d395bb81`). Every gate below ran through the guarded launcher
on that commit, each batch alone on the machine, and the live installation
was restored to 0.0.117 / 136 files / `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` after each batch (the restoration
records under `runtime-evidence/expanded-summoning-restoration`, one per batch of this candidate's set:
`20260925T2256005248418Z-observe-expanded-summoning-inventory.json`, `20260925T2302098623052Z-disposable-expanded-summoning-visual-contracts.json`, `20260925T2322090385235Z-disposable-expanded-summoning.json`, `20260925T2328478527124Z-disposable-expanded-summoning-rules.json`, `20260925T2334579715665Z-disposable-expanded-summoning-visual-lifecycle.json`, `20260925T2348412265034Z-working-save-expanded-summoning-prepare.json`, `20260926T0001214650545Z-working-save-expanded-summoning-creature-review.json`, `20260926T0011181316459Z-working-save-expanded-summoning-creature-review.json`).

| Evidence | Gate | Status | Commit | Assertions |
|---|---|---|---|---|
| `20260925T2250290639749Z-observe-expanded-summoning-inventory` | observe-expanded-summoning-inventory | PASS | 2f04baf4 | 48 |
| `20260925T2254080500336Z-observe-expanded-summoning-native-donors` | observe-expanded-summoning-native-donors | PASS | 2f04baf4 | 2 |
| `20260925T2259196277277Z-disposable-expanded-summoning-visual-contracts` | disposable-expanded-summoning-visual-contracts | PASS | 2f04baf4 | 15 |
| `20260925T2305288036107Z-disposable-expanded-summoning` | disposable-expanded-summoning | PASS | 2f04baf4 | 17 |
| `20260925T2308574998456Z-disposable-expanded-summoning-player-path` | disposable-expanded-summoning-player-path | PASS | 2f04baf4 | 10 |
| `20260925T2325577479285Z-disposable-expanded-summoning-rules` | disposable-expanded-summoning-rules | PASS | 2f04baf4 | 11 |
| `20260925T2332106243721Z-disposable-expanded-summoning-visual-lifecycle` | disposable-expanded-summoning-visual-lifecycle | PASS | 2f04baf4 | 7 |
| `20260925T2338139678603Z-working-save-expanded-summoning-prepare` | working-save-expanded-summoning-prepare | PASS | 2f04baf4 | 12 |
| `20260925T2342075552541Z-working-save-expanded-summoning-verify-cleanup` | working-save-expanded-summoning-verify-cleanup | PASS | 2f04baf4 | 12 |
| `20260925T2346027778408Z-working-save-expanded-summoning-verify-absent` | working-save-expanded-summoning-verify-absent | PASS | 2f04baf4 | 12 |
| `20260925T2351580405941Z-working-save-expanded-summoning-creature-review` | working-save-expanded-summoning-creature-review (half 1 of 2) | PASS | 2f04baf4 | 38 |
| `20260926T0004389221429Z-working-save-expanded-summoning-creature-review` | working-save-expanded-summoning-creature-review (half 2 of 2) | PASS | 2f04baf4 | 24 |
| `20260926T0011187022628Z/20260926T0015363490585Z-disposable-expanded-summoning` | compatibility-mechanical (gunslinger-only; record `20260926T0011187022628Z`, failures=0, liveTreeUnchanged=True) | PASS | 2f04baf4 | 17 |
| `20260926T0011187022628Z/20260926T0021271261533Z-disposable-expanded-summoning` | compatibility-mechanical (gunslinger-high-risk-combined; record `20260926T0011187022628Z`, failures=0, liveTreeUnchanged=True) | PASS | 2f04baf4 | 17 |

The branch carries a documentation-only delta after the candidate: the
requalification record itself - this section, the implementation report's
correction section, `EXPANDED-SUMMONING-PROGRAM-STATE.json` and
`validation/static-validation.json`. Nothing executable changes with it:
`git diff --name-only 2f04baf4..HEAD` lists those four records and nothing
else, and none of them is compiled or packaged (the packaged documents are
`CHANGELOG.md`, `README.md`, `INSTALLATION-COMPATIBILITY.md`,
`SMOKE-TEST-GUIDE.md` and `THIRD-PARTY-ASSETS.md`, all untouched). The
package and DLL hashes do differ at the branch head, and necessarily so:
the exact-reference build stamps the commit into the assembly as
provenance (`tools/build_mod_from_private_references.py --git-commit`
writes `[assembly: AssemblyMetadata("GitCommit", ...)]`), so every commit
produces its own artifact identity. Those hashes name the commit, not a
change in behaviour; the build is otherwise deterministic, and a rebuild
of the candidate's own tree reproduces the candidate's DLL. The
qualification therefore stands on the candidate's package - SHA-256
`bb12daef63a78c9ebcd1af9b3f0f51d7ea5d193c3f46028d425dbf443009de12` and
DLL `1753c235d42b9f23569a5608837e290b0ab1b77ea20cb006d9a7e1c7d395bb81`,
recorded in the deployment manifest of every batch of the set - and the
static gates that read the records (the repository validation, the Phase 1
validator, the manifest tool and the 1805-test domain suite) were rerun on
the branch head and pass. The guarded runtime evidence above stands for
the branch head as for `2f04baf4`.

Live observations carried into the record (each the assertion's own observed
string, clipped):

- Visual resource lifecycle: `loadingGate:framesWaited=35;inProcess=False,screen=False,manual=False,paused=False,mode=Default||baseline:materials=0;textures=0;live=0;donor[mephit_grey_character_d (Instance)/PF/StandardDynamic/asset:mephit_grey_character_d#1441332,rim=RGBA(1, 1, 1, 1),tint=RGBA(1, 1, 1, 1),variantPatch=<none>];donorRim[RGBA(1.3, 1.2, 1.13, 1)]||cycle1:cast=7;dust-mephit=variant:applied,key=dust-mephit,materials=1,slot=_TintColor,rim=1,rimAnimations=0,controller=reinitialized,driven=KMG_SummonVisualVariant (Instance){renderers=1;materials=1;textures=0;released=False},steam-mephit=variant:applied,key=steam-mephit,materials=1,slot=_TintColor,rim=1,rimAnimations=0,controller=reinitialized,driven=KMG_SummonVisualVariant (Instance){renderers=1;materials=1;textures=0;released=False},steam-mephit=variant:applied,key=steam-mephit,materials=1,slot=_TintColor,rim=1,rimAnimations=0,controller=reinitialized,driven=KMG_SummonVisualVariant (Instance){renderers=1;materials=1;textures=0;released=False},steam-mephit=variant:applied,key=steam-mephit,materials=1,slot=_TintColor,rim=1,rimAnimations=0,controller=reinitialized,driven=KMG_SummonVisualVariant (Instance){renderers=1;materials=1;textures=0;released=False},tiger=variant:applied,key=tiger,materials=1,slot=_TintColor,rim=0,rimAnimations=0,coat=1,coat=Stripes,mode=mesh,triangles=3226,size=512,controller=reinitialized,driven=KMG_SummonVisualVariant (Ins...`; failed attach: `outcome=variant:exception:InvalidOperationException;materials=0;textures=0;live=0;native=True`; module sweep: `released=1;swept=0;materials=0;textures=0;live=0;variantOff=True`;
  donor and Pteranodon: `donorSame=True;before[mephit_grey_character_d (Instance)/PF/StandardDynamic/asset:mephit_grey_character_d#1441332,rim=RGBA(1, 1, 1, 1),tint=RGBA(1, 1, 1, 1),variantPatch=<none>];after[mephit_grey_character_d (Instance)/PF/StandardDynamic/asset:mephit_grey_character_d#1441332,rim=RGBA(1, 1, 1, 1),tint=RGBA(1, 1, 1, 1),variantPatch=<none>];rimBefore[RGBA(1.3, 1.2, 1.13, 1)];rimAfter[RGBA(2.4, 1.7...`.
- Cats: `leopard:classification=True;rakeRefused=True;clawGrabbed=False(expected False);biteGrabbed=True;singleAttackClean=True;sameTurnPlan=dropped(3/0);sequence[held=kept(5/2),other=dropped(3/0),charge=kept(5/2),ordinary=dropped(3/0)];released=True;lion:classification=True;rakeRefused=True;clawGrabbed=False(expected False);biteGrabbed=True;singleAttackClean=True;sameTurnPlan=dropped(3/0);sequence[held=kept(5/2),other=dropped(3/0),charge=kept(5/2),ordinary=dropped(3/0)];released=True;dire-lion:classification=True;rakeRefused=True;clawGrabbed=False(expected False);biteGrabbed=True;singleAttackClean=True;sameTurnPlan=dropped(3/0);sequence[held=kept(5/2),other=dropped(3/0),charge=kept(5/2),ordinary=dropped(3/0)];released=True;tiger:classification=True;rakeRefused=True;clawGrabbed=True(expected True);biteGrabbed=True;singleAttackClean=True;sameTurnPlan=dropped(3/0);sequence[held=kept(5/2),other=d...`.
- Grab sizes and the worm: `grizzly-bear:size=Large;Medium:grabbed=True;maintainCmb=115;tripCmb=106,Large:grabbed=True;maintainCmb=115;tripCmb=106,Huge:grabbed=False;freeGrappleCmb=110;freeTripCmb=106;grabBonus=True;maneuverImmuneRefused=True;takenOnceImmunityEnds=True;leopard:size=Medium;Small:grabbed=True;maintainCmb=112;tripCmb=103,Medium:grabbed=True;maintainCmb=112;tripCmb=103,Large:grabbed=False;freeGrappleCmb=107;freeTripCmb=103;grabBonus=True;maneuverImmuneRefused=True;takenOnceImmunityEnds=True;worm:gargantuanGrabbed=True;swallowSizeRefused=True;eligible=True;swallowed=False;stillHeld=True;damage=0->94;notSwallowed=True;colossalRefused=True;hugeSwallowAllowed=True`.
- Flytrap: `placement:hostile=dir1@2.5m,wolf0=dir2@2.5m,wolf1=dir3@2.5m,wolf2=dir4@2.5m,wolf3=dir5@2.5m;links:fourBites=True;link0=True;link1=True;busyBiteRefused=True;link2=True;link3=True;fifthRefused=True;held=4;hostile[holder=True,bite=0];wolf0[holder=True,bite=1];wolf1[holder=True,bite=2];wolf2[holder=True,bite=3];distinct=True;pathing[error=False;points=2;obstacles=PathClear;heldStanding=4;from=(13.13,-6.00,-16.82);to=(25.13,-6.00,-4.82);crowd=(19.13,-6.00,-10.82)];release:escaped=True;swept=2;sweptFree=True;reach:reach=True,los=True,dist=2.50,conscious=True;engulf:stillHeldAfterOwnRound=True;eligible=True;largeMaintained=True;heldAfterLarge=1;engulfed=True;engulfTickDamage=True;heldAfterEngulf=0;multiHoldAfterEngulf=False;hostile=cantMove=True;entangled=False;cantAct=True;ending:relinked=True;holdEnded=True;spatOut=True;regrabbed=True;disposalReleased=True`.
- Mephit roles and the ally-safe cloud: `chillMetal:noMetalUntargetable=True;armored[armor=ChainmailType;metalArmor=True;metalWeapon=False;tier=2];targetable=True;chilled=True;ticks=1,3,3,5,3,0;outcomes=round=2,tier=2,dice=1d4,minimal=1,damage=1/round=3,tier=2,dice=2d4,minimal=2,damage=3/round=4,tier=2,dice=2d4,minimal=2,damage=3/round=5,tier=2,dice=2d4,minimal=2,damage=5/round=6,tier=2,dice=1d4,minimal=1,damage=3/round=7,tier=2,dice=0d4,minimal=0,damage=0;fullTable=True;armed[armor=none;metalArmor=False;metalWeapon=True;tier=1];minimalTicks=1,2,2,2,1,0;minimalTable=True;noMetalAfter=True;execution=ability=KMG_Summoning_Special_IceMephit_SpellLikeTwo;targetable=True;approach=True;endedAfterTicks=True;detached=False;finished=True;result=Success;magma:blinded=True;alliesSighted=True;blindSeconds=30;pooled=True;speed=40->10->40;cannotAttack=True;attackInterrupted=True;abilitiesWork=True;slash18InForm=0;slash18OutOfForm=10(DR 5/magic and the game's difficulty scaling);attacksAgain=True;execution=ability=KMG_Summoning_Special_MagmaMephit_SpellLikeTwo;targetable=True;approach=True;endedAfterTicks=True;detached=False;finished=True;result=Success;glitterdust:blindBefore=False;hostileBlinded=True;partyCasterSighted=True;alliedS...`; wind wall: `windWall:area=True;inside=5;sheltered=True;bow[HuntersBlessingLongbowItem:type=Ranged,autoMiss=True,missChance=0,hit=False];thrown[FrozenCrescentItem:type=Ranged,autoMiss=False,missChance=30,hit=True];melee[LongswordPlus1:type=Melee,autoMiss=False,missChance=0,hit=True];ray:type=RangedTouch,autoMiss=False,missChance=0,hit=True;outcomes=166625bc-9a0f-43bc-b44c-905d58b2a0a6:Deflected:Longbow:missChance=0/166625bc-9a0f-43bc-b44c-905d58b2a0a6:MissChance:ThrowingAxe:missChance=30;ended=True;shelter[component=True;componentBuffIsState=True;areaCaster=KMG_Summoning_Unit_DustMephit;mephitPlayerFaction=False;caster[hasState=True,sourced=KMG_Summoning_Special_DustMephit_WindWallState,allyOfMephit=F...`; stinking cloud: `cloud:area=True;inside=5(KMG_Summoning_Unit_Wolf,KMG_Runtime_ExpandedSummoning_HostileTarget,KMG_Runtime_ExpandedSummoning_RulesCaster,KMG_Summoning_Unit_OozeMephit,StartGamePregenFighterUnit);hostileInside=True;alliesInside=True;hostileNauseated=True;partyCasterClean=True;alliedSummonClean=True;mephitClean=True;gameFramesInside=5;areaTick=ok;insideAfterTick=5;gates[view=True;areaPos=(9.92,-6.00,-12.75);viewPos=(9.92,-6.00,-12.75);shape=ScriptZoneCylinder(radius=6.10,height=100.00,centre=(0.0...`.
- Cyclops: `ac:target=17;tabletop=19;difficulty=-2;modifiers=DexterityBonus:-1,Size:-1,NaturalArmor:7,Armor:4,Difficulty:-2;armorFact=True;natural7=True;noArmorItem=True;dex=8;size=Large;flash:uses=1->0;armed=True;untimedArming=True;saveWhileArmed=5;saveAfterSpentSameSeed=5;stillArmedAfterSave=True;armedNatural=20;hit=True;threat=True;confirmationRoll=1;confirmed=False;autoFlags=False;spent=True;nextNatural=1;nextMiss=True;secondUseAvailable=False`; across the save and the reload: `uses=0;available=False;armedStates=1;untimed=True;reloadedNatural=20;hit=True;spent=True;nextNatural=1;nextMiss=True`.
- Web: `web:spiderPos=(7.11,-6.00,-15.26);hostileSpot=dir0@3m;casterSpot=dir1@3m;web:spiderSize=Medium;oneLargerAllowed=True;twoLargerRefused=True;highTouchAc=114;usesBefore=2;webA:ended=True;frames=9;webB:ended=True;frames=6;rollA=type=RangedTouch,natural=10,bonus=5,targetAc=114,hit=False,weapon=RayItem;missedHighTouch=True;lowTouchAc=5;rollB=type=RangedTouch,natural=10,bonus=5,targetAc=5,hit=True,weapon=RayItem;webbedLowTouch=True;uses=2->1->0;thirdUseAvailable=False;stillWebbedHopeless=True;brokeFreeOverwhelming=True;immunityFact=True;directWebOnSpider=refused`.
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

Reviewer findings and closure: items 1-7 implemented exactly and proven
live above; item 8 (documents) audited in this commit; item 9 (this
requalification) on the one candidate commit; item 10 kept (draft PR #23,
unmerged). Nothing was reclassified as an accepted deviation. Internal
acceptance on this evidence; HumanReview: NOT_PERFORMED_NONBLOCKING.
OwnerDelegationGranted.

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

PASS - Sprints 3-8 corrected and requalified on candidate commit `2f04baf4`; draft PR #23 is ready for owner review. No merge, release, permanent deployment or Sprint 9 under the orders of 2026-09-24 and 2026-09-25.
