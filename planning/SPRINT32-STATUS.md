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
- **Pending utility decision:** Light, Dancing Lights, Ghost Sound, Hallucinatory
  Terrain and Detect Magic have no qualified mapping. A summon-owned illumination,
  illusion/disbelief and aura-sensing subsystem was proposed (24-60 active hours,
  high uncertainty); recommendation is explicit omission/disclosure. No implementation
  or omission approval is inferred while the owner decision is pending.
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

Current validated development candidate is `51cf0e58443ce568b39e25b624c977e2f5fa9775`:
ZIP `6f692e3349cf00d9a666b77bff69ccf12633db75811949b96d1a00d25e6606de`;
DLL `850a381da85923180f16b4acbd1e45630ede655079f2fafd2df50f26e2d8a03a`;
MVID `f8923dc5-a29f-4401-a2e5-6a67a5a2963f`;
SourceState `477f52a8423e7fec23998a70b8edeac2c8d25c6660ff37484ba60aeef34dfcdc`.
Path `artifacts/development-candidates/sprint32-lillend-support/51cf0e58443c/`.
Wrappers,557 selected/2753 registered checks, exact-reference compile and strict
419-file package PASS. Development version0.0.167; no release qualification.
Current input observer and Satyr SLA corrections are newer source, not execution
on that package. Prior results retain their exact dependency limits.

| Evidence | Result and dependency limit |
|---|---|
| Reviewed reuse: bcd support lifecycle | PASS15/15. Both scheduler expiries, death cleanup after native RecentlyDeadBuff expires, external Courage removal, sibling isolation and exact reset pass. Death waits12.065/12.104wall seconds,12game seconds. `artifacts/sprint32-support-lifecycle-bcd41d04/` |
| Fresh51cf Lillend support | PASS13/13. Natural Courage, selfheal and actual party-ally heal18-to0; independent level3 slots2-to1-to0, Standard cost plus exact free first-round touch delivery; sibling untouched; offensive fallback and owned body cleanup. `artifacts/sprint32-lillend-support-51cf0e58/` |
| Reviewed6faa Druid input | PASS28/FAIL3 overallFAIL. All4 real casts pass: preparedOne and convertedOne/d3/d4+1; exact combat cost/slot/legal quantity/120s and unit resets. Final UI restoration aggregate fails, while outer installation/save/profile restore passes. Exact failing UI conjunct not recorded; Wizard/Sorcerer held. `artifacts/sprint32-druid-input-6faa9ba3/` |
| Fresh51cf Wizard input | PASS14/FAIL2 overallFAIL. Visible native command/slot/cost/quantity/base duration and UI finalizers succeed, but aggregate cast fails. Observer incorrectly treats all possible spawn leaves as executed count; exact unreported failed conjunct cannot be recovered. Corrected against registered identity; required rerun pending. `artifacts/sprint32-wizard-input-51cf0e58/` |
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

The live StickyTouch source capture passed self/party-ally healing on51cf. The
quiet exact-owned anchor and finalizer predicates passed Wizard43. Independent
review then found a concrete observer error: Spawns enumerates all conditional
leaves, not executed actions. The corrected assertion binds all modeled leaves
and actual units to the independently registered creature, forbids hidden spawns,
and preserves exactly one actual cast/spawn plus quantity, duration, slot and
cost requirements. Missing old conjunct values remain unknown; original FAIL
is immutable. Reviewer accepted this correction.

Private Satyr Charm Person/Sleep now explicitly enforce Standard/non-full-round
SLA action rules; Lillend's actual bard spells are unchanged. Existing evidence
never recorded native Sleep's flag, so this is a source-contract correction,
not a proven historical execution failure. Existing Satyr cases now inspect
runtime action mappings; natural Sleep cost still needs required control coverage.
[SLA rule](https://www.aonprd.com/UMR.aspx?ItemName=Spell-Like+Abilities).
The unapproved claim that Dancing Lights/Ghost Sound were charter-filtered was
removed. Their explicit disposition remains pending.

Latest transaction `compat-20261026T030229Z-7b9473123752` restored/verified
03:07:01.1836699UTC. Both51cf runs protected Compare PASS:
Mods967/saves118/references18/settings83/tasks0/one unchanged Steam.
No owned live transaction/game/task remains. Original Unity logs retained.

Build attempts/validated development packages/native launches:38/28/43.
Latest build259.647s; wrappers289.365s/273.711s, native122.433s/104.740s.
For02:50-03:21, approximate build/package4.3min, native setup/execution/restoration
9.4min, observer/source review and repair8min, storage/records8min; overlapping
categories are not additive. Earlier complete totals unavailable. Owner waiting
continues, is not approval, and did not block independent work.
Disk minimum remains1.1GB peak plus2GB margin. Verified closed-build duplicate
cleanup03:18:29.2309493UTC reclaimed1,138,981,599B; free4,106,891,264B.
All54 selected staging folders/DLL copies were byte-identical to retained validated
ZIPs. Unique PDBs/packages/receipts/logs/raw evidence/backups/saves retained.
Protected Compare PASS; exact plan receipt and initial preview refusal in history.

Next: build the coherent input/SLA correction once; run required Wizard, then
Sorcerer and affected Druid input phases in separate guarded processes. Planned
20-30minutes plus build; each retains10-15minute target/30minute ceiling and exact
restoration. No diagnostic-only or unchanged repeat. Continue required Satyr
control/Sleep, Lillend control/constrict, gameplay presentation, cold ON/OFF
persistence and affected integration. Pending subsystem rows stay pending.
Final qualification requires remaining coherent evidence review and one frozen
complete-gate candidate, with separate records where identity guards require them.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.
