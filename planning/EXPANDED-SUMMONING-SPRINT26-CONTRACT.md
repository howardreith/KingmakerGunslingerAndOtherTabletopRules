# Sprint 26 contract: Gorgon and Bulette

Status: IN_PROGRESS. This is a new owner-authorized campaign from master
555b16e04613c0800ab0e884beac827184bf46ee on HOWARD-LAPTOP. Sprint 25 is
closed at v0.0.159. No Sprint 27 work is authorized. Fixed production root:
C:/Dev/KingmakerGunslingerLab/wt/s26. No product rows are registered yet.

## Authority and sources

The local original Expanded Summoning Implementation Charter v1.0 DOCX,
Sprint 26 (page 22 in the owner assignment), specifies SM VIII Gorgon and a
bounded Bulette ambush/leap adaptation; it excludes generalized burrowing.
The current owner mission selects untemplated SM VIII Gorgon and SNA VI
Bulette, with only their normal higher-tier quantity roots. Gorgon is a
charter-selected extension, absent from the inspected ordinary core SM table;
Bulette appears on the ordinary SNA VI table. No new feat gate is implied.

Primary adult PF1 references, inspected 2026-10-18:

- [Gorgon](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Gorgon)
  and [legacy Gorgon](https://legacy.aonprd.com/bestiary/gorgon.html).
- [Bulette](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Bulette)
  and [legacy Bulette](https://legacy.aonprd.com/bestiary/bulette.html).
- [Summon Monster VIII](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Monster%208),
  [Summon Nature's Ally VI](https://aonprd.com/SpellDisplay.aspx?ItemName=Summon+Nature%27s+Ally+6),
  and [universal monster rules](https://legacy.aonprd.com/bestiary/universalMonsterRules.html).

The paired creature entries agree. Rules below are source facts or explicitly
identified implementation interpretations, not observations of native behavior.

## Profiles and attack roles

| Field | Gorgon | Bulette |
| --- | --- | --- |
| Type/alignment/HD | Large magical beast, N, 8d10 | Huge magical beast, N, 8d10 |
| HP | 100 | 84 |
| AC/touch/flat-footed | 20/9/20 | 22/10/20 |
| Natural armor | +11 | +12 |
| Fort/Ref/Will | +13/+6/+7 | +11/+8/+5 |
| Str/Dex/Con/Int/Wis/Cha | 24/10/24/2/16/9 | 23/15/20/2/13/6 |
| BAB/CMB/CMD | 8/16/26 | 8/16/28; 32 against trip |
| Ground speed/space/reach | 30/10/5 feet | 40/15/10 feet |
| Ordinary attacks | primary gore +14, 2d8+7; two secondary hooves +9, 1d6+3 | primary bite +13, 2d8+9, 19-20/x2; two primary claws +12, 2d6+6, 20/x2 |
| Feats | Improved Initiative, Iron Will, Power Attack, Skill Focus (Perception) | Improved Initiative, Iron Will, Skill Focus (Perception), Weapon Focus (bite) |
| Skills | Perception +17 | Acrobatics +9 (+17 jumping), Perception +11 |
| Printed senses | darkvision 60 feet, low-light vision, scent | same, plus tremorsense 60 feet |

Skills require their real ranks, class-skill, attribute and feat carriers.
Bulette's jump total combines its normal check, +4 racial jump-only bonus and
+4 from 40-foot speed. Live speed and skill modifiers must remain live; +8 is
not an unconditional Mobility bonus. Inherited unsupported senses remain
unmodeled at their actual scope; tremorsense must not become blindsight.

Gorgon's gore has the full live Strength modifier, not the engine's lone
natural-weapon 1.5 multiplier. Hooves keep secondary accuracy and damage.
Bulette's Savage Bite requires 1.5 times signed live Strength and 19-20 threat
with native confirmation even though claws are also primary. It is not Powerful
Bite, Power Attack, grab, swallow, rend or generic pounce.

## Gorgon staged breath and trample

Breath is a supernatural standard action: 60-foot cone, Constitution-based
DC 10 + floor(live HD/2) + live Constitution modifier (21 at baseline),
1d4+1 rounds between activations. No daily or once-per-summon limit.
Use lawful range, line of effect and existing ally-aware area targeting;
unsafe AI casts fall back to ordinary attacks. No global cone changes.

Exposure interpretation frozen before tests: an unmarked victim gets one
Fortitude save for the first exposure; failure starts temporary petrification.
That victim gets one recovery Fortitude save per round at the initiating DC.
A later distinct breath exposure while this exact Gorgon effect is still
active makes it permanent, without another exposure save. This follows the
specific second-exposure sentence; the generic first-exposure negation does
not insert an unstated second save. Permanent state has no natural recovery
saves. A cured/recovered victim starts a new exposure sequence.

Cross-caster interpretation: a distinct breath from any Gorgon qualifies for
escalation of this exact effect family. An unrelated petrification effect does
not count as a first Gorgon exposure. Preserve initial source attribution and
captured DC; record escalating source separately. One activation can affect
each target only once, even if native delivery repeats callbacks. Separate
instances own independent cooldowns. Activation identity and victim stage
must not depend on a retained live source object.

The victim owns a serializable native petrification condition/buff. Recovery,
actual native cure/removal, death/dismissal of the source, save hydration and
module-disabled hydration must be observed. Permanent effects do not expire
with summon duration. Fixture teardown removing its own state is not cure proof.

Trample reuses the released path/contact/response policy, full-round action,
2d8 plus 1.5 times a positive live Strength bonus, rounded down (a Strength penalty applies once), live Strength/HD DC (baseline
2d8+10/DC21), one contact per victim per round. Keep ally-aware responses and
ordinary attacks distinct; grant neither Overrun nor Cleave.

## Bulette legal surface Leap

A legal native charge may attempt one DC20 jump-specific Acrobatics check.
Success substitutes exactly four primary claw attacks, no bite, against legal
foes within current reach. It never adds attacks to the ordinary routine.
Failure interpretation: complete the legal ordinary charge's single attack;
the special four-claw routine is unavailable and the charge still spends its
action. Interruption or target loss grants no free attack, movement or refund.
Use real charge geometry, lawful landing/occupancy, native opportunity attacks
and native action spending in RTWP and turn-based mode. No cooldown/resource.

This sprint explicitly models the above-ground role on traversable terrain.
Printed burrow 20 feet is unmodeled, never added to ground speed. No underground
travel, untargetability, terrain destruction, wall/door/cliff bypass or teleport.
Visual jump/emergence can be short and rough without changing lawful movement.

## Publication and evidence boundary

Only six new roots: SM VIII one / IX 1d3 Gorgon; SNA VI one / VII 1d3 /
VIII-IX 1d4+1 Bulette. Preserve existing duration/alignment conventions and
all released GUIDs. Expected source-derived totals are 110 project creatures,
Monster 96/545, Nature's Ally 97/557, 1,102 generated placements, 29 retained
wrappers and 1,131 visible choices after publication; verify actual catalogs.
Keep both new keys withheld through qualification. No templates or variants.

Implement compact request-local native observers with mechanics. Final
qualification needs composed profiles, signed/modified damage/reach, staged
breath/cures/cooldown, success/failure/interrupted Leap, both command modes,
exact-owner AI, six actual roots, original bodies/icons, lifecycle/resources,
targeted persistence/module OFF/ON and standalone/high-risk combined controls.
The native planning envelope is about two hours, excluding implementation and
necessary failure repair/restoration. No historical whole-roster campaign.

Both original creature paintings were produced with the available built-in
imagegen tool after actual family pixels were supplied through conversation
image input. Provider/model details beyond the observed tool are unspecified.
HumanReview: NOT_PERFORMED_NONBLOCKING. Geometry is still unimplemented.
No functional or native qualification is claimed by this document.
