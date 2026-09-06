# Kingmaker Gunslinger

Version `0.0.117-elemental-traits` is the in-progress Release C expansion of the
existing **Elemental Races: Ifrit, Oread, Sylph, and Undine** feature module.
It now registers the fixed replacement-slot framework for 21 required
alternate racial traits: ten explicit slot selections, ten retain-base
markers, 21 visible choice markers, and 21 separate hidden providers. The
pure policy covers all legal combinations and overlap exclusions independent
of fact order, while the owned reconciler preserves existing SLA amounts and
removes only exact project providers. Release C remains in progress; the trait
mechanics and guarded runtime, persistence, and compatibility gates are not
yet complete. The first eight traits now have focused native-mechanics proof:
Wildfire Heart, Brazen Flame, Forge-Hardened, Granite Skin, Like the Wind,
Secretive, Thunderous Resilience, and Whispering Wind. Fire, Earth and Air
Insight, Fire in the Blood, Stone in the Blood and Storm in the Blood now pass
focused native checks plus incremental module-OFF/ON save, level-up, rest,
respec and cleanup checks, including spent healing capacity and active buffs.
Seven other required mechanics, the complete trait persistence/lifecycle matrix
and final qualification remain. This is not a finished Release C package.

Release B remains locally qualified. It registers eleven stable feat identities plus fourteen supporting ability,
buff, and weapon-enchantment identities in every module state. When the module
is enabled, all eleven feats publish to the universal feat selector and the
four Combat feats publish to the Fighter combat-feat selector through exact-
GUID-aware reversible transactions. The catalog includes Elemental Strike,
the Scorching Weapons/Inner Flame/Blazing Aura chain, Firesight, Airy Step,
Wings of Air, Cloud Gazer, Inner Breath, Hydraulic Maneuver, and Triton Portal.
Release B passes locally. All eleven feat mechanics pass dedicated and final
integrated guarded scenarios; 24 race/sex/heritage fixtures pass module-OFF/ON
save persistence and exact cleanup; and all six required installed
compatibility profiles pass in both module states with byte-exact restoration.
The required checkpoint push remains blocked only by the external branch
allowlist. Visual Adjustments was not installed and remains **NOT-RUN**.

Release A added one obligatory heritage selection per race. Each selection has
exactly three choices: General plus two alternate heritages. The parent race
blueprints and all 0.0.114 General
SLA/resource/affinity identities remain unchanged, and the module still
defaults ON. A missing marker on a legacy 0.0.114 character resolves as General
without duplicating its racial modifiers or restoring a spent daily use.

The choices are General Ifrit, Lavasoul, Sunsoul; General Oread, Gemsoul,
Ironsoul; General Sylph, Smokesoul, Stormsoul; and General Undine, Mistsoul,
Rimesoul. Alternate heritages apply exact net racial-stat changes, retain the
parent race's common traits and visuals, and reconcile exactly one active
affinity and racial spell-like ability. Audited native Kingmaker substitutions
are Firebelly for Burning Sands, Flare Burst for Sun Metal, Expeditious Retreat
for Blurred Movement, and Blur for Obscuring Mist. Player-facing names describe
the abilities actually granted. Unerring Weapon and Chill Touch use narrow
project-owned implementations where no complete safe Kingmaker donor exists.

All four races are Medium and have distinct, stable project identities.
Ifrits receive +2 Dexterity, +2 Charisma, -2 Wisdom, fire resistance 5, Fire
Affinity, and Burning Hands once per day. Oreads receive +2 Strength, +2
Wisdom, -2 Charisma, acid resistance 5, native Slow and Steady at 20 feet,
Acid Affinity, and Stone Fist once per day. Sylphs receive +2 Dexterity, +2
Intelligence, -2 Constitution, electricity resistance 5, Air Affinity, and
Feather Step once per day. Undines receive +2 Dexterity, +2 Wisdom, -2
Strength, cold resistance 5, Water Affinity, and Hydraulic Push once per day.
Every race receives Keen Senses for exactly +2 racial Perception. Each
affinity adds +1 DC only to the matching Fire, Acid, Electricity, or Cold
spell, and each racial spell-like ability uses total character level as caster
level.

These are practical Kingmaker adaptations: Keen Senses replaces darkvision;
Feather Step replaces Feather Fall; Undine swimming clauses are descriptive
only because the game has no ordinary player swimming system. Hydraulic Push
uses native Bull Rush resolution with total character level plus the best
Intelligence, Wisdom, or Charisma modifier. The races use `RaceId.Aasimar`
for safe doll and equipment compatibility, so some base-game dialogue or
RaceId-only logic can mistake them for Aasimar. Exact race-blueprint
prerequisites remain distinct.

Release A uses only audited vanilla Kingmaker modular character assets and
project-owned stable proxies and color ramps. It adds no original meshes,
copied third-party assets, persistent elemental VFX, or runtime dependencies.
The identities remain registered when the module is OFF so an existing
elemental character can load while the races are hidden from new-character and
respec selectors. Uninstalling the whole mod from a campaign containing its
content remains unsupported. The 0.0.114 release passed its documented
structural, mechanical, persistence, and compatibility gates; those records
remain historical evidence rather than being relabelled as 0.0.115 proof.
Release A independently passed its full 1,407-case suite, clean package gates,
guarded blueprint/mechanics/SLA runs, 24-fixture persistence, exact 0.0.114
migration, native visual state transitions, and all six required installed
compatibility profiles in both module states. Visual Adjustments was not
installed and remains **NOT-RUN**.

The current branch retains version `0.0.113`'s save-load hotfix for the
paper-cartridge mode repair.
Paper-mode reads now use only the native activatable ability's current state;
they never reconcile marker buffs or alter a unit while Kingmaker is loading a
save. The `set_IsOn` hook is cache-only, and a stale marker is ignored
mechanically. Turning Use Paper Cartridges off still immediately selects loose
ammunition and its normal action economy.

Every project-owned 20-unit ammunition batch costs 10% of retail, rounded up
with a 1 gp minimum: 22 gp for loose powder and balls together, and 24 gp for
Paper Cartridges. Native and scoped CMI routes charge once and restore their
owned state on failure. The optional CMI bridge is targeted and idempotent; it
never cycles CMI's UMM toggle or lifecycle.

The release retains Protection from Evil, Good, Law, and Chaos's Wrath-style
defense against new registered matching-alignment mental-control effects. Its
separate default-enabled UMM setting and the player-facing scope remain
unchanged.

It also gives the Gunslinger a distinct native
swashbuckler/privateer class outfit instead of inheriting Fighter clothing. It
uses only Kingmaker's built-in male/female Magus base and accessory entities,
preserves normal equipment and color-ramp behavior, and adds no asset or mod
dependency.

This release also retains 0.0.105's Brown-Fur ordering, player-facing presentation,
and 30-item fixed-loot distribution; the 0.0.104 same-turn summon correction;
and the unrelated accepted 0.0.103 Overhaul Firearm and Expanded Summoning
repairs. It also retains the owner-accepted
0.0.102 starter, Bokken, combat-log, and Acadamae toggle corrections and the
optional Craft Magic Items 2.1.0 integration without linking or packaging
`CraftMagicItems.dll`. The qualified 0.0.99 inner
ammunition UI seam remains: CMI owns its complete top-level mundane selector on
every IMGUI event, while KMG intercepts only the exact finalized Firearm
Ammunition data object before CMI's equipment-only body. Exact 20-unit
ammunition projects use target 5 and KMG's scoped 10%-of-retail price policy:
20/2/24 gp for powder, ball, and paper. The scope restores CMI's ordinary
settings immediately after the KMG ammunition operation. Official firearm support is exactly Pistol, Musket, and
Blunderbuss. Advanced Rifle and Advanced Revolver are hidden legacy identities:
they remain registered only so old-save or deliberately Toy Box-spawned items
load and can be upgraded, but they have no normal selection, starting grant,
vendor, loot, or crafting path. Eastern and Elven weapons use CMI's existing
Martial/Exotic mundane categories followed by ordinary Arms and Armor upgrades,
and KMG's internal firearm state/origin enchantments no longer leak `<null>`
tooltip blocks. Named campaign weapons remain upgrade-only, feature-module
gates remain authoritative, and an absent, disabled, or incompatible CMI
installation leaves ordinary Gunslinger behavior unchanged. See
`INSTALLATION-COMPATIBILITY.md` for the tested assembly fingerprint,
persistence warning, and required fresh-process human checklist.

The retained `0.0.93` compatibility work preserves Bodyguard, In Harm's Way, and
the canonical Call of the Wild Aid Another integration while repairing the
exact Favored Class 1.3.1 and Tweak or Treat 1.1.0 startup conflict caused by
Nodachi's runtime-only martial weapon category. Broad-martial proficiency is
now published transactionally after every `LoadDictionary` postfix has
finished, allowing Favored Class trait construction and Tweak or Treat's
Heirloom Weapon integration to complete before either sees the custom enum.

With compatible Call of the Wild and Favored Class installations, Combat
Traits contain KMG's **Helpful** trait, Race Traits retain Favored Class's
halfling **Helpful**, and ordinary Aid Another plus Bodyguard share the same
+2/+3/+4 replacement and independent-contributor calculation. When Favored
Class traits and Eastern Weapons are both enabled, Equipment Traits also gain
one save-stable **Heirloom Weapon: Nodachi** choice without replacing native or
third-party options.

Every successful Bodyguard contribution remains explicit in Kingmaker's native
expanded attack-roll AC details, sourced from the protector's actual Bodyguard
feat fact and scoped to the exact `RuleCalculateAC` event. Variable Helpful and
other canonical Aid Another increases retain their truthful final AC values.

The retained subsystem is a ninth independent, default-enabled feature module.
It publishes both feats to the normal general and Fighter combat-feat selections
with separate, free, persistent automation modes that default off. Bodyguard
spends native attack-of-opportunity currency before the incoming result is
known and rolls a native target-aware melee attack calculation against AC 10.
In Harm's Way spends Kingmaker's shared immediate/swift budget and redirects
the original successful attack delivery rather than copying, replaying, or
transferring post-damage results.

The candidate retains the complete `0.0.89` weapon-presentation and `0.0.88`
overnight repair sets. Production firearms, Elven Branched Spears, Wakizashi,
Katana, and Nodachi remain calibrated against native presentation donors
without changing their weapon mechanics, blueprint identity, damage, range,
reload, misfire, or acquisition behavior.

The candidate adds full semantic weapon frames, independent held/stored rigs,
and guarded visual evidence for native controls, firing and thrust motion,
reloads, transitions, locomotion, male/female Medium bodies, a Small body,
Enlarge Person, heavy armor, and cloak interaction. Automated visual evidence
supports this cosmetic qualification; focused human play review remains the
final subjective acceptance surface.

Urban Barbarian remains the eighth independent default-enabled module and is
usable without Call of the Wild. Brown-Fur alone requires a compatible Call of
the Wild installation; every unrelated module continues loading without CotW.

The first `0.0.81` Brown-Fur candidate failed human review and is superseded.
The installed `0.0.82` candidate repairs native toggle legibility, live
reservoir counters, distinct icons, and pre-command Personal-spell target
acquisition. Its focused mechanics, persistence, optional-mod profiles, and
all 16 seven-module boundary states pass on one immutable artifact. Human presentation and
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

Open Unity Mod Manager's Kingmaker Gunslinger panel to find eleven checkboxes:
**Gunslinger**, **Acadamae Graduate**, **Shield Other**, **Expanded
Summoning**, **Elven Branched Spears**, **Eastern Weapons**, **Brown-Fur
Transmuter -- requires Call of the Wild**, **Urban Barbarian**, **Bodyguard
and In Harms Way**, **Protection from Alignment: control immunity**, and
**Elemental Races: Ifrit, Oread, Sylph, and Undine**. All eleven modules
default enabled. Older settings migrate to schema 10 while preserving every
explicit value. Any absent module key, including Elemental Races, migrates ON.

The panel shows **Active this process** and **Saved for next restart**. Checkbox changes are saved for the next complete Kingmaker restart; they never rebuild the live blueprint graph while the game is running.

Disabling a module hides its content from new character choices and acquisition.
It does not unregister stable blueprints or strip existing characters, facts,
items, summons, ammunition state, or equipment from a save. All eleven modules
publish independently. Brown-Fur is the only CotW-dependent module: absent or
incompatible CotW leaves saved intent intact but prevents effective Brown-Fur
publication while the other ten modules continue. Urban Barbarian and
Protection from Alignment remain available regardless of CotW compatibility.
Keep the whole mod installed
for any campaign that has used project content.

Elemental Races is enabled by default. Its single KMG UMM checkbox controls
only publication to new-character and respec selectors after a complete
restart. Turning it OFF and restarting hides those choices but keeps all
elemental identities available to existing saves. Removing the entire mod from
a campaign that has used an elemental race remains unsupported.

## Urban Barbarian

Urban Barbarian is a native Barbarian archetype. It loses medium-armor
proficiency and Fast Movement, removes Lore (Nature) from its class skills, and
adds Knowledge (World) while retaining Athletics, Mobility, Perception, and
Persuasion. It never changes the base Barbarian class-skill array.

Crowd Control grants +1 attack and +1 dodge AC while at least two active hostile
creatures are within five feet edge to edge. Weapon reach does not expand that
adjacency. Kingmaker has no precise crowd-movement or crowd-influence system,
so those tabletop clauses are intentional no-ops; the mod does not substitute
difficult-terrain immunity, movement bonuses, or a global Persuasion bonus.

Controlled Rage uses one nested selector containing only the current tier's
legal allocations: six at +4, ten at +6, and fifteen at +8. Bonuses are morale
bonuses to actual Strength, Dexterity, and Constitution and may be split in +2
increments. Each tier defaults independently to full Strength. Selection costs
nothing, persists until changed, and is locked while Rage is active. Controlled
Rage retains Rage rounds, fatigue, Tireless Rage, spellcasting restriction,
Rage powers, and Rage equivalence, but grants none of ordinary Rage's attack,
damage, temporary-HP, Will, or AC adjustments.

Turning Urban Barbarian OFF hides it from new character creation and respec
after restart. All Urban identities remain registered, so existing owners keep
their progression, allocation state, and mechanics. Call of the Wild is an
optional compatibility profile, not a prerequisite.

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

## Production equipment and presentation

Pistol, Musket, and Blunderbuss are the complete supported firearm set. Their
mundane and +1 items are available through the qualified capital and Beneath
the Stolen Lands merchant routes alongside ammunition and maintenance supplies;
named firearm variants retain their documented fixed-loot paths. Gunslinger
starting grants resolve only to Pistol or Musket.

The package uses distinct project firearm models, audio, projectiles, and
transparent 128 px item icons. Rapid Reload and every firearm-category selector
share the corrected exact-three presentation described in
`docs/FIREARM-FEAT-ICON-MAP.md`. Legacy Rifle/Revolver blueprint and rig data
remain loadable for compatibility but are not published or normally acquired.

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
