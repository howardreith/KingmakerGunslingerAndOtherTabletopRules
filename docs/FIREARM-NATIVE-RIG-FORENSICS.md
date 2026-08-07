# Firearm native rig forensics

Status: exact installed type/member metadata captured; live donor inspection
pending. This document records signatures and curated hierarchy facts only. No
proprietary assembly or decompiled source is committed.

## Exact installed assembly findings

Reflection-only inspection of the installed 2.1.7b `Assembly-CSharp.dll`
established:

- `Kingmaker.Blueprints.Items.Weapons.WeaponVisualParameters` has private
  `m_WeaponModel`, `m_WeaponBeltModel`, `m_WeaponSheathModel`,
  `m_WeaponAnimationStyle`, `m_PossibleAttachSlots`, `m_OverrideAttachSlots`,
  `m_Projectiles`, all qualified sound fields, and private auto-property storage
  for `Prototype`. Public getters include `Model`, `BeltModel`, `SheathModel`,
  `AnimStyle`, `AttachSlots`, `Projectiles`, `Prototype`, `HasQuiver`, `IsBow`,
  and `IsTwoHanded`.
- `Kingmaker.View.Equipment.UnitViewHandSlotData` owns `VisibleItem`,
  `VisualModel`, `SheathVisualModel`, main/off-hand transforms, renderer lists,
  equipment/visual slots, and exact methods `AttachModel()` / `AttachModel(bool
  toHand)`, `DestroyModel`, private `DestroySheathModel`, private
  `ReattachSheath`, private `RecreateModel`, `ShowItem`, `Equip`, `Unequip`, and
  `MatchVisuals`. Getters include `OwnerWeaponScale`, `IsActiveSet`, `IsInHand`,
  `HandTransform`, and `Other`.
- `Kingmaker.View.Equipment.UnitViewHandsEquipment` owns exact `Sets`, active
  set, slot array, `QuiverHandSlot`, and doll-room state. Public lifecycle
  includes `GetWeaponModel(bool offHand)`, `GetSelectedWeaponSet`, `UpdateAll`,
  `UpdateBeltPrefabs`, `UpdateVisibility`, `ForceSwitch`, equipment-set/slot
  handlers, and `Dispose`. `IsDollRoom`, `HasQuiverSpawned`, and active weapon
  styles are readable.
- `Kingmaker.View.Equipment.EquipmentOffsets` derives from
  `UnityEngine.MonoBehaviour`. Public fields include exact
  `Transform IkTargetLeftHand`, `IkTargetRightHand`, `JointsParent`, main/off
  hand offsets, slot offsets, race offsets, and backpack policy. The hand slot
  has a private `IkRaceOffsetApply(EquipmentOffsets)` consumer.
- Installed `Kingmaker.View.Animation.WeaponAnimationStyle` values are `None`,
  `SlashingOneHanded`, `Dagger`, `Fencing`, `PiercingOneHanded`, `Shield`,
  `SlashingTwoHanded`, `PiercingTwoHanded`, `Double`, `Bow`, `Crossbow`, `Sling`,
  `Blowgun`, `ThrownStraight`, `ThrownArc`, `Fist`, `TorchOneHanded`,
  `AxeTwoHanded`, `MartialArts`, and `SpecialDevilClaws`. The short-gun allowlist
  is therefore present; `ThrownStraight` remains explicitly prohibited.

The broad first reflection attempt failed with `StackOverflowException` while
resolving the complete assembly type graph. It yielded no evidence. The
accepted strategy uses known exact names and reflection-only metadata resolution.

## Baseline hypotheses to verify

- `WeaponVisualParameters` couples model, belt/sheath, animation, attach slots,
  projectile, prototype, quiver, FX, and sound behavior.
- `UnitViewHandSlotData` and `UnitViewHandsEquipment` own attachment, refresh,
  scale, active-set, and sheath lifecycle.
- Long-gun support IK consumes `EquipmentOffsets.IkTargetLeftHand`.
- Heavy Crossbow is the first long-gun donor; Light Crossbow remains the
  one-handed mechanical ancestry/fallback donor.
- World hands equipment and inventory DollRoom may require distinct refreshes.

## Exact donors

| Donor | Weapon type GUID | Item GUID |
|---|---|---|
| Light Crossbow | `d525e7a6d8d5aa648a976ac41194b8d0` | `511c97c1ea111444aa186b1a58496664` |
| Heavy Crossbow | `36d0551b8a28587438a47fcbbf53c083` | `19a5092244dcf99478dcd73c974828b1` |

## Required exact findings

Pending inspection: projectile view and `BeforeLaunch`; crossbow FX snaps;
animation release and projectile-launch callbacks; donor effective values,
prefab hierarchy,
transforms/renderers/bounds/materials, IK target, muzzle/FX children, models,
attach slots, belt/sheath/quiver, real-unit owner scale, parent/bone path, and
hand-to-target distances.

## Evidence

Guarded save-free scenario `observe-native-firearm-rig-contracts` passed on
published commit `8bdb40b65c24b271f60361b492e559523e595e17`, run
`20260807T0405003890595Z-8623a3dc21f84769bde130d434879a37`, result SHA-256
`6CC73E253B48ADDBB0DE6D59867A7AEF82083914469D8281D978759BDA995BDE`.

Exact native observations:

- Light Crossbow: model `TH_CrossbowLightArmy_Normal`, `Crossbow`, attach slot
  `Shield`, one projectile, no belt model, sheath
  `QR_CrossbowHeavyQuiver_LightArmyNormal`, one renderer, root
  `EquipmentOffsets`, and left target child `!IK_TARGET_LEFT_HAND (15)` at
  `(-0.0250036642,-0.0240078568,0.357007027)`.
- Heavy Crossbow: model `TH_CrossbowHeavy`, `Crossbow`, attach slot `Shield`, one
  projectile, no belt model, sheath `QR_CrossbowHeavyQuiver`, one renderer,
  root `EquipmentOffsets`, and left target child `!IK_TARGET_LEFT_HAND (12)` at
  `(-0.031,-0.051,0.374)`.
- Both donor roots contain `Locator_WeaponCenterFX_00`, surface locators,
  weapon-trail start/end locators, and `Locator_WeaponWarheadFX_00`.
- All five custom equipped prefabs passed runtime structural preparation.
  Musket support is `(0,0,0.468875021)` and muzzle `(0,0,0.8525)`; Blunderbuss
  support/muzzle are `(0,0,0.378125)` / `(0,0,0.6875)`; Rifle support/muzzle
  are `(0,0,0.468875021)` / `(0,0,0.8525)`. Long-gun exact left-hand IK
  assignments passed. Short guns have no support target.

The scenario destroyed both transient donor instances and performed no item,
unit, inventory, blueprint, or save mutation. These are structural facts, not
proof of acceptable grip, scale, clipping, pose, or animation.
