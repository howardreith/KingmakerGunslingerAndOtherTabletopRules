# Firearm feat presentation

Pistol, Musket and Blunderbuss use the native ornamental **P**, **M** and **B**
text route. The game supplies the font, background and selection overlays.
The implementation and its current qualification status are recorded in
[mission state](../planning/ICON-OVERHAUL-STATE.md); source implementation alone
does not establish correct native rendering or owner approval.

| Consumer | Presentation and identity contract |
|---|---|
| Weapon Focus, Greater Weapon Focus, Weapon Specialization, Greater Weapon Specialization, Improved Critical menus | `NativeFirearmFeatIntegration` appends entries with a null UI icon and the native acronym. Each keeps its existing blueprint-valued `FeatureParam`. |
| Selected native firearm feats and character-sheet entries | `FirearmNativeMonogramPresentation` patches the shared two-argument `FeatureUIData` constructor, including `UIFeature` construction. Only the five registered roots with exact supported firearm parameters receive null `Icon` and the corresponding acronym. Names, descriptions, parameters and blueprint sprites are preserved. |
| Actual selected-fact slots | The native Total list passes real `Feature` objects through `CharSComponentAbilitySlot.SetData(IUIDataProvider)`; the character sheet uses `SetFeature(Feature)`. Both read `Fact.Icon` directly. `FirearmNativeFactSlotMonogram` adapts only those exact firearm facts through native `SetIcon(null)` and the existing TMP field. Each original border/mask sequence, fact, parameter, name, rank and fallback sprite remains intact. This extension's current qualification is tracked in mission state. |
| Three Rapid Reload children | The constructor patch covers selected/sheet data. Native static menus return raw features, so the exact Rapid Reload `Items` getter adapts only its existing official child entries to native P/M/B data. Order, count, feature/parameter identity, names, descriptions and native filtering stay intact; blueprint fallback sprites remain unchanged. |
| Rapid Reload parent | The approved transparent loading emblem is loaded once through `ProjectAssetIcons`, key `rapid-reload`. |
| Gun Training | Keeps its existing `gun-training` image through `ProjectAssetIcons.Choose`. It does not share the three parameter sprites. |
| Hidden Rifle/Revolver identities | Retain existing identity and fallback presentation. The patch does not publish or restyle them. |
| Native and eastern weapon categories | Retain native getters and lettering, including Katana and Nodachi. There is no global font, icon getter or category override. |

Saved features can still render when Gunslinger publication is disabled: the
presentation filter uses registered identities, not the publication switch.
The three historical monogram PNGs remain unchanged as blueprint fallbacks until
actual consumers establish that they can be retired safely. A fallback-sprite
contact sheet cannot qualify native typography or a disk save round trip.

Rapid Reload's canonical authority is the approved pilot manifest under
`assets-source/original-icons/icon-overhaul-v2/pilot`. Its preserved 512x512 source
has SHA-256 `4f0d0ec4659bb3f5e378bf1d9dbe1fa3884a4b0b41b2463fec76cbeb670e9a3a`.
The 64x64 RGBA export and `assets/game/icons/rapid-reload.png` both have SHA-256
`5c3280145815f7b161602da161264ac1f43fb3ac3c4bf47ff907b0919a2aa9d3`.
The [catalog](../assets-source/original-icons/icon-catalog.json) delegates source,
runtime copy, installation and package checks to this authority.

The rejected previous export is preserved byte-for-byte as
`assets-source/original-icons/icon-overhaul-v2/references/rapid-reload-rejected.png`,
SHA-256 `efab95075ad8af61fe10425090015a75432b74113fbc34ebc185969e1e82b321`.
Historical manifest and pixel checks validate that archive. The current runtime
copy is independently checked against the approved pilot. Legacy generation
routes fail before writing; use `tools/icon-art/Export-IconPilot.ps1` to verify
the frozen approved export. See the [art guide](ICON-ART-GUIDE.md) and
[reference index](art/ICON-REFERENCE-INDEX.md) before creating or remapping icons.

The firearm Total-list adapter is limited to `CharBNewAbilities.FillData` containing an exact owned firearm fact. If its existing disabled `ContentSizeFitterExtended` uses `PreferredSize`, it temporarily enables that fitter with horizontal fitting unconstrained, so nested feat rows remain scrollable without driving their width. It restores the original enabled state and horizontal mode on creator hide, component disable/destruction or list refill. It never assigns a fixed height, relocates an individual row, or changes another list. Native mode, content extent and cleanup require runtime evidence.
