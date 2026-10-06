# Sprint17 bounded native Bite-distance review

Status: exact983828bf BOUNDED COMMAND/CONTACT51/51 PASS; full Sprint17 NOT QUALIFIED.
Source parent24924453; earlier exact474be538 remains43/51 FAIL in its archive.
Laptop PR26 only; DATA archive-only/ZERO PORTS.32 roots hidden.

## Preserved failure and native audit

The8 at-rule original-vertex contacts miss by0.37891674-0.426136971m.
A separate replay of original vertices, native bindposes, bone matrices and
weights reproduces all8 measurements within1 micrometre. All gap vectors are
horizontal; this is not a bounds-space calculation error. Exact same-frame
Viper poses remain distant. Constrictor's later command-ended poses cannot
substitute for contact. The previous geometry-review wording did not establish
that the authored head must be stretched; no mesh change has been made.

The preserved native Bite census has two entries, each with a short3.5m and
long8m named clip (all2.4s). Exact action:
`Purple_Worm_AnimationSet_Bite 1`. Ordered clips:
`BiteAttack01_Short_3.5m`, `BiteAttack01_Long_8m`,
`BiteAttack02_Short_3.5m`, `BiteAttack02_Long_8m`.
All8 failed command contacts played the first short clip.

Private exact-reference IL audit:
- UnitAttack.TryStartNextAttack assigns world center-to-center target distance
  to the handle before native animation start, without view-scale compensation.
- UnitAnimationActionSpecialAttack.OnStart uses an entry's HasRangeBlend,
  BlendRanges and variants; StartBlendedAttack compares those ranges with the
  handle's AttackTargetDistance getter and selects/clamps/blends native clips.
- Neither path compensates for the two original bodies'0.2 view multiplier.

The serialized numeric BlendRanges values have not been recorded;3.5/8 above
are native clip names, not a claim that those exact float values were measured.
The candidate corrects the scale mismatch through the existing native selector.
Actual selected clip, event/contact and ordinary command behavior passed in
all8 cells. It does not inject an attack event.

## Bounded correction

A getter postfix returns worldDistance/0.2 only for the two exact production
snake GUID/name/prefab identities, Expanded Summoning enabled, the live owned
original-body mesh, the same view animation manager and its exact Bite action,
and the complete ordered native clip census above. Negative/nonfinite/overflow
inputs reject. It does not write the handle's stored value or any native range.
Native Worm, Salamander, research carriers, fallback/released bodies, disabled
module and other actions retain native behavior.

The action, clips, events, rig, geometry, actor positions, movement, weapon,
reach, footprint and combat rules are unchanged. Selecting a different native
clip can change the native event time; no identical-time claim is made.
The command observer now records the getter result alongside the actual clip,
native clocks and exact at-rule weighted-world contact.

Behavior-first tests cover both exact identities, representative distances,
disabled/fallback/non-bite inputs, native/foreign identities, malformed input,
overflow and missing/reordered/replaced clip sets.

## Source gates and exact runtime result

260 focused PASS. Complete unfiltered2062 PASS90.0s; repository/static/icon/
manifest, clean14-reference Release and strict320-member package PASS188.7s.
Log: artifacts/sprint17-bite-animation-distance-dirty-gate.log.
This is a precommit source gate, not an immutable runtime candidate.

Exact983828bfa9b1aff2d79fab3eb8a2954ec28e4a2f passed260 focused,
2062 full85.9s,complete184.7s,508 preflight,168 orchestration,17 provenance,
persistence11/3/19,crowd5/7,clean14-reference Release and strict320 package.
Fresh Steam640820: smoke11/11,profile/body/rules62/62,commands51/51 PASS.
All8 at-rule jaw gaps are0m with actual native BiteAttack01_Long_8m playback,
weight1,projected distance8.321414-8.514899 and world1.6642828-1.70297992m.
View0.2,corpulence0.6/0.5,bite2ft and native approach sum1.7096 are unchanged.
No assertion, threshold, mesh, actor position, stored distance or shared
clip/range mutation. Other distances/size states/variant02 are not inferred.

Actual snapshot2258281699283Z restored23:11:14.0412304UTC;136/.117/exact
fingerprint;lease Completed/recovery=false/released;no game/shared lock/
this-worktree staging/save write. No baseline access or publication.
[Exact evidence and artifact identities](EXPANDED-SUMMONING-SPRINT17-SNAKE-COMMAND-PASS-EVIDENCE.json).
Full lifecycle/crowds/UI/routes/persistence and Salamander remain open.

## Private evidence identities

No proprietary IL or native assets are committed.
- Exact Assembly-CSharp SHA256:
  3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb.
- artifacts/sprint17-native-bite-distance-audit.log:
  3c01d6683d9270325644ad98514d9ed3467ef618e2f9301f92b12d4d0cc397b3.
- artifacts/sprint17-bite-distance-input-audit.log:
  588ea13ee4b9dd8fc49554c1b40412c8fc887925f7f5c98f02090690bf68d431.
- artifacts/sprint17-command-contact-geometry-replay.json:
  a711c6caac61f0995da842efc5716d119c21f8f4b5724ee56508b8251b8bccb4.
- Preserved live census:
  runtime-evidence/20261006T1207285117966Z-disposable-expanded-summoning-serpentine-bodies/sprint17-original-body-review.json.

HumanReview: NOT_PERFORMED_NONBLOCKING.
Sprints14-16 remain complete. Finish Sprint17/fullPhase2B,then owner review.
Phase2C authorized but deferred; no18-22/merge/release/version bump/permanent deployment.
