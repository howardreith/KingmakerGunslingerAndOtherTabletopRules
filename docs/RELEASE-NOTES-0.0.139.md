# Kingmaker Gunslinger 0.0.139

Release: `0.0.139-expanded-summoning-phase1`
Package: `KingmakerGunslinger-0.0.139-expanded-summoning-phase1.zip`
Build label: Kingmaker Gunslinger 0.0.139.
Publication status: owner authorized. Published under explicit owner
authorization on 2026-09-26, after an engineering review of pull request 23.

The qualified firearm SoundBank is unchanged, SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

## What changed

Expanded Summoning Phase 1 brings the charter's Sprints 3 through 8 to the
summoning lists: twenty-five creature work items, new and repaired signature
mechanics, and the visual work that goes with them. Published under explicit
owner authorization on 2026-09-26, after an engineering review of pull
request 23 and the guarded runtime qualification recorded below.

## What is new

**Native publication, pack I (Sprint 3).** Pony, Horse, Owlbear, Cyclops and
the Nature's Ally Frost Giant join the lists. Pony and Horse hooves are
secondary attacks, as the stat blocks have them. The Cyclops wears a +4 hide
armor fact rather than loot, and its Flash of Insight chooses the result of
one die roll before it is rolled: the next attack's own d20 becomes a 20, the
critical confirmation is rolled normally, and the arming lasts until that
attack spends it.

**Plants and the worm (Sprint 4).** Shambling Mound, Giant Flytrap and Purple
Worm. Grab is taken by limb identity against a foe of the holder's size or
smaller. The Flytrap keeps one grab link per bite, four at most, and a mouth
that holds or has engulfed a foe attacks no one else. Its Engulf takes a
Medium or smaller foe held since the round began and deals 1d8+7 crushing
plus 2d6 acid each round. The Purple Worm swallows on a later turn's
successful maintain check against a foe one size smaller, never the turn it
takes hold.

**The mephit family (Sprint 5).** Dust, Ice, Magma, Ooze, Salt and Steam
mephits, each with its own name, icon and rim glow, and with enemy-only
breath. Project Wind Wall, Chill Metal, Pyrotechnics and Magma Form; the
stinking cloud spares allies and glitterdust affects enemies only.

**Signature mechanics repaired (Sprint 6).** Monitor Lizard, Grizzly Bear and
Dire Bear grab on the shared lifecycle. The Giant Spider gains blindsight for
tremorsense, web immunity, and a bounded ranged Web resolved as a ranged
touch attack with no saving throw and the native break-free. The Pixie is
verified unchanged.

**The big cats (Sprints 7 and 8).** Leopard, Lion and Dire Lion grab with the
bite; the Smilodon grabs with the bite and both foreclaws. Every cat rakes
only on a charge or against the foe it has held since its round began, and
rake slots are dropped from any other full attack. The Tiger is new to
Summon Nature's Ally IV with a procedural striped coat; the Cheetah gains a
procedural spotted coat and a once-per-summoning sprint; the Lion wears a
tawny tint.

Every project-owned visual is a private material clone or a texture generated
from the rig's own geometry. Each is owned per view and destroyed with the
view, on a failed attach, and on a module-wide sweep. The game's own textures
are never read, copied or redistributed.

## Known limitation, accepted by the owner

**OwnerAcceptedEngineLimitation: ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD.**

An active grab, hold, swallow or engulf, the mouth occupancy that goes with
it and the held-target rake are session-scoped. Kingmaker carries no active
grapple across a save, and it writes this kind of unit part by type without
its contents, so a reload has neither the hold nor the record of which limb
took it.

What a reload does instead is come back clean: no lingering condition, no
occupied mouth, no delayed damage, no dangling link, no unusable unit and no
deserialization fault when the module is disabled. That clean release is
proven by the persistence scenarios, and it is what the owner accepted on
2026-09-26 in place of re-establishing holds on load, which is not
implemented here.

In play this means a creature you hold when you save is simply free when you
load. Nothing else about the summon changes.

## Compatibility and uninstall

The release changes no ordinary vendor publication, no firearm behaviour and
no existing save format. Disabling or removing the module through Unity Mod
Manager leaves the game's own creatures and lists as they were; summoned KMG
creatures do not survive the module's absence, and a save made with the
module disabled loads without a deserialization fault. To uninstall, disable
Kingmaker Gunslinger in Unity Mod Manager and delete its folder from the
game's `Mods` directory.

## Verification status

| Gate | Result |
| --- | --- |
| Version-aware repository validation | PASS |
| Complete domain suite | PASS, 1,806 of 1,806 |
| Clean Release build and build-output validation | PASS, no warnings |
| Strict standalone UMM package validation | PASS |
| Guarded runtime matrix on candidate `145810a5` | PASS, twelve scenarios and two compatibility transactions |
| Live installation restored and verified after every guarded batch | PASS |
| Owner visual and gameplay spot-check of the new creatures | NOT RUN, to be done in play |

Historical domain checkpoints of 1,288 and 1,325 cases remain archived under
their original releases. The live suite for this release has 1,806 cases.

The mechanical gate is the domain suite and the guarded runtime evidence, not
these notes. Candidate commit `145810a5` carried twelve guarded scenarios and
two compatibility transactions, every one PASS, with the live installation
restored and verified after each batch. The closeout head ran the domain
suite at 1,806 tests with no failures, repository and manifest and static
validation, the exact-reference Release build, and strict package validation.

The owner's own visual and gameplay spot-check of the new creatures is still
to come; it is not a blocker for this publication.

## Compatibility

Optional-mod compatibility is unchanged and was not re-tested. Craft Magic
Items compatibility is unchanged: there is no static `CraftMagicItems.dll`
dependency, and its compatibility profile keeps its existing NOT-TESTED
disposition. Better Vendors support is unchanged and still limited to the
exact 2.0.8 binary. Call of the Wild was exercised in both compatibility
transactions of the guarded matrix and passed.
