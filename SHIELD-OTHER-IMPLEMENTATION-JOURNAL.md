# Shield Other Implementation Journal

## 2026-08-09 - mission start

- Created `codex/shield-other-spell` from exact frozen SHA
  `7ba84439caa1fc92b8c8148ce95ea79fd59bdc57` after fetching origin.
- Verified the remote Acadamae branch points exactly to the frozen SHA.
- Working tree was clean; no Shield Other branch existed locally or remotely.
- Began installed-content and exact 2.1.7b damage-engine inventory.
- Static CotW scan found no Shield Other candidate.
- Proved the finalized-damage/pre-HP seam documented in
  `planning/SHIELD-OTHER-INVENTORY.md`.

Next: complete native donor/list inventory and implement the schema-2 module
slice with focused tests.

## 2026-08-09 - feature module schema 2 source-qualified

- Added independent default-enabled `shield-other` active/pending setting and
  publication gate; UI continues to label active versus next-restart state.
- Schema 1 migrates atomically to schema 2 with Shield Other enabled. Missing
  fields default enabled and malformed settings retain the established
  quarantine/fail-open policy.
- Expanded deterministic coverage from four to all eight module combinations.
- Complete domain/reflection suite PASS 970/970. The first sandboxed run failed
  only the existing audio temp-file `File.Replace` ACL boundary; the identical
  permitted run passed.
- Full `Build-Local.ps1` gate PASS: repository validation, clean exact-reference
  Release build, build/icon/SoundBank audits, deterministic packaging, and
  strict package validation. Candidate local-runtime package SHA-256
  `0133b6dc193a67252fb5b0a1ff2943446447fc3d6f24408932c7c3013f1eef5a`;
  DLL SHA-256
  `13d0cc8669f785de235b1cbff3591d780d78a122194978b2ade0d3524da4cc28`.

Next: complete native donor/list inventory, then add pure Shield Other damage
and link policies with focused tests.

## 2026-08-10 - pure damage and link policy source-qualified

- Added dedicated pure policies under `Spells/ShieldOther`.
- Finalized HP damage uses `subject=floor(D/2)` and
  `caster=D-subject`; 0/1/2/3 and 1,000,001 conserve exactly and assign every
  odd remainder to the caster.
- Invalid links and guarded transferred events preserve the original target
  damage and transfer zero, preventing reciprocal/nested recursion.
- Link validity fails closed for missing subject/caster, dead caster,
  different area, and distance beyond native close-range scaling
  `25 + 5 * floor(casterLevel/2)` feet; the exact boundary remains valid.
- Complete deterministic suite PASS 974/974. Full exact-reference
  `Build-Local.ps1` qualification PASS; package SHA-256
  `b8168b7365a96bd01e976d84484fce4855df6a4ca9f8ef8cd52480985de05172`;
  DLL SHA-256
  `1d792ccccb013bda15556d592c2a3fbc12a691c5fc9dc2a3f0b33bd0cfe777d9`.

Next: publish this policy checkpoint, then add and run the guarded native
donor/spell-list/final-live duplicate observer before blueprint construction.

## 2026-08-10 - guarded inventory observer source-qualified

- Added save-free `observe-shield-other-inventory`, allowlisted consistently in
  C# and PowerShell, with a dedicated observer outside the runtime orchestrator.
- The observer scans the final live blueprint library for foreign Shield Other
  candidates, records GUID/name/display/type/assembly, inventories close-range
  allied spell donors and relevant buff components, lists candidate domain
  lists, and resolves Cleric/Paladin/Inquisitor/Oracle/Warpriest/Psychic class,
  spellbook, and live spell-list identities.
- Isolated runtime preflight PASS 86/86. A preceding combined command hit only
  the preflight artifact-fingerprint race while prior build outputs settled;
  the unchanged isolated rerun passed.
- Complete deterministic suite PASS 974/974 and exact-reference Release/package
  qualification PASS. Candidate package SHA-256
  `8861e924071ca2c7c9d42b5d259ba8d7ebcfd5f4dcb6e3e5838a84366a6cffdb`;
  DLL SHA-256
  `de5724f39ac198b441f628178885bd21435c28a1f6f791a5aafc83d26116d192`.

Next: commit/publish this observer, rebuild from the clean SHA, then run it
through the guarded Steam App ID 640820 launcher and curate its exact evidence.

## 2026-08-10 - final live duplicate/donor/list inventory PASS

- Rebuilt clean published `ef8b80f`; package SHA-256
  `0bc0f4f5eb67041886aada03bc55810e340181123fe58e9acbd5fc68b9903f09`,
  DLL SHA-256
  `58dc8a211cde4c0d4fb0467ceb6b44aacad24b07041b750cc6f84803af5be68d`.
- Guarded final-live inventory PASS:
  `20260810T0408000666226Z-observe-shield-other-inventory`.
- Zero duplicate candidates among 104,644 loaded blueprints. Required five
  base lists and unambiguous CotW Oracle/Warpriest/Psychic live chains are now
  recorded in `planning/SHIELD-OTHER-INVENTORY.md`.
- No save name, save load, input, or save mutation was used.

Next: assign the two necessary stable identities, construct presentation-only
donor clones with exact mechanics, and implement transactional level-2 base
publication plus pure publication tests.

## 2026-08-10 - stable ability and target-buff identities

- Assigned append-only symbols/GUIDs: ability
  `6a8c4c1d2fbe4d6a9a724988c1348401` and target buff
  `7bd92e3c44ad42e7b523ee8ed7afc602`.
- Registration is unconditional across module settings; active registry count
  is 254 and ledger count is 255 including the historical reservation.
- The factory clones native Shield of Faith presentation, then replaces its
  mechanics with an explicit Abjuration spell, harmless ally-only close-range
  targeting, caster-level hours, Extend-enabled native duration, and exact +1
  deflection AC/+1 resistance all-saves components. Material focus is documented
  and abstracted; no inventory component is created.
- Complete validation, 975 deterministic tests, exact-reference Release build,
  strict package validation, and local-runtime packaging PASS.

Next: commit/publish this identity checkpoint, then implement pure-tested
transactional level-2 base-list publication and rollback.

## 2026-08-10 - transactional base spell-list publication

- Added a dedicated five-list level-2 transaction for Cleric, Paladin,
  Inquisitor, Community domain, and Protection domain.
- Publication preserves native/foreign order, singularizes Shield Other by
  reference and GUID, recognizes aliased physical level lists, clears the exact
  `m_SpellsFiltered` cache, and is idempotent.
- Rollback retains original list references and refuses if a later foreign
  replacement makes restoration unsafe. Partial publication rolls back as one
  unit. Publication failure is isolated so registered identities and the
  Gunslinger/Acadamae modules remain available.
- Complete validation, 977 deterministic tests, exact-reference Release build,
  and strict package validation PASS.

Next: publish this checkpoint, then implement first-idle optional CotW discovery
and idempotent Oracle/Warpriest/Psychic reconciliation.

## 2026-08-10 - first-idle optional CotW reconciliation

- Added a dedicated first-idle final-live scan after all LoadDictionary postfixes.
- CotW absence is normal. Present Oracle, Warpriest, and Psychic classes must be
  unambiguous by internal/display identity, class-owned spellbook/list structure,
  maximum spell level, spontaneous/prepared model, divine/arcane flag, and
  casting attribute; known GUIDs are supporting signals rather than sole signals.
- Optional level-2 publication is transactional, preserves foreign entries,
  clears native caches, and immediately performs a second idempotent pass.
- A final-live foreign Shield Other candidate prevents optional publication and
  rolls back the retained base transaction; unsafe rollback fails closed.
- Complete validation, 978 deterministic tests, exact-reference Release build,
  and strict package validation PASS.

Next: commit/publish this checkpoint, add a guarded live blueprint/publication
observer, and verify the exact CotW casting-model and all eight list memberships.

## 2026-08-10 - publication observer strengthened

- Extended the existing guarded, save-free inventory observer to assert exactly
  one level-2 Shield Other membership in all five required lists and every live
  optional CotW list.
- Diagnostics now record Oracle/Warpriest/Psychic spontaneous, arcane, and
  casting-attribute fields for exact live validation of the structural resolver.
- Complete validation, 978 deterministic tests, exact-reference Release build,
  and strict package validation PASS.

Next: publish the observer checkpoint, rebuild its clean SHA, then run the
guarded inventory scenario and curate exact membership/model evidence.

## 2026-08-10 - first guarded publication run and timing repair

- Guarded run `20260810T1235457719557Z-observe-shield-other-inventory` failed
  before a structured result. Exact `output_log.txt` showed a bootstrap
  `NullReferenceException` after UI attachment.
- Root cause: `Main.Load` dereferenced `BlueprintBootstrap.ShieldOther.Ability`
  while LoadDictionary had not yet supplied a pending library in that process.
- The repair attaches the dedicated callback with context only and resolves the
  library, identities, immutable setting, and retained base transaction at the
  actual first idle update. No gameplay or publication policy changed.
- Complete validation, 978 deterministic tests, exact-reference Release build,
  and strict package validation PASS.

Next: publish this timing repair, rebuild the clean SHA, and rerun the guarded
save-free observer once with the changed strategy.

## 2026-08-10 - publication mechanics pass, observer self-filter repair

- Repaired run `20260810T1239058867514Z-observe-shield-other-inventory`
  reached a structured result. All eight required/discovered level-2 list
  memberships passed exactly once, and first-idle reconciliation completed twice.
- Exact CotW models match the resolver: Oracle spontaneous divine Charisma;
  Warpriest prepared divine Wisdom; Psychic spontaneous non-arcane Intelligence.
- The sole FAIL was observer-only: its foreign-duplicate scan counted the newly
  registered project ability itself. Production final-live scanning already
  excluded the ability by reference. The observer now excludes the stable project
  GUID before candidate classification.

Next: qualify/publish the observer-only self-filter, then rerun once to capture a
fully PASS structured result without changing publication mechanics.

## 2026-08-10 - final-live publication PASS

- Guarded run `20260810T1243101482251Z-observe-shield-other-inventory`
  passed on exact published source `823195c683d633fabbe4916119e847e65bb61013`.
- Foreign duplicate count was zero. All five base and three discovered CotW
  lists contained Shield Other exactly once at level 2.
- No save was named, selected, loaded, or mutated.

Next: commit/publish curated evidence, then implement the save-persistent link
component and lifecycle revalidation before the damage transpiler.

## 2026-08-10 - persisted link lifecycle component

- Target buff now owns a dedicated `ShieldOtherBuffComponent`; the native buff
  context persists originating caster and cast-time caster level across saves.
- Exact inspection found no safe public movement/area subscriber contract, so
  the component uses native `ITickEachRound` as its low-cost fallback. It removes
  missing-caster, missing-level, dead-caster, non-live/different-area, and
  out-of-close-range links. The damage seam will revalidate immediately.
- No caster marker identity is required; one caster can maintain multiple target
  buffs, while `StackingType.Replace` gives each subject one nonstacking link.
- Complete validation, 979 deterministic tests, exact-reference Release build,
  and strict package validation PASS.

Next: commit/publish the lifecycle checkpoint, then implement the exact
signature-validated finalized-damage transpiler and exception-safe transfer guard.

## 2026-08-10 - finalized HP split runtime

- Added a Harmony transpiler which requires the exact installed
  `ApplyDifficultyModifiers -> Damage setter -> LastHandledDamage setter` IL
  sequence and inserts one callback immediately after finalized damage is stored.
- The callback revalidates the persisted caster link immediately, applies the
  pure odd-remainder-to-caster split, and triggers one native direct-damage event
  for the caster. A thread-static, `finally`-cleared guard prevents reciprocal,
  nested, and transferred-event recursion.
- The transferred callback overwrites its finalized amount after caster
  difficulty/defense processing, while leaving native temporary-hit-point and HP
  application intact. It creates no attack, save, critical, precision, or source
  rider event. A native warning/combat-log event identifies the transfer.
- If the guarded transfer throws, the original event is restored to its complete
  finalized subject damage before native HP application; no heal-after-damage
  approximation exists.
- Complete validation PASS: 980/980 deterministic tests, exact-reference Release
  build, output validation, SoundBank validation, and strict package validation.
  The first two sandboxed attempts encountered only the existing audio staging
  fixture's denied Windows temp-directory `File.Replace`; the approved
  unrestricted rerun passed without a source change.

Next: commit/publish this damage-runtime checkpoint, then add the disposable
Shield Other behavior and damage runtime scenario needed to prove the exact seam.

## 2026-08-10 - first disposable damage scenario

- Added allowlisted, save-free `disposable-shield-other`. It registers only two
  disposable chargen units in the live unit registry, applies the real persisted
  target buff at caster level 5, and verifies all four +1 bonuses.
- The scenario triggers native `RuleDealDamage` events for finalized values 3 and
  4 and requires exact HP deltas 1/2 and 2/2 plus two player combat-log events.
- Cleanup removes and disposes both units and proves neither remains registered.
- Source qualification PASS: runtime preflight 86 checks, 980/980 deterministic
  tests, exact-reference Release build, and strict package validation.

Next: commit/publish, rebuild the clean SHA, then execute the guarded scenario
through Steam and inspect structured HP/log evidence before expanding its matrix.

## 2026-08-10 - installed-profile timeout and strategy change

- The first guarded `disposable-shield-other` attempt used the currently installed
  broad CotW Mods set. Evidence directory:
  `20260810T1305018393571Z-disposable-shield-other`.
- The request argument, normalized request path, and exact loaded build identity
  were recorded, but the process never reached request consumption. The game log
  stopped during the known CotW startup path at `[Manager] Spawning`; no Shield
  Other callback, mechanics exception, save selection, or save mutation occurred.
- After the established 600-second extended CotW bound, the unchanged strategy
  was retired. The responsive unattended process was force-terminated; it exited
  after bounded verification. This is launch-environment evidence, not a mechanics
  failure and not a PASS.
- Added the scenario to the existing transactional compatibility-profile runner
  so the next attempt isolates `gunslinger-only` and proves exact Mods restoration.

Next: source-qualify/publish the profile allowlist, then run the disposable
scenario in `gunslinger-only` with transactional restoration.

## 2026-08-10 - transpiler declaring-type repair

- Isolated run `20260810T1320348526120Z-disposable-shield-other` proved an exact
  pre-bootstrap Harmony failure. The transpiler correctly targeted the native IL
  sequence but attempted to reflect `LastHandledDamage` from `RuleDealDamage`.
- Exact IL shows the call is `UnitEntityData.set_LastHandledDamage(rule)`. The
  null property lookup caused the `TargetInvocationException`; no gameplay or
  Shield Other mechanics ran.
- Transaction `compat-20260810T131953Z-8ac491185118` was explicitly recovered
  after the outer monitor bound, and exact restoration verified `True`.
- The narrow repair resolves the setter from `UnitEntityData`; the insertion
  location and runtime policy are unchanged.

Next: complete source/build/package qualification, publish, then rerun the
transactional standalone scenario on the changed clean SHA.

## 2026-08-10 - first standalone mechanics PASS

- Transactional `gunslinger-only` run
  `20260810T1330351072971Z-disposable-shield-other` passed on exact published
  source `5e81423e9002234b1cf0a77de7b8357aef67d77e`.
- Live results: one target link; AC/Fortitude/Reflex/Will deltas all +1; finalized
  damage 3 produced subject/caster HP loss 1/2; finalized damage 4 produced 2/2;
  exactly two transfer combat-log events; disposable cleanup true.
- Runtime transaction `compat-20260810T132959Z-c342ce67b0f1` restored the prior
  Mods directory exactly (`restorationVerified=True`). No save was named, loaded,
  or mutated.

Next: expand the disposable scenario across replacement/multi-subject links,
range/death/dispel lifecycle, defenses/temp HP, lethal and recursion/exclusion
cases before the required profile repetitions.

## 2026-08-10 - expanded link lifecycle scenario ready

- Extended `disposable-shield-other` with a second caster and subject.
- It now requires latest-caster replacement on one subject, simultaneous links
  from one caster to two subjects, removal beyond caster-level-5 close range,
  and native buff removal equivalent to dispel. Post-removal damage must remain
  wholly on the subject and create no transfer log.
- Complete source qualification PASS: 980/980 tests, exact-reference Release
  build, output/SoundBank validation, and strict package validation.

Next: publish/rebuild and run the expanded standalone scenario transactionally.

## 2026-08-10 - expanded standalone lifecycle PASS

- Run `20260810T1337175179075Z-disposable-shield-other` passed on exact source
  `9afdc95caa124523f6778f1fd9bb75b60d949cad`.
- Replacement was singular and moved transfer to caster two (`1/0/1` subject/
  first/second caster). One caster maintained two subjects (`1/1`). Moving the
  second subject to 20 meters removed the caster-level-5 link and yielded `2/0`;
  explicit native buff removal also yielded `2/0`. Transfer logs remained exactly
  four, so invalid/removed links emitted none.
- Transaction `compat-20260810T133642Z-7b898f99d298` restored exactly. No save
  was named, loaded, or mutated.

Next: implement the defense/temp-HP/lethal/exclusion/recursion runtime matrix.

## 2026-08-10 - exclusion, recursion, and lethal scenario ready

- Added native Strength and Constitution `RuleDealStatDamage` checks requiring
  exact stat damage with no caster HP transfer.
- Added reciprocal persisted links and requires one 3-point event to remain
  exactly subject 1/caster 2, proving the transferred event cannot recurse.
- Added a native lethal-threshold caster fixture: the caster begins at 1 HP and
  the 2-point transferred share must reach zero/death state while the subject
  retains only 1 damage.
- Complete validation PASS: 980/980 tests, exact Release, output/SoundBank, and
  strict package validation.

Next: publish/rebuild and run this expanded standalone scenario transactionally.

## 2026-08-10 - exclusions, recursion, and lethal standalone PASS

- Run `20260810T1343395800417Z-disposable-shield-other` passed on exact source
  `30098c73a7aa1820f5f889c140a55d4d376e7859`.
- Strength damage 2 and Constitution damage 1 produced no caster HP transfer.
  Reciprocal links conserved one event as subject 1/caster 2, and the final
  transfer reduced a 1-HP caster through the lethal threshold while retaining
  subject share 1. Exactly six legitimate transfer log entries were observed.
- Transaction `compat-20260810T134303Z-8c43ad78f3c0` restored exactly; no save
  was named, loaded, or mutated.

Next: add native target/caster temporary-HP coverage and typed mitigation donors.

## 2026-08-10 - native temporary-HP scenario ready

- Added native `ModifiableValueTemporaryHitPoints` modifiers to the linked
  subject and caster in separate 6-point damage events.
- Subject case requires its 3-point share to consume temporary HP only while the
  caster loses 3 HP. Caster case requires subject HP loss 3 while the transferred
  share consumes caster temporary HP only. Modifiers are explicitly removed.
- Complete validation PASS: 980/980 tests, exact Release, output/SoundBank, and
  strict package validation.

Next: publish/rebuild and run the temporary-HP matrix transactionally.

## 2026-08-10 - temporary-HP observation refresh

- Run `20260810T1349330912897Z-disposable-shield-other` failed only its two
  temporary-HP consumption counters. Mechanical HP outcomes were already exact:
  subject temporary HP yielded subject HP 0/caster HP 3, and caster temporary HP
  yielded subject HP 3/caster HP 0.
- `ModifiableValueTemporaryHitPoints` had not refreshed its cached displayed
  value before the immediate observation. The scenario now calls native
  `UpdateValue()` after each rule completes and before reading remaining temp HP.
- Transaction `compat-20260810T134851Z-f1299dd53841` restored exactly.

Next: qualify/publish the observation repair and rerun once on the changed SHA.

## 2026-08-10 - temporary HP PASS and eight-way module repair

- Run `20260810T1354174801203Z-disposable-shield-other` passed on exact source
  `5c2c5445fc64ab06149d630d6f16b8bcea83048a`; both subject and caster consumed
  exactly 3 of 5 native temporary HP with HP outcomes `0/3` and `3/0`.
- Transaction `compat-20260810T135338Z-4a835b96b91a` restored exactly.
- Repaired the guarded module observer and both profile/matrix writers from the
  inherited two-module schema to schema 2 with Shield Other. Runtime now requires
  three immutable active settings, constant 254 identities, and publication in
  all live spell lists iff Shield Other is enabled. The direct matrix now defines
  all eight Boolean combinations.

Next: source-qualify/publish the module-matrix repair, then run all eight settings
under isolated standalone transactions.

## 2026-08-10 - request serializer three-setting repair

- The first all-enabled profile attempt failed pure preflight before deployment:
  the authoritative runtime request validator/serializer still required only the
  inherited two Boolean parameters. Transaction
  `compat-20260810T135922Z-4b9aa824e344` restored exactly.
- Updated the authoritative validator and serialized request payload to require
  and carry `shieldOther` alongside Gunslinger and Acadamae Graduate.

Next: qualify/publish this final matrix plumbing repair, then retry all eight.

## 2026-08-10 - runtime-side module request repair

- All-enabled run `20260810T1403193768024Z-observe-feature-module-settings`
  reached the exact mod but was rejected at request acceptance with sanitized
  reason `module-states-required`; no observer or save action ran.
- The remaining runtime-side `RuntimeTestRequest` validator still required two
  properties. It now requires exactly the same three Boolean fields as the
  PowerShell preflight and serializer, with a focused source contract test.
- The wrapper could not restore while the rejected process remained open; after
  bounded termination, explicit transaction recovery for
  `compat-20260810T140244Z-f0f80ecf5cc6` verified exact restoration `True`.

Next: validate/publish this final acceptance repair and retry all-enabled.

## 2026-08-10 - eight-way standalone module matrix PASS

All eight guarded `gunslinger-only` profile combinations passed on exact source
`c90c0e43710e4801694b770de5e63121e00d2fd3`. Every run observed 254 registered
identities; each enabled module published only its own surfaces; each disabled
module published none; and every compatibility transaction restored exactly.

| Gunslinger | Acadamae | Shield Other | Evidence |
|---|---|---|---|
| on | on | on | `20260810T1409406936012Z-observe-feature-module-settings` |
| on | on | off | `20260810T1411348446923Z-observe-feature-module-settings` |
| on | off | on | `20260810T1414302754267Z-observe-feature-module-settings` |
| on | off | off | `20260810T1416314266005Z-observe-feature-module-settings` |
| off | on | on | `20260810T1418248328119Z-observe-feature-module-settings` |
| off | on | off | `20260810T1420225795438Z-observe-feature-module-settings` |
| off | off | on | `20260810T1422191614520Z-observe-feature-module-settings` |
| off | off | off | `20260810T1424139686495Z-observe-feature-module-settings` |

The profile-internal deterministic suite remained 981/981 PASS and strict
package validation passed throughout. Next: typed mitigation/immunity and the
remaining lifecycle/save-load runtime assertions.

## 2026-08-10 - typed physical and energy runtime fixture

- Added deterministic native piercing and fire packets with fixed pre-rolled
  values to the disposable scenario. Each assertion observes the finalized
  target and caster HP deltas independently.
- `Build-Local.ps1`: repository validation PASS, 981/981 deterministic tests
  PASS, exact-reference Release build PASS, strict package validation PASS.
- Local runtime package SHA-256:
  `fa77b367906c0388a5a764fced46094246d939dcb8f38af826839216498fbc2a`;
  DLL SHA-256:
  `3007e03a427ce0eb8bdabe16f8c29368721fab7d181ba380e12d458446169147`.

Next: commit/publish this source-qualified fixture and run it in the isolated
standalone profile.

## 2026-08-10 - typed physical and energy runtime PASS

- Guarded standalone evidence:
  `20260810T1430187656299Z-disposable-shield-other`.
- Exact source: `ee0b4fa5f4d30b94dcb10c141eabac139fe6594b`.
- Native piercing 4 finalized to subject/caster `2/2`; native fire 4 finalized
  to `2/2`. All earlier link, bonus, replacement, multiple-subject, range,
  dispel, exclusion, reciprocal, temporary-HP, lethal, logging, and cleanup
  assertions also passed.
- Exactly 10 transfer log entries were observed. Transaction
  `compat-20260810T142942Z-34d10d67c9bb` restored exactly (`True`).

Next: add native target mitigation/immunity fixtures and prove the caster share
is derived from the once-mitigated finalized value without a second defense pass.

## 2026-08-10 - mitigation/immunity fixture source-qualified

- Added request-local native `AddDamageResistancePhysical`,
  `AddDamageResistanceEnergy`, and `AddEnergyImmunity` facts without registering
  any blueprint identity or mutating the live library.
- The cases assert target DR/resistance before the split, caster DR/immunity not
  reapplying to the direct transferred share, and target immunity producing no
  HP loss or transfer.
- `Build-Local.ps1`: repository validation PASS, 981/981 tests PASS,
  exact-reference Release build PASS, strict package validation PASS.
- Local package SHA-256:
  `4a23ba78b16d1f9755036b386ecfce62f89fd983ab5fc559b2c5c583c6a750e0`;
  DLL SHA-256:
  `be417b5d685381128aaa7cdd92fbbc424b1437939aca9a77e04c77572d2e7e68`.

Next: commit/publish and run the guarded standalone disposable scenario.

## 2026-08-10 - mitigation/immunity runtime PASS

- Guarded evidence: `20260810T1436273826468Z-disposable-shield-other` on exact
  source `1c152496abb5af17f016d2e1a6c7d03532d64666`.
- Piercing 6 minus subject DR 2 finalized to 4 and split `2/2`; caster DR 100
  did not reduce the transferred share.
- Fire 6 minus subject resistance 2 finalized to 4 and split `2/2`; caster fire
  immunity did not reduce the transferred share.
- Subject fire immunity finalized to zero and produced subject/caster `0/0`.
- All prior assertions, exactly 12 transfer logs, request-local cleanup, and
  transaction `compat-20260810T143552Z-584d80887866` restoration passed.

Next: qualify area transition and save/load persistence, then begin required
profile repetitions.

## 2026-08-10 - area/death lifecycle fixture source-qualified

- Added request-local live assertions for the exact `UnitEntityData.IsInGame`
  loaded-area boundary and for round-tick removal after transferred damage kills
  the caster. Later damage must remain wholly on the subject after removal.
- `Build-Local.ps1`: repository validation, 981/981 tests, exact-reference
  Release build, and strict package validation all PASS.
- Package SHA-256:
  `36f0cd791f3b836ccd27c9bba0989503a11a39589023b8536a33d47d4097885b`;
  DLL SHA-256:
  `014c4c41f2426825eb82aa8b192ce71db01cf865c477bd88efe82ad1f5eb84d8`.

Next: commit/publish and run guarded standalone runtime qualification.

## 2026-08-10 - top-level context serialization rejected

- `20260810T1450578085280Z-disposable-shield-other` on exact source
  `85f94fbcd24f88471dd037aa34ddbffbbfc01f60` failed closed at
  `native-save-context-roundtrip`: Kingmaker's `DictionaryConverter` rejected a
  top-level `MechanicsContext` because that type is serialized only within its
  surrounding unit graph. No fixture mutation followed the error.
- Transaction `compat-20260810T145017Z-507e7c5d18d3` restored exactly.
- Replaced the invalid boundary with the engine's exact
  `UnitSerialization.Serialize(UnitDescriptor)` graph and native
  `DefaultJsonSettings` reconstruction. The assertion now locates the restored
  Shield Other buff and validates its live caster, subject, and caster level.

Next: source-qualify and rerun this exact unit-save graph.

## 2026-08-10 - unit graph serialized; reconstruction narrowed

- `20260810T1455123546800Z-disposable-shield-other` on exact source
  `57181a87a426e3be67f66eb312289cf26c112d9c` serialized the unit graph to
  20,465 bytes and ran every gameplay assertion successfully, but the combined
  reconstructed-context assertion was false.
- Transaction `compat-20260810T145438Z-9a776e678fcc` restored exactly.
- Added field-by-field observation for reconstructed descriptor, buff, context,
  caster, params/caster level, and target. No persistence criterion is weakened;
  the next run identifies the exact unresolved native reference.

Next: source-qualify/publish the narrower instrumentation and rerun.

## 2026-08-10 - JSON construction requires native unit PostLoad

- `20260810T1458316956358Z-disposable-shield-other` on exact source
  `45549c369d8d115c28dcb07fbe63cddfcf997700` proved descriptor reconstruction
  succeeded, while `RawFacts` had not yet materialized the buff. All gameplay
  assertions passed and transaction `compat-20260810T145757Z-c6e59e37421c`
  restored exactly.
- The fixture now follows the next exact engine lifecycle step: attach the
  restored descriptor to a detached `UnitEntityData`, invoke native `PostLoad`,
  then inspect the materialized Shield Other buff/context. The detached restored
  entity is disposed in `finally`.

Next: source-qualify/publish and rerun the post-load graph.

## 2026-08-10 - exact JSON unit constructor identified

- `20260810T1502144921909Z-disposable-shield-other` on exact source
  `83e31a7585a834dcd8728f06ed6b4c7ce7793fbd` failed before post-load because
  the public view-based `UnitEntityData` constructor rejects a null view.
- Transaction `compat-20260810T150136Z-98d1ebc87297` restored exactly.
- Exact 2.1.7b reflection shows the save loader boundary is the nonpublic
  `UnitEntityData(JsonConstructorMark)` constructor followed by private
  `Initialize(UnitDescriptor, bool)`. The fixture now invokes those exact methods
  before native `PostLoad`, failing closed if either signature changes.

Next: source-qualify/publish and rerun the exact loader sequence.

## 2026-08-10 - restored buff lookup corrected to stable GUID

- `20260810T1505518476389Z-disposable-shield-other` on exact source
  `16e78d32400c37e44fc0e418fd94ea3860942781` completed native JSON construction,
  private initialization, and `PostLoad` without exception, while the lookup
  still required pre-load blueprint object-reference identity.
- Transaction `compat-20260810T150511Z-13543178b558` restored exactly.
- Cross-load blueprint identity is the stable asset GUID, not CLR reference
  identity. The lookup now accepts the exact target-buff GUID and reports raw
  fact/buff counts; caster, target, and caster-level checks remain strict.

Next: source-qualify/publish and rerun the corrected cross-load identity lookup.

## 2026-08-10 - descriptor utility confirmed to strip live buffs

- `20260810T1510107775960Z-disposable-shield-other` on exact source
  `e7d2e6dc7bb41536d463ed23468f6b77b4dadf5b` reported `rawFacts=0` after the
  complete descriptor-loader lifecycle. Stable-GUID lookup was therefore not the
  issue: `UnitSerialization.Serialize(UnitDescriptor)` is a stripped descriptor
  utility and does not represent the live save unit graph.
- Transaction `compat-20260810T150917Z-4456c06824ee` restored exactly.
- The fixture now serializes caster and subject `UnitEntityData` together through
  native `DefaultJsonSettings`, reconstructs both by stable entity ID, invokes
  `PostLoad` on both, and requires the restored buff to link the restored pair.
  It fails if deserialization reuses either live fixture object.

Next: source-qualify/publish and run the two-unit native save graph.

## 2026-08-10 - synthetic partial graphs retired

- `20260810T1514029662666Z-disposable-shield-other` on exact source
  `de1be27fceaf80ef19b4647ab28c7d7283764e35` failed before reconstruction with
  the same native `DictionaryConverter` constraint when serializing paired units.
- Transaction `compat-20260810T151320Z-66eaf4e97106` restored exactly.
- Partial top-level context, stripped descriptor, and paired-unit JSON graphs are
  not the actual game save boundary. The synthetic assertion has been removed
  from `disposable-shield-other`; all failed evidence is retained. Save/load will
  be qualified only through the guarded real-save mechanism and the authorized
  `KMG_AUTOMATION_WORKING` save, never the protected baseline.

Next: restore the disposable scenario to PASS, then implement the guarded
working-save Shield Other persistence stages.

## 2026-08-10 - expanded disposable scenario restored PASS

- `20260810T1517266807175Z-disposable-shield-other` passed on exact source
  `0c6a3c7d6ebd9110c78a88bb00beaa625f1444f8` after retiring the invalid
  synthetic save graph.
- All expanded direct/typed/mitigated/immune/temp-HP/reciprocal/lethal and
  range/area/dispel/death lifecycle assertions passed.
- Transaction `compat-20260810T151653Z-12335a066239` restored exactly.

Next: implement genuine persistence qualification through the guarded
`KMG_AUTOMATION_WORKING` save path.

## 2026-08-10 - CotW and highest-risk consecutive runtime PASS

Exact source `76e5ce35a807388053ed4f911ea4f1c892611870` completed the expanded
`disposable-shield-other` scenario twice in each required compatibility profile:

| Profile | Evidence | Transaction | Restored |
|---|---|---|---|
| `gunslinger-call-of-the-wild` | `20260810T1520404782090Z-disposable-shield-other` | `compat-20260810T151958Z-590855347fb1` | True |
| `gunslinger-call-of-the-wild` | `20260810T1523377379140Z-disposable-shield-other` | `compat-20260810T152250Z-67c8fd2820b7` | True |
| `gunslinger-high-risk-combined` | `20260810T1526369993443Z-disposable-shield-other` | `compat-20260810T152550Z-fa0338a3226d` | True |
| `gunslinger-high-risk-combined` | `20260810T1529104271088Z-disposable-shield-other` | `compat-20260810T152834Z-2f541389dd9b` | True |

The high-risk profile contained exact local Call of the Wild, Arms and Armor,
and Toggle Custom Soundpacks. All gameplay assertions and request-local cleanup
passed in every fresh launch.

Next: guarded real-save persistence, then final 0.0.77 release-source repeats.

## 2026-08-10 - lifecycle run exposed lethal-state gap

- `20260810T1442147693813Z-disposable-shield-other` on exact source
  `454652a71fdb048638cb11428037332aacfd3f31` failed only
  `shield-other-caster-death-termination`. Area unload passed with damage `2/0`;
  all damage, mitigation, cleanup, and other lifecycle assertions passed.
- Transaction `compat-20260810T144135Z-dcad894ec3af` restored exactly.
- Transferred damage reached the lethal boundary before Kingmaker advanced
  `State.IsDead`. Link validity now requires positive HP and absence of
  `IsDead`, `MarkedForDeath`, and `ForceKill`, preventing a stale link during
  deferred death-state publication.

Next: source-qualify, commit/publish, and rerun the same guarded scenario.

## 2026-08-10 - area/death lifecycle runtime PASS

- Guarded evidence: `20260810T1446127542431Z-disposable-shield-other` on exact
  source `79552bd9974907bf001ca645d0974cb05e72dfd8`.
- Caster `IsInGame=false` removed the link on the native round tick and later
  damage remained subject/caster `2/0`.
- After transferred damage reached the lethal caster boundary, the next native
  round tick removed the link using the repaired full lethal-state contract.
- All prior assertions passed; transaction
  `compat-20260810T144535Z-f33a748e4b66` restored exactly.

Next: inspect and use the exact Kingmaker save serializer for a request-local
buff/context persistence round trip before profile repetitions.

## 2026-08-10 - native save-context fixture source-qualified

- Added an exact `DefaultJsonSettings` round trip for the live Shield Other
  `MechanicsContext`, requiring reconstructed object-reference resolution to the
  same registered caster and subject plus caster level 5.
- This follows the repository's established disposable grit persistence pattern
  and writes no save file or protected slot.
- `Build-Local.ps1`: repository validation, 981/981 tests, exact Release, and
  strict package validation PASS. Package SHA-256
  `25e153a66e36f9936f44ad400b564d9ec86b8ffd6524ad9ff211cbc4d6f48ade`;
  DLL SHA-256
  `205120277bae36d5b1502ce40a02792b432e5d1b5675b321b1fd640c60341c0d`.

Next: commit/publish and run guarded standalone runtime qualification.
