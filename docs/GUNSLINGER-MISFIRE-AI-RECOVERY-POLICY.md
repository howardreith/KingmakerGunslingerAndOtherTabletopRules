# AI recovery after a firearm misfire: decision record (POL-01 / S4 T3)

Status: **decision checkpoint on a review branch. An owner decision is required before any product change. Nothing
implemented, merged or released.** Branch `codex/pol01-sprint4-policy`, stacked on draft PR #179 at its accepted head
`9b8a755ec8`. No product, test or harness file is changed by this checkpoint.

## 1. The issue, as the epic states it

From `Gunslinger_Remediation_Epic_2026-11-15.docx`, Sprint 4 ("Resolve firearm rules and progression behavior: repair
demonstrated firearm regressions and settle the remaining adaptation questions without inventing balance rules"),
story S4 T3, "Settle AI recovery after a firearm misfire", inventory record POL-01 / GS-AUD-0013 ("Repaired AI
companion remains suppressed until a player attack order"):

> The order ledger clears suppression only for a deliberate player attack order. Repair, Quick Clear and rest do not
> grant a new attack order under the current contract.

Remedy, verbatim:

- Demonstrate the repaired-companion scenario with automatic AI and a manual-order control. Capture the difference
  between resuming a prior command and authorizing a new AI decision.
- Present the choice: retain explicit player rearm with clear behavior, or allow fresh normal AI decisions after
  successful recovery while keeping the original interrupted attack sequence cancelled. Recommend the latter only if
  the owner wants automatic companion recovery; never resurrect the interrupted burst.
- Implement the chosen transition in `FirearmAttackOrderLedger` and its recovery call sites, scoped to the exact actor
  and firearm and preserving stale-command epochs. Preserve deliberate stop, hold-position, invalid-target and
  combat-end behavior.

Acceptance: misfire stops the current sequence immediately; failed repair, repair of a different firearm and retained
pre-break commands remain suppressed appropriately; recovery itself cannot attack, consume ammunition, duplicate a
shot or bypass action economy; the companion resumes or waits exactly as the recorded policy states, in real-time and
relevant turn-based paths; a deliberate player stop remains effective and the manual-order workaround still functions.
Completion outcome: "Behavior decision then targeted repair". The epic's general rule for decision stories: the
remedy is a recommendation, not an approved rule; prepare the alternatives, observable player behaviour, save
implications and a recommended choice before changing the dependent behaviour.

The epic does not select an option. **An owner decision is required.**

The original finding (`planning/release-audit-0.0.182/FINDINGS.md`, GS-AUD-0013) classifies this as a contract
question, a possible product defect "if AI-controlled party members are in scope", and notes that the policy's own
comment describes the behaviour, so a fresh deliberate order "may be intended"; what looks unintended is that it
survives a successful repair.

## 2. What the current implementation does (source)

- **Break.** A committed misfire degradation of the exact firearm calls
  `BrokenSequenceSuppressionRuntime.OnCommittedDegradation` → `FirearmAttackOrderLedger.Break(actor, weapon)`: the
  item's epoch is incremented, `Suppressed` and `RequiresOrder` are set, and a current order on that weapon is
  cancelled. The ledger is a process-lifetime `ConditionalWeakTable` keyed by actor and then by the exact weapon; it is
  never serialized.
- **Automatic attacks are refused.** `EmptyFirearmAttackCommandPatch` prefixes the native
  `UnitAttack.CreateAttackCommand`. While the weapon is suppressed, a construction not claimed by the player-input path
  is rejected (`BrokenSequenceInterruptionPolicy.EvaluateConstruction` → `RejectInterrupted`) and the factory returns
  no command. Bound commands and captured reload continuations are also refused by epoch, suppression and
  `RequiresOrder` (`NativeFirearmAttackOrder.MayExecute`, `MayResume`).
- **Only a player order releases it.** `Suppressed` is cleared in one place, `Ledger.Accept`, reached only from orders
  proposed by the two patched player-input handlers (desktop click and controller interact). The accepted order owns
  its target; `RequiresOrder` stays set until that order completes normally (`Ledger.Complete`: no command carries it,
  its target is defeated, and the firearm is currently Normal), after which ordinary automatic combat resumes
  (0.0.156). Cancelling an order while `RequiresOrder` is set suppresses the weapon again.
- **Recovery does not touch the ledger.** Quick Clear (`Deeds/QuickClearRuntime`), Repair Firearm
  (`Recovery/RepairTestMusketRuntime`) and the completed-rest maintenance (`Gunsmithing/CompletedRestMaintenancePatch`)
  change the item's condition only. Reloading does not either.
- **Nothing tells the player.** A rejected automatic attack is written to the mod's own log
  (`firearms/attack.command-rejected`, "…broke during its attack sequence; issue a new attack order to fire it again."),
  not to the game's combat log or any notice. A save and load clears the ledger incidentally.
- **Published wording.** v0.0.127 release notes: a break "stops the remainder of that sequence and its automatic
  continuations… A later deliberate player attack with the Broken gun remains possible." v0.0.128: "Recovery alone
  grants no new order." `KNOWN-ISSUES.md` repeats the first.

## 3. Demonstrated, and only inferred

**Demonstrated natively**, in the product's guarded `disposable-firearm-break-interruption` fixture (native click
handler and controller interact, native commands and ability execution, a request-local scene, no save): latest
retained run `20261016T0411002291236Z`, PASS, 491 of 491 assertions, product 0.0.156, commit `162087000707`. The
ledger, misfire runtime and the fixture's sources (`Firing/`, `Misfires/`, `RuntimeTestRunner.FirearmMaintenance.cs`,
`RuntimeTestRunner.FirearmAdditionalInput.cs`) are byte-identical between that commit and this branch. The run used
0.0.156 bytes, not the current ones.

- A misfire break stops the sequence: one shot, Broken, empty, no further powder spent, suppressed
  (`*-real-break`, `*-real-break-old-order-stopped`), real time and turn-based, desktop and controller.
- The native AI retries and is refused: its next-command time advances while no shot follows
  (`*-rtwp-native-ai-retry-window`), and the native `UnitAttack.CreateAttackCommand` returns no command
  (`*-automatic-create-rejected`); a native AI reload captured before new input is refused later
  (`*-stale-ai-reload-rejected`).
- Recovery alone revives nothing: after Quick Clear to Normal (or a manual reload), for at least one second of real
  time covering the AI's 0.3 to 0.4 s retry interval, no shot, no current order, still suppressed, the recovery
  action's own cost charged (`rtwp-` and `tb-quick-clear-alone-never-revives-old-order`); after an out-of-combat Repair
  Firearm to Normal, still suppressed (`repair-native-repair-preserves-item-kit`).
- The manual-order control works: a new explicit click fires the same firearm (`*-quick-clear-new-attack-fires`,
  `repair-new-order-fires-exact-target`), and after that order's target dies with the firearm Normal, automatic
  combat resumes without another click (`cr04-repaired-*`, 0.0.156).

**Inferred from source, not observed:** that a recovered companion left without orders stays idle beyond the observed
second for the rest of the fight; that no other AI action (an ordinary weapon swap, another ability) fills the gap;
the completed-rest path's effect on the ledger (the rest fixture checks restoration, not suppression); and what a
player sees, which by source is nothing.

So the difference the epic asks to capture is established for the current contract: the interrupted command is never
resumed, recovery grants no new decision, and only a new player order authorizes the firearm again.

## 4. The alternatives

### A. Keep explicit player rearm, and make it visible

After a misfire break, the firearm fires again only on a new deliberate player attack order, as today. Repair, Quick
Clear and rest restore the gun but authorize nothing.

- **Player behaviour.** A companion whose gun broke stops firing on its own, even after it is repaired, until the
  player clicks an attack with it. Today this happens silently; option A would add one player-visible notice per
  break (for example in the combat log: "*Name*'s *firearm* broke during its attack sequence; give a new attack order
  to fire it again."), shown once for that actor, firearm and break, and would keep the release-notes wording.
- **Risk.** Players who rely on real-time party AI will still find a repaired companion idle; the notice explains it
  but does not remove it.
- **Implementation.** Presentation only: the notice at the existing rejection site, deduplicated per actor, weapon and
  epoch; documentation. No change to the ledger.

### B. Allow fresh AI decisions after a successful recovery

A successful recovery of the exact firearm to Normal by that firearm's recovery (Quick Clear, Repair Firearm, a
completed rest) releases its suppression for that actor, so the native AI may decide new attacks with it. The
interrupted sequence stays cancelled.

- **Player behaviour.** After Quick Clear or a repair, a companion resumes ordinary automatic combat with its gun. A
  player who wants it to hold fire uses the ordinary stop or hold commands.
- **What does not change.** The break still stops the sequence at once. The epoch is not reset, so every command,
  bound reload and captured continuation from before the break stays refused, and the interrupted burst can never
  resume. A failed repair, a repair of a different firearm and a reload of a Broken gun release nothing. Recovery
  itself performs no attack, spends no ammunition and charges only its own action. A player's explicit order and
  `Complete` work as today. Deliberate stop, hold position, invalid target and combat end stay native.
- **Turn-based.** Party members act on the player's command each turn, so little changes there; AI-controlled allied
  units would choose attacks again on their own turns.
- **Risk.** The rule "Recovery alone grants no new order" (v0.0.128) is reversed and must be stated in the next
  release notes. The rest path needs the firearm's suppressed actor, which the ledger currently cannot look up by
  weapon alone.
- **Implementation.** One ledger transition (for example `Recover(actor, weapon)`: clear `Suppressed` and
  `RequiresOrder`, keep the epoch, leave any current order alone) called from the three recovery sites only on a
  committed change to Normal for the exact item; a weapon-to-actor lookup for the rest path (or a release on the next
  equip); documentation.

Neither option changes save data: the ledger is never serialized under either.

## 5. Does the epic select one?

No. It asks for the choice to be presented and recommends B "only if the owner wants automatic companion recovery".
The published contract (v0.0.127/0.0.128) is A without the notice.

## 6. Recommendation and the decision needed

**Recommendation: B, if you want party AI to resume firing after a successful recovery; otherwise A with the visible
notice.** Leaving A silent is not recommended: the audit found the silent idle surprising enough to file it, and
nothing in the game explains it.

The reasoning for B: Quick Clear, Repair Firearm and a rest are themselves deliberate acts, so treating a successful
one as permission to fight again matches what a player expects, and the epoch already guarantees the broken burst
cannot come back. The reasoning against: it reverses a published rule, and it gives the player less control over a
companion who has just broken a gun (they must stop it explicitly).

**Decision needed from the owner:** A (explicit player rearm, made visible) or B (fresh AI decisions after a
successful recovery, interrupted sequence kept cancelled). If B: whether a completed rest counts as recovery for this
purpose (recommended yes, for consistency), or only Quick Clear and Repair Firearm.

**Blocked on the decision:** every product change in this story (the ledger transition or the notice, its call sites,
the release-note change) and its native qualification. Nothing else in Sprint 4 depends on it.

## 7. Bounded plan once decided

Either way, one cut on this branch:

- **Domain tests** through the ledger and the construction policy. For B: recovery releases only the exact actor and
  firearm; a failed repair, a different firearm and a reload of a Broken gun release nothing; the epoch is unchanged,
  so a pre-break command, a captured reload continuation and a stale bound order stay refused; a later break
  suppresses again; a player order and `Complete` behave as today. For A: the notice once per actor, firearm and
  break, never for an unsuppressed weapon.
- **Native**, one candidate: extend the existing manual-recovery cases of `disposable-firearm-break-interruption`
  (real time and turn-based, Quick Clear and Repair Firearm) with a longer idle window after recovery. For B: the
  native AI constructs and fires a new attack on its own, exactly once per decision, with the recovery's own action
  cost and no ammunition spent by the recovery; a deliberate stop still holds. For A: no shot, and the notice once.
  The manual-order control stays. Run the narrowest launch that holds those cases, twice in fresh processes.
- **Records**: release-note wording, `KNOWN-ISSUES.md`, the status register.

Target: the 15-minute planning budget, 30-minute ceiling.

## Validation of this checkpoint

Records only. Nothing was compiled, built or run for it; the evidence above is the retained run named in section 3
and source reading at `9b8a755ec8`. The native observations are on 0.0.156 bytes; the current bytes were not run for
this decision.
