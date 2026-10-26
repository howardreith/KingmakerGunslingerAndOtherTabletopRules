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

Latest validated development candidate: `279de1e3f73ec3c7324bff8aeb03909f67eda482`.
ZIP `23485d21bf0e741aa8f37779690a66902d1efdabba385406d320532f7ab751b0`;
DLL `4e4b59099043c58aa34e7fe856a9403e5028847763289a6349b73bd55b248b43`;
MVID `3ec2260e-4fea-432b-b76b-63b055fe39d4`;
SourceState `510597c38c862c6810fffa466c9eb8184df0f33401ec7e50c229321251aacd23`.
Path `artifacts/development-candidates/sprint32-lillend-support/279de1e3f73e/`.
Build55: wrappers,557selected/2754registered, exact-reference compilation and
strict package PASS in254.188s. Native63RTWP and64turn **13/13PASS each** on
that same ZIP. No complete final qualification on these inputs.

Both modes show natural Satyr Sleep/Charm, actual normal Standard costs,
CL8/DC15,480s/28800s applications, four-HD Sleep budget/ally exclusion,
controlled-target refusal and independent resources. Both original effects
clear on native source expiry before their own deadlines; exact separate
sibling effects/deadlines survive. Charm's direct attack faction restores
immediately and its group cache/IsEnemy restores after one ordinary frame.
This confirms the62 observer-order defect;62/61/60/59 remain historical FAIL.
Turn mode used native turns and zero fixture AI ticks. The sibling application
is explicitly effect-only lifecycle isolation, not a second casting proof.

Current unbuilt change implements the approved Lillend-only bardic memory
exception using invocation-local actual cast context. Exact native controller,
provider/ability/own live Fascinate guards; original global then actor-local
events and the leading aggro-script callback are retained. Nonmatching calls
delegate unchanged. No ambient flag, persistence or attempted-target immunity
ledger. Independent product review's subclass boundary finding is corrected;
native IL hashes validate original engine code, not foreign Harmony patches.
The fixed grouped provider phase inventories those patches before qualification.
Production23.7s/18focused tests4.5s PASS; fixture review found lawful player-group visibility and HashSet attribution assumptions; both are corrected before dispatch. Native proof pending.
The first observer compile's signature/namespace errors remain in its original log.

The private stateless control destruction adapter has actual Satyr removal and
sibling-isolation proof; retained native area-load handling still needs cold proof.
Affected Lillend book/SLA/cantrip, death and fresh-load cases remain required.
No owner decision is pending.

## Development evidence and reuse limits

All executions retain original source/observer/package identities in history.
A source or compiled observer correction invalidates its affected native case.
No earlier execution below is called fresh on the private-control correction.

| Evidence | Actual result and remaining limit |
|---|---|
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
Suggestion rows remain pending implementation/validation of the approved exception.
No whole-roster replay selected.

## Restoration, headroom, work and next action

Latest transaction `compat-20261026T123446Z-0a7a0ecd2023` restored/verified
12:39:25.0147993UTC. Protected Compare PASS967Mods/118saves/18references/
83settings/tasks0/one unchanged Steam. No owned live transaction/game/task.
Native63/64 raw directories and exact identities are in append-only history;
local originals/intakes: `artifacts/sprint32-satyr-atwill-279de1e3/` and
`artifacts/sprint32-satyr-atwill-turn-279de1e3/`.
Original failures remain immutable; no product-stat changes supplied a verdict.

Counts:55Build-Local attempts /44validated development packages /64native launches.
Build55 measured4.24min; native63 wrapper4.78min/native117.394s; native64
wrapper4.67min/native118.875s. Whole-mission work-category totals unavailable.
Approximately12:26-12:52UTC: inspected contracts/drafted adapter and grouped
observer/review~20min, overlapping the measured native execution/restoration;
cleanup verification~2min. Focused compilation26.4s then23.7s plus the retained
failed observer compile. No exclusive owner wait or speedup claim.

Disk budget:1.1GBcombined peak +2GBreserve before building. Prior cleanup
receipts remain in history. At12:42UTC four completely byte-verified closed
package extractions reclaimed398,214,056B; free3,453,100,032B, protectedComparePASS.
Unique packages/raw evidence/original failures/receipts/PDBs/saves/backups retained.
No machine-wide cleanup or DATA changes.

**Next:** finish independent review of the grouped `lillend-bardic-suggestion`
fixture, batch concrete corrections, package once, then execute its existing
guarded provider profile (expected10min/runtime600s). Observe natural saved,
resisted and successful attempts, preserved own Fascinate/slots/upkeep,
ordinary invisibility/aggro reactions, active-effect refusal and a labeled
ordinary-book negative. No input/save admission or supplied bardic cast.
Then continue affected Lillend controls/lifecycle, grab/constrict, cold
ON/OFF, technical presentation and final integration. Reuse the executed
S30-31 installed-stack original-Working envelopes for persistence.
Hold Person has a concrete outgoing cleanup risk; Fear/SoundBurst terminal
components still need bounded inspection. Invisibility is self-only.
Final frozen candidate qualification remains separate required work.

Prior v0.0.167 gaps stay **NOT_RUN**: all30public-route/body/icon, standalone
final integration, highest-risk combined final integration. [Original dispositions](../docs/SPRINT30-31-COVERAGE.md)
are not a Sprint32 waiver and have not been closed by these focused cases.
