# Source Register

Accessed 2026-07-12 unless otherwise noted. URLs are recorded for reproducibility; the package contains no mirrored third-party source files.

## Primary technical references

1. **Pathfinder: Kingmaker — Update 2.1.7b, May 27, 2021**
   Official Steam news.
   https://store.steampowered.com/news/app/640820/view/3012324802829162085

2. **Call of the Wild / KingmakerRebalance repository**
   License, README, project structure, class precedents.
   https://github.com/Holic75/KingmakerRebalance

3. **Call of the Wild `Main.cs`, pinned revision**
   UMM entry point, Harmony12 initialization, `LibraryScriptableObject.LoadDictionary` postfix, one-time guard, blueprint registry behavior.
   https://raw.githubusercontent.com/Holic75/KingmakerRebalance/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/Main.cs

4. **Call of the Wild project file, pinned revision**
   .NET Framework 4.7, C# 7.3, AnyCPU, local game/UMM/Harmony references, Copy Local behavior.
   https://raw.githubusercontent.com/Holic75/KingmakerRebalance/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/CallOfTheWild.csproj

5. **Call of the Wild license, pinned revision**
   MIT.
   https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/LICENSE

6. **Cowboys and Demons repository, pinned revision**
   Firearm/Gunslinger implementation precedent, code/assets split, known limitations, CC0 repository license.
   https://github.com/Sumotoad987/Cowboys-and-Demons/tree/68f2ee40ef6dc779df0e104392029b7764684fb5

7. **Cowboys and Demons base firearm source**
   Weapon construction, category/visual strategy, reload structure.
   https://github.com/Sumotoad987/Cowboys-and-Demons/blob/68f2ee40ef6dc779df0e104392029b7764684fb5/gun/Firearms/BaseFirearm.cs

8. **Cowboys and Demons armor-piercing source**
   AC rule-event precedent.
   https://github.com/Sumotoad987/Cowboys-and-Demons/blob/68f2ee40ef6dc779df0e104392029b7764684fb5/gun/Firearms/ArmorPiercing.cs

9. **Cowboys and Demons clip source**
   Loaded-state precedent and the wielder-buff limitation being replaced.
   https://github.com/Sumotoad987/Cowboys-and-Demons/blob/68f2ee40ef6dc779df0e104392029b7764684fb5/gun/Firearms/Clip.cs

10. **Cowboys and Demons misfire source**
    Natural-roll/misfire precedent and damaged-firearm adaptation.
    https://github.com/Sumotoad987/Cowboys-and-Demons/blob/68f2ee40ef6dc779df0e104392029b7764684fb5/gun/Firearms/Misfire.cs

11. **Unity Mod Manager Nexus page**
    Version 0.32.5 and update date.
    https://www.nexusmods.com/site/mods/21

12. **Unity Mod Manager repository, pinned revision**
    Loader source and MIT license.
    https://github.com/newman55/unity-mod-manager/tree/6c51f21caa273ffea2747f5bf23c817a0b24bafd

## Blueprint references

13. **Heavy Crossbow — Pathfinder: Kingmaker Wiki**
    Public blueprint names/IDs for `HeavyCrossbow` and `StandardHeavyCrossbow`.
    https://pathfinderkingmaker.fandom.com/wiki/Heavy_Crossbow

14. **Light Crossbow — Pathfinder: Kingmaker Wiki**
    Public blueprint names/IDs for `LightCrossbow` and `StandardLightCrossbow`.
    https://pathfinderkingmaker.fandom.com/wiki/Light_Crossbow

Public wiki data is secondary evidence. It must be confirmed against the installed Kingmaker build.

## Policy and provenance references

15. **Paizo license index**
    https://paizo.com/licenses

16. **Paizo Fan Content Policy and FAQ**
    https://paizo.com/licenses/fancontent
    https://paizo.com/licenses/fancontent/faq

17. **Paizo Community Use Policy**
    https://paizo.com/licenses/communityuse

18. **Paizo policy update history**
    https://paizo.com/blog/new-and-revised-licenses
    https://paizo.com/blog/updates-on-the-community-use-policy-and-fan-content-policy

19. **Owlcat Terms of Use**
    Notes that game use is also subject to the applicable EULA and platform terms.
    https://owlcat.games/tou

The public Owlcat EULA endpoint observed during research was Wrath-specific, so it is not cited as a Kingmaker license. The installed Kingmaker EULA must be reviewed before public distribution.

## Evidence limitations

- A web-visible repository branch can change; pinned commit URLs are used for technical claims.
- Public Kingmaker wiki pages aggregate content across Owlcat games in places; IDs require local verification.
- Cowboys and Demons targets Wrath of the Righteous. Its types, helpers, and asset pipeline are not assumed to exist in Kingmaker.
- Current policy pages can change. Recheck them before each public release.

## Sprint 3 bootstrap references

20. **Harmony 1.2.0.1 Wiki — Home / quick bootstrap example**
    Documents `HarmonyInstance.Create` and `PatchAll(Assembly.GetExecutingAssembly())` for Harmony 1.x.
    https://github.com/pardeike/Harmony/wiki

21. **KingmakerAI `Main.cs`**
    Independent Kingmaker precedent for the zero-argument `LibraryScriptableObject.LoadDictionary` Harmony12 postfix.
    https://github.com/Holic75/KingmakerAi/blob/master/Main.cs

22. **Unity Mod Manager `Log.cs`**
    Public `UnityModManager.ModEntry.ModLogger` surface.
    https://github.com/newman55/unity-mod-manager/blob/master/UnityModManager/Log.cs

23. **Unity Mod Manager `ModManager.cs`**
    Public loader/mod-entry implementation context.
    https://github.com/newman55/unity-mod-manager/blob/master/UnityModManager/ModManager.cs


## Sprint 4 blueprint registration references

24. **Call of the Wild `Helpers.cs`, pinned revision**
    Established Kingmaker pattern for assigning `BlueprintScriptableObject.m_AssetGuid`, collision-checking `BlueprintsByAssetId`, and inserting an asset into both library indexes.
    https://github.com/Holic75/KingmakerRebalance/blob/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild/Helpers.cs

25. **Eldritch Arcana favored-class source**
    Kingmaker precedent for setting `BlueprintFeature.HideInUI`.
    https://github.com/SnowyJune973/EldritichArcana-zhCN/blob/master/FavoredClassBonus.cs

26. **Unity 2019.4 ScriptableObject API**
    Runtime `ScriptableObject.CreateInstance` contract.
    https://docs.unity3d.com/2019.4/Documentation/ScriptReference/ScriptableObject.html

## Sprint 7 proficiency and development-UI references

27. **Cowboys and Demons `FirearmProficiency.cs`, pinned revision**
    Wrath precedent for a dedicated firearm proficiency feature and an `EquipmentRestriction` that checks `UnitDescriptor.GetFeature`.
    https://github.com/Sumotoad987/Cowboys-and-Demons/blob/68f2ee40ef6dc779df0e104392029b7764684fb5/gun/Firearms/FirearmProficiency.cs

28. **Unity Mod Manager — How to create a mod for a Unity game**
    Documents `ModEntry.OnGUI` as the mod-options drawing callback and shows assigning an IMGUI method during load.
    https://github.com/newman55/unity-mod-manager/wiki/How-to-create-a-mod-for-unity-game

29. **Unity Mod Manager `UI.cs`**
    Loader-side implementation that exposes and invokes active mods' `OnGUI` callbacks in the options panel.
    https://github.com/newman55/unity-mod-manager/blob/master/UnityModManager/UI.cs

## Sprint 8 combat-tracing references

30. **Call of the Wild attack-roll usages, pinned repository**
    Kingmaker precedent that `RuleAttackRoll` exposes a weapon relation and that unit distance is queried through `DistanceTo`; exact installed contracts still require reflection verification.
    https://github.com/Holic75/KingmakerRebalance

31. **Call of the Wild weapon-attack usages, pinned repository**
    Kingmaker precedent for `RuleAttackWithWeapon.IsFirstAttack` and `RuleAttackWithWeapon.IsFullAttack` as attack-shape evidence.
    https://github.com/Holic75/KingmakerRebalance

32. **KingmakerAI combat-rule usages**
    Independent Kingmaker precedent for reading `RuleCalculateAC.TargetAC` and engine distance/range values.
    https://github.com/Holic75/KingmakerAi

33. **Harmony patch target annotations**
    Harmony documentation for conditional `Prepare()` and computed `TargetMethod()` patch targeting.
    https://harmony.pardeike.net/articles/annotations.html

These references identify candidate APIs. Sprint 8 deliberately treats them as hypotheses until `inspect-runtime-contracts.ps1` and a running-game trace confirm the target installation.

## Sprint 9 touch-AC references

1. **Cowboys and Demons `ArmorPiercing.cs`**
   Firearm AC event precedent: apply the difference between touch AC and ordinary modified AC rather than replacing the complete attack pipeline.
   https://raw.githubusercontent.com/Sumotoad987/Cowboys-and-Demons/68f2ee40ef6dc779df0e104392029b7764684fb5/gun/Firearms/ArmorPiercing.cs

2. **Call of the Wild combat-rule sources**
   Kingmaker precedents for resolving the attack weapon, reading initiator/target distance, and participating in ordinary rule events.
   https://github.com/Holic75/KingmakerRebalance/tree/1332fb0db844b7863f484ca978bea2349fe49769/CallOfTheWild

3. **KingmakerAI `RuleCalculateAC` usage**
   Independent Kingmaker precedent for reading `RuleCalculateAC.TargetAC`.
   https://github.com/jennyem/pathfinder-mods

These references guide the design but do not substitute for inspecting the installed Kingmaker assemblies. The package contains no mirrored third-party source.

## Sprint 12 persistence references

34. **Craft Magic Items for Kingmaker — source repository**
    Kingmaker precedent for custom item/blueprint recovery, the `Kingmaker.Blueprints.Items.Ecnchantments` namespace, and treating custom item references as save-sensitive content.
    https://github.com/RobRendell/OwlcatKingmakerModCraftMagicItems

35. **Craft Magic Items `Main.cs`**
    Shows the Kingmaker mod's Harmony recovery hooks around blueprint lookup/load and its explicit warning that custom items and feats affect save behavior. This is supporting precedent, not proof that the Sprint 12 dynamic token carrier serializes correctly.
    https://raw.githubusercontent.com/RobRendell/OwlcatKingmakerModCraftMagicItems/master/CraftMagicItems/Main.cs

36. **Kingmaker save-editing format discussion**
    Secondary evidence that Kingmaker saves are ZIP archives containing JSON such as `player.json` and `party.json`. The project does not rely on manual save editing for persistence; actual game save/load remains the authoritative test.
    https://www.reddit.com/r/Pathfinder_Kingmaker/comments/9yutun/savegame_editing_guide/

These references motivated the candidate and test matrix. They do not establish the exact `ItemEntityWeapon.AddEnchantment` contract or save durability on the target installation; `inspect-runtime-contracts.ps1` and the compiled lifecycle matrix must do that.

## Sprint 13 UnitPart-vault references

37. **Call of the Wild `FeatureMechanics.cs`**
    Kingmaker-mod precedent for a custom `UnitPart` subclass and generic unit-part `Get` / `Ensure` access. This supports the API candidate only; it does not prove serialization of the Sprint 13 direct-item-reference graph.
    https://raw.githubusercontent.com/Holic75/KingmakerRebalance/master/CallOfTheWild/NewMechanics/FeatureMechanics.cs

38. **Call of the Wild repository**
    Broader Kingmaker source context for mod-defined unit parts and save-sensitive custom content.
    https://github.com/Holic75/KingmakerRebalance

These references justify the experiment, not the persistence decision. The compiled Kingmaker lifecycle matrix remains authoritative.


## Sprint 14 item-identity references

39. **Public Owlcat-engine mod diagnostic output containing ItemEntityWeapon GUID-like identities**
    Supporting evidence that item entities are exposed with stable-looking GUID values in the engine ecosystem. This is only a reason to inspect the installed Kingmaker contract, not proof of save-lifecycle semantics.
    https://github.com/Truinto/DarkCodex/issues/154

40. **RespecMod entity identity usage**
    Owlcat-engine mod source using `UniqueId` in entity-oriented save/reconstruction workflows. This is Wrath-era supporting precedent and does not prove that Kingmaker `ItemEntityWeapon.UniqueId` survives merchants, duplication, or process restart.
    https://github.com/BarleyFlour/RespecMod

The Sprint 14 source therefore treats `UniqueId` as a provisional installed-contract candidate. Reflection and the complete Kingmaker lifecycle matrix remain authoritative.
