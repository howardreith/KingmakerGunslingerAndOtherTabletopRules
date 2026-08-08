# Rare Firearms and Campaign Integration Journal

## 2026-08-08 — Baseline and first forensics

- Created exact branch from clean `origin/master` merge
  `1c570bd4211d69c5c29f6af46a870146adb1645b`, version 0.0.73. Required
  archetype tip ancestry, clean tree, absent Git lock, and helper allowlist were
  verified; no competing Git/build process was observed.
- Completed archetype evidence records 930/930 deterministic, clean Release,
  SoundBank/package, repeated archetype runtime and working-save PASS. Inherited
  Dodge and Targeting Torso classifications and public CotW status are preserved.
- Assigned nine collision-free design-time GUIDs; no runtime generation.
- Guarded read-only vendor run
  `20260808T1720275373614Z-observe-vendor-table-contracts`, runtime ID
  `20260808T1720275529421Z-c5dbf9e887b64c1b89ac129ba490d2b3`, passed: 43
  tables, 26 associations, exact fixed fields and all current project counts.
- Jhod is structurally over-shared and rejected. Merchant ownership and fixed
  loot/reference uniqueness require a bounded observer extension.
- Extended the existing save-free vendor observer in place (no new activation
  surface) to enumerate exact candidate native weapon enchantments and donors,
  keyword-bounded fixed `BlueprintLoot`/`BlueprintUnitLoot` contents, areas,
  components, and shallow direct reference owners. The code performs only
  reads; no table, loot, inventory, UI, or save API is invoked.
- Source qualification: repository validation PASS; complete 930/930 domain
  suite PASS with its established authorized temp access. The initial sandboxed
  run reproduced only the inherited audio `File.Replace` access denial.
- Clean exact-reference Release, build-output, SoundBank, package creation, and
  strict package validation PASS for 0.0.73. The first guarded invocation
  correctly refused the uncommitted source state; no Kingmaker launch occurred.
- Published observer `b657dc2` launched guarded run
  `20260808T1729448091782Z-observe-vendor-table-contracts`, but its first
  reference strategy timed out before a result: it rescanned the full graph for
  every candidate and enumerated arbitrary collections. Kingmaker exited and no
  mutation/result artifact was produced. This is a rejected instrumentation
  strategy, not content evidence.
- Narrow repair builds one direct-reference index in a single pass and inspects
  only blueprint-valued fields and arrays. The repaired source again passed
  repository validation, 930/930 tests, clean Release, build-output, SoundBank,
  package creation, and strict package validation.
- Repaired guarded PASS:
  `20260808T1734599486274Z-observe-vendor-table-contracts`, runtime ID
  `20260808T1734599848444Z-0502d6b37e1a4049a522927f30c23f3a`, exact source
  `2dc9f99`. It resolved native enhancement, Fey Bane and Thundering contracts
  and 437 bounded fixed loot candidates. Five exact base-campaign targets were
  selected in the acquisition inventory using area/chapter/theme/distinctness.
- The internal-name filter did not resolve Seeking, and the existing unit-only
  vendor owner graph did not identify Smith/general table owners. A final
  bounded forensics refinement now also matches enchantment display names and
  indexes direct references for all 43 vendor tables.
- Final vendor-reference run
  `20260808T1739377557378Z-observe-vendor-table-contracts`, runtime ID
  `20260808T1739377910467Z-f45ce288a1504610a85c458eb6de1e26`, passed on exact
  `9ba25bd`. It proved `SmithVendorTable` is directly referenced once by each of
  `CapitalOwlbearAttack_Blacksmith` and `VerdelBlacksmith`; this capital
  blacksmith table is selected. No enchantment internal or display name matched
  Seeking. A last materially distinct component-type scan is being added for
  concealment mechanics before applying the nonoptional-property hard stop.
- Final component-based PASS
  `20260808T1744000629586Z-observe-vendor-table-contracts`, runtime ID
  `20260808T1744000942305Z-9576e406512a450bbe5766283bc57d5b`, exact source
  `d0e039a`, enumerated 17 relevant weapon enchantments and found neither a
  Seeking identity/display nor any enchantment with a concealment component.
  Together with the prior two live strategies and installed-assembly search,
  this establishes the work order's critical required-native-property hard stop.
  No production blueprint, vendor, loot, Reliable, or version change began.
- Continuation audit identified one remaining false-negative route: a native
  enchantment may use a generic component whose scalar/enum field selects a
  concealment mechanic. The observer now scans every weapon-enchantment
  component's primitive, enum, and string fields and records their exact values.
  This deeper strategy passed repository validation, 930/930 tests, clean
  Release, build-output, SoundBank, package creation, and strict validation.
- Deeper guarded run
  `20260808T1748440170104Z-observe-vendor-table-contracts`, runtime ID
  `20260808T1748440250732Z-7e65d222eca74ef59b9522c572e1ac8e`, passed on exact
  `ebef1c3` and found no generic scalar/enum/string concealment mechanic. The
  provisional stop is confirmed; no independent safe evidence strategy remains.

## Exact next action

The 2026-08-08 continuation amendment is new user authority. Checkpoint
`69fd9a0b9e5889082f15f0e536eb940e79be138b` was verified clean, published and
identical locally/remotely; it descends from baseline `1c570bd`. Native Seeking
absence remains conclusive historical evidence and must not be searched again.
The prior blocker is resolved by user authority, project-owned Seeking GUID
`036fc59fd1e24753b98f9d92cdb1e93e` is authorized, and the registration total is
now ten. Mission status is resumed.

After publishing this amendment checkpoint, inspect exact installed concealment
attack IL/signatures and record the narrow safe hook contract; then add the
stable Seeking manifest identity, focused source policies/tests, and continue
the complete original implementation and qualification mission.

## 2026-08-08 — Project Seeking installed-contract and source checkpoint

- Exact installed Assembly-CSharp MVID is
  `07FA1E4D-8618-41B3-9B8D-FAA17D3B26F7`. `RuleAttackRoll.OnTrigger`
  (token `0x06007173`) calls private
  `TryOvercomeTargetConcealmentAndMissChance` (`0x06007174`) before AC, attack,
  critical, Mirror Image and final sneak-attack cleanup. A false return alone
  assigns `AttackResult` value 8.
- That private method first handles the independent `MissChance`/D100 path,
  then creates and triggers `RuleConcealmentCheck(initiator,target,true)`, stores
  it in `RuleAttackRoll.ConcealmentCheck`, and reads its public `Success` getter.
  `RuleConcealmentCheck.OnTrigger` preserves exact concealment, 20/50 value,
  D100 roll and any native reroll. `Success` is solely `None || Roll > Value`.
- `RuleCheckTargetFlatFooted.OnTrigger` independently calls
  `UnitPartConcealment.Calculate(..., false)` and applies its own
  `IgnoreConcealment`; `UnitPartConcealment.Calculate` also owns invisibility,
  blindness, weather, range and unit-part semantics. Therefore patching either
  global seam would violate the amendment. The selected postfix changes only a
  failed `RuleConcealmentCheck.Success` read when the current parent
  `RuleAttackRoll` stores that exact check and its exact ranged weapon carries
  one valid project Seeking enchantment.
- `WeaponEnchantmentLogic` derives from exact-owner
  `ItemEnchantmentLogic<ItemEntityWeapon>`. The project blueprint contains one
  inert marker; the resolver rejects no/duplicate markers, duplicate runtime
  enchantments, foreign marker carriers, non-ranged items, missing configuration
  and all exceptions. It never infers from name, family, wielder or inventory.
- Added stable active Seeking identity
  `036fc59fd1e24753b98f9d92cdb1e93e`; the other nine final identities are
  append-only reserved until their catalog transaction. Current bootstrap is
  233 active registrations and the ledger is 243 total / 233 active.
- Five focused dependency-free policy tests plus the full suite pass: 935/935.
  Repository validation, clean exact-reference Release, build-output, authentic
  SoundBank, package creation and strict package validation pass at 0.0.73.
  The first sandboxed full run reproduced only the inherited audio
  `File.Replace` denial; the authorized exact rerun passed.

### Exact next action

Commit and publish this source checkpoint, then run guarded save-free bootstrap
and blueprint-contract observation on its exact clean SHA. Curate the run ID,
then implement the remaining nine-blueprint magic-firearm catalog and shared
Reliable threshold service; do not treat this scoped Seeking checkpoint as
mission completion.

- Published source `2ba3866a991df9107209a93dd7369d5072cc0cd7` passed the
  guarded save-free observer. Evidence directory
  `20260808T1839549682548Z-observe-vendor-table-contracts`, runtime ID
  `20260808T1839549954807Z-f994425a76d245288d0c0bec7b29e2b6`, status PASS,
  loaded 0.0.73 and exact Git source, duration 116,410 ms. The invoking command
  host reached its 60-second wait limit while the independently guarded process
  continued; the authenticated final structured result is authoritative and
  Kingmaker exited normally. No save/UI/inventory mutation occurred.

### Revised exact next action

Implement the bounded Reliable enchantment and eight-item magic-firearm catalog
as the remaining nine active registrations, including the shared effective
misfire threshold service and canonical-family validation. Then re-run source
gates and guarded blueprint contracts before vendor/loot publication.
## 2026-08-08 Reliable and magic-firearm source checkpoint

- Activated the original nine reserved Rare Firearms identities, producing the
  amended ten-blueprint feature set and a 242-active/243-ledger manifest.
- Registered project Reliable with one exact reduction-1 marker and built eight
  isolated canonical-family item clones with exact names, prices, weights, native
  enhancement/property references, descriptions, and late-applied family icons.
- Consolidated ordinary fire, Scatter Shot, and Dead Shot behind one effective
  misfire threshold service. Base definitions remain 1..20; effective thresholds
  are 0..20, Reliable applies after Broken/training adjustments, and threshold 0
  retains the native natural-1 miss without a misfire.
- Source validation, 935/935 domain tests, exact-reference Release compilation,
  build-output validation, SoundBank validation, package creation, and strict
  package validation passed. Intermediate package SHA-256 is
  `041a582f364ccfe4ec26c7d0733068075e909ac3d6dc5ef59661c6f0e08225a5` and DLL
  SHA-256 is `caab7e79ae3bc5fbe9ce02e65c00e7dd7b265d5fb30411a79ec689b13f6e42d6`.
- The guarded bootstrap observer correctly refused the dirty source state. The
  next action is to publish this coherent passing source checkpoint, then run the
  save-free live blueprint observer from its clean exact source SHA.
## 2026-08-08 acquisition publication source checkpoint

- Published checkpoint `3c412a0eab9dbb17446a3f7184553005fbd1d005` passed the
  guarded save-free live bootstrap observer at evidence directory
  `20260808T1853527250733Z-observe-vendor-table-contracts`.
- Replaced Jhod-oriented testing stock with exact `SmithVendorTable`
  (`7de959347266092448d8a72089ef9778`) early-firearm/+1 stock and supplies;
  Advanced Rifle/Revolver and named uniques are excluded. BTSL tables receive
  the same permitted early roster and continue to skip absent DLC tables.
- Added transactional count-one fixed publication for the five accepted exact
  `BlueprintLoot` targets, with exact target name/area validation, normalization
  limited to the five project uniques, idempotence, and exact snapshot rollback.
- Repository validation, 935/935 tests, exact-reference Release build, build
  output, SoundBank, and strict package validation pass. Intermediate package /
  DLL hashes are `9c977288fa2904776364129198dbff468c11f0b4829a2f898992e274c47e8045` /
  `88a4caa4328ea00f2b7f9492e6e59721d84d4bc88682d94fac9aca13b63d4df4`.
- Next: publish this coherent source checkpoint and run the guarded live graph
  observer to validate all selected loot target names/types/areas and vendor
  bootstrap mutation against the installed graph.
## 2026-08-08 acquisition observer repair

- Live run `20260808T1859583851736Z-63e8ceb97df44e3ea87c596e66ec0914`
  loaded exact source `61f05e6fad00a53fb7eae0fec925e9b88c4e3365` and
  successfully bootstrapped the acquisition mutations, but the inherited vendor
  observer failed because it still asserted ten Jhod-era entries and the former
  Advanced Rifle/Revolver BTSL roster. This was an observer expectation defect,
  not a publication/bootstrap failure.
- Updated the observer to the eleven-entry Smith/BTSL early/+1 roster and added
  the typed `observe-rare-firearm-acquisition` scenario. It now asserts modern
  and named-item vendor exclusion, zero current Jhod firearm publication, and
  all five exact count-one loot GUID/name/area/item relationships.
- The repaired observer source passes repository validation, 935/935 tests,
  exact-reference Release, SoundBank, and strict package gates. Next action:
  publish and run `observe-rare-firearm-acquisition` from the clean SHA.
## 2026-08-08 acquisition observer narrowing

- Run `20260808T1905352086991Z-377c9b015ad1478d9f6f2a9a0e96f430`
  proved 11/11 exact Smith entries, 44/44 BTSL entries, zero modern/named managed
  vendor entries, zero Jhod project firearms, and 5/5 exact fixed-loot targets.
  Its sole failure was the observer's inherited fixed-entry total of 61; the
  selected Smith table has 16 native entries plus 11 project entries, exactly 27.
- Narrowed that presentation-contract assertion to the observed exact 27 without
  changing publication mechanics. Next action: publish and repeat the dedicated
  acquisition observer.
## 2026-08-08 human acceptance surface checkpoint

- Acquisition observer PASS evidence directory:
  `20260808T1908322207263Z-observe-rare-firearm-acquisition`; exact source
  `01b189b45b521ae97ceea4cf25216fcf59391ecc`.
- Added a development-only Rare Firearm Acceptance panel: complete catalog report,
  one exact selected-item spawn, one-copy eight-item set spawn, and a read-only
  acquisition/current-area audit. It grants no proficiency/ammunition, never runs
  automatically, performs no cleanup-by-blueprint, and makes no unproven live
  entity, coordinate, highlight, or teleport claim.
- Expanded the manual checklist with disposable/pre-entry save guidance, all exact
  targets, capital merchant stock, stale instantiated-container distinction, and
  the shortest combat/presentation checks. No authoritative local Bag of Tricks
  `tp2loc_*` command was proven, so none is guessed.
- All 935 tests and source/build/package gates pass. Next: publish this panel/docs
  checkpoint, then implement focused magic-firearm blueprint, Reliable, Seeking,
  lifecycle, native-property, and Thundering guarded combat scenarios.
## 2026-08-08 blueprint/state observer checkpoint

- Added typed guarded `observe-rare-firearm-blueprint-contracts` coverage for all
  eight exact blueprint/runtime identities, static enchantments, prices/weights,
  Reliable exact-item thresholds, Seeking exact-item authorization, and Last Word
  static enchantment survival across Loaded and Broken state-token replacement.
- Host and mod allowlists plus preflight expectations were extended without
  weakening request validation. Source validation, 935/935 tests, exact-reference
  Release, SoundBank, and strict package validation pass. Next: publish and run
  the clean-SHA observer, then build deterministic live combat scenarios.
## 2026-08-08 — rare blueprint and token observer PASS

- Published source: `2fb5391ba27edfe67d032812ea94129ce6dc6086`.
- Guarded scenario: `observe-rare-firearm-blueprint-contracts`.
- Run ID: `20260808T1915428002243Z-99c2bd30714c4647a7d91e40b494a6b0`.
- Evidence directory: `20260808T1915427845823Z-observe-rare-firearm-blueprint-contracts`.
- Result: PASS at loaded version `0.0.73` with exact source identity.
- All eight live blueprint/item pairs matched GUID, canonical family, exact
  price, family weight, and static-enchantment count. The Last Word authorized
  Seeking while the +1 Pistol control did not; Duelist's Rebuttal supplied the
  one-point Reliable reduction while the control supplied none.
- The shared effective threshold service returned 0 for a normal Reliable
  Pistol and 2 for a trained Broken Reliable Pistol. The Last Word retained all
  three static enchantments before, during Loaded, and during Broken state-token
  replacement (`3,3,3`).
## 2026-08-08 — deterministic Seeking combat PASS

- Final source for this phase: `2ed81a147e709d13c450dc793b2082a633ea159a`.
- Guarded run: `20260808T1943391642314Z-5bde100e871d487cbd1c5357ebeb9bbe`;
  evidence directory `20260808T1943391283437Z-magic-firearm-native-properties`.
- Initial strategy using `AutoHit` was rejected because installed
  `RuleAttackRoll` skipped concealment-check construction. Ordinary attacks then
  proved control `Concealment` versus Seeking advancement, but exposed that the
  loaded optional-mod composition bypasses `RuleAttackRoll.set_Roll` while still
  calling exact `IsSuccessRoll(d20)`; the old adapter therefore failed closed.
- The bounded repair records the exact eligible attack's native `d20` at
  `IsSuccessRoll` only when setter observation is absent and not faulted. It does
  not patch global dice, infer an item, or apply a queued forced roll after native
  success computation. Existing setter observation remains preferred.
- Final PASS used native Blur `dd3ad347240624d46a11a092b4dd4674` and forced
  concealment percentile 1 for both attacks. The +1 control ended Concealment;
  The Last Word retained Partial concealment, used deterministic natural 19,
  hit AC 14, discharged exactly once, evaluated threshold 0 as ordinary, and
  produced no misfire or cleanup fault.
## 2026-08-08 — Reliable direct-fire matrix PASS

- Published source: `4b0bcc6ea8ff0fa2c50ebac70f4483d559122e81`.
- Run ID: `20260808T1948114670922Z-dddc0f3024424ac7bb628fdd7e02e080`;
  evidence directory `20260808T1948114394015Z-reliable-firearm-misfire-matrix`.
- Reliable Pistol threshold 0 plus natural 1 produced native Miss and retained
  Normal condition. Reliable Musket roll 1 produced misfire/Broken; its roll 2
  control produced Hit/Normal. Mundane Musket roll 2 produced misfire/Broken.
- The shared Broken/training/Reliable order returned trained/untrained Pistol
  2/4 and Musket 3/5. Diagnostics: 4 eligible, 4 natural rolls, 2 ordinary,
  2 misfires, 2 Normal-to-Broken, 0 duplicate callbacks, 0 no-rolls, 0 faults.
