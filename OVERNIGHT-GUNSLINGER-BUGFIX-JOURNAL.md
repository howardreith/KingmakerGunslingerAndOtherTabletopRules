# Overnight Gunslinger Bug-Fix Journal

## 2026-08-20 - Issue 7 Border Sentinel later placement

- Pre-change save-free graph run `20260820T0603196319367Z-66ffefa0c02344fd82160591d0a4aea8` passed 23/23 assertions on `e09fbd037b0a0fc41181dd40ff0f77bb972061d5`. It confirmed the stale Oleg row and enumerated 437 bounded fixed-loot candidates.
- Border Sentinel remains stable identity `KMG.EasternWeapons.Nodachi.BorderSentinel` / `c1c7a6746916504ebfdcb2b650a7145b`, +1 cold iron, price 4,420 gp. Exact selected target is base-campaign fixed `BlueprintLoot` `PoorHuman_treasure_chest_03` / `c8b8159fb695be64883b609a7e77e75d` in `StagLordFort`, with zero registered direct references and native Rusty Horseshoe x1 plus Gold x12 contents.
- Commit `3e52dfdac3d86eddabc2e8fa94024b96ced0241b` removes Border Sentinel only from Oleg's project-owned desired set, appends it once to the selected chest, updates five-target/twelve-row cardinalities, and adds a read-only development location audit. Existing identity, mechanics, price, contents, order, module gating, idempotence, and exact-snapshot rollback remain intact.
- Focused campaign/placement/development tests PASS. Repository validation, complete 1,157-test suite, clean exact-reference Release build, output validation, SoundBank validation, standalone package, strict package, and diff checks PASS.
- Immutable guarded run `20260820T0613577259438Z-865e96f7deb44004bda7cef62f0511bb` passed 24/24: `olegRows=0;vendorRows=0;lootRows=1;target=PoorHuman_treasure_chest_03:c8b8159fb695be64883b609a7e77e75d:StagLordFort`. Eastern totals were vendor 96/8 including 48 BTSL, fixed loot 12/5, with zero invalid/unexpected rows.
- Runtime package SHA-256 `F221621C474C4778D92A18D2F126EAED3ACBB675F8B97251CD6FA59E7088EBA8`; loaded/runtime DLL SHA-256 `316CF0CAE762ADDC12338B917D283F554C7B93F95BDD369202A578C6DAC76DA0`; unchanged validated SoundBank SHA-256 `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- Static blueprint publication does not delete any owned/sold/dropped/stashed copy, refresh an already-materialized Oleg inventory, or refill an opened chest. Physical unopened/new-campaign chest materialization remains human-gated.
- Next action: Issue 8, audit current Wwise bank staging/loading/event posting/emitter/package paths against the fresh missing-sound report.

## 2026-08-19 mission intake and baseline

- Required branch: codex/gunslinger-overnight-bugfixes.
- Fetched origin/master: d13268d3abe9ffe89c8195b213c1eee194328672.
- Baseline subject: Seal Urban Barbarian 0.0.87 qualification.
- Baseline version: 0.0.87; informational version
  0.0.87-urban-barbarian-human-review-repair-4.
- Primary worktree was preserved because tmp-phase0-clone/ is unrelated and
  untracked. Isolated worktree:
  .worktrees/gunslinger-overnight-bugfixes.
- No pre-existing local/remote overnight branch and no Git lock were found.
- Qualified ancestors 4f28dcf (runtime foundation), 71368cb (accepted rare
  firearms), and 9e71075 (Elven Branched Spear qualification) are present.
- First Build-Local attempt: repository validation PASS, domain compile FAIL
  because isolated worktree lacked ignored GamePath.props and therefore had an
  empty KingmakerManagedDir/Newtonsoft reference. This was an environment
  materialization failure, not a source failure.
- Safe repair: created ignored/uncommitted GamePath.props using the established
  qualified private reference-bundle paths; no network or dependency install.
- Second .\scripts\Build-Local.ps1: PASS. Repository validation PASS; complete
  1,150/1,150 domain/reflection tests PASS; clean exact-reference Release PASS;
  focused supply icons PASS; build-output PASS; SoundBank PASS; deterministic
  standalone and local-runtime packages PASS strict validation.
- Starting local-runtime package:
  artifacts/local-runtime/0.0.87/KingmakerGunslinger-0.0.87-local-runtime.zip
- Package SHA-256:
  7572D7664C02BD07A1DD713971CFD9E9A46C02E32011E0F7A410F25F5D597CED
- DLL SHA-256:
  1E15AE66DEB0132D80F3774DA0A243AAE13388B91874898695926A966736F78F
- Firearm AssetBundle:
  CC9DA6B2FB43FD2932971E3CCE015610497E4C2DB657F62DBA675A31DE327B20
- Elven Branched Spear AssetBundle:
  6E9FE86E43072361EEC3357D9C73E17ADD71D22BAF257FB8C7ED6F52931CE777
- KMG_Firearms.bnk:
  0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18
- No deployment or runtime launch occurred during baseline qualification.
- Current issue: durable mission checkpoint.
- Next action: validate, commit, publish, and remote-verify these records, then
  begin Issue 1 source/callback investigation.

## 2026-08-19 Issue 1 source-qualified candidate

- Branch/source parent: `codex/gunslinger-overnight-bugfixes` at
  `879ffe152a4ccfbfe42679055f7c392e5d0f1669`; version remains 0.0.87.
- Diagnosis: Acadamae armed an eligible `UnitUseAbility`, but its completion
  lookup depended on the thread-local `OnAction` stack. A native
  `RuleCastSpell.OnTrigger` outside that stack could not consume the armed
  invocation, producing no save or fatigue. Terminal Harmony targets also did
  not freeze their exact overloads, and the old evidence exposed only DC/pass.
- Repair candidate: bind each concrete `RuleCastSpell` constructed during the
  eligible command to the same tracker entry; consume that exact rule once on
  successful completion; consume failed/canceled commands without a save; use
  explicit `RulebookEventContext` and `OnEnded(bool)` patch signatures.
- The native Fortitude path remains `RuleSavingThrow` at DC 15 + spell level.
  Failed saves still add the canonical Fatigued blueprint with null spell
  context and call native `MakePermanent()`; successful saves add nothing.
- One completed accelerated cast now records and publishes caster, spell,
  natural d20, real Fortitude modifier/total, DC, success/failure, and final
  fatigue disposition. Notification failure is exception-contained and cannot
  change spell or fatigue mechanics.
- The guarded scenario now drives real `UnitUseAbility` commands for a forced
  natural-20 success and forced natural-1 failure, in addition to OFF,
  ineligible, duplicate, cancellation, context lifetime, Cord, and rest paths.
- First `test-domain.ps1` compiled and ran all 1,150 tests; one new
  source-contract count expected two failures instead of the strengthened
  scenario's three. The assertion was corrected without production change.
- Second `test-domain.ps1 -Configuration Release`: 1,150/1,150 PASS.
- `Build-Local.ps1`: repository validation PASS; 1,150/1,150 PASS; clean
  exact-reference Release PASS; focused icon PASS; build-output PASS;
  SoundBank PASS; deterministic package and strict package validation PASS.
- Candidate local-runtime package SHA-256:
  `538FE2A912B07990B12DC20C89D379ED8C3878C36FF41A5F46A5A7A3D8556B7B`.
- Candidate DLL SHA-256:
  `E2CCBCE48C95C2183D37442F3D4E334F6EEF151192C6219CA15EFBE19ADB5116`.
- Runtime has not yet run. Next action: diff/staging safety audit, commit and
  approved-helper publish, exact remote verification, then guarded
  `disposable-acadamae-graduate` qualification on the immutable SHA.

## 2026-08-20 Issue 1 first runtime failure and fixture repair

- Dedicated repair commit `d691d508c43f3c048f28860389a6146186c11448`
  was published; HEAD/local/origin equality was exact before launch.
- Guarded Steam run ID
  `20260820T0358157167579Z-246801cb00f44e4a80f6e69e4dffa28c`
  loaded exact source `d691d508...`, version 0.0.87, then returned `ERROR`.
- Exact failure: `ExecuteAcadamaeNativeCommand` required a detached
  `ChargenUnit` command to reach `Success` after one animation tick, but its
  result remained `None`. No scenario assertion ran, so this is not mechanical
  PASS evidence and is not recorded as an Acadamae production failure.
- Repository precedent establishes that the one-tick native command driver
  requires a caster in an actually loaded area. This scenario is intentionally
  save-free and its detached animation/controller clock cannot advance that
  boundary.
- Materially different safe strategy: invoke the exact protected
  `UnitUseAbility.OnAction()` method through reflection. This retains the real
  Harmony prefix/postfix, native `RuleCastSpell` constructor, `OnTrigger`, and
  ability delivery while avoiding a fabricated animation clock and avoiding
  manual tracker/RuleCast calls.
- Corrected fixture gates: `test-domain.ps1 -Configuration Release` PASS
  1,150/1,150; `Build-Local.ps1` PASS repository, all tests, clean exact-
  reference Release, output, SoundBank, package, and strict package validation.
- New package/DLL SHA-256:
  `04E72E53DF1D8F58CEB9C60FDFD23C164E2EDE25991F02458A44F542843D1B9D` /
  `785DCB24EA3E6B4A91AA9867F9DAAAF374872C71A9857293F0573F1D277173C0`.
- Next action: commit/publish this issue-scoped runtime fixture correction,
  verify remote equality, and rerun the guarded scenario.

## 2026-08-20 Issue 1 second runtime failure and narrowed boundary

- Published fixture checkpoint:
  `b2b8d6edcfa16d0fdb72ea7b1e8ecb2cfe50406c`; exact remote equality verified.
- Guarded run ID
  `20260820T0404361106448Z-a262364c94774a8d9e444d73f7c32e10`
  loaded that exact commit/version and returned `ERROR` before assertions.
- Exact failure: reflection reached the native protected action, but the
  installed patched `UnitUseAbility.OnAction_Patch3` threw
  `NullReferenceException` for the detached `ChargenUnit`. This independently
  confirms the save-free fixture cannot supply the loaded-area command context
  required by the composed engine/mod action chain.
- Strategy change: do not attempt another detached animation/action call. The
  scenario now reproduces the rejected production ordering directly: begin the
  exact command scope, construct the native `RuleCastSpell` so the production
  Harmony constructor patch attaches it, end command scope, then trigger that
  exact rule. This proves the new delayed identity retention and native save/
  fatigue consequence without claiming loaded-area animation execution.
- Actual animation-driven command execution and area-transition persistence
  remain explicit consolidated human checks; they are not silently passed.
- Narrowed fixture passes 1,150/1,150 and the complete `Build-Local.ps1`
  repository/Release/output/SoundBank/package/strict-package pipeline.
- Candidate package/DLL SHA-256:
  `134184B1A08FC73153780A711CEF1A309DE311BD6D2AFBC2BE36B00DA7F604B3` /
  `3B5AEB2122E2F736340A7F504DE56F50BED9314109769502276DFC764D3A07FD`.
- Next action: publish this issue-scoped evidence-mode checkpoint and run the
  guarded delayed-terminal scenario.

## 2026-08-20 Issue 1 delayed-terminal live evidence and roll repair

- Published evidence-mode commit:
  `0708d9d7683bf8646a23efad06631cdddf2e473b`; remote equality verified.
- Guarded run ID
  `20260820T0410154111888Z-395ce15edb3b49a19d1f343d0604f369`
  completed without exception and returned FAIL with 11/13 assertions PASS.
- Passed live boundaries: prepared Summoning identity, default-OFF mode, OFF
  native full-round/no-save behavior, Standard action parity, subsequent
  native failed-save fatigue, cancellation, command snapshot, permanent
  context-independent fatigue, native rest removal, view lifecycle, Cord
  substitution, and cleanup.
- The delayed exact rules were consumed: completed count advanced to one and
  then two, diagnostics published, and failure applied permanent fatigue. The
  two failures were diagnostic expectations: success recorded d20 `-80`/total
  `20`; failure recorded d20 `101`/total `1`.
- Exact installed `RuleSavingThrow` metadata and getter IL resolved the cause.
  `BaseRollResult` computes implicit `D20` plus `StatValue`; `RollResult`
  returns that base total plus `SuccessBonus` only when required. The forced
  completion postfix wrote the desired natural roll into `BaseRollResult`, so
  its setter rewrote the stored d20 to natural minus Fortitude.
- Repair: the existing `RuleRollD20.PreRollDice` hook remains the sole forced
  natural-roll setter; the completion postfix only retains natural-20
  `AutoPass`. Diagnostics now record native `D20.Value`, exact `StatValue`,
  conditional bonus, and final `RollResult` under truthful labels.
- Gates: 1,150/1,150 PASS and full `Build-Local.ps1` repository/Release/output/
  SoundBank/package/strict-package PASS.
- Candidate package/DLL SHA-256:
  `3A3EEDB3CBB88BF0BFF6AB37A3717B102342F7EEC0A45BA6D543CFCCAECEB06D` /
  `EC897F4F87166A88FB8A196BA2EB206430D2E6D25364658D03682CE1724AF123`.
- Next action: commit/publish the native-roll diagnostic repair and rerun the
  guarded delayed-terminal scenario.

## 2026-08-20 Issue 1 final automated qualification

- Published production diagnostic repair `1963a80eb3f49cffa6324df6d43450ef9e3fb05f`.
  Run `20260820T0417236469478Z-8e69f69546804934a84ea4d184d7a05d`
  passed 11/13 and proved truthful native diagnostics: success d20 16 +
  Fortitude 100 = 116; failure d20 8 + Fortitude -100 = -92. Only the
  deterministic natural-roll expectations remained stale.
- Published seed experiment `928fab624ab4cdd6e59a9b98d40d95783c708713`.
  Run `20260820T0423312051777Z-775c65b925a240ec9590c0ab5d46913c`
  again passed 12/14 mechanical assertions but proved Unity RNG seeding is not
  the native saving-throw source.
- Installed behavior showed this saving throw does not enter the existing
  `RuleRollD20.PreRollDice` test hook. The final guarded-only completion hook
  writes base total as forced natural plus the real native `StatValue`; it does
  not alter unarmed production saves.
- Published final test-control commit
  `f807eb1cc3dabf9dc66acaa2b773c029a72dc942`; remote equality verified.
- Guarded Steam run
  `20260820T0428503321600Z-d97a49371e1949c89f3de25aac1c6eff`
  passed 14/14 with exact natural 20 success, natural 1 failure, one save per
  completed eligible cast, permanent context-independent fatigue, native rest
  removal, OFF/cancel/snapshot controls, Cord substitution, and cleanup.
- Runtime candidate package/DLL SHA-256:
  `B6E6F409C5B78C16EDBD98A35E349DCD9F6412C08659A6D1300712DA838996CF` /
  `788E98066FD614F78C3CE42B5ADC680BED45F4AF5C3B4A54AF956E257A1F6DDF`.
- Issue 1 is human-gated only for a loaded-area animation-driven command, area
  transition persistence, and visual combat-log/Cord presentation. Next:
  diagnose Issue 2 Focused Aim transactionality.

## 2026-08-20 Issue 2 Focused Aim source candidate

- Human evidence supersedes the prior structural claim that fixed-cost Focused
  Aim was complete: the damage marker appeared while the visible Grit pool did
  not decrease.
- The current delivery path manually spent shared Grit before adding the timed
  Focused Aim buff. That ordering allowed fact-activation resource
  reconciliation to restore the pool after the debit while retaining damage.
- The candidate adds the marker first, rechecks the live authoritative pool,
  spends the effective cost exactly once, verifies the observed delta, and
  removes the marker/restores the snapshot if the debit is not exact.
- Ability ownership is now checked through the exact granted ability fact.
  Existing Swift action, duration, firearm-only damage, Dead Shot multiplier,
  shared resource UI component, and True Grit positive-Grit rule remain intact.
- Added transaction-policy coverage for positive/two uses, exactly one Grit,
  zero Grit, selected True Grit, wrong owner, and duplicate armed activation.
- Added guarded `disposable-focused-aim` coverage for live shared-resource
  deltas, real firearm/crossbow weapon-stat calculation, duplicate/repeat,
  zero-Grit, True Grit, feature remove/add reconciliation, and cleanup.
- Gates PASS: repository validation, 1,151/1,151 tests, exact-reference Release,
  output, SoundBank, package, and strict package validation.
- Candidate package/DLL SHA-256:
  `13BAFA9437E887BDDAE512D63C997542605DA4B5A9C60D033DD5D50812D88B40` /
  `206606B2996DDD16D7F2F5FBF3F5FF00B142307F6B722CD85257B43C904133FD`.
- Next action: commit/publish the Issue 2 candidate and run its guarded Steam
  scenario against the immutable source.

## 2026-08-20 - Issue 2 Focused Aim qualified

- Branch: `codex/gunslinger-overnight-bugfixes`
- Qualified code SHA: `24ffb78ac6821d4aec173213df1f046940be683b`
- Version: `0.0.87`
- Test result: 1,151/1,151 PASS.
- Full gates: repository validation, exact-reference clean Release build, output validation, package build, strict package validation, SoundBank validation, and `git diff --check` PASS.
- Rejected experiments: runs `20260820T0441390621400Z-e19b47452eca4d42b948ec6dbc8cb48f`, `20260820T0445553713878Z-33645d179369449a958e03afd42fd4c9`, `20260820T0450089103961Z-190d6b8e8ff141e18b3151772d18d8ac`, and `20260820T0455022913298Z-4407c2f3e3ec4d88ac3bd2d7f387e9fa` all stopped at marker creation. The successive live-fixture changes proved that health and level were not the boundary and that permanent as well as timed `AddBuff` can return null after materializing the fact.
- Root cause: the old transaction restored Grit when `AddBuff` returned null, even when the exact project marker already existed in `Buffs.RawFacts`; damage therefore remained active without the spend.
- Repair: resolve only the exact Focused Aim marker blueprint from `RawFacts`, then commit the policy-derived cost against the live shared resource with exact delta verification and rollback/removal on failure.
- Passing guarded run: `20260820T0458550047640Z-b20727d24cc24d3297e0e9d23d385235`, 7/7 PASS. Observed ordinary uses `2 -> 1 -> 0`, selected True Grit retained `1`, pistol damage delta `+4`, crossbow delta `0`, zero-Grit/wrong-owner/duplicate controls rejected, and request-local cleanup passed.
- Runtime package SHA-256: `A0D1B53AD3086339167BBBD23BB4B58D596014D989E1754BB4A1DAA30B63DEA0`.
- Runtime DLL SHA-256: `08FCE67E6947CA22E57BF987D7AD01F3EB78B6BFE9C422D1C3560EB8855B4857`.
- Runtime firearm AssetBundle SHA-256: `CC9DA6B2FB43FD2932971E3CCE015610497E4C2DB657F62DBA675A31DE327B20`.
- Remaining human check: visible counter refresh plus ordinary save/load and UI activation coverage.
- Next action: Issue 3 firearm Touch AC boundary and presentation audit.

## 2026-08-20 - Issue 3 firearm penetration qualified

- Branch: `codex/gunslinger-overnight-bugfixes`.
- Qualified code SHA: `1c671acc3196a3f416bdcf4177b7426c0e14ea01`.
- Version: `0.0.87`.
- Diagnosis: the early-firearm path already revalidated exact identity and distance at live `RuleCalculateAC`; Advanced Rifle/Revolver remained on the stale Sprint 9 fail-closed branch and normal player feedback was absent.
- Repair: one effective increment for early direct fire, five effective increments for advanced firearms, with Steady Aim/per-attack bonuses applied before the boundary; exact base help text on production and magic firearms; one Touch/Normal native battle-log annotation per resolved exact-firearm AC event.
- Bounded presentation investigation: repository and installed-adapter evidence exposed no qualified target-hover or attack-preview extension seam. The existing native warning event consumed by `BattleLogManager` was reused rather than creating a broad UI framework.
- Tests: `ac.production-boundaries` and `ac.penetration-presentation`; complete suite 1,152/1,152 PASS.
- Full gates: repository validation, clean exact-reference Release, output validation, SoundBank validation, package build, strict package validation, `git diff --check`, and runtime scenario preflight 109/109 PASS. One initial preflight artifact-fingerprint check raced the just-completed build; an unchanged rerun passed 109/109.
- Guarded run: `20260820T0513443721972Z-cceff2c263254181ad15fd7af638ed3f`, 5/5 PASS. Pistol boundaries 6.095/6.096/6.097m; Musket 12.191/12.192/12.193m; direct Blunderbuss 3.047/3.048/3.049m; Advanced Rifle 121.919/121.920/121.921m; Advanced Revolver 30.479/30.480/30.481m. Each selected Touch/Touch/Normal. A +10 ft. Musket case selected Touch at 15.24m and reported a 50 ft. penetration range.
- Runtime feedback: 16 exact annotations; final line `Musket attack: 50 ft.; penetration range 50 ft.; Touch AC.`
- Runtime package SHA-256: `A05A0389C3C5D4ABC19891EA1B0F57D40AD9A865DA8DF0C3D2F6A6C62F81BF27`.
- Runtime DLL SHA-256: `44208BB5D6351877C71A73E7A3979B732BD8654E89EABDDB46B493F882197C04`.
- Firearm AssetBundle SHA-256: `CC9DA6B2FB43FD2932971E3CCE015610497E4C2DB657F62DBA675A31DE327B20`.
- Remaining human check: real UI tooltip/battle-log readability and player-issued attack confirmation; pre-attack hover remains an optional bounded follow-up.
- Next action: Issue 4 duplicated Acadamae prerequisite presentation.

## 2026-08-20 - Issue 4 Acadamae prerequisite presentation qualified

- Branch: `codex/gunslinger-overnight-bugfixes`.
- Qualified code SHA: `78bc46d21b71dbfb35d430d00755228348afb751`.
- Version: `0.0.87`.
- Diagnosis: `KMG.Feat.AcadamaeGraduate.Description` manually repeated the specialist-Wizard and non-forbidden-Conjuration eligibility requirements already rendered by the exact `PrerequisiteAcadamaeGraduate` component.
- Repair: removed only the manual prerequisite sentence. The prerequisite component, eligibility policy, native `GetUIText()` requirements, feature identity, publication, and mode grant remain unchanged.
- Focused test: `acadamae.prerequisite-presentation` PASS. Complete dependency-free suite: 1,153/1,153 PASS.
- Gates: repository validation, clean exact-reference Release, output validation, SoundBank validation, package build, strict package validation, `git diff --check`, and runtime preflight 109/109 PASS. The first unchanged post-build preflight hit the known staging-state transient `unsupported-does-not-build-or-stage-package`; its cleanup retry passed without a source change.
- Guarded run: `20260820T0524095518410Z-ea7adae339fc4fa4a98bfe7bd52b4222`, 15/15 PASS. Live evidence reported `prerequisitePresentation=True`, exactly one native prerequisite component, retained native prerequisite text, and no duplicated description prose.
- Runtime package SHA-256: `60C0B8AFA7F3E7E9EB98710D6300C9B7D3D4F4AB3BE3C761CED8D5FA5FFCCB34`.
- Runtime DLL SHA-256: `0ECE4157AA0512A5887B7FD6BB5B82BCE963758EE796873FE2AC20113E482E14`.
- Remaining human check: inspect the real feat-selection UI at normal scale and confirm one coherent prerequisite block.
- Next action: Issue 5 exact Oleg maintenance-kit publication.

## 2026-08-20 - Issue 5 Oleg maintenance stock qualified

- Published code SHA: `1586c5e7abd9c8d1b18bac483df88e86700677b0`; local branch and origin equality verified before runtime qualification. Version remains `0.0.87`.
- Diagnosis: the exact installed `C11_OlegVendorTable` (`f720440559fc00949900bfa1575196ac`) was already a bounded publication target but had no issue-owned maintenance transaction. Exact direct owners are `OTP_Oleg` (`5db389e0409ef534d81358555e6ab99d`) and `OTP_Oleg_FirstVisit` (`67db4b8bacc69e643880f0a4ed6dff6f`), one reference each.
- Implemented a module-gated transaction that owns only Repair Kit and Overhaul Kit rows, preserves every unrelated component reference and order, normalizes stale/duplicate/wrong-count owned rows, validates Repair Kit x5 and Overhaul Kit x2, and restores the exact snapshot on later bootstrap failure.
- Focused `vendors.oleg-maintenance-stock` and complete 1,154/1,154 domain/reflection tests PASS. Repository validation, exact-reference clean Release, output validation, SoundBank validation, standalone package, strict package validation, runtime preflight 109/109, and diff checks PASS. The first unchanged post-build preflight hit the known package-staging transient; cleanup retry passed without source changes.
- Guarded Steam run `20260820T0535269245782Z-7edf2d2c158a4085893931e91b14db1d`, scenario `observe-vendor-table-contracts`, passed 20/20. Live registered table showed one Repair Kit row at 5, one Overhaul Kit row at 2, and the two exact owners above.
- Runtime package/DLL SHA-256: `00D8405393B74827915C09B67834B9B2D06BC4FE06E5123808AEC64F59868F69` / `8E7FFC30C596F94BA2EF5D06B6C99EFE6A3CB29AEE25E09C29F2624D6C16A003`.
- Static blueprint publication is qualified. Already-materialized save-owned shop refresh was not observed and is not claimed; actual new/unmaterialized merchant UI remains human-gated.
- Next action: Issue 6 exact Bokken table/lifecycle forensics and bounded ammunition publication.
## 2026-08-20 - Issue 6 Bokken ammunition stock qualified

- Published forensic SHA `3b29451f24cc163f48f03150cce0e7563165beaa` added a bounded read-only localized/dialog/unit metadata traversal because the historical observer discarded units without `AddSharedVendor` before examining `AddVendorItems`.
- Guarded forensic run `20260820T0548397631886Z-31199f78288f43e2b7655b0433abba7b` passed 21/21 and resolved exact `BlueprintUnitLoot` `C11_BokkenVendorTable` (`4778ecb5df5d48742b9be5a204ed4657`). Exact direct owners are `OTP_Bokken` (`4f5acdb403f6ef642959f6bedc051ac7`) and `OTP_Bokken_ZeroState` (`57f84fdde3cc2994284fb3acc4a3cb97`), one reference each through `AddVendorItems`; base `Bokken` is only a prototype.
- Published fix SHA `73e776a91167bc02024e7be794a822fa63fec48e` adds a module-gated exact-unit-loot transaction owning only the existing Black Powder, Lead Ball, and Paper Cartridge references. It preserves all unrelated components/order, normalizes stale/duplicate/wrong-count owned rows to 100 each, validates exact cardinality, and restores the exact snapshot on later bootstrap failure.
- Focused forensic/stock tests and complete 1,156/1,156 suite PASS. Repository validation, clean exact-reference Release, output, SoundBank, standalone package, strict package, and diff gates PASS. Generated test-output access required the approved elevated context after the sandbox denied stale artifact replacement; the same gates then passed without source relaxation.
- Guarded qualified run `20260820T0555507894600Z-0772504de3254a64986e6ea2da172a02` passed 23/23. Live table rows were Powder `1/100`, Balls `1/100`, Paper `1/100`; all 21 native potion/scroll/wand/material rows remained present before the three additions.
- Runtime package/DLL SHA-256: `3D5052D73D3D4E1701ED4313ED7DACD072EC179B47BFD3054D93585EDEF2D70B` / `F97D27FE31D4912BBE20D51EC4AACF0B5D793FACF642FDA705CA6C94569317EA`.
- Static `BlueprintUnitLoot` publication is qualified. Already-materialized save-owned shop refresh was not observed and is not claimed; actual new/unmaterialized Bokken trade UI remains human-gated.
- Next action: Issue 7 exact Border Sentinel blueprint/profile/current publication audit and later fixed-loot candidate selection.

## 2026-08-20 - Issue 8 firearm Wwise regression requalification

- Fresh human evidence that Pistol and possibly other firearm reports are gone supersedes the older listening result. The investigation did not treat the existing structural/runtime pass as audible proof.
- Bounded source and artifact audit found the production bank byte-identical to the last post-polish bank, all five approved WAV sources with nonzero PCM, five unique nonempty embedded media entries, and no recent bank/media byte replacement. A bank/content regeneration hypothesis was therefore rejected as unsupported.
- Published commit `d8fd4ad1836f3ad4d9b54dec908d7818725c64d1` strengthens staging with exact binary chunk/media validation and expands the guarded preview path from Pistol-only to all five exact firearm kinds.
- Full gates PASS: authored-project, deterministic source/audio-polish, production SoundBank, repository, 1,158/1,158 tests, exact-reference clean Release, output, standalone package, strict package, and diff checks.
- Fresh Steam run `20260820T0635323959656Z-88cfa04a0deb4595bfbc2ee8d4284e31` passed 6/6: Ready, one load, all-family IDs 2-6, live Blunderbuss ID 7, ordinary committed Blunderbuss ID 8, and no additional normal post on forced misfire.
- Runtime package/DLL/AssetBundle/SoundBank SHA-256: `8A6E72FEFE3FC2CF585582574776010CDB1DC391E310D5E0807AB72E2023E7E9` / `8DB79393CE44D51029D1B5271B17D75DA4F1E74C38BC365A2232C474B4712821` / `CC9DA6B2FB43FD2932971E3CCE015610497E4C2DB657F62DBA675A31DE327B20` / `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- Issue 8 is human-gated, not declared audibly fixed. A nonzero playing ID cannot prove speaker output, mix, or absence of inherited crossbow layers.
- Next action: Issue 9 firearm selector monograms and Rapid Reload icon audit.

## 2026-08-20 - Issue 9 firearm feat icons qualified

- Root cause: one `KMG_WeaponFocus_*` parameter sprite feeds all five native parametrized feat menus, and the icon publisher assigned the same generic target to every kind. Rapid Reload similarly reused one rejected icon for its top-level feat and children.
- Published `15fff80bef6d6319c5281343264891ce13aa7b4a`: original deterministic P/M/B/Ri/Rv monograms, separate muted salmon reload/ramrod art, editable spec/export source, 64/32 map, provenance, exact publication, package allowlists/counts, semantic test, and live observer.
- Double export reproduced exact hashes. Full gates PASS: repository, 1,159/1,159 tests, exact-reference clean Release, output, SoundBank, deterministic 137-file package, strict package, and diff.
- Guarded Steam run `20260820T0659308177934Z-65bd0924b3dc455ebb04097f00172d90` passed 13/13. Live full and level-up native menus exposed exact B/M/P/Rv/Ri icon names, Rapid Reload/dependent arrays were exact, native top icons were preserved, and selection/prerequisite/effect/isolation/legacy controls passed.
- Runtime package/DLL SHA-256: `FC33F1583EEF3404F7237A54454E9B38C6C770EEA745676ED94AE49D08584F9F` / `0F16A59F89E41157FAB580F109A444A02E28C7089CDCD63D0407C233963A82E0`.
- Final aesthetic/UI-scale acceptance remains human-gated. Next: Issue 10 Elven Branched Spear length, grip, attachment, and attack presentation.

## 2026-08-20 - Issue 10 Elven Branched Spear scale/grip qualified

- Fresh human evidence superseded the earlier visual acceptance. Published diagnostic `47259f964f072bede2c6c51789b6f73bf9d250cd` and failed-closed run `20260820T0712595741030Z-828d00615d54498297ec90f5dfaa4352` measured native `TH_LongspearKnight1` at 2.2825 m along local Y and the rejected custom model at 2.9235 m along local Z. Its child anchors were not part of ordinary Longspear attachment.
- Published repair `4fb73c18514c05918620238a4d6fa6608ee8cf3d` reauthored the source to 2.28 m centered on the primary grip, converts source +Z to native +Y with one explicit prefab `Visual` rotation, and retains every native `WeaponVisualParameters` field except the exact custom model reference.
- Two clean Blender runs reproduced the three FBXs and two PNGs byte-for-byte. Two complete Unity 2018.4.10f1 builds reproduced the 113,269-byte spear bundle at `F671904DDB492EA194C259889D18BC4916E161E107C5E9F179A375DDF87B5B85`.
- Repository validation, 1,159/1,159 tests, clean exact-reference Release, output, SoundBank, standalone package, strict package, and diff gates PASS.
- Guarded Steam run `20260820T0733252707402Z-1a9897121438417f95edefbf51d348e5` passed 22/22. Live custom geometry was 2.27855754 m on +Y versus native 2.28250313 m; all 12 item mappings, native donor fields, mechanics, attacks, switching, effects, identities, and cleanup passed.
- Runtime package/DLL/spear bundle SHA-256: `2DD63CA66036C4CE035B7312F8E771D605D914D42D57FBFCCC970AC646AF80F3` / `B82EBB4243CCC42699E1F2AE6A4C2766FE788097E67FD1E3821C508878F1A469` / `F671904DDB492EA194C259889D18BC4916E161E107C5E9F179A375DDF87B5B85`.
- The disposable unit view deferred synchronous equipped-instance materialization. Final world/inventory doll grip and animation judgment is therefore human-gated, not claimed by renderer bounds.
- Next action: Issue 11 Musket-first, then Blunderbuss, full prefab/IK/muzzle/back/material contract audit and repair.

## 2026-08-20 - Issue 11 Musket and Blunderbuss rig/retexture qualified

- Fresh human evidence superseded the earlier accepted minor-clipping compromise. The audit found Musket at runtime scale `4.186`, Blunderbuss at `20`, inherited attach slots, hidden long-gun holsters, and no independently validated back frame. Crossbow animation and the cloned one-projectile delivery contract remained correct and were preserved.
- The first Blender generator attempt failed before publication because semantic empties were not linked into the view layer. The next two exports exposed process-dependent FBX timestamps and UUIDs; the generator now fixes both exporter seams, and two clean processes reproduce Musket `AF8B08BF8153E23A6B8329A79634A9016347E43DDDF90B53028285B1C25ED397` and Blunderbuss `559F8D8B434729DC8D81881F69C0EB9D39FA33B49402A58A977D004E1DEBF6F3` byte-for-byte.
- The first Unity build correctly rejected Blunderbuss renderer diagonal `0.9912842 m` against a semantic-length bound and exposed an imported support-X handedness flip. The marker sign and renderer envelope were corrected without weakening the `0.86 m` butt-to-muzzle contract.
- Published `b3b76d74edc27664a0bae538e558b7bd6a15b208`: metric +Z licensed-mesh derivatives, exact Grip/Support/Butt/Muzzle/Back markers, muted period tints, scale-1 held prefabs, separate calibrated back prefabs, exact `EquipmentOffsets.IkTargetLeftHand`, fail-closed back validation, inherited-sheath removal only after valid custom back publication, and live held/back calibration controls.
- Two Unity 2018.4.10f1 ForceRebuild passes reproduced the firearm AssetBundle at `1AA75FA1230ABFB60CD5148CA90B99D604DBECE7D80D98D85CB7D7C0A885A8FF`. Repository, 1,159/1,159 tests, clean Release, output, SoundBank, package, strict package, and diff gates passed.
- Guarded Steam run `20260820T0819129064284Z-4b9313e5c2784b099756b97fc139b68e` passed 63/63 at the published SHA. Live Musket/Blunderbuss lengths were `1.33999968/0.8599998 m`; both held frames were identity/scale one, both support targets were exact native IK targets, both back prefabs were distinct and validated, both effective items retained exactly one projectile and Crossbow animation, and inherited sheath models were absent.
- Runtime package/DLL/firearm-bundle/SoundBank SHA-256: `DB09740418E7892BC1174EBA3EC986D2565CB95B09776DB19F119FE0ADB11F51` / `9B0716451906BEAEF47A4D26794C93878F7D1E3F2C771705021C879813751B8E` / `1AA75FA1230ABFB60CD5148CA90B99D604DBECE7D80D98D85CB7D7C0A885A8FF` / `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- Structural runtime evidence cannot establish hand/torso clearance, firing/reload pose, back placement, or texture aesthetics on real bodies. The full world/inventory doll matrix remains human-gated. Next: Issue 12 complete project-owned unique-magic acquisition inventory and organic redistribution.

## Issue 12 automated qualification checkpoint - 2026-08-20

- Baseline checkpoint before Issue 12: `9261dcde6e9442344a37d86f39d7068ebe4a30cc`, branch `codex/gunslinger-overnight-bugfixes`, version `0.0.87`.
- Pre-change observer `20260820T0827266290275Z-observe-rare-firearm-acquisition` passed and enumerated 436 exact fixed-loot candidates.
- Diagnosis: 30 scoped uniques included four multi-project-item clusters, six named Eastern merchant rows, two named branched-spear merchant rows, and Cord in recurring capital Smith stock.
- Repair: every scoped identity now resolves to one distinct deterministic base-campaign `BlueprintLoot`; merchant publication contains no scoped named items; stale project rows are normalized transactionally while native/foreign rows and order are preserved.
- Complete dependency-free suite: 1,160/1,160 PASS. Repository, clean Release/package, output, SoundBank, and strict-package gates PASS.
- First live candidate `20260820T0851451113499Z-5a2a08d9fdc6420d9c8dcef6ed5978eb` failed only a stale Smith-table total; its new 30-item assertion passed. Corrected the exact post-distribution total from 51 to 46 without relaxing reference/null checks.
- Qualified audit `20260820T0855347791407Z-observe-rare-firearm-acquisition`: PASS, 25/25, 30 items/30 targets, every active item `loot=1;target=1;vendor=0`.
- Qualified Cord audit `20260820T0859287762363Z-observe-capital-cord-vendor`: PASS; exact fixed Capital Square Village target and zero vendor rows.
- Runtime package/DLL SHA-256: `72B2E2CDBA163DB127A1ED9B6F6728EBD7AE9DFE5FCA9E674CBF0BFE7D4A81FD` / `3A76F00E2752DB6A7682BC406DEF59FD4E8CDB65410DE3A81AA84831897B6DD9`.
- Remaining human gate: organic pacing/discoverability and unopened/new-campaign container materialization. Existing instantiated state is intentionally not rewritten.
- Next: commit Issue 12, publish, remote-verify, then final release qualification/versioning.

## Final 0.0.88 release qualification checkpoint - 2026-08-20

- Issue 12 commit `89336abc2c5c837fb14a7b4aa267f2ffe1f6aacf` is published with exact local/remote equality.
- Advanced active metadata and guarded-runtime/compatibility package pins to unused patch version `0.0.88`; informational version is `0.0.88-overnight-gunslinger-bugfixes`.
- Added version-aware `validate_urban_barbarian88.py`, inheriting all 0.0.87 validators and adding Issue 12 acquisition invariants. Repository validation PASS.
- Final dependency-free suite: 1,160/1,160 PASS. Clean Release, output, SoundBank, package, and strict-package validation PASS. Compatibility transaction/profile/schema suites PASS.
- Final-version acquisition run `20260820T0917390133721Z-e97b8310338049c294229cb54202ea13`: 25/25 PASS.
- Canonical working-save run `20260820T0920432961745Z-5ed779093f634facb0bdf4c4ce8d829e`: 11/11 PASS on `KMG_AUTOMATION_WORKING`.
- Final manifest-backed exact-reference package: `artifacts/local-runtime/0.0.88/KingmakerGunslinger-0.0.88-local-runtime.zip`, SHA-256 `1A542FCBED586343CE7993606F513D4D0A2555CC49F9DB2BF843C914058210C5`; DLL SHA-256 `2A4E099ACE76046470F598762DB46B766989A48799DED6A5D1D5BDB54ACD9B92`.
- Standalone release archive: `artifacts/packages/KingmakerGunslinger-0.0.88-urban-barbarian.zip`, SHA-256 `858C01EA472A31FCAFDDD89465FA3ACB41CCDDF67D3791F122A5DBEED8F8EC1C`; packaged MSBuild DLL SHA-256 `595AC63E7F0D49F350F10C92E7710A8DFEDF4664B7F533E7EFB150B7B903787E`.
- Firearm/spear/Eastern AssetBundle SHA-256: `1AA75FA1230ABFB60CD5148CA90B99D604DBECE7D80D98D85CB7D7C0A885A8FF` / `F671904DDB492EA194C259889D18BC4916E161E107C5E9F179A375DDF87B5B85` / `079AA2E44E313291C144BD830D302782310274B11375204F9CE8FF6481EF3041`.
- SoundBank SHA-256: `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- The final exact-reference package deployed with manifest `20260820T0925381203669Z`; reusable smoke correctly refused only because release metadata was still dirty. Next: commit/publish release metadata, then rerun reusable mod-load smoke from the clean branch.
