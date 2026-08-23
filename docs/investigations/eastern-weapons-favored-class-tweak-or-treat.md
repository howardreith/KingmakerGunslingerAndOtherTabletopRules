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

Qualification used the immutable 0.0.93 candidate built from
`01d7932ab20c3d8837aaa05c4a5ccd1a5eed55c3`:

- package: `KingmakerGunslinger-0.0.93-bodyguard-in-harms-way.zip`;
- size: 22,533,452 bytes;
- package SHA-256:
  `0f9d12f16c9e95848ecb54b74b3d57342e41c6915581cf06ab06f8b92118df85`;
- DLL SHA-256:
  `b9d2e035e9231e15d3d0b07d13f04935a665aba3682ac6e37c4abf215de0f4eb`;
- DLL MVID: `3a6f2729-3d96-4cc0-bf93-182d57f7cc1f`;
- blueprint ledger SHA-256:
  `87aa30180a51f22e4095af4756844c47612546570748d265eb950d00545b4f25`;
- build-local manifest SHA-256:
  `eff6d656fb050491ffbfff2bb0e6b280348fb35c19fd2f5d04ec98eddd92160c`.

The pre-change Release baseline was 1,201 passing tests. The final Release
suite passed 1,211 tests. Repository validation, blueprint/manifest validation,
the clean Release build, strict package validation, deployment tests, and all
136 runtime-preflight assertions passed. The exact package was installed in the
live UMM `KingmakerGunslinger` folder through the guarded deployment flow.

### Guarded profile results

All launches used Steam App ID 640820 and save-free request-local fixtures
unless explicitly identified as the final working-save smoke. Each profile
transaction reported `restorationVerified=True`. The canonical serialized
original Mods manifest contained 922 entries and had SHA-256
`8d62e9ec2a7d496b5319c9eb547d7739c987bed0ddf4ec32e61ab86910e9768a`;
this hashes the exact compact JSON manifest used by the transaction, including
each file's length, timestamp, and SHA-256.

| Profile | Transaction | Guarded PASS evidence |
| --- | --- | --- |
| A: KMG standalone | `compat-20260823T045511Z-20dcb55c0104` | observer `20260823T0455257976466Z-22e3c5cbaefd4ecdbf6f20891f1c0bfd`; combat `20260823T0456325136638Z-21b91cbc5b6a494397b71d4c67562fcc` |
| B: KMG + CotW | `compat-20260823T045750Z-cd7c32d9173e` | observer `20260823T0458100339328Z-eb2cea31ed384677abb35322ab3ad1cc`; combat `20260823T0459548950689Z-404c74d71f724b1daaec4085812b26af` |
| C: CotW + Favored, traits enabled | `compat-20260823T050153Z-2f0d791d95bd` | load `20260823T0502142194889Z-b02227a157284b969a5b4a15c829d480`; observer `20260823T0504004346135Z-f5c603db795a40a5a4f42607e1a9563a`; combat `20260823T0505482916021Z-f7a8f1556f5747079f8a1da4d6d6cfdb` |
| D: CotW + Favored, traits disabled | `compat-20260823T050747Z-b5b8c2188522` | observer `20260823T0508082675759Z-1262afee36394c7ab260c1896cd06e0d`; combat `20260823T0510104936722Z-493918c23b0348cfa4d9046603654ab4` |
| E: CotW + Favored + Tweak or Treat + Races Unleashed | `compat-20260823T051229Z-71ddbd5ee1ab` | load `20260823T0513043172983Z-4176e91ee9894ef6b35bf7b8927307ca`; observer `20260823T0515151364408Z-0cfca441cbe741cb8266b42e015ab040`; combat `20260823T0517233346444Z-dedfd252ee2a44ee8006fa7d0074ae5d` |
| F: Eastern Weapons disabled + Favored | `compat-20260823T051946Z-416b1eee0092` | module `20260823T0520127052117Z-9196671155e54a48810c55b4b7ead707`; observer `20260823T0522198334264Z-5022490891984321a60078e2fc4a67d2`; combat `20260823T0524076796428Z-70c6e08f40f543b1942c5bf79a0c1b7c` |
| G: Bodyguard disabled + Favored | `compat-20260823T052609Z-6d39e7e9fe11` | module `20260823T0526297693777Z-7f55759ad31a4766bf21d096956f7d1d`; observer `20260823T0528172953006Z-fad949e272f546f1be7901322196f681`; disabled combat `20260823T0530039096065Z-da5c397969994663ab39f00992cb50e1` |

An additional exact high-risk startup run
`20260823T0540141173227Z-4561ef57997e4aeba104e5312e5e800d`
under restored transaction `compat-20260823T053947Z-ca841bc74771` passed.
Its startup log contains `Enabling Traits`, Tweak or Treat's
`Favored class mod found`, and a successful Master Chymist data load. It has no
`Missing AssetId` occurrence. The four Favored Class `KeyNotFoundException`
messages are its pre-existing handled failures for absent unrelated custom
targets (Charmed Life, Panache, Arcane Archer, and Deadeye Devotee); none comes
from `Traits.createEquipmentTraits` or `TweakOrTreat.HeirloomWeapon.load`, and
trait construction continues immediately afterward.

### Runtime contracts proven

The exact live ordering was CotW postfix, KMG postfix, two Races Unleashed
postfixes, two Tweak or Treat postfixes, then Favored Class. Runtime evidence
recorded zero Nodachi entries while foreign builders ran, followed by exactly
one Nodachi entry in every verified broad-Martial grant on the first UMM update.
The late pass was idempotent. Profile F retained this save-compatibility grant
while suppressing Eastern selector content and the optional Nodachi Heirloom
choice.

With traits enabled, Favored Class produced both top-level Trait selections,
Combat Traits (15 choices after KMG), Race Traits (22), Equipment Traits (50
after KMG), Additional Traits, Adopted, and exactly one foreign halfling
Helpful. KMG combat Helpful appeared exactly once in Combat Traits. Both
top-level choices referenced Equipment Traits once. KMG appended exactly one
Heirloom Weapon: Nodachi choice after the 49 foreign choices. In the high-risk
profile, Tweak or Treat had already transformed all five intended racial
Heirloom choices: zero foreign race prerequisites remained and five exact native
feature prerequisites were present. No Tweak or Treat content was duplicated or
removed.

The canonical CotW rank and both ordinary buffs returned the same live matrix:

| Helper ownership | Ordinary attack / ordinary AC / canonical rank | Bodyguard |
| --- | --- | --- |
| none | `2 / 2 / 2` | `+2` |
| combat Helpful | `3 / 3 / 3` | `+3` |
| halfling Helpful | `4 / 4 / 4` | `+4` (same resolver contract) |
| both Helpful variants | `4 / 4 / 4` | `+4` (same resolver contract) |
| Benevolent-style contributor | `4 / 4 / 4` | `+4` |
| combat Helpful + Benevolent | `5 / 5 / 5` | `+5` |
| halfling Helpful + Benevolent | `6 / 6 / 6` | `+6` (same resolver contract) |
| both Helpful + Benevolent | `6 / 6 / 6` | `+6` (same resolver contract) |

The disposable combat fixture directly exercised Bodyguard `+2`, combat
Helpful `+3`, Benevolent `+4`, combat Helpful plus Benevolent `+5`, and two
protectors stacking `+2 + +3 = +5`. In each case AoO expenditure remained one
per attempt and the Aid d20/attack bonus did not change. Failed Aid spent the
AoO, contributed zero, and created no AC source. In Harm's Way after `+3` and
`+5` spent one native immediate action, moved 11 HP damage plus the saving-throw
rider to the interceptor, left the original ally at zero loss, and completed
one frame with no duplicates or leaks. Existing Shield Other isolation passed.
Profile G published neither Bodyguard feat, spent no AoO or immediate action,
created no AC source/frame/interception/log, delivered the original attack to
the original target, and left Shield Other operational.

Profile D proved that `enable_traits=false` suppresses KMG Helpful and Nodachi
Heirloom publication while stable identities and request-local existing-owner
ordinary/Bodyguard grants still resolve. Profile B proved CotW-only contributor
support; profile A proved the standalone `+2` fallback and unchanged In Harm's
Way behavior.

### Human-visible acceptance boundary

Runtime graph evidence suitable for a supervised new-character check is in
`20260823T0533311839250Z-0ef9a5073b254f5ab88474e65a307b79`.
It resolves the native Fighter level-one bonus-feat selection
`41c8486641f7d6d4283ca9dae4147a9f`; the native general feat selection is
`247a4068296e8be42890143f451b4b45`. The Favored observer independently proves
both top-level Trait choices (`34e2812e0f8241bb9e1bee5240c9eb2e` and
`5253dcee502a49249bdd8bfdfe525e9f`) and their complete category routes. Thus a
supervised Human Fighter check should enumerate five independent choice rows:
normal feat, Human bonus feat, Fighter bonus feat, Trait one, and Trait two;
Combat Traits must contain one Helpful. This is structural/mechanical evidence,
not a claim of screenshot-based visual acceptance.

Finally, guarded `working-save-smoke` run
`20260823T0544417890158Z-53e94003d66848b48930aaa9e7624dc7`
passed against the exact `KMG_AUTOMATION_WORKING` descriptor. It correlated one
catalog object through the native load path, observed a stable post-load
fingerprint, and recorded `saveWritingApiObserved=false`; the previously seen
native `Player.PostLoad` failure did not reproduce. `KMG_AUTOMATION_BASELINE`
was neither selected nor mutated.
