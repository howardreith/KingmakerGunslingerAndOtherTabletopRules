# Eastern Weapons / Favored Class / Tweak or Treat investigation

## Evidence boundary

Investigation began from clean `codex/bodyguard-in-harms-way` commit
`a736e25ed929ed6ba190dac23b977528d1521627` on 2026-08-22. The supported
game is Kingmaker 2.1.7b. `Assembly-CSharp.dll` has SHA-256
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`
and MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`.

The live mod directories were copied read-only into the repository's ignored
compatibility-reference root with per-file SHA-256 verification. No live
third-party file was edited and no copied file is packaged or committed.
Public source is intent/naming evidence only; the exact installed DLLs,
decompiled IL, blueprint graph, settings, hashes, MVIDs, and guarded runtime
behavior are authoritative.

## Exact installed artifacts

### Call of the Wild

- UMM ID/version/entry: `CallOfTheWild` / `1.14.4c-2.1` /
  `CallOfTheWild.Main.Load`
- DLL SHA-256: `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915`
- DLL MVID: `8caab254-aacf-4811-8093-44b9184e6e53`
- `Info.json`: `32c0bc48c26eb22787e99fb1eb86d074df2cd7dcfe4804ce8eb381a3a589d44d`
- `settings.json`: `24cc3f80269992a53ebbfd1f5986e5aab056841d6b2f43d8e22e764cdb73f6e8`
  (`balance_fixes=true`)
- `blueprints.txt`: `f227b1c302dc8db9773de483369407ecc4a154b4082d83c97fcfe0c65912a4f4`
- `loaded_blueprints.txt`: `55db70b95cd666530ddc803a5c43d8bf9e996d38ad88bdc6b25da790bf3401c7`
- complete 266-file tree fingerprint:
  `0b1210e8aae8ae514a3557948c4cdc865a0a99104529662f10932c08e510445c`
- source reference: Holic75/KingmakerRebalance
  `1332fb0db844b7863f484ca978bea2349fe49769`

### Favored Class

- UMM ID/version/entry: `ZFavoredClass` / `1.3.1` /
  `ZFavoredClass.Main.Load`
- DLL SHA-256: `dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c`
- DLL MVID: `3efd38e7-8682-4b4d-8d53-e368a3664919`
- `Info.json`: `c7288e37b8a4a588e6cbacc43c4b0f64e3d82b397d873e73ec08af405595fd20`
- `settings.json`: `bdceed77d2bf4a31dd9e4eeb64ef9d55a42ef59d23f46abcb1ddbcc6ef66754b`
  (`enable_traits=true`, `deity_for_everyone=true`)
- `blueprints.txt`: `742bcdeb36c66133aa39bbedbd1c7a95aeccfd9c86e556eaf83148c0ade8f58d`
- `loaded_blueprints.txt`: `92993ecfcf66b4e77f00035fbc505435dd85506760da4c9ecf7a9ba4d3fc8bcb`
- complete 24-file tree fingerprint:
  `3e3e943670bc72a4198fc8431983b3cb4ea64fc7e3c3bdcad918de02379585a8`
- source reference: Holic75/KingmakerFavoredClass
  `56ec6c5fd34f0da037350f951383ca7f1a0c5e57`. That checkout labels itself
  1.3.2b and is not asserted byte-equivalent to installed 1.3.1.

### Tweak or Treat

- UMM ID/version/entry: `TweakOrTreat` / `1.1.0` /
  `TweakOrTreat.Main.Load`
- declared requirements: `CallOfTheWild`, `RacesUnleashed`; declared load-after:
  `CallOfTheWild`, `RacesUnleashed`, `DerringDo`
- DLL SHA-256: `a518324e15632aba46d6c467b156a31e9afd282e9827dee3e79ad14673852b92`
- DLL MVID: `56f6c205-0ccb-47a7-b1d6-f000ff290b68`
- `Info.json`: `139c1d6c79d6aa6adac8b725c50cbc3a9ad21dc3024ff2ce7f636ada7b04cd59`
- `settings.json`: `dd0cdb89922ec129100bd840bc06c6409e6411c1929b34ddd4cf8d6136b8b10d`
- `blueprints.txt`: `e3a5538197401703fa6115c6ab2f059f8289416f238744c6d7f2a9fc97f195e3`
- `loaded_blueprints.txt`: `96e1378211307caa84afcc5ece61823818695a2a998f66ed2a58944160f123a0`
- complete 22-file tree fingerprint:
  `d77893f8c46a1f85bac8a4442a9565194b0c94dd8db1a367f57903ece698024e`

### Races Unleashed

- UMM ID/version/entry: `RacesUnleashed` / `1.0.11` /
  `RacesUnleashed.Main.Load`
- DLL SHA-256: `6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0`
- DLL MVID: `e9b9acb5-9b3f-41ad-bbd7-74494d5d7680`
- `Info.json`: `d09c3f06c529c7365130fcc912882371740f30803f56081c6ac00a2af45eb01f`
- `settings.json`: `270899c3f6c3d29bfe777fc2b55a0bb0404ae50786dbac8f1dccf77e8b9cabbf`
  (`SelectableGanziOddity=false`)
- complete 362-file tree fingerprint:
  `fc60dd21babca8d3981833ebd8acceeb0baf37e49785ac4db93d1ccfb53717e4`

## Exact failure chain

KMG's stable Nodachi category is decimal `4934986`, hexadecimal `0x004b4d4a`.
It is a runtime-added `WeaponCategory`, so its native `ToString()` is
`"4934986"`.

Installed Favored Class 1.3.1 `ZFavoredClass.Traits.createEquipmentTraits()`
has IL size 1418 bytes. It reads `AddProficiencies.WeaponProficiencies` from:

- native Martial Weapon Proficiency
  `203992ef5b35c864390b4e4a1e200629`; and
- native Simple Weapon Proficiency
  `e70ecf1ed95ca2f40b754f1adb22bbdd`.

It concatenates/distincts those arrays, loops each category, calls
`WeaponCategory.ToString()`, appends `HeirloomWeaponTraitSelection`, and calls
Call of the Wild's `Helpers.CreateFeatureSelection`. CotW's closed GuidStorage
has no entry for a dynamically derived foreign name. When KMG 0.0.92 appended
Nodachi to the native Martial array inside KMG's earlier LoadDictionary
postfix, Favored Class derived `4934986HeirloomWeaponTraitSelection` and threw
`Missing AssetId`.

Installed `ZFavoredClass.Main.LibraryScriptableObject_LoadDictionary_Patch`
uses Harmony 1.2 and declares only `HarmonyAfter("RacesUnleashed")`. It calls
`Traits.load(settings.enable_traits)` inside its own catch. `Traits.load`
invokes `createEquipmentTraits()` before it creates both top-level Trait
selections and before it logs `Enabling Traits.`. Consequently UMM may display
Favored Class green even though all player trait publication was skipped.

Installed Tweak or Treat has a second LoadDictionary postfix with priority 200
and Harmony-after `RacesUnleashed`, `DerringDo`, and `ZFavoredClass`. Its exact
static `TweakOrTreat.HeirloomWeapon.load()` method reads Favored Class Equipment
Traits (`af37d78d7bc5451d943b63356f438949`), replaces each installed
`ZFavoredClass.NewMechanics.PrerequisiteRace` with an exact native
`PrerequisiteFeature` from its weapon-familiarity map, and expects Favored
Class's complete Heirloom catalog. A partial Favored graph therefore also
breaks this intended downstream integration.

## Selected repair

KMG continues to register all custom weapon and save-stable trait identities
during its normal blueprint transaction. The early
`EasternWeaponSelectorPublication` now owns only KMG selector publication; it
cannot inspect or mutate broad Martial proficiency arrays.

`EasternWeaponMartialPublication` is a distinct transaction invoked from the
first UMM update, after the entire LoadDictionary postfix chain has returned.
It derives the native Martial authority from the exact native feature, finds
every feature whose `AddProficiencies` grant contains that full authority,
clones only those components, appends Nodachi exactly once, validates every
result, and then configures `EasternWeaponProficiencyRuntime`. It records exact
original component-array references and restores them all on failure.
Repeated reconciliation validates rather than appending again. This retains
standalone behavior and every legitimate broad-Martial owner's Nodachi
proficiency without exposing the custom enum to foreign trait builders.

The same late coordinator observes, but never invokes or patches, exact
`TweakOrTreat.HeirloomWeapon.load()`. If Favored Class traits are enabled and
the Eastern Weapons module is active, KMG transactionally appends one
save-stable `Heirloom Weapon: Nodachi` selection to the completed foreign
Equipment Trait `AllFeatures` array. It has the installed three choices:

1. Nodachi proficiency, requiring nonproficiency;
2. +1 trait bonus on Nodachi attacks of opportunity, requiring proficiency;
3. +2 trait bonus on combat maneuvers while wielding a Nodachi, requiring
   proficiency.

The parent grants KMG's masterwork Nodachi and uses `FeatureGroup.Trait`.
Five KMG-owned identities (parent, three choices, hidden CMB carrier) are
registered regardless of optional-mod presence/settings for old-save safety.
The implementation independently uses native KMG components and no third-party
code, asset, or compile reference. A GUID/reference conflict, incomplete
Favored graph, incomplete Tweak reconciliation, traits-disabled state, or
module-off state leaves the foreign selection untouched. Optional Heirloom
publication failure does not roll back the essential late Martial repair or
disable unrelated KMG modules.

## External identities and complete-trait gate

- Combat Traits: `43d763957f364315b5fff85f9e91ca51`
- Race Traits: `331ed3c4a988415785f71a37b826d0f1`
- Equipment Traits: `af37d78d7bc5451d943b63356f438949`
- first Trait choice: `34e2812e0f8241bb9e1bee5240c9eb2e`
- second Trait choice: `5253dcee502a49249bdd8bfdfe525e9f`
- Adopted: `987e573c15e241c285e0fa1d5ac0a0a2`
- Additional Traits: `6a1f65b204a74c22b0f47e1e2c808441`
- Favored Class halfling Helpful:
  `c9bd9f6cc24f41e684a68e6510afc726`

KMG now requires Equipment Traits, at least twenty structurally valid
three-choice Heirloom selections, and both top-level routes in addition to the
previous Combat/Race/Helpful/Adopted/Additional checks before treating Favored
Class as compatible. This rejects the exact partial-initialization failure.

## Rejected repairs

- Adding a CotW GuidStorage entry for a dynamically derived foreign name would
  couple KMG to foreign internals and would not generalize safely.
- Swallowing the Favored exception would retain a green UMM entry with no
  traits.
- Disabling Equipment Traits, deleting Nodachi, or removing its Martial status
  changes player content to hide the lifecycle defect.
- Delaying all KMG blueprint registration could prevent other mods from seeing
  KMG classes/identities and break unrelated optional integrations.
- Editing third-party settings, ledgers, binaries, or source violates package
  and restoration boundaries.
- Running KMG before/after a named foreign Harmony patch is insufficient because
  the repair must work when either optional mod is absent and across UMM load
  order; the first-update phase is the common post-LoadDictionary boundary.

## Runtime qualification

Guarded profile run IDs, package identity, exact pre/post restoration hashes,
and mechanical outcomes are appended during 0.0.93 qualification. Runtime
proof must include the zero-before/one-after Nodachi array observation,
`Enabling Traits`, complete top-level/Combat/Race/Equipment/Additional graph,
one Helpful per correct category, Tweak or Treat's completed racial Heirloom
transformations, one KMG Nodachi Heirloom choice where eligible, and the shared
ordinary Aid Another/Bodyguard grant matrix.
