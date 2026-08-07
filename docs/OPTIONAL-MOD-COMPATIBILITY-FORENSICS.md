# Optional-mod compatibility forensics

## Authority and method

The exact local bytes beneath the user-authorized examples root are the only
third-party compatibility authority. Raw paths, full manifests, and third-party
payloads remain machine-local under ignored `artifacts/compatibility`. Curated
conclusions here distinguish compiled loadable roots, source-only references,
asset-only references, heuristic source twins, and unavailable logical keys.

Static findings are not runtime qualification. Shared native lookups and shared
Harmony targets are reported but are not automatically conflicts. Exact
project-owned GUID or UMM/assembly identity collisions are critical.

## Installed contract baseline

- Game assembly: `Assembly-CSharp, Version=0.0.0.0`, MVID
  `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`, SHA-256
  `3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB`.
- UMM: `UnityModManager, Version=0.32.4.0`, MVID
  `97735e89-6c7c-4f6c-a737-187e1328fba3`, SHA-256
  `1387468BC3AF41C50FE51859A3BB7AF4922891AA8F13A6187E7A348CEAABFD88`.
- Harmony wrapper: `0Harmony12, Version=1.2.0.1`, MVID
  `918c071f-383e-46dc-a374-6879300cbe15`, SHA-256
  `AA1CD48317254985D8B700CC74953477D1B40C3022CE9AA4C95ED2B8327E1292`.

## Reference inventory

The original read-only inventory run `20260807T1740349239015Z` classified 12
immediate children. After the user supplied `KingmakerRebalance-2.1`, read-only
run `20260807T2126452736628Z` classified all 13 immediate children. The raw
full manifests remain ignored under
`artifacts/compatibility/reference-inventory`.

Compiled loadable roots:

- Arms and Armor: UMM `ArmsArmor` 1.0.10; assembly `ArmsArmor` 1.0.0.0,
  MVID `39978f0f-50f8-4c51-87f8-b1e0cb2c1095`, DLL SHA-256
  `CEC7C177819F8F68ADAC4CB24DF5834C862D0930D77305655AC3195097E33733`.
- Call of the Wild: UMM `CallOfTheWild` 1.14.4c-2.1; assembly
  `CallOfTheWild` 1.0.0.0, MVID
  `8caab254-aacf-4811-8093-44b9184e6e53`, DLL SHA-256
  `4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`.
- Toggle Custom Soundpacks: UMM `ToggleCustomSoundpacks` 1.0.1; assembly
  `ToggleCustomSoundpacks` 1.0.0.0, MVID
  `aa4a97b9-59e2-4f70-94e0-2d6a73fb2449`, DLL SHA-256
  `A2582533DFDFF82D1ECE3EC51D931D72D7C8AAC9A1302C219FCD8FCA070C9434`.
- The five `KAZ_*` children each contain a valid root Info.json and matching
  compiled assembly, so they are UMM equipment mods rather than raw asset
  folders. They remain grouped non-primary extension references and are not
  silently added to primary runtime profiles.

Source-only roots:

- `KingmakerArmsArmor-master` declares ArmsArmor 1.0.10 and is a candidate
  source twin, but no supplied build output proves byte identity.
- `KingmakerRebalance-master` declares CallOfTheWild 1.14.5, which does not
  match the compiled local 1.14.4c-2.1 version; it is related source authority,
  not an exact source twin of the runtime DLL.
- `KingmakerRebalance-2.1` is also source-only and declares CallOfTheWild
  1.15.0. Its `Helpers.cs` is byte-identical to the 1.14.5 tree, including
  class-catalog replacement logic, but it is farther from the compiled
  1.14.4c-2.1 identity and is supporting evidence only.
- `KingmakerToggleCustomSoundpacksMod-master` declares the same UMM identity
  and version as the compiled root, but no supplied build output proves byte
  identity.
- `OwlcatKingmakerModCraftMagicItems-master` declares CraftMagicItems 1.10.0
  and contains source/project files but no DLL; disposition is
  `STATIC-AUDITED-ONLY` unless another exact local loadable root is discovered.

Eddic Respec and Bag of Tricks are absent and are explicitly
`UNAVAILABLE-LOCAL-REFERENCE`.

## Static overlap conclusions

The first deterministic lexical audit scanned Gunslinger plus four source
reference trees. It reported zero cross-owner project-definition GUID
collisions and three shared Harmony targets:

- `LibraryScriptableObject.LoadDictionary`: Gunslinger, the Call of the Wild
  source tree, and Arms & Armor source tree. This is a bootstrap/load-order
  overlap requiring runtime owner/order evidence, not an automatic conflict.
- `RestController.ApplyRest`: Gunslinger Gunsmithing reset and source-only Craft
  Magic Items. Runtime qualification is unavailable for the supplied CMI bytes.
- `RuleCalculateWeaponStats.WeaponSize`: Craft Magic Items and Arms & Armor;
  this does not directly overlap a Gunslinger patch but can affect firearm item
  presentation/stat calculations if those mods act on firearm items.

Curated high-risk observations beyond exact shared targets:

- Arms & Armor patches `UnitViewHandSlotData.OwnerWeaponScale`, several weapon
  and hand-slot contracts, and creates `EquipmentOffsets` for its own weapon
  types. Gunslinger dynamically targets `UnitViewHandSlotData.ReattachSheath`.
  These are adjacent equipment lifecycle risks even though the exact methods
  differ.
- Craft Magic Items reads and rewrites item enchantment collections, resolves
  custom blueprints, clones item-related objects, and patches weapon-size and
  rest flows. The supplied reference is source-only, so these remain static
  risks rather than observed Gunslinger firearm-state defects.
- Toggle Custom Soundpacks patches `AkBankHandle.LoadBank` and `LoadBankAsync`
  and reads `SoundBanksManager` state. It has no observed `KMG_Firearms.bnk`
  literal collision, but runtime coexistence must verify the Gunslinger bank
  remains healthy and discharge remains exactly once.

The scanner is intentionally lexical and labels its findings accordingly. It
does not claim compiled-only targets, overload signatures, dynamically selected
targets, or runtime patch order until exact Harmony12 introspection is added.

## Exact Call of the Wild class-catalog IL

Read-only reflection over exact compiled `CallOfTheWild.dll` SHA-256
`4EBF8E1ED3E66FFED72EA33EA325595629423DACD5BFFA23E3C9109144B26915`,
MVID `8caab254-aacf-4811-8093-44b9184e6e53`, proves
`CallOfTheWild.Helpers.RegisterClass` reads and replaces the array reached via
`CallOfTheWild.Main.library.Root.Progression.CharacterClasses`.

Read-only reflection over exact installed Kingmaker 2.1.7b proves both
`CharBPhaseClass.get_m_ClassesCollection` and
`CharBPhaseClassInChargen.get_m_ClassesCollection` are identical 21-byte IL
getters reading `Game.Instance.BlueprintRoot.Progression.CharacterClasses`.
Gunslinger currently publishes through static `BlueprintRoot.Instance`.
Runtime snapshots must prove whether these roots diverge before repair.
