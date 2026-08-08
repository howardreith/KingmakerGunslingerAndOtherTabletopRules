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
