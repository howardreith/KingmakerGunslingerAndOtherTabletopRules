# Elven Branched Spear blueprint identities

All identities below are active in `blueprints/blueprints.json`, are registered
under every feature-module profile, and are independent of settings. The weapon
category is the stable enum value `0x004b4d47` (`4934983`); it is not a
blueprint GUID. The request-local duplicate-feature test identity is never
registered or saved and is intentionally absent from this manifest.

| Symbol | GUID | Type |
| --- | --- | --- |
| `KMG.ElvenBranchedSpear.WeaponType` | `77f72b0febaf212a5650e7193c00361f` | `BlueprintWeaponType` |
| `KMG.ElvenBranchedSpear.BaseItem` | `6edc216d68810960f85417237748b042` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.MasterworkItem` | `9c9edabf91f2117fd1b642c4d39b9574` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.ColdIronItem` | `8c0de00a236fe0f532d31711dcaa00a2` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.MasterworkColdIronItem` | `b16c34215cae9d60345042157149a4c0` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.Plus1Item` | `66111becd22690a2a19444a5c6bd0c7b` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.Plus1ColdIronItem` | `25d8f6c6f4767b3168f4700a2890954f` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.ExoticWeaponProficiency` | `017d586ec4546feabf6eaaa67ce74a3f` | `BlueprintFeature` |
| `KMG.ElvenBranchedSpear.FinesseTraining` | `3843c643ffcc617faf9121a5f801a70e` | `BlueprintFeature` |
| `KMG.ElvenBranchedSpear.MovementOpportunityAccuracy` | `b0cabc2a4ac0135fab2f89c689dea389` | `BlueprintWeaponEnchantment` |
| `KMG.ElvenBranchedSpear.Boughkeeper` | `4a084b0226e077b58d79e33184018002` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.Thornstep` | `676faa5f811d851c9f14204bf864e1ec` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.MoonlitFork` | `403d62f6d3bb415c86939430176e55c0` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.VipersReach` | `1cfe40563a9b816931bb35e69677ac27` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.BriarCrownedSpear` | `ee580f43f50a0f0afefaedb3ce7133f3` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.SpearOfTheFirstBranch` | `85c18b96ebee3fdc87eb33da93c8fdf6` | `BlueprintItemWeapon` |
| `KMG.ElvenBranchedSpear.BoughkeeperEnchantment` | `c777f06ec91be851794518fcdcc9c596` | `BlueprintWeaponEnchantment` |
| `KMG.ElvenBranchedSpear.ThornstepEnchantment` | `89a27b8a22715a0b609912bc728dcb31` | `BlueprintWeaponEnchantment` |
| `KMG.ElvenBranchedSpear.VipersReachEnchantment` | `be3a16e947fe8496a8301cbb2476cbcb` | `BlueprintWeaponEnchantment` |
| `KMG.ElvenBranchedSpear.BriarCrownedEnchantment` | `62ef4362d84631574bacc977ffdad3e1` | `BlueprintWeaponEnchantment` |
| `KMG.ElvenBranchedSpear.FirstBranchEnchantment` | `2bba46654f15079769b0e6c741e8f803` | `BlueprintWeaponEnchantment` |
| `KMG.ElvenBranchedSpear.BoughkeeperArmorClassBuff` | `064feb1123cfb1ae4f541ef5e4d138a1` | `BlueprintBuff` |
| `KMG.ElvenBranchedSpear.ThornstepSpeedPenaltyBuff` | `339e83672ea2116e55640d175fec0c84` | `BlueprintBuff` |
| `KMG.ElvenBranchedSpear.ThornstepRoundMarker` | `7e2b2d36433396535555d39cc4066763` | `BlueprintBuff` |
| `KMG.ElvenBranchedSpear.VipersReachReflexPenaltyBuff` | `6ac410ab82b81915d64249a213e1815a` | `BlueprintBuff` |
| `KMG.ElvenBranchedSpear.VipersReachRoundMarker` | `dcc7832d9ed7558111ee97da668522fe` | `BlueprintBuff` |
| `KMG.ElvenBranchedSpear.BriarCrownedRoundMarker` | `89cea1f236074e36051a68ece37aa05c` | `BlueprintBuff` |
| `KMG.ElvenBranchedSpear.FirstBranchRoundMarker` | `1bb02c32918071bfa8333a12de4d7e94` | `BlueprintBuff` |
| `KMG.ElvenBranchedSpear.FirstBranchSpeedPenaltyBuff` | `27d76fe829cc0234b7e120b19462848b` | `BlueprintBuff` |

The manifest therefore contains 29 persistent identities: one weapon type, 12
items, two feature children, six enchantments, and eight buffs. Collision,
registration-count, module-profile, and repeated-initialization tests are part
of the 1,028-test dependency-free suite.
