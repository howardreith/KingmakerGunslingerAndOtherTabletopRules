# Kingmaker Gunslinger 0.0.99

This release repairs the optional Craft Magic Items 2.1.0 ammunition interface.
The complete package is
`KingmakerGunslinger-0.0.99-craft-magic-items-ammunition-ui-repair.zip`.

CMI now retains ownership of its top-level **Mundane Crafting** selector on
every Unity IMGUI event. A capability-probed Harmony transpiler branches only
after CMI has finalized the selected crafting data and before its ordinary
equipment-only path reads `NewItemBaseIDs`. The KMG lower panel handles the
three exact 20-unit plain-`BlueprintItem` ammunition recipes and returns to
CMI's common Current Money footer.

The rejected 0.0.98 conditional whole-renderer prefix is removed. UI failures
are recursively unwrapped, logged as KMG rendering faults, and deferred to the
safe update lifecycle; the compatibility graph is never rolled back during
`OnGUI`.

All previously qualified weapon categories, five authorized production
firearms, Eastern and Elven Branched Spear upgrade behavior, the exact
firearm-only Reliable enchantment, module gates, persistence behavior, and
optional reflection boundary remain unchanged. `CraftMagicItems.dll`, CMI
source, CMI data, and CMI localization are not included.

The exact runtime authority remains Craft Magic Items 2.1.0, SHA-256
`4AE2DA61470350B31BEEF162717A604C9CCD322F66193917944EA4A9596E392D`,
MVID `0044a45b-3bca-439e-86c5-a6aa4d42855e`.

The qualified firearm SoundBank remains byte-identical at SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

CMI-generated custom items use CMI persistence and may require both mods to
remain installed. Human interaction with the real UMM ammunition interface is
still a separate acceptance gate and is not implied by automated qualification.
