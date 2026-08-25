# Kingmaker Gunslinger 0.0.100

This release refines the optional Craft Magic Items 2.1.0 integration after
the 0.0.99 ammunition UI repair passed its first human interaction test. The
complete package is
`KingmakerGunslinger-0.0.100-craft-magic-items-post-human-refinement.zip`.

Every exact 20-unit Black Powder Charge, Lead Ball, and Paper Cartridge timed
project now uses progress target 5. CMI's existing price calculation remains
authoritative, so scale-1.0 prices remain 34, 4, and 40 gp respectively.
Exact legacy 0.0.99 ammunition projects with targets 50 or 60 are normalized
idempotently without changing progress, `GoldSpent`, result identity, project
order, cancellation refunds, or completion behavior.

CMI's from-scratch Firearms lists now contain exactly Pistol, Musket, and
Blunderbuss. Advanced Rifle and Advanced Revolver remain registered,
loadable, mechanically supported, Reliable-compatible, indexed for pricing,
and eligible for existing-item upgrades, but this bridge no longer publishes
them as ordinary campaign acquisition bases.

The redundant **Eastern and Elven Weapons** magic category is removed.
Nodachi remains a CMI Martial Weapons base; Wakizashi, Katana, and Elven
Branched Spear remain Exotic Weapons bases. Players craft those mundane bases
first, then enchant the owned item through CMI's ordinary Arms and Armor
workflow. Authored and named versions remain owned-item upgrade targets only.

The native item tooltip now omits only enchantments carrying KMG's exact
`FirearmStateTokenComponent` or `BatteredFirearmOriginComponent` internal
markers. Those enchantments and their save-state mechanics remain present;
Anarchic, Enhancement +5, Reliable, other real qualities, and the dedicated
firearm-condition presentation remain visible.

The qualified 0.0.99 inner ammunition UI interception is preserved. CMI owns
the outer selector, subtype selection, ordinary body, and common money footer;
KMG intercepts only the finalized exact ammunition data object before CMI's
equipment-only body. `CraftMagicItems.dll`, CMI source, data, localization,
and icons are not included.

The exact runtime authority remains Craft Magic Items 2.1.0, SHA-256
`4AE2DA61470350B31BEEF162717A604C9CCD322F66193917944EA4A9596E392D`,
MVID `0044a45b-3bca-439e-86c5-a6aa4d42855e`.

The qualified firearm SoundBank remains byte-identical at SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

CMI-generated custom items use CMI persistence and may require both mods to
remain installed. Automated runtime qualification is distinct from final
human visual and interaction acceptance; the fresh-process 0.0.100 checklist
remains required before release acceptance is claimed.
