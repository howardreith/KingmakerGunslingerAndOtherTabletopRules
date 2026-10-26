# Sprint 32 - Satyr and Lillend Azata

Owner Howie Reith; mission2026-10-25. Sole active operational authority.
Finish at an independently reviewed, natively validated PR; no merge, release,
Sprint33 or unapproved subsystem. [Validation policy](../docs/VALIDATION-POLICY.md).

## Scope, ownership and decisions

Accepted intake master `48e6fb4abfc154f111a4019a806cb79a4184bd1c`.
Branch `codex/expanded-summoning-sprint32-satyr-lillend`;
worktree `C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/.worktrees/expanded-summoning-sprint32`.
[Draft PR74](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/74).
HOWARD-LAPTOP owns mechanics/shared gameplay integration. DATA's released-creature
art and other worktrees remain untouched. Review relevant accepted dependency
changes at the coherent pre-freeze checkpoint.

Charter v1.0 sections3/4/7 and S32: Satyr SNA IV-IX; Lillend SM VI-IX;
quantities1/1d3/1d4+1; dependencies5,29-31. Preserve native action economy,
resources, ownership, cleanup, saved identities and accepted ordinary-map flight.

- Execute and inspect required native/technical visual checks unattended.
  Owner aesthetic approval is separate; no manual owner playtest dependency.
- Owner explicitly omitted Countersong and Darkness; disclose both. No substitute fog.
- Suggestion conditional on active CowWithHat's Custom Spells; omit when absent.
  Its fixed avoid-combat/fascinate-until-harmed behavior, minutes/level, Will/SR
  is the conditional adaptation; unrestricted tabletop instructions are not claimed.
- **Approved Lullaby:** normal cast with printed10-foot burst, Will/SR,
  -5 Perception and -2 Will against sleep; immediately release concentration,
  leaving the CL7 seven-round tail. No sustained concentration subsystem. Native48 proves the RTWP cast, modifiers, refusals and original deadline; final qualification remains pending.
- **Approved Lillend-only noncombat dispatcher:** bounded choices before acting
  for Fascinate (up to3 eligible targets), Inspire Competence and dependent bardic
  Suggestion. Preserve normal actions,20-round native performance pool, one
  performance at a time, exact-source ownership, refusal and cleanup. No global
  AI/control or general performance expansion. Implementation/validation pending.
- **Approved omissions:** Light, Dancing Lights, Ghost Sound, Hallucinatory Terrain
  and Detect Magic. Disclose limitations; no illumination, illusion/disbelief or
  aura-sensing subsystem. Countersong/Darkness omissions above remain unchanged.
- Exact branch addition to sanctioned push wrapper authorized/applied. No release
  number reserved. Development version0.0.167 remains explicitly candidate-bound.
- Guarded Steam640820 transactions, current protected intake, immutable baseline,
  leases, exact source/ZIP checks and restoration remain mandatory. Final qualification
  uses frozen source plus separate records worktree where identity guards require it.

## Current implementation and evidence

Satyr has the adult Fey8/HP44/CL8 profile, Fear/Charm Person/Sleep, conditional
Suggestion, eligibility/refusal policy and bow/dagger/secondary-horns fallback.
Lillend has adult Outsider7/HP73/AC20, Large +1 sword/secondary tail with accepted
shared grab/constrict, bard7 book5/4/2, independent20-round Move Courage,
healing/control and ally-safe Sound Burst eligibility. Ten legal roots are
published for development;1150 prior roots remain. Product scope and final
qualification are incomplete, including the approved additions above.

Supported-profile limits: native Persuasion/Perception combine printed social/
Sense Motive skills; Knowledge(nature) uses own Intelligence. No Survival,
Disguise, Perform or Fly stat is substituted. The accepted ordinary-map airborne
adaptation has no separate Hover fact or aerial maneuverability consumer.
Source profiles: [Satyr](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Satyr),
[Lillend](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lillend).
Printed utility omissions above are now explicitly owner-approved. Do not infer
complete printed spell/feat/skill coverage from these adult stats. Lullaby has focused compilation and native RTWP proof on the candidate below.
Noncombat performances remain required implementation, not pending permission.

Current validated development candidate is `7aed020a82f3214478cffdfc8d7863b8e2c32971`:
ZIP `3d02f64a71e55d6613a0d7f7614c144052b1cc13cbfd573713ba9bddbe75583c`;
DLL `d7e6baa08cb585ee7320c710c27c9d75c572514bb0c66b9fb78636b820c6091a`;
MVID `5fc3dea1-ff5a-48cd-b8e9-37ab3c6f80d9`;
SourceState `a5039d7ba63373097f6745ebbb588b90b5bdd54d4e67748b14ee13e4519afeef`.
Path `artifacts/development-candidates/sprint32-lillend-support/7aed020a82f3/`.
Wrappers,557 selected/2753 registered checks, exact-reference compile and strict
package PASS. Development version0.0.167; no release qualification. Native52
Competence FAIL4PASS/2FAIL: its dispatcher was not bound to a live fact, so the
fixture refused before dispatch. Native53 narrowed cantrips PASS7/7, including
two natural Daze casts and the actual immunity lifetime. Original failures stay
immutable. The periodic dispatcher/load-hook correction is newer than this
candidate and has no native verdict. Input evidence stays bound to4b509d67.

Current source also corrects the newly identified Daze limitation: private native
Spell/Cantrip/effect mapping grants60s nondispellable immunity on successful
affection, blocking private and unchanged native Daze effects. It retains the
printed one-round condition and Will/SR; other native spells are unchanged.
[Daze rule](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Daze).
The test waits for actual native immunity expiry before a second natural cast,
checks remaining summon lifetime>=80s, and isolates grab with the existing owned
maneuver-immunity fixture condition. Independent review found no product blocker;
all9focused tests and corrected production compilation PASS. Native48/51 observe
one natural Standard/DC14/SR5 cast, initial5.96s Dazed and59.96s marker duration.
Their old expiry flag is not proof: early removal also passed it after60s.
Native ContextActionApplyBuff defaults to child ownership, removing the marker
with the6s condition. Current correction explicitly sets AsChild=false. The
Dialog fixture also omitted native round ticks, leaving the AI's one-round
cooldown uncleared. A case-owned adapter now calls the native controller inside
the existing clock; it never resets cooldowns or changes AI scores. Expiry must
be bracketed by actual live-marker presence and first absence. Native53 proves cross-source refusal, true expiry and recast: the marker remains
53.978s after Daze ends, is last present at43270.567 game seconds, and first
absent at its original43270.597 deadline. Native round cooldown clears1-to0;
the second natural cast has normal Standard cost, CL7/DC14/Will/SR. No slot
replenishment, forced rolls or fixture cast commands. Lullaby also passes.

| Evidence | Result and dependency limit |
|---|---|
| Fresh7aed cantrips | Native53 PASS7/7: Lullaby natural cast/modifiers/refusals/original-deadline seam; Daze natural twice, actual60s immunity, cross-source refusal, native cooldown and recast. RTWP only; no broad support/persistence claim. `artifacts/sprint32-lillend-cantrips-7aed020a/` |
| Reviewed reuse:bad22421 Lullaby slice | PASS natural Standard/CL7/DC14/Will/SR,42s tail,-5 Perception,-2 only Will vs Sleep; allied overlap, Sleep immunity and active-effect refusals; owner/sibling budgets unchanged. Target-buff-only original-deadline seam passes with clock restored. Overall native51 remains FAIL for Daze. `artifacts/sprint32-lillend-support-bad22421/` |
| Reviewed reuse: bcd support lifecycle | PASS15/15. Both scheduler expiries, death cleanup after native RecentlyDeadBuff expires, external Courage removal, sibling isolation and exact reset pass. Death waits12.065/12.104wall seconds,12game seconds. `artifacts/sprint32-support-lifecycle-bcd41d04/` |
| Reviewed reuse:51cf Lillend support | PASS13/13. Natural Courage, selfheal and actual party-ally heal18-to0; independent level3 slots2-to1-to0, Standard cost plus exact free first-round touch delivery; sibling untouched; offensive fallback and owned body cleanup. `artifacts/sprint32-lillend-support-51cf0e58/` |
| Fresh4b509d67 native input | Wizard16/16, Sorcerer28/28, Druid31/31 PASS. Real visible menu/target input: prepared Lillend; spontaneous Lillend One/d3/d4+1; prepared Satyr plus converted One/d3/d4+1. Exact source slots/actions/legal quantities/120s/zero bonus and subcase reset/UI finalizers. No supplied acts or manual command ticks. `artifacts/sprint32-{wizard,sorcerer,druid}-input-4b509d67/` |
| Reviewed reuse:2d90 RTWP12/12 and turn10/10 | Natural Move Courage and actual ally+2/enemy exclusion; Standard self-healing18-to0/own level3 slots2-to1/sibling2; useful fallback; completed sword/sword/tail acts; live Nature and sibling isolation. Player-ally healing is established separately on51cf. |
| Reviewed reuse:392 public prepared routes | Satyr six SNA tiers and Lillend four SM tiers; legal quantities, one slot per cast,120s duration, body/icon/selection. Helper pumps native commands; no natural-input timing claim. |
| Reviewed reuse:Satyr control and optional Suggestion | Fear, immunity refusal and bow/dagger/horns fallback in both modes. Provider Suggestion natural DC17/CL8 Satyr and DC16/CL7 Lillend, normal actions/duration/own slot and active-effect refusal. Positive Will/high SR, expiry/damage break and provider cold load NOT_RUN. |
| Body evidence | Independent Lillend authored bindings;62 owned objects released/158 borrowed retained; recognizable rest views. Full gameplay camera motion/contact/hit/death/despawn/performance acceptance remains pending. |

Exact tuples, raw failure paths, reader corrections, original logs and review
findings are retained in [append-only history](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md).
Original Lillend [provenance](../assets-source/original-models/sprint32-support/README.md)
and functional-first cosmetic policy remain; no owner aesthetic approval claimed.

## Finite acceptance contract

All final rows remain incomplete. Early slices above do not replace final qualification.
Concrete review additions within A4/A5: Lillend tail hit/grab, immediate/maintained
2d6+live-Str constrict and death/expiry release, with one affected prior shared
consumer; Charm Monster nonhumanoid target, immunity/protection refusal, CL-day
duration and native break/lifecycle. These address actual new shared opt-in and
broadened private spell checks, not a whole-roster campaign.

| ID | Observable result | Route and evidence | Mode/profile | Invalidated by |
|---|---|---|---|---|
| A1 | Adult identity, defenses, attacks, legal facts, spell lists; exact legal tiers and quantities; existing routes retained | Domain catalog/route tests; native composed-profile and inventory observations | Offline; standalone native | Catalog, builders, registration, adapters |
| A2 | Both public summons pay real cost/slot, produce legal count and duration with usable body/icon; distinct prepared/spontaneous/adapter paths covered where materially different | Existing player-route helper with bounded Sprint 32 cases; structured slot/spawn/expiry receipts | Standalone; affected casting-source integrations | Publication, costs, quantities, duration, adapter generation |
| A3 | Satyr naturally uses useful control without pipes; rejects immune/ineligible targets and avoids repeats; attacks when control is unavailable | Existing disposable first-area fixture and exact-owner AI/command observers; control/refusal/fallback records | RTWP and turn-based | Satyr rules, targeting, shared AI/action handling |
| A4 | Lillend naturally supports with safe buffs/performance and healing; resources/slots and actions debit correctly; ordinary offensive fallback remains useful | Same bounded native fixture; native action/effect/resource observations | RTWP and turn-based | Spellbook, performance mapping, support AI, actions |
| A5 | Expiry, death and dismissal/cancellation leave no owned items/effects/commands; owner/sibling state is isolated | Existing lifecycle/quantity fixture; exact ownership and terminal observations | Standalone; affected modes | Lifecycle, equipment, resources, effects, commands |
| A6 | Fresh-process save/load preserves required serialized state, safely resets session state, cleans up; module OFF deserializes safely; baseline remains compatible | Working prepare -> cold verify/cleanup -> fresh absence, ON/OFF, with original bytes/metadata restored | Installed stack required by Working | Serialized state, registration, lifecycle dependencies |
| A7 | Loading, selection, scale, movement, attack contact, hit/death/despawn and performance are technically usable at gameplay camera | Existing native creature-review/presentation route, agent-inspected captures plus structured facts | Standalone and relevant combined seam | Assets, renderer, attachment, loading, lifecycle |
| A8 | Standalone and directly affected high-risk combined integration work; changed shared consumers retain behavior | Selected existing profile transaction and focused baseline/control cases | Standalone; high-risk combined; additional profile only for concrete dependency | Shared AI/registration/serialization or changed integrations |

## Current work, restoration and next action

The separately identifiable reusable workflow repair retains explicit candidate/
version binding, shared scenario declarations, guarded staging/restoration and
positive/negative admission checks. Concluded-campaign selectors/quotas no longer
block Sprint32. Prior independent tooling findings were corrected. Final coherent
product/rules/coverage review is still pending.

The live StickyTouch source capture passed self/party-ally healing on51cf.
The corrected input observer passed all three4b509d67 paths. Wizard/Sorcerer
actually have two modeled branches, each naming the exact Lillend, but one native
spawn action; Druid has one modeled leaf. Original aggregate failures remain
immutable. Independent review accepted the corrected graph/cost/slot contract.

Private Satyr at-will SLAs explicitly use Standard/non-full-round; existing
runtime mapping assertions and natural Sleep cost remain to execute. Lillend's
bard spells are unchanged. [SLA rule](https://www.aonprd.com/UMR.aspx?ItemName=Spell-Like+Abilities).
Independent dispatcher review identified native targeted Competence ownership,
private Fascinate cap/source-immunity/threat/-4 Perception corrections, and an
exact-source bardic Suggestion threat exception/DC17/no-slot adapter. No general
performance or testing infrastructure is authorized or planned.

Latest transaction `compat-20261026T072236Z-4cffc6a90f07` restored/verified
07:28:12.7557475UTC. Protected Compare PASS: Mods967/saves118/references18/
settings83/tasks0/one unchanged Steam. No owned live transaction/game/task.
Original native52/53 Unity logs and raw results are retained.

Counts:46 Build-Local attempts /35 validated development packages /53 native
launches. Build46 took256.932s; native52 wrapper250.748s/native88.623s;
native53 wrapper338.250s/native171.266s. Full inexpensive2753-test final gate
has not run on these inputs. Earlier complete work-category totals unavailable.
For07:11-07:38 approximate implementation/fixture6min, contract review4min,
build/package4.3min, native setup/execution/restoration9.8min, records3min.
Activities overlap; no additive total/speedup claim. Owner waiting0.

Disk plan remains1.1GB combined peak plus2GB working margin. Verified closed
extraction/build duplicates reclaimed502,187,002B before Build45 and99,429,740B
after native50, with protected Capture/Compare PASS. Build45 began with
3,288,993,792B free; native51 began with3,111,096,320B. Unique packages, raw
evidence/failures, receipts, PDBs, saves and protected backups remain retained.
At07:09UTC,23 fully byte-verified closed extractions reclaimed1,471,067,833B;
free4,339,953,664B and protectedComparePASS. Never use the emergency floor
as the working target. Exact cleanup identities remain in history/local receipts.

Native50/52 disproved two dispatcher attachment assumptions. The repeated
approach was paused and native lifecycle IL inspected. BlueprintUnit only invokes
explicit entity-creation callbacks; arbitrary OwnedGameLogic there is not bound
into unit.Logic. A private hidden periodic Buff now owns the dispatcher, granted
and tracked by native AddFacts on Traits. Frequency=Rounds uses native buff
ticks outside combat; no eager dispatch before book/toggle creation. Removal
cancels only its exact pending command and owned Competence. Exact source handoff
and brain-disabled/normal-action guards remain. The fixture checks the actual
bound carrier, native frequency and nonspell/nondispellable flags.

The same contract review found ineffective optional-book OnTurnOn/PostLoad hooks.
Reconciliation now takes the exact unit explicitly after creation, provider
settling and native AddPostLoadActions (after spellbook load). It never Rest or
replenishes slots; provider transition/cold proof is still required. Focused
production26.7s/6tests5.2sPASS before the final creation-reconcile call. Independent
actual-diff review found no additional dispatch blocker. Icon catalog30/30PASS
after correcting the current registry hash pin; historical published pin retained.

Next: one coherent development build, then only
the changed lillend-performance early proof (expected10min;600s runtime ceiling).
Native53 evidence remains bound to7aed; this unrelated dispatcher correction does
not relabel it as fresh. Competence combat-entry cancellation and fresh-process
persistence remain required. No unchanged third dispatcher attachment retry.

Continue approved Fascinate/bardic Suggestion after the early dispatcher proof.
Its feasibility uses physical90ft sight beyond acquisition range and native
player-group engagement rules; no combat-delay or neutral-harassment workaround.
Remaining finite cases include Satyr Sleep/control, Lillend control/constrict,
gameplay presentation, cold ON/OFF persistence and directly affected integration.
Before final qualification: review relevant accepted dependencies, obtain actual
diff/evidence independent review, freeze source/assets/observers/publication,
run the complete inexpensive gate once, then use that exact ZIP with separate
records where guards require. No merge/release or Sprint33.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
