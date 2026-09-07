# Kingmaker Gunslinger 0.0.116

Roadwarden and Dead Reckoning fill the +3 firearm tier in the Skeletal
Salesman's weapon stock.

| Weapon | Enchantments | Base value |
|---|---|---:|
| Roadwarden | +3 Reliable musket | 33,800 gp |
| Dead Reckoning | +3 Seeking pistol | 33,300 gp |

Reliable reduces Roadwarden's misfire range; it does not guarantee a hit or
eliminate every misfire. Seeking lets Dead Reckoning ignore concealment miss
chances; it does not reveal creatures or grant permission to target them.
Dead Reckoning does not also receive Reliable. Both weapons retain their
family's ordinary ammunition, reload, damage dice, range, and critical rules.

Both appear once in each applicable native C3/C4 stock variant: C3 during
kingdom days 491-690 and C4 from day 691. They follow the shop's normal weapon
ordering and respect player-selected sorts. The observed native buying prices
were 33,800 gp and 33,300 gp at a buying modifier of 1; normal merchant price
multipliers remain in effect.

Already-generated stock receives new entries through native reconciliation
when loading the campaign. Reopening an already-open shop is not a refresh.
Purchases and buyback items are preserved. Very old stock lacking native
KnownItems tracking must wait for normal fresh stock generation; this release
does not regenerate or repair that inventory. The existing five named
fixed-loot rewards are unchanged. These additions belong to the Gunslinger
feature and retain the named-item upgrade-only policy with Craft Magic Items.

All 15 Protection from Alignment spell, communal, selector, and buff
descriptions now explain the existing rules directly: the aligned +2
deflection Armor Class and +2 resistance saving-throw bonuses, prevention of
new qualifying charm, domination, and similar control by a creature of the
specified alignment, and no removal or suppression of control already active.
Control immunity, its setting, spell identities, and other mechanics are
unchanged. Disabled immunity retains the original descriptions.

The accepted content candidate passed repository validation, all 1,398 domain
tests, a clean Release build, strict package validation, and the 24-state
module boundary matrix. Guarded Steam runs verified native stock, displayed
prices, normal shop ordering and purchases, actual +3 attack/damage,
Reliable/Seeking isolation, reload, existing-stock reconciliation, and
purchased-item save/load followed by cleanup. Exact candidate fingerprints,
evidence, availability details, and final release verification are recorded in
[the DATA report](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/master/reports/DATA-MIDGAME-FIREARMS-AND-PROTECTION.md).
Earlier release evidence remains tied to its original artifact.

The owner explicitly authorized finalization, merge to master, origin push,
and this public release. Unmerged concurrent elemental-race work is excluded.

Installable asset: KingmakerGunslinger-0.0.116-midgame-firearms-and-protection.zip.
The guarded publisher records the exact release commit, deterministic build
results, and SHA-256 alongside the asset. This package contains no proprietary
game or Unity reference assemblies.
