# Whiteout native attack contract — 2026-10-05

Inspection classification: **EXACT-NARROW-ATTACK-SEAM**.
Gameplay qualification: **NOT PERFORMED — Gate A BLOCKED-WEATHER-FIXTURE**.
WhiteoutPublished: false. No attack adapter or transient marker was installed.
A successful signature/IL inspection is not real combat qualification.

Installed native `Assembly-CSharp.dll` MVID is
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`, SHA-256
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
Exact method: private instance
`Kingmaker.RuleSystem.Rules.RuleAttackRoll.TryOvercomeTargetConcealmentAndMissChance(): System.Boolean`,
zero parameters, 149 IL bytes. Its return value represents passage of this
native miss-chance stage; true is not an overall attack hit.

## Native stage and call order

| Condition | Exact native behavior |
|---|---|
| IgnoreConcealment | Native classification used by the initial comparison becomes None; if the earlier MissChance branch is not selected, return true |
| MissChance stronger than concealment | Roll the native D100MissChanceRoll and return MissChanceRoll > MissChance directly. It does not also trigger a RuleConcealmentCheck in this branch |
| Otherwise | Trigger one RuleConcealmentCheck(initiator, target, true), store it on this exact RuleAttackRoll, return its Success |
| No native concealment | RuleConcealmentCheck's None path succeeds without a concealment d100 |
| Partial / Total | Native 20% / 50% classification, native d100 and native reroll handling remain owned by RuleConcealmentCheck |
| Native miss | Return false to the native miss branch; a future Whiteout adapter must preserve false and consume no Whiteout roll |

`RuleAttackRoll.OnTrigger` is 1,255 IL bytes. The call at IL_0151 branches to
IL_0464 on false, recording native concealment/miss-chance failure (AttackResult
value 8). On success it creates RuleCalculateAC at IL_016e and proceeds through
ordinary AC, attack-bonus and hit resolution. Mirror-image absorption is later,
at IL_0448; parry handling is after IL_046b. RuleAttackWithWeapon triggers the
attack roll before its downstream hit/damage-resolution rule. AutoHit and
AutoMiss branches bypass this stage earlier; a narrow stage adapter must not
invent coverage for those branches. This includes native automatic-hit handling
for some helpless-target touch attacks.

Neither the 149-byte stage nor its proposed final-success adapter computes cover,
range, approach, legality, movement or threat. Native AC/attack-bonus modifiers,
shoot-into-combat handling, weapon stats, mirror image, parry and damage would
retain their original positions. No cover/threat/pathing rewrite is justified.
These are installed-engine ordering findings, not runtime preservation claims.

## Coverage boundaries

Both RuleAttackRoll constructors take the exact selected weapon or
RuleCalculateWeaponStats and obtain its native BlueprintItemWeapon.AttackType.
Melee, natural and ranged attacks use the same rule; the existing project firearm
fixture also constructs and triggers real RuleAttackRoll. Native ray/ranged-touch
AttackType values share OnTrigger when they reach this stage. No Whiteout ray,
weapon, natural, firearm or nonattack runtime matrix was executed in this mission.

A future adapter applies only to actual calls of this exact method. Nonattack
saving-throw spells, environmental or automatic damage have no extra roll here.
Maneuver coverage must follow an actual native call to this stage; metadata alone
cannot establish it. No broad rule getter or attack-kind rewrite is authorized.

## Bypass, replay and failure requirements

The retained `SeekingConcealmentSuccessPatch` changes only Success for the exact
RuleConcealmentCheck stored by the current parent attack. It does not set
IgnoreConcealment. `SeekingExactItemResolver.IsAuthorized` requires the configured
canonical project enchantment object, an actual ranged weapon, exactly one
project Seeking marker on exactly one matching enchantment, and rejects foreign
markers. Display names, icons and copied identities are insufficient. These
qualified Seeking files remain unchanged.

A future Whiteout postfix would require exact valid initiator/target, exact
request-local provider, current valid module/native contracts, outdoor Rain/Snow
with ActualWeather Light+, then separately test IgnoreConcealment and the exact
Seeking resolver. Preserve every native false result, including the stronger
native MissChance branch. It must never replace concealment classification or
change UnitPartConcealment.Max. A native 20% success followed by independent
10% Whiteout produces 28% combined misses; the existing pure 100×100 enumeration
proves the arithmetic but does not prove attack execution.

Native OnTrigger contains one call to this stage. External replay can invoke it
again, so one exact RuleAttackRoll must own one cached Whiteout decision. The
existing WhiteoutAttackDecision pure model establishes once-only resolution.
A future engine adapter needs an attack-keyed weak table or equivalent native
carrier; current target/weather validity must be rechecked before applying any
cached protection. Diagnostic forced rolls must be scoped to the exact guarded
attack and thread; production should use the native Dice.D100 path. Exceptions
or contract mismatch must preserve the original native result.

No Harmony patch was authored or installed, no attack-scoped table or diagnostic
attack override was bound to gameplay, and no global RNG/concealment/range/weapon
state was altered. Gate A failed exact scene-reference restoration, so the
mission stopped before production implementation and two-process attack
qualification. See [native weather blocker](WHITEOUT-NATIVE-WEATHER-CONTRACT.md)
and the [handoff](../../CODEX-WHITEOUT-UNPUBLISHED-FOUNDATION-HANDOFF-2026-10-05.md).
