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

Read-only inventory run `20260807T1740349239015Z` classified all 12 immediate
children. The raw full manifest remains ignored under
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
- `KingmakerToggleCustomSoundpacksMod-master` declares the same UMM identity
  and version as the compiled root, but no supplied build output proves byte
  identity.
- `OwlcatKingmakerModCraftMagicItems-master` declares CraftMagicItems 1.10.0
  and contains source/project files but no DLL; disposition is
  `STATIC-AUDITED-ONLY` unless another exact local loadable root is discovered.

Eddic Respec and Bag of Tricks are absent and are explicitly
`UNAVAILABLE-LOCAL-REFERENCE`.

## Static overlap conclusions

Pending deterministic scan and curated review.
