# Kingmaker Gunslinger

The current `codex/brown-fur-transmuter-cotw-extension` development candidate
adds Brown-Fur Transmuter as a seventh independent, default-enabled feature
module for Pathfinder: Kingmaker 2.1.7b while retaining Gunslinger, Acadamae
Graduate, Shield Other, Expanded Summoning, Elven Branched Spears, and Eastern
Weapons. Brown-Fur alone requires a compatible Call of the Wild installation;
the combined package and every unrelated module continue loading without CotW.
The first `0.0.81` Brown-Fur candidate failed human review and is superseded.
The installed `0.0.82` candidate repairs native toggle legibility, live
reservoir counters, distinct icons, and pre-command Personal-spell target
acquisition. Its focused mechanics, persistence, optional-mod profiles, and
all 16 boundary states pass on one immutable artifact. Human presentation and
play review accepted that exact artifact on 2026-08-16. Under the revised
runtime policy, the 16-state boundary is the final cross-module seal and no
exhaustive game-launch enumeration is required.

Eastern Weapons adds one stable category each for Wakizashi, Katana, and
Nodachi. Each family has mundane, masterwork, cold iron, and +1 generic forms
plus six named magical weapons spanning late Act I through the late game.
Katana uses exact current grip state: its exotic proficiency permits either
grip, while broad martial proficiency permits two-handed use only. Nodachi is
martial and receives Heavy Blades or Polearms training without becoming reach;
Wakizashi is a native light/finessable weapon. Brace is absent.

Three project-owned equipped models and six original icons are packaged behind
validated native donor fallbacks. Exact merchant and fixed-loot publication
provides complete family progression, and all 12 generic items appear once in
each installed Beneath the Stolen Lands weapon table. The native Brilliant
Energy capstone passed living/undead runtime qualification. Tabletop Deadly is
deferred because Kingmaker 2.1.7b exposes no reliable coup-de-grace save-DC
hook; it is not approximated as ordinary damage.

The retained Elven Branched Spear module adds one stable exotic two-handed
reach family. Its accepted mechanics and save identities remain unchanged:

That family is one stable exotic two-handed reach category: mundane,
masterwork, cold iron, +1, and six named magical weapons all share ordinary
weapon-feat, Elven Weapon Familiarity, Weapon Finesse, Rogue Finesse Training,
and native Agile behavior. Its inherent +2 attack modifier applies only to an
attack of opportunity created at Kingmaker's movement-disengagement boundary.
Brace and pseudo-Brace behavior are intentionally absent. A project-owned
custom spear model is bundled behind a validated, fail-safe native Longspear
fallback.

The first-playtest repair gives the stable custom category a native-readable
name everywhere, removes firearm artwork from spear selector entries, names the
Rogue option **Finesse Training (Elven Branched Spear)**, and gives native
parameterized weapon feats the decorative `EB` category tile. The six generic
weapon tiers are also stocked by both campaign and standalone Beneath the
Stolen Lands weapon merchants. The accepted weapon mechanics, reach, model
fit, and save identities are unchanged.

Expanded Summoning additively extends Summon Monster I-IX and Summon Nature's
Ally I-IX with the approved tabletop rosters and higher-tier same-kind quantity
choices. It never deletes native or third-party blueprint identities; exact
mapped Owlcat semantic duplicates may be suppressed from the visible menu in
favor of one canonical choice. Its 67 summon-safe creature identities remain
registered even when publication is disabled, allowing active summons and old
saves to deserialize and clean up.

The player-facing list presents current-tier singles before `1d3` and
`1d4+1` groups. Exact Owlcat hybrid umbrellas are replaced in the menu by
distinct Redcap, Axiomite, Soul Eater, Bogeyman, Movanic Deva, Frost Giant,
and Thanadaemon choices; their original blueprint objects remain registered.
Dire Bat is intentionally hidden because no acceptable installed bat rig was
found and the Roc proxy failed visual acceptance.

Every visible creature-choice child uses one of 77 project-owned original
128x128 icons; the same creature reuses its cached icon across families and
quantities, while unrelated concepts never share an icon. SNA I exposes only
creature-named choices, and the stable `dire-tiger` identity is displayed as
`Smilodon`. Eagle uses a measured 0.30 view-only scale while remaining Small.

Shield Other is a level-2 Abjuration spell on the Cleric, Paladin, Inquisitor,
Community domain, and Protection domain lists. With an unambiguous installed
Call of the Wild profile it is also published to Oracle, Warpriest, and Psychic
at level 2. The target gains +1 deflection AC and +1 resistance to all saves;
finalized HP damage is conserved and split evenly, with an odd point assigned
to the caster. Close range limits initial targeting; the established link ends
on expiry, dispel, dismissal, dead or missing endpoints, or area separation,
not ordinary post-cast distance.

Acadamae Graduate now grants a per-character **Use Acadamae Graduate** mode,
which defaults off. Leave it off to use a summon spell's native casting time
with no Acadamae save or fatigue risk. Turn it on to accelerate eligible casts
to a Standard action and accept the Fortitude save after each successful cast.
The mode persists until turned off; a command already accelerated while it was
on retains its save obligation.

Fatigue caused by a failed Acadamae save is now ordinary native Fatigued,
independent of the summoning spell and persistent until normal removal such as
rest. The Cord still substitutes that fatigue, and now uses distinct
project-owned cord-and-clasp artwork instead of the donor belt icon.

## Feature modules

Open Unity Mod Manager's Kingmaker Gunslinger panel to find seven checkboxes:
**Gunslinger**, **Acadamae Graduate**, **Shield Other**, **Expanded
Summoning**, **Elven Branched Spears**, **Eastern Weapons**, and **Brown-Fur
Transmuter  requires Call of the Wild**. All default enabled. Older settings
migrate to schema 6 while preserving explicit existing values and enabling
newly absent default-on modules.

The panel shows **Active this process** and **Saved for next restart**. Checkbox changes are saved for the next complete Kingmaker restart; they never rebuild the live blueprint graph while the game is running.

Disabling a module hides its content from new character choices and acquisition.
It does not unregister stable blueprints or strip existing characters, facts,
items, summons, ammunition state, or equipment from a save. All seven modules
publish independently. Brown-Fur is the only CotW-dependent module: absent or
incompatible CotW leaves saved intent intact but prevents effective Brown-Fur
publication while the other six modules continue. Keep the whole mod installed
for any campaign that has used project content.

## Brown-Fur Transmuter

Brown-Fur is a CotW Arcanist archetype with Powerful Change at level 3, Share
Transmutation at level 9, and Transmutation Supremacy at level 20. Powerful
Change spends one reservoir point to improve one qualifying ability bonus from
an Arcanist-slot Transmutation by +2, increasing to +4 at level 20 while
preserving the original bonus descriptor. Share Transmutation independently
spends one point to deliver a genuine Personal Transmutation spell to a willing
creature at Touch, increasing to exactly 30 feet at level 20. Both may modify
the same eligible Arcanist spell for exactly two points. Supremacy gives genuine
Transmutation spells free, non-stacking Extend without changing slot or casting
time.

Powerful Change exposes six distinct native activatable toggles. Exactly one
score can be armed; Kingmaker's selected treatment and the shared Arcane
Reservoir counter remain visible on the action bar. An ineligible or canceled
cast spends nothing and leaves the score armed. Share is a separate activatable
using the same counter; when armed, a supported Personal Transmutation enters
willing-creature target selection before any cast command is created.

Turning Brown-Fur OFF hides it from new character creation and respec after a
restart while compatible CotW remains installed; existing owners retain their
features and effects. Removing CotW from a save containing its Arcanist or a
Brown-Fur character is unsupported because the parent class belongs to CotW.

## Acadamae Graduate

Acadamae Graduate is a general feat for a level-one-or-higher specialist Wizard who is not a Universalist, has not given up school specialization, and does not have Conjuration as an opposition school. A prepared arcane Conjuration (Summoning) spell that would take a Full-Round action instead takes a Standard action. After a successful accelerated cast, the caster makes a Fortitude save at DC 15 + spell level; failure causes Fatigued. Scrolls, wands, spell-like abilities, spontaneous casts, divine casts, non-summoning Conjuration spells, and spells already Standard or faster do not qualify.

Kingmaker represents the installed eligible one-round summons with its Full-Round overlay; it has no separate usable multi-round command representation for these candidates.

## Cord of Stubborn Resolve

The Cord is a belt-slot wondrous item costing 15,000 gp and weighing one pound. While equipped it grants a +2 enhancement bonus to Constitution. Incoming Fatigued becomes 1d6 nonlethal-equivalent damage; incoming Exhausted becomes the same damage plus Fatigued.

Kingmaker 2.1.7b has no usable native nonlethal damage path. The adaptation is untyped self-damage capped so the Cord cannot reduce its wearer below 1 HP. The substitution still occurs at the floor. Exactly one Cord is stocked by the established capital blacksmith through `SmithVendorTable` after the capital is available.

The sections below retain historical subsystem detail; where version-specific
wording conflicts, the 0.0.82 text above and its implementation report are
authoritative.

## Current vertical slice

The build provides the complete base Gunslinger progression, production early
and advanced firearms, stackable Black Powder Charges and Lead Balls, atomic
component consumption, range-limited touch AC, exact item-owned firearm state,
loaded-round enforcement, misfire condition transitions, and same-item
maintenance. Historical Test Musket fixtures remain development-only.

The retained Test Musket diagnostic fixture has one round, a 40-foot range
increment, natural 1–2 misfire, full-round reload requiring a free hand, and a
5-foot misfire burst. It is not distributed as production equipment.

A first misfire consumes the loaded round, forces a miss, and changes only the exact firearm from Normal to Broken. A second misfire from Broken changes the exact firearm to Wrecked and resolves a native Reflex DC 12 plus base weapon-damage burst against every unique qualified unit in five feet, with the exact wielder included once and last.

## Complete maintenance loop

Firearm Proficiency now grants three separate full-round abilities:

```text
Overhaul Firearm: empty/Wrecked + one Repair Kit → empty/Broken
Repair Firearm:   empty/Broken + one Repair Kit → empty/Normal
Reload Firearm:   empty + powder + Lead Ball → loaded
```

Overhaul and Repair are distinct personal extraordinary actions. Each mutates
only during completed delivery, consumes exactly one Firearm Repair Kit,
preserves the same exact runtime item and item-owned state token, and creates no
ammunition. Repair rejects Wrecked, Normal, or loaded Broken firearms without
mutation.

Reload remains a separate full-round operation and is the only maintenance-loop step that consumes Black Powder and a Lead Ball.

## Accelerated qualification harness

Sprint 29 adds a deterministic development fixture and PASS/FAIL matrix. It prepares one exact equipped Test Musket as empty/Wrecked, preserves or creates a second independent empty/Normal Test Musket, ensures two Repair Kits plus one powder-and-ball pair, captures process-local identities and counters, and validates each checkpoint:

```text
FixtureReady → OverhaulPassed → RepairPassed → MaintenanceLoopPassed
```

A one-command immediate diagnostic runs the entire transaction loop without action economy for fast regression checks. The action-bar abilities must still be tested separately for real full-round delivery and interruption behavior.

The item-owned inert `BlueprintWeaponEnchantment` token remains the authoritative state carrier. The rejected `ItemEntityWeapon.UniqueId` vault is not used.

## Installation

Install only the standalone Unity Mod Manager ZIP. Do not install the source archive, complete milestone archive, private reference bundle, compiler package, or framework reference assemblies.

Read `INSTALLATION-COMPATIBILITY.md` before installing, updating, removing, or
using this mod with other gameplay mods. In particular, back up saves before
updates and do not remove the mod from a campaign that has used its content;
there is no uninstall-safe-save claim. `SMOKE-TEST-GUIDE.md` remains the
mechanical diagnostic guide.

## Production equipment and fallback presentation

The production Pistol, Musket, Advanced Rifle, and Advanced Revolver are
available from the qualified capital vendor route alongside powder, lead balls,
and repair kits. Blunderbuss remains unavailable until its numeric scatter-cone
distance is authorized and runtime-qualified.

The core package intentionally uses installed crossbow-compatible fallback
assets under ADR-0007. Pistol/Revolver use Light Crossbow presentation;
Musket/Blunderbuss/Rifle use Heavy Crossbow presentation. Their icons, models,
animations, sounds, equipment attachment behavior, and projectiles therefore
look and sound like crossbows. No custom firearm art, audio, animation, model,
or projectile asset is bundled.

## Direction after Sprint 29

Reload, Overhaul, and Repair use one marker-first exact-equipped-firearm context
and definition-driven policy. Stable historical symbols and compatibility
adapter type names are retained for save and code compatibility; their visible
abilities are production-generic.

## Deliberate deferrals

Custom firearm assets, authorized numeric scatter delivery, crafting, magical
firearms, firearm-using enemies, and dual-wield presentation polish remain
outside the current qualified build.
# Native firearm audio status

Firearm reports now route through Kingmaker's native Wwise integration. The
release package includes one hash-verified `KMG_Firearms.bnk`, generated by
Wwise 2016.2.6.6153 with five embedded firearm media items. The mod stages only
that allowlisted bank into Kingmaker's native Windows SoundBank directory.
Audio failure remains fail-soft for firearm mechanics. The retired Unity
`AudioSource` fallback is not used, and no custom `Init.bnk` is distributed.
