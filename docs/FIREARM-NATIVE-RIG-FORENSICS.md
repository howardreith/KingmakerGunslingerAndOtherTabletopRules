# Firearm native rig forensics

Status: baseline established; exact installed-assembly and live donor inspection
pending. This document records signatures and curated hierarchy facts only. No
proprietary assembly or decompiled source is committed.

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

Pending inspection: full `WeaponVisualParameters`; enum values; hand-slot
attach/model/scale/sheath/destroy/active-set members; hands-equipment lookup and
refresh lifecycle; `EquipmentOffsets` type and IK consumer; DollRoom access;
projectile view and `BeforeLaunch`; crossbow FX snaps; animation release and
projectile-launch callbacks; donor effective values, prefab hierarchy,
transforms/renderers/bounds/materials, IK target, muzzle/FX children, models,
attach slots, belt/sheath/quiver, real-unit owner scale, parent/bone path, and
hand-to-target distances.

## Evidence

The planned guarded scenario is `observe-native-firearm-rig-contracts`. It will
be save-free, non-mutating, fail closed on ambiguity, clean transient objects,
and state that structure does not prove visual quality.

