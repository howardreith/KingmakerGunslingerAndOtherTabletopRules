# Rare Firearms Item Matrix

| Symbol | GUID | Type / display | Family | Static properties | Eq. | Cost | Weight | Acquisition | Status |
|---|---|---|---|---|---:|---:|---:|---|---|
| KMG.Firearms.ReliableEnchantment | ea10817126e14703878d00e84329244e | BlueprintWeaponEnchantment / Reliable | exact-item generic | reduction 1 | +1 | n/a | n/a | item-only | Forensics |
| KMG.Firearms.PistolPlus1Item | d0145d0410a34df08d68a67367c1dfc9 | BlueprintItemWeapon / Pistol +1 | canonical Pistol | Enhancement +1 | +1 | 3,300 | 4 lb | capital + BTSL | Planned |
| KMG.Firearms.MusketPlus1Item | 3402fe01de1648b187c192500e370f01 | BlueprintItemWeapon / Musket +1 | canonical Musket | Enhancement +1 | +1 | 3,800 | 9 lb | capital + BTSL | Planned |
| KMG.Firearms.BlunderbussPlus1Item | 1dc7efe0792040f187a18adfdc54c6e0 | BlueprintItemWeapon / Blunderbuss +1 | canonical Blunderbuss | Enhancement +1 | +1 | 4,300 | 8 lb | capital + BTSL | Planned |
| KMG.Firearms.DuelistsRebuttalItem | bae89c3abc3240578a6bff69044d2c1b | BlueprintItemWeapon / Duelist's Rebuttal | canonical Pistol | Enhancement +2; Reliable | +3 | 19,300 | 4 lb | fixed Act 3/4 | Planned |
| KMG.Firearms.RiverKingsMeasureItem | a27c86b0d87c423d9ba8a05227bbf1e6 | BlueprintItemWeapon / The River King's Measure | canonical Musket | Enhancement +4; Reliable | +5 | 51,800 | 9 lb | fixed Pitax | Planned |
| KMG.Firearms.IrovettisOvationItem | caf23b7555cd4524a7622eaa25266ea1 | BlueprintItemWeapon / Irovetti's Ovation | canonical Blunderbuss | Enhancement +4; Reliable; Thundering* | +6* | 74,300* | 8 lb | distinct fixed Pitax | Planned; fallback 52,300/+5 |
| KMG.Firearms.TheLastWordItem | 0d31f794ba294c1e834af44f918f6721 | BlueprintItemWeapon / The Last Word | canonical Pistol | Enhancement +5; Reliable; Seeking | +7 | 99,300 | 4 lb | final main route | Blocked: required native Seeking absent |
| KMG.Firearms.WatchAtTheWorldsEndItem | 87c7baaaad504b7f8742f2dfcd79d067 | BlueprintItemWeapon / Watch at the World's End | canonical Musket | Enhancement +5; Reliable; Fey Bane | +7 | 99,800 | 9 lb | separate final source | Planned |

All rows reuse exact family presentation and proficiency. Source, runtime,
persistence, compatibility, and disposition remain unqualified until evidence
is appended. `*` marks the sole authorized fallback.
