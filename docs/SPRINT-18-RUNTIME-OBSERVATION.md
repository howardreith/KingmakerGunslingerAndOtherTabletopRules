# Sprint 18 Kingmaker runtime observation

The first user-run UMM candidate produced the following evidence in Pathfinder: Kingmaker 2.1.7b:

- Unity Mod Manager installed and activated version 0.0.18.
- The mod options panel rendered.
- Blueprint initialization completed once.
- Eight expected custom blueprints registered.
- The trusted preflight recorded I01 as PASS.
- The trusted preflight recorded I02 as FAIL: no inherited `ItemEntityWeapon.UniqueId` member was found.
- Adding a Test Musket created the intended Heavy-Crossbow-derived placeholder item.
- The Firearm Proficiency grant appeared ineffective, leaving the placeholder unable to equip.

Exact assembly inspection then confirmed:

- Selected units are exposed through `SelectionManager.Instance` / `GetSingleSelectedUnit()`.
- Features are granted through `UnitDescriptor.Progression.Features.AddFeature(BlueprintFeature, MechanicsContext)`.
- Item entities do not inherit the `EntityDataBase.UniqueId` contract used by units.
- Runtime item enchantments are held by `ItemEntity.m_Enchantments` and exposed through `Enchantments` / `EnchantmentsCollection`.

Sprint 19 uses those exact contracts and rejects the item-identity vault rather than introducing a guessed fallback.
