# Summon Same-Turn Activation Matrix

Status: SOURCE-QUALIFIED AND AUTOMATED-RUNTIME-QUALIFIED; HUMAN ACCEPTANCE PENDING

- Starting master: `cf1ca7aedf34ee76690f8864daedc9319a8e21a6`
- Branch: `codex/summon-same-turn-activation`
- Candidate version: `0.0.104`
- Deterministic suite: 1,307/1,307 PASS
- Focused policy/installed-boundary cases: 18 PASS

| Case | Qualified result | Authoritative evidence |
| --- | --- | --- |
| Ordinary unaccelerated native summon | Unchanged. Full-Round control retains Owlcat's appearance lock and lifecycle grace; first accepted opportunity remains cast round + 2. | `20260827T0207285929281Z-7a4390604a714979a0868bccd0494fd9` |
| Acadamae OFF | Native Full-Round timing; no Acadamae consequence. | Native-control run above |
| Acadamae ON, Standard | One prepared slot, Standard command, one save/publication, one cast-round opportunity, one normal following-round opportunity. | `20260827T0221563245711Z-a37e5f1796b147079fd344abdee13f1d`; repeat `20260827T0224255962019Z-2d12360517174bb6809f8852ad09bae5` |
| Legitimate Quickened summon | Swift command; caster retains Standard/Move and current-turn ownership; summon acts once in cast round and once normally next round. | `20260827T0216535483324Z-b013fe01b1f24d2abba2d245c40fd2da`; repeat `20260827T0219263194513Z-20ace7f093fd494995f2594b832c1fae` |
| KMG Expanded Summoning | Actual KMG choice uses the same generic summon path. | Multiple-summon run below |
| `1d3` summon | Three distinct KMG Eagles each join, receive initiative, and act exactly once in the cast and following rounds. | `20260827T0204341245023Z-6cee4a5f585b4108b98323109152b607` |
| Duplicate callback | Exact unit/current opportunity is idempotent; no second join, initiative entry, or action. | Focused tests plus all accelerated runtime runs |
| Following round | Native scheduling resumes; exactly one opportunity, no stale gate or duplicate entry. | Quickened, Acadamae, and multiple runs |
| Duration | Native duration retained; same-turn activation neither decrements twice nor adds a round. Acadamae expires at its qualified boundary. | Quickened/Acadamae/multiple/native-control runs |
| RTwP | No turn gate, artificial order, or duplicate command; native immediate AI remains active. | `20260827T0209540539692Z-667023d9194a45009557fbf8016b6c9b` |
| Cancelled/failed summon | No summon rule/unit/enrollment and no consequence. | Native-control run |
| Non-summon spawn | No summon lifecycle correlation or special enrollment. | Native-control run and focused policy tests |
| Save/load/reset | No serialized marker; exact transient windows clear at command/scene/combat/mode/reset boundaries. | Source/assembly-backed tests and cleanup assertions |
| Standalone profile | Full guarded matrix and two fresh-process runs for each principal accelerated path. | Runs above |
| Call of the Wild | Exact supported profile PASS and configuration restored byte-for-byte. | Quickened `20260827T0256056248286Z-07e13e9b019e4c3899bb3ff4d30c56d9`; Acadamae `20260827T0320286346830Z-770c3086d3cd440cabc36eec86ecf482` |
| Highest-risk combined | `gunslinger-high-risk-combined` PASS and configuration restored byte-for-byte. | Quickened `20260827T0335035677836Z-f55fb935816d4b999940684c3912c606`; Acadamae `20260827T0338217323762Z-d1517bbda5a44cc0968c8c28631ccd7b` |

## Proven engine divergence

The real player path is:

`UnitUseAbility` -> `RuleCastSpell` -> `ContextActionSpawnMonster` ->
`RuleSummonUnit` -> `EntityCreator.Tick` -> live `UnitEntityData` -> native
combat enrollment and initiative controllers.

The comparison proved two consecutive divergences:

1. `RuleSummonUnit.OnTrigger` reads immutable
   `Context.SourceAbility.IsFullRoundAction`, not the live accelerated command.
   A Standard/Swift summon therefore receives `SummonedUnitAppearBuff` and six
   seconds of lifecycle grace even though its caster lawfully spent only that
   accelerated action.
2. After that state is normalized, `UnitCombatJoinController.Tick` still
   returns early during turn-based combat whenever
   `!CombatController.IsPassing()`. A summon created inside the caster's
   unfinished turn is consequently omitted from combat enrollment before
   `CombatController.ChooseNextUnit` may advance the actor or round.

The ordinary Full-Round control reaches those boundaries at Owlcat's native
accepted time and is not correlated as accelerated. The fixed Standard/Swift
paths retain the caster turn only until every exact spawned unit has passed
native `JoinCombat`, order membership, and initiative preparation.

## Repair invariants

- Only a genuine `RuleSummonUnit` rooted in the exact accelerated spellbook
  invocation can arm a window.
- `UnitEntityData.JoinCombat()` is invoked once per exact correlated summon;
  Owlcat's own join, preparation, initiative, and turn-order handlers do the
  authoritative work.
- `TurnController.Tick` is held only while the same caster/controller/round is
  current and enrollment is incomplete; 240 attempts is the fail-open bound.
- No production code writes initiative, order collections, cooldowns, caster
  resources, AI commands, or current-turn ownership.
- Each spawned unit is keyed independently, so one `1d3`/`1d4+1` invocation
  cannot suppress or duplicate another member.
- RTwP, ordinary Full-Round summons, summons outside the active caster turn,
  and arbitrary unit creation fail closed to native behavior.

## Evidence classification

- Source-qualified: yes.
- Automated runtime-qualified: yes, including standalone, Call of the Wild,
  and highest-risk combined profiles.
- Human-accepted: no; the concise in-game review sequence remains outstanding.
