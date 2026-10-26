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
  leaving the CL7 seven-round tail. No sustained concentration subsystem. Private spell/AI implemented; native proof pending.
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
complete printed spell/feat/skill coverage from these adult stats. Lullaby is implemented with focused compilation PASS and native proof pending.
Noncombat performances remain required implementation, not pending permission.

Current validated development candidate is `8a02948cc1141f816db80a543c47cd30aef78880`:
ZIP `50382bd6302a46a1fd96c0154ddb3512d9b4923af8b6253e5facd2ce445bc7a2`;
DLL `4866d5eb1732c1c80c6cbd61b901a9f58ae655f8592efd7f3af115fa6a259bb4`;
MVID `6427efa1-a246-46cb-bb88-e35bae10fda3`;
SourceState `03ac7acdd44577507a75fea60e2ef6f55a4a669945c54707c53c1ff0e387b2f3`.
Path `artifacts/development-candidates/sprint32-lillend-support/8a02948cc114/`.
Wrappers,557 selected/2753 registered checks, exact-reference compile and strict
package PASS. Development version0.0.167; no release qualification.
Native47 support/body assertions passed but Lullaby stopped at eligibility0:
overall13PASS/2FAIL, no Lullaby cast. Native book-backed level-zero availability
checks finite slots; correction uses the accepted native Spell/Cantrip fact seam
for both Daze and Lullaby. Independent review confirms exact-owner/live bardCL/
Charisma adapter and AI fact preference. Corrected source/fixture compilation
passes; fresh cast proof is pending. Input evidence stays bound to4b509d67.

| Evidence | Result and dependency limit |
|---|---|
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

Latest transaction `compat-20261026T042024Z-273c3e7938f1` restored/verified
04:25:06.5746215UTC. Latest protected Compare PASS:
Mods967/saves118/references18/settings83/tasks0/one unchanged Steam.
No owned live transaction/game/task remains. Original Unity logs retained.

Build attempts/validated development packages/native launches:41/30/47.
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

Next: package the reviewed cantrip correction and run Lullaby/repeated Daze
within the existing standalone Lillend-support phase (expected10minutes,
runtime600s ceiling), then implement the
approved Lillend-only dispatcher and prove its uncertain native seams early.
Batch coherent mechanics and existing fixture adaptations. Required remaining
cases include Satyr Sleep/control, Lillend control/constrict, gameplay presentation,
cold ON/OFF persistence and directly affected integration. Reuse input evidence
only after checking actual changed dependencies. Final qualification still needs
remaining coherent evidence review and one frozen complete-gate candidate, with
separate records where identity guards require them. Do not start Sprint33.

Prior v0.0.167 gaps stay **NOT_RUN**: grouped all30 public-route/body/icon,
standalone final integration and highest-risk combined final integration.
[Original dispositions](../docs/SPRINT30-31-COVERAGE.md) are not a Sprint32 waiver.

Lullaby prepackage: private Standard cantrip, living10ft burst, native Will/SR,
CL-round finite tail, Perception-5 and Will-only Sleep-2; useful-Sleep/ally-safety/
active-effect AI guards. Independent review found inherited duration tooltip;
corrected. Production compile24s and5selected tests/4.5s PASS. One prior observer
compile failed on invented UnitUseAbility.Ability; corrected to installed Spell.
Original error retained; no native failure. Icon catalog/137prior unchanged
records PASS; new Lullaby original and32/48/64/128px previews inspected. Native
mechanics/visual results remain NOT_RUN for these changed bytes.

Independent fixture review found no dispatch blocker. The owned target now has
disclosed native SR5 and each natural cast must produce an exact nonimmune,
nonresisted SR5 event; zero events cannot claim SR proof. This final fixture
addition awaits the candidate build. RTWP proof does not qualify turn-based
execution or secondary burst recipients.

Build40 at3e4d608e stopped before packaging/native dispatch:553/557selected
tests passed; four icon tests shared a stale action-concept count (137instead
of138). One shared declaration now includes the actual Lullaby concept, and
the existing catalog case checks its identity. Focused production24.2s plus
all9affected/S32tests5.1s PASS. Failure logs retained; no product rule changed
to satisfy the observer. Source/art/native fixture review remains applicable.
