# Feature Module Boundary Inventory

Status: IN PROGRESS

UMM 0.32.4 exact public contract: `ModSettings.GetPath(ModEntry)`, `Load<T>(ModEntry)`, `Save(ModEntry)`, static `Save<T>(T, ModEntry)`, `ModEntry.OnGUI` and `OnSaveGUI`, both `Action<ModEntry>`. The native helper is XML-oriented and its malformed-input/byte-retention semantics do not meet the mission's diagnostic-preservation requirement. The authorized project-owned JSON serializer using the existing Newtonsoft.Json reference will use one explicit schema and atomic replacement; UMM remains the UI host.

| Surface | Layer | Module | Baseline owner / note |
|---|---|---|---|
| All 250 active project blueprint GUIDs | Identity | Infrastructure | Unconditional registration; includes unchanged 248 plus Acadamae and Cord; must remain constant across settings |
| Gunslinger class, progression, archetypes, deeds | Identity + public class catalog | Gunslinger | Register always; gate catalog publication only |
| Firearm proficiency and project feat choices | Identity + feat selections | Gunslinger | Gate basic/Fighter publications |
| Native firearm parameter menus | Native catalog mutation | Gunslinger | Gate exact firearm additions; preserve native/foreign entries |
| Firearms, magic firearms, ammunition, Paper Cartridges, supplies | Identity + acquisition | Gunslinger | Register always; gate vendors/crafting/grants/loot |
| Capital, BTSL, and fixed campaign loot | Acquisition | Gunslinger | Gate exact project entries independently |
| Runtime firearm patches, state tokens, visuals, audio | Existing-save support | Gunslinger infrastructure | Exact-identity scoped and inert for unrelated content when OFF |
| Acadamae Graduate | Identity + general feat selection | Acadamae Graduate | Register always; publish exactly once only when ON |
| Cord of Stubborn Resolve and support facts | Identity + capital acquisition | Acadamae Graduate | Register always; one vendor row only when ON |
| Acadamae/Cord runtime patches | Existing-save support | Acadamae Graduate infrastructure | Installed always, exact-fact/item scoped |
| Development diagnostics | UI infrastructure | Infrastructure | Preserve and compose after module controls |

Unresolved inventory work: enumerate every bootstrap publisher, BTSL table, fixed-loot mutation, crafting/grant surface, and optional DLC publication and bind it to the coordinator.

Next concrete action: trace `BlueprintBootstrap` registration/publication order and every mutation handle.
