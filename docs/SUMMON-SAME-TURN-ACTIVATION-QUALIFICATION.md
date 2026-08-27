# Summon Same-Turn Activation Qualification

Status: SOURCE-QUALIFIED AND AUTOMATED-RUNTIME-QUALIFIED; HUMAN ACCEPTANCE PENDING

## Qualified candidate

- Starting qualified master:
  `cf1ca7aedf34ee76690f8864daedc9319a8e21a6`
- Working branch: `codex/summon-same-turn-activation`
- Starting version: `0.0.103`
- Candidate version: `0.0.104`
- Production source freeze:
  `39e282eaed4a2f74393350867272d060ad87e75e`
- Compatibility diagnostic/source predecessor:
  `9d847e714c4965eb8866bb5163088384bbef6546`
- Package SHA-256:
  `6AC31F83253B4A616E274656F44955F3ABC575008A1D6457A75F700E74F4623A`
- DLL SHA-256:
  `1467A767AF9FF16CE34A2ADB6120216F93438667B27EE8F93B8FF7AB45CD1444`

All runtime launches below used the guarded Steam App ID 640820 mechanism.
Save-backed runs named only disposable `KMG_AUTOMATION_WORKING`; no run wrote
the save. Compatibility runs used the guarded request-local real-spellbook
fixture. Runtime artifacts remain machine-local under
`C:/Dev/KingmakerGunslingerLab/runtime-evidence`.

## Failing baseline

### Boundary 1: stale Full-Round appearance classification

- Scenario: `summon-same-turn-activation`
- Run ID:
  `20260826T1445424590411Z-3b96e766c8b144449164781c019dcc51`
- Result: expected FAIL

A real prepared Quickened Summon Monster spell followed
`UnitUseAbility -> RuleCastSpell -> ContextActionSpawnMonster ->
RuleSummonUnit`. The command was Swift and the caster retained Standard/Move,
but the summoned unit reached its cast-round turn with all three resources and
`SummonedUnitAppearBuff`, making `IsAbleToAct=false`. It became able to act
only after the lock cleared. Its lifecycle was also 126 rather than 120
seconds.

Installed IL showed `RuleSummonUnit.OnTrigger` selecting this branch from
immutable `Context.SourceAbility.IsFullRoundAction`, not the actual
accelerated invocation.

### Boundary 2: mid-turn combat enrollment

- Scenario: `summon-same-turn-activation`
- Run ID:
  `20260827T0128367929373Z-3e50f786ced445199af14907997fd094`
- Evidence directory:
  `20260827T0128367773213Z-summon-same-turn-activation`
- Result: expected FAIL on cast-round opportunity/native command/single action

With the appearance lock and duration already corrected, the exact spawned dog
was live immediately after spawn but had:

`inCombat=false; initiative=0; turnOrder=false; round=1`

It was not enrolled until round 2, where it first appeared with:

`inCombat=true; initiative=2; turnOrder=true`

The cast-round opportunity count was 0/1 while the following native opportunity
was 1/1. This isolates combat enrollment, not creation, lifecycle, action
resources, or AI capability, as the remaining first divergence.

Bounded installed IL proved that `UnitCombatJoinController.Tick` returns after
updating player combat whenever turn-based `CombatController.IsPassing()` is
false. During the still-live caster turn that condition prevents the summon
from reaching native `UnitEntityData.JoinCombat()`. Meanwhile
`CombatController.ChooseNextUnit` may advance the actor/round before disposing
the caster turn.

These are automated failing reproductions, not synthetic unit-spawn tests and
not human observations.

## Engine-faithful repair

The final implementation carries exact reference correlation from the real
three-argument `UnitUseAbility` constructor through its exact `RuleCastSpell`,
source context, `RuleSummonUnit`, and each spawned `UnitEntityData`.

For a genuinely accelerated summoning spell resolving in its caster's active
turn, it removes only the stale appearance lock/lifecycle grace, briefly holds
only that caster's incomplete `TurnController.Tick`, and calls the exact
summon's native `UnitEntityData.JoinCombat()` once at the
`UnitCombatJoinController.Tick` boundary. Native
`UnitCombatPrepareController`, `RuleInitiativeRoll`,
`CombatController.HandleUnitRollsInitiative`, ordering, AI, and command logic
remain authoritative. The gate releases once every successful live unit is in
combat/order and prepared, or fails open at 240 observations.

Per-unit `JoinAttempted` state and canonical enrollment/prepared state make
duplicate callbacks no-ops. Ephemeral state clears on activation, destruction,
expiration, combat end, TB disable, scene disposal/load, and runtime reset.
Nothing is serialized.

## Standalone runtime matrix

| Scenario | Evidence directory | Run ID | Result |
| --- | --- | --- | --- |
| Quickened fresh process 1 | `20260827T0216535327527Z-summon-same-turn-activation` | `20260827T0216535483324Z-b013fe01b1f24d2abba2d245c40fd2da` | PASS 10/10 |
| Quickened fresh process 2 | `20260827T0219262881733Z-summon-same-turn-activation` | `20260827T0219263194513Z-20ace7f093fd494995f2594b832c1fae` | PASS 10/10 |
| Acadamae fresh process 1 | `20260827T0221563089446Z-summon-same-turn-acadamae` | `20260827T0221563245711Z-a37e5f1796b147079fd344abdee13f1d` | PASS 11/11 |
| Acadamae fresh process 2 | `20260827T0224255805978Z-summon-same-turn-acadamae` | `20260827T0224255962019Z-2d12360517174bb6809f8852ad09bae5` | PASS 11/11 |
| KMG Expanded `1d3` | `20260827T0204340932544Z-summon-same-turn-multiple` | `20260827T0204341245023Z-6cee4a5f585b4108b98323109152b607` | PASS 10/10 |
| Ordinary/OFF/cancel/non-summon | `20260827T0207285617185Z-summon-same-turn-native-control` | `20260827T0207285929281Z-7a4390604a714979a0868bccd0494fd9` | PASS 7/7 |
| RTwP | `20260827T0209540382903Z-summon-same-turn-rtwp-control` | `20260827T0209540539692Z-667023d9194a45009557fbf8016b6c9b` | PASS 5/5 |

### Quickened result

Both runs used a legitimate prepared/metamagic spell through the actual player
spellbook and command. Swift alone was spent; the caster retained Standard,
Move, and current-turn ownership. The summon entered combat/order, received
native initiative, issued one native cast-round command/action, then one normal
following-round command/action. Duration remained 120 seconds; duplicate
observation did not rejoin or reactivate it; cleanup was exact.

### Acadamae result

Both runs used a real eligible prepared spell with Acadamae ON. One prepared
slot was spent by a Standard command. Exactly one Fortitude save and one
consequence publication occurred, with the established successful-save result.
The caster's accepted remaining action state was unchanged. The summon received
one cast-round and one following-round opportunity, then expired on the exact
qualified 12-second boundary without an orphan or duplicate.

### Multiple/KMG result

One legitimate accelerated KMG Expanded Summoning `1d3` option created three
distinct Eagles through one command/spawn graph and three genuine summon
rules. Every Eagle independently joined combat, received native initiative,
acted once in the cast round, acted once in the following round, and retained
normal duration/cleanup. No caster-wide or invocation-wide idempotence key
suppressed a member.

### Native and RTwP controls

The ordinary Full-Round/Acadamae-OFF spell retained Owlcat's accepted
appearance lock, six-second lifecycle grace, and cast-round + 2 first lawful
opportunity. Cancellation spent no slot and produced no unit or enrollment.
The live non-summon control received no special processing.

RTwP armed no turn-based window, created no artificial actor/order state, and
left native appearance, AI command cadence, and duration untouched.

## Compatibility qualification

| Profile | Scenario | Transaction | Run ID | Result |
| --- | --- | --- | --- | --- |
| `gunslinger-call-of-the-wild` | Quickened | `compat-20260827T025456Z-ccfac4e2b42c` | `20260827T0256056248286Z-07e13e9b019e4c3899bb3ff4d30c56d9` | PASS 10/10 |
| `gunslinger-call-of-the-wild` | Acadamae | `compat-20260827T031923Z-18b90b4890da` | `20260827T0320286346830Z-770c3086d3cd440cabc36eec86ecf482` | PASS 12/12 |
| `gunslinger-high-risk-combined` | Quickened | `compat-20260827T033355Z-58bb196d2c30` | `20260827T0335035677836Z-f55fb935816d4b999940684c3912c606` | PASS 10/10 |
| `gunslinger-high-risk-combined` | Acadamae | `compat-20260827T033713Z-237e27e719da` | `20260827T0338217323762Z-d1517bbda5a44cc0968c8c28631ccd7b` | PASS 12/12 |

All four accepted runs restored the exact pre-transaction optional-mod and
configuration bytes. The fixture invokes the installed native join controller,
prepare controller, and CombatController under a scoped EventBus subscription;
it does not directly insert order entries or fabricate initiative. It restores
the request-local pause bit immediately after native ticking.

Earlier fixture diagnostics and wait-caster-turn timeouts are retained as
non-acceptance evidence. They drove the exact native-controller, EventBus,
tick-order, pause restoration, and turn-gate observations; none is counted in
the PASS matrix.

## Validation gates

- Focused summon policy/installed-boundary tests: 18 PASS.
- Complete deterministic domain/reflection suite: 1,307/1,307 PASS.
- Repository/static validation: PASS.
- Exact installed-assembly compilation/tests: PASS.
- Runtime preflight: 156/156 PASS.
- Clean exact-reference Release build and output validation: PASS.
- Existing SoundBank/asset validation: PASS.
- Deterministic package creation and strict package validation: PASS.
- Compatibility profile definition/resolution tests: PASS (12 profiles).
- Expanded Summoning compatibility profiles: PASS (5 profiles).
- `git diff --check`: PASS.

The final documentation-only seal reruns the applicable deterministic,
repository, clean-build, output, package, and strict-package gates before the
branch is handed off.

## Resolved-feature regression result

- Summon popup placement/sizing/scrolling/deduplication: established tests PASS;
  production UI untouched.
- Expanded Summoning real spellbook, slot, native choices, quantity, and
  celestial/fiendish contracts: tests and KMG runtime control PASS.
- Acadamae OFF/ON action type, slot, one save/consequence, successful and
  failed fatigue, already-Fatigued Exhausted escalation, Cord interaction, and
  cancellation contracts: established tests PASS; production consequence code
  untouched.
- Native duration, expiration, dismissal, cleanup, and save/load safety:
  targeted tests/runtime controls PASS; no persistent marker added.

## Qualification classification

- Source-qualified: **yes**.
- Automated runtime-qualified: **yes**.
- Human-accepted: **no**.
- Release disposition: **owner-approved for merge and public publication on
  2026-08-27**.

The remaining human-only check is a concise in-game observation of one
Acadamae Standard summon and one legitimately Quickened summon in turn-based
combat, including caster remaining actions and the following round. It is not
needed to establish the automated evidence above and has not been claimed.

Public release authorization does not reclassify automated evidence as human
acceptance; the two statuses remain deliberately separate.
