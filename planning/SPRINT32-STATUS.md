# Sprint 32 — Satyr and Lillend Azata

Owner Howie Reith; mission2026-10-25. Sole active operational record.
Finish at an independently reviewed, natively validated PR. No merge, release,
Sprint33, unapproved subsystem or owner playtest dependency.
[Validation policy](../docs/VALIDATION-POLICY.md) ·
[Append-only evidence/history](SPRINT32-WORKFLOW-HISTORY-2026-10-25.md) ·
[Draft PR74](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/74).

## Scope, baseline and ownership

Accepted intake master `48e6fb4abfc154f111a4019a806cb79a4184bd1c`.
Branch `codex/expanded-summoning-sprint32-satyr-lillend`; worktree
`C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/.worktrees/expanded-summoning-sprint32`.
HOWARD-LAPTOP owns mechanics/shared gameplay integration. DATA's released-creature
art/worktrees remain untouched. Review relevant accepted dependency changes at
one coherent checkpoint before freezing final source.
Charter v1.0 sections3/4/7 and S32: Satyr SNA IV-IX; Lillend SM VI-IX;
quantities1/1d3/1d4+1; dependencies5,29-31. Preserve normal actions, independent
resources, ownership, duration, cleanup, stable GUIDs and save compatibility.

## Accepted decisions

- Execute native/technical visual acceptance unattended; owner aesthetic approval
  remains separate. Functional-first cosmetic debt cannot waive technical defects.
- Conditional Suggestion uses CowWithHat Custom Spells when active and is omitted
  when absent. Accepted adaptation: avoid combat/fascinated until harmed,
  minutes/level with Will/SR; no unrestricted instruction interface is claimed.
- Omit/disclose Countersong, Darkness, Light, Dancing Lights, Ghost Sound,
  Hallucinatory Terrain and Detect Magic. No replacement fog or generic subsystem.
- Approved Lullaby: normal Standard cast,10ft burst, Will/SR,−5Perception/−2Will
  versus sleep, immediate concentration release and CL7 seven-round42s tail.
- Approved Lillend-only noncombat dispatcher: preselected Fascinate≤3,
  Inspire Competence and dependent bardic Suggestion, normal actions, native
 20-round pool, exclusivity, exact-source refusal/lifecycle. Competence/Fascinate
  have early native proof; final coverage remains incomplete.
- **Approved 2026-10-26:** implement the Lillend-only memory exception for bardic
  Suggestion on this exact Lillend's fascinated target. Bounded exact-cast context
  transport into the hostile-memory handler must preserve normal casting,
  Will/SR, stealth, invisibility and all other offensive reactions. No general
  memory/AI overhaul or persistence manager is authorized. Estimated3–8active
  hours uncertain. Owner answered "Implement the Lillend-only memory exception";
  implementation is present; native qualification remains pending, with no further owner
  permission needed inside this exact boundary.
- **Approved 2026-10-26:** Lillend Hold Person keeps native automatic Will saves
  each round without an action debit. Owner answered "Accept native automatic
  repeat saves"; PF1 voluntary full-round retries are not implemented. No new
  incapacitated-action subsystem. Private source cleanup remains required.
- **Approved 2026-10-26:** retain native hearing behavior and disclose the deafness
  limitation, including verified optional CotW deafness facts. No hearing adapter
  or new hearing/deafness subsystem is selected. Owner answered "Accept native
  hearing limitation"; native deaf-performer failure and deaf-target exceptions
  are not claimed.
- Exact branch addition to sanctioned push wrapper authorized/applied. Development
  version0.0.167 remains candidate-bound; no release number reserved.
- Guarded Steam640820, leases, current protected intake, immutable baseline,
  named disposable Working/isolated fixtures and exact restoration remain binding.

## Product and current candidate

Satyr: adult Fey8/HP44/CL8, Fear, Charm Person, Sleep, conditional Suggestion,
control eligibility and bow/dagger/secondary-horns fallback. Lillend: adult
Outsider7/HP73/AC20, Large +1 sword/secondary tail, shared grab/constrict opt-in,
bard7 book5/4/2, own20-round Move performances, healing/control, safe Sound Burst,
Lullaby and corrected Daze60s immunity. Ten new public development roots preserve
1150 prior roots. Source profiles: [Satyr](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Satyr),
[Lillend](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lillend).
Native Persuasion/Perception combine printed social/Sense Motive skills; own
Intelligence drives Knowledge(nature). No substitute Survival, Disguise, Perform
or Fly stat. Accepted ordinary-map flight has no separate Hover/aerial system.
[Original body provenance](../assets-source/original-models/sprint32-support/README.md).

Latest validated development candidate: `c2a1e5913b386107ee12946f35bd1b6233ff1868`.
ZIP `ce7a8f1a004201b7758409e7afaa1810e96b04bfa695465b50ea4ce82af5fa6a`;
DLL `aaea5870970bf9df5514164381dc26af2461420ddf4a3fff9b7af68beeb754b0`;
MVID `4883554c-e9cf-47d3-968d-2dd6794fa694`;
SourceState `26b0fc01f55221aaa90553af487f099097fc3aca347337e8d6efcb470f80f259`.
Canonical artifacts/development-candidates/sprint32-lillend-support/c2a1e5913b38/.
Build64: wrappers/558selected/exact compile/strict ZIP PASS267.025s. Build63
was an elevated source-only admission refusal before compilation/package;
ordinary nonadmin Build64 succeeded. This is not final qualification.

Native74 Hold **9/9PASS**: natural CL7/DC16/42s casts and real L2 slots,
accepted automatic failed/successful repeat saves, native source expiry removing
paralysis before its deadline, and exact sibling-effect/budget preservation.
No successful automatic retry explains the source-expiry result. Hold TB remains
pending. Native72 bardic Suggestion remains source-bound **18/18PASS**, including
ordinary-book negative (real L2slot4->3/hostile memory/Fascinate interruption).

Native75 Working prepare **17PASS/2FAIL**: corrected native Pause boundary and
actual dispatcher Competence activation pass. The finite single-ring listener
survey refuses before listener spawn/save. No save write; cold/OFF NOT_RUN.
Native75 cannot identify which geometry predicate failed. Installed review found
that player acquisition range/anchor LOS are not the performance sight contract.
The reviewed correction surveys four finite distances with native EyeShift,
retains all-party/source exclusion, walkability/separation and actual postspawn
mutual eligibility/calm checks, and records one compact rejection-count receipt.
Original73/75 failures and exact restorations remain in history.

Current reviewed, production-compiled batch (not yet packaged): private Sound Burst changes observed native1d12 to PF1
1d8 (explicit nativeD8/D12 input guard); exact private Charm gains stateless
caster/apparent-ally threat handling. One appended identity preserves all7510
prior objects. Grouped native controls cover natural nonhumanoid Charm Monster,
normal RTWP helpful-heal preservation/caster-attack break and safe Sound Burst/
Fort/damage/stun/costs in RTWP/TB. Nonally/ally threat negatives, wards and further
lifecycle remain pending; the TB cast case does not claim a TB threat probe.

Owner accepted the native hearing limitation after additional research found
optional CotW facts and a possible bounded adapter. No hearing adapter code was
written. The limitation remains explicit; the earlier proposed new hearing state
is not required or authorized by this disposition.
Private saved-state selections/budgets and fresh ON/OFF cleanup remain unverified.
Fixture equipment disposal is not native equipment-expiry proof.

## Development evidence and reuse limits

All executions retain original source/observer/package identities in history.
A source or compiled observer correction invalidates its affected native case.
No earlier execution below is called fresh on the private-control correction.

| Evidence | Actual result and remaining limit |
|---|---|
| f5756a4a/native71–73 | Hold8PASS/1FAIL observer placement; bardic18/18PASS including ordinary-book negative; Working prepare15PASS/2FAIL before save. Details and exact restoration above/history. |
| 5a1199de/native69 | 15PASS/3FAIL: all positive bardic saved/resisted/successful/current-effect-refusal cases PASS, same Fascinate/actions/budgets and native offensive reactions. Ordinary negative enters Pause; strict lease/finalizer refuse, outer transaction restores exactly. Negative remains unverified. |
| fa5614e0/native68 | 15PASS/1FAIL: Hold discovery guard, natural saved/high-SR bardic attempts, exact Fascinate/actions/slots/upkeep and global/aggro callbacks PASS. Retention fails after the subsequent invisibility setup; successful effect and remaining checks unverified. |
| d43a72d8/native67 | 12PASS/1FAIL: provider initialization, exact metadata and natural Fascinate PASS; patch-inventory observer null conversion throws before bardic readiness/casting. Fix reuses proven global-patch enumeration; rerun required. |
| fd54cde1/native66 | Bootstrap FAIL before gameplay: CotW Infectious Charms conditional unsupported in private Hold mapping. Exact bounded mapping correction unbuilt; no assertions executed. |
| 096b045c/native65 | 5PASS/2FAIL: provider binding localization collision before natural bardic cast. Original result retained; shared-description correction unbuilt. |
| 279de1e3/native63,64 | RTWP13/13 and turn13/13PASS: natural Sleep/Charm, costs/durations/refusal, expiry and exact sibling-effect isolation. Charm group cache restores one ordinary frame after direct faction restoration. |
| 61ba8681/native62 | 12PASS/1FAIL: both original private effects removed and exact sibling effects preserved. Sleep restored; Charm hostility sampled before verified cache propagation. Observer-only bounded-wait correction pending native proof. |
| 158147c0/native61 | Natural casts/durations PASS with private effects; both source-expiry rows FAIL. Native component exists on real buffs but misses destruction ordering. Stateless exact-event correction and sibling-effect probe are unbuilt. |
| ef3307a2/native59 | Natural Satyr Sleep/Charm, costs/eligibility PASS; both source-expiry rows FAIL. Sleep480s, combat Charm60s. Exact effects remained active after caster disappearance while targets/sibling survived. Private duration/lifecycle correction invalidates these affected cases. |
| 49786548/native57 | Fascinate24/24PASS: natural Move,20-to19-to18, WillDC17/noSR, cap3/frozen targets,24h source marker deadline, ShakeFree, restart refusal, actual0.9409m approach/one save each, visible CureLight threat/slot5-to4, sibling isolation, exact marker/helper/area/command cleanup. No24h expiry/cold/bardic proof. |
| a333ab3f/native55,56 | Fascinate13/13 and Competence12/12PASS; natural brain refusal/activation, +3recipient, audible maintenance and terminal recipient-loss cleanup. Native54 original11PASS/2FAIL remains; corrected reader waited native carrier retirement. Terminal7.699m is later movement, not continuous out-of-range residence. |
| 7aed020a/native53 | Cantrips7/7PASS: natural Lullaby modifiers/refusals/original42s deadline; Daze twice with actual60s marker expiry, cross-source refusal and native AI cooldown1-to0. RTWP only. Earlier48/51 expiry flag was insufficient; original failures retained. |
| bcd41d04;51cf0e58 | Lifecycle15/15 and support13/13PASS: native expiry/death, external Courage/sibling cleanup; natural self/party-ally healing18-to0, ownL3slots2-to1-to0, untouched sibling, Standard and exact free first touch. |
| 4b509d67 native input | Wizard16/16, Sorcerer28/28, Druid31/31PASS: actual visible-menu/target prepared/spontaneous/conversion paths, legal1/d3/d4+1, real slots/actions,120s, body/icon and exact reset. No supplied casts or manual command ticks. |
| 2d90 RTWP12/12;turn10/10 | Natural Courage, real ally+2/enemy exclusion, healing, useful fallback, completed sword/sword/tail mechanics, live Nature and sibling isolation. No full animation/contact claim. |
| Earlier Satyr/optional provider | Fear, immunity refusal and bow/dagger/horns fallback in both modes. Natural provider Suggestion DC17/CL8 Satyr and DC16/CL7 Lillend, normal actions/duration/own book slot/current-effect refusal. Positive Will/highSR, break/expiry/provider cold remain NOT_RUN. |
| Bodies | Independent Lillend bindings,62owned objects released/158borrowed retained, recognizable rest views. Full gameplay-camera movement/contact/hit/death/despawn/performance still pending. |

## Finite acceptance contract

Every final row remains incomplete. Group related observations; add a case only
for a concrete changed risk. Final qualification uses frozen source/assets/
observers/publication inputs, one complete cheap gate and its exact ZIP, plus
separate records worktree where identity guards require. Do not rebuild for prose.

| ID | Expected observable result | Existing route/evidence | Mode/profile | Invalidated by |
|---|---|---|---|---|
| A1 | Correct adult identity/defenses/attacks/facts/spells; legal tiers/quantities, prior routes preserved | Domain catalog/routes; native composed profiles/inventory | Offline; standalone | Catalog/builders/registration/adapters |
| A2 | Player summon pays actual action/slot, legal count/duration, usable body/icon | Native input/route fixture; slot/spawn/expiry receipts | Standalone; materially distinct prepared/spontaneous/conversion | Publication/cost/quantity/duration/adapters |
| A3 | Useful Satyr control without pipes, immune/ineligible/current-effect refusal and ordinary fallback | Exact-owner AI/commands/saves/effects | RTWP/TB | Satyr rules/targeting/shared AI/actions |
| A4 | Useful safe Lillend buffs/performance/healing/control and ordinary fallback; normal independent actions/slots/pools | Same bounded support/performance/cantrip fixtures | RTWP/TB | Book/performance/dispatcher/AI/action handling |
| A5 | Expiry/death/dismissal/cancellation remove owned effects/items/commands, preserve siblings | Existing lifecycle/native scheduler; exact captured instances | Standalone; affected modes | Lifecycle/equipment/resources/effects/commands |
| A6 | Fresh-process state/budgets/ownership survive, moduleOFF safely deserializes, original save restored | Existing Working prepare/cold verify-cleanup/fresh absence ON/OFF | Required installed stack/provider seam | Serialized state/registration/lifecycle |
| A7 | Technical gameplay-camera loading/selection/scale/movement/contact/hit/death/despawn/performance | Existing presentation captures + structured native facts, agent inspection | Standalone/direct combined seam | Assets/rendering/attachment/loading/lifecycle |
| A8 | Standalone and directly affected high-risk combined/shared consumers work | Existing profile transaction/focused baseline and integration cases | Standalone/high-risk combined; additional only for concrete risk | Shared AI/registration/serialization/integrations |

Concrete remaining cases: Lillend Charm Monster
(nonhumanoid/protection/duration/break/lifecycle), safe Sound Burst, real tail
hit/grab/immediate and maintained2d6+liveStr constrict/release with one affected
Salamander baseline; performance combat-entry/TB boundaries; Suggestion positive
Will/highSR/break/expiry/provider persistence; new serialized performance/book/
cantrip state ON/OFF; technical presentation and final integration. Bardic
Suggestion exception has native72 proof; expiry/break/provider-cold rows remain pending.
Private Hold Person requires real initial Will/SR, automatic repeat-save/action
observation and source-loss/sibling lifecycle checks under the accepted adaptation.
No whole-roster replay selected.

## Restoration, headroom, work and next action

Latest installed-stack transaction `sprint32-stack-20261026T181611Z-116b49bce5ca`
and outer Working envelope restored exactly by18:22:51.0126924UTC. Original
Working SHA `650bc9768cf46d419e0d9e6a1793c46493d82dadf1d3498773a8496c1f975850`,
8,550,225B and original timestamps/attributes restored. Protected Compare PASS:
967Mods/118saves/18references/83settings/tasks0/unchangedSteam. No owned live
transaction/game/task. Local original evidence: artifacts/sprint32-working-c2a1e591/.

Counts:66Build-Local attempts /52validated development packages /75native launches.
Build65/66 refused before compilation/package: the Python manifest plan omitted
the appended Sound Burst row, then the icon catalog registry pin was stale. Both
metadata corrections now pass the complete source-only repository wrapper,
including30 icon tests and preserved admission/safety refusals. All7510 earlier
identity records are unchanged. No product behavior changed in these corrections.
Build64 measured4.45min; native74 wrapper6.43min/native179.347s; native75
wrapper6.82min/native206.840s, then outer restoration. Exclusive whole-mission
category totals remain unavailable. Recent18:04?18:40 work: approximately25min
implementation/research/fixture repair,3min verified cleanup,7min native wrapper
(overlapping research); no owner-wait blocking independent work. Two repeated
Hold geometry failures70/71 were resolved by74. Working75 is a different finite
placement failure after73's corrected pause boundary, not a product verdict.

Closed native74 extraction and superseded build62 duplicates reclaimed142,662,260B
against retained exact ZIPs. Protected Compare PASS; all unique packages, raw
failures, receipts, protected backups, saves and DATA retained. Free space after75:
3,457,798,144B at18:50:55UTC after verified closed75 extraction and seven older
byte-identical extraction copies were removed. Two untouched references and
lossless content archives are retained; these are not rebuilt historical candidate
packages. Protected ComparePASS. Full selections/hashes are in history.
Working measured trio902,923,316B before allowance; rounded1.1GB plus2GB reserve
requires3.1GB initial. Each stage requires550MB plus2GB reserve (2.55GB).
No build during an admitted trio; measure again before selecting work.

**Next:** commit the corrected catalog pin, then build one development candidate.
Execute the grouped control proof in gunslinger-only (avoiding optional Suggestion
competition), then corrected Working selection;
HoldTB/FearRTWP/TB remain selected affected checks. Each save-free selection
expects8?10min/runtime600s. Working ON/OFF retain the predeclared60?80min combined
ceiling including restoration, no cold stage after failed prepare. Provider
transition, controls/grab/presentation and final integration remain incomplete.
Final affected production compilation24.7s and18/2755 selected tests PASS1.0s;
independent resolution review found no remaining dispatch blocker. Native checks
remain required. No owner playtest, release or Sprint33 is authorized.

Prior v0.0.167 gaps stay **NOT_RUN**: all30public-route/body/icon, standalone
final integration, highest-risk combined final integration. [Original dispositions](../docs/SPRINT30-31-COVERAGE.md)
are not a Sprint32 waiver and have not been closed by these focused cases.
