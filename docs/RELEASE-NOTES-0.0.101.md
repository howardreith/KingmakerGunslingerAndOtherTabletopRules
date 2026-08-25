# Kingmaker Gunslinger 0.0.101

This release promotes the repository-owner-accepted Craft Magic Items 2.1.0
integration. It contains the complete 0.0.100 post-human-test refinement under
the final package name
`KingmakerGunslinger-0.0.101-craft-magic-items-compatibility.zip`.

Every exact 20-unit Black Powder Charge, Lead Ball, and Paper Cartridge timed
project uses progress target 5. CMI's price calculation remains authoritative,
so scale-1.0 prices remain 34, 4, and 40 gp. Exact legacy ammunition projects
with targets 50 or 60 migrate idempotently without changing progress,
`GoldSpent`, result identity, project order, cancellation refunds, or normal
completion.

CMI's from-scratch Firearms lists contain exactly Pistol, Musket, and
Blunderbuss. Advanced Rifle and Advanced Revolver remain registered, loadable,
mechanically supported, Reliable-compatible, indexed for pricing, and eligible
for owned-item upgrades, but are not published as ordinary campaign creation
bases.

Nodachi remains a CMI Martial Weapons base; Wakizashi, Katana, and Elven
Branched Spear remain Exotic Weapons bases. Players craft those mundane bases
first, then enchant the owned item through CMI's ordinary Arms and Armor
workflow. The redundant standalone Eastern and Elven Weapons magic category is
absent, and named campaign weapons remain upgrade-only.

Native weapon tooltips omit only enchantments carrying KMG's exact
`FirearmStateTokenComponent` or `BatteredFirearmOriginComponent` internal
markers. Those enchantments and their save-state mechanics remain present;
Anarchic, Enhancement +5, Reliable, other real qualities, and KMG's dedicated
firearm-condition presentation remain visible.

The accepted inner ammunition UI interception is unchanged. CMI owns the outer
selector, subtype selection, ordinary body, and common money footer. KMG
intercepts only the finalized exact ammunition data object before CMI's
equipment-only body. A GUI fault never rolls back the graph synchronously.

The exact external authority remains Craft Magic Items 2.1.0, SHA-256
`4AE2DA61470350B31BEEF162717A604C9CCD322F66193917944EA4A9596E392D`,
MVID `0044a45b-3bca-439e-86c5-a6aa4d42855e`.
`CraftMagicItems.dll`, CMI source, data, localization, and icons are not
included. CMI-generated custom items use CMI persistence and may require both
mods to remain installed.

The qualified firearm SoundBank remains byte-identical at SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

The repository owner explicitly accepted the installed 0.0.100 candidate on
2026-08-25 and authorized finalization, merge, publication, and an incremented
release. Version 0.0.101 advances release metadata and repeats the full guarded
source, build, package, and real-CMI qualification against the promoted build.
