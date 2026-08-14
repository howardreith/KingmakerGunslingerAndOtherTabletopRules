# Eastern Weapons qualification

This is the cumulative qualification ledger for release `0.0.80-eastern-weapons`.
PASS is recorded only for an observed completed surface; pending work remains
explicit.

## Current qualified candidate

- Repository validation: PASS.
- Dependency-free domain/reflection suite: PASS, `1047/1047`.
- Clean exact-reference Release build and build-output validation: PASS.
- Strict standalone package validation: PASS.
- Package: `KingmakerGunslinger-0.0.80-eastern-weapons.zip`.
- Functional/artifact source commit:
  `9966edfa160ed4d898482f754a6b8abf1f9ebc11`.
- DLL SHA-256: `B8586B620413F0C0442B60FC0911395550C6B049AD8FF01F78B93A10B962B37D`.
- DLL MVID: `13ba0899-9970-4403-ae00-e6ac32ffe473`.
- Package SHA-256: `0FEAE10BA8DC5941C2C536C2AA3AF7C2BFECA2C2B1B5EBFF1B0300A3DF0DEF0C`.
- Eastern bundle SHA-256:
  `39884FF681EE553DE957E36E01B350AB926A452F994C4E8D33015D57D4EAD1EC`.

## Runtime results

- Expanded Eastern combat: PASS,
  `20260814T1715447972862Z-9c6f32cdb16c41df9de32c4108c9f79c`.
- Focused Eastern combat: PASS,
  `20260814T1513175535242Z-6d95393a15c44b96a168cb21132fee19`.
- Elven Branched Spear combat regression: PASS,
  `20260814T1515370965718Z-017e199cb77b4af891868cec2d3a840b`.
- Vendor/loot observer, module ON: PASS,
  `20260814T1531432806171Z-d6638ced8af7472fabeb9b65f2c233c7`.
- Vendor/loot observer, Eastern module OFF: PASS,
  `20260814T1534024888613Z-29a2ce31f1db4aa7bedec9c2c14e6047`.
- Working-save prepare: PASS,
  `20260814T1546412271086Z-ec58de23ddad45c4b9c57c2594065f28`.
- Module-disabled working-save verification and cleanup: PASS,
  `20260814T1551228284204Z-e4d26f6cabb740c2a9dbbacb4785055e`.
- Final working-save absence verification: PASS,
  `20260814T1554167770731Z-9695a053916645e3b3cc4a44ceae8c29`.
- Call of the Wild optional observer/module/combat profile: PASS, transaction
  `compat-20260814T160106Z-f8e933764054` with exact restoration.
- Arms and Armor grip-dependent combat after narrow compatibility repair: PASS,
  `20260814T1626264154920Z-659ee31c63844b15a53f60366ffd55d6`,
  transaction `compat-20260814T162536Z-edb4ba5e8032` with exact restoration.
- Toggle Custom Soundpacks negative control: PASS, observer
  `20260814T1631281716106Z-f2c9d4b1204e4cfda6676e1195b682b9` and combat
  `20260814T1633141740745Z-18723a67c397410783a5ffc276b2b280`, transaction
  `compat-20260814T163039Z-4e78b724108d` with exact restoration.
- Maximum currently qualified combined profile (Arms and Armor plus Toggle
  Custom Soundpacks): PASS. Optional-mod observer
  `20260814T1635283183758Z-21349748fc914622a1aa4b6d6d2075c2`, six-module
  observer `20260814T1636409575327Z-104bf7b4dfcd4bc38f1f6a924103fe6f`, and
  combat retry `20260814T1641090730695Z-2f410c5cf34e4afda160f266fd9024a4`
  all passed. Transactions `compat-20260814T163437Z-2f088fd5a184` and
  `compat-20260814T164018Z-24ec1f9abdbb` restored exactly.
- Loaded identity matches the built and installed DLL: PASS.
- Mods-tree restoration for every compatibility transaction: PASS.
- Save interaction: exactly two correlated writes to `KMG_AUTOMATION_WORKING`;
  the protected baseline remained distinct and untouched.

The expanded Eastern run proves exact family/catalog identity, corrected static
proficiency names and deterministic merged ordering; singular WK/KA/NO rows in
all seven approved generic selectors; Grace exclusions; the complete
Wakizashi/Katana/Nodachi proficiency matrix; native Light Blades, Heavy Blades,
and Polearms training without doubled Nodachi training; native Wakizashi
finesse across all ten family items without double Dexterity; exact enchantment
arrays for all eighteen named weapons; all five bespoke effects with positive
and negative controls; main-hand, offhand, repeated, Haste, and switching Speed
attack counts; native Brilliant Energy living/undead behavior; and exact
fixture cleanup.

## Final seal

- Complete 64-state fresh-launch module matrix: PASS. All 64 rows were unique,
  used commit `9966edfa160ed4d898482f754a6b8abf1f9ebc11`, version
  `0.0.80-eastern-weapons`, DLL SHA-256
  `B8586B620413F0C0442B60FC0911395550C6B049AD8FF01F78B93A10B962B37D`,
  MVID `13ba0899-9970-4403-ae00-e6ac32ffe473`, and restored settings to
  SHA-256 `2E53FA0A09C56662434F6EA548FF5EBCF91F5AAF293D668248221239A1308655`.
  First run: `20260814T1737485270891Z-dd22caf63606447f8368d22eacaa1b50`;
  last run: `20260814T2009427225332Z-54c3d2eeec7143fabadd3e917f4787cd`.
- Final Call of the Wild transaction
  `compat-20260814T201218Z-6b342ec27bb6`: PASS, optional/module/combat runs
  `20260814T2013232970413Z-f2324298cc5f4675959e08a43a751893`,
  `20260814T2015470027632Z-71dbd6469cf641a98232374f0fb2af84`, and
  `20260814T2018083297181Z-7ebbd6209e9d48288fef67ad8142baf9`.
- Final Arms and Armor transaction
  `compat-20260814T202007Z-01aa6b79327e`: PASS, optional/module/combat runs
  `20260814T2020585890813Z-8dba02841c8641e1998f9a92bde38fc2`,
  `20260814T2022466488993Z-770ce3ea86784697a0377f6ad486209b`, and
  `20260814T2024324375423Z-ac9e6c263c134296916cdd2524ddc14b`.
- Final Toggle Custom Soundpacks transaction
  `compat-20260814T202553Z-30cffa4906c4`: PASS, optional/combat runs
  `20260814T2026432084296Z-364cdc40e3a74c32963e738f5d91fb3d` and
  `20260814T2028305992456Z-32cb19aab02e47f9aec921dcd63fd031`.
- Final maximum-combined transaction
  `compat-20260814T202953Z-d5d917889541`: PASS, optional/module/combat runs
  `20260814T2030427202274Z-d1aa41d6a7364f01855dceac7c13daa2`,
  `20260814T2032292632857Z-1ad250973e0944e097c5d6f09a07dc67`, and
  `20260814T2034139757084Z-d6259b3e22e140968cd79144ca36358c`.
  Every final transaction restored the complete Mods tree exactly.
- Final expanded save-free combat: PASS, run
  `20260814T2047053937321Z-ed49c4c646a140609e6b84d52900adc9`, all 18
  assertions, exact artifact source and loaded DLL identity, no save access.
- Canonical non-mutating working-save smoke: PASS, run
  `20260814T2051106519178Z-a9571d847397477b893818601c4a00ab`.
  The exact `KMG_AUTOMATION_WORKING` descriptor and fingerprint correlated;
  no save-writing API was observed.
- Built, packaged, installed, and runtime-loaded DLL identity comparison:
  PASS, all SHA-256
  `B8586B620413F0C0442B60FC0911395550C6B049AD8FF01F78B93A10B962B37D`.
- Human subjective visual acceptance remains **PENDING HUMAN REVIEW** and is
  not represented as an automated PASS.

## Expanded combat iteration

The expanded save-free candidate passed repository validation, all `1047/1047`
domain/reflection tests, clean exact-reference Release build, build-output
validation, strict deterministic package validation, and guarded live runtime.
Run `20260814T1715447972862Z-9c6f32cdb16c41df9de32c4108c9f79c`
in evidence directory
`20260814T1715447806219Z-disposable-eastern-weapons-combat` loaded DLL SHA-256
`6EBC8BF7B8339A8A65A23354D2EBA467E804210F64464502E579C6769492BBE7`,
MVID `e6465005-9428-48ab-b646-ceffb7ed8a2b`; package SHA-256 was
`98EE14CC220722AC3DED6DF1BCF5E6DF1A60F072F064E4F5E504EE6DCE6BA78A`.
Transaction `compat-20260814T171454Z-0dc865f34ad3` restored the exact Mods
tree, and the save-free scenario performed no save interaction.

Falling Petal observed a seeded miss and a real threatened but unconfirmed
critical with zero applications, then one confirmed critical produced exactly
one +1 Dodge AC modifier; an ordinary hit did not stack it and switching away
removed it. Mountain-Sunder observed a miss consuming nothing, inactive Power
Attack applying nothing, the first active hit applying one force die, repeated
hits and weapon switching remaining at one application, and a post-marker
confirmed critical applying only one `1d6` force result. Unfixed Form observed
ordinary, changed-size, polymorph-only, and simultaneous size-plus-polymorph
states as `Medium -> Large/Large/Large`: either predicate applies one native
weapon-size step and both together still apply only one.

## Module-matrix defect and repair

The first complete-matrix attempt passed its first three fresh states, then
state `on-on-on-on-off-on` exposed a real cross-module defect. With Elven
Branched Spear publication OFF and Eastern Weapons ON, Eastern proficiency
publication required the registered-but-unpublished spear child to follow the
Elven Curve Blade and rolled blueprint initialization back. The ambiguous
timeout is retained as run
`20260814T1724329157253Z-9494ad64eec84343a1345896c0d658e8`;
settings restored exactly and no save was involved.

The selector transaction now accepts exactly two native-consistent states:
`Curve/Spear/Katana/Wakizashi` when spear publication is active, or
`Curve/Katana/Wakizashi` when it is inactive. A spear present out of the
accepted position, or more than one spear child, still fails closed. The
observer independently validates the matching expected order. Targeted repaired
run `20260814T1734002748638Z-ae0fc4a103514f6dbbf72c48fdb11bef`
passed with indices `5/-1/6/7`, all 1,513 active identities, all Eastern
selectors, commerce, assets, and presentation exact, and byte-for-byte settings
restoration. The loaded DLL SHA-256 was
`35D768BA7EA82E524427C6B88EF83907FDC0C09DBDE8E9A6627C6CAD8912BBC0`,
MVID `54d71e83-3045-4a77-9174-597088131ed0`; package SHA-256 was
`AB6AE5A8203B9D9016E0166EBEC73CC1D1FBC6788E8F97E5F1E23805E29E2A49`.
The full 64-state matrix must restart after the repair commit so every accepted
state shares one exact source identity.

## Working-save persistence

The three guarded fresh-process phases passed with exact object-reference
correlation to `KMG_AUTOMATION_WORKING` and an independently resolved, distinct
protected baseline descriptor:

- Prepare refused pre-existing fixtures, then added one instance of every one
  of the 30 item blueprints and the exact Wakizashi/Katana Exotic Weapon
  Proficiency facts to deterministic owners. Every item retained its exact
  blueprint, family type, and stable category. One correlated native save write
  occurred.
- With Eastern Weapons disabled, all 30 items and both facts deserialized
  exactly once, while public selector, campaign, commerce, loot, and custom
  presentation publication was absent. Cleanup removed only those exact items
  and facts and performed one correlated native save write.
- After settings restoration, a third fresh process observed zero feature-owned
  items and facts and performed zero save writes.

No parameterized chosen-weapon fixture was persisted. The installed generic
selectors manufacture parameterized feature instances dynamically and expose no
ordinary stable blueprint identity suitable for safe save-backed test ownership;
the mission permits omission where no safe ordinary fact construction exists.
The settings file was restored byte-for-byte to SHA-256
`2E53FA0A09C56662434F6EA548FF5EBCF91F5AAF293D668248221239A1308655`.

## Optional-mod compatibility

The exact Call of the Wild profile passed the optional-mod identity observer,
the complete six-module publication observer, and the live Eastern combat
fixture. This proves merged `AllFeatures` proficiency order, singular
parameterized categories, grip/proficiency behavior, finesse behavior, bespoke
effects, and cleanup with Call of the Wild active.

The first Arms and Armor combat run positively identified a real compatibility
defect: its hard-coded versatile-weapon classifier forced KMG Katana
one-handed. A narrow reflection-only bridge now extends the exact Arms and
Armor classification and active-slot grip methods for the exact KMG Katana type
only. The repaired run passed grip-dependent proficiency and Moonlit Crossing
positive/negative modes. Arms and Armor's authorized source contains Temple
Sword and Orc Hornbow but no eastern family provider, so no duplicate-name or
cross-proficiency bridge is needed.

Toggle Custom Soundpacks passed as the required negative control. The maximum
currently qualified combined Arms and Armor plus Toggle profile passed exact
mod identity, all-on six-module publication, the Arms and Armor Katana bridge,
and the focused Eastern combat fixture. Its first combat launch ended before
the scenario entered `OnUpdate` because Unity transiently failed to load the
present Steam API DLL; it produced no result and accessed no save. The isolated
retry passed, so the ambiguous launch is retained as environmental evidence and
is not counted as a mechanical PASS.

The current compatibility candidate passed repository validation, all
`1047/1047` tests, clean Release build, output/package validation, and loaded
DLL SHA-256
`14AF254B51979245B8046C57F770700E2ED7BED4DDBADFDB869D518CCDE7A59F`,
MVID `60409a50-c431-4150-ba55-da63727381bc`; package SHA-256
`F469C4D1C7289F2A86C2F1D70AEB6BD39CA99005098DA8FD8D1C52DB944A6445`.
