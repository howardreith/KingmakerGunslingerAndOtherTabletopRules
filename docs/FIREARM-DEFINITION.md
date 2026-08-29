# Firearm definition

`FirearmDefinition` is the immutable policy record carried by the exact firearm blueprint component. It currently defines:

- era and firearm kind;
- capacity;
- range increment in feet;
- highest natural d20 result that misfires;
- misfire-burst radius in feet;
- reload action, free-hand requirement, and rounds per action; and
- scatter classification.

## Misfire burst radius

Version 0.0.26 adds `MisfireBurstRadiusFeet` as an explicit definition field rather than a runtime constant. Valid values are 5 through 100 feet in five-foot steps. The Early Musket/Test Musket declares 5 feet.

The value round-trips through `FirearmDefinitionComponent`. Runtime misfire context captures it from the exact discharged item's component before any burst is scheduled. Invalid or missing values fail closed.

The radius is passed to Kingmaker's native `Feet` value type and native target query. It is not converted by hand in gameplay code.

## Identity boundary

A Heavy Crossbow display name, category, or animation does not make an item a firearm. The exact custom `FirearmDefinitionComponent` remains required. Native Heavy Crossbows therefore cannot enter reload, loaded-round, misfire, condition, or explosion logic.

The official catalog is exactly Pistol, Musket, and Blunderbuss. Stable
Advanced Rifle and Advanced Revolver definitions remain recognized only so an
old save or deliberate Toy Box spawn cannot break these mechanics; recognition
does not publish or ordinarily provide either legacy item.

## State boundary

The definition is immutable blueprint policy. Loaded rounds and condition are per-item state stored only through item-owned inert `BlueprintWeaponEnchantment` tokens. The definition never stores mutable item state, and `ItemEntityWeapon.UniqueId` is not used as a persistence vault.

## Armor penetration presentation

Every exact firearm item now states its base penetration contract in both its description and Qualities presentation. Base distances are Pistol 20 ft., Musket 40 ft., ordinary direct-fire Blunderbuss 10 ft., Advanced Rifle 400 ft. across five 80-foot increments, and Advanced Revolver 100 ft. across five 20-foot increments. Legal per-attack range-increment modifiers adjust the live boundary. Scatter Shot does not enter the direct-fire Touch AC adapter.
