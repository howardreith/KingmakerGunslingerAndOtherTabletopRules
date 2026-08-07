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

The planned guarded scenario is `observe-native-firearm-rig-contracts`. It will
be save-free, non-mutating, fail closed on ambiguity, clean transient objects,
and state that structure does not prove visual quality.
