# In Harm's Way immediate-action economy

## Scope and preserved human evidence

Investigation started on `codex/bodyguard-in-harms-way` at the clean,
remote-equal commit `d39c0cf6fe1f6c7312e45e8250f8245a2a436409`. The
pre-change Release suite passed 1,212 tests. The installed candidate was the
immutable 0.0.94 package with SHA-256
`A0B01C41030FC074CC144A498505796E9D4151113A882DB4CCDC1565C4D75E45`;
its DLL SHA-256 was
`A226B7D2E2337345396574F7B6178C53C90AAB22B25DF47C2ADBAC09C1F54E69`
and MVID was `93ea290b-3ff4-49d6-a3fd-fd6136b2bab7`.

Before another launch, the exact 0.0.94 human log was copied read-only:

- source:
  `C:/Users/howar/AppData/LocalLow/Owlcat Games/Pathfinder Kingmaker/output_log.txt`
- evidence copy:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260823T195314857Z-human-0.0.94-off-turn-immediate/output_log-human-0.0.94-off-turn-immediate.txt`
- size: 626,513 bytes
- SHA-256:
  `64528B2BB530979BFBA99EEB86E82BFFB38EB5F1AFF45916FEE0F2A92586E331`

The decisive frame is `bodyguard-attack-6`. Its complete candidate record says,
in curated form:

```text
protector=HelpfulDefenderTest; bodyguardSuccess=True;
bodyguardContribution=4; inHarmsWayFeatPresent=True;
inHarmsWayActivatablePresent=True; inHarmsWayActivatableIsOn=True;
inHarmsWayMarkerPresent=True; alive=True; conscious=True; canAct=True;
hasSwiftAction=False; swiftCooldown=1.5; standardCooldown=1.5;
moveCooldown=0; isInCombat=True; turnBased=True; round=2;
currentTurn=Kobold; protectorIsCurrentTurn=False;
deliveryContractAvailable=True; decision=swift-cooldown-active
```

Bodyguard spent one native attack of opportunity, succeeded for the canonical
halfling Helpful +4 grant, and the attack remained a hit. In Harm's Way never
entered arbitration or changed a delivery target. This is direct proof that
the rejection was the old action gate, not target redirection.

## Exact supported engine contract

The supported `Assembly-CSharp.dll` is Kingmaker 2.1.7b, SHA-256
`3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB`,
MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.

### Swift actions

`UnitEntityData.HasSwiftAction()` has 27 bytes of IL and is exactly the native
predicate `CombatState.Cooldown.SwiftAction <= 0`. It does not represent an
immediate-action resource, future-turn debt, or a reaction permission. The
engine contains no gameplay type, resource, enum, or command contract that
implements Pathfinder immediate actions.

In RTWP, `Cooldown.SwiftAction` is the real shared six-second swift budget.
Adding six seconds prevents another swift action until the cooldown naturally
ticks down. This is the correct Kingmaker real-time adaptation and remains the
authority in RTWP.

In turn-based mode, positive cooldowns on an off-turn unit are normal
bookkeeping from its completed turn. They do not mean that the unit has spent
the swift action of its next turn. That is exactly what the human log exposed:
the protector was off-turn with 1.5 seconds still present while the Kobold was
the current unit.

### Actual turn lifecycle

`TurnController.Start(bool)` ultimately calls `TurnController.Prepare()` for
the unit's actual turn. At IL offset `0x32`, `Prepare()` calls
`UnitCombatState.Cooldowns.Clear()`, zeroing StandardAction, MoveAction, and
SwiftAction before new-round fact callbacks and action-state/UI calculation.
Later in the same method the native action UI calls `HasSwiftAction()`.

`TurnController.Dispose()` is the observable completion boundary for the
actual `TurnController`. `CombatController.set_CurrentTurn` disposes the old
controller before replacing it. `TurnController.TurnStatus.Delayed`
distinguishes a delay from a completed turn, so debt can follow the later
delayed turn rather than clearing at the original initiative position.
`ForceToEnd(true)` writes StandardAction 6, MoveAction 3, and SwiftAction 6;
those values are completed-turn bookkeeping and are cleared at the next
`Prepare()`.

Consequently, merely adding six seconds when an off-turn reaction occurs is
not sufficient: the next actual turn would erase that write before command
validation. A global-round flag is also incorrect because the charged unit may
delay or cross a global round boundary before taking its actual turn.

### Flat-footed state

The exact target-aware native seam is
`RuleCheckTargetFlatFooted(UnitEntityData attacker, UnitEntityData target)`.
Its `OnTrigger(RulebookEventContext)` evaluates native
`UnitCombatState.CanActInCombat`, helplessness, conditions, visibility, and
other engine rules. In Harm's Way uses that rule independently from the
attack-of-opportunity path. Combat Reflexes may let Bodyguard spend its AoO
before initiative, but it does not waive the immediate-action prohibition
while genuinely flat-footed.

## Optional-mod investigation

The exact installed compatibility DLLs remain:

- Call of the Wild 1.14.4c-2.1: SHA-256
  `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`,
  MVID `8caab254-aacf-4811-8093-44b9184e6e53`;
- Favored Class 1.3.1: SHA-256
  `DCD3ADF98D1A04C30D772381E7C56CE4BEFF35A98BCEA165AFF206A2F0AAC26C`,
  MVID `3efd38e7-8682-4b4d-8d53-e368a3664919`;
- Tweak or Treat 1.1.0: SHA-256
  `A518324E15632ABA46D6C467B156A31E9AFD282E9827DEE3E79AD14673852B92`,
  MVID `56f6c205-0ccb-47a7-b1d6-f000ff290b68`;
- Races Unleashed 1.0.11: SHA-256
  `6D18168CB90FFE60931ADDC8EE11E42B3EF647EF0E6D4B7CE8980D44659F4CB0`,
  MVID `e9b9acb5-9b3f-41ad-bbd7-74494d5d7680`.

Installed-binary and authorized public-source searches found only native
`SwiftAction` cooldown checks and writes. None supplies a complete off-turn,
next-actual-turn immediate-action model. KMG therefore cannot safely delegate
this rule to an optional mod, and no compile-time or runtime dependency is
introduced.

## Selected adaptation

KMG uses native state wherever it is semantically complete:

- on the protector's own turn, the current native swift cooldown is the shared
  budget;
- in RTWP, the native six-second swift cooldown is the shared budget and timer;
- while a turn-based debt is charged, native `SwiftAction` is set to six and a
  narrow `HasSwiftAction()` postfix may only change `true` to `false` for that
  exact debt owner.

Kingmaker's missing off-turn correlation is represented by two hidden,
mechanically inert, save-stable project facts:

- `KMG.Feats.InHarmsWayImmediatePending`
  (`a92164067bad3a85b1da48db5a787686`): an off-turn immediate action has
  charged the owner's next actual turn;
- `KMG.Feats.InHarmsWayImmediateChargedTurn`
  (`326e183f7791e83a38337c6a6d7a8644`): that actual turn is in progress and
  its swift action is unavailable.

An off-turn spend adds Pending without trusting the off-turn raw swift
cooldown. At the exact `Prepare -> Cooldowns.Clear` seam, Pending becomes
Charged and native SwiftAction becomes six before native action-state
calculation. A completed `TurnController.Dispose` removes Charged. A delayed
turn converts Charged back to Pending, so the debt follows the delayed actual
turn. Combat or scene completion clears transient debt. `UnitEntityData.PostLoad`
reasserts native swift denial if a charged turn was saved and restored.

The state is per-unit and tied to actual turns, not global rounds. A second
off-turn immediate action is rejected while Pending or Charged. Standard and
move actions are not changed. Transactional spend rollback removes only the
exact pending fact created by that failed interception, or restores the exact
native cooldown value for on-turn/RTWP failure.

The `HasSwiftAction()` patch is deliberately one-way and identity-gated: it
does nothing for every unit without Charged debt and never changes a native
`false` to `true`. Call of the Wild and other native/third-party swift commands
therefore see the same denied native budget during the charged turn.

## Rejected approaches

- Keeping the 0.0.94 `HasSwiftAction && SwiftAction <= 0` gate confuses current
  swift-command state with reaction availability and reproduces the human
  failure.
- Deleting all action checks would make In Harm's Way unlimited and would not
  consume the next swift action.
- Writing six seconds off-turn without debt is erased by the next
  `TurnController.Prepare()`.
- A once-per-round dictionary or marker clears at the wrong boundary under
  delay and initiative changes and is not save-safe.
- A global `HasSwiftAction()` rewrite would alter unrelated native and modded
  abilities. The selected postfix only denies the exact charged owner.
- Damage transfer, healing, attack replay, and rerolling remain rejected; this
  work changes only reaction economy and preserves the established
  pre-delivery target-redirection seam.

## Fixture blind spot

The 0.0.94 disposable fixture used a real `RuleAttackWithWeapon`, actual HP,
critical, and rider delivery, but it cleared runtime state and manually wrote
`SwiftAction = 0` immediately before positive cases. It ran outside a proven
enemy-owned turn. It therefore proved delivery after manufacturing the old
gate's expected state, but never exercised the human condition: turn-based
combat active, Kobold current, protector off-turn, no debt, and a genuine raw
`HasSwiftAction()` result. The 0.0.95 scenario is required to observe that
state without overriding it and then follow the debt through the protector's
next actual turn.

## Qualification status

The 0.0.95 candidate was installed through the guarded UMM deployment path and
then accepted in ordinary human turn-based play. The accepted package SHA-256
was `18633E561B4F1671B2F36B2A564CD23082A0A0695FD3068B394FB48C7F6DEF43`;
its installed DLL SHA-256 was
`4B99AD4EA367F5D5690FF5ACD8FAF449259CF2D72A3E3474A117A59D314000C6`
and its MVID was `0f990d38-8e07-46d0-a9a8-d11659c9c89d`.

The successful human log was preserved read-only:

- source:
  `C:/Users/howar/AppData/LocalLow/Owlcat Games/Pathfinder Kingmaker/output_log.txt`
- evidence copy:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20260823T190739Z-human-0.0.95-off-turn-pass/output_log-human-0.0.95-off-turn-pass.txt`
- size: 609,590 bytes
- SHA-256:
  `4B4DBAAD521A7AA056DAD21C80905A84F894C92C07511A212338D365719C2461`

The decisive `bodyguard-attack-4` occurred during genuine turn-based combat
with Kobold as the current-turn unit and HelpfulDefenderTest off-turn. The raw
native observation remained `hasSwiftAction=False` with
`swiftCooldown=2.99999833`, but the absence of immediate debt correctly made
the protector eligible. Bodyguard granted +4; KMG recorded
`immediateMode=TurnBased;debtAfter=PendingNextTurn`; the weapon-resolve and
`RuleDealDamage` seams both named HelpfulDefenderTest as the delivery recipient;
and the frame restored the original target after one completed delivery. The
subsequent native turn-start observation recorded
`state=charged;swiftCooldown=6`, proving that the off-turn use charged the
protector's next actual swift action.

The product owner separately confirmed the acceptance oracle in ordinary play:
In Harm's Way intercepted the off-turn attack that previously failed, the
protected ally received no intercepted damage, and the protector received the
delivery. This human mechanical result, the structured native-event evidence,
and the complete automated validation together qualify the 0.0.95 action-
economy repair. The historical 0.0.94 failure remains preserved above rather
than being reclassified as user action depletion.
