# Firearm feat icon map

## Before

Every custom firearm row under Weapon Focus and its native dependent selectors
resolved to `weapon-focus-firearm.png`, a generic target with SHA-256
`EDC404430317C2E1A35D51F675E631FACA49D0D9E440BCF9ADBA606E696A9EE2`.
Rapid Reload and all five of its choices resolved to the same dark square/gold
arrow asset with SHA-256
`552455F3CE043B8D93E3DCE91B73AB78EE1852FDF51BDA99611CDD298D12560E`.

## After

| Firearm/purpose | Sprite key | Monogram/symbol | SHA-256 |
|---|---|---|---|
| Pistol | `firearm-monogram-pistol` | P | `28DE0CC981775AC4E4F66CAE03FE72BFECE6E6B2B3262877E744E772F4C64200` |
| Musket | `firearm-monogram-musket` | M | `8A3E2BADFEE6B89244950DF76E42CEB990C19586D61316F91232D88A35E83447` |
| Blunderbuss | `firearm-monogram-blunderbuss` | B | `0CE9468E625164D4D59985D00E922813C54A03AA80988BA2C1EFA087ADABF2F0` |
| Rifle | `firearm-monogram-rifle` | Ri | `ECFF78EAC6D8C60F1BE2EEBA1F0134E067BBB3BD63621A0008934A292403A7D2` |
| Revolver | `firearm-monogram-revolver` | Rv | `4C2665A67C5E8AC9704798808576ED5FE18DB77F4798CA28F9CBFB702B846159` |
| Rapid Reload top-level feat | `rapid-reload` | reload arrow and ramrod | `E3056F665D5A8FA9010A87372C63A34D42855E74DAB8BCB575A7CF8866AE6CE2` |

The five monograms are assigned to Rapid Reload choices and to the firearm
parameters shared by native Weapon Focus, Greater Weapon Focus, Weapon
Specialization, Greater Weapon Specialization, and Improved Critical. Native
top-level feat icons and every non-firearm choice remain native and unchanged.

The deterministic 64/32-pixel inspection map is
`assets-source/original-icons/firearm-feats/firearm-feat-icon-map.png`, SHA-256
`331C85C81FB135857BAB59599BD697B69CB20D919383F61BB0782643956B6467`.
This map is supporting evidence, not final in-game aesthetic acceptance.
