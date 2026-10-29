# Sprint 34 - hidden native qualification

Current stage: early hidden native mechanics PASS; remaining combat/presentation,
public routes, persistence and release tooling are in progress. Product/build/
promotion root `C:/Dev/KingmakerGunslingerLab/wt/s34`, branch
`codex/expanded-summoning-sprint34-bearded-devil-kyton`; reviewed base
`4b7e3d914883ac695a4146c03de790b2024ea38c`. All ten routes remain hidden.
Sprint33 remains closed; its original-source ON REUSED_PASS is distinct from
final-artifact OFF/combined observations. Historical deferred checks remain NOT_RUN.

Latest executed body candidate (not a release): source
`3f5fffa9d3ed77c8315748c8a59f88781be2f82c`, purpose `sprint34-infernal-bodies`.
ZIP `ef391ca9aa386f650b9b00314d6a1844bcab40805d6ad555661ada2ef4eb0c65`;
DLL `a4fdd7a3d9c742174a2298e434e3bca870087a802724ea604f8773b4161f4092`;
MVID `d9a5333f-cea6-459f-8528-f92da084820f`;
fingerprint `f6075e28432a22413cde514021490835fb49ee40e1a92901855acdccd471036b`.
Build04 PASS265.7s; native body04 raw PASS29/29,421.8s including restoration.
RunId `20261029T2334112439495Z-50b8760a061240419bfd28f5c4bd1ca1`;
239582ms in-game. Native BakeMesh agrees with contact vertices within0.000006m,
but every following ordinary frame has reset chains. Installed UnitAttack.OnAction
increments its planned limb immediately after the hit. The source correction
retains that exact limb through a bounded0.35s follow-through; it adds no attack.
The native glaive mesh is lengthwise localY; its measured contact frame now
supplies a fixed grip-axis correction, respecting the mesh's actual child frame.
**Presentation acceptance remains open until the changed artifact runs.**
Distinct bodies, movement, alternative native clips and private teardown were
observed. The older raw PASS is not visual-contact acceptance or owner approval.

Early mechanics candidate (not a release): source
`8b59b45717c84b403991333bd3d4d0fe844983ec`, purpose `sprint34-infernal-early`.
ZIP SHA256 `b86aef9d5b3984fc34fc98139f19af6d793d882cc6af14c90290f72014c863b2`;
DLL SHA256 `0aae673463b9dcf10bc27464a2983d432816618232c4ef655d99e1a31ece094f`;
MVID `19c76e2e-4398-4a60-a5fd-cdd910109d24`;
source fingerprint `2602f469f4fb378cdd8beb1fbc968892007f04c634c09b714e90d40ac46f6ce3`.
Build06 passed in262.7s, selected10/2766 tests and strict package validation.

Early native03 RunId `20261029T2151470387493Z-6edde9a480e54454a41135b162c2954a`
**PASS20/20**,371.9s including guarded orchestration/restoration. Chain control
spent one standard action, extended10 to25feet and made no attacks; the later
native full attack executed exactly four private chains. Actual CL7 cures rolled
19/17 and healed12/13HP with wound closure; roll7 rejected healing, left Damage157
unchanged and retained the wound. Two real glaive sources caused one2HP tick;
each summon removal retired only its own wound. Both original bodies attached;
live visibility, motion and weapon contact still need acceptance. No personal
owner aesthetic approval. Raw evidence stays under runtime-evidence/
`20261029T2151469985118Z-disposable-expanded-summoning-sprint32-native-review`.

Retained dated history (2026-10-29; immutable original verdicts):
- Native01/source8505d4ac, RunId `20261029T2109139961185Z-69ea789acecd4becb12b9300bb1fef21`,
  FAIL450.8s. Six successful spell cures; seventh parent/held-touch child produced
  no heal event. Original observer omitted touch rolls, so no retrospective miss
  claim. Observer now records all recipients/rolls and permits at most three real
  held-charge retries following an observed miss, without forced outcomes.
- Native02/source52c66717, RunId `20261029T2136335444911Z-af0f98356f614c5c966434ebe1161963`,
  FAIL367.9s. Spell success/rejection observed; two sources gave one bleed tick,
  but removing one summon wrongly removed both wounds. Installed BuffCollection
  IL confirmed global buff-removal broadcast. Production now checks the removed
  buff's exact owner, matching the established lifetime consumer. Native03 proves
  the corrected behavior. Neither earlier failed receipt becomes final-artifact PASS.

## Reuse, gap and proof

| Required behavior | Existing consumer / precise gap | Small patch | Required proof |
|---|---|---|---|
| Two profiles and ten untemplated SM routes | Catalog generator, sanitizer, native caster/quantity path; neither key is live | Append two SM V profiles; register only when all dependencies resolve; suppress all ten roots | Derived surface, old identity prefix, actual casts/slots/quantities/durations |
| Glaive/claw alternatives, reach, equipment | Salamander weapon consumers, giant native equipment switches and owned no-loot instances; no Barbazu profile | Private glaive/two-claw modes and exact weapon reach; no global reach grant | Single/full/charge/AoO, close fallback, disarm, live Strength/Power Attack, independent items |
| Both-claws Beard | DireApeRendGate/ClawRendTracker scope actual command, limb and target; Rend payload has different semantics | Reuse bookkeeping only after both outcomes; separate beard damage/disease, duplicate protection | Same target success; split/miss/replay/sibling/stale-command negatives |
| Infernal Wound and disease | Actual RuleHealDamage modifier/HP consumer; native disease graph and ordinary bleed require inspection | Exact victim/source wound; native spell check, nonmultiplying ticks, day-scale disease, source/summoner cleanup | Real wound HP, spell rejection/success and closure, overlapping sources, unrelated patient/effects, area loss |
| Kyton chains, gaze, defenses | Native commands, typed bonuses, AddEffectRegeneration, fear/save components; approved owned-chain and targeted-gaze policies | Apply the approved action/ownership policy; private chains, armor bonus and actual regeneration | Four native attacks; control/action costs; fear saves; silver/good bypass and death prevention |
| Models/icons/lifecycle | S33 original mesh pipeline; private resource/fader/animation ownership; two new paintings | Inspect native bodies once; compose distinct original bodies if needed; export the paintings | Actual attachment, weapon contact/motion, icon thumbnails/menu, donor/instance/module-off teardown |
| Persistence/release | Existing mixed Working trio and exact-artifact publisher | Exact two-creature scope and selector; no new framework | One ON trio, one OFF trio, combined profile, reproducibility, public bytes/lineage, exact restoration |

## Delivered source and decisions

- Both exact SM V keys are registered with all dependencies and ten suppressed
  V-IX roots. No SNA or template children. Derived totals: 126 creatures,
  SM104/582, SNA105/588; 1170 generated plus29 wrappers when fully public.
  The 7511 released identities remain unchanged; 74 additions comprise two
  units, ten roots,22 private carriers and40 deterministic caller adapters.
- Private glaive/claw modes, conditional Beard, day-scale Devil Chills and
  source-owned Infernal Wound use native attacks/damage/healing. Kyton has
  four private chains, typed armor, native regeneration and its own defenses.
  Native behavior is not inferred from compilation.
- Owner approved standard-action owned-chain control with no attack on activation
  and later normal attacks, plus targeted standard-action gaze. Owner said
  "Don't worry about implementing the heal skill." No replacement skill action;
  actual spell CL16 rejection/cure remains required. See [rules](SPRINT34-RULES.md).
- Both original paintings/exported icons are bound. Two distinct original bodies
  use the measured free-hand rig. Offline decoding/winding/albedos pass; native
  glaive grip, chain contacts/extension and motion still need proof.
  Provenance: `assets-source/original-models/sprint34-infernal-outsiders/SOURCE.md`
  and the original-icons Sprint34 prompt record. Private captures stay local.
- Early observer records actual command/action costs, all recipients, attacks,
  damage, spell slots, healing decisions and resulting owned effects. It never
  forces hit/save/heal results. Seven real cure attempts are bounded by the
  healer's slots. Controls, four-chain execution and healing are the first cells.

## Acceptance and finite plan

| Group | Current evidence | Remaining |
|---|---|---|
| Profiles/defenses/donor isolation | Early native HP/AC/BAB/saves match both baselines | Typed grants, defenses and donor isolation |
| Barbazu modes/Beard/wound/disease | Early healing/tick/owner retirement; body03 real two-claw Beard packet PASS | Charge/AoO/live scaling, disease/lifetime controls and final coverage |
| Kyton chains/gaze/armor/regeneration | Development control/cost/four-chain subobservations | Final qualification; saves, bypass and other action modes |
| AI/actions/cleanup | Existing fixture and owned lifetime reuse | RTWP/TB, cancellation, independent items/no loot |
| Models/icons/ten public routes | Original assets; offline decoder/icon PASS | Native body/contact/UI; all actual public casts |
| Mixed persistence/integration | Exact mixed scope/observer implemented; no native campaign begun | One mixed ON trio and OFF trio, targeted combined |
| Package/publication/restoration | Exact publisher selector and42 refusal/acceptance checks PASS | Final gate/release/downloads |

Focused development uses named filters and production compilation under the
ordinary15/30-minute policy. Focused gate16 compiled production and passed10/2766
selected tests in29.5s after the same-command Beard boundary repair (gate15 before
that small addition passed in31.4s). The exact Sprint34 publisher extension reuses the existing
artifact/run/load/restoration linkage;42 synthetic acceptance/refusal checks PASS,
including complete qualification and premerge policy. The18 affected Sprint33
publisher checks PASS; no historical gameplay campaign was replayed. All four
S34 model files remain covered by strict package membership/hash validation.
Gate17 passed11/2767 focused tests plus production compilation in35.4s.
The mixed `infernal-outsiders` observer reuses the current native Working trio:
one Barbazu, one Kyton, one private recipient carrying source-specific wound,
day-scale disease and finite gaze, plus Kyton-owned chain control. Saved item
owners/flags/slots and cold effects are compared exactly. Native effect-only
preparation is explicitly not cast, attack or saving-throw proof. Current source
adds the exact module gate to the private S34 lifetime/injury consumers; OFF
native proof remains required. The installed-stack wrapper admits this scope
only for the final frozen candidate, with existing exact-save/exit protections.
Gate18 passed the persistence/body-readback observer batch and11/2767 tests, then33 compiled
persistence request admission/refusal cases, in33.2s total. Two installed-stack
wrapper probes also refused scope case variation and development aliases before
transaction entry. No full milestone gate yet.

Gate21 compiled the body correction and initial mechanics observer,11/2767
focused tests PASS35.2s. Gate19/20 failed compilation on observer API usage;
actual CalculateParams and UnitDescriptor.GetFact contracts corrected together.
Gate22 compiled the public-route observer and passed the same11 tests plus42
publisher acceptance/refusal checks in41.1s. The completed gaze sight/immunity
cells are included in the next candidate build's focused compile/test boundary.
The public observer reuses native prepared parent/child casts, slot/quantity/
duration/cleanup; its exact existing `sprint32-sprint34-player-route-*` assertion
prefix is now also required by the publisher. Ten routes remain hidden.

Next executable action: commit/push this coherent batch and build one
`sprint34-infernal-mechanics` development candidate. Use it for the bounded
mechanics phase (profiles/typed defenses, real gaze saves/expiry/sight/immunity,
native regeneration) and the changed body phase. Estimate4.5min build plus two
6-8min guarded sessions from retained timings; restore each and diagnose any
failed boundary before continuing. Actual fatal-injury regeneration proof,
AI/TB, other combat/lifetime controls, final routes and persistence remain open.
No Working envelope has begun. Before final freeze, integrate legitimate upstream
once and recheck patch availability.

Body history (2026-10-29; retain original raw receipts/verdicts):
- Body03/source124c370b, RunId `20261029T2310492875407Z-bc0e5dff17be464a8368ee3d7b68abfe`,
  raw PASS29/29,403.9s. Native glaive2/claws2 with separate7HP Beard; Kyton four
  chains at10/25ft and motion. Images leave contact open. ZIPc29af2c8a09fac665792ed07502fec5439c593087a64cf01482af6c8b4eac071;
  DLL15c7f36816357d836058b881e553c90ad6fe63a9364bdc67860b31d9e7dd7165;
  MVIDc615c208-18c0-4870-820e-be59a8e1965e. A prior43-check commentary count
  was a transcription error; immutable result has29.
- Body01/source10244694, RunId `20261029T2218589057622Z-cc09ace85a7b4102bb588f5d700a1a15`,
  FAIL336.2s: visible/selectable independent Barbazu bodies; move interrupted after
  one frame with zero velocity. No movement/contact acceptance from that run.
- Body02/source30a81e4e, RunId `20261029T2240404051528Z-1e8e7d65260c4264b281c434cdd29dd2`,
  FAIL349.9s: Barbazu travelled5.763m at observed4.064m/s, then executed two native
  glaive attacks. The next natural-mode selection failed. Installed WeaponSlot IL
  returns EmptyHandWeapon for inactive MaybeWeapon; production AI and observer
  now inspect MaybeItem before the normal equipment-switch command, and retain
  before/after slots. Native follow-master interruption is traced in this receipt.
  Reader review rejects its animation sub-PASS: a null-string JValue passed the
  old token-type check. No valid clip/contact acceptance is claimed. The observer
  now requires a nonempty clip; the private glaive visual style is bound to the
  retained actual native main-hand mapping/act event without changing mechanics.
  Glaive bonuses6 then12 and attack bonuses9/4 match the known non-player native
  PowerAttackWatcher transition (existing Hound/Astral fixtures); visual commands
  now settle native ON and record the toggle at each contact. This is not an OFF
  baseline or live-scaling proof. Kyton motion/contact remains NOT_RUN.

Final selection is one Sprint source/build/package gate, grouped mechanics and
presentation/routes, six mixed persistence processes (one ON trio, one OFF trio),
and a targeted highest-risk combined session. Reproducibility only at release.
Visibility-only evidence reuse must be dependency-proven and publisher-supported.

## Intake, permissions and restoration

HOWARD-LAPTOP; actual human installation0.0.117, observed rather than pinned.
Sixteen private build references hash-match the installed2.1.7b runtime.
Intake local `artifacts/sprint34-intake/intake.json`, SHA256
`bfbef80b0beee9afdadc527211780b615f49f9ea87be7183405d220b8d1ca50d`,
covers967 mod entries,118 save entries,20 references,83 settings, sound bank,
tasks/processes/leases. After body04, transaction
`compat-20261029T233135Z-3ed438f66256` reached Restored with restorationVerified=true. Settings restoration and full intake Compare PASS;
original installation, protected saves, sound bank/references and processes match.
No Working envelope has begun.
KMG_AUTOMATION_BASELINE is immutable. Exact original Working is restored after
each future envelope and before source work resumes.

The exact two push branches are allowlisted. Byte-verified backup:
`Push-KingmakerGunslinger.ps1.sprint34-backup-20261029T185007Z` in codex-policy.
Original SHA256 `bcf9021162d39ab78a0aeb83c7d2dfc8ef40f5161257d17898a851d65e40ff04`;
updated `e2f9feb4a9f7881b61f39472ca9862525431bfe2c8f493abddc56e2d08ccd230`.
Existing focused acceptance/refusal suite38 PASS; no broader branch permission.
Verified archival moved711,009,683 bytes of redundant owned scratch to
`E:/KingmakerGunslingerLab/sprint34-private/completed-scratch-20261029T215827Z`.
Map SHA256 `82b91566f3bd71b3549592d88ba111e4d4f6bb0dc68ac31b292c4df8c2406933`,
2445 files. Primary packages/receipts, protected saves/references and current
candidate output remain. Recalculate full staging/build/restoration reserve
before the next native transaction.
A second verified archive retained785,355,409 bytes/2600 files at
`E:/KingmakerGunslingerLab/sprint34-private/completed-scratch-20261029T225448Z`,
map SHA256 `9a03e9c03b1aa3f585b490f74508df2ede4b4b0c22ff0bc42d7d64ccd94208a6`.
Only exact redundant staging/build copies and candidate-only backup copies were
removed from C: after all hashes matched; primary archives and evidence remain.
A third verified archive retained530,684,972 bytes/1856 files at
`E:/KingmakerGunslingerLab/sprint34-private/completed-scratch-20261029T232201Z`,
map SHA256 `a321bca267387d66c95cb7be45213c81b894d177881eec0263d730e6763f0a88`.
It used the same exact-target, hash-map and byte-comparison procedure. C: free
1,579,327,488 bytes afterward; full reserve1,230,513,542 bytes. Recheck after build.
Archive04 retained242,380,892 bytes/833 files at
`E:/KingmakerGunslingerLab/sprint34-private/completed-scratch-20261029T233030Z`,
mapSHA256 `875e6b5ae9114fddbb413016e4176bc37a3dfd0549348681c4508bbd035ce580`.
Archive05 retained1,218,885,654 bytes/4650 files at
`E:/KingmakerGunslingerLab/sprint34-private/completed-scratch-20261029T235218Z`,
mapSHA256 `c783b6fc4d19bcabf402bb3ffc93f7f747cb4f51d82b8b3dd795b7c0ff24e8da`.
Automatic review initially refused deletion of transaction extractions without
redundancy proof; nothing ran. A read-only comparison proved all3038 extracted
files match the seven retained primaryZIPs exactly (proofSHA256
`2dbad717b3b34fd51e76f4e76aaa95f7eaf8346faa0ed7172f398f81545c3f9e`).
The reviewed retry rechecked those hashes, restored transactions and exact paths,
then archived only redundant extraction/build copies and candidate-only backups.
A PowerShell5 nested-array guard stopped before mutation; corrected enumeration
retained every byte/path guard. PrimaryZIPs, receipts, raw evidence, protected
saves/references and current focused compilation remain. C:free1,962,455,040
bytes afterward; recheck the full1,230,513,542-byte reserve after the next build.
Remote recheck2026-10-29: master still4b7e3d91, latest stablev171 andv172 free.
Recheck again immediately before freezing/publication.
No release or completion claim.
