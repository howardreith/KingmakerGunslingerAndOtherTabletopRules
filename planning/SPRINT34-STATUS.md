# Sprint 34 - hidden native qualification

Current stage: grouped AI03 passed all44 checks, including RTWP/TB, disarm,
normal action costs and clean resets. Stock03 passed early healing and chain
controls but its effects group timed out on a separate patient's spell dispatch;
raw damage also exposed ordinary bleed stacking. The reviewed repair preserves
both facts while yielding to stronger native bleed, and reselects the remaining
CotW wound after that native bleed ends. The unrelated-patient control now uses
a released non-SR Hill Giant and records spell-resistance/dispatch outcomes.
These changed consumers still require fresh stock and CotW proof.
All ten routes remain hidden. No final package or Working envelope has started.
Product/build/promotion root `C:/Dev/KingmakerGunslingerLab/wt/s34`, branch
`codex/expanded-summoning-sprint34-bearded-devil-kyton`. Reviewed base
`4b7e3d914883ac695a4146c03de790b2024ea38c`; last remote check still that master,
v171 latest and v172 free. Recheck upstream/version before final freeze.
Sprint33 is CLOSED. Preserve its original-source ON REUSED_PASS distinction
and historical NOT_RUN records; no historical campaign is selected.

Latest executed development candidate: `a98f8d2eeb13cf9cbf6812580aab540f1f83fdd1`,
purpose `sprint34-infernal-mechanics`, developmentv171 (never a release).
ZIP `8ee515f59d8db5801ab7ebc9453a237f710a339b5483ea71407f6b27ac2568d2`;
DLL `6a93fd973bc3a5d39866b1cbb9d08f3353f097c7c27809cce5f6d7101ec43a75`;
MVID `dd493f93-30ef-4072-9d29-30e5e5fa0cbe`;
fingerprint `dde996fd60102713fbd3b790b53ce21136123400cde2ebf8b6eb5a62faacd398`.
Build07 PASS270.257s,11 focused tests and strict package validation.
Draft product PR [84](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/84).

- AI03 PASS44/44,423.268s orchestration/234020ms game, RunId
  `20261030T0346114425097Z-4161244b97d54bafabc4c4ef9878eabb`.
  Both creatures completed native RTWP/TB actions with separate normal costs;
  native disarm succeeded naturally and the Barbazu switched to claws. No loot
  transfer/drop, orphan effects or contaminated reset. TB used native turns.
- Stock03 aggregate FAIL343.813s/165565ms, RunId
  `20261030T0337192833283Z-9ec3cbaf49564e1288a52f8548c65873`.
  Bootstrap, chain control and actual CL7 healing passed: d20=8 rejected0HP,
  d20=11 healed6HP and closed the wound. Two owners caused one2HP tick and
  retired independently. Group reset passed. Unrelated Barbazu healing command
  succeeded without a recorded touch/heal; SR could deny it, but the old trace
  does not establish that cause. Raw coexistence showed ordinary2d6 plus wound2
  ticks, a production gap addressed by the pending repair, not accepted behavior.
- Stock02 preflight FAIL18.556s before staging/launch: an archived package-validator
  executable was missing. Exact verified executable/dependencies restored from
  archive21; unchanged candidate then admitted. No gameplay verdict.

Prior source `5d3c2da02bb3360efdc77b61039ebbf998e9418e`, Build06 PASS264.730s,
retains its immutable receipt and following failures:
- AI02 FAIL369.729s orchestration/180974ms game, RunId
  `20261030T0305283772641Z-088a4a15b5a74cbbb7c92a897383784d`.
  Both RTWP cells passed real AI commands and separate native costs. Kyton's
  reset retained combat memory; source and captured effects were already absent.
  Installed LeaveCombat does not clear shared UnitGroupMemory; TickUnit can
  immediately re-enroll it. TB/disarm remained NOT_RUN.
- Stock01 FAIL241.310s/66509ms, RunId
  `20261030T0313586585292Z-a0603aac7eaa4c4696f683a082be7abc`.
  Bootstrap rolled back at the one-action bleed donor assumption; no native
  scenario assertions executed. Original UMM log retained with SHA256
  `accf25e82c4f44b2ba86f6bd2ce81b7c73d9495274630512b3fc3c02ea35ca96`.
  No claim that stock healing has passed. The repair clones only one validated
  unconditional native HP payload, leaving printed wound timing/cures owned.

Previous effects/lifetime/AI01 candidate `14a87b917c12d7e4101e5424ccf804b6ce12dd47`:
ZIP `b7684ebf7a2260bc376ef21119cfe75697532e838b079b7be158f5185e5b6e3d`;
DLL `b0620e1ea6d27d720f83fcf5935342514a4eabcc132081f55529ea3d711200c0`;
MVID `ebac7be1-644b-445f-8d01-75c533e7f4bf`;
fingerprint `29a0b6346067baabe18c6f608b10a16a9e858dd189dbb5117469d0d914f557e3`.
Build05 PASS264.542s.

- Effects01 PASS8/8,344.342s orchestration/160655ms game, RunId
  `20261030T0239058321483Z-95ca47b12d7041918edeca739b89d3fd`.
  Real unrelated-patient cure healed13HP with one slot; source expiry removed
  exact wound/disease while another source's native2d6 bleed remained and dealt5HP.
- Lifetime01 PASS25/25,351.298s/162581ms, RunId
  `20261030T0246584972238Z-540cf6e3b49f4e06b3fd41bea4f9d527`.
  Queued cancellation, expiry/death, sibling effects/items/resources, summoner
  loss and actual area subscribers passed. Area probe is not a UI scene transition.
- AI01 FAIL401.664s/217339ms, RunId
  `20261030T0229090148714Z-15a9cbd6d9b2474ca617af13b8efc8d6`.
  Actual paid switch and21 claw attacks occurred; completion rows were empty
  because the reused observer requires explicit `ObserveRtwpAttackCompletion`.
  Correction is observer-only; no retrospective aggregate PASS. Later cells NOT_RUN.

Those three transactions are Restored/verified, with full intake Compare PASS
and no Working mutation. Their evidence prefixes are respectively
`20261030T0239057985341Z`, `20261030T0246584600251Z`, `20261030T0229089826890Z`.

Previous mechanics/combat source `22005fd292c363bf90f60e0604c3e7aefab8c4d9`:
ZIP `d2d3d9b808ff220a5fdf8fbf356fe3fc49012c4c8aa473ee180481b191c2c0c3`;
DLL `7ff07ad93654a636c1bc2f511a62a6fdd9e3e5ed485b07493bc592e4440e0555`;
MVID `7c471153-228f-446c-aecd-5a6e6dee35a9`;
fingerprint `ec696304f0e9c25fdedb3754b5060270c944cb59df914c9401ddb251e0805edd`.
Build04 PASS266.923s.

- Mechanics04 PASS33/33,399.311s orchestration/216797ms game, RunId
  `20261030T0140496707227Z-d5a38731b04b4cc8910d4ac1816357b4`.
  Positive Holy Smite37HP injury, native standard cost, suppression1->2->0
  without early healing, then native2HP healing. Fatal ordinary196HP injury
  retained life; subsequent positive good16HP damage permitted native death.
  Profiles/typed defenses/gaze/native donor isolation also passed.
- Combat01 PASS34/34,402.076s orchestration/220727ms game, RunId
  `20261030T0149426827410Z-40cc49c275d84e0994c0eaaca13a21be`.
  Both live signed weapon calculations; Barbazu Power Attack; actual single,
  charge and normal/close/extended AoO commands. Both claws caused separate5HP
  Beard and failed DC17 disease save; onset remained about4days, no early stat
  damage, and real Remove Disease cured it. Kyton normal/controlled attacks passed.

Raw evidence prefixes `20261030T0140496480019Z` and `20261030T0149426514961Z`
under runtime-evidence, suffix `disposable-expanded-summoning-sprint32-native-review`.
Transactions `compat-20261030T013821Z-c6f8b98650ab` and
`compat-20261030T014714Z-c16da1a85ce5` both Restored/verified; full intake Compare
PASS after each. Native receipts retain their exact original identities.
Development observations are never relabeled as final-artifact execution.

Functional presentation: body05 on source
`6c20b2f7e7f0ce47ba9b33b2f68c9361d19a5d88`, RunId
`20261030T0014352465588Z-d0ed1ae39feb4339919786bd7dc387a6`, PASS32/32,
422.183s/235338ms. Actual glaive blade contact, normal/extended chain contact and
follow-through, distinct moving original bodies, independent private resources
and teardown observed. Agent inspected retained images; no personal owner
aesthetic approval. Rough seams/simple motion/minor clipping remain cosmetics.
ZIP `509682bb036c7d6e5a83480872a4fd4e9e3994c43f4dc0e46cf3324992c39fd4`,
DLL `ac5f8762df5bfb2dd0a75daf511d26772e649cd3475974d766fbb04de6391045`,
MVID `edda15b0-ce63-48b5-bf87-115c2186febc`.

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

## Delivered source and explicit decisions

- Exactly `bearded-devil` and `kyton-chain-devil`, SM V, no SNA/templates.
  Ten hidden V-IX routes. Derived126 creatures, SM104/582, SNA105/588,
  1170 generated placements plus29 wrappers when public. The7511 released
  identity bindings are unchanged;74 appended identities only.
- Private alternative glaive/claws, conditional Beard, day-scale Devil Chills,
  exact-source wound/healing and summon-safe effect ownership. Kyton retains
  its own defenses, four private chains, typed armor and native regeneration.
- Owner-approved standard-action control of its four existing owned chains,
  no activation attack, later normal commands/costs, weapon-only10->25ft reach.
  Targeted standard gaze30ft, live Cha DC15, shaken1d3 rounds, eligible allies
  for players/enemies for AI. No environmental/player inventory simulation.
- Owner: "Don't worry about implementing the heal skill." No replacement skill
  action. Printed Heal DC17 is unavailable; real spell CL16 check remains.
  Full print/engine/adaptation contract: [SPRINT34-RULES.md](SPRINT34-RULES.md).
- Two original paintings/exported icons and distinct original rigged bodies.
  140 icon exports verified,138 old exports unchanged. Asset provenance lives
  with original-icons and original-models Sprint34 sources; private captures
  and raw machine evidence stay outside Git.
- New Kyton-only regeneration subclass addresses installed3-second recovery
  and missing good-spell descriptor handling. Native healing/death flag remain
  native; following-round suppression is serialized. Exact compiled interface
  maps verified. Development mechanics04 now proves following-round, good-spell and fatal-injury behavior; cold-load/final qualification remains.
- Mixed persistence observer captures both sources, exact private items, wound,
  day-scale disease, finite gaze, owned chains and in-progress regeneration
  suppression. No Working envelope started. Final candidate only: one ON trio,
  one OFF trio; restore original Working between envelopes and before source work.

## Controlling acceptance

| Group | Observed evidence | Remaining |
|---|---|---|
| Profiles/defenses/isolation | Mechanics04 PASS, including donor isolation | Final dependency-qualified coverage |
| Barbazu modes/Beard/wound/disease | Early03 real healing/tick/source retirement; body05 routines; combat01 signed stats/Power Attack, single/charge/AoO, actual Beard/infection/day onset/native cure PASS | Fresh stock/CotW stronger-bleed coexistence/resumption and unrelated-patient controls; lifetime01 passed |
| Kyton chains/gaze/armor/regeneration | Early03 control/four chains; mechanics04 gaze/defenses/following-round/good-spell/fatal injury; combat01 single/charge/AoO/live stats PASS | Final dependency-qualified coverage |
| AI/actions/cleanup | Lifetime01 PASS; AI03 full RTWP/TB/disarm/reset PASS44/44 | Dependency-qualified final coverage; changed bleed/healing effects controls |
| Models/icons/ten public routes | Body05 functional presentation, real icons/offline exports | Fresh final presentation and all ten actual public casts/slots/quantity/duration/cleanup |
| Mixed persistence/integration | Scoped mixed observer implemented | Six fresh processes total, targeted highest-risk combined |
| Package/publication/restoration | Exact publisher selector,42 acceptance/refusal checks PASS | Final Sprint gate, reproducibility, PR/merge/release/public download/records |

Focused33 PASS36.293s: production compilation and11/2767 selected tests.
Focused32 FAIL22.077s on damage namespace and nonnullable RollEntry access;
both fixed together before focused33. Next executable action: commit/push the
reviewed bleed coexistence and diagnostic repair, one mechanics-purpose build,
then stock grouped healing and CotW effects on that package. AI03 is retained
original-artifact evidence; no repeated AI campaign is selected for this patch.
Allocate about4.5min build and6-8min per native phase from retained timings.
Restore/compare after each; diagnose failed boundaries before continuing.

Final selection remains one coherent Sprint source/build/package gate (no
redundant Tranche), grouped final behavior/presentation/routes, the two mixed
persistence envelopes and one targeted combined session. Reproducibility is
release-only. Integrate legitimate upstream once and settle patch availability
before final freeze. Do not rebuild to stamp merge/reporting commits.

## Evidence history and development checks

| Date/source/run | Immutable result and interpretation |
|---|---|
| Oct29 early01/8505d4ac/`20261029T2109139961185Z-69ea789acecd4becb12b9300bb1fef21` | FAIL450.8s; observer omitted held-touch rolls. No retrospective miss/PASS claim. |
| Oct29 early02/52c66717/`20261029T2136335444911Z-af0f98356f614c5c966434ebe1161963` | FAIL367.9s; global removed-buff broadcast retired sibling wounds. Exact-owner fix followed. |
| Oct29 early03/8b59b457/`20261029T2151470387493Z-6edde9a480e54454a41135b162c2954a` | PASS20/20,371.9s; CL7 rolls19/17 healed12/13 and closed wound; roll7 rejected0HP; two owners one2HP tick and independent retirement. Standard chain activation no attack, later four native attacks. |
| Oct29 body01/10244694/`20261029T2218589057622Z-cc09ace85a7b4102bb588f5d700a1a15` | FAIL336.2s; native ground/movement settling missing. |
| Oct29 body02/30a81e4e/`20261029T2240404051528Z-1e8e7d65260c4264b281c434cdd29dd2` | FAIL349.9s; inactive MaybeWeapon exposed empty-hand fallback. Repaired to MaybeItem. Null-string clip sub-PASS rejected; raw retained. |
| Oct29 body03/124c370b/`20261029T2310492875407Z-bc0e5dff17be464a8368ee3d7b68abfe` | Raw PASS29/29,403.9s; actual separate7HP Beard. Visual contact remained open. Prior43-check commentary was a transcription error. |
| Oct29 body04/3f5fffa9/`20261029T2334112439495Z-50b8760a061240419bfd28f5c4bd1ca1` | Raw PASS29/29,421.8s; BakeMesh contact correct, subsequent frame reset. Fixed exact0.35s limb follow-through and measured glaive axis; body05 proves corrections. |
| Oct30 mechanics01/6c20b2f7/`20261030T0006062556247Z-80d1d82c96c346b6913ed7d2cc073c44` | PASS27/27,378.960s; initial profiles/gaze/regen observations. Following-round/good-spell/fatal scope was absent and later proved in mechanics04. |
| Oct30 mechanics02/44d965a9/`20261030T0056506795918Z-e4b597256e8c4965a827c07c7353929d` | FAIL403.453s; support caster readiness incorrectly required the S34 body renderer. Fixed native support readiness. |
| Oct30 mechanics03/e4b7249f/`20261030T0116405420915Z-c260bee0619f4fd786431fe43325b15e` | FAIL380.026s before any Holy Smite cast; guard omitted separately tracked private hostile. Exact identity/intake exclusion repair then passed mechanics04. |

Early03 exact identities: ZIP
`b86aef9d5b3984fc34fc98139f19af6d793d882cc6af14c90290f72014c863b2`,
DLL `0aae673463b9dcf10bc27464a2983d432816618232c4ef655d99e1a31ece094f`,
MVID `19c76e2e-4398-4a60-a5fd-cdd910109d24`, fingerprint
`2602f469f4fb378cdd8beb1fbc968892007f04c634c09b714e90d40ac46f6ce3`.
Retain raw loaded identities/receipts for every other historical candidate;
reuse requires exact dependency/source/observer/profile linkage.


Gate30 PASS35.577s, production compilation and11/2767 focused tests.
Gate29 PASS31.444s after exact private-group disarm guard review;11/2767 tests.
Gate28 PASS34.707s, production compilation and11/2767 focused tests.
Gate27 FAIL20.359s on two observer lambda-name collisions; corrected together.
Gate26 PASS31.708s, gate25 PASS31.248s and gate24 PASS34.912s (11/2767 tests).
Earlier observer API/collection errors were corrected before their subsequent
focused passes. Publisher42 S34 plus18 affected S33 refusal checks passed;
33 persistence admission/refusal checks and two wrapper refusals passed.
No final milestone gate or final qualification has run.

## Intake and restoration

HOWARD-LAPTOP; actual human installation0.0.117 observed, not a restoration pin.
Sixteen private references hash-match installed Kingmaker2.1.7b/Harmony12/UMM.
IntakeSHA256 `bfbef80b0beee9afdadc527211780b615f49f9ea87be7183405d220b8d1ca50d`
covers967 mod entries,118 save entries,20 references,83 settings, sound bank,
processes/tasks/leases. Latest transaction `compat-20261030T034336Z-605ca22dea80` reached Restored
with verified restoration; full intake Compare PASS after AI03. No game/lease/Working envelope active.
KMG_AUTOMATION_BASELINE is immutable. Original Working remains unchanged.

Only the two authorized push branches were added to the existing wrapper.
Byte-verified backup `Push-KingmakerGunslinger.ps1.sprint34-backup-20261029T185007Z`;
originalSHA256 `bcf9021162d39ab78a0aeb83c7d2dfc8ef40f5161257d17898a851d65e40ff04`,
updated `e2f9feb4a9f7881b61f39472ca9862525431bfe2c8f493abddc56e2d08ccd230`.
Existing focused acceptance/refusal suite38 PASS; all pushes use that wrapper.

Verified redundant-scratch archives1-6 live under
`E:/KingmakerGunslingerLab/sprint34-private/completed-scratch-*`, with exact
hash maps/results retained locally under `artifacts/sprint34-intake/`.
Archive06 retained609,895,179 bytes/2325 files at timestamp20261030T003722Z;
mapSHA256 `ccf741478852bd7ae454b2108788f7c3ec80dc01c9cd882a4f2bbaf8794efa86`.
Both transaction extractions first matched all868 files in the retained primary
ZIP (proofSHA256 `519ed47f955705538e8e8f7d3a60aa68ebe6511ad749f10e984fbb772105c22e`).
Only redundant exact build/extraction/candidate-backup copies were removed fromC:
after E: byte verification. Original saves, primary packages, evidence and
references remain. Recheck full1,230,513,542-byte staging/restoration reserve.
Archive05's initial automatic-review refusal caused no mutation; explicit
primary-ZIP redundancy proof resolved it before the reviewed retry. No permission
boundary is currently blocked. No release/completion claim.

Archive07 retained531,188,268 bytes/1856 files at timestamp20261030T010455Z;
mapSHA256 `a9a592e45c1961233c1f385b9d6513f7b94d460b5d36b452b4738d1e4cb86b10`.
Its434-file transaction extraction exactly matched the retained44d965a9 primary
ZIP (proofSHA256 `07b3a4f52316e62629b9cb5bdb9b01fe5d6a86a0a892d4ab589be84fcf1b099a`).
Only redundant compilation/staging/extraction/candidate-backup copies removed
fromC: after full E: byte verification; primary packages and raw evidence remain.

Archive08 retained531,183,148 bytes/1856 files at timestamp20261030T012459Z;
mapSHA256 `8311e65ddf0204ca73d187d0e86a1bca813de75f50126f70dfba1d1a19f7939c`.
Its434-file extraction matched the retained e4b7249f primary ZIP
(proofSHA256 `6ac20f250da798c1a1ab739a0448aa2a753333ace5068501ae6037031a1d6955`).
Only verified redundant owned scratch was removed after E: archival.

Archive09 (20261030T013740Z) retained135,388,659 bytes/399 redundant compilation
files; map `bee16c9c75dd282265581b1b98eaa2871cf0501c3fb23ba69e7815000f76f9be`,
proof `dd845ec9e8c0e4c269782b3be69c16180debd4dedfb57159ceaa1e33b3e089bc`.
Archive10 (20261030T014554Z) retained321,640,251 bytes/1302 files;
map `412698d956f43dc11cebe63b71205b4c14ab590e8d8e825e302e7842121c7fc5`,
primary-ZIP proof `ba655c6e35e512648852d035ff2f68e369874db63586901d61246538ec6feed6`.
Archive11 (20261030T015650Z) retained321,640,251 bytes/1302 files;
map `4a71091d5ba65b5641ad1572af5493ca02b3250d2e567d4cbd79d27c49a7c250`,
proof `edf2a8671e3fa948f8cf3340269d73182d464d790594289d1301348b74ada6c6`.
All are byte-verified E: archives of redundant owned scratch; primary packages,
original saves, references and raw evidence remain retained. No active game or
lease. No release/completion claim.

Archive12 (20261030T021420Z) retained74,174,718 bytes/155 redundant old build
files; map `640b83c2209800e32afaaf49cfe4ddb89882ab4d0d1aa2ea8d4951961203cafd`,
proof `b450d5c3a84fc0cfa264df8d73821f1d3f6207e7c44349f614fba2db80822cbe`.
Every removed file also remains in the primary ZIP or archive09.

Archive13 retained552 duplicate build files/161563889bytes on E:, map
`01b08f6a0ddecbf53a549738251f543061e92aa3f98ee308e4165b4c73d4b281`.
Archive14/15 each retained1302 files/321789243bytes from restored AI/effects
transactions; maps `f04a39081b934176af641008bfd1b85f353473385e9c0eb5bb50f27b5001e7c4`
and `2821892a1975a1d3689c4e5395aeeb3c152f698f43da5a44d657403b12c3468d`.
Archive16 retained1736 files/429052324bytes, including ZIP-matching staging and
restored lifetime copies; map `cc83cc74edb3d2469c05b4a64c186b45122d01478d4aeab4df9bb18775c4e8e9`.
Primary ZIPs, receipts and raw evidence remain retained; no protected saves touched.

Archives17-20 retain only byte-proven redundant build/transaction copies on E:;
local proof/map/result files remain under artifacts/sprint34-intake. Maps:
17 `22bf3605c81cf2aacb5750e4d8017e7186f9bbc26e51330973ccc7bf34695cfe`;
18 `2e15a9c0deb28ef56b1a5e1eca5617cd0d0d29f2e8fa5423813c6950b50386e1`;
19 `978f4e09acee86f82485868d5e550ce0d68c565b9a6d3a77e2dec86b9a715f35`;
20 `fe1169900ea951a87ce049e462f57993cb92e6b6a9bc870670079909651e0cb1`.
Archive20 retained1302 files/321801531bytes after exact434-file primary-ZIP proof
`a4ca9933f743f0c6a9694cf78bac59f169f69506899d6824dd5455e8ea8ad2e3`.

Archives21-23 retain verified redundant build/staging/restored transaction
copies on E:; original primary ZIPs, raw receipts and protected saves remain.
Maps21 `53a6c89e2194763e8f983f0e3264446e7fda08c00c0838ca726b53d7aa3781d3`,
22 `8d4f18f2b5e05193d750af6eeebe4b34b936202ae40f3e028b429dd8e132797f`,
23 `fa4c64b0ae948c70cefe460dfec7f1aff889805da5fdb40baeda7ae281ebd943`.
The validator executable/dependencies removed in21 were restored byte-for-byte;
only its unneeded PDB remains archived. Never omit that runtime prerequisite.

Archive24 preserved nine older, superseded development primary ZIPs and copies
of their unchanged receipts on E: before removing redundant C: copies. Exact
map `eb5a51d5bf0968d1ad0f5ae1f57061b6cc34fd7d5cccaa660891841ddf1b0d06`,
604374659 bytes; preservation proof
`b0f3a7a1b0a62d44ff79ea65b75dacb13816a8384c7377c05ae34f46bb956cf7`.
Selected reuse packages and the active candidate remain on C:; no unique data
was lost and original receipts were not rewritten.
