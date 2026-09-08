# Feature Module Boundary Inventory

Status: PASS

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

The bootstrap publication plan now binds the class catalog; basic and Fighter feat arrays; all five native parametrized firearm menus; the module-aware capital table; all four optional BTSL table identities; and five exact fixed-loot targets. Gunsmithing crafting abilities and initial firearm/ammunition grants are owned by the always-registered Gunsmithing/class facts, so withholding the class and feat acquisition surfaces prevents new acquisition while preserving existing-save behavior. Runtime mechanics, assets, and audio remain exact-identity infrastructure.

Runtime qualification proved every listed boundary in all four standalone combinations and exact Call of the Wild ON/ON and Gunslinger-OFF/Acadamae-ON configurations. The registered identity count remained 250, second reconciliation was a no-op, and native/foreign fixture entries were preserved.

Next concrete action: retain this inventory as the guard list for final 0.0.75 validation and future player-facing additions.

## Contextual Teleportation module checkpoint

The standalone teleportation feature adds default-ON module 12 / schema 11.
Its three spell identities always register, while native spell-list publication
and all contextual/arrival hooks require the module to be enabled. Desktop and
gamepad destination hooks are 4 each; ordinary-arrival/load hooks are 2 and the
saved-arrival exploration hook is 1. OFF installs zero of these hooks and
preserves already serialized familiarity and arrival-boundary fields.

The preceding desktop source ba32ac2 passed all 26 module boundary states
(845 assertions). The gamepad addition passed all ON, all OFF, only Teleportation
ON and only Teleportation OFF (134 assertions), including native publication,
exact rollback and actual hook counts. Settings bytes were restored exactly.
These are feature checkpoints; complete release qualification remains open.
Exact run IDs and limits are in the [teleportation report](../TELEPORTATION-SPELLS-IMPLEMENTATION-REPORT.md).


The exploration-guard source checkpoint passes all 26 current boundaries with
897 assertions. Hooks are counted from the actual Harmony registry,
including absent unpatched targets OFF. All publication, duplicate/rollback and
settings restoration checks pass. The report records every run ID and the exact
immutable deployed artifact; other mod profiles and disk persistence remain open.
