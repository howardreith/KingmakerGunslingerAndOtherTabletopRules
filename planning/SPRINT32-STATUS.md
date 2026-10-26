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

Current validated development candidate is `0d22cf451b81fda21d1b2f1d5658eddef38262bd`:
ZIP `fa6f9dd1feb0dd850bad2dc12188c7075bb88064381435e993a433aa830edf2e`;
DLL `97839054fbc0c9382197de72e35454760be63976bec759e63b62b644a17c10a2`;
MVID `31283a46-90c0-4b74-a607-c6b6df25162c`;
SourceState `d12aa5a343dbad395c7410dd30ef8fadff377153a3b6ae52783d72b564421530`.
Path `artifacts/development-candidates/sprint32-lillend-support/0d22cf451b81/`.
Wrappers,557 selected/2753 registered checks, exact-reference compile and strict
package PASS. Development version0.0.167; no release qualification. Native49
FAIL before gameplay: Competence icon lookup expected an activatable, but the
captured exact native ID is a BlueprintFeature. Zero gameplay assertions ran.
The corrected feature lookup and carrier presentation are in the working diff;
the second Competence selection on this package is NOT_RUN. On8dd2f548, native48
is FAIL17PASS/2FAIL: all existing support/body and Lullaby checks pass, while
Daze repeat/refusal and aggregate execution fail. The original47 precondition
failure and42 unexecuted package remain in history. Lullaby now uses the proven
owned native Spell/Cantrip fact with live bardCL/Charisma and no slot debit.
Input evidence stays bound to4b509d67. Current working edits are newer than this
candidate and are not represented by its execution.

Current source also corrects the newly identified Daze limitation: private native
Spell/Cantrip/effect mapping grants60s nondispellable immunity on successful
affection, blocking private and unchanged native Daze effects. It retains the
printed one-round condition and Will/SR; other native spells are unchanged.
[Daze rule](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Daze).
The test waits for actual native immunity expiry before a second natural cast,
checks remaining summon lifetime>=80s, and isolates grab with the existing owned
maneuver-immunity fixture condition. Independent review found no product blocker;
all9focused tests and corrected production compilation PASS. Native48 observes
one natural Standard/DC14/SR5 cast,5.96s Dazed and59.96s immunity with natural
expiry. Cross-source refusal and a second cast are unverified. Review found a
copied target-checker cache requiring the existing reset helper, plus a fixture
gate that starved native reconsideration between repeating melee commands.
Both corrections are in progress; original receipt remains FAIL.

| Evidence | Result and dependency limit |
|---|---|
| Fresh8dd2f548 Lullaby slice | PASS natural Standard/CL7/DC14/Will/SR,42s tail,-5 Perception,-2 only Will vs Sleep; allied overlap, Sleep immunity and active-effect refusals; owner/sibling budgets unchanged. Target-buff-only original-deadline seam passes with clock restored. Overall native48 remains FAIL for Daze. `artifacts/sprint32-lillend-support-8dd2f548/` |
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

Latest transaction `compat-20261026T054756Z-6bd74afafe7f` restored/verified
05:51:45.0228680UTC. Latest protected Compare PASS:
Mods967/saves118/references18/settings83/tasks0/one unchanged Steam.
No owned live transaction/game/task remains. Original Unity logs retained.

Build attempts/validated development packages/native launches:44/33/49.
Latest build260.408s; input wrappers278.947s/299.582s/305.221s;
native106.262s/129.664s/138.045s. For03:07-03:46, approximate implementation/
records10min, observer review/repair8min, approved-feature contract research10min,
build/package4.3min, native setup/execution/restoration14.7min, storage4min;
activities overlap and are not additive. Earlier complete totals unavailable.
All owner scope decisions are now answered; no waiting treated as approval.
Disk minimum remains1.1GB peak plus2GB margin. Verified closed-build duplicate
cleanup reclaimed1,138,981,599B with protectedComparePASS. The five closed input-proof extraction copies were byte-verified and removed
03:48:39.5915213UTC, reclaiming496,600,365B; protectedComparePASS.
Free3,821,506,560B immediately after cleanup;3,921,928,192B at04:05UTC.
Unique packages/PDBs/raw evidence/receipts/backups/saves remain retained.
Latest build42 took261.969s and was retained without native dispatch. For04:39-
04:55, approximate implementation/fixture8min, review/research4min, build/package
4.4min, storage2min; overlapping, not additive. Six verified closed build copies
reclaimed127,303,638B at04:53:47.3116800UTC; free3,496,779,776B and protected
ComparePASS. No owner wait or native launch in this interval.

Latest build43 took322.869s; native48 wrapper410.741s including restoration.
Closed47 extraction cleanup reclaimed99,387,310B with exact protectedComparePASS;
prelaunch free3,192,594,432B. Post48 free2,962,231,296B is below the3.1GB working
minimum: obtain verified duplicate headroom before the next build/staging.

Build44 took269.557s;557selected/2753/exactcompile/packagePASS. Native49 wrapper
230.344s/native66.773s failed product registration, not Daze or performance.
Two verified cleanup batches recovered184,386,692B with exactComparePASS.
Build began with3,138,617,344B free. Its147,146,529B held outputs were counted once
against the1.1GB combined allowance before native49;2GB margin remained beyond
the remaining peak. Post49 free2,783,006,720B: renew safe headroom before another
package using only verified duplicates. No known-broken-package second launch.

Next: finish the reviewed Daze correction and approved Competence dispatcher,
compile and review the coherent batch, then run the exact support and new
performance selections (expected10min each,600s runtime ceilings). The latter
uses the existing native Default lease and real outside-combat round controller.
Fascinate feasibility is narrowed to physical90ft sight beyond native acquisition
range; no combat-delay behavior or arbitrary neutral targeting is authorized.
Competence is implemented in the working diff: production compilation23s and
6focused tests4.1sPASS. Independent review confirms the pending-command,
recipient-loss and sight corrections and clears the bounded first native proof.
Combat-entry cancellation and fresh-process persistence remain required; no
native Competence result exists yet. For05:14-05:38 approximate implementation/
fixture18min, read-only review/research8min, compilation2min, records/storage3min;
activities overlap and are not additive. No owner waiting or new native launch.
Batch coherent mechanics and existing fixture adaptations. Required remaining
cases include Satyr Sleep/control, Lillend control/constrict, gameplay presentation,
cold ON/OFF persistence and directly affected integration. Reuse input evidence
only after checking actual changed dependencies. Final qualification still needs
remaining coherent evidence review and one frozen complete-gate candidate, with
separate records where identity guards require them. Do not start Sprint33.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
