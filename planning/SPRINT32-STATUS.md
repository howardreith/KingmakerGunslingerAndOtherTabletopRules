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
- **Pending Lullaby decision:** recommended legal cast/immediate concentration
  release with7-round CL7 tail(2-5active hours), Lillend-only sustained concentration
  (8-20hours), or explicit omission. No implementation while pending.
- **Pending noncombat performance decision:** native summon AI selects in combat;
  Fascinate requires calm conditions. Lillend-only autonomous noncombat dispatcher
  (6-14active hours with uncertainty), or explicit omission of Fascinate, Inspire
  Competence and dependent bardic Suggestion. No provisional dispatcher/performance
  implementation. Conditional spell/SLA Suggestion is independent.
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
qualification are incomplete, including pending decisions above.

Supported-profile limits: native Persuasion/Perception combine printed social/
Sense Motive skills; Knowledge(nature) uses own Intelligence. No Survival,
Disguise, Perform or Fly stat is substituted. The accepted ordinary-map airborne
adaptation has no separate Hover fact or aerial maneuverability consumer.
Source profiles: [Satyr](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Satyr),
[Lillend](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lillend).
Printed utility spell/SLA omissions are undergoing final charter-filter
reconciliation; do not infer complete printed spell/feat/skill coverage from
these adult stats. Lullaby and noncombat performances retain pending decisions.

Current validated development candidate is `6faa9ba369808bc6c12f90c3b6bb77109e4ec617`:
ZIP `236eb0a389f208d88faed0bebb3accb293be0eab95faa5a13832c6122cd23be8`;
DLL `b68183a6c13be6dddf0f4a739c1ca58144db3a9ac314c484fd7b5ce22d48350d`;
MVID `c24be662-0898-4235-a959-203682cb902f`;
SourceState `ac721bda9809c62573eeb9c7b0c7db2e6bfedebd5bfb7a0a7f22a9e5a7a1d908`.
Path `artifacts/development-candidates/sprint32-lillend-support/6faa9ba36980/`.
Wrappers,557 selected/2753 registered checks, exact-reference compile and strict
419-file package PASS. Development version0.0.167; no release qualification.
Current observer corrections are newer source, not execution on that package.

| Evidence | Result and dependency limit |
|---|---|
| Reviewed reuse: bcd support lifecycle | PASS15/15. Both scheduler expiries, death cleanup after native RecentlyDeadBuff expires, external Courage removal, sibling isolation and exact reset pass. Death waits12.065/12.104wall seconds,12game seconds. `artifacts/sprint32-support-lifecycle-bcd41d04/` |
| Fresh6faa Lillend support | PASS10/FAIL2 overallFAIL. Actual selfheal18-to0/Standard/own slot2-to1 succeeds; new post-disposal StickyTouch attribution fails because native Ability.Dispose clears that link. Ally case not reached. `artifacts/sprint32-lillend-support-6faa9ba3/` |
| Fresh6faa Druid input | PASS28/FAIL3 overallFAIL. All4 real casts pass: preparedOne and convertedOne/d3/d4+1; exact combat cost/slot/legal quantity/120s and unit resets. Final UI restoration aggregate fails, while outer installation/save/profile restore passes. Exact failing UI conjunct not recorded; Wizard/Sorcerer held. `artifacts/sprint32-druid-input-6faa9ba3/` |
| Reviewed reuse:2d90 RTWP12/12 and turn10/10 | Natural Move Courage and actual ally+2/enemy exclusion; Standard self-healing18-to0/own level3 slots2-to1/sibling2; useful fallback; completed sword/sword/tail acts; live Nature and sibling isolation. Player-ally healing remains unverified. |
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
| A6 | Fresh-process save/load preserves required serialized state, safely resets session state, cleans up; module OFF deserializes safely; baseline remains compatible | Working prepare â†’ cold verify/cleanup â†’ fresh absence, ON/OFF, with original bytes/metadata restored | Installed stack required by Working | Serialized state, registration, lifecycle dependencies |
| A7 | Loading, selection, scale, movement, attack contact, hit/death/despawn and performance are technically usable at gameplay camera | Existing native creature-review/presentation route, agent-inspected captures plus structured facts | Standalone and relevant combined seam | Assets, renderer, attachment, loading, lifecycle |
| A8 | Standalone and directly affected high-risk combined integration work; changed shared consumers retain behavior | Selected existing profile transaction and focused baseline/control cases | Standalone; high-risk combined; additional profile only for concrete dependency | Shared AI/registration/serialization or changed integrations |

## Current work, restoration and next action

The separately identifiable reusable workflow repair retains explicit candidate/
version binding, shared scenario declarations, guarded staging/restoration and
positive/negative admission checks. Concluded-campaign selectors/quotas no longer
block Sprint32. Prior independent tooling findings were corrected. Final coherent
product/rules/coverage review is still pending.

Current bounded replan captures the live StickyTouch source at existing command
start and RuleHealDamage events before native disposal clears it. The unnecessary
nested-cast observer is removed; exact command cardinality, cost, own slot, native
healing source/recipient/blueprint and damage delta remain mandatory. Input setup
now proves the existing exact profile-owned, sole idle party anchor and suspends
only its brain before combat/Default; it restores that state afterward without
canceling commands or rewriting pins. Existing finalizer conjuncts are recorded.
Exact previous finalizer cause remains unproven; native combat exposed the anchor
to its own AI despite a strict original-empty-command requirement. Independent
review found no further dispatch blocker; focused production23.6s/5tests0.9s PASS.

Latest transaction `compat-20261026T023647Z-826f6d3084ed` restored/verified
02:41:48.4177215UTC. Both6faa runs protected Compare PASS:
Mods967/saves118/references18/settings83/tasks0/one unchanged Steam.
No owned live transaction/game/task remains. Original Unity logs retained.

Build attempts/validated development packages/native launches:37/27/41.
Latest build261.661s; wrappers266.752s/302.066s, native99.872s/135.474s.
For this02:16-02:48 continuation, approximate implementation/records18min,
observer/fixture research/review12min, build4.4min, native setup/execution/
restoration9.5min; categories overlap and are not additive. Earlier complete
summed categories unavailable. No speedup inference. Owner waiting not approval.
Disk minimum remains1.1GB peak plus2GB margin; verified closed-copy cleanup
completed02:49:03.2818987UTC for three exact duplicates:297,941,275B;
free3,545,407,488B. ProtectedComparePASS; exact plan receipt in history.

Next: one coherent corrected package, then `lillend-support` and the shorter
required `wizard-input` to establish common finalizer behavior. If successful,
`sorcerer-input` and affected `druid-input` follow as fresh guarded processes.
Expected20-30minutes plus build; each retains10-15minute target/30minute ceiling
and complete restoration. No unchanged repeated approach. Passed bcd lifecycle
retains its original byte identity and reviewed dependency limits.
Continue remaining control/support risks, technical presentation, cold ON/OFF
persistence and affected integration; pending subsystem rows stay pending.
Final qualification requires independent coherent review and one frozen complete-
gate candidate, with separate records where identity guards require them.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
