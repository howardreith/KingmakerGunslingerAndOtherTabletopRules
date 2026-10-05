# Sprint 16 pre-candidate review - Crocodile and Dire Crocodile

## October 5 takeover engineering review (NOT QUALIFIED)

### Current follow-up: crocodilian-only candidate evidence pack

The guarded `disposable-expanded-summoning-crocodilians` implementation now
extends the original survey with focused live mechanics and twelve real-command
cells. It has **NOT RUN** in this form. The old seven-assertion survey remains
research evidence at its own source/DLL; it does not qualify these new cases.

| Gate | Added live measurement | Current disposition |
| --- | --- | --- |
| Exact profiles | Unmodified scores, size/HD/HP, AC/touch/flat-footed, saves, CMD/trip breakdown, bite/tail attack and damage, secondary flag, Improved Critical, final skills | Written, NOT RUN |
| Bite-only grab | Actual tail and bite RuleAttackWithWeapon events, exact establishing weapon, one hit bundle and no initial rider | Written, NOT RUN |
| Death Roll | Both base lines; +4 Strength and penalty to score 7; native Animal Growth; native Flaming/+1 with the energy description reordered ahead of the base; real bite versus Death Roll through native Stoneskin | Written, NOT RUN |
| Maintain selection | Fresh hold, own size, smaller size, target growing ineligible, prone/already-prone/immunity; real maintain check counts, replay refusal, live hold component round guard, lethal cleanup | Written, NOT RUN |
| Swallowed cadence | Initial bite count; initial native next-tick delay; three BuffCollection-delivered rounds and duplicate scheduler ticks; source ownership; native six-second escape gate/check; zero removal damage | Written, NOT RUN |
| Sprint speed/time | Native queued/executed ability; Haste both orders, Slow, -5 and floor-reaching penalties, native command speed cap; six-/sixty-second buff deadlines, all intermediate rounds and recast | Written, NOT RUN |
| Commands/AI | Both creatures x RTWP/turn-based x manual, fresh AI, AI after a player cast; movement, bite/tail events, actual AI command identity, one Sprint/no failed queue spam, shared cooldown | Written, NOT RUN |
| Targeted persistence | Closed scope in existing guarded three-stage workflow; five summons, active/cooldown pairs, skills/visuals, hold/swallow reset, native buff expiry and saved cleanup | Written, NOT RUN |
| Other lifecycle | Swallow/hold source-death/dismissal/expiry/transition; Worm/Flytrap regression controls | Still to finish |
| Icon consumers | Nine visible consumers mapped to three original emblems; five AI internals explicitly inventoried; offline/profile/protection PASS | Live bindings written, NOT RUN; native UI use still open |
| Original art | Visual lifecycle, jaw/tail contacts, crowd/fallback/resource controls, native Monitor Lizard | Still to finish |
| Routes/publication | Fourteen changed Crocodile roots and six private Dire roots, then publication-only gate and 976/29/1005 totals | Still to finish |

The command cells never write Animation.IsActed or call command.Tick to invent
a contact. AI cells retain their production action list and receive no fixture
attack command. In the player-cooldown AI cell, only its next-decision timestamp
is held until the player's queued Sprint installs the cooldown, then released.
The full-attack cells make only the disposable target maneuver-immune, to keep
grab from correctly truncating the bite/tail sequence; positive bite grab has
its own immunity-free case. Native RNG is preserved around the synchronous
mechanics pack. Buff scheduling tests move only exact disposable deadlines,
never the campaign clock. These fixture choices are disclosed, not substitutes
for a PASS result.

Inner-loop checks: 215/215 focused summoning tests (2017 registered), active
static/icon validators and incremental exact-reference Release compile PASS.
Full suite/clean build/package/guarded qualification remain deferred to the
stable hidden candidate. No new game launch or publication change.

### Current follow-up: native Sprint AI engagement gate

The narrow survey passed its seven assertions; see the separate survey review
for census/graph/skill hashes and the activated interior limitation. Both
original models/paintings now have deterministic exports and offline review.
Those results do not qualify actual combat, Sprint AI or visual lifecycle.

The native `IsEngagedConsideration.Score` reads
`context.Target.Unit ?? context.Unit` and its actual combat engagement flag.
The type extends BlueprintScriptableObject; no library-indexed existing
instance was found. One exact owned native instance is therefore registered
under `KMG.Summoning.Special.Crocodilian.SprintNotEngaged`,
`5c4c807c1f274ef3b82ff16f9f114cf8`, with scores engaged=0/free=1 and multiplier=1.
Only the two crocodilian Sprint AI actions reference it. This is native
scoring configuration, not a new/global AI subsystem.

The pure append helper preserves the native brain's action references and
order, adds exactly one Sprint, refuses duplicate/null entries and never
mutates the native array. The native BlueprintBrain has only Actions beyond
its base blueprint fields; there are no omitted brain-specific fields.
The census now explicitly traverses inline brain/action considerations and
records native/owned action lists, because the generic component graph did not.
Actual movement, bite/tail, cooldown fallback and both modes remain live gates.

Checks: focused 215/215 PASS (2017 registered), incremental Release compile
PASS. The manifest has 1848 summoning identities, 2836 total (2834 active,
2 reserved). The consideration is hidden internal, with no icon consumer.

### Follow-up: preparing the narrow live survey

Sprint now uses a temporary +20-foot UntypedStackable modifier. The audited
native BuffMovementSpeed seam modifies speed; it exposes no temporary
base-speed-to-40 override. Saving/restoring mutable BaseValue would conflict
with other base changes, so the owner's second fallback is used. This is a
bounded CRPG adaptation, not a claim that Haste/Slow/caps are qualified.
The per-creature brain retains the configured natural brain's components and
actions and appends Sprint; its AI cooldown is zero because the same native
cooldown buff gates player and AI. Adjacent-use scoring and live AI proof remain
open pending the native consideration census.

The rank allocations are Crocodile Perception 1 / Stealth 2, Dire Crocodile
6 / 6, Mobility 0 for both. Native AddClassLevels can only repeatedly spend
through a fixed priority list, not express these allocations. A component on
the exact two owned units writes their initially empty rank base values once
at creation; it has a serialized applied guard and does not run at reactivation.
Native class-skill, ability, size and Skill Focus modifiers retain control of
totals. Other profiles and donor blueprints are untouched. Both profiles
explicitly omit aquatic Swim and water-only Stealth.

At the historical research checkpoint, `disposable-expanded-summoning-crocodilians`
was a narrow survey, requiring the normal guarded `KMG_AUTOMATION_WORKING` load.
It reuses the existing disposable correction fixture, records current native
swallow types/members and complete simple action graphs, Run candidates,
engagement considerations and speed buff graphs, checks live land skills and
captures only Monitor Lizard renderer-local bind frames. It writes no save,
changes no publication and runs no historical-root census. It must not be
reported as the completed combat/Sprint/visual qualification pack.

Follow-up checks: focused Sprint 16/correction checks 21/21 and focused
Sprint 16/scenario-wiring checks 17/17 PASS (2012 registered); incremental
Release compile PASS before the additional skill-breakdown fields. Full
pre-launch source/package validation is next, then the narrow guarded survey.

The following source corrections are descendants of intake
`a762ece553ab9b539a53a782102a29e5c70d44eb`. They are not a hidden runtime
candidate. Full gate, package, graph/cadence proof and all live cases remain
pending until the Sprint 16 source, art and scenarios are stable.

- Native `RuleCalculateWeaponStats.OnTrigger` writes its computed base damage
  description at index zero (exact-reference IL offsets 03ad-03b6). A local
  derived rule now captures that object's identity immediately after native
  construction, before after-rule subscribers can reorder it. The native
  subscription manager walks base types, preserving ordinary stats handlers.
  Unique reference lookup, not final index or physical type alone, identifies
  the bite. Missing, duplicated or nonphysical captures fail closed.
- `ModifiableValueAttributeStat.Bonus` derives from `ModifiedValue / 2 - 5`;
  native weapon stats uses the same getter. Death Roll records modified
  Strength, modifier, before/after dice/bonus and captured index. Only the
  positive extra half reaches the base bite. The weapon-aware DamageBundle
  constructor also sets WeaponDamage and WeaponSize; assigning Weapon alone
  did not. Live enchantment, material, DR and negative modifier cases are open.
- The actual establishing bite and reciprocal held target are mandatory.
  The crocodilian maintain claims its held-round counter before rolling;
  replay cannot roll another check or rider. A lethal roll releases its link.
  Death Roll triggers weapon stats and damage, not another attack or on-hit
  event. These are source findings, not live rider/cleanup proof.
- Bounded command-seam audit: the existing summon hold is driven by
  `SummonHoldComponent.OnNewRound -> MaintainLink`; its native grapple parts
  hold relationship state, and its existing attack-command seam is rake, not
  a choice between maintain riders. No existing bounded held-target command
  selector was found. Do not build a reaction/menu subsystem.

`OwnerAcceptedAdaptation: SWALLOW_ELIGIBLE_TARGET_ELSE_DEATH_ROLL`.
On a successful later-turn maintain, a Dire Crocodile swallows an eligible
target at least one size smaller; otherwise it Death Rolls a legal same-size
target. An ineligible target gets neither special rider. The policy is chosen
from pre-roll ownership/age/size state and can resolve only one rider. This is
the owner's deterministic RTWP/AI adaptation, not exact tabletop choice;
actual maintain-path runtime proof remains open.

The existing recorded native swallowed graph
(`20261002T1440160968919Z-observe-expanded-summoning-native-donors`) has:

| Component | Timing / effect |
| --- | --- |
| AddFactContextActions | Activated empty; Deactivated empty; NewRound one physical bludgeoning 4d8+12 action |
| AddStatBonus | AdditionalCMB -2, UntypedStackable, not BAB-scaled |
| AddStatBonus | Dexterity -4, UntypedStackable, not BAB-scaled |

Dire Crocodile now verifies and clones only those two stat penalties, then
constructs empty Activated/Deactivated lists and exactly one NewRound physical
bludgeoning 3d6+13 action. No donor action graph is mutated or retained, nested
or otherwise. Native UnitPartSwallowed still owns escape. The initial bite
remains in SwallowHeld. A fresh recursive live graph and damage-event audit
must prove later-round cadence, no application-frame double hit, no damage on
removal and unchanged Purple Worm/Flytrap. The existing donor observation is
not the new live census required to activate
`SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`; that decision remains conditional.

Validation repair discovered at takeover: commit `6b83eecd` emptied the active
0.0.141 validator. It is restored, with a dispatcher regression fixture for
empty/missing source. Its stale roster/identity/icon/package-count records and
the icon catalog's stale ledger hash are reconciled to existing source:
95 creatures, 1847 summoning identities, 2835 total identities, 107 icons,
976 registered / 970 published / 6 withheld roots. No GUID or artwork changes.
Accepted Sprint 14-15 runtime evidence is retained; this repair does not
retroactively claim their old empty static check performed validation.

Remaining implementation: Sprint speed and AI interaction, exact land skills,
Run disposition, original crocodilian assets and icon-consumer/UI review,
runtime scenario matrix and private donor bind frames. Runtime and owner
visual approval are separate; `HumanReview: NOT_PERFORMED_NONBLOCKING`.

Written while the Sprint 15 guarded batch ran, from source reading only. The
point of doing it here is the standing rule that the adversarial rules and
architecture review happens *before* the expensive candidate, so a
misunderstood signature is not discovered by a runtime matrix.

## Findings that change the implementation

### 1. Replacing the Crocodile's proxy touches no identity, and its label stays

**Corrected after filing.** The first version of this section said to drop the
trailing `"Monitor Lizard"` argument from the Crocodile's catalog entry. That
was wrong, and following it would have broken a pinned count while overstating
how much original visual work this project has finished.

`SummonCreatureSpec.Visual` is consumed in exactly one place -
`ExpandedSummoningBaselineInventory.ProxyVisualCreatures`, which counts an
entry as a proxy when `Visual` differs from `DisplayName` - and it has no
behavioural effect at all. Placements and GUIDs come from the append-only
ledger and are untouched by it, so the mission's requirement that the
Crocodile's identity survive is satisfied however the label is set.

What settles the label is the convention the repository already encodes. The
frozen proxy count is 32, and the test that pins it says in its own message
that *all five Sprint 14 and 15 insects borrow the Giant Spider* - those five
creatures ship original project meshes and are still counted as borrowing,
because they ride the donor's rig. The Sprint 11 ungulates, the Sprint 12
quadrupeds and the Sprint 13 creatures all keep their labels on the same basis.
The Giant Wasp and the Stirge carry no label, which is the one inconsistency in
the set and is not a reason to propagate the looser reading.

So the Crocodile keeps `"Monitor Lizard"`: it will still ride that rig, the
proxy count stays 32, and "replace the proxy" means ship an original mesh in
place of the borrowed body rather than relabel the creature. The remaining work
is the view patch's key map, the asset runtime's dictionary, the bone policy,
the view scale catalog and the four shipping lists - the last of which Sprint 15
proved is easy to forget and silent when forgotten.

`ExpandedSummoningDonorCatalog` keeps its Monitor Lizard row for the same
reason, and the Monitor Lizard is the right rig to keep: a low, wide,
four-legged body with a long tail is a crocodile's shape already, so the work
is a new mesh on a sound donor rather than a donor hunt.

### 2. Death roll has an exact native carrier; no barrier

Death Roll (Ex): while grappling a foe of its own size or smaller, on a
successful grapple check it deals its bite damage, knocks the target prone,
and keeps the grapple.

Every piece exists:
- the grapple check and the hold's maintenance are the Sprint 6 carrier,
  already qualified, and `ShouldSwallowOnMaintain` shows the shape of a
  maintain-time rider;
- prone is `UnitCondition.Prone` via `UnitDescriptor.State.AddCondition`,
  which this project already drives in `TwinShotKnockdownMechanics` - and that
  code verifies the engine accepted the condition rather than assuming it,
  which is the pattern to copy;
- the size restriction has a precedent in `IsSwallowSizeAllowed` /
  `ExpandedSummoningSpecialProfiles.IsSwallowSizeAllowed(targetSize,
  holderSize, delta)`; death roll's delta is 0 (own size or smaller) against
  the Purple Worm's -1.

The two risks worth instrumenting are named in the mission and are both about
cadence rather than capability: no duplicate damage when a round is processed
twice or an event replays, and no death roll without an exact valid held
target. Both have direct precedents - the Sprint 12 disease exercise replays a
runtime component to prove one resolution, and the rake/maintain work already
proves hold-target identity.

### 3. Sprint (Ex) has a native carrier

Once per minute, land speed rises to 40 feet for one round. `StatType.Speed`
takes stat bonuses and the project already writes them
(`ElementalAlternateTrait*` adds +5 Speed; the Elven Branched Spear subtracts
10). So this is a one-round +20 ft bonus with a once-per-minute resource, not
a new subsystem. It must not be a permanent speed increase.

### 4. Hold breath has no consumer and should be omitted with a reason

Rounds equal to four times Constitution, which only matters while submerged.
The mission forbids an underwater movement subsystem and restricts the
creature to a land adaptation, and Kingmaker models neither swimming nor
drowning, so nothing in the rules layer could consult it. This is the
luminescence case exactly: omit it, record the reason on the creature, and
never claim it works. It is not a candidate for the passive-sense label, which
covers only Scent, Darkvision and Low-light Vision.

The swim speed goes the same way as the ants' climb speed: Kingmaker exposes
one movement speed, the 20 ft land speed is used, and the 30 ft swim is
omitted.

### 5. The tail slap must be secondary, and the numbers say so

Crocodile: bite +5 (1d8+4) and tail slap +0 (1d12+2). The five-point gap and
the halved Strength bonus on the tail are a secondary natural attack, so the
tail belongs in `AdditionalSecondaryLimbs` via the `PS()` profile helper, not
in `AdditionalLimbs`. This is the exact mistake the owner rejected in Phase 1
when the Pony and Horse carried hooves as primary attacks, so it gets an
assertion that reads both limbs' live attack bonuses and requires the gap.

Dire Crocodile: bite +18 (3d6+13) and tail slap +13 (4d8+6) - the same
five-point gap, the same halved bonus, the same conclusion.

### 6. Reach

Crocodile is Large with Space 10 ft and Reach 5 ft, so it needs the
reduced-reach carrier the Large ungulates and the Giant Stag Beetle use. Dire
Crocodile is Gargantuan with Space 20 ft and Reach 15 ft, which is the
standard footprint for that size, so it must *not* carry reduced reach.

### 7. Swallow whole is the Purple Worm's carrier at a different size delta

Dire Crocodile: swallow whole 3d6+13, AC 16, 13 hp. Qualified carrier exists.
What is new is a second creature using it, so the size delta, the interior AC
and hit points have to come from the creature rather than from a Purple Worm
constant - the same class of defect as the Drone's poison difficulty class,
which the Sprint 15 pack proves is computed and not copied.

## Numbers to derive rather than assert

Crocodile CMD 18 (22 vs trip) and Dire Crocodile CMD 36 (40 vs trip). The
four-point trip difference on both is a four-legged trip defence, not the
eight-legged one the insects use: `TripDefenseFourLegs` already exists in the
fact table.

## Sequence

1. Freeze nothing new - the contract above is already frozen; this review adds
   no creature facts, only implementation decisions.
2. Death roll first, as a vertical slice on the Crocodile alone, before the
   Dire Crocodile is authored against it.
3. Meshes offline, one review sheet, then one sprint candidate and one batched
   guarded review.

---

# Sprint 17 donor survey - done here because it is tranche research

The mission requires the serpentine bind-frame proof before all three Sprint 17
visuals are committed to one implementation, and explicitly allows the
Salamander to be split off if it cannot share the snake seam. The donor
catalog, read offline, already answers the second question and narrows the
first.

The catalog's only limbless serpentine body is the **Purple Worm**
(`bf2216f48b3f4d24c9c502007649340d`). The Giant Centipede is a segmented body
with many legs, the Monitor Lizard is a quadruped, and nothing else in the
roster has a snake's topology. So the Viper and the Constrictor Snake should
both be authored against the Purple Worm's bind frame, and the proof to run is
that frame's capture plus one minimal vertical slice at each end of the size
range - a Tiny or Small Viper and a Medium Constrictor on a Gargantuan donor's
rig is a large rescale, which is precisely the kind of hypothesis the standing
rule says must survive a slice before a family is authored against it.

The **Salamander should be split off now rather than after a failed
experiment**. Its printed body is a humanoid upper half on a serpentine lower
half, it wields weapons, and its existing identity already ships wearing the
Lizardfolk (`e8276e28b2234a745900fed80670bfdb`), which is a humanoid rig with
arms that can hold a weapon. A Purple Worm frame has no arms at all, so forcing
one rig across all three would cost the Salamander its weapon handling - the
compromised universal rig the mission forbids. Keeping the Lizardfolk donor and
replacing only the mesh gives the serpentine lower body while preserving the
weapon seam, and it is also the smaller change to an identity that already
publishes.

That makes Sprint 17 two seams: the Purple Worm frame for the two snakes, and
the Salamander's existing humanoid donor with a new mesh. Recording the
decision now means the bind-frame proof only has to answer the snake question.

---

# Addendum: what the Crocodile already has, and what it is missing

Read off the shipped profile rather than assumed from the sprint's framing.

`ExpandedSummoningNaturalProfiles` already carries the Crocodile, and its
numbers are the printed ones exactly: Animal, 3 hit dice, Large, Str 19,
Dex 12, Con 17, Int 1, Wis 12, Cha 2, 20-foot speed, +4 natural armour, a 1d8
bite, and - correctly - a 1d12 tail slap in the **secondary** limb slot via the
`PS` helper rather than among the additional primaries. Point 5 of this review
is therefore already satisfied for this creature; it is the Dire Crocodile that
still has to be built that way.

Both of its weapons already resolve, so neither needs a new identity: `Bite1d8`
maps to a native 1d8 bite and `Tail1d12` to a project-owned weapon this
creature is the reason for. Its facts carry the reduced reach a Large creature
with 5-foot reach needs, the four-legged trip defence its printed CMD 18 and 22
require, and Skill Focus in Perception and Stealth.

What it is missing is the whole of its signature behaviour, and the profile says
so in a single deviation line:

> "Grab, death roll, sprint, and hold breath are omitted because no
> duration-bound summon-safe native graph was proven."

That line is the sprint's real target, and it is the shape of record the owner
rejected in Phase 1: an omission written down as a deviation and counted as
completion. Three of its four items have carriers this project already drives,
as the sections above establish - grab is the qualified Sprint 6 carrier that
a dozen creatures use, death roll is a grapple-check rider plus the native
prone condition, and sprint is a one-round Speed bonus with a once-per-minute
resource. Only hold breath has no consumer, because Kingmaker models neither
swimming nor drowning, and that one stays omitted with its reason stated
exactly rather than bundled with three implementable abilities.

So Sprint 16 does not begin by writing the Crocodile. It begins by registering
the Dire Crocodile, and then by replacing that one deviation line with three
implementations and one honest omission.
