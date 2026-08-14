# Eastern Weapons Proficiency Matrix

Status: source and live publication qualified; request-local attack-roll combat qualification remains pending.

| Family | Ordinary authority | Exact category fact | Active grip | Expected nonproficiency penalty |
| --- | --- | --- | --- | --- |
| Wakizashi | Exotic only | Wakizashi | Either hand | `-4` without the exact fact; `0` with it |
| Katana | Martial or exotic | Katana | One-handed | `-4` without the exact exotic fact; `0` with it |
| Katana | Martial or exotic | Katana | Two-handed | `0` with a broad martial grant or the exact exotic fact; otherwise `-4` |
| Nodachi | Martial | Nodachi | Two-handed | `0` with a broad martial or exact category grant; otherwise `-4` |

The static Exotic Weapon Proficiency children are titled exactly:

- `Weapon Proficiency (Elven Branched Spear)`
- `Weapon Proficiency (Katana)`
- `Weapon Proficiency (Wakizashi)`

The new children clone the native Elven Curve Blade child's ordinary
`AddProficiencies`, `PrerequisiteNotProficient`, and `AddStartingEquipment`
shape. They are added only to the merged `AllFeatures` catalog. The serialized
`Features` catalog remains unchanged, matching the accepted spear repair and
Call of the Wild's merged-catalog lifecycle.

Katana uses `ItemEntityWeapon.HoldInTwoHands` as its single native grip
authority. The same authority will be reused by Moonlit Crossing. No animation,
model transform, class, race, or timing heuristic participates in proficiency.

Nodachi is appended to every loaded broad martial weapon-category grant that
contains the unique largest native Martial Weapon Proficiency category set.
The mutation clones the affected `AddProficiencies` component, is idempotent,
and is rollback-owned. Partial enumerated grants are not broadened.

Fighter training is generalized narrowly: Wakizashi uses Light Blades, Katana
uses Heavy Blades, and Nodachi uses the greater native rank from Heavy Blades
or Polearms. Taking the maximum prevents the same underlying training from
being manufactured twice. Fighter-group membership does not alter reach,
animation, or handedness.

The shared selector runtime performs one merged category publication and one
deterministic sort across Elven Branched Spear and all three Eastern families.
It supplies WK, KA, and NO category glyph definitions to the seven approved
generic selectors. Wakizashi alone receives the static
`Finesse Training (Wakizashi)` child.

Live guarded module runs:

- All enabled: `20260814T1230559883866Z-3026975996b14809aa03ee8bfe11558a` — PASS.
- Eastern Weapons disabled: `20260814T1233282614632Z-a9739de4c73c449e89a7a193f2f9b2f0` — PASS.
- Settings restored exactly: SHA-256 `2e53fa0a09c56662434f6ea548ff5ebcf91f5aaf293d668248221239a1308655`.
- Candidate DLL: SHA-256 `4F8C951143D6466DBDF2561CB38F815DD57AA4DCD2F57E2336125CE9B83D390B`; MVID `198d704e-5c73-487b-ae7c-5c5110f58951`.

The complete `disposable-eastern-weapons-combat` scenario will provide the
remaining live attack-roll, grip-switch, respec-style fact reconstruction,
fighter-training, and finesse positive/negative controls.
